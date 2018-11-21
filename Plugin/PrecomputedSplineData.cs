using NeoFurUnityPlugin;
using UnityEngine;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

namespace NeoFur.Data
{
    public class PreComputedGuideData : ScriptableObject
    {
        /// <summary>
        /// Keep this as a relative path to the Assets folder so it
        /// doesnt break across different machines.
        /// </summary>
        public string SplineDataFile;

        /// <summary>
        /// Mesh to use with splines when generating guides
        /// </summary>
        public Mesh sourceMesh;
        
        /// <summary>
        /// The generated guides
        /// </summary>
        public Vector3[] guides;

		public void UpdateGuides(FBXLoad.FBXLine[] lines)
		{
			Vector3[] vertices = sourceMesh.vertices;
			guides = new Vector3[vertices.Length];

			for (int i = 0; i < vertices.Length; ++i)
			{
				guides[i] = GetGuideVert(vertices[i], lines);
			}
		}

		private static Vector3 GetGuideVert(Vector3 vert, FBXLoad.FBXLine[] fbxLines)
		{
			Vector3 ret = Vector3.zero;

			float firstDist = float.MaxValue;
			float secondDist = float.MaxValue;

			FBXLoad.FBXLine firstLine = new FBXLoad.FBXLine();
			FBXLoad.FBXLine secondLine = new FBXLoad.FBXLine();

			for (int i = 0; i < fbxLines.Length; ++i)
			{
				float dist = Vector3.Distance(vert, fbxLines[i].Start);
				if (dist < firstDist)
				{
					firstDist = dist;
					firstLine = fbxLines[i];
				}
				else if (dist < secondDist)
				{
					secondDist = dist;
					secondLine = fbxLines[i];
				}
			}

			if (firstDist == 0)
			{
				ret = firstLine.End - firstLine.Start;
			}
			else
			{
				Vector3 first = firstLine.End - firstLine.Start;
				Vector3 second = secondLine.End - secondLine.Start;

				float ratio = secondDist / firstDist;

				ratio = 1.0f / (ratio + 1.0f);
				ret = Vector3.Lerp(first, second, ratio);
			}

			return ret;
		}
	}
}
