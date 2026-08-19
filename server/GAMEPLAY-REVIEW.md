# DurangoServer — รีวิวโค้ดฝั่งเกมเพลย์ + ต้องทำอะไรต่อ

รีวิว **2026-08-13** · เป้าหมาย: `ServerCore/DurangoServer.cs` (1,716 บรรทัด) + `Program.cs` + `GameCode/Durango.Offline/*`
**สถานะ: หาอย่างเดียว ยังไม่แก้โค้ด**

---

## ก่อนอ่าน — แก้ความเข้าใจเรื่องขอบเขตรีวิวก่อนหน้า

[CODE-REVIEW.md](../game/CODE-REVIEW.md) กับ [ONLINE-REVIEW.md](../game/ONLINE-REVIEW.md) ที่เขียนไว้เมื่อกี้ รีวิว **offline server ที่ฝังอยู่ใน `Assembly-CSharp.dll`** ซึ่งตอนนี้พี่ **ไม่ได้ใช้แล้ว** — ของจริงที่รันในคลิปคือ `DurangoServer` (.NET, process แยก) ตัวนี้

ที่ยังเกี่ยวอยู่:

| ประเด็นเดิม | ยังใช้ได้ไหมกับ DurangoServer |
|---|---|
| **NET-01** 1 packet/tick | ✅ **ยังอยู่เป๊ะ ๆ** — `Connection.cs` ถูก copy มาทั้งไฟล์ (ดู GP-01) |
| NET-09/10 Listener + Connection.Close | ✅ ยังอยู่ — copy มาเหมือนกัน (GP-15) |
| NET-02 `Player.Instance` static | ❌ ไม่เกี่ยว — server ใหม่ไม่มีคลาสนี้ |
| NET-03 `AddInteractionHandler` | ❌ ไม่เกี่ยว — ปัญหาอยู่ฝั่ง client เท่านั้น |
| NET-05 fallback เป็น context โฮสต์ | ⚠️ เปลี่ยนรูป → กลายเป็น GP-07 (ทุกคนโหลด `.player` ไฟล์เดียวกัน) |
| SEC-01 `/entry` ตอบ 127.0.0.1 | ⚠️ ครึ่งเดียว — frontend client แก้ให้ แต่ **radiotower ไม่แก้** (GP-06) |

---

## ตอนนี้เกมเพลย์ทำอะไรได้แล้วบ้าง (32 handler)

```
Auth · Ready · GetClock                      ← เข้าเกม
Move · SetChunk                              ← เดิน + สตรีมแมพ
Say · SayInExclusiveChannel · SayInConversation · PlayEmoticon   ← แชท + อิโมท
Touch · Collect · GetCollectible · DisappearEntityOnTile         ← เก็บของจากธรรมชาติ
GetRecipes · GetArtifactBlueprints · Craft                       ← คราฟต์
OccupyArtifactSite · BuildArtifact · PlaceCapsulatedArtifact
  · GetArtifact · DestructArtifact · EstimateBuild · PutMaterialsIntoArtifact  ← ก่อสร้าง
GetSkills · LearnSkill · UntrainSkill                            ← สกิล
GetInventory · GetStatistics · GetQuests · GetAvailableEmotions
Cheat · Tune
```

เทียบตัวเลข: client ยิงมา **354 ชนิด** → server ใหม่รับ **32** (ของเดิมในเกมรับ 69 แต่เป็นคนละแนว — ตัวใหม่มีระบบ**ก่อสร้าง**ที่ของเดิมไม่มี)

**ที่พิสูจน์แล้วในคลิปว่าใช้ได้จริง:** เข้าเซิร์ฟพร้อมกัน 2 คน · เห็นกัน · เดินซิงก์ · เก็บของ · คราฟต์ · วางกองไฟ

---

# บั๊กในโค้ดเกมเพลย์ (เรียงตามความหนัก)

## GP-01 · ยกบั๊ก 1 packet/tick ติดมาด้วย + tick จริงช้ากว่าที่คิด ⭐

**ไฟล์:** `GameCode/Durango.Offline/Connection.cs:320` (copy มาจาก dll เดิม)

