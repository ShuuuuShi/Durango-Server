# คู่มือติดตั้ง (ไทย)

[English](INSTALL.en.md) · [中文](INSTALL.zh.md)

คู่มือนี้อธิบายวิธีติดตั้งและรันเซิร์ฟเวอร์ Durango ส่วนตัว ตั้งแต่เริ่มจนผู้เล่นเข้าเกมได้

---

## 1. สิ่งที่ต้องมี

| ต้องมี | เวอร์ชัน | ใช้ทำอะไร |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/9.0) | **9.0** | build + รันเซิร์ฟเวอร์ และ test client (Windows / Linux / macOS ได้หมด) |
| [Git](https://git-scm.com/) | ใหม่ล่าสุด | clone โปรเจกต์ |
| RAM / ดิสก์ | ~512 MB / ~200 MB | เซิร์ฟใช้ RAM แค่ ~30–70 MB ตอนรันจริง |

เช็คว่าติดตั้งถูกต้อง:

```bash
dotnet --version    # ต้องขึ้นต้นด้วย 9.
git --version
```

## 2. ดาวน์โหลดโปรเจกต์

```bash
git clone https://github.com/ShuuuuShi/Durango-Server.git
cd Durango-Server
```

## 3. เตรียมข้อมูลเทอร์เรน (จากเกมของคุณเอง)

เซิร์ฟเวอร์ **ไม่แถม** ข้อมูลแผนที่ของเกม เพราะเป็นข้อมูลลิขสิทธิ์ของ NEXON — คุณต้องเตรียมจากเกมที่คุณมีอยู่แล้ว:

```text
server/data/terrains/extracted/<island-id>/    ← วางข้อมูลเทอร์เรนของแต่ละเกาะตรงนี้
server/data/gamefiles/                          ← (ถ้าต้องการให้ launcher โหลดแพตช์ได้) ตัวเกม PC
```

เกาะที่ระบบรองรับมี 13 เกาะ เช่น `pe10gr_1`–`pe10gr_5`, `ri35te`, `ri35de`, `ri40tr` ฯลฯ — ดูรายชื่อเต็มได้จาก `server/data/islands.json`

> ถ้าไม่มีโฟลเดอร์ `terrains/extracted/` เซิร์ฟจะยังเปิดได้ แต่ผู้เล่นจะเข้าเกาะที่ไม่มีข้อมูลไม่ได้

## 4. เปิดเซิร์ฟเวอร์

```bash
cd server
dotnet run -- --whitelist data/whitelist.txt
```

ค่าเริ่มต้นที่ได้ (ปลอดภัยทั้งหมด):

| อย่าง | ค่าเริ่มต้น |
|---|---|
| พอร์ตเกม (TCP) | **8191** |
| Gateway (HTTP — สมัครไอดี/หน้าเว็บ) | **8190** |
| Radiotower (TCP) | 8192 = พอร์ตเกม + 1 (เปิดด้วย `--radiotower`) |
| คำสั่งโกง (`Cheat` packet) | ปิด — เปิดด้วย `--enable-cheat` |
| เพดานผู้เล่น | 32 เส้น (IP เดียวไม่เกิน 4) |
| auto-save | ทุก 60 วินาที |

ตัวเลือกสำคัญอื่น ๆ: `--game-port <พอร์ต>`, `--max-connections-per-ip <n>`, `--admin <ชื่อ>` (ดูทั้งหมดใน `server/Program.cs`)

**whitelist** — แนะนำให้เปิดเสมอ: `server/data/whitelist.txt` เขียนบรรทัดละ 1 entity id หรือชื่อตัวละคร (`#` = คอมเมนต์) แก้ไฟล์แล้วใช้ได้เลย **ไม่ต้องรีสตาร์ทเซิร์ฟ**

**config ทั้งหมด** อยู่ที่ `server/data/config.json` — เปิด/ปิดระบบเกมเพลย์ (Farming, Quests, Market, PvP, Android ฯลฯ) ได้หมดในนี้ hot-reload เหมือนกัน

ลายเซ็นว่าเซิร์ฟปกติ — ทุก 30 วินาทีจะพิมพ์บรรทัดสถิติ:

```text
[loop] 120 tps, ผู้เล่นออนไลน์ 3, สัตว์ 34 ตัว (+ซาก 2), RAM 32 MB
```

## 5. ทดสอบด้วย test client

เปิดเทอร์มินัลใหม่:

```bash
cd test-client
dotnet run -- --gp-check        # ชุดทดสอบเกมเพลย์ — ต้องได้ 36/36
dotnet run -- --multi-check     # ผู้เล่นหลายคน — ต้องได้ 9/9
dotnet run -- --estate-check 127.0.0.1 8191 8190   # ระบบที่ดิน — ต้องเปิดเซิร์ฟด้วย --enable-cheat
```

## 6. ม็อด

**ฝั่งเซิร์ฟเวอร์** (`mod-sdk/` — .NET 9): สร้างโปรเจกต์ใหม่ที่ reference `mod-sdk/DurangoModSdk.csproj` implement `IGamePlugin` วาง dll ที่ build แล้วใน `server/data/mods/` — รายละเอียด protocol ดูในซอร์สของ `mod-sdk/`

**ฝั่งเกม** (`client-mod-sdk/` — net35/Unity Mono): ต้องมีไฟล์ `UnityEngine.CoreModule.dll` จากเกมของคุณเอง (แก้ HintPath ใน `DurangoClientModSdk.csproj` ให้ชี้ไปที่ `Durango_Data/Managed/` ของเกม)

**ตัวอย่างม็อดสมบูรณ์:** `tools/MemoryBotMod/` — บอทเดินเก็บของ/คราฟต์/ทำเควสต์เองได้ อ่าน `tools/MemoryBotMod/HOW-TO-DRIVE.md`

## 7. แก้ปัญหาที่เจอบ่อย

| อาการ | สาเหตุ/วิธีแก้ |
|---|---|
| ผู้เล่นต่อไม่ได้ | เช็ค firewall พอร์ต 8191 (TCP) และ 8190 (HTTP) |
| เข้าเกาะแล้วค้าง/แผนที่ว่าง | ไม่มีข้อมูล `data/terrains/extracted/<island-id>` — กลับไปขั้น 3 |
| `--gp-check` ไม่ครบ 36/36 | เซิร์ฟเป็นคนละเวอร์ชันกับ test client — `dotnet build` ทั้งสองโฟลเดอร์ใหม่ |
| log มี `[error]` ซ้ำ ๆ | เซิร์ฟไม่ตาย (มี exception handler) แต่ควรอ่านว่าระบบไหนแล้วเปิด issue |

## 8. ทำต่อ / พัฒนาร่วมกัน

- โครงสร้างโค้ดเซิร์ฟเวอร์อยู่ที่ `server/ServerCore/` — จุดเริ่มคือ `Program.cs` และ `ServerCore/ServerPlayer.Core.cs` (แกนผู้เล่น + handler 32 ตัว)
- Pull request ยินดีเสมอ — ถ้าแก้เกมเพลย์ รัน `--gp-check` ให้ผ่านก่อนส่ง
