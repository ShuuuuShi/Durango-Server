# Plant Server — Roadmap รองรับเกมครบ 100%

> สถานะ: แผนแม่บท (2026-08-27)  
> เอกสารนี้วางแผนการทำให้เซิร์ฟเวอร์รองรับทุกระบบที่ผู้เล่นเข้าถึงได้จากไคลเอนต์ Durango และใช้เป็นเกณฑ์ก่อนเปิดเมนูให้ผู้เล่นทั่วไป
>
> เอกสารที่เกี่ยวข้อง: [`../project/ROADMAP.md`](../project/ROADMAP.md), [`../testing/TESTPLAN.md`](../testing/TESTPLAN.md), [`Farming.md`](Farming.md), [`Persistence.md`](Persistence.md), [`Islands.md`](Islands.md), [`Quests.md`](Quests.md), [`Building-Audit.md`](Building-Audit.md), [`../project/CAPABILITY-REPORT.md`](../project/CAPABILITY-REPORT.md), [`../operations/BETA-OPS.md`](../operations/BETA-OPS.md)

## 1. ความหมายของ “รองรับครบ 100%”

คำว่า **100%** ในแผนนี้ไม่ได้หมายความว่าได้ source code ของเซิร์ฟเวอร์ต้นฉบับ หรือสามารถยืนยันว่าค่าภายในทุกตัวตรงกับเกมดั้งเดิม เพราะข้อมูลที่ได้จาก client ไม่มี authoritative rules ของหลายระบบ เช่น quest rewards, economy, EXP, AI/spawn และ item semantics บางส่วน

ความหมายที่ตรวจสอบได้คือ **ทุก player-facing flow ที่ไคลเอนต์เปิดให้ใช้งานต้องทำงานครบแบบ end-to-end**:

1. ผู้เล่นเปิดเมนูหรือเริ่ม interaction ได้จากไคลเอนต์จริง
2. เซิร์ฟเวอร์ตรวจสอบสิทธิ์, state, ระยะ, inventory, cooldown และ packet ที่ไม่เชื่อถือทั้งหมด
3. ระบบเปลี่ยน state อย่างเป็น atomic operation หรือมี rollback ที่ชัดเจน
4. save/restart/reconnect แล้วข้อมูลไม่หาย ไม่ซ้ำ และไม่เสียรูปแบบ
5. ผู้เล่นหลายคนพร้อมกันไม่สามารถทำ duplication, bypass ownership หรือรับ reward ซ้ำได้
6. UI, animation, message และ flow บนไคลเอนต์จริงแสดงผลถูกต้อง ไม่ใช่เพียง packet bot ผ่าน
7. ผ่าน automated regression, negative/abuse tests, multi-client test, restart test และ soak test ตามระดับความเสี่ยง

สถานะของ packet หรือ class ใน `GameCode/Messages` เพียงอย่างเดียว **ไม่ถือว่ารองรับระบบนั้นแล้ว** ระบบจะถือว่า complete ได้เมื่อมี authoritative server state, persistence, authorization, test evidence และ real-client validation ครบทั้งหมด

### 1.1 กฎสำหรับข้อมูลที่ไม่พบใน client

เมื่อเกมไม่มี authoritative data ของกฎเดิม ให้สร้างกฎของโครงการเป็น **canonical project-owned data** แยกจาก generated client data และต้องบันทึก:

- แหล่งที่มา: extracted, inferred หรือ project-authored
- เหตุผลและข้อจำกัดของค่าแต่ละชุด
- version/schema ของข้อมูล
- regression test ที่ยืนยันผลลัพธ์
- ความต่างที่ทราบจากเกมต้นฉบับ (ถ้ามี)

ห้ามอ้างว่ากฎที่ inferred หรือ project-authored เป็นกฎ original-server ที่ยืนยันแล้ว

---

## 2. Rollout: เขียนระบบก่อน แล้วเปิดเมนูทีหลังได้

การพัฒนาไม่จำเป็นต้องเปิดทุกระบบให้ผู้เล่นใช้ทันที แต่ต้องมี feature gate ฝั่งเซิร์ฟเวอร์เสมอ การซ่อนปุ่มหรือเมนูใน client อย่างเดียวไม่ใช่มาตรการป้องกัน เพราะ client ที่แก้ไขหรือ packet ที่ส่งโดยตรงยังอาจเรียก handler ได้

### 2.1 สถานะมาตรฐานต่อระบบ

| สถานะ | ความหมาย | ผู้ใช้ที่เข้าถึงได้ |
|---|---|---|
| `Not implemented` | ไม่มี authoritative handler/state หรือยังเป็น protocol/UI เท่านั้น | ไม่มี |
| `Implemented` | มีโค้ดหลัก แต่ยังไม่ผ่าน acceptance ครบ | ไม่มีโดยค่าเริ่มต้น |
| `Internal test` | เปิดเฉพาะ admin, allowlist หรือ test save/world | ทีมทดสอบ |
| `Player enabled` | เปิดให้ผู้เล่นทั่วไป แต่ติดตาม telemetry และ rollback ได้ | ผู้เล่นทั่วไป |
| `Stable` | ผ่าน release gates, soak และไม่มี critical issue ค้าง | ผู้เล่นทั่วไป |

