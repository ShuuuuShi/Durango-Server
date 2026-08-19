# `ServerCore/GameServer.cs`

**หน้าที่:** รับ TCP ที่พอร์ต **8191** ทำ handshake (`GetClock` → `Auth` → `Ready`) แล้วสร้าง `ServerPlayer`
พร้อมเก็บข้อมูลผู้เล่นที่ `/sessions` ส่งมาล่วงหน้า

## `PlayerData` (nested class, บรรทัด 40)
กล่องเก็บสิ่งที่ client บอกมาตอน `/sessions` ก่อนจะต่อ TCP:
`EntityId` `Name` `DisplayJson` `Level` `EntityType` `SkillsJson` `SkillPoints` `KnownSkillsJson`
เก็บใน `_playerData` (มี lock) รอจนกว่าคนนั้นจะ `Ready` แล้วค่อยเอามาใช้

## `Start(port)` → `bool` ✅ GP-15
`_listener.Start(port)` คืน false ถ้า bind ไม่สำเร็จ → `Start()` คืน false ต่อ แล้ว `Program.cs` พิมพ์ `[fatal]` และจบโปรแกรม
เดิมกลืน exception แล้ว `Process()` พ่น `ArgumentNullException` ทุก tick — เซิร์ฟดูเหมือนรันอยู่แต่ไม่รับใคร

## `Listener_ClientAccepted(socket)` — บรรทัด 73 ★ handshake ทั้งหมดอยู่ตรงนี้

สร้าง `Connection` ใหม่ เก็บเข้า `_connections` แล้วผูก handler 3 ตัว
(`entityId` / `playerName` เป็นตัวแปร closure — แยกกันต่อ connection)

**1. `GetClock`** → ตอบ `Clock { ClientTime = ที่ส่งมา, ServerTime = ตอนนี้ }`
client เอาไปคำนวณ latency + ออฟเซ็ตเวลา ทำให้ animation/timer ตรงกันสองฝั่ง

**2. `Auth`** → `TryAuthorize()` แล้วค่อย `SendWelcome()` ✅ GP-12
ไม่ผ่าน = ตอบ `Abort` + `connection.Close()` + log ว่าใครอ้างเป็นใคร

## Session token ✅ GP-12

| เมทอด | หน้าที่ |
|---|---|
| `IssueSession(entityId, data)` | `Gateway` `/sessions` เรียก — คืน token สุ่ม 64 ตัวอักษร ผูกกับ entity id + `PlayerData` (อายุ 12 ชม. เก็บกวาดของหมดอายุตอนออกใบใหม่) |
| `TryAuthorize(auth, out …)` | token ต้องมีจริง ยังไม่หมดอายุ และ **entity id ที่อ้างต้องเป็นของ token นั้น** |
| `RequireSessionToken` | `false` = โหมด `--insecure-auth` กลับไปเชื่อ entity id ดื้อ ๆ (debug เท่านั้น) |
| `TrustClientProfile` | GP-14 — `true` (`--trust-client-profile`) = เชื่อเลเวลจาก client ทุกครั้ง |

token **ไม่ถูกลบหลังใช้** เพราะ client ใช้ใบเดิมตอน reconnect (`GameManager.SendAuthMessage(isReconnect: true)`)
เดิม `/sessions` คืน `session_token = entity id` ⇒ ใครเห็น `AppearPlayer` ของคนอื่นก็สวมรอยได้ทันที

ตัวทดสอบที่ไม่ผ่าน HTTP ต้องขอ token เองก่อน — ดู `test-client/SessionClient.cs`

**3. `Ready`** → จุดที่ผู้เล่นเกิดจริง:
```
entityId ว่าง → connection.Close() แล้วจบ      (กันคนข้าม Auth)
ตอบ OK
data = ข้อมูลที่ผูกมากับ token (GP-12) ถ้าไม่มีค่อย GetPlayerData(entityId)
player = new ServerPlayer(...)
player.RegisterHandlers()        ← ผูก handler 32 ตัว
player.SendSpawnBurst()          ← ยิงสถานะเริ่มต้น
_world.AddPlayer(player)         ← บอกคนอื่น
ServerKnock.HostName = playerName
ผูก ConnetionClosed → _world.RemovePlayer(player)
```
ลำดับสำคัญ: `RegisterHandlers` ต้องมาก่อน `SendSpawnBurst` (ไม่งั้นคำตอบที่ client ยิงกลับมาไม่มีคนรับ)
และ `AddPlayer` มาท้ายสุด (ไม่งั้นคนอื่นเห็นเราก่อนที่เราจะพร้อม)

✅ **GP-11 แก้แล้ว** — เดิมมี `ServerKnock.HostName = playerName` ตรงนี้ ซึ่งทับชื่อเซิร์ฟ
ด้วยชื่อผู้เล่นคนล่าสุด ทำให้ LAN discovery โชว์ชื่อผิด ตอนนี้ตั้งครั้งเดียวที่ `Program.cs` แล้วไม่แตะอีก

⚠️ **ไม่มี timeout** — ต่อ TCP เข้ามาแล้วไม่ส่งอะไรเลย จะค้างใน `_connections` ตลอด กิน buffer ~16 MB ต่อ connection

## `LookupName` / `RegisterName` — บรรทัด 119 / 127
map `entityId → ชื่อ` (มี lock) `/sessions` เป็นคนเติม ถ้าไม่มีชื่อจะคืน entityId แทน

## `RegisterPlayerData` / `GetPlayerData` — บรรทัด 135 / 143
เก็บ/อ่าน `PlayerData` (มี lock) — `Gateway` เขียน, `Ready` handler อ่าน

## `SendWelcome(connection, entityId, name, seq)` — บรรทัด 151
packet ใหญ่ที่บอก client ว่า "เข้ามาแล้ว นี่คือข้อมูลภูมิภาค":
- `Region` — `Id="1"`, `TerrainId="1"`, `TemplateId` จาก terrain จริง, `Role = Rural`, `Name` = ชื่อเซิร์ฟ
- `Storage` — dict ว่าง
- `Options` — `market.ui_enabled = true`, `market.search.limit = 200`
- `Archipelago` / `PersonalRegionId` / `Seasons` = null/ว่าง

`Role = Rural` เลือกไว้ให้ตรงกับ "โหมดหลายคน" ถ้าเปลี่ยนเป็น `Personal` client จะเปิด UI คนละชุด

## `Process()` — บรรทัด 203
```csharp
_listener.Process();        // รับ client ใหม่
_world.ProcessPlayers();    // ระบาย deferred ของทุกคน
lock (_connLock)
    วนถอยหลัง: conn.Process();  ถ้าไม่ Connected() แล้วก็เอาออกจาก list
```
วนถอยหลังเพื่อให้ `RemoveAt` ระหว่างวนได้ปลอดภัย

✅ `conn.Process()` ระบายคิวได้ถึง 512 packet/รอบ และ main loop วิ่ง 120 tps แล้ว (GP-01 แก้แล้ว)
