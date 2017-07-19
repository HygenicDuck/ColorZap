using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kanga;
using Utils;
using System;

namespace Kanga
{
	/// <summary>
	/// Response object handler.
	/// This object is returned to the client when preforming a server request, and is how the client accesses all server objects.
	/// </summary>

	public class ResponseObjectHandler
	{
		private ServerCache m_cache;
		private long m_creationTime;
		private string m_creationKey;
		private string m_deletedDependantObjectID;
		private string[] m_order;
		private bool m_writeToDisk = false;
		private bool m_important = false;
		private HashSet<string> m_hitObjectIDs = new HashSet<string> ();
		private HashSet<string> m_helperObjectIDs = new HashSet<string> ();
		private Dictionary<string, ServerCache.CacheItem> m_cacheFallback = new Dictionary<string, ServerCache.CacheItem> ();
		static private IEnumerable<string> s_mandatoryServerProperties = new List<string>{ "hits", "order" };
		static private IEnumerable<string> s_standardServerProperties = new List<string> {
			"hits",
			"order",
			"helper_objects",
			"creation_key",
			"extra_data"
		};

		public void SetWriteToDisk()
		{
			m_writeToDisk = true;

			SetObjectsAsImportant(true);
		}

		public bool ShouldWriteToDisk()
		{
			return m_writeToDisk;
		}

		public void SetImportant(bool isImportant)
		{
			m_important = isImportant;

			SetObjectsAsImportant(m_important);
		}

		private void SetObjectsAsImportant(bool important)
		{
			foreach (string hitID in m_hitObjectIDs)
			{
				m_cache.SetCacheItemImportant(hitID, important);
			}
				
			foreach (string helperID in m_helperObjectIDs)
			{
				m_cache.SetCacheItemImportant(helperID, important);

			}
		}

		public bool IsImportant()
		{
			return m_important;
		}

		public string DeletedDependantObjectID
		{ 
			get
			{ 
				return m_deletedDependantObjectID; 
			} 
			set
			{ 
				m_deletedDependantObjectID = value; 
			} 
		}

		public Hashtable GenerateHashTable()
		{
			Hashtable table = new Hashtable ();

			table.Add("creation_time", m_creationTime);
			table.Add("creation_key", m_creationKey);

			ArrayList order = new ArrayList ();

			for (int i = 0; i < m_order.Length; ++i)
			{
				order.Add(m_order[i]);
			}

			table.Add("order", order);

			ArrayList hitIDs = new ArrayList ();

			foreach (string key in m_hitObjectIDs)
			{
				ServerCache.CacheItem item = m_cache.GetObject(key);

				if (item != null)
				{
					item.objectData.SetWriteToDisk();
				}

				hitIDs.Add(key);
			}

			table.Add("hit_ids", hitIDs);

			ArrayList helperObjectIDs = new ArrayList ();

			foreach (string key in m_helperObjectIDs)
			{
				ServerCache.CacheItem item = m_cache.GetObject(key);

				if (item != null)
				{
					item.objectData.SetWriteToDisk();
				}

				helperObjectIDs.Add(key);
			}

			table.Add("helper_object_ids", helperObjectIDs);

			return table;
		}

		public void LoadFromHashtable(Hashtable table)
		{
			m_creationTime = (long)table["creation_time"];
			m_creationKey = (string)table["creation_key"];

			ArrayList order = table["order"] as ArrayList;

			m_order = new string[order.Count];

			for (int i = 0; i < order.Count; ++i)
			{
				m_order[i] = order[i] as string;
			}
				
			ArrayList hitIDs = table["hit_ids"] as ArrayList;

			for (int i = 0; i < hitIDs.Count; ++i)
			{
				string hitID = hitIDs[i] as string;

				m_hitObjectIDs.Add(hitID);

				ServerCache.CacheItem cacheItem = m_cache.GetObject(hitID);
				m_cacheFallback.Add(hitID, cacheItem);
			}

			ArrayList helperObjectIDs = table["helper_object_ids"] as ArrayList;

			for (int i = 0; i < helperObjectIDs.Count; ++i)
			{
				string helperObjectID = helperObjectIDs[i] as string;

				m_helperObjectIDs.Add(helperObjectID);

				ServerCache.CacheItem cacheItem = m_cache.GetObject(helperObjectID);
				m_cacheFallback[helperObjectID] = cacheItem;
			}
		}

