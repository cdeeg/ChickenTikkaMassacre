using UnityEngine;
using System.Collections;
using GameStats;

public static class GameStatics{

	public static Vector3 dodo_spawn		= new Vector3( 13.0f, 8.0f, 0.0f);
	public static Vector3 playerSpawn_one	= new Vector3( 8.0f, 9.0f, -16.0f );
	public static Vector3 playerSpawn_two	= new Vector3( 8.0f, 9f, 16.0f  );

	public static Vector3[] PlayerSpawn = new Vector3[2]{playerSpawn_one, playerSpawn_two};

	public static Team[] teams_colors = new Team[2]{Team.Blue, Team.Orange};

	public static teamScore[] teams;

	public static BallLogic dodo;
}
