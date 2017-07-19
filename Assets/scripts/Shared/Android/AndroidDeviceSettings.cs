using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Globalization;


namespace Core
{
	public class AndroidDeviceSettings : DeviceSettings
	{
		AndroidJavaClass m_androidCls = null;

		private AndroidJavaClass GetNativeClass()
		{
			if (m_androidCls == null)
			{
				m_androidCls = new AndroidJavaClass ("com.kwaleeplugins.devicesettings.DeviceLocation");
			}

			return m_androidCls;
		}

		public override void RequestProductLocale()
		{
			#if ANDROID_AMAZON
			//	Getting the location does not currently work on amazon so lets disable for now
			Utils.MarketEvents.SendProductCountryCodeFetchedEvent("");
			#else
			string callbackId =
				NativeIOSDelegate.CreateNativeIOSDelegate(( System.Collections.Hashtable args) => {
					string country = System.Convert.ToString(args["country"]);

					if (country.ToLower() != "null")
					{
						Utils.MarketEvents.SendProductCountryCodeFetchedEvent(country);
					}

				}).name;
			
			GetNativeClass().CallStatic("GetCountry", new object[] { callbackId });
			#endif
		}

		public override string GetLanguageCode()
		{
			return GetNativeClass().CallStatic<string>("GetLanguageCode");
		}

		public override List<InstalledKeyboard> GetKeyboards()
		{
			if (m_keyboards == null)
			{				
				int numKeyboards = GetNativeClass().CallStatic<int>("GetNumKeyboards");

				m_keyboards = new List<InstalledKeyboard> ();

				for (int i = 0; i < numKeyboards; i++)
				{
					string kb = GetNativeClass().CallStatic<string>("GetKeyboardLanguage", i);

					kb = kb.Replace("_", "-");

					if (!string.IsNullOrEmpty(kb))
					{
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
			}

			return m_keyboards;
		}
	}
}