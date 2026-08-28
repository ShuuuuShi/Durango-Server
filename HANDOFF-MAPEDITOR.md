# HANDOFF: Durango Map Editor — Local Preview (ไม่ใช้ server)

สถานะ: 29 ส.ค. 2026 — Local Preview สร้างฉากได้แล้วแต่ยังเทา (mesh ไม่ render เพราะ shader)

## 1. สิ่งที่ทำไปแล้ว

### 1.1 Isolated Preview Clone

สร้างสำเนาแยกที่ `C:\Users\thana\Desktop\MapEditor-Preview\`:

- `game/` — สำเนาจาก `game/` (Unity 2017.4.34f1 PC build)
- `client/` — สำเนาจาก `client/` (source ที่แกะจาก Assembly-CSharp.dll)
- `server/` — สำเนาจาก `server/` (.NET 9)
- `client-mod-sdk/` — client mod SDK (net35)
- `mod-sdk/` — server mod SDK (net9.0)
- `editor-mod/` — client mod สำหรับ editor overlay (F6/F7/F8)
- `launch-preview.ps1` — launcher (มี `-LocalOnly` mode)
- `send-f6.ps1` / `send-f8.ps1` — ส่งปุ่ม F6/F8 ไปยังหน้าต่างเกม clone

ยืนยันแล้วว่าต้นฉบับไม่เปลี่ยน (SHA-256 เทียบก่อน/หลัง):
- `game/DurangoV2.exe` = `557616d7...`
- `game/DurangoV2_Data/resources.assets` = `0fad3922...`
- `server/saves/world.json` = `1e90a195...`

### 1.2 Server-based Editor (โหมดปกติ — ยังใช้ได้)

**Server clone** (`MapEditor-Preview/server/`):

ไฟล์ `ServerCore/Gateway.Admin.cs` เพิ่ม routes (ใต้ `/admin/editor/`):

- `GET /admin/editor/artifacts` — รายการ artifact ทั้งหมด (อ่านจาก `_world.SnapshotArtifacts()`)
- `GET /admin/editor/blueprints` — รายการ blueprint 570 ชนิดจาก `RecipeData.AllBlueprintIds`
- `POST /admin/editor/artifact/add` — เพิ่ม artifact จาก blueprint
- `POST /admin/editor/artifact/move` — ย้าย artifact
- `POST /admin/editor/artifact/remove` — ลบ artifact
- `GET /admin/editor/terrain/biome?x=&y=` — อ่านค่า biome tile
- `POST /admin/editor/terrain/biome` — แก้ biome tile (รักษา flags 2 บิตบน)
- `POST /admin/editor/save` — บังคับ save world

ไฟล์ `ServerCore/TerrainStore.cs` เพิ่ม:
- `SetBiome(int tileX, int tileY, byte biomeType, out string error)` — แก้ biome พร้อม backup `whole.biomes.editor-backup-*`
- `GetRawBiome(int tileX, int tileY)` — อ่าน raw biome byte

ทดสอบผ่านแล้ว:
- เพิ่ม/ย้าย/ลบ artifact ผ่าน API ได้
- แก้ biome ที่ `(120,120)` จาก 0 → 5 แล้ว restart server ค่ายังอยู่
- backup สร้างอัตโนมัติ

**Client mod** (`MapEditor-Preview/editor-mod/MapEditorPreviewPlugin.cs`):

- F6 เปิด/ปิดแผง editor
- F7 refresh รายการ artifacts
- F8 free camera (WASD + Q/E + เมาส์ขวาหมุน)
- ปุ่ม เพิ่ม/ย้าย/ลบ/บันทึก
- ช่องกรอก blueprint และ biome

ติดตั้งที่ `MapEditor-Preview/game/mods/MapEditorPreviewMod.dll`

**Launcher ปกติ** (ใช้ server):

```powershell
powershell -File "C:\Users\thana\Desktop\MapEditor-Preview\launch-preview.ps1" -Terrain ri35te -GatewayPort 8290 -GamePort 8291 -EditorToken preview-editor-local -Wait
```

### 1.3 Local Preview (ไม่มี server — งานที่ยังไม่เสร็จ)

**เป้าหมาย**: เปิดเกม clone โดยไม่ start `DurangoServer.dll`, ไม่มี TCP/HTTP, อ่าน terrain/world จากไฟล์ตรง

**Launcher**:

```powershell
powershell -File "C:\Users\thana\Desktop\MapEditor-Preview\launch-preview.ps1" -LocalOnly -Terrain ri35te -Project "C:\Users\thana\Desktop\MapEditor-Preview\server\saves\world.json" -Wait
```

**ไฟล์ที่สร้างใน client clone:**

| ไฟล์ | ทำอะไร |
|---|---|
| `MapPreviewMode.cs` | ตรวจ flag `DURANGO_LOCAL_PREVIEW=1` หรือ arg `-local-preview`, path guard ให้อยู่ใต้ `MapEditor-Preview/` เท่านั้น |
| `MapPreviewTerrainSource.cs` | อ่าน `info.yml`, `whole.biomes`, `whole.ocean`, `whole.rivers`, `whole.garden`, `whole.landmarks`, `oceans.dm` จาก directory; SetBiome/SaveBiome พร้อม backup |
| `MapPreviewWorldSource.cs` | อ่าน server `world.json` → parse `Artifacts`/`RemovedNaturals`; Save() เขียนกลับพร้อม backup |
| `MapPreviewChunkSource.cs` | ตัด whole arrays เป็น chunk 16×16 (biome 324, ocean 289, river 867) — ยังไม่ได้ใช้ |
| `MapPreviewBootstrap.cs` | MonoBehaviour สร้างฉาก preview: camera, terrain plane, artifact markers, editor GUI |

**จุดเชื่อม startup:**

`Initializer_PC.cs` (clone) เพิ่ม:

```csharp
if (MapPreviewMode.Enabled)
{
    MapPreviewBootstrap.Launch();
    return; // ไม่ LoadScene("Title")
}
```

`DurangoUpdateGate.cs` (clone) เพิ่ม:

```csharp
if (MapPreviewMode.Enabled)
{
    MapPreviewBootstrap.Launch();
    return true; // ข้าม updater check
}
```

`BotBridge.cs` (clone) เพิ่ม guard ไม่ให้เปิด TCP listener ใน local mode.

### 1.4 สถานะ Local Preview ปัจจุบัน

**ทำงานได้:**
- ✅ เปิดเกม clone โดยไม่มี `dotnet DurangoServer.dll`
- ✅ ไม่มี listener บนพอร์ต 8290/8291/8390/8391
- ✅ อ่าน terrain จริงจาก `whole.biomes` (256×256)
- ✅ อ่าน world จริงจาก `world.json` (30 artifacts)
- ✅ checkpoint log ยืนยันครบทุกขั้น (ดู `game/local-preview-hook.log`)
- ✅ ไม่เข้าสู่ Title scene / auth / network flow

**ปัญหาที่ยังแก้ไม่เสร็จ:**
- ❌ หน้าจอเทา — mesh/texture สร้างแล้วแต่ไม่ render

**สาเหตุที่วิเคราะห์:**

`Shader.Find()` อาจคืน null ใน Unity player build เพราะ shader ที่ไม่ถูกอ้างอิงโดย material ใดใน scene จะไม่ถูก include ไว้ใน build เมื่อ `Shader.Find("Standard")` คืน null แล้ว `new Material(null)` จะสร้าง material ที่ไม่ render อะไรเลย

## 2. สิ่งที่ต้องทำต่อ

### 2.1 แก้หน้าจอเทา (เร่งด่วนที่สุด)

แก้ไขล่าสุดใน `MapPreviewBootstrap.cs` เปลี่ยนจาก vertex-colored mesh เป็น:
- สร้าง `Texture2D` จาก biome colors (ทุก tile)
- ใช้ `GameObject.CreatePrimitive(PrimitiveType.Plane)` + material ที่มี texture
- `FindShader()` ลองหลายชื่อ: `"Unlit/Texture"`, `"Sprites/Default"`, `"Legacy Shaders/Diffuse"` ฯลฯ

**แต่ยังไม่ได้ build/install/test รอบล่าสุดนี้**

ขั้นตอนที่ต้องทำ:

```bash
# 1. Build
dotnet build "C:/Users/thana/Desktop/MapEditor-Preview/client/Assembly-CSharp.csproj" -c Release

