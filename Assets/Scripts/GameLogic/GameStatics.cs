using UnityEngine;
using System.Collections;
using GameStats;

public static class GameStatics{

	public static Vector3 dodo_spawn		= new Vector3( 19.5f, 5f, 0f);
	public static Vector3 playerSpawn_one	= new Vector3( 9.53f, -2f, 19.66f);
	public static Vector3 playerSpawn_two	= new Vector3( 10.75f, -2f, -19.66f);
	public static Team[] teams_colors = new Team[2]{Team.Blue, Team.Orange};

	public static teamScore[] teams;

	public static BallLogic dodo;
}