### 2.2 Feature gate ที่ต้องมี

ทุกระบบที่ยังไม่ `Stable` ต้องกำหนด gate ให้ครบตามความเหมาะสม:

1. **UI gate** — ซ่อน/ล็อกเมนู ปุ่ม หรือ interaction จาก client ที่จัดจำหน่าย
2. **Server packet gate** — handler ปฏิเสธ entry point ทุกแบบก่อน mutate state; ต้องไม่ consume item, ให้ reward หรือ mark dirty
3. **Background/state gate** — กำหนดชัดว่า timer, spawn, growth, reward window หรือ job queue จะหยุดทั้งหมด หรือเดินต่อเฉพาะ state ที่ปลอดภัย
4. **Admin/test gate** — allowlist บัญชี, test shard หรือ config สำหรับทีมทดสอบ โดยไม่เปิดผ่าน claimed client identity ที่ไม่ปลอดภัย
5. **Telemetry gate** — log packet ที่ถูกปฏิเสธ, reason code, account/entity, feature state และ mutation failure เพื่อบอกว่ามีคนพยายามใช้ feature ที่ปิดอยู่หรือไม่

ระบบ economy, reward, currency, mail, market, social, PvP และ timed event ต้องมี safe default เป็น **ปิด** จนกว่าจะผ่าน acceptance ของระบบนั้น

### 2.3 รูปแบบการปฏิเสธที่ถูกต้อง

เมื่อ feature ปิดอยู่ เซิร์ฟเวอร์ต้อง:

- ตรวจ gate ก่อน reservation, inventory mutation, timer scheduling หรือ save dirty flag
- ตอบกลับด้วย protocol result/info ที่ client รับได้ ไม่ปล่อยให้ UI ค้าง
- ไม่เปิดช่องให้ packet replay เปลี่ยน state หลังได้รับผลปฏิเสธ
- เพิ่ม counter/log ที่ตรวจสอบย้อนหลังได้

---

## 3. Baseline ปัจจุบัน

โครงการมี survival loop แบบ server-authoritative ที่ใช้งานได้มากแล้ว แต่ยังไม่ใช่เกมครบทุกเมนู ตารางนี้เป็น baseline สำหรับวางงาน ไม่ใช่คำประกาศว่า feature ใด stable แล้ว

### 3.1 ระบบที่มี core loop แล้ว แต่ยังต้อง harden/validate ต่อ

| โดเมน | สถานะ baseline | ข้อจำกัดสำคัญ |
|---|---|---|
| Login, session, character persistence | มี session token, connection limits และ save player | cosmetic/profile บางส่วนยังมาจาก client; ห้ามใช้ `--insecure-auth` นอก test |
| World, movement, interest management | มี streaming, visibility, movement/range validation | server ยังไม่มี heightmap authority เต็มรูปแบบ |
| Gathering, items, tools | มี resource reservation, tool/durability, food และ inventory checks | data tags บางส่วน reconstructed/inferred; wood/tool rule ยังเป็น workaround |
| Combat, animals, death/revive | มี action/weapon/range/cooldown/stamina validation, AI แบบ project-authored และ corpse/butchery | AI/spawn/EXP ไม่ได้ยืนยันจาก original server; PvP ยังควรควบคุมด้วย gate |
| Survival/status | มี gauges, starvation, rest, food effects และ persistence | effect mapping บางส่วนเป็น icon-only หรือ project-defined |
| Crafting/cooking | validation ครอบคลุม recipe, material, workbench, tool, queue และ commit | recipe reform type 2 ยังไม่รองรับ |
| Skills/progression/research | มี cost/gate/unlock/proficiency และ auto nodes | voucher untraining ยังไม่รองรับ; level cap runtime อาจต่ำกว่า data |
| Quests/tutorial | มี state, rewards, dependencies, persistence และ tutorial departure | objectives/rewards เป็น project-authored; ไม่มี daily/NPC dialog flow ครบ |
| Farming | มี plant/water/fertilize/growth/harvest/persistence | sprinkler, gem growth, boosters, encyclopedia และ farm quest ยังไม่มี |
| POI/local warp | มี discovery และ local destination validation | cost/restriction/unlock model ยังไม่ครบ |
| Building/storage | placement, ownership baseline, boxes และ persistence มีแล้ว | building material flow ยังเป็น stub; destruction ทำให้ storage contents หายโดยไม่มี refund/drop |
| Server mods | plugin lifecycle, events และ namespaced storage มีแล้ว | ไม่มี content-pack/client negotiation/schema/signature/hot reload |

### 3.2 ระบบที่ partial หรือห้ามเปิดผู้เล่นก่อนปิดช่องว่าง

