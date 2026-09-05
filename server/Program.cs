using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using DurangoServer.Core;

namespace DurangoServer;

public static class Program
{
    // GP-01: Windows ตั้ง timer resolution ไว้ที่ 15.6 ms เป็นค่าเริ่มต้น
    // ทำให้ Thread.Sleep(5) นอนจริง ~15.6 ms → main loop ได้แค่ ~64 รอบ/วินาที
    // timeBeginPeriod(1) ดันลงมาเหลือ 1 ms ทำให้ Sleep สั้น ๆ แม่นขึ้น
    [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
    private static extern uint TimeBeginPeriod(uint uPeriod);

    [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
    private static extern uint TimeEndPeriod(uint uPeriod);

    /// <summary>รอบต่อวินาทีที่ต้องการของ main loop</summary>
    private const int TargetTps = 120;

    /// <summary>พิมพ์สถิติ tps ทุกกี่วินาที (0 = ปิด)</summary>
    private const int StatsIntervalSeconds = 30;

    /// <summary>อัปเดต ServerStats (ให้ admin panel อ่าน) ทุกกี่วินาที — ถี่กว่า console print เพราะไม่ได้พิมพ์อะไร</summary>
    private const int PanelStatsIntervalSeconds = 2;

    /// <summary>GP-07: เซฟอัตโนมัติทุกกี่วินาที (0 = ปิด) — เซฟเฉพาะที่มีอะไรเปลี่ยน</summary>
    private const int AutoSaveIntervalSeconds = 60;

    /// <summary>เวลาที่ log error ซ้ำ ๆ ครั้งล่าสุด (กัน log ท่วมจอถ้าพังทุก tick)</summary>
    private static readonly Dictionary<string, double> _lastErrorAt = new Dictionary<string, double>();

    /// <summary>เรียกงานประจำ tick แบบไม่ให้ exception หลุดไปฆ่า main loop</summary>
    private static void SafeProcess(string what, Action work)
    {
        try
        {
            work();
        }
        catch (Exception e)
        {
            double now = Durango.Utils.Times.UnixTimeNow();
            if (!_lastErrorAt.TryGetValue(what, out double last) || now - last > 5.0)
            {
                _lastErrorAt[what] = now;
                Console.WriteLine($"[fatal-กันไว้] {what} โยน exception: {e}");
            }
        }
    }

    public static void Main(string[] args)
    {
        // admin web panel: เก็บสำเนา console log ล่าสุดไว้ใน memory ให้ /admin/log อ่านได้ (ดู LiveLog.cs)
        Console.SetOut(new LiveLogTextWriter(Console.Out));
        CrashLog.Install();

        string dataDir = "data";
        string gameFilesDir = null;   // [4 ก.ย. 2026] --gamefiles · ว่าง = <data>/gamefiles
        string terrainId = "ri35te";
        string serverName = "DurangoTH Community Server";
        int gamePort = GameServer.DefaultPort;
        int gatewayPort = Gateway.DefaultPort;
        string assetBundleDir = null;
        bool insecureAuth = false;
        bool enableRadiotower = false;      // GP-12
        int radiotowerPort = 0;             // 0 = ยังไม่ระบุ ⇒ ใช้ gamePort + 1 (ค่าเดิม 8191+1 = 8192)
        string publicHost = null;
        string islandId = null;         // Beta 1.1: โหมดหลายเกาะ
        bool recipeCheck = false;       // --recipe-check: ตรวจข้อมูลคราฟต์/ทำอาหารแล้วออก
        bool dataCheck = false;         // --data-check: เทียบตาราง C# กับ data/assets/*.json แล้วออก
        bool modPackCheck = false;
        string modsDir = "mods";        // ระบบ mod: โฟลเดอร์ .dll (ดู ServerCore/Modding/PluginManager.cs)
        bool requireMods = false;
        bool allowUnknownOptionalMods = true;
        bool requireModSignatures = false;
        string modPublicKey = null;
        string clientModAllowlist = null;
        // Retail client enum does not contain the decompiled-source name SingleMode.
        // Custom remote gateways use Offline transport mode; the client mod selectively
        // enables server-managed menus after joining the remote world.
        string clusterMode = "Offline";
        string adminToken = null;       // กัน /admin/* — ว่าง = ไม่ auth (ค่าเดิม เหมาะกับรันในเครื่อง/LAN เท่านั้น)

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--data":
                    dataDir = args[++i];
                    break;
                case "--gamefiles":
                    // [4 ก.ย. 2026] โฟลเดอร์ไฟล์เกมฉบับจริง (มี filelist.json) ให้ launcher ตรวจ/ซ่อมทีละไฟล์
                    gameFilesDir = args[++i];
                    break;
                case "--terrain":
                    terrainId = args[++i];
                    break;
                case "--name":
                    serverName = args[++i];
                    break;
                case "--game-port":
                    gamePort = int.Parse(args[++i]);
                    break;
                case "--gateway-port":
                    gatewayPort = int.Parse(args[++i]);
                    break;
                case "--assetbundles":
                    assetBundleDir = args[++i];
                    break;
                case "--assetbundles-android":
                    // [Android] ชุด bundle ของ Android (จาก cache เครื่องผู้เล่น — ดู TodoList/ROADMAP-ANDROID.md)
                    Gateway.AssetBundleAndroidDir = args[++i];
                    break;
                case "--player-save":
                    GameServer.PlayerSavePath = args[++i];
                    break;
                case "--saves":
                    SaveStore.Root = args[++i];     // GP-07
                    break;
                case "--max-connections":
                    // H-3: เพดานจำนวน connection พร้อมกัน
                    GameServer.MaxConnections = int.Parse(args[++i]);
                    break;
                case "--max-connections-per-ip":
                    GameServer.MaxConnectionsPerIp = int.Parse(args[++i]);
                    break;
                case "--region-role":
                    // สวิตช์ปิด/เปิดบทสนทนา NPC + ระบบสอนเล่น (Sandbox = ปิด, Rural = เปิดแบบเกมจริง)
                    if (Enum.TryParse(args[++i], true, out Shared.Region.Role parsedRole))
                    {
                        GameServer.RegionRole = parsedRole;
                    }
                    else
                    {
                        Console.WriteLine($"[warn] --region-role '{args[i]}' ไม่รู้จัก — ใช้ {GameServer.RegionRole} ต่อ");
                    }
                    break;
                case "--whitelist":
                    // H-1: ไฟล์รายชื่อที่อนุญาต (entity id หรือชื่อตัวละคร บรรทัดละ 1)
                    AccountStore.WhitelistPath = args[++i];
                    break;
                case "--no-ip-bind":
                    // H-1: ไม่ผูก entity id กับ IP แรกที่จอง (ใช้เมื่อผู้เล่นเน็ตเปลี่ยน IP บ่อย)
                    AccountStore.BindToFirstIp = false;
                    break;
                case "--loose-ip-match":
                    // เทียบ IP แบบวงเดียวกัน (/24) แทนตรงเป๊ะ — สำหรับผู้เล่นที่ใช้ VPN/เน็ตมือถือ
                    // ที่สลับ IP ในวงเดิมตลอด (เช่น Cloudflare WARP) ดู AccountStore.LooseIpMatch
                    AccountStore.LooseIpMatch = true;
                    break;
                case "--no-account-check":
                    // H-1: ปิดการตรวจเจ้าของทั้งหมด (เทสในเครื่องเดียว)
                    AccountStore.Disabled = true;
                    break;
                case "--enable-cheat":
                    // H-2: เปิดคำสั่งทดสอบ (เสกของ/ฟื้นเลือด/เรียกสัตว์/control) — อย่าเปิดบนเซิร์ฟสาธารณะ
                    GameServer.CheatsEnabled = true;
                    break;
                case "--admin":
                    // ระบุได้หลายครั้ง: --admin ชื่อตัวละคร --admin <entityId>
                    GameServer.AddAdmin(args[++i]);
                    break;
                case "--trust-client-profile":
                    // GP-14: กลับไปเชื่อเลเวลจาก client ทุกครั้ง (เช่นตอนเล่นคนเดียวแล้วเลเวลอัปที่เกาะตัวเอง)
                    GameServer.TrustClientProfile = true;
                    break;
                case "--radiotower":
                    // M-5: เปิดพอร์ตแชทส่วนตัว (ตอนนี้ตรวจ session token แล้ว ปลอมชื่อไม่ได้)
                    enableRadiotower = true;
                    break;
                case "--radiotower-port":
                    // ไม่ระบุ = gamePort + 1 — ต้องเลื่อนตามชุดพอร์ตที่ใช้ (เช่นเกม 8291 ⇒ แชท 8292)
                    radiotowerPort = int.Parse(args[++i]);
                    enableRadiotower = true;
                    break;
                case "--insecure-auth":
                    // GP-12: ปิดการตรวจ session token — ใช้เฉพาะตอน debug ด้วย client ที่ไม่ผ่าน HTTP
                    insecureAuth = true;
                    break;
                case "--island":
                    // Beta 1.1: เปิดเป็น "เกาะ" ตามทะเบียนใน data/islands.json
                    // (terrain · พอร์ต · ไฟล์ config · ไฟล์เซฟโลก มาจากทะเบียนหมด)
                    islandId = args[++i];
                    break;
                case "--recipe-check":
                    recipeCheck = true;
                    break;
                case "--data-check":
                    // ตรวจว่าตารางที่ hardcode ไว้ยังตรงกับข้อมูลของเกมไหม (ดู DataDriftCheck.cs)
                    dataCheck = true;
                    break;
                case "--mod-pack-check":
                    modPackCheck = true;
                    break;
                case "--mods":
                    // ระบบ mod: ระบุโฟลเดอร์ .dll เอง (default "mods" ข้าง ๆ exe) — ใส่ "" หรือโฟลเดอร์
                    // ที่ไม่มีจริงเพื่อปิด mod ทั้งหมดชั่วคราวโดยไม่ต้องย้ายไฟล์ออก
                    modsDir = args[++i];
                    break;
                case "--require-mods":
                    requireMods = true;
                    break;
                case "--no-unknown-optional-mods":
                    allowUnknownOptionalMods = false;
                    break;
                case "--require-mod-signatures":
                    requireModSignatures = true;
                    break;
                case "--mod-public-key":
                    modPublicKey = args[++i];
                    break;
                case "--client-mod-allowlist":
                    clientModAllowlist = args[++i];
                    break;
                case "--public-host":
                    // ที่อยู่ที่ client ใช้ต่อ TCP (พอร์ตเกม/แชท) — เช่น 127.0.0.1 เมื่อเล่น
                    // ผ่าน Cloudflare Tunnel (client ต่อผ่าน cloudflared access tcp บนเครื่องตัวเอง)
                    // ไม่ระบุ = ใช้ host ที่ client เรียก gateway มา (เล่นในวงแลน)
                    publicHost = args[++i];
                    break;
                case "--cluster-mode":
                    // /entry ตอบ cluster_mode เท่านี้ (SingleMode เดิม, Online เพื่อเทสตลาด/สารานุกรม/แชทส่วนตัว)
                    // client มีจุดเช็ค ClusterMode == Mode.Online เกือบ 30 จุด — Online ยังไม่ได้เทสครบทุกจุด
                    // ใช้ค่า default "SingleMode" ถ้าไม่ระบุ (เซิร์ฟที่รันอยู่แล้วไม่กระทบ)
                    clusterMode = args[++i];
                    break;
                case "--url-prefix":
                    // [3 ก.ย. 2026] เซิร์ฟ "ทดสอบ" บนพอร์ตอื่นสำหรับมือถือ: APK แพตช์ gateway เป็น http://ip:8290/p
                    // แล้วเกมต่อท้าย "8190" เอง ⇒ request มาเป็น /p8190/... — ตัดคำนำหน้านี้ก่อน route
                    // (ดู Durango.Offline.WebServer.PathPrefix) · client PC ที่ไม่มี prefix ยังใช้ได้ปกติ
                    {
                        string prefix = args[++i].Trim();
                        if (!prefix.StartsWith("/")) prefix = "/" + prefix;
                        Durango.Offline.WebServer.PathPrefix = prefix.TrimEnd('/');
                        Console.WriteLine($"[web] ตัด URL prefix \"{Durango.Offline.WebServer.PathPrefix}\" ก่อน route (เซิร์ฟทดสอบสำหรับมือถือ)");
                    }
                    break;
                case "--admin-token":
                    // กัน /admin/* (สถานะเซิร์ฟ/log สด/เตะผู้เล่น/สั่ง cheat/แก้ config) — ไม่ระบุ = ไม่ auth
                    // เหมือนเดิม (เหมาะกับรันในเครื่อง/LAN เท่านั้น) **ต้องตั้งถ้าเปิดพอร์ตออกอินเทอร์เน็ต**
                    // เข้าหน้า admin ด้วย http://host:port/admin?token=<ค่านี้> ครั้งแรก เบราว์เซอร์จะจำให้เอง
                    adminToken = args[++i];
                    break;
            }
        }

        // --assetbundles: โฟลเดอร์ assetbundle ของตัวเกม (default: หา DurangoV2 ข้าง ๆ โปรเจกต์)
        // server จะ serve ไฟล์เหล่านี้ทาง /assetbundles/* เพราะ client โหลดจาก URL ที่ /knock ตอบ
        if (string.IsNullOrEmpty(assetBundleDir))
        {
            string candidate = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "game", "DurangoV2_Data", "StreamingAssets", "AssetBundles");
            if (Directory.Exists(candidate))
            {
                assetBundleDir = candidate;
            }
        }

