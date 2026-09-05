using System;
using System.Collections.Generic;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// AI farm bot — ต่อเข้า server แล้วเก็บของ/เดินวนเองไม่ต้องมีคนกด
///
/// มีไว้เพื่อ **ทดสอบ** โดยเฉพาะ: ไม่มี UI จึงไม่ติดบทสนทนา NPC / prologue
/// ที่บล็อกการเดินในเกมจริง (client ตัดสินจากซีน Unity ซึ่ง server สั่งไม่ได้)
///
/// รัน: dotnet run -- --bot [host] [port] [นาที]
/// ดูรายละเอียดที่ docs/server/FarmBot.md
/// </summary>
public static class FarmBot
{
    /// <summary>ความเร็วเดินของ bot (หน่วย/วินาที) — ต่ำกว่าเพดาน M-2 ของ server (900) และเท่าค่า default ของตัวเกม</summary>
    private const double BotMoveSpeed = 450.0;

    // ชนิด natural ที่เจอจริงตอนเทสกับเกม (>= 10000 = ของธรรมชาติ)
    private static readonly ushort[] NaturalTypes = { 12119, 12121, 10001, 11001 };

    private sealed class Stats
    {
        public int Touch, Collect, Collected, Abort, Move, Craft, Equip, Chat, Dump;
        public readonly Dictionary<string, int> Items = new Dictionary<string, int>();
        public readonly List<string> Errors = new List<string>();
    }

