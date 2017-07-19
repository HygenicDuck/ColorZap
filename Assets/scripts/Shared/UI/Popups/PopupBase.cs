using UnityEngine;
using System.Collections;
using System;
using Core;

namespace UI
{
	public class PopupBase : UIElement
	{
		protected PopupManager.PopupType m_popupType;
		protected Action<PopupBase,int> m_notifyPopupManagerCallback;
		private Action<int> m_buttonCallback;

		public override void PopulateUIElements()
		{
			// NOTHING YET
		}

		public PopupManager.PopupType PopupType
		{
			get
			{
				return m_popupType;
			}
		}

		public void Init(PopupSetupDataBase data, Action<PopupBase,int> finished)
		{		
			m_notifyPopupManagerCallback = finished;
			m_popupType = data.popupType;
			m_buttonCallback = data.buttonCallback;

			Init(data);
		}

		protected virtual void Init(PopupSetupDataBase data)
		{	
		}

		protected void Close(int buttonIndex)
		{
			if (m_buttonCallback != null)
			{
				m_buttonCallback(buttonIndex);
			}

			if (m_notifyPopupManagerCallback != null)
			{
				m_notifyPopupManagerCallback(this, buttonIndex);
			}
		}

		protected void ButtonCallbackNoClose(int buttonIndex)
		{
			if (m_buttonCallback != null)
			{
				m_buttonCallback(buttonIndex);
			}
		}

		public virtual void OnContainerClosePressed()
		{
			
		}

		public void Close()
		{
			if (m_notifyPopupManagerCallback != null)
			{
				m_notifyPopupManagerCallback(this, 0);
			}
		}
	}
}
