using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

public partial class ServerPlayer
{
    /// <summary>เกราะสึกช้ากว่าอาวุธ — ใช้เฉพาะตอน Tools.Deltas ปิด (ดู ToolDurability.WearFor)</summary>
    private const float CombatArmorWearRatio = 0.25f;
    private static readonly System.Random _repairRng = new System.Random();

    private void HandleRepairItem(RepairItem msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.ToolDurability || !ServerConfig.Current.Tools.Enabled || Dead)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.ItemId) || msg.KitItemIds == null || msg.KitItemIds.Length == 0
            || msg.KitItemIds.Length > 20)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        Item target;
        int repairPerformance = 0;
        lock (_inventory)
        {
            int targetIndex = _inventory.FindIndex(x => x.Id == msg.ItemId);
            if (targetIndex < 0)
            {
                Send(Aborts.Reason(), header.Seq);
                return;
            }
            target = _inventory[targetIndex];
            // [TodoList/03] เทียบกับ max จริงของชิ้น (ลดลงหลังซ่อม) ไม่ใช่ MaxFor ของ prototype ไม่งั้นซ่อมซ้ำได้ทั้งที่เต็ม
            float max = ToolDurability.MaxOf(target) > 0f ? ToolDurability.MaxOf(target) : ToolDurability.MaxFor(target.Prototype);
            if (max <= 0f || ToolDurability.RemainingOf(target) >= max - 0.001f)
            {
                Send(Aborts.Reason(), header.Seq);
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < msg.KitItemIds.Length; i++)
            {
                string kitId = msg.KitItemIds[i];
                if (string.IsNullOrEmpty(kitId) || kitId == msg.ItemId || !seen.Add(kitId))
                {
                    Send(Aborts.Reason(), header.Seq);
                    return;
                }
                int index = _inventory.FindIndex(x => x.Id == kitId);
                if (index < 0 || IsItemLocked(kitId) || !ToolDurability.IsRepairKitFor(target.Prototype, _inventory[index].Prototype))
                {
                    Send(Aborts.Reason(), header.Seq);
                    return;
                }
                repairPerformance += ToolDurability.RepairKitPerformance(_inventory[index].Prototype);
            }
            if (repairPerformance < ToolDurability.RepairPerformanceNeeded(target.Prototype))
            {
                Send(Aborts.Reason(), header.Seq);
                return;
            }
        }

        const float repairSeconds = 1f;
        Send(new Messages.Timer { Duration = repairSeconds }, header.Seq);
        _deferred.Add((Times.UnixTimeNow() + repairSeconds, delegate
        {
            var removedIds = new List<string>();
            Item repaired;
            lock (_inventory)
            {
                int targetIndex = _inventory.FindIndex(x => x.Id == msg.ItemId);
                if (targetIndex < 0) return;

                var liveKitIndices = new List<int>();
                var seen = new HashSet<string>(StringComparer.Ordinal);
                int livePerformance = 0;
                foreach (string kitId in msg.KitItemIds)
                {
                    if (!seen.Add(kitId)) return;
                    int index = _inventory.FindIndex(x => x.Id == kitId);
                    if (index < 0 || IsItemLocked(kitId)
                        || !ToolDurability.IsRepairKitFor(_inventory[targetIndex].Prototype, _inventory[index].Prototype)) return;
                    liveKitIndices.Add(index);
                    livePerformance += ToolDurability.RepairKitPerformance(_inventory[index].Prototype);
                }
                if (livePerformance < ToolDurability.RepairPerformanceNeeded(_inventory[targetIndex].Prototype)) return;

                liveKitIndices.Sort();
                for (int i = liveKitIndices.Count - 1; i >= 0; i--)
                {
                    removedIds.Add(_inventory[liveKitIndices[i]].Id);
                    ForgetInventoryItem(_inventory[liveKitIndices[i]].Id);
                    _inventory.RemoveAt(liveKitIndices[i]);
                    if (liveKitIndices[i] < targetIndex) targetIndex--;
                }
                repaired = _inventory[targetIndex];
                // [TodoList/03] ซ่อมแล้ว max ลด 5-13% ทุกครั้ง (เกม repair_damage_range) — ของที่ซ่อมบ่อยจะพังเร็วขึ้นเรื่อย ๆ
                float before = ToolDurability.MaxOf(repaired) > 0f ? ToolDurability.MaxOf(repaired) : ToolDurability.MaxFor(repaired.Prototype);
                float max = ToolDurability.MaxAfterRepair(before, _repairRng);
                repaired.Durability = ToolDurability.MakeGauge(max, max);
                repaired.RepairRequirement = ToolDurability.RepairRequirementFor(repaired.Prototype);
                _inventory[targetIndex] = repaired;
            }
            MarkDirty();
            Send(new InventoryUpdated
            {
                EntityId = EntityId,
                Items = new[] { repaired },
                RemovedItemIds = removedIds.ToArray(),
                ItemOrder = CurrentInventoryOrder(),
                ProtectedItems = CurrentProtectedItems()
            });
            SendInventory();
            QuestProgress(QuestData.Goal.Repair);
            Console.WriteLine("[repair] {0} repaired {1} with {2} kit(s)", Name, repaired.Prototype, removedIds.Count);
        }));
    }

    private void WearCombatEquipment(bool wearWeapon, bool wearArmor)
    {
        if (!ServerConfig.Current.Features.ToolDurability || !ServerConfig.Current.Tools.Enabled) return;
        // [TodoList/03] อาวุธหัก delta attack (0.0768) เกราะหัก delta defense (0.064) ตามเกม — Deltas ปิด = WearPerUse เดิม
        ToolConfig toolCfg = ServerConfig.Current.Tools;
        bool deltas = toolCfg.Deltas != null && toolCfg.Deltas.Enabled;
        if (!deltas && toolCfg.WearPerUse <= 0f) return;

        var ids = new List<(string id, float amount)>();
        if (wearWeapon && TryGetWeaponItem(out Item weapon, out _))
            ids.Add((weapon.Id, ToolDurability.WearFor(WearKind.Attack, weapon.Prototype)));
        if (wearArmor)
        {
            foreach (KeyValuePair<string, string> pair in _equippedItems)
            {
                if (TryGetEquipped(pair.Key, out Item armor) && EquipData.TryGetArmor(armor.Prototype, out _))
                    ids.Add((armor.Id, ToolDurability.WearFor(WearKind.Defense, armor.Prototype)));
            }
        }
        bool changed = false;
        for (int i = 0; i < ids.Count; i++) changed |= WearDurableItem(ids[i].id, ids[i].amount);
        if (!changed) return;
        MarkDirty();
        SendInventory();
        SendEquipments();
        _world.BroadcastToViewers(EntityId, CurrentDisplay);
        RefreshAbilities();
    }

    private bool WearDurableItem(string itemId, float amount)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0f) return false;
        string brokenName = null;
        lock (_inventory)
        {
            int index = _inventory.FindIndex(x => x.Id == itemId);
            if (index < 0 || !ToolDurability.HasDurability(_inventory[index])) return false;
            Item item = _inventory[index];
            float max = ToolDurability.MaxOf(item);
            float left = ToolDurability.RemainingOf(item) - amount;
            if (left <= 0f)
            {
                brokenName = item.Name ?? item.Prototype;
                _inventory.RemoveAt(index);
                UnequipItemEverywhere(itemId);
                ForgetInventoryItem(itemId);
            }
            else
            {
                item.Durability = ToolDurability.MakeGauge(left, max);
                _inventory[index] = item;
            }
        }
        if (brokenName != null) Send(new Info { Text = $"{brokenName} พังแล้ว" });
        return true;
    }

    private void WearEquippedOnDeath()
    {
        if (!ServerConfig.Current.Features.ToolDurability || !ServerConfig.Current.Tools.Enabled) return;
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (string itemId in _equippedItems.Values) ids.Add(itemId);
        bool changed = false;
        foreach (string itemId in ids)
        {
            Item item;
            lock (_inventory)
            {
                int index = _inventory.FindIndex(x => x.Id == itemId);
                item = index >= 0 ? _inventory[index] : default;
            }
            // [TodoList/07] สัดส่วนอยู่ใน config Death.EquipWearRatio (0.05 เมื่อมีของหล่น · ปิด penalty = 0.10 เดิม)
            DeathConfig deathCfg = ServerConfig.Current.Death;
            float wearRatio = deathCfg != null && deathCfg.Enabled && deathCfg.ItemDrop ? deathCfg.EquipWearRatio : 0.10f;
            if (!string.IsNullOrEmpty(item.Id)) changed |= WearDurableItem(itemId, ToolDurability.MaxOf(item) * wearRatio);
        }
        if (!changed) return;
        MarkDirty();
        SendInventory();
        SendEquipments();
        _world.BroadcastToViewers(EntityId, CurrentDisplay);
        RefreshAbilities();
    }
}
