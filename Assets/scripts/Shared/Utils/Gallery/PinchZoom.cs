using Core;
using UnityEngine;
using UnityEngine.UI;

public class PinchZoom : PinchBase
{
	[SerializeField]
	private float m_zoomSpeed = 0.1f;

	[SerializeField]
	private float m_minUV = 0.1667f;



	private Vector3 m_startPress = Vector3.zero;

	protected override void GetFocus()
	{
		base.GetFocus();
	}

	protected override void LoseFocus()
	{
		base.LoseFocus();
		m_startPress = Vector3.zero;
	}

	void Update()
	{
		if (!HasFocus())
		{
			return;
		}

#if UNITY_EDITOR
		if (Input.GetKey(KeyCode.Space))
		{
			float deltaMagnitudeDiff = Input.mouseScrollDelta.y;
			Vector3 pinchPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
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

			Vector3 pinchPoint = touchZero.position;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;
#endif
			RawImage image = GetComponent<RawImage>();
			Rect uvRect = image.uvRect;

			m_startPress = Vector3.zero;

			// Frame dependant plus a speed factor
			deltaMagnitudeDiff *= m_zoomSpeed * Time.deltaTime;

			Vector3 uvs = uvRect.size;

			// add the magnitude to the scale in the same scale space
			Vector3 uvsFrame = new Vector3 (deltaMagnitudeDiff, deltaMagnitudeDiff);

			// make sure we don't zoom in or zoom out too far
			if (uvs.x + uvsFrame.x > 1f)
			{
				uvsFrame.x = 1f - Mathf.Min(1f, uvs.x);
			}

			if (uvs.y + uvsFrame.y > 1f)
			{
				uvsFrame.y = 1f - Mathf.Min(1f, uvs.y);
			}

			if (uvs.x + uvsFrame.x < m_minUV)
			{
				uvsFrame.x = -(Mathf.Max(m_minUV, uvs.x) - m_minUV);
			}

			if (uvs.y + uvsFrame.y < m_minUV)
			{
				uvsFrame.y = -(Mathf.Max(m_minUV, uvs.y) - m_minUV);
			}

			uvs += uvsFrame;

			RectTransform rectTransform = this.GetRectTransform();
			Vector2 size = rectTransform.GetSize();

			RectTransform parentTransform = (RectTransform)transform.parent;
			Vector2 parentSize = parentTransform.GetSize();

			parentSize = transform.localRotation * parentSize;

			parentSize.x = Mathf.Abs(parentSize.x);
			parentSize.y = Mathf.Abs(parentSize.y);

			Vector2 orignalSize = size;

			Vector2 sizeDiff = new Vector2 ((1f - (parentSize.x / size.x)) * 0.5f, (1f - (parentSize.y / size.y)) * 0.5f);

			size.x /= uvs.x;
			size.y /= uvs.y;

			if (Mathf.Abs(size.x) < parentSize.x || Mathf.Abs(size.y) < parentSize.y)
			{
				return;
			}

			pinchPoint = pinchPoint - transform.position;

			pinchPoint = Quaternion.Inverse(transform.localRotation) * pinchPoint;

			pinchPoint.x /= transform.lossyScale.x;
			pinchPoint.y /= transform.lossyScale.y;

			pinchPoint.x *= uvs.x;
			pinchPoint.y *= uvs.y;

			Vector2 pivot = new Vector2((pinchPoint.x + (orignalSize.x * 0.5f)) / orignalSize.x, (pinchPoint.y + (orignalSize.y * 0.5f)) / orignalSize.y);

			uvRect.size = uvs;

			// get the scale this frame and pivot it from the right place
			Vector2 positionAdjustment = new Vector2((uvsFrame.x) * pivot.x, (uvsFrame.y) * pivot.y);

			Vector2 uvPosition = uvRect.position;

			// apply the position adjust so it scale from the right place
			uvPosition -= positionAdjustment;

			uvRect.position = ConstrainUVSToBounds(uvPosition, uvs, sizeDiff);

			image.uvRect = uvRect;
		}
		else if (Input.touchCount == 1 || Input.GetMouseButton(0))
		{
			RectTransform rectTransform = this.GetRectTransform();
			Vector2 size = rectTransform.GetSize();
			        
			RectTransform parentTransform = (RectTransform)transform.parent;
			Vector2 parentSize = parentTransform.GetSize();

			parentSize = transform.localRotation * parentSize;

			parentSize.x = Mathf.Abs(parentSize.x);
			parentSize.y = Mathf.Abs(parentSize.y);

			Vector2 sizeDiff = new Vector2 ((1f - (parentSize.x / size.x)) * 0.5f, (1f - (parentSize.y / size.y)) * 0.5f);

			Vector3 press = Input.mousePosition;

			if (m_startPress == Vector3.zero)
			{
				m_startPress = press;
			}

			Vector3 frameMovement = m_startPress - press;
			RawImage image = GetComponent<RawImage>();
			Rect uvRect = image.uvRect;

			frameMovement = Quaternion.Inverse(transform.localRotation) * frameMovement;

			frameMovement.x /= transform.lossyScale.x;
			frameMovement.y /= transform.lossyScale.y;

			frameMovement.x *= uvRect.size.x;
			frameMovement.y *= uvRect.size.y;

			Vector2 diff = new Vector2(frameMovement.x / size.x, frameMovement.y / size.y);

			Vector2 uvPosition = uvRect.position + diff;

			uvRect.position = ConstrainUVSToBounds(uvPosition, uvRect.size, sizeDiff);

			image.uvRect = uvRect;

			m_startPress = press;
		}
		else
		{
			m_startPress = Vector3.zero;
		}
	}

	// clamp the UVs to the edges so we don't see white space
	private Vector3 ConstrainUVSToBounds(Vector3 uvPosition, Vector3 uvs, Vector2 halfDiff)
	{

		halfDiff.y = halfDiff.y * uvs.y;
		halfDiff.x = halfDiff.x * uvs.x;

		if (uvPosition.y < -halfDiff.y)
		{
			uvPosition.y = -halfDiff.y;
		}

		if (uvPosition.y + uvs.y > 1f + halfDiff.y)
		{
			uvPosition.y = (1f + halfDiff.y) - uvs.y;
		}

		if (uvPosition.x < -halfDiff.x)
		{
			uvPosition.x = -halfDiff.x;
		}

		if (uvPosition.x + uvs.x > 1f + halfDiff.x)
		{
			uvPosition.x = (1f + halfDiff.x) - uvs.x;	
		}

		return uvPosition;
	}
}