using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public static class NeoFurPluginInfo
	{
		public static readonly PluginBuildType buildType =
#if DEBUG
			PluginBuildType.Debug;
#else
			PluginBuildType.Release;
#endif
		private static Version _version;
		public static Version version
		{
			get
			{
				if (_version == null)
				{
					_version = typeof(NeoFurAsset).Assembly.GetName().Version;
				}
				return _version;
			}
		}
	}
}
