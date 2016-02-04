using UnityEngine;
using System.Collections;

//=================================================================================================================

namespace jChikken
{

	[RequireComponent(typeof(PlayerController))]
	public class Player : MonoBehaviour, IKillable 
	{
		#region fields

		const float onHitBlinkT = 0.3f;

		public Material	 material;
		public Transform dodoContainer;
		public Transform weaponContainer;
		public AnimationCurve blinkCurve;

		public int currScore { get; private set; }

		public bool hasDodo;

		[Range(0, 1), Tooltip("Chance to play an extra idle animation")]
		public float idleAniChance = 0.1f;

		public PlayerController  mController { get; private set; }
		public Animator 		 mAnimatorController { get; private set; }

		public bool hasSpawned = false;
		bool isDead = false;

		public event System.Action onDying; 

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region interface

		public string name { get { return gameObject.name; } }

		public void Kill()
		{
			Debug.Log("On kill Player " + hasSpawned);
			if(hasSpawned)
			{
				if(hasDodo)
				{
					LoseDodo(true);
				}
				isDead = true;
				hasSpawned = false;

				if(onDying != null)
					onDying();
			}
		}

		public void onSpawn(Vector3 p) 
		{
			transform.position = p;
			isDead = false;
			hasSpawned = true;
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
				dodo.GetCaptured(this);
				dodo.transform.SetParent(dodoContainer, false);
				dodo.transform.position = dodoContainer.position;
				dodo.currVelocity = Vector3.zero;

				hasDodo = true;

				mAnimatorController.SetBool("hasDodo", true);
				mAnimatorController.SetFloat("hasDodoF", 1f);
			}
		}

		public void LoseDodo(bool killDodo=false)
		{
			var dodo = GetDodoObj();
			if(dodo != null)
			{
				dodo.transform.SetParent(null);
				dodo.FreeDodo();
				hasDodo = false;
				mAnimatorController.SetBool("hasDodo", false);
				mAnimatorController.SetFloat("hasDodoF", 0f);

				if(killDodo)
					dodo.Kill();
			}
		}

		public void MeleeAttack(Player enemy)
		{
			GetComponent<Rigidbody>().AddForce(transform.forward * 200, ForceMode.Acceleration);
			
			if(enemy != null)
			{
				enemy.OnGetHit(transform.position + Vector3.up);
			}
		}

		public void OnGetHit(Vector3 src)
		{
			src -= Vector3.up * 0.3f;
			Rigidbody r = GetComponent<Rigidbody>();
			r.isKinematic = false;
			r.AddForce((transform.position-src).normalized * 6 + Vector3.up * 2, ForceMode.Impulse);
			mController.lastFreezeTime = Time.time;

			if(hasDodo)
			{
				LoseDodo();
			}

			StartCoroutine(blink(Color.red));
		}

		IEnumerator blink(Color c)
		{
			float startT = Time.time;
			while(Time.time <= startT + onHitBlinkT)
			{
				float t = blinkCurve.Evaluate(jUtility.Math.MapF(Time.time-startT, 0, onHitBlinkT, 0, 1f));
				material.SetColor("_Color", Color.Lerp(Color.white, c, t));
				yield return null;
			}
		}

		#endregion

		//-----------------------------------------------------------------------------------------------------------------

		#region init

		void Awake()
		{
			mAnimatorController = GetComponent<Animator>();
			mController = GetComponent<PlayerController>();
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