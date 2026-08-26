# VPS จริง — วิธีที่ Claude เข้าไปจัดการ (2026-08-24)

> ⚠️ **กฎเหล็ก: มีผู้เล่นจริงอยู่บนเซิร์ฟนี้แล้ว — ห้ามอัป/รีสตาร์ทโดยไม่ถามเจ้าของก่อนทุกครั้ง**
> ต้อง **เทสในเครื่อง local ก่อนเสมอ** (`dotnet run` ธรรมดา ดู `docs/server/BETA-OPS.md`) ยืนยันว่าใช้ได้แล้ว
> ค่อยขออนุญาตอัปขึ้น VPS — แม้แค่ "รีสตาร์ท" (ไม่ใช่แก้โค้ด) ก็ทำให้คนที่กำลังเล่นอยู่หลุดชั่วครู่

## เครื่อง

| รายการ | ค่า |
|---|---|
| IP (สาธารณะ) | `187.127.208.20` |
| SSH | `root@187.127.208.20` — รหัสผ่านถามเจ้าของ (ไม่เก็บในเอกสารนี้) |
| Host key (SHA256) | `SHA256:2F3qfIsSFwi5vtLXAE97PpaD2YHdFcArh0lv8ENwpas` |
| OS | Ubuntu 24.04.4 LTS |
| ทรัพยากร | 1 core · 3.8 GB RAM · 48 GB disk (ว่าง ~44GB) |
| Timezone | UTC (เวลาไทย = UTC+7) |
| ⚠️ เครื่องนี้มี service อื่นของเจ้าของอยู่ด้วย | เช่น `korepilot-egress` — **อย่าไปยุ่ง ufw rule/process ที่ไม่ใช่ durango-\*** |

## เครื่องมือที่ใช้ (ฝั่ง Windows)

`plink.exe`/`pscp.exe` (PuTTY CLI) อยู่ที่ `%TEMP%\opencode\plink.exe` — รองรับ `-pw` ให้ auth แบบไม่ต้อง
โต้ตอบ (ต่างจาก native `ssh`/`scp` ของ Git Bash ที่ยังไม่ได้ตั้ง key-based auth ไว้)

```bash
HK="SHA256:2F3qfIsSFwi5vtLXAE97PpaD2YHdFcArh0lv8ENwpas"
PLINK="/c/Users/thana/AppData/Local/Temp/opencode/plink.exe"
PSCP="/c/Users/thana/AppData/Local/Temp/opencode/pscp.exe"
"$PLINK" -batch -hostkey "$HK" -pw '<รหัสผ่าน>' root@187.127.208.20 "<คำสั่ง>"
"$PSCP" -batch -hostkey "$HK" -pw '<รหัสผ่าน>' -r "<path Windows>" root@187.127.208.20:/root/durango/
```

`-batch` กัน prompt ค้าง, `-hostkey` ต้องใส่ทุกครั้งไม่งั้น `plink` ถามยืนยัน host key แล้วค้าง (ใน batch mode
ค้างแล้ว fail ทันทีเพราะ "Cannot confirm a host key in batch mode" — เจอมาแล้วรอบแรกที่ต่อ)

## โครงสร้างบนเครื่อง

```
/root/durango/
├── linux-x64/              # publish self-contained (ตัวที่รันจริง)
│   ├── DurangoServer        # binary
│   └── admin/index.html     # หน้า admin (แก้แยกจาก binary — อัปทีหลังได้)
├── data/                    # data ชุดจริง (config.json/islands/terrains)
├── saves/                   # เซฟผู้เล่นจริง — **ห้ามลบ/เขียนทับมั่ว ๆ**
├── logs/                    # log เก่าที่หมุนออกมา (restart.sh ย้ายมาไว้ตรงนี้ ลบอัตโนมัติหลัง 7 วัน)
├── server.log                # log ปัจจุบันที่กำลังรัน
└── restart.sh                # สคริปต์ restart แบบไม่ล้างเซฟ (ดูหัวข้อ cron ด้านล่าง)
```

## Deploy โค้ดใหม่ (ทำ **หลังเทส local ผ่านแล้วเท่านั้น** และขอเจ้าของก่อนเสมอ)

