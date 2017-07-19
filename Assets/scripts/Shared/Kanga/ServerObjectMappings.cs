using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Kanga
{
	public static class ServerObjectMappings
	{
		public class MapObject
		{
			public FieldInfo field;
			public bool uploadable; // used to filter out server object parameters that are readonly
		}

		private static Dictionary<Type,Dictionary<string,MapObject>> s_map = new Dictionary<Type,Dictionary<string,MapObject>>();

		public static bool CreateNewType(Type serverObjectType)
		{
			bool created = false;

			if (!s_map.ContainsKey(serverObjectType))
			{
				created = true;
				s_map.Add(serverObjectType, new Dictionary<string,MapObject> ());
			}

			return created;
		}

		public static void AddMapping(Type serverObjectType, string serverKey, string clientKey, bool uploadable)
		{
			if( s_map.ContainsKey(serverObjectType) )
			{
				FieldInfo field = serverObjectType.GetField(clientKey);

				MapObject mapObject = new MapObject {
					field = field, 
					uploadable = uploadable
				};

				if( !s_map[serverObjectType].ContainsKey(serverKey) )
				{
					s_map[serverObjectType].Add(serverKey,mapObject);
				}				
			}
		}

		public static FieldInfo GetValue(Type serverObjectType, string serverKey)
		{
			return s_map[serverObjectType][serverKey].field;
		}

		public static bool GetUploadable(Type serverObjectType, string serverKey)
		{
			return s_map[serverObjectType][serverKey].uploadable;
		}

		public static Dictionary<string, MapObject>.KeyCollection GetKeys(Type serverObjectType)
		{
			return s_map[serverObjectType].Keys;
		}
	}
}