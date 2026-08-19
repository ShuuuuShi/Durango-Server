# HANDOFF — Durango Claude

**อัปเดตล่าสุด:** 19 ส.ค. 2026 — ระบบตัวละครรอบเก็บงาน

- เพิ่ม feature gates, PvP Lv.20+, durability อาวุธ/เกราะ, repair kit และโทษตาย durability 10%
- เพิ่ม skill category research พร้อม cancel/skip/timer/save, inventory order/lock/save, equipment Slot1-3, accessory และ death point/immediate revive
- เพิ่ม `--character-check` ผ่าน 17/17 และ `--smoke-check`; regression ผ่าน Group2 20/20, Cooking 11/11, Skill 13/13, Stats 19/19, Stamina 16/16, Multi 9/9
- deploy Linux ที่ `192.168.1.34` แล้ว: PID `1323227`, binary `server-character-20260819`, backup `backups/saves-before-character-20260819.tar.gz`
- เช็กลิสต์ UI ภาษาไทย: `TEST_CHECKLIST_TH.md`

---

**อัปเดตก่อนหน้า:** 18 ส.ค. 2026 (รอบ 8) — beta 1.0 เกาะเลเวล 1-10 · **เปิดระบบทำอาหารแล้ว**

**รอบ 8 — ระบบทำอาหาร (ตามที่เจ้าของสั่ง: เควส · ทำอาหาร · เพาะปลูก · ปศุสัตว์ · สิทธิ์ที่ดิน)**
- ✨ **ทำอาหารได้จริง** — 152 สูตร · ต้องยืนที่กองไฟ/เตาที่แรงพอ + ถือเครื่องมือที่ถูกชนิด
  ของดิบให้พลัง 60% และทำให้ล้าเพิ่ม ⇒ เนื้อดิบ +18.9 · ย่าง +31.5 · ต้ม +40 · มีเวลาย่อยกินรัวไม่ได้
- 🐛 **เจอบั๊กใหญ่ระหว่างทาง: `AppearArtifact.Tags` เป็น null เสมอ** ⇒ **587 จาก 720 สูตร
  (ทุกสูตรที่ต้องใช้โต๊ะ) กดไม่ได้มาตลอด** โดยไม่มี error ให้เห็น — แก้แล้ว
- 🐛 วางของจากแคปซูลออกมาเป็น "แค่จองพื้นที่" ⇒ กองไฟที่วางใช้เป็นโต๊ะคราฟต์ไม่ได้ — แก้แล้ว
- 🐛 คราฟต์อะไรก็ 2 วิ / 4 สตามินาเท่ากันหมด · สูตรที่ควรได้ 2 ชิ้นได้ชิ้นเดียว · `min_level` ไม่มีผล — แก้ครบ
- 🧪 เพิ่ม `--recipe-check` (server) + `--cook-check` (test-client) · **ผ่าน 11/11**
- ⏭️ **เหลือของรอบนี้: เพาะปลูก → ปศุสัตว์ → สิทธิ์ที่ดิน → เควส** (ดู docs/server/Cooking.md เป็นแบบอย่าง)

**แก้จากการเล่นจริงรอบล่าสุด 3 ข้อ** (ดู CHANGELOG หัวข้อ "รอบ 4")
- 🐛 **คลิกสัตว์แล้วปุ่มโจมตีไม่เด้ง** — `HandleTouch` ไม่มีเคสของสัตว์เลย เมนูเลยว่าง → เพิ่ม `HandleTouchAnimal()`
- 🐛 **สัตว์สวนกลับช้า** — ตอนโดนตีมันติด "เวลาพัก" ได้ถึง 14 วิ + ครั้งแรกใช้คูลดาวน์เต็ม
  → ล้างเวลาพักทันที + สวนกลับใน 0.5 วิ + คูลดาวน์ใช้ `attack_cooltime` จริงของแต่ละชนิด (1.3-3.0 วิ)
