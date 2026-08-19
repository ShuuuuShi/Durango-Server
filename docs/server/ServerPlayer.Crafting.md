# `ServerCore/ServerPlayer.Crafting.cs`

**หน้าที่:** คราฟต์ไอเทม — มี 2 เมทอด

## `ValidateMaterials(slots, materials, out itemIds, out reason)` (GP-08)

ตรวจว่าวัตถุดิบที่ client ส่งมาใช้ได้จริงไหม คืนรายการ item id ที่จะหักตอนคราฟต์เสร็จ
`materials` คือ `Dictionary<slot_id, item id[]>` (ฝั่ง client ใส่คีย์เป็น `recipeSlot.Id` — ดู `client/CraftSystem.cs`)

ปฏิเสธเมื่อ:

| กรณี | เหตุผล |
|---|---|
| ชื่อช่องไม่มีในสูตร | กันยัดช่องมั่วให้ผ่านการนับ |
| จำนวนในช่อง < `count_min` หรือ > `count_max` | ตามข้อมูลสูตรจริงของเกม |
| ไอเทมชิ้นเดียวใส่หลายช่อง | ก้อนหิน 1 ก้อนจ่ายทั้งสูตรไม่ได้ |
| item id ไม่มีในกระเป๋า | เดิมข้ามเงียบ ๆ = คราฟต์ลม |
| ไอเทมสวมอยู่บนตัว | ต้องถอดก่อน |

**ยังไม่ตรวจ tag ของวัตถุดิบ** (เช่นช่อง `main` ต้องเป็น `blade_tool`) เพราะไอเทมที่ server รุ่นนี้สร้าง
ยังไม่มี `Tags` ติดตัวเลย ถ้าเปิดตรวจตอนนี้จะคราฟต์ไม่ได้สักสูตร — ข้อมูล tag อยู่ใน
`RecipeRequirements.Slot.Tags`/`.Materials` พร้อมใช้เมื่อไอเทมมี tag แล้ว

## `HandleCraft(msg, header)`

1. หาชื่อ+ไอคอนจาก `RecipeData.RecipeInfo[msg.RecipeId]` ถ้าไม่เจอใช้ `RecipeId` เป็นชื่อไปเลย
2. **GP-08:** `RecipeRequirements.TryGet()` — ไม่มีสูตรนี้ในเกม = `Abort`
3. **GP-08:** `ValidateMaterials()` ไม่ผ่าน = `Abort` พร้อม log เหตุผล
4. กระเป๋าเต็ม **และไม่มีวัตถุดิบที่จะหัก** = `Abort` (ถ้ามีของจะถูกหัก n ชิ้นแล้วเพิ่ม 1 ชิ้น ยังพอ)
5. `TrySpendStamina(StaminaCostCraft)` ไม่พอ = `Abort`
6. ตอบ `Timer { Duration = 2f }` → client เล่นอนิเมชันคราฟต์
7. สร้าง `Item` ผลลัพธ์ไว้ล่วงหน้า (Level 1, durability เต็ม)
8. เข้าคิว `_deferred` ที่ +2.1 วินาที พอถึงเวลา:
   - หาตำแหน่งของวัตถุดิบทุกชิ้นก่อน — **ขาดชิ้นใดชิ้นหนึ่ง = ยกเลิกทั้งหมด** แล้วตอบ `Abort`
     (ระหว่าง 2.1 วินาทีนั้นผู้เล่นอาจเอาของไปใส่กล่อง/ให้คนอื่น)
   - ลบจากท้ายไปหน้าเพื่อไม่ให้ index เลื่อน แล้วใส่ของที่คราฟต์ได้ลงกระเป๋า
   - ส่ง `Crafted` (พร้อม `ActionInfo`) + `SendInventory()`

## ผลลัพธ์ยังหยาบอยู่

ของที่คราฟต์ได้ **Level 1 เสมอ**, `SuccessRatio = 1f` เสมอ, ไม่มี `Tags` / `Performance`
แปลว่าคุณภาพของ ระดับสกิล และ tag ที่ควรมีผลต่อของ ยังไม่ได้ทำ
(และเป็นเหตุผลที่ยังตรวจ tag วัตถุดิบไม่ได้ — ของที่คราฟต์เองก็ไม่มี tag ไปใช้ต่อ)

## ข้อมูลสูตร

`ServerCore/RecipeRequirements.cs` — generated จาก `scripts/extract_recipes.py`
(720 สูตร / 1,756 ช่อง) **อย่าแก้มือ** ดู [Data.md](Data.md)
