using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using BestHTTP;
using Durango.Logic.Clusters;
using Durango.Logic.Explore;
using Durango.Network;
using Durango.Offline;
using Durango.System;
using Durango.System.Config;
using Durango.Terrain;
using Durango.UI;
using Durango.Utils;
using EasyConsole;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Region;
using Shared.System;
using Shared.Teleport;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
	public enum EmigratedType
	{
		None,
		Explore,
		Warp,
		FromSafeHouse,
		ToWarpRush
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass137_0
	{
		public ReconnectLoadingCurtain curtain;

		public GameManager _003C_003E4__this;

		internal void _003CReconnect_003Eb__0()
		{
			curtain.Connected();
			_003C_003E4__this.ReconnectAuthSucceed();
		}

		internal void _003CReconnect_003Eb__1(string _003Cp0_003E)
		{
			KUtility.DelayedCall(_003C_003E4__this, _003C_003E4__this.MoveToTitle, 0.1f);
		}
	}

	[CompilerGenerated]
	private sealed class _003CReconnect_003Ed__137 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ReconnectLoadingCurtain curtain;

		public GameManager _003C_003E4__this;

		private _003C_003Ec__DisplayClass137_0 _003C_003E8__1;

		private int _003CtryCount_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CReconnect_003Ed__137(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E8__1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			GameManager gameManager = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E8__1 = new _003C_003Ec__DisplayClass137_0();
				_003C_003E8__1.curtain = curtain;
				_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
				gameManager._isReconnecting = true;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				try
				{
					Singleton<AssetBundleManager>.Instance().ClearRequests();
					SafeInvoke(gameManager.PreReconnect);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					gameManager.MoveToTitle();
					return false;
				}
				_003CtryCount_003E5__2 = 0;
				goto IL_0153;
			case 2:
				_003C_003E1__state = -1;
				goto IL_00f0;
			case 3:
				{
					_003C_003E1__state = -1;
					goto IL_0153;
				}
				IL_0153:
				if (Connections.Frontend.Connected())
				{
					break;
				}
				_003CtryCount_003E5__2++;
				if (_003CtryCount_003E5__2 > 3)
				{
					gameManager.MoveToTitle();
					break;
				}
				gameManager.TryConnect();
				goto IL_00f0;
				IL_00f0:
				if (Connections.Frontend.IsAttemptingToConnect())
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 2;
					return true;
				}
				if (Connections.Frontend.Connected())
				{
					gameManager.SendAuthMessage(delegate
					{
						_003C_003E8__1.curtain.Connected();
						_003C_003E8__1._003C_003E4__this.ReconnectAuthSucceed();
					}, delegate
					{
						KUtility.DelayedCall(_003C_003E8__1._003C_003E4__this, _003C_003E8__1._003C_003E4__this.MoveToTitle, 0.1f);
					}, isReconnect: true);
					goto IL_0153;
				}
				_003C_003E2__current = new WaitForSeconds(3f);
				_003C_003E1__state = 3;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private static bool _isSceneClosing;

	[SerializeField]
	private string _clusterListUrlFormat;

	private readonly List<KeyValuePair<string, int>> _endpoints = new List<KeyValuePair<string, int>>();

	private int _endpointIndex;

	private bool _started;

	private bool _isReconnecting;

	private bool _forceMoveToTitle;

	private readonly Queue<string> _lastErrors = new Queue<string>();

	public static bool IsSceneClosing
	{
		get
		{
			if (_isSceneClosing)
			{
				return Application.isPlaying;
			}
			return false;
		}
		private set
		{
			_isSceneClosing = value;
		}
	}

	public static string PlayerId { get; set; }

	public static int PlayerSlotIndex { get; set; }

	public static string SessionToken { get; set; }

	public static bool IsReady { get; private set; }

	public static string GatewayUrl { get; private set; }

	public static string ClusterKey { get; private set; }

	public static Mode ClusterMode { get; private set; }

	public static Cluster ConnectCluster { get; set; }

	public static string ArenaAuthServerUrl { get; private set; }

	public static bool IsPrologueMode { get; private set; }

	public static EmigratedType Emigrated { get; set; }

	public static bool IsPlayerIdSelected { get; set; }

	public static string LastEvictedMsg { get; set; }

	[NotNull]
	public static Durango.Logic.Explore.Region Region { get; private set; }

	public static string PersonalRegionId { get; private set; }

	public static Messages.Archipelago? Archipelago { get; private set; }

	public static bool IsMainScene => IsSceneName(Durango.System.Platform.Instance.MainSceneName);

	public static bool IsTitleScene => IsSceneName("Title");

	public static string PendingIslandAddress { get; set; }

	public static event Action Reset;

	public static event Action Started;

	public static event Action Quitted;

	public event Action MainSceneLoaded;

	public event Action<Welcome> WelcomeReceived;

	public event Action YamlLoaded;

	public event Action PreReconnect;

	public event Action PostReconnect;

	private event Action Ready;

	public string GetLastErrors()
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder stringBuilder = reusable;
		int num = 0;
		int count = _lastErrors.Count;
		foreach (string lastError in _lastErrors)
		{
			if (num == 0)
			{
				stringBuilder.Append("(");
			}
			int num2 = lastError.IndexOf("오류코드: ", StringComparison.Ordinal);
			if (num2 != -1)
			{
				string value = lastError.Substring(num2 + "오류코드: ".Length, 6);
				stringBuilder.Append(value);
				stringBuilder.Append((num != count - 1) ? ", " : ")\n");
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	protected override bool CheckDontDestroyOnLoad()
	{
		return true;
	}

	public string GetClusterListUrl()
	{
		return string.Format(_clusterListUrlFormat, CurrentBundleVersion.GetClientVersion());
	}

	public static void SetCluster(string clusterKey, string url, Mode mode)
	{
		ClusterKey = clusterKey;
		GatewayUrl = url;
		ClusterMode = mode;
	}

	public static void SetArenaAuthServer(string arenaAuthServerUrl)
	{
		ArenaAuthServerUrl = arenaAuthServerUrl;
	}

	protected override void OnAwake()
	{
		ZipConstants.DefaultCodePage = 0;
		IsSceneClosing = false;
		IsPrologueMode = IsSceneName("Prologue");
		if (Used)
		{
			Application.logMessageReceived += LogCallback;
			Application.runInBackground = true;
			Screen.sleepTimeout = -1;
			SceneManager.sceneLoaded += SceneManager_SceneLoaded;
			Region = Durango.Logic.Explore.Region.UnknownRegion;
			PersonalRegionId = null;
			DeviceInfo.Init();
			ScreenInfo.Init();
			ConfigInstance.Initialize();
		}
	}

	private void SceneManager_SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if (IsMainScene)
		{
			SafeInvoke(this.MainSceneLoaded);
			Connections.Frontend.ForceSyncClock();
			Emigrated = EmigratedType.None;
		}
		else if (IsTitleScene && _started)
		{
			Start();
		}
	}

	private static bool IsSceneName(string sceneName)
	{
		return SceneManager.GetActiveScene().name == sceneName;
	}

	private static void LogCallback(string log, string stack, LogType type)
	{
		if (Debug.isDebugBuild && EasyConsole.Console.Instance != null)
		{
			string text = string.Empty;
			switch (type)
			{
			case LogType.Error:
			case LogType.Exception:
				text = "red";
				break;
			case LogType.Warning:
				text = "yellow";
				break;
			}
			string line = ((!string.IsNullOrEmpty(text)) ? $"<color={text}>{type}</color> : {log}" : string.Concat(type, ": ", log));
			EasyConsole.Console.Instance.Print(line);
			EasyConsole.Console.Instance.Print(stack);
		}
		if (type == LogType.Error || type == LogType.Exception)
		{
			ErrorReporter.HandleLog(log, stack, System.DateTime.Now, type);
		}
	}

	private void Start()
	{
		_started = true;
		SessionToken = string.Empty;
		Connections.Frontend.On(delegate(Evicted msg, PacketHeader header)
		{
			_forceMoveToTitle = true;
			switch (msg.Reason)
			{
			case EvictionReason.Duplication:
			{
				string text3 = T._("동일한 계정으로 타 기기에서 접속하였습니다.");
				string text4 = T._("화면을 터치 후 다시 시도해 주세요.");
				LastEvictedMsg = ((!Durango.System.Platform.Instance.UsePCUI) ? (text3 + "\n" + text4) : text3);
				break;
			}
			case EvictionReason.Administrative:
			{
				string text = T._("운영규칙을 위반하여 제제 당하였습니다.");
				string text2 = T._("화면을 터치 후 다시 시도해 주세요.");
				LastEvictedMsg = ((!Durango.System.Platform.Instance.UsePCUI) ? (text + "\n" + text2) : text);
				break;
			}
			default:
				LastEvictedMsg = T._("서버와의 연결이 끊어졌습니다.");
				break;
			}
		});
		Connections.Frontend.On<Error>(DefaultErrorHandler);
		Connections.Frontend.On<OK>(DefaultOKHandler);
		Connections.Frontend.On<Abort>(DefaultAbortHandler);
		Connections.Frontend.On<Info>(DefaultInfoHandler);
		Connections.Radiotower.On<Error>(DefaultErrorHandler);
		Connections.Radiotower.On<OK>(DefaultOKHandler);
		Connections.Radiotower.On<Abort>(DefaultAbortHandler);
		Connections.Radiotower.On<Info>(DefaultInfoHandler);
		Connections.Frontend.On<Emigrated>(EmigratedReceived);
		Connections.Frontend.ConnectionClosed += Frontend_ConnectionClosed;
		this.MainSceneLoaded = null;
		this.WelcomeReceived = null;
		this.YamlLoaded = null;
		this.PreReconnect = null;
		this.PostReconnect = null;
		this.Ready = null;
		GameSystemUtil.Reset();
		if (GameManager.Started != null)
		{
			GameManager.Started();
		}
	}

	private static string LimitText(string text)
	{
		if (text == null)
		{
			return string.Empty;
		}
		if (text.Length > 2500)
		{
			text = text.Substring(0, 2500);
		}
		return text;
	}

	private void DefaultErrorHandler(Error msg, PacketHeader header)
	{
		UIManager.SystemMsg(LimitText(msg.Text), 4f);
		_lastErrors.Enqueue(msg.Text);
		if (_lastErrors.Count > 10)
		{
			_lastErrors.Dequeue();
		}
	}

	private static void DefaultAbortHandler(Abort msg, PacketHeader header)
	{
		UIManager.SystemMsg(LimitText(msg.Text), 4f);
	}

	private static void DefaultOKHandler(OK msg, PacketHeader header)
	{
	}

	private static void DefaultInfoHandler(Info msg, PacketHeader header)
	{
		if (!string.IsNullOrEmpty(msg.Text) && msg.Text.StartsWith("##goto "))
		{
			PendingIslandAddress = msg.Text.Substring("##goto ".Length).Trim();
		}
	}

	private static void EmigratedReceived(Emigrated msg, PacketHeader header)
	{
		switch (msg.Type)
		{
		default:
			Emigrated = EmigratedType.Warp;
			break;
		case TeleportType.Season2:
			Emigrated = EmigratedType.ToWarpRush;
			break;
		case TeleportType.Unknown:
			Emigrated = ((Region.Role() != Role.Safehouse) ? EmigratedType.Explore : EmigratedType.FromSafeHouse);
			break;
		}
		Connections.Frontend.Close();
	}

	private void Update()
	{
		Server.Process();
		Connections.Frontend.Process();
		Connections.Radiotower.Process();
		Connections.Radiotower.MaybeSendKeepalive();
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Return))
		{
			ScreenInfo.ToggleScreenMode();
		}
	}

	private void OnDisable()
	{
		if (Used)
		{
			Connections.Frontend.Close();
			Connections.Radiotower.Close();
			Server.EndServer();
		}
	}

	private void OnApplicationQuit()
	{
		if (Used)
		{
			IsSceneClosing = true;
			if (GameManager.Quitted != null)
			{
				GameManager.Quitted();
			}
		}
	}

	public void SendAuthMessage(Action succeed, Action<string> failed, bool isReconnect = false)
	{
		Auth auth = default(Auth);
		auth.EntityId = PlayerId;
		auth.SessionToken = SessionToken;
		auth.ClientVersion = CurrentBundleVersion.GetClientVersion();
		auth.DeviceModel = SystemInfo.deviceModel;
		Auth msg = auth;
		Connections.Frontend.Send(msg).On(delegate(Welcome welcome, PacketHeader header)
		{
			Durango.Logic.Explore.Region region = Region;
			Region = new Durango.Logic.Explore.Region(welcome.Region);
			PersonalRegionId = welcome.PersonalRegionId;
			Archipelago = welcome.Archipelago;
			if (isReconnect && region.Id != Region.Id)
			{
				Emigrated = EmigratedType.Explore;
				if (failed != null)
				{
					failed(string.Empty);
				}
			}
			else
			{
				SafeInvoke(this.WelcomeReceived, welcome);
				TerrainMeta.Load(Region.TerrainId, succeed, failed);
			}
		}).On(delegate(Error error, PacketHeader header)
		{
			if (failed != null)
			{
				failed(error.Text);
			}
		})
			.Rest(delegate
			{
				failed(string.Empty);
			});
	}

	public void SendReady()
	{
		Connections.Frontend.Send(default(Ready)).On<OK>(delegate
		{
			IsReady = true;
			SafeInvoke(this.Ready);
		});
	}

	public void AddOnReady(Action action)
	{
		if (IsReady)
		{
			action();
			return;
		}
		Ready -= action;
		Ready += action;
	}

	public void SetEndpoints([CanBeNull] IList<KeyValuePair<string, int>> endpoints)
	{
		_endpoints.Clear();
		if (endpoints != null)
		{
			_endpoints.AddRange(endpoints);
		}
		_ = _endpoints.Count;
		_endpointIndex = UnityEngine.Random.Range(0, _endpoints.Count);
	}

	public void TryConnect()
	{
		if (_endpoints.Count != 0)
		{
			_endpointIndex %= _endpoints.Count;
			KeyValuePair<string, int> keyValuePair = _endpoints[_endpointIndex];
			string key = keyValuePair.Key;
			int value = keyValuePair.Value;
			_endpointIndex++;
			Connections.Frontend.ConnectAsync(key, value);
		}
	}

	public void ForceMainSceneLoadedPrologue()
	{
		SafeInvoke(this.MainSceneLoaded);
	}

	private void Frontend_ConnectionClosed()
	{
		if (IsSceneClosing)
		{
			return;
		}
		IsReady = false;
		try
		{
			Connections.Radiotower.Close();
		}
		catch (Exception)
		{
		}
		if (IsTitleScene)
		{
			return;
		}
		if (!string.IsNullOrEmpty(PendingIslandAddress))
		{
			string pendingIslandAddress = PendingIslandAddress;
			PendingIslandAddress = null;
			Emigrated = EmigratedType.None;
			Server._autoConnected = true;
			Server.ConnectTo(pendingIslandAddress);
			return;
		}
		bool num = Emigrated != 0 || _forceMoveToTitle;
		_forceMoveToTitle = false;
		if (!num && !_isReconnecting && Singleton<TerrainBase>.HasInstance() && Singleton<TerrainBase>.Instance().IsReady)
		{
			ReconnectLoadingCurtain reconnectLoadingCurtain = UIManager.ShowLoadingCurtain<ReconnectLoadingCurtain>();
			if (reconnectLoadingCurtain != null)
			{
				StartCoroutine(Reconnect(reconnectLoadingCurtain));
				return;
			}
		}
		MoveToTitleLevel();
	}

	private IEnumerator Reconnect(ReconnectLoadingCurtain curtain)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CReconnect_003Ed__137(0)
		{
			_003C_003E4__this = this,
			curtain = curtain
		};
	}

	private void ReconnectAuthSucceed()
	{
		try
		{
			_isReconnecting = false;
			SafeInvoke(this.PostReconnect);
			SendReady();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			KUtility.DelayedCall(this, MoveToTitle, 0.1f);
		}
	}

	[ExposedInEditor(null)]
	public void MoveToTitle()
	{
		if (Connections.Frontend.Connected())
		{
			_forceMoveToTitle = true;
			Connections.Frontend.Close();
		}
		else
		{
			MoveToTitleLevel();
		}
	}

	private void MoveToTitleLevel()
	{
		_isReconnecting = false;
		IsReady = false;
		IsSceneClosing = true;
		try
		{
			StopAllCoroutines();
			HTTPManager.OnQuit();
			SafeInvoke(GameManager.Reset);
			Server.EndServer();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		SceneManager.LoadScene("Title");
	}

	private static void SafeInvoke(Action action)
	{
		if (action == null)
		{
			return;
		}
		Delegate[] invocationList = action.GetInvocationList();
		int i = 0;
		for (int size = KUtility.GetSize(invocationList); i < size; i++)
		{
			try
			{
				((Action)invocationList[i])();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	private static void SafeInvoke<T>(Action<T> action, T param)
	{
		if (action == null)
		{
			return;
		}
		Delegate[] invocationList = action.GetInvocationList();
		int i = 0;
		for (int size = KUtility.GetSize(invocationList); i < size; i++)
		{
			try
			{
				((Action<T>)invocationList[i])(param);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public void NotifyYamlLoaded()
	{
		SafeInvoke(this.YamlLoaded);
	}
}
