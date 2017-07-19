#if USING_ANALITIC_PROVIDER_APPSFLYER

using System.Collections;
using System.Collections.Generic;
using Utils; 

namespace Analytics
{
	public class AppsFlyerProvider : Provider
	{
		private HashSet<string> m_whiteList;

		public AppsFlyerProvider(HashSet<string> whiteList)
		{
			m_providerName = "AppsFlyer";

			m_whiteList = whiteList;
		}

		public override void StartSession()
		{
			//TODO
		}

		public override void FlushSession()
		{
			//TODO
		}

		public override void LogAdEvent(string eventName)
		{
			AppsFlyer.trackEvent(eventName, "");
		}

		public override void LogEvent(string eventName, Hashtable parameters)
		{
			if (m_whiteList.Contains(eventName))
			{
				Debugger.Log("AppsFlyer logging: " + eventName, (int)SharedSystems.Systems.ANALYTICS);

				if (parameters != null)
				{
					Dictionary<string, string> dict = new Dictionary<string, string> ();

					foreach (DictionaryEntry pair in parameters)
					{
						dict.Add(pair.Key.ToString(), pair.Value.ToString());
					}

					AppsFlyer.trackRichEvent(eventName, dict);
				}
				else
				{
					AppsFlyer.trackEvent(eventName, "");
				}
			}
		}

		public override void LogErrorEvent(string eventName, string errorDescription)
		{
			//TODO
		}

		public override void StartTimedEvent(string eventName, Hashtable parameters)
		{
			//TODO
		}

		public override void StopTimedEvent(string eventName)
		{
			//TODO
		}

	}
}

#endif