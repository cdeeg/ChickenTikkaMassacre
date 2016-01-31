using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

//=================================================================================================================


namespace jUtility
{

	public static partial class Math
	{

		//=================================================================================================================

		#region curve maths

		public const int _defaultBezierResolution = 20;

		//	simple bezier spline interpolation
		public static Vector3 CalcBezier(float t, Vector3 p1, Vector3 a1, Vector3 a2, Vector3 p2)
		{
			Vector3 c = 3 * (a1 - p1);
			Vector3 b = 3 * (a2 - a1) - c;
			Vector3 a = p2 - p1 - c - b;
			
			float Cube = t * t * t;
			float Square = t * t;
			
			Vector3 p = (a * Cube) + (b * Square) + (c * t) + p1;
			return p;
		}

		//------------------------------------------

		public static float CalcBezierLength(Vector3 p1, Vector3 a1, Vector3 a2, Vector3 p2, 
		                                     int precisionSteps=_defaultBezierResolution)
		{
			float t 			= 0;
			float length 		= 0;
			Vector3	point		= Vector3.zero;
			Vector3 lastPoint	= Vector3.zero;

			for(int i = 0; i < precisionSteps; i++)
			{
				t = (float)i / (float)precisionSteps;
				point = CalcBezier( t, p1, a1, a2, p2 );
				if(i > 0)
				{
					length += Vector3.Distance(point, lastPoint);
				}
				lastPoint = point;
			}
			return length;
		}

		#endregion


		//=================================================================================================================


		#region curve

		//	the bezier curve consists of several segments that can be manipulated (break tangents, smooth etc.)
		public class BezierCurve
		{
			public const float 	_defaultTangentLength 	= 0.35f;
			public const int	_defaultBezierCapacity	= 15;

			float 				tangentLength	= _defaultTangentLength;					//	range 0-0.45f; the higher value steepens the curve, 0 yields a linear representation
			int					precision 		= _defaultBezierResolution;					//	number of samples taken per segment

			public float 		CurveLength 		{ get; private set; }					//	length of the curve approx; depends on precision
			public float		CurveLength2D		{ get; private set; }					//	2d length of the curve approx; depends in precision 
			public int			SegmentCount 		{ get; private set;}					//	number of currently used segments
			public int			Capacity			{ get { return segments.Capacity; } }	//	capacity of the segment list
			public int			SamplingPrecision	{ get { return precision; } }			

			public List<BezierSegment> segments;

			//------------------------------------------

			//	zero constructor
			public BezierCurve( int capacity=_defaultBezierCapacity, float tangents = _defaultTangentLength )
			{
				segments 		= new List<BezierSegment>(capacity);
				SegmentCount 	= 0;
				tangentLength	= tangents;
			}

			//	path constructor List<>
			public BezierCurve( List<Vector3> positions, int capacity=_defaultBezierCapacity, float tangents=_defaultTangentLength )
			{
				segments 		= new List<BezierSegment>(positions.Count-1 < capacity ? capacity : capacity*2);
				for(int i=0; i < positions.Count-1; i++)
				{
					segments.Add( new BezierSegment(positions[i], positions[i+1], tangents) );		//	this does not test for double positions!
				}
				SegmentCount 	= segments.Count;
				tangentLength	= tangents;
				RecalcAnchors();
			}
			//	path constructor Array[]
			public BezierCurve( Vector3[] positions, int capacity=_defaultBezierCapacity, float tangents=_defaultTangentLength )
			{
				segments 		= new List<BezierSegment>(positions.Length-1 < capacity ? capacity : capacity*2);
				for(int i=0; i < positions.Length-1; i++)
				{
					segments.Add( new BezierSegment(positions[i], positions[i+1], tangents) );		//	this does not test for double positions!
				}
				SegmentCount 	= segments.Count;
				tangentLength 	= tangents;
				RecalcAnchors();
			}

