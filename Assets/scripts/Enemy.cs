using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Facebook.Unity;


public enum EnemyColor
{
	red,
	yellow,
	green,
	blue,
	purple
}


public class Enemy : MonoBehaviour 
{
	IntVec2 m_gridPos;
	EnemyColor m_color = EnemyColor.blue; 
	Vector3 m_originalScale;

	// Use this for initialization
	void Start () {
		m_originalScale = transform.localScale;
	}

	// Update is called once per frame
	void Update () 
	{

	}

	static public float GetLaneAngle(int lane)
	{
		int numberOfLanes = GameSettings.Instance.numberOfLanes;
		float angleSpread = GameSettings.Instance.angleSpread;
		float angleBetweenLanes = angleSpread / (numberOfLanes - 1);
		float leftMostAngle = -((numberOfLanes - 1)*angleBetweenLanes)/2;
		float angle = leftMostAngle + lane*angleBetweenLanes;
		return angle;
	}

	public void RefreshScreenPosition()
	{
		SetEnemyRotation();
		Vector3 pos = GetScreenPosition();
		transform.localPosition = pos;
	}

	public Vector3 GetScreenPosition()
	{
		float laneAngleInRadians = GetLaneAngle(m_gridPos.x) * Mathf.Deg2Rad;
		float rankDistance = (GameSettings.Instance.maxEnemyDistance - GameSettings.Instance.minEnemyDistance) / (GameSettings.Instance.numberOfRanksPerLane-1);
		float rankRadius = GameSettings.Instance.maxEnemyDistance - (m_gridPos.y * rankDistance);

		Vector3 pos = new Vector3(rankRadius*Mathf.Sin(laneAngleInRadians), rankRadius*Mathf.Cos(laneAngleInRadians));

		//Debug.Log("lane angle "+GetLaneAngle(m_gridPos.x));

		return pos;
	}

	void SetEnemyRotation()
	{
		transform.localRotation = Quaternion.Euler(0f,0f,-GetLaneAngle(m_gridPos.x));
	}

	public void SetColor(EnemyColor color)
	{
		m_color = color;

		Transform child = transform.GetChild(0);
		SpriteRenderer sr = child.gameObject.GetComponent<SpriteRenderer>();

		EnemyManager.Instance.SetColor(sr,color);
	}

	public void DoColorTransition(EnemyColor color)
	{
		IEnumerator coroutine = DoColorTransitionCoRoutine(color);
		StartCoroutine(coroutine);
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

	public void SetGridPos(int lane, int rank)
	{
		m_gridPos.x = lane;
		m_gridPos.y = rank;
	}

	public int GetLane()
	{
		return m_gridPos.x;
	}

	public int GetRank()
	{
		return m_gridPos.y;
	}

	public void MoveForward()
	{
		m_gridPos.y++;
		Vector3 pos = GetScreenPosition();
		GetComponent<MoveSprite>().MoveTo(pos,0.2f);

		if (m_gridPos.y > GameSettings.Instance.numberOfRanksPerLane-1)
		{
			DoDeathSequence();
//			EnemyManager.Instance.DoPlayerDeathExplosion();
//
//			PlayerScore.GetInstance().PlayerDead();
//
//			// game over
//			SceneManager.LoadScene("GameOver");
//			AudioManager.Instance.PlayAudioClip("gameOver");
		}
	}

	public void DoDeathSequence()
	{
		IEnumerator coroutine = DoDeathSequenceCoRoutine();
		StartCoroutine(coroutine);
	}

	IEnumerator DoDeathSequenceCoRoutine()
	{
		Root.Instance.m_ignoreInput = true;

		EnemyManager.Instance.DoPlayerDeathExplosion();
		AudioManager.Instance.PlayAudioClip("playerExplode");
		AudioManager.Instance.PlayAudioClip("gameOver");
		PlayerScore.GetInstance().PlayerDead();
		Root.Instance.PlayerIsDead ();

        LogGameOverEvent(EnemySpawner.Instance.GetSpawnCount());

        yield return new WaitForSeconds(2.0f);

		Root.Instance.m_ignoreInput = false;

		SceneManager.LoadScene("GameOver");
	}

    public void LogGameOverEvent(int enemiesSpawned)
    {
        var parameters = new Dictionary<string, object>();
        parameters["EnemiesSpawned"] = enemiesSpawned;
        FB.LogAppEvent(
            "GameOver",
            null,
            parameters
        );
    }
}

