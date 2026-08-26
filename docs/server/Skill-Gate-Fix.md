# แก้บั๊ก: ระบบความสามารถ (skill) ไม่มีผลกับการคราฟ/สร้าง

**วันที่:** 25 ส.ค. 2026
**เจ้าของสั่ง:** "ไอเทมที่ยังไม่ปลดสกิล ต้องไม่แสดง แก้เลย" + "ไอเทมที่ปลดในสกิลได้แต่เราซ่อนไว้ เอากลับมาด้วย"
(หลังยืนยัน "สกิลไม่แสดงผลกับตัวเกม ท่าต่อสู้ ของบางอย่างไม่ต้องปลดสกิลก็คราฟได้")

## สาเหตุที่เจอ

`RecipeGateData.cs` (ข้อมูล "ต้องมีความสามารถเท่าไรถึงจะได้สูตรนี้" สกัดจากข้อมูลเกมจริง) **มีอยู่ในโค้ด
อยู่แล้ว** แต่ตรวจด้วย `grep -rn "RecipeGateData" server/` แล้วพบว่า**ไม่เคยถูกเรียกใช้ที่ไหนเลยในทั้ง
โปรเจกต์** นอกจากไฟล์นิยามตัวเอง — และมีแค่ 22 รายการ (เฉพาะ `reform_*`) จากที่ควรมีจริง 670+ รายการ
(สกัดไม่ครบตั้งแต่แรกด้วย)

ผลคือ: ตราบใดที่สูตร/แบบก่อสร้างอยู่ใน "unlocked set" ของผู้เล่น (ไม่ว่าจะมาจาก Starter list หรือ
`RecipeUnlockData.Collect`) **คราฟ/สร้างได้ทันทีถ้าวัตถุดิบ/โต๊ะ/เครื่องมือครบ** — ไม่เคยเช็คว่าผู้เล่นมี
"ความชำนาญ" (ability) พอจริงไหมเลย ระบบสกิลทั้งหมดจึงดูเหมือน "ไม่มีผล" กับการคราฟ

## ข้อมูลจริงจากเกม (แกะจาก APK มือถือ 5.2.1)

แต่ละสูตร/แบบก่อสร้างมี field `required_ability` (ตัวเลขอ้างถึง `Shared.Ability.Derived`) +
`required_ability_value` (สูตรคำนวณ เป็น `"N × level"` เสมอ — **level = เลเวลตัวละครตอนคราฟ ไม่ใช่
เลเวลของสูตรเอง**) ตัวอย่าง: ขวานเทียร์ 1 (`assembled_axe_one_01`) ต้องการ `Weaponcraft (210) ≥ 0.5 × เลเวล`

เจ้าของสังเกตเองว่า **ขวานเริ่มคราฟได้ตอนเลเวล 2 พอดี** — ใช้ข้อมูลนี้ตั้งสูตรค่าความสามารถพื้นฐาน (ยัง
ไม่ฝึกอะไรเลย) แบบย้อนกลับ: ต้องเป็น `(เลเวล − 1)` ถึงจะพอดี:
- Lv.1 → ค่า 0, เกณฑ์ 0.5×1=0.5 → **0 < 0.5 ยังคราฟไม่ได้** (ตรงกับที่เจ้าของเจอตอนแรก)
- Lv.2 → ค่า 1, เกณฑ์ 0.5×2=1.0 → **1 ≥ 1.0 คราฟได้พอดี** (ตรงกับที่เจ้าของยืนยัน)

ดึงข้อมูล `required_ability`/`required_ability_value` ครบทั้ง 720 สูตร (670 มีเงื่อนไขจริง) และ 570
แบบก่อสร้าง (549 มีเงื่อนไขจริง) แล้ว — ทุกสูตรเป็นรูปแบบ `"N × level"` หรือ `"level × N"` เหมือนกันหมด
ไม่มีรูปแบบแปลกปลอม

`required_ability` ที่เจอในสูตรคราฟมีแค่ 9 ค่า: `210`(Weaponcraft) `211`(Armorcraft) `215`(Tailor)
`216`(Smith) `217`(Cook) `218`(Furnishing) `219`(Construction) `220`(Farming) `239`(Handicraft)

## การแก้ (โครงเดียวกับ `AbilityValue(Basic)` ที่มีอยู่แล้วสำหรับค่าสถานะ 8 ตัว)

