using System;
using System.Collections.Generic;
using System.IO;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

// GP-07: เซฟ/โหลด state ของผู้เล่นลงดิสก์
// เดิม _inventory / _knownSkills / ตำแหน่ง อยู่ใน RAM ล้วน ออกเกม = หายหมด
// ดูรายละเอียดที่ docs/server/ServerPlayer.Persistence.md
public partial class ServerPlayer
{
    /// <summary>ได้ของแถมเริ่มต้นไปแล้วหรือยัง — กันแจกกองไฟซ้ำทุกครั้งที่ login</summary>
    private bool _starterGiven;

    /// <summary>มีอะไรเปลี่ยนตั้งแต่เซฟครั้งล่าสุดไหม</summary>
    private bool _dirty;

    public bool IsDirty => _dirty;

    /// <summary>เรียกทุกครั้งที่ state ที่ต้องเซฟเปลี่ยน (ของ/สกิล/ตำแหน่ง)</summary>
    public void MarkDirty()
    {
        _dirty = true;
    }

    /// <summary>
    /// โหลด state จากดิสก์ทับของที่ได้จาก /sessions
    /// ถ้ายังไม่เคยมีไฟล์เซฟ = ผู้เล่นใหม่ → แจกของเริ่มต้นให้
    /// </summary>
    private void LoadPersistedState()
    {
        string savePath = SaveStore.PlayerPath(EntityId);
        bool hadSave = System.IO.File.Exists(savePath) || System.IO.File.Exists(savePath + ".tmp");
        PlayerSave save = SaveStore.Load<PlayerSave>(savePath);

        if (save == null)
        {
            if (hadSave)
            {
                throw new InvalidDataException($"ไฟล์เซฟผู้เล่น {EntityId} อ่านไม่ได้และถูกกักกัน — ไม่สร้างตัวละครใหม่ทับ");
            }
            GrantStarterItems();
            ApplySurvivalSave(null);          // เฟส C — ค่าเริ่มต้นเต็มหลอด
            _dirty = true;
            Console.WriteLine($"[save] ผู้เล่นใหม่ {Name} ({EntityId}) — แจกของเริ่มต้น");
            return;
        }

        // Character rename is authoritative on the server. The island snapshot sent
        // by an older client may still contain the previous name after reconnecting.
        if (!string.IsNullOrWhiteSpace(save.Name))
        {
            Name = save.Name.Trim();
        }

        lock (_inventory)
        {
            _inventory.Clear();
            if (save.Inventory != null)
            {
                for (int i = 0; i < save.Inventory.Count; i++)
                {
                    ItemSave it = save.Inventory[i];
                    if (it != null && !string.IsNullOrEmpty(it.Id))
                    {
                        Item loaded = it.ToItem();
                        // Migrate the old starter/cheat axe icon. The prototype's
                        // actual client icon is weapon_axe_onehand_stone_2;
                        // keeping the prototype id but sending the old icon key
                        // makes the inventory render a missing icon.
                        if (string.Equals(loaded.Prototype, "axe_onehand_stone_01", StringComparison.Ordinal)
                            && !string.Equals(loaded.Icon, "weapon_axe_onehand_stone_2", StringComparison.Ordinal))
                        {
                            loaded.Icon = "weapon_axe_onehand_stone_2";
                            _dirty = true;
                        }
                        _inventory.Add(loaded);
                    }
                }
            }
        }
        ApplyInventoryStateSave(save);
        ApplyPoiSave(save);

        if (save.KnownSkills != null && save.KnownSkills.Count > 0)
        {
            _knownSkills.Clear();
            for (int i = 0; i < save.KnownSkills.Count; i++)
            {
                SkillBundleSave s = save.KnownSkills[i];
                if (s != null && !string.IsNullOrEmpty(s.SkillId))
                {
                    _knownSkills.Add(s.ToBundle());
                }
            }
        }

        if (save.SkillPoints > 0)
        {
            _skillPoints = save.SkillPoints;
        }

        // รอยแยก/วาร์ปเรกเซเลอเรเตอร์ — Warp Matter สะสม (ดู ServerPlayer.WarpAccelerator.cs)
        _warpMatterBalance = save.WarpMatterBalance;
        _weeklyWarpMatterAcquired = save.WeeklyWarpMatterAcquired;
        _weeklyWarpMatterRefreshAt = save.WeeklyWarpMatterRefreshAt;

        // GP-14: เลเวลที่ server เซฟไว้ชนะค่าที่ client อ้างมาทาง /sessions
        // (ค่าจาก client ใช้ได้ครั้งเดียวตอน login แรก ซึ่งเป็นตอนที่ยังไม่มีไฟล์เซฟ)
        if (save.Level > 0 && save.Level != Level)
        {
            if (GameServer.TrustClientProfile)
            {
                Console.WriteLine($"[player] {Name}: --trust-client-profile — ใช้เลเวล {Level} จาก client (ที่เซฟไว้คือ {save.Level})");
            }
            else
            {
                Console.WriteLine($"[player] {Name}: client อ้างเลเวล {Level} แต่ server เซฟไว้ {save.Level} — ใช้ของ server");
                Level = save.Level;
            }
        }
        else if (save.Level > 0)
        {
            Level = save.Level;
        }

        // Beta 1.0: exp เป็นตัวจริง เลเวลเป็นผลลัพธ์ — โหลดทีหลังเสมอเพื่อให้ทับค่าข้างบนได้
        // (ผู้เล่นเก่าที่ยังไม่มี exp ในเซฟ จะได้ exp ขั้นต่ำของเลเวลที่มีอยู่ ไม่ถูกลดเลเวล)
        RestoreExp(save.TotalExp);
        SyncExpToLevel();

        // หน้าตา/เพศเป็นเรื่องความสวยงาม ให้ client เปลี่ยนได้ตามเกาะตัวเอง
        // แต่ถ้ารอบนี้ client ไม่ได้ส่งมา ให้ใช้ของที่เคยเซฟไว้ (ไม่งั้นกลายเป็นตัวละครเปล่า)
        if (!_hasLoadedDisplay && !string.IsNullOrEmpty(save.DisplayJson))
        {
            try
            {
                PlayerDisplay display = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerDisplay>(save.DisplayJson);
                display.EntityId = EntityId;
                _loadedDisplay = display;
                _hasLoadedDisplay = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[save] display ในไฟล์เซฟอ่านไม่ออก: " + e.Message);
            }
        }
        if (!_entityTypeFromClient && IsPlayerEntityType(save.EntityType))
        {
            EntityType = save.EntityType;
        }

        // Beta 1.1: ตำแหน่งที่จำไว้ใช้ได้เฉพาะ "เกาะเดิม" เท่านั้น
        // เดินทางมาจากเกาะอื่น = เกิดที่จุดเข้าเกมของเกาะนี้ (พิกัดของอีกเกาะอาจเป็นกลางทะเล/นอกแมพ)
        string here = IslandRegistry.Current?.Id;
        bool sameIsland = here == null || string.Equals(save.LastIsland, here, StringComparison.OrdinalIgnoreCase);
        if (save.HasPosition && sameIsland)
        {
            _lastPosition = new WorldPosition(save.PosX, save.PosY);
            _lastYaw = save.Yaw;
            _hasPosition = true;
        }
        else if (save.HasPosition)
        {
            Console.WriteLine("[island] {0} มาจากเกาะ '{1}' → เกิดที่จุดเข้าเกมของ '{2}'",
                Name, string.IsNullOrEmpty(save.LastIsland) ? "(ไม่ระบุ)" : save.LastIsland, here);
        }

        // ที่เก็บ key/value ของ client — ส่งคืนตอนล็อกอินผ่าน Welcome.Storage (ดู SaveModels.ClientStorage)
        _clientStorage.Clear();
        if (save.ClientStorage != null)
        {
            foreach (KeyValuePair<string, string> pair in save.ClientStorage)
            {
                try
                {
                    _clientStorage[pair.Key] = Convert.FromBase64String(pair.Value ?? "");
                }
                catch (FormatException)
                {
                    // ค่าเสีย = ข้ามไป ดีกว่าทำให้โหลดตัวละครทั้งตัวพัง
                    Console.WriteLine($"[storage] ค่าเสียของ {Name}: key={pair.Key} — ข้าม");
                }
            }
        }

        // เฟส C — อุปกรณ์ที่ใส่อยู่ (RebuildEquipments จะกรองของที่หายไปแล้วออกให้เอง)
        _equipmentPresets.Clear();
        _currentEquipSlotType = IsPlayablePreset((Shared.Item.EquipSlotType)save.CurrentEquipSlotType)
            ? (Shared.Item.EquipSlotType)save.CurrentEquipSlotType
            : Shared.Item.EquipSlotType.Slot1;
        bool loadedPresets = false;
        if (save.EquipmentPresets != null)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> savedPreset in save.EquipmentPresets)
            {
                if (!int.TryParse(savedPreset.Key, out int raw)) continue;
                var type = (Shared.Item.EquipSlotType)raw;
                if (!IsPlayablePreset(type) || savedPreset.Value == null) continue;
                Dictionary<string, string> target = GetEquipmentPreset(type);
                foreach (KeyValuePair<string, string> pair in savedPreset.Value)
                {
                    if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value)) target[pair.Key] = pair.Value;
                }
                loadedPresets = true;
            }
        }
        if (!loadedPresets && save.EquippedItems != null)
        {
            foreach (KeyValuePair<string, string> pair in save.EquippedItems)
            {
                if (!string.IsNullOrEmpty(pair.Key) && !string.IsNullOrEmpty(pair.Value))
                {
                    GetEquipmentPreset(Shared.Item.EquipSlotType.Slot1)[pair.Key] = pair.Value;
                }
            }
        }
        _accessoryId = string.IsNullOrWhiteSpace(save.AccessoryId) ? null : save.AccessoryId;

        Job = (Shared.Player.Job)save.Job;
        ApplySurvivalSave(save.Survival);     // เฟส C
        ApplyProficiencySave(save.CategoryExp);   // ความชำนาญของหมวดสกิล
        ApplySkillResearchSave(save);
        ApplyDeathSave(save);
        ApplyGroup2Save(save);
        ApplyQuestSave(save);
        ApplyPartySave(save);
        ApplySocialSave(save);
        ApplyMailSave(save);
        ApplyWalletSave(save);
        ApplyClanSave(save);

        _starterGiven = save.StarterGiven;
        if (!_starterGiven)
        {
            // เซฟเก่าจากตอนที่ยังไม่มี flag นี้ — ถือว่าเคยได้ไปแล้ว ไม่แจกซ้ำ
            _starterGiven = true;
        }

        _dirty = false;
        int itemCount;
        lock (_inventory)
        {
            itemCount = _inventory.Count;
        }
        Console.WriteLine($"[save] โหลด {Name} ({EntityId}): ของ {itemCount} ชิ้น, สกิล {_knownSkills.Count} ตัว, แต้ม {_skillPoints}, ตำแหน่ง {(_hasPosition ? "จำได้" : "จุดเข้าเกม")}");
    }

    private void GrantStarterItems()
    {
        if (_starterGiven)
        {
            return;
        }
        lock (_inventory)
        {
            // ไอเทมเริ่มต้น: กองไฟ (capsule) สำหรับวางสิ่งก่อสร้างโดยไม่ต้องคราฟ
            _inventory.Add(MakeCapsuleItem("capsulated_bonfire", "กองไฟ", "furniture_workbench_bonfire"));
            // Beta 1.0: แจกขวานหินด้วย — มือเปล่าตีได้ ~6 หน่วย ล่าสัตว์แทบไม่ไหว
            // (ดูตารางสมดุลใน docs/testing/BETA-1.0-PLAN.md)
            _inventory.Add(MakeGatheredItem(new Generator
            {
                Id = "axe_onehand_stone_01",
                Name = "ขวานหิน",
                Icon = "weapon_axe_onehand_stone_2"
            }));
        }
        _starterGiven = true;
    }

    /// <summary>เขียน state ลงดิสก์ (เรียกตอนออกเกมและตอน autosave)</summary>
    public bool Save()
    {
        PlayerSave save = new PlayerSave
        {
            EntityId = EntityId,
            Name = Name,
            Level = Level,
            EntityType = EntityType,
            Job = (int)Job,
            SkillPoints = _skillPoints,
            TotalExp = TotalExp,
            WarpMatterBalance = _warpMatterBalance,
            WeeklyWarpMatterAcquired = _weeklyWarpMatterAcquired,
            WeeklyWarpMatterRefreshAt = _weeklyWarpMatterRefreshAt,
            LastIsland = IslandRegistry.Current?.Id,
            StarterGiven = _starterGiven,
            HasPosition = _hasPosition,
            PosX = _lastPosition.x,
            PosY = _lastPosition.y,
            Yaw = _lastYaw,
            // GP-14: เก็บหน้าตาไว้ด้วย เพื่อให้ตัวละครหน้าเหมือนเดิมแม้ login มาแบบไม่มีข้อมูลจากเกาะ
            DisplayJson = _hasLoadedDisplay
                ? Newtonsoft.Json.JsonConvert.SerializeObject(_loadedDisplay)
                : null
        };

        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                save.Inventory.Add(ItemSave.From(_inventory[i]));
            }
        }
        FillInventoryStateSave(save);
        FillPoiSave(save);
        save.CurrentEquipSlotType = (int)_currentEquipSlotType;
        save.AccessoryId = _accessoryId;
        save.ClientStorage.Clear();
        foreach (KeyValuePair<string, byte[]> pair in _clientStorage)
        {
            save.ClientStorage[pair.Key] = Convert.ToBase64String(pair.Value ?? Array.Empty<byte>());
        }
        foreach (KeyValuePair<Shared.Item.EquipSlotType, Dictionary<string, string>> preset in _equipmentPresets)
        {
            save.EquipmentPresets[((int)preset.Key).ToString()] = new Dictionary<string, string>(preset.Value);
        }
        foreach (KeyValuePair<string, string> pair in _equippedItems)
        {
            save.EquippedItems[pair.Key] = pair.Value;      // เฟส C
        }
        save.Survival = BuildSurvivalSave();                // เฟส C
        save.CategoryExp = BuildProficiencySave();          // ความชำนาญของหมวดสกิล
        FillSkillResearchSave(save);
        FillDeathSave(save);
        FillGroup2Save(save);
        FillQuestSave(save);
        FillPartySave(save);
        FillSocialSave(save);
        FillMailSave(save);
        FillWalletSave(save);
        FillClanSave(save);
        for (int i = 0; i < _knownSkills.Count; i++)
        {
            save.KnownSkills.Add(SkillBundleSave.From(_knownSkills[i]));
        }

        bool ok = SaveStore.Save(SaveStore.PlayerPath(EntityId), save);
        if (ok)
        {
            _dirty = false;
        }
        return ok;
    }
}
