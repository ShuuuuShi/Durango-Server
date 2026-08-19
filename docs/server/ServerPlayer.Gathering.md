# `ServerCore/ServerPlayer.Gathering.cs`

**หน้าที่:** เก็บของจากธรรมชาติ — แตะต้นไม้/ก้อนหิน → ดูว่าเก็บอะไรได้ → เก็บ → ได้ไอเทม → ต้นไม้หาย
รวมถึง **แตะสัตว์** (ปุ่มโจมตี) และ **แล่ซากสัตว์** ซึ่งใช้ท่อ `Collectible` เดียวกัน

วงจรเต็ม: `Touch` → `Touched(Collectible)` → `Collect` → `Timer` → (รอ 2 วิ) → `Collected` + `Inventory`

---

## `HandleTouch(msg, header)` — บรรทัด 37

client แตะอะไรสักอย่างในโลก server ต้องบอกว่ามันคืออะไรและทำอะไรกับมันได้

1. `msg.EntityType <= 0` → เมินทันที
1.5 **ถ้า id เป็นสัตว์ที่มีอยู่จริง** → `HandleTouchAnimal()` แล้วจบ (สัตว์ส่ง `Tile = (-1,-1)` มาเสมอ
   ถ้าปล่อยให้ตกไปทางของธรรมชาติจะได้เมนูเปล่า = **ปุ่มโจมตีไม่ขึ้น** ดู [Combat.md](Combat.md))
2. ตั้ง id: ถ้า client ไม่ส่ง `EntityId` มา สร้างจากพิกัดเป็น `natural_{x}_{y}` (id นี้ใช้อ้างอิงต่อใน `Collect`)
3. **ถ้า `EntityType >= 10000`** (= ของธรรมชาติ): ✅ GP-09 ตรวจ 3 ชั้นก่อน
   - `Terrain.TryGetNatural(tile)` — ไม่มีของธรรมชาติที่ tile นั้นจริง = `Abort` (เดิมขุดอากาศได้)
   - `IsWithinReach(tile)` — ไกลเกิน 8 tile จากตำแหน่งล่าสุด = `Abort`
   - ชนิดของที่ใช้มาจาก **garden ของ server** ไม่ใช่ `msg.EntityType` (เดิมเลือกได้ว่าจะให้ต้นไม้ออกอะไร)
   - ผ่านแล้ว `_world.RegisterNaturalTile(id, tile)` — ผูก id ↔ tile ไว้ให้ `Collect` ใช้
   - ใส่ `Interactions = {506, 10268}` (เลข interaction ของ "เก็บ")
   - ขอ generator จาก `_world.GetOrCreateGenerators()` — state เป็น**ของกลางทั้งเซิร์ฟ** ✅ GP-03
   - แนบ `Collectible` ที่มี generator ทั้งหมดกลับไป (เป็นสำเนา ผู้เรียกแก้ของกลางไม่ได้)
4. ตอบ `Touched` ด้วย `header.Seq`

### `IsWithinReach(tile)` / `MaxReachTiles = 8`

1 tile = 200 หน่วยโลก เทียบกับ `CurrentPosition` (= ปลายทางของ `Move` ล่าสุด ดู GP-02)
เผื่อระยะไว้กว้างกว่าที่ client ให้แตะจริง เพราะ server รู้แค่จุดปลายทาง ไม่ได้เห็นตำแหน่งระหว่างเดิน

✅ **GP-10 แก้แล้ว** — `EntityType < 10000` (สิ่งปลูกสร้าง) มีสาขาของตัวเองแล้ว
ดึง blueprint จาก `RecipeData.BlueprintByType` → ใส่ `EntityName` จาก `BlueprintName`
แล้วประกอบรายการ interaction จาก `BlueprintComponents`:

| component | interaction |
|---|---|
| `Workbench` | 501 |
| `Shelter` | 407 |
| `Sanctum` | 503 |
| `Bandstand` | 552 |
| `Inventory` | 404 |

(ค่าพื้นฐาน 103 ใส่ให้ทุกอัน) — กองไฟที่วางแล้วจึงคลิกใช้งานได้จริง

## `MakeGenerators(entityType)` — บรรทัด 105 `static`

สร้างรายการ "เก็บอะไรได้บ้าง" จากตาราง `NaturalData.Map`

```
สำหรับ entry ตัวที่ i:
    Amount   = 3 - (i % 2)      → สลับ 3, 2, 3, 2 ...
    Effort   = 1 + i
    Duration = 1.5 + i
    ToolRequirements = { bare_hands: 1 }     ← มือเปล่าเก็บได้หมด
```
ถ้า `entityType` ไม่อยู่ในตาราง → คืน generator เดียวคือ `leaf` (ใบไม้) เป็น fallback ไม่ปล่อยให้ว่าง

