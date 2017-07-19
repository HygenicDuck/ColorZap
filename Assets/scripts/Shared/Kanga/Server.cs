using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using Utils;
using System;


namespace Kanga
{
	public class Server : MonoSingleton<Server>
	{
		public const int NO_CODE_RETURNED = -1;
		public const int NOTHING = 0;
		public const int REQUEST_SUCCESS = 200;
		public const int CREATE_SUCCESS = 201;
		public const int UPDATE_SUCCESS = 203;
		public const int AUTH_SUCCESS = 205;
		public const int NOT_FOUND = 404;
		public const int CONFLICT = 409;
		public const int LIVE_REDIRECT_ERROR = 421;
		public const int LOCKED = 423;
		public const int VERSION_ERROR = 426;
		public const int SOFT_SERVER_MAINTENANCE = 428;
		public const int MAINTENANCE_ERROR = 503;
		public const int MAX_RETRIES = 2;

		// Client errors
		public const int REQUEST_TIMEOUT = -100;
		public const int NETWORK_LOSS = -101;

		private const float QUEUE_REFRESH_TIME = 0.5f;
		private const float QUEUE_REFRESH_TIME_NO_NETWORK = 5f;




		#if UNITY_ANDROID || UNITY_EDITOR
		
		#else
		private X509Certificate2 m_rootCert;
		private X509Certificate2 m_intermediateCert;
		#endif
		protected Dictionary<string,QueueItem> m_requestQueue = new Dictionary<string,QueueItem> ();
		private bool m_queueActive = false;
		private bool m_authSessionAvailable = false;
		public string m_baseURI{ set; get; }
		private ServerCache m_serverCache;
		private bool m_networkLoss = false;
		private bool m_queuePaused = false;
		private long m_lastServerTimeOffset = 0;
		private bool m_isNewVersion = false;

		private AuthDetails m_authDetails = new AuthDetails ();

		public Server()
		{
			m_serverCache = new ServerCache (this);
		}

		~Server()
		{
		}

		protected override void Init()
		{
		}

		public AuthDetails AuthDetails
		{
			get
			{
				return m_authDetails;
			}
		}

		public bool IsNewVersion
		{
			get
			{
				return m_isNewVersion;
			}
		}

		public bool NetworkLoss
		{
			get
			{
				return m_networkLoss;
			}
		}

		public ServerCache GetServerCache()
		{
			return m_serverCache;
		}

		void OnApplicationQuit()
		{
			m_serverCache.WriteOutCache();
		}

		public void OnApplicationLoad()
		{
			if (AuthDetails.authGameEditionVersion == Utils.Auth.GetApplicationVersion())
			{
				Debugger.Log("Server::OnApplicationLoad application version the same", (int)SharedSystems.Systems.KANGA);
				m_serverCache.LoadCache();
			}
			else
			{
				Debugger.Log("Server::OnApplicationLoad new application version: " + Utils.Auth.GetApplicationVersion() + " from version: " + AuthDetails.authGameEditionVersion, (int)SharedSystems.Systems.KANGA);
				m_isNewVersion = true;
				NukeLocalCache();
				AuthDetails.authGameEditionVersion = Utils.Auth.GetApplicationVersion();
				AuthDetails.SaveToPlayerPreferences();
			}
		}

		public virtual void RegisterServerObjects()
		{
			ServerObjectFactory.RegisterType<KangaObjects.Product>("product");
			ServerObjectFactory.RegisterType<KangaObjects.Bank>("bank");
			ServerObjectFactory.RegisterType<KangaObjects.BankTransaction>("bank_transaction");
			ServerObjectFactory.RegisterType<KangaObjects.Currency>("currency");
		}

		public void ClearCache()
		{
			m_serverCache.Clear();
		}

		public void NukeLocalCache()
		{
			m_serverCache.NukeLocalCache();
		}

		public void WriteOutCache()
		{
			m_serverCache.WriteOutCache();
		}

		public T GetObjectFromCache<T>(string key) where T: ServerObjectBase
		{	
			T ret = GetObjectFromCache(key) as T;

			return ret;
		}

		public ServerObjectBase GetObjectFromCache(string key)
		{
			ServerObjectBase ret = null;
			ServerCache.CacheItem item = m_serverCache.GetObject(key);

			if (item != null)
			{
				item.objectData.Populate();
				ret = item.objectData;
			}

			return ret;
		}

		public T CreateServerObject<T>()  where T: ServerObjectDocument, new()
		{
			return m_serverCache.CreateServerObject<T>();
		}

