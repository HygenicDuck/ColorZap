﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class EnemyManager : MonoBehaviour 
{
	public static EnemyManager Instance
	{
		get
		{
			return instance;
		}
	}

	private static EnemyManager instance;


	public GameObject enemyPrefab;
	public Sprite[] sprites;
	public Color[] colors;
	public GameObject[] m_explosionPrefabs;
	public GameObject m_bulletPrefab;
	public GameObject m_playerDeathExplosionPrefab;

	List<Enemy> enemies = new List<Enemy>();

	public EnemyManager()
	{
		instance = this;
	}

	// Use this for initialization
	void Start () 
	{
	}

	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void SetColor(SpriteRenderer sr, EnemyColor color)
	{
		switch(color)
		{
		case EnemyColor.blue:
			//sr.color = colors[0];
			sr.sprite = sprites[0];
			break;
		case EnemyColor.green:
			//sr.color = colors[1];
			sr.sprite = sprites[1];
			break;
		case EnemyColor.purple:
			//sr.color = colors[2];
			sr.sprite = sprites[2];
			break;
		case EnemyColor.red:
			//sr.color = colors[3];
			sr.sprite = sprites[3];
			break;
		case EnemyColor.yellow:
			//sr.color = colors[4];
			sr.sprite = sprites[4];
			break;
		}
	}

	public Color ProperColourFromEnemyColour(EnemyColor color)
	{
		Color c = colors[0];
		switch(color)
		{
		case EnemyColor.blue:
			c = colors[0];
			break;
		case EnemyColor.green:
			c = colors[1];
			break;
		case EnemyColor.purple:
			c = colors[2];
			break;
		case EnemyColor.red:
			c = colors[3];
			break;
		case EnemyColor.yellow:
			c = colors[4];
			break;
		}

		return c;
	}

	void AddRandomEnemiesOnTopRank(SpawnLine spawnLine)
	{
		for(int i=0; i<GameSettings.Instance.numberOfLanes; i++)
		{
			int color = (int)spawnLine.GetColorAtLane(i);

			if (color > -1)
			{
				AddEnemyOnLane(i,(EnemyColor)color);
			}
		}
	}

	public void AddEnemyOnLane(int lane, EnemyColor color)
	{
		GameObject gOb = Instantiate(enemyPrefab);
		gOb.transform.SetParent(transform);
		Enemy enemy = gOb.GetComponent<Enemy>();
		enemies.Add(enemy);
		enemy.SetGridPos(lane,0);
		enemy.SetColor(color);
		enemy.RefreshScreenPosition();
	}

	public void ScrollEnemiesIn()
	{
		// scroll existing enemies down
		foreach(Enemy enemy in enemies)
		{
			enemy.MoveForward();
		}
	}

	public void SpawnNewEnemies(SpawnLine spawnLine)
	{
		// add new enemies
		AddRandomEnemiesOnTopRank(spawnLine);
	}

	Enemy EnemyAtPosition(int lane, int rank)
	{
		foreach(Enemy enemy in enemies)
		{
			if ((enemy.GetLane() == lane) && (enemy.GetRank() == rank))
			{
				return enemy;
			}
		}

		return null;
	}

	public EnemyColor PlayerShoots(int lane, EnemyColor color)
	{
		// returns the new colour of the player
		EnemyColor newPlayerColour = color;
		Vector3 bulletTarget = Vector3.zero;
		bool finished = false;
		bool hitEnemy = false;
		for(int i=GameSettings.Instance.numberOfRanksPerLane; i>=0 && !finished; i--)
		{
			Enemy enemy = EnemyAtPosition(lane,i);
			if (enemy != null)
			{
				if (enemy.GetColor() == color)
				{
					// explode enemy
					GameObject gOb = Instantiate(m_explosionPrefabs[0]);
					gOb.transform.SetParent(transform);
					Vector3 pos = enemy.transform.localPosition;
					pos.z -= 1.0f;
					gOb.transform.localPosition = pos;
					ParticleSystem ps = gOb.GetComponent<ParticleSystem>();
					ps.startColor = ProperColourFromEnemyColour(color);

					int scoreInc = PlayerScore.GetInstance().EnemyKilled();

					GameHUD.GetInstance ().ShowEnemyShotScore (enemy.gameObject.transform.position, scoreInc);
					enemies.Remove(enemy);
					Destroy(enemy.gameObject);
					enemy.transform.SetParent(null);

					AudioManager.Instance.PlayAudioClip("explosion");

					TutorialManager.Instance.ShowTutorialMessageAfterADelay (1f, TutorialManager.MessageID.SHOOT_SAME_COLOUR_TO_DESTROY);
				}
				else
				{
					newPlayerColour = enemy.GetColor();
					//enemy.SetColor(color);
					enemy.DoColorTransition(color);
					finished = true;
					AudioManager.Instance.PlayAudioClip("swapColour");

					bulletTarget = enemy.transform.localPosition;
					hitEnemy = true;

					TutorialManager.Instance.ShowTutorialMessageAfterADelay (0.5f, TutorialManager.MessageID.SHOOT_DIFFERENT_COLOUR_TO_SWAP);
				}
			}
		}

		if (!finished)
		{
			bulletTarget = GetLanePosOffTheScreen(lane);
		}

		CreateBullet(lane, bulletTarget, hitEnemy);

		return newPlayerColour;
	}


	static public Vector3 GetLanePosOffTheScreen(int lane)
	{
		float laneAngleInRadians = Enemy.GetLaneAngle(lane) * Mathf.Deg2Rad;
		float rankDistance = GameSettings.Instance.maxEnemyDistance * 1.5f;

		Vector3 pos = new Vector3(rankDistance*Mathf.Sin(laneAngleInRadians), rankDistance*Mathf.Cos(laneAngleInRadians));

		return pos;
	}

	void CreateBullet(int lane, Vector3 target, bool hitEnemy)
	{
		GameObject gOb = Instantiate(m_bulletPrefab);
		gOb.transform.parent = transform;
		gOb.transform.localPosition = Vector3.zero;
		float travelTime = 0.1f;
		MoveSprite mover = gOb.GetComponent<MoveSprite>();
		mover.MoveTo(target,travelTime,true);
		Lifetimer lt = gOb.GetComponent<Lifetimer>();
		lt.StartTimer(travelTime);
		gOb.transform.rotation = Quaternion.Euler(0f,0f,-Enemy.GetLaneAngle(lane));
		if (hitEnemy) 
		{
			StartCoroutine (DoRicochetBullet (lane, target, travelTime));
		}
	}

	IEnumerator DoRicochetBullet(int lane, Vector3 target, float travelTime)
	{
		yield return new WaitForSeconds (travelTime);
		GameObject gOb = Instantiate(m_bulletPrefab);
		gOb.transform.parent = transform;
		gOb.transform.localPosition = target;
		MoveSprite mover = gOb.GetComponent<MoveSprite>();
		mover.MoveTo(Vector3.zero,travelTime,true);
		Lifetimer lt = gOb.GetComponent<Lifetimer>();
		lt.StartTimer(travelTime);
		gOb.transform.rotation = Quaternion.Euler(0f,0f,-Enemy.GetLaneAngle(lane)+180f);
	}

	public void DoPlayerDeathExplosion()
	{
		GameObject gOb = Instantiate(m_playerDeathExplosionPrefab);
		gOb.transform.parent = transform;
		gOb.transform.localPosition = Vector3.zero;
		Root.Instance.ShowPlayerSprite (false);
	}
}
