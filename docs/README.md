# Durango Claude — สารบัญเอกสาร

โปรเจกต์รื้อ **Durango: Wild Lands** (NEXON, ปิดตัวไปแล้ว) ให้กลับมาเล่นออนไลน์ได้ด้วย server ที่เขียนเอง

```
Durango Claude/
├── game/           ตัวเกม Unity 2017.4.34f1 ที่ patch แล้ว (รันอันนี้)
├── game-backup/    สำเนาสำรองของ game/ ก่อน patch
├── server/         DurangoServer — .NET 9, เซิร์ฟเวอร์ที่เขียนเอง  ★ หัวใจของโปรเจกต์
├── client/         ซอร์ส client ที่ถอดจาก Assembly-CSharp.dll (คอมไพล์ผ่านแล้ว)
├── test-client/    client จำลองสำหรับยิง packet ทดสอบ (159 บรรทัด)
└── docs/           ← อยู่ตรงนี้
```

## เริ่มอ่านจากตรงไหน

| อยากรู้ว่า... | อ่านที่ |
|---|---|
| ทั้งระบบต่อกันยังไง เข้าเกมแล้วเกิดอะไรขึ้นบ้าง | **[ARCHITECTURE.md](ARCHITECTURE.md)** ← เริ่มที่นี่ |
| server แต่ละไฟล์ทำอะไร เมทอดไหนทำงานยังไง | [server/README.md](server/README.md) |
| client มีคลาสอะไร เมทอดชื่ออะไร อยู่บรรทัดไหน | [client/INDEX.md](client/INDEX.md) |
| ยังมีบั๊กอะไรค้างอยู่ ต้องทำอะไรต่อ | [../server/GAMEPLAY-REVIEW.md](../server/GAMEPLAY-REVIEW.md) |
| **จะเปิด beta 1.0 ต้องมีอะไรบ้าง ผ่านเกณฑ์หรือยัง** | **[BETA-1.0-PLAN.md](BETA-1.0-PLAN.md)** |
| บั๊กฝั่ง client / offline server เดิมในเกม | [../game/CODE-REVIEW.md](../game/CODE-REVIEW.md) · [../game/ONLINE-REVIEW.md](../game/ONLINE-REVIEW.md) |

## เอกสารสองแบบ ต่างกันตรงไหน

| | `docs/server/` | `docs/client/` |
|---|---|---|
| ที่มา | **เขียนมือ** | **auto-generated จากซอร์ส** |
| ครอบคลุม | 16 ไฟล์ · ~102 เมทอด | 3,760 ไฟล์ · 23,813 เมทอด |
| บอกอะไร | ไฟล์นี้ทำอะไร เมทอดนี้ทำงานยังไง ทีละขั้น + ข้อควรระวัง | ลายเซ็นจริง + เลขบรรทัดจริง + packet ที่ไฟล์นั้นส่ง/รับ |
| อัปเดตยังไง | แก้มือเมื่อโค้ดเปลี่ยน | รันสคริปต์ใหม่ (ดู [client/INDEX.md](client/INDEX.md)) |

> ฝั่ง client เป็นโค้ดที่ถอดจาก dll — **ไม่ใช่ซอร์สต้นฉบับของ NEXON** ชื่อตัวแปรท้องถิ่น (`num`, `flag`, `text2`) เป็นชื่อที่ decompiler ตั้งเอง แต่ชื่อคลาส/เมทอด/ฟิลด์เป็นของจริง

## สถานะตอนนี้

**ทำได้แล้ว** — 2 คนเข้าเซิร์ฟพร้อมกัน เห็นกัน เดินซิงก์ แชท อิโมท เก็บของ คราฟต์ วางสิ่งปลูกสร้าง เรียนสกิล

**ยังไม่ได้ทำ** — ใส่อุปกรณ์ · ค่าสถานะเอาชีวิตรอด · สัตว์/ต่อสู้/ตาย-ฟื้น · เพ็ท · ฟาร์ม · คลังเก็บของ · ปาร์ตี้/แคลน/เพื่อน/เมล · วาร์ป
(server รับ packet ได้ 32 จาก 354 ชนิดที่ client ส่ง — รายละเอียดใน [GAMEPLAY-REVIEW](../server/GAMEPLAY-REVIEW.md))

## รันยังไง

```powershell
# 1) server
cd "server"
dotnet run

# 2) เกม (เปิดได้หลายหน้าต่าง)
cd "..\game"
.\launch.bat
```

จากนั้นในเกมเลือกเซิร์ฟเวอร์ **"Multi Play Server"** (อย่าเลือก "Online Server (For Test)" — โหมดนั้นแชทใช้ไม่ได้ ดูเหตุผลใน GAMEPLAY-REVIEW ข้อ GP-06)

| ฟัง | พอร์ต | ใคร |
|---|---|---|
| TCP 8190 | Gateway HTTP | `/knock` `/sessions` `/entry` `/terrains` `/assetbundles` |
| UDP 8191 | knock discovery | ค้นหาเกาะใน LAN |
| TCP 8191 | GameServer | packet เกมทั้งหมด |
| TCP 8192 | RadiotowerServer | แชท (ยังไม่ถูกใช้จริง — ดู GP-06) |

เล่นข้ามเครื่องต้องเปิด firewall ทั้ง 4 ช่องนี้