- ✨ **แล่เนื้อได้แล้ว** — `ButcheryData.cs` (เนื้อ/หนัง/กระดูก/เขา/ขนนก) · ซากอยู่ 150 วิ · ซากเรืองแสงให้คนที่ฆ่า
- 🐛 **แถม (เจอตอนเทส):** เปิดเซิร์ฟซ้ำสองตัวแล้ว Windows ยอมให้ฟังพอร์ต 8191 พร้อมกันทั้งคู่
  ⇒ ผู้เล่นครึ่งหนึ่งไปโผล่อีกโลก คนละไฟล์เซฟ · แก้ด้วย `ExclusiveAddressUse` ตอนนี้ตัวที่สองปิดตัวเองแล้ว

**รอบ 5-6 (ขยายขอบเขต beta ตามที่เจ้าของสั่ง)**
- ✨ **ระบบเลเวล** — ตารางเลเวลค่าจริงของเกม (`LevelData`) · ได้ exp จากล่า/เก็บ/แล่/คราฟต์/สร้าง
  · ขึ้นเลเวลได้แต้มสกิล · exp เป็นตัวจริง เลเวลคิดใหม่จาก exp ตอนโหลดเซฟ
- ✨ **`data/config.json`** — เรทเกิดสัตว์/สมดุล/exp แก้ได้โดยไม่ต้อง build (hot-reload 5 วิ) · เมนูข้อ 17
- ✨ **เครื่องมือของจริง (ปิด GP-08b)** — ไอเทมมี `Tags` จริงจากข้อมูลเกม 1,904 ชนิด
  ตัดไม้ต้องมีขวาน · แล่ซากต้องมีมีด · ทุบหิน/แร่ต้องมีอีเต้อ · คราฟต์เช็ค tag+วัสดุตามสูตรจริง
- ⏸️ **ระบบอาชีพ** — เจ้าของสั่งพักไว้ก่อน (เกมนี้ไม่มีระบบอาชีพในตัว ต้องออกแบบเองทั้งหมด)
- ✅ **สกิลมีผลจริงแล้ว** — เก็บของ/แล่เร็วขึ้น+ได้ของเพิ่ม · ดาเมจ · ป้องกัน · คราฟต์ · สตามินา
  (เรียนสกิลเลเวล N = N แต้ม · ต้องมีเลเวลผู้เล่นถึง · ผู้เล่นใหม่เริ่ม 0 แต้ม)
- 🚧 **เกาะแยกเลเวล — ครึ่งทาง** โครงสร้างพร้อม (`--island isle01/isle02`, ตัวละครข้ามเกาะได้จริง)
  แต่ **เดินทางจากในเกมยังไม่ได้** (client ฮาร์ดโค้ด gateway 8190) + มีบั๊กค้าง 1 ข้อ → `docs/server/Islands.md`
  ⚠️ โหมดเกาะเป็นของเสริม ต้องสั่ง `--island` เอง — beta 1.0 ที่เปิดจริงยังเป็นเกาะเดียวและผ่าน 45/45

สถานะ: ปิดบั๊กกันโกงในลิสต์ครบทุกข้อแล้ว (H-1…H-8 · M-1…M-7 · GP-08/09/12/14)
- `--gp-check` ผ่าน **45/45** · `--multi-check` (3 คนพร้อมกัน) ผ่าน **9/9** · `--cook-check` ผ่าน **11/11**
- โซกเทสบอทฟาร์ม 3 ตัว 30 นาที: **120 tps คงที่ · RAM ~58-65 MB · exception 0**
- สัตว์ 34 ตัวจาก 10 ชนิดเกิดในโลกจริง **เรนเดอร์+เล่นอนิเมชันในตัวเกมได้แล้ว**
  (ยืน/เดิน/วิ่ง/โจมตี/ตาย · หันหน้าถูก · ตายแล้วค้างท่าไม่วนซ้ำ)
