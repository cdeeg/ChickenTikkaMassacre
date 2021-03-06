﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CameraControl : MonoBehaviour
{
	public CameraControlSettings settings;

	PlayerActions actions;
	GameObject trg;

	public bool FreeMove { get; set; } // set to true if you don't want autofollow

	void Start()
	{
		// create player actions (not the same as in SimpleCharacterController, since
		// this stuff here will be ignored by the Oculus (I hope))
		actions = PlayerActions.CreateWithDefaultBindings();

		// invert Y axis if necessary
		actions.CamMove.InvertYAxis = settings.invertYAxis;

		FreeMove = false;
	}

	public void SetFollowTarget( GameObject obj )
	{
		trg = obj;
	}

	void Update()
	{
		// this doesn't need to check for isLocalPlayer. turns out, cameras ARE
		// already local player only (soooo sweet!)
		if( actions.CamMove.IsPressed )
		{
			float x = actions.CamMove.X;
			if( settings.invertXAxis ) x *= -1f; // invert x manually if necessary
			float y = actions.CamMove.Y;

			transform.Rotate( new Vector3( 0, x, 0 ) );
			Vector3 move = transform.position;
			move.y += Time.deltaTime * settings.cameraMovementSpeed * y;
			transform.position = move;
		}
		else if( settings.autofollowWhenNecessary && !FreeMove )
		{
			if( trg == null ) return;

			transform.LookAt( trg.transform );
		}
	}
}
