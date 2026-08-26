using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

public static class Program
{
    private static int _receivedCount;

    /// <summary>เฟส C — เก็บ item id ล่าสุดจาก Inventory ไว้ใช้ทดสอบ Equip</summary>
    private static string _lastAxeItemId;
    private static string _lastBoxCapsuleId;
    private static string _lastLeafItemId;
    private static string _placedBoxId;
    private static string _lastClothesItemId;

    private static void OnInventory(Inventory msg, PacketHeader header)
    {
        OnMsg(msg, header);
        Item[] items = msg.InventoryItems.Items;
        if (items == null) return;
        foreach (Item it in items)
        {
            if (it.Prototype == "axe_onehand_stone_01") _lastAxeItemId = it.Id;
            if (it.Prototype == "clothes_builder_01") _lastClothesItemId = it.Id;
            if (it.Prototype == "capsulated_fur_box_03_leaf") _lastBoxCapsuleId = it.Id;
            if (it.Prototype == "leaf") _lastLeafItemId = it.Id;
        }
    }

    private static string Show(Gauge g)
    {
        if (g == null) return "(null)";
        double now = Times.UnixTimeNow();
        float v = g.Get(now);
        float later = g.Get(now + 10.0);
        string trend = Math.Abs(later - v) < 0.01f ? "นิ่ง" : (later > v ? $"เพิ่ม→{later:F0} ใน 10 วิ" : $"ลด→{later:F0} ใน 10 วิ");
        return $"{v:F0}/{g.Max(now):F0} ({trend})";
    }

    private static void OnSurvival(Survival msg, PacketHeader header)
    {
        _receivedCount++;
        Console.WriteLine("[recv] Survival ของ " + msg.EntityId);
        if (msg.Gauges != null)
            foreach (var kv in msg.Gauges)
                Console.WriteLine($"         {kv.Key,-8} = {Show(kv.Value)}");
    }

    private static void OnSurvivalUpdated(SurvivalUpdated msg, PacketHeader header)
    {
        _receivedCount++;
        string keys = msg.Updated == null ? "-" : string.Join(",", msg.Updated.Keys);
        Console.Write($"[recv] SurvivalUpdated [{keys}]");
        if (msg.Updated != null)
            foreach (var kv in msg.Updated)
                Console.Write($"  {kv.Key}={Show(kv.Value)}");
        Console.WriteLine();
    }

    private static int _animalCount;
    private static readonly HashSet<string> _animalIds = new HashSet<string>();

    private static void OnAnimal(AppearAnimal msg, PacketHeader header)
    {
        _receivedCount++;
        _animalCount++;
        _animalIds.Add(msg.EntityId);
        if (_animalCount <= 3)
        {
            var p0 = msg.Move.Movements[0].Path[0];
            Console.WriteLine($"[recv] AppearAnimal {msg.EntityId} type={msg.EntityType} lv={msg.Level} scale={msg.Display.BaseScale} pos=({p0.Position.x:F0},{p0.Position.y:F0}) life={msg.Survival.Life.Get():F0}");
        }
    }

    private static void OnArtifact(AppearArtifact msg, PacketHeader header)
    {
        _receivedCount++;
        _placedBoxId = msg.EntityId;
        Console.WriteLine($"[recv] AppearArtifact {msg.EntityId} type={msg.EntityType} tile={msg.Tile.x},{msg.Tile.y}");
    }

    private static void OnInventoryUpdated(InventoryUpdated msg, PacketHeader header)
    {
        _receivedCount++;
        int added = msg.Items?.Length ?? 0;
        int removed = msg.RemovedItemIds?.Length ?? 0;
        Console.WriteLine($"[recv] InventoryUpdated ของ {msg.EntityId}: +{added} -{removed}");
    }

    private static void OnPlayerDisplay(PlayerDisplay msg, PacketHeader header)
    {
        _receivedCount++;
        Console.WriteLine($"[recv] PlayerDisplay: Equip={msg.Equip ?? "(ไม่มี)"} | Body={msg.Body ?? "(ไม่มี)"} | Framework={msg.WeaponInfo.WeaponFramework ?? "-"}");
    }

