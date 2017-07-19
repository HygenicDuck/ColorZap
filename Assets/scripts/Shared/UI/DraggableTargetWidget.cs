using UnityEngine;
using System.Collections;
using System;
using UI;
using UnityEngine.EventSystems;
using Utils;
using UnityEngine.UI;

public class DraggableTargetWidget : UIElement
{
	public interface IDelegate
	{
		void MovedIntoTarget(bool enter, DraggableWidget.Payload payload);
		void Dragged(Vector2 pos);
		bool CanRecievePayload(DraggableWidget.Payload payload);
		void PayloadRecieved(DraggableWidget.Payload payload, bool withinTarget);
	}

	[SerializeField] private RectTransform m_targetRoot;
	[SerializeField] private bool m_sendDroppedWithinTargetReceipt;
	[SerializeField] private int m_dragGroupID;
	[SerializeField] private int m_targetID;

	private IDelegate m_delegate;
	private bool m_withinBounds;

	protected override void Awake()
	{
		base.Awake();
	}

	public void OnEnable()
	{
		AddEventHandlers();
	}

	public void OnDisable()
	{
		RemoveEventHandlers();
	}

	public override void PopulateUIElements()
	{
	}

	public void Configure(IDelegate del)
	{
		m_delegate = del;
	}

	public RectTransform GetTargetRectTransform()
	{
		return m_targetRoot;
	}

	public void SetTargetRectTansform(RectTransform target)
	{
		m_targetRoot = target;
	}

	private void AddEventHandlers()
	{
		Utils.DragEvents.DragMoved += HandleDragMoved;
		Utils.DragEvents.DragDropped += HandleDragDropped;
	}

	private void RemoveEventHandlers()
	{
		Utils.DragEvents.DragMoved -= HandleDragMoved;
		Utils.DragEvents.DragDropped -= HandleDragDropped;
	}

	private void HandleDragMoved(int groupID, Vector2 position, DraggableWidget.Payload payload)
	{
		if (groupID == m_dragGroupID)
		{
			bool withinBounds = RectTransformUtility.RectangleContainsScreenPoint(m_targetRoot, position);

			if (withinBounds != m_withinBounds)
			{
				bool enable = withinBounds;

				if (payload != null)
				{
					bool targetCanReceivePayload = m_delegate.CanRecievePayload(payload);

					enable = enable && targetCanReceivePayload;
				}

				m_delegate.MovedIntoTarget(enable, payload);

				m_withinBounds = withinBounds;
			}

			m_delegate.Dragged(position);
		}
	}

	private void HandleDragDropped(int groupID, DraggableWidget.Payload payload)
	{
		if (groupID == m_dragGroupID)
		{
			if (m_withinBounds && m_delegate.CanRecievePayload(payload))
			{
				if (m_sendDroppedWithinTargetReceipt)
				{	
					Utils.DragEvents.SendDragDroppedWithinTargetEvent(m_dragGroupID, m_targetID, m_targetRoot);
				}
			}

			m_delegate.PayloadRecieved(payload, m_withinBounds);

			m_withinBounds = false;
		}
	}
}