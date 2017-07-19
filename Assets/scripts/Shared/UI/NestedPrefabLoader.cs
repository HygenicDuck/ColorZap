using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Core;
using Utils;
using AssetBundles;

[ExecuteInEditMode]
public class NestedPrefabLoader : BaseBehaviour
{
	[System.Serializable] 
	public class PrefabTest
	{
		public string prefabName;
		public GameObject root;
	}

	[SerializeField]
	private List<PrefabTest> m_prefabTest = new List<PrefabTest> ();

	void Start ()
	{
		Debugger.Log("NestedPrefabLoader Start");

		CleanupPrefabs ();
		CreateNestedPrefabs ();
	}

	private void CleanupPrefabs ()
	{
		foreach (PrefabTest testObj in m_prefabTest)
		{
			foreach (Transform child in testObj.root.transform)
			{
				if (Application.isPlaying)
				{
					GameObject.Destroy (child.gameObject);
				}
				else
				{
					GameObject.DestroyImmediate (child.gameObject);
				}

			}
		}
	}

	private void CreateNestedPrefabs ()
	{		
		foreach (PrefabTest testObj in m_prefabTest)
		{
			GameObject itemPrefab = (GameObject)AssetBundleManager.LoadAsset("prefabs", testObj.prefabName);

			GameObject obj = itemPrefab != null ? Instantiate (itemPrefab, Vector3.zero, Quaternion.identity) as GameObject : null;
			obj.transform.SetParent (testObj.root.transform, false);			
		}
	}
}
