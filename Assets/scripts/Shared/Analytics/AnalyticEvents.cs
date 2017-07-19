using System.Collections;
using UnityEngine;

namespace Analytics
{
	public class Events 
	{
		
		public const string UI_SCREEN = "uiscreen";
		public const string UI_BUTTON = "uiselect";
		public const string NOTIFICATION_OPENEDFROMSIDEBAR = "notificationOpenedFromGame";
		public const string NOTIFICATION_SEEN = "notificationSeen";
		public const string BANNER_VIEWS = "bannersViewed";

		/// <summary>
		/// QUEST
		/// </summary>
		public const string QUEST_STARTED = "questStarted";
		public const string QUEST_COMPLETED = "questCompleted";
		public const string FTUE_COMPLETED = "ftueCompleted";

		/// <summary>
		/// CAMERA
		/// </summary>
		public const string CAMERA_SINGLE 			= "cameraSingle";
		public const string CAMERA_MULTI 			= "cameraMulti";
		public const string CAMERA_SNAP 			= "cameraSnap";
		public const string CAMERA_OTHERSOURCES 	= "cameraOtherSources";
		public const string CAMERA_BACK 			= "cameraBack";
		public const string CAMERA_RESHOOT 			= "cameraReshoot";

		/// <summary>
		/// IMAGE SOURCES
		/// </summary>
		public const string OTHERRESOURCES_GOT_PHOTO_FROM_ALBUM 			= "otherResourcesGotPhotoFromAlbum";
		public const string OTHERRESOURCES_GOT_PHOTO_FROM_WEB 				= "otherResourcesGotPhotoFromWeb";
		public const string OTHERRESOURCES_GOT_PHOTO_FROM_MYWAFFLEPHOTOS 	= "otherResourcesGotPhotoFromMyWafflePhotos";
		public const string OTHERRESOURCES_GOT_PHOTO_FROM_FACEBOOK 			= "otherResourcesGotPhotoFromFacebook";

		/// <summary>
		/// IMAGE DOWNLOADS
		/// </summary>
		public const string IMAGE_OVER_SIZE_LIMIT	= "imageOverSizeLimit";

		/// <summary>
		/// PUSH NOTIFICATIONS
		/// </summary>
		public const string PUSH_ACCEPTANCE_POPUP 			= "pushAcceptancePopup";
		public const string PUSH_ACCEPTANCE_POPUP_BUTTON 	= "pushAcceptancePopupButton";
		public const string PUSH_ACCEPTANCE_POPUP_SUCCESS 	= "pushAcceptancePopupSuccess";
		public const string PUSH_ACCEPTANCE_POPUP_FAIL 		= "pushAcceptancePopupFail";
		public const string NOTIFICATIONS_TOKENADDED     	= "notificationTokenAdded";

		/// <summary>
		/// CREATE
		/// </summary>
		public const string QUESTION_EDIT = "questionedit";
		public const string QUESTION_EDITED = "questionedited";
		public const string QUIZ_EDIT = "quizEdit";
		public const string QUIZ_EDITED = "quizEdited";
		public const string QUIZ_PUBLISHED_CLIENT = "quizPublishedClient";
		public const string QUESTION_CREATED_WITH_STICKERS= "puzzle_created_with_stickers";

		/// <summary>
		/// SOCIAL
		/// </summary>
		public const string SOCIAL_COMMENT = "socialComment";
		public const string SOCIAL_LIKE = "collectionLike";
		public const string SOCIAL_UNLIKE = "collectionUnlike";
		public const string SOCIAL_DISLIKE = "collectionDislike";
		public const string SOCIAL_UNDISLIKE = "collectionUndislike";
		public const string SOCIAL_REPORT = "socialReport";
		public const string SOCIAL_SHARE_PUZZLE = "socialSharePuzzle";
		public const string SOCIAL_SHARE_COLLECTION = "socialShareCollection";
		public const string SOCIAL_SHARE_SCORE = "socialShareScore";
		public const string SOCIAL_SHARE_FACEBOOK_COLLECTION = "socialShareFacebookCollection";
		public const string SOCIAL_SHARE_GENERIC = "socialShareGeneric";
		public const string SOCIAL_SHARE_PLAYER_TITLE = "socialSharePlayerTitle";
		public const string SOCIAL_FACEBOOK_POSTED = "facebookPosted";
		public const string SOICAL_SHARE_MOMENT = "socialShareMoment";


