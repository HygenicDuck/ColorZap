using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Purchasing;

namespace Market
{
//	public class AppStore : IStoreListener
//	{
//		public interface IDelegate
//		{
//			void OnFetchedAppStoreProducts(ProductCollection products);
//			void OnAppStorePurchasedComplete(Product product);
//			void OnAppStorePurchasedFailed();
//		}
//
//		// The Unity Purchasing system.
//		private static IStoreController m_StoreController;
//
//		// The store-specific Purchasing subsystems.
//		private static IExtensionProvider m_StoreExtensionProvider;
//
//		private IDelegate m_delegate;
//
//		public void Initialise(IDelegate iDelegate)
//		{
//			m_delegate = iDelegate;
//		}
//
//		public void FetchProducts(List<string> productIDs)
//		{
//			if (IsInitialized())
//			{
//				m_delegate.OnFetchedAppStoreProducts(m_StoreController.products);
//				return;
//			}
//
//			// Create a builder, first passing in a suite of Unity provided stores.
////			ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
//
//			foreach( string productID in productIDs)
//			{
//				Utils.Debugger.Log("AppStore Purchasing product asychronously: " +  productID, (int)SharedSystems.Systems.MARKET);
//
////				builder.AddProduct(productID, ProductType.Consumable);
//			}
//
//			#if ANDROID_AMAZON && !SERVER_ENVIROMENT_CANDIDATE
//			builder.Configure<IAmazonConfiguration>().WriteSandboxJSON(builder.products);
//			#endif
//
//			// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
//			// and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
////			UnityPurchasing.Initialize(this, builder);
//		}
//
//
//		public bool IsInitialized()
//		{
//			// Only say we are initialized if both the Purchasing references are set.
//			return m_StoreController != null && m_StoreExtensionProvider != null;
//		}
//
//
//		public void BuyProductID(string productId)
//		{
//			// If Purchasing has been initialized ...
//			if (IsInitialized())
//			{
//				// ... look up the Product reference with the general product identifier and the Purchasing 
//				// system's products collection.
//				Product product = m_StoreController.products.WithID(productId);
//
//				// If the look up found a product for this device's store and that product is ready to be sold ... 
//				if (product != null && product.availableToPurchase)
//				{
//					Utils.Debugger.Log(string.Format("AppStore Purchasing product asychronously: '{0}'", product.definition.id), (int)SharedSystems.Systems.MARKET);
//					// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
//					// asynchronously.
//					m_StoreController.InitiatePurchase(product);
//				}
//				// Otherwise ...
//				else
//				{
//					// ... report the product look-up failure situation  
//					Utils.Debugger.Log("AppStore BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase", (int)SharedSystems.Systems.MARKET);
//				}
//			}
//			// Otherwise ...
//			else
//			{
//				// ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
//				// retrying initiailization.
//				Utils.Debugger.Log("AppStore BuyProductID FAIL. Not initialized.", (int)SharedSystems.Systems.MARKET);
//			}
//		}
//
//
//		// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
//		// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
//		public void RestorePurchases()
//		{
//			// If Purchasing has not yet been set up ...
//			if (!IsInitialized())
//			{
//				// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
//				Utils.Debugger.Log("RestorePurchases FAIL. Not initialized.", (int)SharedSystems.Systems.MARKET);
//				return;
//			}
//
//			// If we are running on an Apple device ... 
//			if (Application.platform == RuntimePlatform.IPhonePlayer ||
//			    Application.platform == RuntimePlatform.OSXPlayer)
//			{
//				// ... begin restoring purchases
//				Utils.Debugger.Log("AppStore RestorePurchases started ...");
//
//				// Fetch the Apple store-specific subsystem.
////				IAppleExtensions apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
//				// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
//				// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
////				apple.RestoreTransactions((result) => {
////					// The first phase of restoration. If no more responses are received on ProcessPurchase then 
////					// no purchases are available to be restored.
////					Utils.Debugger.Log("AppStore RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.", (int)SharedSystems.Systems.MARKET);
////				});
//			}
//			// Otherwise ...
//			else
//			{
//				// We are not running on an Apple device. No work is necessary to restore purchases.
//				Utils.Debugger.Log("AppStore RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform, (int)SharedSystems.Systems.MARKET);
//			}
//		}
//
//
//		//
//		// --- IStoreListener
//		//
//
//		public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//		{
//			// Purchasing has succeeded initializing. Collect our Purchasing references.
//			Utils.Debugger.Log("AppStore OnInitialized: PASS");
//
//			// Overall Purchasing system, configured with products for this application.
//			m_StoreController = controller;
//			// Store specific subsystem, for accessing device-specific store features.
//			m_StoreExtensionProvider = extensions;
//
//			//TODO: we probably need this for reciept validation, so pass through in delegate?
//			//string amazonUserId = extensions.GetExtension<IAmazonExtensions>().amazonUserId;
//
//			m_delegate.OnFetchedAppStoreProducts(m_StoreController.products);
//		}
//
//
//		public void OnInitializeFailed(InitializationFailureReason error)
//		{
//			// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
//			Utils.Debugger.Log("AppStore OnInitializeFailed InitializationFailureReason:" + error, (int)SharedSystems.Systems.MARKET);
//		}
//
//
//		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
//		{
//			m_delegate.OnAppStorePurchasedComplete(args.purchasedProduct);
//
//			// Return a flag indicating whether this product has completely been received, or if the application needs 
//			// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
//			// saving purchased products to the cloud, and when that save is delayed. 
//			return PurchaseProcessingResult.Complete;
//		}
//
//
//		public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//		{
//			// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
//			// this reason with the user to guide their troubleshooting actions.
//			Utils.Debugger.Log(string.Format("AppStore OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason), (int)SharedSystems.Systems.MARKET);
//
//			m_delegate.OnAppStorePurchasedFailed();
//		}
//	}
}