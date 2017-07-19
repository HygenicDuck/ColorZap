using UnityEngine;
using UnityEngine.UI;
using System;
using UI;
using Core;
using Utils;

namespace UI
{
	public class ButtonSound : BaseBehaviour
	{
		[SerializeField]
		string m_soundToPlay;
		[SerializeField]
		string m_toggleOffSound;

		Button m_button;
		Toggle m_toggle;

		void Start()
		{
			m_button = GetComponent<Button>();
			if (m_button == null)
			{
				m_toggle = GetComponent<Toggle>();
				if (m_toggle == null)
				{
//					Debugger.Error("ButtonSound component : can't find a button or toggle on gameObject '" + gameObject.name + "'", (int)WaffleSystems.Systems.AUDIO);
					return;
				}

				m_toggle.onValueChanged.AddListener(OnTogglePressed);
				return;
			}

			m_button.onClick.AddListener(OnButtonPressed);
		}

		public void OnButtonPressed()
		{
			AudioManager.Instance.PlayAudioClip(m_soundToPlay);
		}

		public void OnTogglePressed(bool isEnabled)
		{
			if (isEnabled)
			{
				AudioManager.Instance.PlayAudioClip(m_soundToPlay);
			}
			else
			{
				AudioManager.Instance.PlayAudioClip(m_toggleOffSound);
			}
		}

	}
}
