using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

//=================================================================================================================

public static class UnityExtensionMethods
{
	
	#region vector extensions
	
	public static float SqrDist(this Vector2 a, Vector2 b) 			{ return (a-b).sqrMagnitude; }
	public static float SqrDist(this Vector3 a, Vector3 b) 			{ return (a-b).sqrMagnitude; }
	public static float Distance2f(this Vector3 p, Vector3 p2) 		{ return Vector2.Distance( new Vector2(p.x, p.z), new Vector2(p2.x, p2.z) ); }
	public static float SqrDistance2f(this Vector3 p, Vector3 p2)	{ return (new Vector2(p.x,p.z)-new Vector2(p2.x,p2.z)).sqrMagnitude; }

	/// <summary>
	/// Calculate a perpendicular vector that is left of this vector (in counter-clockwise direction)
	/// </summary>
	public static Vector2 GetPerpendicular(this Vector2 vec)
	{
		return new Vector2(-vec.y, vec.x);
	}
	public static Vector2 toXZ(this Vector3 vec)
	{
		return new Vector2(vec.x, vec.z);
	}
	public static Vector3 toXZDir(this Vector3 vec)
	{
		return Vector3.Normalize(new Vector3(vec.x, 0, vec.z));
	}

	public static Vector3 toXYZ(this Vector2 vec, float y)
	{
		return new Vector3(vec.x, y, vec.y);
	}

	#endregion
	
	//=================================================================================================================

	#region transform extensions

	/// <summary>
	/// Performs a full recursive search for given name and returns the first match. 
	/// Uppercase is ignored.
	/// </summary>
	public static Transform FindChildInHierarchy( this Transform parent, string search )
	{
		search = search.ToLower();
		if( parent.name.ToLower().Equals(search) ) return parent;

		foreach( Transform child in parent )
		{
			Transform result = FindChildInHierarchy(child, search);
			if(result != null) return result;
		}

		return null;
	}
	
	/// <summary>
	/// Performs a full recursive search for specified component and returns the first match.
	/// </summary>
	/// <returns>First TComponent found or null.</returns>
	/// <param name="parent">Transform to start from (also gets checked for TComponent).</param>
	public static TComponent GetComponentInHierarchy<TComponent>( this Transform parent, int depth=int.MaxValue ) where TComponent : Component
	{
		if(parent == null) return null;
		TComponent c = parent.GetComponent<TComponent>();
		if(c != null) return c;

		foreach(Transform child in parent)
		{
			c = GetComponentInHierarchy<TComponent>(child);
			if(c != null) return c;
		}
		return null;
	}

	/// <summary>
	/// Performs a full recursive search for specified component and returns all occurances.
	/// </summary>
	/// <returns>A list of all found components.</returns>
	/// <param name="parent">Transform to start from (also gets checked for TComponent).</param>
	/// <typeparam name="TComponent">Component type to search for.</typeparam>
	public static List<TComponent> GetComponentsInHierarchy<TComponent>( this Transform parent, int depth=int.MaxValue ) where TComponent : Component
	{
		if(parent == null) return null;
		List<TComponent> result = new List<TComponent>(4);
		_recursiveComponentSearch<TComponent>(parent, result, depth);
		return result;
	}

	/// <summary>
	/// Performs a full recursive search for specified type and returns all occurances.
	/// You may search interfaces with with method, but the typecasting is expensive
	/// </summary>
	/// <returns>A list of all found components.</returns>
	/// <param name="parent">Transform to start from (also gets checked for TComponent).</param>
	/// <typeparam name="T">Type to search for.</typeparam>
	public static List<T> GetTypesInHierarchy<T>( this Transform parent, int depth=int.MaxValue ) where T : class
	{
		if(parent == null) return null;
		List<T> result = new List<T>(4);
		_recursiveTypeSearch<T>(parent, result, depth);
		return result;
	}

