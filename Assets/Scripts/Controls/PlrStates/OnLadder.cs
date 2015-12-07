using UnityEngine;
using System.Collections;

public class OnLadder : PlayerState {

	// Use this for initialization

	Vector3 ladderUp;

	public override PlayerState Update(Player plr, bool DEBUG = false) 
	{

		//if(plr.action.Grab_Btn.just_pressed) ChangeStateTo(stateChange.to_Normal, plr);

		if (plr.action.Jump_Btn.just_pressed)
		{
			plrUtility.Movement.Perform_Jump(plr);
			return NormalOrCarryItem(plr);
		}

		Collider n_ladder = plrUtility.bello.WhereIsThe_(Bello.ObjectLayer.Ladder, plr.body);

		if(n_ladder == null)
		{
			//Top End of ladder reached
			plr.body.position += plr.body.forward * 0.45f;
			return NormalOrCarryItem(plr);
		}
		ladderUp = n_ladder.transform.up;

		stateChange command = Move(plr.body, plr.action.Move);
		if(command != stateChange.NoChange)
		{
			if(command == stateChange.to_Normal) return NormalOrCarryItem(plr);
			return ChangeStateTo(command, plr);
		}
		plrUtility.Movement.PlacePlayerOnLadder(n_ladder, plr);

		return this;
	}

	override public stateChange Move(Transform plrBody, Input_Plug.Analog_Stick move)
	{
		if(move.nVector.y != 0)
		{
			if(move.nVector.y < 0)
			{
			
				if( plrUtility.bello.WhereIsThe_(Bello.ObjectLayer.Ground, plrBody))
				{
					return stateChange.to_Normal;//SwitchToNormal();
				}
				else
				{
					plrBody.position -= ladderUp * Time.deltaTime * 2.0f;
				}
			}
			else
			{
				plrBody.position += ladderUp * Time.deltaTime * 2.0f;
			}
		}
		else {};


		return stateChange.NoChange;		
	}

	PlayerState NormalOrCarryItem(Player plr)
	{
		if(plr.item_Holding == null) return ChangeStateTo(stateChange.to_Normal, plr);
		return ChangeStateTo(stateChange.to_CarryItem, plr);
	}
}
