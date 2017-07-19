using System;
using UnityEngine;
using UnityEngine.UI;

public class Rotation : MonoBehaviour
{
	Quaternion startRot;
	Quaternion destinationRot;
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

	public void RotateTo (Quaternion p, float time)
	{
		startRot = transform.localRotation;
		destinationRot = p;
		travelTime = time;
		timePassed = 0.0f;
		moving = true;
	}

	public void ResetRotation()
	{
		// reset the transform to the start position
		transform.localRotation = startRot;
		moving = false;
	}
	
	public void SetRotation(Quaternion p)
	{
		transform.localRotation = p;
		moving = false;
	}
	
	public void RotateBy (Quaternion offset, float time, bool linear = false)
	{
		startRot = transform.localRotation;
		destinationRot = startRot*offset;
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
			Quaternion q = Quaternion.Lerp (startRot, destinationRot, t);
			//Vector2 p = Vector2.Lerp(startRot,destinationRot,t);
			transform.localRotation = q;
		}
	}
}
