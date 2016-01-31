using UnityEngine;
using System.Collections;

using jChikken;

public class DodoState_Captured : DodoStateBase
{

	float stateStartT;
	float lastEscapeTry;
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		stateStartT = Time.time;
		lastEscapeTry = stateStartT + behaviour.escapeAttemptInterval;
	}
	
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if(Time.time >= stateStartT + behaviour.escapeMinDelay)
		{
			if(Time.time >= lastEscapeTry + behaviour.escapeAttemptInterval)
			{
				if(Random.value <= behaviour.escapeAttemptChance)
				{
					behaviour.mController.SetTrigger("tryFlee");
				}
				lastEscapeTry = Time.time;
			}
		}
	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
	}

	protected override state GetMyState ()
	{
		return state.captured;
	}
}
