# รายงานความสามารถ — เราทำอะไรได้บ้างกับ Durango Opencode

สำรวจ 23 ส.ค. 2026 โดยอ่านโค้ดจริงทั้ง `client/` (3,845 ไฟล์) และ `server/` (3,538 ไฟล์)
บวกแกะ `game/DurangoV2_Data/resources.assets` ด้วย UnityPy + manual byte parsing

สัญลักษณ์: ✅ ยืนยันจากโค้ด/หลักฐานจริง · ⚠️ น่าจะใช่แต่ไม่ 100% · ❓ ยังไม่รู้

---

## 1. สถาปัตยกรรม client

- ✅ Unity **2017.4.34f1** + NGUI, โค้ด client 3,845 ไฟล์ (`.cs`) จาก ILSpy decompile
- ✅ Build จริงได้: `tools/build-client.ps1` รัน `dotnet build client\Assembly-CSharp.csproj -c Release`
  แล้ว copy ไปทับ `game/DurangoV2_Data/Managed/Assembly-CSharp.dll` — สำรอง DLL เดิมไว้ที่ `game-backup/`
  ยืนยันผลทดสอบจริง 16 ส.ค.: build 0 error (~5 วิ, DLL 5.8MB), เกมบูตถึงหน้าไตเติ้ล, ต่อเซิร์ฟสำเร็จ
  (`docs/client/BUILD.md`)
- ✅ **ข้อจำกัดสำคัญ (ENV-01)**: `resources.assets` ถูก Unity เองรายงานว่า corrupted
  (`game/client.log`: `The file '.../resources.assets' is corrupted! [Position out of bounds!]`,
  และ `level3` scene ก็เจอเช่นกัน) — **นี่คือ noise ปกติที่มีอยู่แล้วในบิลด์ต้นฉบับทุกครั้งที่รัน
  แม้ไม่แตะไฟล์เลย ไม่ fatal เกมยังบูตต่อได้ปกติ** (ยืนยันแล้วโดยเทียบกับ DLL+resources.assets ต้นฉบับล้วน ๆ
  23 ส.ค.) แต่หมายความว่า `[SerializeField]` ตัวไหนก็อาจเป็น null ได้ไม่มีแพทเทิร์นตายตัว
  (`docs/client/TUNING.md:45-56`)
- ✅ ขอบเขตที่แก้ได้จริง (`docs/client/TUNING.md:25-43`): ตรรกะ C# ทุกบรรทัด, ข้อความ, ตำแหน่ง/สี widget,
  ซ่อน/โชว์ UI, พฤติกรรมปุ่ม — **แต่แก้ prefab/atlas/ฟอนต์/scene ใหม่ไม่ได้** เพราะไม่มี TypeTree ให้ repack
- ✅ `Json.ReadFromFile` (`client/Durango.Utils/Json.cs:88`) อ่านข้อมูลเกมส่วนใหญ่ผ่าน
  `Resources.Load(...) as TextAsset` จาก resources.assets — TextAsset ธรรมดา (เช่น catalog แปลภาษา,
  `offline/clusters`) แก้ได้ปลอดภัยด้วย UnityPy โดยตรง (ต้อง assign เป็น `str` ปกติ **ห้ามใช้
  `surrogateescape`** — เคยทำให้ serializer พังไฟล์ทั้งไฟล์มาแล้ว) แต่ MonoBehaviour/ScriptableObject
  serialized data (เช่น UIPrefabMap) อ่าน field ผ่าน UnityPy ปกติไม่ได้เลยเพราะไม่มี TypeTree — ต้อง
  manual byte-parse ตามลำดับ field ในซอร์สโค้ดเอาเอง (ดูหัวข้อ 3)

## 2. สถาปัตยกรรม server

- ✅ โปรโตคอล: header 24 ไบต์ (Time/Seq/ReplyOf/TypeCode/PayloadSize) + payload = MsgPack → Snappy
  (`docs/ARCHITECTURE.md` §2, encode/decode ที่ `server/GameCode/Durango.Network/Packet.cs`)