- ซ่อนเมนูของระบบที่ยังไม่ได้ทำ 24 เมนู · ปิดบทสนทนา NPC ด้วย `RegionRole=Sandbox`

**อ่านก่อนวางแผน:** [docs/GOAL.md](docs/GOAL.md) (เกมนี้จะเป็นอะไร) · [docs/ROADMAP.md](docs/ROADMAP.md) (beta 4 รอบ → เปิดจริง)
**ก่อนเทส:** [docs/TESTPLAN.md](docs/TESTPLAN.md) — รายการเทสทั้งหมด (อัตโนมัติ + เช็คลิสต์เล่นจริง + เกณฑ์ผ่าน)

**เหลือก่อนเปิดจริง:** เล่นด้วยตัวเกมจริง 30 นาทีเป็นรอบสุดท้าย (เกณฑ์ข้อ 3 ใน
[docs/BETA-1.0-PLAN.md](docs/BETA-1.0-PLAN.md) §4) แล้วเปิดได้เลย

**คำสั่งเปิดเซิร์ฟสำหรับ beta:**
```bash
cd "C:\Users\thana\Desktop\Durango Claude\server"
dotnet run -- --whitelist data/whitelist.txt
#   cheat ปิดอยู่ (ไม่ใส่ --enable-cheat) · radiotower ปิดอยู่ (M-5) · ผูก entity id กับ IP แรกที่จอง
```

---

## 1. โครงสร้างโปรเจกต์

```
C:\Users\thana\Desktop\Durango Claude\
├── game/          ตัวเกม Unity 2017.4.34f1 (1.1 GB, dll patch แล้ว)  ← รันด้วย launch.bat
├── game-backup/   สำเนา dll ก่อน patch
├── server/        DurangoServer (.NET 9)  ← ตัวหลักที่เราเขียนเอง
├── client/        3,760 ไฟล์ .cs จาก ILSpy — คอมไพล์ผ่าน 0 error
├── test-client/   ตัวทดสอบ headless + FarmBot
└── docs/          README · ARCHITECTURE · CHANGELOG · discord-update
                   docs/server/ (18 ไฟล์) · docs/client/ (139 หน้า)
```

### วิธีเทสแบบคลิกเดียว 🖱️

ดับเบิลคลิก **`เทสเกม.bat`** ที่โฟลเดอร์หลัก — เมนูภาษาไทย เลือกข้อเอาได้เลย

| ข้อ | ทำอะไร |
|---|---|
| 1 | build → เปิดเซิร์ฟ (cheat เปิด) → เปิดเกม → ต่อ 127.0.0.1 ให้ → โชว์เช็คลิสต์ |
| 2 / 3 | เปิดเฉพาะเซิร์ฟ / เฉพาะเกม |
| 4 | เช็คลิสต์ 30 นาที ว่าต้องดูอะไรบ้าง |
| **5** | **กล่องเครื่องมือตอนเล่น** — เสกสัตว์ตรงหน้า/ฆ่าให้ได้ซาก/เติมเลือด/เสกของ/วาร์ป (ผ่าน `control`) |
| 6 / 7 / 8 / 9 | `--gp-check` 45 ข้อ · `--multi-check` · บอทฟาร์ม 5 นาที · โซกเทส 30 นาที (บอท 3 ตัว) |
| **20 / 21 / 22** | **เทสระบบทำอาหาร 11 ข้อ (`--cook-check`) · ตรวจข้อมูลสูตร/อาหาร (`--recipe-check`) · เช็คลิสต์ทำอาหาร** |
| 10-16 | บอทคอนโซล · ดูใครออนไลน์ · สำรองเซฟ .zip · รีเซ็ตโลก · whitelist · log เกม · เปิดโฟลเดอร์ |
| 17-19 | ตั้งค่าเซิร์ฟ (เรทเกิด/สมดุล/exp) · **build ตัวเกมจากซอร์ส** · ย้อน DLL |
| 88 / 99 | ปิดเซิร์ฟ · เปิดโหมดเปิดจริง (whitelist, cheat ปิด) |

