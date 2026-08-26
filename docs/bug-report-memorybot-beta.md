# รายงานบั๊ก — เทสด้วย MemoryBot (beta readiness)

**วันที่:** 26 ส.ค. 2026
**วิธีเทส:** ใช้ `DurangoMemoryBot` (TCP 8193) อ่าน managed state แบบ whitelist + สั่งงาน semantic + admin cheat API (HTTP 8190) ตั้งเงื่อนไขเทส
**ตัวละคร:** `252` (entity `ce96e22b-42d6-4ab2-97f3-f1db9ec9f40b`) — local เท่านั้น ยังไม่ deploy production

---

## ✅ ผลการแก้ (26 ส.ค. 2026)

| รหัส | สถานะ | หมายเหตุ |
|------|-------|---------|
| H1 | ✅ แก้แล้ว | `combat.use_action` ตรวจ action กับ `GetCurrentBattleActions()` + cooldown/prohibited ก่อนตอบ → `action_not_available` / `action_on_cooldown` / `action_prohibited` |
| H2 | ✅ แก้แล้ว | `player.move_to` ตรวจขอบเขตเทียบ `TerrainMeta.TileCount * 200` → `position_out_of_bounds` |
| H3 | ✅ แก้แล้ว (ยืนยันในเกมจริง) | แท็บสกิลโผล่หมวดเดียว ("tile" + กรอบเหลือง) — สาเหตุจริง: `DurangoServer.csproj` ไม่ copy `server/data/terrains/` ไป runtime ⇒ `region_template` เป็น null ⇒ client `SkillCategoryWidget.Init()` ออกก่อนสร้างรายการหมวด ⇒ เพิ่ม copy directive ใน csproj + defensive `Rebuild()` ตอนเปิด UI |
| M3 | ✅ แก้แล้ว | `Die()` zero life velocity → หลังตาย `life` เป็น `0` จริง ไม่ใช่ `~0.5` |
| M1 | ⏸ เลื่อน | เป็น design decision ที่บันทึกในโค้ดแล้ว (AFK กับนั่งพักแชร์ ID เดียวกัน) — ใช้งานได้ ไม่ใช่บั๊กฟังก์ชัน; แยก ID ต้องเพิ่ม client data ใหม่ เสี่ยงไอคอนหาย |
| M2 | ⏸ เลื่อน | ชื่อเกาหลีคือข้อมูลต้นฉบับของเกม (`ItemNameData.cs` สร้างจาก `prototype_data` 2777 รายการ) — ไม่ใช่บั๊กที่แนะนำเข้ามา; แปลไทยเป็นงาน localization แยกต่างหาก |

---

## สรุป

ระบบหลัก **ทำงานได้จริง** ผ่าน MemoryBot ครบ: เดิน · ต่อสู้ · เก็บ/ใช้ของ · บัฟ/ดีบัฟ · นั่งพัก · ตาย · เลเวลอัป
แต่พบจุดที่ควรแก้ก่อนเปิด beta ตามรายการข้างล่าง ไม่มีบั๊กระดับ "ของหาย / เซิร์ฟล่ม" แต่มีหลายจุดที่ทำให้ **เทสอัตโนมัติหลงทางหรือให้ผลลัพธ์ผิด** และจุด UX ที่คนเล่นจริงจะเจอ

---

## 🔴 High — ควรแก้ก่อน beta

### H1. คำสั่ง `combat.use_action` / `interaction.execute` คืน "accepted" ทั้งที่ action ไม่มีจริง
- **อาการ:** สั่ง `combat.use_action action_id=barehand_combination` (ท่าที่ต้องเรียน `reckless` lv2) ผ่าน bot → ตอบ `accepted` แต่จริง ๆ client **ไม่ส่ง packet ไปเซิร์ฟเลย** (ท่าไม่โผล่ใน `GetCurrentBattleActions()` ที่ถูกกรองแล้ว)
- **ผลกระทบ:** บอท/ระบบอัตโนมัติคิดว่าทำสำเร็จ แต่เกมไม่เกิดอะไร → เทสล้มโดยไม่รู้ตัว และถ้าเอา pattern นี้ไปใช้ควบคุมจริงจะวินิจฉัยยาก
- **สาเหตุ:** `MemoryBotCommands.Execute()` เรียก `combat.UseBattleAction(id)` แล้วตอบ accepted ทันที โดยไม่เช็คว่า action id อยู่ในรายการที่ client เปิดอยู่หรือไม่
- **ที่:** `tools/MemoryBotMod/MemoryBotCommands.cs` (combat.use_action / interaction.execute)
- **แนวแก้:** เช็ค action id กับ `GetCurrentBattleActions()` (และ `MenuList` สำหรับ interaction) ก่อนตอบ; ถ้าไม่มี → `rejected: action_not_available`

