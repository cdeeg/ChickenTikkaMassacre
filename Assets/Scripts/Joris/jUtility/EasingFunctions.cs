/*
 * 		2013 Joris Drobka
 * 
 * 		basic easing functions for unity
 * 
 */

using UnityEngine;

//== TYPES ========================================================================================================

#region types

public enum EaseType 	
{ 
	Linear=0, 

	//	quadratic
	QuadraticIN, 
	QuadraticOUT, 
	QuadraticINOUT, 

	//	cubic
	CubicIN, 
	CubicOUT, 
	CubicINOUT,

	//	elastic
	ElasticIN, 
	ElasticOUT, 
	ElasticINOUT,

	//	bounce
	BounceIN, 
	BounceOUT, 
	BounceINOUT,

	//	back
	BackIN, 
	BackOUT, 
	BackINOUT 
}

public static class EaseTypeExtension
{
	public static bool hasAdditionalParameters(this EaseType type)
	{
		switch(type)
		{
		case EaseType.BackIN:
		case EaseType.BackOUT:
		case EaseType.BackINOUT:
		case EaseType.BounceIN:
		case EaseType.BounceOUT:
		case EaseType.BounceINOUT:
		case EaseType.ElasticIN:
		case EaseType.ElasticOUT:
		case EaseType.ElasticINOUT:

			return true;

		default:

			return false;
		}
	}
}

#endregion

//=================================================================================================================

#region easing functions

//	easing functions
public static class Easing
{
	public static Vector2 EaseVector2(EaseType type, float time, Vector2 startValue, Vector2 targetValue, float duration, float easeParamA = 1, float easeParamB = 1) 
	{
		Vector2 v = new Vector2();
		v.x = EaseMovement(type, time, startValue.x, targetValue.x-startValue.x, duration, easeParamA, easeParamB);
		v.y = EaseMovement(type, time, startValue.y, targetValue.y-startValue.y, duration, easeParamA, easeParamB);
		return v;
	}
	
//--------------------------------------------------------
	
	public static Vector3 EaseVector3(EaseType type, float time, Vector3 startValue, Vector3 targetValue, float duration, float easeParamA = 1, float easeParamB = 1) 
	{
		Vector3 v = new Vector3();
		v.x = EaseMovement(type, time, startValue.x, targetValue.x-startValue.x, duration, easeParamA, easeParamB);
		v.y = EaseMovement(type, time, startValue.y, targetValue.y-startValue.y, duration, easeParamA, easeParamB);
		v.z = EaseMovement(type, time, startValue.z, targetValue.z-startValue.z, duration, easeParamA, easeParamB);
		return v;
	}

//--------------------------------------------------------

	public static Quaternion EaseQuaternion(EaseType type, float time, Quaternion startValue, Quaternion targetValue, float duration, float easeParamA = 1, float easeParamB = 1)
	{
		Quaternion q = new Quaternion();
		q.x = EaseMovement(type, time, startValue.x, targetValue.x - startValue.x, duration, easeParamA, easeParamB);
		q.y = EaseMovement(type, time, startValue.y, targetValue.y - startValue.y, duration, easeParamA, easeParamB);
		q.z = EaseMovement(type, time, startValue.z, targetValue.z - startValue.z, duration, easeParamA, easeParamB);
		q.w = EaseMovement(type, time, startValue.w, targetValue.w - startValue.w, duration, easeParamA, easeParamB);
		return q;
	}	
	
//--------------------------------------------------------
	
	public static float EaseMovement(EaseType type, float time, float startValue, float deltaValue, float duration, float easeParamA = 1, float easeParamB = 1)
	{
		switch((int) type)
		{
		case 1: return EaseInQuadratic(time,startValue,deltaValue,duration);
		case 2: return EaseOutQuadratic(time,startValue,deltaValue,duration);
		case 3: return EaseInOutQuadratic(time,startValue,deltaValue,duration);
		case 4: return EaseInCubic(time,startValue,deltaValue,duration);
		case 5: return EaseOutCubic(time,startValue,deltaValue,duration);
		case 6: return EaseInOutCubic(time,startValue,deltaValue,duration);
		case 7: return EaseInElastic(time,startValue,deltaValue,duration, easeParamA, easeParamB);
		case 8: return EaseOutElastic(time,startValue,deltaValue,duration, easeParamA, easeParamB);
		case 9: return EaseInOutElastic(time,startValue,deltaValue,duration, easeParamA, easeParamB);
		case 10: return EaseInBounce(time,startValue,deltaValue,duration);
		case 11: return EaseOutBounce(time,startValue,deltaValue,duration);
		case 12: return EaseInOutBounce(time,startValue,deltaValue,duration);
		case 13: return EaseInBack(time,startValue,deltaValue,duration, easeParamA);
		case 14: return EaseOutBack(time,startValue,deltaValue,duration, easeParamA);
		case 15: return EaseInOutBack(time,startValue,deltaValue,duration, easeParamA);
		default: return LinearTween(time,startValue,deltaValue,duration);
		}
	}
	
//--------------------------------------------------------
	
