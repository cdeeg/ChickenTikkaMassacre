using UnityEngine;
using System.Collections;

using jChikken;

//=================================================================================================================

public class DodoState_Idle : DodoStateBase
{

	//	chance to move a few steps around current position
	const float idleMoveChance = 0.1f;
	
	enum idleStates { standing, move, waitForExit }
	
	idleStates substate;
	Vector3 idleMoveTarget;
	
	//	last time the dodo thought about moving a few steps
	float lastIdleMoveTry;
	
	//	last time the dodo thought about relocating
	float lastRelocateTry;
	
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		substate = idleStates.standing;
		lastRelocateTry = Time.time;
		lastIdleMoveTry = Time.time;
	}
	
	//-----------------------------------------------------------------------------------------------------------------
	
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		switch(substate)
		{
		case idleStates.standing:

			behaviour.ApplyGravity();

			//	think about relocating
			if(Time.time >= lastRelocateTry + 1f)
			{
				if(Random.value <= behaviour.relocateChance)
				{
					if(Random.value > 0.33f)
						behaviour.GenerateRandomPathExplore();
					else
						behaviour.GenerateRandomPathHome();

					substate = idleStates.waitForExit;
				}
				lastRelocateTry = Time.time;
			}
			
			//	think about moving around a bit
			if(Time.time >= lastIdleMoveTry + 1f)
			{
				if(Random.value <= idleMoveChance)
				{
					behaviour.GenerateSmallIdlePath();
//					idleMoveTarget = behaviour.GetNewRandomPositionAroundMe();
//					Debug.Log("new idle move target: " + behaviour.transform.position + " -> " + idleMoveTarget);
//					substate = idleStates.move;
				}
				lastIdleMoveTry = Time.time;
			}
			break;
			
		case idleStates.move:

			if( behaviour.MoveTowards(idleMoveTarget) )
			{
				substate = idleStates.standing;
				lastRelocateTry = Time.time;
				lastIdleMoveTry = Time.time;
			}
			break;
		}
	}
	
	//-----------------------------------------------------------------------------------------------------------------
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
	}
	
	//-----------------------------------------------------------------------------------------------------------------

	protected override state GetMyState ()
	{
		return state.idle;
	}

}

//=================================================================================================================
