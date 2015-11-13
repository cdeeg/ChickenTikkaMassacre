using UnityEngine;
using System.Collections;

namespace GameStats {

	public enum Team
	{
		Blue,
		Orange
	}

	public struct teamScore
	{
		public Team		team;
		int 			points;
		public int		addPoints
		{
			set
			{
				points += value;
			}
			get
			{ return points;}
		}

		public int		currentPoints
		{
			set{}
			get{return points;}
		}

		public TeamBasket basket;

		public teamScore(TeamBasket b)
		{
			this.team	=	b.GetTeam();
			points		=	0;

			this.basket = b;
		}
	}

	public struct GameStart_Info
	{

	}
}
