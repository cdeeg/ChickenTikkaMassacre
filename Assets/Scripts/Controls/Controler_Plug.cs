using UnityEngine;
using System.Collections;

using Input_Plug;

namespace Controler_Plug {

	//Controler class to map keyInput to 'character-actions'.
	//
	//Player  possible actions
	public struct Actions
	{
		public Button Start_Btn;	// start-button -> menu/pause
		public Button Select_Btn;	// select-button  (?)

		public Button Jump_Btn;		// A-button -> well ... accelerating the player body upwards? 
		public Button Attack_Btn;	// X-button -> flinging fists in front of you
		public Button Grab_Btn;		// B-Button -> Grab/Throw, Pick up things, hold them over your head and fling them along your current 'forward direction' by pressing the button again (throw) 
									// things like stones and grenades.

		public Button Aim_Btn;		// Left-Trigger	 -> to be able to shot accuratly you first hold down this button to aim your player's attack to the point where you're looking (with the occulus) and then...
		public Button Shoot_Btn;	// Right-Trigger -> shot, fire a projectile towards the point you aimed at. things like spears.

		public Analog_Stick Move;	// Left_Analog	-> Moving around.

		public Actions(	Button Start_Btn,
		               	Button Select_Btn,
		                Button Jump_Btn,
		                Button Attack_Btn,
		               	Button Grab_Btn,
		                Button Aim_Btn,
		                Button Shoot_Btn,

		               Analog_Stick Move
			)
		{
			this.Start_Btn	= Start_Btn;
			this.Select_Btn	= Select_Btn;
			this.Jump_Btn	= Jump_Btn;
			this.Attack_Btn	= Attack_Btn;
			this.Grab_Btn	= Grab_Btn;
			this.Aim_Btn	= Aim_Btn;
			this.Shoot_Btn	= Shoot_Btn;

			this.Move		= Move;
		}
	}
}
