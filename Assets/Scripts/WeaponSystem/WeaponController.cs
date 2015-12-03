using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour {

	public Weapon standardMelee;
	public Weapon standardRange;

	public Weapon[] weapons;
	
	Weapon currentMeleeWeapon;
	Weapon currentRangeWeapon;
	Weapon currentWeapon;

	public int GetDamage() { return currentWeapon.damage; }
	public bool HasRangeWeaponEquipped() { return currentWeapon.type != WeaponType.Melee; }

	public void SetWeapon( Weapon w )
	{
		if( w.type == WeaponType.Melee )
		{
			currentMeleeWeapon = w;
		}
		else
		{
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

		return change;
	}

	/** Equip melee weapon. Returns true if the current weapon changed, false if nothing changed. */
	public bool EquipMelee()
	{
		bool change = currentWeapon.type != WeaponType.Melee;
		currentWeapon = currentMeleeWeapon;

		return change;
	}

	/** Shoot current range weapon. Returns true if the weapon still has ammo left, false if it's empty (the melee weapon will be
	 * equipped automatically).
	  */
	public bool ShootRangedWeapon()
	{
		bool hasAmmo = currentRangeWeapon.UseAmmo();

		if( !hasAmmo )
		{
			currentRangeWeapon = standardRange;
			EquipMelee();
		}

		return hasAmmo;
	}

	void Start ()
	{
		if( standardMelee == null && standardRange == null )
		{
			Debug.LogError("WeaponController: No standard weapons found!");
			return;
		}

		currentRangeWeapon = standardRange;
		currentMeleeWeapon = standardMelee;

		currentWeapon = standardMelee;
		Debug.Log("current weapon"+currentWeapon);
	}
}
