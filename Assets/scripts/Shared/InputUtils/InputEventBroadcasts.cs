using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Purchasing;

namespace InputUtils
{
	public class InputEventBroadcasts : MonoSingleton<InputEventBroadcasts>
	{
		protected override void Init()
		{
		}


		private void Update()
		{
			#if UNITY_ANDROID
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				Utils.InputEvents.SendBackButtonDownEvent();
			}
			#endif

			if (Input.GetMouseButtonDown(0))
			{
				Utils.InputEvents.SendScreenTappedDownEvent();
			}
		}
	}
}