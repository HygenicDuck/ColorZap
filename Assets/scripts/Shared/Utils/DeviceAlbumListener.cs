using UnityEngine;
using System.Collections;
using Utils;
using NativeGallery;

public class DeviceAlbumListener : MonoSingleton<DeviceAlbumListener>
{
	protected override void Init()
	{
	}

	public void OnAlbumAuthorised(string val)
	{
		Debugger.Log("album authorised " + val, (int)SharedSystems.Systems.UIIMAGE_PICKER);

		UIImagePicker._OnAlbumAuthorised(((UIImagePicker.AccessStatus)int.Parse(val)));
	}
}