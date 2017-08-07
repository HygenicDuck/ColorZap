﻿using System.Collections;
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
		return m_tutorialsAlreadyShown [(int)tutorialID];
	}

	void TutorialHasBeenShown (MessageID tutorialID)
	{
		m_tutorialsAlreadyShown [(int)tutorialID] = true;
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
		Root.Instance.PauseGame (false);
	}
}