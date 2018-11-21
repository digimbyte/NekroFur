using System;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.NeoFur.Editor
{
    [Serializable]
    public struct FurPhysicsParameters
    {
        //public float AirResistanceMultiplier;
        //public float Bendiness;
        //public float GravityInfluence;
        //public float MaxRotationFromNormal;
        //public float MaxStretchDistanceMultiplier;
        //public float NormalDirectionBlend;
        //public float RadialForceInfluence;
        //public float SpringAngleStiffness;
        //public float SpringDampeningMultiplier;
        //public float SpringLengthStiffness;
        //public float VelocityInfluence;
        //public float WindInfluence;

        //public override string ToString()
        //{
        //    return string.Format("FurPhysicsParams: [ AirResistanceMultiplier = {0} ]", AirResistanceMultiplier);
        //}
    }

    [Serializable]
    public struct ComponentParameters
    {
        public float ActiveShellCountScale;
        public string FurAsset;
        public float LODEndDistance;
        public float LODMinimumShellCount;
        public float LODStartDistance;
        public string Material;
        public float MaximumDistanceFromCamera;
        public float ShellCount;
        public float ShellDistance;
        public float ShellOffsetTextureCoordinateIndex;
        public float VisibleLengthScale;
        public FurPhysicsParameters FurPhysicsParameters;

        public override string ToString()
        {
            return string.Format("Component: [ Num Shells = {0}, {1} ]", ShellCount, FurPhysicsParameters);
        }
    }

    [Serializable]
    public struct ShaderParameters
    {
        //optimized params
        //public float bColorTip;
        //public float bColorRoot;
        public float AOPattern;
        public float AOPatternDarkness;
        public float AlphaBasedAO;
        public float PatternCurvePower;
        public float bAdjustPatternCurve;

        // complex params
        public string ColorTipMapUND;
        public string ColorRootMapUND;
        public float bColorTipMapUND;
        public float bColorRootMapUND;
        public string ColorMapUND;
        public string ColorMapOVR;
        public Vector2 ColorMapUVScaleUND;
        public Vector2 ColorMapUVScaleOVR;
        public Vector2 ColorTipMapUND_UVScale;
        public Vector2 ColorTipMapOVR_UVScale;
        public Color ColorRootOVR;
        public Color ColorRootUND;
        public Color ColorTipOVR;
        public Color ColorTipUND;
        public string DensityMapOVR;
        public string DensityMapUND;
        public Vector2 DensityMapUND_UVScale;
        public Vector2 DensityMapOVR_UVScale;
        public float DensityOVR;
        public float DensityUND;
        public float DitherAmount;
        public string GradientMapOVR;
        public string GradientScatterMapUND;
        public string GradientMapScatter;
        public string GradientMapUND;
        public float HMPDepthUND;
        public string HeightMapUND;
        public string HMPMap;
        public Vector2 HeightMapUND_UVScale;
        public float HueVariationUND;
        public float RoughnessOVR;
        public float RoughnessUND;
        public float ScatterOVR;
        public float Specular;
        public float Metallic;
        public float SpecularBreakupOVR;
        public float SpecularBreakupUND;
        public string StrandShapeMapUND;
        public Vector2 StrandShapeMapUND_UVScale;
        public string StrandShapeMapOVR;
        public Vector2 StrandShapeMapOVR_UVScale;
        public float StrandLengthMinUND;
        public float StrandLengthMaxUND;
        public float StrandLengthMinOVR;
        public float StrandLengthMaxOVR;
        public float ValueVariationUND;
        public float bColorOVR;
        public float bColorMapOVR;
        public float bColorMapUND;
        public float bDensityMapOVR;
        public float bDensityMapUND;
        public float bUseAnimatedDither;
        public float bGradientMapOVR;
        public float bGradientMapUND;
        public float bGradientMapRootToTipOVR;
        public float bGradientMapRootToTipUND;
        public float bHeightMapOVR;
        public float bHeightMapUND;
        public float bOVR;
        public float bOpaqueBaseUND;

        public override string ToString()
        {
            return string.Format("Shader: [ ColorMapUND = {0}, ColorMapOVR = {1}, ColorMapUVScaleUND = {2}, ColorRootOVR = {3}, ColorRootUND = {4}, ... ]",
                ColorMapUND, ColorMapOVR, ColorTipMapUND_UVScale, ColorRootOVR, ColorRootUND);
        }
    }

    [Serializable]
    public struct NeoFurConfig
    {
        public string Name;
        public string ShaderType;
        public string PluginVersion;
        public string Source;
        public ComponentParameters Component;
        public ShaderParameters Shader;

        public override string ToString()
        {
            return string.Format("Config: [ Name = {0}, Plugin = {1}, ShaderType = {2}, {3}, {4} ]", Name, PluginVersion, ShaderType, Component, Shader);
        }
    }
}
