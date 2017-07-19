using UnityEngine;
using System.Collections;
using System;

[System.Serializable]
public class GameObjectWithChecks
{
	[SerializeField] public GameObject m_gameObject;
	private readonly Type m_component;
	private readonly bool m_requiresGameObject = true;

	public GameObjectWithChecks(Type component, bool requiresGameObject)
	{
		m_component = component;
		m_requiresGameObject = requiresGameObject;
	}

	public GameObjectWithChecks(Type component) : this(component, true) 
	{
	}

	public bool isValid
	{
		get
		{
			if (m_gameObject == null)
			{ 
				return !m_requiresGameObject;
			}
			else
			{
				return m_gameObject.GetComponent(m_component) != null;
			}
		}
	}

	public string ComponentName
	{
		get
		{
			return m_component.Name;
		}
	}

	public T GetComponent<T>()
	{
		return m_gameObject.GetComponent<T>();
	}
}