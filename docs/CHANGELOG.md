# CHANGELOG

## 2026-08-22 — ตัวละครที่สร้างมาถึงเซิร์ฟครบ (ชื่อ/เพศ/หน้าตา) + ซ่อมชื่อไทยใน config

### 🐛 1. สร้างตัวละครแล้วชื่อ/โมเดลไม่ตรง — เจอต้นเหตุแล้ว

อาการเดิมใน log: `[gateway] session player: … name=(ว่าง!) level=0 display=no`
และตัวละครทุกคนหน้าตาเหมือนกันหมด (ผมล้าน ชุดทหาร ตัวขนาดเดียวกัน)

**ต้นเหตุ:** `POST /players` ฝั่งเซิร์ฟ **ทิ้งทุกอย่างที่หน้าสร้างส่งมา** — รับแค่ `name`
มาลง dictionary ในหน่วยความจำ แล้วสุ่ม GUID คืนไป ส่วน `gender` / `model_info` ไม่ถูกอ่านเลย
พอ client เรียก `/sessions` ต่อ (ส่งมาแค่ entity id) เซิร์ฟจึงไม่มีอะไรจะบอกได้
⇒ ตัวละครใหม่เกิดมาไม่มีชื่อ หน้าตาเป็นค่า default

**แก้ 3 จุด**

| ที่ | ทำอะไร |
|---|---|
| `server/ServerCore/Gateway.cs` `/players` | อ่าน `name` / `gender` / `model_info` ครบ → แปลงเป็น `PlayerDisplay` แล้ว**เขียน `saves/players/<id>.json` ตั้งแต่ตอนสร้าง** |
| `server/ServerCore/Gateway.cs` `/sessions` | client ส่งมาแค่ id = เติมชื่อ/เลเวล/หน้าตาจากไฟล์เซฟให้ (ก่อนถึง `AccountStore.TryClaim` ซึ่งใช้ชื่อจากตรงนี้) |
| `client/Durango.Prologue/PrologueManager.cs` | หลัง `/players` สำเร็จ เอา display + เพศที่ปั้นไว้ใส่ `PlayerContext` แล้ว **`Save()` ลงดิสก์** (เดิมจำแค่ id/ชื่อ ในหน่วยความจำ ปิดเกมแล้วหาย) |

**เทสใหม่ `--create-check` 12 ข้อ** (`test-client/CreateCharacterCheck.cs`)
ไล่ตั้งแต่ `POST /players` ➜ `/sessions` แบบส่งแค่ id ➜ เข้าเกม แล้วอ่าน `AppearPlayer` ของตัวเอง
เช็คว่า ชื่อ · เพศ (1001) · ทรงผม · สีผิว · ขนาดตัว · เสียง · ร่างเปล่าตามเพศ ตรงกับที่สร้าง

### 🐛 2. ชื่อไทยใน `data/config.json` กลายเป็นตัวขยะ

log ขึ้น `[animal]   เน€เธยเน€เธเธ...` ทั้งที่ข้อความรอบ ๆ ในบรรทัดเดียวกันเป็นไทยปกติ
⇒ ไม่ใช่เรื่อง console encoding แต่**ตัวข้อมูลในไฟล์พังเอง**

ไล่ดูแล้ว `server/data/config.json` มีชื่อเสีย 14 จุด (สัตว์ 10 + โซน 4) จากการที่มีคน/เครื่องมือ
อ่านไฟล์ UTF-8 นี้เป็น ANSI (cp874) แล้วเขียนกลับ — ข้อมูลหายจริง กู้กลับไม่ได้
(ไฟล์ `data/config.json` ที่รากโปรเจกต์ยังดีอยู่ ไม่ได้โดน)

**แก้:** `ConfigRoot.RepairMangledNames()` — ตอนโหลด config ถ้าชื่อมีร่องรอยการแปลงรหัสผิด
(U+FFFD · อักขระคุม C1 · `€`) ให้ใช้ชื่อตั้งต้นจากโค้ดแทน แล้ว `Reload` เขียนไฟล์กลับให้เอง
⇒ **ไฟล์ซ่อมตัวเองรอบเดียวจบ** และถ้าโดนอีกก็ซ่อมเองอีก · ชื่อที่ผู้ดูแลตั้งเองไม่ถูกแตะ

### ผลชุดเทส
`create 12` · `gp 45` · `smoke` · `character 17` · `multi 9`  ⇒ ผ่านหมด
`quest 30/33` — ตก 3 ข้อ (แพ · สวมอุปกรณ์ · ปลูกผัก) **ค้างมาก่อนรอบนี้** ยังไม่ได้ไล่

## 2026-08-19 (รอบ 4) — เอารายการตรวจเซิร์ฟมาใส่เป็นเควส

รายละเอียดเต็ม: [docs/server/Quest-Checklist.md](server/Quest-Checklist.md)

### ไอเดีย ★

> **หน้าต่างเควสในเกม = เช็คลิสต์เทส**

เกณฑ์เปิด beta ข้อ 3 คือ "เล่นเกมจริง 30 นาทีแล้วดูว่าระบบไหนพัง" — เดิมต้องถือกระดาษไล่เอง
ตอนนี้ยกรายการนั้นมาเป็นเควส 12 ข้อ ⇒ เดินเล่นไปก็รู้เองว่าเหลือระบบไหนยังไม่ได้ลอง
และ **ตัวนับมาจาก server** ข้อไหนขึ้น = packet เดินครบวงจรจริง ไม่ใช่ความจำของคนเทส

ปิดทิ้งตอนเทสผ่านแล้วด้วย `Features.QuestChecklist = false` (hot-reload ไม่ต้อง build)

### 12 ข้อ — เลือกเฉพาะระบบที่สายสอนเล่นไม่ได้แตะ
ปลูก · รดน้ำ · ใส่ปุ๋ย · เก็บเกี่ยว · ตักน้ำ · สวมอุปกรณ์ · ล่าด้วยธนู ·
ซ่อมของ · เก็บของเข้ากล่อง · เรียนสกิล · ตายแล้วฟื้น · กินอาหาร

### ตัวนับใหม่ 9 ตัว
`Water` `Fertilize` `DrawWater` `Equip` `Repair` `Store` `LearnSkill` `Revive` `HuntRanged`
— เกี่ยวเข้าจุดที่ทำงาน**สำเร็จจริง** ไม่ใช่ตอนรับ packet

### 🐛 บั๊กที่เจอเพราะทำอันนี้ (แก้แล้ว 2 ข้อ)

**1. เควสแบบไม่เจาะจงถูกนับสองเด้ง** — ผู้เรียกยิง `QuestProgress` สองครั้งเสมอ
(ทั่วไป + เจาะจง) แต่ตัวกรองปล่อยเควสที่ไม่เจาะจงผ่านทั้งสองครั้ง
⇒ เควส **"สร้างสิ่งปลูกสร้าง 2 อย่าง" จบตั้งแต่สร้างชิ้นแรก**
(เทสเดิมจับไม่ได้เพราะดูแค่ "เสร็จไหม" ไม่ได้ดูว่าใช้กี่ครั้ง)

**2. `cheat questskip` ข้ามเควสต่อแพไปด้วย** — วน `QuestData.All` แล้วเว้นตัวสุดท้าย
พอเพิ่มชุดตรวจต่อท้าย ตัวสุดท้ายกลายเป็นข้อในชุดตรวจ ⇒ เควสปลายสายถูกมาร์คว่าเสร็จ
แก้ให้วนเฉพาะ `QuestData.Story`

### ผลชุดเทสเต็ม
`quest 33` · `farm 39` · `gp 45` · `vision 12` · `multi 9` · `stat 19` · `character 17` · `skill 13` · `cook 11`
⇒ **รวม 198 ข้อ ตก 0**

## 2026-08-19 (รอบ 3) — ระบบปลูกผัก

รายละเอียดเต็ม: [docs/server/Farming.md](server/Farming.md) · เทส: `เทสเกม.bat` ข้อ **25**

### บทเรียนหลักของรอบนี้ ★

> **การเก็บเกี่ยวไม่มี packet ของตัวเอง — client ใช้เมนู "เก็บ" ชุดเดียวกับของธรรมชาติ**

ฝั่ง client ไม่มี `Interaction.Harvest` เลย มีแค่ ปลูก/ใส่ปุ๋ย/รดน้ำ/ถอน (508-511)
ที่ผูกกับ component `Growable` ⇒ พอต้นโตแล้ว server แค่ตั้ง `Generator` ให้ artifact ชิ้นนั้น
แล้วปล่อยให้ `Touch → Collectible → Collect` ที่มีอยู่แล้วทำงานต่อ เหมือนแล่ซากสัตว์เป๊ะ ๆ

### ของใหม่
| อย่าง | รายละเอียด |
|---|---|
| `CropData.cs` | พืช **53 ชนิด** + ปุ๋ย 15 ชนิด + ภาชนะตักน้ำ 34 ชนิด (สร้างจากไฟล์เกม) |
| handler 5 ตัว | `PlantSeed` · `WaterPlant` · `FertilizePlant` · `UprootPlant` · `DrawWater` |
| เก็บเกี่ยว | ผ่านทาง `Touch`/`Collect` เดิม — ได้ผลผลิต + เมล็ดคืน |
| เดินเวลา | `ServerWorld.TickFarms` ทุก 1 วิ · เซฟ/โหลดข้ามการรีสตาร์ท |
| `FarmingConfig` | ปรับความเร็วโต/น้ำ/ปุ๋ย/สตามินาได้สดใน `data/config.json` |
| cheat | `farm` · `seeds` · `grow` · `farms` · `save` |

### กติกาที่เขียนเอง (ข้อมูลเกมไม่ได้บอก)
- **เวลาโตย่อ 1/20** (`GrowthScale` 0.05) — ของจริงต้นไม้ผลใช้ 21 ชั่วโมง
- **น้ำไม่ครบ = ตาย** โดยใช้ `survivability` ของพืชเป็นเกณฑ์ผ่อนผัน
- **ปุ๋ยกำหนดจำนวนผลผลิต** (ข้าวโพดใส่เต็มได้ 6 ชิ้น ไม่ใส่ได้ 1)
- **ผิดไบโอม** โตช้าลง 1.5 เท่า และได้ผลผลิตครึ่งเดียว

### ช่องโหว่ที่ปิดตั้งแต่แรก
- ส่ง id ไอเทมชิ้นเดียวซ้ำ 10 ครั้งในลิสต์เดียว → `HashSet` กันซ้ำก่อนหัก
- **รีสตาร์ทเซิร์ฟแล้วผลผลิตเกิดใหม่** → เซฟจำนวนที่เหลือจริงจาก generator
  (เทสไว้ที่ `--farm-resume-check` รัน 2 เฟสคั่นด้วยการรีสตาร์ท — ยืนยันแล้วว่าเหลือ 2 ไม่ใช่ 3)
- ปลูก/รดน้ำ/เก็บแปลงของคนอื่น · ทำจากอีกฟากเกาะ · เก็บเกินจำนวนที่มี

### กับดักในไฟล์ข้อมูลเกมที่เจอใหม่
1. **asset `performance` มี 3 ก้อน** และมี asset ดิบ (`color_cloth.raw`) แทรกกลาง
   ⇒ กฎ "เจอบรรทัดชิดซ้าย = จบ asset" ตัดกลางคัน ตาราง `fertilizer` หายทั้งก้อน
   แก้ด้วยการอ่านตารางย่อยแบบ**นับปีกกา**
2. **ข้อมูลเกมไม่มี tag ของผลผลิต** — `corn_crop` ไม่มีที่ไหนบอกว่าเป็นธัญพืช
   ⇒ ต้องเดาจาก tag ของเมล็ด ไม่งั้นข้าวโพดที่ปลูกเองเอาไปทำอาหารไม่ได้

## 2026-08-19 (รอบ 2) — ระบบเควสเสถียร + ตรวจระบบสร้าง/ปลูกผัก

รายละเอียด: [docs/server/Building-Audit.md](server/Building-Audit.md)

### แก้บั๊กระบบสร้าง 4 ข้อ
| # | บั๊ก | ผลกระทบ |
|---|---|---|
| 1 | `BuildArtifact` ไม่เช็คสถานะ ⇒ สร้างซ้ำใส่ของเดิมได้ไม่จำกัด | 🔴 ปั๊ม exp · ความชำนาญ · **ความคืบหน้าเควส** |
| 2 | ขนาดสิ่งปลูกสร้างเชื่อ client ⇒ จอง 200×200 tile ได้ | 🔴 จองที่ทั้งย่านด้วยของชิ้นเดียว |
| 3 | เช็คทับซ้อนแค่ tile มุม | 🟠 วางบ้านซ้อนกันได้ |
| 4 | `DestructArtifact` ไม่เช็คระยะ | 🟡 ทุบข้ามเกาะได้ |

### ระบบปลูกผัก — ยังไม่มีในเซิร์ฟ
ตรวจแล้วไม่มีโค้ดสักบรรทัด (`Features.Farming` ประกาศไว้แต่ไม่มีใครเช็ค)
ฝั่ง client มี packet ครบ 6 ตัว + ข้อมูล `crops`/`crop_data` พร้อมแล้ว ⇒ ทำเป็นงานแยกรอบเดียวจบ

### เทสเสถียรขึ้น
- `cheat heal` ตัวใหม่ (ฟื้นเต็ม + ล้างความล้า) — แก้ `gp-check` ตกสุ่มเพราะไฟล์เซฟค้างที่ `Fatigue 87.5`
- `cheat spawn` แนบ `[id=...]` มาด้วย — เทสไม่ต้องเดาว่า "สัตว์ตัวล่าสุดที่โผล่" คือตัวไหน
- `multi-check` เปลี่ยนตัววัดจาก *จำนวนชิ้น* เป็น *จำนวนครั้งที่เก็บสำเร็จ*
  (โบนัสสกิลแถมของโดยไม่กินหน่วย ⇒ เทสเดิมตกเองเวลาโบนัสติด — เซิร์ฟไม่ได้ปั๊มของ)

### ผลชุดเทสเต็ม (เซิร์ฟรีสตาร์ทใหม่)
`quest 26` · `gp 45` · `vision 12` · `multi 9` · `stat 19` · `character 17` · `skill 13` · `cook 11`
⇒ **รวม 152 ข้อ ตก 0**

## 2026-08-19 — ระบบเควส (สายสอนเล่น → ต่อแพ)

รายละเอียดเต็ม: [docs/server/Quests.md](server/Quests.md) · เทส: `เทสเกม.bat` ข้อ **24**

### บทเรียนหลักของรอบนี้ ★

> **เควส 1,386 อันในข้อมูลเกมมีแต่ "หน้าตา" ไม่มี "สมอง"**

`quests_for_client` มี 8 ฟิลด์ (ชื่อ · คำอธิบาย · ไอคอน · ลำดับ · หมวด · ชนิด · โชว์บน HUD · จบเอง)
**ไม่มีเงื่อนไข ไม่มีเป้าหมาย ไม่มีรางวัลสักฟิลด์** — เหมือน `exp_amount` ของสัตว์ที่เป็น 0 ทุกตัว
⇒ ใช้ **id ของจริง** (ได้ชื่อ/ไอคอน/การจัดกลุ่มของแท้ และได้คำแปลไทยฟรีถ้าวันหลังเปิดแค็ตตาล็อกได้)
แล้ว **เขียนเงื่อนไข/รางวัลเอง** ที่ `QuestData.cs`

อีกอย่างที่เพิ่งรู้: **ชื่อหมวดเควสมาจาก server** (`QuestCategories.Epic.Name`)
⇒ ใส่ภาษาไทยได้เลยวันนี้ ไม่ต้องรอเรื่องฟอนต์/แค็ตตาล็อก

### เพิ่ม

- ✨ **ระบบเควสครบวงจร** — `GetQuests` · `GetQuestState` · `RequestQuestReward` ·
  `NotifyQuestProceed` · `QuestStarted` · `QuestRewardResults` (เดิม `GetQuests` ตอบรายการว่างเฉย ๆ)
- ✨ **สายสอนเล่น 8 ขั้น จบที่ต่อแพหนีเกาะ** — เก็บของ → คราฟต์เครื่องมือ → เก็บท่อนซุง → ล่า →
  แล่ → ทำอาหาร → สร้าง → **ต่อแพ `tutorial_boat`** (`story_enter_safehouse` เควสเนื้อเรื่องจริงของเกม)
  · เรียงให้ผู้เล่นชนกำแพง "ไม่มีขวานก็ตัดไม้ใหญ่ไม่ได้" เองในขั้นที่ 3
- ✨ **ตัวนับไม่ได้เขียนใหม่สักตัว** — เกี่ยวกับ `GainExpForGather/Kill/Butchery/Craft/Build` ที่มีอยู่แล้ว
- ✨ ความคืบหน้า/ทำเสร็จ/รับรางวัลแล้ว เก็บลงไฟล์เซฟผู้เล่น
- ✨ ชื่อหมวดเป็นไทย ("เอาชีวิตรอด") · ข้อความแจ้งรับ/สำเร็จเป็นไทยผ่าน `Info`
- 🔓 เลิกซ่อนเมนู `Quest` + `CategoryToDo` ฝั่ง client (ต้อง build client ใหม่ถึงจะเห็น)
- 🧪 `--quest-check` **ผ่าน 20/20** (ต่อแพด้วย packet จริง `OccupyArtifactSite` → `BuildArtifact`)
- 🧪 `cheat quests` ดูสถานะ · `cheat questskip` ข้ามไปขั้นสุดท้าย · `cheat gather` / `cheat attack` สั่งตัวเองได้แล้ว