```csharp
private void ProcessPacketQueue()
{
    if (_packetQueue.Count != 0)
    {
        Packet packet = _packetQueue.Dequeue();   // ❌ 1 ตัว/tick เหมือนเดิม
```

คูณกับ main loop ใน `Program.cs`:
```csharp
while (true)
{
    gameServer.Process();
    gateway.Process();
    radiotower.Process();
    Thread.Sleep(5);          // ⚠️ Windows timer resolution ปกติ = 15.6 ms
}
```

`Thread.Sleep(5)` บน Windows ที่ไม่ได้เรียก `timeBeginPeriod` จะนอนจริง ~15.6 ms → **~64 tick/วินาที ไม่ใช่ 200** → เพดานคือ **~64 packet/วินาที/ผู้เล่น** (เท่ากับตอนใช้ offline server ที่ 60 FPS เป๊ะ)

ตอน login client ยิงรวดเดียว: `GetInventory` `GetSkills` `GetRecipes` `GetArtifactBlueprints` `GetQuests` `GetStatistics` `GetAvailableEmotions` `SetChunk`×N + `Move` ต่อเนื่อง → คิวยาวเป็นวินาที

**แก้ 2 จุด:**
```csharp
// 1) Connection.cs — ระบายทั้งคิว (ฝั่ง client ในเกมทำแบบนี้อยู่แล้ว)
while (_packetQueue.Count != 0) { ... }

// 2) Program.cs — ล็อก tick rate จริง
// ใช้ Stopwatch + Thread.Sleep(1) หรือเรียก timeBeginPeriod(1) ตอนเริ่ม
```
> เช็คง่าย ๆ ว่าโดนจริงไหม: นับ tick/วินาทีแล้ว print — ถ้าได้ ~64 ไม่ใช่ ~200 คือโดน

---

## GP-02 · เซิร์ฟไม่เก็บ "ตำแหน่ง" ผู้เล่นเลยสักนิด ⭐

**ไฟล์:** `ServerCore/DurangoServer.cs` — `HandleMove()` / `MakeAppearPlayer()`

```csharp
private void HandleMove(Move msg, PacketHeader header)
{
    _world.Broadcast(msg);        // ส่งต่ออย่างเดียว ไม่เก็บอะไรเลย
}

public AppearPlayer MakeAppearPlayer()
{
    WorldPosition pos = _world.GetEntryPosition();    // ❌ จุดเกิดเสมอ
    ...
}
```

ผลที่เกิด:
- **คนเข้าใหม่เห็นคนที่เล่นอยู่ยืนอยู่ที่จุดเกิด** ทั้งที่จริงเขาอยู่อีกฝั่งเกาะ — จนกว่าคนนั้นจะขยับแล้วส่ง `Move` มา (ถ้ายืนนิ่งก็เห็นผิดตลอด)
- ออกเกมแล้วเข้าใหม่ = **เด้งกลับจุดเกิดเสมอ**
- ทำ interest management (ส่งเฉพาะคนที่อยู่ใกล้) ไม่ได้เพราะไม่รู้ว่าใครอยู่ไหน
- เซฟตำแหน่งไม่ได้

**แก้:** เก็บ `Location` ตัวสุดท้ายจาก `msg.Movements[^1].Path[^1]` ไว้ใน `ServerPlayer` แล้วให้ `MakeAppearPlayer()` ใช้ค่านั้น (fallback เป็น entry point เฉพาะตอนเข้าครั้งแรก)

---

## GP-03 · `_generatorState` เป็นของ "ต่อผู้เล่น" → เก็บของซ้ำได้ ⭐

**ไฟล์:** `ServerCore/DurangoServer.cs:145`

```csharp
public class ServerPlayer
{
    private readonly Dictionary<string, List<Generator>> _generatorState = new ...;   // ❌ อยู่ใน Player
```

