# `ServerCore/ServerPlayer.Core.cs`

**หน้าที่:** สถานะของผู้เล่น 1 คนบนเซิร์ฟเวอร์ + จุดรวมการลงทะเบียน handler ทุกตัว + packet พื้นฐาน (เดิน/พูด/ขอ chunk)

`ServerPlayer` เป็น `partial class` แตกเป็น 7 ไฟล์ ไฟล์นี้คือแกน — สร้าง 1 ตัวต่อ 1 client ที่ส่ง `Ready` แล้ว และตายเมื่อ connection ปิด

---

## ฟิลด์ (บรรทัด 36–54)

| ฟิลด์ | ความหมาย | ข้อควรระวัง |
|---|---|---|
| `EntityId` | id ผู้เล่น — มาจาก **session ที่ผูกกับ token** ไม่ใช่จาก `Auth.EntityId` ดิบ ๆ | ✅ GP-12 |
| `Name` / `Level` / `EntityType` | ชื่อ, เลเวล, เพศ (1000=ชาย 1001=หญิง) | ✅ GP-14 — เลเวลยึดของ server, entity type ต้องอยู่ช่วง 1000-1999 |
| `_conn` | ท่อ TCP ของคนนี้ | |
| `_world` | โลกที่สังกัด (มีใบเดียวทั้งเซิร์ฟ) | |
| `_deferred` | คิวงานหน่วงเวลา `(เวลาที่จะทำ, งาน)` | ทำงานใน `Process()` เท่านั้น = main thread เสมอ |
| `_lastPosition` `_lastYaw` `_hasPosition` | ตำแหน่ง/ทิศล่าสุด อัปเดตจาก `Move` | ✅ GP-02 — เปิดผ่าน property `CurrentPosition` / `CurrentYaw` |
| `_inventory` | กระเป๋า | RAM ล้วน ไม่เซฟ (GP-07) |
| `_skills` / `_skillPoints` / `_knownSkills` | สกิล เริ่มที่ 777 แต้ม | RAM ล้วน |
| `_loadedDisplay` / `_hasLoadedDisplay` | หน้าตาที่โหลดมาได้แล้ว | ถ้าโหลดไม่ได้จะใช้หน้าตา default (หัวโล้นไม่มีเสื้อ) |

---

## เมทอด

### `ServerPlayer(entityId, name, conn, world, data)` — บรรทัด 56
Constructor ถ้ามี `data` (จาก `/sessions`) ใช้ `ApplyPlayerData()` ถ้าไม่มีถอยไปอ่านไฟล์เซฟด้วย `LoadPlayerSave()`

### `ApplyPlayerData(data)` — บรรทัด 72
เอาข้อมูลที่ client ส่งมาตอน `/sessions` มาใส่ตัวผู้เล่น ทำตามลำดับ:
1. **แจกกองไฟให้ฟรี 1 อัน** (`MakeCapsuleItem("capsulated_bonfire")`) — ของแถมสำหรับเทส ไม่ใช่กติกาเกมจริง
2. ทับ `Level` / `EntityType` / `Name` ถ้าค่าที่ส่งมาใช้ได้ ✅ GP-14
   - `ClampLevel()` ตัดที่เพดาน `MaxPlayerLevel = 60` (พร้อม log ว่า client อ้างมาเท่าไหร่)
   - `IsPlayerEntityType()` รับเฉพาะ 1000-1999 (2000+ = สัตว์, 10000+ = ของธรรมชาติ) ไม่ผ่านก็ใช้ค่าเดิม
   - จำไว้ว่าค่าไหนมาจาก client (`_levelFromClient` / `_entityTypeFromClient`) เพื่อไม่ให้ fallback มาทับทีหลัง
   - ค่าสุดท้ายอาจถูก `LoadPersistedState()` ทับอีกที ถ้าคนนี้มีไฟล์เซฟอยู่แล้ว (ดู [Persistence.md](Persistence.md))
3. แกะ `DisplayJson` เป็น `PlayerDisplay` แล้ว **บังคับ `EntityId` เป็นของเรา** (กัน client ส่ง id คนอื่นมาใน display)
4. ถ้าแกะ display ไม่สำเร็จ → ถอยไป `LoadPlayerSave()`
5. แกะ `SkillsJson` / `KnownSkillsJson` / `SkillPoints`

