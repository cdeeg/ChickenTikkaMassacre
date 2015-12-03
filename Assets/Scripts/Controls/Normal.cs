using UnityEngine;
using System.Collections;

public class Normal : PlayerState
{
	public void CheckForStateChange()
	{
		//Check if fallen of map
		//Check if health < 0
		//Check if Start-Btn is pressed (?) -> should come from game
		//Check if ...

	}

	public override PlayerState Update(Player plr, bool DEBUG = false) 
	{
		this.DEBUG = DEBUG;
		//Move Around
		{
			//If grounded
			if(plrUtility.CheckForGround(plr.body)) 
			{
				if(DEBUG) Debug.Log("OnGround");
				Move(plr.body, plr.action.Move);
                
				//Jump logic
				if (plr.action.Jump_Btn.just_pressed)
				{
					plr.body.GetComponent<Rigidbody>().AddForce(Vector3.up * 5000.0f);
					plr.myAnimation.clip = plr.myAnimation.GetClip("Flap");
					plr.myAnimation.Play("Flap");
				}

            }
            //If feet are in the air
            else
			{
				if(DEBUG) Debug.Log("InAir");
                plrUtility.Move_InAir(plr.body, plr.action.Move);
			}
		}//

		//Object Interaction
		{
			if( plr.item_Holding != null )
			{
				if(plr.action.Grab_Btn.just_pressed)
				{	
					plr.DropItem();
				}
			}
			else
			{
				plrUtility.LookForPickUp(plr);
			}

		}

        if (plr.action.Select_Btn.isPressed)
        {
			GameCommand.RespawnPlayer(plr.id);
        }

		if(plr.action.Attack_Btn.just_pressed) 
		{

			plr.myAnimation.clip = plr.myAnimation.GetClip("slap");
			plr.myAnimation.Play("slap");

			Collider[] c = plrUtility.CheckForHit(plr);



			if(c != null) 
			{
				//Debug.Log("Player hit!");
				int enemyID = (plr.id == 0) ? 1 : 0;
				GameCommand.HitPlayer( c[0].transform.position, enemyID);
			}
		}


		//Jump logic
		if (plr.action.Shoot_Btn.just_pressed)
		{
			plr.body.GetComponent<Rigidbody>().AddForce(Vector3.up * 5000.0f);
			//plr.myAnimation.clip = plr.myAnimation.GetClip("slap");
			//plr.myAnimation.Play("slap");
		}


		//look for ladder
		RaycastHit hit = plrUtility.CheckForLadder(plr);
		if( hit.collider != null )
		{
			//Draw help/interaction button
			Debug.Log("Test");
			if(plr.action.Start_Btn.isPressed)
			{
				plrUtility.PlaceOnLadder(plr, hit);

				plr.body.forward	=	-(hit.normal - Vector3.up * hit.normal.y);

				Rigidbody r = plr.body.GetComponent<Rigidbody>();
				r.useGravity = false;
				r.velocity = Vector3.zero;

//				return new OnLadder();
			}
			//if (button pressed) -> climb on ladder

		}
		else
		{
			Debug.Log("no test");
		}

		return this;
	}


    //Override Base Functions
	//maye replace with Delegates instead of abstracts+override
	override public void Move(Transform plrBody, Input_Plug.Analog_Stick move)
	{
		if( move.inputIntensity == 0.0f ) return; 
		
		Vector3 plrPos_mapped = new Vector3(plrBody.position.x,0,plrBody.position.z).normalized;
		Vector3 dir = Vector3.Cross(plrPos_mapped, Vector3.down );
		dir = dir.normalized;
		
		Rigidbody my_rigidbody	=	plrBody.GetComponent<Rigidbody>();
		Vector3 moveVector		=	(move.x == 0 && move.y == 0) ? Vector3.zero : new Vector3(move.x, 0, move.y);
		moveVector 			   *=	4.5f;
		moveVector				= 	dir * moveVector.x + plrPos_mapped * moveVector.z;// new Vector3( moveVector.x * dir.x, 0, moveVector.z * plrBody.position.normalized.z );
		plrBody.forward			=	Vector3.RotateTowards(plrBody.forward, moveVector, 25.0f, 1);
		moveVector			   +=	Vector3.up * Mathf.Clamp(	my_rigidbody.velocity.y + Physics.gravity.y * Time.deltaTime,
		                                            Physics.gravity.y, 15);
		if(DEBUG) Debug.DrawRay(plrBody.position, moveVector, Color.magenta,2.0f);  
		
		
		//transform.Translate(moveVector * 0.25f);
		
		my_rigidbody.velocity = moveVector;
		
		// DropFromWall(plrBody);
		
	}


	

}