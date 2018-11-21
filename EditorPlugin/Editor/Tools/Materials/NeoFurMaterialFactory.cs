using UnityEngine;
using UnityEditor;
using NeoFurUnityPlugin;
using System.Collections.Generic;
using System.Linq;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    [InitializeOnLoad]
    public class NeoFurMaterialFactoryWindow : EditorWindow
    {
        static NeoFurMaterialFactoryWindow _window;

        [MenuItem("Neoglyphic/NeoFur/Create/Complex Fur Mat", false, 0)]
        [MenuItem("Assets/Neoglyphic/NeoFur/Create/Complex Fur Mat", false, 0)]
        static void CreateComplexMaterial()
        {
            NeoFurUtils.MaterialUtils.CreateMaterialAsset(
                NeoFurUtils.GetUniqueAssetPathNameInCurrentFolder("New NeoFur Complex Material.mat"),
                NeoFurUtils.MaterialUtils.MaterialQuality.HighEnd);
        }

        [MenuItem("Neoglyphic/NeoFur/Create/Optimized Fur Mat", false, 1)]
        [MenuItem("Assets/Neoglyphic/NeoFur/Create/Optimized Fur Mat", false, 1)]
        static void CreateOptimizedMaterial()
        {
            NeoFurUtils.MaterialUtils.CreateMaterialAsset
                (NeoFurUtils.GetUniqueAssetPathNameInCurrentFolder("New NeoFur Optimized Material.mat"),
                NeoFurUtils.MaterialUtils.MaterialQuality.Optimized);
        }

        [MenuItem("Neoglyphic/NeoFur/Create/Mobile Fur Mat", false, 2)]
        [MenuItem("Assets/Neoglyphic/NeoFur/Create/Mobile Fur Mat", false, 2)]
        static void CreateMobileMaterial()
        {
            NeoFurUtils.MaterialUtils.CreateMaterialAsset(
                NeoFurUtils.GetUniqueAssetPathNameInCurrentFolder("New NeoFur Mobile Material.mat"),
                NeoFurUtils.MaterialUtils.MaterialQuality.Mobile);
        }

        //[MenuItem("Neoglyphic/NeoFur/(Experimental)/Material Factory", false, 0)]
        //[MenuItem("Assets/Neoglyphic/NeoFur/(Experimental)/Material Factory", false, 0)]
        static void CreateMaterialBatch()
        {
            _window = GetWindow<NeoFurMaterialFactoryWindow>(true, "NeoFur Material Factory", true);
            _window.position = new Rect(500, 500, 400, 400);
            _window.Show();
        }

        //[MenuItem("Neoglyphic/NeoFur/(Experimental)/Assign Mats to Scene", false, int.MaxValue)]
        //[MenuItem("Assets/Neoglyphic/NeoFur/(Experimental)/Assign Mats to Scene", false, int.MaxValue)]
        static void AssignMatsInProjectToMatchingMatsInScene()
        {
            string path = NeoFurUtils.GetCurrentProjectFolder();

            Debug.Log("Loading assets from: " + path);

            List<Material> mats = NeoFurUtils.MaterialUtils.GetMatsAtPath(path);

            NeoFurUtils.MaterialUtils.AssignMatsInProjectToMatchingMatsInScene(mats);
        }
		
        string batchText = "";
        NeoFurUtils.MaterialUtils.MaterialQuality profile = NeoFurUtils.MaterialUtils.MaterialQuality.Mobile;
        Action currentAction = Action.NONE;

        enum Action
        {
            NONE = 0,
            BATCH = 1,
            DUPLICATE = 2,
        }

        // draw GUI for material batcher
        void OnGUI()
        {
            currentAction = (Action)EditorGUILayout.EnumPopup("Action to perform: ", currentAction);

            profile = (NeoFurUtils.MaterialUtils.MaterialQuality)EditorGUILayout.EnumPopup("Type of Material:", profile);

            switch (currentAction)
            {
                case Action.BATCH:
                    DoBatch(profile);
                    break;
                case Action.DUPLICATE:
                    NeoFurUtils.MaterialUtils.DuplicateMatsFromScene(profile);
                    break;
                default:
                    break;
            }

            if (GUILayout.Button("Cancel"))
            {
                Close();
                _window = null;
                return;
            }
        }

        void DoBatch(NeoFurUtils.MaterialUtils.MaterialQuality profile)
        {
            EditorGUILayout.LabelField("Batch Text:");

            batchText = EditorGUILayout.TextField(batchText);

            if (GUILayout.Button("Create Batch"))
            {
                string[] names = batchText.Split(new char[] { ',', '\n' });

                for (int i = 0; i < names.Length; ++i)
                {
                    string name = names[i];

                    if (!string.IsNullOrEmpty(name))
                    {
                        NeoFurUtils.MaterialUtils.CreateMaterialAsset(NeoFurUtils.GetUniqueAssetPathNameInCurrentFolder(name), profile);
                    }
                }
            }
        }

        #region commented FIX STRAND TEXTURE code

        /*
        [MenuItem("Assets/NeoFur/Fix Strand Textures")]
        static void FixStrandTexture()
        {
            if (Selection.activeObject != null && Selection.activeObject is Material)
            {
                FixStrandTexture(Selection.activeObject as Material);
            }
        }

        [MenuItem("Assets/NeoFur/Fix Strand Textures ON ALL MATERIALS")]
        static void FixStrandTextures()
        {
            string[] matPaths = AssetDatabase.GetAllAssetPaths().Where(s => s.EndsWith(".mat")).ToArray();

            Debug.Log("Mats processed = " + matPaths.Length);

            EditorUtility.DisplayProgressBar("Working...", "", 0);

            for (int i = 0; i < matPaths.Length; ++i)
            {
                string matPath = matPaths[i];

                Material m = (Material)AssetDatabase.LoadAssetAtPath(matPath, typeof(Material));

                FixStrandTexture(m);

                EditorUtility.DisplayProgressBar("Working...",
                    "Rebuilding mats: " + i + " out of " + matPaths.Length,
                    ((float)i / (float)matPaths.Length));
            }

            EditorUtility.ClearProgressBar();
        }

        static void FixStrandTexture(Material material)
        {
            bool isModified = false;

            Texture oldDefaultTex = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/NeoFur/Textures/T_poission_01.TGA");
            Texture newDefaultTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/NeoFur/Textures/T_poission_01-Growth.dds");
            Texture newDefaultIDTexture = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/NeoFur/Textures/T_poission_01-ColorIndex.dds");

            if (material.shader.name == "NeoFur/FurSurface" || material.shader.name == "NeoFur/FurSurfaceVTFMobile")
            {
                Texture _StrandShapeMapUND = material.GetTexture("_StrandShapeMapUND");
                Texture _StrandShapeMapOVR = material.GetTexture("_StrandShapeMapOVR");

                if (!_StrandShapeMapUND)
                {
                    //material.SetTextureScale("_StrandColorIndexMapUND", material.GetTextureScale("_StrandShapeMapUND"));
                    material.SetTexture("_StrandShapeMapUND", newDefaultTexture);
                    //material.SetTexture("_StrandColorIndexMapUND", newDefaultIDTexture);
                    isModified = true;
                }

                //if (_StrandShapeMapOVR == newDefaultTexture)
                //{
                //	material.SetTextureScale("_StrandColorIndexMapOVR", material.GetTextureScale("_StrandShapeMapOVR"));
                    //material.SetTexture("_StrandShapeMapOVR", newDefaultTexture);
                    //material.SetTexture("_StrandColorIndexMapOVR", newDefaultIDTexture);
                //	isModified = true;
                //}
            }
            if (isModified)
            {
                EditorUtility.SetDirty(material);
            }
        }
        */

        //Probably dont need this anymore.
        /*
        [MenuItem("Assets/NeoFur/Fix Root and Tip Color")]
        static void FixRootTipColor()
        {
            if (Selection.activeObject != null && Selection.activeObject is Material)
            {
                FixRootTipColor(Selection.activeObject as Material);
            }
        }

        [MenuItem("Assets/NeoFur/Fix Root and Tip Color ON ALL MATERIALS")]
        static void FixRootTipColorAll()
        {
            string[] matPaths = AssetDatabase.GetAllAssetPaths().Where(s => s.EndsWith(".mat")).ToArray();

            Debug.Log("Mats processed = " + matPaths.Length);

            EditorUtility.DisplayProgressBar("Fixing Root And Tip Colors", "", 0);

            for (int i = 0; i < matPaths.Length; ++i)
            {
                string matPath = matPaths[i];

                Material m = (Material)AssetDatabase.LoadAssetAtPath(matPath, typeof(Material));

                FixRootTipColor(m);

                EditorUtility.DisplayProgressBar("Fixing Root And Tip Colors",
                    "Rebuilding mats: " + i + " out of " + matPaths.Length,
                    ((float)i / (float)matPaths.Length));
            }

            EditorUtility.ClearProgressBar();
        }

        static void FixRootTipColor(Material material)
        {
            bool isModified = false;

            if (material.shader.name == "NeoFur/FurSurfaceComplex")
            {
                bool usesUNDTexture = material.GetFloat("_bColorMapUND") >= 0.5;
                bool usesOVRTexture = material.GetFloat("_bColorMapOVR") >= 0.5;

                if (usesUNDTexture)
                {
                    material.SetColor("_ColorRootUND", Color.white);
                    material.SetColor("_ColorTipUND", Color.white);
                    isModified = true;
                }
                if (usesOVRTexture)
                {
                    material.SetColor("_ColorRootOVR", Color.white);
                    material.SetColor("_ColorTipOVR", Color.white);
                    isModified = true;
                }
            }
            else if (material.shader.name == "NeoFur/FurSurface")
            {
                bool usesRootTexture = material.GetFloat("_bColorRootMapUND") >= 0.5;
                bool usesTipTexture = material.GetFloat("_bColorTipMapUND") >= 0.5;

                if (usesRootTexture)
                {
                    material.SetColor("_ColorRootUND", Color.white);
                    isModified = true;
                }
                if (usesTipTexture)
                {
                    material.SetColor("_ColorTipUND", Color.white);
                    isModified = true;
                }
            }
            if (isModified)
            {
                EditorUtility.SetDirty(material);
            }
        }
        */

        //[MenuItem("NeoFur/Rebuild Material")]
        //[MenuItem("Assets/NeoFur/Rebuild Material")]
        //static void RebuildMaterial()
        //{
        //    if (Selection.activeObject != null && Selection.activeObject is Material)
        //    {
        //        Material m = Selection.activeObject as Material;
        //        if (m.shader.name == ComplexShader.name)
        //            NeoFurAssetUtils.RebuildMaterialKeywords(m);
        //    }
        //}

        //[MenuItem("NeoFur/Rebuild Material Keywords")]
        //[MenuItem("Assets/NeoFur/Rebuild Material Keywords")]
        //static void RebuildMaterials()
        //{
        //    string[] matPaths = AssetDatabase.GetAllAssetPaths().Where(s => s.EndsWith(".mat")).ToArray();

        //    Debug.Log("Mats processed = " + matPaths.Length);

        //    EditorUtility.DisplayProgressBar("Rebuilding Shader Keywords", "", 0);

        //    for (int i = 0; i < matPaths.Length; ++i)
        //    {
        //        string matPath = matPaths[i];

        //        Material m = (Material)AssetDatabase.LoadAssetAtPath(matPath, typeof(Material));

        //        if (m.shader.name == ComplexShader.name)
        //            NeoFurAssetUtils.RebuildMaterialKeywords(m);

        //        EditorUtility.DisplayProgressBar("Rebuilding Shader Keywords",
        //            "Rebuilding mats: " + i + " out of " + matPaths.Length,
        //            ((float)i / (float)matPaths.Length));
        //    }

        //    EditorUtility.ClearProgressBar();
        //}

        #endregion
    }
}
