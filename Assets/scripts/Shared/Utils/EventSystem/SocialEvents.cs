using System;
using Utils;

namespace Utils
{
	public class SocialEvents
	{
		public delegate void NativeShareFinishedDelegate (bool completed, string activityType);
		public static event NativeShareFinishedDelegate NativeShareFinished;
		public static void SendNativeShareFinishedEvent(bool completed, string activityType)
		{
			if (NativeShareFinished != null)
			{
//				Debugger.Log("Event NativeShareFinished sent", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.EVENT_SYSTEM);
				NativeShareFinished(completed, activityType);
			}
		}
	}
}