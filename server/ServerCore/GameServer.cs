using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// GameServer — ดูรายละเอียดที่ docs/server/GameServer.md

public class GameServer
{
    public const int DefaultPort = 8191;

    /// <summary>พอร์ตที่เปิดฟังจริง — Gateway ต้องบอกค่านี้ให้ client ไม่ใช่ DefaultPort</summary>
    public int Port { get; private set; } = DefaultPort;

    public static string PlayerSavePath;

    /// <summary>
    /// GP-14: true = เชื่อเลเวลที่ client ส่งมาทาง /sessions ทุกครั้ง (พฤติกรรมเดิม)
    /// false (ค่าเริ่มต้น) = ใช้เลเวลที่ server เซฟไว้ ค่าจาก client มีผลแค่ตอน login ครั้งแรก
    /// ตั้งด้วย <c>--trust-client-profile</c>
    /// </summary>
    public static bool TrustClientProfile;

    // ── H-2: คำสั่งทดสอบ (packet Cheat) ────────────────────────────────
    // เดิมใครต่อเข้ามาก็สั่งได้ทุกคำสั่ง รวมถึง control ที่ลากตัวละครคนอื่นไปไหนก็ได้
    // ตอนนี้ปิดเป็นค่าเริ่มต้น ต้องเปิดด้วย --enable-cheat และคำสั่งที่ยุ่งกับคนอื่นต้องเป็น admin

    /// <summary>เปิดคำสั่งทดสอบไหม (ตั้งด้วย <c>--enable-cheat</c>)</summary>
    public static bool CheatsEnabled;

    /// <summary>
    /// Role ของภูมิภาคที่ส่งไปกับ Welcome — ตัวนี้เป็นสวิตช์ปิดบทสนทนา NPC/ระบบสอนเล่น
    ///
    /// `PlayGuideSystem` ฝั่ง client รับค่านี้แล้ว **ออกจาก Initialize ทันทีถ้าเป็น Sandbox/Invalid**
    /// (ดู client/PlayGuideSystem.cs:237 → Initialize:610) ⇒ ไม่มีไดอะล็อกเด้งมาบังจอ
    /// อยากได้ระบบสอนเล่นกลับมาให้ตั้ง <c>--region-role Rural</c>
    /// </summary>
    public static Role RegionRole = Role.Sandbox;

    /// <summary>รายชื่อ admin (entity id หรือชื่อตัวละคร) — ตั้งด้วย <c>--admin</c> ซ้ำได้หลายครั้ง</summary>
    private static readonly HashSet<string> _admins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public static void AddAdmin(string idOrName)
    {
        if (!string.IsNullOrWhiteSpace(idOrName))
        {
            _admins.Add(idOrName.Trim());
        }
    }

    public static int AdminCount => _admins.Count;

    /// <summary>เป็น admin ไหม — ไม่ได้ตั้ง admin ไว้เลย = ไม่มีใครเป็น (คำสั่งที่ยุ่งกับคนอื่นถูกปิดสนิท)</summary>
    public static bool IsAdmin(string entityId, string name)
    {
        return (!string.IsNullOrEmpty(entityId) && _admins.Contains(entityId))
            || (!string.IsNullOrEmpty(name) && _admins.Contains(name.Trim()));
    }

    public class PlayerData
    {
        public string EntityId;
        public string Name;
        public string DisplayJson;
        public int Level;
        public ushort EntityType;
        public string SkillsJson;
        public int SkillPoints;
        public string KnownSkillsJson;
    }

    private readonly Dictionary<string, PlayerData> _playerData = new Dictionary<string, PlayerData>();
    private readonly object _playerDataLock = new object();

    // ── GP-12: session token ──────────────────────────────────────────────
    // เดิม Auth เชื่อ EntityId ที่ client ส่งมาดื้อ ๆ (และ /sessions ก็คืน token = entity id)
    // ใครก็ตามที่รู้ entity id ของคนอื่นจึงสวมรอยได้ทันที
    // ตอนนี้ /sessions เป็นคนออก token สุ่มและผูกไว้กับ entity id — Auth ต้องยื่นคู่ที่ตรงกัน

    /// <summary>token ที่ออกไปแล้วอยู่ได้นานแค่ไหน (วินาที)</summary>
    private const double SessionTtlSeconds = 12 * 3600;

    private sealed class Session
    {
        public string EntityId;
        public PlayerData Data;
        public double IssuedAt;
    }

    private readonly Dictionary<string, Session> _sessions = new Dictionary<string, Session>();
    private readonly object _sessionLock = new object();