### หมายเหตุ

- 🔴 **การสร้างสิ่งปลูกสร้างยังไม่กินวัสดุ** (`PutMaterialsIntoArtifact` ตอบ OK เฉย ๆ)
  ⇒ ต่อแพได้โดยไม่ต้องมีท่อนซุงจริง — เป็นช่องโหว่ของระบบก่อสร้าง ไม่ใช่ของระบบเควส
- ชื่อเควสในหน้าต่างยังเป็นเกาหลีจนกว่าจะเปิดแค็ตตาล็อกไทย (ดู docs/client/TUNING.md §2.1)
- `Features.Quests` ต้องเป็น `true` ใน **`data/config.json`** ด้วย — ค่าในโค้ดไม่ทับไฟล์ที่มีอยู่แล้ว

## 2026-08-19 — ระยะการมองเห็น (interest management)

รายละเอียดเต็ม: [docs/server/Vision.md](server/Vision.md)

### บทเรียนหลักของรอบนี้ ★

> **กรองตอนส่งอย่างเดียวไม่พอ — ต้องมีรอบตรวจ "ใครเข้า/ออกระยะ" ด้วย**

ถ้ากรองแต่ตอน broadcast คนที่เดินเข้ามาใหม่จะ **ไม่มีวันเห็นใครเลย** เพราะเขาได้รับแต่ packet
ของสิ่งที่ตัวเองรู้จักอยู่แล้ว ไม่มีอะไรบอกว่า "ตรงนั้นมีคนอยู่นะ" — ต้องมีตัวคอยส่ง `Appear`
ตอนเข้าระยะและ `Disappear` ตอนออก

และ **Appear ต้องออกทางเดียว** (`Observe*`) เท่านั้น ถ้ายิง `MakeAppear()` ตรง ๆ ผ่าน broadcast
รอบตรวจถัดไปจะเห็นว่า id นั้นยังไม่อยู่ในเซ็ต แล้วส่งซ้ำอีกที

### เพิ่ม

- ✨ **ส่งเฉพาะสิ่งที่อยู่รอบตัว** — เดิม `Broadcast` ส่งให้ทุกคนในเกาะโดยไม่ดูระยะ จาก **47 จุด**
  รวมการเดินของผู้เล่นและของสัตว์ทุกตัว ⇒ โตแบบ N² · ที่ 100 คนคือ **~20,000 packet/วินาที**
  ตอนนี้เหลือ `Broadcast` แบบเดิม **3 จุด และเป็นแชททั้งหมด**
- ✨ `BroadcastToViewers(entityId, msg)` — ข่าวเกี่ยวกับ entity ตัวหนึ่งไปหาเฉพาะคนที่เห็นมันอยู่
  · `BroadcastNear(pos, msg)` — เหตุการณ์ที่ผูกกับจุดในโลก · `Announce*` — ของเกิดใหม่/ถูกลบ
- ✨ **รอบตรวจทุก 0.4 วิ** (`ServerPlayer.Vision`) ส่ง `Appear`/`Disappear` ตอนเข้า-ออกระยะ
  ครอบคลุมทั้งผู้เล่น · สัตว์ · สิ่งปลูกสร้าง
- ✨ **ระยะเข้า 24 tile / ระยะออก 32 tile** — ตั้งไม่เท่ากันโดยตั้งใจ ไม่งั้นคนที่ยืนพอดีขอบ
  จะโผล่-หายรัว ๆ ทุกครั้งที่ขยับไม่กี่ก้าว · ปรับได้ที่ `config.json` → `World` (hot-reload)
- ✨ ตอนเข้าเกมส่งเฉพาะของรอบตัว — เดิมส่ง **สิ่งปลูกสร้างทั้งเกาะ + สัตว์ทั้งเกาะ + ผู้เล่นทุกคน**
  ให้คนที่เพิ่งเข้ามา (ที่ 100 คนคือ ~4,000 `AppearArtifact` ในชุดเดียว)
- 🧪 `--vision-check` **ผ่าน 12/12** · วัดได้ว่าตอนอยู่ไกลกัน **ไม่ได้รับ packet การเดินของอีกฝ่ายเลย (0)**
  และที่จุดเกิดเห็นสัตว์ **17 จาก 34 ตัว** (ตัดครึ่งตั้งแต่ยังไม่มีคนเยอะ)
- 🧪 `cheat tp <tileX> <tileY>` — วาร์ปตัวเองไว้เทสระยะ (เดินจริงติดเพดานความเร็ว M-2)

### หมายเหตุ

- `ViewCulling: false` ใน config = กลับไปพฤติกรรมเดิมทันที ไว้ตัดตัวแปรเวลาสงสัยว่าบั๊กมาจากตรงนี้
- แชท (`SayInExclusiveChannel` / `SayInConversation`) ยังเป็นช่องรวมทั้งเกาะเหมือนเดิม


## 2026-08-18 — เปิดระบบทำอาหาร (Features.Cooking)

รายละเอียดเต็ม: [docs/server/Cooking.md](server/Cooking.md)

### บทเรียนหลักของรอบนี้ ★

> **สูตร 587 จาก 720 อันกดไม่ได้มาตลอด เพราะ server ส่ง `AppearArtifact.Tags = null`**

client ตัดสินว่า "สูตรนี้ทำที่โต๊ะตัวนี้ได้ไหม" จาก tag ของโต๊ะที่ server ส่งมา
(`Crafting.Recipe.IsValidWorkbench` → `workbench.GetTag(...)`) — ส่ง null = หาโต๊ะที่ผ่านไม่เจอสักตัว
ระบบทำอาหารทั้งระบบเลยตายตั้งแต่ยังไม่เริ่ม โดยไม่มี error อะไรให้เห็น

⇒ เจอ "เมนูขึ้นแต่กดไม่ได้" ให้เช็คก่อนว่า **ข้อมูลที่ client ใช้ตัดสินใจ server ส่งไปครบไหม**
(โรคเดียวกับเรื่องสกิลเต็มหมวดของรอบที่แล้ว)

### เพิ่ม

- ✨ **ระบบทำอาหาร** — สูตรหมวด `cook` 152 อัน · **ต้องยืนที่กองไฟ/เตาที่แรงพอ + ถือเครื่องมือที่ถูกชนิด**
  ไล่ระดับ: กองไฟ (cook 15) → กองไฟใหญ่/แคมป์กริล (40) → เตาดิน (45) → ครัว (60)
- ✨ **โภชนาการจริง 352 ชนิด** (`FoodData` จาก TextAsset `performance`) แทนการเดาจากคำในชื่อ prototype
  · **ของดิบให้พลังแค่ 60% และทำให้ล้าเพิ่ม** ⇒ เนื้อดิบ +18.9 · ย่างแล้ว +31.5 · ต้มแล้ว +40
  · มี **เวลาย่อย** — กินติด ๆ กันรวดเดียวทั้งกระเป๋าไม่ได้อีกแล้ว
- ✨ **สูตรแปรรูป (type 1)** — ย่าง/ต้ม/นึ่ง/ทอด/ตากแห้ง 73 อัน: prototype เดิมแต่ตัด tag `raw_food`
  ทิ้งแล้วเติม `taste_good` ตามที่ข้อมูลเกมกำหนด · สภาพ "สุกแล้ว" เซฟลงไฟล์ด้วย
- ✨ **`--recipe-check`** (server) ตรวจข้อมูลคราฟต์/อาหารโดยไม่ต้องเปิดเซิร์ฟ ·
  **`--cook-check`** (test-client) เทสกับเซิร์ฟจริง 11 ข้อ
- ✨ cheat `give <prototype> [จำนวน]` — เสกไอเทมอะไรก็ได้ที่มีในเกม (เทสสูตรได้โดยไม่ต้องออกไปล่า)

### แก้

- 🐛 **`AppearArtifact.Tags` เป็น null เสมอ** → ส่ง tag จริงของโต๊ะ (`WorkbenchTagData`)
  ⇒ ปลดล็อกสูตรที่ต้องใช้โต๊ะทั้ง 587 อัน ไม่ใช่แค่สายทำอาหาร
- 🐛 **วางของจากแคปซูลออกมาเป็น `Occupied`** (= แค่จองพื้นที่) → เปลี่ยนเป็น `Completed`
  ⇒ กองไฟที่วางจากแคปซูลใช้เป็นโต๊ะคราฟต์ได้จริง
- 🐛 **ผลลัพธ์ของสูตรใช้ชื่อสูตรเป็น prototype** → ใช้ `prototype_id` จริง และเลือกตามวัตถุดิบที่ใส่
  (สูตร `broth` ใส่เนื้อได้ `broth_meat` ใส่ผักได้ `broth_vege`)
- 🐛 **คราฟต์อะไรก็ 2 วินาที / 4 สตามินา เท่ากันหมด** → ใช้ `duration`/`energy` จริงของแต่ละสูตร
- 🐛 **สูตรที่ควรได้หลายชิ้นได้ชิ้นเดียว** → ใช้ `count` จริง (น้ำซุปได้ 2 ถ้วย)
- 🐛 **`min_level` ของสูตรไม่มีผล** → ตรวจเลเวลผู้เล่นก่อนคราฟต์
- 🐛 สูตรแก้ทรงเสื้อ (Reform, 22 อัน) เคยคราฟต์ผ่านแล้วได้ของมั่ว → ปฏิเสธพร้อมบอกเหตุผล

### ผลทดสอบ

`--gp-check` **45/45** · `--multi-check` **9/9** · `--cook-check` **11/11** · `--recipe-check` ✅

---

## 2026-08-17 — เทสในเกมจริง: เจอบั๊กชุด "client ใช้ค่าเดิมของตัวเอง"

### บทเรียนหลักของรอบนี้ ★

บั๊ก 3 ตัวที่ผู้เล่นเห็น — **เลเวลค้างที่ 7 · สกิลเต็มทุกหมวด · หลอดสตามินา 999/999** — เป็นโรคเดียวกัน:

> **เซิร์ฟไม่ส่งค่ามา = client ใช้ค่าเดิมของตัวเอง ไม่ใช่ค่าว่าง**

และ "ค่าเดิมของ client" คือตัวละครบนเกาะออฟไลน์ของเครื่องนั้น ซึ่งมัก **เลเวล 60 สกิลเต็ม**

⇒ ทุกอย่างที่ server เป็นเจ้าของ **ต้องส่งให้ครบทุกช่องทุกครั้ง** ไม่ใช่ส่งเฉพาะที่มีค่า
เจออะไรแสดงผลเป็น "เต็ม/สูงผิดปกติ" ให้สงสัยข้อนี้ก่อน

| อาการ | ต้นเหตุ | แก้ |
|---|---|---|
| เลเวลค้างที่ 7 ทั้งที่ลบเซฟแล้ว | รับ `Level` จาก `/sessions` ของ client | เลเวลคิดจาก exp ที่ server เก็บเท่านั้น |
| สกิลเต็มทุกหมวด | ส่ง `Categories` เป็น dict ว่าง · client วนอัปเดต**เฉพาะหมวดที่มีในข้อความ** หมวดที่ไม่ส่งจะไม่ถูกรีเซ็ต | ส่งครบทั้ง 13 หมวดเสมอ (`BuildSkillCategories`) |
| หลอดสตามินา 999/999 | HUD สมัครรับ event หลังจาก `Survival` มาถึงแล้ว | ดึงค่าปัจจุบันมาวาดทันทีตอน HUD เริ่ม |

> รายการสกิลย่อย (`SkillList`) ไม่มีปัญหานี้เพราะ client มีลูปตั้งตัวที่ไม่ได้ส่งมาให้เป็น 0
> แต่ **หมวดสกิลไม่มีลูปนั้น** — ความต่างเล็ก ๆ ที่ทำให้อาการต่างกันสิ้นเชิง

### ปุ่มกากบาทกดไม่ปิด — แก้ผิดจุด 4 รอบกว่าจะเจอ ★

**ต้นเหตุจริง:** `UITitleWidget_PC.OnStart()` ที่ ILSpy ถอดมาได้

```csharp
if (currencies != null && (nint)currencies.LongLength >= 4)
{
    base.OnStart();   // ← ที่ผูก onClick ของปุ่มกากบาท
}
```

การผูกปุ่มปิดถูกครอบด้วยเงื่อนไข *"ต้องมีช่องแสดงสกุลเงินอย่างน้อย 4 ช่อง"*
⇒ หน้าต่างที่ไม่โชว์สกุลเงิน (คราฟ · กระเป๋า · ก่อสร้าง) **ไม่ผูกปุ่มปิดเลย**
⇒ ปุ่มวาดให้เห็นแต่กดแล้วเงียบสนิท **ไม่มี error ใด ๆ**

แก้: ย้าย `base.OnStart()` ออกนอกเงื่อนไข (แก้ `OnEnable` ที่เป็นแบบเดียวกันด้วย)

**วิธีที่ทำให้เจอ — ควรทำตั้งแต่รอบแรก:**
1. ใส่ log ใน `UITitleWidget.OnStart` → **ไม่ขึ้นเลยสักครั้ง** ⇒ ตัดคลาสนี้ออก
2. กดแท็บอื่นในหน้าต่างเดียวกัน → **กดได้ปกติ** ⇒ ไม่ใช่ปัญหาการรับคลิกทั้งหน้าต่าง
3. ไล่ว่าใครสืบทอด `UITitleWidget` → เจอ `_PC` ที่ override แล้วครอบ `base.OnStart()`

> **ใส่ตัวดักก่อนเสมอ อย่าแก้ตามที่เดา** — 4 รอบแรกเป็นการเดาจากโค้ดล้วน ๆ ผิดทั้งหมด

### เกมอืดจนโหลดแมพไม่จบ — exception รัวทุกเฟรม

log บวม **721 KB ใน 2 นาที** · NullReferenceException **4,216 ครั้ง** จาก 3 จุดที่รันทุกเฟรม
(`FatigueGaugeScrollSprite.Update` · `MapIndicators.LateUpdate` · `MoveTrail.Update`)
ทั้งหมดเรียก `PlayerBehavior.LocalPlayer` / SerializeField ก่อนพร้อม โดยไม่เช็ค null

อีกตัว: **`TRex_Stand` เล่นไม่ได้** ทำ log บวม 3 MB (15,308 บรรทัด) — มีใน DLL ต้นฉบับด้วย
แก้ด้วยการเช็ค `Anim[motionName] != null` ก่อนเรียก `Play()`

**ผล: log 721 KB → 4 KB · NRE 4,216 → 0**

### อื่น ๆ

- `GameManager.LimitText` ไม่เช็ค null ⇒ client โยน exception **ทุกครั้งที่ server ตอบ `Abort`**
  (เก็บของไม่มีเครื่องมือ · อยู่ไกลเกินเอื้อม · สตามินาไม่พอ)
- `AnimalManager` handler ปุ่มโจมตี: เรียก `GetTargetComponent<WildAnimalAI>()` โดยไม่เช็ค null
  (สัตว์จาก server เราไม่ได้ผ่าน `PrepareLoad` จึงไม่มี component นี้) — ย้าย `SetInteractionTarget(null)`
  ขึ้นก่อนด้วย เพื่อให้เมนูปิดเสมอแม้ส่วนหลังพัง

### เครื่องมือใหม่: ชุดออฟไลน์เป็น "ตัวควบคุม"

`Durango_Ver_PC_Final` มี `Assembly-CSharp.dll` **ต้นฉบับ 6.02 MB** (ของเรา build เอง 5.8 MB)
`resources.assets` ขนาดเท่ากันเป๊ะ ⇒ **ไฟล์เกมเราไม่ได้เสีย** ตัดสมมติฐานนั้นทิ้งได้

เจออาการแปลก ๆ ให้ **สลับ DLL ต้นฉบับมาเทียบก่อน** — ตัดสินได้ทันทีว่าปัญหาเกิดจากเราหรือจากเกม
⚠️ แต่ DLL ต้นฉบับ **ต่อเซิร์ฟเราไม่ได้** (autoconnect เป็นแพตช์ของเราเอง) มันจะเข้าเกาะออฟไลน์ของตัวเองแทน
— ต้องระวังตอนเทียบผล ไม่งั้นสรุปผิดเหมือนที่เคยพลาดมาแล้ว

## 2026-08-16 — แมพโหลดล่วงหน้า + ตัวใหญ่อยู่กลางเกาะ

### แก้อาการ "วิ่งไปแล้วแมพรีเฟรชเป็นระยะ" ★

เจอ **2 สาเหตุซ้อนกัน** ทั้งคู่อยู่ที่การส่ง/สร้าง chunk (1 chunk = 16×16 tile)

**1. server ส่ง chunk ซ้ำทุกครั้งที่ข้ามขอบ** — ต้นตอจริง
`ChunkPool.LoadChunk()` ฝั่ง client ถ้าได้ข้อมูลของ chunk ที่โหลดไว้แล้ว มัน **`Reset()` ทิ้งแล้วสร้างใหม่ทั้งก้อน**
ไม่ได้เช็คว่าข้อมูลเหมือนเดิมหรือเปล่า ⇒ เดิม `HandleSetChunk` ส่งทั้งกรอบทุกครั้ง = สร้างพื้น/ต้นไม้/หญ้าใหม่ทั้งจอ
ตอนนี้ส่งเฉพาะ chunk ที่ "เพิ่งเข้ามาในกรอบ" (จำกรอบเดิมไว้ใน `_sentChunkCx/Cy/Range`)

