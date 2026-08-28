# HANDOFF — Durango / งานตัวเกมวันนี้

## อัปเดตล่าสุด — 28 ส.ค. 2026: ตรวจบั๊กตัวเกมและ Server โดยเฉพาะ “มอนล่องหน”

### สถานะที่ตรวจยืนยันแล้ว

- Server build ผ่าน `0 error / 0 warning` ด้วย `dotnet build server\DurangoServer.csproj --no-restore`
- Server ที่กำลังรันอยู่ตอบสนองปกติ: ประมาณ `119–120 TPS`, มอนมีชีวิต `34 ตัว`, พอร์ตเกม `8191`, Gateway `8190`
- `/entry` ตอบ `200` และ log ยืนยันว่า Client เข้าโลกจริง, โหลด terrain/chunk และ Server ส่ง initial world พร้อมสัตว์ `34 ตัว`
- มอนที่ตั้งใน `server/data/config.json` มี 10 ชนิด รวมโควตา 34 ตัว
- ตรวจ `Info.5.2.1.json` และไฟล์ bundle จริงแล้ว: prefab ของมอนทั้ง 10 ชนิดมีไฟล์จริง, ขนาดตรงกับ manifest และ CRC ตรงกับชื่อไฟล์ครบทุกตัว
- `AnimalMotionData` มี motion mapping ของมอนทั้ง 10 ชนิดครบ จึงไม่น่าใช่ปัญหา Server ไม่รู้จักชนิดหรือไม่มีชื่อ animation
- รอบนี้เป็นการตรวจและสรุป ยังไม่ได้แก้โค้ดเพิ่ม และยังไม่ได้ deploy production

### สาเหตุที่เป็นไปได้ของ “มอนล่องหน”

1. **มอนอยู่นอกระยะมองเห็น — ความเป็นไปได้สูง**

   จุดเริ่มเกมคือประมาณ tile `40,177` แต่จุดกลางโซนที่ Server ใช้จริงอยู่ไกลกว่า `ViewRangeTiles=24`:

   - meadow ประมาณ `32.6` tiles
   - forest ประมาณ `55.3` tiles
   - raptor den ประมาณ `40.7` tiles
   - highland ประมาณ `170.8` tiles

   จึงมีโอกาสที่ผู้เล่นยืนจุดเริ่มเกมแล้วไม่เห็นมอน แม้ Server จะมีมอนและทำงานถูกต้องตาม view culling
   ดู `server/data/config.json` ส่วน `Zones` และ `World`

2. **Server ส่งความสูงมอนเป็น 0 หรือใช้ค่าประมาณผิด — บั๊กจริงที่ต้องแก้**

   Terrain มีไฟล์ `whole.elevations` แต่ `TerrainStore` ยังไม่ได้โหลดไฟล์นี้ มอนจึงใช้ `GroundHeightHint`
   จากค่าความสูงที่ Client รายงานล่าสุดแทน หากไม่มีค่าดังกล่าวจะได้ `Height=0` ทำให้ Client วางมอนใต้พื้นหรือผิดระดับ

   ไฟล์เกี่ยวข้อง:

   - `server/ServerCore/TerrainStore.cs` — ยังไม่ได้โหลด `whole.elevations`
   - `server/ServerCore/AnimalSpawner.cs` — กำหนด `animal.Height = GroundHeightHint`
   - `server/ServerCore/ServerPlayer.Vision.cs` — ซ่อมความสูงเฉพาะเมื่อค่าเป็น 0
   - `client/PathMovable.cs` — คำนวณตำแหน่ง Y จาก `Floor + Height + WaterDepth`

3. **Client โหลด prefab ไม่สำเร็จแล้วไม่มี retry — บั๊กจริงที่ทำให้หายถาวรได้**

   `client/AnimalManager.cs` เรียก `RequestAsset()` แล้วถ้า asset ไม่พร้อม, path ว่าง หรือหา bundle ไม่พบ จะลบสถานะ ghost
   และแสดง error เท่านั้น นอกจากนี้ยัง dereference `AnimalBehavior` โดยไม่ null-check

   หาก Client โหลดไม่สำเร็จ แต่ Server เพิ่ม entity เข้า `_seenAnimals` ไปแล้ว Server จะคิดว่า Client เห็นมอนแล้ว
   จึงไม่ส่ง `AppearAnimal` ซ้ำจนกว่าจะออก/เข้าเขตใหม่

   จุดต้องแก้:

   - log `EntityId`, `EntityType`, prefab path และสาเหตุ RequestAsset fail
   - null-check `AnimalBehavior`, `Animation` และ Renderer
   - ล้าง `_animals`/`_ghosts` ให้ถูกต้องเมื่อโหลดล้มเหลว
   - เพิ่ม retry หรือ protocol ACK ให้ Server ส่ง Appear ซ้ำได้

4. **Fallback spawn ยังยอมใช้จุดที่ตรวจไม่ผ่าน**

   ถ้าหาจุดบนบกไม่สำเร็จใน 80 ครั้ง Server ใช้จุดสุ่มสุดท้าย แม้จุดนั้นอาจเป็นหาด/น้ำ/พื้นที่ผิดระดับ
   Log รอบนี้พบจริงกับ `โดโดฟิซิส` จึงอาจทำให้มอนอยู่ใต้ฉากหรือไกลจากจุดที่คาดไว้

### บั๊กอื่นที่พบจากการตรวจ

- `game/game.log` แจ้ง `resources.assets` corrupted หลายครั้ง
- `level2` และ `level3` แจ้ง corrupted
- GameObject `CombatModeButton` อ้าง script ที่หาย
- Server แจ้ง `Messages.GetPersonalProducts has no TypeCode` และลง handler ที่ key `0` — ไม่ใช่ต้นเหตุโดยตรงของมอนล่องหน แต่เป็น protocol bug
- `test-client` build ไม่ผ่าน เพราะ `test-client/DurangoTestClient.csproj` ไม่มี package `Lib.Harmony` เหมือน Server
- test client รุ่นเก่าส่ง character id แบบสุ่ม/ไม่มีอยู่จริง จึงถูก Gateway ปฏิเสธด้วย `character_not_found`; visibility test ยังใช้ยืนยันไม่ได้
- Server ที่ใช้ทดสอบเปิด `--enable-cheat --admin gm --admin-token test-token` อยู่ ห้ามนำคำสั่งนี้ไปใช้ production
- Log มี `dock ไม่เจอ` ตอนสร้างโลก เป็นปัญหา world/POI แยกจากมอนล่องหน

### ลำดับงานถัดไปที่แนะนำ

1. แก้/เพิ่มระบบอ่าน `whole.elevations` และกำหนดความสูงมอนตาม tile จริง
2. ห้ามใช้จุด fallback ที่ไม่ผ่าน `IsLand`; ให้ค้นจุด valid หรือข้ามการ spawn รอบนั้น
3. เพิ่ม diagnostic log และ retry ใน `AnimalManager.MakeAnimalObject()`
4. ปรับ spawn zone หรือเพิ่ม test spawn ใกล้จุดเริ่มเกม เพื่อแยก “อยู่นอกระยะ” กับ “โหลดไม่ขึ้น”
5. ซ่อม/ตรวจไฟล์ `resources.assets`, `level2`, `level3` และ script `CombatModeButton`
6. เพิ่ม `Lib.Harmony` ให้ test-client แล้วปรับ vision test ให้ใช้ตัวละครที่มีอยู่จริง
7. ทดสอบใหม่ด้วยเกมจริง: เข้าโลก → เดินเข้า meadow → ตรวจว่ามอนปรากฏ/เคลื่อนที่/ถูกโจมตี/หายเมื่อออกระยะ → reconnect แล้วตรวจซ้ำ

### ไฟล์สำคัญ

- สเปก Map Editor: `tools/MapEditor/spec.md`
- Client สร้างมอน: `client/AnimalManager.cs`
- Client การเคลื่อนที่/ความสูง: `client/PathMovable.cs`
- Server spawn: `server/ServerCore/AnimalSpawner.cs`
- Server ส่ง visibility: `server/ServerCore/ServerPlayer.Vision.cs`
- Server สัตว์และ packet: `server/ServerCore/ServerAnimal.cs`
- Terrain: `server/ServerCore/TerrainStore.cs`
- Log ตัวเกม: `game/game.log`

---

# HANDOFF — Durango Claude
## งานล่าสุด — Main ใช้ปุ่ม PC / ในเกมใช้ Mobile UI และจัดชุด DLL หลัก (28 ส.ค. 2026)

สถานะที่ยืนยันกับเกมจริง:

- หน้า Main/หน้าเลือกเซิร์ฟเวอร์ใช้ prefab และปุ่มแบบ PC โดยแพตช์ `UIPrefabMap.GetTitle()` ให้คืน `_titlePC`
- UI ในฉากเกมยังเป็น Mobile UI (`Platform_PC.UsePCUI = false`) และแสดงปุ่มมือถือ
- คลิกขวาเพื่อเดินผ่านแล้ว โดย `PlayerController.OnAwake()` ไม่กั้น handler `MoveToPosition` ด้วย `UsePCUI`
- พื้นหลัง Main กลับมาแล้ว: เปลี่ยน `game/DurangoV2_Data/StreamingAssets/Movie/PC/title.mp4` ที่เสีย (9,440,309 bytes, 11.2 วินาที) เป็นไฟล์จาก `game-backup` (62,976,166 bytes, 103.2 วินาที)
- ไฟล์ title เดิมเก็บไว้ที่ `game/DurangoV2_Data/StreamingAssets/Movie/PC/title.mp4.corrupt-20260828`
- `Dinoworld Server` ใช้ NGUI BBCode ที่ถูกต้อง `[C2185B]Dinoworld Server[-]` จึงแสดงสีชมพูเข้ม
- ภาพทดสอบยืนยัน: Main มีพื้นหลังวิดีโอและปุ่ม PC; เข้าโลกแล้ว Mobile UI ยังทำงาน

### ชุด DLL หลักและ backup

- ตัวหลักที่เกมใช้งาน: `game/DurangoV2_Data/Managed/Assembly-CSharp.dll`
- backup หลักของแพตช์วันนี้: `game/Backups/Assembly-CSharp.dll.main-20260828.dll`
- ต้นฉบับสำหรับแพตช์ซ้ำ: `game/DurangoV2_Data/Managed/Assembly-CSharp.dll.bak`
- backup DLL รุ่นเก่า `Assembly-CSharp.dll.bak.patched.dll`, `Assembly-CSharp.dll.original-ilpatch-backup` และ `Assembly-CSharp.dll.recompiled-mod-backup` ถูกลบแล้ว
- hash SHA256 ของตัวหลักและ backup วันนี้ตรงกัน: `0EA5815F21E15D496777EC39571C9C34D908F59068448ED50E7166C02C59D5DE`

### คำสั่งแพตช์ DLL

ต้องแพตช์จาก `.bak` ทุกครั้ง ห้ามแพตช์ซ้ำจาก DLL ที่แพตช์แล้ว:

```powershell
cd 'C:\Users\thana\Desktop\Durango Opencode'
dotnet build tools\DllPatcher\DllPatcher.csproj --no-restore --verbosity quiet
& tools\DllPatcher\bin\Debug\net9.0\DllPatcher.exe game\DurangoV2_Data\Managed\Assembly-CSharp.dll.bak
Copy-Item game\DurangoV2_Data\Managed\Assembly-CSharp.dll.bak.patched.dll game\DurangoV2_Data\Managed\Assembly-CSharp.dll -Force
```

หลังแพตช์ใหม่ ให้คัดลอกตัวหลักไปทับ `game/Backups/Assembly-CSharp.dll.main-YYYYMMDD.dll` และตรวจ hash ก่อนลบไฟล์เก่า

ตรวจแพตช์สำคัญ:

```powershell
& tools\DllPatcher\bin\Debug\net9.0\DllPatcher.exe game\DurangoV2_Data\Managed\Assembly-CSharp.dll --dump UIPrefabMap GetTitle
& tools\DllPatcher\bin\Debug\net9.0\DllPatcher.exe game\DurangoV2_Data\Managed\Assembly-CSharp.dll --dump PlayerController OnAwake
```

ค่าที่ควรเห็นคือ `UIPrefabMap::_titlePC`, `InputSystem::On` สำหรับการเดิน 2 จุด และไม่มี `Platform::get_UsePCUI` คั่น handler ใน `PlayerController.OnAwake()`

ข้อควรระวัง:

- เปิดเกมผ่าน Computer Use `@oai/sky` หากต้องการหน้าต่างที่มองเห็นได้; `Start-Process` จาก shell บางครั้งสร้าง process แบบไม่มีหน้าต่าง
- warning `resources.assets is corrupted` ยังเป็นปัญหาแยกจาก `title.mp4`; เกมยังเข้าโลกได้ แต่ควรกู้ `resources.assets` แยกก่อน deploy

---
**อัปเดตล่าสุด:** 26 ส.ค. 2026 — **แก้บั๊ก "แท็บสกิลว่าง (เห็นแค่ tile)" — สาเหตุจริงคือเซิร์ฟไม่ copy terrain data ไป runtime**

## งานล่าสุดสำหรับ agent คนถัดไป (26 ส.ค. 2026) — แก้บั๊กแท็บสกิลว่างในโหมด online

- **อาการ:** เปิดแท็บสกิลผ่านเซิร์ฟบ้าน (online) เห็นแค่ช่องเดียว ("tile" + กรอบเหลือง) หมวดอื่นไม่โผล่ — เล่น **offline** (client โหลด terrain จาก Unity Resources เอง) แท็บสกิลปกติ ผู้เล่นเทสเทียบเองแล้วชี้ตรงจุด
- **สาเหตุจริง (ยืนยันด้วยการทดสอบสด ไม่ใช่แค่อ่านโค้ด):** `server/DurangoServer.csproj` ไม่มี directive copy `server/data/` ไป output dir เลย — `server/bin/Debug/net9.0/data/` มีแค่ `config.json` ไม่มี `terrains/`/`islands/`/`islands.json`/`whitelist.txt` ⇒ `TerrainStore.Load()` หา `info.yml` ไม่เจอ ⇒ `region_template` เป็น null ⇒ `GameServer.SendWelcome()` ส่ง `Region.TemplateId = null` ให้ client ⇒ `SkillCategoryWidget.Init()` เจอ `SingletonDict.Get(null) == null` ออกก่อนสร้างรายการหมวดสกิลเลย
  - ระหว่างไล่บั๊กเคยสงสัยผิดทาง 2 รอบ: (1) cheat mode เปิดอยู่ — ปิดแล้วยังบั๊ก, (2) world save เก่าเพี้ยน — ลบโลกสร้างใหม่แล้วยังบั๊ก ⇒ ตัดจนเหลือจุดเดียวคือ terrain data ที่ runtime path
- **แก้แล้ว 2 จุด:**
  1. `server/DurangoServer.csproj` — เพิ่ม `<ItemGroup>` copy `server/data/**/*` (ยกเว้น `config.json`) ไป output dir อัตโนมัติทุกครั้งที่ build (`CopyToOutputDirectory=PreserveNewest`)
  2. `client/Durango.UI/SkillCategoryWidget.cs` + `SkillGroup.cs` — defensive fix: ดึงลอจิกสร้างรายการออกเป็น `BuildCategoryList()` (idempotent, เคลียร์ list ก่อนสร้างใหม่) แล้วเรียก `Rebuild()` ทุกครั้งที่เปิด UI ผ่าน `SkillGroup.OnOpened()` — กันเคสในอนาคตที่ Region อาจยังไม่พร้อมจริง ๆ ตอน `Init()`
  3. บั๊กแฝงที่เจอระหว่างแก้: ชื่อหมวด `Weaponcrafting`/`Armorcrafting` ขึ้นเป็น raw localization key เพราะข้อมูล localize ใช้ชื่อ enum เก่า (`WeaponCrafting`/`ArmorCrafting` ตัว C ใหญ่) — แก้ใน `client/Durango.Logic.Skill/Util.cs`
- **ยืนยันแล้วในเกมจริง:** build ทั้งเซิร์ฟ+client ผ่าน 0 error → เปิดเซิร์ฟ (มี terrain data) + เปิดเกม online (ปิด cheat, โลกใหม่) → แท็บสกิลขึ้นครบ — ผู้เล่นเทสเองยืนยัน "ผ่านแล้ว"
- **สถานะ:** build ผ่าน 0 error ทั้งสองฝั่ง ยังไม่ได้ deploy production — รอเจ้าของอนุมัติ
- รายละเอียดเต็ม (พร้อมภาพ): `docs/server/Skill-Tab-Blank-Fix.md` · สรุปสั้นในรายงานบั๊ก: `docs/reports/bug-report-memorybot-beta.md` หัวข้อ H3

## งานล่าสุดสำหรับ agent คนถัดไป (26 ส.ค. 2026) — MemoryBot MVP

- สร้าง client mod แยก `tools/MemoryBotMod/` (net35, โหลดจาก `game/mods/`) ตามที่เจ้าของขอ: อ่านข้อมูลแบบ memory-like จาก managed state ของเกม ไม่ใช่ raw process memory และสั่งงานผ่าน API เกมจริง ไม่ใช้เมาส์/คีย์บอร์ด/ภาพเป็นกลไกหลัก
- TCP loopback `127.0.0.1:8193` (แยกจาก `BotBridge` เดิมที่ 8192) · JSON-line · `request_id` · queue สูงสุด 32 · ประมวลผลบน Unity main thread เท่านั้น · token opt-in ผ่าน `DURANGO_MEMORYBOT_TOKEN`
- Read paths แบบ whitelist: `game`, `screen`, `player.local`, `survival`, `inventory`/`inv`, `status`, `interaction`, `combat`, `world.nearby` — path นอก registry ปฏิเสธ ไม่มี arbitrary reflection
- Commands แบบ semantic: `player.stop`, `player.move_to`, `interaction.select_nearest`, `interaction.refresh`, `interaction.execute`, `inventory.use`, `combat.use_action`, `ui.open` (Inventory/Skill/Status) — ผ่านระบบเกม/server validation เดิม ไม่ bypass
- Capture เฉพาะ `op=capture` (on-demand) เขียน PNG ใต้ `game/MemoryBotCaptures/`; ไม่มี screenshot เป็น data source ปกติ
- **เทสสดบน MainScene จริงครบ**: `player.move_to` ขยับจริง · `ui.open` เปิดจริง · `interaction.select_nearest`→`read interaction`→`interaction.execute Rest` กระตุ้นการพักครบ pipeline — เซิร์ฟ log `[rest] 252 เริ่มพักที่ กองไฟ`, read `status` เห็น `away_from_keyboard`, `survival` เห็น fatigue velocity `-4` ลดจริง · token ผิด→`unauthorized` · request ใหญ่/unknown path/unknown command ปฏิเสธหมด · capture เขียนไฟล์ PNG จริง
- build ผ่าน 0 warning/0 error; ติดตั้งเฉพาะ `game/mods/DurangoMemoryBot.dll` ไม่แตะ `Assembly-CSharp.dll`
- เอกสาร: `docs/client/MemoryBot.md` · build/install: `tools/build-memory-bot.ps1` · smoke client: `tools/MemoryBotClient/memorybot-client.ps1`
- **ยังไม่ได้ deploy production**; local test เท่านั้น (mod DLL เป็น trusted code ใน process เดียวกับเกม ไม่ใช่ sandbox)

## งานล่าสุดสำหรับ agent คนถัดไป (26 ส.ค. 2026) — ระบบพักทุก Shelter

- แก้บัพนั่งพัก: `TryStartResting()` เปิด status effect `away_from_keyboard` และส่ง `Messages.StatusEffects` ให้ client จึงมีไอคอนบัพตอนพัก
- `StopResting()` ปิด status effect และส่ง packet ใหม่เมื่อเดิน/ทำกิจกรรม/ความล้าหมด; ป้องกัน `SleepChecker` wake-up packet ปิดบัพขณะ server ยังพักจริง
- แก้ false stop จาก movement jitter/snap เข้า attachment: `RememberPosition(Move)` หยุดพักเฉพาะเมื่อขยับจริงเกิน 10 world units ไม่ใช่ทุก Move packet
- เปลี่ยนเกณฑ์จุดพักจากเดาชื่อ blueprint (`fire/tent/bed/rest`) เป็นข้อมูลเกมจริง `RecipeData.BlueprintComponents[id]` ที่มี component `Shelter` — ครอบคลุมกองไฟ เต็นท์ เก้าอี้ โซฟา เตียง เสื่อ และสิ่งก่อสร้างพักอื่นทั้งหมดที่ client แสดง Interaction.Rest (159 IDs)
- ปรับข้อความแจ้งเตือนจาก "กองไฟ" เป็น "สิ่งก่อสร้างสำหรับพักผ่อน" ให้ตรงกับทุกชนิดของ Shelter
- เพิ่ม regression ใน `test-client/StaminaCheck.cs`: ตรวจ fatigue ลดจริง, เปิด/ปิด `away_from_keyboard`, และสร้าง/พักที่ `camp_square_fire`
- ผลทดสอบล่าสุด: `--stamina-check` ผ่าน **19/19**; server build ผ่าน 0 errors; test-client build ผ่าน 0 errors
- **ยังไม่ได้ deploy production**; local test เท่านั้น

---

**อัปเดตก่อนหน้า:** 25 ส.ค. 2026 — **ท่าต่อสู้ยึดจากสกิลที่เรียนจริงแล้ว (combat skill-gating) — เทสอัตโนมัติผ่าน 13/13**

## งานล่าสุดสำหรับ agent คนถัดไป (25 ส.ค. 2026)

- **ท่าต่อสู้ (combat) ยึดจากสกิลที่เรียนจริงแล้ว** — เจ้าของย้ำ 2 รอบว่า "ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน"
  เดิม `HandleUseBattleAction` ตรวจแค่ tag อาวุธ ไม่เคยเช็ค `_knownSkills` เลย ⇒ modded client
  ใช้ท่าพิเศษ (smash/stab/flurry/aimedshot ฯลฯ) ได้ทุกอย่างโดยไม่เรียนสกิล
- สกัดข้อมูล "สกิลไหนปลดท่าต่อสู้อะไร" จาก `resources.strings.txt` (skills → rewards type=8 → action_ids)
  ด้วยสคริปต์ใหม่ `server/scripts/extract_action_unlocks.py` (เลียนแบบ `extract_recipe_unlocks.py`)
  → สร้าง `server/ServerCore/ActionUnlockData.cs` (AlwaysActions 27 ท่าพื้นฐาน + BySkill 14 สกิลที่ให้ 32 ท่า)
- เพิ่ม `UnlockedActions()` / `IsActionUnlocked()` ใน `ServerPlayer.Skills.cs` (เหมือน `UnlockedRecipes()`)
- เพิ่ม skill check ใน `HandleUseBattleAction` (`ServerPlayer.Combat.cs`) หลัง weapon-tag check
  ก่อน cooldown/target check — ท่าที่ไม่ได้เรียนสกิลถูกปฏิเสธพร้อมข้อความ "ต้องเรียนสกิลก่อน"
- กรอง `HandleGetActions` ให้ส่งเฉพาะท่าที่ปลดล็อกแล้ว (เดิมส่งครบทุกท่าของอาวุธ)
- เพิ่มเทส `--combat-skill-check` (`test-client/CombatSkillCheck.cs`) 13 ข้อ: ผู้เล่นใหม่ได้ 6 ท่า
  (พื้นฐาน + auto-grant kick/reckless/dodge) ไม่เห็น barehand_combination/melee_tackle;
  สั่งใช้ท่าที่ยังไม่เรียน → Abort; หลัง maxskills → เห็นครบ 8 ท่า และผ่าน skill check
- ผลทดสอบสด: `--combat-skill-check` ผ่าน 13/13; `--gp-check` ผ่าน 45/45; `--skill-check` ผ่าน 13/13
  — server Debug build ผ่าน 0 errors (warning เดิมจำนวนมาก)
- server log ยืนยัน: "ปฏิเสธ ... ยังไม่ได้เรียนสกิลที่ปลดล็อกท่า barehand_combination" (ก่อนแก้)
  → หลัง maxskills: "ปฏิเสธ ... ไม่มีเป้าหมาย fake-target-id ในโลก" (ผ่าน skill check แล้ว ตกที่ target check)
- **ยังไม่ได้ deploy production และไม่ได้ลบไฟล์ใด ๆ ในงานรอบนี้**; worktree มีงานเก่าค้างจำนวนมาก
  ห้าม reset/cleanup/ลบไฟล์โดยไม่ถามเจ้าของก่อน

## 🚀 QUICK HANDOFF — ให้ agent ใหม่เริ่มตรงนี้

**โปรเจกต์:** `C:\Users\thana\Desktop\Durango Opencode`

**สถานะที่ยืนยันแล้ว:**
- Build server ผ่าน 0 error
- Food status effects ทำงานจริงและเทสสดแล้ว: `poisoning` HP ลด ~1/sec, `life_up` HP เพิ่ม ~1/sec, บัฟ/ดีบัฟสตามินาทำงาน
- เมนูสร้างซ่อน 10 หมวดสำหรับ non-admin และ admin ยังเห็นครบ — เทสสดแล้ว
- แก้บั๊กสร้างตัวใหม่ซ้ำ: normalize `::1`/`127.0.0.1` ใน `AccountStore`
- แก้ auto-connect: ใช้ `DURANGO_AUTOCONNECT=127.0.0.1`
- ท่าต่อสู้ยึดจากสกิลที่เรียนจริง: `--combat-skill-check` ผ่าน 13/13 — เทสสดแล้ว
- **ยังไม่ deploy production**; ทำ local test เท่านั้น

