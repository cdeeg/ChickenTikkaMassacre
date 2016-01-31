using UnityEngine;
using System.Collections;
using jChikken;

public class PlayerState_Idle : StateMachineBehaviour 
{

	float stateStartT;
	float lastIdleTry;

	Player player;

	float lastRand = 100;
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		player = animator.GetComponent<Player>();
		stateStartT = Time.time;
		lastIdleTry = stateStartT + 1f;
		lastRand = 100;
	}
	
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if(Time.time >= lastIdleTry + 1f)
		{
			if(Random.value <= player.idleAniChance)
			{
				lastRand = Random.Range(0f, 20f);
			}
			else lastRand = 100;

			lastIdleTry = Time.time;
		}
		animator.SetFloat("idleRand", lastRand);
	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{

	}

}
