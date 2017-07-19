using UnityEngine;
using System.Collections;
using System;
using UI;
using UnityEngine.EventSystems;
using Utils;
using UnityEngine.UI;



public class DraggableWidget : UIElement, IPointerDownHandler
{
	public interface IDelegate
	{
		void ConfigureClone(GameObject cloneObject);
		void OnPayloadDelivered(RectTransform targetRectTrans, int targetID);
		void OnPayloadNotDelivered();
		void OnDragTriggered();
		void OnDragTriggeredButDisabled();
		bool DroppedInTarget(RectTransform targetRectTrans, int targetID);
		void Dragged(Vector2 pos);
	}

	public class Payload
	{
		// Currently empty, just used as an interface but we may need data here in the future.
	}

	public enum Direction
	{
		UP,
		DOWN,
		LEFT,
		RIGHT,
		ANY,
		NONE
	}

	private static int s_singleDragIsDragging = 0;
	private static int s_dragIDCurrentIndex = 1;

	[SerializeField] private float m_triggerPickupTime = 0.3f;
	[SerializeField] private float m_cancelPickupOnMovement = 8.0f;
	[SerializeField] private float m_triggerPickupOnMovement = 10.0f;
	[SerializeField] private Direction m_dragDirection;
	[SerializeField] private int m_dragGroupID;
	[SerializeField] private GameObject m_prefabCloneObject;

