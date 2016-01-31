using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//=================================================================================================================

namespace jUtility
{

    public static partial class Math
    {
        //-----------------------------------------------------------------------------------------------

        //	equals

        const float defaultFloatingTolerance = 0.0001f;

        /// <summary>
        /// Test if floating point a equals b with a tolerance
        /// </summary>
        public static bool SloppyEquals_f(float a, float b, float tolerance = defaultFloatingTolerance)
        {
            return !(b < a - tolerance || b > a + tolerance);
        }
        /// <summary>
        /// Test if Vector2 a equals b with a tolerance
        /// </summary>
        public static bool SloppyEquals_2f(Vector2 a, Vector2 b, float tolerance = defaultFloatingTolerance)
        {
            return SloppyEquals_f(a.x, b.x, tolerance) && SloppyEquals_f(a.y, b.y, tolerance);
        }
        /// <summary>
        /// Test if Vector3 a equals b with a tolerance
        /// </summary>
        public static bool SloppyEquals_3f(Vector3 a, Vector3 b, float tolerance = defaultFloatingTolerance)
        {
            return 	SloppyEquals_f(a.x, b.x, tolerance) && 
					SloppyEquals_f(a.y, b.y, tolerance) && 
					SloppyEquals_f(a.z, b.z, tolerance);
        }
        /// <summary>
        /// Test if Vector3 a equals b with a tolerance on XZ plane, ignoring y value
        /// </summary>
        public static bool SloppyEquals_3to2f(Vector3 a, Vector3 b, float tolerance = defaultFloatingTolerance)
        {
            return SloppyEquals_f(a.x, b.x, tolerance) && SloppyEquals_f(a.z, b.z, tolerance);
        }

        //-----------------------------------------------------------------------------------------------

        /// <summary>
        /// standard range mapping
        /// </summary>
        public static float MapF(float val, float min1, float max1, float min2, float max2, bool clamp = true)
        {
            if (clamp)
                val = Mathf.Clamp(val, min1, max1);
            return ((max2 - min2) * ((val - min1) / (max1 - min1))) + min2;
        }

        //-----------------------------------------------------------------------------------------------

        //	angles

        public static float GetVector2AngleBetween(Vector2 a, Vector2 b)
        {
            float angle = Vector2.Angle(a, b);
            Vector3 cross = Vector3.Cross(a, b);

            if (cross.z > 0)
                angle = 360 - angle;
            return angle;
        }

        //-----------------------------------------------------------------------------------------------

        /// <summary>
        /// hypothetical 2D cross product
        /// </summary>
        public static float Cross2D2f(Vector2 v1, Vector2 v2)
        {
            return (v1.x * v2.y) - (v1.y * v2.x);
        }

        /// <summary>
        /// hypothetical 2D cross product with vector3s
        /// </summary>
        public static float Cross2D3f(Vector3 v1, Vector3 v2)
        {
            return (v1.x * v2.z) - (v1.z * v2.x);
        }

        //-----------------------------------------------------------------------------------------------

        /// <summary>
        /// returns true if two given lines intersect at any point [Vector3]
        /// </summary>
        public static bool LinesIntersect3f(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return LinesIntersect2f(new Vector2(a.x, a.z), new Vector2(b.x, b.z), new Vector2(c.x, c.z), new Vector2(d.x, d.z));
        }

        //	returns true if two given lines intersect at any point [Vector2]
        public static bool LinesIntersect2f(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float numerator = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
            float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));

            if (denominator == 0 && numerator != 0)
                // Lines are parallel.
                return false;

