//=================================================================================================================

/*
 * 		DodoBehaviour.cs by Joris Drobka
 * 
 * 		Gameplay representation of the Dodo, manages the Mecanimn statemachine 
 *	 	on gamestate changes.
 * 
 * 		The states are:
 * 
 * 		- Idling: 			stand/look around, sometimes move to random place around current 
 * 							Pathfinder node. Has a % chance to change to 'MoveTo' state.
 * 							If a player is closer than AlertDistance, change to 'Flee' state.
 * 		- MoveTo:			Move to a randomly selected Pathfinder node, then start idling again
 * 							If a player is closer than AlertDistance, change to 'Flee' state
 * 		- Falling:			Dodo falls. If it has a target node from a previous MoveTo state,
 * 							the dodo will try to fly to it, and if grounded again, will continue to move.
 * 		- Flee:				Move away from a player until a certain safety distance is reached. 
 * 							Works exactly like MoveTo, but without random selection of target not
 * 							and an aborting condition as soon as safety distance is achieved.
 * 							Safety distance must be greater than AlertDistance.
 * 		- Captured:			After a certain amount of time, the Dodo will try to free itself ('FreeingAttempt' state).
 * 							If the player loses the Dodo, it changes to 'Flee' state
 * 		- EscapeAttempt:	If captured, the dodo tries to free itself from the players' clutches.
 * 							When timed correctly, the player may suppress this attempt, 
 * 							forcing the dodo back to 'Captured' state. Otherwise, the Dodo will enter 'Flee' again.
 * 		- Bouncing:			If the dodo was released due to a weapon attack etc, it will retain
 * 							its momentum for a second and bounce around before idling again.
 * 
 * 
 * 		TODO:
 * 
 * 		√ states
 * 		√ grounded tests
 * 		√ die & respawn
 * 		• create spawning solution
 * 		√ choose random position around current pos
 * 		√ choose random node to move to
 * 		• path optimization (on-the-fly)
 * 		• 
 * 
 */

//=================================================================================================================

using UnityEngine;
using System.Collections;
using JPathfinder;
using System.Collections.Generic;

//=================================================================================================================

namespace jChikken
{

	public class DodoBehaviour : MonoBehaviour, IKillable
	{
		#region constants

		/// <summary>
		/// minimum height difference between two platform levels in the environment
		/// </summary>
		public const float _minHeightStep = 2f;

		const float rotationSpeed = 0.3f;

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region def
		
		public enum alertMode 
		{
			byDistance, 
			byDistanceAndSight
		}



		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region fields

		//	settings

		[Tooltip("movement speed")]
		public float speed = 5f;

		[Tooltip("how fast the dodo will be at maximum speed")]
		public float acceleration;

		[Tooltip("Determines how the dodo gets alerted from a player")]
		public alertMode AlertBehaviour;

		[Range(0f, 10f), 
		 Tooltip("Min distance to a player before " +
				 "Dodo will attempt to flee. Must be smaller than SafetyDistance")]
		public float AlertDistance = 4f;

		[Range(0f, 9.5f), 
		 Tooltip("Min distance to players before Dodo aborts a flee attempt." +
		         "Must be greater than AlertDistance;")]
		public float SafetyDistance = 5f;

		[Range(0f, 5f), Tooltip("changes at which height difference the dodo will not be aware of a player anymore")]
		public float heightAwarenessDist = 2f;

		[Range(0, 360), Tooltip("field of view for dodo in degrees")]
		public float fieldOfView = 180;

		[Tooltip("Time before Dodo respawn if it should die or a player scores.")]
		public float RespawnTime = 3f;

		[Tooltip("Radius in which the Dodo may move randomly while idling.")]
		public float idlingRadius = 1.5f;

		[Range(0f, 1f), Tooltip("Chance per second for the dodo to choose a place to go to")]
		public float relocateChance = 0.05f;

		[Range(0f, 15f), Tooltip("Minimum time before Dodo tries to escape when captured")]
		public float escapeMinDelay = 8f;

		[Range(1f, 5f), Tooltip("Interval at which the Dodo will attempt an escape when captured")]
		public float escapeAttemptInterval = 1f;

		[Range(0f, 1f), Tooltip("Chance of the Dodo trying to escape")]
		public float escapeAttemptChance = 0.1f;
		
