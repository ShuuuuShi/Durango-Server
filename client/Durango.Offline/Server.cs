using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Durango.Logic.Clusters;
using Durango.Utils;
using UnityEngine;

namespace Durango.Offline;

public class Server
{
	private static Gateway _gateway;

	private static GameServer _gameServer;

	public static PlayerContext _localPlayer;

	[CompilerGenerated]
	private static Func<string, WorldContext> cache0;

	[CompilerGenerated]
	private static Func<string, PlayerContext> cache1;

	public static bool _isConnectingKylloxServer;

	public static bool _isOpenedKylloxServer;

	public string Key { get; private set; }

	public List<Context> Contexts { get; private set; }

	public Cluster Cluster { get; private set; }

	public static double CurrentVersion => 1.2;

	public Server(string key, Dictionary<string, string> names)
	{
		Server server = this;
		Key = key;
		Contexts = new List<Context>();
		Cluster = new Cluster();
		Cluster.Mode = ((key == "free") ? Mode.Editable : ((key == "solo") ? Mode.SingleMode : ((key == "multi") ? Mode.MultiMode : ((!(key == "online")) ? Mode.Offline : Mode.Online))));
		Cluster.Names = names;
		int islandPort = GetIslandPort();
		Cluster.GatewayUrlRoot = "http://127.0.0.1:" + islandPort;
		Cluster.OnRequestAccount = delegate(Action<Account> action)
		{
			Account account = new Account
			{
				Players = new List<PlayerInfo>()
			};
			foreach (Context context3 in server.Contexts)
			{
				PlayerContext player = context3.Player;
				PlayerInfo playerInfo = player.PlayerInfo;
				playerInfo.OfflineFunc = () => new Pair<PortraitBuilder.Argument, int>(player.AppearPlayer.Display.GetPortraitArgument(player.AppearPlayer.EntityId, player.AppearPlayer.IsMale()), player.AppearPlayer.Freq);
				account.Players.Add(playerInfo);
			}
			account.MaxPlayerSlotCount = Mathf.Max(2, (server.Cluster.Mode == Mode.Editable) ? 7 : ((server.Cluster.Mode == Mode.SingleMode) ? 3 : ((server.Cluster.Mode == Mode.MultiMode) ? 3 : account.Players.Count)));
			account.PlayerSlotCount = ((server.Cluster.Mode == Mode.Editable) ? 7 : ((server.Cluster.Mode == Mode.SingleMode) ? 3 : ((server.Cluster.Mode == Mode.MultiMode) ? 3 : account.Players.Count)));
			action?.Invoke(account);
		};
		Cluster.OnDeletePlayer = delegate(string entityId)
		{
			for (int i = 0; i < server.Contexts.Count; i++)
			{
				Context context2 = server.Contexts[i];
				if (context2.EntityId == entityId)
				{
					try
					{
						File.Delete(context2.Player.Path);
						File.Delete(context2.World.Path);
						File.Delete(Path.GetDirectoryName(context2.World.Path) + "\\" + context2.PlayerSlot + ".gen");
						File.Delete(Path.GetDirectoryName(context2.World.Path) + "\\" + context2.PlayerSlot + ".ua60vol");
					}
					catch (Exception)
					{
					}
					server.Contexts.RemoveAt(i);
					break;
				}
			}
		};
		Cluster.OnConfirm = delegate(string entityId)
		{
			// [แก้เอง] "online" คือปุ่ม "Online Server (For Test)" ของแท้จากเกมต้นฉบับ (ดู Servers.cs)
			// เดิมมันเรียก BeginServer เหมือน free/solo/multi ทุกอย่าง ⇒ ได้แค่เซิร์ฟจำลองในเครื่องที่ติด
			// ป้าย Mode.Online ไว้เฉย ๆ (ไม่ได้ต่อเซิร์ฟจริงเลย) — ผู้เล่นเลยเลือกโหมดเล่นจริงจาก main UI
			// ไม่ได้ตามที่ขอ ("ต้องเลือกได้ว่าจะเล่นโหมดไหน ไม่ใช่บังคับออนไลน์")
			// ตอนนี้ให้ "online" ต่อเซิร์ฟจริงทันที (ใช้ ip ล่าสุดที่เคยกรอกผ่านเมนู "เยี่ยมชมเกาะเพื่อน"
			// ในเกม ถ้ายังไม่เคยกรอกเลยก็ fallback เป็นเซิร์ฟเราเอง 127.0.0.1) ไม่ล็อคแค่ของเรา — ผู้เล่น
			// เปลี่ยนเซิร์ฟได้อยู่ดีผ่านเมนูนั้น (มันเซฟค่าลง "last_connect_ip" คีย์เดียวกัน)
			// ⚠️ เดิมลองโชว์กล่องกรอก IP (TextInputPopup) ตรงนี้เลย — เทสจริงแล้วเจอ NullReferenceException
			// (ดู output_log.txt) เพราะ UIManager.Popup ยังไม่ init ตอนอยู่หน้าไตเติ้ล (popup canvas เป็นของ
			// scene ในเกม ไม่ใช่ scene ไตเติ้ล) ต่างจาก MenuListGroupBase.ShowConnectIpInput ที่เรียกได้
			// เพราะอยู่ในเกมแล้ว — ต้องมี UI งานเพิ่มถ้าจะทำกล่องกรอกที่หน้าไตเติ้ลจริง ๆ (ยังไม่ได้ทำ)
			// ⚠️ รอบสอง: เรียก ConnectTo(ip) ตรงนี้เลยโดยไม่เรียก BeginServer ก่อน — เจอ "[400] Bad
			// Request/คิวการล็อกอิน" ทันที เพราะ ConnectTo ใช้ _localPlayer (สร้างใน BeginServer เท่านั้น)
			// ตอนปกติ ConnectTo ถูกออกแบบมาให้เรียกตอน "อยู่ในเซสชันโลคอลอยู่แล้ว" (เช่นเมนู "เยี่ยมชมเกาะ
			// เพื่อน" ระหว่างเล่น) ไม่ใช่จากหน้าไตเติ้ลตรง ๆ ⇒ ต้อง BeginServer (ตั้ง _localPlayer) ก่อนเสมอ
			Context context = server.Contexts.Find((Context x) => x.EntityId == entityId);
			if (context == null)
			{
				int num = 0;
				if (server.Contexts.Count > 0)
				{
					num = server.Contexts[server.Contexts.Count - 1].PlayerSlot + 1;
				}
				WorldContext worldContext = new WorldContext();
				worldContext.Initialize(WorldContext.MakePath(num, key));
				worldContext.PlayerSlot = num;
				PlayerContext playerContext2 = new PlayerContext();
				playerContext2.Initialize(PlayerContext.MakePath(num, key));
				playerContext2.PlayerSlot = num;
				context = new Context(worldContext, playerContext2);
				server.Contexts.Add(context);
			}
			BeginServer(context.World, context.Player);
			if (key == "online")
			{
				string ip = Preferences.GetString("last_connect_ip", "127.0.0.1");
				if (string.IsNullOrEmpty(ip))
				{
					ip = "127.0.0.1";
				}
				ConnectTo(ip);
			}
		};
		string[] files = AppData.GetFiles(WorldContext.GetBasePath(Key), "*.world", SearchOption.TopDirectoryOnly);
		if (files == null)
		{
			return;
		}
		IEnumerable<WorldContext> enumerable = from x in files.Select(WorldContext.Load)
			where x != null
			select x;
		string[] files2 = AppData.GetFiles(WorldContext.GetBasePath(Key), "*.player", SearchOption.TopDirectoryOnly);
		Dictionary<int, PlayerContext> dictionary = new Dictionary<int, PlayerContext>();
		if (files2 != null)
		{
			foreach (PlayerContext item in from x in files2.Select(PlayerContext.Load)
				where x != null
				select x)
			{
				dictionary[item.PlayerSlot] = item;
			}
		}
		Contexts = new List<Context>();
		foreach (WorldContext item2 in enumerable)
		{
			PlayerContext playerContext = dictionary.Get(item2.PlayerSlot);
			if (playerContext == null)
			{
				playerContext = new PlayerContext();
				playerContext.Initialize(PlayerContext.MakePath(item2.PlayerSlot, Key));
			}
			Contexts.Add(new Context(item2, playerContext));
		}
		Contexts = Contexts.OrderBy((Context x) => x.PlayerSlot).ToList();
	}

