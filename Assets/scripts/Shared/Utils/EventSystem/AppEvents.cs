using System;
using Utils;
using UnityEngine;

namespace Utils
{
	public class AppEvents
	{
		public delegate void OnAppPausedDelegate ();
		public static event OnAppPausedDelegate OnAppPaused;
		public static void SendOnAppPausedEvent()
		{
			if (OnAppPaused != null)
			{
				OnAppPaused();
			}
		}

		public delegate void OnAppUnpausedDelegate ();
		public static event OnAppUnpausedDelegate OnAppUnpaused;
		public static void SendOnAppUnpausedEvent()
		{
			if (OnAppUnpaused != null)
			{
				OnAppUnpaused();
			}
		}
	}
}