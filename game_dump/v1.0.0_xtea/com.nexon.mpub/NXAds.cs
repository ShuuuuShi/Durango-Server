using System;
using UnityEngine;

namespace com.nexon.mpub;

public class NXAds : MonoBehaviour
{
	public enum EventType
	{
		ContentDownloadStart,
		ContentDownloadEnd,
		ToyLogin,
		Registration,
		Tutorial,
		CheckPoint,
		FirstBattle,
		ChallengeStart
	}

	private static NXAds _instance;

	private string tagString = "NXAds Unity message : ";

	private static bool created;

	private string androidPluginClass = "com.nexon.mpub.ads.NXAdsUnityPlugin";

	private bool enableDebug => DebugMode;

	public bool DebugMode => false;

	public static NXAds instance => _instance;

	private void Awake()
	{
		if (created)
		{
			Object.Destroy((Object)(object)((Component)((Component)this).transform).gameObject);
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)((Component)((Component)this).transform).gameObject);
		_instance = this;
		init();
		created = true;
	}

	public void init()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		NXLog("start");
		sendDebugEnableToNative(enableDebug);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass(androidPluginClass);
			try
			{
				((AndroidJavaObject)val).CallStatic("onCreate", new object[0]);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void setUserId(string userId)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		NXLog("setUserId : " + userId);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass(androidPluginClass);
			try
			{
				((AndroidJavaObject)val).CallStatic("setUserId", new object[1] { userId });
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void trackingEvent(EventType eventType)
	{
		trackingEvent(eventType.ToString());
	}

	public void trackingEvent(string eventName)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		NXLog("TrackingEvent : " + eventName);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass(androidPluginClass);
			try
			{
				((AndroidJavaObject)val).CallStatic("trackingEvent", new object[1] { eventName });
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void trackingPurchase(string itemId, double itemPrice, string currency)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Invalid comparison between Unknown and I4
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		NXLog("TrackingPurchase : " + itemId + " / " + itemPrice + " / " + currency);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass(androidPluginClass);
			try
			{
				object[] array = new object[3] { itemId, itemPrice, currency };
				((AndroidJavaObject)val).CallStatic("trackingPurchase", array);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	private void sendDebugEnableToNative(bool enable)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		AndroidJavaClass val = new AndroidJavaClass(androidPluginClass);
		try
		{
			((AndroidJavaObject)val).CallStatic("setDebugMode", new object[1] { enable });
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void NXLog(string logText)
	{
		if (DebugMode && string.IsNullOrEmpty(logText))
		{
		}
	}
}
