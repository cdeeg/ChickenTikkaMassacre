using UnityEngine;
using System.Collections;

public static class plrUtility {


	public static plrUI UI = new plrUI();
	public static plrMovement Movement = new plrMovement();
	public static plrCombat Combat = new plrCombat();
	public static Bello bello = new Bello();


	public static PlayerState.stateChange LookForInteraction(Player plr, bool can_PickUp, bool can_GrabLadder)
	{
		Collider c = null;
		if(can_PickUp)
		{
			c = bello.WhereIsThe_(Bello.ObjectLayer.PickUp, plr.body);
			if( c != null)
			{
				if(plr.action.Grab_Btn.just_pressed)
				{
					if(c.transform.parent != null) plr.PickUpItem(c.transform.parent.gameObject);
					else plr.PickUpItem( c.gameObject );
					//Create_UI_Button(null, plr);
					return PlayerState.stateChange.NoChange;
				}
			}
		}
		if(can_GrabLadder)//Lock for ladder
		{
			c = bello.WhereIsThe_(Bello.ObjectLayer.Ladder, plr.body);
			if( c != null)
			{
				if(plr.action.Grab_Btn.just_pressed)
				{
					//Place player On Ladder
					return PlayerState.stateChange.to_OnLadder;
				}
			}
		}
		//else

		UI.Create_UI_Button(c, plr);
		return PlayerState.stateChange.NoChange;
	}
}