			//	copy contructor
			public BezierCurve( List<BezierSegment> src, int capacity=_defaultBezierCapacity, float tangents=_defaultTangentLength )
			{
				segments = new List<BezierSegment>(src.Count < capacity ? capacity : capacity*2);
				for(int i = 0; i < segments.Count; i++)
					segments.Add( src[i].Clone() );
				SegmentCount 	= segments.Count;
				tangentLength 	= tangents;
			}



            //	settings
            public void SetTangentLength(float length=_defaultTangentLength) 	{ tangentLength = length; }
			public void SetBezierResolution(int res=_defaultBezierResolution) 	{ precision = res; }

			//------------------------------------------

			/// <summary>
			/// returns a list of positions sampled from the curve
			/// </summary>
			public List<Vector3> GetSampledCurve()
			{
				return GetSampledCurve(precision*SegmentCount);
			}
			/// <summary>
			/// returns a list of positions sampled from the curve
			///	precision(number of samples) can be adjusted or left to default setting
			/// </summary>
			public List<Vector3> GetSampledCurve(int samplingPrecision)
			{
				List<Vector3> sample = new List<Vector3>();
				for(int i=0; i < samplingPrecision; i++)
				{
					float t = (float)i / (float)samplingPrecision;
					sample.Add( GetPointOnCurve(t) );
				}
				return sample;
			}
			/// <summary>
			/// Updates an existing sample list
			/// </summary>
			public void GetSampledCurve(List<Vector3> result)
			{
				GetSampledCurve(result, precision*SegmentCount);
			}
			/// <summary>
			/// Updates an existing sample list
			/// precision(number of samples) can be adjusted or left to default setting
			/// </summary>
			public void GetSampledCurve(List<Vector3> result, int samplingPrecision)
			{
				if(result == null)
					result = new List<Vector3>(samplingPrecision);
				else
					result.Clear();

				for(int i=0; i < samplingPrecision; i++)
				{
					float t = (float)i / (float)samplingPrecision;
					result.Add( GetPointOnCurve(t) );
				}
			}

			/// <summary>
			/// Updates an existing sample list
			/// precision(number of samples) can be adjusted or left to default setting
			/// The method also receives an assumed breakpoint on the curve and returns its corresponding index in the sampled result
			/// </summary>
			public int GetSampledCurve(List<Vector3> result, Vector3 breakPoint, int samplingPrecision, int keepBegin=0)
			{
				if(result == null)
					result = new List<Vector3>(samplingPrecision);
				else
					result.Clear();

				int breakIndex = 0;
				Vector3 lastP = (result.Count > 0) ? result[0] : Vector3.zero;
				for(int i=keepBegin; i < samplingPrecision; i++)
				{
					float t 	= (float)i / (float)samplingPrecision;
					Vector3 p 	= GetPointOnCurve(t);
					result.Add(p);

					if(lastP.SqrDistance2f(breakPoint) + p.SqrDistance2f(breakPoint) <= p.SqrDistance2f(lastP) + 0.1f)
					{
						breakIndex = i;
					}
				}
				return breakIndex;
			}

			//------------------------------------------

			public Vector3 GetPointOnCurve(float t)
			{
				t = Mathf.Clamp(t, 0, 1);
				BezierSegment segment = GetSegmentByT(t);
				if(segment != null)
				{
					float segmentT = MapF( t-segment.CurveStartT, 0, segment.CurveEndT-segment.CurveStartT, 0, 1, true );
					return segment.GetPointOnSegment(segmentT);
				}
				return Vector3.zero;
			}

