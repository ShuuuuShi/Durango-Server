using System;
using NPA;
using SimpleJSON;
using UnityEngine;
using com.nexon.mpub;

public class ToyLoginHelper : INPListenerType, INPListener, INPBannerListener
{
	private enum LoginStep
	{
		None,
		TryEnterToy,
		TryLogin,
		TryGetUserInfo
	}

	public delegate void LoginFailedDelegate(string errorMsg, bool willQuit);

	public delegate void ResultDelegate(bool success);

	private static ToyLoginHelper _instance;

	private static bool IsToyEntered;

	public string ToyServiceId = string.Empty;

	private Action _onLoginSuccess;

	private LoginFailedDelegate _onLoginFailure;

	private ResultDelegate _logoutResult;

	private bool _loginFirstTime = true;

	private bool _loggedOut;

	private LoginStep _prevLoginStep;

	public static ToyLoginHelper Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new ToyLoginHelper();
			}
			return _instance;
		}
	}

	public static string NPSN { get; set; }

	public static string Token { get; set; }

	public static string NPA { get; set; }

	public static bool IsConnectFacebook { get; private set; }

	public static bool IsConnectGooglePlus { get; private set; }

	private ToyLoginHelper()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Invalid comparison between Unknown and I4
		TextAsset val = Resources.Load<TextAsset>("toy_service_id");
		if ((Object)(object)val != (Object)null)
		{
			ToyServiceId = val.text.Trim();
		}
		SystemLanguage systemLanguage = Application.systemLanguage;
		if ((int)systemLanguage == 23)
		{
			NPAccount.Instance.setLocale(NPLocale.KO_KR);
		}
		else
		{
			NPAccount.Instance.setLocale(NPLocale.EN_US);
		}
	}

	void INPBannerListener.OnBannerClick(string landInfo)
	{
	}

	void INPBannerListener.OnBannerFailed(NPResult npResult)
	{
	}

	void INPBannerListener.OnBannerDismiss()
	{
	}

	public static void Login(Action onSuccess, LoginFailedDelegate onFailure)
	{
		if (!IsToyEntered)
		{
			Instance.TryEnterToy(onSuccess, onFailure);
		}
		else
		{
			Instance.TryGetUserInfo(onSuccess, onFailure);
		}
	}

	private void PreLogout(ResultDelegate onResult)
	{
		NPSN = null;
		Token = null;
		NPA = null;
		_loginFirstTime = true;
		_logoutResult = onResult;
		_loggedOut = true;
	}

	public static void Logout(ResultDelegate onResult)
	{
		ToyLoginHelper instance = Instance;
		instance.PreLogout(onResult);
		NPAccount.Instance.Logout(instance);
	}

	public static void Leave(ResultDelegate onResult)
	{
		ToyLoginHelper instance = Instance;
		instance.PreLogout(onResult);
		NPAccount.Instance.UnregisterService(instance);
	}

	private void TryEnterToy(Action onSuccess, LoginFailedDelegate onFailure)
	{
		_onLoginSuccess = onSuccess;
		_onLoginFailure = onFailure;
		_prevLoginStep = LoginStep.TryEnterToy;
		NPAccount.Instance.EnterToy(this);
	}

	private void UnexpectedCondition(string output)
	{
		ToyFailure();
	}

	private void TryGetUserInfo(Action onSuccess = null, LoginFailedDelegate onFailure = null)
	{
		if (onSuccess != null)
		{
			_onLoginSuccess = onSuccess;
		}
		if (onFailure != null)
		{
			_onLoginFailure = onFailure;
		}
		_prevLoginStep = LoginStep.TryGetUserInfo;
		NPAccount.Instance.GetUserInfo(this);
	}

	private void LoginSuccess(string npsn, string npToken, string npa)
	{
		NPSN = npsn;
		Token = npToken;
		NPA = npa;
		XigncodeIntegration.SetUserInfo(npa);
		NXAds.instance.setUserId(npa);
		NPAccount instance = NPAccount.Instance;
		instance.getSnsConnectionStatus(this);
		if (_loginFirstTime)
		{
			_loginFirstTime = false;
		}
		LoginFinished();
	}

	private void LoginFinished()
	{
		if (_onLoginSuccess != null)
		{
			_onLoginSuccess();
		}
		_onLoginSuccess = null;
		_onLoginFailure = null;
	}

	private void ToyFailure(string errorMsg = null, bool willQuit = false)
	{
		if (_onLoginFailure != null)
		{
			_onLoginFailure(errorMsg, willQuit);
		}
		_onLoginSuccess = null;
		_onLoginFailure = null;
	}

	private void EnterToyResultHandler(int errorCode, NPResult npResult)
	{
		if (_prevLoginStep == LoginStep.TryEnterToy)
		{
			IsToyEntered = true;
			ShowBanner("1");
			if (errorCode == 0)
			{
				TryGetUserInfo();
			}
			else if (!AuthCrashFallbackToLogin(errorCode))
			{
				HandleToyError(errorCode, npResult);
			}
		}
	}

	private void LoginResultHandler(int errorCode, NPResult npResult)
	{
		if (_prevLoginStep != LoginStep.TryLogin)
		{
			UnexpectedCondition("LoginResult");
			return;
		}
		NPAccount instance = NPAccount.Instance;
		switch (errorCode)
		{
		case 0:
			KSingleton<GameManager>.Instance().PushNotification.Initialize();
			TryGetUserInfo();
			return;
		case 1301:
			instance.RecoverUser(this);
			break;
		case 1202:
			instance.ResolveAlreadyLoginedUser(this);
			break;
		}
		HandleToyError(errorCode, npResult);
	}

	private void SnsConnectionStateHandler(int errorCode, NPResult result)
	{
		IsConnectFacebook = false;
		IsConnectGooglePlus = false;
		if (errorCode != 0)
		{
			return;
		}
		JSONNode jSONNode = result.resultJson["result"]["list"];
		for (int i = 0; i < jSONNode.Count; i++)
		{
			JSONNode jSONNode2 = jSONNode[i];
			bool flag = jSONNode2["isConnect"].AsInt == 1;
			switch (jSONNode2["name"].Value)
			{
			case "facebook":
				IsConnectFacebook = flag;
				break;
			case "googleplus":
				IsConnectGooglePlus = flag;
				break;
			}
		}
	}

	private bool AuthCrashFallbackToLogin(int errorCode)
	{
		if (!NPAccount.Instance.isAuthCrashError(errorCode))
		{
			return false;
		}
		_prevLoginStep = LoginStep.TryLogin;
		if (_loggedOut)
		{
			_loggedOut = false;
			NPAccount.Instance.Login(this);
		}
		else
		{
			NPAccount.Instance.Login(NPLoginType.NPLOginTypeGameCenter, this);
		}
		return true;
	}

	private void GetUserInfoResultHandler(int errorCode, NPResult npResult)
	{
		if (_prevLoginStep != LoginStep.TryGetUserInfo)
		{
			UnexpectedCondition("GetUserInfoResult");
		}
		else if (errorCode == 0)
		{
			string npsn = npResult.resultJson["result"]["npsn"];
			string npToken = npResult.resultJson["result"]["npToken"];
			string npa = npResult.resultJson["result"]["npaCode"];
			LoginSuccess(npsn, npToken, npa);
		}
		else if (!AuthCrashFallbackToLogin(errorCode))
		{
			HandleToyError(errorCode, npResult);
		}
	}

	private void ShowBanner(string groupCode)
	{
		NPAccount.Instance.ShowBanner(groupCode, this);
	}

	private void HandleToyError(int errorCode, NPResult npResult)
	{
		if (npResult == null || npResult.resultJson != null)
		{
		}
		switch (errorCode)
		{
		case 92000:
			ToyFailure("#toy_error_permission_denied_app_will_close", willQuit: true);
			break;
		case -9:
			if (errorCode == -9)
			{
				ShowBanner("1");
			}
			ToyFailure("#toy_error_temporarily_down_for_maintenance_tap_to_quit", willQuit: true);
			break;
		default:
			ToyFailure();
			break;
		}
	}

	public void OnResult(NPResult npResult)
	{
		int asInt = npResult.resultJson["errorCode"].AsInt;
		switch (npResult.requestTag)
		{
		case NPRequestTypeTag.NPRequestTypeEnterToy:
			EnterToyResultHandler(asInt, npResult);
			break;
		case NPRequestTypeTag.NPRequestTypeLoginWithNX:
		case NPRequestTypeTag.NPRequestTypeLogin:
		case NPRequestTypeTag.NPRequestTypeLoginWithGameCenter:
		case NPRequestTypeTag.NPRequestTypeLoginWithNaver:
		case NPRequestTypeTag.NPRequestTypeLoginWithTwitter:
		case NPRequestTypeTag.NPRequestTypeLoginWithEmail:
		case NPRequestTypeTag.NPRequestTypeLoginWithNaverChannel:
		case NPRequestTypeTag.NPRequestTypeLoginWithKakao:
		case NPRequestTypeTag.NPRequestTypeLoginWithGPlus:
		case NPRequestTypeTag.NPRequestTypeLoginWithFB:
		case NPRequestTypeTag.NPRequestTypeLoginWithGuest:
			LoginResultHandler(asInt, npResult);
			break;
		case NPRequestTypeTag.NPRequestTypeGetSnsConnectionStatus:
			SnsConnectionStateHandler(asInt, npResult);
			break;
		case NPRequestTypeTag.NPRequestTypeLogout:
		case NPRequestTypeTag.NPRequestTypeUnregisterSVC:
		{
			bool flag = false;
			flag = asInt == 0 || (NPAccount.Instance.isAuthCrashError(asInt) ? true : false);
			if (_logoutResult != null)
			{
				_logoutResult(flag);
				_logoutResult = null;
			}
			break;
		}
		case NPRequestTypeTag.NPRequestTypeGetUserInfo:
			GetUserInfoResultHandler(asInt, npResult);
			break;
		}
	}
}
