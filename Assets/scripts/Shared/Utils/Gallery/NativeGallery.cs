using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using NativeGallery;


namespace NativeGallery
{

	// This is an interface to use the androd gallery and the image picker to pick an image in unity.
	// You can still use the classes UIImagePicker directly for a more specific iOS related
	// set of functionality.
	public class ImagePicker
	{
		public static void FetchLastImages(int num, System.Action<Texture[]> callback)
		{
#if UNITY_IPHONE
			UIImagePicker.FetchLastImages( num, ( Texture[] textures, bool ok, string errMsg )=> {
				callback(textures);
			} );
#endif
		}

		public static void OpenGallery(System.Action<Texture> callback)
		{

#if UNITY_IPHONE
			UIImagePicker.OpenPhotoAlbum( ( Texture texture, bool ok )=>{
				callback(texture);
			} );
#elif UNITY_ANDROID
			Tastybits.NativeGallery.AndroidGallery.OpenGallery((Texture tex) => {
				callback(tex);
			});
#endif
		}


		public static void OpenGoogleImageSearch(System.Action<Texture, bool, string> callback, float height)
		{
#if UNITY_IPHONE
			UIImagePicker.OpenGoogleImageSearch(( Texture texture, bool ok, string error )=>{
				callback(texture, ok, error);
			}, height);
#elif UNITY_ANDROID

			AndroidWebView.OpenGoogleImageSearch((texture) => {
				callback(texture, true, "");
			}, height);
#endif
		}
	}



}