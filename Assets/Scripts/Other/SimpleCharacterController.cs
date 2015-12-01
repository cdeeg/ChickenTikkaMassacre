using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using InControl;

public class SimpleCharacterController : NetworkBehaviour
{
	public void SetColor(Color32 col) {}

	InputDevice myDevice;
	PlayerCharacterSettings settings;

	#region Init
	public void Initialize(PlayerCharacterSettings genSettings, InputDevice dev)
	{
		myDevice = dev;
		settings = genSettings;
	}
	#endregion

	#region Unity
	void Start ()
	{
		if( !isLocalPlayer ) return;
	}
	
	void Update ()
	{
		Vector3 dire = Vector3.zero;

		// only update for the local player, NOT for everyone on the server
		if( !isLocalPlayer || myDevice == null ) return;

		if( myDevice.DPadDown.IsPressed || myDevice.LeftStickDown.IsPressed )
		{
			dire = Time.deltaTime * settings.moveSpeed * Vector3.back;
		}
		if( myDevice.DPadUp.IsPressed || myDevice.LeftStickUp.IsPressed )
		{
			dire = Time.deltaTime * settings.moveSpeed * Vector3.forward;
		}
		if( myDevice.DPadLeft.IsPressed || myDevice.LeftStickLeft.IsPressed )
		{
			dire = Time.deltaTime * settings.moveSpeed * Vector3.left;
		}
		if( myDevice.DPadRight.IsPressed || myDevice.LeftStickRight.IsPressed )
		{
			dire = Time.deltaTime * settings.moveSpeed * Vector3.right;
		}

		transform.position += dire;

		// synch with server

	}
	#endregion
}
