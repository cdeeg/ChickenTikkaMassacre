//=================================================================================================================

/*
 * 		CharacterController for players, base is taken from Unity Standart ThirdPersonController
 * 	
 * 
 */

//=================================================================================================================

using System;
using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

//=================================================================================================================

namespace jChikken
{
	[RequireComponent(typeof(Rigidbody))]
	[RequireComponent(typeof(CapsuleCollider))]
	[RequireComponent(typeof(Animator))]
	public class PlayerController : MonoBehaviour
	{
		#region fields

		//	settings

		[SerializeField] float m_MovingTurnSpeed = 360;
		[SerializeField] float m_StationaryTurnSpeed = 180;
		[SerializeField] float m_JumpPower = 16f;
		[Range(1f, 4f)][SerializeField] float m_GravityMultiplier = 2f;
		[SerializeField] float m_RunCycleLegOffset = 0.2f; //specific to the character in sample assets, will need to be modified to work with others
		[SerializeField] float m_MoveSpeedMultiplier = 1f;
		[SerializeField] float m_AnimSpeedMultiplier = 1f;
		[SerializeField] float m_GroundCheckDistance = 0.1f;

		[SerializeField] float m_airborneFactor = 3f;

		//	state fields

		Rigidbody m_Rigidbody;
		Animator m_Animator;
		float m_OrigGroundCheckDistance;
		const float k_Half = 0.5f;

		Vector3 m_GroundNormal;
		float 	m_CapsuleHeight;
		Vector3 m_CapsuleCenter;
		CapsuleCollider m_Capsule;

		bool  isGrounded;
		float turnAmount;
		float forwardAmount;

		//	accessors

		public Vector3 currVelocity 
		{
			get { return m_Rigidbody.velocity; }
		}

		#endregion

		//=================================================================================================================

		#region init

		void Start()
		{
			m_Animator = GetComponent<Animator>();
			m_Rigidbody = GetComponent<Rigidbody>();
			m_Capsule = GetComponent<CapsuleCollider>();
			m_CapsuleHeight = m_Capsule.height;
			m_CapsuleCenter = m_Capsule.center;
			
			m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			m_OrigGroundCheckDistance = m_GroundCheckDistance;

			m_Animator.applyRootMotion = false;
		}

		#endregion

		//=================================================================================================================

		#region interface

		public void Move(Vector3 move, bool crouch, bool jump)
		{
			
			// convert the world relative moveInput vector into a local-relative
			// turn amount and forward amount required to head in the desired
			// direction.
			if (move.magnitude > 1f) move.Normalize();
			move = transform.InverseTransformDirection(move);
			CheckGroundStatus();
			move = Vector3.ProjectOnPlane(move, m_GroundNormal);
			turnAmount = Mathf.Atan2(move.x, move.z);
			forwardAmount = move.z;

			ApplyExtraTurnRotation();
			
			// control and velocity handling is different when grounded and airborne:
			if (isGrounded)
			{
				HandleGroundedMovement(crouch, jump);
			}
			else
			{
				HandleAirborneMovement();
			}
		
			// send input and other state parameters to the animator
			UpdateAnimatorStates(move);
		}

		public void MeleeAttack()
		{
			//	check if dodo is in front of character
			m_Animator.SetTrigger("PerformMelee");
			m_Rigidbody.AddForce(transform.forward * 200, ForceMode.Acceleration);
		}

		public void SpecialAttack(Weapon weapon)
		{
			m_Animator.SetTrigger("PerformSpecial");
		}

		#endregion

		//-----------------------------------------------------------------------------------------------------------------


		//-----------------------------------------------------------------------------------------------------------------
		
