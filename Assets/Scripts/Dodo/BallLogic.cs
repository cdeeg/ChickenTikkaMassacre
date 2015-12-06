using UnityEngine;
using System.Collections;

public class BallLogic : MonoBehaviour {

	bool isAsleep = true;
	
	bool isStartingToGetJiggly = false;

	float jiggleTime = 0;
	float jiggleCD = Random.value * 3 + 2;
	float asSleep = 0;
	float asSleeCD = Random.value * 3 + 8;

	bool InBreakFreeIteration = false;

	Vector3 ai_targetPos = Vector3.zero;
	Player[] plrs;

	NavMeshAgent NMA;
	Rigidbody parent_Rigidbody;
	// Use this for initialization
	void Start () {
		plrs = GameObject.Find("GAME").GetComponent<GAME>().GetPlayers();
		NMA = transform.parent.GetComponent<NavMeshAgent>();
		parent_Rigidbody = transform.parent.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
	
		if(transform.parent.parent) return;

		Manual_SwitchPosition();
		if(ai_targetPos != Vector3.zero) {FlyTo(ai_targetPos); return;}
		else if( PlayerIsNear() ){Flee(); return;}
		else Relax();

		bool prevNMA_value = NMA.enabled;
		NMA.enabled = true;
		CheckIfEdgeIsToClose();
		CheckForBehaviour();
		NMA.enabled = prevNMA_value;

		CheckIfFallenOfLevel();
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log("Jimmies rustled");

		switch(other.tag)
		{
			case "Basket":
			{
				//calculate team -> point
				GameCommand.ScorePoint( other.GetComponent<TeamBasket>().GetTeam() );
				break;
			}
			case"Weapon":
			{
				//calculate dmg -> k.o.
				break;
			}
			case"Water":
			{
				//should not be here - so go back or start jumping like crazy
				SwitchPosition(Random.Range(0,4));
				break;
			}
			default: return;
		}
	}

	void OnCollisionEnter(Collision other)
	{
		if(other.collider.tag == "Water")
		{
			SwitchPosition(Random.Range(0,4));
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
		asSleeCD = Random.value * 3 + 8;
		InBreakFreeIteration = false;

		transform.parent.parent = null;
		transform.parent.position = GameStatics.dodo_spawn;
		parent_Rigidbody.isKinematic = false;
		parent_Rigidbody.velocity = Vector3.zero;
	}

	void CheckForBehaviour()
	{
		bool isBeingHold = (gameObject.transform.parent.parent != null) ? true : false;
		if(!isBeingHold)
		{
			InBreakFreeIteration = false;
			isAsleep = true;
			asSleeCD = Random.value * 3 + 8;
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
				if(!InBreakFreeIteration)
				{
					InBreakFreeIteration = true;
					StartCoroutine("AwakeAndJump");
				}
			}
		}
	}

	void BreakFree()
	{
		if(transform.parent == null) return;

		GameCommand.Dodo.DropDodo(transform.parent.name);

		AudioClip[] c = Resources.LoadAll<AudioClip>("Sounds/Dodo/Freed");
		GetComponent<AudioSource>().PlayOneShot(c[Random.Range(0, c.Length - 1)]);
	}

	void Jiggle()
	{
		jiggleCD = Random.value * 2 + 0.5f;
		jiggleTime = 0;
		isStartingToGetJiggly = false;

		Debug.Log("JIGGLE");
		Vector3 sum = Vector3.right * Random.Range(-1.0f, 1.0f) * 45.0f;
		sum += Vector3.forward * Random.Range(-1.0f, 1) * 45.0f;
		sum += Vector3.up * Random.Range(100, 250);

		Jump(sum);

		AudioClip[] c = Resources.LoadAll<AudioClip>("Sounds/Dodo/Freed");
		GetComponent<AudioSource>().PlayOneShot(c[Random.Range(0, c.Length - 1)]);
	}

	void Jump(Vector3 dir)
	{
		parent_Rigidbody.AddForce( dir );
	}

	void CheckIfEdgeIsToClose()
	{
		LayerMask lM = plrUtility.bello.GetLayer(Bello.ObjectLayer.Ground);
		if(!Physics.Raycast(transform.position, Vector3.down, 0.5f, lM)) return;

		if(NMA.isOnNavMesh)
		{
			NavMeshHit nHit;
			if(NMA.FindClosestEdge(out nHit ))
			{
				if( Vector3.Distance( transform.position, nHit.position ) < 0.5f)
				{
					lM = plrUtility.bello.GetLayer(Bello.ObjectLayer.Water);
					if(!Physics.Raycast(nHit.position + (nHit.position - transform.position).normalized * 0.2f , Vector3.down, 2f, lM)) return;
					Vector3 sum = Vector3.zero;
					sum = (transform.position - nHit.position).normalized * Random.Range(40, 80);
					sum -= Vector3.up * sum.y;
					sum += Vector3.up * Random.Range(50,150);

					Jump(sum);
				}
			}
		}
	}

