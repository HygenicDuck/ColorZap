using UnityEngine;
using System.Collections;


namespace KangaObjects
{
	public class Bank: Kanga.ServerObjectDocument
	{
		public class Balances: Kanga.ServerObjectBase
		{
			public int coins;
			public int gems;
			public int facebookGhost;
			public int twitterGhost;

			protected override void AddMappings()
			{
				AddMapping("currency_coin", () => coins);
				AddMapping("currency_gem", () => gems);
				AddMapping("currency_facebook_ghost", () => facebookGhost);
				AddMapping("currency_twitter_ghost", () => twitterGhost);
			}
		}
	
		public Balances balances;
		public string playerId;
		public float purchaseTransactionUsdTotal;
		public long lastPurchaseTransactionDate;
		public long lastCashPurchaseTransactionDate;

		protected override void AddMappings()
		{
			base.AddMappings();

			AddMapping("balances", () => balances);
			AddMapping("player_id", () => playerId);
			AddMapping("purchase_transaction_usd_total", () => purchaseTransactionUsdTotal);
			AddMapping("last_purchase_transaction_date", () => lastPurchaseTransactionDate);
			AddMapping("last_cash_purchase_transaction_date", () => lastCashPurchaseTransactionDate);
		}
	}


	public class BankTransaction: Kanga.ServerObjectDocument
	{
		public int value;
		public string currencyId;

		protected override void AddMappings()
		{
			base.AddMappings();

			AddMapping("value", () => value);
			AddMapping("currency_id", () => currencyId);
		}
	}

	public class Currency: Kanga.ServerObjectDocument
	{
		public string name;
		public string plural;
		public string code;

		protected override void AddMappings()
		{
			base.AddMappings();

			AddMapping("name", () => name);
			AddMapping("plural", () => plural);
			AddMapping("code", () => code);
		}
	}
}
