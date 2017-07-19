using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utils;
using AssetBundles;

namespace UI
{
	[System.Serializable]
	public class ScreenID
	{
		public ScreenID(int newID)
		{
			ID = newID;
		}

		[SerializeField]
		private int ID;

		public int Get
		{
			get
			{
				return ID;	
			}
		}
	}

	public class ScreenManager : MonoSingleton<ScreenManager>
	{
		public class TransitionData
		{
			private bool m_transitionInProgress;
			private bool m_inTransitionFinished;
			private bool m_outTransitionFinished;

			public TransitionData()
			{
				Reset();
			}

			public void Reset()
			{
				m_transitionInProgress = false;
				m_inTransitionFinished = false;
				m_outTransitionFinished = false;
			}

			public void StartTransition(Screen currentScreen, Screen toScreen)
			{
				Utils.Debugger.Assert(!m_transitionInProgress, "StartTranstion called but one already in progress");

				m_transitionInProgress = true;

				// WE NEED TO SET A FLAG TO TRUE FROM THE START IF THERE ISN'T A CURRENT SCENE OR TOSCENE
				m_inTransitionFinished = toScreen == null ? true : false;
				m_outTransitionFinished = currentScreen == null ? true : false;
			}

			public void SetInTransitionFinished()
			{
				m_inTransitionFinished = true;
			}

			public void SetOutTransitionFinished()
			{
				m_outTransitionFinished = true;
			}

			public bool GetInAndOutTransitionsFinished()
			{
				return (m_inTransitionFinished && m_outTransitionFinished) ? true : false;
			}
				
		}

		private const float SCREEN_RATIO_IPAD = 0.75f;
		private const float FADE_DURATION_OF_FRONT_END_MUSIC = 0.5f;

		private Stack<Screen> m_mainStack = new Stack<Screen> ();
		private Screen m_prevScreen;
		private Screen m_currentScreen;
		private TransitionData m_transitionData;
		private List<Screen> m_screensToDestory;

		//	TODO: This isn't good. Need to fix the ChallengeFlowScreen so it can safely be disabled and re-enabled without borking the animator
		private bool m_leavePreviousScreenActive;


		[SerializeField] private GameObject m_rootCanvas;
		private GameObject m_inputBlocker;

		protected override void Init()
		{
			m_transitionData = new TransitionData ();
			m_screensToDestory = new List<Screen> ();
		}

		public Screen GetCurrentScreen()
		{
			return m_currentScreen;
		}

		public int GetCurrentScreenID()
		{
			if (m_currentScreen != null)
			{
				return m_currentScreen.ScreenID.Get;
			}

			return -1;
		}

		public int GetRootScreenID()
		{
			// Return ID of first screen in stack
			foreach (Screen stackScreen in m_mainStack)
			{
				return stackScreen.ScreenID.Get;
			}

			return -1;
		}

		public void SetRootCanvas(GameObject rootCanvas)
		{
			m_rootCanvas = rootCanvas;
			CreateProtectionScreen();
		}

		public GameObject GetRootCanvas()
		{
			return m_rootCanvas;
		}

		public float GetCanvasRatio()
		{
			GameObject canvasObject = GetRootCanvas();
			RectTransform canvasRect = canvasObject.GetRectTransform();
			Vector2 canvasSize = canvasRect.GetSize();

			float canvasRatio = canvasSize.x / canvasSize.y;

			return canvasRatio;
		}

		public Vector2 GetScreenCenterPoint()
		{
			GameObject canvasObject = GetRootCanvas();
			RectTransform canvasRect = canvasObject.GetRectTransform();
			Vector2 canvasSize = canvasRect.GetSize();
			Vector2 center = canvasSize * 0.5f;

			return center;
		}

		public bool IsCanvasRatioIPad()
		{
			float canvasRatio = GetCanvasRatio();
			if (Mathf.Approximately(canvasRatio, 0.75f))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
			
		private void CreateProtectionScreen()
		{
			GameObject itemPrefab = (GameObject)AssetBundleManager.LoadAsset("prefabs", "InputBlocker");
			m_inputBlocker = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity) as GameObject;
			m_inputBlocker.transform.SetParent(m_rootCanvas.transform, false);
			m_inputBlocker.name = "InputBlocker";
			m_inputBlocker.SetActive(false);
		}
		public void ActivateProtectionScreen(bool activate)
		{
			m_inputBlocker.SetActive(activate);
		}