**2. ระยะมองเห็นแคบเกิน** — `_visibleRange = 1` (3×3 chunk = 48×48 tile)
chunk ใหม่จึงถูกสร้างในระยะที่ตาเห็นพอดี · เปลี่ยนเป็น 2 (5×5 = 80×80 tile) การโหลดเลยเกิดไกลออกไปอีก 16 tile

**ยืนยันในเกมแล้ว** — เดินข้ามขอบ chunk ตอนนี้ log ขึ้น:

```
[chunk] ฟหกฟหก ย้ายไป chunk 6,3 — ส่ง garden ใหม่ 9 ก้อน   (เดินทแยง)
[chunk] ฟหกฟหก ย้ายไป chunk 6,2 — ส่ง garden ใหม่ 5 ก้อน   (เดินตรง)
```

เดิมเลขนี้คือ 9 ก้อนทุกครั้ง (และจะเป็น 25 ถ้าขยายระยะโดยไม่แก้ข้อ 1) · tps ยังนิ่งที่ 120

⚠️ `World.ChunkSendRange` (server) กับ `_visibleRange` (client) **ต้องเท่ากันเสมอ** — ดู `docs/server/Config.md`

ไฟล์: `ServerCore/ServerPlayer.Core.cs` · `ServerCore/ServerConfig.cs` (หัวข้อ `world` ใหม่) ·
`client/Durango.Terrain/TerrainBase.cs`

### ไดโนเสาร์ตัวใหญ่ย้ายไปอยู่กลางเกาะ

ข้อมูลเกมมีฟิลด์ **`size_level` 1–7** ต่อสัตว์ 1 ชนิด — สกัดเพิ่มลง `AnimalData.SizeLevel` แล้ว (พร้อม `Difficulty`)

> ❌ ใช้ `Scale` แทนไม่ได้ — เป็นตัวคูณของ prefab แต่ละโมเดล เทียบข้ามชนิดไม่ได้
> (แร็ปเตอร์ `Scale 2.2` แต่ตัวเล็กกว่าบราคิโอที่ `Scale 1.27`)

ระยะที่ต้องเกิดลึกจากชายฝั่ง = `MinTilesInland + (size_level − 1) × InlandTilesPerSize` (4 + 3)

| ขนาด | ต้องลึก | ตัวอย่าง |
|---:|---:|---|
| 1 | 4 tile | กิ้งก่า · คอมป์โซกนาทัส |
| 2 | 7 tile | แร็ปเตอร์ · โปรโตเซราท็อปส์ |
| 4 | 13 tile | สเตโกซอรัส · ทริเซราท็อปส์ |

โซนก็ใช้ระยะของตัวที่ใหญ่ที่สุดในโซน + ครึ่งหนึ่งของรัศมีโซน · ถ้าจุดที่ตั้งไว้ตื้นเกิน
จะค้นเป็นวงแล้วเลือก**จุดที่ลึกที่สุด** (เดิมเลือกจุดแรกที่เจอ) · ถ้าไม่เจอเลยใช้
"จุดที่กลางเกาะที่สุด" (`TerrainStore.TryDeepestLand`) — ไม่มีทางตกไปอยู่ริมหาดอีก

เปิดเซิร์ฟแล้วมีบรรทัดสรุปให้ตรวจด้วยตาโดยไม่ต้องเดินหาในเกม:

```
[animal] โซน ที่ราบสูง: จุดที่ตั้งไว้ (tile 32,201) ตื้นเกิน — ย้ายไป tile 67,186 (ลึก 19 tile ต้องการ 18)
[animal]   ทริเซราท็อปส์ (ขนาด 4) ×2 ใกล้ฝั่งสุด 12 tile / ต้องการ 12
[animal]   กิ้งก่า (ขนาด 1) ×6 ใกล้ฝั่งสุด 9 tile / ต้องการ 3
```

ครบทั้ง 10 ชนิดผ่านเกณฑ์ของตัวเอง

ไฟล์: `ServerCore/AnimalSpawner.cs` · `ServerCore/AnimalData.cs` (gen ใหม่) · `ServerCore/TerrainStore.cs` ·
`scripts/extract_animals.py` · เอกสารใหม่ `docs/server/Biomes.md`

## 2026-08-13 — เฟส A: ทำของที่มีอยู่ให้ถูก

แก้ 9 บั๊กจาก [GAMEPLAY-REVIEW](../server/GAMEPLAY-REVIEW.md) ทั้งหมด build ผ่าน 0 errors และรันทดสอบแล้ว

### GP-01 · เพดาน packet — จาก ~64 เป็น ~61,000 ต่อวินาที/คน ★

ปัญหาเป็นสองชั้นที่คูณกัน:

| ชั้น | เดิม | ตอนนี้ |
|---|---|---|
| `Connection.ProcessPacketQueue()` | ดึง packet **1 ตัว** ต่อการเรียก 1 ครั้ง | `while` ระบายทั้งคิว เพดาน 512/tick |
| `Program.cs` main loop | `Thread.Sleep(5)` → บน Windows นอนจริง ~15.6 ms = **~64 tps** | `timeBeginPeriod(1)` + `Stopwatch` ล็อกที่ **120 tps** |

- ฝั่ง client (`client/Durango.Network/Connection.cs`) ใช้ `while` อยู่แล้ว โค้ดสองตัวนี้เป็นแฝดกัน — ฝั่ง server หาย `while` ไป จึงเป็นบั๊กไม่ใช่การออกแบบ
- เปลี่ยนจากถือ lock ตลอดการ process มาเป็น **dequeue ในล็อก / เรียก handler นอกล็อก** เพื่อไม่ให้ handler ที่ทำงานนานบล็อก thread pool ที่กำลัง enqueue
- ชนเพดาน 512 แล้ว `LogWarning` บอกจำนวนที่ค้าง — ไม่ตัดทิ้งเงียบ ๆ
- `timeBeginPeriod` ข้ามให้อัตโนมัติถ้าไม่ใช่ Windows, `timeEndPeriod` อยู่ใน `finally`

**ยืนยันแล้ว:** log ขึ้น `[loop] 120 tps, ผู้เล่นออนไลน์ 0` (เดิมจะได้ ~64)

ไฟล์: `GameCode/Durango.Offline/Connection.cs` · `Program.cs`

### GP-02 · เก็บตำแหน่งผู้เล่น

เดิม `HandleMove` มีบรรทัดเดียวคือ `_world.Broadcast(msg)` — server ไม่รู้ว่าใครอยู่ไหน
ทำให้ `MakeAppearPlayer()` ต้องใช้จุดเกิดเสมอ **คนที่เข้ามาทีหลังเห็นคนที่เล่นอยู่ยืนที่จุดเกิด** จนกว่าเขาจะขยับ

- เพิ่ม `_lastPosition` / `_lastYaw` / `_hasPosition` + property `CurrentPosition` / `CurrentYaw`
- `RememberPosition()` อ่าน `Movements[^1].Path[^1]` ทุกครั้งที่ได้ `Move` (เช็ค null ทุกชั้น)
- `MakeAppearPlayer()` ใช้ `CurrentPosition` / `CurrentYaw` แทนจุดเกิด (fallback เป็นจุดเกิดถ้ายังไม่เคยขยับ)

ไฟล์: `ServerCore/ServerPlayer.Core.cs` · `ServerCore/ServerPlayer.Sync.cs`

### GP-03 · `_generatorState` ย้ายไประดับ world + จองแบบอะตอมมิก

เดิมอยู่ใน `ServerPlayer` = แยกกันคนละชุดต่อผู้เล่น → **2 คนตัดต้นเดียวกันได้ของครบทั้งคู่**
และคนที่เก็บหมดก่อนสั่งลบต้นไม้ขณะที่อีกคนยังเก็บต้นผีต่อได้

- ย้ายไป `ServerWorld._generators` + `_genLock`
- `GetOrCreateGenerators()` / `PeekGenerators()` คืน **สำเนา** เสมอ กันผู้เรียกแก้ของกลาง
- `TryReserveGenerator()` — **หักจำนวนทันทีที่ขอ** ไม่ใช่ตอนเก็บเสร็จ สองคนกดพร้อมกันบนหน่วยสุดท้ายจะผ่านคนเดียว
- `CollectibleChanged` เปลี่ยนจาก `Send` เป็น `Broadcast` เพราะ state เป็นของกลางแล้ว

แลกมาด้วย: ผู้เล่นหลุดระหว่างรอ 2.1 วินาที หน่วยที่จองไว้จะหายไป — คุ้มกว่าปล่อยให้ก๊อปของ

ไฟล์: `ServerCore/ServerWorld.cs` · `ServerCore/ServerPlayer.Gathering.cs` · `ServerCore/ServerPlayer.Core.cs`

### GP-04 · เก็บสิ่งปลูกสร้าง + ส่งให้คนเข้าใหม่ + ตรวจเจ้าของก่อนทุบ

เดิมทุกเมทอด broadcast แล้วทิ้ง `ServerWorld` ไม่มีที่เก็บ artifact เลย

- เพิ่ม `ServerWorld._artifacts` + `AddArtifact` / `TryGetArtifact` / `RemoveArtifact` / `SetArtifactBuildingState` / `SnapshotArtifacts` / `ArtifactCount`
- `AddPlayer()` ยิง `SnapshotArtifacts()` ทั้งชุดให้คนใหม่ก่อนส่ง `AppearPlayer` (log บอกจำนวนท้ายบรรทัด `artifacts=N`)
- `HandleBuildArtifact` อัปเดต `BuildingState` เป็น `Built` ในที่เก็บด้วย
- `HandleDestructArtifact` ตรวจ 2 ชั้น: มีของจริงไหม + `CanModifyArtifact()` (เทียบ `FounderEntityId` / `ArchitectEntityIds`) ไม่ผ่าน = `Abort` + log

ไฟล์: `ServerCore/ServerWorld.cs` · `ServerCore/ServerPlayer.Building.cs`

### GP-05 · แชทมีชื่อคนพูดแล้ว

client เช็ค `if (msg.Message.Speaker.HasValue)` ก่อนตั้งชื่อในกล่องแชท — server ไม่เคยเติมให้ แชทเลยขึ้นแบบไม่มีชื่อ

- เพิ่ม `StampSpeaker()` เติม `Speaker = new RadioId { Name, Freq = 0 }`
- บังคับ `Message.EntityId` เป็นของจริงด้วย กัน client ปลอมเป็นคนอื่นตอนพิมพ์
- log เปลี่ยนไปโชว์ชื่อผู้เล่นแทน entity id

ไฟล์: `ServerCore/ServerPlayer.Core.cs`

### GP-11 · ไม่ทับชื่อเซิร์ฟด้วยชื่อผู้เล่น

ลบ `ServerKnock.HostName = playerName;` ใน `Ready` handler ที่ทับชื่อเซิร์ฟด้วยชื่อคนล่าสุดที่เข้ามา
ทำให้ LAN discovery โชว์ชื่อผิด — ตอนนี้ตั้งครั้งเดียวที่ `Program.cs`

ไฟล์: `ServerCore/GameServer.cs`

### GP-13 · ลบพารามิเตอร์ที่ไม่ถูกใช้

`BroadcastExcept<T>(except, msg, bool excludeSelf = false)` — `excludeSelf` ไม่เคยถูกอ่านในบอดี้เลย
พฤติกรรมถูกอยู่แล้วแต่ชวนเข้าใจผิดว่ามีสวิตช์

ไฟล์: `ServerCore/ServerWorld.cs`

### GP-15 · `Listener` ทน bind ล้มเหลว + ปิด socket ปลอดภัย

เดิม bind ไม่ผ่าน → กลืน exception → `_acceptArgs` เป็น null → `Process()` เรียก `Accept()` ทุก tick
→ `ArgumentNullException` ท่วมคอนโซล และเซิร์ฟดูเหมือนรันอยู่แต่ไม่รับใคร

- `Start()` คืน `bool` + flag `_started` — bind ไม่ผ่านแล้ว `Process()`/`Accept()` return ทันที
- flag `_closing` — callback ที่ยิงมาหลัง `Close()` เก็บกวาด socket แล้วจบ
- `Close()` ใช้ `try/catch` แยกสำหรับ `Shutdown` และ `Close` (เดิม `try/finally` ไม่มี catch → exception ทะลุออก) + `Dispose()` `SocketAsyncEventArgs`
- `Accept()` ห่อ try/catch — เจอ `ObjectDisposedException` ก็หยุดรับ
- `Accept_Completed` แยก `OperationAborted` (ปกติตอนปิด) ออกจาก error จริง
- `Program.cs`: พอร์ตเกม bind ไม่ได้ → `[fatal]` แล้วจบ / radiotower bind ไม่ได้ → `[warn]` แล้วเล่นต่อ

ไฟล์: `GameCode/Durango.Offline/Listener.cs` · `ServerCore/GameServer.cs` · `ServerCore/RadiotowerServer.cs` · `Program.cs`

### GP-10 · `Touch` รองรับสิ่งปลูกสร้าง

(ทำโดยเจ้าของโปรเจกต์ระหว่างรีวิว) `EntityType < 10000` มีสาขาของตัวเองแล้ว —
ดึง blueprint จาก `RecipeData.BlueprintByType` → `EntityName` จาก `BlueprintName` →
ประกอบ interaction จาก `BlueprintComponents` (Workbench 501 · Shelter 407 · Sanctum 503 · Bandstand 552 · Inventory 404)

---

## 2026-08-13 — เฟส B: GP-07 เซฟลงดิสก์

state ทั้งหมดเคยอยู่ใน RAM ล้วน ปิดเซิร์ฟทีของ/สกิล/บ้าน/ต้นไม้หายเกลี้ยง

**ไฟล์ใหม่:** `SaveStore.cs` (I/O) · `SaveModels.cs` (รูปแบบข้อมูล) · `ServerPlayer.Persistence.cs` · `ArtifactFactory.cs`

- เซฟลง `server/saves/` — `world.json` (สิ่งปลูกสร้าง + ต้นไม้ที่ถูกเก็บ) และ `players/<entityId>.json` (ของ/สกิล/แต้ม/ตำแหน่ง)
- เขียนแบบ **tmp-then-swap** (`File.Move(overwrite)`) กันไฟล์พังถ้าเซิร์ฟดับกลางคัน
- เก็บเป็น **record ของเราเอง** ไม่ serialize struct ของ `Messages/` ตรง ๆ เพราะ `Item.Ext` เป็น `object`
  และ struct พวกนั้นต้องตรงกับ client เป๊ะ ๆ ห้ามผูกไฟล์เซฟไว้ด้วย
- เซฟ 3 จังหวะ: ผู้เล่นออกเกม (ก่อน `RemovePlayer`) · autosave ทุก 60 วิ (เฉพาะที่ dirty) · Ctrl+C (force)
- `--saves <path>` เปลี่ยนที่เก็บได้
- ย้าย `MakeArtifact` → `ArtifactFactory.Make()` (static) เพราะตอนโหลดเซฟไม่มี `ServerPlayer` ให้อ้างอิง
- ย้ายการแจกกองไฟจาก `ApplyPlayerData()` → `GrantStarterItems()` + flag `StarterGiven`
  ไม่งั้นพอมีเซฟแล้วผู้เล่นจะสะสมกองไฟเพิ่มทุกครั้งที่ login

**ทดสอบแล้วด้วย `test-client` 3 รอบ:**

| รอบ | ผล |
|---|---|
| 1 | สร้างไฟล์เซฟ — ตำแหน่ง (8040, 35400), แต้ม 776, กองไฟ 1 |
| 2 | `โหลด test-client-1: ของ 1 ชิ้น, สกิล 1 ตัว, แต้ม 776, ตำแหน่ง จำได้` — กองไฟยัง **1 อัน** ไม่แจกซ้ำ, แต้มลดเป็น 775 = สะสมต่อจริง |
| 3 | ป้อน `world.json` มือ → `โหลดโลกแล้ว: สิ่งปลูกสร้าง 1 ชิ้น` + คนเข้ามาได้ `artifacts=1` |

**ยังไม่ได้ทดสอบ:** เส้นทาง Ctrl+C และการสร้าง/ทุบสิ่งปลูกสร้างจริงผ่านเกม (test-client ยังไม่ส่ง packet ก่อสร้าง)

รายละเอียดเต็มที่ [server/Persistence.md](server/Persistence.md)

---

## 2026-08-13 — เฟส C (1/4): ระบบสวมใส่อุปกรณ์

**ไฟล์ใหม่:** `ServerCore/ServerPlayer.Equipment.cs` · `ServerCore/EquipData.cs` · `scripts/extract_equip.py`

เดิม `SendEquipments()` ตอบ `Presets = null` เป็น stub — คราฟต์ขวานมาก็ใส่ไม่ได้
และที่แย่กว่าคือ **stub นั้นทำให้ client โยน NullReferenceException**
(`EquipSystem.EquipmentsReceived` deref `msg.Presets` และ `equipmentSlot.ItemSlots` ตรง ๆ ไม่เช็ค null)

- `HandleEquip` / `HandleGetEquipments` — ตรวจว่ามีของจริงในกระเป๋าก่อนใส่, ตอบ `Abort` เมื่อปฏิเสธ
  (client ส่งด้วย `.All(...)` = รอ reply ถ้าไม่ตอบอะไร UI ค้าง)
