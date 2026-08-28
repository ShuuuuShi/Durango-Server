# ระบบ Mod (เซิร์ฟ) — เขียน 24 ส.ค. 2026 · V1.1 27 ส.ค. 2026

เจ้าของสั่ง "อยากให้ตัวเกมรองรับ mod เหมือน Minecraft เผื่ออัปเดตแก้ไขในอนาคต" — ทำแล้วทั้งฝั่ง**เซิร์ฟ**
(เอกสารนี้) และฝั่ง**เกม (client)** จบแล้ว — client mod ดู `docs/client/Modding.md`

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

## 2.1 Directory package and mod.json

New server mods can be installed as a directory package. Flat server/mods/*.dll mods remain supported for backward compatibility.

    server/mods/my-mod/
      mod.json
      MyMod.dll

Example mod.json:

    {
      "id": "my-mod",
      "name": "My Mod",
      "version": "1.0.0",
      "api_version": "1.0",
      "assembly": "MyMod.dll",
      "dependencies": [],
      "required": false
    }

The loader validates the manifest, loads dependencies before dependants, and reports invalid manifests, missing assemblies, missing dependencies, and dependency cycles in /admin/mods. A package may also include sha256 and content_sha256 for integrity checks.

## 3. IModApi มีอะไรให้บ้าง (V1 + V1.1 + Event Bus foundation)

| เมธอด | ทำอะไร |
|---|---|
| `Log(string)` | เขียน console log ของเซิร์ฟ ขึ้นต้น `[mod:ชื่อ mod]` ให้เอง |
| `RegisterCommand(verb, handler)` | เพิ่มคำสั่ง `cheat <verb> [args]` ใหม่ — ชนชื่อกับ mod อื่นจะถูกปฏิเสธ (ดู log), ชนกับคำสั่งในตัวเซิร์ฟจะไม่มีวันถูกเรียกเลยเพราะคำสั่งในตัวเช็คก่อนเสมอ |
| `OnPlayerJoined(handler)` | ตัวละครเข้าโลกสำเร็จ |
| `OnPlayerLeft(handler)` | ตัดการเชื่อมต่อ |
| `OnPlayerDied(handler)` *(V1.1)* | ผู้เล่นล้มจริง (`Die()` — IsDead=true แล้ว, ยิงครั้งเดียวต่อการตาย 1 รอบ) |
| `OnTick(handler)` | ทุก tick ของ main loop (~120/วิ) — **ห้ามทำงานหนัก** ในนี้ |
| `GetOnlinePlayers()` | รายชื่อผู้เล่นออนไลน์ตอนนี้ทั้งหมด |
| `FindPlayer(nameOrId)` | หาผู้เล่นออนไลน์คนเดียว |
| `BroadcastMessage(text)` | ส่ง popup ข้อความให้ทุกคนที่ออนไลน์ |

`IModPlayer`: `EntityId` / `Name` / `Level` / `IsDead` / `TileX,TileY` (อ่านอย่างเดียว) +
`SendMessage(text)` / `Teleport(x,y)` (สั่งได้) +
*(V1.1)* `CountItem(prototypeId)` / `GetInventorySummary()` (อ่านกระเป๋าติดตัว) /
`GiveItem(prototypeId, count)` (เพิ่มของแบบ "เก็บได้เอง" durability/tag ครบ+sync client)

### Optional Event Bus / Storage foundation (M0/M1)

cast `IModApi` เป็น `IModEventsApi` ได้โดยไม่กระทบ mod V1 เดิม:

```csharp
if (api is IModEventsApi events)
{
    events.Subscribe("craft.completed", e => api.Log(e.EventName), EventPriority.Monitor);
    events.Subscribe("player.died", e => api.BroadcastMessage("มีผู้เล่นล้มแล้ว"));
    events.Storage.SaveJson("state", "{\"schema\":1}");
}
```

event catalog ตอนนี้มี lifecycle/post-commit และ before/cancellable สำหรับ gameplay ที่กำลังทยอยผูก:
`player.joined`, `player.left`, `player.died`, `player.revived`, `server.tick`, `inventory.added`,
`inventory.removed`, `craft.before`, `craft.completed`, `craft.failed`, `gather.before`,
`gather.completed`, `butchery.before`, `butchery.completed`, `farm.before_plant`, `farm.planted`,
`farm.before_harvest`, `farm.harvested`, `building.before_place`, `building.placed`,
`building.before_complete`, `building.completed`, `building.before_destroy`, `building.destroyed`,
`combat.before_attack`, `combat.attack`, `combat.before_damage`, `combat.damage`,
`quest.progressed`, `quest.completed`, `progress.level_up`, `progress.skill_learned`,
`travel.entered`, `travel.leaving`, `chat.message`. Event context มีชื่อ/id/time/player, `Data`,
สถานะ committed และ `Cancel(reason)` — ทุก before hook อยู่หลัง authorization และก่อน consume/mutate;
เมื่อ cancel แล้ว action หยุดโดยไม่หัก stamina/วัสดุ/ของ และ post-event ยิงหลัง commit เท่านั้น

การเรียก `IModEventsApi.Storage` จะเก็บไฟล์ namespaced ใต้ `saves/mods/<mod-id>/` แบบ atomic
และกัน path traversal; `FlushStorage()` ถูกเรียกตอน Ctrl+C ก่อนปิดเซิร์ฟ

### M2–M5 ที่เพิ่มแล้ว

- M3: package ใต้ `mods/<id>/` รองรับ `content/*.json` โดย `kind` เป็น `item`, `recipe`, `loot`, `buildable` หรือ `quest`; `id` ต้องเป็น `<mod-id>:<local-id>`. Loader ตรวจขนาด/path/JSON/schema/duplicate และคำนวณ `content_sha256` ก่อนเปิดใช้งาน
- M4: cast `IModApi` เป็น `IClientPresentationApi` เพื่อ `RegisterSceneHook`, `RegisterHud` และ `ValidateAsset`; exception ของ hook ถูกแยกต่อม็อด
- M5: client ส่ง `ModHello` หลัง `Welcome` ก่อน `Ready`. เปิด policy ด้วย `--require-mods`, `--no-unknown-optional-mods`, `--require-mod-signatures` และ `--mod-public-key`. ดู hash/จำนวน event/command/error/rate-limit ได้ที่ `/admin/mods`
- Rate limits: packet ต่อ connection, arguments ของ mod command, subscription และขนาด manifest; server ยังคง authoritative และปฏิเสธ handshake ก่อนสร้าง player

## 4. ทดสอบแล้ว — `tools/ExampleMod` + `tools/ExampleGameplayMod`

**รอบ real client + server (27 ส.ค. 2026)** — เปิดเซิร์ฟจริงด้วย
`--enable-cheat --admin gm` และติดตั้ง `ExampleMod.dll` กับ `ExampleGameplayMod.dll`
ใน `server/mods/` จากนั้นเปิด `DurangoV2.exe` ต่อเข้าเซิร์ฟเดียวกัน:

```
[mods] โหลด 'ExampleGameplayMod' v0.1.0 ... สำเร็จ (ครบ 3 เฟส)
[mods] โหลด 'ExampleMod' v1.1.0 ... สำเร็จ (ครบ 3 เฟส)
[world] player joined: multi-3 (multi-3), total=1
```

คำสั่งที่ยิงผ่าน console client และผลที่ได้จริง:

```
cheat hello world       → สวัสดี modtestbot! ทักมาว่า: world
cheat modgive stone 2   → ให้ stone x2 แล้ว (ในกระเป๋าตอนนี้รวม 2 ชิ้น)
cheat inv               → ของติดตัว: ... stone x2
cheat die               → [ExampleMod] ☠ gm ล้มลงที่ tile 40,177!
cheat eventstatus       → ExampleGameplayMod events=1, storage=ok
```

ทดสอบข้ามฝั่งเพิ่ม: admin สั่ง `control multi-3 tp 40 177` แล้ว client mod กด F10
อ่านตำแหน่งใหม่ได้เป็น `(-100, -44.142, 500)` — ยืนยันว่า server command มีผลกับ
ตัวละครจริงและ client mod อ่าน state หลัง sync ได้ ไม่ได้แก้ตำแหน่งฝั่ง client เอง

**รอบ V1.1 (27 ส.ค. 2026)** — `--console` bot ยิงคำสั่งจริงผ่านเซิร์ฟ:
```
[info] ให้ blade_stone x2 แล้ว (ในกระเป๋าตอนนี้รวม 2 ชิ้น)      ← GiveItem + CountItem
[info] ของติดตัว: axe_onehand_stone_01 x1, blade_stone x2, ... ← GetInventorySummary
[info] [ExampleMod] ☠ modtestbot ล้มลงที่ tile 40,177!          ← OnPlayerDied (ตายจาก cheat hurt)
```
(ปิดท้าย: เซฟบอททดสอบถูกลบทิ้ง)

**รอบ V1 (24 ส.ค. 2026)**:
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
  **เจอจริง 27 ส.ค.:** ตั้งชื่อ `give` ใน ExampleMod → โดนคำสั่งในตัว `give` กลืนไปเฉย ๆ (log ตอบ
  "ได้ 돌멩ย x5" ของ built-in) ต้องเปลี่ยนเป็น `modgive` — ตั้งชื่อขึ้นต้น `mod` หรือชื่อ mod เองจะปลอดภัยสุด
- **exception ใน handler ของ mod ไม่ทำให้เซิร์ฟล้ม** — `PluginManager` จับทุก exception ของ mod ไว้ (ทั้ง
  ตอน `OnLoad` และตอน fire event/คำสั่ง) แต่ mod ตัวที่ throw ตอน `OnLoad` จะไม่ได้ทำงานเลยทั้งตัว
- คำสั่งของ mod ผ่านด่าน `--enable-cheat` เดียวกับคำสั่งทดสอบในตัว — ปิดไว้บนเซิร์ฟสาธารณะเหมือนเดิม

## 6. ฝั่งเกม (client) — ทำแล้ว ✅

ทำต่อจนจบแล้วตั้งแต่ปลาย ส.ค. 2026 (loader 3 เฟส + SDK เล็ก ๆ ผ่าน ProjectReference ของ Assembly-CSharp)
สถาปัตยกรรม/วิธีเขียน/API/gotcha ทั้งหมดอยู่ที่ **`docs/client/Modding.md`** · ตัวอย่าง:
`tools/ExampleClientMod/` · mod จริง: `tools/MemoryBotMod/`

สิ่งที่ยังไม่ทำฝั่ง client (ถ้าจะทำต่อ): AssetBundle loader สำหรับ "ของใหม่โดยไม่ build" และ hook ระดับ
UI/packet — เหมือนฝั่งเซิร์ฟ, เปิด API เพิ่มเป็น method ใหม่ใน interface ได้เสมอ โดยไม่พัง mod เดิม
