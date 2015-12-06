using UnityEngine;
using System.Collections;

public class KO : PlayerState {

	float timeStamp;
	// Use this for initialization
	public KO(float timeStamp)
	{
		this.timeStamp = timeStamp;
	}

	public override PlayerState Update (Player plr, bool DEBUG)
	{
		if(Time.time - timeStamp > 2) 
		{
			plr.health = Player.MaxHealth;
			return new Normal();
		}
		Debug.Log("still KO: " + timeStamp);
		return new KO(timeStamp);
	}

	public override stateChange Move (Transform plrBody, Input_Plug.Analog_Stick move)
	{
		return stateChange.NoChange;
	}
}
