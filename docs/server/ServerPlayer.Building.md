# `ServerCore/ServerPlayer.Building.cs`

**หน้าที่:** ระบบก่อสร้าง — จองพื้นที่ วางของจากแคปซูล สร้าง ทุบ

นี่คือส่วนที่ **offline server เดิมของ NEXON ไม่มี** เป็นของที่เขียนขึ้นใหม่ทั้งหมด

---

## `MakeCapsuleItem(prototype, name, icon)` — บรรทัด 37 `static`

สร้าง "ไอเทมแคปซูล" — ของที่วางลงพื้นแล้วกลายเป็นสิ่งปลูกสร้าง
สาระอยู่ที่ `Ext = new ArtifactCapsule { BlueprintId = ... }` โดย `BlueprintId` = `prototype` ที่ตัดคำนำหน้า `capsulated_` ออก
(เช่น `capsulated_bonfire` → blueprint `bonfire`)

ใช้ 2 ที่: แจกกองไฟตอนเข้าเกม (`ApplyPlayerData`) และ cheat `add bonfire`

## `HandleOccupyArtifactSite(msg, header)` — บรรทัด 83

"จองพื้นที่" — ขั้นแรกของการสร้างอาคาร

1. แปลง `msg.BlueprintId` → `entityType` ผ่าน `RecipeData.BlueprintType` **ไม่เจอ = ตอบ `Abort` แล้วจบ** (กัน blueprint มั่ว)
2. ถ้า client ไม่ส่งขนาดมา (`size.x <= 0`) ดึงขนาดจริงจาก `RecipeData.BlueprintSize`
3. **`_world.Broadcast(MakeArtifact(...))`** → ทุกคนเห็นโครงอาคารโผล่ทันที
4. ตอบ `Timer { 2f }` + `Occupied { EntityId, TileX, TileY, Floor }`

## `MakeArtifact(entityId, entityType, tile, size, rotation, floor, stories)` — บรรทัด 110

โรงงานสร้าง `AppearArtifact` — packet ที่บอก client ว่า "มีสิ่งปลูกสร้างตรงนี้"
ค่าที่ตั้งไว้ตายตัวตอนนี้:

| ฟิลด์ | ค่า | หมายถึง |
|---|---|---|
| `IsAlive` | `true` | |
| `Durability` | เต็ม (Gauge 1.0) | ยังไม่มีระบบผุพัง |
| `BuildingState` | `Occupied` | สถานะ "จองแล้ว" ไม่ใช่ "สร้างเสร็จ" |
| `Display.Parts` | dict ว่าง | ยังไม่มีระบบเลือกวัสดุ/สี |
| `FounderEntityId` / `ArchitectEntityIds` | ตัวเราคนเดียว | **มีข้อมูลเจ้าของแล้ว แต่ยังไม่มีใครเอาไปเช็คสิทธิ์** |
| `Tags` / `Postprocess` / `Farming` / `Cage` | null | ระบบย่อยที่ยังไม่ได้ทำ |

## `HandlePlaceCapsulatedArtifact(msg, header)` — บรรทัด 179

วางของจากแคปซูล (เช่นกองไฟ) — ทางลัดที่ข้ามขั้นตอนก่อสร้างไปเลย

1. หาไอเทมใน `_inventory` ตาม `msg.ItemId` **แล้วลบทิ้งทันที** (ใช้แล้วหมดไป) — ไม่เจอ = `Abort`
2. `prototype` → ตัด `capsulated_` → `BlueprintId` → `RecipeData.BlueprintType` ไม่เจอ = `Abort`
3. ดึงขนาดจาก `BlueprintSize`
4. `Broadcast(MakeArtifact(...))` + `Timer` + `SendInventory()`

ลำดับตรงนี้ถูกแล้ว: ลบของก่อนตรวจ blueprint จะทำของหาย — โค้ดตรวจ blueprint **หลัง** ลบของไปแล้ว
⚠️ ถ้า blueprint ไม่รู้จัก ของจะหายไปโดยไม่ได้อะไรกลับมา ควรตรวจให้ครบก่อนค่อยลบ

## `HandleBuildArtifact(msg, header)` — บรรทัด 215

"ลงมือสร้าง" — ตอบ `Timer { 2f }` แล้วเข้าคิว `_deferred` +2.1 วิ พอถึงเวลา broadcast:
- `ArtifactBuilt { EntityId, BuilderId }`
- `ArtifactCompleted { EntityId }`

ไม่ตรวจวัสดุ ไม่ตรวจว่า `EntityId` นี้มีอยู่จริงไหม ส่ง id อะไรมาก็ "สร้างเสร็จ"

## `HandleGetArtifact(msg, header)` — บรรทัด 226

ตอบ `ArtifactMaterials` ที่มี dict ว่าง — client ถามว่าอาคารนี้ใส่วัสดุอะไรไปแล้วบ้าง เราตอบว่า "ไม่มี"
เป็น stub ให้ UI ไม่ค้าง

## `HandleDestructArtifact(msg, header)` ✅ GP-04

```
ไม่พบ artifact ในโลก        → log + Abort
CanModifyArtifact() ไม่ผ่าน → log + Abort
ผ่าน → RemoveArtifact() + Broadcast(DisappearEntity)
```
เดิมเป็น `Broadcast(DisappearEntity)` บรรทัดเดียวโดยไม่ตรวจอะไร

---

## ✅ GP-04 แก้แล้ว — มีที่เก็บสิ่งปลูกสร้างแล้ว

เดิมทุกเมทอด **broadcast แล้วทิ้ง** — `ServerWorld` ไม่มีที่เก็บ artifact เลย
ทำให้คนที่เข้าทีหลังไม่เห็นบ้าน และตรวจสิทธิ์การทุบไม่ได้

ตอนนี้:
1. `HandleOccupyArtifactSite` / `HandlePlaceCapsulatedArtifact` → `_world.AddArtifact(a)` ก่อน broadcast
2. `HandleBuildArtifact` → `_world.SetArtifactBuildingState(id, Built)` ให้คนเข้าทีหลังเห็นสถานะถูก
3. `ServerWorld.AddPlayer()` ยิง `SnapshotArtifacts()` ทั้งชุดให้คนใหม่ก่อนส่ง `AppearPlayer`
4. `HandleDestructArtifact` ตรวจ 2 ชั้นก่อนทุบ — มีของจริงไหม + เป็นเจ้าของไหม

### `CanModifyArtifact(artifact)` ✅ ใหม่
`FounderEntityId == EntityId` หรืออยู่ใน `ArchitectEntityIds` → ทุบได้
ไม่ใช่ → log แล้วตอบ `Abort` (เดิมส่ง entityId อะไรมาก็ทุบได้ รวมบ้านคนอื่น)

**ที่ยังค้าง:** ยังไม่เขียนลงดิสก์ (GP-07) — รีสตาร์ทเซิร์ฟบ้านยังหายอยู่
