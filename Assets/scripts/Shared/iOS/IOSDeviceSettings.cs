using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Globalization;


namespace Core
{
	public class IOSDeviceSettings : DeviceSettings
	{
		[DllImport("__Internal")]
		private static extern int _GetNumAppleKeyboards();

		[DllImport("__Internal")]
		private static extern string _GetAppleKeyboard(int idx);

		[DllImport("__Internal")]
		private static extern string _GetLanguageCode(string[] availableLanguages, int numAvailableLang);

		[DllImport("__Internal")]
		private static extern void _RequestProductLocale();



		public override List<InstalledKeyboard> GetKeyboards()
		{
			if (m_keyboards == null)
			{
				m_keyboards = new List<InstalledKeyboard> ();

				int numKB = _GetNumAppleKeyboards();
				for (int i = 0; i < numKB; i++)
				{
					string kb = _GetAppleKeyboard(i);

					int endIdx = kb.IndexOf("@");

					if (endIdx != -1)
					{
						kb = kb.Remove(endIdx);
						kb = kb.Replace("_", "-");
					}

					try
					{
						CultureInfo ci = CultureInfo.GetCultureInfo(kb);
						string displayName = ci.IsNeutralCulture ? ci.NativeName : ci.Parent.NativeName;

						m_keyboards.Add(new InstalledKeyboard {
							languageID = kb,
							languageDisplayName = displayName
						});
					}
					catch
					{
						Debugger.Log("no culture info found for " + kb, (int)SharedSystems.Systems.CORE);
					}
				}
			}
			return m_keyboards;
		}

		public override string GetLanguageCode()
		{
			string languageCode = _GetLanguageCode(s_supportedLanguages, s_supportedLanguages.Length);
			return languageCode.Replace('-', '_');
		}

		public override void RequestProductLocale()
		{
			_RequestProductLocale();
		}
	}
}