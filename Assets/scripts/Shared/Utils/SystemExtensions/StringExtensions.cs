using UnityEngine;
using System.Security.Cryptography;
using System.Text;

namespace Utils
{

	static class StringExtensions
	{
		public static string[] GetWords(this string text, bool includeSpecialChars = false)
		{
			string regex = includeSpecialChars ? "[\\w'-]+" : @"\w+";

			System.Text.RegularExpressions.MatchCollection matchCollection = System.Text.RegularExpressions.Regex.Matches(text, regex);

			string[] matches = null;
			if (matchCollection.Count > 0)
			{
				matches = new string[matchCollection.Count];
				for (var i = 0; i < matchCollection.Count; i++)
				{
					matches[i] = matchCollection[i].ToString();
				}
			}

			return matches;
		}

		public static string RemoveSpecialCharacters(this string text)
		{
			string toText = System.Text.RegularExpressions.Regex.Replace(text, "[^\\w\\s]", "");
			toText = System.Text.RegularExpressions.Regex.Replace(toText, "_", "");
			return toText;
		}

		public static string ReplaceSpecialCharacters(this string text, string replacement = "_")
		{
			string toText = System.Text.RegularExpressions.Regex.Replace(text, "[^\\w\\s]", replacement);
			return toText;
		}

		public static bool ContainsAToZCharacter(this string text)
		{
			int letterCount = System.Text.RegularExpressions.Regex.Matches(text, @"[a-zA-Z]").Count;
			return letterCount > 0;
		}

		public static string RemoveNonAToZ(this string text)
		{
			string toText = System.Text.RegularExpressions.Regex.Replace(text, "[^a-zA-Z\\s]", "");
			return toText;
		}

		public static string CalculateMD5Hash(this string input)
		{
			// step 1, calculate MD5 hash from input
			MD5 md5 = System.Security.Cryptography.MD5.Create();
			byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			// step 2, convert byte array to hex string
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}
			return sb.ToString();
		}

		public static string Ordinal(this string input, int num)
		{
			if (num <= 0)
			{
				return num.ToString();
			}

/*			switch (num % 100)
			{
			case 11:
			case 12:
			case 13:
//				return TextManager.Get("Ordinal.th");
			}

			switch (num % 10)
			{
			case 1:
//				return TextManager.Get("Ordinal.st");
			case 2:
//				return TextManager.Get("Ordinal.nd");
			case 3:
//				return TextManager.Get("Ordinal.rd");
			default:
//				return TextManager.Get("Ordinal.th");
			}*/
			return null;
		}

		public static bool ContainsSurrogatePair(this string obj)
		{
			bool ret = false;

			foreach (char element in obj)
			{
				if (char.IsHighSurrogate(element))
				{
					ret = true;
					break;
				}
			}

			return ret;
		}
	}
}