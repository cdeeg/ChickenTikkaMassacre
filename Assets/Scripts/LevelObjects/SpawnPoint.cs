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
	[SyncVar]CurrentSpawnStatus currentState;
	
	/** Get the position where the player character should be located at the beginning of the level.
	 * If spawnHere is not set, Vector3.zero is returned.
	 */
	public Vector3 GetSpawnPosition() { if( spawnHere == null ) return Vector3.zero; return spawnHere.transform.position; }
	
	/** Indicator whether this spawn point is occupied by a team or not. */
	public bool HasTeamAssigned() { return currentState.occupied; }
	
	/** Flags this spawner as occupied. */
	public void AssignTeam()
	{
		// don't try to assign an occupied spawner
		if( currentState.occupied ) return;
		
		currentState.occupied = true;
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
	
	void Start()
	{
		currentState = new CurrentSpawnStatus { occupied = false, teamMemberCount = 0, score = 0 };
	}
}