		public void UpdateFromCacheFallback()
		{
			foreach (string hitID in m_hitObjectIDs)
			{
				ServerCache.CacheItem cacheItem = m_cache.GetObject(hitID);
				if (cacheItem != null)
				{
					m_cacheFallback[hitID] = cacheItem;
				}
			}

			foreach (string helperObjectID in m_helperObjectIDs)
			{
				ServerCache.CacheItem cacheItem = m_cache.GetObject(helperObjectID);
				if (cacheItem != null)
				{
					m_cacheFallback[helperObjectID] = cacheItem;
				}
			}
		}

		public override string ToString()
		{

			return (@"
				m_creationTime " + m_creationTime + @"
				m_creationKey " + m_creationKey + @"
				m_hitObjectIDs " + m_hitObjectIDs.Count + @"
				m_helperObjectIDs " + m_helperObjectIDs.Count + @"
			");
		}


		public ResponseObjectHandler(ServerCache cache)
		{
			m_cache = cache;
		}

		public ResponseObjectHandler(Hashtable resourceData, ServerCache cache)
		{
			m_creationTime = Utils.Date.GetEpochTimeMills();
			m_cache = cache;

			ParseResponseData(resourceData);
		}

		public ResponseObjectHandler(ServerObjectBase[] hits, ServerObjectBase[] helperObjects, ServerCache cache)
		{
			m_creationTime = Utils.Date.GetEpochTimeMills();
			m_cache = cache;

			BuildFromCache(hits, helperObjects);
		}

		public ResponseObjectHandler(ServerObjectBase[] hits, ServerObjectBase[] helperObjects, ServerCache cache, long createdOnTime)
		{
			m_creationTime = createdOnTime;
			m_cache = cache;

			BuildFromCache(hits, helperObjects);
		}

		private void ParseResponseData(Hashtable resourceData)
		{
			if (CheckForNonStandardObject(resourceData))
			{
				FixNonStandardObject(resourceData);
			}
				
			m_creationKey = (string)resourceData["creation_key"];

			ParseHits(resourceData);
			ParseHelperObjects(resourceData);
			ParseExtraData(resourceData);
		}

		public bool CheckForNonStandardObject(Hashtable resourceData)
		{
			bool nonStandardObject = false;
			if (resourceData.Keys.Count > 3)  // Check for hits, order, helperObjects, and object_id
			{
				return true;
			}

			foreach (string key in s_mandatoryServerProperties)
			{
				if (!resourceData.ContainsKey(key))
				{
					return true;
				}
			}
				
			return nonStandardObject;
		}

		public void FixNonStandardObject(Hashtable resourceData)
		{
			if (!resourceData.ContainsKey("hits"))
			{
				resourceData.Add("hits", new Hashtable ());
			}
			if (!resourceData.ContainsKey("order"))
			{
				resourceData.Add("order", new ArrayList ());
			}
			if (!resourceData.ContainsKey("helper_objects"))
			{
				resourceData.Add("helper_objects", new Hashtable ());
			}

			Hashtable helperObjects = (Hashtable)resourceData["helper_objects"];
			ArrayList order = (ArrayList)resourceData["order"];

			if (resourceData.ContainsKey("object_id"))
			{
				Hashtable hits = (Hashtable)resourceData["hits"];
				string objectId = (string)resourceData["object_id"];
				Hashtable copyObject = (Hashtable)resourceData[objectId];
				hits.Add(objectId, copyObject);
				order.Add(objectId);
				resourceData.Remove(objectId);
			}

			if (resourceData.ContainsKey("friends"))
			{
				Hashtable friends = (Hashtable)resourceData["friends"];

				Hashtable hits = (Hashtable)resourceData["hits"];

				foreach (string key in friends.Keys)
				{
					hits.Add(key, friends[key]);
					order.Add(key);
				}
			}

			if (resourceData.ContainsKey("tags_topped") && resourceData.ContainsKey("global_tags_topped"))
			{
				ArrayList tagsTopped = (ArrayList)resourceData["tags_topped"];
				ArrayList globalTopped = (ArrayList)resourceData["global_tags_topped"];

				Hashtable tagsToppedObject = new Hashtable ();

				tagsToppedObject.Add("object_type", "tags_topped");
				tagsToppedObject.Add("object_id", "tags_topped");
				tagsToppedObject.Add("tags_topped", tagsTopped);
				tagsToppedObject.Add("global_tags_topped", globalTopped);

				resourceData.Remove("tags_topped");
				resourceData.Remove("global_tags_topped");

				helperObjects.Add("tags_topped", tagsToppedObject);
			}

			// This if statement is handling the play-history-stats and play-stats endpoints.
			// play-stats is a version of play-history-stats with data stripped out to
			// fix a bug
			if (resourceData.ContainsKey("player_round_stats") && resourceData.ContainsKey("player_chain_stats") && resourceData.ContainsKey("round_stats") && resourceData.ContainsKey("chain_stats"))
			{
				Hashtable playerRoundStats = (Hashtable)resourceData["player_round_stats"];
				Hashtable playerChainStats = (Hashtable)resourceData["player_chain_stats"];
				Hashtable roundStats = (Hashtable)resourceData["round_stats"];
				Hashtable chainStats = (Hashtable)resourceData["chain_stats"];

				Hashtable playHistoryStatsObject = new Hashtable ();

				playHistoryStatsObject.Add("object_type", "play_history_stats");
				playHistoryStatsObject.Add("object_id", "play_history_stats");
				playHistoryStatsObject.Add("player_round_stats", playerRoundStats);
				playHistoryStatsObject.Add("player_chain_stats", playerChainStats);
				playHistoryStatsObject.Add("round_stats", roundStats);
				playHistoryStatsObject.Add("chain_stats", chainStats);

				resourceData.Remove("player_round_stats");
				resourceData.Remove("player_chain_stats");
				resourceData.Remove("round_stats");
				resourceData.Remove("chain_stats");

				Hashtable hits = (Hashtable)resourceData["hits"];
				hits.Add("play_history_stats", playHistoryStatsObject);
				order.Add("play_history_stats");
			}
			else if (resourceData.ContainsKey("round_stats") && resourceData.ContainsKey("chain_stats"))
			{
				Hashtable roundStats = (Hashtable)resourceData["round_stats"];
				Hashtable chainStats = (Hashtable)resourceData["chain_stats"];

				Hashtable playStatsObject = new Hashtable ();

				playStatsObject.Add("object_type", "play_stats");
				playStatsObject.Add("object_id", "play_stats");
				playStatsObject.Add("round_stats", roundStats);
				playStatsObject.Add("chain_stats", chainStats);

				resourceData.Remove("round_stats");
				resourceData.Remove("chain_stats");

				Hashtable hits = (Hashtable)resourceData["hits"];
				hits.Add("play_stats", playStatsObject);
				order.Add("play_stats");
			}
			else if (resourceData.ContainsKey("collection_ids"))
			{
				Utils.Debugger.Warning("collection_ids key is found at resources level. Server are removing this.", (int)SharedSystems.Systems.SERVER_CACHE);
				resourceData.Remove("collection_ids");
			}

			List<string> deleteList = new List<string> ();

			foreach (string key in resourceData.Keys)
			{
				if (!s_standardServerProperties.Contains(key))
				{
					helperObjects.Add(key, resourceData[key]);
					deleteList.Add(key);
				}
			}

			foreach (string key in deleteList)
			{
				resourceData.Remove(key);
			}
		}

		/// <summary>
		/// Stuffs the cache with the fallback data, this gets called just before we write this object out to disk, this insures all the objects it needs will be written out to the object cache
		/// </summary>
		public void StuffCache()
		{
			Dictionary<string, ServerCache.CacheItem>.KeyCollection keyColl = m_cacheFallback.Keys;

			foreach (string key in keyColl)
			{
				ServerCache.CacheItem cacheItem = m_cacheFallback[key];

				if (cacheItem == null)
				{
					Debugger.Error("Cache item " + key + " is null", (int)SharedSystems.Systems.SERVER_CACHE);
				}
				else
				{
					ServerObjectBase serverObject = m_cacheFallback[key].objectData;
					serverObject.SetWriteToDisk();

					m_cache.SetObject(key, serverObject);
				}
			}
		}

		private void ParseHits(Hashtable resourceData)
		{
			Hashtable hits = (Hashtable)resourceData["hits"];
			ArrayList order = new ArrayList ();
			order = (ArrayList)resourceData["order"];
			m_order = new string[order.Count];

			for (int i = 0; i < order.Count; i++)
			{
				string key = (string)order[i];
				Hashtable objectHashtable = (Hashtable)hits[key];
				if (objectHashtable != null) //With pagination we might receive more objects in order than in hits
				{
					string objectType = (string)objectHashtable["object_type"];

					ServerObjectBase obj = ServerObjectFactory.CreateInstance(objectType);
					if (obj != null)
					{
						obj.ParseResponse(objectHashtable, true);
						m_hitObjectIDs.Add(key);
						ServerCache.CacheItem item = m_cache.SetObject(key, obj);
						m_cacheFallback.Add(key, item);
					}
				}
				m_order[i] = key;
			}
		}

		private void ParseHelperObjects(Hashtable resourceData)
		{
			Hashtable helperObjects = (Hashtable)resourceData["helper_objects"];
			if (helperObjects != null)
			{

				// todo: DIRTY HACK for tag_scores in incorrect format
				if (helperObjects.ContainsKey("tag_scores"))
				{
					Hashtable tagsDict = (Hashtable)helperObjects["tag_scores"];

					ArrayList newTagsList = new ArrayList ();

					foreach (string dictKey in tagsDict.Keys)
					{
						newTagsList.Add(tagsDict[dictKey]);
					}

					if (tagsDict.ContainsKey("TagScores"))
					{
						tagsDict.Remove("TagScores");
					}

					tagsDict.Add("TagScores", newTagsList);


					if (!tagsDict.ContainsKey("objectType"))
					{
						tagsDict.Add("object_type", "tag_scores");	
					}

					if (!tagsDict.ContainsKey("objectID"))
					{
						tagsDict.Add("object_id", "tag_scores");	
					}
				}
				// end of DIRTY HACK

				foreach (string key in helperObjects.Keys)
				{
					Hashtable objectHashtable = helperObjects[key] as Hashtable;
					if (objectHashtable != null)
					{
						// todo: DIRTY HACK for missing object_type in the GET /game-settings response
						if (helperObjects.ContainsKey("settings"))
						{
							objectHashtable.Add("object_type", "game");
							objectHashtable.Add("object_id", "settings");
						}
						// end of DIRTY HACK

						string objectType = (string)objectHashtable["object_type"];

						ServerObjectBase obj = ServerObjectFactory.CreateInstance(objectType);
						if (obj != null)
						{
							obj.ParseResponse(objectHashtable, false);
							m_helperObjectIDs.Add(key);	
							ServerCache.CacheItem item = m_cache.SetObject(key, obj);
							m_cacheFallback.Add(key, item);
						}
						else
						{
							Debugger.Log("Failed to instantiate server object of type " + objectType + ". Missing call to ServerObjectFactory.RegisterType?"); 
						}
					}
					else
					{
						var serverObject = helperObjects[key];
						string name = "null";

						if (serverObject != null)
						{
							Type objectType = serverObject.GetType();
							name = objectType.Name;
						}

						Debugger.Log("unexpected object: " + key + ", of type: " + name + " in helper_objects.", Debugger.Severity.WARNING);
					}
				}
			}
		}

		private void ParseExtraData(Hashtable resourceData)
		{
			Hashtable extraData = (Hashtable)resourceData["extra_data"];
			if (extraData != null)
			{
				List<ServerObjectBase> dataToSend = new List<ServerObjectBase> ();
				foreach (string key in extraData.Keys)
				{
					Hashtable objectHashtable = extraData[key] as Hashtable;
					if (objectHashtable != null)
					{
						string objectType = (string)objectHashtable["object_type"];

						ServerObjectBase obj = ServerObjectFactory.CreateInstance(objectType);
						if (obj != null)
						{
							obj.ParseResponse(objectHashtable, true);
							dataToSend.Add(obj);
						}
						else
						{
							Debugger.Log("Failed to instantiate server object of type " + objectType + ". Missing call to ServerObjectFactory.RegisterType?"); 
						}
					}
				}
				KangaEvents.SendExtraDataEvent(dataToSend);
			}
		}

		private void BuildFromCache(ServerObjectBase[] hits, ServerObjectBase[] helperObjects)
		{
			if (hits.Length > 0)
			{
				m_order = new string[hits.Length];

				for (int i = 0; i < hits.Length; i++)
				{
					string key = hits[i].GetObjectID();
					m_hitObjectIDs.Add(key);
					m_order[i] = key;

					ServerCache.CacheItem cacheItem = m_cache.GetObject(key);

					m_cacheFallback[key] = cacheItem;
				}
			}

			if (helperObjects != null && helperObjects.Length > 0)
			{
				for (int i = 0; i < helperObjects.Length; i++)
				{
					string key = helperObjects[i].GetObjectID();
					m_helperObjectIDs.Add(key);

					ServerCache.CacheItem cacheItem = m_cache.GetObject(key);

					m_cacheFallback[key] = cacheItem;
				}
			}
		}

		public void SearchCache(Request request)
		{
			CacheSearchCriteria cacheSearch = request.cacheSearch;

			if (cacheSearch == null)
			{
				return;
			}

			List<ServerObjectBase> foundObjects = m_cache.SearchCache(cacheSearch);
			List<ServerObjectBase> cachedObjects = new List<ServerObjectBase> ();

			// revers the list as if more than one is found then they'll be in the wrong order
			foundObjects.Reverse();

			// make a list of all the cached objects that meet the search criteria that aren't already in the response
			for (int i = 0; i < foundObjects.Count; ++i)
			{
				string objectID = foundObjects[i].GetObjectID();

				if (!m_hitObjectIDs.Contains(objectID))
				{
					cachedObjects.Add(foundObjects[i]);
				}
			}
				
			List<string> cachedOrder = new List<string> ();

			for (int i = 0; i < cachedObjects.Count; ++i)
			{
				string objectID = cachedObjects[i].GetObjectID();

				ServerCache.CacheItem item = m_cache.GetObject(objectID);

				if (item != null)
				{
					cachedOrder.Add(objectID);

					m_hitObjectIDs.Add(objectID);

					m_cacheFallback[objectID] = item;
				}
			}

			string[] foundHits = cachedOrder.ToArray();

			m_order = foundHits.Concat(m_order).ToArray();

			CheckForRemovedObjects();
		}

		private void CheckForRemovedObjects()
		{
			List<string> foundObjectIDs = new List<string> ();

			for (int i = 0; i < m_order.Length; ++i)
			{
				// grab all the items out the fallback
				ServerCache.CacheItem item = m_cacheFallback[m_order[i]];
				ServerObjectDocument serverObject = item.objectData as ServerObjectDocument;

				// we are looking for documents that are temp objects
				if (serverObject != null && serverObject.isTempObject)
				{
					item = m_cache.GetObject(m_order[i]);

					// if the temp object isn't in the cache then its been removed so we don't want it here either...
					if (item == null)
					{
						continue;
					}
				}

				// if we've reached here then all is good
				foundObjectIDs.Add(m_order[i]);
			}

			if (foundObjectIDs.Count == m_hitObjectIDs.Count)
			{
				return;
			}

			m_order = foundObjectIDs.ToArray();
		}

		public void RemoveDeletedHits()
		{
			List<string> orderList = new List<string> (m_order);
			for (int i = orderList.Count - 1; i >= 0; i--)
			{
				string key = orderList[i];
				ServerObjectBase serverObject = GetHit(key);
				if (serverObject != null)
				{
					ServerObjectDocument doc = serverObject as ServerObjectDocument;
					if (doc != null && doc.deletedOn != 0)
					{
						orderList.RemoveAt(i);
						m_hitObjectIDs.Remove(key);
					}
				}
			}

			m_order = orderList.ToArray();
		}

		public string[] GetOrder()
		{
			return m_order;
		}

		public int GetNumHits()
		{
			return m_order.Length;
		}

		public int GetNumHitIDs()
		{
			return m_hitObjectIDs.Count;
		}

		public ServerObjectBase GetHit(string key)
		{
			ServerObjectBase ret = null;

			if (m_hitObjectIDs.Contains(key))
			{
				ServerCache.CacheItem item = m_cache.GetObject(key);

				if (item != null)
				{
					ret = item.objectData;
					ret.Populate();
				}
				else
				{
					ret = m_cacheFallback[key].objectData;
					ret.Populate();
				}
			}

			return ret;				
		}

		public ServerObjectBase GetHelperObject(string key)
		{			
			ServerObjectBase ret = null;

			if (m_helperObjectIDs.Contains(key))
			{
				ServerCache.CacheItem item = m_cache.GetObject(key);

				if (item != null)
				{
					ret = item.objectData;
					ret.Populate();
				}
				else
				{
					ret = m_cacheFallback[key].objectData;
					ret.Populate();
				}
			}

			return ret;				
		}

		public ServerObjectBase GetCacheFallbackObject(string key)
		{			
			ServerObjectBase ret = null;

			if (m_cacheFallback.ContainsKey(key))
			{
				ret = m_cacheFallback[key].objectData;
				ret.Populate();
			}

			return ret;				
		}

		public T GetHit<T>(string key) where T: ServerObjectBase
		{	
			T ret = null;
			ServerObjectBase baseObj = GetHit(key);

			if (baseObj != null && baseObj.GetType() == typeof(T))
			{
				ret = (T)baseObj;
			}

			return ret;				
		}

		public T GetHelperObject<T>(string key) where T: ServerObjectBase
		{	
			T ret = null;
			ServerObjectBase baseObj = GetHelperObject(key);

			if (baseObj != null && baseObj.GetType() == typeof(T))
			{
				ret = (T)baseObj;
			}

			return ret;				
		}

		// return the first instance of a type within the helper objects, only use if you know there will only be one
		public T GetHelperObject<T>() where T: ServerObjectBase
		{
			foreach (string key in m_helperObjectIDs)
			{
				ServerCache.CacheItem item = m_cache.GetObject(key);

				if (item != null && item.objectData.GetType() == typeof(T))
				{
					return (T)GetHelperObject(key);
				}
			}

			return null;				
		}

		public List<T> GetHelperObjects<T>() where T: ServerObjectBase
		{
			List<T> helperObjectsOfTypeT = new List<T>();
			foreach (string key in m_helperObjectIDs)
			{
				ServerCache.CacheItem item = m_cache.GetObject(key);

				if (item != null && item.objectData.GetType() == typeof(T))
				{
					helperObjectsOfTypeT.Add((T)GetHelperObject(key));
				}
			}

			return helperObjectsOfTypeT;				
		}

		public ServerObjectBase GetHitFromIndex(int idx)
		{
			ServerObjectBase ret = null;

			if (idx >= 0 && idx < m_order.Length)
			{
				string key = m_order[idx];
				ret = GetHit(key);
			}

			return ret;
		}

		public T GetHitFromIndex<T>(int idx) where T: ServerObjectBase
		{	
			T ret = null;
			ServerObjectBase baseObj = GetHitFromIndex(idx);

			if (baseObj != null && baseObj.GetType() == typeof(T))
			{
				ret = (T)baseObj;
			}

			return ret;				
		}


		//****************************************************
		//	TODO: REMOVE THIS WHEN ANDY FIXES THE GET PRODUCTS ENDPOINT
		//****************************************************
		public HashSet<string> TempGetHelperObjectIDs()
		{
			return m_helperObjectIDs;
		}
		//****************************************************

		public string GetObjectId()
		{
			return m_order[0];
		}

		public string GetCreationKey()
		{
			return m_creationKey;
		}

		public long GetCreationTime()
		{
			return m_creationTime;
		}
	}
}
