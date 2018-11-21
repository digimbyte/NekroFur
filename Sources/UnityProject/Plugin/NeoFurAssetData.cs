using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com


namespace NeoFurUnityPlugin
{
	//This class stores some NeoFurAsset data.
	[Serializable]
	public class NeoFurAssetData
	{
		/// <summary>
		/// Methods used for obtaining fur guides
		/// </summary>
		public enum GuideMethod
		{
            Splines = 0,
			Morphs	=1,
			Normals	=2
		};

		[SerializeField]
		private Camera mSceneCamera;
		/// <summary>
		/// Main camera which will render fur
		/// </summary>
		public Camera sceneCamera
		{
			get {return mSceneCamera;}
			set {mSceneCamera = value;}
		}
		
		[SerializeField]
		private Mesh mOverrideGrowthMesh;
		/// <summary>
		/// Growth mesh for fur shell generation
		/// </summary>
		public Mesh overrideGrowthMesh
		{
			get {return mOverrideGrowthMesh; }
			set { mOverrideGrowthMesh = value;}
		}

		[SerializeField]
		private Material mFurryMat;	
		/// <summary>
		/// The fur material
		/// </summary>
		public Material	furryMat
		{
			get {return mFurryMat;}
			set {mFurryMat = value;}
		}

		[SerializeField]
		private GuideMethod mGuideMethod = GuideMethod.Normals;
		/// <summary>
		/// Method for obtaining this object's fur guides
		/// </summary>
		public GuideMethod guideMethod
		{
			get {return mGuideMethod;}
			set {mGuideMethod = value;}
		}

		[SerializeField]
		private int mFurSubMeshIndex;
		/// <summary>
		/// Submesh index to use for fur growth
		/// </summary>
		public int furSubMeshIndex
		{
			get {return mFurSubMeshIndex;}
			set {mFurSubMeshIndex = value;}
		}

		[SerializeField]
		private WindZone mWind;
		/// <summary>
		/// WindZone which applies its forces to the object's fur
		/// </summary>
		public WindZone wind
		{
			get {return mWind;}
			set {mWind = value;}
		}

		[SerializeField]
		private int mShellCount = 20;
		/// <summary>
		/// Number of fur shells to create
		/// </summary>
		public int shellCount
		{
			get {return mShellCount;}
			set {mShellCount = value;}
		}

		[SerializeField]
		private bool mbDrawControlPoints = false;
		/// <summary>
		/// Draw fur control points for debugging?
		/// </summary>
		public bool bDrawControlPoints
		{
			get {return mbDrawControlPoints;}
			set {mbDrawControlPoints = value;}
		}

		[SerializeField]
		private bool mbThreadCPUPhysics = false;
		public bool bThreadCPUPhysics
		{
			get {return mbThreadCPUPhysics;}
			set {mbThreadCPUPhysics = value;}
		}

		[SerializeField]
		private float mShellOffset = 0;
		public float shellOffset
		{
			get { return mShellOffset; }
			set { mShellOffset = value; }
		}
	}
}
