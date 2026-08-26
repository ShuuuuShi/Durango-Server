# แท็บสกิลว่างในโหมด online — แก้แล้ว (26 ส.ค. 2026)

## อาการ

เปิดแท็บสกิลในเกม (ผ่านเซิร์ฟบ้าน) เห็นแค่ช่องเดียว ข้อความ "tile" มีกรอบเหลือง หมวดสกิลอื่นทั้งหมด (~13 หมวด: เอาชีวิตรอด/ต่อสู้ระยะประชิด/ยิงธนู/ป้องกัน/ชำแหละ/เก็บเกี่ยว/ทำอาหาร/ตีอาวุธ/ตีชุด/ก่อสร้าง/ฟาร์ม/แปรรูป/สกิลซีซัน 2) ไม่โผล่เลย

ภาพที่เจ้าของถ่ายตอนเจอบั๊ก (โหมด online):

![แท็บสกิลว่าง โผล่แค่ tile กรอบเหลือง](shots/skill-tab-bug-online.png)

## ไล่บั๊ก

ทดสอบเทียบ **offline vs online**:

- **offline** (client โหลดข้อมูล terrain จาก Unity Resources bundle ของตัวเอง) — แท็บสกิลขึ้นปกติ ครบทุกหมวด
- **online** (ผ่านเซิร์ฟบ้าน 127.0.0.1) — บั๊กทุกครั้ง

สงสัยผิดทางไป 2 รอบก่อนเจอของจริง:

1. **cheat mode** — ปิด `--enable-cheat --admin gm` แล้วรีสตาร์ทเซิร์ฟ ยังบั๊กเหมือนเดิม → ไม่ใช่
2. **world save เก่าเพี้ยน** — ลบ `world.json` + player/account saves ทั้งหมด ให้เซิร์ฟสร้างโลกใหม่สะอาด ยังบั๊กเหมือนเดิม → ไม่ใช่

จุดที่เจ้าของสั่งให้ไล่ต่อ: "ไล่หาให้เจอว่าเป็นที่ตรงไหน ห้ามเดา แก้ให้น้อยที่สุด"

## สาเหตุจริง (ยืนยันด้วยการเช็คไฟล์จริง)

`server/DurangoServer.csproj` **ไม่มี directive copy `server/data/` ไป output directory เลย**

ตรวจแล้วพบว่า `server/bin/Debug/net9.0/data/` (โฟลเดอร์ที่เซิร์ฟรันจริง) มีแค่ `config.json` — ไม่มี `terrains/`, `islands/`, `islands.json`, `whitelist.txt` ทั้งที่ `server/data/` (source) มีครบ

ห่วงโซ่ที่ทำให้เกิดบั๊ก:

1. `TerrainStore.Load(dataDir, terrainId)` หา `data/terrains/extracted/ri35te/info.yml` → **หาไม่เจอ**
2. `File.Exists(infoPath)` เป็น false → `Info = new TerrainInfoJson()` (ค่าเริ่มต้นเปล่าทั้งหมด) → `region_template` เป็น `null`
3. `GameServer.SendWelcome()` ส่ง `Region.TemplateId = _world.Terrain.Info.region_template` = `null` ให้ client ผ่าน packet `Welcome`
4. client: `SkillCategoryWidget.Init()` เรียก `SingletonDict<string, RegionTemplate>.Get(null)` ได้ `null`
5. `if (regionTemplate == null) { return; }` — ออกจากฟังก์ชันทันที **ก่อน**สร้าง `_categoryList` และก่อนเรียก `nodes.Set()`
6. grid ของหมวดสกิลเลยค้างอยู่ในสภาพ prefab เริ่มต้น = template node เปล่า 1 ช่องที่เห็นเป็น "tile"

ทำไม offline ไม่เจอบั๊ก: client โหลด terrain info จาก Unity Resources bundle ของตัวเอง (`Durango.Offline/TerrainLoader.cs`) ซึ่งมี `region_template` ฝังอยู่ในไฟล์อยู่แล้ว ไม่ได้พึ่งเซิร์ฟเลย

