using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

public partial class ServerPlayer
{
    private const float CombatArmorWearRatio = 0.25f;

    private void HandleRepairItem(RepairItem msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.ToolDurability || !ServerConfig.Current.Tools.Enabled || Dead)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.ItemId) || msg.KitItemIds == null || msg.KitItemIds.Length == 0
            || msg.KitItemIds.Length > 20)
        {
            Send(default(Abort), header.Seq);
            return;
        }

        Item target;
        int repairPerformance = 0;
        lock (_inventory)
        {
            int targetIndex = _inventory.FindIndex(x => x.Id == msg.ItemId);
            if (targetIndex < 0)
            {
                Send(default(Abort), header.Seq);
                return;
            }
            target = _inventory[targetIndex];
            float max = ToolDurability.MaxFor(target.Prototype);
            if (max <= 0f || ToolDurability.RemainingOf(target) >= max - 0.001f)
            {
                Send(default(Abort), header.Seq);
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < msg.KitItemIds.Length; i++)
            {
                string kitId = msg.KitItemIds[i];
                if (string.IsNullOrEmpty(kitId) || kitId == msg.ItemId || !seen.Add(kitId))
                {
                    Send(default(Abort), header.Seq);
                    return;
                }
                int index = _inventory.FindIndex(x => x.Id == kitId);
                if (index < 0 || IsItemLocked(kitId) || !ToolDurability.IsRepairKitFor(target.Prototype, _inventory[index].Prototype))
                {
                    Send(default(Abort), header.Seq);
                    return;
                }
                repairPerformance += ToolDurability.RepairKitPerformance(_inventory[index].Prototype);
            }
            if (repairPerformance < ToolDurability.RepairPerformanceNeeded(target.Prototype))
            {
                Send(default(Abort), header.Seq);
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
                float max = ToolDurability.MaxFor(repaired.Prototype);
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
            Console.WriteLine("[repair] {0} repaired {1} with {2} kit(s)", Name, repaired.Prototype, removedIds.Count);
        }));
    }

    private void WearCombatEquipment(bool wearWeapon, bool wearArmor)
    {
        if (!ServerConfig.Current.Features.ToolDurability || !ServerConfig.Current.Tools.Enabled) return;
        float baseWear = ServerConfig.Current.Tools.WearPerUse;
        if (baseWear <= 0f) return;

        var ids = new List<(string id, float amount)>();
        if (wearWeapon && TryGetWeaponItem(out Item weapon, out _)) ids.Add((weapon.Id, baseWear));
        if (wearArmor)
        {
            foreach (KeyValuePair<string, string> pair in _equippedItems)
            {
                if (TryGetEquipped(pair.Key, out Item armor) && EquipData.TryGetArmor(armor.Prototype, out _))
                    ids.Add((armor.Id, baseWear * CombatArmorWearRatio));
            }
        }
        bool changed = false;
        for (int i = 0; i < ids.Count; i++) changed |= WearDurableItem(ids[i].id, ids[i].amount);
        if (!changed) return;
        MarkDirty();
        SendInventory();
        SendEquipments();
        _world.Broadcast(CurrentDisplay);
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
            if (!string.IsNullOrEmpty(item.Id)) changed |= WearDurableItem(itemId, ToolDurability.MaxOf(item) * 0.10f);
        }
        if (!changed) return;
        MarkDirty();
        SendInventory();
        SendEquipments();
        _world.Broadcast(CurrentDisplay);
        RefreshAbilities();
    }
}
