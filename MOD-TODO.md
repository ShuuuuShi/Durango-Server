# Durango Mod System — Full-stack TODO

เป้าหมาย: ทำให้ม็อดเพิ่ม gameplay/content/presentation ได้จริงบน server ที่ authoritative,
รองรับ vanilla client เมื่อเป็น server-only mod และติดตั้ง/ถอด/rollback ได้โดยไม่ปนกับ
เซิร์ฟเวอร์ vanilla

สถานะปัจจุบัน: M0–M5 มี framework/build gate แล้ว แต่ M3 ยังเป็น validation pipeline
เป็นหลัก — ยังต้องต่อ registry ของเกมให้ content ใหม่ถูกใช้จริง

## P0 — แยก runtime และล็อกสัญญา

- [x] แยก vanilla server กับ mod-enabled server เป็นคนละโฟลเดอร์
- [x] SDK server/client และ protocol `ModHello`
- [x] before/cancel + post-commit event framework
- [x] package manifest, dependency ordering, SHA-256 และ signature hooks
- [x] metrics, packet/manifest/command limits และ admin diagnostics
- [ ] ทำ build profile `Vanilla` ที่ตัด mod assembly/reference ออกจาก binary จริง
- [ ] ทำ build profile `Modded` ที่ตรวจ SDK/protocol version อัตโนมัติ
- [ ] เพิ่ม compatibility matrix: vanilla client ↔ server-only mod ↔ client mod
- [ ] เขียน contract tests กัน API เดิมของม็อดเก่าพัง

## P1 — Runtime content registry (สำคัญที่สุด)

- [ ] ออกแบบ registry กลางสำหรับ `item`, `recipe`, `loot`, `buildable`, `quest`
- [ ] register content หลัง validation ก่อน world/player รับเข้า
- [ ] namespace lookup: `mod-id:local-id` และ alias ที่ปลอด collision
- [ ] validate field schema, type/range, required fields และ unknown fields
- [ ] validate reference ภายใน/ข้ามม็อดและรายงาน dependency ที่หาย
- [ ] conflict policy: duplicate ID, override, priority และ explicit `replace`
- [ ] ต่อ item registry เข้ากับ inventory, drop, collect, craft, equip, durability, tags
- [ ] ต่อ recipe registry เข้ากับ craft menu, requirement, table, tool และ output
- [ ] ต่อ loot registry เข้ากับ animal/corpse/gather/drop และ deterministic RNG
- [ ] ต่อ buildable registry เข้ากับ blueprint, placement, material, completion, demolition
- [ ] ต่อ quest registry เข้ากับ progress, reward, completion และ persistence
- [ ] ส่ง content catalog/hash ให้ client หลัง negotiation
- [ ] เพิ่ม `--content-check` และ test pack valid/invalid/reference/conflict

## P2 — Transaction และ gameplay API

- [ ] เพิ่ม inventory transaction: reserve/consume/refund/commit แบบ atomic
- [ ] เพิ่ม API ลบ/ย้าย/เปลี่ยนไอเทมโดย server ตรวจทุกครั้ง
- [ ] เพิ่ม typed event payload แทน dictionary string สำหรับ action สำคัญ
- [ ] ครอบคลุม before/cancel ทุก mutation รวม inventory, quest reward, progress, revive และ travel
- [ ] กำหนด event ordering, re-entrancy และห้าม cancel หลัง commit ให้เป็น contract test
- [ ] รองรับ deferred action cancellation/timeout/rollback ไม่ให้ stamina หรือของค้าง
- [ ] เพิ่ม API สำหรับ server-side config/feature flag แบบ read-only หรือ permissioned
- [ ] เพิ่ม permission/capability ของม็อดและป้องกัน command ชน built-in ตั้งแต่โหลด

## P3 — Client presentation และ assets

- [x] hotkey, OnGameReady, OnUpdate, scene hook และ HUD hook
- [x] path-safe asset hash/manifest validation
- [x] โหลด AssetBundle/texture/audio/prefab จาก package ที่ verify แล้ว พร้อม model/material/texture override และ spawn API
- [ ] asset cache, versioning, unload และ memory budget ต่อม็อด
- [ ] UI API: panel, button, label, modal, notification และ input context
- [ ] scene lifecycle: load/unload/reconnect/reload ไม่ยิง handler ซ้ำ
- [ ] localization API และ fallback language
- [ ] client content catalog sync กับ server catalog
- [ ] fail-safe เมื่อ asset/UI ของม็อดเสีย โดยไม่ทำให้เกมหลักล้ม
- [ ] ตัวอย่างม็อดจริง: HUD status, custom menu, content preview

## P4 — Multiplayer, trust และ security

- [x] required/optional mod policy และ reject ก่อนสร้าง player
- [x] per-mod hash/signature fields และ RSA verification path
- [ ] server ส่ง authoritative required catalog/challenge ให้ clientก่อน Ready
- [ ] signature policy ครบวงจร: key rotation, key ID, revocation และ expiry
- [ ] ตรวจ package signature/assembly hash/content hash ให้ใช้ policy เดียวกัน
- [ ] mismatch report ที่อ่านง่าย: missing/extra/version/hash/signature
- [ ] mod allow-list/deny-list และ disable รายม็อดโดยไม่แก้ source
- [ ] quota ต่อม็อด: event subscriptions, command rate, storage, CPU warning และ output size
- [ ] ป้องกัน event recursion, broadcast spam และ cross-mod data access
- [ ] document ขอบเขต trusted in-process DLL และ threat model

