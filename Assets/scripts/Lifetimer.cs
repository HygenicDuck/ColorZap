using UnityEngine;
using System.Collections;

public class Lifetimer : MonoBehaviour {
	[SerializeField] private float _lifeTime = 5.0f;
	[SerializeField] private bool _startOnSpawn = false;

	void Start () 
	{
		if (_startOnSpawn)
			StartTimer(_lifeTime);
	}

	public void StartTimer(float time)
	{
		Destroy(gameObject, time);
		//StartCoroutine(SelfDestructCoroutine(time));
	}

	IEnumerator SelfDestructCoroutine(float time) 
	{
		yield return new WaitForSeconds(time);
		Destroy(this.gameObject);
	}
}
