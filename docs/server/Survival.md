# ค่าสถานะเอาชีวิตรอด (เฟส C)

ไฟล์: `ServerCore/ServerPlayer.Survival.cs`

เดิม `MakeAppearPlayer()` ส่ง `Life = Gauge(1, 0, [(0, 1)])` ค่าเดียวตายตัว ไม่มีการลด
ตอนนี้มี **เลือด · สตามินา · ความล้า** ที่เปลี่ยนตามเวลาและการกระทำจริง

---

## กลไก Gauge — จุดที่ต้องเข้าใจก่อนแก้อะไร

`Gauge` ของเกมนี้ **ไม่ใช่ตัวเลข** แต่เป็น **keyframe ที่ client ลากเส้นเอง**

```csharp
new Gauge(max, min, new[] {
    new GaugeNode { Time = ตอนนี้,      Value = 94 },
    new GaugeNode { Time = ตอนนี้ + 1.5, Value = 100 },
})
```
client เอาเวลาปัจจุบันมา interpolate ระหว่างสองจุด → หลอดขยับเองลื่น ๆ
ก่อนจุดแรกใช้ค่าจุดแรก หลังจุดสุดท้ายใช้ค่าจุดสุดท้าย (`Gauge.CurrentValueAndVelocity`)

**⇒ server ไม่ต้อง tick ค่าสถานะทุกเฟรม** ส่งใหม่เฉพาะตอน *อัตราเปลี่ยน* หรือ *ค่ากระโดด* เท่านั้น
(เก็บของ / โดนตี / พัก) ระหว่างนั้น client คำนวณเองได้ถูกต้อง

นี่คือเหตุผลที่ `GaugeState` เก็บแค่ `(Value, Velocity, Max, UpdatedAt)` แล้วคำนวณ `ValueAt(now)` เมื่อต้องใช้

---

## `GaugeState` (nested class)

| สมาชิก | ทำอะไร |
|---|---|
| `ValueAt(now)` | ค่า ณ เวลานั้น (clamp 0..Max) |
| `Settle(now)` | ตรึงค่าปัจจุบันไว้ก่อนเปลี่ยน velocity — **ต้องเรียกก่อนแก้ `Value` เสมอ** ไม่งั้นค่าจะกระโดด |
| `ToGauge(now)` | แปลงเป็น `Gauge` — 1 จุดถ้านิ่ง, 2 จุดถ้ากำลังเปลี่ยน โดยจุดที่สองคือตอน**ชนขอบพอดี** (0 หรือ Max) client จะได้ไม่วาดทะลุ |

---

## ค่าตั้งต้น (แก้ที่หัวไฟล์ที่เดียว)

| ค่า | ตัวเลข | หมายเหตุ |
|---|---|---|
| `LifeMax` | 100 | |
| `LifeRegenPerSec` | 0.5 | เต็มจาก 0 ใน ~3.3 นาที (เริ่มฟื้นหลังโดนตี) |
| `StaminaMax` | 100 | |
| `StaminaRegenPerSec` | 4 | เต็มจาก 0 ใน 25 วินาที |
| `FatigueMax` | 100 | |
| `FatiguePerSec` | 100/3600 | เต็มใน 1 ชั่วโมง |
| `FatigueCaution` | 60 | เกินนี้ค่าใช้จ่ายสตามินา ×1.5 |
| `FatigueDanger` | 85 | เกินนี้ ×2 |

**ค่าใช้จ่ายสตามินาต่อการกระทำ:** เก็บของ 6 · คราฟต์ 4 · ก่อสร้าง 8

---

## เมทอด

### `TrySpendStamina(cost)`
คูณ cost ตามความล้าก่อน → `Settle()` → ไม่พอคืน `false` (ผู้เรียกต้องตอบ `Abort`)
พอแล้วหักออกแล้วตั้ง `Velocity = StaminaRegenPerSec` (ใช้เสร็จเริ่มฟื้นทันที) แล้ว `PushGauges("stamina")`

เรียกจาก 3 ที่: `HandleCollect` · `HandleCraft` · `HandleOccupyArtifactSite`

### `ApplyDamage(amount)`
ลดเลือด คืน `true` ถ้าตาย — เตรียมไว้ให้ระบบต่อสู้ ตอนนี้เรียกจาก cheat `hurt` เท่านั้น

### `RestoreSurvival(clearFatigue)`
ฟื้นเลือด+สตามินาเต็ม, `clearFatigue: true` ล้างความล้าด้วย

### `PushGauges(params keys)`
ส่ง `SurvivalUpdated` เฉพาะ gauge ที่เปลี่ยน
- ถ้ามี `life` จะ **broadcast ให้คนอื่นด้วย** (คนอื่นต้องเห็นหลอดเลือดเรา) — สตามินา/ความล้าเป็นเรื่องส่วนตัว ไม่ต้องส่ง
- ⚠️ `Removed` ต้องเป็น **array ว่าง ห้าม null** — client วน `msg.Removed.Length` ตรง ๆ ไม่เช็ค null

---

## ที่ client อ่านค่าพวกนี้

