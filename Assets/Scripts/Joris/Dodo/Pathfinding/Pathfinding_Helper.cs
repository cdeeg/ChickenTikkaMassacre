using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPathfinder
{

	//== Dijkstra Node ================================================================================================

	#region dijkstra

	public class Node
	{
		public int 				ID;
		public Vector3 			pos;

		public float 			AggregatedCost;
		public Edge				LowestCostEdge;
		public List<Edge> 		AdjacentEdges = new List<Edge>();
	}

	#endregion


	//== Path Edge ====================================================================================================

	#region path edge

	//	edge between two nodes
	public class Edge
	{
		public Node A;
		public Node B;

		public Vector3 posA { get { return A.pos; } }
		public Vector3 posB { get { return B.pos; } }

		public connectDir 	Direction;
		public float 		Cost;
		public bool 		isBlocked;
		public bool			dynamicBlock;

		public Edge(Node a, Node b, connectDir dir)
		{
			A 	 = a;
			B 	 = b;
			Cost = 0;
		}
		public Edge(Node a, Node b, connectDir dir, float cost)
		{
			A	 = a;
			B	 = b;
			Direction = dir;
			Cost = cost;
		}
		
		public Node GetOther(Node node)
		{
			if(node == A)
				return B;
			else if(node == B)
				return A;
			else return null;
		}

		public bool canTravelFromNode(Node n)
		{
			if(Contains(n))
			{
				switch(Direction)
				{
				case connectDir.AB:	return n == A;
				case connectDir.BA: return n == B;
				default:			return true;
				}
			}
			return false;
		}
		
		public Node GetOtherByComp(int nodeID, int roomID)
		{
			if(A.ID == nodeID)
				return B;
			else if(B.ID == nodeID)
				return A;
			else return null;
		}
		
		public bool Compare( Edge edge )
		{
			return (edge.A == A && edge.B == B ) 
					|| (edge.A == B  && edge.B == A );
		}
		
		public bool Contains(Node p)
		{
			return p == A || p == B;
		}
	}

	#endregion

	//== Graph ========================================================================================================

	#region graph

	//	data structure representing a graph traversable with dijkstra method
	//	a graph consists of a collection of nodes and edges to connect them
	public class Graph
	{
		public float InitCost;				//	cost for traveling to first node
		public float FinishCost;			//	cost for traveling to desitnation position
		public List<Node> 	Nodes;
		public List<Edge> 	Edges;
		
		public Graph()
		{
			Nodes = new List<Node>();
			Edges = new List<Edge>();		
		}
		
		//	remove duplicates in edge listing
		public void RemoveEdgeDuplicates()
		{
			if(Edges != null & Edges.Count > 0)
			{
				for(int i = 0; i < Edges.Count; i++)
				{	
					for(int j = 0; j < Edges.Count; j++)
					{
						if( i != j && Edges[i].Compare(Edges[j]) )
						{
							if(Edges[i].isBlocked || Edges[j].isBlocked)
							{
								Edges[i].isBlocked = true;
								Edges[j].isBlocked = true;
							}
							Edges.RemoveAt(j);
						}
					}
				}			
			}
		}
		
		//	remove duplicates in node adjacent listing
		public void RemoveNodeAdjacentDuplicates()
		{
			if(Nodes == null)
				return;
			
			for(int n = 0; n < Nodes.Count; n++)
			{
				for(int a = 0; a < Nodes[n].AdjacentEdges.Count; a++)
				{
					for(int b = 0; b < Nodes[n].AdjacentEdges.Count; b++)
					{
						if(a != b && Nodes[n].AdjacentEdges[a].Compare(Nodes[n].AdjacentEdges[b]))
						{
							Nodes[n].AdjacentEdges.RemoveAt(b);
						}
					}
				}
			}
		}
		
		//	resets all nodes of the graph
		public void Reset()
		{
			for(int i = 0; i < Nodes.Count; i++)
			{
				Nodes[i].AggregatedCost = Dijkstra.INFINITY;
				Nodes[i].LowestCostEdge = null;
			}
		}
		
		public Edge FindEdge(Node p1, Node p2)
		{
			if(p1 == null || p2 == null)
				return null;
			return Edges.Find( x => x.Contains(p1) && x.Contains(p2) );
		}

		public Node GetNearestNode(Vector3 p)
		{
			float closest = float.MaxValue;
			int id = -1;
			for(int i = 0; i < Nodes.Count; i++)
			{
				float mag = Vector3.Distance(p, Nodes[i].pos);
				if(mag < closest)
				{
					closest = mag;
					id = i;
				}
			}
			return id > -1 ? Nodes[id] : null;
		}

		public string PrintEdges()
		{
			var b = new System.Text.StringBuilder();
			b.Append("graph edges:: \n");
			int counter = 0;
			foreach(Edge e in Edges)
			{
				if(++counter % 4 == 0)
				{
					counter = 1;
					b.Append("\n");
				}

				b.Append(" " + e.A.ID.ToString());
				switch(e.Direction)
				{
				case connectDir.both:	b.Append(" <-> "); break;
				case connectDir.AB:		b.Append(" -> "); break;
				case connectDir.BA:		b.Append(" <- "); break;
				}
				b.Append(e.B.ID.ToString() + "  | ");


			}
			return b.ToString();
		}
	}

	#endregion

	//=================================================================================================================

	#region movement path base


	public class Path
	{
		const float drag	= 0.92f;
		const float yOffset = 0.01f;

		private List<Node> nodes = new List<Node>(12);
		private List<Vector3> positions = new List<Vector3>(12);
		private int currIndex = 0;

		public Vector3 currNode
		{
			get {
				return (currIndex >= 0 && currIndex < positions.Count) ? positions[currIndex] : Vector3.one*-100; 
			}
		}

		public Path() {}
		public Path(List<Node> nodes, Vector3 start, Vector3 target)
		{
			this.nodes 		= nodes;
			this.positions 	= nodes.Select( x=> x.pos ).ToList();
			this.positions.Insert(0, start);
			this.positions.Add(target);
		}

		public bool isValid()
		{
			return positions.Count > 1;
		}

		public void StartPathMove()
		{
			currIndex = 0;
		}

		/// <summary>
		/// get next node position in this path
		/// returns true if path target is reached
		/// </summary>
		public bool TryGetNextNode( Vector3 currPos, out Vector3 nextPos )
		{
			if(positions == null || positions.Count == 0)
			{
				nextPos = Vector3.zero;
				return false;
			}

			Vector3 lastNode = positions[Mathf.Clamp(currIndex, 0, 1000)];
			bool reachedNext = Vector3.Distance(lastNode, currPos) > Vector3.Distance(lastNode, positions[currIndex]);

			if(reachedNext)
			{
				if(currIndex == positions.Count-1)
				{
					nextPos = positions.Last();
					return true;
				}
				else
				{
					currIndex++;
				}
			}
			nextPos = positions[currIndex];
			return false;
		}

	}



	////	class representing a whole calculated path
	////	it starts with the first node calculated by Dijkstra and ends with the actual target position
	////	the path optimizes itself on creation
	//public class MovementPath
	//{
	//	public 	  const float DefaultDrag 		= 0.92f;
	//	protected const float SlowingDistance 	= 1.1f;
	//	protected const float DefaultSlowing	= 0.8f;
	//	const float YOffset 					= 0.1f;
	//	
	//	public List<Vector3> 		Nodes;
	//	public List<DijkstraNode>	PathNodes;
	//
	//	public readonly float AddedCost;
	//	
	//	public bool canReachTarget = true;
	//
	//	//---------------------------------------------------------------------------------------------------------------
	//
	//	public MovementPath( List<DijkstraNode> nodes )
	//	{
	//		PathNodes 	= nodes;
	//		Nodes 		= new List<Vector3>();
	//
	//		int count = 0;
	//		foreach(DijkstraNode n in nodes)
	//		{
	//			Nodes.Add(n.pos + Vector3.up * YOffset);
	//			count++;
	//		}
	//		CurrentNodeIndex = 0;
	//		
	//		AddedCost = nodes[nodes.Count-1].AggregatedCost;
	//	}
	//
	//	//---------------------------------------------------------------------------------------------------------------	
	//		
	//	protected void LimitVelocity( ref Vector3 velocity, float max )
	//	{
	//		if(velocity.magnitude > max)
	//			velocity = velocity.normalized * max;
	//	}
	//	
	//	//	sets next position and returns false if end of path is reached
	//	public virtual bool TrySetNextNode( Vector3 currPos, float minDistToTarget = 0.2f )
	//	{
	//		float mag1 = Vector3.Distance( currPos, GetNodeLeveled(CurrentNode, currPos.y) );
	//		
	//		float mag2 = Vector3.Distance( currPos, GetNodeLeveled(NextNode, currPos.y) );
	//		float mag3 = Vector3.Distance( CurrentNode, NextNode );
	//				
	//		if( mag3 > mag2*0.8f || mag1 < minDistToTarget )
	//		{
	//			CurrentNodeIndex = Mathf.Clamp( CurrentNodeIndex + 1, 0, Nodes.Count-1 );
	//			OnReachNextNode();
	//			return true;
	//		}
	//		return false;
	//	}
	//	
	//	//---------------------------------------------------------------------------------------------------------------
	//	
	//	protected virtual void OnReachNextNode()
	//	{
	//
	//	}
	//	
	//	public bool hasReachedTarget( Vector3 currPos, float minDistToTarget = 0.2f )
	//	{
	//		return CurrentNodeIndex == Nodes.Count-1 
	//			&& Vector3.Distance( currPos, GetNodeLeveled(CurrentNode, currPos.y) ) < minDistToTarget;
	//	}
	//	
	//	//	returns true if entity is near path end
	//	//	used to slow down movement to the end
	//	protected bool isApproachingTarget( Vector3 entity )
	//	{
	//		return  CurrentNodeIndex >= Nodes.Count-2
	//				&& Vector3.Distance(entity, Nodes[Nodes.Count-1]) < SlowingDistance;
	//	}
	//	
	//	//---------------------------------------------------------------------------------------------------------------	
	//	
	//	public IEnumerable GetNodes()
	//	{
	//		foreach(Vector3 v in Nodes)
	//			yield return v;
	//	}
	//		
	//	public Vector3 GetNode(int id)
	//	{
	//		if(id < 0) id = 0;
	//		else if(id >= Nodes.Count) id = Nodes.Count-1;
	//		return Nodes[id];
	//	}
	//	
	//	//---------------------------------------------------------------------------------------------------------------
	//
	//	protected bool isValid(int nodeIndex) { return nodeIndex >= 0 && nodeIndex < Nodes.Count-1; }
	//		
	//	public int 		CurrentNodeIndex	{ get; protected set; }
	//	public Vector3 	NextNode 			{ get { return Nodes[Mathf.Clamp(CurrentNodeIndex+1,0,Nodes.Count-1)]; } }
	//	public Vector3 	CurrentNode 		{ get { return Nodes[CurrentNodeIndex]; } }
	//	public int 		Length				{ get { return Nodes.Count; } }
	//	
	//	protected Vector3 GetNodeLeveled(Vector3 node, float ylevel)
	//	{
	//		return new Vector3(node.x, ylevel, node.z);
	//	}
	//
	//	//---------------------------------------------------------------------------------------------------------------
	//
	////	const string splitSymbol = ";";
	////
	////	//	encodes path node ids into string
	////	public static string SerializeToString(MovementPath path)
	////	{
	////		System.Text.StringBuilder b = new System.Text.StringBuilder();
	////		for(int i = 1; i < path.PathNodes.Count; i++)
	////		{
	////			b.Append( splitSymbol );
	////			b.Append( path.PathNodes[i].UniqueID.ToString() );
	////		}
	////		return b.ToString();
	////	}
	////
	////	public static MovementPath DeserializeFromString(string serialized)
	////	{
	////		string[] ids = serialized.Split(splitSymbol);
	////
	////		List<int> nodes = new List<int>(ids.Length);
	////		for(int i = 0; i < ids.Length; i++)
	////		{
	////			int nodeID;
	////			if(!System.Int32.TryParse(ids[i], out nodeID))
	////			{
	////				throw new System.Exception("error while deserializing path: id array could not be parsed from string. " +
	////				                           "\nFull string: " + serialized);
	////			}
	////			else
	////			{
	////				nodes.Add(nodeID);
	////			}
	////		}
	////		return new MovementPath( nodes );
	////	}
	//}

	#endregion

}

//=================================================================================================================


