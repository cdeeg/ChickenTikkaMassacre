using UnityEngine;
using System.Collections;

public static class plrUtility {

	static LayerMask lm_Ladder = 1 << 13;
	static LayerMask lM_Ground = 1 << 15;
	static LayerMask lM_PickUp = 1 << 16;

	public static bool CheckForGround(Transform plrBody)
	{

		Vector3 	startingPoint 	= plrBody.position;
		Vector3 	vDirection		= Vector3.down;													//Change this if player needs to react to the curve of the world.
		float 		checkRange		= plrBody.GetComponent<CapsuleCollider>().height * 0.5f * plrBody.localScale.y + 0.1f;	//dependet on plr: height/2;

		Debug.DrawRay(startingPoint, vDirection * checkRange, Color.cyan);

		if( Physics.Raycast(startingPoint, vDirection, checkRange, lM_Ground  ) )
		{
			return true;
		}
		return false;
	}

	public static Collider CheckForPickUp(Transform plrBody)
	{
		Vector3 startingPoint = plrBody.position + plrBody.forward * 1.0f;
		float sphereRadius = 0.35f;

		Collider[] c = Physics.OverlapSphere(startingPoint, sphereRadius, lM_PickUp);

		if(c.Length > 0) return c[0];		

		return null;
	}

	public static GameObject Create_UI_Button(Collider c)
	{
		GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		btn.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);		
		
		btn.transform.position = c.transform.position + Vector3.up * 1.0f;
		btn.transform.rotation = Quaternion.Euler(0,180,0);
		Texture button = Resources.Load<Texture>("UI/BUTTON_Y");
		
		Renderer r = btn.GetComponent<Renderer>();
		r.material.shader = Shader.Find("Unlit/Transparent");
		r.material.SetTexture("_MainTex", button);

		return btn;
	}

	public static void UpdateButton(GameObject btn, Collider c)
	{
		btn.transform.position = c.transform.position + Vector3.up * 1.0f;
		btn.transform.rotation = Quaternion.Euler(0,180,0);
	}
	
	public static void Move_InAir(Transform plrBody, Input_Plug.Analog_Stick move)
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

	public static void LookForPickUp(Player plr)
	{
		Collider c = CheckForPickUp(plr.body);
		
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
			else ;//							{GameObject.Destroy(plr.buttonDisplay_Y);}	//destroy prev. object holder (button)
		}
		
		
	}

	public static Vector3 DropFromWall(Vector3 moveVector, Transform t)
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

	public static RaycastHit CheckForLadder(Player plr)
	{
		Transform t = plr.body;
		CapsuleCollider c		 		= t.GetComponent<CapsuleCollider>();
		Vector3 		start_offset	= Vector3.down * (c.height * 0.5f * t.localScale.y - 0.05f);
		Vector3			start			= t.position + start_offset;
		Vector3			rayVector 		= t.forward * (c.radius * t.localScale.z + 1.05f);
				
		Ray				ray				= new Ray(start, rayVector);
		LayerMask		lM				= lm_Ladder; //1 << 13;
		RaycastHit		hit;

		Debug.DrawRay(start, rayVector, Color.blue,2.0f);  

		if (Physics.Raycast(ray, out hit, rayVector.magnitude, lM))
		{
			if(Mathf.Abs( Vector3.Dot(hit.normal, hit.collider.transform.forward) ) == 1 || hit.normal == hit.collider.transform.right) hit = new RaycastHit();

			else
			{
				if(plr.buttonDisplay_Y == null)	{plr.buttonDisplay_Y = plrUtility.Create_UI_Button(plr.hitBox);}//if no button yet exists, create one,
				else 							{plrUtility.UpdateButton(plr.buttonDisplay_Y, plr.hitBox);}		//else update the current one.
				Debug.Log("Something");	 
			}

		}
		else 
		{
			if(plr.buttonDisplay_Y == null)	{;}											//
			else 							{GameObject.Destroy(plr.buttonDisplay_Y);}//GameObject.Destroy(plr.buttonDisplay_Y);}	//destroy prev. object holder (button)
			Debug.Log("NOOOOOOOOTHIIIIIIING");
		}
		return hit;
	}

	public static void PlaceOnLadder(Player plr, RaycastHit hit)
	{

		BoxCollider hitBox_Ladder = hit.collider.GetComponent<BoxCollider>();

		Vector3 topOfBox = hitBox_Ladder.size.y * 0.5f * hit.collider.transform.up + hit.collider.transform.position;
		topOfBox -= hitBox_Ladder.transform.right * 0.5f * hitBox_Ladder.size.x;

		Debug.DrawRay(topOfBox - Vector3.right * 0.5f, Vector3.right, Color.yellow, 1.0f);
		Debug.DrawRay(topOfBox - Vector3.up * 0.5f, Vector3.up, Color.yellow, 1.0f);

		Debug.DrawRay(hit.point - Vector3.right * 0.5f, Vector3.right, Color.magenta, 1.0f);
		Debug.DrawRay(hit.point - Vector3.up * 0.5f, Vector3.up, Color.magenta, 1.0f);


		Debug.DrawRay(topOfBox ,(hit.point - topOfBox), Color.magenta, 1.0f);
		Debug.DrawRay(topOfBox , -hitBox_Ladder.transform.up * hitBox_Ladder.size.y, Color.yellow, 1.0f);

		topOfBox -= hitBox_Ladder.transform.up * (hit.point - topOfBox).magnitude;// * Vector3.Dot((hit.point - topOfBox).normalized, -hitBox_Ladder.transform.up);

		Debug.DrawRay(topOfBox - Vector3.right * 0.5f, Vector3.right, Color.cyan, 1.0f);
		Debug.DrawRay(topOfBox - Vector3.up * 0.5f, Vector3.up, Color.cyan, 1.0f);

		//Vector3 OnLadderPos	=	hit.collider.transform.position - hit.collider.transform.right * hit.collider.GetComponent<BoxCollider>().size.x * 0.5f;
		//OnLadderPos -= Vector3.up * (OnLadderPos.y - hit.point.y);
		//OnLadderPos -= hit.collider.transform.right * hit.collider.GetComponent<BoxCollider>().size.x * 0.5f * (OnLadderPos.y - hit.point.y) / (hit.collider.GetComponent<BoxCollider>().size.y * 0.5f);

		topOfBox -= plr.body.forward * plr.hitBox.size.z * plr.body.localScale.z * 0.45f;
		topOfBox += Vector3.up * plr.hitBox.size.y * 0.5f * plr.body.localScale.y;
		plr.body.position = topOfBox;
	}

	public static Collider[] CheckForHit(Player plr)
	{

		float		radius = 1.1f;
		Vector3		center = plr.body.position + plr.body.forward *( plr.hitBox.bounds.extents.z * 0.5f * plr.body.localScale.z + radius + 0.25f); 
		LayerMask		lM = 1 << 17;

		Debug.DrawLine(center + Vector3.left * radius * 0.5f, center + Vector3.right * radius * 0.5f, Color.yellow, 2.0f);
		Debug.DrawLine(center + Vector3.forward * radius * 0.5f, center + Vector3.back * radius * 0.5f, Color.yellow, 2.0f);
		Debug.DrawLine(center + Vector3.up * radius * 0.5f, center + Vector3.down * radius * 0.5f, Color.yellow, 2.0f);
		Collider[] c = Physics.OverlapSphere(center, radius, lM);
		//if(c != null) Debug.Log("got Something");
		return c;
	}
}
