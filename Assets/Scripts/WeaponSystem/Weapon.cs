using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public WeaponType type;

	[Header("General")]
	public int damage;
	public int ammo = 1;

	[Header("Range/Passive")]
	public WeaponProjectile projectile;
	public RangeWeaponBehaviour projectileBehaviour;
	public int curveMultiplier = 2;
	
	[Header("Passive Only")]
	public GameObject visualComponent;
	public bool hasProjectile = false;

	[Header("Cheats")]
	public bool infiniteAmmo = false;

	/** Returns true if the weapon still has ammo, false otherwise. */
	public bool UseAmmo() { if( infiniteAmmo ) return true; currentAmmo--; return currentAmmo > 0; }

	int currentAmmo;

	void Start()
	{
		if( type == WeaponType.LongRange || type == WeaponType.ShortRange )
		{
			if( projectile == null )
			{
				Debug.LogWarning("Range weapon found without projectile!");
			}
		}
		else if( type == WeaponType.Passive )
		{
			if( visualComponent == null )
			{
				Debug.LogWarning("Passive weapon without visual component found!");
			}
			if( hasProjectile && projectile == null )
			{
				Debug.LogWarning("Passive weapon that should have a projectile has no projectile prefab set!");
			}
		}

		if( ammo < 1 && !infiniteAmmo )
		{
			Debug.LogWarning("Weapon has no ammo and will be instantly scrapped.");
		}
	}

	public void Init()
	{
		currentAmmo = ammo;
	}

	public void Reset()
	{
		currentAmmo = ammo;
	}
}
