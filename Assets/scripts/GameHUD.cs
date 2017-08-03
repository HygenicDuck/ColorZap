using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour {

	[SerializeField] private Text m_currentScore;
	[SerializeField] private Text m_highScore;
	[SerializeField] private Text m_multiplier;
	[SerializeField] private GameObject m_scorePrefab;
	private Camera m_sceneCamera;

	private static GameHUD s_instance = null;

	public GameHUD()
	{
		s_instance = this;
	}

	public static GameHUD GetInstance()
	{
		return s_instance;
	}

	void Start()
	{
		GameObject cam = GameObject.Find ("Main Camera");
		m_sceneCamera = cam.GetComponent<Camera> ();
	}

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

	Vector2 WorldCoordsToUICoords(Vector3 worldPos)
	{
		Vector3 screenPos = m_sceneCamera.WorldToScreenPoint (worldPos);

		return screenPos;
	}

	public void ShowEnemyShotScore(Vector3 enemyPos)
	{
		Vector3 screenPos = WorldCoordsToUICoords (enemyPos);
		screenPos -= new Vector3 (517f, 260f, 0f);
		screenPos *= 2f;
		GameObject scoreObject = Instantiate (m_scorePrefab, transform.parent);
		scoreObject.transform.localPosition = screenPos;
	}

}
