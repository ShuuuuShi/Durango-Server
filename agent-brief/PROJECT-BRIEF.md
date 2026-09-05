# PROJECT-BRIEF — Durango Thailand Private Server

> สำหรับ agent ใหม่: อ่านไฟล์นี้ก่อน แล้วตาม Roadmap ปัจจุบัน
> กฎเด็ดขาด: ห้ามแจกโค้ด — อธิบายได้ แต่ห้ามโพสต์/คัดลอก source
> นอก Roadmap = เตือนแล้วพากลับแผน

---

## 1. โปรเจกต์นี้คืออะไร

ชุบชีวิตเกม Durango: Wild Lands (NEXON) ด้วย private server ใหม่
- client: Unity 2017.4.34f1 (decompile)
- server: C# / .NET 9 พูดโปรโตคอลเดิม
- ตำแหน่ง: `C:\Users\thana\Desktop\Durango Opencode`

## 2. ทิศทางปัจจุบัน (ล็อกแล้ว)

**แกนหลัก = สเปกเกมเพลย์เปิดตัว** ไม่ใช่แค่เทสเปิดเกาะดิบ

อ่านตามลำดับ:
1. `docs/Plan/ROADMAP-LAUNCH-SPEC.md` ← แผนงานปัจจุบัน
2. `docs/Plan/Durango-Beta-1.0.0.md` ← สเปกเปิดตัว
3. `HANDOFF-2026-09-01.md` ← กับดักล่าสุดฝั่ง client/mod
4. `docs/testing/BETA-1.0-PLAN.md` ← เกณฑ์เสถียร ใช้ในเฟส S1

เผ่า (clan) = เลื่อนหลัง S8
ตัด: เกาะส่วนตัวถาวร, PvP เต็ม, สงครามเผ่าเต็ม, ของหลัง Second Wave

## 3. เฟสปัจจุบัน

- S0 ล็อกสเปก = ทำแล้ว
- **กำลังทำ: S1 เสถียรแกน** แล้วต่อ **S2 ลูปชั่วโมงแรก**
- ถัดไป: S3 ที่ดิน+T-stone → S4 สกิล 12 สาย → S5 stance → S6 เกาะไม่เสถียร → S7 จับ/ฝึกไดโน → S8 องค์กร+เควสต์

## 4. ทีม

| ชื่อ | หน้าที่ |
|---|---|
| เลขามะลิ | ประสานตาม Roadmap / เตือนนอกแผน |
| วิศวกรเซิร์ฟ | เซิร์ฟ |
| นักทดสอบ | เทส / เกณฑ์ |
| ช่าง Client | client / mod |
| ที่ปรึกษาเกมเพลย์ | ทักท้วงเมื่อหลุดสเปกเปิดตัว |

## 5. วิธีรันสั้น ๆ

- โปรเจกต์: `C:\Users\thana\Desktop\Durango Opencode`
- เปิดเซิร์ฟ (โหมดปลอดภัย): `server` + `--whitelist data/whitelist.txt` (อย่าเปิด `--enable-cheat` ค้างตอนเปิดจริง)
- client ที่เล่นจริง: `dist\DurangoTH-Clean` เปิดด้วย `-durango-updated`
- เทส: `test-client` → `--gp-check` / `--multi-check`
- พอร์ตเทสสดมักใช้ gateway 8290 / game 8291 (ดู handoff ล่าสุด)

## 6. กฎเหล็ก

0. เทสหลัก = เกมจริง + มอด บังคับตัวด้วย MemoryBot เท่านั้น — gp-check ไม่ใช่คำตอบ client

1. ห้ามแจกซอร์ส
2. ห้าม Harmony / method override
3. ห้ามก๊อป DLL ตอนเกมเปิด
4. ห้ามเปิดเกมโดยไม่มี `-durango-updated`
5. ห้ามเปลี่ยน `DurangoClientModSdk.dll` ใน Managed ของเครื่องเล่น
6. นอก Roadmap = เตือนแล้วพากลับ

## 7. ของที่มีเล่นได้แล้ว (ฐาน)

เข้าเซิร์ฟหลายคน · เก็บของ · คราฟต์ · สร้างบ้าน · สกิล · สู้สัตว์ · เซฟ · แชท · ความล้า/หิวคร่าว ๆ · กันโกงหลักผ่าน gp/multi-check รอบก่อน

ยังขาดตามสเปกเปิดตัว: ดูรายการใน `ROADMAP-LAUNCH-SPEC.md`