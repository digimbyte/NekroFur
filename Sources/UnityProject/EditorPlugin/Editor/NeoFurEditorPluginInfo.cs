using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public static class NeoFurEditorPluginInfo
	{
		public static readonly PluginBuildType buildType =
#if DEBUG
			PluginBuildType.Debug;
#else
			PluginBuildType.Release;
#endif
	}
}
