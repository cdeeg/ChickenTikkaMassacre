using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum EEventType
{
	PLAYER_GET_HIT,
	PLAYER_DIED,
	PLAYER_USE_WEAPON,
	PLAYER_SCORED,
	DODO_ESCAPED,
	DODO_GRABBED,
}

public class GlobalEventHandler
{
	private class EventObject
	{
		public EEventType MyEvent;
		public ListenerDelegate ThisDelegate;

		public EventObject() {}
	}

	List<EventObject> listenerObjects;

	public delegate void ListenerDelegate( object sender, EventArgs args );

	static GlobalEventHandler instance;

	public static GlobalEventHandler GetInstance()
	{
		if( instance == null )
			instance = new GlobalEventHandler();

		return instance;
	}

	private GlobalEventHandler()
	{
		listenerObjects = new List<EventObject>();
	}

	public void ClearAllListeners()
	{
		listenerObjects.Clear();
	}

	public void RegisterListener( EEventType eventType, ListenerDelegate delg )
	{
		EventObject obj = new EventObject();
		obj.MyEvent = eventType;
		obj.ThisDelegate = delg;
		listenerObjects.Add( obj );
	}

	public void UnregisterListener( EEventType eventType, ListenerDelegate delg )
	{
		EventObject obj;
		for( int x=0; x<listenerObjects.Count; ++x )
		{
			obj = listenerObjects[x];
			if( eventType == obj.MyEvent  )
			{
				if( delg.GetHashCode() == obj.ThisDelegate.GetHashCode() )
				{
					listenerObjects.RemoveAt( x );
					break;
				}
			}
		}
	}

	public void ThrowEvent(object sender, EEventType eventType, EventArgs args)
	{
		for( int x=0; x<listenerObjects.Count; ++x )
		{
			EventObject obj = listenerObjects[x];
			if( obj.MyEvent == eventType )
			{
				obj.ThisDelegate( sender, args );
			}
		}
	}
}