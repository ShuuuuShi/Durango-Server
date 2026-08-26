using System;
using System.Collections.Generic;
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

/// <summary>End-to-end packet and reconnect check for character systems group 2.</summary>
public static class Group2Check
{
    private static int _passed;
    private static int _failed;
    private static Statistics? _statistics;
    private static Survival? _survival;
    private static Messages.StatusEffects? _effects;
    private static Titles? _titles;
    private static ResistanceExpCaps? _caps;
    private static TargetTitle? _target;
    private static Messages.Title? _title;
    private static AppearPlayer? _self;
    private static int _aborts;
    private static Item[] _inventory = Array.Empty<Item>();
    private static Actions? _actions;
    private static string _lastAnimalId;
    /// <summary>ตัวแรกที่โผล่หลังสั่ง `cheat spawn` — คือตัวที่เสกจริง (เกิดตรงตำแหน่งเรา)</summary>
    private static string _spawnedAnimalId;
    private static readonly List<string> _infos = new List<string>();
    private static Skills? _skills;

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++)
        {
            connection.Process();
            Thread.Sleep(10);
        }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine("  [PASS] " + name + (detail == null ? "" : " - " + detail)); }
        else { _failed++; Console.WriteLine("  [FAIL] " + name + (detail == null ? "" : " - " + detail)); }
    }

    private static Connection Connect(string host, int gamePort, int gatewayPort, string id, string claimedName)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, claimedName);
        if (string.IsNullOrEmpty(token)) return null;

        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        connection.Recv<Welcome>((m, h) => { });
        connection.Recv<Clock>((m, h) => { });
        connection.Recv<OK>((m, h) => { });
        connection.Recv<Abort>((m, h) => _aborts++);
        connection.Recv<Statistics>((m, h) => _statistics = m);
        connection.Recv<Survival>((m, h) => _survival = m);
        connection.Recv<Messages.StatusEffects>((m, h) => _effects = m);
        connection.Recv<Titles>((m, h) => _titles = m);
        connection.Recv<ResistanceExpCaps>((m, h) => _caps = m);
        connection.Recv<TargetTitle>((m, h) => _target = m);
        connection.Recv<Messages.Title>((m, h) => _title = m);
        connection.Recv<AppearPlayer>((m, h) => { if (m.EntityId == id) _self = m; });
        connection.Recv<Skills>((m, h) => _skills = m);
        connection.Recv<Inventory>((m, h) => _inventory = m.InventoryItems.Items ?? Array.Empty<Item>());
        connection.Recv<Equipments>((m, h) => { });
        connection.Recv<PlayerDisplay>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { });
        connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { });
        // 🐛 เดิมเก็บ "ตัวล่าสุดที่ appear" ⇒ พอ server มีระบบระยะมองเห็นแล้ว สัตว์เดินเข้า/ออกจอ
        //    ตลอดเวลา AppearAnimal จึงมาเรื่อย ๆ และตัวล่าสุดมักเป็น **ตัวที่เพิ่งเดินผ่านมาไกล ๆ**
        //    ไม่ใช่ตัวที่เราเพิ่งเสก ⇒ ยิงธนูแล้วได้ "เป้าหมายไกลไป (4745 > 1900)" แบบสุ่ม
        //    ตอนนี้เก็บ "ตัวแรกที่ appear หลังสั่งเสก" แทน
        connection.Recv<AppearAnimal>((m, h) =>
        {
            _lastAnimalId = m.EntityId;
            if (_spawnedAnimalId == null) _spawnedAnimalId = m.EntityId;
        });
        connection.Recv<AppearArtifact>((m, h) => { });
        connection.Recv<Move>((m, h) => { });
        connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { });
        connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<ExpGained>((m, h) => { });
        connection.Recv<SurvivalUpdated>((m, h) => { });
        connection.Recv<Actions>((m, h) => _actions = m);
        connection.Recv<InventoryUpdated>((m, h) => { });
        connection.Recv<ItemUsed>((m, h) => { });
        connection.Recv<Info>((m, h) => { if (m.Text != null) _infos.Add(m.Text); });
        connection.Recv<Damaged>((m, h) => { });
        connection.Recv<BattleBegun>((m, h) => { });
        connection.Recv<BattleEnded>((m, h) => { });
        connection.StartReceive();

        connection.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(connection, 300);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "group2-check" });
        Pump(connection, 500);
        connection.Send(default(Ready));
        Pump(connection, 1800);
        return connection;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== group 2 check: {host}:{gamePort} ===");
        string id = "group2-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string originalName = "GroupTwo";
        string renamed = "GroupTwoRenamed";

        Connection connection = Connect(host, gamePort, gatewayPort, id, originalName);
        if (connection == null) return 2;

        connection.Send(default(GetStatistics));
        connection.Send(default(GetStatusEffects));
        connection.Send(default(GetResistanceExpCaps));
        connection.Send(default(GetTitles));
        connection.Send(default(GetTargetTitle));
        Pump(connection, 1000);

        Check("hungry gauge is sent", _survival?.Gauges != null && _survival.Value.Gauges.ContainsKey("hungry"));
        Check("resistance levels/exps are populated",
            _statistics?.ResistanceLevels?.Count > 0 && _statistics?.ResistanceExps?.Count > 0);
        Check("resistance caps reply is populated", _caps?.Caps?.Count > 0);
        Check("starter title is unlocked", _titles?.TitleIds?.Contains("combat_basic_1") == true);
        Check("status effect list replies", _effects.HasValue && _effects.Value.EntityId == id);

        connection.Send(new ToggleStatusEffect { Id = "away_from_keyboard", Toggle = true });
        Pump(connection, 500);
        Check("toggle status effect changes live list",
            _effects.HasValue && _effects.Value._StatusEffects?.Any(x => x.EffectId == "away_from_keyboard") == true);

        // Food uses real inventory + UseItem and must create the food status effect.
        connection.Send(new Cheat { _Cheat = "give belly_steak" });
        Pump(connection, 500);
        Item steak = _inventory.FirstOrDefault(x => x.Prototype == "belly_steak");
        int foodAborts = _aborts;
        if (!string.IsNullOrEmpty(steak.Id))
        {
            connection.Send(new UseItem { ItemId = steak.Id, Accept = true });
            Pump(connection, 700);
        }
        Check("food can be consumed through UseItem", !string.IsNullOrEmpty(steak.Id) && _aborts == foodAborts);
        Check("food creates its real status effect",
            _effects.HasValue && _effects.Value._StatusEffects?.Any(x => x.EffectId == "hot_food") == true);

        // Bow/crossbow attacks require an actual arrow and earn ranged proficiency on a kill.
        connection.Send(new Cheat { _Cheat = "give bow_wooden_01" });
        Pump(connection, 400);
        Item bow = _inventory.FirstOrDefault(x => x.Prototype == "bow_wooden_01");
        if (!string.IsNullOrEmpty(bow.Id))
        {
            connection.Send(new Equip { SlotName = "both", SlotType = EquipSlotType.Slot1, ItemId = bow.Id, Action = "equip" });
            Pump(connection, 500);
        }
        _spawnedAnimalId = null;
        _infos.Clear();
        connection.Send(new Cheat { _Cheat = "spawn 2001" });
        Pump(connection, 800);
        // server แนบ [id=...] มากับข้อความตอบ — ใช้ id นั้นตรง ๆ หมดปัญหาเดาผิดตัว
        foreach (string line in _infos)
        {
            int at = line.IndexOf("[id=", StringComparison.Ordinal);
            if (at < 0) continue;
            int end = line.IndexOf(']', at);
            if (end > at) { _spawnedAnimalId = line.Substring(at + 4, end - at - 4); break; }
        }
        connection.Send(default(GetActions));
        Pump(connection, 400);
        string actionId = _actions?.BattleActions?.FirstOrDefault().Id;
        int noAmmoAborts = _aborts;
        if (!string.IsNullOrEmpty(actionId) && !string.IsNullOrEmpty(_spawnedAnimalId ?? _lastAnimalId))
        {
            connection.Send(new UseBattleAction
            {
                ActionId = actionId, StartAt = Times.UnixTimeNow(), TargetEntityId = _spawnedAnimalId ?? _lastAnimalId, TargetTile = null
            });
            Pump(connection, 500);
        }
        Check("ranged attack without ammo is rejected", _aborts > noAmmoAborts);

        connection.Send(new Cheat { _Cheat = "give gunpowder_arrow" });
        Pump(connection, 400);
        int withAmmoAborts = _aborts;
        if (!string.IsNullOrEmpty(actionId) && !string.IsNullOrEmpty(_spawnedAnimalId ?? _lastAnimalId))
        {
            connection.Send(new UseBattleAction
            {
                ActionId = actionId, StartAt = Times.UnixTimeNow(), TargetEntityId = _spawnedAnimalId ?? _lastAnimalId, TargetTile = null
            });
            Pump(connection, 1800);
        }
        Check("ranged attack with ammo is accepted", _aborts == withAmmoAborts);
        Check("ranged attack consumes one arrow", !_inventory.Any(x => x.Prototype == "gunpowder_arrow"));
        bool rangedProgress = _skills?.Categories != null
            && _skills.Value.Categories.TryGetValue(Category.RangedCombat, out SkillCategory ranged)
            && (ranged.Exp > 0 || ranged.Level > 0);
        Check("ranged kill grants ranged proficiency", rangedProgress);

        connection.Send(new SelectTitle { TitleId = "combat_basic_1" });
        Pump(connection, 500);
        Check("selected title is pushed", _title?.TitleId == "combat_basic_1");

        connection.Send(new SelectTargetTitle { TitleId = "combat_basic_1" });
        Pump(connection, 400);
        connection.Send(default(GetTargetTitle));
        Pump(connection, 400);
        Check("target title round-trips", _target?.TitleId == "combat_basic_1");

        int abortsBeforeRename = _aborts;
        connection.Send(new Rename { EntityId = id, Name = renamed, PrevName = originalName, IsFirstRename = true });
        Pump(connection, 700);
        Check("character rename is accepted", _aborts == abortsBeforeRename);
        Check("renamed appearance is broadcast", _self?.Name == renamed, _self?.Name);

        connection.Close();
        Thread.Sleep(1400); // server disconnect path saves the player

        _self = null;
        _title = null;
        _target = null;
        _effects = null;
        Connection reconnect = Connect(host, gamePort, gatewayPort, id, originalName);
        if (reconnect == null) return 2;
        reconnect.Send(default(GetStatusEffects));
        reconnect.Send(default(GetTargetTitle));
        Pump(reconnect, 800);
        Check("renamed name survives reconnect", _self?.Name == renamed, _self?.Name);
        Check("selected title survives reconnect", _self?.Title.TitleId == "combat_basic_1", _self?.Title.TitleId);
        Check("target title survives reconnect", _target?.TitleId == "combat_basic_1");
        Check("toggled status survives reconnect",
            _effects.HasValue && _effects.Value._StatusEffects?.Any(x => x.EffectId == "away_from_keyboard") == true);
        reconnect.Close();

        Console.WriteLine($"=== group 2 result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
