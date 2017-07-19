using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace Utils
{

	static class InputFieldExtensions
	{
		public static bool WasDonePressed(this InputField inputField)
		{
			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit"))
			{
				return true;
			}

			return true; // PUB-1072 - this function always fails on device, no button is down at the end of editing when on iOS
		}
	}
}