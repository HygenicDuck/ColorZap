using UnityEngine;
using System.Collections;

public class Lane : MonoBehaviour {

	[SerializeField] private SpriteRenderer m_laneSprite;

	// Use this for initialization
	void Start () {
	
	}
	
	public void SetLaneColor(Color c)
	{
		m_laneSprite.color = c;
	}
}
