// Conversion of standard DragRigidbody.js to DragRigidbody2D.cs
// Ian Grant v001
using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour
{
	// Class Variables
//	[SerializeField] private float m_timeBetweenSpawns = 1.0f;
//	[SerializeField] private int m_maxSpawns = 1;
//	[SerializeField] private GameObject[] m_blocks;
//	[SerializeField] private Transform m_blocksRoot;
//	[SerializeField] private BoxCollider2D m_range;

	float m_timer;
	float m_moveDuration;
	Vector3 m_startPos;
	Vector3 m_destPos;
	bool m_moving;



	void Awake()
	{
		m_moving = false;
		m_timer = 0f;
	}

	// Update
	void Update ()
	{
		if (m_moving)
		{
			m_timer += Time.deltaTime;
			if (m_timer >= m_moveDuration)
			{
				m_timer = m_moveDuration;
				m_moving = false;
				ReachedDestination();
			}

			//float p = m_timer / m_moveDuration;
            Vector3 dpos = m_destPos - m_startPos;
            Vector3 pos = new Vector3(
                QuadraticLerp(m_timer, m_startPos.x, dpos.x, m_moveDuration),
                QuadraticLerp(m_timer, m_startPos.y, dpos.y, m_moveDuration),
                QuadraticLerp(m_timer, m_startPos.z, dpos.z, m_moveDuration)
                );
            //Vector3 pos = m_startPos + (m_destPos - m_startPos)*p;
			transform.position = pos;
		}
	}

    float QuadraticLerp(float t, float b, float c, float d)
    {
        t /= d / 2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t - 2) - 1) + b;
    }

    public void MoveBy(Vector3 vec, float duration)
	{
		MoveTo(transform.position + vec, duration);
	}

	public void MoveTo(Vector3 vec, float duration)
	{
		m_timer = 0f;
		m_moveDuration = duration;
		m_moving = true;
		m_startPos = transform.position;
		m_destPos = vec;
//		Rigidbody2D rb = GetComponent<Rigidbody2D>();
//		rb.isKinematic = true;
	}

	void ReachedDestination()
	{
//		Rigidbody2D rb = GetComponent<Rigidbody2D>();
//		rb.isKinematic = false;
	}

    public void SetPosition(Vector3 pos)
    {
        m_moving = false;
        m_timer = 0f;
        transform.position = pos;
    }

}