- ✅ พอร์ต: Gateway HTTP **8190**, GameServer TCP **8191**, RadiotowerServer TCP **8192**
- ✅ เซฟเป็นไฟล์ JSON ล้วน ไม่มี database engine — `server/saves/players/*.json` (39 ไฟล์ ณ ตอนสำรวจ)
  + `server/saves/world.json`
- ✅ Main loop เธรดเดียว 120 tps ใน `Program.cs`
- ✅ ระบบ cheat/admin: `--enable-cheat` flag เปิด cheat commands ในเกม (`cheat spawn/heal/tp/poi/...`)
  ผ่าน `ServerPlayer.Cheat.cs` และไฟล์ `CheatPOI.cs` ฯลฯ

### Admin web panel (`server/admin/index.html` + `server/ServerCore/Gateway.Admin.cs`, 301 บรรทัด)

ยืนยันจากอ่านโค้ดจริง — endpoint ทั้งหมดที่มีจริง:

| Method | Path | ทำอะไร |
|---|---|---|
| GET | `/admin`, `/admin/` | เสิร์ฟหน้า HTML จาก `server/admin/index.html` อ่านจากดิสก์ทุกครั้ง |
| GET | `/admin/status` | tps, online_players, alive_animals, ram_mb, uptime, cheats_enabled ฯลฯ |
| GET | `/admin/players` | รายชื่อผู้เล่นออนไลน์ (entity_id, name, level, tile, hp, dead) |
| POST | `/admin/players/kick` | เตะผู้เล่นออกด้วย entity_id + reason |
| POST | `/admin/players/teleport` | วาร์ปผู้เล่นไป tile x,y |
| GET | `/admin/poi` | รายการ POI (ท่าเรือ/หลุมวาร์ป) + filter `?problems=1` |
| POST | `/admin/poi/move` | ย้าย POI |
| POST | `/admin/poi/remove` | ลบ POI |
| POST | `/admin/poi/add` | เพิ่ม POI ใหม่จาก blueprint |
| GET/POST | `/admin/config` | อ่าน/เขียน `data/config.json` สด (hot-reload ทันที ไม่รอ 5 วิ) |
| GET | `/admin/log` | log สดแบบ poll (`?after=<cursor>`) |
| POST | `/admin/cheat` | สั่ง cheat command ในนามผู้เล่นออนไลน์ที่เลือก (ต้อง `--enable-cheat`) |

⚠️ **ไม่มีระบบ auth** — ตั้งใจ เพราะ Gateway bind แค่ localhost/แลนเจ้าของเซิร์ฟ ไม่ได้ expose อินเทอร์เน็ต
— **ถ้าจะเปิดสาธารณะต้องเพิ่ม auth เอง**

## 3. ระบบ UI — PC vs Mobile

### ยืนยันจากโค้ด
- `client/UIPrefabMap.cs:6` — `[ResourcePath("ui_prefab_map")]`, มี `Type.Mobile`/`Type.PC` และ array
  `_mainMobile`/`_prologueMobile`/`_titleMobile` คู่กับ `_mainPC`/`_prologuePC`/`_titlePC` (+`_prologueAdditional*`)
- `client/Durango.System/Platform.cs:102-114` — `UIType` คืน `Type.PC` ถ้า `UsePCUI == true`
- `client/Durango.System/Platform_PC.cs:20` — `public override bool UsePCUI => true;` (hardcode)
- `client/Durango.System/Platform.cs:124-127` — static constructor ของ `Platform` เซ็ต
  `Instance = new Platform_PC()` แบบไม่มีเงื่อนไขใด ๆ เลย (ไม่ใช่ `#if UNITY_ANDROID` แยก build) —
  เกม build นี้เป็น PC-only เต็มรูปแบบ, `Platform_Android.cs` มีแค่ `RequestPermission` override เดียว
  ไม่ override `UsePCUI` เลย (โค้ด Android เต็มถูกตัดออกตอน build ต้นฉบับ)

### ✅ สำรวจ resources.assets ยืนยันแล้ว — Mobile UI prefab **มีข้อมูลจริง ไม่ใช่ array ว่าง**

