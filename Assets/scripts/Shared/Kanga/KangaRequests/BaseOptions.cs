using UnityEngine;
using System;
using System.Collections;
using Kanga;

namespace KangaRequests
{
	public class BaseOptions
	{
		public int limit = 1000;
		public int hitsSize = -1;
		public int orderSize = -1;
		public string sortBy = "new";
		public string filterBy = null;
		public string queryText = null;
		public int cacheTimeOut = 0;
		public string requestGroupId = null;
		public bool ditchRequestIfCanceled = false;

		public virtual void UpdateArgs(Hashtable args)
		{
			args.Add("limit", limit);

			if (sortBy != null)
			{
				args.Add("sort_by", sortBy);
			}

			if (hitsSize >= 0)
			{
				args.Add("hits_size", hitsSize);
			}

			if (orderSize >= 0)
			{
				args.Add("order_size", orderSize);
			}

			if (queryText != null)
			{
				args.Add("query_text", queryText);
			}

			if (filterBy != null)
			{
				args.Add("filter_by", filterBy);
			}
		}
	}
}
