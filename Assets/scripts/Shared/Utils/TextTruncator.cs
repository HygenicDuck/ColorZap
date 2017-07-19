using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
	public class TextTruncator : BaseBehaviour
	{
		[SerializeField] private int m_numCharacters = 13;
		[SerializeField] private bool m_useElippsis = false;
		[SerializeField] private bool m_isUserName = false;

		private Text m_textField;
		private string m_text;

		protected override void Awake()
		{
			base.Awake();

			m_textField = GetComponent<Text>();

			UpdateText();
		}

		void Update()
		{
			if (m_textField.text != m_text)
			{
				UpdateText();
			}
		}

		// Use this for initialization
		public void UpdateText()
		{
			if (m_isUserName)
			{
//				m_textField.TruncateName(m_textField.text, m_numCharacters);

			}
			else
			{
//				m_textField.TruncateText(m_textField.text, m_useElippsis, m_numCharacters);
			}

			m_text = m_textField.text;
		}
	}
}