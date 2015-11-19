using UnityEngine;
using System.Collections;

using Controler_Plug;

public class Player {

	//Information
	public Transform	body;
	public Actions		action;
	public int			id;

	//GameObject componets
	public Animation	myAnimation;
	public BoxCollider			hitBox;
	//Changable
	public GameObject 	buttonDisplay_Y;
	public GameObject	item_Holding;

	//calculators
	// base stats - may be moved
	public readonly float base_movementspeed	= 5;
	public readonly float base_AirSpeed			= 0.5f;

	//constants
	float ItemThrowForce = 400.0f;	//When droping items -> force multiplicator aplied on object

	GameObject grabbedItem = null;
	public Player(Transform body, Actions action, int id)
	{
		this.id		= id;
		this.body	= body;
		this.action	= action;

		myAnimation 	= body.FindChild("playerModel").GetComponent<Animation>();
		hitBox			= body.FindChild("hitBox").GetComponent<BoxCollider>();
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

		if(toPickUp.tag == "Ball")
		{
			GameStatics.dodo.PlaySound_Catch();
		}
	
	}

	public void DropItem()
	{
		Rigidbody rB = item_Holding.GetComponent<Rigidbody>();
		item_Holding.transform.parent = null;
		rB.isKinematic = false;
		rB.AddForceAtPosition(item_Holding.transform.forward * ItemThrowForce, item_Holding.transform.position + item_Holding.transform.up * 0.1f);
		
		item_Holding = null;
	}

	public void GetSlapped (Vector3 contactPoint)
	{
		Vector3 force_direction = (body.position - contactPoint).normalized;

		body.GetComponent<Rigidbody>().AddForce( force_direction* 2500 + Vector3.up * 2000);

		if(item_Holding != null) DropItem();

		//myAnimation.Play("Flap");

		if(Random.value > 0.5f)
		{
			AudioClip[] c = Resources.LoadAll<AudioClip>("Sounds/Player/Punsh");
			body.GetComponent<AudioSource>().PlayOneShot(c[Random.Range(0, c.Length - 1)]);
		}

		Debug.Log("Gotslapped: " + body.name);
	}
}
