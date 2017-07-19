#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Quests
{
	public class Quest
	{
		string m_name;
		List<QuestTrigger> m_triggers = new List<QuestTrigger> ();
		List<SubQuest> m_subquests = new List<SubQuest>();

		bool m_isCompleted = false;
		bool m_isActive = false;

		public string Name { get { return m_name; } }
		public bool IsComplete { get { return m_isCompleted; } }
		public bool IsActive { get { return m_isActive; } }

		public Quest(string name)
		{
			m_isCompleted = false;
			m_isActive = false;
			m_name = name;
		}

		public void Start()
		{
			m_isActive = true;
		}

		public void Complete()
		{
			m_isCompleted = true;
			m_isActive = false;
		}

		public void AddTrigger(QuestTrigger trigger)
		{
			m_triggers.Add(trigger);
		}

		public void AddSubquest(SubQuest subquest)
		{
			m_subquests.Add(subquest);
		}

		public bool CheckTriggers(QuestEvent qEvent)
		{
			if (qEvent == null)
			{
				return false;
			}

			bool triggerQuest = m_triggers.Count > 0;
			foreach (QuestTrigger questTrigger in m_triggers)
			{
				if (!questTrigger.IsTriggered)
				{
					questTrigger.IsTriggered = questTrigger.CheckEvent(qEvent);
					triggerQuest = triggerQuest && questTrigger.IsTriggered;
				}
			}

			QuestSystem.QuestDebugger("_/\\_.·->>> ["+m_name+"] Check trigger \"" + qEvent +"\""+ (triggerQuest ? " ****COMPLETED!!!***":""), m_name);

			return triggerQuest;
		}

		public bool CheckEvent(QuestEvent qEvent)
		{
			bool completed = true;
			foreach (SubQuest subQuest in m_subquests)
			{
				if (!subQuest.IsComplete)
				{
					bool justCompleted = subQuest.CheckEvent(qEvent);
					completed = completed && justCompleted;
				}

			}

			QuestSystem.QuestDebugger("_/\\_.·-> ["+m_name+"] Check event \"" + qEvent +"\""+ (completed ? " ****COMPLETED!!!***":""), m_name);

			return completed;
		}


		public SubQuest SearchSubQuestByName(string subquestName)
		{
			foreach (SubQuest subquest in m_subquests)
			{
				if (subquest.Name == subquestName)
				{
					return subquest;
				}
			}
			return null;
		}

//		public QuestSave.Quest SaveQuest()
//		{
//			int questSize = m_subquests.Count;
//
//			QuestSave.Quest savedQuest = new QuestSave.Quest();
//			savedQuest.subquests = new QuestSave.Quest.SubQuest[questSize];
//
//			for (int i = 0; i < questSize; ++i)
//			{
//				SubQuest subQuest = m_subquests[i];
//				savedQuest.subquests[i] = subQuest.SaveSubQuest();
//			}
//			savedQuest.name = m_name;
//			savedQuest.complete = m_isCompleted;
//			savedQuest.active = m_isActive;
//
//			return savedQuest;
//		}
//
//		public void LoadQuest(QuestSave.Quest savedQuest)
//		{
//			m_isCompleted = savedQuest.complete;
//			m_isActive = savedQuest.active;
//
//			for (int i = 0; i < savedQuest.subquests.Length; ++i)
//			{
//				QuestSave.Quest.SubQuest savedSubquest = savedQuest.subquests[i];
//
//				SubQuest subquest = SearchSubQuestByName(savedSubquest.name);
//				if (subquest != null)
//				{
//					subquest.LoadSubQuest(savedSubquest);
//				}
//			}
//		}
	}

}

#endif