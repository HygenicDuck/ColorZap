#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;
using UI;
using UnityEngine.UI;

namespace Quests
{
	/*
	 * CONDITIONS SHARED
	 */

	public class ConditionCurrentScreen : QuestCondition
	{
		private int m_screenID;

		public ConditionCurrentScreen(int screenID)
		{
			m_screenID = screenID;
		}

		public override bool Condition()
		{
			int currentScreenID = ScreenManager.Instance.GetCurrentScreen().ScreenID.Get;
			return (m_screenID == currentScreenID);
		}
	}

	public class ConditionTutorialCompleted : QuestCondition
	{
		private string m_tutorial;
		private bool m_completed;

		public ConditionTutorialCompleted(string tutorial, bool completed)
		{
			m_tutorial = tutorial;
			m_completed = completed;
		}

		public override bool Condition()
		{
			return (QuestSystem.Instance.IsQuestCompleted(m_tutorial) == m_completed);
		}
	}

	public class ConditionCurrentScreenNot : QuestCondition
	{
		private int[] m_screenIDs;

		public ConditionCurrentScreenNot(params int[] screenIds)
		{
			m_screenIDs = screenIds;
		}
			

		public override bool Condition()
		{
			int currentScreenID = ScreenManager.Instance.GetCurrentScreen().ScreenID.Get;

			bool screenNot = true;
			for (int i = 0; i < m_screenIDs.Length; ++i)
			{
				screenNot = screenNot && (m_screenIDs[i] != currentScreenID);

			}

			return screenNot;
		}
	}

	public class ConditionUIElementLoaded : QuestCondition
	{
		private string m_prefabName;
		private bool m_condition;

		public ConditionUIElementLoaded(string prefabName, bool condition)
		{
			m_prefabName = prefabName;
			m_condition = condition;
		}


		public override bool Condition()
		{
			UI.Screen currentScreen = ScreenManager.Instance.GetCurrentScreen();
			Transform transform = currentScreen.transform.FindChildByName(m_prefabName);

			return ((transform != null) == m_condition);
		}
	}
}

#endif