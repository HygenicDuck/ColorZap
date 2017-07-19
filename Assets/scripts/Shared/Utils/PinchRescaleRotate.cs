using Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PinchRescaleRotate : PinchBase
{
	private const float ZOOMSPEED = 0.01f;


	[SerializeField]
	private float m_minScale = 0.5f;

	[SerializeField]
	private bool m_comeToTopOnDrag;


	private Canvas m_tempCanvas;
	private Vector3 m_initialPressPosition = Vector3.zero;


#if UNITY_EDITOR
	private float m_previousMagnitude = 0f;
	private float m_previousAngle = 0f;
#endif
	private Vector3 m_posDiff;

	private bool m_firstTouch = false;
	private bool m_secondTouch = false;

	Transform m_oldParent;

	public void ResetDif()
	{
		m_posDiff = Vector2.zero;
	}

	protected override void GetFocus()
	{
		base.GetFocus();
		if (m_comeToTopOnDrag)
		{
			m_oldParent = transform.parent;
			transform.SetParent(UI.ScreenManager.Instance.GetRootCanvas().transform, true);
		}
	}

	protected override void LoseFocus()
	{
		base.LoseFocus();
		m_firstTouch = false;
		m_secondTouch = false;
		m_initialPressPosition = Vector3.zero;

		if (m_comeToTopOnDrag)
		{
			transform.SetParent(m_oldParent, true);
		}
	}

	void FixedUpdate()
	{

		if (!HasFocus())
		{
			return;
		}

#if UNITY_EDITOR
		Vector3 inputPosition = Input.mousePosition;

		if (Input.GetKey(KeyCode.Space) && Input.GetMouseButton(0))
		{
			Vector3 positionInCameraSpace = Camera.main.WorldToScreenPoint(transform.position);

			Vector3 positionDif = inputPosition -  positionInCameraSpace;
			float magnitude = positionDif.magnitude;
			float angle = AngleOf(positionDif);

			if (!m_secondTouch)
			{
				m_initialPressPosition = inputPosition;
				m_previousMagnitude = magnitude;
				m_previousAngle = angle;
			}

			float deltaMagnitude = magnitude - m_previousMagnitude;
			m_previousMagnitude = magnitude;

			float deltaAngle =  m_previousAngle - angle;
			m_previousAngle = angle;
				
#else
		// If there are two touches on the device...
		if (Input.touchCount == 2)
		{
			// Store both touches.
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

			
			// Find the magnitude of the vector (the distance) between the touches in each frame.
			Vector3 prevTouchDelta = (touchZeroPrevPos - touchOnePrevPos);
			Vector3 touchDelta = (touchZero.position - touchOne.position);

			// Find the difference in the distances between each frame.
			float deltaMagnitude = touchDelta.magnitude - prevTouchDelta.magnitude;

			//Find dife angles
			float deltaAngle =  AngleOf(prevTouchDelta) - AngleOf(touchDelta);
#endif
			deltaMagnitude *= ZOOMSPEED;

			Vector3 localScale = transform.localScale;
			localScale.x += deltaMagnitude;
			if (localScale.x < m_minScale)
			{
				localScale.x = m_minScale;
			}
			localScale.y = localScale.x;
			transform.localScale = localScale;


			transform.Rotate(0,0,deltaAngle);

			m_secondTouch = true;
			m_firstTouch = false;
		}
		else if (Input.touchCount == 1 || (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)))
		{

#if UNITY_EDITOR
			Vector3 touchPositionWorld = Camera.main.ScreenToWorldPoint(inputPosition);
#else
			Touch touchZero = Input.GetTouch(0);
			Vector3 touchPositionWorld = Camera.main.ScreenToWorldPoint(touchZero.position);
#endif

			if (!m_firstTouch)
			{
				m_posDiff = transform.position - touchPositionWorld;
			}
			else
			{
				transform.position = touchPositionWorld + m_posDiff;
			}

			m_firstTouch = true;
			m_secondTouch = false;
		}
		else
		{
			m_firstTouch = false;
			m_secondTouch = false;
		}

	}

	public float AngleOf(Vector2 vector)
	{
		float angle = Mathf.Asin(vector.y / vector.magnitude);
		if (vector.x > 0)
		{
			angle = Mathf.PI - angle;
		}
		return angle * Mathf.Rad2Deg;
	}

}

