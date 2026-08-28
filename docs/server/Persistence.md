# ระบบเซฟ (GP-07)

ไฟล์ที่เกี่ยวข้อง: `ServerCore/SaveStore.cs` · `ServerCore/SaveModels.cs` · `ServerCore/ServerPlayer.Persistence.cs` · `ServerCore/ArtifactFactory.cs`
(+ ส่วนที่แทรกใน `ServerWorld.cs` · `TerrainStore.cs` · `GameServer.cs` · `Program.cs`)

ก่อนหน้านี้ state ทั้งหมดอยู่ใน RAM ล้วน — ออกเกม/ปิดเซิร์ฟทีของ สกิล บ้าน ต้นไม้ หายเกลี้ยง

---

## ไฟล์เซฟหน้าตาแบบไหน

```
server/saves/
├── world.json                      สิ่งปลูกสร้าง + ต้นไม้ที่ถูกเก็บไปแล้ว
└── players/
    └── <entityId>.json             ของ สกิล แต้ม ตำแหน่ง
```

เปลี่ยนที่เก็บได้ด้วย `--saves <path>` ตอนรันเซิร์ฟ

ตัวอย่างไฟล์ผู้เล่นจริงจากการทดสอบ:
```json
{
  "EntityId": "test-client-1",
  "Name": "test-client-1",
  "Level": 60,
  "PosX": 8040.0, "PosY": 35400.0, "Yaw": 0.0, "HasPosition": true,
  "SkillPoints": 775,
  "Inventory": [ { "Id": "6b547b64-...", "Prototype": "capsulated_bonfire",
                   "Name": "กองไฟ", "CapsuleBlueprintId": "bonfire" } ],
  "KnownSkills": [ { "Category": 7, "SkillId": "gathering", "Levels": { "__base__": 1 } } ],
  "StarterGiven": true,
  "Version": 1
}
```

---

## `SaveStore.cs` — ชั้น I/O

| สมาชิก | ทำอะไร |
|---|---|
| `Root` | โฟลเดอร์รากของเซฟ (ตั้งจาก `--saves`) |
| `WorldPath` / `PlayerPath(id)` | ประกอบ path |
| `SafeFileName(raw)` | แทนอักขระที่ใช้เป็นชื่อไฟล์ไม่ได้ด้วย `_` + ตัดที่ 120 ตัว — **entity id มาจาก client จึงเชื่อไม่ได้** |
| `Load<T>(path)` | อ่าน JSON คืน `null` ถ้าไม่มีไฟล์/พัง (log แล้วไปต่อ ไม่ throw) |
| `Save<T>(path, data)` | เขียนแบบปลอดภัย ↓ |

**เขียนแบบ tmp-then-swap:**
```csharp
File.WriteAllText(path + ".tmp", json);
File.Move(tmp, path, overwrite: true);   // atomic บนโวลุ่มเดียวกัน
```
ถ้าเขียนทับตรง ๆ แล้วเซิร์ฟดับกลางคัน จะได้ไฟล์ JSON ที่ไม่ครบ = โหลดกลับไม่ได้เลย
วิธีนี้ไฟล์เดิมอยู่ครบจนกว่าตัวใหม่จะเขียนเสร็จ

---

## `SaveModels.cs` — รูปแบบข้อมูล

**ทำไมไม่ serialize struct ของ `Messages/` ตรง ๆ**
1. `Item.Ext` เป็น `object` — Newtonsoft deserialize กลับมาได้ `JObject` ไม่ใช่ชนิดเดิม ต้องพึ่ง `TypeNameHandling` ซึ่งทำให้ไฟล์อ่านยากและผูกกับชื่อ assembly
2. struct ใน `Messages/` ต้องตรงกับ client เป๊ะ ๆ ห้ามแตะ — ถ้าผูกไฟล์เซฟไว้กับมัน วันหลังอัปเดต client แล้วเซฟเก่าพังทันที

