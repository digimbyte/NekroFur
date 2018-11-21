using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections.ObjectModel;
using NeoFurUnityPlugin;
using System.Reflection;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.Editor
{
	class NeoSuiteSplashWindow : EditorWindow
	{
		private static string showAutomaticallyKey = "NeoSuite_SplashWindow_ShowAutomaticallyVersion";
		private static bool showAutomatically
		{
			get
			{
				if (!EditorPrefs.HasKey(showAutomaticallyKey))
				{
					return true;
				}
				string storedVersion = EditorPrefs.GetString(showAutomaticallyKey);
				return storedVersion != NeoFurPluginInfo.version.ToString();
			}
			set
			{
				if (value)
				{
					EditorPrefs.DeleteKey(showAutomaticallyKey);
				}
				else
				{
					EditorPrefs.SetString(showAutomaticallyKey, NeoFurPluginInfo.version.ToString());
				}
			}
		}

		private static List<Type> tabTypes = new List<Type>();
        private List<NeoSuiteSplashTab> m_neoSuiteTabs = new List<NeoSuiteSplashTab>();
        private const float minHeight = 300;

        private int ActiveTabIndex
        {
            get
            {
                return EditorPrefs.GetInt("NeoSuiteSplashWindow_CurrentTab", 0);
            }

            set
            {
                int v = value;
                if (v > m_neoSuiteTabs.Count || v < 0)
                {
                    v = 0;
                }
                EditorPrefs.SetInt("NeoSuiteSplashWindow_CurrentTab", v);
            }
        }

		private NeoSuiteSplashTab activeTab
		{
			get
			{
				if (m_neoSuiteTabs.Count <= 0)
				{
					return null;
				}

				return m_neoSuiteTabs[ActiveTabIndex];
			}
		}

        [MenuItem("Window/Neoglyphic", false, int.MaxValue)]
		[MenuItem("Neoglyphic/Welcome Screen!", false, int.MaxValue)]
		private static void Init()
		{
			EditorWindow window = GetWindow<NeoSuiteSplashWindow>(true, "Neoglyphic: Welcome!", true);
			window.minSize = new Vector2(768, minHeight);
			window.maxSize = new Vector2(768, minHeight);
			window.Show();
			window.minSize = new Vector2(768, minHeight);
			window.maxSize = new Vector2(768, minHeight);
		}

		[InitializeOnLoadMethod]
		private static void InitOnLoad()
		{
            EditorApplication.update += InitOnce;
		}

		private static void InitOnce()
		{
            // show if license info needs to be entered?
            if (showAutomatically)
			{
				Init();
				showAutomatically = false;
			}
			EditorApplication.update -= InitOnce;
		}

		public static void RegisterTabType(Type type)
		{
			if (!typeof(NeoSuiteSplashTab).IsAssignableFrom(type))
			{
				throw new Exception($"type \"{type}\" is not a {nameof(NeoSuiteSplashTab)}.");
			}

			if (!tabTypes.Contains(type))
			{
				tabTypes.Add(type);
			}
		}

        void OnGUI()
        {
            if (activeTab == null)
			{
				return;
			}
			
            if (m_neoSuiteTabs.Count > 1)
            {
				//Draw tabs here.
            }

			activeTab.OnGUI();

			float newHeight = Math.Max(activeTab.requiredHeight, minHeight);
			minSize = new Vector2(768, newHeight);
			maxSize = minSize;
		}

		private void OnEnable()
        {
			m_neoSuiteTabs.Clear();

			foreach (var tabType in tabTypes)
			{
				NeoSuiteSplashTab tab = (NeoSuiteSplashTab)Activator.CreateInstance(tabType, new object[] { this });
				m_neoSuiteTabs.Add(tab);
			}

			m_neoSuiteTabs.Sort((a, b) => a.Priority.CompareTo(b));

			foreach (NeoSuiteSplashTab tab in m_neoSuiteTabs)
			{
				tab.OnEnable();
			}
		}

        private void OnDisable()
        {
            foreach(NeoSuiteSplashTab tab in m_neoSuiteTabs)
            {
                tab.OnDisable();
            }

			m_neoSuiteTabs.Clear();
        }
	}
}
