using UnityEngine;
using System.Collections;

using Input_Plug;
using Controler_Plug;

public class player_placecHolder : MonoBehaviour {

	//Actions plr_actions;
	Player player;


	Actions startUp_CreateControler()
	{
		//Creation of the virtual controler for the player actions
		//drawing the button info from the keyLayout file to map the buttons for the actions

		Button start_Btn	= new Button(KeyLayout.start_btnName, KeyLayout.start_Key);
		Button select_Btn	= new Button(KeyLayout.select_btnName, KeyLayout.select_Key);

		Button attack_Btn	= new Button(KeyLayout.attack_btnName, KeyLayout.attack_Key);
		Button jump_Btn		= new Button(KeyLayout.jump_btnName, KeyLayout.jump_Key);
		Button grab_Btn		= new Button(KeyLayout.grab_btnName, KeyLayout.grab_Key);

		Button aim_Btn		= new Button(KeyLayout.Aim_btnName, KeyLayout.Aim_Key);
		Button shoot_Btn	= new Button(KeyLayout.Shoot_btnName, KeyLayout.Shoot_Key);

		ButtonAxis buttonAxis	= new ButtonAxis(	KeyLayout.Move_Axis_Xn, KeyLayout.Move_Axis_Xp,
		                        	         		KeyLayout.Move_Axis_Yn, KeyLayout.Move_Axis_Yp);

		Analog_Stick move 		= new Analog_Stick(KeyLayout.Move_Axis_X, KeyLayout.Move_Axis_Y, buttonAxis);

		return new Actions(start_Btn, select_Btn, jump_Btn, attack_Btn, grab_Btn, aim_Btn, shoot_Btn, move);
	}
	// Use this for initialization
	void Start () {
	
		Actions plr_actions = startUp_CreateControler();
		player = new Player(transform, plr_actions);
	}
	
	// Update is called once per frame
	void Update () {

		PlayerState pState = new PlayerState();

		pState.playerState_Normal(player, true);
	}

	/*
	void Move_midAir(Analog_Stick move)
	{
		Rigidbody my_rigidbody = GetComponent<Rigidbody>();
		Vector3 moveVector = (move.x == 0 && move.y == 0) ? Vector3.zero : new Vector3(move.x, 0, move.y);
		moveVector = moveVector;
		
		//Vector3 rotatedForward = Vector3.RotateTowards(transform.forward, moveVector, 25.0f, 1);
		//float vectorAngle = Vector3.Angle(transform.forward, rotatedForward);
		//if(rotatedForward.x < 0) vectorAngle *= -1;
		//Quaternion newRotation = Quaternion.Euler(0, vectorAngle,0);
		//transform.rotation = newRotation;
		if(moveVector != Vector3.zero) transform.forward = Vector3.RotateTowards(transform.forward, moveVector, 25.0f, 1);
		
		//moveVector += Vector3.up * Mathf.Clamp(my_rigidbody.velocity.y + Physics.gravity.y * Time.deltaTime, Physics.gravity.y, 100);
		
		//transform.Translate(moveVector * 0.25f);
		Vector3 newSpeed = (my_rigidbody.velocity + moveVector);
		newSpeed = new Vector3(Mathf.Clamp(newSpeed.x, -player_AirSpeed, player_AirSpeed), 0, Mathf.Clamp(newSpeed.z, -player_AirSpeed, player_AirSpeed));

		//moveVector += Vector3.up * Mathf.Clamp(my_rigidbody.velocity.y + Physics.gravity.y * Time.deltaTime, Physics.gravity.y, 100);

		my_rigidbody.velocity += moveVector * Time.deltaTime;

		
	}
	*/

}
