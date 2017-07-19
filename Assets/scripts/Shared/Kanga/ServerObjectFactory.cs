using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Kanga
{
	/// <summary>
	/// Server object factory.
	/// Factory to create any server object given the objectID from the server request
	/// </summary>
	public static class ServerObjectFactory
	{	
		private static Dictionary<string, Func<ServerObjectBase>> m_idFuncs = new Dictionary<string, Func<ServerObjectBase>>();
		private static Dictionary<string, Type> m_idTypes = new Dictionary<string, Type>();

		public static void RegisterType<T>( string objectID ) where T: ServerObjectBase, new()
		{
			if (m_idFuncs.ContainsKey(objectID))
			{
				return;
			}

			m_idFuncs.Add( objectID, ()=> {
				T obj = new T();
				return obj;
			});

			m_idTypes.Add(objectID, typeof(T));
		}

		public static ServerObjectBase CreateInstance( string objectID )
		{
			ServerObjectBase ret = null;

			if( objectID != null && m_idFuncs.ContainsKey( objectID ) )
			{
				ret = m_idFuncs[objectID]();
			}

			return ret;
		}

		public static string GetObjectType(Type type)
		{
			foreach (KeyValuePair<string, Type> pair in m_idTypes)
			{
				if (pair.Value == type)
				{
					return pair.Key;
				}
			}

			return null;
		}
	}
}
