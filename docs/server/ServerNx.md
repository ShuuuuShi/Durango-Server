# DurangoServerNx — เซิร์ฟใหม่พอร์ตตรงจากเซิร์ฟในตัวของเกม (มือถือก่อน)

สร้าง: 3 ก.ย. 2026 · โค้ด: `server-nx/` · ตัวเปิด: `เปิดเซิร์ฟNX.bat` → `tools/start-server-nx.ps1`

## นี่คืออะไร ทำไมต้องมี

`nexonSRC/Durango.Offline/` คือ**เซิร์ฟที่ NEXON เขียนเอง** แล้วแถมมากับ client DLL
(โหมดออฟไลน์/LAN ของเกมจริง — Gateway HTTP 8190 + GameServer TCP 8191)
DurangoServerNx คือการพอร์ตชุดนั้นขึ้น .NET 9 ให้รันแยกต่างหาก **ให้ client มือถือ
(Android 5.2.1 ของแท้) เล่นได้ก่อน** โดยคงพฤติกรรม/รูปแบบข้อมูลของต้นฉบับให้มากที่สุด

- เซิร์ฟเดิม (`server/DurangoServer`) **ไม่ถูกแตะ** — ยังรัน beta PC ต่อไป (พอร์ต 8290)
- เซิร์ฟใหม่เป็น "แกนแท้ 43 handlers": แตะเก็บ/เดิน/ปลูก/สร้าง/เก็บของ/แชทช่องรวม/เซฟ-โหลด
  ของที่ต้นฉบับไม่มี (clan/mail/market จริง/quest/warp ฯลฯ ~300 messages) ยังไม่มี —
  client จะแสดง "ยังไม่เปิด" ตามพฤติกรรมแท้ (ดู roadmap ท้ายไฟล์)

## โครงสร้าง

```
server-nx/
  Program.cs          CLI + main loop 120 TPS + เซฟทุก 60 วิ
  SelfTest.cs         --selftest จำลอง handshake เกมแท้ผ่าน TCP จริง
  Core/
    Host.cs           สล็อตผู้เล่น + โลก (แทน Server.cs/Servers.cs ต้นฉบับ — โลกเดียวหลายผู้เล่น)
    GameServer.cs     TCP, handshake แท้ GetClock→Auth→Ready ตอบ Clock→Welcome→OK
    Gateway.cs        HTTP /knock /notice /sessions /admission /entry /players /accounts /terrains/*
    World.cs          chunk 16×16, garden (ต้นไม้/แร่), artifacts, สภาพอากาศ
    Player.cs         39 handler แท้ (Move/Touch/PlantSeed/Equip/Storage/SayInExclusiveChannel/...)
    PlayerContext.cs / WorldContext.cs   เซฟ .player/.world JSON ฟอร์แมตต้นฉบับ
    TerrainLoader.cs  โหลด terrain zip จากดิสก์ (ฟอร์แมตเดียวกับต้นฉบับ)
    Cheats.cs / MarketManager.cs / ArtifactManager.cs / CropYaml.cs / PerformanceYaml.cs
  Support/            util พอร์ตเฉพาะส่วน (Json/Gettext/KUtility/Enums/AppData) + data classes (Yaml.*)
```

ชั้น protocol/data **ใช้ของเดิมจาก `server/GameCode/**` ผ่าน link ใน csproj ไม่ fork**
(989 message structs — parity ยืนยันแล้วว่าตรงต้นฉบับ 100% ดู `docs/parity/message-parity-report.md`)

## รัน

```
tools\start-server-nx.ps1          เมนู (หรือคลิก เปิดเซิร์ฟNX.bat)
# มือถือจริง:   DurangoServerNx --gateway-port 8190 --game-port 8191 --assetbundles-android <dir>
# ทดสอบ:       --gateway-port 18290 --game-port 18291   (ใช้ได้แม้เซิร์ฟเดิมรันอยู่)
# ตรวจ:        DurangoServerNx --selftest --gateway-port 18290 --game-port 18291
```

- พอร์ต 8190 = ค่าที่ APK ฝังมาในตัว (เปลี่ยนพอร์ตแล้ว APK เก่าต่อไม่ได้ ต้องแพตช์ APK ใหม่)
- bind wildcard ไม่ได้ (ไม่ใช่ admin) ⇒ WebServer fallback เป็น loopback อัตโนมัติ —
  ให้มือถือใน LAN ต่อได้ต้อง `netsh http add urlacl url=http://*:8190/ user=Everyone` (ทำครั้งเดียว)
