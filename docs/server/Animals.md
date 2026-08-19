# สัตว์ในโลก (เฟส C — รอบที่ 1)

ไฟล์: `ServerCore/ServerAnimal.cs` · `ServerCore/AnimalSpawner.cs` · `ServerCore/AnimalData.cs` · `scripts/extract_animals.py`

**รอบนี้ทำแค่ "สัตว์โผล่ในโลก + เดินสุ่ม" ยังไม่มีการต่อสู้** — ระบบต่อสู้/ตาย/ฟื้นเป็นรอบถัดไป

---

## `AnimalData.cs` — ตารางสัตว์

**สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
```bash
python scripts/extract_animals.py ../game/DurangoV2_Data/resources.strings.txt ServerCore/AnimalData.cs
```

สัตว์ในเกมคือ entity type **2000–2999** (ตาม enum `EntityType.FIRST_ANIMAL_MODEL_ID = 2000`)
สกัดได้ **213 ชนิด** เก็บ `EntityType` · `Name` · `ModelPath` · `Scale` · `AiFactorId` · `Tamable`

ตัวอย่าง:

| type | ชื่อ | โมเดล | scale | AI |
|---:|---|---|---:|---|
| 2000 | 스테고사우루스 | `Stegosaurus/StegosaurusPrefab` | 1.5 | `stegosaurus_ai` |
| 2001 | 랩터 | `Raptor/RaptorPrefab` | 2.2 | `raptor_ai` (tamable) |
| 2003 | 트리케라톱스 | `Tricera/TriceratopsPrefab` | 1.0 | `triceratops_ai` (tamable) |

> ⚠️ ค่าพลังชีวิต/ดาเมจจริงในข้อมูลเกมเป็น **สูตรข้อความ** เช่น `(0 + combat_level * 5) * unstable_factor`
> ต้องมีตัวคำนวณสูตร (NCalc) มาแปลง ตอนนี้ยังไม่ได้ทำ จึงใช้ค่าคงที่จาก `AnimalSpawner` แทน

---

## `ServerAnimal` — สัตว์ 1 ตัว

| สมาชิก | ทำอะไร |
|---|---|
| `Home` | จุดที่เกิด เดินออกไปไกลกว่า `WanderRadius` ไม่ได้ |
| `Position` / `Yaw` | ตำแหน่งที่ server จำไว้ |
| `ApplyDamage(amount, now)` | ลดเลือด คืน `true` ถ้าตาย — **เตรียมไว้ให้รอบต่อสู้ ยังไม่มีใครเรียก** |
| `MakeAppear()` | สร้าง `AppearAnimal` |
| `MakeMove(dest, speed, now, out travelSeconds)` | สร้าง `Move` 2 จุด (ที่อยู่ปัจจุบัน → ที่หมาย) |

### เรื่อง `MotionName`
`Movement.MotionName` ของสัตว์เป็น `[SerializeField]` ที่ตั้งต่อ prefab ใน Unity (`ClientAnimalActor._movingMotion`)
**server รู้ค่านี้ไม่ได้** จึงส่ง `null` — เหมือนที่โค้ดในเกมเอง (`AnimalManager.MakeAnimal`) ทำ
ตำแหน่งยังซิงก์ถูกต้อง แต่ **อนิเมชันเดินอาจไม่เล่น** ต้องเทสกับเกมจริงถึงจะรู้

---

## `AnimalSpawner` — เกิดและเดิน

ค่าปรับได้ที่หัวไฟล์:

| ค่า | ตัวเลข | หมายเหตุ |
|---|---|---|
| `TargetCount` | 12 | จำนวนสัตว์ในโลก |
| `SpawnRadius` | 6000 | กระจายรอบจุดเกิด (หน่วยโลก = tile × 200 ⇒ 30 tile) |
| `WanderRadius` | 2500 | เดินห่างบ้านตัวเองได้แค่ไหน |
| `WalkSpeed` | 120 | หน่วยโลก/วินาที |
| `RestIntervalMin/Max` | 5–14 วิ | **พักหลังเดินถึงแล้ว** ไม่ใช่ระยะห่างระหว่างคำสั่ง |
| `SpawnTypes` | 2000, 2002, 2003, 2004 | เลือกตัวไม่ดุก่อน เพราะยังไม่มีระบบต่อสู้ |

