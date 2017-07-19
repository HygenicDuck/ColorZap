using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool
{
	private Queue<Component> m_elements;
	private Component m_prefab;
	private bool m_dynamicSize;
	private Transform m_defaultParent;

	public Pool(bool dynamicSize, Component prefab, int initialSize, Transform defaultParent)
	{
		m_elements = new Queue<Component> ();
		m_dynamicSize = dynamicSize;
		m_prefab = prefab;
		m_defaultParent = defaultParent;

		for (int j = 0; j < initialSize; ++j)
		{
			Component obj = Component.Instantiate(m_prefab) as Component;
			obj.transform.name = m_prefab.transform.name;

			obj.transform.SetParent(m_defaultParent.transform, false);
			obj.gameObject.SetActive(false);
			m_elements.Enqueue(obj);
		}
	}

	public T GetElement<T>() where T : Component
	{
		return GetElement<T>(m_defaultParent);
	}
		
	public T GetElement<T>(Transform newParent, bool worldPositionStays = true) where T : Component
	{
		T element = null;

		if (newParent == null)
		{
			newParent = m_defaultParent;
		}

		if (m_elements.Count > 0)
		{
			element = m_elements.Dequeue() as T;
		}
		else
		{
			if (m_dynamicSize)
			{
				element = Component.Instantiate(m_prefab) as T;
				element.name = m_prefab.name;
				element.transform.SetParent(m_defaultParent, worldPositionStays);
			}
		}
		if (element != null)
		{
			element.transform.SetParent(newParent, worldPositionStays);
			element.gameObject.SetActive(true);
		}
		return element;
	}

	public void ReturnElement(Component component)
	{
		component.transform.SetParent(m_defaultParent, true);
		m_elements.Enqueue(component);
		component.gameObject.SetActive(false);
	}
}