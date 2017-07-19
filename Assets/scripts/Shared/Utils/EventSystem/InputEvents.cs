using System;
using Utils;

namespace Utils
{
	public class InputEvents
	{
		public delegate void BackButtonDownDelegate ();
		public static event BackButtonDownDelegate BackButtonDown;
		public static void SendBackButtonDownEvent()
		{
			if (BackButtonDown != null)
			{
//				Debugger.Log("Event BackButtonDown sent", (int)WaffleSystems.Systems.EVENT_SYSTEM);
				BackButtonDown();
			}
		}

		public delegate void ScreenTappedDownDelegate ();
		public static event ScreenTappedDownDelegate ScreenTappedDown;
		public static void SendScreenTappedDownEvent()
		{
			if (ScreenTappedDown != null)
			{
//				Debugger.Log("Event ScreenTappedDown sent", (int)WaffleSystems.Systems.EVENT_SYSTEM);
				ScreenTappedDown();
			}
		}
	}
}