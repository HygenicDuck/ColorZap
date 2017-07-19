using System;
using UnityEngine;
using UnityEngine.UI;

public class MoveSprite : MonoBehaviour
{
	private Vector2 startPos;
	private Vector2 destinationPos;
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

	public void MoveTo (Vector2 p, float time, bool linear = false)
	{
		Transform rt = gameObject.GetComponent<Transform>();
		startPos = rt.localPosition;
		destinationPos = p;
		travelTime = time;
		timePassed = 0.0f;
		moving = true;
		isLinear = linear;
	}

	public void ResetPosition()
	{
		// reset the transform to the start position
		Transform rt = gameObject.GetComponent<Transform>();
		rt.localPosition = startPos;
		moving = false;
	}

	public void MoveBy (Vector2 offset, float time, bool linear = false)
	{
		Transform rt = gameObject.GetComponent<Transform>();
		startPos = rt.localPosition;
		destinationPos = startPos+offset;
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
			Vector2 p = Vector2.Lerp(startPos,destinationPos,t);
			Transform rt = gameObject.GetComponent<Transform>();
			rt.localPosition = new Vector3(p.x, p.y, rt.localPosition.z);
		}
	}
}
