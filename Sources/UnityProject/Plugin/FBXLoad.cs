using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// This copy of NeoFur for Unity is licensed to anthony.magdelaine@blackantmaster.com

//fix ambig ref
using Debug	=UnityEngine.Debug;


namespace NeoFurUnityPlugin
{
	//A simple ascii fbx parser that only worries about getting curves and lines
	public class FBXLoad
	{
		[Serializable]
		public struct FBXLine
		{
			public Vector3	Start, End;
		}

		internal struct Setting
		{
			internal string	mTypeShort;
			internal string	mTypeLong;
			internal object	mValue;			//might be int or vec or something
		}

		internal class NurbsCurve
		{
			internal int			mOrder;
			internal bool			mbRational;
			internal List<float>	mPoints;
			internal List<int>		mKnotVector;
		}

		//settings for up axis, scale etc
		Dictionary<string, Setting>	mGlobalSettings	=new Dictionary<string, Setting>();

		//model specific settings
		Dictionary<string, Setting>	mModelSettings	=new Dictionary<string, Setting>();

		//nurbs
		List<NurbsCurve>	mNurbs	=new List<NurbsCurve>();

		//line segment data
		List<float>	mPoints		=new List<float>();
		List<int>	mIndexes	=new List<int>();

		List<FBXLine>	mProcessedLines	=new List<FBXLine>();


		public bool Load(string path)
		{
			FileStream	fs	=new FileStream(path, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				return	false;
			}

			StreamReader	sr	=new StreamReader(fs);
			if(sr == null)
			{
				fs.Close();
				return	false;
			}

			mPoints.Clear();
			mIndexes.Clear();
			mProcessedLines.Clear();

			while(true)
			{
				string	line	=sr.ReadLine();
				if(line == null || sr.EndOfStream)
				{
					break;
				}

				line	=line.TrimStart(' ', '\t');

				if(line.StartsWith(";"))
				{
					continue;	//comment
				}

				if(line.Contains("FBX Binary"))
				{
					return	false;	//binary format, fail
				}

				if(line.Contains("{"))
				{
					string sectionName	=line.Substring(0, line.IndexOf(':'));
					if(sectionName == "GlobalSettings")
					{
						ReadSettings(sr);
					}
					else if(sectionName == "Objects")
					{
						ReadSection(sectionName, sr);
					}
					else
					{
						ReadSection(sectionName, sr);
					}
				}

				if(line.Contains("}"))
				{
					break;
				}
			}
			sr.Close();
			fs.Close();

			if(mProcessedLines.Count > 0)
			{
				//was fbxlines
				return	true;
			}

			if(mNurbs.Count == 0)
			{
				//no nurbs either
				return	false;
			}

			ProcessNurbs();
			return	(mProcessedLines.Count > 0);
		}


		public List<FBXLine> GetLines()
		{
			return	mProcessedLines;
		}


		void ReadSection(string sectionName, StreamReader sr)
		{
			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				if(line.StartsWith(";"))
				{
					continue;	//comment
				}

				if(line.Contains("{"))
				{
					string secName	=line.Substring(0, line.IndexOf(':'));
					if(secName == "Points")
					{
						ReadPoints(line, sr);
					}
					else if(secName == "PointsIndex")
					{
						ReadIndexes(line, sr);
					}
					else if(secName == "Model")
					{
						ReadModel(sr);
					}
					else if(secName == "Geometry")
					{
						//see what type of geometry
						int		lastComma	=line.LastIndexOf(',');
						string	typeSZ		=line.Substring(lastComma + 1);

						typeSZ	=typeSZ.Trim(' ', '\"', '{');

						if(typeSZ == "NurbsCurve")
						{
							NurbsCurve	nc	=ReadNurbsCurve(sr, line);
							if(nc != null)
							{
								mNurbs.Add(nc);
							}
						}
						else
						{
							ReadSection(secName, sr);
						}
					}
					else
					{
						ReadSection(secName, sr);
					}
				}

				if(line.Contains("}"))
				{
					break;
				}
			}
		}


		void ReadPoints(string header, StreamReader sr)
		{
			ReadPoints(header, sr, mPoints);
		}


