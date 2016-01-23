using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

struct CurrentSpawnStatus
{
	public bool occupied;
	public int teamMemberCount;
	public int score;
}
public class SpawnPoint : NetworkBehaviour
{
	public GameObject spawnHere;

	// current stats of the spawn point (gets synced with the server)
	[SyncVar(hook="OnStateChanged")]CurrentSpawnStatus currentState;

	/** Get the position where the player character should be located at the beginning of the level.
	 * If spawnHere is not set, Vector3.zero is returned.
	 */
	public Vector3 GetSpawnPosition() { if( spawnHere == null ) return Vector3.zero; return spawnHere.transform.position; }

	/** Indicator whether this spawn point is occupied by a team or not. */
	public bool HasTeamAssigned() { return currentState.occupied; }
	/** Call when team member scores. */
	public void Score()
	{
		currentState.score++;

		// TODO check if local player is assigned here or not
		if( isLocalPlayer )
			GlobalEventHandler.GetInstance().ThrowEvent( this, EEventType.PLAYER_SCORED, new PlayerScoredEventArgs() );
	}

	/** Flags this spawner as occupied. */
	public void AssignTeam()
	{
		// don't try to assign an occupied spawner
		if( currentState.occupied ) return;

		currentState.occupied = true;
	}

	void OnStateChanged( CurrentSpawnStatus cur )
	{
		currentState = cur;
	}

	/** Adds a new member to the team assigned to this spawn point. */
	public void AddTeamMember()
	{
		if( !currentState.occupied )
		{
			return;
		}
		currentState.teamMemberCount++;
	}

	/** Removes one team member. If no team members are left, the spawn point is
	 * set to unoccupied again.
	 */
	public void RemoveTeamMember()
	{
		currentState.teamMemberCount--;

		if( currentState.teamMemberCount <= 0 )
		{
			currentState.occupied = false;
		}
	}
}
