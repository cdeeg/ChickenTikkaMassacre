using UnityEngine;
using System.Collections;

using jChikken;

public class DodoState_Bounce : DodoStateBase
{

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		behaviour.mRigidBody.isKinematic = false;
		behaviour.mRigidBody.velocity = behaviour.currVelocity;
	}
	
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		behaviour.currVelocity = behaviour.mRigidBody.velocity;
	}
	
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		behaviour.mRigidBody.isKinematic = true;
	}

	protected override state GetMyState ()
	{
		return state.bouncing;
	}
	
}
