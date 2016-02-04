using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider), typeof(Animation))]
public class VegetationRustle : MonoBehaviour
{
	Animation anim;

	void Awake()
	{
		anim = GetComponent<Animation>();
	}

	void OnTriggerStay(Collider other)
	{
		Rigidbody r = other.GetComponent<Rigidbody>();

		if(r != null && r.velocity.magnitude > 0.2f && !anim.isPlaying)
		{
			anim.Play();
		}
	}
}



