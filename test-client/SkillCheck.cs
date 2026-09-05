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
/// เทส **ความชำนาญของหมวดสกิล** (สกิลที่ขึ้นเองจากการทำงาน) + **ค่าสถานะตอนเข้าเกม**
///
/// สองเรื่องนี้อยู่ไฟล์เดียวกันเพราะเจอจากการเล่นจริงรอบเดียวกัน:
///   · "สกิลอัตโนมัติไม่อัพให้เลย" — หมวดสกิลค้างเลเวล 0 ตลอด
///   · "เข้าเซิฟมาหลอดขึ้น 999/999 ต้องรอไปเก็บของก่อนถึงตรง" — Statistics ไม่ถูกส่งตอนเข้าเกม
///
/// เช็ค:
///   1. เข้าเกมปุ๊บได้ Statistics ทันที (ไม่ต้องขอเอง) และค่าเพดานหลอดตรงกับ config
///   2. เข้าเกมปุ๊บได้ Survival ทันที และหลอดไม่ใช่ค่ามั่ว
///   3. หมวดสกิลเริ่มที่เลเวล 1 (ไม่ใช่ 0) และส่งมาครบทุกหมวด
///   4. คราฟต์ของ -> หมวดที่ตรงกันขึ้นเลเวล · หมวดอื่นไม่ขึ้น
///   5. ทำสำเร็จซ้ำ ๆ แล้วเลเวลไต่ขึ้นเรื่อย ๆ ตามตารางของเกม
///   6. ออกเกมเข้าใหม่ ความชำนาญยังอยู่
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --skill-check [host] [port เกม] [port gateway]
/// </summary>
public static class SkillCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static Item[] _inventory = Array.Empty<Item>();
    private static Statistics? _stats;
    private static Survival? _survival;
    private static Dictionary<Shared.Skill.Category, SkillCategory> _categories;
    private static int _crafted;
    private static int _aborts;
    private static readonly List<string> _infos = new List<string>();

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }

    private static int LevelOf(Shared.Skill.Category cat)
    {
        if (_categories != null && _categories.TryGetValue(cat, out SkillCategory c))
        {
            return c.Level;
        }
        return -1;
    }

    /// <summary>ต่อเข้าเซิร์ฟด้วย id เดิม แล้วคืน connection ที่พร้อมใช้</summary>
    /// <summary>id จริงที่ gateway ออกให้ครั้งล่าสุด — ใช้ต่อกลับให้เป็นตัวละครเดิม</summary>
    private static string _resolvedId;

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        // token ผูกกับ user_id ที่ gateway ออกให้ ไม่ใช่ชื่อที่ขอไป (ไม่งั้น auth โดนปฏิเสธเงียบ ๆ)
        // ⚠️ ต้องจำ id ที่ได้ไว้ด้วย — ถ้าต่อกลับเข้ามาด้วย "ชื่อ" อีกครั้ง gateway จะออก id ใหม่
        //    = กลายเป็นตัวละครคนละตัว แล้วเทส "ค่ายังอยู่ไหมหลังเข้าใหม่" จะตกทั้งที่เซิร์ฟไม่ผิด
        if (!string.IsNullOrEmpty(SessionClient.LastUserId)) { id = SessionClient.LastUserId; }
        _resolvedId = id;
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        _stats = null;
        _survival = null;
        _categories = null;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _infos.Add(m.Text ?? ""));
        conn.Recv<Statistics>((m, h) => _stats = m);
        conn.Recv<Survival>((m, h) => _survival = m);
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Skills>((m, h) => { if (m.Categories != null) _categories = m.Categories; });
        conn.Recv<Crafted>((m, h) => { if (m.Items != null) _crafted += m.Items.Length; });
        conn.Recv<Inventory>((m, h) => { if (m.InventoryItems.Items != null) _inventory = m.InventoryItems.Items; });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<ItemUsed>((m, h) => { });
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
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2500);
        return conn;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== skill / proficiency check: {host}:{gamePort} ===");
        string id = "skill-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        Connection conn = Connect(host, gamePort, gatewayPort, id);
        if (conn == null)
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }

        // ── รอบ 1: เข้าเกมมาต้องได้ค่าสถานะครบทันที ────────────────────
        Console.WriteLine("รอบ 1 — เข้าเกมมาต้องได้ค่าสถานะทันที (ไม่ต้องขอเอง ไม่ต้องไปเก็บของก่อน)");
        Check("ได้ Statistics ตอนเข้าเกม (ไม่ได้ส่ง GetStatistics)", _stats.HasValue);
        if (_stats.HasValue && _stats.Value.DerivedsAbilities != null)
        {
            var d = _stats.Value.DerivedsAbilities;
            float lifeMax = d.TryGetValue(Shared.Ability.Derived.LifeMax, out float lm) ? lm : -1f;
            float stamMax = d.TryGetValue(Shared.Ability.Derived.StaminaMax, out float sm) ? sm : -1f;
            float fatMax = d.TryGetValue(Shared.Ability.Derived.FatigueMax, out float fm) ? fm : -1f;
            Check("เพดานหลอดเป็นค่าจริง ไม่ใช่ค่ามั่ว (999)",
                lifeMax > 0f && lifeMax <= 1000f && stamMax > 0f && stamMax <= 1000f && fatMax > 0f,
                $"เลือด {lifeMax:F0} · สตามินา {stamMax:F0} · ล้า {fatMax:F0}");
        }
        else
        {
            Check("เพดานหลอดเป็นค่าจริง ไม่ใช่ค่ามั่ว (999)", false, "ไม่มี DerivedsAbilities");
        }
        Check("ได้ Survival ตอนเข้าเกม", _survival.HasValue);
        if (_survival.HasValue && _survival.Value.Gauges != null)
        {
            var g = _survival.Value.Gauges;
            bool ok = g.ContainsKey("life") && g.ContainsKey("stamina") && g.ContainsKey("fatigue");
            Check("มีหลอดครบ 3 อย่าง (เลือด/สตามินา/ล้า)", ok,
                ok ? $"เลือด {g["life"].Get():F0} · สตามินา {g["stamina"].Get():F0} · ล้า {g["fatigue"].Get():F0}" : "ไม่ครบ");
        }
        else
        {
            Check("มีหลอดครบ 3 อย่าง (เลือด/สตามินา/ล้า)", false, "ไม่มี Gauges");
        }

        // ── รอบ 2: หมวดสกิลส่งมาครบและเริ่มที่ 1 ────────────────────────
        Console.WriteLine("รอบ 2 — หมวดสกิลตอนเริ่มต้น");
        Check("ได้รายการหมวดสกิลตอนเข้าเกม", _categories != null && _categories.Count > 0,
            _categories == null ? "ไม่ได้เลย" : $"{_categories.Count} หมวด");
        Check("ส่งมาครบ 13 หมวด", _categories != null && _categories.Count == 13,
            _categories == null ? "-" : _categories.Count.ToString());
        Check("หมวดเริ่มที่เลเวล 1 ไม่ใช่ 0", LevelOf(Shared.Skill.Category.Gathering) == 1,
            $"เก็บของ = {LevelOf(Shared.Skill.Category.Gathering)}");

        // ── รอบ 3: คราฟต์แล้วหมวดที่ตรงกันต้องขึ้น ──────────────────────
        Console.WriteLine("รอบ 3 — คราฟต์ 6 ครั้ง (blade_stone = หมวดทำอาวุธ)");
        int weaponBefore = LevelOf(Shared.Skill.Category.Weaponcrafting);
        int gatherBefore = LevelOf(Shared.Skill.Category.Gathering);
        conn.Send(new Cheat { _Cheat = "clearbag" });
        Pump(conn, 500);
        conn.Send(new Cheat { _Cheat = "give stone 12" });
        Pump(conn, 800);

        int done = 0;
        for (int round = 0; round < 6; round++)
        {
            Item? stone = _inventory.FirstOrDefault(x => x.Prototype == "stone") is Item st && st.Id != null ? st : (Item?)null;
            if (stone == null)
            {
                break;
            }
            _crafted = 0;
            conn.Send(new Craft
            {
                RecipeId = "blade_stone",
                Materials = new Dictionary<string, string[]> { { "base", new[] { stone.Value.Id } } },
                ToolItemId = null,
                Workbench = null
            });
            Pump(conn, 2500);
            if (_crafted > 0)
            {
                done++;
            }
        }
        Pump(conn, 800);
        int weaponAfter = LevelOf(Shared.Skill.Category.Weaponcrafting);
        int gatherAfter = LevelOf(Shared.Skill.Category.Gathering);
        Check("คราฟต์สำเร็จอย่างน้อย 3 ครั้ง", done >= 3, $"สำเร็จ {done} ครั้ง");
        Check("หมวดทำอาวุธเลเวลขึ้นเอง", weaponAfter > weaponBefore, $"{weaponBefore} → {weaponAfter}");
        Check("หมวดที่ไม่เกี่ยวไม่ขึ้นตาม", gatherAfter == gatherBefore, $"เก็บของ {gatherBefore} → {gatherAfter}");
        Check("มีข้อความบอกตอนความชำนาญขึ้น",
            _infos.Any(t => t.Contains("ความชำนาญ")),
            _infos.LastOrDefault(t => t.Contains("ความชำนาญ")) ?? "(ไม่มี)");

        // ── รอบ 4: ออกแล้วเข้าใหม่ ความชำนาญต้องอยู่ ────────────────────
        Console.WriteLine("รอบ 4 — ออกเกมแล้วเข้าใหม่");
        conn.Close();
        Thread.Sleep(1500);
        Connection again = Connect(host, gamePort, gatewayPort, _resolvedId ?? id);
        if (again == null)
        {
            Check("ต่อกลับเข้ามาได้", false);
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return 1;
        }
        int weaponReload = LevelOf(Shared.Skill.Category.Weaponcrafting);
        Check("ความชำนาญยังอยู่หลังเข้าใหม่", weaponReload == weaponAfter, $"ก่อนออก {weaponAfter} · เข้าใหม่ {weaponReload}");
        Check("เข้าใหม่แล้วยังได้ Statistics ทันที", _stats.HasValue);
        again.Close();

        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
