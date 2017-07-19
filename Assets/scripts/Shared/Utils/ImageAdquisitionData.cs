using UnityEngine;
using System.Collections;

public class ImageAdquisitionData
{
	public enum ChannelType
	{
		GOOGLE,
		CAMERA,
		ROLL,
		FACEBOOK
	}

	private string m_channel;
	private Vector2 m_resolution;

	public string Channel { get { return m_channel; } }
	public Vector2 Resolution { get { return m_resolution; } }


	public ImageAdquisitionData(ChannelType channel, Vector2 originalResolution)
	{
		Set(channel, originalResolution);
	}

	public void Set(ChannelType channel, Vector2 originalResolution)
	{
		m_resolution = originalResolution;
		m_channel = channel.ToString();
	}
}
