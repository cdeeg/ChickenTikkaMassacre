using UnityEngine;
using System.Collections;

public class Normal : PlayerState
{
	public override PlayerState Update(Player plr, bool DEBUG = false) 
	{
		stateChange s = CheckForStateChangers(plr);
		if(s  != stateChange.NoChange)
		{
			 return ChangeStateTo(s, plr);
		}

		//Move Around
		{
			Move(plr.body, plr.action.Move);
		}//
		//Object Interaction
		{
			//If player is holding an item
			if( plr.item_Holding != null )
			{
				return ChangeStateTo(stateChange.to_CarryItem, plr);
				if(plr.action.Grab_Btn.just_pressed)
				{	
					plr.ThroWItem();
				}
			}
			//IF not
			else
			{
				stateChange change_command = plrUtility.LookForInteraction(plr, true, true);
				if(change_command != stateChange.NoChange) return ChangeStateTo(change_command, plr);
			}

		}//

		//Attack Logic
		if(plr.action.Attack_Btn.just_pressed) 
		{
			plrUtility.Combat.Attack_Meele(plr);
		}//

		//DEBUG//
		if (plr.action.Shoot_Btn.just_pressed)
		{
			plrUtility.Movement.Perform_Jump(plr);
		}
		if (plr.action.Select_Btn.isPressed)
		{
			GameCommand.RespawnPlayer(plr.id);
		}//debug//

		return this;
	}

	override public stateChange Move(Transform plrBody, Input_Plug.Analog_Stick move)
	{
		if( move.inputIntensity == 0.0f ) return stateChange.NoChange; 

		Vector3 plrPos_mapped = new Vector3(plrBody.position.x,0,plrBody.position.z).normalized;
		Vector3 dir = Vector3.Cross(plrPos_mapped, Vector3.down);
		dir = dir.normalized;

		Rigidbody my_rigidbody	=	plrBody.GetComponent<Rigidbody>();
		Vector3 moveVector = (move.x == 0 && move.y == 0) ? Vector3.zero : new Vector3(move.x, 0, move.y);

		Collider c_ground = plrUtility.bello.WhereIsThe_(Bello.ObjectLayer.Ground, plrBody);
		if(c_ground != null) 
		{
			moveVector 			   *=	4.5f;
		}
		else
		{
			moveVector *= 2.5f;
			moveVector = plrUtility.Movement.DropFromWall(moveVector, plrBody);
		}

		moveVector = dir * moveVector.x + plrPos_mapped * moveVector.z;
		plrBody.forward = Vector3.RotateTowards(plrBody.forward, moveVector, 25.0f, 1);
		moveVector += Vector3.up * Mathf.Clamp(my_rigidbody.velocity.y + Physics.gravity.y * Time.deltaTime,
		                                       Physics.gravity.y, 15);
		my_rigidbody.velocity = moveVector;

		if(DEBUG) Debug.DrawRay(plrBody.position, moveVector, Color.magenta,2.0f); 
		return stateChange.NoChange;
	}

}