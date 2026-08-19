# `ServerCore/ServerWorld.cs`

**หน้าที่:** โลกหนึ่งใบ — ถือรายชื่อผู้เล่นที่ออนไลน์ + terrain + กระจาย packet ให้ทุกคน
มีอินสแตนซ์เดียวทั้งเซิร์ฟ สร้างใน `Program.cs` แล้วส่งต่อให้ `GameServer` กับ `Gateway`

## ฟิลด์

| ฟิลด์ | หมายเหตุ |
|---|---|
| `Terrain` | `TerrainStore` — แผนที่ ต้นไม้ แม่น้ำ |
| `ServerName` | ชื่อที่โชว์ในเกม (ค่าเริ่มต้น `"Multi Play Server"`) |
| `EntryPoint` | จุดเกิด ดึงจาก terrain |
| `_lock` | กันชนกันระหว่าง main loop กับ socket callback |
| `_players` | ผู้เล่นที่ผ่าน `Ready` แล้ว |

| `_artifacts` + `_artifactLock` | ✅ GP-04 — สิ่งปลูกสร้างทั้งหมดในโลก |
| `_generators` + `_genLock` | ✅ GP-03 — state จุดเก็บของ (ย้ายมาจาก `ServerPlayer`) |

## `GetEntryPosition()` — บรรทัด 50
แปลงพิกัด tile เป็นพิกัดโลก: `EntryPoint * 200f` (1 tile = 200 หน่วยโลก)

## `AddPlayer(player)` — บรรทัด 56 ★
```
lock:
    ส่ง AppearPlayer ของ "ทุกคนที่อยู่ก่อน" ให้คนใหม่     ← คนใหม่เห็นคนเก่า
    เพิ่มคนใหม่เข้า _players
BroadcastExcept(คนใหม่, คนใหม่.MakeAppearPlayer())        ← คนเก่าเห็นคนใหม่
log "[world] player joined: ... total=N"
```
ลำดับถูกต้อง: เพิ่มเข้า list **หลัง** ส่งของคนเก่าให้แล้ว จึงไม่ส่ง AppearPlayer ของตัวเองซ้ำ
และ broadcast อยู่นอก lock เพื่อไม่ให้ถือ lock ตอนทำ I/O

✅ **GP-04**: ก่อนจะส่ง AppearPlayer จะยิง `SnapshotArtifacts()` ทั้งชุดให้คนใหม่ก่อน
คนที่เข้ามากลางเกมจึงเห็นบ้านที่คนอื่นสร้างไว้แล้ว (log บอกจำนวนท้ายบรรทัด `artifacts=N`)

## จัดการสิ่งปลูกสร้าง (GP-04)

| เมทอด | ทำอะไร |
|---|---|
| `AddArtifact(a)` | จำไว้ เรียกทุกครั้งที่สร้าง/วางของ |
| `TryGetArtifact(id, out a)` | ดึงมาตรวจสิทธิ์ก่อนทุบ |
| `RemoveArtifact(id)` | ลบตอนทุบสำเร็จ |
| `SetArtifactBuildingState(id, state)` | อัปเดต Occupied → Built ให้คนเข้าทีหลังเห็นถูก |
| `SnapshotArtifacts()` | สำเนาทั้งหมด ใช้ตอน `AddPlayer` |
| `ArtifactCount` | จำนวนปัจจุบัน |

## จัดการจุดเก็บของ (GP-03)

| เมทอด | ทำอะไร |
|---|---|
| `GetOrCreateGenerators(id, type, factory)` | ยังไม่มีก็สร้าง คืน**สำเนา**เสมอ กันผู้เรียกแก้ของกลาง |
| `PeekGenerators(id)` | ดูเฉย ๆ คืน null ถ้ายังไม่มีใครแตะ |
| `TryReserveGenerator(id, genId, out gen, out ranOut)` | **จอง 1 หน่วยแบบอะตอมมิก** — หักทันทีที่ขอ สองคนกดพร้อมกันบนหน่วยสุดท้ายจะผ่านคนเดียว |
| `RegisterNaturalTile(id, tile)` | ✅ GP-09 — ผูก entity id ของธรรมชาติกับ tile ที่ตรวจแล้วว่ามีของจริง (เรียกจาก `HandleTouch` เท่านั้น) |
| `TryGetNaturalTile(id, out tile)` | ✅ GP-09 — `Collect`/`DisappearEntityOnTile` อ่าน tile จากที่นี่ ไม่อ่านจาก packet ของ client |
| `ForgetNaturalTile(id)` | ✅ GP-09 — จุดนี้ถูกเก็บหมด/ถูกลบไปแล้ว |

`_naturalTiles` ใช้ `_genLock` ตัวเดียวกับ `_generators` เพราะเป็นข้อมูลชุดเดียวกันเชิงตรรกะ
(อยู่ในหน่วยความจำอย่างเดียว ไม่ได้เซฟ — เปิดเซิร์ฟใหม่ก็แค่ต้องแตะใหม่ก่อนเก็บ)

## `RemovePlayer(player)` — บรรทัด 70
เอาออกจาก list (ใน lock) แล้ว `Broadcast(DisappearEntity)` ให้ทุกคน + log
เรียกจาก event `ConnetionClosed` ที่ผูกไว้ใน `GameServer`

## `Broadcast<T>(msg)` — บรรทัด 80
```csharp
lock (_lock) { snapshot = _players.ToArray(); }    // ก๊อปก่อน
foreach (p in snapshot) p.Send(msg);               // แล้วค่อยส่งนอก lock
```
**snapshot pattern** — ปลอดภัยกว่า `foreach` บน list จริง ถ้า `Send` ทำให้คนหลุด (`RemovePlayer`)
ก็ไม่เกิด `InvalidOperationException` กลางลูป
(offline server เดิมในเกมใช้ foreach ตรง ๆ ซึ่งเป็นบั๊ก NET-17 — ตัวนี้แก้ไว้แล้ว)

## `BroadcastExcept<T>(except, msg, excludeSelf = false)` — บรรทัด 93
เหมือน `Broadcast` แต่ข้ามคนที่ระบุ

✅ **GP-13 แก้แล้ว** — เดิมมีพารามิเตอร์ `bool excludeSelf` ที่ไม่เคยถูกอ่านในบอดี้เลย
(ตัดสินจาก `p == except` อย่างเดียว) พฤติกรรมถูกอยู่แล้วแต่ชวนเข้าใจผิด จึงเอาออก

## `Count` — บรรทัด 110
จำนวนคนออนไลน์ (อ่านใน lock) ใช้ตอบ cheat `info`

## `ProcessPlayers()` — บรรทัด 121
```csharp
lock (_lock) { foreach (p in _players) p.Process(); }
```
เรียกทุก tick จาก `GameServer.Process()` → แต่ละคนไประบายคิว `_deferred` ของตัวเอง

⚠️ ตรงนี้ `foreach` บน list จริงขณะถือ lock — ปลอดภัยเพราะ `Player.Process()` แตะแค่ `_deferred`
ของตัวเอง และ `RemovePlayer` ถูกเรียกจากเฟสอื่นของ main loop (ตอน `conn.Process()`) ไม่ใช่ตอนนี้
แต่ถ้าวันหลังมี deferred action ที่ทำให้คนหลุด จะพังตรงนี้ — ควรเปลี่ยนเป็น snapshot เหมือน `Broadcast`
