using System;
using UnityEngine;
using Utils;

#if UNITY_ANDROID

namespace Tastybits.NativeGallery
{
	
	public class AndroidGallery
	{

		static AndroidJavaClass _AndroidCls = null;
		static AndroidJavaClass AndroidCls
		{
			get
			{ 
				if (_AndroidCls == null)
				{
					_AndroidCls = new AndroidJavaClass ("com.NativeGallery.AndroidGallery");
				}
				return _AndroidCls;
			}
		}

		public static void OpenGallery(System.Action<Texture> callback)
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
						WebImage.LoadImageFromDisk(path, (WebImage.ServerImage image) => {
							if (image.texture != null)
							{
								callback(image.texture);
							}
						});
					}
					else
					{
						callback(null);
					}

				}).name;
			
			AndroidCls.CallStatic("OpenGallery", new object[] { callbackId });
		}
	}
}

#endif
