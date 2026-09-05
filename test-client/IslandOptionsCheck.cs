using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทส **หน้าเลือกเกาะของ UI เกมเอง** (native GenericSelector) ตั้งแต่ต้นจนจบ
///
/// จำลองสิ่งที่ client ทำจริงตอนกดสมอ ⚓ ที่ท่าเรือ:
///   1. WarpToPort            → เซิร์ฟวาร์ปผู้เล่นไปยืนที่ dock
///   2. GetIslandTravelOptions → เซิร์ฟตอบ IslandTravelOptions (Ids/Names/RequiredLevels)
///   3. เช็คว่ารายการมีเกาะปลายทางจริง (ไม่รวมเกาะปัจจุบัน) และกรองตามเลเวล
///   4. TravelByRegion เกาะแรกที่ได้ → ต้องได้ Info "##goto <addr>" + Emigrated
///
/// รัน: dotnet run -- --island-options-check [host] [gamePort] [gatewayPort]
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --island ... --enable-cheat และ Features.IslandTravel=true
/// </summary>
public static class IslandOptionsCheck
{
    private static int _passed, _failed;
    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { conn.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== island options (native UI) check: {host}:{gamePort} ===");
        string id = "isleopt-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม"); return 1; }
        if (!string.IsNullOrEmpty(SessionClient.LastUserId)) id = SessionClient.LastUserId;

        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        int aborts = 0, emigrated = 0;
        var infos = new List<string>();
        IslandTravelOptions? opts = null;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => { lock (infos) infos.Add(m.Text ?? ""); });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Actions>((m, h) => { });
        conn.Recv<Inventory>((m, h) => { });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Recipes>((m, h) => { });
        conn.Recv<ArtifactBlueprints>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearAnimal>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<Emigrated>((m, h) => emigrated++);
        conn.Recv<IslandTravelOptions>((m, h) => opts = m);
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2500);

        // ปลดเลเวลให้ถึงเกาะปลายทางที่ล็อกเลเวล (isle03 ต้องเลเวล 18)
        conn.Send(new Cheat { _Cheat = "set_level 20" });
        Pump(conn, 800);

        // 1) WarpToPort — เซิร์ฟวาร์ปไป dock
        Console.WriteLine("ขั้น 1 — WarpToPort (วาร์ปไปท่าเรือ)");
        aborts = 0;
        conn.Send(default(WarpToPort));
        Pump(conn, 1200);
        Check("WarpToPort ไม่ถูกปฏิเสธ", aborts == 0, aborts == 0 ? "ผ่าน" : $"โดน Abort {aborts}");

        // 2) GetIslandTravelOptions — ขอรายการเกาะจากท่าเรือ
        Console.WriteLine("ขั้น 2 — GetIslandTravelOptions (ขอรายการเกาะ)");
        opts = null; aborts = 0;
        conn.Send(new GetIslandTravelOptions());
        Pump(conn, 1000);
        bool gotOpts = opts.HasValue && opts.Value.Ids != null;
        Check("ได้ IslandTravelOptions กลับมา (ไม่โดน Abort)", gotOpts && aborts == 0,
            gotOpts ? $"{opts.Value.Ids.Length} ปลายทาง" : $"ไม่ได้รายการ (abort {aborts})");
        if (!gotOpts) { conn.Close(); Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ==="); return _failed == 0 ? 0 : 1; }

        var ids = opts.Value.Ids;
        var names = opts.Value.Names ?? Array.Empty<string>();
        for (int i = 0; i < ids.Length; i++)
            Console.WriteLine($"     • {ids[i]} = {(i < names.Length ? names[i] : "?")}");

        Check("รายการมีเกาะปลายทางอย่างน้อย 1 เกาะ", ids.Length >= 1, $"{ids.Length} เกาะ");
        Check("ไม่รวมเกาะปัจจุบัน (ไม่มี isle01)", !ids.Contains("isle01"),
            ids.Contains("isle01") ? "มี isle01 ทั้งที่อยู่เกาะนี้!" : "ถูกต้อง");
        Check("มีเกาะหิมะ (isle02) ในรายการ", ids.Contains("isle02"),
            ids.Contains("isle02") ? "มี" : "ไม่มี");

        if (ids.Length == 0)
        {
            Console.WriteLine("  (ไม่มีปลายทาง — ข้ามขั้น 3; ปกติถ้าผู้เล่นเลเวลไม่ถึงเกาะปลายทาง เช่นเซิร์ฟจริงไม่เปิด cheat)");
            conn.Close();
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return _failed == 0 ? 0 : 1;
        }

        // 3) TravelByRegion เกาะแรก → ##goto + Emigrated
        string dest = ids[0];
        Console.WriteLine($"ขั้น 3 — TravelByRegion → {dest}");
        infos.Clear(); emigrated = 0; aborts = 0;
        conn.Send(new TravelByRegion { RegionId = dest });
        Pump(conn, 1200);
        string joined; lock (infos) joined = string.Join(" | ", infos);
        Check("ได้คำสั่ง ##goto (handoff ไปเซิร์ฟปลายทาง)", joined.Contains("##goto"),
            joined.Contains("##goto") ? joined.Split('|').FirstOrDefault(s => s.Contains("##goto"))?.Trim() : "ไม่มี ##goto");
        Check("ได้ Emigrated (client ปิดการเชื่อมต่อเพื่อเข้าเกาะใหม่)", emigrated > 0,
            emigrated > 0 ? "ได้" : "ไม่ได้");

        conn.Close();
        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