    private static void OnMsg<T>(T msg, PacketHeader header)
    {
        _receivedCount++;
        string text;
        try
        {
            text = msg?.ToString() ?? "<null>";
        }
        catch (Exception e)
        {
            text = "<ToString threw: " + e.GetType().Name + ">";
        }
        if (text.Length > 400)
        {
            text = text.Substring(0, 400) + "...";
        }
        Console.WriteLine($"[recv] type={header.TypeCode} seq={header.Seq} replyOf={header.ReplyOf}: {text}");
    }

    private static int _animalMoves;

    private static void OnMove(Move msg, PacketHeader header)
    {
        _receivedCount++;
        if (msg.EntityId != null && msg.EntityId.StartsWith("animal_"))
        {
            _animalMoves++;
            if (_animalMoves <= 3)
            {
                var path = msg.Movements[0].Path;
                Console.WriteLine($"[recv] สัตว์ {msg.EntityId} เดิน ({path[0].Position.x:F0},{path[0].Position.y:F0}) → ({path[^1].Position.x:F0},{path[^1].Position.y:F0}) ใช้เวลา {path[^1].Time - path[0].Time:F1} วิ");
            }
        }
    }

    private static void Reg<T>(Connection c)
    {
        c.Recv<T>(OnMsg);
    }

