using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;
using NeoFurUnityPlugin;
using System.Collections;
using System.IO;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    internal partial class NeoFurUtils
    {
        public static class ConfigUtils
        {
            static readonly string MaterialsFolderName = "Materials";
            
            [InitializeOnLoadMethod]
            static void InitOnLoad_UpdateFunc()
            {
                EditorApplication.update += UpdateCB;
            }

            static bool m_needsToMakeThings = false;
            static string m_currentFolder = "Assets";
            static string m_targetFolderName = "";
            static NeoFurConfig[] m_configs;
            static string m_stringStdOut = "";
            static string m_stringStdErr = "";

            static void UpdateCB()
            {
                if (!m_needsToMakeThings) return;
                if (m_configs == null || m_configs.Length == 0) return;

                //make material folders
                for(int i = 0; i < m_configs.Length; ++i)
                {
                    NeoFurConfig config = m_configs[i];

                    string materialFolder = m_currentFolder + "/" + MaterialsFolderName;

                    if (!AssetDatabase.IsValidFolder(materialFolder))
                    {
                        materialFolder = AssetDatabase.CreateFolder(m_currentFolder, MaterialsFolderName);

                        return;
                    }

                    // get material profile directory
                    string materialProfileFolder = materialFolder + "/" + (m_targetFolderName);

                    if (!AssetDatabase.IsValidFolder(materialProfileFolder))
                    {
                        materialProfileFolder = AssetDatabase.CreateFolder(materialFolder, m_targetFolderName);

                        return;
                    }

                    EditorUtility.DisplayProgressBar("NeoFurConfig Utils", "Generating material paths...", i / m_configs.Length);
                }

                EditorUtility.ClearProgressBar();

                MakeThings(m_currentFolder);

                DoTheDebugDump(m_currentFolder, m_targetFolderName);

                // reset the generation state
                Reset();                
            }

            private static void LogError(string msg)
            {
                msg = ("!!! ERROR ERROR !!!   " + msg + "\n");
                m_stringStdErr += msg;
                Log(msg);
            }

            private static void LogWarning(string msg)
            {
                msg = ("!!! WARNING !!!   " + msg + "\n");
                m_stringStdErr += msg;
                Log(msg);
            }

            private static void Log(string msg)
            {
                m_stringStdOut += msg + "\n";
            }

            private static void Reset()
            {
                m_stringStdOut = "";
                m_stringStdErr = "";
                m_needsToMakeThings = false;
                m_currentFolder = "Assets";
                m_targetFolderName = "";
                m_configs = null;
            }

            // [MenuItem("Neoglyphic/NeoFur/Import Materials...", false, 0)]
            // [MenuItem("Assets/Neoglyphic/NeoFur/Import Materials...", false, 0)]
            static void ImportNeoFurMaterials()
            {
                Reset();

                m_currentFolder = GetCurrentProjectFolder();

                string configPath = EditorUtility.OpenFolderPanel("Select Config Folder", m_currentFolder, "");

                if (configPath == null || configPath.Length == 0) return;

                //remove the asset data path
                string pathToSubtract = Application.dataPath;

                pathToSubtract = pathToSubtract.Substring(0, Application.dataPath.LastIndexOf("/"));

                configPath = configPath.Substring(pathToSubtract.Length + 1);

                if (configPath == null || configPath.Length == 0) return;

                m_targetFolderName = configPath.Substring(configPath.LastIndexOf("/") + 1);
                
                Log("Target folder name = " + m_targetFolderName);

                string[] allFiles = AssetDatabase.FindAssets("", new string[] { configPath });

                List<string> jsonFiles = new List<string>();

                foreach (string str in allFiles)
                {
                    string name = AssetDatabase.GUIDToAssetPath(str);

                    if (name.EndsWith(".JSON") || name.EndsWith(".json"))
                    {
                        Log("Adding config file: " + name);

                        jsonFiles.Add(name);
                    }
                }

                var paths = jsonFiles.ToArray();

                // load configs
                m_configs = GetNeoFurConfigsFromJsonFiles(paths);

                m_needsToMakeThings = true;
            }

            private static void DoTheDebugDump(string folder, string name)
            {
                // standard output
                string fPath = folder + "/" + name.ToLower() + "_debug_dump.txt";
                if (File.Exists(fPath))
                    File.Delete(fPath);
                StreamWriter sw = File.CreateText(fPath);
                sw.Write(m_stringStdOut);
                sw.Close();
                sw = null;

                // error output
                fPath = folder + "/" + name.ToLower() + "_debug_error_dump.txt";
                if (File.Exists(fPath))
                    File.Delete(fPath);
                sw = File.CreateText(fPath);
                sw.Write(m_stringStdErr);
                sw.Close();
                sw = null;

                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }

            private static void MakeThings(string currentFolder)
            {
                // get NFA components
                GameObject go = GameObject.Find("MatBalls");
                Dictionary<int, NeoFurAsset> nfaDict = new Dictionary<int, NeoFurAsset>();

                for (int i = 0; i < go.transform.childCount; ++i)
                {
                    Transform child = go.transform.GetChild(i);
                    string numberStr = child.name;
                    numberStr = numberStr.Substring(numberStr.LastIndexOf("_") + 1);

                    int number = int.Parse(numberStr);

                    if (!nfaDict.ContainsKey(number))
                    {
                        Log("Found nfa go: " + child.name);
                        nfaDict.Add(number, child.GetComponentInChildren<NeoFurAsset>());
                    }
                }

                //wrap in try catch just in case it crashes
                //so the display bar is closed and doesnt hang Unity
                try
                {
                    List<Material> nfMats = new List<Material>();
                    // assign config data to NFA Components and Materials
                    for (int i = 0; i < m_configs.Length; ++i)
                    {
                        NeoFurConfig config = m_configs[i];
                        NeoFurAsset nfa = nfaDict[i + 1];
                        bool cancel =
                            EditorUtility.DisplayCancelableProgressBar("NeoFur Generation from Configs",
                                                "Building assets", (float)i / (float)(m_configs.Length));

                        if (cancel) break;

                        Material m = CreateNFAPairFromConfig(currentFolder, config, nfa);

                        if (m != null)
                            nfMats.Add(m);
                    }
                }
                catch (Exception e)
                {
                    string err = "Config generator crashed: " + e.Message;
                    LogError(err);
                    Debug.LogError(err);
                }

                EditorUtility.ClearProgressBar();
            }

            static MaterialUtils.MaterialQuality GetShaderTypeFromConfig(NeoFurConfig config)
            {
                switch (config.ShaderType)
                {
                    case "Complex":
                        return MaterialUtils.MaterialQuality.HighEnd;
                    case "Optimized":
                        return MaterialUtils.MaterialQuality.Optimized;
                    case "Mobile":
                        return MaterialUtils.MaterialQuality.Mobile;
                }

                return MaterialUtils.MaterialQuality.HighEnd;
            }

            static Material CreateNFAPairFromConfig(string currentFolder, NeoFurConfig config, NeoFurAsset nfa)
            {
                Material mat = null;

                string materialFolder = currentFolder + "/" + MaterialsFolderName;

                if (!AssetDatabase.IsValidFolder(materialFolder))
                {
                    materialFolder = AssetDatabase.CreateFolder(currentFolder, MaterialsFolderName);
                }

                // get material profile directory
                MaterialUtils.MaterialQuality qualityProfile = GetShaderTypeFromConfig(config);

                string materialProfileFolder = materialFolder + "/" + (m_targetFolderName);

                if (!AssetDatabase.IsValidFolder(materialProfileFolder))
                {
                    materialProfileFolder = AssetDatabase.CreateFolder(materialFolder, m_targetFolderName);
                }

                string materialSubName = config.Name.Substring(config.Name.LastIndexOf("_") + 1);
                string materialName = "MAT_NeoFur_" + materialSubName;

                Log("=======================================\nCreating mat + nfa: " + materialName);

                string path = materialProfileFolder + "/" + materialName/* + "_" + config.ShaderType*/ + ".mat";

                // apply settings based on config file
                mat = MaterialUtils.CreateMaterialAsset(path, qualityProfile);
                
                ApplyMaterialProperties(mat, config);

                ApplyComponentProperties(mat, config, nfa);

                Log("=======================================");

                return mat;
            }

            static NeoFurConfig LoadConfigFromJSON(string jsonContents)
            {
                //reference JSON lib to load file
                return JsonUtility.FromJson<NeoFurConfig>(jsonContents);
            }

            static string SaveConfigToJSON(NeoFurConfig config)
            {
                return JsonUtility.ToJson(config);
            }

            static NeoFurConfig[] GetNeoFurConfigsFromJsonFiles(string[] jsonFilePaths)
            {
                List<NeoFurConfig> configs = new List<NeoFurConfig>();

                foreach (string path in jsonFilePaths)
                {
                    string contents = ReadFile(path);

                    if (!string.IsNullOrEmpty(contents))
                    {
                        NeoFurConfig nfConfig = LoadConfigFromJSON(contents);

                        configs.Add(nfConfig);
                    }
                }

                NeoFurConfig[] ret = configs.OrderBy(o => GetAssetIndexFromName(o.Name)).ToArray();

                return ret;
            }

            static void PrintField(object o, FieldInfo field)
            {
                Log("Prop: " + field.Name + " = " + field.GetValue(o));
            }

            static void PrintFields(object o, FieldInfo[] fields)
            {
                foreach (FieldInfo field in fields)
                {
                    PrintField(o, field);
                }
            }

            static NeoFurAsset[] GetAllNFAsInCurrentScene()
            {
                return GameObject.FindObjectsOfType<NeoFurAsset>();
            }

            static int GetAssetIndexFromName(string str)
            {
                string goName = str;
                string numberString = goName.Substring(goName.LastIndexOf("_") + 1);

                int number;

                bool wasParsed = int.TryParse(numberString, out number);

                return wasParsed ? number : -1;
            }

            static void ApplyMaterialProperties(Material mat, NeoFurConfig config)
            {
                Type shaderParameterType = typeof(ShaderParameters);

                FieldInfo[] shaderFields = shaderParameterType.GetFields();
                
                string unityMappedFieldName;

                foreach (FieldInfo field in shaderFields)
                {
                    unityMappedFieldName = field.Name;

                    // handle special cases here until things are standardized between Unity and Unreal versions
                    // check for uv scale
                    // check for color maps since we let the user pick two (one for tip and root each) instead of one for both tip and root

                    if (unityMappedFieldName == "HMPMap")
                        unityMappedFieldName = "HeightMapUND";
                    if (unityMappedFieldName == "GradientMapScatter")
                        unityMappedFieldName = "GradientScatterMapUND";

                    string propertyName = "_" + unityMappedFieldName;

                    if (!mat.HasProperty(propertyName) && !unityMappedFieldName.Contains("_UVScale") &&
                        propertyName != "_ColorMapUND" && propertyName != "_ColorMapOVR")
                    {
                        LogWarning("Trying to set property " + propertyName +
                            " but material doesnt have that property");

                        continue;
                    }

                    // get the base type value from the config field
                    object configValue = field.GetValue(config.Shader);

                    if (configValue == null)
                    {
                        LogWarning("Null config value: " + unityMappedFieldName + " Type: " + field.FieldType);
                        continue;
                    }

                    // set texture properties
                    if (field.FieldType == typeof(string))
                    {
                        string fileName = (string)configValue;

                        if (fileName.StartsWith("./"))
                            fileName = fileName.Substring(fileName.IndexOf("/"));

                        string extension = fileName.Substring(fileName.LastIndexOf(".") + 1).ToLower();

                        // check the file extension
                        if (extension == "tga" || extension == "png" || extension == "jpg")
                        {
                            string assetPath = m_currentFolder + fileName;

                            //get texture
                            Texture2D value = (Texture2D)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));

                            if (value == null)
                            {
                                Debug.LogErrorFormat("Missing Texture: {0}", assetPath);
                                continue;
                            }

                            // convert from Unreal single color map style to Unity double color map style
                            if (propertyName == "_ColorMapOVR" || propertyName == "_ColorMapUND")
                            {
                                string tipString = propertyName.Replace("ColorMap", "ColorTipMap");
                                string rootString = propertyName.Replace("ColorMap", "ColorRootMap");

                                mat.SetTexture(tipString, value);
                                mat.SetTexture(rootString, value);
                            }
                            else
                            {
                                Log("Setting Texture: " + propertyName + " = " + value.name);
                                mat.SetTexture(propertyName, value);

                                if (propertyName == "_ColorTipMapUND" || propertyName == "_ColorTipMapOVR")
                                {
                                    propertyName = propertyName.Replace("Tip", "Root");
                                    Log("Setting Root Color Texture Manually: " + propertyName + " = " + value.name);
                                    mat.SetTexture(propertyName, value);
                                }
                            }
                        }
                    }

                    // set float properties
                    if (field.FieldType == typeof(float) ||
                        field.FieldType == typeof(int))
                    {
                        float value = (float)configValue;
                        Log("Setting Float: " + propertyName + " = " + value);
                        mat.SetFloat(propertyName, value);
                    }

                    // set color properties
                    if (field.FieldType == typeof(Color))
                    {
                        Color value = (Color)configValue;
                        Log("Setting Color: " + propertyName + " = " + value);
                        mat.SetColor(propertyName, value);
                    }

                    if (field.FieldType == typeof(Vector2))
                    {
                        Vector2 value = (Vector2)configValue;

                        if (propertyName.EndsWith("_UVScale"))
                        {
                            string subName = propertyName.Substring(0, propertyName.IndexOf("_UVScale"));

                            if (!mat.HasProperty(subName))
                            {
                                LogWarning("Material is missing Texture UV property: " + subName + ". Not setting texture UV scale.");
                                continue;
                            }

                            Log("Setting Texture Scale: " + subName + " = " + value);
                            mat.SetTextureScale(subName, value);

                            if (subName == "_ColorTipMapUND" || subName == "_ColorTipMapOVR")
                            {
                                subName = subName.Replace("Tip", "Root");
                                Log("Setting Texture Scale Manually: " + subName + " = " + value);
                                mat.SetTextureScale(subName, value);
                            }
                        }
                    }

                    // set vector properties
                    if (field.FieldType == typeof(Vector4))
                    {
                        Vector4 value = (Vector4)configValue;
                        Log("Setting Vector4: " + propertyName + " = " + value);
                        mat.SetVector(propertyName, value);
                    }
                }

                //@HACK @TODO: hardcoding roughness/smoothness
                mat.SetFloat("_RoughnessUND", .25f);
                mat.SetFloat("_RoughnessOVR", .25f);

                MaterialUtils.RebuildMaterialKeywords(mat);

                EditorUtility.SetDirty(mat);
            }

            static void ApplyComponentProperties(Material mat, NeoFurConfig config, NeoFurAsset nfa)
            {
                if (nfa == null) return;

                Log("Assigning material: " + mat.name + " is assigned to " + nfa.gameObject.name);

                nfa.data.furryMat = mat;

                // set component values
                SetFieldValuesToProperties(config.Component, typeof(ComponentParameters), nfa, typeof(NeoFurAsset), true);

                // apply physics stuff
                SetFieldValuesToProperties(config.Component.FurPhysicsParameters, typeof(FurPhysicsParameters), nfa.physParams, typeof(PhysicsStuff));
                
                // make sure the nfa is serialized and marked as needing to be serialized
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            }

            private static void SetFieldValuesToProperties(object source, Type sourceType, object target, Type targetType, bool bConvertUnits = false)
            {
                FieldInfo[] fields = sourceType.GetFields();

                // only get the properties from NeoFurAsset
                PropertyInfo[] properties = targetType.GetProperties();

                Dictionary<string, FieldInfo> tempDictionary = new Dictionary<string, FieldInfo>();

                foreach (PropertyInfo field1 in properties)
                {
                    foreach (FieldInfo field2 in fields)
                    {
                        if (field1.Name.CompareTo(field2.Name) == 0)
                        {
                            Log("Found matching property in config and NFA: " + field2.Name);
                            tempDictionary.Add(field2.Name, field2);
                        }
                    }
                }

                foreach (PropertyInfo prop in properties)
                {
                    if (tempDictionary.ContainsKey(prop.Name))
                    {
                        FieldInfo field = tempDictionary[prop.Name];

                        object value = null;

                        

                        if (prop.PropertyType == field.FieldType)
                        {
                            value = field.GetValue(source);
                        }
                        else if (prop.PropertyType == typeof(int) &&
                            (field.FieldType == typeof(float) || field.FieldType == typeof(double)))
                        {
                            float v = (float)field.GetValue(source);

                            value = (int)v;
                        }
                        else
                        {
                            LogWarning("Incompatible types: " + prop.Name + " prop type = " +
                                prop.PropertyType + " field type = " + field.FieldType);
                            continue;
                        }

                        Log("Setting prop: " + prop.Name + " value = " + value);

                        //set the property value
                        prop.SetValue(target, value, null);
                    }
                }
            }
        }
    }
}
