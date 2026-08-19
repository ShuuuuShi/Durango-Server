using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// บอทคอนโซล — สั่งงานด้วย "คำสั่ง" แล้วบอทแปลงเป็น packet ให้ (แนวเดียวกับ OpenKore)
/// ใช้เทส server ได้ทั้งหมดโดยไม่ต้องเปิดตัวเกม ไม่ต้องแตะเมาส์/คีย์บอร์ด
///
/// รัน:
///   dotnet run -- --console [host] [port เกม] [ชื่อ] [port gateway]     ← พิมพ์คำสั่งเอง
///   echo "look; farm 20; status" | dotnet run -- --console               ← ป้อนทาง stdin
///   dotnet run -- --console --cmd "look; attack near; status"            ← สั่งรวดเดียวแล้วออก
///   dotnet run -- --console --script test.txt                            ← อ่านจากไฟล์
///
/// โครงสร้าง: เธรดเดียวจัดการ packet ทั้งหมด (loop `Process()`)
/// เธรดอ่าน stdin แยกต่างหากแล้วส่งคำสั่งเข้า queue — handler จึงไม่ต้องกังวลเรื่อง race
/// </summary>
public static class BotConsole
{
    /// <summary>1 tile = กี่หน่วยโลก</summary>
    private const float TileSize = 200f;

    private sealed class Entity
    {
        public string Id;
        public string Kind;          // player / animal / artifact
        public string Name;
        public ushort Type;
        public int Level;
        public bool Alive = true;
        public float X, Y;
        public float Life = -1f;
    }

    private sealed class Bot
    {
        public Connection Conn;
        public string EntityId;
        public float X = 8000f, Y = 35400f;      // จุดเกิดโดยประมาณ (tile 40,177)
        public Point2 Chunk = new Point2(-1, -1);
        public readonly Dictionary<string, Entity> Entities = new Dictionary<string, Entity>();
        public readonly Dictionary<(int x, int y), ushort> Naturals = new Dictionary<(int, int), ushort>();
        public readonly List<Item> Inventory = new List<Item>();
        public readonly Dictionary<string, Gauge> Gauges = new Dictionary<string, Gauge>();
        public Gauge Life;
        public string[] BattleActions = Array.Empty<string>();
        public bool Dead;
        public bool Dump;

        // จุดเก็บของที่แตะค้างไว้
        public string PendingNaturalId;
        public string PendingGeneratorId;
        public Point2 PendingTile;

        public string Target;                    // เป้าหมายที่กำลังตี
        public double BusyUntil;                 // กำลังรอ Timer/Collected อยู่

        public float Tx => X / TileSize;
        public float Ty => Y / TileSize;
    }

    private static float Val(Gauge g)
    {
        return g == null ? -1f : g.Get(Times.UnixTimeNow());
    }

    private static Gauge GaugeOf(Bot bot, string key)
    {
        return bot.Gauges.TryGetValue(key, out Gauge g) ? g : null;
    }

    public static int Run(string host, int gamePort, int gatewayPort, string name, List<string> queued, bool interactive)
    {
        Console.WriteLine($"=== บอทคอนโซล: {name} -> {host}:{gamePort} (gateway {gatewayPort}) ===");

        string token = SessionClient.Fetch(host, gatewayPort, name, name);
        var bot = new Bot { EntityId = name };

        using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            sock.Connect(host, gamePort);
        }
        catch (Exception e)
        {
            Console.WriteLine("ต่อ server ไม่ได้: " + e.Message);
            return 1;
        }
        bot.Conn = new Connection(sock);
        RegisterHandlers(bot);
        bot.Conn.StartReceive();

        bot.Conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(bot, 300);
        bot.Conn.Send(new Auth { EntityId = name, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(bot, 600);
        bot.Conn.Send(default(Ready));
        Pump(bot, 1500);
        SendMove(bot, bot.X, bot.Y);
        SyncChunk(bot, force: true);
        Pump(bot, 800);
        bot.Conn.Send(default(GetActions));
        bot.Conn.Send(default(GetInventory));
        Pump(bot, 600);

        var commands = new ConcurrentQueue<string>();
        foreach (string c in queued)
        {
            commands.Enqueue(c);
        }

        Thread reader = null;
        if (interactive)
        {
            Console.WriteLine("พิมพ์ help เพื่อดูคำสั่ง · quit เพื่อออก");
            reader = new Thread(() =>
            {
                string line;
                while ((line = Console.ReadLine()) != null)
                {
                    foreach (string part in line.Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(part))
                        {
                            commands.Enqueue(part.Trim());
                        }
                    }
                }
                commands.Enqueue("quit");
            })
            { IsBackground = true };
            reader.Start();
        }

        double waitUntil = 0;
        bool running = true;
        while (running && bot.Conn.Connected())
        {
            bot.Conn.Process();
            double now = Times.UnixTimeNow();

            if (now < waitUntil || now < bot.BusyUntil)
            {
                Thread.Sleep(20);
                continue;
            }

            if (!commands.TryDequeue(out string cmd))
            {
                if (!interactive)
                {
                    break;                    // สั่งครบแล้วและไม่ได้รอคำสั่งจากคน
                }
                Thread.Sleep(20);
                continue;
            }

            try
            {
                running = Execute(bot, cmd, ref waitUntil);
            }
            catch (Exception e)
            {
                Console.WriteLine("คำสั่งพัง: " + e.Message);
            }
        }

        Pump(bot, 500);
        bot.Conn.Close();
        Console.WriteLine("ปิดการเชื่อมต่อแล้ว");
        return 0;
    }

