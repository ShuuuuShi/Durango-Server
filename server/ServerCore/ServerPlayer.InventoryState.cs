using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

public partial class ServerPlayer
{
    private readonly List<string> _inventoryOrder = new List<string>();
    private readonly HashSet<string> _lockedItemIds = new HashSet<string>(StringComparer.Ordinal);

    private string[] CurrentInventoryOrder()
    {
        lock (_inventory)
        {
            var live = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < _inventory.Count; i++) live.Add(_inventory[i].Id);
            _inventoryOrder.RemoveAll(id => !live.Contains(id));
            for (int i = 0; i < _inventory.Count; i++)
            {
                if (!_inventoryOrder.Contains(_inventory[i].Id)) _inventoryOrder.Add(_inventory[i].Id);
            }
            _lockedItemIds.RemoveWhere(id => !live.Contains(id));
            return _inventoryOrder.ToArray();
        }
    }

    private ProtectedItems CurrentProtectedItems()
    {
        CurrentInventoryOrder();
        var ids = new string[_lockedItemIds.Count];
        _lockedItemIds.CopyTo(ids);
        Array.Sort(ids, StringComparer.Ordinal);
        return new ProtectedItems { ItemIds = ids };
    }

    private bool IsItemLocked(string itemId)
    {
        return !string.IsNullOrEmpty(itemId) && _lockedItemIds.Contains(itemId);
    }

    private void ForgetInventoryItem(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        _inventoryOrder.Remove(itemId);
        _lockedItemIds.Remove(itemId);
    }

    private void HandleInventoryOrder(InventoryOrder msg, PacketHeader header)
    {
        if (msg.TargetArtifact.HasValue || msg.ItemOrder == null || msg.ItemOrder.Length > PlayerInventoryMaxSize)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        lock (_inventory)
        {
            if (msg.ItemOrder.Length != _inventory.Count)
            {
                Send(default(Abort), header.Seq);
                return;
            }
            var live = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < _inventory.Count; i++) live.Add(_inventory[i].Id);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < msg.ItemOrder.Length; i++)
            {
                if (!live.Contains(msg.ItemOrder[i]) || !seen.Add(msg.ItemOrder[i]))
                {
                    Send(default(Abort), header.Seq);
                    return;
                }
            }
            _inventoryOrder.Clear();
            _inventoryOrder.AddRange(msg.ItemOrder);
        }
        MarkDirty();
        Send(new InventoryUpdated { EntityId = EntityId, ItemOrder = CurrentInventoryOrder() }, header.Seq);
    }

    private void HandleLockOrUnlockItems(LockOrUnlockItems msg, PacketHeader header)
    {
        if (msg.ItemIds == null || msg.ItemIds.Length == 0 || msg.ItemIds.Length > PlayerInventoryMaxSize)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        lock (_inventory)
        {
            var live = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < _inventory.Count; i++) live.Add(_inventory[i].Id);
            for (int i = 0; i < msg.ItemIds.Length; i++)
            {
                if (!live.Contains(msg.ItemIds[i]))
                {
                    Send(default(Abort), header.Seq);
                    return;
                }
            }
            for (int i = 0; i < msg.ItemIds.Length; i++)
            {
                if (msg.Lock) _lockedItemIds.Add(msg.ItemIds[i]);
                else _lockedItemIds.Remove(msg.ItemIds[i]);
            }
        }
        MarkDirty();
        Send(new InventoryUpdated { EntityId = EntityId, ProtectedItems = CurrentProtectedItems() }, header.Seq);
    }

    private void ApplyInventoryStateSave(PlayerSave save)
    {
        _inventoryOrder.Clear();
        _lockedItemIds.Clear();
        if (save?.InventoryOrder != null) _inventoryOrder.AddRange(save.InventoryOrder);
        if (save?.LockedItemIds != null)
        {
            for (int i = 0; i < save.LockedItemIds.Count; i++) _lockedItemIds.Add(save.LockedItemIds[i]);
        }
        CurrentInventoryOrder();
    }

    private void FillInventoryStateSave(PlayerSave save)
    {
        save.InventoryOrder = new List<string>(CurrentInventoryOrder());
        save.LockedItemIds = new List<string>(_lockedItemIds);
    }
}
