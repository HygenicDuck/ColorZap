using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;
using System.Collections.Generic;
using System.Globalization;


namespace Core
{
	public class DeviceSettingsManager : MonoSingleton<DeviceSettingsManager>
	{
		private DeviceSettings m_settings;

		public DeviceSettings Settings { get { return m_settings; } }
			
		protected override void Init()
		{
			#if UNITY_EDITOR
			m_settings = new DeviceSettings ();
			#elif UNITY_IOS
			m_settings = new IOSDeviceSettings ();
			#elif UNITY_ANDROID
			m_settings = new AndroidDeviceSettings ();
			#else
			m_settings = new DeviceSettings();
			#endif
		}

		public void _OnProductLocaleFetched(string countryCode)
		{
			Utils.MarketEvents.SendProductCountryCodeFetchedEvent(countryCode);
		}
	}

	public class DeviceSettings
	{
		protected static string[] s_supportedLanguages = {
			"en", "en-GB", 
			"ja-JP", 
			"it", "it-IT",
			"pt", "pt-BR",
			"ru-RU",
			"es", "es-ES",
			"fr", "fr-FR",
			"de", "de-DE",
			"ko-KR",
			"tr-TR",
			"ar",
			"zh-chs", "zh-cht"
		};

		public class InstalledKeyboard
		{
			public string languageID;
			public string languageDisplayName;
		}

		protected List<InstalledKeyboard> m_keyboards;

		public virtual List<InstalledKeyboard> GetKeyboards()
		{
			if (m_keyboards == null)
			{				
				m_keyboards = new List<InstalledKeyboard> ();

				m_keyboards.Add(new InstalledKeyboard {
					languageID = "en_GB",
					languageDisplayName = "English"
				});
			}

			return m_keyboards;
		}

		public virtual string GetLanguageCode()
		{
			return "en_GB";
		}

		public virtual void RequestProductLocale()
		{
		}
	}

}
	