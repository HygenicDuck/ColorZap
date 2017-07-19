#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Quests
{
	public class SubQuest
	{
		bool m_isCompleted;
		bool m_reset;
		string m_name;
		int m_step = 0;
		QuestEvent m_qEvent;
		Quest m_parent;

		List<QuestElement> m_elements = new List<QuestElement> ();

		public bool IsComplete { get { return m_isCompleted; } }
		public string Name { get { return m_name; } }

		public SubQuest(string name, Quest parent)
		{
			m_name = name;
			m_isCompleted = false;
			m_parent = parent;
		}


		public void Reset(int step = 0, bool setReset = false)
		{
			m_step = step-1;
			m_qEvent = null;
			m_reset = setReset;

			for (int i = step; i < m_elements.Count; ++i)
			{
				QuestElement element = m_elements[i];
				element.Reset();
			}
		}

		public void AddElement(QuestElement element, bool ignoreDelay = false)
		{
			element.ParentSubquest = this;
			m_elements.Add(element);

			if (element.GetType() == typeof(ActionDelay) && !ignoreDelay)
			{
				m_elements.Add(new ObjectiveTimerFinished ());
			}
		}

		public T GetPreviousElement<T>() where T: QuestElement
		{
			for (int i = m_step - 1; i >= 0; --i)
			{
				QuestElement element = m_elements[i];

				if (element.GetType() == typeof(T))
				{
					return element as T;
				}
				else if (element.GetType().IsSubclassOf(typeof(QuestCondition)))
				{
					QuestCondition condition = element as QuestCondition;
					T result = condition.SubQuest.GetPreviousElement<T>();
					if (result != null)
					{
						return result;
					}
				}
			}
			return null;
		}

		public bool CheckEvent(QuestEvent qEvent)
		{
			m_qEvent = qEvent;
			bool running = true;
			while (running && m_step < m_elements.Count && !m_reset)
			{
				QuestElement element = m_elements[m_step];
				if (!element.IsRunning)
				{
					element.IsRunning = true;
					QuestAction action = element as QuestAction;
					if (action != null)
					{
						QuestSystem.QuestDebugger("(" + m_name + ") execute ACTION " + action.ToString(), m_parent != null ? m_parent.Name : "");
						action.Execute();

						m_step++;
					}
					else
					{
						bool success;
						QuestObjective objective = element as QuestObjective;
						if (objective != null)
						{
							success = m_qEvent != null && objective.CheckEvent(m_qEvent);
						}
						else
						{
							QuestCondition condition = element as QuestCondition;
							if (condition != null)
							{
								QuestSystem.QuestDebugger("(" + m_name + ") check CONDITION " + condition.ToString(), m_parent != null ? m_parent.Name : "");
								success = condition.CheckEvent(m_qEvent);
							}
							else
							{
								if (element.GetType() == typeof(QuestAnd))
								{
									QuestAnd and = element as QuestAnd;
									success = m_qEvent != null && and.CheckEvent(m_qEvent);
								}
								else
								{
									QuestOr or = element as QuestOr;
									success = m_qEvent != null && or.CheckEvent(m_qEvent);
								}
							}
						}


						if (success)
						{
							QuestSystem.QuestDebugger("(" + m_name + "): " + element.ToString() + " ¡¡¡COMPLETED!!! ", m_parent != null ? m_parent.Name : "");
							m_step++;
						}
						else
						{
							running = false;
						}
					}
					element.IsRunning = false;
				}
				else
				{
					running = false;
				}
			}

			bool didReset = m_reset;
			m_reset = false;

			m_isCompleted = m_step == m_elements.Count;

			return m_isCompleted || didReset;
		}

//		public QuestSave.Quest.SubQuest SaveSubQuest()
//		{
//			QuestSave.Quest.SubQuest savedSubquest = new QuestSave.Quest.SubQuest ();
//
//			savedSubquest.name = m_name;
//			savedSubquest.complete = m_isCompleted;
//
//			return savedSubquest;
//		}
//
//
//		public void LoadSubQuest(QuestSave.Quest.SubQuest savedSubquest)
//		{
//			m_isCompleted = savedSubquest.complete;
//		}

	}
}

#endif