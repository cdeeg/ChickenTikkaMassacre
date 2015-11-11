using UnityEngine;
using System.Collections;

using Controler_Plug;

public class Player {

	public Transform	body;
	public Actions		action;

	public GameObject 	buttonDisplay_Y;
	public GameObject	item_Holding;
	//calculators
	public readonly float base_movementspeed	= 5;
	public readonly float base_AirSpeed			= 0.5f;
	
	GameObject grabbedItem = null;
	public Player(Transform body, Actions action)
	{
		this.body	= body;
		this.action	= action;
	}
}