## P5 — Tooling และการติดตั้ง

- [ ] `durango-mod new` สร้าง server/client/content mod template
- [ ] `durango-mod build` build SDK-compatible DLL
- [ ] `durango-mod validate` ตรวจ manifest/content/reference/signature
- [ ] `durango-mod pack` สร้าง package deterministic พร้อม hash
- [ ] `durango-mod install` ลง server/client แยก root และตรวจ version
- [ ] `durango-mod update` พร้อม backup และ atomic swap
- [ ] `durango-mod remove` ปิด/ถอดโดยไม่ลบ save ของม็อดโดยอัตโนมัติ
- [x] PowerShell runtime patch installer + rollback สำหรับ server runtime ของเรา
- [ ] package lockfile และ reproducible build metadata
- [ ] migration hook สำหรับ schema save ของม็อด
- [ ] package signing tool และ verify command

## P6 — Testing และ release gates

- [ ] unit tests: schema, namespace, canonical hash, signature, dependency graph
- [ ] integration tests: load phases, event cancellation, transaction rollback
- [ ] method override tests: prefix/postfix/replace, original skip, chain ordering, conflict, exception isolation, unpatch/reload
- [ ] protocol tests: old client, vanilla client, matching/missing/wrong/signed mod
- [ ] content tests: item/recipe/loot/buildable/quest end-to-end ใน world จริง
- [ ] restart/crash persistence tests ของ world/player/mod storage
- [ ] 5-client soak 2 ชั่วโมง
- [ ] 20-client soak 2–3 ชั่วโมง พร้อม packet flood/mod handler fault injection
- [ ] memory/CPU/output-rate budget และ admin metrics validation
- [ ] clean install/uninstall/rollback test บน Windows และ Linux
- [ ] release artifact checksum + changelog + upgrade notes

## P2.5 — Native method override / patch pipeline

เป้าหมายคือให้ม็อด override method เดิมของเกมได้จริง นอกเหนือจากการ subscribe event โดยต้องทำงานแบบตรวจสอบได้และถอดกลับได้

- [x] กำหนด patch point ที่รองรับ `Before/Prefix`, `After/Postfix`, `Replace` และการเลือกว่าจะเรียก original method หรือไม่
- [ ] สร้าง stable method ID จาก assembly/type/method/signature พร้อมตรวจ game build และ SDK compatibility
- [x] resolve method และตรวจ parameter/return type ก่อนติดตั้ง patch; ถ้าไม่ตรงให้ fail-closed พร้อม error ที่อ่านได้
- [x] ทำ patch chain สำหรับหลายม็อด: priority, deterministic ordering และ explicit `Replace` conflict policy
- [x] รองรับ original call, skip original, เปลี่ยน arguments/return value และจัดการ exception ตาม contract
- [x] unpatch ได้อย่างปลอดภัยเมื่อ disable, rollback หรือ unload ม็อด (hot reload ทั้ง assembly ยังอยู่ใน backlog)
- [x] ป้องกัน recursion, double patch, thread-safety และจำกัด server/client target ให้อยู่ใน assembly ของฝั่งตัวเอง
- [ ] deny-list method สำคัญด้าน authentication, network framing, save corruption และ server authority เว้นแต่ได้รับ capability เฉพาะ
- [x] isolate failure ของ patch และคืน runtime กลับสู่สถานะก่อนติดตั้งเมื่อ patch โหลดไม่สำเร็จ
- [ ] เพิ่ม debug/admin diagnostics: patch list, owner mod, order, target signature, status และ last exception
- [x] เขียนตัวอย่าง override method จริงใน server และ client พร้อม compatibility/fallback path

## Definition of Done: Full Mod System

- [ ] ม็อดสามารถ override method ที่ประกาศรองรับได้จริง โดยตรวจ version/signature, conflict, permission และ rollback ครบ

- [ ] ม็อด server-only ใช้กับ vanilla client ได้โดยไม่ต้องลง DLL ฝั่ง client
- [ ] ม็อดที่มี content ใหม่ทำให้ item/recipe/loot/buildable/quest ใช้ได้จริงในเกม
- [ ] client UI/asset ของม็อดโหลดและ fail-safe ได้
- [ ] server ปฏิเสธ package/client ที่ missing, mismatch, unsigned หรือหมดสิทธิ์ตาม policy
- [ ] restart/crash ไม่ทำให้ของผู้เล่นหรือ save ของม็อดเสีย
- [ ] install/update/rollback ทำซ้ำได้จาก clean server directory
- [ ] acceptance soak ผ่านโดยไม่มี server crash, duplication หรือ unbounded memory growth

## ลำดับงานถัดไป

1. P1: runtime content registry + item/recipe end-to-end
2. P2: atomic inventory/content transactions
3. P2.5: native method override / patch pipeline
4. P3: AssetBundle + real UI API
4. P4: authoritative catalog challenge/signature policy
5. P5/P6: CLI, package lifecycle และ soak/release gates
