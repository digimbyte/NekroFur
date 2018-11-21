using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public class UnpackedMesh
	{
		public class BlendShape
		{
			public string name;
			public Vector3[] deltaVertices;
			public Vector3[] deltaNormals;
			public Vector3[] deltaTangents;
		}

		public Mesh originalMesh { get; private set; }
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector4[] tangents;
		public Vector2[] uv0s;
		public Vector2[] uv1s;
		public BoneWeight[] boneWeights;
        public Matrix4x4[] bindposes;
		public int[] triangles;

		public List<BlendShape> blendShapes = new List<BlendShape>();

		public UnpackedMesh()
		{

		}

		public UnpackedMesh(Mesh originalMesh)
		{
			this.originalMesh = originalMesh;
			Unpack();
		}

		public BlendShape GetBlendShape(string name)
		{
			foreach (var blendShape in blendShapes)
			{
				if (blendShape.name == name)
				{
					return blendShape;
				}
			}
			return null;
		}

		private void Unpack()
		{
			vertices = originalMesh.vertices;
			normals = originalMesh.normals;
			tangents = originalMesh.tangents;
			uv0s = originalMesh.uv;
			uv1s = originalMesh.uv2;
			boneWeights = originalMesh.boneWeights;
			triangles = originalMesh.triangles;
		}

		public void Apply()
		{
			originalMesh.vertices = vertices;
			originalMesh.normals = normals;
			originalMesh.tangents = tangents;
			originalMesh.uv = uv0s;
			originalMesh.uv2 = uv1s;
			originalMesh.boneWeights = boneWeights;
            originalMesh.bindposes = bindposes;
			originalMesh.triangles = triangles;
			originalMesh.UploadMeshData(false);
		}

		public Mesh ToMesh()
		{
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.tangents = tangents;
			mesh.uv = uv0s;
			mesh.uv2 = uv1s;
			mesh.boneWeights = boneWeights;
            mesh.bindposes = bindposes;
            mesh.triangles = triangles;

			foreach (var blendShape in blendShapes)
			{
				mesh.AddBlendShapeFrame(blendShape.name, 1, blendShape.deltaVertices, blendShape.deltaNormals, blendShape.deltaTangents);
			}

			mesh.UploadMeshData(false);

			return mesh;
		}
	}
}
