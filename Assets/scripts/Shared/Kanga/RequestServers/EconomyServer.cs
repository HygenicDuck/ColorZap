using UnityEngine;
using System.Collections;
using Kanga;
//using WaffleIronObjects;
//using WaffleRequests;
using Utils;
using KangaRequests;


public class EconomyServer: BaseServer
{
	public interface IDelegate
	{
		void PurchaseSuccess();
		void PurchaseFail();
		void ConsumeSuccess();
		void ConsumeFail();
	}

	private IDelegate m_delegate;

	public EconomyServer(IDelegate myDelegate) : base(myDelegate as Core.BaseBehaviour)
	{
		m_delegate = myDelegate;
	}

	private void PurchaseAndConsumeSuccessCallback(ResponseObjectHandler objHandler)
	{
//		Debugger.Log("PurchaseAndConsume SUCCESS", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);

		m_delegate.PurchaseSuccess();
	}

	private void PurchaseAndConsumeFailCallback(Hashtable errorTable)
	{
//		Debugger.Log("PurchaseAndConsume FAIL", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);
//		Debugger.PrintHashTableAsServerObject(errorTable, "Error", (int)WaffleSystems.Systems.ECONOMY);

		m_delegate.PurchaseFail();
	}

	public void PurchaseAndConsume(string productCode, string itemCode, int quantity)
	{
		Economy.PurchaseAndConsumeOptions options = new Economy.PurchaseAndConsumeOptions (productCode, itemCode, quantity);
		options.requestGroupId = GetRequestGroup();

		Economy.RequestPurchaseAndConsume(options, PurchaseAndConsumeFailCallback, PurchaseAndConsumeSuccessCallback);
	}

	private void PurchaseSuccessCallback(ResponseObjectHandler objHandler)
	{
//		Debugger.Log("PurchaseSuccessCallback SUCCESS", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);

		m_delegate.PurchaseSuccess();
	}

	private void PurchaseFailCallback(Hashtable errorTable)
	{
//		Debugger.Log("PurchaseFailCallback SUCCESS", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);
//		Debugger.PrintHashTableAsServerObject(errorTable, "Error", (int)WaffleSystems.Systems.ECONOMY);

		m_delegate.PurchaseFail();
	}

	public void Purchase(string productCode)
	{
		BaseOptions options = new BaseOptions ();
		options.requestGroupId = GetRequestGroup();

		Economy.RequestPurchase(productCode, options, PurchaseFailCallback, PurchaseSuccessCallback);
	}

	public void MultiPurchase(string[] productList, string inventoryGroupId = null)
	{
		Economy.MultiPurchaseOptions options = new Economy.MultiPurchaseOptions (productList, inventoryGroupId);
		options.requestGroupId = GetRequestGroup();

		Economy.RequestMultiPurchase(options, PurchaseFailCallback, PurchaseSuccessCallback);
	}

	private void ConsumeSuccessCallback(ResponseObjectHandler objHandler)
	{
//		Debugger.Log("ConsumeSuccessCallback SUCCESS", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);

		m_delegate.ConsumeSuccess();
	}

	private void ConsumeFailCallback(Hashtable errorTable)
	{
//		Debugger.Log("ConsumeFailCallback SUCCESS", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);
//		Debugger.PrintHashTableAsServerObject(errorTable, "Error", (int)WaffleSystems.Systems.ECONOMY);
	}

	public void Consume(string itemCode, int quantity)
	{
		Economy.ConsumeOptions options = new Economy.ConsumeOptions (itemCode, quantity);
		options.requestGroupId = GetRequestGroup();

		Economy.RequestConsumeItem(options, ConsumeFailCallback, ConsumeSuccessCallback);
	}


//	public void PurchaseAppStoreProduct(UnityEngine.Purchasing.Product product)
//	{
//		string receiptPayload = "";
//		string userID = "";
//		Hashtable receiptDict = (Hashtable)JSON.JsonDecode(product.receipt);
//
//		Debugger.PrintHashTableAsServerObject(receiptDict, "receiptDict", (int)SharedSystems.Systems.MARKET);
//
//		#if ANDROID_AMAZON
//		if (receiptDict.ContainsKey("Payload"))
//		{
//			Hashtable receiptSubDict = (Hashtable)JSON.JsonDecode((string)receiptDict["Payload"]);
//
//			if (receiptSubDict.ContainsKey("receiptId"))
//			{
//				receiptPayload = (string)receiptSubDict["receiptId"];
//				Utils.Debugger.Log("PurchaseAppStoreProduct receiptPayload " + receiptPayload, (int)SharedSystems.Systems.MARKET);
//			}
//
//			if (receiptSubDict.ContainsKey("userId"))
//			{
//				userID = (string)receiptSubDict["userId"];
//				Utils.Debugger.Log("PurchaseAppStoreProduct userID " + userID, (int)SharedSystems.Systems.MARKET);
//			}
//		}
//		#else
//		if (receiptDict.ContainsKey("Payload"))
//		{
//			receiptPayload = (string)receiptDict["Payload"];
//		}
//		#endif
//
//		Economy.AppStorePurchaseOptions options = new Economy.AppStorePurchaseOptions (product.definition.id, receiptPayload, product.metadata.localizedPrice.ToString(), product.metadata.isoCurrencyCode, userID);
//
//		Utils.Debugger.Log("OnAppStorePurchasedComplete code " + product.definition.id, (int)SharedSystems.Systems.MARKET);
//		Utils.Debugger.Log("OnAppStorePurchasedComplete receipt " + receiptPayload, (int)SharedSystems.Systems.MARKET);
//		Utils.Debugger.Log("OnAppStorePurchasedComplete metadata.localizedPrice.ToString() " + product.metadata.localizedPrice.ToString(), (int)SharedSystems.Systems.MARKET);
//		Utils.Debugger.Log("OnAppStorePurchasedComplete metadata.isoCurrencyCode " + product.metadata.isoCurrencyCode, (int)SharedSystems.Systems.MARKET);
//
//
//		Economy.RequestAppStorePurchase(options, PurchaseAppStoreProductFailCallback, PurchaseAppStoreProductSuccessCallback);
//	}

	private void PurchaseAppStoreProductSuccessCallback(ResponseObjectHandler objHandler)
	{
		Debugger.Log("PurchaseAppStoreProductSuccessCallback SUCCESS", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.MARKET);

		m_delegate.PurchaseSuccess();
	}

	private void PurchaseAppStoreProductFailCallback(Hashtable errorTable)
	{
		Debugger.Log("PurchaseAppStoreProductFailCallback SUCCESS", Debugger.Severity.MESSAGE, (int)SharedSystems.Systems.MARKET);
//		Debugger.PrintHashTableAsServerObject(errorTable, "Error", (int)WaffleSystems.Systems.ECONOMY);
		m_delegate.PurchaseFail();
	}
}