		//-----------------------------------------------------------------------------------------------------------------

		public Rigidbody	mRigidBody;
		public Animator 	mController	{ get; private set; }
		private DodoSpawner	mSpawner;
		private SphereCollider mCollider;

		private float localRot;

		private Vector3 position { 	get { return transform.position; } }

		//-----------------------------------------------------------------------------------------------------------------

		//	pathing

		public Path currentPath { get; private set; }
		public bool hasPath		{ get { return currentPath != null; } }

		public Vector3 currVelocity { get; set; }

		//-----------------------------------------------------------------------------------------------------------------

		//	state values

		public state lastState		{ get; set; }
		public state currentState	{ get; set; }

		float distToGround;

		/// <summary>
		/// true if Dodo is on ground
		/// </summary>
		bool isGrounded;
		/// <summary>
		/// true if the dodo has spawned on the field.
		/// </summary>
		public bool hasSpawned { get; private set; }
		/// <summary>
		/// true if dodo was cooked or otherwise
		/// </summary>
		bool isDead;
		/// <summary>
		/// true if dodo was fetched by a player
		/// </summary>
		public bool isCaptured { get { return captor != null; } }
		/// <summary>
		/// player that is currently holding this dodo
		/// </summary>
		Player captor;
		/// <summary>
		/// player to currently flee from
		/// </summary>
		private Player fleeFrom;
		/// <summary>
		/// get from collider to determine if character is on ground
		/// </summary>
		float ownHeight;
		float maxDistToGround;

		#endregion

		//=================================================================================================================
		
		#region init
		
		void Awake() 
		{
			hasSpawned = true;
			mController = transform.GetComponentInChildren<Animator>();
			mSpawner 	= GameObject.FindObjectOfType<DodoSpawner>();
			mCollider 	= GetComponent<SphereCollider>();
			
			if(mRigidBody == null)
				mRigidBody = GetComponent<Rigidbody>();
			mRigidBody.isKinematic = true;
			mRigidBody.useGravity = true;
			
			ownHeight = mCollider.radius * 2;
			maxDistToGround = (ownHeight / 2) + 0.1f;
		}
		
		#endregion

		//=================================================================================================================
		
		#region interface

		public string name { get { return name; } }

		/// <summary>
		/// call this to spawn the dodo
		/// </summary>
		public void Kill()
		{
			if(hasSpawned)
			{
				Debug.Log("on kill dodo");
				isDead = true;
				mController.SetTrigger("die");
				hasSpawned = false;
				StartCoroutine("waitForRespawn");
			}
		}

		public void OnSpawn(Vector3 p)
		{
			if(isDead)
			{
				Debug.Log("dodo onSpawn");
				transform.position = p;
				isDead = false;
				hasSpawned = true;
			}
		}

		//-----------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// call this to force the 'Captured' state upon the dodo
		/// </summary>
		public void GetCaptured(Player player)
		{
			captor = player;
			mCollider.enabled = false;
			UpdateStateValues();
		}

		public void FreeDodo()
		{
			mCollider.enabled = true;
			captor = null;
			UpdateStateValues();
		}

		//-----------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// movement method with simple acceleration model. Can be used from 
		/// different animator states as a movement update method.
		/// </summary>
		public bool MoveTowards(Vector3 target)
		{
			Vector3 v = target - position;

			currVelocity += (v.normalized * acceleration) * Time.deltaTime;

			Vector3 velLeveled = new Vector3(currVelocity.x, 0, currVelocity.z);
			if(velLeveled.magnitude > speed)
			{
				currVelocity = velLeveled.normalized * speed + Vector3.down * currVelocity.y;
			}

			if(isGrounded)
				currVelocity *= 0.8f;	//	friction


			//	translation
			ApplyGravity();
			Vector3 translation = currVelocity * Time.deltaTime;
			Vector3 nextPos = transform.position + translation;



			if(Vector3.Distance(position, target) < Vector3.Distance(position, nextPos))
			{
				//	reached target
				transform.position = target;
				return true;
			}
			else
			{
				transform.position = nextPos;
				return false;
			}
		}

		/// <summary>
		/// just apply current velocity; make sure to use either MoveTowards() or this if you want 
		/// to move the dodo, never both. 
		/// </summary>
		public void ApplyVelocity()
		{
			transform.position += currVelocity * Time.deltaTime;
		}

