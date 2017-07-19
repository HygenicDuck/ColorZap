using UnityEngine;
using System.Collections;


// #define USING_ANALITIC_PROVIDER_DELTADNA
// #define USING_ANALITIC_PROVIDER_APPSFLYER

namespace Analytics
{
	public abstract class Provider
	{
		protected string m_providerName;
		protected string m_userId = null;

		public string GetName()
		{
			return m_providerName;
		}
	
		public abstract void StartSession();
		public abstract void FlushSession();

		public abstract void LogAdEvent(string eventName); // Only providers interested in events that trigger ads will use this function
		public abstract void LogEvent(string eventName, Hashtable parameters);
		public abstract void LogErrorEvent(string eventName, string errorDescription);

		public abstract void StartTimedEvent(string eventName, Hashtable parameters);
		public abstract void StopTimedEvent(string eventName);

		public void SetUserId(string userId)
		{
			m_userId = userId;
		}
	}
}