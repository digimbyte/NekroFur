using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
    public class Vector3Comparer : System.Collections.Generic.IEqualityComparer<Vector3>
    {
        private static Vector3Comparer _instance;
        public static Vector3Comparer Instance
        {
            get
            {
                if (_instance == null) _instance = new Vector3Comparer();

                return _instance;
            }
        }
        public bool Equals(Vector3 x, Vector3 y)
        {
            return x.x == y.x && x.y == y.y && x.z == y.z;
        }

        public int GetHashCode(Vector3 obj)
        {
            if (obj.x == -0) obj.x = 0;
            if (obj.y == -0) obj.y = 0;
            if (obj.z == -0) obj.z = 0;

            return obj.GetHashCode();
        }
    }

    public class VertexProcessor : System.IDisposable
	{
        private Dictionary<Vector3, int> vec3ToVertIndexMap = new Dictionary<Vector3, int>(Vector3Comparer.Instance);

		public NeoFurAsset neoFurAsset { get; private set; }
		public VertexProcessorResource<Mesh> meshResource { get; private set; }
		public VertexProcessorResource<UnpackedMesh> unpackedMeshResource { get; private set; }
		public VertexProcessorResource<List<int>> optimizedVertexMapResource { get; private set; }
		private Mesh lastRebuiltMeshSource;
		private List<VertexFilter> filterList = new List<VertexFilter>();
		public ReadOnlyCollection<VertexFilter> filters { get; private set; }
		public VertexProcessorResource<Texture2D> basePositionTextureResource { get; private set; }
		public VertexProcessorResource<Texture2D> baseNormalTextureResource { get; private set; }
		public VertexProcessorResource<Texture2D> baseTangentTextureResource { get; private set; }
		public RenderTexture positionTexture { get; private set; }
		public RenderTexture normalTexture { get; private set; }
		public RenderTexture tangentTexture { get; private set; }
		public RenderTexture previousPositionTexture { get; private set; }

		private VertexProcessorDebug _vertexProcessorDebug;
		public VertexProcessorDebug vertexProcessorDebug
		{
			get
			{
				return _vertexProcessorDebug != null ? _vertexProcessorDebug : _vertexProcessorDebug = new VertexProcessorDebug(this);
			}
			private set
			{
				_vertexProcessorDebug = value;
			}
		}

		public string baseResourceKey { get; private set; }

		//TODO: Move this into NeoFurAsset?
		private bool hasPreviousLocalToWorldMatrix;
		private Matrix4x4 _previousLocalToWorldMatrix;
		public Matrix4x4 previousLocalToWorldMatrix
		{
			get
			{
				if (hasPreviousLocalToWorldMatrix)
				{
					return _previousLocalToWorldMatrix;
				}
				return neoFurAsset.renderer.worldToLocalMatrix;
			}
			set
			{
				_previousLocalToWorldMatrix = value;
				hasPreviousLocalToWorldMatrix  = true;
			}
		}

		public float averageProcessTime { get; private set; }
		public float lastProcessTime { get; private set; }

		public VertexProcessor(NeoFurAsset neoFurAsset)
		{
			this.neoFurAsset = neoFurAsset;
			filters = new ReadOnlyCollection<VertexFilter>(filterList);
		}

		System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
		public void Process()
		{
			stopwatch.Start();

			if (neoFurAsset.mesh != lastRebuiltMeshSource)
			{
				hasPreviousLocalToWorldMatrix = false;
				Rebuild();
			}

			//Any new filters that havent been rebuilt at least once need to be rebuilt.
			foreach (var filter in filterList)
			{
				if (!filter.hasRebuildOnce)
				{
					filter.Rebuild();
				}
			}
			
			Shader.SetGlobalFloat("_NeoFur_DeltaTime", neoFurAsset.deltaTime);
			Shader.SetGlobalTexture("_NeoFur_PositionTexture", positionTexture);
			Shader.SetGlobalTexture("_NeoFur_NormalTexture", normalTexture);
			Shader.SetGlobalTexture("_NeoFur_TangentTexture", tangentTexture);
			Shader.SetGlobalTexture("_NeoFur_PreviousPositionTexture", previousPositionTexture);

			Matrix4x4 localToWorldMatrix = neoFurAsset.renderer.localToWorldMatrix;
			Matrix4x4 worldToLocalMatrix = neoFurAsset.renderer.worldToLocalMatrix;
			Shader.SetGlobalMatrix("_NeoFur_LocalToWorldMatrix", localToWorldMatrix);
			Shader.SetGlobalMatrix("_NeoFur_PreviousLocalToWorldMatrix", previousLocalToWorldMatrix);
			Shader.SetGlobalMatrix("_NeoFur_WorldToLocalMatrix", worldToLocalMatrix);

			Shader.SetGlobalFloat("_NeoFur_ShellDistance", neoFurAsset.ShellDistanceInMeters);

			VertexProcessorUtility.CopyTexture(positionTexture, previousPositionTexture);

			VertexProcessorUtility.CopyTexture(basePositionTextureResource.value, positionTexture);
			VertexProcessorUtility.CopyTexture(baseNormalTextureResource.value, normalTexture);
			VertexProcessorUtility.CopyTexture(baseTangentTextureResource.value, tangentTexture);

			foreach (var filter in filterList)
			{
				filter.Process();
			}

			previousLocalToWorldMatrix = localToWorldMatrix;

			stopwatch.Stop();
			lastProcessTime = (float)((double)stopwatch.ElapsedTicks/System.Diagnostics.Stopwatch.Frequency);
			stopwatch.Reset();

			averageProcessTime = Mathf.Lerp(averageProcessTime, lastProcessTime, 0.01f);
		}

		public void Rebuild()
		{
			DestroyResources();

			if (!neoFurAsset.mesh)
			{
				return;
			}

			baseResourceKey = neoFurAsset.mesh.GetInstanceID().ToString()+"_"+neoFurAsset.data.furSubMeshIndex;

			optimizedVertexMapResource = VertexProcessorCache.GetResource<List<int>>(baseResourceKey+"_OptimizedVertexMapResource");
			if (optimizedVertexMapResource.value == null)
			{
				optimizedVertexMapResource.value = new List<int>(neoFurAsset.mesh.vertexCount);
			}

			unpackedMeshResource = VertexProcessorCache.GetResource<UnpackedMesh>(baseResourceKey+"_UnpackedMesh");
			if (unpackedMeshResource.value == null)
			{
				optimizedVertexMapResource.value.Clear();
				unpackedMeshResource.value = VertexProcessorUtility.CreateOptimizedMesh(neoFurAsset.mesh, neoFurAsset.data.furSubMeshIndex, optimizedVertexMapResource.value);
			}

			int width;
			int height;
			VertexProcessorUtility.CalculateVertexTextureSize(unpackedMeshResource.value.vertices.Length, out width, out height);

			basePositionTextureResource = VertexProcessorCache.GetResource<Texture2D>(baseResourceKey+"_BasePositionTexture");
			if (basePositionTextureResource.value == null)
			{
				basePositionTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
				basePositionTextureResource.value.filterMode = FilterMode.Point;
			}

			baseNormalTextureResource = VertexProcessorCache.GetResource<Texture2D>(baseResourceKey+"_BaseNormalTexture");
			if (baseNormalTextureResource.value == null)
			{
				baseNormalTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
				baseNormalTextureResource.value.filterMode = FilterMode.Point;
			}

			baseTangentTextureResource = VertexProcessorCache.GetResource<Texture2D>(baseResourceKey+"_BaseTangentTexture");
			if (baseTangentTextureResource.value == null)
			{
				baseTangentTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
				baseTangentTextureResource.value.filterMode = FilterMode.Point;
			}

			positionTexture = new RenderTexture(width, height, 0, VertexProcessorUtility.float4RenderTextureFormat);
			positionTexture.filterMode = FilterMode.Point;
			positionTexture.Create();

			normalTexture = new RenderTexture(width, height, 0, VertexProcessorUtility.float4RenderTextureFormat);
			normalTexture.filterMode = FilterMode.Point;
			normalTexture.Create();

			tangentTexture = new RenderTexture(width, height, 0, VertexProcessorUtility.float4RenderTextureFormat);
			tangentTexture.filterMode = FilterMode.Point;
			tangentTexture.Create();

			previousPositionTexture = new RenderTexture(width, height, 0, VertexProcessorUtility.float4RenderTextureFormat);
			previousPositionTexture.filterMode = FilterMode.Point;
			previousPositionTexture.Create();

			//Vector3[] vertices = mesh.vertices;
			//Vector3[] normals = mesh.normals;
			//Vector4[] tangents = mesh.tangents;
			//Vector2[] uv1 = mesh.uv2;
			if (unpackedMeshResource.value.uv1s == null || unpackedMeshResource.value.uv1s.Length == 0)
			{
				unpackedMeshResource.value.uv1s = new Vector2[unpackedMeshResource.value.vertices.Length];
			}

			Color[] positionPixels = new Color[width*height];
			Color[] normalPixels = new Color[width*height];
			Color[] tangentPixels = new Color[width*height];
			/*
            // get CPs and add to dictionary
            for (int i = 0; i < unpackedMesh.vertices.Length; i++)
            {
                if (!vec3ToVertIndexMap.ContainsKey(unpackedMesh.vertices[i]))
                {
                    vec3ToVertIndexMap.Add(unpackedMesh.vertices[i], i);
                }
            }

            cpIndexToVertIndex = new int[vec3ToVertIndexMap.Count];

            {
                int i = 0;

                foreach (var item in vec3ToVertIndexMap)
                {
                    cpIndexToVertIndex[i] = item.Value;
                    ++i;
                }
            }
			*/
			for (int i = 0; i < unpackedMeshResource.value.vertices.Length; i++)
			{
				int y = i/width;
				int x = i%width;

				Vector3 vertex = unpackedMeshResource.value.vertices[i];
				Vector3 normal = unpackedMeshResource.value.normals[i];
				Vector4 tangent = unpackedMeshResource.value.tangents[i];

				Color positionColor = new Color(vertex.x, vertex.y, vertex.z, 1);
				Color normalColor = new Color(normal.x, normal.y, normal.z, 1);
				Color tangentColor = new Color(tangent.x, tangent.y, tangent.z, 1);

				positionPixels[i] = positionColor;
				normalPixels[i] = normalColor;
				tangentPixels[i] = tangentColor;

				unpackedMeshResource.value.uv1s[i] = new Vector2((x+0.5f)/width, (y+0.5f)/height);
			}

			meshResource = VertexProcessorCache.GetResource<Mesh>(baseResourceKey+"_Mesh");
			if (meshResource.value == null)
			{
				meshResource.value = unpackedMeshResource.value.ToMesh();
				meshResource.value.hideFlags = HideFlags.DontSave;
				meshResource.value.bounds = neoFurAsset.mesh.bounds;
			}

			basePositionTextureResource.value.SetPixels(positionPixels);
			baseNormalTextureResource.value.SetPixels(normalPixels);
			baseTangentTextureResource.value.SetPixels(tangentPixels);

			basePositionTextureResource.value.Apply();
			baseNormalTextureResource.value.Apply();
			baseTangentTextureResource.value.Apply();

			//All filters need to be rebuilt.
			foreach (var filter in filterList)
			{
				filter.Rebuild();
			}

			lastRebuiltMeshSource = neoFurAsset.mesh;
		}

		public void DebugDraw()
		{
			if (positionTexture)
			{
				Rect rect;

                GUILayout.Label("BasePos");
                rect = GUILayoutUtility.GetRect(basePositionTextureResource.value.width * 4, basePositionTextureResource.value.height * 4, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(rect, basePositionTextureResource.value, ScaleMode.StretchToFill, false);
                GUILayout.Space(5);

                GUILayout.Label("BaseNorm");
                rect = GUILayoutUtility.GetRect(baseNormalTextureResource.value.width * 4, baseNormalTextureResource.value.height * 4, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(rect, baseNormalTextureResource.value, ScaleMode.StretchToFill, false);
                GUILayout.Space(5);

                GUILayout.Label("BaseTan");
                rect = GUILayoutUtility.GetRect(baseTangentTextureResource.value.width * 4, baseTangentTextureResource.value.height * 4, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(rect, baseTangentTextureResource.value, ScaleMode.StretchToFill, false);
                GUILayout.Space(5);

                GUILayout.Label("PositionTexture");
				rect = GUILayoutUtility.GetRect(positionTexture.width*4, positionTexture.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, positionTexture, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);

				GUILayout.Label("NormalTexture");
				rect = GUILayoutUtility.GetRect(normalTexture.width*4, normalTexture.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, normalTexture, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);

				GUILayout.Label("TangentTexture");
				rect = GUILayoutUtility.GetRect(tangentTexture.width*4, tangentTexture.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, tangentTexture, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);

				foreach (var filter in filterList)
				{
					filter.DebugDraw();
				}
			}
		}

		public T AddFilter<T>() where T:VertexFilter
		{
			return (T)AddFilter(typeof(T));
		}

		public VertexFilter AddFilter(System.Type type)
		{
			VertexFilter filter = (VertexFilter)System.Activator.CreateInstance(type, this);
			filterList.Add(filter);
			return filter;
		}

		public void RemoveFilter(VertexFilter filter)
		{
			if (filterList.Contains(filter))
			{
				filterList.Remove(filter);
			}
		}

		public T GetFilter<T>() where T:VertexFilter
		{
			foreach (var filter in filterList)
			{
				if (filter is T)
				{
					return (T)filter;
				}
			}
			return null;
		}

		public void SetFilterIndex(VertexFilter filter, int index)
		{
			if (!filterList.Contains(filter))
			{
				return;
			}
			filterList.Remove(filter);
			filterList.Insert(index, filter);
		}

		private void DestroyResources()
		{
			if (basePositionTextureResource != null)
			{
				VertexProcessorCache.ReleaseResource(basePositionTextureResource);
				basePositionTextureResource = null;
			}
			if (baseNormalTextureResource != null)
			{
				VertexProcessorCache.ReleaseResource(baseNormalTextureResource);
				basePositionTextureResource = null;
			}
			if (baseTangentTextureResource != null)
			{
				VertexProcessorCache.ReleaseResource(baseTangentTextureResource);
				basePositionTextureResource = null;
			}

			if (positionTexture)
			{
				Object.DestroyImmediate(positionTexture);
			}
			if (normalTexture)
			{
				Object.DestroyImmediate(normalTexture);
			}
			if (tangentTexture)
			{
				Object.DestroyImmediate(tangentTexture);
			}

			if (previousPositionTexture)
			{
				Object.DestroyImmediate(previousPositionTexture);
			}

			if (optimizedVertexMapResource != null)
			{
				VertexProcessorCache.ReleaseResource(optimizedVertexMapResource);
				optimizedVertexMapResource = null;
			}

			if (unpackedMeshResource != null)
			{
				VertexProcessorCache.ReleaseResource(unpackedMeshResource);
				unpackedMeshResource = null;
			}

			if (meshResource != null)
			{
				VertexProcessorCache.ReleaseResource(meshResource);
				meshResource = null;
			}

			if (vertexProcessorDebug != null)
			{
				vertexProcessorDebug.Dispose();
				vertexProcessorDebug = null;
			}
		}

		public void BindToMaterial(Material material)
		{
			material.SetTexture("_NeoFur_PositionTexture", positionTexture);
			material.SetTexture("_NeoFur_NormalTexture", normalTexture);
			material.SetTexture("_NeoFur_TangentTexture", tangentTexture);
			material.SetFloat("_NeoFur_ShellDistance", neoFurAsset.ShellDistanceInMeters);

			for (int i = 0; i < filterList.Count; i++)
			{
				VertexFilter filter = filterList[i];
				filter.BindToMaterial(material);
			}
		}

		public void Dispose()
		{
			while (filterList.Count > 0)
			{
				filterList[0].Dispose();
			}

			DestroyResources();
		}
	}
}