ต้นไม้ต้นเดียวกัน (`natural_120_88`) **สองคนมี state คนละชุด**:
- A ตัด 3 ครั้งจนหมด → `RemoveNatural` → broadcast `DisappearEntityOnTile`
- B ที่กำลังตัดอยู่ยังมี generator ค้างในเครื่องตัวเอง → **ตัดต่อได้อีก 3 ครั้ง** จากต้นที่หายไปแล้ว
- รวม = ได้ของ 2 เท่าจากต้นเดียว

**แก้:** ย้าย `_generatorState` ไป `ServerWorld` + ครอบ lock เดียวกับ `RemoveNatural` แล้วส่ง `CollectibleChanged` ให้ทุกคนที่กำลังจ้องต้นนั้น

---

## GP-04 · สิ่งปลูกสร้างไม่ถูกเก็บที่ไหนเลย → คนเข้าทีหลังไม่เห็นบ้าน ⭐

```csharp
private void HandleOccupyArtifactSite(...)
{
    ...
    _world.Broadcast(MakeArtifact(entityId, entityType, msg.Tile, size, ...));   // broadcast แล้วจบ
    Send(new Messages.Timer { Duration = 2f }, header.Seq);
    Send(new Occupied { ... }, header.Seq);
}
```

`ServerWorld` มีแค่ `Terrain` กับ `_players` — **ไม่มี `List<AppearArtifact>`**

- คนที่เข้ามาทีหลัง: `AddPlayer()` ส่งให้แค่ `other.MakeAppearPlayer()` → **ไม่เห็นสิ่งปลูกสร้างใด ๆ** ที่สร้างไปก่อนหน้า
- รีสตาร์ทเซิร์ฟ = บ้านหายหมด
- `HandleDestructArtifact` broadcast `DisappearEntity` ทันที **โดยไม่เช็คว่ามีของจริงไหม / เป็นเจ้าของไหม** → ใครก็ทุบบ้านใครก็ได้ แค่ส่ง entityId มา

**แก้:** ทำ `ArtifactStore` ใน `ServerWorld` (dict `entityId → AppearArtifact`) → ส่งทั้งชุดตอน `AddPlayer` → เซฟลง JSON → เช็ค `FounderEntityId`/`ArchitectEntityIds` ก่อน destruct

---

## GP-05 · แชทขึ้นแต่ไม่มีชื่อคนพูด

```csharp
_conn.Recv<SayInExclusiveChannel>(delegate(SayInExclusiveChannel msg, PacketHeader header)
{
    Console.WriteLine("[chat] {0}: {1}", msg.Message.EntityId, msg.Message.Body);
    _world.Broadcast(msg);        // ❌ ส่งดิบ ไม่ได้เติม Speaker
});
```

ฝั่ง client (`SocialSystem.OnSay`):
```csharp
if (msg.Message.Speaker.HasValue)
    chatStruct2.Name = msg.Message.Speaker.Value.Name;   // ไม่มีค่า → ชื่อว่าง
```

offline server เดิมเติมให้ก่อนเสมอ:
```csharp
message.Speaker = new RadioId { Name = _context.AppearPlayer.Name, Freq = _context.AppearPlayer.Freq };
```

**แก้:** เติม `msg.Message.Speaker = new RadioId { Name = this.Name, Freq = 0 }` ก่อน broadcast
> หมายเหตุ: การ broadcast กลับหาคนส่งด้วย **ถูกแล้ว** — client ไม่ได้เพิ่มข้อความตัวเองลง log ตอนส่ง ต้องรอ echo กลับ

---

## GP-06 · `RadiotowerServer` ที่เขียนไว้ ไม่เคยมีใครต่อเลย + แชทส่วนตัวตาย

ไล่เส้นทางแชทฝั่ง client:

```csharp
// แชทช่องปกติ (Region/Clan/Party)
Durango.Network.Connection connection = ((GameManager.ClusterMode != 0) ? Connections.Frontend : Connections.Radiotower);
connection.Send(msg);      // SingleMode(3) != 0 → Frontend ✅ ใช้ได้

// แชทส่วนตัว / conversation
Connections.Radiotower.Send(msg);      // ❌ Radiotower เสมอ ไม่มีเงื่อนไข
```

