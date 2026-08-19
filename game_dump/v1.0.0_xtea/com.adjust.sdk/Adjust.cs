using System;
using UnityEngine;

namespace com.adjust.sdk;

public class Adjust : MonoBehaviour
{
	private const string errorMessage = "adjust: SDK not started. Start it manually using the 'start' method.";

	private static IAdjust instance;

	private static Action<string> deferredDeeplinkDelegate;

	private static Action<AdjustEventSuccess> eventSuccessDelegate;

	private static Action<AdjustEventFailure> eventFailureDelegate;

	private static Action<AdjustSessionSuccess> sessionSuccessDelegate;

	private static Action<AdjustSessionFailure> sessionFailureDelegate;

	private static Action<AdjustAttribution> attributionChangedDelegate;

	public bool startManually = true;

	public bool eventBuffering;

	public bool printAttribution = true;

	public bool sendInBackground;

	public bool launchDeferredDeeplink = true;

	public string appToken = "{Your App Token}";

	public AdjustLogLevel logLevel = AdjustLogLevel.Info;

	public AdjustEnvironment environment;

	private void Awake()
	{
		if (instance != null)
		{
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)((Component)this).transform).gameObject);
		if (!startManually)
		{
			AdjustConfig adjustConfig = ((logLevel == AdjustLogLevel.Suppress) ? new AdjustConfig(appToken, environment, allowSuppressLogLevel: true) : new AdjustConfig(appToken, environment));
			adjustConfig.setLogLevel(logLevel);
			adjustConfig.setSendInBackground(sendInBackground);
			adjustConfig.setEventBufferingEnabled(eventBuffering);
			adjustConfig.setLaunchDeferredDeeplink(launchDeferredDeeplink);
			if (printAttribution)
			{
				adjustConfig.setEventSuccessDelegate(EventSuccessCallback);
				adjustConfig.setEventFailureDelegate(EventFailureCallback);
				adjustConfig.setSessionSuccessDelegate(SessionSuccessCallback);
				adjustConfig.setSessionFailureDelegate(SessionFailureCallback);
				adjustConfig.setDeferredDeeplinkDelegate(DeferredDeeplinkCallback);
				adjustConfig.setAttributionChangedDelegate(AttributionChangedCallback);
			}
			start(adjustConfig);
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (instance != null)
		{
			if (pauseStatus)
			{
				instance.onPause();
			}
			else
			{
				instance.onResume();
			}
		}
	}

	public static void start(AdjustConfig adjustConfig)
	{
		if (instance == null && adjustConfig != null)
		{
			instance = new AdjustAndroid();
			if (instance != null)
			{
				eventSuccessDelegate = adjustConfig.getEventSuccessDelegate();
				eventFailureDelegate = adjustConfig.getEventFailureDelegate();
				sessionSuccessDelegate = adjustConfig.getSessionSuccessDelegate();
				sessionFailureDelegate = adjustConfig.getSessionFailureDelegate();
				deferredDeeplinkDelegate = adjustConfig.getDeferredDeeplinkDelegate();
				attributionChangedDelegate = adjustConfig.getAttributionChangedDelegate();
				instance.start(adjustConfig);
			}
		}
	}

	public static void trackEvent(AdjustEvent adjustEvent)
	{
		if (instance != null && adjustEvent != null)
		{
			instance.trackEvent(adjustEvent);
		}
	}

	public static void setEnabled(bool enabled)
	{
		if (instance != null)
		{
			instance.setEnabled(enabled);
		}
	}

	public static bool isEnabled()
	{
		if (instance == null)
		{
			return false;
		}
		return instance.isEnabled();
	}

	public static void setOfflineMode(bool enabled)
	{
		if (instance != null)
		{
			instance.setOfflineMode(enabled);
		}
	}

	public static void sendFirstPackages()
	{
		if (instance != null)
		{
			instance.sendFirstPackages();
		}
	}

	public static void addSessionPartnerParameter(string key, string value)
	{
		AdjustAndroid.addSessionPartnerParameter(key, value);
	}

	public static void addSessionCallbackParameter(string key, string value)
	{
		AdjustAndroid.addSessionCallbackParameter(key, value);
	}

	public static void removeSessionPartnerParameter(string key)
	{
		AdjustAndroid.removeSessionPartnerParameter(key);
	}

	public static void removeSessionCallbackParameter(string key)
	{
		AdjustAndroid.removeSessionCallbackParameter(key);
	}

	public static void resetSessionPartnerParameters()
	{
		AdjustAndroid.resetSessionPartnerParameters();
	}

	public static void resetSessionCallbackParameters()
	{
		AdjustAndroid.resetSessionCallbackParameters();
	}

	public static void setDeviceToken(string deviceToken)
	{
		if (instance != null)
		{
			instance.setDeviceToken(deviceToken);
		}
	}

	public static string getIdfa()
	{
		if (instance == null)
		{
			return null;
		}
		return instance.getIdfa();
	}

	public static void setReferrer(string referrer)
	{
		if (instance != null)
		{
			instance.setReferrer(referrer);
		}
	}

	public static void getGoogleAdId(Action<string> onDeviceIdsRead)
	{
		if (instance != null)
		{
			instance.getGoogleAdId(onDeviceIdsRead);
		}
	}

	public void GetNativeAttribution(string attributionData)
	{
		if (instance != null && attributionChangedDelegate != null)
		{
			AdjustAttribution obj = new AdjustAttribution(attributionData);
			attributionChangedDelegate(obj);
		}
	}

	public void GetNativeEventSuccess(string eventSuccessData)
	{
		if (instance != null && eventSuccessDelegate != null)
		{
			AdjustEventSuccess obj = new AdjustEventSuccess(eventSuccessData);
			eventSuccessDelegate(obj);
		}
	}

	public void GetNativeEventFailure(string eventFailureData)
	{
		if (instance != null && eventFailureDelegate != null)
		{
			AdjustEventFailure obj = new AdjustEventFailure(eventFailureData);
			eventFailureDelegate(obj);
		}
	}

	public void GetNativeSessionSuccess(string sessionSuccessData)
	{
		if (instance != null && sessionSuccessDelegate != null)
		{
			AdjustSessionSuccess obj = new AdjustSessionSuccess(sessionSuccessData);
			sessionSuccessDelegate(obj);
		}
	}

	public void GetNativeSessionFailure(string sessionFailureData)
	{
		if (instance != null && sessionFailureDelegate != null)
		{
			AdjustSessionFailure obj = new AdjustSessionFailure(sessionFailureData);
			sessionFailureDelegate(obj);
		}
	}

	public void GetNativeDeferredDeeplink(string deeplinkURL)
	{
		if (instance != null && deferredDeeplinkDelegate != null)
		{
			deferredDeeplinkDelegate(deeplinkURL);
		}
	}

	private void AttributionChangedCallback(AdjustAttribution attributionData)
	{
		if (attributionData.trackerName != null)
		{
		}
		if (attributionData.trackerToken != null)
		{
		}
		if (attributionData.network != null)
		{
		}
		if (attributionData.campaign != null)
		{
		}
		if (attributionData.adgroup != null)
		{
		}
		if (attributionData.creative != null)
		{
		}
		if (attributionData.clickLabel == null)
		{
		}
	}

	private void EventSuccessCallback(AdjustEventSuccess eventSuccessData)
	{
		if (eventSuccessData.Message != null)
		{
		}
		if (eventSuccessData.Timestamp != null)
		{
		}
		if (eventSuccessData.Adid != null)
		{
		}
		if (eventSuccessData.EventToken != null)
		{
		}
		if (eventSuccessData.JsonResponse == null)
		{
		}
	}

	private void EventFailureCallback(AdjustEventFailure eventFailureData)
	{
		if (eventFailureData.Message != null)
		{
		}
		if (eventFailureData.Timestamp != null)
		{
		}
		if (eventFailureData.Adid != null)
		{
		}
		if (eventFailureData.EventToken != null)
		{
		}
		if (eventFailureData.JsonResponse == null)
		{
		}
	}

	private void SessionSuccessCallback(AdjustSessionSuccess sessionSuccessData)
	{
		if (sessionSuccessData.Message != null)
		{
		}
		if (sessionSuccessData.Timestamp != null)
		{
		}
		if (sessionSuccessData.Adid != null)
		{
		}
		if (sessionSuccessData.JsonResponse == null)
		{
		}
	}

	private void SessionFailureCallback(AdjustSessionFailure sessionFailureData)
	{
		if (sessionFailureData.Message != null)
		{
		}
		if (sessionFailureData.Timestamp != null)
		{
		}
		if (sessionFailureData.Adid != null)
		{
		}
		if (sessionFailureData.JsonResponse == null)
		{
		}
	}

	private void DeferredDeeplinkCallback(string deeplinkURL)
	{
		if (deeplinkURL == null)
		{
		}
	}
}
