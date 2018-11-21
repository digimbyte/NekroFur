using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Reflection;
using Neoglyphic.Editor;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    public class NeoMaterialInspector : ShaderGUI
    {
        #region Style Values Used For Inspector

        protected static class Styles
        {
            //und style values
            public static bool bShowUND
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowUND", true); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowUND", value); }
            }
            public static bool bShowColorUND
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowColorUND", true); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowColorUND", value); }
            }
            public static bool bShowShapeUND
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowShapeUND", true); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowShapeUND", value); }
            }
            public static bool bShowDensityUND
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowDensityUND", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowDensityUND", value); }
            }
            public static bool bShowHeightUND
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowHeightUND", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowHeightUND", value); }
            }
            public static bool bShowLightingUND
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowLightingUND", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowLightingUND", value); }
            }

            //ovr style values
            public static bool bShowOVR
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowOVR", true); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowOVR", value); }
            }
            public static bool bShowColorOVR
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowColorOVR", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowColorOVR", value); }
            }
            public static bool bShowShapeOVR
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowShapeOVR", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowShapeOVR", value); }
            }
            public static bool bShowDensityOVR
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowDensityOVR", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowDensityOVR", value); }
            }
            public static bool bShowHeightOVR
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowHeightOVR", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowHeightOVR", value); }
            }
            public static bool bShowLightingOVR
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowLightingOVR", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowLightingOVR", value); }
            }

            //surface style values
            public static bool bShowSurfaceValues
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowSurfaceValues", true); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowSurfaceValues", value); }
            }

            public static bool bShowAOValues
            {
                get { return EditorPrefs.GetBool("NeoFur_MaterialInspector_bShowAOValues", false); }
                set { EditorPrefs.SetBool("NeoFur_MaterialInspector_bShowAOValues", value); }
            }

            //properties
            public static GUIContent GradientMapLabelUND = new GUIContent("Gradient Map UND", "Gradient Map for undercoat strand color");
            public static GUIContent GradientMapLabelOVR = new GUIContent("Gradient Map UND", "Gradient Map for overcoat strand color");
            public static GUIContent GradientScatterMapLabelUND = new GUIContent("Gradient Scatter Map UND");
            public static GUIContent GradientScatterMapLabelOVR = new GUIContent("Gradient Scatter Map OVR");
            public static GUIContent StrandNormalMapLabelUND = new GUIContent("Strand Normal Map UND", "Per strand normal map. Determines normals along the length of the strand");
            public static GUIContent StrandNormalMapLabelOVR = new GUIContent("Strand Normal Map OVR", "Per strand normal map. Determines normals along the length of the strand");
        }

        #endregion

        // add different shader profiles here.
        // used to swap shaders and show/hide
        // certain values and settings
        private enum ShaderProfile
        {
            OPTIMIZED = 0,
            DOUBLESIDED = 1,
            MOBILE = 2,
            COMPLEX = 3,
            COMPLEX_DOUBLESIDED = 4,
        }

        private ShaderProfile shaderProfile = ShaderProfile.OPTIMIZED;

        #region Material Properties
		
        // UND properties
        //color
        MaterialProperty _bColorMapUND;
        MaterialProperty _bGradientMapUND;
        MaterialProperty _bGradientMapRootToTipUND;
        MaterialProperty _GradientMapUND;
        MaterialProperty _ColorRootMapUND;
        MaterialProperty _ColorTipMapUND;
        MaterialProperty _ColorRootUND;
        MaterialProperty _ColorTipUND;
		MaterialProperty _ColorTintRootUND;
		MaterialProperty _ColorTintTipUND;
		MaterialProperty _HueVariationUND;
        MaterialProperty _ValueVariationUND;
        MaterialProperty _ValueVariationMapUND;

        // shape
        MaterialProperty _StrandShapeMapUND;
        MaterialProperty _StrandColorIndexMapUND;
		MaterialProperty _StrandShapeMapOVR;

		//density
		MaterialProperty _bDensityUND;
        MaterialProperty _bDensityMapUND;
        MaterialProperty _DensityMapUND;
        MaterialProperty _DensityUND;
        MaterialProperty _DensityMinUND;
        MaterialProperty _DensityMaxUND;
        MaterialProperty _bOpaqueBaseUND;
		MaterialProperty _BaseLayerHeightThreshold;
		//height
		MaterialProperty _bHeightMapUND;
        MaterialProperty _HMPDepthUND;
        MaterialProperty _HeightMapUND;
        MaterialProperty _StrandLengthMinUND;
        MaterialProperty _StrandLengthMaxUND;
        MaterialProperty _StrandAlphaOffset;

        //lighting
        MaterialProperty _bUseAnimatedDither;
        MaterialProperty _DitherAmoutUND;
        MaterialProperty _GradientScatterMapUND;
        MaterialProperty _RoughnessUND;
        MaterialProperty _SpecularBreakupUND;

        //OVR properties
        MaterialProperty _bOVR;
        //color
        MaterialProperty _bColorOVR;
        MaterialProperty _bColorMapOVR;
        MaterialProperty _bGradientMapOVR;
        MaterialProperty _bGradientMapRootToTipOVR;
        MaterialProperty _GradientMapOVR;
        MaterialProperty _ColorRootMapOVR;
        MaterialProperty _ColorTipMapOVR;
        MaterialProperty _ColorRootOVR;
        MaterialProperty _ColorTipOVR;
		MaterialProperty _ColorTintRootOVR;
		MaterialProperty _ColorTintTipOVR;

		//shape
		MaterialProperty _bAdjustPatternCurve;
        MaterialProperty _PatternCurvePower;

        //density
        MaterialProperty _bDensityOVR;
        MaterialProperty _bDensityMapOVR;
        MaterialProperty _DensityMapOVR;
        MaterialProperty _DensityOVR;
        MaterialProperty _DensityMinOVR;
        MaterialProperty _DensityMaxOVR;

        //height
        MaterialProperty _bHeightMapOVR;
        MaterialProperty _StrandLengthMinOVR;
        MaterialProperty _StrandLengthMaxOVR;

        //lighting
        MaterialProperty _GradientScatterMapOVR;
        MaterialProperty _RoughnessOVR;
        MaterialProperty _ScatterOVR;
        MaterialProperty _SpecularBreakupOVR;

        // UND + OVR properties

        //ao
        MaterialProperty _AOValue;
        MaterialProperty _AOPattern;
        MaterialProperty _AOPatternDarkness;

        MaterialProperty _EmissionColor;
        MaterialProperty _Smoothness;
		MaterialProperty _Metallic;
		MaterialProperty _RimBrightness;
		MaterialProperty _RimCenter;
		MaterialProperty _RimContrast;

		MaterialProperty _CullMode;
		#endregion

		SerializedProperty m_EnableInstancingVariants;

		private bool isMobileProfile
        {
            get { return shaderProfile == ShaderProfile.MOBILE; }
        }

        private bool isComplexProfile
        {
            get { return shaderProfile == ShaderProfile.COMPLEX || shaderProfile == ShaderProfile.COMPLEX_DOUBLESIDED; }
        }

        private bool isOptimizedProfile
        {
            get { return shaderProfile == ShaderProfile.OPTIMIZED || shaderProfile == ShaderProfile.DOUBLESIDED; }
        }

        private void FindStuff(MaterialEditor editor, MaterialProperty[] props)
        {
			m_EnableInstancingVariants = editor.serializedObject.FindProperty("m_EnableInstancingVariants");
			
			// UND properties
			if (isComplexProfile) // complex profile
                FindComplexStuff(props);
            else if (isOptimizedProfile) // optimzed profile
                FindOptimizedStuff(props);
            else if (isMobileProfile) // mobile profile
                FindMobileStuff(props);

			//TODO: Move all common properties here instead of having duplicate code.
			_CullMode = FindProperty("_CullMode", props);

		}

        private void FindComplexStuff(MaterialProperty[] props)
        {
            // und color
            _bColorMapUND = FindProperty("_bColorMapUND", props);
            _ColorTipMapUND = FindProperty("_ColorTipMapUND", props);
            _ColorRootMapUND = FindProperty("_ColorRootMapUND", props);
            _ColorRootUND = FindProperty("_ColorRootUND", props);
            _ColorTipUND = FindProperty("_ColorTipUND", props);
            _ColorTintRootUND = FindProperty("_ColorTintRootUND", props);
            _ColorTintTipUND = FindProperty("_ColorTintTipUND", props);

            _bGradientMapUND = FindProperty("_bGradientMapUND", props);
            _bGradientMapRootToTipUND = FindProperty("_bGradientMapRootToTipUND", props);
            _GradientMapUND = FindProperty("_GradientMapUND", props);
            _HueVariationUND = FindProperty("_HueVariationUND", props);
            _ValueVariationUND = FindProperty("_ValueVariationUND", props);
            _ValueVariationMapUND = FindProperty("_ValueVariationMapUND", props);

			// und shape
			_StrandShapeMapUND = FindProperty("_StrandShapeMapUND", props);
			_StrandShapeMapOVR = FindProperty("_StrandShapeMapOVR", props);
			_StrandColorIndexMapUND = FindProperty("_StrandColorIndexMapUND", props);

			// und density
			_bDensityUND = FindProperty("_bDensityUND", props);
            _DensityMapUND = FindProperty("_DensityMapUND", props);
            _DensityMinUND = FindProperty("_DensityMinUND", props);
            _DensityMaxUND = FindProperty("_DensityMaxUND", props);
            _bDensityMapUND = FindProperty("_bDensityMapUND", props);
            _DensityUND = FindProperty("_DensityUND", props);
            _bOpaqueBaseUND = FindProperty("_bOpaqueBaseUND", props);
			_BaseLayerHeightThreshold = FindProperty("_BaseLayerHeightThreshold", props);

			// und height
			_bHeightMapUND = FindProperty("_bHeightMapUND", props);
            _HeightMapUND = FindProperty("_HeightMapUND", props);
            _HMPDepthUND = FindProperty("_HMPDepthUND", props);
            _StrandLengthMinUND = FindProperty("_StrandLengthMinUND", props);
            _StrandLengthMaxUND = FindProperty("_StrandLengthMaxUND", props);

            // und lighting
            _bUseAnimatedDither = FindProperty("_bUseAnimatedDither", props);
            _DitherAmoutUND = FindProperty("_DitherAmount", props);
            _GradientScatterMapUND = FindProperty("_GradientScatterMapUND", props);
            _RoughnessUND = FindProperty("_RoughnessUND", props);
            _SpecularBreakupUND = FindProperty("_SpecularBreakupUND", props);

            // OVR properties
            _bOVR = FindProperty("_bOVR", props);
            // OVR color
            _bColorOVR = FindProperty("_bColorOVR", props);
            _bColorMapOVR = FindProperty("_bColorMapOVR", props);
            _bGradientMapOVR = FindProperty("_bGradientMapOVR", props);
            _bGradientMapRootToTipOVR = FindProperty("_bGradientMapRootToTipOVR", props);
            _GradientMapOVR = FindProperty("_GradientMapOVR", props);
            _ColorRootMapOVR = FindProperty("_ColorRootMapOVR", props);
            _ColorTipMapOVR = FindProperty("_ColorTipMapOVR", props);
            _ColorRootOVR = FindProperty("_ColorRootOVR", props);
            _ColorTipOVR = FindProperty("_ColorTipOVR", props);
			_ColorTintRootOVR = FindProperty("_ColorTintRootOVR", props);
			_ColorTintTipOVR = FindProperty("_ColorTintTipOVR", props);
			
            // OVR density
            _bDensityOVR = FindProperty("_bDensityOVR", props);
            _bDensityMapOVR = FindProperty("_bDensityMapOVR", props);
            _DensityMapOVR = FindProperty("_DensityMapOVR", props);
            _DensityOVR = FindProperty("_DensityOVR", props);
            _DensityMinOVR = FindProperty("_DensityMinOVR", props);
            _DensityMaxOVR = FindProperty("_DensityMaxOVR", props);

            // OVR height
            _bHeightMapOVR = FindProperty("_bHeightMapOVR", props);
            _StrandLengthMinOVR = FindProperty("_StrandLengthMinOVR", props);
            _StrandLengthMaxOVR = FindProperty("_StrandLengthMaxOVR", props);

            // OVR lighting
            _GradientScatterMapOVR = FindProperty("_GradientScatterMapOVR", props);
            _RoughnessOVR = FindProperty("_RoughnessOVR", props);
            _ScatterOVR = FindProperty("_ScatterOVR", props);
            _SpecularBreakupOVR = FindProperty("_SpecularBreakupOVR", props);

            // surface values
            _EmissionColor = FindProperty("_EmissionColor", props);
            _AOValue = FindProperty("_AOValue", props);
        }

        private void FindOptimizedStuff(MaterialProperty[] props)
        {
            // color
            _bColorMapUND = FindProperty("_bColorMapUND", props);
            _ColorTipMapUND = FindProperty("_ColorTipMapUND", props);
            _ColorRootMapUND = FindProperty("_ColorRootMapUND", props);
            _ColorRootUND = FindProperty("_ColorRootUND", props);
            _ColorTipUND = FindProperty("_ColorTipUND", props);
            _ColorTintRootUND = FindProperty("_ColorTintRootUND", props);
            _ColorTintTipUND = FindProperty("_ColorTintTipUND", props);

            // und shape
            _StrandShapeMapUND = FindProperty("_StrandShapeMapUND", props);
            _bAdjustPatternCurve = FindProperty("_bAdjustPatternCurve", props);
			_PatternCurvePower = FindProperty("_PatternCurvePower", props);

            // height
            _bHeightMapUND = FindProperty("_bHeightMapUND", props);
            _HeightMapUND = FindProperty("_HeightMapUND", props);
            _StrandLengthMinUND = FindProperty("_StrandLengthMinUND", props);
            _StrandLengthMaxUND = FindProperty("_StrandLengthMaxUND", props);

            //surface AO values
            _AOValue = FindProperty("_AOValue", props);
            _AOPattern = FindProperty("_AOPattern", props);
            _AOPatternDarkness = FindProperty("_AOPatternDarkness", props);

            // surface lighting
            _Smoothness = FindProperty("_Smoothness", props);
            _Metallic = FindProperty("_Metallic", props);

			_RimBrightness = FindProperty("_RimBrightness", props);
			_RimCenter = FindProperty("_RimCenter", props);
			_RimContrast = FindProperty("_RimContrast", props);
		}

        private void FindMobileStuff(MaterialProperty[] props)
        {
            // color
            _ColorTipMapUND = FindProperty("_ColorTipMapUND", props);

            // und shape
            _StrandShapeMapUND = FindProperty("_StrandShapeMapUND", props);

			// height
			_HeightMapUND = FindProperty("_HeightMapUND", props);
            _StrandLengthMinUND = FindProperty("_StrandLengthMinUND", props);
            _StrandLengthMaxUND = FindProperty("_StrandLengthMaxUND", props);
            _StrandAlphaOffset = FindProperty("_StrandAlphaOffset", props);

            // surface lighting
            _Smoothness = FindProperty("_Smoothness", props);
            _Metallic = FindProperty("_Metallic", props);
            _AOPattern = FindProperty("_AOPattern", props);

			_RimBrightness = FindProperty("_RimBrightness", props);
			_RimCenter = FindProperty("_RimCenter", props);
			_RimContrast = FindProperty("_RimContrast", props);
		}

        private void DrawShaderProfileDropdown(MaterialEditor editor)
        {
            Material mat = editor.target as Material;

            {
                // TODO: need to change this so it checks keywords instead of file names
                bool complex = mat.shader.name.Contains("Complex");
                bool mobile = mat.shader.name.Contains("Mobile");

                shaderProfile = ShaderProfile.OPTIMIZED;
                if (complex) shaderProfile = ShaderProfile.COMPLEX;
                if (mobile) shaderProfile = ShaderProfile.MOBILE;
            }
        }

		void SetMaterialKeyword(string keyword, bool state, Material material)
        {
            if (state) material.EnableKeyword(keyword);
            else material.DisableKeyword(keyword);
        }

        bool GetMaterialKeyword(string keyword, Material material)
        {
            return material.IsKeywordEnabled(keyword);
        }

        private bool DoToggleProperty(string label, MaterialProperty property, MaterialEditor editor, string tooltip = null)
        {
			editor.ShaderProperty(property, label);
			return property.floatValue >= 0.5f;
        }

        private void DoRangeProperty(string label, MaterialProperty property, MaterialEditor editor)
        {
			editor.ShaderProperty(property, label);
        }

		void DoEnumProperty<T>(string label, MaterialProperty property, MaterialEditor editor)
		{
			System.Type enumType = typeof(T);
			string[] enumNames = System.Enum.GetNames(enumType);

			EditorGUI.showMixedValue = property.hasMixedValue;
			int mode = (int)property.floatValue;

			EditorGUI.BeginChangeCheck();
			mode = EditorGUILayout.Popup(label, mode, enumNames);
			if (EditorGUI.EndChangeCheck())
			{
				editor.RegisterPropertyChangeUndo(label);
				property.floatValue = mode;
			}

			EditorGUI.showMixedValue = false;
		}

		// Draw UND Color GUI
		private void DrawColorUND(string label, MaterialEditor editor)
        {
            Styles.bShowColorUND = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowColorUND);

            if (!Styles.bShowColorUND) return;

            if (isComplexProfile)
            {
                bool bColorMap = DoToggleProperty("Enable Color Map", _bColorMapUND, editor);

                if (bColorMap) // color maps
                {
                    NeoGUIHelper.PushIndent();

                    bool bGradientMap = DoToggleProperty("Enable Gradient Map UND", _bGradientMapUND, editor);

                    if (bGradientMap)
                    {
                        NeoGUIHelper.PushIndent();

                        DoToggleProperty("Color Gradient Root To Tip", _bGradientMapRootToTipUND, editor);

                        //gradient map
                        editor.TexturePropertySingleLine(Styles.GradientMapLabelUND, _GradientMapUND);

                        NeoGUIHelper.PopIndent();
                    }
                    else
                    {
                        // root and tip color maps
                        editor.TextureProperty(_ColorRootMapUND, "Color Root Map UND");
                        editor.TextureProperty(_ColorTipMapUND, "Color Tip Map UND");
                    }

                    //Textures can be tinted now.
                    editor.ShaderProperty(_ColorTintRootUND, "Root Tint");
                    editor.ShaderProperty(_ColorTintTipUND, "Tip Tint");

                    NeoGUIHelper.PopIndent();
                }
                else // color values
                {
                    editor.ShaderProperty(_ColorRootUND, "Root Color");
                    editor.ShaderProperty(_ColorTipUND, "Tip Color");
                }

                EditorGUILayout.Space();
                // hue and value
                DoRangeProperty("Hue Variation", _HueVariationUND, editor);
                DoRangeProperty("Value Variation", _ValueVariationUND, editor);
            }
            else if (isOptimizedProfile)
            {
                bool bColorMapUND = DoToggleProperty("Use Color Maps", _bColorMapUND, editor);
                
                if(bColorMapUND)
                {
                    editor.TextureProperty(_ColorRootMapUND, "Root Color Map UND");
                    editor.ColorProperty(_ColorTintRootUND, "Root Tint Color UND");

                    editor.TextureProperty(_ColorTipMapUND, "Tip Color Map UND");
                    editor.ColorProperty(_ColorTintTipUND, "Tip Tint Color UND");
                }
                else
                {
                    editor.ColorProperty(_ColorRootUND, "Root Color UND");
                    editor.ColorProperty(_ColorTipUND, "Tip Color UND");
                }
            }
            else if (isMobileProfile)
            {
                editor.TextureProperty(_ColorTipMapUND, "Color Map");
            }
        }

        // Draw UND Shape GUI
        private void DrawShapeUND(string label, MaterialEditor editor)
        {
            Styles.bShowShapeUND = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowShapeUND);

            if (!Styles.bShowShapeUND) return;

            if (isComplexProfile)
            {
                editor.TextureProperty(_StrandShapeMapUND, "Strand Shape Map UND", false);
                editor.TextureProperty(_StrandColorIndexMapUND, "Strand Index Map UND", false);
                editor.TextureScaleOffsetProperty(_StrandShapeMapUND);
            }
            else
            {
                editor.TextureProperty(_StrandShapeMapUND, "Strand Shape Map UND");
            }

            if (isOptimizedProfile)
            {
				if (_StrandShapeMapUND.textureValue)
				{
					bool showCurve = DoToggleProperty("Enable curve adjustments for strands", _bAdjustPatternCurve, editor);

					if (showCurve)
					{
						DoRangeProperty("Pattern curve power", _PatternCurvePower, editor);
					}
				}
            }
        }

        // Draw UND Density GUI
        private void DrawDensityUND(string label, MaterialEditor editor)
        {
            Styles.bShowDensityUND = NeoGUIHelper.MaterialHeaderWithToggle(_bDensityUND, editor, label, Styles.bShowDensityUND);

            if (!Styles.bShowDensityUND) return;

            EditorGUI.BeginDisabledGroup(_bDensityUND.floatValue == 0.0f);

            if (isComplexProfile)
            {
                bool bDensityMap = DoToggleProperty("Enable Density Map UND", _bDensityMapUND, editor);

                if (bDensityMap)
                {
                    NeoGUIHelper.PushIndent();
                    editor.TextureProperty(_DensityMapUND, "Density Map UND");
					DoRangeProperty("Min Density of strands UND", _DensityMinUND, editor);
					DoRangeProperty("Max Density of strands UND", _DensityMaxUND, editor);
					NeoGUIHelper.PopIndent();
                }
                else
                {
                    DoRangeProperty("Density of strands UND", _DensityUND, editor);
                }
            }
            else if (isOptimizedProfile)
            {
                editor.TextureProperty(_DensityMapUND, "Density Map UND");
                DoRangeProperty("Min Density of strands UND", _DensityMinUND, editor);
                DoRangeProperty("Max Density of strands UND", _DensityMaxUND, editor);
            }

            EditorGUI.EndDisabledGroup();
        }

        // Draw UND Height GUI
        private void DrawHeightUND(string label, MaterialEditor editor)
        {
            Styles.bShowHeightUND = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowHeightUND);

            if (!Styles.bShowHeightUND) return;

            if (!isMobileProfile)
            {
                if (isComplexProfile)
                {
                    bool bOpaque = DoToggleProperty("Make base layer opaque", _bOpaqueBaseUND, editor);
                    if (bOpaque && _bHeightMapUND.floatValue >= 0.5f)
                    {
                        NeoGUIHelper.PushIndent();
                        DoRangeProperty("Base Layer Height Threshold", _BaseLayerHeightThreshold, editor);
                        NeoGUIHelper.PopIndent();
                    }
                }

                DoRangeProperty("Min Strand Length/Height", _StrandLengthMinUND, editor);
                DoRangeProperty("Max Strand Length/Height", _StrandLengthMaxUND, editor);

                bool bHeightMap = DoToggleProperty("Enable Height Map", _bHeightMapUND, editor);

                if (!bHeightMap) return;

                NeoGUIHelper.PushIndent();

                if (isComplexProfile)
                    DoRangeProperty("Height Map Influence", _HMPDepthUND, editor);
            }
            else if (isMobileProfile)
            {
                DoRangeProperty("Strand base layer offset", _StrandAlphaOffset, editor);
            }

            editor.TextureProperty(_HeightMapUND, "Height Map");

            if (!isMobileProfile)
                NeoGUIHelper.PopIndent();
        }

        // Draw UND Lighting GUI
        private void DrawLightingUND(string label, MaterialEditor editor)
        {
            Styles.bShowLightingUND = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowLightingUND);

            if (!Styles.bShowLightingUND) return;

            bool useDither = DoToggleProperty("Use animated dither", _bUseAnimatedDither, editor);

            if (useDither)
            {
                NeoGUIHelper.PushIndent();

                DoRangeProperty("Dither amount", _DitherAmoutUND, editor);

                NeoGUIHelper.PopIndent();
            }

            editor.TexturePropertySingleLine(Styles.GradientScatterMapLabelUND, _GradientScatterMapUND);
            DoRangeProperty("Smoothness UND", _RoughnessUND, editor);
            DoRangeProperty("Specular reflections breakup UND", _SpecularBreakupUND, editor);
        }

        // Draw UND GUI
        private void DrawUndercoat(MaterialEditor editor)
        {
            NeoGUIHelper.PushTextColor(Color.white);
            NeoGUIHelper.PushBGColor(NeoGUIHelper.Colors.LightGrey);

            Styles.bShowUND = NeoGUIHelper.HeaderWithFoldout("1. undercoat (und)", Styles.bShowUND);

            NeoGUIHelper.PopTextColor();
            NeoGUIHelper.PopBGColor();

            if (!Styles.bShowUND) return;

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            NeoGUIHelper.PushIndent();
            {
                EditorGUILayout.Space();
                DrawColorUND("1.1 UND COLOR", editor);
                EditorGUILayout.Space();
                DrawShapeUND("1.2 UND SHAPE", editor);
                if (isComplexProfile)
                {
                    EditorGUILayout.Space();
                    DrawDensityUND("1.3 UND DENSITY", editor);
                }
                EditorGUILayout.Space();
                DrawHeightUND("1.4 UND HEIGHT", editor);
                if (isComplexProfile)
                {
                    EditorGUILayout.Space();
                    DrawLightingUND("1.5 UND LIGHTING", editor);
                }
            }
            NeoGUIHelper.PopIndent();

            EditorGUI.indentLevel = prevIndent;
        }

        // Draw OVR Color GUI
        private void DrawColorOVR(string label, MaterialEditor editor)
        {
            Styles.bShowColorOVR = NeoGUIHelper.MaterialHeaderWithToggle(_bColorOVR, editor, label, Styles.bShowColorOVR);

            if (!Styles.bShowColorOVR) return;

            EditorGUI.BeginDisabledGroup(_bOVR.floatValue == 0.0f || _bColorOVR.floatValue == 0.0f);

            bool bColorMap = DoToggleProperty("Enable Color Map", _bColorMapOVR, editor);

            if (bColorMap) // color maps
            {
                NeoGUIHelper.PushIndent();

                bool bGradientMap = DoToggleProperty("Enable Gradient Map OVR", _bGradientMapOVR, editor);

                if (bGradientMap)
                {
                    NeoGUIHelper.PushIndent();

                    DoToggleProperty("Color Gradient Root To Tip", _bGradientMapRootToTipOVR, editor);

                    //gradient map
                    editor.TexturePropertySingleLine(Styles.GradientMapLabelOVR, _GradientMapOVR);

                    NeoGUIHelper.PopIndent();
                }
                else
                {
                    // root and tip color maps
                    editor.TextureProperty(_ColorRootMapOVR, "Color Root Map OVR");
                    editor.TextureProperty(_ColorTipMapOVR, "Color Tip Map OVR");
                }

				editor.ShaderProperty(_ColorTintRootOVR, "Root Tint");
				editor.ShaderProperty(_ColorTintTipOVR, "Tip Tint");

                NeoGUIHelper.PopIndent();
            }
            else // color values
            {
				editor.ShaderProperty(_ColorRootOVR, "Root Color");
				editor.ShaderProperty(_ColorTipOVR, "Tip Color");
            }
            EditorGUI.EndDisabledGroup();
        }

        // Draw OVR Shape GUI
        private void DrawShapeOVR(string label, MaterialEditor editor)
        {
            Styles.bShowShapeOVR = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowShapeOVR);

            if (!Styles.bShowShapeOVR) return;
			editor.TextureScaleOffsetProperty(_StrandShapeMapOVR);
		}

        // Draw OVR Density GUI
        private void DrawDensityOVR(string label, MaterialEditor editor)
        {
            Styles.bShowDensityOVR = NeoGUIHelper.MaterialHeaderWithToggle(_bDensityOVR, editor, label, Styles.bShowDensityOVR);

            if (!Styles.bShowDensityOVR) return;

            EditorGUI.BeginDisabledGroup(_bOVR.floatValue == 0.0f || _bDensityOVR.floatValue == 0.0f);

            bool bDensityMap = DoToggleProperty("Enable Density Map OVR", _bDensityMapOVR, editor);

            if (bDensityMap)
            {
                NeoGUIHelper.PushIndent();
                editor.TextureProperty(_DensityMapOVR, "Density Map OVR");
				DoRangeProperty("Min Density of strands OVR", _DensityMinOVR, editor);
				DoRangeProperty("Max Density of strands OVR", _DensityMaxOVR, editor);
				NeoGUIHelper.PopIndent();
            }
            else
            {
                DoRangeProperty("Density of strands OVR", _DensityOVR, editor);
            }
            EditorGUI.EndDisabledGroup();
        }

        // Draw OVR Height GUI
        private void DrawHeightOVR(string label, MaterialEditor editor)
        {
            Styles.bShowHeightOVR = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowHeightOVR);

            if (!Styles.bShowHeightOVR) return;

            DoToggleProperty("Enable use of UND Height Map", _bHeightMapOVR, editor);
            DoRangeProperty("Min Strand Length/Height", _StrandLengthMinOVR, editor);
            DoRangeProperty("Max Strand Length/Height", _StrandLengthMaxOVR, editor);
        }

        // Draw OVR Lighting GUI
        private void DrawLightingOVR(string label, MaterialEditor editor)
        {
            Styles.bShowLightingOVR = NeoGUIHelper.HeaderWithFoldout(label, Styles.bShowLightingOVR);

            if (!Styles.bShowLightingOVR) return;

            editor.TexturePropertySingleLine(Styles.GradientScatterMapLabelOVR, _GradientScatterMapOVR);
            DoRangeProperty("Smoothness OVR", _RoughnessOVR, editor);
            DoRangeProperty("Scatter OVR", _ScatterOVR, editor);
            DoRangeProperty("Specular reflections breakup OVR", _SpecularBreakupOVR, editor);
        }

        // Draw OVR GUI
        private void DrawOvercoat(MaterialEditor editor)
        {
            NeoGUIHelper.PushTextColor(Color.white);
            NeoGUIHelper.PushBGColor(NeoGUIHelper.Colors.LightGrey);

            Styles.bShowOVR = NeoGUIHelper.MaterialHeaderWithToggle(_bOVR, editor, "2. overcoat (OVR)", Styles.bShowOVR);

            NeoGUIHelper.PopTextColor();
            NeoGUIHelper.PopBGColor();

            if (!Styles.bShowOVR) return;

            EditorGUI.BeginDisabledGroup(_bOVR.floatValue == 0.0f);

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            NeoGUIHelper.PushIndent();
            {
                EditorGUILayout.Space();
                DrawColorOVR("2.1 OVR COLOR", editor);
                EditorGUILayout.Space();
                DrawShapeOVR("2.2 OVR SHAPE", editor);
                EditorGUILayout.Space();
                DrawDensityOVR("2.3 OVR DENSITY", editor);
                EditorGUILayout.Space();
                DrawHeightOVR("2.4 OVR HEIGHT", editor);
                EditorGUILayout.Space();
                DrawLightingOVR("2.5 OVR LIGHTING", editor);
            }
            NeoGUIHelper.PopIndent();

            EditorGUI.indentLevel = prevIndent;

            EditorGUI.EndDisabledGroup();
        }

        private void DrawAOValues(MaterialEditor editor)
        {
            if (isComplexProfile)
            {
                DoRangeProperty("Overall AO", _AOValue, editor);
                return;
            }

            if (!isMobileProfile)
            {
                Styles.bShowAOValues = NeoGUIHelper.HeaderWithFoldout("Ambient Occlusion", Styles.bShowAOValues);

                if (!Styles.bShowAOValues) return;
            }

            if (!isMobileProfile)
                NeoGUIHelper.PushIndent();

            if (isOptimizedProfile)
            {
                DoRangeProperty("AO Occlusion", _AOValue, editor);
            }

            DoRangeProperty("AO Pattern", _AOPattern, editor);

            if (isOptimizedProfile)
            {
                DoRangeProperty("AO Pattern Darkness", _AOPatternDarkness, editor);
            }

            if (!isMobileProfile)
                NeoGUIHelper.PopIndent();
        }

        private void DrawSurfaceValues(MaterialEditor editor)
        {
            NeoGUIHelper.PushTextColor(Color.white);
            NeoGUIHelper.PushBGColor(NeoGUIHelper.Colors.LightGrey);

            if (isComplexProfile)
                Styles.bShowSurfaceValues = NeoGUIHelper.HeaderWithFoldout("3. SURFACE VALUES", Styles.bShowSurfaceValues);
            else
                Styles.bShowSurfaceValues = NeoGUIHelper.HeaderWithFoldout("2. SURFACE VALUES", Styles.bShowSurfaceValues);

            NeoGUIHelper.PopTextColor();
            NeoGUIHelper.PopBGColor();

            if (!Styles.bShowSurfaceValues) return;

            NeoGUIHelper.PushIndent();

            //if (!isComplexProfile)
            {
                EditorGUILayout.Space();

                DrawAOValues(editor);

                EditorGUILayout.Space();
            }

            if (isComplexProfile)
            {
                _EmissionColor.colorValue = editor.ColorProperty(_EmissionColor, "Emissive Color");
            }
            else
            {
                DoRangeProperty("Smoothness", _Smoothness, editor);
				DoRangeProperty("Metallicness", _Metallic, editor);
				DoRangeProperty("Rim Brightness", _RimBrightness, editor);
				DoRangeProperty("Rim Center", _RimCenter, editor);
				DoRangeProperty("Rim Contrast", _RimContrast, editor);
			}

			DoEnumProperty<CullMode>("Cull Mode", _CullMode, editor);

			NeoGUIHelper.PopIndent();
        }

        private void DrawGUI(MaterialEditor editor, MaterialProperty[] props)
        {
            DrawShaderProfileDropdown(editor);

            FindStuff(editor, props);

            EditorGUILayout.Space();

            DrawUndercoat(editor);

            EditorGUILayout.Space();

            if (isComplexProfile)
            {
                DrawOvercoat(editor);

                EditorGUILayout.Space();
            }

            DrawSurfaceValues(editor);
		}

		private void UpdateKeywords(Material material)
		{
			if (isComplexProfile)
			{
				SetMaterialKeyword("COLOR_MAPS_UND_ON", material.GetFloat("_bColorMapUND") >= 0.5f, material);
				SetMaterialKeyword("NEOFUR_COMPLEX_OVR_ON", material.GetFloat("_bOVR") >= 0.5f, material);
				SetMaterialKeyword("NEOFUR_COMPLEX_DENSITY_ON", material.GetFloat("_bDensityUND") >= 0.5f, material);
			}
			else if (isOptimizedProfile)
			{
				SetMaterialKeyword("COLOR_MAPS_UND_ON", (_bColorMapUND.floatValue >= 0.5f), material);
			}
			
			if (isComplexProfile || isOptimizedProfile)
			{
				SetMaterialKeyword("HEIGHT_MAPS_UND_ON", material.GetFloat("_bHeightMapUND") >= 0.5f, material);
			}
		}

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
			EditorGUIUtility.labelWidth = 0f;

			EditorGUI.BeginChangeCheck();
			DrawGUI(materialEditor, properties);

			if (m_EnableInstancingVariants != null)
			{
				m_EnableInstancingVariants.boolValue = true;
			}

			if (EditorGUI.EndChangeCheck())
			{
				foreach (var obj in materialEditor.targets)
				{
					UpdateKeywords((Material)obj);
				}
			}
		}
    }
}
