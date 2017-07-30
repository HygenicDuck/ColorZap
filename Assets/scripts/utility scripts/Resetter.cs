using UnityEngine;
using System.Collections;

public class Resetter : MonoBehaviour {

	private Vector2 initialPos;
	private Quaternion initialRotation;
	private Vector3 initialScale;

	void Start () 
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		initialPos = rt.localPosition;
		initialRotation = rt.localRotation;
		initialScale = rt.localScale;
	}

	public void DoReset () 
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		rt.localPosition = initialPos;
		rt.localRotation = initialRotation;
		rt.localScale = initialScale;	
	}
}
