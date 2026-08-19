# ระบบรอยแยก (Crack / 균열) — เท่าที่กู้ได้จากฝั่ง client

ตรวจ 19 ส.ค. 2026 · แหล่งข้อมูล: `client/` (ILSpy) + `game/DurangoV2_Data/resources.strings.txt`

**สรุปหัวข้อเดียว:** รอยแยกคือ**ประตูไปเกาะไม่เสถียร** — client รู้ว่าประตูบานไหนพาไปเกาะแบบไหน
และรู้ว่า "ความไม่เสถียร" ทำให้สัตว์แรงขึ้นเท่าไร แต่ **ไม่รู้เลยว่าสัตว์ชนิดไหนเกิดตรงไหน**

---

## 1. รอยแยก 1 บาน = อะไรบ้าง

### `Messages/Crack.cs` — สิ่งที่ server บอก client เกี่ยวกับรอยแยกบานหนึ่ง

| ฟิลด์ | ความหมาย |
|---|---|
| `ActivatedSince` / `ActivatedUntil` | หน้าต่างเวลาที่รอยแยกเปิดให้เข้า (nullable) |
| `CurrentInvestment` / `RequiredInvestment` / `InvestmentUnit` | ต้องลงแรง/ลงของเท่าไรถึงจะเปิดได้ ลงทีละกี่หน่วย |
| **`PotentialBiocoms`** (`string[]`) | **รายชื่อ "ชุมชนสิ่งมีชีวิต" ที่อาจเจอในเกาะปลายทาง** |

`PotentialBiocoms` คือจุดเดียวที่รอยแยกเชื่อมกับสัตว์ — และมันเป็น **ข้อความล้วนที่ server ส่งมา**
client แค่เอาไปวาดในกล่อง `Durango.UI/BiocomInfo.cs` (ชื่อไหนยังไม่เคยเจอจะโชว์เป็น `?`)

### `Yaml/Crack.cs` — ค่าคงที่ฝั่ง client

```csharp
public class Crack { public string VoucherId; }   // required_voucher_id — ตั๋วที่ต้องใช้เปิด
```

มีแค่ฟิลด์เดียว

---

## 2. "มีกี่ชนิด" — ตอบ 2 ชั้น

### ชั้นที่ 1 · ปลายทาง: `archipelago_templates` = **86 แบบ (เปิดใช้จริง 77)**

อยู่ที่ `resources.strings.txt` บรรทัด 1,285,767 · หน้าตา 1 รายการ:

```json
"35TeT01": {
  "biome": 0, "level": 35, "role": 4, "max_population": 300,
  "required_unstable_factor": { "min": 1, "max": 10 },
  "start_region_template_id": "ri35te171228",
  "region_template_ids": ["ri35te171228", "ri35teSub01", "ri35teSub02"],
  "active": true, "prerequisite_quests": {}
}
```

| แกน | ค่าที่มีจริง |
|---|---|
| `role` | **4 (Risky) ทั้ง 86 แบบ** — รอยแยกพาไปเกาะอันตรายอย่างเดียว ไม่มีแบบอื่น |
| `level` | 15 · 18 · 20 · 25 · 30 · 35 · 40 · 45 · 50 · 55 · 60 (11 ขั้น) |
| `biome` | ป่าเขตร้อน 17 · ทุนดรา 12 · ทะเลทราย 12 · หนองน้ำ 12 · ทุ่งหิมะ 10 · ภูเขาไฟ 8 · ทุ่งหญ้า 3 · ป่าเขตอบอุ่น 3 |
| `region_template_ids` | รวมแล้ว **129 แมพ** ที่ไม่ซ้ำกัน (1 archipelago มีได้หลายแมพย่อย) |

ชื่อแมพอ่านได้เป็นระบบ: `ri35te171228` = **ri**sky · เลเวล **35** · **te**mperate · วันที่ทำ 2017-12-28
(เกาะที่เซิร์ฟเราใช้อยู่คือ `ri35te` — มาจากชุดนี้ตรง ๆ)

ตัวห้อย `Sub01/Sub02` = แมพย่อยในหมู่เกาะเดียวกัน · `Q01` เทียบกับ `T01` ต่างกันที่มีแมพเควสเพิ่ม
(`ri35teSub03_copper` · `ri35deSub03_car` · `ri40tuSub03_powerbox`)

