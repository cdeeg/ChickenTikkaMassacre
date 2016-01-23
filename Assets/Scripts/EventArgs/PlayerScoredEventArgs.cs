using UnityEngine;
using System.Collections;
using System;

public class PlayerScoredEventArgs : EventArgs
{
	public bool MyTeam { get; set; }

	public PlayerScoredEventArgs()
	{
		MyTeam = false;
	}

	public PlayerScoredEventArgs( bool isMyTeam )
	{
		MyTeam = isMyTeam;
	}
}
