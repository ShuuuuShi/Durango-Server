using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using L10N;
using MMT;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yaml.Util;

public class AuthMenuGroup : MonoBehaviour
{
	private enum State
	{
		Initial,
		LoginToy,
		ToyFailed,
		Knock,
		CheckL10NLoaded,
		CheckDataLoaded,
		NPAGetUser,
		GetUser,
		RequestCoupon,
		VeirfyCoupon,
		VeirfyCouponFailed,
		GetFrontend,
		TryConnect,
		Connecting,
		Welcome,
		FadeOutPrologue,
		FadeOutLoading,
		PrologueLoading,
		Loading,
		Evicted,
		ConnectError,
		Error
	}

	[SerializeField]
	private UILabel _explainLabel;

	[SerializeField]
	private UILabel _versionInfoLabel;

	[SerializeField]
	private Selectable _downloadNewVersionBtn;

	[SerializeField]
	private UITweener _loadingCurtain;

	[SerializeField]
	private GameObject _gatewaySelectionGroup;

	[SerializeField]
	private GameObject[] _emigratedObjects;

	[SerializeField]
	private GameObject[] _noneEmigratedObjects;

	[SerializeField]
	private MobileMovieTexture _movieTexture;

	[SerializeField]
	private AudioSource _bgmSound;

	[SerializeField]
	private GameObject _couponObject;

	[SerializeField]
	private UIInput _couponInput;

	[SerializeField]
	private PrerequsiteLoader _prerequsiteLoader;

	[SerializeField]
	private UIFontSetting _fontSetting;

	[SerializeField]
	private UILabel _debugLogLabel;

	[SerializeField]
	private UILabel _debugWarningLabel;

	[SerializeField]
	private UILabel _debugErrorLabel;

	private float _lastRequestTime = -1f;

	private string _downloadUrl = string.Empty;

	private string _userId = string.Empty;

	private string _sessionToken = string.Empty;

	private Dictionary<string, string> _sessionHeaders = new Dictionary<string, string>();

	private string _couponStr = string.Empty;

	private string _serverVersion;

	private string _errorMsg = string.Empty;

	private WWW _requestWWW;

	private int _connectIndex;

	private int _radiotowerConnectIndex;

	private int _connectAttempt;

	private JArray _addressList;

	private JArray _radiotowerAddressList;

	private State _curState;

	private bool _quitWhenToyErrorOccurred;

	private Loader _yamlLoader;

	private float _initialStateTime;

	public static bool StartByPrologue { get; set; }

	private string SessionToken
	{
		get
		{
			return _sessionToken;
		}
		set
		{
			_sessionToken = value;
			_sessionHeaders["Authorization"] = _sessionToken;
		}
	}

	private string ServerVersion
	{
		get
		{
			return _serverVersion;
		}
		set
		{
			_serverVersion = value;
			UpdateVersionInfo();
		}
	}

