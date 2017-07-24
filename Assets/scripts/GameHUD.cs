using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour {

	[SerializeField] private Text m_currentScore;
	[SerializeField] private Text m_highScore;
	[SerializeField] private Text m_multiplier;

	void Update()
	{
		m_currentScore.text = PlayerScore.GetInstance().GetCurrentScore().ToString();

		int multipier = Mathf.Max(1, PlayerScore.GetInstance().GetCurrentMultiplier());
		if (multipier > 1)
		{
			m_multiplier.text = multipier.ToString();
			m_multiplier.gameObject.SetActive(true);
		}
		else
		{
			m_multiplier.gameObject.SetActive(false);
		}

		m_highScore.text = PlayerScore.GetInstance().GetHighScore().ToString();
	}

}