		void UpdateAnimatorStates(Vector3 move)
		{
			// update the animator parameters
			m_Animator.SetBool("hasControl", true);
			m_Animator.SetBool("isGrounded", isGrounded);
			m_Animator.SetFloat("Forward", forwardAmount, 0.05f, Time.deltaTime);
			m_Animator.SetFloat("Turn", turnAmount, 0.05f, Time.deltaTime);

			if (!isGrounded)
			{
				m_Animator.SetFloat("Jump", m_Rigidbody.velocity.y);
			}

			// the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
			// which affects the movement speed because of the root motion.
			if (isGrounded && move.magnitude > 0)
			{
				m_Animator.speed = m_AnimSpeedMultiplier;
			}
			else
			{
				// don't use that while airborne
				m_Animator.speed = 1;
			}
		}
		
		
		void HandleAirborneMovement()
		{
			// apply extra gravity from multiplier:
			Vector3 extraGravityForce = (Physics.gravity * m_GravityMultiplier) - Physics.gravity;
			m_Rigidbody.AddForce(extraGravityForce);
			
			m_GroundCheckDistance = m_Rigidbody.velocity.y < 0 ? m_OrigGroundCheckDistance : 0.01f;
		}
		
		
		void HandleGroundedMovement(bool crouch, bool jump)
		{
			// check whether conditions are right to allow a jump:
			if (jump && !crouch)// && m_Animator.GetCurrentAnimatorStateInfo(0).IsName("isGrounded"))
			{
				// jump!
				m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpPower, m_Rigidbody.velocity.z);
				isGrounded = false;
				m_Animator.applyRootMotion = false;
				m_GroundCheckDistance = 0.1f;
			}
		}
		
		void ApplyExtraTurnRotation()
		{
			// help the character turn faster (this is in addition to root rotation in the animation)
			float turnSpeed = Mathf.Lerp(m_StationaryTurnSpeed, m_MovingTurnSpeed, forwardAmount);
			transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0);
		}
		
		
		public void OnAnimatorMove()
		{
			// we implement this function to override the default root motion.
			// this allows us to modify the positional speed before it's applied.
			if (/*isGrounded &&*/ Time.deltaTime > 0)
			{
	//			Vector3 v = (m_Animator.deltaPosition * m_MoveSpeedMultiplier) / Time.deltaTime;

				Vector3 v = transform.forward * ( m_Animator.GetFloat("Forward") * m_MoveSpeedMultiplier ) / Time.deltaTime;

				if(!isGrounded)
					v *= m_airborneFactor;

				// we preserve the existing y part of the current velocity.
				v.y = m_Rigidbody.velocity.y;
				m_Rigidbody.velocity = v;
			}
		}
		
		
		void CheckGroundStatus()
		{
			float off = m_Capsule.radius * 0.5f;
			Vector3 p1 = transform.position;
			Vector3 p2 = p1 + Vector3.right * off;
			Vector3 p3 = p1 - Vector3.right * off;
			Vector3 p4 = p1 + Vector3.forward * off;
			Vector3 p5 = p1 - Vector3.forward * off;

			if( groundCheck(p1) )
			{

			}
			else if( groundCheck(p2) )
			{

			}
			else if( groundCheck(p3) )
			{

			}
//			else if( groundCheck(p4) )
//			{
//
//			}
			else if( groundCheck(p5) )
			{

			}
			else
			{
				isGrounded = false;
				m_GroundNormal = Vector3.up;
		//		m_Animator.applyRootMotion = false;
			}

//			RaycastHit hitInfo;
//			#if UNITY_EDITOR
//			// helper to visualise the ground check ray in the scene view
//			Debug.DrawLine(transform.position + (Vector3.up * 0.1f), transform.position + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
//			#endif
//			// 0.1f is a small offset to start the ray from inside the character
//			// it is also good to note that the transform position in the sample assets is at the base of the character
//			if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
//			{
//				m_GroundNormal = hitInfo.normal;
//				isGrounded = true;
//		//		m_Animator.applyRootMotion = true;
//			}
//			else
//			{
//				isGrounded = false;
//				m_GroundNormal = Vector3.up;
//				m_Animator.applyRootMotion = false;
//			}
		}


		bool groundCheck(Vector3 p)
		{
			RaycastHit hitInfo;
			#if UNITY_EDITOR
			// helper to visualise the ground check ray in the scene view
			Debug.DrawLine(p + (Vector3.up * 0.1f), p + (Vector3.up * 0.1f) + (Vector3.down * m_GroundCheckDistance));
			#endif
			// 0.1f is a small offset to start the ray from inside the character
			// it is also good to note that the transform position in the sample assets is at the base of the character
			if (Physics.Raycast(p + (Vector3.up * 0.1f), Vector3.down, out hitInfo, m_GroundCheckDistance))
			{
				m_GroundNormal = hitInfo.normal;
				isGrounded = true;
	//			m_Animator.applyRootMotion = true;
				return true;
			}
			else
			{
				return false;
			}
		}
	}


}

//=================================================================================================================