using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Core;
using Utils;
using AssetBundles;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UI
{
	/// <summary>
	/// UIElement.
	/// All UI elements (widgets, screens, tableviews, cells) controllers have to inherit from it
	/// </summary>

	public abstract class UIElement : BaseBehaviour
	{
		protected UIAnimator m_uiAnimator;
		private bool m_buttonsAlreadyHaveListener = false;

		private const string EVENT_SET_BUTTON_TAG = "EventSet";

		#if UNITY_EDITOR
		private bool m_populated = false;
		private List<UIElement> m_populationList = new List<UIElement> ();
		private const string CANVAS_TAG = "Canvas";
		private static Transform s_canvas;
		private int m_level = 0;
		#endif

		protected bool isRoot = true;

		protected override void Awake()
		{
			base.Awake();
			m_uiAnimator = GetComponent<UIAnimator>();

			UnrootChilds();
		}

		protected virtual void Start()
		{
			/* ||| Readme |||
			 * RegisterButtonsEvent(), register analytics and qust events over a button, the UIElement that do the registration
			 * will determine the final even name
			 * UIElements can be childs of another UIElements, so the registration order it is important to get the correct final name.
			 * The correct order to register was registering on Awake, but there was race condition bug so it needed to be moved to Start.
			 * The order Start happen between UIElement childs doesn't guarantee a good naming so some extra loginc needed to be added:
			 * - Register buttons events now it is a recursive function that will try to register first on child UIElements. UIElements deeper in the tree have preference to register its child buttons.
			 * - This was not totally correct, because only Prefabs should register buttons. This is solved with UnrootChilds(), it avoid deeper UIElements that are not prefabs register button events.
			 * - Since Start can be called from any UIElement component, m_buttonsAlreadyHaveListener avoid buttons to be registered twice.
				Sincerely yours: Manuel.
			*/

			#if QUEST_SYSTEM_ACTIVE
			Quests.QuestSystem.Instance.SendEvent(new Quests.EventUIElementStarted (name));
			#endif

			RegisterButtonsEvent();
		}

		public void UnrootChilds()
		{
			UIElement[] uiElements = GetComponentsInChildren<UIElement>(true);
			for (int i = 0; i < uiElements.Length; ++i)
			{
				UIElement uiElement = uiElements[i];

				if (uiElement.gameObject.GetInstanceID() != this.gameObject.GetInstanceID())
				{
					uiElement.isRoot = false;
				}
			}
		}

		public void RegisterButtonsEvent()
		{
			//Add analytics to all buttons
			if (!m_buttonsAlreadyHaveListener)
			{
				UIElement[] uiElements = GetComponentsInChildren<UIElement>(true);
				for (int i = 0; i < uiElements.Length; ++i)
				{
					UIElement uiElement = uiElements[i];

					if (uiElement.gameObject.GetInstanceID() != this.gameObject.GetInstanceID()
					    && uiElement.isRoot
					    && uiElement.GetType() != typeof(ArtistHelper))
					{
						uiElement.RegisterButtonsEvent();
					}
				}

				Button[] buttons = GetComponentsInChildren<Button>(true);
				for (int i = 0; i < buttons.Length; ++i)
				{
					RegisterButtonEvent(buttons[i]);
				}

				m_buttonsAlreadyHaveListener = true;
			}
		}

		private void RegisterButtonEvent(Button button)
		{
			if (button.tag != EVENT_SET_BUTTON_TAG)
			{
				Screen screen = ScreenManager.Instance.GetCurrentScreen();
				if (screen != null)
				{
					string screenName = screen.name;
					button.onClick.AddListener(() => {
						Utils.Debugger.Log("Sent button event \"" + transform.name + "/" + button.name + "\"", (int)SharedSystems.Systems.QUEST);

						#if QUEST_SYSTEM_ACTIVE
						Quests.QuestSystem.Instance.SendEvent(new Quests.EventButtonPressed (transform.name, button.name));
						#endif
					
						string analyticButtonName = button.name; 
						if (transform.name != button.name)
						{
							analyticButtonName = transform.name + "_" + analyticButtonName;
						}
						if (screenName != transform.name)
						{
							analyticButtonName = screenName + "_" + analyticButtonName;
						}

						analyticButtonName = analyticButtonName.Replace("(", "").Replace(")", "");
						Analytics.Dispatcher.SendButtonEvent(analyticButtonName);
					});	

					button.tag = EVENT_SET_BUTTON_TAG;
				}
			}
		}

		public static GameObject LoadUIElementStatic(string prefabName, Vector3 position, Transform parent)
		{
			GameObject itemPrefab = (GameObject)AssetBundleManager.LoadAsset("prefabs", prefabName);

			if (itemPrefab != null)
			{
				return LoadUIElementStatic(itemPrefab, position, parent);
			}
			return null;
		}

		public static GameObject LoadUIElementStatic(GameObject itemPrefab, Vector3 position, Transform parent)
		{
			GameObject obj = null;

			obj = Instantiate(itemPrefab, position, Quaternion.identity) as GameObject;
			obj.name = itemPrefab.name;
			obj.transform.SetParent(parent, false);	

			return obj;
		}

		protected GameObject LoadUIElement(string prefabName, Vector3 position, Transform parent = null)
		{
			GameObject itemPrefab = (GameObject)AssetBundleManager.LoadAsset("prefabs", prefabName);

			if (itemPrefab == null)
			{
//				Debugger.Log("Widget Prefab not found: " + prefabName, Debugger.Severity.ERROR, (int)WaffleSystems.Systems.GENERIC);
				return null;
			}
			return LoadUIElement(itemPrefab, position, parent);
		}
		protected GameObject LoadUIElement(GameObject itemPrefab, Vector3 position, Transform parent = null)
		{
			GameObject obj = null;

			#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
			{

				obj = PrefabUtility.InstantiatePrefab(itemPrefab) as GameObject;
				obj.transform.position = position;
				obj.name = itemPrefab.name;

				UIElement uiElement = obj.GetComponent<UIElement>();
				if (uiElement != null)
				{
					uiElement.RePopulate(m_level + 1);
					m_populationList.Add(uiElement);
				}
				else
				{
					Debug.LogError("The element " + obj.name + " doesn't derive from UIElement"); 
					Debug.Log("Tell this to your favourite programmer: The element " + obj.name + " doesn't derive from UIElement"); 
					UnPopulate();
					GameObject.DestroyImmediate(obj);
					return null;
				}

				obj.transform.name = "L_" + m_level + "_" + obj.transform.name;
			}
			else
			#endif
			{
				obj = Instantiate(itemPrefab, position, Quaternion.identity) as GameObject;
				obj.name = itemPrefab.name;
			}

			if (parent == null)
			{
				parent = transform;
			}
			obj.transform.SetParent(parent, false);	

			return obj;
		}

		public abstract void PopulateUIElements();

		#if UNITY_EDITOR
		public void UnPopulate()
		{
			m_populated = false;
			for (int i = m_populationList.Count - 1; i >= 0; i--)
			{
				UI.UIElement uiElement = m_populationList[i];

				m_populationList.Remove(uiElement);
				uiElement.UnPopulate();
				GameObject.DestroyImmediate(uiElement.gameObject);
			}
		}

		public void ApplyPrefab()
		{
			bool itWasPopulated = false;
			for (int i = 0; i < m_populationList.Count; ++i)
			{
				m_populationList[i].ApplyPrefab();
				itWasPopulated = true;
			}

			UnPopulate();
			if (PrefabUtility.GetPrefabParent(gameObject) != null)
			{
				PrefabUtility.ReplacePrefab(gameObject, PrefabUtility.GetPrefabParent(gameObject), ReplacePrefabOptions.ConnectToPrefab);
			}
			if (itWasPopulated)
			{
				RePopulate();
			}
		}

		public void RePopulate(int level = 0)
		{
			m_level = level;
			if (!m_populated)
			{
				PopulateUIElements();
				m_populated = true;
			}
		}
			
		public void Refresh()
		{
			UnPopulate();
			PopulateUIElements();
		}
			

		//KEEP THIS CODE HERE, MIGHT BE USEFUL IN FUTURE
		/*
		void Awake()
		{
			//base.Awake();

			if (!UnityEditor.EditorApplication.isPlaying)
			{
				if (s_canvas == null)
				{
					s_canvas = GameObject.FindGameObjectWithTag(CANVAS_TAG).transform;
				}
				if (s_canvas != null)
				{
					if (s_canvas.childCount == 1)
					{
						BootStrap.MicroBootStrap();
					}
				}
			}
		}
			
		void OnDestroy()
		{
			if (!UnityEditor.EditorApplication.isPlaying)
			{
				UnPopulate();
			
				if (s_canvas != null)
				{
					if (s_canvas.childCount == 1)
					{
						BootStrap.MicroBootStrapClean();
					}
				}
			}

		}*/
		#endif
	}
}