**งานต่อที่ต้องทำ:**
1. ตรวจ `git diff` และ build ล่าสุดก่อนแก้ต่อ
2. อย่าเดา requirement — ถ้าไม่แน่ใจให้ถามเจ้าของก่อน
3. งานระบบที่ยังค้าง: weather/environment debuff, combat debuff, ยาลบ debuff, audit skill-gating ของ gathering/hunting/farming (combat ทำแล้ว)
4. ตรวจแท็บ Craft/Build ที่ยังไม่ได้ไล่: Cooking, Farmland/Road, Fence/Gate, Installation/Decoration, Clan, Rest/Shelter, Transportation Facility
5. แก้/ทดสอบ local ก่อนเสมอ และห้าม deploy โดยไม่ได้รับอนุญาต

**คำสั่งเริ่มต้น:**
```powershell
cd 'C:\Users\thana\Desktop\Durango Opencode'
git status --short
dotnet build server\DurangoServer.csproj -c Debug -v quiet
```

**เปิด local server:**
```powershell
cd 'C:\Users\thana\Desktop\Durango Opencode\server'
$wd=(Get-Location).Path
Start-Process (Join-Path $wd 'bin\Debug\net9.0\DurangoServer.exe') -ArgumentList '--enable-cheat','--admin','gm' -WorkingDirectory $wd
```

**ตรวจผู้เล่นออนไลน์:**
```powershell
curl http://127.0.0.1:8190/admin/players
```

**ไฟล์สำคัญ:**
- `server/ServerCore/ServerConfig.cs` — `StatusEffectConfig`, `CraftMenuConfig`
- `server/ServerCore/ServerPlayer.Group2.cs` — mapping food effects และ status hooks
- `server/ServerCore/ServerPlayer.SkillEffects.cs` — `StaminaCostScale()`
- `server/ServerCore/ServerPlayer.Survival.cs` — `TickSurvival()` สำหรับ life/poison
- `server/ServerCore/ServerPlayer.Skills.cs` — filter blueprint/recipe ตามสกิล + `UnlockedActions()`/`IsActionUnlocked()`
- `server/ServerCore/ServerPlayer.Combat.cs` — `HandleUseBattleAction` (skill check) + `HandleGetActions` (กรองท่า)
- `server/ServerCore/ActionUnlockData.cs` — สกัดจากเกม: ท่าพื้นฐาน 27 + สกิลที่ให้ท่า 14 ตัว
- `server/ServerCore/RecipeUnlockData.cs` — สกัดจากเกม: สูตรพื้นฐาน 219 + สกิลที่ให้สูตร
- `server/data/config.json` — ค่า `StatusEffects` และ `CraftMenu`

**ข้อควรระวัง:** `tools/click.ps1` ต้องใช้ foreground click; การส่ง `PostMessage` ไม่ทำงานกับ Unity client นี้. ไฟล์ generated จาก `scripts/extract_*.py` ห้ามแก้มือ.

---

>
> ### 1) บัฟ/ดีบัฟจากอาหารมีผลจริงแล้ว (เดิมขึ้นแค่ไอคอน) — เทสในเกมสดผ่าน
> 18 บัฟจากข้อมูลเกมจริง จับเป็น 4 กลไก (ทิศทางบัฟ/ดีบัฟยึดจากไอเทมจริงใน `FoodData.cs` ไม่เดา):
> - บัฟสตามินา 13 ตัว (energetic/stamina_up/drink_water ฯลฯ) → ทำงานเปลืองสตามินาน้อยลง ~10%
> - ดีบัฟสตามินา 3 ตัว (thirsty/eat_bizarre_food/drunk) → เปลืองมากขึ้น ~8%
> - `life_up` → เลือดฟื้น +1/วิ · `poisoning` → เลือดไหล -1/วิ
> - เทสสด: poisoning HP 108→95 · life_up 95→108 (เต็มแล้วหยุดเอง) ✅
> - โค้ด: `StatusEffectConfig` ใน `ServerConfig.cs` (`config.json → StatusEffects`), classification +
>   `StatusStaminaCostDelta()`/`StatusLifeVelocityDelta()` ใน `ServerPlayer.Group2.cs`, hook ที่
>   `StaminaCostScale()` (SkillEffects) + `TickSurvival()` (Survival) · cheat ทดสอบ: `effect <id> [วิ]`
> - **หมายเหตุ:** เดิมวางแผนให้ drunk ลดความแม่นยำ แต่โค้ดต่อสู้ตีเข้าเสมอ (ไม่มีระบบพลาด) ความแม่นยำ
>   เป็นแค่เลขโชว์ เลยเปลี่ยน drunk เป็นดีบัฟสตามินาแทน · ดู `docs/server/Status-Effects-Report.md`
>
> ### 2) เจอสาเหตุจริง "บังคับสร้างตัวใหม่" อีกชั้น — IP loopback ไม่ตรงสตริง
> `POST /accounts` ผ่าน `localhost` (=IPv6 `::1`) คืนรายการ**ว่าง** แต่ผ่าน `127.0.0.1` คืนตัวละครครบ —
> client ต่อมาทาง loopback ที่ .NET มองเป็นคนละสตริงกับที่ account จองไว้ (`127.0.0.1`) → หน้าเลือกตัวว่าง
> เด้งไปสร้างใหม่ · แก้: เพิ่ม `AccountStore.NormalizeIp()` (`::1`/`::ffff:x` → IPv4) ใช้ทั้ง `TryClaim`
> + `FindByIp` · **ยืนยัน**: `/accounts` ผ่าน localhost คืน desgvz แล้ว
>
> ### 3) ซ่อน 10 แท็บในเมนู "สร้าง" ให้เหลือแค่ admin (เจ้าของวงกลมในรูป) — เทสสดผ่าน
> ซ่อน: Installation/Decoration(`deco_and_installation`) · Other(`etc`) · Clan(`clan`) ·
> Rest/Shelter(`residence`) · Transportation Facility(`traffic`) · Storage/Workbench(`furniture_and_workbench`) ·
> Snare/Trap(`trap`) · Wood(`plant_collectible`) · region · building/furniture — คงไว้: Clothing ·
> Building Components(`modular_attach`) · Weapon · Farmland/Road(`tile`) · Fence/Gate(`border`)
> - แมพชื่อแท็บ→หมวดยืนยันจาก localization เกาหลีจริง (นา무=Wood=plant_collectible ฯลฯ ไม่เดา)
> - กลไก: client โชว์แท็บก็ต่อเมื่อหมวดมีของ `Available` ≥1 (ดู client `RecipeSelectorGroup`) → server
>   ไม่ส่ง blueprint 471 ไอดีในหมวดพวกนี้เข้า unlocked list ของ non-admin แท็บเลยหายเอง · admin ได้ครบ
> - โค้ด: `RecipeData.HiddenBuildTabBlueprints`/`IsHiddenBuildTabBlueprint` + filter ใน `BuildUnlocked`
> - **เทสสด (ตัวละคร lvl1 non-admin)**: แท็บที่ซ่อนหายครบ เหลือเฉพาะที่คงไว้ ✅
>
> ### 4) คลายปม "ทำไมไม่ auto-connect" + game/ ไม่ได้พังจริง
> auto-connect มีอยู่แล้ว — ตั้ง `DURANGO_AUTOCONNECT=127.0.0.1` ตอนเปิด (ผ่าน `เล่นเกม.bat`/
> `launch-autoconnect.bat`) เข้าโลกตรงไม่ต้องกดเมนู · เช็ค hash แล้ว asset ใน `game/` เหมือน official
> (`Original_Game/Durango_Ver_PC_Final`) เป๊ะทุกไฟล์ (level2 ที่ log ว่า corrupted เป็น warning ไม่ตาย)
> — **ไม่ต้อง rebuild จาก official** มีแค่ `Assembly-CSharp.dll` ที่ patch (build จาก `client/`)
>
> **สถานะ**: build ผ่าน 0 error · เซิร์ฟทดสอบ `local-test-server9.log` (PID รันอยู่) · client `game/` +
> `DURANGO_AUTOCONNECT=127.0.0.1` เข้าเกมได้ · ทั้ง 3 งานหลักเทสสดผ่านหมด · **ยังไม่ deploy production**
> (ยึดกฎเดิม — เทส local ก่อนเสมอ)

---

**อัปเดตก่อนหน้า:** 25 ส.ค. 2026 — **ไล่เช็คเมนูคราฟทีละแท็บกับเจ้าของ เจอของอีเวนต์/ระบบ/เชทหลุดเพิ่มอีก 3 จุด (s02_* ทั้งชุด, Universal Workbench Cheat, System/Cheat แท็บ) แก้ครบตามที่เจ้าของสั่งแล้ว**
>
> ### ไล่ทีละแท็บในเมนู Craft/Build เจอบั๊กเพิ่ม 3 จุด
> - **ของอีเวนต์อีก 39 รายการ** (`s02_*` prefix — ฤดูกาล 2 — 20 blueprint + 17 recipe ที่ไม่เคยเช็คมาก่อน
>   เพราะ pattern เดิมมีแค่ "season2" ไม่ใช่ "s02_") + 2 ไอดีพิเศษชื่อไม่ส่อ (`camp_radio_station_02`,
>   `statue_03_a`) — เพิ่ม pattern `s02_` และ exact-match 2 ตัวนั้นแล้ว ยืนยันด้วยภาพจริง: แท็บ
>   "Specialty Crafting" หายไปทั้งแท็บ (ว่างสนิท)
> - **"Universal Workbench (Cheat)"** โผล่ตรงๆ ในแท็บ "Other" — ชื่อเต็มจริง "โต๊ะสารพัดประโยชน์ (ไว้
>   ใช้โกง)" เจอสัญญาณใหม่ (`subcategory: "system"`) พบอีก 3 อัน (`package`/`tutorial_boat`/
>   `tutorial_bonfire`) กรองออกหมดแล้ว
> - **แท็บ "System/Cheat"** (หมวด `system` จริงจากข้อมูลเกม — ชื่อทับศัพท์ NEXON เอง มีแค่สูตรย้อม/
>   ฟอกสี 6 อัน ไม่ใช่คำสั่งโกง) — เจ้าของสั่งซ่อนทั้งแท็บให้ admin เท่านั้นด้วย แก้แล้ว
>
> **ถามแล้วไม่เดา**: "Warp Gate Silo"/"Drop-off Point" ที่โผล่สว่างด้วย — เจ้าของยืนยันว่าเป็นของซ่อมได้
> ตามเควสจริง **ไม่ใช่บั๊ก คงไว้ตามเดิม**
>
> รายละเอียดเต็มที่ `docs/server/Skill-Gate-Fix.md` หัวข้อ "รอบ 5"
>
> **สถานะ**: build ผ่าน 0 error ทุกรอบ เซิร์ฟทดสอบรีสตาร์ตแล้ว (`local-test-server19.log`) พร้อมให้ทดสอบ
> ต่อ — ยังเหลือแท็บที่ยังไม่ได้ไล่ (Cooking/Farmland-Road/Fence-Gate/Installation-Decoration/Clan/
> Rest-Shelter/Transportation Facility) ถ้าอยากให้ไล่ต่อบอกได้เลย

**อัปเดตก่อนหน้า:** 25 ส.ค. 2026 — **เจอสาเหตุจริงของ "บังคับสร้างตัวใหม่" (account slot count โกหกจำนวนจริง) + แก้ของอีเวนต์/พร็อพระบบที่หลุดเข้าเมนูเพิ่มอีก 2 รอบ — ยืนยันด้วยภาพจริงว่าตัวละครเดิม resume ถูกต้องแล้ว**
>
> ### 🎉 เจอสาเหตุจริงของ "ทำไมบังคับสร้างตัวใหม่ทุกครั้ง"
> `Gateway.cs` (`POST /accounts`) hardcode `player_slot_count: 7` เสมอ — เช็คโค้ด client
> (`PlayerSelectionSystem.cs`) แล้วพบว่าค่านี้ไม่ใช่ "จำนวนตัวละครที่มี" แต่คือ "จำนวนช่องที่ account ได้"
> ใช้เทียบกับจำนวนจริงที่ส่งมา — เซิร์ฟทดสอบสะสม account จาก IP เดียว (127.0.0.1) ไว้ถึง **80 อัน**
> (เทสมาหลายเดือน) ทำให้ `7 < 80` กลายเป็น "เกินโควตา" เสมอ ⇒ client fallback ไปหน้าสร้างตัวใหม่แทนที่จะ
> โชว์ตัวเลือก — แก้โดยเรียง `AccountStore.FindByIp()` ตามเล่นล่าสุดก่อน + ใช้ค่าจริงจากเกม
> (`player_slot_count=3, max=7` ตาม `client/Durango.Offline/Server.cs`) + ตัดรายการเหลือแค่ 3 ล่าสุด —
> **ยืนยันด้วยภาพจริง**: หน้า "Select Character" โผล่ถูกต้อง (3/3) เลือกแล้วเข้าเกมได้ "desgvz" Lv.4
> HP ตรงกับก่อนหน้าเป๊ะ ไม่ใช่ตัวใหม่
>
> ### ของอีเวนต์/พร็อพระบบหลุดเข้าเมนูเพิ่มอีก 2 จุด (เจอจากเจ้าของเทสจริง)
> - `IsEventRecipeCategory` (เช็คแค่หมวด) จับของอีเวนต์ในสูตรได้แค่ 7/24 (ที่เหลือ Category เป็น
>   "cook"/"weapon_and_tool" ปกติเป๊ะ) — เปลี่ยนเป็น `IsEventRecipe` เช็คทั้งหมวด+ชื่อ id จับครบ 24/24 แล้ว
> - พร็อพระบบ 39 อัน (camp_radio_station/camp_warehouse/camp_square_fire ฯลฯ — เจ้าของกด "Storage" ใน
>   เกมแล้วเห็นเองว่าหน้ารายละเอียดขึ้น "Player cannot build" แต่ยังกดสร้างได้) — เพิ่ม
>   `RecipeData.IsSystemOnlyBlueprint` กรองออกจากทุกคนรวม admin (คนละแบบกับของอีเวนต์)
> - เช็คแล้ว: "คราฟฟรีไม่ใช้วัตถุดิบ" ไม่ใช่ปัญหาฝั่งสูตร (219 สูตรฟรีทุกอันมีวัตถุดิบจริงกำกับ) — ฝั่ง
>   blueprint ระบบวางเองยังไม่หักวัตถุดิบเลยในทุกกรณี (ข้อจำกัด beta เดิม แยกเรื่องจาก "ของ admin")
>
> รายละเอียดเต็มที่ `docs/server/Skill-Gate-Fix.md` หัวข้อ "รอบ 4"
>
> **สถานะ**: build ผ่าน 0 error ทุกรอบ เซิร์ฟทดสอบรีสตาร์ตแล้ว (`local-test-server16.log`) ยืนยันแล้วในเกม
> จริง ยังไม่ได้ deploy ขึ้นเซิร์ฟบ้าน/อัป client release — ยังไม่ได้ไล่ท่าต่อสู้/ระบบเก็บของ/ล่าสัตว์/
> ปลูกผัก (เจ้าของขอไว้แต่บอกให้หยุดรอเทสก่อน)

**อัปเดตก่อนหน้า:** 25 ส.ค. 2026 — **รายการคราฟ/สร้างเปลี่ยนไปอ้างอิงระบบสกิลจริงล้วนๆ (AlwaysRecipes+Collect) แทนลิสต์คัดเอง + แก้บั๊กพักผ่อนไม่ลดความล้า (กองไฟจริงไม่ตรง filter) + เพิ่มเอฟเฟคเลเวลอัพ/เรียนสกิล — ยังไม่ได้ไล่ท่าต่อสู้**
>
> ### รายการคราฟ/สร้าง "อ้างอิงจากสกิลเท่านั้น" ตามที่เจ้าของสั่ง
> ไล่โค้ดเจอว่าโปรเจกต์มีระบบสกิลจริงอยู่แล้วครบ (`RecipeUnlockData.AlwaysRecipes`/`AlwaysBlueprints` —
> 219 สูตรฟรีจริงจากเกม + `BySkill`/`Collect()` — 501 สูตรต้องเรียนสกิลจริง + `AutomaticSkillData`/
> `EnsureAutomaticSkills()` — โหนด AUTO ที่ปลดตามความชำนาญหมวด) **แค่ไม่เคยถูกเสียบใช้เต็มที่** —
> `BuildUnlocked()` เดิมใช้ `config.json Starter.Recipes` (34/12 อันคัดเองสมัยเบต้า ไม่ผูกสกิลเลย) ตรงกับ
> ที่เจ้าของเจอ "ไอเทม tool หลายอย่างไม่ต้องเรียนสกิลก็โผล่" — เปลี่ยนฐานเป็น `AlwaysRecipes`/
> `AlwaysBlueprints` + เอาเกณฑ์ความสามารถที่ผมประมาณเอาเองรอบก่อน (`RecipeGateData`) ออกทั้งหมด (ซ้อนทับ
> ของจริง) — เช็ค `HandleCraft`/`HandleOccupyArtifactSite` ก็เปลี่ยนไปเช็ค membership ตรงๆ กับ unlocked
> set จริงแทน รายละเอียดเต็มที่ `docs/server/Skill-Gate-Fix.md` หัวข้อ "รอบ 3"
>
> ### แก้บั๊กพักผ่อนไม่ลดความล้า
> `ServerWorld.IsRestBlueprint()` เดิมเช็คแค่ชื่อมี "bonfire"/"campfire"/"tent"/"hammock" — **กองไฟตั้งต้น
> จริงชื่อ `camp_square_fire`** ไม่มีคำเหล่านี้เลยสักคำ! นั่งพักข้างกองไฟจริงเลยไม่เคยผ่านเงื่อนไข ความล้า
> ไม่มีทางลดได้จริงตามที่เจ้าของเจอ — ขยาย pattern เป็น fire/tent/hammock/bed/rest/grill (เช็คกับ
> blueprint จริงทั้ง 570 อันแล้วไม่มีตัวหลุดที่มีปัญหาจริงจัง)
>
> ### เพิ่มเอฟเฟค level-up / เรียนสกิล
> เจ้าของสังเกต "ท่าทางเอฟเฟคตอนเลเวลอัพก็ไม่มี ตอนนี้แสดงแค่ xp" — เดิมส่งแค่ `ExpGained`/`Statistics`
> (ตัวเลขขยับ) แต่ไม่เคยส่ง `Rewarded{LevelUpEffect}`/`Rewarded{SkillRewardEffect}` ที่ client
> (`AlarmGroup.cs`) รอรับเพื่อเล่นป๊อปอัพ/เอฟเฟคจริง — เพิ่มการส่งทั้งสองจุดแล้ว (`GainExp`/`HandleTrainSkill`)
>
> ### แก้ untrain สกิลแล้วรายการคราฟไม่หาย
> `HandleUntrainSkill` ไม่เคย push รายการคราฟใหม่ (จุดเดียวกับที่พลาดตอนแรกในรอบก่อน แค่ตกหล่น handler
> นี้ไป) — เพิ่ม `SendUnlockedRecipesAndBlueprints()` แล้ว
>
> **ยังไม่ได้ทำ**: เจ้าของย้ำ 2 รอบว่า "ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน" — ยังไม่ได้ไล่โค้ดคอมแบตฝั่ง
> client เลยว่าตัวเลือกท่าที่ใช้จริงอ้างอิง `_knownSkills` ถูกจุดไหม (เป็นระบบคนละส่วนกับ recipe gate)
> รวมถึงระบบเก็บของ/ล่าสัตว์/ปลูกผัก ที่เจ้าของขอให้ไล่ตรวจตามแนวทางเดียวกันด้วย — ยังไม่ได้เริ่ม
>
> **สถานะ**: build ผ่าน 0 error เซิร์ฟทดสอบรีสตาร์ตแล้ว (`local-test-server12.log`) ยังไม่ได้ deploy
> ขึ้นเซิร์ฟบ้าน/อัป client release
>
> ### ย้อนกลับไปลิสต์เบต้าดั้งเดิม (34 สูตร/12 แบบก่อสร้าง)
> เจ้าของสั่งย้อนกลับ (ดูตัวเลือกที่เลือกใน AskUserQuestion) หลังเจอว่า `StarterCuratedContent` (457/515,
> คัดตาม 3 แนวเล่น) เปิดช่องให้สงสัยเรื่อง "ขวานหาย" อีกครั้ง — `BuildUnlocked()` (`ServerPlayer.Skills.cs`)
> กลับไปใช้ `ServerConfig.Current.Starter.Recipes`/`Blueprints` (`data/config.json`) เหมือนก่อนเซสชันนี้เริ่ม
> **ยืนยันแล้วด้วยภาพจริงในเกม**: "Improvised One-Handed Axe" โผล่ปกติ (Max Level 19, Skill Category
> "Weapon/Tool", วัตถุดิบ Blade/Strap/Stick 0/1 ครบ 3 ช่อง) — `StarterCuratedContent.cs` (457/515) ยังอยู่
> ในโค้ดเผื่อกลับมาใช้ทีหลัง แค่ไม่ได้ต่อกับ `BuildUnlocked()` แล้วตอนนี้
>
> ### 📋 บันทึกสาเหตุ "ขวานหาย" ทั้ง 2 รอบไว้แล้ว
> เขียน `docs/server/Axe-Recipe-Fix-Log.md` สรุปสาเหตุที่ 1 (แก้แล้วถาวร — `RecipeListWidget.cs` ซ่อน
> สูตรที่วัตถุดิบไม่พอ) กับสาเหตุที่ 2 (กำลังไล่อยู่ — ทฤษฎีเจ้าของว่าเป็นระบบสกิล) พร้อมพิสูจน์ว่า
> `assembled_axe_one_01` อยู่ในทั้ง 3 ลิสต์ที่เคยลอง (34/12, 720/570, 457/515) เท่ากันหมด ⇒ ปัญหาไม่ได้
> อยู่ที่ลิสต์ปลดล็อกที่เลือกใช้แน่นอน
>
> ### ⚠️ เจอบั๊กใหญ่กว่าเดิมระหว่างทาง: ระบบสกิลไม่มีผลจริงในเกม
> เจ้าของยืนยันเอง: **"สกิลไม่แสดงผลกับตัวเกม ท่าต่อสู้ ของบางอย่างไม่ต้องปลดสกิลก็คราฟได้"** — แปลว่า
> ระบบสกิล (`_knownSkills`, `HandleTrainSkill`, `RecipeUnlockData.Collect`) ที่มีอยู่ในโค้ด **ไม่ได้ไปมีผล
> จริงกับ (ก) ท่าต่อสู้/คอมแบต และ (ข) การเช็คสิทธิ์คราฟบางจุด** — เพิ่งเริ่มไล่โค้ด (`ServerPlayer.Core.cs`
> line 228 เป็น path เติม skills จาก JSON โดยตรง น่าจะเป็นแค่ debug/admin path ไม่ใช่ flow ปกติ) **ยังไม่ได้
> สรุปสาเหตุ** — เป็นบั๊กแยกจากเรื่องขวาน/รายการคราฟที่ทำอยู่ รอเจ้าของบอกว่าจะให้ไล่ต่อเลยไหม
>
> **สถานะ**: build ผ่าน 0 error, เซิร์ฟทดสอบรีสตาร์ตแล้ว (`local-test-server9.log`) ด้วยลิสต์ 34/12 —
> ยังไม่ได้ deploy ขึ้นเซิร์ฟบ้าน/อัป client release เหมือนเดิม
>
> ### 1) จำกัดสูตรที่ MinLevel ≤ 30 (457 → 354 อัน)
> เจ้าของสั่ง "จำกัดแค่เลเวล 30" — `StarterCuratedContent.Recipes` ตัดเทียร์ 35-55 ออก 103 อัน (สูตรพวกนี้
> ยังปลดล็อกเองได้ปกติผ่านสกิลตอนเลเวลถึง ไม่ได้ปิดถาวร) — blueprints คงไว้ 515 เท่าเดิม (ไม่มีข้อมูล
> MinLevel ให้คัดตามแกนนี้)
>
> ### 2) ซ่อนสูตรที่ต้องใช้โต๊ะคราฟ จนกว่าจะยืนอยู่ที่โต๊ะนั้นจริง
> `client/Durango.UI/RecipeListWidget.cs` (`EnumerateItems`) — เพิ่ม `if (!IsFavorites && blocker ==
> CraftBlocker.Workbench) continue;` (รอบก่อนแก้แค่ `Materials` ให้โชว์จางๆ, รอบนี้ `Workbench` ให้ซ่อนไปเลย
> เหมือน `Materials` แบบเดิมก่อนแก้) — เมนูคราฟจะสั้นลงมากตอนไม่ได้ยืนอยู่ที่โต๊ะไหนเลย
>
> ### 3) ของอีเวนต์ให้ admin เท่านั้น — แก้ที่ server จริง (เจอว่า client-side hide ไม่พอ!)
> ไล่โค้ด server เจอว่า `HandleCraft`/`HandleOccupyArtifactSite`/`HandlePlaceCapsulatedArtifact`
> **ไม่เคยเช็ค unlocked set เลย** — เช็คแค่ level/วัตถุดิบ/โต๊ะ/เครื่องมือ/ตำแหน่ง ⇒ แก้ client หรือยัด
> packet ตรงๆ ยังคราฟ/วางของอีเวนต์ได้แม้ไม่ถูกปลดล็อกในเมนู — เพิ่ม `RecipeData.IsEventRecipeCategory`/
> `IsEventBlueprint` (ตรวจหมวด season2/recipe_book หรือชื่อ blueprint แพทเทิร์นอีเวนต์) แล้วเช็ค `IsAdmin`
> จริงในทั้ง 3 handler ผู้เล่นทั่วไปคราฟ/วางของอีเวนต์ไม่ได้แล้วไม่ว่าจะพยายามยังไง
>
> รายงานเต็ม (โค้ด+เหตุผลละเอียด) ต่อจากรายงานรอบแรกที่ `docs/server/Starter-Recipes-Report.md` หัวข้อ
> "รอบ 2"
>
> **สถานะ**: build ผ่านทั้ง server+client (0 error) · รีสตาร์ตเซิร์ฟทดสอบ (`local-test-server8.log`) +
> `build-client.ps1` ติดตั้ง DLL ใหม่แล้ว + เปิดเกมชี้ไป `127.0.0.1` ให้เจ้าของเช็คแล้ว — **ยังไม่ได้ deploy
> ขึ้นเซิร์ฟบ้าน/อัป client release** รอทดสอบ+ขออนุญาตก่อนเหมือนเดิม

