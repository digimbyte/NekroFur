using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public static class VertexProcessorCache
	{
		private static Dictionary<string, object> resourceDict = new Dictionary<string, object>();

		public static VertexProcessorResource<T> GetResource<T>(string key) where T:class
		{
			object resourceObject;
			if (!resourceDict.TryGetValue(key, out resourceObject))
			{
				resourceObject = new VertexProcessorResource<T>(key);
				resourceDict.Add(key, resourceObject);
			}
			VertexProcessorResource<T> resource = (VertexProcessorResource<T>)resourceObject;
			resource.AddReference();
			return resource;
		}

		public static void ReleaseResource<T>(VertexProcessorResource<T> resource) where T:class
		{
			if (!resourceDict.ContainsKey(resource.key))
			{
				throw new System.Exception("Cannot release resource with key \""+resource.key+"\" because it is not referenced.");
			}

			resource.RemoveReference();
			if (resource.referenceCount <= 0)
			{
				resourceDict.Remove(resource.key);
				resource.Dispose();
				
			}
		}
	}
}
