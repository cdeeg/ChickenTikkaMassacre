using UnityEngine;
using System.Collections;
using Input_Plug;

public abstract class PlayerState  {

	public abstract PlayerState Update (Player plr, bool DEBUG = false);

	public abstract stateChange Move(Transform plrBody, Analog_Stick move);

	public enum stateChange
	{
		NoChange,
		to_OnLadder,
		to_Normal,
		to_InAiming,
		to_Ko,
		to_Drowning,
		to_CarryItem
	}

	public bool DEBUG = false;

	// Update is called once per frame

	//public abstract void GetSlapped();
	public PlayerState ChangeStateTo(stateChange command, Player plr)
	{

		if(plr.buttonDisplay_Y)
		{
			plr.buttonDisplay_Y.SetActive(false);
		}

		PlayerState toReturn = new Normal();

		switch( command )
		{
		case PlayerState.stateChange.to_OnLadder:
			
			Rigidbody r = plr.body.GetComponent<Rigidbody>();
			r.useGravity = false;
			r.velocity = Vector3.zero;
			plrUtility.Movement.PlacePlayerOnLadder(plrUtility.bello.WhereIsThe_(Bello.ObjectLayer.Ladder, plr.body), plr);
			toReturn = new OnLadder();
			break;
			
		case PlayerState.stateChange.to_Normal:
			
			plr.body.GetComponent<Rigidbody>().useGravity = true;
			toReturn = new Normal();
			break;

		case PlayerState.stateChange.to_Drowning:

			if(plr.item_Holding != null) plr.DropItem();
			plr.body.GetComponent<Rigidbody>().useGravity = false;
			toReturn = new Drowning(plr.body.position);
			break;
		
		case PlayerState.stateChange.to_CarryItem:
			
			plr.body.GetComponent<Rigidbody>().useGravity = true;
			toReturn = new CarryItem();
			break;

			
		default:
			toReturn = new Normal();
			break;
		}

		return toReturn;
	}

	public stateChange CheckForStateChangers(Player plr)
	{
		if(plrUtility.bello.WhereIsThe_(Bello.ObjectLayer.Water, plr.body))	return stateChange.to_Drowning;

		if(plr.health == 0) return stateChange.to_Ko;

		return stateChange.NoChange;
	}

	public void RegenerateHealth (Player plr)
	{
		//plr.health += 1 * Time.deltaTime;
	}

}
