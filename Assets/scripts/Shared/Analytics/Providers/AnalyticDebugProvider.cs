using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine.UI;

namespace Analytics
{
	public class AnalyticDebugProvider : Provider
	{
		private const float VISIBLE_TIME = 10f;
		private const string OBJECT_NAME = "AnalyticsDebug";

		private class AnalyticEvent
		{
			public string eventName;
			public float timeStamp;
		}

		private static bool s_active = false;

		private List<AnalyticEvent> m_firedEvents = new List<AnalyticEvent> ();
		private Text m_text;

		public static void EnableOnScreenDebug(bool enabled)
		{
			s_active = enabled;
		}

		private void PrintEventOnScreen(string eventName)
		{
			if (s_active == false)
			{
				return;
			}

			AnalyticEvent newEvent = new AnalyticEvent();

			newEvent.eventName = eventName;
			newEvent.timeStamp = Time.time;

			m_firedEvents.Add(newEvent);

			if (m_text == null)
			{
				GameObject text = GameObject.Find(OBJECT_NAME);

				if (text == null)
				{

					GameObject debugCanvas = GameObject.Find("DebugPopupCanvas");

					if (debugCanvas != null)
					{
						text = new GameObject (OBJECT_NAME);
						text.transform.SetParent(debugCanvas.transform);

						m_text = text.AddComponent<Text>();

						m_text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

						text.transform.position = debugCanvas.transform.position;

						RectTransform textRect = text.GetRectTransform();
						RectTransform canvasRect = debugCanvas.GetRectTransform();
						Vector2 size = canvasRect.GetSize();

						textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x * canvasRect.localScale.x);
						textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y * canvasRect.localScale.y);

						m_text.fontSize = 40;
						m_text.raycastTarget = false;
						m_text.color = Color.black;
					}
					else
					{
						return;
					}
				}
				else
				{
					m_text = text.GetComponent<Text>();
				}
			}

			m_text.text = "";

			for (int i = m_firedEvents.Count - 1; i >= 0; --i)
			{
				if (Time.time - m_firedEvents[i].timeStamp < VISIBLE_TIME)
				{
					m_text.text += m_firedEvents[i].eventName + "\n";
				}
				else
				{
					m_firedEvents.RemoveAt(i);
				}
			}
		}



		public override void StartSession()
		{
			PrintEventOnScreen("START SESSION");
		}

		public override void FlushSession()
		{
			PrintEventOnScreen("FLUSH SESSION");
		}

		public override void LogAdEvent(string eventName)
		{
			PrintEventOnScreen("AD EVENT: " + eventName);
		}

		public override void LogEvent(string eventName, Hashtable parameters)
		{
			if (s_active)
			{
				string stringParameters = "";

				foreach (DictionaryEntry pair in parameters)
				{
					stringParameters += ":- " + (string)pair.Key + " = " + pair.Value.ToString();
				}

				PrintEventOnScreen(eventName + stringParameters);
			}
		}

		public override void LogErrorEvent(string eventName, string errorDescription)
		{
			PrintEventOnScreen("ERROR EVENT: " + eventName + ", " + errorDescription);
		}

		public override void StartTimedEvent(string eventName, Hashtable parameters)
		{
			LogEvent("START TIMED EVENT: " + eventName, parameters);
		}

		public override void StopTimedEvent(string eventName)
		{
			PrintEventOnScreen("STOP TIMED EVENT: " + eventName);
		}
	}
}