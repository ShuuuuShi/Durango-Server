using System;
using UnityEngine;

namespace com.adjust.sdk;

public class AdjustAndroid : IAdjust
{
	private class AttributionChangeListener : AndroidJavaProxy
	{
		private Action<AdjustAttribution> callback;

		public AttributionChangeListener(Action<AdjustAttribution> pCallback)
			: base("com.adjust.sdk.OnAttributionChangedListener")
		{
			callback = pCallback;
		}

		public void onAttributionChanged(AndroidJavaObject attribution)
		{
			if (callback != null)
			{
				AdjustAttribution adjustAttribution = new AdjustAttribution();
				adjustAttribution.trackerName = attribution.Get<string>(AdjustUtils.KeyTrackerName);
				adjustAttribution.trackerToken = attribution.Get<string>(AdjustUtils.KeyTrackerToken);
				adjustAttribution.network = attribution.Get<string>(AdjustUtils.KeyNetwork);
				adjustAttribution.campaign = attribution.Get<string>(AdjustUtils.KeyCampaign);
				adjustAttribution.adgroup = attribution.Get<string>(AdjustUtils.KeyAdgroup);
				adjustAttribution.creative = attribution.Get<string>(AdjustUtils.KeyCreative);
				adjustAttribution.clickLabel = attribution.Get<string>(AdjustUtils.KeyClickLabel);
				callback(adjustAttribution);
			}
		}
	}

	private class DeferredDeeplinkListener : AndroidJavaProxy
	{
		private Action<string> callback;

		public DeferredDeeplinkListener(Action<string> pCallback)
			: base("com.adjust.sdk.OnDeeplinkResponseListener")
		{
			callback = pCallback;
		}

		public bool launchReceivedDeeplink(AndroidJavaObject deeplink)
		{
			if (callback == null)
			{
				return launchDeferredDeeplink;
			}
			string obj = deeplink.Call<string>("toString", new object[0]);
			callback(obj);
			return launchDeferredDeeplink;
		}
	}

	private class EventTrackingSucceededListener : AndroidJavaProxy
	{
		private Action<AdjustEventSuccess> callback;

		public EventTrackingSucceededListener(Action<AdjustEventSuccess> pCallback)
			: base("com.adjust.sdk.OnEventTrackingSucceededListener")
		{
			callback = pCallback;
		}

		public void onFinishedEventTrackingSucceeded(AndroidJavaObject eventSuccessData)
		{
			if (callback != null && eventSuccessData != null)
			{
				AdjustEventSuccess adjustEventSuccess = new AdjustEventSuccess();
				adjustEventSuccess.Adid = eventSuccessData.Get<string>(AdjustUtils.KeyAdid);
				adjustEventSuccess.Message = eventSuccessData.Get<string>(AdjustUtils.KeyMessage);
				adjustEventSuccess.Timestamp = eventSuccessData.Get<string>(AdjustUtils.KeyTimestamp);
				adjustEventSuccess.EventToken = eventSuccessData.Get<string>(AdjustUtils.KeyEventToken);
				try
				{
					AndroidJavaObject val = eventSuccessData.Get<AndroidJavaObject>(AdjustUtils.KeyJsonResponse);
					string jsonResponseString = val.Call<string>("toString", new object[0]);
					adjustEventSuccess.BuildJsonResponseFromString(jsonResponseString);
				}
				catch (Exception)
				{
				}
				callback(adjustEventSuccess);
			}
		}
	}

	private class EventTrackingFailedListener : AndroidJavaProxy
	{
		private Action<AdjustEventFailure> callback;

		public EventTrackingFailedListener(Action<AdjustEventFailure> pCallback)
			: base("com.adjust.sdk.OnEventTrackingFailedListener")
		{
			callback = pCallback;
		}

		public void onFinishedEventTrackingFailed(AndroidJavaObject eventFailureData)
		{
			if (callback != null && eventFailureData != null)
			{
				AdjustEventFailure adjustEventFailure = new AdjustEventFailure();
				adjustEventFailure.Adid = eventFailureData.Get<string>(AdjustUtils.KeyAdid);
				adjustEventFailure.Message = eventFailureData.Get<string>(AdjustUtils.KeyMessage);
				adjustEventFailure.WillRetry = eventFailureData.Get<bool>(AdjustUtils.KeyWillRetry);
				adjustEventFailure.Timestamp = eventFailureData.Get<string>(AdjustUtils.KeyTimestamp);
				adjustEventFailure.EventToken = eventFailureData.Get<string>(AdjustUtils.KeyEventToken);
				try
				{
					AndroidJavaObject val = eventFailureData.Get<AndroidJavaObject>(AdjustUtils.KeyJsonResponse);
					string jsonResponseString = val.Call<string>("toString", new object[0]);
					adjustEventFailure.BuildJsonResponseFromString(jsonResponseString);
				}
				catch (Exception)
				{
				}
				callback(adjustEventFailure);
			}
		}
	}

