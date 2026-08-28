# DinoWorld Launcher

Launcher ตัวเปิดเกมแบบเกมออนไลน์ทั่วไป (GS / Ragnarok style) — ธีมดำเข้ม+ชมพูเข้ม, โลโก้ผีเสื้อ 8-bit
สร้างเมื่อ 26 ส.ค. 2026 ตามคำสั่ง "จัดการสร้างเลย" (หลังผ่าน mockup `docs/design/mockups/launcher_mockup.html`)

## ทำอะไรได้

| ฟีเจอร์ | รายละเอียด |
|---|---|
| 📰 ประกาศ | โหลดจากเซิร์ฟ `GET /launcher/news` — แท็บกรอง ประกาศ/อัปเดต/อีเวนต์ |
| 🟢 สถานะเซิร์ฟ | ชื่อเซิฟ + จำนวนผู้เล่น/เพดาน + เวอร์ชันล่าสุด จาก `GET /launcher/status` (refresh ทุก 5 วิ) |
| ▶ เข้าเกม | เปิด `DurangoV2.exe` พร้อม env **`DURANGO_AUTOCONNECT=<ip[:port]>`** → client ต่อเซิฟทันที ไม่ต้องพิมพ์ IP (client อ่าน env นี้อยู่แล้ว: `client/Durango.Offline/Server.cs` → `AutoConnectTarget`) |
| 📦 อัปเดตเกม | logic เดียวกับ `tools/Updater`: manifest → โหลด zip → ตรวจ SHA256 → แตก temp → robocopy /MIR สลับ (เว้น AppData*/server.txt/version.txt/game.log/launcher_settings.json) |
| 🔧 ซ่อมไฟล์ | force re-download ชุดไฟล์ปัจจุบันผ่าน manifest เดียวกัน |
| ⚙️ ตั้งค่า | ip:port เซิร์ฟ + toggle auto-patch — เซฟที่ `launcher_settings.json` ข้างเกม |

**Fail-safe หลัก**: launcher/เน็ต/manifest พังอย่างไรก็ยังกด "เข้าเกม" ได้เสมอ (เข้าเวอร์ชันเดิม) —
หลักการเดียวกับ tools/Updater "ห้ามบล็อกการเข้าเกม"

## Endpoint ฝั่ง server (ใหม่)

อยู่ที่ `server/ServerCore/Gateway.Launcher.cs` (partial Gateway, ลงทะเบียนใน constructor ของ Gateway.cs)
— prefix `/launcher/*` = โซนผู้เล่น ต่างจาก `/admin/*` ตรงที่**อ่านอย่างเดียว ไม่มี action, ไม่ต้อง token**

| Route | คืนอะไร |
|---|---|
| `GET /launcher/news` | `{items:[{cat,date,title,body}]}` อ่านจาก `data/launcher_news.json` (cache 5 วิ — แก้ไฟล์ได้ตลอดไม่ต้อง restart) |
| `GET /launcher/status` | `{name, players, max_players, tps, latest_version}` — subset ปลอดภัยของ /admin/status |
| `GET /launcher/version` | `{version, zip_url?, sha256?, notes?}` จาก `data/launcher_patch.json` |

### ไฟล์ data (repo `data/`)

- `launcher_news.json` — ประกาศบน launcher (`cat` = news/update/event)
- `launcher_patch.json` — manifest แพตช์ รูปแบบเดียวกับ manifest.json ของ Updater
  (`version`,`zip_url`,`sha256`,`notes`) · **zip_url ว่าง = ยังไม่แจกแพตช์ผ่านเซิร์ฟ**
  (launcher จะ fallback ไป update-manifest-url.txt / GitHub Releases เดิม)

## build & deploy

```bash
# server (route ใหม่มากับ build ปกติ)
dotnet build server/DurangoServer.csproj -c Release

# launcher → exe เดียว ไม่ต้องมี .NET ในเครื่องผู้เล่น
dotnet publish tools/Launcher/DinoWorld.Launcher.csproj -c Release -r win-x64 -p:SelfContained=true
# → tools/Launcher/bin/Release/net9.0-windows/win-x64/publish/DinoWorldLauncher.exe
```

วิธีใช้: เอา `DinoWorldLauncher.exe` ไปวาง**ในโฟลเดอร์เกม ข้าง ๆ DurangoV2.exe** (ต้องข้างเกม เพราะ
อัปเดต/ซ่อมไฟล์/เปิดเกม ทำงานกับโฟลเดอร์ตัวเอง) — เปิดครั้งแรกตั้ง ip:port ที่ ⚙️ ตั้งค่า แล้วกด ▶ เข้าเกม

## โครงสร้างโค้ด (tools/Launcher/)

| ไฟล์ | หน้าที่ |
|---|---|
| `MainWindow.xaml(.cs)` | UI หลัก + logic ทั้งหมด (news/status/patch/play) |
| `SettingsDialog.xaml(.cs)` | หน้าต่างตั้งค่า ip:port + auto-patch |
| `PixelButterfly.cs` | ผีเสื้อ 8-bit วาดจากพิกเซลแมป 12x10 (Canvas+Rectangle) |

## ของที่ยังไม่ได้ทำ (เฟสถัดไปตามแผน)

- ประกาศแก้ผ่าน admin panel (ตอนนี้แก้ JSON ไฟล์ตรงๆ) · multi-island dropdown ในตั้งค่า
- วัด ping จริง (ตอนนี้ placeholder) · icon .ico ให้ exe
- เฟส 3: login username/password ใน launcher (ต้องเพิ่มระบบบัญชีฝั่ง AccountStore ก่อน)
