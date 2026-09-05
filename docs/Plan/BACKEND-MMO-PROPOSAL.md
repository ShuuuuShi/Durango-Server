# ข้อเสนอ: backend MMO ให้ใกล้ของเดิมที่สุด + ผลตรวจแพ็กเก็ต + เลเวลเกาะแรก

เขียน: 4 ก.ย. 2026 · สถานะ: **ข้อเสนอ รอเจ้าของตัดสินใจ** · ผูกกับ `ROADMAP-LAUNCH-SPEC.md` (S1–S8)

วิธีตรวจ: `python server/scripts/generate_protocol_inventory.py` (นับ message ที่ generate + ที่เซิร์ฟ `Recv<>`)
แล้วสแกน `client/**/*.cs` หา message ที่ตัวเกมส่งจริง (`Send(new X …)`) มาเทียบกับที่เซิร์ฟรับ

---

## 1. แพ็กเก็ตที่ยังไม่ได้เชื่อมเซิร์ฟ

| ตัวเลข | ค่า |
|---|---|
| message ทั้งหมดที่มี TypeCode (โปรโตคอลเกมเดิม) | 852 |
| message ที่ **ตัวเกมส่งหาเซิร์ฟจริง** (client→server) | 283 ชนิด |
| ที่เซิร์ฟลงทะเบียนรับ (`Recv<>`) | 250 |
| **ตัวเกมส่งแต่เซิร์ฟไม่รับ** | **141 ชนิด** |

เซิร์ฟตอบ `Abort`/เงียบ ⇒ ปุ่มในเกมกดแล้วไม่เกิดอะไร แยกตามระบบ + จับคู่กับ roadmap:

| ระบบ | จำนวน | ตัวอย่าง message | อยู่เฟสไหน |
|---|---|---|---|
| ตาย / ชุบชีวิต | 3 | `Resurrect` `ConfirmResurrection` `SetResurrectionRewards` | **S1** (แกนเล่น) |
| คราฟต์ / ไอเทม | 8 | `CancelCrafting` `SkipPostprocess` `DeliverItems` `GetReceivedItems` (ฝากคราฟต์) `SetRecipeLike` | **S2** |
| ภารกิจ / สอนเล่น | 13 | `TutorialEvent` `SkipTutorialMission` `ParticipateTutorialBoat` `PutMaterialsIntoTutorialBoat` `CheckSequenceMissionCleared` `CustomQuestEvent` `Accept/RefuseSuggestion` | **S2** (ลูปชั่วโมงแรก) |
| สิ่งปลูกสร้าง / ซ่อม | 13 | `RepairArtifact` `CompleteArtifact` `CapsulateArtifact` `SetArtifactAccess` `ExtendFloor` `Fire/ExtinguishBurnable` `Sprinkle` `GrowRapidly` | **S3** (ที่ดิน) |
| คลัง / โกดัง | 10 | `GetWarehouse` `AddItemsToWarehouse` `MakeSection` … | S3 (ถ้าเปิดโกดังบนที่ดิน) |
| วิจัย / สกิลพิเศษ | 6 | `StartPersonalResearch` `DrawActiveSkill` `ChargeEffect` `TakeEffect` | S4 |
| เกาะ / วาร์ป / เดินทาง | 19 | `GetRegion` `GetRoutes` `TravelToStableRegion` `SailingBack` `WarpToUrbanRegion` `RequestNearestPOI` `OpenMap` | **S6** (เกาะไม่เสถียร) |
| สัตว์เลี้ยง / เชือกจูง | 20 | `SpawnPet` `Feeding` `PutInReinsToCage` `StartPetTask` … | S7 |
| แคลน | 7 | `SetClanInfo` `SetClanMemberRole` `Block/Unblock` | หลัง S8 (เลื่อน) |
| โซเชียล / แชท | 6 | `RadioTalk` (ไปทางพอร์ต radiotower แล้ว) `GetLatestChatLog` `KickVisitor` `SetSocialOptions` | S3/S8 |
| ร้านค้า / เงินสด / คูปอง | 11 | `PurchaseCommodity` `AcceptTENCoupon` `RequestTechSupport` | **ตัด** (ไม่มีเงินสด) |
| คอนเสิร์ต / มินิเกม | 12 | `HostConcert` `MiniGameDanceScore` `Scribble` | **ตัด** |
| ยานพาหนะ / บอลลูน | 4 | `MountVehicle` `MountAirBalloon` | **ตัด** (Second Wave) |
| อื่น ๆ | 9 | `Weather` `DiscoverAnimal` `InteractWithEpicNPC` `SuggestBreak` `RepairImmediate` | ดูรายตัว |