```bash
# 1) build ฝั่ง Windows
cd "C:\Users\thana\Desktop\Durango Opencode\server"
dotnet build -c Debug                                              # เช็ค compile ก่อน (เร็ว)
dotnet publish -c Release -r linux-x64 --self-contained true -o "../publish/linux-x64"

# 2) อัปโหลดทับ (ระวัง: ต้อง kill โปรเซสเดิมก่อนไม่งั้น pscp ทับไบนารีที่กำลังรันอยู่ค้างกลางทางได้
#    ถ้าเน็ตสะดุด — ปลอดภัยกว่าถ้า kill ก่อนอัป ไม่ใช่อัปแล้วค่อย kill)
"$PLINK" ... "pkill -9 -f DurangoServer"
"$PSCP" ... -r "C:\...\publish\linux-x64" root@187.127.208.20:/root/durango/
"$PSCP" ... "C:\...\server\admin\index.html" root@187.127.208.20:/root/durango/linux-x64/admin/index.html
"$PLINK" ... "chmod +x /root/durango/linux-x64/DurangoServer"

# 3) รันใหม่ (หรือใช้ /root/durango/restart.sh ซึ่งทำขั้นตอนนี้ให้ + หมุน log)
"$PLINK" ... "cd /root/durango/linux-x64 && nohup ./DurangoServer \
  --data /root/durango/data --saves /root/durango/saves \
  --public-host 187.127.208.20 --enable-cheat --name 'Durango VPS Test' \
  --admin-token <TOKEN> > /root/durango/server.log 2>&1 & disown"

# 4) เช็คผล
"$PLINK" ... "tail -20 /root/durango/server.log"
curl -s http://187.127.208.20:8190/entry     # ต้องได้ JSON กลับมา ไม่ error
```

⚠️ **`pscp`/`plink` เคยเจอ "Connection timed out" เฉย ๆ กลางทาง** (เน็ตสะดุดชั่วคราว ไม่ใช่ VPS ล่ม) —
ถ้าเจอให้เช็ค `ping 187.127.208.20` ก่อนสรุปว่าเครื่องมีปัญหา แล้วลองใหม่อีกครั้งได้เลย

## Firewall (ufw) — พอร์ตที่เปิดให้ Durango

```
8190/tcp   durango-gateway     (HTTP: /entry /knock /sessions /admin/* /assetbundles/*)
8191/tcp   durango-game        (TCP เกมจริง)
8191/udp   durango-knock       (server list broadcast)
8192/tcp   durango-radiotower  (แชทส่วนตัว — ปิดอยู่ ไม่ได้เปิด --radiotower)
```
เพิ่มด้วย `ufw allow <port>/<tcp|udp> comment durango-xxx` — **ใส่ comment `durango-` เสมอ** จะได้แยกจาก
rule เดิมของ service อื่นบนเครื่องนี้ได้ง่าย (`ufw status | grep durango`)

## Admin panel

`http://187.127.208.20:8190/admin?token=<TOKEN>` — ต้องมี token ต่อท้าย URL ครั้งแรก (เบราว์เซอร์จำให้เอง
ผ่าน localStorage หลังจากนั้น) ดูรายละเอียดระบบ token ที่ `Gateway.Admin.cs` (`GuardAdminRoutes`) — token
จริงถามเจ้าของ ไม่เก็บในเอกสารนี้ (repo อาจเป็น public ในอนาคต)

