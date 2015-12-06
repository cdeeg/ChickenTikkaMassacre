using UnityEngine;
using System.Collections;
using Controler_Plug;
using Input_Plug;

public static class CreatePlayers{

	public static Player[] DoIt()
	{
		//moveTo -> gameControls as static with nr of players and further options
		GameObject newPlayer_one = GameObject.Instantiate(Resources.Load<GameObject>("Characters/Player_one"), GameStatics.playerSpawn_one, Quaternion.identity) as GameObject;
		GameObject newPlayer_two = GameObject.Instantiate(Resources.Load<GameObject>("Characters/Player_two"), GameStatics.playerSpawn_two, Quaternion.identity) as GameObject;
		return new Player[2]{ new Player(newPlayer_one.transform, controler_one(), 0 ),  new Player(newPlayer_two.transform, controler_two(), 1 )};

	}

	static Actions controler_one()
	{
		//move this too to the mysterious place where players are created
		// and make it so that 'Actions" are a value or fixed, dunno networking will decide how to do this
		
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
	
	static Actions controler_two()
	{
		//Creation of the virtual controler for the player actions
		//drawing the button info from the keyLayout file to map the buttons for the actions
		
		Button start_Btn	= new Button("Start_Two", KeyCode.Joystick2Button1);
		Button select_Btn	= new Button("Select_Two", KeyCode.Joystick2Button6);
		
		Button attack_Btn	= new Button("X_Two", KeyCode.Joystick2Button2);
		Button jump_Btn		= new Button("A_Two", KeyCode.Joystick2Button0);
		Button grab_Btn		= new Button("Y_Two", KeyCode.Joystick2Button3);
		
		Button aim_Btn		= new Button("LT_Two", KeyLayout.Aim_Key);
		Button shoot_Btn	= new Button("RT_Two", KeyCode.Joystick2Button5);
		
		ButtonAxis buttonAxis	= new ButtonAxis(	KeyLayout.Move_Axis_Xn, KeyLayout.Move_Axis_Xp,
		                                       KeyLayout.Move_Axis_Yn, KeyLayout.Move_Axis_Yp);
		
		Analog_Stick move 		= new Analog_Stick("axis 0_Two", "axis 1_Two", buttonAxis);
		
		return new Actions(start_Btn, select_Btn, jump_Btn, attack_Btn, grab_Btn, aim_Btn, shoot_Btn, move);
	}
}
