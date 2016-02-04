using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//=================================================================================================================

namespace JPathfinder
{

	[CreateAssetMenu]

	public class GraphData : ScriptableObject 
	{
		public const string nodeContainerName = "PathNodes";

		//	settings

		/// <summary>
		/// distance on which nodes are connected automatically if not supressed by the user
		/// </summary>
		[Range(0.5f, 12f)]
		public float autoConnectionDist = 2f;

		//	data
		public List<graphNode> 			nodes;
		public List<graphConnection>	edges;

		public bool hasContent()
		{
			return 	nodes != null && nodes.Count > 0
				&& edges != null && edges.Count > 0;
		}
	}


	public enum connectDir { both, AB, BA }

	[System.Serializable]
	public struct graphNode
	{
		public int ID;
		public Vector3 position;
	}

	[System.Serializable]
	public struct graphConnection
	{
		public int A;
		public int B;
		public connectDir dir;
		public bool forced;
		public bool supressed;
	}

}

//=================================================================================================================