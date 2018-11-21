using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public static class VertexProcessorUtility
	{
		public static RenderTextureFormat float4RenderTextureFormat
		{
			get
			{
				if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
				{
					return RenderTextureFormat.ARGBFloat;
				}
				else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
				{
					return RenderTextureFormat.ARGBHalf;
				}
				throw new System.Exception("No usable RenderTextureFormat supported.");
			}
		}

		public static TextureFormat float4TextureFormat
		{
			get
			{
				if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat))
				{
					return TextureFormat.RGBAFloat;
				}
				else if (SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf))
				{
					return TextureFormat.RGBAHalf;
				}
				throw new System.Exception("No usable TextureFormat supported.");
			}
		}

		public static void CalculateVertexTextureSize(int count, out int width, out int height)
		{
			float sqrt = Mathf.Sqrt(count);
			width = Mathf.CeilToInt(sqrt);
			height = width;

			while (true)
			{
				int newHeight = height-1;
				if (width*newHeight > count)
				{
					height = newHeight;
				}
				else
				{
					break;
				}
			}
		}

        // CopyTexture is a little faster than a blit so use it if its available.
        // SystemInfo texture support might be broken? So always defaulting to using Blit on all platforms instead of CopyTexture, for now
        private static bool textureToRTTextureCopySupported = false && basicTextureCopySupported && (SystemInfo.copyTextureSupport & CopyTextureSupport.TextureToRT) != 0;
		public static void CopyTexture(Texture2D source, RenderTexture dest)
		{
            if (textureToRTTextureCopySupported)
            {
                Graphics.CopyTexture(source, 0, 0, dest, 0, 0);
            }
            else
            {
				Graphics.Blit(source, dest);
			}
		}

        // SystemInfo texture support might be broken? So always defaulting to using Blit on all platforms instead of CopyTexture, for now
        private static bool basicTextureCopySupported = false && (SystemInfo.copyTextureSupport & CopyTextureSupport.Basic) != 0;
		public static void CopyTexture(RenderTexture source, RenderTexture dest)
		{
            if (basicTextureCopySupported)
            {
                Graphics.CopyTexture(source, 0, 0, dest, 0, 0);
            }
            else
            {
				Graphics.Blit(source, dest);
			}
		}

		public static UnpackedMesh CreateOptimizedMesh(Mesh sourceMesh, int submeshIndex, List<int> optimizedVertexMap)
		{
			Vector3[] sourceVertices = sourceMesh.vertices;
			Vector3[] sourceNormals = sourceMesh.normals;
			Vector4[] sourceTangents = sourceMesh.tangents;
			Vector2[] sourceUVs = sourceMesh.uv;
			BoneWeight[] sourceBoneWeights = sourceMesh.boneWeights;
            Matrix4x4[] sourceBindposes = sourceMesh.bindposes;
			int[] sourceTriangles = sourceMesh.GetTriangles(submeshIndex);

			int[] vertexMap = new int[sourceVertices.Length];
			for (int i = 0; i < vertexMap.Length; i++)
			{
				vertexMap[i] = -1;
			}

			int[] triangles = new int[sourceTriangles.Length];
			for (int i = 0; i < sourceTriangles.Length; i++)
			{
				int vertexIndex = sourceTriangles[i];
				int remappedIndex = vertexMap[vertexIndex];
				if (remappedIndex == -1)
				{
					remappedIndex = optimizedVertexMap.Count;
					vertexMap[vertexIndex] = remappedIndex;
					optimizedVertexMap.Add(vertexIndex);
				}
				triangles[i] = remappedIndex;
			}

			UnpackedMesh unpackedMesh = new UnpackedMesh();
			unpackedMesh.vertices = new Vector3[optimizedVertexMap.Count];

			if (sourceNormals.Length > 0)
			{
				unpackedMesh.normals = new Vector3[optimizedVertexMap.Count];
			}
			if (sourceTangents.Length > 0)
			{
				unpackedMesh.tangents = new Vector4[optimizedVertexMap.Count];
			}
			if (sourceUVs.Length > 0)
			{
				unpackedMesh.uv0s = new Vector2[optimizedVertexMap.Count];
			}
			if (sourceBoneWeights.Length > 0)
			{
				unpackedMesh.boneWeights = new BoneWeight[optimizedVertexMap.Count];
			}

            unpackedMesh.bindposes = sourceBindposes;

            for (int i = 0; i < optimizedVertexMap.Count; i++)
			{
				int remappedIndex = optimizedVertexMap[i];

				unpackedMesh.vertices[i] = sourceVertices[remappedIndex];
				if (unpackedMesh.normals != null)
				{
					unpackedMesh.normals[i] = sourceNormals[remappedIndex];
				}
				if (unpackedMesh.tangents != null)
				{
					unpackedMesh.tangents[i] = sourceTangents[remappedIndex];
				}
				if (unpackedMesh.uv0s != null)
				{
					unpackedMesh.uv0s[i] = sourceUVs[remappedIndex];
				}
				if (unpackedMesh.boneWeights != null)
				{
					unpackedMesh.boneWeights[i] = sourceBoneWeights[remappedIndex];
				}
			}

			unpackedMesh.triangles = triangles;
			
			if (sourceMesh.blendShapeCount > 0)
			{
				Vector3[] sourceBlendTangents = new Vector3[sourceVertices.Length];
				Vector3[] deltaVertices = new Vector3[optimizedVertexMap.Count];
				Vector3[] deltaNormals = new Vector3[optimizedVertexMap.Count];
				Vector3[] deltaTangents = new Vector3[optimizedVertexMap.Count];
				for (int i = 0; i < sourceMesh.blendShapeCount; i++)
				{
					string blendShapeName = sourceMesh.GetBlendShapeName(i);
					int frameCount = sourceMesh.GetBlendShapeFrameCount(i);

					float weight = sourceMesh.GetBlendShapeFrameWeight(i, 0);
					sourceMesh.GetBlendShapeFrameVertices(i, 0, sourceVertices, sourceNormals, sourceBlendTangents);
					for (int k = 0; k < unpackedMesh.vertices.Length; k++)
					{
						int remappedIndex = optimizedVertexMap[k];
						deltaVertices[k] = sourceVertices[remappedIndex];
						deltaNormals[k] = sourceNormals[remappedIndex];
						deltaTangents[k] = sourceBlendTangents[remappedIndex];
					}

					UnpackedMesh.BlendShape blendShape = new UnpackedMesh.BlendShape();
					blendShape.name =blendShapeName;
					blendShape.deltaVertices = deltaVertices;
					blendShape.deltaNormals = deltaNormals;
					blendShape.deltaTangents = deltaTangents;
					unpackedMesh.blendShapes.Add(blendShape);
				}
			}

			return unpackedMesh;
		}

		public static unsafe void MatrixArrayToColorArray(Matrix4x4[] mats, Color[] colors)
		{
			fixed (Matrix4x4* pMatri = mats)
			{
				fixed (Color* pColors = colors)
				{
					Matrix4x4* pMats = pMatri;
					Color* pCols = pColors;

					for (int i = 0; i < mats.Length; i++)
					{
						pCols->r    =pMats->m00;
						pCols->g    =pMats->m01;
						pCols->b    =pMats->m02;
						pCols->a    =pMats->m03;

						pCols++;

						pCols->r    =pMats->m10;
						pCols->g    =pMats->m11;
						pCols->b    =pMats->m12;
						pCols->a    =pMats->m13;

						pCols++;

						pCols->r    =pMats->m20;
						pCols->g    =pMats->m21;
						pCols->b    =pMats->m22;
						pCols->a    =pMats->m23;

						pCols++;

						pCols->r    =pMats->m30;
						pCols->g    =pMats->m31;
						pCols->b    =pMats->m32;
						pCols->a    =pMats->m33;
						pCols++;
						pMats++;
					}
				}
			}
		}

		public static Vector3 ToTangentSpace(Vector3 inVector, Vector3 normal, Vector3 binormal, Vector3 tangent)
		{
			Vector3 outNormal;
			outNormal.x = Vector3.Dot(tangent, inVector);
			outNormal.y = Vector3.Dot(binormal, inVector);
			outNormal.z = Vector3.Dot(normal, inVector);

			return outNormal;
		}

		public static Vector3 FromTangentSpace(Vector3 inVector, Vector3 normal, Vector3 binormal, Vector3 tangent)
		{
			Vector3 v0 = new Vector3(tangent.x, binormal.x, normal.x);
			Vector3 v1 = new Vector3(tangent.y, binormal.y, normal.y);
			Vector3 v2 = new Vector3(tangent.z, binormal.z, normal.z);

			Vector3 outNormal;
			outNormal.x = Vector3.Dot(v0, inVector);
			outNormal.y = Vector3.Dot(v1, inVector);
			outNormal.z = Vector3.Dot(v2, inVector);

			return outNormal;
		}
	}
}
