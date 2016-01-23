using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider))]
public class InteractionTrigger : MonoBehaviour
{
	public string listeningTag = "Dodo";

	public bool CanGrabDodo { get; private set; }
	public GameObject DodoObject { get; private set; }

	void Start()
	{
		SphereCollider sphColl = GetComponent<SphereCollider>();
		if( sphColl == null )
		{
			Debug.Log("InteractionTrigger: Please add a sphere collider!");
			return;
		}

		sphColl.isTrigger = true;
	}

	void OnTriggerEnter ( Collider col )
	{
		if( col.gameObject.tag == listeningTag )
		{
			CanGrabDodo = true;
			DodoObject = col.gameObject;
		}
	}

	void OnTriggerExit( Collider col )
	{
		if( col.gameObject.tag == listeningTag )
		{
			CanGrabDodo = false;
		}
	}
}