**ไฟล์ใหม่/แก้:**
- `server/ServerCore/RecipeGateData.cs` — เขียนใหม่ทั้งไฟล์ ครบ 670 สูตร (จาก 22 เดิม) + เพิ่ม
  `BlueprintGateData` (คลาสใหม่ในไฟล์เดียวกัน) ครบ 549 แบบก่อสร้าง
- `server/ServerCore/ServerPlayer.Abilities.cs` — เพิ่ม:
  - `CategoryForDerivedAbility(int)` — แมป Derived ability (210/211/...) → `Shared.Skill.Category`
    (Weaponcrafting/Armorcrafting/Cooking/Constructing/Farming/Process) ตัวที่ไม่มีหมวดตรงในเกมเรา
    (Smith/Tailor/Furnishing/Handicraft) ใช้หมวดใกล้เคียงที่สุด (ดูคอมเมนต์ในโค้ด)
  - `DerivedAbilityValue(int)` — `(เลเวล−1) + max(0, ความชำนาญหมวดที่แมป−1)` — ฝึกสกิลจริงแล้วปลดเร็ว
    กว่ารอเลเวลอย่างเดียวได้ (ตอบโจทย์ข้อ 2 ที่เจ้าของสั่ง: "ไอเทมที่ปลดในสกิลได้ เอากลับมาด้วย" —
    ความชำนาญที่โตจากการทำงานซ้ำ ๆ ยังมีผลจริงเหมือนเดิม แค่ตอนนี้ผลนั้น**มีความหมาย**แล้ว)
  - `MeetsRecipeGate(string)` / `MeetsBlueprintGate(string)` — เทียบค่าจริงกับเกณฑ์
- `server/ServerCore/ServerPlayer.Skills.cs` (`BuildUnlocked()`) — กรอง `recipes`/`blueprints` ท้ายสุด
  ด้วย `MeetsRecipeGate`/`MeetsBlueprintGate` — ตัวที่ไม่ผ่านเกณฑ์จะกลายเป็น **`Available=false`** ฝั่ง
  client ⇒ **ซ่อนอัตโนมัติผ่านกลไก `IsValidCategoryItem`/`HasValidCategoryItems` ที่มีอยู่แล้ว** ไม่ต้อง
  เพิ่ม CraftBlocker ใหม่หรือแก้ client เลย
- `server/ServerCore/ServerPlayer.Crafting.cs` (`HandleCraft`) — เพิ่มเช็ค `MeetsRecipeGate` (กันกรณี
  client เก่า/แก้ packet ส่ง RecipeId ตรงมา ไม่พึ่งแค่การซ่อนที่เมนู)
- `server/ServerCore/ServerPlayer.Building.cs` (`HandleOccupyArtifactSite`) — เพิ่มเช็ค
  `MeetsBlueprintGate` แบบเดียวกัน

## ผลลัพธ์ที่ควรเห็น

- ตัวละคร Lv.1 ใหม่: สูตรที่มีเกณฑ์ (เช่นขวาน 0.5×level) **จะไม่โผล่จนกว่าจะถึง Lv.2**
- ตัวละครที่ฝึกความชำนาญหมวดที่เกี่ยวข้อง (เช่นตีเหล็ก/ทำอาวุธซ้ำ ๆ จนหมวด Weaponcrafting ขึ้นเลเวล)
  จะเห็นสูตรเทียร์สูงกว่าเร็วขึ้น **โดยไม่ต้องรอเลเวลตัวละครอย่างเดียว** — นี่คือ "ไอเทมที่ปลดในสกิลได้"
  ที่เจ้าของอยากได้กลับมา
- สูตร/แบบก่อสร้างที่ไม่มีเกณฑ์ (ไม่อยู่ใน `RecipeGateData`/`BlueprintGateData`) ไม่กระทบ — ปลดล็อกตาม
  Starter list/skill unlock เหมือนเดิมทุกอย่าง

## บั๊กที่ 2 (เจอระหว่างเทสรอบแรก) — เมนูคราฟต์ไม่รีเฟรชหลังปลดล็อกใหม่

เจ้าของเทสแล้วเจอ: "สกิลสร้างมีดหิน ปลดออโต้แล้ว แต่เรายังสร้างไม่ได้" — เช็คเลขจริงแล้ว
(`DerivedAbilityValue`/`RecipeGateData`) สูตรนี้ควรผ่านเกณฑ์ที่เลเวลนั้นแล้วจริง ๆ ไม่ใช่บั๊กจากเกณฑ์ข้อ 1