    // ───────────────────────────── คำสั่ง ─────────────────────────────

    /// <summary>คืน false เมื่อสั่ง quit</summary>
    private static bool Execute(Bot bot, string line, ref double waitUntil)
    {
        string[] a = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (a.Length == 0)
        {
            return true;
        }
        string cmd = a[0].ToLowerInvariant();
        switch (cmd)
        {
            case "help":
                PrintHelp();
                break;

            case "quit":
            case "exit":
                return false;

            case "wait":
                waitUntil = Times.UnixTimeNow() + (a.Length > 1 ? ParseD(a[1]) : 1.0);
                break;

            case "status":
                PrintStatus(bot);
                break;

            case "inv":
                PrintInventory(bot);
                break;

            case "look":
                PrintSurroundings(bot);
                break;

            case "dump":
                bot.Dump = !bot.Dump;
                Console.WriteLine("dump packet: " + (bot.Dump ? "เปิด" : "ปิด"));
                break;

            case "move":
            {
                // move <tileX> <tileY>
                float tx = ParseF(a[1]);
                float ty = ParseF(a[2]);
                WalkTo(bot, tx * TileSize, ty * TileSize);
                Console.WriteLine($"เดินไป tile {tx},{ty}");
                waitUntil = Times.UnixTimeNow() + 0.3;
                break;
            }

            case "goto":
            {
                // goto <entityId> — ไปยืนข้าง ๆ เป้าหมาย
                Entity e = FindEntity(bot, a.Length > 1 ? a[1] : "near");
                if (e == null) { Console.WriteLine("ไม่เจอเป้าหมาย"); break; }
                WalkTo(bot, e.X, e.Y);
                Console.WriteLine($"เดินไปหา {Label(e)} ที่ tile {e.X / TileSize:F0},{e.Y / TileSize:F0}");
                waitUntil = Times.UnixTimeNow() + 0.3;
                break;
            }

            case "touch":
            {
                (int x, int y)? tile = a.Length >= 3
                    ? ((int)ParseF(a[1]), (int)ParseF(a[2]))
                    : NearestNatural(bot);
                if (tile == null) { Console.WriteLine("ไม่รู้จักของธรรมชาติแถวนี้เลย (ลอง look หรือ move ก่อน)"); break; }
                Touch(bot, tile.Value);
                waitUntil = Times.UnixTimeNow() + 0.6;
                break;
            }

            case "collect":
            {
                if (bot.PendingNaturalId == null) { Console.WriteLine("ยังไม่ได้แตะอะไรไว้ — สั่ง touch ก่อน"); break; }
                string gen = a.Length > 1 ? a[1] : bot.PendingGeneratorId;
                bot.Conn.Send(new Collect { EntityId = bot.PendingNaturalId, GeneratorId = gen, Tile = bot.PendingTile });
                Console.WriteLine($"เก็บ {gen} จาก {bot.PendingNaturalId}");
                bot.BusyUntil = Times.UnixTimeNow() + 2.5;
                break;
            }

            case "farm":
            {
                double secs = a.Length > 1 ? ParseD(a[1]) : 15.0;
                Farm(bot, secs);
                break;
            }

            case "target":
            {
                Entity e = FindEntity(bot, a.Length > 1 ? a[1] : "near");
                if (e == null) { Console.WriteLine("ไม่เจอเป้าหมาย"); break; }
                bot.Target = e.Id;
                bot.Conn.Send(new SelectBattleTarget { EntityId = e.Id, Tile = new Point2((int)(e.X / TileSize), (int)(e.Y / TileSize)) });
                Console.WriteLine($"เล็ง {Label(e)}");
                break;
            }

            case "attack":
            {
                Entity e = FindEntity(bot, a.Length > 1 ? a[1] : (bot.Target ?? "near"));
                if (e == null) { Console.WriteLine("ไม่เจอเป้าหมาย"); break; }
                string action = a.Length > 2 ? a[2] : DefaultAction(bot);
                bot.Target = e.Id;
                Attack(bot, e, action);
                waitUntil = Times.UnixTimeNow() + 1.0;
                break;
            }

            case "kill":
            {
                Entity e = FindEntity(bot, a.Length > 1 ? a[1] : (bot.Target ?? "near"));
                if (e == null) { Console.WriteLine("ไม่เจอเป้าหมาย"); break; }
                Kill(bot, e, a.Length > 2 ? ParseD(a[2]) : 30.0);
                break;
            }

            case "revive":
                bot.Conn.Send(new Revive { WarpholeTile = null });
                Console.WriteLine("ขอฟื้น...");
                waitUntil = Times.UnixTimeNow() + 1.5;
                break;

            case "craft":
            {
                if (a.Length < 2) { Console.WriteLine("ใช้: craft <recipeId> [slot=itemId ...]"); break; }
                var mats = new Dictionary<string, string[]>();
                for (int i = 2; i < a.Length; i++)
                {
                    int eq = a[i].IndexOf('=');
                    if (eq > 0)
                    {
                        mats[a[i].Substring(0, eq)] = new[] { a[i].Substring(eq + 1) };
                    }
                }
                bot.Conn.Send(new Craft { RecipeId = a[1], Materials = mats.Count > 0 ? mats : null });
                Console.WriteLine($"คราฟต์ {a[1]} ({mats.Count} ช่อง)");
                bot.BusyUntil = Times.UnixTimeNow() + 2.5;
                break;
            }

            case "equip":
            {
                if (a.Length < 3) { Console.WriteLine("ใช้: equip <itemId> <slot> [equip|unequip]"); break; }
                // ⚠️ ต้องส่ง Action ด้วย — server เช็ค msg.Action == "equip"
                // ไม่ส่ง = ถูกตีความว่า "ถอด" แล้วตอบ Abort (เคยหลงคิดว่าใส่ของไม่ได้เพราะเหตุนี้)
                string act = a.Length >= 4 ? a[3] : "equip";
                bot.Conn.Send(new Equip { ItemId = a[1], SlotName = a[2], Action = act });
                Console.WriteLine($"{(act == "equip" ? "ใส่" : "ถอด")} {a[1]} ที่ช่อง {a[2]}");
                break;
            }

            case "chat":
            {
                string body = line.Substring(line.IndexOf(' ') + 1);
                bot.Conn.Send(new SayInExclusiveChannel
                {
                    Message = new Message_ { EntityId = bot.EntityId, Body = body, Time = Times.UnixTimeNow() }
                });
                Console.WriteLine($"พูด: {body}");
                break;
            }

            case "cheat":
            {
                string body = line.Substring(line.IndexOf(' ') + 1);
                bot.Conn.Send(new Cheat { _Cheat = body });
                Console.WriteLine($"cheat: {body}");
                waitUntil = Times.UnixTimeNow() + 0.5;
                break;
            }

            case "control":
            {
                // control <ชื่อ|id> <tp|walk|stop|gather|attack|say|status> [args]
                // ส่งผ่าน packet Cheat ให้ server ไปสั่งตัวละครคนนั้นอีกที
                if (a.Length < 3) { Console.WriteLine("ใช้: control <ชื่อ|id> <tp|walk|stop|gather|attack|say|status> [args]"); break; }
                bot.Conn.Send(new Cheat { _Cheat = line });
                Console.WriteLine($"สั่ง {a[1]}: {string.Join(" ", a, 2, a.Length - 2)}");
                waitUntil = Times.UnixTimeNow() + 0.5;
                break;
            }

            case "chunk":
                SyncChunk(bot, force: true);
                Console.WriteLine($"ขอ chunk {bot.Chunk.x},{bot.Chunk.y}");
                waitUntil = Times.UnixTimeNow() + 0.5;
                break;

            case "skill":
            {
                // skill <id> <level> — เรียนสกิล (ต้องมีแต้มสกิลจากการขึ้นเลเวล)
                if (a.Length < 2)
                {
                    Console.WriteLine("ใช้: skill <ชื่อสกิล> [เลเวล]  เช่น  skill gathering 5");
                    break;
                }
                int lv = a.Length > 2 && int.TryParse(a[2], out int parsed) ? parsed : 1;
                bot.Conn.Send(new LearnSkill { SkillId = a[1], SubId = null, Level = lv });
                Console.WriteLine($"ขอเรียน {a[1]} เลเวล {lv}");
                waitUntil = Times.UnixTimeNow() + 0.8;
                break;
            }

            case "actions":
                bot.Conn.Send(default(GetActions));
                waitUntil = Times.UnixTimeNow() + 0.5;
                Console.WriteLine("ท่าโจมตีที่มี: " + (bot.BattleActions.Length == 0 ? "(ยังไม่มี)" : string.Join(", ", bot.BattleActions)));
                break;

            default:
                Console.WriteLine("ไม่รู้จักคำสั่ง: " + cmd + " (พิมพ์ help)");
                break;
        }
        return true;
    }

