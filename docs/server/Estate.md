# ระบบที่ดิน (Estate) — `ServerCore/EstateManager.cs`

ที่ดินส่วนตัว (사유지) บนเกาะเสถียร · ประกาศ 4×4 · ขยายทีละช่อง · สิทธิ์เพื่อน/คนนอก · วาร์ปบ้าน
เก็บใน **`saves/world.json`** ไม่ใช่เซฟผู้เล่น (ช่องที่ดินทับกันข้ามตัวละครได้ ต้องดูทั้งโลกพร้อมกัน)

เปิด/ปิดด้วย `Features.LandPermission` ใน `data/config.json` (ตอนนี้ **เปิด**)
ปิดอยู่ = ทุก packet ของที่ดินตอบ `Info { "ระบบสิทธิ์ที่ดินยังไม่เปิดใช้งาน" }`

---

## หน่วยที่ใช้ — อ่านก่อน ไม่งั้นงงแน่

| หน่วย | ขนาด | ที่ใช้ |
|---|---|---|
| tile | 200 world unit | ตำแหน่งตัวละคร/สิ่งปลูกสร้าง |
| **estate unit (ช่อง)** | 4×4 tile | `EstateRecord.Cells`, `EstateGrids.Cells`, `DeclareEstate.Cell` |
| chunk | 16×16 tile = 4×4 ช่อง | `EstateGrids.Chunks` |

`ToEstateUnit()` แปลง tile → ช่องให้อัตโนมัติ **โดยเดาจากค่า**: ถ้า x หรือ y > `MaxEstateUnit` (63)
ถือว่าเป็น tile แล้วหารสี่ ⇒ พิกัดช่อง 0-63 กับ tile 0-63 แยกกันไม่ออก (แมพกว้าง 256 tile = 64 ช่องพอดี
เลยยังไม่เป็นปัญหาจริง แต่ถ้าขยายแมพต้องแก้)

| ค่าคงที่ | ค่า | ความหมาย |
|---|---|---|
| `InitialSide` | 4 | ด้านของแปลงตอนประกาศ |
| `InitialCells` | 16 | จำนวนช่องตอนประกาศ (= หน่วยของ `EstateLicense.Size`) |
| `MaxCells` | 64 | **เพดานต่อแปลง** — เท่ากับ 8×8 ถ้าขยายเป็นสี่เหลี่ยม |
| `UpkeepDays` | 7 | ค่าดูแลต่อครั้ง |

---

## Packet ที่รองรับ (`ServerPlayer.Systems.cs`)

| ขาเข้า | ตอบกลับ | เงื่อนไข |
|---|---|---|
| `GetEstateLicenses` | `EstateLicenses` | คืนแปลงของตัวเอง + `LargestPersonalEstateSize` (หน่วยช่อง) |
| `DeclareEstate` | `EstateLicense` + broadcast `EstateGrids` | มีแปลงอยู่แล้วไม่ได้ · ทับแปลงคนอื่นไม่ได้ |
| `ExpandEstate` | `EstateLicense` + broadcast | ต้องติดกับแปลง (Manhattan = 1) · ไม่ทับคนอื่น · ไม่เกิน 64 ช่อง |
| `ShrinkEstate` | `EstateLicense` + broadcast | เล็กกว่า 16 ช่องไม่ได้ |
| `SetEstateLicense` | `OK` | เจ้าของเท่านั้น |
| `ExtendEstateActivation` | `EstateLicense` | ต่ออายุ +7 วันจากวันหมด (หรือจากวันนี้ถ้าหมดไปแล้ว) |
| `RemoveEstate` | `OK` + broadcast | เจ้าของเท่านั้น |
| `ReturnToEstate` | `OK` + วาร์ป | ไปกลางช่องมุมของแปลง |
| `VisitEstate` | `OK` + วาร์ป | ไปแปลงของคนที่ระบุ |
| `GetEstateLicenseById` / `GetClanEstateLicense` | `EstateLicense` | เผ่า = ยังไม่มี ตอบใบว่าง |

`EstateGrids` ถูกส่งให้ทุกคนในโลกเมื่อมีการประกาศ/ขยาย/หด/สละ และส่งให้คนที่เพิ่งเห็น chunk ใหม่
(`ServerPlayer.Core.cs` — `BuildGrids(viewChunks)`)