		public void RemoveServerObject(ServerObjectDocument serverObjectDocument)
		{
			m_serverCache.RemoveObject(serverObjectDocument.objectID);
		}

		public void SetCacheItemImportant(string key, bool isImportant)
		{
			m_serverCache.SetCacheItemImportant(key, isImportant);
		}

		public void SetChainCerts(string rootCert, string intermediateCert)
		{
			#if UNITY_ANDROID || UNITY_EDITOR
			#else
			m_rootCert = new X509Certificate2 (rootCert);
			m_intermediateCert = new X509Certificate2 (intermediateCert);
			HTTP.CertVerifier.verifier = new HTTP.CertVerifier.CertVerifierCB (CertVerifier);
			#endif
		}

		public bool CertVerifier(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			#if UNITY_ANDROID || UNITY_EDITOR
			return true;

			#else
			var pinnedChain = new X509Chain (false);

			pinnedChain.ChainPolicy.ExtraStore.Add(m_rootCert);
			pinnedChain.ChainPolicy.ExtraStore.Add(m_intermediateCert);

			pinnedChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
			pinnedChain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;

			var primaryCert = new X509Certificate2 (certificate);
			pinnedChain.Build(primaryCert);

			if (pinnedChain.ChainStatus.Length == 1 &&
			    pinnedChain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot)
			{
				return true;
			}
			else
			{
				// not valid for one or more reasons
				Debugger.Log("SSL CERTIFICATE NOT VALID", Debugger.Severity.WARNING);
				return false;
			}
			#endif
		}

		public bool LoadFromPlayerPreferences()
		{
			m_authDetails.LoadFromPlayerPreferences();
			if (System.String.Equals(m_authDetails.authPlayerId, ""))     // No auth
			{
				return false;
			}

			m_authSessionAvailable = true;
			return true;
		}

		public Hashtable CreateAuthPayload(RequestData requestData, string uri)
		{
			Hashtable payload = new Hashtable ();
			payload.Add("install_id", m_authDetails.authInstallId.ToString());
			payload.Add("game_id", m_authDetails.authGameId);
			payload.Add("game_edition_id", m_authDetails.authGameEditionId);
			payload.Add("game_edition_version", m_authDetails.authGameEditionVersion);
			payload.Add("auth_resource_variant", m_authDetails.authResourceVariant);
			payload.Add("utc_offset", m_lastServerTimeOffset);

			requestData.time = Utils.Date.GetEpochTimeMills();
			requestData.uri = uri;

			payload.Add("request_data", requestData.GetJson());

			if (!System.String.Equals(m_authDetails.authCreationKey, ""))
			{
				payload.Add("request_data_hash", Utils.Auth.GetHMAC(m_authDetails.authCreationKey, (string)payload["request_data"]));
			}

			if (!System.String.Equals(m_authDetails.authPlayerId, ""))
			{
				payload.Add("auth_player_id", m_authDetails.authPlayerId);
			}

			return payload;
		}

		public void StartQueue()
		{
			m_queueActive = true;
			Utils.CoroutineHelper.Instance.Run(QueueTicker());
		}

		public void PauseQueue()
		{
			m_queuePaused = true;
		}

		public void UnPauseQueue()
		{
			m_queuePaused = false;
		}
			
		IEnumerator QueueTicker()
		{
			while (m_queueActive)
			{
				float timeToWait = QUEUE_REFRESH_TIME;

				// ease off the pressure if there is a network loss
				if (m_networkLoss)
				{
					timeToWait = QUEUE_REFRESH_TIME_NO_NETWORK;
				}

				yield return new WaitForSeconds (timeToWait);

				if (!m_queuePaused)
				{
					CheckQueue();
				}
			}
		}

		public void FlushQueue()
		{
			if (!m_queuePaused)
			{
				CheckQueue ();
			}
		}

		private void CheckQueue()
		{
			if (m_requestQueue.Count > 0)
			{
				bool deleteQueue = true;
				foreach (string key in m_requestQueue.Keys)
				{
					QueueStatus queueStatus = m_requestQueue[key].status;
					if (queueStatus == QueueStatus.NEW_REQUEST || queueStatus == QueueStatus.REFRESH || queueStatus == QueueStatus.REPEAT)
					{
						if (m_authSessionAvailable)
						{
							AuthRequest(m_requestQueue[key]);
						}
						else
						{
							// TODO NO session, handle errors and special cases (newPlayer Event, getStrings, createPlayer)	
							AuthRequest(m_requestQueue[key]);
						}
						deleteQueue = false;
					}
					else if (queueStatus == QueueStatus.IN_PROGRESS)
					{
						deleteQueue = false;
					}
					else
					{
						//case CANCELED, TIMEOUT, ERROR, FINISHED
					}
				}

				if (deleteQueue)
				{
					m_requestQueue.Clear();
				}
			}
		}

