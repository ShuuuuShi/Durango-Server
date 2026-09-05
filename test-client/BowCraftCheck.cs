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
/// บั๊ก "คราฟต์ธนูไม่ได้ทั้งเซิร์ฟ" (เจอตอนไล่เคส FLOKi)
///   bow_wooden_assembled ต้องใช้ของ tag `string_long` 2 ชิ้น
///   ของที่หาได้จริงมีตัวเดียวคือ rope_long ซึ่งมาจากสูตร extend_rope (ต่อเชือก)
///   แต่สูตรแปรรูปเดิมคืน prototype เดิม ⇒ ต่อเชือกแล้วยังได้ rope ธรรมดา ⇒ ธนูคราฟต์ไม่ได้
/// </summary>
public static class BowCraftCheck
{
    private static int _passed, _failed;
    private static readonly List<string> _infos = new();
    private static Item[] _inv = Array.Empty<Item>();
    private static int _aborts;
    private static readonly List<Item> _crafted = new();

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static List<Item> Of(string prototype)
        => _inv.Where(x => x.Prototype == prototype).ToList();

    private static bool HasTag(Item it, string tag)
        => it.Tags != null && it.Tags.Any(t => t.Id == tag);

    private static string Tags(Item it)
        => it.Tags == null ? "(ไม่มี)" : string.Join(",", it.Tags.Select(t => t.Id));

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
            "bow-" + Guid.NewGuid().ToString("N")[..6], isMale: true, modelInfo);
        if (string.IsNullOrEmpty(id)) { Console.WriteLine("สร้างตัวละครไม่ได้"); return 2; }
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + id + "\"}}");
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("ขอ token ไม่ได้"); return 2; }

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var c = new Connection(socket);
        c.Recv<Welcome>((m, h) => { }); c.Recv<Clock>((m, h) => { }); c.Recv<OK>((m, h) => { });
        c.Recv<Abort>((m, h) => _aborts++); c.Recv<Info>((m, h) => _infos.Add(m.Text));
        c.Recv<Inventory>((m, h) => { if (m.InventoryItems.Items != null) _inv = m.InventoryItems.Items; });
        c.Recv<InventoryItems>((m, h) => { if (m.Items != null) _inv = m.Items; });
        c.Recv<Skills>((m, h) => { }); c.Recv<Statistics>((m, h) => { });
        c.Recv<Survival>((m, h) => { }); c.Recv<AppearPlayer>((m, h) => { });
        c.Recv<AppearAnimal>((m, h) => { }); c.Recv<AppearArtifact>((m, h) => { });
        c.Recv<Move>((m, h) => { }); c.Recv<Teleported>((m, h) => { });
        c.Recv<DefoggedChunks>((m, h) => { }); c.Recv<Chunk>((m, h) => { });
        c.Recv<QuestCategories>((m, h) => { }); c.Recv<WalletUpdated>((m, h) => { });
        c.Recv<Recipes>((m, h) => { }); c.Recv<ArtifactBlueprints>((m, h) => { });
        c.Recv<Messages.Timer>((m, h) => { });
        c.StartReceive();
        c.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(c, 250);
        c.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "bow-check" });
        Pump(c, 450);
        c.Send(default(Ready)); Pump(c, 1500);

        Console.WriteLine($"=== คราฟต์ธนู: {host}:{gamePort} ===");
        c.Send(new Cheat { _Cheat = "maxskills" });
        Pump(c, 1500);

        // รอบ 1 — ต่อเชือก 2 เส้น ต้องได้เชือกยาว (string_long)
        Console.WriteLine("รอบ 1 — ต่อเชือก (extend_rope) ต้องได้เชือกยาว");
        for (int i = 0; i < 4; i++) { c.Send(new Cheat { _Cheat = "give rope 1" }); Pump(c, 350); }
        c.Send(default(GetInventory)); Pump(c, 600);
        List<Item> ropes = Of("rope");
        if (ropes.Count < 4)
        {
            Check("เสกเชือกได้ 4 เส้น", false, "ได้ " + ropes.Count);
            Console.WriteLine($"\nสรุป: ผ่าน {_passed} · ไม่ผ่าน {_failed}");
            c.Close();
            return 1;
        }

        for (int round = 0; round < 2; round++)
        {
            _aborts = 0;
            _crafted.Clear();
            c.Send(default(GetInventory)); Pump(c, 500);
            ropes = Of("rope");
            if (ropes.Count < 2) { break; }
            c.Send(new Craft
            {
                RecipeId = "extend_rope",
                Materials = new Dictionary<string, string[]>
                {
                    { "base",   new[] { ropes[0].Id } },
                    { "string", new[] { ropes[1].Id } }
                },
                ToolItemId = null
            });
            Pump(c, 2500);
        }

        c.Send(default(GetInventory)); Pump(c, 800);
        List<Item> longs = _inv.Where(x => HasTag(x, "string_long")).ToList();
        Check("ต่อเชือกแล้วได้ของที่ติด tag string_long",
            longs.Count > 0,
            longs.Count > 0
                ? $"{longs.Count} ชิ้น · {longs[0].Prototype} [{Tags(longs[0])}]"
                : "ไม่มีเลย — ธนูจะคราฟต์ไม่ได้");

        // รอบ 2 — คราฟต์ธนู
        Console.WriteLine("รอบ 2 — คราฟต์ธนูไม้ (bow_wooden_assembled)");
        c.Send(new Cheat { _Cheat = "give stick_wood_long 1" });
        Pump(c, 600);
        c.Send(default(GetInventory)); Pump(c, 600);
        List<Item> sticks = _inv.Where(x => HasTag(x, "stick_long")).ToList();
        longs = _inv.Where(x => HasTag(x, "string_long")).ToList();
        if (sticks.Count < 1 || longs.Count < 2)
        {
            Check("มีวัตถุดิบครบ (ไม้ยาว 1 + เชือกยาว 2)", false,
                $"ไม้ยาว {sticks.Count} · เชือกยาว {longs.Count}");
        }
        else
        {
            _aborts = 0;
            _crafted.Clear();
            c.Send(new Craft
            {
                RecipeId = "bow_wooden_assembled",
                Materials = new Dictionary<string, string[]>
                {
                    { "main",      new[] { sticks[0].Id } },
                    { "connector", new[] { longs[0].Id, longs[1].Id } }
                },
                ToolItemId = null
            });
            Pump(c, 3000);
            c.Send(default(GetInventory)); Pump(c, 800);
            bool gotBow = _inv.Any(x => (x.Prototype ?? "").Contains("bow"))
                          || _crafted.Any(x => (x.Prototype ?? "").Contains("bow"));
            Check("คราฟต์ธนูสำเร็จ", gotBow && _aborts == 0,
                $"abort={_aborts} · {string.Join(" / ", _infos.TakeLast(2))}");
        }

        Console.WriteLine($"\nสรุป: ผ่าน {_passed} · ไม่ผ่าน {_failed}");
        c.Close();
        return _failed == 0 ? 0 : 1;
    }
}
