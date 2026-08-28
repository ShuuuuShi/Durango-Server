# `ServerCore/Gateway.cs`

**หน้าที่:** HTTP ที่พอร์ต **8190** — ทุกอย่างก่อนที่ client จะเปิด TCP เข้าเกม
(ตรวจเวอร์ชัน, สร้าง session, บอกที่อยู่ game server, ส่ง terrain, ส่ง asset bundle)

## Constructor — บรรทัด 45
สร้าง `WebServer` แล้วลงทะเบียน route ทั้งหมดเป็น lambda

### `GET /knock`
บอกเวอร์ชัน + ที่อยู่ asset bundle
```csharp
["server_version"] = "5.2.1"
["compatible"] = true                                        // ⚠️ true เสมอ
["assetbundle_index_url"] = $"http://{host}/assetbundles/Info.5.2.1.json"
```
`host` เอามาจาก `request.UserHostName` (= host ที่ client พิมพ์เข้ามา) จึงใช้ได้ทั้ง localhost และ LAN
⚠️ `compatible = true` ตายตัว → โฮสต์กับแขกคนละเวอร์ชันก็ต่อกันติด แล้วไปพังตอน deserialize (GP-19)

### `POST /sessions` — จุดที่ข้อมูลผู้เล่นเข้ามา
client ส่ง JSON เซฟทั้งก้อนมาในฟิลด์ `player` โค้ดพยายามแกะจากหลายที่ (เผื่อคนละรุ่นตั้งชื่อคีย์ต่างกัน):
```
appear_player.EntityId / entity_id   → entityId
appear_player.Name / name            → ชื่อ
appear_player.Level / EntityType     → เลเวล / เพศ
appear_player.Display                → หน้าตา (เก็บเป็น string ดิบ)
player_info.player_entity_id / player_name    → ทับอีกที
skills / skill_points / known_skills → สกิล
```
แล้ว `RegisterName()` + `RegisterPlayerData()` + **`IssueSession()`** → ตอบ `{user_id, session_token}`

ห่อ try/catch ทั้งก้อน — JSON เพี้ยนก็ยังเข้าเกมได้ (แต่จะไม่มีหน้าตา/สกิล)

✅ **GP-12 แก้แล้ว** — `session_token` เป็นค่าสุ่ม 64 ตัวอักษรที่ `GameServer` ออกให้และผูกกับ entity id
เดิมคืน `entityId` ตรง ๆ ⇒ ใครเห็น id ของคนอื่น (มากับ `AppearPlayer` ทุก packet) ก็ Auth เป็นเขาได้

✅ **GP-14 แก้แล้ว** — ข้อมูลตรงนี้ยัง "มาจาก client 100%" เหมือนเดิม (เลี่ยงไม่ได้ เพราะเป็นเซฟของเกาะเขา)
แต่ฝั่ง `ServerPlayer` ไม่เชื่อดื้อ ๆ แล้ว: เลเวลใช้ได้เฉพาะ login แรก + ตัดที่เพดาน 60,
entity type ต้องอยู่ช่วง 1000-1999 — ดู [ServerPlayer.Core.md](ServerPlayer.Core.md)

### `GET /admission`
`{"admitted": true}` เสมอ — ไม่มีระบบคิว/แบน

### `GET /entry`
```csharp
["frontend_addresses"]   = ["127.0.0.1:" + _gameServer.Port]     // พอร์ตจริง ไม่ใช่ค่าคงที่
["radiotower_addresses"] = ["127.0.0.1:" + _radiotowerPort]
["cluster_mode"] = "SingleMode"
```

> ✅ **แก้แล้วตอนเทสกับเกมจริง** เดิมใช้ `GameServer.DefaultPort` (ค่าคงที่ 8191)
> พอรันด้วย `--game-port 8391` (ซึ่งจำเป็นเพราะ dll ที่ patch แล้วใช้ default 8390/8391)
> gateway ยังบอก client ให้ต่อ 8191 → **client วิ่งไปพอร์ตที่ไม่มีใครฟัง**
> ตอนนี้ `GameServer.Port` / `RadiotowerServer.Port` จำพอร์ตที่ `Start()` เปิดจริง
- `frontend` เป็น `127.0.0.1` ก็ยังใช้ได้ เพราะ **client เขียนทับด้วย IP จริงของ gateway ให้เอง**
  (แต่มันแก้แค่ `frontend_addresses[0]` เท่านั้น)
- `radiotower` **ไม่ถูกเขียนทับ** → ถ้าวันไหนเปิด Online mode เครื่องแขกจะวิ่งไปหา 127.0.0.1 ของตัวเอง (GP-06)
- `cluster_mode = "SingleMode"` คือสวิตช์สำคัญ: ทำให้ client โหลดข้อมูลเกมจากไฟล์ในเครื่อง
  ถ้าเปลี่ยนเป็น `"Online"` มันจะมาขอ 71 ไฟล์จากเราแทน — ดู [ARCHITECTURE.md ข้อ 7](../project/ARCHITECTURE.md)

### `POST /players`
สร้างตัวละครใหม่ — ตอนนี้แค่สุ่ม GUID + จำชื่อ แล้วตอบ `entity_id` (ไม่ได้สร้าง state จริง)

### `GET /terrains/1` และ `/terrains/1/whole_biomes`
ส่ง `TerrainInfoJson` (เป็น JSON) และ biome ทั้งแมพ (เป็น binary)

## `UnhandledUrl(url)` — บรรทัด 166
route ที่มีพารามิเตอร์อยู่ใน path จับด้วย prefix แทน

**`/assetbundles/<ชื่อไฟล์>`** — ส่งไฟล์จาก `_assetBundleDir`
มีการกัน path traversal 3 ชั้น:
```csharp
string safeName = Path.GetFileName(fileName);
if (safeName != fileName || fileName.Contains("..")) → BadRequest
```
ตัด query string ออกก่อน แล้วเทียบว่าชื่อหลังตัด directory ต้องเท่าเดิม — `../../windows/system32` ผ่านไม่ได้

**`/terrains/1/ocean/<x>,<y>`** / **`/rivers/...`** → ส่ง chunk เดียว
**`/terrains/1/<x>,<y>`** → ต่อ biome + ocean + river + landmark เป็นก้อนเดียวส่งกลับ

อย่างอื่นทั้งหมด → `BadRequestResponse`
⚠️ นี่คือเหตุผลที่เปิด Online mode ตอนนี้ไม่ได้ — `/assets/...` จะโดน BadRequest แล้ว client ค้างหน้าโหลด

## `Point2FromUrl(url)` — บรรทัด 240
แกะ `"12,34"` ท้าย URL เป็น `Point2`
⚠️ `int.Parse` ไม่ห่อ try — URL มั่ว ๆ จะโยน exception (WebServer จับให้ชั้นบน แต่ควรใช้ `TryParse`)

## `Process()` — บรรทัด 247
`_webServer.Process()` — ระบายคิว HTTP ที่ callback สะสมไว้ ทำงานบน main thread เสมอ
