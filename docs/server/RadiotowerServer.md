# `ServerCore/RadiotowerServer.cs`

**หน้าที่:** เซิร์ฟเวอร์แชทแยกที่พอร์ต **8192**

> 🔒 **beta 1.0: ปิดเป็นค่าเริ่มต้น** — พอร์ตนี้ไม่มี auth เลย ใครต่อเข้ามาก็ประกาศตัวเป็นใครก็ได้ (M-5)
> ปลอดภัยที่จะปิดเพราะ client ใน SingleMode **ไม่เคยต่อพอร์ตนี้อยู่แล้ว** (เหตุผลอยู่ในหัวข้อถัดไป)
> เปิดกลับด้วย `dotnet run -- --radiotower`

## ⚠️ อ่านก่อน: ตอนนี้ยังไม่มีใครต่อเข้ามาเลย

client เลือกท่อส่งแชทแบบนี้:
```csharp
// แชทช่องปกติ (Region/Clan/Party)
var connection = (GameManager.ClusterMode != 0) ? Connections.Frontend : Connections.Radiotower;
connection.Send(msg);
```
`/entry` ตอบ `cluster_mode = "SingleMode"` (= 3, ไม่ใช่ 0) → **แชทวิ่งไปทาง Frontend (พอร์ต 8191)**
ซึ่งมี handler อยู่ใน `ServerPlayer.Core.cs` แล้ว

ส่วน `SayInConversation` (แชทส่วนตัว) บังคับใช้ Radiotower เสมอ แต่ SingleMode ทำให้ client
ตั้ง `State = Ready` **โดยไม่ต่อจริง** → `Send()` เจอ `!Connected()` แล้ว return false เงียบ ๆ

⇒ **คลาสนี้ทั้งคลาสเป็นโค้ดตาย** และแชทส่วนตัวใช้ไม่ได้ (GP-06)
จะปลุกให้ทำงานต้องเปลี่ยน `cluster_mode` เป็น `Online` **พร้อมกับ** แก้ `radiotower_addresses` ให้เป็น IP จริง

## `Start(port)` → `bool` ✅ GP-15
เหมือน `GameServer.Start()` — คืน false ถ้า bind ไม่สำเร็จ
ต่างกันตรงที่ `Program.cs` แค่เตือน `[warn]` แล้วเล่นต่อได้ (แชทส่วนตัวใช้ไม่ได้เท่านั้น) ไม่จบโปรแกรม

## `ClientAccepted(socket)` — บรรทัด 49
ผูก 3 handler:
- `Tune` → ตอบ `Conversations { _Conversations = null }` (handshake ของ radiotower)
- `SayInExclusiveChannel` → log + `Broadcast`
- `SayInConversation` → log + `Broadcast`

⚠️ ไม่เติม `Message.Speaker` เหมือนกัน → ถ้าวันหน้าเปิดใช้จริง แชทจะไม่มีชื่อคนพูด (GP-05)

## `Broadcast<T>(msg)` — บรรทัด 74
วนถอยหลังส่งให้ทุก connection ห่อ `try/catch (Exception) {}` ต่อคน — คนหนึ่งส่งไม่ได้ก็ไม่หยุดคนอื่น
(catch เปล่าแบบนี้กลืน error หมด ควร log ไว้บ้าง)

## `Process()` — บรรทัด 91
`_listener.Process()` + วน `conn.Process()` + เก็บกวาด connection ที่ตายแล้ว — โครงเดียวกับ `GameServer.Process()`
