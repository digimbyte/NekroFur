using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin.Utils
{
    internal class InvertMaterialHMPDepth
    {
        //[MenuItem("Assets/NeoFur/(Experimental)/InvertHMPDepth")]
        static void InvertHMPDepth()
        {
            List<Material> mats2Change = new List<Material>();
            
            if ((Selection.assetGUIDs == null || Selection.assetGUIDs.Length == 0)
                || (Selection.assetGUIDs.Length == 1 && !AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]).Contains(".")))
            {
                Debug.Log("Inverting all neofur complex mats");

                string[] assetPaths = AssetDatabase.GetAllAssetPaths();

                for (int i = 0; i < assetPaths.Length; ++i)
                {
                    string assetPath = assetPaths[i];

                    bool cancel =
                        EditorUtility.DisplayCancelableProgressBar("Getting Mats From Database...",
                        assetPath,
                        (float)i / (float)assetPaths.Length);

                    if (cancel)
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    if (assetPath.EndsWith(".mat"))
                    {
                        Material asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material)) as Material;
                        mats2Change.Add(asset);
                    }
                }
            }
            else
            {
                for(int i = 0; i < Selection.assetGUIDs.Length; ++i)
                {
                    bool cancel =
                        EditorUtility.DisplayCancelableProgressBar("Getting Mats From Selection...",
                        Selection.assetGUIDs[i],
                        (float)i / (float)Selection.assetGUIDs.Length);

                    if (cancel)
                    {
                        EditorUtility.ClearProgressBar();
                        return;
                    }

                    string assetGUID = Selection.assetGUIDs[i];
                    string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
                    
                    if (assetPath.EndsWith(".mat"))
                        mats2Change.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material)) as Material);
                }
            }

            for(int i = 0; i < mats2Change.Count; ++i)
            {
                Material asset = mats2Change[i];

                bool cancel =
                    EditorUtility.DisplayCancelableProgressBar("Inverting HMP Values...",
                    asset.name,
                    (float)i / (float)mats2Change.Count);

                if (cancel)
                {
                    EditorUtility.ClearProgressBar();
                    return;
                }

                if (asset.HasProperty("_HMPDepthUND"))
                {
                    float hmpDepth = asset.GetFloat("_HMPDepthUND");
                    asset.SetFloat("_HMPDepthUND", 1 - hmpDepth);
                    EditorUtility.SetDirty(asset);
                }
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