### `Process()`
- **ไม่มีคนเล่น = ไม่ขยับเลย** ประหยัดทั้ง CPU และแบนด์วิดท์
- ถึงเวลาแล้วสุ่มจุดหมายในรัศมีจากบ้าน แล้ว `Broadcast(Move)`

### บั๊กที่เจอตอนทดสอบ (แก้แล้ว)
รอบแรกตั้ง `NextMoveAt = now + สุ่ม(6–16 วิ)` แต่เวลาเดินจริงยาวได้ถึง **18.9 วิ**

⇒ server สั่งเดินใหม่**ก่อน**สัตว์จะถึงที่หมาย และเพราะ `MakeMove` อัปเดต `Position`
เป็นปลายทางทันที สัตว์จะกระโดดไปข้างหน้าในสายตาผู้เล่น

แก้เป็น `NextMoveAt = now + travelSeconds + พัก` — จำนวนคำสั่งเดินในช่วงทดสอบเดียวกัน
ลดจาก **88 → 38 ครั้ง**

---

## สัตว์ไม่ถูกเซฟ

ตั้งใจ — เปิดเซิร์ฟใหม่ก็เกิดใหม่หมด เพราะสัตว์เป็น**ของชั่วคราวในโลก** ไม่ใช่ความคืบหน้าของผู้เล่น
(ต่างจากบ้าน/ของในกล่องที่ต้องอยู่ถาวร)

`_rng` ใช้ seed คงที่ (12345) ⇒ ตำแหน่งเกิดเหมือนเดิมทุกครั้ง ทำให้ทดสอบซ้ำได้

---

## ผลทดสอบ

`test-client` ข้อ 29:

```
[animal] เกิดสัตว์ 12 ตัวรอบจุดเกิด
[world] player joined: test-client-1, artifacts=0, สัตว์=12

[recv] AppearAnimal animal_9be8b93391e6 type=2000 lv=11 scale=1.5 pos=(12776,37653) life=160
[recv] AppearAnimal animal_3b0c4223d831 type=2004 lv=6  scale=1.27 pos=(10594,39847) life=110
[recv] สัตว์ animal_ffc5c6cee5a0 เดิน (5884,37216) → (7222,38123) ใช้เวลา 13.5 วิ
   สัตว์เดินไปทั้งหมด 38 ครั้ง
```

ครบ: เกิด 12 ตัว · ส่งให้คนที่เข้ามา · type/level/scale/ตำแหน่ง/เลือดถูก · เดินสุ่มด้วยจังหวะสมเหตุผล

---

## ที่ยังไม่ได้ทำ

- **ไม่มีการต่อสู้** — `UseBattleAction` `Damaged` `BattleBegun` `ExitBattle` ยังไม่มี handler
- **ตายไม่ได้** — `ApplyDamage()` พร้อมแล้วแต่ไม่มีใครเรียก, `Remove()` พร้อมแต่ยังไม่ถูกใช้
- **ไม่มี AI** — เดินสุ่มอย่างเดียว ไม่ไล่ ไม่หนี ไม่รวมฝูง (`AiFactorId` เก็บไว้แล้วแต่ยังไม่ใช้)
- **ไม่เกิดใหม่** — ตายแล้วหายเลย ยังไม่มี respawn
- **ไม่เช็คภูมิประเทศ** — สุ่มจุดได้ทั้งในทะเล/บนหน้าผา (ข้อมูล `survivability` ต่อ biome มีในเกมแต่ยังไม่ได้ใช้)
- **ค่าพลังใช้สูตรง่าย ๆ** `life = 50 + level × 10` ไม่ใช่สูตรจริงของเกม
- **อนิเมชันเดินอาจไม่เล่น** เพราะ `MotionName = null` (ดูข้างบน)
