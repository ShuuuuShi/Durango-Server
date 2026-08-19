# ระบบต่อสู้ (เฟส C รอบ 2)

**ไฟล์:** `ServerCore/ServerPlayer.Combat.cs` · `ServerCore/ActionData.cs` · ส่วน AI ใน `ServerCore/AnimalSpawner.cs`
· เมนูตอนแตะสัตว์/แล่เนื้อใน `ServerCore/ServerPlayer.Gathering.cs` + `ServerCore/ButcheryData.cs`

## แตะสัตว์แล้วปุ่มโจมตีต้องขึ้น

เมนูวงกลมที่โผล่ตอนแตะอะไรสักอย่าง **มาจาก `Touched.Interactions` ของ server ล้วน ๆ**
client แค่แปลงเลขเป็นปุ่ม (`client/InteractionData/Interaction.cs` — `Attack = 1`, `Collect = 506`)

```
client → Touch {EntityId=<id สัตว์>, EntityType=2000-2999, Tile=(-1,-1)}
              ↑ สัตว์ส่ง Tile เป็น (-1,-1) เสมอ (client/InteractionObject.cs → Tile)
server → Touched {EntityName, Level, Interactions=[1]}                   ← ยังไม่ตาย
server → Touched {EntityName, Level, Collectible{Generators=[...]}}      ← ซาก (เมนูแล่เนื้อ)
```

🐛 **บั๊กที่เจอตอนเล่นจริง (15 ส.ค.):** แตะสัตว์แล้วปุ่มโจมตีไม่ขึ้นเลย
เพราะ `HandleTouch` มีแค่เคส "ของธรรมชาติ (type ≥ 10000)" กับ "สิ่งปลูกสร้าง"
สัตว์ตกไปทางของธรรมชาติแล้วตอบ `Touched` ที่ `Interactions` ว่าง = เมนูเปล่า
แถม `EntityId` ที่ตอบกลับถูกเขียนทับเป็น `natural_-1_-1` ด้วย

## วงจรเต็ม

```
client → UseBattleAction {ActionId, StartAt, TargetEntityId, TargetTile}
server   ตรวจ: ตายอยู่? · มีท่านี้จริง? · ท่านี้ใช้กับอาวุธที่ถืออยู่ได้? · คูลดาวน์? · เป้าอยู่ในระยะ? · สตามินาพอ?
server → BattleBegun (เฉพาะครั้งแรกที่เปลี่ยนเป้า — client ใช้เข้าโหมดต่อสู้)
server → OK
         ...หน่วงตาม attack_time ของท่า...
server → Damaged {AttackerId, VictimId, Damage{Value,Result,Effects}, EventAt}   broadcast ทุกคน
         ถ้าเลือดหมด → EntityDied broadcast
```

อ้างอิงลำดับจาก offline server เดิมที่ฝังอยู่ใน client (`client/Durango.Offline/Player.cs → OnUseBattleAction`)
ต่างกันตรงที่ **ดาเมจคิดที่ server** — ของเดิม client เป็นคนคิดแล้ว push เข้า connection ตัวเอง

## ข้อมูลท่า — `ActionData.cs` (generated)

```bash
python scripts/extract_actions.py game/DurangoV2_Data/resources.strings.txt ServerCore/ActionData.cs
```
สกัดจาก TextAsset `player_battle_actions` (59 ท่า) + `tag_allow_actions` (อาวุธ 10 แบบ)

| ฟิลด์ | มาจาก | ใช้ทำอะไร |
|---|---|---|
| `Stamina` | `meta.stamina` | หักสตามินา (ขั้นต่ำ 3 เพราะท่าพื้นฐานในเกมเป็น 0 = ตีรัวฟรี) |
| `Cooltime` | `meta.cooltime` | คูลดาวน์ต่อท่า (server จำเอง ไม่เชื่อ client) |
| `UseRange` / `Radius` | `meta.use_range` / `attack_info.radius` | ระยะที่ตีถึง (+ slack 400 เพราะ server รู้แค่ปลายทางของ Move ล่าสุด) |
| `AttackTime` | `attack_info.attack_time` | ดาเมจเข้าหลังกดกี่วินาที |
| `DamageBonus` / `Impact,Pierce,Cut` | `attack_info` | ตัวคูณดาเมจ |

`WeaponActions[tag]` = ท่าที่ใช้ได้ของอาวุธแต่ละแบบ (`bare_hands` 8 ท่า, `onehand`, `twohand`, `bow` ...)
tag มาจาก `EquipData.Weapons[prototype].Framework` ของของที่ใส่ในช่อง `main`

## สูตรดาเมจ

