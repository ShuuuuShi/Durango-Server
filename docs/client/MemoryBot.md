# DurangoMemoryBot MVP

`DurangoMemoryBot.dll` เป็น client mod สำหรับอ่าน managed state แบบ whitelist และสั่งงานผ่าน API เกมจริง โดยไม่ใช้เมาส์/คีย์บอร์ดหรือ screenshot เป็นค่าเริ่มต้น

## ข้อจำกัดสำคัญ

- ไม่ใช่ raw process-memory reader/writer
- ไม่เปิด arbitrary reflection หรือ private object path
- mod ทำงานใน Unity Mono process เดียวกับเกม จึงต้องติดตั้งเฉพาะ DLL ที่เชื่อถือได้
- คำสั่ง gameplay ยังผ่านระบบเกมและ server validation เดิม

## ติดตั้ง

```powershell
powershell -ExecutionPolicy Bypass -File tools/build-memory-bot.ps1
```

สคริปต์จะติดตั้งเฉพาะ `game/mods/DurangoMemoryBot.dll` ไม่ overwrite `Assembly-CSharp.dll`

## เปิด bridge

ตั้งค่าได้ก่อนเปิดเกม:

```powershell
$env:DURANGO_MEMORYBOT=1
$env:DURANGO_MEMORYBOT_PORT=8193
$env:DURANGO_MEMORYBOT_TOKEN='ตั้งรหัสลับถ้าต้องการ'
$env:DURANGO_MEMORYBOT_OVERLAY=1
```

ค่า default คือ port `8193` และไม่มี token ถ้าไม่ตั้ง environment variable

## Protocol

ส่ง JSON หนึ่งบรรทัดผ่าน TCP loopback `127.0.0.1:8193` และรับ JSON หนึ่งบรรทัดกลับ

อ่านข้อมูล:

```json
{"request_id":"1","op":"read","path":"player.local"}
```

paths ที่รองรับใน MVP:

```text
game
screen
player.local
survival
inventory
inv
status
interaction
combat
world.nearby
```

สั่งงาน:

```json
{"request_id":"2","op":"command","name":"player.stop"}
{"request_id":"3","op":"command","name":"player.move_to","x":8200,"y":35600}
{"request_id":"4","op":"command","name":"inventory.use","item_id":"item-id"}
{"request_id":"5","op":"command","name":"combat.use_action","action_id":"barehand_kick_a"}
{"request_id":"6","op":"command","name":"ui.open","uri":"Inventory"}
{"request_id":"7","op":"command","name":"interaction.select_nearest","kind":"prop"}
{"request_id":"8","op":"command","name":"interaction.refresh"}
{"request_id":"9","op":"command","name":"interaction.execute","action_id":"Rest"}
```

`interaction.select_nearest` จะเลือกเป้าหมายใกล้สุดจากระบบ interaction จริงและส่ง touch request ตาม pipeline เกมเดิม; หลังจากนั้นอ่าน `interaction` เพื่อรอเมนู แล้วจึงใช้ `interaction.execute` ด้วย action ที่มีอยู่และไม่ถูกปิดใช้งาน เช่น `Rest`, `Collect` หรือ `Attack`. คำสั่งเหล่านี้คืนสถานะการรับคำสั่ง ไม่ได้ bypass server validation.

Capture เฉพาะเมื่อสั่ง:

```json
{"request_id":"7","op":"capture","filename":"manual-check.png"}
```

ไฟล์ capture จะถูกเขียนใต้ `game/MemoryBotCaptures/`; ไม่รับ absolute path หรือ `..`

## Smoke client

```powershell
powershell -ExecutionPolicy Bypass -File tools/MemoryBotClient/memorybot-client.ps1 -Path game
powershell -ExecutionPolicy Bypass -File tools/MemoryBotClient/memorybot-client.ps1 -Path survival
powershell -ExecutionPolicy Bypass -File tools/MemoryBotClient/memorybot-client.ps1 -Capture
```

## Security

- bridge bind loopback เท่านั้น
- ตั้ง `DURANGO_MEMORYBOT_TOKEN` สำหรับ local authentication
- จำกัด queue และ request line
- ทุกคำสั่งทำบน Unity main thread
- อย่าเปิด port นี้ออกอินเทอร์เน็ต
- `tap` ของ `BotBridge` เดิมไม่ใช่ส่วนหนึ่งของ MemoryBot API

## Real-client bridge verification (2026-08-27)

`DurangoMemoryBot.dll` was rebuilt and installed to `game/mods/`, then loaded by the real `DurangoV2.exe` client together with `ExampleClientMod`. The bridge was enabled on `127.0.0.1:8193` with a token.

Observed through the TCP bridge, without mouse, keyboard, screenshot, or desktop input automation:

```text
read game          -> scene=Main, ready=true, main_scene=true
read player.local  -> position=[8100,35500], tile=[40.5,177.5], alive=true
command player.move_to x=8200 y=35600 -> accepted
read player.local  -> position=[8184.107,35584.11], tile=[40.921,177.921], moving=false
command player.stop -> accepted
```

The position change confirms that the mod can drive the local player through the game's managed API while the user keeps control of the mouse. The server still validates the underlying gameplay requests.

## Daily quest runner (test only)

The test-only daily runner reads the real daily quest cache and executes the game's managed APIs in this order: farming, water, equipment, ranged hunt, revive, repair, storage, skill learning, eating, rest, local warp, and island travel. It claims each reward when `auto_reward` is enabled.

Start it through the same loopback protocol:

```json
{"request_id":"20","op":"command","name":"bot.start","kind":"daily"}
{"request_id":"21","op":"read","path":"daily.quests","limit":32}
{"request_id":"22","op":"command","name":"bot.status"}
{"request_id":"23","op":"command","name":"bot.stop"}
```

`DURANGO_MEMORYBOT_TEST=1` enables test-fixture provisioning only (items, farms, level, and test animal). It is off by default. `DURANGO_MEMORYBOT_UI=1` adds the test-only in-game panel for Daily/Gather/Stop, fixture provisioning, and reward claiming. This runner is not enabled for production use.
