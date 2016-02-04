using UnityEngine;
using System.Collections;

namespace jChikken
{


	public interface IKillable
	{
		void Kill();
		string name { get; }
	}
}