**อ่านผล:** ในสโคปเปิดตัว (S1–S8) เหลือประมาณ **95 ชนิด**; ที่ตัดทิ้งได้เลย 27 (ร้านค้า/คอนเสิร์ต/พาหนะ); แคลน 7 เลื่อน
ไฟล์เต็ม: `docs/server/protocol-inventory.json` (รันสคริปต์ใหม่ทุกครั้งก่อนปล่อยเวอร์ชัน)

---

## 2. ออกแบบ backend ให้ใกล้ของเดิม

### 2.1 ของเดิม (อ่านจากตัว client) vs ของเราตอนนี้

| ชิ้น | เกมเดิม (NEXON) | ของเราตอนนี้ |
|---|---|---|
| Gateway (HTTP) | `/knock` `/entry` `/sessions` `/players` `/assets/*` + launcher | มีครบ ใน `Gateway.cs` process เดียวกับเกาะ |
| Cluster | `cluster_mode=Online` ผู้เล่นย้ายเกาะข้าม process ได้ | `--cluster-mode Online` เปิดแล้ว แต่ **ยังไม่มีตัวประสานระหว่างเกาะ** |
| Region server | 1 เกาะ = 1 process, เกาะไม่เสถียรเกิด/ดับได้ | 1 เกาะ = 1 process (`--island`) ทำแล้ว · ยังไม่มีเกิด/ดับอัตโนมัติ |
| Radiotower (แชท/ปาร์ตี้) | process แยก | มีแล้ว (`--radiotower-port`, auth ด้วย session token) |
| ข้อมูลเกม | TextAsset JSON ชุดเดียว ใช้ทั้ง client และ server | client อ่าน JSON จาก `/assets/*` **แต่เซิร์ฟใช้ตาราง C# ที่สกัดไว้ 28,185 บรรทัด (30 ไฟล์)** |
| เซฟ | DB กลาง | JSON ต่อคน (`saves/players/`) ทุกเกาะเขียนไฟล์เดียวกัน |

### 2.2 หลักการที่เสนอ (เรียงตามสำคัญ)

**ก. ข้อมูลเกมอ่านจาก JSON ตอนรัน — ชุดเดียวกับที่ส่งให้ client** ← ตามที่เจ้าของสั่ง "รายการคราฟต์ต้องอ่านจาก JSON"
- บั๊ก "ตัดแกน" (3 ก.ย.) คือผลตรงของการมีข้อมูลสองก๊อป: client เลือกดาบหินได้ตาม `recipes.json` แต่ตาราง C# ของเซิร์ฟสกัดจาก dump คนละรุ่น
- ทำ `GameData` ตัวเดียว โหลด `data/assets/**/*.json` ตอนสตาร์ท (แคชในหน่วยความจำ, hot-reload ได้แบบ config)
  แล้วให้โค้ดเดิมเรียกผ่าน interface เดิม (`RecipeMeta.TryGet`, `RecipeRequirements.Slots`, `ItemTagData.For`, …) — โค้ดเกมเพลย์ไม่ต้องแก้
- ลำดับย้าย: **สูตรคราฟต์ก่อน** (`recipes.json` → RecipeMeta + RecipeRequirements + RecipeData ชื่อ/หมวด/ไอคอน) → tag/ชื่อไอเทม (`prototype_data.json`) → พิมพ์เขียว (`blueprints.json`) → สกิล/ปลดล็อก (`skills.json`, `rewards.json`) → template เกาะ → อาหาร
- ตาราง C# เก็บไว้เป็น fallback + `--data-check` เทียบสองข้างจนกว่าจะเท่ากัน 100% แล้วค่อยลบ
- ข้อยกเว้น: `RecipeGateData` (required_ability) ใน JSON ที่ถอดมา **เสีย** (692/720 เป็นค่าพัง) ต้องใช้ตาราง C# ต่อจนกว่าจะถอดใหม่