        // --player-save: save ของเกาะตัวเอง (0.player) ใช้เป็น fallback ถ้า /sessions ไม่ส่งข้อมูลผู้เล่นมา
        if (string.IsNullOrEmpty(GameServer.PlayerSavePath))
        {
            string candidate = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "game", "AppData", "offline", "multi", "0.player");
            if (File.Exists(candidate))
            {
                GameServer.PlayerSavePath = candidate;
            }
        }

        ServerKnock.HostName = serverName;

        Console.WriteLine("=== DurangoServer ===");

        // Beta 1.1: โหมดหลายเกาะ — ทุกอย่างของเกาะมาจากทะเบียนเดียวกัน
        // [เพิ่มเอง] มาโครคำสั่งทดสอบที่ตัวเกมนิยามมาเอง (data/cheat_macros.json)
        CheatMacros.Load(dataDir);
        // [4 ก.ย. 2026] รายชื่อคนที่ถูกระงับ — อยู่ที่ data/bans.json ที่ทุกเกาะใช้ร่วมกัน
        // (โหลดก่อนเปิดพอร์ต ไม่งั้นคนแรกที่ต่อเข้ามาจะยังไม่ถูกตรวจ)
        BanList.Load(dataDir);
        string configPath = Path.Combine(dataDir, "config.json");
        if (!string.IsNullOrEmpty(islandId))
        {
            IslandRegistry.Load(Path.Combine(dataDir, "islands.json"));
            if (!IslandRegistry.Select(islandId))
            {
                Console.WriteLine("[fatal] ไม่มีเกาะนี้ในทะเบียน");
                return;
            }
            IslandInfo isle = IslandRegistry.Current;
            terrainId = isle.Terrain;
            gamePort = isle.GamePort;
            gatewayPort = isle.GatewayPort;
            serverName = isle.Name;
            ServerKnock.HostName = serverName;
            configPath = Path.Combine(dataDir, "islands", isle.Id, "config.json");
            Console.WriteLine($"[island] {isle}");
        }
        Console.WriteLine($"terrain: {terrainId} | data dir: {dataDir}");

        // ค่าปรับสมดุล (เรทเกิดสัตว์ · เลือด/ดาเมจ · exp) อยู่ในไฟล์ JSON แก้ได้ระหว่างเซิร์ฟรัน
        ServerConfig.Load(configPath);
        GatheringTools.Load(dataDir);
        JobCatalog.Load(dataDir);
        SkillParity.Report();

        if (recipeCheck)
        {
            // ตรวจข้อมูลอย่างเดียว ไม่ต้องโหลด terrain / เปิดพอร์ต
            Environment.ExitCode = RecipeCheck.Run();
            return;
        }
        if (dataCheck)
        {
            // เทียบข้อมูลอย่างเดียว ไม่ต้องโหลด terrain / เปิดพอร์ต
            Environment.ExitCode = DataDriftCheck.Run(dataDir);
            return;
        }
        if (modPackCheck)
        {
            Environment.ExitCode = ModPackCheck.Run(modsDir);
            return;
        }

        TerrainStore terrain;
        try
        {
            terrain = TerrainStore.Load(dataDir, terrainId);
        }
        catch (Exception e)
        {
            Console.WriteLine("[fatal] terrain load failed: " + e.Message);
            return;
        }
        Console.WriteLine($"terrain loaded: {terrain.Width}x{terrain.Height}, entry={terrain.EntryPoint.x},{terrain.EntryPoint.y}");

        ServerWorld world = new ServerWorld(terrain, serverName);
        // GP-07: โหลดสิ่งปลูกสร้าง + ต้นไม้ที่ถูกเก็บไปแล้ว ก่อนรับ client
        Console.WriteLine($"[save] โฟลเดอร์เซฟ: {System.IO.Path.GetFullPath(SaveStore.Root)}");
        world.Load();
        world.Animals.SpawnInitial();      // เฟส C — สัตว์ไม่ถูกเซฟ เกิดใหม่ทุกครั้งที่เปิดเซิร์ฟ

        // ระบบ mod: โหลดหลัง world พร้อมแล้ว แต่ก่อนเปิดพอร์ตรับผู้เล่น (ดู ServerCore/Modding/)
        PluginManager.LoadAll(world, modsDir);

        GameServer gameServer = new GameServer(world);
        gameServer.RequireSessionToken = !insecureAuth;      // GP-12
        gameServer.ModPolicy.RequireHello = requireMods;
        gameServer.ModPolicy.AllowUnknownOptional = allowUnknownOptionalMods;
        gameServer.ModPolicy.RequireSignatures = requireModSignatures;
        gameServer.ModPolicy.TrustedPublicKey = modPublicKey;
        if (!string.IsNullOrWhiteSpace(clientModAllowlist))
            gameServer.ModPolicy.ClientAllowlist = ModNegotiation.LoadClientAllowlist(clientModAllowlist);
        Console.WriteLine($"[mods] negotiation={(requireMods ? "required" : "optional")} · unknown optional={(allowUnknownOptionalMods ? "allowed" : "rejected")} · signatures={(requireModSignatures ? "required" : "optional")}");
        Console.WriteLine($"[gameserver] เพดาน connection {GameServer.MaxConnections} เส้น (จาก IP เดียวกัน {GameServer.MaxConnectionsPerIp})");
        Console.WriteLine(GameServer.RegionRole == Shared.Region.Role.Sandbox || GameServer.RegionRole == Shared.Region.Role.Invalid
            ? $"[region] role={GameServer.RegionRole} — ปิดบทสนทนา NPC/ระบบสอนเล่น (เปลี่ยนด้วย --region-role Rural)"
            : $"[region] role={GameServer.RegionRole} — ระบบสอนเล่นของ client ทำงานตามปกติ");
        // H-1: สรุปสถานะการตรวจเจ้าของ entity id
        if (AccountStore.Disabled)
        {
            Console.WriteLine("[account] ⚠️ ปิดการตรวจเจ้าของ entity id (--no-account-check) — ใครก็สวมรอยกันได้");
        }
        else
        {
            int listed = AccountStore.LoadWhitelist();
            Console.WriteLine(AccountStore.WhitelistActive
                ? $"[account] รายชื่อที่อนุญาต {listed} รายการ · ผูก IP: {(AccountStore.BindToFirstIp ? "เปิด" : "ปิด")}"
                : $"[account] ไม่ได้ตั้งรายชื่อที่อนุญาต (ใครมาก่อนจองก่อน) · ผูก IP: {(AccountStore.BindToFirstIp ? "เปิด" : "ปิด")}");
        }
        // H-2: บอกให้ชัดตอนเปิดเซิร์ฟว่าคำสั่งทดสอบเปิดอยู่ไหม
        Console.WriteLine(GameServer.CheatsEnabled
            ? $"[cheat] ⚠️ เปิดคำสั่งทดสอบอยู่ (--enable-cheat) · admin {GameServer.AdminCount} คน"
            : "[cheat] คำสั่งทดสอบปิดอยู่ — เปิดด้วย --enable-cheat ถ้าต้องการเทส");
        if (insecureAuth)
        {
            Console.WriteLine("[auth] ⚠️ --insecure-auth: ไม่ตรวจ session token — ใครก็สวมรอยเป็นใครก็ได้");
        }
        // GP-15: Start คืน false ถ้า bind ไม่สำเร็จ — เดิมกลืน exception แล้วเซิร์ฟดูเหมือนรันอยู่แต่ไม่รับใคร
        if (!gameServer.Start(gamePort))
        {
            Console.WriteLine($"[fatal] เปิดพอร์ตเกม {gamePort} ไม่ได้ — พอร์ตถูกใช้อยู่ (เปิดเซิร์ฟซ้ำ?) หรือไม่มีสิทธิ์");
            return;
        }
        // Radiotower = server แชทแยก (พอร์ตเกม + 1, client ต่อตาม radiotower_addresses ใน /entry)
        //
        // M-5 (แก้แล้ว): เดิมพอร์ตนี้ไม่มี auth เลย ใครต่อเข้ามาก็ประกาศตัวเป็นใครก็ได้แล้วพูดแทนคนนั้น
        // ตอนนี้ Tune ต้องยื่น session token ที่ /sessions ออกให้ (เหมือน Auth ของพอร์ตเกม)
        // และ server เติมชื่อคนพูดเอง (GP-05) — เปิดได้แล้วบนเซิร์ฟสาธารณะ
        RadiotowerServer radiotower = new RadiotowerServer(gameServer);
        if (enableRadiotower)
        {
            // ไม่ระบุ --radiotower-port = เกาะไปกับพอร์ตเกม (8191 ⇒ 8192 เท่าค่าเดิม, 8291 ⇒ 8292)
            int chatPort = radiotowerPort > 0 ? radiotowerPort : gamePort + 1;
            if (!radiotower.Start(chatPort))
            {
                Console.WriteLine($"[warn] เปิดพอร์ต radiotower {chatPort} ไม่ได้ — แชทส่วนตัวจะใช้ไม่ได้ แต่เล่นต่อได้");
                enableRadiotower = false;
            }
        }
        else
        {
            Console.WriteLine("[radiotower] ปิดอยู่ — เปิดด้วย --radiotower (หรือ --radiotower-port <พอร์ต>)");
        }

        if (!string.Equals(clusterMode, "SingleMode", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[gateway] cluster_mode = {clusterMode} (ทดลอง — client มีจุดเช็ค ClusterMode == Online หลายสิบจุดที่ยังไม่ได้เทสครบ)");
            if (clusterMode.Equals("Online", StringComparison.OrdinalIgnoreCase) && !enableRadiotower)
            {
                Console.WriteLine("[gateway] แนะนำเปิด --radiotower ด้วย — โหมด Online client จะโชว์แท็บแชทส่วนตัว");
            }
        }

        Gateway gateway;
        try
        {
            // ข่าว/ประกาศบน DinoWorld Launcher อ่านจาก data/launcher_news.json (แก้ไฟล์ได้ตลอดไม่ต้อง restart)
            Gateway.LauncherNewsPath = Path.Combine(dataDir, "launcher_news.json");
            Gateway.AssetsDir = Path.Combine(dataDir, "assets");
            Gateway.GameFilesDir = string.IsNullOrWhiteSpace(gameFilesDir)
                ? Path.Combine(dataDir, "gamefiles")
                : gameFilesDir;
            Console.WriteLine("[launcher] ไฟล์เกมสำหรับตรวจ/ซ่อม: {0} ({1})",
                Path.GetFullPath(Gateway.GameFilesDir),
                File.Exists(Path.Combine(Gateway.GameFilesDir, "filelist.json")) ? "มี filelist.json" : "ยังไม่มี filelist.json");

            // [4 ก.ย. 2026] "ทุกเครื่องใช้ไฟล์เดียวกัน" — เซิร์ฟอ่านข้อมูลเกม (สูตรคราฟต์ + สิ่งปลูกสร้าง)
            // จาก JSON ตัวเดียวกับที่เสิร์ฟให้ client ผ่าน /assets/* + สร้าง manifest ให้เทียบชุดข้อมูลได้
            GameData.LoadAll(Gateway.AssetsDir);

            // [เพิ่มเอง] 31 ส.ค. 2026 — ผูก "ระยะที่ client วาด" กับ "ระยะที่เซิร์ฟส่งจริง"
            // ตัวเกมเอา world_chunk_range ไปสร้าง ChunkPool ตรง ๆ ถ้ามันกว้างกว่า ChunkSendRange
            // วงนอกจะเป็นที่ว่างสีเทาตัดตรง ๆ (ดู ClientModPolicy.ServerChunkSendRange)
            ClientModPolicy.ServerChunkSendRange = ServerConfig.Current.World.ChunkSendRange;

            // 1 chunk = 16 tile ⇒ รัศมีที่ผู้เล่นมองเห็น = (range*2+1)*16/2
            // ถ้า ViewRangeTiles แคบกว่านี้ สัตว์/ผู้เล่นที่อยู่ในจอแต่นอกระยะจะไม่ได้รับแพ็กเก็ต
            // อัปเดต ⇒ **ยืนแข็งอยู่กับที่จนกว่าจะมีอะไรไปกระตุ้น** (เจ้าของเจอ: "ไดโนถูกสตัน")
            float renderRadiusTiles = (ServerConfig.Current.World.ChunkSendRange * 2 + 1) * 16f / 2f;
            float view = ServerConfig.Current.World.ViewRangeTiles;
            if (view > renderRadiusTiles + 1f)
            {
                Console.WriteLine($"[world] ⚠️ ViewRangeTiles={view} กว้างกว่าระยะ terrain ที่ส่ง " +
                    $"({renderRadiusTiles} tile จาก ChunkSendRange={ServerConfig.Current.World.ChunkSendRange}) " +
                    "— ส่งข้อมูลสัตว์เกินพื้นดินที่ผู้เล่นมี (เรดาร์) ลด ViewRangeTiles ให้ใกล้กล้องจริง (~12–16)");
            }
            gateway = new Gateway(gameServer, world, gatewayPort, assetBundleDir,
                enableRadiotower ? radiotower.Port : 0, publicHost, Path.Combine(dataDir, "reports"), clusterMode, adminToken);
            Console.WriteLine($"[gateway] listening on {gateway.BindPrefix} (UDP knock: {gatewayPort + 1})");
        }
        catch (Exception e)
        {
            Console.WriteLine("[fatal] gateway start failed: " + e.Message);
            Console.WriteLine("  hint: run as Administrator or free the port first");
            try { if (enableRadiotower) radiotower.Close(); } catch (Exception) { }
            try { gameServer.Close(); } catch (Exception) { }
            return;
        }

        // GP-01: ดัน timer resolution ลงเหลือ 1 ms ไม่งั้น Sleep สั้น ๆ จะนอนจริง ~15.6 ms
        bool hiResTimer = false;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                hiResTimer = TimeBeginPeriod(1) == 0;
            }
            catch (Exception)
            {
            }
        }
        Console.WriteLine(hiResTimer
            ? $"[loop] target {TargetTps} tps (timer resolution 1ms)"
            : $"[loop] target {TargetTps} tps (ใช้ timer ปกติของระบบ — tps จริงอาจต่ำกว่านี้)");

        // GP-07: กด Ctrl+C แล้วต้องได้เซฟก่อนตาย ไม่งั้นงานตั้งแต่ autosave ล่าสุดหาย
        Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;                 // ขอจัดการเอง อย่าเพิ่งฆ่า process
            Console.WriteLine();
            Console.WriteLine("[save] กำลังปิดเซิร์ฟ — เซฟทุกอย่างก่อน...");
            PluginManager.Instance?.DisableAll();
            int n = world.SaveAll(force: true);
            bool modsOk = PluginManager.Instance?.FlushStorage() ?? true;
            Console.WriteLine($"[save] เขียนไป {n} ไฟล์ + mod storage {(modsOk ? "ครบ" : "มีบางตัวล้มเหลว")} ปิดเรียบร้อย");
            Environment.Exit(0);
        };

        // ตรวจตารางเควสตอนเปิดเซิร์ฟ — พิมพ์ผิดใน QuestData จะทำให้เควสเงียบหายไปเฉย ๆ
        // ถ้าไม่จับตรงนี้ จะไปเจอตอนผู้เล่นเล่นค้างอยู่ครึ่งสาย (ดู docs/server/Quests.md)
        if (!DurangoServer.Core.QuestData.ValidateAndReport())
        {
            Console.WriteLine("[quest] ⚠️ เควสบางอันจะไม่ทำงาน — แก้ QuestData.cs แล้ว build ใหม่");
        }

        Console.WriteLine("server running. Ctrl+C to stop.");

        double tickMs = 1000.0 / TargetTps;
        Stopwatch clock = Stopwatch.StartNew();
        double nextTickAt = 0.0;
        int ticksSinceReport = 0;
        double lastReportAt = 0.0;
        double lastSaveAt = 0.0;
        int ticksSincePanelUpdate = 0;
        double lastPanelUpdateAt = 0.0;
        double lastModsTickAt = 0.0;    // ระบบ mod: ไว้คำนวณ deltaSeconds ให้ OnTick

        // แบ็กอัพเซฟตามรอบ (config → Save) · BackupOnStartup = มีจุดย้อนกลับตั้งแต่เพิ่งเปิดเซิร์ฟ
        SaveBackup.Schedule(0.0, ServerConfig.Current.Save?.BackupOnStartup ?? true);
        try
        {
            while (true)
            {
                // ⚠️ exception เดียวที่หลุดออกมาจากตรงนี้ = เซิร์ฟดับทั้งใบ ผู้เล่นหลุดหมด
                // และงานตั้งแต่ autosave ล่าสุด (สูงสุด 60 วิ) หาย — จับแยกทีละระบบแล้วเล่นต่อ
                SafeProcess("gameserver", gameServer.Process);
                SafeProcess("gateway", gateway.Process);
                if (enableRadiotower)
                {
                    SafeProcess("radiotower", radiotower.Process);
                }

                ticksSinceReport++;
                ticksSincePanelUpdate++;
                nextTickAt += tickMs;
                double now = clock.Elapsed.TotalMilliseconds;
                double delay = nextTickAt - now;
                if (delay >= 1.0)
                {
                    Thread.Sleep((int)delay);
                }
                else if (delay > 0.0)
                {
                    Thread.SpinWait(100);
                }
                else
                {
                    // ตามไม่ทัน — รีเซ็ตฐานเวลา ไม่ให้หนี้เวลาสะสมแล้วไล่ยิงรัว
                    nextTickAt = now;
                }

                // แก้ config.json ระหว่างเซิร์ฟรันแล้วมีผลทันที (ตรวจไฟล์ทุก 5 วินาที)
                SafeProcess("config-reload", () => ServerConfig.Tick(now / 1000.0));

                // ระบบ mod: เรียกทุก tick จริง (~120/วิ) — mod เขียนงานหนักเองระวังด้วย
                double modsDt = Math.Min(0.25, Math.Max(0.0, (now - lastModsTickAt) / 1000.0));
                lastModsTickAt = now;
                SafeProcess("mods-tick", () => PluginManager.Instance?.FireTick(modsDt));

                // เลือด/สตามินา/ความล้า — ต้องเดินต่อแม้ผู้เล่นไม่ได้ทำอะไร
                // (ล้าเต็มแล้วเลือดไหลลงจนตายได้ ต้องมีคนคอยนับให้)
                SafeProcess("survival", () => world.TickSurvival(Durango.Utils.Times.UnixTimeNow()));

                // [4 ก.ย. 2026] รอบ autosave ย้ายไปตั้งใน config.json → Save.AutoSaveSeconds
                // (เดิมเป็นค่าคงที่ 60 วิในโค้ด แก้ไม่ได้เลยถ้าไม่ build ใหม่)
                int autoSaveSeconds = ServerConfig.Current.Save?.AutoSaveSeconds ?? AutoSaveIntervalSeconds;
                if (autoSaveSeconds > 0 && now - lastSaveAt >= autoSaveSeconds * 1000.0)
                {
                    lastSaveAt = now;
                    SafeProcess("autosave", () =>
                    {
                        int n = world.SaveAll();
                        if (n > 0)
                        {
                            Console.WriteLine($"[save] autosave {n} ไฟล์");
                        }
                    });
                }

                // แบ็กอัพตามรอบ (ค่าเริ่มต้นทุก 4 ชม. เก็บย้อนหลัง 12 ชุด = 2 วัน)
                // เซฟก่อนเสมอ ไม่งั้นได้ภาพเก่ากว่าที่ควร
                SafeProcess("backup", () =>
                {
                    if (SaveBackup.Due(now))
                    {
                        world.SaveAll(force: true);
                        SaveBackup.Tick(now);
                    }
                });

                if (StatsIntervalSeconds > 0 && now - lastReportAt >= StatsIntervalSeconds * 1000.0)
                {
                    double tps = ticksSinceReport * 1000.0 / (now - lastReportAt);
                    int alive = world.Animals.AliveCount;
                    int corpses = world.Animals.Count - alive;
                    Console.WriteLine($"[loop] {tps:F0} tps, ผู้เล่นออนไลน์ {world.Count}, สัตว์ {alive} ตัว{(corpses > 0 ? $" (+ซาก {corpses})" : "")}, RAM {GC.GetTotalMemory(false) / 1048576} MB");
                    ticksSinceReport = 0;
                    lastReportAt = now;
                }

                // admin web panel: อัปเดต ServerStats ถี่กว่า console print (ดู ServerStats.cs, Gateway /admin/status)
                if (now - lastPanelUpdateAt >= PanelStatsIntervalSeconds * 1000.0)
                {
                    double tps = ticksSincePanelUpdate * 1000.0 / Math.Max(1.0, now - lastPanelUpdateAt);
                    int alive = world.Animals.AliveCount;
                    int corpses = world.Animals.Count - alive;
                    ServerStats.Update(tps, world.Count, alive, corpses);
                    ticksSincePanelUpdate = 0;
                    lastPanelUpdateAt = now;
                }
            }
        }
        finally
        {
            try { PluginManager.Instance?.DisableAll(); } catch (Exception e) { Console.WriteLine($"[shutdown] mod disable failed: {e.Message}"); }
            try { PluginManager.Instance?.FlushStorage(); } catch (Exception e) { Console.WriteLine($"[shutdown] mod storage flush failed: {e.Message}"); }
            try { gateway.Close(); } catch (Exception e) { Console.WriteLine($"[shutdown] gateway ปิดไม่สำเร็จ: {e.Message}"); }
            try { if (enableRadiotower) radiotower.Close(); } catch (Exception e) { Console.WriteLine($"[shutdown] radiotower ปิดไม่สำเร็จ: {e.Message}"); }
            try { gameServer.Close(); } catch (Exception e) { Console.WriteLine($"[shutdown] gameserver ปิดไม่สำเร็จ: {e.Message}"); }
            if (hiResTimer)
            {
                TimeEndPeriod(1);
            }
        }
    }
}
