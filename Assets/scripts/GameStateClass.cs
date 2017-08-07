using UnityEngine;
using System.Collections;
using System.IO;

public class GameStateClass : MonoBehaviour 
{

	public static int NUMBER_OF_LEVELS = 16;
	public bool[] m_unlockedLevels;
	public bool[] m_shownTutorials;
	int m_levelNumber;
    static GameStateClass m_instance = null;
	bool m_swipeTutorialHasBeenShown = false;
	int m_lastPlayedLevel = 0;


    public static GameStateClass Instance
    {
        get
        {
            return m_instance;
        }
    }

    public GameStateClass()
    {
    }

    // Use this for initialization
    void Start () 
	{
	}

	public void Initialise()
	{
		//AGTEMP - no music
		//AudioSource audio = GetComponent<AudioSource>();
		//audio.volume = 0f;

		m_unlockedLevels = new bool[NUMBER_OF_LEVELS];
		m_unlockedLevels[0] = true;
		m_shownTutorials = new bool[(int)TutorialManager.MessageID.LAST];
		m_instance = this;
		LoadProgressFromFile();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetLevelNumber(int num)
	{
		m_levelNumber = num;
	}

	public int GetLevelNumber()
	{
		return m_levelNumber;
	}

	public int GetLastPlayedLevelNumber()
	{
		return m_lastPlayedLevel;
	}

    public void CompletedLevel(int livesUsed)
    {
		Debug.Log ("CompletedLevel "+livesUsed+", "+m_levelNumber);

		if (m_levelNumber + 1 < NUMBER_OF_LEVELS) 
		{
			// only unlock up to the last of the available levels
			m_unlockedLevels [m_levelNumber + 1] = true;
			SaveProgressToFile();
		}

		RecordLastPlayedLevel ();
    }

	public void RecordLastPlayedLevel()
	{
		m_lastPlayedLevel = m_levelNumber;
	}

	public void ShownTutorial (TutorialManager.MessageID tutorialID)
	{
		int i = (int)tutorialID;
		if (i < m_shownTutorials.Length) 
		{
			m_shownTutorials [i] = true;
		}
	}

	public bool HasTutorialBeenShown(TutorialManager.MessageID tutorialID)
	{
		int i = (int)tutorialID;
		if (i < m_shownTutorials.Length) 
		{
			return m_shownTutorials [i];
		}

		return false;
	}


	//AGTEMP - untested code below. There must be a better way
	// (e.g. make a simple class to contain just 1 int, which is the highest
	// level unlocked. Serialize that to json.)

	public class SaveClass
	{
		public int levelReached;
	}

	void SaveProgressToFile()
	{
		Debug.Log("SaveProgressToFile 1");
		SaveClass saveClass = new SaveClass();
		for(int i=0; i<NUMBER_OF_LEVELS; i++)
		{
			if (m_unlockedLevels[i])
			{
				saveClass.levelReached = i;
			}
		}
		Debug.Log("SaveProgressToFile 2");

		string json = JsonUtility.ToJson (saveClass);
		string path = Application.persistentDataPath + "/savefile.json";
		StreamWriter writer = new StreamWriter (path, false);
		Debug.Log("SaveProgressToFile 3");

		writer.WriteLine (json);
		writer.Close ();
		Debug.Log("SaveProgressToFile 4");

	}

	void LoadProgressFromFile()
	{
		string path = Application.persistentDataPath + "/savefile.json";
		if (System.IO.File.Exists(path))
		{
			StreamReader reader = new StreamReader (path);
			string json = reader.ReadToEnd ();
			reader.Close ();
			if (json != null)
			{
				SaveClass saveClass = JsonUtility.FromJson<SaveClass> (json);
				for(int i=0; i<NUMBER_OF_LEVELS; i++)
				{
					Debug.Log("LoadProgressFromFile "+i+" size = "+m_unlockedLevels.Length);
					m_unlockedLevels[i] = (i <= saveClass.levelReached);
				}
			}
		}
	}

	public void FadeOutMusic()
	{
		AudioSource audio = GetComponent<AudioSource>();
		StartCoroutine(FadeOut(audio,1f));
	}

	public void FadeInMusic()
	{
		AudioSource audio = GetComponent<AudioSource>();
		StartCoroutine(FadeIn(audio,0.5f,1f));
	}

	public static IEnumerator FadeOut (AudioSource audioSource, float FadeTime) {
		float startVolume = audioSource.volume;

		while (audioSource.volume > 0) {
			audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

			yield return null;
		}

		//audioSource.Stop ();
		audioSource.volume = 0f;
	}

	public static IEnumerator FadeIn (AudioSource audioSource, float volume, float FadeTime) {
		audioSource.volume = 0f;
		float startVolume = audioSource.volume;
		//audioSource.Play ();

		while (audioSource.volume < volume) {
			audioSource.volume += volume * Time.deltaTime / FadeTime;

			yield return null;
		}

		audioSource.volume = volume;
	}


}
