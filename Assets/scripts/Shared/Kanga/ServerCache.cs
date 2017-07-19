using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kanga;
using Utils;
using System;

namespace Kanga
{
	public class ServerCache
	{
		private const string SAVE_FILE = "kanga_cache.txt";
		private const string METHOD = "method";
		private const string ACTION = "action";
		private const string ENDPOINT = "endpoint";
		private const string URI_OBJECT = "uri_object";
		private const string CREATED = "created";
		private const string DATA_OBJECT = "data_object";
		private const string SAVED_REQUEST = "saved_request";

		public class CacheItem
		{
			public ServerObjectBase objectData;
			public bool isImportant;
			public long createdTime;
		}

		private Dictionary<string,CacheItem> m_cachedObjects = new Dictionary<string,CacheItem> ();
		private Dictionary<string,ResponseObjectHandler> m_requestCache = new Dictionary<string,ResponseObjectHandler> ();
		private Server m_server;
		private bool m_localCacheLoaded = false;

		public ServerCache(Server server)
		{
			m_server = server;
		}

		public ResponseObjectHandler CreateObjectHandler(Hashtable resources, QueueItem queueItem)
		{
			Kanga.ResponseObjectHandler objHandler = new Kanga.ResponseObjectHandler (resources, this);

			if (queueItem.request.cacheToDisk)
			{
				objHandler.SetWriteToDisk();
			}

			AddRequest(queueItem.cacheKey, objHandler);

			if (queueItem.dependantObject != null)
			{
				objHandler.DeletedDependantObjectID = queueItem.dependantObject.GetObjectID();
			}

			return objHandler;
		}

		// All new server objects must be created here so that they end up in the cache
		public T CreateServerObject<T>() where T: ServerObjectDocument, new()
		{
			T serverObject = new T ();

			CacheItem item = new CacheItem {
				objectData = serverObject,
				createdTime = Utils.Date.GetEpochTimeMills()
			};
					
			serverObject.Initialise(item.GetHashCode() + item.createdTime.ToString());

			m_cachedObjects.Add(serverObject.GetObjectID(), item);

			return serverObject;
		}

		public void Clear()
		{
			Dictionary<string,CacheItem> cachedObjects = new Dictionary<string,CacheItem> ();
			Dictionary<string,ResponseObjectHandler> requestCache = new Dictionary<string,ResponseObjectHandler> ();

			Dictionary<string, CacheItem>.KeyCollection objectDict = m_cachedObjects.Keys;

			List<string> waitingOnRequests = new List<string> ();

			foreach (string key in objectDict)
			{
				CacheItem item = m_cachedObjects[key];

				ServerObjectBase serverObject = item.objectData;

				if (serverObject.QueueItemDependancy != null || item.isImportant)
				{
					cachedObjects.Add(key, item);

					if (serverObject.QueueItemDependancy != null)
					{
						waitingOnRequests.Add(serverObject.QueueItemDependancy.cacheKey);	
					}
				}
			}
				
			Dictionary<string, ResponseObjectHandler>.KeyCollection requestDict = m_requestCache.Keys;

			foreach (string key in requestDict)
			{
				ResponseObjectHandler item = m_requestCache[key];

				if (item.IsImportant() || item.ShouldWriteToDisk() || waitingOnRequests.Contains(key))
				{
					requestCache.Add(key, item);
					continue;
				}
			}

			m_cachedObjects = cachedObjects;
			m_requestCache = requestCache;
		}

		public void NukeLocalCache()
		{
			m_cachedObjects = new Dictionary<string,CacheItem> ();
			m_requestCache = new Dictionary<string,ResponseObjectHandler> ();

			string fullpath = Path.Combine(Application.persistentDataPath, SAVE_FILE);

			if (File.Exists(fullpath))
			{
				File.Delete(fullpath);
			}

			m_localCacheLoaded = true;

			Debugger.Log("local cache nuked...", (int)SharedSystems.Systems.SERVER_CACHE);
		}

