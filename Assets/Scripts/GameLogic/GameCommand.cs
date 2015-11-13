using UnityEngine;
using System.Collections;
using GameStats;
public static class GameCommand {

	public static GAME gameInstance;
	public static DodoControls Dodo;

	//Init

	public static void SetGameInstance(GAME g)
	{
		gameInstance = g;
		Dodo = new DodoControls();
		Create_Teams();
	}

	static void Create_Teams()
	{
		GameObject[] g = GameObject.FindGameObjectsWithTag("Basket");
		
		teamScore[] teams = new teamScore[g.Length];
		
		for (int i = 0; i < g.Length; i++)
		{
			TeamBasket baskets = g[i].transform.GetComponent<TeamBasket>();
			baskets.SetTeam( GameStatics.teams_colors[i] );
			
			teams[i] = new teamScore(baskets);
		}		
		
		GameStatics.teams = teams;		
	}
	//

	public static void RespawnPlayer(int plr_Id)
	{
		Player[] plrs = gameInstance.GetPlayers();
		plrs[plr_Id].body.position = GameStatics.PlayerSpawn[plr_Id];
	}

	public static void RespawnAllPlayers()
	{
		Player[] plrs = gameInstance.GetPlayers();

		for (int i = 0; i < plrs.Length; i++)
		{
			plrs[i].body.position = GameStatics.PlayerSpawn[i];
		}
	}

	public static void ScorePoint(Team t)
	{
		for (int i = 0; i < GameStatics.teams.Length; i++)
		{
			teamScore ts = GameStatics.teams[i];
			
			if( ts.team == t )
			{
				GameStatics.teams[i].addPoints = 1;
			}
			
		}
		GameStatics.dodo.ResetBall();
	}

}
