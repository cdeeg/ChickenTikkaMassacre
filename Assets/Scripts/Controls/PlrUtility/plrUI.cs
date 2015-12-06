using UnityEngine;
using System.Collections;

public class plrUI {

	public void Create_UI_Button(Collider c, Player plr)
	{
		//Update - destroy
		if(c == null)
		{
			if(plr.buttonDisplay_Y != null)
			{
				plr.buttonDisplay_Y.SetActive(false);
				return;
			}
			else return;//Do nothing
		}
		
		GameObject btn = plr.buttonDisplay_Y;
		
		btn.transform.position = c.transform.position + Vector3.up * 1.0f;
		btn.transform.rotation = Quaternion.Euler(0,180,0);
		
		Texture button = Resources.Load<Texture>("UI/BUTTON_Y");
		
		Renderer r = btn.GetComponent<Renderer>();
		r.material.shader = Shader.Find("Unlit/Transparent");
		r.material.SetTexture("_MainTex", button);
		
		plr.buttonDisplay_Y.SetActive(true);
		return;
	}
}