	private static float LinearTween (float t, float b, float c, float d) {
		return c*t/d + b;
	}
	
//--------------------------------------------------------
	
	private static float EaseInBack (float t, float b, float c, float d, float s) {
		if (s == 0f) s = 1.70158f;
		return c*(t/=d)*t*((s+1)*t - s) + b;
	}
	
	private static float EaseOutBack (float t, float b, float c, float d, float s) {
		if (s == 0) s = 1.70158f;
		return c*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
	}
	
	private static float EaseInOutBack (float t, float b, float c, float d, float s) {
		if (s == 0) s = 1.70158f;
		if ((t/=d/2) < 1) return c/2*(t*t*(((s*=(1.525f))+1)*t - s)) + b;
		return c/2*((t-=2)*t*(((s*=(1.525f))+1)*t + s) + 2) + b;
	}
	
//--------------------------------------------------------
	
	private static float EaseInQuadratic (float t, float b, float c, float d) {
		return c*(t/=d)*t + b;
	}

	
	private static float EaseOutQuadratic (float t, float  b, float c, float d) {
		return -c *(t/=d)*(t-2) + b;
	}

	
	private static float EaseInOutQuadratic (float t, float b, float c, float d) {
		if ((t/=d/2) < 1) return c/2*t*t + b;
		return -c/2 * ((--t)*(t-2) - 1) + b;
	}
	
//--------------------------------------------------------
	
	private static float EaseInCubic (float t, float b, float c, float d) {
		return c*(t/=d)*t*t + b;
	}
	
	private static float EaseOutCubic (float t, float b, float c, float d) {
		return c*((t=t/d-1)*t*t + 1) + b;
	}

	private static float EaseInOutCubic(float t, float b, float c, float d) {
		if ((t/=d/2) < 1) return c/2*t*t*t + b;
		return c/2*((t-=2)*t*t + 2) + b;
	}
	
//--------------------------------------------------------
	
	private static float EaseInBounce (float t, float b, float c, float d) {
		return c - EaseOutBounce (d-t, 0, c, d) + b;
	}
	
	private static float EaseOutBounce (float t, float b, float c, float d) 
	{
		if ((t/=d) < (1/2.75f)) {
			return c*(7.5625f*t*t) + b;
		} else if (t < (2/2.75f)) {
			return c*(7.5625f*(t-=(1.5f/2.75f))*t + 0.75f) + b;
		} else if (t < (2.5/2.75f)) {
			return c*(7.5625f*(t-=(2.25f/2.75f))*t + 0.9375f) + b;
		} else {
			return c*(7.5625f*(t-=(2.625f/2.75f))*t + 0.984375f) + b;
		}
	}
	
	private static float EaseInOutBounce (float t, float b, float c, float d) {
		if (t < d/2) return EaseInBounce (t*2, 0, c, d) * 0.5f + b;
		return EaseOutBounce (t*2-d, 0, c, d) * 0.5f + c*0.5f + b;
	}
	
//--------------------------------------------------------
	
	private static float EaseInElastic (float t, float b, float c, float d, float a, float p) {
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (p == 0) p=d*0.3f;
		float s = 0;
		if (a < Mathf.Abs(c)) { a=c; s=p/4; }
		else s = p/(2*Mathf.PI) * Mathf.Asin (c/a);
		return -(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )) + b;
	}
	
	private static float EaseOutElastic (float t, float b, float c, float d, float a, float p) {
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (p == 0f) p=d*0.3f;
		float s = 0;
		if (a < Mathf.Abs(c)) { a=c; s=p/4; }
		else s = p/(2*Mathf.PI) * Mathf.Asin (c/a);
		return a*Mathf.Pow(2,-10*t) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p ) + c + b;
	}
	
	private static float EaseInOutElastic (float t, float b, float c, float d, float a, float p) {
		if (t==0) return  b;  if ((t/=d/2)==2) return b+c;  if (p == 0) p=d*(0.3f*1.5f);
		float s = 0;
		if (a < Mathf.Abs(c)) { a=c; s=p/4; }
		else s = p/(2*Mathf.PI) * Mathf.Asin (c/a);
		if (t < 1) return -0.5f*(a*Mathf.Pow(2,10f*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )) + b;
		return a*Mathf.Pow(2,-10*(t-=1)) * Mathf.Sin( (t*d-s)*(2*Mathf.PI)/p )*0.5f + c + b;
	}	
}

#endregion

//=================================================================================================================