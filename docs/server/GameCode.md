# `GameCode/` — โค้ดที่ยกมาจากตัวเกม

โฟลเดอร์นี้ **copy มาจาก `Assembly-CSharp.dll` ที่ decompile แล้ว** เพื่อให้ server พูดโปรโตคอลเดียวกับ client เป๊ะ ๆ

| ส่วน | ไฟล์ | หมายเหตุ |
|---|---|---|
| `Messages/` | 985 | struct ของ packet ทุกชนิด แต่ละตัวมี `const uint TypeCode` + `Pack`/`Unpack` |
| `Durango.Network/` | 4 | `Packet` `PacketHeader` `EntityType` `MotionOption` |
| `Durango.Offline/` | 3 | `Connection` `Listener` `WebServer` |
| `Shared.*/` | ~200 | enum ที่ใช้ร่วมกัน (Ability, Battle, Item, Skill, Region...) |
| `Shims/` | 7 | ตัวแทน API ของ Unity ที่ .NET ธรรมดาไม่มี |

> **ห้ามแก้ `Messages/` เอง** — ถ้าโครงสร้าง struct ไม่ตรงกับ client จะ deserialize พัง

## `Shims/` — ตัวแทน Unity
| ไฟล์ | แทนอะไร |
|---|---|
| `DebugShim.cs` | `Debug.Log/LogWarning/LogError/LogException` → `Console.WriteLine` พร้อม prefix `[LOG]` `[EXC]` |
| `UnityEngineShims.cs` | `Vector3` `Mathf` ฯลฯ |
| `TimesShim.cs` | `Times.UnixTimeNow()` — ฐานเวลาที่ต้องตรงกับ client |
| `KUtility*.cs` | helper เล็ก ๆ |
| `LocalizeSystemStub.cs` | ระบบแปลภาษา (คืนข้อความดิบ) |
| `JetBrainsAnnotations.cs` | `[NotNull]` `[CanBeNull]` ให้คอมไพล์ผ่าน |

---

## ⚠️ บั๊กที่ติดมาจากตัวเกม (ยังไม่ได้แก้)

### ✅ `Durango.Offline/Connection.cs` — GP-01 แก้แล้ว
เดิมดึง packet ออกมาแค่ 1 ตัวต่อการเรียก 1 ครั้ง ทั้งที่ฝั่ง client ระบายทั้งคิวด้วย `while`
โค้ดสองตัวนี้เป็นแฝดกัน ตัวนี้หาย `while` ไป จึงเป็นบั๊กไม่ใช่การออกแบบ

ตอนนี้:
```csharp
private const int MaxPacketsPerTick = 512;

while (processed < MaxPacketsPerTick)
{
    lock (_packetQueue) { if (empty) break; packet = Dequeue(); }   // dequeue ในล็อก
    processed++;
    try { handler(packet); } catch { log }                          // เรียก handler นอกล็อก
}
if (ชนเพดาน) LogWarning("ค้างอีก N ตัว");                            // ไม่ตัดทิ้งเงียบ ๆ
```
เปลี่ยนจากถือ lock ตลอดการ process มาเป็น dequeue ในล็อก/เรียก handler นอกล็อก
เพื่อไม่ให้ handler ที่ทำงานนานบล็อก thread pool ที่กำลัง enqueue เข้ามา

### `Durango.Offline/Connection.cs` — buffer 16 MB ต่อ connection
จอง `byte[2097152]` ไว้ **8 ก้อน** ตอนสร้าง (send×2, receive, packing, compressing, decompressing, received, remaining)
= ~16 MB ต่อผู้เล่น 1 คน (ฝั่ง client ใช้ 256 KB ต่างกัน 8 เท่า)
4 คน ≈ 65 MB แค่ buffer

### ✅ `Durango.Offline/Listener.cs` — GP-15 แก้แล้ว
เดิม: bind ล้มเหลว → กลืน exception → `_acceptArgs` ยังเป็น null → `Process()` เรียก `Accept()` ทุก tick
→ `ArgumentNullException` ท่วมคอนโซล และเซิร์ฟดูเหมือนรันอยู่แต่ไม่รับใคร
และ `Close()` ปิด socket ทั้งที่ `AcceptAsync` ค้าง → unhandled `SocketException` บน thread pool
(อาการนี้เคยทำให้ตัวเกมดับมาแล้ว — เห็นใน `game/game2.log` บรรทัดสุดท้าย)

ตอนนี้:
- `Start()` คืน `bool` + มี flag `_started` — bind ไม่ผ่านแล้ว `Process()`/`Accept()` จะ return ทันที ไม่แตะ socket อีก
- `_closing` flag — callback ที่ยิงมาหลัง `Close()` จะเก็บกวาด socket แล้วจบ ไม่ไปแตะของที่ทิ้งแล้ว
- `Close()` ใช้ `try/catch` แยกกันสำหรับ `Shutdown` และ `Close` + `Dispose()` ตัว `SocketAsyncEventArgs`
- `Accept()` ห่อ try/catch — เจอ `ObjectDisposedException` ก็หยุดรับ ไม่วนพ่น error
- `Accept_Completed` แยกแยะ `OperationAborted` (ปกติตอนปิด) ออกจาก error จริง

### `Durango.Offline/WebServer.cs:220` — แตะ state จาก thread pool
`KnockListenerCallback` อ่าน `ServerKnock.HostName` บน thread pool
ในตัวเกมโค้ดนี้อ่าน `PlayerBehavior.LocalPlayer.PlayerName` ซึ่ง **ผิดกฎ Unity**
ในเวอร์ชัน server เปลี่ยนเป็น `volatile string` แล้ว = **แก้ถูกต้องแล้ว** ✅
