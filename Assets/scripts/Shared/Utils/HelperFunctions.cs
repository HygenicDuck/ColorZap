using System;
using System.Text.RegularExpressions;

namespace Utils
{
	public enum Operator
	{
		EQUAL,
		GREATER,
		LESSTHAN,
	}

	public class HelperFunctions
	{
		public static T[] InitialiseArray<T>(int length) where T : new()
		{
			T[] array = new T[length];
			for (int i = 0; i < length; ++i)
			{
				array[i] = new T();
			}

			return array;
		}

		public static bool ValidateEmail(string email)
		{
			// regex taken from: http://www.codeproject.com/Articles/22777/Email-Address-Validation-Using-Regular-Expression 
			const string EMAIL_PATTERN =
				@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
            + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
              + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
            + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

			if (!string.IsNullOrEmpty(email))
			{
				return Regex.IsMatch(email, EMAIL_PATTERN);
			}
			else
			{
				return false;
			}
		}


		public static bool ValidatePassword(string password)
		{
			// regex taken from: https://www.mkyong.com/regular-expressions/how-to-validate-password-with-regular-expression/
			const string PASS_PATTERN = "(.{4,20})";
//			(?=.*\d)		#   must contains one digit from 0-9
//			(?=.*[a-z])		#   must contains one lowercase characters
//			(?=.*[A-Z])		#   must contains one uppercase characters
//			(?=.*[@#$%])		#   must contains one special symbols in the list "@#$%"

			if (!string.IsNullOrEmpty(password))
			{
				return Regex.IsMatch(password, PASS_PATTERN);
			}
			else
			{
				return false;
			}
		}
	}
}