using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace UI
{
	public abstract class PopupContainerBaseWidget : UI.UIElement
	{

		public interface IDelegate
		{
			void FinishedAnimatingClosed();
		}

		[SerializeField] private Image m_backgroundImage;
		[SerializeField] private Button m_closeButton;
		[SerializeField] private Button m_blockingButton;
		[SerializeField] private Transform m_contentRoot;

		private PopupBase m_popupContentBase;
		private IDelegate m_delegate;
		private Action m_containerClosed;
		private bool m_closePressed;
		private Vector2 m_originalSize;

		protected override void Awake()
		{
			base.Awake();

			RectTransform rt = (RectTransform)(m_contentRoot.parent);
			m_originalSize = new Vector2 (rt.sizeDelta.x, rt.sizeDelta.y);
		}

		public override void PopulateUIElements()
		{
			// NOTHING YET
		}

		public virtual void Initialise(IDelegate myDelegate)
		{
			m_delegate = myDelegate;
		}

		private void HandleBackButtonEvent()
		{
			if (!m_closePressed && (m_blockingButton.enabled || m_closeButton.gameObject.activeSelf))
			{
				OnClosePressed();
			}
		}

		public virtual PopupBase CreateContent(PopupSetupDataBase data)
		{
			if (m_popupContentBase != null)
			{
				Utils.Debugger.Warning("Already have popup content base", (int)SharedSystems.Systems.POPUP_MANAGER);
				return null;
			}
			else
			{
				RectTransform rt = (RectTransform)(m_contentRoot.parent);
				if (data.containerHeight > 0f)
				{
					rt.sizeDelta = new Vector2 (rt.sizeDelta.x, data.containerHeight);	
				}
				else
				{
					rt.sizeDelta = m_originalSize;
				}

				m_closePressed = false;

				m_closeButton.gameObject.SetActive(data.showCloseButton);
				m_backgroundImage.gameObject.SetActive(data.showBackground);

				m_containerClosed = data.containerClosed;

				m_blockingButton.enabled = data.backgroundClickDismisses;

				GameObject contentObject = LoadUIElement(data.prefabName, Vector3.zero, m_contentRoot);
				m_popupContentBase = contentObject.GetComponent<PopupBase>();

				if (m_popupContentBase != null)
				{
					m_popupContentBase.Init(data, ContentCloseButtonPressedCallback);
				}
				else
				{
					Utils.Debugger.Warning("Popup content prefab script does not derive from PopupBase: " + data.prefabName, (int)SharedSystems.Systems.POPUP_MANAGER);
				}

				PlayAnimationEntrance();

				InputUtils.InputFocusHandler.Instance.RegisterBackButtonCallback(this, HandleBackButtonEvent);

				return m_popupContentBase;
			}
		}

		public void ContentCloseButtonPressedCallback(PopupBase activePopup, int buttonIndex)
		{
			PlayAnimationExit();
		}

		public virtual void OnClosePressed()
		{
			m_closePressed = true;

			if (m_popupContentBase != null)
			{
				m_popupContentBase.OnContainerClosePressed();
			}

			PlayAnimationExit();
		}

		protected virtual void FinishedAnimatingOut()
		{
			if (m_popupContentBase != null)
			{
				Destroy(m_popupContentBase.gameObject);
				m_popupContentBase = null;
				InputUtils.InputFocusHandler.Instance.RemoveFocus(this);
			}

			if (m_delegate != null)
			{
				m_delegate.FinishedAnimatingClosed();
			}

			if (m_closePressed && m_containerClosed != null)
			{
				m_containerClosed();
			}
		}

		protected virtual void FinishedAnimatingIn()
		{
		}

		public abstract void PlayAnimationEntrance();
		public abstract void PlayAnimationExit();
	}
}