	/// <summary>
	/// [แก้เอง] ต่อเซิร์ฟอัตโนมัติตอนเปิดเกม — ตั้ง env DURANGO_AUTOCONNECT=&lt;ip[:port]&gt;
	/// เดิมเป็น IL patch ใน tools/DllPatcher ตอนนี้ย้ายมาอยู่ในซอร์สแล้ว
	/// (ทำครั้งเดียวต่อการเปิดเกม 1 รอบ — ไม่งั้นวนกลับหน้าไตเติ้ลไม่รู้จบ)
	/// </summary>
	public static bool _autoConnected;

	/// <summary>
	/// [แก้เอง] ข้ามการต่ออัตโนมัติ 1 ครั้ง — ใช้ตอนย้ายเกาะ (มี PendingIslandAddress)
	/// กัน BeginServer ต่อกลับเกาะแรกทับเกาะปลายทาง
	/// </summary>
	public static bool _skipNextAutoConnect;

	/// <summary>
	/// [แก้เอง] เซิร์ฟเป้าหมายที่บังคับต่ออัตโนมัติ (ใช้เมื่อไม่มี env DURANGO_AUTOCONNECT)
	/// ⚠️ env DURANGO_AUTOCONNECT ต้องมีผลที่นี่ด้วย — TitleMenuGroup อ่านฟิลด์นี้ตรง ๆ
	///
	/// [แก้เอง] 24 ส.ค. 2026 — **เจอต้นเหตุจริงของบั๊ก "[400]/คิวการล็อกอิน" วันนี้แล้ว**: เดิม
	/// `_defaultAutoConnectTarget = "192.168.1.34"` (IP เซิร์ฟ LAN เก่าที่ตายไปแล้ว) ⇒ ไม่ตั้ง env
	/// DURANGO_AUTOCONNECT เลย เกมก็ยังพยายามต่อ 192.168.1.34 อัตโนมัติอยู่ดี (ไม่ใช่ค่าว่างอย่างที่คิด
	/// ตอนไล่บั๊กรอบแรก!) ต่อไม่ติด (เครื่องนี้ไม่มี IP วงนั้นแล้ว) ⇒ error ที่เห็นทั้งหมดมาจากพยายามต่อ
	/// เซิร์ฟผีตัวนี้ ไม่ใช่เซิร์ฟเราเลย (เป็นเหตุผลที่ server log ไม่เห็น request อะไรเข้ามาเลยสักครั้ง)
	/// ⇒ เปลี่ยนเป็นค่าว่าง — ไม่ตั้ง env = **ไม่ auto-connect ไปไหนเลย** (offline mode จริง ๆ) ผู้เล่น
	/// กด Start แล้วเจอกล่องกรอก IP ทันที (ดู TitleMenuUserControlBase.OnConfirm) หรือใช้เมนูในเกม
	/// "เยี่ยมชมเกาะเพื่อน → กรอกที่อยู่โดยตรง" ก็ได้ — ไม่ล็อคที่ไหนอีกต่อไปตามที่เจ้าของสั่ง
	/// </summary>
	public static string AutoConnectTarget
	{
		get
		{
			string env = global::System.Environment.GetEnvironmentVariable("DURANGO_AUTOCONNECT");
			return !string.IsNullOrEmpty(env) ? env : _defaultAutoConnectTarget;
		}
	}

