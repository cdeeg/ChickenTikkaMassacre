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
	//init_end

	//PLAYERS
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

	public static void HitPlayer(Vector3 contactPoint, int plr_ID)
	{
		gameInstance.GetPlayers()[plr_ID].GetSlapped(contactPoint);
	}

	public static void PlayerKnockOut(int plrID)
	{
		Debug.Log("Player KO: " + plrID);
		if (plrID == 0) gameInstance.pState_one = new KO(Time.time);
		else gameInstance.pState_two = new KO(Time.time);

		gameInstance.GetPlayers()[plrID].body.GetComponent<Rigidbody>().velocity *= 0.2f;

		GameObject effect = GameObject.Instantiate( Resources.Load<GameObject>("Effects/Ko_Indicator") );
		effect.transform.position = gameInstance.GetPlayers()[plrID].body.position + Vector3.up * 1;
		effect.transform.parent = gameInstance.GetPlayers()[plrID].body;
		GameObject.Destroy(effect, 2);

	}
	//player_end

	//GAME
	public static void ScorePoint(Team t)
	{
		for (int i = 0; i < GameStatics.teams.Length; i++)
		{
			teamScore ts = GameStatics.teams[i];
			
			if( ts.team == t )
			{
				GameStatics.teams[i].addPoints = 1;
				
				if(ts.team == Team.Orange)
				{
					gameInstance.StartCoroutine("GrillMeat", GameObject.FindGameObjectWithTag("Team 2 Meat"));					
				}
				else
				{
					gameInstance.StartCoroutine("GrillMeat", GameObject.FindGameObjectWithTag("Team 1 Meat"));
				}
				
			}
			
		}
		GameStatics.dodo.ResetBall();
	}
	//game_end
}
