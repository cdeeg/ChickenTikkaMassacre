using UnityEngine;
using System.Collections;

public class plrCombat : MonoBehaviour {

	public Collider[] CheckForHit(Player plr, bool DODEBUG = false)
	{
		
		float		radius = 1.1f;
		Vector3		center = plr.body.position + plr.body.forward *( plr.hitBox.bounds.extents.z * 0.5f * plr.body.localScale.z + radius + 0.25f); 
		LayerMask	lM = 1 << 17;
		Collider[] c = Physics.OverlapSphere(center, radius, lM);
		
		if(DODEBUG)
		{
			Debug.DrawLine(center + Vector3.left * radius * 0.5f, center + Vector3.right * radius * 0.5f, Color.yellow, 2.0f);
			Debug.DrawLine(center + Vector3.forward * radius * 0.5f, center + Vector3.back * radius * 0.5f, Color.yellow, 2.0f);
			Debug.DrawLine(center + Vector3.up * radius * 0.5f, center + Vector3.down * radius * 0.5f, Color.yellow, 2.0f);
		}
		
		//if(c != null) Debug.Log("got Something");
		return c;
	}

	public void Attack_Meele(Player plr)
	{
		plr.myAnimation.clip = plr.myAnimation.GetClip("slap");
		plr.myAnimation.Play("slap");
		
		Collider[] c = CheckForHit(plr);
		if(c.Length > 0) 
		{
			//Debug.Log("Player hit!");
			int enemyID = (plr.id == 0) ? 1 : 0;
			Debug.Log("HitPlayer: " + c[0].transform.name);
			GameCommand.HitPlayer( c[0].transform.position, enemyID);
		}
	}
}