| คลาส | เก็บอะไร |
|---|---|
| `SaveEnvelope` | `Version` — เพิ่มเลขเมื่อโครงสร้างเปลี่ยนจนอ่านของเก่าไม่ได้ |
| `ItemSave` | Id, Prototype, ชื่อ, ไอคอน, Level, Size, GeneratorId, `CapsuleBlueprintId` |
| `SkillBundleSave` | Category, SkillId, Levels |
| `PlayerSave` | ข้อมูลผู้เล่น + `StarterGiven` |
| `ArtifactSave` | ทุกอย่างที่ `ArtifactFactory.Make()` ต้องใช้ |
| `WorldSave` | TerrainId, Artifacts, RemovedNaturals |

> ⚠️ **ข้อแลก:** ฟิลด์ที่ไม่ได้อยู่ในโมเดลจะไม่ถูกเก็บ ตอนนี้ครอบคลุมของที่มีจริงครบแล้ว
> (ไอเทมที่คราฟต์/เก็บได้ยังไม่มี `Tags`/`Performance`) **ถ้าวันหลังไอเทมมีข้อมูลมากขึ้นต้องมาเพิ่มที่นี่ด้วย**

---

## `ServerPlayer.Persistence.cs`

### `LoadPersistedState()`
เรียกท้าย constructor — **หลัง** `ApplyPlayerData()` เพราะข้อมูลจาก `/sessions` ใช้แค่ ชื่อ/เลเวล/หน้าตา
ส่วนของในกระเป๋าเป็นของฝั่ง server (client แก้ไฟล์เซฟตัวเองไม่มีผลกับของ)

```
ไม่มีไฟล์เซฟ → GrantStarterItems() (กองไฟ 1 อัน) แล้วจบ   ← ผู้เล่นใหม่
มีไฟล์เซฟ   → เคลียร์ _inventory แล้วโหลดใหม่จากไฟล์
              โหลดสกิล/แต้ม/ตำแหน่ง
              _starterGiven = save.StarterGiven
```

### `GrantStarterItems()`
แจกกองไฟครั้งเดียวตลอดกาล — เดิม `ApplyPlayerData()` แจกทุกครั้งที่ login
พอมีเซฟแล้วถ้าไม่ย้ายออกมา ผู้เล่นจะสะสมกองไฟเพิ่มทุกรอบที่เข้าเกม

### `Save()`
แปลง state เป็น `PlayerSave` แล้วเขียนลงดิสก์ อ่าน `_inventory` ใน `lock`

### `MarkDirty()` / `IsDirty`
autosave ข้ามผู้เล่นที่ไม่มีอะไรเปลี่ยน จุดที่เรียก `MarkDirty()`:

| ที่ | ตอนไหน |
|---|---|
| `ServerPlayer.Core` — `RememberPosition()` | เดิน |
| `ServerPlayer.Gathering` — หลังใส่ของลงกระเป๋า | เก็บของ |
| `ServerPlayer.Crafting` — หลังคราฟต์เสร็จ | คราฟต์ |
| `ServerPlayer.Building` — หลังวางแคปซูล | ของออกจากกระเป๋า |
| `ServerPlayer.Skills` — เรียน/ลืมสกิล | สกิล |

---

## ฝั่งโลก

### `ServerWorld.Save()` / `Load()`
- `Save()` รวม artifact ทั้งหมด + `Terrain.GetRemovedNaturals()` → `world.json`
- `Load()` เรียกใน `Program.cs` **ก่อนเปิดรับ client**
  - artifact ตัวไหน deserialize ไม่ได้ → log แล้วข้าม **ไม่ทำให้โลกทั้งใบโหลดไม่ได้**
  - `TerrainId` ในเซฟไม่ตรงกับแมพที่โหลด → เตือน แต่ยังโหลดต่อ

### `ServerWorld.SaveAll(force)`
เซฟโลก + ผู้เล่นที่ยังออนไลน์ `force: false` (autosave) ข้ามตัวที่ไม่ dirty · `force: true` (ปิดเซิร์ฟ) เซฟหมด

### `_artifactBlueprints`
`blueprintId` ไม่มีอยู่ใน `AppearArtifact` แต่จำเป็นตอนสร้างกลับ (ใช้หา default look / เช็คว่าเป็น Burnable)
เลยต้องเก็บ map แยก `entityId → blueprintId`

### `TerrainStore` — ต้นไม้ที่ถูกเก็บไปแล้ว
เก็บเป็น **รายการพิกัดที่ถูกลบ** (`_removedNaturals`) ไม่ใช่ dump `Garden` ทั้งก้อน
เพราะ `Garden` derive มาจากไฟล์ terrain — ถ้าวันหลังเปลี่ยนแมพ ไฟล์เซฟเก่าก็ยังใช้ได้ (พิกัดไหนไม่มีของก็ข้ามไป)