แต่ `/entry` ตอบ `cluster_mode = "SingleMode"` → ฝั่ง client:
```csharp
if (GameManager.ClusterMode != 0)
    State = ConnectState.Ready;        // ตั้ง Ready โดย "ไม่ต่อ" radiotower เลย
```

⇒ `Connections.Radiotower` ไม่เคย connect → `Send()` เจอ `!Connected()` → **return false เงียบ ๆ**

**สรุป:**
- แชทช่องปกติ ✅ ทำงาน (ผ่าน frontend)
- แชทส่วนตัว / `SayInConversation` ❌ ตายสนิท ไม่มี error ให้เห็น
- `RadiotowerServer` ที่เปิดฟังพอร์ต **8192** = โค้ดตายทั้งคลาส

ถ้าจะเปิดใช้จริงต้องแก้ 2 จุดพร้อมกัน:
1. `/entry` ส่ง `radiotower_addresses` เป็น IP จริง — **client ไม่ได้ rewrite ให้** (โค้ด rewrite แตะเฉพาะ `frontend_addresses[0]`) ตอนนี้ส่ง `127.0.0.1:8192` ⇒ เครื่องแขกจะวิ่งไปหาตัวเอง
2. `cluster_mode` ต้องเป็น `Online` ถึงจะ trigger การต่อ — แต่โหมด Online จะเปิด gate อื่นอีกเพียบที่ยังไม่ได้ทำ

**คำแนะนำ:** ปล่อยแชทไว้บน frontend ต่อไป แล้ว **ลบ/พักคลาส `RadiotowerServer` ไว้ก่อน** จะได้ไม่หลงคิดว่ามันทำงานอยู่

---

## GP-07 · ของ/สกิล/เลเวล ไม่เซฟ + ทุกคนโหลดไฟล์เซฟเดียวกัน

```csharp
private void LoadPlayerSave()
{
    string path = GameServer.PlayerSavePath;     // ⚠️ static ตัวเดียว ใช้ร่วมกันทุกคน
    ...
    JObject save = JObject.Parse(File.ReadAllText(path));
    _loadedDisplay = save["appear_player"]?["Display"]...
```

`PlayerSavePath` ชี้ไป `DurangoV2/AppData/offline/multi/0.player` ไฟล์เดียว

- ถ้า `/sessions` ไม่ได้ส่ง `Display` มา (parse fail / client คนละรุ่น) → `LoadPlayerSave()` → **ผู้เล่นทุกคนได้หน้าตา + เลเวลของเซฟไฟล์นั้น** = หน้าตาเหมือนกันหมด
- `_inventory` / `_knownSkills` / `_skillPoints` อยู่ใน RAM ล้วน **ไม่มีโค้ดเขียนกลับ** → ออกเกม = เก็บของมาทั้งวันหายเกลี้ยง
- `TerrainStore.RemoveNatural()` แก้ `Garden` ใน memory (คนเข้าทีหลังในเซสชันเดียวกันเห็นถูก ✅) แต่ **ไม่มี Save** → รีสตาร์ท ต้นไม้ขึ้นใหม่หมด

**แก้:** ทำ `saves/{entityId}.json` (inventory + skills + position + artifacts) เขียนตอน disconnect + auto-save ทุก N วินาที

---

## GP-08 · Craft ไม่ตรวจอะไรเลย — เสกของได้ทุกอย่าง

> ✅ **แก้แล้ว 14 ส.ค. 2026** — ดู [CHANGELOG](../docs/CHANGELOG.md) หัวข้อ "เลิกเชื่อ client" (ยังเหลือการตรวจ tag วัตถุดิบ)

```csharp
private void HandleCraft(Craft msg, PacketHeader header)
{
    // ไม่เช็คว่า recipe นี้ต้องใช้วัตถุดิบอะไร
    // ไม่เช็คว่าผู้เล่นมีวัตถุดิบจริงไหม
    // ไม่เช็คสกิล/เวิร์กเบนช์
    Send(new Messages.Timer { Duration = 2f }, header.Seq);
    ...
    foreach (string id in ids)
    {
        int idx = _inventory.FindIndex(it => it.Id == id);
        if (idx >= 0) _inventory.RemoveAt(idx);      // ลบ "เท่าที่หาเจอ" ไม่เจอก็ข้าม
    }
    _inventory.Add(crafted);                          // ได้ของเสมอ
```

