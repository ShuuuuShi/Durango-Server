using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Teleport;

namespace DurangoServer.Core;

/// <summary>
/// รีโมทคุมตัวละครของผู้เล่นคนอื่น (แบบ OpenKore — สั่งด้วย packet ไม่ใช่เมาส์/คีย์บอร์ด)
///
/// ทำไมต้องทำที่ server: ตัวเกมจริง **ไม่ยอมให้ server สั่ง <c>Move</c> ตัวเราเอง**
/// (`PlayerManager.HandleMoveMsg` หา entity ใน `_players` ซึ่งไม่มีตัวเราอยู่ในนั้น)
/// แต่ยอมรับ <c>Teleported</c> เสมอ — `PlayerController.Teleport()` ย้ายตัวละครท้องถิ่นทันที
/// การเดินจึงทำด้วยการ "วาร์ปทีละก้าว" ถี่ ๆ ให้ดูเหมือนเดิน แล้ว broadcast Move ให้คนอื่นเห็นลื่น ๆ
///
/// สั่งผ่าน cheat: <c>control &lt;ชื่อ|entityId&gt; &lt;คำสั่ง&gt;</c> — ดู docs/server/RemoteControl.md
/// </summary>
public partial class ServerPlayer
{
    /// <summary>ก้าวละกี่วินาที (ถี่กว่านี้ client จะกระตุก)</summary>
    private const double StepInterval = 0.35;

    /// <summary>ระยะต่อก้าว (หน่วยโลก) — 1 tile = 200</summary>
    private const float StepDistance = 200f;

    /// <summary>เดินได้สูงสุดกี่ก้าวต่อคำสั่ง (กันสั่งเดินข้ามแมพแล้ว deferred บวม)</summary>
    private const int MaxWalkSteps = 120;

    /// <summary>คิวเดินที่ยังเหลือ (ยกเลิกได้ด้วยคำสั่ง stop)</summary>
    private int _walkToken;

    /// <summary>ย้ายตัวละครไป tile นี้ทันที (client ยอมรับ Teleported เสมอ)</summary>
    public void ControlTeleport(int tileX, int tileY)
    {
        var pos = new WorldPosition(tileX * 200f + 100f, tileY * 200f + 100f);
        Send(new Teleported { Tile = new Point2(tileX, tileY), Type = TeleportType.Unknown });
        StopResting();
        RememberPosition(pos, _lastYaw);
        // คนอื่นไม่ได้รับ Teleported ของเรา ต้องยิง Move ให้เห็นว่าเราย้ายที่
        _world.BroadcastToViewers(EntityId, MakeStepMove(pos, 0.2), except: this);
        Console.WriteLine("[control] {0} → tile {1},{2}", Name, tileX, tileY);
    }

    /// <summary>เดินไป tile ปลายทางทีละก้าว (ยกเลิกด้วย ControlStop)</summary>
    public void ControlWalk(int tileX, int tileY)
    {
        int token = ++_walkToken;
        var dest = new WorldPosition(tileX * 200f + 100f, tileY * 200f + 100f);
        Console.WriteLine("[control] {0} เดินไป tile {1},{2}", Name, tileX, tileY);
        ScheduleStep(token, dest, 0);
    }

    /// <summary>หยุดเดิน</summary>
    public void ControlStop()
    {
        _walkToken++;
        Console.WriteLine("[control] {0} หยุดเดิน", Name);
    }

