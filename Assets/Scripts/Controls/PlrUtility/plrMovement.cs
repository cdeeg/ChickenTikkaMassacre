using UnityEngine;
using System.Collections;

public class plrMovement {

	public Vector3 DropFromWall(Vector3 moveVector, Transform t)
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

	public void Perform_Jump(Player plr)
	{		
		plr.body.GetComponent<Rigidbody>().AddForce(Vector3.up * 5000.0f);		
	}

	public void PlacePlayerOnLadder(Collider ladder, Player plr)
	{
		//Collider ladder = Bello.WhereIsThe_(Bello.ObjectLayer.Ladder, plr.body);
		
		if(ladder == null) return;
		
		BoxCollider hitBox_Ladder = ladder.GetComponent<BoxCollider>();
		
		Vector3 topOfBox = ladder.transform.position + ladder.transform.up * hitBox_Ladder.size.y * 0.5f;
		topOfBox -= hitBox_Ladder.transform.right * 0.5f * hitBox_Ladder.size.x;
		
		plr.body.forward	= (ladder.transform.right - Vector3.up * ladder.transform.right.y);
		
		float maxHeightDiff	= topOfBox.y -  (topOfBox - ladder.transform.up * ladder.transform.localScale.y * hitBox_Ladder.size.y).y;
		float heightDiff	= topOfBox.y - (plr.body.position - Vector3.up * plr.hitBox.size.y * plr.body.localScale.y * 0.5f).y;
		
		plr.body.position = topOfBox - ladder.transform.up * hitBox_Ladder.size.y * ladder.transform.localScale.y  *  (heightDiff / maxHeightDiff);
		plr.body.position += plr.body.up * plr.body.localScale.y;
		plr.body.position -= plr.body.forward * plr.body.localScale.z * plr.hitBox.size.z * 0.51f;
	}

	public void MoveOnBridge(Transform plrBody, Input_Plug.Analog_Stick move, Collider c_ground)
	{
			
		if( move.inputIntensity == 0.0f ) return; 
		
		Vector3 plrPos_mapped = new Vector3(plrBody.position.x,0,plrBody.position.z).normalized;
		Vector3 dir = Vector3.Cross(plrPos_mapped, Vector3.down );
		dir = dir.normalized;
		
		Rigidbody my_rigidbody	=	plrBody.GetComponent<Rigidbody>();
		Vector3 moveVector		=	(move.x == 0 && move.y == 0) ? Vector3.zero : new Vector3(move.x, 0, move.y);
		moveVector 			   *=	2.5f;
		moveVector				= 	dir * moveVector.x + plrPos_mapped * moveVector.z;// new Vector3( moveVector.x * dir.x, 0, moveVector.z * plrBody.position.normalized.z );
		
		float negation = Mathf.Sign( Vector3.Dot(moveVector.normalized, c_ground.transform.right) );
		moveVector				+= c_ground.transform.right * negation;
		
		
		plrBody.forward			=	Vector3.RotateTowards(plrBody.forward, moveVector, 25.0f, 1);
		moveVector			   +=	Vector3.up * Mathf.Clamp(	my_rigidbody.velocity.y + Physics.gravity.y * Time.deltaTime,
		                                            Physics.gravity.y, 15);

		my_rigidbody.velocity = moveVector;
	}
}
