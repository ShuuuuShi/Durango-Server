using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Etc;

namespace DurangoTestClient;

/// <summary>S1: ตรวจ building material economy — deposit, build, destruct/refund</summary>
public static class BuildingEconomyCheck
{
    private static int _passed, _failed, _aborts, _oks, _timers;
    private static string _lastBuiltEntityId;
    private static ArtifactMaterials? _lastArtifactMaterials;
    private static Inventory? _inventory;
    private static int _appearArtifacts;

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        // ต้องมีไฟล์เซฟก่อน ไม่งั้น gateway ออก token ผูก id ชั่วคราวแล้ว Auth ปฏิเสธ
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + id + "\"}}");
        if (string.IsNullOrEmpty(token)) return null;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var c = new Connection(socket);
        c.Recv<Welcome>((m, h) => { }); c.Recv<Clock>((m, h) => { }); c.Recv<OK>((m, h) => _oks++);
        c.Recv<Abort>((m, h) => _aborts++); c.Recv<Info>((m, h) => { });
        c.Recv<Inventory>((m, h) => _inventory = m);
        c.Recv<ArtifactMaterials>((m, h) => _lastArtifactMaterials = m);
        c.Recv<ArtifactBuilt>((m, h) => { }); c.Recv<ArtifactCompleted>((m, h) => { });
        c.Recv<Messages.Timer>((m, h) => _timers++); c.Recv<Skills>((m, h) => { });
        c.Recv<Statistics>((m, h) => { }); c.Recv<Survival>((m, h) => { });
        c.Recv<AppearPlayer>((m, h) => { }); c.Recv<AppearAnimal>((m, h) => { });
        c.Recv<AppearArtifact>((m, h) => { _lastBuiltEntityId = m.EntityId; _appearArtifacts++; });
        c.Recv<Move>((m, h) => { }); c.Recv<Teleported>((m, h) => { });
        c.Recv<DefoggedChunks>((m, h) => { }); c.Recv<Chunk>((m, h) => { });
        c.Recv<QuestCategories>((m, h) => { }); c.Recv<WalletUpdated>((m, h) => { });
        c.Recv<Recipes>((m, h) => { }); c.Recv<ArtifactBlueprints>((m, h) => { });
        c.StartReceive();
        c.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(c, 250);
        c.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "build-econ-check" }); Pump(c, 450);
        c.Send(default(Ready)); Pump(c, 1500);
        return c;
    }

    private static int CountPrototype(Item[] items, string prototype)
    {
        int count = 0;
        foreach (Item item in items)
            if (item.Prototype == prototype) count++;
        return count;
    }

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
            "build-econ-" + Guid.NewGuid().ToString("N")[..6], isMale: false, modelInfo);
        if (string.IsNullOrEmpty(id)) { Console.WriteLine("สร้างตัวละครไม่ได้"); return 2; }
        Console.WriteLine($"=== building economy check: {host}:{gamePort} ===");

        Connection c = Connect(host, gamePort, gatewayPort, id);
        if (c == null) { Console.WriteLine("[FAIL] connect failed"); return 2; }

        // ── 1. OccupyArtifactSite → rejected Build (bonfire requires four burnable items) ──
        Console.WriteLine("round 1 — occupy → rejected build (bonfire, no materials)");
        _lastBuiltEntityId = null;
        _appearArtifacts = 0;
        _oks = 0; _aborts = 0;
        c.Send(new OccupyArtifactSite
        {
            BlueprintId = "bonfire",
            Tile = new Point2(42, 177),
            Rotation = Rotation.None
        });
        Pump(c, 1000);
        Check("OccupyArtifactSite creates occupied artifact", _lastBuiltEntityId != null && _appearArtifacts > 0,
            "entity=" + _lastBuiltEntityId + " appear=" + _appearArtifacts);

        if (_lastBuiltEntityId != null)
        {
            string artifactId = _lastBuiltEntityId;

            // GetArtifact — no materials deposited yet
            _lastArtifactMaterials = null;
            c.Send(new GetArtifact { EntityId = artifactId });
            Pump(c, 350);
            Check("GetArtifact returns for occupied artifact", _lastArtifactMaterials.HasValue);

            _timers = 0; _oks = 0; _aborts = 0;
            c.Send(new BuildArtifact { EntityId = artifactId, Tile = new Point2(42, 177) });
            Pump(c, 500);
            Check("BuildArtifact rejects missing required materials", _timers == 0 && _aborts > 0, "timer=" + _timers + " abort=" + _aborts);

            // Destruct
            _oks = 0; _aborts = 0;
            c.Send(new DestructArtifact { EntityId = artifactId, Tile = new Point2(42, 177) });
            Pump(c, 500);
            Check("DestructArtifact succeeds", _aborts == 0, "abort=" + _aborts);
        }

        // ── 2. Deposit materials → Build → Destruct (bonfire needs four burnable items) ──
        Console.WriteLine("round 2 — deposit → build → destruct (bonfire with materials)");
        _oks = 0;
        for (int i = 0; i < 4; i++)
        {
            c.Send(new Cheat { _Cheat = "give leaf" });
            Pump(c, 300);
        }

        // Occupy another bonfire artifact — different tile
        _lastBuiltEntityId = null;
        _appearArtifacts = 0;
        _oks = 0; _aborts = 0;
        c.Send(new OccupyArtifactSite
        {
            BlueprintId = "bonfire",
            Tile = new Point2(43, 177),
            Rotation = Rotation.None
        });
        Pump(c, 1000);
        Check("OccupyArtifactSite creates 2nd bonfire", _lastBuiltEntityId != null, "entity=" + _lastBuiltEntityId);

        if (_lastBuiltEntityId != null)
        {
            string boxId = _lastBuiltEntityId;

            // Find four burnable leaf items in inventory.
            var leafIds = new List<string>();
            if (_inventory.HasValue && _inventory.Value.InventoryItems.Items != null)
            {
                foreach (Item it in _inventory.Value.InventoryItems.Items)
                    if (it.Prototype == "leaf") leafIds.Add(it.Id);
            }
            Check("have four leaves in inventory", leafIds.Count >= 4, "leaves=" + leafIds.Count);

            if (leafIds.Count >= 4)
            {
                // Deposit the authoritative bonfire requirement (four burnable items).
                _oks = 0; _aborts = 0;
                var materials = new Dictionary<string, string[]> { ["main"] = leafIds.GetRange(0, 4).ToArray() };
                c.Send(new PutMaterialsIntoArtifact
                {
                    EntityId = boxId,
                    Tile = new Point2(43, 177),
                    Materials = materials
                });
                Pump(c, 500);
                Check("PutMaterialsIntoArtifact succeeds", _oks > 0 && _aborts == 0, "ok=" + _oks + " abort=" + _aborts);

                // Verify materials deposited
                _lastArtifactMaterials = null;
                c.Send(new GetArtifact { EntityId = boxId });
                Pump(c, 350);
                bool hasDeposit = _lastArtifactMaterials.HasValue
                    && _lastArtifactMaterials.Value.Materials != null
                    && _lastArtifactMaterials.Value.Materials.ContainsKey("main");
                Check("GetArtifact shows deposited materials", hasDeposit);

                // A nearby second player knows the entity id but must not inspect or mutate the owner's ledger.
                // ต้องสร้างตัวละครจริงเหมือนคนแรก ไม่งั้น Auth ตกแล้ว packet ไม่ถูกประมวลผล
                // (ทำให้ทุกข้อ "คนอื่นต้องถูกปฏิเสธ" ผ่านแบบหลอก ๆ เพราะไม่มี abort กลับมาเลย)
                string intruderId = CreateCharacterCheck.CreatePlayer(host, gatewayPort,
                    "build-intruder-" + Guid.NewGuid().ToString("N")[..6], isMale: false, modelInfo);
                Connection intruder = Connect(host, gamePort, gatewayPort, intruderId);
                Check("second client connects", intruder != null);
                if (intruder != null)
                {
                    _lastArtifactMaterials = null; _aborts = 0;
                    intruder.Send(new GetArtifact { EntityId = boxId });
                    Pump(intruder, 400);
                    Check("second client cannot read construction ledger", _aborts > 0 && !_lastArtifactMaterials.HasValue, "abort=" + _aborts);

                    _aborts = 0;
                    intruder.Send(new BuildArtifact { EntityId = boxId, Tile = new Point2(43, 177) });
                    Pump(intruder, 400);
                    Check("second client cannot build owner's artifact", _aborts > 0, "abort=" + _aborts);

                    _aborts = 0;
                    intruder.Send(new DestructArtifact { EntityId = boxId, Tile = new Point2(43, 177) });
                    Pump(intruder, 400);
                    Check("second client cannot demolish owner's artifact", _aborts > 0, "abort=" + _aborts);
                    intruder.Close();
                    c.Send(default(GetInventory));
                    Pump(c, 300);
                }

                // A fully reserved bonfire builds once, then demolition refunds its completed ledger.
                _timers = 0; _aborts = 0;
                c.Send(new BuildArtifact { EntityId = boxId, Tile = new Point2(43, 177) });
                Pump(c, 3000);
                Check("BuildArtifact succeeds with complete authoritative materials", _timers > 0 && _aborts == 0,
                    "timer=" + _timers + " abort=" + _aborts);

                _oks = 0; _aborts = 0;
                c.Send(new DestructArtifact { EntityId = boxId, Tile = new Point2(43, 177) });
                Pump(c, 500);
                bool refunded = _inventory.HasValue && _inventory.Value.InventoryItems.Items != null
                    && CountPrototype(_inventory.Value.InventoryItems.Items, "leaf") >= 4;
                Check("DestructArtifact refunds completed construction ledger", _aborts == 0 && refunded,
                    "abort=" + _aborts + " leaves=" + (_inventory.HasValue ? CountPrototype(_inventory.Value.InventoryItems.Items, "leaf") : 0));
            }
        }

        c.Close();
        Console.WriteLine($"=== building economy result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