`GetRecipes` ส่ง `RecipeData.AllRecipeIds` (ทุกสูตรในเกม) ให้อยู่แล้ว ⇒ client ที่แก้ packet ส่ง `Craft{RecipeId=อะไรก็ได้, Materials=null}` = **ได้ของชิ้นนั้นฟรี**

ยังไม่ใช่ปัญหาถ้าเล่นกับเพื่อน แต่ต้องทำก่อนเปิดสาธารณะ

---

## GP-09 · Collect เชื่อ `Tile` ที่ client ส่งมา 100%

> ✅ **แก้แล้ว 14 ส.ค. 2026** — tile มาจากที่ server ผูกไว้ตอน `Touch` (ยังเหลือหน่วง 2.1 วิ ตายตัว)

```csharp
_deferred.Add((Times.UnixTimeNow() + 2.1, () =>
{
    ...
    if (ranOut && _world.Terrain.RemoveNatural(msg.Tile.x, msg.Tile.y))   // ❌ ไม่ตรวจว่า tile ตรงกับ EntityId ไหม
```

- `msg.EntityId` กับ `msg.Tile` ไม่ได้ผูกกัน → ส่ง tile ไหนมาก็ลบต้นไม้ตรงนั้น
- `natural_{x}_{y}` เป็น id ที่ derive จาก tile อยู่แล้ว — ควร parse กลับมาเทียบ
- เวลา deferred fix ไว้ **2.1 วิ ตายตัว** ทั้งที่ generator มี `Duration = 1.5f + i` ของตัวเอง → animation ฝั่ง client กับเวลาจริงไม่ตรงกัน

---

## GP-10 · แตะสิ่งปลูกสร้างไม่ได้ (กองไฟที่วางแล้วใช้ไม่ได้)

```csharp
private void HandleTouch(Touch msg, PacketHeader header)
{
    ...
    if (msg.EntityType >= 10000)     // ⚠️ ทำเฉพาะของธรรมชาติ
    {
        reply.Interactions = new[] { 506, 10268 };
        reply.Collectible = ...
    }
    Send(reply, header.Seq);          // < 10000 → ส่ง Touched เปล่า ๆ
}
```

`EntityType < 10000` = **artifact / สิ่งปลูกสร้าง** — offline server เดิมทำครบ:
```csharp
Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().RecipeContainer.GetBlueprint(touch.EntityType);
msg.EntityName = blueprint.Name;
List<InteractionData.Interaction> list = new List<...>();   // เมนูโต้ตอบ
```

⇒ วางกองไฟได้ แต่**คลิกแล้วไม่มีเมนู** (จุดไฟ / ปรุงอาหาร / เก็บกลับ ไม่มีสักอัน) — ระบบก่อสร้างเลยยังเป็นแค่ของตกแต่ง

**แก้:** เติม branch `< 10000` โดยใช้ `RecipeData.BlueprintType` ที่มีอยู่แล้ว map กลับเป็นชื่อ + interaction id

---

## GP-11 · ชื่อเซิร์ฟใน LAN discovery ถูกทับด้วยชื่อผู้เล่นคนล่าสุด

```csharp
// Program.cs
ServerKnock.HostName = serverName;          // "Multi Play Server" ✅

// DurangoServer.cs — Ready handler
ServerKnock.HostName = playerName;          // ❌ ทับทุกครั้งที่มีคนเข้า
```
`WebServer.cs:220` เอาค่านี้ไปตอบ UDP knock → คนที่ค้นหาเกาะใน LAN จะเห็นชื่อ **ผู้เล่นคนล่าสุด** แทนชื่อเซิร์ฟ

---

## GP-12 · Auth สวมรอยได้ทันที

> ✅ **แก้แล้ว 14 ส.ค. 2026** — session token สุ่มจาก `/sessions` ผูกกับ entity id