| ระบบ | สถานะปัจจุบัน | เงื่อนไขก่อน `Player enabled` |
|---|---|---|
| Building economy | วาง/build ได้ แต่ `PutMaterialsIntoArtifact` ยังไม่สร้าง material economy ที่แท้จริง | material reservation/commit, cancel/refund, destruction drop/refund, restart/multi-client tests |
| Island travel | มี protocol/server-side flow บางส่วนและ config อาจเปิด | client handoff `##goto`, island packet parity, reconnect/session/save/rollback และ real-client travel |
| Warp accelerator | มี state machine/handlers แต่ feature ปิดและยังไม่ real-client verified | join cost/currency, reward claim, persistence/abort rules, cleanup, client/manual test |
| PvP | มี code path แต่ policy/config จำกัด | anti-grief, consent/zone rules, death/drop/reward semantics, multi-client abuse suite |
| Land/estate permissions | มี owner/architect baseline | parcel authority, invite/revoke, inheritance/transfer, storage/build permissions, persistence |
| Private conversation | มี routing attempt/radiotower แต่ยังไม่ใช่ authenticated social service | session-bound identity, authorization, rate limiting, client routing verification |

### 3.3 Protocol/UI มีอยู่ แต่ยังไม่มี authoritative game subsystem

ต้องถือว่า `Not implemented` จนกว่าจะออกแบบ state model และ acceptance ครบ:

- Party และ group lifecycle
- Friends / social graph
- Clan
- Mail
- Wallet, currency และ payment flow
- Trade และ market
- Estate/land service ที่สมบูรณ์
- Pet, taming, mount และ livestock
- Jobs
- ระบบ client menu อื่นที่พบจาก protocol inventory แต่ยังไม่มี ServerCore handler/state

---

## 4. หลักสถาปัตยกรรมที่ทุกระบบต้องผ่าน

### 4.1 Server-authoritative boundary

ทุก packet เป็น input ที่ไม่เชื่อถือได้ การเปลี่ยนแปลงสำคัญต้องอยู่ฝั่ง server และมีลำดับมาตรฐาน:

1. ตรวจ feature gate, session/player state และ rate/deferred-action constraints
2. ตรวจ authorization, ownership, membership, range, target existence และ cooldown
3. ตรวจ inventory/currency/resource พร้อม reserve อย่าง atomic เมื่อมีการแข่งขัน
4. คำนวณผลลัพธ์จาก canonical server data ไม่เชื่อ client-provided IDs/counts/stat values
5. commit state, inventory/currency และ dirty/save state แบบ all-or-nothing
6. sync packet และ post-commit mod events หลัง mutation สำเร็จเท่านั้น
7. บันทึก audit event สำหรับ economy/reward/trade/permission mutation

### 4.2 State, ownership และ idempotency

ระบบใหม่ทุกระบบต้องระบุใน design ก่อนเขียน code:

- state machine และ valid transitions
- owner/member/permission model และ behavior เมื่อผู้เล่น offline/disconnect/deleted
- unique transaction/action ID หรือ idempotency policy สำหรับ retry/replay packet
- lock/reservation timeout, commit, abort และ recovery path
- ขอบเขตโลก/island/server ที่ state เป็นเจ้าของ
- event ที่ cancellable ได้ และจุด mutation ที่ cancellation ต้องเกิดก่อนเสมอ

### 4.3 Persistence และ compatibility

`SaveStore` ใช้ project-owned save models และ atomic temp-then-move write; ทุก feature ต้องรักษาหลักการนี้:

- เพิ่ม field แบบ backward-compatible พร้อม default ที่ปลอดภัย หรือเพิ่ม `SaveEnvelope.Version` พร้อม migration ที่ทดสอบได้
- ระบุว่าการอัปเกรด save เก่าจะ map state อย่างไร และ reject/save quarantine เมื่ออ่านไม่ได้อย่างไร
- ทดสอบ save ระหว่าง pending state, restart, reconnect และ crash-like interruption ตามความเสี่ยง
- backup ก่อน schema/เศรษฐกิจ/โลก migration และทำ restore drill ก่อน production
- ห้ามอาศัยการ serialize client message struct เป็น save format

### 4.4 Data provenance

generated code จาก `server/scripts/` ต้อง regenerate จาก source dump ด้วย script ที่เกี่ยวข้อง ไม่แก้ generated table ด้วยมือโดยไม่มีเหตุผลและ regeneration rule

ข้อมูลที่ project-authored ต้องอยู่ในที่ชัดเจน, version ได้, review ได้ และมี test matrix ครอบคลุม โดยเฉพาะ quest, EXP, AI/spawn, economy, crop output semantics, tags และ costs

### 4.5 Observability และการป้องกันข้อมูลเสีย

ก่อนเปิด feature ที่สร้างหรือโอน value ต้องมี:

- structured reason สำหรับ reject/abort
- metric ของ accepted/rejected/replayed/timeout/rollback actions
- audit log ของ item, currency, reward, trade, mail และ permission mutation
- alert/threshold ที่ชี้ duplication, negative balance, impossible state หรือ exception rate
- soak run แบบไม่มี exception และตรวจ TPS/RAM/queue growth

---

## 5. Roadmap หลัก

แต่ละ milestone ให้ถือว่าเริ่มที่ `Implemented`/ปิดเมนู และเปลี่ยนเป็น `Internal test` ก่อน `Player enabled` เสมอ ยกเว้น feature ที่ไม่มี player-facing rollout