เมนูจัดการกับดักให้หมดแล้ว: kill เซิร์ฟก่อน build · กันเปิดเซิร์ฟ/เกมซ้อน ·
ถ้าเผลอเปิดโหมดเปิดจริงค้างไว้แล้วมากดเทส มันจะปิดแล้วเปิดใหม่ให้เอง (cheat ปิด = เทสตกยกแผง)
โหมดเทสเปิดด้วย `--enable-cheat --admin gm` เสมอ เพราะกล่องเครื่องมือ (ข้อ 5) สั่งผ่านบอทชื่อ `gm`

เอกสารสำหรับ**ผู้ทดสอบ/ผู้เล่น** (ว่าจะได้เจออะไรบ้าง): `docs/BETA-1.0-PLAYERS.html` — เปิดในเบราว์เซอร์ได้เลย

**ข้อ 17 = ตั้งค่าเซิร์ฟ** — เรทเกิดสัตว์ · สมดุล · exp อยู่ใน `server/data/config.json`
แก้แล้วมีผลใน 5 วินาทีโดยไม่ต้อง build/รีสตาร์ท (ตารางสัตว์ต้องเปิดเซิร์ฟใหม่) ดู `docs/server/Config.md`
**ข้อมูลผู้เล่นเก็บเป็นไฟล์ JSON** ที่ `server/saves/players/<id>.json` (ไม่มี database engine)

> ตัวเมนูอยู่ที่ `tools/menu.ps1` — **ต้องเซฟเป็น UTF-8 มี BOM** ไม่งั้น PowerShell 5.1
> อ่านเป็น ANSI แล้วภาษาไทยเละทั้งไฟล์ · ส่วนไฟล์ .bat ต้องเป็น ASCII ล้วน (cmd อ่านเป็น CP874)

### แก้ตัวเกม: build จากซอร์สได้เลย (ไม่ต้อง patch แล้ว) 🔧

`client/` (3,760 ไฟล์จาก ILSpy) **คอมไพล์ผ่านและเกมรันได้จริง** — เทสแล้วบูตขึ้น + เข้าเซิร์ฟได้

```bash
เทสเกม.bat → ข้อ 18       # build จากซอร์ส + วางลงเกม (สำรองของเดิมให้อัตโนมัติ)
เทสเกม.bat → ข้อ 19       # ย้อน DLL กลับอันก่อนหน้า
powershell -File "tools\build-client.ps1" [-Restore] [-NoInstall]
```

patch เดิมทั้งหมดย้ายเข้าซอร์สแล้ว (autoconnect · ซ่อนเมนู 24 อัน · สัตว์โผล่ · self-IP filter ·
`ConnectTo` รับ `ip:port`) — หาจุดที่แก้เองด้วย `grep -rn "\[แก้เอง\]" client/`
`tools/DllPatcher` ยังเก็บไว้แต่ปกติไม่ต้องใช้แล้ว · รายละเอียด: `docs/client/BUILD.md`

⚠️ ปิดเกมก่อน build (DLL ล็อก) · อย่าเปลี่ยนชื่อคลาส/ฟิลด์ที่ Unity ใช้ (prefab จะหลุด)

### คำสั่งที่ใช้บ่อย

