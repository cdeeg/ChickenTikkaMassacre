using System;
using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;

namespace jChikken
{

	[RequireComponent(typeof (PlayerController))]
	public class PlayerInput : MonoBehaviour
	{
		public string HorizontalAxis;
		public string VerticalAxis;
		public KeyCode Attack;
		public KeyCode Jump;

		public HitTester mHitTester;

		private Player m_Player;
		private PlayerController m_Character;	// A reference to the ThirdPersonCharacter on the object
		private Transform m_Cam;                // A reference to the main camera in the scenes transform
		private Vector3 m_CamForward;           // The current forward direction of the camera
		private Vector3 m_Move;					// the world-relative desired move direction, calculated from the camForward and user input.
		private bool m_Jump;                      
		private bool m_MeleeAttack;
		private bool m_SpecialAttack;

		private bool inMelee;

		public Weapon currWeapon { get; private set; }

		void Awake()
		{
			m_Character = GetComponent<PlayerController>();
			m_Player = GetComponent<Player>();
		}

		private void Start()
		{
			// get the transform of the main camera
			if (Camera.main != null)
			{
				m_Cam = Camera.main.transform;
			}
			else
			{
				Debug.LogWarning(
					"Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
				// we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
			}
			
			// get the third person character ( this should never be null due to require component )
			m_Character = GetComponent<PlayerController>();
		}
		
		
		private void Update()
		{
			if (!m_Jump)
			{
				m_Jump = Input.GetKeyDown(Jump);
		//		m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
			}
			if(!m_MeleeAttack)
			{
				m_MeleeAttack = Input.GetKeyDown(Attack);
		//		m_MeleeAttack = CrossPlatformInputManager.GetButtonDown("MeleeAttack");
			}
			if(!m_SpecialAttack && currWeapon != null)
			{
				m_SpecialAttack = Input.GetKeyDown(Jump);
		//		m_SpecialAttack = CrossPlatformInputManager.GetButtonDown("SpecialAttack");
			}
		}
		
		
		// Fixed update is called in sync with physics
		private void FixedUpdate()
		{
			if(!enabled)
				return;

			// read inputs
			float h = Input.GetAxis(HorizontalAxis);
			float v = Input.GetAxis(VerticalAxis);
//			float h = CrossPlatformInputManager.GetAxis("Horizontal");
//			float v = CrossPlatformInputManager.GetAxis("Vertical");

			bool crouch = Input.GetKey(KeyCode.C);

			// calculate move direction to pass to character
			if (m_Cam != null)
			{
				// calculate camera relative direction to move:
				m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
				m_Move = v*m_CamForward + h*m_Cam.right;
			}
			else
			{
				// we use world-relative directions in the case of no main camera
				m_Move = v*Vector3.forward + h*Vector3.right;
			}

//			// walk speed multiplier
//			if (Input.GetKey(KeyCode.LeftShift))
//			{
//				m_Move *= 0.5f;
//			}

			// pass all parameters to the character control script
			m_Character.Move(m_Move, crouch, m_Jump, m_Player.hasDodo ? 0.75f : 1f);

			if(m_SpecialAttack)
			{
				m_Character.SpecialAttack(currWeapon);
			}
			else if(m_MeleeAttack && !inMelee)
			{
				m_Player.mAnimatorController.SetTrigger("PerformMelee");
				StartCoroutine("attackTiming");
			}

			m_Jump = false;
			m_MeleeAttack = false;
			m_SpecialAttack = false;
		}

		bool testForEnemy()
		{
			HitBox b = mHitTester.GetTarget<HitBox>();
			return b != null && b.mPlayer == GameManager.GetOpponent(m_Player);
		}

		bool testForDodo()
		{
			DodoBehaviour dodo = mHitTester.GetTarget<DodoBehaviour>();
			return dodo != null && !dodo.isCaptured;
		}

		IEnumerator attackTiming()
		{
			inMelee = true;
			yield return new WaitForSeconds(0.4f);
			if(testForDodo())
			{
				DodoBehaviour dodo = mHitTester.GetTarget<DodoBehaviour>();
				m_Player.FetchDodo();
			}
			else if(testForEnemy())
			{
				m_Player.MeleeAttack(mHitTester.GetTarget<HitBox>().mPlayer);
			}
			inMelee = false;
		}

	}

}