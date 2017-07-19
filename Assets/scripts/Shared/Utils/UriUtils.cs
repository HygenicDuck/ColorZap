using System;
using System.Collections.Generic;
using System.Collections;

namespace Utils
{
	public static class UriUtils
	{
		public static Hashtable DecodeQueryParameters(string uri)
		{
			if (uri == null || uri.Length == 0)
			{
				return new Hashtable ();
			}

			Hashtable table = new Hashtable();

			int paramsStart = uri.IndexOf('?') + 1;

			string getOptions = uri.Substring(paramsStart);

			string[] parameters = getOptions.Split(new[] { '&', ';' }, StringSplitOptions.RemoveEmptyEntries);

			Array.ForEach(parameters, parameter => {

				string[] pair = parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				table.Add(pair[0], pair[1]);

			});

			return table;
		}
	}
}

