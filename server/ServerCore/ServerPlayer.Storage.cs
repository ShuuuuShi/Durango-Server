using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

// เฟส C — กล่องเก็บของ
// ย้ายของระหว่างกระเป๋าผู้เล่นกับสิ่งปลูกสร้างที่มี component "Inventory" (หีบ/ตู้)
// ดูรายละเอียดที่ docs/server/Storage.md
public partial class ServerPlayer
{
    /// <summary>ความจุกล่อง (กระเป๋าผู้เล่นคือ 50 ดู SendInventory)</summary>
    private const int BoxMaxSize = 200;

    /// <summary>
    /// ⚠️ อย่าสับสนกับกล่องเก็บของข้างบน — คนละเรื่องกันคนละอย่าง
    ///
    /// นี่คือที่เก็บ key/value ของ client เอง (`SetStorageItem` → `Welcome.Storage`)
    /// ใช้จำสถานะ UI/ความคืบหน้า เช่น "encyclopedia", "RecentlyUnlockedMenuList"
    /// ไม่ใช่ไอเทมในเกม ดูคอมเมนต์เต็มที่ PlayerSave.ClientStorage
    /// </summary>
    private readonly Dictionary<string, byte[]> _clientStorage = new Dictionary<string, byte[]>();

    /// <summary>ขนาดค่าสูงสุดต่อ key — กันผู้เล่นยัดข้อมูลจนไฟล์เซฟบวม</summary>
    private const int MaxStorageValueBytes = 64 * 1024;

    /// <summary>จำนวน key สูงสุดต่อผู้เล่น</summary>
    private const int MaxStorageKeys = 64;

    /// <summary>ค่าที่เก็บไว้ทั้งหมด — GameServer หยิบไปใส่ Welcome ตอนล็อกอิน</summary>
    public IReadOnlyDictionary<string, byte[]> ClientStorage => _clientStorage;

    /// <summary>
    /// client ขอดูของในกระเป๋าตัวเอง (Target = null) หรือในกล่อง (Target = สิ่งปลูกสร้าง)
    /// </summary>
    private void HandleGetInventory(GetInventory msg, PacketHeader header)
    {
        if (!msg.Target.HasValue || string.IsNullOrEmpty(msg.Target.Value.EntityId))
        {
            SendInventory();
            return;
        }

        string boxId = msg.Target.Value.EntityId;
        if (!CanUseBox(boxId, header))
        {
            return;
        }
        SendBoxInventory(boxId, header.Seq);
    }

    private void SendBoxInventory(string boxId, uint replyOf = 0u)
    {
        Item[] items = _world.GetBoxItems(boxId);
        Send(new Inventory
        {
            EntityId = boxId,
            InventoryItems = new InventoryItems
            {
                EntityId = boxId,
                Items = items.Length == 0 ? null : items
            },
            InventoryInfos = new InventoryInfos
            {
                EntityId = boxId,
                MaxSize = BoxMaxSize,
                LockedItemIds = null,
                ItemOrder = null,
                ProtectedItems = default
            },
            Wallet = null
        }, replyOf);
    }

    /// <summary>
    /// M-4: กล่องนี้เปิดได้ไหม — เดิมเช็คแค่ "เป็นกล่อง" ⇒ รู้ entity id ของกล่อง
    /// (ซึ่งมากับ AppearArtifact ที่ broadcast ให้ทุกคน) ก็ขนของในบ้านคนอื่นได้จากอีกฟากแมพ
    /// </summary>
    private bool CanUseBox(string boxId, PacketHeader header)
    {
        if (!_world.IsStorage(boxId))
        {
            Console.WriteLine("[storage] {0}: {1} ไม่ใช่กล่อง", Name, boxId);
            Send(Aborts.Reason(), header.Seq);
            return false;
        }
        if (!_world.TryGetArtifact(boxId, out AppearArtifact box))
        {
            Send(Aborts.Reason(), header.Seq);
            return false;
        }
        if (!CanModifyArtifact(box))
        {
            Console.WriteLine("[storage] ปฏิเสธ {0}: กล่อง {1} ไม่ใช่ของตัวเอง", Name, boxId);
            Send(Aborts.Reason(), header.Seq);
            return false;
        }
        if (!IsWithinReach(box.Tile))
        {
            Console.WriteLine("[storage] ปฏิเสธ {0}: กล่อง {1} อยู่ไกลเกินเอื้อม", Name, boxId);
            Send(Aborts.Reason(), header.Seq);
            return false;
        }
        return true;
    }