		private bool CheckServerKnownErrors(HTTP.Request request, QueueItem queueItem)
		{
			bool interruptRequest = false;
			if (m_queuePaused == false)
			{
				if (request != null && request.response != null && request.response.status == LIVE_REDIRECT_ERROR) // 421
				{
					ProcessLiveRedirect();
					RetryQueueItem(queueItem);
					interruptRequest = true;
				}
				else if (request != null && request.response != null && request.response.status == VERSION_ERROR) // 426
				{
					ProcessUpdateAvailable();
					interruptRequest = m_queuePaused;
				}
				else if (request != null && request.response != null && (request.response.status == MAINTENANCE_ERROR || request.response.status == SOFT_SERVER_MAINTENANCE)) // 503, 428
				{
					ProcessServerMaintenance();
					RetryQueueItem(queueItem);
					interruptRequest = m_queuePaused;
				}
				else if (request != null && request.response != null && request.response.status == LOCKED) // 423
				{
					ProcessUserLocked();
					interruptRequest = m_queuePaused;
				}
			}

			return interruptRequest;
		}

		protected virtual void ProcessLiveRedirect()
		{
			PauseQueue();
		}

		protected virtual void ProcessUpdateAvailable()
		{
			PauseQueue();
		}

		protected virtual void ProcessServerMaintenance()
		{
			PauseQueue();
		}

		protected virtual void ProcessUserLocked()
		{
			PauseQueue();
		}

		//TODO: clean up / optimize this. Or there may be a system function to do all this already
		private static string SerializePayload(Hashtable payload)
		{
			string ret = "";

			foreach (DictionaryEntry pair in payload)
			{
				ret = ret + WWW.EscapeURL((string)pair.Key);
				ret = ret + "=";

				if (pair.Value != null)
				{
					if (!(pair.Value is string))
					{
						ret = ret + WWW.EscapeURL((string)pair.Value.ToString());
					}
					else
					{
						ret = ret + WWW.EscapeURL((string)pair.Value);	
					}
				}
				else
				{
					ret = ret + WWW.EscapeURL("null");
				}

				ret = ret + "&";
			}

			ret = ret.Substring(0, ret.Length - 1);

			return ret;
		}

		private IEnumerator TimeoutRequest(QueueItem queueItem)
		{
			yield return new WaitForSeconds (queueItem.responseTimeout);

			Hashtable error = new Hashtable ();
			error.Add("code", REQUEST_TIMEOUT);
			error.Add("status", "Max specified timeout reached");

			queueItem.status = QueueStatus.TIMEOUT;

			foreach (QueueCallbacks callbacks in queueItem.callbacks)
			{
				if (callbacks.error != null)
				{
					callbacks.error(error);
				}
			}
		}

