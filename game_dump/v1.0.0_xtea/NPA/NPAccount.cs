using System;
using System.Collections.Generic;
using System.Threading;
using SimpleJSON;
using UnityEngine;

namespace NPA;

public class NPAccount
{
	private class Nested
	{
		internal static readonly NPAccount instance;

		static Nested()
		{
			instance = new NPAccount();
		}
	}

	private const bool isDebug = false;

	public static string FRIEND_FILTER_TYPE_FRIENDS = "friends";

	public static string FRIEND_FILTER_TYPE_INVITES = "invites";

	public static string FRIEND_FILTER_TYPE_ALL = string.Empty;

	public static int LOCAL_PUSH_TYPE_ON;

	public static int LOCAL_PUSH_TYPE_AFTER = 1;

	public static int LOCAL_PUSH_TYPE_NOW = 2;

	public string GAMEOBJECT_NAME = "NPAccount";

	public GameObject mGameObject;

	public static string serviceID;

	private AndroidJavaObject account;

	private static int incrementCallIdValue = 1;

	public static NPAccount Instance => Nested.instance;

	private NPAccount()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Expected O, but got Unknown
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		mGameObject = new GameObject(GAMEOBJECT_NAME, new Type[1] { typeof(NPAccountGameObject) });
		Object.DontDestroyOnLoad((Object)(object)mGameObject);
		if ((int)Application.platform != 11)
		{
			return;
		}
		AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		try
		{
			AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
			try
			{
				ToyDebugLog("init android java object");
				if (serviceID == null)
				{
					account = new AndroidJavaObject("kr.co.nexon.npaccount.NPAccountForUnityObject", new object[1] { @static });
				}
				else
				{
					account = new AndroidJavaObject("kr.co.nexon.npaccount.NPAccountForUnityObject", new object[2] { @static, serviceID });
				}
			}
			finally
			{
				((IDisposable)@static)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void setPushListener(INPRecvNotificationListener listener)
	{
		mGameObject.SendMessage("setPushListener", (object)listener);
	}

	private void setGCMListener(INPGCMListener listener)
	{
		mGameObject.SendMessage("setGCMListener", (object)listener);
	}

	private void setListener(INPListenerType listener, string callId)
	{
		object[] array = new object[2] { listener, callId };
		mGameObject.SendMessage("setListener", (object)array);
	}

	public void callJavaMethod(string method, string args, string callId)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		if ((int)Application.platform != 11)
		{
			return;
		}
		AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		try
		{
			AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
			try
			{
				account.Call("ExecuteMethod", new object[4] { @static, method, args, callId });
			}
			finally
			{
				((IDisposable)@static)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void callIOSMethod(string method, string args, string callId)
	{
	}

	public bool isAuthCrashError(int errorCode)
	{
		switch (errorCode)
		{
		case 5001:
		case 5002:
		case 5003:
		case 90502:
		case 90707:
			callJavaMethod("initAccount", string.Empty, string.Empty);
			callIOSMethod("initAccount", string.Empty, string.Empty);
			return true;
		default:
			return false;
		}
	}

	public void pushInit(INPRecvNotificationListener pushListener, string senderID = "")
	{
		pushInit(pushListener, null, isSkip: false, senderID);
	}

	public void pushInit(INPRecvNotificationListener pushListener, INPGCMListener gcmListener, bool isSkip, string senderID = "")
	{
		setPushListener(pushListener);
		if (gcmListener != null)
		{
			setGCMListener(gcmListener);
		}
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsBool = isSkip;
		jSONArray[1] = senderID;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("gcmInit", text, string.Empty);
		callIOSMethod("apnsInit", string.Empty, string.Empty);
	}

	public void RegisterPush(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("registerPush", string.Empty, callId);
		callIOSMethod("registerPush", string.Empty, callId);
	}

	public void UnregisterPush(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("unregisterPush", string.Empty, callId);
		callIOSMethod("unregisterPush", string.Empty, callId);
	}

	public void Login(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("login", string.Empty, callId);
		callIOSMethod("loginWithParentViewController:completeBlock:", string.Empty, callId);
	}

	public void Login(NPLoginType loginType, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)loginType;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("login", text, callId);
		callIOSMethod("loginWithType:parentViewController:completeBlock:", text, callId);
	}

	public void LoginForKakao(string kakaoID, string accessToken, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = kakaoID;
		jSONArray[1] = accessToken;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("loginForKakao", text, callId);
		callIOSMethod("loginWithKakaoID:accessToken:parentViewController:completeBlock:", text, callId);
	}

	public void UnregisterService(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("unregisterService", string.Empty, callId);
		callIOSMethod("unregisterServiceWithCompleteBlock:", string.Empty, callId);
	}

	public void RecoverUser(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("recoverUser", string.Empty, callId);
		callIOSMethod("recoverUserWithCompleteBlock:", string.Empty, callId);
	}

	public void Logout(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("logout", string.Empty, callId);
		callIOSMethod("logoutWithCompleteBlock:", string.Empty, callId);
	}

	public void GetUserInfo(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getUserInfo", string.Empty, callId);
		callIOSMethod("getUserInfoWithCompleteBlock:", string.Empty, callId);
	}

	private bool isSnsType(NPLoginType loginType)
	{
		if (loginType >= NPLoginType.NPLoginTypeFaceBook && loginType <= NPLoginType.NPLoginTypeGooglePlus)
		{
			return true;
		}
		return false;
	}

	public void GetFriends(int next, string filterType, INPListener listener)
	{
		if (isSnsType(GetLoginType()))
		{
			GetFriends((NPSnsType)GetLoginType(), next, filterType, listener);
			return;
		}
		NPResult nPResult = new NPResult();
		nPResult.errorCode = 91005;
		nPResult.requestTag = NPRequestTypeTag.NPRequestTypeGetFriends;
		nPResult.resultJson = new JSONClass();
		nPResult.resultJson["errorCode"].AsInt = 91005;
		nPResult.resultJson["result"] = new JSONClass();
		listener.OnResult(nPResult);
	}

	public void GetFriends(NPSnsType snsType, int next, string filterType, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)snsType;
		jSONArray[1].AsInt = next;
		jSONArray[2] = filterType;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("getFriends", text, callId);
		callIOSMethod("getFriendsWithNextPage:type:filterType:completeBlock:", text, callId);
	}

	public void Share(string title, string content, string url)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = title;
		jSONArray[1] = content;
		jSONArray[2] = url;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("commonShare", text, string.Empty);
		callIOSMethod("shareWithParentViewController:title:content:url:", text, string.Empty);
	}

	public void ShowBanner(string groupCode, INPBannerListener bannerListener)
	{
		string callId = generateCallId();
		setListener(bannerListener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = groupCode;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showBanner", text, callId);
		callIOSMethod("showBannerWithParentViewController:groupCode:bannerClickBlock:failedBlock:dismissBannerBlock:", text, callId);
	}

	public void NXLogin(string id, string pw, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = id;
		jSONArray[1] = pw;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("NXLogin", text, callId);
		callIOSMethod("NXLoginWithParentViewController:userID:pw:completeBlock:", text, callId);
	}

	public void Post(string msg, string description, string link, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = msg;
		jSONArray[1] = description;
		jSONArray[2] = link;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("post", text, callId);
		callIOSMethod("post", text, callId);
	}

	public void PostImage(string msg, string imageURL, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = msg;
		jSONArray[1] = imageURL;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("postImage", text, callId);
		callIOSMethod("postImage", text, callId);
	}

	public void DismissEndingBanner()
	{
		callJavaMethod("dismissEndingBanner", string.Empty, string.Empty);
	}

	public void GetNexonSN(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getNexonSN", string.Empty, callId);
		callIOSMethod("getNexonSNWithViewController:completeBlock:", string.Empty, callId);
	}

	public NPLoginType GetLoginType()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		callJavaMethod("loadLoginType", string.Empty, string.Empty);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				return (NPLoginType)account.Get<int>("loginType");
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return NPLoginType.NPLoginTypeDefault;
	}

	public void GetLoginType(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getLoginType", string.Empty, callId);
		callIOSMethod("loginType", string.Empty, callId);
	}

	public void ShowNotice()
	{
		callJavaMethod("showNotice", string.Empty, string.Empty);
		callIOSMethod("showNotice", string.Empty, string.Empty);
	}

	public void ShowNotice(INPOnCloseListener closeListener)
	{
		string callId = generateCallId();
		setListener(closeListener, callId);
		callJavaMethod("showNotice", "useCloseListener", callId);
		callIOSMethod("showNotice", "useCloseBlock", callId);
	}

	public void ShowFAQ()
	{
		callJavaMethod("showFAQ", string.Empty, string.Empty);
		callIOSMethod("showFAQ", string.Empty, string.Empty);
	}

	public void ShowWeb(string title, string url)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = title;
		jSONArray[1] = url;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showWeb", text, string.Empty);
		callIOSMethod("showWeb", text, string.Empty);
	}