    /// <summary>เอาของจากกระเป๋าใส่กล่อง</summary>
    private void HandlePutInItem(PutInItem msg, PacketHeader header)
    {
        string boxId = msg.EntityId;
        if (!CanUseBox(boxId, header))
        {
            return;
        }
        if (msg.ItemIds == null || msg.ItemIds.Length == 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        for (int i = 0; i < msg.ItemIds.Length; i++)
        {
            if (IsItemLocked(msg.ItemIds[i]))
            {
                Send(Aborts.Reason(), header.Seq);
                return;
            }
        }

        // ดึงของออกจากกระเป๋าเฉพาะ id ที่มีจริง — client ส่ง id มั่วมาก็ไม่ทำอะไรหาย
        var moving = new List<Item>();
        lock (_inventory)
        {
            for (int i = 0; i < msg.ItemIds.Length; i++)
            {
                int idx = _inventory.FindIndex(x => x.Id == msg.ItemIds[i]);
                if (idx >= 0)
                {
                    moving.Add(_inventory[idx]);
                    ForgetInventoryItem(_inventory[idx].Id);
                    _inventory.RemoveAt(idx);
                }
            }
        }
        if (moving.Count == 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // กล่องเต็ม → คืนของกลับกระเป๋าทั้งหมด (ไม่ให้ของหายกลางทาง)
        if (!_world.TryPutInBox(boxId, moving, BoxMaxSize))
        {
            lock (_inventory)
            {
                _inventory.AddRange(moving);
            }
            Console.WriteLine("[storage] กล่อง {0} เต็ม — คืนของ {1} ชิ้นให้ {2}", boxId, moving.Count, Name);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        Console.WriteLine("[storage] {0} ใส่ของ {1} ชิ้นลงกล่อง {2}", Name, moving.Count, boxId);
        MarkDirty();
        QuestProgress(QuestData.Goal.Store, null, moving.Count);

        var movedIds = new string[moving.Count];
        for (int i = 0; i < moving.Count; i++)
        {
            movedIds[i] = moving[i].Id;
        }

        // แจ้ง 2 ฝั่ง: ของหายจากกระเป๋าเรา / ของโผล่ในกล่อง
        Send(new InventoryUpdated { EntityId = EntityId, RemovedItemIds = movedIds });
        BroadcastBoxUpdate(boxId, moving.ToArray(), null);
        SendInventory();
    }

    /// <summary>เอาของจากกล่องใส่กระเป๋า</summary>
    private void HandleTakeOutItem(TakeOutItem msg, PacketHeader header)
    {
        string boxId = msg.EntityId;
        if (!CanUseBox(boxId, header))
        {
            return;
        }
        if (msg.ItemIds == null || msg.ItemIds.Length == 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        int free;
        lock (_inventory)
        {
            free = PlayerInventoryMaxSize - _inventory.Count;
        }
        if (free <= 0)
        {
            Console.WriteLine("[storage] กระเป๋า {0} เต็ม หยิบของไม่ได้", Name);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // หยิบได้เท่าที่กระเป๋ารับไหว
        List<Item> taken = _world.TakeFromBox(boxId, msg.ItemIds, free);
        if (taken.Count == 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        lock (_inventory)
        {
            _inventory.AddRange(taken);
        }
        Console.WriteLine("[storage] {0} หยิบของ {1} ชิ้นจากกล่อง {2}", Name, taken.Count, boxId);
        MarkDirty();

        var takenIds = new string[taken.Count];
        for (int i = 0; i < taken.Count; i++)
        {
            takenIds[i] = taken[i].Id;
        }

        BroadcastBoxUpdate(boxId, null, takenIds);
        Send(new InventoryUpdated { EntityId = EntityId, Items = taken.ToArray() });
        // ตอบอะไรก็ได้ที่ไม่ใช่ Abort — client เช็คด้วย Packet.IsSuccess
        Send(default(OK), header.Seq);
        SendInventory();
        AfterTakeFromBox(boxId);       // [TodoList/07] กล่องของตกว่างแล้ว → เก็บกล่อง + เอาหมุดออก
    }

    /// <summary>
    /// บอกทุกคนว่าของในกล่องเปลี่ยน — คนอื่นที่เปิดกล่องเดียวกันค้างอยู่จะได้เห็นตรงกัน
    /// </summary>
    private void BroadcastBoxUpdate(string boxId, Item[] added, string[] removedIds)
    {
        _world.BroadcastToViewers(boxId, new InventoryUpdated
        {
            EntityId = boxId,
            Items = added,
            RemovedItemIds = removedIds
        });
    }
}