```
atk    = 6 + เลเวล*0.3           (+10 ถ้าถืออาวุธจริง)
damage = atk × DamageBonus × (Impact+Pierce+Cut)
       × สุ่ม 0.85–1.15
       × 1.6 ถ้าคริ (โอกาส 12%)
```
ยังหยาบอยู่ตั้งใจ — ค่าพลังรายชิ้นของอาวุธ/เกราะยังไม่มี (ต้องรอ `Tags`/`Performance` ของไอเทม
ซึ่งเป็นตัวเดียวกับที่ GP-08b รออยู่) ดาเมจของสัตว์คือ `3 + เลเวลสัตว์ × 0.6`

## ตาย / ฟื้น

| ใคร | เกิดอะไร |
|---|---|
| ผู้เล่น | `Die()` → broadcast `EntityDied` · ตั้ง `Dead` → `Touch`/`Collect`/`Craft`/`UseBattleAction` ตอบ `Abort` ทั้งหมด |
| ผู้เล่นสั่ง `Revive` | เลือด/สตามินาเต็ม (ความล้าไม่ล้าง) · วาร์ปกลับจุดเกิด · ตอบ `Revived` + broadcast `EntityRevived` |
| สัตว์ | broadcast `EntityDied` ทันที · ซากอยู่ 20 วิ แล้ว `DisappearEntity` · เกิดตัวใหม่ในอีก 45 วิ |

`EntityDied`/`EntityRevived` ฝั่ง client ไปเข้า `ObjectManager.SetEntityAlive()` ซึ่งใช้ฟิลด์ `At`
เป็นเวลาที่จะเล่นอนิเมชัน — ต้องส่งเวลา server ปัจจุบัน ไม่ใช่ 0

**cheat ที่ใช้เทส:** `die` (ตายทันที) · `hurt` (30 ดาเมจ) · `rest` (ฟื้นเต็ม)

## AI สัตว์ตอนโดนตี — `AnimalSpawner.ProcessAi()`

```
โดนตี → จำผู้โจมตีไว้ 20 วินาที (โดนซ้ำ = ต่ออายุ ไม่รีเซ็ตจังหวะตี)
        + ล้างเวลาพัก (NextMoveAt = now) แล้วตั้งสวนกลับครั้งแรกใน 0.5 วินาที
  ตัวขี้ตกใจ (กิ้งก่า/คอมป์โซ/โดโด/เฟนาโค/สเตโก) → วิ่งหนีออกจากผู้เล่นทีละก้าว
  ตัวอื่น                                         → เดินเข้าหาจนถึงระยะ 300 แล้วกัดตามคูลดาวน์ของชนิดนั้น
ไกลเกิน 4000 หรือหมดเวลา → เลิกสนใจ กลับไปเดินสุ่ม
```

**คูลดาวน์การกัด** ใช้ `attack_cooltime` **ค่าจริงจากข้อมูลเกม** เก็บไว้ในคอลัมน์สุดท้ายของ
`SpawnTable.Entries` (1.3–3.0 วิ แล้วแต่ชนิด) ไม่ใช่ค่าคงที่ 2.5 วิเหมือนเดิม

🐛 **บั๊กที่เจอตอนเทส (รอบแรก):** สัตว์ไม่เคยตีกลับเลย เพราะ `OnAttacked` สร้าง state ใหม่ทุกหมัด
ทำให้ `NextAttackAt` ถูกเลื่อนออกไปเรื่อย ๆ — แก้โดยถ้าโกรธคนเดิมอยู่แล้วให้ต่ออายุอย่างเดียว

🐛 **บั๊กที่เจอตอนเล่นจริง (15 ส.ค.): "ตีแล้วมันสวนกลับช้าเกินไป"** — สองสาเหตุซ้อนกัน
1. ทั้งการไล่และการหนีใน `ProcessAi` ติดเงื่อนไข `now >= NextMoveAt` ซึ่งตอนโดนตีมักเป็น
   ช่วง "พักหลังเดินถึงที่หมาย" ที่ยาวได้ถึง **14 วินาที** ⇒ สัตว์ยืนนิ่งรอหมดเวลาพักก่อนค่อยขยับ
2. การกัดครั้งแรกใช้คูลดาวน์เต็ม (2.5 วิ) ทั้งที่ควรตอบสนองทันที

วัดจริงหลังแก้ (`--console ... --cmd "cheat spawn 2017; wait 6; attack near"`):
```
[ดาเมจ 19:59:40.0] เรา → animal_f9a27… 5 (Hit)
[ดาเมจ 19:59:40.5] animal_f9a27… → เรา 4 (Hit)   ← สวนกลับใน 0.5 วิ
[ดาเมจ 19:59:41.8] animal_f9a27… → เรา 4 (Hit)   ← จากนั้นทุก 1.3 วิ = attack_cooltime จริงของโปรโตเซราท็อปส์
```

เดินเข้าหาทีละก้าว (ก้าวละ ~1 วินาที) เพราะ `MakeMove` ตั้งตำแหน่งฝั่ง server เป็นปลายทางทันที
ถ้าสั่งเดินไกล ๆ ทีเดียวแล้วสั่งใหม่ทับ ตัวจะกระตุกฝั่ง client

