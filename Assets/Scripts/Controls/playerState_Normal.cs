using UnityEngine;
using System.Collections;

public class PlayerState
{
	public void CheckForStateChange()
	{
		//Check if fallen of map
		//Check if health < 0
		//Check if Start-Btn is pressed (?) -> should come from game
		//Check if ...

	}

	public void playerState_Normal(Player plr, bool DEBUG = false) 
	{
		//Move Around
		{
			//If grounded
			if(plrUtility.CheckForGround(plr.body)) 
			{
				if(DEBUG) Debug.Log("OnGround");
				Move(plr.body, plr.action.Move);
                if (plr.action.Jump_Btn.just_pressed) plr.body.GetComponent<Rigidbody>().AddForce(Vector3.up * 5000.0f);

            }
            //If feet are in the air
            else
			{
				if(DEBUG) Debug.Log("InAir");
                Move_InAir(plr.body, plr.action.Move);
			}
		}//

		//Object Interaction
		{
			if( plr.item_Holding != null )
			{
				if(plr.action.Grab_Btn.just_pressed)
				{	//plr.item_Holding.layer = 1 << 20;
					Rigidbody rB = plr.item_Holding.GetComponent<Rigidbody>();
					plr.item_Holding.transform.parent = null;
					rB.isKinematic = false;
					rB.AddForceAtPosition(plr.item_Holding.transform.forward * 500.0f, plr.item_Holding.transform.position + plr.item_Holding.transform.up * 0.1f);
					
					plr.item_Holding = null;
                    

				}
			}
			else
			{
				LookForPickUp(plr);
			}

		}

        if (plr.action.Select_Btn.isPressed)
        {
			GameCommand.RespawnPlayer(plr.id);
        }
	}

	//shared 
	void Move(Transform plrBody, Input_Plug.Analog_Stick move)
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
		Debug.DrawRay(plrBody.position, moveVector, Color.magenta,2.0f);  


		//transform.Translate(moveVector * 0.25f);

		my_rigidbody.velocity = moveVector;

       // DropFromWall(plrBody);
	
	}

    void Move_InAir(Transform plrBody, Input_Plug.Analog_Stick move)
    {
        //if (move.inputIntensity == 0.0f) return;

        Vector3 plrPos_mapped = new Vector3(plrBody.position.x, 0, plrBody.position.z).normalized;
        Vector3 dir = Vector3.Cross(plrPos_mapped, Vector3.down);
        dir = dir.normalized;

        Rigidbody my_rigidbody = plrBody.GetComponent<Rigidbody>();
        Vector3 moveVector = (move.x == 0 && move.y == 0) ? Vector3.zero : new Vector3(move.x, 0, move.y);
        moveVector *= 2.5f;
        moveVector = dir * moveVector.x + plrPos_mapped * moveVector.z;// new Vector3( moveVector.x * dir.x, 0, moveVector.z * plrBody.position.normalized.z );

        plrBody.forward = Vector3.RotateTowards(plrBody.forward, moveVector, 25.0f, 1);



        moveVector += Vector3.up * Mathf.Clamp(my_rigidbody.velocity.y + Physics.gravity.y * Time.deltaTime,
                                                Physics.gravity.y, 15);

        //transform.Translate(moveVector * 0.25f);
		moveVector = DropFromWall(moveVector, plrBody);
		my_rigidbody.velocity = moveVector;
       

    }

    void LookForPickUp(Player plr)
	{
		Collider c = plrUtility.CheckForPickUp(plr.body);

		if(c != null)
		{
			//if(DEBUG) Debug.Log("FOUND SOMETHING");

			//Display pick Up symbol
			if(plr.buttonDisplay_Y == null)	{plr.buttonDisplay_Y = plrUtility.Create_UI_Button(c);}	//if no button yet exists, create one,
			else 							{plrUtility.UpdateButton(plr.buttonDisplay_Y, c);}		//else update the current one.
		
			//PickUp
			if(plr.action.Grab_Btn.just_pressed) plr.PickUpItem(c.gameObject);
		
		}
		else //No Object in range
		{
			if(plr.buttonDisplay_Y == null)	{;}											//
			else 							{GameObject.Destroy(plr.buttonDisplay_Y);}	//destroy prev. object holder (button)
		}


	}

	Vector3 DropFromWall(Vector3 moveVector, Transform t)
    {
		CapsuleCollider c		 		= t.GetComponent<CapsuleCollider>();
		Vector3 		start_offset	= Vector3.down * (c.height * 0.5f * t.localScale.y - 0.01f);
		Vector3			start			= t.position + start_offset;
		//Vector3			horizontalVelo	= t.GetComponent<Rigidbody>().velocity.normalized;
		//				horizontalVelo -= Vector3.up * horizontalVelo.y;
		Vector3			rayVector 		= t.forward * (c.radius * t.localScale.z + 0.05f);

		Debug.DrawRay(start, rayVector, Color.red,2.0f);  

		Ray				ray				= new Ray(start, rayVector);
        LayerMask		lM				= 1 << 15;
		RaycastHit		hit;
		if (Physics.Raycast(ray, out hit, rayVector.magnitude, lM))
        {
			t.position -=  rayVector.normalized * ( ( (rayVector.magnitude) - hit.distance )  + 0.01f);
            return new Vector3(0, moveVector.y ,0); 
        }

		return moveVector;
    }
}