```bash
# เปิด server (ต้องปิด process เดิมก่อน ไม่งั้นไฟล์ล็อก MSB3021)
cd "C:\Users\thana\Desktop\Durango Claude\server" && dotnet run

# เปิดเกม  (ห้ามใช้ Start-Process -ArgumentList เพราะไม่ใส่ quote ให้ path ที่มีช่องว่าง)
cmd /c "C:\Users\thana\Desktop\Durango Claude\game\launch.bat"

# เปิดเซิร์ฟแบบ "พร้อมให้คนอื่นเข้า" (ปิด cheat, เฉพาะคนในรายชื่อ)
cd "C:\Users\thana\Desktop\Durango Claude\server" && dotnet run -- --whitelist data/whitelist.txt

# เปิดเกม + ต่อ 127.0.0.1 อัตโนมัติ (คลิกเดียวคือปุ่ม "เริ่ม" ที่เหลือ client ต่อเอง)
powershell -File "C:\Users\thana\Desktop\Durango Claude\tools\connect-game.ps1"
#   ⚠️ ห้ามเปิดเกมซ้อน 2 ตัว — client มี server ในตัว (พอร์ต 8390/8391) ตัวที่สองจะเด้งทันที

# รัน bot ฟาร์มเอง 5 นาที (พารามิเตอร์ที่ 5 = พอร์ต gateway ไม่ใส่ = พอร์ตเกม-1)
cd "C:\Users\thana\Desktop\Durango Claude\test-client"
dotnet run --no-build -- --bot 127.0.0.1 8191 5 farmbot-1

# เทสว่า server ปฏิเสธ packet โกงจริงไหม (45 ข้อ: GP-08/09/12/14 · H-5/6/7 · M-2/4/7 ·
#   ทิ้งของ/กินของ · แตะสัตว์/แล่เนื้อ)  ⚠️ ต้องเปิด server ด้วย --enable-cheat
dotnet run --no-build -- --gp-check

# เทส 3 คนออนพร้อมกัน + แย่งเก็บของจุดเดียวกัน (ดูว่าของไม่ถูกปั๊ม)
dotnet run --no-build -- --multi-check

# สร้าง RecipeRequirements.cs ใหม่จากข้อมูลเกม (ปกติไม่ต้องรัน)
cd "C:\Users\thana\Desktop\Durango Claude\server"
python scripts/extract_recipes.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/RecipeRequirements.cs

# ILSpy decompile (เครื่องมี .NET 9 แต่ ilspycmd ขอ 6)
export DOTNET_ROLL_FORWARD=LatestMajor && ilspycmd -p ...
```

**พอร์ต:** gateway HTTP **8190** · game TCP **8191** · radiotower (แชท) **8192**
> ~~⚠️ `ConnectTo()` ฮาร์ดโค้ด 8190~~ ✅ แก้ในซอร์สแล้ว — รับ `ip:port` ได้ (จำเป็นกับระบบหลายเกาะ)
> (พอร์ต 8390 ที่เห็นในโค้ดคือ server ภายในของ client เอง คนละตัว — ผมเคยวินิจฉัยผิดว่าต้องใช้ 8390 แก้ใน CHANGELOG แล้ว)

---

## 2. งานถัดไป

### 2.1 เทสรอบสุดท้ายก่อนเปิด beta ← **สิ่งที่ค้างอยู่ตอนหยุด**

เกณฑ์ทั้ง 5 ข้ออยู่ใน `docs/BETA-1.0-PLAN.md` §4 — ผ่านแล้ว 4 ข้อ เหลือข้อ 3:

| # | เกณฑ์ | สถานะ |
|---|---|---|
| 1 | `--gp-check` ผ่านครบ | ✅ 45/45 |
| 2 | บอทฟาร์ม 30 นาที: exception 0 · tps ≥100 · RAM ไม่โตเกิน 20% | ✅ 120 tps · RAM นิ่ง |
| 3 | **เล่นด้วยตัวเกมจริง 30 นาที** | ⏳ รอบแรกเจอ 3 บั๊ก (แก้แล้ว) — ต้องเล่นซ้ำ |
| 4 | 3 client พร้อมกัน: เห็นกันครบ · ไม่มีของก๊อป | ✅ `--multi-check` 9/9 |
| 5 | เปิดทิ้งไว้ไม่มีคนเล่น: เซฟไม่โต · สัตว์ครบโควตา | ✅ (เทส 15 นาที ไม่ใช่ 3 ชม.) |

