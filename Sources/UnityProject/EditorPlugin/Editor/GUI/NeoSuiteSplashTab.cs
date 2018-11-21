using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace Neoglyphic.Editor
{
    internal abstract class NeoSuiteSplashTab
    {
        public NeoSuiteSplashTab(NeoSuiteSplashWindow window)
		{
			this.window = window;
		}

		public NeoSuiteSplashWindow window { get; private set; }

		//used to determine tab sorting
		public abstract float Priority { get; }

		public abstract float requiredHeight { get; }

		//used to render content in tab
		public abstract void OnGUI();
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}
