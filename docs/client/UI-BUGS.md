# ผลตรวจฝั่ง client / UI — 19 ส.ค. 2026

ตรวจ `client/` (3,760 ไฟล์จาก ILSpy) เทียบกับสิ่งที่ `server/` ส่งจริง + หลักฐานจาก
`game/game1.log` · `game2.log` (log จริงจากการเล่น 13 ส.ค. · exception 58,878 บรรทัด)

> ผลตรวจนี้ **ยังไม่ได้แก้อะไร** — เป็นรายการให้ตัดสินใจว่าจะแก้อะไรก่อน

---

## 0. สามเรื่องที่ต้องรู้ก่อนอ่านรายการ

### 0.1 `null` ที่ส่งข้ามสายไม่ทำให้ client พัง — แต่ทำให้ค่าเป็น 0/ว่าง

`Pack`/`Unpack` ของทุก message **จองอาร์เรย์และ Dictionary ให้เสมอ**
(`result.TagModifications = new Tag[num2]` · `result.Modifiers = new Dictionary<string,float>(num9)`)
⇒ ฟิลด์ที่ server ส่ง `null` มาถึง client เป็น **คอลเลกชันว่าง ไม่ใช่ null**

**ผลที่ตามมา:** เลิกกังวลเรื่อง NullReferenceException จากฟิลด์ที่เราส่ง null
แต่ต้องไปกังวลว่า **UI จะโชว์ 0 หรือว่างเปล่า** แทน (ดูหัวข้อ 2)

⚠️ ข้อยกเว้นคือฟิลด์ที่เป็น **class** (`Gauge`) กับ **nullable struct** (`X?`) — สองอันนี้เป็น null ได้จริง

### 0.2 `DictionaryExtensions.Get` เช็ค null ผิดตัว

```csharp
public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default)
{
    if (key == null) return defaultValue;   // ← เช็ค key
    return (!dict.TryGetValue(key, out var value)) ? defaultValue : value;   // ← dict ไม่ได้เช็ค
}
```

ตอนนี้ไม่ระเบิดเพราะข้อ 0.1 (dict ที่มาจากสายไม่เคยเป็น null)
แต่ถ้าวันหลังมีใครสร้าง `Statistics` ขึ้นมาเองในโค้ด client แล้วไม่ได้ตั้ง dict ⇒ พังทันที
**อยู่ในไฟล์ `client/DictionaryExtensions.cs:10`** — ถ้าจะ build client ใหม่ ควรเติม `dict == null` ไปเลย (1 บรรทัด)

### 0.3 ไม่มี timeout ของ "คำตอบที่ไม่เคยมา"

`Connection` ฝั่ง client ลงทะเบียน reply handler ตาม seq แล้ว **ไม่มีกลไกหมดอายุเลย**
(ไม่มีคำว่า timeout ใน `Durango.Network/Connection.cs`)

⇒ ทุก packet ที่ server ไม่มี handler = handler ค้างใน dict ตลอดอายุ session
และ UI ที่รอคำตอบอยู่ก็ **รอตลอดไป** (ดูหัวข้อ 3)

---

## 1. บั๊กที่ยืนยันจาก log จริง

จำนวนในวงเล็บ = จำนวนครั้งที่เจอใน `game1.log`

### 🔴 1.1 กดปุ่มลัดเปิดกระเป๋าแล้ว **กระเป๋าไม่เปิด** (3 ครั้ง)

```
NullReferenceException
  at Durango.UI.InventoryGroup.SetNormalInventory ()
  at Durango.UI.InventoryGroup.Open ()
  at Durango.UI.MenuHelper.Open (MenuType type, Boolean immediately)
  at Durango.UI.MenuHelper.Toggle (...)
  at InputKeyboard...<InitShortcut>b__0 (...)      ← มาจากปุ่มลัดคีย์บอร์ด
```

**ต้นเหตุ** — `client/Durango.UI/InventoryGroup.cs:331`

```csharp
private void SetNormalInventory()
{
    VehicleBase vehicle = PlayerBehavior.LocalPlayer.Driver.Vehicle;   // ← ไม่เช็ค null สักตัว
    ...
}
```

`PlayerBehavior.LocalPlayer` เป็น **static field ธรรมดา** (`PlayerBehavior.cs:232` คืน `_localPlayer` ตรง ๆ)
⇒ **เป็น null จนกว่าตัวละครจะเกิดในโลก** · กดปุ่มลัดตอนยังโหลดอยู่ = พัง

**แก้:** `if (PlayerBehavior.LocalPlayer?.Driver != null) { ... }` แล้วค่อยทำต่อ