		public void WriteOutCache()
		{
			// don't write out the local cache if we haven't loaded it in because we'll be overwriting things!
			if (!m_localCacheLoaded)
			{
				Debugger.Log("cache not loaded, will not write out cache.", Debugger.Severity.WARNING, (int)SharedSystems.Systems.SERVER_CACHE);
				return;
			}

			string fullpath = Path.Combine(Application.persistentDataPath, SAVE_FILE);
				
			Hashtable serverCache = new Hashtable ();

			//WriteOutLocalCache(serverCache); TODO: we need to handle the failed responses from these and drop them if they are no longer needed, at the moment they just stack up
			WriteOutCachedRequests(serverCache);
			WriteOutCachedObjects(serverCache);

			string json = JSON.JsonEncode(serverCache);

			if( !string.IsNullOrEmpty(json) )
			{
				if (File.Exists(fullpath))
				{
					Debugger.Log("Deleting old cache", (int)SharedSystems.Systems.SERVER_CACHE);

					File.Delete(fullpath);
				}

				StreamWriter stream = File.AppendText(fullpath);

				stream.Write(json);

				stream.Close();

				Debugger.Log("local cache successfully written out", (int)SharedSystems.Systems.SERVER_CACHE);
			}
		}

		private void WriteOutLocalCache(Hashtable serverCache)
		{
			Dictionary<string, CacheItem>.KeyCollection keyColl = m_cachedObjects.Keys;

			Hashtable localCache = new Hashtable ();
			int numRequests = 0;

			foreach (string key in keyColl)
			{
				CacheItem item = m_cachedObjects[key];

				QueueItem queueItem = item.objectData.QueueItemDependancy;

				if (queueItem != null)
				{
					Hashtable table = new Hashtable ();

					Request request = queueItem.request;

					table.Add(METHOD, request.method);
					table.Add(ACTION, request.action);
					table.Add(URI_OBJECT, request.uriObject);
					table.Add(ENDPOINT, request.endpoint);
					table.Add(CREATED, item.createdTime);

					ServerObjectBase objectData = item.objectData;

					table.Add(DATA_OBJECT, objectData.GenerateHashTable(false));

					RequestData requestData = request.requestData;

					table.Add("request_data", requestData.GenerateHashTable());

					localCache.Add(SAVED_REQUEST + "_" + numRequests.ToString(), table);

					++numRequests;
				}
			}

			serverCache.Add("local_cache", localCache);
		}

		private void WriteOutCachedRequests(Hashtable serverCache)
		{
			Dictionary<string, ResponseObjectHandler>.KeyCollection keyColl = m_requestCache.Keys;

			Hashtable cachedRequests = new Hashtable ();
			int numRequests = 0;

			foreach (string key in keyColl)
			{
				ResponseObjectHandler item = m_requestCache[key];

				if (item.ShouldWriteToDisk())
				{
					Hashtable handler = item.GenerateHashTable();

					item.StuffCache();

					cachedRequests.Add(key, handler);

					++numRequests;
				}
			}

			serverCache.Add("cached_requests", cachedRequests);

			Debugger.Log("Found " + numRequests + "requests to write to disk", (int)SharedSystems.Systems.SERVER_CACHE);
		}

		private void WriteOutCachedObjects(Hashtable serverCache)
		{
			Dictionary<string, CacheItem>.KeyCollection keyColl = m_cachedObjects.Keys;

			Hashtable cahcedObjects = new Hashtable ();

			foreach (string key in keyColl)
			{
				ServerCache.CacheItem cacheItem = m_cachedObjects[key] as ServerCache.CacheItem;
				ServerObjectBase serverObject = cacheItem.objectData;

				// object must be of a registered type else we will be unable to instantiate it when we load
				if (serverObject.ShouldWriteToDisk() && ServerObjectFactory.GetObjectType(cacheItem.objectData.GetType()) != null)
				{
					cahcedObjects.Add(key, cacheItem.objectData.GenerateHashTable(false));

					Debugger.Log("Writing out cached object " + key, (int)SharedSystems.Systems.SERVER_CACHE);
				}
			}

			serverCache.Add("cached_objects", cahcedObjects);

			Debugger.Log("Found " + cahcedObjects.Count + "objects to write to disk", (int)SharedSystems.Systems.SERVER_CACHE);
		}