	private static string _defaultAutoConnectTarget = "";

	public static void BeginServer(WorldContext worldCtx, PlayerContext playerCtx)
	{
		EndServer();
		_gameServer = new GameServer(worldCtx, playerCtx);
		_gateway = new Gateway(_gameServer, worldCtx, playerCtx);
		_localPlayer = playerCtx;

		if (_skipNextAutoConnect)
		{
			_skipNextAutoConnect = false;
			_autoConnected = true;
		}
		else if (!_autoConnected)
		{
			string autoTarget = global::System.Environment.GetEnvironmentVariable("DURANGO_AUTOCONNECT");
			if (string.IsNullOrEmpty(autoTarget))
			{
				autoTarget = AutoConnectTarget;
			}
			if (!string.IsNullOrEmpty(autoTarget))
			{
				_autoConnected = true;
				ConnectTo(autoTarget);
			}
		}
	}

	public static void EndServer()
	{
		if (_gateway != null)
		{
			_gateway.Close();
			_gateway = null;
		}
		if (_gameServer != null)
		{
			_gameServer.Close();
			_gameServer = null;
		}
		// [แก้เอง] เปิด/โหลดเกาะใหม่ครั้งหน้าให้ต่อเซิร์ฟเป้าหมายอัตโนมัติอีกครั้ง
		// (เดิม set ครั้งเดียวตลอดการเปิดเกม — หลังเซิร์ฟ restart ผู้เล่นโดนตัด
		//  แล้วต้องเข้าผ่านเมนู "เยี่ยมบ้านเพื่อน" ใหม่ทุกครั้ง)
		_autoConnected = false;
	}

