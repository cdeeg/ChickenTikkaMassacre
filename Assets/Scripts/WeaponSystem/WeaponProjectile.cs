using UnityEngine;
using System.Collections;

struct ProjectileState
{
	public Vector3 position;
	public bool impact;
}
	
public class WeaponProjectile : MonoBehaviour
{
	// TODO use NetworkBehaviour
	public GameObject visualRepresentation;

	ParticleSystem impactParticles;
	Vector3 startPosition;
	Vector3 targetPosition;
	WeaponController myController;
	float flyingSpeed;
	bool didImpact;
	bool done;
	int myWeaponId;
	float passedTime;

	public int GetWeaponId() { return myWeaponId; }

	public void Initialize( Vector3 start, Vector3 target, float speed, int weaponId, WeaponController watcher, bool usePhysics = false )
	{
		if( visualRepresentation != null ) visualRepresentation.SetActive( true );

		passedTime = 0.0f;
		if( impactParticles == null )
			impactParticles = GetComponent<ParticleSystem>();
		if( impactParticles != null )
		{
			impactParticles.loop = false; // don't loop that shit
		}
		else Debug.Log("WeaponProjectile: No impact particle effect found!");

		startPosition = start;
		targetPosition = target;
		flyingSpeed = speed;
		myWeaponId = weaponId;
		myController = watcher;
		done = false;
		passedTime = 0.0f;
	}
	
	void Update ()
	{
		if( didImpact )
		{
			// check if the impact particle effect is still playing
			if( ( impactParticles == null || !impactParticles.IsAlive() ) && !done)
			{
				impactParticles.Stop();
				// it's done? send it to the object pool for reuse
				myController.ProjectileDone();
				done = true;
			}

			// don't do anything else, this projectile is done with its business
			return;
		}

		passedTime += Time.deltaTime;
		transform.position = Vector3.Lerp( transform.position, targetPosition, flyingSpeed * passedTime );

		if( !didImpact && Vector3.Distance( transform.position, targetPosition ) < 0.1f )
		{
			if( impactParticles != null )
			{
				impactParticles.Play();
			}
			if( visualRepresentation != null ) visualRepresentation.SetActive( false );
			didImpact = true;
		}
	}

	public void Reset()
	{
		if( visualRepresentation != null ) visualRepresentation.SetActive( false );
		didImpact = false;
		done = false;
		passedTime = 0.0f;
	}
}
