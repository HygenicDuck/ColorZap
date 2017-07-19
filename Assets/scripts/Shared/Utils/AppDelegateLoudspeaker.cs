using UnityEngine;
using Utils;
using System.Runtime.InteropServices;

public class AppDelegateLoudspeaker : MonoSingleton<AppDelegateLoudspeaker>
{
	protected override void Init()
	{
	}

	public void SetReadyForMessages()
	{
		SendReadyToRecieveMessagesEvent();

		if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			_UnityReadyToRecieveMessages();
		}
		else if(Application.platform == RuntimePlatform.Android)
		{
			AndroidCls.CallStatic("Start");
		}
	}

	[DllImport("__Internal")]
	private static extern int _UnityReadyToRecieveMessages();

	static AndroidJavaClass s_androidCls = null;
	static AndroidJavaClass AndroidCls
	{
		get
		{ 
			if (s_androidCls == null)
			{
				s_androidCls = new AndroidJavaClass ("com.kwaleeplugins.apploudspeaker.Broadcast");
			}
			return s_androidCls;
		}
	}


	// Unity-side events
	public delegate void RegisteredForRemoteNotificationsDelegate(string token);
	public static event RegisteredForRemoteNotificationsDelegate RegisteredForRemoteNotifications;

	public delegate void FailedToRegisterForRemoteNotificationsDelegate();
	public static event FailedToRegisterForRemoteNotificationsDelegate FailedToRegisterForRemoteNotifications;

	public delegate void ReceivedRemoteNotificationDelegate(string payload);
	public static event ReceivedRemoteNotificationDelegate ReceivedRemoteNotification;

	public delegate void ReceivedLocalNotificationDelegate();
	public static event ReceivedLocalNotificationDelegate ReceivedLocalNotification;

	public delegate void OpenedURLDelegate(string url);
	public static event OpenedURLDelegate OpenURL;

	public delegate void ReceivedMemoryWarningDelegate();
	public static event ReceivedMemoryWarningDelegate ReceivedMemoryWarning;

	public delegate void SignificantTimeChangeDelegate();
	public static event SignificantTimeChangeDelegate SignificantTimeChange;

	public delegate void WillChangeStatusBarFrameDelegate();
	public static event WillChangeStatusBarFrameDelegate WillChangeStatusBarFrame;

	public delegate void WillChangeStatusBarOrientationDelegate();
	public static event WillChangeStatusBarOrientationDelegate WillChangeStatusBarOrientation;

	public delegate void ApplicationDidFinishLaunchingDelegate();
	public static event ApplicationDidFinishLaunchingDelegate ApplicationDidFinishLaunching;

	public delegate void ApplicationDidBecomeActiveDelegate();
	public static event ApplicationDidBecomeActiveDelegate ApplicationDidBecomeActive;

	public delegate void ApplicationWillResignActiveDelegate();
	public static event ApplicationWillResignActiveDelegate ApplicationWillResignActive;

	// see function body for comment
