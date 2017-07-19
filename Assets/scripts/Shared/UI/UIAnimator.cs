using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using Core;
using Utils;

namespace UI
{
	public class UIAnimator : BaseBehaviour
	{
		public enum ScreenTransitionType
		{
			NONE,
			INTRO,
			OUTRO
		}

		[System.Serializable] 
		public class UIAnimState
		{
			public string animationName;
			public bool isPositionRelative;
			public bool overrideFirstPosition;
			public ScreenTransitionType screenTransitionType = ScreenTransitionType.NONE;
			public ScreenID otherScreenID = new ScreenID (0);
			public string soundEffect = string.Empty;

			[System.NonSerialized]
			public int animationNameHash = -1;

			public int GetAnimNameHash()
			{
				if (animationNameHash == -1)
				{
					animationNameHash = Animator.StringToHash(animationName);
				}

				return animationNameHash;
			}
		}

		private const float FINISH_ANIM_STATE_TIME = 0.99f; 

		[SerializeField] private List<UIAnimState> m_animStates = new List<UIAnimState> ();
		[SerializeField] private Animator m_animator;
		[SerializeField] private bool m_updateFlow;
		[SerializeField] private bool m_useFirstFrameHack = false;
		[SerializeField] private bool m_continueAnimationOnEnable = true;
		[SerializeField] private bool m_turnOnAnimatorOnEnable = true;

		private Vector3 m_startPosition = Vector3.zero;
		private bool m_animStarted;
		private Action m_animFinishedCallback = null;
		private Action<string, bool> m_animStateChangedCallback;
		private UIAnimState m_activeUIAnimState;
		private UIAnimState m_incomingUIAnimState;
		private bool m_activeAnimHasFinished = true;
		private float m_currentClipTime = 0f;
		private bool m_isOverrideFirstPosition;
		private bool m_isRelativePosition;
		private IEnumerator m_checkAnimFinishedCoroutine;

		protected override void Awake()
		{
			base.Awake();

			if (m_animator == null)
			{
				m_animator = GetComponent<Animator>();
			}
		}

		protected void Start()
		{
			if (m_useFirstFrameHack)
			{
				/// UPDATE: Unity will not fix this bug, it is now up to us to rework how we initialse our prefabs in order to avoid this frame glitch, this hacky fix is causing animations to 
				/// play incorrectly http://ec2-54-72-119-128.eu-west-1.compute.amazonaws.com/issue/PUB-895 :(

				// http://forum.unity3d.com/threads/5-2-1p1-problem-when-mecanim-animation-on-the-first-frame.357737/ this appears to be a bug that was supposidly fixed in 3.1 but we are still getting it,
				// the animator pops for a single frame, using the prefab values instead of the values inside the default animation state. The workaround is to update the animator in awake so that it starts
				// inside the default state
				m_animator.Update(0.02f);
			}
		}

		void OnEnable()
		{
			// automatically switch the animator on incase it accidentally gets turned off in the editor and then checked in.
			m_animator.enabled = m_turnOnAnimatorOnEnable;

			if (m_activeUIAnimState != null && m_continueAnimationOnEnable)
			{
				m_animator.Play(m_activeUIAnimState.animationName, 0, m_currentClipTime);
			}
		}

		void OnDisable()
		{
			m_animator.enabled = false;
		}

		public List<UIAnimState> GetAnimationList()
		{		
			return m_animStates;
		}

		public void AddState(string name)
		{		
			foreach (UIAnimState state in m_animStates)
			{
				if (state.animationName == name)
				{
					return;
				}
			}

			UIAnimState newState = new UIAnimState ();
			newState.animationName = name;

			m_animStates.Add(newState);
		}

		public void SetAnimator(Animator animator)
		{		
			m_animator = animator;
		}

		public Animator GetAnimator()
		{
			return m_animator;
		}

		public void SetupStateChangedCallback(Action<string,bool> animStateChangedCallback)
		{
			m_animStateChangedCallback = animStateChangedCallback;
		}


