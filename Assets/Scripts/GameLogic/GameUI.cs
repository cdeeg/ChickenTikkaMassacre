using UnityEngine;
using System.Collections;
using GameStats;

public static class GameUI {

	public static void DrawScoreBoard()
	{
		//First UI mockUp -> move to UI script
		
		string score = "";
		bool first = true;
		foreach(teamScore ts in GameStatics.teams)
		{
			if(!first) score += " : ";
			first = false;
			score += "[ " + ts.currentPoints + " ]";
		}
		
		GUI.Box( new Rect(Screen.width * 0.5f - 150,Screen.height * 0.5f - 25, 300, 50), score);
	}
}