		/// <summary>
		/// POWERUPS
		/// </summary>
		public const string POWERUP_BOMB_USED = "powerupbombused";
		public const string POWERUP_RETRY_USED = "powerupretryused";
		public const string POWERUP_RETRY_SHOWN = "powerupretryshown";
		public const string POWERUP_BOMB_AUTO_ON = "bombpreloadon";
		public const string POWERUP_BOMB_AUTO_OFF = "bombpreloadoff";

		/// <summary>
		/// GIFTING
		/// </summary>
		public const string GIFTING_CONSUMED = "giftsconsumed";
		public const string GIFTING_REQUESTED = "giftrequested";
		public const string GIFTING_GIFTED = "gifted";
		public const string GIFTING_POPUP= "gifting";
		public const string GIFTING_POPUP_OPEN = "popup";
		public const string GIFTING_POPUP_CLOSE = "popupclosed";
		public const string GIFTING_POPUP_ACCEPTED = "popupaccepted";
		public const string GIFTING_POPUP_INBOX = "inbox";
		public const string GIFTING_POPUP_SOCIALINVITE = "invitefriends";
		public const string GIFTING_POPUP_SENDLIVES = "sendlives";
		public const string GIFTING_POPUP_SENDBOMBS = "sendbombs";
		public const string GIFTING_POPUP_SENDCOINS = "sendcoins";
		public const string GIFTING_POPUP_REQUESTLIVES = "requestlives";


		/// <summary>
		/// INVITES
		/// </summary>
		public const string INVITES_FACEBOOK_SENT = "facebookinvite";
		public const string INVITES_FACEBOOK_SENT_FROM_FRIENDSSCREEN = "friendsScreen";
		public const string INVITES_FACEBOOK_SENT_FROM_CHAININVITE = "chaininvite";
		public const string INVITES_FACEBOOK_SENT_FROM_GIFTINGPOPUP = "giftingpopup";
		public const string INVITES_PUZZLE = "puzzleInvitesSent";
		public const string INVITES_COLLECTION = "collectionInvitesSent";


		/// <summary>
		/// UISELECT
		/// </summary>
		public const string UISELECT_RANDOMOPPONENT = "randomopponentcell";
		public const string UISELECT_FACEBOOKOPPONENT = "facebookopponentcell";
		public const string UISELECT_WAFFLEOPPONENT = "waffleopponentcell";


		/// <summary>
		/// PROFILE
		/// </summary>
		public const string PROFILE_VIEWED = "profileViewed";
		public const string PROFILE_FOLLOW = "profileFollow";
		public const string PROFILE_UNFOLLOW = "profileUnfollow";
		public const string PROFILE_MAKE_FRIEND = "profileMakeFriend";
		public const string PROFILE_UNFRIEND = "profileUnfriend";
		public const string PROFILE_PLAY = "profilePlay";
		public const string PROFILE_CHANGE_BACKGROUND = "profileChangeBackground";
		public const string PROFILE_CHANGE_AVATAR = "profileChangePhoto";
		public const string PROFILE_CHANGE_NAME = "profileChangeName"; // TODO: Not in this sprint - remove this comment once added
		public const string PROFILE_CHANGE_AVATAR_FROM_FACEBOOK = "profileChangePhotoFromFB"; // TODO: Not in this sprint - remove this comment once added



		/// <summary>
		/// SHOP
		/// </summary>
		public const string SHOP_OUT_OF_LIVES = "outOfLivesPopup";
		public const string SHOP_SHOW_UI = "shopUiPopup";
		public const string SHOP_SHOW_POST_CHALLENGE = "shopPopupPostChallenge";


		/// <summary>
		/// RESOURCE_BAR
		/// </summary>
		public const string RESOURCE_BAR_COIN_BUTTON_PRESSED = "coinShopButtonPressed";
		public const string RESOURCE_BAR_LIFE_BUTTON_PRESSED = "lifeButtonPressed";
		public const string RESOURCE_BAR_LEVEL_BUTTON_PRESSED = "levelButtonPressed";


