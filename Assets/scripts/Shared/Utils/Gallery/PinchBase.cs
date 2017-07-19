using UnityEngine;
using System.Collections;
using Core;
using UnityEngine.EventSystems;

public class PinchBase : BaseBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	private System.Action m_getFocusCallback;
	private System.Action m_loseFocusCallback;


	void OnDisable()
	{
		Input.multiTouchEnabled = false;

		if (HasFocus())
		{
			LoseFocus();
		}
	}

	private static bool s_thereIsTarget = false;
	protected int m_pointDownCount = 0;
	int m_firstPointerId;

	public void SetCallbacks(System.Action getFocusCallback, System.Action loseFocusCallback)
	{
		m_getFocusCallback = getFocusCallback;
		m_loseFocusCallback = loseFocusCallback;
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		if (!s_thereIsTarget || m_pointDownCount > 0)
		{
			if (m_pointDownCount == 0)
			{
				m_firstPointerId = pointerEventData.pointerId;
				GetFocus();
			}
			m_pointDownCount++;
		}

	}

	public void OnPointerUp(PointerEventData pointerEventData)
	{
		if (m_pointDownCount > 0)
		{
			m_pointDownCount--;

			if (m_pointDownCount <= 0 || pointerEventData.pointerId == m_firstPointerId)
			{
				LoseFocus();
				m_pointDownCount = 0;
			}
		}
	}

	public void OnDrag(PointerEventData pointerEventData)
	{

	}

	public void ForceFocus()
	{
		m_pointDownCount = 1;
		GetFocus();
	}
		
	protected bool HasFocus()
	{
		return m_pointDownCount > 0;
	}

	protected virtual void GetFocus()
	{
		Input.multiTouchEnabled = true;
		s_thereIsTarget = true;
		if (m_getFocusCallback != null)
		{
			m_getFocusCallback();
		}
	}
	protected virtual void LoseFocus()
	{
		s_thereIsTarget = false;
		if (m_loseFocusCallback != null)
		{
			m_loseFocusCallback();
		}
	}

}
