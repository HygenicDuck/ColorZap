using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Globalization;

#if UNITY_ANDROID
namespace Social
{
	public class AndroidNativeShare : NativeShareInterface
	{
		AndroidJavaClass m_androidCls = null;

		private void CreateNativeClass()
		{
			if (m_androidCls == null)
			{
				m_androidCls = new AndroidJavaClass ("com.kwaleeplugins.nativeshare.Sharing");
			}
		}

		public override void Post(string url, string message)
		{
			CreateNativeClass();

			string callbackId =
				NativeIOSDelegate.CreateNativeIOSDelegate(( System.Collections.Hashtable args) => {
					Utils.SocialEvents.SendNativeShareFinishedEvent(true, string.Empty);
				}).name;
			
			m_androidCls.CallStatic("Post", new object[] { callbackId, url });
		}
	}
}
#endif
	