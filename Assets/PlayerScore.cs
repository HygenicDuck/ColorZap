using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerScore : FileIO
{
	private const string SAVE_FILE = "player_details.txt";
	private const string KEY_HIGH_SCORE = "high_score";
	private const int DEFAULT_HIGH_SCORE = 0;
	private int m_highScore = 0;


	private const int ENEMY_KILL_SCORE = 100;
	private int m_scoreMultiplier = 0;
	private int m_playerScore = 0;

	private static PlayerScore s_instance = null;

	public static PlayerScore GetInstance()
	{
		if (s_instance == null)
		{
			s_instance = new PlayerScore ();
			s_instance.Initialise();
		}

		return s_instance;
	}

	private PlayerScore() : base()
	{
	}

	~PlayerScore()
	{
	}

	protected override void Initialise()
	{
		base.Initialise();
	}


	// EXTERNAL HANDS

	public void EnemyKilled()
	{
		m_scoreMultiplier += 1;
		m_playerScore += (ENEMY_KILL_SCORE * m_scoreMultiplier);
	}

	public void PlayerDead()
	{
		SaveScore();
	}

	public void ColourChanged()
	{
		m_scoreMultiplier = 0;
	}

	private void SaveScore()
	{
		if (m_playerScore > m_highScore)
		{
			m_highScore = m_playerScore;
			SaveData();
		}
	}

	public int GetCurrentScore()
	{
		return m_playerScore;
	}

	public int GetHighScore()
	{
		return m_highScore;
	}

	public int GetCurrentMultiplier()
	{
		return m_scoreMultiplier;
	}

	// FILE IO

	protected override string GetFileName()
	{
		return SAVE_FILE;
	}

	protected override void HandleHashtableFromRead(Hashtable hashtable)
	{
		m_highScore = hashtable[KEY_HIGH_SCORE] as int? ?? DEFAULT_HIGH_SCORE;
	}

	protected override Hashtable CreateHashtableToWrite()
	{
		Hashtable contentsHashtable = new Hashtable ();

		contentsHashtable.Add(KEY_HIGH_SCORE, m_playerScore);

		return contentsHashtable;
	}

}
