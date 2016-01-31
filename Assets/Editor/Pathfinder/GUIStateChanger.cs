using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//=================================================================================================================

public class GUIStateChanger
{
	private enum actionType { _void, _parameter }
	
	private struct stateAction
	{
		private object _action;
		private object _param;
		private int		_hash;
		
		
		public actionType 	Type;
		public string 		Message;
		
		public stateAction(System.Action action, string msg="") 
		{
			this._action = action as object;
			this._param	 = null;
			this.Message = msg;
			this.Type 	 = actionType._void;
			this._hash	= action.GetHashCode();
		}
		public stateAction(System.Action<object> action, object state, string msg="")
		{
			this._action = action as object;
			this._param  = state;
			this.Message = msg;
			this.Type	 = actionType._parameter;
			this._hash	 = action.GetHashCode() + (state != null ? state.GetHashCode() : 0);
		}
		public void fireAction()
		{
			switch(Type)
			{
			case actionType._void:
				
				System.Action a = _action as System.Action;
				if(a != null)
				{
					a();
				}
				break;
				
			case actionType._parameter:
				
				System.Action<object> pa = _action as System.Action<object>;
				if(pa != null)
				{
					pa(_param);
				}
				break;
			}
		}
		
		public override int GetHashCode ()
		{
			return _hash;
		}
	}
	
	private List<stateAction> queue;
	
	
	public GUIStateChanger()
	{
		queue = new List<stateAction>();
	}
	
	public void Add(System.Action<object> action, object state, string message="")
	{
		if(action != null)
		{
			int hash = action.GetHashCode() + (state != null ? state.GetHashCode() : 0);
			if(!queue.Exists(x=>x.GetHashCode()==hash))
			{
				queue.Add(new stateAction(action, state, message));
			}
		}
	}
	public void Add(System.Action action, string message="")
	{
		if(action != null)
		{
			int hash = action.GetHashCode();
			if(!queue.Exists(x=>x.GetHashCode()==hash))
			{
				queue.Add(new stateAction(action, message));
			}
		}
	}
	
	/// <summary>
	/// Applies all state changes and returns wether a state was changed.
	/// </summary>
	public bool Apply()
	{
		if(Event.current.type == EventType.layout)
		{
			bool changed = queue.Count > 0;
			for(int i = 0; i < queue.Count; i++)
			{
				queue[i].fireAction();
			}
			queue.Clear();
			return changed;
		}
		return false;
	}
}

//=================================================================================================================



