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


		// <summary>
		/// nodes inside the dodo's spawn area
		/// </summary>
		private List<Node> spawnArea = new List<Node>(12);

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region interface

		public Node GetRandomNodeInSpawnArea()
		{
			if(spawnArea.Count > 0)
			{
				return spawnArea[ Random.Range(0, spawnArea.Count) ];
			}
			return null;
		}

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region init

		void Start () 
		{
			SetupPathEngine();
		}

		void SetupPathEngine()
		{
			foreach(Node n in PathFinder.graph.Nodes)
			{
				if( Mathf.Abs(n.pos.y-transform.position.y) <= DodoBehaviour._minHeightStep &&
				   Vector3.Distance(n.pos, transform.position) < radius)
				{
					spawnArea.Add(n);
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
		}

		#endregion

	}

}

//=================================================================================================================
