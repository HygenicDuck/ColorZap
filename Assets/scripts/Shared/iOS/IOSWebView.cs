using System;
using UnityEngine;
using Utils;
using System.Runtime.InteropServices;

#if UNITY_IPHONE

public class IOSWebView
{
	[DllImport("__Internal")]
	private static extern int _OpenBackgroundUrl(string callbackName, string URL);

	[DllImport("__Internal")]
	private static extern int _OpenSafariUrl(string callbackName, string URL);

	public static void OpenBackgroundUrl(string url)
	{
		string callbackName = NativeIOSDelegate.CreateNativeIOSDelegate((System.Collections.Hashtable args) => {
		}).name;

		_OpenBackgroundUrl(callbackName, url);
	}

	public static void OpenSafariUrl(string url)
	{
		string callbackName = NativeIOSDelegate.CreateNativeIOSDelegate((System.Collections.Hashtable args) => {
		}).name;

		_OpenSafariUrl(callbackName, url);
	}
}

#endif
