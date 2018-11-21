using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Reflection;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    public class NeoFurValidator
    {
        [InitializeOnLoadMethod]
        static void TryToValidate()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            bool success = false;

            for (var i = 0; i < assemblies.Length; ++i)
            {
                if (assemblies[i].FullName.Contains("NeoFur")) success = true;
            }

            if (!success)
                CreateDialogWindow();
        }

        // create the popup
        static void CreateDialogWindow()
        {
            string msg = "Hello!\n\nThanks for downloading one of our NeoFur Asset Packs!\n\nIt seems you are missing the NeoFur Plugin that is required in order to use these assets. Please purchase and then import your copy of NeoFur into this project.\n\nBest,\nThe Neoglyphic Team";

            EditorUtility.DisplayDialog("NeoFur Validation", msg, "Close");
        }
    }
}