```csharp
connection.Recv<Auth>(delegate(Auth auth, PacketHeader header)
{
    entityId = string.IsNullOrEmpty(auth.EntityId) ? Guid.NewGuid().ToString() : auth.EntityId;
    playerName = LookupName(entityId);
    SendWelcome(connection, entityId, playerName, header.Seq);      // ไม่ตรวจ token เลย
});
```
+ `/sessions` คืน `session_token = entityId` (token = id ตรง ๆ)

⇒ รู้ entityId ของใคร = ต่อเข้ามาเป็นคนนั้นได้เลย พร้อมได้ `PlayerData` ของเขา

---

## GP-13 · `BroadcastExcept` มีพารามิเตอร์หลอก

```csharp
public void BroadcastExcept<T>(ServerPlayer except, T msg, bool excludeSelf = false) where T : struct
{
    ...
    foreach (ServerPlayer p in snapshot)
    {
        if (p == except) continue;      // ใช้แค่ except
        p.Send(msg);
    }
}
// เรียกจริง:
BroadcastExcept(player, player.MakeAppearPlayer(), excludeSelf: true);   // excludeSelf ไม่ถูกอ่านเลย
```
ไม่ทำให้พัง (พฤติกรรมถูกอยู่แล้ว) แต่อ่านแล้วเข้าใจผิดว่ามีสวิตช์ — ควรลบพารามิเตอร์ทิ้ง

---

## GP-14 · Level / EntityType / Display เชื่อ client 100%

> ✅ **แก้แล้ว 14 ส.ค. 2026** — เลเวลยึดของ server, entity type ต้องอยู่ช่วง 1000-1999

```csharp
// Gateway /sessions
data.Level = appear.Value<int?>("Level") ?? 0;
data.EntityType = appear.Value<ushort?>("EntityType") ?? 0;
data.DisplayJson = display.ToString(...);
```
มาจากไฟล์ `.player` บนเครื่อง client ล้วน ๆ — แก้ไฟล์เองก็ตั้งเลเวลอะไรก็ได้

---

## GP-15 · บั๊กที่ copy ติดมาจาก dll เดิม (ยังอยู่ครบ)

**`Listener.cs`** — เหมือน ONLINE-REVIEW NET-09/NET-05 เป๊ะ:
```csharp
public void Start(int port)
{
    _listenSocket = new Socket(...);        // สร้างก่อน try
    try { _listenSocket.Bind(local_end); ... _acceptArgs = new SocketAsyncEventArgs(); ... }
    catch (Exception e) { Debug.LogException(e); }    // bind fail → กลืน แล้วไปต่อ
}

private void Accept()
{
    if (!_listenSocket.AcceptAsync(_acceptArgs))      // ❌ _acceptArgs ยัง null อยู่
```
**ถ้าเปิดเซิร์ฟซ้ำ / พอร์ตไม่ว่าง** → bind พัง → `_acceptArgs` เป็น null → `Process()` เรียก `Accept()` ทุก tick → **`ArgumentNullException` ท่วมคอนโซล ~64 ครั้ง/วินาที** และเซิร์ฟดูเหมือนรันอยู่แต่ไม่รับใคร

`Close()` ก็ยังปิด socket ขณะ `AcceptAsync` ค้าง (unhandled exception บน thread pool) และ `try/finally` ไม่มี `catch`

**`Connection.cs`** — buffer ตายตัว 8 ก้อน × 2 MB ≈ **16 MB ต่อ 1 connection**, ไม่มี timeout สำหรับ connection ที่ต่อแล้วไม่ส่ง `Ready` (ค้างใน `_connections` ตลอด)

---

# ระบบที่ยัง "ไม่มีเลย" — เรียงตามผลกระทบต่อการเล่น

