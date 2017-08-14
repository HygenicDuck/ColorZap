using UnityEngine;
using System.Collections;
using System.IO;

public class GameStateClass : MonoBehaviour 
{
	bool m_tutorialHasBeenShown = false;
	static GameStateClass m_instance = null;

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
		Initialise();
	}

	public void Initialise()
	{
		if (m_instance != null)
		{
			Destroy(this);
		}
		else
		{
			m_tutorialHasBeenShown = false;
			m_instance = this;
			DontDestroyOnLoad(this);
		}
	}

	public bool TutorialHasBeenShown
	{
		get
		{
			return m_tutorialHasBeenShown;
		}

		set
		{
			m_tutorialHasBeenShown = value;
		}
	}

}
