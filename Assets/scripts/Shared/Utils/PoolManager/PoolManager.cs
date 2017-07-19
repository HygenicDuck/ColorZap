using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utils;

[System.Serializable]
public class CLPoolEditorElement
{
	public MonoBehaviour Perfab;
	public int InitialNumberElements;
	public bool DynamicSize = true;
}


public class PoolManager : MonoBehaviour
{

	private static PoolManager m_instance;
	public List<CLPoolEditorElement> m_editorPoolList = new List<CLPoolEditorElement> ();
	public Dictionary<string, Pool> m_pools = new Dictionary<string, Pool> ();
	public List<string> m_idNames = new List<string> ();

	public void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		else
		{
			Destroy(this.gameObject);
			Debugger.Log("It is possible only one instance of CLPoolManager", Debugger.Severity.ERROR);
			return;
		}

		m_idNames.Clear();
		Debugger.Log("Pools number " + m_editorPoolList.Count);
		for (int i = 0; i < m_editorPoolList.Count; i++)
		{
			MonoBehaviour mono = m_editorPoolList[i].Perfab;
			int nElements = m_editorPoolList[i].InitialNumberElements;
			bool isDynamic = m_editorPoolList[i].DynamicSize;
			string name = mono.GetPrefabName();

			//Create subfolder
			GameObject subfolder = new GameObject ();
			subfolder.name = name + "_PoolContainer";
			subfolder.transform.SetParent(this.transform);

			//Create the new pool
			m_idNames.Add(name);
			m_pools.Add(name, new Pool (isDynamic, mono, nElements, subfolder.transform));
			Debugger.Log("Added pool" + name);
		}
	}

	private Pool GetPool(string name)
	{
		try
		{
			Pool pool = m_instance.m_pools[name];

			return pool;
		}
		catch (KeyNotFoundException)
		{
			Debugger.Log("WARNING: Accessing nonexistent pool " + name + " (add a pool for this element in the Editor)", Debugger.Severity.ERROR);
			return null;
		}
	}

	public static Pool GetPoolById(PoolId.IdEnum id)
	{
		return m_instance.GetPool(m_instance.m_idNames[(int)id]);
	}

	public static Pool GetPoolByName(string name)
	{
		return m_instance.GetPool(name);
	}

	public static T GetElement<T>(string poolName, Transform newParent = null) where T : MonoBehaviour
	{
		Pool pool = m_instance.GetPool(poolName);
		T element = pool.GetElement<T>(newParent);
		return element;
	}
	public static T GetElement<T>(PoolId.IdEnum id, Transform newParent = null) where T : MonoBehaviour
	{
		return GetElement<T>(m_instance.m_idNames[(int)id], newParent);
	}

	public static void ReturnElement(MonoBehaviour mono)
	{
		Pool pool = m_instance.GetPool(mono.GetPrefabName());
		pool.ReturnElement(mono);
	}
}

public static class PoolExtensions
{
	public static string GetScriptName(this MonoBehaviour mono)
	{
		return mono.GetType().Name;
	}

	public static string GetPoolName(this MonoBehaviour mono)
	{
		return mono.transform.name;
	}

	public static string GetPrefabName(this MonoBehaviour mono)
	{
		return mono.transform.name;
	}

	public static void ReturnToPool(this MonoBehaviour mono)
	{
		PoolManager.ReturnElement(mono);
	}

}