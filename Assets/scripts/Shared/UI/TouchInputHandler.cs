using UnityEngine;
using System.Collections;
using Core;
using UnityEngine.EventSystems;

public class TouchInputHandler : BaseBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
	public delegate void InputEventHandler (Vector2 position);
	public event InputEventHandler TouchUpInsideOccured;

	private bool m_dragValid = false;

	public void OnPointerDown(PointerEventData eventData)
	{
		m_dragValid = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (m_dragValid)
		{
			if (TouchUpInsideOccured != null)
			{
				TouchUpInsideOccured(eventData.position);
			}
		}
		m_dragValid = false;
	}

	public void OnDrag(PointerEventData eventData)
	{
		const float MIN_DISTANCE = 1;
		if (!ClickValid(eventData, MIN_DISTANCE))
		{
			m_dragValid = false;
		}
	}

	private static bool ClickValid(PointerEventData eventData, float minDistance)
	{
		Vector2 point1 = eventData.pressPosition;
		Vector2 point2 = eventData.position;
		float xDiff = Mathf.Abs(point1.x - point2.x);
		float yDiff = Mathf.Abs(point1.y - point2.y);
		return xDiff <= minDistance || yDiff <= minDistance;
	}
}