**สาเหตุจริง**: `client/RecipeSystem.cs` ขอ `GetRecipes`/`GetArtifactBlueprints` **แค่ครั้งเดียวตอน
`OnReady()`** (ตอนเข้าเกม) แล้วเก็บผล `Available` ของแต่ละไอเทมไว้ใช้ทั้งเซสชัน — หน้าสกิลคำนวณ "AUTO"
สดใหม่ทุกครั้งที่เปิดดู (จากเลเวลปัจจุบันตรง ๆ ฝั่ง client) แต่เมนูคราฟต์/สร้างอ่านจาก snapshot เก่าที่
ไม่เคยอัพเดตอีกเลยหลังจากนั้น — ขึ้นเลเวล/ความชำนาญขึ้น/เรียนสกิลใหม่ทีหลัง จึงไม่มีทางเห็นของใหม่จนกว่า
จะออกจากเกมแล้วเข้าใหม่ทั้งเซสชัน (บั๊กเดิมอยู่ก่อนแล้ว แค่ไม่มีใครสังเกตเพราะก่อนหน้านี้ unlocked set
แทบไม่เคยเปลี่ยนกลางเซสชันเลย จนกระทั่งบั๊กที่ 1 ถูกแก้ให้ unlocked set เปลี่ยนจริงตามเลเวล/ความชำนาญ)

**การแก้**: เพิ่ม `SendUnlockedRecipesAndBlueprints()` (`ServerPlayer.Skills.cs`) — push
`Recipes`/`ArtifactBlueprints` ใหม่แบบเดียวกับ `SendSkills()` — เรียกที่ 3 จุดที่ทำให้ unlocked set
เปลี่ยนได้จริง:
- `ServerPlayer.Progress.cs` (`GainExp`) — ตอนขึ้นเลเวล
- `ServerPlayer.Proficiency.cs` — ตอนความชำนาญหมวดใดหมวดหนึ่งขึ้นเลเวล
- `ServerPlayer.Skills.cs` (`HandleTrainSkill`) — ตอนกดเรียนสกิลใหม่ (`RecipeUnlockData.Collect` ได้ผล
  ทันที)

ฝั่ง client ไม่ต้องแก้อะไรเลย — `OnRecipeListMsg`/`OnBlueprintListMsg` (`client/RecipeSystem.cs`) รับ
ข้อความ `Recipes`/`ArtifactBlueprints` แล้วอัพเดต `Available` list ให้อัตโนมัติอยู่แล้วทุกครั้งที่มีข้อความ
เข้ามาใหม่ (เดิมแค่ไม่เคยมีข้อความใหม่ส่งมาอีกหลัง login เท่านั้นเอง)

## รอบ 3 (25 ส.ค. 2026) — เลิกใช้สูตรที่ประมาณเอาเอง เปลี่ยนไปใช้ระบบสกิลจริงที่มีอยู่แล้ว

เจ้าของสั่งชัดเจน: **"รายการคราฟอ้างอิงจากสกิลเท่านั้น"** + เจอว่า "ไอเทม tool หลายอย่างไม่ต้องเรียนสกิล
ก็แสดงในรายการคราฟ"

ไล่โค้ดใหม่แล้วพบว่าโปรเจกต์นี้มีระบบสกิลจริงที่สกัดจากข้อมูลเกมอยู่แล้วครบ **แค่ไม่เคยถูกเสียบใช้เต็มที่**:

- `RecipeUnlockData.AlwaysRecipes`/`AlwaysBlueprints` — สูตร "ไม่มีสกิลไหนปลดล็อก = ได้ตั้งแต่แรก"
  (219 จาก 720 สูตร ตามคอมเมนต์หัวไฟล์ที่มีอยู่ก่อนแล้ว) — ของจริงจากเกม ไม่ใช่ลิสต์ที่คัดเอง
- `RecipeUnlockData.BySkill`/`Collect()` — ที่เหลือ 501 อันต้องเรียนสกิลนั้นถึงเลเวลนั้นจริง (มีอยู่แล้ว
  และ `BuildUnlocked()` เรียกอยู่แล้วในลูป `_knownSkills` — จุดนี้ไม่ได้พัง)
