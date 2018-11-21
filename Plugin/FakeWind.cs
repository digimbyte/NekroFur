using UnityEngine;
using System.Collections;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com


namespace NeoFurUnityPlugin
{
	//This class tries to guess at what Unity's wind generator is doing,
	//as all the information they provide is a direction.
	internal class FakeWind
	{
		WindZone	mWindZone;
		Random		mRand	=new Random();

		//current values updated per frame
		Vector3	mDirection;
		float	mForce;

		//deltas
		float	mForceDelta;
		float	mPulseForce;
		float	mPulseDecay;

		//state
		bool	mbPulseActive;

		//constants
		const float	PulseDecayTime			=1f;	//time to decay a gust
		const float	TurbulanceAngleFactor	=3000f;	//how much turb tweaks direction


		internal FakeWind(WindZone wz)
		{
			mWindZone	=wz;
		}


		internal void SetWindZone(WindZone wz)
		{
			mWindZone	=wz;
		}


		internal Vector3 GetDirection()
		{
			return	mDirection;
		}


		internal float GetForce()
		{
			return	mForce;
		}


		internal float GetGustFactor()
		{
			return	mPulseForce;
		}

        static char NeoFurGetRandomByte()
        {
            return (char)Random.Range(0, 255);
        }


        internal void Update(float deltaTime)
		{
			if(mWindZone == null)
			{
				//need to zero these out, if the user changes zones
				//and goes to null, will be left with a permanent wind
				mDirection	=Vector3.zero;
				mForce		=0f;
				return;
			}

			mDirection	=mWindZone.transform.forward;

			mForce	=mWindZone.windMain + mForceDelta;

			float	turb	=mWindZone.windTurbulence;

			turb	*=deltaTime;

			float	turbChange	=Random.Range(-turb, turb);

			mForceDelta	+=turbChange;

			mForceDelta	=Mathf.Clamp(mForceDelta,
				-mWindZone.windTurbulence, mWindZone.windTurbulence);

			//do a slight perturbation of direction
			Quaternion	turbRot	=Quaternion.AngleAxis(
				turbChange * TurbulanceAngleFactor, Vector3.up);

			mDirection	=turbRot * mDirection;

			if(!mbPulseActive)
			{
				//guessing this is the chance per second
				float	pulseChance	=mWindZone.windPulseFrequency * deltaTime;

				pulseChance	*=10000f;

				int	die	=Random.Range(0, 10000);

				if(die < pulseChance)
				{
					//TODO: curvey attack on wind gust so it ramps up
					//this ramps up instantly
					mbPulseActive	=true;
					mPulseForce		=mWindZone.windPulseMagnitude;
					mPulseDecay		=(1f / PulseDecayTime) * mPulseForce;
				}
			}
			else
			{
				//TODO: try just setting this as gustfactor without magnifying
				mForce	+=mPulseForce;

				mPulseForce	-=mPulseDecay * deltaTime;
				if(mPulseForce <= 0f)
				{
					mbPulseActive	=false;
				}
			}
		}
	}
}