| Milestone | เป้าหมาย | งานหลัก | Exit gate ก่อนเลื่อนสถานะ |
|---|---|---|---|
| **S0 — Foundation & audit** | ทำให้แผน, config, docs และ protocol inventory ตรงกัน | feature registry; packet/UI/menu inventory; data provenance catalog; reconcile config/doc drift; save migration/backup policy; shared feature-gate contract | ทุก menu มี owner/status/dependency; config ที่เปิดจริงตรงกับ docs; baseline regression ผ่านจาก clean controlled saves |
| **S1 — Core-loop parity** | ปิดช่องว่างของ survival/building/inventory loop | building material economy; completion/cancel/destruction refund/drop; storage/workbench hardening; terrain/collision/navigation authority investigation; item/equipment/data audit | build/craft/gather/storage/death/revive loop ผ่าน real client, restart และ multi-client; ไม่มี free build หรือ storage-loss policy ที่ไม่ตั้งใจ |
| **S2 — World & travel** | ทำให้การเดินทางและหลาย island รองรับจริง | POI discovery/cost/restriction/unlock; client-supported handoff; island-mode packet parity; session transfer; reconnect/rollback; cross-island persistence | client เดินทางไป-กลับจริง, restart แล้ว state ถูกต้อง, island checks ผ่าน, failure กลับสู่ state ปลอดภัย |
| **S3 — PvE, events & advanced survival** | เปิดระบบต่อสู้/กิจกรรมที่มีผลต่อ reward อย่างปลอดภัย | accelerator lifecycle; currency/cost/reward claim; abort/offline cleanup/persistence; advanced farming; effects/resistance; PvP policy หากเปิด | event และ reward ไม่มี duplication/loss; manual UI/animation ผ่าน; timed-event soak ผ่าน; PvP abuse suite ผ่านก่อนเปิด |
| **S4 — Social & permissions** | สร้าง social state ที่ authoritative | party, friends, authenticated private chat, clan, estate/land permissions; invite/revoke/leave/offline behavior | identity ไม่ spoof, membership/permission restart-safe, rate limits และ multi-client authorization tests ผ่าน |
| **S5 — Economy & services** | ทำให้ value transfer ตรวจสอบและ rollback ได้ | wallet/currency, mail, trade, market, fees/taxes, delivery/expiry/recovery, immutable transaction/audit ledger | every debit/credit มี transaction evidence; replay/race/disconnect/crash tests ผ่าน; backup/restore แสดงยอดตรง |
| **S6 — Creatures & character services** | ครอบคลุม pets/taming/mount/livestock/jobs และเมนูตัวละครที่เหลือ | entity state, ownership, AI, inventory/equipment, jobs/progression, world transfer, persistence | full lifecycle บน client จริง, death/despawn/transfer cases, multi-owner/permission และ long soak ผ่าน |
| **S7 — Content & extensibility** | รองรับ content/mod ecosystem โดยไม่ทำลาย compatibility | content packs, schemas/hash validation, server-client mod negotiation, version compatibility, example packs/mods | invalid content ถูก reject ก่อน runtime; compatible client/server/mod matrix ผ่าน; example ผ่าน build/regression |
| **S8 — 100% certification** | ยืนยันทุก player-facing system ก่อนประกาศครบ | feature matrix closure; clean/upgrade saves; negative/fuzz packets; multi-client contention; real-client walkthrough; Linux/staging/backup-restore; rollout/rollback drills | ทุกแถวใน matrix เป็น `Stable`, release gates ผ่าน, ไม่มี critical/known data-loss/duplication issue ค้าง |

### 5.1 ลำดับความสำคัญ

1. ทำ **S0** ก่อน เพื่อไม่ให้เอกสาร/config กล่าวคนละสถานะ
2. ทำ **S1** ก่อน economy/social เพราะ building, inventory, storage และ persistence เป็นฐานของ reward/value ทุกระบบ
3. ทำ **S2** ก่อนเปิด island travel จริง แม้ config จะเปิดอยู่ในบาง environment
4. ทำ **S3** โดยแยก internal event/test world จนกว่าการ claim/reward/abort จะพิสูจน์ได้
5. ทำ **S4** ก่อน **S5** ในส่วน identity/permission ที่ market/mail/trade ต้องพึ่ง
6. ทำ **S5** ก่อนเปิดเศรษฐกิจให้สาธารณะ เพราะ bugs มีผลต่อ value และ save ย้อนหลัง
7. ทำ **S6–S7** ตาม protocol inventory ที่เหลือ โดยใช้ work package เดียวกัน
8. **S8** เป็น certification ไม่ใช่ milestone สำหรับเขียน feature ใหม่

---

## 6. Work package สำหรับทุกระบบหรือเมนูที่ยังไม่ครบ

ห้ามเริ่มจากการเขียน handler ทันที ให้สร้าง work package ที่ตอบคำถามเหล่านี้ก่อน:

### A. Discovery

- เมนู, interaction, packet request/response และ client state machine คืออะไร
- มี ServerCore handler/state อยู่แล้วหรือเป็น protocol-only
- client data ให้ facts ใด และข้อใดต้อง project-author
- feature นี้พึ่ง inventory, currency, permissions, island หรือ service อื่นใด

### B. Contract

