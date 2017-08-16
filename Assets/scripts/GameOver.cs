using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour {

	[SerializeField] private Text m_currentScoreText;
	[SerializeField] private Text m_highScoreText;
	[SerializeField] private Color m_winnerColor;
	[SerializeField] private Color m_loserColor;
	[SerializeField] private Visibility m_startButtonVis;

	public void Start()
	{
		m_highScoreText.text = PlayerScore.GetInstance().GetHighScore().ToString();
		m_currentScoreText.text = PlayerScore.GetInstance().GetCurrentScore().ToString();

		m_currentScoreText.color = PlayerScore.GetInstance().GetCurrentScore() > PlayerScore.GetInstance().GetHighScore() ? m_winnerColor : m_loserColor;

		m_startButtonVis.SineScale (0.03f, 100000f, 0.4f);

		AudioManager.Instance.PlayAudioClip("gameOverText");
	}

	public void OnPlayPressed()
	{
		AudioManager.Instance.PlayAudioClip ("startButtonPress");

		Debug.Log ("OnPlayPressed");
		SceneManager.LoadScene("menu");
	}
}
