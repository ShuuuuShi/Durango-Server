# วิธีสั่ง MemoryBot ให้ได้ผล

บอทไม่ใช่แชทบอท สั่งผิดแล้วยืนนิ่งหรือไปล่าเอง  
ช่องทางเดียวที่ได้ผล: **TCP `127.0.0.1:8193`** ส่ง JSON หนึ่งบรรทัดจบด้วย `\n` แล้วอ่านคำตอบ

เซิร์ฟเทสเครื่องนี้: `server.txt` = `127.0.0.1:8290` (เกม `8291`)  
**ห้าม**ยิง admin ไป `8190` — นั่นไม่ใช่เซิร์ฟที่เกมต่ออยู่

---

## 0) ห้ามทำ (ต้นเหตุที่ Claude พังงาน)

- ห้าม `give` / `maxskills` / `cheat exp` เพื่อข้ามสูตรล็อก
- ห้ามเปิด `Durango.exe` โดยไม่มี `-durango-updated` (เกตอัปเดตฆ่าโปรเซส)
- ห้ามก๊อป DLL ตอนเกมเปิดอยู่ แล้วบอกว่ามอดใหม่โหลดแล้ว — **ไม่โหลด**
- ห้ามสั่ง `bot.goal` ตอนฉาก Title
- ห้าม `bot.start` โหมด `survival`/`gather` เป็นงานหลัก — งานหลักคือเควสรายวัน + เป้าพิเศษ
- ห้ามยิง `craft.make` รัวแล้วไม่ดู `bot.status`
- ห้ามสั่งงานซ้อน: ล่า + คราฟต์ + เก็บ พร้อมกัน โดยไม่ `bot.stop` ก่อน

---

## 1) ลำดับทุกครั้งก่อนสั่ง (บังคับ)

```
1. TCP 8193 ต่อได้ไหม
2. read path=game  → main_scene=true และ ready=true ถึงจะสั่งงาน
3. read path=survival
4. read path=inventory
5. command name=bot.status   ดู phase + last_reason + goals
6. ค่อยสั่ง หนึ่งอย่าง
7. รอ 2–5 วินาที แล้วอ่าน status ซ้ำ จนกว่า last_reason จะขยับ
```

ยัง Title / `game_not_ready` = ยังไม่เข้าโลก สั่งไปก็ค้าง

---

## 2) สั่งงานยังไง

ทุกคำสั่งรูปเดียวกัน:

```json
{"request_id":"1","op":"command","name":"bot.goal","kind":"craft","entity_id":"bow_wooden_assembled","count":1}
```

อ่านสถานะ:

```json
{"request_id":"2","op":"read","path":"inventory"}
```

`op` มีสองค่า: `read` กับ `command`

### งานที่ควรถามบอทให้คิดเอง → `bot.goal`

กองงาน: ล่าง = เควสรายวัน, บน = ของที่สั่งตอนนี้

| kind | ใส่เป้าที่ | ตัวอย่าง |
|---|---|---|
| `craft` | `entity_id` = id สูตร | `bow_wooden_assembled` (ไม่ใช่ `bow_wooden_01`) |
| `gather` | `entity_id` = ชื่อ/tag ของ | `stone` |
| `hunt` | ไม่ต้องเป้า | ล่า |
| `level` | `count` = เลเวล | `count:15` |
| `skill` | `entity_id` = id สกิล | `bow_assembled` |

ธนูไม้: สูตรจริงคือ `bow_wooden_assembled`  
ปลดด้วยสกิล `bow_assembled` (หมวดทำอาวุธ เลเวลหมวด 15, 2 แต้ม)  
วัตถุดิบ: กิ่งยาว 1 (`stick_long` / `wood_bough_long`) + สายยาว 2 (`string_long` เช่น `ducktape`)  
ปอ (`flax`) เป็น `string_normal` **ใช้ทำธนูนี้ไม่ได้**

สั่งธนู:

```json
{"request_id":"bow","op":"command","name":"bot.goal","kind":"craft","entity_id":"bow_wooden_assembled","count":1}
```

พอมีธนูในกระเป๋า:

```json
{"request_id":"eq","op":"command","name":"inventory.equip","item_id":"bow_wooden_assembled"}
```

จากนั้นอย่าสั่งอะไรเพิ่ม ชั้นล่างคือเควสรายวัน บอทไปต่อเอง

สลับงาน: **`bot.stop` ก่อน** แล้วค่อย `bot.goal` ใหม่

---

## 3) อ่าน `last_reason` แล้วทำต่อ ไม่เดา

