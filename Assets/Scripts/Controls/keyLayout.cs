using UnityEngine;
using System.Collections;

public static class KeyLayout {
	//Keys to be mapped to buttons used for actions


	/*
	NAMING CONVENTION
	
	public static KeyCode	NameOfTheAction_Key	= KeyCode.ID_of_The_keyboard_key;
	public static string	NameOfTheAction_btnName = "Name needed to call the button or axis";

	*/

	public static KeyCode	start_Key		= KeyCode.Escape;
	public static string 	start_btnName	= "Start";

	public static KeyCode	select_Key		= KeyCode.Joystick1Button6;
	public static string 	select_btnName	= "Select";

	public static KeyCode	attack_Key		= KeyCode.Mouse0;
	public static string 	attack_btnName	= "X";

	public static KeyCode	jump_Key		= KeyCode.Joystick1Button0;
	public static string 	jump_btnName	= "A";

	public static KeyCode	grab_Key		= KeyCode.Joystick1Button1;
	public static string 	grab_btnName	= "Y";

	public static KeyCode	Aim_Key			= KeyCode.Mouse1;
	public static string 	Aim_btnName 	= "LT";

	public static KeyCode	Shoot_Key		= KeyCode.Mouse3;
	public static string 	Shoot_btnName	= "RT";

	//Move Axis//
	public static KeyCode	Move_Axis_Xn	= KeyCode.A; 
	public static KeyCode	Move_Axis_Xp	= KeyCode.D;

	public static KeyCode	Move_Axis_Yn	= KeyCode.S;
	public static KeyCode	Move_Axis_Yp	= KeyCode.W;

	public static string	Move_Axis_X		= "axis 0";
	public static string	Move_Axis_Y		= "axis 1";



}
