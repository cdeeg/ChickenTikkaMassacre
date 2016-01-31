using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;



//== TYPES ========================================================================================================

#region types

public enum Direction		{ north=0, northEast, east, southEast, south, southWest, west, northWest }

#endregion


//=================================================================================================================

namespace jUtility
{

	#region misc

	public static class Misc
	{

		//-- Directions ---------------------------------------------------------------------------------
		
		const int angleDirStep = 45;
		
		public static Direction Get8Direction2f( Vector2 v )
		{
			v = v.normalized;
			
			Direction d = Direction.north;
			float angle = -jUtility.Math.GetVector2AngleBetween(v, Vector2.up);
			if(angle < 0)
				angle += 360;
			
			if(angle < 22.5f || angle > 337.5f)
				d = Direction.north;
			else if(angle < 67.5f)
				d = Direction.northEast;
			else if(angle < 112.5f)
				d = Direction.east;
			else if(angle < 157.5f)
				d = Direction.southEast;
			else if(angle < 202.5f)
				d = Direction.south;
			else if(angle < 247.5f)
				d = Direction.southWest;
			else if(angle < 292.5f)
				d = Direction.west;
			else 
				d = Direction.northWest;
			
			//			Debug.Log("angle: " + angle + " dir: " + d.ToString());
			
			return d;
		}
		
		public static Direction Get8Direction3f( Vector3 v )
		{
			v = v.normalized;
			
			float verticalDot = Vector3.Dot(v, Vector3.forward);
			float horizontalDot = Vector3.Dot(v, Vector3.right);
			
			//	up-right
			if(verticalDot > 0 && verticalDot < 0.93f && horizontalDot > 0 && horizontalDot < 0.8f)
				return Direction.northEast;
			//	up-left
			else if(verticalDot > 0 && verticalDot < 0.93f && horizontalDot <= 0 && horizontalDot > -0.8f)
				return Direction.northWest;
			//	down-right
			else if(verticalDot < 0 && verticalDot > -0.93f && horizontalDot > 0.5f && horizontalDot < 0.96f)
				return Direction.southEast;
			//	down-left
			else if(verticalDot < 0 && verticalDot > -0.93f && horizontalDot > -0.96f && horizontalDot < -0.28f)
				return Direction.southWest;
			//	up
			else if(verticalDot > 0.5f && horizontalDot < 0.72f && horizontalDot > -0.72f)
				return Direction.north;
			//	down
			else if(verticalDot < -0.5f && horizontalDot < 0.72f && horizontalDot > -0.72f)
				return Direction.south;
			//	right
			else if(horizontalDot > 0.5f)
				return Direction.east;
			//	left
			else if(horizontalDot < -0.5f)
				return Direction.west;
			else
				return Direction.north;
		}
		
		//-- Array Search -------------------------------------------------------------------------------
		
		//	returns the index of the smallest distance in an array of indices
		public static int GetShortestDistanceID(float[] distances)
		{
			int id = 0;
			float mag = float.MaxValue;
			for(int i = 0; i < distances.Length; i++)
			{
				if(distances[i] < mag)
				{
					id = i;
					mag = distances[i];
				}
			}
			return id;
		}
		
		public static int GetShortestDistanceID(float[] distances, int defaultID)
		{
			int id = defaultID;
			float mag = float.MaxValue;
			for(int i = 0; i < distances.Length; i++)
			{
				if(distances[i] < mag)
				{
					id = i;
					mag = distances[i];
				}
			}
			return id;
		}
		
		public static int GetShortestDistanceID(float[] distances, float minDist, int defaultID = 0)
		{
			int id = defaultID;
			float mag = float.MaxValue;
			for(int i = 0; i < distances.Length; i++)
			{
				if(distances[i] <= minDist && distances[i] < mag)
				{
					id = i;
					mag = distances[i];
				}
			}
			return id;
		}
		
		public static int GetShortestDistanceID(float[] distances, out float shortestDist, float minimumDist = float.MaxValue, int defaultID = 0)
		{
			int id = defaultID;
			float mag = float.MaxValue;
			for(int i = 0; i < distances.Length; i++)
			{
				if(distances[i] <= minimumDist && distances[i] < mag)
				{
					id = i;
					mag = distances[i];
				}
			}
			shortestDist = mag;
			return id;
		}
		
		//-- Time ---------------------------------------------------------------------------------------
		
		//	DateTime formatting
		
		public static string FormatDateTime_German( DateTime time )
		{
			return time.Day.ToString() + "/" + time.Month.ToString() + "/" + time.Year.ToString();
		}
		
		public static string FormatDateTimeExact_German( DateTime time )
		{
			return FormatDateTime_German(time) + " " + time.Hour.ToString() + ":" + time.Minute.ToString();
		}

		//-- String formatting --------------------------------------------------------------------------