	private static void _recursiveComponentSearch<TComponent>(Transform curr, List<TComponent> list, int depth) where TComponent : Component
	{
		TComponent c = curr.GetComponent<TComponent>();
		if(c != null) list.Add(c);

		if(depth > 0)
		{
			foreach(Transform child in curr)
			{
				_recursiveComponentSearch<TComponent>(child, list, depth-1);
			}
		}
	}

	private static void _recursiveTypeSearch<T>(Transform curr, List<T> list, int depth) where T : class
	{
		Component[] components = curr.GetComponents<Component>();
		foreach(Component c in components)
		{
			if(c is T)
				list.Add(c as T);
		}
		if(depth > 0)
		{
			foreach(Transform child in curr)
			{
				_recursiveTypeSearch<T>(child, list, depth-1);
			}
		}
	}

	/// <summary>
	/// Resets the transform t zero pos, rotation and scale 1,1,1
	/// </summary>
	public static void ResetTransform(this Transform trans)
	{
		trans.position = Vector3.zero;
		trans.localRotation = Quaternion.identity;
		trans.localScale = Vector3.one;
	}

	#endregion

	//=================================================================================================================

	#region rectTransform extensions

	//	rect transform helper methods
	public static void SetDefaultScale(this RectTransform trans) {
		trans.localScale = new Vector3(1, 1, 1);
	}
	public static void SetPivotAndAnchors(this RectTransform trans, Vector2 aVec) {
		trans.pivot = aVec;
		trans.anchorMin = aVec;
		trans.anchorMax = aVec;
	}
	
	public static Vector2 GetSize(this RectTransform trans) {
		return trans.rect.size;
	}
	public static float GetWidth(this RectTransform trans) {
		return trans.rect.width;
	}
	public static float GetHeight(this RectTransform trans) {
		return trans.rect.height;
	}
	
