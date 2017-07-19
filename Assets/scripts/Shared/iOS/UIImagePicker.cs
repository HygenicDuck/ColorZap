using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using Utils;


namespace NativeGallery
{

	public enum ImagePickerType
	{
		UIImagePickerControllerSourceTypePhotoLibrary = 0,
		UIImagePickerControllerSourceTypeCamera = 1,
		UIImagePickerControllerSourceTypeSavedPhotosAlbum = 2
	}


	public class UIImagePicker
	{
		public enum AccessStatus
		{
			NOT_DETERMINED = 0,
			RESTRICTED,
			DENIED,
			AUTHORISED
		}

		// iOS platform implementation.
		[DllImport("__Internal")]
		private static extern int _GetAlbumAuthorised();

		[DllImport("__Internal")]
		private static extern int _RequestAlbumAuthorised();

		[DllImport("__Internal")]
		private static extern string _ImagePickerOpen(int type, bool frontFacingIsDefault, string nativeCallbackId);

		[DllImport("__Internal")]
		private static extern string _GetLastImages(int num, string nativeCallbackId);
		
		[DllImport("__Internal")]
		private static extern string _ImagePickerGetPath();
		
		[DllImport("__Internal")]
		private static extern int _ImagePickerGetCallCount();

		[DllImport("__Internal")]
		private static extern int _OpenImageSearch(string callbackName, string URL, float screenOccupationY, string webRegEx, string imageRegEx);

		[DllImport("__Internal")]
		private static extern int _CloseImageSearch();

		public delegate void AlbumAuthorisedDelegate (UIImagePicker.AccessStatus status);
		public static event AlbumAuthorisedDelegate AlbumAuthorised;

		public static void _OnAlbumAuthorised(UIImagePicker.AccessStatus status)
		{
			if (AlbumAuthorised != null)
			{
				Debugger.Log("Found callback " + status.ToString(), (int)SharedSystems.Systems.UIIMAGE_PICKER);

				AlbumAuthorised(status);
			}
		}

		public static AccessStatus GetAlbumAuthorised()
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				return AccessStatus.AUTHORISED;
			}

			return (AccessStatus)_GetAlbumAuthorised();
		}

		public static void RequestAlbumAuthorisation()
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				return;
			}

			_RequestAlbumAuthorised();
		}

		public static void FetchLastImages(int num, System.Action<Texture[],bool,string> callback)
		{
			Debugger.Log("FetchLastImages " + num, (int)SharedSystems.Systems.UIIMAGE_PICKER);

			UIImagePicker.GetMostRecentImages(num, ( bool succeeded, bool cancelled, string[] paths) => {

				if (succeeded && !cancelled)
				{
					if (paths == null)
					{
						paths = new string[0];
					}

					Texture[] textures = new Texture[paths.Length];
					string msg = "";

					for (int i = 0; i < paths.Length; ++i)
					{
						int index = i;

						Debugger.Log("Fetching image = " + paths[i], (int)SharedSystems.Systems.UIIMAGE_PICKER);

						WebImage.LoadImageFromDisk(paths[i], (WebImage.ServerImage image) => {
							if (image.texture != null)
							{
								textures[index] = image.texture;
							}

							if (index == paths.Length - 1)
							{
								callback(textures, true, msg);
							}
						});
					}
				}
			});
		}

		public static void OpenPhotoAlbum(System.Action<Texture,bool> callback)
		{
			UIImagePicker.Open(ImagePickerType.UIImagePickerControllerSourceTypePhotoLibrary, ( bool succeeded, bool cancelled, string path) => {

				if (succeeded && !cancelled)
				{
					WebImage.LoadImageFromDisk(path, (WebImage.ServerImage image) => {
						if (image.texture != null)
						{
							callback(image.texture, true);
						}
						else
						{
							callback(null, false);
						}
					});
				}
				else
				{
					callback(null, false);
				}
			});
		}

		public static void CloseGoogleImageSearch()
		{
			_CloseImageSearch();
		}

		public static void OpenGoogleImageSearch(System.Action<Texture,bool,string> callback, float height)
		{
			string callbackName = NativeIOSDelegate.CreateNativeIOSDelegate((System.Collections.Hashtable args) => {
				bool succeeded = System.Convert.ToBoolean(args["succeeded"]);

				string url = System.Convert.ToString(args["url"]);
				Debugger.Log("Returned with succeeded = " + succeeded + " url = " + url, (int)SharedSystems.Systems.UIIMAGE_PICKER);

				WebImage.Request(url, 
					(WebImage.ServerImage image) => {
						callback(image.texture, succeeded, "");
					},
					(string error) => {
						callback(null, false, error);
					}
				);

				_CloseImageSearch();
			}).name;

//			WaffleIronObjects.GameSettings gameSettings = WaffleUtils.GameSettingsManager.Instance.GetGameSettings();
//			_OpenImageSearch(callbackName, gameSettings.googleURL, height, gameSettings.googleRegEx, gameSettings.googleImgRegEx);
		}

		public static string Open(ImagePickerType type, System.Action<bool,bool,string> deleg)
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				deleg(false, false, "");
				return "";
			}

			return _ImagePickerOpen((int)type, true, NativeIOSDelegate.CreateNativeIOSDelegate((System.Collections.Hashtable args) => {
				bool succeeded = System.Convert.ToBoolean(args["succeeded"]);
				bool cancelled = System.Convert.ToBoolean(args["cancelled"]);
				string path = System.Convert.ToString(args["path"]);
				Debugger.Log("UIImagePicker returned with succeeded = " + succeeded + " cancelled = " + cancelled + " path = " + path, (int)SharedSystems.Systems.UIIMAGE_PICKER);
				deleg(succeeded, cancelled, path);
			}).name);
		}

		private static string GetMostRecentImages(int num, System.Action<bool,bool,string[]> deleg)
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				deleg(false, false, null);
				return "";
			}

			return _GetLastImages(num, NativeIOSDelegate.CreateNativeIOSDelegate((System.Collections.Hashtable args) => {
				bool succeeded = System.Convert.ToBoolean(args["succeeded"]);
				bool cancelled = System.Convert.ToBoolean(args["cancelled"]);

				string[] paths = new string[num];

				for (int i = 0; i < num; ++i)
				{
					paths[i] = System.Convert.ToString(args["path" + i]);

					Debugger.Log("PATH " + i + " = " + paths[i], (int)SharedSystems.Systems.UIIMAGE_PICKER);
				}

				Debugger.Log("UIImagePicker returned with succeeded = " + succeeded + " cancelled = " + cancelled, (int)SharedSystems.Systems.UIIMAGE_PICKER);
				deleg(succeeded, cancelled, paths);
			}).name);
		}
		
		public static string GetPath()
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				return "";
			}
			return _ImagePickerGetPath();
		}

		
		public static int GetCallCount()
		{
			if (Application.platform != RuntimePlatform.IPhonePlayer)
			{
				return 0;
			}
			return _ImagePickerGetCallCount();
		}


	}

}
	