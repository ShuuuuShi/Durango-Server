using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Etc;

namespace DurangoTestClient;

/// <summary>
/// บั๊ก #7 (เปียก/สกปรก) + ระบบป่วย — ตรวจว่าสถานะขึ้นจริงและมีผลจริง
///   · ลงน้ำ (แม่น้ำ tile 61,133 ของ ri35te) ต้องได้สถานะ wet
///   · ป่วยแล้วต้องเดินช้าลง (SetBaseMoveSpeed) และคราฟต์นานขึ้น (Timer)
/// </summary>
public static class WetSickCheck
{
    private static int _passed, _failed;
    private static readonly List<string> _infos = new();
    private static Messages.StatusEffect[] _effects = Array.Empty<Messages.StatusEffect>();
    private static readonly List<float> _timers = new();
    private static readonly List<int> _speeds = new();
    private static readonly List<string> _leaves = new();

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static bool Has(string id) => _effects.Any(e => e.Id == id || e.EffectId == id);

    private static string Describe()
        => _effects.Length == 0 ? "(ไม่มีสถานะ)" : string.Join(",", _effects.Select(e => e.EffectId ?? e.Id));

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        string modelInfo =
            "{\"hair\":\"hair_f_01\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"F0D9B7\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":1,\"body_size\":1.0}";
        string id = CreateCharacterCheck.CreatePlayer(host, gatewayPort,
            "wet-" + Guid.NewGuid().ToString("N")[..6], isMale: false, modelInfo);
        if (string.IsNullOrEmpty(id)) { Console.WriteLine("สร้างตัวละครไม่ได้"); return 2; }
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + id + "\"}}");
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("ขอ token ไม่ได้"); return 2; }

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var c = new Connection(socket);
        c.Recv<Welcome>((m, h) => { }); c.Recv<Clock>((m, h) => { }); c.Recv<OK>((m, h) => { });
        c.Recv<Abort>((m, h) => { }); c.Recv<Info>((m, h) => _infos.Add(m.Text));
        c.Recv<Messages.StatusEffects>((m, h) => _effects = m._StatusEffects ?? Array.Empty<Messages.StatusEffect>());
        c.Recv<SetBaseMoveSpeed>((m, h) => _speeds.Add(m.NormalSpeed));
        c.Recv<Messages.Timer>((m, h) => _timers.Add(m.Duration));
        c.Recv<Inventory>((m, h) =>
        {
            _leaves.Clear();
            foreach (Item it in m.InventoryItems.Items ?? Array.Empty<Item>())
            {
                if (it.Prototype == "leaf") { _leaves.Add(it.Id); }
            }
        }); c.Recv<Skills>((m, h) => { });
        c.Recv<Statistics>((m, h) => { }); c.Recv<Survival>((m, h) => { });
        c.Recv<AppearPlayer>((m, h) => { }); c.Recv<AppearAnimal>((m, h) => { });
        c.Recv<AppearArtifact>((m, h) => { }); c.Recv<Move>((m, h) => { });
        c.Recv<Teleported>((m, h) => { }); c.Recv<DefoggedChunks>((m, h) => { });
        c.Recv<Chunk>((m, h) => { }); c.Recv<QuestCategories>((m, h) => { });
        c.Recv<WalletUpdated>((m, h) => { }); c.Recv<Recipes>((m, h) => { });
        c.Recv<ArtifactBlueprints>((m, h) => { });
        c.StartReceive();
        c.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(c, 250);
        c.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "wet-sick-check" });
        Pump(c, 450);
        c.Send(default(Ready)); Pump(c, 1500);

        Console.WriteLine($"=== เปียก/สกปรก/ป่วย: {host}:{gamePort} ===");

        // รอบ 1 — ลงน้ำแล้วต้องเปียก
        Console.WriteLine("รอบ 1 — วาร์ปลงแม่น้ำ (61,133) ต้องได้สถานะเปียก");
        c.Send(new Cheat { _Cheat = "tp 61 133" });
        // ตัวตรวจสภาพทำงานหลัง SceneReady (บอทไม่ส่ง SetChunk เซิร์ฟรอ 10 วิ) + รอบตรวจ 2 วิ
        Pump(c, 14000);
        c.Send(default(GetStatusEffects));
        Pump(c, 800);
        Check("ลงน้ำแล้วขึ้นสถานะเปียก", Has("wet"), Describe());

        // รอบ 2 — ป่วยแล้วเดินช้าลง
        Console.WriteLine("รอบ 2 — ป่วยแล้วต้องเดินช้าลง");
        _speeds.Clear();
        c.Send(new Cheat { _Cheat = "sick" });
        Pump(c, 1200);
        c.Send(default(GetStatusEffects));
        Pump(c, 600);
        Check("ป่วยแล้วขึ้นสถานะป่วย", Has("poison_heat"), Describe());
        Check("ป่วยแล้วเซิร์ฟสั่งเดินช้าลง", _speeds.Count > 0 && _speeds[_speeds.Count - 1] < 500,
            _speeds.Count > 0 ? "ความเร็ว " + _speeds[_speeds.Count - 1] : "ไม่ได้รับ SetBaseMoveSpeed");

        // รอบ 3 — ป่วยแล้วคราฟต์นานขึ้น
        Console.WriteLine("รอบ 3 — ป่วยแล้วคราฟต์นานขึ้น");
        c.Send(new Cheat { _Cheat = "cure" });
        Pump(c, 1200);
        float healthy = MeasureCraft(c);
        Pump(c, 6000);      // รอให้คราฟต์รอบแรกจบก่อน ไม่งั้นรอบสองไปทับกัน
        c.Send(new Cheat { _Cheat = "sick" });
        Pump(c, 1200);
        float sick = MeasureCraft(c);
        Check("ป่วยแล้วเวลาคราฟต์นานขึ้น", healthy > 0f && sick > healthy,
            $"ปกติ {healthy:F1} วิ · ป่วย {sick:F1} วิ");

        // รอบ 4 — หายป่วยแล้วความเร็วกลับมา
        Console.WriteLine("รอบ 4 — หายป่วยแล้วเดินเร็วเท่าเดิม");
        _speeds.Clear();
        c.Send(new Cheat { _Cheat = "cure" });
        Pump(c, 1200);
        Check("หายป่วยแล้วความเร็วกลับเป็นปกติ",
            _speeds.Count > 0 && _speeds[_speeds.Count - 1] >= 500,
            _speeds.Count > 0 ? "ความเร็ว " + _speeds[_speeds.Count - 1] : "ไม่ได้รับ SetBaseMoveSpeed");

        Console.WriteLine($"\nสรุป: ผ่าน {_passed} · ไม่ผ่าน {_failed}");
        c.Close();
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>คราฟต์เชือกแล้วคืนเวลาที่เซิร์ฟบอก (0 = ไม่ได้เริ่มคราฟต์)</summary>
    private static float MeasureCraft(Connection c)
    {
        _timers.Clear();
        _leaves.Clear();
        c.Send(new Cheat { _Cheat = "give leaf 2" });
        Pump(c, 1000);
        if (_leaves.Count < 2) { return 0f; }
        c.Send(new Craft
        {
            RecipeId = "hat_leaf",      // มือเปล่า ไม่ต้องใช้โต๊ะ · ช่อง main ใส่ใบไม้ 2 ใบ
            Materials = new Dictionary<string, string[]>
            {
                { "main", new[] { _leaves[_leaves.Count - 1], _leaves[_leaves.Count - 2] } }
            },
            ToolItemId = null
        });
        Pump(c, 3000);
        return _timers.Count > 0 ? _timers[_timers.Count - 1] : 0f;
    }
}