ข้อ 3 ต้องดู: สัตว์ไม่วาร์ป/ไม่ค้างท่า · ตาย-ฟื้นแล้วจอเด้งไปจุดเกิดจริง ·
ออกจากโหมดต่อสู้ได้ · กระเป๋าเต็มแล้วกด "ทิ้ง" ได้ · กินของแล้วสตามินาขึ้น
**เพิ่มจากรอบที่แล้ว:** คลิกสัตว์แล้วปุ่มโจมตีขึ้นไหม · โดนสวนกลับไวพอไหม ·
ฆ่าแล้วซากเรืองแสงไหม · แตะซากแล้วมีเมนูเนื้อ/หนัง/กระดูก และแล่แล้วของเข้ากระเป๋าจริงไหม
(ออกจากโหมดต่อสู้ก่อนถึงจะแตะซากได้ — ตอนอยู่ในโหมดต่อสู้ client ไม่เปิดเมนูให้)

```bash
# เปิดเกม + ต่อ 127.0.0.1 อัตโนมัติ
powershell -File "C:\Users\thana\Desktop\Durango Claude\tools\connect-game.ps1"
```

### 2.2 หลัง beta 1.0 (ยังไม่ทำ)

- เพ็ท · ปาร์ตี้ · แคลน · ตลาด · ภารกิจ (เมนูซ่อนไว้หมดแล้ว)
- แชทส่วนตัว: ต้องทำ auth ให้ radiotower ก่อน (M-5) ตอนนี้ปิดพอร์ตไว้
- ความสูงพื้น (elevation): `ChunkData` ของเกมไม่มีข้อมูลนี้ — server ใช้ `Height` ที่ client รายงานมา
- `whole.ocean` แยกน้ำ/พื้นดินยังถอดรหัสไม่ได้ (ดูบันทึกใน `TerrainStore.cs`)

## 3. บั๊กที่รู้แล้วแต่ยังไม่แก้

| รหัส | เรื่อง |
|---|---|
| GP-06 | แชทส่วนตัวไม่ทำงาน |
| ~~GP-08b~~ | ✅ แก้แล้ว — ไอเทมมี `Tags` จริง (`ItemTagData`) คราฟต์เช็ค tag+วัสดุ · เก็บของต้องมีเครื่องมือ |
| ~~GP-09b~~ | ✅ แก้แล้ว — เก็บของ/แล่/คราฟต์ ใช้ `Duration` จริงและคูณด้วยสกิล |
| ENV-01 | `resources.assets` ยังเสียอยู่ |
| — | ~~NPC dialogue ปิดจาก server ไม่ได้~~ ✅ แก้แล้วด้วย `RegionRole=Sandbox` (`PlayGuideSystem.Initialize` early-return) |
| — | ~~สัตว์ `MotionName = null`~~ ✅ แก้แล้ว — ดึงชื่อคลิปจาก prefab ด้วย UnityPy (`scripts/extract_animal_motions.py` → `AnimalMotionData.cs` 213 ชนิด) |
| — | ~~ไอเทมยังไม่มีข้อมูลโภชนาการจริง~~ ✅ แก้แล้ว — `FoodData` 352 ชนิดจาก TextAsset `performance` |
| — | สูตรแก้ทรงเสื้อ (Reform 22 อัน) ยังทำไม่ได้ — ต้องมีระบบ reform slot ก่อน (ปฏิเสธพร้อมบอกเหตุผลแล้ว) |

---

## 4. ผลทดสอบที่ยืนยันกับเกมจริงแล้ว ✅