UnityPy อ่าน MonoBehaviour ปกติไม่ได้เลย (17,257 จาก 17,298 MonoBehaviour ใน `resources.assets` throw
"Expected to read X bytes, but only read Y bytes" — ตรงกับ ENV-01 ที่บันทึกไว้แล้วว่าไฟล์นี้ไม่มี TypeTree)
แต่หาสตริง `ui_prefab_map` เจอ **1 ครั้ง** ตรงกับ object path_id **27088** (byte_size=3,020 ไบต์) —
ใช้ลำดับ field ตามที่ declare ใน `UIPrefabMap.cs` มา parse ไบต์ดิบเองแบบ manual (ไม่พึ่ง TypeTree)
แล้ว**พอดีกับขนาด object เป๊ะ (จบที่ offset 3020/3020)**:

| field | จำนวน GameObject ref |
|---|---|
| `_mainMobile` | **95** |
| `_prologueMobile` | **11** |
| `_titleMobile` | **3** |
| `_mainPC` | 96 |
| `_prologuePC` | 11 |
| `_titlePC` | 3 |
| `_prologueAdditionalMobile` | 13 |
| `_prologueAdditionalPC` | 13 |

สุ่มตรวจ path_id ที่ `_mainMobile`/`_titleMobile`/`_prologueMobile` ชี้ไปจริง 10 ตัว — **ทุกตัวเป็น
object type `GameObject` จริง** และมีชื่อตรงกับกลุ่ม UI ที่คาดว่าจะมี เช่น `TitleMenuGroup`,
`TitlePlayerSelectionGroup`, `PrologueCharacterSelectGroup`, `AlarmGroup`, `ArtifactInfoGroup` ฯลฯ

**สรุป: Mobile UI prefab set แทบจะครบเท่า PC (95 vs 96 / 3 vs 3 / 11 vs 11 / 13 vs 13) — ถูก bundle มาจริง
ไม่ได้ว่างเปล่า**

### คำตอบ: "บังคับให้เกมใช้ Mobile UI ได้จริงไหม"

- ✅ **ข้อมูล prefab พร้อมใช้** (ยืนยันข้างบน)
- ⚠️ **วิธีบังคับ (ยังไม่เคยลองจริง)**: แก้ `Platform.cs:124-127` (static ctor) หรือ `Platform_PC.cs:20`
  ให้ `UsePCUI => false` แล้ว build ใหม่ — ทำได้ตามหลักการ (ไม่แตะชื่อ class/field ที่ Unity ผูกไว้ จึงไม่เสี่ยง
  "referenced script missing") แต่ **ยังไม่เคยรันจริงในเกม** ต้องเทสว่า: (1) Unity's native loader เองยัง
  deserialize object นี้ได้ตอน runtime จริงไหม (แม้ manual parse จะสำเร็จ แต่ตัวเกมเองยัง log
  "resources.assets is corrupted" อยู่ — ไม่รับประกันว่า field อื่นในไฟล์เดียวกันจะโหลดได้เสมอ)
  (2) UI มือถือออกแบบมาสำหรับจอสัมผัส จะแสดงผล/ควบคุมด้วยเมาส์-คีย์บอร์ดยังไงไม่รู้ (3) `DefaultUISize`/
  `SupportPortrait`/`UsePCRenderer` ก็ผูกกับ `Platform` เดียวกัน จะเปลี่ยนตามไปด้วย
- ❓ ยังไม่ได้ตรวจ `StreamingAssets/AssetBundles/` เลย — เวลาไม่พอในรอบนี้ ควรตรวจเพิ่มถ้าต้องการความมั่นใจ
  สูงสุด แต่จากหลักฐานที่ resources.assets มีข้อมูล Mobile ครบแล้ว ความจำเป็นลดลงมาก

## 4. สิ่งที่ทำงานได้แล้วในเซสชันนี้ (ยืนยันจาก log/save จริง)

- ✅ เข้าเกมได้สำเร็จเต็มรูปแบบ (23 ส.ค.) — title screen → prologue (หน้ารถไฟ) → สร้างตัวละคร → เข้าเกาะจริง
  ครบทุกขั้นตอนไม่พัง มีไฟล์เซฟจริง 39+ ไฟล์ที่ `server/saves/players/*.json`, HUD/Combat system ทำงานปกติ