		public List<ServerObjectBase> SearchCache(CacheSearchCriteria cacheSearch)
		{
			List<ServerObjectBase> returnList = new List<ServerObjectBase> ();

			Dictionary<string, CacheItem>.KeyCollection keyColl = m_cachedObjects.Keys;

			foreach (string key in keyColl)
			{
				ServerCache.CacheItem cacheItem = m_cachedObjects[key] as ServerCache.CacheItem;
				ServerObjectBase serverObject = cacheItem.objectData;

				if (serverObject.GetType() == cacheSearch.type)
				{
					Dictionary<string, string> fieldData = cacheSearch.fieldData;

					Dictionary<string, string>.KeyCollection serverKeys = fieldData.Keys;
					Hashtable table = serverObject.GenerateHashTable(false);

					bool objectFound = true;

					foreach (string serverKey in serverKeys)
					{
						if (fieldData[serverKey] != table[serverKey] as string)
						{
							objectFound = false;
							break;
						}
					}

					if (objectFound)
					{
						returnList.Add(serverObject);
					}
				}
			}

			return returnList;
		}

		public void CachedRequestFailed(Hashtable objHandler)
		{
			// TODO: check response status and update the local cached objects appropriately

			Debugger.Log("CachedRequestFailed", (int)SharedSystems.Systems.SERVER_CACHE);
		}

		public void CachedRequestSuccess(Kanga.ResponseObjectHandler objHandler)
		{
			Debugger.Log("CachedRequestSuccess", (int)SharedSystems.Systems.SERVER_CACHE);
		}

		public void LoadCache()
		{
			string fullpath = Path.Combine(Application.persistentDataPath, SAVE_FILE);

			if (File.Exists(fullpath))
			{
				string json = File.ReadAllText(fullpath);

				Hashtable serverCache = JSON.JsonDecode(json) as Hashtable;

				if (serverCache != null)
				{
					ReadInCachedObjects((Hashtable)serverCache["cached_objects"]);
					//ReadInLocalCache((Hashtable)serverCache["local_cache"]); TODO: we need to handle the failed responses from these and drop them if they are no longer needed, at the moment they just stack up
					ReadInCachedRequests((Hashtable)serverCache["cached_requests"]);
				}
			}
			else
			{
				Debugger.Log("Cache does not exist.", (int)SharedSystems.Systems.SERVER_CACHE);
			}

			m_localCacheLoaded = true;
		}

		private void ReadInLocalCache(Hashtable localCache)
		{
			if (localCache != null)
			{
				foreach (DictionaryEntry pair in localCache)
				{
					Hashtable table = localCache[pair.Key] as Hashtable;
					Hashtable dataObjectTable = table[DATA_OBJECT] as Hashtable;

					ServerObjectBase dataObject = ServerObjectFactory.CreateInstance(dataObjectTable["object_type"] as string);

					dataObject.ParseResponse(dataObjectTable, true);

					CacheItem item = new CacheItem {
						objectData = dataObject,
						createdTime = (int)table[CREATED]
					};

					string objectID = (dataObject.GetObjectID() != null) ? dataObject.GetObjectID() : item.createdTime.ToString();

					m_cachedObjects.Add(objectID, item);

					Request request = new Request {
						method = table[METHOD] as string,
						endpoint = table[ENDPOINT] as string,
						uriObject = table[URI_OBJECT] as string,
						action = table[ACTION] as string
					};

					request.requestData.args = table["request_data"] as Hashtable;

					m_server.QueueRequest(request, null, CachedRequestFailed, CachedRequestSuccess, null, 0, dataObject);
				}
			}
		}

		private void ReadInCachedRequests(Hashtable cachedRequests)
		{
			if (cachedRequests != null)
			{
				foreach (DictionaryEntry pair in cachedRequests)
				{
					Hashtable handler = cachedRequests[pair.Key] as Hashtable;

					ResponseObjectHandler responseObjectHandler = new ResponseObjectHandler (this);
					responseObjectHandler.LoadFromHashtable(handler);
					responseObjectHandler.SetWriteToDisk();

					m_requestCache.Add((string)pair.Key, responseObjectHandler);
				}
			}
		}

