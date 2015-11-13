using UnityEngine;
using System.Collections;

using Controler_Plug;

public class Player {

	//Information
	public Transform	body;
	public Actions		action;
	public int			id;

	//Changable
	public GameObject 	buttonDisplay_Y;
	public GameObject	item_Holding;

	//calculators
	// base stats - may be moved
	public readonly float base_movementspeed	= 5;
	public readonly float base_AirSpeed			= 0.5f;
	
	GameObject grabbedItem = null;
	public Player(Transform body, Actions action, int id)
	{
		this.id		= id;
		this.body	= body;
		this.action	= action;
	}

	public void PickUpItem(GameObject toPickUp)
	{

		item_Holding = toPickUp;
		
		item_Holding.transform.position = body.position + Vector3.up * 1.25f;
		item_Holding.transform.rotation = body.rotation;
		item_Holding.transform.parent	= body;
		//plr.item_Holding.layer = 1 << 21;

		Rigidbody rB = item_Holding.GetComponent<Rigidbody>();
		rB.isKinematic = true;
		
		GameObject.Destroy(buttonDisplay_Y);	//destroy prev. object holder (button)
	
	}
}
