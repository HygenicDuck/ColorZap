using UnityEngine;
using System.Collections;
using Core;
using System.Collections.Generic;
using System;

namespace KangaObjects
{
	public static class ProductExtensions
	{
		public static bool IsAppStoreProduct(this Product obj)
		{
			return !string.IsNullOrEmpty(obj.localizedPrice);
		}

		public static int GetCost(this Product obj)
		{
			int ret = -1;

			if (obj.costs != null && obj.costs.Length > 0)
			{
				ret = obj.costs[0].value;
			}

			return ret;
		}

		public static string GetCostCurrency(this Product obj)
		{
			string ret = "";

			if (obj.costs != null && obj.costs.Length > 0)
			{
				ret = obj.costs[0].currencyID;
			}

			return ret;
		}

		public static bool SideEffectsContainItem(this Product obj, string itemName)
		{
			bool ret = false;

			if (obj.sideEffects != null && !string.IsNullOrEmpty(itemName))
			{
				for (int i = 0; i < obj.sideEffects.Length; i++)
				{
					Product.SideEffect sideEffect = obj.sideEffects[i];
					if (sideEffect != null && sideEffect.name.ToLower() == itemName)
					{
						ret = true;
						break;
					}
				}
			}

			return ret;
		}

		public static List<string> GetInventoryItemObjectIDs(this Product obj)
		{
			List<string> ret = new List<string> ();

			if (obj.sideEffects != null)
			{
				for (int i = 0; i < obj.sideEffects.Length; i++)
				{
					Product.SideEffect sideEffect = obj.sideEffects[i];
					if (sideEffect != null)
					{
						ret.Add(sideEffect.objectID);
					}
				}
			}

			return ret;
		}

		public static List<string> GetInventoryItemCodes(this Product obj)
		{
			List<string> ret = new List<string> ();

			if (obj.sideEffects != null)
			{
				for (int i = 0; i < obj.sideEffects.Length; i++)
				{
					Product.SideEffect sideEffect = obj.sideEffects[i];
					if (sideEffect != null)
					{
						ret.Add(sideEffect.objectID.Replace("inventory_item_", ""));
					}
				}
			}

			return ret;
		}


		public static int GetInventoryQuantity(this Product obj, string inventoryCode)
		{
			int ret = 0;

			if (obj.sideEffects != null)
			{
				for (int i = 0; i < obj.sideEffects.Length; i++)
				{					
					Product.SideEffect sideEffect = obj.sideEffects[i];

					if (sideEffect != null && (sideEffect.objectID.Replace("inventory_item_", "") == inventoryCode))
					{
						ret += sideEffect.quantity;
					}
				}
			}

			return ret;
		}

		public static int GetCurrencyPackQuantity(this Product obj, string currencyCode)
		{
			int ret = 0;

			if (obj.sideEffects != null)
			{
				for (int i = 0; i < obj.sideEffects.Length; i++)
				{
					Product.SideEffect sideEffect = obj.sideEffects[i];

					if (sideEffect != null && sideEffect.objectType == currencyCode)
					{
						ret += sideEffect.quantity;
					}
				}
			}

			return ret;
		}
	}
}