	public static void Process()
	{
		if (_gateway != null)
		{
			_gateway.Process();
		}
		if (_gameServer != null)
		{
			_gameServer.Process();
		}
	}

	/// <summary>
	/// [แก้เอง] เดิมฮาร์ดโค้ด gateway ที่พอร์ต 8190 ทำให้ต่อได้เซิร์ฟเดียวต่อ 1 เครื่อง
	/// ตอนนี้รับ "ip" หรือ "ip:port" ก็ได้ — ระบบหลายเกาะ (1 เกาะ = 1 พอร์ต) ถึงจะทำงานได้
	/// </summary>
	public static void ConnectTo(string ip)
	{
		Cluster cluster = new Cluster();
		if (ip.StartsWith("http://"))
		{
			ip = ip.Substring(7);
		}
		string gatewayUrlRoot = (ip.IndexOf(':') >= 0) ? ("http://" + ip) : ("http://" + ip + ":" + 8190);
		cluster.OnRequestAccount = delegate(Action<Account> action)
		{
			Account account = new Account();
			account.MaxPlayerSlotCount = 7;
			account.PlayerSlotCount = 1;
			account.Players = new List<PlayerInfo>();
			account.Players.Add(_localPlayer.PlayerInfo);
			action?.Invoke(account);
		};
		if (_isConnectingKylloxServer)
		{
			_isOpenedKylloxServer = true;
		}
		cluster.LocalPlayer = Json.Write(_localPlayer);
		cluster.GatewayUrlRoot = gatewayUrlRoot;
		cluster.Mode = Mode.Offline;
		GameManager.ConnectCluster = cluster;
		// [แก้เอง] MoveToTitle() รีสตาร์ท TitleMenuGroup ใหม่ตั้งแต่ State.Initial → GetClusterList
		// จุดนั้นเช็คแค่ AutoConnectTarget (ไม่รู้จัก ConnectCluster เลย) — ถ้าว่าง จะไป Resources.Load
		// ("offline/clusters") ซึ่งไม่มีในเกมแพตช์ของเรา ⇒ TryUpdateClusters ล้มเหลว → State.Error ทันที
		// (โชว์ "[400] Bad Request/คิวการล็อกอิน" — คนละสาเหตุกับบั๊ก 192.168.1.34 เดิม แต่หน้าตาเหมือนกัน)
		// ⇒ ตั้ง _defaultAutoConnectTarget ไว้ด้วยเสมอตอน ConnectTo ถูกเรียก กัน GetClusterList หลุดไปทาง
		// TextAsset ที่ไม่มีจริง (ไม่กระทบ env DURANGO_AUTOCONNECT — ยังชนะอยู่เพราะเช็คก่อนใน getter)
		_defaultAutoConnectTarget = ip;
		GameManager.Emigrated = GameManager.EmigratedType.Explore;
		Singleton<GameManager>.Instance().MoveToTitle();
	}

	public static void SendLogs(string log)
	{
		if (_localPlayer.IsConnectedKylloxServer)
		{
			WebClient webClient = new WebClient();
			if (new StreamReader(webClient.OpenRead("http://durangomanager.000webhostapp.com/server/log.txt")).ReadToEnd().IndexOf(log) == -1)
			{
				NameValueCollection nameValueCollection = new NameValueCollection();
				nameValueCollection["data"] = log;
				webClient.UploadValues("http://durangomanager.000webhostapp.com/server/upload_log.php", nameValueCollection);
			}
		}
	}

	public static int GetIslandPort()
	{
		string environmentVariable = global::System.Environment.GetEnvironmentVariable("DURANGO_ISLAND_PORT");
		if (!string.IsNullOrEmpty(environmentVariable) && int.TryParse(environmentVariable, out var result) && result > 0)
		{
			return result;
		}
		return 8390;
	}
}
