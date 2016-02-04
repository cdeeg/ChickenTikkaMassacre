using UnityEngine;
using System.Collections;

namespace jChikken
{

	public class PlayerSpawner : MonoBehaviour
	{

		public Player mPlayer;

		[Tooltip("Blink the player when he respawns")]
		public AnimationCurve blinkCurve;

		public float blinkTime = 2f;
		public float RespawnTime = 1.5f;

		private Renderer[] renderers;

		void Awake()
		{
			if(mPlayer != null)
			{
				renderers = mPlayer.gameObject.GetComponentsInHierarchy<Renderer>().ToArray();
				mPlayer.onDying += OnDie;
			}
			else
			{
				throw new System.Exception("no player set for spawner");
			}
		}

		void Start()
		{
			Respawn();
		}

		void OnDie()
		{
			StartCoroutine("waitForRespawn");
		}

		IEnumerator waitForRespawn()
		{
			yield return new WaitForSeconds(RespawnTime);
			
			if(GameManager.GameIsRunning())
			{
				Respawn();
			}
		}

		public void Respawn()
		{
			if(mPlayer != null && !mPlayer.hasSpawned)
			{
				mPlayer.onSpawn(this.transform.position);
				StartCoroutine("SpawnAnimation");
			}
		}



		private IEnumerator SpawnAnimation()
		{
			float startT = Time.time;

			while(Time.time <= startT + blinkTime)
			{
				float t = blinkCurve.Evaluate(jUtility.Math.MapF(Time.time - startT, 0, blinkTime, 0, 1, true));
				SetRenderers(t >= 0.5f);
				yield return null;
			}

			SetRenderers(true);
		}

		private void SetRenderers(bool b)
		{
			for(int i = 0; i < renderers.Length; i++)
			{
				renderers[i].enabled = b;
			}
		}

	}

}