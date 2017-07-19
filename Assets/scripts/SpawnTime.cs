using UnityEngine;
using System.Collections;

[System.Serializable]
public class SpawnTime
{
	[SerializeField] private int m_spawns;
	[SerializeField] private float m_time;

	public int GetSpawns()
	{
		return m_spawns;
	}

	public float GetTime()
	{
		return m_time;
	}
}