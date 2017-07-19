#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;

namespace Quests
{
	/*
	 * EVENTS SHARED
	 */

	public class EventTimerFinished : QuestEvent
	{
		public EventTimerFinished()
		{
		}
	}

	public class EventZeroChainsInYourTurn : QuestEvent
	{
		public EventZeroChainsInYourTurn()
		{
		}
	}

	public class EventPopupClosed : QuestEvent
	{
		public EventPopupClosed()
		{
		}
	}


	public class EventBattleBoardYourTurnToChoose : QuestEvent
	{
		public EventBattleBoardYourTurnToChoose()
		{
		}
	}

	public class EventQuestFinished : QuestEvent
	{
		private string m_questName;

		public string QuestName {get { return m_questName; } }

		public EventQuestFinished(string questName)
		{
			m_questName = questName;
		}
	}

	public class EventScreenLoaded : QuestEvent
	{
		private int m_screenId;

		public int ScreenId {get { return m_screenId; } }

		public EventScreenLoaded(int screenId)
		{
			m_screenId = screenId;
		}
	}

	public class EventUIElementStarted : QuestEvent
	{
		private string m_prefabName;

		public string PrefabName {get { return m_prefabName; } }

		public EventUIElementStarted(string prefabName)
		{
			m_prefabName = prefabName;
		}
	}

	public class EventButtonPressed : QuestEvent
	{
		private string m_UIElementName;
		private string m_buttonName;

		public string UIElementName {get { return m_UIElementName; } }
		public string ButtonName {get { return m_buttonName; } }

		public EventButtonPressed(string uiElementName, string buttonName)
		{
			m_UIElementName = uiElementName;
			m_buttonName = buttonName;
		}
	}

	public class EventPopupOpened : QuestEvent
	{
		private string m_popupName;

		public string PopupName {get { return m_popupName; } }

		public EventPopupOpened(string popupName)
		{
			m_popupName = popupName;
		}
	}

}

#endif