			/// <summary>
			/// returns the next point on the curve determined by a speed value
			///	speedT (agentSpeed / curveLength) must be precalculated beforehand to ensure consistent movement,
			///	otherwise floatingpoint errors will result in a movement jittering
			/// </summary>
			/// <returns>The next point on curve.</returns>
			/// <param name="t">T.</param>
			/// <param name="speedT">Speed t.</param>
			/// <param name="nextT">Next t.</param>
			/// <param name="maxMove">Max move.</param>
			/// <param name="prevPos">Previous position.</param>
			public Vector3 GetNextPointOnCurve(float t, float speedT, out float nextT, float movementSpeed, Vector3 prevPos)
			{
				nextT = Mathf.Clamp( t + speedT, 0f, 1f );
				Vector3 nextPos = GetPointOnCurve(nextT);
				Vector3 v = nextPos - prevPos;
				if(v.magnitude > movementSpeed)
				{
					float factor 	= v.magnitude / movementSpeed;
					speedT			*= factor;
					nextT 			= Mathf.Clamp( t + speedT, 0f, 1f );
					nextPos 		= GetPointOnCurve(nextT);
				}
				return nextPos;
			}

			public float GetSpeedT(float speed)
			{
				return speed / CurveLength;
			}

			//------------------------------------------

			/// <summary>
			/// returns point on the curve nearest to given position
			/// </summary>
			public Vector3 GetNearestPointOnCurve(Vector3 pos)
			{
				return GetNearestPointOnCurve(pos, precision);
			}
			/// <summary>
			/// returns point on the curve nearest to given position as well as its segment index
			/// </summary>
			public Vector3 GetNearestPointOnCurve(Vector3 pos, out int segmentIndex)
			{
				return GetNearestPointOnCurve(pos, out segmentIndex, precision);
			}
			/// <summary>
			/// returns point on the curve nearest to given position, with an optional precision param (curve resolution while testing)
			/// </summary>
			public Vector3 GetNearestPointOnCurve(Vector3 pos, int samplingPrecision)
			{
				int index;
				return GetNearestPointOnCurve(pos, out index, samplingPrecision);
			}
			/// <summary>
			/// returns point on the curve nearest to given position as well as its segment index, 
			/// with an optional precision param (curve resolution while testing).
			/// </summary>
			public Vector3 GetNearestPointOnCurve(Vector3 pos, out int segmentIndex, int samplingPrecision)
			{
				segmentIndex 	= -1;
				float minDist 	= float.MaxValue;
				Vector3 nearest	= Vector3.zero;
				for(int i = 0; i < SegmentCount; i++)
				{
					Vector3 p = segments[i].GetNearestPointOnSegment(pos, precision);
					float mag = Vector3.Distance(pos, p);
					if(mag < minDist)
					{
						segmentIndex 	= i;
						minDist 		= mag;
						nearest 		= p;
					}
				}
				return nearest;
			}

			//------------------------------------------

			/// <summary>
			/// returns forward direction from given normalized curve position
			/// </summary>
			public Vector3 GetDirectionOnCurve(float t)
			{
				Vector3 p0 = GetPointOnCurve(t-0.001f);
				Vector3 p1 = GetPointOnCurve(t+0.001f);
				return (p1-p0).normalized;
			}

			public Quaternion GetRotationOnCurve(float t)
			{
				Vector3 dir = GetDirectionOnCurve(t);
				return Quaternion.LookRotation(dir);
			}

			//------------------------------------------

			/// <summary>
			/// Return closest T value on curve to given pos
			/// </summary>
			/// <returns>The T by position.</returns>
			/// <param name="pos">Position.</param>
			public float GetTByPos(Vector3 pos)
			{
				for(int i = 0; i < segments.Count; i++)
				{
					float t = segments[i].GetTByPos(pos, precision);
					if(i == 0 && t == 0f)
						return 0;
					else if(i == segments.Count && t == 1f)
						return 1;
					else if(t != 0f && t != 1f)
						return t;
				}
				return 0;
			}

			//------------------------------------------

