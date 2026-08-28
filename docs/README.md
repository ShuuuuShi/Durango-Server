# Durango documentation index

เอกสารแยกตามเจ้าของและหน้าที่แล้ว ไม่รวมเอกสารทุกชนิดไว้ที่ root เดียวกัน

## โครงสร้าง

```text
docs/
├── project/       ภาพรวม เป้าหมาย สถาปัตยกรรม roadmap และ changelog
├── mod-system/    SDK, loader, method override และสัญญาระบบม็อด
├── client/        เอกสาร client/Unity และ API ฝั่งผู้เล่น
├── server/        เอกสาร server/gameplay/data/deployment
├── testing/       test plan, acceptance และ release gates
├── operations/    คู่มือเปิด beta, ประกาศ, player guide และ ops
├── reports/       bug report และผลตรวจสอบ
└── design/        mockup และเอกสารออกแบบ
```

## เริ่มอ่านตามงาน

| ต้องการดู | เอกสาร |
|---|---|
| เข้าใจระบบทั้งโปรเจกต์ | [project/ARCHITECTURE.md](project/ARCHITECTURE.md) |
| เป้าหมายและลำดับงาน | [project/GOAL.md](project/GOAL.md) · [project/ROADMAP.md](project/ROADMAP.md) |
| เขียน server/client mod | [server/Modding.md](server/Modding.md) · [client/Modding.md](client/Modding.md) |
| เปลี่ยน method ในเกม | [mod-system/MethodOverrides.md](mod-system/MethodOverrides.md) |
| เปลี่ยนตัวละคร/โมเดล/texture/AssetBundle | [client/RenderMods.md](client/RenderMods.md) |
| ดู API/class ที่ถอดจาก client | [client/INDEX.md](client/INDEX.md) |
| ดูระบบ server และ gameplay | [server/README.md](server/README.md) |
| ทดสอบก่อนปล่อย | [testing/TESTPLAN.md](testing/TESTPLAN.md) · [testing/BETA-1.0-PLAN.md](testing/BETA-1.0-PLAN.md) |
| เปิด server / ดูแลผู้เล่น | [operations/BETA-OPS.md](operations/BETA-OPS.md) |
| ดูบั๊กและรายงานตรวจ | [reports/bug-report-memorybot-beta.md](reports/bug-report-memorybot-beta.md) |

## หลักการจัดเอกสาร

- `project/` คือเอกสารตัดสินใจและภาพรวมที่ใช้ร่วมกันทั้งระบบ
- `mod-system/` คือเอกสารของ framework ม็อดโดยตรง
- `client/` และ `server/` คือเอกสารตาม runtime ที่รับผิดชอบ
- `testing/`, `operations/`, `reports/` แยกตามวงจรใช้งาน ไม่ปะปนกับ reference ของโค้ด
- เอกสาร auto-generated ของ client ยังอยู่ใน `client/` เพราะต้องอ้างอิง path และ index เดิมของซอร์ส
