using System;
using Utils;
using UnityEngine;
using KangaObjects;

namespace Utils
{
	public class InventoryEvents
	{
		public delegate void ItemsFetchedDelegate ();
		public static event ItemsFetchedDelegate ItemsFetched;
		public static void SendItemsFetchedEvent()
		{
			if (ItemsFetched != null)
			{
//				Debugger.Log("Event ItemsFetched sent", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.COLLECTIBLES);
				ItemsFetched();
			}
		}

		public delegate void CollectiblesLoadedDelegate ();
		public static event CollectiblesLoadedDelegate CollectiblesLoaded;
		public static void SendCollectiblesLoadedEvent()
		{
			if (CollectiblesLoaded != null)
			{
//				Debugger.Log("Event CollectiblesLoaded sent", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.COLLECTIBLES);
				CollectiblesLoaded();
			}
		}

		public delegate void CollectiblesPackPurchaseDelegate(bool start);
		public static event CollectiblesPackPurchaseDelegate CollectiblesPackPurchase;
		public static void SendCollectiblesPackPurchaseEvent(bool start)
		{
			if (CollectiblesPackPurchase != null)
			{
//				Debugger.Log("Event CollectiblesLoaded sent", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.COLLECTIBLES);
				CollectiblesPackPurchase(start);
			}
		}
	}
}