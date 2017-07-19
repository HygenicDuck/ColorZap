using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kanga;
using Utils;

public class TextManager
{
	public const string ELLIPSIS = "...";

	private static readonly bool DEBUG_TEST_MISSING_STRINGS = false;
	private const string DEBUG_MISSING_STRING_TEXT = "_0_";

	//Strings
//	public static string Get(string key)
//	{
//		return DEBUG_TEST_MISSING_STRINGS ? DEBUG_MISSING_STRING_TEXT : LocalizationManager.GetString(key);
//	}
//
//	public static bool Contains(string key)
//	{
//		return LocalizationManager.ContainsKey(key);
//	}
//	
//	//Arrays
//	public static string Get(string key, int index)
//	{
//		return DEBUG_TEST_MISSING_STRINGS ? DEBUG_MISSING_STRING_TEXT : LocalizationManager.GetArrayEntry(key, index);
//	}
//	
//	public static string GetRandom(string arrayKey)
//	{
//		int i = UnityEngine.Random.Range(0, Count(arrayKey));
//		return Get(arrayKey, i);
//	}
//	
//	public static int Count(string key)
//	{
//		return LocalizationManager.GetArrayCount(key);
//	}
//	
//	public static string[] GetAll(string key)
//	{
//		return LocalizationManager.GetArray(key);
//	}
//		
//	public static void Initialize()
//	{
//		#if UNITY_EDITOR
//		if (!Application.isPlaying)
//		{
//			InitializeEditor();	
//		}
//		else
//		{
//			InitializeInGame();
//		}
//		#else
//		InitializeInGame();
//		#endif
//	}
//
//	private static void InitializeEditor()
//	{
//		string deviceLanguage = "en_GB";
//		LocalizationManager.SetCurrentLanguage(deviceLanguage);
//	}
//
//	private static void InitializeInGame()
//	{
//		string deviceLanguage = Core.DeviceSettingsManager.Instance == null ? "en_GB" : Core.DeviceSettingsManager.Instance.Settings.GetLanguageCode();
//
//		LocalizationManager.SetCurrentLanguage(deviceLanguage);
//
//		WaffleIron.Server wIron = WaffleIron.Server.Instance;
//		if (wIron.HasAuthPlayer())
//		{
//			Player player = wIron.GetPlayerHandler().GetPlayer();
//			if (player != null)
//			{
//				PlayerProfile playerProfile = wIron.GetPlayerHandler().GetPlayerProfile();
//				if (playerProfile != null && playerProfile.locale != null)
//				{
//					string playerLocale = playerProfile.locale.language;
//					if (playerProfile.locale.region != null)
//					{
//						playerLocale += "_" + playerProfile.locale.region;
//					}
//
//					if (deviceLanguage != playerLocale)
//					{
//						WaffleRequests.Player.EditPlayerOptions options = new WaffleRequests.Player.EditPlayerOptions (player.GetObjectID());
//						options.SetLocale(deviceLanguage);
//						WaffleRequests.Player.EditPlayer(options, null, null);
//
//						//Update localy
//						int indexOf = deviceLanguage.IndexOf('_');
//						if (indexOf >= 0)
//						{
//							playerProfile.locale.language = deviceLanguage.Substring(0, indexOf);
//							playerProfile.locale.region = deviceLanguage.Substring(indexOf + 1, deviceLanguage.Length - indexOf - 1);
//						}
//						else
//						{
//							playerProfile.locale.language = deviceLanguage;
//							playerProfile.locale.region = null;
//						}
//					}
//				}
//			}
//		}
//
//
//		#if UNITY_EDITOR
//		#else
//		DownloadTexts();
//		#endif
//	}
//
//
//	private static void DownloadTexts()
//	{
//		//TODO
//		//int version = LocalizationManager.GetCurrentVersion();
//		//WaffleRequests.Texts.Options options = new WaffleRequests.Texts.Options {
//		//	version = 0 
//		//};
//
//		//WaffleRequests.Texts.GetTexts(options, GetTextFailCallback, GetTesxSuccessCallback);
//
//	}
//
//	private static void TestSuccessCallback(ResponseObjectHandler objHandler)
//	{
//		Debugger.Log("******** TextManager success********" + objHandler.ToString(), Debugger.Severity.MESSAGE);
//
//		WaffleIronObjects.LocalisedText text = objHandler.GetHit<WaffleIronObjects.LocalisedText>("text");
//		if (text.version > LocalizationManager.GetCurrentVersion())
//		{
//			text.SaveToFile();
//			//TODO: reload text 
//		}
//
//	}
//
//	private static void TestFailCallback(Hashtable errorTable)
//	{
//		Debugger.Log("******** TextManager fail********", Debugger.Severity.MESSAGE);
//	}


	private static TextManager instance;

	private static TextManager Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new TextManager ();
			}

			return instance;
		}
	}
}


