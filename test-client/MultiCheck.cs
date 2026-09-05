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
/// เทส "หลายคนออนพร้อมกัน" ตามเกณฑ์เปิด beta 1.0 ข้อ 4
/// — ต่อ 3 client พร้อมกัน แล้วให้ทั้งสามแย่งเก็บของจากจุดเดียวกัน
///   เพื่อดูว่าของ **ไม่ถูกปั๊ม** (จำนวนที่ได้รวมกันต้องไม่เกินที่จุดนั้นมีจริง)
/// — เช็คว่าเห็นกันและกัน และรู้พร้อมกันเมื่อของหมด
///
/// รัน: dotnet run -- --multi-check [host] [port เกม] [port gateway]
/// </summary>
public static class MultiCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private sealed class Client
    {
        public string Id;
        public string Token;
        public Socket Sock;
        public Connection Conn;
        public bool Welcomed;
        public int Aborts;
        public int CollectedItems;
        public int CollectSuccesses;
        public int Disappears;
        public readonly HashSet<string> SawPlayers = new HashSet<string>();
        public readonly Dictionary<(int x, int y), ushort> Naturals = new Dictionary<(int x, int y), ushort>();
        public readonly Dictionary<string, Generator[]> Touched = new Dictionary<string, Generator[]>();
    }

    /// <summary>เดิน pump ให้ทุก client พร้อมกัน (ไม่ให้ใครค้างคิวอยู่คนเดียว)</summary>
    private static void PumpAll(List<Client> cs, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            for (int c = 0; c < cs.Count; c++) cs[c].Conn.Process();
            Thread.Sleep(10);
        }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== multi check: {host}:{gamePort} — 3 คนออนพร้อมกัน ===");

        var clients = new List<Client>();
        for (int i = 1; i <= 3; i++)
        {
            string id = $"multi-{i}";
            string token = SessionClient.Fetch(host, gatewayPort, id, id);
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"ขอ token ให้ {id} ไม่ได้ — ใส่ชื่อนี้ใน data/whitelist.txt หรือยัง");
                return 1;
            }

            var c = new Client { Id = id, Token = token };
            c.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            c.Sock.Connect(host, gamePort);
            c.Conn = new Connection(c.Sock);

            c.Conn.Recv<Welcome>((m, h) => c.Welcomed = true);
            c.Conn.Recv<Clock>((m, h) => { });
            c.Conn.Recv<OK>((m, h) => { });
            c.Conn.Recv<Abort>((m, h) => c.Aborts++);
            c.Conn.Recv<Messages.Timer>((m, h) => { });
            c.Conn.Recv<Touched>((m, h) =>
            {
                if (m.Collectible.Generators != null) c.Touched[m.EntityId ?? ""] = m.Collectible.Generators;
            });
            c.Conn.Recv<Collected>((m, h) =>
            {
                // 1 Collected = จอง generator ได้ 1 หน่วย (นี่คือตัวที่ห้ามเกินจำนวนที่มีจริง)
                // แต่ Items อาจมี 2 ชิ้นได้ถ้าสกิลหมวดเก็บของสุ่มโบนัสติด — โบนัสไม่กินหน่วย
                if (m.Result == Shared.Item.Result.Success) c.CollectSuccesses++;
                if (m.Items != null) c.CollectedItems += m.Items.Length;
            });
            c.Conn.Recv<CollectibleChanged>((m, h) => { });
            c.Conn.Recv<DisappearEntityOnTile>((m, h) =>
            {
                c.Disappears++;
                c.Naturals.Remove((m.Tile.x, m.Tile.y));
            });
            c.Conn.Recv<AppearPlayer>((m, h) => { if (m.EntityId != c.Id) c.SawPlayers.Add(m.EntityId ?? ""); });
            c.Conn.Recv<DisappearEntity>((m, h) => { });
            c.Conn.Recv<Move>((m, h) => { });
            c.Conn.Recv<Inventory>((m, h) => { });
            c.Conn.Recv<InventoryUpdated>((m, h) => { });
            c.Conn.Recv<Survival>((m, h) => { });
            c.Conn.Recv<SurvivalUpdated>((m, h) => { });
            c.Conn.Recv<AppearAnimal>((m, h) => { });
            c.Conn.Recv<AppearArtifact>((m, h) => { });
            c.Conn.Recv<Equipments>((m, h) => { });
            c.Conn.Recv<Skills>((m, h) => { });
            c.Conn.Recv<Statistics>((m, h) => { });
            c.Conn.Recv<DefoggedChunks>((m, h) => { });
            c.Conn.Recv<QuestCategories>((m, h) => { });
            c.Conn.Recv<WalletUpdated>((m, h) => { });
            c.Conn.Recv<Info>((m, h) => { });
            c.Conn.Recv<EntityDied>((m, h) => { });
            c.Conn.Recv<Chunk>((m, h) =>
            {
                byte[] g = m.Garden;
                if (g == null) return;
                for (int k = 0; k + 6 <= g.Length; k += 6)
                {
                    c.Naturals[(BitConverter.ToUInt16(g, k), BitConverter.ToUInt16(g, k + 2))] = BitConverter.ToUInt16(g, k + 4);
                }
            });
            c.Conn.StartReceive();

            c.Conn.Send(new GetClock { Time = Times.UnixTimeNow() });
            clients.Add(c);
        }

        PumpAll(clients, 400);
        foreach (Client c in clients)
        {
            c.Conn.Send(new Auth { EntityId = c.Id, SessionToken = c.Token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        }
        PumpAll(clients, 800);
        foreach (Client c in clients) c.Conn.Send(default(Ready));
        PumpAll(clients, 2000);

        Console.WriteLine("ต่อพร้อมกัน 3 เส้น");
        Check("ทุกคนเข้าเกมได้พร้อมกัน", clients.TrueForAll(c => c.Welcomed),
            string.Join(" ", clients.ConvertAll(c => $"{c.Id}={c.Welcomed}")));
        Check("ไม่มีใครโดนเตะระหว่างเข้า", clients.TrueForAll(c => c.Conn.Connected()));

        // ยืนที่เดียวกันหมด แล้วขอ chunk เดียวกัน
        float px = 8000f, py = 35400f;
        foreach (Client c in clients)
        {
            MoveTo(c.Conn, c.Id, px, py);
            c.Conn.Send(new SetChunk { Chunk = new Point2(2, 11) });
        }
        PumpAll(clients, 1500);

        Check("เห็นกันและกันในแมพ", clients.TrueForAll(c => c.SawPlayers.Count >= 2),
            string.Join(" ", clients.ConvertAll(c => $"{c.Id} เห็น {c.SawPlayers.Count}")));

        // หาของธรรมชาติที่ใกล้ที่สุดใน chunk นี้ แล้วเดินไปยืนข้าง ๆ กันทั้งสามคน
        int cx = (int)(px / 200f), cy = (int)(py / 200f);
        (int x, int y) target = default;
        ushort targetType = 0;
        bool found = false;
        int best = int.MaxValue;
        foreach (var kv in clients[0].Naturals)
        {
            int dx = kv.Key.x - cx, dy = kv.Key.y - cy;
            int d2 = dx * dx + dy * dy;
            if (d2 < best) { best = d2; target = kv.Key; targetType = kv.Value; found = true; }
        }
        if (!found)
        {
            Console.WriteLine("  [ข้าม] chunk นี้ไม่มีของธรรมชาติเลย เทสแย่งเก็บของไม่ได้");
        }
        else
        {
            Console.WriteLine($"เดินไปที่ของธรรมชาติ tile {target.x},{target.y} (ห่างจุดเกิด {Math.Sqrt(best):F1} tile)");
            float tx = target.x * 200f + 100f, ty = target.y * 200f + 100f;
            foreach (Client c in clients) WalkTo(clients, c, ref px, ref py, tx, ty);
            PumpAll(clients, 800);

            Console.WriteLine($"แย่งเก็บของจุดเดียวกัน — tile {target.x},{target.y} ชนิด {targetType}");
            string naturalId = $"natural_{target.x}_{target.y}";
            foreach (Client c in clients)
            {
                c.Conn.Send(new Touch { EntityId = naturalId, EntityType = targetType, Tile = new Point2(target.x, target.y) });
            }
            PumpAll(clients, 1200);

            Check("ทุกคนแตะจุดเดียวกันได้", clients.TrueForAll(c => c.Touched.ContainsKey(naturalId)),
                string.Join(" ", clients.ConvertAll(c => $"{c.Id}={c.Touched.ContainsKey(naturalId)}")));

            Generator[] gens = clients[0].Touched.TryGetValue(naturalId, out Generator[] g0) ? g0 : null;
            if (gens == null || gens.Length == 0)
            {
                Check("จุดเก็บของมี generator", false, "ไม่ได้ generator กลับมาเลย");
            }
            else
            {
                // ทุกคนต้องเห็นจำนวนที่เหลือชุดเดียวกัน (GP-03: state อยู่ที่ world)
                bool sameView = true;
                foreach (Client c in clients)
                {
                    if (!c.Touched.TryGetValue(naturalId, out Generator[] gc) || gc.Length != gens.Length) { sameView = false; break; }
                    for (int i = 0; i < gc.Length; i++)
                    {
                        if (gc[i].Id != gens[i].Id || gc[i].Amount != gens[i].Amount) { sameView = false; break; }
                    }
                }
                Check("ทุกคนเห็นจำนวนที่เหลือชุดเดียวกัน", sameView);

                Generator gen = gens[0];
                int available = gen.Amount;
                Console.WriteLine($"  generator {gen.Id} มี {available} หน่วย — สั่งเก็บรวม {clients.Count * (available + 2)} ครั้ง");

                // ทุกคนรัวเก็บพร้อมกันเกินจำนวนที่มี
                for (int round = 0; round < available + 2; round++)
                {
                    foreach (Client c in clients)
                    {
                        c.Conn.Send(new Collect { EntityId = naturalId, GeneratorId = gen.Id, ToolItemId = null });
                    }
                    PumpAll(clients, 400);
                }
                PumpAll(clients, 5000);

                int total = 0;
                int units = 0;
                foreach (Client c in clients) { total += c.CollectedItems; units += c.CollectSuccesses; }
                // เทียบ "จำนวนครั้งที่เก็บสำเร็จ" กับหน่วยที่มี ไม่ใช่จำนวนชิ้นที่ได้ —
                // สกิลหมวดเก็บของมีโอกาสสุ่มแถมของเพิ่ม 1 ชิ้นโดยไม่กินหน่วย (RollGatherBonus)
                // ถ้าวัดจากจำนวนชิ้น เทสจะตกเองเวลาโบนัสติด ทั้งที่ของไม่ได้ถูกปั๊ม
                Check("ของไม่ถูกปั๊ม (เก็บสำเร็จไม่เกินหน่วยที่มีจริง)", units <= available,
                    $"มี {available} หน่วย แต่เก็บสำเร็จ {units} ครั้ง ({string.Join(" ", clients.ConvertAll(c => $"{c.Id}={c.CollectSuccesses}"))})");
                if (total > units)
                {
                    Console.WriteLine($"  (ได้ของรวม {total} ชิ้นจาก {units} หน่วย — โบนัสสกิลติด {total - units} ครั้ง)");
                }
                Check("มีคนเก็บได้จริงอย่างน้อย 1 ชิ้น", total >= 1,
                    string.Join(" ", clients.ConvertAll(c => $"{c.Id}={c.CollectedItems}")));
                Check("คนที่แย่งไม่ทันโดนปฏิเสธ", clients.Exists(c => c.Aborts > 0),
                    string.Join(" ", clients.ConvertAll(c => $"{c.Id} abort={c.Aborts}")));
            }
        }

        // ทุกคนต้องยังอยู่ครบหลังเทส (ไม่มี exception ฝั่ง server ทำให้หลุด)
        PumpAll(clients, 500);
        Check("ยังต่ออยู่ครบทั้ง 3 คนหลังเทส", clients.TrueForAll(c => c.Conn.Connected()));

        foreach (Client c in clients) c.Conn.Close();

        Console.WriteLine();
        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>เดินทีละก้าว (M-2 ที่ server กันวาร์ป) โดย pump ให้ทุกคนไปพร้อมกัน</summary>
    private static void WalkTo(List<Client> all, Client c, ref float px, ref float py, float x, float y)
    {
        float sx = px, sy = py;
        for (int guard = 0; guard < 20; guard++)
        {
            float dx = x - sx, dy = y - sy;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1f) break;
            float step = Math.Min(MaxStepUnits, dist);
            sx += dx / dist * step;
            sy += dy / dist * step;
            MoveTo(c.Conn, c.Id, sx, sy);
            PumpAll(all, 1000);
            if (dist <= MaxStepUnits) break;
        }
    }

    /// <summary>M-2: server ยอมให้ขยับได้ 900 หน่วย/วิ + เผื่อ 300 — ก้าวละ 900 ต่อ 1 วินาทีจึงอยู่ในเกณฑ์</summary>
    private const float MaxStepUnits = 900f;

    private static void MoveTo(Connection conn, string entityId, float x, float y)
    {
        conn.Send(new Move
        {
            EntityId = entityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = "Barehand_Walk",
                    MotionOption = 5,
                    PlaybackRate = 1f,
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        new Location { Position = new WorldPosition(x, y), Yaw = 0f, Time = Times.UnixTimeNow(), Floor = 0, Height = 0f }
                    }
                }
            }
        });
    }
}
