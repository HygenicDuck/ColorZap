using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColourThrob : MonoBehaviour {

	// continuously interpolates between the specified colours

	[SerializeField] Color[] colors;
	[SerializeField] float timeBetweenColours;
	[SerializeField] bool preferTextOverOutline = false;
	int _currentColorIndex = 0;
	float timer = 0;

	
	private Text _text = null;
	private Outline _outline = null;
	
	
	void Start () {
		_currentColorIndex = 0;
		_text = gameObject.GetComponent<Text>();
		_outline = gameObject.GetComponent<Outline>();
	}

	void SetColor(Color c)
	{
		if ((_outline != null) && !preferTextOverOutline)
		{
			Color old = _outline.effectColor;
			c.a = old.a;
			_outline.effectColor = c;
		}
		else if (_text != null)
		{
			Color old = _text.color;
			c.a = old.a;
			_text.color = c;
		}
	}

	// Update is called once per frame
	void Update () 
	{
		int nextColourIndex = _currentColorIndex+1;
		if (nextColourIndex >= colors.Length)
			nextColourIndex = 0;

		float pTime = timer/timeBetweenColours;
		Color c = new Color(Mathf.Lerp (colors[_currentColorIndex].r, colors[nextColourIndex].r, pTime),
		                    Mathf.Lerp (colors[_currentColorIndex].g, colors[nextColourIndex].g, pTime),
		                    Mathf.Lerp (colors[_currentColorIndex].b, colors[nextColourIndex].b, pTime)
		                    );
		SetColor (c);

		timer += Time.deltaTime;
		if (timer > timeBetweenColours)
		{
			timer -= timeBetweenColours;
			_currentColorIndex++;
			if (_currentColorIndex >= colors.Length)
				_currentColorIndex = 0;
		}
	}
}
