using UnityEngine;
using System.Collections;

using Controler_Plug;

public class Player {

	//Information
	public Transform	body;
	public Actions		action;
	public int			id;
	public static int 	MaxHealth = 5;
	public int			health = MaxHealth;

	//GameObject componets
	public Animation	myAnimation;
	public BoxCollider			hitBox;
	//Changable
	public GameObject 	buttonDisplay_Y;
	public GameObject	item_Holding;

	//calculators
	// base stats - may be moved
	public readonly float base_movementspeed	= 5.0f;
	public readonly float base_AirSpeed			= 0.5f;
	public readonly float base_CarrySpeed		= 3.0f;
	//constants
	float ItemThrowForce = 250.0f;	//When droping items -> force multiplicator aplied on object

	GameObject grabbedItem = null;
	public Player(Transform body, Actions action, int id)
	{
		this.id		= id;
		this.body	= body;
		this.action	= action;

		myAnimation 	= body.FindChild("playerModel").GetComponent<Animation>();
		hitBox			= body.FindChild("hitBox").GetComponent<BoxCollider>();

		buttonDisplay_Y = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		buttonDisplay_Y.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
		buttonDisplay_Y.SetActive(false);
	}

	public void PickUpItem(GameObject toPickUp)
	{

		item_Holding = toPickUp;
		
		item_Holding.transform.position = body.position + Vector3.up * 1.25f;
		item_Holding.transform.rotation = body.rotation;
		item_Holding.transform.parent	= body;
		//plr.item_Holding.layer = 1 << 21;
		Rigidbody rB;
		if(item_Holding.GetComponent<Rigidbody>())	rB = item_Holding.GetComponent<Rigidbody>();
		else 										rB = item_Holding.transform.GetChild(0).GetComponent<Rigidbody>();
		rB.isKinematic = true;

		if(toPickUp.tag == "Ball")
		{
			GameCommand.Dodo.PickUpDodo();
		}
	
	}

	public void ThroWItem()
	{
		if(item_Holding == null) return;

		Rigidbody rB = item_Holding.GetComponent<Rigidbody>();
		item_Holding.transform.parent = null;
		rB.isKinematic = false;
		rB.AddForceAtPosition(item_Holding.transform.forward * ItemThrowForce, item_Holding.transform.position + item_Holding.transform.up * 0.1f);
		
		item_Holding = null;
	}

	public void DropItem()
	{
		if(item_Holding == null) return;

		Rigidbody rB = item_Holding.GetComponent<Rigidbody>();
		item_Holding.transform.parent = null;
		rB.isKinematic = false;
		rB.AddForceAtPosition(Vector3.up * ItemThrowForce, item_Holding.transform.position + item_Holding.transform.up * 0.1f);
		
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

		health -= 1;

		if(health == 0) GameCommand.PlayerKnockOut(id);
	}
}
