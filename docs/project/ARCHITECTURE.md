# สถาปัตยกรรม — ทั้งระบบต่อกันยังไง

## 1. ภาพรวม

Durango ตัวจริงเป็นเกมออนไลน์ที่ NEXON ปิดไปแล้ว สิ่งที่เหลือคือ **client ที่คุยกับเซิร์ฟเวอร์ที่ไม่มีอยู่แล้ว**
โปรเจกต์นี้เลยเขียน **เซิร์ฟเวอร์ขึ้นมาใหม่** ให้พูดภาษาเดียวกับ client

```
┌──────────── เครื่องโฮสต์ ────────────┐        ┌──── เครื่องแขก ────┐
│                                      │        │                    │
│  DurangoServer.exe  (.NET 9)         │        │   DurangoV2.exe    │
│  ├── Gateway         TCP  8190 ──────┼────────┼── HTTP: knock,     │
│  │   └── knock       UDP  8191       │        │   sessions, entry, │
│  ├── GameServer      TCP  8191 ──────┼────────┼── terrain, bundles │
│  └── RadiotowerServer TCP 8192       │        │                    │
│                                      │        │   TCP: packet เกม  │
│  DurangoV2.exe (โฮสต์เล่นเองด้วย) ────┼────────┘                    │
└──────────────────────────────────────┘        └────────────────────┘
```

จุดสำคัญ: **server ไม่ได้อยู่ในเกม** เป็นคนละ process กัน โฮสต์ก็เป็นแค่ client อีกตัวที่ต่อเข้ามาที่ `127.0.0.1`
(ต่างจาก offline server เดิมที่ NEXON ฝังไว้ใน `Assembly-CSharp.dll` ซึ่งเราไม่ใช้แล้ว)

## 2. โปรโตคอล

ทุก packet มีรูปแบบเดียวกัน — header 24 ไบต์ + payload:

```
 0        8        12       16       20       24
 ├────────┼────────┼────────┼────────┼────────┤
 │  Time  │  Seq   │ReplyOf │TypeCode│ Size   │  payload (MsgPack → Snappy)
 │ uint64 │ uint32 │ uint32 │ uint32 │ uint32 │
 └────────┴────────┴────────┴────────┴────────┘
   ms      เลขลำดับ  ตอบของ   ชนิด    ขนาด
                     seq ไหน  packet  payload
```

- **TypeCode** — เลขประจำชนิด packet เช่น `Move` = 4, `Touched` = 2020 ฝังเป็น `const uint TypeCode` ในทุก struct ใต้ `Messages/`
- **Seq / ReplyOf** — client ส่ง `Seq=25` มา server ตอบกลับด้วย `ReplyOf=25` ทำให้ client จับคู่คำตอบได้ (`.On<T>()`)
- **payload** — pack ด้วย MsgPack แล้วบีบด้วย Snappy
- โค้ด encode/decode อยู่ที่ `server/GameCode/Durango.Network/Packet.cs` — **copy มาจาก client ตรง ๆ** จึงเข้ากันได้แน่นอน

> เพราะ TypeCode ถูกคอมไพล์ฝังใน client ไปแล้ว → **เพิ่ม packet ชนิดใหม่ไม่ได้** ถ้าไม่ patch dll

## 3. ลำดับการเข้าเกม (ตามจริงทีละขั้น)

```
client                                    server
  │                                         │
  │─ GET  /knock ──────────────────────────▶│ Gateway.cs:45
  │◀── {server_version, assetbundle urls}   │
  │                                         │
  │─ POST /sessions {player: <json เซฟ>} ──▶│ Gateway.cs — แกะ entityId, ชื่อ,
  │◀── {user_id, session_token}             │   เลเวล, หน้าตา → RegisterPlayerData()
  │                                         │
  │─ GET  /admission ──────────────────────▶│ ตอบ admitted:true เสมอ
  │─ GET  /entry ──────────────────────────▶│ ตอบ frontend 127.0.0.1:8191
  │                                         │   (client เขียนทับเป็น IP จริงให้เอง)
  │─ GET  /terrains/1 , whole_biomes ──────▶│ TerrainStore
  │                                         │
  │═══ เปิด TCP ไป 8191 ════════════════════▶│ Listener → GameServer.cs:73
  │─ GetClock ─────────────────────────────▶│ ตอบ Clock (ซิงก์นาฬิกา)
  │─ Auth {EntityId} ──────────────────────▶│ SendWelcome()  GameServer.cs:151
  │◀── Welcome {region, storage, options}   │
  │─ Ready ────────────────────────────────▶│ GameServer.cs:102
  │◀── OK                                   │   → new ServerPlayer(...)
  │                                         │   → RegisterHandlers()   ★ ผูก handler 32 ตัว
  │◀══ SendSpawnBurst() ════════════════════│   → SendSpawnBurst()     ★ ยิงสถานะเริ่มต้น
  │    Skills, Inventory, Equipments,       │   → world.AddPlayer()    ★ บอกคนอื่นว่ามีคนมา
  │    DefoggedChunks, QuestCategories,     │
  │    WalletUpdated, AppearPlayer          │
  │                                         │
  │◀── AppearPlayer ของผู้เล่นคนอื่นทุกคน ────│ ServerWorld.cs:56
  │──▶ คนอื่นได้ AppearPlayer ของเรา         │
  │                                         │
  ▼ เข้าเกมได้แล้ว                           ▼
```