- server-owned model, lifecycle และ ownership คืออะไร
- config/feature gate ชื่อใด และ default เป็นอะไร
- packet ไหนเป็น entry point ทุกทาง รวมถึง retry/legacy path
- result/error/Info ที่ client ต้องได้รับเมื่อ feature ปิดหรือ action ไม่ผ่านคืออะไร
- save schema/default/migration/rollback policy คืออะไร

### C. Implementation

- validation, rate limit, lock/reservation, idempotency และ atomic commit อยู่จุดใด
- ใครเป็นผู้ mutate state, ผู้บันทึก save และผู้ emit post-commit event
- cancellation ของ mod event เกิดก่อน resource commit หรือไม่
- timeout/disconnect/server restart ระหว่าง action จะ recover อย่างไร

### D. Verification

- automated happy path, invalid state, replay packet, authorization และ boundary tests
- multi-client contention/race test
- two-phase restart test สำหรับ pending/reward/resource/value state
- real-client menu, animation, message และ relog test
- soak/load test ถ้ามี timers, queue, spawner หรือ broadcast

### E. Rollout

- `Implemented` → `Internal test` → `Player enabled` → `Stable` มี exit evidence ใดบ้าง
- allowlist/shard/config อยู่ที่ใด
- telemetry threshold ใดทำให้ปิด feature กลับ
- rollback จะจัดการ state ที่เกิดระหว่าง feature เปิดทดลองอย่างไร

---

## 7. แผนทดสอบและหลักฐานบังคับ

### 7.1 Regression baseline ที่ต้องไม่พัง

ก่อน merge/เปิด feature ที่กระทบ gameplay ให้รัน checks ที่เกี่ยวข้องจาก `test-client` อย่างน้อย:

- `--gp-check` — protocol/anti-cheat baseline
- `--multi-check` — multi-client contention
- `--cook-check`, `--recipe-check` — cooking/recipe data
- `--tool-check` — durability/repair/break
- `--stamina-check` — survival/rest/fatigue
- `--skill-check`, `--combat-skill-check` — skill/progression/action gate
- `--quest-check` — quest state/reward/persistence
- `--farm-check`, `--farm-resume-check setup|verify` — farming lifecycle/restart
- `--poi-check`, `--vision-check`, `--group2-check` และ checks อื่นที่ feature กระทบ

ใช้ `tools/menu.ps1` เพื่อ build/start test server และรัน checks ตาม workflow ที่มีอยู่ ห้ามสรุปว่า feature ผ่านจาก build สำเร็จอย่างเดียว

### 7.2 หลักฐานแยกตามระดับ

| หลักฐาน | พิสูจน์อะไร | ใช้เมื่อ |
|---|---|---|
| Protocol test | request/response, validation, reject behavior, state mutation | ทุก feature |
| Negative/replay test | spoof, duplicate packet, invalid state, ownership/range bypass | ทุก mutation/value feature |
| Multi-client test | race, reservation, simultaneous ownership/reward action | shared world, storage, party, market, event |
| Restart test | save/reload, pending state, reward/item duplication/loss | ทุก persistent feature |
| Real-client test | menu visibility, UI, animation, packet ordering, playable flow | ทุก player-facing feature |
| Packet dump | raw message diagnosis ผ่าน bot console `dump` | protocol/UI mismatch |
| UI dump / packet watcher | structural UI และ bandwidth diagnosis | presentation/performance investigation |
| Soak test | exception-free timers, memory, TPS, queue and cleanup | event, spawner, AI, farm, chat, market |
| Linux/staging test | runtime/deploy behavior ที่ต่างจาก Windows dev | ก่อน production |

### 7.3 Save discipline สำหรับการทดสอบ

- ใช้ fresh หรือ controlled save และบันทึก test identity/state ที่เริ่มต้น
- อย่าตัดสิน failure จาก final item count เพียงอย่างเดียวเมื่อ skill bonus สามารถเพิ่มผลผลิตได้; วัด reservation/commit ที่ตั้งใจทดสอบ
- สำหรับ resource/reward ที่มี partial state ให้ใช้ two-phase `setup → restart → verify`
- ใช้ Ctrl+C เพื่อ force save ตาม operational guidance ไม่ kill process แบบไม่รู้ state
- ก่อน test migration/economy ให้ backup และทดสอบ restore เป็นส่วนหนึ่งของ acceptance

---

## 8. กฎการเปิดเมนูตามความเสี่ยง

| กลุ่ม | กฎก่อนเปิดให้ผู้เล่นทั่วไป |
|---|---|
| Inventory, crafting, building, storage | ต้อง authoritative, atomic, restart-safe; ต้องไม่มี free/สูญหาย flow ที่ไม่ระบุเป็น design |
| Quest/reward/progression | reward anti-duplication, repeat/relog/restart, prerequisite และ project-authored data tests ต้องครบ |
| PvP/combat/event | consent/zone/range/action validation, death/reward policy, anti-grief และ multi-client soak ต้องครบ |
| Travel/island | real-client handoff, session/save/reconnect/failure recovery ต้องผ่าน end-to-end |
| Party/social/clan/land | session-bound identity, permission model, offline/disconnect state และ authorization tests ต้องครบ |
| Currency/mail/trade/market | immutable ledger/audit, replay/race/disconnect/crash recovery และ backup/restore ต้องครบ |
| Pets/mount/livestock/jobs | ownership, entity lifecycle, transfer, despawn/death, persistence และ client lifecycle test ต้องครบ |