		private void AuthRequest(QueueItem queueItem)
		{
			// Remake queuItem URI to account for baseURL changes
			string uri = CreateAuthURLPrefix(queueItem.request.endpoint, queueItem.request.uriObject, queueItem.request.action);
			queueItem.uri = uri;

			Hashtable payload = CreateAuthPayload(queueItem.request.requestData, queueItem.uri);

			if (queueItem.request.method == "GET")
			{						
				string serialisedPayload = SerializePayload(payload);
				queueItem.uri = queueItem.uri + "?" + serialisedPayload;
			}
			else
			{
				queueItem.uri = queueItem.uri + (String.IsNullOrEmpty(m_authDetails.authPlayerId) ? "" : ("?auth_player_id=" + m_authDetails.authPlayerId));
			}

			queueItem.status = QueueStatus.IN_PROGRESS;

			if (queueItem.request.unAuthPayload != null)
			{  // add extra payload data outside the auth payload
				foreach (string key in queueItem.request.unAuthPayload.Keys)
				{
					payload[key] = queueItem.request.unAuthPayload[key];
				}
			}

			Debugger.Log("Method: " + queueItem.request.method + ", URI: " + queueItem.uri, (int)SharedSystems.Systems.HTTP_REQUEST);

			HTTP.Request theRequest = null;
			if (queueItem.request.method == "GET")
			{
				theRequest = new HTTP.Request (queueItem.request.method, queueItem.uri);
			}
			else
			{
				theRequest = new HTTP.Request (queueItem.request.method, queueItem.uri, payload);

				Debugger.LogHashtableAsJson("payload", payload, (int)SharedSystems.Systems.HTTP_REQUEST);
			}

			if (queueItem.responseTimeout > 0f)
			{
				queueItem.timeoutCoroutine = StartCoroutine(TimeoutRequest(queueItem));
			}
				
			theRequest.Send((request) => {
				if (queueItem.status == QueueStatus.TIMEOUT)
				{
					return;
				}

				if (queueItem.responseTimeout > 0f)
				{
					StopCoroutine(queueItem.timeoutCoroutine);
				}

				if (queueItem.status == QueueStatus.CANCELED)
				{
					return;
				}

				if (CheckServerKnownErrors(request, queueItem))
				{
					return;
				}

				//loss of network, set the item to retry...
				if (request.response == null)
				{
					if (!m_networkLoss)
					{
						m_networkLoss = true;
						Utils.KangaEvents.SendNetworkStatusChangedEvent(m_networkLoss);
					}

					Debugger.Log("No response, is the network down?", (int)SharedSystems.Systems.KANGA);

					RetryQueueItem(queueItem);

					return;
				}

				if (m_networkLoss)
				{
					m_networkLoss = false;
					Utils.KangaEvents.SendNetworkStatusChangedEvent(m_networkLoss);
				}

				Debugger.Log("Method: " + queueItem.request.method + ", URI: " + queueItem.uri, (int)SharedSystems.Systems.HTTP_RESPONSE);
				Debugger.Log("Text: " + request.response.Text, (int)SharedSystems.Systems.HTTP_RESPONSE);

				Hashtable returnObject = (Hashtable)JSON.JsonDecode(request.response.Text);
				if (string.Equals(returnObject, "") || returnObject == null)
				{
					Hashtable error = new Hashtable ();
					error.Add("code", request.response.status);
					error.Add("status", request.response.message);

					QueueItemError(queueItem, error);
				}
				else if (returnObject.ContainsKey("message"))
				{
					Hashtable error = new Hashtable ();
					string message = (string)returnObject["message"];
					int status = (int)returnObject["status"];

					error.Add("code", status);
					error.Add("status", message);

					QueueItemError(queueItem, error);
				}
				else
				{
					Hashtable response = (Hashtable)JSON.JsonDecode((string)returnObject["response"]);
					Hashtable status = (Hashtable)response["status"];
					Hashtable resources = (Hashtable)response["resources"];
					if (response["time"] != null)
					{
						long time = Convert.ToInt64(response["time"]);
						m_lastServerTimeOffset = time - Utils.Date.GetEpochTimeMills();
					}
						
					if (status["code"] == null)
					{
						status.Add("code", NO_CODE_RETURNED);
					}

					if (CheckStatusCodeSuccess((int)status["code"]))
					{
						queueItem.status = QueueStatus.FINISHED;

//						Debugger.PrintHashTableKeys(resources, "all KEYS:", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableAsServerObject(resources, "all:", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableKeys((Hashtable)resources["helper_objects"], "HELPER OBJECT keys", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableKeys((Hashtable)resources["extra_data"], "EXTRA DATA keys", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableKeys((Hashtable)resources["hits"], "HITS keys", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableAsServerObject((Hashtable)resources["helper_objects"], "HELPER OBJECTS :" + queueItem.uri + "\nhelper_objects: ", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableAsServerObject((Hashtable)resources["hits"], "HITS :" + queueItem.uri + "\nhits: ", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
//						Debugger.PrintHashTableAsServerObject((Hashtable)resources["extra_data"], "EXTRA_DATA :" + queueItem.uri + "\nhits: ", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);
					
						// if there was a dependant server object attached to this request then we should of got it back with the response so it needs removing from the cache
						if (queueItem.dependantObject != null)
						{
							m_serverCache.RemoveObject(queueItem.dependantObject.GetObjectID());
						}

						ResponseObjectHandler objHandler = m_serverCache.CreateObjectHandler(resources, queueItem);

						objHandler.SearchCache(queueItem.request);

						foreach (QueueCallbacks callbacks in queueItem.callbacks)
						{
							if (callbacks.finished != null)
							{
								try
								{
									callbacks.finished(objHandler);
								}
								catch(Exception ex)
								{
									Debugger.Log("Error calling a success callback for uri: " + queueItem.uri + ", message: " + ex.Message);
								}
							}
						}
					}
					else
					{
						if ((int)status["code"] == NOTHING)
						{
						}
						else
						{
							Hashtable error = new Hashtable ();
							error.Add("code", status["code"]);
							error.Add("status", status["message"]);
							error.Add("resources", resources);

//							Debugger.PrintHashTableAsServerObject(error, "FAILED " + queueItem.uri + "\n ERROR: ", (int)WaffleSystems.Systems.WAFFLE_IRON_RESPONSES);

							QueueItemError(queueItem, error);
						}
					}
				}
			});
		}

