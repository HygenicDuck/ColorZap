#if USING_ANALITIC_PROVIDER_APPTELIGENT
using UnityEngine;
using System.Collections;

namespace Analytics
{
	public class ApteligentProvider : Provider
	{

		public ApteligentProvider()
		{
			m_providerName = "ApteligentProvider";
		}

		public override void StartSession()
		{
			Crittercism.SetUsername(m_userId);
		}

		public override void FlushSession()
		{
			// Empty
		}

		public override void LogAdEvent(string eventName)
		{
		}

		public override void LogEvent(string eventName, Hashtable parameters)
		{
			Crittercism.LeaveBreadcrumb(eventName + JSON.JsonEncode(parameters));
		}

		public override void LogErrorEvent(string eventName, string errorDescription)
		{
			Crittercism.LeaveBreadcrumb(eventName + " " + errorDescription);
		}

		public override void StartTimedEvent(string eventName, Hashtable parameters)
		{
			Crittercism.BeginUserflow(eventName);
		}

		public override void StopTimedEvent(string eventName)
		{
			Crittercism.EndUserflow(eventName);
		}

	}
}
#endif