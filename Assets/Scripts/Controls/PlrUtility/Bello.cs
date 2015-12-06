using UnityEngine;
using System.Collections;

public class Bello {

	static LayerMask lM_Ladder = 1 << 13;
	static LayerMask lM_Ground = 1 << 15;
	static LayerMask lM_PickUp = 1 << 16;
	static LayerMask lM_Weapon = 1 << 17;
	static LayerMask lM_Button = 1 << 22;
	static LayerMask lM_Water = 1 << 4;

	public enum ObjectLayer
	{
		Ground,
		Ladder,
		Ball,
		PickUp,
		Weapon,
		Button,
		Water
	}

	struct RayInfo
	{
		public Ray ray;
		public LayerMask lM;

		public RayInfo(Ray ray, LayerMask lM)
		{
			this.ray = ray;
			this.lM = lM;
		}
	}

	public LayerMask GetLayer(ObjectLayer oL)
	{
		return 				 (oL == ObjectLayer.Ground)		? lM_Ground
							: (oL == ObjectLayer.PickUp)	? lM_PickUp
							: (oL == ObjectLayer.Ladder)	? lM_Ladder
							: (oL == ObjectLayer.Weapon)	? lM_Weapon
							: (oL == ObjectLayer.Water)		? lM_Water
							: lM_Button;
	}

	Collider Sniffer(RayInfo whereToLook)
	{
		Collider c = null;

		if(whereToLook.lM == lM_Ladder || whereToLook.lM == lM_Ground || whereToLook.lM == lM_Water)
		{
			RaycastHit hit;
			Physics.Raycast(whereToLook.ray.origin, whereToLook.ray.direction, out hit, whereToLook.ray.direction.magnitude, whereToLook.lM  );
			c = hit.collider;

			if(whereToLook.lM == lM_Ladder)
			{
				if(c != null && hit.normal != -c.transform.right) c = null;
			}
		}
		else
		{
			Collider[] cc = Physics.OverlapSphere(whereToLook.ray.origin, whereToLook.ray.direction.magnitude, whereToLook.lM);
			if(cc.Length > 0) c = cc[0];

		}

		return c;
	}

	public Collider WhereIsThe_(ObjectLayer oL, Transform plrBody)
	{
		RayInfo whereToLook = Get_WhereToLook(oL, plrBody );
		
		Collider c = Sniffer(whereToLook);
		
		return c;
	}

	RayInfo Get_WhereToLook(ObjectLayer oL, Transform plrBody)
	{
		//RayInfo toReturn;
		Ray ray;
		LayerMask lM = GetLayer( oL );
		
		switch(oL)
		{
		case ObjectLayer.Ground:
		{
			Vector3 ori = plrBody.position;
			Vector3 dir = Vector3.down;
			float	mag = plrBody.GetComponent<CapsuleCollider>().height * 0.5f * plrBody.localScale.y + 0.1f;;
			ray = new Ray(ori, dir * mag);
			break;
		}
			
		case ObjectLayer.Ladder:
		{
			
			CapsuleCollider c = plrBody.GetComponent<CapsuleCollider>();
			
			Vector3 ori = plrBody.position + Vector3.down * (c.height * 0.5f * plrBody.localScale.y - 0.05f);;
			Vector3 dir = plrBody.forward;
			float	mag = (c.radius * plrBody.localScale.z + 1.05f);
			ray = new Ray(ori, dir * mag);
			break;
		}
			
		case ObjectLayer.PickUp:
		{
			
			Vector3 ori = plrBody.position + plrBody.forward * 1.0f;
			Vector3 dir = Vector3.forward;
			float	mag = 0.35f; //aka radius
			ray = new Ray(ori, dir * mag);
			break;
		}

		case ObjectLayer.Water:
		{
			Vector3 ori = plrBody.position;
			Vector3 dir = Vector3.down;
			float	mag = plrBody.GetComponent<CapsuleCollider>().height * 0.5f * plrBody.localScale.y + 0.1f;;
			ray = new Ray(ori, dir * mag);
			break;
		}	

		default:
		{
			
			Vector3 ori = plrBody.position;
			Vector3 dir = Vector3.down;
			float	mag = plrBody.GetComponent<CapsuleCollider>().height * 0.5f * plrBody.localScale.y + 0.1f;;
			ray = new Ray(ori, dir * mag);
			break;
		}
		}
		return new RayInfo(ray, lM);
	}


}
