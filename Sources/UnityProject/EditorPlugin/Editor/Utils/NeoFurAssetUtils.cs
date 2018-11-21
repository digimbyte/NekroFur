using UnityEngine;
using UnityEditor;
using System.IO;
using NeoFurUnityPlugin;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    internal partial class NeoFurUtils
    {
        public static string GetUniqueAssetPathNameInCurrentFolder(string fileName)
        {
            string path;

            System.Type assetDatabaseType = typeof(AssetDatabase);
            path = (string)assetDatabaseType.GetMethod("GetUniquePathNameAtSelectedPath",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(assetDatabaseType, new object[] { fileName });

            return path;
        }

        public static string GetCurrentProjectFolder()
        {
            string path = GetUniqueAssetPathNameInCurrentFolder("dummy");
            path = path.Substring(0, path.LastIndexOf('/'));
            return path;
        }

        static string ReadFile(string path)
        {
            string contents = "";

            if (File.Exists(path))
            {
                contents = File.ReadAllText(path);
            }

            return contents;
        }

        static NeoFurAsset[] GetNeoFurAssetsInCurrentScene()
        {
            return GameObject.FindObjectsOfType<NeoFurAsset>();
        }
    }
}