### H2. `player.move_to` ไม่ตรวจขอบเขตโลก
- **อาการ:** `player.move_to x=-999999 y=-999999` ถูกตอบ `accepted` และตัวละครเดินออกนอกโลก (เดินไปทางพิกัดลบ)
- **ผลกระทบ:** ตัวละครเดินออกนอกเกาะ/นอกโลกได้ถ้าใช้บอท; แม้เซิร์ฟน่าจะ clamp ใน `Move` packet ภายหลัง แต่ตัวละครเดินผิดทางเปล่า ๆ
- **ที่:** `tools/MemoryBotMod/MemoryBotCommands.cs` (player.move_to)
- **แนวแก้:** validate/clamp พิกัดก่อนเรียก `MoveToPosition` หรือให้เซิร์ฟปฏิเสธ move นอกขอบเขตอย่างชัดเจน

### H3. แท็บสกิลโผล่หมวดเดียว ("tile" + กรอบเหลือง) สกิลอื่นหาย
- **อาการ:** เปิดแท็บสกิลแล้วเห็นแค่ช่องเดียว ข้อความ "tile" มีกรอบเหลือง หมวดสกิลอื่นไม่โผล่ (ควรมี ~13 หมวด: เอาชีวิตรอด/ต่อสู้ระยะประชิด/ยิงธนู/ป้องกัน/ชำแหละ/เก็บเกี่ยว/ทำอาหาร/ตีอาวุธ/ตีชุด/ก่อสร้าง/ฟาร์ม/แปรรูป/สกิลซีซัน 2)
- **ผลกระทบ:** ผู้เล่นเปิดเมนูสกิลแล้วใช้การไม่ได้ — เป็น UI หลักที่บล็อกการเล่นต่อ (beta-blocking)
- **สาเหตุที่แท้จริง (ยืนยันด้วยการทดสอบสด ไม่ใช่แค่โค้ด):** `server/DurangoServer.csproj` **ไม่มี directive copy ข้อมูล `server/data/` ไปที่ output directory** (`server/bin/Debug/net9.0/data/`) — โฟลเดอร์ที่รันจริงมีแค่ `config.json` ไม่มี `terrains/`, `islands/`, `islands.json`, `whitelist.txt` เลย
  - `TerrainStore.Load()` หา `data/terrains/extracted/<terrainId>/info.yml` ไม่เจอ → `Info = new TerrainInfoJson()` (ค่าเริ่มต้นเปล่า) → `region_template` เป็น **null**
  - `GameServer.SendWelcome()` ส่ง `Region.TemplateId = _world.Terrain.Info.region_template` = null ให้ client
  - client: `SkillCategoryWidget.Init()` → `SingletonDict<string, RegionTemplate>.Get(null)` = null → `if (regionTemplate == null) return;` ออกก่อนสร้าง `_categoryList` เลย → grid ไม่เคยเรียก `nodes.Set()` → เหลือแค่ template node เปล่า 1 ช่อง ("tile")
  - ยืนยันด้วยการทดสอบเปรียบเทียบจริง: โหมด **offline** (client โหลด terrain จาก Unity Resources ของตัวเอง ซึ่งมี `region_template` ครบ) ไม่มีบั๊ก แต่โหมด **online** (ผ่านเซิร์ฟบ้าน) บั๊กเสมอ แม้ปิด `--enable-cheat` และลบโลกสร้างใหม่แล้วก็ยังบั๊ก — ตัดทุกสมมติฐานอื่น (cheat mode, world save เก่า, mod, Season filter) ออกจนเหลือจุดเดียว: เซิร์ฟไม่มีข้อมูล terrain ที่ runtime path
  - **หมายเหตุ:** สมมติฐานแรก ("Init() รันก่อน Welcome มาถึง") เป็นไปได้ในทางทฤษฎีแต่ไม่ใช่สาเหตุจริงของบั๊กนี้ — เก็บการแก้ `Rebuild()` ไว้เป็น defensive fix เพราะไม่เสียหายและกันเคสอื่นที่ template อาจยังไม่พร้อมจริง ๆ ได้
