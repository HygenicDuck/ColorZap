using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utils;
using WaffleUI;
using System;

namespace Core
{
	/// <summary>
	/// Base behaviour.
	/// All classes which inherit from MonoBehaviour should inherit from this class instead
	/// </summary>

	public class BaseBehaviour : MonoBehaviour
	{
		List<string> m_imageRequests;
		List<BaseServer> m_serversObject;

		protected virtual void Awake()
		{
		}

		protected virtual void OnDestroy()
		{
			CancelRequests();
		}

		protected virtual void CancelRequests()
		{
			if (m_serversObject != null)
			{
				foreach (BaseServer server in m_serversObject)
				{
					server.CancelRequests();
				}
			}

			if (m_imageRequests != null)
			{
				foreach (string imageKey in m_imageRequests)
				{
//					WaffleUtils.WebImage.CancelRequest(imageKey);
				}
				m_imageRequests.Clear();
			}
		}

		protected void ImageRequest(string url, Action<Utils.WebImage.ServerImage> callback, Action<string> failedCallback = null, Action<ProgressResponse> progressCallback = null, string cancelKey = WebImage.DEFAULT_URL_CANCEL_KEY, bool waffle = true, float requestTimeout = -1)
		{
			if (m_imageRequests == null)
			{
				m_imageRequests = new List<string> ();
			}

			if (cancelKey != null)
			{
				m_imageRequests.Add(cancelKey);
			}

			if (waffle)
			{
//				WaffleUtils.WebImage.RequestWaffle(url, callback, failedCallback, progressCallback, cancelKey, requestTimeout: requestTimeout);
			}
			else
			{
				Utils.WebImage.Request(url, callback, failedCallback, progressCallback, cancelKey, requestTimeout: requestTimeout);
			}
		}

		public void RegisterServer(BaseServer server)
		{
			if (m_serversObject == null)
			{
				m_serversObject = new List<BaseServer> ();
			}
			m_serversObject.Add(server);
		}

		protected void RunActionAfterDelay(float delay, Action action)
		{
			StartCoroutine(RunActionAfterDelayInternal(delay, action));
		}

		private IEnumerator RunActionAfterDelayInternal(float delay, Action action)
		{
			yield return new WaitForSeconds (delay);
			action();
		}

		protected void RunActionAfterFrame(int frames, Action action)
		{
			StartCoroutine(RunActionAfterDelayInternal(frames, action));
		}

		private IEnumerator RunActionAfterDelayInternal(int frames, Action action)
		{
			for (int i = 0; i < frames; ++i)
			{
				yield return new WaitForEndOfFrame ();
			}
			action();
		}
	}
}
