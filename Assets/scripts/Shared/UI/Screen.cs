using System;
using System.Collections.Generic;
using UnityEngine;
using UI;
using Core;

namespace UI
{
	public abstract class Screen : UIElement
	{
		[SerializeField]
		protected ScreenID m_screenID;

		public const string HOMEBACK_SFX = "homeBack";


		#region Properties

		public ScreenID ScreenID
		{
			get
			{
				return m_screenID;
			}
		}

		#endregion

		protected override void Awake()
		{
			base.Awake();
		}

		protected virtual void OnEnable()
		{
			InputUtils.InputFocusHandler.Instance.AddFocus(this);
		}

		protected virtual void OnDisable()
		{
			InputUtils.InputFocusHandler.Instance.RemoveFocus(this);
		}

		public virtual void BackClick()
		{
			AudioManager.Instance.PlayAudioClip(HOMEBACK_SFX);
			ScreenManager.Instance.PopScreen();
		}

		public virtual void TransitionInComplete()
		{
		}

		public virtual void TransitionOutComplete()
		{
		}

		public void StartTransitionOut(ScreenID targetScreenID, Action finishedCallback = null)
		{		
			if (m_uiAnimator != null)
			{
				m_uiAnimator.TransitionToScreen(targetScreenID, UIAnimator.ScreenTransitionType.OUTRO, finishedCallback);
			}
			else if (finishedCallback != null)
			{
				finishedCallback();
			}
		}

		public void StartTransitionIn(ScreenID previousScreenID, Action finishedCallback = null)
		{		
			if (m_uiAnimator != null)
			{
				m_uiAnimator.TransitionToScreen(previousScreenID, UIAnimator.ScreenTransitionType.INTRO, finishedCallback);
			}
			else if (finishedCallback != null)
			{
				finishedCallback();
			}
		}

		public void StartTransition(string animName, Action finishedCallback = null)
		{		
			if (m_uiAnimator != null)
			{
				m_uiAnimator.PlayAnimation(animName, finishedCallback);
			}
			else if (finishedCallback != null)
			{
				finishedCallback();
			}
		}
	}
}