//	public delegate void ApplicationDidEnterBackgroundDelegate();
//	public static event ApplicationDidEnterBackgroundDelegate ApplicationDidEnterBackground;
//
//	public delegate void ApplicationWillEnterForegroundDelegate();
//	public static event ApplicationWillEnterForegroundDelegate ApplicationWillEnterForeground;

	public delegate void ApplicationWillTerminateDelegate();
	public static event ApplicationWillTerminateDelegate ApplicationWillTerminate;

	public delegate void ApplicationReadyToRecieveMessagesDelegate();
	public static event ApplicationReadyToRecieveMessagesDelegate ApplicationReadyToRecieveMessages;

	// Native-Side Notification Handlers
	
	public void _BroadcastUIApplicationDidRegisterForRemoteNotificationsNotification(string token)
	{
		if (RegisteredForRemoteNotifications != null)
		{
			Debugger.Log("C# Registered for remote notifications, token : " + token, (int)SharedSystems.Systems.OS_MESSAGES);
			RegisteredForRemoteNotifications(token);
		}
	}

	public void _BroadcastUIApplicationDidFailToRegisterForRemoteNotificationsNotification()
	{
		if (FailedToRegisterForRemoteNotifications != null)
		{
			Debugger.Log("C# Failed to register for remote notifications", (int)SharedSystems.Systems.OS_MESSAGES);
			FailedToRegisterForRemoteNotifications();
		}
	}

	public void _BroadcastUIApplicationDidReceiveRemoteNotificationNotification(string payload)
	{
		if (ReceivedRemoteNotification != null)
		{
			Debugger.Log("C# Received remote notification", (int)SharedSystems.Systems.OS_MESSAGES);
			ReceivedRemoteNotification(payload);
		}
	}

	public void _BroadcastUIApplicationDidReceiveLocalNotificationNotification()
	{
		if (ReceivedLocalNotification != null)
		{
			Debugger.Log("C# Received local notification", (int)SharedSystems.Systems.OS_MESSAGES);
			ReceivedLocalNotification();
		}
	}

	public void _BroadcastUIApplicationDidOpenURL(string url)
	{
		if (OpenURL != null)
		{
			Debugger.Log("C# Opened URL", (int)SharedSystems.Systems.OS_MESSAGES);
			OpenURL(url);
		}
	}

	public void _BroadcastUIApplicationDidReceiveMemoryWarningNotification()
	{
		if (ReceivedMemoryWarning != null)
		{
			Debugger.Log("C# Received memory warning", Debugger.Severity.WARNING, (int)SharedSystems.Systems.OS_MESSAGES);
			ReceivedMemoryWarning();
		}
	}

	public void _BroadcastUIApplicationSignificantTimeChangeNotification()
	{
		if (SignificantTimeChange != null)
		{
			Debugger.Log("C# significant time change", Debugger.Severity.WARNING, (int)SharedSystems.Systems.OS_MESSAGES);
			SignificantTimeChange();
		}
	}

	public void _BroadcastUIApplicationWillChangeStatusBarFrameNotification()
	{
		if (WillChangeStatusBarFrame != null)
		{
			Debugger.Log("C# Will change status bar frame", (int)SharedSystems.Systems.OS_MESSAGES);
			WillChangeStatusBarFrame();
		}
	}

	public void _BroadcastUIApplicationWillChangeStatusBarOrientationNotification()
	{
		if (WillChangeStatusBarOrientation != null)
		{
			Debugger.Log("C# Will change statis bar orientation", (int)SharedSystems.Systems.OS_MESSAGES);
			WillChangeStatusBarOrientation();
		}
	}

	// I don't think we can instantiate this class early enough in order to receive this message.
	public void _BroadcastUIApplicationDidFinishLaunchingNotification()
	{
		if (ApplicationDidFinishLaunching != null)
		{
			Debugger.Log("C# Application did finish launching", (int)SharedSystems.Systems.OS_MESSAGES);
			ApplicationDidFinishLaunching();
		}
	}

	// I don't think we can instantiate this class early enough in order to receive this message.
	public void _BroadcastUIApplicationDidBecomeActiveNotification()
	{
		if (ApplicationDidBecomeActive != null)
		{
			Debugger.Log("C# Application did become active", (int)SharedSystems.Systems.OS_MESSAGES);
			ApplicationDidBecomeActive();
		}
	}

	public void _BroadcastUIApplicationWillResignActiveNotification()
	{
		if (ApplicationWillResignActive != null)
		{
			Debugger.Log("C# Application will resign active", (int)SharedSystems.Systems.OS_MESSAGES);
			ApplicationWillResignActive();
		}
	}

	// Unity has already been stopped by the time applicationDidEnterBackground is called, use OnApplicationPause(bool paused)
	// which is called on all MonoBehaviours by default. Its also worth noting that the behaviour of applicationDidEnterBackground/Foreground
	// ia slightly different to OnApplicationPause so it is reccomended not to use ApplicationWillEnterForeground unless completely neccasry.

//	public void _BroadcastUIApplicationDidEnterBackgroundNotification()
//	{
//		Debugger.Log("C# Application did enter background try", (int)SharedSystems.Systems.OS_MESSAGES);
//		if (ApplicationDidEnterBackground != null)
//		{
//			Debugger.Log("C# Application did enter background", (int)SharedSystems.Systems.OS_MESSAGES);
//			ApplicationDidEnterBackground();
//		}
//	}
//
//	public void _BroadcastUIApplicationWillEnterForegroundNotification()
//	{
//		if (ApplicationWillEnterForeground != null)
//		{
//			Debugger.Log("C# Application will enter foreground", (int)SharedSystems.Systems.OS_MESSAGES);
//			ApplicationWillEnterForeground();
//		}
//	}

	public void _BroadcastUIApplicationWillTerminateNotification()
	{
		if (ApplicationWillTerminate != null)
		{
			Debugger.Log("C# Application will terminate", Debugger.Severity.WARNING);
			ApplicationWillTerminate();
		}
	}

	public void SendReadyToRecieveMessagesEvent()
	{
		if (ApplicationReadyToRecieveMessages != null)
		{
			Debugger.Log("C# Application will terminate", (int)SharedSystems.Systems.OS_MESSAGES);
			ApplicationReadyToRecieveMessages();
		}
	}
}
