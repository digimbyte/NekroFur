using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public class VertexProcessorResource<T>:System.IDisposable where T:class
	{
		public T value { get; set; }
		public string key { get; private set; }
		public int referenceCount { get; private set; }

		public VertexProcessorResource(string key)
		{
			this.key = key;
		}

		public void AddReference()
		{
			referenceCount++;
		}

		public void RemoveReference()
		{
			referenceCount--;
		}

		public void Dispose()
		{
			if (value is System.IDisposable)
			{
				((System.IDisposable)value).Dispose();
			}

			if (value is Object)
			{
				Object.DestroyImmediate(value as Object);
			}
			
			value = null;
		}
	}
}
