using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Mono singleton Class. Extend this class to make singleton component.
/// Example: 
/// <code>
/// public class Foo : MonoSingleton<Foo>
/// </code>. To get the instance of Foo class, use <code>Foo.instance</code>
/// Override <code>Init()</code> method instead of using <code>Awake()</code>
/// from this class.
/// </summary>
public abstract class MonoSingleton<T> : Core.BaseBehaviour where T : MonoSingleton<T>
{
	private static T m_Instance = null;
	public static T Instance
	{
		get
		{
			// Instance requiered for the first time, we look for it
			if (m_Instance == null)
			{
				m_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

				// Object not found, we create a temporary one
				if (m_Instance == null)
				{
					isTemporaryInstance = true;
					m_Instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();

					// Problem during the creation, this should not happen
					if (m_Instance == null)
					{
						Debug.LogError("Problem during the creation of " + typeof(T).ToString());
					}
				}
				if (!_isInitialized)
				{
					_isInitialized = true;
					m_Instance.Init();
				}
			}
			return m_Instance;
		}
	}

	public static D GetTypedInstance<D>() where D : T
	{
		if (m_Instance == null)
		{
			m_Instance = GameObject.FindObjectOfType(typeof(D)) as D;

			// Object not found, we create a temporary one
			if (m_Instance == null)
			{
				isTemporaryInstance = true;
				m_Instance = new GameObject(typeof(D).ToString(), typeof(D)).GetComponent<D>();

				// Problem during the creation, this should not happen
				if (m_Instance == null)
				{
					Debug.LogError("Problem during the creation of " + typeof(D).ToString());
				}
			}
			if (!_isInitialized)
			{
				_isInitialized = true;
				m_Instance.Init();
			}
		}
		return m_Instance as D;
	} 

	public static bool isTemporaryInstance { private set; get; }

	private static bool _isInitialized;

	// If no other monobehaviour request the instance in an awake function
	// executing before this one, no need to search the object.
	protected override void Awake()
	{
		base.Awake();

		if (m_Instance == null)
		{
			m_Instance = this as T;
		}
		else if (m_Instance != this)
		{
			Debug.LogError("Another instance of " + GetType() + " is already exist! Destroying self...");
			DestroyImmediate(this);
			return;
		}
		if (!_isInitialized)
		{
			DontDestroyOnLoad(gameObject);
			_isInitialized = true;
			m_Instance.Init();
		}
	}

	protected virtual bool PerisistBetweenScenes
	{
		get
		{
			return false;
		}
	}

	/// <summary>
	/// This function is called when the instance is used the first time
	/// Put all the initializations you need here, as you would do in Awake
	/// </summary>
	protected abstract void Init();

	public void Touch() 
	{
		//Dick all
	}
}