หลังจากนี้เป็น loop ปกติ: client ส่ง `Move` / `SetChunk` / `Touch` / `Collect` / `Craft` ฯลฯ
server ตอบเฉพาะคนส่ง หรือ `Broadcast` ให้ทุกคนแล้วแต่ชนิด

## 4. main loop ของ server

`Program.cs` — วนไม่หยุด เธรดเดียว:

```csharp
TimeBeginPeriod(1);                       // ดัน timer resolution ลงเหลือ 1 ms
while (true)
{
    gameServer.Process();     // รับ client ใหม่ + process packet ของทุกคน + รัน deferred
    gateway.Process();        // ตอบ HTTP ที่ค้างอยู่ในคิว
    radiotower.Process();
    // จับเวลาด้วย Stopwatch แล้ว sleep ตามส่วนที่เหลือ เพื่อล็อกที่ 120 tps
}
```

**เธรดเดียวทั้งหมด** — handler ทุกตัวจึงไม่ต้องกังวลเรื่อง race ยกเว้นส่วนที่ socket callback แตะ
(`Connection._packetQueue` มี lock อยู่แล้ว เพราะ callback ของ `SocketAsyncEventArgs` วิ่งบน thread pool)

> ✅ **GP-01 แก้แล้ว** เดิมเป็น `Thread.Sleep(5)` ซึ่งบน Windows (timer resolution 15.6 ms) นอนจริง ~15.6 ms
> = ~64 รอบ/วินาที และ `ProcessPacketQueue()` ดึง packet แค่ 1 ตัวต่อรอบ → เพดาน ~64 packet/วินาที/คน
> ตอนนี้ล็อกที่ **120 tps** (ยืนยันจาก log `[loop] 120 tps`) และระบายคิวได้ถึง 512 packet/tick
> เพดานใหม่ ≈ 61,000 packet/วินาที/คน — ไม่ใช่คอขวดอีกต่อไป

## 5. state อยู่ที่ไหนบ้าง

| state | เก็บที่ | รอดตอนออกเกมไหม | รอดตอนรีสตาร์ทเซิร์ฟไหม |
|---|---|---|---|
| ตำแหน่งผู้เล่น | `ServerPlayer._lastPosition` → `saves/players/*.json` | ✓ | ✓ |
| กระเป๋าของ | `ServerPlayer._inventory` → `saves/players/*.json` | ✓ | ✓ |
| สกิล | `ServerPlayer._knownSkills` → `saves/players/*.json` | ✓ | ✓ |
| หน้าตา/เลเวล | ส่งมาจาก client ทุกครั้งที่เข้า | ✓ (client เก็บ) | ✓ |
| ต้นไม้ที่ตัดไป | `TerrainStore._removedNaturals` → `saves/world.json` | ✓ | ✓ |
| สิ่งปลูกสร้าง | `ServerWorld._artifacts` → `saves/world.json` | ✓ | ✓ |
| generator ของธรรมชาติ | `ServerWorld._generators` — ของกลาง (RAM) | ✓ | ✗ |

✅ **GP-07 เสร็จแล้ว** — เซฟลง `server/saves/` ตอนผู้เล่นออกเกม, ทุก 60 วินาที, และตอนกด Ctrl+C
รายละเอียดที่ [../server/Persistence.md](../server/Persistence.md)

ที่ยังไม่รอด: จำนวนที่เหลือของจุดเก็บของ (generator) — รีเซ็ตเมื่อรีสตาร์ท ซึ่งตั้งใจ เพราะต้นไม้ควรฟื้นตัว
ดูรายการเต็มที่ [GAMEPLAY-REVIEW](../../server/GAMEPLAY-REVIEW.md) และสิ่งที่แก้ไปแล้วที่ [CHANGELOG](CHANGELOG.md)

