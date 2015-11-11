using UnityEngine;
using System.Collections;

public class globalControl : MonoBehaviour {

	static float CamX;
	static float CamY;
	static float CamZ;

	public Transform CamHolder; 

	// Use this for initialization
	void Start () {

		Transform tomove = (CamHolder != null) ?  CamHolder : Camera.main.transform;
		CamX = tomove.position.x;
		CamY = tomove.position.y;
		CamZ = tomove.position.z;

		//DontDestroyOnLoad(gameObject);

	}
	
	// Update is called once per frame
	void Update () {
	

		if(Input.GetKey(KeyCode.Space)) 	MoveCam(Vector3.up);
		if(Input.GetKey(KeyCode.LeftShift)) MoveCam(-Vector3.up);

		if(Input.GetKey(KeyCode.UpArrow))		MoveCam( Vector3.forward);
		if(Input.GetKey(KeyCode.DownArrow))		MoveCam(-Vector3.forward);
		if(Input.GetKey(KeyCode.LeftArrow))		MoveCam( Vector3.left);
		if(Input.GetKey(KeyCode.RightArrow))	MoveCam(-Vector3.left);

		RotateCam();

		SwitchLevel();


	}

	void SwitchLevel()
	{
		if(Input.GetKeyDown(KeyCode.F1)) Application.LoadLevel(0);
		if(Input.GetKeyDown(KeyCode.F2)) Application.LoadLevel(1);
		//if(Input.GetKeyDown(KeyCode.F3)) Application.LoadLevel(2);
		//if(Input.GetKeyDown(KeyCode.F4)) Application.LoadLevel(3);
		//if(Input.GetKeyDown(KeyCode.F5)) Application.LoadLevel(4);
	}

	void RotateCam()
	{
		Transform tomove = (CamHolder != null) ?  CamHolder : Camera.main.transform;

		tomove.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 10 * Time.deltaTime, Space.Self);
		tomove.Rotate(Vector3.right * Input.GetAxis("Mouse Y") * -10 * Time.deltaTime, Space.Self);

		tomove.rotation = Quaternion.Euler(tomove.rotation.eulerAngles.x, tomove.rotation.eulerAngles.y ,0);
	}

	void MoveCam(Vector3 dir)
	{
		
		Transform tomove = (CamHolder != null) ?  CamHolder : Camera.main.transform;

		float camSpeed = 10.0f;
		tomove.position += dir * camSpeed * Time.deltaTime;
	}

	void OnGUI()
	{
		ShowData();
	}

	void ShowData()
	{
		Rect cornerRect = new Rect(10, 10, 500, 100);
		Transform tomove = (CamHolder != null) ?  CamHolder : Camera.main.transform;
		GUI.TextArea(cornerRect, "Cam: " + tomove.position.x + "_X, " +  tomove.position.y + "_Y, " + tomove.position.z + "_Z");
	}
}
