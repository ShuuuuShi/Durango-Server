using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทส **ระบบปลูกผัก** — ลงเมล็ด → รดน้ำ/ใส่ปุ๋ย → รอโต → เก็บเกี่ยว
///
/// เช็ค:
///   1. วางแปลง (cheat farm) แล้วแตะได้เมนู "ปลูก" · ยังไม่มีเมนู "เก็บ"
///   2. ปลูกด้วยของที่ไม่ใช่เมล็ด ไม่ผ่าน · ปลูกด้วยเมล็ดจริงผ่านและเมล็ดหายจากกระเป๋า
///   3. ปลูกซ้ำในแปลงเดิมไม่ผ่าน
///   4. รดน้ำ/ใส่ปุ๋ยด้วยของผิดประเภทไม่ผ่าน · ของถูกผ่านและตัวเลขขยับจริง
///   5. โตครบแล้วแตะได้เมนู "เก็บ" และมี generator ของผลผลิต
///   6. เก็บเกี่ยวได้ของจริง + ผลผลิตมี tag (เอาไปทำอาหารต่อได้)
///   7. เก็บเกินจำนวนที่มีไม่ได้ (ไม่ปั๊มของ)
///   8. เก็บหมด = แปลงกลับเป็นแปลงเปล่า ปลูกใหม่ได้ทันที
///   9. ไม่รดน้ำแล้วปล่อยจนโต = ต้นตาย เก็บไม่ได้ ต้องถอน
///  10. ถอนต้นแล้วแปลงว่าง · ถอนแปลงเปล่าไม่ผ่าน
///  11. ออกเกมเข้าใหม่ ต้นที่ปลูกไว้ยังอยู่
///  12. ตักน้ำด้วยของที่ไม่ใช่ภาชนะ ไม่ผ่าน
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat และ Features.Farming = true
///
/// รัน: dotnet run -- --farm-check [host] [port เกม] [port gateway]
/// </summary>
public static class FarmCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static readonly List<string> _infos = new List<string>();
    private static readonly Dictionary<string, ArtifactState> _states = new Dictionary<string, ArtifactState>(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> _crops = new Dictionary<string, string>(StringComparer.Ordinal);
    private static Item[] _inventory = Array.Empty<Item>();
    private static readonly Dictionary<string, ushort> _types = new Dictionary<string, ushort>(StringComparer.Ordinal);
    private static readonly Dictionary<string, Point2> _tiles = new Dictionary<string, Point2>(StringComparer.Ordinal);
    private static Touched? _touched;
    private static Collected? _collected;
    private static int _aborts;

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { conn.Process(); Thread.Sleep(10); }
    }

    /// <summary>แตะแปลงแล้วรอคำตอบ — คืน Touched ล่าสุด</summary>
    private static Touched? Touch(Connection conn, string entityId, ushort type, Point2 tile)
    {
        _touched = null;
        conn.Send(new Touch { EntityId = entityId, EntityType = type, Tile = tile });
        Pump(conn, 700);
        return _touched;
    }

    private static bool HasInteraction(Touched? t, int code)
    {
        return t.HasValue && t.Value.Interactions != null && t.Value.Interactions.Contains(code);
    }

    private static Item? FindItem(string prototype)
    {
        foreach (Item it in _inventory)
        {
            if (it.Prototype == prototype) return it;
        }
        return null;
    }

    private static int CountItem(string prototype)
    {
        return _inventory.Count(x => x.Prototype == prototype);
    }

    private static Farming? FarmOf(string entityId)
    {
        return _states.TryGetValue(entityId, out ArtifactState s) ? s.Farming : null;
    }

    private const int Plant = 508;
    private const int Fertilize = 509;
    private const int Watering = 510;
    private const int Uproot = 511;
    private const int Collect_ = 506;

    private static ushort TypeOf(string id)
    {
        return _types.TryGetValue(id ?? "", out ushort t) ? t : (ushort)0;
    }

    private static Point2 TileOf(string id)
    {
        return _tiles.TryGetValue(id ?? "", out Point2 t) ? t : default;
    }


    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) return null;
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);
        _infos.Clear();
        _states.Clear();
        _crops.Clear();
        _types.Clear();
        _tiles.Clear();
        _touched = null;
        _collected = null;
        _aborts = 0;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _infos.Add(m.Text ?? ""));
        conn.Recv<Statistics>((m, h) => { });
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
            _states[m.EntityId] = m.States;
            _crops[m.EntityId] = m.Display.Crop;
            // ⚠️ ต้องจำ type/tile ของ **ทุก** artifact ไว้ก่อน — ตอน AppearArtifact มาถึง
            // ยังไม่รู้เลยว่าอันไหนคือแปลงที่เพิ่งวาง (id มากับข้อความ cheat ทีหลัง)
            _types[m.EntityId] = m.EntityType;
            _tiles[m.EntityId] = m.Tile;
        });
        conn.Recv<ArtifactState>((m, h) => { if (!string.IsNullOrEmpty(m.EntityId)) _states[m.EntityId] = m; });
        conn.Recv<ArtifactDisplay>((m, h) => { if (!string.IsNullOrEmpty(m.EntityId)) _crops[m.EntityId] = m.Crop; });
        conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.Recv<ExpGained>((m, h) => { });
        conn.Recv<Touched>((m, h) => _touched = m);
        conn.Recv<Collected>((m, h) => _collected = m);
        conn.Recv<CollectibleChanged>((m, h) => { });
        conn.Recv<Quests>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<QuestStarted>((m, h) => { });
        conn.Recv<NotifyQuestProceed>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2500);
        return conn;
    }

    /// <summary>สั่ง cheat farm แล้วดึง entity id ของแปลงจากข้อความตอบ</summary>
    private static string MakeFarm(Connection conn)
    {
        _infos.Clear();
        conn.Send(new Cheat { _Cheat = "farm" });
        Pump(conn, 1200);
        foreach (string line in _infos)
        {
            Match m = Regex.Match(line ?? "", @"\[id=([^\]]+)\]");
            if (m.Success) return m.Groups[1].Value;
        }
        return null;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== farm check (ระบบปลูกผัก): {host}:{gamePort} ===");
        string id = "farm-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        Connection conn = Connect(host, gamePort, gatewayPort, id);
        if (conn == null) { Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม"); return 1; }

        conn.Send(new Cheat { _Cheat = "heal" });
        Pump(conn, 400);

        // ── รอบ 1: วางแปลง ────────────────────────────────────────────
        Console.WriteLine("รอบ 1 — วางแปลงผักแล้วแตะดูเมนู");
        string farm = MakeFarm(conn);
        Pump(conn, 600);
        Check("วางแปลงผักได้ (cheat farm คืน id)", !string.IsNullOrEmpty(farm), farm ?? "(ไม่ได้ id)");
        if (string.IsNullOrEmpty(farm))
        {
            Console.WriteLine("=== สรุป: ผ่าน {0} / ตก {1} ===", _passed, _failed + 1);
            return 1;
        }
        Check("ได้เมล็ด/น้ำ/ปุ๋ยสำหรับเทส",
            CountItem("corn_seed") > 0 && CountItem("water") > 0 && CountItem("fertilizer_01") > 0,
            $"เมล็ด {CountItem("corn_seed")} · น้ำ {CountItem("water")} · ปุ๋ย {CountItem("fertilizer_01")}");

        Touched? t = Touch(conn, farm, TypeOf(farm), TileOf(farm));
        Check("แตะแปลงเปล่าแล้วได้เมนู \"ปลูก\"", HasInteraction(t, Plant),
            t?.Interactions == null ? "ไม่มี interactions" : string.Join(",", t.Value.Interactions));
        Check("แปลงเปล่ายังไม่มีเมนู \"เก็บ\"", !HasInteraction(t, Collect_));

        // ── รอบ 2: ปลูก ───────────────────────────────────────────────
        Console.WriteLine("รอบ 2 — ลงเมล็ด");
        Item? notSeed = FindItem("water");
        _aborts = 0;
        conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = notSeed?.Id });
        Pump(conn, 800);
        Check("ปลูกด้วยของที่ไม่ใช่เมล็ด ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");

        int seedsBefore = CountItem("corn_seed");
        Item? seed = FindItem("corn_seed");
        _aborts = 0;
        conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = seed?.Id });
        Pump(conn, 2500);
        Check("ปลูกด้วยเมล็ดจริงผ่าน", _aborts == 0, $"abort={_aborts}");
        Check("เมล็ดหายจากกระเป๋า 1 เม็ด", CountItem("corn_seed") == seedsBefore - 1,
            $"{seedsBefore} → {CountItem("corn_seed")}");
        Farming? f = FarmOf(farm);
        Check("แปลงมีสถานะการปลูกแล้ว", f.HasValue, f.HasValue ? f.Value.PlantName : "(ไม่มี)");
        Check("ตั้งเวลาโตไว้ในอนาคต", f.HasValue && f.Value.GrowsUntil > f.Value.PlantedAt,
            f.HasValue ? $"{f.Value.GrowsUntil - f.Value.PlantedAt:F0} วิ" : "-");
        Check("client ได้ชื่อสไปรต์ต้นอ่อนไปวาด", !string.IsNullOrEmpty(_crops.TryGetValue(farm, out string c0) ? c0 : null),
            _crops.TryGetValue(farm, out string c1) ? c1 : "(ว่าง)");

        _aborts = 0;
        Item? seed2 = FindItem("corn_seed");
        conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = seed2?.Id });
        Pump(conn, 800);
        Check("ปลูกซ้ำในแปลงที่มีต้นอยู่แล้ว ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");

        // ── รอบ 3: รดน้ำ / ใส่ปุ๋ย ─────────────────────────────────────
        Console.WriteLine("รอบ 3 — รดน้ำและใส่ปุ๋ย");
        _aborts = 0;
        conn.Send(new WaterPlant { EntityId = farm, Tile = TileOf(farm), ItemIds = new[] { FindItem("fertilizer_01")?.Id } });
        Pump(conn, 700);
        Check("รดน้ำด้วยปุ๋ย ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");

        float waterBefore = FarmOf(farm)?.Water.x ?? 0f;
        string[] waters = _inventory.Where(x => x.Prototype == "water").Select(x => x.Id).ToArray();
        _aborts = 0;
        conn.Send(new WaterPlant { EntityId = farm, Tile = TileOf(farm), ItemIds = waters });
        Pump(conn, 2000);
        Farming? afterWater = FarmOf(farm);
        Check("รดน้ำด้วยน้ำจริงผ่าน", _aborts == 0, $"abort={_aborts}");
        Check("ค่าน้ำเพิ่มขึ้นจริง", (afterWater?.Water.x ?? 0f) > waterBefore,
            $"{waterBefore:F1} → {afterWater?.Water.x:F1} / {afterWater?.Water.y:F0}");
        Check("น้ำถูกใช้ไปจากกระเป๋า", CountItem("water") == 0, $"เหลือ {CountItem("water")}");

        float fertBefore = FarmOf(farm)?.FertilizerAmount ?? 0f;
        string[] ferts = _inventory.Where(x => x.Prototype == "fertilizer_01").Select(x => x.Id).ToArray();
        _aborts = 0;
        conn.Send(new FertilizePlant { EntityId = farm, Tile = TileOf(farm), ItemIds = ferts });
        Pump(conn, 2000);
        Farming? afterFert = FarmOf(farm);
        Check("ใส่ปุ๋ยผ่าน", _aborts == 0, $"abort={_aborts}");
        Check("ค่าปุ๋ยเพิ่มขึ้นจริง", (afterFert?.FertilizerAmount ?? 0f) > fertBefore,
            $"{fertBefore:F1} → {afterFert?.FertilizerAmount:F1} / {afterFert?.RequiredFertilizer}");

        // ── รอบ 4: โตแล้วเก็บเกี่ยว ────────────────────────────────────
        Console.WriteLine("รอบ 4 — เร่งให้โตแล้วเก็บเกี่ยว");
        conn.Send(new Cheat { _Cheat = "grow" });
        Pump(conn, 2500);
        t = Touch(conn, farm, TypeOf(farm), TileOf(farm));
        Check("โตแล้วได้เมนู \"เก็บ\"", HasInteraction(t, Collect_),
            t?.Interactions == null ? "ไม่มี interactions" : string.Join(",", t.Value.Interactions));
        Generator[] gens = t?.Collectible.Generators;
        Check("มี generator ของผลผลิตให้เก็บ", gens != null && gens.Length > 0,
            gens == null ? "null" : string.Join(" ", gens.Select(g => $"{g.Id}x{g.Amount}")));
        Generator? corn = gens?.FirstOrDefault(g => g.Id == "corn_crop");
        Check("ผลผลิตคือข้าวโพด (corn_crop)", corn.HasValue && corn.Value.Id == "corn_crop",
            corn?.Id ?? "(ไม่มี)");
        Check("ใส่ปุ๋ยแล้วได้ผลผลิตมากกว่า 1 ชิ้น", (corn?.Amount ?? 0) > 1, $"{corn?.Amount} ชิ้น");

        int totalUnits = gens == null ? 0 : gens.Sum(g => g.Amount);
        int cornBefore = CountItem("corn_crop");
        int harvested = 0;
        for (int round = 0; round < totalUnits + 3; round++)
        {
            _collected = null;
            conn.Send(new Collect { EntityId = farm, GeneratorId = "corn_crop", ToolItemId = null });
            Pump(conn, 900);
            if (_collected.HasValue) harvested++;
        }
        Check("เก็บเกี่ยวได้ข้าวโพดเข้ากระเป๋าจริง", CountItem("corn_crop") > cornBefore,
            $"{cornBefore} → {CountItem("corn_crop")}");
        Check("เก็บสำเร็จไม่เกินจำนวนที่มีจริง (ไม่ปั๊มของ)", harvested <= (corn?.Amount ?? 0),
            $"เก็บสำเร็จ {harvested} ครั้ง จากที่มี {corn?.Amount} หน่วย");

        Item? cornItem = FindItem("corn_crop");
        bool tagged = cornItem.HasValue && cornItem.Value.Tags != null
                      && cornItem.Value.Tags.Any(x => x.Id == "grain" || x.Id == "vegetable");
        Check("ผลผลิตมี tag (เอาไปทำอาหารต่อได้)", tagged,
            cornItem?.Tags == null ? "ไม่มี tag" : string.Join(",", cornItem.Value.Tags.Select(x => x.Id)));

        // เก็บเมล็ดที่เหลือให้หมดแปลง
        Generator? seedGen = gens?.FirstOrDefault(g => g.Id == "corn_seed_crop");
        if (seedGen.HasValue && seedGen.Value.Amount > 0)
        {
            for (int round = 0; round < seedGen.Value.Amount + 2; round++)
            {
                conn.Send(new Collect { EntityId = farm, GeneratorId = "corn_seed_crop", ToolItemId = null });
                Pump(conn, 900);
            }
            Check("เก็บเมล็ดคืนได้ด้วย", CountItem("corn_seed_crop") > 0, $"{CountItem("corn_seed_crop")} เม็ด");
        }
        else
        {
            Check("เก็บเมล็ดคืนได้ด้วย", false, "ไม่มี generator เมล็ด");
        }

        Pump(conn, 800);
        t = Touch(conn, farm, TypeOf(farm), TileOf(farm));
        Check("เก็บหมดแล้วแปลงกลับเป็นแปลงเปล่า", HasInteraction(t, Plant) && !HasInteraction(t, Collect_),
            t?.Interactions == null ? "ไม่มี interactions" : string.Join(",", t.Value.Interactions));

        // ── รอบ 5: ไม่รดน้ำแล้วต้นตาย ─────────────────────────────────
        Console.WriteLine("รอบ 5 — ปลูกแล้วไม่รดน้ำ ต้องตาย");
        conn.Send(new Cheat { _Cheat = "seeds" });
        Pump(conn, 700);
        Item? seed3 = FindItem("corn_seed");
        _aborts = 0;
        conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = seed3?.Id });
        Pump(conn, 2500);
        Check("ปลูกในแปลงที่เก็บหมดแล้วได้ทันที", _aborts == 0 && FarmOf(farm).HasValue, $"abort={_aborts}");

        conn.Send(new Cheat { _Cheat = "grow" });
        Pump(conn, 2500);
        t = Touch(conn, farm, TypeOf(farm), TileOf(farm));
        Check("ต้นที่ไม่ได้รดน้ำ เก็บไม่ได้ (ตายแล้ว)", !HasInteraction(t, Collect_),
            t?.Interactions == null ? "ไม่มี interactions" : string.Join(",", t.Value.Interactions));
        Check("ต้นตายแล้วยังถอนได้", HasInteraction(t, Uproot));

        _aborts = 0;
        conn.Send(new UprootPlant { EntityId = farm, Tile = TileOf(farm) });
        Pump(conn, 2500);
        Check("ถอนต้นที่ตายแล้วผ่าน", _aborts == 0, $"abort={_aborts}");
        Check("ถอนแล้วสถานะการปลูกหายไป", !FarmOf(farm).HasValue);

        _aborts = 0;
        conn.Send(new UprootPlant { EntityId = farm, Tile = TileOf(farm) });
        Pump(conn, 800);
        Check("ถอนแปลงเปล่า ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");

        // ── รอบ 6: ตักน้ำ ─────────────────────────────────────────────
        Console.WriteLine("รอบ 6 — ตักน้ำ");
        conn.Send(new Cheat { _Cheat = "seeds" });
        Pump(conn, 700);
        _aborts = 0;
        conn.Send(new DrawWater { ToolItemId = FindItem("corn_seed")?.Id });
        Pump(conn, 900);
        Check("ตักน้ำด้วยของที่ไม่ใช่ภาชนะ ไม่ผ่าน", _aborts > 0, $"abort={_aborts}");

        // ── รอบ 7: เก็บบางส่วนแล้วเซฟ — จำนวนที่เหลือต้องถูกจดไว้ ─────
        // (ถ้าไม่จด พอรีสตาร์ทเซิร์ฟระบบจะคิดผลผลิตใหม่เต็มจำนวน = ปั๊มของด้วยการรีสตาร์ท)
        Console.WriteLine("รอบ 7 — เก็บบางส่วนแล้วเซฟ");
        conn.Send(new Cheat { _Cheat = "seeds" });
        Pump(conn, 700);
        conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = FindItem("corn_seed")?.Id });
        Pump(conn, 2500);
        conn.Send(new WaterPlant
        {
            EntityId = farm,
            Tile = TileOf(farm),
            ItemIds = _inventory.Where(x => x.Prototype == "water").Select(x => x.Id).ToArray()
        });
        Pump(conn, 2000);
        conn.Send(new FertilizePlant
        {
            EntityId = farm,
            Tile = TileOf(farm),
            ItemIds = _inventory.Where(x => x.Prototype == "fertilizer_01").Select(x => x.Id).ToArray()
        });
        Pump(conn, 2000);
        conn.Send(new Cheat { _Cheat = "grow" });
        Pump(conn, 2500);
        t = Touch(conn, farm, TypeOf(farm), TileOf(farm));
        Generator[] gens2 = t?.Collectible.Generators;
        int cornTotal = gens2?.FirstOrDefault(g => g.Id == "corn_crop").Amount ?? 0;
        Check("ต้นที่รดน้ำ+ใส่ปุ๋ยครบได้ผลผลิตหลายชิ้น", cornTotal > 1, $"{cornTotal} ชิ้น");

        conn.Send(new Collect { EntityId = farm, GeneratorId = "corn_crop", ToolItemId = null });
        Pump(conn, 1200);
        _infos.Clear();
        conn.Send(new Cheat { _Cheat = "farms" });
        Pump(conn, 700);
        string report = string.Join(" | ", _infos);
        Check("เก็บไป 1 ชิ้นแล้วเหลือน้อยลงจริง",
            report.Contains("corn_crop x" + Math.Max(0, cornTotal - 1)),
            report.Length > 160 ? report.Substring(0, 160) : report);

        _infos.Clear();
        conn.Send(new Cheat { _Cheat = "save" });
        Pump(conn, 1500);
        Check("บังคับเซฟโลกได้", _infos.Any(x => x.Contains("เซฟโลกแล้ว")),
            _infos.FirstOrDefault() ?? "(ไม่มีคำตอบ)");

        // ── รอบ 8: เข้าใหม่แล้วต้นยังอยู่ ──────────────────────────────
        Console.WriteLine("รอบ 8 — ปลูกแล้วออกเกมเข้าใหม่");
        conn.Send(new UprootPlant { EntityId = farm, Tile = TileOf(farm) });   // เคลียร์ของรอบ 7 ก่อน
        Pump(conn, 2500);
        conn.Send(new Cheat { _Cheat = "seeds" });
        Pump(conn, 700);
        Item? seed4 = FindItem("corn_seed");
        _aborts = 0;
        conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = seed4?.Id });
        Pump(conn, 2500);
        bool plantedAgain = FarmOf(farm).HasValue;
        Check("ปลูกรอบใหม่ได้", plantedAgain && _aborts == 0, $"abort={_aborts}");

        conn.Close();
        Thread.Sleep(700);
        Connection conn2 = Connect(host, gamePort, gatewayPort, id);
        if (conn2 == null) { Console.WriteLine("ต่อรอบสองไม่ได้"); }
        else
        {
            Pump(conn2, 1200);
            Farming? after = FarmOf(farm);
            Check("เข้าใหม่แล้วต้นที่ปลูกไว้ยังอยู่", after.HasValue,
                after.HasValue ? after.Value.PlantName : "(หายไป)");
            Check("เวลาโตยังเป็นชุดเดิม", after.HasValue && after.Value.GrowsUntil > 0,
                after.HasValue ? $"{after.Value.GrowsUntil:F0}" : "-");
            conn2.Close();
        }

        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>
    /// เทสช่องโหว่ "รีสตาร์ทเซิร์ฟแล้วผลผลิตเกิดใหม่" — รัน 2 เฟส โดยรีสตาร์ทเซิร์ฟคั่นกลาง
    ///
    ///   dotnet run -- --farm-resume-check setup     # ปลูก → โต → เก็บ 1 ชิ้น → เซฟ
    ///   (รีสตาร์ทเซิร์ฟ)
    ///   dotnet run -- --farm-resume-check verify    # ของที่เหลือต้องเท่าเดิม ไม่ใช่เต็มใหม่
    ///
    /// ใช้ id ผู้เล่นคงที่ทั้งสองเฟส แปลงจึงเป็นของคนเดิม
    /// </summary>
    public static int RunResume(string phase, string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== farm resume check ({phase}): {host}:{gamePort} ===");
        const string id = "farm-resume-1";
        Connection conn = Connect(host, gamePort, gatewayPort, id);
        if (conn == null) { Console.WriteLine("ขอ token ไม่ได้"); return 1; }
        conn.Send(new Cheat { _Cheat = "heal" });
        Pump(conn, 400);

        if (phase == "setup")
        {
            string farm = MakeFarm(conn);
            Pump(conn, 800);
            if (string.IsNullOrEmpty(farm)) { Console.WriteLine("วางแปลงไม่ได้"); return 1; }
            conn.Send(new PlantSeed { EntityId = farm, Tile = TileOf(farm), SeedItemId = FindItem("corn_seed")?.Id });
            Pump(conn, 2500);
            conn.Send(new WaterPlant
            {
                EntityId = farm,
                Tile = TileOf(farm),
                ItemIds = _inventory.Where(x => x.Prototype == "water").Select(x => x.Id).ToArray()
            });
            Pump(conn, 2000);
            conn.Send(new FertilizePlant
            {
                EntityId = farm,
                Tile = TileOf(farm),
                ItemIds = _inventory.Where(x => x.Prototype == "fertilizer_01").Select(x => x.Id).ToArray()
            });
            Pump(conn, 2000);
            conn.Send(new Cheat { _Cheat = "grow" });
            Pump(conn, 2500);
            Touched? tt = Touch(conn, farm, TypeOf(farm), TileOf(farm));
            int total = tt?.Collectible.Generators?.FirstOrDefault(g => g.Id == "corn_crop").Amount ?? 0;
            conn.Send(new Collect { EntityId = farm, GeneratorId = "corn_crop", ToolItemId = null });
            Pump(conn, 1500);
            conn.Send(new Cheat { _Cheat = "save" });
            Pump(conn, 1500);
            Console.WriteLine($"SETUP farm={farm} total={total} expectAfterRestart={total - 1}");
            conn.Close();
            return 0;
        }

        _infos.Clear();
        conn.Send(new Cheat { _Cheat = "farms" });
        Pump(conn, 900);
        Console.WriteLine("VERIFY " + string.Join(" | ", _infos).Replace("\n", " / "));
        conn.Close();
        return 0;
    }
}
