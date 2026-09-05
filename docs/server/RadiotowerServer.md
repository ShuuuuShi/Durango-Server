# `ServerCore/RadiotowerServer.cs`

**หน้าที่:** เซิร์ฟเวอร์แชทแยก — พอร์ต **เกม + 1** (ค่าเริ่มต้น 8191 ⇒ 8192, ชุด 829x ⇒ 8292)

> ✅ **2 ก.ย. 2026: เปิดใช้ได้แล้ว** — `--radiotower` (หรือ `--radiotower-port <พอร์ต>`)
> เดิมปิดเป็นค่าเริ่มต้นเพราะไม่มี auth (M-5) ตอนนี้ตรวจ session token แล้ว ปลอมชื่อไม่ได้

## เปิดยังไง

```bash
dotnet run -- --radiotower                  # พอร์ต = พอร์ตเกม + 1
dotnet run -- --radiotower-port 8292        # ระบุเอง (เปิด --radiotower ให้ในตัว)
```

พอร์ตที่เปิดจริงถูกส่งให้ client ผ่าน `/entry` → `radiotower_addresses`
(`Gateway.cs` บรรทัด ~326; ถ้าไม่เปิด จะส่ง array ว่างและ client จะไม่พยายามต่อ)

## ⚠️ client จะต่อพอร์ตนี้ก็ต่อเมื่อ `cluster_mode = "Online"`

`SocialSystem.ConnectionHelper.Process()` ในตัวเกม:

```csharp
if (GameManager.ClusterMode != 0)   // 0 = Online
{
    State = ConnectState.Ready;     // ตั้งว่า "พร้อม" โดยไม่ต่อจริง
}
```

เซิร์ฟตอนนี้ส่ง `cluster_mode = "Offline"` เป็นค่าเริ่มต้น (`Program.cs`) ⇒ **ตัวเกมยังไม่ต่อพอร์ตนี้**
แชทช่องรวม (Region/Clan/Party) วิ่งบน connection เกมที่ Auth แล้วอยู่ดี (handler อยู่ใน `ServerPlayer.Core.cs`)
สิ่งที่ยังไม่ได้ใช้จริงคือ **แชทส่วนตัว** (`SayInConversation`) ที่บังคับใช้ Radiotower เสมอ

จะให้ตัวเกมต่อจริงต้องเลือกอย่างใดอย่างหนึ่ง:
1. `--cluster-mode Online` — client มีจุดเช็ค `ClusterMode == Online` หลายสิบจุดที่ยังไม่ได้เทสครบ
2. แพตช์ client (`client/SocialSystem.cs`) ให้ต่อ radiotower โดยไม่ดู ClusterMode — ต้อง build
   `Assembly-CSharp.dll` ใหม่ + แพ็ก `DurangoTH-Clean` ใหม่

ฝั่งเซิร์ฟพร้อมแล้วทั้งสองทาง — เทสด้วย `dotnet run -- --radiotower-check <host> <พอร์ตแชท> <พอร์ต gateway>`

## `Start(port)` → `bool` ✅ GP-15
เหมือน `GameServer.Start()` — คืน false ถ้า bind ไม่สำเร็จ
`Program.cs` แค่เตือน `[warn]` แล้วเล่นต่อได้ (แชทส่วนตัวใช้ไม่ได้เท่านั้น) ไม่จบโปรแกรม
ถ้า bind ไม่ได้จะปิด `enableRadiotower` ด้วย เพื่อไม่ให้ `/entry` โฆษณาพอร์ตที่ไม่มีอยู่จริง

## `ClientAccepted(socket)`

ทุก connection มี state ของตัวเอง (`Client`: `EntityId` · `Name` · `Authed` · `LastChatAt` · `ConnectedAt`)

| handler | ทำอะไร |
|---|---|
| `Tune` | **ตรวจ session token** ผ่าน `GameServer.TryAuthorizeChat()` — ผ่านแล้วตอบ `Conversations`; ไม่ผ่าน ส่ง `Abort` พร้อมเหตุผลแล้วปิด connection |
| `SayInExclusiveChannel` | `AcceptChat` → `StampSpeaker` → `Broadcast` |
| `SayInConversation` | เหมือนกัน |

### M-5 (แก้แล้ว) — auth
`Tune` ที่ client ส่งมามี `EntityId` + `SessionToken` อยู่แล้ว (`SocialSystem.RequestAuth`)
`TryAuthorizeChat` ใช้ตาราง session เดียวกับที่ `Auth` ของพอร์ตเกมใช้ ⇒ token ต้องเป็นตัวที่ `/sessions`
ออกให้ ยังไม่หมดอายุ (12 ชม.) และ entity id ที่อ้างต้องเป็นของ session นั้นเท่านั้น
โหมด `--insecure-auth` ยังกลับไปเชื่อ entity id ที่ส่งมาเหมือนเดิม (สำหรับ debug ในเครื่อง)

### GP-05 (แก้แล้ว) — ชื่อคนพูด
`StampSpeaker()` เขียนทับ `Message.EntityId` ด้วย id ของ session แล้วเติม
`Message.Speaker = new RadioId { Name = <ชื่อจากไฟล์เซฟ> }` ⇒ ปลอมเป็นคนอื่นไม่ได้แม้จะยัด id มาในข้อความ

### `AcceptChat()` — กรองก่อน broadcast
- ยังไม่ผ่าน `Tune` ⇒ ทิ้ง (ไม่ตอบกลับ — client ไม่ได้รอคำตอบของแชทอยู่แล้ว)
- `Features.Chat = false` ⇒ ทิ้ง
- cooldown 0.5 วิ/คน · ข้อความว่าง (whitespace) ⇒ ทิ้ง · ยาวเกิน 200 ตัว ⇒ ตัด

> 🐛 **บั๊กที่เจอตอนเทส (2 ก.ย. 2026):** ทั้งที่นี่และ `ServerPlayer.AcceptChat` เดิมเขียน
> `message.Body as string` แต่โปรโตคอลจริง `Message_.Body` **ไม่เคยเป็น string** — ตัวเกมส่ง
> `RadioTalk { Text = ... }` มาเสมอ ⇒ `body` เป็น null ทุกครั้ง **เพดาน 200 ตัวอักษรกับการกรอง
> ข้อความว่างจึงไม่เคยทำงานเลย** ทั้งพอร์ตแชทและพอร์ตเกม
> แก้ด้วย `ChatBody.ReadText()` / `ChatBody.WriteText()` (ดู `ServerCore/ChatBody.cs`)

## `Broadcast<T>(msg)`
วนถอยหลังส่งให้ทุก connection **ที่ Tune ผ่านแล้ว** ห่อ try/catch ต่อคน (คนหนึ่งส่งไม่ได้ก็ไม่หยุดคนอื่น)
และ log ข้อผิดพลาดไว้ ไม่กลืนเงียบเหมือนเดิม

## `Process()`
`_listener.Process()` + วน `conn.Process()` + เก็บกวาด connection ที่ตายแล้ว
เพิ่ม: connection ที่ต่อเข้ามาแล้ว **ไม่ Tune ภายใน 30 วินาที** จะถูกปิดทิ้ง (กัน socket ค้างกินที่)
