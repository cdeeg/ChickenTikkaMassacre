using UnityEngine;
using System.Collections;
using Input_Plug;

public abstract class PlayerState  {

	public bool DEBUG = false;

	// Update is called once per frame
	public abstract PlayerState Update (Player plr, bool DEBUG = false);
	public abstract void Move(Transform plrBody, Analog_Stick move);
	//public abstract void GetSlapped();

}
