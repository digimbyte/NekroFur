using UnityEditor;
using NeoFur.Data;
using UnityEngine;
using System.Collections.Generic;
using NeoFurUnityPlugin;
using System.IO;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFur.Editor
{
	[CanEditMultipleObjects]
    [CustomEditor(typeof(PreComputedGuideData))]
    class PreComputedGuideDataEditor : UnityEditor.Editor
    {
		private static string projectPath
		{
			get
			{
				return Directory.GetParent(Application.dataPath).FullName;
			}
		}

		private SerializedProperty sourceMeshProperty;
		private SerializedProperty splineDataFileProperty;

		private void OnEnable()
		{
			sourceMeshProperty = serializedObject.FindProperty("sourceMesh");
			splineDataFileProperty = serializedObject.FindProperty("SplineDataFile");
		}

		public override void OnInspectorGUI()
        {
			serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(splineDataFileProperty);
            EditorGUI.EndDisabledGroup();

			if (string.IsNullOrEmpty(splineDataFileProperty.stringValue))
			{
				EditorGUILayout.HelpBox("You must select an fbx file to load spline guides from.", MessageType.Error);
			}

			// let the user force the update
			if (GUILayout.Button("Select FBX File"))
			{
				string path = EditorUtility.OpenFilePanel("Select and FBX file with guide splines.", "Assets/", "fbx");
				if (!string.IsNullOrEmpty(path))
				{
					if (projectPath.Length+1 >= path.Length)
					{
						Debug.LogError("Path must be inside project directory.");
					}
					else
					{
						path = path.Substring(projectPath.Length+1);
						if (!File.Exists(path))
						{
							throw new System.Exception("File \""+path+"\" does not exist.");
						}
						else
						{
							Undo.RecordObjects(targets, "Load FBX Splines");
							foreach (var target in targets)
							{
								PreComputedGuideData data = (PreComputedGuideData)target;
								data.SplineDataFile = path;
							}
						}
					}
				}
			}

			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(sourceMeshProperty);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Spline Data Edit");
				foreach (var target in targets)
				{
					Reload((PreComputedGuideData)target);
				}
				
			}

			EditorGUILayout.Space();

			// let the user force the update
			if (GUILayout.Button("Force Update"))
            {
				foreach (var target in targets)
				{
					Reload((PreComputedGuideData)target);
				}
			}

			serializedObject.ApplyModifiedProperties();
		}

		[MenuItem("Assets/Create/NeoFur/PreComputedGuideData")]
        public static void CreateAsset()
        {
			PreComputedGuideData asset = CreateInstance<PreComputedGuideData>();

			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (path == "")
			{
				path = "Assets";
			}
			else if (Path.GetExtension(path) != "")
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}

			path = AssetDatabase.GenerateUniqueAssetPath(path+"/New "+typeof(PreComputedGuideData).Name+".asset");

			SaveAsset(asset, path, true);
		}

		public static PreComputedGuideData CreateAssetWithPopup(Mesh sourceMesh, bool select)
		{
			string targetPath = EditorUtility.SaveFilePanelInProject("Save PreComputedGuideData", "New PreComputedGuideData", "asset", "Select a location to save the new PreComputedGuideData.");
			Debug.Log(targetPath);
			if (string.IsNullOrEmpty(targetPath))
			{
				return null;
			}

			PreComputedGuideData asset = CreateInstance<PreComputedGuideData>();
			asset.sourceMesh = sourceMesh;

			SaveAsset(asset, targetPath, select);

			return asset;
		}

		public static void SaveAsset(PreComputedGuideData asset, string path, bool select)
		{
			AssetDatabase.CreateAsset(asset, path);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			if (select)
			{
				EditorApplication.delayCall += () =>
				{
					Selection.activeObject = asset;
				};
			}
		}

        private static void Reload(PreComputedGuideData data)
        {
            SaveSplineData(data);
        }

        private static void SaveSplineData(PreComputedGuideData data)
        {
            if (string.IsNullOrEmpty(data.SplineDataFile)) return;
			
            if (!File.Exists(data.SplineDataFile))
            {
                Debug.LogError("Spline FBX File does not exist!");
                return;
            }

            if (!data.sourceMesh)
            {
                Debug.LogWarning("You need to assign a mesh");
                return;
            }

            FBXLoad fbxLoader = new FBXLoad();

            fbxLoader.Load(data.SplineDataFile);
            fbxLoader.BakeModelTransform();
            FBXLoad.FBXLine[] lines = fbxLoader.GetLines().ToArray();

			data.UpdateGuides(lines);

            EditorUtility.SetDirty(data);
		}
    }
}