- **ที่:** `server/DurangoServer.csproj` (สาเหตุจริง) + `client/Durango.UI/SkillCategoryWidget.cs`/`SkillGroup.cs` (defensive fix)
- **แนวแก้:**
  1. เพิ่ม `<ItemGroup>` ใน `DurangoServer.csproj` copy `server/data/**/*` (ยกเว้น `config.json` ที่มีอยู่แล้วแยกทะเบียน) ไป output dir ด้วย `CopyToOutputDirectory=PreserveNewest` ทุกครั้งที่ build
  2. ดึงลอจิกสร้างรายการหมวดสกิลออกเป็น `SkillCategoryWidget.BuildCategoryList()` (เคลียร์ `_categoryList` ก่อน → idempotent) แล้วเรียก `Rebuild()` จาก `SkillGroup.OnOpened()` ทุกครั้งที่เปิด UI (defensive — กันเคส Region ยังไม่พร้อมจริง ๆ ในอนาคต)
- **หมายเหตุ (บั๊กแฝงที่เจอระหว่างแก้):** ชื่อหมวด 2 หมวด (`Weaponcrafting`/`Armorcrafting`) ขึ้นเป็น raw key `#Shared.Skill.Category.Weaponcrafting` เพราะ localization data (`localize_text_enum`) ใช้ชื่อ enum เก่า `WeaponCrafting`/`ArmorCrafting` (ตัว C ใหญ่) — แก้แล้วใน `client/Durango.Logic.Skill/Util.cs` (`CategoryLocalizeName` เทียบ key เก่าก่อน)
- **ยืนยันแล้วในเกมจริง:** เปิดเซิร์ฟ (มี terrain data ที่ runtime) + เปิดเกม online → แท็บสกิลขึ้นครบ ผู้เล่นยืนยันเอง ("ผ่านแล้ว")
- **รายละเอียดเต็มพร้อมภาพหน้าจอตอนเจอบั๊ก:** `docs/server/Skill-Tab-Blank-Fix.md`

---

## 🟡 Medium — ควรรู้ไว้ / แก้รอบถัดไป

### M1. ไอคอนบัฟ `away_from_keyboard` ใช้ร่วมกันระหว่าง "นั่งพัก" กับ "AFK/Idle"
- **อาการ:** สถานะ `away_from_keyboard` ปรากฏทั้งตอนนั่งพัก และตอนตัวละครยืนเฉย ๆ (Idle) — UI แยกไม่ออกว่ากำลังพักจริง หรือแค่ AFK
- **ที่:** `client/SleepChecker.cs` (Sleep/WakeUp ใช้ `away_from_keyboard`) vs `server/ServerCore/ServerPlayer.Group2.cs` (rest ใช้ `away_from_keyboard`)
- **ผลกระทบ:** คนเล่นเห็นไอคอน "พัก" ทั้งที่ไม่ได้พัก → เข้าใจผิดเรื่องบัฟ
- **หมายเหตุ:** ในโค้ดมีคอมเมนต์รับทราบแล้ว ("SleepChecker ใช้ away_from_keyboard ร่วมกับบัพนั่งพัก") แต่เป็นหนี้ทางออกแบบที่ควรแยก ID ให้ชัดก่อน beta ถ้าทำได้

### M2. ชื่อไอเทมโชว์เป็นเกาหลี ไม่ใช่ไทย
- **อาการ:** `read inventory` ได้ `name: "고기"` (เนื้อ) ทั้งที่ส่วนอื่นของเกมมี localization ไทย
- **ผลกระทบ:** ข้อมูล state ที่บอท/ระบบอ่านได้เป็นเกาหลี บอทต้อง map เอง; ผู้เล่นไทยเห็นชื่อไอเทมเกาหลีบางชิ้น
- **ที่:** `server/ServerCore/ItemNameData.cs` (map ชื่อไอเทมไม่ครบ)

