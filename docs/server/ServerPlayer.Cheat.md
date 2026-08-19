# `ServerCore/ServerPlayer.Cheat.cs`

**หน้าที่:** คำสั่งโกงสำหรับทดสอบ — มีเมทอดเดียว

## `HandleCheat(msg, header)` — บรรทัด 37

รับข้อความจาก client แล้ว `.Trim().ToLower()` ก่อนเทียบ

| คำสั่ง | ทำอะไร |
|---|---|
| `tp spawn` | `SendTeleport(_world.GetEntryPosition())` — วาร์ปกลับจุดเกิด |
| `info` | ตอบ `Info` ว่า `"DurangoServer v0.1 - players: N"` (N = จำนวนคนออนไลน์จริง) |
| `stats` | ยิง `SendStatistics()` ซ้ำ |
| `add bonfire` / `add_bonfire` | เพิ่มกองไฟลงกระเป๋า + `SendInventory()` |
| อื่น ๆ | ตอบ `"unknown cheat: ..."` |

`Console.WriteLine($"[cheat] {EntityId}: {cmd}")` ทุกครั้ง — เห็นในหน้าต่าง server ว่าใครสั่งอะไร

> คำสั่ง `info` นี่แหละที่ทำให้ข้อความ `DurangoServer v0.1 - players: 1` โผล่ในเทอร์มินอลตอนอัดคลิป

## ⚠️ เปิดรับจากทุกคน ไม่มีเงื่อนไข (GP-07 ใน GAMEPLAY-REVIEW)

```csharp
_conn.Recv<Cheat>(HandleCheat);      // ไม่เช็คว่าเป็นโฮสต์ ไม่เช็ค debug build
```

ตอนนี้คำสั่งที่มียังไม่อันตราย (วาร์ปตัวเอง / กองไฟ 1 อัน) แต่พอเพิ่มคำสั่งอย่าง "เสกไอเทม" หรือ "อมตะ"
เข้าไป แขวนไว้แบบนี้ = แขกคนไหนก็ยิงใส่เกาะโฮสต์ได้

ทางแก้ง่ายสุด: ใส่เงื่อนไขที่หัวเมทอด — เช็คว่า `EntityId` ตรงกับโฮสต์ หรือรับเฉพาะ connection ที่มาจาก `127.0.0.1`