- `RebuildEquipments()` — ตรรกะเดียวกับ offline server เดิมของเกม: รีเซ็ตส่วนที่อุปกรณ์คุมแล้วทาทับตามของที่ใส่
  พร้อม **เก็บกวาดช่องที่ของหายไปแล้ว** ออกจาก `_equippedItems`
- broadcast `PlayerDisplay` ทุกครั้งที่เปลี่ยน → คนอื่นเห็นหน้าตาใหม่
- เซฟ `EquippedItems` ลงไฟล์ผู้เล่นด้วย
- cheat ใหม่ `add axe` / `add clothes` สำหรับทดสอบ (บอกด้วยว่า prototype นั้นรู้จักโมเดลไหม)
- `LoadPlayerSave()` อ่าน `EntityType` เพิ่ม — ไม่งั้นอาจได้ display หญิงแต่ EntityType ชาย แล้วเลือกโมเดลเกราะผิดเพศ

**`EquipData.cs` — สกัดข้อมูลจริงจากตัวเกม** ด้วย `scripts/extract_equip.py`
อ่านบล็อก `"weapon"` / `"armor"` ของ performances จาก `game/DurangoV2_Data/resources.strings.txt`
ได้ **อาวุธ 248 · เกราะ 376** พร้อม path โมเดลจริง (สร้างอัตโนมัติ อย่าแก้ด้วยมือ)

**ทดสอบครบ 5 เส้นทางด้วย `test-client` (ข้อ 13–18):**

| ทำอะไร | ผล |
|---|---|
| ใส่ขวาน | `Equip=Models/Equipment/Melee/tier1x_axe_onehand_stone.fbx` · `Framework=onehand` |
| ใส่เสื้อ | `Body=Models/PC/Male/Body/m_body_builder.fbx` (ขวานยังอยู่) |
| ถอดขวาน | `Equip=(ไม่มี)` เสื้อยังอยู่ |
| ใส่ของที่ไม่มีจริง | `Abort` + log ปฏิเสธ |
| รีสตาร์ท | `EquippedItems` โหลดกลับครบ |

รายละเอียดที่ [server/Equipment.md](server/Equipment.md)

**เฟส C ที่เหลือ:** ค่าสถานะเอาชีวิตรอด · สัตว์/ต่อสู้/ตาย-ฟื้น · กล่องเก็บของ

---

## 2026-08-13 — เฟส C (2/4): ค่าสถานะเอาชีวิตรอด

**ไฟล์ใหม่:** `ServerCore/ServerPlayer.Survival.cs`

เดิม `AppearPlayer` ส่ง `Life = Gauge(1,0,[(0,1)])` ค่าเดียวตายตัว ตอนนี้มี **เลือด · สตามินา · ความล้า** จริง

**จุดที่ทำให้ระบบนี้เบา:** `Gauge` ของเกมเป็น *keyframe ที่ client interpolate เอง* ไม่ใช่ตัวเลข
server ส่ง `[(ตอนนี้, 94), (ตอนนี้+1.5, 100)]` แล้ว client ลากเส้นให้ — **ไม่ต้อง tick ทุกเฟรม**
ส่งใหม่เฉพาะตอนอัตราเปลี่ยนหรือค่ากระโดด (เก็บของ/โดนตี/พัก)

- `GaugeState` เก็บ `(Value, Velocity, Max, UpdatedAt)` → `ToGauge()` สร้าง 2 จุดที่จบพอดีตอนชนขอบ
- สตามินา: เก็บของ 6 · คราฟต์ 4 · ก่อสร้าง 8 — ไม่พอ = `Abort`
- ความล้าเต็มใน 1 ชม. เกิน 60 ทำให้สตามินาแพง ×1.5 เกิน 85 ×2
- `PushGauges()` ส่ง `SurvivalUpdated` เฉพาะที่เปลี่ยน — `life` broadcast ให้คนอื่นด้วย (เห็นหลอดเลือด) สตามินา/ความล้าเป็นเรื่องส่วนตัว
- ⚠️ `SurvivalUpdated.Removed` ต้องเป็น array ว่าง ห้าม null (client วน `.Length` ตรง ๆ)
- `SendStatistics()` เพิ่ม `LifeMax` `StaminaMax` `FatigueMax` `FatigueCaution` `FatigueDanger`
- เซฟด้วย — โหลดกลับ: เลือดคงค่าเดิม สตามินาเต็ม (ถือว่าได้พัก) ความล้าคงค่าเดิม
- cheat ใหม่: `survival` `rest` `tired` `hurt` `exhaust`

**ทดสอบ (test-client ข้อ 19–23):** ค่าเริ่มต้นถูก · เก็บของหัก 6 พร้อมเส้นฟื้น · สตามินา 0 แล้วเก็บของโดน `Abort` ·
`hurt` ลดเลือดแล้วฟื้น 0.5/วิ · `rest` คืนทุกอย่าง · เซฟอ่านกลับได้

> เกร็ด: รอบแรกเคส "สตามินาไม่พอ" **ไม่ได้ทดสอบจริง** — ตั้งไว้ 3 แล้วรอ 700ms
> แต่สตามินาฟื้น 4/วิ กลับไปเกิน 6 ก่อนคำสั่งถึง ต้องตั้ง 0 แล้วยิงทันทีถึงเจอ

รายละเอียดที่ [server/Survival.md](server/Survival.md)

**เฟส C ที่เหลือ:** สัตว์/ต่อสู้/ตาย-ฟื้น · กล่องเก็บของ

---

## 2026-08-13 — เฟส C (3/4): กล่องเก็บของ

**ไฟล์ใหม่:** `ServerCore/ServerPlayer.Storage.cs` (+ ส่วนที่แทรกใน `ServerWorld.cs`)

- `IsStorage()` ตัดสินจาก **blueprint ที่มี component `"Inventory"`** — ใช้ `_artifactBlueprints` ที่เก็บไว้ตอน GP-07
- `GetInventory` ที่มี `Target` = เปิดกล่อง, ไม่มี = กระเป๋าตัวเอง
- `PutInItem` / `TakeOutItem` — กระเป๋า 50 ช่อง กล่อง 200 ช่อง
- **กล่องเต็มแล้วคืนของกลับกระเป๋าทั้งหมด** ไม่ให้ของหายกลางทาง (เอาออกจากกระเป๋าไปแล้วก่อนเช็ค)
- หยิบของ **ได้เท่าที่กระเป๋ารับไหว** ไม่ล้น
- `InventoryUpdated` ของกล่อง broadcast ให้ทุกคน — คนอื่นที่เปิดกล่องเดียวกันเห็นตรงกัน
- `TakeOutItem` ต้องตอบ `OK` (client เช็ค `Packet.IsSuccess` ซึ่งมองว่า `Abort` = ล้มเหลว)
- เซฟของในกล่องลง `world.json` · ทุบกล่องแล้วของข้างในหายไปด้วย

**ทดสอบ (test-client ข้อ 24–28):** วางกล่อง → เปิด → ใส่ใบไม้ → หยิบกลับ → ใส่ของลงสิ่งที่ไม่ใช่กล่องโดน `Abort`
และป้อน `world.json` ที่มีของ 2 ชิ้นในกล่องแล้วรีสตาร์ท → `กล่องที่มีของ 1 ใบ`

รายละเอียดที่ [server/Storage.md](server/Storage.md)

**เฟส C ที่เหลือ:** สัตว์ + ระบบต่อสู้ + ตาย/ฟื้น

---

## 2026-08-13 - เฟส C (4/4 รอบที่ 1): สัตว์โผล่ในโลก + เดินสุ่ม

**ไฟล์ใหม่:** `ServerCore/ServerAnimal.cs` · `AnimalSpawner.cs` · `AnimalData.cs` · `scripts/extract_animals.py`

รอบนี้ทำแค่ให้สัตว์มีอยู่ในโลกและเดินได้ **ระบบต่อสู้เป็นรอบถัดไป**

- สกัดข้อมูลสัตว์ **213 ชนิด** (entity type 2000-2999) จาก `resources.strings.txt` - ชื่อ/โมเดล/ขนาด/AI/tamable
- เกิด 12 ตัวรอบจุดเกิด กระจายในรัศมี 6000 หน่วย (seed คงที่ ทดสอบซ้ำได้)
- เดินสุ่มในรัศมี 2500 จากบ้านตัวเอง · **ไม่มีคนเล่น = ไม่ขยับเลย**
- ส่งสัตว์ทั้งหมดให้ผู้เล่นที่เพิ่งเข้ามา (เหมือนที่ทำกับสิ่งปลูกสร้าง)
- **สัตว์ไม่ถูกเซฟ** ตั้งใจ - เป็นของชั่วคราวในโลก ไม่ใช่ความคืบหน้าผู้เล่น

**บั๊กที่เจอตอนทดสอบและแก้แล้ว:** ตั้งเวลาสั่งเดินใหม่ทุก 6-16 วิ แต่เวลาเดินจริงยาวได้ถึง 18.9 วิ
สั่งเดินทับก่อนถึงที่หมาย และเพราะ `MakeMove` อัปเดตตำแหน่งเป็นปลายทางทันที สัตว์จะกระโดดไปข้างหน้า
แก้เป็น รอเดินถึง + พัก ทำให้คำสั่งเดินในช่วงทดสอบเดียวกันลดจาก **88 เหลือ 38 ครั้ง**

**ข้อจำกัดที่ต้องเทสกับเกมจริง:** `Movement.MotionName` ของสัตว์เป็น `[SerializeField]` ต่อ prefab
ที่ server รู้ไม่ได้ จึงส่ง `null` (เหมือนโค้ดในเกมเอง) ตำแหน่งซิงก์ถูกแต่ **อนิเมชันเดินอาจไม่เล่น**

รายละเอียดที่ [server/Animals.md](server/Animals.md)

**เหลือ:** ระบบต่อสู้ + ตาย/ฟื้น (`UseBattleAction` `Damaged` `Revive`) + AI ไล่/หนี + respawn

---

## 2026-08-13 - เทสกับเกมจริง (ระดับ 1 + 2)

เปิด server + เปิดเกมจริง ขับ UI ด้วย synthetic input (P/Invoke `SetCursorPos`/`mouse_event`)
จนเข้าโลกได้ แล้ววิเคราะห์ log ทั้งสองฝั่ง

### ✅ ผลที่ยืนยันได้
- เกมบูต → เลือกเซิร์ฟเวอร์ → **เข้าโลกได้จริง** (ตัวละคร Lv.60 ยืนในป่าหิมะ HUD/มินิแมพครบ)
- `game.log` ตอนอยู่ในเกม **มีแต่ error ที่รู้อยู่แล้วจาก ENV-01** (asset corrupted / CombatModeButton /
  Wwise bank) **ไม่มี NullReferenceException สักตัว** — ต่างจาก `game1.log` เดิมที่มี 3,220 ตัว

### 🐛 บั๊กที่เจอและแก้แล้ว: `/entry` รายงานพอร์ตผิด
`Gateway` ใช้ค่าคงที่ `GameServer.DefaultPort` แทนพอร์ตที่เปิดฟังจริง
พอรันด้วย `--game-port` อื่น client จะได้ที่อยู่ผิด → ต่อไม่ติด
แก้: เพิ่ม `GameServer.Port` / `RadiotowerServer.Port` ที่จำค่าจาก `Start()`
> `test-client` จับไม่ได้เพราะมันต่อพอร์ตเกมตรง ๆ ไม่ผ่าน `/entry`

### 🔍 เข้าใจโครงสร้างการเชื่อมต่อผิดมาตลอด (สำคัญ)
**เลือก "Multi Play Mode" จากหน้า title = client สตาร์ท server ของตัวเองที่ 8390/8391
ไม่ได้มาต่อ server ภายนอกเลย**

ตอนแรกผมเดาว่า "ถ้า server ภายนอกยึดพอร์ต 8390 ไว้ client จะมาต่อของเรา" — **ผิด**
ทดลองแล้ว client พ่น:
```
SocketException: An attempt was made to access a socket in a way forbidden by its access permissions.
  at System.Net.EndPointListener..ctor            ← bind 8390 ไม่ได้เพราะเรายึดอยู่
InvalidOperationException: You must call the Bind method before performing this operation.
  at Durango.Offline.Listener.Accept ()
  at GameManager.Update ()                        ← พ่นทุกเฟรม
```
อันหลังคือบั๊ก **NET-05** ที่รีวิวไว้ตั้งแต่แรก ปรากฏตัวจริง (และเป็นบั๊กเดียวกับ GP-15 ที่แก้ในเซิร์ฟเราไปแล้ว)

**ทางเดียวที่จะเข้า server ภายนอกคือเมนูในเกม "ใส่ ip ของเกาะ"** ซึ่ง `Server.ConnectTo()`
**hardcode พอร์ต 8190** (บั๊ก BUG-11 ที่รายงานไว้และยังไม่ได้แก้)
⇒ server ต้องอยู่ที่ **8190/8191 (ค่า default เดิม)** ไม่ใช่ 8390

### ยังพิสูจน์ไม่ได้
**ยังไม่มี packet จากเกมจริงเข้า server เราสักตัว** (`client connected` = 0)
ต้องเข้าเมนู "ใส่ ip เกาะ" ในเกมก่อน — ยังหาเมนูไม่เจอ (ปุ่ม ☰ มุมซ้ายล่างเป็นแผงอิโมติคอน)
⇒ equip / survival / storage / animals **ยังไม่ได้ทดสอบกับ client จริง**

### หมายเหตุ
Windows Defender Firewall เด้งขออนุญาต `durangov2` ตอนเข้าเกม — **กด Cancel ไว้**
(เทสในเครื่องเดียวใช้ loopback ไม่ผ่าน firewall) ถ้าจะเล่นข้ามเครื่องต้องกด Allow

---

## 2026-08-13 - ✅ เกมจริงต่อเข้า server สำเร็จ (เจ้าของโปรเจกต์เชื่อมให้)

ผมขับ UI เองไม่เจอเมนู "เยี่ยมชมเกาะเพื่อน" เจ้าของโปรเจกต์เลยกดเชื่อมให้ แล้วผมวิเคราะห์ log ต่อ

### handshake ครบทุกขั้น
```
GET  /knock?version=5.2.1&platform=WindowsPlayer&bundle_id=com.nexon.durango.wildlands -> 200
POST /sessions   -> session player: 58f75e09-... display=yes
GET  /admission  -> 200
GET  /entry      -> 200
[gameserver] client connected from 127.0.0.1:61539
GET  /terrains/1 , /terrains/1/whole_biomes , /notice -> 200
[world] player joined: 58f75e09-..., total=1, artifacts=0, สัตว์=12
[emotions] request, motions=73 emoticons=24
GET  /terrains/1/<x>,<y>  (chunk ทยอยเข้ามา)
```

### ผลสำคัญ
- **`game.log` ไม่มี exception เลยสักตัว** — โลกโหลดครบ 104,914 objects
  มีแต่ asset warning ที่รู้อยู่แล้วจาก ENV-01
- **ส่งสัตว์ 12 ตัวให้ client จริงแล้ว** (`สัตว์=12` ตอน player joined)
- ยืนยันว่า `Equipments` / `Survival` ที่ส่งตอน `SendSpawnBurst` **ไม่ทำให้ client พัง**
  (เดิม `Presets = null` เคยทำให้ NRE)

### 🐛 บั๊กที่เจอจากการเทสนี้ (แก้แล้ว): `Messages.Say` ไม่มี TypeCode
```
[connection] warning: Messages.Say has no TypeCode, handler registered under key 0
```
`Messages/Say.cs` **ไม่มี `const uint TypeCode`** (เป็น struct ที่ฝังในข้อความอื่น ไม่ใช่ packet เดี่ยว)
`RegisterMessageHandlerToRegistry` เลยลงทะเบียน handler ใต้ **key 0**

ฝั่ง client `TypeCode == 0` แปลว่า **"packet ตอบกลับ (reply)"** ⇒ reply ใด ๆ ที่วิ่งเข้ามาจะถูก
พยายาม deserialize เป็น `Say`

แก้: ลบ `_conn.Recv<Say>(HandleSay)` + เมทอด `HandleSay` ทิ้ง

### ยังไม่ได้ทดสอบ
ยังไม่ได้ลองใช้งานจริงในเกม (ใส่ขวาน / เก็บของ / เปิดกล่อง / ดูสัตว์เดิน)
— ทดสอบแค่ว่า **เข้าเกมได้และ spawn burst ไม่พัง**

---

## 2026-08-14 — เลิกเชื่อ client: GP-08 · GP-09 · GP-12 · GP-14

หลักการเดียวกันทั้ง 4 ข้อ: **ข้อมูลที่ client ส่งมาคือ "คำขอ" ไม่ใช่ "ข้อเท็จจริง"**
ทุกข้อมีตัวทดสอบจริงอยู่ใน `dotnet run -- --gp-check` (ผ่าน 16/16)

### GP-12 · `Auth` ต้องมี session token ที่ server ออกให้

เดิม `/sessions` คืน `session_token = entity id` และ `Auth` ก็รับ `EntityId` ที่ client อ้างมาดื้อ ๆ
⇒ **ใครก็ตามที่เห็น entity id ของคนอื่น (มากับ `AppearPlayer` ทุก packet) สวมรอยได้ทันที**