## 6. ปรับแต่งเกมได้ถึงไหน

| ชั้น | คุมได้แค่ไหน | ทำยังไง |
|---|---|---|
| กฎ/ตรรกะเกม | **100%** | แก้ `server/ServerCore/` |
| ข้อมูลเกม — ไอเทม สูตร บลูพรินต์ สกิล อาชีพ สัตว์ เควส | **100%** (ยังไม่ได้เปิดใช้) | client มีสวิตช์อยู่แล้ว: ถ้า `cluster_mode = "Online"` มันจะโหลด **71 ไฟล์ JSON จาก gateway เรา** แทนไฟล์ที่อบมากับเกม — ดูข้อ 7 |
| โมเดล/เท็กซ์เจอร์/เสียง | **สูง** | Gateway มี route `/assetbundles/` อยู่แล้ว ติดแค่ patch ที่ hardcode `Info.5.2.1` ให้ใช้ไฟล์ในเครื่อง |
| UI / เรนเดอร์ / อนิเมชัน | **ต่ำ–กลาง** | ต้อง patch `Assembly-CSharp.dll` ทีละจุด |

## 7. สวิตช์ Online mode (ยังไม่ได้เปิด)

`client/Yaml.Util/Loader.cs` โหลดข้อมูลเกมแบบนี้:

```csharp
string url = GameManager.GatewayUrl + postFix;        // เช่น /assets/item/recipes
if (GameManager.ClusterMode == Mode.Online)
    yamlData = Json.Read<T>(Http.Request(url));       // ← โหลดจาก server เรา
else
    yamlData = Json.ReadFromFile<T>("offline" + postFix);   // ← จาก resources.assets ในเกม
```

ตอนนี้ `Gateway.cs` ตอบ `cluster_mode = "SingleMode"` → ใช้ไฟล์ที่อบมากับเกม
ถ้าอยากคุมข้อมูลเกมเองทั้งหมด ต้องทำ 3 อย่าง:

1. เปลี่ยน `cluster_mode` เป็น `"Online"`
2. **เพิ่ม route `/assets/*`** — ตอนนี้ `Gateway.UnhandledUrl` ตอบ `BadRequest` ให้ทุก path ที่ไม่ใช่ `/assetbundles/` หรือ `/terrains/1/` → client จะ retry 5 รอบแล้วค้างหน้าโหลดถาวร
3. **แก้ `radiotower_addresses` ให้เป็น IP จริง** — Online mode ทำให้ client ต่อ radiotower จริง แต่โค้ด rewrite ฝั่ง client แตะแค่ `frontend_addresses[0]` ไม่แตะ radiotower

ไฟล์ข้อมูล 71 ตัวที่ client จะขอ (ชื่อว่า yaml แต่เนื้อเป็น JSON):

```
/assets/item/recipes            /assets/item/prototype_data      /assets/building/blueprints
/assets/entity_types/animal     /assets/entity_types/natural     /assets/entity_types/artifact
/assets/skill/skills            /assets/skill/categories         /assets/player/jobs
/assets/survival/status_effects /assets/pet/pets_for_client      /assets/quests/quests_for_client
/assets/factions                /assets/constants                /assets/titles         ... รวม 71
```

ชุดตั้งต้นอยู่ใน `game/DurangoV2_Data/resources.assets` (path `offline/assets/...`) — แกะออกมาด้วย AssetStudio/UABE ได้

## 8. ถ้าจะแก้อะไร เริ่มดูไฟล์ไหน

| อยากทำ | ไฟล์ |
|---|---|
| เพิ่ม packet ที่ server ยังไม่รับ | `ServerCore/ServerPlayer.Core.cs` → `RegisterHandlers()` |
| แก้พฤติกรรมการเก็บของ | `ServerCore/ServerPlayer.Gathering.cs` |
| แก้ระบบก่อสร้าง | `ServerCore/ServerPlayer.Building.cs` |
| แก้สิ่งที่ส่งตอนเข้าเกม | `ServerCore/ServerPlayer.Sync.cs` → `SendSpawnBurst()` |
| แก้ HTTP route / โหมดเซิร์ฟ | `ServerCore/Gateway.cs` |
| แก้การรับ client / handshake | `ServerCore/GameServer.cs` |
| แก้ tick rate / อาร์กิวเมนต์ | `Program.cs` |
| แก้ terrain / ต้นไม้ | `ServerCore/TerrainStore.cs` |
| แก้ระบบเซฟ | `ServerCore/SaveStore.cs` · `SaveModels.cs` · `ServerPlayer.Persistence.cs` |
