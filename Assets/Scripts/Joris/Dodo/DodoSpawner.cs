using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JPathfinder;

//=================================================================================================================

namespace jChikken
{

	public class DodoSpawner : MonoBehaviour 
	{
		#region fields

		/// <summary>
		/// radius of the spawn area
		/// </summary>
		[Tooltip("determines area in which the dodo may spawn. The dodo will try to stay inside the radius.")]
		public float radius = 2.5f;

		public float exploreRadius = 20f;

		public DodoBehaviour mDodo;

		// <summary>
		/// nodes inside the dodo's spawn area
		/// </summary>
		private List<Node> spawnArea = new List<Node>(12);
		private List<Node> exploreArea = new List<Node>(20);

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region interface

		public void Respawn()
		{
			Debug.Log("dodospawner respawn: " + mDodo != null + " " + (!mDodo.hasSpawned));
			if(mDodo != null && !mDodo.hasSpawned)
			{
				mDodo.OnSpawn(GetRandomNodeInSpawnArea().pos + Vector3.up * 1.5f);
			}
		}

		public Node GetRandomNodeInSpawnArea()
		{
			if(spawnArea.Count > 0)
			{
				return spawnArea[ Random.Range(0, spawnArea.Count) ];
			}
			return null;
		}

		public Node GetRandomNodeInExploreArea()
		{
			if(exploreArea.Count > 0)
			{
				return exploreArea[ Random.Range(0, exploreArea.Count) ];
			}
			return null;
		}

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region init

		void Awake()
		{
			mDodo = GameObject.FindObjectOfType<DodoBehaviour>();
		}

		void Start () 
		{
			SetupPathEngine();
		}

		void SetupPathEngine()
		{
			foreach(Node n in PathFinder.graph.Nodes)
			{
				float d = Vector3.Distance(n.pos, transform.position);
				if( Mathf.Abs(n.pos.y-transform.position.y) <= DodoBehaviour._minHeightStep &&
				    d <= radius)
				{
					spawnArea.Add(n);
				}
				else if(d <= exploreRadius)
				{
					exploreArea.Add(n);
				}
			}
		}

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region helper

		void OnDrawGizmosSelected()
		{
			if(radius > 0)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(transform.position, radius);
			}

			if(exploreRadius > 0)
			{
				Gizmos.color = new Color(1,1,1,0.75f);
				Gizmos.DrawWireSphere(transform.position, exploreRadius);
			}
			Gizmos.color = Color.white;
		}

		#endregion

	}

}

//=================================================================================================================
