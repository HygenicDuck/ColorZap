using System;
using Utils;
using UnityEngine;
using KangaObjects;
using Kanga;
using System.Collections.Generic;

namespace Utils
{
	public class KangaEvents
	{
		public delegate void NetworkStatusChangedDelegate (bool networkLoss);
		public static event NetworkStatusChangedDelegate NetworkStatusChanged;
		public static void SendNetworkStatusChangedEvent(bool networkLoss)
		{
			if (NetworkStatusChanged != null)
			{
				Debugger.Log("Event NetworkStatusChanged sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.KANGA);
				NetworkStatusChanged(networkLoss);
			}
		}


		public delegate void ReceivedExtraDataDelegate (List<ServerObjectBase> data);
		public static event ReceivedExtraDataDelegate ReceivedExtraData;
		public static void SendExtraDataEvent(List<ServerObjectBase> data)
		{
			if (ReceivedExtraData != null)
			{
				Debugger.Log("Event Received Extra Data sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.KANGA);
				ReceivedExtraData(data);
			}
		}
	}
}