using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JPathfinder
{

	//== Dijkstra Algorithm ===========================================================================================

	#region dijkstra

	//	implementation of Dijkstra's algorithm,
	//	which solves the shortest path problem in a graph
	//	Perform() is called first. Given the starting(source) node, 
	//	it calculates all minimum cost path from any node to the source
	//	Next, RetrieveShortestPath() returns a shortest path from given target node to the source

	public static class Dijkstra
	{
		const bool _DEBUG = false;
		
		public const float INFINITY = Mathf.Infinity;
		static Graph MGraph;
		static List<Node> Queue;
		
		//---------------------------------------------------------------------------------------------------------------
		
		//	weights all graph nodes to given source node
		public static bool Perform( Graph graph, Node source )
		{
			if( source == null || !graph.Nodes.Contains(source) )
				return false;
			
			if(_DEBUG)
				Debug.Log("----->START DIJKSTRA " + source.ID);
			
			MGraph 		  = graph;
			MGraph.Reset();

			source.AggregatedCost = 0;
			bool finished = false;
			Queue = new List<Node>(graph.Nodes);
					
			int count = 0;
			while(!finished)
			{
				Node nextNode = Queue.OrderBy( c => c.AggregatedCost ).FirstOrDefault( c => !float.IsPositiveInfinity(c.AggregatedCost) );
				if(nextNode != null)
				{
					ProcessNode(nextNode);
					Queue.Remove(nextNode);
					count++;
				}
				else
					finished = true;
			}		

			if(_DEBUG)
				Debug.Log ("------>Finish Dijkstra " + count);
			
			return true;
		}
		
		//---------------------------------------------------------------------------------------------------------------
		
		//	calculates cost of the all edges to the given node
		//	if it provides a better path to source
		static void ProcessNode(Node node)
		{
			foreach(Edge e in node.AdjacentEdges)
			{	
		//		Debug.Log("process edge of " + node.ID + ".... blocked? " + e.isBlocked + " canTravel? " + e.canTravelFromNode(node));
				if( !e.isBlocked && e.canTravelFromNode(node) )
				{
					float cost = node.AggregatedCost + e.Cost;
					if(	cost < e.GetOther(node).AggregatedCost )
					{
						e.GetOther(node).AggregatedCost = cost;
						e.GetOther(node).LowestCostEdge = e;	
		//				Debug.Log(node.ID + " connected to " + e.GetOther(node).ID + "... -> " + cost);
					}
				}
			}
		}
		
		//---------------------------------------------------------------------------------------------------------------
		
		//	traverses the graph from given target node to source
		public static List<Node> RetrieveShortestPath( Node src, Node destination )
		{
			if(	MGraph == null 
			    || !MGraph.Nodes.Contains(src) 
			   	|| !MGraph.Nodes.Contains(destination))
			{
				return null;
			}
					
			List<Node> path = new List<Node>();
			
			Node current = destination;
			path.Add(destination);
			bool noPath = false;
			
			if(!float.IsInfinity(current.AggregatedCost))
			{
				while( current.LowestCostEdge != null )
				{	
	//				Debug.Log("OTHER? " + (current.EdgeWithLowestCost.GetOther(current) != null) );
			//		Debug.Log(current.UniqueID + " to " + current.EdgeWithLowestCost.GetOther(current).UniqueID);
	//				Debug.Log((current as GridNode).TileX + " " + (current as GridNode).TileY + "   |   " 
	//							+ (current.EdgeWithLowestCost.GetOther(current) as GridNode).TileX
	//							+ (current.EdgeWithLowestCost.GetOther(current) as GridNode).TileY);
	//				
					path.Add(current);
					
					if(	current.LowestCostEdge == null
						&& current.AggregatedCost != 0)
					{
						noPath = true;
					}
					current = current.LowestCostEdge.GetOther(current);
				}
			}
			else
				noPath = true;
			
			if(noPath)
			{	
				//	if path could not be found, retrieve the last possible node and return a path to this
				Node lastPossible = GetUnfinishedPath(); 			
				return RetrieveShortestPath( src, lastPossible );
			}
			else
			{
				path.Add(src);
				path.Reverse();
				return path;
			}
		}
		
		//---------------------------------------------------------------------------------------------------------------
		
		public static List<Node> RetrieveAllReachableNodes( float maxMovement )
		{
			List<Node> result = new List<Node>();
			foreach(Node node in MGraph.Nodes) 
			{
				if(node.AggregatedCost <= maxMovement)
					result.Add(node);
			}
			return result;
		}

		
		//---------------------------------------------------------------------------------------------------------------
		
		//	returns the the node before the first blocked edge
		//	to retrieve the most accurate possible path
		static Node GetUnfinishedPath()
		{
			foreach(Node node in MGraph.Nodes)
			{
				if(!float.IsInfinity(node.AggregatedCost))
				{
					foreach(Edge e in node.AdjacentEdges)
						if( float.IsInfinity(e.GetOther(node).AggregatedCost) )
							return node;
				}
			}
			return null;
		}	
	}

	#endregion

}

//=================================================================================================================