ระบบต่อไปนี้ต้องคง `Not implemented` หรือ `Implemented`/ปิดเมนู จนกว่าจะผ่านเงื่อนไขเฉพาะของตน: island travel, warp accelerator, economy, market, mail, party, friends, clan, estate permissions, pets/mounts/livestock และ jobs

หาก implementation กับ documentation/config ระบุคนละสถานะ ให้ถือว่า feature **ยังไม่พร้อมเปิด** จนกว่าจะ reconcile ข้อเท็จจริงและเพิ่ม evidence

---

## 9. Feature-completeness matrix

สร้างและรักษา tracker เป็นส่วนหนึ่งของ S0/S8 โดยให้หนึ่งแถวต่อ player-facing subsystem หรือ menu flow

### 9.1 Core systems

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Connection / Auth / Session | Verified | Verified | Verified | N/A (in-memory) | Verified | Verified (SmokeCheck) | Verified | `Stable` |  |
| Session token (GP-12) | N/A | Verified | Verified | N/A | Verified | Verified (SmokeCheck, MultiCheck) | Verified | `Stable` |  |
| Rate limiting (M-6) | N/A | Verified | Verified | N/A | Verified | Verified (MultiCheck) | Verified | `Stable` |  |
| Player sync / Vision | Verified | Verified | Verified | N/A | Verified | Verified (VisionCheck, MultiCheck) | Verified | `Stable` |  |
| Chat (exclusive channel) | Verified | Verified | Verified | N/A | Verified | Verified (SmokeCheck) | Verified | `Stable` |  |
| Emotes | Verified | Verified | Verified | N/A | Verified | Not started | Not started | `Internal test` |  |
| Save / Persistence | N/A | Verified | Verified | Verified | Verified | Verified (SaveStoreCheck 14/14, WorldPersistenceCheck) | Verified | `Stable` |  |

### 9.2 Player character

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Progression / Leveling | Verified | Verified | Verified | Verified | Verified | Verified (StatCheck, GpCheck) | Verified | `Stable` |  |
| Skills (learn/untrain/research) | Verified | Verified | Verified | Verified | Verified | Verified (SkillCheck, GpCheck) | Verified | `Stable` |  |
| Skill effects (auto triggers) | Verified | Verified | Verified | N/A (derived) | Verified | Verified (SkillCheck) | Verified | `Stable` |  |
| Proficiency | Verified | Verified | Verified | Verified | Verified | Verified (StatCheck) | Verified | `Stable` |  |
| Abilities | Verified | Verified | Verified | N/A (derived) | N/A | Not started | Not started | `Internal test` |  |
| Status effects / Resistances | Verified | Verified | Verified | Verified | Verified | Verified (Group2Check) | Verified | `Stable` |  |
| Titles | Verified | Verified | Verified | Verified | Verified | Verified (Group2Check) | Verified | `Stable` |  |
| Survival (HP/Stamina/Fatigue/Hunger) | Verified | Verified | Verified | Verified | Verified | Verified (StaminaCheck, StatCheck) | Verified | `Stable` |  |
| Death / Revival | Verified | Verified | Verified | Verified | Verified | Verified (SmokeCheck) | Verified | `Stable` |  |
| Rename character | Verified | Verified | Verified | Verified | Verified | Verified (Group2Check) | Not started | `Internal test` |  |

### 9.3 Inventory & items

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Inventory (put/take/dump/use/order/lock) | Verified | Verified | Verified | Verified | Verified | Verified (GpCheck, SmokeCheck) | Verified | `Stable` |  |
| Item tags / Processing | Verified | Verified | Verified | Verified | Verified | Verified (CookCheck 11/11, RecipeCheck) | Verified | `Stable` |  |
| Food system | Verified | Verified | Verified | N/A (config) | Verified | Verified (CookCheck, RecipeCheck) | Verified | `Stable` |  |

### 9.4 Equipment

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Equipment / Equip / Accessories | Verified | Verified | Verified | Verified | Verified | Verified (GpCheck) | Verified | `Stable` |  |
| Tool durability / Repair | Verified | Verified | Verified | Verified | Verified | Verified (ToolCheck, GpCheck) | Verified | `Stable` |  |

### 9.5 Gathering & resources

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Gathering (touch/collect) | Verified | Verified | Verified | Verified | Verified | Verified (GpCheck, SmokeCheck) | Verified | `Stable` |  |
| Butchery | Verified | Verified | Verified | Verified | Verified | Verified (GpCheck) | Verified | `Stable` |  |

### 9.6 Crafting

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Crafting (recipes / build queue) | Verified | Verified | Verified | N/A (recipe data) | Verified | Verified (CookCheck, RecipeCheck, SmokeCheck) | Verified | `Stable` |  |
| Cooking (recipe type 1) | Verified | Verified | Verified | N/A | Verified | Verified (CookCheck 11/11, RecipeCheck) | Verified | `Stable` |  |
| Recipe reform type 2 | Not started | Not started | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |

