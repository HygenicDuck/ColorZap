using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets._2D
{
    public class ScaleMovement : MonoBehaviour
    {
		private Vector2 startScale;
		private Vector2 destinationScale;
		private float travelTime;
		private float timePassed;
		private bool moving;
		private bool isLinear;

		// Use this for initialization
		private void Start()
		{

		}

		private void Awake()
		{
			moving = false;
		}

		public void ScaleTo (Vector2 scale, float time, bool linear = false)
		{
			RectTransform rt = gameObject.GetComponent<RectTransform>();
			startScale = rt.localScale;
			destinationScale = scale;
			travelTime = time;
			timePassed = 0.0f;
			moving = true;
			isLinear = linear;
		}

		public void ScaleBy (Vector2 offset, float time, bool linear = false)
		{
			RectTransform rt = gameObject.GetComponent<RectTransform>();
			startScale = rt.localScale;
			destinationScale = startScale+offset;
			travelTime = time;
			timePassed = 0.0f;
			moving = true;
			isLinear = linear;
		}

		private void Update()
		{
			if (moving)
			{
				timePassed += Time.deltaTime;
				float t = timePassed / travelTime;
				if (t >= 1) 
				{
					t = 1;
					moving = false;
				}
				if (!isLinear)
					t = t*t * (3f - 2f*t);
				Vector2 p = Vector2.Lerp(startScale,destinationScale,t);
				RectTransform rt = gameObject.GetComponent<RectTransform>();
				rt.localScale = new Vector3(p.x, p.y, rt.localPosition.z);
			}
		}
    }
}