			/// <summary>
			/// recalculate anchor positioning without broken tangents
			/// </summary>
			public void RecalcAnchors()
			{
				if(SegmentCount > 1)
				{
					Vector3 dirIN 	= Vector3.zero;
					Vector3 dirOUT	= Vector3.zero;

					for(int i = 0; i < SegmentCount; i++)
					{
						//	first segment
						if(i == 0)
						{
							dirIN 	= segments[i].segmentIN_nrm;
							dirOUT	= (segments[i].p1 - segments[i+1].p2).normalized;
						}
						//	last segment
						else if(i == SegmentCount-1)
						{
							dirIN	= (segments[i].p2 - segments[i-1].p1).normalized;
							dirOUT	= segments[i].segmentOUT_nrm;
						}
						//	inner segments
						else if(i > 0 && i < SegmentCount-1)
						{
							dirIN 	= (segments[i].p2 - segments[i-1].p1).normalized;
							dirOUT	= (segments[i].p1 - segments[i+1].p2).normalized;
						}

						//	set tangents
						segments[i].a1 = segments[i].p1 + (dirIN  * tangentLength * segments[i].Magnitude);
						segments[i].a2 = segments[i].p2 + (dirOUT * tangentLength * segments[i].Magnitude);
					}
				}

				RecalcLength();
			}

			public void SetAnchor(int segmentIndex, Vector3 anchorA, Vector3 anchorB)
			{
				if(segmentIndex >= 0 && segmentIndex < SegmentCount)
				{
					if(segmentIndex != 0)
						segments[segmentIndex].a1 = anchorA;
					if(segmentIndex != SegmentCount-1)
						segments[segmentIndex].a2 = anchorB;
				}
			}

			public void SetAnchor(int nodeIndex, Vector3 anchor)
			{
				int count = SegmentCount * 2;
				if(nodeIndex >= 0 && nodeIndex < count)
				{
					int segmentIndex = (nodeIndex - (nodeIndex%2)) / 2;
					Debug.Log("node index: " + nodeIndex + " segment index: " + segmentIndex);

					if(nodeIndex % 2 == 0)
					{
						segments[segmentIndex].a1 = anchor;
					}
					else
					{
						segments[segmentIndex].a2 = anchor;
//						if(segmentIndex == SegmentCount-1)
//						{
//							segments[segmentIndex].a1 = anchor;
//						}
					}
				}
			}

			/// <summary>
			/// calculate curve length and segment t values
			/// </summary>
			public void RecalcLength()
			{
				//	recalc curve length
				int i;
				CurveLength 	= 0;
				CurveLength2D	= 0;
				for(i = 0; i < SegmentCount; i++)
				{
					segments[i].Recalc(precision);
					CurveLength 	+= segments[i].Length;
					CurveLength2D	+= segments[i].Length2D;
				}

				if(CurveLength > 0)
				{
					//	recalc segment start/ends in relation to whole curve
					float currT = 0;
					for(i = 0; i < SegmentCount; i++)
					{
						segments[i].CurveStartT = currT;
						segments[i].CurveEndT 	= currT + segments[i].Length / CurveLength;
						currT					= segments[i].CurveEndT;
					}
					if(SegmentCount > 0 && segments.Count > 0)
					{
						segments[SegmentCount-1].CurveEndT = 1;
					}
				}
			}

			//------------------------------------------

