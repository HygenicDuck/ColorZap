using System;
using UnityEngine;
using Utils;

#if UNITY_ANDROID

public class AndroidWebView
{
	static AndroidJavaClass _AndroidCls = null;
	static AndroidJavaClass AndroidCls
	{
		get
		{ 
			if (_AndroidCls == null)
			{
				_AndroidCls = new AndroidJavaClass ("com.kwaleeplugins.webview.AndroidWebView");
			}
			return _AndroidCls;
		}
	}

	public static void OpenGoogleImageSearch(System.Action<Texture> callback, float height)
	{ 
		if (Application.platform != RuntimePlatform.Android)
		{
			Debug.LogError("Cannot do this on a non Android platform");
		}

		string callbackId =
			NativeIOSDelegate.CreateNativeIOSDelegate(( System.Collections.Hashtable args) => {

				bool succeeded = System.Convert.ToBoolean(args["succeeded"]);
				bool cancelled = System.Convert.ToBoolean(args["cancelled"]);
				string path = System.Convert.ToString(args["path"]);

				if (succeeded && !cancelled)
				{
					WebImage.Request(path, (WebImage.ServerImage image) => {
						callback(image.texture);
					});
				}
				else
				{
					callback(null);
				}
			}).name;

//		WaffleIronObjects.GameSettings gameSettings = WaffleUtils.GameSettingsManager.Instance.GetGameSettings();
//
//		AndroidCls.CallStatic("OpenImageSearch", new object[] {
//			callbackId,
//			gameSettings.googleURL,
//			height,
//			gameSettings.googleRegEx,
//			gameSettings.googleImgRegExAndroid
//		});
	}

	public static void OpenBackgroundUrl(string url)
	{
		string callbackId =
			NativeIOSDelegate.CreateNativeIOSDelegate(( System.Collections.Hashtable args) => {

			}).name;
		
		AndroidCls.CallStatic("OpenBackgroundUrl", new object[] {
			callbackId,
			url
		});
	}
}

#endif