	IEnumerator AwakeAndJump()
	{

		float duration = 1.5f;
		float current = 0.0f;

		while(current < duration)
		{
			if(transform.parent.parent != null)
			{
				transform.parent.position = transform.parent.parent.position + Vector3.up * 1.25f + Vector3.one * Random.Range(-0.35f, 0.455f) * current / duration;
			}

			yield return false;
			current += Time.deltaTime;
		}

		BreakFree();
	}

	public void EndAwakening()
	{
		StopAllCoroutines();
		InBreakFreeIteration = false;
	}

	public void PlaySound_Catch()
	{
		AudioClip[] c = Resources.LoadAll<AudioClip>("Sounds/Dodo/Catch");
		GetComponent<AudioSource>().PlayOneShot(c[Random.Range(0, c.Length - 1)]);
	}



	void SwitchPosition(int id)
	{
		if(id > 3) id = 3;

		ai_targetPos = GameObject.Find("Dodo_Targets").transform.GetChild(id).position;	

		NMA.enabled = false;
	}

	void FlyTo(Vector3 targetPos)
	{
		if(Vector3.Distance(transform.position, targetPos) < 2)
		{
			ai_targetPos = Vector3.zero;
			parent_Rigidbody.velocity *= 0.2f;
			return;
		}

		Vector3 dir = targetPos - transform.position;
		dir = dir.normalized;
		parent_Rigidbody.velocity = (dir * 200.0f) * Time.deltaTime + Vector3.up * Mathf.Clamp(parent_Rigidbody.velocity.y, -4.0f, 800.0f);
		if(targetPos.y + 2  > transform.position.y) parent_Rigidbody.AddForce(Vector3.up * 50 * Time.deltaTime);

		LayerMask lm = plrUtility.bello.GetLayer(Bello.ObjectLayer.Ground);
		if(Physics.Raycast(transform.position, Vector3.down, 1.5f, lm)) {transform.parent.position += Vector3.up * 1 * Time.deltaTime;}
		
		lm = plrUtility.bello.GetLayer(Bello.ObjectLayer.Water);
		if(Physics.Raycast(transform.position, Vector3.down, 1.5f, lm)) {transform.parent .position += Vector3.up * 1 * Time.deltaTime;}
	}

	bool PlayerIsNear()
	{
		if( Vector3.Distance( plrs[0].body.position, transform.position ) < 4 ) return true;
		if( Vector3.Distance( plrs[1].body.position, transform.position ) < 4 ) return true;
		return false;
	}

	void Flee()
	{
		NMA.enabled = true;
		if(!NMA.isOnNavMesh) {Debug.Log("No navmesh connection"); return;}

		transform.rotation = Quaternion.Euler(0,0,0);

		LayerMask lm = plrUtility.bello.GetLayer(Bello.ObjectLayer.Ground);
		RaycastHit hit;
		if(!Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f, lm)) {Relax(); return;}

		float plrOneDistance = Vector3.Distance( plrs[0].body.position, transform.position );
		float plrTwoDistance = Vector3.Distance( plrs[1].body.position, transform.position );

		Vector3 goal = ( plrOneDistance < plrTwoDistance) ? 
			( transform.position - plrs[0].body.position ).normalized - ( transform.position - plrs[0].body.position ).normalized.y * Vector3.up + transform.position: 
			( transform.position - plrs[1].body.position ).normalized - ( transform.position - plrs[1].body.position ).normalized.y * Vector3.up + transform.position;

		parent_Rigidbody.isKinematic = true;

		/*
		NavMeshHit nHit;
		if(NMA.FindClosestEdge(out nHit))
		{
			if( Vector3.Distance(transform.position , nHit.position) < 0.5f ) goal = goal; 
		}
		*/

		NMA.destination = goal;
		GameObject.Find("00").transform.position = goal;

	}

	void Relax()
	{
		parent_Rigidbody.isKinematic = false;
		NMA.enabled = false;

	}

	//DEBUG
	void Manual_SwitchPosition()
	{
		int i = -1;
		if(Input.GetKeyDown(KeyCode.Keypad0)){	i = 0; }
		
		if(Input.GetKeyDown(KeyCode.Keypad1)){	i = 1; }
		
		if(Input.GetKeyDown(KeyCode.Keypad2)){  i = 2; }	
		
		if(Input.GetKeyDown(KeyCode.Keypad3)){  i = 3; }	
		
		if( i != -1 ) SwitchPosition(i);
	}
}