**ข. โครง process เหมือนเดิม: Gateway + Coordinator + Region×N + Radiotower**
- `Gateway`: บัญชี/session/entry/assets/launcher/admin — **แยก process** ออกจากเกาะ (ตอนนี้ปนกับเกาะ isle01 ⇒ รีเกาะ = ผู้เล่นทุกเกาะหลุด)
- `Coordinator` (ใหม่, เล็ก): ทะเบียนเกาะที่รันอยู่ (id, พอร์ต, เลเวล, คนออนไลน์), ออก "ตั๋วเดินทาง" ให้ผู้เล่นย้ายเกาะ, ล็อกเซฟตัวละครให้เกาะเดียวเขียนได้ตอนหนึ่ง, สั่งเกิด/ดับเกาะไม่เสถียร (S6)
- `Region`: ตัวที่มีอยู่ (`--island`) ต่อ Coordinator ด้วย token · แพ็กเก็ตกลุ่ม "เดินทาง" (`GetRoutes` `TravelToStableRegion` `SailingBack`) = ขอตั๋วจาก Coordinator แล้วตอบ client ให้ต่อพอร์ตเกาะใหม่ (client ทำได้อยู่แล้วเพราะ `ClusterMode == Online`)
- `Radiotower`: คงเดิม ต่อ Coordinator เพื่อรู้ว่าใครอยู่เกาะไหน (ปาร์ตี้/กระซิบข้ามเกาะ)

**ค. เซฟ: ยัง JSON ได้ แต่ต้องมีเจ้าของไฟล์คนเดียว**
- ตอนนี้ 2 เกาะเขียน `saves/players/<id>.json` ได้พร้อมกัน ⇒ ของหาย/ย้อนได้ตอนย้ายเกาะ
- ขั้นแรก: Coordinator ถือ lock ต่อผู้เล่น (เกาะขอ lock ก่อน load, คืนตอน save สุดท้าย) — ไม่ต้องเปลี่ยน format
- ขั้นถัดไป (คนเยอะ): ย้ายเป็น SQLite ไฟล์เดียวที่ Coordinator (ตาราง players/inventory/estates) เกาะติดต่อผ่าน HTTP ภายใน

**ง. gating ด้วย `Features` ต่อไป แต่ให้ตอบ client ถูกชนิด**
- message ที่อยู่ในกลุ่ม "ตัด" 27 ชนิด: ตอบ `Abort` + `Info` ข้อความไทยว่า "ยังไม่เปิดในรุ่นนี้" แทนเงียบ (ตอนนี้บางตัว client ค้างรอ)
- ทำตาราง `MessagePolicy` (message → เฟส/Feature) ให้ `--data-check` รายงานว่าเฟสนี้ยังขาดตัวไหน

### 2.3 ลำดับทำ (ผูก roadmap)

| ขั้น | งาน | ผล |
|---|---|---|
| 1 (S1, 1–2 วัน) | `GameData` โหลด `recipes.json` แทน RecipeMeta/RecipeRequirements/RecipeData + `--data-check` เทียบ | สูตรคราฟต์ตรง client 100% ตลอดไป |
| 2 (S1) | แพ็กเก็ตตาย/ชุบชีวิต 3 ตัว + `CancelCrafting` | ปุ่มพื้นฐานไม่เงียบ |
| 3 (S2) | Tutorial/Quest 13 ตัว + ฝากคราฟต์ | ลูปชั่วโมงแรกครบ |
| 4 (S3) | แยก Gateway ออกเป็น process + Coordinator lock เซฟ | รีเกาะไม่ทำให้หลุดทั้งเซิร์ฟ |
| 5 (S3) | Artifact 13 ตัว (ซ่อม/แคปซูล/สิทธิ์) + โกดัง 10 | ที่ดินใช้งานจริง |
| 6 (S6) | Coordinator ออกตั๋วเดินทาง + เกาะไม่เสถียรเกิด/ดับ + แพ็กเก็ตเดินทาง 19 | เดินทางในเกมได้ |
| 7 (S7) | สัตว์เลี้ยง 20 | |
| 8 (หลัง S8) | แคลน 7 | |

---

## 3. เลเวลเกาะแรกสูงเกินไป — เพราะอะไร แก้ยังไง

**ต้นเหตุ:** message `Region` ที่เซิร์ฟส่ง **ไม่มีช่อง Level** — ตัวเกมคำนวณเองจาก `TemplateId`
(`client/Durango.Logic.Explore/Region.cs`: `Level => Template.Level`) เซิร์ฟส่ง `TemplateId = ri35teSub01`
(จาก `terrains/ri35te/info.yml`) ⇒ ทุกหน้าจอโชว์ **Lv.35** ทั้งที่ของจริงบนเซิร์ฟคือสัตว์ Lv1–10, `RegionLevel = 1`, `ResourceLevel = 0`

**Lv.35 ปลอมนี้กระทบอะไรใน client:**
- หน้าโหลด / แผนที่โลก / โปรไฟล์ โชว์ "Lv.35" — ผู้เล่นใหม่คิดว่ามาผิดเกาะ
- `CombatInspector`: สัตว์ที่เลเวลต่ำกว่าเกาะขึ้นไอคอน "ง่าย" ⇒ **แร็ปเตอร์ Lv10 ก็ขึ้นว่าง่าย**
- `PlayGuide` เงื่อนไข `_level <= Region.Level` ⇒ คำแนะนำของเกาะสูงโผล่ตั้งแต่เกาะแรก
- `ExploreSystem` จัดกลุ่มเกาะที่เคยไปตามเลเวล/ไบโอม

