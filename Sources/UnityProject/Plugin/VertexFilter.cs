using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFurUnityPlugin
{
	public abstract class VertexFilter:System.IDisposable
	{
		public VertexProcessor processor { get; private set; }
		public bool hasRebuildOnce { get; private set; }

		public VertexFilter(VertexProcessor processor)
		{
			this.processor = processor;
		}

		public void Process()
		{
			OnProcess();
		}
		protected virtual void OnProcess()
		{

		}

		public void Rebuild()
		{
			OnRebuild();
			hasRebuildOnce = true;
		}
		protected virtual void OnRebuild()
		{

		}

		public void DebugDraw()
		{
			GUILayout.Label(GetType().Name);
			OnDebugDraw();
		}
		protected virtual void OnDebugDraw()
		{

		}

		public void BindToMaterial(Material material)
		{
			OnBindToMaterial(material);
		}
		protected virtual void OnBindToMaterial(Material material)
		{

		}

		public virtual void Dispose()
		{
			processor.RemoveFilter(this);
			processor = null;
		}
	}
}
