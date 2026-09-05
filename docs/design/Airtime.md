# ระบบ Airtime (เวลาเล่นแบบเติม) — เอกสารออกแบบ

สถานะ: 📐 ออกแบบ 3 ก.ย. 2026 · ยังไม่ลงมือ · ทำเป็น **server mod** (`tools/AirtimeMod`) + ต่อ SDK อีกนิดในแกน

## 1. แนวคิด

ผู้เล่นแต่ละบัญชีมี "แอร์ไทม์" เป็น**วินาที** · นับถอยหลังเฉพาะตอน**ออนไลน์อยู่ในโลก** · หมด = ออกจากเกม เข้าใหม่ไม่ได้จนกว่าจะเติม
เติมได้ 3 ทาง: แอดมินเติมตรง · **โค้ดเติม** (สร้างล่วงหน้า ใช้ครั้งเดียว) · ฟรีรายวัน (ตั้งค่าได้ 0 = ไม่แจก)

ทำไมเป็น mod: ตรรกะทั้งหมด (นับเวลา/โค้ด/เตือน) ไม่ยุ่งกับเกมเพลย์ อยู่นอกแกนได้ · เปิด-ปิดด้วยการถอดไฟล์ .dll · แกนต้องเพิ่มแค่ "จุดเสียบ" 4 อย่างที่ SDK ยังไม่มี (ข้อ 6)

## 2. กติกา (ค่าเริ่มต้น — ทุกค่าอยู่ใน `mods/airtime/config.json`)

| ค่า | เริ่มต้น | ความหมาย |
|---|---|---|
| `Enabled` | true | ปิด = ไม่นับ ไม่กันใคร (mod โหลดแต่นิ่ง) |
| `NewAccountFreeMinutes` | 180 | บัญชีใหม่ได้ 3 ชม. ฟรี (ครั้งเดียว) |
| `DailyFreeMinutes` | 30 | ทุกวัน 00:00 (เวลาไทย) เติมให้ฟรีไม่เกินค่านี้ **ถ้ายอดต่ำกว่า** (ไม่สะสม) — ให้คนไม่เติมยังแวะมาได้ |
| `TickSeconds` | 60 | หักทุก 60 วิ (นับจากตอน join จริง ไม่ใช่ตอน connect) |
| `WarnAtMinutes` | [30, 10, 5, 1] | เตือนผ่านข้อความในเกมเมื่อเหลือเท่านี้ |
| `GraceSeconds` | 90 | ถึง 0 แล้วยังอยู่ต่อได้อีก 90 วิ (เดินไปที่ปลอดภัย/เก็บของ) แล้วค่อยเตะ |
| `KickMessage` | "แอร์ไทม์หมด — เติมโค้ดแล้วเข้าใหม่ได้เลย" | ข้อความตอนเตะ/ตอนเข้าไม่ได้ |
| `AdminsUnlimited` | true | admin (`--admin gm` / IsAdmin) ไม่นับเวลา |
| `PauseWhenNoOtherPlayers` | false | ถ้า true: ออนไลน์คนเดียวทั้งเซิร์ฟไม่หัก (ทางเลือกใจดี) |
| `CodeLength` | 10 | โค้ด `AIR-XXXX-XXXX` ตัวอักษร A-Z2-9 (ตัดตัวสับสน 0/O/1/I) |
| `RedeemRateLimitPerMinute` | 5 | กันเดาโค้ด |

การนับ: หักเฉพาะตอน `Joined` (อยู่ในโลก) · ตาย/ยืนเฉย/AFK ก็นับ (ระบบง่าย ตรวจสอบได้) · ออกจากเกม = หยุดนับทันที ไม่มีเศษหาย (เก็บ `LastTickAt` แล้วหักตามจริงตอน tick ถัดไป ไม่ปัดขึ้น)

## 3. ข้อมูล (ผ่าน `IModStorage` → `data/mods/airtime/*.json`)