- `GameServer.IssueSession()` ออก token สุ่ม 64 ตัวอักษร ผูกกับ entity id + `PlayerData` ของ session นั้น (อายุ 12 ชม.)
- `TryAuthorize()` ตรวจว่า token มีจริง ยังไม่หมดอายุ และ **entity id ที่อ้างต้องเป็นของ token นั้น**
  ไม่ผ่าน = ตอบ `Abort` แล้วปิด connection พร้อม log บอกว่าใครอ้างเป็นใคร
- token ไม่ถูกลบหลังใช้ เพราะ client ใช้ token เดิมตอน reconnect (`SendAuthMessage(isReconnect: true)`)
- `Ready` ใช้ `PlayerData` ที่ผูกมากับ token ไม่ใช่ค้นจาก entity id ที่ client อ้าง
- `--insecure-auth` = ปิดการตรวจ (มีไว้ debug เท่านั้น มี log เตือนตัวใหญ่ตอนเปิดเซิร์ฟ)

**ผลข้างเคียงที่ต้องรู้:** ตัวทดสอบต้องขอ token ทาง HTTP ก่อน — เพิ่ม `test-client/SessionClient.cs`
และ FarmBot รับพอร์ต gateway เป็นอาร์กิวเมนต์ที่ 5 (ไม่ใส่ = พอร์ตเกม − 1)

ไฟล์: `ServerCore/GameServer.cs` · `ServerCore/Gateway.cs` · `Program.cs` · `test-client/SessionClient.cs`

### GP-09 · `Touch`/`Collect` อิงพื้นที่จริงของ server

เดิม `HandleTouch` สร้าง generator ให้ **ทุก tile ที่ client ขอมา** (ขุดอากาศได้ไม่จำกัด และเลือกชนิดของที่จะได้เอง)
และ `HandleCollect`/`DisappearEntityOnTile` ก็ลบต้นไม้ตาม `Tile` ที่ client แนบมา (ยิงรัว ๆ = ถางป่าทั้งแมพจากมุมไหนก็ได้)

- `TerrainStore.TryGetNatural(x, y, out type)` — อ่าน garden (record 6 ไบต์: x, y, entityType) ว่าจุดนั้นมีอะไรอยู่จริง
- `HandleTouch` ปฏิเสธถ้า **ไม่มีของธรรมชาติที่ tile นั้น** หรือ **อยู่ไกลเกิน 8 tile** จากตำแหน่งล่าสุดของผู้เล่น
  และใช้ **ชนิดจาก garden** ไม่ใช่จาก `msg.EntityType`
- ผ่านแล้วจึงผูก entity id ↔ tile ไว้ที่ `ServerWorld._naturalTiles`
- `HandleCollect` อ่าน tile จากที่ผูกไว้ **ไม่แตะ `msg.Tile` อีกเลย** + เช็คระยะซ้ำ (เดินหนีระหว่างเก็บไม่ได้)
- `DisappearEntityOnTile` ที่ client ส่งมา ต้องเป็นจุดที่เคยแตะและอยู่ในระยะ ไม่งั้นเมิน

ไฟล์: `ServerCore/TerrainStore.cs` · `ServerCore/ServerWorld.cs` · `ServerCore/ServerPlayer.Gathering.cs` · `ServerCore/ServerPlayer.Core.cs`

### GP-08 · `Craft` ตรวจวัตถุดิบจริง

เดิมหักของด้วย `FindIndex` แล้ว **ข้ามเงียบ ๆ ถ้าหาไม่เจอ** ⇒ ส่ง `Craft` เปล่า ๆ ก็ได้ของ

- `scripts/extract_recipes.py` สกัด TextAsset `recipes` จาก `resources.strings.txt`
  → `ServerCore/RecipeRequirements.cs` (**720 สูตร / 1,756 ช่องวัตถุดิบ** พร้อม count_min/count_max/tags)
- `HandleCraft` ปฏิเสธเมื่อ: ไม่มีสูตรนั้นในเกม · ส่งชื่อช่องที่ไม่มีในสูตร · จำนวนต่อช่องไม่อยู่ในช่วง
  · ใส่ไอเทมชิ้นเดียวซ้ำหลายช่อง · ใส่ item id ที่ไม่มีในกระเป๋า · ใส่ของที่สวมอยู่บนตัว
- ตอนคราฟต์เสร็จ (หลัง 2.1 วิ) **หักแบบครบทุกชิ้นหรือไม่ทำเลย** — ถ้าระหว่างนั้นเอาของไปใส่กล่องแล้วก็ `Abort`
- กระเป๋าเต็มแต่มีวัตถุดิบจะถูกหัก = คราฟต์ได้ (หัก n เพิ่ม 1)

**ยังไม่ได้ทำ:** ตรวจ tag ของวัตถุดิบ (เช่น "ช่อง main ต้องเป็นใบมีดหิน") เพราะไอเทมที่ server รุ่นนี้สร้าง
ยังไม่มี `Tags` ติดตัว — ถ้าเปิดตรวจตอนนี้จะคราฟต์ไม่ได้เลยสักสูตร ข้อมูล tag ถูกสกัดเก็บไว้ในไฟล์แล้ว รอไอเทมมี tag ก่อน

ไฟล์: `ServerCore/ServerPlayer.Crafting.cs` · `ServerCore/RecipeRequirements.cs` · `scripts/extract_recipes.py`

### GP-14 · เลเวล/หน้าตาเป็นของ server

`/sessions` รับ JSON จากเกาะของ client ตรง ๆ ค่าทุกอย่างในนั้นจึงปลอมได้

- เลเวลจาก client ใช้ได้ **เฉพาะตอน login ครั้งแรก** (ตอนที่ยังไม่มีไฟล์เซฟ) และถูกตัดที่เพดาน 60
  login รอบต่อ ๆ ไปใช้เลเวลใน `PlayerSave` เสมอ พร้อม log ว่า client อ้างเท่าไหร่
- `EntityType` ต้องอยู่ช่วง 1000-1999 (2000+ เป็นสัตว์, 10000+ เป็นของธรรมชาติ) ไม่งั้นใช้ค่าเดิม
- หน้าตา (`Display`) ยังให้ client เปลี่ยนได้ตามเกาะตัวเอง (เป็นเรื่องความสวยงาม)
  แต่เก็บลง `PlayerSave.DisplayJson` ด้วย เพื่อให้ login ที่ไม่มีข้อมูลจากเกาะยังหน้าเหมือนเดิม
- `--trust-client-profile` = กลับไปเชื่อเลเวลจาก client ทุกครั้ง (สำหรับคนที่เล่นคนเดียวแล้วเลเวลอัปที่เกาะตัวเอง)

🐛 **บั๊กที่เจอระหว่างเขียนเทส:** ผู้เล่นที่อ้าง Lv.5 กลับโผล่มาเป็น Lv.60
`LoadPlayerSave()` (fallback อ่าน `0.player` ของเจ้าของเครื่อง) ทับเลเวลที่ client ส่งมาทับทุกครั้ง —
ตอนนี้ fallback จะไม่ทับค่าที่มาจาก session

ไฟล์: `ServerCore/ServerPlayer.Core.cs` · `ServerCore/ServerPlayer.Persistence.cs` · `ServerCore/SaveModels.cs` · `ServerCore/GameServer.cs`

### ตัวทดสอบใหม่: `--gp-check`

```bash
cd test-client && dotnet run -- --gp-check [host] [port เกม] [port gateway]
```
ยิง packet แบบ client โกงแล้วเช็คว่า server ปฏิเสธจริง (16 ข้อ) — exit code 1 ถ้ามีข้อไหนตก
ปิดท้ายด้วยการคราฟต์ด้วยของจริงเพื่อยืนยันว่า **ของที่ควรทำได้ยังทำได้อยู่**

### FarmBot ฉลาดขึ้น (ผลพวงจาก GP-09)

เดิมสุ่ม tile แล้วเดาชนิดของ ⇒ หลัง GP-09 จะโดนปฏิเสธหมด
ตอนนี้อ่าน garden จาก packet `Chunk` เพื่อรู้ตำแหน่ง+ชนิดของจริง แล้วเดินไปเก็บทีละจุดที่ใกล้ที่สุด
(0 abort, 10 ชิ้นใน 24 วินาที) และขอ chunk ใหม่ทุกครั้งที่ข้ามเขต

ไฟล์: `test-client/FarmBot.cs` · `test-client/GpCheck.cs` · `test-client/Program.cs`

---

## 2026-08-15 — เตรียมเปิดออนไลน์: กันเซิร์ฟล่ม + ปิด cheat + กันสวมรอย

ให้ agent ไล่ตรวจโค้ดทั้ง `ServerCore/` หา "จุดที่จะระเบิดเมื่อมีคนแปลกหน้าเข้ามา" ได้มา 8 ข้อรุนแรงสูง
รอบนี้แก้ 3 ข้อที่ต้องมาก่อน (H-4 → H-2 → H-1)

### H-4 · exception เดียวเคยฆ่าเซิร์ฟทั้งใบ ✅

`Program.cs` เดิมเป็น `try { while(true) {...} } finally {...}` — **ไม่มี catch เลย**
และ `WebServer.Process()` เขียน response โดยไม่จับ exception ⇒ `curl http://เซิร์ฟ:8190/terrains/1/whole_biomes`
แล้วกด Ctrl+C กลางคัน (หรือโดน port scanner) = `HttpListenerException` → **เซิร์ฟดับ ผู้เล่นหลุดหมด
งานตั้งแต่ autosave ล่าสุดหาย (สูงสุด 60 วิ)**

- `SafeProcess()` ครอบ `gameServer/gateway/radiotower` แยกกันทีละตัว + กัน log ท่วม (ซ้ำใน 5 วิ พิมพ์ครั้งเดียว)
- `WebServer.Process()` จับ exception ตอนเขียน/ปิด response แล้ว `Abort()` เฉพาะรายนั้น
- `ServerWorld.ProcessPlayers()` วนบน **snapshot** แทนการถือ `_lock` วน list —
  เดิมถ้า `Send` ล้มระหว่างวน connection จะ `Close()` → `RemovePlayer` → แก้ list ที่กำลัง foreach อยู่
  (lock เป็น reentrant จึงเข้าได้) → `InvalidOperationException` ฆ่า main loop

**ยืนยันแล้ว:** ยิง curl ตัดกลางคัน 3 นัด — ขึ้น `[web] ส่ง response ไม่สำเร็จ (client หลุดไปแล้ว?)` แล้วเซิร์ฟเล่นต่อปกติ

### H-2 · packet `Cheat` เปิดให้ทุกคน ✅

ใครต่อเข้ามาก็เสกของ/เรียกสัตว์ไม่จำกัด/ฟื้นเลือดได้ และที่หนักสุดคือ `control` ที่
**ลากตัวละครของคนอื่นไปไหนก็ได้ · พูดแทนเขา · บังคับให้ตีสัตว์จนสัตว์ไล่กัดเขาตาย**

- ปิดทั้งหมดเป็นค่าเริ่มต้น → ตอบ `Info` บอกว่าปิดอยู่ (ไม่เงียบหาย)
- `--enable-cheat` เปิดเฉพาะคำสั่งที่ทำกับตัวเอง
- `control` ต้องเป็น admin (`--admin <ชื่อ|entityId>` ใส่ซ้ำได้) — ไม่ตั้ง = ไม่มีใครใช้ได้

**เทสแล้ว 3 เคส:** ปิดอยู่ = ปฏิเสธหมด · เปิด cheat แต่ไม่ใช่ admin = `add axe` ได้ แต่ `control` ไม่ได้ · admin = ได้ทั้งคู่

### H-1 · ใครก็อ้างเป็นตัวละครของคนอื่นได้ ✅ (กันได้เท่าที่ client เอื้อ)

`/sessions` ให้ client บอก entity id เองล้วน ๆ แต่ id เป็นของสาธารณะ (มากับ `AppearPlayer` ทุก packet)
⇒ รู้ id ของใคร = เข้าเกมเป็นเขาได้ทั้งกระเป๋า/สกิล/ตำแหน่ง แถม logout แล้วเซฟทับของเจ้าตัว

ตัวเกมไม่ได้ส่งรหัสผ่านอะไรมาเลย (มีแค่ชื่อ/เลเวล/id) และแก้ client ไม่ได้ จึงกัน 2 ชั้นฝั่ง server
(`ServerCore/AccountStore.cs` — ใหม่):

| ชั้น | ทำอะไร |
|---|---|
| รายชื่อที่อนุญาต | `--whitelist data/whitelist.txt` — คนนอกรายชื่อขอ token ไม่ได้ (ตอบ **403**) |
| จองตอนเข้าครั้งแรก | entity id ผูกกับ IP แรกที่จอง เก็บที่ `saves/accounts/<id>.json` · IP อื่นมาอ้าง = ปฏิเสธ (`--no-ip-bind` ปิดได้) |

**เทสแล้ว:** `mallory` (นอกรายชื่อ) โดนปฏิเสธที่ `/sessions` แล้วตกที่ `Auth` ซ้ำอีกชั้น ·
`alice` (ในรายชื่อ) เข้าได้ · **ตัวเกมจริงเข้าได้ปกติ** (`[account] จอง 8ae11e65… (ฟหกฟหก) ให้ 127.0.0.1`)

ดู [docs/server/Accounts.md](server/Accounts.md)

### 🐛 ตัวเกมเด้ง 4 ครั้ง — ไม่ใช่บั๊กของโค้ด

ระหว่างเทสตัวเกมเด้งซ้ำ ๆ (crash dump ล้วน ไม่มี log) สุดท้ายพบว่า **เปิดเกมซ้อนกันหลายตัว**
client มี server ในตัวที่ bind พอร์ต 8390/8391 (จาก dll patch) ตัวที่สอง bind ไม่ได้ = ตายทันที
md5 ยืนยันว่า dll ที่ใช้อยู่เหมือนตัวที่รันได้ก่อนหน้าทุกไบต์

- `tools/connect-game.ps1` เจอเกมเปิดอยู่เกิน 1 ตัว = ไม่ยอมเปิดเพิ่ม + เขียน log ลง `game/client.log`
- เปลี่ยนมาต่อเซิร์ฟด้วย `DURANGO_AUTOCONNECT` (patch ที่มีอยู่แล้วใน dll) แทนการคลิกเมนู 6 ครั้ง
  → เหลือคลิกเดียว (ปุ่ม "เริ่ม") ไม่ต้องพึ่งพิกัดคลิกที่เพี้ยนง่าย

### ที่ agent เจอแต่ยังไม่ได้แก้ (เรียงตามความสำคัญ)

| # | เรื่อง |
|---|---|
| H-3 | 1 connection จอง ~17 MB **ตั้งแต่ก่อน Auth** และไม่มี timeout ⇒ เปิด TCP ค้าง 200 เส้น = OOM |
| H-5 | `Touch` ใช้ `EntityId` ที่ client ตั้งเองเป็นคีย์ของ generator ⇒ เปลี่ยน id ไปเรื่อย ๆ = เก็บของจากต้นเดียวไม่มีวันหมด |
| H-6 | `BuildArtifact` ไม่ตรวจอะไรเลย + ยัด `_deferred` ได้ไม่จำกัด ⇒ broadcast storm |
| H-7 | `OccupyArtifactSite` สร้างได้ไม่จำกัด ทุกชิ้นเซฟถาวร + ส่งทั้งหมดให้คนเข้าใหม่ ⇒ คนใหม่เข้าเกมไม่ได้ |
| H-8 | token ใช้ซ้ำหลายเส้นพร้อมกันได้ ⇒ เปิด 2 client id เดียวกัน = ก๊อปของ |
| M-1 | `Move`/`PlayEmoticon` broadcast ดิบ ไม่เขียนทับ `EntityId` ⇒ ปลอมเป็นคนอื่นได้ |
| M-2 | ไม่ตรวจความเร็วการเดิน ⇒ วาร์ปข้ามแมพได้ ทำให้การตรวจ "ระยะเอื้อม/ระยะตี" ไร้ผล |
| M-4 | กล่องเก็บของไม่ตรวจเจ้าของ/ระยะ ⇒ ขนของในบ้านคนอื่นได้ |
| M-6 | ไม่มี rate limit เลย · `SetChunk` สแกน garden ทั้งก้อน 9 รอบต่อครั้ง |

---

## 2026-08-15 (รอบ 2) — Beta 1.0 รอบ A+B

### A · เกาะเริ่มต้นเลเวล 1-10

- `ServerCore/SpawnTable.cs` (ใหม่) — ตารางสัตว์ **10 ชนิด รวม 34 ตัว** แทนการสุ่ม 12 ตัวจาก 4 ชนิด
  แต่ละชนิดล็อกช่วงเลเวล/โควตา/นิสัย (`Flee` `FightBack` `Aggressive`) และ **ระยะห่างขั้นต่ำจากจุดเกิด**
  — โอวิแรปเตอร์ต้องเกิดนอก 12 tile · แร็ปเตอร์นอก 20 tile (คนเพิ่งเข้าเกมจะได้ไม่โดนรุมทันที)
- สัตว์นิสัย `Aggressive` **ไล่กัดเองเมื่อเห็นคนในระยะ 6 tile** (เดิมต้องโดนตีก่อนถึงจะสนใจ)
- นิสัยมาจากตาราง ไม่ใช่เดาจากชื่อไฟล์ AI อีกต่อไป
- เกิดใหม่เป็น **ชนิดเดิมที่ตายไป** (โควตาไม่เพี้ยน) · 60 วิ · ซากอยู่ 30 วิ
- **สมดุลใหม่:** เลือดสัตว์ `30+lv×8` (เดิม `50+lv×10`) · ดาเมจ `2+lv×0.4` (เดิม `3+lv×0.6`)
  เดิมมือเปล่าสู้ตัว lv10 = ตี 25 ครั้งแต่โดนกลับ 216 หน่วย ทั้งที่เลือดมี 100 ⇒ ตายแน่นอน
