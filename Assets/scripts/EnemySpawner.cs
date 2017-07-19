using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoSingleton<EnemySpawner> 
{

	public const int LANES = 5;
	public enum Colours
	{
		RED,
		YELLOW,
		GREEN,
		BLUE,
		PURPLE,
		NUM
	}
		
	[SerializeField] private SpawnLine[] m_initialLines;
	[SerializeField] private SpawnTime[] m_spawnTimes;
	[SerializeField] private int m_lineIntroduceForthColour = 10;
	[SerializeField] private int m_lineIntroduceFifthColour = 20;

	private int m_spawnLine = 0;
	private Colours m_maxColour = Colours.GREEN;

	protected override void Init()
	{

	}

	public SpawnLine GetNextSpawn()
	{
		SpawnLine spawnLine = null;

		if (m_spawnLine < m_initialLines.Length)
		{
			spawnLine = m_initialLines[m_spawnLine];
		}
		else
		{
			spawnLine = new SpawnLine ();
			spawnLine.CreateData(m_maxColour);

			// SET THE TIME TO NEXT SPAWN
			for (int i = m_spawnTimes.Length - 1; i >= 0; i--)
			{
				SpawnTime spawnTime = m_spawnTimes[i];
				if (m_spawnLine >= spawnTime.GetSpawns())
				{
					spawnLine.SetTimeToNext(spawnTime.GetTime());
					break;
				}
			}
		}

		m_spawnLine++;


		// UNLOCK COLOURS
		if (m_spawnLine >= m_lineIntroduceFifthColour)
		{
			m_maxColour = Colours.PURPLE;
		}
		else if (m_spawnLine >= m_lineIntroduceForthColour)
		{
			m_maxColour = Colours.BLUE;
		}

		return spawnLine;
	}

	protected void Update()
	{
	}
}
