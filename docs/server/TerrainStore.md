# `ServerCore/TerrainStore.cs`

**หน้าที่:** โหลดและเสิร์ฟข้อมูลแผนที่ — biome, ทะเล, แม่น้ำ, landmark, ต้นไม้/หิน (garden)

แมพเก็บเป็น byte array แบน ๆ ทั้งผืน แล้วตัดเป็น chunk ตอนที่ client ขอ

| ค่าคงที่ | ค่า | ความหมาย |
|---|---|---|
| `TileSize` | 16 | 1 chunk = 16×16 tile |
| `BiomesPerChunk` | 324 | 18×18 (16 + ขอบข้างละ 1 เพื่อให้ต่อรอยเนียน) |

## `Load(dataDir, terrainId)` — บรรทัด 126 `static`
factory เรียก constructor ส่วนตัว

## constructor — บรรทัด 35
อ่านไฟล์จาก `data/terrains/<id>/` แล้วคำนวณ `Width`/`Height` จากขนาดไฟล์ biome
(`side = sqrt(length)`) แล้ว `NumChunksX/Y = Width / 16`

## `EntryPoint` — บรรทัด 30
จุดเกิด อ่านจาก `Info.entry_points[0]` ถ้าไม่มีใช้กลางแมพ

## `GetChunkBiomes/Ocean/River/Landmark(x, y)` — บรรทัด 131–152
ตัด chunk ออกจาก array ใหญ่ด้วย `CopyChunk()` แต่ละอันมี `count` (ไบต์ต่อ tile) กับขอบไม่เท่ากัน
`Landmark` ต่างจากพวกอื่น — เตรียมไว้ล่วงหน้าใน `BuildLandmarkChunks()` แล้วเก็บใน dict

## `GetChunkGarden(x, y)` — บรรทัด 157
กรอง `Garden` (record ละ 6 ไบต์: `x:ushort, y:ushort, type:ushort`) เอาเฉพาะที่ตกอยู่ใน chunk นี้
นี่คือข้อมูลที่ `HandleSetChunk` ส่งให้ client เพื่อวาดต้นไม้/ก้อนหิน

## `TryGetNatural(tileX, tileY, out entityType)` ✅ GP-09
มีของธรรมชาติอยู่ที่ tile นี้จริงไหม และเป็นชนิดอะไร — วน `Garden` หา record ที่พิกัดตรง
`HandleTouch` ใช้ตัวนี้ปฏิเสธการแตะ tile ที่ไม่มีอะไรอยู่ และใช้ `entityType` **จากที่นี่**
แทนค่าที่ client ส่งมา (ไม่งั้น client เลือกได้ว่าจะให้ต้นไม้ต้นนั้นออกของอะไร)

เป็นการวนทั้ง array เหมือน `RemoveNatural` — แมพ 256×256 มี garden ไม่กี่พัน record ยังไม่คุ้มที่จะทำ index

## `RemoveNatural(tileX, tileY)` — บรรทัด 190
ลบต้นไม้ 1 ต้นออกจาก `Garden`
```
lock (_gardenLock):
    Garden.Length % 6 != 0 → return false        (กันข้อมูลเพี้ยน)
    วนทีละ 6 ไบต์ เจอพิกัดตรง "ตัวแรก" → ข้าม แล้วตั้ง removed = true
    ไม่เจอเลย → return false                     (ไม่แตะของเดิม)
    สร้าง array ใหม่ทับ Garden
```
`!removed &&` ในเงื่อนไขทำให้ลบแค่ตัวแรกที่เจอ — ถ้ามีของซ้อนพิกัดเดียวกัน จะเหลือตัวที่สองไว้

⚠️ **แก้ใน memory อย่างเดียว ไม่มี `Save()`** → คนที่เข้ามาทีหลังในเซสชันเดียวกันเห็นถูกต้อง
แต่รีสตาร์ทเซิร์ฟแล้วต้นไม้ขึ้นใหม่หมด (GP-07)
⚠️ สร้าง array ใหม่ทั้งก้อนทุกครั้งที่ตัดต้นไม้ — O(n) ต่อครั้ง บนแมพที่มีของหลักหมื่นจะเริ่มหน่วง
ควรเปลี่ยนไปใช้ `HashSet<(x,y)>` ของที่ถูกลบแล้วค่อยกรองตอนส่ง

## `CopyChunk(...)` — บรรทัด 227 `static`
ก๊อป chunk พร้อมขอบ ใช้ `Math.Clamp` กับพิกัดต้นทาง → chunk ที่ติดขอบแมพจะซ้ำ tile ริมสุดแทนที่จะอ่านนอก array
