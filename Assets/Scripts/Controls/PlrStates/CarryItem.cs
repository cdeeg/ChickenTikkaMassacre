using System;
using UnityEngine;
using System.Collections;

public class CarryItem : Normal
{
	public override PlayerState Update (Player plr, bool DEBUG)
	{
		stateChange s = CheckForStateChangers(plr);
		if(s  != stateChange.NoChange)
		{
			return ChangeStateTo(s, plr);
		}
		//Move Around
		{
			//If feet are touching the grounded
			Move(plr.body, plr.action.Move);
			
		}//

		if( plr.item_Holding != null )
		{
			if(plr.action.Grab_Btn.just_pressed)
			{	
				plr.ThroWItem();
				return ChangeStateTo(stateChange.to_Normal, plr);
			}
		}

		else return ChangeStateTo(stateChange.to_Normal, plr);

		return this;
	}
	

}