            // Lines are collinear or intersect at a single point.
            return true;
        }

        //-----------------------------------------------------------------------------------------------

        //	returns distance between point and a line defined by the positions AB
        public static float GetPointToLineDist(Vector3 point, Vector3 lineA, Vector3 lineB)
        {
            Vector3 v = lineB - lineA;
            Vector3 w = point - lineA;

            float c1 = Vector3.Dot(w, v);
            float c2 = Vector3.Dot(v, v);
            float b = c1 / c2;

            Vector3 pb = lineA + b * v;
            return Vector3.Distance(point, pb);
        }

        /// <summary>
        /// returns distance between a point and a line segment AB
        /// </summary>
        public static float GetPointToLineSegmentDist3f(Vector3 point, Vector3 segmentA, Vector3 segmentB)
        {
            Vector3 v = segmentB - segmentA;
            Vector3 w = point - segmentA;

            float c1 = Vector3.Dot(w, v);
            if (c1 <= 0)
                return Vector3.Distance(point, segmentA);

            float c2 = Vector3.Dot(v, v);
            if (c2 <= c1)
                return Vector3.Distance(point, segmentB);

            float b = c1 / c2;
            Vector3 pb = segmentA + b * v;
            return Vector3.Distance(point, pb);
        }

        /// <summary>
        /// returns distance between a point and a line segment AB
        /// </summary>
        public static float GetPointToLineSegmentDist2f(Vector2 point, Vector2 segmentA, Vector2 segmentB)
        {
            Vector2 v = segmentB - segmentA;
            Vector2 w = point - segmentA;

            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
                return Vector2.Distance(point, segmentA);

            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
                return Vector2.Distance(point, segmentB);

            float b = c1 / c2;
            Vector2 pb = segmentA + b * v;
            return Vector2.Distance(point, pb);
        }

        // Find the distance from this point to a line segment (which is not the same as from this 
        //  point to anywhere on an infinite line). Also returns the closest point.
        public static float DistanceToLineSegment(Vector2 A, Vector2 B, Vector2 P, out Vector2 closestPoint)
        {
            return Mathf.Sqrt(DistanceToLineSegmentSquared(A, B, P, out closestPoint));
        }

        public static float DistanceToLineSegment(Vector2 A, Vector2 B, Vector2 P)
        {
            Vector2 p;
            return Mathf.Sqrt(DistanceToLineSegmentSquared(A, B, P, out p));
        }

		public static float DistanceToLineSegmentSquared(Vector2 A, Vector2 B, Vector2 P)
		{
			Vector2 p;
			return DistanceToLineSegmentSquared(A, B, P, out p);
		}
        // Same as above, but avoid using Sqrt(), saves a new nanoseconds in cases where you only want 
        //  to compare several distances to find the smallest or largest, but don't need the distance
        public static float DistanceToLineSegmentSquared(Vector2 A, Vector2 B, Vector2 P, out Vector2 closestPoint)
        {
            // Compute length of line segment (squared) and handle special case of coincident points
            float segmentLengthSquared = A.SqrDist(B);
            if (segmentLengthSquared < 1E-7f)  // Arbitrary "close enough for government work" value
            {
                closestPoint = A;
                return P.SqrDist(closestPoint);
            }

            // Use the magic formula to compute the "projection" of this point on the infinite line
            Vector2 lineSegment = B - A;
            float t = Vector2.Dot(P - A, lineSegment) / segmentLengthSquared;

            // Handle the two cases where the projection is not on the line segment, and the case where 
            //  the projection is on the segment
            if (t <= 0)
                closestPoint = A;
            else if (t >= 1)
                closestPoint = B;
            else
                closestPoint = A + (lineSegment * t);
            return P.SqrDist(closestPoint);
        }

        private static float _dotProduct2D(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

		/// <summary>
		/// Returns closest distance between two line segments
		/// If distance is 0, the segments intersect
		/// </summary>
		public static float DistanceBetweenLineSegments(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
		{
			if(LineSegmentsIntersect(a1, a2, b1, b2))
			{
				return 0f;
			}
			float[] d = new float[4];
			d[0] = DistanceToLineSegmentSquared(a1, b1, b2);
			d[1] = DistanceToLineSegmentSquared(a2, b1, b2);
			d[2] = DistanceToLineSegmentSquared(b1, a1, a2);
			d[3] = DistanceToLineSegmentSquared(b2, a1, a2);
			int index = 0;
			float dist = d[0];
			for(int i = 1; i < 4; i++)
			{
				if(d[i] < dist)
				{
					dist = d[i];
					index = i;
				}
			}
			return Mathf.Sqrt( d[index] );
		}

		/// <summary>
		/// Returns closest distance between two line segments as well as the closest point between them.
		/// If distance is 0, the segments intersect at closestP
		/// </summary>
		public static float DistanceBetweenLineSegments(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, 
		                                                out Vector2 closestP1, out Vector2 closestP2)
		{
			IntersectionInfo info;
			if(LineSegmentsIntersect(a1, a2, b1, b2, out closestP1, out info))
			{
				closestP2 = closestP1;
				return 0f;
			}
			Vector2[] c = new Vector2[4];
			float[] d	= new float[4];
			d[0] = DistanceToLineSegmentSquared(b1, b2, a1, out c[0]);
			d[1] = DistanceToLineSegmentSquared(b1, b2, a2, out c[1]);
			d[2] = DistanceToLineSegmentSquared(a1, a2, b1, out c[2]);
			d[3] = DistanceToLineSegmentSquared(a1, a2, b2, out c[3]);
			int index = 0;
			float dist = d[0];
			for(int i = 1; i < 4; i++)
			{
				if(d[i] < dist)
				{
					dist = d[i];
					index = i;
				}
			}
			closestP1 = c[index];
			switch(index)
			{
			case 0:	closestP2 = a1; break;
			case 1: closestP2 = a2; break;
			case 2: closestP2 = b1; break;
			case 3: closestP2 = b2; break;
			default: closestP2 = Vector2.zero; break;
			}
			return Mathf.Sqrt( d[index] );
		}

        public static float GetPointPlaneDistance(Vector3 point, Vector3 plane, Vector3 planeNRM, out Vector3 onPlane)
        {
            float sb, sn, sd;
            sn = -Vector3.Dot(planeNRM.normalized, (point - plane));
            sd = Vector3.Dot(planeNRM, planeNRM);
            sb = sn / sd;

            onPlane = point + sb * planeNRM;
            return Vector3.Distance(point, onPlane);
        }

		/// <summary>
		/// Returns true if point is on the line segment AB
		/// </summary>
		public static bool TestPointOnLineSegment3f(Vector3 A, Vector3 B, Vector3 point, float tolerance=0.0001f)
		{
			float AB = Vector3.Distance(A, B);
			float AP = Vector3.Distance(A, point);
			float PB = Vector3.Distance(B, point);
			return SloppyEquals_f(AB, AP + PB, tolerance);
		}

		/// <summary>
		/// Returns true if point is on the line segment AB
		/// </summary>
		public static bool TestPointOnLineSegment2f(Vector2 A, Vector2 B, Vector2 point, float tolerance=0.0001f)
		{
			float AB = Vector2.Distance(A, B);
			float AP = Vector2.Distance(A, point);
			float PB = Vector2.Distance(point, B);
			return SloppyEquals_f(AB, AP + PB, tolerance);
		}
    

        //-----------------------------------------------------------------------------------------------

        public static int LineCircleIntersection3f(Vector3 circle, float radius, Vector3 lineA, Vector3 lineB, out Vector3 intersectA, out Vector3 intersectB)
        {
            Vector3 v = (lineB - lineA).normalized;
            Vector2 intersectA_2f, intersectB_2f;
            int _case = LineCircleIntersection2f(new Vector2(circle.x, circle.z), radius,
                                                 new Vector2(lineA.x, lineA.z), new Vector2(lineB.x, lineB.z),
                                                 out intersectA_2f, out intersectB_2f);
            switch (_case)
            {
                case 1:

                    intersectA = Vector3.Project(new Vector3(intersectA_2f.x, 0, intersectA_2f.y), v);
                    intersectB = Vector3.zero;
                    break;

                case 2:

                    intersectA = Vector3.Project(new Vector3(intersectA_2f.x, 0, intersectA_2f.y), v);
                    intersectB = Vector3.Project(new Vector3(intersectB_2f.x, 0, intersectB_2f.y), v);
                    break;

                default:

                    intersectA = Vector3.zero;
                    intersectB = Vector3.zero;
                    break;
            }

            return _case;
		}

		/// <summary>
		/// returns wether a line is intersecting a circle, on which points and how
		///	0 - no intersection, 1 - one intersection (intersectA), 2 two intersections (intersectB)
		/// </summary>
        public static int LineCircleIntersection2f(Vector2 circle, float radius, Vector2 lineA, Vector2 lineB, out Vector2 intersectA, out Vector2 intersectB)
        {
            Vector2 v = (lineB - lineA).normalized;
            Vector2 proj = lineA + Vector2.Dot(circle - lineA, v) * v;
            float distSqr = Vector2.Distance(circle, proj);

            if (SloppyEquals_f(distSqr, radius))
            {
                //	line touches circle at one point
                float offset = Mathf.Sqrt(radius * radius - distSqr);
                intersectA = proj + offset * v;
                intersectB = proj - offset * v;
                if (GetPointToLineDist(intersectA, lineA, lineB) > GetPointToLineDist(intersectB, lineA, lineB))
                {
                    intersectA = intersectB;
                }
                return 1;
            }
            else if (distSqr < radius)
            {
                //	line crosses circle, two intersections
                float offset = Mathf.Sqrt(radius * radius - distSqr);
                intersectA = proj + offset * v;
                intersectB = proj - offset * v;
                return 2;
            }
            else
            {
                //	no intersection
                intersectA = Vector2.zero;
                intersectB = Vector2.zero;
                return 0;
            }
        }

        public static bool LineCircleIntersect_Simple3f(Vector3 p, float radius, Vector2 linePoint1, Vector2 linePoint2)
        {
            return LineCircleIntersect_Simple2f(new Vector2(p.x, p.z), radius, linePoint1, linePoint2);
        }

        public static bool LineCircleIntersect_Simple2f(Vector2 p, float radius, Vector2 linePoint1, Vector2 linePoint2)
        {
            Vector2 p1 = new Vector2(linePoint1.x, linePoint1.y);
            Vector2 p2 = new Vector2(linePoint2.x, linePoint2.y);
            p1.x -= p.x;
            p1.y -= p.y;
            p2.x -= p.x;
            p2.y -= p.y;
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            float dr = (float)Mathf.Sqrt((dx * dx) + (dy * dy));
            float D = (p1.x * p2.y) - (p2.x * p1.y);

            float di = (radius * radius) * (dr * dr) - (D * D);

            if (di < 0) return false;
            else return true;
        }

        //-----------------------------------------------------------------------------------------------

		/// <summary>
		/// returns wether or not given point is inside segment AB
		/// </summary>
        public static bool PointInSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            if (a.x != b.x)     //	segment not vertical
            {
                if (a.x <= point.x && point.x <= b.x)
                    return true;
                if (a.x >= point.x && point.x >= b.x)
                    return true;
            }
            else                //	segment vertical, test y
            {
                if (a.y <= point.y && point.y <= b.y)
                    return true;
                if (a.y >= point.y && point.y >= b.y)
                    return true;
            }
            return false;
        }

        //-----------------------------------------------------------------------------------------------

        //	

		/// <summary>
		/// returns wether a given segment and plane intersect, how, and at which point
		///	return values are: 
		///	0 - no intersection
		///	1 - intersection in <intersectPoint>
		///	2 - segment lies in plane
		/// </summary>
        public static int PlaneSegmentIntersection(Vector3 segmentA, Vector3 segmentB,
                                                   Vector3 planeP, Vector3 planeNRM,
                                                   out Vector3 intersectPoint)
        {
            intersectPoint = Vector3.zero;

            Vector3 u = segmentB - segmentA;
            Vector3 w = segmentA - planeP;

            float d = Vector3.Dot(planeNRM, u);
            float n = -Vector3.Dot(planeNRM, w);

            if (Mathf.Abs(d) < 0.001f)          //	segment is parallel to plane
            {
                if (n == 0)                     //	segment lies in plane
                    return 2;
                else
                    return 0;                   //	no intersection
            }
            float sI = n / d;
            if (sI < 0 || sI > 1)
                return 0;                       //	no intersection

            intersectPoint = segmentA + u * sI;
            return 1;
        }

        //-----------------------------------------------------------------------------------------------

		/// <summary>
		/// Get average of given numbers
		/// </summary>
        public static float CalcAverage(params float[] vals)
        {
            float sum = 0;
            for (int i = 0; i < vals.Length; i++)
                sum += vals[i];
            return sum / (float)vals.Length;
        }

		/// <summary>
		/// Get center point (average) of given positions
		/// </summary>
        public static Vector2 CalcCentroid(IEnumerable<Vector2> positions)
        {
            Vector2 sum = new Vector2();
            foreach (Vector2 p in positions)
            {
                sum += p;
            }
            return sum / positions.Count();
        }
		/// <summary>
		/// Get center point (average) of given positions
		/// </summary>
        public static Vector3 CalcCentroid(IEnumerable<Vector3> positions)
        {
            Vector3 sum = new Vector3();
            foreach (Vector3 p in positions)
            {
                sum += p;
            }
            return sum / positions.Count();
        }

        //-----------------------------------------------------------------------------------------------

        //	truncates given floating point number after n digits
        public static float TruncateFloat(float f, int digits)
        {
            float d = 10; for (int i = 1; i < digits; i++) d *= 10;
            return Mathf.RoundToInt(f * d) / d;
        }

        //-----------------------------------------------------------------------------------------------

 
        public static float AreaOfTriangle(Vector2 a, Vector2 b, Vector2 c)
        {
            float abLength = (a - b).magnitude;
            float acLength = (a - c).magnitude;
            float bcLength = (b - c).magnitude;
            // Heron's formula
            float s = (abLength + acLength + bcLength) / 2;
            return Mathf.Sqrt(s * (s - abLength) * (s - acLength) * (s - bcLength));
        }
        public static float AreaOfTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            float abLength = (a - b).magnitude;
            float acLength = (a - c).magnitude;
            float bcLength = (b - c).magnitude;
            // Heron's formula
            float s = (abLength + acLength + bcLength) / 2;
            return Mathf.Sqrt(s * (s - abLength) * (s - acLength) * (s - bcLength));
        }

   

        // based on http://mathworld.wolfram.com/TrianglePointPicking.html
        public static Vector2 RandomPointInsideTriangle(Vector2 a, Vector2 b, Vector2 c)
        {

            Vector2 aOrigin = Vector2.zero;
            Vector2 bOrigin = b - a;
            Vector2 cOrigin = c - a;

            Vector2 randomInsideQuad = Random.value * bOrigin + Random.value * cOrigin;
            bool isInsideTri = IsPointInsideTriangle(randomInsideQuad, aOrigin, bOrigin, cOrigin);
            if (!isInsideTri)
            {
                Vector2 quadCenter = (bOrigin + cOrigin) * 0.5f;
                randomInsideQuad -= quadCenter;
                randomInsideQuad.x *= -1;
                randomInsideQuad.y *= -1;
                randomInsideQuad += quadCenter;
            }
            randomInsideQuad += a;

            return randomInsideQuad;
        }

        public static Vector3 RandomPointInsideTriangle(Vector3 a, Vector3 b, Vector3 c)
        {

            //Vector3 aOrigin = Vector3.zero;
            Vector3 bOrigin = b - a;
            Vector3 cOrigin = c - a;
            Vector3 crossPoint = (bOrigin - cOrigin) * 0.5f;
            Vector3 crossPointOffset = cOrigin + crossPoint;
            Vector3 normal = Vector3.Cross(bOrigin, cOrigin);
            Vector3 planeNormal = Vector3.Cross(normal, crossPoint);
            Plane crossPlane = new Plane(planeNormal.normalized, bOrigin);


            //var normal = (bOrigin + cOrigin).normalized;
            //Plane plane = new Plane(normal, bOrigin);

            Vector3 randomInsideQuad = Random.value * bOrigin + Random.value * cOrigin;


            if (crossPlane.GetDistanceToPoint(randomInsideQuad) > 0)
            {
                Vector3 localToCross = randomInsideQuad - crossPointOffset;
                randomInsideQuad -= localToCross * 2;
            }


            //var distance = plane.GetDistanceToPoint(randomInsideQuad);

            //if (distance > 0)
            //{
            //    Vector3 backVector = normal * -distance;
            //    randomInsideQuad += backVector;
            //}

            //bool isInsideTri = IsPointInsideTriangle(randomInsideQuad, aOrigin, bOrigin, cOrigin);
            //if (!isInsideTri)
            //{
            //    Vector2 quadCenter = (bOrigin + cOrigin) * 0.5f;
            //    randomInsideQuad -= quadCenter;
            //    randomInsideQuad.x *= -1;
            //    randomInsideQuad.y *= -1;
            //    randomInsideQuad += quadCenter;
            //}
            randomInsideQuad += a;

            return randomInsideQuad;
        }

        //http://stackoverflow.com/questions/2049582/how-to-determine-a-point-in-a-triangle
        public static bool IsPointInsideTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            var s = a.y * c.x - a.x * c.y + (c.y - a.y) * point.x + (a.x - c.x) * point.y;
            var t = a.x * b.y - a.y * b.x + (a.y - b.y) * point.x + (b.x - a.x) * point.y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -b.y * c.x + a.y * (c.x - b.x) + a.x * (b.y - c.y) + b.x * c.y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0 && t > 0 && (s + t) < A;
        }

        /// <summary>
        /// Test if two line segments intersect.
        /// Based on the top answer on http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
        /// </summary>
        /// <param name="p1">Start point of the first line</param>
        /// <param name="p2">End point of the first line</param>
        /// <param name="q1">Start point of the second line</param>
        /// <param name="q2">End point of the second line</param>
        /// <param name="pointOfIntersection">The point of intersection will be written to this Vector2. It will</param>
        /// <param name="intersectionInfo">Additional info about the test.</param>
        /// <returns>true if the line segments intersect.</returns>
        public enum IntersectionInfo
        {
            Collinear,
            Parallel,
            Intersecting,
            NonIntersecting
        }
        static public bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2 pointOfIntersection, out IntersectionInfo intersectionInfo)
        {
            Vector2 r = p2 - p1;
            Vector2 s = q2 - q1;
            float rXs = Vector3.Cross(r, s).z;
            float qpXr = Vector3.Cross((q1 - p1), r).z;

            //t = (q − p) × s / (r × s)
            float t = Vector3.Cross((q1 - p1), s).z / rXs;

            //u = (q − p) × r / (r × s)
            float u = qpXr / rXs;

            //If r × s = 0 and (q − p) × r = 0, then the two lines are collinear.
            if (rXs == 0 && qpXr == 0)
            {
                pointOfIntersection = new Vector2(float.NaN, float.NaN);
                intersectionInfo = IntersectionInfo.Collinear;
                return false;
            }
            else if (rXs == 0 && qpXr != 0) // If r × s = 0 and(q − p) × r ≠ 0, then the two lines are parallel and non-intersecting.
            {
                pointOfIntersection = new Vector2(float.NaN, float.NaN);
                intersectionInfo = IntersectionInfo.Parallel;
                return false;
            }
            else if (rXs != 0 && t <= 1 && t >= 0 && u <= 1 && u >= 0) // If r × s ≠ 0 and 0 ≤ t ≤ 1 and 0 ≤ u ≤ 1, the two line segments meet at the point p + t r = q + u s
            {
                pointOfIntersection = p1 + t * r;
                intersectionInfo = IntersectionInfo.Intersecting;
                return true;
            }
            else
            {
                // Otherwise, the two line segments are not parallel but do not intersect.
                pointOfIntersection = new Vector2(float.NaN, float.NaN);
                intersectionInfo = IntersectionInfo.NonIntersecting;
                return false;
            }
        }
        static public bool LineSegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2)
        {
            Vector2 dummy;
            IntersectionInfo info;
            return LineSegmentsIntersect(p1, p2, q1, q2, out dummy, out info);
        }
        /// <summary>
        /// Test if a point is left of, right of or on an infinite 2D line.
        /// </summary>
        /// <returns>
        /// greater than 0 for point left of the line through lineStart to lineEnd
        /// equal 0 for point on the line
        /// less than 0 for point right of the line
        /// </returns>
        public static float IsLeft(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            return ((lineEnd.x - lineStart.x) * (point.y - lineStart.y) - (point.x - lineStart.x) * (lineEnd.y - lineStart.y));
        }

        /// <summary>
        /// Project a vector onto another one. (Orthogonal projection)
        /// </summary>
        /// <param name="vec">The vector that will be projected.</param>
        /// <param name="ontoVec">The vector to be projected on.</param>
        /// <returns>The vector component of vec in the direction of ontoVec. Returns the zero vector when ontoVec is zero.</returns>
        public static Vector2 Project(Vector2 vec, Vector2 ontoVec)
        {
            float ontoDot = Vector2.Dot(ontoVec, ontoVec);
            if (ontoDot != 0)
                return (Vector2.Dot(vec, ontoVec) / ontoDot) * ontoVec;
            else
                return Vector2.zero;
        }

        //=================================================================================================================


        /// <summary>
        /// Fits a plane into a given point set with at least 3 points and returns a best-fit plane centroid+normal representation.
        /// </summary>
        /// <returns><c>true</c>, if plane by averaging was fitted, <c>false</c> otherwise.</returns>
        /// <param name="points">Points to average the plane out.</param>
        /// <param name="planeP">Plane centroid.</param>
        /// <param name="planeNRM">Plane normal.</param>
        public static bool BestFitPlaneByAveraging(IEnumerable<Vector3> points, out Vector3 planeP, out Vector3 planeNRM)
        {

            // Assume at least three points

            IEnumerator<Vector3> a = points.GetEnumerator();
            IEnumerator<Vector3> b = points.GetEnumerator();

            Vector3 centroid = Vector3.zero;
            Vector3 normal = Vector3.zero;

            b.MoveNext();
            int count = 0;
            while (a.MoveNext())
            {
                if (!b.MoveNext())
                {
                    b = points.GetEnumerator();
                    // b.Reset(); Reset is not supported when using yield
                    b.MoveNext();
                }
                Vector3 i = a.Current;
                Vector3 j = b.Current;

                normal.x += (i.y - j.y) * (i.z + j.z); // Project on yz
                normal.y += (i.z - j.z) * (i.x + j.x); // Project on xz
                normal.z += (i.x - j.x) * (i.y + j.y); // Project on xy
                centroid += j;
                count++;
            }
            if (count < 3)
            {
                planeP = centroid / (float)count;
                planeNRM = Vector3.up;
                return false;
            }
            else
            {
                planeP = centroid / (float)count;
                planeNRM = normal.normalized;
                return true;
            }
        }

        /// <summary>
        /// Calculate the area of the non-self-intersecting polygon.
        /// </summary>
        /// <param name="polygon">The vertices of the polygon.</param>
        /// <returns></returns>
        public static float SignedAreaOfPolygon(IEnumerable<Vector2> verts)
        {
            float signedArea = 0;
            Vector2[] vertices = verts.ToArray();
            for (int i = 0; i < vertices.Length; i++)
            {
                signedArea += 0.5f * (vertices[i].x * vertices[(i + 1) % vertices.Length].y - vertices[(i + 1) % vertices.Length].x * vertices[i].y);
            }
            return signedArea;
        }
        /// <summary>
        /// Calculate the area of the non-self-intersecting polygon that is in the XZ plane.
        /// </summary>
        /// <param name="polygon">The vertices of the polygon.</param>
        /// <returns></returns>
        public static float SignedAreaOfPolygon(IEnumerable<Vector3> verts)
        {
            return SignedAreaOfPolygon(verts.Select((vert) => new Vector2(vert.x, vert.z)));
        }
    }
}
//=================================================================================================================
