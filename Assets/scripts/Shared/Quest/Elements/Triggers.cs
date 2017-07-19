#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;

namespace Quests
{
	/*
	 * TRIGGERS SHARED
	 */
	public class TriggerQuestFinished : QuestTrigger
	{
		private string m_questName;

		public TriggerQuestFinished(string questName)
		{
			m_questName = questName;
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			EventQuestFinished finishedEvent = questEvent as EventQuestFinished;
			if (finishedEvent != null)
			{
				if (m_questName == finishedEvent.QuestName)
				{
					return true;
				}
			}
			return false;
		}
	}

	public class TriggerScreenLoaded : QuestTrigger
	{
		private int m_screenID;

		public TriggerScreenLoaded(int screenID)
		{
			m_screenID = screenID;
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			EventScreenLoaded screenEvent = questEvent as EventScreenLoaded;
			if (screenEvent != null)
			{
				if (typeof(EventScreenLoaded) == screenEvent.GetType())
				{
					return m_screenID == ((EventScreenLoaded)questEvent).ScreenId;
				}
			}
			return false;
		}
	}

	public class TriggerUIElementStarted : QuestTrigger
	{
		private string m_prefabName;

		public TriggerUIElementStarted(string prefabName)
		{
			m_prefabName = prefabName;
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			EventUIElementStarted screenEvent = questEvent as EventUIElementStarted;
			if (screenEvent != null)
			{
				if (typeof(EventUIElementStarted) == screenEvent.GetType())
				{
					return m_prefabName == ((EventUIElementStarted)questEvent).PrefabName;
				}
			}
			return false;
		}
	}

	public class TriggerPressedButton : QuestTrigger
	{
		private string m_UIElementName;
		private string m_buttonName;

		public TriggerPressedButton(string UIElementName, string buttonName)
		{
			m_UIElementName = UIElementName;
			m_buttonName = buttonName;
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			if (typeof(EventButtonPressed) == questEvent.GetType())
			{
				EventButtonPressed buttonEvent = (EventButtonPressed)questEvent;
				return (buttonEvent.ButtonName == m_buttonName && m_UIElementName == buttonEvent.UIElementName);
			}
			return false;
		}
	}

	public class TriggerPopupOpened : QuestTrigger
	{
		private string m_prefabName;

		public TriggerPopupOpened(string prefabName)
		{
			m_prefabName = prefabName;
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			EventPopupOpened popupEvent = questEvent as EventPopupOpened;
			if (popupEvent != null)
			{
				return m_prefabName == ((EventPopupOpened)questEvent).PopupName;
			}
			return false;
		}
	}

}

#endif