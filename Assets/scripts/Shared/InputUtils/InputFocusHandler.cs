using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

namespace InputUtils
{
	public class InputFocusHandler : MonoSingleton<InputFocusHandler>
	{
		public class Focus
		{
			public int instanceID;
			public Action backButtonCallback;
		}

		private List<Focus> m_focusStack = new List<Focus> ();

		protected override void Init()
		{
		}

		private void OnEnable()
		{
			Utils.InputEvents.BackButtonDown += HandleBackButtonPressed;
		}

		private void OnDisable()
		{
			Utils.InputEvents.BackButtonDown -= HandleBackButtonPressed;
		}

		public Focus AddFocus(BaseBehaviour parent)
		{
			Focus newFocus = new Focus () { 
				instanceID = parent.GetInstanceID()
			};

			m_focusStack.Add(newFocus);
			return newFocus;
		}

		public void RemoveFocus(BaseBehaviour parent)
		{
			Focus focusToRemove = FindFocus(parent);
			if (focusToRemove != null)
			{
				m_focusStack.Remove(focusToRemove);
			}
		}

		private Focus FindFocus(BaseBehaviour parent)
		{
			Focus focus = m_focusStack.Find(item => item.instanceID == parent.GetInstanceID());
			return focus; 
		}

		private Focus FindOrCreateFocus(BaseBehaviour parent)
		{
			Focus focus = FindFocus(parent);

			if (focus == null)
			{
				focus = AddFocus(parent);
			}

			return focus;
		}

		public void RegisterBackButtonCallback(BaseBehaviour parent, Action backButtonCallback)
		{
			Focus focus = FindOrCreateFocus(parent);
			focus.backButtonCallback = backButtonCallback;
		}

		private void HandleBackButtonPressed()
		{
			if (m_focusStack.Count > 0)
			{
				Focus topFocus = m_focusStack[m_focusStack.Count - 1];
				if (topFocus.backButtonCallback != null)
				{
					topFocus.backButtonCallback();
				}
			}
		}
	}
}