- **แจกขวานหินตอนเข้าเกมครั้งแรก** (เดิมแจกแค่กองไฟ)
- ซ่อนเมนูของระบบที่ยังไม่ได้ทำ 24 เมนู ด้วย patch `MenuSystem.IsHiddenMenu`
  (ฝั่ง server สั่งซ่อนได้แค่ Party เมนูเดียว เพราะ client มี binding ให้แค่ `party.ui_enabled`)

### B · H-3 กัน DoS + H-8 กันก๊อปของ

**H-3** — `Connection` จองบัฟเฟอร์ **ตั้งแต่ตอน accept ก่อน Auth**
- ลด `BufferCapacity` 2 MB → 512 KB (8 บัฟเฟอร์ ⇒ ~16 MB → ~4 MB ต่อเส้น)
- เพดาน 32 เส้น · 4 เส้นต่อ IP — **เช็คก่อนสร้าง `Connection`** ไม่งั้นจองบัฟเฟอร์ไปแล้ว
- เส้นที่ต่อมาแล้วไม่ Auth ใน 15 วิ / ไม่ Ready ใน 45 วิ = ตัดทิ้ง
  (เดิม half-open ไม่มีทางทำให้ `Connected()` เป็น false ⇒ ค้างตลอดกาล)
- ⚠️ ตกหล่นตอนแรก: `SetBuffer(_receiveBuffer, 0, 2097152)` ยังฝังเลขเดิมไว้ → ทุก connection โยน
  `ArgumentOutOfRangeException` **แต่เซิร์ฟไม่ตายเพราะ H-4 จับไว้** (เป็นการพิสูจน์ H-4 โดยบังเอิญ)

**H-8** — เข้าเกมพร้อมกัน 2 เส้นด้วย entity id เดียวกัน = ต่างคนต่างโหลดกระเป๋าจากไฟล์เดียวกัน ⇒ ก๊อปของ
- `Ready`: ถ้ามีผู้เล่น id นี้ออนไลน์อยู่ → `Kick()` เส้นเดิม (เซฟก่อนปิด)
- `Auth`/`Ready` ซ้ำบน connection เดิม = ปฏิเสธ (เดิมสร้าง `ServerPlayer` ซ้ำ เกิดผีค้างในโลก)

**เทสแล้วทั้งหมด:** ยิง 12 เส้นจาก IP เดียว → เหลือ 4 ที่เหลือโดนปฏิเสธ ·
ต่อแล้วเงียบ 18 วิ → `ตัด 127.0.0.1: ต่อมา 15 วิ แล้วยังไม่ Auth` ·
alice เข้าซ้ำ → `เตะเส้นเดิมออก` แล้วเส้นเก่าปิดจริง · ตัวเกมจริงเข้าได้ปกติ ไม่มี error

---

## 2026-08-15 (รอบ 3) — Beta 1.0 รอบ C+D+E: ปิดบั๊กกันโกงที่เหลือครบ

### C · H-5 · M-1 · M-2

**H-5** — `Touch` เคยใช้ `msg.EntityId` ที่ client ตั้งเองเป็นคีย์ของ "จำนวนที่เหลือ"
ยืนที่เดิมแล้วเปลี่ยน id ไปเรื่อย ๆ (`a1` `a2` `a3`…) ก็ได้ generator ชุดใหม่เต็มจำนวนทุกครั้ง
= **เก็บของจากต้นเดียวไม่มีวันหมด** แถม `_generators`/`_naturalTiles` โตไม่จำกัดตามจำนวน id ที่ client คิดขึ้นมา
- id มาจากพิกัดอย่างเดียว: `natural_<tile.x>_<tile.y>` — ค่าที่ client ส่งมาไม่ถูกใช้เลย

**M-1** — `Move` / `PlayEmoticon` ถูก broadcast ดิบ ๆ ⇒ ใส่ `EntityId` ของคนอื่นแล้วสั่งให้เขาเดิน/ทำท่าได้
- ทับ `msg.EntityId = EntityId` ก่อน broadcast ทุกครั้ง

**M-2** — ไม่มีเพดานความเร็ว ⇒ วาร์ปข้ามแมพได้ ทำให้ระยะเอื้อม (H-5/GP-09) และระยะตีไร้ผลทั้งหมด
- เพดาน 900 หน่วย/วินาที + เผื่อ 300 (หน้าต่างวัด 2 วิ) เกินแล้วดึงกลับด้วย `Teleported`
- ผลข้างเคียง: บอทเทสที่เคยส่ง `Move` ทีเดียวข้ามครึ่งแมพใช้ไม่ได้อีกต่อไป
  → `FarmBot`/`GpCheck` เดินเป็นก้าว ๆ (`WalkTo`) แทน

### D · H-6 · H-7 · M-4 · M-7

**H-7 จองที่สร้าง** — เดิมรับทุกพิกัดที่ส่งมา
- เพดาน **40 หลังต่อคน** · ต้องอยู่ในแมพ · ในระยะเอื้อม · ห้ามทับของเดิม

**H-6 สั่งสร้าง/รื้อ** — เดิมไม่เช็คว่ามีของจริงหรือเป็นของใคร
- `TryGetArtifact` + `CanModifyArtifact` + เพดานคิวงาน 32 + ต้องมีสตามินา

**M-4 กล่องเก็บของ** — เดิมเช็คแค่ "เป็นกล่อง" ⇒ รู้ entity id (ซึ่งมากับ `AppearArtifact` ที่ broadcast ให้ทุกคน)
ก็ขนของในบ้านคนอื่นได้จากอีกฟากแมพ
- `CanUseBox()` = เป็นกล่อง + เป็นของตัวเอง + อยู่ในระยะเอื้อม ใช้ทั้ง `PutInItem` และ `TakeOutItem`

**M-7 ช่องอุปกรณ์/สกิล** — ชื่อช่องและ skill id ไม่ถูกกรอง ⇒ ยิงชื่อมั่ว ๆ ใส่ไฟล์เซฟจนบวมเป็น GB
- ช่องอุปกรณ์: whitelist `ValidSlots`
- สกิล: id ต้องมีใน `SkillData` · เพดานเลเวล 60 · `SubId` ≤ 40 ตัวอักษร

### E · M-6 ประสิทธิภาพตอนคนเยอะ

- **rate limit 120 packet/วินาที** ต่อ connection — เกิน 3 หน้าต่างติดกันถึงเตะ (กันคนเน็ตกระตุกโดนลูกหลง)
- **index garden รายก้อน** (`_gardenByChunk`) — เดิม `SetChunk` สแกน garden ทั้งก้อน **9 รอบต่อการเดิน 1 ครั้ง**
  คนเดียวเดินไปมาก็ทำ tps ตกได้ทั้งเซิร์ฟ · invalidate index ตอน `RemoveNatural`

### ช่องโหว่ที่เจอตอนโซกเทส (ไม่ได้อยู่ในลิสต์เดิม)

**ทิ้งของ/กินของไม่ได้เลย** — `DumpItems` และ `UseItem` **ไม่มี handler ฝั่ง server**
บอทฟาร์มเก็บของจนเต็ม 50 ช่องภายใน ~2 นาที แล้วหลังจากนั้นทำอะไรไม่ได้อีกเลยจนจบ 30 นาที
คนเล่นจริงจะเจอเหมือนกัน และไม่มีทางออกเพราะคนเพิ่งเข้าเกมยังไม่มีกล่องเก็บของ
- `ServerCore/ServerPlayer.Items.cs` (ใหม่) — `HandleDumpItems` (ทิ้งจากกระเป๋าหรือจากกล่องของตัวเอง,
  เพดาน 50 ชิ้น/packet, id ที่ไม่มีจริงถูกข้าม) · `HandleUseItem` (กินของ: +30 สตามินา, ความล้า −15)
- ของที่ "กินได้" ดูจาก prototype (fruit/berry/meat/fish/egg/mushroom/nut/seed/honey/milk/food)
  ยังไม่มีข้อมูลโภชนาการจริงของเกม — beta เอาแค่ "กินแล้วได้แรง"

**`Teleported` ไม่เคยถูกส่ง** — `SendTeleport()` ส่งแต่ `Move` ซึ่ง **ตัวคนนั้นเองไม่สนใจ**
(`PlayerManager` ข้าม `Move` ของ local player ทิ้ง) แปลว่า:
- ตอนฟื้นจากตาย server ย้ายให้แล้วแต่จอผู้เล่นไม่ขยับ (server กับ client มองตำแหน่งคนละที่)
- M-2 ที่ "ดึงกลับที่เดิม" จริง ๆ แล้วดึงไม่ได้ ได้แค่ไม่รับ move นั้น
แก้เป็นส่ง `Teleported` (ตัวเอง) + `Move` (คนอื่นเห็น) และ **แยกความรุนแรง**:
เกินเพดานไม่ถึง 3 เท่า = ไม่รับ move เฉย ๆ · เกิน 3 เท่าขึ้นไป = สั่ง `Teleported` เด้งกลับ
เพราะ client กิน `Teleported` ด้วยการ **ขึ้นจอโหลด** ถ้าเด้งทุกครั้งที่คลาดนิดเดียวคนเล่นปกติจะเจอจอโหลดกะพริบทั้งเกม
(ตอนฟื้นจากตายใช้ `TeleportType.Revive` ซึ่งไม่เล่นท่า Warp_End)

**บอทฟาร์มวาร์ป** — `FarmBot` กระโดดไปยืนบนเป้าหมายทันที ซึ่ง M-2 ปฏิเสธทุกก้าว
ทำให้ 30 นาทีแรกของโซกเทสวัดอะไรไม่ได้เลย (bot วนแตะ tile ไกล ๆ แล้วโดน abort รัว ๆ)
- เดินทีละก้าวตามเวลาจริงที่ **450 หน่วย/วินาที** (ตัวเกมจริง default 500 · ดู `PlayerController.MoveSpeed`)
- รับ `Teleported` แล้วยอมรับตำแหน่งของ server
- กระเป๋าเต็ม → `DumpItems` ทิ้ง 25 ชิ้นแล้วเก็บต่อ

### ปิดของที่ยังไม่พร้อมเปิดให้คนนอก

- **radiotower (พอร์ต 8192) ปิดเป็นค่าเริ่มต้น** — พอร์ตนี้ไม่มี auth เลย ใครต่อเข้ามาก็ประกาศตัวเป็นใครก็ได้ (M-5)
  แชทช่องรวมวิ่งบน connection เกมที่ Auth แล้ว ไม่ได้ใช้พอร์ตนี้ · เปิดกลับด้วย `--radiotower`
- **แชทมีเพดานแล้ว** — ข้อความยาวไม่เกิน 200 ตัวอักษร · ส่งได้ทุก 0.7 วินาที
  (1 ข้อความ = broadcast ออก N ข้อความตามจำนวนคนในโลก คนเดียวจึงกินแบนด์วิดท์แบบคูณได้)
- `[loop]` ในบรรทัดสถิติทุก 30 วินาที เพิ่ม **จำนวนสัตว์ในโลก + RAM** เอาไว้ดูตอนเปิดเซิร์ฟจริง

### เทสที่เพิ่ม

- `--gp-check` เพิ่มรอบ D (จองที่ไกล/นอกแมพ · สร้างของที่ไม่มีจริง/ของคนอื่น · เปิดกล่องคนอื่น ·
  ช่องอุปกรณ์ที่ไม่มีจริง · สกิลที่ไม่มีในเกม · สกิลเลเวล 9999) → **ผ่าน 30/30**
- `--multi-check` (ใหม่, `test-client/MultiCheck.cs`) — ต่อ 3 client พร้อมกันแล้ว **แย่งเก็บของจุดเดียวกัน**
  15 ครั้งบนของที่มี 3 หน่วย → ได้รวม 3 ชิ้นพอดี ที่เหลือ `Abort` · ทุกคนเห็นกันและเห็นจำนวนที่เหลือชุดเดียวกัน
  → **ผ่าน 9/9**
- `--gp-check` เพิ่มหมวดทิ้งของ/กินของ (ทิ้งของที่ไม่มีจริง · ทิ้งทีเดียว 80 ชิ้น · ทิ้งของในกล่องคนอื่น ·
  กินของที่ไม่มีจริง · กินขวาน · ทิ้งของจริงแล้วของหายจริง) → **รวมเป็น 36/36**

---

## 2026-08-15 (รอบ 4) — เทสกับเกมจริง: ปุ่มโจมตี · ความไวของสัตว์ · แล่เนื้อ

สามข้อที่เจ้าของโปรเจกต์เจอตอนเล่นด้วยตัวเกมจริง (เกณฑ์ข้อ 3 ของ Beta 1.0)

### 1. คลิกที่สัตว์แล้วไอคอนโจมตีไม่เด้ง ★

เมนูวงกลมของ client **มาจาก `Touched.Interactions` ของ server ล้วน ๆ** (client แค่แปลงเลขเป็นปุ่ม)
แต่ `HandleTouch` มีแค่ 2 เคส: ของธรรมชาติ (type ≥ 10000) กับสิ่งปลูกสร้าง (มี blueprint)

สัตว์ (type 2000-2999) จึงตกไปทางของธรรมชาติ → ตอบ `Touched` ที่ `Interactions` ว่าง = **เมนูเปล่า**
ซ้ำร้าย `EntityId` ที่ตอบกลับถูกเขียนทับเป็น `natural_-1_-1` เพราะสัตว์ส่ง `Tile = (-1,-1)` มาเสมอ
(`client/InteractionObject.cs` → `Tile` คืน `-Vector2.one` ถ้าเป้าเป็น Animal)

- เพิ่มสาขาสัตว์ใน `HandleTouch` → `HandleTouchAnimal()`
  - ยังเป็นอยู่ → `Interactions = [1]` (Attack) + ชื่อไทย + เลเวล
  - ซาก → `Collectible` ของการแล่เนื้อ (ข้อ 3)
- เปลี่ยนเลข interaction ดิบ `{506, 10268}` เป็นค่าคงที่ที่มีชื่อ

ไฟล์: `ServerCore/ServerPlayer.Gathering.cs`

### 2. สัตว์สวนกลับช้าเกินไป

สองสาเหตุซ้อนกัน:

| สาเหตุ | เดิม | ตอนนี้ |
|---|---|---|
| ทั้งไล่และหนีติดเงื่อนไข `now >= NextMoveAt` ซึ่งตอนโดนตีมักเป็นช่วง "พักหลังเดินถึงที่หมาย" | ยืนนิ่งรอได้ถึง **14 วินาที** | `OnAttacked`/`LookForPrey` ล้าง `NextMoveAt = now` |
| การกัดครั้งแรกใช้คูลดาวน์เต็ม | 2.5 วิ | `FirstAttackDelay` 0.5 วิ |
| คูลดาวน์ครั้งถัด ๆ ไป | คงที่ 2.5 วิทุกชนิด | `attack_cooltime` **ค่าจริงจากข้อมูลเกม** 1.3–3.0 วิ แล้วแต่ชนิด |
| ความเร็วไล่ | 220 หน่วย/วิ | 300 (ก้าวละ ~1 วิ เท่าเดิม แต่ก้าวยาวขึ้น) |

วัดจริง: ตีโปรโตเซราท็อปส์ที่กำลังพักอยู่ → สวนกลับใน **0.5 วิ** แล้วกัดทุก **1.3 วิ** (= ค่าจริงของชนิดนี้)

ไฟล์: `ServerCore/AnimalSpawner.cs` · `ServerCore/SpawnTable.cs` (คอลัมน์ `AttackCooltime`)

### 3. แล่เนื้อไม่ได้ (ของใหม่)

เกมไม่มีเมนู "แล่" แยก — มันคือ `Collectible` ชุดเดียวกับเก็บของธรรมชาติ แต่เจ้าของ generator เป็นซากสัตว์

- **`ServerCore/ButcheryData.cs` (ใหม่)** — ชิ้นส่วนของสัตว์ 10 ชนิดในเกาะเริ่มต้น
  รหัส generator (`meat` `leather_raw` `bone_leg` `bone_horn` `feather` ...) และไอคอนเป็น**ของจริงจากเกม**
  ส่วนจำนวน/เวลาตั้งเองตาม `size_level` เพราะตารางดรอปจริงอยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client
  ตัวใหญ่ (size 4) ได้เนื้อ 4 ชิ้น · ทุก 5 เลเวลได้เพิ่มชนิดละ 1
- สัตว์ตาย → สร้าง generator ของซากเก็บที่ world (ของกลาง กันสองคนแล่ซากเดียวได้ครบทั้งคู่)
  + broadcast `CollectibleDisplay` ให้ซาก**เรืองแสง**สำหรับคนที่ฆ่า (`AnimalBehavior.IsLootable`)
- `HandleCollect` แยกสาขาซาก → `HandleButchery()`: ระยะคิดจากตัวซากตรง ๆ · หน่วงตาม `Duration` จริงของชิ้นส่วน
- **`ServerWorld.TryReserveCorpsePart()`** แยกจาก `TryReserveGenerator()` เพราะซากหมด**ทีละชิ้นส่วน**
  (เนื้อหมดแล้วยังแล่หนังต่อได้) ต่างจากต้นไม้ที่หมดทีเดียวทั้งต้น
