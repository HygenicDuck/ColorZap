using UnityEngine;
using System.Collections;

[System.Serializable]
public class SpawnLine
{
	[SerializeField] private int[] m_laneColours = new int[EnemySpawner.LANES];
	[SerializeField] private float m_timeToNext = 5f;

	System.Random random = new System.Random ();

	public void CreateData(EnemySpawner.Colours maxColour)
	{
		m_laneColours = new int[EnemySpawner.LANES];

		for (int i = 0; i < EnemySpawner.LANES; i++)
		{
			m_laneColours[i] = (int)GetRandomColour(maxColour);
		}

		m_timeToNext = 3f;
	}

	public void SetTimeToNext(float time)
	{
		m_timeToNext = time;
	}

	public float GetTimeToNext()
	{
		return m_timeToNext;
	}

	public int GetColorAtLane(int lane)
	{
		return m_laneColours[lane];
	}

	private EnemySpawner.Colours GetRandomColour(EnemySpawner.Colours maxColour)
	{
		int nextInt = random.Next((int)(maxColour) + 1);
		return (EnemySpawner.Colours)nextInt;
	}

	public void Print()
	{
		Debug.Log("m_timeToNext: " + m_timeToNext + "[" + m_laneColours[0] + ", " + m_laneColours[1] + ", " + m_laneColours[2] + ", " + m_laneColours[3] + ", " + m_laneColours[4] + "]");
	}
}