**ทางเลือก**

| # | วิธี | ทำอะไร | ข้อดี / ข้อเสีย |
|---|---|---|---|
| **1 (แนะนำ ทำได้เลย)** | ส่ง `TemplateId` เลเวลต่ำแทน | เพิ่มช่อง `Template` ในทะเบียนเกาะ (`data/islands.json` / `IslandRegistry`) ให้ isle01 = `ru10gr170511` (Lv10 ชนบท) หรือ `sh05tr…` (Lv5) แล้ว `GameServer.cs:682` ใช้ค่านี้ก่อน `info.yml` · ตั้ง `Survival.RegionLevel` ของ isle01 = 10 ให้สูตรความล้าตรงกัน | ไม่รีเซ็ตโลก ไม่แตะ client แก้ ~20 บรรทัด + รีสตาร์ต 1 ครั้ง · ข้อเสีย: ชื่อไบโอมบนหน้าจอเป็น "ทุ่งหญ้า" ทั้งที่แผนที่เป็นเขตอบอุ่น (แค่ข้อความ) |
| 2 | ใช้ terrain เลเวลต่ำจริง | มี `pe10gr_1..5` (Lv10 ทุ่งหญ้า) ถอดไว้แล้ว 5 ใบ หรือถอด `ru10gr`/`sh05tr` จากเกม | ตรงของเดิมที่สุด (เกมเดิม: สอนเล่น → เกาะ Lv5–10 → Lv35) · **ต้องรีเซ็ตโลก** เพราะบ้านผู้เล่นอยู่บนแผนที่เดิม ⇒ ทำตอนเปิด "เกาะเริ่มต้นใหม่" isle00 ใน S6 แล้วให้ isle01 (Lv35) เป็นเกาะถัดไปที่ต้องเลเวล ≥ 25 |
| 3 | ปรับเลขข้างเซิร์ฟให้เข้ากับ 35 | ยกสัตว์/วัสดุขึ้นเป็น Lv30–40 | ผู้เล่นใหม่ตายหมด — ไม่แนะนำ |

**เสนอ:** ทำข้อ 1 ทันที (รวมกับรีสตาร์ตรอบแก้สูตร "ตัดแกน") และวางข้อ 2 ไว้ใน S6 เป็นระบบเกาะตามเลเวลจริง
ถ้าเลือก Lv10: สัตว์ Lv1–4 ขึ้น "ง่าย", Lv5–10 ขึ้นปกติ, แร็ปเตอร์ Lv7–10 ไม่โดนตีตราว่าง่ายอีก

---

## 3.5 ของจริงของ Nexon (What!Studio) จาก NDC 2014/2016/2018 — แล้วเราปรับตามได้แค่ไหน