	private State CurState
	{
		get
		{
			return _curState;
		}
		set
		{
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0212: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Expected O, but got Unknown
			//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Expected O, but got Unknown
			_curState = value;
			if (_curState == State.Initial)
			{
				_initialStateTime = Time.realtimeSinceStartup;
			}
			float num = Time.realtimeSinceStartup - _initialStateTime;
			State state = _curState;
			State curState = _curState;
			_explainLabel.text = string.Empty;
			switch (_curState)
			{
			case State.Initial:
				_couponObject.SetActive(false);
				_userId = string.Empty;
				SessionToken = string.Empty;
				KSingleton<GameManager>.Instance().SetPlayerId("0");
				if (KSingleton<GameManager>.Instance().IsEvicted)
				{
					KSingleton<GameManager>.Instance().IsEvicted = false;
					state = State.Evicted;
				}
				else
				{
					state = State.LoginToy;
				}
				break;
			case State.LoginToy:
				SetExplainLabel("#title_get_account");
				ToyLoginHelper.Login(ToyLoginSucceed, ToyFailed);
				break;
			case State.Knock:
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("/knock");
				stringBuilder.Append("?version=");
				stringBuilder.Append(WWW.EscapeURL(CurrentBundleVersion.GetClientVersion()));
				stringBuilder.Append("&platform=");
				stringBuilder.Append(WWW.EscapeURL(((Enum)Application.platform).ToString()));
				((MonoBehaviour)this).StartCoroutine(CoRequestUrl(stringBuilder.ToString()));
				break;
			}
			case State.CheckL10NLoaded:
				if (!LocalizeSystem.LoadI18NFinished)
				{
					((MonoBehaviour)this).StartCoroutine(LocalizeSystem.LoadI18N());
				}
				break;
			case State.CheckDataLoaded:
				if ((Object)(object)_yamlLoader == (Object)null)
				{
					_yamlLoader = ((Component)this).gameObject.AddComponent<Loader>();
				}
				if (!_yamlLoader.IsFinished)
				{
					_yamlLoader.Load();
				}
				break;
			case State.NPAGetUser:
			{
				WWWForm val = new WWWForm();
				val.AddField("account_id", ToyLoginHelper.NPSN);
				val.AddField("account_provider", "toy");
				val.AddField("account_provider_id", ToyLoginHelper.Instance.ToyServiceId);
				val.AddField("token", ToyLoginHelper.Token);
				val.AddField("locale", LocalizeSystem.Locale);
				((MonoBehaviour)this).StartCoroutine(CoRequestUrl("/sessions", val));
				break;
			}
			case State.GetUser:
				((MonoBehaviour)this).StartCoroutine(CoRequestUrl("/users/" + _userId, null, auth: true));
				break;
			case State.RequestCoupon:
				_couponObject.SetActive(true);
				break;
			case State.VeirfyCoupon:
			{
				_couponObject.SetActive(false);
				WWWForm val2 = new WWWForm();
				val2.AddField("ticket", _couponStr);
				((MonoBehaviour)this).StartCoroutine(CoRequestUrl("/users/" + _userId + "/ticket", val2, auth: true));
				break;
			}
			case State.VeirfyCouponFailed:
				SetExplainLabel("#title_verify_coupon_error");
				break;
			case State.GetFrontend:
				((MonoBehaviour)this).StartCoroutine(CoRequestUrl("/entry?player_id=" + GameManager.PlayerId, null, auth: true));
				break;
			case State.TryConnect:
			{
				if (_addressList.Count == 0)
				{
					state = State.ConnectError;
					break;
				}
				if (_connectAttempt >= 3)
				{
					state = State.ConnectError;
					break;
				}
				_connectIndex %= _addressList.Count;
				JToken token = _addressList[_connectIndex];
				string @string = token.GetString();
				if (string.IsNullOrEmpty(@string) || !KUtility.TryParseHostPort(@string, out var host, out var port))
				{
					state = State.ConnectError;
					break;
				}
				string host2 = host;
				int port2 = 11010;
				if (_radiotowerAddressList != null && _radiotowerAddressList.Count != 0)
				{
					_radiotowerConnectIndex %= _radiotowerAddressList.Count;
					JToken token2 = _radiotowerAddressList[_radiotowerConnectIndex];
					string string2 = token2.GetString();
					if (!string.IsNullOrEmpty(string2) && !KUtility.TryParseHostPort(string2, out host2, out port2))
					{
						LogError("Cannot parse the Radiotower address: " + string2);
					}
				}
				KSingleton<GameManager>.Instance().TryConnect(host, port, host2, port2);
				state = State.Connecting;
				_connectIndex++;
				_radiotowerConnectIndex++;
				_connectAttempt++;
				break;
			}
			case State.Welcome:
				KSingleton<GameManager>.Instance().SendAuthMessage(delegate
				{
					CurState = State.FadeOutLoading;
				}, delegate(string error)
				{
					Connections.Frontend.Close(callClosedHandler: false);
					_errorMsg = error;
					CurState = State.ConnectError;
				});
				break;
			case State.FadeOutPrologue:
			case State.FadeOutLoading:
			{
				float waitTime = ((!KSingleton<GameManager>.Instance().IsEmigrated) ? 0f : 3f);
				((MonoBehaviour)this).StartCoroutine(CoBeginFadeout(waitTime));
				break;
			}
			case State.PrologueLoading:
				((MonoBehaviour)this).StartCoroutine(CoLoadingLevel("Prologue"));
				break;
			case State.Loading:
				((MonoBehaviour)this).StartCoroutine(CoLoadingLevel("Main"));
				break;
			case State.Evicted:
				_lastRequestTime = Time.time;
				SetExplainLabel("#title_evicted");
				break;
			case State.ConnectError:
			case State.Error:
				_lastRequestTime = Time.time;
				CheckStartByPrologue();
				SetExplainLabel((_curState != State.Error) ? T.N_("서버 접속 중 에러가 발생했습니다.\n화면을 터치 후 다시 시도해 주세요.") : T.N_("서버와의 통신 중 에러가 발생했습니다.\n화면을 터치 후 다시 시도해 주세요."));
				if (!string.IsNullOrEmpty(_errorMsg))
				{
					_explainLabel.text = _errorMsg;
					_errorMsg = string.Empty;
				}
				break;
			}
			if (state != curState)
			{
				CurState = state;
			}
		}
	}

	private void UpdateVersionInfo()
	{
		_versionInfoLabel.text = $"* NPA: {ToyLoginHelper.NPA} / Server: {ServerVersion}";
	}

	private void Awake()
	{
		ApplyEmigrationMode();
		CheckStartByPrologue();
		_downloadNewVersionBtn.Clicked = DownloadNewVersion_Clicked;
	}

	private IEnumerator Start()
	{
		if ((Object)(object)_fontSetting != (Object)null)
		{
			_fontSetting.Init();
		}
		while ((Object)(object)_gatewaySelectionGroup != (Object)null && _gatewaySelectionGroup.activeSelf)
		{
			yield return null;
		}
		CurState = State.Initial;
		yield return null;
	}

	private void SetExplainLabel(string key)
	{
		_explainLabel.text = LocalizeSystem.Get(key);
	}

	private void CheckStartByPrologue()
	{
		((Component)_loadingCurtain).gameObject.SetActive(StartByPrologue);
		if (StartByPrologue)
		{
			_bgmSound.Stop();
			_gatewaySelectionGroup.SetActive(false);
		}
		else if (!_bgmSound.isPlaying)
		{
			_bgmSound.Play();
			_gatewaySelectionGroup.SetActive(Debug.isDebugBuild && !KSingleton<GameManager>.Instance().IsEmigrated);
		}
		StartByPrologue = false;
	}

	private void ApplyEmigrationMode()
	{
		bool isEmigrated = KSingleton<GameManager>.Instance().IsEmigrated;
		for (int i = 0; i < _emigratedObjects.Length; i++)
		{
			_emigratedObjects[i].SetActive(isEmigrated);
		}
		for (int j = 0; j < _noneEmigratedObjects.Length; j++)
		{
			_noneEmigratedObjects[j].SetActive(!isEmigrated);
		}
		_movieTexture.Path = ((!isEmigrated) ? "Movie/title_movie_train.ogv" : "Movie/sailing.ogv");
	}

	private IEnumerator CoBeginFadeout(float waitTime)
	{
		yield return (object)new WaitForSeconds(waitTime);
		if (CurState == State.FadeOutLoading)
		{
			((Component)_prerequsiteLoader).gameObject.SetActive(true);
			_prerequsiteLoader.TotalCount = KSingleton<AssetBundleManager>.Instance().PrerequsitesCount;
			KSingleton<AssetBundleManager>.Instance().StartPrerequisiteLoading(_prerequsiteLoader.ProgressChanged, _prerequsiteLoader.DetailedProgressChanged, AssetBundleManager_PrerequisiteLoadingCompleted);
			_explainLabel.text = T._("게임의 구동에 필요한 추가 데이터를 다운로드 합니다.\n와이파이 환경이 아닐 때에는 데이터 요금이 부과 될 수 있습니다.");
		}
		else
		{
			ActiveFadeOutTweener();
		}
	}

	private void AssetBundleManager_PrerequisiteLoadingCompleted(bool succeed)
	{
		((Component)_prerequsiteLoader).gameObject.SetActive(false);
		if (succeed)
		{
			ActiveFadeOutTweener();
		}
		else
		{
			CurState = State.Error;
		}
	}

	private void ActiveFadeOutTweener()
	{
		if (((Component)_loadingCurtain).gameObject.activeSelf)
		{
			FadeOutFinished();
			return;
		}
		((Component)_loadingCurtain).gameObject.SetActive(true);
		_loadingCurtain.SetOnFinished(FadeOutFinished);
		_loadingCurtain.ResetToBeginning();
		_loadingCurtain.PlayForward();
	}

	private void FadeOutFinished()
	{
		switch (CurState)
		{
		case State.FadeOutPrologue:
			CurState = State.PrologueLoading;
			break;
		case State.FadeOutLoading:
			CurState = State.Loading;
			break;
		}
	}

	private IEnumerator CoLoadingLevel(string level)
	{
		AsyncOperation loadAsyncOp = SceneManager.LoadSceneAsync(level);
		loadAsyncOp.allowSceneActivation = false;
		while (loadAsyncOp.progress < 0.9f)
		{
			yield return null;
		}
		loadAsyncOp.allowSceneActivation = true;
	}

	private void Update()
	{
		bool flag = CurState == State.CheckDataLoaded || CurState == State.Welcome || CurState == State.CheckL10NLoaded;
		flag |= IsRequestWaiting();
		flag |= IsConnecting();
		if (flag | IsEmigratedWaiting())
		{
			int num = (int)(Time.time * 10f);
			num %= 5;
			if (CurState == State.CheckL10NLoaded)
			{
				SetExplainLabel("#title_check_data_load");
				if (LocalizeSystem.DownloadL10NState == LocalizeSystem.DownloadState.Failed)
				{
					CurState = State.Error;
				}
				else if (LocalizeSystem.LoadI18NFinished)
				{
					CurState = State.CheckDataLoaded;
				}
			}
			else if (CurState == State.CheckDataLoaded)
			{
				SetExplainLabel("#title_check_data_load");
				AssetBundleManager.Status currentStatus = KSingleton<AssetBundleManager>.Instance().CurrentStatus;
				if (currentStatus == AssetBundleManager.Status.Succeed && _yamlLoader.IsFinished)
				{
					if (Loader.LoadSucceed)
					{
						CurState = State.NPAGetUser;
					}
					else
					{
						_yamlLoader.Stop();
						CurState = State.Error;
					}
				}
				else if (currentStatus == AssetBundleManager.Status.Failed)
				{
					CurState = State.Error;
				}
			}
			else if (IsConnecting())
			{
				SetExplainLabel(T.N_("서버에 접속 중입니다."));
			}
			else if (IsEmigratedWaiting())
			{
				SetExplainLabel("#title_emigrated_waiting");
			}
			else
			{
				SetExplainLabel(T.N_("서버와 통신 중입니다."));
			}
			for (int i = 0; i < num; i++)
			{
				_explainLabel.text += ".";
			}
		}
		switch (CurState)
		{
		case State.LoginToy:
		case State.PrologueLoading:
		case State.Loading:
			return;
		case State.ToyFailed:
			if (Input.GetMouseButtonUp(0))
			{
				if (_quitWhenToyErrorOccurred)
				{
					Application.Quit();
				}
				else
				{
					CurState = State.LoginToy;
				}
			}
			return;
		case State.VeirfyCouponFailed:
			if (Input.GetMouseButtonUp(0))
			{
				CurState = State.RequestCoupon;
			}
			return;
		case State.Connecting:
			if (Connections.Frontend.Connected())
			{
				CurState = State.Welcome;
			}
			else if (!Connections.Frontend.IsAttemptingToConnect())
			{
				CurState = State.TryConnect;
			}
			return;
		case State.Evicted:
		case State.ConnectError:
		case State.Error:
			if (Input.GetMouseButtonUp(0))
			{
				CurState = State.Initial;
				_debugLogLabel.text = string.Empty;
				_debugWarningLabel.text = string.Empty;
				_debugErrorLabel.text = string.Empty;
			}
			return;
		}
		if (GetResponse(out var result))
		{
			if (result.Length > 0)
			{
				OnRequestSucceed(result);
			}
		}
		else
		{
			CurState = State.Error;
		}
	}

	private void OnRequestSucceed(string response)
	{
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		JObject jObject = KUtility.ParseJson<JObject>(response);
		if (jObject == null)
		{
			CurState = State.Error;
			return;
		}
		switch (CurState)
		{
		case State.Initial:
			break;
		case State.Knock:
			ServerVersion = jObject.Get<string>("server_version");
			if (!jObject.Get("compatible", defaultVal: false))
			{
				string text = jObject.Get<string>("download_url");
				if (text != null)
				{
					((Component)_downloadNewVersionBtn).gameObject.SetActive(true);
					SetExplainLabel("#title_get_new_version");
					((Component)_explainLabel).transform.localPosition = new Vector3(0f, -145f, 0f);
					_downloadUrl = text;
				}
				else
				{
					LogError("No download url");
					CurState = State.Error;
				}
			}
			else
			{
				string infoHolderPath = jObject.Get<string>("assetbundle_index_url");
				KSingleton<AssetBundleManager>.Instance().Initialize(infoHolderPath);
				CurState = State.CheckL10NLoaded;
				((Component)_explainLabel).transform.localPosition = new Vector3(0f, -145f, 0f);
			}
			break;
		case State.CheckL10NLoaded:
		case State.CheckDataLoaded:
			break;
		case State.NPAGetUser:
			_userId = jObject.Get<string>("user_id");
			SessionToken = jObject.Get<string>("session_token");
			if (string.IsNullOrEmpty(_userId) || string.IsNullOrEmpty(SessionToken))
			{
				CurState = State.Error;
				break;
			}
			KSingleton<GameManager>.Instance().SessionToken = SessionToken;
			CurState = State.GetUser;
			break;
		case State.GetUser:
			if (!(jObject.Get("player_entity_ids") is JArray jArray))
			{
				CurState = State.Error;
			}
			else if (jObject.Get("approved", defaultVal: false))
			{
				string text2 = GameManager.PlayerId.ToString();
				JToken token = ((jArray.Count <= 0) ? null : jArray[0]);
				for (int i = 1; i < jArray.Count; i++)
				{
					JToken jToken = jArray[i];
					if (jToken != null && jToken.GetString() == text2)
					{
						token = jToken;
						break;
					}
				}
				string @string = token.GetString();
				if (@string != null)
				{
					if (text2 != @string)
					{
						KSingleton<GameManager>.Instance().SetPlayerId(@string);
					}
					CurState = State.GetFrontend;
				}
				else
				{
					CurState = State.FadeOutPrologue;
				}
			}
			else
			{
				CurState = State.RequestCoupon;
			}
			break;
		case State.RequestCoupon:
			break;
		case State.VeirfyCoupon:
			CurState = ((!jObject.Get("success", defaultVal: false)) ? State.VeirfyCouponFailed : State.GetUser);
			break;
		case State.VeirfyCouponFailed:
			break;
		case State.GetFrontend:
			_addressList = jObject.Get("frontend_addresses") as JArray;
			if (_addressList == null || _addressList.Count == 0)
			{
				CurState = State.Error;
				break;
			}
			_connectIndex = Random.Range(0, _addressList.Count);
			_radiotowerAddressList = jObject.Get("radiotower_addresses") as JArray;
			if (_radiotowerAddressList != null)
			{
				_radiotowerConnectIndex = Random.Range(0, _radiotowerAddressList.Count);
			}
			_connectAttempt = 0;
			CurState = State.TryConnect;
			break;
		case State.TryConnect:
			break;
		case State.Connecting:
			break;
		case State.Loading:
			break;
		case State.LoginToy:
		case State.ToyFailed:
		case State.Welcome:
		case State.FadeOutPrologue:
		case State.FadeOutLoading:
		case State.PrologueLoading:
			break;
		}
	}

	private bool IsRequestWaiting()
	{
		return _requestWWW != null && !_requestWWW.isDone;
	}

	private bool IsConnecting()
	{
		return CurState == State.TryConnect || CurState == State.Connecting;
	}

	private bool IsEmigratedWaiting()
	{
		return CurState == State.FadeOutLoading && KSingleton<GameManager>.Instance().IsEmigrated;
	}

	private bool GetResponse(out string result)
	{
		result = string.Empty;
		if (_requestWWW == null)
		{
			return true;
		}
		try
		{
			string text = CheckError();
			if (!string.IsNullOrEmpty(text))
			{
				result = text;
				_requestWWW.Dispose();
				_requestWWW = null;
				return false;
			}
			if (_requestWWW.isDone)
			{
				result = _requestWWW.text;
				_requestWWW.Dispose();
				_requestWWW = null;
				return true;
			}
		}
		catch (Exception ex)
		{
			_requestWWW = null;
			result = ex.Message;
			return false;
		}
		return true;
	}

	private string CheckError()
	{
		string error = _requestWWW.error;
		if (string.IsNullOrEmpty(error))
		{
			return string.Empty;
		}
		JObject jObject = KUtility.ParseJson<JObject>(_requestWWW.text);
		if (jObject != null && jObject.Get("error") is JObject token)
		{
			_errorMsg = string.Format("[{0}] {1}\n{2}", token.Get<string>("code"), token.Get<string>("name"), token.Get<string>("message"));
		}
		return error;
	}

	private IEnumerator CoRequestUrl(string postFix, WWWForm form = null, bool auth = false)
	{
		if (auth)
		{
			_requestWWW = new WWW(KSingleton<GameManager>.Instance().GatewayUrl + postFix, (form == null) ? null : form.data, _sessionHeaders);
		}
		else if (form != null)
		{
			_requestWWW = new WWW(KSingleton<GameManager>.Instance().GatewayUrl + postFix, form);
		}
		else
		{
			_requestWWW = new WWW(KSingleton<GameManager>.Instance().GatewayUrl + postFix);
		}
		SetExplainLabel(T.N_("서버와 통신 중입니다."));
		yield return _requestWWW;
	}

	public void OnFinishIntro()
	{
		((MonoBehaviour)this).StartCoroutine(CoFinishIntro());
	}

	private IEnumerator CoFinishIntro()
	{
		UIWidget widget = ((Component)this).GetComponent<UIWidget>();
		if (Object.op_Implicit((Object)(object)widget))
		{
			while (widget.alpha < 1f)
			{
				widget.alpha += 0.1f;
				yield return (object)new WaitForSeconds(0.1f);
			}
		}
		CurState = State.Loading;
	}

	public void ToyLoginSucceed()
	{
		CurState = State.Knock;
	}

	public void ToyFailed(string errorMsg, bool willQuit)
	{
		CurState = State.ToyFailed;
		_quitWhenToyErrorOccurred = willQuit;
		SetExplainLabel((!string.IsNullOrEmpty(errorMsg)) ? errorMsg : "#title_toy_login_failed_tap_to_retry");
	}

	[Conditional("DEBUG_LEVEL_LOG")]
	private void Log(string text)
	{
		_debugLogLabel.text = text;
	}

	[Conditional("DEBUG_LEVEL_WARN")]
	[Conditional("DEBUG_LEVEL_LOG")]
	private void LogWarning(string text)
	{
		_debugWarningLabel.text = text;
	}

	private void LogError(string text)
	{
		if (Debug.isDebugBuild)
		{
			_debugErrorLabel.text = text;
		}
		Debug.LogError((object)text);
	}

	private void DownloadNewVersion_Clicked()
	{
		Application.OpenURL(_downloadUrl);
	}

	public void OnSubmitCoupon()
	{
		_couponStr = _couponInput.value.Replace("-", string.Empty);
		CurState = State.VeirfyCoupon;
	}

	public void OnChangeAuthMethod()
	{
		ToyLoginHelper.Logout(delegate
		{
			CurState = State.Initial;
		});
	}
}
