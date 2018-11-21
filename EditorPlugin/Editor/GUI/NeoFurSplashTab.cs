using NeoFurUnityPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.Editor
{
    internal class NeoFurSplashTab : NeoSuiteSplashTab
    {
        private class LinkButton
        {
            public string title { get; private set; }
            public string url { get; private set; }
            public Texture2D texture { get; private set; }

            public LinkButton(string title, string url, Texture2D texture)
            {
                this.title = title;
                this.url = url;
                this.texture = texture;
            }
        }

		public override float Priority
		{
			get
			{
				return 0;
			}
		}

		private float _requiredHeight;
		public override float requiredHeight
		{
			get
			{
				return _requiredHeight;
			}
		}

		private static string _versionString;
        private static string versionString
        {
            get
            {
                if (_versionString == null)
                {
                    _versionString = NeoFurPluginInfo.version.Major + "." + NeoFurPluginInfo.version.Minor + "." + NeoFurPluginInfo.version.Build;
                }
                return _versionString;
            }
        }

        private Texture _banner;
        private Texture banner
        {
            get
            {
                if (!_banner)
                {
                    _banner = AssetDatabase.LoadAssetAtPath<Texture>("Assets/Plugins/NeoFur/Editor/Textures/SplashBanner.png");
                }
                return _banner;
            }
        }

        private Texture2D _storeButtonTexture;
        private Texture2D storeButtonTexture
        {
            get
            {
                if (!_storeButtonTexture)
                {
                    _storeButtonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/NeoFur/Editor/Textures/StoreButton.png");
                }
                return _storeButtonTexture;
            }
        }

        private Texture2D _supportButton;
        private Texture2D supportButtonTexture
        {
            get
            {
                if (!_supportButton)
                {
                    _supportButton = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/NeoFur/Editor/Textures/SupportDocumentationButton.png");
                }
                return _supportButton;
            }
        }

        private Texture2D _websiteButton;
        private Texture2D websiteButtonTexture
        {
            get
            {
                if (!_websiteButton)
                {
                    _websiteButton = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/NeoFur/Editor/Textures/WebsiteButton.png");
                }
                return _websiteButton;
            }
        }

        private Texture2D _tutorialButton;
        private Texture2D tutorialButtonTexture
        {
            get
            {
                if (!_tutorialButton)
                {
                    _tutorialButton = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Plugins/NeoFur/Editor/Textures/YouTubeTutorialButton.png");
                }
                return _tutorialButton;
            }
        }

        private List<Action> columns = new List<Action>();
        private List<LinkButton> linkButtons = new List<LinkButton>();
        private ProjectIssueNotifications projectIssueNotifications;
		private GUIStyle linkButtonTextStyle;

        [InitializeOnLoadMethod]
        static void InitOnLoadFunc()
        {
			NeoSuiteSplashWindow.RegisterTabType(typeof(NeoFurSplashTab));
        }

        public NeoFurSplashTab(NeoSuiteSplashWindow window) : base(window)
        {

        }

        public override void OnEnable()
        {
            columns.Add(IssueColumn);

			linkButtons.Add(new LinkButton("Website", "https://www.neoglyphic.com/neofur/unity", websiteButtonTexture));
            linkButtons.Add(new LinkButton("Store", "https://store.neoglyphic.com/products/neofur/", storeButtonTexture));
            linkButtons.Add(new LinkButton("Tutorials", "https://www.youtube.com/watch?v=EovsIsJsU-U&list=PLX2UqL9qLkirKPGp1hL-db4rapBNdutr_", tutorialButtonTexture));
            linkButtons.Add(new LinkButton("Support", "http://support.neoglyphic.com/forums/6-neofur-for-unity-knowledge-base/", supportButtonTexture));

            projectIssueNotifications = new ProjectIssueNotifications();
            projectIssueNotifications.forceShowIssues = true;
            projectIssueNotifications.onRepaint += window.Repaint;
		}

		public override void OnDisable()
        {
            columns.Clear();

            projectIssueNotifications.onRepaint -= window.Repaint;
		}

		public override void OnGUI()
        {
			//This doesnt like being in OnEnable.
			if (linkButtonTextStyle == null)
			{
				linkButtonTextStyle = new GUIStyle(EditorStyles.boldLabel);
				linkButtonTextStyle.fontSize = 14;
			}

			int headerHeight = Mathf.RoundToInt(((float)banner.height/banner.width)*window.maxSize.x);

			Rect headerRect = new Rect(0, 0, window.minSize.x, headerHeight);
			GUI.DrawTexture(headerRect, banner, ScaleMode.ScaleAndCrop);

			GUILayout.BeginArea(headerRect);

			GUILayout.FlexibleSpace();
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

			GUI.color = new Color(1, 1, 1, 0.5f);
			GUILayout.Label("version "+versionString, EditorStyles.whiteLabel);
			GUI.color = Color.white;

			EditorGUILayout.Space();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			GUILayout.EndArea();

			GUILayout.Space(headerHeight);

			Rect totalLayoutRect = EditorGUILayout.BeginVertical();

			if (linkButtons.Count > 0)
			{
				Rect layoutRect = EditorGUILayout.BeginVertical();

				//Dont know why its not already this width... Its 4 pixels off for some reason.
				layoutRect.width = window.maxSize.x;

				if (Event.current.type == EventType.Repaint)
				{
					if (EditorGUIUtility.isProSkin)
					{
						GUI.color = new Color(0, 0, 0, 0.25f);
					}
					else
					{
						GUI.color = new Color(0, 0, 0, 0.125f);
					}
					GUI.DrawTexture(layoutRect, Texture2D.whiteTexture);
					GUI.color = Color.white;
				}

				GUILayout.Space(20);
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Space(40);
				foreach (var linkButton in linkButtons)
				{
					const int buttonSize = 96;
					Rect buttonRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(buttonSize), GUILayout.Height(buttonSize+20));
					Rect iconRect = buttonRect;
					iconRect.height = iconRect.width;
					EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);

					GUI.color = new Color(1, 1, 1, 0);
					if (GUI.Button(buttonRect, ""))
					{
						Application.OpenURL(linkButton.url);
					}
					GUI.color = Color.white;

					GUI.DrawTexture(iconRect, linkButton.texture);
					EditorGUILayout.BeginVertical();
					GUILayout.FlexibleSpace();
					BeginHorizontalCentered();
					GUILayout.Label(linkButton.title, linkButtonTextStyle);
					EndHorizontalCentered();
					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();
					GUILayout.Space(40);
				}
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				GUILayout.Space(20);
				EditorGUILayout.EndVertical();

				GUILayout.Space(20);
			}

			int columnWidth = Mathf.RoundToInt(window.maxSize.x/columns.Count);
			Rect columnsRect = EditorGUILayout.BeginHorizontal();
			for (int i = 0; i < columns.Count; i++)
			{
				Action column = columns[i];
				Rect columnRect = EditorGUILayout.BeginHorizontal(GUILayout.Width(columnWidth));
				GUILayout.Space(20);
				EditorGUILayout.BeginVertical();
				column();
				EditorGUILayout.EndVertical();
				GUILayout.Space(20);
				EditorGUILayout.EndHorizontal();

				if (i < columns.Count-1)
				{
					Rect dividerRect = columnsRect;
					dividerRect.x = columnRect.x+columnRect.width-1;
					dividerRect.width = 2;

					if (EditorGUIUtility.isProSkin)
					{
						GUI.color = new Color(0, 0, 0, 0.25f);
					}
					else
					{
						GUI.color = new Color(0, 0, 0, 0.125f);
					}
					GUI.DrawTexture(dividerRect, Texture2D.whiteTexture);
					GUI.color = Color.white;
				}
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(40);
			EditorGUILayout.EndVertical();

			if (Event.current.type == EventType.Repaint)
			{
				_requiredHeight = totalLayoutRect.y+totalLayoutRect.height;
			}
        }

        void IssueColumn()
        {
            DrawHeader("For best results with NeoFur, fix the following items:");

            GUILayout.Space(20);

            if (projectIssueNotifications.OnGUI())
            {
                BeginHorizontalCentered();
                GUILayout.Label("No issues detected.");
                EndHorizontalCentered();
            }
        }


		private void DrawHeader(string header)
        {
            BeginHorizontalCentered();
            GUILayout.Label(header, EditorStyles.boldLabel);
            EndHorizontalCentered();
        }

        private void BeginHorizontalCentered()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
        }

        private void EndHorizontalCentered()
        {
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
	}
}
