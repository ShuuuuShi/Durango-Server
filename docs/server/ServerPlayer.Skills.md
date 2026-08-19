# `ServerCore/ServerPlayer.Skills.cs`

**หน้าที่:** เรียน/ลืมสกิล และส่งค่าสถานะตัวละคร

## `HandleLearnSkill(msg, header)` — บรรทัด 37

1. `_skillPoints <= 0` → `Abort` (เริ่มเกมมี **777 แต้ม** ตั้งไว้ใน `ServerPlayer.Core.cs:51`)
2. หัก 1 แต้ม
3. หา category จาก `SkillData.SkillCategory[msg.SkillId]` ไม่เจอใช้ `0`
4. มีสกิลนี้อยู่แล้ว → อัปเดต `Levels[SubId]`, ยังไม่มี → เพิ่ม `SkillBundle` ใหม่
   (`SubId` ที่เป็น null จะถูกแทนด้วย `"__base__"` เพื่อใช้เป็น key ได้)
5. ตอบ `OK` + `SendSkills()`

⚠️ หัก **1 แต้มต่อ 1 ครั้งเสมอ** ไม่ว่าจะเรียนสกิลระดับไหน และไม่ตรวจว่าสกิลนั้นมีอยู่จริงไหม
หรือมีสกิลก่อนหน้าที่ต้องเรียนก่อนไหม (prerequisite)

## `HandleUntrainSkill(msg, header)` — บรรทัด 71

หาแล้วลบทั้ง `SkillBundle` + คืน 1 แต้ม → `OK` + `SendSkills()`

⚠️ ลบทั้ง bundle แม้สกิลนั้นจะมีหลาย `SubId` แต่คืนแค่ 1 แต้ม → ใครที่เรียนหลาย SubId ในสกิลเดียวจะ **เสียแต้มฟรี**
และเพราะไม่ตรวจอะไร กด untrain สกิลที่ไม่มีก็ไม่คืนแต้ม (`index < 0` ข้ามไป) แต่ยังตอบ `OK`

## `SendSkills()` — บรรทัด 84

ส่ง `Skills` — รายการสกิลที่รู้ + แต้มคงเหลือ + `Categories`
สังเกต `SkillList = _knownSkills.Count == 0 ? null : ...` — ส่ง `null` แทน array ว่าง เพราะ client บางจอเช็ค null

## `SendStatistics()` — บรรทัด 97

ส่งค่าสถานะ **ตายตัวทั้งหมด**:
- `BasicAbilities` ทั้ง 8 ตัว (Strength, Charisma, Dexterity, Agility, Endurance, Will, Intelligence, Perception) = **20 เท่ากันหมด**
- `DerivedsAbilities`: Swimming / Gathering / Handicraft / MaxHealth = **100**
- `Level` = ค่าจริงของผู้เล่น, `Exp` = 0
- `ResistanceLevels` / `Modifiers` / `RepresentPowers` = null

แปลว่าตอนนี้ **ค่าสถานะไม่มีผลอะไรกับเกมเลย** ทุกคนเก่งเท่ากันหมด และไม่มีระบบ EXP
เป็น stub ให้ UI มีเลขโชว์ ถ้าจะทำจริงต้องผูกกับสกิลที่เรียน + อุปกรณ์ที่ใส่
