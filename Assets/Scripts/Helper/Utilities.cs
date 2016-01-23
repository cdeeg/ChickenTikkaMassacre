using System.Collections;
using System;
using UnityEngine;

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
	public float jumpHeight;
	public float jumpMoveSlowdown;
	public bool allowJumpingWithWeapon;

	public PlayerCharacterSettings()
	{
		health = 10;
		jumpHeight = 3.0f;
		jumpMoveSlowdown = 0.5f;
		allowJumpingWithWeapon = false;
	}
}

[Serializable]
public class CameraControlSettings
{
	[Header("General")]
	public float minHeight;
	public float maxHeight;
	public float cameraMovementSpeed;
	public bool autofollowWhenNecessary;
	[Header("Controls")]
	public float coneDiameterLower;
	public float coneDiameterUpper;
	public bool invertYAxis;
	public bool invertXAxis;

	public CameraControlSettings()
	{
		minHeight = 0f;
		maxHeight = 10f;
		coneDiameterLower = 5f;
		coneDiameterUpper = 10f;
		invertYAxis = false;
		invertXAxis = false;
		autofollowWhenNecessary = true;
	}
}

public enum CameraBehaviour
{
	StraightLine,
	Cone
}


public enum PlayerNetworkAction
{
	NONE,
	USE_WEAPON,
	TOGGLE_RANGED,
	MOVE,
	JUMPING,
	DODO_INTERACTION
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

public enum RangeWeaponRefireTime
{
	AnimationIsDone,
	ProjectileImpact
}