using UnityEngine;
using System.Collections;

public class Drowning : PlayerState {

	Vector3 impactPoint;
	float timer;
	public Drowning(Vector3 impactPoint)
	{
		this.impactPoint = impactPoint;
		timer = 0;
	}


	public override PlayerState Update (Player plr, bool DEBUG)
	{
		timer += Time.deltaTime;

		if(timer >= 2.0f)
		{
			GameCommand.RespawnPlayer(plr.id);
			return ChangeStateTo(stateChange.to_Normal, plr);
		}

		plr.body.transform.position -= plr.body.transform.position.normalized * 0.03f;

		return this;
	}

	public override stateChange Move (Transform plrBody, Input_Plug.Analog_Stick move)
	{
		return stateChange.NoChange;
	}
}
