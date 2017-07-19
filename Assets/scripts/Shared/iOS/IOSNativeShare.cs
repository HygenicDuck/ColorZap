using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Globalization;
using System;

#if UNITY_IOS
namespace Social
{
	public class IOSNativeShare : NativeShareInterface
	{
		[DllImport("__Internal")]
		private static extern void _Post(string url, string message);

		private void Awake ()
		{
			gameObject.name = "IOSNativeShare";
		}

		public override void Post(string url, string message)
		{
			_Post(url, message);
		}

		public void _OnNativeShareFinished(string strData)
		{
			System.Collections.Hashtable args = JSON.JsonDecode(strData) as System.Collections.Hashtable;

			bool completed = System.Convert.ToBoolean(args["completed"]);
			string activityType = System.Convert.ToString(args["activityType"]);


			Debug.Log("_OnNativeShareFinished completed " + completed);
			Debug.Log("_OnNativeShareFinished activityType " + activityType);
			Utils.SocialEvents.SendNativeShareFinishedEvent(completed, activityType);
		}
	}
}
#endif
	