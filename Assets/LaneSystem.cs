using UnityEngine;
using System.Collections;

public class LaneSystem : MonoBehaviour {

	[SerializeField] private Color[] m_laneColors;
	public GameObject m_lanePrefab;

	// Use this for initialization
	void Start () 
	{
		int numberOfLanes = GameSettings.Instance.numberOfLanes;
		float angleSpread = GameSettings.Instance.angleSpread;
		float angleBetweenLanes = angleSpread / (numberOfLanes - 1);
		float leftMostAngle = -((numberOfLanes - 1)*angleBetweenLanes)/2;

		for (int i=0; i<numberOfLanes; i++)
		{
			GameObject lane = Instantiate(m_lanePrefab);
			lane.transform.SetParent(transform);
			lane.transform.localPosition = Vector3.zero;
			float angle = leftMostAngle + i*angleBetweenLanes;
			lane.transform.localRotation = Quaternion.Euler(0f,0f,angle);
			if (m_laneColors.Length > 0)
			{
				lane.GetComponent<Lane>().SetLaneColor(m_laneColors[i % m_laneColors.Length]);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
