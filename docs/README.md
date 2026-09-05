# Durango documentation index

เอกสารแยกตามเจ้าของและหน้าที่แล้ว ไม่รวมเอกสารทุกชนิดไว้ที่ root เดียวกัน

## โครงสร้าง

```text
docs/
├── Plan/          สเปกเปิดตัว + Roadmap ปัจจุบัน
├── project/       สถาปัตยกรรม changelog capability
├── mod-system/    SDK, loader, method override และสัญญาระบบม็อด
├── client/        เอกสาร client/Unity และ API ฝั่งผู้เล่น
├── server/        เอกสาร server/gameplay/data/deployment
├── testing/       test plan และ release gates
├── operations/    คู่มือเปิด beta และ ops
├── reports/       รายงานตรวจ (ถ้ามี)
└── design/        mockup และเอกสารออกแบบ
```

## เริ่มอ่านตามงาน

| ต้องการดู | เอกสาร |
|---|---|
| Roadmap ปัจจุบัน (สเปกเปิดตัว) | [Plan/ROADMAP-LAUNCH-SPEC.md](Plan/ROADMAP-LAUNCH-SPEC.md) |
| สเปกเกมเพลย์เปิดตัว | [Plan/Durango-Beta-1.0.0.md](Plan/Durango-Beta-1.0.0.md) |
| เข้าใจระบบทั้งโปรเจกต์ | [project/ARCHITECTURE.md](project/ARCHITECTURE.md) |
| เขียน server/client mod | [server/Modding.md](server/Modding.md) · [client/Modding.md](client/Modding.md) |
| เปลี่ยน method ในเกม | [mod-system/MethodOverrides.md](mod-system/MethodOverrides.md) |
| เปลี่ยนตัวละคร/โมเดล/texture/AssetBundle | [client/RenderMods.md](client/RenderMods.md) |
| ดู API/class ที่ถอดจาก client | [client/INDEX.md](client/INDEX.md) |
| ดูระบบ server และ gameplay | [server/README.md](server/README.md) |
| ทดสอบก่อนปล่อย | [testing/TESTPLAN.md](testing/TESTPLAN.md) · [testing/BETA-1.0-PLAN.md](testing/BETA-1.0-PLAN.md) |
| เปิด server / ดูแลผู้เล่น | [operations/BETA-OPS.md](operations/BETA-OPS.md) |

## หลักการจัดเอกสาร

- `Plan/` คือสเปกเปิดตัวและ Roadmap ที่ใช้นำงานปัจจุบัน
- `project/` คือสถาปัตยกรรมและประวัติระบบ
- `mod-system/` คือเอกสารของ framework ม็อดโดยตรง
- `client/` และ `server/` คือเอกสารตาม runtime ที่รับผิดชอบ
- `testing/`, `operations/`, `reports/` แยกตามวงจรใช้งาน ไม่ปะปนกับ reference ของโค้ด
- เอกสาร auto-generated ของ client ยังอยู่ใน `client/` เพราะต้องอ้างอิง path และ index เดิมของซอร์ส