			/// <summary>
			/// given an index and a new list of points, the curve is recalced without the need
			///	to create a full new bezier object
			/// </summary>
			public void ChangeCurve(List<Vector3> positions)
			{
				if(positions == null)
					return;
				
				//	set curve to 0
				if(positions.Count == 0)
				{
					SegmentCount = 0;
					return;
				}

				//	2 positions is always a straight line;
				//	prevents weird sampling on updating a single segment curve
				if(positions.Count==2)
				{
					if(segments.Count == 0)
					{
						segments = new List<BezierSegment>();
						segments.Add(new BezierSegment(positions[0], positions[1]));
					}
					SegmentCount = 1;
					segments[0].p1 = positions[0];
					segments[0].p2 = positions[1];
					segments[0].a1 = positions[0] + (positions[1] - positions[0]).normalized * tangentLength;
					segments[0].a2 = positions[1] + (positions[0] - positions[1]).normalized * tangentLength;
					RecalcLength();
					segments[0].CurveStartT = 0; 
					segments[0].CurveEndT = 1;
					return;
				}
				// 	new positions need equal/fewer segments than currently used,
				//	only adjust positions
				else if(positions.Count-1 <= segments.Count)
				{
					SegmentCount = positions.Count-1;
					for(int i = 0; i < SegmentCount; i++)
					{
						segments[i].p1 = positions[i];
						segments[i].p2 = positions[i+1];
					}
				}
				//	new positions need more segments than currently used
				else
				{
					if(positions.Count-1 > Capacity)
					{
						segments.Capacity = Capacity*5 < positions.Count-1 ? Capacity*10 : Capacity*2;
						Debug.Log(RichTextUtil.color("Bezier Warning:: ", Color.yellow) 
						          + "\ncapacity has been exceeded. New Capacity: " + Capacity + "( for " + positions.Count + " segments)");
					}
					int i;
					for(i = 0; i < segments.Count; i++)
					{
						segments[i].p1 = positions[i];
						segments[i].p2 = positions[i+1];
					}
					for(/*i=i*/; i < positions.Count-1; i++)
					{
						segments.Add(new BezierSegment(positions[i], positions[i+1]));
					}
					SegmentCount = segments.Count;
				}

				//	anchors
				RecalcAnchors();
			}

			//------------------------------------------
			
			public IEnumerable GetSegments()
			{
				foreach(BezierSegment segment in segments)
					yield return segment;
			}

			//	returns a segment on the curve depending on T
			BezierSegment GetSegmentByT(float t)
			{
				t = Mathf.Clamp(t, 0f, 1f);
				for(int i = 0; i < SegmentCount; i++)
				{
					if(t >= segments[i].CurveStartT && t <= segments[i].CurveEndT)
						return segments[i];
				}
				return null;
			}

			//------------------------------------------

			//	returns original positions as SerializedVector3 string format, separated by a ';'
			public SerializableVector3[] ToSerializablePositions()
			{
				List<SerializableVector3> result = new List<SerializableVector3>(segments.Count+1);
				for(int i = 0; i < SegmentCount; i++)
				{
					result.Add(new SerializableVector3(segments[i].p1));
					if(i == SegmentCount-1)
					{
						result.Add(new SerializableVector3(segments[i].p2));
					}
				}
				return result.ToArray();
			}

			//------------------------------------------

			//=================================================================================================================
		
			//	a bezier segment consists of two line points and 
			//	two matching anchors to interpolate the curve 
			public class BezierSegment
			{
				public Vector3 p1;
				public Vector3 p2;
				public Vector3 a1;
				public Vector3 a2;

				public float   	CurveStartT;
				public float   	CurveEndT;
				public float   	Length			{ get; private set; }
				public float 	Length2D		{ get; private set; }
				public float   	Magnitude		{ get { return (p2-p1).magnitude; } }

				public Vector3 segmentIN		{ get { return p2-p1; } }
				public Vector3 segmentOUT		{ get { return p1-p2; } }
				public Vector3 segmentIN_nrm	{ get { return segmentIN.normalized; } }
				public Vector3 segmentOUT_nrm	{ get { return segmentOUT.normalized; } }

				//------------------------------------------

                ///// <summary>
                ///// Used for deserialization.
                ///// Ctor does nothing, so all fields have their default value.
                ///// </summary>
                //public BezierSegment()
                //{}

				public BezierSegment( Vector3 p1, Vector3 p2, float tangents = BezierCurve._defaultTangentLength )
				{
					this.p1 = p1;
					this.p2 = p2;
					this.a1 = p1 + (p2-p1).normalized * tangents * Magnitude;
					this.a2 = p2 + (p1-p2).normalized * tangents * Magnitude;
				}

                //public void Serialize(NetworkWriter writer)
                //{
                //    writer.Write(p1);
                //    writer.Write(p2);
                //    writer.Write(a1);
                //    writer.Write(a2);
                //    writer.Write(CurveStartT);
                //    writer.Write(CurveEndT);
                //    writer.Write(Length);
                //    writer.Write(Length2D);
                //}

