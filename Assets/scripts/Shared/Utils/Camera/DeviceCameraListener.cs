using UnityEngine;
using System.Collections;
using Utils;

public class DeviceCameraListener : MonoSingleton<DeviceCameraListener>
{
    protected override void Init()
    {
    }

    void Update()
    {
        DeviceCamera.CheckForFrame();
    }

	protected override void OnDestroy()
    {
        Debugger.Log("Destroying Camera", (int)SharedSystems.Systems.CAMERA_FEED);
        DeviceCamera.StopCameraCapture();
    }

    public void _OnCameraFrameAvailale()
    {
        Debugger.Log("Camera frame available", (int)SharedSystems.Systems.CAMERA_FEED);
        DeviceCamera._OnCameraFrameAvailale();
    }

    public void _OnCameraAuthorised()
    {
        Debugger.Log("Camera has been authorised", (int)SharedSystems.Systems.CAMERA_FEED);
        DeviceCamera._OnCameraAuthorised();
    }

    public void _OnPictureTaken(string path)
    {
        Debugger.Log("Picture taken: " + path, (int)SharedSystems.Systems.CAMERA_FEED);
        DeviceCamera._OnPictureTaken(path);
    }

    public void _OnCameraAuthorisedUpdated(string statusString)
    {
        DeviceCamera.AccessStatus status = DeviceCamera.AccessStatus.NOT_DETERMINED;

        if (System.Enum.IsDefined(typeof(DeviceCamera.AccessStatus), status))
        {
            status = (DeviceCamera.AccessStatus)System.Enum.Parse(typeof(DeviceCamera.AccessStatus), statusString);
        }

        Debugger.Log("_OnCameraAuthorised " + status, (int)SharedSystems.Systems.CAMERA_FEED);
    }

    public void _OnPictureTakenFailed(string failed)
    {
        Debugger.Log("_OnPictureTakenFailed with string: " + failed, (int)SharedSystems.Systems.CAMERA_FEED);
    }

    public void OnApplicationPause(bool paused)
    {
		Debugger.Log("OnApplicationPause: " + paused, (int)SharedSystems.Systems.CAMERA_FEED);

        DeviceCamera.OnApplicationPaused(paused);
    }
}