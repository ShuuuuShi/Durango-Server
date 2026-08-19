using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using ExploreData;
using Holoville.HOTween;
using K1Network;
using L10N;
using Messages;
using SimpleJSON;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yaml.Util;

public class GameManager : KSingleton<GameManager>
{
	[SerializeField]
	private string _gatewayURL;

	private string _lastHost;

	private int _lastPort;

	private bool _isDisabled;

	private bool _started;

	private bool _isReady;

	private bool _isReconnecting;

	private PushNotification _pushNotification;

	public bool ForceMoveToTitle { get; set; }

	public static ulong PlayerId { get; private set; }

	public string SessionToken { get; set; }

	public string GatewayUrl
	{
		get
		{
			return _gatewayURL;
		}
		set
		{
			_gatewayURL = value;
		}
	}

	public static bool IsPrologueMode { get; private set; }

	public static bool IsPvPEnabled => false;

	public bool IsEmigrated { get; set; }

	public bool IsEvicted { get; set; }

	public ExploreData.Region Region { get; private set; }

	public PushNotification PushNotification => (_pushNotification == null) ? (_pushNotification = new PushNotification()) : _pushNotification;

	public event Action MainSceneLoaded;

	public event Action Ready;

	public event Action MainSceneClosed;

	public event Action<Dictionary<string, byte[]>> StorageLoaded;

	public event Action PreReconnect;

	public event Action PostReconnect;

	public void SetPlayerId(string strId)
	{
		if (string.IsNullOrEmpty(strId) || !ulong.TryParse(strId, out var result))
		{
			Debug.LogError((object)("Cannot set player_id - " + strId));
		}
		else
		{
			PlayerId = result;
		}
	}

	protected override bool CheckDontDestroyOnLoad()
	{
		return true;
	}

	public string MakeGatewayUrl(string urlPostfix)
	{
		return GatewayUrl + urlPostfix;
	}