แหล่ง: [NDC2018 สรุปเซสชัน (winterjung)](https://www.winterjung.dev/ndc2018-durango/) · [NoSQL บน MMORPG (gist)](https://gist.github.com/nayong/3c0d109118a55f0711309465bc02643f) ·
สไลด์ [Vol.2 (NDC16)](https://www.slideshare.net/sublee/lt-vol-2) · [Vol.3 (NDC18)](https://www.slideshare.net/slideshow/vol-3-95472828/95472828) · [SPOF-free (NDC14)](https://www.slideshare.net/slideshow/spof-mmorpg/35288078) · [ข่าว thisisgame](https://www.thisisgame.com/webzine/gameevent/nboard/227/?n=82259)

| หัวข้อ | ของ Nexon จริง | ของเราตอนนี้ | ปรับได้ / ทำตาม |
|---|---|---|---|
| ภาษา/รันไทม์ | **Python** (เครื่องมือ deploy = Fabric, Ansible, boto3) | C# .NET 9 | ไม่ต้องตาม — ภาษาไม่ใช่สาระ โปรโตคอลเหมือนกันพอ |
| ฐานข้อมูล | **Couchbase (NoSQL key-value, 1 entity = 1 document, schemaless)** + Redis + MySQL + Elasticsearch เสริม | JSON 1 ไฟล์ต่อผู้เล่น (= 1 document เหมือนกัน) | **ตรงหลักการอยู่แล้ว** — สิ่งที่ขาดคือ CAS/lock กันสองเกาะเขียนพร้อมกัน (ข้อ ค.) |
| ความสอดคล้อง | BASE ไม่ใช่ ACID · **CAS key** (เวอร์ชัน document กันเขียนทับ) · **promise document** สำหรับงานข้ามโหนด (pending→committed) เช่นส่งของข้ามเกาะ | เขียนทับตรง ๆ | ทำตาม: ใส่ `Version` ใน PlayerSave + Coordinator ถือ lock · ย้ายเกาะ/ส่งของข้ามเกาะใช้ promise (บันทึกก่อน ทำ ยืนยัน) |
| โลก | **archipelago ไม่มี channel** · เกาะมี "ความจุประชากร" · เกาะผูกกับ shard แบบ static | 1 เกาะ = 1 process · ไม่มี channel เหมือนกัน | **ตรงอยู่แล้ว** — เพิ่มความจุต่อเกาะ + ทะเบียนเกาะ→process ที่ Coordinator |
| ภายในเกาะ | โลกหั่นเป็น **chunk 30m×30m** · หลายโหนดถือ chunk ร่วมกันแบบ N:M · sync ด้วย RPC + Pub/Sub · ผู้เล่นบนโหนดอื่นเห็นเป็น **ghost** | chunk เหมือนกัน (`ChunkSendRange`) แต่ 1 เกาะ = 1 โหนดเสมอ | ยังไม่ต้อง — คนต่อเกาะ ≤ 50 โหนดเดียวพอ (เกมเดิมมีคนเป็นแสน) ออกแบบให้ ServerWorld ไม่ผูกกับ "ทั้งเกาะ" ไว้เผื่ออนาคต |
| service | player/login/property/combat แยกเป็น service หลายโหนด **ไม่มี SPOF** · คุยกัน P2P · **etcd** เป็นตัวตกลง (consensus) | ทุกอย่างอยู่ใน process เกาะ · Gateway ปนอยู่กับ isle01 | ทำตามแบบย่อ: Gateway แยก process + Coordinator (แทน etcd) — ยอมมี SPOF เดียวคือ Coordinator เพราะขนาดเรา |
| shard | cluster แบ่งเป็น shard กัน connection ล้น · อัปเดตแบบ rolling (ปิดครึ่ง shard ทีละครึ่ง = แพตช์ไม่ต้อง downtime) | รี = หลุดทั้งเซิร์ฟ | ทำตามได้เมื่อแยก Gateway: รีเกาะทีละเกาะ ผู้เล่นเกาะอื่นไม่หลุด |
| ข้อความระหว่างโหนด | zeromq (เจอบั๊ก libzmq #2942 ล่มเป็นลูกโซ่ตอนเปิดตัว) | ไม่มี | ใช้ HTTP/JSON ภายในพอ (โหนดน้อย) |
| deploy | Terraform + Ansible + Packer + GitLab CI · เครื่องมือ CLI สร้าง EC2 เอง | `tools/deploy-vps.sh` + systemd | เพิ่ม: unit ต่อเกาะ (`durango@isle01.service`) + สคริปต์ rolling |
| บทเรียนเปิดตัว | พยายาม "เซิร์ฟเดียวรับทุกคน" แล้วล้ม แต่ได้ความจุต่อเซิร์ฟสูงกว่า MMO ทั่วไป · ปัญหาหลัก = ระบบกระจายซับซ้อน + บั๊กไลบรารี | เซิร์ฟเดียว 50 คน | อย่าทำระบบกระจายเกินขนาดจริง — Coordinator เล็ก ๆ พอ |

**สรุปสิ่งที่ปรับตามของเขา (เพิ่มเข้าแผนขั้น 4/6):** (1) `Version` + CAS ใน PlayerSave (2) promise document สำหรับย้ายเกาะ/ส่งของข้ามเกาะ (3) ความจุต่อเกาะ + ทะเบียนเกาะที่ Coordinator (4) รี/แพตช์ทีละเกาะ

## 4. สิ่งที่ต้องตัดสินใจ

1. เลเวลเกาะแรก: เอา **Lv10** (`ru10gr`) หรือ **Lv5** (`sh05tr`)?
2. เริ่มขั้น 1 ของ backend (สูตรคราฟต์อ่าน `recipes.json` ตอนรัน) เลยไหม — ใช้เวลา 1–2 วัน ไม่กระทบผู้เล่น (ตารางเดิมเป็น fallback)
3. แยก Gateway ออกจากเกาะ (ขั้น 4) จะทำก่อน S3 หรือรอ — ถ้าจะเปิด isle02 ให้ผู้เล่นจริง ควรทำก่อน
