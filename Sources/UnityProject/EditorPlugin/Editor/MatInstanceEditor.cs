using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com


namespace NeoFurUnityPlugin
{
	public class MatInstanceEditor : MaterialEditor
	{
		public override void Awake()
		{
			if(this == null)
			{
//				Debug.Log("Awake() Null this");
			}
			else if(this.target == null)
			{
//				Debug.Log("Awake() Null target");
			}
			else
			{
//				Debug.Log("Awake(): " + this.target.ToString());
				base.Awake();
			}

			if(this != null)
			{
//				Debug.Log("Awake() is persistant: " + EditorUtility.IsPersistent(this));
				hideFlags	=HideFlags.DontSave;
			}
		}


		public override void OnEnable()
		{
			if(this == null)
			{
//				Debug.Log("OnEnable() Null this");
			}
			else if(this.target == null)
			{
//				Debug.Log("OnEnable() Null target");
			}
			else
			{
//				Debug.Log("OnEnable(): " + this.target.ToString());
				base.OnEnable();
			}

			if(this != null)
			{
//				Debug.Log("OnEnable() is persistant: " + EditorUtility.IsPersistent(this));
			}
		}


		public override void OnInspectorGUI()
		{
			if(this == null)
			{
//				Debug.Log("OnInspectorGUI() Null this");
			}
			else if(this.target == null)
			{
//				Debug.Log("OnInspectorGUI() Null target");
			}
			else
			{
//				Debug.Log("OnInspectorGUI(): " + this.target.ToString());
				base.OnInspectorGUI();
			}

			if(this != null)
			{
//				Debug.Log("OnInspectorGUI() is persistant: " + EditorUtility.IsPersistent(this));
			}
		}


		public override void OnDisable()
		{
			if(this == null)
			{
//				Debug.Log("OnDisable() Null this");
			}
			else if(this.target == null)
			{
//				Debug.Log("OnDisable() Null target");
			}
			else
			{
//				Debug.Log("Disable(): " + this.target.ToString());
				base.OnDisable();
			}

//			MatInstanceEditor	[]eds	=FindObjectsOfType<MatInstanceEditor>();

//			Debug.Log("Have " + eds.Length + " editors sticking around");

			if(this != null)
			{
//				Debug.Log("OnDisable() is persistant: " + EditorUtility.IsPersistent(this));
			}
		}
	}
}