### 🔴 1.2 กดปุ่มลัดถ่ายภาพ → พังแบบเดียวกัน (4 ครั้ง)

`client/Durango.UI/ScreenCaptureGroup.cs:142`

```csharp
public override bool Open()
{
    bool zoomOut = !PlayerBehavior.LocalPlayer.Driver.IsHovering;   // ← แบบเดียวกันเป๊ะ
    return Open(zoomOut);
}
```

⚠️ **เมนู "ถ่ายภาพ" (`MenuType.Screenshot`) ไม่ได้อยู่ในรายการ 24 เมนูที่ซ่อน** — ผู้เล่นเจอได้จริง

### 🟠 1.3 `PlayerBehavior.LocalPlayer.` ที่ไม่เช็ค null — อีก 60+ จุด

นับเฉพาะใน `Durango.UI/` (ไม่รวม `?.` และการเช็ค null)

| ไฟล์ | จำนวนจุด | เปิดใช้ใน beta ไหม |
|---|---:|---|
| `WorldMapGroup.cs` | 9 | ใช่ (แผนที่) |
| `InteractionGroup.cs` | 7 | **ใช่ — เมนูตอนแตะของ/สัตว์** |
| `InteractionHelperList_PC.cs` | 6 | **ใช่** |
| `ContextActionGroupBase.cs` | 6 | **ใช่** |
| `CombatGroup.cs` | 6 | **ใช่ — โหมดต่อสู้** |
| `BattleActionButtons.cs` | 3 | **ใช่ — ปุ่มโจมตี** |
| `CraftGroupBase.cs` | 3 | **ใช่** |
| `MarketGroup.cs` / `EstateGroup.cs` / `ClanBaseWidget.cs` | 6/4/3 | ไม่ (ซ่อนแล้ว) |

ส่วนใหญ่ทำงานหลังตัวละครเกิดแล้วจึงไม่พังในทางปฏิบัติ
**แต่เป็นระเบิดเวลาช่วง 5-10 วินาทีแรกหลังเข้าเกม** ซึ่งเป็นช่วงที่ผู้เล่นกดมั่วที่สุด

### 🟡 1.4 ย่อ/ขยายหน้าต่างเกมแล้วมี exception (24 ครั้ง)

```
NullReferenceException
  at UIBase.Close ()
  at UIBase.CloseUI ()
  at UIBase.CloseAllUI ()
  at UIManager.OnScreenSizeChanged ()      ← เปลี่ยนขนาดหน้าต่าง
```

เกี่ยวโดยตรงกับกับดักที่จดไว้ใน HANDOFF (หน้าต่างออกมา 1010×588 แทนที่จะเป็น 1600)
`CloseAllUI()` ถูก patch ไปแล้วรอบก่อน (`[แก้เอง]` เรื่องลูป 100 รอบ) **แต่ `Close()` เองยังไม่ได้เช็ค null**

### ✅ 1.5 ที่แก้ไปแล้ว (ยืนยันว่าหายจริง)

| อาการเดิม | จำนวนใน log | สถานะ |
|---|---:|---|
| `MapIndicators.UpdateIndicators` NRE ทุกเฟรม | 19,595 | ✅ patch แล้ว |
| `MoveTrail.Update` NRE ทุกเฟรม | 19,594 | ✅ patch แล้ว |
| `FatigueGaugeScrollSprite.Update` NRE ทุกเฟรม | 19,594 | ✅ patch แล้ว |
| `TitleBarMenuGroup.RefreshTitleBarMenuList` | 20 | ✅ patch แล้ว |

**รวม 3 ตัวแรก = 58,783 จาก 58,878 บรรทัด** (99.8% ของ exception ทั้ง log)
นี่คือสาเหตุที่ log บวม 12 MB และเกมอืด — ตอนนี้หายแล้ว

---

## 2. ค่าที่ UI โชว์ผิด/โชว์ 0 (ไม่พัง แต่ผู้เล่นเห็น)

### 🔴 2.1 หน้าตัวละคร: **พลังรบ / พลังคราฟต์ / พลังเก็บของ = 0 ทั้งสามช่อง**

`client/Durango.UI/CharacterAbilityWidget.cs:84`

```csharp
float num = (statistics.HasValue ? statistics.Value.RepresentPowers.Get(_types[i], 0f) : 0f);
uILabel.text = ((int)num).ToString();
```

`_types` = ทั้ง 3 ค่าของ `RepresentType` (`CombatPower` · `CraftingPower` · `CollectingPower`)
server ส่ง `RepresentPowers = null` ⇒ มาถึงเป็น dict ว่าง ⇒ **โชว์ 0 หมดทั้งสามช่อง**