- เซฟอยู่ที่ `server-nx/AppData-nx/offline/nx/{slot}.player|.world` (โลก = สล็อต 0)

## ตรวจแล้ว (3 ก.ย. 2026)

| ทดสอบ | ผล |
|---|---|
| `dotnet build -c Release` | ✅ 0 error 0 warning |
| game data โหลดครบ (prototype 2,407 / artifact 560 / natural 711 / recipe 720 / pet 74) | ✅ |
| HTTP: /knock (Android) /notice /entry /sessions /players /terrains/1 | ✅ |
| selftest TCP: GetClock→Clock, Auth→Welcome (region=1/template=pe10gr_1), Ready→OK, SetChunk→9 chunks | ✅ |
| เซฟ .player/.world ลงดิสก์ + โหลดกลับ | ✅ |
| มือถือ MuMu (APK 0.1.4) เข้าโลกจริง | ⬜ รอเทส |

## Deviation จากต้นฉบับ (ตั้งใจทั้งหมด)

| จุด | ต้นฉบับ | DurangoServerNx | เหตุผล |
|---|---|---|---|
| โลก/สล็อต | 1 สล็อต = 1 โลก (offline โฮสต์คนเดียว) | โลกเดียว (สล็อต 0) + ผู้เล่น N คน | มือถือหลายคนเล่นรวม |
| /sessions | token = entity id, context เดียว | token GUID + session→สล็อต, รับ player JSON แบบ LAN joiner | รองรับหลายคน + คง response รูปร่างเดิม |
| /knock bundle URLs | CDN ของ Nexon | โฮสต์ตัวเอง + เสิร์ฟไฟล์จากดิสก์ | CDN ตายแล้ว — มือถือต้องโหลดจากเรา |
| cluster_mode | Offline/Editable | ค่าเดิม (Offline เป็นหลัก) | เมนูครบตาม `MenuSystem.ShowInOffline` — ค่า "SingleMode" ของเซิร์ฟเดิมตกไปที่ Offline อยู่แล้ว (ToEnum fail → default) |
| หน้าตา random | EditPlayerDisplayProxy (UI client) | default เรียบ ๆ | prologue ของ client ส่ง model_info มาเองเสมอ |
| blob encyclopedia | เติม memo ครบตาม locale เครื่องโฮสต์ | เริ่มว่าง client อัปเดตเอง | เซิร์ฟไม่มีตารางภาษา |
| สีไอเทมสุ่ม | ตารางสี .raw ใน resources | '#'hex ได้ / นอกนั้น default ขาว | ไฟล์ .raw ไม่มีบนเซิร์ฟ — client เรนเดอร์เอง |
| Gettext | resolve ตาม locale เครื่องโฮสต์ | th_TH → en_US → msgid | ผู้เล่นไทยได้ชื่อไทยจาก data โดยตรง |
| /players ต่อ context | ใช้ _playerCtx ตัวเดียว | ผูก session จาก Authorization header → สล็อต | หลายผู้เล่น |

## Roadmap ต่อ (หลัง MuMu ผ่าน)

1. **มือถือ E2E**: APK 0.1.4 ชี้ host เครื่องนี้ → knock → สร้างตัวละคร → เข้าโลก → เดิน/แตะเก็บ → ออก → เข้าใหม่ได้ของเดิม
2. **เปิดระบบจาก contract**: ~300 messages ที่ backend MMO เคยรับ (รายการครบใน `docs/parity/`) —
   หยิบทีละกลุ่ม: travel/warp → quest → taming (ใช้ NDC16 notes เป็นแนวสถาปัตยกรรม)
3. **เทียบผลกับเซิร์ฟเดิม**: พฤติกรรมแท้ 43 handlers เป็น baseline ตรวจ gp-check 5 ข้อที่ตก

## ข้อควรระวัง

- `nexonSRC/` เป็น decompile ของ NEXON — **gitignored ห้าม push** (DMCA) โค้ดใน `server-nx/`
  เขียนใหม่เป็นสไตล์ของเราโดยอ้างอิงต้นฉบับ ไม่ก๊อปไฟล์ตรง
- แชทบนมือถือวิ่งบน connection เกม (Mode.Offline) ตามแท้ — ไม่มี radiotower
- ถ้าเปลี่ยน `--gateway-port` จาก 8190: มือถือต้องใช้ `--url-prefix` แบบเซิร์ฟเดิม หรือแพตช์ APK
