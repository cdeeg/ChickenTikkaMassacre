using UnityEngine;
using System.Collections;

public class WeaponTrigger : MonoBehaviour
{
	int damage = 1;		// standard damage is one (in case something skips the Init)
	bool canDoDamage;	// to avoid double or triple damage due to certain collider weirdness

	public void Init( int dmg )
	{
		canDoDamage = true;
		damage = dmg;
	}

	void OnTriggerEnter( Collider coll )
	{
		if( coll.gameObject.tag == "Player" && canDoDamage )
		{
			canDoDamage = false;
			// do shit
			SimpleCharacterController chara = coll.gameObject.GetComponent<SimpleCharacterController>();
			if( chara == null )
			{
				Debug.LogError("WeaponTrigger: Player object found without SimpleCharacterController script attached!");
				return;
			}
			chara.ApplyDamage( damage );
		}
	}

	public void CanDealDamageAgain()
	{
		canDoDamage = true;
	}
}
