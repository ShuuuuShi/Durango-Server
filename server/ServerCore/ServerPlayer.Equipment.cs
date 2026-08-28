using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;
using Shared.Item;

namespace DurangoServer.Core;

// เฟส C — ระบบสวมใส่อุปกรณ์
// เดิม SendEquipments() ตอบ Presets = null ซึ่งทำให้ client โยน NullReferenceException
// (EquipSystem.EquipmentsReceived deref msg.Presets ตรง ๆ ไม่เช็ค null)
// ดูรายละเอียดที่ docs/server/Equipment.md
public partial class ServerPlayer
{
    /// <summary>ช่อง → item id ที่ใส่อยู่ (เช่น "main" → guid ของขวาน)</summary>
    private readonly Dictionary<EquipSlotType, Dictionary<string, string>> _equipmentPresets =
        new Dictionary<EquipSlotType, Dictionary<string, string>>();
    private EquipSlotType _currentEquipSlotType = EquipSlotType.Slot1;
    private string _accessoryId;

    private Dictionary<string, string> _equippedItems => GetEquipmentPreset(_currentEquipSlotType);

    private static bool IsPlayablePreset(EquipSlotType type)
    {
        return type == EquipSlotType.Slot1 || type == EquipSlotType.Slot2 || type == EquipSlotType.Slot3;
    }

    private Dictionary<string, string> GetEquipmentPreset(EquipSlotType type)
    {
        if (!IsPlayablePreset(type)) type = EquipSlotType.Slot1;
        if (!_equipmentPresets.TryGetValue(type, out Dictionary<string, string> preset))
        {
            preset = new Dictionary<string, string>(StringComparer.Ordinal);
            _equipmentPresets[type] = preset;
        }
        return preset;
    }

    private void UnequipItemEverywhere(string itemId)
    {
        foreach (Dictionary<string, string> preset in _equipmentPresets.Values)
        {
            var stale = new List<string>();
            foreach (KeyValuePair<string, string> pair in preset)
            {
                if (pair.Value == itemId) stale.Add(pair.Key);
            }
            for (int i = 0; i < stale.Count; i++) preset.Remove(stale[i]);
        }
    }

    /// <summary>
    /// M-7: ชื่อช่องที่ยอมรับ — เดิมรับชื่ออะไรก็ได้จาก client
    /// ยิงชื่อช่องไม่ซ้ำล้านครั้ง = dict ล้าน entry แล้วถูกเขียนลงไฟล์เซฟทั้งหมด (ไฟล์บวมเป็น GB)
    ///
    /// 🐛 รายชื่อเดิมเขียนด้วยมือแล้ว **ผิด** — ไม่มี "both" ซึ่งเป็นช่องของ
    /// **อาวุธสองมือ 121 ชิ้นจาก 248 ชิ้น** ⇒ ใส่ขวาน/ค้อนสองมือไม่ได้เลย server ตอบ Abort
    /// (ขาด gloves/shoes/bag/precious ด้วย · ส่วน hand/leg/foot/back/waist/accessory/costume
    ///  ที่มีในรายการเดิมไม่มีอยู่จริงในข้อมูลเกมสักช่อง)
    ///
    /// ตอนนี้สร้างจากช่องที่มีจริงใน <see cref="EquipData"/> — เพิ่มของใหม่แล้วไม่ต้องมาแก้ที่นี่อีก
    /// </summary>
    private static readonly HashSet<string> ValidSlots = BuildValidSlots();

    private static HashSet<string> BuildValidSlots()
    {
        // "hoody" = เสื้อคลุมที่กินช่องหมวกไปด้วย · client เปลี่ยนชื่อช่องเองตอนส่ง (EquipSystem.EquipItem)
        var set = new HashSet<string>(StringComparer.Ordinal) { "hoody" };
        foreach (EquipData.WeaponInfo w in EquipData.Weapons.Values)
        {
            if (!string.IsNullOrEmpty(w.Slot))
            {
                set.Add(w.Slot);
            }
        }
        foreach (EquipData.ArmorInfo a in EquipData.Armors.Values)
        {
            if (!string.IsNullOrEmpty(a.Slot))
            {
                set.Add(a.Slot);
            }
        }
        return set;
    }