    public static void Run(string host, int port, double minutes, string entityId, string name, int gatewayPort = 0)
    {
        Console.WriteLine($"=== FarmBot: {name} -> {host}:{port} เป็นเวลา {minutes} นาที ===");
        // GP-12: ขอ session token ก่อน ไม่งั้น server ปฏิเสธตั้งแต่ Auth
        string sessionToken = SessionClient.Fetch(
            host,
            gatewayPort > 0 ? gatewayPort : port - 1,
            entityId,
            name);
        var stats = new Stats();
        var rng = new Random();

        using var sock = new System.Net.Sockets.Socket(
            System.Net.Sockets.AddressFamily.InterNetwork,
            System.Net.Sockets.SocketType.Stream,
            System.Net.Sockets.ProtocolType.Tcp);
        sock.Connect(host, port);
        var conn = new Connection(sock);

        // ---- สถานะที่ bot ต้องรู้ ----
        string pendingNaturalId = null;
        string pendingGeneratorId = null;
        Point2 pendingTile = default;
        bool busy = false;              // กำลังรอ Timer/Collected อยู่
        double busyUntil = 0;
        float stamina = -1f;
        var inventory = new List<Item>();
        // GP-09: server ไม่ยอมให้แตะ tile ที่ไม่มีของธรรมชาติจริงแล้ว
        // bot จึงต้องอ่านตำแหน่งของจริงจาก garden ที่มากับ packet Chunk (x,y,ชนิด อย่างละ 2 ไบต์)
        var naturals = new Dictionary<(int x, int y), ushort>();
        var badTiles = new HashSet<(int x, int y)>();

        conn.Recv<Welcome>((m, h) => Console.WriteLine($"[bot] Welcome: region={m.Region.Name}"));
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) =>
        {
            stats.Abort++;
            busy = false;
            // จุดที่ server ปฏิเสธ ไม่ต้องกลับไปลองอีก (ของหมดแล้ว/ไกลเกินไป/สตามินาไม่พอ)
            badTiles.Add((pendingTile.x, pendingTile.y));
            pendingNaturalId = null;
            pendingGeneratorId = null;
        });

        conn.Recv<Touched>((m, h) =>
        {
            Generator[] gens = m.Collectible.Generators;
            if (gens != null && gens.Length > 0)
            {
                pendingNaturalId = m.EntityId;
                pendingGeneratorId = gens[0].Id;
            }
            else
            {
                pendingNaturalId = null;
            }
        });

        conn.Recv<Messages.Timer>((m, h) => { busy = true; busyUntil = Times.UnixTimeNow() + m.Duration + 0.4; });

        conn.Recv<Collected>((m, h) =>
        {
            stats.Collected++;
            busy = false;
            if (m.Items != null)
            {
                foreach (Item it in m.Items)
                {
                    string k = it.Name ?? it.Prototype ?? "?";
                    stats.Items[k] = stats.Items.GetValueOrDefault(k) + 1;
                }
            }
            if (m.RanOut)
            {
                pendingNaturalId = null;   // จุดนี้หมดแล้ว ไปหาจุดใหม่
            }
        });

        conn.Recv<Inventory>((m, h) =>
        {
            inventory.Clear();
            if (m.InventoryItems.Items != null) inventory.AddRange(m.InventoryItems.Items);
        });

        conn.Recv<SurvivalUpdated>((m, h) =>
        {
            if (m.Updated != null && m.Updated.TryGetValue("stamina", out Gauge g))
            {
                stamina = g.Get(Times.UnixTimeNow());
            }
        });
        conn.Recv<Survival>((m, h) =>
        {
            if (m.Gauges != null && m.Gauges.TryGetValue("stamina", out Gauge g))
            {
                stamina = g.Get(Times.UnixTimeNow());
            }
        });

        conn.Recv<AppearAnimal>((m, h) => { });
        conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Chunk>((m, h) =>
        {
            byte[] g = m.Garden;
            if (g == null) return;
            for (int i = 0; i + 6 <= g.Length; i += 6)
            {
                naturals[(BitConverter.ToUInt16(g, i), BitConverter.ToUInt16(g, i + 2))] = BitConverter.ToUInt16(g, i + 4);
            }
        });
        conn.Recv<DisappearEntityOnTile>((m, h) => naturals.Remove((m.Tile.x, m.Tile.y)));
        conn.Recv<Info>((m, h) => Console.WriteLine($"[bot] Info: {m.Text}"));
        // M-2: ถ้า server ดึงกลับ ต้องยอมรับตำแหน่งของ server ไม่งั้นจะเดินหลุดต่อไปเรื่อย ๆ
        bool yanked = false;
        float yankX = 0f, yankY = 0f;
        conn.Recv<Teleported>((m, h) =>
        {
            yanked = true;
            yankX = m.Tile.x * 200f + 100f;
            yankY = m.Tile.y * 200f + 100f;
        });

        conn.StartReceive();

        // ---- handshake ----
        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = entityId, SessionToken = sessionToken });
        Pump(conn, 800);
        conn.Send(default(Ready));
        Pump(conn, 1500);

        // ตำแหน่งเริ่มต้นแถวจุดเกิด (tile 40,177 -> world x200)
        float px = 8000f, py = 35400f;
        Point2 lastChunk = new Point2((int)(px / 200 / 16), (int)(py / 200 / 16));
        conn.Send(new SetChunk { Chunk = lastChunk });
        Pump(conn, 600);

        double lastMoveAt = 0;
        double endAt = Times.UnixTimeNow() + minutes * 60.0;
        double nextReport = Times.UnixTimeNow() + 15.0;
        double nextAction = 0;

        while (Times.UnixTimeNow() < endAt)
        {
            conn.Process();
            double now = Times.UnixTimeNow();

            if (busy && now < busyUntil) { Thread.Sleep(20); continue; }
            busy = false;

            if (now < nextAction) { Thread.Sleep(20); continue; }

            // กระเป๋าเต็ม = เก็บอะไรไม่ได้อีก ทิ้งของทิ้งครึ่งกระเป๋าแล้วเก็บต่อ
            // (เจอตอนโซกเทส 30 นาที: bot ตัน 50 ช่องแล้ววนแตะ-โดนปฏิเสธไปจนจบ)
            if (inventory.Count >= 50)
            {
                var dump = new List<string>();
                lock (inventory)
                {
                    for (int i = 0; i < inventory.Count && dump.Count < 25; i++) dump.Add(inventory[i].Id);
                }
                conn.Send(new DumpItems { ItemIds = dump.ToArray() });
                stats.Dump += dump.Count;
                nextAction = now + 0.6;
                continue;
            }

            if (pendingNaturalId != null && pendingGeneratorId != null)
            {
                // มีจุดเก็บของค้างอยู่ -> เก็บต่อ
                conn.Send(new Collect
                {
                    EntityId = pendingNaturalId,
                    GeneratorId = pendingGeneratorId,
                    Tile = pendingTile
                });
                stats.Collect++;
                nextAction = now + 0.4;
            }
            else
            {
                // GP-09: เลือกของธรรมชาติจริงที่ใกล้ที่สุดจากที่ server บอกมา ถ้ายังไม่รู้จักที่ไหนเลยก็เดินสุ่มหา
                (int x, int y) target = default;
                bool haveTarget = false;
                double bestDist = double.MaxValue;
                foreach (var kv in naturals)
                {
                    if (badTiles.Contains(kv.Key)) continue;
                    double dx = kv.Key.x - px / 200.0;
                    double dy = kv.Key.y - py / 200.0;
                    double d = dx * dx + dy * dy;
                    if (d < bestDist) { bestDist = d; target = kv.Key; haveTarget = true; }
                }

                if (yanked)
                {
                    // server ดึงกลับ = ตำแหน่งที่ bot คิดไว้ผิด เอาของ server เป็นหลัก
                    px = yankX; py = yankY; yanked = false;
                }

                // M-2: server จำกัดความเร็วไว้เท่าตัวเกมจริง (client default 500 หน่วย/วิ)
                // เดิม bot วาร์ปไปยืนบนเป้าหมายทันที → โดนปฏิเสธทุกก้าวแล้ววนแตะ tile ไกล ๆ ไม่จบ
                float destX, destY;
                if (haveTarget)
                {
                    destX = target.x * 200f;
                    destY = target.y * 200f;
                }
                else
                {
                    destX = px + (float)(rng.NextDouble() - 0.5) * 800f;
                    destY = py + (float)(rng.NextDouble() - 0.5) * 800f;
                }
                float mdx = destX - px, mdy = destY - py;
                float mdist = MathF.Sqrt(mdx * mdx + mdy * mdy);
                float stepMax = (float)(BotMoveSpeed * (now - lastMoveAt));
                if (lastMoveAt <= 0) stepMax = (float)(BotMoveSpeed * 0.5);
                if (mdist > stepMax && mdist > 0.001f)
                {
                    px += mdx / mdist * stepMax;
                    py += mdy / mdist * stepMax;
                    haveTarget = false;          // ยังไปไม่ถึง อย่าเพิ่งแตะ
                }
                else
                {
                    px = destX;
                    py = destY;
                }
                lastMoveAt = now;
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
                                new Location { Position = new WorldPosition(px, py), Yaw = 0f, Time = now, Floor = 0, Height = 0f }
                            }
                        }
                    }
                });
                stats.Move++;

                // ขอ chunk ที่ยืนอยู่ทุกครั้งที่ข้ามเขต จะได้รู้จักของธรรมชาติแถวนั้นเพิ่ม
                var chunk = new Point2((int)(px / 200 / 16), (int)(py / 200 / 16));
                if (chunk.x != lastChunk.x || chunk.y != lastChunk.y)
                {
                    lastChunk = chunk;
                    conn.Send(new SetChunk { Chunk = chunk });
                }

                if (haveTarget)
                {
                    pendingTile = new Point2(target.x, target.y);
                    conn.Send(new Touch
                    {
                        EntityId = $"natural_{target.x}_{target.y}",
                        EntityType = naturals[target],
                        Tile = pendingTile
                    });
                    stats.Touch++;
                }
                nextAction = now + 0.6;
            }

            if (now >= nextReport)
            {
                nextReport = now + 15.0;
                Console.WriteLine($"[bot] touch={stats.Touch} collect={stats.Collect} ได้ของ={stats.Collected} abort={stats.Abort} เดิน={stats.Move} ทิ้ง={stats.Dump} สตามินา={stamina:F0} กระเป๋า={inventory.Count}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== สรุปผล FarmBot ===");
        Console.WriteLine($"  แตะจุดเก็บของ : {stats.Touch}");
        Console.WriteLine($"  สั่งเก็บ      : {stats.Collect}");
        Console.WriteLine($"  ได้ของจริง    : {stats.Collected}");
        Console.WriteLine($"  โดนปฏิเสธ     : {stats.Abort}");
        Console.WriteLine($"  เดิน          : {stats.Move}");
        Console.WriteLine($"  ของในกระเป๋า  : {inventory.Count} ชิ้น");
        if (stats.Items.Count > 0)
        {
            Console.WriteLine("  ของที่เก็บได้ :");
            foreach (KeyValuePair<string, int> kv in stats.Items)
            {
                Console.WriteLine($"      {kv.Key} x{kv.Value}");
            }
        }
        conn.Close();
    }

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { conn.Process(); Thread.Sleep(10); }
    }
}