		public Screen CreateScreen(string prefabName)
		{
			Screen ret = null;

			Debugger.Log("CreateScreen(" + prefabName + ")", (int)SharedSystems.Systems.SCREEN_MANAGER);

			GameObject itemPrefab = (GameObject)AssetBundleManager.LoadAsset("prefabs", prefabName);

			if (itemPrefab != null)
			{
				GameObject obj = itemPrefab != null ? Instantiate(itemPrefab, Vector3.zero, Quaternion.identity) as GameObject : null;

				if (obj != null)
				{
					obj.transform.SetParent(m_rootCanvas.transform, false);
					ret = obj.GetComponent<Screen>();

					if (ret == null)
					{
						Debugger.Log("This element in missing Screen component: " + prefabName, Debugger.Severity.ERROR);
					}
				}
				obj.name = prefabName;

				Analytics.Dispatcher.SendScreenEvent(ret.name);
			}
			else
			{
				Debugger.Log("Failed to load screen prefab: " + prefabName, Debugger.Severity.ERROR);
			}

			return ret;
		}

		public T CreateScreen<T>(string prefabName) where T: Screen
		{			
			Screen baseScreen = CreateScreen(prefabName);

			T ret = baseScreen as T;

			return ret;
		}
			
		public void PushScreen(Screen destinationScreen, bool drawTransitionBehind = false, bool setupManualTransitions = false, bool leavePreviousScreenActive = false)
		{
			m_leavePreviousScreenActive = leavePreviousScreenActive;

			ProcessScreenAction(destinationScreen, drawTransitionBehind, setupManualTransitions);

			m_mainStack.Push(destinationScreen);

			Analytics.Dispatcher.SendScreenEvent(destinationScreen.name);
		}


		public void AddScreenAsRoot(Screen rootScreen)
		{
			Debugger.Log("AddScreenAsRoot(" + rootScreen.name + ")", (int)SharedSystems.Systems.SCREEN_MANAGER);

			Stack<Screen> reverseStack = new Stack<Screen> ();
			while (m_mainStack.Count > 0)
			{
				reverseStack.Push(m_mainStack.Pop());
			}

			m_mainStack.Push(rootScreen);
			while (reverseStack.Count > 0)
			{
				m_mainStack.Push(reverseStack.Pop());
			}

		}
	
		public void ReplaceScreen(Screen destinationScreen, bool drawTransitionBehind = false)
		{
			m_screensToDestory.Add(m_mainStack.Pop());
			m_mainStack.Push(destinationScreen);

			ProcessScreenAction(destinationScreen, drawTransitionBehind);
		}

		public void ReplaceScreenAtStackLevel(Screen destinationScreen, int stackLevel, bool drawTransitionBehind = false, bool setupManualTransitions = false)
		{
			while (m_mainStack.Count > stackLevel)
			{
				m_screensToDestory.Add(m_mainStack.Pop());
			}
			m_mainStack.Push(destinationScreen);

			ProcessScreenAction(destinationScreen, drawTransitionBehind, setupManualTransitions);
		}

		public void ReplaceAllScreens(Screen destinationScreen, bool drawTransitionBehind = false)
		{
			ReplaceScreenAtStackLevel(destinationScreen, 0, drawTransitionBehind);
		}

		private void ProcessScreenAction(Screen destinationScreen, bool drawTransitionBehind = false, bool setupManualTransitions = false)
		{
			Debugger.Log("ProcessScreenAction(" + destinationScreen.name + ")", (int)SharedSystems.Systems.SCREEN_MANAGER);

			if (drawTransitionBehind)
			{
				int currentIndex = m_currentScreen.transform.GetSiblingIndex();

				m_currentScreen.transform.SetSiblingIndex(currentIndex + 1);
				destinationScreen.transform.SetSiblingIndex(currentIndex);
			}

			m_transitionData.StartTransition(m_currentScreen, destinationScreen);

			Screen currentSceen = m_currentScreen;
			m_currentScreen = destinationScreen;

			if (setupManualTransitions)
			{
				m_prevScreen = currentSceen;
			}
			else
			{
				SetupScreenAnimations(currentSceen, destinationScreen);
			}
		}

		public void SetupManualTransitions(string prevScreenAnimName, string destinationScreenAnimName)
		{
			SetupManualScreenAnimations(m_prevScreen, prevScreenAnimName, m_currentScreen, destinationScreenAnimName);
		}