		public void ChangeAnimaState(int index, string animationName)
		{	
			UIAnimState animState = m_animStates[index];
			animState.animationName = animationName;
			animState.animationNameHash = -1;	
		}

		private void PlayAnimationInternal(UIAnimState state, Action onFinished = null, float startTime = 0)
		{
			if (state == null)
			{
				if (onFinished != null)
				{
					onFinished();
				}

				return;
			}

			m_incomingUIAnimState = state;

			m_animFinishedCallback = onFinished;

			m_startPosition = transform.localPosition;
			m_isOverrideFirstPosition = state.overrideFirstPosition;
			m_isRelativePosition = state.isPositionRelative;

			m_animator.Play(state.animationName, 0, startTime);

			if (m_animStateChangedCallback != null)
			{
				m_animStateChangedCallback(state.animationName, true);
			}

			m_currentClipTime = 0;

			m_animStarted = false;

			AudioManager.Instance.PlayAudioClip(state.soundEffect);				
		}

		private void SetupNewActiveAnimState(UIAnimState state)
		{
			m_activeUIAnimState = state;
			m_isOverrideFirstPosition = state.overrideFirstPosition;
			m_isRelativePosition = state.isPositionRelative;
			m_activeAnimHasFinished = false;

			if (m_animStarted && m_animStateChangedCallback != null)
			{
				m_animStateChangedCallback(m_activeUIAnimState.animationName, true);
			}

			m_animStarted = true;
		}
		 
		private void Update()
		{
			if (ShouldUpdateAnimState())
			{
				UpdateAnimState();
			}	
		}

		private bool ShouldUpdateAnimState()
		{
			//	TODO: May need to Optimize this so we only update if m_updateFlow OR if the anim is currently active
			return /*m_updateFlow && */m_animator != null;
		}

		public void PlayAnimation(string animationName, Action onFinished, float startTime = 0)
		{
			if (gameObject.activeSelf && gameObject.activeInHierarchy)
			{
				PlayAnimationInternal(GetAnimStateFromAnimationName(animationName), onFinished, startTime);
			}
		}

		public void PlayAnimation(string animationName)
		{
			if (gameObject.activeSelf && gameObject.activeInHierarchy)
			{
				PlayAnimation(animationName, null);
			}
			else
			{
				Debugger.Error("Trying to play animation: " + animationName + ", but game object is not activeSelf or InHierachy");
			}
		}

		public void TransitionToScreen(ScreenID otherScreenID, ScreenTransitionType transitionType, Action onFinished = null)
		{
			PlayAnimationInternal(GetAnimStateFromScreenID(otherScreenID, transitionType), onFinished);
		}

		private void AnimationFinished()
		{
			if (m_animFinishedCallback != null)
			{
				System.Action action = m_animFinishedCallback;
				m_animFinishedCallback = null;

				action();
			}
		}

		private UIAnimState GetAnimStateFromAnimationName(string animationName)
		{
			foreach (UIAnimState animState in m_animStates)
			{
				if (animState.animationName == animationName)
				{
					return animState; 
				}
			}

			Utils.Debugger.Log("The animation " + animationName + " trying to be played in (" + gameObject.name + ") doesn't exist", Utils.Debugger.Severity.ERROR);
			return null;
		}

		private UIAnimState GetAnimStateFromScreenID(ScreenID otherScreenID, ScreenTransitionType transitionType)
		{
			UIAnimState ret = null;
			foreach (UIAnimState animState in m_animStates)
			{
				if (animState.otherScreenID.Get == otherScreenID.Get &&
				    animState.screenTransitionType == transitionType)
				{
					ret = animState;
					break;
				}
			}
			return ret;
		}

		private UIAnimState GetAnimStateFromAnimationNameHash(int animationNameHash)
		{
			foreach (UIAnimState animState in m_animStates)
			{
				if (animState.GetAnimNameHash() == animationNameHash)
				{
					return animState;
				}
			}

			return null;
		}

