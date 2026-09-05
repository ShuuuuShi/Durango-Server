using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Item;
using Shared.Skill;

namespace DurangoTestClient;

/// <summary>End-to-end packet/reconnect coverage for the remaining character systems.</summary>
public static class CharacterSystemsCheck
{
    private static int _passed, _failed, _aborts;
    private static Inventory? _inventory;
    private static Equipments? _equipments;
    private static PlayerDisplay? _display;
    private static Points? _points;
    private static bool _revived;
    private static string[] _lastOrder;

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine("  [PASS] " + name + (detail == null ? "" : " - " + detail)); }
        else { _failed++; Console.WriteLine("  [FAIL] " + name + (detail == null ? "" : " - " + detail)); }
    }

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, "CharacterCheck");
        if (string.IsNullOrEmpty(token)) return null;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var c = new Connection(socket);
        c.Recv<Welcome>((m, h) => { }); c.Recv<Clock>((m, h) => { }); c.Recv<OK>((m, h) => { });
        c.Recv<Abort>((m, h) => _aborts++);
        c.Recv<Inventory>((m, h) => _inventory = m);
        c.Recv<InventoryUpdated>((m, h) => { if (m.ItemOrder != null) _lastOrder = m.ItemOrder; });
        c.Recv<Equipments>((m, h) => _equipments = m);
        c.Recv<PlayerDisplay>((m, h) => _display = m);
        c.Recv<Points>((m, h) => _points = m);
        c.Recv<Revived>((m, h) => _revived = true);
        c.Recv<Messages.Timer>((m, h) => { }); c.Recv<Info>((m, h) => { });
        c.Recv<Skills>((m, h) => { }); c.Recv<Statistics>((m, h) => { });
        c.Recv<Survival>((m, h) => { }); c.Recv<SurvivalUpdated>((m, h) => { });
        c.Recv<AppearPlayer>((m, h) => { if (m.EntityId == id) _display = m.Display; }); c.Recv<EntityDied>((m, h) => { });
        c.Recv<EntityRevived>((m, h) => { }); c.Recv<Move>((m, h) => { });
        c.Recv<Teleported>((m, h) => { }); c.Recv<DefoggedChunks>((m, h) => { });
        c.Recv<QuestCategories>((m, h) => { }); c.Recv<WalletUpdated>((m, h) => { });
        c.Recv<Recipes>((m, h) => { }); c.Recv<ArtifactBlueprints>((m, h) => { });
        c.Recv<Chunk>((m, h) => { }); c.Recv<AppearAnimal>((m, h) => { });
        c.StartReceive();
        c.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(c, 250);
        c.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "character-check" }); Pump(c, 450);
        c.Send(default(Ready)); Pump(c, 1700);
        return c;
    }

    private static Item[] Items => _inventory?.InventoryItems.Items ?? Array.Empty<Item>();

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = _aborts = 0;
        string id = "character-" + Guid.NewGuid().ToString("N")[..8];
        Console.WriteLine($"=== character systems check: {host}:{gamePort} ===");
        Connection c = Connect(host, gamePort, gatewayPort, id);
        if (c == null) return 2;

        Item axe = Items.FirstOrDefault(x => x.Prototype == "axe_onehand_stone_01");
        Check("starter durable weapon exists", !string.IsNullOrEmpty(axe.Id) && axe.RepairRequirement.HasValue);

        c.Send(new LockOrUnlockItems { Lock = true, ItemIds = new[] { axe.Id } }); Pump(c, 400);
        int beforeDump = _aborts;
        c.Send(new DumpItems { ItemIds = new[] { axe.Id }, Tile = new Point2(0, 0) }); Pump(c, 400);
        Check("locked item cannot be discarded", _aborts > beforeDump && Items.Any(x => x.Id == axe.Id));

        string[] reversed = Items.Select(x => x.Id).Reverse().ToArray();
        int beforeOrder = _aborts;
        c.Send(new InventoryOrder { ItemOrder = reversed }); Pump(c, 400);
        Check("inventory order is accepted", _aborts == beforeOrder && (_lastOrder?.SequenceEqual(reversed) ?? false));

        c.Send(new Cheat { _Cheat = "give bow_wooden_01" }); Pump(c, 450);
        Item bow = Items.FirstOrDefault(x => x.Prototype == "bow_wooden_01");
        c.Send(new Equip { SlotName = "main", SlotType = EquipSlotType.Slot1, ItemId = axe.Id, Action = "equip" }); Pump(c, 350);
        c.Send(new Equip { SlotName = "both", SlotType = EquipSlotType.Slot2, ItemId = bow.Id, Action = "equip" }); Pump(c, 350);
        c.Send(default(GetEquipments)); Pump(c, 350);
        bool presets = _equipments?.Presets?.ContainsKey(EquipSlotType.Slot1) == true
            && _equipments?.Presets?.ContainsKey(EquipSlotType.Slot2) == true
            && _equipments?.Presets?.ContainsKey(EquipSlotType.Slot3) == true;
        Check("three equipment presets are returned", presets);
        c.Send(new ChangeEquipSlotType { SlotType = EquipSlotType.Slot2 }); Pump(c, 400);
        Check("active equipment preset changes", _equipments?.CurrentType == EquipSlotType.Slot2);

        c.Send(new AttachAccessory { AccessoryId = "character_check_accessory" }); Pump(c, 350);
        Check("accessory is reflected in player display", _display?.Accessory == "character_check_accessory");

        float full = bow.Durability?.RealMax() ?? 0;
        c.Send(new Cheat { _Cheat = "die" }); Pump(c, 500);
        bow = Items.FirstOrDefault(x => x.Id == bow.Id);
        float worn = bow.Durability?.Get(0) ?? full;
        Check("death creates a death point", _points?.DeathPoint.HasValue == true);
        Check("death wears current equipment by 10 percent", full > 0 && worn < full, $"{worn:F1}/{full:F1}");

        c.Send(new Cheat { _Cheat = "give tool_repair_kit_01" }); Pump(c, 450);
        Item kit = Items.FirstOrDefault(x => x.Prototype == "tool_repair_kit_01");
        int beforeRepair = _aborts;
        c.Send(new ReviveImmediately { VoucherId = null }); Pump(c, 350);
        c.Send(new RepairItem { ItemId = bow.Id, KitItemIds = new[] { kit.Id } }); Pump(c, 1400);
        bow = Items.FirstOrDefault(x => x.Id == bow.Id);
        Check("immediate revive works", _revived);
        Check("repair request succeeds", _aborts == beforeRepair);
        Check("repair restores full durability and consumes kit",
            Math.Abs((bow.Durability?.Get(0) ?? 0) - full) < 0.01f && !Items.Any(x => x.Id == kit.Id));

        int invalidResearch = _aborts;
        c.Send(new ResearchSkillCategory { Category = Category.Invalid, SkipCategory = null }); Pump(c, 300);
        Check("invalid skill research is rejected", _aborts > invalidResearch);

        c.Close(); Thread.Sleep(1200);
        _inventory = null; _equipments = null; _display = null;
        Connection reconnect = Connect(host, gamePort, gatewayPort, id);
        if (reconnect == null) return 2;
        reconnect.Send(default(GetEquipments)); Pump(reconnect, 450);
        Check("active equipment preset survives reconnect", _equipments?.CurrentType == EquipSlotType.Slot2);
        Check("inventory lock survives reconnect", _inventory?.InventoryInfos.LockedItemIds?.Contains(axe.Id) == true);
        string[] survivingOriginal = reversed.Where(x => Items.Any(item => item.Id == x)).ToArray();
        string[] loadedOrder = _inventory?.InventoryInfos.ItemOrder ?? Array.Empty<string>();
        Check("inventory order survives reconnect", loadedOrder.Take(survivingOriginal.Length).SequenceEqual(survivingOriginal));
        Check("accessory survives reconnect", _display?.Accessory == "character_check_accessory");
        reconnect.Send(default(RemoveDeathPoint)); Pump(reconnect, 350);
        Check("death point can be removed", _points?.DeathPoint.HasValue == false);
        reconnect.Close();

        Console.WriteLine($"=== character systems result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
