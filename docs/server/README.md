# Server — สารบัญ doc ระดับเมทอด

`DurangoServer` — .NET 9, เธรดเดียว, ~5,600 บรรทัด (ไม่นับ `GameCode/`)
เอกสารชุดนี้ **เขียนมือ** อธิบายว่าแต่ละไฟล์ทำอะไรและแต่ละเมทอดทำงานยังไงทีละขั้น

> ภาพรวมว่าทุกอย่างต่อกันยังไง อ่าน [../project/ARCHITECTURE.md](../project/ARCHITECTURE.md) ก่อน
> รายการบั๊กที่ยังค้าง + roadmap อยู่ที่ [../../server/GAMEPLAY-REVIEW.md](../../server/GAMEPLAY-REVIEW.md)

## โครงไฟล์

```
server/
├── Program.cs                       จุดเริ่ม + main loop
├── ServerCore/
│   ├── ServerCommon.cs              DictExtensions, ServerKnock
│   ├── ServerWorld.cs               โลก + รายชื่อผู้เล่น + broadcast
│   ├── ServerPlayer.Core.cs         ★ สถานะผู้เล่น + RegisterHandlers()
│   ├── ServerPlayer.Gathering.cs      เก็บของจากธรรมชาติ
│   ├── ServerPlayer.Crafting.cs       คราฟต์
│   ├── ServerPlayer.Building.cs       ก่อสร้าง
│   ├── ServerPlayer.Skills.cs         สกิล + ค่าสถานะ
│   ├── ServerPlayer.Sync.cs           ยิงสถานะไป client
│   ├── ServerPlayer.Cheat.cs          คำสั่งโกง
│   ├── ServerPlayer.Equipment.cs      สวมใส่อุปกรณ์
│   ├── ServerPlayer.Survival.cs       เลือด/สตามินา/ความล้า
│   ├── ServerPlayer.Storage.cs        กล่องเก็บของ
│   ├── ServerAnimal.cs AnimalSpawner.cs   สัตว์ในโลก
│   ├── AnimalData.cs                ตารางสัตว์ 213 ชนิด (สร้างอัตโนมัติ)
│   ├── EquipData.cs                 ตารางโมเดลอาวุธ/เกราะ (สร้างอัตโนมัติ)
│   ├── GameServer.cs                TCP 8191 + handshake
│   ├── Gateway.cs                   HTTP 8190
│   ├── RadiotowerServer.cs          TCP 8192 (ยังไม่ถูกใช้)
│   ├── SaveStore.cs                 เขียน/อ่านไฟล์เซฟ (atomic)
│   ├── SaveModels.cs                รูปแบบข้อมูลในไฟล์เซฟ
│   ├── ServerPlayer.Persistence.cs    เซฟ/โหลด state ผู้เล่น
│   ├── ArtifactFactory.cs           สร้าง AppearArtifact (ใช้ทั้งตอนสร้างและตอนโหลดเซฟ)
│   ├── TerrainStore.cs              แผนที่
│   └── RecipeData/SkillData/NaturalData.cs   ตารางข้อมูล
├── GameCode/                        ยกมาจากตัวเกม — ห้ามแก้ Messages/
└── Shims/                           ตัวแทน Unity API
```

## สารบัญ

