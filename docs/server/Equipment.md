# ระบบสวมใส่อุปกรณ์ (เฟส C)

ไฟล์: `ServerCore/ServerPlayer.Equipment.cs` · `ServerCore/EquipData.cs` · `scripts/extract_equip.py`

ก่อนหน้านี้ `SendEquipments()` ตอบ `Presets = null` เป็น stub — คราฟต์ขวานมาก็ใส่ไม่ได้
และที่แย่กว่านั้นคือ **มันทำให้ client โยน NullReferenceException**

---

## บั๊กที่ stub เดิมสร้างไว้

`client/EquipSystem.cs` — `EquipmentsReceived()`:
```csharp
if (!msg.Presets.ContainsKey(key))        // ❌ deref ตรง ๆ ไม่เช็ค null
...
foreach (KeyValuePair<string, Item> itemSlot in equipmentSlot.ItemSlots)   // ❌ เหมือนกัน
```
เราส่ง `Presets = null` ทุกครั้งที่เข้าเกม (`SendSpawnBurst`) → NRE ทันทีที่ client ประมวลผล packet นี้
ตอนนี้ `Presets` และ `ItemSlots` **ไม่มีวันเป็น null** แล้ว

---

## โปรโตคอล

```
client                                        server
  │─ Equip { SlotName, SlotType, ItemId, ────▶│ HandleEquip()
  │          Action = "equip"/"unequip" }     │   ตรวจว่ามีของจริงในกระเป๋า
  │                                           │   อัปเดต _equippedItems
  │◀── Equipments (replyOf = seq เดิม) ────────│   RebuildEquipments()
  │                                           │
  │◀── PlayerDisplay (broadcast ให้ทุกคน) ─────│   คนอื่นเห็นหน้าตาใหม่
```

client ส่งด้วย `.All(...)` = **รอ reply ของ seq นั้น** ถ้า server ไม่ตอบอะไรเลย UI จะค้าง
เลยต้องตอบ `Equipments` เมื่อสำเร็จ และ `Abort` เมื่อปฏิเสธ

---

## `ServerPlayer.Equipment.cs`

| สมาชิก | ทำอะไร |
|---|---|
| `_equippedItems` | `ช่อง → item id` เช่น `"main" → guid ของขวาน` |
| `_display` / `_displayReady` | หน้าตาปัจจุบัน = พื้นฐาน + อุปกรณ์ |
| `IsMale` | `EntityType != 1001` ใช้เลือกโมเดลเกราะชาย/หญิง |
| `BaseDisplay()` | หน้าตาพื้นฐานจาก `/sessions` หรือไฟล์เซฟของเกม |
| `CurrentDisplay` | property ที่ `MakeAppearPlayer()` เรียกใช้ |

### `HandleEquip(msg, header)`
```
Action == "equip":
    ไม่มีของใน _inventory → log + Abort         ← กัน client ส่ง id มั่ว
    _equippedItems[SlotName] = ItemId
มิฉะนั้น:
    Remove ไม่สำเร็จ (ช่องว่างอยู่แล้ว) → Abort   ← ไม่งั้น client รอค้าง
→ MarkDirty() → Send(Equipments, header.Seq) → Broadcast(_display)
```

### `RebuildEquipments()`
ตรรกะเดียวกับ offline server เดิมของเกม (`Durango.Offline.Player.UpdateEquipments`):

1. เริ่มจาก `BaseDisplay()` แล้ว **รีเซ็ตส่วนที่อุปกรณ์คุม**
   `Body = DefaultBody`, `Head = null`, `BodyColor = ขาว`, `WeaponInfo/Equip/EquipColor = ล้าง`
2. วนของที่ใส่อยู่ — หาใน `_inventory` ไม่เจอก็ข้าม
3. เจอใน `EquipData.Weapons` → `display.Equip = Model`, `WeaponInfo.WeaponFramework = Framework`
4. เจอใน `EquipData.Armors` → `slot == "body"` ทา `Body`, `slot == "head"` ทา `Head` (เลือกโมเดลตามเพศ)
5. **เก็บกวาดช่องที่ของหายไปแล้ว** (โดนวาง/ใช้ไป) ออกจาก `_equippedItems` แล้ว `MarkDirty()`
6. คืน `Equipments { CurrentType = Slot1, Presets = { [Slot1] = { ItemSlots } } }`