		private void ReadInCachedObjects(Hashtable cachedObjects)
		{
			if (cachedObjects != null)
			{
				foreach (DictionaryEntry pair in cachedObjects)
				{
					Hashtable dataObjectTable = cachedObjects[pair.Key] as Hashtable;

					ServerObjectBase dataObject = ServerObjectFactory.CreateInstance(dataObjectTable["object_type"] as string);

					if (dataObject == null)
					{
						Debugger.Log("ServerObject has been setup incorrectly!", Debugger.Severity.ERROR, (int)SharedSystems.Systems.SERVER_CACHE);
					}
					else
					{
						dataObject.ParseResponse(dataObjectTable, true);
						dataObject.SetWriteToDisk();

						CacheItem item = new CacheItem {
							objectData = dataObject,
							createdTime = Utils.Date.GetEpochTimeMills()
						};

						if (!m_cachedObjects.ContainsKey(pair.Key.ToString()))
						{
							m_cachedObjects.Add(pair.Key.ToString(), item);
						}
					}
				}
			}
		}

		private void AddRequest(string key, Kanga.ResponseObjectHandler objHandler)
		{
			if (!m_requestCache.ContainsKey(key))
			{
				m_requestCache.Add(key, objHandler);
			}
			else
			{
				m_requestCache[key] = objHandler;
			}
		}

		public ResponseObjectHandler GetCachedObjectHandler(QueueItem queueItem)
		{
			// Check for specified object IDs in the object cahce and generate a response object handler if we find them and their dependancies
			ResponseObjectHandler ret = CheckForCachedObjects(queueItem);

			if (ret == null)
			{		
				// object cahce has failed or most likely the object IDs were not specified so see if we have made this request before
				ret = CheckForCachedRequest(queueItem);
			}

			if (ret != null)
			{
				// we have a response object so lets check any cache search criteria that we need to poke into the resonse
				ret.SearchCache(queueItem.request);
			}
			else if (queueItem.request.cacheSearch != null)
			{
				// we have failed to find IDs and we have not made this request before, search the cache for missing objects we have locally and create a response if we find them
				List<ServerObjectBase> foundObjects = SearchCache(queueItem.request.cacheSearch);

				if (foundObjects.Count > 0)
				{
					ret = new ResponseObjectHandler (foundObjects.ToArray(), new ServerObjectBase[0], this);	

					m_requestCache.Add(queueItem.cacheKey, ret);
				}
			}

			return ret;
		}

		private ResponseObjectHandler CheckForCachedRequest(QueueItem queueItem)
		{
			ResponseObjectHandler ret = null;

			if (m_requestCache.ContainsKey(queueItem.cacheKey))
			{					
				ret = m_requestCache[queueItem.cacheKey];
				ret.UpdateFromCacheFallback();
			}

			return ret;
		}

		private ResponseObjectHandler CheckForCachedObjects(QueueItem queueItem)
		{
			Request incomingRequest = queueItem.request;
			ResponseObjectHandler ret = null;

			if (incomingRequest.cacheObjectIDs != null && incomingRequest.cacheObjectIDs.Length > 0)
			{				
				List<string> helperObjectKeys = new List<string> ();

				bool hasAllCachedObjects = HasAllCacheObjects(incomingRequest, ref helperObjectKeys);

				if (hasAllCachedObjects)
				{
					ret = GenerateHandlerWithCachedObjects(incomingRequest, helperObjectKeys);
					m_requestCache[queueItem.cacheKey] = ret;
				}
			}

			return ret;
		}

		private bool HasAllCacheObjects(Request incomingRequest, ref List<string> helperObjectKeys)
		{
			bool ret = true;

			for (int i = 0; i < incomingRequest.cacheObjectIDs.Length; i++)
			{
				if (ret)
				{
					string key = incomingRequest.cacheObjectIDs[i];
					CacheItem item = GetObject(key);
					if (item == null)
					{
						ret = false;
						break;
					}


					string[] dependencies = item.objectData.GetCacheDependecies();
					if (dependencies != null)
					{
						for (int j = 0; j < dependencies.Length; j++)
						{
							string jKey = dependencies[j];
							CacheItem jItem = GetObject(jKey);

							if (jItem == null)
							{
								if (incomingRequest.requireCacheDependencies)
								{
									ret = false;
									break;
								}
							}
							else
							{
								jItem.objectData.Populate();
								helperObjectKeys.Add(jKey);		
							}
						}
					}
				}
				else
				{
					break;
				}
			}

			return ret;
		}