    private void ScheduleStep(int token, WorldPosition dest, int stepIndex)
    {
        _deferred.Add((Times.UnixTimeNow() + StepInterval, () =>
        {
            if (token != _walkToken || Dead)
            {
                return;                     // มีคำสั่งใหม่มาแทน หรือเราตายไปแล้ว
            }
            WorldPosition me = CurrentPosition;
            float dx = dest.x - me.x;
            float dy = dest.y - me.y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist <= StepDistance || stepIndex >= MaxWalkSteps)
            {
                ControlTeleport((int)(dest.x / 200f), (int)(dest.y / 200f));
                return;
            }
            var next = new WorldPosition(me.x + dx / dist * StepDistance, me.y + dy / dist * StepDistance);
            Send(new Teleported { Tile = new Point2((int)(next.x / 200f), (int)(next.y / 200f)), Type = TeleportType.Unknown });
            RememberPosition(next, MathF.Atan2(dx, dy) * (180f / MathF.PI));
            _world.BroadcastToViewers(EntityId, MakeStepMove(next, StepInterval), except: this);
            ScheduleStep(token, dest, stepIndex + 1);
        }));
    }

    /// <summary>packet Move สำหรับให้ "คนอื่น" เห็นเราเคลื่อนที่ (ตัวเราเองใช้ Teleported)</summary>
    private Move MakeStepMove(WorldPosition to, double seconds)
    {
        double now = Times.UnixTimeNow();
        return new Move
        {
            EntityId = EntityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = "Barehand_Walk",
                    MotionOption = 5,
                    PlaybackRate = 1f,
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        new Location { Position = CurrentPosition, Yaw = _lastYaw, Time = now, Floor = 0, Height = 0f },
                        new Location { Position = to, Yaw = _lastYaw, Time = now + seconds, Floor = 0, Height = 0f }
                    }
                }
            }
        };
    }

    /// <summary>แตะของธรรมชาติที่ใกล้ที่สุดที่เอื้อมถึง แล้วเก็บ 1 ครั้ง</summary>
    public string ControlGather()
    {
        if (!_world.Terrain.TryFindNaturalNear(CurrentPosition, MaxReachTiles, out Point2 tile, out ushort type))
        {
            return "ไม่มีของธรรมชาติในระยะเอื้อม";
        }
        var touch = new Touch
        {
            EntityId = $"natural_{tile.x}_{tile.y}",
            EntityType = type,
            Tile = tile
        };
        HandleTouch(touch, default);
        Generator[] gens = _world.PeekGenerators(touch.EntityId);
        if (gens == null || gens.Length == 0)
        {
            return $"แตะ tile {tile.x},{tile.y} แล้วแต่ไม่มีอะไรให้เก็บ";
        }
        HandleCollect(new Collect { EntityId = touch.EntityId, GeneratorId = gens[0].Id, Tile = tile }, default);
        return $"เก็บ {gens[0].Name} ที่ tile {tile.x},{tile.y}";
    }

    /// <summary>ตีสัตว์ที่ใกล้ที่สุดด้วยท่าแรกของอาวุธที่ถืออยู่</summary>
    public string ControlAttackNearest()
    {
        ServerAnimal[] animals = _world.Animals.Snapshot();
        ServerAnimal best = null;
        float bestDist = float.MaxValue;
        WorldPosition me = CurrentPosition;
        for (int i = 0; i < animals.Length; i++)
        {
            if (!animals[i].IsAlive)
            {
                continue;
            }
            float dx = animals[i].Position.x - me.x;
            float dy = animals[i].Position.y - me.y;
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d < bestDist)
            {
                bestDist = d;
                best = animals[i];
            }
        }
        if (best == null)
        {
            return "ไม่มีสัตว์ในโลก";
        }
        string[] actions = ActionData.ForWeaponTag(CurrentWeaponTag());
        if (actions.Length == 0)
        {
            return "ไม่มีท่าโจมตี";
        }
        HandleUseBattleAction(new UseBattleAction
        {
            ActionId = actions[0],
            StartAt = Times.UnixTimeNow(),
            TargetEntityId = best.EntityId,
            TargetTile = new Point2((int)(best.Position.x / 200f), (int)(best.Position.y / 200f))
        }, default);
        return $"ตี {best.EntityId} (type {best.EntityType} lv{best.Level}, ห่าง {bestDist / 200f:F1} tile)";
    }

    /// <summary>
    /// เรียกสัตว์มาเกิด "ข้างตัวผู้เล่นคนนั้น" (ไม่ใช่ข้างตัวคนสั่ง)
    /// เอาไว้เทสตอนตัวเองยืนอยู่ในเกม แล้วสั่งจากเครื่องมือข้างนอกให้สัตว์โผล่ตรงหน้า
    /// </summary>
    public string ControlSpawn(ushort entityType)
    {
        ServerAnimal born = _world.Animals.SpawnAt(CurrentPosition, entityType, CurrentHeight);
        SpawnTable.Entry e = SpawnTable.Find(born.EntityType);
        return $"เรียก {e?.Name ?? ("type " + born.EntityType)} lv{born.Level} มาเกิดข้าง {Name}";
    }

    /// <summary>ฆ่าสัตว์ตัวที่ใกล้ผู้เล่นคนนั้นที่สุด — ได้ซากไว้เทสการแล่เนื้อทันที</summary>
    public string ControlKillNearest()
    {
        ServerAnimal[] animals = _world.Animals.Snapshot();
        ServerAnimal best = null;
        float bestDist = float.MaxValue;
        WorldPosition me = CurrentPosition;
        for (int i = 0; i < animals.Length; i++)
        {
            if (!animals[i].IsAlive)
            {
                continue;
            }
            float dx = animals[i].Position.x - me.x;
            float dy = animals[i].Position.y - me.y;
            float d = MathF.Sqrt(dx * dx + dy * dy);
            if (d < bestDist)
            {
                bestDist = d;
                best = animals[i];
            }
        }
        if (best == null)
        {
            return "ไม่มีสัตว์เป็น ๆ ในโลก";
        }
        // ให้เครดิตคนที่ถูกสั่ง ไม่ใช่ admin — ซากจะได้เรืองแสงให้คนที่ยืนอยู่ตรงนั้น
        _world.Animals.Damage(best.EntityId, best.LifeMax * 2f, EntityId);
        return $"ฆ่า {best.EntityId} (type {best.EntityType} lv{best.Level}) ห่าง {bestDist / 200f:F1} tile — แตะซากเพื่อแล่ได้เลย";
    }

    /// <summary>เติมเลือด/สตามินาให้เต็ม + ล้างความล้า (ฟื้นให้ด้วยถ้าตายอยู่)</summary>
    public string ControlHeal()
    {
        bool wasDead = Dead;
        if (wasDead)
        {
            ReviveAtSpawn();
        }
        else
        {
            RestoreSurvival(clearFatigue: true);
        }
        return wasDead ? $"ฟื้น {Name} แล้ว (วาร์ปกลับจุดเกิด)" : $"เติมเลือด/สตามินา/ล้างความล้าให้ {Name} แล้ว";
    }

    /// <summary>เสกของทดสอบให้ผู้เล่นคนนั้น</summary>
    public string ControlGive(string what, int count = 1)
    {
        switch (what)
        {
            case "axe":
                GiveEquipTestItem("axe_onehand_stone_01", "ขวานหิน", "weapon_axe_onehand_stone_2", 0);
                return $"ให้ขวานหินกับ {Name}";
            case "clothes":
                GiveEquipTestItem("clothes_builder_01", "ชุดช่าง", "clothes_builder_01", 0);
                return $"ให้ชุดช่างกับ {Name}";
            case "bonfire":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_bonfire", "กองไฟ", "furniture_workbench_bonfire"));
                }
                MarkDirty();
                SendInventory();
                return $"ให้กองไฟกับ {Name}";
            case "tent":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_tent", "เต็นท์", "building_house_tent"));
                }
                MarkDirty();
                SendInventory();
                return $"ให้เต็นท์กับ {Name}";
            case "temptent":
            case "temp tent":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_temptent", "เต็นท์ชั่วคราว", "building_house_temp"));
                }
                MarkDirty();
                SendInventory();
                return $"ให้เต็นท์ชั่วคราวกับ {Name}";
            case "worktable":
            case "fur_table":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_fur_table", "โต๊ะคราฟต์", "furniture_fur_table_01"));
                }
                MarkDirty();
                SendInventory();
                return $"ให้โต๊ะคราฟต์กับ {Name}";
            case "box":
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_fur_box_03_leaf", "กล่องใบไม้", "furniture_box"));
                }
                MarkDirty();
                SendInventory();
                return $"ให้กล่องใบไม้กับ {Name}";
            // วัตถุดิบพื้นฐาน — ไว้เทสสายคราฟต์ในเกมจริง (หิน 5 ก้อน = คราฟต์มีดหินได้เลย)
            case "stone":
                for (int i = 0; i < 5; i++)
                {
                    GiveEquipTestItem("stone", "หิน", "icon_nat_stone", 0);
                }
                return $"ให้หิน 5 ก้อนกับ {Name} (คราฟต์ใบมีดหินได้เลย)";
            case "knife":
                GiveEquipTestItem("blade_stone", "ใบมีดหิน", "icon_nat_blade_stone", 0);
                return $"ให้ใบมีดหินกับ {Name}";
            // ชุดทำอาหารครบเซ็ต — ไว้เทสเช็คลิสต์ทำอาหารโดยไม่ต้องออกไปล่า/ขุดดินเอง
            case "cook":
            case "cookkit":
                GiveByPrototype("meat", 3, out _);
                GiveByPrototype("wood_bough", 2, out _);
                GiveByPrototype("water", 2, out _);
                GiveByPrototype("pot_01", 1, out _);
                GiveByPrototype("grill_stone", 1, out _);
                lock (_inventory)
                {
                    _inventory.Add(MakeCapsuleItem("capsulated_bonfire", "กองไฟ", "furniture_workbench_bonfire"));
                    _inventory.Add(MakeCapsuleItem("capsulated_bonfire_01", "กองไฟใหญ่", "furniture_workbench_bonfire_01"));
                }
                MarkDirty();
                SendInventory();
                return $"ให้ชุดทำอาหารกับ {Name} — เนื้อ 3 · กิ่งไม้ 2 · น้ำ 2 · หม้อ · เตาย่าง · กองไฟ + กองไฟใหญ่";
            default:
                // ชื่อ prototype ตรง ๆ ก็ให้ได้ (`control <ชื่อ> give meat`) — เทสสูตรไหนก็เสกของนั้น
                if (GiveByPrototype(what, count, out int given))
                {
                    string msg = $"ให้ {ItemNameData.NameOf(what, what)} x{given} กับ {Name}";
                    if (given < count)
                    {
                        msg += $" (ขอ {count} แต่กระเป๋าเหลือที่แค่ {given} ช่อง)";
                    }
                    return msg;
                }
                return "ให้ได้: axe · clothes · bonfire · box · stone · knife · cook (ชุดทำอาหาร) หรือชื่อ prototype ตรง ๆ";
        }
    }

    /// <summary>ช่องกระเป๋าที่ยังว่างอยู่</summary>
    public int FreeInventorySlots()
    {
        lock (_inventory)
        {
            return Math.Max(0, PlayerInventoryMaxSize - _inventory.Count);
        }
    }

    /// <summary>
    /// เสกไอเทมตามชื่อ prototype — คืน false ถ้าไม่มีของชิ้นนั้นในเกม
    ///
    /// [แก้เอง] 3 ก.ย. 2026 — เดิมผู้เรียก clamp จำนวนไว้ที่ 50 ตายตัว ซึ่งเป็นเลขเดียวกับ
    /// ขนาดกระเป๋าทั้งใบ ⇒ ถ้ามีของอยู่แล้วจะล้นเงียบ ๆ · และมาโครของเกมที่สั่ง `it ... 60`
    /// ก็ได้ไม่ครบโดยไม่บอกสาเหตุ  ตอนนี้ตัดตามช่องที่เหลือจริงแล้ว *บอกกลับ* ว่าให้ได้เท่าไร
    /// </summary>
    private bool GiveByPrototype(string prototype, int count, out int given)
    {
        given = 0;
        if (string.IsNullOrEmpty(prototype) || !ItemNameData.Map.ContainsKey(prototype))
        {
            return false;
        }
        int room = FreeInventorySlots();
        given = Math.Clamp(count, 0, room);
        for (int i = 0; i < given; i++)
        {
            GiveEquipTestItem(prototype, ItemNameData.NameOf(prototype, prototype), ItemNameData.IconOf(prototype, string.Empty), 0);
        }
        return true;
    }

    /// <summary>พูดในช่องรวมแทนผู้เล่นคนนั้น</summary>
    public void ControlSay(string text)
    {
        _world.Broadcast(new SayInExclusiveChannel
        {
            Message = StampSpeaker(new Message_ { EntityId = EntityId, Body = text, Time = Times.UnixTimeNow() })
        });
    }

    /// <summary>สรุปสถานะสั้น ๆ ไว้ตอบกลับคนสั่ง</summary>
    public string ControlStatus()
    {
        WorldPosition p = CurrentPosition;
        int items;
        lock (_inventory)
        {
            items = _inventory.Count;
        }
        return $"{Name} tile {p.x / 200f:F0},{p.y / 200f:F0} สูง {CurrentHeight:F0} ชั้น {CurrentFloor} เลือด {CurrentLife:F0} ของ {items} ชิ้น{(Dead ? " ☠" : "")}";
    }
}
