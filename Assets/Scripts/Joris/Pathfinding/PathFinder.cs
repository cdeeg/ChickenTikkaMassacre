using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//=================================================================================================================


namespace JPathfinder
{

	public class PathFinder : MonoBehaviour 
	{
		#region properties

		public GraphData graphData;

		[Tooltip("shows the whole graph in the scene")]
		public bool _debugGraph;
		[Tooltip("display node ids")]
		public bool _debugNodeIDs;
		[Tooltip("shows aggregated costs of all nodes from current source")]
		public bool _debugDijkstraCost;
		[Tooltip("shows last calculated path")]
		public bool _debugLastPath;
		[Tooltip("shows lowest cost edges to last source node")]
		public bool _debugLowestCostGraph;
		[Tooltip("testupdates a path from 0,0,0 to current cursor position")]
		public bool _debugPathFinder;

		/// <summary>
		/// last calculated path
		/// </summary>
		private static List<Node> lastPath;
		/// <summary>
		/// last source node for dijkstra
		/// </summary>
		private static Node lastSrcNode;

		#endregion

		//=================================================================================================================

		#region singleton

		public static PathFinder instance
		{
			get {  
				if(_instanceBacking == null)
				{
					GameObject go = new GameObject("Pathfinder");
					_instanceBacking = go.AddComponent<PathFinder>();
				}
				return _instanceBacking;
			}
		}
		private static PathFinder _instanceBacking;

		#endregion

		//=================================================================================================================

		#region interface

		public static Graph graph { get; private set; }

		public static Path GetPath(Vector3 start, Vector3 target)
		{
			if(graph == null)
			{
				throw new System.Exception("You must setup a graph first before attempting to calculate paths!");
				return null;
			}

			Node srcNode = graph.GetNearestNode(start);
			Node targetNode = graph.GetNearestNode(target);

			if(srcNode != null && targetNode != null)
			{
				Dijkstra.Perform(graph, srcNode);
				List<Node> nodes = Dijkstra.RetrieveShortestPath(srcNode, targetNode);

				lastPath = nodes;
				lastSrcNode = srcNode;

				return new Path(nodes, start, target);
			}
			else
			{
				throw new System.Exception("could not find start & target node in pathfinder graph!");
				return null;
			}
		}

		public static Node GetNextNodeRandom(Vector3 p)
		{
			return GetNextNodeRandom(graph.GetNearestNode(p));
		}

		public static Node GetNextNodeRandom(Node n)
		{
			if(n != null && n.AdjacentEdges.Count > 0)
			{
				return n.AdjacentEdges[Random.Range(0, n.AdjacentEdges.Count)].GetOther(n);
			}
			return null;
		}

		#endregion

		//=================================================================================================================

		#region init

		void Awake()
		{
			if(graphData != null)
			{
				SetupGraph(graphData);
			}
		}

