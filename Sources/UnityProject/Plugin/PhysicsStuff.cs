using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com


namespace NeoFurUnityPlugin
{
	[Serializable]
	public class PhysicsStuff
	{
		[SerializeField]
		private float	mVelocityInfluence = 1f;
		/// <summary>
		/// Influence of velocity forces
		/// </summary>
		public float	VelocityInfluence
		{
			get {return mVelocityInfluence;}
			set {mVelocityInfluence = value;}
		}

		[SerializeField]
		private float	mSpringLenStiff = 250f;
		/// <summary>
		/// Spring length stiffness
		/// </summary>
		public float	SpringLengthStiffness
		{
			get {return mSpringLenStiff;}
			set {mSpringLenStiff = value; }
		}

        // converted spring length stiffness
        internal float SpringLengthStiffnessInUnityUnits
        {
            get { return Neoglyphic.NeoFur.ConvertUnits.ToMeters(mSpringLenStiff); }
        }

		[SerializeField]
		private float	mSpringAngleStiff = 1200f;
		/// <summary>
		/// Spring angular stiffness
		/// </summary>
		public float	SpringAngleStiffness
		{
			get {return mSpringAngleStiff;}
			set {mSpringAngleStiff = value;}
		}

        // converted spring angle stiffness
        internal float SpringAngleStiffnessInUnityUnits
        {
            get { return Neoglyphic.NeoFur.ConvertUnits.ToMeters(mSpringAngleStiff); }
        }

		[SerializeField]
		private float	mSpringDampMult = 0.95f;
		/// <summary>
		/// Multiplier for spring dampening force
		/// </summary>
		public float	SpringDampeningMultiplier
		{
			get {return mSpringDampMult;}
			set {mSpringDampMult = value;}
		}

		[SerializeField]
		private float	mGravityInfluence = 1f;
		/// <summary>
		/// Scale for gravitational force influence
		/// </summary>
		public float	GravityInfluence
		{
			get {return mGravityInfluence;}
			set {mGravityInfluence = value;}
		}

		[SerializeField]
		private float	mAirResistMult = 0.95f;
		/// <summary>
		/// Scale for air resistance
		/// </summary>
		public float	AirResistanceMultiplier
		{
			get {return mAirResistMult;}
			set {mAirResistMult = value;}
		}

		[SerializeField]
		private float	mMaxStretchDistMult = 1.25f;
		/// <summary>
		/// Largest distance that fur strands can stretch to
		/// </summary>
		public float	MaxStretchDistanceMultiplier
		{
			get {return mMaxStretchDistMult;}
			set { mMaxStretchDistMult = value;}
		}

		[SerializeField]
		private float	mMinStretchDistMult = 0.75f;
		/// <summary>
		/// Distance to which fur strands must expand to, as a minimum
		/// </summary>
		public float	MinStretchDistanceMultiplier
		{
			get {return mMinStretchDistMult;}
			set {mMinStretchDistMult = value;}
		}

		[SerializeField]
		private float	mMaxRotFromNormal = 60f;
		/// <summary>
		/// Largest angle from their normal that fur strands can reach
		/// </summary>
		public float	MaxRotationFromNormal
		{
			get {return mMaxRotFromNormal;}
			set {mMaxRotFromNormal = value;}
		}

		[SerializeField]
		private float	mRadialForceInfluence = 1f;
		/// <summary>
		/// Influence of radial forces
		/// </summary>
		public float	RadialForceInfluence
		{
			get {return mRadialForceInfluence;}
			set {mRadialForceInfluence = value;}
		}
		
		[SerializeField]
		private float	mWindInfluence = 1f;
		/// <summary>
		/// Influence of wind forces
		/// </summary>
		public float	WindInfluence
		{
			get {return mWindInfluence;}
			set {mWindInfluence = value;}
		}

		[SerializeField]
		private float	mBendExponent = 1f;
		/// <summary>
		/// Control for 'bendiness' of fur strands
		/// </summary>
		public float	Bendiness
		{
			get {return mBendExponent;}
			set {mBendExponent = value;}
		}
	}
}
