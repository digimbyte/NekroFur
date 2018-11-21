using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Res = EditorPlugin.Properties.Resources;
using Neoglyphic.Editor;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	[CustomEditor(typeof(NeoFurAsset))]
	public class NeoAssetInspector : Editor
	{
		MaterialEditor	mMatEdit;

		//foldout open/closed states
		public static bool bPreProcessFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bPreProcessFolded", true); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bPreProcessFolded", value); }
		}

		public static bool bPlatformsFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bPlatformsFolded", false); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bPlatformsFolded", value); }
		}

		public static bool bPhysicsFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bPhysicsFolded", true); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bPhysicsFolded", value); }
		}

		public static bool bLODFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bLODFolded", true); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bLODFolded", value); }
		}

		public static bool bDebugDrawFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bDebugDrawFolded", false); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bDebugDrawFolded", value); }
		}

		public static bool bDebugDrawTexturesFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bDebugDrawTexturesFolded", false); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bDebugDrawTexturesFolded", value); }
		}

		public static bool repaintDebugTextures
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_repaintDebugTextures", true); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_repaintDebugTextures", value); }
		}

		public static bool bShadowsFolded
		{
			get { return EditorPrefs.GetBool("NeoFur_NeoFurAssetInspector_bShadowsFolded", false); }
			set { EditorPrefs.SetBool("NeoFur_NeoFurAssetInspector_bShadowsFolded", value); }
		}

		//used to detect change in the guide method
		//so that fur data can be reprocessed
		NeoFurAssetData.GuideMethod mLastGuideMethod;
		
		//dropdown value for morphs selector
		int		mMorphChoice;

		//NFA component being inspected
		NeoFurAsset		nfa;

        private class Styles
        {
            public static GUIContent labelFurryMat = new GUIContent(Res.Label_FurMat, Res.Tooltip_FurMat);
            public static GUIContent labelFurSubMesh = new GUIContent(Res.Label_SubMeshIndex, Res.Tooltip_SubMeshIndex);
            public static GUIContent labelShellCount = new GUIContent(Res.Label_ShellCount, Res.Tooltip_ShellCount);
            public static GUIContent labelFurLength = new GUIContent(Res.Label_FurLength, Res.Tooltip_FurLength);
			public static GUIContent labelShellOffset = new GUIContent("Shell OFfset", "Offset all the shells from the mesh.");

            public static GUIContent labelShellFade = new GUIContent(Res.Label_ShellFade, Res.Tooltip_ShellFade);
            public static GUIContent labelNormalDirection = new GUIContent(Res.Label_NormalDirectionBlend, Res.Tooltip_NormalDirectionBlend);
            public static GUIContent foldoutShadows = new GUIContent(Res.Foldout_Shadows);
            public static GUIContent labelUseRenderSettings = new GUIContent(Res.Label_UseRendererSettings, Res.Tooltip_UseRendererSettings);
            public static GUIContent labelShadowCastingMode = new GUIContent(Res.Label_ShadowCastingMode, Res.Tooltip_ShadowCastingMode);
            public static GUIContent labelRecieveShadows = new GUIContent(Res.Label_RecieveShadows, Res.Tooltip_RecieveShadows);
			public static GUIContent foldoutDebugDraw = new GUIContent(Res.Foldout_DebugDraw);
			public static GUIContent foldoutDebugDrawTextures = new GUIContent("Simulation Textures");
            public static GUIContent labelDrawControlPoints = new GUIContent(Res.Label_DrawControlPoints, Res.Tooltip_DrawControlPoints);
            public static GUIContent labelDrawFurGuides = new GUIContent(Res.Label_DrawFurGuides, Res.Tooltip_DrawFurGuides);

            public static GUIContent labelControlPointDotColor = new GUIContent(Res.Label_ControlPointDotColor, Res.Tooltip_ControlPointDotColor);
            public static GUIContent labelControlPointRayColor = new GUIContent(Res.Label_ControlPointRayColor, Res.Tooltip_ControlPointRayColor);
            public static GUIContent labelControlPointDotSize = new GUIContent(Res.Label_ControlPointDotSize, Res.Tooltip_ControlPointDotSize);
            public static GUIContent labelGuideDotColor = new GUIContent(Res.Label_GuideDotColor, Res.Tooltip_GuideDotColor);
            public static GUIContent labelGuideRayColor = new GUIContent(Res.Label_GuideRayColor, Res.Tooltip_GuideRayColor);
            public static GUIContent labelGuideDotSize = new GUIContent(Res.Label_GuideDotSize, Res.Tooltip_GuideDotSize);
            public static GUIContent foldoutLOD = new GUIContent(Res.Foldout_LOD);
            public static GUIContent labelLODStartDist = new GUIContent(Res.Label_LODStartDist, Res.Tooltip_LODStartDist);
            public static GUIContent labelLODEndDist = new GUIContent(Res.Label_LODEndDist, Res.Tooltip_LODEndDist);

            public static GUIContent labelLODMinShellCount = new GUIContent(Res.Label_LODMinShells, Res.Tooltip_LODMinShells);
            public static GUIContent labelLODMaxCamDist = new GUIContent(Res.Label_LODMaxCamDist, Res.Tooltip_LODMaxCamDist);
            public static GUIContent labelNearCompressionDist = new GUIContent(Res.Label_LODNearCompressionDist, Res.Tooltip_LODNearCompressionDist);
            public static GUIContent labelNearCompressionMin = new GUIContent(Res.Label_LODNearCompressionMin, Res.Tooltip_LODNearCompressionMin);
            public static GUIContent labelNearCompressionMax = new GUIContent(Res.Label_LODNearCompressionMax, Res.Tooltip_LODNearCompressionMax);

            public static GUIContent foldoutPhysics = new GUIContent(Res.Foldout_Physics);
            public static GUIContent labelWind = new GUIContent(Res.Label_WindZone, Res.Tooltip_WindZone);
            public static GUIContent labelVelocityInfluence = new GUIContent(Res.Label_VelocityInfluence, Res.Tooltip_VelocityInfluence);
            public static GUIContent labelSpringLenStiff = new GUIContent(Res.Label_SpringLenStiff, Res.Tooltip_SpringLenStiff);
            public static GUIContent labelSpringAngleStiff = new GUIContent(Res.Label_SpringAngStiff, Res.Tooltip_SpringAngStiff);
            public static GUIContent labelSpringDampMult = new GUIContent(Res.Label_SpringDampMult, Res.Tooltip_SpringDampMult);
            public static GUIContent labelGravityInfluence = new GUIContent(Res.Label_GravityInfluence, Res.Tooltip_GravityInfluence);
            public static GUIContent labelAirResistMult = new GUIContent(Res.Label_AirResistMult, Res.Tooltip_AirResistMult);
            public static GUIContent labelMaxStretchDistMult = new GUIContent(Res.Label_MaxStretchMult, Res.Tooltip_MaxStretchMult);
            public static GUIContent labelMinStretchDistMult = new GUIContent(Res.Label_MinStretchMult, Res.Tooltip_MinStretchMult);
            public static GUIContent labelMaxRotFromNormal = new GUIContent(Res.Label_MaxRotFromNormal, Res.Tooltip_MaxRotFromNormal);
            public static GUIContent labelRadialForceInfluence = new GUIContent(Res.Label_RadialInfluence, Res.Tooltip_RadialInfluence);
            public static GUIContent labelWindInfluence = new GUIContent(Res.Label_WindInfluence, Res.Tooltip_WindInfluence);
            public static GUIContent labelBendExponent = new GUIContent(Res.Label_BendExp, Res.Tooltip_BendExp);

            public static GUIContent labelSceneCamera = new GUIContent(Res.Label_SceneCamera, Res.Tooltip_SceneCamera);

            public static GUIContent labelOverrideRenderer = new GUIContent(Res.Label_OverrideRenderer, Res.ToolTip_OverrideRenderer);
            public static GUIContent labelGrowthMesh = new GUIContent(Res.Label_OverrideGrowthMesh, Res.Tooltip_OverrideGrowthMesh);
            public static GUIContent foldoutPreProcess = new GUIContent(Res.Foldout_Preprocess);
            public static GUIContent labelFurGuideGenMethod = new GUIContent(Res.Label_FurGuideGenMethod, Res.Tooltip_FurGuideGenMethod);
            public static GUIContent labelGuideLength = new GUIContent(Res.Label_GuideLength, Res.Tooltip_GuideLength);
            public static GUIContent labelMorphTargetWeight = new GUIContent("", "Morph target weight.");

            public static GUIContent foldoutPlatforms = new GUIContent(Res.Foldout_Platforms);
        }

		private ProjectIssueNotifications projectIssueNotifications;
		SerializedProperty matProp;
        SerializedProperty subMeshIndexProp;
        SerializedProperty shellCountProp;
		SerializedProperty shellOffsetProp;
        SerializedProperty shellDistanceProp;
        SerializedProperty shellDistanceScaleProp;
        SerializedProperty normalDirectionBlendProp;
        SerializedProperty renderSettingsProp;
        SerializedProperty castShadowProp;
        SerializedProperty receiveShadowsProp;
        SerializedProperty drawControlPointsProp;
        SerializedProperty drawGuidesProp;
        
        SerializedProperty lodStartDistProp;
        SerializedProperty lodEndDistProp;
        SerializedProperty lodMinShellCountProp;
        SerializedProperty maxCamDistProp;
        SerializedProperty nearCompressionDistProp;
        SerializedProperty nearCompressionMinProp;
        SerializedProperty nearCompressionMaxProp;
        SerializedProperty windProp;
        SerializedProperty velocityInfluenceProp;
        SerializedProperty springLenStiffProp;
        SerializedProperty springAngleStiffProp;
        SerializedProperty springDampMultProp;
        SerializedProperty gravityInfluenceProp;
        SerializedProperty airResistMultProp;
        SerializedProperty maxStretchDistMultProp;
        SerializedProperty minStretchDistMultProp;
        SerializedProperty maxRotFromNormalProp;
        SerializedProperty radialForceInfluenceProp;
        SerializedProperty windInfluenceProp;
        SerializedProperty bendExponentProp;

        SerializedProperty guideLengthProp;

        SerializedProperty morphWeightsProp;
        SerializedProperty threadCPUPhysics;

        static int prevID = -1;

        void FindProperties()
        {
            int currID = target.GetInstanceID();

            if (currID == prevID) return;

            prevID = currID;
            
            matProp = GetSerializedProperty("mData.mFurryMat");
            subMeshIndexProp = GetSerializedProperty("mData.mFurSubMeshIndex");
            shellCountProp = GetSerializedProperty("mData.mShellCount");
			shellOffsetProp = GetSerializedProperty("mData.mShellOffset");
            shellDistanceProp = GetSerializedProperty("mFurLength");
            shellDistanceScaleProp = GetSerializedProperty("mShellFade");
            normalDirectionBlendProp = GetSerializedProperty("mNormalDirectionBlend");
            renderSettingsProp = GetSerializedProperty("mbUseRendererSettings");
            castShadowProp = GetSerializedProperty("mbShadowCastingMode");
            receiveShadowsProp = GetSerializedProperty("mbRecieveShadows");
            drawControlPointsProp = GetSerializedProperty("mData.mbDrawControlPoints");
            drawGuidesProp = GetSerializedProperty("mbDrawGuides");

            lodStartDistProp = GetSerializedProperty("mLODStartDist");
            lodEndDistProp = GetSerializedProperty("mLODEndDist");
            lodMinShellCountProp = GetSerializedProperty("mLODMinShellCount");
            maxCamDistProp = GetSerializedProperty("mMaxCamDist");
            nearCompressionDistProp = GetSerializedProperty("mNearCompressionDistance");
            nearCompressionMinProp = GetSerializedProperty("mNearCompressionMin");
            nearCompressionMaxProp = GetSerializedProperty("mNearCompressionMax");
            windProp = GetSerializedProperty("mData.mWind");
            velocityInfluenceProp = GetSerializedProperty("mPhysParams.mVelocityInfluence");
            springLenStiffProp = GetSerializedProperty("mPhysParams.mSpringLenStiff");
            springAngleStiffProp = GetSerializedProperty("mPhysParams.mSpringAngleStiff");
            springDampMultProp = GetSerializedProperty("mPhysParams.mSpringDampMult");
            gravityInfluenceProp = GetSerializedProperty("mPhysParams.mGravityInfluence");
            airResistMultProp = GetSerializedProperty("mPhysParams.mAirResistMult");
            maxStretchDistMultProp = GetSerializedProperty("mPhysParams.mMaxStretchDistMult");
            minStretchDistMultProp = GetSerializedProperty("mPhysParams.mMinStretchDistMult");
            maxRotFromNormalProp = GetSerializedProperty("mPhysParams.mMaxRotFromNormal");
            radialForceInfluenceProp = GetSerializedProperty("mPhysParams.mRadialForceInfluence");
            windInfluenceProp = GetSerializedProperty("mPhysParams.mWindInfluence");
            bendExponentProp = GetSerializedProperty("mPhysParams.mBendExponent");

            guideLengthProp = GetSerializedProperty("mGuideLengthPercentage");

            morphWeightsProp = GetSerializedProperty("mMorphWeights");
            threadCPUPhysics = GetSerializedProperty("mData.mbThreadCPUPhysics");
        }

        private void OnEnable()
        {
			nfa = (NeoFurAsset)target;

            projectIssueNotifications = new ProjectIssueNotifications();
			projectIssueNotifications.onRepaint += Repaint;
		}

		private void OnDisable()
        {
			projectIssueNotifications.onRepaint -= Repaint;
			prevID = -1;
		}

        public override void OnInspectorGUI()
		{
			// update serializedObject with NFA's values
			serializedObject.Update();

            FindProperties();

            GUILayout.Space(15);
			projectIssueNotifications.OnGUI();

			if(!TopSection(nfa))
			{
				//early out
				serializedObject.ApplyModifiedProperties();
				return;
			}

            GUILayout.Space(25);

            //fur material, only allow changes editor time
            if (!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(matProp, Styles.labelFurryMat);
            }

            //fur submesh index
            //don't allow changing this at game time
            if (!Application.isPlaying)
            {
                EditorGUILayout.PropertyField(subMeshIndexProp, Styles.labelFurSubMesh);
            }
            // shell count, fur length, shell fade
            EditorGUILayout.PropertyField(shellCountProp, Styles.labelShellCount);

            EditorGUILayout.PropertyField(shellDistanceProp, Styles.labelFurLength);

            EditorGUILayout.PropertyField(shellDistanceScaleProp, Styles.labelShellFade);

			EditorGUILayout.PropertyField(shellOffsetProp, Styles.labelShellOffset);

			GUILayout.Space(25);

            //Preprocessing foldout
            //skip if not in editor mode
            if (!Application.isPlaying)
			{
				PreProcessSection(nfa);
            }

            bShadowsFolded = NeoGUIHelper.HeaderWithFoldout(Styles.foldoutShadows.text, bShadowsFolded);
			if(bShadowsFolded)
			{
				EditorGUI.indentLevel++;

				//shadow settings
				EditorGUILayout.PropertyField(renderSettingsProp, Styles.labelUseRenderSettings);

				if(!nfa.useRendererSettings)
				{
					EditorGUILayout.PropertyField(castShadowProp, Styles.labelShadowCastingMode);

					EditorGUILayout.PropertyField(receiveShadowsProp, Styles.labelRecieveShadows);
				}

				EditorGUI.indentLevel--;
			}

            //LOD foldout
            //In the past here we'd check for an assigned scene camera, as LOD is kind of meaningless
            //without the ability to check the distance to the camera.  But alot of users are setting
            //up cameras in code, so they need to be able to edit these numbers.
            {
                bLODFolded = NeoGUIHelper.HeaderWithFoldout(Styles.foldoutLOD.text, bLODFolded);
				if(bLODFolded)
				{
					EditorGUI.indentLevel++;
                    
                    EditorGUILayout.PropertyField(lodStartDistProp, Styles.labelLODStartDist);

					EditorGUILayout.PropertyField(lodEndDistProp, Styles.labelLODEndDist);

					EditorGUILayout.PropertyField(lodMinShellCountProp, Styles.labelLODMinShellCount);

					EditorGUILayout.PropertyField(maxCamDistProp, Styles.labelLODMaxCamDist);

					//near compression stuff
					EditorGUILayout.PropertyField(nearCompressionDistProp, Styles.labelNearCompressionDist);

					EditorGUILayout.PropertyField(nearCompressionMinProp, Styles.labelNearCompressionMin);

					EditorGUILayout.PropertyField(nearCompressionMaxProp, Styles.labelNearCompressionMax);

					EditorGUI.indentLevel--;
				}
			}

            // Physics foldout
            bPhysicsFolded = NeoGUIHelper.HeaderWithFoldout(Styles.foldoutPhysics.text, bPhysicsFolded);
			if(bPhysicsFolded)
			{
				EditorGUI.indentLevel++;

				EditorGUILayout.PropertyField(velocityInfluenceProp, Styles.labelVelocityInfluence);

				EditorGUILayout.PropertyField(springLenStiffProp, Styles.labelSpringLenStiff);

				EditorGUILayout.PropertyField(springAngleStiffProp, Styles.labelSpringAngleStiff);

				EditorGUILayout.PropertyField(springDampMultProp, Styles.labelSpringDampMult);

				EditorGUILayout.PropertyField(gravityInfluenceProp, Styles.labelGravityInfluence);

				EditorGUILayout.PropertyField(airResistMultProp, Styles.labelAirResistMult);

				EditorGUILayout.PropertyField(maxStretchDistMultProp, Styles.labelMaxStretchDistMult);

				EditorGUILayout.PropertyField(minStretchDistMultProp, Styles.labelMinStretchDistMult);

				EditorGUILayout.PropertyField(maxRotFromNormalProp, Styles.labelMaxRotFromNormal);

				EditorGUILayout.PropertyField(radialForceInfluenceProp, Styles.labelRadialForceInfluence);

                EditorGUILayout.PropertyField(windProp, Styles.labelWind);

                if (windProp.objectReferenceValue != null)
                {
                    NeoGUIHelper.PushIndent();
                    EditorGUILayout.PropertyField(windInfluenceProp, Styles.labelWindInfluence);
                    NeoGUIHelper.PopIndent();
                }

				EditorGUILayout.PropertyField(bendExponentProp, Styles.labelBendExponent);

                // normal direction blend
                EditorGUILayout.PropertyField(normalDirectionBlendProp, Styles.labelNormalDirection);

                EditorGUI.indentLevel--;
			}

            // Debug Draw foldout
            bDebugDrawFolded = NeoGUIHelper.HeaderWithFoldout(Styles.foldoutDebugDraw.text, bDebugDrawFolded);
            if (bDebugDrawFolded)
            {
				NeoFurAsset neoFurAsset = ((NeoFurAsset)target);

				EditorGUI.indentLevel++;

				if (neoFurAsset.vertexProcessor != null)
				{
					GUILayout.Label($"Average compute time: {(neoFurAsset.vertexProcessor.averageProcessTime*1000).ToString("0.00")}ms");
					GUILayout.Label($"Active shells: {(neoFurAsset.lodShellCount)}");
				}
				
                // debug: draw control points?
                EditorGUILayout.PropertyField(drawControlPointsProp, Styles.labelDrawControlPoints);

				// debug: draw fur guides?
				EditorGUILayout.PropertyField(drawGuidesProp, Styles.labelDrawFurGuides);

                // if the application is playing, then show the section for debug textures
                if (Application.isPlaying)
                {
                    bDebugDrawTexturesFolded = NeoGUIHelper.HeaderWithFoldout(Styles.foldoutDebugDrawTextures.text, bDebugDrawTexturesFolded);
                    if (bDebugDrawTexturesFolded)
                    {
                        int originalIndent = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;

                        if (neoFurAsset.vertexProcessor != null)
                        {
                            repaintDebugTextures = EditorGUILayout.Toggle("Live Update", repaintDebugTextures);
                            neoFurAsset.vertexProcessor.DebugDraw();
                        }

                        if (repaintDebugTextures)
                        {
                            Repaint();
                        }

                        EditorGUI.indentLevel = originalIndent;
                    }
                }

				EditorGUI.indentLevel--;
            }

			// send values from property fields back to NFA
			serializedObject.ApplyModifiedProperties();

			//if runtime, do material editor at the bottom
			if (Application.isPlaying)
			{
				ValidateMaterialEditor();

				// only do stuff with the material editor if it was instantiated
				if (mMatEdit && mMatEdit.target)
				{
					GUI.changed = false;

					EditorGUIUtility.labelWidth = 0;

					mMatEdit.DrawHeader();
					mMatEdit.OnInspectorGUI();

					bool didUndo = Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed";
					if (GUI.changed || didUndo)
					{
						nfa.CreateMaterialInstance();
					}
				}
			}
		}

		void ValidateMaterialEditor()
		{
			if (mMatEdit && mMatEdit.target)
			{
				return;
			}

			if (mMatEdit)
			{
				DestroyImmediate(mMatEdit);
			}

			if (nfa.data.furryMat)
			{
				mMatEdit    =(MatInstanceEditor)Editor.CreateEditor(nfa.data.furryMat, typeof(MatInstanceEditor));
				mMatEdit.hideFlags  =HideFlags.DontSave;
			}
		}

		private Dictionary<string, SerializedProperty> propertyDict = new Dictionary<string, SerializedProperty>();
		private SerializedProperty GetSerializedProperty(string propertyPath)
		{
			SerializedProperty property;
            if (propertyDict.ContainsKey(propertyPath))
            {
                return propertyDict[propertyPath];
            }
            else
            {
                property = serializedObject.FindProperty(propertyPath);
                if (property == null)
                {
                    throw new ArgumentException($"Property \"{propertyPath}\" not found.", nameof(propertyPath));
                }
                propertyDict.Add(propertyPath, property);
            }

			return property;
		}

		bool TopSection(NeoFurAsset nfa)
		{
			EditorGUIUtility.labelWidth	=EditorGUIUtility.currentViewWidth / 3;

			//camera
			EditorGUILayout.PropertyField(GetSerializedProperty("mData.mSceneCamera"),
					new GUIContent(Res.Label_SceneCamera, Res.Tooltip_SceneCamera));

			if (Application.isPlaying)
			{
				return	true;
			}

			// try to use the gameobject tagged "main camera" as the default mSceneCamera
			if(nfa.data.sceneCamera == null && Camera.main != null)
			{
				nfa.data.sceneCamera = Camera.main;
			}
	
			if (nfa.renderer && !nfa.overrideRenderer)
			{
				EditorGUILayout.HelpBox("Using "+nfa.renderer.GetType().Name+" on this GameObject.", MessageType.Info);
			}

			EditorGUILayout.PropertyField(GetSerializedProperty("mOverrideRenderer"), Styles.labelOverrideRenderer, true);

			HashSet<Type> notSupportedRendererTypes = new HashSet<Type>();
			foreach (var target in targets)
			{
				NeoFurAsset neoFur = target as NeoFurAsset;
				if (neoFur.overrideRenderer && !neoFur.RendererIsSupported(neoFur.overrideRenderer))
				{
					notSupportedRendererTypes.Add(neoFur.overrideRenderer.GetType());
					neoFur.overrideRenderer = null;
				}
			}

			if (notSupportedRendererTypes.Count > 0)
			{
				foreach (var type in notSupportedRendererTypes)
				{
					EditorUtility.DisplayDialog(type+" Not Supported", type+"s are not supported. Only MeshRenderer and SkinnedMeshRenderer are supported.", "Ok");
				}
			}

			EditorGUILayout.PropertyField(GetSerializedProperty("mData.mOverrideGrowthMesh"),
                    Styles.labelGrowthMesh);

			//early out if main stuff isn't set up
			if (!nfa.renderer)
			{
				EditorGUILayout.HelpBox("No supported renderer on this GameObject and no Override Renderer specified. Add a MeshRenderer and MeshFilter or a SkinnedMeshRenderer to this object, or drag a MeshRenderer or SkinnedMeshRenderer onto the Override Renderer field.", MessageType.Error);
				return false;
			}
			else if (!nfa.mesh)
			{
				EditorGUILayout.HelpBox(nfa.renderer.GetType().Name+" on "+nfa.renderer.gameObject.name+" does not have a mesh. Give the render a mesh, specify an override renderer that does have a mesh, or provide an override growth mesh.", MessageType.Error);
				return	false;
			}

			return	true;
		}


		void PreProcessSection(NeoFurAsset nfa)
		{
            bPreProcessFolded = NeoGUIHelper.HeaderWithFoldout(Styles.foldoutPreProcess.text, bPreProcessFolded);
            if (bPreProcessFolded)
			{
				EditorGUI.indentLevel++;

				EditorGUIUtility.labelWidth =EditorGUIUtility.currentViewWidth / 5;
				GUILayout.BeginHorizontal();

				EditorGUILayout.LabelField(Styles.labelFurGuideGenMethod);

				string[] choices = new string[3];

                choices[0] = "Splines";
				choices[1]  ="Morphs";
				choices[2]  ="Normals";

				nfa.data.guideMethod    =(NeoFurAssetData.GuideMethod)(GUILayout.SelectionGrid(
					(int)nfa.data.guideMethod, choices, 3, EditorStyles.radioButton));

				mLastGuideMethod = nfa.data.guideMethod;

				GUILayout.EndHorizontal();
				
				if(nfa.data.guideMethod == NeoFurAssetData.GuideMethod.Normals)
				{
                    float oldWidth = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth	=EditorGUIUtility.currentViewWidth / 2;

					//desired length of guides generated from normals
					EditorGUILayout.PropertyField(guideLengthProp, Styles.labelGuideLength);
                    EditorGUIUtility.labelWidth = oldWidth;
				}
				
				else if(nfa.data.guideMethod == NeoFurAssetData.GuideMethod.Morphs)
				{
					MorphSection(nfa);
				}
                else if(nfa.data.guideMethod == NeoFurAssetData.GuideMethod.Splines)
                {
                    SplineSection(nfa);
                }

				EditorGUI.indentLevel--;
			}
		}

		/*
		void SplineSection(NeoFurAsset nfa)
		{
			string	fbxPathButtonText	="";
			if(nfa.mPathToFBX == null || nfa.mPathToFBX == "")
			{
				fbxPathButtonText	="Pick Spline FBX File";
			}
			else
			{
				fbxPathButtonText	="Change Spline FBX File";
			}

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			if(GUILayout.Button(fbxPathButtonText))
			{
				string	fullPath	=EditorUtility.OpenFilePanel(
					"FBX Spline Data File", "", "*.fbx;*.FBX");

				nfa.mPathToFBX	=FileUtil.GetProjectRelativePath(fullPath);
				if(nfa.mPathToFBX == "")
				{
					nfa.mPathToFBX	=fullPath;
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUIUtility.labelWidth	=EditorGUIUtility.currentViewWidth / 3.5f;

			//path to spline data
			EditorGUILayout.LabelField("Path to FBX:", nfa.mPathToFBX);

			//spline grab
			if(BackGroundWorker.IsWorking())
			{
				EditorGUILayout.HelpBox("Working... ", MessageType.Info);
			}
			else if(nfa.mPathToFBX != null && nfa.mPathToFBX != "")
			{
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if(nfa.bSplinesReady())
				{
					if(GUILayout.Button("Reload Spline Data"))
					{
						object					context;
						ThreadStuff.ThreadFunc	func;

						nfa.GrabSplineData(out context, out func);

						if(context != null && func != null)
						{
							BackGroundWorker.eComplete	+=OnComplete;
							mFunc	=func;
							BackGroundWorker.RunJob("Computing...", "Computing fur guides...", context, RunThread);
						}
					}
				}
				else
				{
					if(GUILayout.Button("Grab Spline Data"))
					{
						object					context;
						ThreadStuff.ThreadFunc	func;					

						nfa.GrabSplineData(out context, out func);

						if(context != null && func != null)
						{
							BackGroundWorker.eComplete	+=OnComplete;
							mFunc	=func;
							BackGroundWorker.RunJob("Computing...", "Computing fur guides...", context, RunThread);
						}
					}
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}

			if(!nfa.bSplinesReady())
			{
				EditorGUILayout.HelpBox("No fur guides loaded, fixed length guides will be used.", MessageType.Warning);
			}
			else if(!nfa.bGuided() && nfa.bSplinesReady() && !BackGroundWorker.IsWorking())
			{
				EditorGUILayout.HelpBox("Fur Guides processed.  Recompute shell data to use them at runtime.", MessageType.Warning);
			}
		}
		*/

		void MorphSection(NeoFurAsset nfa)
        {
            Mesh gm = nfa.mesh;

            int morphCount = gm.blendShapeCount;

            if (morphCount <= 0)
            {
                EditorGUILayout.HelpBox("Growth mesh contains no morphs...", MessageType.Info);
            }
            else
            {
                if (nfa.morphWeights == null || nfa.morphWeights.Length != morphCount)
                {
                    nfa.morphWeights = new float[morphCount];
                }

                string[] choices = new string[morphCount];

                EditorGUILayout.HelpBox("Set morphs to desired levels.", MessageType.Info);

                for (int i = 0; i < morphCount; i++)
                {
                    choices[i] = gm.GetBlendShapeName(i);
                }
                float oldWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / 2;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                mMorphChoice = EditorGUILayout.Popup(mMorphChoice, choices);

                EditorGUILayout.PropertyField(morphWeightsProp.GetArrayElementAtIndex(mMorphChoice), Styles.labelMorphTargetWeight);

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = oldWidth;
            }
        }

        private void SplineSection(NeoFurAsset nfa)
        {
            GUILayout.Space(10);

            Undo.RecordObject(target, "NeoFur Spline Data Assignment For " + nfa.gameObject.name);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spline Data Asset: ");
            nfa.SplineGuideData = (NeoFur.Data.PreComputedGuideData)EditorGUILayout.ObjectField("", nfa.SplineGuideData, typeof(NeoFur.Data.PreComputedGuideData), false);
            GUILayout.EndHorizontal();

            if (!nfa.SplineGuideData)
            {
				EditorGUILayout.HelpBox("Please specify a PrecomputedSplineData asset file to use by adding it to the object field or create a new one by clicking the button below!", MessageType.Info);
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
            if (GUILayout.Button("Create New PreComputedGuideData"))
            {
                NeoFur.Data.PreComputedGuideData splineData = NeoFur.Editor.PreComputedGuideDataEditor.CreateAssetWithPopup(nfa.mesh, true);
				if (splineData != null)
				{
					nfa.SplineGuideData = splineData;
				}
            }
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
        }
	}
}