- ✅ ตัวละครสร้างมาถึงเซิร์ฟครบ (ชื่อ/เพศ/หน้าตา) — แก้บั๊กใน `Gateway.cs` (`/players`, `/sessions`) และ
  `PrologueManager.cs`, เทสผ่าน `--create-check` 12/12
- ✅ ระบบเควส: สายสอนเล่น 8 ขั้น → ต่อแพหนีเกาะ, เช็คลิสต์ "รายการตรวจเซิร์ฟ" ในเกม 12 ข้อ (`--quest-check`
  เคยได้ 33/33 แต่ **ค้างอยู่ 30/33 ตอนนี้** — 3 ข้อตกตั้งแต่งาน POI/เกาะรอบก่อน)
- ✅ ระบบ POI (จุดสนใจ) — cheat `poi list/move/remove/add` มี API ครบ ตอนนี้เพิ่มเข้าถึงผ่าน admin web
  panel ได้แล้วด้วย
- ✅ ระบบปลูกผัก 53 ชนิด (`--farm-check` 39/39), ทำอาหาร 152 สูตร (`--cook-check` 11/11), แล่เนื้อ,
  ระบบเลเวล/สกิล, กันโกง (`--gp-check` 45/45), หลายคนพร้อมกัน (`--multi-check` 9/9)
- ✅ **แก้บั๊กเซิร์ฟสำคัญ 2 จุด (23 ส.ค.)**: `WebServer.JsonResponse` ส่ง `Content-Type: json` (ผิด MIME
  มาตรฐาน ขาด `application/`) และไม่ตั้ง `ContentLength64` (fallback เป็น chunked) — ทั้งสองทำให้ BestHTTP
  (เก่าในเกม) มองว่า HTTP response ทุกตัว fail แม้ status 200 จริง แก้แล้วทั้งคู่ที่
  `server/GameCode/Durango.Offline/WebServer.cs`
- ⏳ **ยังไม่ยืนยันด้วยตาในเกมจริงรอบล่าสุด**: เมนูสกิล/งานไม่โผล่ (ยังไม่วินิจฉัย), warphole โดนหินทับ
  (แก้โค้ดแล้วแต่ยังไม่ทดสอบซ้ำ)

## 5. จุดเสี่ยง/บั๊กที่มีบันทึกไว้แล้ว — แผนที่กับดัก

grep `[แก้เอง]` ทั่วโปรเจกต์เจอ **82+ จุด**, `[ย้อนกลับ]` 1 จุด, `[ชั่วคราว]` 2 จุด กระจุกตัวที่:

| ไฟล์ | จำนวนจุด | เรื่องหลัก |
|---|---|---|
| `client/Durango.UI/TitleMenuGroup.cs` | 9 | บังคับใช้เซิร์ฟของเราเอง, ข้ามเลือกคลัสเตอร์, บั๊กเมนูสกิล/คราฟต์/เควสหายทั้งชุด |
| `client/Durango.Prologue/PrologueManager.cs` | 8 | ข้าม cutscene รถไฟ (23 ส.ค. — ทำสำเร็จโดยไม่พังซ้ำรอบก่อน), บั๊กจอดำถาวรผู้เล่นใหม่ |
| `client/Durango.UI/RecipeSelectorGroup.cs` | 6 | UI คราฟต์ |
| `client/GameManager.cs` | 5 | ระบบย้ายเกาะ, null check |
| `client/Durango.Offline/Server.cs` | 5 | autoconnect ผ่าน env, `ConnectTo` รับ `ip:port` |
| `server/ServerCore/ServerWorld.cs` | 3 | ใช้ `LandDistance` (oceans.dm) แทน `IsLand/WaterDepthAt` ที่พัง |
| `client/UIBase.cs` | 3 | `CloseAllUI` บังคับปิด UI ที่ค้าง, UI dump debug |