`PlayerHudGroupBase.cs`:
```csharp
SetLife(character.GetGauge("life"));
Gauge gauge = character.GetGauge("stamina");
SetEnergy((gauge == null) ? character.GetGauge("energy") : gauge);
```
`FatigueSystem.cs` อ่าน `"fatigue"` + เกณฑ์จาก `Derived.FatigueCaution` / `FatigueDanger`

⇒ key ที่ต้องส่งคือ **`life` · `stamina` · `fatigue`** เท่านั้น (ชื่ออื่นส่งไปก็ไม่มีใครอ่าน)

`SendStatistics()` เพิ่ม `LifeMax` `StaminaMax` `FatigueMax` `FatigueCaution` `FatigueDanger` ให้ HUD คำนวณหลอดถูก

---

## เซฟ

`SurvivalSave { Life, Stamina, Fatigue }` อยู่ในไฟล์ผู้เล่น

ตอนโหลดกลับ (`ApplySurvivalSave`) จงใจให้:
- **เลือด** = ค่าที่เซฟไว้ (ตายมาก็ยังเลือดน้อย)
- **สตามินา** = เต็มเสมอ (ถือว่าออกเกมไปแล้วได้พัก)
- **ความล้า** = ค่าที่เซฟไว้ (ออกเกมไม่ได้ช่วยล้างความล้า ต้องพักในเกม)

---

## cheat ทดสอบ

| คำสั่ง | ทำอะไร |
|---|---|
| `survival` | โชว์ค่าปัจจุบันทั้ง 3 ตัว |
| `rest` | ฟื้นทุกอย่าง ล้างความล้า |
| `tired` | ตั้งสตามินาเป็น 0 |
| `hurt` | ลดเลือด 30 |
| `exhaust` | ตั้งความล้า 90 (เกิน danger) |

---

## ผลทดสอบ

`test-client` ข้อ 19–23:

| ทำอะไร | ผลที่ได้ |
|---|---|
| ค่าเริ่มต้น | `เลือด 100/100 · สตามินา 100/100 · ความล้า 1/100` |
| เก็บของ | `stamina=94/100 (เพิ่ม→100 ใน 10 วิ)` — หัก 6 แล้วเส้นฟื้นถูกต้อง |
| สตามินา 0 แล้วเก็บของ | **`Abort`** + log `สตามินาไม่พอสำหรับเก็บของ` |
| `hurt` | `life=70/100 (เพิ่ม→75 ใน 10 วิ)` — ฟื้น 0.5/วิ ตรงตามตั้ง |
| `rest` | `life=100 (นิ่ง) stamina=100 (นิ่ง) fatigue=0` |
| เซฟ | `{"Life":100,"Stamina":100,"Fatigue":0.035}` |

> ตอนเทสรอบแรกเคสสตามินาไม่พอ **ไม่ได้ทดสอบจริง** — ตั้งไว้ที่ 3 แล้วรอ 700ms
> สตามินาฟื้น 4/วิ กลับไปเกิน 6 ก่อนคำสั่งถึง เลยผ่านไปได้ ต้องตั้งเป็น 0 แล้วยิงทันทีถึงจะเจอ

---

## ที่ยังไม่ได้ทำ

- **ตายแล้วยังไม่มีอะไรเกิดขึ้น** — `ApplyDamage` คืน `true` ได้แต่ยังไม่มีใครเรียก
  ยังไม่มี `Revive` / `Resurrect` handler ต้องทำพร้อมระบบต่อสู้
- **ไม่มีความหิว/กระหาย** — client HUD ไม่ได้อ่าน key พวกนี้ เลยยังไม่ทำ
- **กินอาหารยังไม่ฟื้นค่าอะไร** — `UseItem` ยังไม่ผูกกับ survival
- **ความล้าไม่มีผลอย่างอื่น** นอกจากทำให้สตามินาแพงขึ้น (ของจริงมีผลกับ biome/สภาพอากาศด้วย)
- **`GetStatusEffects` / status effect** ยังไม่มี handler

## แก้ไข 26 ส.ค. 2026 — พักที่ Shelter ทุกชนิด + ไอคอนบัพ

- จุดพักยึดจาก `RecipeData.BlueprintComponents[blueprintId]` ที่มี component `Shelter` เหมือนเกณฑ์ที่ client ใช้เพิ่ม `Interaction.Rest` ไม่เดาจากชื่อ blueprint อีกต่อไป
- ครอบคลุมกองไฟ เต็นท์ เก้าอี้ โซฟา เตียง เสื่อ สระ/เฟอร์นิเจอร์ และจุดพักอื่นที่มี `Shelter` ในข้อมูลเกม
- เริ่มพักเปิด `away_from_keyboard` ผ่าน `Messages.StatusEffects`; หยุดพักปิด effect และส่ง packet ใหม่
- `Move` ที่เป็น jitter/snap เข้า attachment ไม่ทำให้พักหลุด; หยุดเฉพาะการขยับจริงเกิน 10 world units
- `RestFatiguePerSec=4` ทำงานจริงและถูกตรวจด้วย `test-client/StaminaCheck.cs`
- ผลล่าสุด: `--stamina-check` **19/19** ผ่าน (fatigue ลด, buff เปิด, buff ปิดเมื่อหยุดพัก)
