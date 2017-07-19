using UnityEngine;
using System.Collections;
using Kanga;
using System;
//using WaffleIronObjects;
using KangaRequests;

namespace KangaRequests
{
	public class Economy
	{
		public static void GetBank(BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback, Action<Kanga.ResponseObjectHandler> cacheCallback = null)
		{
			Request request = new Request {
				method = "GET",
				endpoint = "bank",
				cacheToDisk = true
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, cacheCallback, requestOptions.cacheTimeOut);
		}

		public static void GetInventoryItems(BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback, Action<Kanga.ResponseObjectHandler> cacheCallback = null)
		{
			Request request = new Request {
				method = "GET",
				endpoint = "inventory-items"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, cacheCallback, requestOptions.cacheTimeOut);
		}

		public static void GetInventory(BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback, Action<Kanga.ResponseObjectHandler> cacheCallback = null)
		{
			Request request = new Request {
				method = "GET",
				endpoint = "inventory"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, cacheCallback, requestOptions.cacheTimeOut);
		}

		public static void GetProducts(BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback, Action<Kanga.ResponseObjectHandler> cacheCallback = null)
		{
			Request request = new Request {
				method = "GET",
				endpoint = "products"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

		public static void GetPeriodicReplenish(string itemCode, BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "inventory",
				uriObject = "periodic-replenish",
				action = itemCode
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}


		public static void CollectPeriodicReplenish(string itemCode, BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "inventory",
				uriObject = "periodic-replenish",
				action = itemCode + "/collect"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}
			
		public static void RequestPurchase(string productCode, BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "products",
				uriObject = productCode,
				action = "purchase"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

		public class MultiPurchaseOptions : BaseOptions
		{
			private string[] m_porductList;
			private string m_inventoryGroupId = null;

			public MultiPurchaseOptions(string[] productList)
			{
				m_porductList = productList;
			}

			public MultiPurchaseOptions(string[] productList, string inventoryGroupId)
			{
				m_porductList = productList;
				m_inventoryGroupId = inventoryGroupId;
			}

			public override void UpdateArgs(Hashtable args)
			{
				base.UpdateArgs(args);
				args.Add("product_ids", m_porductList);
				if (!string.IsNullOrEmpty(m_inventoryGroupId))
				{
					args.Add("inventory_group_id", m_inventoryGroupId);
				}
			}
		}

		public static void RequestMultiPurchase(MultiPurchaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "products",
				action = "soft-currency-multi-purchase"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

		public class PurchaseAndConsumeOptions : BaseOptions
		{
			private string m_productCode;
			private string m_itemCode;
			private int m_quantity;


			public PurchaseAndConsumeOptions(string productCode, string itemCode, int quantity)
			{
				m_productCode = productCode;
				m_itemCode = itemCode;
				m_quantity = quantity;
			}

			public override void UpdateArgs(Hashtable args)
			{
				base.UpdateArgs(args);

				Hashtable operation = new Hashtable ();

				operation.Add("item_code", m_itemCode);
				operation.Add("consume", m_quantity);

				Hashtable[] operations = { operation }; 

				args.Add("product_id", m_productCode);
				args.Add("operations", operations);
			}
		}

		public static void RequestPurchaseAndConsume(PurchaseAndConsumeOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "inventory",
				action = "purchase-consume"
			};
					
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

		public class ConsumeOptions : BaseOptions
		{
			private string m_itemCode;
			private int m_quantity;

			public ConsumeOptions(string itemCode, int quantity)
			{
				m_itemCode = itemCode;
				m_quantity = quantity;
			}

			public override void UpdateArgs(Hashtable args)
			{
				base.UpdateArgs(args);

				Hashtable operation = new Hashtable ();

				operation.Add("avatar_id", 1);
				operation.Add("item_code", m_itemCode);
				operation.Add("consume", m_quantity);

				Hashtable[] operations = { operation }; 
			
				args.Add("operations", operations);
			}
		}

		public static void RequestConsumeItem(ConsumeOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "inventory",
				action = "consume"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}


		public static void RequestConsumeSpin(BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "inventory",
				action = "spend-spin"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

		public class AppStorePurchaseOptions : BaseOptions
		{
			private string m_productCode;
			private string m_receipt;
			private string m_userID;
			private string m_priceCharged;
			private string m_currencyCode;

			public string ProductCode { get { return m_productCode; } }

			public AppStorePurchaseOptions(string code, string receipt, string priceCharged, string currencyCode, string userID)
			{
				m_productCode = code;
				m_receipt = receipt;
				m_priceCharged = priceCharged;
				m_currencyCode = currencyCode;
				m_userID = userID;
			}

			public override void UpdateArgs(Hashtable args)
			{
				base.UpdateArgs(args);
#if UNITY_ANDROID
				#if ANDROID_AMAZON
				args.Add("amazon_receipt", m_receipt);
				args.Add("amazon_user_id", m_userID);
				#else
				args.Add("google_play_receipt", m_receipt);
				#endif
#else
				args.Add("apple_receipt", m_receipt);
#endif
				args.Add("price_charged", m_priceCharged);
				args.Add("currency_code", m_currencyCode);
			}
		}

		public static void RequestAppStorePurchase(AppStorePurchaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "POST",
				endpoint = "products",
				uriObject = requestOptions.ProductCode,
				action = "purchase"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

		public static void GetLevelUnlocks(BaseOptions requestOptions, Action<Hashtable> errorCallback, Action<Kanga.ResponseObjectHandler> successCallback)
		{
			Request request = new Request {
				method = "GET",
				endpoint = "levelling-rewards"
			};
			requestOptions.UpdateArgs(request.requestData.args);

			string groupId = requestOptions.requestGroupId;
			Kanga.Server.Instance.QueueRequest(request, groupId, errorCallback, successCallback, null, requestOptions.cacheTimeOut);
		}

	}
}