ทุก `JToken.Parse` ห่อ try/catch แยกกัน — พังตัวใดตัวหนึ่งไม่ล้มทั้งหมด แค่ log แล้วไปต่อ

### `LoadPlayerSave()` — บรรทัด 144
อ่าน `GameServer.PlayerSavePath` (ไฟล์ `.player` ของเกม) เอา `appear_player.Display` กับ `Level` มาใช้
✅ GP-14: **ไม่ทับ** `Level`/`EntityType` ที่มาจาก session รอบนี้ (เจอตอนเทส: ผู้เล่นอ้าง Lv.5 แต่โผล่มาเป็น Lv.60 ของเจ้าของเครื่อง)
⚠️ `PlayerSavePath` เป็น **static ตัวเดียวใช้ร่วมกันทุกคน** → ถ้า `/sessions` ไม่ส่ง display มา ผู้เล่นทุกคนจะหน้าตาเหมือนกันหมด (GP-07)

### `Process()` — บรรทัด 173
เรียกทุก tick จาก `ServerWorld.ProcessPlayers()` ทำอย่างเดียวคือ **ระบายคิว `_deferred`**:
```
วนถอยหลัง → ถ้าถึงเวลาแล้ว: เอาออกจากคิว "ก่อน" แล้วค่อยเรียก
```
เอาออกก่อนเรียกสำคัญมาก — งานที่ throw จะไม่ค้างวนซ้ำ และแต่ละงานห่อ try/catch เดี่ยว งานหนึ่งพังไม่ลากงานอื่นตาย

### `Send<T>(msg)` / `Send<T>(msg, replyOf)` — บรรทัด 194 / 206
ห่อ `_conn.Send()` ด้วย try/catch — ส่งไม่ได้ (socket ตายแล้ว) ก็แค่ log ไม่โยน exception ออกไปให้ handler ล้ม
ตัวที่มี `replyOf` ใช้ตอบคำขอที่ client รออยู่ (ต้องใส่ `header.Seq` ของคำขอนั้น ไม่งั้น client รอค้าง)

### `RegisterHandlers()` — บรรทัด 218 ★ ไฟล์นี้ที่ต้องแก้บ่อยสุด
ผูก packet → เมทอด ทั้งหมด **32 ชนิด** เรียกครั้งเดียวตอน `Ready`
handler ที่เป็น logic ยาว ๆ แยกไปอยู่ partial ไฟล์อื่น ที่เหลือเขียน inline แบบตอบสั้น ๆ:

| packet | ทำอะไร | อยู่ที่ |
|---|---|---|
| `Move` `Say` `SetChunk` | เดิน / พูด / ขอแผนที่ | ไฟล์นี้ |
| `Touch` `Collect` `GetCollectible` `DisappearEntityOnTile` | เก็บของจากธรรมชาติ | [Gathering](ServerPlayer.Gathering.md) |
| `Craft` | คราฟต์ | [Crafting](ServerPlayer.Crafting.md) |
| `OccupyArtifactSite` `BuildArtifact` `PlaceCapsulatedArtifact` `DestructArtifact` `GetArtifact` `EstimateBuild` `PutMaterialsIntoArtifact` | ก่อสร้าง | [Building](ServerPlayer.Building.md) |
| `LearnSkill` `UntrainSkill` `GetSkills` `GetStatistics` | สกิล/ค่าสถานะ | [Skills](ServerPlayer.Skills.md) |
| `Cheat` | คำสั่งโกง | [Cheat](ServerPlayer.Cheat.md) |
| `GetInventory` | ตอบด้วย `SendInventory()` | [Sync](ServerPlayer.Sync.md) |
| `GetRecipes` `GetArtifactBlueprints` | ตอบ **สูตรทั้งหมดในเกม** จาก `RecipeData` | inline |
| `GetQuests` | ตอบ `Todos = null` (ยังไม่มีเควสจริง) | inline |
| `GetAvailableEmotions` | ตอบรายการท่าทาง/อิโมติคอนจาก `NaturalData` | inline |
| `PlayEmoticon` | `_world.Broadcast(msg)` ให้ทุกคนเห็น | inline |
| `SayInExclusiveChannel` `SayInConversation` | แชท — `StampSpeaker()` แล้ว `Broadcast` | inline |