    /// <summary>
    /// false = ยอมให้ Auth ที่ไม่มี token ผ่าน (ใช้กับ test-client รุ่นเก่า / debug เท่านั้น)
    /// ตั้งค่าด้วย <c>--insecure-auth</c>
    /// </summary>
    public bool RequireSessionToken { get; set; } = true;

    /// <summary>ออก token ใหม่ให้ session หนึ่ง ๆ แล้วผูกกับ entity id (เรียกจาก Gateway /sessions)</summary>
    public string IssueSession(string entityId, PlayerData data)
    {
        string token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        double now = Times.UnixTimeNow();
        lock (_sessionLock)
        {
            // เก็บกวาดของหมดอายุไปด้วย ไม่งั้น dictionary โตขึ้นเรื่อย ๆ ตลอดอายุ process
            var expired = new List<string>();
            foreach (KeyValuePair<string, Session> pair in _sessions)
            {
                if (now - pair.Value.IssuedAt > SessionTtlSeconds)
                {
                    expired.Add(pair.Key);
                }
            }
            for (int i = 0; i < expired.Count; i++)
            {
                _sessions.Remove(expired[i]);
            }
            _sessions[token] = new Session { EntityId = entityId, Data = data, IssuedAt = now };
        }
        return token;
    }

    /// <summary>
    /// ตรวจ Auth: token ต้องเป็นของจริงที่ /sessions ออกให้ และต้องตรงกับ entity id ที่อ้างมา
    /// token ไม่ถูกลบทิ้งหลังใช้ เพราะ client ใช้ token เดิมตอน reconnect (SendAuthMessage isReconnect)
    /// </summary>
    private bool TryAuthorize(Auth auth, out string entityId, out PlayerData data, out string reason)
    {
        entityId = auth.EntityId;
        data = null;
        reason = null;

        Session session = null;
        if (!string.IsNullOrEmpty(auth.SessionToken))
        {
            lock (_sessionLock)
            {
                if (_sessions.TryGetValue(auth.SessionToken, out Session s)
                    && Times.UnixTimeNow() - s.IssuedAt <= SessionTtlSeconds)
                {
                    session = s;
                }
            }
        }

        if (session == null)
        {
            if (RequireSessionToken)
            {
                reason = string.IsNullOrEmpty(auth.SessionToken) ? "ไม่มี session token" : "token ไม่รู้จักหรือหมดอายุ";
                return false;
            }
            // โหมด --insecure-auth: กลับไปเชื่อ entity id ที่ส่งมา
            entityId = string.IsNullOrEmpty(auth.EntityId) ? Guid.NewGuid().ToString() : auth.EntityId;
            data = GetPlayerData(entityId);
            return true;
        }

        // token จริง — entity id ที่ client อ้างต้องเป็นของ session นี้เท่านั้น
        if (!string.IsNullOrEmpty(auth.EntityId) && auth.EntityId != session.EntityId)
        {
            reason = $"token เป็นของ {session.EntityId} แต่อ้างเป็น {auth.EntityId}";
            return false;
        }
        entityId = session.EntityId;
        data = session.Data ?? GetPlayerData(entityId);
        return true;
    }

    private readonly Listener _listener = new Listener();
    private readonly ServerWorld _world;
    private readonly Dictionary<string, string> _namesByEntity = new Dictionary<string, string>();
    private readonly List<ConnState> _connections = new List<ConnState>();
    private readonly object _connLock = new object();

    // ── H-3: เพดานจำนวน connection + เส้นตายของ handshake ─────────────────
    // Connection 1 ตัวจองบัฟเฟอร์ ~4 MB **ตั้งแต่ตอน accept** (ก่อน Auth)
    // ถ้าไม่มีเพดาน/ไม่มี timeout แค่เปิด TCP ค้างไว้เฉย ๆ ก็ทำให้ RAM หมดได้

    /// <summary>รับพร้อมกันได้กี่เส้น (ตั้งด้วย <c>--max-connections</c>)</summary>
    public static int MaxConnections = 10;

    /// <summary>จาก IP เดียวกันได้กี่เส้น (กันคนเดียวจองหมด)</summary>
    public static int MaxConnectionsPerIp = 4;

    /// <summary>ต่อเข้ามาแล้วต้อง Auth ภายในกี่วินาที</summary>
    private const double AuthDeadlineSeconds = 15.0;

    /// <summary>Auth แล้วต้อง Ready ภายในกี่วินาที</summary>
    private const double ReadyDeadlineSeconds = 45.0;