- `AutomaticSkillData.Nodes` + `EnsureAutomaticSkills()` — สกิลที่ปลดอัตโนมัติตามเลเวล**ความชำนาญ
  หมวด** (ไม่ใช่เลเวลตัวละคร) เช่นโหนด "AUTO" ที่เห็นในหน้าสกิล — มีอยู่แล้ว 71 โหนด ใช้งานได้จริง (เห็น
  log `[skill-auto] ... lv=2 (category 5)` ตอนเทส) แต่ `BuildUnlocked()` ไม่เคยเรียกมันเองมาก่อน (พึ่งพา
  `SendSkills()` เรียกให้เท่านั้น ซึ่งไม่ได้รันคู่กับทุกครั้งที่ `BuildUnlocked()` ทำงาน)

**ตัวที่ผิดจริง**: รอบก่อนหน้า (ทั้งรอบที่ใช้ `config.json Starter.Recipes` 34/12 อัน และรอบที่ผมเพิ่ม
`RecipeGateData`/`DerivedAbilityValue` เอง) **ไม่มีอันไหนอ้างอิงระบบสกิลจริงข้างบนนี้เลย** —
`Starter.Recipes` เป็นลิสต์คัดเองสมัยเบต้าที่ "ฟรีทั้งชุด" ไม่ผูกกับสกิล (ตรงกับที่เจ้าของเจอ: ไอเทม tool
โผล่โดยไม่ต้องเรียนสกิล) ส่วน `RecipeGateData` ที่ผมสร้างเองจากสูตร `required_ability × level` เป็นการ
ประมาณเอาเองจากข้อมูล item ไม่ใช่ระบบสกิลของเกมจริง เลยซ้อนทับ/ขัดกับของจริงที่มีอยู่แล้ว

**การแก้**: `BuildUnlocked()` (`ServerPlayer.Skills.cs`) เปลี่ยนฐานจาก `Starter.Recipes/Blueprints`
เป็น `AlwaysRecipes`/`AlwaysBlueprints` + เรียก `EnsureAutomaticSkills()` เองก่อนไล่ `_knownSkills` loop
(กัน AUTO-tier ตกหล่นตอนยังไม่เคยเรียก `SendSkills()`) + **เอา `MeetsRecipeGate`/`MeetsBlueprintGate`
ออกจากทุกจุด** (`BuildUnlocked`, `HandleCraft`, `HandleOccupyArtifactSite`) — แทนที่ด้วยการเช็ค
membership ตรงๆ ว่าสูตร/แบบก่อสร้างนั้นอยู่ใน `UnlockedRecipes()`/`UnlockedBlueprints()` จริงไหม (ของจริง
จากสกิล ไม่ใช่สูตรประมาณ) — โค้ด `RecipeGateData.cs`/`ServerPlayer.Abilities.cs` (`DerivedAbilityValue`
ฯลฯ) ยังอยู่ในโปรเจกต์เผื่ออ้างอิง แต่ไม่มีจุดไหนเรียกใช้แล้ว

**ผลลัพธ์**: รายการคราฟตอนนี้ = 219 สูตรฟรี + สูตรจากสกิลที่เรียน/auto-unlock จริง เท่านั้น — ไอเทม tool
ที่เคยโผล่โดยไม่ต้องเรียนสกิล (มาจาก `Starter.Recipes` 34 อันเดิม) จะหายไปจนกว่าจะเรียนสกิลที่เกี่ยวข้อง
จริง ตรงตามที่สั่ง

## ยังไม่ได้แตะ — ท่าต่อสู้ (combat moves)

เจ้าของยืนยันย้ำอีกครั้ง: **"ระบบต่อสู้ด้วยท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน"** — เช็คเบื้องต้นแล้วพบว่า
`AutomaticSkillData.Nodes` มีโหนดหมวดต่อสู้อยู่แล้ว (เช่น `kick`/`reckless` หมวด MeleeCombat) และตอนนี้
`EnsureAutomaticSkills()` ถูกเรียกสม่ำเสมอขึ้น (ทั้งจาก `SendSkills()` เดิม และจาก `BuildUnlocked()` ที่
เพิ่งเพิ่ม) น่าจะช่วยให้ `_knownSkills` ที่ส่งไปหา client ถูกต้องขึ้นแล้วในระดับหนึ่ง — แต่**ยังไม่ได้ไล่โค้ด
ฝั่ง client ว่าตัวเลือกท่าต่อสู้ที่ผู้เล่นกดใช้จริงในคอมแบตอ้างอิงจาก `_knownSkills` ถูกจุดไหมด้วย** เป็น
ระบบคนละส่วนกับ recipe/blueprint gate ยังไม่ได้ไล่โค้ดคอมแบตของ client เลย รอคิวถัดไป

## สถานะ

