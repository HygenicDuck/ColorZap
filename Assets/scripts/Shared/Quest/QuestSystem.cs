#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Quests
{
	public class QuestSystem
	{
		private List<Quest> m_quests = new List<Quest> ();
		private List<Quest> m_activeQuests = new List<Quest> ();
		private List<Quest> m_completedQuests = new List<Quest> ();
		private bool m_activated;
		private List<QuestEvent> m_queuedEvents = new List<QuestEvent> ();

		private static QuestSystem s_instance;

		public static QuestSystem Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new QuestSystem ();
				}
				return s_instance; 
			} 
		}

		public void Activate()
		{
			m_activated = true;

			foreach(QuestEvent questEvent in m_queuedEvents)
			{
				SendEvent(questEvent);
			}

			m_queuedEvents.Clear();
		}

		public void Clear()
		{
			m_activeQuests.Clear();
			m_completedQuests.Clear();
			m_quests.Clear();
		}

		public void SendEvent(QuestEvent questEvent)
		{
			if (m_activated)
			{
				Utils.Debugger.Log("Sent event \"" + questEvent + "\"", (int)SharedSystems.Systems.QUEST);

				for (int i = m_quests.Count - 1; i >= 0; --i)
				{
					Quest quest = m_quests[i];
					bool triggered = quest.CheckTriggers(questEvent);
				
					if (triggered)
					{
						StartQuest(quest);
					}
				}

				for (int i = m_activeQuests.Count - 1; i >= 0; --i)
				{
					Quest quest = m_activeQuests[i];
					bool isCompleted = quest.CheckEvent(questEvent);
					if (isCompleted)
					{
						CompleteQuest(quest);
					}
				}
			}
			else
			{
				Utils.Debugger.Log("Quest system not activated. Queing event \"" + questEvent + "\"", (int)SharedSystems.Systems.QUEST);

				m_queuedEvents.Add(questEvent);
			}
		}

		public void TriggerQuest(string questName)
		{
			Quest quest = SearchQuestByName(questName);
			if (quest != null)
			{
				StartQuest(quest);
			}
		}

		public void AddQuest(Quest quest)
		{
			if (quest.IsComplete)
			{
				m_completedQuests.Add(quest);
			}
			else
			{
				if (quest.IsActive)
				{
					m_activeQuests.Add(quest);
				}
				else
				{
					m_quests.Add(quest);
				}
			}
		}
			
		private void StartQuest(Quest quest)
		{
			QuestSystem.QuestDebugger("               /////_QUEST [" + quest.Name + "] STARTED_////// ", quest.Name);
			Analytics.Dispatcher.SendEvent(Analytics.Events.QUEST_STARTED, "quest_name", quest.Name);

			quest.Start();
			m_activeQuests.Add(quest);
			m_quests.Remove(quest);
		}

		private void CompleteQuest(Quest quest)
		{
			QuestSystem.QuestDebugger("               ¡¡¡¡¡¡ QUEST" + quest.Name + " COMPLETED !!!!!!! ", quest.Name);
			Analytics.Dispatcher.SendEvent(Analytics.Events.QUEST_COMPLETED, "quest_name", quest.Name);

			quest.Complete();
			m_activeQuests.Remove(quest);
			m_completedQuests.Add(quest);
			SendEvent(new EventQuestFinished (quest.Name));
			SaveQuests();
		}

		public void ForceCompleteQuests(List<string> questNames)
		{
			foreach (string questName in questNames)
			{
				Quest quest = SearchQuestByName(questName);

				if (quest != null && !quest.IsComplete)
				{
					quest.Complete();
					m_activeQuests.Remove(quest);
					m_completedQuests.Add(quest);
				}
			}

			SaveQuests();
		}

		public bool IsAnyQuestActive()
		{
			return m_activeQuests.Count > 0;
		}

		public bool IsQuestActive(string questName)
		{
			Quest quest = SearchActiveQuestByName(questName);
			return (quest != null && quest.IsActive);
		}

		public bool IsQuestCompleted(string questName)
		{
			Quest quest = SearchCompletedQuestByName(questName);
			return (quest != null && quest.IsComplete);
		}

		public Quest SearchQuestByName(string questName)
		{
			foreach (Quest quest in m_quests)
			{
				if (quest.Name == questName)
				{
					return quest;
				}
			}
			return null;
		}

		public Quest SearchActiveQuestByName(string questName)
		{
			foreach (Quest quest in m_activeQuests)
			{
				if (quest.Name == questName)
				{
					return quest;
				}
			}
			return null;
		}

		public Quest SearchCompletedQuestByName(string questName)
		{
			foreach (Quest quest in m_completedQuests)
			{
				if (quest.Name == questName)
				{
					return quest;
				}
			}
			return null;
		}

		public void Print()
		{
			foreach (Quest quest in m_quests)
			{
				Utils.Debugger.Log("Quest " + quest.Name, (int)SharedSystems.Systems.QUEST);
			}
		}


/*		public void SaveQuests()
		{
			int questsSize = m_quests.Count + m_completedQuests.Count + m_activeQuests.Count;
			QuestSave questSaveData = new QuestSave ();
			questSaveData.quests = new QuestSave.Quest[questsSize];

			for (int i = 0; i < m_activeQuests.Count; ++i)
			{
				Quest quest = m_activeQuests[i];
				questSaveData.quests[i] = quest.SaveQuest();
			}
			int addIndex = m_activeQuests.Count;
			for (int i = 0; i < m_completedQuests.Count; ++i)
			{
				Quest quest = m_completedQuests[i];
				questSaveData.quests[i + addIndex] = quest.SaveQuest();
			}
			addIndex += m_completedQuests.Count;
			for (int i = 0; i < m_quests.Count; ++i)
			{
				Quest quest = m_quests[i];
				questSaveData.quests[i + addIndex] = quest.SaveQuest();
			}

			WaffleRequests.QuestSave.QuestSaveOptions options = new WaffleRequests.QuestSave.QuestSaveOptions (questSaveData);
			WaffleRequests.QuestSave.SaveQuest(options, null, null);
		}*/

//		public void LoadQuests(QuestSave questSave)
//		{
//			for (int i = 0; i < questSave.quests.Length; ++i)
//			{
//				QuestSave.Quest savedQuest = questSave.quests[i];
//
//				Quest quest = SearchQuestByName(savedQuest.name);
//				if (quest != null)
//				{
//					quest.LoadQuest(savedQuest);
//
//					if (quest.IsComplete)
//					{
//						m_quests.Remove(quest);
//						m_completedQuests.Add(quest);
//					}
//					else if (quest.IsActive)
//					{
//						m_quests.Remove(quest);
//						m_activeQuests.Add(quest);
//					}
//				}
//			}
//		}


//		public string[] m_debugFilterQuest = {
//			PlayphotoTutorial.PLAY_NEW_GAME
//		};
		public static void QuestDebugger(string message, string questName)
		{
			#if !SERVER_ENVIROMENT_CANDIDATE || CANDIDATE_DEBUG
			if (Instance.m_debugFilterQuest.Length > 0 && !string.IsNullOrEmpty(questName))
			{
				for (int i = 0; i < Instance.m_debugFilterQuest.Length; ++i)
				{
					if (Instance.m_debugFilterQuest[i] == questName)
					{
						Utils.Debugger.Log(message, (int)SharedSystems.Systems.QUEST);
						return;
					}
				}
			}
			else
			{
				Utils.Debugger.Log(message, (int)SharedSystems.Systems.QUEST);
			}
			#endif
		}

	}
}
#endif