**กับดักเฉพาะที่ควรจำ**:
- ห้ามใช้ `IsLand()/WaterDepthAt()` ตัดสินน้ำ/บก — ใช้ `LandDistance()` เท่านั้น
- ต้อง `kill DurangoServer.exe`/`dotnet.exe` ก่อน build ทุกครั้ง ไม่งั้นไฟล์ exe ล็อก
- `dotnet run` ใช้ **Debug** config เสมอ (ไม่ใช่ Release) — ถ้า build แค่ `-c Release` แล้ว `dotnet run`
  จะยังใช้ DLL Debug เก่า ต้อง build Debug ด้วยถ้าจะรันผ่าน `dotnet run`
- `Start-Process -ArgumentList` ของ PowerShell ไม่ auto-quote path ที่มี space (เช่น "Durango Opencode")
  ทำให้ `-logFile <path>` แยก argument ผิดแบบเงียบ ๆ — ต้อง manual-quote เอง
- แก้ resources.assets ด้วย UnityPy ห้ามใช้ `surrogateescape` ตอน assign string (พังไฟล์มาแล้วจริง)
- `Features.*` ใน `data/config.json` ทับ default ในโค้ดเสมอ
- เควสจะโผล่บน HUD ต้อง `display_on_hud: true`
- สูตรคราฟต์ขึ้นสีเทา = client บล็อกเอง packet ไม่ถึง server เลย — ใช้ `cheat why <สูตร>` แทนเดา

## 6. ระบบที่มีศักยภาพแต่ยังไม่เคยแตะ/ทดสอบ

- ✅ ยืนยันจากโค้ด: `data/config.json` → `Features` ปิดอยู่ **8 ระบบ**:
  `Jobs: false`, `Livestock: false`, `Taming: false`, `Market: false`, `Pvp: false`,
  `LandPermission: false`, `PartyAndClan: false`, `Emotes: false`
- ✅ Message/protocol struct สำหรับ Party/Clan/Pet/Market **มีครบอยู่แล้ว** ใน `server/GameCode/Messages/`
  (เช่น `MakeParty.cs`, `MakeClan.cs`, `SpawnPet.cs`, `MarketCollectPayment.cs` ฯลฯ — พบ 90+ ไฟล์ที่ชื่อ
  เกี่ยวกับ Party/Clan/Pet/Market/Job) และมี `Shared.Clan`, `Shared.ClanFund`, `Shared.Market`, `Shared.Pet`
  เป็น data type ฝั่ง shared
- ✅ **แต่ `server/ServerCore/` (โค้ด logic จริงของเซิร์ฟ) ไม่มีไฟล์ implement ระบบเหล่านี้เลย** — grep หา
  `RegisterHandlers()` ไม่พบการลงทะเบียน handler ของ Party/Clan/Pet/Market สักตัว ⇒ struct/protocol พร้อม
  แต่ **ยังไม่มีสมอง** เหมือนที่เควสเคยเป็นก่อนแก้
- ⚠️ Pet/Taming มี client-side script จำนวนมาก (`PetAI.cs`, `PetManager.cs`, `PetExtension.cs`,
  `TameableHelper.cs`) และ Shared data — น่าจะทำได้ในแนวเดียวกับที่ทำเควส/farming สำเร็จมาแล้ว แต่ต้อง
  ออกแบบ state machine เอง (server ยังไม่มีต้นแบบให้ก็อป)
- ❓ **หลังบีตา 1.0**: แชทส่วนตัว (ต้องทำ auth ให้ radiotower ก่อน), elevation/ความสูงพื้นจาก `ChunkData`
  (ไม่มีข้อมูลนี้ในเกม server ใช้ค่าที่ client รายงานแทน)

## 7. คำถามที่ยังค้างอยู่ (ต้องสำรวจเพิ่ม)

- ❓ ยังไม่ได้ scan `game/DurangoV2_Data/StreamingAssets/AssetBundles/` เลย — มีไฟล์เป็นพันไฟล์
- ❓ ยังไม่ได้ลองรัน build จริงพร้อม `UsePCUI => false` เพื่อยืนยันว่า Mobile UI render ได้จริงในเกม
- ❓ `--quest-check` ตก 3 ข้อ ยังไม่วินิจฉัยสาเหตุ
- ❓ เมนูสกิล/งานไม่โผล่ในเกม — ยังไม่มี screenshot ยืนยันรอบล่าสุด
