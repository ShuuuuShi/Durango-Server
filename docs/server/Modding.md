# ระบบ Mod (เซิร์ฟ) — เขียนเมื่อ 24 ส.ค. 2026

เจ้าของสั่ง "อยากให้ตัวเกมรองรับ mod เหมือน Minecraft เผื่ออัปเดตแก้ไขในอนาคต" — ทำแล้วฝั่ง**เซิร์ฟ**
เต็มรูปแบบ (โหลด .dll ภายนอกได้จริง ไม่ต้องแก้/build `DurangoServer.dll` ทุกครั้งที่อยากเพิ่มฟีเจอร์)
ฝั่ง**เกม** (client) ยังไม่ได้ทำ — ดูหัวข้อ 6 ท้ายไฟล์นี้ว่าทำได้แค่ไหนถ้าจะทำต่อ

## 1. ภาพรวมสถาปัตยกรรม

```
mod-sdk/DurangoModSdk.csproj   ← อินเทอร์เฟซเล็ก ๆ (IGamePlugin/IModApi/IModPlayer) mod อ้างอิงแค่นี้
server/ServerCore/Modding/     ← ตัวเซิร์ฟจริง: PluginManager (โหลด+dispatch), ServerModPlayer (ตัวห่อ)
server/mods/*.dll               ← ใส่ mod ที่ build แล้วตรงนี้ (สแกนตอนเซิร์ฟบูตเท่านั้น ไม่ hot-reload)
tools/ExampleMod/               ← mod ตัวอย่าง สาธิตทุก hook ในไฟล์เดียว (ทดสอบผ่านแล้ว)
```

`DurangoServer.dll` เอง**ไม่รู้จัก** mod แต่ละตัวเลย — รู้จักแค่อินเทอร์เฟซ `IGamePlugin`/`IModApi` จาก
`DurangoModSdk.dll` เท่านั้น (คนละแอสเซมบลีกัน) นี่คือสิ่งที่ทำให้ "อัปเดตเซิร์ฟแล้ว mod เก่ายังใช้ได้"
เป็นไปได้ — ตราบใดที่ไม่ได้แก้ไฟล์ใน `mod-sdk/` (แก้ได้แต่ห้ามลบ/เปลี่ยน signature เมธอดเดิม เพิ่มเมธอด
ใหม่ในอินเทอร์เฟซได้อย่างเดียวถ้าอยาก backward-compatible จริง ๆ)

## 2. วิธีเขียน mod ใหม่

1. สร้างโปรเจกต์ class library (net9.0) อ้างอิง `DurangoModSdk.dll` (ตัวที่ build ไว้แล้ว ไม่ต้องพ่วง
   `DurangoServer.dll`) — ดู `tools/ExampleMod/ExampleMod.csproj` เป็นตัวอย่าง
2. เขียนคลาสเดียวที่ implement `IGamePlugin`:
   ```csharp
   public sealed class MyPlugin : IGamePlugin
   {
       public string Name => "MyPlugin";
       public string Version => "1.0.0";
       public void OnLoad(IModApi api)
       {
           api.RegisterCommand("mycmd", (player, args) => $"สวัสดี {player.Name}");
           api.OnPlayerJoined(p => api.BroadcastMessage($"{p.Name} เข้าเกมแล้ว"));
       }
   }
   ```
3. `dotnet build -c Release` แล้วเอา `.dll` (ตัวของ mod เองเท่านั้น — **ห้าม** เอา `DurangoModSdk.dll`
   ไปด้วยถ้าเซิร์ฟที่จะรันมีตัวมันเองอยู่แล้ว การมี assembly ชื่อเดียวกันสองชุดโหลดพร้อมกันจะทำให้
   `typeof(IGamePlugin).IsAssignableFrom(...)` เช็คไม่ผ่านเงียบ ๆ — ดูหัวข้อ 5 กับดัก) ไปวางใน `server/mods/`
4. เปิดเซิร์ฟด้วย `--enable-cheat` (คำสั่งของ mod ทั้งหมดวิ่งผ่านระบบ cheat command เดิม ต้องเปิดเสมอ)
   แล้วดู log ตอนบูต: `[mods] โหลด 'MyPlugin' v1.0.0 จาก MyPlugin.dll สำเร็จ`

**ทดสอบใน console** (ไม่ต้องเปิดเกมจริง): `test-client\dotnet run --no-build -- --console 127.0.0.1 8191
mybot --cmd "cheat mycmd"` แล้วดู `[info] ...` ที่ตอบกลับ

## 3. IModApi มีอะไรให้บ้าง (V1)

| เมธอด | ทำอะไร |
|---|---|
| `Log(string)` | เขียน console log ของเซิร์ฟ ขึ้นต้น `[mod:ชื่อ mod]` ให้เอง |
| `RegisterCommand(verb, handler)` | เพิ่มคำสั่ง `cheat <verb> [args]` ใหม่ — ชนชื่อกับ mod อื่นจะถูกปฏิเสธ (ดู log), ชนกับคำสั่งในตัวเซิร์ฟจะไม่มีวันถูกเรียกเลยเพราะคำสั่งในตัวเช็คก่อนเสมอ |
| `OnPlayerJoined(handler)` | ตัวละครเข้าโลกสำเร็จ |
| `OnPlayerLeft(handler)` | ตัดการเชื่อมต่อ |
| `OnTick(handler)` | ทุก tick ของ main loop (~120/วิ) — **ห้ามทำงานหนัก** ในนี้ |
| `GetOnlinePlayers()` | รายชื่อผู้เล่นออนไลน์ตอนนี้ทั้งหมด |
| `FindPlayer(nameOrId)` | หาผู้เล่นออนไลน์คนเดียว |
| `BroadcastMessage(text)` | ส่ง popup ข้อความให้ทุกคนที่ออนไลน์ |