                //public void Deserialize(NetworkReader reader)
                //{
                //    p1 = reader.ReadVector3();
                //    p2 = reader.ReadVector3();
                //    a1 = reader.ReadVector3();
                //    a2 = reader.ReadVector3();
                //    CurveStartT = reader.ReadSingle();
                //    CurveEndT = reader.ReadSingle();
                //    Length = reader.ReadSingle();
                //    Length2D = reader.ReadSingle();
                //}

                public BezierSegment Clone()
				{
					return this.MemberwiseClone() as BezierSegment;
				}

				//------------------------------------------

				public void Recalc(int precision)
				{
					Length 		= CalcBezierLength(p1, a1, a2, p2);
					Length2D	= CalcBezierLength(	new Vector3(p1.x,0,p1.z), new Vector3(a1.x,0,a1.z), 
					                            	new Vector3(a2.x,0,a2.z), new Vector3(p2.x,0,p2.z));
				}

				//------------------------------------------
				
				public Vector3 GetPointOnSegment(float t)
				{
					t = Mathf.Clamp(t, 0f, 1f);
					return CalcBezier( t, p1, a1, a2, p2 );
				}

				public Vector3 GetNearestPointOnSegment(Vector3 pos, int precision)
				{
					float 	minDist = float.MaxValue;
					Vector3 nearest = Vector3.zero;
					for(int i=0; i < precision; i++)
					{
						float t 	= (float)i / (float)precision;
						Vector3 p 	= GetPointOnSegment(t);
						float mag	= Vector3.Distance(p, pos);
						if(mag < minDist)
						{
							minDist = mag;
							nearest = p;
						}
					}
					return nearest;
				}

				public float GetTByPos(Vector3 pos, int precision)
				{
					float 	minDist = float.MaxValue;
					float 	bestT	= 0;
					for(int i=0; i < precision; i++)
					{
						float t 	= (float)i / (float)precision;
						Vector3 p 	= GetPointOnSegment(t);
						float mag	= Vector3.Distance(p, pos);
						if(mag < minDist)
						{
							minDist = mag;
							bestT	= t;
						}
					}
					return bestT;
				}

				//------------------------------------------

				//	given a number of lines to form a polygon, 
				//	the anchors of this segment get shortened to fit into the polygon constraint  
				public void ClampAnchors( List<Vector3[]> polygon )
				{
					return;

					//	anchor a1
					//foreach( Vector3[] line in polygon )
					//{
					//	Vector3[] anchorLine 	= new Vector3[] { p1, a1 };
					//	Vector3 intersection;
					//	if(GetPointOfLineIntersection3f( anchorLine[0], anchorLine[1], line[0], line[1], out intersection))
					//	{
					//		Vector3 v1				= intersection - p1;
					//		Vector3 v2				= a1 - p1;
					//		if( v1.magnitude < v2.magnitude )
					//		{
					//			a1 = p1 + v1;
					//			break;
					//		}
					//	}
					//}

					////	anchor a2
					//foreach( Vector3[] line in polygon )
					//{
					//	Vector3[] anchorLine 	= new Vector3[] { p2, a2 };
					//	Vector3 intersection;
					//	if(GetPointOfLineIntersection3f( anchorLine[0], anchorLine[1], line[0], line[1], out intersection))
					//	{
					//		Vector3 v1				= intersection - p2;
					//		Vector3 v2				= a2 - p2;
					//		if( v1.magnitude < v2.magnitude )
					//		{
					//			a2 = p2 + v1;
					//			break;
					//		}
					//	}
					//}
				}

				//------------------------------------------

				public bool Compare(Vector3 a, Vector3 b)
				{
					return jUtility.Math.SloppyEquals_3to2f(a, p1) && jUtility.Math.SloppyEquals_3to2f(b, p2);
				}

                
            }

		}

		#endregion
		
		//-----------------------------------------------------------------------------------------------
	}

}


//=================================================================================================================