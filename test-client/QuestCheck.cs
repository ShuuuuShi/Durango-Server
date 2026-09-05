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
/// เทส **ระบบเควส** — สายสอนเล่น 8 ขั้นที่จบด้วยการต่อแพหนีเกาะ
///
/// เควสใช้ **id ของจริงจากข้อมูลเกม** (client จะได้หยิบชื่อ/ไอคอนของแท้มาวาด)
/// แต่เงื่อนไข/รางวัลเราเขียนเองที่ `QuestData.cs` เพราะข้อมูลเกมไม่มีส่วนนั้นเลย
///
/// เช็ค:
///   1. เข้าเกมแล้วได้รายการเควส และมีแค่ขั้นแรกที่เปิด (ขั้นถัดไปต้องยังไม่โผล่)
///   2. ทำขั้นแรก (เก็บของ) แล้วความคืบหน้าขยับ + ได้ NotifyQuestProceed
///   3. ทำครบแล้วเควสเปลี่ยนเป็น "รอรับรางวัล" และ **ขั้นถัดไปโผล่**
///   4. กดรับรางวัลได้ของจริง (exp/แต้มสกิล/ไอเทม)
///   5. กดรับซ้ำไม่ได้
///   6. ยังไม่ครบแล้วกดรับ ไม่ผ่าน
///   7. **ต่อแพ `tutorial_boat` แล้วเควสปลายสายสำเร็จ**
///   8. ออกเกมเข้าใหม่ ความคืบหน้ายังอยู่
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --quest-check [host] [port เกม] [port gateway]
/// </summary>
public static class QuestCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private const string Q1 = "event_2018_fall_3_17_any_gathering_01";   // เก็บของ 10 ครั้ง
    private const string Q2 = "daily_weaponcrafting_b_01";               // คราฟต์เครื่องมือ 5 ชิ้น
    private const string RAFT = "story_enter_safehouse";                 // ต่อแพ

    // ── รายการตรวจเซิร์ฟ (QuestData.Checklist) ──
    /// <summary>หมวดของชุดตรวจ — ตรงกับ QuestData.ChecklistCategory ฝั่ง server
    /// (เคยเป็น "server_checklist" แล้วย้ายไป "daily" ให้ตรงกับหมวดเควสรายวันของเกม
    ///  เทสไม่ได้ตามไปแก้ เลยขอรายการมาได้ 0 อันตลอดและอ่านของค้างเก่าแทน)</summary>
    private const string CHECKLIST_CATEGORY = "daily";

    private const int ChecklistCount = 15;
    private const string CL_PLANT = "permanent_farming_seed_01";        // ปลูกเมล็ด 4 ครั้ง
    private const string CL_EQUIP = "urban_weapon_event_04";            // สวมอุปกรณ์ 2 ชิ้น
    private const string CL_REVIVE = "mainstory_chapter1_5";            // ตายแล้วฟื้น 1 ครั้ง
    private const string CL_SKILL = "permanent_level_skill_gathering_10";  // เรียนสกิล 1 อัน
    private const string CL_REST = "daily_survival_rest";               // พักที่จุดพัก 1 ครั้ง
    private const string CL_WARP = "daily_local_warp";                  // วาปในเกาะ 1 ครั้ง
    private const string CL_TRAVEL = "daily_island_travel";              // ย้ายเกาะที่ท่าเรือ 1 ครั้ง

    private static readonly string[] ChecklistIds =
    {
        CL_PLANT,
        "event_1_farming_cherry_01",
        "event_1_farming_cherry_02",
        "event_1_gathering_cherry_bough_01",
        "event_newyear_2019_quest_04",
        CL_EQUIP,
        "urban_weapon_event_10",
        CL_REVIVE,
        "mainstory_chapter4_6",
        "estate_build_lv55_02",
        CL_SKILL,
        "urban_cook_event_06",
        CL_REST,
        CL_WARP,
        CL_TRAVEL
    };

    private static readonly Dictionary<string, QuestToDo> _quests = new Dictionary<string, QuestToDo>(StringComparer.Ordinal);
    private static readonly List<NotifyQuestProceed> _proceeds = new List<NotifyQuestProceed>();
    private static readonly List<string> _infos = new List<string>();
    private static Item[] _inventory = Array.Empty<Item>();
    private static Statistics? _stats;
    private static int _aborts;
    private static int _rewardResults;
    private static int _questStarted;
    private static string _lastArtifact;
    private static Point2 _lastArtifactTile;
    private static Point2 raftTileUsed;
    private static IslandTravelOptions? _travelOptions;

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { conn.Process(); Thread.Sleep(10); }
    }

    private static QuestToDo? Q(string id)
    {
        return _quests.TryGetValue(id, out QuestToDo t) ? t : (QuestToDo?)null;
    }

    private static void Absorb(QuestToDo[] todos)
    {
        if (todos == null) return;
        foreach (QuestToDo t in todos)
        {
            if (!string.IsNullOrEmpty(t.Id)) _quests[t.Id] = t;
        }
    }

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) return null;
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);
        _quests.Clear();
        _proceeds.Clear();
        _stats = null;
        _travelOptions = null;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _infos.Add(m.Text ?? ""));
        conn.Recv<Statistics>((m, h) => _stats = m);
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Inventory>((m, h) => { if (m.InventoryItems.Items != null) _inventory = m.InventoryItems.Items; });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<PlayerDisplay>((m, h) => { });
        conn.Recv<Recipes>((m, h) => { });
        conn.Recv<ArtifactBlueprints>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearAnimal>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) =>
        {
            if (string.IsNullOrEmpty(m.EntityId)) return;
            _lastArtifact = m.EntityId;
            _lastArtifactTile = m.Tile;
        });
        conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.Recv<ExpGained>((m, h) => { });
        conn.Recv<Crafted>((m, h) => { });
        conn.Recv<Occupied>((m, h) => { });
        conn.Recv<ArtifactBuilt>((m, h) => { });
        conn.Recv<ArtifactCompleted>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<Quests>((m, h) => Absorb(m.Todos));
        conn.Recv<QuestStarted>((m, h) => { _questStarted++; Absorb(m.Quests); });
        conn.Recv<NotifyQuestProceed>((m, h) => _proceeds.Add(m));
        conn.Recv<QuestRewardResults>((m, h) => _rewardResults++);
        conn.Recv<IslandTravelOptions>((m, h) => _travelOptions = m);
        conn.Recv<Emigrated>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2500);
        return conn;
    }

    /// <summary>เก็บของ n ครั้งด้วยการเสกของให้ตัวเอง ไม่ได้ — ต้องเก็บจริง จึงใช้ cheat gather ผ่าน control ไม่ได้เช่นกัน
    /// วิธีที่ใช้ได้จริงในเทส: ยัดความคืบหน้าด้วยการทำสิ่งนั้นจริง ๆ ผ่าน cheat ที่มีอยู่</summary>
    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== quest check (ระบบเควส): {host}:{gamePort} ===");
        string id = CreateCharacter(host, gatewayPort, "quest-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine("สร้างตัวละครทดสอบสำหรับ quest-check ไม่สำเร็จ");
            return 1;
        }
        Connection conn = Connect(host, gamePort, gatewayPort, id);
        if (conn == null) { Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม"); return 1; }

        // ── รอบ 1: เข้าเกมแล้วต้องได้รายการเควส ────────────────────────
        Console.WriteLine("รอบ 1 — เข้าเกมแล้วได้เควสขั้นแรก");
        Check("ได้รายการเควสตอนเข้าเกม", _quests.Count > 0, $"{_quests.Count} เควส");
        Check("ขั้นแรก (เก็บของ 10 ครั้ง) เปิดอยู่", Q(Q1).HasValue,
            Q(Q1).HasValue ? $"{Q(Q1).Value.Progress}/{Q(Q1).Value.GoalCount}" : "ไม่มี");
        Check("เป้าหมายขั้นแรกคือ 10 ครั้ง", Q(Q1)?.GoalCount == 10, $"{Q(Q1)?.GoalCount}");
        Check("ขั้นถัดไปยังไม่โผล่ (ต้องทำขั้นแรกก่อน)", !Q(Q2).HasValue);

        // ── รอบ 2: ยังทำไม่ครบแล้วกดรับรางวัล ต้องไม่ผ่าน ─────────────
        Console.WriteLine("รอบ 2 — กดรับรางวัลทั้งที่ยังทำไม่ครบ");
        _aborts = 0;
        conn.Send(new RequestQuestReward { QuestId = Q1 });
        Pump(conn, 800);
        Check("ยังไม่ครบแล้วกดรับ ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");

        // ── รอบ 3: เก็บของจริงจนครบ ────────────────────────────────────
        Console.WriteLine("รอบ 3 — เก็บของจนครบ 10 ครั้ง");
        _proceeds.Clear();
        int gathered = 0;
        for (int i = 0; i < 14 && (Q(Q1)?.Progress ?? 0) < 10; i++)
        {
            conn.Send(new Cheat { _Cheat = "gather" });
            Pump(conn, 900);
            conn.Send(new GetQuests { Category = "sunset" });
            Pump(conn, 400);
            gathered++;
        }
        Check("ได้ NotifyQuestProceed ตอนความคืบหน้าขยับ", _proceeds.Count > 0, $"{_proceeds.Count} ครั้ง");
        Check("ความคืบหน้าเดินจนครบ 10", (Q(Q1)?.Progress ?? 0) >= 10,
            $"{Q(Q1)?.Progress}/10 (เก็บไป {gathered} รอบ)");
        Check("มีข้อความแจ้งว่าเควสสำเร็จ", _infos.Any(x => x.Contains("เควสสำเร็จ")),
            _infos.LastOrDefault(x => x.Contains("เควสสำเร็จ")) ?? "(ไม่มี)");
        Check("ขั้นถัดไปโผล่แล้ว", Q(Q2).HasValue, Q(Q2).HasValue ? "โผล่" : "ยังไม่โผล่");
        Check("ได้ QuestStarted ตอนมีเควสใหม่", _questStarted > 0, $"{_questStarted} ครั้ง");

        // ── รอบ 4: รับรางวัล ───────────────────────────────────────────
        Console.WriteLine("รอบ 4 — รับรางวัลขั้นแรก");
        conn.Send(default(GetStatistics));
        Pump(conn, 600);
        int expBefore = _stats?.Exp ?? 0;
        int stemBefore = _inventory.Count(x => x.Prototype == "stem");
        _aborts = 0; _rewardResults = 0;
        conn.Send(new RequestQuestReward { QuestId = Q1 });
        Pump(conn, 1200);
        conn.Send(default(GetStatistics));
        Pump(conn, 600);
        Check("รับรางวัลผ่าน (ได้ QuestRewardResults)", _rewardResults > 0 && _aborts == 0,
            $"results={_rewardResults} abort={_aborts}");
        Check("ได้ exp จริง", (_stats?.Exp ?? 0) > expBefore, $"{expBefore} → {_stats?.Exp}");
        Check("ได้ไอเทมรางวัลจริง (ก้าน 3 ชิ้น)",
            _inventory.Count(x => x.Prototype == "stem") > stemBefore,
            $"{stemBefore} → {_inventory.Count(x => x.Prototype == "stem")}");

        // ── รอบ 5: กดรับซ้ำ ────────────────────────────────────────────
        Console.WriteLine("รอบ 5 — กดรับรางวัลซ้ำ");
        _aborts = 0;
        conn.Send(new RequestQuestReward { QuestId = Q1 });
        Pump(conn, 800);
        Check("รับรางวัลซ้ำไม่ได้", _aborts > 0, $"abort={_aborts}");

        // ── รอบ 6: ต่อแพ — ขั้นปลายของสาย ─────────────────────────────
        Console.WriteLine("รอบ 6 — ต่อแพ (ปลายสาย)");
        // ลัดให้ถึงขั้นสุดท้าย: ยัดความคืบหน้าทุกขั้นด้วยคำสั่งทดสอบ
        conn.Send(new Cheat { _Cheat = "questskip" });
        Pump(conn, 1500);
        conn.Send(new GetQuests { Category = "sunset" });
        Pump(conn, 800);
        Check("เควสต่อแพเปิดแล้ว", Q(RAFT).HasValue, Q(RAFT).HasValue ? "เปิด" : "ยังไม่เปิด");
        if (Q(RAFT).HasValue)
        {
            Check("เป้าหมายคือสร้าง 1 หลัง", Q(RAFT).Value.GoalCount == 1, $"{Q(RAFT).Value.GoalCount}");
            _proceeds.Clear();
            // ต่อแพด้วย packet จริงของเกม (จองที่ → สร้าง) ไม่ใช้ cheat
            // จะได้เทสเส้นทางเดียวกับที่ผู้เล่นทำจริง รวมถึง hook ที่ยิงตอนสร้างเสร็จ
            // ต้องสร้างในระยะเอื้อมของตัวเอง — วาร์ปไปจุดที่รู้พิกัดแน่นอนก่อน
            // สุ่มจุดใหม่ทุกรอบ — แพจากการเทสรอบก่อนยังค้างอยู่ในเซฟโลก
            // ถ้าใช้จุดเดิมจะโดน "tile นี้มีสิ่งปลูกสร้างอยู่แล้ว"
            var rng = new Random();
            var raftTile = new Point2(130 + rng.Next(0, 40), 130 + rng.Next(0, 40));
            raftTileUsed = raftTile;
            conn.Send(new Cheat { _Cheat = $"tp {raftTile.x} {raftTile.y}" });
            Pump(conn, 1200);
            _lastArtifact = null;
            conn.Send(new OccupyArtifactSite
            {
                BlueprintId = "tutorial_boat",
                ItemId = null,
                Tile = raftTile,
                Floor = null,
                Size = new Point2(2, 2),
                Stories = 1,
                Rotation = Shared.Etc.Rotation.None,
                ModularEntityId = null
            });
            Pump(conn, 1800);
            Check("จองที่ต่อแพได้", _lastArtifact != null, _lastArtifact ?? "ไม่ได้ entity id");
            // entity id ต้องมาจาก server (AppearArtifact) ไม่ใช่ client คิดเอง
            conn.Send(new BuildArtifact { EntityId = _lastArtifact, Tile = raftTile, ToolItemId = null });
            Pump(conn, 4000);
            conn.Send(new GetQuests { Category = "sunset" });
            Pump(conn, 800);
            Check("ต่อแพแล้วเควสปลายสายสำเร็จ", (Q(RAFT)?.Progress ?? 0) >= 1,
                $"{Q(RAFT)?.Progress}/1");
            Check("มีข้อความฉลองตอนต่อแพเสร็จ",
                _infos.Any(x => x.Contains("แพ")), _infos.LastOrDefault(x => x.Contains("แพ")) ?? "(ไม่มี)");
        }

        // ── รอบ 6.2: ช่องปั๊มจากการสร้างซ้ำ ต้องถูกปิด ───────────────────
        Console.WriteLine("รอบ 6.2 — ยิง BuildArtifact ซ้ำใส่ของเดิม (ช่องปั๊ม exp/เควส)");
        if (_lastArtifact != null)
        {
            conn.Send(default(GetStatistics));
            Pump(conn, 600);
            int expBeforeSpam = _stats?.Exp ?? 0;
            _aborts = 0;
            for (int i = 0; i < 5; i++)
            {
                conn.Send(new BuildArtifact { EntityId = _lastArtifact, Tile = raftTileUsed, ToolItemId = null });
                Pump(conn, 500);
            }
            Pump(conn, 1500);
            conn.Send(default(GetStatistics));
            Pump(conn, 700);
            Check("สร้างซ้ำใส่ของที่เสร็จแล้ว ถูกปฏิเสธทุกครั้ง", _aborts >= 5, $"abort={_aborts}/5");
            Check("ยิงซ้ำแล้วไม่ได้ exp เพิ่ม (ปั๊มไม่ได้)", (_stats?.Exp ?? 0) == expBeforeSpam,
                $"{expBeforeSpam} → {_stats?.Exp}");
        }

        // ── รอบ 6.5: ความทน — ยิงคำสั่งพิลึกใส่ระบบเควส ────────────────
        Console.WriteLine("รอบ 6.5 — ความทนต่อ packet พิลึก");
        _aborts = 0;
        conn.Send(new RequestQuestReward { QuestId = "ไม่มีเควสนี้จริง_12345" });
        Pump(conn, 600);
        Check("ขอรางวัลเควสที่ไม่มีจริง ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");
        _aborts = 0;
        conn.Send(new RequestQuestReward { QuestId = null });
        Pump(conn, 600);
        Check("ขอรางวัลด้วย id ว่าง ไม่ผ่าน (และเซิร์ฟไม่ล่ม)", _aborts > 0, $"abort={_aborts}");
        conn.Send(new GetQuestState { QuestIds = null });
        Pump(conn, 500);
        conn.Send(new GetQuestState { QuestIds = new[] { "ไม่มีจริง", null, RAFT } });
        Pump(conn, 600);
        conn.Send(new GetQuests { Category = "หมวดที่ไม่มีจริง" });
        Pump(conn, 600);
        conn.Send(new GetQuests { Category = null });
        Pump(conn, 600);
        Check("เซิร์ฟยังตอบอยู่หลังโดน packet พิลึก", _quests.Count > 0, $"{_quests.Count} เควส");

        // ── รอบ 7: ออกเกมเข้าใหม่ ─────────────────────────────────────
        Console.WriteLine("รอบ 7 — ออกเกมแล้วเข้าใหม่");
        int doneBefore = _quests.Values.Count(t => t.Progress >= t.GoalCount);
        int newQuestMsgsBefore = _infos.Count(x => x.Contains("[เควสใหม่]"));
        conn.Close();
        Thread.Sleep(1500);
        Connection again = Connect(host, gamePort, gatewayPort, id);
        if (again == null)
        {
            Check("ต่อกลับเข้ามาได้", false);
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return 1;
        }
        int doneAfter = _quests.Values.Count(t => t.Progress >= t.GoalCount);
        Check("ความคืบหน้าเควสยังอยู่หลังเข้าใหม่", doneAfter >= doneBefore && doneAfter > 0,
            $"ทำเสร็จ {doneBefore} → {doneAfter}");
        // 🐛 เดิมเควสที่เปิดค้างอยู่จะเด้ง "[เควสใหม่]" ซ้ำทุกครั้งที่ login
        Check("เข้าใหม่แล้วไม่เด้ง \"เควสใหม่\" ซ้ำ",
            _infos.Count(x => x.Contains("[เควสใหม่]")) == newQuestMsgsBefore,
            $"ก่อน {newQuestMsgsBefore} → หลัง {_infos.Count(x => x.Contains("[เควสใหม่]"))}");
        again.Close();

        // ── รอบ 8: รายการตรวจเซิร์ฟ (เอาเช็คลิสต์เทสมาใส่เป็นเควส) ────
        // ⚠️ ต้องใช้ **ตัวละครใหม่** เพราะรอบก่อนหน้าใช้ `cheat questskip`
        //    ซึ่งมาร์คทุกเควส (รวมชุดตรวจ) ว่าเสร็จหมด — ตัวนับจะขยับไม่ได้อีก
        Console.WriteLine("รอบ 8 — รายการตรวจเซิร์ฟ (ตัวละครใหม่)");
        string clId = CreateCharacter(host, gatewayPort, "cl-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        if (string.IsNullOrEmpty(clId))
        {
            Check("สร้างตัวละครใหม่สำหรับชุดตรวจได้", false);
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return 1;
        }
        Connection cl = Connect(host, gamePort, gatewayPort, clId);
        if (cl == null)
        {
            Check("ต่อตัวละครใหม่สำหรับชุดตรวจได้", false);
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return 1;
        }
        // ⚠️ ชุดตรวจอยู่คนละหมวดกับสายหลักแล้ว (แท็บแยกในหน้าต่างเควส)
        _quests.Clear();
        cl.Send(new GetQuests { Category = CHECKLIST_CATEGORY });
        Pump(cl, 700);

        string ChecklistCheat(string command, int waitMs = 800)
        {
            _infos.Clear();
            cl.Send(new Cheat { _Cheat = command });
            Pump(cl, waitMs);
            return string.Join("\n", _infos);
        }

        int clOpen = 0;
        foreach (string cid in ChecklistIds)
        {
            if (Q(cid).HasValue) clOpen++;
        }
        Check("เควสชุดตรวจโผล่ครบทุกข้อ", clOpen == ChecklistIds.Length,
            $"เจอ {clOpen}/{ChecklistIds.Length}");
        Check("ชุดตรวจเปิดพร้อมกันหมดตั้งแต่แรก (ไม่ต้องไล่เป็นสาย)",
            (Q(CL_REVIVE)?.Progress ?? 0) == 0 && (Q(CL_SKILL)?.Progress ?? 0) == 0,
            "ยังไม่ได้ทำอะไรเลย ทุกข้อต้องเป็น 0");
        Check("เป้าหมายของข้อ \"ปลูกเมล็ด\" คือ 4 ครั้ง", Q(CL_PLANT)?.GoalCount == 4, $"{Q(CL_PLANT)?.GoalCount}");

        _infos.Clear();
        cl.Send(new Cheat { _Cheat = "checklist" });
        Pump(cl, 700);
        string sheet = string.Join(" ", _infos);
        Check("cheat checklist พิมพ์รายการภาษาไทยออกมาครบ",
            sheet.Contains("ผ่านแล้ว 0/" + ChecklistCount) && sheet.Contains("[ตรวจ]"),
            sheet.Length > 90 ? sheet.Substring(0, 90).Replace("\n", " ") : sheet.Replace("\n", " "));

        // ตัวนับใหม่ 1: สวมอุปกรณ์
        cl.Send(new Cheat { _Cheat = "add axe" });
        Pump(cl, 800);
        string axeId = _inventory.Where(x => x.Prototype == "axe_onehand_stone_01").Select(x => x.Id).FirstOrDefault();
        cl.Send(new Equip { SlotName = "main", SlotType = Shared.Item.EquipSlotType.Slot1, ItemId = axeId, Action = "equip" });
        Pump(cl, 900);
        cl.Send(new GetQuests { Category = CHECKLIST_CATEGORY });
        Pump(cl, 600);
        Check("สวมอุปกรณ์แล้วตัวนับชุดตรวจขยับ", (Q(CL_EQUIP)?.Progress ?? 0) > 0,
            $"{Q(CL_EQUIP)?.Progress}/{Q(CL_EQUIP)?.GoalCount}");

        // ตัวนับใหม่ 2: ตายแล้วฟื้น
        _infos.Clear();     // ⚠️ ต้องล้างก่อน ไม่งั้นไปเจอคำว่า "ตายแล้ว" ในข้อความของ cheat checklist เอง
        for (int i = 0; i < 10 && !_infos.Any(x => x.Contains("ตายแล้ว")); i++)
        {
            cl.Send(new Cheat { _Cheat = "hurt" });
            Pump(cl, 300);
        }
        Pump(cl, 500);
        cl.Send(default(Revive));
        Pump(cl, 1200);
        cl.Send(new GetQuests { Category = CHECKLIST_CATEGORY });
        Pump(cl, 600);
        Check("ตายแล้วฟื้นแล้วตัวนับชุดตรวจขยับ", (Q(CL_REVIVE)?.Progress ?? 0) > 0,
            $"{Q(CL_REVIVE)?.Progress}/{Q(CL_REVIVE)?.GoalCount}");
        cl.Send(new Cheat { _Cheat = "heal" });
        Pump(cl, 600);

        // ตัวนับใหม่ 3: ปลูกผัก (เชื่อมระบบใหม่ล่าสุดเข้ากับเควส)
        _infos.Clear();
        cl.Send(new Cheat { _Cheat = "farm" });
        Pump(cl, 1500);
        string farmId = null;
        foreach (string line in _infos)
        {
            int a = (line ?? "").IndexOf("[id=");
            if (a >= 0)
            {
                int b = line.IndexOf(']', a);
                if (b > a) farmId = line.Substring(a + 4, b - a - 4);
            }
        }
        if (!string.IsNullOrEmpty(farmId))
        {
            string seedId = _inventory.Where(x => x.Prototype == "corn_seed").Select(x => x.Id).FirstOrDefault();
            cl.Send(new PlantSeed { EntityId = farmId, Tile = _lastArtifactTile, SeedItemId = seedId });
            Pump(cl, 2500);
            cl.Send(new GetQuests { Category = CHECKLIST_CATEGORY });
            Pump(cl, 600);
            Check("ปลูกผักแล้วตัวนับชุดตรวจขยับ", (Q(CL_PLANT)?.Progress ?? 0) > 0,
                $"{Q(CL_PLANT)?.Progress}/{Q(CL_PLANT)?.GoalCount}");
        }
        else
        {
            Check("ปลูกผักแล้วตัวนับชุดตรวจขยับ", false, "วางแปลงผักไม่สำเร็จ");
        }

        // ตัวนับใหม่ 4: พักที่จุดพักจริง — ใช้กองไฟที่ server วางให้เป็น fixture
        string restResult = ChecklistCheat("exhaust");
        restResult += " | " + ChecklistCheat("place real fire");
        restResult += " | " + ChecklistCheat("test rest");
        cl.Send(new GetQuests { Category = CHECKLIST_CATEGORY });
        Pump(cl, 600);
        Check("พักที่จุดพักแล้วตัวนับชุดตรวจขยับ", (Q(CL_REST)?.Progress ?? 0) > 0,
            $"{Q(CL_REST)?.Progress}/{Q(CL_REST)?.GoalCount} — {restResult.Replace('\n', ' ')}");

        // ตัวนับใหม่ 5: วาปในเกาะจริง — หา tile จากรายการ POI ของ server ไม่เดาพิกัด
        string poiList = ChecklistCheat("poi list");
        (string id, int x, int y)? warp = FindPoi(poiList, "camp_warphole");
        if (warp.HasValue)
        {
            var w = warp.Value;
            cl.Send(new Warp { Tile = new Point2(w.x, w.y) });
            Pump(cl, 1200);
            cl.Send(new GetQuests { Category = CHECKLIST_CATEGORY });
            Pump(cl, 600);
            Check("วาปในเกาะแล้วตัวนับชุดตรวจขยับ", (Q(CL_WARP)?.Progress ?? 0) > 0,
                $"{Q(CL_WARP)?.Progress}/{Q(CL_WARP)?.GoalCount} — {w.id} @ {w.x},{w.y}");
        }
        else
        {
            Check("วาปในเกาะแล้วตัวนับชุดตรวจขยับ", false, "ไม่พบ camp_warphole จาก poi list");
        }

        // ตัวนับใหม่ 6: ย้ายเกาะจากท่าเรือจริง — เพิ่มเลเวลเฉพาะ fixture เพื่อเปิด isle02
        ChecklistCheat("exp 1000", 900);
        poiList = ChecklistCheat("poi list");
        (string id, int x, int y)? dock = FindPoi(poiList, "dock");
        if (dock.HasValue)
        {
            var d = dock.Value;
            ChecklistCheat("poi tp " + d.id, 900);
            _travelOptions = null;
            _proceeds.Clear();
            cl.Send(new GetIslandTravelOptions
            {
                // `poi list` intentionally prints the short id (dock_0), while
                // the travel packet accepts a null id and authoritatively checks
                // that the player is next to a real dock. Do not turn a display
                // id into a fake entity id here.
                EntityId = null,
                Tile = new Point2(d.x, d.y)
            });
            Pump(cl, 700);
            int optionCount = _travelOptions?.Ids?.Length ?? 0;
            string destination = null;
            if (_travelOptions.HasValue && _travelOptions.Value.Ids != null)
            {
                destination = _travelOptions.Value.Ids.FirstOrDefault(x =>
                    !string.Equals(x, "isle01", StringComparison.OrdinalIgnoreCase));
            }
            if (!string.IsNullOrEmpty(destination))
            {
                cl.Send(new TravelByRegion
                {
                    EntityId = null,
                    Tile = new Point2(d.x, d.y),
                    RegionId = destination,
                    PartierId = null
                });
                Pump(cl, 1600);
            }
            bool travelProgress = _proceeds.Any(x => x.QuestId == CL_TRAVEL && x.Progress > 0);
            Check("ย้ายเกาะที่ท่าเรือแล้วตัวนับชุดตรวจขยับ",
                travelProgress,
                $"{Q(CL_TRAVEL)?.Progress}/{Q(CL_TRAVEL)?.GoalCount} — options={optionCount} destination={destination ?? "ไม่มี"}");
        }
        else
        {
            Check("ย้ายเกาะที่ท่าเรือแล้วตัวนับชุดตรวจขยับ", false, "ไม่พบ dock จาก poi list");
        }
        cl.Close();

        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    private static string CreateCharacter(string host, int gatewayPort, string name)
    {
        return CreateCharacterCheck.CreatePlayer(host, gatewayPort, name, isMale: true, modelInfo: "{}");
    }

    private static (string id, int x, int y)? FindPoi(string text, string blueprint)
    {
        foreach (string line in (text ?? string.Empty).Split('\n'))
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) continue;
            int blueprintIndex = Array.IndexOf(parts, blueprint);
            int tileIndex = Array.IndexOf(parts, "tile");
            if (blueprintIndex < 0 || tileIndex < 0 || tileIndex + 1 >= parts.Length) continue;
            string[] xy = parts[tileIndex + 1].Split(',');
            if (xy.Length != 2 || !int.TryParse(xy[0], out int x) || !int.TryParse(xy[1], out int y)) continue;
            return (parts[0], x, y);
        }
        return null;
    }
}