| ไฟล์ | ทำอะไร | doc |
|---|---|---|
| `Program.cs` | argument, เปิดเซิร์ฟ, main loop | [Program.md](Program.md) |
| `ServerCore/ServerCommon.cs` | helper + ชื่อเซิร์ฟสำหรับ LAN discovery | [ServerCommon.md](ServerCommon.md) |
| `ServerCore/ServerWorld.cs` | โลก + broadcast | [ServerWorld.md](ServerWorld.md) |
| `ServerCore/ServerPlayer.Core.cs` | ★ แกนผู้เล่น + ผูก handler 32 ตัว | [ServerPlayer.Core.md](ServerPlayer.Core.md) |
| `ServerCore/ServerPlayer.Gathering.cs` | Touch → Collect → ได้ไอเทม | [ServerPlayer.Gathering.md](ServerPlayer.Gathering.md) |
| `ServerCore/ServerPlayer.Crafting.cs` | คราฟต์ | [ServerPlayer.Crafting.md](ServerPlayer.Crafting.md) |
| `ServerCore/ServerPlayer.Building.cs` | จองที่ วางของ สร้าง ทุบ | [ServerPlayer.Building.md](ServerPlayer.Building.md) |
| `ServerCore/ServerPlayer.Skills.cs` | เรียน/ลืมสกิล + ค่าสถานะ | [ServerPlayer.Skills.md](ServerPlayer.Skills.md) |
| `ServerCore/ServerPlayer.Sync.cs` | SendSpawnBurst + packet สถานะ | [ServerPlayer.Sync.md](ServerPlayer.Sync.md) |
| `ServerCore/ServerPlayer.Cheat.cs` | คำสั่งโกง | [ServerPlayer.Cheat.md](ServerPlayer.Cheat.md) |
| `ServerCore/GameServer.cs` | รับ client + handshake | [GameServer.md](GameServer.md) |
| `ServerCore/Gateway.cs` | HTTP route ทั้งหมด | [Gateway.md](Gateway.md) |
| `ServerCore/RadiotowerServer.cs` | แชท (โค้ดตาย) | [RadiotowerServer.md](RadiotowerServer.md) |
| `ServerCore/TerrainStore.cs` | แผนที่ + ต้นไม้ | [TerrainStore.md](TerrainStore.md) |
| `ServerCore/SaveStore.cs` `SaveModels.cs` `ServerPlayer.Persistence.cs` `ArtifactFactory.cs` | ระบบเซฟ | [Persistence.md](Persistence.md) |
| `ServerCore/ServerPlayer.Equipment.cs` `EquipData.cs` | สวมใส่อุปกรณ์ | [Equipment.md](Equipment.md) |
| `ServerCore/ServerPlayer.Survival.cs` | ค่าสถานะเอาชีวิตรอด | [Survival.md](Survival.md) |
| `ServerCore/ServerPlayer.Storage.cs` | กล่องเก็บของ | [Storage.md](Storage.md) |
| `ServerCore/ServerPlayer.Items.cs` | ทิ้งของ / กินของ | [Items.md](Items.md) |
| `ServerCore/ServerAnimal.cs` `AnimalSpawner.cs` `AnimalData.cs` | สัตว์ในโลก | [Animals.md](Animals.md) |
| `ServerCore/*Data.cs` `RecipeRequirements.cs` | ตารางข้อมูลเกม | [Data.md](Data.md) |
| `test-client/FarmBot.cs` `GpCheck.cs` | บอทฟาร์มไว้เทส + เทส packet โกง | [FarmBot.md](FarmBot.md) |
| `GameCode/` + `Shims/` | โค้ดยกมาจากเกม + บั๊กที่ติดมา | [GameCode.md](GameCode.md) |

## packet ที่รับได้ตอนนี้ (32 จาก 354 ที่ client ส่ง)

| กลุ่ม | packet |
|---|---|
| เข้าเกม | `GetClock` `Auth` `Ready` |
| เคลื่อนไหว | `Move` `SetChunk` |
| แชท | `Say` `SayInExclusiveChannel` `SayInConversation` `PlayEmoticon` `GetAvailableEmotions` |
| เก็บของ | `Touch` `Collect` `GetCollectible` `DisappearEntityOnTile` |
| คราฟต์ | `GetRecipes` `GetArtifactBlueprints` `Craft` |
| ก่อสร้าง | `OccupyArtifactSite` `BuildArtifact` `PlaceCapsulatedArtifact` `GetArtifact` `DestructArtifact` `EstimateBuild` `PutMaterialsIntoArtifact` |
| สกิล | `GetSkills` `LearnSkill` `UntrainSkill` `GetStatistics` |
| อุปกรณ์ | `Equip` `GetEquipments` |
| ค่าสถานะ | ส่ง `Survival` / `SurvivalUpdated` (ยังไม่รับ packet ฝั่งนี้) |
| กล่องเก็บของ | `PutInItem` `TakeOutItem` (+ `GetInventory` ที่มี Target) |
| อื่น ๆ | `GetInventory` `GetQuests` `Cheat` `Tune` |

**ยังไม่รับ 320 ชนิด** — ที่กระทบการเล่นมากสุด: `Revive`/`Resurrect` (ตายแล้วติด),
`GetDefoggedChunks`, ทุกอย่างเกี่ยวกับ party/clan/friend/mail/warp/quest

## เฟส A แก้เสร็จแล้ว ✅

