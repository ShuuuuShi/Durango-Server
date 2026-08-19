# `Program.cs`

**หน้าที่:** จุดเริ่มโปรแกรม — อ่าน argument, โหลด terrain, เปิดทั้ง 3 เซิร์ฟเวอร์, แล้ววน main loop

## `Main(args)`

### 1. ค่าเริ่มต้น + argument
| argument | ค่าเริ่มต้น | ความหมาย |
|---|---|---|
| `--data` | `data` | โฟลเดอร์ terrain |
| `--terrain` | `ri35te` | id แมพที่จะโหลด |
| `--name` | `Multi Play Server` | ชื่อที่โชว์ในเกม |
| `--game-port` | 8191 | TCP เกม |
| `--gateway-port` | 8190 | HTTP |
| `--assetbundles` | (เดาเอง) | โฟลเดอร์ asset bundle |
| `--player-save` | (เดาเอง) | ไฟล์ `.player` ที่ใช้ดึงหน้าตา |
| `--insecure-auth` | ปิด | GP-12: ไม่ตรวจ session token — **debug เท่านั้น** ใครก็สวมรอยเป็นใครก็ได้ |
| `--trust-client-profile` | ปิด | GP-14: เชื่อเลเวลที่ client ส่งมาทุกครั้ง (ไม่ใช้เลเวลในไฟล์เซฟ) |
| `--whitelist <ไฟล์>` | ไม่ใช้ | H-1: อนุญาตเฉพาะ entity id/ชื่อในไฟล์ (ดู [Accounts.md](Accounts.md)) |
| `--no-ip-bind` | ผูก IP | H-1: ไม่ผูก entity id กับ IP ที่จองครั้งแรก |
| `--no-account-check` | ตรวจ | H-1: ปิดการตรวจเจ้าของทั้งหมด (เทสในเครื่องเดียว) |
| `--enable-cheat` | ปิด | H-2: เปิด packet `Cheat` (เสกของ/เรียกสัตว์/ฟื้นเลือด) |
| `--admin <ชื่อ\|id>` | ไม่มี | H-2: ให้สิทธิ์ใช้ `control` คุมตัวละครคนอื่น (ใส่ซ้ำได้หลายครั้ง) |
| `--max-connections <n>` | 32 | H-3: เพดาน connection ทั้งเซิร์ฟ |
| `--max-connections-per-ip <n>` | 4 | H-3: เพดานต่อ IP (เทสหลาย client ในเครื่องเดียวต้องเพิ่ม) |
| `--radiotower` | **ปิด** | M-5: เปิดพอร์ตแชทส่วนตัว 8192 ซึ่งไม่มี auth (ดู [RadiotowerServer.md](RadiotowerServer.md)) |

⚠️ parser ใช้ `args[++i]` ตรง ๆ — ใส่ `--terrain` ท้ายสุดโดยไม่ตามด้วยค่า = `IndexOutOfRangeException`

### 2. เดา path ให้อัตโนมัติ
ถ้าไม่ได้ระบุ `--assetbundles` / `--player-save` จะไล่ขึ้นไป 4 ชั้นจาก `AppContext.BaseDirectory`
(`server/bin/Debug/net9.0/` → `Durango Claude/`) แล้วมองหา `game/DurangoV2_Data/...`

> path นี้ถูกแก้แล้วหลังย้ายโฟลเดอร์ (เดิมชี้ `DurangoV2/` ตอนนี้ชี้ `game/`)
> ถ้าย้ายโฟลเดอร์อีกหรือ build เป็น Release (path เป็น `bin/Release/net9.0`) ก็ยังใช้ได้ เพราะนับชั้นเท่ากัน

### 3. โหลด terrain
`TerrainStore.Load(dataDir, terrainId)` — ล้มเหลว = พิมพ์ `[fatal]` แล้ว `return` (ไม่เปิดเซิร์ฟ)

### 4. เปิดเซิร์ฟเวอร์
```
ServerWorld world  = new ServerWorld(terrain, serverName)
GameServer         → พอร์ต 8191
RadiotowerServer   → พอร์ต 8192
Gateway            → พอร์ต 8190  (+ UDP knock ที่ 8191)
```
✅ **GP-15 แก้แล้ว** — `Start()` ทั้งสองตัวคืน `bool` แล้ว
- พอร์ตเกม bind ไม่ได้ → พิมพ์ `[fatal]` แล้วจบโปรแกรม (ไม่ปล่อยให้รันแบบรับใครไม่ได้)
- พอร์ต radiotower bind ไม่ได้ → เตือน `[warn]` แล้วเล่นต่อได้ (แชทส่วนตัวใช้ไม่ได้เท่านั้น)
- `Gateway` ยังห่อ try/catch เหมือนเดิม

### 5. main loop
```csharp
TimeBeginPeriod(1);                       // GP-01: timer resolution 15.6ms → 1ms
Stopwatch clock = Stopwatch.StartNew();
while (true)
{
    gameServer.Process();
    gateway.Process();
    radiotower.Process();
    // sleep เท่าที่เหลือของ tick นี้ (1000/120 ms) ถ้าตามไม่ทันก็รีเซ็ตฐานเวลา
    // ไม่ให้หนี้เวลาสะสมแล้วไล่ยิงรัว
}
```
เธรดเดียวจบ ไม่มี graceful shutdown (ปิดด้วย Ctrl+C เท่านั้น) — `TimeEndPeriod(1)` อยู่ใน `finally`

### ✅ GP-01 แก้แล้ว
เดิม `Thread.Sleep(5)` บน Windows ที่ timer resolution 15.6 ms นอนจริง ~15.6 ms → **~64 รอบ/วินาที**
รวมกับ `ProcessPacketQueue()` ที่ดึง packet แค่ 1 ตัว/รอบ = เพดาน ~64 packet/วินาที/ผู้เล่น

ตอนนี้:
- `TimeBeginPeriod(1)` (winmm.dll) ดัน timer resolution ลงเหลือ 1 ms — ข้ามให้อัตโนมัติถ้าไม่ใช่ Windows
- ล็อก tick ด้วย `Stopwatch` ที่ `TargetTps = 120`
- `ProcessPacketQueue()` ระบายทั้งคิว (เพดาน 512/tick แล้ว log เตือนถ้าชน)

ค่าที่ปรับได้อยู่หัวไฟล์: `TargetTps` และ `StatsIntervalSeconds` (พิมพ์ `[loop] N tps, ผู้เล่นออนไลน์ N` ทุก 30 วิ)

ตรวจว่าได้ผลจริง: ดูบรรทัด `[loop] 120 tps` ใน log — ถ้าเห็น ~64 แปลว่า `timeBeginPeriod` ไม่ติด
