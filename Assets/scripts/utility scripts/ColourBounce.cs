using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets._2D
{
    public class ColourBounce : MonoBehaviour
    {
		// continuously interpolates between the two specified colours

		[SerializeField] Color _colour1;
		[SerializeField] Color _colour2;
		[SerializeField] float _cycleTime;
		[SerializeField] MaskableGraphic[] _otherImages;

		float _cyclePos = 0f;

		// Use this for initialization
		private void Start()
		{
		}

		public void DoBounce (Color colour1, Color colour2, float cycleTime, float cyclePos=0f)
		{
			_colour1 = colour1;
			_colour2 = colour2;
			_cycleTime = cycleTime;
			_cyclePos = cyclePos;		// goes from 0 to 1
		}

		public void Restart(float pos=0f)
		{
			_cyclePos = pos;
		}

		private void Update()
		{
			//RawImage ri = GetComponent<RawImage>();
			_cyclePos += Time.deltaTime / _cycleTime;
			while (_cyclePos > 1f)
				_cyclePos -= 1f;

			float i = _cyclePos * 2;
			if (i > 1f) 
				i = 2f - i;

			Color outColour = Color.Lerp(_colour1, _colour2, i);

			foreach(MaskableGraphic raw in _otherImages)
			{
				// preserve alpha
				Color c = raw.color;
				c.r = outColour.r;
				c.g = outColour.g;
				c.b = outColour.b;
				raw.color = c;
			}
		}

		private void Awake()
		{
		}
    }
}
