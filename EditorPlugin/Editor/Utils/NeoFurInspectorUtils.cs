using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    public static class NeoMaterialInspectorUtils
    {
        private static string currentMaterialName = "NO_NEOFUR_MATERAIL";
        private static Dictionary<string, string> _heirarchy = new Dictionary<string, string>();

        public static void DrawProperty(MaterialEditor editor, MaterialProperty prop)
        {
            switch (prop.type)
            {
                case MaterialProperty.PropType.Color:
                    prop.colorValue = editor.ColorProperty(prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Float:
                    prop.floatValue = editor.FloatProperty(prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Range:
                    prop.floatValue = editor.RangeProperty(prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Texture:
                    prop.textureValue = editor.TextureProperty(prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Vector:
                    prop.vectorValue = editor.VectorProperty(prop, prop.displayName);
                    break;
                default:
                    break;
            }
        }

        public static void DrawProperty(Rect rect, MaterialProperty prop, string label, MaterialEditor editor)
        {
            switch (prop.type)
            {
                case MaterialProperty.PropType.Color:
                    prop.colorValue = editor.ColorProperty(rect, prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Float:
                    prop.floatValue = editor.FloatProperty(rect, prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Range:
                    prop.floatValue = editor.RangeProperty(rect, prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Texture:
                    rect.height *= 4;
                    prop.textureValue = editor.TextureProperty(rect, prop, prop.displayName);
                    break;
                case MaterialProperty.PropType.Vector:
                    prop.vectorValue = editor.VectorProperty(rect, prop, prop.displayName);
                    break;
                default:
                    break;
            }
        }

        public static bool GetParentMaterialValue(string valueName, MaterialEditor editor)
        {
            Material mat = editor.target as Material;

            return mat.HasProperty(valueName) && mat.GetFloat(valueName) == 1;
        }

        public static void AddHierarchyName(string materialName, string name, string parent)
        {
            if (currentMaterialName != materialName)
            {
                clearHierarchy();

                currentMaterialName = materialName;
            }

            if (!_heirarchy.ContainsKey(name))
                _heirarchy.Add(name, parent);
        }

        public static bool ShouldShowProperty(string name, MaterialEditor editor)
        {
            if (!_heirarchy.ContainsKey(name)) return true;

            if (_heirarchy.ContainsKey(name))
            {

            }

            return ShouldShowProperty(_heirarchy[name], editor);
        }

        private static string getParentInHierarchy(string name)
        {
            if (_heirarchy.ContainsKey(name))
                return _heirarchy[name];

            return null;
        }

        private static void clearHierarchy()
        {
            _heirarchy.Clear();

            Debug.Log("Clearing material property styling hierarchy thingy");
        }
    }
}