| ลำดับ | ระบบ | packet ที่ต้องทำ | ทำไมสำคัญ |
|---|---|---|---|
| 1 | **สวมใส่อุปกรณ์** | `Equip` · `Equipments` | ตอนนี้ `SendEquipments()` ส่ง `Presets = null` → ใส่ของไม่ได้เลย คราฟต์ขวานมาก็ใช้ไม่ได้ |
| 2 | **แตะ/ใช้งานสิ่งปลูกสร้าง** | GP-10 | ก่อสร้างได้แต่ใช้ไม่ได้ = ระบบยังไม่ครบวง |
| 3 | **ค่าสถานะเอาชีวิตรอด** | `Survival` · `SurvivalUpdated` · `GetStatusEffects` | ตอนนี้ส่ง `Life = Gauge(1)` ค่าเดียว ไม่มีหิว/ล้า/สตามินา |
| 4 | **สัตว์ + ต่อสู้** | `AppearAnimal` · `UseBattleAction` · `Damaged` · `BattleBegun` · `ExitBattle` | ไม่มีศัตรูในโลกเลย — และถ้าจะทำต้องทำ **ตาย/ฟื้น** (`Revive`) ด้วย ไม่งั้นตายแล้วติดจอ |
| 5 | **กล่อง/คลังเก็บของ** | `PutInItem` · `TakeOutItem` · `GetInventory(target)` | เก็บของเกิน 50 ช่องไม่ได้ (`MaxSize = 50`) |
| 6 | **ฟาร์ม** | `PlantSeed` · `Sprinkle` · `UprootPlant` | สายปลูกพืชทั้งสาย |
| 7 | **เพ็ท / พาหนะ** | `SpawnPet` · `Mount` · `Unmount` … | |
| 8 | **เควสจริง** | `GetQuests` ตอนนี้คืน `Todos = null` | ไม่มีเป้าหมายให้ทำ |
| 9 | **ตลาด** | `SearchProducts` · `BuyProduct` | |
| 10 | ปาร์ตี้ / แคลน / เพื่อน / เมล / วาร์ป | ~90 packet | ทำทีหลังได้ ไม่กระทบการเล่นพื้นฐาน |

---

# ต้องทำอะไรต่อ — roadmap

### เฟส A · ทำของที่มีอยู่ให้ "ถูก" ก่อน — ✅ เสร็จแล้ว 2026-08-13
- [x] **GP-01** ใส่ `while` ใน `ProcessPacketQueue` + คุม tick rate จริงใน `Program.cs` → ยืนยัน 120 tps
- [x] **GP-02** เก็บตำแหน่งผู้เล่นจาก `Move` แล้วใช้ใน `MakeAppearPlayer`
- [x] **GP-04** ทำ `ArtifactStore` ใน `ServerWorld` + ส่งให้คนเข้าใหม่ + ตรวจเจ้าของก่อนทุบ
- [x] **GP-03** ย้าย `_generatorState` ไป world-level + จองแบบอะตอมมิก
- [x] **GP-05** เติม `Message.Speaker` ก่อน broadcast แชท
- [x] **GP-10** รองรับ `Touch` ของ `EntityType < 10000`
- [x] **GP-11** เอา `ServerKnock.HostName = playerName` ออก
- [x] **GP-13** ลบพารามิเตอร์ `excludeSelf` ที่ไม่ถูกใช้ (แถม)
- [x] **GP-15** กัน `Listener` bind fail + ปิด socket ปลอดภัย
- 📋 รายละเอียดทั้งหมดที่ [../docs/CHANGELOG.md](../docs/CHANGELOG.md)
- ⏳ **เกณฑ์ผ่าน (ยังไม่ได้ทดสอบกับ client จริง):** คนที่ 3 เข้ามากลางเกม เห็นบ้าน เห็นคนอื่นยืนถูกตำแหน่ง แชทมีชื่อ

### เฟส B · เก็บความคืบหน้าได้ — ✅ ส่วน GP-07 เสร็จแล้ว
- [x] **GP-07** เซฟ `saves/players/{entityId}.json` — inventory + skills + position
- [x] เซฟ `Garden` (ต้นไม้ที่ตัดไป) + artifact ลงดิสก์ (`saves/world.json`)
- [ ] แยก `PlayerSavePath` ออกจาก static ตัวเดียว (ยังใช้เป็น fallback หน้าตาอยู่)
- 📋 รายละเอียดที่ [../docs/server/Persistence.md](../docs/server/Persistence.md)
- ✅ **เกณฑ์ผ่าน:** ปิดเซิร์ฟเปิดใหม่ ของยังอยู่ บ้านยังอยู่ ยืนที่เดิม

