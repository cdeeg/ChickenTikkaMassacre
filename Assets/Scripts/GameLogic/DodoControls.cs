using UnityEngine;
using System.Collections;

public class DodoControls {

	public DodoControls()
	{
		CreateDodo();
	}

	//initialisation

	void CreateDodo()
	{
		GameObject d = GameObject.Instantiate( Resources.Load<GameObject>("Characters/Dodo"), GameStatics.dodo_spawn, Quaternion.identity) as GameObject;
		GameStatics.dodo = d.transform.GetChild(0).GetComponent<BallLogic>();
	}

	//

	//Interface

	public void DropDodo(string dodoHolder)
	{
		Player[] Players = GameCommand.gameInstance.GetPlayers();
		Player plr = Players[0];
		if(dodoHolder == "")
		{
			
			plr = Players[0];
		}
		else if(dodoHolder == "")
		{
			plr = Players[1];
		}
		
		plr.item_Holding.layer = 1 << 20;
		Rigidbody rB = plr.item_Holding.GetComponent<Rigidbody>();
		plr.item_Holding.transform.parent = null;
		rB.isKinematic = false;
		
		Vector3 escape_dir = plr.item_Holding.transform.forward * Random.Range(-1.0f, 1) * 500.0f;
		escape_dir += plr.item_Holding.transform.right * Random.Range(-1.0f, 1) * 500.0f;
		escape_dir += Vector3.up * Random.Range( 250, 350);
		
		rB.AddForceAtPosition(escape_dir, plr.item_Holding.transform.position + plr.item_Holding.transform.up * 0.1f);
		
		plr.item_Holding = null;
		
	}
}