    /// <summary>ช่องที่ของชิ้นนี้ใส่ได้จริงตามข้อมูลเกม — รับ "hoody" แทน "body" ได้ด้วย</summary>
    private static bool FitsSlot(string prototype, string slot)
    {
        string real = EquipData.SlotOf(prototype);
        if (real == null)
        {
            return false;
        }
        if (string.Equals(real, slot, StringComparison.Ordinal))
        {
            return true;
        }
        return slot == "hoody" && real == "body";
    }

    /// <summary>หน้าตาปัจจุบัน = หน้าตาพื้นฐาน + อุปกรณ์ที่ใส่</summary>
    private PlayerDisplay _display;
    private bool _displayReady;

    private bool IsMale => EntityType != 1001;

    /// <summary>หน้าตาพื้นฐานก่อนใส่อุปกรณ์ (จาก /sessions หรือไฟล์เซฟของเกม)</summary>
    private PlayerDisplay BaseDisplay()
    {
        if (_hasLoadedDisplay)
        {
            return _loadedDisplay;
        }
        return new PlayerDisplay
        {
            EntityId = EntityId,
            DefaultBody = null,
            DefaultInner = null,
            DefaultHead = null,
            DefaultHair = null,
            Hair = null,
            Body = null,
            Head = null,
            Equip = null,
            Beard = null,
            BodyColor = null,
            HeadColor = null,
            EquipColor = null,
            SkinColor = null,
            HairColor = null,
            EyeColor = null,
            LipColor = null,
            Portrait = 0,
            PortraitBg = 0,
            PortraitBgColor = null,
            PortraitIcon = null,
            VoiceType = 0,
            BodySize = 1f,
            Invisible = false,
            WeaponInfo = default,
            BoardingOn = 0,
            BoardingTime = default,
            VehicleEntityId = null,
            Accessory = null
        };
    }

    /// <summary>หน้าตาที่จะส่งให้ client (สร้างครั้งแรกเมื่อถูกเรียก)</summary>
    public PlayerDisplay CurrentDisplay
    {
        get
        {
            if (!_displayReady)
            {
                RebuildEquipments();
            }
            return _display;
        }
    }