		public void PopScreen(bool drawTransitionBehind = false)
		{
			Screen prevScreen = m_mainStack.Pop();

			m_currentScreen = m_mainStack.Peek();

			m_screensToDestory.Add(prevScreen);

			m_currentScreen.gameObject.SetActive(true);

			if (drawTransitionBehind)
			{
				int currentIndex = m_currentScreen.transform.GetSiblingIndex();

				m_currentScreen.transform.SetSiblingIndex(currentIndex + 1);
				prevScreen.transform.SetSiblingIndex(currentIndex);
			}

			SetupScreenAnimations(prevScreen, m_currentScreen);
		}

		public void PopToRoot()
		{
			#if AUDIO_AVAILABLE
			AudioManager.Instance.MusicFadeVolumeTo(0f, FADE_DURATION_OF_FRONT_END_MUSIC, ChallengesTableView.AUDIO_FRONT_END_MUSIC);
			#endif
			PopToStackLevel(0);
		}

		public Screen PopToStackLevel(int stackLevel)
		{
			Screen prevScreen = m_currentScreen;
			Screen poppedScreen = null;

			while (m_mainStack.Count > (stackLevel + 1))
			{
				poppedScreen = m_mainStack.Pop();

				if (poppedScreen != prevScreen)
				{
					Destroy(poppedScreen.gameObject);
				}
			}

			if (poppedScreen == null)
			{
				Debugger.Log("PopToStackLevel() :: Already at Level " + stackLevel, Debugger.Severity.ERROR);
				return null;
			}

			m_currentScreen = m_mainStack.Peek();

			m_screensToDestory.Add(prevScreen);
			m_currentScreen.gameObject.SetActive(true);
			SetupScreenAnimations(prevScreen, m_currentScreen);

			return m_currentScreen;
		}
			
		public Screen PopToScreen(int popToScreenID)
		{
			Screen ret = null;

			int stackLevel = GetScreenStackLevel(popToScreenID);

			if (stackLevel < m_mainStack.Count && stackLevel >= 0)
			{
				ret = PopToStackLevel(stackLevel);
			}

			return ret;
		}


		public bool ScreenExistsOnStack(int screenID)
		{
			return ScreenManager.Instance.GetScreenStackLevel(screenID) > -1;
		}

		/// <summary>
		/// If the specified screen ID exists on the stack, gets the screen instance, otherwise returns null
		/// </summary>
		public Screen GetScreenInstanceFromStack(int screenID)
		{
			foreach (Screen stackScreen in m_mainStack)
			{
				if (stackScreen.ScreenID.Get == screenID)
				{
					// Early out with result
					return stackScreen;
				}
			}

			return null;
		}

		///	<summary>
		/// Returns -1 if screen is not in the stack 
		/// </summary>
		public int GetScreenStackLevel(int screenID)
		{
			int stackLevel = m_mainStack.Count - 1;
			bool found = false;

			foreach (Screen stackScreen in m_mainStack)
			{
				if (stackScreen.ScreenID.Get == screenID)
				{	
					found = true;
					break;
				}

				stackLevel--;
			}

			if (!found)
			{
				stackLevel = -1;
			}

			return stackLevel;
		}

		public Screen GetScreenFromID(int screenID)
		{
			foreach (Screen stackScreen in m_mainStack)
			{
				if (stackScreen.ScreenID.Get == screenID)
				{	
					return stackScreen;
				}
			}

			return null;
		}

		/// <summary>
		/// Get the id of the previous screen to the supplied ID. Returns -1 if there
		/// is no previous screen
		/// </summary>
		public int GetPreviousScreenId(int screenID)
		{
			return GetPreviousScreenId(GetScreenInstanceFromStack(screenID));
		}

		/// <summary>
		/// Get the id of the previous screen to the supplied instance. Returns -1 if there
		/// is no previous screen
		/// </summary>
		public int GetPreviousScreenId(Screen screenInstance)
		{
			if (screenInstance == null)
			{
				// Early out - bad parameter
				return -1;
			}

			bool foundTargetScreen = false;

			foreach (Screen stackScreen in m_mainStack)
			{
				if (foundTargetScreen)
				{
					// Early out - previous screen found
					return stackScreen.ScreenID.Get;
				}
				else if (stackScreen == screenInstance)
				{
					foundTargetScreen = true;
				}
			}

			// If we get here, we're not on the stack yet
			if (m_mainStack.Count > 0)
			{
				// Early out - use most recent screen as previous
				Screen prevScreen = m_mainStack.Peek();
				return prevScreen.ScreenID.Get;
			}

			// If we get here we didn't find a previous screen
			return -1;
		}