### 9.7 Building & storage

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Building (blueprints / artifacts) | Verified | Verified | Verified | Verified | Verified | Verified (BuildingEconomyCheck 14/14, BlueprintRequirementsCheck 6/6) | Verified | `Implemented` | building material economy ยังเป็น stub |
| Storage / Workbenches | Verified | Verified | Verified | Verified | Verified | Verified (StorageWorkbenchCheck 6/6) | Verified | `Implemented` | destruction drop/refund ยังไม่มี |
| Capsulated artifacts | Verified | Verified | Verified | Verified | Verified | Verified (BlueprintRequirementsCheck) | Not started | `Internal test` |  |

### 9.8 Combat & animals

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Combat (actions / battle) | Verified | Verified | Verified | N/A (stateless) | Verified | Verified (CombatSkillCheck, GpCheck) | Verified | `Stable` |  |
| Animals (spawn / AI) | Verified | Verified | Verified | N/A (config) | Verified | Verified (GpCheck, SmokeCheck) | Verified | `Stable` | AI/spawn/project-authored |
| PvP | Not started | Partial (structs exist) | Not started | N/A | Not started | Not started | Not started | `Not implemented` |  config `Pvp: false` |

### 9.9 Quests & tutorial

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Quests | Verified | Verified | Verified | Verified | Partial | Verified (QuestCheck 30/33) | Partial | `Implemented` | objectives/rewards เป็น project-authored; config `Quests: false` |
| QuestChecklist | Verified | Verified | Verified | Verified | Verified | Verified (QuestCheck) | Verified | `Stable` |  |
| Tutorial / Depart | Verified | Verified | Verified | N/A | Verified | Verified (CreateCharacterCheck) | Verified | `Stable` |  |

### 9.10 POI & travel

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| POI (discovery / explore) | Verified | Verified | Verified | Verified | Verified | Verified (PoiCheck) | Verified | `Stable` |  |
| Local warp | Verified | Verified | Verified | Verified | Verified | Verified (PoiCheck) | Verified | `Implemented` | cost/restriction/unlock model ยังไม่ครบ |
| Island travel | Verified | Verified (gated) | Verified (gated) | Not started | Verified (gated) | Verified (IslandTravelGateCheck 17/17) | Not started | `Implemented` | config `IslandTravel: false`; ต้องผ่าน S2 ก่อนเปิด |
| Warp accelerator | Verified | Verified (gated) | Verified (gated) | Verified (partial) | Verified (gated) | Verified (WarpAcceleratorGateCheck 7/7) | Not started | `Implemented` | config `WarpAccelerator: false` |

### 9.11 Farming

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Farming (plant/water/fertilize/harvest) | Verified | Verified (gated) | Verified (gated) | Verified (gated) | Verified (gated) | Verified (FarmCheck 39/39) | Not started | `Implemented` | config `Farming: false`; sprinkler/gem/boosters ยังไม่มี |
| Sprinkler / Advanced farming | Not started | Not started | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Crop encyclopedia | Not started | Not started | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |

### 9.12 Protocol-only (ไม่มี server handler)

| โดเมน/Flow | UI flow | Packet mapping | Authoritative state | Save/migration | Anti-abuse | Automated | Real client | Rollout status | Owner/decision |
|---|---|---|---|---|---|---|---|---|---|
| Party | Partial (structs) | Verified | Verified | Verified (PartyId/PartyLeader) | Verified (PartyGateCheck 19/19) | Verified (PartyGateCheck) | Not started | `Implemented` | config `PartyAndClan: false`; invite/join/reject handshake ตาม client protocol; ยังไม่ real-client verified |
| Clan | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` | config `PartyAndClan: false` |
| Friends / Social graph | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Private conversation | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` | radiotower routing ยังไม่ authenticated |
| Mail | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Market / Shop | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` | config `Market: false` |
| Trade | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Wallet / Currency | Not started | Not started | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Pet / Taming | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` | config `Taming: false` |
| Mount | Not started | Not started | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Livestock | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` | config `Livestock: false` |
| Jobs | Not started | Not started | Not started | N/A | N/A | Not started | Not started | `Not implemented` | config `Jobs: false` |
| Land / Estate permissions | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` | config `LandPermission: false` |
| Dye / Bleach | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Factions | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Discovery / Encyclopedia | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Missions | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Milestone | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Attendance | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Cargo / Warphole defense | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Archipelago | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Band / Music | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| NPC Conversation | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Engagement | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| Blocklist | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |
| AddOns / DLC | Partial (structs) | Partial | Not started | N/A | N/A | Not started | Not started | `Not implemented` |  |

### 9.13 คอลัมน์และค่าที่ใช้

