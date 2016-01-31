//=================================================================================================================

using UnityEngine;
using System.Collections;

//=================================================================================================================

[RequireComponent(typeof(Light))]
public class LightFlicker : MonoBehaviour
{

	public float xamp;
	public float yamp;
	public float zamp;
	public float xfreq;
	public float yfreq;
	public float zfreq;

	//new Light light;
	Vector3 startPosLocal;

	void Start () 
	{
		//light 			= GetComponent<Light>();
		startPosLocal 	= transform.localPosition;
	}
	
	void Update()
	{

		float x = Mathf.Sin(xfreq * Time.time) * xamp;
		float y = Mathf.Sin(yfreq * Time.time) * yamp;
		float z = Mathf.Cos(zfreq * Time.time) * zamp;

		transform.localPosition = startPosLocal + new Vector3(x,y,z);
	}
}

//=================================================================================================================