    private static void PrintHelp()
    {
        Console.WriteLine(@"คำสั่ง (คั่นหลายคำสั่งด้วย ; ได้)
  status                 สถานะตัวเอง (ตำแหน่ง/เลือด/สตามินา/ความล้า)
  inv                    ของในกระเป๋า
  look                   สิ่งที่อยู่รอบตัว (ผู้เล่น/สัตว์/สิ่งปลูกสร้าง/ธรรมชาติ)
  move <tx> <ty>         เดินไปที่ tile
  goto <id|near>         เดินไปหา entity (near = ตัวที่ใกล้สุด)
  touch [tx ty]          แตะของธรรมชาติ (ไม่ใส่พิกัด = ใกล้สุด)
  collect [generatorId]  เก็บจากจุดที่แตะไว้
  farm [วินาที]           วนเก็บของเองจนครบเวลา
  target <id|near>       เล็งเป้า
  attack [id] [actionId] โจมตี 1 ครั้ง
  kill [id] [วินาที]      โจมตีซ้ำจนตายหรือหมดเวลา
  revive                 ขอฟื้นหลังตาย
  craft <recipe> [slot=itemId ...]
  equip <itemId> <slot>
  chat <ข้อความ>          พูดในช่องรวม
  cheat <คำสั่ง>          ส่ง packet Cheat (add axe / rest / hurt / tired ...)
  control <ชื่อ> <คำสั่ง>  คุมตัวละครของผู้เล่นคนอื่น (ที่เล่นในตัวเกมจริง):
                           tp <tx> <ty> · walk <tx> <ty> · stop · gather · attack · say <ข้อความ> · status
  chunk                  ขอข้อมูล chunk ที่ยืนอยู่ใหม่
  actions                ขอ/โชว์ท่าโจมตีที่ server ให้มา
  dump                   สลับโหมดโชว์ packet ที่เข้ามา
  wait <วินาที>           รอ
  quit                   ออก");
    }

    private static void PrintStatus(Bot bot)
    {
        Console.WriteLine($"[สถานะ] {bot.EntityId} tile {bot.Tx:F1},{bot.Ty:F1} (โลก {bot.X:F0},{bot.Y:F0}) chunk {bot.Chunk.x},{bot.Chunk.y}");
        Console.WriteLine($"         เลือด {Val(bot.Life):F0} · สตามินา {Val(GaugeOf(bot, "stamina")):F0} · ความล้า {Val(GaugeOf(bot, "fatigue")):F0}"
            + (bot.Dead ? " · ☠ ตายแล้ว" : ""));
        Console.WriteLine($"         ของ {bot.Inventory.Count} ชิ้น · รู้จักธรรมชาติ {bot.Naturals.Count} จุด · เห็น entity {bot.Entities.Count} ตัว");
    }

    private static void PrintInventory(Bot bot)
    {
        if (bot.Inventory.Count == 0)
        {
            Console.WriteLine("กระเป๋าว่าง");
            return;
        }
        Console.WriteLine($"ของในกระเป๋า {bot.Inventory.Count} ชิ้น:");
        for (int i = 0; i < bot.Inventory.Count; i++)
        {
            Item it = bot.Inventory[i];
            // โชว์ slot ที่ server แนบมาใน Performance ด้วย — client ใช้ค่านี้ตัดสินว่าใส่ได้ไหม
            // (EquipSystem.EquipItem → GetStringAttribute("slot")) ไม่มีค่านี้ = กดใส่แล้วเงียบ
            string slot = null;
            if (it.Performance != null)
            {
                for (int k = 0; k < it.Performance.Length && slot == null; k++)
                {
                    if (it.Performance[k].Strs != null)
                    {
                        it.Performance[k].Strs.TryGetValue("slot", out slot);
                    }
                }
            }
            string dura = it.Durability == null ? "" : $" ทน {it.Durability.Get(0.0):F0}/{it.Durability.RealMax():F0}";
            Console.WriteLine($"  {i,2}. {it.Name} ({it.Prototype})  slot={slot ?? "-"}{dura}  id={it.Id}");
        }
    }

    private static void PrintSurroundings(Bot bot)
    {
        Console.WriteLine($"รอบตัว (ยืนที่ tile {bot.Tx:F1},{bot.Ty:F1}):");
        var list = new List<(double d, Entity e)>();
        foreach (Entity e in bot.Entities.Values)
        {
            list.Add((Dist2(bot, e.X, e.Y), e));
        }
        list.Sort((p, q) => p.d.CompareTo(q.d));
        for (int i = 0; i < list.Count && i < 15; i++)
        {
            Entity e = list[i].e;
            Console.WriteLine($"  {e.Kind,-8} {Label(e)}  tile {e.X / TileSize:F0},{e.Y / TileSize:F0}  ห่าง {Math.Sqrt(list[i].d):F1} tile"
                + (e.Life >= 0 ? $"  เลือด {e.Life:F0}" : "") + (e.Alive ? "" : "  ☠"));
        }
        if (list.Count > 15)
        {
            Console.WriteLine($"  ... อีก {list.Count - 15} ตัว");
        }
        (int x, int y)? near = NearestNatural(bot);
        Console.WriteLine(near == null
            ? "  ธรรมชาติ: ยังไม่รู้จักจุดไหนเลย"
            : $"  ธรรมชาติใกล้สุด: tile {near.Value.x},{near.Value.y} ชนิด {bot.Naturals[near.Value]} (รู้จักทั้งหมด {bot.Naturals.Count} จุด)");
    }

    // ───────────────────────────── พฤติกรรมอัตโนมัติ ─────────────────────────────

    private static void Farm(Bot bot, double seconds)
    {
        double endAt = Times.UnixTimeNow() + seconds;
        int got = 0, touched = 0;
        var bad = new HashSet<(int, int)>();
        Console.WriteLine($"เริ่มฟาร์ม {seconds:F0} วินาที...");
        while (Times.UnixTimeNow() < endAt)
        {
            bot.Conn.Process();
            double now = Times.UnixTimeNow();
            if (now < bot.BusyUntil) { Thread.Sleep(20); continue; }

            if (bot.PendingNaturalId != null && bot.PendingGeneratorId != null)
            {
                int before = bot.Inventory.Count;
                bot.Conn.Send(new Collect { EntityId = bot.PendingNaturalId, GeneratorId = bot.PendingGeneratorId, Tile = bot.PendingTile });
                bot.BusyUntil = now + 2.4;
                Pump(bot, 2500);
                if (bot.Inventory.Count > before) { got++; }
                continue;
            }

            (int x, int y)? tile = NearestNatural(bot, bad);
            if (tile == null)
            {
                SendMove(bot, bot.X + 600f, bot.Y + 200f);
                SyncChunk(bot);
                Pump(bot, 500);
                continue;
            }
            WalkTo(bot, tile.Value.x * TileSize, tile.Value.y * TileSize);
            Touch(bot, tile.Value, quiet: true);
            touched++;
            Pump(bot, 700);
            if (bot.PendingNaturalId == null)
            {
                bad.Add(tile.Value);       // server ปฏิเสธจุดนี้ ไม่ต้องกลับมาอีก
            }
        }
        Console.WriteLine($"ฟาร์มจบ: แตะ {touched} จุด ได้ของ {got} ชิ้น (กระเป๋ามี {bot.Inventory.Count} ชิ้น)");
    }

    private static void Kill(Bot bot, Entity target, double seconds)
    {
        double endAt = Times.UnixTimeNow() + seconds;
        string action = DefaultAction(bot);
        int swings = 0;
        Console.WriteLine($"ตี {Label(target)} จนตาย (สูงสุด {seconds:F0} วินาที)...");
        WalkTo(bot, target.X, target.Y);
        bot.Conn.Send(new SelectBattleTarget { EntityId = target.Id, Tile = new Point2((int)(target.X / TileSize), (int)(target.Y / TileSize)) });
        Pump(bot, 400);
        while (Times.UnixTimeNow() < endAt)
        {
            if (!bot.Entities.TryGetValue(target.Id, out Entity cur) || !cur.Alive)
            {
                Console.WriteLine($"{Label(target)} ตายแล้ว (ตีไป {swings} ครั้ง)");
                return;
            }
            if (bot.Dead)
            {
                Console.WriteLine($"เราตายก่อน (ตีไป {swings} ครั้ง)");
                return;
            }
            Attack(bot, cur, action, quiet: true);
            swings++;
            Pump(bot, 1000);
        }
        Console.WriteLine($"หมดเวลา — ตีไป {swings} ครั้ง เป้าหมายเลือดเหลือ "
            + (bot.Entities.TryGetValue(target.Id, out Entity last) ? last.Life.ToString("F0") : "?"));
    }

    // ───────────────────────────── ท่อส่ง packet ─────────────────────────────

    private static void Attack(Bot bot, Entity target, string action, bool quiet = false)
    {
        bot.Conn.Send(new UseBattleAction
        {
            ActionId = action,
            StartAt = Times.UnixTimeNow(),
            TargetEntityId = target.Id,
            TargetTile = new Point2((int)(target.X / TileSize), (int)(target.Y / TileSize))
        });
        if (!quiet)
        {
            Console.WriteLine($"ตี {Label(target)} ด้วย {action}");
        }
    }

    private static void Touch(Bot bot, (int x, int y) tile, bool quiet = false)
    {
        bot.PendingNaturalId = null;
        bot.PendingGeneratorId = null;
        bot.PendingTile = new Point2(tile.x, tile.y);
        ushort type = bot.Naturals.TryGetValue(tile, out ushort t) ? t : (ushort)12119;
        bot.Conn.Send(new Touch { EntityId = $"natural_{tile.x}_{tile.y}", EntityType = type, Tile = bot.PendingTile });
        if (!quiet)
        {
            Console.WriteLine($"แตะ tile {tile.x},{tile.y} (ชนิด {type})");
        }
    }

    /// <summary>
    /// เดินไปจุดหมายเป็นก้าว ๆ — server กันการวาร์ป (M-2: เกิน ~900 หน่วย/วิ ถูกดึงกลับ)
    /// เดิมบอทส่ง Move ทีเดียวข้ามครึ่งแมพ ซึ่งตอนนี้ไม่ผ่านแล้ว
    /// </summary>
    private static void WalkTo(Bot bot, float x, float y)
    {
        for (int guard = 0; guard < 40; guard++)
        {
            float dx = x - bot.X, dy = y - bot.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist < 1f) return;
            float step = MathF.Min(700f, dist);
            SendMove(bot, bot.X + dx / dist * step, bot.Y + dy / dist * step);
            SyncChunk(bot);
            Pump(bot, 500);
            if (dist <= 700f) return;
        }
    }

