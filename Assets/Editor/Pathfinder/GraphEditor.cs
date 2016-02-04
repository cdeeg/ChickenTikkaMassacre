using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JPathfinder;

//=================================================================================================================

[CustomEditor(typeof(GraphData))]
public class GraphEditor :  Editor
{
	#region fields

	const float yOff = 0.01f;
	static Vector3 NULLPOS = Vector3.one * -100;

	//	data obj
	GraphData		 data;
	SerializedObject dataObj;

	//	controller fields
	EditorNode 		 	nodeOnHover;
	EditorNode 		 	nodeOnHandle;
	EditorConnection 	connectionOnHover;
	Vector3 		 	currGroundP;

	GUIStateChanger stateChanger
	{
		get {
			if(_stateChangerBacking == null)
				_stateChangerBacking = new GUIStateChanger();
			return _stateChangerBacking;
		}
	}
	GUIStateChanger _stateChangerBacking;

//	//	tmp nodes & edges
//	IEnumerable<GameObject> nodes 
//	{
//		get {
//			foreach(Transform t in nodeContainer)
//				yield return t.gameObject;
//		}
//	}

	List<EditorNode> nodes = new List<EditorNode>();
	List<EditorConnection> edges = new List<EditorConnection>();

	int getFreeNodeID()
	{
		var n = nodes.OrderByDescending(x=> x.ID).FirstOrDefault();
		return n != null ? n.ID + 1 : 0; 
	}

	bool isDirty = false;

	//	util window
	const int _utilWindowWith = 280;
	const int _utilWindowHeight = 130;
	const int _utilWindowMargin = 12;
	
	Rect utilWindowRect
	{
		get {
			return new Rect(sceneWindow.width - _utilWindowWith - _utilWindowMargin,
			                sceneWindow.height - _utilWindowHeight - _utilWindowMargin,
			                _utilWindowWith, _utilWindowHeight);
		}
	}

	//	container to place all temporary nodes into
	Transform nodeContainer
	{
		get {
			if(_nodeContainerBacking == null)
			{
				GameObject go = GameObject.Find(GraphData.nodeContainerName);
				_nodeContainerBacking = go != null 
									  ? go.transform 
									  : new GameObject(GraphData.nodeContainerName).transform;
			}
			return _nodeContainerBacking;
		}
	}
	Transform _nodeContainerBacking;

	//	node visualization
	GameObject nodePrefab
	{
		get { 
			if(_nodePrefabBacking == null)
				_nodePrefabBacking = EditorGUIUtility.Load("Pathfinder/Pathnode.prefab") as GameObject;
			return _nodePrefabBacking;
		}
	}
	GameObject _nodePrefabBacking;

	//	editor sceneview camera
	static Camera sceneCam
	{
		get {
			return 	SceneView.lastActiveSceneView != null 
					? SceneView.lastActiveSceneView.camera	
					:	null;
		}
	}

	//	editor sceneview dimensions
	static Rect sceneWindow
	{
		get {
			return sceneCam != null ? SceneView.lastActiveSceneView.position : new Rect();
		}
	}

	bool hasLoadedGraph 
	{
		get {
			return nodeContainer.childCount > 0;
		}
	}


	#endregion

	//=================================================================================================================

	#region init

	static bool isEnabled = false;

	void OnEnable()
	{
		if(!isEnabled)
		{
			dataObj = new SerializedObject(target);
			data = (GraphData) target;

			hookupSceneGUI();
			EditorNode.SetObjHandler(nodeHandler);

			if(!hasLoadedGraph)
				ReadFromData();

			isEnabled = true;
		}
	}

	void OnDisable()
	{
		if(isEnabled)
		{
			SetNodeOnHover(null);
			unhookSceneGUI();

			ClearNodes();

			isEnabled = false;
		}
	}

	#endregion

	//=================================================================================================================

	#region scene controls

	void hookupSceneGUI()
	{
		SceneView.onSceneGUIDelegate += OnSceneGUI;
		EditorApplication.update += Update;
	}
	void unhookSceneGUI()
	{
		SceneView.onSceneGUIDelegate -= OnSceneGUI;
		EditorApplication.update += Update;
	}

	//---------------------------------------------------------------------------------------------------------------

	void Update()
	{
		if(data != null)
		{
			SceneView.RepaintAll();
		}
	}