    // ── M-6: rate limit ─────────────────────────────────────────────────
    // packet ขอ ~30 ไบต์ แต่ทำให้ server ทำงานหนักได้มาก (GetRecipes = 720 รายการ,
    // SetChunk = 9 chunk) ยิงรัว ๆ คนเดียวก็ทำ tps ตกทั้งเซิร์ฟ

    /// <summary>packet ต่อวินาทีที่ยอมให้ (ค่าปกติของ client อยู่หลักสิบ)</summary>
    public static int MaxPacketsPerSecond = 120;

    /// <summary>M5: mod handshake is opt-in for backwards compatibility; production can enable it with --require-mods.</summary>
    public ModNegotiationPolicy ModPolicy { get; } = new ModNegotiationPolicy();
    public string ModCatalogHash => PluginManager.Instance == null ? "" : ModNegotiation.BuildServerCatalog(PluginManager.Instance.Mods);

    /// <summary>เกินเพดานติดกันกี่วินาทีถึงตัดการเชื่อมต่อ</summary>
    private const int RateStrikesBeforeKick = 3;

    /// <summary>สถานะของแต่ละ connection — ใช้บังคับเส้นตายและกันเข้าซ้ำ</summary>
    private sealed class ConnState
    {
        public Durango.Offline.Connection Conn;
        public string Ip;
        public double AcceptedAt;
        public bool Authed;
        public bool ModHelloReceived;
        public bool PlayerCreated;

        // M-6: หน้าต่างนับ packet
        public double WindowStart;
        public int PacketsInWindow;
        public int Strikes;
        public bool Rejected;
    }

    public GameServer(ServerWorld world)
    {
        _world = world;
    }

    /// <summary>เปิดฟังพอร์ตเกม คืน false ถ้า bind ไม่สำเร็จ (GP-15)</summary>
    public bool Start(int port)
    {
        if (!_listener.Start(port))
        {
            return false;
        }
        Port = port;
        _listener.ClientAccepted += Listener_ClientAccepted;
        Console.WriteLine($"[gameserver] listening on 0.0.0.0:{port}");
        return true;
    }

    public void Close()
    {
        _listener.ClientAccepted -= Listener_ClientAccepted;
        _listener.Close();
        ConnState[] snapshot;
        lock (_connLock)
        {
            snapshot = _connections.ToArray();
            _connections.Clear();
        }
        for (int i = 0; i < snapshot.Length; i++)
        {
            try { snapshot[i].Conn.Close(); } catch (Exception e) { Console.WriteLine($"[gameserver] ปิด connection ไม่สำเร็จ: {e.Message}"); }
        }
    }

