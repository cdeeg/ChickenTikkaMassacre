using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;

//=================================================================================================================

namespace jUtility
{

	public static class UnityUtil
	{
		
		//-- UI helper ----------------------------------------------------------------------------------

		#region UI helper

		public static Ray GetUIToWorldRay( Vector2 v )
		{
			return Camera.main.ScreenPointToRay( new Vector3( v.x, Screen.height-v.y, 0 ) );
		}
		
		public static Ray GetScreenToWorldRay( Vector3 v, Camera c=null )
		{
			if(c != null)
				return c.ScreenPointToRay( v );
			else
				return Camera.main.ScreenPointToRay( v );
		}
		
		//------------------------------------------
		
		public static Vector2 GetWorldToScreenPos( Vector3 p, Camera c=null )
		{
			if(c == null)
				c = Camera.main;
			return Camera.main.WorldToScreenPoint(p);
		}
		
		public static Vector2 GetWorldToUIPos( Vector3 p, Camera c=null )
		{
			if(c == null)
				c = Camera.main;
			Vector2 pos = c.WorldToScreenPoint(p);
			pos.y = Screen.height - pos.y;
			return pos;
		}
		
		public static Vector2 InvertScreenPos( Vector2 p )
		{
			return new Vector2(p.x, Screen.height-p.y);
		}
		
		public static Vector2 InvertScreenPos( Vector3 p )
		{
			return new Vector2(p.x, Screen.height-p.y);
		}

		//------------------------------------------


		#endregion


		//-- Raycast helper -----------------------------------------------------------------------------

		#region raycasting

		//	returns a usable raycast layermask value searching the specified masks
		public static int GetRayCastSearchMask( params LayerMask[] masks) 
		{
			if(masks.Length > 1)
				return CombineMasks(false, masks);
			else if(masks.Length > 0)
				return 1 << masks[0].value; 
			
			return 0;
		}
		//	returns a usable raycast layermask value ignoring the specified masks
		public static int GetRayCastIgnoreMask( params LayerMask[] masks)
		{ 
			if(masks.Length > 1)
				return CombineMasks(true, masks);
			else if(masks.Length > 0)
				return ~( 1 << masks[0].value ); 
			return 0;
		}
		//	combines layermasks (not thoroughly tested! )
		public static int CombineMasks( bool invert, params LayerMask[] masks )
		{
			if(masks.Length > 0)
			{
				LayerMask m = (1 << masks[0].value);
				for(int i = 1; i < masks.Length; i++)
				{
					m |= (1 << masks[i].value);
				}
				if(invert)
					m = ~m;
				return m;
			}
			return 0;
		}
		
		//	layermasked linecast between two points
		public static bool NoObstacleBetween(Vector3 p1, Vector3 p2, float yOffset = 0, int layerMask = 0)
		{
			return !Physics.Linecast(p1+Vector3.up*yOffset, p2+Vector3.up*yOffset, layerMask);
		}

		#endregion
		
		//-- Searching ----------------------------------------------------------------------------------

		#region searching

		//	searches for a specified interface in any given GameObject 
		public static TInterface SearchForInterface<TInterface>( GameObject g )
			where TInterface : class
		{
			if(typeof(TInterface).IsInterface)
			{
				Component[] comps = g.transform.root.GetComponents(typeof(Component));
				foreach(Component c in comps)
				{
					if(c is TInterface)
						return c as TInterface;
				}
			}	
			return null;
		}
		
		//------------------------------------------
		
		public static bool TransformIsChildOf( Transform child, Transform hierarchy )
		{
			if( hierarchy == null )	return false;
			return FindChildInHierarchy(hierarchy, child.name) != null;
		}
		
		public static Transform FindChildInHierarchy( Transform parent, string search )
		{
			if( parent.name.Equals(search) ) return parent;
			Transform[] childs = parent.GetComponentsInChildren<Transform>();

			foreach( Transform child in childs )
				if(child.name.Equals(search))
					return child;
			
			return null;
		}

		//	same as extension method::

		
		//------------------------------------------
		
		public static List<Collider> GetCollidersFromHierarchy( Transform parent, List<Collider> found = null )
		{
			if(found == null)
				found = new List<Collider>();
			
			if(parent.GetComponent<Collider>() != null)
				found.Add(parent.GetComponent<Collider>());
			foreach(Transform child in parent)
			{
				found = GetCollidersFromHierarchy( child, found );
			}
			return found;
		}

		#endregion
		
		//-- Camera -------------------------------------------------------------------------------------

		#region camera

		//	returns if a point is inside the camera viewport
		//	this is cheaper than frustrum testing, but doesn't take
		//	overlapping objects or boundaries into account
		public static bool TestPointOnScreen( Camera c, Vector3 p )
		{
			Vector2 screenP = Camera.main.WorldToViewportPoint( p );
			return 	screenP.x >= 0 && screenP.x <= 1
				&& screenP.y >= 0 && screenP.y <= 1;
		}
		
		//	returns true if the given bounds are in or intersecting the given camera view
		//	tolerance expands the bounds size
		public static bool TestBoundsIntersectViewFrustrum( Camera c, Bounds b, float tolerance )
		{
			b.Expand(tolerance);
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
			return GeometryUtility.TestPlanesAABB(planes, b);
		}
		
