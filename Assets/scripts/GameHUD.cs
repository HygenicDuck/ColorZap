using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour {

	[SerializeField] private Text m_currentScore;
	[SerializeField] private Text m_highScore;
	[SerializeField] private Text m_multiplier;
	[SerializeField] private GameObject m_scorePrefab;
	[SerializeField] private CanvasScaler m_canvasScaler;

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
		m_canvasScaler = GetComponentInParent<CanvasScaler> ();
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
		Debug.Log ("WorldCoordsToUICoords " + screenPos.x + ", " + screenPos.y);

		return screenPos;
	}

	public void ShowEnemyShotScore(Vector3 enemyPos)
	{
		Debug.Log ("screen xdim : " + Screen.width);
		Debug.Log ("screen ydim : " + Screen.height);
		Vector3 screenPos = WorldCoordsToUICoords (enemyPos);
		screenPos -= new Vector3 (Screen.width/2, Screen.height/2, 0f);
		screenPos *= (m_canvasScaler.referenceResolution.x / Screen.width);
		GameObject scoreObject = Instantiate (m_scorePrefab, transform.parent);
		scoreObject.transform.localPosition = screenPos;
	}

}
