using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {

    [SerializeField]
    GameObject[] m_tutorialRoots;

	static TutorialManager m_instance = null;

	bool[] m_tutorialsAlreadyShown;


	public enum MessageID
	{
		SHOOT_SAME_COLOUR_TO_DESTROY = 0,
		SHOOT_DIFFERENT_COLOUR_TO_SWAP,
		TAP_LANE_TO_FIRE,
		LAST
	}

	public static TutorialManager Instance
	{
		get
		{
			return m_instance;
		}
	}




	// Use this for initialization
	void Start ()
    {
		m_instance = this;
        ClearAllTutorialMessages();
		m_tutorialsAlreadyShown = new bool[m_tutorialRoots.Length];
    }
	
	// Update is called once per frame
	void Update () {
		
	}

	bool HasTutorialBeenShown (MessageID tutorialID)
	{
		if (GameStateClass.Instance != null)
		{
			Debug.Log("TutorialHasBeenShown = "+GameStateClass.Instance.TutorialHasBeenShown+", m_tutorialsAlreadyShown = "+m_tutorialsAlreadyShown[(int)tutorialID]);
			return GameStateClass.Instance.TutorialHasBeenShown || m_tutorialsAlreadyShown [(int)tutorialID];
		}
		return m_tutorialsAlreadyShown [(int)tutorialID];
	}

	void TutorialHasBeenShown (MessageID tutorialID)
	{
		m_tutorialsAlreadyShown [(int)tutorialID] = true;
		bool allTutorialsShown = true;
		foreach(bool b in m_tutorialsAlreadyShown)
		{
			allTutorialsShown = allTutorialsShown && b;
		}

		if (allTutorialsShown)
		{
			if (GameStateClass.Instance != null)
			{
				GameStateClass.Instance.TutorialHasBeenShown = true;
			}
		}
	}

	public void ShowTutorialMessageAfterADelay(float time, MessageID tutorialID)
	{
		StartCoroutine(ShowTutorialMessageAfterADelayCoroutine(time, tutorialID));
	}

	IEnumerator ShowTutorialMessageAfterADelayCoroutine(float time, MessageID tutorialID)
	{
		yield return new WaitForSeconds (time);
		ShowTutorialMessage (tutorialID);
	}

    public void ShowTutorialMessage(MessageID tutorialID)
    {
//		if (!GameStateClass.Instance.HasTutorialBeenShown (tutorialID)) 
		if (!HasTutorialBeenShown (tutorialID)) 
		{
			for (int i = 0; i < m_tutorialRoots.Length; i++) 
			{
				m_tutorialRoots [i].SetActive (i == (int)tutorialID);
			}

//			GameStateClass.Instance.ShownTutorial (tutorialID);
			TutorialHasBeenShown(tutorialID);
			Root.Instance.PauseGame (true);
			AudioManager.Instance.PlayAudioClip ("tutorialMessage");
		}
    }

    public void ClearAllTutorialMessages()
    {
        for (int i = 0; i < m_tutorialRoots.Length; i++)
        {
            m_tutorialRoots[i].SetActive(false);
        }
    }

	public void ClearedTutorialMessage()
	{
		AudioManager.Instance.PlayAudioClip ("buttonPress");
		Root.Instance.PauseGame (false);
	}

	public void ShootWhereTapped()
	{
		Vector3 pos = Input.mousePosition;
		Root.Instance.HandleTouch (pos);
	}
}
