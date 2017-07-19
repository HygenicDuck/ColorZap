using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Globalization;
using System;


namespace Social
{
	public class NativeShare : MonoSingleton<NativeShare>
	{
		private NativeShareInterface m_impl = null;
			
		protected override void Init()
		{
			#if !UNITY_EDITOR
			#if UNITY_IOS
			m_impl = gameObject.AddComponent<IOSNativeShare>();
			#elif UNITY_ANDROID
			m_impl = gameObject.AddComponent<AndroidNativeShare>();
			#endif
			#endif
		}

		public void Post(string url, string message)
		{
			if (m_impl != null)
			{
				m_impl.Post(url, message);
			}
		}
	}

	public class NativeShareInterface : MonoBehaviour
	{
		public virtual void Post(string url, string message){}
	}
}
	