| # | เรื่อง | ที่ |
|---|---|---|
| GP-01 | 1 packet/tick + `Sleep(5)` → ตอนนี้ 120 tps, 512 packet/tick | [GameCode.md](GameCode.md) · [Program.md](Program.md) |
| GP-02 | เก็บตำแหน่งผู้เล่นจาก `Move` แล้ว | [ServerPlayer.Core.md](ServerPlayer.Core.md) · [Sync](ServerPlayer.Sync.md) |
| GP-03 | `_generatorState` ย้ายไป world + จองแบบอะตอมมิก | [ServerPlayer.Gathering.md](ServerPlayer.Gathering.md) |
| GP-04 | เก็บสิ่งปลูกสร้าง + ส่งให้คนเข้าใหม่ + ตรวจเจ้าของก่อนทุบ | [ServerPlayer.Building.md](ServerPlayer.Building.md) |
| GP-05 | เติมชื่อคนพูดในแชท | [ServerPlayer.Core.md](ServerPlayer.Core.md) |
| GP-10 | `Touch` รองรับสิ่งปลูกสร้างแล้ว | [ServerPlayer.Gathering.md](ServerPlayer.Gathering.md) |
| GP-11 | ไม่ทับชื่อเซิร์ฟด้วยชื่อผู้เล่น | [GameServer.md](GameServer.md) |
| GP-13 | ลบพารามิเตอร์ `excludeSelf` ที่ไม่ถูกใช้ | [ServerWorld.md](ServerWorld.md) |
| GP-15 | `Listener` ทน bind ล้มเหลว + ปิด socket ปลอดภัย | [GameCode.md](GameCode.md) |

รายละเอียดที่ [../project/CHANGELOG.md](../project/CHANGELOG.md)

## เฟส B–C เสร็จแล้ว ✅

| # | เรื่อง | doc |
|---|---|---|
| GP-07 | เซฟของ/สกิล/ตำแหน่ง/บ้าน/ต้นไม้ ลงดิสก์ | [Persistence.md](Persistence.md) |
| เฟส C (1/4) | สวมใส่อุปกรณ์ + แก้ NRE ที่ stub เดิมทำให้ client พัง | [Equipment.md](Equipment.md) |
| เฟส C (2/4) | ค่าสถานะเอาชีวิตรอด (เลือด/สตามินา/ความล้า) | [Survival.md](Survival.md) |
| เฟส C (3/4) | กล่องเก็บของ | [Storage.md](Storage.md) |
| เฟส C (4/4 รอบ 1) | สัตว์โผล่ในโลก + เดินสุ่ม (ยังไม่มีต่อสู้) | [Animals.md](Animals.md) |

## เลิกเชื่อ client เสร็จแล้ว ✅ (14 ส.ค. 2026)

| # | เรื่อง | doc |
|---|---|---|
| GP-08 | Craft ตรวจวัตถุดิบจริงตามสูตร (720 สูตรจากข้อมูลเกม) | [ServerPlayer.Crafting.md](ServerPlayer.Crafting.md) · [Data.md](Data.md) |
| GP-09 | Touch/Collect อิง garden ของ server + ระยะเอื้อม | [ServerPlayer.Gathering.md](ServerPlayer.Gathering.md) · [TerrainStore.md](TerrainStore.md) |
| GP-12 | Auth ต้องมี session token ที่ gateway ออกให้ | [GameServer.md](GameServer.md) · [Gateway.md](Gateway.md) |
| GP-14 | เลเวลเป็นของ server, entity type อยู่ในช่วงผู้เล่นเท่านั้น | [ServerPlayer.Core.md](ServerPlayer.Core.md) · [Persistence.md](Persistence.md) |

ทดสอบด้วย `cd test-client && dotnet run -- --gp-check` (16 ข้อ) — ดู [FarmBot.md](FarmBot.md)

## ที่ยังค้าง

| # | เรื่อง |
|---|---|
| GP-06 | แชทส่วนตัวตาย (`RadiotowerServer` ไม่มีใครต่อ) |
| GP-08b | ยังไม่ตรวจ tag ของวัตถุดิบ (ต้องให้ไอเทมมี `Tags` ก่อน) |
| GP-09b | หน่วงเก็บของยัง 2.1 วินาทีตายตัว ไม่ได้ใช้ `generator.Duration` |
