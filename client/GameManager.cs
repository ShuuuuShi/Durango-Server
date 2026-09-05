using System;
using System.Collections;
using System.Collections.Generic;
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
			return _isSceneClosing && Application.isPlaying;
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
		string clusterListUrlFormat = _clusterListUrlFormat;
		return string.Format(clusterListUrlFormat, CurrentBundleVersion.GetClientVersion());
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
		// [4 ก.ย. 2026] บังคับเปิดผ่าน DinoWorld Launcher เท่านั้น — ไม่ผ่าน = ปิดเกมทันที (ดู LauncherGate.cs)
		if (!LauncherGate.Enforce())
		{
			return;
		}

		// [แก้เอง] ระบบ mod ฝั่งเกม — โหลดครั้งเดียวตอนเกมบูต (ดู ClientModLoader.cs)
		ClientModLoader.LoadAll();

		_started = true;
		SessionToken = string.Empty;
		Connections.Frontend.On(delegate(Evicted msg, PacketHeader header)
		{
			_forceMoveToTitle = true;
			switch (msg.Reason)
			{
			case EvictionReason.Duplication:
			{
				string text = T._("동일한 계정으로 타 기기에서 접속하였습니다.");
				string arg = T._("화면을 터치 후 다시 시도해 주세요.");
				string lastEvictedMsg = ((!Durango.System.Platform.Instance.UsePCUI) ? $"{text}\n{arg}" : text);
				LastEvictedMsg = lastEvictedMsg;
				break;
			}
			case EvictionReason.Administrative:
			{
				string text = T._("운영규칙을 위반하여 제제 당하였습니다.");
				string arg = T._("화면을 터치 후 다시 시도해 주세요.");
				string lastEvictedMsg = ((!Durango.System.Platform.Instance.UsePCUI) ? $"{text}\n{arg}" : text);
				LastEvictedMsg = lastEvictedMsg;
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
		// [แก้เอง] เดิมไม่เช็ค null
		// server ตอบ Abort เปล่า ๆ (ไม่มีข้อความ) ทุกครั้งที่ปฏิเสธการกระทำ — เก็บของไม่มีเครื่องมือ,
		// อยู่ไกลเกินเอื้อม, สตามินาไม่พอ ฯลฯ ⇒ NullReferenceException ใน DefaultAbortHandler ทุกครั้ง
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

	/// <summary>
	/// [แก้เอง] ที่อยู่ของเกาะปลายทางที่ server สั่งให้ย้ายไป (ว่าง = ไม่ได้กำลังย้ายเกาะ)
	/// server ส่งมาเป็นข้อความ Info ขึ้นต้นด้วย "##goto " เพราะเกมไม่มี packet
	/// สำหรับ "ย้ายไปเซิร์ฟอื่น" มาแต่แรก (ทุกเกาะของเกมจริงอยู่ใน cluster เดียวกัน)
	/// </summary>
	public static string PendingIslandAddress { get; set; }

	private static void DefaultInfoHandler(Info msg, PacketHeader header)
	{
		if (string.IsNullOrEmpty(msg.Text))
		{
			return;
		}
		// [แก้เอง] ระบบเกาะแยกเลเวล: server สั่งย้ายเกาะด้วย "##goto <ip:port>" แล้วตามด้วย Emigrated
		if (msg.Text.StartsWith("##goto "))
		{
			PendingIslandAddress = msg.Text.Substring("##goto ".Length).Trim();
			return;   // คำสั่งภายใน ไม่ใช่ข้อความให้ผู้เล่นอ่าน
		}
		// [แก้เอง] 31 ส.ค. 2026 — เดิมทิ้งข้อความทุกอันที่ไม่ใช่ "##goto" **ไม่แสดงอะไรเลย**
		//
		// 🐛 นี่คือต้นเหตุที่ผู้เล่นแจ้งว่า "คราฟไม่ได้": เซิร์ฟส่งเหตุผลกลับมาทุกครั้งอยู่แล้ว
		//    ("สูตรนี้ยังไม่ปลดล็อก — เรียนสกิลที่เกี่ยวข้องก่อน", "ตรงนี้มีสิ่งปลูกสร้างอยู่แล้ว",
		//     "กระเป๋าเต็ม (50/50 ช่อง)") แต่ไม่มีใครเอาไปแสดง ⇒ กดแล้วเงียบ ไม่รู้ว่าเพราะอะไร
		//    ตัวที่เคยแสดงคือ BroadcastDisplay ในมอด ซึ่งถอดออกไปตอน v0.1.0
		//    (ยืนยันจาก log จริงบน VPS: คราฟต์สำเร็จ 1,455 ครั้ง/3 ชม. abort แค่ 6 ครั้ง
		//     ⇒ ระบบไม่ได้พัง ผู้เล่นแค่ไม่เห็นคำอธิบาย)
		//
		// ใช้ SystemMsg = กล่องประกาศของตัวเกมเอง ตัวเดียวกับที่ DefaultAbortHandler ใช้
		// ⇒ ระบบประกาศจากแอดมิน (POST /admin/broadcast) กลับมาใช้ได้ด้วยในตัว
		// ⚠️ handler นี้ทำงานตอนรับแพ็กเก็ต ซึ่งอาจเกิดตั้งแต่ยังไม่อยู่ในซีน Main
		// ถ้าแตะ UIManager ตอนนั้น Singleton จะ "สร้าง" UIManager ขึ้นนอกซีน แล้ว InitUIGroups
		// หา prefab ไม่เจอ ⇒ ArgumentException + ตัว singleton จริงในซีน Main พังตามไปด้วย
		if (!IsMainScene)
		{
			return;
		}
		// [3 ก.ย. 2026] บรอดแคสต์แอดมินแบบกำหนดเวลา/ขนาด/สี — เข้ารหัสไว้ในข้อความ (แนวเดียวกับ ##goto)
		//   รูปแบบ: "##bc|d=<วินาที>|z=<ขนาด>|c=<hex สี>|<ข้อความ>"  (ทุกฟิลด์ยกเว้นข้อความไม่บังคับ)
		//   ไม่ต้องแก้โปรโตคอล Info (มีแค่ Text) — ผู้เล่นรุ่นเก่าที่ยังไม่อัปจะเห็นข้อความดิบ ไม่พัง
		if (msg.Text.StartsWith("##bc|"))
		{
			ShowAdminBroadcast(msg.Text.Substring("##bc|".Length));
			return;
		}
		UIManager.SystemMsg(LimitText(msg.Text), 4f);
	}

	/// <summary>อ่าน "d=..|z=..|c=..|ข้อความ" แล้วโชว์ด้วยเวลา/ขนาด/สีที่กำหนด</summary>
	private static void ShowAdminBroadcast(string payload)
	{
		float duration = 4f;
		float scale = 1f;
		string color = null;
		string text = payload;
		int bar = payload.IndexOf('|');
		// ไล่อ่านฟิลด์ x=y จนกว่าจะเจอส่วนที่ไม่ใช่ฟิลด์ = ข้อความจริง
		while (bar >= 0)
		{
			string token = text.Substring(0, bar);
			int eq = token.IndexOf('=');
			if (eq != 1) { break; }   // ไม่ใช่รูปแบบ x=y แล้ว = เริ่มข้อความจริง
			char kind = token[0];
			string val = token.Substring(2);
			switch (kind)
			{
			case 'd': float.TryParse(val, out duration); break;
			case 'z': float.TryParse(val, out scale); break;
			case 'c': color = val; break;
			}
			text = text.Substring(bar + 1);
			bar = text.IndexOf('|');
		}
		if (duration <= 0f) { duration = 4f; }
		duration = Mathf.Clamp(duration, 1f, 120f);
		scale = Mathf.Clamp((scale <= 0f) ? 1f : scale, 0.5f, 4f);
		text = LimitText(text);
		if (!string.IsNullOrEmpty(color))
		{
			// NGUI BBCode: [c][RRGGBB]...[-][/c]
			text = "[c][" + color.TrimStart('#') + "]" + text + "[-][/c]";
		}
		UIManager.SystemMsg(null, text, duration, scale);
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
			// M5: advertise loaded client mods before the normal Ready packet. Legacy servers
			// simply ignore this optional message; required-mod servers validate it.
			string modCatalogHash;
			string modManifest = ClientModLoader.BuildNegotiationManifest(out modCatalogHash);
			ModHello modHello = default(ModHello);
			modHello.Protocol = 1;
			modHello.ManifestJson = modManifest;
			modHello.CatalogHash = modCatalogHash;
			Connections.Frontend.Send(modHello);
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
		if (_endpoints.Count == 0)
		{
		}
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
		// [แก้เอง] ย้ายเกาะ: ต่อไปที่เกาะปลายทางแทนการกลับไปเกาะเดิม
		// (ต้องทำก่อนเช็ค reconnect ไม่งั้นมันจะพยายามต่อเซิร์ฟเดิมซ้ำ)
		if (!string.IsNullOrEmpty(PendingIslandAddress))
		{
			string target = PendingIslandAddress;
			PendingIslandAddress = null;
			Emigrated = EmigratedType.None;
			Server._autoConnected = true;      // กัน BeginServer ต่อกลับเกาะแรกทับ
			Server._skipNextAutoConnect = true;
			Server.ConnectTo(target);
			return;
		}

		bool forceMoveToTitle = _forceMoveToTitle;
		bool flag = Emigrated != 0 || forceMoveToTitle;
		_forceMoveToTitle = false;
		// An explicit MoveToTitle (logout/change character) must finish loading the
		// title scene.  The old auto-connect fallback below intercepted the closed
		// socket and connected to the previous server again, so character switching
		// looked like a reconnect to the old character.
		if (forceMoveToTitle)
		{
			MoveToTitleLevel();
			return;
		}
		if (!flag && !_isReconnecting && Singleton<TerrainBase>.HasInstance() && Singleton<TerrainBase>.Instance().IsReady)
		{
			ReconnectLoadingCurtain reconnectLoadingCurtain = UIManager.ShowLoadingCurtain<ReconnectLoadingCurtain>();
			if (reconnectLoadingCurtain != null)
			{
				StartCoroutine(Reconnect(reconnectLoadingCurtain));
				return;
			}
		}
		// [แก้เอง] บังคับต่อเซิร์ฟของเราเองแทนการกลับหน้า title — เซิร์ฟ restart/ปิด
		// connection แล้วผู้เล่นไม่ต้องเข้าเมนู "เยี่ยมบ้านเพื่อน" ใหม่ทุกครั้ง
		if (!string.IsNullOrEmpty(Server.AutoConnectTarget))
		{
			Server._autoConnected = false;
			Server.ConnectTo(Server.AutoConnectTarget);
			return;
		}
		MoveToTitleLevel();
	}

	private IEnumerator Reconnect(ReconnectLoadingCurtain curtain)
	{
		_isReconnecting = true;
		yield return null;
		try
		{
			Singleton<AssetBundleManager>.Instance().ClearRequests();
			SafeInvoke(this.PreReconnect);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
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
				break;
			}
			TryConnect();
			while (Connections.Frontend.IsAttemptingToConnect())
			{
				yield return null;
			}
			if (Connections.Frontend.Connected())
			{
				SendAuthMessage(delegate
				{
					curtain.Connected();
					ReconnectAuthSucceed();
				}, delegate
				{
					KUtility.DelayedCall(this, MoveToTitle, 0.1f);
				}, isReconnect: true);
			}
			else
			{
				yield return new WaitForSeconds(3f);
			}
		}
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
				Action action2 = (Action)invocationList[i];
				action2();
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
				Action<T> action2 = (Action<T>)invocationList[i];
				action2(param);
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
