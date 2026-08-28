# กล่องเก็บของ (เฟส C)

ไฟล์: `ServerCore/ServerPlayer.Storage.cs` + ส่วนที่แทรกใน `ServerWorld.cs`

ย้ายของระหว่างกระเป๋าผู้เล่น (50 ช่อง) กับสิ่งปลูกสร้างที่เป็นกล่อง (200 ช่อง)

---

## อะไรนับเป็นกล่อง

`ServerWorld.IsStorage(entityId)` — สิ่งปลูกสร้างที่ **blueprint มี component `"Inventory"`**

```csharp
_artifacts มี entityId นี้ไหม
  → _artifactBlueprints[entityId] ได้ blueprintId
    → RecipeData.BlueprintComponents[blueprintId] มี "Inventory" ไหม
```

ตัวอย่าง blueprint ที่เป็นกล่อง: `fur_box_03_leaf` · `secured_box_02` · `classroom_locker_store` · `garden_bin_01`
(component `"Inventory"` โผล่ในตาราง 49 ครั้ง)

> เป็นเหตุผลที่ต้องเก็บ `_artifactBlueprints` แยกไว้ (ทำตอน GP-07) — `AppearArtifact` ไม่มี blueprint id

---

## โปรโตคอล

```
เปิดกล่อง:
  client ─ GetInventory { Target = PropKey{EntityId, Tile} } ─▶ server
         ◀─ Inventory (ของในกล่อง, MaxSize = 200)

ใส่ของ:
  client ─ PutInItem { EntityId, Tile, ItemIds } ─▶ server     (ไม่รอ reply)
         ◀─ InventoryUpdated (ผู้เล่น: -ของ)
         ◀─ InventoryUpdated (กล่อง: +ของ)  ← broadcast ให้ทุกคน
         ◀─ Inventory (กระเป๋าชุดใหม่)

หยิบของ:
  client ─ TakeOutItem { EntityId, Tile, ItemIds } ─▶ server
         ◀─ InventoryUpdated (กล่อง: -ของ)  ← broadcast
         ◀─ InventoryUpdated (ผู้เล่น: +ของ)
         ◀─ OK (replyOf = seq)   ← client เช็คด้วย Packet.IsSuccess
```

`GetInventory` ที่ **ไม่มี `Target`** = ขอกระเป๋าตัวเอง (เส้นทางเดิม `SendInventory()`)

`TakeOutItem` ฝั่ง client ใช้ `.All(packet => onResult(Packet.IsSuccess(packet)))`
— `Packet.IsSuccess` คืน false เฉพาะ TypeCode 1022 / **1024 (Abort)** / 3650
⇒ ตอบ `OK` = สำเร็จ, `Abort` = ล้มเหลว

---

## เมทอด

### `HandleGetInventory`
ไม่มี `Target` → `SendInventory()` · มี `Target` แต่ไม่ใช่กล่อง → `Abort` · เป็นกล่อง → `SendBoxInventory()`

### `HandlePutInItem`
1. ไม่ใช่กล่อง / `ItemIds` ว่าง → `Abort`
2. ดึงของออกจากกระเป๋า **เฉพาะ id ที่มีจริง** — client ส่ง id มั่วมาก็ไม่ทำอะไรหาย
3. ไม่ได้อะไรเลย → `Abort`
4. `TryPutInBox()` — **กล่องเต็ม → คืนของกลับกระเป๋าทั้งหมด** แล้ว `Abort`
   (สำคัญ: ถ้าไม่คืน ของจะหายกลางทาง เพราะเอาออกจากกระเป๋าไปแล้ว)
5. แจ้ง 2 ฝั่ง + `SendInventory()`

### `HandleTakeOutItem`
1. ไม่ใช่กล่อง / ว่าง → `Abort`
2. กระเป๋าเต็ม (50) → `Abort`
3. `TakeFromBox(..., limit: ช่องว่างที่เหลือ)` — **หยิบได้เท่าที่กระเป๋ารับไหว** ไม่ล้น
4. ไม่ได้อะไรเลย → `Abort`
5. แจ้ง 2 ฝั่ง + `OK` + `SendInventory()`