	private IDelegate m_delegate;
	private GameObject m_clonedObject;
	private string m_clonePrefabName;
	private int m_dragID;
	private bool m_dragEnabled = true;
	private bool m_dragActive;
	private bool m_droppedInTarget;
	private int m_droppedInTargetID;
	private Payload m_payload;
	private IEnumerator m_triggerDragCoroutine;
	private IEnumerator m_updateDragCoroutine;
	private Vector2 m_cloneStartPos;
	private Vector2 m_dragInitialPos;
	private Vector2 m_dragPos;
	private RectTransform m_droppedInTargetRectTrans = null;
	private bool m_assistedDragEnabled = true;
	private int m_fingerId = -1;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (m_clonedObject != null)
		{
			Destroy(m_clonedObject);	
		}
	}

	public void OnEnable()
	{
		m_dragID = s_dragIDCurrentIndex;
		s_dragIDCurrentIndex++;

		AddEventHandlers();
	}

	public void OnDisable()
	{
		if (m_dragID == s_singleDragIsDragging)
		{
			s_singleDragIsDragging = 0;
		}

		RemoveEventHandlers();
	}

	public override void PopulateUIElements()
	{
		
	}

	public void SetUpdateDrag(bool enable)
	{
		m_assistedDragEnabled = enable;
	}

	public void EnableDragging(bool enable)
	{
		m_dragEnabled = enable;
	}

	public void StopDraggingAndReset()
	{
		if (m_dragActive)
		{
			UnscheduleDraggingUpdate();
			FinishedTriggerDragTransition();
		}
	}

	public bool GetDragActive()
	{
		return m_dragActive;
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		#if UNITY_EDITOR
		OnPointerDownInternal(0);
		#else
		if (Input.touchCount == 1)
		{
			Touch touch = Input.GetTouch(0);
			OnPointerDownInternal(touch.fingerId);
		}
		#endif

	}

	public void OnPointerDownInternal(int fingerId)
	{
		Debugger.Log("PointerDown", (int)SharedSystems.Systems.DRAG_AND_DROP);
		if (s_singleDragIsDragging == 0)
		{
			m_dragInitialPos = GetTouchPosition();

			m_triggerDragCoroutine = ScheduleTriggerDrag(m_triggerPickupTime);

			StartCoroutine(m_triggerDragCoroutine);

			s_singleDragIsDragging = m_dragID;

			m_fingerId = fingerId;

			Debugger.Log("PointerDown Success " + m_fingerId, (int)SharedSystems.Systems.DRAG_AND_DROP);
		}
	}

	public void OnPointerUpInternal()
	{
		Debugger.Log("OnPointerUp success", (int)SharedSystems.Systems.DRAG_AND_DROP);

		UnscheduleTriggerDrag();
		if (m_dragActive)
		{
			FinishedTriggerDrag();
		}
		else
		{
			ResetDragData();
		}

		m_fingerId = -1;

	}

	public void OnPointerClick(PointerEventData data)
	{

	}
		
	private bool TestTriggerDragEarly(Vector2 worldPos)
	{
		bool ret = false;

		if (m_dragDirection != Direction.NONE)
		{
			float xDiff = worldPos.x - m_dragInitialPos.x;
			float yDiff = worldPos.y - m_dragInitialPos.y;

			float cancelDiff = -1f;
			if (m_dragDirection == Direction.UP ||
			    m_dragDirection == Direction.DOWN)
			{
				if (Mathf.Abs(yDiff) <= Mathf.Abs(xDiff))
				{
					cancelDiff = Mathf.Abs(xDiff);
				}
			}
			else if (m_dragDirection == Direction.LEFT ||
			         m_dragDirection == Direction.RIGHT)
			{
				if (Mathf.Abs(yDiff) >= Mathf.Abs(xDiff))
				{
					cancelDiff = Mathf.Abs(yDiff);
				}
			}

			if (cancelDiff >= 0f && cancelDiff > m_cancelPickupOnMovement)
			{
				UnscheduleTriggerDrag();
				ResetDragData();
			}
			else if (cancelDiff < 0f)
			{
				bool triggerDragNow = false;

				switch (m_dragDirection)
				{
				case Direction.UP:
				case Direction.DOWN:
					triggerDragNow = (Mathf.Abs(yDiff) >= m_triggerPickupOnMovement) ? true : false;
					break;
				case Direction.LEFT:
				case Direction.RIGHT:
					triggerDragNow = (Mathf.Abs(xDiff) >= m_triggerPickupOnMovement) ? true : false;
					break;
				case Direction.ANY:
					triggerDragNow = ((Mathf.Abs(xDiff) >= m_triggerPickupOnMovement) || (Mathf.Abs(yDiff) >= m_triggerPickupOnMovement)) ? true : false;
					break;
				}

				if (triggerDragNow)
				{
					UnscheduleTriggerDrag();
					TriggerDrag();
					ret = true;
				}
			}
		}

		return ret;
	}

	public static Vector2 GetTouchPosition()
	{
		Vector2 touchPos;
		#if UNITY_EDITOR
		touchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		#else
		touchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
		#endif
		return touchPos;
	}

	private bool IsTouchDown()
	{
		bool ret = false;
		#if UNITY_EDITOR
		ret = Input.GetMouseButton(0);
		#else
		if (Input.touchCount > 0)
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.fingerId == m_fingerId)
				{
					ret = true;
				}
			}
		}
		#endif
	
		return ret;
	}

	private void ResetDragData()
	{
		m_dragActive = false;
		m_dragInitialPos = Vector2.zero;
		m_dragPos = Vector2.zero;
		m_cloneStartPos = Vector2.zero;
		m_droppedInTarget = false;
		m_droppedInTargetRectTrans = null;
		m_droppedInTargetID = 0;

		s_singleDragIsDragging = 0;
	}

	private IEnumerator ScheduleTriggerDrag(float time)
	{
		Debugger.Log("ScheduleTriggerDrag", (int)SharedSystems.Systems.DRAG_AND_DROP);

		while (time > 0f)
		{
			yield return new WaitForEndOfFrame ();

			if (IsTouchDown())
			{
				if (TestTriggerDragEarly(GetTouchPosition()))
				{
					yield return 0;
				}
			}
			else
			{
				OnPointerUpInternal();
			}
	

			time -= Time.deltaTime;
		}

		TriggerDrag();
	}


	private void UnscheduleTriggerDrag()
	{
		StopCoroutine(m_triggerDragCoroutine);
	}

	private void TriggerDrag()
	{
		Debugger.Log("TriggerDrag", (int)SharedSystems.Systems.DRAG_AND_DROP);

		if (!m_dragEnabled)
		{
			m_delegate.OnDragTriggeredButDisabled();
			ResetDragData();
			return;
		}

		m_delegate.OnDragTriggered();

		CanvasGroup canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 0f;	
		}

		CreateClonedObject();

		m_dragActive = true;
		m_dragPos = m_dragInitialPos;

		m_clonedObject.SetActive(true);

		m_delegate.ConfigureClone(m_clonedObject);

		m_clonedObject.transform.position = new Vector2 (this.gameObject.transform.position.x, this.gameObject.transform.position.y);

		m_cloneStartPos = m_clonedObject.transform.position;

		Utils.DragEvents.SendDragPickedUpEvent(m_dragGroupID, m_payload);

		ScheduleDraggingUpdate();
	}

	private void FinishedTriggerDrag()
	{
		Debugger.Log("FinishedTriggerDrag", (int)SharedSystems.Systems.DRAG_AND_DROP);

		UnscheduleDraggingUpdate();

		Utils.DragEvents.SendDragDroppedEvent(m_dragGroupID, m_payload);

		if (m_droppedInTarget)
		{
			//TODO: some kind of scale to animation
			if (!m_delegate.DroppedInTarget(m_droppedInTargetRectTrans, m_droppedInTargetID))
			{
				m_delegate.OnPayloadDelivered(m_droppedInTargetRectTrans, m_droppedInTargetID);

				FinishedTriggerDragTransition();
			}
		}
		else
		{
			m_delegate.OnPayloadNotDelivered();

			//TODO: some kind of move to m_cloneTargetPos animation
			FinishedTriggerDragTransition();
		}
	}

	public void DroppedInTargetAnimationFinished()
	{
		m_delegate.OnPayloadDelivered(m_droppedInTargetRectTrans, m_droppedInTargetID);

		FinishedTriggerDragTransition();
	}

	private void FinishedTriggerDragTransition()
	{
		Debugger.Log("FinishedTriggerDragTransition", (int)SharedSystems.Systems.DRAG_AND_DROP);
		m_clonedObject.SetActive(false);

		CanvasGroup canvasGroup = this.gameObject.GetComponent<CanvasGroup>();
		if (canvasGroup != null)
		{
			canvasGroup.alpha = 1f;	
		}

		ResetDragData();			
	}

	private void ScheduleDraggingUpdate()
	{
		m_updateDragCoroutine = UpdateDragging();
		StartCoroutine(m_updateDragCoroutine);
	}

	IEnumerator UpdateDragging()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame ();

			if (IsTouchDown())
			{
				m_dragPos = GetTouchPosition();
				if (m_assistedDragEnabled)
				{
					Vector2 offset = m_dragPos - m_dragInitialPos;
					Vector2 newParentPos = m_cloneStartPos + offset;

					m_clonedObject.transform.position = newParentPos;
				}
			}
			else
			{
				OnPointerUpInternal();
			}


			if (m_dragActive)
			{
				Utils.DragEvents.SendDragMovedEvent(m_dragGroupID, m_dragPos, m_payload);
			}
		}
	}

	private void UnscheduleDraggingUpdate()
	{
		if (m_updateDragCoroutine != null)
		{
			StopCoroutine(m_updateDragCoroutine);	
		}
	}

	public void Configure(Payload payload, string prefabName)
	{
		m_payload = payload;
		m_clonePrefabName = prefabName;
	}

	public void SetGroupID(int id)
	{
		m_dragGroupID = id;
	}

	public void SetDelegate(IDelegate del)
	{
		m_delegate = del;
	}

	private void CreateClonedObject()
	{
		if (m_clonedObject == null)
		{
			if (m_prefabCloneObject != null)
			{
				m_clonedObject = LoadUIElement(m_prefabCloneObject, Vector3.zero, this.gameObject.transform.parent);
			}
			else if (!string.IsNullOrEmpty(m_clonePrefabName))
			{
				m_clonedObject = LoadUIElement(m_clonePrefabName, Vector3.zero, this.gameObject.transform.parent);
			}

			if (m_clonedObject != null)
			{
				DraggableWidget cloneDrag = m_clonedObject.GetComponent<DraggableWidget>();
				if (cloneDrag != null)
				{
					cloneDrag.enabled = false;
				}

				//TODO: Maybe the clone objects need to come from a manager which handles a pool of clones, and which canvas they are drawn on.
				Canvas canvas = m_clonedObject.AddComponent<Canvas>();
				canvas.overrideSorting = true;
				canvas.sortingOrder = 1;
				GameObject rootCanvas = ScreenManager.Instance.GetRootCanvas();
				m_clonedObject.transform.SetParent(rootCanvas.transform);
			}
		}
	}

	public GameObject GetClonedObject()
	{
		return m_clonedObject;
	}

	private void AddEventHandlers()
	{
		Utils.DragEvents.DragDroppedWithinTarget += HandleDroppedWithinTarget;
		Utils.DragEvents.DragMoved += HandleDragMoved;
	}

	private void RemoveEventHandlers()
	{
		Utils.DragEvents.DragDroppedWithinTarget -= HandleDroppedWithinTarget;
		Utils.DragEvents.DragMoved -= HandleDragMoved;
	}

	private void HandleDroppedWithinTarget(int dragGroupID, int targetID, RectTransform targetRectTrans)
	{
		if (m_dragActive)
		{
			if (dragGroupID == m_dragGroupID)
			{
				m_droppedInTargetRectTrans = targetRectTrans;
				m_droppedInTarget = true;
				m_droppedInTargetID = targetID;
			}
		}
	}

	private void HandleDragMoved(int groupID, Vector2 position, DraggableWidget.Payload payload)
	{
		if (groupID == m_dragGroupID)
		{
			m_delegate.Dragged(position);
		}
	}
}