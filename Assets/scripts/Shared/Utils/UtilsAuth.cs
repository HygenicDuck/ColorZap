using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System;


namespace Utils
{
	public class Auth
	{
		public static string GetApplicationVersion()
		{
			String version = Utils.EmbededVersion.Version;

			#if UNITY_EDITOR
			// example: would convert 1.11.3.222 => 1.11.3.0
			string[] versionValues = version.Split('.');

			Debugger.Assert(versionValues.Length == 4, "Auth::GetApplicationVersion() we do not have a 4 piece build number");
			if (versionValues.Length == 4)
			{
				version = "";

				for (int i = 0; i < versionValues.Length - 1; i++)
				{
					if (i > 0)
					{
						version += ".";
					}
					version += versionValues[i];
				}
			}

			version = version + ".0";

			#endif

			return version;
		}


		public static string GetHMAC(string authCreationKey, string requestData)
		{
			HMACSHA256 hmac = new HMACSHA256 (Encoding.ASCII.GetBytes(authCreationKey));

			return System.Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(requestData)));
		}

//		public static void RunIfNetwork(Action callback, float retryAfterDelay = 2f)
//		{
//			WaffleIron.Server wIron = WaffleIron.Server.Instance;
//			if (wIron.NetworkLoss)
//			{				
//				NetworkLossPopup.Instance.ShowRetryOnly(
//					() => {
//						RunIfNetwork(callback);		
//					}, retryAfterDelay);
//
//			}
//			else
//			{
//				if (callback != null)
//				{
//					callback();
//				}
//
//			}
//		}
//
//		public static void RetryUntilNetwork(Action callback)
//		{
//			WaffleIron.Server wIron = WaffleIron.Server.Instance;
//			if (wIron.NetworkLoss)
//			{				
//				NetworkLossPopup.Instance.ShowRetryOnly(
//					() => {
//						callback();
//					});
//
//			}
//			else
//			{
//				callback();
//			}
//		}
	}
}