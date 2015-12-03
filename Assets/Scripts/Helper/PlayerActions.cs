using UnityEngine;
using System.Collections;
using InControl;

public class PlayerActions : PlayerActionSet
{
	public PlayerAction Left;
	public PlayerAction Right;
	public PlayerAction Up;
	public PlayerAction Down;
	public PlayerTwoAxisAction Move;

	public PlayerAction DodoInteraction;	// pickup and throw dodd
	public PlayerAction UseWeapon;			// use equipped weapon (fire ranged weapon, use melee weapon)
	public PlayerAction ToggleRanged;		// equip/unequip range weapon

	public PlayerActions()
	{
		Left = CreatePlayerAction( "Move Left" );
		Right = CreatePlayerAction( "Move Right" );
		Up = CreatePlayerAction( "Move Up" );
		Down = CreatePlayerAction( "Move Down" );
		Move = CreateTwoAxisPlayerAction( Left, Right, Down, Up );

		DodoInteraction = CreatePlayerAction( "Pick up/Throw Dodo" );
		UseWeapon = CreatePlayerAction( "Use Equipped Weapon" );
		ToggleRanged = CreatePlayerAction( "Equip/Unequip Range Weapon" );
	}

	public static PlayerActions CreateWithDefaultBindings()
	{
		PlayerActions playerActions = new PlayerActions();
		
//		playerActions.Fire.AddDefaultBinding( Key.Shift, Key.A );
//		playerActions.Fire.AddDefaultBinding( InputControlType.Action1 );
//		playerActions.Fire.AddDefaultBinding( Mouse.LeftButton );
//		
//		playerActions.Jump.AddDefaultBinding( Key.Space );
//		playerActions.Jump.AddDefaultBinding( InputControlType.Action3 );
//		playerActions.Jump.AddDefaultBinding( InputControlType.Back );
//		playerActions.Jump.AddDefaultBinding( InputControlType.System );
		
		playerActions.Up.AddDefaultBinding( Key.UpArrow );
		playerActions.Down.AddDefaultBinding( Key.DownArrow );
		playerActions.Left.AddDefaultBinding( Key.LeftArrow );
		playerActions.Right.AddDefaultBinding( Key.RightArrow );
		
		playerActions.Left.AddDefaultBinding( InputControlType.LeftStickLeft );
		playerActions.Right.AddDefaultBinding( InputControlType.LeftStickRight );
		playerActions.Up.AddDefaultBinding( InputControlType.LeftStickUp );
		playerActions.Down.AddDefaultBinding( InputControlType.LeftStickDown );
		
		playerActions.Left.AddDefaultBinding( InputControlType.DPadLeft );
		playerActions.Right.AddDefaultBinding( InputControlType.DPadRight );
		playerActions.Up.AddDefaultBinding( InputControlType.DPadUp );
		playerActions.Down.AddDefaultBinding( InputControlType.DPadDown );
		
		playerActions.Up.AddDefaultBinding( Key.W );
		playerActions.Down.AddDefaultBinding( Key.S );
		playerActions.Left.AddDefaultBinding( Key.A );
		playerActions.Right.AddDefaultBinding( Key.D );

		playerActions.DodoInteraction.AddDefaultBinding( Key.Space );
		playerActions.DodoInteraction.AddDefaultBinding( Key.O );
		playerActions.DodoInteraction.AddDefaultBinding( InputControlType.Action1 );

		playerActions.UseWeapon.AddDefaultBinding( Key.P );
		playerActions.UseWeapon.AddDefaultBinding( Key.M );
		playerActions.UseWeapon.AddDefaultBinding( InputControlType.Action2 );

		playerActions.ToggleRanged.AddDefaultBinding( Key.R );
		playerActions.ToggleRanged.AddDefaultBinding( Key.I );
		playerActions.ToggleRanged.AddDefaultBinding( InputControlType.RightBumper );
		
		playerActions.ListenOptions.IncludeUnknownControllers = true;
		playerActions.ListenOptions.MaxAllowedBindings = 3;
		// playerActions.ListenOptions.MaxAllowedBindingsPerType = 1;
		// playerActions.ListenOptions.UnsetDuplicateBindingsOnSet = true;
		// playerActions.ListenOptions.IncludeMouseButtons = true;
		
		playerActions.ListenOptions.OnBindingFound = ( action, binding ) =>
		{
			if (binding == new KeyBindingSource( Key.Escape ))
			{
				action.StopListeningForBinding();
				return false;
			}
			return true;
		};
		
		playerActions.ListenOptions.OnBindingAdded += ( action, binding ) =>
		{
			Debug.Log( "Binding added... " + binding.DeviceName + ": " + binding.Name );
		};
		
		playerActions.ListenOptions.OnBindingRejected += ( action, binding, reason ) =>
		{
			Debug.Log( "Binding rejected... " + reason );
		};
		
		return playerActions;
	}
}
