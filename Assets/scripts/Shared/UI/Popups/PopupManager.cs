using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Kanga;
using Utils;
using Core;
using AssetBundles;
#if QUEST_SYSTEM_ACTIVE
using Quests;
#endif

namespace UI
{
	public class PopupSetupDataBase
	{
		public string prefabName;
		public PopupManager.PopupType popupType;
		public int origin;
		public bool showCloseButton = true;
		public bool showBackground = true;
		public bool backgroundClickDismisses = false;
		public bool checkDevicePerformance = true;
		public Action<int> buttonCallback;
		public Action containerClosed;
		public float containerHeight = -1f;
	}

	public class PopupManager : MonoSingleton<PopupManager>, PopupContainerBaseWidget.IDelegate
	{
		public enum PopupType
		{
			USER,
			SYSTEM,
			DEBUG
		}

		public class TriggerSetting
		{
			public PopupSetupDataBase data;
			public int priority;
			public bool ready;
			public bool wantsToShow;

			public TriggerSetting()
			{
				priority = -1;
				ready = false;
				wantsToShow = false;
			}
		}

		public static readonly int DebugContinue = 0;
		public static readonly int DebugIgnore = 1;

		[SerializeField] private Transform m_root;
		[SerializeField] private Transform m_debugRoot;
		[SerializeField] private GameObject m_prefabPopupContainerWidget;

		private TriggerSetting[] m_triggerSettings;
		private string m_activeDebugStack;
		private HashSet<string> m_ignoreDebug = new HashSet<string> ();
		private bool m_hasActivePopup = false;
		private PopupBase m_debugPopupBase = null;
		private PopupContainerBaseWidget m_popupContainerWidget;
		// Shared c# script and derive on waffle to handle intro and outro anims
		private PopupSetupDataBase m_popupWaiting;
		private Action<PopupBase> m_popupWaitingCallback;

		public Transform RootCanvas { get { return m_root; } }
		public Transform DebugRootCanvas { get { return m_debugRoot; } }
		public PopupContainerBaseWidget ContainerWidget { get { return m_popupContainerWidget; } }

		public void SetTriggerSettings(TriggerSetting[] triggerSettings)
		{
			m_triggerSettings = triggerSettings;
		}

		protected override void Init()
		{
			if (m_root == null)
			{
				Debugger.Warning("POPUP MANAGER No root assigned", (int)SharedSystems.Systems.POPUP_MANAGER);
			}
			if (m_debugRoot == null)
			{
				Debugger.Warning("POPUP MANAGER No debug root assigned", (int)SharedSystems.Systems.POPUP_MANAGER);
			}
			if (m_prefabPopupContainerWidget == null)
			{
				Debugger.Warning("POPUP MANAGER No prefab container widget assigned", (int)SharedSystems.Systems.POPUP_MANAGER);
			}

			GameObject containerObject = UI.UIElement.LoadUIElementStatic(m_prefabPopupContainerWidget, Vector3.zero, m_root);
			m_popupContainerWidget = containerObject.GetComponent<PopupContainerBaseWidget>();
			m_popupContainerWidget.Initialise(this);
		}

		public bool IsPopupOnScreen()
		{
			return m_hasActivePopup;
		}

		public PopupBase ShowPopup(PopupSetupDataBase data)
		{	
			TryForceCloseDebugPopup();

			PopupBase popup = null;
			if (!m_hasActivePopup)
			{
				switch (data.popupType)
				{
				case PopupType.USER:
					#if QUEST_SYSTEM_ACTIVE

					Quests.QuestSystem.Instance.SendEvent(new Quests.EventPopupOpened (data.prefabName));
					#endif
					popup = HandleUserPopup(data);
					break;
				case PopupType.SYSTEM:
					HandleSystemPopup(data);
					break;
				case PopupType.DEBUG:
					HandleDebugPopup(data);
					break;	
				}
			}
			return popup;
		}

		public PopupBase ShowPopupOrWait(PopupSetupDataBase data, Action<PopupBase> showedPopupCallback = null)
		{
			TryForceCloseDebugPopup();

			PopupBase popup = null;

			if (!m_hasActivePopup)
			{
				popup = ShowPopup(data);
			}
			else
			{
				m_popupWaiting = data;
				m_popupWaitingCallback = showedPopupCallback;
			}

			return popup;
		}

		private void TryForceCloseDebugPopup()
		{
			if (m_hasActivePopup && m_debugPopupBase != null)
			{
				GameObject.Destroy(m_debugPopupBase.gameObject);
				m_debugPopupBase = null;
				m_hasActivePopup = false;

				#if QUEST_SYSTEM_ACTIVE
				QuestSystem.Instance.SendEvent(new EventPopupClosed ());
				#endif
			}
		}