- HTTP handshake ครบวงจร
- `[touch]`×7 → `[collect]`×3 → `[natural] ran out`
- เมนูเก็บของวงกลมโชว์ **"3"** และ **"1.53 วินาที"** ตรงกับ `Amount=3, Duration=1.5f` ที่ server ส่ง
- นับถอยหลัง 3→2 พิสูจน์ว่า GP-03 (จองเครื่องปั่นแบบ atomic) ทำงาน
- ไฟล์เซฟมี `ปลา ×3` · ตำแหน่งผู้เล่นถูกจำข้ามเซสชัน
- **client ไม่มี exception เลยตลอดเซสชัน**
- UDP knock discovery โชว์ "Multi Play Server 192.168.1.39" ⇒ **GP-11 แก้ได้จริง**
- FarmBot จับบั๊กกระเป๋าล้นได้ (52 ชิ้น > MaxSize 50) → แก้แล้ว เทสซ้ำได้ 50 ชิ้นพอดี + abort 13 ครั้ง ตรงกับ log `[inventory] กระเป๋าเต็ม` 13 บรรทัด

### 14 ส.ค. — GP-08/09/12/14 (เทสด้วย test-client ยังไม่ได้เทสกับเกมจริง)
- `--gp-check` ผ่าน **16/16**: token ปลอมเข้าไม่ได้ · แตะ tile เปล่า/ไกลเกินเอื้อมไม่ได้ · เก็บของโดยไม่แตะไม่ได้
  · สั่งลบต้นไม้ที่ไม่เคยแตะไม่มีผล · คราฟต์ลม/ของปลอม/สูตรมั่ว/ใส่ของซ้ำช่องไม่ได้ · เลเวลอ้างเกินโดนตัด
- และ **ของที่ควรทำได้ยังทำได้**: คราฟต์ด้วยของจริงสำเร็จ + วัตถุดิบถูกหักจริง (3 ชิ้น → เหลือ 1)
- FarmBot รุ่นใหม่ (อ่าน garden จาก `Chunk`) ฟาร์มได้ 10 ชิ้นใน 24 วินาที **abort 0**
- 🐛 เจอระหว่างทาง: `LoadPlayerSave()` ทับเลเวลจาก session ด้วย `0.player` ของเจ้าของเครื่อง (Lv.5 → Lv.60) — แก้แล้ว

---

## 5. กับดักที่เคยเสียเวลาไปแล้ว (อย่าเหยียบซ้ำ)

1. **ไฟล์ exe ล็อก** — ต้อง kill DurangoServer/dotnet ก่อน build ทุกครั้ง
2. **PowerShell `-ArgumentList` ไม่ใส่ quote** ให้ path ที่มีช่องว่าง ⇒ `-logFile` โดนตัดที่คำว่า "Durango" — ใช้ `launch.bat` แทน
3. **bash heredoc ทำ backslash/`\n` เพี้ยน** — เขียน Python script ด้วย Write tool แทน, ใช้ `chr(92)` แทน backslash
4. **เทสสตามินาเคยผ่านแบบหลอก** — ตั้ง 3 แล้วรอ 700ms แต่ regen 4/s ดันขึ้นเกินค่าใช้ 6 ⇒ ต้องตั้ง 0 แล้วยิงทันที
5. **`Messages.Say` ไม่มี TypeCode** ⇒ handler ไปลงคีย์ 0 ซึ่งเป็นคีย์ reply ของ client — ลบ handler ทิ้งแล้ว
6. **`PushGauges` ต้องส่ง `Removed = Array.Empty<string>()`** ห้ามเป็น null
7. **`RebuildEquipments()` ต้องคืน `Presets`/`ItemSlots` ที่ไม่ใช่ null** ไม่งั้น client NRE (`EquipSystem.EquipmentsReceived` deref ตรง ๆ)
8. **TakeOutItem ต้องตอบ `OK`** เพราะ client เช็ค `Packet.IsSuccess`
9. **Firewall** — ตอน Windows Defender ถาม ผมกด **Cancel** ไม่ใช่ Allow (เป็นการเปลี่ยนความปลอดภัยเครื่อง เจ้าของเครื่องควรตัดสินเอง) เทสผ่าน loopback ไม่ต้องใช้

