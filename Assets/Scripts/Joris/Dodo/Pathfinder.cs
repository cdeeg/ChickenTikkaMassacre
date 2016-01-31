//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//
////=================================================================================================================
//
//public static class Pathfinder
//{
//	const float MinWorldDist = 1;
//
//	static GridTile 			lastGridStart;
//	static GridTile 			lastGridTarget;
//	static GridPath			 	lastGridPath;
//
//	static WorldPath			lastWorldPath;
//	
////	public static GridPath GetPathOnGrid( Room room, GridTile start, GridTile target, float movementLeft = 1000 )
////	{
////		if(start == target)
////			return null;
////		
////		if( start != lastGridStart || target != lastGridTarget )
////		{
////			room.MGrid.UpdateCurrentGraph( start, target );
////			Dijkstra.Perform( room.MGrid.CurrentGraph, start.MNode );
////			
////			List<DijkstraNode> nodes 	= Dijkstra.RetrieveShortestPath( target.MNode );
////			if(nodes != null)
////			{
////				GridPath path				= new GridPath( nodes );
////				lastGridStart 				= start;
////				lastGridTarget 				= target;
////				lastGridPath				= path;
////			}
////			else 
////				return null;
////		}
////		return lastGridPath;
////		
////	}
//	
//	public static GridPath GetPathOnGrid( Graph gridGraph, GridTile start, GridTile target, float movementLeft = 1000 )
//	{
//		if(start == target)
//			return null;
//		
//		if( start != lastGridStart || target != lastGridTarget )
//		{
//			Dijkstra.Perform( gridGraph, start.MNode );
//			
//			List<DijkstraNode> nodes 	= Dijkstra.RetrieveShortestPath( target.MNode );
//			if(nodes != null)
//			{
//				GridPath path	= new GridPath( nodes );
//				lastGridStart 	= start;
//				lastGridTarget 	= target;
//				lastGridPath	= path;
//			}
//			else 
//				return null;
//		}
//		return lastGridPath;
//		
//	}
//
//	public static WorldPath GetPathInWorld( Vector3 startP, Vector3 targetP, float movementLeft = 1000 )
//	{
//		if( Vector3.Distance(startP, targetP) < MinWorldDist )
//			return null;
//
//		DijkstraNode start 	= GetNearestNode( startP, Level.MGraph );
//		DijkstraNode target = GetNearestNode( targetP, Level.MGraph );
//
//		Dijkstra.Perform( Level.MGraph, start );
//		List<DijkstraNode> nodes = Dijkstra.RetrieveShortestPath( target );
//
//		if(nodes != null)
//		{
//			lastWorldPath = new WorldPath( nodes, startP, targetP );
//			return lastWorldPath;
//		}
//
//		return null;
//	}
//	
//	static DijkstraNode GetNearestNode( Vector3 pos, Graph graph )
//	{
//		float[] distances = new float[ graph.Nodes.Count ];
//		for(int i = 0; i < distances.Length; i++)
//			distances[i] = Vector3.Distance( pos, graph.Nodes[i].pos );
//		return graph.Nodes[ Utility.GetShortestDistanceID( distances ) ];
//	}
//}
//
///*void TestGridPath(int x, int y)
//	{
//		Debug.Log("---------------------------> TEST GRID PATH");
//		
//		Room room 			= RoomManager.GetRoomByID(1);
//		GridTile a			= room.MGrid.GetTileByIndices(4, 7);
//		GridTile b			= room.MGrid.GetTileByIndices(x, y);
//		DijkstraNode start 	= a.MNode;
//		DijkstraNode end	= b.MNode;
//		
//		Debug.Log(a.TileID + " " + start.AdjacentEdges.Count + " " + b.TileID + " " + end.AdjacentEdges.Count);
//		
//		Dijkstra.Perform(room.MGrid.MGraph, start);
//		List<DijkstraNode> path = Dijkstra.RetrieveShortestPath(end);
//		
//		if(path != null && path.Count > 2)
//		{
//			List<Vector3> positions = new List<Vector3>();
//			foreach(DijkstraNode node in path)
//			{
//				positions.Add(node.pos);
//			}
//			
//		//	MPathVisual.CreatePath( positions );
//			room.MGrid.DisplayGridPath( path );
//		}
//	}*/
//
////=================================================================================================================
//
//#region movement path base
//
////	class representing a whole calculated path
////	it starts with the first node calculated by Dijkstra and ends with the actual target position
////	the path optimizes itself on creation
//public abstract class MovementPath
//{
//	public 	  const float DefaultDrag 		= 0.92f;
//	protected const float SlowingDistance 	= 1.1f;
//	protected const float DefaultSlowing	= 0.8f;
//	const float YOffset 					= 0.1f;
//	
//	public List<Vector3> 		Nodes;
//	public List<DijkstraNode>	PathNodes;
//	List<int> 					RoomNodes;
//	
//	public readonly float AddedCost;
//	
//	public bool canReachTarget = true;
//
//	public MovementPath( List<DijkstraNode> nodes )
//	{
//		PathNodes 	= nodes;
//		Nodes 		= new List<Vector3>();
//		RoomNodes 	= new List<int>();
//		
//		int count = 0;
//		foreach(DijkstraNode n in nodes)
//		{
//			Nodes.Add(n.pos + Vector3.up * YOffset);
//			count++;
//			if(!RoomNodes.Contains(n.RoomID))
//				RoomNodes.Add(n.RoomID);
//		}
//		CurrentNodeIndex = 0;
//		
//		AddedCost = nodes[nodes.Count-1].AggregatedCost;
//	}
//	
//	//-------------------------------------	
//		
//	protected void LimitVelocity( ref Vector3 velocity, float max )
//	{
//		if(velocity.magnitude > max)
//			velocity = velocity.normalized * max;
//	}
//	
//	//-------------------------------------
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
//	//-------------------------------------
//	
//	protected abstract void OnReachNextNode();
//	
//	//-------------------------------------
//	
//	public bool hasReachedTarget( Vector3 currPos, float minDistToTarget = 0.2f )
//	{
//		return CurrentNodeIndex == Nodes.Count-1 
//			&& Vector3.Distance( currPos, GetNodeLeveled(CurrentNode, currPos.y) ) < minDistToTarget;
//	}
//	
//	//-------------------------------------
//	
//	//	returns true if entity is near path end
//	//	used to slow down movement to the end
//	protected bool isApproachingTarget( Vector3 entity )
//	{
//		return  CurrentNodeIndex >= Nodes.Count-2
//				&& Vector3.Distance(entity, Nodes[Nodes.Count-1]) < SlowingDistance;
//	}
//	
//	//-------------------------------------	
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
//	//-------------------------------------
//	
//	public bool PassesRoom(int roomID) { return RoomNodes.Contains(roomID); }
//	protected bool isValid(int nodeIndex) { return nodeIndex >= 0 && nodeIndex < Nodes.Count-1; }
//		
//	public int 		CurrentNodeIndex	{ get; protected set; }
//	public Vector3 	NextNode 			{ get { return Nodes[Mathf.Clamp(CurrentNodeIndex+1,0,Nodes.Count-1)]; } }
//	public Vector3 	CurrentNode 		{ get { return Nodes[CurrentNodeIndex]; } }
//	public int 		Length				{ get { return Nodes.Count; } }
//	
//	protected Vector3 GetNodeLeveled(Vector3 node, float level)
//	{
//		return new Vector3(node.x, level, node.z);
//	}
//
//	//	encodes path node ids into string
//	public string GetSerializedPath()
//	{
//		string[] ids = PathNodes.Select( x => x.UniqueID ).ToArray();
//		string merged = ids[0];
//		for(int i = 1; i < ids.Length; i++)
//			merged += ";" + ids[i];
//		return merged;
//	}
//}
//
//#endregion
//
////-------------------------------------------------------------------------------------------
//
//#region gridpath
//
//public class GridPath : MovementPath
//{	
//	public bool[] MovementMask;		//	stores which nodes a given combatant can reach with his movement
//
//	public static GridPath DeserializeGridPath( RoomGrid grid, string serialized )
//	{
//		string[] nodeIDs = serialized.Split(';');
//		List<DijkstraNode> nodes = new List<DijkstraNode>();
//		foreach(string id in nodeIDs)
//		{
//			DijkstraNode node = grid.MGraph.Nodes.Find( x => x.UniqueID.Equals(id) );
//			if(node != null)
//			{
//				nodes.Add(node);
//			}
//			else
//				Debug.Log("WARNING: Could not find node " + id + " on grid path generation");
//		}
//
//		return new GridPath( nodes );
//	}
//
//	public GridPath(  List<DijkstraNode> nodes ) : base(nodes) 
//	{
//		MovementMask = new bool[Nodes.Count];
//		for(int i = 0; i < MovementMask.Length; i++) MovementMask[i] = true;
//	}
//	
//	//	calculates a velocity on the path for given entity
//	//	drag dampens the velocity, slowing is used when approaching the target
//	public Vector3 MoveOnPath ( Vector3 entity, Vector3 velocity, float speed, float drag = DefaultDrag, float slowing = DefaultSlowing )
//	{
//		Vector3 v 	= GetNodeLeveled(CurrentNode, entity.y) - entity;
//		int next 	= CurrentNodeIndex + 1;
//		
//		if(isValid( next ))
//		{
//			//	check if entered new tile - then ease to next
//			Vector3 mag = GetNodeLeveled(NextNode, entity.y) - entity;
//			if(mag.magnitude <= RoomGrid.TileSize*1.5f)
//			{
//				float weighting = 1 - Easing.EaseMovement( EaseType.CubicOUT, mag.magnitude, 0, 1, RoomGrid.TileSize*1.5f );
//				velocity += Vector3.Lerp( v.normalized, mag.normalized, weighting ) * speed * Time.deltaTime;
//				LimitVelocity( ref velocity, speed );
//				velocity *= isApproachingTarget(entity) ? slowing : drag;
//				return velocity;
//			}
//		}
//		
//		//	else move directly to target
//		velocity += v.normalized * speed * Time.deltaTime;
//		LimitVelocity( ref velocity, speed );
//		velocity *= isApproachingTarget(entity) ? slowing : drag;
//		return velocity;
//	}
//	
//	protected override void OnReachNextNode ()
//	{
//
//	}
//
//	//	returns a bool array matching the node array
//	//	indicating if a node point is reachable
//	//	-> used to visualize the grid path 
//	public void SetMovementMask( float movementLeft )
//	{
//		MovementMask = new bool[PathNodes.Count];
//		for(int i = 0; i < MovementMask.Length; i++)
//		{
//			MovementMask[i] = PathNodes[i].AggregatedCost <= movementLeft;
//			if(MovementMask[i] == false)
//				canReachTarget = false;
//		}
//	}
//	
//	public GridNode GetLastReachableNode()
//	{
//		for(int i = MovementMask.Length-1; i >= 0; i--)
//		{
//			if(MovementMask[i])
//				return PathNodes[i] as GridNode;
//		}
//		return null;
//	}
//}
//
//#endregion
//
////-------------------------------------------------------------------------------------------
//
//#region worldpath
//
//public class WorldPath : MovementPath
//{
//	public static WorldPath DeserializeWorldPath( string serialized, Vector3 startPos, Vector3 targetPos )
//	{
//		string[] nodeIDs = serialized.Split(';');
//		List<DijkstraNode> nodes = new List<DijkstraNode>();
//		foreach(string id in nodeIDs)
//		{
//			DijkstraNode node = RoomManager.GetPathNode( id ) as DijkstraNode;
//			if(node != null)
//			{
//				nodes.Add(node);
//			}
//			else
//				Debug.Log("WARNING: Could not find node " + id + " on movement path generation");
//		}
//		
//		return new WorldPath( nodes, startPos, targetPos );
//	}
//
//	public static event intCallback OnEnterNewRoomEvent;
//
//	public WorldPath(  List<DijkstraNode> nodes, Vector3 startP, Vector3 targetPos  ) : base(nodes)
//	{
//		Nodes.Insert(0, startP);
//		Nodes.Add(targetPos);
//	}
//
//	public WorldPath GetClone()
//	{
//		WorldPath p = (WorldPath) this.MemberwiseClone();
//		p.Nodes = new List<Vector3>(Nodes.ToArray());
//		return p;
//	}
//	
//	//-------------------------------------
//
//	public Vector3 MoveOnPath (Vector3 entity, Vector3 velocity, float speed, float drag = DefaultDrag, float slowing = DefaultSlowing)
//	{
//		Vector3 v 	= GetNodeLeveled(CurrentNode, entity.y) - entity;
//		int next 	= CurrentNodeIndex + 1;
//
//		velocity += v.normalized * speed * Time.deltaTime;
//		LimitVelocity( ref velocity, speed );
//		velocity *= isApproachingTarget(entity) ? slowing : drag;
//		return velocity;;
//	}
//	
//	//-------------------------------------
//	
//	protected override void OnReachNextNode ()
//	{
//		if( CurrentNodeIndex < PathNodes.Count
//		   	&& CurrentNodeIndex > 0 )
//		{
//			if(PathNodes[CurrentNodeIndex-1].RoomID != PathNodes[CurrentNodeIndex].RoomID)
//			{
//				if(OnEnterNewRoomEvent != null)
//					OnEnterNewRoomEvent( PathNodes[CurrentNodeIndex].RoomID );
//			}
//		}
//	}
//	
//	//-------------------------------------	
//	
//	//	is called by entity using the path after its creation
//	public void Optimize(Vector3 entity)
//	{
//	//	TrimStart(entity);
//		TrimEnd();
//		CheckIfInCollider();
//	}
////-------------------------------------
//	
//	//	checks wether first node can be ignored
//	void TrimStart(Vector3 entity)
//	{
//		if(Utility.NoObstacleBetween(entity, NextNode))
//		{
//			float distStartToFirst 	= Vector3.Distance(CurrentNode, entity);
//			float distStartToSecond = Vector3.Distance(NextNode, entity);
//			float distFirstToSecond = Vector3.Distance(CurrentNode, NextNode);
//			
//			if(	distStartToSecond < distStartToFirst + distFirstToSecond )
//			{
//				PathNodes.RemoveAt(0);
//				Nodes.RemoveAt(0);
//			}
//		}
//	}
//
//	//-------------------------------------	
//	
//	//	checks if second to last node can be ignored
//	void TrimEnd()
//	{
//		int lastID = Nodes.Count-1;
//		if(Nodes.Count >= 3 && Utility.NoObstacleBetween(Nodes[lastID-2], Nodes[lastID]))
//		{
//			float distTargetToFirst  = Vector3.Distance(Nodes[lastID], Nodes[lastID-1]);
//			float distTargetToSecond = Vector3.Distance(Nodes[lastID], Nodes[lastID-2]);
//			float distFirstToSecond  = Vector3.Distance(Nodes[lastID-1], Nodes[lastID-2]);
//			
//			if( distTargetToSecond < distTargetToFirst + distFirstToSecond)
//			{
//				Nodes.RemoveAt(lastID-1);
//			}
//		}
//	}
//
//	//-------------------------------------
//	
//	//	recalcs target position if its inside a static collider 
//	//	and offsets the target node if necessary
//	void CheckIfInCollider()
//	{
//		if(Nodes.Count >= 3)
//		{
//			int lastID = Nodes.Count-1;
//			Vector3 v = Nodes[lastID-1]-Nodes[lastID];
//			RaycastHit[] hitInfo = Physics.RaycastAll(Nodes[lastID], v.normalized, v.magnitude);
//			foreach(RaycastHit hit in hitInfo)
//			{
//				if( hit.transform.gameObject.layer != GameUtility.LayerMask_DYNAMIC.value )
//				{
//					Nodes[lastID] = hit.point - v.normalized;
//				}
//			}
//		}
//	}
//
//	//-------------------------------------
//
//	//	reset start and end for each character manually
//	public void SetEndingsPos( Vector3 start, Vector3 end )
//	{
//		Nodes.RemoveAt(Nodes.Count-1);
//		Nodes[0] = start;
//		Nodes[Nodes.Count-1] = end;
//	}
//
//	public void ResetMovement()
//	{
//		CurrentNodeIndex = 0;
//	}
//}
//
//#endregion
//
////=================================================================================================================