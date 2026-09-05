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
/// เทส **ระยะการมองเห็น** (interest management)
///
/// เดิม server ส่งทุกอย่างให้ทุกคนในเกาะโดยไม่ดูระยะ ⇒ ที่ 100 คนคือ ~20,000 packet/วินาที
/// ตอนนี้ส่งเฉพาะสิ่งที่อยู่ในระยะรอบตัว และมีรอบตรวจคอยส่ง Appear/Disappear ตอนเข้า/ออกระยะ
///
/// เช็ค:
///   1. สองคนยืนใกล้กันตอนเข้าเกม → เห็นกัน (ได้ AppearPlayer)
///   2. คนหนึ่งวาร์ปออกไปไกล → อีกคนได้ DisappearEntity
///   3. วาร์ปกลับมา → ได้ AppearPlayer อีกครั้ง (รอบตรวจทำงาน)
///   4. ตอนอยู่ไกลกัน การเดินของอีกฝ่าย **ไม่ถูกส่งมา** (นี่คือของที่ประหยัดจริง)
///   5. วาร์ปไปมุมไกล ๆ ของเกาะ → ไม่มีสัตว์อยู่ในสายตา
///   6. เสกสัตว์ตรงหน้า → เห็นทันที ไม่ต้องรอรอบตรวจ
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --vision-check [host] [port เกม] [port gateway]
/// </summary>
public static class VisionCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    /// <summary>ตัวสังเกตการณ์ 1 ตัว — จำว่าเห็นใคร/อะไรอยู่บ้างจาก packet ที่ได้รับ</summary>
    private sealed class Watcher
    {
        public string Id;
        public Connection Conn;
        public readonly HashSet<string> VisiblePlayers = new HashSet<string>(StringComparer.Ordinal);
        public readonly HashSet<string> VisibleAnimals = new HashSet<string>(StringComparer.Ordinal);
        public int AppearPlayerCount;
        public int DisappearCount;
        /// <summary>นับ Move ของ entity อื่น (ไม่นับของตัวเอง) — ใช้วัดว่าประหยัด packet จริงไหม</summary>
        public int MovesFromOthers;

        public void Pump(int ms)
        {
            for (int i = 0; i < ms / 10; i++)
            {
                Conn.Process();
                Thread.Sleep(10);
            }
        }
    }

    private static Watcher Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        var w = new Watcher { Id = id, Conn = new Connection(sock) };
        Connection conn = w.Conn;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => { });
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Inventory>((m, h) => { });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<PlayerDisplay>((m, h) => { });
        conn.Recv<Recipes>((m, h) => { });
        conn.Recv<ArtifactBlueprints>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.Recv<Teleported>((m, h) => { });
        conn.Recv<EntityDied>((m, h) => { });
        conn.Recv<EntityRevived>((m, h) => { });

        conn.Recv<AppearPlayer>((m, h) =>
        {
            if (m.EntityId != null && m.EntityId != w.Id)
            {
                w.VisiblePlayers.Add(m.EntityId);
                w.AppearPlayerCount++;
            }
        });
        conn.Recv<AppearAnimal>((m, h) =>
        {
            if (m.EntityId != null) w.VisibleAnimals.Add(m.EntityId);
        });
        conn.Recv<DisappearEntity>((m, h) =>
        {
            if (m.EntityId == null) return;
            bool had = w.VisiblePlayers.Remove(m.EntityId) | w.VisibleAnimals.Remove(m.EntityId);
            if (had) w.DisappearCount++;
        });
        conn.Recv<Move>((m, h) =>
        {
            if (m.EntityId != null && m.EntityId != w.Id) w.MovesFromOthers++;
        });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        w.Pump(400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        w.Pump(600);
        conn.Send(default(Ready));
        w.Pump(2500);
        return w;
    }

    /// <summary>ปั๊มทั้งสองฝั่งพร้อมกัน (ถ้าปั๊มทีละตัว อีกตัวจะไม่ได้อ่าน socket เลย)</summary>
    private static void PumpBoth(Watcher a, Watcher b, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            a.Conn.Process();
            b.Conn.Process();
            Thread.Sleep(10);
        }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== vision check (ระยะการมองเห็น): {host}:{gamePort} ===");
        string idA = "vis-a-" + Guid.NewGuid().ToString("N").Substring(0, 6);
        string idB = "vis-b-" + Guid.NewGuid().ToString("N").Substring(0, 6);

        Watcher a = Connect(host, gamePort, gatewayPort, idA);
        Watcher b = a == null ? null : Connect(host, gamePort, gatewayPort, idB);
        if (a == null || b == null)
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }
        PumpBoth(a, b, 1500);

        // ── รอบ 1: ยืนใกล้กันตอนเข้าเกม ต้องเห็นกัน ───────────────────
        Console.WriteLine("รอบ 1 — เข้าเกมพร้อมกันที่จุดเกิด");
        Check("A เห็น B", a.VisiblePlayers.Contains(idB), string.Join(",", a.VisiblePlayers));
        Check("B เห็น A", b.VisiblePlayers.Contains(idA), string.Join(",", b.VisiblePlayers));
        Check("มีสัตว์อยู่ในสายตาบ้าง", a.VisibleAnimals.Count > 0, $"{a.VisibleAnimals.Count} ตัว");
        int animalsAtSpawn = a.VisibleAnimals.Count;

        // ── รอบ 2: A วาร์ปหนีไปไกล → B ต้องเห็น A หายไป ───────────────
        Console.WriteLine("รอบ 2 — A วาร์ปออกไป 120 tile (ไกลกว่าระยะหาย 32 tile มาก)");
        a.Conn.Send(new Cheat { _Cheat = "tp 20 20" });
        PumpBoth(a, b, 2500);
        Check("B ไม่เห็น A แล้ว", !b.VisiblePlayers.Contains(idA));
        Check("A ไม่เห็น B แล้ว", !a.VisiblePlayers.Contains(idB));
        Check("B ได้รับ DisappearEntity จริง", b.DisappearCount > 0, $"{b.DisappearCount} ครั้ง");

        // ── รอบ 3: ตอนอยู่ไกลกัน การเดินของอีกฝ่ายต้องไม่ถูกส่งมา ─────
        Console.WriteLine("รอบ 3 — B ขยับไปมาตอนที่ A อยู่ไกล");
        int movesBefore = a.MovesFromOthers;
        for (int i = 0; i < 4; i++)
        {
            b.Conn.Send(new Cheat { _Cheat = "tp 14" + i + "0 140" });
            PumpBoth(a, b, 500);
        }
        PumpBoth(a, b, 1200);
        Check("A ไม่ได้รับการเดินของ B เลย (ประหยัด packet ได้จริง)",
            a.MovesFromOthers == movesBefore, $"ได้เพิ่ม {a.MovesFromOthers - movesBefore} packet");

        // ── รอบ 4: A วาร์ปกลับมาหา B → ต้องเห็นกันอีกครั้ง ────────────
        Console.WriteLine("รอบ 4 — A วาร์ปกลับมาที่เดียวกับ B");
        a.Conn.Send(new Cheat { _Cheat = "tp 140 140" });
        b.Conn.Send(new Cheat { _Cheat = "tp 141 140" });
        PumpBoth(a, b, 2500);
        Check("A กลับมาเห็น B อีกครั้ง", a.VisiblePlayers.Contains(idB));
        Check("B กลับมาเห็น A อีกครั้ง", b.VisiblePlayers.Contains(idA));

        // ── รอบ 5: ที่ใหม่ต้องไม่ลากสัตว์จากจุดเกิดตามมา ──────────────
        Console.WriteLine("รอบ 5 — สัตว์ที่จุดเกิดต้องหลุดสายตาไปแล้ว");
        Check("สัตว์ในสายตาลดลงหลังย้ายที่", a.VisibleAnimals.Count < animalsAtSpawn || animalsAtSpawn == 0,
            $"จุดเกิด {animalsAtSpawn} ตัว → ที่ใหม่ {a.VisibleAnimals.Count} ตัว");

        // ── รอบ 6: เสกสัตว์ตรงหน้าต้องเห็นทันที ───────────────────────
        Console.WriteLine("รอบ 6 — เสกสัตว์ตรงหน้า A");
        int before = a.VisibleAnimals.Count;
        a.Conn.Send(new Cheat { _Cheat = "spawn" });
        PumpBoth(a, b, 1200);
        Check("เห็นสัตว์ที่เพิ่งเสกทันที", a.VisibleAnimals.Count > before,
            $"{before} → {a.VisibleAnimals.Count} ตัว");
        Check("B ที่ยืนใกล้ ๆ ก็เห็นด้วย", b.VisibleAnimals.Count > 0, $"{b.VisibleAnimals.Count} ตัว");

        a.Conn.Close();
        b.Conn.Close();
        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
