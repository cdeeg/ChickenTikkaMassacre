using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkManager))]
public class StartupManager : MonoBehaviour {

	public bool UseOverride = false;
	public bool useOculus = false;
	public bool useNetwork = false;
	public GameObject playerPrefab;
	public GameObject overridePrefab;
	public string nextScene;

	[Header("General Settings")]
	public int minPlayers = 2;
	public int maxPlayers = 4;
	public string[] menuScenes;

	[Header("Player Settings")]
	public Color32[] playerColors;
	public Mesh[] playerCharacters;
	public PlayerCharacterSettings settings;

	void Start()
	{
		if( nextScene.Length == 0 )
		{
			Debug.LogError("StartupManager: Failed to load next scene because there is none.");
			return;
		}

		// keep this game object, regardless of which scene is loading
		DontDestroyOnLoad(transform.gameObject);

		// tell network manager which prefab to use for the players
		NetworkManager mng = GetComponent<NetworkManager>();
		mng.playerPrefab = UseOverride ? overridePrefab : playerPrefab;
		mng.enabled = false;

		// load real level/menu
		Application.LoadLevel( nextScene );
	}
}