# 2. Install (ต้องหยุดเกมก่อน)
cp "C:/Users/thana/Desktop/MapEditor-Preview/client/bin/Release/net35/Assembly-CSharp.dll" \
   "C:/Users/thana/Desktop/MapEditor-Preview/game/DurangoV2_Data/Managed/Assembly-CSharp.dll"

# 3. ล้าง log เก่า
rm -f "C:/Users/thana/Desktop/MapEditor-Preview/game/local-preview-hook.log"
rm -f "C:/Users/thana/Desktop/MapEditor-Preview/logs/local-preview.log"

# 4. เปิด LocalOnly
powershell -File "C:/Users/thana/Desktop/MapEditor-Preview/launch-preview.ps1" -LocalOnly -Terrain ri35te
```

**ถ้ายังเทา:**

1. เช็ค `local-preview-hook.log` ดูบรรทัด `shader=...` — ถ้า `null` แปลว่าไม่มี shader ใดใช้ได้
2. ลองใช้ material ที่มีอยู่แล้วใน scene เดิม — หาจาก `Resources.FindObjectsOfTypeAll<Material>()`
3. หรือใช้ `AssetBundle.LoadFromFile()` โหลด material จาก bundle ของเกมเอง
4. ตรวจว่า camera ถูก enable และมี `clearFlags` ที่ถูกต้อง
5. ถ้า plane ใหญ่เกิน ให้ลอง `farClipPlane = 200000` (ตั้งไว้แล้วในโค้ดล่าสุด)

### 2.2 ปรับ Editor Controls

`MapPreviewBootstrap.cs` มีปัญหา: ช่อง `Tile X`/`Tile Y` ยังใช้ `_biomeText` ร่วมกัน (bug) ต้องแยกเป็น `_tileXText`/`_tileYText` — แก้บางส่วนแล้วใน edit ล่าสุด แต่ยังไม่ได้ build

ต้องอัปเดต `SetBiome()`, `AddArtifact()`, `MoveArtifact()` ให้ใช้ค่าจาก `_tileXText`/`_tileYText` แทนค่า hardcoded `(0,0)`:

```csharp
private int TileX { get { int v; return Int32.TryParse(_tileXText, out v) ? v : 0; } }
private int TileY { get { int v; return Int32.TryParse(_tileYText, out v) ? v : 0; } }
```

### 2.3 ปรับ Camera Controls

ปัจจุบัน `UpdateCamera()` ถูกเรียกเฉพาะเมื่อ `!_editorOpen` — ควรเรียกทุกเฟรม และเพิ่ม:
- RMB + เมาส์เคลื่อน = หมุนกล้อง
- Scroll = ซูมเข้า/ออก
- แสดงพิกัด tile ที่เมาส์ชี้

### 2.4 ใช้ Game Renderer จริง (ระยะยาว)

Local Preview ปัจจุบันเป็น texture plane แบบง่าย ไม่ใช่ renderer ของเกม

เพื่อให้เหมือนเกมจริงที่สุด ต้อง:

1. ใช้ `TerrainChunk_PC` mesh (flat tile quads, UV, material) จาก source เดิม
2. ใช้ `WaterData`/`RiverData` สำหรับน้ำ/แม่น้ำ overlay
3. ใช้ `AssetBundleManager` โหลด prefab/material/texture จาก `StreamingAssets/AssetBundles/`
4. ใช้ `TerrainChunkBase` สำหรับ natural/landmark placement
5. ใช้ `ArtifactManager`/`ModelComponent` pipeline สำหรับ artifact จริง

**ข้อจำกัดที่ยอมรับได้:**
- Main scene และ serialized dependencies อยู่ใน build data ไม่มี `.unity` source
- `resources.assets` มี corruption warning เดิมของเกม
- terrain mesh เป็น flat `Y=0` (ไม่มี heightmap/collision)
- prefab บางตัวอาจไม่โหลดได้

### 2.5 Undo/Redo

ยังไม่มี undo/redo ใน Local Preview ต้องเพิ่ม:
- Stack ของ snapshot (terrain bytes + world artifacts)
- Ctrl+Z = undo, Ctrl+Y = redo
- Snapshot ก่อนทุก mutation

### 2.6 Water/River Overlay

`MapPreviewTerrainSource` มี `Ocean`/`Rivers` bytes แล้วแต่ยังไม่ได้ใช้ในการ render

ต้องเพิ่มใน `CreateBiomeTexture()`:
- อ่าน `Ocean` bytes → ถ้า depth > threshold ให้ override สีเป็นสีน้ำ
- อ่าน `Rivers` bytes → ถ้า channel 2 (depth) > 5 ให้ override เป็นสีแม่น้ำ

## 3. โครงสร้างไฟล์สำคัญ

### 3.1 Terrain binary format (จาก `TerrainStore.cs`)

```
whole.biomes:  width × height bytes (6 bits biome + 2 bits flags)
whole.ocean:   (width+1) × (height+1) bytes (water depth per vertex)
whole.rivers:  (width+1) × (height+1) × 3 bytes (flow_x, flow_y, depth)
oceans.dm:     width × height bytes (signed coast distance)
whole.garden:  N × 6 bytes (x:u16, y:u16, entityType:u16)
whole.landmarks: N × 16 bytes (x:u16, y:u16, id:u16, rotate:u8, offsets, scales)
```

### 3.2 Biome enum (จาก `Shared.Region.Biome`)

```
0=TemperateForest  1=TropicalForest  2=Desert       3=Tundra
4=SnowField        5=Grassland       6=SwampMud     7=Volcanic
9=PebbleBeach     10=SandBeach      11=ColdOcean   12=WarmOcean
13=River          14=Lake           15=Lava
```

### 3.3 World save format (จาก `SaveModels.cs`)

```json
{
  "TerrainId": "ri35te",
  "Artifacts": [
    {
      "EntityId": "poi_dock_5",
      "EntityType": 7001,
      "BlueprintId": "dock",
      "TileX": 170, "TileY": 90,
      "SizeX": 3, "SizeY": 3,
      "Rotation": 0, "Floor": 0, "Stories": 1,
      "BuildingState": 0
    }
  ],
  "RemovedNaturals": [[55,166],[56,168],...],
  "Boxes": {}, "Farms": [], "Version": 2
}
```

### 3.4 Coordinate system

```
1 tile = 200 Unity world units
1 chunk = 16×16 tiles = 3200×3200 units
tile (x,y) → world (x*200, 0, y*200)
tile center = tile origin + (100, 0, 100)
```

## 4. วิธีเปิด/ปิด/ตรวจสอบ

### เปิด Local Preview (ไม่มี server):

```powershell
powershell -File "C:\Users\thana\Desktop\MapEditor-Preview\launch-preview.ps1" `
  -LocalOnly `
  -Terrain ri35te `
  -Project "C:\Users\thana\Desktop\MapEditor-Preview\server\saves\world.json" `
  -Wait
