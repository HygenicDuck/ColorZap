using UnityEngine;
using System.Collections;
using Kanga;
//using WaffleIronObjects;
//using WaffleRequests;
using Utils;
using KangaRequests;


public class BankServer: BaseServer
{
	public interface IDelegate
	{
		void GetBankSuccess(KangaObjects.Bank bank);
		void SetBankObjectHandler(ResponseObjectHandler objHandler);
	}

	private IDelegate m_delegate;

	public BankServer(IDelegate myDelegate) : base(myDelegate as Core.BaseBehaviour)
	{
		m_delegate = myDelegate;
	}

	public void GetBank(bool useCacheCallback = false, int cacheTimeout = Utils.TimeUtils.SECOND * 5)
	{
		BaseOptions options = new BaseOptions ();
		options.cacheTimeOut = cacheTimeout;

		if( useCacheCallback )
		{
			Economy.GetBank(options, BankFailCallback, BankSuccessCallback, BankSuccessCallback);
		}
		else
		{
			Economy.GetBank(options, BankFailCallback, BankSuccessCallback);
		}
	}
		
	private void BankSuccessCallback(ResponseObjectHandler objHandler)
	{
//		Debugger.Log("BankSuccessCallback", (int)WaffleSystems.Systems.ECONOMY);

		KangaObjects.Bank bank = objHandler.GetHelperObject("bank") as KangaObjects.Bank;
		if (bank != null)
		{
			Kanga.Server.Instance.SetCacheItemImportant(bank.objectID, true);
//			AdsManager.Instance.SetLastPurchaseTime(bank.lastCashPurchaseTransactionDate);
			m_delegate.GetBankSuccess(bank);
		}

		m_delegate.SetBankObjectHandler(objHandler);
	}

	private void BankFailCallback(Hashtable errorTable)
	{
//		Debugger.Log("BankServer FAIL", Debugger.Severity.MESSAGE, (int)WaffleSystems.Systems.ECONOMY);
//		Debugger.PrintHashTableAsServerObject(errorTable, "Error", (int)WaffleSystems.Systems.ECONOMY);

		m_delegate.SetBankObjectHandler(null);
	}

}

