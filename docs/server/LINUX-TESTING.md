# เทสเซิร์ฟเวอร์บน Linux (2026-08-21)

> **ตั้งแต่วันนี้เป็นต้นไป ให้เทสเซิร์ฟเวอร์บนเครื่อง Linux 192.168.1.34 แทนเครื่อง Windows**
> เพราะเป็นสภาพแวดล้อมเดียวกับที่ผู้เล่นจริงจะได้ใช้ (และยืนยันแล้วว่า bind พอร์ต/เซฟ/POI
> ทำงานบน Linux ได้)

## เครื่องทดสอบ

| รายการ | ค่า |
|---|---|
| IP (LAN) | 192.168.1.34 |
| OS | Ubuntu 26.04 LTS (kernel 7.0) |
| SSH | `ssh vibespell@192.168.1.34` (pass: 2526, sudo: 2526) |
| ทรัพยากร | 32 GB RAM · 40 core · พื้นที่ว่าง 49 GB |
| .NET | ไม่ได้ติด (ใช้ publish แบบ self-contained) |

⚠️ ฝั่ง Windows ใช้ plink/pscp (ดาวน์โหลดไว้ที่ `%TEMP%\opencode\`) ต่อ SSH ได้เลย
```powershell
$HK = "SHA256:W3xLumzyu1vNis6GatD9is1iRQYxBF/vF05QmvJn7Q4"
& "$env:TEMP\opencode\plink.exe" -batch -hostkey $HK -pw 2526 vibespell@192.168.1.34 "<คำสั่ง>"
```

## โครงสร้างบนเครื่อง

```
/home/vibespell/durango/
├── linux-x64/            # publish self-contained (ตัวที่รันจริง)
├── data/                 # data ชุดทดสอบ (config/islands/terrains) — ยังไม่ใช้
├── saves/                # saves ชุดทดสอบ — ยังไม่ใช้
├── AssetBundles/         # asset bundle ของเกม (server serve ผ่าน /assetbundles/*)
├── korepilot-test/       # data+saves ชุดโลกจริงที่ใช้รัน (มีผู้เล่นอยู่แล้ว)
│   ├── data/  saves/  server/  backups/
├── server.log            # log เซิร์ฟ (root เป็นเจ้าของ → ต้องรันผ่าน sudo)
```

**data ที่รันจริง:** `korepilot-test/data` + `korepilot-test/saves` (โลกเดิมของผู้เล่น ไม่ใช่ชุดทดสอบ)

## คำสั่งที่ใช้บ่อย

### Build + publish ใหม่ (ฝั่ง Windows)
```powershell
# ที่ C:\Users\thana\Desktop\Durango Opencode\server
dotnet build -c Debug                      # เช็ค compile ก่อน
dotnet publish -c Release -r linux-x64 --self-contained true -o "..\publish\linux-x64"
```

### อัปโหลด build ใหม่ (ทับโฟลเดอร์เดิม)
```powershell
& "$env:TEMP\opencode\pscp.exe" -batch -hostkey $HK -pw 2526 -r `
  "C:\Users\thana\Desktop\Durango Opencode\publish\linux-x64" `
  vibespell@192.168.1.34:/home/vibespell/durango/
```

### Restart เซิร์ฟ
```bash
# 1) ฆ่าเซิร์ฟเดิม — ระวัง: ห้าม pkill -f DurangoServer ในคำสั่งเดียวกับตัว start
#    (pattern จับ command line ของ bash -c เอง → ฆ่าตัวเองตาย)
echo 2526 | sudo -S pkill -9 -f DurangoServer

# 2) รันใหม่ (log เป็น root → ต้องรันผ่าน sudo)
echo 2526 | sudo -S bash -c 'cd /home/vibespell/durango/linux-x64 && \
  nohup ./DurangoServer \
  --data /home/vibespell/durango/korepilot-test/data \
  --saves /home/vibespell/durango/korepilot-test/saves \
  --assetbundles /home/vibespell/durango/AssetBundles \
  --public-host 192.168.1.34 --enable-cheat --admin gm --region-role Rural \
  > /home/vibespell/durango/server.log 2>&1 &'

# 3) เช็ค log
tail -30 /home/vibespell/durango/server.log
# สำเร็จต้องเห็น: [gameserver] listening on 0.0.0.0:8191
#                [gateway] listening on http://*:8190/  และ  server running.
```

### เปิดเกมจากเครื่อง Windows ต่อเข้าเซิร์ฟ Linux
```powershell
powershell -File tools\connect-game.ps1 -Ip 192.168.1.34
# เช็คฝั่งเซิร์ฟ: grep "player joined" /home/vibespell/durango/server.log
```

## พอร์ต

| พอร์ต | ใช้ | หมายเหตุบน Linux |
|---|---|---|
| 8190 TCP | Gateway HTTP + /assetbundles + /reports | bind `*:8190` ได้ **ไม่ต้อง root** (ต่างจาก Windows ที่ fallback loopback) |
| 8191 TCP | GameServer | bind 0.0.0.0 |
| 8191 UDP | knock (server list) | |
| 8192 TCP | Radiotower (แชท) | ปิดอยู่ (`--radiotower` ถึงจะเปิด) |

## Checklist สิ่งที่ต้องเทสบน Linux (สถานะล่าสุด)

- [x] ต่อเกมข้ามเครื่อง (Windows → Linux) — player ฟหกฟหก เข้า/ออกได้
- [x] Gateway HTTP /knock ตอบข้าม LAN
- [x] POI ธรรมชาติถูกวางบนโลก (ชุดใกล้จุดเกิด: warp_accelerator (55,172) + camp_warphole (60,186) + dock (67,170) + ชุดไกล 6 จุดจาก build ก่อนหน้า)
- [x] ระบบเควสเปิด (config `"Quests": true`) — มีเมนูเควส
- [x] ไดโนเสาร์กระจายทั่วแผนที่ — 9 โซน (4 ใกล้จุดเกิด + 5 ไกล: ทุ่งหญ้าตะวันตก/ชายป่าตะวันออก/ที่ราบสูงเหนือ/ทุ่งไกล/หุบแร็ปเตอร์ไกล) + ห่างกันอย่างน้อย 4 tile (MinSeparationTiles) ไม่จับกลุ่ม
- [ ] **สแกนหลุม (SearchWarphole)** — ⚠️ crash เดิมเกิดจากสแกนเจอ 0 จุด (POI ไกลเกินรัศมี) — วาง POI ใกล้จุดเกิดแล้ว ต้องเทสซ้ำ
- [ ] สายเนื้อเรื่อง (Epic → เนื้อเรื่อง) แสดง 12 บท
- [ ] ปุ่มรายงานบัค → ไฟล์ใน `korepilot-test/data/reports/`
- [ ] หน้าเควสไม่โหลดค้าง (GetQuestScoreInfos ตอบแล้ว) + รายการแสดง 10 เควส
- [ ] คราฟต์กรองตามเลเวล
- [ ] วาร์ป POI / WarpBack / WarpToPort

## เหตุการณ์ที่บันทึกไว้

### 2026-08-21 — เกม crash ตอนกดสแกนหลุม
- server ตอบ `[poi] ค้นหาหลุม เจอ 0 จุด` (ผู้เล่นอยู่ไกลจาก POI — โลกตอนนั้นมี artifact เดียว)
- client crash — ยังหาจุด crash แน่ชัดไม่ได้ (output_log มีแต่ SocketException ตอน boot เกมซ้อน)
- **สันนิษฐานแรก:** เกี่ยวข้องกับผลลัพธ์ 0 จุดหรือ build เก่า — รอเทสซ้ำกับ build ใหม่
- **หมายเหตุ:** client บนเครื่องนี้เป็น NVIDIA GeForce 210 (VRAM 972 MB) — การ์ดเก่า เกมรันได้แต่คับขัน

### ข้อผิดพลาดที่เจอระหว่าง deploy (กันเจอซ้ำ)
1. `pkill -f DurangoServer` ฆ่าตัวเอง (command line ของ bash -c มีคำว่า DurangoServer) → แยก kill กับ start คนละคำสั่ง
2. `server.log` ตกเป็น root หลังรันผ่าน sudo → ต้องรันผ่าน sudo ทุกครั้ง (หรือ chown กลับ)
3. `pscp` ไม่ expand `~` ใน path ปลายทาง → ใช้ `/home/vibespell/...` เต็ม
4. `publish/linux-x64` เก่า — อัปโหลดทับแล้วต้อง restart จริง (เช็คด้วย `[world] วาง POI ธรรมชาติ` ว่ามี build ใหม่)