		/// <summary>
		/// applies the gravity force to current velocity
		/// </summary>
		public void ApplyGravity()
		{
			float g = Mathf.Clamp(currVelocity.y - (4f * Time.deltaTime), (isGrounded ? -1.5f : -16f), 100f);

			//	prevent falling through collider
			if(g * Time.deltaTime > distToGround)
				g = distToGround;

			currVelocity = new Vector3(currVelocity.x, g, currVelocity.z);
		}

		/// <summary>
		/// nulls current velocity & clears current path, resulting in an instant stop
		/// </summary>
		public void StopMovement()
		{
			currVelocity = Vector3.zero;
			ClearCurrentPath();
		}

		public void ClearCurrentPath()
		{
			currentPath = null;
		}

		#endregion

		//=================================================================================================================

		#region behaviour
		
		void Update()
		{
			if(isDead)
			{
				ApplyGravity();
				ApplyVelocity();
			}
			else if(!isGrounded)
			{
				GroundStuff();
				UpdateStateValues();
			}
		}

		void FixedUpdate()
		{
			GroundStuff();
			
			//	copy velocity from player if dodo is captured
			if(isCaptured)
			{
				currVelocity = captor.mController.currVelocity;
			}

			Vector3 velLeveled = new Vector3(currVelocity.x, 0, currVelocity.z);
			if(!isDead && !isCaptured && velLeveled.magnitude > 0.01f)
			{
				if(currentState == state.idle)
				{
					currVelocity *= 0.95f;
				}

				localRot += velLeveled.magnitude * 5;
				Quaternion forward = Quaternion.LookRotation(velLeveled.normalized, Vector3.up);
				Quaternion rot = Quaternion.AngleAxis(localRot, Vector3.right);

				transform.rotation = forward * rot;
			}
			
			UpdateStateValues();
		}

		void GroundStuff()
		{
			Vector3 groundP;
			isGrounded = Groundtest(position, out groundP);
			if(isGrounded)
			{
				float d = position.y - groundP.y;
				if(d > 0)
					transform.position = new Vector3(transform.position.x, groundP.y + maxDistToGround-0.15f, transform.position.z);
			}
			
			//			distToGround = GetDistanceToGround(out groundP);
			//			isGrounded = distToGround <= maxDistToGround;
		}

		//-----------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// updates animatorcontroller parameters
		/// </summary>
		void UpdateStateValues()
		{
			mController.SetBool("isGrounded", isGrounded);
			mController.SetBool("hasPath", hasPath);
			mController.SetBool("seePlayer", fleeFrom != null);
			mController.SetBool("isDead", isDead);
			mController.SetBool("isCaptured", isCaptured);
			mController.SetFloat("momentum", currVelocity.magnitude);
			mController.SetFloat("yVel", currVelocity.y);
		}

		//-----------------------------------------------------------------------------------------------------------------

		IEnumerator waitForRespawn()
		{
			Debug.Log("dodo start waitforrespawn:: " + RespawnTime);
			yield return new WaitForSeconds(RespawnTime);

			Debug.Log("dodo waitforrespawn:: " + GameManager.GameIsRunning());
			if(GameManager.GameIsRunning())
			{
				mSpawner.Respawn();
			}
		}

		#endregion

		//=================================================================================================================

		#region util

		static Vector3 NULLPOS = Vector3.one * -100;

		/// <summary>
		/// returns true if dodo is on ground
		/// </summary>
		public float GetDistanceToGround(out Vector3 point)
		{
			return GetDistanceToGround(position, out point);
		}

		private float GetDistanceToGround(Vector3 p, out Vector3 point)
		{
			RaycastHit hit;
			if( Physics.Raycast(p, Vector3.down, out hit, LayerMask.GetMask("Ground")) )
			{
				point = hit.point;
				return Vector3.Distance(p, hit.point);
			}
			point = Vector3.one * -100;
			return 1000f;
		}

