using UnityEngine;
using System.Collections;
using System;

namespace Utils
{
    public class DeviceCameraEditor : DeviceCameraInterface
    {
        private static WebCamTexture s_cameraTex = new WebCamTexture();

        private bool bFirstFrameAvailable;

        public bool frameAvailableCalled
        {
            get { return bFirstFrameAvailable; }
            set { bFirstFrameAvailable = value; }
        }

        public bool frontFacingCamera
        {
            get { return false; }
            set { //bool facing = value; 
			}
        }

        public bool flashOn
        {
            get { return false; }
            set { //bool flash = value; 
			}
        }

        public void RequestCameraAuthorisation()
        {

        }

        public DeviceCamera.AccessStatus GetCameraAuthorised()
        {
            return DeviceCamera.AccessStatus.AUTHORISED;
        }

        public void StartCameraCapture()
        {
            Utils.CoroutineHelper.Instance.Run(DelayCameraStart());
        }

        public void StopCameraCapture()
        {
            frameAvailableCalled = false;
            s_cameraTex.Stop();
        }

        public void SetFrontCamera(bool isFront)
        {
            Debugger.Warning("SetFrontCamera is not supported on this platform.");
        }

        public void SetFlashOn(bool flashOn)
        {
            Debugger.Warning("_SetFlashOn is not supported on this platform.");
        }

        public bool IsFlashOn()
        {
            return false;
        }

        public Vector2 GetCameraSize()
        {
           return new Vector2(s_cameraTex.width, s_cameraTex.height);
        }

        public Texture GetCameraTexture(Vector2 size)
        {
            return s_cameraTex;
        }

        public void TakePicture()
        {
            DeviceCameraListener.Instance._OnPictureTaken("");
        }

        public void FocusPicture(Vector2 screenPoint)
        {
            Debugger.Warning("FocusPicutre is not supported on this platform.");
        }

		public void SetAsActive(bool active)
		{
			Debugger.Warning("SetAsActive is not supported on this platform.");
		}

        public Vector3 GetDeviceScale()
        {
            return new Vector3(-1, 1, 1);
        }

        public Vector3 GetDevicePictureScale()
        {
            return new Vector3(-1, 1, 1);
        }

        public Quaternion GetDeviceRotation()
        {
            return Quaternion.Euler(0, 0, 0);
        }

        public Quaternion GetDevicePictureRotation()
        {
            return Quaternion.Euler(0, 0, 0);
        }

        private IEnumerator DelayCameraStart()
        {
            yield return new WaitForSeconds(1);

            s_cameraTex.Play();
        }

        public bool IsFirstFrameReady()
        {
            return s_cameraTex != null && s_cameraTex.didUpdateThisFrame;
        }

        public void CopyTextureData(Texture2D copyTo)
        {
            copyTo.SetPixels32(s_cameraTex.GetPixels32());
            copyTo.Apply();
        }

        public void OnApplicationPaused(bool paused)
        {

        }
    }
}