    private void HandleEquip(Equip msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Equipment)
        {
            RejectFeatureDisabled("Equipment", "Equip", "ระบบอุปกรณ์ยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (!IsPlayablePreset(msg.SlotType))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        Dictionary<string, string> preset = GetEquipmentPreset(msg.SlotType);
        string slot = msg.SlotName ?? string.Empty;
        if (!ValidSlots.Contains(slot))
        {
            Console.WriteLine("[equip] ปฏิเสธ {0}: ไม่มีช่องชื่อ '{1}'", Name, slot);
            Send(default(Abort), header.Seq);
            return;
        }
        bool equip = msg.Action == "equip";
        if (equip)
        {
            // ต้องมีของจริงในกระเป๋าถึงจะใส่ได้ — ไม่งั้น client ส่ง id มั่วมาก็ใส่ได้
            string prototype = null;
            lock (_inventory)
            {
                int idx = _inventory.FindIndex(x => x.Id == msg.ItemId);
                if (idx >= 0)
                {
                    prototype = _inventory[idx].Prototype;
                }
            }
            if (prototype == null)
            {
                Console.WriteLine("[equip] ปฏิเสธ: {0} ไม่มีไอเทม {1}", Name, msg.ItemId);
                Send(default(Abort), header.Seq);
                return;
            }
            // 🐛 เดิมเช็คแค่ "มีของอยู่ในกระเป๋าไหม" ไม่ได้ดูว่าของชิ้นนั้นใส่ช่องนี้ได้จริงหรือเปล่า
            //    ⇒ ยิง packet เอา **เนื้อดิบใส่ช่องหมวก** ได้ แล้วมันถูกเขียนลงไฟล์เซฟด้วย
            //    ตอนนี้ค่าป้องกัน/ดาเมจคิดจากของที่ใส่จริง จึงยิ่งต้องกั้น
            if (!FitsSlot(prototype, slot))
            {
                Console.WriteLine("[equip] ปฏิเสธ {0}: {1} ใส่ช่อง '{2}' ไม่ได้ (ของชิ้นนี้เป็นของช่อง '{3}')",
                    Name, prototype, slot, EquipData.SlotOf(prototype) ?? "ใส่ไม่ได้");
                Send(default(Abort), header.Seq);
                return;
            }
            preset[slot] = msg.ItemId;
        }
        else if (!preset.Remove(slot))
        {
            // ถอดช่องที่ไม่ได้ใส่อะไรอยู่ — ตอบ Abort ไม่งั้น client รอค้าง
            Send(default(Abort), header.Seq);
            return;
        }

        Console.WriteLine("[equip] {0} {1} slot={2} item={3}", Name, equip ? "ใส่" : "ถอด", msg.SlotName, msg.ItemId);
        MarkDirty();

        Equipments result = RebuildEquipments();
        Send(result, header.Seq);          // client รอ reply ของ seq นี้ (.All)
        if (msg.SlotType == _currentEquipSlotType) _world.BroadcastToViewers(EntityId, _display);
        // อุปกรณ์มีผลกับดาเมจ/ค่าป้องกัน/หลอดแล้ว — หน้าตัวละครกับหลอดต้องอัปเดตทันที
        RefreshAbilities();
        if (equip)
        {
            // นับเฉพาะตอน "ใส่" ไม่นับตอนถอด (ไม่งั้นใส่-ถอดสลับกันก็ผ่านเควสได้)
            QuestProgress(QuestData.Goal.Equip);
            QuestProgress(QuestData.Goal.Equip, slot);
        }
    }

    private void HandleGetEquipments(GetEquipments msg, PacketHeader header)
    {
        Send(RebuildEquipments(), header.Seq);
    }

    private void HandleChangeEquipSlotType(ChangeEquipSlotType msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Equipment)
        {
            RejectFeatureDisabled("Equipment", "ChangeEquipSlotType", "ระบบอุปกรณ์ยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (!IsPlayablePreset(msg.SlotType))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        _currentEquipSlotType = msg.SlotType;
        MarkDirty();
        RebuildEquipments();
        Send(default(OK), header.Seq);
        Send(RebuildEquipments());
        _world.BroadcastToViewers(EntityId, _display);
        RefreshAbilities();
    }

    private void HandleAttachAccessory(AttachAccessory msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Equipment)
        {
            RejectFeatureDisabled("Equipment", "AttachAccessory", "ระบบอุปกรณ์ยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (string.IsNullOrWhiteSpace(msg.AccessoryId) || msg.AccessoryId.Length > 128)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        _accessoryId = msg.AccessoryId.Trim();
        MarkDirty();
        RebuildEquipments();
        Send(default(OK), header.Seq);
        _world.BroadcastToViewers(EntityId, _display);
    }

    private void HandleResetAccessory(ResetAccessory msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Equipment)
        {
            RejectFeatureDisabled("Equipment", "ResetAccessory", "ระบบอุปกรณ์ยังไม่เปิดในรอบนี้", header);
            return;
        }
        _accessoryId = null;
        MarkDirty();
        RebuildEquipments();
        Send(default(OK), header.Seq);
        _world.BroadcastToViewers(EntityId, _display);
    }

    private EquipmentSlot BuildPresetSlot(EquipSlotType type)
    {
        Dictionary<string, string> source = GetEquipmentPreset(type);
        var items = new Dictionary<string, Item>();
        var stale = new List<string>();
        foreach (KeyValuePair<string, string> pair in source)
        {
            lock (_inventory)
            {
                int idx = _inventory.FindIndex(x => x.Id == pair.Value);
                if (idx >= 0) items[pair.Key] = _inventory[idx];
                else stale.Add(pair.Key);
            }
        }
        for (int i = 0; i < stale.Count; i++) source.Remove(stale[i]);
        if (stale.Count > 0) MarkDirty();
        return new EquipmentSlot
        {
            ItemSlots = items,
            IsLocked = false,
            UnlockSince = null,
            UnlockUntil = null,
            TitleId = _selectedTitleId ?? string.Empty
        };
    }

    /// <summary>
    /// สร้าง <see cref="Equipments"/> ใหม่จาก _equippedItems พร้อมอัปเดต _display
    /// ตรรกะเดียวกับ offline server เดิมของเกม (Durango.Offline.Player.UpdateEquipments)
    /// </summary>
    private Equipments RebuildEquipments()
    {
        PlayerDisplay display = BaseDisplay();
        display.EntityId = EntityId;

        // รีเซ็ตส่วนที่อุปกรณ์คุมก่อน แล้วค่อยทาทับตามของที่ใส่
        display.Body = display.DefaultBody;
        display.Head = null;
        if (display.BodyColor == null || display.BodyColor.Length < 3)
        {
            display.BodyColor = new[] { "FFFFFF", "FFFFFF", "FFFFFF" };
        }
        display.WeaponInfo = default;
        display.Equip = null;
        display.EquipColor = null;
        display.Accessory = _accessoryId;

        var itemSlots = new Dictionary<string, Item>();

        foreach (KeyValuePair<string, string> pair in _equippedItems)
        {
            Item item;
            bool found;
            lock (_inventory)
            {
                int idx = _inventory.FindIndex(x => x.Id == pair.Value);
                found = idx >= 0;
                item = found ? _inventory[idx] : default;
            }
            if (!found)
            {
                // ของหายไปจากกระเป๋าแล้ว (โดนใช้/วางไป) — ข้ามไป เดี๋ยวโดนเก็บกวาดข้างล่าง
                continue;
            }

            itemSlots[pair.Key] = item;
            string[] color = { item.ColorR, item.ColorG, item.ColorB };

            if (EquipData.TryGetWeapon(item.Prototype, out EquipData.WeaponInfo weapon))
            {
                display.WeaponInfo = new WeaponDisplayInfo
                {
                    WeaponFramework = weapon.Framework
                };
                display.Equip = weapon.Model;
                display.EquipColor = color;
            }

            if (EquipData.TryGetArmor(item.Prototype, out EquipData.ArmorInfo armor))
            {
                string model = IsMale ? armor.MaleModel : armor.FemaleModel;
                if (!string.IsNullOrEmpty(model))
                {
                    if (armor.Slot == "body")
                    {
                        display.Body = model;
                        display.BodyColor = color;
                    }
                    else if (armor.Slot == "head")
                    {
                        display.Head = model;
                        display.HeadColor = color;
                    }
                }
            }
        }

        // เก็บกวาดช่องที่ของหายไปแล้ว ไม่ให้ค้างในเซฟ
        if (itemSlots.Count != _equippedItems.Count)
        {
            var stale = new List<string>();
            foreach (string slot in _equippedItems.Keys)
            {
                if (!itemSlots.ContainsKey(slot))
                {
                    stale.Add(slot);
                }
            }
            for (int i = 0; i < stale.Count; i++)
            {
                _equippedItems.Remove(stale[i]);
            }
            if (stale.Count > 0)
            {
                MarkDirty();
            }
        }

        _display = display;
        _displayReady = true;

        return new Equipments
        {
            CurrentType = _currentEquipSlotType,
            // ⚠️ ห้ามเป็น null — client deref ตรง ๆ ใน EquipSystem.EquipmentsReceived
            Presets = new Dictionary<EquipSlotType, EquipmentSlot>
            {
                [EquipSlotType.Slot1] = _currentEquipSlotType == EquipSlotType.Slot1
                    ? new EquipmentSlot { ItemSlots = itemSlots, IsLocked = false, UnlockSince = null, UnlockUntil = null, TitleId = _selectedTitleId ?? string.Empty }
                    : BuildPresetSlot(EquipSlotType.Slot1),
                [EquipSlotType.Slot2] = _currentEquipSlotType == EquipSlotType.Slot2
                    ? new EquipmentSlot { ItemSlots = itemSlots, IsLocked = false, UnlockSince = null, UnlockUntil = null, TitleId = _selectedTitleId ?? string.Empty }
                    : BuildPresetSlot(EquipSlotType.Slot2),
                [EquipSlotType.Slot3] = _currentEquipSlotType == EquipSlotType.Slot3
                    ? new EquipmentSlot { ItemSlots = itemSlots, IsLocked = false, UnlockSince = null, UnlockUntil = null, TitleId = _selectedTitleId ?? string.Empty }
                    : BuildPresetSlot(EquipSlotType.Slot3)
            }
        };
    }
}