		//	returns true if the given bounds are fully inside the given camera view - more expensive
		//	tolerance expands the bounds size
		public static bool TestBoundsInViewFrustrum( Camera c, Bounds b, float tolerance=0 )
		{
			b.Expand(tolerance);
			
			Bounds[] result = new Bounds[8];
			result[0] = new Bounds( b.center + new Vector3(b.extents.x, b.extents.y, b.extents.z), Vector3.one*0.01f );
			result[1] = new Bounds( b.center + new Vector3(-b.extents.x, b.extents.y, b.extents.z), Vector3.one*0.01f );
			result[2] = new Bounds( b.center + new Vector3(-b.extents.x, b.extents.y, -b.extents.z), Vector3.one*0.01f );
			result[3] = new Bounds( b.center + new Vector3(b.extents.x, b.extents.y, -b.extents.z), Vector3.one*0.01f );
			
			result[4] = new Bounds( b.center + new Vector3(b.extents.x, -b.extents.y, b.extents.z), Vector3.one*0.01f );
			result[5] = new Bounds( b.center + new Vector3(-b.extents.x, -b.extents.y, b.extents.z), Vector3.one*0.01f );
			result[6] = new Bounds( b.center + new Vector3(-b.extents.x, -b.extents.y, -b.extents.z), Vector3.one*0.01f );
			result[7] = new Bounds( b.center + new Vector3(b.extents.x, -b.extents.y, -b.extents.z), Vector3.one*0.01f );
			
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(c);
			
			bool isFullyVisible = true;
			for(int i = 0; i < 8; i++)
			{
				if( !GeometryUtility.TestPlanesAABB(planes, result[i]) )
					isFullyVisible = false;
			}
			return isFullyVisible;
		}

		#endregion

		//-- Misc ---------------------------------------------------------------------------------------

		/// <summary>
		/// Creates a Texture2D out of a Sprite (performance-heavy!)
		/// Works with single sprites as well as atlased ones
		/// </summary>
		public static Texture2D GetTextureFromSprite(Sprite sprite)
		{
			if(sprite.rect.width != sprite.texture.width)
			{
				Texture2D newText = new Texture2D((int)sprite.rect.width,(int)sprite.rect.height);
				Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x, 
				                                             (int)sprite.textureRect.y, 
				                                             (int)sprite.textureRect.width, 
				                                             (int)sprite.textureRect.height );
				newText.SetPixels(newColors);
				newText.Apply();
				return newText;
			} 
			else
				return sprite.texture;
		}


		/// <summary>
		/// Destroys a component and all components that have a dependency to it via the [RequireComponent] tag.
		/// The operation is expensive as it uses reflection and Unity's DestroyImmediate()
		/// </summary>
		public static void DestroyComponentWithDependencies(Component c)
		{
			System.Type typetoDestroy = c.GetType();
			Component[] components = c.GetComponents<Component>();

			for(int i = 0; i < components.Length; i++)
			{
				if(components[i] != null && components[i] != c)
				{
					var attr = components[i].GetType().GetCustomAttributes(typeof(RequireComponent), true) as RequireComponent[];

					for(int j = 0; j < attr.Length; j++)
					{

						if(attr != null)
						{
							if(attr[j].m_Type0 != null && attr[j].m_Type0.Equals(typetoDestroy))
							{
								DestroyComponentWithDependencies(components[i]);
								continue;
							}
							if(attr[j].m_Type1 != null && attr[j].m_Type1.Equals(typetoDestroy))
							{
								DestroyComponentWithDependencies(components[i]);
								continue;
							}
							if(attr[j].m_Type2 != null && attr[j].m_Type2.Equals(typetoDestroy))
							{
								DestroyComponentWithDependencies(components[i]);
								continue;
							}
						}
					}
				}
			}
			Component.DestroyImmediate(c);
		}
	
		//	analyzes given effect hierarchy considering ParticleSystems and Animators,
		//	and returning the full duration 
		//	note: the animator component can only be analyzed during edit mode,
		//	otherwise only particle systems are checked.
//		public static float GetEffectDuration(GameObject effectRoot, float currLength)
//		{
//			#if UNITY_EDITOR
//			Animator a = effectRoot.GetComponent<Animator>();
//			if(a != null)
//			{
//				currLength = GetAnimatorDuration(a, currLength);
//			}
//			#endif
//			ParticleSystem p = effectRoot.GetComponent<ParticleSystem>();
//			if(p != null)
//			{
//				Debug.Log("particle system " + p.name + " duration: " + p.duration + " delay: " + p.startDelay + " :: " + (p.duration+p.startDelay));
//				if(p.duration+p.startDelay > currLength)
//					currLength = p.duration+p.startDelay;
//			}
//			foreach(Transform child in effectRoot.transform)
//			{
//				currLength = GetEffectDuration(child.gameObject, currLength);
//			}
//			return currLength;
//		}
//		static float GetAnimatorDuration(Animator a, float currLength)
//		{
//			#if UNITY_EDITOR
//			Debug.Log("check animator duration of " + a.name);
//			UnityEditorInternal.AnimatorController ac 			= a.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
//			if(ac != null)
//			{
//				UnityEditorInternal.AnimatorControllerLayer layer 	= ac.GetLayer(0);
//				if(layer != null)
//				{
//					UnityEditorInternal.StateMachine sm = layer.stateMachine;
//					for(int i = 0; i < sm.stateCount; i++) 
//					{
//						UnityEditorInternal.State state = sm.GetState(i);
//						AnimationClip clip = state.GetMotion() as AnimationClip;
//						if(clip != null) 
//						{
//							Debug.Log("animator clip " + i + " duration: " + clip.length);
//							currLength = clip.length > currLength ? clip.length : currLength;
//						}
//					}
//				}
//				else
//					Debug.Log(RichTextUtil.color("Error: empty Animator found", Color.red));
//			}
//			else
//				Debug.Log(RichTextUtil.color("Error: empty Animator found", Color.red));
//
//
//			#endif
//			return currLength;
//		}
	}
}

//=================================================================================================================