	public static void SetPositionOfPivot(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x, newPos.y, trans.localPosition.z);
	}
	
	public static void SetLeftBottomPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
	}
	public static void SetLeftTopPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
	}
	public static void SetRightBottomPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y + (trans.pivot.y * trans.rect.height), trans.localPosition.z);
	}
	public static void SetRightTopPosition(this RectTransform trans, Vector2 newPos) {
		trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width), newPos.y - ((1f - trans.pivot.y) * trans.rect.height), trans.localPosition.z);
	}
	
	public static void SetSize(this RectTransform trans, Vector2 newSize) {
		Vector2 oldSize = trans.rect.size;
		Vector2 deltaSize = newSize - oldSize;
		trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
		trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x), deltaSize.y * (1f - trans.pivot.y));
	}
	public static void SetWidth(this RectTransform trans, float newSize) {
		SetSize(trans, new Vector2(newSize, trans.rect.size.y));
	}
	public static void SetHeight(this RectTransform trans, float newSize) {
		SetSize(trans, new Vector2(trans.rect.size.x, newSize));
	}

	#endregion

	//=================================================================================================================

	#region Canvas
	/// <summary>
	/// Calulates Position for RectTransform.position from a transform.position. Does not Work with WorldSpace Canvas!
	/// </summary>
	/// <param name="_Canvas"> The Canvas parent of the RectTransform.</param>
	/// <param name="_Position">Position of in world space of the "Transform" you want the "RectTransform" to be.</param>
	/// <param name="_Cam">The Camera which is used. Note this is useful for split screen and both RenderModes of the Canvas.</param>
	/// <returns></returns>
	public static Vector3 CalculatePositionFromTransformToRectTransform(this Canvas _Canvas, Vector3 _Position, Camera _Cam)
	{
		Vector3 Return = Vector3.zero;
		if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{   
			Return = _Cam.WorldToScreenPoint(_Position);
		}
		else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			Vector2 tempVector = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_Canvas.transform as RectTransform, _Cam.WorldToScreenPoint(_Position), _Cam, out tempVector);
			Return = _Canvas.transform.TransformPoint(tempVector);
		}
		
		return Return;
	}
	
	/// <summary>
	/// Calulates Position for RectTransform.position Mouse Position. Does not Work with WorldSpace Canvas!
	/// </summary>
	/// <param name="_Canvas">The Canvas parent of the RectTransform.</param>
	/// <param name="_Cam">The Camera which is used. Note this is useful for split screen and both RenderModes of the Canvas.</param>
	/// <returns></returns>
	public static Vector3 CalculatePositionFromMouseToRectTransform(this Canvas _Canvas, Camera _Cam)
	{
		Vector3 Return = Vector3.zero;
		
		if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			Return = Input.mousePosition;
		}
		else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			Vector2 tempVector = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_Canvas.transform as RectTransform, Input.mousePosition, _Cam, out tempVector);
			Return = _Canvas.transform.TransformPoint(tempVector);
		}
		
		return Return;
	}
	
	/// <summary>
	/// Calculates Position for "Transform".position from a "RectTransform".position. Does not Work with WorldSpace Canvas!
	/// </summary>
	/// <param name="_Canvas">The Canvas parent of the RectTransform.</param>
	/// <param name="_Position">Position of the "RectTransform" UI element you want the "Transform" object to be placed to.</param>
	/// <param name="_Cam">The Camera which is used. Note this is useful for split screen and both RenderModes of the Canvas.</param>
	/// <returns></returns>
	public static Vector3 CalculatePositionFromRectTransformToTransform(this Canvas _Canvas, Vector3 _Position, Camera _Cam)
	{
		Vector3 Return = Vector3.zero;
		if (_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
		{
			Return = _Cam.ScreenToWorldPoint(_Position);
		}
		else if (_Canvas.renderMode == RenderMode.ScreenSpaceCamera)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(_Canvas.transform as RectTransform, _Cam.WorldToScreenPoint(_Position), _Cam, out Return);
		}
		return Return;
	}
	#endregion

	//=================================================================================================================

	#region gameobject extensions

	/// <summary>
	/// Performs a full recursive search for specified component and returns the first match.
	/// </summary>
	/// <returns>First TComponent found or null.</returns>
	/// <param name="parent">GameObject to start from (also gets checked for TComponent).</param>
	public static TComponent GetComponentInHierarchy<TComponent>( this GameObject parent, int depth=int.MaxValue ) where TComponent : Component
	{
		if(parent == null)
		{
			return null;
		}
		return parent.transform.GetComponentInHierarchy<TComponent>(depth);
	}

	/// <summary>
	/// Performs a full recursive search for specified component and returns all occurances.
	/// </summary>
	/// <returns>A list of all found components.</returns>
	/// <param name="parent">Transform to start from (also gets checked for TComponent).</param>
	/// <typeparam name="TComponent">The 1st type parameter.</typeparam>
	public static List<TComponent> GetComponentsInHierarchy<TComponent>( this GameObject parent, int depth=int.MaxValue ) where TComponent : Component
	{
		if(parent == null)
		{
			return null;
		}
		return parent.transform.GetComponentsInHierarchy<TComponent>(depth);
	}

	/// <summary>
	/// Performs a full recursive search for specified type and returns all occurances.
	/// You may search interfaces with with method, but the typecasting is expensive
	/// </summary>
	/// <returns>A list of all found components.</returns>
	/// <param name="parent">Transform to start from (also gets checked for TComponent).</param>
	/// <typeparam name="T">Type to search for.</typeparam>
	public static List<T> GetTypesInHierarchy<T>( this GameObject parent, int depth=int.MaxValue ) where T : class
	{
		if(parent == null) return null;
		return parent.transform.GetTypesInHierarchy<T>(depth);
	}

	/// <summary>
	/// Perform an action on each type found within a full recursive search of parent and its child hierarchy
	/// </summary>
	public static void SendMessagesToComponents<T>( this GameObject parent, System.Action<T> action, int depth=int.MaxValue ) where T : class
	{
		if(parent == null || action == null) return;
		foreach(T type in parent.transform.GetTypesInHierarchy<T>(depth))
		{
			action(type);
		}
	}

	#endregion

}

//=================================================================================================================