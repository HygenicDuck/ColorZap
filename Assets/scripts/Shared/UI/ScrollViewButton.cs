using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class ScrollViewButton : MonoBehaviour  {

	private const string BUTTON_NORMAL_ANIM = "Normal";

	Animator m_animator;
	Button m_button;
	Vector3 m_lastPosition;

	void Awake()
	{
		m_animator = GetComponent<Animator>();
		m_button = GetComponent<Button>();

	}

	private IEnumerator OneFrameDelayedAnim (string anim)
	{
		yield return new WaitForEndOfFrame();

		m_animator.Play(anim);
	}

	public void Update () 
	{
		if (m_button.interactable)
		{
			if (transform.position != m_lastPosition)
			{
				StartCoroutine(OneFrameDelayedAnim(BUTTON_NORMAL_ANIM));
				m_lastPosition = transform.position;
			}
		}
	}

	public void SetInteractable(bool interactable)
	{
		m_button.interactable = interactable;
		if (interactable)
		{
			StartCoroutine(OneFrameDelayedAnim(BUTTON_NORMAL_ANIM));
		}
	}
}
