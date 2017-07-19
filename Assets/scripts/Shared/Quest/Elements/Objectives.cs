#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;

namespace Quests
{
	/*
	 * OBJECTIVES SHARED
	 */


	public class ObjectiveTimerFinished : QuestObjective
	{
		public ObjectiveTimerFinished()
		{
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			if (typeof(EventTimerFinished) == questEvent.GetType())
			{
				return true;
			}
			return false;
		}
	}

	public class ObjectiveZeroChainsInYourTurn : QuestObjective
	{
		public ObjectiveZeroChainsInYourTurn()
		{
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			if (typeof(EventZeroChainsInYourTurn) == questEvent.GetType())
			{
				return true;
			}
			return false;
		}
	}

	public class ObjectivePopupClosed : QuestObjective
	{
		public ObjectivePopupClosed()
		{
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			if (typeof(EventPopupClosed) == questEvent.GetType())
			{
				return true;
			}
			return false;
		}
	}

	public class ObjectiveBattleBoardYourTurnToChoose : QuestObjective
	{
		public ObjectiveBattleBoardYourTurnToChoose()
		{
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			if (typeof(EventBattleBoardYourTurnToChoose) == questEvent.GetType())
			{
				return true;
			}
			return false;
		}
	}

	public class ObjectiveScreenLoaded : QuestObjective
	{
		private int m_screenID;

		public int ScreenId { get { return m_screenID; } }

		public ObjectiveScreenLoaded(int screenID)
		{
			m_screenID = screenID;
		}

		public override bool CheckEvent(QuestEvent questEvent)
		{
			if (typeof(EventScreenLoaded) == questEvent.GetType())
			{
				return m_screenID == ((EventScreenLoaded) questEvent).ScreenId;
			}
			return false;
		}
	}

	public class ObjectiveUIElementStarted : QuestObjective
	{
		private string m_prefabName;

		public ObjectiveUIElementStarted(string prefabName)
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

	public class ObjectivePressedButton : QuestObjective
	{
		private string m_UIElementName;
		private string m_buttonName;

		public ObjectivePressedButton(string UIElementName, string buttonName)
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


}

#endif