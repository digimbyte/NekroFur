using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Reflection;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	internal class ProjectIssueNotifications : EditorWindowExtension
	{
		private struct Issue
		{
			public delegate void FixButtonDelegate();

			public string message { get; private set; }
			public FixButtonDelegate fixButton { get; private set; }
			public MessageType messageType { get; private set; }

			public Issue(string message, FixButtonDelegate fixButton, MessageType messageType)
			{
				this.message = message;
				this.fixButton = fixButton;
				this.messageType = messageType;
			}
		}

		private static readonly Shader neoFurDeferredShadingShader = Shader.Find("Hidden/NeoFurInternal-DeferredShading");
		private static readonly Shader neoFurDeferredReflectionsShader = Shader.Find("Hidden/NeoFurInternal-DeferredReflections");

		public static Shader deferredShadingShader
		{
			get
			{
				return GraphicsSettings.GetCustomShader(BuiltinShaderType.DeferredShading);
			}
			set
			{
				GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseCustom);
				GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, value);
			}
		}

		public static Shader deferredReflectionsShader
		{
			get
			{
				return GraphicsSettings.GetCustomShader(BuiltinShaderType.DeferredReflections);
			}
			set
			{
				
				GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredReflections, BuiltinShaderMode.UseCustom);
				GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredReflections, value);
			}
		}

		private static bool proxyRenderPathNotFound;
		private static PropertyInfo _proxyRenderPathProperty;
		private static PropertyInfo proxyRenderPathProperty
		{
			get
			{
				if (_proxyRenderPathProperty == null && !proxyRenderPathNotFound)
				{
					try
					{
						foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
						{
							if (assembly.GetName().Name == "Assembly-CSharp-Editor-firstpass")
							{
								foreach (var type in assembly.GetTypes())
								{
									if (type.FullName == "Neoglyphic.NeoFur.Editor.NeoFurDeferredSettingsProxy")
									{
										_proxyRenderPathProperty = type.GetProperty("renderingPath", BindingFlags.Public|BindingFlags.Static);
										break;
									}
								}
								if (_proxyRenderPathProperty != null)
								{
									break;
								}
							}
						}
					}
					catch (System.Exception ex)
					{
						Debug.LogException(ex);
					}
					proxyRenderPathNotFound = _proxyRenderPathProperty == null;
				}

				return _proxyRenderPathProperty;
			}
		}

		public static RenderingPath renderingPath
		{
			//This is how we need to get and set the render path in both Unity 5.4 and 5.5+
			get
			{
				if (proxyRenderPathNotFound)
				{
					return RenderingPath.DeferredShading;
				}
				return (RenderingPath)proxyRenderPathProperty.GetValue(null, new object[] { });
			}
			set
			{
				if (proxyRenderPathNotFound)
				{
					return;
				}
				proxyRenderPathProperty.SetValue(null, value, new object[] { });
			}
		}

		public static ColorSpace colorSpace
		{
			get
			{
				return PlayerSettings.colorSpace;
			}
			set
			{
				PlayerSettings.colorSpace = value;
			}
		}

		public bool forceShowIssues { get; set; }

		private List<Issue> issueCache = new List<Issue>();

		public bool OnGUI()
		{
			if (Event.current.type == EventType.ValidateCommand)
			{
				if (Event.current.commandName == "UndoRedoPerformed")
				{
					Repaint();
				}
			}

			if (renderingPath != RenderingPath.DeferredShading)
			{
				issueCache.Add(new Issue("Rendering path is set to "+renderingPath+". <b>"+RenderingPath.DeferredShading+"</b> rendering path is recommended.", FixRenderingPath, MessageType.Warning));
			}
			else
			{
				bool state = deferredShadingShader != neoFurDeferredShadingShader;
				state |= GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredShading) != BuiltinShaderMode.UseCustom;
				state |= deferredReflectionsShader != neoFurDeferredReflectionsShader;
				state |= GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredReflections) != BuiltinShaderMode.UseCustom;
				if (state)
				{
					issueCache.Add(new Issue("When using Deferred Shading, <b>Deferred</b> shading and reflection <b>shaders</b> must be changed to NeoFur versions in graphics settings.", FixRenderingPath, MessageType.Error));
				}
			}
			if (colorSpace != ColorSpace.Linear)
			{
				issueCache.Add(new Issue("Color space is currently set to "+colorSpace+". <b>"+ColorSpace.Linear+"</b> color space is recommended.", FixColorSpace, MessageType.Warning));
			}

			bool returnValue = true;

			if (NeoFurPluginInfo.buildType == PluginBuildType.Debug)
			{
				returnValue = false;
				EditorGUILayout.HelpBox("NeoFurUnityPlugin is build in Debug mode. Do not ship this.", MessageType.Warning);
			}

			if (NeoFurEditorPluginInfo.buildType == PluginBuildType.Debug)
			{
				returnValue = false;
				EditorGUILayout.HelpBox("EditorPlugin is build in Debug mode. Do not ship this.", MessageType.Warning);
			}

			if (issueCache.Count > 0)
			{
				StringBuilder sb = new StringBuilder();

				bool hasError = issueCache.Any(v => v.messageType == MessageType.Error);

				MessageType messageType = MessageType.Warning;

				if (hasError)
				{
					sb.Append("<b>The following critical issues have been detected. They must be resolved.</b>\n\n");
					issueCache.RemoveAll(v => v.messageType != MessageType.Error);
					messageType = MessageType.Error;
				}
				else
				{
					//Dont show warnings if the user hid them, but do show errors.
					if (!forceShowIssues && !NeoFurEditorSettings.instance.showInspectorNotifiations)
					{
						return true;
					}
					sb.Append("<b>The following issues have been detected. For best results they should be resolved.</b>\n\n");
				}

				
				for (int i = 0; i < issueCache.Count; i++)
				{
					Issue issue = issueCache[i];
					
					sb.Append("● ");
					sb.Append(issue.message);
					if (i < issueCache.Count-1)
					{
						sb.Append("\n\n");
					}
				}

				//Extra space for button.
				sb.Append("\n\n\n");

				bool originalRichText = false;
				GUIStyle helpBoxStyle = GUI.skin.GetStyle("HelpBox");
				if (helpBoxStyle != null)
				{
					originalRichText = helpBoxStyle.richText;
					helpBoxStyle.richText = true;
				}

				EditorGUILayout.HelpBox(sb.ToString(), messageType);

				if (helpBoxStyle != null)
				{
					helpBoxStyle.richText = originalRichText;
				}

				//Put button inside helpbox.
				GUILayout.Space(-30);
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Fix Issues Now"))
				{
					foreach (var issue in issueCache)
					{
						issue.fixButton?.Invoke();
					}
				}
				EditorGUILayout.Space();
				if (!hasError)
				{
					//Only show hide button for warnings.
					if (!forceShowIssues && GUILayout.Button("Hide Issues"))
					{
						Undo.RecordObject(NeoFurEditorSettings.instance, "Hide NeoFur Inspector Issues");
						NeoFurEditorSettings.instance.showInspectorNotifiations = false;
						NeoFurEditorSettings.instance.Save();
					}
					EditorGUILayout.Space();
				}
				
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();

				EditorGUILayout.Space();
			}

			returnValue &= issueCache.Count == 0;

			issueCache.Clear();

			return returnValue;
		}

		private static void FixRenderingPath()
		{
			if (renderingPath != RenderingPath.DeferredShading)
			{
				Object playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
				if (playerSettings)
				{
					Undo.RecordObject(playerSettings, "Change Project Settings");
				}
				renderingPath = RenderingPath.DeferredShading;
			}

			BuiltinShaderMode currentShadingShaderMode = GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredShading);
			BuiltinShaderMode currentReflectionsShaderMode = GraphicsSettings.GetShaderMode(BuiltinShaderType.DeferredReflections);

			bool needsShaderUpdate = deferredShadingShader != neoFurDeferredShadingShader || deferredReflectionsShader != deferredReflectionsShader;
			needsShaderUpdate |= currentShadingShaderMode != BuiltinShaderMode.UseCustom || currentReflectionsShaderMode != BuiltinShaderMode.UseCustom;
			if (needsShaderUpdate)
			{
				string warningMessage = "";

				if (currentShadingShaderMode == BuiltinShaderMode.UseCustom && deferredShadingShader && deferredShadingShader != neoFurDeferredShadingShader)
				{
					warningMessage += "Deferred shading shader is currently set to "+deferredShadingShader.name+". It will be changed to a neofur deferred shading shader.\n";
				}
				if (currentReflectionsShaderMode == BuiltinShaderMode.UseCustom && deferredReflectionsShader && deferredReflectionsShader != neoFurDeferredReflectionsShader)
				{
					warningMessage += "Deferred reflections shader is currently set to "+deferredReflectionsShader.name+". It will be changed to a neofur deferred reflections shader.\n";
				}

				if (warningMessage != "")
				{
					warningMessage += "Do you want to change shader references listed above?";
					if (!EditorUtility.DisplayDialog("Deferred rendering shader conflict.", warningMessage, "Yes", "No"))
					{
						return;
					}
				}

				Object graphicsSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset").FirstOrDefault();
				if (graphicsSettings)
				{
					Undo.RecordObject(graphicsSettings, "Change Project Settings");
				}

				deferredShadingShader = neoFurDeferredShadingShader;
				deferredReflectionsShader = neoFurDeferredReflectionsShader;
			}
		}

		private static void FixColorSpace()
		{
			if (colorSpace != ColorSpace.Linear)
			{
				Object playerSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset").FirstOrDefault();
				if (playerSettings)
				{
					Undo.RecordObject(playerSettings, "Change Project Settings");
				}
				colorSpace = ColorSpace.Linear;
			}
		}
	}
}