Build ผ่านทั้งหมด (0 error) เซิร์ฟทดสอบรีสตาร์ตแล้ว ยังไม่ได้ deploy ขึ้นเซิร์ฟบ้าน/อัป client release

## รอบ 4 (25 ส.ค. 2026) — เจอบั๊กจริงของ "ทำไมบังคับสร้างตัวใหม่" + แก้ของอีเวนต์/ระบบที่ตกหล่นเพิ่ม

### "ทำไมบังคับสร้างตัวใหม่" — เจอสาเหตุจริงแล้ว
`Gateway.cs` (`POST /accounts`) hardcode `player_slot_count: 7` เสมอ โดยไม่สนจำนวนตัวละครจริง —
เช็คฝั่ง client (`PlayerSelectionSystem.cs`) แล้วพบว่า `player_slot_count` **ไม่ใช่** "จำนวนตัวละครที่มี"
แต่คือ "จำนวนช่องที่ account ได้รับ" ใช้เทียบกับจำนวนจริงที่ส่งมา (`PlayerSlotExceeded = PlayerSlotCount
< size`) — ระหว่างพัฒนา/เทสสะสม account ไว้จาก IP เดียว (127.0.0.1) มากถึง **80 อัน** (ทดสอบมาหลายเดือน)
ทำให้ `7 < 80` กลายเป็น "เกินโควตา" เสมอ ⇒ หน้าเลือกตัวละครพังหรือ client fallback ไปสร้างใหม่แทน

**แก้:**
- `AccountStore.FindByIp()` — เรียงผลลัพธ์ตาม `LastSeenAt` ล่าสุดก่อน (เดิมสุ่มตามลำดับไฟล์)
- `Gateway.cs` (`/accounts`) — ใช้ค่าจริงจากเกม (`client/Durango.Offline/Server.cs`): `player_slot_count
  = 3` (MultiMode), `max_player_slot_count = 7` (Editable/dev) แทนที่ hardcode 7/7 เดิม + **ตัดรายการที่
  ส่งกลับให้เหลือแค่ 3 ตัวล่าสุด** แทนที่จะส่งทั้ง 80 อัน

**ยืนยันแล้วด้วยภาพจริง**: หน้า "Select Character" โผล่ถูกต้อง (3/3), เลือกตัวแรกแล้วเข้าเกมได้ตัวละคร
เดิม "desgvz" Lv.4 HP 56/115 ตรงกับสถานะก่อนหน้าเป๊ะ (ไม่ใช่ตัวใหม่)

หมายเหตุ: การ์ดเลือกตัวละครยังโชว์ "Lv. 0"/"Unknown" (ไม่ใช่ชื่อ/เลเวลจริง) — endpoint ส่ง
`player_name`/`player_level` ถูกต้องแล้ว (ยืนยันด้วย curl) แต่ UI การ์ดอาจต้องการฟิลด์อื่นเพิ่ม (ภาพตัวละคร/
ข้อมูลอื่น) ที่เรายังไม่ได้ส่ง — ไม่กระทบการทำงานจริง (เลือกได้ถูกตัว) แค่ตัวเลขบนการ์ดโชว์ไม่ตรง ยังไม่ได้ไล่

### ของอีเวนต์ยังหลุดเพิ่ม — แก้ IsEventRecipeCategory ที่จับได้แค่ category
`AlwaysRecipes` (219 อัน) มีของอีเวนต์จริง 24 อัน (santa/halloween/valentine/newyear2019) ที่ Category
เป็น "cook"/"weapon_and_tool" ปกติเป๊ะ — `IsEventRecipeCategory` (เช็คแค่หมวด) จับได้แค่ 7/24 — เปลี่ยนเป็น
`IsEventRecipe(id, category)` เช็คทั้งหมวดและชื่อ id (pattern เดียวกับ blueprint) ยืนยันจับครบ 24/24 แล้ว

### พร็อพระบบ (39 อัน) หลุดเข้าเมนูสร้างด้วย — เจอจากภาพจริง
เจ้าของกด "Storage" ในเมนู หน้ารายละเอียดขึ้นเองว่า **"(System Building: Player cannot build)"** แต่ยัง
กดสร้างได้ — เพิ่ม `RecipeData.IsSystemOnlyBlueprint` (39 ไอดีตรงจาก field `description` ของข้อมูลเกมจริง
เช่น camp_radio_station/camp_warehouse/camp_square_fire) กรองออกจาก unlocked set **ทุกคนรวม admin**
(คนละแบบกับของอีเวนต์ที่ admin ยังใช้ได้ — พร็อพนี้ไม่มีใครควรสร้างเองได้เลยตามดีไซน์เกม)

