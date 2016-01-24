using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
#if UNITY_5_2

#elif
using UnityEngine.SceneManagement;
#endif

public class StartupManager : MonoBehaviour
{

	public bool useOculus = false;
	public string nextScene;

	[Header("General Settings")]
	public int minPlayers = 2;
	public int maxPlayers = 4;
	public string[] menuScenes;

	[Header("Player Settings")]
	public Color32[] playerColors;
	public PlayerCharacterSettings settings;

	WeaponObjectPool pool;

	public WeaponObjectPool GetWeaponObjectPool() { return pool; }

	void Start()
	{
		if( nextScene.Length == 0 )
		{
			Debug.LogError("StartupManager: Failed to load next scene because there is none.");
			return;
		}

		pool = GetComponent<WeaponObjectPool>();

		// keep this game object, regardless of which scene is loading
		DontDestroyOnLoad(transform.gameObject);

		// load real level/menu
		#if UNITY_5_2
		Application.LoadLevel( nextScene );
		#elif
		SceneManager.LoadScene( nextScene );
		#endif
	}
}
