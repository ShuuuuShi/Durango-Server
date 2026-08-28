# สวิตช์เปิด/ปิดระบบ — standard beta profile

standard launcher (`tools/menu.ps1`) รัน server จาก `server/` ดังนั้น profile ที่ใช้จริงคือ [`../../server/data/config.json`](../../server/data/config.json). `Program.cs` อ่าน `<--data หรือ current working directory>/config.json`; `--island` จะใช้ `data/islands/<id>/config.json` แทน. `FeatureConfig.Defaults()` เป็น fallback/seed profile ไม่ใช่คำประกาศ rollout ของ standard launcher.

สถานะ rollout อยู่ใน [`S0-FOUNDATION.md`](S0-FOUNDATION.md) และ [`plantServer.md`](plantServer.md). Flag `true` หมายถึง handler เปิดรับ packet; ไม่ได้แปลว่า Stable หรือเปิดให้ผู้เล่นสาธารณะเสมอไป.

## เปิดอยู่ใน standard profile (13 ระบบ)

| ระบบ | flag | หมายเหตุ |
|---|---|---|
| เก็บของจากธรรมชาติ | `Gathering` | ต้องมีเครื่องมือตามชนิดของ |
| คราฟต์ | `Crafting` | ตรวจสูตร/tag/material ฝั่ง server |
| ต่อสู้ | `Combat` | |
| แล่ซาก | `Butchery` | |
| ก่อสร้าง/กล่องเก็บของ | `Building` | material economy ยังไม่ Stable |
| เลเวล + exp | `Progression` | เพดาน 20 |
| สกิล | `Skills` | |
| สวมใส่อุปกรณ์ | `Equipment` | |
| เลือด/สตามินา/ความล้า | `Survival` | |
| ความทนทานเครื่องมือ | `ToolDurability` | |
| แชทช่องรวม | `Chat` | |
| ทำอาหาร | `Cooking` | ต้องใช้ workbench/tool ตามสูตร |
| checklist เควสทีมทดสอบ | `QuestChecklist` | ไม่เปิดสาย quest ทั่วไป |

## ปิดอยู่ใน standard profile (12 ระบบ)

| ระบบ | flag | สภาพ |
|---|---|---|
| เดินทางข้ามเกาะ | `IslandTravel` | Implemented แต่ต้องคงปิดจนผ่าน S2 handoff/reconnect/rollback + real-client evidence |
| อาชีพ | `Jobs` | ไม่มี authoritative subsystem |
| เพาะปลูก/ทำนา | `Farming` | Internal test; เปิดเฉพาะ profile test เมื่อมี acceptance evidence |
| ปศุสัตว์ | `Livestock` | ไม่มี authoritative subsystem |
| จับ/ขี่ไดโน | `Taming` | ไม่มี authoritative subsystem |
| ตลาด | `Market` | ไม่มี ledger/service state |
| Warp Accelerator | `WarpAccelerator` | Internal test; reward/abort/persistence acceptance ยังไม่ครบ |
| เควส 4 กลุ่ม NPC | `Quests` | Internal test; project-authored reward/objective evidence ยังไม่ครบ |
| PK เกาะ 20+ | `Pvp` | policy/anti-grief suite ยังไม่ครบ |
| สิทธิ์ที่ดิน | `LandPermission` | permission model ยังไม่ครบ |
| ปาร์ตี้/แคลน | `PartyAndClan` | ไม่มี authoritative subsystem |
| อีโมติคอนผู้เล่น | `Emotes` | ปิดใน profile นี้ |

## กฎการเปลี่ยน flag

ห้ามเปิด `IslandTravel` ใน standard profile จนกว่า S2 ตาม `plantServer.md` จะผ่าน. สำหรับ feature อื่น ต้องมี server packet gate, UI decision, persistence/anti-abuse coverage และ rollout evidence ก่อนเปลี่ยน profile. การแก้ `server/data/config.json` hot-reload ได้ภายในประมาณ 5 วินาที แต่การเปิด flag ไม่แทน acceptance.

เปิด server แล้วจะพิมพ์ inventory ของ effective config ผ่าน `[feature]` log; ใช้ log นั้นยืนยัน profile ที่ process ใช้จริงเสมอ.

## เพดานเลเวล

`Features.MaxPlayerLevel` ตั้งไว้ `20` ใน standard profile. `0` คือไม่จำกัดภายในเพดานที่ code/data รองรับ.