    private static void SendMove(Bot bot, float x, float y)
    {
        bot.X = x;
        bot.Y = y;
        bot.Conn.Send(new Move
        {
            EntityId = bot.EntityId,
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

    private static void SyncChunk(Bot bot, bool force = false)
    {
        var c = new Point2((int)(bot.X / TileSize / 16), (int)(bot.Y / TileSize / 16));
        if (force || c.x != bot.Chunk.x || c.y != bot.Chunk.y)
        {
            bot.Chunk = c;
            bot.Conn.Send(new SetChunk { Chunk = c });
        }
    }

    // ───────────────────────────── รับ packet ─────────────────────────────

    private static void RegisterHandlers(Bot bot)
    {
        Connection c = bot.Conn;

        c.Recv<Welcome>((m, h) => Console.WriteLine($"[เข้าเกม] region={m.Region.Name}"));
        c.Recv<Clock>((m, h) => { });
        c.Recv<OK>((m, h) => { if (bot.Dump) Console.WriteLine("[recv] OK"); });
        c.Recv<Abort>((m, h) =>
        {
            Console.WriteLine("[server ปฏิเสธ] Abort");
            bot.BusyUntil = 0;
            bot.PendingNaturalId = null;
            bot.PendingGeneratorId = null;
        });
        c.Recv<Info>((m, h) => Console.WriteLine("[info] " + m.Text));
        c.Recv<Messages.Timer>((m, h) => { if (bot.Dump) Console.WriteLine($"[recv] Timer {m.Duration}s"); });

        c.Recv<Inventory>((m, h) =>
        {
            bot.Inventory.Clear();
            if (m.InventoryItems.Items != null) bot.Inventory.AddRange(m.InventoryItems.Items);
        });
        c.Recv<InventoryUpdated>((m, h) => bot.Conn.Send(default(GetInventory)));
        c.Recv<Collected>((m, h) =>
        {
            bot.BusyUntil = 0;
            string got = m.Items != null && m.Items.Length > 0 ? m.Items[0].Name : "?";
            Console.WriteLine($"[เก็บได้] {got}" + (m.RanOut ? " (จุดนี้หมดแล้ว)" : ""));
            if (m.RanOut) { bot.PendingNaturalId = null; bot.PendingGeneratorId = null; }
        });
        c.Recv<Crafted>((m, h) =>
        {
            bot.BusyUntil = 0;
            Console.WriteLine("[คราฟต์สำเร็จ] " + (m.Items != null && m.Items.Length > 0 ? m.Items[0].Name : "?"));
        });
        c.Recv<Touched>((m, h) =>
        {
            Generator[] gens = m.Collectible.Generators;
            if (gens != null && gens.Length > 0)
            {
                bot.PendingNaturalId = m.EntityId;
                bot.PendingGeneratorId = gens[0].Id;
                if (bot.Dump) Console.WriteLine($"[แตะได้] {m.EntityId} เก็บได้: {string.Join(", ", Array.ConvertAll(gens, g => $"{g.Name} x{g.Amount}"))}");
            }
        });
        c.Recv<Collectible>((m, h) => { });
        c.Recv<CollectibleChanged>((m, h) => { });

        c.Recv<Chunk>((m, h) =>
        {
            byte[] g = m.Garden;
            if (g == null) return;
            for (int i = 0; i + 6 <= g.Length; i += 6)
            {
                bot.Naturals[(BitConverter.ToUInt16(g, i), BitConverter.ToUInt16(g, i + 2))] = BitConverter.ToUInt16(g, i + 4);
            }
        });
        c.Recv<DisappearEntityOnTile>((m, h) => bot.Naturals.Remove((m.Tile.x, m.Tile.y)));

        c.Recv<AppearPlayer>((m, h) =>
        {
            Entity e = Touch(bot, m.EntityId, "player");
            e.Name = m.Name;
            e.Level = m.Level;
            e.Type = m.EntityType;
            e.Alive = m.IsAlive;
            SetPos(e, m.Move);
            e.Life = Val(m.Survival.Life);
        });
        c.Recv<AppearAnimal>((m, h) =>
        {
            Entity e = Touch(bot, m.EntityId, "animal");
            e.Name = "สัตว์ type " + m.EntityType;
            e.Type = m.EntityType;
            e.Level = m.Level;
            e.Alive = m.IsAlive;
            SetPos(e, m.Move);
            e.Life = Val(m.Survival.Life);
        });
        c.Recv<AppearArtifact>((m, h) =>
        {
            Entity e = Touch(bot, m.EntityId, "artifact");
            e.Type = m.EntityType;
            e.X = m.Tile.x * TileSize;
            e.Y = m.Tile.y * TileSize;
        });
        c.Recv<DisappearEntity>((m, h) => bot.Entities.Remove(m.EntityId));
        c.Recv<Move>((m, h) =>
        {
            if (m.EntityId == bot.EntityId)
            {
                // server สั่งย้ายเราเอง (วาร์ป/เกิดใหม่) — จำตำแหน่งใหม่ไว้ ไม่งั้นคำสั่งถัดไปใช้พิกัดเก่า
                var self = new Entity();
                SetPos(self, m);
                if (self.X != 0f || self.Y != 0f)
                {
                    bot.X = self.X;
                    bot.Y = self.Y;
                    Console.WriteLine($"[ย้ายตำแหน่ง] server ย้ายเราไป tile {bot.Tx:F0},{bot.Ty:F0}");
                }
                return;
            }
            if (bot.Entities.TryGetValue(m.EntityId ?? "", out Entity e)) SetPos(e, m);
        });

        c.Recv<Survival>((m, h) =>
        {
            if (m.EntityId == bot.EntityId)
            {
                bot.Life = m.Life;
                if (m.Gauges != null) foreach (var kv in m.Gauges) bot.Gauges[kv.Key] = kv.Value;
            }
            else if (bot.Entities.TryGetValue(m.EntityId ?? "", out Entity e))
            {
                e.Life = Val(m.Life);
            }
        });
        c.Recv<SurvivalUpdated>((m, h) =>
        {
            if (m.Updated == null) return;
            if (m.EntityId == bot.EntityId)
            {
                foreach (var kv in m.Updated)
                {
                    if (kv.Key == "life") bot.Life = kv.Value; else bot.Gauges[kv.Key] = kv.Value;
                }
            }
            else if (bot.Entities.TryGetValue(m.EntityId ?? "", out Entity e) && m.Updated.TryGetValue("life", out Gauge lg))
            {
                e.Life = Val(lg);
            }
        });

        // ── ต่อสู้ (ยังไม่มีฝั่ง server — ใส่ไว้ให้เห็นทันทีที่ทำเสร็จ) ──
        c.Recv<Actions>((m, h) =>
        {
            bot.BattleActions = m.BattleActions == null
                ? Array.Empty<string>()
                : Array.ConvertAll(m.BattleActions, x => x.Id);
            Console.WriteLine("[ท่าโจมตี] " + (bot.BattleActions.Length == 0 ? "(ว่าง)" : string.Join(", ", bot.BattleActions)));
        });
        c.Recv<Damaged>((m, h) =>
        {
            string who = m.VictimId == bot.EntityId ? "เรา" : Short(m.VictimId);
            string by = m.AttackerId == bot.EntityId ? "เรา" : Short(m.AttackerId);
            // ใส่เวลาไว้ด้วย — ใช้วัด "โดนตีแล้วสัตว์สวนกลับช้าแค่ไหน" ได้จาก log ตรง ๆ
            Console.WriteLine($"[ดาเมจ {DateTime.Now:HH:mm:ss.f}] {by} → {who} {m.Damage.Value} ({m.Damage.Result})");
        });
        c.Recv<BattleBegun>((m, h) => Console.WriteLine($"[เข้าสู่การต่อสู้] {Short(m.EntityId)} vs {Short(m.EnemyId)}"));
        c.Recv<BattleEnded>((m, h) => Console.WriteLine($"[จบการต่อสู้] {Short(m.EntityId)}"));
        c.Recv<EntityDied>((m, h) =>
        {
            if (m.EntityId == bot.EntityId) { bot.Dead = true; Console.WriteLine("[ตาย] เราตายแล้ว — สั่ง revive เพื่อฟื้น"); }
            else
            {
                if (bot.Entities.TryGetValue(m.EntityId ?? "", out Entity e)) e.Alive = false;
                Console.WriteLine($"[ตาย] {Short(m.EntityId)}");
            }
        });
        c.Recv<Revived>((m, h) => { bot.Dead = false; Console.WriteLine("[ฟื้น] กลับมาแล้ว"); });

        // ที่เหลือรับไว้เฉย ๆ กันคำเตือน "ไม่มี handler"
        c.Recv<Equipments>((m, h) => { });
        c.Recv<Skills>((m, h) => { });
        c.Recv<Statistics>((m, h) => { });
        c.Recv<DefoggedChunks>((m, h) => { });
        c.Recv<QuestCategories>((m, h) => { });
        c.Recv<Quests>((m, h) => { });
        c.Recv<WalletUpdated>((m, h) => { });
        c.Recv<AvailableEmotions>((m, h) => { });
        c.Recv<PlayEmoticon>((m, h) => { });
        c.Recv<Recipes>((m, h) => { });
        c.Recv<ArtifactBlueprints>((m, h) => { });
        c.Recv<SayInExclusiveChannel>((m, h) =>
            Console.WriteLine($"[แชท] {(m.Message.Speaker.HasValue ? m.Message.Speaker.Value.Name : "?")}: {m.Message.Body}"));
    }

    // ───────────────────────────── helper ─────────────────────────────

    private static Entity Touch(Bot bot, string id, string kind)
    {
        if (string.IsNullOrEmpty(id))
        {
            id = "(ไม่มีชื่อ)";
        }
        if (!bot.Entities.TryGetValue(id, out Entity e))
        {
            e = new Entity { Id = id, Kind = kind };
            bot.Entities[id] = e;
        }
        e.Kind = kind;
        return e;
    }

    private static void SetPos(Entity e, Move move)
    {
        Movement[] ms = move.Movements;
        if (ms == null || ms.Length == 0) return;
        Location[] path = ms[ms.Length - 1].Path;
        if (path == null || path.Length == 0) return;
        e.X = path[path.Length - 1].Position.x;
        e.Y = path[path.Length - 1].Position.y;
    }

    private static double Dist2(Bot bot, float x, float y)
    {
        double dx = (x - bot.X) / TileSize;
        double dy = (y - bot.Y) / TileSize;
        return dx * dx + dy * dy;
    }

    private static Entity FindEntity(Bot bot, string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (bot.Entities.TryGetValue(key, out Entity exact)) return exact;

        bool animalsOnly = key == "near" || key == "animal";
        bool playersOnly = key == "player";
        Entity best = null;
        double bestD = double.MaxValue;
        foreach (Entity e in bot.Entities.Values)
        {
            if (e.Id == bot.EntityId) continue;
            if (animalsOnly && e.Kind != "animal") continue;
            if (playersOnly && e.Kind != "player") continue;
            if (!animalsOnly && !playersOnly && e.Id.IndexOf(key, StringComparison.OrdinalIgnoreCase) < 0) continue;
            if (!e.Alive) continue;
            double d = Dist2(bot, e.X, e.Y);
            if (d < bestD) { bestD = d; best = e; }
        }
        return best;
    }

    private static (int x, int y)? NearestNatural(Bot bot, HashSet<(int, int)> skip = null)
    {
        (int x, int y)? best = null;
        double bestD = double.MaxValue;
        foreach (var kv in bot.Naturals)
        {
            if (skip != null && skip.Contains(kv.Key)) continue;
            double dx = kv.Key.x - bot.Tx;
            double dy = kv.Key.y - bot.Ty;
            double d = dx * dx + dy * dy;
            if (d < bestD) { bestD = d; best = kv.Key; }
        }
        return best;
    }

    private static string DefaultAction(Bot bot)
    {
        return bot.BattleActions.Length > 0 ? bot.BattleActions[0] : "barehand_default_a";
    }

    private static string Label(Entity e)
    {
        return $"{(string.IsNullOrEmpty(e.Name) ? Short(e.Id) : e.Name)}[{Short(e.Id)}]";
    }

    private static string Short(string id)
    {
        if (string.IsNullOrEmpty(id)) return "?";
        return id.Length <= 12 ? id : id.Substring(0, 12) + "…";
    }

    private static float ParseF(string s)
    {
        return float.Parse(s, CultureInfo.InvariantCulture);
    }

    private static double ParseD(string s)
    {
        return double.Parse(s, CultureInfo.InvariantCulture);
    }

    private static void Pump(Bot bot, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            bot.Conn.Process();
            Thread.Sleep(10);
        }
    }
}
