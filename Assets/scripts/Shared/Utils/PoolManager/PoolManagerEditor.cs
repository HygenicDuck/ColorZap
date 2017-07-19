#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.IO;

[CustomEditor(typeof(PoolManager))]
public class PoolManagerEditor : Editor
{

	public List<CLPoolEditorElement> m_poolList;
	public string m_warningText = null;
	private CLPoolEditorElement m_newPoolElement = new CLPoolEditorElement ();

	[MenuItem("PoolManager/CreatePoolManager", false, 0)]
	static void CreatePoolManager()
	{
		GameObject go = GameObject.Find("PoolManager");
		if (go == null)
		{
			go = new GameObject ();
			go.name = "PoolManager";
			go.AddComponent<PoolManager>();
		}
		else
		{
			Debug.LogError("It is possible only one instance of CLPoolManager");
		}
	}

	[MenuItem("PoolManager/Help", false, 0)]
	static void Info()
	{
		EditorWindow window = EditorWindow.GetWindow(typeof(InfoWindow));
		window.minSize = new Vector2 (800, 600);
	}

	public override void OnInspectorGUI()
	{
		PoolManager poolTarget = (PoolManager)target;
		m_poolList = poolTarget.m_editorPoolList;


		DrawGenerator();
		EditorGUILayout.Space();
		DrawPoolElements();
	}

	public void DrawPoolElements()
	{
		EditorGUILayout.LabelField("Pools", EditorStyles.toolbarButton);
		int size = m_poolList.Count;
		for (int i = 0; i < size; ++i)
		{
			CLPoolEditorElement element = m_poolList[i];

			EditorGUILayout.BeginVertical("Button");
			EditorGUILayout.LabelField(element.Perfab.GetPrefabName() + " pool", EditorStyles.boldLabel);
			element.DynamicSize = EditorGUILayout.Toggle("Allow pool to grow", element.DynamicSize);
			element.InitialNumberElements = EditorGUILayout.IntField("Initial size", element.InitialNumberElements);
			EditorGUILayout.LabelField("Return type (component):", element.Perfab.GetScriptName());
	
			if (GUILayout.Button("Remove", GUILayout.Width(100)))
			{
				m_poolList.Remove(element);
				GenerateIdsSrc();
				return;
			}
			EditorGUILayout.EndVertical();

		}
		EditorGUILayout.HelpBox("To know how to acces the pool elements in code visit the help section in the Tool bar", MessageType.Info);
	}

	public void DrawGenerator()
	{
		bool hasElement = (m_newPoolElement.Perfab != null);
		bool isAlreadyInPool = AlreadyInPool(m_newPoolElement);

		EditorGUILayout.BeginVertical("Button");
		EditorGUILayout.LabelField("Pool creator box!");
		m_newPoolElement.Perfab = (MonoBehaviour)EditorGUILayout.ObjectField(m_newPoolElement.Perfab, typeof(MonoBehaviour), false);


		if (hasElement)
		{
			if (isAlreadyInPool)
			{
				EditorGUILayout.HelpBox("Already in pool", MessageType.Error);
			}
			else
			{
				EditorGUILayout.LabelField("Return type (component):", m_newPoolElement.Perfab.GetScriptName());
				EditorGUILayout.LabelField("Pool name :", m_newPoolElement.Perfab.GetPrefabName());

				EditorGUILayout.HelpBox("The pool name is the same as the name of the prefab to pool", MessageType.Warning);
			}
		}
		else
		{
			EditorGUILayout.HelpBox("Drag here a Prefab from proyect window", MessageType.Info);
		}


		EditorGUILayout.BeginHorizontal();
		GUI.enabled = (hasElement && !isAlreadyInPool);
		if (GUILayout.Button("Add pool", GUILayout.Width(100)))
		{
			m_poolList.Add(m_newPoolElement);
			GenerateIdsSrc();
			m_newPoolElement = new CLPoolEditorElement ();
		}
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();
	}


	private void GenerateIdsSrc()
	{
		const string srcStart = 
			@"using UnityEngine;
using System.Collections;
		                                
public class PoolId 
{
	public enum IdEnum
	{
";

		const string srcEnd = @"
	};
}";

		StringBuilder content = new StringBuilder ();

		content.Append(srcStart);
		int size = m_poolList.Count;
		for (int i = 0; i < size; ++i)
		{
			CLPoolEditorElement element = m_poolList[i];
			if (i == 0)
			{
				content.Append("\t\t" + element.Perfab.GetPrefabName() + "=0");
			}
			else
			{			
				content.Append(element.Perfab.GetPrefabName());
			}
			if (i != size - 1)
			{
				content.Append(", ");    
			}

		}
		content.Append(srcEnd);

		MonoScript script = MonoScript.FromScriptableObject(this);
		string directory = AssetDatabase.GetAssetPath(script);
		directory = directory.Remove(directory.LastIndexOf("/"));
		directory += "/PoolId.cs";
		System.IO.File.WriteAllText(directory, content.ToString());

	}

	public bool AlreadyInPool(CLPoolEditorElement elem)
	{
		if (elem.Perfab == null)
		{
			return false;
		}
		foreach (CLPoolEditorElement p in m_poolList)
		{
			if (p.Perfab.GetPrefabName() == elem.Perfab.GetPrefabName())
			{
				return true;
			}
		}
		return false;
	}
}


public class InfoWindow : EditorWindow
{

	void OnGUI()
	{
		EditorGUILayout.LabelField("Editor");
		EditorGUILayout.HelpBox("Simply create a pool in PoolManager>CreatePoolManager and follow the instructions from the PoolManager object in the hierarchy window.", MessageType.Info, true);
		EditorGUILayout.LabelField("Scripting");
		EditorGUILayout.HelpBox("-Pools stores references of components (GameObjects' components)." +
		"\n-The name of each pool coincide with the name of the prefab used to create that pool." +
		"\n-Also, an enumerator with all the pools ids is autogeneratedd in PoolId.IdEnum, it allows easy access to your pools." +
		"\n-When you get an element from the pool it is automatically activated." +
		"\n-When you return an element to the pool it is automatically deactivated." +
		"\n\nIn the next examples we assume \n- We have prefab \"Bullet\" in the pool.\n- Bullet has a component BulletBehaviour." +
		"\n\nHow to get an element from a pool." +
		"\n1)By id:" +
		"\n\n\tBulletBehaviour bullet = PoolManager.GetElement<BulletBehaviour>(PoolId.IdEnum.Bullet, newPos, newParentTransform);." +
		"\n\n2)By poolName:" +
		"\n\n\t  BulletBehaviour bullet = PoolManager.GetElement<BulletBehaviour>(\"Bullet\", newPos, newParentTransform);." +
		"\n\n3)Getting a reference of the pool:" +
		"\n\n\tPool bulletsPool = GetPoolById (PoolId.IdEnum.Bullet);\n\tor  Pool bulletsPool = GetPoolByName (\"Bullet\");" +
		"\n\n\nHow to return an element to a pool." +
		"\n1)Directly return to pool from the component pooled:" +
		"\n\n\tthis.ReturnToPool(); (from BulletBehaviour Script)\n\tor bullet.ReturnToPool()" +
		"\n\n2)Call the return to pool method in PoolManager:" +
		"\n\n\tPoolManager.ReturnElement(bullet);" +
		"\n\n3)Call the return to pool method from the pool reference:" +
		"\n\n\tbulletsPool.ReturnElement(bullet);\n", MessageType.Info, true);

		if (GUILayout.Button("got it!", GUILayout.Width(100)))
		{
			this.Close();
		}

	}
}
#endif