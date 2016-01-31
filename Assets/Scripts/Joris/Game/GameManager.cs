using UnityEngine;
using System.Collections;

//=================================================================================================================

namespace jChikken
{

	public class GameManager : MonoBehaviour 
	{
		#region singleton

		private static GameManager instance
		{
			get { 
				if(_instance == null)
				{
					_instance = new GameObject("GameManager").AddComponent<GameManager>();
					_instance.Setup();
				}
				return _instance;
			}
		}
		private static GameManager _instance;

		#endregion

		//=================================================================================================================

		#region fields

		private Player playerA;
		private Player playerB;
		private bool   gameIsRunning;

		#endregion

		//=================================================================================================================

		#region interface

		public static bool GameIsRunning()
		{
			return instance.gameIsRunning;
		}

		public static Player GetPlayerA() { return instance.playerA; }
		public static Player GetPlayerB() { return instance.playerB; }
		public static Player GetOpponent(Player player)
		{
			return player == instance.playerA ? instance.playerB : instance.playerA;
		}

		#endregion

		//=================================================================================================================

		#region init

		bool isInitialized=false;

		void Start()
		{
			Setup();
		}

		void Setup()
		{
			if(!isInitialized)
			{
				Player[] players = GameObject.FindObjectsOfType<Player>();
				if(players.Length == 2)
				{
					playerA = players[0];
					playerB = players[1];
					isInitialized = true;
				}
				else
				{
					throw new System.Exception("there must be exactly two players in the scene!");
				}
			}
		}

		#endregion
	}

}

//=================================================================================================================