- ซากอยู่ในโลก **30 → 150 วินาที** (ตัวใหญ่มี 8-9 หน่วยให้แล่ ~30 วิ + เวลาเดินไปหา) · แล่หมดตัว = ซากหายทันที
- `Remove()` ล้าง generator/คิวซากทิ้งด้วย ไม่งั้น `_generators` โตขึ้นทุกตัวที่ตาย

### 4. 🐛 เปิดเซิร์ฟซ้ำสองตัวแล้ว **ทั้งคู่ทำงานพร้อมกัน** (เจอโดยบังเอิญตอนเทส) ★

ระหว่างเทสรอบนี้ `--gp-check` จู่ ๆ ตกจาก 42/42 เหลือ 12/42 ทั้งที่โค้ดไม่ได้แตะ —
ผลที่ได้แปลกมาก (คราฟต์ลมสำเร็จ · token มั่วเข้าได้ · สองคนใหม่ได้ token เดียวกัน)

สาเหตุ: `Listener.Start()` ใช้ `Socket` ธรรมดาโดยไม่ตั้ง `ExclusiveAddressUse`
**Windows จึงยอมให้สอง process ฟังพอร์ต 8191 พร้อมกัน** แล้วแบ่งกันรับ connection แบบสุ่ม
⇒ มี DurangoServer ตัวเก่าค้างอยู่ 1 ตัว ผู้เล่นครึ่งหนึ่งไปโผล่อีกโลกหนึ่ง

**อันตรายมากถ้าเกิดตอนเปิด beta จริง** — คนละโลก คนละไฟล์เซฟ ทับกันตอน autosave

- ตั้ง `_listenSocket.ExclusiveAddressUse = true` ก่อน `Bind()`
- ทดสอบแล้ว: ตัวที่สองได้ `SocketException 10048` → เข้าทาง `[fatal] เปิดพอร์ตเกม 8191 ไม่ได้ —
  พอร์ตถูกใช้อยู่ (เปิดเซิร์ฟซ้ำ?)` แล้วปิดตัวเอง เหลือตัวเดียวตามที่ควรเป็น

ไฟล์: `GameCode/Durango.Offline/Listener.cs`

### เก็บกวาดระหว่างทาง

- cheat ใหม่ `kill animal` — ฆ่าสัตว์ตัวที่ใกล้ที่สุดทันที (ไว้เทสการแล่โดยไม่ต้องยืนตีเป็นนาที)
- บรรทัด `[loop]` นับ**เฉพาะตัวเป็น ๆ** แล้วต่อท้าย `(+ซาก n)` — ซากอยู่นานกว่าเวลาเกิดใหม่
  ถ้ารายงานรวมกันตัวเลขจะเกินโควตาเป็นระยะจนคนดูแลเซิร์ฟเข้าใจผิด
- `[ดาเมจ]` ของบอทคอนโซลมีเวลากำกับแล้ว — วัดความไวของสัตว์จาก log ได้ตรง ๆ

### เมนูเทสคลิกเดียว (`เทสเกม.bat` + `tools/menu.ps1`)

ดับเบิลคลิกแล้วเลือกข้อได้เลย — build/เปิดเซิร์ฟ/เปิดเกม/รันเทส/ปิดเซิร์ฟ ครบในที่เดียว
เขียนขึ้นเพราะขั้นตอนเทสมีกับดักที่ลืมทีไรเสียเวลาทุกที และเมนูกันให้หมดแล้ว:

- kill `DurangoServer.exe` ก่อน build เสมอ (MSB3021)
- กันเปิดเซิร์ฟ/เกมซ้อนกัน 2 ตัว
- **ตรวจโหมดของเซิร์ฟที่เปิดค้างอยู่จาก command line** — ถ้าเป็นโหมดเปิดจริง (cheat ปิด)
  แล้วผู้ใช้กดเทส มันจะปิดแล้วเปิดใหม่ให้ (ไม่งั้นเทสตกยกแผงแบบไม่รู้สาเหตุ)
- ข้อ 4 = เช็คลิสต์ 30 นาทีว่าต้องดูอะไรบ้าง (รวมของใหม่รอบนี้: ปุ่มโจมตี · ความไวสัตว์ · แล่เนื้อ)

⚠️ ข้อจำกัดเรื่องภาษา: `.bat` ต้องเป็น **ASCII ล้วน** (cmd อ่านเป็น CP874) ·
`.ps1` ต้องเซฟเป็น **UTF-8 มี BOM** (PowerShell 5.1 อ่านไฟล์ไม่มี BOM เป็น ANSI)
ข้อความไทยทั้งหมดจึงอยู่ใน `.ps1` ไม่ใช่ `.bat`

### กล่องเครื่องมือตอนเล่น + คำสั่ง control ชุดใหม่

ปัญหาจริงตอนเทส: ยืนอยู่ในเกมแล้วต้อง **เดินหาสัตว์เป็นนาที** กว่าจะทดสอบการต่อสู้/แล่เนื้อได้สักรอบ
แก้ด้วยการต่อยอด `control` (รีโมทคุมตัวละครที่มีอยู่แล้ว) ให้สั่งจากเมนูข้างนอกได้:

| คำสั่งใหม่ | ทำอะไร |
|---|---|
| `control <ชื่อ> spawn [type]` | เสกสัตว์มาเกิด **ข้างตัวคนนั้น** (ไม่ใช่ข้างตัว admin) |
| `control <ชื่อ> kill` | ฆ่าสัตว์ตัวใกล้ที่สุดของคนนั้น — เครดิตเป็นของคนนั้น ซากจึงเรืองแสงให้เขา |
| `control <ชื่อ> heal` | เลือด/สตามินาเต็ม · ตายอยู่ = ฟื้นให้ |
| `control <ชื่อ> give <axe/clothes/bonfire/box>` | เสกของทดสอบให้ |
| `cheat who` | ใครออนไลน์บ้าง (ชื่อ · id · tile · เลเวล · ตายอยู่ไหม) — เมนูใช้หาชื่อไปสั่งต่อ |

- แยก `ReviveAtSpawn()` ออกจาก `HandleRevive()` — การฟื้นที่ admin สั่งต้อง**ไม่ส่ง** `Revived`
  ซึ่งเป็น "คำตอบ" ของ packet ที่ client ไม่เคยส่งมา (`ReplyOf = 0` ไปชนคีย์ reply ของ client)
- เพิ่ม `ServerWorld.SnapshotPlayers()` · `AnimalSpawner.AliveCount`

### เอกสารสำหรับผู้ทดสอบ

`docs/BETA-1.0-PLAYERS.html` — หน้าเดียวจบว่าเข้าไปแล้วจะเจออะไร ระบบไหนเปิด/ปิด/ยังไม่มี ·
สัตว์ 10 ชนิดแล่ได้อะไรบ้าง · กติกา (whitelist/เพดาน/ตายไม่เสียของ) · เช็คลิสต์ 15 ข้อที่ติ๊กได้ ·
บั๊กที่รู้แล้วไม่ต้องแจ้งซ้ำ

### เทสที่เพิ่ม

`--gp-check` หมวดใหม่ "แตะสัตว์ / แล่เนื้อ" 5 ข้อ → **รวมเป็น 42/42**
(แตะสัตว์เป็น ๆ ได้ปุ่มโจมตี · แล่สัตว์ที่ยังไม่ตายไม่ผ่าน · แตะซากได้เมนูแล่ ·
แล่ซากได้ของจริงเข้ากระเป๋า · แล่ซากไกลเกินเอื้อมไม่ผ่าน) · `--multi-check` ยัง 9/9

---

## 2026-08-15 (รอบ 5) — ระบบเลเวล + ไฟล์ตั้งค่าเซิร์ฟ

### ระบบค่าประสบการณ์/เลเวล (ของใหม่)

- **`ServerCore/LevelData.cs`** — ตารางเลเวล **ค่าจริงของเกม** สกัดจาก `level_thresholds`
  ด้วย `scripts/extract_levels.py` (lv2 = 11 exp · lv3 = 25 · lv11 = 733 · ถึง lv81)
  client ใช้ตารางชุดเดียวกันวาดหลอด exp จึงต้องตรงกันเป๊ะ
- **`ServerCore/ServerPlayer.Progress.cs`** — `GainExp()` ส่ง `ExpGained` + `Statistics` ชุดใหม่
  ขึ้นเลเวลแล้ว: เพิ่มแต้มสกิล · เติมเลือด/สตามินา (เพดานผูกกับเลเวล) · broadcast ให้คนอื่นเห็นเลเวลใหม่
- ได้ exp จาก: ฆ่าสัตว์ (ตามเลเวลสัตว์) · เก็บของ · แล่เนื้อ · คราฟต์ · สร้างของ
- **exp เป็นตัวจริง เลเวลเป็นผลลัพธ์** — ตอนโหลดเซฟคิดเลเวลใหม่จาก `TotalExp` เสมอ
  (ผู้เล่นเก่าที่ยังไม่มี exp ในเซฟ ได้ exp ขั้นต่ำของเลเวลเดิม ไม่ถูกลดเลเวล)

⚠️ ข้อมูลเกมตั้ง `exp_amount` ของสัตว์ทุกตัวเป็น **0** — ของจริงคิด exp จากระบบ ability/resistance
ที่อยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client **แต้มที่ให้ต่อการกระทำจึงเป็นของเราเอง**

### `data/config.json` — ปรับสมดุลโดยไม่ต้อง build ใหม่ ★

เดิมเรทเกิดสัตว์/เลือด/ดาเมจ/exp เป็น `const` ในโค้ด แก้ทีต้อง build ใหม่ทุกที
คนที่ดูแลเซิร์ฟแต่ไม่ได้เขียนโค้ดปรับอะไรเองไม่ได้เลย

- **`ServerCore/ServerConfig.cs`** — โหลด JSON ตอนเปิดเซิร์ฟ (ไม่มีไฟล์ = สร้างค่าเริ่มต้นให้)
  แล้ว **ตรวจเวลาแก้ไขไฟล์ทุก 5 วินาที** เจอว่าเปลี่ยนก็โหลดใหม่ทันที
- ไฟล์เสีย/ค่าไม่ผ่าน `Validate()` → เตือนใน log แล้ว**ใช้ค่าเดิมต่อ** เซิร์ฟไม่ล่ม
- `SpawnTable` เหลือแค่ตัวอ่านค่า · `AnimalSpawner` เปลี่ยน `const` 10 ตัวเป็น property ที่อ่านจาก config
- มีผลทันที: เลือด/ดาเมจ/ความเร็ว/เวลาซาก/เวลาเกิดใหม่/exp
  · มีผลตอนเปิดใหม่: ชนิดและโควตาสัตว์ (สัตว์เกิดไปแล้วตั้งแต่ตอนเปิด)
- **เมนูข้อ 17** แก้ค่าได้จาก UI ไม่ต้องแตะ JSON เอง (มีตรวจค่าก่อนเซฟทุกช่อง)

ทดสอบแล้ว: แก้ `KillBase` 4 → 120 ระหว่างเซิร์ฟรัน → log ขึ้น `[config] โหลด config.json แล้ว`
ภายใน 5 วินาที → ฆ่าสัตว์ตัวถัดไปได้ 126 exp → `[level] ⭐ ขึ้นเลเวล 1 → 6` → เซฟลงไฟล์ครบ

### เอกสาร

`docs/server/Config.md` — ค่าทุกตัวหมายถึงอะไร มีผลเมื่อไร **และข้อมูลผู้เล่นเก็บยังไง**
(ไฟล์ JSON ใน `saves/players/<id>.json` ไม่มี database engine — เหตุผลและวิธีสำรอง/แก้มือ)

---

## 2026-08-15 (รอบ 6) — เครื่องมือของจริง: ปิด GP-08b ★

เดิมไอเทมที่ server สร้าง **ไม่มี `Tags` เลย** ทำให้สองอย่างพังพร้อมกัน:
เก็บของอะไรก็ได้ด้วยมือเปล่า · คราฟต์เอาอะไรมายัดก็ผ่าน (ตรวจได้แค่ "มีของอยู่ในกระเป๋าไหม")

### `ItemTagData.cs` (generated, 1,904 ไอเทม)

`scripts/extract_item_tags.py` ดึง `tags` ของทุก prototype จาก `prototype_data` ในข้อมูลเกม
เช่น `stone` = `chunk_normal + stone` · `wood_log` = `wood + pillar_normal + burnable`

⚠️ ข้อมูลเกมมี tag เครื่องมือรวม ๆ แค่ `tool` ไม่ได้แยกขวาน/มีด/อีเต้อ (ตารางที่แยกอยู่ฝั่ง
server ของ NEXON) สคริปต์จึง**เติม tag ชนิดเครื่องมือเองจากชื่อ prototype** —
`axe` `knife` `pickaxe` `shovel` `hammer` `sickle` ระดับตามวัสดุ (หิน 1 · กระดูก 2 · โลหะ 3)

### ผลที่ตามมา

| เรื่อง | ก่อน | หลัง |
|---|---|---|
| ตัดไม้ (`wood_log`/`wood_bough`/`wood_bush`) | มือเปล่า | ต้องมี **ขวาน** |
| หินก้อนใหญ่ / แร่ | มือเปล่า | ต้องมี **อีเต้อ** |
| แล่ซากสัตว์ | มือเปล่า | ต้องมี **มีด** |
| ผลไม้ · ใบไม้ · ลำต้น · หินก้อนเล็ก · ดินเหนียว | มือเปล่า | มือเปล่าเหมือนเดิม |
| คราฟต์ | ตรวจแค่ว่ามีของจริง | ตรวจ **tag + วัสดุตามสูตร** (`chunk_normal` + `stone`) |

- server **ไม่เชื่อ `ToolItemId` ที่ client ส่งมา** — ตรวจเองว่าไอเทมนั้นอยู่ในกระเป๋าจริง
  และมี tag ตรงกับที่ generator ขอ · ไม่ส่ง id มาก็ค้นในกระเป๋าให้เอง (บอทไม่ต้องรู้เรื่อง tag)
- ขาดเครื่องมือ → ตอบ `ToolNeeded` พร้อม **รายชื่อสูตรที่คราฟต์เครื่องมือนั้นได้**
  (`ItemTagData.RecipesMakingTag`) client เปิดหน้าสูตรให้เลย
- เช็คเครื่องมือ **ก่อน**จอง generator ไม่งั้นคนที่ไม่มีขวานกินหน่วยของคนอื่นทิ้งเปล่า ๆ

### สายเริ่มต้นยังเล่นได้จริง

เกิดมาพร้อมขวานหิน → เก็บหินด้วยมือเปล่า → คราฟต์ `blade_stone` (หิน 1 ก้อน) = **มีด** → แล่ซากได้
ตรวจแล้วว่าสูตรนี้มีจริงในเกมและวัตถุดิบหาได้ด้วยมือเปล่า (ไม่ได้ตันตั้งแต่ต้น)

cheat ใหม่สำหรับเทส: `add stone` · `add knife` · `add pickaxe`

### เทส

`--gp-check` เพิ่ม 2 ข้อ → **44/44**
(แล่เนื้อโดยไม่มีมีดไม่ผ่าน · มีมีดแล้วแล่ได้ · คราฟต์มีดด้วยของที่ไม่ใช่หินไม่ผ่าน · ใส่หินจริงสำเร็จ)
ข้อ "คราฟต์ปกติ" เดิมเคยผ่านแบบหลอก (ยัด item อะไรก็ได้ 3 ชิ้น) — เปลี่ยนเป็นสูตรจริงแล้ว

---

## 2026-08-16 — สกิลมีผลกับเกมจริง

เดิมสกิลเป็นแค่ตัวเลขในไฟล์เซฟ: เรียนได้ ลืมได้ UI โชว์ครบ **แต่ไม่มีผลกับอะไรเลย**
และจ่าย 1 แต้มได้สกิลเลเวล 60 (เลเวลมาจาก client ล้วน ๆ)

### ผลของสกิล — `ServerPlayer.SkillEffects.cs` (ใหม่)

รวมเลเวลสกิลทุกอันใน**หมวดเดียวกัน** เทียบ `skills.FullAt` (60) → สัดส่วน 0-1 → คูณเพดานของหมวด
(ไม่ผูกกับสกิลรายตัว เพราะข้อมูลผลของสกิลแต่ละอันอยู่ฝั่ง server ของ NEXON — ที่ได้มามีแค่ชื่อ 275 อันกับหมวด)

| หมวด | มีผลกับ | เพดาน |
|---|---|---|
| `Gathering` | เก็บของเร็วขึ้น + โอกาสได้ของเพิ่ม 1 ชิ้น | 40% / 30% |
| `Butchery` | แล่ซากเร็วขึ้น + โอกาสได้ชิ้นส่วนเพิ่ม | 40% / 30% |
| `MeleeCombat` | ดาเมจที่ตีออก | +50% |
| `Defense` | ดาเมจที่รับ | -30% |
| `Weaponcrafting` `Armorcrafting` `Constructing` `Cooking` `Process` | คราฟต์เร็วขึ้น | 40% |
| `Survival` | ประหยัดสตามินาทุกอย่าง | 30% |

ทุกเพดานปรับได้ที่ `data/config.json` → `skills` (เมนูข้อ 17 → 7)

### ราคาและเงื่อนไขการเรียน