> ตอนนี้ใช้ preset เดียว (`Slot1`) เหมือน offline server เดิม — เกมจริงมี Avatar/Slot1-4

---

## `EquipData.cs` — ตารางโมเดล

**สร้างอัตโนมัติ อย่าแก้ด้วยมือ**

```bash
python scripts/extract_equip.py ../game/DurangoV2_Data/resources.strings.txt ServerCore/EquipData.cs
```

สกัดจากบล็อก `"weapon"` / `"armor"` ของ performances ใน `resources.strings.txt`
(ไฟล์ strings dump ของ `resources.assets`) ได้ **อาวุธ 248 · เกราะ 376**

| ชนิด | ฟิลด์ที่เก็บ |
|---|---|
| `WeaponInfo` | `Model` · `Framework` (onehand/twohand/…) · `Slot` |
| `ArmorInfo` | `MaleModel` · `FemaleModel` · `Slot` (body/head/precious/…) |

รายการที่โมเดลเป็น `"None"` ถูกข้ามตอนสกัด (เป็นของที่ไม่มีรูปร่าง เช่นของสะสม)

> ในเกมจริง client อ่านข้อมูลนี้จาก YAML ฝั่งตัวเอง (`PerformanceYaml`) แต่ server ต้องรู้ด้วย
> เพราะเป็นคนตัดสินว่าใส่ของแล้วหน้าตาเปลี่ยนยังไง แล้ว broadcast ให้คนอื่น

---

## cheat สำหรับทดสอบ

| คำสั่ง | ได้อะไร |
|---|---|
| `add axe` | `axe_onehand_stone_01` (ขวานหิน) |
| `add clothes` | `clothes_builder_01` (ชุดช่าง) |

ตอบกลับพร้อมบอกว่า prototype นั้น **รู้จักโมเดลไหม** — ถ้า "ไม่" แปลว่าไม่มีใน `EquipData`

---

## ผลทดสอบ

`test-client` (ข้อ 13–18) ทดสอบครบทั้ง 5 เส้นทาง:

| # | ทำอะไร | ผลที่ได้ |
|---|---|---|
| 13/14 | cheat เอาขวาน + เสื้อ | `รู้จักโมเดล: ใช่` ทั้งคู่ |
| 15 | ใส่ขวานช่อง `main` | `Equip=Models/Equipment/Melee/tier1x_axe_onehand_stone.fbx` · `Framework=onehand` |
| 16 | ใส่เสื้อช่อง `body` | `Body=Models/PC/Male/Body/m_body_builder.fbx` (ขวานยังอยู่) |
| 17 | ถอดขวาน | `Equip=(ไม่มี)` · `Framework=-` · **เสื้อยังอยู่** |
| 18 | ใส่ของที่ไม่มีในกระเป๋า | `Abort` + log `ปฏิเสธ: ... ไม่มีไอเทม` |

เซฟ/โหลดผ่าน: `EquippedItems: {"body": "47204e06-..."}` — ช่อง `main` หายไปถูกต้องหลังถอด
รีสตาร์ทแล้วโหลดของกลับมาครบ 3 ชิ้น

---

## ที่ยังไม่ได้ทำ

- **preset เดียว** — เกมจริงมี Avatar/Slot1–4 สลับชุดได้ (`ChangeEquipSlotType` ยังไม่มี handler)
- **ไม่มีผลต่อค่าสถานะ** — ใส่ขวานแล้วไม่ได้ attack เพิ่ม เพราะ `SendStatistics()` ยังส่งค่าตายตัว
- **ไม่ตรวจว่าของนั้นใส่ช่องนั้นได้ไหม** — client ส่ง `SlotName` อะไรมาก็รับ (เอาขวานใส่ช่อง `head` ได้)
  ถ้าจะกันต้องเทียบกับ `EquipData.*.Slot`
- **`AttachAccessory` / `ResetAccessory` / `GetAttachableAccessories`** ยังไม่มี handler
- **ความทนทาน (durability)** ยังไม่ลดเมื่อใช้งาน