	public void ShowWeb(string title, string url, string postData)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = title;
		jSONArray[1] = url;
		if (postData == null)
		{
			jSONArray[2] = string.Empty;
		}
		else
		{
			jSONArray[2] = postData;
		}
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showWeb", text, string.Empty);
		callIOSMethod("showWeb", text, string.Empty);
	}

	public void ShowEventWeb(string url)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = url;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showEventWeb", text, string.Empty);
		callIOSMethod("showEventWeb", text, string.Empty);
	}

	public void ShowEndingBanner(INPEndingBannerListener endingBannerListener)
	{
		string callId = generateCallId();
		setListener(endingBannerListener, callId);
		callJavaMethod("showEndingBanner", string.Empty, callId);
	}

	public void ShowInputCoupon(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("showInputCoupon", string.Empty, callId);
		callIOSMethod("showInputCoupon", string.Empty, callId);
	}

	public void ShowCustomerService(NPCSInfo param)
	{
		JSONArray jSONArray = new JSONArray();
		string empty = string.Empty;
		if (param != null)
		{
			JSONClass jSONClass = new JSONClass();
			if (param.questionInfos != null)
			{
				JSONArray jSONArray2 = new JSONArray();
				for (int i = 0; i < param.questionInfos.Length; i++)
				{
					jSONArray2[i] = param.questionInfos[i];
				}
				jSONClass["questionInfos"] = jSONArray2;
			}
			foreach (KeyValuePair<string, string> item in param)
			{
				if (item.Value == null)
				{
					jSONClass[item.Key] = string.Empty;
				}
				else
				{
					jSONClass[item.Key] = item.Value;
				}
			}
			jSONArray[0] = jSONClass.ToString();
		}
		else
		{
			jSONArray[0] = string.Empty;
		}
		ToyDebugLog(empty);
		callJavaMethod("showCustomerService", empty, string.Empty);
		callIOSMethod("showCustomerServiceWithParentViewController", empty, string.Empty);
	}

	public void ShowCustomerService(NPCSInfo param, INPPlateListener plateListener)
	{
		ShowPlate(0, param, plateListener);
	}

	public void ShowHelpCenter(NPCSInfo param)
	{
		JSONArray jSONArray = new JSONArray();
		string empty = string.Empty;
		if (param != null)
		{
			JSONClass jSONClass = new JSONClass();
			if (param.questionInfos != null)
			{
				JSONArray jSONArray2 = new JSONArray();
				for (int i = 0; i < param.questionInfos.Length; i++)
				{
					jSONArray2[i] = param.questionInfos[i];
				}
				jSONClass["questionInfos"] = jSONArray2;
			}
			foreach (KeyValuePair<string, string> item in param)
			{
				if (item.Value == null)
				{
					jSONClass[item.Key] = string.Empty;
				}
				else
				{
					jSONClass[item.Key] = item.Value;
				}
			}
			jSONArray[0] = jSONClass.ToString();
		}
		else
		{
			jSONArray[0] = string.Empty;
		}
		ToyDebugLog(empty);
		callJavaMethod("showHelpCenter", empty, string.Empty);
		callIOSMethod("showHelpCenter", empty, string.Empty);
	}

	public void SendEvent(string category, string action, string label, string value)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = category;
		jSONArray[1] = action;
		jSONArray[2] = label;
		jSONArray[3] = value;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("sendEvent", text, string.Empty);
		callIOSMethod("sendEventWithCategory:action:label:value:", text, string.Empty);
	}

	public void SendScreen(string screenName)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = screenName;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("sendScreen", text, string.Empty);
		callIOSMethod("sendScreen:", text, string.Empty);
	}

	public void SendEcommerceTransaction(string transactionID, string affiliation, double revenue, double tax, double shipping, string currencyCode)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = transactionID;
		jSONArray[1] = affiliation;
		jSONArray[2].AsDouble = revenue;
		jSONArray[3].AsDouble = tax;
		jSONArray[4].AsDouble = shipping;
		jSONArray[5] = currencyCode;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("sendEcommerceTransaction", text, string.Empty);
		callIOSMethod("sendEcommerceTransactionWithId:affiliation:revenue:tax:shipping:currencyCode:", text, string.Empty);
	}

	public void SendEcommerceItem(string transactionID, string name, string sku, string category, double price, long quantity, string currencyCode)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = transactionID;
		jSONArray[1] = name;
		jSONArray[2] = sku;
		jSONArray[3] = category;
		jSONArray[4].AsDouble = price;
		jSONArray[5].AsInt = (int)quantity;
		jSONArray[6] = currencyCode;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("sendEcommerceItem", text, string.Empty);
		callIOSMethod("sendEcommerceItemWithTransactionId:name:sku:category:price:quantity:currencyCode:", text, string.Empty);
	}

	public void StartSession()
	{
		callJavaMethod("startSession", string.Empty, string.Empty);
		callIOSMethod("startSession", string.Empty, string.Empty);
	}

	public void EndSession()
	{
		callJavaMethod("endSession", string.Empty, string.Empty);
		callIOSMethod("endSession", string.Empty, string.Empty);
	}

	public void setCountry(NPCountry country)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)country;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("setCountry", text, string.Empty);
		callIOSMethod("setCountry", text, string.Empty);
	}

	public NPCountry getCountry()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		callJavaMethod("loadCountry", string.Empty, string.Empty);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				return (NPCountry)account.Get<int>("country");
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return NPCountry.UnitedStatesofAmerica;
	}

	public void setLocale(NPLocale locale)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)locale;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("setLocale", text, string.Empty);
		callIOSMethod("setLocale", text, string.Empty);
	}

	public NPLocale getLocale()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		callJavaMethod("loadLocale", string.Empty, string.Empty);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
				try
				{
					return (NPLocale)account.Get<int>("locale");
				}
				finally
				{
					((IDisposable)@static)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return NPLocale.EN_US;
	}

	public void dispatchLocalPush(NPNotificationData data)
	{
		JSONClass jSONClass = new JSONClass();
		JSONClass jSONClass2 = new JSONClass();
		jSONClass["notificationID"].AsInt = data.notificationID;
		jSONClass["message"] = data.message;
		jSONClass["meta"] = data.meta;
		jSONClass["pushType"].AsInt = data.pushType;
		jSONClass["badgeNumber"].AsInt = data.badgeNumber;
		jSONClass2["year"].AsInt = data.time.year;
		jSONClass2["month"].AsInt = data.time.month;
		jSONClass2["day"].AsInt = data.time.day;
		jSONClass2["hour"].AsInt = data.time.hour;
		jSONClass2["minute"].AsInt = data.time.minute;
		jSONClass2["sec"].AsInt = data.time.sec;
		jSONClass["time"] = jSONClass2;
		string text = jSONClass.ToString();
		ToyDebugLog(text);
		callJavaMethod("dispatchLocalPush", text, string.Empty);
		callIOSMethod("dispatchLocalPush", text, string.Empty);
	}

	public void cancelLocalPush(int notificationID)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = notificationID;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("cancelLocalPush", text, string.Empty);
		callIOSMethod("cancelLocalPush", text, string.Empty);
	}

	public void cancelAllLocalPush()
	{
		callJavaMethod("cancelAllLocalPush", string.Empty, string.Empty);
		callIOSMethod("cancelAllLocalPush", string.Empty, string.Empty);
	}

	public void showAchievement(INPListener gameServiceCloseListener)
	{
		string callId = generateCallId();
		setListener(gameServiceCloseListener, callId);
		callJavaMethod("showAchievement", string.Empty, callId);
		callIOSMethod("showAchievement", string.Empty, callId);
	}

	public void setStepsAchievement(string achievementID, int steps)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = achievementID;
		jSONArray[1].AsInt = steps;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("setStepsAchievement", text, string.Empty);
		callIOSMethod("setStepsAchievement", text, string.Empty);
	}

	public void setStepsAchievementImmediate(string achievementID, int steps, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = achievementID;
		jSONArray[1].AsInt = steps;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("setStepsAchievementImmediate", text, callId);
		callIOSMethod("setStepsAchievementImmediate", text, callId);
	}

	public void unlockAchievement(string achievementID)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = achievementID;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("unlockAchievement", text, string.Empty);
		callIOSMethod("unlockAchievement", text, string.Empty);
	}

	public void unlockAchievementImmediate(string achievementID, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = achievementID;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("unlockAchievementImmediate", text, callId);
		callIOSMethod("unlockAchievementImmediate", text, callId);
	}

	public void incrementAchievement(string achievementID, int increment)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = achievementID;
		jSONArray[1].AsInt = increment;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("incrementAchievement", text, string.Empty);
		callIOSMethod("incrementAchievement", text, string.Empty);
	}

	public void incrementAchievementImmediate(string achievementID, int increment, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = achievementID;
		jSONArray[1].AsInt = increment;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("incrementAchievementImmediate", text, callId);
		callIOSMethod("incrementAchievementImmediate", text, callId);
	}

	public void loadAchievementData(bool forceReload, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsBool = forceReload;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("loadAchievementData", text, callId);
		callIOSMethod("loadAchievementData", text, callId);
	}

	public void showAllLeaderBoard(INPListener gameServiceCloseListener)
	{
		string callId = generateCallId();
		setListener(gameServiceCloseListener, callId);
		callJavaMethod("showAllLeaderBoard", string.Empty, callId);
		callIOSMethod("showAllLeaderBoard", string.Empty, callId);
	}

	public void showLeaderBoard(string leaderBoardID, INPListener gameServiceCloseListener)
	{
		string callId = generateCallId();
		setListener(gameServiceCloseListener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = leaderBoardID;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showLeaderBoard", text, callId);
		callIOSMethod("showLeaderBoard", text, callId);
	}

	public void submitScore(string leaderBoardID, long score)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = leaderBoardID;
		jSONArray[1] = score.ToString();
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("submitScore", text, string.Empty);
		callIOSMethod("submitScore", text, string.Empty);
	}

	public void submitScoreImmediate(string leaderBoardID, long score, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = leaderBoardID;
		jSONArray[1] = score.ToString();
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("submitScoreImmediate", text, callId);
		callIOSMethod("submitScoreImmediate", text, callId);
	}

	public void loadCurrentPlayerLeaderboardScore(string leaderBoardID, int span, int leaderboardCollection, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = leaderBoardID;
		jSONArray[1].AsInt = span;
		jSONArray[2].AsInt = leaderboardCollection;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("loadCurrentPlayerLeaderboardScore", text, callId);
		callIOSMethod("loadCurrentPlayerLeaderboardScore", text, callId);
	}

	public void ConnectGamePlatform(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("connectGamePlatform", string.Empty, callId);
		callIOSMethod("connectGamePlatform", string.Empty, callId);
	}

	public void DisconnectGamePlatform(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("disconnectGamePlatform", string.Empty, callId);
		callIOSMethod("disconnectGamePlatform", string.Empty, callId);
	}

	public void LogoutGamePlatform(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("logoutGamePlatform", string.Empty, callId);
	}

	public bool IsEnableGamePlatform()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		callJavaMethod("isEnableGamePlatform", string.Empty, string.Empty);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
				try
				{
					return account.Get<bool>("isEnableGamePlatform");
				}
				finally
				{
					((IDisposable)@static)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return false;
	}

	public void ScreenCapture(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("screenCapture", string.Empty, callId);
	}

	public void GetPlayerStats(bool forceReload, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsBool = forceReload;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("getPlayerStats", text, callId);
	}

	public void ShowPlate(NPCSInfo param)
	{
		ShowPlate(0, param, null);
	}

	public void ShowPlate(int group, NPCSInfo param)
	{
		ShowPlate(group, param, null);
	}

	public void ShowPlate(int group, NPCSInfo param, INPPlateListener plateListener)
	{
		string callId = generateCallId();
		setListener(plateListener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = group;
		string empty = string.Empty;
		if (param != null)
		{
			JSONClass jSONClass = new JSONClass();
			if (param.questionInfos != null)
			{
				JSONArray jSONArray2 = new JSONArray();
				for (int i = 0; i < param.questionInfos.Length; i++)
				{
					jSONArray2[i] = param.questionInfos[i];
				}
				jSONClass["questionInfos"] = jSONArray2;
			}
			foreach (KeyValuePair<string, string> item in param)
			{
				if (item.Value == null)
				{
					jSONClass[item.Key] = string.Empty;
				}
				else
				{
					jSONClass[item.Key] = item.Value;
				}
			}
			jSONArray[1] = jSONClass.ToString();
		}
		else
		{
			jSONArray[1] = string.Empty;
		}
		empty = jSONArray.ToString();
		ToyDebugLog(empty);
		callJavaMethod("showPlate", empty, callId);
		callIOSMethod("showPlateWithParentViewController:group:params:plateActionPerformedResultBlock", empty, callId);
	}

	public void ShowTermsOfAgree(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("showTermsOfAgree", string.Empty, callId);
		callIOSMethod("showTermsOfAgree", string.Empty, callId);
	}

	public void snsConnect(NPSnsType snsType, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)snsType;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("snsConnect", text, callId);
		callIOSMethod("snsConnect", text, callId);
	}

	public void snsDisconnect(NPSnsType snsType, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)snsType;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("snsDisconnect", text, callId);
		callIOSMethod("snsDisconnect", text, callId);
	}

	public void getSnsConnectionStatus(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getSnsConnectionStatus", string.Empty, callId);
		callIOSMethod("getSnsConnectionStatus", string.Empty, callId);
	}

	public void getSnsUserInfo(NPSnsType snsType, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)snsType;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("getSnsUserInfo", text, callId);
		callIOSMethod("getSnsUserInfo", text, callId);
	}

	public void getSnsTokenList(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getSnsTokenList", string.Empty, callId);
		callIOSMethod("getSnsTokenList", string.Empty, callId);
	}

	public void GetCountryFromServer(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getCountryFromServer", string.Empty, callId);
		callIOSMethod("getCountryFromServer", string.Empty, callId);
	}

	public string getUUID()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		callJavaMethod("loadUUID", string.Empty, string.Empty);
		if ((int)Application.platform == 11)
		{
			AndroidJavaClass val = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			try
			{
				AndroidJavaObject @static = ((AndroidJavaObject)val).GetStatic<AndroidJavaObject>("currentActivity");
				try
				{
					return account.Get<string>("uuid");
				}
				finally
				{
					((IDisposable)@static)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		return string.Empty;
	}

	public void ShowDataBackup(INPListener listener)
	{
		ShowDataBackup(string.Empty, listener);
	}

	public void ShowDataBackup(string title, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = title;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showDataBackup", text, callId);
		callIOSMethod("showDataBackup", text, callId);
	}

	public void ShowDataRestore(INPListener listener)
	{
		ShowDataRestore(string.Empty, listener);
	}

	public void ShowDataRestore(string title, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = title;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showDataRestore", text, callId);
		callIOSMethod("showDataRestore", text, callId);
	}

	public void SetDisableLoginTypes(int[] loginTypes)
	{
		JSONArray jSONArray = new JSONArray();
		string text;
		if (loginTypes == null)
		{
			text = "[]";
		}
		else
		{
			for (int i = 0; i < loginTypes.Length; i++)
			{
				jSONArray[i].AsInt = loginTypes[i];
			}
			text = jSONArray.ToString();
		}
		ToyDebugLog(text);
	}

	public void ResolveAlreadyLoginedUser(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("resolveAlreadyLoginedUser", string.Empty, callId);
		callIOSMethod("resolveAlreadyLoginedUser", string.Empty, callId);
	}

	public void EnterToy(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("enterToy", string.Empty, callId);
		callIOSMethod("enterToyWithParentViewController", string.Empty, callId);
	}

	public void ShowSettlementFund(string itemName, string itemPrice, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = itemName;
		jSONArray[1] = itemPrice;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showSettlementFund", text, callId);
		callIOSMethod("showSettlementFundWithParentViewController", text, callId);
	}

	public void ShowPushNSms(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("showPushNSmsSetting", string.Empty, callId);
		callIOSMethod("showPushNSmsSetting:completeBlock", string.Empty, callId);
	}

	public void GetServiceInfo(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getServiceInfo", string.Empty, callId);
		callIOSMethod("getServiceInfoWithCompleteBlock", string.Empty, callId);
	}

	public void GetAdvertisingId(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("getAdvertisingId", string.Empty, callId);
		callIOSMethod("getAdvertisingIdWithCompleteBlock", string.Empty, callId);
	}

	public void BillingRequestProducts(List<string> productIds, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		JSONArray jSONArray2 = new JSONArray();
		if (productIds != null)
		{
			for (int i = 0; i < productIds.Count; i++)
			{
				jSONArray2[i] = productIds[i];
			}
		}
		jSONArray[0] = jSONArray2;
		string args = jSONArray.ToString();
		callJavaMethod("billingRequestProducts", args, callId);
		callIOSMethod("billingRequestProductsWithProductIds:completionBlock:", args, callId);
	}

	public void BillingPayment(string productId, Dictionary<string, string> meta, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		if (productId != null)
		{
			jSONArray[0] = productId;
		}
		else
		{
			jSONArray[0] = string.Empty;
		}
		if (meta != null)
		{
			jSONArray[1] = dicToJSON(meta).ToString();
		}
		else
		{
			jSONArray[1] = "{}";
		}
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("billingPayment", text, callId);
		callIOSMethod("billingRequestPaymentWithProductId:meta:completionBlock:", text, callId);
	}

	public void BillingRestore(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("billingRestore", string.Empty, callId);
		callIOSMethod("billingRestoreWithCompleteBlock:", string.Empty, callId);
	}

	public void BillingFinish(string stampToken, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		if (stampToken != null)
		{
			jSONArray[0] = stampToken;
		}
		else
		{
			jSONArray[0] = string.Empty;
		}
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("billingFinish", text, callId);
		callIOSMethod("billingFinishWithStampToken:completeBlock:", text, callId);
	}

	private JSONClass dicToJSON(Dictionary<string, string> dic)
	{
		JSONClass jSONClass = new JSONClass();
		foreach (KeyValuePair<string, string> item in dic)
		{
			if (item.Value == null)
			{
				jSONClass[item.Key] = string.Empty;
			}
			else
			{
				jSONClass[item.Key] = item.Value;
			}
		}
		return jSONClass;
	}

	public void FBLogPurchase(double purchaseAmount, string currency, Dictionary<string, string> param)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = purchaseAmount.ToString();
		jSONArray[1] = currency;
		if (param != null && param.Count > 0)
		{
			jSONArray[2] = dicToJSON(param).ToString();
		}
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("fbLogPurchase", text, string.Empty);
		callIOSMethod("fbLogPurchase", text, string.Empty);
	}

	public void FBLogEvent(string eventName)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = eventName;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("fbLogEvent", text, string.Empty);
		callIOSMethod("fbLogEvent", text, string.Empty);
	}

	public void FBLogEvent(string eventName, Dictionary<string, string> param)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = eventName;
		if (param != null && param.Count > 0)
		{
			jSONArray[1] = dicToJSON(param).ToString();
		}
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("fbLogEvent", text, string.Empty);
		callIOSMethod("fbLogEvent", text, string.Empty);
	}

	public void FBLogEvent(string eventName, double valueToSum)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = eventName;
		jSONArray[1] = valueToSum.ToString();
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("fbLogEvent", text, string.Empty);
		callIOSMethod("fbLogEvent", text, string.Empty);
	}

	public void FBLogEvent(string eventName, double valueToSum, Dictionary<string, string> param)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = eventName;
		jSONArray[1] = valueToSum.ToString();
		if (param != null && param.Count > 0)
		{
			jSONArray[2] = dicToJSON(param).ToString();
		}
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("fbLogEvent", text, string.Empty);
		callIOSMethod("fbLogEvent", text, string.Empty);
	}

	public void FBActivateApp()
	{
		callJavaMethod("fbActivateApp", string.Empty, string.Empty);
		callIOSMethod("fbActivateApp", string.Empty, string.Empty);
	}

	public void FBDeactivateApp()
	{
		callJavaMethod("fbDeactivateApp", string.Empty, string.Empty);
		callIOSMethod("fbDeactivateApp", string.Empty, string.Empty);
	}

	public void FBFetchDeferredAppLink(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("fbFetchDeferredAppLink", string.Empty, callId);
		callIOSMethod("fbFetchDeferredAppLink", string.Empty, callId);
	}

	public void FBAppInvite(string appLinkUrl, string previewImageUrl, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = appLinkUrl;
		jSONArray[1] = previewImageUrl;
		string args = jSONArray.ToString();
		callJavaMethod("fbAppInvite", args, callId);
		callIOSMethod("fbAppInvite", args, callId);
	}

	public void FBShare(string title, string description, string contentUrl, string imageUrl, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = title;
		jSONArray[1] = description;
		if (contentUrl == null)
		{
			jSONArray[2] = string.Empty;
		}
		else
		{
			jSONArray[2] = contentUrl;
		}
		if (imageUrl == null)
		{
			jSONArray[3] = string.Empty;
		}
		else
		{
			jSONArray[3] = imageUrl;
		}
		string args = jSONArray.ToString();
		callJavaMethod("fbShare", args, callId);
		callIOSMethod("fbShare", args, callId);
	}

	public void FBGetFriends(int nextPage, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = nextPage;
		string args = jSONArray.ToString();
		callJavaMethod("fbGetFriends", args, callId);
		callIOSMethod("fbGetFriends", args, callId);
	}

	public void FBSetIsDebugEnabled(bool enabled)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsBool = enabled;
		string args = jSONArray.ToString();
		callJavaMethod("fbSetIsDebugEnabled", args, string.Empty);
		callIOSMethod("fbSetIsDebugEnabled", args, string.Empty);
	}

	public void ResetBadgeCount(int count)
	{
	}

	public void SetPushAgreement(NPPushAgreementType agreeStatus, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = (int)agreeStatus;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("setPushAgreement", text, callId);
		callIOSMethod("setPushAgreementWithPushStatus:completeBlock", text, callId);
	}

	public void RequestPermissions(List<string> permissions, int requestCode, string rationaleMsg, INPRuntimePermissionListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		JSONArray jSONArray2 = new JSONArray();
		if (permissions != null)
		{
			for (int i = 0; i < permissions.Count; i++)
			{
				jSONArray2[i] = permissions[i];
			}
		}
		jSONArray[0] = jSONArray2;
		jSONArray[1].AsInt = requestCode;
		if (rationaleMsg != null)
		{
			jSONArray[2] = rationaleMsg;
		}
		else
		{
			jSONArray[2] = string.Empty;
		}
		string args = jSONArray.ToString();
		callJavaMethod("requestPermissions", args, callId);
	}

	public void GetPromotion(string placementId, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = placementId;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("getPromotion", text, callId);
		callIOSMethod("getPromotionWithPlacementId:completeBlock", text, callId);
	}

	public void ShowPromotion(string placementId, INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		JSONArray jSONArray = new JSONArray();
		jSONArray[0] = placementId;
		string text = jSONArray.ToString();
		ToyDebugLog(text);
		callJavaMethod("showPromotion", text, callId);
		callIOSMethod("showPromotionWithPlacementId:viewController:completeBlock", text, callId);
	}

	public void ShowAccountMenu(INPListener listener)
	{
		string callId = generateCallId();
		setListener(listener, callId);
		callJavaMethod("showAccountMenu", string.Empty, callId);
		callIOSMethod("showAccountMenuWithViewController:", string.Empty, callId);
	}

	public void ShowToday(int groupCode)
	{
		JSONArray jSONArray = new JSONArray();
		jSONArray[0].AsInt = groupCode;
		string args = jSONArray.ToString();
		callJavaMethod("showToday", args, string.Empty);
		callIOSMethod("showTodayWithParentViewController:groupCode:", args, string.Empty);
	}

	private string generateCallId()
	{
		int num = Interlocked.Increment(ref incrementCallIdValue);
		return "Id" + num;
	}

	public static void ToyDebugLog(string text)
	{
	}
}