		/// <summary>
		/// CHALLENGE FLOW
		/// </summary>
		public const string CHALLENGE_PLAY = "createChallengePlay";
		public const string CHALLENGE_NON_AVAILABLE = "noAvailableChains";
		public const string CHALLENGE_NON_AVAILABLE_CURRENT = "activeChains";
		public const string CHALLENGE_FIRST_QUIZ_PICKED = "challengeFirstQuizPicked";

		/// <summary>
		/// QUIZ PLAY
		/// </summary>
		public const string QUIZ_LOADING_SPINNER = "quizLoadingSpinner";
		public const string QUESTION_LOADING_SPINNER = "questionLoadingSpinner";
		public const string PLAY_RESULT_UPLOADING_SPINNER = "playResultUploadingSpinner";
		public const string QUIZ_PLAYED_CLIENT = "quizPlayedClient";
		public const string QUIZZES_PLAYED_PREFIX = "quizzes_played_";

		/// <summary>
		/// ADVERTS
		/// </summary>
		public const string AD_INTERSTITIAL_DISPLAYED = "adInterstitialDisplayed";
		public const string AD_INTERSTITIAL_CLOSED = "adInterstitialDismissed";

		public const string AD_REWARD_DISPLAYED = "adRewardDisplayed";
		public const string AD_REWARD_CLOSED = "adRewardClosed";

		public const string AD_ATTRIBUTION_ANON_PLAYER = "adAttributionAnonPlayer";
		public const string AD_ATTRIBUTION = "adAttribution";

		public const string AD_NEWGAME_POPUP = "adInNewGamePopup";
		public const string AD_NEWGAME_POPUP_PARAM = "accepted";

		public const string LOGGED_IN = "LoggedIn";
		public const string FIRST_LOGIN_EMAIL = "FirstLoginEmailIntroduced";
		public const string FIRST_LOGIN_PASSWORD = "FirstLoginPasswordIntroduced";
		public const string CHANGED_TO_FACEBOOK = "ChangedToFacebook";
		public const string LOGOUT_FACEBOOK = "logoutFacebook";
		public const string USER_PROMOTED = "UserPromoted";

		public const string CHAT_REQUESTED = "chatRequested";
		public const string CHAT_ACCEPTED = "chatAccepted";
		public const string CHAT_DECLINED = "chatDeciled";
		public const string CHAT_SENT = "chatSent";
		public const string CHAT_CLOSED = "chatClosed";
		public const string CHAT_READ = "chatRead";

		/// <summary>
		/// WAFFLEIRON SERVER
		/// </summary>
		public const string SERVER_QUEUE_INFORMATION = "serverQueueInformation";
		public const string FORCE_UPGRADE_POPUP = "forced_upgrade_popup";
		public const string NEW_USER_FROM_LOGGED_OUT = "new_user_from_logged_out";

		/// <summary>
		/// NETWORK
		/// </summary>
		public const string NETWORK_TYPE = "networkState";

		/// <summary>
		/// FACEBOOK COLLECTION
		/// </summary>
	
		public const string FACEBOOK_COLLECTION_GENERATION_STARTED = "facebookQuizGenerationStarted";
		public const string FACEBOOK_PUZZLE_CREATED = "facebookQuestionCreated";
		public const string FACEBOOK_COLLECTION_CREATED = "facebookQuizCreated";
		public const string FACEBOOK_COLLECTION_POPUP_SHOWN = "facebookQuizPopupShown";
		public const string FACEBOOK_COLLECTION_POPUP_PLAY_PRESSED = "facebookQuizPopupPlayPressed";
		public const string FACEBOOK_COLLECTION_POPUP_NOT_NOW_PRESSED = "facebookQuizPopupNotNowPressed";

		public const string TIME_SINCE_NEW_GAME = "timeSinceNewGame";

		/// <summary>
		/// POWERUPS
		/// </summary>
		public const string POWERUP_USED_PREFIX = "waffleiron_powerup_";
		public const string POWERUP_USED_SUFFIX = "_used";


		/// <summary>
		/// REWARDS
		/// </summary>
		public const string REWARDS_SHOWINSTACAMERA = "camera_reward_popup";
	}
}