### เช็คแล้ว: "คราฟได้โดยไม่ใช้วัตถุดิบ" ไม่ใช่ปัญหาฝั่งสูตร
เจ้าของตั้งข้อสังเกตว่าของที่ไม่มีสูตร/คราฟฟรีคือของ admin — เช็ค `RecipeRequirements.cs` กับ
`AlwaysRecipes` ทั้ง 219 อันแล้ว **ทุกอันมีวัตถุดิบจริงกำกับอยู่ครบ (ไม่มีอันไหน total min = 0)** — ฝั่ง
สูตรคราฟไม่มีปัญหานี้ ส่วนฝั่งแบบก่อสร้าง (blueprint) ระบบวางเองยังไม่หักวัตถุดิบเลยในทุกกรณี (ข้อจำกัด
beta เดิม แยกเรื่องจาก "ของ admin") — ตัวชี้วัดที่แม่นสำหรับ blueprint คือ flag "player cannot build"
ข้างบนนี้ ไม่ใช่การนับวัตถุดิบ

## รอบ 5 (25 ส.ค. 2026) — ไล่เช็คทุกแท็บในเมนูจริง เจอของอีเวนต์หลุดเพิ่มอีก (s02_* ทั้งชุด)

เจ้าของขอดูรายการคราฟทีละแท็บ — ไล่เช็คแล้วเจอ:

### "Specialty Crafting" (`recipe_book` category) ยังมี "Communication Center" โผล่สว่างอยู่
`camp_radio_station_02`/`statue_03_a` (ทั้งคู่ category จริง = `recipe_book`) ชื่อไม่มี pattern ไหนใน
`EventNamePatterns` ตรงเข้าเลยสักคำ (ไม่ใช่ xmas/santa/valentine/ฯลฯ) — หลุดผ่านการเช็คได้ ไล่ดูข้อมูลจริง
เจอว่าของ `recipe_book`/`constructing_season2` อีก 20 อันก็ใช้ prefix **`s02_`** ทั้งหมด (เช่น
`s02_flag_bohnanza_01`, `s02_shelter_01`) ซึ่งไม่ตรง pattern "season2" ที่มีอยู่แล้ว (ไม่ใช่ substring
เดียวกัน) — เพิ่ม `"s02_"` เข้า `EventNamePatterns` (จับเพิ่มได้ 20/22) + เพิ่ม `camp_radio_station_02`/
`statue_03_a` เป็น exact-match exception อีก 2 ตัวที่เหลือ (ชื่อไม่ส่อเลยจริงๆ)

**ผลพลอยได้**: pattern `s02_` ตัวนี้ใช้ร่วมกันทั้งฝั่งสูตรคราฟ (`IsEventRecipe`) กับแบบก่อสร้าง — เจอว่า
`AlwaysRecipes` มีสูตร `s02_*` หลุดอยู่อีก **17 อัน** (s02_bag_plastic, s02_clothes_rubber, s02_doll ฯลฯ)
ที่ไม่เคยถูกจับมาก่อนเลยเช่นกัน แก้พร้อมกันในรอบเดียว

### "System/Cheat" (`system` category) — ของจริงในเกม ไม่ใช่ตัวช่วยที่เราใส่เอง
เจ้าของอาจสังเกตว่ามีแท็บชื่อ "System/Cheat" ในเมนู — ตรวจแล้วเป็นชื่อหมวดจริงจากข้อมูลเกม NEXON เอง
(`#recipe_category_system` แปลเป็น "시스템/치트" ในภาษาเกาหลี) แต่เนื้อหาจริงข้างในเป็นแค่สูตรย้อม/ฟอกสี
เสื้อผ้า 6 อัน (`dye_color_*`/`bleach_color_*`) ไม่ใช่คำสั่งโกงจริง — คงไว้ตามเดิม (เป็นเนื้อหาจริงที่มากับ
เกม ไม่ใช่ debug tool ของเรา) แต่ชื่อแท็บอาจดูแปลกเพราะทับศัพท์เกาหลีตรงตัว — ยังไม่ได้แก้ชื่อ/ซ่อน
รอเจ้าของสั่งว่าจะให้ทำยังไง (ซ่อนทั้งแท็บ / เปลี่ยนชื่อ / ปล่อยไว้ตามเดิม)

