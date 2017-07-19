using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

	[SerializeField] private Text m_currentScoreText;
	[SerializeField] private Text m_highScoreText;
	[SerializeField] private Color m_winnerColor;
	[SerializeField] private Color m_loserColor;

	public void Start()
	{
		m_highScoreText.text = PlayerScore.GetInstance().GetHighScore().ToString();
		m_currentScoreText.text = PlayerScore.GetInstance().GetCurrentScore().ToString();

		m_currentScoreText.color = PlayerScore.GetInstance().GetCurrentScore() > PlayerScore.GetInstance().GetHighScore() ? m_winnerColor : m_loserColor;
	}

	public void OnPlayPressed()
	{
		SceneManager.LoadScene("main");
	}
}
