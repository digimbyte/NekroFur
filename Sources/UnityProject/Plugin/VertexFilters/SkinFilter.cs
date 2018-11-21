using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin.VertexFilters
{
	public class SkinFilter : VertexFilter
	{
        Matrix4x4[] bindPoses;

        // bone matrix texture (width = number of matrices, height = 4 for matrix rows/columns) using uv for id/index
        Texture2D boneMatrixTexture;
		VertexProcessorResource<Texture2D> boneIndexTextureResource;
        VertexProcessorResource<Texture2D> boneWeightTextureResource;

        Color[] boneIndexColors;
        Color[] boneWeightColors;
        Color[] _tempMorphPosColors;
        Color[] _tempMorphNormColors;
        Color[] _tempMorphTanColors;
        Color[] _tempColorBuffer;
		Matrix4x4[] _tempMatrixBuffer;

		Transform[] bones;
		
        VertexProcessorResource<BakedMorph>[] bakedFurMorphResources;
        
        Matrix4x4 localTransform;
        Matrix4x4 localTransformInverse;

        RenderBuffer[] targetBufferCache = new RenderBuffer[3];

        internal class BakedMorph : System.IDisposable
        {
            public Texture2D positionOffsetTexture;
            public Texture2D normalOffsetTexture;
            public Texture2D tangentOffsetTexture;

            public void Init(int width, int height, TextureFormat format)
            {
                Dispose();

                positionOffsetTexture = new Texture2D(width, height, format, false);
                normalOffsetTexture = new Texture2D(width, height, format, false);
                tangentOffsetTexture = new Texture2D(width, height, format, false);

                positionOffsetTexture.filterMode = FilterMode.Point;
                normalOffsetTexture.filterMode = FilterMode.Point;
                tangentOffsetTexture.filterMode = FilterMode.Point;
            }

            public void Dispose()
            {
                if (positionOffsetTexture) Object.DestroyImmediate(positionOffsetTexture);
                if (normalOffsetTexture) Object.DestroyImmediate(normalOffsetTexture);
                if (tangentOffsetTexture) Object.DestroyImmediate(tangentOffsetTexture);

                positionOffsetTexture = null;
                normalOffsetTexture = null;
                tangentOffsetTexture = null;
            }
        }

        private Material _boneMaterial;
        private Material boneMaterial
        {
            get
            {
                return _boneMaterial ? _boneMaterial : _boneMaterial = new Material(Shader.Find("Hidden/NeoFur/VertexFilter/Bones"));
            }
        }

        public SkinFilter(VertexProcessor processor) : base(processor) { }

        protected override void OnProcess()
        {
            base.OnProcess();

            localTransform = processor.neoFurAsset.renderer.localToWorldMatrix;
            localTransformInverse = localTransform.inverse;

            DoTheBlits();
        }

        private void DoTheBlits()
        {
            // get temp RTs
            RenderTexture posRT =
                RenderTexture.GetTemporary(processor.positionTexture.width,
                                           processor.positionTexture.height,
                                           processor.positionTexture.depth,
                                           processor.positionTexture.format);
            posRT.filterMode = FilterMode.Point;

            RenderTexture normRT =
                RenderTexture.GetTemporary(processor.normalTexture.width,
                                           processor.normalTexture.height,
                                           processor.normalTexture.depth,
                                           processor.normalTexture.format);
            normRT.filterMode = FilterMode.Point;

            RenderTexture tanRT =
                RenderTexture.GetTemporary(processor.tangentTexture.width,
                                           processor.tangentTexture.height,
                                           processor.tangentTexture.depth,
                                           processor.tangentTexture.format);
            tanRT.filterMode = FilterMode.Point;

            // blit using bone material
            targetBufferCache[0] = posRT.colorBuffer;
            targetBufferCache[1] = normRT.colorBuffer;
            targetBufferCache[2] = tanRT.colorBuffer;

            Graphics.SetRenderTarget(targetBufferCache, posRT.depthBuffer);

            // process morphs
            if (bakedFurMorphResources != null)
            {
                for (int i = 0; i < bakedFurMorphResources.Length; ++i)
                {
					BakedMorph bakedFurMorph = bakedFurMorphResources[i].value;

                    //use this weight for morph targets
                    float blendWeight = processor.neoFurAsset.skinnedMeshRenderer.GetBlendShapeWeight(i) / 100;

					if (blendWeight == 0)
					{
						continue;
					}
                    
                    // set material properties
                    boneMaterial.SetFloat("_BlendWeight", blendWeight);
                    boneMaterial.SetTexture("_BlendPosOffsetTexture", bakedFurMorph.positionOffsetTexture);
                    boneMaterial.SetTexture("_BlendNormOffsetTexture", bakedFurMorph.normalOffsetTexture);
                    boneMaterial.SetTexture("_BlendTanOffsetTexture", bakedFurMorph.tangentOffsetTexture);

                    // blit using bone material
                    targetBufferCache[0] = posRT.colorBuffer;
                    targetBufferCache[1] = normRT.colorBuffer;
                    targetBufferCache[2] = tanRT.colorBuffer;

                    Graphics.SetRenderTarget(targetBufferCache, posRT.depthBuffer);

                    // blit to target buffer cache
                    Graphics.Blit(null, boneMaterial, 0);

					// blit/copy target buffers to processor RTs
					VertexProcessorUtility.CopyTexture(posRT, processor.positionTexture);
					VertexProcessorUtility.CopyTexture(normRT, processor.normalTexture);
					VertexProcessorUtility.CopyTexture(tanRT, processor.tangentTexture);
                }
            }

            ProcessBoneWeights();

            // blit using bone material
            targetBufferCache[0] = posRT.colorBuffer;
            targetBufferCache[1] = normRT.colorBuffer;
            targetBufferCache[2] = tanRT.colorBuffer;

            Graphics.SetRenderTarget(targetBufferCache, posRT.depthBuffer);

            // blit to target buffer cache
            Graphics.Blit(null, boneMaterial, 1);

			// blit/copy target buffers to processor RTs
			VertexProcessorUtility.CopyTexture(posRT, processor.positionTexture);
			VertexProcessorUtility.CopyTexture(normRT, processor.normalTexture);
			VertexProcessorUtility.CopyTexture(tanRT, processor.tangentTexture);

            // release temp RTs
            RenderTexture.ReleaseTemporary(posRT);
            RenderTexture.ReleaseTemporary(normRT);
            RenderTexture.ReleaseTemporary(tanRT);
        }

        private void ProcessBoneWeights()
        {
            // bake bone matrices
            if (_tempColorBuffer == null || _tempColorBuffer.Length != bones.Length*4)
            {
				_tempColorBuffer = new Color[bones.Length*4];
				_tempMatrixBuffer = new Matrix4x4[bones.Length];
			}
			for (int i = 0; i < bindPoses.Length; ++i)
			{
				Matrix4x4 bindPose = bindPoses[i];
				Matrix4x4 boneMatrix = bones[i].localToWorldMatrix;
				Matrix4x4 outputMatrix = localTransformInverse * boneMatrix * bindPose;
				_tempMatrixBuffer[i] = outputMatrix;
			}
			VertexProcessorUtility.MatrixArrayToColorArray(_tempMatrixBuffer, _tempColorBuffer);

			boneMatrixTexture.SetPixels(_tempColorBuffer);

			boneMatrixTexture.Apply();

            // set texture
            boneMaterial.SetTexture("_NeoFur_BoneMatrixTexture", boneMatrixTexture);
        }

        private void FreeTextures()
        {
			if (boneIndexTextureResource != null)
			{
				VertexProcessorCache.ReleaseResource(boneIndexTextureResource);
				boneIndexTextureResource = null;
			}
			if (boneWeightTextureResource != null)
			{
				VertexProcessorCache.ReleaseResource(boneWeightTextureResource);
				boneWeightTextureResource = null;
			}
			if (boneMatrixTexture)
			{
				Object.DestroyImmediate(boneMatrixTexture);
			}

            if(bakedFurMorphResources != null)
			{
				for (int i = 0; i < bakedFurMorphResources.Length; ++i)
				{
					VertexProcessorCache.ReleaseResource(bakedFurMorphResources[i]);
				}
				bakedFurMorphResources = null;
			}
              
        }

        protected override void OnRebuild()
        {
            base.OnRebuild();

            FreeTextures();
            RebuildMorphTextures();
            RebuildBoneWeightTextures();
        }
        
        private void RebuildMorphTextures()
        {
            int blendShapeCount = processor.unpackedMeshResource.value.blendShapes.Count;

            if (blendShapeCount > 0)
            {
                if (bakedFurMorphResources == null || bakedFurMorphResources.Length != blendShapeCount)
                {
					bakedFurMorphResources = new VertexProcessorResource<BakedMorph>[blendShapeCount];
					
                }
                int width = processor.positionTexture.width;
                int height = processor.positionTexture.height;

                int arrayLen = width * height;

                for (int i = 0; i < blendShapeCount; ++i)
                {
					VertexProcessorResource<BakedMorph> morphResource = bakedFurMorphResources[i];
					if (morphResource == null)
					{
						morphResource = VertexProcessorCache.GetResource<BakedMorph>(processor.baseResourceKey+"_BakedMorph_"+i);
						bakedFurMorphResources[i] = morphResource;
					}
					if (morphResource.value == null)
					{
						morphResource.value = new BakedMorph();

						UnpackedMesh.BlendShape blendShape = processor.unpackedMeshResource.value.blendShapes[i];

						if (_tempMorphPosColors == null || _tempMorphPosColors.Length != arrayLen)
						{
							_tempMorphPosColors = new Color[arrayLen];
							_tempMorphNormColors = new Color[arrayLen];
							_tempMorphTanColors = new Color[arrayLen];
						}

						// initialize the baked blend shapes
						morphResource.value.Init(width, height, VertexProcessorUtility.float4TextureFormat);

						// bake the data
						for (int j = 0; j < blendShape.deltaVertices.Length; ++j)
						{
							Vector3 pos = blendShape.deltaVertices[j];
							Vector3 norm = blendShape.deltaNormals[j];
							Vector3 tan = blendShape.deltaTangents[j];

							int y = j / width;
							int x = j % width;

							_tempMorphPosColors[morphResource.value.positionOffsetTexture.width * y + x]
								= new Color(pos.x, pos.y, pos.z, 1);

							_tempMorphNormColors[morphResource.value.normalOffsetTexture.width * y + x]
								= new Color(norm.x, norm.y, norm.z, 1);

							_tempMorphTanColors[morphResource.value.tangentOffsetTexture.width * y + x]
								= new Color(tan.x, tan.y, tan.z, 1);

						}

						morphResource.value.positionOffsetTexture.SetPixels(_tempMorphPosColors);
						morphResource.value.normalOffsetTexture.SetPixels(_tempMorphNormColors);
						morphResource.value.tangentOffsetTexture.SetPixels(_tempMorphTanColors);

						// apply changes to the textures
						morphResource.value.positionOffsetTexture.Apply();
						morphResource.value.normalOffsetTexture.Apply();
						morphResource.value.tangentOffsetTexture.Apply();
					}
                }
            }
        }

        private void RebuildBoneWeightTextures()
        {
            bones = processor.neoFurAsset.skinnedMeshRenderer.bones;
            
            // remake bone matrix texture
            bindPoses = processor.unpackedMeshResource.value.bindposes;
            boneMaterial.SetFloat("_NumBindPoses", bindPoses.Length);
            
			boneMatrixTexture = new Texture2D(4, bindPoses.Length, VertexProcessorUtility.float4TextureFormat, false);
			boneMatrixTexture.filterMode = FilterMode.Point;

			// remake bone weights textures
			BoneWeight[] boneWeights = processor.unpackedMeshResource.value.boneWeights;

            int width = processor.positionTexture.width;
            int height = processor.positionTexture.height;

			boneWeightTextureResource = VertexProcessorCache.GetResource<Texture2D>(processor.baseResourceKey+"_BoneWeightTexture");
			if (boneWeightTextureResource.value == null)
			{
				boneWeightTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
				boneWeightTextureResource.value.filterMode = FilterMode.Point;
			}

			boneIndexTextureResource = VertexProcessorCache.GetResource<Texture2D>(processor.baseResourceKey+"_BoneIndexTexture");
			if (boneIndexTextureResource.value == null)
			{
				boneIndexTextureResource.value = new Texture2D(width, height, VertexProcessorUtility.float4TextureFormat, false);
				boneIndexTextureResource.value.filterMode = FilterMode.Point;
			}

            if (boneWeights == null || boneWeights.Length <= 0)
            {
                Debug.LogError("NeoFur: Using a (Growth) Mesh that does not have any bind poses or bone weights. Please use a MeshRenderer instead or import a (Growth) Mesh with bind poses/bone weights (using the same rig as the display skinned mesh renderer)." +
                   processor.neoFurAsset.gameObject.name,
                   processor.neoFurAsset.gameObject);
            }
            else
            {
                if (boneIndexColors == null || boneIndexColors.Length != boneWeights.Length)
                    boneIndexColors = new Color[boneIndexTextureResource.value.width * boneIndexTextureResource.value.height];

                if (boneWeightColors == null || boneWeightColors.Length != boneWeights.Length)
                    boneWeightColors = new Color[boneWeightTextureResource.value.width * boneWeightTextureResource.value.height];

                // rebuild bone weight textures (indices + weights)
                for (int i = 0; i < boneWeights.Length; ++i)
                {
                    int y = i / width;
                    int x = i % width;

                    BoneWeight bw = boneWeights[i];

                    boneIndexColors[boneIndexTextureResource.value.width * y + x] = new Color(bw.boneIndex0, bw.boneIndex1, bw.boneIndex2, bw.boneIndex3);
                    boneWeightColors[boneWeightTextureResource.value.width * y + x] = new Color(bw.weight0, bw.weight1, bw.weight2, bw.weight3);
                }

                boneIndexTextureResource.value.SetPixels(boneIndexColors);
                boneWeightTextureResource.value.SetPixels(boneWeightColors);

                boneIndexTextureResource.value.Apply();
                boneWeightTextureResource.value.Apply();
            }

            // set material bone textures
            boneMaterial.SetTexture("_NeoFur_BoneIndexTexture", boneIndexTextureResource.value);
            boneMaterial.SetTexture("_NeoFur_BoneWeightTexture", boneWeightTextureResource.value);
        }
        
        protected override void OnDebugDraw()
        {
			base.OnDebugDraw();

			Rect rect;

			if (boneMatrixTexture)
			{
				GUILayout.Label("Bone Matrix Texture");
				rect = GUILayoutUtility.GetRect(boneMatrixTexture.width*4, boneMatrixTexture.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, boneMatrixTexture, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);
			}

			if (boneIndexTextureResource != null)
			{
				GUILayout.Label("Bone Index Map Texture");
				rect = GUILayoutUtility.GetRect(boneIndexTextureResource.value.width*4, boneIndexTextureResource.value.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, boneIndexTextureResource.value, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);
			}

			if (boneWeightTextureResource != null)
			{
				GUILayout.Label("Bone Weight Texture");
				rect = GUILayoutUtility.GetRect(boneWeightTextureResource.value.width*4, boneWeightTextureResource.value.height*4, GUILayout.ExpandWidth(false));
				GUI.DrawTexture(rect, boneWeightTextureResource.value, ScaleMode.StretchToFill, false);
				GUILayout.Space(5);
			}

			if (bakedFurMorphResources != null && bakedFurMorphResources.Length > 0)
			{
				GUILayout.Label("Morph Target Offsets");
				for (int i = 0; i < bakedFurMorphResources.Length; ++i)
				{
					BakedMorph morph = bakedFurMorphResources[i].value;

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					GUILayout.Label("Morph "+i+" Position");
					rect = GUILayoutUtility.GetRect(morph.positionOffsetTexture.width*4, morph.positionOffsetTexture.height*4, GUILayout.ExpandWidth(false));
					GUI.DrawTexture(rect, morph.positionOffsetTexture, ScaleMode.StretchToFill, false);
					GUILayout.Space(5);
					GUILayout.EndVertical();

					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					GUILayout.Label("Morph "+i+" Normal");
					rect = GUILayoutUtility.GetRect(morph.normalOffsetTexture.width*4, morph.normalOffsetTexture.height*4, GUILayout.ExpandWidth(false));
					GUI.DrawTexture(rect, morph.normalOffsetTexture, ScaleMode.StretchToFill, false);
					GUILayout.Space(5);
					GUILayout.EndVertical();

					GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
					GUILayout.Label("Morph "+i+" Tangnet");
					rect = GUILayoutUtility.GetRect(morph.tangentOffsetTexture.width*4, morph.tangentOffsetTexture.height*4, GUILayout.ExpandWidth(false));
					GUI.DrawTexture(rect, morph.tangentOffsetTexture, ScaleMode.StretchToFill, false);
					GUILayout.Space(5);
					GUILayout.EndVertical();

					GUILayout.EndHorizontal();
					GUILayout.Space(5);
				}
			}
        }

        public override void Dispose()
        {
            FreeTextures();
            if(_boneMaterial)
                Object.DestroyImmediate(_boneMaterial);
            base.Dispose();
        }
    }
}