**อัปเดตก่อนหน้า:** 24 ส.ค. 2026 — **คัดสูตรคราฟ/แบบก่อสร้างใหม่ตาม 3 แนวเล่น (สร้างบ้าน/ปลูกผัก/ล่าสัตว์) แทนการปลดล็อกครบ 720 อันที่รกเกินไป — เขียนรายงานเกณฑ์คัดไว้แล้ว ยังไม่ได้ deploy**
>
> ### 📋 คัดลิสต์ 720 สูตร / 570 แบบก่อสร้าง เหลือ 457 / 515 ตามคำสั่ง
> เจ้าของสั่ง: "คิดว่าไอเทมไหนจำเป็นสำหรับสร้างบ้าน/ปลูกผัก/ล่าสัตว์ เขียนรายงานมา แล้วแสดงแค่นั้นก่อน"
> (หลังพบว่าปลดล็อกครบ 720 อันจากรอบก่อนหน้าทำให้เมนูคราฟรกไปด้วยชุดคอสตูม/อาหารหรู/ของอีเวนต์เก่า) —
> คัดด้วยเกณฑ์: เอาทั้งหมวด `weapon_and_tool`(132)/`tool`(74)/`material_process`(85)/`modular_attach`(64)
> (แกนของ 3 แนวเล่นตรงๆ) + คัดหมวดย่อยจาก `cook`(เอา 59/152 — fire/water/preserve/ingredient เท่านั้น,
> ตัดเมนูหรู/ยา) + คัดจาก `clothing`(เอา 43/157 — novice/shoes/gloves/bag พื้นฐาน, ตัดชุดคอสตูม) —
> ตัดหมวด season2/event/system ทิ้งทั้งหมด — แบบก่อสร้างคัดด้วยชื่อ (ตัด halloween/xmas/army/compi ฯลฯ)
> เก็บ 515/570 อัน (ที่เหลือแทบทั้งหมดคือเฟอร์นิเจอร์/สิ่งปลูกสร้างพื้นฐานอยู่แล้ว)
>
> โค้ดใหม่: `server/ServerCore/StarterCuratedContent.cs` (เก็บลิสต์ 457+515 อัน) — เสียบใช้ใน
> `BuildUnlocked()` (`ServerPlayer.Skills.cs`) แทน `RecipeData.AllRecipeIds`/`AllBlueprintIds` เดิม
> รายงานฉบับเต็ม (ตาราง เกณฑ์ เหตุผลแต่ละหมวด) อยู่ที่ `docs/server/Starter-Recipes-Report.md`
>
> **สถานะ**: build ผ่าน (0 error) · รีสตาร์ตเซิร์ฟทดสอบในเครื่องแล้ว (`local-test-server7.log`) รอเจ้าของ
> เข้าเช็คเมนูคราฟจริง — **ยังไม่ได้ deploy ขึ้นเซิร์ฟบ้าน/อัป client ใหม่** รอขออนุญาตตามกติกาเดิม ถ้าเช็ค
> แล้วขาดของจำเป็นบอกชื่อมาได้เลย จะเพิ่มเข้า `StarterCuratedContent.cs` ตรงจุด

**อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (23:50) — **เจอสาเหตุจริงของบั๊ก "ขวานไม่โผล่" แล้ว (ไม่ใช่บั๊ก!) + ปลดล็อกสูตรครบ 720 อัน (จากเดิม 34) ให้คราฟได้ทุกอย่างเหมือนโหมดออฟไลน์ — ยังไม่ได้ deploy รอขออนุญาต**
>
> ### 🎉 เจอสาเหตุจริงของ "ขวานไม่โผล่ในเมนูคราฟต์" แล้ว — ไม่ใช่บั๊ก resources.assets เลย!
> ใส่ debug log เขียนไฟล์ตรง ๆ (`File.AppendAllText`) แทน `Debug.Log` ที่ไม่ยอมบันทึกก่อนหน้านี้ (สาเหตุ:
> ปิดเกมด้วย Force-kill ไม่ให้เวลา Unity flush log — รอบนี้ปิดแบบปกติผ่านปุ่ม X ของหน้าต่างแทน) ไล่ตาม
> ทีละขั้นจนเจอจุดจริง: **`RecipeListWidget.cs` (`EnumerateItems`) มีโค้ดที่ซ่อนสูตรที่ "วัตถุดิบไม่พอ"
> ออกจากลิสต์แบบเงียบ ๆ โดยตั้งใจ** (`if (!IsFavorites && blocker == CraftBlocker.Materials) continue;`
> — คอมเมนต์เดิมบอกตรง ๆ ว่า "วัตถุดิบไม่พอ → ซ่อน (เจตนาเดิม)") — ขวานจริงต้องใช้วัตถุดิบ 3 อย่าง
> (blade+เชือก+ด้าม) ไม่มีใครมีของครบเลยเลยไม่เคยเห็นมันโผล่ — **แก้โดยลบเงื่อนไข `continue` นี้ออก** ตาม
> ที่เจ้าของสั่ง (อยากให้เห็นสูตรครบเหมือนโหมดออฟไลน์) เทสยืนยันด้วยภาพจริง: search "axe" เจอ **"Improvised
> One-Handed Axe"** และ **"Improvised Two-Handed Axe"** ครบทั้งคู่
>
> ### ✅ ปลดล็อกสูตร/แบบก่อสร้างครบทุกอันที่เซิร์ฟรองรับ (720 สูตร, จากเดิมแค่ 34)
> ระหว่างเทสเจอว่า "ไอเทมไม่ครบเยอะมาก" เทียบกับโหมดออฟไลน์ของเกมต้นฉบับ — สาเหตุคือ
> `BuildUnlocked()` (`ServerPlayer.Skills.cs`) เดิมใช้ `ServerConfig.Current.Starter.Recipes`/
> `Blueprints` (ลิสต์คัดสรรเฉพาะเบต้าใน `data/config.json`, แค่ 34/12 อันจาก 720/? ที่รองรับจริง) —
> **เปลี่ยนมาใช้ `RecipeData.AllRecipeIds`/`AllBlueprintIds` แทน** (ลิสต์เต็มที่มีอยู่แล้วในโค้ด ใช้ตอบ
> `GetRecipes`/`GetArtifactBlueprints` อยู่แล้วเหมือนกัน) — ตอนนี้ตัวละครใหม่ทุกตัวปลดล็อกสูตรครบ **720
> อัน** ทันที (เทสยืนยันด้วย bot ตรงๆ) ของเดิมใน config.json ยังอยู่เผื่ออยากกลับไปคุมขอบเขตทีหลัง
>
> ### ✅ เพิ่มคำสั่งเทส `maxskills`
> `ServerPlayer.Cheat.cs` — พิมพ์ `maxskills` (ต้องมี `--enable-cheat`) จะตั้งเลเวลผู้เล่นเป็น 60 (max) +
> ปลดทุกสกิลในเกมที่เลเวล 60 ทันที ไม่ผ่านระบบแต้ม/ลำดับปกติ — ไว้เทสของที่ต้องใช้ tool/skill level สูงๆ
> โดยไม่ต้องไต่เลเวลเอง
>
> **เทสแล้ว**: `--gp-check` 45/45 ผ่านหมด ก่อน+หลังการเปลี่ยนแปลงทั้งหมด — **ยังไม่ได้ deploy ขึ้นเซิร์ฟบ้าน
> (`100.84.186.56`) หรืออัป client ใหม่เลย รอขออนุญาตเจ้าของก่อน** ตามกฎมาตรฐานของโปรเจกต์
>

**อัปเดตล่าสุด:** 24 ส.ค. 2026 (23:30) — **workaround บั๊กขวาน: สลับให้ใช้ "มีด" แทน "ขวาน" ทุกจุดที่เดิมต้องมีขวานเท่านั้น — deploy ขึ้นเซิร์ฟบ้านแล้ว พร้อมให้ผู้เล่นเทส**
>
> ### ลองสลับ resources.assets จากฐานเก่า (12 ส.ค., ยืนยันจากเจ้าของว่ามีขวานใช้ได้ตอนเปิดออฟไลน์) — ไม่ช่วย เลิกทางนี้แล้ว
> เทียบไฟล์ `.assets`/`level*` ทั้งหมดระหว่างฐานเก่า (`game-backup/`, 12 ส.ค.) กับฐานปัจจุบัน (23 ส.ค.)
> พบว่า **มีแค่ `resources.assets` ไฟล์เดียวที่ต่างกัน** (ไฟล์อื่นเหมือนกันเป๊ะ รวมถึงตัวที่แก้ crash
> เรื้อรังไว้) — ลองสลับ `resources.assets` เป็นตัวเก่าดู (backup ตัวปัจจุบันไว้ก่อน) ผลคือ:
> - คำเตือน "resources.assets is corrupted [Position out of bounds]" **ยังขึ้นเหมือนเดิม** (แถมมี
>   `level3` ขึ้นเพิ่มด้วย) — สรุปว่าคำเตือนนี้เป็น false-positive/ไม่แน่นอนของ Unity เอง ไม่ใช่ตัวชี้วัด
>   ความเสียหายจริง อย่าไปยึดเป็นหลักฐานอีก
> - **ขวานก็ยังไม่โผล่ในเมนูคราฟต์เหมือนเดิม** แม้ใช้ resources.assets ฐานเก่าที่เจ้าของยืนยันว่าเคยเห็น
>   ขวานตอนเปิดออฟไลน์ก็ตาม — แปลว่าอาจไม่ใช่แค่ resources.assets อย่างเดียวที่เกี่ยวข้อง (อาจต้องมี
>   client build/DLL ชุดเดียวกับตอนนั้นด้วย) จุดนี้ซับซ้อนเกินจะไล่ต่อตอนนี้
> **สลับ resources.assets กลับเป็นตัวที่เทสผ่านแล้วเรียบร้อย** (hash ตรงกับก่อนสลับ `ca09b06d...`)
> ไม่ทิ้งอะไรค้างไว้ในเครื่อง — เจ้าของสั่งเลิกไล่ทางนี้ ไปทาง workaround แทน
>
> ### ✅ Workaround: เปลี่ยนทุกจุดที่ต้องมี "ขวาน" เท่านั้น ให้ใช้ "มีด" แทนได้ด้วย
> เพราะคราฟต์ขวานเองใช้ไม่ได้ (known-issue ด้านบน) ผู้เล่นเลยไม่มีทางได้ขวานมาเลย ⇒ ทุกอย่างที่ต้องมีขวาน
> ก็ติดไปด้วย (โดยเฉพาะ **ตัดไม้** ซึ่งเป็นจุดบล็อกการเล่นทั้งระบบ) แก้ 2 จุด:
> 1. **`ServerPlayer.Gathering.cs`** (`ToolForPrototype`) — `wood_log`/`wood_bough`/`wood_bush` เปลี่ยน
>    จากต้องมี tag `axe` เป็น tag `knife` แทน (ของเดิมมีแค่ตัวนี้ที่บล็อกทั้งระบบเพราะไม่มี fallback เลย)
> 2. **`RecipeMeta.cs`** — สแกนทั้งไฟล์หา recipe ที่ต้องมี `axe_onehand_tool`/`axe_twohand_tool` **โดยไม่มี
>    `knife` เป็นทางเลือกอยู่แล้ว** เจอ 13 สูตร (board/board_bak/board_metal/pillar_metal/pillar_stone/
>    s02_board/s02_dry_rubber/s02_gloves ตระกูล/s02_shoes ตระกูล) — เพิ่ม `T("knife", <level เดียวกับ
>    axe>)` เข้าไปใน tools array ของทุกสูตร (ไม่ได้ลบ axe option ออก แค่เพิ่ม knife เป็นทางเลือกคู่กัน
>    พอแก้บั๊กขวานจริงแล้ว axe ก็ยังใช้ได้ปกติ ไม่ต้อง revert อะไร)
> เทสแล้ว: `--gp-check` 45/45 · `--multi-check` 9/9 ผ่านหมด ก่อน deploy ขึ้นเซิร์ฟบ้าน (`100.84.186.56`)
> — เช็ค `/entry` ตอบ 200 ปกติหลัง restart
>
> ⚠️ **ข้อควรระวัง**: แก้ฝั่งเซิร์ฟอย่างเดียว (การตรวจเครื่องมือจริงตอนกด/ตัดไม้) — ส่วน**ตัดไม้ (gathering)
> ไม่มีเมนูแสดงเงื่อนไขล่วงหน้า** เดินไปแตะแล้วเซิร์ฟตัดสินเลย ⇒ ใช้มีดตัดไม้ได้ทันทีไม่ต้องอัป client
> แต่ **เมนูคราฟต์ 13 สูตรที่แก้ (board/pillar/s02_*)** ตัวข้อความ "Work Axe" ที่โชว์ในลิสต์มาจากข้อมูล
> ฝั่ง client เอง (resources.assets) ไม่ใช่จากเซิร์ฟ — อาจจะยังโชว์ "Work Axe" เฉยๆ ไม่ขึ้น "Work Knife"
> เพิ่มให้เห็น แม้เซิร์ฟจะยอมรับมีดแล้วก็ตาม (ต้องเทสจริงว่าเกมให้กดคราฟต์ผ่านด้วยมีดไหมทั้งที่ UI ไม่โชว์)
> — **จุดสำคัญที่สุด (ตัดไม้) แก้ได้แน่นอนไม่มีเงื่อนไขนี้** ส่วน 13 สูตรคราฟต์เป็นโบนัสที่อาจต้องเทสเพิ่ม
>

**อัปเดตล่าสุด:** 24 ส.ค. 2026 (22:10) — **แก้บั๊ก Level=0 สำเร็จ (deploy แล้ว) + ไล่บั๊ก "ขวานไม่โผล่ในเมนูคราฟต์" ลึกมาก สรุปเป็น known-issue (resources.assets เสียหาย ไม่ใช่โค้ด)**
>
> ### ✅ แก้บั๊ก Level=0 สำเร็จจริง — deploy ขึ้นเซิร์ฟบ้าน + อัปโหลด client ใหม่แล้ว
> ดูรายละเอียด root cause เต็มที่หัวข้อก่อนหน้า (Gateway.cs `/sessions` เติม Level จากเซฟ แต่ gate ผิด
> เงื่อนไข) — เทสยืนยันด้วยภาพจริงแล้วว่าหน้า Character โชว์ "Lv. 1" ถูกต้อง (จากเดิม "Lv. 0")
> Client release ใหม่ `client-2026-08-24-2206` มีบั๊กนี้แก้ครบแล้ว (ของก่อนหน้า `-1856`/`-2008` ยังไม่มี
> การแก้นี้ อย่าแนะนำคนโหลดจาก tag เก่าพวกนั้น)
>
> ### 🐛 known-issue: สูตร "ขวาน" (`assembled_axe_one_01` และแนวโน้มสูตรอื่นๆ) ไม่โผล่ในเมนูคราฟต์
> ผู้เล่นรายงานว่าคราฟต์ขวานไม่ได้ — ไล่มาไกลมากแล้วก่อนสรุปว่าเป็น known-issue เก็บไว้ก่อน:
>
> 1. **เซิร์ฟยืนยันแล้ว 2 รอบว่าปลดล็อกสูตรนี้ให้ถูกต้อง** (bot test ตรงๆ ได้ `assembled_axe_one_01` ใน
>    45 สูตรที่ส่งมา — ไม่ใช่ปัญหาฝั่งเซิร์ฟ)
> 2. **โค้ด client ที่กรองว่าอะไรโชว์ในลิสต์** (`RecipeSelectorGroup.IsValidCategoryItem` →
>    `AddRecipeItems`) เช็คแค่ 3 อย่าง: `Available` (id match ตรงกับที่เซิร์ฟส่ง) → `MinLevel` (เพิ่งแก้ไป
>    แล้วข้างบน) → workbench ที่เลือกอยู่ (recipe นี้ไม่ต้องมี table เลยผ่านอัตโนมัติ) — **อ่านค่าจริงจาก
>    `resources.assets` มาเทียบแล้ว ทุกฟิลด์ตรงกับที่ server สมมติไว้หมด** (`min_level:1`,
>    `category:"weapon_and_tool"`, `workbench_tags:{}`, `tool_tags:{bare_hands:1}`) — ไม่มีจุดไหนใน
>    logic ที่ควรจะกรองออกเลย
> 3. **ใส่ Debug.Log ชั่วคราวเพื่อดูค่าจริงตอนรัน** (ลบออกแล้ว ไม่เหลือในโค้ด) — พบว่า **Debug.Log ทุก
>    อย่างในเกมหยุดถูกบันทึกลง log หลังจากจุดหนึ่งไปเลย** (ทดสอบด้วย log ที่รู้อยู่แล้วว่าทำงานแน่ๆ
>    อย่าง `[craftui] จัดตำแหน่งแล้ว` ก็หายไปเหมือนกัน) — เป็นสัญญาณว่าเกิดปัญหาระดับ engine ไม่ใช่ logic
>    เรา
> 4. **เจอสาเหตุที่เป็นไปได้จริง**: `game.log` มี Unity เตือนเองตรงๆ ตอนโหลดฉาก 2 ครั้ง:
>    ```
>    The file '...resources.assets' is corrupted! Remove it and launch unity again!
>    [Position out of bounds!]
>    ```
>    เช็ค hash/ขนาดไฟล์เทียบกับ `game-backup/` (12 ส.ค.) พบว่า**ไม่ตรงกันเลย** (ขนาดต่างกัน ~96KB, hash
>    คนละอัน) — ตรงกับ note ที่มีอยู่แล้วว่า "23 ส.ค. เปลี่ยนฐาน game/ ใหม่ (แก้ crash เรื้อรัง)" คือมีการ
>    สลับไฟล์ฐานไปแล้วก่อนหน้านี้ ไม่ใช่พังวันนี้ แต่ตัวไฟล์ที่สลับมาอาจมีความเสียหายบางส่วนติดมาด้วย
>
> **สรุป**: มีความเป็นไปได้สูงว่า resources.assets เสียหายบางส่วนจริง ทำให้บาง object (อาจรวมถึงสูตร
> ที่ซับซ้อนกว่าอย่างขวาน ซึ่งมี 3 ช่องวัตถุดิบ ไม่ใช่ 1 ช่องแบบสูตรทั่วไป) โหลดไม่ครบเงียบๆ — **ไม่ใช่
> อะไรที่แก้ด้วยการเขียน C# ต่อได้อีกแล้ว** ต้องดึง/แพตช์ resources.assets จากแหล่งที่สะอาดกว่านี้ เป็นงาน
> คนละขนาดจากที่ทำในเซสชันนี้ — **เจ้าของสั่งเก็บเป็น known-issue ไว้ก่อน** ของอื่นในเมนูคราฟต์ทำงานปกติ
> ไม่บล็อกการเล่นโดยรวม
>
> **เจอของแถมระหว่างขุด** (ไม่ใช่สาเหตุ แต่เป็นบั๊กจริงอีกจุด ยังไม่ได้แก้): สูตรขวานจริงตามข้อมูลเกม
> ต้องใช้ **3 ช่องวัตถุดิบ** (blade + เชือก + ด้าม) ไม่ใช่แค่หิน 1 ช่องอย่างที่ `server/ServerCore/
> RecipeMeta.cs` สมมติไว้ — ถ้าสูตรนี้โผล่มาให้กดคราฟต์ได้จริงในอนาคต การคราฟต์อาจไม่ตรงกับที่ตั้งใจไว้
>

**อัปเดตล่าสุด:** 24 ส.ค. 2026 (20:10) — **แก้บั๊กใหญ่: เซิร์ฟบังคับสร้างตัวละครใหม่ทุกครั้งที่ปิด-เปิดเกม — แก้แล้ว เทสผ่านจริงด้วยภาพหน้าจอ**
>
> ### 🐛→✅ บั๊กจริง: `/accounts` ไม่เคยมี endpoint จริงฝั่งเซิร์ฟเลย — client ใช้ตัวแปรชั่วคราวแทนมาตลอด
> ผู้เล่นรายงาน "ต้องสร้างตัวละครใหม่ทุกครั้งที่เข้าเซิร์ฟ" — ไล่โค้ดเจอว่าตอนแก้บั๊ก "ค้างหน้า main"
> ก่อนหน้านี้ (ดูหัวข้อด้านล่าง) มีจุดที่ `ForceSetCluster()` (`Clusters.cs`) ตั้ง `OnRequestAccount` ให้ตอบ
> จาก **ตัวแปรในหน่วยความจำ** (`Durango.Offline.Server._localPlayer`) แทน เพราะตอนนั้นเซิร์ฟยังไม่มี
> endpoint `/accounts` จริงเลย (ของจริงต้องตอบว่า "IP นี้เคยสร้างตัวละครอะไรไว้บ้าง") — ตัวแปรนั้นอยู่ได้
> แค่ตอนเกมยังไม่ปิด พอปิดเกมแล้วเปิดใหม่กลับเป็นค่าว่างทุกครั้ง หน้าเลือกตัวละครเลยว่างเปล่า บังคับสร้างใหม่
> ตลอด **ทั้งที่ตัวละครเก่ายังอยู่ในเซฟเซิร์ฟจริงครบ ไม่ได้หายไปไหนเลย** — กระทบทุกเซิร์ฟที่เคยรันมา (VPS
> เดิมด้วย) ไม่ใช่บั๊กเฉพาะเซิร์ฟบ้าน แค่ไม่มีใครสังเกตเพราะปกติเทสสั้นๆ ไม่ได้ปิดเกมแล้วเปิดใหม่จริง
>
> **แก้ 2 ฝั่ง:**
> 1. **เซิร์ฟ** (`AccountStore.cs` + `Gateway.cs`) — เพิ่ม `AccountStore.FindByIp()` สแกน
>    `saves/accounts/*.json` หา entity id ที่เคยจองจาก IP นี้ (ใช้ mechanism เดิมที่มีอยู่แล้วจาก H-1
>    ป้องกันสวมรอย) แล้วเพิ่ม route `POST /accounts` ใน Gateway.cs ให้ตอบ JSON ตรงตาม schema ที่ client
>    ต้องการ (`players[]` พร้อม `player_entity_id`/`player_name`/`player_level`/`disconnected_at`)
> 2. **Client** (`Clusters.cs` + `TitleMenuUserControlBase.cs` + `TitleMenuGroup.cs`) — เอา
>    `OnRequestAccount` override (ตัวแปรชั่วคราว) ออก ให้ไหลไปเรียก `/accounts` จริงแทน — แต่เจอปัญหาใหม่
>    ทันที: โค้ดเดิมอ่านผลลัพธ์แบบ **sync ทันทีบรรทัดถัดมา** (ใช้ได้ตอนตัวแปรชั่วคราวตอบทันที แต่ HTTP
>    จริงเป็น async ตอบช้ากว่านั้น) ⇒ เห็นค่าว่างเสมอ แก้โดยเพิ่ม `RequestAccountAsync()` ใหม่ที่รอ
>    callback จริงก่อนตัดสินใจ (`TitleMenuUserControlBase.cs`) แล้วแก้ `TitleMenuGroup.cs` ให้ตัดสินใจ
>    "มีตัวละครไหม" ข้างใน callback แทนที่จะอ่านทันที
>
> **เทสยืนยันแล้วด้วยภาพหน้าจอจริง**: curl `/accounts` ตรงๆ ได้ 3 ตัวละครที่เคยสร้างจาก IP นี้ถูกต้อง →
> build client ใหม่ → เปิดเกมจริงชี้ไปเซิร์ฟบ้าน → **ขึ้นหน้า "Select Character" 3/7 ตัว** (ไม่ใช่หน้า
> สร้างเกาะใหม่แบบก่อนแก้) — ปิดเกมแล้วเปิดใหม่ก็ยังเห็นครบ ไม่รีเซ็ตแล้ว
>
> ⚠️ **จุดคอสเมติกเล็กน้อยที่ยังไม่ได้ทำ** — การ์ดตัวละครโชว์ "Lv. 0" / "#0000 kHz" / "Unknown" เกาะ (ค่า
> placeholder default ของ client เพราะ `/accounts` response ที่ทำไว้ส่งแค่ level/ชื่อ/entity_id ยังไม่ได้
> เติม field ความถี่วิทยุ/ชื่อเกาะบ้าน) **ไม่กระทบการเล่นจริง** เป็นแค่ตัวเลขโชว์บนการ์ดเลือกตัวละคร ถ้า
> อยากแก้ให้ตรงต้องดูว่า client อ่านค่าพวกนี้จาก field ไหนใน PlayerInfo/Account เพิ่ม
>
> **✅ Repackage + อัปโหลดแก้แล้วเสร็จสมบูรณ์** — release ใหม่ `client-2026-08-24-2008` (repo
> `SuperCodeTH/Durango-TH-Client`, ตั้งเป็น Latest แล้ว) มี client ที่แก้บั๊กนี้ครบ ยืนยัน manifest
> resolve ถูกต้อง — เจอปัญหาย่อยระหว่างแพ็ก: โฟลเดอร์ `dist/DurangoTH` เดิมโดน lock ค้าง (ไม่รู้สาเหตุแน่ชัด
> ลบไม่ได้แม้ folder จะว่างแล้ว) เลี่ยงด้วยการแพ็กไปที่ `dist3/` แทน ไม่ต้องแก้อะไรที่ระบบ — ถ้าเจอ lock
> แบบนี้อีกให้ลองเปลี่ยน `-Out` ของ `package-game.ps1` ไปโฟลเดอร์ใหม่แทนเสียเวลาน้อยกว่าไล่หา process
>

