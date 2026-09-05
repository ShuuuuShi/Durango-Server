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
/// เทส **ท่าต่อสู้ยึดจากสกิลที่เรียนจริง** (combat skill-gating)
///
/// เจ้าของย้ำ 2 รอบ: "ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน"
/// เดิม `HandleUseBattleAction` ตรวจแค่ tag อาวุธ ไม่เคยเช็ค `_knownSkills`
/// ⇒ modded client ใช้ท่าพิเศษได้ทุกอย่างโดยไม่เรียนสกิล
///
/// เช็ค:
///   1. ผู้เล่นใหม่ (มือเปล่า) ได้แค่ท่าพื้นฐาน + ท่าที่ auto-grant (kick/reckless/dodge)
///      ไม่เห็นท่าพิเศษที่ต้องเรียนสกิล (barehand_combination, melee_tackle)
///   2. สั่งใช้ท่าที่ยังไม่ได้เรียน → ถูกปฏิเสธ (Abort)
///   3. หลัง `maxskills` (ปลดทุกสกิล) → เห็นท่าครบทุกอย่าง
///   4. หลัง `maxskills` → ใช้ท่าพิเศษได้ (ไม่ถูกปฏิเสธจาก skill check)
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --combat-skill-check [host] [port เกม] [port gateway]
/// </summary>
public static class CombatSkillCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static ActionStatus[] _actions = Array.Empty<ActionStatus>();
    private static int _aborts;
    private static int _okCount;
    private static readonly List<string> _infos = new List<string>();

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }

    private static bool HasAction(string id)
    {
        return _actions.Any(a => a.Id == id);
    }

    private static int ActionCount => _actions?.Length ?? 0;

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }
        // [4 ก.ย. 2026] token ผูกกับ user_id ที่ gateway ออกให้ (id ที่ไม่มีเซฟ = ได้ id ใหม่)
        // ต้อง Auth ด้วย id นี้ ไม่งั้นโดน "token เป็นของคนอื่น" → player ไม่เข้าจริง → GetActions 0 ท่า
        if (!string.IsNullOrEmpty(SessionClient.LastUserId)) { id = SessionClient.LastUserId; }
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        _actions = Array.Empty<ActionStatus>();
        _aborts = 0;
        _okCount = 0;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => _okCount++);
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _infos.Add(m.Text ?? ""));
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Actions>((m, h) => { if (m.BattleActions != null) _actions = m.BattleActions; });
        conn.Recv<Inventory>((m, h) => { });
        conn.Recv<InventoryUpdated>((m, h) => { });
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
        conn.Recv<BattleBegun>((m, h) => { });
        conn.Recv<Damaged>((m, h) => { });
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
        Console.WriteLine($"=== combat skill check: {host}:{gamePort} ===");
        string id = "combat-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        Connection conn = Connect(host, gamePort, gatewayPort, id);
        if (conn == null)
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }

        // ── รอบ 1: ผู้เล่นใหม่ (มือเปล่า) — ได้แค่ท่าพื้นฐาน + auto-grant ────
        Console.WriteLine("รอบ 1 — ผู้เล่นใหม่ (มือเปล่า): ท่าที่ได้โดยไม่เรียนสกิล");
        conn.Send(default(GetActions));
        Pump(conn, 500);

        // ท่าพื้นฐาน (AlwaysActions) — ทุกคนได้
        Check("ได้ท่าพื้นฐาน barehand_default_a", HasAction("barehand_default_a"));
        Check("ได้ท่าพื้นฐาน barehand_default_b", HasAction("barehand_default_b"));

        // ท่าที่ auto-grant (kick/reckless/dodge ปลดอัตโนมัติที่หมวด lv1)
        // kick → barehand_kick_a, barehand_kick_b
        Check("ได้ท่า kick (auto-grant) barehand_kick_a", HasAction("barehand_kick_a"),
            HasAction("barehand_kick_a") ? "มี" : "ไม่มี");
        // reckless lv1 → barehand_smash
        Check("ได้ท่า reckless lv1 (auto-grant) barehand_smash", HasAction("barehand_smash"),
            HasAction("barehand_smash") ? "มี" : "ไม่มี");
        // dodge → barehand_dodge
        Check("ได้ท่า dodge (auto-grant) barehand_dodge", HasAction("barehand_dodge"),
            HasAction("barehand_dodge") ? "มี" : "ไม่มี");

        // ท่าที่ต้องเรียนสกิล — ยังไม่ได้เรียน ต้องไม่เห็น
        // barehand_combination = reckless lv2 (auto-grant แค่ lv1)
        Check("ไม่เห็น barehand_combination (ต้อง reckless lv2)", !HasAction("barehand_combination"),
            HasAction("barehand_combination") ? "โผล่ทั้งที่ไม่ได้เรียน!" : "ไม่โผล่ ถูกต้อง");
        // melee_tackle = tackle skill (ไม่มี auto-grant)
        Check("ไม่เห็น melee_tackle (ต้องเรียน tackle)", !HasAction("melee_tackle"),
            HasAction("melee_tackle") ? "โผล่ทั้งที่ไม่ได้เรียน!" : "ไม่โผล่ ถูกต้อง");

        int freshCount = ActionCount;
        Check($"ได้ {freshCount} ท่า (ไม่ใช่ 8 ครบ — ต้องหายบางส่วน)", freshCount < 8 && freshCount > 0,
            $"{freshCount} ท่า");

        // ── รอบ 2: สั่งใช้ท่าที่ยังไม่ได้เรียน → ต้องถูกปฏิเสธ ──────────
        Console.WriteLine("รอบ 2 — สั่งใช้ท่าที่ยังไม่ได้เรียนสกิล");
        _aborts = 0;
        _infos.Clear();
        conn.Send(new UseBattleAction
        {
            ActionId = "barehand_combination",
            StartAt = Times.UnixTimeNow(),
            TargetEntityId = "fake-target-id",
            TargetTile = null
        });
        Pump(conn, 500);
        Check("ถูกปฏิเสธ (Abort) เพราะยังไม่ได้เรียนสกิล", _aborts > 0,
            _aborts > 0 ? "ปฏิเสธแล้ว" : "ผ่านทั้งที่ไม่ได้เรียน!");

        // ── รอบ 3: หลัง maxskills → เห็นท่าครบ ──────────────────────
        Console.WriteLine("รอบ 3 — หลังปลดสกิลทั้งหมด (maxskills)");
        conn.Send(new Cheat { _Cheat = "maxskills" });
        Pump(conn, 2000);

        conn.Send(default(GetActions));
        Pump(conn, 500);

        Check("เห็น barehand_combination หลังปลดสกิล", HasAction("barehand_combination"),
            HasAction("barehand_combination") ? "มีแล้ว" : "ยังไม่มี");
        Check("เห็น melee_tackle หลังปลดสกิล", HasAction("melee_tackle"),
            HasAction("melee_tackle") ? "มีแล้ว" : "ยังไม่มี");

        int maxCount = ActionCount;
        Check($"ได้ {maxCount} ท่า (มากกว่าตอนไม่ได้เรียน)", maxCount > freshCount,
            $"ก่อน {freshCount} → หลัง {maxCount}");

        // ── รอบ 4: หลัง maxskills → ใช้ท่าพิเศษได้ ───────────────────
        Console.WriteLine("รอบ 4 — หลังปลดสกิล: ใช้ท่าพิเศษได้ (ไม่ถูกปฏิเสธจาก skill check)");
        _aborts = 0;
        _okCount = 0;
        _infos.Clear();
        conn.Send(new UseBattleAction
        {
            ActionId = "barehand_combination",
            StartAt = Times.UnixTimeNow(),
            TargetEntityId = "fake-target-id",
            TargetTile = null
        });
        Pump(conn, 500);
        // อาจได้ OK (ผ่าน skill check) หรือ Abort (เพราะเป้าหมายไม่มีจริง — แต่ไม่ใช่เพราะ skill check)
        // สำคัญ: ไม่มี Info ว่า "ต้องเรียนสกิล" เพราะผ่าน skill check แล้ว
        bool noSkillReject = !_infos.Any(t => t.Contains("สกิล"));
        Check("ผ่าน skill check (ไม่มีข้อความ 'ต้องเรียนสกิล')", noSkillReject,
            noSkillReject ? "ผ่าน" : "ยังถูกปฏิเสธจาก skill check");

        conn.Close();

        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
