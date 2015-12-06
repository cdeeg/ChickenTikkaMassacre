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

	private Utilities() {}
}


[Serializable]
public class PlayerCharacterSettings
{
	public int health;
	public float moveSpeed;
	public float ladderSpeed;
	public float respawnDelay;
	public float rotationSpeed;

	public PlayerCharacterSettings()
	{
		health = 10;
		rotationSpeed = 3.0f;
	}
}

public enum WeaponType
{
	Melee,
	ShortRange,
	LongRange,
	Passive
}

public enum RangeWeaponBehaviour
{
	Line,
	Curve
}