		//	returns a string of a floating point number with n floating digits
		public static string FormatFloatToString(float f, int digits)
		{
			if(digits == 0)
			{
				return Mathf.FloorToInt(f).ToString();
			}
			int d = 10; for(int i = 1; i < digits; i++) d *= 10;
			return (Mathf.RoundToInt(f*d) / (float)d).ToString();
		}
		
		//-- Other --------------------------------------------------------------------------------------

		//	returns current framerate for display
		public static string GetRoundedFrameRate(int floatingDigits)
		{
			return Mathf.RoundToInt(1.0f/Time.smoothDeltaTime).ToString();
		}

		//	breaks up a given number into its individual digits for display
		public static List<int> GetNumberDigits(int num)
		{
			List<int> listOfInts = new List<int>();
			while(num > 0)
			{
				listOfInts.Add(num % 10);
				num = num / 10;
			}
			listOfInts.Reverse();
			return listOfInts;
		}
		
	}
	
	#endregion
	
}

#region richtext utility

public static class RichTextUtil
{
	static Color warningColor;
	static Color errorColor;
	static Color processColor;
	static Color successColor;

	static RichTextUtil()
	{
		warningColor 	= _hexToColor("bbb000");
		errorColor		= _hexToColor("8e0000");
		processColor	= _hexToColor("4145A6FF");
		successColor	= _hexToColor("04a200");
	}

	//	convert Color/Color32 to hex representation
	public static string _colorToHex(Color32 color)
	{
		string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
		return hex;
	}
	
	//	convert hex to Color
	public static Color _hexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}
	
	public static string color( string text, Color32 c )
	{
		return "<color=#" + _colorToHex(c) + ">" + text + "</color>";
	}
	
	public static string italic( string text )
	{
		return "<i>" + text + "</i>";
	}
	
	public static string emphasized( string text )
	{
		return "<b>" + text + "</b>";
	}
	
	public static string size( string text, int size )
	{
		return "<size=" + size.ToString() + ">" + text + "</size>";
	}
	
	public static string FormatWarningMessage(string sourceScript, string msg)
	{
		return RichTextUtil.color("Warning: ", warningColor) + RichTextUtil.emphasized(sourceScript + ":: ") + msg;
	}
	public static string FormatErrorMessage(string sourceScript, string msg)
	{
		return RichTextUtil.emphasized(RichTextUtil.color("Error: ", errorColor) + sourceScript + ":: ") + msg;
	}
	public static string FormatProcessMessage(string sourceScript, string msg)
	{
		return RichTextUtil.color("Process: ", processColor) + RichTextUtil.emphasized(sourceScript + ":: ") + msg;
	}
	public static string FormatSuccessMessage(string sourceScript, string msg)
	{
		return RichTextUtil.color("Success: ", successColor) + RichTextUtil.emphasized(sourceScript + ":: ") + msg;
	}
}

#endregion

//=================================================================================================================

#region flag utility

public static class EnumHelper
{
	public static bool EnumFlagTest(this Enum keys, Enum flag)
	{
		ulong keysVal = Convert.ToUInt64(keys);
		ulong flagVal = Convert.ToUInt64(flag);
		
		return (keysVal & flagVal) == flagVal;
	}
	
	//	//	returns true if given enums share a flag
	//	public static bool Compare<T>(T a, T b)
	//		where T : struct
	//	{
	//		foreach(T e1 in IterateFlags(a))
	//			if(IsFlagPresent<T>(e1, b))
	//				return true;
	//		return false;
	//	}
	//	
	//	//	returns true if given enum input has given flag
	//	public static bool IsFlagPresent<T>(T input, T lookingForFlag)
	//		where T : struct
	//	{
	//		int intVal = (int) (object) input;
	//		int intLookingFor = (int) (object) lookingForFlag;
	//		return intVal == intLookingFor && intLookingFor != 0;
	//	}
	//	
	//	//	returns each flag entry of an enum separately
	//	public static IEnumerable IterateFlags<T>(T input)
	//		where T : struct
	//	{
	//	    foreach (T e in Enum.GetValues(input.GetType()))
	//		{
	//			if(IsFlagPresent<T>(input, e))
	//				yield return e;
	//		}
	//	}
	//	
	//	public static List<T> GetFlags<T>(T input)
	//		where T : struct
	//	{
	//		List<T> result = new List<T>();
	//		foreach(T t in IterateFlags<T>(input))
	//			result.Add(t);
	//		return result;
	//	}
}

#endregion


//=================================================================================================================

public static class StringUtil
{
    public static string StringWithUniqueInt(IEnumerable<string> currentStrings, string startString)
    {
        var query = currentStrings.Where((str)=>str.Length > startString.Length)
              .Select((str) => new { start = str.Substring(0, startString.Length), end = str.Substring(startString.Length) })
              .Where((str) => {
                  int i;
                  return str.start.Equals(startString, StringComparison.CurrentCultureIgnoreCase) && int.TryParse(str.end, out i);
              })
              .Select((name) => int.Parse(name.end));
        if(query.Count() > 0)
        {
            return startString + (query.Max() + 1);
        }
        else
        {
            return startString + "1";
        }
    }
}