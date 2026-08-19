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
        string dataDir = "data";
        string terrainId = "ri35te";
        string serverName = "Multi Play Server";
        int gamePort = GameServer.DefaultPort;
        int gatewayPort = Gateway.DefaultPort;
        string assetBundleDir = null;
        bool insecureAuth = false;
        bool enableRadiotower = false;      // GP-12
        string publicHost = null;
        string islandId = null;         // Beta 1.1: โหมดหลายเกาะ
        bool recipeCheck = false;       // --recipe-check: ตรวจข้อมูลคราฟต์/ทำอาหารแล้วออก

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--data":
                    dataDir = args[++i];
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
                    // M-5: เปิดพอร์ตแชทส่วนตัว 8192 (default = ปิด เพราะไม่มี auth ปลอมชื่อได้)
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
                case "--public-host":
                    // ที่อยู่ที่ client ใช้ต่อ TCP (พอร์ตเกม/แชท) — เช่น 127.0.0.1 เมื่อเล่น
                    // ผ่าน Cloudflare Tunnel (client ต่อผ่าน cloudflared access tcp บนเครื่องตัวเอง)
                    // ไม่ระบุ = ใช้ host ที่ client เรียก gateway มา (เล่นในวงแลน)
                    publicHost = args[++i];
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

        if (recipeCheck)
        {
            // ตรวจข้อมูลอย่างเดียว ไม่ต้องโหลด terrain / เปิดพอร์ต
            Environment.ExitCode = RecipeCheck.Run();
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
        GameServer gameServer = new GameServer(world);
        gameServer.RequireSessionToken = !insecureAuth;      // GP-12
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
        // Radiotower = server แชทแยก (พอร์ต 8192, client ต่อตาม radiotower_addresses ใน /entry)
        //
        // M-5: พอร์ตนี้ **ไม่มี auth เลย** — ใครต่อเข้ามาก็ประกาศตัวเป็นใครก็ได้แล้วพูดแทนคนนั้น
        // beta 1.0 จึงปิดไว้เป็นค่าเริ่มต้น (แชทช่องรวมวิ่งบน connection เกมที่ Auth แล้ว ไม่ได้ใช้พอร์ตนี้)
        // เปิดด้วย --radiotower ถ้าจะกลับมาทำแชทส่วนตัว
        RadiotowerServer radiotower = new RadiotowerServer();
        if (enableRadiotower)
        {
            if (!radiotower.Start(RadiotowerServer.DefaultPort))
            {
                Console.WriteLine($"[warn] เปิดพอร์ต radiotower {RadiotowerServer.DefaultPort} ไม่ได้ — แชทส่วนตัวจะใช้ไม่ได้ แต่เล่นต่อได้");
            }
        }
        else
        {
            Console.WriteLine("[radiotower] ปิดอยู่ (M-5: ไม่มี auth) — เปิดด้วย --radiotower");
        }

        Gateway gateway;
        try
        {
            gateway = new Gateway(gameServer, world, gatewayPort, assetBundleDir, radiotower.Port, publicHost);
            Console.WriteLine($"[gateway] listening on {gateway.BindPrefix} (UDP knock: {gatewayPort + 1})");
        }
        catch (Exception e)
        {
            Console.WriteLine("[fatal] gateway start failed: " + e.Message);
            Console.WriteLine("  hint: run as Administrator or free the port first");
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
            int n = world.SaveAll(force: true);
            Console.WriteLine($"[save] เขียนไป {n} ไฟล์ ปิดเรียบร้อย");
            Environment.Exit(0);
        };

        Console.WriteLine("server running. Ctrl+C to stop.");

        double tickMs = 1000.0 / TargetTps;
        Stopwatch clock = Stopwatch.StartNew();
        double nextTickAt = 0.0;
        int ticksSinceReport = 0;
        double lastReportAt = 0.0;
        double lastSaveAt = 0.0;
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
                ServerConfig.Tick(now / 1000.0);

                // เลือด/สตามินา/ความล้า — ต้องเดินต่อแม้ผู้เล่นไม่ได้ทำอะไร
                // (ล้าเต็มแล้วเลือดไหลลงจนตายได้ ต้องมีคนคอยนับให้)
                world.TickSurvival(Durango.Utils.Times.UnixTimeNow());

                if (AutoSaveIntervalSeconds > 0 && now - lastSaveAt >= AutoSaveIntervalSeconds * 1000.0)
                {
                    lastSaveAt = now;
                    int n = world.SaveAll();     // GP-07 — ข้ามถ้าไม่มีอะไรเปลี่ยน
                    if (n > 0)
                    {
                        Console.WriteLine($"[save] autosave {n} ไฟล์");
                    }
                }

                if (StatsIntervalSeconds > 0 && now - lastReportAt >= StatsIntervalSeconds * 1000.0)
                {
                    double tps = ticksSinceReport * 1000.0 / (now - lastReportAt);
                    int alive = world.Animals.AliveCount;
                    int corpses = world.Animals.Count - alive;
                    Console.WriteLine($"[loop] {tps:F0} tps, ผู้เล่นออนไลน์ {world.Count}, สัตว์ {alive} ตัว{(corpses > 0 ? $" (+ซาก {corpses})" : "")}, RAM {GC.GetTotalMemory(false) / 1048576} MB");
                    ticksSinceReport = 0;
                    lastReportAt = now;
                }
            }
        }
        finally
        {
            if (hiResTimer)
            {
                TimeEndPeriod(1);
            }
        }
    }
}
