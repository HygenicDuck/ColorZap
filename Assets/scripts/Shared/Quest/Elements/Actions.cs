#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;
using UI;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Quests
{
	/*
	 * ACTIONS SHARED
	 */
	public class ActionShowScreen : QuestAction
	{
		private string m_screenName;
		private bool m_drawTransitionBehind;
		private bool m_replace;

		public ActionShowScreen(string screenName, bool drawTransitionBehind = false, bool replace = false)
		{
			m_screenName = screenName;
			m_replace = replace;
			m_drawTransitionBehind = drawTransitionBehind;
		}

		public override void Execute()
		{
			UI.Screen newScreen = ScreenManager.Instance.CreateScreen(m_screenName);
			if (m_replace)
			{
				ScreenManager.Instance.ReplaceAllScreens(newScreen, m_drawTransitionBehind);
			}
			else
			{
				ScreenManager.Instance.PushScreen(newScreen, m_drawTransitionBehind);
			}
		}
	}

	public class ActionPopToRoot : QuestAction
	{
		public ActionPopToRoot()
		{
		}

		public override void Execute()
		{
			ScreenManager.Instance.PopToRoot();
		}
	}

	public class ActionSetRootScreen : QuestAction
	{
		private string m_screenName;
		private bool m_replace;

		public ActionSetRootScreen(string screenName)
		{
			m_screenName = screenName;
		}

		public override void Execute()
		{
			UI.Screen newScreen = ScreenManager.Instance.CreateScreen(m_screenName);
			ScreenManager.Instance.AddScreenAsRoot(newScreen);
		}
	}

	public class ActionUIElementToTop : QuestAction
	{
		private string m_uiElementName;
		private string m_buttonName;
		private Canvas m_buttonCanvas;
		private GraphicRaycaster m_rayCaster;
		private bool m_isPopup;

		public Canvas Canvas { get { return m_buttonCanvas; } }
		public GraphicRaycaster RayCaster { get { return m_rayCaster; } }

		public ActionUIElementToTop(string UIElementName, string buttonName, bool isPopup = false)
		{
			m_uiElementName = UIElementName;
			m_buttonName = buttonName;
			m_isPopup = isPopup;
		}

		public override void Execute()
		{
			Transform uiElement;
			if (m_isPopup)
			{
				Transform popupCanvas = PopupManager.Instance.RootCanvas;
				uiElement = popupCanvas.FindChildByName(m_uiElementName);
			}
			else
			{
				UI.Screen currentScreen = ScreenManager.Instance.GetCurrentScreen();
				uiElement = currentScreen.transform.FindChildByName(m_uiElementName);
			}

			if (uiElement != null)
			{
				Transform button = uiElement.FindChildByName(m_buttonName);
				if (button != null)
				{
					m_buttonCanvas = button.gameObject.AddComponent<Canvas>();
					m_rayCaster = button.gameObject.AddComponent<GraphicRaycaster>();
					m_buttonCanvas.overrideSorting = true;
					m_buttonCanvas.sortingOrder = 2;
				}
			}
		}
	}

	public class ActionOnElementBase : QuestAction
	{
		private string m_uiElementName;
		private string m_childName;
		private bool m_isPopup;
		private bool m_multipleButtons;
		private bool m_includeInactiveInSearch;
		private int m_searchInSpecificScreenID;


		public ActionOnElementBase(string UIElementName, string childName, bool isPopup, bool multipleButtons, bool includeInactiveInSearch = true, int searchInSpecificScreenID = -1)
		{
			m_uiElementName = UIElementName;
			m_childName = childName;
			m_isPopup = isPopup;
			m_multipleButtons = multipleButtons;
			m_includeInactiveInSearch = includeInactiveInSearch;
			m_searchInSpecificScreenID = searchInSpecificScreenID;
		}

		protected virtual void PerformActionOnElement(Transform element)
		{			
		}

		public override void Execute()
		{
			Transform uiElement;
			if (m_isPopup)
			{
				Transform popupCanvas = PopupManager.Instance.RootCanvas;
				uiElement = popupCanvas.FindChildByName(m_uiElementName, m_includeInactiveInSearch);
			}
			else
			{
/*				UI.Screen currentScreen = null;
				if (m_searchInSpecificScreenID > (int)WaffleScreen.ID.None)
				{
					currentScreen = ScreenManager.Instance.GetScreenFromID(m_searchInSpecificScreenID);
				}
				else
				{
					currentScreen = ScreenManager.Instance.GetCurrentScreen();
				}
				uiElement = currentScreen.transform.FindChildByName(m_uiElementName, m_includeInactiveInSearch);*/
			}

/*			if (uiElement != null)
			{
				if (m_multipleButtons)
				{
					List<Transform> children = uiElement.FindAllChildrenByName(m_childName, includeInactive: m_includeInactiveInSearch);
					foreach (Transform child in children)
					{
						if (child != null)
						{
							PerformActionOnElement(child);
						}	
					}
				}
				else
				{
					Transform child = uiElement.FindChildByName(m_childName, includeInactive: m_includeInactiveInSearch);
					if (child != null)
					{
						PerformActionOnElement(child);
					}
				}
			}*/
		}
	}


	public class ActionShowUIElement : ActionOnElementBase
	{
		private bool m_show;

		public ActionShowUIElement(string UIElementName, string childName, bool show = true, bool isPopup = false, bool multipleButtons = false, bool includeInactiveInSearch = true) : base(UIElementName, childName, isPopup, multipleButtons, includeInactiveInSearch)
		{
			m_show = show;
		}

		protected override void PerformActionOnElement(Transform element)
		{			
			if (element != null)
			{
				element.gameObject.SetActive(m_show);
			}
		}
	}

	public class ActionMoveUIElement : ActionOnElementBase
	{
		private Vector2 m_offset;

		public ActionMoveUIElement(string UIElementName, string childName, Vector2 offset, bool isPopup = false, bool multipleButtons = false, bool includeInactiveInSearch = true) : base(UIElementName, childName, isPopup, multipleButtons, includeInactiveInSearch)
		{
			m_offset = offset;
		}

		protected override void PerformActionOnElement(Transform element)
		{			
			if (element != null)
			{
				Vector3 position = element.position;
				position.x += m_offset.x;
				position.y += m_offset.y;
				element.position = position;
			}
		}
	}

	public class ActionEnableButton : ActionOnElementBase
	{
		private bool m_enable;

		public ActionEnableButton(string UIElementName, string childName, bool enable = true, bool isPopup = false, bool multipleButtons = false, int searchInSpecificScreenID = -1) : base(UIElementName, childName, isPopup, multipleButtons, searchInSpecificScreenID: searchInSpecificScreenID)
		{
			m_enable = enable;
		}

		protected override void PerformActionOnElement(Transform element)
		{			
			Button button = element.GetComponent<Button>();
			if (button != null)
			{
				button.interactable = m_enable;
			}
		}
	}

	public class ActionEnableToggle : ActionOnElementBase
	{
		private bool m_enable;

		public ActionEnableToggle(string UIElementName, string childName, bool enable = true, bool isPopup = false, bool multipleButtons = false) : base(UIElementName, childName, isPopup, multipleButtons)
		{
			m_enable = enable;
		}

		protected override void PerformActionOnElement(Transform element)
		{			
			Toggle toggle = element.GetComponent<Toggle>();
			if (toggle != null)
			{
				toggle.interactable = m_enable;
			}
		}
	}

	public class ActionUIElementToTopCancel : QuestAction
	{
		public ActionUIElementToTopCancel()
		{
		}

		public override void Execute()
		{
			ActionUIElementToTop previusAction = m_parentSubquest.GetPreviousElement<ActionUIElementToTop>();
			if (previusAction != null)
			{
				GameObject.Destroy(previusAction.RayCaster);
				GameObject.Destroy(previusAction.Canvas);
			}

		}
	}

	public class ActionDelay : QuestAction
	{
		float m_time;

		public ActionDelay(float timer)
		{
			m_time = timer;
		}

		private static IEnumerator Delay(float delay)
		{
			yield return new WaitForSeconds (delay);
			QuestSystem.Instance.SendEvent(new EventTimerFinished ());
		}

		public override void Execute()
		{
			Utils.CoroutineHelper.Instance.Run(Delay(m_time));
		}
	}

	public class ActionTriggerAnalyticEvent : QuestAction
	{
		string m_eventName;

		public ActionTriggerAnalyticEvent(string eventName)
		{
			m_eventName = eventName;
		}

		public override void Execute()
		{
			Analytics.Dispatcher.SendEvent(m_eventName);
		}
	}

	public class ActionEnableAllButtonsIn : QuestAction
	{
		private string m_uiElementName;
		private bool m_enable;
		private Button[] m_buttons;
		private bool m_isPopup;

		public Button[] Buttons { get { return m_buttons; } }

		public ActionEnableAllButtonsIn(string uiElement, bool enable, bool isPopup = false)
		{
			m_uiElementName = uiElement;
			m_enable = enable;
			m_isPopup = isPopup;
		}

		public override void Execute()
		{
			Transform uiElement;
			if (m_isPopup)
			{
				Transform popupCanvas = PopupManager.Instance.RootCanvas;
				uiElement = popupCanvas.FindChildByName(m_uiElementName);
			}
			else
			{
				UI.Screen currentScreen = ScreenManager.Instance.GetCurrentScreen();
				uiElement = currentScreen.transform.FindChildByName(m_uiElementName);
			}

			if (uiElement != null)
			{
				m_buttons = uiElement.gameObject.GetComponentsInChildren<Button>();

				for (int i = 0; i < m_buttons.Length; ++i)
				{
					m_buttons[i].interactable = m_enable;
				}
			}
		}
	}

	public class ActionEnableAllButtonsInLast : QuestAction
	{
		private bool m_enable;

		public ActionEnableAllButtonsInLast(bool enable)
		{
			m_enable = enable;
		}

		public override void Execute()
		{
			ActionEnableAllButtonsIn previusAction = m_parentSubquest.GetPreviousElement<ActionEnableAllButtonsIn>();
			if (previusAction != null)
			{
				Button[] buttons = previusAction.Buttons;
				for (int i = 0; i < buttons.Length; ++i)
				{
					buttons[i].interactable = m_enable;
				}
			}
		}
	}

	public class ActionProtectFromTouch : QuestAction
	{
		private bool m_protect;

		public ActionProtectFromTouch(bool enable)
		{
			m_protect = enable;
		}

		public override void Execute()
		{
			ScreenManager.Instance.ActivateProtectionScreen(m_protect);
		}
	}


	public class ActionRepeatSubquest : QuestAction
	{
		SubQuest m_subQuest;
		int m_step = 0;

		public ActionRepeatSubquest(SubQuest subQuest = null, int step = 0)
		{
			m_subQuest = subQuest;
			m_step = step;
		}

		public override void Execute()
		{
			if (m_subQuest != null)
			{
				m_subQuest.Reset(m_step);
			}
			else
			{
				m_parentSubquest.Reset(m_step);
			}
		}
	}

	public class ActionSendTutorialEvent : QuestAction
	{

		System.Action m_action;

		public ActionSendTutorialEvent(System.Action action)
		{
			m_action = action;
		}

		public override void Execute()
		{
			if (m_action != null)
			{
				m_action();
			}
		}
	}
}

#endif