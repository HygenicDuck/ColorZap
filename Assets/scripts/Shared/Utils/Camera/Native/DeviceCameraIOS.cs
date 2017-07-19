using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace Utils
{
    public class DeviceCameraIOS : DeviceCameraInterface
    {

        [DllImport("__Internal")]
        private static extern int _GetCameraAuthorised();

        [DllImport("__Internal")]
        private static extern int _RequestCameraAuthorisation();

        [DllImport("__Internal")]
        private static extern void _StartCameraCapture();

        [DllImport("__Internal")]
        private static extern void _StopCameraCapture();

        [DllImport("__Internal")]
        private static extern void _SetFrontCamera(bool isFront);

        [DllImport("__Internal")]
        private static extern void _SetFlashOn(bool flashOn);

        [DllImport("__Internal")]
        private static extern int _GetCameraWidth();

        [DllImport("__Internal")]
        private static extern int _GetCameraHeight();

        [DllImport("__Internal")]
        private static extern System.IntPtr _GetCameraTexturePtr();

        [DllImport("__Internal")]
        private static extern void _TakePicture();

        [DllImport("__Internal")]
        private static extern void _FocusPicture(float x, float y);

		[DllImport("__Internal")]
		private static extern void _SetAsActive(bool active);

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

        public DeviceCamera.AccessStatus GetCameraAuthorised()
        {
			Debugger.Log("GetCameraAuthorised", (int)SharedSystems.Systems.CAMERA_FEED);

            return (DeviceCamera.AccessStatus)_GetCameraAuthorised();
        }

        public void RequestCameraAuthorisation()
        {
			Debugger.Log("RequestCameraAuthorisation", (int)SharedSystems.Systems.CAMERA_FEED);

            _RequestCameraAuthorisation();
        }

        public void StartCameraCapture()
        {
			Debugger.Log("StartCameraCapture", (int)SharedSystems.Systems.CAMERA_FEED);

            _StartCameraCapture();
        }

        public void StopCameraCapture()
        {
			Debugger.Log("StopCameraCapture", (int)SharedSystems.Systems.CAMERA_FEED);

            frameAvailableCalled = false;
            _StopCameraCapture();
        }

        public void SetFrontCamera(bool isFront)
        {
			Debugger.Log("SetFrontCamera: " + isFront, (int)SharedSystems.Systems.CAMERA_FEED);

            frontFacingCamera = isFront;
            _SetFrontCamera(isFront);
        }

        public void SetFlashOn(bool flashOn)
        {
			Debugger.Log("SetFlashOn: " + flashOn, (int)SharedSystems.Systems.CAMERA_FEED);

            this.flashOn = flashOn;
            _SetFlashOn(flashOn);
        }

        public bool IsFlashOn()
        {
            return flashOn;
        }

        public Vector2 GetCameraSize()
        {
			int width = _GetCameraWidth();
			int height = _GetCameraHeight();

			Debugger.Log("GetCameraSize: " + width + ", " + height, (int)SharedSystems.Systems.CAMERA_FEED);

			return new Vector2(width, height);
        }

        public Texture GetCameraTexture(Vector2 size)
        {
			Debugger.Log("GetCameraTexture: " + size.x + ", " + size.y, (int)SharedSystems.Systems.CAMERA_FEED);

            return Texture2D.CreateExternalTexture((int)size.x, (int)size.y, TextureFormat.RGBA32, false, false, _GetCameraTexturePtr());
        }

        public void TakePicture()
        {
			Debugger.Log("TakePicture", (int)SharedSystems.Systems.CAMERA_FEED);

            _TakePicture();
        }

        public void FocusPicture(Vector2 screenPoint)
        {
			Debugger.Log("FocusPicture: " + screenPoint.x + ", " + screenPoint.y, (int)SharedSystems.Systems.CAMERA_FEED);

            _FocusPicture(screenPoint.x, screenPoint.y);
        }

		public void SetAsActive(bool active)
		{
			Debugger.Log("SetAsActive: " + active, (int)SharedSystems.Systems.CAMERA_FEED);

			_SetAsActive(active);
		}

        public Vector3 GetDeviceScale()
        {
            if (frontFacingCamera)
            {
                return new Vector3(1, 1, 1);
            }
            else
            {
                return new Vector3(-1, 1, 1);
            }
        }

        public Vector3 GetDevicePictureScale()
        {
            if (!frontFacingCamera)
            {
                return new Vector3(1, 1, 1);
            }
            else
            {
                return new Vector3(-1, 1, 1);
            }
        }

        public Quaternion GetDeviceRotation()
        {
            if (frontFacingCamera)
            {
                return Quaternion.Euler(0, 0, -90);
            }
            else
            {
                return Quaternion.Euler(0, 0, 90);
            }
        }

        public Quaternion GetDevicePictureRotation()
        {
            if (!frontFacingCamera)
            {
                return Quaternion.Euler(0, 0, -90);
            }
            else
            {
                return Quaternion.Euler(0, 0, 90);
            }
        }

        public bool IsFirstFrameReady()
        {
            return true;
        }

        public void CopyTextureData(Texture2D copyTo)
        {
            throw new NotImplementedException();
        }

        public void OnApplicationPaused(bool paused)
        {
            
        }
    }
}
