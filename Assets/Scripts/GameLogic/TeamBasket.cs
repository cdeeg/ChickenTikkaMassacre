using UnityEngine;
using System.Collections;
using GameStats;

public class TeamBasket : MonoBehaviour {

	Team myTeam;
	public int myScore = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetTeam(Team team)
	{
		myTeam = team;
	}

	public Team GetTeam()
	{
		return myTeam;
	}
}
