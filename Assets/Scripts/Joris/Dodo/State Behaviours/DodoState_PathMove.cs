using UnityEngine;
using System.Collections;

using jChikken;
using JPathfinder;

//=================================================================================================================

public class DodoState_PathMove : DodoStateBase
{

	Vector3 nextTargetPos;
	
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		nextTargetPos = behaviour.currentPath.currNode;			//	this allows interrupting a pathing state and come back to it later on 
																//	maybe we need a function that finds best node beyond current in relation to dodo pos
	}
	
	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
	{
		if(behaviour.hasPath)
		{
			Debug.DrawLine(behaviour.transform.position, nextTargetPos, Color.cyan);
			if(behaviour.currentPath.TryGetNextNode(behaviour.transform.position, out nextTargetPos))
			{
				Debug.Log("reached path target!");
				behaviour.ClearCurrentPath();
			}
			else
			{
				behaviour.MoveTowards(nextTargetPos);
			}
		}
	}
	
	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
	}

	protected override state GetMyState ()
	{
		return state.moveTo;
	}
}

//=================================================================================================================