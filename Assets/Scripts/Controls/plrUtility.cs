using UnityEngine;
using System.Collections;

public static class plrUtility {

	static LayerMask lM_Ground = 1 << 15;
	static LayerMask lM_PickUp = 1 << 16;

	public static bool CheckForGround(Transform plrBody)
	{

		Vector3 	startingPoint 	= plrBody.position;
		Vector3 	vDirection		= Vector3.down;							//Change this if player needs to react to the curve of the world.
		float 		checkRange		= 1.45f;									//dependet on plr: height/2;

		if( Physics.Raycast(startingPoint, vDirection, checkRange, lM_Ground  ) )
		{
			return true;
		}
		return false;
	}

	public static Collider CheckForPickUp(Transform plrBody)
	{
		Vector3 startingPoint = plrBody.position + plrBody.forward * 1.0f;
		float sphereRadius = 0.35f;

		Collider[] c = Physics.OverlapSphere(startingPoint, sphereRadius, lM_PickUp);

		if(c.Length > 0) return c[0];		

		return null;
	}

	public static GameObject Create_UI_Button(Collider c)
	{
		GameObject btn = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
		btn.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);		
		
		btn.transform.position = c.transform.position + Vector3.up * 1.0f;
		btn.transform.rotation = Quaternion.Euler(0,180,0);
		Texture button = Resources.Load<Texture>("UI/BUTTON_Y");
		
		Renderer r = btn.GetComponent<Renderer>();
		r.material.shader = Shader.Find("Unlit/Transparent");
		r.material.SetTexture("_MainTex", button);

		return btn;
	}

	public static void UpdateButton(GameObject btn, Collider c)
	{
		btn.transform.position = c.transform.position + Vector3.up * 1.0f;
		btn.transform.rotation = Quaternion.Euler(0,180,0);
	}
}
