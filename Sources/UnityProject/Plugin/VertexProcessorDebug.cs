using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public class VertexProcessorDebug:System.IDisposable
	{
		public VertexProcessor processor { get; private set; }

		private Mesh _mesh;
		public Mesh mesh
		{
			get
			{
				if (!_mesh)
				{
					RebuildDebugMesh();
				}
				return _mesh;
			}
			set
			{
				_mesh = value;
			}
		}

		private Material _material;
		private Material material
		{
			get
			{
				return _material ? _material : _material = new Material(Shader.Find("Hidden/NeoFur/Debug/FurDebug"));
			}
		}

		public VertexProcessorDebug(VertexProcessor processor)
		{
			this.processor = processor;
		}

		private void RebuildDebugMesh()
		{
			if (processor.unpackedMeshResource == null)
			{
				return;
			}

			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
			mesh.bounds = processor.meshResource.value.bounds;

			Vector3[] meshVertices = processor.unpackedMeshResource.value.vertices;
			Vector3[] meshNormals = processor.unpackedMeshResource.value.normals;
			Vector2[] meshUV1s = processor.unpackedMeshResource.value.uv1s;

			int meshVertexCount = meshVertices.Length;

			Vector3[] vertices = new Vector3[meshVertexCount*4];
			Vector3[] normals = new Vector3[meshVertexCount*4];
			Vector2[] uv0s = new Vector2[meshVertexCount*4];
			Vector2[] uv1s = new Vector2[meshVertexCount*4];
			int[] controlPointLineIndices = new int[meshVertexCount*2];
			int[] guideLineIndices = new int[meshVertexCount*2];

			for (int i = 0; i < meshVertexCount; i++)
			{
				int iTimes4 = i*4;
				int vertexIndex0 = iTimes4+0;
				int vertexIndex1 = iTimes4+1;
				int vertexIndex2 = iTimes4+2;
				int vertexIndex3 = iTimes4+3;

				Vector3 meshVertex = meshVertices[i];
				Vector3 meshNormal = meshNormals[i];
				Vector2 meshUV1 = meshUV1s[i];

				vertices[vertexIndex0] = meshVertex;
				vertices[vertexIndex1] = meshVertex;
				vertices[vertexIndex2] = meshVertex;
				vertices[vertexIndex3] = meshVertex;
				uv0s[vertexIndex0] = new Vector2(0, 0);
				uv0s[vertexIndex1] = new Vector2(1, 0);
				uv0s[vertexIndex2] = new Vector2(2, 0);
				uv0s[vertexIndex3] = new Vector2(3, 0);

				uv1s[vertexIndex0] = meshUV1;
				uv1s[vertexIndex1] = meshUV1;
				uv1s[vertexIndex2] = meshUV1;
				uv1s[vertexIndex3] = meshUV1;

				controlPointLineIndices[i*2+0] = vertexIndex0;
				controlPointLineIndices[i*2+1] = vertexIndex1;
				guideLineIndices[i*2+0] = vertexIndex2;
				guideLineIndices[i*2+1] = vertexIndex3;
			}

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv0s;
			mesh.uv2 = uv1s;

			mesh.subMeshCount = 2;
			mesh.SetIndices(controlPointLineIndices, MeshTopology.Lines, 0);
			mesh.SetIndices(guideLineIndices, MeshTopology.Lines, 1);
		}

		public void Dispose()
		{
			if (_mesh)
			{
				Object.DestroyImmediate(_mesh);
			}
		}

		public void DrawNow(bool drawControlPoints, bool drawGuides)
		{
			if (!drawControlPoints && !drawGuides)
			{
				return;
			}

			if (!mesh)
			{
				return;
			}

			processor.BindToMaterial(material);

			material.SetPass(0);
			if (drawControlPoints)
			{
				Graphics.DrawMeshNow(mesh, processor.neoFurAsset.renderer.localToWorldMatrix, 0);
			}
			if (drawGuides)
			{
				Graphics.DrawMeshNow(mesh, processor.neoFurAsset.renderer.localToWorldMatrix, 1);
			}
		}

		public void Draw(bool drawControlPoints, bool drawGuides)
		{
			if (!drawControlPoints && !drawGuides)
			{
				return;
			}

			if (!mesh)
			{
				return;
			}

			processor.BindToMaterial(material);

			if (drawControlPoints)
			{
				Graphics.DrawMesh(mesh, processor.neoFurAsset.renderer.localToWorldMatrix, material, 0, null, 0);
			}
			if (drawGuides)
			{
				Graphics.DrawMesh(mesh, processor.neoFurAsset.renderer.localToWorldMatrix, material, 0, null, 1);
			}
		}
	}
}
