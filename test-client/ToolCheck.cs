using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทส **ความทนทานของเครื่องมือ** (Beta 1.0 — ดู ToolDurability.cs)
///
/// วิธีวัด: เสกมีดหิน แล้วแล่ซากไปเรื่อย ๆ จนมีดพัง โดยดูตัวเลขจาก `cheat tools` ทุกช่วง
///   1. มีดใหม่ต้องเต็มหลอดตามวัสดุ (หิน = 40)
///   2. แล่ 1 ชิ้นส่วน หลอดลด 1
///   3. **ทำไม่สำเร็จต้องไม่สึก** (แล่ซากที่อยู่ไกลเกินเอื้อม)
///   4. หลอดหมด = มีดหายจากกระเป๋า + บอกผู้เล่นว่าพังแล้ว
///   5. มีดพังแล้วแล่ต่อไม่ได้ (กลับไปเป็น "ไม่มีเครื่องมือ")
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --tool-check [host] [port เกม] [port gateway]
/// </summary>
public static class ToolCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static int _aborts;
    private static int _collected;
    private static string _info = "";
    private static readonly List<string> _infos = new List<string>();

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }

    /// <summary>อ่านตัวเลข "เหลือ/เต็ม" ของเครื่องมือชิ้นแรกจากข้อความของ cheat tools</summary>
    private static (float left, float max)? ReadTool(Connection conn)
    {
        _info = "";
        conn.Send(new Cheat { _Cheat = "tools" });
        Pump(conn, 700);
        foreach (string line in _info.Split('\n'))
        {
            int dash = line.LastIndexOf('—');
            if (dash < 0) continue;
            string[] parts = line.Substring(dash + 1).Trim().Split('/');
            if (parts.Length != 2) continue;
            if (float.TryParse(parts[0], out float l) && float.TryParse(parts[1], out float m))
            {
                return (l, m);
            }
        }
        return null;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== tool durability check: {host}:{gamePort} ===");

        string id = "tool-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }

        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        var appearAnimals = new Dictionary<string, (float x, float y)>();
        var gone = new HashSet<string>();
        Touched lastTouched = default;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => { _info += (m.Text ?? "") + "\n"; _infos.Add(m.Text ?? ""); });
        conn.Recv<Touched>((m, h) => lastTouched = m);
        conn.Recv<Collected>((m, h) => { if (m.Items != null) _collected += m.Items.Length; });
        conn.Recv<ToolNeeded>((m, h) => _infos.Add("[ToolNeeded] " + m.TagNames));
        conn.Recv<AppearAnimal>((m, h) =>
        {
            float ax = 0f, ay = 0f;
            if (m.Move.Movements != null && m.Move.Movements.Length > 0)
            {
                Location[] path = m.Move.Movements[0].Path;
                if (path != null && path.Length > 0)
                {
                    ax = path[path.Length - 1].Position.x;
                    ay = path[path.Length - 1].Position.y;
                }
            }
            appearAnimals[m.EntityId ?? ""] = (ax, ay);
        });
        conn.Recv<DisappearEntity>((m, h) => gone.Add(m.EntityId ?? ""));
        conn.Recv<EntityDied>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Inventory>((m, h) => { });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<CollectibleChanged>((m, h) => { });
        conn.Recv<CollectibleDisplay>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<DisappearEntityOnTile>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<Damaged>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2000);

        conn.Send(new Cheat { _Cheat = "clearbag" });
        Pump(conn, 500);

        Console.WriteLine("รอบ 1 — มีดใหม่เต็มหลอด");
        conn.Send(new Cheat { _Cheat = "add knife" });
        Pump(conn, 600);
        (float left, float max)? t = ReadTool(conn);
        if (t == null)
        {
            Console.WriteLine("  [ตก ] อ่านความทนทานของมีดไม่ได้ — ระบบปิดอยู่หรือเปล่า (Tools.Enabled)");
            Console.WriteLine("        ข้อความล่าสุด: " + _info.Trim());
            _failed++;
            conn.Close();
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return 1;
        }
        float fullMax = t.Value.max;
        Check("มีดหินใหม่เต็มหลอด (วัสดุระดับ 1 = 40)", Math.Abs(t.Value.left - fullMax) < 0.01f && Math.Abs(fullMax - 40f) < 0.01f,
            $"{t.Value.left:F0}/{fullMax:F0}");

        Console.WriteLine("รอบ 2 — ทำไม่สำเร็จต้องไม่สึก");
        _aborts = 0; _collected = 0;
        // แล่ซากที่ไม่มีอยู่จริง — server ต้องปฏิเสธ และมีดต้องไม่สึก
        conn.Send(new Collect { EntityId = "animal_ไม่มีตัวนี้", GeneratorId = "meat", Tile = new Point2(-1, -1) });
        Pump(conn, 1200);
        (float left, float max)? t2 = ReadTool(conn);
        Check("แล่ซากที่ไม่มีอยู่จริง ไม่ผ่าน", _aborts > 0 && _collected == 0, $"abort={_aborts} collected={_collected}");
        Check("ทำไม่สำเร็จแล้วมีดไม่สึก", t2 != null && Math.Abs(t2.Value.left - fullMax) < 0.01f,
            t2 == null ? "อ่านไม่ได้" : $"{t2.Value.left:F0}/{t2.Value.max:F0}");

        Console.WriteLine("รอบ 3 — แล่ซากแล้วหลอดลดจริง");
        float before = t2?.left ?? fullMax;
        int butchered = 0;
        int corpses = 0;
        float after = before;
        bool broke = false;

        // แล่ไปเรื่อย ๆ จนมีดพัง — เสกซากใหม่เมื่อแล่หมดตัว
        for (int round = 0; round < 60 && !broke; round++)
        {
            string victim = SpawnAndKill(conn, appearAnimals, gone);
            if (victim == null)
            {
                Console.WriteLine("  [ข้าม] เสกสัตว์มาแล่ไม่ได้");
                break;
            }
            corpses++;
            // แตะซากเพื่อดูว่ามีชิ้นส่วนอะไรบ้าง
            lastTouched = default;
            conn.Send(new Touch { EntityId = victim, EntityType = 2042, Tile = new Point2(-1, -1) });
            Pump(conn, 700);
            Generator[] parts = lastTouched.Collectible.Generators;
            if (parts == null || parts.Length == 0)
            {
                continue;
            }
            for (int i = 0; i < parts.Length && !broke; i++)
            {
                _collected = 0; _aborts = 0; _infos.Clear();
                conn.Send(new Collect { EntityId = victim, GeneratorId = parts[i].Id, Tile = new Point2(-1, -1) });
                Pump(conn, 2500);
                if (_collected == 0)
                {
                    continue;                       // ไม่ได้ของ = ไม่ควรสึก ข้ามไป
                }
                butchered++;

                // ⚠️ **ห้ามดูจากข้อความว่ามีคำว่า "พังแล้ว"** — ข้อความเตือนตอนใกล้หมดคือ
                // "เครื่องมือใกล้พังแล้ว (เหลือ 8/40)" ซึ่งมีคำนั้นอยู่ด้วย เคยหลงคิดว่ามีดพังตั้งแต่ครั้งที่ 32
                // ตัวชี้ขาดคือ **มีดยังอยู่ในกระเป๋าไหม**
                (float left, float max)? now = ReadTool(conn);
                if (now == null)
                {
                    broke = true;
                }
                if (butchered == 1)
                {
                    after = now?.left ?? -1f;
                    Check("แล่ 1 ชิ้นส่วน หลอดลด 1", Math.Abs(before - after - 1f) < 0.01f,
                        $"{before:F0} → {after:F0}");
                }
            }
        }

        Console.WriteLine($"รอบ 4 — แล่ไปทั้งหมด {butchered} ชิ้นส่วน จาก {corpses} ซาก");
        Check("มีดพังเมื่อใช้ครบตามความทนทาน", broke, broke ? $"พังหลังใช้ {butchered} ครั้ง (ความทนทาน {fullMax:F0})" : $"ใช้ไป {butchered} ครั้งแล้วยังไม่พัง");
        if (broke)
        {
            Check("ใช้ได้ตรงตามความทนทานที่กำหนด", Math.Abs(butchered - fullMax) <= 1f,
                $"ใช้จริง {butchered} ครั้ง · กำหนดไว้ {fullMax:F0}");
            Check("ผู้เล่นได้รับแจ้งว่ามีดพัง", _infos.Exists(x => x.Contains("ต้องคราฟต์อันใหม่")),
                string.Join(" | ", _infos.FindAll(x => x.Contains("พัง"))));

            Console.WriteLine("รอบ 5 — ไม่มีมีดแล้วแล่ต่อไม่ได้");
            string victim2 = SpawnAndKill(conn, appearAnimals, gone);
            if (victim2 == null)
            {
                Console.WriteLine("  [ข้าม] เสกซากมาเทสต่อไม่ได้");
            }
            else
            {
                lastTouched = default;
                conn.Send(new Touch { EntityId = victim2, EntityType = 2042, Tile = new Point2(-1, -1) });
                Pump(conn, 700);
                Generator[] parts2 = lastTouched.Collectible.Generators;
                _aborts = 0; _collected = 0; _infos.Clear();
                conn.Send(new Collect
                {
                    EntityId = victim2,
                    GeneratorId = parts2 != null && parts2.Length > 0 ? parts2[0].Id : "meat",
                    Tile = new Point2(-1, -1)
                });
                Pump(conn, 2000);
                Check("มีดพังแล้วแล่ซากไม่ได้อีก", _collected == 0,
                    $"collected={_collected} · {(_infos.Exists(x => x.StartsWith("[ToolNeeded]")) ? "server บอกว่าต้องใช้เครื่องมือ" : "abort=" + _aborts)}");
            }
        }

        conn.Close();
        Console.WriteLine();
        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>เสกกิ้งก่าข้างตัวแล้วฆ่าทิ้ง คืน entity id ของซาก (null = ไม่สำเร็จ)</summary>
    private static string SpawnAndKill(Connection conn, Dictionary<string, (float x, float y)> appearAnimals, HashSet<string> gone)
    {
        appearAnimals.Clear();
        conn.Send(new Cheat { _Cheat = "spawn 2042" });   // กิ้งก่า — ชิ้นส่วนน้อย เทสเร็ว
        Pump(conn, 1200);
        string victim = null;
        foreach (KeyValuePair<string, (float x, float y)> kv in appearAnimals)
        {
            if (gone.Contains(kv.Key)) continue;
            victim = kv.Key;
        }
        if (victim == null)
        {
            return null;
        }
        conn.Send(new Cheat { _Cheat = "kill animal" });
        Pump(conn, 1500);
        return victim;
    }
}