**อัปเดตล่าสุด:** 24 ส.ค. 2026 (19:30) — **VPS หลักล่ม (SSH/game port timeout ทั้งคู่) — ย้ายมารันที่เครื่อง `linuxserver` ที่บ้านชั่วคราวผ่าน Tailscale แล้ว เปิดใช้งานจริงแล้ว**
>
> ### ⚠️ VPS (`187.127.208.20`) ไม่ตอบสนองเลยทั้งเครื่อง — ต้องเช็ค Hostinger panel
> ตรวจแล้ว: port 22 (SSH) และ 8190 (game gateway) timeout ทั้งคู่ (ไม่ใช่แค่ SSH) — traceroute ไม่ตอบ
> สักhop เดียว — เป็นไปได้สูงว่า VPS ถูก suspend/ดับ ต้องเข้า hpanel.hostinger.com เช็ค/กด Start เอง (SSH
> เข้าไม่ได้ ผมช่วยจากตรงนี้ไม่ได้เลย) **ยังไม่ได้แก้ ณ ตอนบันทึกนี้**
>
> ### ✅ ย้ายมารันที่บ้านผ่าน Tailscale ชั่วคราว — ใช้งานได้จริงแล้ว
> เครื่อง `linuxserver` (Tailscale IP `100.84.186.56`, user `vibespell`) เดิมเป็น home server hub มี
> WordPress/Portainer/Filebrowser/AdGuard/Jellyfin/Uptime Kuma รันอยู่แล้ว — **สั่งหยุด Portainer +
> Filebrowser ชั่วคราว** (2 ตัวที่อันตรายสุดถ้าเพื่อนเข้าถึง — Portainer คุม Docker เทียบเท่า root,
> Filebrowser เปิดไฟล์ตรงๆ) ด้วย `docker stop portainer filebrowser` — เปิดกลับได้ด้วย `docker start ...`
> **บทเรียนสำคัญ — ufw ใช้กันพอร์ตบน `tailscale0` ไม่ได้จริง**: ตั้ง `ufw deny in on tailscale0` (เว้น
> 8190/8191) ไปแล้วดูเหมือนถูกต้อง (`ufw status` โชว์ rule ถูก) แต่ SSH ยังทะลุเข้าได้จริง — เช็คด้วย
> `iptables -L INPUT` พบว่า Tailscale สร้าง chain `ts-input` ของตัวเองอยู่**ลำดับที่ 1 ก่อน ufw ทุกอัน**
> ในเครื่อง — ACCEPT ไปแล้วก่อนจะถึงกฎ ufw เลย **ต้องคุมที่ Tailscale ACL (Access Controls ในเว็บ
> console) เท่านั้น ไม่ใช่ host firewall** — เขียนนโยบาย ACL ที่ถูกต้องไว้ให้แล้วแต่ยังไม่ได้ apply (ดู
> ด้านล่าง) เพราะสุดท้ายใช้ทางลัด "หยุด container อันตราย 2 ตัว" แทนก่อนเพราะเร่งด่วน (ปลอดภัยพอสำหรับตอนนี้
> แต่ไม่ใช่การแก้ที่สมบูรณ์แบบ — SSH ยังเข้าถึงได้ทางทฤษฎีถ้ามีคนรู้/เดา password ได้ ซึ่งเพื่อนไม่มีทางรู้)
>
> **นโยบาย Tailscale ACL ที่ถูกต้อง (ยังไม่ได้ apply จริง — ต้องทำในเว็บ console เอง เป็น security setting)**:
> ```json
> {
> 	"tagOwners": { "tag:durango": ["autogroup:admin"] },
> 	"acls": [
> 		{"action": "accept", "src": ["autogroup:member"], "dst": ["*:*"]},
> 		{"action": "accept", "src": ["*"], "dst": ["tag:durango:8190,8191"]}
> 	]
> }
> ```
> ต้องแก้ที่ Policies → General access rules → Edit as text ใน console.tailscale.com + ติด tag
> `tag:durango` ให้เครื่อง `linuxserver` ด้วย (Machines → `⋯` → Edit ACL tags)
>
> ### เซิร์ฟที่บ้านตอนนี้
> - Build ล่าสุดวันนี้ (มีบั๊กที่แก้ทั้งหมดในเซสชันนี้ครบ: generator, admin token, broadcast, prologue-skip,
>   deferred-queue/stamina fix) รันอยู่ที่ `~/durango/linux-x64/DurangoServer` บนเครื่อง `linuxserver`
> - Flags: `--data ~/durango/data --saves ~/durango/saves --public-host 100.84.186.56 --name "Durango (Home)"
>   --admin-token <token ใหม่ แยกจาก VPS>` — **ไม่มี `--enable-cheat`** (ของจริง ไม่ใช่โหมดเทส)
> - เทสแล้ว: `/entry` ตอบ 200 พร้อม IP ถูกต้อง, `/admin/*` บล็อก 401 ไม่มี token / 200 มี token
> - พบโฟลเดอร์ `~/durango/` เดิมมีของทดลองเก่าค้างจากก่อนหน้า (15 ส.ค., เคยลองผ่าน Cloudflare Tunnel แต่
>   login cert ล้มเหลว) — `saves/`/`data/` เดิมมีแค่ test data ไม่ใช่ผู้เล่นจริง ใช้ต่อได้ปลอดภัย ระวังอย่าเผลอ
>   ทับไฟล์ `server.log` เก่าที่เป็นของ root (permission denied) ใช้ path log ใหม่แทนแล้ว
> - **Docker build ล้มเหลว** (ตั้งใจจะรันใน container เพื่อ isolate เพิ่ม) — DNS ของเครื่องนี้ (ผ่าน Tailscale
>   MagicDNS → AdGuard ในเครื่องเดียวกัน) resolve `mcr.microsoft.com` ไม่ได้ (SERVFAIL) ไม่ใช่ปัญหาที่เรา
>   ควรไปแก้ (อาจกระทบ DNS บ้านทั้งหลัง) — เลยรันตรงบน host แทน ไม่ใช้ Docker
>
> ### Client (GitHub Release) อัปเดตแล้วให้ชี้มาที่บ้านแทน VPS
> Release ใหม่ `client-2026-08-24-1856` (repo `SuperCodeTH/Durango-TH-Client`, ตั้งเป็น Latest) —
> `server.txt` ในชุดชี้ไปที่ `100.84.186.56` แล้ว — **auto-updater ไม่แก้ `server.txt` ให้คนที่ติดตั้งอยู่แล้ว**
> (design ตั้งใจกันไฟล์ตั้งค่าโดนทับ) ⇒ **คนที่เคยโหลด/เล่นอยู่แล้วต้องแก้ `server.txt` เป็น `100.84.186.56`
> เองด้วยมือ** — เขียนโพสประกาศเต็มรูปแบบไว้ที่ `docs/operations/ANNOUNCE-tailscale.md` แล้ว (มีวิธีสมัคร Tailscale +
> รับสิทธิ์ + ตั้งค่าเป็นขั้นตอน) ลิงก์รับสิทธิ์: `https://login.tailscale.com/admin/invite/uVPapaMPTtH7N2TBMssQ11`
> (แบบ reusable ใช้ได้หลายคน)
>
> ### ทำต่อเมื่อมีเวลา
> - Apply Tailscale ACL ที่เขียนไว้ให้ข้างบน (ปิดช่อง SSH ให้สมบูรณ์ ไม่ใช่แค่หยุด container เสี่ยง 2 ตัว)
> - เช็ค Hostinger panel กู้ VPS กลับมา — ถ้ากลับมาได้อาจย้ายกลับ VPS แล้วเลิกพึ่งเครื่องบ้าน
> - พิจารณาต่อ Docker isolation ใหม่ (โหลด base image จากเครื่องอื่นที่ DNS ปกติ แล้ว `docker load` แทน pull)

**อัปเดตล่าสุด:** 24 ส.ค. 2026 (17:07) — **อัปโหลดชุดแจกใหม่ที่แก้บั๊ก auto-update แล้วขึ้น GitHub Release จริง (client-only repo) — พร้อมให้เพื่อนโหลดได้เลย**
>
> ### ✅ package + อัปโหลด GitHub Release เสร็จสมบูรณ์
> รัน `tools\package-game.ps1 -Ip 187.127.208.20 -Version 2026-08-24-1701 -ReleaseTag client-2026-08-24-1701`
> เต็มขั้นตอน (rebuild client จากซอร์ส → เช็ค SHA256 DLL ตรงกับซอร์สผ่าน → build DurangoUpdater ตัวที่แก้
> บั๊กโฟลเดอร์ห่อแล้ว → zip 0.82GB) ยืนยันก่อนอัปว่า `DurangoUpdater.exe` ในตัว zip ตรง (SHA256 ตรงกับ
> ตัวที่ผ่านการเทสจริงรอบก่อนหน้าเป๊ะ) แล้วอัปขึ้น repo แยก `SuperCodeTH/Durango-TH-Client` (client-only
> ไม่มีซอร์ส) ด้วย `gh release create client-2026-08-24-1701` — อัปทั้ง `DurangoTH.zip` + `manifest.json`
> **ตรวจ URL `releases/latest/download/manifest.json` จริงแล้ว resolve ถูกต้อง** (GitHub เลื่อน release
> ใหม่เป็น "Latest" ให้อัตโนมัติเพราะใหม่กว่า) — เพื่อนที่โหลดชุดใหม่นี้จะได้ auto-update ที่ใช้งานได้จริงแล้ว
>
> ⚠️ **release เก่า `client-20260824` (22 คนโหลดไปแล้วก่อนหน้า) ไม่มี `DurangoUpdater.exe` เลย** (เป็นชุด
> manual-only ก่อนระบบ auto-update จะมีด้วยซ้ำ) — คนที่โหลดชุดนั้นไปจะ**ไม่มีทางได้อัปเดตอัตโนมัติ**
> ต้องแจ้งให้โหลดซ้ำจากลิงก์ release ใหม่ (`client-2026-08-24-1701`) เอง อย่างน้อยครั้งเดียว
>
> **ยังไม่ได้แตะ VPS จริงเลยทั้งเซสชันนี้** — งานที่ค้างขึ้น VPS ยังเหมือนเดิม (บั๊กเก็บของ/generator, admin
> token, broadcast, ปิดฉากรถไฟ, deferred-queue/stamina fix) ต้องขอก่อนทุกครั้งตามกฎที่ตั้งไว้

**อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (17:00) — **เทสระบบออโต้อัพเดทด้วยของจริง (zip จริง 800MB จาก dist2/) เจอบั๊กร้ายแรง 1 จุด แก้แล้ว เทสซ้ำผ่าน 100%**
>
> ### 🐛→✅ บั๊กจริง: auto-update ไม่เคยทำงานได้เลยสักครั้ง เพราะ zip แจกจริงมีโฟลเดอร์ห่อชั้นนอก
> เทสรอบก่อนหน้าใช้ notepad.exe จำลอง + zip แบนราบ (ไม่มีโฟลเดอร์ห่อ) เลยไม่เจอบั๊กนี้ รอบนี้เทสด้วย
> **zip ของจริงที่ build จาก `package-game.ps1`** (`dist2/DurangoTH.zip`, 800MB) พบว่าโครงสร้างข้างในเป็น
> `DurangoTH\DurangoV2.exe` (มีโฟลเดอร์ `DurangoTH\` ห่ออีกชั้น) ไม่ใช่ `DurangoV2.exe` อยู่ที่ root ตรง ๆ
> `DurangoUpdater` เดิมเช็คแค่ `File.Exists(root + "DurangoV2.exe")` ตรง ๆ — หาไม่เจอทุกครั้ง แล้ว
> **ยกเลิกอัปเดตแบบเงียบ ๆ** (fallback ไปเปิดเวอร์ชันเดิมต่อตามดีไซน์ "ห้ามเปิดเกมไม่ได้") ⇒ ผลคือ
> **ระบบอัปเดตจะไม่เคยอัปเดตอะไรให้ใครเลยสักครั้ง โดยไม่มี error ให้เห็นเลย** (เงียบสนิท ผู้เล่นก็เข้าใจว่าเป็น
> เวอร์ชันล่าสุดตลอด) — เป็นบั๊กที่ร้ายแรงที่สุดในระบบนี้ เพราะทำให้ฟีเจอร์หลักใช้งานไม่ได้เลยแม้แต่ครั้งเดียว
> **แก้แล้ว** (`tools/Updater/Program.cs`): เปลี่ยนจากเช็ค root ตรง ๆ เป็นหา `DurangoV2.exe` แบบไล่ลึก
> (`Directory.GetFiles(extractDir, "DurangoV2.exe", SearchOption.AllDirectories)`) แล้วใช้โฟลเดอร์ที่เจอไฟล์
> เป็น source จริงสำหรับ `robocopy /MIR` — รองรับทั้ง zip แบนราบและ zip ที่ห่อโฟลเดอร์ไว้ชั้นเดียว
> **เทสซ้ำด้วยของจริงทั้งหมด** (ไม่ใช่จำลองแล้ว): เอา `dist2/DurangoTH` จริง (1.1GB) มาเป็นโฟลเดอร์ "ติดตั้ง
> เดิม" (`live/`) แล้วจงใจทำให้ต่างจาก zip จริง — ลบไฟล์ asset ทิ้ง 1 ไฟล์ (`globalgamemanagers.assets.resS`),
> วางไฟล์ขยะเก่าทิ้งไว้ 1 ไฟล์, ตั้ง `server.txt`/`AppData`/`AppData2` เป็นค่าจำลองของผู้เล่น — เสิร์ฟ
> `dist2/DurangoTH.zip` จริงผ่าน local HTTP server (python, 127.0.0.1:8079) พร้อม manifest.json ที่มี
> SHA256 ของจริง แล้วรัน `DurangoUpdater.exe` ตัวจริงเข้ากับ `live/` ตรง ๆ
> **ผลเทส (ผ่านทุกข้อ):** โหลด+ตรวจ SHA256 ผ่าน → แตกไฟล์เจอ `DurangoV2.exe` ในโฟลเดอร์ห่อถูกต้อง →
> `robocopy /MIR` คืนไฟล์ asset ที่ลบไปกลับมา (เช็ค SHA256 ตรงกับต้นฉบับเป๊ะ) → ลบไฟล์ขยะเก่าออกถูกต้อง →
> `server.txt`/`AppData`/`AppData2`/`update-manifest-url.txt` **รอดจากการโดนทับ/ลบทั้งหมด** → `version.txt`
> อัปเดตถูกต้อง → **เปิดเกมจริงสำเร็จ** (เช็ค process `DurangoV2` ขึ้นจริง, Mono init log ปกติ)
> **ทำความสะอาดโฟลเดอร์เทส (`test-autoupdate/`, `dist2/manifest.json`) เรียบร้อยแล้ว ไม่เหลือขยะ**
> **ยังไม่ได้อัป VPS / ยังไม่ได้แพ็กชุดแจกจริงพร้อม updater ตัวใหม่นี้** — ต้องรัน
> `package-game.ps1 -ReleaseTag <tag>` ใหม่อีกครั้งเพื่อให้ได้ `DurangoUpdater.exe` เวอร์ชันที่แก้บั๊กนี้แล้ว
> ก่อนอัปโหลดขึ้น GitHub Release จริง (ชุดที่เคย build ไว้ก่อนหน้านี้ยังเป็นตัวที่มีบั๊กนี้อยู่)
>
> **อัปเดตก่อนหน้า:** 24 ส.ค. 2026 — **แก้บั๊กที่ agent ตรวจเจอทั้ง 2 จุดแล้ว (deferred queue เพดาน + stamina คืนเมื่อล้มเหลว) — build ผ่าน 0 error — ยังไม่ได้อัป VPS รอขออนุญาต**
>
> ### ✅ แก้บั๊ก "deferred queue ไม่มีเพดาน" (5 handler) + "stamina ไม่คืนตอนล้มเหลว" (4 จุด) — build ผ่านแล้ว
> ทำตาม pattern เดียวกับ `HandleBuildArtifact`/handler อื่นที่เช็คถูกอยู่แล้ว: เติม
> `if (_deferred.Count >= MaxPendingActions) { ...Abort...; return; }` ก่อนจุด `_deferred.Add()` ทุกจุด —
> `HandleCollect`/`HandleButchery` (`ServerPlayer.Gathering.cs`), `HandleHarvest` (`ServerPlayer.Farming.cs`),
> `HandleUseBattleAction` (`ServerPlayer.Combat.cs`, เช็คก่อนหักสตามินา), `HandleCraft`
> (`ServerPlayer.Crafting.cs`, เช็คก่อนหักสตามินาเช่นกัน)
> ส่วนจุดหักสตามินาก่อนแล้วเช็คทีหลัง — เพิ่ม `RestoreStamina(cost, 0f)` ตรงจุดที่ล้มเหลว 4 จุด:
> `HandleCollect`/`HandleButchery` (จองไม่ทัน race), `HandleHarvest` (เก็บซากไม่ทัน),
> `HandleUseBattleAction` เดิมมีอยู่แล้ว (ยิงธนูไม่มีลูก), `HandleCraft`
> (วัตถุดิบหายไประหว่างรอ deferred คืนสตามินาด้วยแล้ว), `HandleOccupy`/build (`ServerPlayer.Building.cs`)
> — คืน 2 จุด: blueprint ไม่รู้จัก + วางทับที่คนอื่น (ก่อนหน้านี้หักไปแล้วไม่คืนเลยทั้งคู่)
> **`dotnet build -c Debug` ผ่าน 0 error** (มีแต่ warning nullability เดิมที่ไม่เกี่ยวกับจุดที่แก้)
> **ยังไม่ได้เทสด้วยบอทจริง/ยังไม่ได้อัป VPS** — เป็นแค่ compile-verify ตามที่แก้โค้ดตรงตาม pattern เดิมที่พิสูจน์
> แล้วว่าใช้ได้ (`HandleCollect`/`Butchery`/`Harvest` เทสจริงตอนแก้บั๊ก generator ผ่านมาแล้วก่อนหน้า)
> เรื่องกล่องเก็บของทุบแล้วของหายถาวร (finding #3) **ยังไม่แก้** — เป็น known limitation ที่ทีมรู้อยู่แล้ว
> ต้องทำฟีเจอร์ "ของตกพื้น" ใหม่ ไม่ใช่ bugfix ธรรมดา ไม่อยู่ในสโคปครั้งนี้
>
> **อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (10:15) — **สร้างระบบออโต้อัพเดท (DurangoUpdater) เสร็จ เทสกลไกผ่านหมดแล้ว + เจอบั๊กเพิ่ม 2 จุด (deferred queue ไม่มีเพดาน, stamina ไม่คืนตอนล้มเหลว) — ทั้งหมดยังไม่ได้อัป VPS**
>
> ### ✅ ระบบออโต้อัพเดท (แนวทาง C เต็มรูปแบบ) — เขียนเสร็จ เทสกลไกผ่านครบแล้ว
> โปรเจกต์ใหม่ `tools/Updater/` (.NET 9 console, publish self-contained win-x64 = ไฟล์เดียวไม่ต้องมี
> .NET runtime) — วางในโฟลเดอร์เกมข้าง ๆ `DurangoV2.exe`, `เล่นเกม.bat` เรียกตัวนี้แทน (มี fallback เปิด
> DurangoV2.exe ตรงถ้าไม่เจอ updater — เผื่อชุดแจกเก่า)
> **หลักการความปลอดภัย:** ไม่แตะไฟล์เกมจริงจนกว่าจะโหลด+ตรวจ SHA256+แตกไฟล์ลงโฟลเดอร์ชั่วคราวครบสมบูรณ์
> ก่อนเสมอ — สลับเข้าโฟลเดอร์จริงด้วย `robocopy /MIR` ท้ายสุดจุดเดียว (เว้น `AppData`/`AppData2`/
> `server.txt`/`update-manifest-url.txt`/`version.txt`/`game.log` ไม่ให้โดนทับ) เน็ตหลุด/hash ไม่ตรง/
> manifest โหลดไม่ได้ = ข้ามเงียบ ๆ เปิดเกมเวอร์ชันเดิมต่อ ไม่มีทางเปิดเกมไม่ได้เพราะอัปเดตพัง
> **เทสจริงด้วยชุดจำลอง** (notepad.exe แทน DurangoV2.exe, ตั้ง local HTTP server เสิร์ฟ manifest.json+zip):
> version 1→2 เทียบถูก → โหลด+ตรวจ hash ผ่าน → แตกไฟล์ → robocopy สลับ → version.txt อัปเดตถูก →
> AppData/server.txt **รอดจากการโดนทับ** → ไฟล์เก่าที่ไม่มีในเวอร์ชันใหม่ถูกลบถูกต้อง (mirror จริง) →
> เปิดเกมสำเร็จ **เจอบั๊กจริงระหว่างเทส**: `LaunchGame` ตั้ง `UseShellExecute=true` คู่กับ
> `EnvironmentVariables["DURANGO_AUTOCONNECT"]` — .NET ไม่ยอมให้ตั้งคู่กันแบบนี้ (throw
> `InvalidOperationException`) แก้เป็น `UseShellExecute=false` แล้วเทสซ้ำผ่าน
> `tools/package-game.ps1` เพิ่ม param `-Version`/`-ManifestRepo`/`-ReleaseTag`/`-SkipUpdater` — build+
> แพ็ก `DurangoUpdater.exe`+`version.txt`+`update-manifest-url.txt` เข้าชุดแจกอัตโนมัติ + สร้าง
> `manifest.json` แยกไว้อัปโหลดคู่กับ zip บน GitHub Release เดียวกัน (`gh release upload`)
> **ยังไม่ได้แพ็ก+แจกจริง** (ต้องรัน `package-game.ps1 -ReleaseTag <tag ที่จะใช้จริง>` แล้วอัปทั้ง 2 ไฟล์)
>
> ### 🐛 Agent ไล่หาบั๊กเพิ่มเติมทั่วระบบเกมเพลย์หลัก — เจอ 2 จุดใหม่ (อ่านโค้ดอย่างเดียว ยังไม่ได้แก้)
> 1. **`_deferred` queue ไม่มีเพดานใน 5 handler** (มั่นใจสูง กระทบทั้งเซิร์ฟ) — `HandleCollect`/
>    `HandleButchery` (`ServerPlayer.Gathering.cs:542,648`), `HandleHarvest`
>    (`ServerPlayer.Farming.cs:528`), `HandleUseBattleAction` (`ServerPlayer.Combat.cs:168`),
>    `HandleCraft` (`ServerPlayer.Crafting.cs:822`) เรียก `_deferred.Add()` โดยไม่เช็ค
>    `MaxPendingActions` ก่อน (ต่างจาก `HandleBuildArtifact`/farming ที่เช็คถูก) — คอมเมนต์ในโค้ดเอง
>    เตือนไว้ตรง ๆ ว่าไม่เช็คแล้ว "main loop ค้าง" ได้ — ผู้เล่นสแปม packet ใน 5 ระบบนี้ (~2-3 วิ ก่อนโดน
>    rate-limit เตะ) ยัด entry เข้าคิวได้เยอะ เสี่ยงกระทบผู้เล่นทุกคนพร้อมกัน (ไม่ใช่แค่คนสแปม)
> 2. **หัก stamina ก่อนเช็คเงื่อนไขที่ทำให้ล้มเหลวได้ทีหลัง ไม่มีคืน** (มั่นใจ กระทบเล็กน้อยแต่บ่อย) —
>    `ServerPlayer.Gathering.cs:518,625` (เก็บของ/แล่ซากไม่มีเครื่องมือ), `ServerPlayer.Building.cs:131`
>    (วางบ้านทับที่คนอื่น), `ServerPlayer.Crafting.cs:752` (ของหายระหว่างรอ deferred) — โค้ดจุดอื่นใน
>    ไฟล์เดียวกัน (`ServerPlayer.Combat.cs:141-147`) มี `RestoreStamina` ตอนยิงธนูไม่มีลูกศรอยู่แล้ว แสดง
>    ว่ารู้ pattern นี้ดี แค่ลืมใช้ใน 4 จุดนี้
> 3. ทุบกล่องเก็บของ → ของข้างในหายถาวร (`ServerWorld.cs:634-661`) — **ทีมรู้อยู่แล้ว มีคอมเมนต์ยอมรับใน
>    โค้ดเอง** ไม่ใช่บั๊กที่ซ่อนอยู่ ยกมาย้ำเพราะตอนนี้มีผู้เล่นจริงแล้ว
> ตรวจแล้ว **ไม่พบ** บั๊กประเภทเดียวกับที่เพิ่งแก้ไป (ลบทั้งกลุ่มทั้งที่ควรลบทีละชิ้น) เพิ่มเติมอีก —
> จุดอื่นที่แตะ `_generators` ทำถูกต้องหมดแล้ว
>
> **อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (09:20) — **แก้บั๊ก "เก็บของยังไม่หมด แต่ต้นไม้หายไปก่อน" (ยังไม่ได้อัป VPS — รอขออนุญาต)**
>
> ### ✅ แก้บั๊ก "เก็บของยังไม่หมด แต่ object หายไปก่อน" — ผู้เล่นจริงเจอ ยืนยันสาเหตุ+แก้+เทสแล้ว
> ต้นไม้บางชนิด (ดู `NaturalData.cs` type 14004/14005/14014/14017/14029) มี generator 2 ชนิดพร้อมกัน
> ("กิ่งไม้" + "ท่อนไม้") ใน `List<Generator>` เดียวกันของ tile เดียว — บั๊กอยู่ที่
> `ServerWorld.TryReserveGenerator()` (`server/ServerCore/ServerWorld.cs`): พอ generator ชนิดที่กำลังเก็บ
> (เช่น กิ่งไม้) ถึงหน่วยสุดท้าย โค้ดเดิมสั่ง `_generators.Remove(naturalId)` **ทั้ง tile ทันที** — ท่อนไม้ที่
> ยังไม่ได้เก็บเลยหายไปด้วย ตรงกับที่ผู้เล่นรายงาน "เก็บแค่กิ่งไม้ แต่ต้นไม้หายไปก่อน" เป๊ะ
> **แก้แล้ว** ให้ตรงตาม pattern ที่ `TryReserveCorpsePart` (ฟังก์ชันข้างล่างในไฟล์เดียวกัน) ทำถูกอยู่แล้ว:
> เอาออกแค่ generator ชนิดที่หมด (`RemoveAt`) แล้วค่อยเช็คว่า **ทุกชนิด** ในจุดนั้นหมดหรือยัง
> (`gens.Count == 0`) ถึงจะลบทั้ง tile จริง — เช็ค `_generators.Remove`/`RemoveAt` ทุกจุดในโค้ดแล้ว
> จุดอื่น (`ServerWorld.Farming.cs`, cleanup calls อื่นๆ) เป็น whole-removal ที่ถูกต้องอยู่แล้ว ไม่ใช่บั๊ก
> **เทสยืนยันแล้วในเครื่องนี้** ด้วยบอทจริง (tile 58,190 มีกิ่งไม้ x3 + ท่อนไม้ x2): เก็บกิ่งไม้ครบ 3 →
> touch ซ้ำ tile เดิม → **ยังอยู่** เหลือ "ท่อนไม้ x2" ตามที่ควรจะเป็น → เก็บท่อนไม้ต่อจนครบ →
> ข้อความ "(จุดนี้หมดแล้ว)" โผล่ถูกจังหวะ → touch tile เดิมอีกครั้งได้ `Abort` (tile หายจริงแล้ว)
> **ยังไม่ได้อัปขึ้น VPS** — บั๊กนี้กระทบผู้เล่นจริงตรงๆ (เก็บไม้แล้วเสียของฟรี) ควรอัปเร็วๆ นี้ แต่ต้องขอ
> เจ้าของก่อนตามกฎเหล็ก (ดู `docs/server/VPS-DEPLOY.md`)
>
> ### 📋 แผนระบบออโต้อัพเดท — วางแผนไว้แล้ว (ยังไม่ได้ลงมือทำ)
> ส่ง subagent ไปสำรวจโค้ด+วางแผน พบว่ามีฐานพร้อมใช้ 2 อย่างอยู่แล้วในเกม ไม่ต้องเขียนใหม่หมด:
> `NoticeSystem` (client) ทำงานสมบูรณ์แล้ว รอแค่ `/notice` (Gateway.cs) ตอบ URL จริงแทน `{}`, และ
> `/knock`'s `compatible`/`download_url` (เดิมมีอยู่แล้วในเกม NEXON) รอแค่เทียบ version ที่เซิร์ฟ
> เสนอ 3 แนวทาง A (แจ้งเตือนในเกม, เสี่ยงศูนย์, งานเล็ก) → B (บล็อกหน้าไตเติ้ลถ้าเวอร์ชันไม่ตรง, งานกลาง)
> → C (ดาวน์โหลด+แพตช์อัตโนมัติเต็มรูปแบบ, งานใหญ่หลายวัน, เสี่ยงสูง) **แนะนำทำ A+B พอสำหรับกลุ่มเพื่อน
> ขนาดนี้ ไม่แนะนำ C** รายละเอียดเต็มอยู่ในข้อความที่ agent รายงานกลับมา (ยังไม่ได้บันทึกเป็นไฟล์ถาวร —
> ควรเซฟเป็น `docs/server/AutoUpdate-Plan.md` รอบหน้าถ้าจะทำต่อ)
>
> **อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (08:45) — **⚠️ มีผู้เล่นจริงบน VPS แล้ว — ห้ามอัป/รีสตาร์ทเซิร์ฟจริงโดยไม่ถามก่อน (เทส local ก่อนเสมอ)**
>
> ดู **`docs/server/VPS-DEPLOY.md`** สำหรับวิธี SSH เข้า VPS/deploy/โครงสร้าง/cron — เขียนไว้ให้ agent อื่น
> อ่านต่อได้ง่าย

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> ### ✅ แก้บั๊ก "เกมปิดตัวเองตอนผู้เล่นใหม่สร้างตัวละคร" (ฉากรถไฟ/หนังเปิด) — เซิร์ฟสั่งข้ามได้แล้ว
> ผู้เล่นจริงเจอเกมปิดตัวเองกะทันหันตอนสร้างตัวละครครั้งแรก — log: `MediaPlayerCtrl: Could not open file`
> (ฉากรถไฟเต็มมีวิดีโอเล่นที่ไฟล์หายไปจาก asset bundle) เกมมีกลไก `PrologueManager.ToBeSkipped` อยู่แล้ว
> (เคยพิสูจน์ว่าข้ามได้ปลอดภัย — ดูคอมเมนต์เดิมในไฟล์) แต่ไม่เคยถูกเปิดใช้อัตโนมัติ
> **แก้แล้ว:** เพิ่ม `ServerConfig.SkipPrologueVideo` (default `true`, hot-reload ผ่าน `data/config.json`)
> ส่งผ่านทั้ง `/knock` และ `/entry` — client อ่านแล้ว set `ToBeSkipped` เอง **⚠️ ต้องอ่านที่ `/knock` ด้วย
> ไม่ใช่แค่ `/entry`** เพราะผู้เล่นใหม่ (ไม่มี PlayerId) ข้าม `GetFrontend`/`/entry` ไปเลย
> (`NPAGetUser → FadeOutPrologue` ตรง ๆ) — ลองแค่ `/entry` รอบแรกแล้ว "เหมือนได้ผล" (บังเอิญ เพราะ field
> static ค้างจาก process เดิม) พอเทสซ้ำด้วย process ใหม่จริง ๆ ถึงเจอว่าไม่ได้ผล ต้องเพิ่มที่ `/knock` ด้วย
> **เทสยืนยันแล้ว** (ล้าง saves+AppData ให้เป็นผู้เล่นใหม่จริง 100%): เข้าเกมตรงไปหน้า "Region" → "Gender/
> Profession" (เมนูเลือกอาชีพ ไม่มีวิดีโอ ไม่มีฉากรถไฟเดินเลือกผู้โดยสารเลย) — ไม่มี crash
> ยังไม่ได้อัปขึ้น VPS จริง (มีผู้เล่นออนไลน์อยู่ — ต้องขอเจ้าของก่อนเสมอ ดู `docs/server/VPS-DEPLOY.md`)
>
> ### ✅ เพิ่มปุ่ม "บรอดแคสต์" ในหน้า admin — ส่งข้อความให้ทุกคนที่ออนไลน์ได้ทันที
> `POST /admin/broadcast` (ต้องมี token เหมือน /admin/* อื่น) → `_world.Broadcast(new Info{Text=...})`
> ทุกคนเห็นเป็น popup ในเกม เทสผ่าน local (บอทได้รับข้อความจริง) ยังไม่ได้อัปขึ้น VPS
>
> ### ✅ แก้บั๊ก "เปิดเกมจากชุดแจกแล้วค้างหน้าไตเติ้ล" (regression จากการแก้ "เลือกโหมดได้จริง" เมื่อกี้)
> ตอนแก้ให้ mainUI เลือกโหมดได้จริง (ไม่บังคับออนไลน์) ผมเอา intercept ออกจาก
> `TitleMenuUserControlBase.OnConfirm()` ทั้งบล็อก — **แต่บล็อกแรก (เช็ค `Server.AutoConnectTarget`)
> ไม่ควรเอาออก** มันคือกลไกคนละเรื่องกับ "โหมดที่ผู้เล่นเลือกเอง": `AutoConnectTarget` เป็นค่าที่
> **operator ตั้งเอง** ผ่าน env `DURANGO_AUTOCONNECT` หรือ `server.txt` ในชุดแจก (`tools/dist-template`)
> — คือ build นี้ถูกบังคับให้ต่อเซิร์ฟเดียวเสมอ ไม่ใช่ผู้เล่นเลือกเอง ⇒ ต้อง intercept ทันทีที่หน้าไตเติ้ล
> ไม่งั้นคนที่ได้ชุดแจกไปต้องกดผ่านหน้า "เลือกเซิร์ฟเวอร์" เอง (Select Server → เลือกโหมด → OK → แตะจอ)
> ก่อน `BeginServer()` (ที่เช็ค `AutoConnectTarget` เหมือนกัน) จะมีโอกาสทำงาน — ดูจากข้างนอกเหมือน
> "ค้างหน้า main" เฉย ๆ (เจ้าของเทสชุดแจกเองแล้วเจอ) **แก้แล้ว**: เอาบล็อกเช็ค `AutoConnectTarget` กลับเข้า
> `OnConfirm()` ตามเดิม เทสยืนยันแล้ว — แตะจอครั้งเดียวจากชุดแจก (เล่นเกม.bat ตั้ง env ให้) พาตรงเข้าหน้า
> สร้างตัวละครทันที ไม่ต้องเลือกเมนูอะไรเพิ่มจริง ๆ (ตรงตามที่ อ่านก่อนเล่น.txt สัญญาไว้)
> **ชุดแจกล่าสุดที่มีบั๊กนี้แก้แล้ว:** `dist2\DurangoTH.zip` (SHA256 `BF98DA37...`) — ชุดเก่าที่
> `dist\DurangoTH.zip` (SHA256 `6315F593...`) **ยังมีบั๊กนี้อยู่ อย่าแจกอันนั้น**
>
> ### ✅ ล็อคหน้า /admin ด้วย token แล้ว — จำเป็นเพราะเอาเซิร์ฟไปตั้งบน VPS ที่เปิดพอร์ตออกอินเทอร์เน็ตจริง
> คอมเมนต์เดิมใน `Gateway.Admin.cs` สมมติว่า `/admin/*` ไม่มีวันถูก expose ออกอินเทอร์เน็ต (bind แค่
> localhost/LAN) — ผิดสมมติฐานทันทีที่เอาไปตั้งบน VPS จริง (ต้องเปิด ufw ให้พอร์ต 8190 คุยกับเกมได้)
> ⇒ ใครก็ตามที่รู้ IP เปิดเบราว์เซอร์เข้า `/admin` ได้เลยโดยไม่ต้องมีรหัส สั่งเตะผู้เล่น/เทเลพอร์ต/แก้
> POI/แก้ config/สั่ง cheat ได้หมด (เพราะ `--enable-cheat` เปิดอยู่)
> **แก้แล้ว:** เพิ่ม `--admin-token <รหัสลับ>` (Program.cs → Gateway ctor) — `Gateway.Admin.cs` มี
> `GuardAdminRoutes()` ห่อทุก route ที่ขึ้นต้น `/admin/` (ยกเว้น `/admin` กับ `/admin/` ที่เสิร์ฟ HTML เฉย ๆ)
> ด้วยการเช็ค `?token=` (query string หรือ POST field) เทียบกับค่าที่ตั้งไว้ — **ไม่ระบุ `--admin-token` =
> พฤติกรรมเดิมทุกอย่าง ไม่ auth** (เผื่อรันในเครื่อง/LAN แบบเดิม) ฝั่ง `admin/index.html` อ่าน `?token=`
> จาก URL ตอนเข้าครั้งแรกแล้วจำไว้ใน `localStorage` (`durango_admin_token`) — เข้าครั้งต่อไปแค่ `/admin`
> เฉย ๆ ก็ยังใช้ได้ ไม่ต้องพิมพ์ token ซ้ำทุกครั้ง
> VPS ปัจจุบันตั้ง `--admin-token` ไว้แล้ว (ดูใน `docs/` หรือถามเจ้าของ ไม่เก็บ token ไว้ในนี้)
> เทสยืนยันแล้ว: ไม่ใส่ token → 401 · ใส่ token → 200 · `/entry`,`/knock`,`/sessions` (ที่ client เกมต้องใช้)
> ยังเปิดอยู่ปกติไม่กระทบ
>
> ### ✅ เอาเซิร์ฟไปรันบน VPS จริงแล้ว ต่อจากเครื่อง Windows ผ่านอินเทอร์เน็ตจริงสำเร็จ
> VPS: `root@187.127.208.20` (Ubuntu 24.04, 1 core/3.8GB RAM, ของเจ้าของเอง มี service อื่นรันอยู่ด้วย
> เช่น korepilot-egress — **อย่าไปยุ่งกับ ufw rule อื่นที่ไม่ใช่ durango-\***)
> ขั้นตอนที่ทำ: `dotnet publish -c Release -r linux-x64 --self-contained true` → อัปโหลดด้วย
> `plink.exe`/`pscp.exe` (อยู่ที่ `%TEMP%\opencode\`, host key: `SHA256:2F3qfIsSFwi5vtLXAE97PpaD2YHdFcArh0lv8ENwpas`)
> → เปิดพอร์ตด้วย `ufw allow 8190/tcp`, `8191/tcp`, `8191/udp`, `8192/tcp` (comment `durango-*` กันสับสนกับ
> ของเดิม) → รันด้วย `nohup ./DurangoServer --data /root/durango/data --saves /root/durango/saves
> --public-host 187.127.208.20 --enable-cheat --name "Durango VPS Test" &`
> **ไม่ต้องอัปโหลด AssetBundles (643MB)** — client มี asset ครบอยู่แล้วในเครื่อง ไม่ต้อง serve จาก server
> ก็เข้าเกมได้ปกติ (ประหยัดเวลาอัปโหลดไปเยอะ)
> เทสจาก `connect-game.ps1 -Ip 187.127.208.20` (กลไกเดียวกับปุ่ม "Online Server (For Test)" ที่หน้าไตเติ้ล
> — ทั้งคู่เรียก `Server.ConnectTo(ip)`) — **สำเร็จเต็มรูปแบบ**: `curl http://187.127.208.20:8190/entry`
> ตอบจากอินเทอร์เน็ตจริง, log ฝั่ง VPS โชว์ `[world] player joined`, เข้าเกมได้ปกติ ชื่อเซิร์ฟ "Durango VPS
> Test" ขึ้นในหน้า island-info จริง (ยืนยันว่าไม่ใช่ข้อมูล cache จากเครื่อง local)
> **เซิร์ฟยังรันอยู่บน VPS** (`ps aux | grep DurangoServer` เช็คได้ผ่าน SSH) — ถ้าจะปิด:
> `pkill -9 -f DurangoServer` (ระวัง: รันแยกคำสั่งกับตัว start เหมือนเครื่อง Linux LAN เดิม ไม่งั้นจับ pattern
> ตัวเองตาย)
>
> ### ✅ หน้าไตเติ้ลเลือกโหมดได้จริงแล้ว (Creative Island/Single/Multi = local, Online Server = ต่อเซิร์ฟจริง)
> เจ้าของสั่งชัดว่า **"ต้องเลือกได้ว่าจะเล่นโหมดไหน ไม่ใช่บังคับออนไลน์แบบนี้"** — ของเดิม (เมื่อกี้ในเซสชันนี้เอง)
> ลองแก้โดย intercept ปุ่มยืนยันหลักของหน้าไตเติ้ล (`TitleMenuUserControlBase.OnConfirm()`) ให้บังคับต่อเซิร์ฟ
> เราทันทีไม่ว่าจะกดปุ่มไหน — **นั่นผิดหลักการที่เจ้าของต้องการ** เอาออกหมดแล้ว
>
> **สิ่งที่ทำจริงตอนนี้:** เกมต้นฉบับมีปุ่ม "Online Server (For Test)" อยู่แล้ว (`Durango.Offline/Servers.cs`
> — คู่กับ "Creative Island"/"Single Play Mode"/"Multi Play Mode") เดิมกดแล้วได้แค่เซิร์ฟจำลองในเครื่อง
> (พอร์ต 8390 ติดป้าย Mode.Online เฉย ๆ ไม่ได้ต่อเซิร์ฟจริงเลย) แก้ `Cluster.OnConfirm` ของ key "online" ใน
> `Durango.Offline/Server.cs` ให้เรียก `ConnectTo(ip)` จริง (ip = ค่าล่าสุดจากเมนู "เยี่ยมชมเกาะเพื่อน"
> ในเกม ถ้าไม่เคยกรอกก็ fallback 127.0.0.1) — ส่วน Creative Island/Single/Multi **ไม่แตะ** ยังเป็นเซิร์ฟ
> จำลองในเครื่องเหมือนเดิมทุกอย่าง (ไม่กระทบผู้เล่นที่เล่นออฟไลน์อยู่)
>
> **เจอบั๊กจริง 2 ชั้นระหว่างทำ (ทั้งคู่ไม่เคยเจอมาก่อนเพราะไม่เคยมีใครกดปุ่มนี้จริงจังขนาดนี้):**
> 1. เรียก `ConnectTo()` ตรง ๆ โดยไม่เรียก `BeginServer()` ก่อน → `_localPlayer` เป็น null → error ทันที
>    (แก้: เรียก `BeginServer(context.World, context.Player)` ก่อนเสมอเหมือน key อื่น แล้วค่อย `ConnectTo`)
> 2. **ตัวจริงที่ทำให้เจอ "[400] Bad Request/คิวการล็อกอิน" ซ้ำอีกรอบ** (คนละสาเหตุกับบั๊ก 192.168.1.34
>    เดิมที่แก้ไปก่อนหน้านี้ในเซสชันเดียวกัน — หน้าตาเหมือนกันแต่ต้นเหตุคนละจุด): `ConnectTo()` เรียก
>    `MoveToTitle()` → รีสตาร์ท `TitleMenuGroup` ใหม่ → `State.SelectCluster` เรียก
>    `TitleMenuUserControlBase.ShowCluster()` ซึ่งอ่าน `LastSelectedClusterKey` (คีย์ค้างใน Preferences
>    จากตอนเลือกในหน้า "เลือกเซิร์ฟเวอร์" เช่น "online") มาทับ `GameManager.GatewayUrl` กลับเป็นของ
>    คลัสเตอร์เดิม (เซิร์ฟจำลองในเครื่องพอร์ต 8390) เงียบ ๆ — client เลยยิง `/knock`,`/sessions` ไปเซิร์ฟ
>    จำลองแทนเซิร์ฟจริง แล้ว `Durango.Offline/WebServer.cs` (เซิร์ฟจำลองนั้น) ตอบ 400 ให้ POST body ที่ไม่ใช่
>    `application/x-www-form-urlencoded` (เราส่ง JSON) ⇒ โผล่เป็น "[400] Bad Request" พอดี
>    **แก้:** `TitleMenuUserControlBase.ForceSetClusters()` ต้องอัปเดต `LastSelectedClusterKey` ด้วยเสมอ
>    (ผ่าน property setter ที่เซฟลง Preferences จริง) ไม่งั้น `ShowCluster()` รอบถัดไปหาคลัสเตอร์ผิดตัว
>
> **วิธีวินิจฉัย:** เพิ่ม `Debug.Log` ชั่วคราวใน `TitleMenuGroup.RequestHttpUrl`/`CheckError` (ลบออกแล้ว)
> อ่านจาก `%LOCALAPPDATA%Low\NEXON Korea\Durango_ Wild Lands\output_log.txt` (Unity player log) — เจอ
> `RequestHttpUrl: http://127.0.0.1:8390/...` (ไม่ใช่ 8190!) นี่แหละที่ชี้ต้นเหตุจริง
>
> **เทสยืนยันแล้วครบวงจร** (ล้าง `game/AppData/offline/` + Windows Registry `HKCU:\Software\NEXON Korea\
> Durango: Wild Lands` ก่อนเทสรอบสุดท้ายเพื่อจำลองผู้เล่นใหม่ 100%): เปิดเกม → "Select Server" → เลือก
> "Online Server (For Test)" → กด OK → แตะหน้าจอ → server log โชว์ `GET /knock`→`POST /sessions` จริง
> พร้อมโหลดตัวละครจริงจากเซฟ (`b218bbef` เลเวล 60) → เข้าฉาก "Select your character" (รถไฟ) สำเร็จ — ไม่มี
> [400] อีกแล้ว ส่วน "Creative Island" (โหมดโลคอล) เทสซ้ำหลังแก้ ยังทำงานปกติ ไม่มี regression
>
> **อัปเดต: เทสจนเข้าเกมจริงสำเร็จแล้ว** — กด "Online Server (For Test)" → หน้ารถไฟเลือกตัวละคร → เข้าเกาะจริง
> ได้สมบูรณ์ (`[gameserver] client connected`, terrain โหลดครบ, `ผู้เล่นออนไลน์ 1`) ตัวละครยืนบนชายหาดจริง
> HUD/เมนูครบ (Character/Skill/Craft/Build/Bag/Visit a Friend's Island/Tasks) — **ปุ่มนี้ใช้งานได้เต็มรูปแบบแล้ว**
> ⚠️ เจอ 1 ครั้งระหว่างเทส: เกม**ปิดตัวเองกะทันหัน**ตอนอยู่หน้ารถไฟ (cutscene แนะนำตัวละครใหม่) — log โชว์
> `MediaPlayerCtrl: Could not open file` (ไฟล์วิดีโอ intro ไม่มีใน asset bundle ที่แพตช์ไว้ — ช่องโหว่เดิม
> ไม่เกี่ยวกับโค้ดที่แก้วันนี้) เทสซ้ำรอบถัดมาผ่านฉลุยไม่เจอซ้ำ (อาจเพราะรอบนั้นข้ามไปใช้ตัวละครที่มีอยู่แล้ว
> ไม่ต้องเล่น cutscene แนะนำตัว) **ยังไม่ได้ไล่ต้นเหตุวิดีโอนี้ให้ชัด** — ถ้าเกิดซ้ำบ่อยควรหา asset วิดีโอที่
> ขาดมาเติม หรือ patch `MediaPlayerCtrl` ให้ fail-safe (ข้ามได้ ไม่ปิดเกม)
>
> ### ✅ เจอต้นเหตุจริงของบั๊ก [400] Bad Request / คิวล็อกอิน แล้ว (รอบแรก — คนละจุดกับด้านบน) — แก้เสร็จ เทสผ่านทั้ง 2 เส้นทาง
> ต้นเหตุ**ไม่ใช่**จุดที่สงสัยไว้ก่อนหน้า (`TitleMenuGroup.cs` GetClusterList) — ของจริงอยู่ที่
> `client/Durango.Offline/Server.cs`: `_defaultAutoConnectTarget = "192.168.1.34"` (IP LAN เก่าตายแล้ว
> จากเฟสโปรเจกต์ก่อนหน้า) เป็นค่า fallback ตอนไม่ได้ตั้ง env `DURANGO_AUTOCONNECT` ⇒ เกม**ไม่เคย**มี
> `AutoConnectTarget` ว่างจริง มันวิ่งไปพยายามต่อ IP ตายนั้นเงียบ ๆ ทุกครั้งแทน — เป็นเหตุผลที่ไม่มี request
> ไหนไปถึงเซิร์ฟเราเลย และเป็นเหตุผลที่กล่องกรอก IP ใน `TitleMenuUserControlBase.OnConfirm()` ที่เพิ่งเขียน
> ไปก่อนหน้าไม่มีวันถูกเรียกถึง (เพราะเงื่อนไข `AutoConnectTarget` ว่างไม่เคยเป็นจริง)
> **แก้แล้ว:** เปลี่ยนเป็น `_defaultAutoConnectTarget = ""` — rebuild + เทสสด 2 เส้นทาง:
> - **ไม่ตั้ง env**: เข้าหน้า "เลือกตัวละคร 0/7" (offline "Creative Island") ได้สะอาด ไม่มี error [400] อีกแล้ว
> - **ตั้ง env** (`connect-game.ps1 -Ip 127.0.0.1`): regression ผ่าน เข้าเกมปกติเหมือนเดิมทุกอย่าง
> กล่องกรอก IP หน้าไตเติ้ลตอนนี้ **ควรเข้าถึงได้จริงแล้ว** (เงื่อนไขที่บล็อกไว้หายไปแล้ว) แต่ยังไม่ได้เทส
> คลิกผ่านจริงจนถึงกล่องนั้นโดยตรง (ทางที่ยืนยันแล้วคือ `connect-game.ps1 -Ip <ip>` ผ่าน env var)
>
> ### ✅ เพิ่ม `--cluster-mode Online` ให้เซิร์ฟ — เทส "online server for test" ได้จริงแล้ว
> เดิม `/entry` hardcode ตอบ `cluster_mode: "SingleMode"` เสมอ (ดู `Gateway.cs`) ทำให้ client ปิดฟีเจอร์ที่
> เช็ค `ClusterMode == Mode.Online` เกือบ 30 จุด (ตลาด/สารานุกรม/แชทส่วนตัว Radiotower/แชร์เพลง ฯลฯ)
> เพิ่ม CLI flag `--cluster-mode <SingleMode|Online|...>` (default ยังเป็น `SingleMode` เหมือนเดิม —
> **ไม่กระทบเซิร์ฟที่รันอยู่แล้วถ้าไม่ใส่ flag**) ส่งค่าผ่าน constructor ของ `Gateway` แทนการ hardcode
> **เทสแล้ว** รันเซิร์ฟด้วย `--cluster-mode Online --radiotower --enable-cheat`:
> - `curl /entry` ตอบ `"cluster_mode": "Online"` ถูกต้อง
> - บอทคอนโซล regression: join/mod hook/เดิน/status ปกติทุกอย่าง ไม่มี error
> - เปิดเกมจริงต่อเข้า: join สำเร็จ, HUD/มินิแมป/วงล้อแอคชั่นเรนเดอร์ปกติ, กดเปิด World Map ได้ปกติ
>   (จุดที่โค้ด client แยกสาขาพิเศษ `ClusterMode == SingleMode` ในไฟล์นี้) ไม่มี crash/error
> วิธีเปิดใช้: `dotnet run -- --cluster-mode Online --radiotower` (แนะนำเปิด `--radiotower` คู่กันเสมอ
> เพราะ Online mode client จะโชว์แท็บแชทส่วนตัวที่ต้องพึ่งพอร์ตนั้น)
> **ยังไม่ได้เทสทุกจุดที่เช็ค `ClusterMode == Online`** (30 จุดใน client) — เทสแค่ HUD หลัก+World Map
> เท่านั้น จุดอื่น (ตลาด CommodityList, สารานุกรม EncyclopediaGroup, แชร์เพลง MusicSheetEditor ฯลฯ) ยังไม่
> ได้ลองเปิดหน้าจอจริง เผื่อมี UI ไหนคาดหวัง data ที่เซิร์ฟยังไม่ implement (เช่น ตลาดจริง — `Features.Market`
> ปิดอยู่ ดูรายงาน 303-message-gap ด้านล่าง) แล้ว error/ว่างเปล่า — ควรเทสไล่ทีละหน้าก่อนบอกว่า "ใช้ได้เต็มที่"
>
> ### ✅ ระบบ mod แยก 3 เฟส (PreLoad/Load/PostLoad) แบบ Minecraft/Forge — ทั้งเซิร์ฟและเกม
> เจ้าของสั่งเพิ่มจากระบบ mod เดิม (ที่มีแค่ OnLoad เฟสเดียว) — เหตุผล: ถ้า mod B ต้องอ้างถึงของที่
> mod A ลงทะเบียนไว้ ลำดับที่สแกนไฟล์เจอ mod ไหนก่อนเป็นเรื่องบังเอิญ ⇒ ต้องให้ "ทุก mod ผ่าน PreLoad
> ก่อนใครเริ่ม Load" และ "ทุก mod ผ่าน Load ก่อนใครเริ่ม PostLoad" (ดู comment เต็มที่
> `mod-sdk/IGamePlugin.cs`) — `PluginManager.cs` (เซิร์ฟ) แก้เป็น 2 ขั้น: สแกน+สร้าง instance ทุก mod
> ก่อน แล้วไล่ 3 เฟสทีละเฟสข้ามทุก mod (ไม่ใช่ mod ละ 3 เฟสรวด) เทสแล้ว log โชว์ "PreLoad → โหลดแล้ว →
> PostLoad" ตามลำดับถูกต้อง (ดู `tools/ExampleMod/ExamplePlugin.cs`)
> ⚠️ **เจอ regression ระหว่างทำ**: แก้ interface แล้วลืม rebuild `tools/ExampleMod` ทำให้ mod เก่าที่ deploy
> ไว้ใน `server/mods/` โหลดไม่ขึ้น (`TypeLoadException` เงียบ ๆ) — แก้แล้ว (rebuild + copy DLL ใหม่) จำไว้:
> **แก้ mod-sdk แล้วต้อง rebuild mod ทุกตัวที่ deploy อยู่ด้วยเสมอ ไม่งั้นพังแบบไม่มี error ชัดเจน**
>
> ### ✅ ระบบ mod ฝั่งเกม (client) — เพิ่มใหม่วันนี้ (ยังไม่ได้เทสด้วย mod จริง แค่ compile+ติดตั้งผ่าน)
> `client-mod-sdk/` (net35, อ้าง UnityEngine.CoreModule.dll เหมือน `client/Assembly-CSharp.csproj`) +
> `client/ClientModLoader.cs` (โหลด .dll จาก `mods/` ข้าง ๆ DurangoV2.exe ตอน `GameManager.Start()`)
> — API มี Log/ShowMessage/RegisterHotkey/OnGameReady/LocalPlayer เรียกจาก `GameManager.Start()`
> `tools/build-client.ps1` อัปเดตให้ก๊อป `DurangoClientModSdk.dll` ไปคู่กับ `Assembly-CSharp.dll` ด้วยเสมอ
> (Assembly-CSharp อ้างอิงมันอยู่ ไม่งั้น Mono resolve ไม่เจอตอนเกมรัน) **ยังไม่ได้เขียน/เทส
> `tools/ExampleClientMod` จริง** — งานค้างถ้าจะทำต่อ
>
> ### ✅ ใส่ IP เซิร์ฟเองจากหน้าหลักได้แล้ว (โค้ดใน `TitleMenuUserControlBase.OnConfirm()`)
> ไม่ตั้ง env `DURANGO_AUTOCONNECT` = กดปุ่มหลักแล้วเจอกล่องกรอก IP ทันที (ใช้ `TextInputPopup` +
> `Server.ConnectTo(ip)` ตัวเดียวกับที่ปุ่ม "เยี่ยมชมเกาะเพื่อน → กรอกที่อยู่โดยตรง" ใช้อยู่แล้ว — ไม่ได้
> เขียน UI ใหม่ แค่ย้ายจุดเรียก) **โค้ด build ผ่าน ติดตั้งแล้ว แต่ยังเทสจบวงจรไม่ได้** เพราะเจอบั๊กแยกที่
> บล็อกอยู่ก่อนหน้า — ดูข้อถัดไป
>
> ### 🐛 พบบั๊กสำคัญ (ไล่ต้นเหตุไปได้ไกลแล้ว แต่ยังไม่แก้จบ): กดปุ่มหน้าไตเติ้ลโดยไม่มี autoconnect
> เจอ **"[400] Bad Request / คิวการล็อกอิน (ไม่สามารถเข้าถึง)"** วนซ้ำ กดผ่านไปเล่นไม่ได้เลย
>
> **ไล่ไปถึงไหนแล้ว:** ยืนยันด้วย log สดว่า **ไม่มี HTTP request ไหนไปถึงเซิร์ฟเราเลยสักครั้ง** ตอนเจอ error
> นี้ (ลอง `Monitor` tail log สดพร้อมคลิกจริงแล้ว 2 รอบ) ⇒ ไม่ใช่เซิร์ฟเราตอบ 400 แน่ ๆ — request ไปตายที่
> ไหนสักที่ **ก่อน**จะถึงเซิร์ฟเรา ลองแก้ที่ `TitleMenuGroup.cs` case `State.GetClusterList` (จุดที่ไม่มี
> `AutoConnectTarget` แล้วต้องอ่าน TextAsset "offline/clusters" จาก resources.assets ซึ่ง base เกมชุดใหม่
> ไม่มีแพตช์นี้แล้ว) ให้ fallback เป็น `ForceSetClusters("127.0.0.1")` — **เทสแล้วไม่ได้ผล ยัง error เหมือนเดิม**
> ⇒ เอาการแก้ตรงนั้นออกแล้ว (กันความเสี่ยงเพิ่มโดยไม่ได้อะไร) **สรุปคือต้นเหตุจริงอยู่ลึกกว่าจุดที่คิดไว้
> ตอนแรก** ต้องมีเวลาเจาะให้มากกว่านี้ (แนบ debugger จริง/เพิ่ม log ชั่วคราวในโค้ด client) ถึงจะเจอจุดจริง
>
> **ทางที่ใช้ได้จริงตอนนี้:** `tools\connect-game.ps1 -Ip <ip อื่น>` (ตั้ง env `DURANGO_AUTOCONNECT`)
> ยังคงทำงานถูกต้อง 100% (regression check ผ่านซ้ำหลายรอบวันนี้) — นี่คือคำตอบที่ใช้ได้จริงตอนนี้สำหรับ
> "ใส่ IP เซิร์ฟอื่น" ส่วนกล่องกรอก IP ในเกม (โค้ดใน `OnConfirm()`) ยังอยู่ในซอร์ส (ไม่ได้ลบออก เผื่อวันหน้า
> เจอต้นเหตุจริงแล้วจะใช้ได้ทันที) แต่ **เข้าไม่ถึงจริงในทางปฏิบัติ** เพราะบั๊กนี้บล็อกอยู่ก่อนหน้า
>
> ### 📋 รายงานเทียบ "เกมส่งอะไรมา" กับ "เซิร์ฟรับอะไรบ้าง" (message-level audit)
> ไล่ grep `client/Messages/*.cs` (985 ชนิด) หา message ที่ client โค้ดจริงส่งออกได้ (`Connections.*.Send`)
> เทียบกับ `Recv<T>` ทั้งหมดที่เซิร์ฟลงทะเบียน (`server/`) — **client ส่งได้จริง 365 ชนิด เซิร์ฟรับจริงแค่
> 88 ชนิด ⇒ ไม่มี handler เลย 303 ชนิด** ส่วนใหญ่แมตช์กับ `Features.*` ที่รู้อยู่แล้วว่าปิด (Market/
> Taming/Livestock/PartyAndClan/Pvp/LandPermission) บวกระบบที่ไม่เคยมี Feature flag เลยเพราะไม่เคยแตะ:
> **Mail (จดหมาย), Friend (เพื่อน), Music/Concert (เพลง/คอนเสิร์ต), Mount/Vehicle (ขี่สัตว์/พาหนะ),
> Archipelago/multi-region travel (เดินทางข้ามภูมิภาค — beta 1.0 ตั้งใจให้เป็นเกาะเดียวอยู่แล้ว), daily
> mission board (คนละระบบกับเควสหลักที่ทำแล้ว), S02* PVP arena minigame**
> รายชื่อเต็ม 303 รายการ + สคริปต์ grep ที่ใช้ ยังไม่ได้เซฟลงไฟล์โปรเจกต์ (รันจาก `/tmp` ตอนนี้ หายไปกับ
> session ถ้าไม่รันซ้ำ) — ควรเซฟเป็น `docs/FeatureGapAudit.md` รอบหน้าถ้าจะใช้ต่อ
> **เป้าหมาย "ให้ได้เหมือนตอน Nexon เปิด beta" ยังไม่ชัดว่าหมายถึงอะไร** — เกม Nexon จริงมีระบบครบทุกอัน
> ข้างบน (เป็นเกมการค้าที่เปิดจริง) ในขณะที่ scope "beta 1.0" ของเราเองที่ตั้งไว้ตั้งแต่แรกคือแค่ survival/
> craft/build loop เดี่ยว ๆ (ที่ทำเสร็จเกือบหมดแล้ว) — ถ้าจะไล่ทำ 303 message ให้ครบจริงคืองานขนาดหลาย
> สัปดาห์ถึงเป็นเดือน ควรถามเจ้าของให้ชัดว่าจะเอาระดับไหนก่อนเริ่มลงมือ
>
> ### 📣 โพสอัปเดตลง Discord แล้ว (ช่อง announce) พร้อมภาพเทสวันนี้ 3 ภาพ

---

**อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (01:00) — **ระบบ mod ฝั่งเซิร์ฟใช้งานได้จริง + มี Server Controller เป็น .exe แล้ว ✅**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> ### ✅ ระบบ mod (server-side) — เจ้าของสั่ง "เหมือน Minecraft" ทำเสร็จ + เทสจริงแล้ว
> - `mod-sdk/DurangoModSdk.csproj` — อินเทอร์เฟซเล็ก ๆ (`IGamePlugin`/`IModApi`/`IModPlayer`) ที่ mod
>   ภายนอกอ้างอิง (คนละ assembly กับ `DurangoServer.dll` — อัปเดตเซิร์ฟแล้ว mod เก่ายังใช้ได้)
> - `server/ServerCore/Modding/PluginManager.cs` — สแกนโหลด `.dll` จาก `server/mods/` ตอนบูต,
>   dispatch คำสั่ง `cheat <verb>` ที่ไม่ตรงกับคำสั่งในตัวไปให้ mod, event `OnPlayerJoined/Left/OnTick`
> - `tools/ExampleMod/` — mod ตัวอย่างสาธิตครบทุก hook **เทสผ่านจริงผ่าน console bot**: ทักทายคนเข้าเกม,
>   ตอบคำสั่งพร้อม args, สะสมเวลาออนไลน์ผ่าน OnTick, fallback "unknown cheat" ทำงานถูกสำหรับคำสั่งที่ไม่มี
>   mod รับ — ดูรายละเอียด/วิธีเขียน mod เองที่ **`docs/server/Modding.md`**
> - ⚠️ ไม่ hot-reload (ต้อง restart เซิร์ฟ), ยังไม่มี hook ระดับคราฟต์/ต่อสู้/กระเป๋า (v1)
> - ฝั่ง **client (เกม)** ยังไม่ได้ทำ — เกมเป็น Unity Mono backend (ไม่ใช่ IL2CPP) เปิดทางให้ทำได้ถ้าจะทำต่อ
>   (AssetBundle สำหรับของใหม่ = ง่าย, BepInEx-style behavior mod = งานใหญ่แยกต่างหาก) ดูหัวข้อ 6 ใน Modding.md
>
> ### ✅ Mod Loader tab ใน admin panel — "รู้มอดโหลดจริงไหม"
> `/admin/mods` (endpoint ใหม่) + section ใหม่ในหน้า `server/admin/index.html` โชว์ทุก mod ที่พยายามโหลด
> (สำเร็จ/พัง + error message), เวอร์ชัน, ไฟล์ที่มา, คำสั่งที่เพิ่ม, hook ที่ผูก — เทสแล้วในเบราว์เซอร์จริง
>
> ### ✅ Durango Server Controller — GUI ควบคุมเซิร์ฟตัวจริง เป็น .exe แล้ว
> เจ้าของสั่ง "อยากได้ตัวควบคุมเซิร์ฟเวอร์แบบเป็น gui จริงๆ เป็นไฟล์ exe เปิดใช้งานตั้งค่าเซิฟได้เลย"
> — `tools/ServerController/` (WinForms + WebView2, net9.0-windows) build ได้ไฟล์
> **`DurangoServerController.exe`** จริง (`bin/Release/net9.0-windows/`) เปิดแล้ว:
> - แถบเครื่องมือบนสุด: ปุ่ม **เปิด/ปิด/รีสตาร์ทเซิร์ฟ** จริง (สั่ง `dotnet run` เป็น process ลูก) +
>   checkbox `--enable-cheat` + สถานะสด (🟢/🔴 + PID)
> - ฝัง **WebView2 แสดง `server/admin/index.html` ตัวเดิมทั้งหมด** (ผู้เล่น/POI/config/log/**Mod
>   Loader**/cheat console) — ไม่ต้องเปิดเบราว์เซอร์แยก ไม่ต้องเขียน UI ซ้ำ
> - แผง log ดิบด้านล่างไว้ดูตอนเซิร์ฟกำลังเปิด/ปิด (ก่อน WebView2 จะต่อติด)
> - หาโฟลเดอร์ `server/` เองอัตโนมัติ (เดินขึ้นจากตำแหน่ง .exe หา `server/DurangoServer.csproj`)
> - เปิดโปรแกรมแล้วเจอเซิร์ฟรันอยู่ก่อนแล้ว = แค่ต่อเข้าไปดู ไม่เปิดซ้ำ (กันพอร์ตชน)
>
> **เทสแล้วจริง**: เปิด .exe → auto-start เซิร์ฟ → WebView2 โหลด admin panel ครบ (เห็น Mod Loader/POI/log
> สด) → กดปุ่ม "ปิดเซิร์ฟ" → process ตายจริง + สถานะเปลี่ยนถูก → กด "เปิดเซิร์ฟ" → เซิร์ฟกลับมาจริง เชื่อม
> ต่อใหม่อัตโนมัติ — วนครบทุกปุ่มแล้ว
>
> ⚠️ **ยังไม่มีไอคอนโปรแกรม** (ไม่มีไฟล์ .ico ให้) และ **"ปิดเซิร์ฟ" คือ kill process tree ตรง ๆ**
> (เหมือน `taskkill /F` ที่ใช้กันมาตลอด — ไม่ใช่ graceful shutdown ผ่าน Ctrl+C จริง เพราะ process ลูกไม่มี
> console ให้ส่งสัญญาณ) autosave ทุก 60 วิ อยู่แล้วเหมือนเดิม เสี่ยงข้อมูลหายแค่ในช่วงนั้นเท่าที่เคยเป็น
> **build เอง**: `dotnet build tools\ServerController -c Release` (ต้องมี Microsoft Edge WebView2
> Runtime ในเครื่อง — ปกติมีอยู่แล้วถ้าเคยใช้ Edge/Windows 11)