    private void Listener_ClientAccepted(Socket socket)
    {
        // H-3: เช็คเพดาน **ก่อน** สร้าง Connection เพราะ Connection จองบัฟเฟอร์ทันทีที่สร้าง
        string ip = "?";
        try
        {
            ip = (socket.RemoteEndPoint as System.Net.IPEndPoint)?.Address?.ToString() ?? "?";
        }
        catch (Exception)
        {
        }

        int total;
        int fromSameIp = 0;
        lock (_connLock)
        {
            total = _connections.Count;
            for (int i = 0; i < _connections.Count; i++)
            {
                if (_connections[i].Ip == ip)
                {
                    fromSameIp++;
                }
            }
        }
        if (MaxConnections <= 0 || MaxConnectionsPerIp <= 0 || total >= MaxConnections || fromSameIp >= MaxConnectionsPerIp)
        {
            Console.WriteLine($"[gameserver] ปฏิเสธ {ip}: เต็มเพดาน (ทั้งหมด {total}/{MaxConnections}, จาก IP นี้ {fromSameIp}/{MaxConnectionsPerIp})");
            try
            {
                socket.Close();
            }
            catch (Exception)
            {
            }
            return;
        }

        Durango.Offline.Connection connection = new Durango.Offline.Connection(socket);
        ConnState state = new ConnState
        {
            Conn = connection,
            Ip = ip,
            AcceptedAt = Times.UnixTimeNow(),
            WindowStart = Times.UnixTimeNow()
        };
        lock (_connLock)
        {
            _connections.Add(state);
        }
        string entityId = null;
        string playerName = null;
        PlayerData authedData = null;

        connection.Recv<GetClock>(delegate(GetClock getClock, PacketHeader header)
        {
            Clock msg = default;
            msg.ClientTime = getClock.Time;
            msg.ServerTime = Times.UnixTimeNow();
            connection.Send(msg, header.Seq);
        });
        connection.Recv<Auth>(delegate(Auth auth, PacketHeader header)
        {
            // GP-12: ไม่รับ entity id ที่ client อ้างลอย ๆ อีกแล้ว ต้องมี token จาก /sessions
            if (!TryAuthorize(auth, out string authedId, out PlayerData data, out string reason))
            {
                Console.WriteLine($"[auth] ปฏิเสธ {socket.RemoteEndPoint}: {reason} (อ้างเป็น {auth.EntityId})");
                connection.Send(default(Abort), header.Seq);
                connection.Close();
                return;
            }
            // H-8: Auth ซ้ำบนเส้นเดิมไม่มีเหตุผลที่ถูกต้อง และทำให้ state ของ connection สับสน
            if (state.Authed)
            {
                Console.WriteLine($"[auth] {ip} ส่ง Auth ซ้ำบน connection เดิม — ปฏิเสธ");
                connection.Send(default(Abort), header.Seq);
                return;
            }
            state.Authed = true;
            entityId = authedId;
            authedData = data;
            playerName = LookupName(entityId);
            SendWelcome(connection, entityId, playerName, header.Seq);
        });
        connection.Recv<ModHello>(delegate(ModHello hello, PacketHeader header)
        {
            if (!state.Authed || state.PlayerCreated || state.ModHelloReceived || state.Rejected)
            { connection.Send(default(Abort), header.Seq); return; }
            IReadOnlyList<PluginManager.LoadedModInfo> mods = PluginManager.Instance?.Mods ?? Array.Empty<PluginManager.LoadedModInfo>();
            ModNegotiationResult result = ModNegotiation.Validate(hello.ManifestJson, hello.CatalogHash, mods, ModPolicy);
            if (!result.Accepted)
            {
                state.Rejected = true;
                Console.WriteLine($"[mods] ปฏิเสธ handshake ของ {ip}: {result.Reason}");
                Error error = default; error.Text = "mod negotiation failed: " + result.Reason;
                connection.Send(error, header.Seq); connection.Close(); return;
            }
            state.ModHelloReceived = true;
        });
        connection.Recv<Ready>(delegate(Ready ready, PacketHeader header)
        {
            if (string.IsNullOrEmpty(entityId) || state.Rejected)
            {
                connection.Close();
                return;
            }
            if (ModPolicy.RequireHello && !state.ModHelloReceived)
            {
                state.Rejected = true;
                Error error = default; error.Text = "mod negotiation required before Ready";
                connection.Send(error, header.Seq); connection.Close(); return;
            }
            // H-8: Ready ซ้ำ = สร้าง ServerPlayer ซ้ำบนเส้นเดียว → ผีค้างในโลก + เซฟทับกันเอง
            if (state.PlayerCreated)
            {
                Console.WriteLine($"[gameserver] {playerName} ส่ง Ready ซ้ำ — ปฏิเสธ");
                connection.Send(default(Abort), header.Seq);
                return;
            }
            state.PlayerCreated = true;

            // H-8: เข้าพร้อมกัน 2 เส้นด้วย entity id เดียวกัน = ก๊อปของได้
            // (ต่างคนต่างโหลดกระเป๋าจากไฟล์เดียวกัน แล้วยัดใส่กล่องคนละรอบ)
            // เตะเส้นเก่าออกก่อนเสมอ — เซฟให้เรียบร้อยแล้วค่อยปิด
            ServerPlayer existing = _world.FindPlayer(entityId);
            if (existing != null)
            {
                Console.WriteLine($"[gameserver] {playerName} เข้าซ้ำจาก {ip} — เตะเส้นเดิมออก");
                existing.Kick("มีการเข้าเกมด้วยตัวละครนี้จากที่อื่น");
            }
            connection.Send(default(OK), header.Seq);
            // GP-12: ใช้ข้อมูลที่ผูกมากับ token ไม่ใช่ค้นจาก entity id ที่ client อ้าง
            PlayerData data = authedData ?? GetPlayerData(entityId);
            ServerPlayer player = new ServerPlayer(entityId, playerName, connection, _world, data);
            player.RegisterHandlers();
            player.SendSpawnBurst();
            _world.AddPlayer(player);
            // GP-11: เดิมมี ServerKnock.HostName = playerName ตรงนี้ ซึ่งทับชื่อเซิร์ฟ
            // ด้วยชื่อผู้เล่นคนล่าสุดที่เข้ามา ทำให้ LAN discovery โชว์ชื่อผิด
            // ชื่อเซิร์ฟถูกตั้งครั้งเดียวใน Program.cs แล้ว ไม่ต้องแตะอีก
            connection.ConnetionClosed += delegate
            {
                // GP-07: เซฟก่อนเอาออกจากโลก ไม่งั้นของที่เก็บมาทั้งเซสชันหายหมด
                try
                {
                    player.LeavePartyOnDisconnect();
                    player.Save();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[save] เซฟ {player.EntityId} ตอนออกเกมไม่สำเร็จ: {e.Message}");
                }
                _world.RemovePlayer(player);
            };
        });
        connection.StartReceive();
        Console.WriteLine($"[gameserver] client connected from {socket.RemoteEndPoint}");
    }

