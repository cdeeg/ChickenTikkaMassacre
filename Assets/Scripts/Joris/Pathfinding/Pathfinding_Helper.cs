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
		public int currIndex { get; private set; }
		public int nodeCount { get { return positions.Count; } }

		public Vector3 currNode
		{
			get {
				return (currIndex >= 0 && currIndex < positions.Count) ? positions[currIndex] : Vector3.one*-100; 
			}
		}

		public Vector3 GetNode(int index)
		{
			return positions[ Mathf.Clamp(index, 0, positions.Count-1) ];
		}

		public Path() {}
		public Path(List<Node> nodes, Vector3 start, Vector3 target)
		{
			this.nodes 		= nodes;
			this.positions 	= nodes.Select( x=> x.pos ).ToList();
			this.positions.Insert(0, start);
			this.positions.Add(target);

			string s = "path (" + positions.Count + ")";
			foreach(Vector3 p in positions)
				s += "\n\t " + p.ToString();
			Debug.Log(s);
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

			Vector3 lastNode = positions[Mathf.Clamp(currIndex-1, 0, 1000)];
			bool reachedNext = lastNode.Distance2f(currPos) >= lastNode.Distance2f(positions[currIndex]) - 0.3f;

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

	
	#endregion

}

//=================================================================================================================


