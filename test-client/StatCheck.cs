using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Ability;
using Shared.Item;

namespace DurangoTestClient;

/// <summary>
/// เทส **ค่าสถานะของตัวละคร** — ของสามอย่างที่เคยเป็นของปลอมทั้งหมด
///
///   1. ค่าสถานะ 8 ตัว (พลัง/ความอดทน/...) เคยส่งค่าคงที่ 20 เท่ากันหมดทุกคนตลอดชีพ
///   2. เลือด/สตามินาสูงสุด เคยเป็นค่าคงที่จาก config ⇒ ขึ้นเลเวลแล้วตัวไม่แข็งขึ้นเลย
///   3. อุปกรณ์ไม่มีค่าพลัง ⇒ อาวุธทุกชิ้นบวกดาเมจเท่ากัน · เกราะไม่มีค่าป้องกันเลย
///
/// เช็ค:
///   1. ค่าสถานะ 8 ตัวส่งมาครบและอยู่ในช่วงที่เป็นไปได้
///   2. ยัด exp ให้ขึ้นเลเวล → เลือด/สตามินาสูงสุดต้องโตตาม
///   3. ฟาร์มความชำนาญหมวดทำอาวุธ → ความคล่องมือ (Dexterity) ขึ้น · ค่าที่ไม่เกี่ยวไม่ขึ้น
///   4. ใส่ขวานหิน → พลังโจมตี (Derived.Attack) เพิ่ม
///   5. ใส่ขวานสองมือ (ช่อง "both") ได้จริง — เดิม server ปฏิเสธเพราะไม่มีช่องนี้ในรายการ
///   6. อาวุธแรงกว่า = พลังโจมตีมากกว่า (ไม่ใช่ +10 เท่ากันหมด)
///   7. ใส่ของผิดช่อง (หินใส่หัว) ต้องโดนปฏิเสธ
///   8. ใส่เสื้อ → ค่าป้องกัน (Derived.Defense) เพิ่มขึ้นจาก 0
///   9. **โดนดาเมจก้อนเดิมแล้วเจ็บน้อยลงจริง** เมื่อใส่เกราะ
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --stat-check [host] [port เกม] [port gateway]
/// </summary>
public static class StatCheck
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
    private static SurvivalUpdated? _survivalUpdated;
    private static Gauge _lifeGauge;
    private static int _aborts;
    private static int _crafted;
    private static readonly List<string> _infos = new List<string>();

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }

    private static int Basic(Shared.Ability.Basic ability)
    {
        if (_stats.HasValue && _stats.Value.BasicAbilities != null
            && _stats.Value.BasicAbilities.TryGetValue(ability, out int v))
        {
            return v;
        }
        return -1;
    }

    private static float Derived(Shared.Ability.Derived ability)
    {
        if (_stats.HasValue && _stats.Value.DerivedsAbilities != null
            && _stats.Value.DerivedsAbilities.TryGetValue(ability, out float v))
        {
            return v;
        }
        return -1f;
    }

    /// <summary>
    /// เลือดปัจจุบันจากหลอดล่าสุดที่เซิร์ฟส่งมา (−1 ถ้ายังไม่เคยได้)
    ///
    /// ⚠️ `SurvivalUpdated` ส่งมา **เฉพาะหลอดที่เพิ่งเปลี่ยน** (เสียสตามินา = ส่งแค่ stamina/fatigue)
    /// จึงต้องจำหลอดเลือดล่าสุดไว้เอง ไม่ใช่อ่านจากข้อความล่าสุดที่ได้
    /// </summary>
    private static float CurrentLife()
    {
        return _lifeGauge != null ? _lifeGauge.Get() : -1f;
    }

    /// <summary>ขอ Statistics ชุดใหม่แล้วรอจนได้</summary>
    private static void RefreshStats(Connection conn)
    {
        _stats = null;
        conn.Send(default(GetStatistics));
        Pump(conn, 700);
    }

    /// <summary>ใส่ของ คืน true ถ้าเซิร์ฟรับ (ไม่ Abort)</summary>
    private static bool Equip(Connection conn, string itemId, string slot)
    {
        int before = _aborts;
        conn.Send(new Equip { ItemId = itemId, SlotName = slot, SlotType = EquipSlotType.Slot1, Action = "equip" });
        Pump(conn, 700);
        return _aborts == before;
    }

    private static void Unequip(Connection conn, string slot)
    {
        conn.Send(new Equip { ItemId = string.Empty, SlotName = slot, SlotType = EquipSlotType.Slot1, Action = "unequip" });
        Pump(conn, 500);
    }

    /// <summary>ขอของจากเซิร์ฟแล้วคืน id ของชิ้นที่เพิ่งได้ (null ถ้าไม่ได้)</summary>
    private static string GiveAndFind(Connection conn, string prototype)
    {
        conn.Send(new Cheat { _Cheat = "give " + prototype });
        Pump(conn, 900);
        Item found = _inventory.FirstOrDefault(x => x.Prototype == prototype);
        return found.Id;
    }

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        _stats = null;
        _survival = null;
        _survivalUpdated = null;
        _lifeGauge = null;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _infos.Add(m.Text ?? ""));
        conn.Recv<Statistics>((m, h) => _stats = m);
        conn.Recv<Survival>((m, h) =>
        {
            _survival = m;
            if (m.Gauges != null && m.Gauges.TryGetValue("life", out Gauge lg)) { _lifeGauge = lg; }
        });
        conn.Recv<SurvivalUpdated>((m, h) =>
        {
            _survivalUpdated = m;
            if (m.Updated != null && m.Updated.TryGetValue("life", out Gauge lg)) { _lifeGauge = lg; }
        });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Crafted>((m, h) => { if (m.Items != null) _crafted += m.Items.Length; });
        conn.Recv<Inventory>((m, h) => { if (m.InventoryItems.Items != null) _inventory = m.InventoryItems.Items; });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<ItemUsed>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<PlayerDisplay>((m, h) => { });
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
        conn.Recv<ExpGained>((m, h) => { });
        conn.Recv<EntityDied>((m, h) => { });
        conn.Recv<EntityRevived>((m, h) => { });
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
        Console.WriteLine($"=== stat check (ค่าสถานะตัวละคร): {host}:{gamePort} ===");
        string id = "stat-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        Connection conn = Connect(host, gamePort, gatewayPort, id);
        if (conn == null)
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }

        // ── รอบ 1: ค่าสถานะ 8 ตัวต้องมีจริง ──────────────────────────────
        Console.WriteLine("รอบ 1 — ค่าสถานะ 8 ตัว");
        Check("ได้ Statistics ตอนเข้าเกม", _stats.HasValue);
        var basics = new[]
        {
            Shared.Ability.Basic.Strength, Shared.Ability.Basic.Charisma, Shared.Ability.Basic.Dexterity,
            Shared.Ability.Basic.Agility, Shared.Ability.Basic.Endurance, Shared.Ability.Basic.Will,
            Shared.Ability.Basic.Intelligence, Shared.Ability.Basic.Perception
        };
        bool allPresent = basics.All(b => Basic(b) > 0);
        Check("ส่งค่าสถานะครบทั้ง 8 ตัว", allPresent,
            string.Join(" ", basics.Select(b => $"{b}={Basic(b)}")));

        // ── รอบ 2: ขึ้นเลเวลแล้วหลอดต้องโต ─────────────────────────────
        Console.WriteLine("รอบ 2 — ขึ้นเลเวลแล้วเลือด/สตามินาสูงสุดต้องโตขึ้น");
        float lifeMaxBefore = Derived(Shared.Ability.Derived.LifeMax);
        float stamMaxBefore = Derived(Shared.Ability.Derived.StaminaMax);
        int enduranceBefore = Basic(Shared.Ability.Basic.Endurance);
        conn.Send(new Cheat { _Cheat = "exp 2000" });
        Pump(conn, 1200);
        RefreshStats(conn);
        float lifeMaxAfter = Derived(Shared.Ability.Derived.LifeMax);
        float stamMaxAfter = Derived(Shared.Ability.Derived.StaminaMax);
        Check("เลือดสูงสุดเพิ่มหลังขึ้นเลเวล", lifeMaxAfter > lifeMaxBefore,
            $"{lifeMaxBefore:F0} → {lifeMaxAfter:F0}");
        Check("สตามินาสูงสุดเพิ่มหลังขึ้นเลเวล", stamMaxAfter > stamMaxBefore,
            $"{stamMaxBefore:F0} → {stamMaxAfter:F0}");
        Check("ค่าสถานะเพิ่มตามเลเวลด้วย", Basic(Shared.Ability.Basic.Endurance) > enduranceBefore,
            $"ความอดทน {enduranceBefore} → {Basic(Shared.Ability.Basic.Endurance)}");

        // ── รอบ 3: ความชำนาญป้อนค่าสถานะ ───────────────────────────────
        Console.WriteLine("รอบ 3 — คราฟต์ซ้ำ ๆ (หมวดทำอาวุธ) แล้วความคล่องมือต้องขึ้น");
        int dexBefore = Basic(Shared.Ability.Basic.Dexterity);
        int charismaBefore = Basic(Shared.Ability.Basic.Charisma);
        conn.Send(new Cheat { _Cheat = "clearbag" });
        Pump(conn, 500);
        conn.Send(new Cheat { _Cheat = "give stone 15" });
        Pump(conn, 900);
        int done = 0;
        for (int round = 0; round < 10; round++)
        {
            Item stone = _inventory.FirstOrDefault(x => x.Prototype == "stone");
            if (stone.Id == null)
            {
                break;
            }
            _crafted = 0;
            conn.Send(new Craft
            {
                RecipeId = "blade_stone",
                Materials = new Dictionary<string, string[]> { { "base", new[] { stone.Id } } },
                ToolItemId = null,
                Workbench = null
            });
            Pump(conn, 2200);
            if (_crafted > 0)
            {
                done++;
            }
        }
        RefreshStats(conn);
        Check("คราฟต์สำเร็จอย่างน้อย 5 ครั้ง", done >= 5, $"สำเร็จ {done} ครั้ง");
        Check("ความคล่องมือขึ้นตามความชำนาญหมวดทำอาวุธ", Basic(Shared.Ability.Basic.Dexterity) > dexBefore,
            $"{dexBefore} → {Basic(Shared.Ability.Basic.Dexterity)}");
        // คราฟต์ให้ exp ผู้เล่นด้วย ⇒ ทุกค่าขยับขึ้นนิดหน่อยตามเลเวล
        // ที่ต้องพิสูจน์คือ **ค่าที่ตรงกับงานที่ทำต้องขึ้นมากกว่า** ค่าที่ไม่เกี่ยว
        int dexGain = Basic(Shared.Ability.Basic.Dexterity) - dexBefore;
        int charismaGain = Basic(Shared.Ability.Basic.Charisma) - charismaBefore;
        Check("ค่าที่ตรงกับงานที่ทำขึ้นมากกว่าค่าที่ไม่เกี่ยว", dexGain > charismaGain,
            $"ความคล่องมือ +{dexGain} · เสน่ห์ +{charismaGain}");

        // ── รอบ 4: อาวุธต้องมีค่าพลังรายชิ้น ────────────────────────────
        Console.WriteLine("รอบ 4 — อาวุธ: ค่าพลังต่างกันตามชิ้น + ใส่ช่อง both ได้");
        conn.Send(new Cheat { _Cheat = "clearbag" });
        Pump(conn, 600);
        RefreshStats(conn);
        float attackBare = Derived(Shared.Ability.Derived.Attack);
        Check("มีค่าพลังโจมตีใน Statistics", attackBare > 0f, $"มือเปล่า {attackBare:F1}");

        string axeId = GiveAndFind(conn, "axe_onehand_stone_01");
        bool axeOk = axeId != null && Equip(conn, axeId, "main");
        RefreshStats(conn);
        float attackAxe = Derived(Shared.Ability.Derived.Attack);
        Check("ใส่ขวานหิน (ช่อง main) ได้", axeOk);
        Check("ถืออาวุธแล้วพลังโจมตีเพิ่ม", attackAxe > attackBare,
            $"{attackBare:F1} → {attackAxe:F1}");

        // ขวานสองมืออยู่ช่อง "both" — เดิมไม่มีช่องนี้ในรายการที่ server ยอมรับ ⇒ ใส่ไม่ได้เลย
        Unequip(conn, "main");
        string bigAxeId = GiveAndFind(conn, "axe_twohand_stone_01");
        bool bigOk = bigAxeId != null && Equip(conn, bigAxeId, "both");
        RefreshStats(conn);
        float attackBig = Derived(Shared.Ability.Derived.Attack);
        Check("ใส่ขวานสองมือ (ช่อง both) ได้", bigOk);
        Check("อาวุธที่แรงกว่าให้พลังโจมตีมากกว่า", attackBig > attackAxe,
            $"ขวานมือเดียว {attackAxe:F1} · ขวานสองมือ {attackBig:F1}");
        Unequip(conn, "both");

        // ── รอบ 5: ใส่ของผิดช่องต้องโดนปฏิเสธ ──────────────────────────
        Console.WriteLine("รอบ 5 — ใส่ของผิดช่อง");
        string stoneId = GiveAndFind(conn, "stone");
        bool wrongSlotAccepted = stoneId != null && Equip(conn, stoneId, "head");
        Check("เอาหินใส่ช่องหมวกไม่ได้", !wrongSlotAccepted);

        // ── รอบ 6: เกราะต้องมีค่าป้องกันและลดดาเมจจริง ─────────────────
        Console.WriteLine("รอบ 6 — เกราะ: ค่าป้องกัน + ลดดาเมจจริง");
        // Earlier rounds can attract an aggressive animal and kill the test
        // character. Revive and refill first so the fixed damage probe is valid.
        conn.Send(default(Revive));
        Pump(conn, 350);
        conn.Send(new Cheat { _Cheat = "rest" });
        Pump(conn, 350);
        RefreshStats(conn);
        float defenseBare = Derived(Shared.Ability.Derived.Defense);

        // วัดดาเมจก้อนคงที่ (cheat hurt = 30) ตอนไม่ใส่เกราะ
        float lifeBeforeHit = CurrentLife();
        conn.Send(new Cheat { _Cheat = "hurt" });
        Pump(conn, 600);
        float damageBare = lifeBeforeHit - CurrentLife();

        string clothesId = GiveAndFind(conn, "clothes_builder_01");
        bool clothesOk = clothesId != null && Equip(conn, clothesId, "body");
        RefreshStats(conn);
        float defenseArmored = Derived(Shared.Ability.Derived.Defense);
        Check("ใส่เสื้อได้", clothesOk);
        Check("ใส่เกราะแล้วค่าป้องกันเพิ่มจาก 0", defenseArmored > defenseBare && defenseArmored > 0f,
            $"{defenseBare:F1} → {defenseArmored:F1}");

        float lifeBeforeHit2 = CurrentLife();
        conn.Send(new Cheat { _Cheat = "hurt" });
        Pump(conn, 600);
        float damageArmored = lifeBeforeHit2 - CurrentLife();
        Check("ใส่เกราะแล้วโดนดาเมจก้อนเดิมเจ็บน้อยลง", damageArmored < damageBare - 1f,
            $"ไม่ใส่ {damageBare:F1} · ใส่ {damageArmored:F1}");

        // ── รอบ 7: ค่าที่ได้ต้องอยู่รอดข้ามการเข้าใหม่ ───────────────────
        Console.WriteLine("รอบ 7 — ออกเกมแล้วเข้าใหม่");
        float lifeMaxSaved = Derived(Shared.Ability.Derived.LifeMax);
        int dexSaved = Basic(Shared.Ability.Basic.Dexterity);
        conn.Close();
        Thread.Sleep(1500);
        Connection again = Connect(host, gamePort, gatewayPort, id);
        if (again == null)
        {
            Check("ต่อกลับเข้ามาได้", false);
            Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
            return 1;
        }
        Check("เลือดสูงสุดยังเท่าเดิมหลังเข้าใหม่",
            Math.Abs(Derived(Shared.Ability.Derived.LifeMax) - lifeMaxSaved) < 0.5f,
            $"{lifeMaxSaved:F0} → {Derived(Shared.Ability.Derived.LifeMax):F0}");
        Check("ค่าสถานะยังเท่าเดิมหลังเข้าใหม่", Basic(Shared.Ability.Basic.Dexterity) == dexSaved,
            $"ความคล่องมือ {dexSaved} → {Basic(Shared.Ability.Basic.Dexterity)}");
        again.Close();

        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
