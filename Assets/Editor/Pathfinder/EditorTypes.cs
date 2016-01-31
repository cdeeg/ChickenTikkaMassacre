using UnityEngine;
using UnityEditor;
using System.Collections;

//=================================================================================================================

namespace JPathfinder
{
	public class EditorNode
	{
		static System.Func<GameObject> objHandler;
		public static void SetObjHandler(System.Func<GameObject> handler) 
		{
			objHandler = handler;
		}

		public int ID;
		private Vector3 positionBackup;

		public EditorNode(int id, Vector3 p)
		{
			this.ID = id;
			this.positionBackup = p;
			createObj();
		}

		public GameObject obj
		{
			get { 
				if(_objBacking == null)
				{
					createObj();
				}
				return _objBacking;
			}
		}
		private GameObject _objBacking;

		void createObj()
		{
			if(objHandler != null)
			{
				_objBacking = objHandler();
				_objBacking.transform.position = positionBackup;
				_objBacking.name = "node " + ID.ToString();
			}
			else
				throw new System.Exception("set objHandler first before creating EditorNodes!");
		}

		public void DestroyObj()
		{
			if(_objBacking != null)
			{
				GameObject.DestroyImmediate(_objBacking);
			}
		}

		public Vector3 position 
		{
			get { return obj.transform.position; }
		}

		public void MoveTo(Vector3 pos)
		{
			obj.transform.position = pos;
			positionBackup = pos;
		}
		public void FaceCam(Camera c)
		{
			obj.transform.up = -c.transform.forward;
		}

		public graphNode toGraphNode()
		{
			var gn = new graphNode();
			gn.position = obj.transform.position;
			gn.ID = ID;
			return gn;
		}
	}

	//=================================================================================================================

	public class EditorConnection 
	{
		const int _snapDist = 10;
		const float _selectionFrameWidth = 0.05f;
		const float _arrowCapSize = 0.5f;

		public readonly EditorNode nodeA;
		public readonly EditorNode nodeB;

		public connectDir dir;

		/// <summary>
		/// if true, this connection will not be disabled when distance between nodes is too high
		/// </summary>
		public bool forceConnection = false;
		/// <summary>
		/// if true, this connection will not be in the final graph
		/// </summary>
		public bool supressConnection = false;

		public float magnitude { get { return isValid() ? Vector3.Distance(pA, pB) : 0; } }

		private Vector3 pA { get { return nodeA != null ? nodeA.position : Vector3.one*-100; } }
		private Vector3 pB { get { return nodeB != null ? nodeB.position : Vector3.one*-100; } }
		private Vector3 pA2 { get { return pA + (pB-pA).normalized * 0.2f; } }
		private Vector3 pB2 { get { return pB + (pA-pB).normalized * 0.2f; } }

		//---------------------------------------------------------------------------------------------------------------

		public EditorConnection(EditorNode a, EditorNode b, connectDir dir=connectDir.both, bool force=false)
		{
			this.nodeA = a;
			this.nodeB = b;
			this.dir = dir;
			this.forceConnection = force;
		}

		public bool isValid()
		{
			return nodeA != null && nodeB != null;
		}

		/// <summary>
		/// cycle through connection mode
		/// </summary>
		public void NextMode()
		{
			dir = (connectDir) ((((int)dir) + 1) % System.Enum.GetNames(typeof(connectDir)).Length);
			forceConnection = true;
		}

		//---------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// is this connection currently hovered by user?
		/// </summary>
		public bool CheckHover()
		{
			return isValid() && !supressConnection && HandleUtility.DistanceToLine(pA2, pB2) <= _snapDist;
		}

		public bool containsNode(int node)
		{
			return node != null && (nodeA.ID == node || nodeB.ID == node);
		}
		public bool containsNodes(int idA, int idB)
		{
			return ((nodeA.ID == idA && nodeB.ID == idB) || (nodeA.ID == idB && nodeB.ID == idA));
		}
		public bool containsNode(EditorNode node)
		{
			return node != null && (nodeA == node || nodeB == node);
		}
		public bool containsNodes(EditorNode a, EditorNode b)
		{
			return a != null && b != null && ((nodeA == a && nodeB == b) || (nodeA == b && nodeB == a));
		}

		//---------------------------------------------------------------------------------------------------------------

		public void DrawHandle(bool isHovered)
		{
			if(isValid())
			{
				Vector3 v = pB - pA;
				if(isHovered)
				{
					Handles.color = new Color(1,1,1,0.75f);
					Vector3 vv = Vector3.Cross(v.normalized, Vector3.up).normalized * _selectionFrameWidth;
					Vector3 p1 = pA - vv, p2 = pA + vv, p3 = pB + vv, p4 = pB - vv;
					Handles.DrawLine(p1, p2);
					Handles.DrawLine(p2, p3);
					Handles.DrawLine(p3, p4);
					Handles.DrawLine(p4, p1);
				}

				Color c = forceConnection ? Color.magenta : Color.green;
				Handles.color = isHovered ? Color.yellow : c;


				switch(dir)
				{
				case connectDir.both: 

				//	Handles.ArrowCap(GetHashCode(), pB - v.normalized*0.4f, Quaternion.LookRotation(v.normalized), _arrowCapSize);
				//	Handles.ArrowCap(GetHashCode(), pA + v.normalized*0.4f, Quaternion.LookRotation(-v.normalized), _arrowCapSize);
					Handles.DrawLine(pA + v.normalized*0.1f, pB - v.normalized*0.1f);
					break;
				
				case connectDir.AB:

					Handles.ArrowCap(GetHashCode(), pB - v.normalized*0.7f, Quaternion.LookRotation(v.normalized), _arrowCapSize);
					Handles.DrawLine(pA2, pB2);
					break;
				case connectDir.BA:
					
					Handles.ArrowCap(GetHashCode(), pA + v.normalized*0.7f, Quaternion.LookRotation(-v.normalized), _arrowCapSize);
					Handles.DrawLine(pA2, pB2);
					break;
				}


				Handles.color = Color.white;
			}
		}

	}

}

//=================================================================================================================