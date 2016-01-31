using UnityEngine;
using System.Collections;

using jChikken;


public enum state 
{
	idle=0,
	moveTo,
	falling,
	flee,
	captured,
	escapeAttempt,
	bouncing,
	dead
}


public abstract class DodoStateBase : StateMachineBehaviour
{
	protected static state currentState;
	protected static state lastState;

	protected DodoBehaviour behaviour;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		behaviour = animator.GetComponent<DodoBehaviour>();
		currentState = GetMyState();
	}
	
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{

	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		lastState = GetMyState();
	}

	protected abstract state GetMyState();
}
