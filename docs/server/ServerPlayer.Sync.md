# `ServerCore/ServerPlayer.Sync.cs`

**หน้าที่:** ยิงสถานะจาก server ไป client — ทั้งตอนเข้าเกมและตอนมีอะไรเปลี่ยน

## `SendSpawnBurst()` — บรรทัด 67 ★ ชุดข้อมูลตอนเข้าเกม

เรียกครั้งเดียวใน `GameServer` ตอนได้ `Ready` **ก่อน** `world.AddPlayer()` ยิงตามลำดับ:

```
SendSkills()            → สกิล + แต้ม
SendInventory()         → กระเป๋า
SendEquipments()        → อุปกรณ์ที่ใส่ (ว่าง)
SendDefoggedChunks()    → เปิดหมอกทั้งแมพ
SendQuestCategories()   → หมวดเควส (เปล่า)
WalletUpdated           → กระเป๋าเงิน (null ทั้งสามช่อง)
MakeAppearPlayer()      → ตัวเราเอง
```

ลำดับสำคัญ: `AppearPlayer` ต้องมาท้ายสุด เพราะ client จะเริ่มวาดตัวละครเมื่อได้ packet นี้
ถ้ายิงก่อนข้อมูลอื่น จะเห็นตัวละครโผล่มาก่อนแล้วค่อยมีของ

## `SendTeleport(pos)` — บรรทัด 37

วาร์ปตัวเองโดยส่ง `Move` ที่มี path จุดเดียวคือปลายทาง
`MotionName = "Barehand_Stand"`, `MotionOption = 34` (ยืนเฉย ๆ), `Time = ตอนนี้` → client เห็นเป็นการ "ย้ายตำแหน่งทันที"
ใช้จาก cheat `tp spawn` เท่านั้น

## `SendInventory()` — บรรทัด 87

ส่ง `Inventory` ทั้งใบ ครอบ `lock (_inventory)` ตลอด
- `MaxSize = 50` ตายตัว
- `Items = ว่าง ? null : array` (ส่ง null แทน array ว่างเหมือน `SendSkills`)
- `LockedItemIds` / `ItemOrder` / `ProtectedItems` ยังไม่ได้ทำ

⚠️ ส่ง **ทั้งกระเป๋า** ทุกครั้งที่มีอะไรเปลี่ยนแม้แต่ชิ้นเดียว (เก็บของ 1 ชิ้น = ส่งใหม่หมด)
ยังไม่เป็นปัญหาที่ 50 ช่อง แต่ควรเปลี่ยนไปใช้ `InventoryUpdated` (ส่งเฉพาะที่เปลี่ยน) เมื่อของเยอะขึ้น

## `SendEquipments()` — บรรทัด 112

```csharp
Send(new Equipments { CurrentType = EquipSlotType.Invalid, Presets = null });
```
**ยังไม่ได้ทำระบบสวมใส่** — ตอบเปล่า ๆ ให้ UI ไม่ค้าง
ผลคือคราฟต์ขวานมาก็ใส่ไม่ได้ นี่เป็นงานอันดับ 1 ที่ควรทำต่อ

## `SendDefoggedChunks()` — บรรทัด 121

สร้าง array ของ **ทุก chunk ในแมพ** แล้วส่งไปทีเดียว = เปิดหมอกทั้งแผนที่ให้เลย
ทางลัดที่ทำให้ระบบสำรวจหายไป แต่ก็ทำให้ไม่ต้องเก็บ state ว่าใครเปิดหมอกตรงไหนแล้ว
(บนแมพ 256×256 = 16×16 chunk = 256 จุด ยังส่งทีเดียวได้สบาย)

## `SendQuestCategories()` — บรรทัด 136

`Categories = null` + `Epic` หมวด `"sunset"` ชื่อว่าง — stub ให้ UI เควสเปิดได้โดยไม่พัง

## `MakeAppearPlayer()` — บรรทัด 152 ★

packet ที่บอกทุกคนว่า "ผู้เล่นคนนี้หน้าตาแบบนี้ อยู่ตรงนี้"
ใช้ 2 ที่: ส่งให้ตัวเองตอนเข้าเกม และส่งให้คนอื่นใน `ServerWorld.AddPlayer()`

ประกอบด้วย `EntityId` `EntityType` `Name` `Level` `Title`(ว่าง) `Member`(แคลนว่าง) `Display` `Move` `Survival`

✅ **GP-02 แก้แล้ว**
```csharp
WorldPosition pos = CurrentPosition;   // ตำแหน่งจริง fallback เป็นจุดเกิดถ้ายังไม่เคยขยับ
...
Yaw = CurrentYaw
```
เดิมใช้ `_world.GetEntryPosition()` ตายตัว ทำให้คนเข้าใหม่เห็นคนที่เล่นอยู่ยืนที่จุดเกิด
ตำแหน่งมาจาก `RememberPosition()` ที่อัปเดตทุกครั้งที่ได้ packet `Move` (ดู [Core](ServerPlayer.Core.md))

**ที่ยังค้าง:** ตำแหน่งอยู่ใน RAM — ออกเกมแล้วเข้าใหม่ยังเด้งกลับจุดเกิด (GP-07)

`Survival.Life` ตั้งเป็น Gauge เต็มค่าเดียว ไม่มีการลด — ยังไม่มีระบบเอาชีวิตรอด

## `MakeDisplay()` — บรรทัด 220

มี display ที่โหลดมาได้ → ใช้อันนั้น
ไม่มี → คืน `PlayerDisplay` ที่ **null เกือบทุกฟิลด์** (`Body` `Head` `Hair` `Equip` สีทั้งหมด)
client จะวาดเป็นตัวละคร default — หัวโล้น ไม่มีเสื้อผ้า ตัวสีเทา

ถ้าเห็นตัวละครหน้าตาแบบนั้นในเกม แปลว่า `/sessions` ไม่ได้ส่ง `Display` มา หรือแกะ JSON ไม่ผ่าน
(ดู `ApplyPlayerData` ใน [Core](ServerPlayer.Core.md))