### "Other" แท็บ — เจอ "Universal Workbench (Cheat)" โผล่ตรงๆ
เจ้าของกด "Other" เจอ **"Universal Workbench (Cheat)"** เป็นตัวเลือกจริง — เช็คข้อมูลเกมพบว่าชื่อเต็ม
คือ "만능작업대(치트용)" = "โต๊ะสารพัดประโยชน์ (ไว้ใช้โกง)" ตัวบ่งชี้อยู่ที่ field **`subcategory`**
(ไม่ใช่ `category` หลักที่เคยเช็ค) = `"system"` — เป็นสัญญาณอีกแบบที่ยังไม่เคยตรวจมาก่อน ไล่เช็คทั้งชุด
เจออีก 3 อันที่ subcategory เดียวกัน (`package`, `tutorial_boat`, `tutorial_bonfire` — ของช่วงสอนเล่น)
เพิ่มทั้ง 4 เข้า `SystemOnlyBlueprints` แล้ว — เช็คฝั่งสูตรคราฟด้วย (subcategory="system") เจอแค่ 6 อันเดิม
(dye/bleach) ไม่มีอะไรใหม่

### ยังไม่ยืนยัน — "Warp Gate Silo" (`warp_sailo`) กับ "Drop-off Point"
ยังโผล่สว่างอยู่ใน "Other" — เช็คแล้วไม่อยู่ในไฟล์ข้อมูล blueprint category ที่แกะมา (จาก 570 ทั้งหมด
แกะได้แค่ 556 อัน อีก 14 อันไม่มีข้อมูล category ให้เช็ค รวมสองอันนี้ด้วย) — จากบริบทก่อนหน้าในเซสชันนี้
("จุดรับส่ง"/"หลุมวาร์ป" เป็นสิ่งปลูกสร้าง POI ที่ซ่อมได้ตามเควส) อาจเป็นของจริงที่ผู้เล่นควรใช้ได้ ไม่ใช่
ของระบบ — **ไม่กล้าฟันธงโดยไม่มีข้อมูลยืนยัน** รอเจ้าของบอกว่าเคยเจอ 2 อันนี้ในบริบทไหน (ซ่อม POI vs
สร้างเองในเมนู) ถึงจะสรุปได้ว่าควรกรองออกไหม

### สั่งแล้ว (25 ส.ค. 2026)
- **"System/Cheat" แท็บ**: เจ้าของสั่งซ่อนทั้งแท็บ ให้ admin เท่านั้น (แม้เนื้อหาจริงจะเป็นแค่สูตรย้อม/
  ฟอกสี ไม่ใช่คำสั่งโกงจริง) — เพิ่ม `RecipeData.IsSystemRecipeCategory` (category == "system") กรองใน
  `BuildUnlocked()` และเช็คซ้ำใน `HandleCraft` เหมือนของอีเวนต์
- **"Warp Gate Silo" / "Drop-off Point"**: เจ้าของยืนยันว่าเป็นของที่ซ่อมได้ตามเควส ไม่ใช่ของระบบ —
  **คงไว้ตามเดิม ไม่กรอง**

---

## รอบ 6 — ท่าต่อสู้ยึดจากสกิลที่เรียนจริง (25 ส.ค. 2026)

เจ้าของย้ำ 2 รอบว่า **"ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน"** — ก่อนหน้านี้ได้ทำแค่ฝั่งสูตรคราฟต์/แบบก่อสร้าง
(recipe/blueprint gating) แต่ฝั่งต่อสู้ (combat moves) ยังไม่ได้แตะเลย

### ปัญหา

`HandleUseBattleAction` (`ServerPlayer.Combat.cs`) ตรวจแค่:
1. Feature flag (Combat เปิดอยู่ไหม)
2. ตายอยู่ไหม
3. ท่ามีจริงในเกมไหม (`ActionData.TryGet`)
4. **ท่าเป็นของอาวุธที่ถืออยู่ไหม** (`ActionData.ForWeaponTag` — ตรวจแค่ tag อาวุธ)
5. คูลดาวน์ / เป้าหมาย / ระยะ / PvP / สตามินา / ลูกธนู

**ไม่มีการเช็ค `_knownSkills` เลย** — modded client ส่ง `UseBattleAction { ActionId = "onehand_smash" }`
ได้โดยไม่ต้องเรียนสกิล `onehanded_smash` เลย เพราะผ่านการตรวจ tag อาวุธแล้ว (ถือดาบ = ท่านี้เป็นท่าของดาบ)
`HandleGetActions` ก็ส่งครบทุกท่าของอาวุธ ไม่กรองตามสกิลที่เรียน

