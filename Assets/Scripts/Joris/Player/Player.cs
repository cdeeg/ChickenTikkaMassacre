using UnityEngine;
using System.Collections;

//=================================================================================================================

namespace jChikken
{

	[RequireComponent(typeof(PlayerController))]
	public class Player : MonoBehaviour, IKillable 
	{
		#region fields

		public Transform dodoContainer;
		public Transform weaponContainer;

		public int currScore { get; private set; }

		public bool hasDodo;

		[Range(0, 1), Tooltip("Chance to play an extra idle animation")]
		public float idleAniChance = 0.1f;

		public float RespawnTime = 2f;

		public PlayerController  mController { get; private set; }
		public Animator 		 mAnimatorController { get; private set; }

		bool hasSpawned = true;
		bool isDead = false;

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region interface

		public string name { get { return name; } }

		public void Kill()
		{
			if(hasSpawned)
			{
				isDead = true;
				hasSpawned = false;
				StartCoroutine("waitForRespawn");
			}
		}

		public void ScorePoint()
		{
			currScore++;
			//	make end game check

			mAnimatorController.SetTrigger("scorePoint_self");
			GameManager.GetOpponent(this).mAnimatorController.SetTrigger("scorePoint_opponent");
		}

		public void FetchDodo()
		{
			var dodo = GetDodoObj();
			if(dodo != null)
			{
				dodo.transform.SetParent(dodoContainer);
				dodo.GetCaptured(this);

				mAnimatorController.SetBool("hasDodo", true);
				mAnimatorController.SetFloat("hasDodoF", 1f);
			}
		}

		public void LoseDodo()
		{
			var dodo = GetDodoObj();
			if(dodo != null)
			{
				dodo.transform.SetParent(null);
				dodo.FreeDodo();

				mAnimatorController.SetBool("hasDodo", false);
				mAnimatorController.SetFloat("hasDodoF", 0f);
			}
		}

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region init

		void Awake()
		{
			mAnimatorController = GetComponent<Animator>();
		}

		#endregion

		#region behaviour

		IEnumerator waitForRespawn()
		{
			yield return new WaitForSeconds(RespawnTime);
			
			if(GameManager.GameIsRunning())
			{
		//		mSpawner.Respawn();
			}
		}


		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region util

		private DodoBehaviour GetDodoObj()
		{
			return GameObject.FindObjectOfType<DodoBehaviour>();
		}

		#endregion
	}

}

//=================================================================================================================