using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHand : MonoBehaviour {

    [SerializeField]
    public GameObject m_handRoot;

    [SerializeField]
    public GameObject m_handSprite;

    [SerializeField]
    public GameObject m_startPoint;

    [SerializeField]
    public GameObject m_endPoint;

    [SerializeField]
    public float m_strokeTime;

    [SerializeField]
    public float m_handDownTime;

    [SerializeField]
    public float m_handDownDistance;



    // Use this for initialization
    void Start ()
    {
        StartCoroutine(MoveHand());
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    private void OnEnable()
    {
        StartCoroutine(MoveHand());
    }

    IEnumerator MoveHand()
    {
        Mover handMover = m_handRoot.GetComponent<Mover>();
        Mover handSpriteMover = m_handSprite.GetComponent<Mover>();

        while (true)
        {
			Vector3 startPos = m_startPoint.transform.position;	// - new Vector3(0f, m_handDownDistance, 0f);
            handMover.SetPosition(startPos);
            handSpriteMover.MoveBy(new Vector3(0, m_handDownDistance, 0f), m_handDownTime);
            yield return new WaitForSeconds(m_handDownTime);
            handMover.MoveTo(m_endPoint.transform.position, m_strokeTime);
            yield return new WaitForSeconds(m_strokeTime);
            handSpriteMover.MoveBy(new Vector3(0, -m_handDownDistance, 0f), m_handDownTime);
            yield return new WaitForSeconds(m_handDownTime);
        }
    }

    public void ClearTutorialMessage()
    {
        gameObject.SetActive(false);
    }
}