### ชั้นที่ 2 · ความรุนแรง: `unstable_factors_client` = **10 ระดับ**

```json
"1":  { "required_resistance_level": 0,  "recommend_resistance_level": 1,  "recommend_collecting_power": 53 }
"5":  { "required_resistance_level": 12, "recommend_resistance_level": 19, "recommend_collecting_power": 233 }
"10": { "required_resistance_level": 80, "recommend_resistance_level": 90, "recommend_collecting_power": 632 }
```

`recommend_combat_power` เป็น 100 เท่ากันทั้ง 10 ระดับ (น่าจะเลิกใช้แล้ว)

**⇒ "ชนิดของรอยแยก" ที่แท้จริง = (หมู่เกาะปลายทาง 77 แบบ) × (ความไม่เสถียร 1-10)**
โดยแต่ละหมู่เกาะจำกัดช่วง unstable factor ของตัวเองไว้ใน `required_unstable_factor`
เช่น 27 แบบรับ 1-10 ทั้งช่วง · 18 แบบรับแค่ 1-5 · 18 แบบรับแค่ 6-8 · ที่เหลือล็อกค่าเดียว

---

## 3. ความไม่เสถียรมีผลกับสัตว์ยังไง — **นี่คือของจริงที่กู้มาได้**

ตาราง `animal` (214 ชนิด) เก็บค่าสถานะเป็น**สูตร** ไม่ใช่ตัวเลข และสูตรมีตัวแปร `unstable_factor` อยู่ด้วย

```
life_max      = (1.06  * ((combat_level + 24) ** 2)) * unstable_factor
attack        = (21.5  + combat_level * 0.9)        * unstable_factor
defense       = (0     + combat_level * 5)          * unstable_factor
attack_rating = (0     + combat_level * 6)          * unstable_factor
accuracy      = (0     + combat_level * 5)          * unstable_factor
groggy_max    = (1.145 * ((combat_level+24) ** 2))  * unstable_factor
life_velocity = ...                                 * unstable_factor
```

**7 ฟิลด์นี้มี `unstable_factor` ครบทั้ง 214 ชนิด** (ตรวจแล้ว 214/214 ทุกฟิลด์)

⇒ **รอยแยกไม่ได้เลือกว่าสัตว์ตัวไหนเกิด — มันเป็น "ตัวคูณความแรง" ของสัตว์ทั้งเกาะ**
เกาะเดียวกัน unstable factor 1 กับ 10 คือสัตว์ชุดเดียวกันแต่แรงกว่ากัน 10 เท่า

ค่าที่ **ไม่มี** `unstable_factor`: `stamina_max` (คงที่ 100) · `dodge` · `critical` · `knock_down_duration`

ตัวคุมอีกตัวคือ `combat_level_ranges` ของแต่ละชนิด — ช่วงเลเวลที่ชนิดนั้นโผล่ได้
(ส่วนใหญ่ `[1, 80]` · บางตัวเช่นสัตว์เลี้ยงพิเศษ `[56, 84]`)

---

## 4. ❌ จุดเกิดสัตว์ — **ไม่มีอยู่ในฝั่ง client เลย**

ตรวจครบ 3 ที่แล้วไม่เจอ:

| ที่ตรวจ | ผล |
|---|---|
| ตาราง `animal` — 52 ฟิลด์ต่อชนิด | **ไม่มีฟิลด์ region / biome / habitat / spawn / density สักตัว** |
| ตาราง `region_templates` | **ไม่มีตารางนี้ในดัมป์เลย** — มีแต่ `region_template_ids` ที่อ้างถึงชื่อ |
| ไฟล์ terrain ที่ส่งมากับเกม | 10 ชั้น: `biomes` `elevations` `garden` `humidities` `landmarks` `no_plant` `ocean` `rivers` `temperatures` `waterdepths` — **ไม่มีชั้นไหนเป็นจุดเกิดสัตว์** |

