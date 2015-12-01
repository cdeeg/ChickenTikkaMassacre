using System.Collections;
using System;

public class Utilities
{
	static Utilities instance;

	public static Utilities GetInstance()
	{
		if(instance == null)
			instance = new Utilities();

		return instance;
	}

	private Utilities()
	{}
}


[Serializable]
public class PlayerCharacterSettings
{
	public float moveSpeed;
	public float ladderSpeed;
	public float respawnDelay;

	public PlayerCharacterSettings()
	{}
}

public enum WeaponType
{
	Melee,
	ShortRange,
	LongRange,
	Passive
}