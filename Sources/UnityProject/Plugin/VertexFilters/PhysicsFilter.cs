using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin.VertexFilters
{
	public class PhysicsFilter : VertexFilter
	{
		private Material _physicsMaterial;
		private Material physicsMaterial
		{
			get
			{
				return _physicsMaterial ? _physicsMaterial : _physicsMaterial = new Material(Shader.Find("Hidden/NeoFur/VertexFilter/Physics"));
			}
		}

		//Cant use Material.FindPass() in 5.4
		private const int windPassIndex = 0;
		private const int radialForcePassIndex = 1;
		private const int applyPositionPassIndex = 2;
		private const int simulatePassIndex = 3;

		private RenderTexture positionTexture;
		private RenderTexture velocityTexture;
		private VertexProcessorResource<Texture2D> guideTextureResource;

		public PhysicsFilter(VertexProcessor processor) : base(processor)
		{
		}

		private List<RadialForce> radialForces = new List<RadialForce>();
		public void QueueRadialForce(RadialForce force)
		{
			radialForces.Add(force);
		}

		private static bool isDx9 = SystemInfo.graphicsDeviceVersion.Contains("Direct3D 9.0c");
		private RenderBuffer[] targetBufferCache2x = new RenderBuffer[2];
		private RenderBuffer[] targetBufferCache3x;
		protected override void OnProcess()
		{
			base.OnProcess();

			physicsMaterial.SetTexture("_NeoFur_PhysicsPositionTexture", positionTexture);
			physicsMaterial.SetTexture("_NeoFur_PhysicsVelocityTexture", velocityTexture);
			physicsMaterial.SetTexture("_NeoFur_PhysicsGuideTexture", guideTextureResource.value);

			physicsMaterial.SetVector("_NeoFur_PhysicsGravityVector", Physics.gravity*processor.neoFurAsset.physParams.GravityInfluence);

			physicsMaterial.SetFloat("_NeoFur_SpringLengthStiffness", processor.neoFurAsset.physParams.SpringLengthStiffnessInUnityUnits*10);
			physicsMaterial.SetFloat("_NeoFur_SpringAngleStiffness", processor.neoFurAsset.physParams.SpringLengthStiffnessInUnityUnits*10);

			physicsMaterial.SetFloat("_NeoFur_SpringMultiplyer", processor.neoFurAsset.physParams.SpringDampeningMultiplier);
			physicsMaterial.SetFloat("_NeoFur_AirResistanceMultiplyer", processor.neoFurAsset.physParams.AirResistanceMultiplier);

			Vector4 constraints = new Vector4(processor.neoFurAsset.physParams.MinStretchDistanceMultiplier, processor.neoFurAsset.physParams.MaxStretchDistanceMultiplier, processor.neoFurAsset.physParams.MaxRotationFromNormal);
			physicsMaterial.SetVector("_NeoFur_MinMaxConstraints", constraints);

			RenderTexture positionSwap = RenderTexture.GetTemporary(positionTexture.width, positionTexture.height, positionTexture.depth, positionTexture.format);
			positionSwap.filterMode = FilterMode.Point;

			RenderTexture velocitySwap = RenderTexture.GetTemporary(velocityTexture.width, velocityTexture.height, velocityTexture.depth, velocityTexture.format);
			velocitySwap.filterMode = FilterMode.Point;
			
			//TODO: We could support multiple wind sources. Like how we support multiple radial forces
			FakeWind wind = processor.neoFurAsset.mFakeWind;
			if (wind != null)
			{
				Vector3 windVector = wind.GetDirection() * wind.GetForce();
				physicsMaterial.SetVector("_NeoFur_WindVector", windVector);
				physicsMaterial.SetFloat("_NeoFur_WindGustFactor", wind.GetGustFactor());
				physicsMaterial.SetFloat("_NeoFur_WindInfluence", processor.neoFurAsset.physParams.WindInfluence);
				Graphics.Blit(null, velocityTexture, physicsMaterial, windPassIndex);
			}

			if (radialForces.Count > 0)
			{
				physicsMaterial.SetFloat("_NeoFur_RadialForceInfluence", processor.neoFurAsset.physParams.RadialForceInfluence);
				foreach (var force in radialForces)
				{
					physicsMaterial.SetVector("_NeoFur_RadialForcePosition", force.Origin);
					physicsMaterial.SetFloat("_NeoFur_RadialForceRadius", force.Radius);
					physicsMaterial.SetFloat("_NeoFur_RadialForcePower", force.Strength);

					Graphics.Blit(null, velocityTexture, physicsMaterial, radialForcePassIndex);
				}
			}

			radialForces.Clear();

			targetBufferCache2x[0] = positionSwap.colorBuffer;
			targetBufferCache2x[1] = velocitySwap.colorBuffer;
			Graphics.SetRenderTarget(targetBufferCache2x, positionSwap.depthBuffer);
			Graphics.Blit(null, physicsMaterial, simulatePassIndex);

			Graphics.SetRenderTarget(null);

			VertexProcessorUtility.CopyTexture(positionSwap, positionTexture);
			VertexProcessorUtility.CopyTexture(velocitySwap, velocityTexture);

			if (isDx9)
			{
				if (targetBufferCache3x == null)
				{
					targetBufferCache3x = new RenderBuffer[3];
				}
				
				RenderTexture tangentSwap = RenderTexture.GetTemporary(processor.tangentTexture.width, processor.tangentTexture.height, processor.tangentTexture.depth, processor.tangentTexture.format);
				tangentSwap.filterMode = FilterMode.Point;

				targetBufferCache3x[0] = positionSwap.colorBuffer;
				targetBufferCache3x[1] = velocitySwap.colorBuffer;
				targetBufferCache3x[2] = tangentSwap.colorBuffer;
				Graphics.SetRenderTarget(targetBufferCache3x, positionSwap.depthBuffer);
				Graphics.Blit(null, physicsMaterial, applyPositionPassIndex);

				VertexProcessorUtility.CopyTexture(positionSwap, processor.positionTexture);
				VertexProcessorUtility.CopyTexture(velocitySwap, processor.normalTexture);
				VertexProcessorUtility.CopyTexture(tangentSwap, processor.tangentTexture);

				Graphics.SetRenderTarget(null);

				RenderTexture.ReleaseTemporary(tangentSwap);
			}

			Graphics.SetRenderTarget(null);
			RenderTexture.ReleaseTemporary(positionSwap);
			RenderTexture.ReleaseTemporary(velocitySwap);
		}

		protected override void OnRebuild()
		{
			base.OnRebuild();

            DestroyResources();

			int width = processor.positionTexture.width;
			int height = processor.positionTexture.height;

			positionTexture = new RenderTexture(width, height, 0, VertexProcessorUtility.float4RenderTextureFormat);
			positionTexture.filterMode = FilterMode.Point;
			positionTexture.Create();

            velocityTexture = new RenderTexture(width, height, 0, VertexProcessorUtility.float4RenderTextureFormat);
			velocityTexture.filterMode = FilterMode.Point;
			velocityTexture.Create();

            float[] weights = processor.neoFurAsset.morphWeights;

			StringBuilder guideTextureKeySB = new StringBuilder();
			guideTextureKeySB.Append(processor.baseResourceKey);
			guideTextureKeySB.Append("_");
			guideTextureKeySB.Append(processor.neoFurAsset.data.guideMethod);
			guideTextureKeySB.Append("_");
			if (processor.neoFurAsset.data.guideMethod == NeoFurAssetData.GuideMethod.Morphs)
			{
				for (int i = 0; i < weights.Length; i++)
				{
					guideTextureKeySB.Append(weights[i].ToString(CultureInfo.InvariantCulture));
					if (i < weights.Length-1)
					{
						guideTextureKeySB.Append("_");
					}
				}
			}
			else if (processor.neoFurAsset.data.guideMethod == NeoFurAssetData.GuideMethod.Splines)
			{
				if (processor.neoFurAsset.SplineGuideData == null)
				{
					guideTextureKeySB.Append("null");
				}
				else
				{
					guideTextureKeySB.Append(processor.neoFurAsset.SplineGuideData.GetInstanceID().ToString());
				}
			}

			guideTextureKeySB.Append("_PhysicsGuideTexture");
			guideTextureResource = VertexProcessorCache.GetResource<Texture2D>(guideTextureKeySB.ToString());

            if (guideTextureResource.value == null)
            {
                if (processor.neoFurAsset.data.guideMethod == NeoFurAssetData.GuideMethod.Normals)
                {
                    DefaultToNormalGuides();
                }
                else if (processor.neoFurAsset.data.guideMethod == NeoFurAssetData.GuideMethod.Morphs)
                {
                    UnpackedMesh mesh = processor.unpackedMeshResource.value;

                    if (mesh.blendShapes.Count != weights.Length)
                    {
                        Debug.LogError("mesh.blendShapes.Count != weights.Length", processor.neoFurAsset.gameObject);

                        DefaultToNormalGuides();
                    }
                    else if (weights.Length > 0)
                    {
                        Color[] guideVectorColors = new Color[width * height];

                        for (int i = 0; i < weights.Length; i++)
                        {
                            float weight = weights[i] / 100.0f;
                            if (weight == 0)
                            {
                                continue;
                            }

                            UnpackedMesh.BlendShape blendShape = mesh.blendShapes[i];

                            for (int j = 0; j < mesh.vertices.Length; j++)
                            {
                                Vector3 guideVector = blendShape.deltaVertices[j] * weight;
                                guideVectorColors[j] += new Color(guideVector.x, guideVector.y, guideVector.z, 0);
                            }
                        }

                        for (int i = 0; i < mesh.tangents.Length; i++)
                        {
                            Vector3 normal = mesh.normals[i];
                            Vector4 tangent = mesh.tangents[i];

                            Color guideVectorColor = guideVectorColors[i];
                            Vector3 guideVector = new Vector3(guideVectorColor.r, guideVectorColor.g, guideVectorColor.b);

                            Vector3 binormal = Vector3.Cross(normal, tangent);
                            Vector3 tangentSpaceGuide = VertexProcessorUtility.ToTangentSpace(guideVector, normal, binormal, tangent);
                            guideVectorColors[i] = new Color(tangentSpaceGuide.x, tangentSpaceGuide.y, tangentSpaceGuide.z, 0);
                        }
                        guideTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
                        guideTextureResource.value.filterMode = FilterMode.Point;
                        guideTextureResource.value.SetPixels(guideVectorColors);
                        guideTextureResource.value.Apply();
                    }
                    else
                    {
                        DefaultToNormalGuides();
                    }
                }
                else if (processor.neoFurAsset.data.guideMethod == NeoFurAssetData.GuideMethod.Splines)
                {
                    NeoFur.Data.PreComputedGuideData splineData = processor.neoFurAsset.SplineGuideData;

                    if(splineData == null)
                    {
                        Debug.LogWarning("Guide mode set to Splines but no spline data file is loaded", processor.neoFurAsset.gameObject);
                        DefaultToNormalGuides();
                    }
                    else if (splineData.guides.Length == 0 || splineData.guides.Length != processor.unpackedMeshResource.value.vertices.Length)
                    {
                        Debug.Log("num guides = " + splineData.guides.Length + " num verts: " + width * height, processor.neoFurAsset.gameObject);
                        Debug.LogError("Submesh for NFA does not match submesh for Spline Data Asset", processor.neoFurAsset.gameObject);
                        DefaultToNormalGuides();
                    }
                    else
                    {
						UnpackedMesh mesh = processor.unpackedMeshResource.value;

						Color[] guideVectorColors = new Color[width * height];
						for (int i = 0; i < splineData.guides.Length; i++)
                        {
							Vector3 normal = mesh.normals[i];
							Vector4 tangent = mesh.tangents[i];

							int remappedIndex = processor.optimizedVertexMapResource.value[i];
							Vector3 guideVector = splineData.guides[remappedIndex];

							Vector3 binormal = Vector3.Cross(normal, tangent);
							Vector3 tangentSpaceGuide = VertexProcessorUtility.ToTangentSpace(guideVector, normal, binormal, tangent);
							guideVectorColors[i] = new Color(tangentSpaceGuide.x, tangentSpaceGuide.y, tangentSpaceGuide.z, 0);
						}

                        guideTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
                        guideTextureResource.value.filterMode = FilterMode.Point;
                        guideTextureResource.value.SetPixels(guideVectorColors);
                        guideTextureResource.value.Apply();
                    }
                }
			}
		}

        private void DefaultToNormalGuides()
        {
            guideTextureResource.value = new Texture2D(1, 1, VertexProcessorUtility.float4TextureFormat, false);
            guideTextureResource.value.filterMode = FilterMode.Point;
            guideTextureResource.value.SetPixel(0, 0, new Color(0, 0, 1, 1));
            guideTextureResource.value.Apply();
        }

        protected override void OnDebugDraw()
		{
			base.OnDebugDraw();

			Rect rect;

			if (positionTexture)
			{
				GUILayout.Label("Control Point Texture");
				rect = GUILayoutUtility.GetRect(positionTexture.width*4, positionTexture.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, positionTexture, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);
			}
			
			if (velocityTexture)
			{
				GUILayout.Label("Velocity Texture");
				rect = GUILayoutUtility.GetRect(velocityTexture.width*4, velocityTexture.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, velocityTexture, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);
			}

			if (guideTextureResource != null)
			{
				GUILayout.Label("Guide Texture");
				rect = GUILayoutUtility.GetRect(guideTextureResource.value.width*4, guideTextureResource.value.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, guideTextureResource.value, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);
			}
		}

		private void DestroyResources()
		{
			if (positionTexture)
			{
				Object.DestroyImmediate(positionTexture);
			}
			if (velocityTexture)
			{
				Object.DestroyImmediate(velocityTexture);
			}
			if (guideTextureResource != null)
			{
				VertexProcessorCache.ReleaseResource(guideTextureResource);
				guideTextureResource = null;
			}
		}

		protected override void OnBindToMaterial(Material material)
		{
			base.OnBindToMaterial(material);

			material.SetTexture("_NeoFur_PhysicsPositionTexture", positionTexture);
			material.SetTexture("_NeoFur_PhysicsVelocityTexture", velocityTexture);
			material.SetTexture("_NeoFur_PhysicsGuideTexture", guideTextureResource.value);
		}

		public override void Dispose()
		{
			base.Dispose();

			DestroyResources();
		}
	}
}
