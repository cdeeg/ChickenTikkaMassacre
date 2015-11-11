using UnityEngine;
using System.Collections;

public class BallLogic : MonoBehaviour {

	bool isAsleep = true;
	
	bool isStartingToGetJiggly = false;

	float jiggleTime = 0;
	float jiggleCD = Random.value * 3 + 2;
	float asSleep = 0;
	float asSleeCD = Random.value * 3 + 2;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		CheckIfFallenOfLevel();

		CheckForBehaviour();
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log("Jimmies rustled");

		switch(other.tag)
		{
			case "Basket":
			{
				//calculate team -> point
				GAME.ScorePoint( other.GetComponent<TeamBasket>().GetTeam() );
				break;
			}
			case"Weapon":
			{
				//calculate dmg -> k.o.
				break;
			}
			default: return;
		}
	}

	void CheckIfFallenOfLevel()
	{
		if(transform.position.y < -15) ResetBall();
	}

	public void ResetBall()
	{
		isAsleep = true;
		
		isStartingToGetJiggly = false;
		
		jiggleTime = 0;
		jiggleCD = Random.value * 3 + 2;
		asSleep = 0;
		asSleeCD = Random.value * 3 + 2;

		transform.parent.parent = null;
		transform.parent.position = GameStatics.dodo_spawn;
		transform.parent.GetComponent<Rigidbody>().isKinematic = false;
		transform.parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
	}

	void CheckForBehaviour()
	{
		bool isBeingHold = (gameObject.transform.parent.parent != null) ? true : false;
		if(!isBeingHold)
		{
			isAsleep = true;
			asSleeCD = Random.value * 3 + 2;
			asSleep = 0;

			if(!isStartingToGetJiggly)
			{
				jiggleTime += Time.deltaTime;
				
				if(jiggleTime >= jiggleCD)
				{
					isStartingToGetJiggly = true;
				}
			}
			else
			{
				Jiggle();
			}

		}
		else
		{
			isStartingToGetJiggly = false;
			jiggleTime = 0;

			if(isAsleep)
			{
				asSleep += Time.deltaTime;

				if(asSleep >= asSleeCD)
				{
					isAsleep = false;
				}
			}
			else
			{
				BreakFree();
			}
		}
	}

	void BreakFree()
	{
		if(transform.parent == null) return;

		GameObject.Find("GAME").GetComponent<GAME>().DropDodo(transform.parent.name);
	}

	void Jiggle()
	{
		jiggleCD = Random.value * 3 + 2;
		jiggleTime = 0;
		isStartingToGetJiggly = false;

		Debug.Log("JIGGLE");
		Vector3 sum = Vector3.right * Random.Range(-1.0f, 1.0f) * 55.0f;
		sum += Vector3.forward * Random.Range(-1.0f, 1) * 55.0f;
		sum += Vector3.up * Random.Range(200, 350);

		transform.parent.GetComponent<Rigidbody>().AddForce( sum );
	}
}
