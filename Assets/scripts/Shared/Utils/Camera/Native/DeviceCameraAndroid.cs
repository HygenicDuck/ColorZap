using UnityEngine;
using System.Collections;
using System;

namespace Utils
{
    public class DeviceCameraAndroid : DeviceCameraInterface
    {
        private AndroidJavaObject KwaleeCameraAndroidInstance;//Java instance
        private int widthRequest = 640;
        private int heightRequest = 480;
        private int fpsRequest = 30;

        private const string ANDROID_PERMISSION_CAMERA = "android.permission.CAMERA";

        bool bFrameAvailableCalled;
        bool bFrontFacingCamera;
        bool bFlashOn;

        public bool frameAvailableCalled
        {
            get { return bFrameAvailableCalled; }
            set { bFrameAvailableCalled = value; }
        }

        public bool frontFacingCamera
        {
            get { return bFrontFacingCamera; }
            set { bFrontFacingCamera = value; }
        }

        public bool flashOn
        {
            get { return bFlashOn; }
            set { bFlashOn = value; }
        }

        private void CreateJavaBridge()
        {
            using (AndroidJavaClass JavaClass = new AndroidJavaClass("com.sockmonkeystudios.camera.KwaleeCamera"))
            {
                if (JavaClass != null)
                {
                    KwaleeCameraAndroidInstance = JavaClass.CallStatic<AndroidJavaObject>("instance");
                }
            }
        }

		public void ResetCamera()
		{
			if (KwaleeCameraAndroidInstance == null)
			{
				CreateJavaBridge();
			}

			KwaleeCameraAndroidInstance.Call("CreateTempActivity");
		}

        public void StartCameraCapture()
        {
            if (KwaleeCameraAndroidInstance == null)
            {
                CreateJavaBridge();
            }

            KwaleeCameraAndroidInstance.Call("Initialize");
            KwaleeCameraAndroidInstance.Call("StartCameraPreview", (frontFacingCamera == false ? 0 : 1), widthRequest, heightRequest, fpsRequest);

            Utils.CoroutineHelper.Instance.Run(UpdateNativeCamera());
        }

        public void StopCameraCapture()
        {
            if (KwaleeCameraAndroidInstance == null)
            {
                Debugger.Log("KwaleeCameraAndroidInstance is already null", (int)SharedSystems.Systems.CAMERA_FEED);
                return;
            }

            frameAvailableCalled = false;

            if (KwaleeCameraAndroidInstance != null)
            {
                KwaleeCameraAndroidInstance.Call("Release");
                KwaleeCameraAndroidInstance.Dispose();
                KwaleeCameraAndroidInstance = null;
            }
        }

        public bool IsFirstFrameReady()
        {
            if (KwaleeCameraAndroidInstance == null)
                return false;

            return KwaleeCameraAndroidInstance.Call<bool>("IsFirstFrameReady");
        }

        public void SetFrontCamera(bool isFront)
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            frontFacingCamera = isFront;

            KwaleeCameraAndroidInstance.Call("StartCameraPreview", (frontFacingCamera == false ? 0 : 1), widthRequest, heightRequest, fpsRequest);
        }

        public void SetFlashOn(bool flashOn)
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            KwaleeCameraAndroidInstance.Call("SetFlash", flashOn);
        }

        public bool IsFlashOn()
        {
            return flashOn;
        }

        private int CameraWidth()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return KwaleeCameraAndroidInstance.Call<int>("GetTextureWidth");
        }

        private int CameraHeight()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return KwaleeCameraAndroidInstance.Call<int>("GetTextureHeight");
        }

        private System.IntPtr GetCameraTexturePtr()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return (System.IntPtr)KwaleeCameraAndroidInstance.Call<int>("GetCameraTexturePtr");
        }

        public Vector2 GetCameraSize()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

			// Compensate for issue in the camera plugin jar which seems to be returning the incorrect width and height           
			return new Vector2(CameraHeight(),CameraWidth());
        }

        public Texture GetCameraTexture(Vector2 size)
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            Debugger.Log("GetCameraTexture size: " + size, (int)SharedSystems.Systems.CAMERA_FEED);

            return Texture2D.CreateExternalTexture((int)size.x, (int)size.y, TextureFormat.RGBA32, false, false, GetCameraTexturePtr());
        }

        public void TakePicture()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            KwaleeCameraAndroidInstance.Call("TakePicture", DeviceCameraListener.Instance.name);
        }

        public void FocusPicture(Vector2 screenPoint)
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            KwaleeCameraAndroidInstance.Call("SetCameraFocus", screenPoint.x, screenPoint.y);
        }

		public void SetAsActive(bool active)
		{
			if (KwaleeCameraAndroidInstance == null)
			{
				throw new System.Exception ("KwaleeCameraAndroidInstance is null");
			}

			KwaleeCameraAndroidInstance.Call("SetAsActive", active);
		}

        public Vector3 GetDeviceScale()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return new Vector3(1, 1, 1);
        }

        public Vector3 GetDevicePictureScale()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return new Vector3(1, 1, 1);
        }

        public Quaternion GetDeviceRotation()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return Quaternion.Euler(0, 0, 0);
        }

        public Quaternion GetDevicePictureRotation()
        {
            if (KwaleeCameraAndroidInstance == null)
                throw new System.Exception("KwaleeCameraAndroidInstance is null");

            return Quaternion.Euler(0, 0, 0);
        }


        public void RequestCameraAuthorisation()
        {
            Debug.Log("RequestCameraAuthorisation");

            using (AndroidJavaClass JavaClass = new AndroidJavaClass("com.sockmonkeystudios.camera.KwaleeCamera"))
            {
                JavaClass.CallStatic("RequestPermission", ANDROID_PERMISSION_CAMERA, DeviceCameraListener.Instance.name, "_OnCameraAuthorisedUpdated");
            }
        }

        public DeviceCamera.AccessStatus GetCameraAuthorised()
        {
            DeviceCamera.AccessStatus status = DeviceCamera.AccessStatus.NOT_DETERMINED;

            using (AndroidJavaClass JavaClass = new AndroidJavaClass("com.sockmonkeystudios.camera.KwaleeCamera"))
            {
                string stringReturned = JavaClass.CallStatic<string>("CheckForPermissions", ANDROID_PERMISSION_CAMERA);

                if (System.Enum.IsDefined(typeof(DeviceCamera.AccessStatus), stringReturned))
                {
                    status = (DeviceCamera.AccessStatus)System.Enum.Parse(typeof(DeviceCamera.AccessStatus), stringReturned);
                }

                Debug.Log(stringReturned);
            }
            return status;
        }


        private IEnumerator UpdateNativeCamera()
        {
            Utils.CoroutineHelper.Instance.Stop(UpdateNativeCamera());

            while (KwaleeCameraAndroidInstance != null)
            {
                KwaleeCameraAndroidInstance.Call("UpdateTexture");
                yield return null;
            }
        }

        public void CopyTextureData(Texture2D copyTo)
        {
            throw new NotImplementedException();
        }

        public void OnApplicationPaused(bool paused)
        {
            if (KwaleeCameraAndroidInstance != null)
            {
                KwaleeCameraAndroidInstance.Call("OnApplicationPaused", paused);
            }
        }
    }
}
