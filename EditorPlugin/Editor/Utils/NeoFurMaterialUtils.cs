using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    using NeoFurUnityPlugin;

    internal partial class NeoFurUtils
    {
        public static class MaterialUtils
        {
            public enum PropertyType
            {
                NONE = 0, // OBJECT
                FLOAT,
                VECTOR,
                COLOR,
                TEXTURE,
            }

            public enum MaterialQuality
            {
                NONE = 0,
                Mobile = 1,
                Optimized = 2,
                HighEnd = 3,
            }

            static Shader ComplexShader = Shader.Find("NeoFur/FurSurfaceComplex");
            static Shader OptimizedShader = Shader.Find("NeoFur/FurSurface");
            static Shader MobileShader = Shader.Find("NeoFur/FurSurfaceVTFMobile");


            static Shader GetShaderFromProfile(MaterialQuality profile)
            {
                Shader sh;

                switch (profile)
                {
                    case MaterialQuality.Mobile:
                        sh = MobileShader;
                        break;
                    case MaterialQuality.Optimized:
                        sh = OptimizedShader;
                        break;
                    case MaterialQuality.HighEnd:
                        sh = ComplexShader;
                        break;
                    default:
                        sh = ComplexShader;
                        break;
                }

                return sh;
            }

            public static Material DuplicateMaterialAsset(string fileName, Material mat,
                MaterialQuality profile = MaterialQuality.HighEnd)
            {
                Shader s = GetShaderFromProfile(profile);

                if (!s)
                {
                    Debug.LogErrorFormat("NeoFur: Trying to duplicate {0} mat but shader could not be found.", profile.ToString());
                    return null;
                }

                Material m = new Material(mat);

                if (s != null) m.shader = s;

                CreateMaterialAsset(fileName, m);

                return m;
            }

            public static Material CreateMaterialAsset(string fileName, MaterialQuality profile)
            {
                Shader s = GetShaderFromProfile(profile);

                if (!s)
                {
                    Debug.LogErrorFormat("NeoFur: Trying to make {0} mat but shader could not be found.", profile.ToString());
                    return null;
                }

                Material m = new Material(s);

                CreateMaterialAsset(fileName, m);

                return m;
            }

            public static void CreateMaterialAsset(string fileName, Material m)
            {
                //add file extension
                if (!fileName.EndsWith(".mat"))
                    fileName += ".mat";
                AssetDatabase.SetLabels(m, new string[] { "Material", "NeoFur", "" });
                AssetDatabase.CreateAsset(m, fileName);
            }

            public static void RebuildMaterialKeywords(Material m)
            {
                //if (m.shader.name.CompareTo(shaderName) == 0)
                {
                    RebuildKeyword(m, "HEIGHT_MAPS_UND_ON");
                    RebuildKeyword(m, "COLOR_MAPS_UND_ON");
                    RebuildKeyword(m, "NEOFUR_COMPLEX_DENSITY_ON");
                    RebuildKeyword(m, "NEOFUR_COMPLEX_OVR_ON");
                    RebuildKeyword(m, "NEOFUR_COMPLEX_ON");

                    Selection.activeObject = m;

                    EditorUtility.SetDirty(m);

                    AssetDatabase.SaveAssets();
                }
            }

            public static void RebuildMaterial(Material m)
            {
                bool colorMapsUND = m.GetFloat("_bColorMapUND") == 1;
                bool colorMapsOVR = m.GetFloat("_bColorMapOVR") == 1;
                bool heightMapsUND = m.GetFloat("_bHeightMapUND") == 1;
                bool ovr = m.GetFloat("_bOVR") == 1;
                bool densityUND = m.GetFloat("_bDensityUND") == 1;

                RebuildKeyword(m, "HEIGHT_MAPS_UND_ON", heightMapsUND);
                RebuildKeyword(m, "COLOR_MAPS_UND_ON", heightMapsUND);
                RebuildKeyword(m, "NEOFUR_COMPLEX_DENSITY_ON", heightMapsUND);
                RebuildKeyword(m, "NEOFUR_COMPLEX_OVR_ON", heightMapsUND);
                RebuildKeyword(m, "NEOFUR_COMPLEX_ON", heightMapsUND);
            }
            
            public static void RebuildKeyword(Material m, string keyword)
            {
                bool b = m.IsKeywordEnabled(keyword);
                RebuildKeyword(m, keyword, b);
            }

            private static void RebuildKeyword(Material m, string keyword, bool b)
            {
                if (b) m.EnableKeyword(keyword);
                else m.DisableKeyword(keyword);
            }

            public static List<Material> GetMatsAtPath(string path)
            {
                string[] matPaths = AssetDatabase.FindAssets("t:Material", new string[] { path });

                List<Material> mats = new List<Material>();

                for (int i = 0; i < matPaths.Length; ++i)
                {
                    EditorUtility.DisplayProgressBar("Assigning Materials to Scene",
                        "Loading materials: " + i + "/" + matPaths.Length,
                        (float)i / matPaths.Length);

                    if (!string.IsNullOrEmpty(matPaths[i]))
                    {
                        Debug.Log("Loading material: " + matPaths[i]);

                        Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(matPaths[i]));

                        if (m == null) continue;

                        mats.Add(m);
                    }
                }

                return mats;
            }

            public static void AssignMatsInProjectToMatchingMatsInScene(List<Material> mats)
            {
                NeoFurAsset[] nfas = GetNeoFurAssetsInCurrentScene();

                if (nfas.Length == 0)
                {
                    Debug.LogWarning("No NeoFur Assets found in Scene");
                    return;
                }


                if (mats.Count == 0)
                {
                    Debug.LogWarning("No mats found in current project folder");
                    EditorUtility.ClearProgressBar();
                    return;
                }
                else
                {
                    Debug.Log("Found " + mats.Count + " materials");
                }

                for (int i = 0; i < nfas.Length; ++i)
                {
                    NeoFurAsset nfa = nfas[i];

                    EditorUtility.DisplayProgressBar("Assigning Materials to Scene",
                        "Assigning " + i + "/" + nfas.Length + " to " + nfa.gameObject.name,
                        (float)i / (float)nfas.Length);

                    for (int m = 0; m < mats.Count; ++m)
                    {
                        Material mat = mats[m] as Material;

                        if (mat == null)
                        {
                            Debug.LogWarning("Mat is null for some reason");
                            continue;
                        }

                        if (nfa.data.furryMat == null)
                        {
                            Debug.Log("NFA material ref is NULL");
                            continue;
                        }

                        if (mat.name.CompareTo(nfa.data.furryMat.name) == 0)
                        {
                            nfa.data.furryMat = mat;
                            EditorUtility.SetDirty(nfa);
                        }
                    }
                }

                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

                EditorUtility.ClearProgressBar();
            }

            public static void DuplicateMatsFromScene(MaterialQuality profile)
            {
                if (GUILayout.Button("Duplicate Mats"))
                {
                    NeoFurAsset[] assets = GetNeoFurAssetsInCurrentScene();

                    for (int i = 0; i < assets.Length; ++i)
                    {
                        if (assets == null) continue;

                        EditorUtility.DisplayProgressBar("Duplicating Materials", "Copying: " + i +
                            " of " + assets.Length, (float)i / (float)assets.Length);

                        Material m = assets[i].data.furryMat;

                        if (m == null) continue;

                        DuplicateMaterialAsset(
                            GetUniqueAssetPathNameInCurrentFolder(m.name),
                            m, profile);
                    }

                    EditorUtility.ClearProgressBar();
                }
            }

            public static void DoBatch(string batchText, MaterialQuality batchProfile)
            {
                string[] names = batchText.Split(new char[] { ',', '\n' });

                for (int i = 0; i < names.Length; ++i)
                {
                    string name = names[i];

                    if (!string.IsNullOrEmpty(name))
                    {
                        CreateMaterialAsset(GetUniqueAssetPathNameInCurrentFolder(name),
                                            batchProfile);
                    }
                }
            }
        }
    }
}
