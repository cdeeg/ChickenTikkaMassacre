using UnityEngine;
using System.Collections;

namespace jChikken
{
	[RequireComponent(typeof(BoxCollider))]
	public class DeadZone : MonoBehaviour 
	{

		void OnTriggerEnter(Collider other)
		{
			IKillable toKill = jUtility.UnityUtil.SearchForInterface<IKillable>(other.gameObject);
			if(toKill != null)
			{
				toKill.Kill();


				Debug.Log("kill " + toKill.ToString());
			}
		}
	}

}