### สาเหตุ: เซิร์ฟไม่มีข้อมูล "สกิลไหนปลดท่าต่อสู้อะไร"

ข้อมูลนี้มีอยู่ใน client (`Yaml/Reward.cs` → `ActionIds` + `RewardType.Action=8`) แต่ไม่เคยถูกสกัดมาใช้ฝั่งเซิร์ฟ
เซิร์ฟมีแค่ `ActionData.cs` (สถิติท่า 60 ตัว + อาวุธ→ท่า mapping) ไม่มี field บอกว่าท่านี้ต้องเรียนสกิลอะไร

### วิธีแก้ — สกัดข้อมูล + เพิ่ม skill gate (เหมือน `RecipeUnlockData`)

**1. สกัดข้อมูล** — สคริปต์ใหม่ `server/scripts/extract_action_unlocks.py` (เลียนแบบ
`extract_recipe_unlocks.py`) อ่าน `resources.strings.txt` จาก `skills` + `rewards` TextAsset:
- `skills`: โครงสร้าง `{ หมวด: { skillId: { subId: [ {rewards: [...]}, ... ] } } }` index = เลเวล
- `rewards`: `{ rewardId: { type: 8, action_ids: [...] } }` — type=8 (Action) = ปลดล็อกท่าต่อสู้

ผลลัพธ์ `server/ServerCore/ActionUnlockData.cs`:
- `AlwaysActions` — 27 ท่าพื้นฐาน (ทุก `*_default_*` — ท่าตีปกติที่ไม่ต้องเรียนสกิล)
- `BySkill` — 14 สกิลที่ให้ท่า 32 ตัว (kick→barehand_kick_a/b, reckless→barehand_smash/combo,
  dodge→*_dodge, onehanded_smash→onehand_smash/axe/blunt, aimed_shot→ranged_*_aimedshot ฯลฯ)
- `Collect(skillId, subId, level, HashSet<string>)` — สะสมท่าตามเลเวล (เหมือน `RecipeUnlockData.Collect`)

**2. เพิ่ม helper** — `ServerPlayer.Skills.cs`:
- `UnlockedActions()` — คืน `HashSet<string>` ของท่าที่ปลดแล้ว (AlwaysActions + Collect จาก `_knownSkills`)
- `IsActionUnlocked(actionId)` — เช็ค membership แบบเดียวกับ `UnlockedRecipes`

**3. เพิ่ม skill check** — `ServerPlayer.Combat.cs::HandleUseBattleAction`:
หลัง weapon-tag check (บรรทัดเดิม) ก่อน cooldown/target check:
```csharp
if (!IsActionUnlocked(action.Id))
{
    Send(new Info { Text = "ต้องเรียนสกิลก่อนจึงจะใช้ท่านี้ได้" }, header.Seq);
    Send(default(Abort), header.Seq);
    return;
}
```

**4. กรอง `HandleGetActions`** — ส่งเฉพาะท่าที่ `UnlockedActions()` มี (เดิมส่งครบทุกท่าของอาวุธ)

### ผลทดสอบ

`--combat-skill-check` (`test-client/CombatSkillCheck.cs`) 13 ข้อ:
- ผู้เล่นใหม่ (มือเปล่า) ได้ 6 ท่า: defaults (2) + auto-grant kick/reckless/dodge (4)
- ไม่เห็น `barehand_combination` (ต้อง reckless lv2) และ `melee_tackle` (ต้องเรียน tackle)
- สั่งใช้ `barehand_combination` → Abort (ปฏิเสธเพราะยังไม่ได้เรียนสกิล)
- หลัง `maxskills`: เห็นครบ 8 ท่า + ผ่าน skill check (ตกที่ target check แทน — คนละเหตุผล)

Regression: `--gp-check` 45/45 · `--skill-check` 13/13 — ผ่านครบ

### ยังไม่ได้ทำ (รอบถัดไป)
- **Gathering/Hunting/Farming skill-gating**: ยังไม่ได้ตรวจว่าระบบเก็บของ/แล่เนื้อ/ปลูกผัก
  ตรวจสกิลที่เรียนจริงไหม (เหมือนที่ combat พึ่งทำเสร็จ) — เจ้าของขอให้ไล่ตรวจตามแนวทางเดียวกัน
