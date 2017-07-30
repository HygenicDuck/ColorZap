using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ColourStrobe : MonoBehaviour {

	// rapidly flickers between the primary colours

	int numberOfColors = 8;
	Color[] colors = null;
	int currentColorIndex = 1;
	
	private Text _text = null;
	private Outline _outline = null;


	// Use this for initialization
	void Start () {
		InitColors();
		_text = gameObject.GetComponent<Text>();
		_outline = gameObject.GetComponent<Outline>();
	}

	void InitColors()
	{
		colors = new Color[numberOfColors];

		for(int i=0; i<numberOfColors; i++)
		{
			colors[i] = new Color((i&1)*255, ((i&2)/2)*255, ((i&4)/4)*255);
		}
	}

	void SetColor(Color c)
	{
		if (_outline != null)
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
		SetColor(colors[currentColorIndex]);

		// misses out first and last to avoid black and white.
		currentColorIndex++;
		if (currentColorIndex >= numberOfColors-1)
			currentColorIndex = 1;
	}
}
