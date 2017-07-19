using Core;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Utils
{
	public class KeyboardExtensions : BaseBehaviour
	{
		[DllImport("__Internal")]
		private static extern bool _WasKeyboardDonePressed();

		[DllImport("__Internal")]
		private static extern bool _ShowDoneButton(bool show);

		public static bool WasKeyboardDonePressed()
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				return _WasKeyboardDonePressed();
			}
			else
			{
				return true;
			}
		}

		public static void ShowDoneButton(bool show)
		{
			if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				_ShowDoneButton(show);
			}
		}
	}
}