		private void RetryQueueItem(QueueItem queueItem)
		{
			if ((queueItem.numRetries >= MAX_RETRIES) && (queueItem.dependantObject == null))
			{
				Hashtable error = new Hashtable ();
				error.Add("code", NETWORK_LOSS);
				error.Add("status", "Max retries exceeded");

				QueueItemError(queueItem, error);
			}
			else
			{
				queueItem.status = QueueStatus.REPEAT;
				queueItem.numRetries++;	
			}
		}

		public long GetServerAdjustedEpochTimeMills()
		{
			long time = Utils.Date.GetEpochTimeMills() + m_lastServerTimeOffset;
			return time;
		}

		private bool CheckStatusCodeSuccess(int statusCode)
		{
			switch (statusCode)
			{
			case REQUEST_SUCCESS:
			case CREATE_SUCCESS:
			case AUTH_SUCCESS:
			case UPDATE_SUCCESS:
				{
					return true;
				}
			default:
				{
					return false;
				}
			}
		}
		private void QueueItemError(QueueItem queueItem, Hashtable error)
		{
			queueItem.status = QueueStatus.ERROR;

			switch ((int)error["code"])
			{
			case CONFLICT:
			case NOT_FOUND:
			case REQUEST_TIMEOUT:
			case NETWORK_LOSS:
			case LOCKED:
				break;
			default:
				
				string args = "";

				foreach (var arg in queueItem.request.requestData.args)
				{
					args += ", " + arg.ToString();
				}

				Debugger.Error(error["code"] + ", " + error["status"] + " " + queueItem.request.method + "/" + queueItem.request.endpoint + "/" + queueItem.request.action + "/" + queueItem.request.uriObject + ", " + Debugger.ServerObjectAsString(queueItem.request.requestData.args));

				if (queueItem.request.unAuthPayload != null)
				{
					Debugger.Warning("extrapayload: " + Debugger.ServerObjectAsString(queueItem.request.requestData.args));
				}

				break;
			}
				
			foreach (QueueCallbacks callbacks in queueItem.callbacks)
			{
				if (callbacks.error != null)
				{
					callbacks.error(error);
				}
			}
		}
			
		private string CreateQueueKey(QueueItem queueItem)
		{
			string encodedArgs = JSON.JsonEncode(queueItem.request.requestData.args);
			if (!string.IsNullOrEmpty(encodedArgs))
			{
				return Utils.Auth.GetHMAC(queueItem.uri, encodedArgs);
			}
			else
			{
				string argsString = Debugger.ServerObjectAsString(queueItem.request.requestData.args);
				Debugger.Error("Server::CreateQueueKey() encodedArgs is null: " + argsString);
				return "";
			}
		}

		private string CreateAuthURLPrefix(string endpoint, string serverObject, string action)
		{
			string authURL = null;
			string authServerURL = m_baseURI;
			if (serverObject != null && !string.Equals(serverObject, ""))
			{
				authURL = authServerURL + endpoint + "/" + serverObject;
			}
			else
			{
				authURL = authServerURL + endpoint;
			}

			if (action != null && !string.Equals(action, ""))
			{
				authURL = authURL + "/" + action;
			}
				
			return authURL;
		}

		public bool KeyExistsAndIsActive(string key)
		{
			return (m_requestQueue.ContainsKey(key) &&
			m_requestQueue[key].status != QueueStatus.FINISHED &&
			m_requestQueue[key].status != QueueStatus.ERROR &&
			m_requestQueue[key].status != QueueStatus.CANCELED &&
			m_requestQueue[key].status != QueueStatus.TIMEOUT);
		}

		protected void UnqueueRequest(string queueKey)
		{
			//TODO: cancel request, now it only kills the callbacks
			if (KeyExistsAndIsActive(queueKey))
			{

				QueueItem queueItem = m_requestQueue[queueKey];

				queueItem.callbacks.Clear();

				if (queueItem.ditchRequestIfCanceled)
				{
					queueItem.status = QueueStatus.CANCELED;
				}
			}
		}