		Vector3 currTarget;
		void Update()
		{
			if(_debugPathFinder
			   && graph != null
			   && Input.GetMouseButton(0))
			{
				Vector3 groundP = Vector3.zero;
				RaycastHit hit;
				if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, LayerMask.GetMask("Ground") ))
				{
					groundP = hit.point;
					if(groundP != currTarget)
					{
						currTarget = groundP;
						GetPath(Vector3.zero, currTarget);
					}
				}
			}
		}

		/// <summary>
		/// fetches pathnode data from the scene and constructs the levelgraph 
		/// </summary>
		public static void SetupGraph(GraphData data)
		{
			graph = new Graph();
			if(data.hasContent())
			{
				//	create nodes
				var nodes = data.nodes.Select((graphNode n)=> {
					Node node = new Node();
					node.pos = n.position;
					node.ID = n.ID;
					return node;
				})
				.ToList();
			
				//	create edges
				var edges = data.edges.Select((graphConnection c)=> {

					Node a = nodes.Find(x=> x.ID==c.A);
					Node b = nodes.Find(x=> x.ID==c.B);
					if(a == null || b == null)
					{
						throw new System.Exception("could not load edge, wrong IDs stored: "
						                           + c.A + " / " + c.B + " nodecount: " + nodes.Count);
					}

					if(!c.supressed
					   && (c.forced || GetEdgeCost(a, b) <= data.autoConnectionDist))
					{
						Edge e = new Edge(a, b, c.dir, GetEdgeCost(a, b));
						a.AdjacentEdges.Add(e);
						b.AdjacentEdges.Add(e);
						return e;
					}
					else 
						return null;
				})
				.Where(z=> z != null)
				.ToList();

				//	create graph
				graph.Nodes = nodes;
				graph.Edges = edges;


				Debug.Log(graph.PrintEdges());

			}
			else
			{
				throw new System.Exception("could not load graph; Graphdata is empty.");
			}
		}

		#endregion

		//=================================================================================================================

		#region util

		static float GetEdgeCost(Node a, Node b)
		{
			return Vector3.Distance(a.pos, b.pos);
		}

		#endregion

		//=================================================================================================================

		#region debug

		void OnGUI()
		{
			if((_debugNodeIDs || _debugDijkstraCost) && graph != null)
			{
				foreach(Node n in graph.Nodes)
				{
					string s = "";
					float w = 0;
					if(_debugNodeIDs && _debugDijkstraCost)
					{
						w = 32;
						s = n.ID.ToString() + ": " + (Mathf.RoundToInt(n.AggregatedCost*100f)/100f).ToString();
					}
					else if(_debugNodeIDs)
					{
						w = 16;
						s = n.ID.ToString();
					}
					else if(_debugDijkstraCost)
					{
						w = 16;
						s = (Mathf.RoundToInt(n.AggregatedCost*100f)/100f).ToString();
					}

					Vector2 p = Camera.main.WorldToScreenPoint(n.pos);
					p.y = Screen.height - p.y;
					Rect r = new Rect(p.x-w, p.y-32, w*2, 28);
					GUI.Box(r, s);
				}
			}
		}

		//-----------------------------------------------------------------------------------------------------------------

		const float yOff = 0.01f;
		const float shorten = 0.125f;
		const float capSize = 0.05f;


		void OnDrawGizmos()
		{
			if(graph != null)
			{
				if(_debugGraph)
				{

					//	draw whole graph
					Gizmos.color = Color.white;
					for(int i = 0; i < graph.Nodes.Count; i++)
					{
						Gizmos.DrawIcon(graph.Nodes[i].pos + Vector3.up*yOff, "nodeTex", false);
					}

					foreach(Edge e in graph.Edges)
					{
						Vector3 v = (e.posB - e.posA).normalized;
						switch(e.Direction)
						{
						case connectDir.both:
							Vector3 vv = Vector3.Cross((e.posB-e.posA).normalized, Vector3.up).normalized * 0.015f;
							Gizmos.color = Color.green;
							DrawConnection(e.posA, e.posB + vv - v * shorten * 0.25f, false);
							DrawConnection(e.posB, e.posA - vv + v * shorten * 0.25f, false);
							break;
						case connectDir.AB:
							Gizmos.color = Color.cyan;
							DrawConnection(e.posA, e.posB - v * shorten, true);
							break;
						case connectDir.BA:
							Gizmos.color = Color.cyan;
							DrawConnection(e.posB, e.posA + v * shorten, true);
							break;
						}
					}
					Gizmos.color = Color.white;
				}

				//	draw lowest cost graph
				Gizmos.color = Color.yellow;
				foreach(Node n in graph.Nodes)
				{
					if(n.LowestCostEdge != null)
					{
						Gizmos.DrawLine(n.LowestCostEdge.posA, n.LowestCostEdge.posB);
					}
				}

				//	draw last path
				if(_debugLastPath
				   && lastPath != null)
				{
					Gizmos.color = Color.red;
					for(int i = 0; i < lastPath.Count-1; i++)
					{
						Gizmos.DrawWireSphere(lastPath[i].pos, 0.2f);

						Vector3 vv = Vector3.Cross((lastPath[i+1].pos-lastPath[i].pos).normalized, Vector3.up);
						vv = vv.normalized * 0.02f;
						Gizmos.DrawLine(lastPath[i].pos + vv, lastPath[i+1].pos + vv);
					}
				}

				//	draw source node
				if(_debugDijkstraCost && lastSrcNode != null)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawWireSphere(lastSrcNode.pos, 0.3f);
				}
			}
		}


		static void DrawConnection(Vector3 p1, Vector3 p2, bool drawCap)
		{
			p1 += Vector3.up*yOff;
			p2 += Vector3.up*yOff;
			Gizmos.DrawLine(p1, p2);

			if(drawCap)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawSphere(p2, capSize);
		//		Gizmos.DrawCube(p2, Vector3.one*capSize);
			}
		}

		static string PrintPathIds(IEnumerable<Node> nodes)
		{
			var b = new System.Text.StringBuilder();
			b.Append("path:: ");
			foreach(Node n in nodes)
			{
				b.Append(" " + n.ID + " | ");
			}
			return b.ToString();
		}

		#endregion

	}



}


//=================================================================================================================