```
accounts.json  { "<entityId>": { "name", "balance_sec", "used_sec_total", "granted_free_at": "2026-09-03", "created": unix, "history": [ {at, delta_sec, by, note} … ≤50 ล่าสุด ] } }
codes.json     { "AIR-7K2M-9QXD": { "minutes": 600, "created": unix, "created_by": "gm", "used_by": null, "used_at": null, "note": "ลูกค้า A" } }
```
เซฟทุกครั้งที่ยอดเปลี่ยน + ทุก 5 นาที (tick) · โหลดตอน mod เริ่ม · **entityId = ตัวละคร** (เกมเรายังผูก 1 บัญชี = 1 ตัว ถ้าอนาคตมีหลายตัวต่อบัญชีให้ย้าย key เป็น account id)

## 4. เส้นทางผู้เล่น (flow)

1. **เข้าเกม** → แกนยิง `player.before_join` (cancellable) → mod เช็คยอด: ≤0 และไม่ใช่ admin → `Cancel("แอร์ไทม์หมด …")` → แกนส่ง `Error{Text}` ให้ client แล้วปิด (client โชว์กล่องข้อความหน้าไตเติ้ล — เส้นทางเดียวกับ "mod negotiation required")
2. **อยู่ในโลก** → `OnTick` ทุก 60 วิ: หัก `now − LastTickAt` · ถึงเกณฑ์เตือน → `SendMessage("⏳ แอร์ไทม์เหลือ 10 นาที")` · ถึง 0 → เริ่ม grace → ครบ → `player.Kick(KickMessage)`
3. **เติมด้วยโค้ด** (ผู้เล่นทำเองในเกม): พิมพ์ในแชท `/airtime AIR-7K2M-9QXD` → แกนส่งข้อความที่ขึ้นต้น `/` ให้ mod ก่อน broadcast (ข้อ 6) → mod ตรวจ/หัก rate limit → เติม → ตอบ "เติม 10 ชม. แล้ว เหลือ 12 ชม. 30 นาที" · โค้ดผิด 5 ครั้ง/นาที → บล็อก 10 นาที
4. **ดูยอด**: `/airtime` เฉย ๆ → "เหลือ 2 ชม. 15 นาที · ฟรีรายวันจะได้ตอน 00:00"
5. **แอดมิน** (ในเกมผ่าน cheat ถ้าเปิด หรือหน้า admin):
   `airtime add <ชื่อ> <นาที> [หมายเหตุ]` · `airtime set …` · `airtime info <ชื่อ>` · `airtime codes new <นาที> <จำนวน> [หมายเหตุ]` · `airtime codes list [unused]` · `airtime codes revoke <โค้ด>` · `airtime top` (ใครใช้เยอะสุด)
6. **หน้า admin** (แท็บ "Airtime" ใน `admin/index.html`): ตารางยอดทุกคน (ออนไลน์/ออฟไลน์, เหลือ, ใช้ไปรวม) · ปุ่มเติม · สร้างโค้ดเป็นชุดแล้ว copy ไปส่งลูกค้า · ประวัติ — ผ่าน route ที่ mod ลงทะเบียน `/admin/mods/airtime/*` (ข้อ 6)

## 5. ความปลอดภัย / กันโกง

- เวลานับฝั่งเซิร์ฟล้วน client ไม่รู้อะไร · โค้ดสุ่มจาก `RandomNumberGenerator` 10 ตัว (~10^15 แบบ) + rate limit + log ทุกครั้งที่เดาผิด (ชื่อ/IP)
- โค้ดใช้แล้วเก็บ `used_by` ไว้ตลอด (ตรวจย้อนได้) · แอดมินเติมตรงมี `by` + `note` ทุกรายการ
- reconnect ถี่ ๆ เพื่อไม่ให้ tick ทำงาน: หักตาม `now − LastTickAt` ตอน leave ด้วย ⇒ ไม่มีช่องหลบ
- เปลี่ยนชื่อตัวละครไม่กระทบ (key = entityId)
- สำรอง: `accounts.json` เป็นไฟล์เดียว เล็ก อยู่ใน `saves/` backup เดิม

## 6. ที่แกนต้องเพิ่มให้ SDK (เล็ก · ทำก่อน mod)