    public static void Main(string[] args)
    {
        // --console = บอทคอนโซล สั่งงานด้วยคำสั่งแล้วยิง packet ให้ (แนว OpenKore)
        //   --console [host] [port] [ชื่อ] [port gateway]     พิมพ์คำสั่งเอง / ป้อนทาง stdin
        //   --console --cmd "look; farm 20; status"           สั่งรวดเดียวแล้วออก
        //   --console --script ไฟล์.txt                        อ่านคำสั่งจากไฟล์ (บรรทัดละคำสั่ง, # = คอมเมนต์)
        if (args.Length >= 1 && args[0] == "--console")
        {
            string ch = "127.0.0.1";
            int cp = 8191;
            string cname = "bot-1";
            int cgw = 0;
            var queued = new List<string>();
            bool interactive = true;
            var positional = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "--cmd" && i + 1 < args.Length)
                {
                    foreach (string part in args[++i].Split(';'))
                    {
                        if (!string.IsNullOrWhiteSpace(part)) queued.Add(part.Trim());
                    }
                    interactive = false;
                }
                else if (args[i] == "--script" && i + 1 < args.Length)
                {
                    foreach (string raw in System.IO.File.ReadAllLines(args[++i]))
                    {
                        string l = raw.Trim();
                        if (l.Length == 0 || l.StartsWith("#")) continue;
                        foreach (string part in l.Split(';'))
                        {
                            if (!string.IsNullOrWhiteSpace(part)) queued.Add(part.Trim());
                        }
                    }
                    interactive = false;
                }
                else
                {
                    positional.Add(args[i]);
                }
            }
            if (positional.Count >= 1) ch = positional[0];
            if (positional.Count >= 2) cp = int.Parse(positional[1]);
            if (positional.Count >= 3) cname = positional[2];
            if (positional.Count >= 4) cgw = int.Parse(positional[3]);
            Environment.ExitCode = BotConsole.Run(ch, cp, cgw > 0 ? cgw : cp - 1, cname, queued, interactive);
            return;
        }

        // --gp-check [host] [port เกม] [port gateway] = เทสว่า server ปฏิเสธ packet โกงจริงไหม (GP-08/09/12)
        if (args.Length >= 1 && args[0] == "--gp-check")
        {
            string gh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int gp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int ggw = args.Length >= 4 ? int.Parse(args[3]) : gp - 1;
            Environment.ExitCode = GpCheck.Run(gh, gp, ggw);
            return;
        }

        // --stamina-check [host] [port เกม] [port gateway] = เทสระบบสตามินา/ความล้าด้วยตัวละครเลเวล 1
        if (args.Length >= 1 && args[0] == "--stamina-check")
        {
            string sh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int sp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int sgw = args.Length >= 4 ? int.Parse(args[3]) : sp - 1;
            Environment.ExitCode = StaminaCheck.Run(sh, sp, sgw);
            return;
        }

        // --skill-check [host] [port เกม] [port gateway] = เทสความชำนาญหมวดสกิล + ค่าสถานะตอนเข้าเกม
        if (args.Length >= 1 && args[0] == "--skill-check")
        {
            string sh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int spp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int sgw = args.Length >= 4 ? int.Parse(args[3]) : spp - 1;
            Environment.ExitCode = SkillCheck.Run(sh, spp, sgw);
            return;
        }

        // --combat-skill-check [host] [port เกม] [port gateway] = เทสท่าต่อสู้ยึดจากสกิลที่เรียนจริง
        if (args.Length >= 1 && args[0] == "--combat-skill-check")
        {
            string csh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int csp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int csgw = args.Length >= 4 ? int.Parse(args[3]) : csp - 1;
            Environment.ExitCode = CombatSkillCheck.Run(csh, csp, csgw);
            return;
        }

        // --quest-check [host] [port เกม] [port gateway] = เทสระบบเควส (สายสอนเล่น 8 ขั้น จบที่ต่อแพ)
        if (args.Length >= 1 && args[0] == "--quest-check")
        {
            string qh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int qp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int qgw = args.Length >= 4 ? int.Parse(args[3]) : qp - 1;
            Environment.ExitCode = QuestCheck.Run(qh, qp, qgw);
            return;
        }

        // --farm-check [host] [port เกม] [port gateway] = เทสระบบปลูกผัก (ปลูก/รดน้ำ/ใส่ปุ๋ย/เก็บเกี่ยว)
        if (args.Length >= 1 && args[0] == "--farm-check")
        {
            string fh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int fp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int fgw = args.Length >= 4 ? int.Parse(args[3]) : fp - 1;
            Environment.ExitCode = FarmCheck.Run(fh, fp, fgw);
            return;
        }

        // --farm-resume-check <setup|verify> = เทสว่ารีสตาร์ทเซิร์ฟแล้วผลผลิตไม่เกิดใหม่ (รัน 2 เฟส)
        if (args.Length >= 1 && args[0] == "--farm-resume-check")
        {
            string ph = args.Length >= 2 ? args[1] : "setup";
            string rh = args.Length >= 3 ? args[2] : "127.0.0.1";
            int rp = args.Length >= 4 ? int.Parse(args[3]) : 8191;
            int rgw = args.Length >= 5 ? int.Parse(args[4]) : rp - 1;
            Environment.ExitCode = FarmCheck.RunResume(ph, rh, rp, rgw);
            return;
        }

        // --vision-check [host] [port เกม] [port gateway] = เทสระยะการมองเห็น (เข้า/ออกจอ · ไม่ส่ง packet คนไกล)
        if (args.Length >= 1 && args[0] == "--vision-check")
        {
            string vh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int vp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int vgw = args.Length >= 4 ? int.Parse(args[3]) : vp - 1;
            Environment.ExitCode = VisionCheck.Run(vh, vp, vgw);
            return;
        }

        // --stat-check [host] [port เกม] [port gateway] = เทสค่าสถานะตัวละคร (8 ตัว · หลอดโตตามเลเวล · พลังอาวุธ/เกราะ)
        if (args.Length >= 1 && args[0] == "--stat-check")
        {
            string ah = args.Length >= 2 ? args[1] : "127.0.0.1";
            int ap = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int agw = args.Length >= 4 ? int.Parse(args[3]) : ap - 1;
            Environment.ExitCode = StatCheck.Run(ah, ap, agw);
            return;
        }

        // --group2-check: real packet + save/reconnect coverage for all character group 2 UI data.
        if (args.Length >= 1 && args[0] == "--group2-check")
        {
            string h = args.Length >= 2 ? args[1] : "127.0.0.1";
            int p = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int gw = args.Length >= 4 ? int.Parse(args[3]) : p - 1;
            Environment.ExitCode = Group2Check.Run(h, p, gw);
            return;
        }

        if (args.Length >= 1 && args[0] == "--character-check")
        {
            string h = args.Length >= 2 ? args[1] : "127.0.0.1";
            int p = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int gw = args.Length >= 4 ? int.Parse(args[3]) : p - 1;
            Environment.ExitCode = CharacterSystemsCheck.Run(h, p, gw);
            return;
        }

        if (args.Length >= 1 && args[0] == "--poi-check")
        {
            string h = args.Length >= 2 ? args[1] : "127.0.0.1";
            int p = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int gw = args.Length >= 4 ? int.Parse(args[3]) : p - 1;
            Environment.ExitCode = PoiCheck.Run(h, p, gw);
            return;
        }

        if (args.Length >= 1 && args[0] == "--create-check")
        {
            string h = args.Length >= 2 ? args[1] : "127.0.0.1";
            int p = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int gw = args.Length >= 4 ? int.Parse(args[3]) : p - 1;
            Environment.ExitCode = CreateCharacterCheck.Run(h, p, gw);
            return;
        }

        if (args.Length >= 1 && args[0] == "--recipe-check")
        {
            string h = args.Length >= 2 ? args[1] : "127.0.0.1";
            int p = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int gw = args.Length >= 4 ? int.Parse(args[3]) : p - 1;
            Environment.ExitCode = RecipeCheck.Run(h, p, gw);
            return;
        }

        if (args.Length >= 1 && args[0] == "--smoke-check")
        {
            string h = args.Length >= 2 ? args[1] : "127.0.0.1";
            int p = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int gw = args.Length >= 4 ? int.Parse(args[3]) : p - 1;
            Environment.ExitCode = SmokeCheck.Run(h, p, gw);
            return;
        }

        // --cook-check [host] [port เกม] [port gateway] = เทสระบบทำอาหาร (ต้องมีไฟ/เครื่องมือ · สุกดีกว่าดิบ)
        if (args.Length >= 1 && args[0] == "--cook-check")
        {
            string ch = args.Length >= 2 ? args[1] : "127.0.0.1";
            int cp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int cgw = args.Length >= 4 ? int.Parse(args[3]) : cp - 1;
            Environment.ExitCode = CookCheck.Run(ch, cp, cgw);
            return;
        }

        // --tool-check [host] [port เกม] [port gateway] = เทสความทนทานเครื่องมือ (สึก/พัง/ใช้ต่อไม่ได้)
        if (args.Length >= 1 && args[0] == "--tool-check")
        {
            string th = args.Length >= 2 ? args[1] : "127.0.0.1";
            int tp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int tgw = args.Length >= 4 ? int.Parse(args[3]) : tp - 1;
            Environment.ExitCode = ToolCheck.Run(th, tp, tgw);
            return;
        }

        // --multi-check [host] [port เกม] [port gateway] = เทส 3 คนออนพร้อมกัน + แย่งเก็บของจุดเดียวกัน
        if (args.Length >= 1 && args[0] == "--multi-check")
        {
            string mh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int mp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            int mgw = args.Length >= 4 ? int.Parse(args[3]) : mp - 1;
            Environment.ExitCode = MultiCheck.Run(mh, mp, mgw);
            return;
        }

        // --bot [host] [port] [นาที] [ชื่อ] [พอร์ต gateway] = AI farm bot (ไม่มี UI จึงไม่ติดบทสนทนา NPC)
        // พอร์ต gateway ไม่ใส่ = พอร์ตเกม - 1 (GP-12: ต้องขอ session token จาก gateway ก่อน Auth)
        if (args.Length >= 1 && args[0] == "--bot")
        {
            string bh = args.Length >= 2 ? args[1] : "127.0.0.1";
            int bp = args.Length >= 3 ? int.Parse(args[2]) : 8191;
            double mins = args.Length >= 4 ? double.Parse(args[3]) : 1.0;
            string bname = args.Length >= 5 ? args[4] : "farmbot-1";
            int bgw = args.Length >= 6 ? int.Parse(args[5]) : 0;
            FarmBot.Run(bh, bp, mins, bname, bname, bgw);
            return;
        }

        string host = "127.0.0.1";
        int port = 8191;
        if (args.Length >= 1) host = args[0];
        if (args.Length >= 2) port = int.Parse(args[1]);

        Console.WriteLine($"connecting to {host}:{port}...");
        using Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, port);
        Console.WriteLine("connected.");

        var conn = new Connection(sock);
        Reg<GetClock>(conn);
        Reg<Clock>(conn);
        Reg<Welcome>(conn);
        Reg<OK>(conn);
        Reg<Say>(conn);
        conn.Recv<Move>(OnMove);
        Reg<Info>(conn);
        Reg<Statistics>(conn);
        Reg<Chunk>(conn);
        Reg<Skills>(conn);
        conn.Recv<Inventory>(OnInventory);
        conn.Recv<PlayerDisplay>(OnPlayerDisplay);
        Reg<Equipments>(conn);
        Reg<DefoggedChunks>(conn);
        Reg<QuestCategories>(conn);
        Reg<WalletUpdated>(conn);
        Reg<AppearPlayer>(conn);
        Reg<Abort>(conn);
        conn.Recv<AppearArtifact>(OnArtifact);
        conn.Recv<AppearAnimal>(OnAnimal);
        conn.Recv<InventoryUpdated>(OnInventoryUpdated);
        conn.Recv<Survival>(OnSurvival);
        conn.Recv<SurvivalUpdated>(OnSurvivalUpdated);
        Reg<DisappearEntity>(conn);
        Reg<Recipes>(conn);
        Reg<ArtifactBlueprints>(conn);
        Reg<AvailableEmotions>(conn);
        Reg<PlayEmoticon>(conn);
        conn.StartReceive();

        Console.WriteLine("== 1. GetClock ==");
        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 800);

        string entityId = "test-client-1";
        // GP-12: server ไม่รับ token มั่ว ๆ แล้ว ต้องขอจาก gateway ก่อน (เหมือนตัวเกมจริง)
        string sessionToken = SessionClient.Fetch(host, port - 1, entityId, entityId);
        Console.WriteLine("== 2. Auth ==");
        conn.Send(new Auth
        {
            EntityId = entityId,
            SessionToken = sessionToken,
            ClientVersion = "5.2.1",
            DeviceModel = "PC"
        });
        Pump(conn, 800);

        Console.WriteLine("== 3. Ready ==");
        conn.Send(default(Ready));
        Pump(conn, 2500);

        Console.WriteLine("== 4. SetChunk (entry chunk 2,11) ==");
        conn.Send(new SetChunk { Chunk = new Point2(2, 11) });
        Pump(conn, 800);

        Console.WriteLine("== 5. GetStatistics ==");
        conn.Send(default(GetStatistics));
        Pump(conn, 800);

        Console.WriteLine("== 6. Move (broadcast check) ==");
        conn.Send(new Move
        {
            EntityId = entityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = "Barehand_Walk",
                    MotionOption = 1,
                    PlaybackRate = 1f,
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        new Location { Position = new WorldPosition(8040f, 35400f), Yaw = 0f, Time = Times.UnixTimeNow(), Floor = 0, Height = 0f }
                    }
                }
            }
        });
        Pump(conn, 800);

        Console.WriteLine("== 7. Cheat 'info' ==");
        conn.Send(new Cheat { _Cheat = "info" });
        Pump(conn, 800);

        Console.WriteLine("== 8. Cheat 'tp spawn' ==");
        conn.Send(new Cheat { _Cheat = "tp spawn" });
        Pump(conn, 800);

        Console.WriteLine("== 9. GetRecipes ==");
        conn.Send(default(GetRecipes));
        Pump(conn, 800);

        Console.WriteLine("== 10. GetArtifactBlueprints ==");
        conn.Send(default(GetArtifactBlueprints));
        Pump(conn, 800);

        Console.WriteLine("== 11. GetAvailableEmotions ==");
        conn.Send(default(GetAvailableEmotions));
        Pump(conn, 800);

        Console.WriteLine("== 12. LearnSkill gathering lv1 ==");
        conn.Send(new LearnSkill { SkillId = "gathering", SubId = "__base__", Level = 1 });
        Pump(conn, 800);

        Console.WriteLine("== 13. cheat 'add axe' ==");
        conn.Send(new Cheat { _Cheat = "add axe" });
        Pump(conn, 900);

        Console.WriteLine("== 14. cheat 'add clothes' ==");
        conn.Send(new Cheat { _Cheat = "add clothes" });
        Pump(conn, 900);

        if (_lastAxeItemId != null)
        {
            Console.WriteLine($"== 15. Equip ขวานเข้าช่อง main (item={_lastAxeItemId}) ==");
            conn.Send(new Equip { SlotName = "main", SlotType = Shared.Item.EquipSlotType.Slot1, ItemId = _lastAxeItemId, Action = "equip" });
            Pump(conn, 900);
        }
        else Console.WriteLine("!! ไม่ได้ item id ของขวาน");

        if (_lastClothesItemId != null)
        {
            Console.WriteLine($"== 16. Equip เสื้อเข้าช่อง body ==");
            conn.Send(new Equip { SlotName = "body", SlotType = Shared.Item.EquipSlotType.Slot1, ItemId = _lastClothesItemId, Action = "equip" });
            Pump(conn, 900);
        }

        Console.WriteLine("== 17. Unequip ขวาน ==");
        conn.Send(new Equip { SlotName = "main", SlotType = Shared.Item.EquipSlotType.Slot1, ItemId = null, Action = "unequip" });
        Pump(conn, 900);

        Console.WriteLine("== 18. Equip ของที่ไม่มีในกระเป๋า (ต้องโดนปฏิเสธ) ==");
        conn.Send(new Equip { SlotName = "main", SlotType = Shared.Item.EquipSlotType.Slot1, ItemId = "ของปลอม-ไม่มีจริง", Action = "equip" });
        Pump(conn, 900);

        Console.WriteLine("== 19. cheat 'survival' (ค่าเริ่มต้น) ==");
        conn.Send(new Cheat { _Cheat = "survival" });
        Pump(conn, 800);

        Console.WriteLine("== 20. Touch + Collect (ควรหักสตามินา 6) ==");
        conn.Send(new Touch { EntityId = "natural_41_177", EntityType = 10001, Tile = new Point2(41, 177) });
        Pump(conn, 800);
        conn.Send(new Collect { EntityId = "natural_41_177", GeneratorId = "leaf", Tile = new Point2(41, 177) });
        Pump(conn, 1200);
        conn.Send(new Cheat { _Cheat = "survival" });
        Pump(conn, 800);

        Console.WriteLine("== 21. cheat 'tired' แล้วลองเก็บของ (ควรโดน Abort) ==");
        conn.Send(new Cheat { _Cheat = "tired" });
        Pump(conn, 150);
        conn.Send(new Collect { EntityId = "natural_41_177", GeneratorId = "leaf", Tile = new Point2(41, 177) });
        Pump(conn, 1200);

        Console.WriteLine("== 22. cheat 'hurt' (ลดเลือด 30) ==");
        conn.Send(new Cheat { _Cheat = "hurt" });
        Pump(conn, 800);

        Console.WriteLine("== 23. cheat 'rest' (ฟื้นทุกอย่าง) ==");
        conn.Send(new Cheat { _Cheat = "rest" });
        Pump(conn, 800);

        Console.WriteLine("== 24. cheat 'add box' + วางกล่องลงพื้น ==");
        conn.Send(new Cheat { _Cheat = "add box" });
        Pump(conn, 900);
        if (_lastBoxCapsuleId != null)
        {
            conn.Send(new PlaceCapsulatedArtifact { ItemId = _lastBoxCapsuleId, Tile = new Point2(42, 177) });
            Pump(conn, 1200);
        }
        else Console.WriteLine("!! ไม่ได้ capsule ของกล่อง");

        Console.WriteLine($"== 25. ดูของในกล่อง {_placedBoxId} (ควรว่าง) ==");
        conn.Send(new GetInventory { Target = new PropKey { EntityId = _placedBoxId, Tile = new Point2(42, 177) } });
        Pump(conn, 900);

        Console.WriteLine("== 26. ใส่ใบไม้ลงกล่อง ==");
        if (_lastLeafItemId != null)
        {
            conn.Send(new PutInItem { EntityId = _placedBoxId, Tile = new Point2(42, 177), ItemIds = new[] { _lastLeafItemId } });
            Pump(conn, 1000);
            conn.Send(new GetInventory { Target = new PropKey { EntityId = _placedBoxId, Tile = new Point2(42, 177) } });
            Pump(conn, 900);
        }
        else Console.WriteLine("!! ไม่มีใบไม้ในกระเป๋า");

        Console.WriteLine("== 27. หยิบใบไม้กลับ ==");
        if (_lastLeafItemId != null)
        {
            conn.Send(new TakeOutItem { EntityId = _placedBoxId, Tile = new Point2(42, 177), ItemIds = new[] { _lastLeafItemId } });
            Pump(conn, 1000);
        }

        Console.WriteLine("== 28. ใส่ของลงสิ่งที่ไม่ใช่กล่อง (ควรโดน Abort) ==");
        conn.Send(new PutInItem { EntityId = "ไม่มีจริง", Tile = new Point2(0, 0), ItemIds = new[] { _lastLeafItemId ?? "x" } });
        Pump(conn, 900);

        Console.WriteLine($"== 29. รอดูสัตว์เดิน 20 วินาที (มีสัตว์ {_animalIds.Count} ตัว) ==");
        Pump(conn, 20000);
        Console.WriteLine($"   สัตว์เดินไปทั้งหมด {_animalMoves} ครั้ง");

        Console.WriteLine($"done. total messages received: {_receivedCount}");
        conn.Close();
    }

    private static void Pump(Connection conn, int ms)
    {
        int steps = ms / 10;
        for (int i = 0; i < steps; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }
}
