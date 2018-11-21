using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	/// <summary>
	/// A place to store any project wide NeoFur plugin settings.
	/// There should be an asset of this type in the NeoFur/Resources directory.
	/// </summary>
	public class NeoFurEditorSettings:ScriptableObject
	{
		private const string assetPath = "Assets/Plugins/NeoFur/NeoFurEditorSettings.asset";

		private static NeoFurEditorSettings _instance;
		public static NeoFurEditorSettings instance
		{
			get
			{
				if (!_instance)
				{
					_instance = AssetDatabase.LoadAssetAtPath<NeoFurEditorSettings>(assetPath);
					if (!_instance)
					{
						string directory = Directory.GetParent(assetPath).FullName;
						if (!Directory.Exists(directory))
						{
							throw new System.Exception($"Expected \"{directory}\" to exist.");
						}
						_instance = CreateInstance<NeoFurEditorSettings>();
					}
				}
				return _instance;
			}
		}

		[SerializeField]
		private bool _showInspectorNotifications = true;
		/// <summary>
		/// Gets or sets a value indicating if the user has clicked "Hide Issues" in the notifications part of the <see cref="NeoFurAsset"/> inspector.
		/// </summary>
		public bool showInspectorNotifiations
		{
			get
			{
				return _showInspectorNotifications;
			}
			set
			{
				_showInspectorNotifications = value;
			}
		}

		[SerializeField]
		private bool _showUpgradeNotifications = true;
		/// <summary>
		/// Gets or sets a value indicating if the user has clicked "Hide" in the upgrade message part of the <see cref="NeoFurAsset"/> inspector.
		/// </summary>
		public bool showUpgradeNotifications
		{
			get
			{
				return _showUpgradeNotifications;
			}
			set
			{
				_showUpgradeNotifications = value;
			}
		}

		public void Save()
		{
			NeoFurEditorSettings settingsAsset = AssetDatabase.LoadAssetAtPath<NeoFurEditorSettings>(assetPath);
			if (settingsAsset != null && settingsAsset != this)
			{
				throw new System.Exception("Invalid state.");
			}
			if (settingsAsset == null)
			{
				AssetDatabase.CreateAsset(this, assetPath);
			}

			//Unfortunately this is the only way to save an asset while keeping references
			//to it and its undo history.
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}
	}
}
