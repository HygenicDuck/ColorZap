/*
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
	static class TextExtensions
	{
		static public void TruncateText(this Text obj, string text, bool useEllipsis, int length = -1)
		{
			if( text.ContainsSurrogatePair())
			{
				//	the truncating algorithm below can currently split a surrogate pair which causes errors (and a crash on android)  
				return;
			}

			if (length == -1)
			{
				RectTransform rectTransform = obj.GetRectTransform();
				TextGenerationSettings settings = obj.GetGenerationSettings(rectTransform.rect.size);
				obj.cachedTextGenerator.Populate(text, settings);

				length = obj.cachedTextGenerator.characterCountVisible;
			}

			if (text.Length > length && length > TextManager.ELLIPSIS.Length)
			{
				if (useEllipsis)
				{
					obj.text = text.Substring(0, length - TextManager.ELLIPSIS.Length);

					obj.text += TextManager.ELLIPSIS;
				}
				else
				{
					obj.text = text.Substring(0, length);
				}
			}
			else
			{
				obj.text = text;
			}
		}

		static public void TruncateName(this Text obj, string name, int length, int numLines = 1)
		{
			if (length == -1)
			{
				RectTransform rectTransform = obj.GetRectTransform();
				TextGenerationSettings settings = obj.GetGenerationSettings(rectTransform.rect.size);
				obj.cachedTextGenerator.Populate(name, settings);

				length = obj.cachedTextGenerator.characterCountVisible;
			}

			if (name.Length > length && length != 0)
			{

				if (length <= TextManager.ELLIPSIS.Length)
				{
					Debugger.Error("Text field width is 3 characters or less, is this correct?!");
					return;
				}

				string nameString = name;

				if (numLines == 1)
				{
					while (nameString.Length > length)
					{
						string[] words = nameString.GetWords(true);

						if (words.Length >= 2 && words[words.Length - 1].Length > 1)
						{
							nameString = words[0] + " " + words[words.Length - 1][0];
						}
						else if (words.Length == 2 && words[1].Length == 1)
						{
							nameString = words[0];
						}
						else
						{
							nameString = name.Substring(0, length - TextManager.ELLIPSIS.Length);
							nameString += TextManager.ELLIPSIS;
						}
					}
				}
				else
				{
					string[] words = nameString.GetWords(true);

					nameString = "";

					for (int i = 0; i < words.Length; ++i)
					{
						if (words[i].Length > length)
						{
							nameString += words[i].Substring(0, length - TextManager.ELLIPSIS.Length) + TextManager.ELLIPSIS;
						}
						else
						{
							nameString += words[i];
						}

						if (i != words.Length - 1)
						{
							nameString += " ";
						}
					}

					if (nameString.Length > length * numLines)
					{
						nameString = words[0] + " " + words[words.Length - 1];
					}
				}

				obj.text = nameString;
			}
			else
			{
				obj.text = name;
			}
		}

		static public void TruncateNumber(this Text obj, int number)
		{
			if (number > 1000000)
			{
				string truncatedNumber = TruncateNumberCeil(number, 1000000);
				obj.text = string.Format(TextManager.Get("NumberFormatting.1M"), truncatedNumber);
			}
			else if (number >= 1000)
			{
				string truncatedNumber = TruncateNumberCeil(number, 1000);
				obj.text = string.Format(TextManager.Get("NumberFormatting.1K"), truncatedNumber);
			}
			else
			{
				obj.text = number.ToString();
			}
		}

		static private string TruncateNumberCeil(int number, float ceil)
		{
			float dec = number / ceil; 
			string floatString = dec.ToString();
			Debug.Log(floatString);
			int decimalPointPoisiton = floatString.LastIndexOf('.');

			string prefix = floatString;
			string suffix = "0";
			if (decimalPointPoisiton != -1)
			{
				prefix = floatString.Substring(0, decimalPointPoisiton);
				if (prefix.Length < 3)
				{
					suffix = floatString.Substring(decimalPointPoisiton + 1, 1);
				}
			}


			if (suffix == "0")
			{
				return prefix;
			}
			else
			{
				string result = prefix;
				result += TextManager.Get("NumberFormatting.Comma");
				result += suffix;
				return result;
			}
		}
	}
}*/