    private string LookupName(string entityId)
    {
        lock (_namesByEntity)
        {
            return _namesByEntity.TryGetValue(entityId, out string n) ? n : entityId;
        }
    }

    public void RegisterName(string entityId, string name)
    {
        lock (_namesByEntity)
        {
            _namesByEntity[entityId] = name;
        }
    }

    public void RegisterPlayerData(PlayerData data)
    {
        lock (_playerDataLock)
        {
            _playerData[data.EntityId] = data;
        }
    }

    public PlayerData GetPlayerData(string entityId)
    {
        lock (_playerDataLock)
        {
            return _playerData.TryGetValue(entityId, out PlayerData data) ? data : null;
        }
    }

    private void SendWelcome(Durango.Offline.Connection connection, string entityId, string name, uint seq)
    {
        Welcome msg = default;
        msg.UserId = entityId;
        msg.Name = name;
        msg.Region = new Region
        {
            Id = "1",
            TerrainId = "1",
            TemplateId = _world.Terrain.Info.region_template,
            Role = RegionRole,
            Name = _world.ServerName,
            CreatedAt = 0.0
        };
        msg.Storage = new Storage
        {
            Data = new Dictionary<string, byte[]>()
        };
        msg.Options = new Options
        {
            Bool = new[]
            {
                new BoolOption
                {
                    Key = "market.ui_enabled",
                    Value = true
                }
            },
            Int = new[]
            {
                new IntegerOption
                {
                    Key = "market.search.limit",
                    Value = 200L
                }
            },
            Float = null
        };
        msg.Archipelago = null;
        msg.PersonalRegionId = null;
        msg.Seasons = new Seasons
        {
            _Seasons = null
        };
        msg.SocialOptions = new SocialOptions
        {
            Options = new Dictionary<SocialOptionType, bool>()
        };
        msg.EngagementRewardSent = false;
        connection.Send(msg, seq);
    }

	public void Process()
	{
		_listener.Process();
		// Process simulation independently so a player/world exception cannot starve all sockets.
		try
		{
			_world.ProcessPlayers();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		double now = Times.UnixTimeNow();
		ConnState[] snapshot;
		lock (_connLock)
		{
			snapshot = _connections.ToArray();
		}
		for (int i = 0; i < snapshot.Length; i++)
		{
			ConnState state = snapshot[i];
			try
			{
				state.Conn.Process();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				state.Conn.Close();
			}
			bool remove = !state.Conn.Connected();
			if (remove)
			{
				// Always run the idempotent close path so disconnect handlers save/remove the player.
				state.Conn.Close();
			}
			if (!remove)
			{
				// M-6: นับ packet ต่อวินาที เกินเพดานติดกันหลายรอบ = ตัด
				double windowAge = now - state.WindowStart;
				if (windowAge >= 1.0)
				{
					int received = state.Conn.TotalReceivedPackets;
					int inWindow = received - state.PacketsInWindow;
					state.PacketsInWindow = received;
					state.WindowStart = now;
					if (inWindow > MaxPacketsPerSecond)
					{
						state.Strikes++;
						Console.WriteLine($"[gameserver] {state.Ip} ยิง {inWindow} packet/วิ (เพดาน {MaxPacketsPerSecond}) ครั้งที่ {state.Strikes}");
						if (state.Strikes >= RateStrikesBeforeKick)
						{
							Console.WriteLine($"[gameserver] ตัด {state.Ip}: ยิง packet ถี่เกินติดกัน {state.Strikes} วินาที");
							state.Conn.Close();
							remove = true;
						}
					}
					else if (state.Strikes > 0)
					{
						state.Strikes--;
					}
				}
			}
			if (!remove)
			{
				double age = now - state.AcceptedAt;
				remove = (!state.Authed && age > AuthDeadlineSeconds)
					|| (state.Authed && !state.PlayerCreated && age > ReadyDeadlineSeconds);
				if (remove)
				{
					Console.WriteLine($"[gameserver] ตัด {state.Ip}: ต่อมา {age:F0} วิ แล้วยัง{(state.Authed ? "ไม่ Ready" : "ไม่ Auth")}");
					state.Conn.Close();
				}
			}
			if (remove)
			{
				lock (_connLock)
				{
					_connections.Remove(state);
				}
			}
		}
	}
}
