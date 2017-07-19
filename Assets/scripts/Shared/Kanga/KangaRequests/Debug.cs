using UnityEngine;
using System.Collections;
using Kanga;
using System;
using KangaRequests;

namespace KangaRequests
{
	public static class Debug
	{
		public class DebugDumpOptions : BaseOptions
		{
			private string m_logText;
			private string m_logCategory;

			public DebugDumpOptions(string logText, string logCategory)
			{
				m_logText = logText;
				m_logCategory = logCategory;
			}

			public override void UpdateArgs(Hashtable args)
			{
				base.UpdateArgs(args);

				args.Add("log_text", m_logText);
				args.Add("log_category", m_logCategory);
			}
		}

		public static void DebugDump(DebugDumpOptions options, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback, Action<Kanga.ResponseObjectHandler> cacheCallback = null)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "debug-dump",
				cacheToDisk = false
			};

			options.UpdateArgs(request.requestData.args);

			string groupId = options.requestGroupId;

			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, cacheCallback, options.cacheTimeOut);
		}
	}
}

