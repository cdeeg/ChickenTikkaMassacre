using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Weapon : MonoBehaviour {

	public WeaponType type;

	[Header("General")]
	public int damage;
	public int ammo = 1;

	[Header("Melee")]
	public SphereCollider hitRange;

	[Header("Range/Passive")]
	public WeaponProjectile projectile;
	public Transform projectileSpawnPoint;
	public RangeWeaponBehaviour projectileBehaviour;
	public float projectileSpeed = 3f;
	public int curveMultiplier = 2;
	public RangeWeaponRefireTime shootAgainAfter;

	[Header("Passive Only")]
	public GameObject visualComponent;
	public bool hasProjectile = false;

	[Header("Other Settings")]
	public bool infiniteAmmo = false;

	/** Set by the server dynamically */
	public int WeaponID { get; set; }

	/** Returns true if the weapon still has ammo after shooting, false otherwise. */
	public bool UseAmmo() { if( infiniteAmmo ) return true; currentAmmo--; return currentAmmo > 0; }

	/** Returns the weapon's projectile if it has one, else null. Melee weapons don't have projectiles. */
	public GameObject GetProjectile() { return projectile.gameObject; }
	/** Returns the weapon's projectile if it has one, else null. Passive weapons only. */
	public GameObject GetVisualComponent() { return visualComponent; }
	/** Get the position at which the projectile should start. */
	public Vector3 GetProjectileSpawnPosition() { return projectileSpawnPoint.transform.position; }

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
		else // melee
		{
			if( hitRange == null )
			{
				Debug.LogWarning("Melee weapon without hit collider found! This weapon won't deal any damage!");
			}
			else
			{
				// make sure the collider is a trigger
				hitRange.isTrigger = true;
				// disable collider until the player uses the weapon
				hitRange.enabled = false;
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

	public void MeleeHit()
	{
		hitRange.enabled = true;
	}
}
