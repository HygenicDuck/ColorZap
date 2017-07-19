using System.Collections;
using System.Collections.Generic;
using Utils;

// #define USING_ANALITIC_PROVIDER_DELTADNA
// #define USING_ANALITIC_PROVIDER_APPSFLYER
// #define USING_ANALITIC_PROVIDER_APPTELIGENT
// #define USING_ANALITIC_PROVIDER_KANGA

namespace Analytics
{
	public static class Dispatcher
	{
		public class Param
		{
			public string name;
			public object value;
		}
			
		public static void SendAdEvent(string eventName)
		{
			Handler.LogAdEvent(eventName);
		}

		public static void SendEvent(string eventName)
		{
			Handler.LogEvent(eventName, null);
		}

		public static void SendEvent(string eventName, string paramName, object paramVal)
		{
			Hashtable parameters = new Hashtable ();

			parameters.Add(paramName, Filter(paramVal));

			Handler.LogEvent(eventName, parameters);
		}

		public static void SendEvent(string eventName, Param[] moreParams)
		{
			Hashtable parameters = new Hashtable ();

			for (int i = 0; i < moreParams.Length; ++i)
			{			
				parameters.Add(moreParams[i].name, Filter(moreParams[i].value));
			}

			Handler.LogEvent(eventName, parameters);
		}

		public static void SendScreenEvent(string screenName)
		{
			SendEvent(Events.UI_SCREEN, "screen_name", screenName);
		}

		public static void SendButtonEvent(string buttonName)
		{
			SendEvent(Events.UI_BUTTON, "button_name", buttonName);
		}

		private static object Filter(object param)
		{
			if (param.GetType() == typeof(string))
			{
				string paramString = (string)param;
				paramString = paramString.Replace('/', '_').Replace('-', '_').Replace(' ', '_');
				param = paramString;
			}
			return param;
		}
	}

	public class Handler
	{
		private static HashSet<string> m_adCampaignWhiteList = new HashSet<string>();
		private static List<Provider> s_providersList = new List<Provider> ();
		private static bool s_enabled = true;
	
		public static void EnableEvents(bool enabled)
		{
			s_enabled = enabled;
		}

		public static void Initialize(string userId = null)
		{
			foreach (Provider provider in s_providersList)
			{
				provider.FlushSession();
			}

			s_providersList.Clear ();

			m_adCampaignWhiteList.Add(Analytics.Events.FTUE_COMPLETED);
			m_adCampaignWhiteList.Add(Analytics.Events.QUIZ_PUBLISHED_CLIENT);
			m_adCampaignWhiteList.Add(Analytics.Events.INVITES_FACEBOOK_SENT);
			m_adCampaignWhiteList.Add(Analytics.Events.QUIZ_PLAYED_CLIENT);

			#if USING_ANALITIC_PROVIDER_KANGA
			RegisterProvider(new KangaProvider ());
			#endif
			RegisterProvider(new FacebookProvider (m_adCampaignWhiteList));
			#if USING_ANALITIC_PROVIDER_APPSFLYER
			RegisterProvider(new AppsFlyerProvider (m_adCampaignWhiteList));
			#endif

			//	Disabling for this sprint so we can pull out some android plugins to get below the 65K ref limit
			//	Will re-enable next sprint

			#if !UNITY_ANDROID
//			RegisterProvider(new DeltaDNAProvider ());
//			RegisterProvider(new ApteligentProvider ());
			#endif

			#if !SERVER_ENVIROMENT_CANDIDATE
			RegisterProvider(new AnalyticDebugProvider ());
			#endif

			SetUserId(userId);
			StartSession();
		}

		public static void RegisterProvider(Provider provider)
		{
			Debugger.Log("Register provider: " + provider.GetName(), (int)SharedSystems.Systems.ANALYTICS);


			s_providersList.Add(provider);
		}

		public static void StartSession()
		{
			Debugger.Log("Start session", (int)SharedSystems.Systems.ANALYTICS);

			foreach (Provider provider in s_providersList)
			{
				provider.StartSession();
			}
		}
		public static void FlushSession()
		{
			Debugger.Log("Flush session", (int)SharedSystems.Systems.ANALYTICS);

			foreach (Provider provider in s_providersList)
			{
				provider.FlushSession();
			}
		}

		public static void LogAdEvent(string eventName)
		{
			if (s_enabled)
			{
				Debugger.Log("Log ad event " + eventName, (int)SharedSystems.Systems.ANALYTICS);

				foreach (Provider provider in s_providersList)
				{
					provider.LogAdEvent(eventName);
				}
			}
		}

		public static void LogEvent(string eventName, Hashtable parameters)
		{
			if (s_enabled)
			{
				Debugger.LogHashtableAsJson("Log event " + eventName, parameters, (int)SharedSystems.Systems.ANALYTICS);

				foreach (Provider provider in s_providersList)
				{
					provider.LogEvent(eventName, parameters);
				}
			}
		}

		public static void LogErrorEvent(string eventName, string description)
		{
			if (s_enabled)
			{
				Debugger.Log("Log error event "+ eventName + ", " + description, (int)SharedSystems.Systems.ANALYTICS);

				foreach (Provider provider in s_providersList)
				{
					provider.LogErrorEvent(eventName, description);
				}
			}
		}

		public static void StartTimeEvent(string eventName, Hashtable parameters)
		{
			if (s_enabled)
			{
				Debugger.Log("Start timed event " + eventName, (int)SharedSystems.Systems.ANALYTICS);

				foreach (Provider provider in s_providersList)
				{
					provider.StartTimedEvent(eventName, parameters);
				}
			}
		}

		public static void StopTimedEvent(string eventName, Hashtable parameters)
		{
			Debugger.Log("Stop event " + eventName, (int)SharedSystems.Systems.ANALYTICS);

			foreach (Provider provider in s_providersList)
			{
				provider.StopTimedEvent(eventName);
			}
		}
			
		public static void SetUserId(string userId)
		{
			Debugger.Log("Start session with user " + userId, (int)SharedSystems.Systems.ANALYTICS);

			foreach (Provider provider in s_providersList)
			{
				provider.SetUserId(userId);
			}
		}
	
	}


}