		private bool Groundtest(Vector3 p, out Vector3 point)
		{
			point = NULLPOS;
			RaycastHit hit;
			Debug.DrawLine(p + Vector3.up * 0.1f, p + Vector3.down * (maxDistToGround+0.1f) );
			if(Physics.Raycast(p + Vector3.up*0.1f, Vector3.down, out hit, maxDistToGround + 0.1f, LayerMask.GetMask("Ground")))
			{
				point = hit.point;
				return true;
			}
			return false;
		}

		//-----------------------------------------------------------------------------------------------------------------

		/// <summary>
		/// returns a valid new random position around the dodo's position.
		/// </summary>
		public Vector3 GetNewRandomPositionAroundMe()
		{
			Vector3 newP = transform.position;
			bool isValidP = false;
			int att = 100;
			while( att >= 0 && !isValidP )
			{
				float x = Random.Range(-idlingRadius, idlingRadius);
				float z = Random.Range(-idlingRadius, idlingRadius);

				Vector3 groundP;
				float d = GetDistanceToGround(new Vector3(x, transform.position.y+10, z), out groundP);

				isValidP = Mathf.Abs(groundP.y - transform.position.y) <= _minHeightStep;

				if(isValidP)
					newP = groundP;

				att--;
			}
			return att > 0 ? newP : transform.position;
		}

		/// <summary>
		/// create a new random path to a node near the dodo's spawn point
		/// </summary>
		public void GenerateRandomPathHome()
		{
			Node n = mSpawner.GetRandomNodeInSpawnArea();
			if(n != null)
			{
				Path p = PathFinder.GetPath(position, n.pos);
				if(p.isValid())
				{
					currentPath = p;
					currentPath.StartPathMove();
				}
			}
		}

		public void GenerateRandomPathExplore()
		{
			Node n = mSpawner.GetRandomNodeInExploreArea();
			if(n != null)
			{
				Path p = PathFinder.GetPath(position, n.pos);
				if(p.isValid())
				{
					currentPath = p;
					currentPath.StartPathMove();
				}
			}
		}

		public void GenerateSmallIdlePath()
		{
			Node n = PathFinder.GetNextNodeRandom(position);
			if(n != null)
			{
				Path p = PathFinder.GetPath(position, n.pos);
				if(p.isValid())
				{
					currentPath = p;
					currentPath.StartPathMove();
				}
			}
		}

		/// <summary>
		/// Generate a path that leads away from given position while checking if the general direction of
		/// the path doesn't lead into the other's players arms
		/// </summary>
		/// <param name="fleeFrom">Flee from.</param>
		public void GenerateNewFleeingPath(Vector3 fleeFrom)
		{

		}

		//-----------------------------------------------------------------------------------------------------------------

		private Player CheckForPlayersNearby(Vector3 testP)
		{
			if(AlertTest(GameManager.GetPlayerA(), testP))
				return GameManager.GetPlayerA();
			else if(AlertTest(GameManager.GetPlayerB(), testP))
				return GameManager.GetPlayerB();
			else
				return null;
		}

		/// <summary>
		/// returns true if the dodo reacts to a player
		/// </summary>
		private bool AlertTest(Player player, Vector3 testP)
		{
			Vector3 v = player.transform.position - testP;
			float d = v.magnitude;

			//	height difference test
			if(v.y > heightAwarenessDist)
			{
				return false;
			}

			//	distance & viewing angle tests
			float vDot = Vector3.Dot(transform.forward, new Vector3(v.x, 0, v.z).normalized);
			float fovMin = jUtility.Math.MapF(fieldOfView/2, 0, 180, -1, 1);


			switch(AlertBehaviour)
			{
			case alertMode.byDistance:				return d < AlertDistance;
			case alertMode.byDistanceAndSight:		return d < AlertDistance && vDot >= fovMin;
			default:								return false;
			}
		}

		#endregion

		//=================================================================================================================

		#region debug

		void OnDrawGizmos()
		{
			if(hasPath)
			{
				Vector3 off = Vector3.up * ownHeight / 2;
				Gizmos.color = Color.Lerp(Color.red, Color.yellow, 0.4f);
				Gizmos.DrawLine(position, currentPath.currNode + off);
				for(int i = currentPath.currIndex; i < currentPath.nodeCount-1; i++)
				{
					Gizmos.DrawLine(currentPath.GetNode(i) + off, currentPath.GetNode(i+1) + off);
				}
			}
		}

		#endregion
	}

}

//=================================================================================================================