---

**อัปเดตก่อนหน้า:** 24 ส.ค. 2026 (00:00) — **หน้าคราฟต์: ของที่ทำได้ตอนนี้ลอยขึ้นบนสุดของทั้งช่องกลางแล้ว ✅**

> ระบบเดิม (`RecipeListWidget.SubList.EnumerateItems`) มีอยู่แล้วที่จัดของ "ทำได้ตอนนี้" ขึ้นก่อน แต่ทำแค่
> **ภายในกลุ่มย่อยตัวเอง** — ลำดับของกลุ่มย่อยเองยังเรียงตามตัวอักษรเฉย ๆ (`SortComparison`) ⇒ กลุ่มที่ไม่มี
> อะไรทำได้เลยแต่ชื่อขึ้นต้นด้วยตัวอักษรแรก ๆ ก็ยังโผล่บนกว่ากลุ่มที่มีของพร้อมคราฟต์ได้
> **แก้:** เพิ่ม `SubList.HasCraftableNow()` แล้วใช้เป็นคีย์เรียงลำดับ**กลุ่ม**ก่อนเรียงตัวอักษร (หลัง
> favorites เหมือนเดิม) — กลุ่มที่มีของคราฟต์ได้อย่างน้อย 1 ชิ้นขึ้นก่อนกลุ่มที่ทำไม่ได้เลยทั้งกลุ่ม
> เทสแล้ว (ให้ไม้/หิน/เชือกด้วย cheat give): "ใบมีดหิน" (อันเดียวที่ทำได้ตอนนี้) ลอยขึ้นบนสุดของหมวด
> "ทั้งหมด" จริง ก่อนของล็อกทั้งหมด ✅ (คนละเรื่องกับการเอาแถบหัวข้อกลุ่มย่อยออกที่ทำไปก่อนหน้า —
> อันนั้นซ่อนแค่ตัวหนังสือคั่น ไม่ได้แก้ลำดับ)

---