| เพิ่ม | ที่ไหน | ทำไม |
|---|---|---|
| `IModPlayer.Kick(string reason)` | `ServerModPlayer` → `ServerPlayer.Kick` (มีอยู่แล้ว) | เตะตอนเวลาหมด |
| `IModPlayer.IsAdmin` | `ServerModPlayer` → `ServerPlayer.IsAdmin` | ยกเว้นแอดมิน |
| event `player.before_join` (cancellable, มี `Data["name"]`, `["ip"]`) | `GameServer` ตรงหลัง `Ready` ก่อน `AddPlayer` — ถ้า `IsCancelled` ส่ง `Error{Text=CancelReason}` แล้วปิด | กันเข้าเมื่อยอด 0 |
| event `chat.command` (cancellable, `Data["text"]`) — ยิงเมื่อข้อความแชทขึ้นต้น `/` | `ServerPlayer` ตอนรับ `Say`/radiotower ก่อน broadcast · ถ้า mod cancel = ไม่ broadcast | ให้ผู้เล่นเติมโค้ดได้โดยไม่ต้องเปิด `--enable-cheat` (VPS ปิดอยู่) |
| `IModApi.RegisterAdminRoute(path, handler)` | `Gateway.Admin` ผูกใต้ `/admin/mods/<modId>/` + guard token เดิม | หน้าแอดมินของ mod |
| `IModApi.RunLater(seconds, action)` (ทางเลือก) | PluginManager | ทำ grace period ง่ายขึ้น (ไม่มีก็ใช้ OnTick นับเอง) |

ทั้งหมดเป็น additive — mod เก่า (Example*) ไม่กระทบ · `ApiVersion` ขยับ 1.1 → 1.2

## 7. โครงสร้าง mod

```
tools/AirtimeMod/
├── AirtimeMod.csproj        (อ้าง mod-sdk เหมือน ExampleGameplayMod)
├── AirtimePlugin.cs         IGamePlugin: OnLoad → โหลด storage/config · subscribe before_join, chat.command · OnPlayerJoined/Left · OnTick
├── AirtimeLedger.cs         บัญชี/ประวัติ/tick/เตือน (pure logic — เทสได้โดยไม่ต้องรันเซิร์ฟ)
├── AirtimeCodes.cs          สร้าง/ตรวจ/ใช้โค้ด + rate limit
├── AirtimeConfig.cs         ค่าตั้งใน mods/airtime/config.json (สร้างให้ถ้าไม่มี)
└── AirtimeAdminRoutes.cs    JSON API สำหรับหน้า admin
tests/AirtimeMod.Tests/      (xunit) — tick/grace/daily free/code rate limit
server/admin/index.html      + แท็บ Airtime
```

## 8. ลำดับทำ (ประมาณ 1.5-2 วัน)

1. SDK 4 จุด (ข้อ 6) + build + `ExampleGameplayMod` ยังโหลดได้ — ครึ่งวัน
2. `AirtimeLedger` + `AirtimeCodes` + unit test — ครึ่งวัน
3. `AirtimePlugin` เชื่อม event + คำสั่ง + ข้อความ — ครึ่งวัน
4. แท็บ admin + เทสจริงในเครื่อง (บัญชีใหม่ 3 ชม. → set เหลือ 2 นาที → ดูเตือน/grace/เตะ → เติมโค้ด → เข้าใหม่) — ครึ่งวัน
5. ขึ้น VPS: ประกาศล่วงหน้า + ตั้ง `NewAccountFreeMinutes` ให้ผู้เล่นเดิมได้ยอดตั้งต้น (สคริปต์ seed จาก `saves/players/*.json`)

## 9. คำถามที่ต้องตอบก่อนลงมือ

- ผู้เล่นเดิม 44 คนบน VPS ได้ยอดตั้งต้นเท่าไร (เสนอ: เท่า `NewAccountFreeMinutes` 3 ชม. ทุกคน)
- ฟรีรายวัน 30 นาทีเอาไหม (0 = ไม่ให้เลย เข้าไม่ได้จนกว่าจะเติม)
- โค้ดขายเป็นแพ็กไหน (เสนอ 10 ชม. / 30 ชม. / 100 ชม.) — มีผลแค่ค่า default ตอนสร้างโค้ด
- แสดงยอดคงเหลือบนหน้าจอเกมไหม — ตอนนี้ทำได้แค่ข้อความในแชท/Info · ถ้าอยากมี HUD ต้องแก้ client (มอดฝั่ง client) เป็นเฟสถัดไป
