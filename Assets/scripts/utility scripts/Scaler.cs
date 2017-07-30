using UnityEngine;
using System.Collections;

public class Scaler : MonoBehaviour {

	[SerializeField] private AnimationCurve scaleCurve = AnimationCurve.Linear(0,0,1,1);

	private Vector3 startScale;
	private Vector3 destinationScale;
	private float travelTime;
	private float timePassed;
	private bool moving;
	private ScaleTypes scaleType;

	private enum ScaleTypes
	{
		none = 0,
		linear,
		cubic,
		useCurve
	}
		

	// Use this for initialization
	private void Start()
	{
		
	}
	
	private void Awake()
	{
		moving = false;
	}
	
	// multiplies current scale by given factor, and then interpolates from that back to the currect scale
	public void ScaleBackFrom(float scaleFactor, float time, bool linear = false)
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		destinationScale = rt.localScale;
		startScale = destinationScale * scaleFactor;
		rt.localScale = startScale;
		travelTime = time;
		timePassed = 0.0f;
		moving = true;
		scaleType = linear ? ScaleTypes.linear : ScaleTypes.cubic;
	}

	// multiplies current scale by given factor, and scales toward it
	public void ScaleTo(float scaleFactor, float time, bool linear = false)
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		ScaleToAbsolute(rt.localScale * scaleFactor,time,linear);
	}

	// scales towards the given scale
	public void ScaleToAbsolute(Vector3 scale, float time, bool linear = false)
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		startScale = rt.localScale;
		destinationScale = scale;
		travelTime = time;
		timePassed = 0.0f;
		moving = true;
		scaleType = linear ? ScaleTypes.linear : ScaleTypes.cubic;
	}

	public void ScaleUsingCurve(float time)
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		startScale = rt.localScale;
		travelTime = time;
		timePassed = 0.0f;
		moving = true;
		scaleType = ScaleTypes.useCurve;
	}

	public void SetScale(Vector3 scale)
	{
		RectTransform rt = gameObject.GetComponent<RectTransform>();
		rt.localScale = scale;
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
				
			Vector3 p;

			if (scaleType == ScaleTypes.useCurve)
			{
				t = scaleCurve.Evaluate(t);
				p = new Vector3(startScale.x*t, startScale.y*t, 1f);
			}
			else
			{
				if (scaleType == ScaleTypes.cubic)
				{
					t = t*t * (3f - 2f*t);
				}
				p = Vector3.Lerp(startScale,destinationScale,t);
			}

			RectTransform rt = gameObject.GetComponent<RectTransform>();
			p.z = 1.0f;
			rt.localScale = p;
		}
	}

}
