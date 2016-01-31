using UnityEngine;
using System.Collections;

public class UV_Scrolling : MonoBehaviour {

	public Material sharedMat;

	//scroll main texture based on time
	
	float scrollSpeed = 0.5f;
	float offset;
	//float rotate;

	//Renderer mRenderer;
	
	void Update ()
	{
		if (sharedMat != null) {
			offset += (Time.deltaTime * scrollSpeed) / 12.0f;
			sharedMat.SetTextureOffset ("_MainTex", new Vector2 (0, offset));
		}
	} 

}
