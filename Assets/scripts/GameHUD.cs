using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour {

	[SerializeField] private Text m_currentScore;
	[SerializeField] private Text m_highScore;
	[SerializeField] private Text m_multiplier;
	[SerializeField] private Scaler m_multiplierScaler;
	[SerializeField] private Visibility m_multiplierVisibility;
	[SerializeField] private GameObject m_scorePrefab;
	[SerializeField] private CanvasScaler m_canvasScaler;

	private Camera m_sceneCamera;
	private int m_previousMultiplier;

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
		m_canvasScaler = GetComponentInParent<CanvasScaler> ();
		m_multiplierScaler.SetScale (Vector3.one);
		m_multiplierVisibility.SetAlpha (0f);
		m_previousMultiplier = 1;
	}

	void Update()
	{
		m_currentScore.text = PlayerScore.GetInstance().GetCurrentScore().ToString();

		int multipier = Mathf.Max(1, PlayerScore.GetInstance().GetCurrentMultiplier());
		if (m_previousMultiplier != multipier) 
		{
			m_previousMultiplier = multipier;
			if (multipier > 1) 
			{
				m_multiplier.text = "x" + multipier.ToString ();
				m_multiplier.gameObject.SetActive (true);
				m_multiplierScaler.SetScale (Vector3.one);
				m_multiplierVisibility.SetAlpha (1f);
			} 
			else 
			{
				const float fadeTime = 0.25f;
				m_multiplierScaler.ScaleToAbsolute (new Vector3 (4f, 4f), fadeTime, true);
				m_multiplierVisibility.FadeOut (fadeTime*0.8f);
				//m_multiplier.gameObject.SetActive(false);
			}
		}

		m_highScore.text = PlayerScore.GetInstance().GetHighScore().ToString();
	}

	Vector2 WorldCoordsToUICoords(Vector3 worldPos)
	{
		Vector3 screenPos = m_sceneCamera.WorldToScreenPoint (worldPos);

		return screenPos;
	}

	public void ShowEnemyShotScore(Vector3 enemyPos, int scoreInc)
	{
		Vector3 screenPos = WorldCoordsToUICoords (enemyPos);
		screenPos -= new Vector3 (Screen.width/2, Screen.height/2, 0f);
		screenPos *= (m_canvasScaler.referenceResolution.x / Screen.width);
		GameObject scoreObject = Instantiate (m_scorePrefab, transform.parent);
		scoreObject.transform.localPosition = screenPos;

		const float fadeTime = 0.5f;

		Scaler scaler = scoreObject.GetComponent<Scaler> ();
		scaler.ScaleTo (3f, fadeTime, true);

		Visibility vis = scoreObject.GetComponent<Visibility> ();
		vis.FadeOut (fadeTime*2);

		Text text = scoreObject.GetComponent<Text> ();
		text.text = scoreInc.ToString ();
	}

}