สังเกตว่า **ต้นไม้/ก้อนหินมีจุดวางจริง** (`whole.garden` — server เราอ่านใช้อยู่)
แต่ **สัตว์ไม่มี** ⇒ ในเกมจริง NEXON คิดจุดเกิดสัตว์ที่ server ตอนรันไทม์ ไม่ได้ฝังมากับแมพ

**ข้อมูลนี้กู้กลับมาไม่ได้** เหมือนกับ `exp_amount` ของสัตว์ที่เป็น 0 ทุกตัว
(ทั้งสองอย่างอยู่ฝั่ง server ของ NEXON ซึ่งไม่ได้ติดมากับ client)

---

## 5. เทียบกับที่เซิร์ฟเราทำอยู่

เพราะข้อ 4 เราจึงต้องออกแบบจุดเกิดเองทั้งหมด — ของที่มีตอนนี้:

| กติกาของเรา | อยู่ที่ | มาจากไหน |
|---|---|---|
| ตารางชนิด/โควตา/ช่วงเลเวล | `SpawnTable` + `config.json` → `Spawn` | คิดเอง |
| โซนที่อยู่อาศัย (วัดจากระยะห่างจุดเข้าเกม) | `config.json` → `Zones` | คิดเอง |
| ตัวใหญ่ต้องอยู่ลึกเข้าไปในแผ่นดิน | `MinTilesInland` + `InlandTilesPerSize` | คิดเอง (ใช้ `size_level` ของจริง) |
| เลือด/ดาเมจสัตว์ | `config.json` → `Animals.LifeBase/PerLevel` | คิดเอง |

**ที่ยังไม่ได้ใช้ทั้งที่มีข้อมูลจริงอยู่แล้ว:**

1. **`unstable_factor`** — สูตรจริงมีครบทั้ง 214 ชนิด แต่เซิร์ฟเราคิดเลือด/ดาเมจสัตว์
   จาก `LifeBase + level*LifePerLevel` ที่คิดเอง · ถ้าใส่ `UnstableFactor` ต่อเกาะ
   ลงใน `IslandRegistry`/`config.json` แล้วคูณตามสูตรข้างบน จะได้สเกลความยากแบบเดียวกับเกมจริง
   และได้ "เกาะเดียวกันแต่ยากต่างกัน" ฟรี ๆ — ตรงกับที่ระบบเกาะแยกเลเวลต้องการพอดี
2. **สูตรค่าสถานะสัตว์ของจริง** (`life_max = 1.06*((lv+24)^2)`) — โตแบบกำลังสอง
   ต่างจากของเราที่เป็นเส้นตรง (`30 + lv*8`) · ที่เลเวล 10 ของจริง = 1,155 ของเรา = 110
3. **`biome` + `level` + `required_unstable_factor` ของ archipelago 77 แบบ** — ใช้เป็นพิมพ์เขียว
   ว่าเกาะที่ 2/3/4 ควรเป็นไบโอมอะไร เลเวลเท่าไร แทนที่จะเดาเอง

---

## 6. ระบบรอยแยกในสถานะปัจจุบันของเซิร์ฟเรา

- **ยังไม่ได้ทำเลย** — ไม่มี handler ของ `InvestToCrack` และไม่เคยส่ง `Crack` ให้ client
- เมนู `MenuType.WarpShop` ("워프 유적" วาร์ปโฮลข้ามเกาะ) **ซ่อนไว้แล้ว** ใน `MenuSystem.NotImplementedYet`
- การข้ามเกาะที่เราทำ (`ServerPlayer.Travel`) ใช้วิธีคนละทางกับรอยแยกของเกมจริง:
  ส่ง `Info "##goto host:port"` + `Emigrated` ให้ client ตัดสายแล้วต่อเซิร์ฟใหม่
  (ดู `docs/server/Islands.md`) — ไม่ได้ใช้ระบบรอยแยก/ลงทุน/unstable factor ของเดิมเลย

ถ้าวันหลังจะทำรอยแยกของจริง ต้องมี: `Crack` message · handler `InvestToCrack` ·
ตารางว่ารอยแยกไหนพาไปเกาะไหน (ยืมจาก `archipelago_templates` ได้) · และ `PotentialBiocoms`
ซึ่งต้องมาจากตารางชนิดสัตว์ต่อเกาะที่ **เราต้องเขียนเอง** เพราะของเดิมไม่มี