`IModPlayer`: `EntityId` / `Name` / `Level` / `IsDead` / `TileX,TileY` (อ่านอย่างเดียว) +
`SendMessage(text)` / `Teleport(x,y)` (สั่งได้)

**ยังไม่มีใน V1** (เพิ่มทีหลังได้ ไม่กระทบ mod เดิม): hook ก่อน/หลังคราฟต์-ต่อสู้-เก็บของ, เข้าถึง
กระเป๋า/ไอเทมของผู้เล่น, ลงทะเบียน blueprint/recipe ใหม่, เซฟ state ของ mod เองแบบมีปลั๊กอินช่วย (ตอนนี้
mod ต้องเซฟไฟล์เองถ้าอยากให้ state รอดจากการรีสตาร์ทเซิร์ฟ — ดูตัวอย่าง `_playtimeSeconds` ใน ExampleMod
ที่ไม่เซฟ หายทุกครั้งที่รีสตาร์ท)

## 4. ทดสอบแล้ว (24 ส.ค. 2026) — `tools/ExampleMod`

รัน `--console` bot ยิงคำสั่งจริงผ่านเซิร์ฟที่มี ExampleMod โหลดอยู่ ได้ผลครบ:
```
[info] [ExampleMod] modtestbot เข้าเกมแล้ว — ยินดีต้อนรับ!      ← OnPlayerJoined + BroadcastMessage
[info] สวัสดี modtestbot! ทักมาว่า: world                          ← RegisterCommand("hello", ...) รับ args ถูก
[info] modtestbot ออนไลน์สะสม (นับจากเซิร์ฟรอบนี้) 7 วินาที        ← OnTick สะสม state ถูกต้อง
[info] unknown cheat: zzznonexistent                               ← คำสั่งที่ไม่มี mod ไหนรับ ยัง fallback ถูก
```

## 5. กับดักที่ต้องรู้

- **ไม่ hot-reload** — เพิ่ม/แก้ mod ต้อง restart เซิร์ฟเสมอ (`PluginManager.LoadAll` เรียกครั้งเดียวตอนบูต)
- **ห้ามเอา `DurangoModSdk.dll` ไปแปะใน `mods/` ด้วย** ถ้าเซิร์ฟมีอยู่แล้ว (ProjectReference ของ
  `DurangoServer.csproj` เอง) — โหลดซ้ำสองชุดจะทำให้ type identity ไม่ตรงกัน mod หาย "เงียบ ๆ" (ไม่ error
  ชัดเจน แค่ `typeof(IGamePlugin).IsAssignableFrom(type)` คืน false) เทสแล้วเจอปัญหานี้ตอน dev รอบแรก
- **`RegisterCommand` ชนกับคำสั่งในตัวเซิร์ฟตรวจไม่ได้** — `PluginManager` ไม่ได้ hardcode รายชื่อคำสั่ง
  ในตัวทั้งหมดไว้เช็ค (จะหลุดตกยุคเวลาเพิ่มคำสั่งใหม่) ถ้า mod ตั้งชื่อคำสั่งชนกับที่มีอยู่แล้ว (give/heal/tp/...)
  คำสั่งของ mod จะไม่มีวันถูกเรียกเลยเงียบ ๆ เพราะ switch คำสั่งในตัวเช็คก่อนเสมอ — ตั้งชื่อ verb ให้ไม่ซ้ำ
- **exception ใน handler ของ mod ไม่ทำให้เซิร์ฟล้ม** — `PluginManager` จับทุก exception ของ mod ไว้ (ทั้ง
  ตอน `OnLoad` และตอน fire event/คำสั่ง) แต่ mod ตัวที่ throw ตอน `OnLoad` จะไม่ได้ทำงานเลยทั้งตัว
- คำสั่งของ mod ผ่านด่าน `--enable-cheat` เดียวกับคำสั่งทดสอบในตัว — ปิดไว้บนเซิร์ฟสาธารณะเหมือนเดิม

## 6. ฝั่งเกม (client) — ยังไม่ได้ทำ ทำได้แค่ไหนถ้าจะทำต่อ

เกมเป็น Unity 2017.4.34f1 **backend Mono** (ยืนยันแล้ว — มีโฟลเดอร์ `Mono/`+`Managed/` ข้าง exe ไม่ใช่
IL2CPP) เปิดทางให้ทำได้จริง 2 ระดับ ถ้าเจ้าของอยากทำต่อ:

- **มีของใหม่โดยไม่ build ใหม่** (ง่ายกว่า) — ใช้ Unity AssetBundle (รองรับในตัว Unity อยู่แล้ว) โหลด
  โมเดล/พื้นผิว/เสียงจากไฟล์นอกได้ ผสานกับสูตร/ไอเทมที่ประกาศฝั่งเซิร์ฟ (คุมได้เต็มที่อยู่แล้ว)
- **มีพฤติกรรม/ระบบใหม่ในเกม** (ยากกว่ามาก) — ต้องมี mod-loader แบบ BepInEx/MelonLoader (inject เข้า
  runtime แล้วโหลด DLL ภายนอกที่ patch โค้ดเกมได้ระหว่างรัน) เกมนี้ backend Mono เข้ากับเครื่องมือพวกนี้ได้
  ในหลักการ แต่ไม่เคยลองจริง — เป็นงานคนละขนาดกับที่ทำวันนี้ ต้องแยกทำเป็นงานใหม่ต่างหาก