## แก้อะไรบ้าง

### 1. `server/DurangoServer.csproj` (แก้ตัวจริง)

เพิ่ม `<ItemGroup>` ให้ copy `server/data/**/*` (ยกเว้น `config.json` ที่ลงทะเบียนแยกอยู่แล้ว) ไป output directory อัตโนมัติทุกครั้งที่ build:

```xml
<ItemGroup>
  <None Include="..\server\data\**\*" Exclude="..\server\data\config.json">
    <Link>data\%(RecursiveDir)%(FileName)%(Extension)</Link>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

### 2. `client/Durango.UI/SkillCategoryWidget.cs` + `SkillGroup.cs` (defensive fix)

ไม่ใช่ต้นเหตุจริง แต่เก็บไว้เพราะไม่มีผลเสียและกันเคสอื่นในอนาคตที่ Region/Template อาจยังไม่พร้อมจริง ๆ ตอน `Init()`:

- ดึงลอจิกสร้างรายการหมวดออกเป็น `BuildCategoryList()` — เคลียร์ `_categoryList` ก่อนเสมอ (idempotent เรียกซ้ำได้)
- เพิ่ม `Rebuild()` public method
- `SkillGroup.OnOpened()` เรียก `_skillCategory.Rebuild()` ทุกครั้งที่เปิด UI

### 3. `client/Durango.Logic.Skill/Util.cs` (บั๊กแฝงที่เจอระหว่างทาง)

ชื่อหมวด `Weaponcrafting`/`Armorcrafting` ขึ้นเป็น raw localization key (`#Shared.Skill.Category.Weaponcrafting`) แทนที่จะเป็นชื่อจริง เพราะข้อมูล localization (`localize_text_enum`) ใช้ชื่อ enum เก่า `WeaponCrafting`/`ArmorCrafting` (ตัว C ใหญ่) ส่วน enum ปัจจุบันคือ `Weaponcrafting`/`Armorcrafting` (ตัว c เล็ก) — แก้ `CategoryLocalizeName()` ให้เทียบ key เก่าก่อน

## ยืนยันผลจริงในเกม

1. ปิด cheat mode, ลบโลกเก่า, สร้างโลกใหม่
2. เพิ่ม copy directive ใน `.csproj` แล้ว build เซิร์ฟใหม่ → ตรวจว่า `server/bin/Debug/net9.0/data/terrains/extracted/ri35te/info.yml` มีจริง และ `region_template: "ri35teSub01"` อยู่ในนั้น
3. build client ใหม่ (ตัด debug log ที่เคยใส่ไว้ระหว่างไล่บั๊กออกแล้ว) → ติดตั้ง DLL
4. เปิดเซิร์ฟ + เปิดเกม เชื่อมต่อ online (ไม่มี cheat, โลกใหม่)
5. เจ้าของเช็คเองในเกม → **แท็บสกิลขึ้นครบทุกหมวด** — ยืนยัน "ผ่านแล้ว"

## บทเรียน

อาการ "ทำงานถูกต้อง offline/retail แต่พังเฉพาะตอนต่อเซิร์ฟบ้าน" ควรเช็คก่อนเป็นอันดับแรกว่าโฟลเดอร์ output จริงของเซิร์ฟ (`server/bin/Debug/net9.0/`) มีไฟล์ข้อมูลตรงกับ source (`server/data/`) ไหม — .NET SDK-style project ไม่ copy โฟลเดอร์ content ที่อยู่นอก project directory ให้อัตโนมัติ ต้องประกาศ `<None Include>`/`<Content Include>` เอง

## อ้างอิง

- รายงานบั๊กเต็ม: `docs/bug-report-memorybot-beta.md` หัวข้อ H3
- `HANDOFF.md` — หัวข้อ "แก้บั๊กแท็บสกิลว่างในโหมด online" (26 ส.ค. 2026)
- ภาพ MemoryBot ตอนเทสระบบสกิล (แยกเซสชัน แต่ยืนยันว่าเข้าเกมได้ปกติในตอนนั้น): `shots/main-scene-memorybot-check.png`
