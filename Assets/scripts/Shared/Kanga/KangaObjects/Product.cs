using UnityEngine;
using System.Collections;
using Core;
using System.Collections.Generic;

namespace KangaObjects
{
	public class Product : Kanga.ServerObjectDocument
	{
		public class Descriptions : Kanga.ServerObjectBase
		{
			public string description1;
			public string description2;
			public string description3;
			public string description4;
			public string bannerText1;
			public string bannerText2;
			public string rosetteText1;
			public string rosetteText2;

			protected override void AddMappings()
			{
				AddMapping("description_1", () => description1);
				AddMapping("description_2", () => description2);
				AddMapping("description_3", () => description3);
				AddMapping("description_4", () => description4);
				AddMapping("banner_text_1", () => bannerText1);
				AddMapping("banner_text_2", () => bannerText2);
				AddMapping("rosette_text_1", () => rosetteText1);
				AddMapping("rosette_text_2", () => rosetteText2);
			}
		}

		public class Cost : Kanga.ServerObjectBase
		{
			public int value;
			public string objectType;
			public string currencyID;
			public string id;

			protected override void AddMappings()
			{
				AddMapping("value", () => value);
				AddMapping("object_type", () => objectType);
				AddMapping("currency_id", () => currencyID);
				AddMapping("id", () => id);
			}
		}

		public class SideEffect : Kanga.ServerObjectBase
		{
			public int quantity;
			public string objectType;
			public string name;
			public string objectID;

			protected override void AddMappings()
			{
				AddMapping("quantity", () => quantity);
				AddMapping("object_type", () => objectType);
				AddMapping("name", () => name);
				AddMapping("object_id", () => objectID);
			}
		}

		//TODO: move this waffle specific data into waffle products
		public class ExtraData : Kanga.ServerObjectBase
		{
			public int colourScheme;
			public bool showBanner;
			public bool showRosette;
			public string bannerText1;
			public string bannerText2;
			public string rosetteText1;
			public string rosetteText2;
			public string backgroundImageURL;
			public bool altTextLayout;
			public bool watchVideo;
			public bool miniShopOnly;

			protected override void AddMappings()
			{
				AddMapping("colour_scheme", () => colourScheme);
				AddMapping("show_banner", () => showBanner);
				AddMapping("show_rosette", () => showRosette);
				AddMapping("banner_text_1", () => bannerText1);
				AddMapping("banner_text_2", () => bannerText2);
				AddMapping("rosette_text_1", () => rosetteText1);
				AddMapping("rosette_text_2", () => rosetteText2);
				AddMapping("background_image_url", () => backgroundImageURL);
				AddMapping("alt_text_layout", () => altTextLayout);
				AddMapping("watch_video", () => watchVideo);
				AddMapping("mini_shop_only", () => miniShopOnly);
			}
		}

		public bool requiresStoreReceipt;
		public bool consumable;
		public bool active;
		public string code;
		public string name;
		public string imageUrl;
		public string currencyCode;
		public string priceCountry;
		public string description;
		public string description2;
		public string description3;
		public string localizedPrice;
		public string category;
		public string subcategory;
		public int order;
		public Cost[] costs;
		public SideEffect[] sideEffects;
		public ExtraData extraData;
		public Descriptions descriptions;
		public bool showInShop;

		protected override void AddMappings()
		{
			base.AddMappings();

			AddMapping("requires_store_receipt", () => requiresStoreReceipt);
			AddMapping("consumable", () => consumable);
			AddMapping("active", () => active);
			AddMapping("code", () => code);
			AddMapping("name", () => name);
			AddMapping("image_url", () => imageUrl);
			AddMapping("currencyCode", () => currencyCode);
			AddMapping("priceCountry", () => priceCountry);
			AddMapping("description", () => description);
			AddMapping("description_2", () => description2);
			AddMapping("description_3", () => description3);
			AddMapping("category", () => category);
			AddMapping("sub_category", () => subcategory);
			AddMapping("order", () => order);
			AddMapping("costs", () => costs);
			AddMapping("side_effects", () => sideEffects);
			AddMapping("extra_data", () => extraData);
			AddMapping("descriptions", () => descriptions);
			AddMapping("show_in_shop", () => showInShop);
		}
	}
}
