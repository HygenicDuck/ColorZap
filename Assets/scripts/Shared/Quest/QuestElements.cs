#if QUEST_SYSTEM_ACTIVE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Quests
{
	public abstract class QuestEvent
	{
	}

	public abstract class QuestTrigger
	{
		protected bool m_isTriggered = false;
		public bool IsTriggered {	get { return m_isTriggered; } set{ m_isTriggered = value; } }
		public abstract bool CheckEvent(QuestEvent questEvent);
	}

	public abstract class QuestElement
	{
		protected SubQuest m_parentSubquest;
		public SubQuest ParentSubquest { set { m_parentSubquest = value; } }
		protected bool m_isRunning = false;
		public bool IsRunning { set { m_isRunning = value; } get { return m_isRunning; } }
		public virtual void Reset() {}
	}

	public abstract class QuestAction: QuestElement
	{
		public abstract void Execute();
	}

	public abstract class QuestObjective:  QuestElement
	{
		public abstract bool CheckEvent(QuestEvent questEvent);
	}

	public abstract class QuestCondition: QuestElement
	{
		public abstract bool Condition();
		private SubQuest m_subquest;
		public bool m_conditionAccomplished = false;

		public SubQuest SubQuest { get { return m_subquest; } }

		public QuestCondition ()
		{
			m_subquest = new SubQuest ("ConditionSubquest", null);
		}

		public bool CheckEvent(QuestEvent qEvent)
		{
			if (m_conditionAccomplished || Condition())
			{
				m_conditionAccomplished = true;
				return m_subquest.CheckEvent(qEvent);
			}
			else
			{
				return true;
			}
		}

		public void AddElement(QuestElement element)
		{
			m_subquest.AddElement(element);
		}

		public override void Reset()
		{
			m_conditionAccomplished = false;
			m_subquest.Reset(setReset:true);
		}
	}

	public class QuestAnd: QuestElement
	{
		List <QuestObjective> m_objectives = new List<QuestObjective> ();
		List <bool> m_completed = new List<bool> ();

		public bool CheckEvent(QuestEvent qEvent)
		{
			bool completed = true;
			for (int i = 0; i < m_objectives.Count; ++i)
			{
				QuestObjective objective = m_objectives[i];
				if (!m_completed[i])
				{
					m_completed[i] = objective.CheckEvent(qEvent);
				}
				completed = completed && m_completed[i];
			}

			return completed;
		}

		public void AddObjective(QuestObjective objective)
		{
			m_objectives.Add(objective);
			m_completed.Add(false);
		}
	}

	public class QuestOr: QuestElement
	{
		List <QuestObjective> m_objectives = new List<QuestObjective> ();

		public bool CheckEvent(QuestEvent qEvent)
		{
			for (int i = 0; i < m_objectives.Count; ++i)
			{
				QuestObjective objective = m_objectives[i];

				if (objective.CheckEvent(qEvent))
				{
					return true;
				}
			}

			return false;
		}

		public void AddObjective(QuestObjective objective)
		{
			m_objectives.Add(objective);
		}
	}
}

#endif