### `ArtifactFactory.Make()`
ย้ายตัวสร้าง `AppearArtifact` ออกจาก `ServerPlayer.Building` มาเป็น `static`
เพราะตอนโหลดเซฟ `ServerWorld` ต้องสร้าง artifact เองโดยไม่มี `ServerPlayer` ให้อ้างอิง

---

## เซฟตอนไหนบ้าง

| จังหวะ | เซฟอะไร | โค้ด |
|---|---|---|
| ผู้เล่นออกเกม | ผู้เล่นคนนั้น | `GameServer` — `ConnetionClosed` (เซฟ**ก่อน** `RemovePlayer`) |
| ทุก 60 วินาที | ทุกอย่างที่ dirty | `Program.cs` — `AutoSaveIntervalSeconds` |
| กด Ctrl+C | ทุกอย่าง (force) | `Program.cs` — `Console.CancelKeyPress` + `e.Cancel = true` |

---

## ผลทดสอบ

ทดสอบด้วย `test-client` (`dotnet run 127.0.0.1 8191`) รัน 3 รอบ:

| รอบ | log ฝั่ง server | ผล |
|---|---|---|
| 1 | `ผู้เล่นใหม่ test-client-1 — แจกของเริ่มต้น` | สร้าง `players/test-client-1.json` — ตำแหน่ง (8040, 35400), แต้ม 776, กองไฟ 1 |
| 2 | `โหลด test-client-1: ของ 1 ชิ้น, สกิล 1 ตัว, แต้ม 776, ตำแหน่ง จำได้` | **กองไฟยัง 1 อัน** (ไม่แจกซ้ำ) แต้มลดเป็น 775 = สะสมต่อจากเดิมจริง |
| 3 | `โหลดโลกแล้ว: สิ่งปลูกสร้าง 1 ชิ้น` + `artifacts=1` | ป้อน `world.json` มือ → artifact ถูกสร้างกลับและส่งให้ผู้เล่นที่เข้ามา |

**ที่ยังไม่ได้ทดสอบ:** เส้นทาง Ctrl+C (`CancelKeyPress`) และการสร้าง/ทุบสิ่งปลูกสร้างจริงผ่านเกม
— `test-client` ยังไม่ส่ง `OccupyArtifactSite` / `PlaceCapsulatedArtifact` ต้องเทสด้วยเกมจริง

---

## ที่ยังไม่ครอบคลุม

- ✅ **เลเวล** ใช้ค่าใน `PlayerSave` เป็นหลักแล้ว (GP-14) ค่าจาก client มีผลเฉพาะ login แรก
  `--trust-client-profile` = กลับไปเชื่อ client ทุกครั้ง
  **หน้าตา** ยังให้ client เปลี่ยนได้ตามเกาะตัวเอง (เป็นเรื่องความสวยงาม) แต่เก็บลง `PlayerSave.DisplayJson`
  ไว้ใช้ตอน login ที่ไม่มีข้อมูลจากเกาะ
- **`_skills`** (`Dictionary<Category, SkillCategory>` จาก `/sessions`) ยังไม่ถูกเซฟ — เซฟแค่ `_knownSkills`
- Save schema ใช้ `SaveEnvelope.CurrentVersion`; input legacy จะถูก normalize ก่อนใช้ แต่ future version หรือ JSON ที่อ่านไม่ได้จะถูกกักกันเป็น `.rejected-<UTC timestamp>` และไม่ถูกสร้างตัวละครใหม่ทับ
- หาก primary ไม่มีแต่มี `<save>.tmp` ที่อ่านได้ จะกู้เป็น primary ตอนเปิด; atomic write ยังเป็นรายไฟล์ ไม่ใช่ transaction ร่วมระหว่าง world/player/account
- ขั้นตอน backup/restore และ restore drill อยู่ที่ [`S0-FOUNDATION.md`](S0-FOUNDATION.md#backup-and-restore-procedure); helper ในเมนูใช้สำหรับ snapshot local ขณะ server หยุดเท่านั้น
