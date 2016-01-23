﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class WeaponController : NetworkBehaviour
{
	public Weapon standardMelee;
	public Weapon standardRange;

	Transform weaponAnchor;

	Weapon standardRangeInstance;
	Weapon standardMeleeInstance;

	Weapon currentRangeInstance;
	Weapon currentMeleeInstance;

	Weapon currentMeleeWeapon;
	Weapon currentRangeWeapon;
	Weapon currentWeapon;

	WeaponObjectPool pool;
	public WeaponProjectile activeProjectile;

	public int GetDamage() { return currentWeapon.damage; }
	public bool HasRangeWeaponEquipped() { return currentWeapon.type != WeaponType.Melee; }

	public void SetWeapon( Weapon w )
	{
		if( w.type == WeaponType.Melee )
		{
			if( pool != null )
			{
				// add old instance to object pool
				pool.AddToPool( currentMeleeInstance );
				// try to get existing instance of this weapon
				Weapon insta = pool.GetWeaponInstanceByID( w.WeaponID );
				if( insta != null )
				{
					// found instance -> attach to weaponAnchor and set reference
					insta.transform.position = weaponAnchor.position;
					insta.transform.SetParent( weaponAnchor );
					currentMeleeInstance = insta;
				}
				else
				{
					// no existing instance -> create one
					GameObject instance = (GameObject)Instantiate(w.gameObject, weaponAnchor.position, weaponAnchor.rotation);
					currentMeleeInstance = instance.GetComponent<Weapon>();
					currentMeleeInstance.transform.SetParent( weaponAnchor );
				}
			}
			currentMeleeWeapon = w;
		}
		else
		{
			if( pool != null )
			{
				// add old instance to object pool
				pool.AddToPool( currentRangeInstance );
				// try to get existing instance of this weapon
				Weapon insta = pool.GetWeaponInstanceByID( w.WeaponID );
				if( insta != null )
				{
					// found instance -> attach to weaponAnchor and set reference
					insta.transform.position = weaponAnchor.position;
					insta.transform.SetParent( weaponAnchor );
					currentRangeInstance = insta;
				}
				else
				{
					// no existing instance -> create one
					GameObject instance = (GameObject)Instantiate(w.gameObject, weaponAnchor.position, weaponAnchor.rotation);
					currentRangeInstance = instance.GetComponent<Weapon>();
					currentRangeInstance.transform.SetParent( weaponAnchor );
				}
			}
			currentRangeWeapon = w;
		}
	}

	public bool ToggleRanged()
	{
		if( currentWeapon.type != WeaponType.Melee )
			return EquipMelee();
		else
			return EquipRange();
	}

	public void Purge()
	{
		currentMeleeWeapon = standardRange;
		currentRangeWeapon = standardMelee;

		EquipMelee();
	}

	/** Equip range weapon */
	public bool EquipRange()
	{
		bool change = currentWeapon.type == WeaponType.Melee;

		currentWeapon = currentRangeWeapon;

		if( change )
		{
			currentMeleeInstance.gameObject.SetActive( false );
			currentRangeInstance.gameObject.SetActive( true );
		}

		return change;
	}

	/** Equip melee weapon. Returns true if the current weapon changed, false if nothing changed. */
	public bool EquipMelee( bool alwaysReturnTrue = false )
	{
		bool change = currentWeapon.type != WeaponType.Melee;
		currentWeapon = currentMeleeWeapon;

		if( change )
		{
			currentRangeInstance.gameObject.SetActive( false );
			currentMeleeInstance.gameObject.SetActive( true );
		}

		return change;
	}

	/** Shoot current range weapon. Returns true if the weapon still has ammo left, false if it's empty (the melee weapon will be
	 *  equipped automatically).
	 */
	public bool UseWeapon()
	{
		if( currentWeapon.type != WeaponType.Melee )
		{
			if( activeProjectile != null ) return true; // current projectile limit: 1

			// TODO passive: init visual rep here and let visual rep decide when to init proj (fkn timing shit bullshittery)
			WeaponProjectile proj = pool.GetProjectileInstanceForWeapon( currentWeapon.WeaponID );
			if( proj == null )
			{
				GameObject obj = currentWeapon.GetProjectile();
				obj = (GameObject)Instantiate( obj, Vector3.zero, Quaternion.identity );
				proj = obj.GetComponent<WeaponProjectile>();
			}
			proj.transform.position = transform.TransformPoint( currentWeapon.GetProjectileSpawnPosition() );
			proj.Initialize( proj.transform.position, new Vector3(4f, 0f, -4f), currentWeapon.projectileSpeed, currentWeapon.WeaponID, this );
			activeProjectile = proj;
		}

		bool hasAmmo = currentWeapon.UseAmmo();

		if( !hasAmmo )
		{
			if( currentWeapon.type != WeaponType.Melee )
			{
				currentRangeWeapon = standardRange;
				currentRangeInstance = standardRangeInstance;
			}
			else
			{
				currentMeleeWeapon = standardMelee;
				currentMeleeInstance = standardMeleeInstance;
			}
		}

		return hasAmmo;
	}

	public void ProjectileDone()
	{
		if( activeProjectile == null ) { Debug.Log("FAIL"); return; }

		Debug.Log("ADD TO POOL");
		pool.AddProjectileToPool( activeProjectile );
		activeProjectile = null;
	}

	public void Initialize (Transform t, WeaponObjectPool wop)
	{
		if( standardMelee == null && standardRange == null )
		{
			Debug.LogError("WeaponController: No standard weapons found!");
			return;
		}

		pool = wop;

		currentRangeWeapon = standardRange;
		currentMeleeWeapon = standardMelee;

		currentWeapon = standardMelee;
		CreateStandardWeapons( t );
		weaponAnchor = t;
	}

	public void CreateStandardWeapons(Transform t)
	{
		GameObject insta = (GameObject)Instantiate(standardRange.gameObject, t.position, Quaternion.identity);
		standardRangeInstance = insta.GetComponent<Weapon>();
		standardRangeInstance.WeaponID = 1;
		standardRangeInstance.gameObject.SetActive( false );
		standardRangeInstance.transform.SetParent( t );
		currentRangeInstance = standardRangeInstance;

		insta = (GameObject)Instantiate(standardMelee.gameObject, t.position, Quaternion.identity);
		standardMeleeInstance = insta.GetComponent<Weapon>();
		standardMeleeInstance.WeaponID = 0;
		standardMeleeInstance.transform.SetParent( t );
		currentMeleeInstance = standardMeleeInstance;
	}
}