**อัปเดตก่อนหน้า:** 23 ส.ค. 2026 (ดึกมาก) — **เจอต้นเหตุจริงที่ทำให้ "UI คราฟต์เป็น PC" แล้ว แก้เสร็จ ✅**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> รอบก่อน (ดึก รอบแรก) สรุปผิดว่า "เกมต้นฉบับไม่มี mobile prefab แยกสำหรับหน้าคราฟต์" — เจ้าของไม่เชื่อ
> สั่งให้ **"ไปดูตัวเกมต้นฉบับแล้วค่อยมาแก้"** ⇒ ไล่ byte-parse `resources.assets` จริงจัง (ไม่ใช่แค่ grep
> ซอร์ส C#) แล้วพบว่า **สรุปผิดจริง ๆ** — มี `RecipeSelectorGroup` (มือถือ, path_id 1054) แยกจาก
> `RecipeSelectorGroup_PC` (path_id 3245) ชัดเจน โครงสร้างลูกข้างในต่างกันเยอะ (เช่น มือถือไม่มี
> `RecipeFilterWidget` เป็นลูกตรงของ Container แบบ PC — ซ้อนอยู่ใต้ `RecipeListWidget` แทน, `BackSprite`
> ของมือถือเป็นพี่น้องกับ Container ไม่ใช่ลูกแบบ PC) **เกมมี UI มือถือแยกจริง แค่โค้ดของเราเองไปบัง**
>
> ### ✅ ต้นเหตุตัวจริง: `CraftScreen.cs` (ไฟล์ที่เราเขียนเองสมัยก่อน ไม่ใช่ของเกม) จัดตำแหน่งทับทุก UI
> ที่โหลด — ตอนเขียนโค้ดนี้ทดสอบอยู่บน UI PC เท่านั้น (ก่อนจะสลับ default เป็นมือถือวันนี้) โค้ดมันหา
> `container.Find("BackSprite")` แบบ hardcode ตามโครงสร้าง PC ⇒ พอรันบน prefab มือถือ (โครงสร้างคนละแบบ)
> หาไม่เจอ (เงียบ ๆ ไม่ throw) แล้วจัดตำแหน่งส่วนที่เหลือทับผิด ๆ ⇒ ดูเหมือน "เอา UI PC มาใช้" ทั้งที่จริง ๆ
> คือ prefab มือถือแท้ ๆ ที่ถูกจัดตำแหน่งพังไปครึ่งหนึ่ง
> **แก้:** `CraftScreen.Enabled` เพิ่มเงื่อนไข `Platform.Instance.UsePCUI` — ทำงานเฉพาะโหมด PC เท่านั้น
> โหมดมือถือปล่อย prefab ของ NEXON เอง render ตามธรรมชาติ ไม่ไปยุ่งด้วยเลย
> **ผล (เทสแล้ว):** หน้าคราฟต์ตอนนี้เป็น 3 คอลัมน์ (หมวดหมู่ไอคอนกริด/รายการไอเทม/รายละเอียด+ปุ่มสร้าง)
> ครบ สวยงาม ไม่มีอะไรล้น/ถูกตัด — ของจริงจาก NEXON ล้วน ๆ
>
> ### ✅ เอา sub-category header ("ชุดสำรวจ" ฯลฯ) ออกจากรายการไอเทมกลางแล้ว
> `RecipeSubListWidget.cs::SetRecipes` — เดิมแสดงแถบหัวข้อกลุ่มย่อยคั่นระหว่างกลุ่มไอเทม (มาจาก
> `RecipeListWidget.SubList.Text`, แปลจาก `#recipe_category_<subcategory>`) เจ้าของสั่งเอาออก เหลือ
> แค่ไอเทมล้วน ๆ — ซ่อน `_titleWidget` + ยุบความสูงเป็น 0 (ต้องยุบด้วย ไม่งั้นเหลือช่องว่างเปล่า เพราะ
> `TitleHeight` ยังถูกใช้คำนวณตำแหน่ง scroll ต่อ) เทสแล้ว: รายการไหลต่อเนื่องไม่มีแถบคั่นเลย ✅
>
> ### ⏸️ งานเดิม (แก้แล้วแต่ถูก supersede) — CraftUiLayoutConfig hot-reload system
> ลบไปแล้วตามที่สั่ง (ไฟล์ `client/CraftUiLayoutConfig.cs` + `game/craft_ui_layout.json` ที่ตกค้าง)
> กลับไปใช้ const ในโค้ด — **แต่ตอนนี้ CraftScreen.cs ทั้งไฟล์ไม่ทำงานแล้วในโหมดมือถือ (ตาม fix ข้างบน)
> เลยไม่มีผลอะไรกับที่เห็นตอนนี้** ยังไม่ได้ลบไฟล์ CraftScreen.cs ทิ้งทั้งไฟล์ (เผื่อยังอยากใช้ตอนทดสอบ
> ด้วย UI PC บ้าง — สลับกลับด้วย `DURANGO_FORCE_PCUI=1`)
>
> ### 💡 บทเรียนสำคัญ — จำไว้ใช้กับหน้าจออื่น
> **ห้ามสรุปจาก grep ซอร์ส C# อย่างเดียวว่า "ไม่มี mobile prefab แยก"** เกมนี้ไม่ได้ใช้ชื่อคลาส `_Mobile`
> แยกเหมือนที่คิด — ความต่าง PC/Mobile อยู่ที่ **ชื่อ GameObject ใน `_mainPC` มีคำต่อท้าย `_PC` เกือบทุกตัว
> (`XxxGroup_PC`) ส่วนตัวมือถือใช้ชื่อเดิมไม่มีต่อท้ายเลย** ต้องเช็คด้วย byte-parse `ui_prefab_map`
> (path_id 27088 ใน resources.assets, สคริปต์ตัวอย่าง `scratch_extract_uiprefab.py`/
> `scratch_explore_hierarchy.py` ที่เขียนไว้รอบนี้) ถึงจะรู้แน่ — **ถ้ามีสกรีนไหนที่ "ดูเหมือน PC" อีก
> ให้สงสัยโค้ดกำหนดเอง (`[แก้เอง]`) ก่อนว่าไปจัดตำแหน่งทับ prefab มือถือผิดๆ หรือเปล่า อย่าเพิ่งคิดว่า
> เกมไม่มี mobile prefab ให้**

---

**อัปเดตก่อนหน้า:** 23 ส.ค. 2026 (ดึก) — **แก้ "ไม่มีเมนูกดวาป" ที่หลุมวาร์ปสำเร็จ ✅ + พบว่าหน้าคราฟต์ยังเป็น UI PC (รอตัดสินใจ)**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> ### ✅ แก้ "ไม่มีเมนูกดวาป" เสร็จแล้ว — เทสในเกมจริงจนจบ (ไล่ทั้งเชนได้สำเร็จ)
> ต้นเหตุแท้จริงมี **3 จุดขาดพร้อมกัน** (ไม่ใช่แค่จุดเดียว) เจอโดยไล่โค้ด client ย้อนจาก UI ไปหา server:
> 1. `ServerPlayer.Gathering.cs` `HandleTouch` — component `"Warphole"` (ติดมากับทั้ง `camp_warphole` และ
>    `neutral_warphole` เสมอ) **ไม่เคยถูกจับใน switch เลย** ⇒ เมนู "วาป" (`Interaction.Warp=515`) ไม่โผล่ตอนแตะ
>    หลุมวาร์ปสักครั้ง — นี่คือสาเหตุตรงตามที่เจ้าของเห็น "ไม่มีเมนู" (เพิ่ม `case "Warphole": interactions.Add(515)`)
> 2. กดเมนู "วาป" แล้ว client ส่ง `IsWarpholeAvailable{EntityId,Tile}` รอ reply `OK` ก่อนเปิดแผนที่ — **ไม่เคยมี
>    handler เลยตั้งแต่แรก** (เพิ่ม `HandleIsWarpholeAvailable` ใน `ServerPlayer.POI.cs` ตอบ OK เสมอถ้า artifact
>    ยังอยู่จริงและเป็นตระกูลหลุมวาร์ป — ยังไม่มีระบบล็อกหลุมวาร์ป)
> 3. แผนที่โหมดวาปเปิดมาแล้วส่ง `GetWarpCosts`/`GetWarpBackCost` เพื่อเอาราคา/สถานะมาแปะป้ายไอคอน — **ก็ไม่เคยมี
>    handler เหมือนกัน** (เพิ่มทั้งคู่ — cost = 0 เสมอ ตาม pattern เดียวกับ WarpAccelerator ที่ยังไม่มีระบบเงิน)
>
> เทสจริงในเกม (มือถือ): แตะหลุมวาร์ป → เมนู **"วาร์ป"** โผล่ข้าง "ทำลาย" แล้ว ✅ → กดแล้วแผนที่เปิดหัวข้อ
> **"เลือกหลุมวาร์ปที่จะเข้าไป"** ทันที ✅ (เชนทำงานครบ touch→interact→IsWarpholeAvailable→GetWarpCosts)
> ⚠️ **ยังไม่ได้เทสวาร์ปจริงไปหลุมที่ 2** เพราะต้อง "ค้นพบ" (explore) หลุมที่สองก่อนถึงจะมีหมุดโผล่บนแผนที่ให้กด
> (ป้ายราคาบนหมุดมาจาก `WarpCosts` ที่ตอบแล้ว แต่ตัวหมุดเองสร้างจากระบบ explore แยกต่างหาก คนละกลไกกัน)
> ไว้ลองเดินไปหาหลุมที่ 2 ให้ค้นพบก่อนแล้วค่อยกดวาปจริงดูรอบหน้า
>
> ### ⏳ พบเพิ่ม (ยังไม่ได้แก้ รอเจ้าของตัดสินใจ): **หน้าสร้าง/คราฟต์ (`RecipeSelectorGroup`) ยังเป็น UI แบบ PC**
> ไล่โค้ดแล้วพบว่า **เกมต้นฉบับไม่มีพรีแฟบ/คลาสมือถือแยกสำหรับหน้านี้เลย** — `grep _Mobile` ทั่ว `client/` เจอแค่
> 4 ไฟล์ (`TerrainChunk_Mobile`/`Terrain_Mobile`/`UIAnchorPolicy_Mobile`/`BlurController_Mobile`) ไม่มี
> `RecipeSelectorGroup` หรือคราฟต์-ที่เกี่ยวข้องเลยสักตัว และ `_mainMobile`(95)/`_mainPC`(96) ต่างกันแค่ 1 รายการ
> (ดู `docs/project/CAPABILITY-REPORT.md` หัวข้อ 3) ⇒ **น่าจะเป็นของเดิมจากค่าย ไม่ใช่บั๊กที่เราทำพัง** หน้าคราฟต์แชร์
> เลย์เอาต์เดียวกันทั้งสองแพลตฟอร์มในเกมต้นฉบับจริง ๆ — ยังไม่ได้ยืนยัน 100% ด้วยการ diff รายชื่อ GameObject
> ใน `_mainMobile` vs `_mainPC` แบบละเอียด (ต้อง manual byte-parse ใหม่ ดู CAPABILITY-REPORT หัวข้อ 3 วิธีทำ)
> **ทางเลือกที่ยังไม่ได้ถาม:** (ก) ปล่อยไว้แบบนี้ (ข) ยืนยันด้วย byte-diff ให้ชัดก่อน (ค) ออกแบบ UI คราฟต์แบบ
> มือถือเองใหม่ทั้งหน้า (มีซอร์สเต็ม แก้ได้แต่เป็นงานใหญ่)
> (สังเกตด้วย: มี debug overlay สีแดง "manualWidth=... Screen=..." ค้างอยู่บนหน้าคราฟต์ — เป็นของเก่าตั้งแต่
> 17 ส.ค. คนละเรื่องกับงานวันนี้ ไม่ได้เกี่ยวกับการสลับ UI มือถือ)

---

**อัปเดตก่อนหน้า:** 23 ส.ค. 2026 (ค่ำ รอบ 3) — **UI มือถือเป็นค่าเริ่มต้นแล้ว + คลิกขวาเพื่อเดินใช้ได้ทั้งสอง UI ✅**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> เจ้าของสั่ง 2 เรื่อง: **(1) ใช้ UI มือถือเป็นหลัก** (2) **เพิ่มคลิกเพื่อเดินในโหมดมือถือ** — ทำเสร็จทั้งคู่แล้ว
> build+ติดตั้งลงเกมจริง เทสยืนยันในเกมแล้ว
>
> ### ✅ 1. UI มือถือเป็น default แล้ว — `client/Durango.System/Platform_PC.cs`
> เดิมต้องตั้ง env `DURANGO_MOBILEUI=1` ถึงจะเห็น UI มือถือ (ทดลองครั้งแรกรอบก่อน) ตอนนี้สลับ default แล้ว:
> **ไม่ตั้ง env อะไรเลย = UI มือถือเสมอ** ถ้าอยากย้อนกลับไป UI PC ชั่วคราว (เทียบผล/debug) ตั้ง
> `DURANGO_FORCE_PCUI=1` แทน — เทสแล้วเปิดเกมมาเจอ status bar มือถือ (WiFi/เวลา/battery%) ทันทีไม่ต้องตั้งอะไร
>
> ### ✅ 2. คลิกขวาเพื่อเดิน (click-to-walk) ใช้ได้ในโหมดมือถือแล้ว — `client/PlayerController.cs`
> ต้นเหตุ: `InputMouse.cs` ผูกปุ่มขวาคลิก → `InputCommand.MoveToPosition` แบบไม่มีเงื่อนไขแพลตฟอร์มอยู่แล้ว
> (เกมมีฟีเจอร์นี้เดิม) แต่ `PlayerController.OnAwake()` สมัคร handler รับคำสั่งนี้ **เฉพาะตอน
> `Platform.Instance.UsePCUI == true` เท่านั้น** ⇒ โหมดมือถือคลิกขวาไปก็เงียบ ไม่มีอะไรเกิดขึ้น
> **แก้:** ตัดเงื่อนไข `if (UsePCUI)` ออก สมัคร handler เสมอไม่ว่า UI ไหน
> เทสแล้ว: คลิกขวาบนพื้น ตัวละครวิ่งไปจุดนั้นจริง (เห็นท่าวิ่ง + ตำแหน่งบน minimap ขยับ) ในโหมด UI มือถือ
> ⚠️ นี่คือปุ่มเมาส์ขวา (หรือซ้ายถ้าเปิด "reversed mouse button") ไม่ใช่ touch gesture จริง — เกมต้นฉบับไม่มี
> "แตะเพื่อเดิน" บนมือถือจริงเลย (มีแต่ virtual joystick, ดู `InputVirtualStick.cs`) เราแค่เปิดทางลัดของเมาส์
> ที่มีอยู่แล้วให้ใช้ได้ตอนทดสอบผ่านคอม ถ้าจะรันบนมือถือจริงต้องดูเรื่อง virtual joystick แยกต่างหาก

---

**อัปเดตก่อนหน้า:** 23 ส.ค. 2026 (ค่ำ รอบ 2) — **กฎวาง POI ใหม่: ท่าเรือติดแม่น้ำเท่านั้น, หลุมวาร์ป/รอยแยกต้องอยู่บนเกาะ ✅**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> เจ้าของสั่งแก้กฎวาง POI: **ท่าเรือ (`dock`) ต้องติดแม่น้ำเท่านั้น** (เดิมติดน้ำอะไรก็ได้ ทะเล/ทะเลสาบ/แม่น้ำ)
> **หลุมวาร์ป/รอยแยกทุกชนิด (`camp_warphole`/`neutral_warphole`/`warp_accelerator`) ต้องอยู่ลึกเข้าเกาะ ไม่ใช่ริมน้ำ**
> (ก่อนหน้านี้ minInland แค่ 2-3 tile ทำให้วางติดชายฝั่งได้ — เจอจริงตอนเทส 6/7 จุดที่ "สะอาด" รอบก่อนจริง ๆ
> ก็ยังริมน้ำอยู่ดี แค่ไม่ได้โดนหินทับ)
>
> ### ✅ แก้ที่ `server/ServerCore/ServerWorld.cs`:
> 1. เพิ่ม `TouchesRiver(tx,ty,size)` — เช็ค `Terrain.BiomeAt() == Biome.River` ตรง ๆ (คนละอันกับ `TouchesWater`
>    ที่ใช้ `LandDistance`/oceans.dm ซึ่งวัดระยะจาก**ทะเล**เท่านั้น ไม่รู้จักแม่น้ำเลย)
> 2. `dock` เปลี่ยนจากเรียก `TouchesWater` → `TouchesRiver`
> 3. footprint ทุกชิ้นห้ามทับ biome River โดยตรง (เพิ่มเข้าไปในเช็ค allLand เดิม)
> 4. `minInland` ของ `warp_accelerator`/`camp_warphole`/`neutral_warphole` ยกจาก 2-3 → **10** (ชุดไกลจุดเกิด)
>    และ **6** (ชุดใกล้จุดเกิด — วงแหวนหาที่วางแคบแค่ 12-35 tile รอบจุดเกิดซึ่งใกล้ชายหาด ยกเท่าชุดไกลจะหาที่วางไม่เจอเลย)
> 5. `ServerPlayer.CheatPOI.cs` (`DescribePOIProblem`, ใช้ทั้ง `cheat poi check` และ `/admin/poi?problems=1`)
>    อัปเดตตาม: ท่าเรือเช็ค `TouchesRiver` แทน `TouchesWater` + เพิ่มเช็คใหม่ "หลุมวาร์ป/รอยแยกใกล้น้ำเกินไป"
>    (`LandDistance < 6`) กันบั๊กแบบ `warp_accelerator_1` (POI เก่าที่ไม่เคยถูก validate ตำแหน่งซ้ำ) เกิดซ้ำแบบเงียบ ๆ
>
> ### ✅ ล้าง POI เก่าทั้ง 9 จุดใน `server/saves/world.json` แล้วให้วางใหม่ตามกฎ (สำรองไว้ที่
> `world.json.backup-before-poi-reset-20260823`) — restart แล้ว **วางใหม่สำเร็จ 8/9** (near-entry dock หาที่วาง
> ไม่เจอ เพราะไม่มีแม่น้ำในวงแหวน 12-35 tile รอบจุดเกิด — ข้ามไปเงียบ ๆ ไม่ error, ยอมรับได้)
> เทสยืนยันในเกมแล้ว 2 จุด: ท่าเรือใหม่ (170,90) ติดริมน้ำจริง · หลุมวาร์ปใหม่ (67,165) อยู่ลึกในหุบเขา
> ไม่เห็นน้ำเลย ✅ `/admin/poi?problems=1` ว่างเปล่า (ไม่มีปัญหาเหลือ)
>
> ⚠️ **ยังไม่ได้ตรวจว่า "แม่น้ำ" ที่ท่าเรือใหม่ไปติดคือแม่น้ำจริงหรือทะเลสาบ** (ภาพดูเหมือนลำน้ำแคบ ๆ
> สมเหตุสมผล แต่ยังไม่ได้เทียบกับแผนที่ biome เต็ม) และยังไม่ได้ลองรีสตาร์ทซ้ำหลายรอบดูว่า `EnsureNaturalPOIs`
> เจอที่วางแม่น้ำได้เสถียรทุกครั้งไหม (ตอนนี้ทดสอบแค่รอบเดียว)

---

**อัปเดตก่อนหน้า:** 23 ส.ค. 2026 (ค่ำ) — **เทสหลุมวาร์ป/POI ครบ 7 จุด, เจอ+แก้ 1 จุดพังจริง ✅**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> รอบก่อนหน้า (23 ส.ค. เย็น) เปลี่ยนฐาน `game/` ใหม่ + ยืนยัน Mobile UI ใช้งานได้จริง (17:57) แล้วเริ่ม
> เทสหลุมวาร์ปที่เคยสงสัยว่า "โดนหินทับ" แต่ session ตัดจบกลางคัน — รอบนี้เทสต่อจนจบ:
>
> ### ✅ เทสหลุมวาร์ป/warp accelerator ทั้ง 7 จุดในเซฟ (ใช้ `POST /admin/cheat` สั่ง `tp <tx> <ty>`
> ให้ตัวละครจริงในเกม แทนคลิกเมาส์เอง — เร็วกว่ามาก) **6/7 สะอาดดี ไม่มีหินทับ เดินถึงได้จริง**:
> `poi_warp_accelerator_0`(89,91) · `poi_camp_warphole_2`(41,161) · `poi_camp_warphole_3`(125,135) ·
> `poi_neutral_warphole_4`(112,166) · `poi_near_warp_accelerator_0`(55,165) · `poi_near_camp_warphole_1`(69,158)
>
> ### 🐛 เจอจุดพังจริง 1 จุด — **แก้แล้ว**: `poi_warp_accelerator_1` เดิมอยู่ tile **(209,92)**
> จุดโต้ตอบ ("รอยแยก") ลอยอยู่กลางน้ำลึก เดินเข้าไปไม่ถึง (ได้ข้อความ "ไปลึกกว่านี้ไม่ได้" ซ้ำ 2 ครั้งทุกรอบที่ tp เข้าไป)
> **root cause**: นี่คือ POI เก่าที่วางไว้ตั้งแต่ก่อนแพตช์ `EnsureNaturalPOIs`/`ClearNaturalsUnderPOIs` —
> ตัวแพตช์นั้นแก้แค่ "หิน/ต้นไม้ทับ POI" (`ClearNaturalsUnderPOIs`) ไม่เคยเช็คว่า**ตำแหน่ง**ของ POI เก่าที่มีอยู่แล้ว
> ยังอยู่บนบกไหม (เช็ค `LandDistance`/ring เฉพาะตอนวางใหม่ใน `PlacePOISpots` เท่านั้น) — ของเก่าที่พิกัดผิดเลยไม่เคยถูกจับ
> **แก้ด้วย** `POST /admin/poi/move id=poi_warp_accelerator_1 x=205 y=88` ย้ายเข้าฝั่ง 4 tile แล้ว `cheat save`
> — เทสซ้ำแล้ว: ไม่มีข้อความ "ไปลึกกว่านี้ไม่ได้" อีก เปิดวงล้อโต้ตอบได้ปกติบนบก ✅
> ⚠️ **ยังไม่ได้ไล่เช็คว่ามี POI เก่าจุดอื่นที่พิกัดผิดแบบเดียวกันซ่อนอยู่อีกไหม** (เช็คแค่ 7 จุดที่ HANDOFF รอบก่อนสงสัยไว้
> ยังไม่ได้ไล่ `near_dock_2`/`dock_5` หรือสแกนทั้งเซฟหาพิกัดที่ `LandDistance < 1`)
>
> ### ⏸️ ค้างจาก session ก่อน (ยังไม่แตะต่อรอบนี้)
> - **แปล recipe category เป็นไทยทำค้างกลางคัน** — แก้ `client/MarkupFormatter.cs` เพิ่ม locale `"th"`
>   ให้ dict เดียว (key `"lv"`) จากทั้งหมดหลายสิบ key (ดู `scratch_category.json`/`scratch_dbg.txt` ที่ค้างไว้ในโปรเจกต์
>   — เป็นข้อมูลดิบภาษาเกาหลีของ `#recipe_category_*` ที่ดึงมาไว้เทียบ) — ยังไม่ build ทดสอบ

---

**อัปเดตก่อนหน้า:** 22 ส.ค. 2026 (รอบ 2) — **ตัวละครที่สร้างมาถึงเซิร์ฟครบทั้งชื่อ/เพศ/หน้าตา ✅**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> **สถานะ:** เปิดเกม → สร้าง/เลือกตัวละคร → เข้าโลกได้ และ**ตัวละครหน้าตาตรงกับที่ปั้นแล้ว**
> เซิร์ฟที่ใช้เทส = `127.0.0.1` (`dotnet run -- --enable-cheat` ที่ `server/`)
> เปิดเกม: `powershell -File tools\connect-game.ps1 -Ip 127.0.0.1`
>
> ### ✅ แก้เสร็จรอบนี้ (22 ส.ค. รอบ 2)
> 1. **โมเดล/ชื่อตัวละครไม่ตรงที่สร้าง** — 🐛 ต้นเหตุคือ `POST /players` ฝั่งเซิร์ฟ**ทิ้ง `gender`+`model_info` ทั้งดุ้น**
>    แล้วสุ่ม GUID คืนไปเฉย ๆ · แก้ 3 จุด:
>    - `Gateway.cs /players` — อ่าน name/gender/model_info → เขียน `saves/players/<id>.json` **ตั้งแต่ตอนสร้าง**
>    - `Gateway.cs /sessions` — client ส่งมาแค่ id = เติมชื่อ/เลเวล/หน้าตาจากไฟล์เซฟให้
>    - `PrologueManager.cs` — เอา display+เพศที่ปั้นไว้ใส่ `PlayerContext` แล้ว **`Save()` ลงดิสก์** (เดิมอยู่แต่ใน RAM)
>    - เทสใหม่ **`--create-check` 12/12** (`test-client/CreateCharacterCheck.cs`) — ไล่ `/players` ➜ `/sessions` ➜ เข้าเกม
>      แล้วเช็ค `AppearPlayer` ของตัวเองว่า ชื่อ/เพศ/ผม/สีผิว/ขนาดตัว/เสียง ตรงกับที่สร้าง
> 2. **ชื่อไทยใน log เป็นตัวขยะ** (`[animal] เน€เธยเน€เธเธ…`) — ไม่ใช่ console encoding แต่
>    `server/data/config.json` **พังจริง 14 จุด** (สัตว์ 10 + โซน 4) เพราะเคยถูกอ่านเป็น ANSI (cp874) แล้วเขียนกลับ
>    · แก้ด้วย `ConfigRoot.RepairMangledNames()` — เจอร่องรอยแปลงรหัสผิด (U+FFFD · C1 · `€`) ก็ใช้ชื่อตั้งต้นแทน
>    แล้วเขียนไฟล์กลับให้เอง ⇒ **ไฟล์ซ่อมตัวเองแล้ว** ตอนนี้ log ขึ้น "กิ้งก่า / ทริเซราท็อปส์ / …" ถูกต้อง
>
> ### ⏳ ค้างต่อ (เรียงตามลำดับ)
> 1. **`--quest-check` ตก 3 ข้อ** (30/33) — "ข้อความฉลองตอนต่อแพ" · "สวมอุปกรณ์แล้วตัวนับขยับ" ·
>    "ปลูกผักแล้วตัวนับขยับ" · **ค้างมาก่อนรอบ 22 ส.ค. รอบ 2** (19 ส.ค. เคย 33/33) น่าจะพังตอนงาน POI/เกาะ 20–21 ส.ค.
> 2. **เมนูสกิล/งานไม่โผล่** — ยังไม่ได้วินิจฉัย (Skill ไม่ได้อยู่ใน NotImplementedYet; ClusterMode ควรเป็น SingleMode จาก /entry แล้ว)
>    — **ต้องขอ screenshot หน้าเมนูวงกลมในเกม** ก่อน
> 3. **cheat จัดการ POI ยังทำค้าง** — `cheat poi list / poi move <id> <x> <y> / poi remove <id> / poi add <bp> <x> <y>`
>    (API ครบแล้ว: `RemoveArtifact`+`AnnounceGone`, struct AppearArtifact มี Tile แก้ได้) — ไว้แก้ตำแหน่งสด ๆ ไม่ต้องแก้ world.json มือ
> 4. ~~warphole โดนหินทับ~~ ✅ **เทสยืนยันในเกมแล้ว 23 ส.ค. ค่ำ** — 6/7 สะอาด, เจอ+แก้ 1 จุด
>    (`poi_warp_accelerator_1` ลอยกลางน้ำลึก ย้ายเข้าฝั่งแล้ว) ดูหัวข้อบนสุดของไฟล์นี้
> 5. เซิร์ฟ Linux 192.168.1.34 ตอนนี้**ล่ม/SSH เข้าไม่ได้** (Permission denied) — ต้องกลับไป deploy build ใหม่ล่าสุด
>
> ### ⚠️ กับดักที่ต้องจำ
> - **ห้ามใช้ `IsLand()/WaterDepthAt()`** ตัดสินน้ำ/บก — ใช้ `LandDistance()` (oceans.dm) เท่านั้น
> - แก้ world.json ต้อง**หยุดเซิร์ฟก่อน** แล้วรีสตาร์ทถึงจะ re-place POI
> - `EnsureNaturalPOIs` เช็ครายชิ้นแล้ว แก้ save มือเฉพาะ blueprint ที่ผิดได้ (เช่นลบเฉพาะ BlueprintId=dock)
> - **ห้ามเปิด/เซฟ `server/data/config.json` ด้วยเครื่องมือที่เขียนไฟล์เป็น ANSI** — ชื่อไทยจะพังแบบกู้ไม่ได้
>   (ตอนนี้เซิร์ฟซ่อมชื่อให้เองแล้ว แต่ชื่อที่ตั้งเองจะหาย กลับไปเป็นชื่อตั้งต้น)
> - **ต้อง kill `DurangoServer.exe` ก่อน build ทุกครั้ง** ไม่งั้นไฟล์ exe ล็อก (MSB3021)

---

**อัปเดตก่อนหน้า:** 22 ส.ค. 2026 (รอบ 1) — **Main UI ครบวงจร: เปิดเกม → สร้าง/เลือกตัวละคร → เข้าเซิร์ฟได้จริง ✅**

> ### ✅ แก้เสร็จรอบนั้น
> 1. **SelectPlayer error "เชื่อมต่อไม่ได้"** — account ไม่ถูกดึง (ข้าม ShowCluster) ⇒ `ForceSetClusters()` เรียก `UpdateServerAndPlayerInfo()` แล้ว + refresh(force) ทุกครั้งที่เข้า path auto (ตัวละครใหม่ขึ้นใน list)
> 2. **Knock error** — `AutoConnectTarget` เดิม hardcode 192.168.1.34 ไม่สน env ⇒ เปลี่ยนเป็น property อ่าน env ก่อน; `CheckUpdate()` ข้าม UpdateManager/db.kyllox.pe.kr (เว็บเจ้าเก่าตาย)
> 3. **auth เตะกลับหน้าหลัง (token ≠ id)** — `/sessions` ใช้ JSON ค้างตอนบูต (id ว่าง) ⇒ NPAGetUser บังคับใส่ id จาก `GameManager.PlayerId` ทุกครั้ง / ไม่มี context ก็ส่ง minimal JSON; PrologueManager set ทับ id ใหม่เสมอ
> 4. **เลือกตัวละครแล้ววนลูป** — เดิม MoveToTitle() รีโหลด title ⇒ เปลี่ยนเป็น `CurState = State.Knock` เข้าเกมเลย (ทั้งช่องตัวละครและช่องสร้างใหม่)
> 5. **ท่าเรือ/หลุมวาร์ปวางผิด** — 🐛 ใหญ่: `IsLand/WaterDepthAt` ใช้ whole.ocean ที่ตีความไม่ได้ = ค่ามั่ว ⇒ เปลี่ยนมาใช้ **`LandDistance` (oceans.dm)** ทุกจุด; วาง POI เคลียร์ natural ใต้ footprint + เว้นระยะจากของธรรมชาติรอบข้าง ≤3; เช็ค POI **รายชิ้น** (dock หายจะถูกวางซ่อม ไม่ถูกข้ามทั้งชุด); dock บังคับ **ติดน้ำ** (`TouchesWater` ≥2 shore) + ใกล้จุดเกิด
>    - ท่าเรือใกล้จุดเกิดตอนนี้: tile 55,157 · ชุดทั่วเกาะ: 76,51

