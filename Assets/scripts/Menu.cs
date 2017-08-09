using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
	
	[SerializeField] private Visibility m_startButtonVis;

	// Use this for initialization
	void Start () {
		m_startButtonVis.SineScale (0.03f, 100000f, 0.4f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartGame()
	{
		Debug.Log ("Press");
		AudioManager.Instance.PlayAudioClip ("startButtonPress");

		SceneManager.LoadScene("main");
	}
}
