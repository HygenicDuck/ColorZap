using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Purchasing;
using Kanga;
using System.Collections;
using Utils;
using KangaRequests;
using KangaObjects;

namespace Market
{
//	public class ProductManager : MonoSingleton<ProductManager>, AppStore.IDelegate, EconomyServer.IDelegate, BankServer.IDelegate
//	{
//		private const string BANK_OBJECT_ID = "bank";
//		protected const float RETRY_REQUEST_DELAY = 3f;
//
//		private const string CURRENCY_NAME_GEM = "currency_gem";
//		private const string CURRENCY_NAME_FBGHOST = "currency_facebook_ghost";
//		private const string CURRENCY_NAME_TWGHOST = "currency_twitter_ghost";
//		private const string CURRENCY_NAME_COIN = "currency_coin";
//
//		public enum PurchaseResult
//		{
//			SUCCESS,
//			CANNOT_AFFORD,
//			ERROR
//		}
//
//		private AppStore m_appStore;
//		private List<KangaObjects.Product> m_kangaProducts = new List<KangaObjects.Product> ();
//		private EconomyServer m_economyServer;
//		private BankServer m_bankServer;
//		private ResponseObjectHandler m_bankObjHandler;
//		private Action<PurchaseResult> m_finishedCallback;
//		private KangaObjects.Product m_purchaseInProgress;
//
//		private bool m_kangaProductsReady;
//
//		public List<KangaObjects.Product> KangaProducts { get { return m_kangaProducts; } }
//		public bool KangProductsReady { get { return m_kangaProductsReady; } }
//
//		protected override void Init()
//		{
//			m_appStore = new AppStore ();
//			m_appStore.Initialise(this);
//
//			m_economyServer = new EconomyServer (this);
//			m_bankServer = new BankServer (this);
//		}
//
//
//		public int GetProductCost(string productID)
//		{
//			KangaObjects.Product product = GetProduct(productID);
//			if (product != null)
//			{
//				return product.GetCost();
//			}
//			return 0;
//		}
//
//		public bool CanAffordProducts(string[] productIDs)
//		{
//			int costInGems = 0;
//			int costInCoins = 0;
//			Bank.Balances balances = GetBankBalance();
//
//			if (balances == null)
//			{
//				return false;
//			}
//
//			for (int i = 0; i < productIDs.Length; ++i)
//			{
//				KangaObjects.Product product = GetProduct(productIDs[i]);
//
//				if (product != null && !product.IsAppStoreProduct())
//				{
//					int productCost = product.GetCost();
//					string costCurrency = product.GetCostCurrency();
//
//					if (costCurrency == CURRENCY_NAME_GEM)
//					{
//						costInGems += productCost;
//					}
//					else if (costCurrency == CURRENCY_NAME_FBGHOST)
//					{
//
//					}
//					else if (costCurrency == CURRENCY_NAME_TWGHOST)
//					{
//
//					}
//					else
//					{
//						costInCoins += productCost;
//					}
//
//				}
//			}
//
//			return balances.gems >= costInGems && balances.coins >= costInCoins;
//		}
//
//		public bool CanAffordProduct(string productID)
//		{
//			bool ret = false;
//			KangaObjects.Product product = GetProduct(productID);
//			if (product != null)
//			{
//				if (!product.IsAppStoreProduct())
//				{
//					int productCost = product.GetCost();
//					string costCurrency = product.GetCostCurrency();
//
//					Bank.Balances balances = GetBankBalance();
//
//
//					if (balances != null)
//					{
//						if (costCurrency == CURRENCY_NAME_GEM)
//						{
//							ret = balances.gems >= productCost;
//						}
//						else if (costCurrency == CURRENCY_NAME_FBGHOST)
//						{
//							ret = balances.facebookGhost >= productCost;
//						}
//						else if (costCurrency == CURRENCY_NAME_TWGHOST)
//						{
//							ret = balances.twitterGhost >= productCost;
//						}
//						else
//						{
//							ret = balances.coins >= productCost;
//						}
//					}
//				}
//			}
//
//			return ret;
//		}
//
//		public Bank.Balances GetBankBalance()
//		{
//			Bank.Balances ret = null;
//
//			KangaObjects.Bank bank = m_bankObjHandler.GetHelperObject(BANK_OBJECT_ID) as KangaObjects.Bank;
//			if (bank != null)
//			{
//				ret = bank.balances;
//			}
//
//			Utils.Debugger.Assert(ret != null, "GetBankBalance returning null in Product Manager");
//
//			return ret;
//		}
//
//		public virtual void FetchProducts()
//		{
//			BaseOptions options = new BaseOptions ();
//			Economy.GetProducts(options, FetchKangaProductsFailed, FetchKangaProductsSuccess, FetchKangaProductsSuccess);
//
//			m_bankServer.GetBank();
//		}
//
//		private void FetchKangaProductsFailed(Hashtable error)
//		{
//			StartCoroutine(RetryFetchProducts());
//		}
//
//		private IEnumerator RetryFetchProducts()
//		{
//			yield return new WaitForSeconds (RETRY_REQUEST_DELAY);
//			FetchProducts();
//		}
//
//		private IEnumerator RetryBank()
//		{
//			yield return new WaitForSeconds (RETRY_REQUEST_DELAY);
//			m_bankServer.GetBank();
//		}
//
//		private void FetchKangaProductsSuccess(ResponseObjectHandler objHandler)
//		{
//			Utils.Debugger.Log("FetchKangaProductsSuccess ", (int)SharedSystems.Systems.MARKET);
//
//			if (!m_kangaProductsReady)
//			{
//				m_kangaProducts.Clear();
//
//				List<string> appStoreProductsToFetch = new List<string> ();
//
//				int numHits = objHandler.GetNumHits();
//				for (int i = 0; i < numHits; i++)
//				{
//					Utils.Debugger.Log("FetchKangaProductsSuccess2", (int)SharedSystems.Systems.MARKET);
//
//					KangaObjects.Product product = objHandler.GetHitFromIndex<KangaObjects.Product>(i);
//					if (product != null)
//					{
//						product.Print("Kanga product: " + product.code + " ", (int)SharedSystems.Systems.MARKET);
//
//						if (product.requiresStoreReceipt)
//						{
//							appStoreProductsToFetch.Add(product.code);
//						}
//
//						m_kangaProducts.Add(product);
//					}
//				}
//
//				m_kangaProductsReady = true;
//
//				Utils.MarketEvents.SendProductsFetchedEvent();
//
//				m_appStore.FetchProducts(appStoreProductsToFetch);
//			}
//		}
//
//		public void BuyProduct(string productID, Action<PurchaseResult> finishedCallback)
//		{
//			Utils.Debugger.Log("BuyProduct " + productID, (int)SharedSystems.Systems.MARKET);
//
//			m_finishedCallback = finishedCallback;
//
//			m_purchaseInProgress = GetProduct(productID);
//
//			if (m_purchaseInProgress != null)
//			{
//
//				if (m_purchaseInProgress.IsAppStoreProduct())
//				{
//					m_appStore.BuyProductID(productID);
//				}
//				else
//				{
//					if (CanAffordProduct(m_purchaseInProgress.code))
//					{
//						m_economyServer.Purchase(m_purchaseInProgress.code);
//					}
//					else
//					{
//						Utils.Debugger.Log("Cannot afford product " + productID, (int)SharedSystems.Systems.MARKET);
//
//						Finish(PurchaseResult.CANNOT_AFFORD);
//					}
//				}
//			}
//		}
//
//		public void BuyMultipleProducts(string[] productIDs, Action<PurchaseResult> finishedCallback, string inventoryGroupId = null)
//		{
//			Utils.Debugger.Log("Buy Multiple Products ", (int)SharedSystems.Systems.MARKET);
//
//			m_finishedCallback = finishedCallback;
//			m_purchaseInProgress = GetProduct(productIDs[0]);
//
//			if (CanAffordProducts(productIDs))
//			{
//				m_economyServer.MultiPurchase(productIDs, inventoryGroupId);
//			}
//			else
//			{
//				Utils.Debugger.Log("Cannot afford multiple products ", (int)SharedSystems.Systems.MARKET);
//
//				Finish(PurchaseResult.CANNOT_AFFORD);
//			}
//				
//
//		}
//
//		public void BuyAndConsume(string productID, string inventoryID, int quantity, Action<PurchaseResult> finishedCallback = null)
//		{
//			m_finishedCallback = finishedCallback;
//
//			if (CanAffordProduct(productID))
//			{
//				m_purchaseInProgress = GetProduct(productID);
//
//				m_economyServer.PurchaseAndConsume(productID, inventoryID, quantity);	
//			}
//			else
//			{
//				Utils.Debugger.Log("Cannot afford product " + productID, (int)SharedSystems.Systems.MARKET);
//
//				Finish(PurchaseResult.CANNOT_AFFORD);
//			}
//		}
//
//		public void Consume(string inventoryID, int quantity)
//		{
//			m_economyServer.Consume(inventoryID, quantity);
//		}
//
//		public KangaObjects.Product GetProduct(string productCode)
//		{
//			KangaObjects.Product ret = null;
//
//			if (m_kangaProducts != null)
//			{
//				ret = m_kangaProducts.Find(item => item.code == productCode);
//			}
//
//			return ret;
//		}
//
//		public List<KangaObjects.Product> GetProductsForCategory(string categoryString)
//		{
//			return m_kangaProducts.FindAll(item => item.category == categoryString);
//		}
//
//		private void Finish(PurchaseResult result)
//		{
//			if (m_finishedCallback != null)
//			{
//				m_finishedCallback(result);
//			}	
//
//			m_finishedCallback = null;
//		}
//
//
//		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//		//
//		// EconomyServer.IDelegate
//		//
//		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//		public void PurchaseSuccess()
//		{
//			Finish(PurchaseResult.SUCCESS);
//
//			Utils.MarketEvents.SendProductPurchasedEvent(m_purchaseInProgress);
//
//			m_purchaseInProgress = null;
//		}
//
//		public void PurchaseFail()
//		{
//			m_purchaseInProgress = null;
//
//			Finish(PurchaseResult.ERROR);
//		}
//
//		public void ConsumeSuccess()
//		{
//
//		}
//
//		public void ConsumeFail()
//		{
//
//		}
//
//		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//		//
//		// BankServer.IDelegate
//		//
//		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//		public void GetBankSuccess(KangaObjects.Bank bank)
//		{			
//		}
//
//		public void SetBankObjectHandler(ResponseObjectHandler objHandler)
//		{
//			if (objHandler != null)
//			{				
//				m_bankObjHandler = objHandler;
//				m_bankObjHandler.SetImportant(true);	
//			}
//			else
//			{
//				StartCoroutine(RetryBank());
//			}
//		}
//
//		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//		//
//		// AppStore.IDelegate
//		//
//		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//		public void OnFetchedAppStoreProducts(ProductCollection appStoreProducts)
//		{		
//			//	iterate m_kangaProducts and find any matching appStoreProducts and setup localizedPrice 
//
//			Debugger.Log("Found app store products", (int)SharedSystems.Systems.MARKET);
//
//			for (int i = 0; i < appStoreProducts.all.Length; i++)
//			{
//				UnityEngine.Purchasing.Product appStoreProduct = appStoreProducts.all[i];
//
//				Debugger.Log("id: " + appStoreProduct.definition.id, (int)SharedSystems.Systems.MARKET);
//
//				if (appStoreProduct.availableToPurchase)
//				{
//					KangaObjects.Product kangaProduct = m_kangaProducts.Find(item => item.code == appStoreProduct.definition.id);
//					if (kangaProduct != null)
//					{
//						kangaProduct.localizedPrice = appStoreProduct.metadata.localizedPriceString;
//						kangaProduct.currencyCode = appStoreProduct.metadata.isoCurrencyCode;
//					}
//				}
//			}
//		}
//
//		public void OnAppStorePurchasedComplete(UnityEngine.Purchasing.Product product)
//		{
//			#if SERVER_ENVIROMENT_CANDIDATE && !CANDIDATE_DEBUG
//			WaffleAppsFlyer.Instance.ProductPurchased(product.metadata.localizedPriceString, product.metadata.isoCurrencyCode, product.metadata.localizedTitle);
//			#endif
//
//			m_economyServer.PurchaseAppStoreProduct(product);
//		}
//
//		public void OnAppStorePurchasedFailed()
//		{
//			Finish(PurchaseResult.ERROR);
//		}
//	}
}