## `HandleTouchAnimal(animal, header)`

| สถานะสัตว์ | ตอบอะไร |
|---|---|
| ยังเป็นอยู่ | `Interactions = [1]` (Attack) + `EntityName` ไทย + `Level` |
| ซาก + ยังแล่ไม่หมด | `Collectible.Generators` จาก `ButcheryData` (ต้องอยู่ในระยะ 8 tile ไม่งั้น `Abort`) |
| ซากที่แล่หมดแล้ว | `Touched` เปล่า (เมนูว่าง — ไม่ใช่ `Abort` เพราะไม่ใช่การโกง) |

## `HandleButchery(corpse, msg, header)` — เส้นทางแล่เนื้อ

เหมือน `HandleCollect` แต่ 3 อย่างต่างกัน (ดู [Combat.md](Combat.md) หัวข้อ "แล่เนื้อ")
1. ระยะคิดจากตำแหน่งซากตรง ๆ ไม่ใช่ tile
2. ใช้ `TryReserveCorpsePart()` — ชิ้นส่วนหนึ่งหมดแล้วยังแล่ชิ้นอื่นต่อได้
3. หน่วงตาม `Duration` ของชิ้นส่วนจริง ๆ (2-4.5 วิ) ไม่ใช่ 2.1 ตายตัวแบบของธรรมชาติ
   · แล่หมดตัว → `Animals.Remove()` ซากหายทันที

## `HandleCollect(msg, header)` — บรรทัด 131

0. **ถ้า id เป็นสัตว์** → `HandleButchery()` แล้วจบ (ซากไม่มี tile ให้ผูก)
0. **✅ GP-09: `_world.TryGetNaturalTile(msg.EntityId)`** — tile มาจากที่ server ผูกไว้ตอน `Touch`
   **ไม่อ่าน `msg.Tile` อีกเลย** ยังไม่เคยแตะ = `Abort` · เดินออกไปไกลระหว่างเก็บ = `Abort`
1. **`_world.TryReserveGenerator(...)` — จอง 1 หน่วยแบบอะตอมมิกทันทีที่ขอ** ✅ GP-03
   หักจำนวนตอนนี้เลย ไม่ใช่ตอนเก็บเสร็จ → สองคนกดพร้อมกันบนหน่วยสุดท้ายจะผ่านคนเดียว
2. จองไม่ได้ (ไม่มีจุดนี้ / ไม่มี generator / หมดแล้ว) → ตอบ `Abort` แล้วจบ
3. ตอบ `Timer { Duration = 2f }` ทันที → client เล่นอนิเมชันเก็บของ
4. สร้างไอเทมไว้ล่วงหน้าด้วย `MakeGatheredItem()`
5. **เข้าคิว `_deferred` ที่ +2.1 วินาที** พอถึงเวลาแล้วค่อย:
   - ส่ง `Collected` (พร้อม `RanOut`) + **broadcast** `CollectibleChanged` ให้ทุกคนที่เปิดจุดนี้ค้างไว้
   - ถ้าหมดจริง → `Terrain.RemoveNatural()` แล้ว **broadcast `DisappearEntityOnTile`** ให้ทุกคนเห็นต้นไม้หาย
   - ใส่ไอเทมลงกระเป๋าแล้ว `SendInventory()`

⚠️ **ที่ยังค้าง**
- หน่วง **2.1 วินาทีตายตัว** ไม่ได้ใช้ `generator.Duration` ที่ตัวเองสร้างมา → อนิเมชันฝั่ง client กับเวลาจริงไม่ตรงกันเมื่อ generator ตัวหลัง ๆ มี Duration 2.5, 3.5 (GP-09b)
- ถ้าผู้เล่นหลุดระหว่าง 2.1 วินาที หน่วยที่จองไว้จะหายไปเฉย ๆ (แลกกับการกันก๊อปของ ซึ่งคุ้มกว่า)

## `MakeGatheredItem(generator)` — บรรทัด 197 `static`

แปลง generator เป็น `Item` — `Id` เป็น GUID ใหม่ทุกครั้ง, `Prototype` = `generator.Id`,
`Durability` เต็ม, สี `FFFFFF` ทั้งสาม, ที่เหลือ null หมด
(ฟิลด์ที่ null คือของที่ยังไม่ได้ทำ: `Tags` `Performance` `RepairRequirement` `EmotionalMotions`)

## `HandleGetCollectible(msg, header)` — บรรทัด 234

client ขอดูสถานะจุดเก็บของอีกรอบ (เช่นเปิด UI ใหม่) — มี state ก็ส่ง generator ปัจจุบันไป ไม่มีก็ส่ง `Generators = null`
ไม่มี side effect ปลอดภัยที่จะเรียกซ้ำ
