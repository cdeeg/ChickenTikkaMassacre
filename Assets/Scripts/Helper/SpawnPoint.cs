using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{

	public GameObject spawnThis;
	public Transform spawnHere;

	void Start ()
	{
		Instantiate( spawnThis, spawnHere.position, Quaternion.identity );
	}
}