- เรียนสกิลเลเวล N ใช้ **N แต้ม** (อัปจาก 2→5 จ่าย 3) · ลืมคืนแต้มเท่าที่จ่ายจริง
- **เลเวลสกิลต้องไม่เกินเลเวลผู้เล่น** (`RequiredPlayerLevelPerSkillLevel`)
- **ผู้เล่นใหม่เริ่มที่ 0 แต้ม** (เดิมแจก 777 ตั้งแต่แรกเพราะสกิลยังไม่มีผล) — แต้มมาจากการขึ้นเลเวลเท่านั้น

### เก็บกวาดที่เจอระหว่างทาง

- 🐛 **`Statistics.Exp` ส่ง 0 เสมอ** — หลอด exp ฝั่ง client เลยไม่ขยับทั้งที่ server เก็บ exp ถูก แก้เป็น `TotalExp`
- ✅ **ปิด GP-09b** — เวลาเก็บของใช้ `generator.Duration` จริงแล้ว (เดิม 2 วิตายตัวทุกชิ้น)
  และเวลาที่บอก client กับที่ server หน่วงตรงกันทั้งเก็บของ/แล่/คราฟต์
- cheat `skills` — ดูเลเวล/exp/แต้ม/โบนัสของตัวเองเทียบก่อน-หลังเรียน
- บอทคอนโซลมีคำสั่ง `skill <ชื่อ> <เลเวล>`
- config เติมหัวข้อที่หายไปในไฟล์แล้วเขียนกลับให้เอง (อัปเดตเซิร์ฟแล้วไฟล์เก่าไม่มีหัวข้อใหม่ = แก้ผ่านเมนูไม่ได้)

### เทส

`--gp-check` **45/45** (เพิ่ม "เรียนสกิลเลเวล 60 ตอนตัวเองเลเวลต่ำ ไม่ผ่าน") · `--multi-check` 9/9
เดินสายจริง: ฆ่ากิ้งก่า 2 ตัว → เลเวล 3 → ได้ 6 แต้ม → เรียน `gathering` เลเวล 2 → เหลือ 4 แต้ม
→ `cheat skills` โชว์ "เก็บของ 2 (เร็วขึ้น 1%)" ตรงกับสูตร 2/60 × 40%

---

## 2026-08-16 (รอบ 2) — เกาะแยกเลเวล (ครึ่งทาง) + แก้ tps ร่วง

### เกาะแยกตามช่วงเลเวล — `--island <id>`

1 เกาะ = 1 process · คนละ terrain/config/ไฟล์เซฟโลก/พอร์ต · **ตัวละครใช้เซฟร่วมกันทุกเกาะ**
ทะเบียนอยู่ที่ `data/islands.json` (เริ่มต้นให้ 3 เกาะ: lv1-10 · lv10-20 · lv20-30)

- สร้าง config ให้เกาะใหม่แล้ว **เลื่อนช่วงเลเวลสัตว์ให้ตรงกับเกาะอัตโนมัติ**
  (ตารางมาตรฐาน lv1-10 → เกาะ lv10-20 กิ้งก่ากลายเป็น lv10-12 เรียงอ่อน-แรงเหมือนเดิม)
- `PlayerSave.LastIsland` — เข้ามาคนละเกาะกับที่จำไว้ = เกิดที่จุดเข้าเกมของเกาะใหม่
- `cheat islands` · `cheat travel <id>` · `control <ชื่อ> travel <id>`
- **เทสแล้วได้จริง:** เลเวล 10 · exp 409 · แต้มสกิล 27 · ของ 3 ชิ้น ข้ามจาก isle01 → isle02 ครบ
  และเกาะ 2 (terrain คนละอัน จุดเข้าเกมคนละที่) เกิดสัตว์ lv10-20 ตามที่ตั้งไว้

⏳ **ยังเดินทางจากในเกมไม่ได้** — `Server.ConnectTo()` ของ client ฮาร์ดโค้ด gateway 8190
และไม่มี packet ไหนส่ง "ที่อยู่เซิร์ฟใหม่" มาให้ · ลองทางแยก IP (127.0.0.2) แล้วติด Access denied
ต้องเป็น admin → **ทางที่แนะนำคือ patch client** (มีโครง DllPatcher อยู่แล้ว) ดู `docs/server/Islands.md`

🐛 **บั๊กค้าง:** โหมด `--island` ทำให้ `--gp-check` ตก 2 ข้อ (แตะสัตว์แล้ว client ได้ `Touched` ว่างทั้งก้อน
ทั้งที่ log ฝั่ง server บอกว่าตอบถูก) — โหมดเกาะเดียวยัง **45/45** · บันทึกที่รู้ทั้งหมดไว้ใน Islands.md แล้ว

### 🐛 tps ร่วงจาก 120 เหลือ 64 — แก้แล้ว ★

ตอนย้ายตารางสัตว์ไป config เขียน `SpawnTable.Entries` เป็น property ที่ **สร้าง List + array ใหม่ทุกครั้งที่อ่าน**
แต่ AI ถามตารางนี้ **ทุก tick ต่อสัตว์ทุกตัว** (นิสัย + คูลดาวน์กัด) ⇒ 40 ตัว × 120 tps = allocation หลายพันครั้ง/วินาที

- อาการที่เห็นก่อน: `--gp-check` ตกแบบสุ่มคนละข้อทุกรอบ เพราะ reply มาช้ากว่าที่เทสรอ
- แก้ด้วยการแคช entries + dictionary ตาม entity type สร้างใหม่เฉพาะตอน config โหลดใหม่
  (เทียบ reference ของ list ที่ ServerConfig ถือไว้)
- หลังแก้: **120 tps นิ่ง** · RAM 2 MB ตอนไม่มีคนเล่น

> บทเรียน: property ที่หน้าตาเหมือนอ่านค่าเฉย ๆ แต่จริง ๆ allocate ทุกครั้ง เป็นกับดักที่มองไม่เห็นจาก call site
> — ของที่ AI loop เรียกทุก tick ต้องเป็นการอ่านค่าล้วน ๆ เท่านั้น

### เทส

โหมดเกาะเดียว: `--gp-check` **45/45** · `--multi-check` **9/9** (ทั้งคู่รันหลังแก้ tps แล้ว)

---

## 2026-08-16 (รอบ 3) — เลิกใช้ IL patch: แก้ซอร์สแล้ว build ตัวเกมเองได้ ★

**คำถามคือ "ต้อง patch อย่างเดียวเลยไหม" คำตอบคือไม่ — build เองได้ และดีกว่ามาก**

`client/` (3,760 ไฟล์จาก ILSpy) มี `Assembly-CSharp.csproj` ที่อ้าง DLL ของ Unity ในเกมโดยตรง
build ผ่าน 0 error ตั้งแต่ 13 ส.ค. แต่ **ไม่เคยมีใครลองเอา DLL ที่ build เองไปใส่เกมจริง**

### ผลทดสอบ

| เรื่อง | ผล |
|---|---|
| build (`dotnet build -c Release`) | 0 error ~5 วินาที · DLL 5.8 MB |
| เกมบูต | ✅ ถึงหน้าไตเติ้ล ไม่มี TypeLoad/MissingMethod |
| ต่อเซิร์ฟด้วย DLL ที่ build เอง | ✅ `[world] player joined: ฟหกฟหก level=60` |
| `referenced script is missing` | 1 อัน (`CombatModeButton`) — **มีอยู่แล้วใน DLL ที่ patch** ไม่ใช่ของใหม่ |

รอบแรกที่ลอง (ก่อนพอร์ต patch) เจอ 4 อัน — พอพอร์ต patch ครบเหลือ 1 เท่า DLL เดิมเป๊ะ

### ย้าย patch ทั้งหมดเข้าซอร์ส

| patch เดิม (IL) | ย้ายไปอยู่ที่ |
|---|---|
| `PatchAutoConnect` | `Durango.Offline/Server.cs → BeginServer` |
| ฮาร์ดโค้ด gateway 8190 | `Server.cs → ConnectTo` — **รับ `ip:port` แล้ว** |
| `PatchHideUnimplementedMenus` | `MenuSystem.cs → NotImplementedYet` (แก้รายการง่าย ๆ) |
| `PatchServerAnimalSpawn` | `AnimalManager.cs` handler ของ `AppearAnimal` |
| `PatchSelfIpFilter` | `Durango.UI/MenuListGroupBase.cs → OnSelectItem` |
| ที่เหลือ | อยู่ในซอร์สอยู่แล้ว (ซอร์สถอดมาจาก DLL ที่ patch รอบแรกไปแล้ว) |

ทุกจุดมีคอมเมนต์ `[แก้เอง]` — `grep -rn "\[แก้เอง\]" client/`

### ปลดล็อกระบบเกาะไปในตัว

พอแก้ซอร์สได้ ก็ทำ **เดินทางข้ามเกาะ** ที่ติดอยู่ได้เลย:
- `GameManager.DefaultInfoHandler` ดักข้อความ `##goto <ip:port>` ที่ server ส่งมา
- `Frontend_ConnectionClosed` ต่อไปเกาะปลายทางแทนการกลับเกาะเดิม
- `ConnectTo` รับพอร์ตแล้ว (เดิมฮาร์ดโค้ด 8190 = เหตุผลเดียวที่ทำเกาะหลายเกาะไม่ได้)

⚠️ **ยังไม่ได้เทสกับเกมจริง** — เขียนเสร็จแล้วแต่ต้องลองเดินทางในเกมจริงอีกที

### เครื่องมือ

`tools/build-client.ps1` — ปิดเกม → build → สำรอง DLL เดิม (เก็บ 10 อันล่าสุด) → วางลงเกม
`-Restore` ย้อนกลับ · `-NoInstall` build เฉย ๆ · เมนู `เทสเกม.bat` ข้อ **18/19**
เอกสาร: `docs/client/BUILD.md`

---

## 2026-08-16 (รอบ 4) — สัตว์เกิดเป็นโซน + แก้อาการ "เดินวาร์ป" ★

### 🐛 ต้นตอของอาการสัตว์วาร์ป — แก้แล้ว

`MakeMove()` ตั้ง `Position = ปลายทาง` **ทันทีที่สั่งเดิน** server จึงคิดว่าสัตว์ถึงที่หมายแล้ว
ตั้งแต่วินาทีแรก ทั้งที่ client ยังเดินอยู่ ⇒ ถ้ามีคำสั่งใหม่แทรกกลางทาง (โดนตี · เข้าระยะกัด · ตาย)
**จุดเริ่มของคำสั่งใหม่จะเป็นปลายทางเก่า** แล้ว client ที่เดินไปได้แค่ครึ่งทางจะ**กระโดดไปข้างหน้าทันที**

ฝั่ง client ยืนยันว่าเป็นแบบนี้จริง: `PathMovable.ApplyLocation()` **เซ็ตตำแหน่งตรง ๆ**
จาก path ที่ได้รับ ไม่มีการหน่วง/เกลี่ย — path บอกให้อยู่ตรงไหนก็เด้งไปตรงนั้นทันที

**แก้:** `ServerAnimal` เก็บ "เส้นทางที่กำลังเดิน" (`_from` `_to` `_moveStartAt` `_moveEndAt`)
แล้วคำนวณตำแหน่งจริงตามเวลา — `Position` กลายเป็นค่าที่คิดจากเวลา ไม่ใช่ค่าที่ตั้งไว้ล่วงหน้า
ทุกคำสั่งใหม่ (เดิน/ท่าโจมตี/ท่าตาย) จึงเริ่มจากจุดที่สัตว์อยู่จริง

ผลพลอยได้: **ระยะที่ใช้ตัดสินใจถูกต้องขึ้นด้วย** — เดิม server วัดระยะจาก "ปลายทาง"
ทำให้สัตว์ตัดสินใจกัด/เลิกไล่จากตำแหน่งที่ตัวเองยังไปไม่ถึง

### เดินเป็นขาสั้น ๆ

เดิมสั่งเดินทีเดียวได้ไกลถึง 2,500 หน่วย (สปีด 120 = เดินยาว 20 วินาทีรวดเดียว) ดูเป็นหุ่นยนต์
และถ้ามีอะไรมาขัดกลางทางก็เพี้ยนเยอะ — ตอนนี้ตัดเป็นขาละไม่เกิน 5 วินาที (`animals.MaxWalkLegSeconds`)
พร้อมย้าย `WalkSpeed` · เวลาพัก เข้า config ด้วย (พักสั้นลงเป็น 4-11 วิ จากเดิม 5-14)

### สัตว์เกิดเป็นโซน (`config.json` → `zones`)

เดิมสัตว์ทุกชนิดกระจายมั่วทั่วเกาะในรัศมี 30 tile — เดินไปทางไหนก็เจอเหมือนกันหมด ไม่มีอะไรให้จำ

ตอนนี้แต่ละชนิดมี**โซนที่อยู่อาศัย** เกิดในโซนและเดินอยู่ในโซนนั้น:

| โซน | ห่างจุดเข้าเกม | สัตว์ |
|---|---|---|
| ทุ่งหญ้าหน้าบ้าน | 0 tile (รัศมี 14) | กิ้งก่า · คอมป์โซ · โดโด |
| ชายป่า | +22,+6 (รัศมี 13) | เฟนาโค · โปรโตเซราท็อปส์ |
| ที่ราบสูง | −8,+24 (รัศมี 13) | พาราซอโร · สเตโก · ไทรเซรา |
| หุบแร็ปเตอร์ | +26,−22 (รัศมี 10) | โอวิแรปเตอร์ · แร็ปเตอร์ |

- จุดกึ่งกลางเป็น**ระยะห่างจากจุดเข้าเกม** ไม่ใช่พิกัดตายตัว → เอา config เดียวไปใช้กับเกาะไหนก็ได้
- `Zones` ว่าง = กระจายทั่วเกาะแบบเดิม
- ทำไมถึงสำคัญกับความสนุก: ผู้เล่นจำได้ว่า "ทางนั้นมีแร็ปเตอร์ ยังไปไม่ได้" — ความกลัวมีทิศทาง
  และวันที่กล้าเดินเข้าไปคือวันที่รู้สึกว่าตัวเองเก่งขึ้นจริง (ดู `docs/GOAL.md`)

ยืนยันแล้ว: ยืนที่จุดเข้าเกมเห็นแต่กิ้งก่า/คอมป์โซ/โดโด · ตัวใหญ่อยู่ห่างออกไป 13+ tile

### เทส

`--gp-check` **45/45** · `--multi-check` **9/9**
เพิ่มตัววัด "ระยะกระโดด" ของสัตว์ในตัวเทส (เทียบจุดเริ่มคำสั่งใหม่กับตำแหน่งที่ควรอยู่ตามคำสั่งเก่า)

⚠️ **ข้อควรรู้เรื่องเทสตัวนี้:** มันจะวัดได้เฉพาะตอนจับจังหวะ "สัตว์กำลังเดินอยู่ + อยู่ในระยะตี" ทัน
ถ้าจับไม่ทันจะขึ้น `[ข้าม]` แทนที่จะผ่านแบบหลอก — ตอนพัฒนาเจอว่าเวอร์ชันแรกของเทสนี้
**ผ่านทั้งที่ยังมีบั๊กอยู่** เพราะไม่มีคำสั่งใหม่เข้ามาให้วัดเลย จึงเพิ่มการนับคำสั่งไว้กันหลอกตัวเอง
**การยืนยันจริงต้องเล่นดูด้วยตา**

### เอกสารใหม่

- `docs/GOAL.md` — ทำเสร็จแล้วเกมมีระบบอะไร · เล่นไปทางไหน · สนุกตรงไหน 5 ข้อ · และ**จะไม่เป็นอะไร**
- `docs/ROADMAP.md` — beta 4 รอบ (B1 เกาะเดียว → B2 เพื่อน 5-10 → B3 หลายเกาะ → B4 ซ้อมเปิด) → เปิดจริง
  พร้อมเกณฑ์ผ่านของแต่ละรอบและกติกาหลังเปิดจริง

---

## ที่ยังค้าง

| # | เรื่อง | ความสำคัญ |
|---|---|---|
| GP-06 | แชทส่วนตัวตาย (`RadiotowerServer` ไม่มีใครต่อเพราะ `cluster_mode = SingleMode`) | กลาง |
| — | แล่เนื้อยังไม่ต้องใช้มีด และยังไม่มีสกิล butchery มาคูณจำนวน/ความเร็ว | ต่ำ |
| GP-09b | หน่วงเก็บของยัง 2.1 วินาทีตายตัว ไม่ได้ใช้ `Duration` ของ generator | ต่ำ |
| GP-08b | ยังไม่ตรวจ tag วัตถุดิบ (ต้องให้ไอเทมมี `Tags` ก่อน) | ต่ำ |
| — | ยังไม่มี: สัตว์/ต่อสู้ · เพ็ท · ฟาร์ม · คลัง · ปาร์ตี้/แคลน | เฟส C รอบ 2 |

## หมายเหตุสภาพแวดล้อม

`[webserver] wildcard bind denied (Access is denied.), falling back to loopback`

Gateway ผูกได้แค่ loopback → **เครื่องอื่นเข้าไม่ได้** เพราะ .NET 9 บน Windows ใช้ http.sys ที่ต้องมีสิทธิ์
(ต่างจาก Mono ในตัวเกมที่ไม่ต้อง) แก้ครั้งเดียวด้วย PowerShell as Administrator:

```powershell
netsh http add urlacl url=http://*:8190/ user=Everyone
```

หรือรัน server ด้วยสิทธิ์ Administrator ทุกครั้ง
