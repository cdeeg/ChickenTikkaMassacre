//=================================================================================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//=================================================================================================================

namespace jUtility
{


	public static class Reflection 
	{

		//	returns an instance of all classes in the assembly that derive from T
		public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
		{
			List<T> objects = new List<T>();
			foreach (Type type in 
			         Assembly.GetAssembly(typeof(T)).GetTypes()
			         .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
			{
				objects.Add((T)Activator.CreateInstance(type, constructorArgs));
			}
			objects.Sort();
			return objects;
		}

		//	returns all types that derive from T
		public static System.Type[] GetTypesInAssembly<T>( bool discardAbstractTypes ) where T : class
		{
			 return Assembly.GetAssembly(typeof(T)).GetTypes()
					.Where(x=>x.IsSubclassOf(typeof(T)) && (discardAbstractTypes ? !x.IsAbstract : true)).ToArray();
		}

	}
	
}

//=================================================================================================================