```

### เปิด Server-based Editor:

```powershell
powershell -File "C:\Users\thana\Desktop\MapEditor-Preview\launch-preview.ps1" `
  -Terrain ri35te -GatewayPort 8290 -GamePort 8291 `
  -EditorToken preview-editor-local -Wait
```

### ตรวจสอบว่าไม่มี server:

```powershell
Get-NetTCPConnection -State Listen | Where-Object { $_.LocalPort -in 8290,8291,8390,8391 }
Get-CimInstance Win32_Process | Where-Object { $_.CommandLine -like "*MapEditor-Preview*DurangoServer*" }
```

### ตรวจสอบว่าต้นฉบับไม่เปลี่ยน:

```bash
sha256sum "../../game/DurangoV2.exe"
sha256sum "../../game/DurangoV2_Data/resources.assets"
sha256sum "../../server/saves/world.json"
```

## 5. คำเตือน

- **ห้ามแก้ไฟล์ใน `game/`, `client/`, `server/` ต้นฉบับ** — แก้เฉพาะใน `MapEditor-Preview/`
- **ห้าม deploy ไป server จริง** — Local Preview เขียนเฉพาะ `saves-preview/` และ `data/terrains/extracted/` ใน clone
- **ห้ามเขียนกลับ `resources.assets`/`level*`** — ยังไม่มีระบบ Unity asset write-back
- Build client ต้องหยุดเกมก่อน (DLL ถูกล็อก)
- Build mod (`editor-mod/`) แล้วต้อง copy ไป `game/mods/` ด้วย และต้องหยุดเกมก่อน copy

## 6. Environment variables

| ตัวแปร | ค่า | ใช้เมื่อ |
|---|---|---|
| `DURANGO_LOCAL_PREVIEW` | `1` | เปิด Local Preview mode |
| `DURANGO_PREVIEW_ROOT` | path | root ของ preview (default: `../../MapEditor-Preview` จาก `game/`) |
| `DURANGO_PREVIEW_TERRAIN` | id | terrain id (default: `ri35te`) |
| `DURANGO_PREVIEW_TERRAIN_DIR` | path | path ไปยัง extracted terrain |
| `DURANGO_PREVIEW_WORLD` | path | path ไปยัง world.json |

Command-line arg `-local-preview` ก็เปิดโหมดนี้ได้เช่นกัน

## 7. Log files

| ไฟล์ | เนื้อหา |
|---|---|
| `logs/local-preview.log` | Unity player log (LocalOnly mode) |
| `logs/client.log` | Unity player log (server mode) |
| `logs/server.stdout.log` | server console output |
| `game/local-preview-hook.log` | checkpoint จาก `MapPreviewBootstrap.Awake()` |
| `game/clientmods.log` | client mod loading log |