	private class SessionTrackingSucceededListener : AndroidJavaProxy
	{
		private Action<AdjustSessionSuccess> callback;

		public SessionTrackingSucceededListener(Action<AdjustSessionSuccess> pCallback)
			: base("com.adjust.sdk.OnSessionTrackingSucceededListener")
		{
			callback = pCallback;
		}

		public void onFinishedSessionTrackingSucceeded(AndroidJavaObject sessionSuccessData)
		{
			if (callback != null && sessionSuccessData != null)
			{
				AdjustSessionSuccess adjustSessionSuccess = new AdjustSessionSuccess();
				adjustSessionSuccess.Adid = sessionSuccessData.Get<string>(AdjustUtils.KeyAdid);
				adjustSessionSuccess.Message = sessionSuccessData.Get<string>(AdjustUtils.KeyMessage);
				adjustSessionSuccess.Timestamp = sessionSuccessData.Get<string>(AdjustUtils.KeyTimestamp);
				try
				{
					AndroidJavaObject val = sessionSuccessData.Get<AndroidJavaObject>(AdjustUtils.KeyJsonResponse);
					string jsonResponseString = val.Call<string>("toString", new object[0]);
					adjustSessionSuccess.BuildJsonResponseFromString(jsonResponseString);
				}
				catch (Exception)
				{
				}
				callback(adjustSessionSuccess);
			}
		}
	}

	private class SessionTrackingFailedListener : AndroidJavaProxy
	{
		private Action<AdjustSessionFailure> callback;

		public SessionTrackingFailedListener(Action<AdjustSessionFailure> pCallback)
			: base("com.adjust.sdk.OnSessionTrackingFailedListener")
		{
			callback = pCallback;
		}

		public void onFinishedSessionTrackingFailed(AndroidJavaObject sessionFailureData)
		{
			if (callback != null && sessionFailureData != null)
			{
				AdjustSessionFailure adjustSessionFailure = new AdjustSessionFailure();
				adjustSessionFailure.Adid = sessionFailureData.Get<string>(AdjustUtils.KeyAdid);
				adjustSessionFailure.Message = sessionFailureData.Get<string>(AdjustUtils.KeyMessage);
				adjustSessionFailure.WillRetry = sessionFailureData.Get<bool>(AdjustUtils.KeyWillRetry);
				adjustSessionFailure.Timestamp = sessionFailureData.Get<string>(AdjustUtils.KeyTimestamp);
				try
				{
					AndroidJavaObject val = sessionFailureData.Get<AndroidJavaObject>(AdjustUtils.KeyJsonResponse);
					string jsonResponseString = val.Call<string>("toString", new object[0]);
					adjustSessionFailure.BuildJsonResponseFromString(jsonResponseString);
				}
				catch (Exception)
				{
				}
				callback(adjustSessionFailure);
			}
		}
	}

	private class DeviceIdsReadListener : AndroidJavaProxy
	{
		private Action<string> onPlayAdIdReadCallback;

		public DeviceIdsReadListener(Action<string> pCallback)
			: base("com.adjust.sdk.OnDeviceIdsRead")
		{
			onPlayAdIdReadCallback = pCallback;
		}

		public void onGoogleAdIdRead(string playAdId)
		{
			if (onPlayAdIdReadCallback != null)
			{
				onPlayAdIdReadCallback(playAdId);
			}
		}

		public void onGoogleAdIdRead(AndroidJavaObject ajoAdId)
		{
			if (ajoAdId == null)
			{
				string playAdId = null;
				onGoogleAdIdRead(playAdId);
			}
			else
			{
				onGoogleAdIdRead(ajoAdId.Call<string>("toString", new object[0]));
			}
		}
	}

	private const string sdkPrefix = "unity4.10.2";

	private static bool launchDeferredDeeplink = true;

	private static AndroidJavaClass ajcAdjust;

	private AndroidJavaObject ajoCurrentActivity;

	private DeferredDeeplinkListener onDeferredDeeplinkListener;

	private AttributionChangeListener onAttributionChangedListener;

