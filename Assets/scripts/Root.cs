using UnityEngine;
using System.Collections;
using Facebook.Unity;

public class GameSettings
{
	public static GameSettings Instance
	{
		get
		{
			return instance;
		}
	}

	private static GameSettings instance;

	public int numberOfLanes = 5;
	public float angleSpread = 85f;

	public int numberOfRanksPerLane = 8;
	public float maxEnemyDistance = 8f;
	public float minEnemyDistance = 2f;

	public GameSettings()
	{
		instance = this;
	}
}





public class Root : MonoBehaviour
{
	public static Root Instance
	{
		get
		{
			return instance;
		}
	}

	private static Root instance;

	[SerializeField] private GameObject m_prefabHUD;
	[SerializeField] private Transform m_HUDTransform;
	[SerializeField] private SpriteScaler m_shieldScaler;
	[SerializeField] private AudioSource m_ambienceSound;

	public GameObject m_playerPrefab;
	public Transform m_gamePlayRoot;

	enum GameState
	{
		GET_READY,
		PLAYING
	}

	GameState m_gameState;


	GameSettings m_gameSettings = new GameSettings ();
	Player m_player;
	float enemyTimer = 0f;
	float m_timeToNextRowOfEnemies = 0.5f;
	bool m_enemiesScrolled = false;
	public bool m_ignoreInput = false;

	public EnemyManager m_enemyManager;

	public Root()
	{
		instance = this;
	}

	void Awake()
	{
		CreateHUD();
		FB.Init(FBInitCallback);
		m_shieldScaler.SetScale (Vector3.zero);
	}

	private void FBInitCallback()
	{
		if (FB.IsInitialized)
		{
			FB.ActivateApp();
		}
	}

	public void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			if (FB.IsInitialized)
			{
				FB.ActivateApp();
			}
		}
	}

	// Use this for initialization
	void Start()
	{
		PlayerScore.GetInstance ().ResetScore ();
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		GameObject player = Instantiate(m_playerPrefab);
		player.transform.SetParent(m_gamePlayRoot);
		player.transform.localPosition = Vector3.zero;
		m_player = player.GetComponent<Player>();
		enemyTimer = 0f;
		AudioManager.Instance.Initialise();
		m_gameState = GameState.GET_READY;
		StartCoroutine (StartGamePlayAfterDelay ());
	}

	private void CreateHUD()
	{
		GameObject go = Instantiate(m_prefabHUD) as GameObject;
		go.transform.SetParent(m_HUDTransform, false);
	}

	void Update()
	{
		if (m_gameState == GameState.PLAYING) 
		{
			if (Input.GetMouseButtonDown (0) && !m_ignoreInput) {
				Vector3 pos = Input.mousePosition;
				HandleTouch (pos);
			}

			UpdateEnemies ();
		}
	}

	IEnumerator StartGamePlayAfterDelay()
	{
		yield return new WaitForSeconds (3.0f);
		m_gameState = GameState.PLAYING;
		ShowPlayerSprite(true);
		m_ambienceSound.volume = 1f;
		yield return new WaitForSeconds (0.5f);
		m_shieldScaler.ScaleToAbsolute (new Vector3 (3f, 3f, 1f), 0.8f);
	}

	void UpdateEnemies()
	{
		enemyTimer += Time.deltaTime;

		if (!m_enemiesScrolled)
		{
			float enemyScrollTime = Mathf.Max(0f, m_timeToNextRowOfEnemies - 0.3f);
			if (enemyTimer >= enemyScrollTime)
			{
				m_enemyManager.ScrollEnemiesIn();
				m_enemiesScrolled = true;
				AudioManager.Instance.PlayAudioClip("enemiesMove");
			}
		}

		if (enemyTimer >= m_timeToNextRowOfEnemies)
		{
			enemyTimer = 0f;
			SpawnLine spawnLine = EnemySpawner.Instance.GetNextSpawn();
			m_timeToNextRowOfEnemies = spawnLine.GetTimeToNext();
			m_enemiesScrolled = false;
			m_enemyManager.SpawnNewEnemies(spawnLine);
			AudioManager.Instance.PlayAudioClip("enemiesAppear");
		}
	}


	void HandleTouch(Vector3 screenTouchPos)
	{
		// work out which lane was touched

		Camera camera = Camera.main;

		Vector3 rootPos = camera.WorldToScreenPoint(transform.position);

		Vector3 raySpacePos = screenTouchPos - rootPos;
		float angle = Mathf.Atan2(raySpacePos.x, raySpacePos.y) * Mathf.Rad2Deg;

		int numberOfLanes = GameSettings.Instance.numberOfLanes;
		float angleSpread = GameSettings.Instance.angleSpread;
		float angleBetweenLanes = angleSpread / (numberOfLanes - 1);
		float leftMostAngle = -((numberOfLanes - 1) * angleBetweenLanes) / 2;

		int lane = (int)((angle - leftMostAngle + (angleBetweenLanes / 2)) / angleBetweenLanes);
		lane = Mathf.Clamp(lane, 0, numberOfLanes - 1);

		m_player.RotateToLane(lane);

		DoFire(lane);
	}


	void DoFire(int lane)
	{
		EnemyColor newColor = m_enemyManager.PlayerShoots(lane, m_player.GetColor());

		//m_player.SetColor(newColor);
		m_player.DoColorTransition(newColor);

		AudioManager.Instance.PlayAudioClip("fireBullet");
	}


	public void RotateTest()
	{
		int lane = Random.Range(0, GameSettings.Instance.numberOfLanes);
		m_player.RotateToLane(lane);
	}

	public void ShowPlayerSprite(bool show)
	{
		m_player.gameObject.SetActive (show);
	}
}
