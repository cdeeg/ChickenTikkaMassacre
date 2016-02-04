using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace jChikken
{
	[RequireComponent(typeof(BoxCollider))]
	public class HitTester : MonoBehaviour 
	{
		public Player mPlayer;
		public Collider[] ignore;

		List<GameObject> inHitZone = new List<GameObject>();
	
		public T GetTarget<T>() where T : Component
		{
			inHitZone.RemoveAll(x=> x==null);
			foreach(GameObject go in inHitZone)
			{
				T c = go.GetComponent<T>();
				if(c != null)
					return c;
			}
			return null;
		}


		void OnTriggerEnter(Collider other)
		{
			if(!ignore.Contains(other))
			{
				if(!inHitZone.Contains(other.gameObject))
				{
					inHitZone.Add(other.gameObject);
				}
			}
		}

		void OnTriggerExit(Collider other)
		{
			inHitZone.Remove(other.gameObject);
		}
	}

}