	private EventTrackingFailedListener onEventTrackingFailedListener;

	private EventTrackingSucceededListener onEventTrackingSucceededListener;

	private SessionTrackingFailedListener onSessionTrackingFailedListener;

	private SessionTrackingSucceededListener onSessionTrackingSucceededListener;

	public AdjustAndroid()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		ajoCurrentActivity = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
	}

	public void start(AdjustConfig adjustConfig)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Expected O, but got Unknown
		AndroidJavaObject val = ((adjustConfig.environment != 0) ? ((AndroidJavaObject)new AndroidJavaClass("com.adjust.sdk.AdjustConfig")).GetStatic<AndroidJavaObject>("ENVIRONMENT_PRODUCTION") : ((AndroidJavaObject)new AndroidJavaClass("com.adjust.sdk.AdjustConfig")).GetStatic<AndroidJavaObject>("ENVIRONMENT_SANDBOX"));
		bool? allowSuppressLogLevel = adjustConfig.allowSuppressLogLevel;
		AndroidJavaObject val3;
		if (allowSuppressLogLevel.HasValue)
		{
			AndroidJavaObject val2 = new AndroidJavaObject("java.lang.Boolean", new object[1] { adjustConfig.allowSuppressLogLevel.Value });
			val3 = new AndroidJavaObject("com.adjust.sdk.AdjustConfig", new object[4] { ajoCurrentActivity, adjustConfig.appToken, val, val2 });
		}
		else
		{
			val3 = new AndroidJavaObject("com.adjust.sdk.AdjustConfig", new object[3] { ajoCurrentActivity, adjustConfig.appToken, val });
		}
		launchDeferredDeeplink = adjustConfig.launchDeferredDeeplink;
		AdjustLogLevel? logLevel = adjustConfig.logLevel;
		if (logLevel.HasValue)
		{
			AndroidJavaObject @static = ((AndroidJavaObject)new AndroidJavaClass("com.adjust.sdk.LogLevel")).GetStatic<AndroidJavaObject>(adjustConfig.logLevel.Value.uppercaseToString());
			if (@static != null)
			{
				val3.Call("setLogLevel", new object[1] { @static });
			}
		}
		double? delayStart = adjustConfig.delayStart;
		if (delayStart.HasValue)
		{
			val3.Call("setDelayStart", new object[1] { adjustConfig.delayStart });
		}
		bool? eventBufferingEnabled = adjustConfig.eventBufferingEnabled;
		if (eventBufferingEnabled.HasValue)
		{
			AndroidJavaObject val4 = new AndroidJavaObject("java.lang.Boolean", new object[1] { adjustConfig.eventBufferingEnabled.Value });
			val3.Call("setEventBufferingEnabled", new object[1] { val4 });
		}
		bool? sendInBackground = adjustConfig.sendInBackground;
		if (sendInBackground.HasValue)
		{
			val3.Call("setSendInBackground", new object[1] { adjustConfig.sendInBackground.Value });
		}
		if (adjustConfig.userAgent != null)
		{
			val3.Call("setUserAgent", new object[1] { adjustConfig.userAgent });
		}
		if (!string.IsNullOrEmpty(adjustConfig.processName))
		{
			val3.Call("setProcessName", new object[1] { adjustConfig.processName });
		}
		if (adjustConfig.defaultTracker != null)
		{
			val3.Call("setDefaultTracker", new object[1] { adjustConfig.defaultTracker });
		}
		if (adjustConfig.attributionChangedDelegate != null)
		{
			onAttributionChangedListener = new AttributionChangeListener(adjustConfig.attributionChangedDelegate);
			val3.Call("setOnAttributionChangedListener", new object[1] { onAttributionChangedListener });
		}
		if (adjustConfig.eventSuccessDelegate != null)
		{
			onEventTrackingSucceededListener = new EventTrackingSucceededListener(adjustConfig.eventSuccessDelegate);
			val3.Call("setOnEventTrackingSucceededListener", new object[1] { onEventTrackingSucceededListener });
		}
		if (adjustConfig.eventFailureDelegate != null)
		{
			onEventTrackingFailedListener = new EventTrackingFailedListener(adjustConfig.eventFailureDelegate);
			val3.Call("setOnEventTrackingFailedListener", new object[1] { onEventTrackingFailedListener });
		}
		if (adjustConfig.sessionSuccessDelegate != null)
		{
			onSessionTrackingSucceededListener = new SessionTrackingSucceededListener(adjustConfig.sessionSuccessDelegate);
			val3.Call("setOnSessionTrackingSucceededListener", new object[1] { onSessionTrackingSucceededListener });
		}
		if (adjustConfig.sessionFailureDelegate != null)
		{
			onSessionTrackingFailedListener = new SessionTrackingFailedListener(adjustConfig.sessionFailureDelegate);
			val3.Call("setOnSessionTrackingFailedListener", new object[1] { onSessionTrackingFailedListener });
		}
		if (adjustConfig.deferredDeeplinkDelegate != null)
		{
			onDeferredDeeplinkListener = new DeferredDeeplinkListener(adjustConfig.deferredDeeplinkDelegate);
			val3.Call("setOnDeeplinkResponseListener", new object[1] { onDeferredDeeplinkListener });
		}
		val3.Call("setSdkPrefix", new object[1] { "unity4.10.2" });
		((AndroidJavaObject)ajcAdjust).CallStatic("onCreate", new object[1] { val3 });
		((AndroidJavaObject)ajcAdjust).CallStatic("onResume", new object[0]);
	}

	public void trackEvent(AdjustEvent adjustEvent)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		AndroidJavaObject val = new AndroidJavaObject("com.adjust.sdk.AdjustEvent", new object[1] { adjustEvent.eventToken });
		double? revenue = adjustEvent.revenue;
		if (revenue.HasValue && adjustEvent.currency != null)
		{
			object[] array = new object[2];
			double? revenue2 = adjustEvent.revenue;
			array[0] = revenue2.Value;
			array[1] = adjustEvent.currency;
			val.Call("setRevenue", array);
		}
		if (adjustEvent.callbackList != null)
		{
			for (int i = 0; i < adjustEvent.callbackList.Count; i += 2)
			{
				string text = adjustEvent.callbackList[i];
				string text2 = adjustEvent.callbackList[i + 1];
				val.Call("addCallbackParameter", new object[2] { text, text2 });
			}
		}
		if (adjustEvent.partnerList != null)
		{
			for (int j = 0; j < adjustEvent.partnerList.Count; j += 2)
			{
				string text3 = adjustEvent.partnerList[j];
				string text4 = adjustEvent.partnerList[j + 1];
				val.Call("addPartnerParameter", new object[2] { text3, text4 });
			}
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("trackEvent", new object[1] { val });
	}

	public bool isEnabled()
	{
		return ((AndroidJavaObject)ajcAdjust).CallStatic<bool>("isEnabled", new object[0]);
	}

	public void setEnabled(bool enabled)
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("setEnabled", new object[1] { enabled });
	}

	public void setOfflineMode(bool enabled)
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("setOfflineMode", new object[1] { enabled });
	}

	public void sendFirstPackages()
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("sendFirstPackages", new object[0]);
	}

	public void setDeviceToken(string deviceToken)
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("setPushToken", new object[1] { deviceToken });
	}

	public static void addSessionPartnerParameter(string key, string value)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("addSessionPartnerParameter", new object[2] { key, value });
	}

	public static void addSessionCallbackParameter(string key, string value)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("addSessionCallbackParameter", new object[2] { key, value });
	}

	public static void removeSessionPartnerParameter(string key)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("removeSessionPartnerParameter", new object[1] { key });
	}

	public static void removeSessionCallbackParameter(string key)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("removeSessionCallbackParameter", new object[1] { key });
	}

	public static void resetSessionPartnerParameters()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("resetSessionPartnerParameters", new object[0]);
	}

	public static void resetSessionCallbackParameters()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		if (ajcAdjust == null)
		{
			ajcAdjust = new AndroidJavaClass("com.adjust.sdk.Adjust");
		}
		((AndroidJavaObject)ajcAdjust).CallStatic("resetSessionCallbackParameters", new object[0]);
	}

	public void onPause()
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("onPause", new object[0]);
	}

	public void onResume()
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("onResume", new object[0]);
	}

	public void setReferrer(string referrer)
	{
		((AndroidJavaObject)ajcAdjust).CallStatic("setReferrer", new object[1] { referrer });
	}

	public void getGoogleAdId(Action<string> onDeviceIdsRead)
	{
		DeviceIdsReadListener deviceIdsReadListener = new DeviceIdsReadListener(onDeviceIdsRead);
		((AndroidJavaObject)ajcAdjust).CallStatic("getGoogleAdId", new object[2] { ajoCurrentActivity, deviceIdsReadListener });
	}

	public string getIdfa()
	{
		return null;
	}
}
