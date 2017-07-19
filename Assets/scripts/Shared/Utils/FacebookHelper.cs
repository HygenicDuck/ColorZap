using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using Kanga;
using System;

public class FacebookHelper
{
	public const string GAMEREQUEST_APPREQUEST = "apprequest";
	public const string GAMEREQUEST_ASKFOR = "askfor";
	public const string GAMEREQUEST_SEND = "send";
	public const string GAMEREQUEST_TURN = "turn";
	private const string APP_LINK_ID = "https://fb.me/504893686374126";

	public static bool IsLoggedIn()
	{
		return FB.IsLoggedIn;
	}

	public static void Login()
	{
/*		FacebookAuth.Login(
			() => {
				//TODO: Show error popup
			},
			() => {
			});*/
	}

	public static void RequestInvitableFriends(Action<IGraphResult> callback, Action<IGraphResult> failCallback)
	{
		RequestFriends("invitable_friends", callback, failCallback);
	}

	private static void RequestFriends(string friendsURI, Action<IGraphResult> callback, Action<IGraphResult> failCallback, int amount = 5000)
	{
		FB.API("/me/" + friendsURI + ("?limit=" + amount), HttpMethod.GET, (result) => {

			if (result.ResultDictionary.ContainsKey("error"))
			{
				failCallback(result);
			}
			else
			{
				callback(result);
			}
		});
	}

	public static void RequestImageNormal(string id, Action<Texture2D> callback)
	{
		RequestImage(id, "normal", callback);
	}

	public static void RequestImageLarge(string id, Action<Texture2D> callback)
	{
		RequestImage(id, "large", callback);
	}

	private static void RequestImage(string id, string size, Action<Texture2D> callback)
	{
		FB.API(id + "/picture?type=" + size, HttpMethod.GET, (result) => {
			callback(result.Texture);
		});

	
	}

	public static void RequestCover(string id, Action<Texture2D> callback = null)
	{
		FB.API(id + "/?fields=cover", HttpMethod.GET, (result) => {

			IDictionary<string,object> cover = result.ResultDictionary["cover"] as IDictionary<string,object>;

			string coverUrl = cover["source"] as string;

			Utils.WebImage.Request(coverUrl, ((Utils.WebImage.ServerImage obj) => 
				{
					callback (obj.texture as Texture2D);
				}));

		});


	}

	public static string GetImageMediumUrl(string id)
	{
		return "https" + "://graph.facebook.com/" + id + "/picture?type=med";
	}

	public static string GetImageLargeUrl(string id)
	{
		return "https" + "://graph.facebook.com/" + id + "/picture?type=large";
	}



	public static void AppRequestGameInvite(string recipienteMessage, string clientMessage, List<string> facebookIds, Action<IAppRequestResult> callback, Action errorCallback = null)
	{
		FB.AppRequest(
			recipienteMessage,
			facebookIds, null, null, null, GAMEREQUEST_APPREQUEST, clientMessage,
			delegate (IAppRequestResult result) {
				if (string.IsNullOrEmpty(result.Error))
				{
					Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "AppRequestGameInvite", (int)SharedSystems.Systems.FACEBOOK_HELPER);

					callback(result);
				}
				else
				{
					if (errorCallback != null)
					{
						errorCallback();
					}
				}
			}
		);
	}

	public static void GetAppRequests(Action<IGraphResult> callback, Action failcallback = null)
	{
		FB.API("me/apprequests", HttpMethod.GET, (result) => {
			Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "CheckkAppRequest", (int)SharedSystems.Systems.FACEBOOK_HELPER);

			if (!result.ResultDictionary.ContainsKey("error"))
			{
				callback(result);
			}
			else
			{
				if (failcallback != null)
				{
					failcallback();
				}
			}
		});
	}
			

	public static void ShareLink(Uri contentURL, string contentTitle, string contentDescription, Uri photoURL, Action callback)
	{
		FB.ShareLink(contentURL, contentTitle, contentDescription, photoURL, delegate (IShareResult result) {
			if (string.IsNullOrEmpty(result.Error))
			{
				Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "AppRequestGameInvite", (int)SharedSystems.Systems.FACEBOOK_HELPER);
				callback();
			}
			else
			{
				callback();
			}
		});
	}

	public static void SendAppInvite(string imageURL, Action<bool> callback)
	{
		Facebook.Unity.FB.Mobile.AppInvite(new System.Uri (APP_LINK_ID), new System.Uri (imageURL), (result) => {
			if (string.IsNullOrEmpty(result.Error))
			{
				Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "SendAppInvite", (int)SharedSystems.Systems.FACEBOOK_HELPER);
			}
			callback(result.Cancelled);
		});
	}

	public static void DeleteAppRequests(string appRequest)
	{
		FB.API(appRequest, HttpMethod.DELETE, (result) => {
			Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "CheckkAppRequest", (int)SharedSystems.Systems.FACEBOOK_HELPER);

			if (result.ResultDictionary.ContainsKey("error"))
			{
				Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "Error deleting app request", (int)SharedSystems.Systems.FACEBOOK_HELPER);
			}

		});
	}


	public static void AppRequestGift(string openGraphId, string recipienteMessage, string clientMessage, List<string> facebookIds, Action<IAppRequestResult> callback, Action errorCallback = null)
	{
		FB.AppRequest(
			recipienteMessage, OGActionType.SEND, openGraphId, facebookIds, GAMEREQUEST_SEND, clientMessage,
			delegate (IAppRequestResult result) {
				if (string.IsNullOrEmpty(result.Error))
				{
					Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "AppRequestGameInvite", (int)SharedSystems.Systems.FACEBOOK_HELPER);

					callback(result);
				}
				else
				{
					if (errorCallback != null)
					{
						errorCallback();
					}
				}
			}
		);
	}

	public static void AppRequestAskFor(string openGraphId, string recipienteMessage, string clientMessage, List<string> facebookIds, Action<IAppRequestResult> callback, Action errorCallback = null)
	{
		FB.AppRequest(
			recipienteMessage, OGActionType.ASKFOR, openGraphId, facebookIds, GAMEREQUEST_ASKFOR, clientMessage,
			delegate (IAppRequestResult result) {
				if (string.IsNullOrEmpty(result.Error))
				{
					Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "AppRequestGameInvite", (int)SharedSystems.Systems.FACEBOOK_HELPER);

					callback(result);
				}
				else
				{
					if (errorCallback != null)
					{
						errorCallback();
					}
				}
			}
		);
	}

	public static void AppRequestYourTurn(string recipienteMessage, string clientMessage, List<string> facebookIds, Action<IAppRequestResult> callback, Action errorCallback = null)
	{
		FB.AppRequest(recipienteMessage, facebookIds, null, null, null, GAMEREQUEST_TURN, clientMessage,
			delegate (IAppRequestResult result) {
				if (string.IsNullOrEmpty(result.Error))
				{
					Utils.Debugger.PrintDictionaryAsServerObject(result.ResultDictionary, "AppRequestYourTurn", (int)SharedSystems.Systems.FACEBOOK_HELPER);

					if (callback != null)
					{
						callback(result);
					}
				}
				else
				{
					if (errorCallback != null)
					{
						errorCallback();
					}
				}
			}
		);
	}
}
