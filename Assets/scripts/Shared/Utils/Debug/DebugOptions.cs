using UnityEngine;
using UI;

namespace Utils
{
	public class DebugOptions
	{
		private static bool s_enableFpsCounter = false;
		private static bool s_enableDeviceSpecs = false;

		public static void Initialise()
		{
			if (s_enableFpsCounter)
			{
				Transform canvas = PopupManager.Instance.DebugRootCanvas;
				UI.UIElement.LoadUIElementStatic(FrameRateCounter.PREFAB_NAME, Vector3.zero, canvas);
			}

			if (s_enableDeviceSpecs)
			{
				Transform canvas = PopupManager.Instance.DebugRootCanvas;
				UI.UIElement.LoadUIElementStatic(DeviceSpecsWidget.PREFAB_NAME, Vector3.zero, canvas);
			}
		}

		public static void EnableFpsCounter(bool enable)
		{
			s_enableFpsCounter = enable;
		}

		public static void EnableDeviceSpecs(bool enable)
		{
			s_enableDeviceSpecs = enable;
		}
	}
}