		public void CancelWaitingPopup(Type type)
		{
			if (m_popupWaiting.GetType() == type)
			{
				m_popupWaiting = null;
				m_popupWaitingCallback = null;
			}
		}

		public void SystemReady(int system)
		{
			m_triggerSettings[system].ready = true;

			CheckForSystemPopups();
		}

		private void CheckForSystemPopups()
		{
			TriggerSetting highestPrioritySystem = new TriggerSetting ();
		
			// find the highest priority system that wants to show a popup
			for (int i = 0; i < m_triggerSettings.GetLength(0); ++i)
			{
				TriggerSetting system = m_triggerSettings[i];

				if (system.wantsToShow && system.priority > highestPrioritySystem.priority)
				{
					highestPrioritySystem = system;
				}
			}

			// now check if any system aren't ready and then check if their priority is higher than the one that wants to show
			for (int i = 0; i < m_triggerSettings.GetLength(0); ++i)
			{
				TriggerSetting system = m_triggerSettings[i];

				if (!system.ready && system.priority > highestPrioritySystem.priority)
				{
					// we are waiting on a system that has a higher priority than that which wants to show, so it'll have to wait
					return;
				}
			}

			if (highestPrioritySystem.wantsToShow)
			{
				ShowPopupInternal(highestPrioritySystem.data, m_popupContainerWidget);

				ResetSystems();
			}
		}

		private void ResetSystems()
		{
			for (int i = 0; i < m_triggerSettings.GetLength(0); ++i)
			{
				TriggerSetting system = m_triggerSettings[i];

				system.wantsToShow = false;
				system.ready = false;
			}
		}

		private PopupBase HandleUserPopup(PopupSetupDataBase data)
		{
			//	Always show
			return ShowPopupInternal(data, m_popupContainerWidget);
		}

		private void HandleSystemPopup(PopupSetupDataBase data)
		{
			m_triggerSettings[data.origin].wantsToShow = true;
			m_triggerSettings[data.origin].ready = true;
			m_triggerSettings[data.origin].data = data;

			CheckForSystemPopups();
		}

		private void HandleDebugPopup(PopupSetupDataBase data)
		{
			string stackTrace = Environment.StackTrace;

			if (!m_ignoreDebug.Contains(stackTrace))
			{
				ShowDebugPopupInternal(data, m_debugRoot);
				m_activeDebugStack = stackTrace;
			}
		}

		private PopupBase ShowPopupInternal(PopupSetupDataBase data, PopupContainerBaseWidget containerBase)
		{
			PopupBase popupComponent = containerBase.CreateContent(data);
			if (popupComponent != null)
			{
				m_hasActivePopup = true;
			}
			return popupComponent;
		}

		private void ShowDebugPopupInternal(PopupSetupDataBase data, Transform rootTransform)
		{
			GameObject itemPrefab = AssetBundleManager.LoadAsset("prefabs", data.prefabName) as GameObject;
			if (itemPrefab != null)
			{
				GameObject obj = UI.UIElement.LoadUIElementStatic(itemPrefab, Vector3.zero, rootTransform);
				m_debugPopupBase = obj.GetComponent<PopupBase>();

				if (m_debugPopupBase != null)
				{
					m_debugPopupBase.Init(data, PopupClosed);
					m_hasActivePopup = true;
				}
			}
		}

		public void PopupClosed(PopupBase activePopup, int buttonIndex)
		{
			if (activePopup != null)
			{		
				CheckDebugButtonOnClose(activePopup, buttonIndex);

				Destroy(activePopup.gameObject);
				m_hasActivePopup = false;

				#if QUEST_SYSTEM_ACTIVE
				QuestSystem.Instance.SendEvent(new EventPopupClosed ());
				#endif
			}
		}

		private void CheckDebugButtonOnClose(PopupBase activePopup, int buttonIndex)
		{
			if (activePopup.PopupType == PopupType.DEBUG)
			{
				if (buttonIndex == DebugIgnore)
				{
					m_ignoreDebug.Add(m_activeDebugStack);
				}
				ShowNextPopup();
			}
		}

		private void ShowNextPopup()
		{
			m_hasActivePopup = false;

			if (m_popupWaiting != null)
			{
				PopupBase popup = ShowPopup(m_popupWaiting);
				if (m_popupWaitingCallback != null)
				{
					m_popupWaitingCallback(popup);
				}
				m_popupWaiting = null;
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//
		// PopupContainerBaseWidget.IDelegate
		//
		//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		public void FinishedAnimatingClosed()
		{
			ShowNextPopup();
		}
	}
}
