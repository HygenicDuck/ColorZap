using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

namespace Utils
{
	public class DeviceCamera
	{
		public enum AccessStatus
		{
			NOT_DETERMINED = 0,
			RESTRICTED,
			DENIED,
			AUTHORISED
		}

		private static bool s_frameAvailableCalled = false;
		private static bool s_cameraStarted = false;

		public delegate void CameraIsAvailableDelegate ();
		public static event CameraIsAvailableDelegate CameraIsAvailable;

		public delegate void CameraAuthorisedDelegate ();
		public static event CameraAuthorisedDelegate CameraAuthorised;

		public delegate void PictureTakenDelegate (Texture texture);
		public static event PictureTakenDelegate PictureTaken;

		private static DeviceCameraInterface nativeDevice;

		private static DeviceCameraInterface CreateNativeDevice()
		{
			if (Application.platform == RuntimePlatform.Android)
			{
				nativeDevice = new DeviceCameraAndroid ();
			}
			else if (Application.platform == RuntimePlatform.IPhonePlayer)
			{
				nativeDevice = new DeviceCameraIOS ();
			}
			else
			{
				nativeDevice = new DeviceCameraEditor ();
			}

			return nativeDevice;
		}

		/// <summary>
		/// This is a hack in order reset the android camera on first use to avoid the lag issue on some devices.
		/// Hopefully it can be removed if we migrate the android plugin to use the new camera2 classes.
		/// </summary>
		public static void ResetAndroidCamera()
		{
			#if UNITY_ANDROID
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			DeviceCameraAndroid androidDevice = nativeDevice as  DeviceCameraAndroid;
			androidDevice.ResetCamera();
			#endif
		}

		public static AccessStatus GetCameraAuthorised()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetCameraAuthorised();

		}

		public static void RequestCameraAuthorisation()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.RequestCameraAuthorisation();
		}

		public static void StartCameraCapture()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.StartCameraCapture();

			s_cameraStarted = true;
		}

		public static void StopCameraCapture()
		{
			if (nativeDevice != null && s_cameraStarted)
			{
				nativeDevice.StopCameraCapture();
			}

			s_cameraStarted = false;
			s_frameAvailableCalled = false;
		}

		public static void SetFrontCamera(bool isFront)
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.SetFrontCamera(isFront);
		}

		public static void SetFlashOn(bool flashOn)
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.SetFlashOn(flashOn);
		}

		public static bool IsFlashOn()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.IsFlashOn();

		}

		public static Vector2 GetCameraSize()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetCameraSize();
		}

		public static Texture GetCameraTexture(Vector2 size)
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetCameraTexture(size);
		}

		public static void TakePicture()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.TakePicture();
		}

		public static void FocusPicture(Vector2 screenPoint)
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.FocusPicture(screenPoint);
		}

		public static void SetAsActive(bool active)
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			nativeDevice.SetAsActive(active);
		}

		public static Vector3 GetDeviceScale()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetDeviceScale();
		}

		public static Vector3 GetDevicePictureScale()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetDevicePictureScale();
		}

		public static Quaternion GetDeviceRotation()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetDeviceRotation();
		}

		public static Quaternion GetDevicePictureRotation()
		{
			if (nativeDevice == null && CreateNativeDevice() == null)
			{
				throw new Exception ("nativeDevice is null");
			}

			return nativeDevice.GetDevicePictureRotation();
		}


		public static void CheckForFrame()
		{
			if (s_frameAvailableCalled == false)
			{
				if (nativeDevice == null && CreateNativeDevice() == null)
				{
					throw new Exception ("nativeDevice is null");
				}

				if (nativeDevice.IsFirstFrameReady())
				{
					s_frameAvailableCalled = true;
					_OnCameraFrameAvailale();
				}
			}
		}

		public static void OnApplicationPaused(bool paused)
		{
			if (nativeDevice != null)
			{
				nativeDevice.OnApplicationPaused(paused);
			}
		}

		public static void _OnCameraFrameAvailale()
		{
			if (CameraIsAvailable != null)
			{
				CameraIsAvailable();
			}
		}

		public static void _OnCameraAuthorised()
		{
			if (CameraAuthorised != null)
			{
				CameraAuthorised();
			}
		}

		public static void _OnPictureTaken(string path)
		{
			if (PictureTaken != null)
			{
				Vector2 size = DeviceCamera.GetCameraSize();

				Texture2D pictureTex = null;

				if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
				{
					pictureTex = new Texture2D ((int)size.x, (int)size.y, TextureFormat.RGBA32, false, false);

					if (nativeDevice != null)
					{
						nativeDevice.CopyTextureData(pictureTex);
					}

					PictureTaken(pictureTex);
				}
				else
				{
					WebImage.LoadImageFromDisk(path, (WebImage.ServerImage image) => {

						Debugger.Log("Loaded image from disk...", (int)SharedSystems.Systems.CAMERA_FEED);

						PictureTaken(image.texture);
					},
						(string error) => {
							Debugger.Log("FAILED to load image from disk: " + error, (int)SharedSystems.Systems.CAMERA_FEED);
						});
				}
			}
			else
			{
				Debugger.Log("PictureTaken is null", (int)SharedSystems.Systems.CAMERA_FEED);
			}
		}

	}
}