		private void LateUpdate()
		{
			if (m_isRelativePosition)
			{
				transform.localPosition += m_startPosition;
			}
			else if (m_isOverrideFirstPosition)
			{
				float currentClipTime = m_animStarted ? m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime : 0;
				Vector3 posDifference = (m_startPosition - transform.localPosition);
				transform.localPosition += Vector3.Lerp(posDifference, Vector3.zero, currentClipTime);
			}
		}

		private void UpdateAnimState()
		{
			AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);

			m_currentClipTime = stateInfo.normalizedTime;

			if (m_incomingUIAnimState != null)
			{
				if (stateInfo.shortNameHash == m_incomingUIAnimState.GetAnimNameHash())
				{
					SetupNewActiveAnimState(m_incomingUIAnimState);

					m_incomingUIAnimState = null;
				}
			}
			else
			{
				UpdateAnimFinished(stateInfo);
				UpdateAnimStarted(stateInfo);
			}
		}

		private void UpdateAnimStarted(AnimatorStateInfo stateInfo)
		{
			int animStateShortNameHash = stateInfo.shortNameHash;
			int currentAnimStateShortNameHash = m_activeUIAnimState != null ? m_activeUIAnimState.GetAnimNameHash() : -1;

			if (animStateShortNameHash != currentAnimStateShortNameHash)
			{
				UIAnimState uiState = GetAnimStateFromAnimationNameHash(animStateShortNameHash);
				if (uiState != null)
				{
					SetupNewActiveAnimState(uiState);
				}
			}
		}

		private void UpdateAnimFinished(AnimatorStateInfo stateInfo)
		{
			int animStateShortNameHash = stateInfo.shortNameHash;
			int currentAnimStateShortNameHash = m_activeUIAnimState != null ? m_activeUIAnimState.GetAnimNameHash() : -1;
		
			if (!m_activeAnimHasFinished)
			{
				if (stateInfo.normalizedTime >= 1f || animStateShortNameHash != currentAnimStateShortNameHash)
				{
					m_activeAnimHasFinished = true;

					if (m_animStateChangedCallback != null)
					{
						m_animStateChangedCallback(m_activeUIAnimState.animationName, false);
					}

					AnimationFinished();
				}
			}
		}

		public void ContinueAnimFlow(string triggerName)
		{
			m_animator.SetTrigger(triggerName);
		}

		public void SetAnimatorSpeed(float speed)
		{
			m_animator.speed = speed;
		}

		public void StopAnimator()
		{
			m_animator.enabled = false;
		}

		public bool IsAScreenTransition(string animName)
		{
			bool ret = false;
			foreach (UIAnimState animState in m_animStates)
			{
				if (animState.animationName == animName &&
				    animState.screenTransitionType != ScreenTransitionType.NONE)
				{
					ret = true;
					break;
				}
			}
			return ret;
		}

		public void FinishCurrentAnimationNow()
		{
			AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);
			m_animator.Play(m_activeUIAnimState.animationName, 0, FINISH_ANIM_STATE_TIME * stateInfo.length);
		}

		private void AnimEventPlaySound(string sound)
		{
//			Debugger.Log("AnimEventPlaySound " + sound, (int)WaffleSystems.Systems.AUDIO);
			AudioManager.Instance.PlayAudioClip(sound);
		}

		private void AnimEventStopSound()
		{
//			Debugger.Log("AnimEventStopSound ", (int)WaffleSystems.Systems.AUDIO);
			AudioManager.Instance.StopSoundEffect();
		}

		private void AnimEventPlayMusic(string sound)
		{
//			Debugger.Log("AnimEventPlayMusic " + sound, (int)WaffleSystems.Systems.AUDIO);
			AudioManager.Instance.PlayMusic(sound);
		}

		private void AnimEventStopMusic(float time = 0f)
		{
//			Debugger.Log("AnimEventStopMusic ", (int)WaffleSystems.Systems.AUDIO);
			AudioManager.Instance.MusicFadeVolumeTo(0f, time);
		}

		private void AnimEventFadeUpMusic(float time = 0f)
		{
//			Debugger.Log("AnimEventFadeUpMusic ", (int)WaffleSystems.Systems.AUDIO);
			AudioManager.Instance.MusicFadeVolumeTo(1f, time);
		}
	}
}