---

**อัปเดตก่อนหน้า:** 19 ส.ค. 2026 (รอบดึก) — **เล่นจริงรอบแรกหลังทำระบบปลูกผัก + เควสเช็คลิสต์**

> ## 🔴 อ่านตรงนี้ก่อนถ้าเพิ่งเปิดเซสชันใหม่
>
> **ค้างอยู่ที่:** เจ้าของเปิดเกมจริงเล่นดู แล้วรายงานมา 3 เรื่อง — แก้ไปแล้วทั้ง 3 แต่
> **ยังไม่ได้ยืนยันด้วยตาในเกม** เพราะเจ้าของปิดเกมก่อน
>
> ### ✅ ยืนยันแล้ว: **ภาษาไทยขึ้นในเกมจริง ฟอนต์เรนเดอร์ได้ 100%**
> แคปหน้าจอตอนอยู่ในเกมเห็นชัด: "เนื้อสัตว์ต้ม" · "เนื้อ" · "กองไฟ" · ชื่อผู้เล่น "ฟหกฟหก"
> **ไม่มีสี่เหลี่ยม □□□ เลย** ⇒ ปิดความเสี่ยงเรื่องฟอนต์ที่ค้างมาตั้งแต่ TUNING.md §2.1 ได้แล้ว
>
> ### ที่ยังต้องยืนยัน (1 ข้อ)
> - **หน้าเควสมีแท็บ "รายการตรวจเซิร์ฟ" ไหม** และกดเข้าไปเห็น 12 ข้อหรือเปล่า
>   (ยังไม่ได้ดู — เกมถูกปิดก่อนเปิดหน้าเควสทัน)
>
> เปิดเกม: `powershell -File tools\connect-game.ps1` (เซิร์ฟต้องรันด้วย `--enable-cheat`)

---

## 0. สามเรื่องที่เจ้าของรายงานจากการเล่นจริง (19 ส.ค. รอบดึก)

### 0.1 "เทสลิสไม่เห็น เห็นแค่ 2 เควส" — เจอต้นเหตุแล้ว ✅

ตาราง `quests_for_client` มีฟิลด์ `display_on_hud` — **จาก 1,386 เควส เป็น true แค่ 126 อัน**
ใน 20 id ที่เราใช้ เป็น true แค่ 2 อัน คือ `mainstory_chapter1_5` กับ `mainstory_chapter4_6`
⇒ **ตรงกับที่เจ้าของเห็นเป๊ะ ๆ** (client: `if (info == null || !info.DisplayOnHud) return;`)

**แก้:** ทำเป็น **แท็บแยก** แทน — `QuestCategory.Name` มาจาก server ⇒ ตั้งชื่อไทยได้เลย
- หมวดใหม่ `QuestData.ChecklistCategory = "server_checklist"` ชื่อแท็บ **"รายการตรวจเซิร์ฟ"**
- `SendQuestList` ต้อง **แยกเป็นคนละ packet ต่อหมวด** ไม่งั้นแท็บใหม่ว่างเปล่า
- เทส `--quest-check` **33/33**

> ⚠️ **ที่ยังเหลือ:** ถ้าอยากให้ขึ้นบน **HUD** ด้วย ต้องเลือก id จาก 126 อันที่ hud=true
> ซึ่งเกือบทั้งหมดเป็น `mainstory_chapter*` (คำบรรยายเกาหลีจะไม่ตรงกับสิ่งที่ให้ทำเลย)
> — เป็นการแลกกันระหว่าง "เห็นบน HUD" กับ "ข้อความตรงกับงาน" ยังไม่ได้ตัดสินใจ

### 0.2 "ทำเนื้อเสียบไม้ไม่ได้" — เพิ่ม `cheat why` มาตอบ ✅

packet **ไม่เคยมาถึง server เลย** (client ทำปุ่มเป็นสีเทาเงียบ ๆ) ⇒ ดู log ฝั่งเซิร์ฟไม่เจออะไร

คำสั่งใหม่ **`cheat why <ชื่อสูตร>`** ไล่เช็คทุกเงื่อนไขแล้วบอกว่าขาดอะไร:

```
cheat why skewer
สูตร skewer (หมวด cook)
[/] ระบบเปิดอยู่
[/] เลเวล 1 (ต้องการ 1)
[x] ช่อง 'base' ต้องการ 1 ชิ้น — ในกระเป๋ามีที่ใช้ได้ 0 ชิ้น · รับ: eatable
[?] ต้องยืนที่ cook lv1 (client เป็นคนเลือกให้ตอนกดคราฟต์)
[x] ต้องมีเครื่องมือ stick_long / stick_normal / stick_short — ไม่มีในกระเป๋า
```

⇒ **เนื้อเสียบไม้ต้องมีพร้อมกัน 3 อย่าง:** ของกินได้ 1 ชิ้น + **กิ่งไม้ (`wood_bough`) ถือเป็นเครื่องมือ**
+ ยืนที่กองไฟ · ตัวที่มักลืมคือกิ่งไม้

### 0.3 "ช่วยแปลไทยใหม่" — ไม่ต้องแปลเอง เกมมีของแท้อยู่แล้ว ✅ (รอยืนยันฟอนต์)

แกะ `resources.assets` ด้วย UnityPy เจอ **catalog ไทยฉบับทางการของ NEXON**:

| | |
|---|---|
| ไฟล์ | gettext `.mo` — **7,479,988 ไบต์ · 33,066 ข้อความ** |
| ความครบ | สุ่มตรวจ 4,000 รายการแรก เป็นไทย 3,877 (~97%) |
| ผู้แปล | May Cho ทีมโลคัลไลซ์ NEXON · PO-Revision-Date 2019-12-09 |

**ติดตั้งแล้วที่** `game/locales/th-TH/LC_MESSAGES/messages.mo`
(ก๊อปไว้ทั้ง `th-TH` · `th_TH` · `th` เผื่อ NGettext resolve คนละแบบ)

ทำไมเส้นทางนี้ถึงใช้ได้: `T.InstallCatalog(locale)` → `new Catalog("messages","locales",Culture)`
อ่านจาก **โฟลเดอร์จริงข้างตัวเกม** ไม่ผ่าน `resources.assets` ที่เสีย (ENV-01)
ส่วนอีกเส้น `Resources.Load("offline/i18n/th_TH")` พังเพราะ ENV-01 — นั่นคือเหตุผลที่เกมยังเป็นเกาหลี

สคริปต์ที่ใช้แกะ: `scratchpad/extract_i18n.py` (ถ้าหาย: UnityPy loop หา TextAsset ชื่อ `th_TH`)

✅ **เทสในเกมจริงแล้ว — ฟอนต์ไทยมาครบ** (แคปหน้าจอ 19 ส.ค. รอบดึก)
เห็น "เนื้อสัตว์ต้ม" · "เนื้อ" · "กองไฟ" · ชื่อผู้เล่นไทย ครบทุกตัว ไม่มีสี่เหลี่ยม
⇒ **ความเสี่ยงเรื่อง atlas ฟอนต์ที่ค้างมาตั้งแต่ TUNING.md §2.1 ปิดได้แล้ว**
NEXON เปิดเซิร์ฟไทยจริง ฟอนต์ไทยเลยติดมากับบิลด์ด้วย

---

## 0.9 สรุปของใหม่ในรอบนี้ทั้งหมด

| อย่าง | ไฟล์ |
|---|---|
| ระบบปลูกผัก 53 ชนิด | `docs/server/Farming.md` · `--farm-check` 39/39 |
| รายการตรวจในเควส 12 ข้อ + แท็บแยก | `docs/server/Quest-Checklist.md` · `--quest-check` 33/33 |
| `cheat why <สูตร>` | `ServerPlayer.Crafting.ExplainRecipe()` |
| `cheat checklist` · `cheat farm/seeds/grow/farms` · `cheat save` | `ServerPlayer.Cheat.cs` |
| catalog ไทย | `game/locales/th-TH/LC_MESSAGES/messages.mo` |

**ชุดเทสเต็มล่าสุด: 198 ข้อ ตก 0** (quest 33 · farm 39 · gp 45 · vision 12 · multi 9 · stat 19 · character 17 · skill 13 · cook 11)

---

**เดิม:** 19 ส.ค. 2026 — **ระบบเควส: สายสอนเล่น → ต่อแพหนีเกาะ**

รายละเอียด: [docs/server/Quests.md](docs/server/Quests.md) · เทส: `เทสเกม.bat` ข้อ **24** (`--quest-check`)

- 🐛 **เควส 1,386 อันในข้อมูลเกมมีแต่ "หน้าตา" ไม่มี "สมอง"** — `quests_for_client` มี 8 ฟิลด์
  (ชื่อ/คำอธิบาย/ไอคอน/ลำดับ/หมวด/ชนิด/HUD/จบเอง) **ไม่มีเงื่อนไข-เป้าหมาย-รางวัลสักฟิลด์**
  ⇒ ใช้ **id ของจริง** แล้วเขียนเงื่อนไข/รางวัลเองที่ `QuestData.cs`
- ✨ **สายสอนเล่น 8 ขั้น จบที่ต่อแพ** — เก็บของ → คราฟต์เครื่องมือ → เก็บท่อนซุง → ล่า → แล่ →
  ทำอาหาร → สร้าง → **ต่อแพ `tutorial_boat`** (id ปลายทาง `story_enter_safehouse` เควสเนื้อเรื่องจริง
  คำบรรยายเกาหลีคือ "ต้องหนีออกจากอังโครา สร้างแพ" ตรงกับที่ให้ทำเป๊ะ)
- ✨ ตัวนับ **ไม่ได้เขียนใหม่สักตัว** — เกี่ยวกับ `GainExpForGather/Kill/Butchery/Craft/Build` ที่มีอยู่แล้ว
- 💡 **ชื่อหมวดเควสมาจาก server** (`QuestCategories.Epic.Name`) ⇒ ใส่ไทยได้เลยวันนี้ ("เอาชีวิตรอด")
  ส่วนชื่อเควสรายอันยังเป็นเกาหลีจนกว่าจะเปิดแค็ตตาล็อกไทย (ดู docs/client/TUNING.md §2.1)
- 🔓 เลิกซ่อนเมนู `Quest` + `CategoryToDo` แล้ว — **ต้อง build client ใหม่ถึงจะเห็นเมนู**
- 🧪 `cheat quests` ดูสถานะ · `cheat questskip` ข้ามไปขั้นสุดท้าย · `cheat gather`/`attack` สั่งตัวเองได้แล้ว

**ผลเทส:** `--quest-check` **20/20** (ต่อแพด้วย packet จริง OccupyArtifactSite → BuildArtifact)
regression ผ่านหมด: vision 12/12 · gp 45/45 · multi 9/9 · stat 19/19 · character 17/17 ·
group2 20/20 · skill 13/13 · cook 11/11

> ⚠️ **`Features.Quests` ต้องเปิดใน `data/config.json` ด้วย** — ค่าเริ่มต้นในโค้ดไม่ทับไฟล์ที่มีอยู่แล้ว
> (เสียเวลาไปหนึ่งรอบเพราะเรื่องนี้: โค้ดตั้ง true แล้วแต่ไฟล์ยังเป็น false ⇒ ได้เควส 0 อัน)
>
> 🔴 **การสร้างสิ่งปลูกสร้างยังไม่กินวัสดุ** (`PutMaterialsIntoArtifact` ตอบ OK เฉย ๆ)
> ⇒ ต่อแพได้โดยไม่ต้องมีท่อนซุงจริง — ช่องโหว่ของระบบก่อสร้าง ควรแก้แยก

---


**อัปเดตล่าสุด:** 19 ส.ค. 2026 — **ระยะการมองเห็น (interest management)**

รายละเอียด: [docs/server/Vision.md](docs/server/Vision.md) · เทส: `เทสเกม.bat` ข้อ **23** (`--vision-check`)

- 🐛 **เดิมส่งทุกอย่างให้ทุกคนในเกาะโดยไม่ดูระยะ** จาก **47 จุดที่เรียก `Broadcast`**
  รวมการเดินของผู้เล่นและของสัตว์ทุกตัว ⇒ โตแบบ N² · **ที่ 100 คน ≈ 20,000 packet/วินาที**
- ✨ ตอนนี้ส่งเฉพาะสิ่งที่อยู่รอบตัว — เหลือ `Broadcast` แบบเดิม **3 จุด และเป็นแชททั้งหมด**
  · `BroadcastToViewers(entityId, msg)` ข่าวของ entity → เฉพาะคนที่เห็นมันอยู่
  · `BroadcastNear(pos, msg)` เหตุการณ์ผูกกับจุดในโลก · `Announce*` ของเกิดใหม่/ถูกลบ
- ✨ **รอบตรวจทุก 0.4 วิ** ส่ง `Appear`/`Disappear` ตอนเข้า-ออกระยะ (ผู้เล่น · สัตว์ · สิ่งปลูกสร้าง)
  ⚠️ ต้องมีทั้งการกรองตอนส่ง **และ** รอบตรวจ ขาดอย่างหลัง = คนเดินเข้ามาใหม่จะไม่เห็นใครเลย
  ⚠️ `Appear` ต้องออกทาง `Observe*` เท่านั้น ยิง `MakeAppear()` ตรง ๆ = รอบตรวจส่งซ้ำอีกที
- ✨ เข้าเกมแล้วส่งเฉพาะของรอบตัว (เดิมส่งทั้งเกาะ — ที่ 100 คนคือ ~4,000 `AppearArtifact` ชุดเดียว)
- ⚙️ ปรับสดได้ที่ `config.json` → `World`: `ViewRangeTiles` 24 · `ViewMarginTiles` 8 ·
  `ViewCheckSeconds` 0.4 · **`ViewCulling: false` = กลับพฤติกรรมเดิมทันที** (ไว้ตัดตัวแปรตอนหาบั๊ก)
- 🧪 `cheat tp <tileX> <tileY>` — วาร์ปตัวเองไว้เทสระยะ

**ผลเทส:** `--vision-check` **12/12** (อยู่ไกลกันได้ packet การเดินของอีกฝ่าย **0** · ที่จุดเกิดเห็นสัตว์ 17/34)
regression **ผ่านหมดทุกชุด**: gp 45/45 · multi 9/9 · stat 19/19 · character 17/17 · group2 20/20 ·
skill 13/13 · cook 11/11 · stamina 16/16 · tool 8/8 (ชุด tool ใช้เวลา >5 นาที อย่าตั้ง timeout 300 วิ)

> ⚠️ **กับดักตอนรันเทส:** ถ้า `dotnet run` ค้างจากรอบก่อนแล้วรันซ้ำ **log จะปนกัน 2 รอบในไฟล์เดียว**
> (เจอมาแล้ว: อ่านได้ "43/2" ทั้งที่รันสะอาดได้ 45/45) — `taskkill /F /IM dotnet.exe` ก่อนรันใหม่
> แล้วเช็คว่าไฟล์มีบรรทัด "สรุป" เพียงบรรทัดเดียว

---


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

**แก้จากการเล่นจริงรอบล่าสุด 3 ข้อ** (CHANGELOG หัวข้อรอบที่ 4 ของสาย "เล่นจริง" — คนละอันกับ 2026-08-19 รอบ 4)
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

**อ่านก่อนวางแผน:** [docs/project/GOAL.md](docs/project/GOAL.md) (เกมนี้จะเป็นอะไร) · [docs/project/ROADMAP.md](docs/project/ROADMAP.md) (beta 4 รอบ → เปิดจริง)
**ก่อนเทส:** [docs/testing/TESTPLAN.md](docs/testing/TESTPLAN.md) — รายการเทสทั้งหมด (อัตโนมัติ + เช็คลิสต์เล่นจริง + เกณฑ์ผ่าน)

**เหลือก่อนเปิดจริง:** เล่นด้วยตัวเกมจริง 30 นาทีเป็นรอบสุดท้าย (เกณฑ์ข้อ 3 ใน
[docs/testing/BETA-1.0-PLAN.md](docs/testing/BETA-1.0-PLAN.md) §4) แล้วเปิดได้เลย

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

เอกสารสำหรับ**ผู้ทดสอบ/ผู้เล่น** (ว่าจะได้เจออะไรบ้าง): `docs/operations/BETA-1.0-PLAYERS.html` — เปิดในเบราว์เซอร์ได้เลย

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

เกณฑ์ทั้ง 5 ข้ออยู่ใน `docs/testing/BETA-1.0-PLAN.md` §4 — ผ่านแล้ว 4 ข้อ เหลือข้อ 3:

| # | เกณฑ์ | สถานะ |
|---|---|---|
| 1 | `--gp-check` ผ่านครบ | ✅ 45/45 |
| 2 | บอทฟาร์ม 30 นาที: exception 0 · tps ≥100 · RAM ไม่โตเกิน 20% | ✅ 120 tps · RAM นิ่ง |
| 3 | **เล่นด้วยตัวเกมจริง 30 นาที** | ⏳ เล่นแล้ว 2 รอบ · รอบล่าสุดเจอ 3 เรื่อง แก้แล้วทั้งหมด **แต่ยังไม่ได้ยืนยันในเกม** (ดู §0) |
| 4 | 3 client พร้อมกัน: เห็นกันครบ · ไม่มีของก๊อป | ✅ `--multi-check` 9/9 |
| 5 | เปิดทิ้งไว้ไม่มีคนเล่น: เซฟไม่โต · สัตว์ครบโควตา | ✅ (เทส 15 นาที ไม่ใช่ 3 ชม.) |

ข้อ 3 ต้องดู: สัตว์ไม่วาร์ป/ไม่ค้างท่า · ตาย-ฟื้นแล้วจอเด้งไปจุดเกิดจริง ·
ออกจากโหมดต่อสู้ได้ · กระเป๋าเต็มแล้วกด "ทิ้ง" ได้ · กินของแล้วสตามินาขึ้น
**เพิ่มจากรอบที่แล้ว:** คลิกสัตว์แล้วปุ่มโจมตีขึ้นไหม · โดนสวนกลับไวพอไหม ·
ฆ่าแล้วซากเรืองแสงไหม · แตะซากแล้วมีเมนูเนื้อ/หนัง/กระดูก และแล่แล้วของเข้ากระเป๋าจริงไหม
(ออกจากโหมดต่อสู้ก่อนถึงจะแตะซากได้ — ตอนอยู่ในโหมดต่อสู้ client ไม่เปิดเมนูให้)

**ตอนนี้มีเช็คลิสต์ในเกมแล้ว ไม่ต้องถือกระดาษ** — เปิดหน้าเควส แท็บ "รายการตรวจเซิร์ฟ"
หรือพิมพ์ `cheat checklist` ดูรายการไทยพร้อมความคืบหน้า · คราฟต์อะไรไม่ได้พิมพ์ `cheat why <สูตร>`

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

### 19 ส.ค. (รอบ 4) — รายการตรวจเซิร์ฟอยู่ในเควสแล้ว

**เกณฑ์เปิด beta ข้อ 3 (เล่นเกมจริง 30 นาที) ไม่ต้องถือกระดาษเช็คลิสต์แล้ว**
ยกรายการมาเป็นเควส 12 ข้อ เปิดพร้อมกันหมด — เดินเล่นไปก็รู้เองว่าเหลือระบบไหน
ดู `docs/server/Quest-Checklist.md`

- ระหว่างเล่นพิมพ์ **`cheat checklist`** ดูรายการภาษาไทย + ความคืบหน้า
- เทสผ่านหมดแล้วปิดด้วย `data/config.json` → `"QuestChecklist": false` (hot-reload)
- 🐛 เจอบั๊กจริง 2 ข้อระหว่างทำ: เควสไม่เจาะจงถูกนับสองเด้ง (เควส "สร้าง 2 อย่าง"
  จบตั้งแต่ชิ้นแรก) · `cheat questskip` ข้ามเควสต่อแพ — แก้แล้วทั้งคู่

| ชุดเทสเต็ม | ผล |
|---|---|
| quest 33 · farm 39 · gp 45 · vision 12 · multi 9 | ตก 0 |
| stat 19 · character 17 · skill 13 · cook 11 | ตก 0 |
| **รวม 198 ข้อ** | **ตก 0** |

---

### 19 ส.ค. (รอบ 3) — ระบบปลูกผัก

พืช **53 ชนิด** จากข้อมูลเกมจริง · ปลูก → รดน้ำ/ใส่ปุ๋ย → รอโต → เก็บเกี่ยว → ได้เมล็ดคืน
รายละเอียด: `docs/server/Farming.md` · เทส `--farm-check` **39/39**

| ชุดเทสเต็มหลังทำเสร็จ (เซิร์ฟรีสตาร์ทใหม่) | ผล |
|---|---|
| farm 39 · quest 26 · gp 45 · vision 12 · multi 9 | ตก 0 |
| stat 19 · character 17 · skill 13 · cook 11 | ตก 0 |
| **รวม 191 ข้อ** | **ตก 0** |

**สิ่งที่ต้องรู้ถ้าจะแก้ต่อ:**
- แปลงผัก = artifact ที่ blueprint มี component `Growable` (`farm_tile_01..04`)
- **เก็บเกี่ยวไม่มี packet ของตัวเอง** — ใช้ `Touch`/`Collect` ชุดเดียวกับของธรรมชาติ
  (client ไม่มี `Interaction.Harvest`)
- เมนูตอนแตะมาจาก `Touched.Interactions` ล้วน ๆ — ไม่ใส่เลข = ไม่มีปุ่มอะไรขึ้นเลย
- `data/config.json` → `Farming.GrowthScale` (0.05 = เร็วกว่าเกมจริง 20 เท่า)
- cheat: `farm` · `seeds` · `grow` · `farms` · `save`

---

### 19 ส.ค. — ระบบเควส + ตรวจระบบสร้าง (ชุดเทสเต็ม เซิร์ฟรีสตาร์ทใหม่)

| ชุดเทส | ผล |
|---|---|
| `--quest-check` (เควส) | **26 / 0** |
| `--gp-check` (กันโกง) | **45 / 0** |
| `--vision-check` (ระยะมองเห็น) | **12 / 0** |
| `--multi-check` (3 คนพร้อมกัน) | **9 / 0** |
| `--stat-check` (ค่าสถานะ) | **19 / 0** |
| `--character-check` | **17 / 0** |
| `--skill-check` | **13 / 0** |
| `--cook-check` | **11 / 0** |
| **รวม** | **152 ข้อ ตก 0** |

แก้บั๊กระบบสร้าง 4 ข้อในรอบนี้ (สร้างซ้ำปั๊ม exp/เควส · ขนาดจาก client ไม่จำกัด ·
เช็คทับซ้อนแค่ tile มุม · ทุบไม่เช็คระยะ) — รายละเอียดที่ `docs/server/Building-Audit.md`

⚠️ **ระบบปลูกผักยังไม่มีในเซิร์ฟเลย** (packet ฝั่ง client พร้อมแล้ว · ข้อมูล `crops` มีแล้ว)

---

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
9. **เทสแกว่งเอง = state ค้าง ไม่ใช่โค้ดพัง** — ก่อนสรุปว่าโค้ดพัง ให้ **รีสตาร์ทเซิร์ฟแล้วรันใหม่**
   (`cheat spawn` สะสมสัตว์ 35→45 ตัวต่อ session ⇒ packet ตอน login โตจน `Pump(500)` อ่านไม่ทัน)
   และดูว่าไฟล์ผลเทสมี **บรรทัดสรุปซ้อนกันสองอัน** ไหม — เคยมี `dotnet run` ค้างจาก timeout
   เขียนทับไฟล์เดียวกันจนอ่านผลผิดมาแล้ว (`taskkill /F /IM dotnet.exe` ก่อนรันชุดเต็ม)
10. **สูตรคราฟต์ขึ้นสีเทา = client บล็อกเอง packet ไม่เคยมาถึง server** — ดู log ฝั่งเซิร์ฟไม่เจออะไรเลย
   ใช้ `cheat why <สูตร>` แทนการเดา (เจอตอนเทสจริง: "ทำเนื้อเสียบไม้ไม่ได้")
11. **เควสจะโผล่บน HUD ได้ต้อง `display_on_hud: true` ในตารางของเกม** — มีแค่ 126 จาก 1,386 อัน
   id ที่ hud=false จะ**เงียบหายไปเฉย ๆ** (client null-check แล้ว return) ไม่มี error ให้เห็น
12. **ไฟล์ `resources.strings.txt` ไม่ใช่ JSON ก้อนเดียว** — asset ชื่อซ้ำได้ (`performance` มี 3 ก้อน)
   มี asset ดิบ (ไบนารี) แทรกกลาง และ **ปีกกาปิดของ asset ท้าย ๆ หายไปเฉย ๆ**
   ⇒ ตัวสกัดต้องนับปีกกาเอง อย่าเชื่อการเยื้องบรรทัด (ดู `scripts/extract_crops.py`)
13. **`Features.*` ใน `data/config.json` ทับค่า default ในโค้ดเสมอ** — `Quests: false` ที่ค้างอยู่
   ทำให้ระบบเควสคืน 0 เควสโดยไม่มี error ให้เห็น
14. **Firewall** — ตอน Windows Defender ถาม ผมกด **Cancel** ไม่ใช่ Allow (เป็นการเปลี่ยนความปลอดภัยเครื่อง เจ้าของเครื่องควรตัดสินเอง) เทสผ่าน loopback ไม่ต้องใช้

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