	protected override void OnAwake()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		IsPrologueMode = IsSceneName("Prologue");
		if (Used)
		{
			Application.logMessageReceived += new LogCallback(LogCallback);
			ErrorReporter.Initialize();
			Application.runInBackground = true;
			Screen.sleepTimeout = -1;
			HOTween.Init(false, false, true);
			HOTween.EnableOverwriteManager(false);
			DeviceInfo.Init();
			GameSystem<OptionSystem>.Instance();
			CheckLocale();
			PushNotification.Initialize();
			AndroidKeyboardManager.Install();
		}
	}

	private void CheckLocale()
	{
		if (LocalizeSystem.IsLocaleNotLoadedYet)
		{
			string systemLocale = GetSystemLocale();
			SetLocale(systemLocale);
		}
	}

	private static string GetSystemLocale()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		SystemLanguage systemLanguage = Application.systemLanguage;
		if ((int)systemLanguage == 23)
		{
			return "ko_KR";
		}
		return "en_US";
	}

	public static bool IsMainScene()
	{
		return IsSceneName("Main");
	}

	private static bool IsSceneName(string sceneName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Scene activeScene = SceneManager.GetActiveScene();
		return ((Scene)(ref activeScene)).name == sceneName;
	}

	private static void LogCallback(string log, string stack, LogType type)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Debug.isDebugBuild && (Object)(object)Console.Instance != (Object)null)
		{
			Console.Instance.Print(string.Concat(type, ": ", log));
			Console.Instance.Print(stack);
		}
		if ((int)type == 0 || (int)type == 4)
		{
			ErrorReporter.HandleLog(log, stack, DateTime.Now, type);
		}
	}

	private void Start()
	{
		_started = true;
		SessionToken = string.Empty;
		Connections.Frontend.On<Evicted>(delegate
		{
			ForceMoveToTitle = true;
			IsEvicted = true;
		});
		Connections.Frontend.On<Error>(DefaultErrorHandler);
		Connections.Frontend.On<OK>(DefaultOKHandler);
		Connections.Radiotower.On<Error>(DefaultErrorHandler);
		Connections.Radiotower.On<OK>(DefaultOKHandler);
		Connections.Frontend.ConnetionClosed += Frontend_ConnectionClosed;
		Connections.Frontend.On<Emigrated>(EmigratedReceived);
		Connections.Frontend.On<PushFallback>(PushFallbackMsgReceived);
		Connections.Frontend.On(delegate(VerifyXigncode msg, PacketHeader header)
		{
			Connections.Frontend.Send(new XigncodeCookie
			{
				Cookie = XigncodeIntegration.GetCookie(msg.Seed)
			});
		});
		bool flag = !IsPrologueMode;
		if (flag)
		{
			this.MainSceneLoaded = null;
			this.Ready = null;
			this.MainSceneClosed = null;
			this.StorageLoaded = null;
			this.PreReconnect = null;
			this.PostReconnect = null;
		}
		GameSystemUtil.InstantiateGameSystem(flag);
	}

	private void PushFallbackMsgReceived(PushFallback msg, PacketHeader header)
	{
		JSONNode recvNotification = JSON.Parse(msg.JsonData);
		PushNotification.OnRecvNotification(recvNotification);
	}

	public static void DefaultErrorHandler(Error msg, PacketHeader header)
	{
		UIManager.SystemMsg(msg.Text, 4f);
	}

	public static void DefaultInfoHandler(Info msg, PacketHeader header)
	{
		UIManager.SystemMsg(msg.Text, 4f);
	}

	public static void DefaultOKHandler(OK msg, PacketHeader header)
	{
	}

	private void Update()
	{
		Connections.Frontend.Process();
		Connections.Radiotower.Process();
		Connections.Radiotower.MaybeSendKeepalive();
	}

	private void OnDisable()
	{
		if (Used)
		{
			_isDisabled = true;
			Connections.Frontend.Close();
			Connections.Radiotower.Close();
		}
	}

	private void OnLevelWasLoaded(int level)
	{
		if (!Used)
		{
			return;
		}
		if (IsMainScene())
		{
			if (this.MainSceneLoaded != null)
			{
				this.MainSceneLoaded();
			}
			Connections.Frontend.ForceSyncClock();
			IsEmigrated = false;
		}
		else if (IsSceneName("Title") && _started)
		{
			Start();
		}
	}

	public void SendAuthMessage(Action succeed, Action<string> failed)
	{
		Auth auth = default(Auth);
		auth.EntityId = PlayerId;
		auth.SessionToken = SessionToken;
		auth.ClientVersion = CurrentBundleVersion.GetClientVersion();
		auth.DeviceModel = SystemInfo.deviceModel;
		Auth msg = auth;
		Connections.Frontend.Send(msg).On(delegate(Welcome welcome, PacketHeader header)
		{
			Region = new ExploreData.Region(welcome.Region);
			GameSystem<PlayGuideSystem>.Instance().Initialize(welcome.Region.Role, welcome.Storage.Data);
			if (this.StorageLoaded != null)
			{
				this.StorageLoaded(welcome.Storage.Data);
			}
			TerrainMeta.Init();
			TerrainMeta.Load(Region.TerrainId, Region.Role(), succeed, failed);
		}).On(delegate(Error error, PacketHeader header)
		{
			if (failed != null)
			{
				failed(error.Text);
			}
		});
	}

	public void SendReady()
	{
		Connections.Frontend.Send(default(Ready)).On<OK>(delegate
		{
			_isReady = true;
			if (this.Ready != null)
			{
				this.Ready();
			}
		});
	}

	public void AddOnReady(Action action)
	{
		if (_isReady)
		{
			action();
			return;
		}
		this.Ready = (Action)Delegate.Remove(this.Ready, action);
		this.Ready = (Action)Delegate.Combine(this.Ready, action);
	}

	public void TryConnect(string host, int port, string radiotowerHost, int radiotowerPort)
	{
		if (!string.IsNullOrEmpty(radiotowerHost))
		{
			SocialSystem socialSystem = GameSystem<SocialSystem>.Instance();
			socialSystem.Host = radiotowerHost;
			socialSystem.Port = radiotowerPort;
		}
		_lastHost = host;
		_lastPort = port;
		((MonoBehaviour)this).StartCoroutine(CoConnect(host, port));
	}

	public void ForceMainSceneLoadedPrologue()
	{
		if (this.MainSceneLoaded != null)
		{
			this.MainSceneLoaded();
		}
	}

	private void Frontend_ConnectionClosed()
	{
		if (!_isDisabled)
		{
			try
			{
				Connections.Radiotower.Close();
			}
			catch (Exception)
			{
			}
			bool flag = IsEmigrated || ForceMoveToTitle || !IsMainScene();
			ForceMoveToTitle = false;
			_isReady = false;
			LoadingCurtainGroup loadingCurtainGroup = ((!flag) ? UIManager.FindScript<LoadingCurtainGroup>() : null);
			if ((Object)(object)loadingCurtainGroup == (Object)null || _isReconnecting)
			{
				MoveToTitleLevel();
				return;
			}
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(Reconnect(loadingCurtainGroup));
		}
	}

	private IEnumerator Reconnect(LoadingCurtainGroup loadingCurtain)
	{
		_isReconnecting = true;
		yield return ((MonoBehaviour)this).StartCoroutine(loadingCurtain.CoTakeScreenShot());
		try
		{
			loadingCurtain.Show();
			KSingleton<AssetBundleManager>.Instance().ClearRequests();
			if (this.PreReconnect != null)
			{
				this.PreReconnect();
			}
		}
		catch (Exception ex)
		{
			Exception e = ex;
			Debug.LogException(e);
			MoveToTitle();
			yield break;
		}
		int tryCount = 0;
		while (!Connections.Frontend.Connected())
		{
			tryCount++;
			if (tryCount > 3)
			{
				MoveToTitle();
				yield break;
			}
			((MonoBehaviour)this).StartCoroutine(CoConnect(_lastHost, _lastPort));
			while (!Connections.Frontend.Connected() && Connections.Frontend.IsAttemptingToConnect())
			{
				yield return null;
			}
		}
		SendAuthMessage(ReconnectAuthSucceed, delegate
		{
			KUtility.DelayedCall((MonoBehaviour)(object)this, MoveToTitle, 0.1f);
		});
	}

	private void ReconnectAuthSucceed()
	{
		try
		{
			_isReconnecting = false;
			if (this.PostReconnect != null)
			{
				this.PostReconnect();
			}
			SendReady();
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
			KUtility.DelayedCall((MonoBehaviour)(object)this, MoveToTitle, 0.1f);
		}
	}

	private void MoveToTitleLevel()
	{
		_isReconnecting = false;
		try
		{
			((MonoBehaviour)this).StopAllCoroutines();
			HTTPManager.OnQuit();
			KSingleton<AssetBundleManager>.Instance().StopBackgroundDownloading();
			KSingleton<AssetBundleManager>.Instance().ClearAll();
			Connections.Reset();
			KCollisionUtility.Reset();
			if (this.MainSceneClosed != null && IsMainScene())
			{
				this.MainSceneClosed();
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
		SceneManager.LoadScene("Title");
	}

	public void MoveToTitle()
	{
		if (Connections.Frontend.Connected())
		{
			ForceMoveToTitle = true;
			Connections.Frontend.Close();
		}
		else
		{
			MoveToTitleLevel();
		}
	}

	private IEnumerator CoConnect(string host, int port)
	{
		Connections.Frontend.ConnectAsync(host, port);
		while (!Connections.Frontend.Connected())
		{
			yield return null;
		}
		Connections.Frontend.StartReceive();
	}

	private void EmigratedReceived(Emigrated msg, PacketHeader header)
	{
		IsEmigrated = true;
		Connections.Frontend.Close();
	}

	public void SetLocale(string locale)
	{
		locale = LocalizeSystem.NormalizeLocale(locale);
		if (LocalizeSystem.IsLocaleNotLoadedYet)
		{
			LocalizeSystem.SetLocale(locale);
		}
		else if (IsSceneName("Title"))
		{
			LocalizeSystem.SetLocale(locale);
		}
		else
		{
			if (!(LocalizeSystem.Locale != locale))
			{
				return;
			}
			UIManager.MessageBox.Show(T._("언어 설정을 바꾸면 게임을 재시작 해야 합니다.\n재시작 하시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					LocalizeSystem.SetLocale(locale);
					Loader.LoadSucceed = false;
					MoveToTitle();
				}
			});
		}
	}
}
