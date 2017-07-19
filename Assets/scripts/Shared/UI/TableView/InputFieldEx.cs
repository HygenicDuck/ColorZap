//Script taken from Unity forums

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class InputFieldEx : InputField
{

	private bool m_routeToParent = false;
	private bool m_allowDrag = true;
	private bool m_wasInteractable;

	public void SetAllowDrag(bool allow)
	{
		m_allowDrag = allow;
	}

	/// <summary>
	/// Do action for all parents
	/// </summary>
	private void DoForParents<T>(Action<T> action) where T:IEventSystemHandler
	{
		Transform parent = transform.parent;
		while (parent != null)
		{
			foreach (var component in parent.GetComponents<Component>())
			{
				if (component is T)
				{
					action((T)(IEventSystemHandler)component);
				}
			}
			parent = parent.parent;
		}
	}


	/// <summary>
	/// Drag event
	/// </summary>
	public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
	{
		if (m_allowDrag)
		{
			if (m_routeToParent)
			{
				DoForParents<IDragHandler>((parent) => {
					parent.OnDrag(eventData);
				});
			}
			else
			{
				base.OnDrag(eventData);
			}
		}
	}

	/// <summary>
	/// Begin drag event
	/// </summary>
	public override void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
	{
		if (m_allowDrag)
		{
			m_wasInteractable = interactable;

			if (Math.Abs(eventData.delta.x) > Math.Abs(eventData.delta.y))
			{
				m_routeToParent = true;
			}
			else if (Math.Abs(eventData.delta.x) < Math.Abs(eventData.delta.y))
			{
				m_routeToParent = true;
			}
			else
			{
				m_routeToParent = false;
			}

			if (m_routeToParent)
			{
				DoForParents<IBeginDragHandler>((parent) => {
					parent.OnBeginDrag(eventData);
				});
				DeactivateInputField();
				interactable = false;
			}
			else
			{
				base.OnBeginDrag(eventData);
			}
		}
	}

	/// <summary>
	/// End drag event
	/// </summary>
	public override void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
	{
		if (m_allowDrag)
		{
			if (m_routeToParent)
			{
				DoForParents<IEndDragHandler>((parent) => {
					parent.OnEndDrag(eventData);
				});
			}
			else
			{
				base.OnEndDrag(eventData);
			}

			interactable = m_wasInteractable;
		}
		m_routeToParent = false;
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		//Nothing
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
	}
}
