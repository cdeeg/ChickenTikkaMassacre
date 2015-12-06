using UnityEngine;
using System.Collections;

using Controler_Plug;
using Input_Plug;

using GameStats;

public class GAME : MonoBehaviour {

	public bool DoSplitScreen = false;
	public bool DoOnRail = false;

	public PlayerState pState_one = new Normal();
	public PlayerState pState_two = new Normal();
	Player[] Players;

	//Splitscreen
	Transform plr_one_cam;
	Transform plr_two_cam;
	Transform cam; //Oculus

	// Use this for initialization
	void Start () {

		//Prefab instantiations and rule setup
		Players = CreatePlayers.DoIt();
		GameCommand.SetGameInstance(this);
		// simple Splitscreen solution
		if(DoSplitScreen) CreatePlayerCams();
		//single player cam/oculus
		else cam = GameObject.Find("OVRCameraRig/TrackingSpace/TrackerAnchor").transform;
	}
	
	// Update is called once per frame
	void Update () {

		//player update
		pState_one = pState_one.Update(Players[0], true);
		pState_two = pState_two.Update(Players[1], false);
		
		// simple Splitscreen solution
		if(DoSplitScreen) SplitScreen();
		else PositionCamera();

		//DEBUG
		if(Input.GetKeyDown(KeyCode.Keypad7)) Players[0].GetSlapped(Players[0].body.position);
	}
	
	void PositionCamera()
	{
		Vector3 newCamPos = cam.transform.forward * 8.0f;
		newCamPos += Vector3.up * 16;
		cam.parent.parent.position = newCamPos;
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

	public IEnumerator GrillMeat(GameObject meat)
	{
		if(meat == null) Debug.Log("no meat");

		meat.transform.parent.FindChild("Fire").GetComponent<ParticleSystem>().Play();
		
		meat.transform.FindChild("Dodo/Dodo 1").gameObject.SetActive(true);

		meat.GetComponent<AudioSource>().PlayOneShot( Resources.Load<AudioClip>("Sounds/ScorePoint/fire_temp") );

		float duration = 5.0f;
		float current = 0;
		float startTime = Time.time;
		
		while(Time.time - startTime < duration)
		{
			meat.transform.Rotate(meat.transform.forward, 20 * Time.deltaTime);
			yield return false;	
		}
		
		meat.transform.FindChild("Dodo/Dodo 1").gameObject.SetActive(false);
		
		yield return false;
	}

	//SPLITSCREEN
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
	void SplitScreen()
	{
		plr_one_cam.LookAt(Players[0].body);
		plr_two_cam.LookAt(Players[1].body);
		
		if(DoOnRail)plr_one_cam.position = plr_one_cam.transform.forward * 3.5f + Vector3.up * 16;
		if(DoOnRail)plr_two_cam.position = plr_two_cam.transform.forward * 3.5f + Vector3.up * 16;
	}
	//splitscreen_end


}
