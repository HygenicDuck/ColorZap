#if USING_ANALITIC_PROVIDER_DELTADNA

using UnityEngine;
using System.Collections;
using DeltaDNA;
using System.Collections.Generic; 

namespace Analytics
{
	public class DeltaDNAProvider : Provider
	{
		string m_apiKey;

		public const string LIVE_KEY = "86462214804035263539088621014119";
		public const string PRODUCTION_KEY = "86459504759570979440537995614119";
		public const string COLLECT_URL = "http://collect2911wfflr.deltadna.net/collect/api";
		public const string ENGAGE_URL = "http://engage2911wfflr.deltadna.net";

		private HashSet<string> m_blackList = new HashSet<string>();

		public DeltaDNAProvider()
		{
			m_providerName = "DeltaDNA";

			m_blackList.Add(Analytics.Events.BANNER_VIEWS);
			m_blackList.Add(Analytics.Events.QUIZ_LOADING_SPINNER);
			m_blackList.Add(Analytics.Events.QUESTION_LOADING_SPINNER);
			m_blackList.Add(Analytics.Events.PLAY_RESULT_UPLOADING_SPINNER);
			m_blackList.Add(Analytics.Events.LOGOUT_FACEBOOK);
		
			bool isLive = false;
			#if (SERVER_ENVIROMENT_CANDIDATE && !CANDIDATE_DEBUG)
			isLive = true;
			#endif

			m_apiKey = isLive ? LIVE_KEY : PRODUCTION_KEY;
		}

		public override void StartSession()
		{
			DDNA.Instance.StartSDK(m_apiKey, COLLECT_URL, ENGAGE_URL, m_userId);
		}

		public override void FlushSession()
		{
			//Empty
		}

		public override void LogAdEvent(string eventName)
		{
		}

		public override void LogEvent(string eventName, Hashtable parameters)
		{
			if (m_blackList.Contains(eventName))
			{
				return;
			}

			GameEvent optionsEvent = new GameEvent (eventName);

			if (parameters != null)
			{
				foreach (DictionaryEntry pair in parameters)
				{
					optionsEvent.AddParam((string)pair.Key, pair.Value);
				}
			}
				
			DDNA.Instance.RecordEvent (optionsEvent);
		}

		public override void LogErrorEvent(string eventName, string errorDescription)
		{
			//Empty
		}

		public override void StartTimedEvent(string eventName, Hashtable parameters)
		{
			//Empty
		}

		public override void StopTimedEvent(string eventName)
		{
			//Empty
		}

	}
}
#endif