		private ResponseObjectHandler GenerateHandlerWithCachedObjects(Request incomingRequest, List<string> helperObjectKeys)
		{
			ResponseObjectHandler ret = null;

			ServerObjectBase[] hits = new ServerObjectBase[incomingRequest.cacheObjectIDs.Length];
			ServerObjectBase[] helperObjects = helperObjectKeys.Count > 0 ? new ServerObjectBase[helperObjectKeys.Count] : null;

			long oldestObjectTime = Utils.Date.GetEpochTimeMills();
			for (int i = 0; i < incomingRequest.cacheObjectIDs.Length; i++)
			{
				CacheItem item = GetObject(incomingRequest.cacheObjectIDs[i]);
				if (item != null)
				{
					hits[i] = item.objectData;

					if (item.createdTime < oldestObjectTime)
					{
						oldestObjectTime = item.createdTime;
					}
				}
			}

			int j = 0;
			foreach (string key in helperObjectKeys)
			{
				CacheItem item = GetObject(key);

				if (item != null)
				{
					helperObjects[j] = item.objectData;

					if (item.createdTime < oldestObjectTime)
					{
						oldestObjectTime = item.createdTime;
					}
				}

				j++;
			}

			ret = new ResponseObjectHandler (hits, helperObjects, this, oldestObjectTime);

			return ret;
		}

		private CacheItem GetObjectWithinTimeout(string key, long maxTimeout)
		{
			CacheItem ret = null;
			CacheItem item = GetObject(key);
			if (item != null)
			{
				long cacheTime = Utils.Date.GetEpochTimeMills() - item.createdTime;

				if (cacheTime < maxTimeout)
				{
					ret = item;
				}
			}

			return ret;
		}

		public void RemoveObject(string key)
		{
			if (m_cachedObjects.ContainsKey(key))
			{
				m_cachedObjects.Remove(key);
			}
		}

		public void SetCacheItem(string key, CacheItem item)
		{
			if (!m_cachedObjects.ContainsKey(key))
			{
				m_cachedObjects.Add(key, item);
			}
			else
			{
				CacheItem cachedItem = m_cachedObjects[key];
				if (item.objectData.IsNewer(cachedItem.objectData))
				{
					//	maintain isimportant flag
					item.isImportant = cachedItem.isImportant;

					m_cachedObjects[key] = item;
				}
				else
				{
					// 	Even if we don't stuff the cache here we at least need to update the cache time to now, as the object in the cache is up to date. 
					//	This way then when we next do a request for this object then the cache time-out check will be correct.
					cachedItem.createdTime = item.createdTime;
				}
			}
		}

		public void SetCacheItemImportant(string key, bool isImportant)
		{
			if (m_cachedObjects.ContainsKey(key))
			{
				CacheItem cachedItem = m_cachedObjects[key];
				cachedItem.isImportant = isImportant;
			}
		}

		public CacheItem SetObject(string key, ServerObjectBase val)
		{
			CacheItem item = new CacheItem {
				objectData = val,
				createdTime = Utils.Date.GetEpochTimeMills()
			};
		
			SetCacheItem(key, item);

			return item;
		}

		public CacheItem GetObject(string key)
		{
			if (key == null)
			{
				Debugger.Log("'key' is null!!", Debugger.Severity.ERROR, (int)SharedSystems.Systems.SERVER_CACHE);
				return null;
			}

			if (m_cachedObjects.ContainsKey(key))
			{
				return m_cachedObjects[key];
			}

			return null;
		}

		public void PrintImportantItems()
		{
			Debugger.Log("**********************************", (int)SharedSystems.Systems.SERVER_CACHE);
			Debugger.Log("Cache items marked as important:  ", (int)SharedSystems.Systems.SERVER_CACHE);

			Dictionary<string, CacheItem>.KeyCollection objectDict = m_cachedObjects.Keys;

			foreach (string key in objectDict)
			{
				CacheItem item = m_cachedObjects[key];
				if (item.isImportant)
				{
					Debugger.Log(item.objectData.GetObjectID(), (int)SharedSystems.Systems.SERVER_CACHE);	
				}
			}

			Debugger.Log("**********************************", (int)SharedSystems.Systems.SERVER_CACHE);
		}
	}
}
