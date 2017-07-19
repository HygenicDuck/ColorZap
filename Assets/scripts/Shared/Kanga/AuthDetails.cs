using UnityEngine;
using System.Collections;
using Utils;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace Kanga
{
	public enum QueueStatus
	{
		NEW_REQUEST,		// new request, not processed yet
		IN_PROGRESS,		// request sent to server, if too long in this status flag to timeout
		CANCELED,			// request canceled by user, we might not be able to cancel it but callbacks
							// are destroyed
		TIMEOUT,			// request has gone over the max allowed time for a request
		ERROR,
		REPEAT,				// repeat the request with the same hash so that the server can recognise if it's already done it
		FINISHED,			// request has finished, queueItem will be deleted on next process call
		REFRESH				// request will be retried, could be fallen connection (via keep-alive connection)
							// or the result of user retry after error popup (timeout for ex.)
	};


	// TODO Data classes, move somewere else latter
	public class QueueItem
	{
		public long creationTimeStamp = Utils.Date.GetEpochTimeMills (); // queue item create time, used in
																		 // timeout calcs, etc
		public QueueStatus status;	// used to manage the the request progression in the queue, see enum for
									// explanation of statuses
		public string uri;
		public Request request;		// this contains all the payload data, the meat is in here...
		public ServerObjectBase dependantObject;
		public float responseTimeout;		// how long to wait for a response, set in the queue so we can have different
									// timeouts dependent on the request type
		public long cacheTimeout;


		public bool sessionAvailable;   // was a auth session available at the time of the request?
										// if not we need to recalculate the hashes
		public int timeoutRetryCount = 0;  // how may retries have we done on this item, used to prevent infinite
										   // loops on auto-retries
		public bool showPopupForErrors;	   // should this item present a user options popup if it fails?
		public List<QueueCallbacks> callbacks = new List<QueueCallbacks> ();

		public string cacheKey;		//	key used for caching the response of this request

		// TODO HERE BE DRAGONS!
		public Hashtable options = new Hashtable ();

		public bool ditchRequestIfCanceled = false;

		public int numRetries = 0;
		public Coroutine timeoutCoroutine;
	}

	public class QueueCallbacks
	{
		public System.Action<ProgressDetail> progress;
		public System.Action<Kanga.ResponseObjectHandler> finished;  // called with return data when the callback is finished
		public System.Action<Hashtable> error;     // called when a error is triggered, will have a error type and server status
		public System.Action<Kanga.ResponseObjectHandler> cache;		// called right away if we have a cached value, if cached value is under the
										// timeout the request won't be sent to the server.
										// if the timeout value is over, the cache action still returns the cached data,
										// but the request will proceed to the server and later a finished action will be
										// called with the new data
		public string group; //The associated cancelation group
	}

	public class ProgressDetail
	{
		public string status;
		public float completion;
		public Hashtable partialData;
	}
		
	public class CacheSearchCriteria
	{
		public Type type;
		public Dictionary<string, string> fieldData = new Dictionary<string, string>();
	}

	public class Request
	{
		public string method = "GET";
		public string endpoint;
		public string uriObject;
		public string action;
		public RequestData requestData = new RequestData ();
		public Hashtable unAuthPayload; 
		public string[] cacheObjectIDs;
		public bool requireCacheDependencies = true;
		public CacheSearchCriteria cacheSearch;
		public bool cacheToDisk = false;
	}

	public class RequestData
	{
		public Hashtable args = new Hashtable();
		public long time = Utils.Date.GetEpochTimeMills ();
		public string uri;

		public Hashtable GenerateHashTable ()
		{
			Hashtable returnObject = new Hashtable ();
			returnObject.Add ("args", args);
			returnObject.Add ("time", time);
			returnObject.Add ("uri", uri);

			return returnObject;
		}

		public string GetJson ()
		{
			return JSON.JsonEncode(GenerateHashTable());
		}
	}

	public class AuthPayload
	{
		public string install_id;
		public string game_id;
		public string game_edition_id;
		public string game_edition_version;
		public string request_data;
	}


	public class AuthDetails {

		[DllImport("__Internal")]
		private static extern int _CopyStandardDefaultsToShared();

		public string authCreationKey{ set; get; }
		public string authPlayerId{ set; get; }
		public string authInstallId{ set; get; }
		public string authGameId{ set; get; }
		public string authGameEditionId{ set; get; }
		public string authGameEditionVersion{ set; get; }
		public string facebookAccessToken{ set; get; }
		public string authResourceVariant{ set; get; }

		public void initAuthInstallId ( int seed )
		{
			authInstallId = Utils.Auth.GetHMAC (Utils.Date.GetEpochTimeMills().ToString(), seed.ToString());
		}

		public void LoadFromPlayerPreferences ()
		{
			authCreationKey = PlayerPrefs.GetString ("authCreationKey", "");
			authPlayerId = PlayerPrefs.GetString ("authPlayerId", "");
			authInstallId = PlayerPrefs.GetString ("authInstallId", "");
			authGameId = PlayerPrefs.GetString ("authGameId", "");
			authGameEditionId = PlayerPrefs.GetString ("authGameEditionId", "");
			authGameEditionVersion = PlayerPrefs.GetString ("authGameEditionVersion", Utils.Auth.GetApplicationVersion());
			facebookAccessToken = PlayerPrefs.GetString ("facebookAccessToken", "");
			authResourceVariant = PlayerPrefs.GetString ("authResourceVariant", "");
		}

		public void SaveToPlayerPreferences ()
		{
			PlayerPrefs.SetString ("authCreationKey", authCreationKey);
			PlayerPrefs.SetString ("authPlayerId", authPlayerId);
			PlayerPrefs.SetString ("authInstallId", authInstallId);
			PlayerPrefs.SetString ("authGameId", authGameId);
			PlayerPrefs.SetString ("authGameEditionId", authGameEditionId);
			PlayerPrefs.SetString ("authGameEditionVersion", authGameEditionVersion);
			PlayerPrefs.SetString ("facebookAccessToken", facebookAccessToken);
			PlayerPrefs.Save ();

			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_CopyStandardDefaultsToShared();
			}
		}

		public void Logout ()
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save ();

			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_CopyStandardDefaultsToShared();
			}
		}
	}
}
