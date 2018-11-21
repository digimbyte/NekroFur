using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	internal class EditorWindowExtension
	{
		public delegate void OnRepaint();
		public event OnRepaint onRepaint;

		public void Repaint()
		{
			onRepaint?.Invoke();
		}
	}
}
