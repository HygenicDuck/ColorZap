using UnityEngine;
using UnityEngine.UI;
using UI;
using Core;
using Utils;

namespace Utils
{
	public class FrameRateCounter : BaseBehaviour
	{
		public const string PREFAB_NAME = "FrameRateCounter";

		[SerializeField] private Text m_text;
		[SerializeField] private float m_smoothFactor = 0.1f;

		private float m_fpsSmoothed = 0.0f;

		private void Update()
		{
			float fps = Time.smoothDeltaTime != 0.0f ? 1.0f / Time.smoothDeltaTime : 0.0f;

			m_fpsSmoothed = Mathf.Lerp(m_fpsSmoothed, fps, m_smoothFactor);
				
			m_text.text = m_fpsSmoothed.ToString("N0") + " fps";
		}
	}
}