---

## สิทธิ์ — บังคับใช้จริงแล้ว (แก้ 2 ก.ย. 2026)

`Shared.Estate.AccessRights` = `Enter | UseFacility | Give | Take | Occupy | Destruct` (flags)

ค่าเริ่มต้นตอนประกาศ: คนนอก = `Enter` · เพื่อน = `Enter | UseFacility | Give`

> 🐛 **บั๊กที่เจอ:** ค่าพวกนี้ถูกเก็บและส่งกลับให้ client ถูกต้อง แต่ **ไม่มีโค้ดที่ไหนเรียกใช้เลย**
> (`EstateManager.OwnsTile` ไม่ถูกอ้างถึงจากไฟล์ไหนทั้งนั้น) ⇒ ใครก็เดินเข้าไปสร้างของบนที่ดินคนอื่นได้
> ทั้งที่หน้า UI บอกว่าไม่ให้สิทธิ์

แก้โดยเพิ่ม `ServerPlayer.CanUseLand(tile, need)` + `RejectIfLandLocked(...)` แล้วเสียบที่:

| การกระทำ | สิทธิ์ที่ต้องมี | handler |
|---|---|---|
| จองที่สร้าง | `Occupy` | `HandleOccupyArtifactSite` |
| วางของสำเร็จรูป (แคปซูล) | `Occupy` | `HandlePlaceCapsulatedArtifact` |

ตรรกะ: ที่สาธารณะ (ไม่มีแปลงครอบ) หรือที่ดินตัวเอง = ผ่านเสมอ · เพื่อน (ดูจาก friend list ของผู้ขอ
ซึ่งระบบเพื่อนเพิ่มสองทางอยู่แล้ว) ใช้ชุด `Friends` · ที่เหลือใช้ชุด `Others`

**ยังไม่ต้องเสียบเพิ่ม** เพราะมีด่านเจ้าของ artifact กันอยู่แล้ว:
ทุบของ (`CanModifyArtifact`) · เปิดกล่องคนอื่น (`ServerPlayer.Storage.cs`) · ปลูก/ถอน/เก็บเกี่ยว
(`CheckFarmAccess` → `CanModifyArtifact`)

---

## เทส

```bash
# เซิร์ฟ (ต้องมี --enable-cheat เพราะตัวเทสใช้ cheat tp / cheat save)
dotnet run -- --gateway-port 8290 --game-port 8291 --radiotower-port 8292 --enable-cheat

# เฟส 1 — 39 ข้อ (ประกาศ/ขยาย/หด/สิทธิ์/ค่าดูแล/วาร์ป/สละ/broadcast)
cd test-client && dotnet run -- --estate-check 127.0.0.1 8291 8290
#   ท้ายรันจะพิมพ์ entityId ที่ทิ้งแปลงไว้ให้เทสต่อ

# เฟส 2 — รีสตาร์ทเซิร์ฟก่อน แล้วเช็คว่าที่ดินยังอยู่ (5 ข้อ)
dotnet run -- --estate-reload-check 127.0.0.1 8291 8290 <entityId ที่ได้จากเฟส 1>
```

ผลล่าสุด (2 ก.ย. 2026): **39/39 + 5/5 ผ่าน**

---

## ที่ยังไม่ได้ทำ (รู้ตัวอยู่)

- **ค่าดูแลหมดอายุแล้วไม่เกิดอะไรขึ้น** — `UpkeepUntil` ถูกเก็บ/ต่ออายุถูกต้อง แต่ไม่มี tick ที่คืน
  ที่ดินที่ปล่อยร้าง ⇒ แปลงของคนที่เลิกเล่นจะจองที่ไว้ตลอดไป (ต้องมี job เก็บกวาดก่อนเปิดยาว)
- `ExtendEstateActivation.Cost` ถูกมองข้าม — ต่ออายุฟรี (รอระบบ T-stone ตาม roadmap S3)
- เผ่า/หมู่บ้าน (`ClanEstate`, `UrbanEstate`) ยังไม่ทำ — ตอบใบว่างเสมอ
- ประกาศได้ทุกที่ที่ยังว่าง ไม่เช็คว่าเป็นน้ำ/ภูเขา/จุดสำคัญ