### เฟส C · ทำให้ "เล่นเป็นเกม" ได้
- [x] **Equip / Equipments** — เสร็จแล้ว พร้อม `EquipData` (อาวุธ 248 / เกราะ 376 สกัดจากตัวเกม)
      📋 [../docs/server/Equipment.md](../docs/server/Equipment.md)
- [x] **Survival gauges** — เลือด/สตามินา/ความล้า เสร็จแล้ว (เวลากลางวันกลางคืนยังไม่ทำ)
      📋 [../docs/server/Survival.md](../docs/server/Survival.md)
- [~] **สัตว์ + ต่อสู้ + ตาย/ฟื้น** - รอบ 1 เสร็จ: สัตว์โผล่ในโลก + เดินสุ่ม (213 ชนิด)
      ยังเหลือ: ต่อสู้ · ตาย/ฟื้น · AI ไล่/หนี · respawn
      📋 [../docs/server/Animals.md](../docs/server/Animals.md)
- [x] **กล่องเก็บของ** — เสร็จแล้ว 📋 [../docs/server/Storage.md](../docs/server/Storage.md)

### เฟส D · กันโกงก่อนเปิดให้คนอื่น
- [x] **GP-08** ตรวจวัตถุดิบจริงตอน Craft — 720 สูตรจากข้อมูลเกม (`RecipeRequirements.cs`)
      ยังเหลือ: ตรวจ **tag** ของวัตถุดิบ (ต้องให้ไอเทมที่ server สร้างมี `Tags` ก่อน)
      📋 [../docs/server/ServerPlayer.Crafting.md](../docs/server/ServerPlayer.Crafting.md)
- [x] **GP-09** ผูก `EntityId` กับ `Tile` ตอน `Touch` + เช็คว่ามีของธรรมชาติจริง + ระยะเอื้อม 8 tile
      ยังเหลือ: หน่วง 2.1 วิ ตายตัว ไม่ได้ใช้ `generator.Duration`
      📋 [../docs/server/ServerPlayer.Gathering.md](../docs/server/ServerPlayer.Gathering.md)
- [x] **GP-12** session token จริง (สุ่ม 64 ตัวอักษร ผูกกับ entityId ใน `/sessions`, อายุ 12 ชม.)
      📋 [../docs/server/GameServer.md](../docs/server/GameServer.md)
- [x] **GP-14** เลเวลยึดของ server (client มีผลแค่ login แรก + เพดาน 60), entity type 1000-1999 เท่านั้น
      📋 [../docs/server/ServerPlayer.Core.md](../docs/server/ServerPlayer.Core.md)
- [x] **GP-04** เช็คเจ้าของก่อน `DestructArtifact` (ทำไปแล้วในเฟส A)
- [ ] timeout connection ที่ไม่ส่ง `Ready`
- [ ] rate limit — ยังยิง `Touch`/`Collect` รัวได้ไม่จำกัด (โดนปฏิเสธก็จริงแต่กิน CPU)

ทดสอบทั้งหมดด้วย `cd test-client && dotnet run -- --gp-check` (16 ข้อ, exit code 1 ถ้าตก)

---

## ยังไม่ได้ตรวจ

- `ServerCore/RecipeData.cs` (3,183 บรรทัด) — ยังไม่ได้ไล่ว่า blueprint/recipe map ครบไหม
- `ServerCore/SkillData.cs`, `NaturalData.cs` — ดูผ่าน ๆ
- `GameCode/MessagePacking.cs` + Packing — ยังไม่ได้ตรวจว่า pack/unpack ตรงกับ client ทุกตัวไหม
- `Shims/*` — `TimesShim`, `UnityEngineShims` ยังไม่ได้ดูว่าค่าเวลาตรงกับที่ client คาดหวังไหม
- ยังไม่ได้รันทดสอบเอง — ทั้งหมดนี้อ่านจากโค้ด ควรยืนยันด้วยการเปิด 3 client แล้วดู log