| last_reason / reason | ความหมาย | สิ่งที่ต้องทำ |
|---|---|---|
| `game_not_ready` | ยังไม่เข้าโลก | รอ อย่าสั่งซ้ำรัว |
| `recipe_locked` | สูตรยังไม่ปลด | เปิดสกิล เรียน `bow_assembled` หรือบดคราฟต์ `blade_stone` จนหมวดอาวุธถึง 15 |
| `missing_material` + `สายยาว` | ช่องเชือกไม่ครบ | ต้องของ tag `string_long` อีก ไม่ใช่ปอ |
| `กระเป๋าเต็ม` / inventory_full | ช่องเต็ม | หยุดเก็บ ทิ้งขยะ (น้ำ/ปุ๋ย/หินเกิน) ห้ามเก็บต่อ |
| `เหนื่อย แต่ยังไม่เห็นที่พัก` | ล้าแล้วไม่เจอไฟ | ต้องเดินหาไฟ ไม่ใช่ยืนนิ่ง (มอดชุดใหม่เดินหาจุดเกิด) |
| `สัตว์เข้าใกล้ ตีสวน` | สมองร่างกายแทรก | งานคราฟต์รอได้ จนกว่าจะห่าง อย่าไป `bot.start survival` ซ้ำ |
| `crafting_...` | กดคราฟต์แล้ว | รอ 3–4 วิ แล้วเช็กกระเป๋า |
| `recipe_not_found` | id สูตรผิด | ใช้ `bow_wooden_assembled` |

`craft.make` ใช้**ตรวจครั้งเดียว** ว่าติดอะไร  
อย่าใช้เป็นเครื่องบดหลักโดยไม่ดู reason — บดสูตรล็อกไม่ได้

---

## 4) คำสั่งทีละก้าว (เมื่อบอทคิดเองไม่ขยับ)

เก็บของจากก้อนที่คลิกได้:

```
1. map.walk_to  kind=tile  x,y ของก้อน
2. รอให้ถึง (player.local moving=false)
3. interaction.select_nearest  kind=natural
4. รอ ~1.5 วิ
5. read path=interaction   ดู menus[].gather.available / has_tool / disabled
6. interaction.execute  action_id=Collect  menu_id=ตัวที่ available
```

ห้าม `execute` ทันทีหลัง `select` — เมนูมาช้า

คราฟต์กดเอง (ไม่รอ runner):

```
ui.open uri=Craft
craft.make entity_id=<สูตร>
```

ถ้า `recipe_locked` → `ui.open uri=Skill` แล้วให้บอท/คนกดเรียนในหน้าสกิล  
อย่า `maxskills`

สวมของ: `inventory.equip` ไม่ใช่ `inventory.use`

เดินไกล: `map.walk_to` + `kind=tile` (มี pathfinding)  
อย่า `kind=direct` ข้ามทะเล

---

## 5) สมองร่างกาย (ตัดงานได้)

ทุกติ๊กถามจากบนลงล่าง งานธนูรอได้:

ตาย → โดนตี → เลือดน้อยกิน → แรงหมดกิน → ล้าไปพัก → ถุงเต็มหยุดเก็บ → ค่อยทำงานที่สั่ง

ปิดชั่วคราว (ต้องตั้งก่อนเปิดเกม):

- `DURANGO_MEMORYBOT_BODY=0` ปิดสมองร่างกาย
- `DURANGO_MEMORYBOT_FIGHT=0` ไม่ตีสวน
- `DURANGO_MEMORYBOT_DROP=0` ไม่ทิ้งขยะ

อย่าปิดร่างกายแล้วลืมเปิดกลับ

---

## 6) เปิดเกม / ใส่มอดใหม่

1. `bot.stop`
2. ปิด `Durango.exe`
3. ก๊อป `tools/MemoryBotMod/bin/Release/net35/DurangoMemoryBot.dll`  
   ไป `dist/DurangoTH-Clean/mods/`
4. เปิดด้วย `-durango-updated` จากโฟลเดอร์ Clean  
   ตั้ง `DURANGO_AUTOCONNECT=http://127.0.0.1:8290`
5. รอ `main_scene` + `ready`
6. ค่อย `bot.goal`

ชุดเล่นจริงคือ **DurangoTH-Clean** ไม่ใช่ v2

---

## 7) read path ที่ใช้บ่อย

| path | ได้แก่อะไร |
|---|---|
| `game` | ฉาก / พร้อมหรือยัง |
| `player.local` | ตำแหน่ง เดินอยู่ไหม |
| `survival` | เลือด แรง ล้า |
| `inventory` | ของในถุง `count` / prototype |
| `interaction` | เป้าและเมนูเก็บ ของนี้ต้องมีก่อน execute |
| `skills` | แต้ม + สถานะเรียน |
| `bot` หรือ `bot.status` | งานที่รันอยู่ |
| `world.nearby` | ของรอบตัว |

---

## สูตรสั้นเวลาเจ้าของพูดว่า «ไปทำธนู»

```
อ่าน game → ต้องอยู่ในโลก
อ่าน inventory + survival
bot.stop
bot.goal kind=craft entity_id=bow_wooden_assembled count=1
วนอ่าน bot.status ทุก 4 วิ
ถ้า missing สายยาว แลถุงเต็ม → ให้คนทิ้งขยะ อย่าเสกเทป
ถ้า recipe_locked → เปิดสกิล บด blade_stone จนเรียน bow_assembled ได้
มีธนูในถุง → inventory.equip
อย่าแตะ survival — ชั้นล่างเป็นเควสรายวันอยู่แล้ว
```