### M3. หลัง `die` ค่าเลือดค้างที่ ~0.5 ไม่ใช่ 0
- **อาการ:** หลัง `die` อ่าน `survival` ได้ `life: 0.528` ทั้งที่ `alive:false` / `/admin/players` รายงาน `dead:true`
- **ผลกระทบ:** โค้ดที่เช็ค `life <= 0` แทนที่จะเช็ค `Dead` flag อาจทำงานผิดพลาด
- **ที่:** `server/ServerCore/ServerPlayer.Death.cs` (ลำดับการตั้งค่า gauge กับ Dead flag)

---

## 🟢 Low — ข้อมูลสำหรับรอบถัดไป

### L1. `give` cheat clamp ไม่ตรงผลจริง
- `cheat give meat 60` โค้ด clamp ไว้ที่ 50 (`ServerPlayer.Cheat.cs:181`) แต่ผลจริงได้แค่ ~20 ชิ้นต่อครั้ง (น่าจะมี cap อีกชั้นใน `GiveByPrototype`)
- ไม่กระทบผู้เล่น (เป็น tool เทส) แต่ทำให้เทสกระเป๋าเต็ม/สแตกหลงทาง

### L2. เก็บของธรรมชาติ (natural) ที่ spawn ไม่มีของให้เก็บ
- ที่ tile (132,127) มี `natural_132_127` แต่ `gens=0` (หมดแล้ว) → `world.nearby` ไม่คืน natural ตัวนั้น
- เป็นพฤติกรรมที่ถูกต้อง (ของเก็บหมด) แต่ทำให้เทส gathering ผ่านบอทต้องวาร์ปหาจุดใหม่บ่อย

---

## ✅ ระบบที่เทสแล้วทำงานถูกต้อง (หลักฐาน)

| ระบบ | ผล | หลักฐาน |
|------|-----|---------|
| เดิน / หยุด | ผ่าน | `move_to` ขยับจริง, `stop` หยุดจริง, ตำแหน่งจาก `player.local` เปลี่ยน |
| ต่อสู้ครบวงจร | ผ่าน | spawn → touch → attack → สัตว์หนี → ไล่ตี → ตาย → `exp 22` → เลเวล 1→4 → MeleeCombat 1→2 |
| สกิลเกตท่าต่อสู้ | ผ่าน | fresh player ได้ 6 ท่า barehand ตรงตาม `ActionUnlockData`; `barehand_combination`/`melee_tackle` ไม่อยู่ในลิสต์ |
| สถานะพิษ | ผ่าน | `poisoning` → HP ลดต่อเนื่อง, `effect clear` ล้างหาย |
| สถานะฟื้นเลือด | ผ่าน | `life_up` → HP ฟื้นต่อเนื่อง (87.6 → 92.4 ใน 3 วิ) |
| นั่งพัก | ผ่าน | `away_from_keyboard` ขึ้น + fatigue velocity `-4` + fatigue ลด; เดินหนี/เข้าต่อสู้แล้วบัพหายถูกต้อง |
| ใช้ของ/กิน | ผ่าน | `inventory.use` → ไอเทมหายจากกระเป๋า (5→4), สถานะอัปเดต |
| ตาย | ผ่าน | `die` → `alive:false`, `/admin/players` รายงาน `dead:true` |
| กระเป๋าเต็ม | ไม่พบ overflow | ให้ของเกินแล้วยังไม่ทะลุ capacity (30 < 50) |

---

## ⚠️ ข้อจำกัดการเทสรอบนี้ (ยังไม่ได้ยืนยันด้วยตา)

- **เมนูคราฟต์ / skill-gating ซ่อนของฟรี** — ต้องดูในเกมจริง (MemoryBot read path ยังไม่มี `craft`/`recipe` เฉพาะ)
- **ฟื้นจากจุดเกิด / Death Point** — ไม่มี command revive ใน MemoryBot จึงยังไม่ได้เทส UI ฟื้น
- **PvP** — ระบบปิดอยู่ (`Features.Pvp=false`) ยังไม่ได้เทสข้อกำหนดเลเวล 20

---

## คำแนะนำลำดับแก้ก่อน beta

1. แก้ H1 + H2 (เป็นจุดที่บอท/เทสอัตโนมัติให้ผลลวง และเสี่ยงตัวละครหลุดโลก)
2. ตัดสินใจเรื่อง M1 (แยก ID ของบัพพักกับ AFK) — ถ้าทำได้เร็ว ทำเลย
3. เติม read path `craft`/`recipe` + command `revive` ให้ MemoryBot แล้วเทสเมนูคราฟต์/ฟื้นซ้ำ
