using UnityEngine;
using System.Collections;

using Controler_Plug;
using Input_Plug;

using GameStats;

public class GAME : MonoBehaviour {

	PlayerState pState_one = new PlayerState();
	PlayerState pState_two = new PlayerState();
	Player[] Players;

	//Splitscreen
	Transform plr_one_cam;
	Transform plr_two_cam;
	//Playball dodo;

	//Gamestate gamestate;
	//Scoreboard scoreboard;

	Transform cam;

	// Use this for initialization
	void Start () {

		//Prefab instantiations and rule setup
		Players = CreatePlayers();
		GameCommand.SetGameInstance(this);



		// simple Splitscreen solution
		CreatePlayerCams();


		//single player cam/oculus
		cam = GameObject.Find("OVRCameraRig").transform;
	}
	
	// Update is called once per frame
	void Update () {

		//player update
		pState_one.playerState_Normal(Players[0], true);
		pState_two.playerState_Normal(Players[1], false);

	
		// simple Splitscreen solution
		plr_one_cam.LookAt(Players[0].body);
		plr_two_cam.LookAt(Players[1].body);

		//DEBUG
		if(Input.GetKey(KeyCode.LeftArrow)) MoveCam_Hor(Vector3.up);
		if(Input.GetKey(KeyCode.RightArrow))MoveCam_Hor(Vector3.down);
		if(Input.GetKey(KeyCode.UpArrow))	MoveCam_Ver(-1);
		if(Input.GetKey(KeyCode.DownArrow))	MoveCam_Ver( 1);
	}

	void OnGUI()
	{
		GameUI.DrawScoreBoard();
	}

	//Interface
	public Player[] GetPlayers()
	{
		return Players;
	}

	//ToBeMoved -> player creation
	Player[] CreatePlayers()
	{
		//moveTo -> gameControls as static with nr of players and further options
		GameObject newPlayer_one = GameObject.Instantiate(Resources.Load<GameObject>("Characters/Player_one"), GameStatics.playerSpawn_one, Quaternion.identity) as GameObject;
		GameObject newPlayer_two = GameObject.Instantiate(Resources.Load<GameObject>("Characters/Player_two"), GameStatics.playerSpawn_two, Quaternion.identity) as GameObject;
		return new Player[2]{ new Player(newPlayer_one.transform, controler_one(), 0 ),  new Player(newPlayer_two.transform, controler_two(), 1 )};
		
	}

	Actions controler_one()
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

	Actions controler_two()
	{
		//Creation of the virtual controler for the player actions
		//drawing the button info from the keyLayout file to map the buttons for the actions
		
		Button start_Btn	= new Button(KeyLayout.start_btnName, KeyLayout.start_Key);
		Button select_Btn	= new Button("Select_Two", KeyCode.Joystick2Button6);
		
		Button attack_Btn	= new Button("X_Two", KeyCode.Joystick2Button2);
		Button jump_Btn		= new Button("A_Two", KeyCode.Joystick2Button0);
		Button grab_Btn		= new Button("Y_Two", KeyCode.Joystick2Button3);
		
		Button aim_Btn		= new Button("LT_Two", KeyLayout.Aim_Key);
		Button shoot_Btn	= new Button("RT_Two", KeyLayout.Shoot_Key);
		
		ButtonAxis buttonAxis	= new ButtonAxis(	KeyLayout.Move_Axis_Xn, KeyLayout.Move_Axis_Xp,
		                                       KeyLayout.Move_Axis_Yn, KeyLayout.Move_Axis_Yp);
		
		Analog_Stick move 		= new Analog_Stick("axis 0_Two", "axis 1_Two", buttonAxis);
		
		return new Actions(start_Btn, select_Btn, jump_Btn, attack_Btn, grab_Btn, aim_Btn, shoot_Btn, move);
		
	}

	//DEBUG
	void MoveCam_Hor(Vector3 dir)
	{
		cam.RotateAround(Vector3.zero, dir, 20 * Time.deltaTime);
	}

	void MoveCam_Ver(int dir)
	{
		cam.RotateAround(Vector3.zero, cam.right, dir * 20 * Time.deltaTime);
	}
	
	void CreatePlayerCams()
	{
		Camera.main.enabled = false;
		Vector3 camSpawn = Vector3.up * 12.0f;

		GameObject d = GameObject.Instantiate( Resources.Load<GameObject>("plrCam"), camSpawn, Quaternion.identity) as GameObject;
		plr_one_cam = d.transform;
		plr_one_cam.GetComponent<Camera>().rect = new Rect(0,0, 1, 0.5f);
		plr_one_cam.GetComponent<Camera>().fieldOfView = 35.0f;

		d = GameObject.Instantiate( Resources.Load<GameObject>("plrCam"), camSpawn, Quaternion.identity) as GameObject;
		plr_two_cam = d.transform;
		plr_two_cam.GetComponent<Camera>().rect = new Rect(0f,0.5f, 1, 0.5f);
		plr_two_cam.GetComponent<Camera>().fieldOfView = 35.0f;
	}
	
}
