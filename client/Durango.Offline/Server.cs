using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using Durango.Logic.Clusters;
using Durango.Utils;
using Newtonsoft.Json.Linq;
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
		// The Online Server entry must load persistent accounts from the real gateway.
		// Keeping the embedded callback here makes a fresh local player appear after restart.
		if (key == "online")
		{
			// [แก้เอง] 31 ส.ค. 2026 — เดิมฮาร์ดโค้ด "http://127.0.0.1:8190" = เครื่องผู้เล่นเอง
			// หน้า "เลือกเซิร์ฟเวอร์" จะยิง POST /accounts ไปที่นั่นเพื่อเอารายชื่อตัวละครมาโชว์
			// ซึ่งไม่มีอะไรตอบ ⇒ account เป็น null ⇒ TitleClusterSelection เรียก SetPlayerInfo(-1)
			// ⇒ ช่องจำนวนคนค้างที่ "กำลังตรวจสอบ..." ตลอดไป และกดเข้าไปก็ไม่เจอตัวละคร
			// (ดู TitleClusterSelection.ShowClusters → Clusters.GetOrRequestAccounts → RequestAccounts)
			// ⇒ ชี้ไปเซิร์ฟจริงจาก server.txt ตัวเดียวกับที่ปุ่มยืนยันใช้ จะได้ไม่ต้องตั้งค่าสองที่
			Cluster.GatewayUrlRoot = ToGatewayUrl(ResolveOnlineTarget());
			Cluster.OnRequestAccount = null;
		}
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
				// [แก้เอง] 31 ส.ค. 2026 — เดิมอ่านแต่ "last_connect_ip" ⇒ ผู้เล่นที่เพิ่งโหลดชุดแจกมา
				// ยังไม่เคยกรอก ip เลย จะ fallback ไป 127.0.0.1 (ไม่มีอะไรรันอยู่) ⇒ ค้างที่สถานะ
				// "ตรวจสอบ..." แล้วหาตัวละครไม่เจอ ทั้งที่ server.txt ชี้เซิร์ฟจริงอยู่แล้ว
				ConnectTo(ResolveOnlineTarget());
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
			// [แก้เอง] ห้ามอ่าน env DURANGO_AUTOCONNECT ตรงนี้
			// DurangoUpdater ตั้ง env นี้ทุกครั้งที่เปิดเกมจาก server.txt
			// ถ้า getter คืนค่านั้น Title/OnConfirm จะ ConnectTo ทันที ⇒ ข้ามหน้า Main
			// เจ้าของสั่ง: กดปุ่มที่หน้า Main ค่อยเชื่อมเซิร์ฟ
			// ปุ่ม Online ยังชี้ server.txt ผ่าน ResolveOnlineTarget() ตามเดิม
			// หลังกดเชื่อมแล้ว ConnectTo() ยังตั้ง _defaultAutoConnectTarget สำหรับ reconnect
			return _defaultAutoConnectTarget ?? string.Empty;
		}
	}

	private static string _defaultAutoConnectTarget = "";

	/// <summary>
	/// อ่านเซิร์ฟที่ operator ตั้งไว้ให้ชุดแจกจาก &lt;โฟลเดอร์เกม&gt;/server.txt
	/// บรรทัดแรกที่ไม่ใช่คอมเมนต์ (#) และไม่ว่าง = เซิร์ฟหลัก (บรรทัดถัดไป = ตัวสำรอง ยังไม่ใช้ตรงนี้)
	/// ไฟล์เดียวกับที่มอด DurangoClientCore อ่าน — จะได้ไม่ต้องตั้งค่าสองที่
	/// คืนค่าว่างถ้าไม่มีไฟล์/อ่านไม่ได้ ⇒ ผู้เรียกต้อง fallback เอง
	/// </summary>
	/// <summary>
	/// เซิร์ฟที่ปุ่ม "เซิร์ฟออนไลน์" ควรชี้ไป — ลำดับ: server.txt → ip ล่าสุดที่ผู้เล่นเคยกรอกเอง → เครื่องตัวเอง
	/// ใช้ร่วมกันทั้งตอนสร้างรายการในหน้าเลือกเซิร์ฟ (ดึงรายชื่อตัวละคร) และตอนกดยืนยัน (ต่อจริง)
	/// จะได้ไม่หลุดกันเหมือนเดิมที่ตอนโชว์รายการชี้ 127.0.0.1 แต่ตอนกดยืนยันชี้เซิร์ฟจริง
	/// </summary>
	internal static string ResolveOnlineTarget()
	{
		string text = ReadServerTxtTarget();
		if (string.IsNullOrEmpty(text))
		{
			text = Preferences.GetString("last_connect_ip", string.Empty);
		}
		if (string.IsNullOrEmpty(text))
		{
			text = "127.0.0.1";
		}
		return text;
	}

	/// <summary>เติม http:// และพอร์ต 8190 ให้ถ้ายังไม่มี — รับได้ทั้ง "ip", "ip:port" และ url เต็ม</summary>
	internal static string ToGatewayUrl(string ip)
	{
		if (ip.StartsWith("http://"))
		{
			return ip;
		}
		return (ip.IndexOf(':') >= 0) ? ("http://" + ip) : ("http://" + ip + ":" + 8190);
	}

	/// <summary>
	/// [4 ก.ย. 2026] เซิร์ฟที่มือถือต่อเมื่อไม่มี server.txt ในเครื่อง (ผู้เล่นมือถือทั่วไปไม่มีทางวางไฟล์เอง)
	/// ว่าง = เหมือน PC (ไม่ต่อไปไหน ให้กรอก IP เอง) · ค่านี้ตั้งตอน build ด้วย tools/AndroidApk (ดู docs/server/Android.md)
	/// </summary>
	public const string DefaultMobileServer = "187.53.129.69:8190";

	private static string ReadServerTxtTarget()
	{
		// [4 ก.ย. 2026] มือถือ (APK ที่ build เองแบบ Mono — ใช้ DLL ชุดเดียวกับ PC): Application.dataPath คือไฟล์ APK
		// เขียนไม่ได้ ⇒ หา server.txt เพิ่มที่ persistentDataPath (Android/data/<package>/files/server.txt)
		// ลำดับ: <โฟลเดอร์เกม>/server.txt (PC) → persistentDataPath/server.txt (มือถือ) → ค่า default ที่ฝังตอน build
		string[] candidates;
		try
		{
			candidates = new[]
			{
				Path.Combine(Directory.GetParent(Application.dataPath).FullName, "server.txt"),
				Path.Combine(Application.persistentDataPath, "server.txt")
			};
		}
		catch (Exception)
		{
			candidates = new[] { Path.Combine(Application.persistentDataPath, "server.txt") };
		}
		foreach (string path in candidates)
		{
			try
			{
				if (!File.Exists(path))
				{
					continue;
				}
				string[] lines = File.ReadAllLines(path);
				for (int i = 0; i < lines.Length; i++)
				{
					string line = lines[i].Trim();
					if (line.Length > 0 && !line.StartsWith("#"))
					{
						return line;
					}
				}
			}
			catch (Exception)
			{
			}
		}
		if (Application.isMobilePlatform)
		{
			UnityEngine.Debug.Log("[durango] ไม่พบ server.txt ที่ " + string.Join(" | ", candidates) + " ⇒ ใช้ค่า default " + DefaultMobileServer);
			return DefaultMobileServer;
		}
		return string.Empty;
	}

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
			// Deliberately wait for the normal title/menu flow.  Auto-connect is
			// disabled so launching the EXE can never silently select a server.
			_autoConnected = true;
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
		// [แก้เอง] 31 ส.ค. 2026 — เดิมบังคับ Mode.Offline เพราะเซิร์ฟเรายังไม่มี route "/assets/*"
		// ถ้าตั้ง Online ทั้งที่ไม่มี Loader จะขอ 71 ไฟล์ไม่เจอ รีทรายไฟล์ละ 5 รอบ = ค้างหน้าโหลด
		//
		// ผลข้างเคียงของ Offline ที่เจ็บที่สุดคือ MenuSystem.ShowInOffline อนุญาตแค่ 10 เมนู
		// ไม่มี Craft/Skill/Quest ⇒ ผู้เล่นไม่มีปุ่มคราฟต์ ต้องให้มอดมาเรียก EnableMenu() ปลดล็อกให้
		// พอเลิกใช้มอดจึงเหลือทางเดียวคือแก้ที่ต้นเหตุ — ทำ route ให้เซิร์ฟเสิร์ฟไฟล์พวกนั้นได้จริง
		//
		// ตอนนี้เซิร์ฟมี "/assets/*" ครบ 71 เส้นทางแล้ว (ดู server/ServerCore/Gateway.cs) จึงเปิด Online ได้
		// ได้กลับมาทั้งเมนูครบ และแก้สูตรคราฟต์/พิมพ์เขียวจากฝั่งเซิร์ฟโดยผู้เล่นไม่ต้องโหลดเกมใหม่
		cluster.Mode = Mode.Online;
		GameManager.ConnectCluster = cluster;
		// [แก้เอง] MoveToTitle() รีสตาร์ท TitleMenuGroup ใหม่ตั้งแต่ State.Initial → GetClusterList
		// จุดนั้นเช็คแค่ AutoConnectTarget (ไม่รู้จัก ConnectCluster เลย) — ถ้าว่าง จะไป Resources.Load
		// ("offline/clusters") ซึ่งไม่มีในเกมแพตช์ของเรา ⇒ TryUpdateClusters ล้มเหลว → State.Error ทันที
		// (โชว์ "[400] Bad Request/คิวการล็อกอิน" — คนละสาเหตุกับบั๊ก 192.168.1.34 เดิม แต่หน้าตาเหมือนกัน)
		// ⇒ ตั้ง _defaultAutoConnectTarget ไว้ด้วยเสมอตอน ConnectTo ถูกเรียก กัน GetClusterList หลุดไปทาง
		// TextAsset ที่ไม่มีจริง (ไม่กระทบ env DURANGO_AUTOCONNECT — ยังชนะอยู่เพราะเช็คก่อนใน getter)
		_defaultAutoConnectTarget = gatewayUrlRoot;
		FetchServerPolicy(gatewayUrlRoot);
		GameManager.Emigrated = GameManager.EmigratedType.Explore;
		Singleton<GameManager>.Instance().MoveToTitle();
	}

	/// <summary>
	/// ระยะ chunk ที่เซิร์ฟบอกมาทาง /knock (`world_chunk_range`) — ใช้ที่ TerrainBase.InitChunkPool
	/// 2 = 5×5 chunk (ค่าเดิมของเกม) · 4 = 9×9 chunk
	/// ⚠️ ต้องไม่เกิน `World.ChunkSendRange` ของเซิร์ฟ ไม่งั้น chunk วงนอกจะไม่มีต้นไม้/หิน
	/// (เซิร์ฟ clamp ค่านี้ให้อยู่ 2-4 มาแล้วฝั่งมัน ดู ClientModPolicy.cs)
	/// </summary>
	public static int WorldChunkRange = 2;

	/// <summary>เมนูที่เซิร์ฟอนุญาต (`enabled_menus`) เช่น Skill/Craft/Quest — ว่าง = ใช้ค่าของเกม</summary>
	public static string[] EnabledMenus = new string[0];

	/// <summary>เซิร์ฟบอกให้ข้ามหน้าเลือก region (`skip_region_selection`)</summary>
	public static bool SkipRegionSelection;

	/// <summary>
	/// เมนูที่เซิร์ฟสั่งให้ซ่อน (`hidden_menus`) — ชื่อ MenuType เช่น "Market", "Clan"
	/// ใช้ที่ `MenuSystem.IsHiddenMenu` แทนรายการฮาร์ดโค้ดเดิม
	/// ว่าง = ไม่ซ่อนอะไร · เซิร์ฟรุ่นเก่าที่ไม่ส่งค่านี้มาก็ได้ค่าว่างเหมือนกัน (ไม่ซ่อน)
	/// </summary>
	public static string[] HiddenMenus = new string[0];

	/// <summary>จำนวนคนออนไลน์ที่เซิร์ฟตอบมาล่าสุด (-1 = ยังไม่รู้)</summary>
	public static int OnlinePlayers = -1;

	/// <summary>เซิร์ฟตอบ /knock กลับมาไหม — ใช้ตัดสินสีจุดสถานะบนหน้าไตเติ้ล</summary>
	public static bool ServerReachable;

	/// <summary>เช็คสถานะไปแล้วอย่างน้อย 1 รอบ (ใช้แยก "ยังไม่รู้" ออกจาก "ติดต่อไม่ได้")</summary>
	public static bool ServerStatusKnown;

	/// <summary>เวลาที่เช็คสถานะรอบล่าสุด (Time.realtimeSinceStartup) — กันยิงถี่เกิน</summary>
	private static float _lastStatusCheck = -999f;

	/// <summary>
	/// เช็คสถานะเซิร์ฟแบบเบา ๆ สำหรับโชว์จุดเขียว/แดง + จำนวนคนบนหน้าไตเติ้ล
	///
	/// [เพิ่มเอง] 31 ส.ค. 2026 — เจ้าของขอ "จุดเขียวเล็ก ๆ บอกว่าเซิร์ฟรันอยู่ไหม + จำนวนคนออนไลน์
	/// ถ้าเซิร์ฟไม่เปิดให้เป็นจุดแดง"
	/// ยิงใน thread แยกเสมอ — ถ้าเซิร์ฟล่ม การรอ timeout บน main thread จะทำให้หน้าไตเติ้ลค้าง
	/// </summary>
	public static void RefreshServerStatus(bool force = false)
	{
		float now = Time.realtimeSinceStartup;
		if (!force && now - _lastStatusCheck < 10f)
		{
			return;
		}
		_lastStatusCheck = now;
		// [3 ก.ย. 2026] เดิม hardcode version=5.2.1 (เวอร์ชันเอนจิน) ⇒ เซิร์ฟที่ตั้ง RequiredVersionOfClient
		//   เทียบไม่ตรงตลอด (เตะทุกคน) · ส่งเวอร์ชัน custom จริง ("CustomClient 0.1.3") ให้เซิร์ฟเทียบ MAJOR.MINOR ได้
		if (Application.isMobilePlatform) UnityEngine.Debug.Log("[durango] RefreshServerStatus target=" + ResolveOnlineTarget());
		string url = ToGatewayUrl(ResolveOnlineTarget()).TrimEnd('/') + "/knock?version=" + global::System.Uri.EscapeDataString(CurrentBundleVersion.GetClientVersion()) + "&platform=" + Application.platform.ToString() /* [4 ก.ย. 2026] มือถือ (APK build เอง) ต้องส่ง Android ให้เซิร์ฟเลือก bundle ชุด Android */;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				string json;
				using (WebClient webClient = new WebClient())
				{
					json = webClient.DownloadString(url);
				}
				JObject jObject = JObject.Parse(json);
				JToken count = jObject["online_players"];
				OnlinePlayers = (count != null) ? (int)count : -1;
				ServerReachable = true;
			}
			catch (Exception)
			{
				// เซิร์ฟปิด/เน็ตมีปัญหา = จุดแดง ไม่ต้องโวยวาย
				ServerReachable = false;
				OnlinePlayers = -1;
			}
			finally
			{
				ServerStatusKnown = true;
			}
		});
		thread.IsBackground = true;
		thread.Start();
	}

	/// <summary>
	/// ข้อความสถานะสำหรับต่อท้ายชื่อเซิร์ฟบนหน้าไตเติ้ล — จุดสี + จำนวนคน
	/// ใช้ NGUI BBCode ⇒ label ที่เอาไปใช้ต้องเปิด supportEncoding ก่อน
	/// </summary>
	public static string StatusSuffix()
	{
		if (!ServerStatusKnown)
		{
			return "  [999999]●[-]";                       // เทา = กำลังเช็ค
		}
		if (!ServerReachable)
		{
			return "  [DE8A70]● ออฟไลน์[-]";                // แดง = ติดต่อไม่ได้
		}
		string people = (OnlinePlayers >= 0) ? ("  " + OnlinePlayers + " คน") : string.Empty;
		return "  [7FB877]●[-]" + people;                  // เขียว = รันอยู่
	}


	/// <summary>
	/// [แก้เอง] 31 ส.ค. 2026 — ย้ายมาจากมอด `DurangoClientCore` (ApplyWorldChunkRange/ApplyServerMenus)
	/// เจ้าของสั่งเลิกใช้ระบบมอดแล้วมาทำเป็นแพตช์แทน เพราะมอดหลุด/หายบ่อย
	///
	/// ตัวเกมไม่เคยยิง /knock ไปหาเซิร์ฟจริงเลย (Durango.Offline/Gateway.cs เป็นฝั่ง *ตอบ* /knock
	/// ของเซิร์ฟจำลองในเครื่อง คนละทาง) ⇒ ต้องยิงเองตรงนี้ตอนกดต่อเซิร์ฟ
	///
	/// ยิงใน thread แยกเสมอ — ถ้ายิงบน main thread แล้วเซิร์ฟช้า/ล่ม เกมจะค้างทั้งจอจนกว่าจะ timeout
	/// ค่าที่ได้เป็น field ธรรมดา อ่านทีหลังตอนเข้าโลก (InitChunkPool) ซึ่งช้ากว่าตรงนี้มาก ทันอยู่แล้ว
	/// ถ้ายิงไม่ติด = คงค่าเดิมของเกมไว้ (range 2) ไม่ทำให้อะไรพัง
	/// </summary>
	private static void FetchServerPolicy(string gatewayUrlRoot)
	{
		string url = gatewayUrlRoot.TrimEnd('/') + "/knock?version=" + global::System.Uri.EscapeDataString(CurrentBundleVersion.GetClientVersion()) + "&platform=" + Application.platform.ToString() /* [4 ก.ย. 2026] มือถือ (APK build เอง) ต้องส่ง Android ให้เซิร์ฟเลือก bundle ชุด Android */;
		Thread thread = new Thread((ThreadStart)delegate
		{
			try
			{
				string json;
				using (WebClient webClient = new WebClient())
				{
					json = webClient.DownloadString(url);
				}
				JObject jObject = JObject.Parse(json);
				JToken jToken = jObject["client_mod"];
				if (jToken == null)
				{
					return;
				}
				JToken jToken2 = jToken["world_chunk_range"];
				if (jToken2 != null)
				{
					WorldChunkRange = Math.Max(2, Math.Min(4, (int)jToken2));
				}
				JToken jToken3 = jToken["skip_region_selection"];
				if (jToken3 != null)
				{
					SkipRegionSelection = (bool)jToken3;
				}
				JArray jArray = jToken["enabled_menus"] as JArray;
				if (jArray != null)
				{
					List<string> list = new List<string>();
					foreach (JToken item in jArray)
					{
						list.Add((string)item);
					}
					EnabledMenus = list.ToArray();
				}
				JArray jArray2 = jToken["hidden_menus"] as JArray;
				if (jArray2 != null)
				{
					List<string> list2 = new List<string>();
					foreach (JToken item2 in jArray2)
					{
						string name = (string)item2;
						if (!string.IsNullOrEmpty(name))
						{
							list2.Add(name);
						}
					}
					HiddenMenus = list2.ToArray();
				}
				UnityEngine.Debug.Log("[durango] server policy: chunk_range=" + WorldChunkRange
					+ " menus=" + string.Join(",", EnabledMenus)
					+ " skip_region=" + SkipRegionSelection);
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.Log("[durango] knock failed (ใช้ค่าเดิมของเกมต่อ): " + ex.Message);
			}
		});
		thread.IsBackground = true;
		thread.Start();
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
