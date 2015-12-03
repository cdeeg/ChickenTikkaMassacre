using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using InControl;
using System.Collections.Generic;

struct PlayerMove
{
	public int moveNum;
	public Vector3 moveDire;
}

[RequireComponent(typeof(WeaponController))]
public class SimpleCharacterController : NetworkBehaviour
{
	// important objects
	PlayerActions actions;				// controls
	PlayerCharacterSettings settings;	// character settings (from Startup Manager)
	WeaponController weaponController;	// weapon controller

	[SyncVar(hook="OnServerStateChanged")] PlayerMove serverMove;	// current position
	[SyncVar]Color playerColor = Color.black;						// chosen color (sync once)
	[SyncVar]int currentHealth;										// current health
	List<PlayerActions> pendingMoves;								// pending actions made by the player
	PlayerMove predictedState;										// state of the player deduced from their previous action

	int initialHealth; // TODO remove?

	public void SetColor( Color col ) { playerColor = col; } // set player color

	#region Network
	[Server] void Initialize()
	{
		// get the settings (since the StartupManager object also contains the NetworkManager,
		// we can be sure it's there
		settings = FindObjectOfType<StartupManager>().settings;

		// set initial and current health
		initialHealth = currentHealth = settings.health;

		// get WeaponController
		weaponController = GetComponent<WeaponController>();

		// initialize controls
		actions = PlayerActions.CreateWithDefaultBindings();
		// TODO check what PlayerPrefs do exactly
	}

	void OnServerStateChanged ( PlayerMove newMove )
	{
		serverMove = newMove;
	}

	void UpdatePredictedState ()
	{
		predictedState = serverMove;
//		foreach (PlayerMove action in pendingMoves)
//		{
//			predictedState = DoAction(predictedState, action);
//		}
	}

	#endregion

	#region Combat
	public void ApplyDamage( int dmg )
	{
		currentHealth -= dmg;
		if( currentHealth <= 0 )
		{
			Die ();
		}
		else if( currentHealth > initialHealth )
		{
			currentHealth = initialHealth;
		}
	}

	void Die()
	{
		weaponController.Purge();
		// TODO activate ragdoll
	}

	public void Respawn()
	{
		currentHealth = initialHealth;
		// TODO deactivate ragdoll
	}
	#endregion

	#region Unity
	void Awake()
	{
		Initialize();
	}

	void Start()
	{
		if (isLocalPlayer) {
			pendingMoves = new List<PlayerActions>();
			UpdatePredictedState();
		}
	}

	void DoAction(PlayerMove previous, PlayerAction action)
	{

	}

	void Update ()
//	PlayerMove DoAction( PlayerMove prev, PlayerAction action )
	{
		Vector3 dire = Vector3.zero;
		Quaternion lookAt = Quaternion.identity;
//		action.Name == "Use Weapon"

		// only update for the local player, NOT for everyone on the server
		if( !isLocalPlayer || actions == null ) return;

		if( actions.UseWeapon.WasReleased )
		{
			if( weaponController.HasRangeWeaponEquipped() )
				weaponController.ShootRangedWeapon();
		}
		else if( actions.ToggleRanged.WasReleased )
		{
			weaponController.ToggleRanged();
		}

		// no walking while aiming
		if( weaponController.HasRangeWeaponEquipped() )
		{
			// TODO set lookAt to transform.position - $(cameraRaycastTarget)
			return;
		}
		else
		{
			// move (don't move while holding range weapons)
			dire.x = Time.deltaTime * settings.moveSpeed * actions.Move.X;
			dire.z = Time.deltaTime * settings.moveSpeed * actions.Move.Y;
			lookAt = Quaternion.LookRotation( Vector3.forward, Vector3.up );
		}

//		return new PlayerMove { dire, }

		transform.position += dire;
		transform.LookAt( transform.position + dire );

		if( dire != Vector3.zero )
			transform.rotation = Quaternion.Slerp( transform.rotation, lookAt, Time.deltaTime * settings.rotationSpeed );

		// TODO synch with server

	}
	#endregion
}