### `BroadcastBoxUpdate`
`InventoryUpdated` ของกล่อง **broadcast ให้ทุกคน** — คนอื่นที่เปิดกล่องเดียวกันค้างอยู่จะได้เห็นตรงกัน
(ต่างจากกระเป๋าผู้เล่นที่ส่งเฉพาะเจ้าตัว)

---

## ฝั่ง `ServerWorld`

| เมทอด | ทำอะไร |
|---|---|
| `IsStorage(id)` | เป็นกล่องไหม |
| `GetBoxItems(id)` | ของในกล่อง (สำเนา) |
| `TryPutInBox(id, items, maxSize)` | ใส่แบบ all-or-nothing — เต็มคืน `false` โดยไม่ใส่อะไรเลย |
| `TakeFromBox(id, ids, limit)` | หยิบตาม id ไม่เกิน limit |
| `TakeAllFromBox(id)` | ดึงของทั้งหมดเพื่อคืนก่อนทุบ |
| `SnapshotBoxes()` / `RestoreBox()` | เซฟ/โหลด |

การทุบกล่องผ่าน `HandleDestructArtifact()` จะดึงของทั้งหมดในกล่องกลับเข้ากระเป๋าของผู้มีสิทธิ์ก่อนเรียก `RemoveArtifact()` จึงไม่ทำของหาย; `RemoveArtifact()` เองยังเป็น cleanup primitive และไม่ควรถูกเรียกจาก gameplay โดยตรงโดยไม่ drain กล่องก่อน

---

## เซฟ

`WorldSave.Boxes` : `entity id ของกล่อง → List<ItemSave>` (กล่องว่างไม่ถูกเขียนลงไฟล์)

---

## ผลทดสอบ

`test-client` ข้อ 24–28:

| ทำอะไร | ผล |
|---|---|
| cheat `add box` แล้ววางลงพื้น | `AppearArtifact type=6171 tile=42,177` |
| เปิดกล่องเปล่า | ตอบ `Inventory` (replyOf ถูกต้อง) |
| ใส่ใบไม้ | `InventoryUpdated ผู้เล่น: +0 -1` · `InventoryUpdated กล่อง: +1 -0` |
| หยิบกลับ | `กล่อง: +0 -1` · `ผู้เล่น: +1 -0` · `OK` |
| ใส่ของลงสิ่งที่ไม่ใช่กล่อง | `Abort` + log `พยายามใส่ของลง ... ที่ไม่ใช่กล่อง` |
| ป้อน `world.json` ที่มีของ 2 ชิ้นในกล่อง แล้วรีสตาร์ท | `โหลดโลกแล้ว: ... กล่องที่มีของ 1 ใบ` |

---

## สิทธิ์และระยะ

การเปิด/อ่านกล่อง (`GetInventory`) และการย้ายของทั้งสองทางใช้ `CanUseBox()` เหมือนกัน: ต้องเป็น storage artifact จริง, เป็นเจ้าของหรือ architect, และอยู่ในระยะเอื้อม จึงไม่เปิดเผยของในกล่องของคนอื่นหรือกล่องที่อยู่ไกล

## ที่ยังไม่ได้ทำ

- **ไม่มีการจัดเรียง/ล็อกไอเทม** (`ItemOrder` / `LockedItemIds` ส่ง null)
- การทุบกล่องคืนของให้ผู้มีสิทธิ์แล้ว; นโยบายดรอปของลงพื้นยังไม่จำเป็นเพราะ S1 ใช้คืนเข้ากระเป๋าโดยตรง
- **ไม่มีการจัดเรียง/ล็อกไอเทม** (`ItemOrder` / `LockedItemIds` ส่ง null)
- **คลังใหญ่ (warehouse)** เป็นคนละระบบ ยังไม่ได้ทำ (`GetWarehouse` / `AddItemsToWarehouse` / `PopItemsFromWarehouse`)
