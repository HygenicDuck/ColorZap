using UnityEngine;
using System.Collections;

namespace Utils
{
    public interface DeviceCameraInterface
    {
        bool frameAvailableCalled{get;set;}
        bool frontFacingCamera { get; set; }
        bool flashOn { get; set; }

        void RequestCameraAuthorisation();
        DeviceCamera.AccessStatus GetCameraAuthorised();
        void StartCameraCapture();
        void StopCameraCapture();
        void SetFrontCamera(bool isFront);
        void SetFlashOn(bool flashOn);

        bool IsFlashOn();
        bool IsFirstFrameReady();

        Vector2 GetCameraSize();
        Texture GetCameraTexture(Vector2 size);

        void TakePicture();
        void FocusPicture(Vector2 screenPoint);
		void SetAsActive(bool active);

        Vector3 GetDeviceScale();
        Vector3 GetDevicePictureScale();

        Quaternion GetDeviceRotation();
        Quaternion GetDevicePictureRotation();

        void CopyTextureData(Texture2D copyTo);

        void OnApplicationPaused(bool paused);

    }
}