## แล่เนื้อ (butchery)

**ไม่มีเมนู "แล่" แยกในเกม** — มันคือ `Collectible` ชุดเดียวกับการเก็บของธรรมชาติ
ต่างกันแค่เจ้าของ generator เป็นซากสัตว์แทนที่จะเป็น tile

```
สัตว์ตาย → server สร้าง generator ของซาก (ButcheryData) เก็บไว้ที่ world ด้วย key = id ของสัตว์
         → broadcast CollectibleDisplay {DistributableEntities=[คนที่ฆ่า]}
            (client เอาไปเปิดขอบเรืองแสงรอบซาก — AnimalBehavior.IsLootable)
แตะซาก   → Touched {Collectible{Generators=[เนื้อ, หนัง, กระดูก...]}}
เลือกชิ้น → Collect {EntityId=<id สัตว์>, GeneratorId="meat"} → Timer → Collected
แล่หมดตัว → ซากหายจากโลกทันที (DisappearEntity) ไม่ต้องรอครบเวลา
```

| เรื่อง | ค่า | ทำไม |
|---|---|---|
| ซากอยู่ในโลก | 150 วินาที | ตัวใหญ่มี 8-9 หน่วยให้แล่ (~30 วิ) + เวลาเดินไปหา — เดิม 30 วิสั้นจนซากหายคาตา |
| ระยะที่แล่ได้ | 8 tile จากตัวซาก | คิดจากตำแหน่งซากตรง ๆ เพราะซากไม่ได้ผูกกับ tile |
| ใครแล่ได้ | ทุกคน | ไฟเรืองแสงให้เฉพาะคนฆ่า แต่ server ไม่ห้ามคนอื่น — เล่นด้วยกันจะได้ช่วยกันเก็บ |
| เครื่องมือ | มือเปล่า | ของจริงต้องใช้มีดแล่ รอ `Tags` ของไอเทม (ตัวเดียวกับ GP-08b) |

`ServerWorld.TryReserveCorpsePart()` แยกจาก `TryReserveGenerator()` เพราะซาก
**หมดทีละชิ้นส่วน** (เนื้อหมดแล้วยังแล่หนังต่อได้) ต่างจากต้นไม้ที่หมดทีเดียวทั้งต้น

**`ButcheryData.cs`** — รหัส generator (`meat` `leather_raw` `bone_leg` ...) และไอคอนเป็น**ของจริงจากเกม**
แต่ **จำนวน/เวลาตั้งเอง** ตาม `size_level` เพราะตารางดรอปจริงอยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client
(ตัวใหญ่ size 4 ได้เนื้อ 4 ชิ้น · ทุก 5 เลเวลได้เพิ่มชนิดละ 1)

## ที่ยังไม่ได้ทำ

- ป้องกัน/หลบ/สวนกลับ (`DamageResult` ส่ง `Hit` อย่างเดียว) · groggy · knockback
- แล่เนื้อยังไม่ต้องใช้มีด และยังไม่มีสกิล butchery มาคูณจำนวน/ความเร็ว
- สัตว์ไม่ตีกันเอง (ตัวนิสัย `Aggressive` ไล่กัด**คน**ได้แล้วผ่าน `LookForPrey`)
- ตายแล้วยังไม่เสียของ/ค่าประสบการณ์ · ยังไม่มี `ReviveImmediately` / CPR ของเพื่อน
- ดาเมจไม่คิดเกราะที่ใส่ (ยังไม่มีค่าป้องกันรายชิ้น)

## เทส

```bash
# บอทคอนโซล: ตีจนตาย / ตาย / ฟื้น
cd test-client
dotnet run --no-build -- --console 127.0.0.1 8191 bot --cmd "kill near 40; status"
dotnet run --no-build -- --console 127.0.0.1 8191 bot --cmd "cheat die; wait 2; status; revive; status"

# วัดว่าโดนตีแล้วสัตว์สวนกลับเร็วแค่ไหน (บรรทัด [ดาเมจ] มีเวลาให้อ่าน)
dotnet run --no-build -- --console 127.0.0.1 8191 bot --cmd "cheat spawn 2017; wait 6; attack near; wait 10"

# แตะสัตว์/แล่เนื้อ อยู่ใน --gp-check หัวข้อ "แตะสัตว์ / แล่เนื้อ" (5 ข้อ)
dotnet run --no-build -- --gp-check
```

**cheat ที่เพิ่มมาเพื่อเทสการแล่:** `kill animal` — ฆ่าสัตว์ตัวที่ใกล้ที่สุดทันที
(ใช้คู่กับ `spawn <ชนิด>` จะได้ไม่ต้องยืนตีเป็นนาทีกว่าจะได้ซาก)

ดู [FarmBot.md](FarmBot.md) สำหรับคำสั่งทั้งหมดของบอทคอนโซล