นี่คือ **ตัวเลขใหญ่ที่สุดบนหน้าตัวละคร** และตอนนี้เรามีข้อมูลครบพอจะคิดจริงแล้ว
(หลังทำระบบค่าสถานะ/พลังอาวุธ/ค่าป้องกันเสร็จ) — แก้ที่ `SendStatistics()` ฝั่ง server 3 บรรทัด

### 🟡 2.2 ช่อง "ค่าต้านทานเฉลี่ย" โชว์ 1 เสมอ

`CharacterAbilityWidget.cs:91` → `GetAverageResistanceLevel()` → `ResistanceLevels` ว่าง → คืน 1
(ช่องนี้โผล่เฉพาะตอนเลเวลเต็มเพดาน — `_isInit && Level >= MaxLevels.Player` — beta cap 20 จึงยังไม่เห็น)

### 🟡 2.3 รายการ Derived ที่หน้าตัวละครโชว์ **อ่านจากซอร์สไม่ได้**

`CharacterStatusGroup._abilityLayouts` เป็น `[SerializeField]` ⇒ กำหนดใน **prefab ของ Unity** ไม่ใช่ในโค้ด
⇒ ไม่มีทางรู้จากซอร์สว่าหน้านั้นโชว์ช่องไหนบ้าง · `GetDeriveds` คืน 0 ให้คีย์ที่ไม่ได้ส่ง

**ต้องเปิดเกมจริงแล้วถ่ายหน้าจอหน้า "능력치" มาดูว่ามีช่องไหนเป็น 0 อยู่บ้าง** แล้วค่อยเติมฝั่ง server
(ตอนนี้ส่งไป 20 คีย์แล้ว: Attack/AttackRating/Accuracy/Critical/Defense/Dodge + สายอาชีพ 7 + หลอด 6)

---

## 3. ปุ่มที่กดแล้ว "ไม่มีอะไรเกิดขึ้น" (server ไม่มี handler)

client ยิง packet แล้ว server **ทิ้งเงียบ** (packet ที่ไม่มี handler ถูก drop ที่
`GameCode/Durango.Offline/Connection.cs:351` — `TryGetValue` แล้ว `value?.Invoke`)

| ปุ่ม / การกระทำ | packet | อาการที่ผู้เล่นเจอ | เปิดใช้ใน beta |
|---|---|---|:---:|
| **ปุ่ม "วิจัย" ในหน้าสกิล** | `ResearchSkillCategory` | กดยืนยันแล้วเงียบสนิท ไม่มี error | ✅ |
| ยกเลิก/เร่งวิจัย | `Cancel…` / `Skip…` | เหมือนกัน | ✅ |
| **จัดเรียงของในกระเป๋า** | `InventoryOrder` | ลากจัดแล้วออกเกม เข้าใหม่เรียงใหม่หมด | ✅ |
| **ล็อกไอเทมกันทิ้ง** | `LockOrUnlockItems` | กดล็อกแล้วไม่ล็อกจริง | ✅ |
| สลับชุดอุปกรณ์ (preset) | `ChangeEquipSlotType` | ไม่สลับ (server ฮาร์ดโค้ด Slot1) | ✅ |
| **ซ่อมของ** | `RepairItem` | ⚠️ ดูหมายเหตุข้างล่าง | ✅ |
| ฟื้นทันที | `ReviveImmediately` | ไม่เกิดอะไร | ✅ |
| เปลี่ยนชื่อ | `Rename` | ไม่เกิดอะไร | ✅ |
| ฉายา (title) | `GetTitles` / `SelectTitle` | ไม่มีฉายาให้เลือกเลย | ✅ |
| ค่าต้านทาน | `GetResistanceExpCaps` | popup โชว์ค่าเริ่มต้น | ✅ |
| บัฟ/ดีบัฟ | `GetStatusEffects` | ไม่มีไอคอนบัฟเลย | ✅ |

ทั้งหมดนี้เป็น **fire-and-forget** (`Send(...)` เฉย ๆ ไม่ผูก `.On()`) ⇒ **UI ไม่ค้าง แค่เงียบ**