มีฟีเจอร์: สถานะเซิร์ฟ/log สด/ผู้เล่นออนไลน์ (เตะ/เทเลพอร์ต)/ย้าย-เพิ่ม-ลบ POI/แก้ config.json สด/
Mod Loader/สั่ง cheat ในนามผู้เล่น/**บรอดแคสต์ข้อความให้ทุกคน**

## รีสตาร์ทอัตโนมัติทุกวัน (cron)

ตั้งไว้แล้วที่ `crontab -l` บน VPS:
```
0 21 * * * /root/durango/restart.sh >> /root/durango/logs/cron.log 2>&1
```
= 21:00 UTC = **04:00 เวลาไทย** ทุกวัน (ช่วงคนเล่นน้อยสุด) — `restart.sh` **ไม่ล้างเซฟ** (kill โปรเซสเดิม
+ หมุน log เก่าไปไว้ที่ `logs/` (ลบทิ้งเองหลัง 7 วัน) + รันใหม่ด้วย flag เดิม) ตัวสคริปต์อยู่ที่
`/root/durango/restart.sh` บน VPS โดยตรง (ไม่ได้ track ใน git — ถ้าจะแก้ต้อง SSH เข้าไปแก้ตรง ๆ หรือ
อัปโหลดทับใหม่)

## Feature flag ที่เซิร์ฟสั่ง client ได้โดยไม่ต้อง build/แจก client ใหม่

`data/config.json` → `ServerConfig` (`ConfigRoot`) มีค่าที่ hot-reload (เช็คทุก 5 วิ) และบางค่าถูกส่งต่อให้
client ผ่าน `/knock` หรือ `/entry`:

| ค่าใน config.json | ส่งผ่าน | ผลที่ client |
|---|---|---|
| (ตัวเลขเรทเกิด/exp/เลือด/ดาเมจ ฯลฯ) | ไม่ส่งให้ client — ใช้ฝั่งเซิร์ฟเท่านั้น | มีผลทันที ไม่ต้อง restart |
| `SkipPrologueVideo` (bool) | `/knock` **และ** `/entry` (`skip_prologue_video`) | ข้ามฉากรถไฟ/หนังเปิดตอนสร้างตัวละครใหม่ (default `true` — ฉากเต็มมี `MediaPlayerCtrl` เล่นวิดีโอที่ไฟล์หายไป ทำเกมปิดตัวเองกะทันหันตอนผู้เล่นใหม่สร้างตัวละครครั้งแรก) |
| `ClusterMode` (ตั้งผ่าน `--cluster-mode` ไม่ใช่ config.json) | `/entry` (`cluster_mode`) | สลับ SingleMode/Online ที่ client เห็น (ดู `docs/`, ยังไม่ได้เทสทุกจุดที่ Online mode มีผล) |

รูปแบบนี้ทำต่อได้เรื่อย ๆ — ถ้าจะเพิ่ม flag ใหม่ที่ต้องคุมจาก server: (1) เพิ่ม field ใน `ConfigRoot`
(`ServerConfig.cs`) (2) ใส่ค่าใน response ของ `/knock` (ถ้าต้องมีผลก่อนผู้เล่นมี `PlayerId`) หรือ `/entry`
(ถ้ามีผลหลังต่อเซิร์ฟจริงแล้วพอ) (3) ฝั่ง client อ่านค่าที่ `TitleMenuGroup.cs` case ที่ตรงกัน แล้ว set ค่า
ให้ระบบที่เกี่ยวข้อง — **ต้องอัป client รอบเดียวตอนเพิ่ม flag ใหม่ แต่หลังจากนั้นสลับได้จาก config.json
อย่างเดียวตลอดไป**

## บทเรียนจากการดีบักวันนี้ (กันเจอซ้ำ)

1. **ผู้เล่นใหม่ (ไม่มี `PlayerId`) ข้าม `GetFrontend`/`/entry` ไปเลย** — flow ไปที่
   `NPAGetUser → FadeOutPrologue` ตรง ๆ (ดู `TitleMenuGroup.cs` line ~918) ⇒ flag ที่ต้องมีผล **ก่อน**
   ฉากสร้างตัวละครต้องส่งผ่าน `/knock` (จุดแรกสุดที่ทุกคนเจอเหมือนกัน) ไม่ใช่แค่ `/entry`
2. **`server.txt` ในชุดแจก ห้ามเขียนด้วย `Set-Content -Encoding UTF8`** ของ PowerShell 5.1 — ใส่ BOM เสมอ
   ทำให้ `เล่นเกม.bat` (อ่านด้วย `for /f` ของ cmd.exe) ได้ค่า IP เพี้ยน เกม throw `UriFormatException`
   เงียบ ๆ แล้วค้างหน้าไตเติ้ล — ต้องใช้ `[System.IO.File]::WriteAllLines(path, lines, [System.Text.UTF8Encoding]::new($false))` แทน (ดู `tools/package-game.ps1`)