		public T PopToScreen<T>(int popToScreenID) where T: Screen
		{
			Screen baseScreen = PopToScreen(popToScreenID);

			T ret = baseScreen as T;

			return ret;
		}

		public Screen PopToOrCreateScreen(int popToScreenID, string prefabName, bool drawTransitionBehind = false)
		{
			UI.Screen baseScreen = ScreenManager.Instance.PopToScreen(popToScreenID);

			if (baseScreen == null)
			{
				baseScreen = ScreenManager.Instance.CreateScreen(prefabName);
				ScreenManager.Instance.PushScreen(baseScreen, drawTransitionBehind);
			}

			return baseScreen;
		}

		public T PopToOrCreateScreen<T>(int popToScreenID, string prefabName, bool drawTransitionBehind = false) where T: Screen
		{
			Screen baseScreen = PopToOrCreateScreen(popToScreenID, prefabName, drawTransitionBehind);

			T ret = baseScreen as T;

			return ret;
		}
		private void SetupManualScreenAnimations(Screen prevScreen, string prevAnimName, Screen newScreen, string newAnimName)
		{
			if (prevScreen != null && newScreen != null)
			{
				m_prevScreen = prevScreen;

				if (!string.IsNullOrEmpty(prevAnimName))
				{
					prevScreen.StartTransition(prevAnimName, FinishedOutAnim);	
				}
				else
				{
					FinishedOutAnim();
				}

				if (!string.IsNullOrEmpty(newAnimName))
				{
					newScreen.StartTransition(newAnimName, FinishedInAnim);	
				}
				else
				{
					FinishedInAnim();
				}
			}
			else
			{
				FinishedInAnim();
				FinishedOutAnim();
			}
		}

		private void SetupScreenAnimations(Screen prevScreen, Screen newScreen)
		{
			if (prevScreen != null && newScreen != null)
			{
				m_prevScreen = prevScreen;

				ActivateProtectionScreen(true);
				prevScreen.StartTransitionOut(newScreen.ScreenID, FinishedOutAnim);
				newScreen.StartTransitionIn(prevScreen.ScreenID, FinishedInAnim);
			}
			else
			{
				FinishedInAnim();
				FinishedOutAnim();
			}
		}


		public void FinishedOutAnim()
		{
			m_transitionData.SetOutTransitionFinished();
			if (m_transitionData.GetInAndOutTransitionsFinished())
			{
				FinishedTransitionAnimations();
			}
		}

		public void FinishedInAnim()
		{
			m_transitionData.SetInTransitionFinished();

			if (m_transitionData.GetInAndOutTransitionsFinished())
			{
				FinishedTransitionAnimations();
			}
		}

		private void FinishedTransitionAnimations()
		{
			ActivateProtectionScreen(false);
			if (m_screensToDestory.Count == 0 && m_prevScreen != null)
			{
				if( !m_leavePreviousScreenActive )
				{
					m_prevScreen.gameObject.SetActive(false);	
				}
			}

			if (m_prevScreen != null)
			{
				m_prevScreen.TransitionOutComplete();
			}
			m_prevScreen = null;

			var i = m_mainStack.Count - 1;
			// WE JUST SET ALL THE SIBLING INDEXES THE SAME SO IT SHOULD THEN USE THE STACK ORDER
			foreach (Screen screen in m_mainStack)
			{
				screen.transform.SetSiblingIndex(i);
				--i;
			}				

			foreach (Screen screen in m_screensToDestory)
			{
				Destroy(screen.gameObject);
			}

			m_screensToDestory.Clear();
			m_transitionData.Reset();

			m_leavePreviousScreenActive = false;

			if (m_currentScreen != null)
			{
				#if QUEST_SYSTEM_ACTIVE
				Quests.QuestSystem.Instance.SendEvent(new Quests.EventScreenLoaded (m_currentScreen.ScreenID.Get));
				#endif
//				MenuObserver.Instance.SetCurrentScreen(m_currentScreen.ScreenID.Get);

				m_currentScreen.TransitionInComplete();
			}
		}
	}
}