> ⚠️ **ยกเว้น `RepairItem`** — `client/RepairSystem.cs:11` ผูก `.On(Timer)` / `.On(EnergyWarning)` ไว้
> และ `RepairGroup.OnItemRepair` ถูกเรียกจากใน handler นั้นเท่านั้น
> ⇒ **ถ้ามีทางกดปุ่มซ่อมได้จริง หน้าต่างซ่อมจะค้างรอตลอดไป** ไม่มีทั้งสำเร็จและ error
> ตอนนี้ยังกดไม่ได้เพราะ server ส่ง `RepairRequirement = null` ทุกชิ้น (ปุ่มไม่โผล่)
> **แต่วันไหนทำระบบซ่อม ต้องทำ handler พร้อมกัน ไม่งั้นได้หน้าต่างค้าง**

**ข้อเสนอราคาถูก:** ตอบ `Abort` + `Info` ให้ packet เหล่านี้แทนที่จะเงียบ
(แบบเดียวกับที่ทำกับ `GetAvailableEmotions` แล้ว) ⇒ อย่างน้อยผู้เล่นรู้ว่า "ยังไม่เปิดในรอบนี้"

---

## 4. ของที่ client ทำไว้ให้แล้ว แต่ server ไม่เคยสั่งใช้

พวกนี้ไม่ใช่บั๊ก — เป็น **ของฟรีที่วางทิ้งไว้** ส่ง packet ไปก็ได้เอฟเฟกต์เลย

| ของที่ client มี | packet ที่ต้องส่ง | ตอนนี้เราทำแทนด้วยอะไร |
|---|---|---|
| ป้ายเด้ง "+exp ความชำนาญ" พร้อมหลอด | `SkillCategoryExperienced` | `Info` ข้อความล้วน (`IndicatorGroup.cs:80` รออยู่) |
| เอฟเฟกต์ขึ้นเลเวลเต็มจอ | `LevelUpEffect` | ไม่มีเลย (`AlarmGroup.cs:286` รออยู่) |
| เอฟเฟกต์หมวดสกิลขึ้นเลเวล | `CategoryLevelUpRewardEffect` | `Info` ข้อความล้วน |
| ป้ายเด้งค่าต้านทาน | `ExpGained.ResistanceType` | ส่ง null อยู่ (ยังไม่มีระบบต้านทาน) |

`ExpGained` ที่เราส่งอยู่แล้วทำงานถูกต้อง — `IndicatorGroup.OnExpGained` เด้งเลข exp ให้แล้ว

---

## 5. หมายเหตุระดับสภาพแวดล้อม (ENV-01)

`resources.assets` ของบิลด์นี้เสียบางส่วน ("is corrupted!" ตั้งแต่บูต)
⇒ **`[SerializeField]` ของ widget ไหนก็เป็น null ได้ทั้งนั้น** โดยไม่มีรูปแบบตายตัว

นี่คือสาเหตุร่วมของ patch ที่ทำไปแล้วหลายอัน (`FatigueGaugeScrollSprite` · `ExpectResultWidget` ·
`UITitleWidget_PC` ปุ่มกากบาทตาย) — **เวลาจะแตะ UI ตัวไหน ให้ถือว่า field ที่ผูกจาก prefab เป็น null ได้เสมอ**
และเขียนแบบ `if (x != null)` ทีละตัว ไม่ใช่ผูกยาวต่อกันรวดเดียว

---

## 6. ลำดับที่ควรแก้

| # | เรื่อง | ที่ | แรง |
|---|---|---|---|
| 1 | `LocalPlayer`/`Driver` null guard ใน `InventoryGroup` + `ScreenCaptureGroup` | client | ต่ำ (4 บรรทัด) |
| 2 | เติม `RepresentPowers` 3 ค่า (พลังรบ/คราฟต์/เก็บของ) | server | ต่ำ |
| 3 | ตอบ `Abort`+`Info` ให้ packet ในตาราง §3 แทนการเงียบ | server | ต่ำ-กลาง |
| 4 | `UIBase.Close()` null guard (แก้ exception ตอนย่อ/ขยายจอ) | client | ต่ำ |
| 5 | ส่ง `SkillCategoryExperienced` + `LevelUpEffect` (ได้เอฟเฟกต์ฟรี) | server | ต่ำ |
| 6 | `DictionaryExtensions.Get` เช็ค `dict == null` ด้วย | client | ต่ำ |
| 7 | เก็บ `InventoryOrder` / `LockOrUnlockItems` ลงเซฟจริง | server | กลาง |

ข้อ 1-6 รวมกันน่าจะไม่ถึงครึ่งวัน และได้ผลที่ผู้เล่นเห็นทันทีเกือบทั้งหมด

> ⚠️ ข้อที่แตะ client ต้อง **build ตัวเกมใหม่** (`เทสเกม.bat` ข้อ 18) และปิดเกมก่อน build
