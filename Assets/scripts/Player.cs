using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour 
{
	const float ROTATION_TIME = 0.2f;
	Rotation m_rotator;
	EnemyColor m_color = EnemyColor.purple;
	Vector3 m_originalScale;

	// Use this for initialization
	void Start () 
	{
		m_rotator = GetComponent<Rotation>();
		m_rotator.SetRotation(Quaternion.identity);
		SetColor(EnemyColor.red);
		m_originalScale = transform.localScale;
	}

	public void RotateToLane(int laneNum)
	{
		int numberOfLanes = GameSettings.Instance.numberOfLanes;
		float angleSpread = GameSettings.Instance.angleSpread;
		float angleBetweenLanes = angleSpread / (numberOfLanes - 1);
		float leftMostAngle = -((numberOfLanes - 1)*angleBetweenLanes)/2;

		float angle = -(leftMostAngle + laneNum*angleBetweenLanes);
		m_rotator.RotateTo(Quaternion.Euler(0f,0f,angle), ROTATION_TIME);
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void SetColor(EnemyColor color)
	{
		if (m_color != color)
		{
			PlayerScore.GetInstance().ColourChanged();

			m_color = color;

			Transform child = transform.GetChild(0);
			SpriteRenderer sr = child.gameObject.GetComponent<SpriteRenderer>();

			EnemyManager.Instance.SetColor(sr,color);
		}
	}

	public void DoColorTransition(EnemyColor color)
	{
		if (m_color != color)
		{
			IEnumerator coroutine = DoColorTransitionCoRoutine(color);
			StartCoroutine(coroutine);
		}
	}

	IEnumerator DoColorTransitionCoRoutine(EnemyColor color)
	{
		const float SHRINK_TIME = 0.1f;

		SpriteScaler scaler = GetComponent<SpriteScaler>();

		Transform rt = gameObject.GetComponent<Transform>();

		scaler.ScaleToAbsolute(Vector3.zero,SHRINK_TIME);
		yield return new WaitForSeconds(SHRINK_TIME);
		SetColor(color);
		scaler.ScaleToAbsolute(m_originalScale,SHRINK_TIME);
	}

	public EnemyColor GetColor()
	{
		return m_color;
	}
}
