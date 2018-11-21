using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com


namespace NeoFurUnityPlugin
{
	[InitializeOnLoad]
	internal class EditorWatcher : Editor
	{
		static EditorWatcher()
		{
			EditorApplication.playmodeStateChanged	+=OnEditorStateChanged;
		}


		static void OnEditorStateChanged()
		{
//			Debug.Log("State Changed:" + EditorApplication.isPlaying);

			Scene	s	=SceneManager.GetActiveScene();
			if(s != null)
			{
				GameObject	[]objs	=s.GetRootGameObjects();

				for(int i=0;i < objs.Length;i++)
				{
					NeoFurAsset	[]neos	=objs[i].GetComponentsInChildren<NeoFurAsset>();
					if(neos == null)
					{
						continue;
					}

					for(int j=0;j < neos.Length;j++)
					{
						neos[j].FreeDebugGuides();
					}
				}
			}
		}
	}
}