		public void CancelGroupRequest(string groupId)
		{
			foreach (KeyValuePair<string, QueueItem> queueItemPair in m_requestQueue)
			{
				QueueItem queueItem = queueItemPair.Value;

				for (int i = queueItem.callbacks.Count - 1; i >= 0; --i)
				{
					QueueCallbacks callbacks = queueItem.callbacks[i];
					if (callbacks.group == groupId)
					{
						queueItem.callbacks.Remove(callbacks);

						if (queueItem.ditchRequestIfCanceled && queueItem.callbacks.Count == 0)
						{
							//Only cancel if all the callbacks has been removed.
							queueItem.status = QueueStatus.CANCELED;
						}
					}
				}
			}
		}

		public void MarkRequestAsTrashable(string requestId, bool ditch)
		{
			if (m_requestQueue.ContainsKey(requestId))
			{
				m_requestQueue[requestId].ditchRequestIfCanceled = ditch;
			}
		}

		public string QueueRequest(
			Request request, 
			string groupId, 
			System.Action<Hashtable> errorCallback, 
			System.Action<Kanga.ResponseObjectHandler> finishedCallback, 
			System.Action<Kanga.ResponseObjectHandler> cacheCallback = null, 
			long cacheTimeout = 0, 
			ServerObjectBase serverObject = null, 
			float responseTimeout = -1f)
		{
			if (!m_queueActive)
			{
				Debugger.Error("QueueRequest called on server before queue is active");
			}

			string uri = CreateAuthURLPrefix(request.endpoint, request.uriObject, request.action);

//			Debugger.PrintHashTableAsServerObject(request.requestData.args, "QueueRequest :" + uri + "\n" , (int)WaffleSystems.Systems.WAFFLE_IRON_REQUESTS);

			QueueCallbacks requestCallbacks = new QueueCallbacks {
				finished = finishedCallback,
				error = errorCallback,
				cache = cacheCallback,
				group = groupId
			};

			QueueItem queueItem = new QueueItem {
				uri = uri,
				request = request,
				dependantObject = serverObject
			};

			if (serverObject != null)
			{
				serverObject.QueueItemDependancy = queueItem;
			}

			string queueKey = CreateQueueKey(queueItem);

			queueItem.cacheKey = queueKey;
			queueItem.cacheTimeout = cacheTimeout;
			queueItem.responseTimeout = responseTimeout;
			queueItem.callbacks.Add(requestCallbacks);
			queueItem.status = QueueStatus.NEW_REQUEST;

			ResponseObjectHandler cachedObjHandler = m_serverCache.GetCachedObjectHandler(queueItem);

			if (cachedObjHandler != null)
			{
				if (!request.cacheToDisk && cachedObjHandler.ShouldWriteToDisk())
				{
					request.cacheToDisk = true;
				}

				if (request.cacheToDisk)
				{
					cachedObjHandler.SetWriteToDisk();
				}

				long cacheTime = Utils.Date.GetEpochTimeMills() - cachedObjHandler.GetCreationTime();
				bool withinCacheTime = (cacheTime < queueItem.cacheTimeout);
				bool finished = false;

				foreach (QueueCallbacks callbacks in queueItem.callbacks)
				{
					// if the request is still within the timeout then the data is still valid and we call the finish callback
					if (withinCacheTime)
					{
						callbacks.finished(cachedObjHandler);
						finished = true;
					}
					else if (callbacks.cache != null) // if we have a cache callback then we can hit it with old data and eventually the original request will update it with valid data
					{
						callbacks.cache(cachedObjHandler);
					}
				}

				if (finished)
				{
					queueItem.status = QueueStatus.FINISHED;
					return queueKey;
				}						
			}

			if (KeyExistsAndIsActive(queueKey))  // Key already exists in queue, repeated url query?
			{
//				Debugger.Log("Key exists and is Active: " + queueItem.uri, (int)WaffleSystems.Systems.WAFFLE_IRON_REQUESTS);
				m_requestQueue[queueKey].callbacks.Add(requestCallbacks);
			}
			else
			{
				m_requestQueue[queueKey] = queueItem;
			}

			return queueKey;
		}
			
		public void CreatePlayer(Hashtable unAuthPayload, System.Action<Hashtable> errorCallback, System.Action<Kanga.ResponseObjectHandler> finishedCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "players",
				unAuthPayload = unAuthPayload
			};

			QueueRequest(request, null, errorCallback, finishedCallback);
		}
	}
}