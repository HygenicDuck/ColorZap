using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public void PlaySound(string sfxName)
	{
		AudioManager.Instance.PlayAudioClip(sfxName);
	}
}