		void ReadPoints(string header, StreamReader sr, List<float> points)
		{
			string	numPoints	=header.Substring(header.IndexOf(':') + 1);

			numPoints	=numPoints.Trim(' ', '\t', '*', '{');

			int	nPoints;
			if(!int.TryParse(numPoints,
				System.Globalization.NumberStyles.Integer,
				System.Globalization.CultureInfo.InvariantCulture,
				out nPoints))
			{
				return;
			}

			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				//strip a: if there
				if(line.Contains("a:"))
				{
					line	=line.Substring(line.IndexOf(':') + 1);
				}

				//read out floats
				string	[]elements	=line.Split(',');
				foreach(string el in elements)
				{
					//read in double for a bit more precision
					//for the units conversion
					//TODO: proper unit conversion
					double	val;
					if(!double.TryParse(el,
						System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture,
						out val))
					{
						continue;
					}

					//fbx default is centimeters
					//all scale factors are relative to that
					//unity wants meters
					//no need to even use the settings scales
					//as the fbx exporter automatically puts all values
					//in centimeters
					points.Add((float)(val / 100.0));
				}

				if(line.Contains("}"))
				{
					break;
				}
			}

			if(nPoints != points.Count)
			{
				//warn here
				Debug.LogWarning("nPoints != mIndexes.Count in ReadPoints\n");
			}
		}


		void ReadKnots(string header, StreamReader sr, List<int> knots)
		{
			string	numKnots	=header.Substring(header.IndexOf(':') + 1);

			numKnots	=numKnots.Trim(' ', '\t', '*', '{');

			int	nKnots;
			if(!int.TryParse(numKnots,
				System.Globalization.NumberStyles.Integer,
				System.Globalization.CultureInfo.InvariantCulture,
				out nKnots))
			{
				return;
			}

			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				//strip a: if there
				if(line.Contains("a:"))
				{
					line	=line.Substring(line.IndexOf(':') + 1);
				}

				//read out ints
				string	[]elements	=line.Split(',');
				foreach(string el in elements)
				{
					int	val;
					if(!int.TryParse(el,
						System.Globalization.NumberStyles.Integer,
						System.Globalization.CultureInfo.InvariantCulture,
						out val))
					{
						continue;
					}
					knots.Add(val);
				}

				if(line.Contains("}"))
				{
					break;
				}
			}

			if(nKnots != knots.Count)
			{
				//warn here
				Debug.LogWarning("nPoints != mIndexes.Count in ReadPoints\n");
			}
		}


		//nurbs and such
		NurbsCurve ReadNurbsCurve(StreamReader sr, string header)
		{
			int		braceCount	=1;
			bool	mbValid		=true;

			NurbsCurve	nc	=new NurbsCurve();
			nc.mPoints		=new List<float>();
			nc.mKnotVector	=new List<int>();
			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				if(line.Contains("{"))
				{
					braceCount++;
				}
				else if(line.StartsWith("}"))
				{
					braceCount--;
					if(braceCount == 0)
					{
						break;
					}
				}

				if(line.StartsWith("Type:"))
				{
					if(!line.EndsWith("NurbsCurve\""))
					{
						Debug.LogError("Unsupported curve type : " + line);
						mbValid	=false;
					}
				}
				else if(line.StartsWith("Order:"))
				{
					string	[]bits	=line.Split(' ');
					if(!int.TryParse(bits[bits.Length - 1], out nc.mOrder))
					{
						Debug.LogError("Failed to parse NurbsCurve order.");
						mbValid	=false;
					}
				}
				else if(line.StartsWith("Dimension:"))
				{
					int		dim;
					string	[]bits	=line.Split(' ');
					if(!int.TryParse(bits[bits.Length - 1], out dim))
					{
						Debug.LogError("Failed to parse NurbsCurve dimension.");
						mbValid	=false;
					}
					if(dim != 3)
					{
						Debug.LogError("Unsupported NurbsCurve dimension: " + dim);
						mbValid	=false;
					}
				}
				else if(line.StartsWith("Form:"))
				{
					if(!line.EndsWith("Open\""))
					{
						Debug.LogError("Unsupported curve form : " + line);
						mbValid	=false;
					}
				}
				else if(line.StartsWith("Rational:"))
				{
					int		rat;
					string	[]bits	=line.Split(' ');
					if(!int.TryParse(bits[bits.Length - 1], out rat))
					{
						Debug.LogError("Failed to parse NurbsCurve rationality.");
						mbValid	=false;
					}
					nc.mbRational	=(rat != 0);
				}
				else if(line.StartsWith("Points:"))
				{
					ReadPoints(line, sr, nc.mPoints);
					braceCount--;
				}
				else if(line.StartsWith("KnotVector:"))
				{
					ReadKnots(line, sr, nc.mKnotVector);
					braceCount--;
				}
			}

			if(mbValid)
			{
				return	nc;
			}
			else
			{
				return	null;
			}
		}


		//TODO: This needs a lot more testing to make it robust
		//For instance what happens if there are multiple model
		//sections in a single file?
		void ReadModel(StreamReader sr)
		{
			mModelSettings.Clear();

			int	braceCount	=1;
			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				if(line.Contains("{"))
				{
					braceCount++;
				}
				else if(line.StartsWith("}"))
				{
					braceCount--;
					if(braceCount == 0)
					{
						return;
					}
				}
				if(!line.StartsWith("P:"))
				{
					continue;
				}

				//trim P:
				line	=line.Substring(3);

				Setting	set	=new Setting();

				string	[]toks	=line.Split(',');

				toks[0]	=toks[0].Trim('\"', ' ');
				toks[1]	=toks[1].Trim('\"', ' ');
				toks[2]	=toks[2].Trim('\"', ' ');

				set.mTypeShort	=toks[1];
				set.mTypeLong	=toks[2];

				//toks 4 -> should be the actual data
				switch(toks[1])
				{
					case	"bool":
						int	val;
						if(!int.TryParse(toks[4], out val))
						{
							//warn
						}
						set.mValue	=(val != 0);
						break;

					case	"enum":
						if(!int.TryParse(toks[4], out val))
						{
							//warn
						}
						set.mValue	=val;	//index of something?
						break;

					case	"KString":
						set.mValue	=toks[4].Trim('\"', ' ');
						break;

					case	"Vector3D":
						float	fVal0, fVal1, fVal2;
						if(!float.TryParse(toks[4], out fVal0))
						{
							//warn
						}
						if(!float.TryParse(toks[5], out fVal1))
						{
							//warn
						}
						if(!float.TryParse(toks[6], out fVal2))
						{
							//warn
						}
						set.mValue	=new Vector3(fVal0, fVal1, fVal2);
						break;

					case	"Lcl Rotation":
						if(!float.TryParse(toks[4], out fVal0))
						{
							//warn
						}
						if(!float.TryParse(toks[5], out fVal1))
						{
							//warn
						}
						if(!float.TryParse(toks[6], out fVal2))
						{
							//warn
						}
						set.mValue	=new Vector3(fVal0, fVal1, fVal2);
						break;

					case	"int":
						if(!int.TryParse(toks[4], out val))
						{
							//warn
						}
						set.mValue	=val;
						break;
				}

				mModelSettings.Add(toks[0], set);
			}
		}


		void ReadIndexes(string header, StreamReader sr)
		{
			string	numPoints	=header.Substring(header.IndexOf(':') + 1);

			numPoints	=numPoints.Trim(' ', '\t', '*', '{');

			int	nPoints;
			if(!int.TryParse(numPoints,
				System.Globalization.NumberStyles.Integer,
				System.Globalization.CultureInfo.InvariantCulture,
				out nPoints))
			{
				return;
			}

			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				//strip a: if there
				if(line.Contains("a:"))
				{
					line	=line.Substring(line.IndexOf(':') + 1);
				}

				//read out indexes
				string	[]elements	=line.Split(',');
				foreach(string el in elements)
				{
					int	val;
					if(!int.TryParse(el,
						System.Globalization.NumberStyles.Integer,
						System.Globalization.CultureInfo.InvariantCulture,
						out val))
					{
						continue;
					}

					mIndexes.Add(val);
				}

				if(line.Contains("}"))
				{
					break;
				}
			}

			if(nPoints == mIndexes.Count)
			{
				//process into lines
				ProcessLines();

				//clear unprocessed data
				mPoints.Clear();
				mIndexes.Clear();
			}
			else
			{
				//warn here
				Debug.LogWarning("nPoints != mIndexes.Count in ReadIndexes\n");
			}
		}


		//grab the settings for axisisisiiss and such
		void ReadSettings(StreamReader sr)
		{
			mGlobalSettings.Clear();

			int	braceCount	=1;
			while(true)
			{
				string	line	=sr.ReadLine();
				line			=line.TrimStart(' ', '\t');

				if(line.Contains("{"))
				{
					braceCount++;
				}
				else if(line.StartsWith("}"))
				{
					braceCount--;
					if(braceCount == 0)
					{
						return;
					}
				}
				if(!line.StartsWith("P:"))
				{
					continue;
				}

				//trim P:
				line	=line.Substring(3);

				Setting	set	=new Setting();

				string	[]toks	=line.Split(',');

				toks[0]	=toks[0].Trim('\"', ' ');
				toks[1]	=toks[1].Trim('\"', ' ');
				toks[2]	=toks[2].Trim('\"', ' ');

				set.mTypeShort	=toks[1];
				set.mTypeLong	=toks[2];

				//toks 4 -> should be the actual data
				switch(toks[1])
				{
					case	"int":
						int	val;
						if(!int.TryParse(toks[4], out val))
						{
							//warn
						}
						set.mValue	=val;
						break;

					case	"double":
						double	dVal;
						if(!double.TryParse(toks[4], out dVal))
						{
							//warn
						}
						set.mValue	=dVal;
						break;
				}

				mGlobalSettings.Add(toks[0], set);
			}
		}


		void ProcessNurbs()
		{
			List<float>	temp	=new List<float>();
			foreach(NurbsCurve nc in mNurbs)
			{
				//start
				temp.Add(nc.mPoints[0]);
				temp.Add(nc.mPoints[1]);
				temp.Add(nc.mPoints[2]);

				//end
				temp.Add(nc.mPoints[nc.mPoints.Count - 4]);
				temp.Add(nc.mPoints[nc.mPoints.Count - 3]);
				temp.Add(nc.mPoints[nc.mPoints.Count - 2]);

				ProcessLine(temp);
				temp.Clear();
			}
			mNurbs.Clear();
		}


		void ProcessLine(List<float> flist)
		{
			//only really interested in the start and end points
			if(flist.Count < 6)
			{
				//todo warn or something
				return;
			}

			FBXLine	line;

			//start
			line.Start.x	=flist[0];
			line.Start.y	=flist[1];
			line.Start.z	=flist[2];

			//get endpoint as an offset vector
			Vector3	ofs	=line.Start;

			ofs.x	=flist[flist.Count - 3] - ofs.x;
			ofs.y	=flist[flist.Count - 2] - ofs.y;
			ofs.z	=flist[flist.Count - 1] - ofs.z;

			if((int)mGlobalSettings["CoordAxis"].mValue == 0)
			{
				if((int)mGlobalSettings["CoordAxisSign"].mValue == 1)
				{
					line.Start.x	=-line.Start.x;
					ofs.x			=-ofs.x;
				}
			}

			Matrix4x4	adjust	=Matrix4x4.identity;

			//adjust coordinate system
			//Unity is a +Y up, +Z forward, +X right system
			if((int)mGlobalSettings["UpAxis"].mValue == 2)
			{
				//z up so need to rotate 90 x
				float	rotAmount	=((int)mGlobalSettings["FrontAxisSign"].mValue == 1)? 90.0f : -90.0f;

				adjust	*=Matrix4x4.TRS(Vector3.zero,
					Quaternion.Euler(Vector3.right * rotAmount),
					Vector3.one);
			}

			line.Start	=adjust.MultiplyPoint(line.Start);

			ofs	=adjust.MultiplyVector(ofs);

			line.End	=line.Start + ofs;

			mProcessedLines.Add(line);
		}


		void ProcessLines()
		{
			ProcessLines(mIndexes, mPoints);
		}


		void ProcessLines(List<int> indexes, List<float> points)
		{
			//parsing stuff
			List<float>	temp	=new List<float>();
			foreach(int idx in indexes)
			{
				if(idx < 0)
				{
					//end of line
					int	i	=(((-idx) - 1) * 3);
					temp.Add(points[i]);
					temp.Add(points[i + 1]);
					temp.Add(points[i + 2]);

					ProcessLine(temp);

					temp.Clear();
				}
				else if(idx >= 0 && idx < points.Count)
				{
					int	i	=(idx * 3);
					temp.Add(points[i]);
					temp.Add(points[i + 1]);
					temp.Add(points[i + 2]);
				}
			}
		}
        
		public void BakeModelTransform()
		{
			if(!mModelSettings.ContainsKey("Lcl Rotation"))
			{
				return;
			}

			Vector3	rot	=(Vector3)mModelSettings["Lcl Rotation"].mValue;

			Matrix4x4	adjust	=Matrix4x4.TRS(Vector3.zero,
				Quaternion.Euler(rot), Vector3.one);

			FBXLine	[]lines	=mProcessedLines.ToArray();

			mProcessedLines.Clear();

			for(int i=0;i < lines.Length;i++)
			{
				lines[i].Start	=adjust.MultiplyPoint(lines[i].Start);
				lines[i].End	=adjust.MultiplyPoint(lines[i].End);

				mProcessedLines.Add(lines[i]);
			}

			lines	=null;
		}
	}
}