---

## 6. ไฟล์ที่ต้องรู้จัก

**Server**
| ไฟล์ | หน้าที่ |
|---|---|
| `ServerCore/ServerPlayer.Core.cs` | ฟิลด์ · ctor · `RegisterHandlers()` (32 packet) · Move · SetChunk |
| `ServerCore/ServerPlayer.Survival.cs` | `GaugeState` · เลือด/สตามินา/ความล้า · `PushGauges` |
| `ServerCore/ServerPlayer.Equipment.cs` | สวมใส่ของ |
| `ServerCore/ServerPlayer.Storage.cs` | กล่องเก็บของ (`BoxMaxSize=200`) |
| `ServerCore/ServerPlayer.Sync.cs` | `PlayerInventoryMaxSize = 50` · `InventoryFull` |
| `ServerCore/ServerWorld.cs` | สิ่งก่อสร้าง · เครื่องปั่น · กล่อง · สัตว์ · Save/Load |
| `ServerCore/Gateway.cs` | HTTP + UDP knock (`/entry` ใช้พอร์ตจริง ไม่ใช่ค่าคงที่) |
| `ServerCore/AnimalSpawner.cs` · `ServerAnimal.cs` | สัตว์ 34 ตัว เดินสุ่ม/ไล่/หนี · ซาก · เกิดใหม่ |
| `ServerCore/ServerConfig.cs` | อ่าน `data/config.json` (เรทเกิด/สมดุล/exp) + hot-reload ทุก 5 วิ |
| `ServerCore/SpawnTable.cs` | ตัวอ่านตารางสัตว์จาก config (ตัวเลขจริงอยู่ใน config.json) |
| `ServerCore/LevelData.cs` | ตารางเลเวล **ค่าจริงของเกม** — สร้างจาก `scripts/extract_levels.py` |
| `ServerCore/ServerPlayer.Progress.cs` | exp/เลเวล/แต้มสกิล — `GainExpFor*()` |
| `ServerCore/ButcheryData.cs` | ซากสัตว์แต่ละชนิดแล่ได้อะไรบ้าง |
| `ServerCore/EquipData.cs` · `AnimalData.cs` · `RecipeRequirements.cs` | **สร้างอัตโนมัติ** จาก `scripts/extract_*.py` อย่าแก้มือ |
| `ServerCore/GameServer.cs` | GP-12: `IssueSession` / `TryAuthorize` — Auth ต้องมี token จาก `/sessions` |
| `Program.cs` | `TargetTps=120` · `timeBeginPeriod(1)` · auto-save 60 วิ · Ctrl+C เซฟ |

**Client (จุดสำคัญ)**
- `client/Durango.Offline/Server.cs` — `ConnectTo` ฮาร์ดโค้ด 8190
- `client/Durango.UI/MenuListGroupBase.cs` — บั๊กกรอง IP ตัวเอง (ข้อ 2.1)
- `client/GameManager.cs:172` — `IsPrologueMode`
- `client/EquipSystem.cs` — `EquipmentsReceived` deref `msg.Presets` ไม่เช็ค null

---

## 7. ที่มาของโปรเจกต์

ชุบชีวิตเกม **Durango: Wild Lands** ของ NEXON ที่ปิดไปแล้ว ด้วย private server เขียนเอง
โค้ดฝั่ง client มาจากการ decompile ด้วย ILSpy — server พูดโปรโตคอลเดิมของเกม
(header 24 ไบต์: Time/Seq/ReplyOf/TypeCode/PayloadSize + MsgPack → Snappy)

**`Gauge` ทำงานยังไง:** เป็น array ของ keyframe ที่ client ประมาณค่าเอง
server ส่ง `[(ตอนนี้,ค่า),(อนาคต,เป้าหมาย)]` ครั้งเดียว **ไม่ต้อง tick ทุกเฟรม**

