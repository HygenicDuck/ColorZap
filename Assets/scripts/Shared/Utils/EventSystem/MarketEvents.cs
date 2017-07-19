using System;
using Utils;
using UnityEngine;
using KangaObjects;

namespace Utils
{
	public class MarketEvents
	{
		public delegate void ProductPurchasedDelegate (Product product);
		public static event ProductPurchasedDelegate ProductPurchased;
		public static void SendProductPurchasedEvent(Product product)
		{
			if (ProductPurchased != null)
			{
				Debugger.Log("Event ProductPurchased sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.MARKET);
				ProductPurchased(product);
			}
		}

		public delegate void ProductsFetchedDelegate ();
		public static event ProductsFetchedDelegate ProductsFetched;
		public static void SendProductsFetchedEvent()
		{
			if (ProductsFetched != null)
			{
				Debugger.Log("Event ProductsFetched sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.MARKET);
				ProductsFetched();
			}
		}

		public delegate void ProductCountryCodeFetchedDelegate (string countryCode);
		public static event ProductCountryCodeFetchedDelegate ProductCountryCodeFetched;
		public static void SendProductCountryCodeFetchedEvent(string countryCode)
		{
			if (ProductCountryCodeFetched != null)
			{
				Debugger.Log("Event ProductCountryCodeFetched sent", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.MARKET);
				ProductCountryCodeFetched(countryCode);
			}
		}
	}
}