> ✅ **GP-05 แก้แล้ว** — handler แชททั้งสองตัวเรียก `StampSpeaker()` ก่อน broadcast
> การ broadcast กลับหาคนส่งด้วยนั้น **ถูกแล้ว** — client ไม่ได้เพิ่มข้อความตัวเองลง log ตอนส่ง ต้องรอ echo กลับมา

### `StampSpeaker(message)` ✅ GP-05
เติม `Speaker = new RadioId { Name, Freq = 0 }` และบังคับ `EntityId` เป็นของจริงก่อน broadcast
- client เช็ค `if (msg.Message.Speaker.HasValue)` ก่อนตั้งชื่อในกล่องแชท ไม่เติม = แชทไม่มีชื่อคนพูด
- บังคับ `EntityId` กัน client ปลอมเป็นคนอื่นตอนพิมพ์แชท

### `HandleMove(msg, header)` ✅ GP-02
```csharp
RememberPosition(msg);      // จำจุดปลายทางล่าสุดไว้
_world.Broadcast(msg);
```
เดิมมีแต่บรรทัด `Broadcast` — server ไม่รู้ว่าใครอยู่ไหน ทำให้ `MakeAppearPlayer()` ต้องใช้จุดเกิดเสมอ
คนเข้าใหม่จึงเห็นคนที่เล่นอยู่ยืนที่จุดเกิดจนกว่าเขาจะขยับ

### `RememberPosition(msg)` ✅ GP-02
อ่าน `Movements[^1].Path[^1]` (จุดปลายทางของ movement ล่าสุด) เก็บ `Position` + `Yaw`
เช็ค null/array ว่างทุกชั้นก่อนอ่าน — client ส่ง Move เปล่ามาก็ไม่พัง

### `HandleSay(msg, header)` — บรรทัด 322
`Broadcast` ต่ออย่างเดียว (packet `Say` เป็นคนละตัวกับ `SayInExclusiveChannel`)

### `HandleSetChunk(msg, header)` — บรรทัด 327
client บอกว่าตอนนี้ยืนอยู่ chunk ไหน → server ส่ง **garden ของ 3×3 chunk รอบตัว**
```csharp
int cx = Math.Clamp(msg.Chunk.x, 0, _world.Terrain.NumChunksX - 1);   // กันค่าเกินขอบแมพ
for (i = cx-1 .. cx+1)
  for (j = cy-1 .. cy+1)
      Send(new Chunk { _Chunk = (i,j), Garden = Terrain.GetChunkGarden(i,j) ?? ว่าง });
```
`Clamp` + เช็คขอบซ้ำอีกชั้นในลูป = client ส่งพิกัดมั่วมาก็ไม่ทำ server ล้ม
⚠️ **ไม่มีการจำว่าเคยส่ง chunk ไหนไปแล้ว** → เดินไปมาข้ามขอบ chunk จะส่งซ้ำเรื่อย ๆ (เปลืองแบนด์วิดท์ แต่ไม่ผิด)

---

## ถ้าจะเพิ่ม packet ใหม่ ทำยังไง

1. เช็คก่อนว่ามี struct ใน `server/GameCode/Messages/` แล้วหรือยัง (มีครบ 985 ตัวจาก client)
2. เพิ่มบรรทัดใน `RegisterHandlers()`:
   ```csharp
   _conn.Recv<ชื่อPacket>(Handleชื่อPacket);
   ```
3. เขียน handler ไว้ใน partial ไฟล์ที่ตรงโดเมน (สร้างไฟล์ใหม่ก็ได้ csproj auto-glob อยู่แล้ว)
4. ถ้า client รอคำตอบ → ต้อง `Send(reply, header.Seq)` ไม่งั้น UI ค้าง
5. ถ้าเป็นการกระทำที่คนอื่นต้องเห็น → `_world.Broadcast(...)`
