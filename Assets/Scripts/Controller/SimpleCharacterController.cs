using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using InControl;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

struct PlayerMove
{
	public int moveNum;				// server sync

	public Vector3 currentPosition;	// position
	public Vector3 lookAtThis;		// rotation
	public bool hasRangeEquipped;	// current weapon type
	public bool useWeapon;			// is using weapon
	public bool grabbing;			// player is trying to grab the dodo
}

struct PlayerNetworkActionContainer
{
	// the player's current action
	public PlayerNetworkAction action;

	// current position
	public float moveX;
	public float moveY;
	public float moveZ;

	// current rotation
	public float rotateX;
	public float rotateZ;

	// true if range weapon is equipped, false if not
	public bool hasRange;
	public bool usingWeapon;
	public bool isGrabbing;
}

[RequireComponent(typeof(WeaponController))]
[NetworkSettings(channel=2)]public class SimpleCharacterController : NetworkBehaviour
{
	public Transform weaponAnchor;		// parent weapons here!
	public GameObject rayCastObject;	// check if the character is jumping or not
	public InteractionTrigger grabber;	// also used for melee weapon hit detection
	public Transform dodoAnchor;		// parent dodo to this

	// REMOVE THESE WHEN ANIMS WORK!
	float maxGrabTime = 1f;
	float maxUseWeaponTime = 1f;
	// END TMP SHIT

	// local variables
	PlayerActions actions;				// controls
	PlayerCharacterSettings settings;	// character settings (from Startup Manager)
	WeaponController weaponController;	// weapon controller

	// network variables
	[SyncVar(hook="OnServerStateChanged")] PlayerMove serverMove;	// current position (sync a if necessary)
//	[SyncVar]Color playerColor = Color.black;						// chosen color (sync once) // TODO
	[SyncVar]int currentHealth;										// current health

	List<PlayerNetworkActionContainer> pendingMoves;				// pending actions made by the player
	PlayerMove predictedState;										// state of the player deduced from their previous action
	SpawnPoint home;

	int initialHealth; // TODO remove?
	bool hasRangeWeapon;
	bool usingWeapon;
	bool isJumping;
	bool isGrabbing;
	Vector3 lastLookDirection;
	Rigidbody rb;

	//	public void SetColor( Color col ) { playerColor = col; } // set player color // TODO

	#region Network
	// called when client is started, aka the player is spawned -> initialize stuff
	// NOTE: this is that function that is called not just on the client
	public override void OnStartClient()
	{
		// get the settings (since the StartupManager object also contains the NetworkManager,
		// we can be sure it's there
		StartupManager myManager = FindObjectOfType<StartupManager>();
		settings = myManager.settings;

		// set initial and current health
		initialHealth = currentHealth = settings.health;

		isJumping = false;
		hasRangeWeapon = false;
		isGrabbing = false;
		usingWeapon = false;

		// get WeaponController
		weaponController = GetComponent<WeaponController>();
		weaponController.Initialize(weaponAnchor, myManager.GetWeaponObjectPool());

		if( rayCastObject == null )
		{
			Debug.Log("SimpleCharacterController: OnStartClient: Can't do ray cast; disabling jumping...");
		}

		// initialize controls
		actions = PlayerActions.CreateWithDefaultBindings();

		// TODO check what PlayerPrefs do exactly (prolly control scheme bs)
	}

	void OnStopClient()
	{
		RemovePlayerFromTeam();
	}

	void OnStopHost()
	{
		RemovePlayerFromTeam();
	}

	void OnDisable()
	{
		RemovePlayerFromTeam();
	}

	void RemovePlayerFromTeam()
	{
		// remove this player from their team (team gets unassigned if player count is 0)
		if( home != null ) home.RemoveTeamMember();
	}

	void FindFreeSpawnPoint()
	{
		if( home != null ) return; // if the dude already HAS a home, don't search for another one

		SpawnPoint[] spawners = FindObjectsOfType<SpawnPoint>();
		if( spawners.Length == 0 )
		{
			Debug.LogWarning("SimpleCharacterController: OnStartClient: Can't find any spawn points in this level. Aborting...");
			return;
		}

		// check all found spawn points if a team has been assigned yet
		home = null;
		for( int i = 0; i < spawners.Length; ++i )
		{
			if( !spawners[i].HasTeamAssigned() )
			{
				// spawner is free -> assign this player to it to make a new team
				spawners[i].AssignTeam();
				home = spawners[i];
				break;
			}
		}
	}

	void OnServerStateChanged ( PlayerMove newMove )
	{
		serverMove = newMove;
		if (pendingMoves != null)
		{
			// remove moves until the move count of local and server match
			while (pendingMoves.Count > (predictedState.moveNum - serverMove.moveNum))
			{
				pendingMoves.RemoveAt( 0 );
			}
			// update state
			UpdatePredictedState();
		}
	}

	void UpdatePredictedState ()
	{
		predictedState = serverMove;
		foreach (PlayerNetworkActionContainer action in pendingMoves)
		{
			predictedState = DoAction(predictedState, action);
		}
	}

	void SyncState ( bool init )
	{
		PlayerMove currentState = isLocalPlayer ? predictedState : serverMove;

		// match up equipped weapon
		if( currentState.hasRangeEquipped != weaponController.HasRangeWeaponEquipped() )
		{
			weaponController.ToggleRanged();
		}
		else if( currentState.grabbing != isGrabbing )
		{
			if( currentState.grabbing )
			{
				if( dodoAnchor.childCount == 0 )
				{
					if( grabber != null && grabber.CanGrabDodo )
					{
//						grabber.DodoObject.GetComponent<Rigidbody>().detectCollisions = false;
//						grabber.DodoObject.GetComponent<Rigidbody>().useGravity = false;
//						grabber.DodoObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

						grabber.DodoObject.transform.position = dodoAnchor.position;
						grabber.DodoObject.transform.parent = dodoAnchor;
					}
				}
				else
				{
//					grabber.DodoObject.GetComponent<Rigidbody>().detectCollisions = false;
//					grabber.DodoObject.GetComponent<Rigidbody>().useGravity = false;
					dodoAnchor.GetChild( 0 ).transform.parent = null;
				}
			}
		}
		else if( currentState.useWeapon != usingWeapon )
		{
			bool success = weaponController.UseWeapon();
			if( usingWeapon && success )
				StartCoroutine( UsingWeapon() );
		}

		// current position is not set (aka the player didn't move)? don't change look rotation
		if( currentState.lookAtThis.x != -2 && currentState.lookAtThis.z != -2 )
			transform.rotation = Quaternion.LookRotation( currentState.lookAtThis );


		if( !weaponController.HasRangeWeaponEquipped() )
		{
			// lerp position for smooth movement
			transform.position = Vector3.Lerp( transform.position, currentState.currentPosition, Time.deltaTime * settings.moveSpeed );
		}
	}
	#endregion

	#region helper coroutines (will be replaced by animation triggers)
	IEnumerator Grabbing()
	{
		yield return new WaitForSeconds( maxGrabTime );

		isGrabbing = false;
	}

	IEnumerator UsingWeapon()
	{
		usingWeapon = true;

		yield return new WaitForSeconds( maxUseWeaponTime );

		usingWeapon = false;
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
		transform.position = home.GetSpawnPosition();
		// TODO spawn at enemy's homebase for shorter ways?
		// TODO deactivate ragdoll
	}
	#endregion

	#region Unity
	void Start()
	{
		if ( isLocalPlayer )
		{
			pendingMoves = new List<PlayerNetworkActionContainer>();
			FindFreeSpawnPoint();
			transform.position = home.GetSpawnPosition();
			lastLookDirection = new Vector3(-2,0,-2);
			rb = GetComponent<Rigidbody>();
			serverMove = new PlayerMove
			{
				moveNum = 0,
				currentPosition = home.GetSpawnPosition(),
				lookAtThis = lastLookDirection,
				hasRangeEquipped = false,
				useWeapon = false,
				grabbing = false
			};
			UpdatePredictedState();
		}
		SyncState( true );
		//SyncColor(); // TODO (if we have smth like that)
	}

	void Update ()
	{
		if (isLocalPlayer)
		{
			bool grabThis = isGrabbing;
			bool badaboom = usingWeapon;

			// set up PlayerNetworkActionContainer
			PlayerNetworkActionContainer pressedKey = new PlayerNetworkActionContainer();

			pressedKey.action = PlayerNetworkAction.NONE;
			pressedKey.moveX = transform.position.x;
			pressedKey.moveZ = transform.position.y;
			pressedKey.moveY = transform.position.z;

			pressedKey.rotateX = lastLookDirection.x;
			pressedKey.rotateZ = lastLookDirection.z;

			// use WasPressed here to avoid mass toggling range/mass using weapon
			if( actions.UseWeapon.WasPressed &&
				!usingWeapon &&
				( settings.allowJumpingWithWeapon // no additional checks necessary
				|| !settings.allowJumpingWithWeapon && !isJumping ) ) // check if player is jumping
			{
				pressedKey.action = PlayerNetworkAction.USE_WEAPON;
				badaboom = true;
			}
			else if( actions.ToggleRanged.WasPressed )
			{
				hasRangeWeapon = !hasRangeWeapon;
			}
			else if( actions.Move.IsPressed && !hasRangeWeapon )
			{
				pressedKey.action = PlayerNetworkAction.MOVE;

				Vector3 dire = transform.position;
				dire.x += Time.deltaTime * settings.moveSpeed * actions.Move.X;
				dire.z += Time.deltaTime * settings.moveSpeed * actions.Move.Y;

				transform.position = dire;
				pressedKey.moveX = dire.x;
				pressedKey.moveY = dire.z;
				pressedKey.moveZ = dire.y;

				lastLookDirection = new Vector3( actions.Move.X, 0, actions.Move.Y );
				transform.rotation = Quaternion.LookRotation( lastLookDirection );
				pressedKey.rotateX = lastLookDirection.x;
				pressedKey.rotateZ = lastLookDirection.z;
			}
			else if( actions.DodoInteraction.WasPressed && !isGrabbing )
			{
				pressedKey.action = PlayerNetworkAction.DODO_INTERACTION;
				grabThis = true;
			}
			else if( isJumping )
			{
				pressedKey.action = PlayerNetworkAction.JUMPING;
			}

			pressedKey.hasRange = hasRangeWeapon;
			pressedKey.isGrabbing = grabThis;
			pressedKey.usingWeapon = badaboom;

			pendingMoves.Add(pressedKey);
			UpdatePredictedState();
			CmdExecute( pressedKey );
		}

		// synchronize state with the server
		SyncState( false );
	}

	void FixedUpdate()
	{
		// don't jump if: not local player, jump indicator is missing, range weapon is equipped
		if( isLocalPlayer && rayCastObject != null && !hasRangeWeapon )
		{
			RaycastHit hitJump;
			Physics.Raycast( new Ray( rayCastObject.transform.position, Vector3.down ), out hitJump );
			if( hitJump.distance > Mathf.Abs(rayCastObject.transform.localPosition.y) )
			{
				isJumping = true;
			}
			else
			{
				isJumping = false;
			}

			if( actions.Jump.WasPressed && !isJumping )
			{
				rb.AddForce( settings.jumpHeight * transform.up );
			}
		}
	}

	[Command(channel=0)] void CmdExecute(PlayerNetworkActionContainer action)
	{
		serverMove = DoAction(serverMove, action);
	}

	// create PlayerMove by using the info from the PlayerNetworkActionContainer
	PlayerMove DoAction( PlayerMove prev, PlayerNetworkActionContainer action )
	{
		// default: move nowhere
		Vector3 dire = Vector3.zero;
		Vector3 rot = Vector3.zero;

		if( action.action == PlayerNetworkAction.MOVE || isJumping )
		{
			// get position
			dire.x = action.moveX;
			dire.z = action.moveY;
			dire.y = action.moveZ;
			transform.position = dire;

		}
		else // no movement
		{
			dire = transform.position;
		}

		if( rot.x != -2 && rot.z != -2 )
		{
			rot.x = action.rotateX;
			rot.z = action.rotateZ;
			transform.rotation = Quaternion.LookRotation( rot, Vector3.up );
		}

		// this thing will go to the server!
		return new PlayerMove
		{
			moveNum = prev.moveNum + 1,
			currentPosition = dire,
			lookAtThis = rot,
			hasRangeEquipped = action.hasRange,
			useWeapon = action.usingWeapon,
			grabbing = action.isGrabbing
		};
	}
	#endregion
}