| คอลัมน์ | ความหมาย |
|---|---|
| UI flow | client มีเมนู/interaction ที่ผู้เล่นเปิดได้จริง; `Verified` = ทดสอบแล้วว่า client แสดงผลถูก |
| Packet mapping | request/response packets ที่ server รับ/ส่งได้ครบ; `Partial` = มี struct แต่ยังไม่มี handler |
| Authoritative state | server มี state model ที่ compute/validate เอง ไม่เชื่อ client; `Verified` = state ถูก save/reload |
| Save/migration | มี save schema, backward-compatible migration, หรือ atomic write; `Verified` = ผ่าน restart/crash test |
| Anti-abuse | มี authorization, rate limit, race protection, idempotency; `Verified` = ผ่าน negative/replay/multi-client test |
| Automated | มี automated test suite; ระบุจำนวน test case ที่ผ่าน |
| Real client | ทดสอบกับ client จริงแล้ว; `Verified` = UI/animation/flow ถูกต้อง |
| Rollout status | ตาม Section 2.1: `Not implemented`, `Implemented`, `Internal test`, `Player enabled`, `Stable` |

### 9.14 กฎการอัปเดต

- ห้ามเปลี่ยน `Rollout status` เป็น `Stable` หากช่อง essential (Authoritative state, Save, Anti-abuse, Automated, Real client) ใดไม่เป็น `Verified`
- ทุกครั้งที่เพิ่ม feature ใหม่ ต้องเพิ่มแถวใน matrix ก่อน merge
- อัปเดต matrix พร้อมกับ work package completion evidence เสมอ
- ใช้ `Not started`, `In progress`, `Verified`, `N/A` ในคอลัมน์หลักฐาน
- รายการ tracker ต้องอ้างอิง protocol inventory และ client UI dump จริง ไม่ใช้เพียงรายชื่อที่จำได้

---

## 10. จุดเริ่มต้นใน source และเอกสาร

| หัวข้อ | จุดเริ่มต้น |
|---|---|
| Connection/session/runtime | `server/ServerCore/GameServer.cs`, `server/Program.cs` |
| Player gameplay handlers | `server/ServerCore/ServerPlayer.*.cs` |
| World, natural state, POI, farms | `server/ServerCore/ServerWorld*.cs`, `server/ServerCore/AnimalSpawner.cs` |
| Save schemas/storage | `server/ServerCore/SaveModels.cs`, `server/ServerCore/SaveStore.cs`, [`Persistence.md`](Persistence.md) |
| Recipe/skill/crop/action data | `server/scripts/`, `server/ServerCore/RecipeRequirements.cs`, `SkillNodeData.cs`, `CropData.cs`, `ActionData.cs` |
| Feature flags | `server/data/config.json`, `server/ServerCore/ServerConfig.cs` |
| Mods/events | `server/ServerCore/Modding/`, [`Modding.md`](Modding.md) |
| Protocol tests | `test-client/Program.cs`, `test-client/*Check.cs`, [`../testing/TESTPLAN.md`](../testing/TESTPLAN.md) |
| Real-client diagnostics | `docs/server/RemoteControl.md`, `client/UiDump.cs`, `client/Durango.Development/PacketWatcher*.cs` |
| Production policy | [`../operations/BETA-OPS.md`](../operations/BETA-OPS.md), `VPS-DEPLOY.md`, `LINUX-TESTING.md` |

---

## 11. Release certification (S8)

ก่อนประกาศว่า server รองรับเกมครบ 100% ต้องมีหลักฐานต่อไปนี้ทั้งหมด:

1. Feature-completeness matrix ครอบคลุมทุก menu/interaction ที่ client เปิด และทุกแถวเป็น `Stable`
2. ทุก feature มี UI flow, protocol mapping, server state, persistence และ anti-abuse decision ที่ review ได้
3. Automated suite ผ่านจาก clean save และ regression ที่เกี่ยวข้องผ่านหลังทุก work package
4. มี negative/replay/authorization coverage สำหรับ packet ที่ mutate state ทุกชนิด
5. ทุก shared-value feature ผ่าน multi-client contention และ disconnect/retry tests
6. ทุก persistent/reward/partial state ผ่าน restart/upgrade-save/backup-restore test
7. ทุก player-facing flow ผ่าน real-client walkthrough พร้อมบันทึก issue ที่ไม่ใช่ blocker อย่างชัดเจน
8. Timed/background systems ผ่าน soak โดยไม่มี exception, queue leak, runaway memory หรือ corruption
9. Linux/staging build, startup, save, restart และ health endpoint ผ่านตาม deployment documentation
10. มี rollout plan, telemetry dashboard/log queries, feature-disable procedure และ tested rollback/restore plan

หากเงื่อนไขใดไม่ครบ ให้ระบุ feature เป็น `Implemented`, `Internal test` หรือ `Player enabled` ตามหลักฐานจริง ไม่ใช้คำว่า complete

---

## 12. สิ่งที่แผนนี้ไม่ทำในทันที

เอกสารนี้ไม่เปิด feature flag, ไม่แก้ runtime config, ไม่แก้ client UI และไม่ทำให้ระบบที่ยังไม่ผ่าน test ใช้งานได้เอง งานถัดไปควรเริ่มจาก **S0: feature registry + protocol/UI inventory + reconcile สถานะ config/docs** แล้วจึงเลือก S1 work package ที่ปิดช่องโหว่ material economy และ persistence ของ core loop ก่อนระบบที่มี value transfer สูงกว่า