	void OnSceneGUI(SceneView view)
	{
		if(data == null)
		{
			return;
		}

		stateChanger.Apply();
		dataObj.Update();

		Event e = Event.current;

		bool hoverUtilWindow = utilWindowRect.Contains(e.mousePosition);

		if(!e.alt && e.button != 2)
		{
			int controlID = GUIUtility.GetControlID(GetHashCode(), FocusType.Passive);

			UpdateControls();

			switch(e.type)
			{
			case EventType.mouseDown:

				if(!hoverUtilWindow)
				{
					onMouseDown(e);
					Repaint();
				}
				break;

			case EventType.mouseDrag:

				if(!hoverUtilWindow)
				{
					onMouseDrag(e);
					Repaint();
				}
				break;

			case EventType.mouseUp:

				if(!hoverUtilWindow)
				{
					onMouseUp(e);
					Repaint();
				}
				break;

			case EventType.layout:

				HandleUtility.AddDefaultControl(controlID);
				UpdateNodes();
				break;
			}
		}


		//	handle drawing
		DrawSceneHandles();

		//	GUI drawing
		DrawSceneGUI();

		Repaint();

		dataObj.ApplyModifiedProperties();
	}

	//---------------------------------------------------------------------------------------------------------------

	void UpdateControls()
	{
		if(sceneCam == null)
		{
			return;
		}

		//	get groundP
		RaycastHit hit;
		Vector3 p = new Vector3( Event.current.mousePosition.x, sceneWindow.height-Event.current.mousePosition.y, 0);
		if(Physics.Raycast(sceneCam.ScreenPointToRay(p), out hit))
		{
			if(hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
			{
				currGroundP = hit.point + Vector3.up * yOff;
			}
			else
				currGroundP = NULLPOS;
		}
		else
		{
			currGroundP = NULLPOS;
		}

		//	get hover
		connectionOnHover = null;
		if(hit.transform != null && nodes.Exists(x=> x.obj == hit.transform.gameObject))
		{
			EditorNode node = GetNodeByGO(hit.transform.gameObject);
			SetNodeOnHover(node);
			currGroundP = node.position;
		}
		else
		{
			SetNodeOnHover(null);
			foreach(EditorConnection c in edges)
			{
				if(c.forceConnection || c.magnitude <= data.autoConnectionDist)
				{
					if(c.CheckHover())
					{
						connectionOnHover = c;
						break;
					}
				}
			}
		}
	}

	void SetNodeOnHover(EditorNode node)
	{
		if(node != nodeOnHover)
		{
			nodeOnHover = node;
		}
	}


	//---------------------------------------------------------------------------------------------------------------

	bool connectMode = false;

	void onMouseDown(Event e)
	{
		if(e.button == 0)
		{
			if(e.shift)
			{
				if(connectionOnHover != null)
				{
					connectionOnHover.NextMode();
					isDirty = true;
				}
			}
			else if(e.control)
			{
				connectMode = true;
			}
			else if(nodeOnHover == null)
			{
				connectionOnHover = null;
				stateChanger.Add(()=> {
					nodeOnHover = CreateNode(currGroundP);
				});
				e.Use();
			}
		}

		if(nodeOnHover != null)
		{
			nodeOnHandle = nodeOnHover;
		}
	}

	void onMouseUp(Event e)
	{
		if(e.button == 0)
		{
			if(e.control && nodeOnHover != null)
			{
				if(nodeOnHandle != null)
				{
					CreateConnection(nodeOnHover, nodeOnHandle, connectDir.both, true);
				}
			}
			else if(e.shift)
			{
				EditorNode toDelete = nodeOnHover;
				SetNodeOnHover(null);
				stateChanger.Add(()=> {
					if(toDelete != null)
					{
						DeleteNodeConnections(toDelete.ID);
						DeleteNode(toDelete);
					}
				});

			}
			else if(nodeOnHandle != null)
			{
				stateChanger.Add(()=> {
					AutoConnect(nodeOnHandle);
				});
			}
		}

		connectMode = false;
		nodeOnHandle = null;
	}

	void onMouseDrag(Event e)
	{
		if(e.button == 0)
		{
			if(!connectMode)
			{
				if(nodeOnHandle != null && currGroundP != NULLPOS)
				{
					stateChanger.Add(()=> {
						nodeOnHandle.MoveTo(currGroundP);
						AutoConnect(nodeOnHandle);
						isDirty = true;
					});
				}
			}
		}
	}

	#endregion

	//=================================================================================================================

	#region scene drawing

	void DrawSceneHandles()
	{
		foreach(EditorConnection c in edges)
		{
			if(!c.supressConnection && (c.forceConnection || c.magnitude <= data.autoConnectionDist))
			{
				c.DrawHandle(c == connectionOnHover);
			}
		}
		
		if(nodeOnHover != null)
		{
			Handles.color = Color.yellow;
			Handles.DrawWireDisc(nodeOnHover.position, -sceneCam.transform.forward, 0.55f);
			if(Event.current.shift)
			{
				Handles.color = new Color(1,0,0,0.5f);
				Handles.DrawSolidDisc(nodeOnHover.position, -sceneCam.transform.forward, 0.52f);
			}
			Handles.color = Color.white;
		}
		if(nodeOnHandle != null)
		{
			Handles.color = new Color(0,0.5f,1f, 0.6f);
			Handles.DrawSolidDisc(nodeOnHandle.position, -sceneCam.transform.forward, 0.48f);

			if(Event.current.control && currGroundP != NULLPOS)
			{
				Handles.color = Color.blue;
				Handles.DrawLine(nodeOnHandle.position, currGroundP);
			}
			Handles.color = Color.white;
		}
	}
	
	void DrawSceneGUI()
	{
		Rect window = utilWindowRect;
		Handles.BeginGUI(window);
		
		GUI.Box(new Rect(0,0,window.width,window.height), "");
		Rect r = new Rect(6,6,window.width-12,20);
		EditorGUI.PropertyField(r, dataObj.FindProperty("autoConnectionDist"), 
		                        new GUIContent("Autoconnection Dist"));
		

		r.y += 26; Rect r2 = new Rect(r); r2.y += 26; r.width = r.width/2 - 2; 
		if(GUI.Button(r, "Save"))
		{
			stateChanger.Add(WriteToData);
			Event.current.Use();
		}
		r.x += r.width + 4;
		if(GUI.Button(r, "Clear"))
		{
//			stateChanger.Add(()=> {
//				if(target != null)
//				{
//					dataObj.ApplyModifiedProperties();
					ClearNodes();
//					data.nodes.Clear();
//					data.edges.Clear();
//					dataObj.Update();
//				}
//			});
			Event.current.Use();
		}

		if(GUI.Button(r2, "Clear Editor Data"))
		{
			//			stateChanger.Add(()=> {
			if(target != null)
			{
				dataObj.ApplyModifiedProperties();
				ClearNodes();
				data.nodes.Clear();
				data.edges.Clear();
				dataObj.Update();
			}
//			});
			Event.current.Use();
		}

		GUILayout.EndArea();
		
		Repaint();
		Handles.EndGUI();
	}

	#endregion

	//=================================================================================================================

	#region data handling

	EditorNode GetNodeByGO(GameObject go)
	{
		return go != null ? nodes.Find(x=> x.obj == go) : null;
	}

	EditorNode CreateNode(Vector3 p, bool autoconnect=true)
	{
		if(p != NULLPOS)
		{
			EditorNode node = new EditorNode(getFreeNodeID(), p);
			nodes.Add(node);
			if(autoconnect)
				AutoConnect(node);
			isDirty = true;
			return node;
		}
		return null;
	}
	
	GameObject nodeHandler()
	{
		GameObject go = GameObject.Instantiate(nodePrefab) as GameObject;
		go.transform.SetParent(nodeContainer, true);
		return go;
	}

	bool areConnected(EditorNode nodeA, EditorNode nodeB)
	{
		return edges.Exists( x=> x.containsNodes(nodeA, nodeB) );
	}

	EditorConnection CreateConnection(EditorNode a, EditorNode b, connectDir dir, bool forced)
	{
		if(a != b && a != null && b != null)
		{
			if(areConnected(a, b))
			{
				EditorConnection c = edges.Find(x=> x.containsNodes(a, b));

				if(!c.forceConnection)
				{
					c.supressConnection = c.magnitude <= data.autoConnectionDist;
				}
				else 
					c.supressConnection = !c.supressConnection;
				c.forceConnection = true;
				isDirty = true;
				return c;
			}
			else
			{
				EditorConnection c = new EditorConnection(a, b, dir, forced);
				c.supressConnection = false;
				edges.Add(c);
				isDirty = true;
				return c;
			}
		}
		return null;
	}
	EditorConnection CreateConnection(EditorNode a, EditorNode b, graphConnection gc)
	{
		EditorConnection c = CreateConnection(a, b, gc.dir, gc.forced);
		if(c != null)
			c.supressConnection = gc.supressed;
		return c;
	}


	void DeleteNode(EditorNode node)
	{
		if(node != null)
		{
			node.DestroyObj();
			nodes.Remove(node);
			isDirty = true;
		}
	}

	void DeleteConnection(EditorConnection c)
	{
		if(c != null)
		{
			edges.Remove(c);
			isDirty = true;
		}
	}
	void DeleteConnection(EditorNode a, EditorNode b)
	{
		EditorConnection c = edges.Find(x=> x.containsNodes(a, b));
		if(c != null)
		{
			edges.Remove(c);
			isDirty = true;
		}
	}
	void ToggleSupressConnection(EditorNode a, EditorNode b)
	{
		EditorConnection c = edges.Find(x=> x.containsNodes(a, b));
		if(c != null)
		{
			c.supressConnection = !c.supressConnection;
		}
	}
	void DeleteNodeConnections(int nodeID)
	{
		edges.RemoveAll(x=> x.containsNode(nodeID));
		Debug.Log("deleted connections:: " + nodeID);
		PrintConnections();
	}

	void ClearNodes()
	{
		while(nodes.Count > 0)
		{
			DeleteNode(nodes[0]);
		}
		edges.Clear();
	}

	void UpdateNodes()
	{
		if(sceneCam != null)
		{
			foreach(EditorNode n in nodes)
			{
				n.FaceCam(sceneCam);
			}
		}
	}

	//---------------------------------------------------------------------------------------------------------------

	/// <summary>
	/// autoconnect a newly placed node to all nodes around it
	/// </summary>
	void AutoConnect(EditorNode node)
	{
		if(node != null)
		{
			foreach(EditorNode n in nodes)
			{
				if(n != node && n != null && !areConnected(n, node))
				{
					float mag = Vector3.Distance(n.position, node.position);
					if(mag <= data.autoConnectionDist)
					{
						CreateConnection(n, node, connectDir.both, false);
					}
				}
			}
		}
	}

	#endregion

	//=================================================================================================================

	#region inspector GUI

	void OnInspectorGUI()
	{
		base.OnInspectorGUI();
	}

	#endregion

	//=================================================================================================================

	#region I/O

	void WriteToData()
	{
		Debug.Log("try write... " + (data != null) + " / " + isDirty);
		if(data != null && isDirty)
		{
			int counter=0;
			data.nodes = nodes.Select((EditorNode n) => {
				var node = new graphNode();
				node.ID  = counter++;
				node.position = n.position;
				return node;
			}).ToList();
			data.edges = edges.Select((EditorConnection c)=> {
				var gc = new graphConnection();
				gc.A   = c.nodeA.ID;
				gc.B   = c.nodeB.ID;
				gc.dir = c.dir;
				gc.forced = c.forceConnection;
				gc.supressed = c.supressConnection;
				return gc;
			}).ToList();

			dataObj.Update();

			EditorUtility.SetDirty(data);
			AssetDatabase.SaveAssets();

			isDirty = false;
		}
	}

	void ReadFromData()
	{
		if(data != null && data.hasContent())
		{
			ClearNodes();
			foreach(graphNode node in data.nodes)
			{
				CreateNode(node.position, autoconnect: false);
			}

			for(int i = 0; i < data.edges.Count; i++)
			{
				int idA = data.edges[i].A;
				int idB = data.edges[i].B;
				EditorNode a = nodes.Find(x=> x.ID==idA);
				EditorNode b = nodes.Find(x=> x.ID==idB);

				if(a != null && b != null)
				{
					CreateConnection(a, b, data.edges[i]);
				}
				else throw new System.Exception("could not read connection between nodes " + idA + " and " + idB);
			}
			dataObj.Update();

			isDirty = false;
		}
	}

	#endregion

	//=================================================================================================================

	#region util

	void PrintConnections()
	{
		var b = new System.Text.StringBuilder();
		b.Append("Edges: " + edges.Count);
		for(int i = 0; i < edges.Count; i++)
		{
			b.Append("\n\t" + i.ToString() + ": " + (edges[i].nodeA != null ? edges[i].nodeA.ID.ToString() : "null")
			         + " | " + (edges[i].nodeB != null ? edges[i].nodeB.ID.ToString() : "null"));
		}
		Debug.Log(b.ToString());
	}

	#endregion

}

//=================================================================================================================