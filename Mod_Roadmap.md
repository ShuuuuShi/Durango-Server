# Durango Mod System Roadmap

สถานะ: Design baseline / Implementation roadmap
วันที่: 2026-08-28
เป้าหมาย: เพิ่มไอเทม สูตรคราฟต์ สิ่งก่อสร้าง สถานะ และ override ผ่าน mod โดยไม่แก้ไฟล์เกม

## 1. หลักการ

- Server เป็น authoritative source ของ content, inventory, craft, combat และ world state
- Client mod แสดงผล รับ input ทำ UI และโหลด asset ที่ server อนุมัติ
- ฟีเจอร์ใหม่ต้องอยู่ใน mod package; Assembly-CSharp.dll เหลือเฉพาะ stable bootstrap/bridge
- มอดอ้างอิงเฉพาะ SDK ไม่อ้าง game internal assembly หรือ DurangoServer.dll
- มอดที่พังต้อง isolate ไม่หยุดมอดอื่นหรือเซิร์ฟเวอร์
- ตรวจ capability, permission, dependency, quota, metrics และ audit ได้
- Assembly.LoadFrom และ Unity/Mono ไม่ใช่ hard sandbox; hard sandbox ต้องแยก runtime เป็น process ในอนาคต

## 2. Baseline ปัจจุบัน

- Server SDK: IGamePlugin, IModApi, PreLoad/Load/PostLoad
- Server loader: โหลด server/mods/*.dll, กั้น 3 phase และมี /admin/mods
- Event bus: before/after, priority, cancellation และ storage ต่อ namespace
- Player API: player data, inventory พื้นฐาน, teleport, give item
- Client SDK/loader: log, message, hotkey, ready, local player, update และ game/mods/*.dll
- ตัวอย่าง: ExampleMod, ExampleGameplayMod, MemoryBotMod
- ข้อจำกัด: startup-only, dependency ยังไม่ topological, payload เป็น string dictionary, ไม่มี content registry, handshake, asset API และ unload จริง

## 3. Target architecture

Mod package
-> Mod Manager ตรวจ manifest/hash/dependency/permission
-> Server Runtime: Content Registry + Override Bus + Events + Scheduler + Storage
-> authoritative persistence/validation
-> Mod Handshake
-> Client Runtime: resolver + asset/UI registry + presentation

Server mod ลงทะเบียน content, ตรวจ/cancel operation, commands, jobs, storage และประกาศ manifest
Client mod แสดง icon/model/effect/localization/UI และรับ input
Client ห้ามสร้าง item หรือยืนยันผล craft/damage/reward/inventory เอง

## 4. Package และ manifest

โครงสร้าง:

mods/dinoworld.core/
- mod.json
- server/Dinoworld.Core.dll
- client/Dinoworld.Core.Client.dll
- content/items.json
- content/recipes.json
- content/blueprints.json
- content/statuses.json
- content/localization/th.json
- content/assets/icons และ models
- config.json

manifest ต้องมี id, name, version, apiVersion, serverAssembly/clientAssembly, content, dependencies, capabilities, permissions และ configSchemaVersion

กติกา: id lowercase เป็น stable identity; version ใช้ SemVer; ตรวจ missing/incompatible/cycle; path ห้ามหลุด package; โหลดเฉพาะ assembly ที่ระบุ; production ตรวจ SHA-256/signature/allowlist
ไม่ copy DurangoModSdk.dll หรือ DurangoClientModSdk.dll เข้า mod directory

## 5. Stable IDs

ใช้ namespaced ID:
- dinoworld.core:item_wooden_bow
- dinoworld.core:recipe_wooden_bow
- my.mod:blueprint_watch_tower
- my.mod:status_well_rested

ห้ามใช้ integer หรือ enum ภายในเป็น public identity
rename ใช้ alias/migration
item persistence เก็บ modId, contentId, definitionVersion และ payload
ถอดมอดต้องมี policy: quarantine, placeholder/convert หรือ block world load

## 6. Content registry

ItemDefinition:
- id, display/description keys, BasePrototypeId
- icon/model/asset references
- stack, weight, durability, quality, tags
- equip/use behavior, slot, food/heal/status payload
- owner mod, definition version, player-visible/admin-only/creative-only

RecipeDefinition:
- id, category, sort order, localization
- ingredients เป็น namespaced item ID + จำนวน
- output item + จำนวน + quality + custom payload
- tool/workstation/blueprint, level, skill, duration, cost
- discovery/visibility, adminOnly, playerVisible, enabled
- server validator และ before/after commit hooks

ตัวอย่าง recipe ธนู:
- id: dinoworld.core:recipe_wooden_bow
- ingredients: game:wood 12, game:fiber 8
- output: dinoworld.core:item_wooden_bow 1
- requiredSkill: game:archery
- requiredLevel: 1
- workstation: game:crafting_table

Asset strategy:
1. data-only ใช้ BasePrototypeId ไม่ต้องมี client companion
2. client content pack โหลด icon/model/animation/effect/localization ที่ server approve
3. advanced presentation เพิ่ม tab/tooltip/recipe screen พร้อม fallback

Server ห้ามรับ asset path ดิบจาก client; client map จาก manifest และตรวจ hash

## 7. Override pipeline

ทุก operation เดินเส้นทางเดียว:
create context -> base validation -> before handlers ตาม priority
-> resolve cancellation/conflict -> authoritative commit
-> after/committed handlers -> result/audit

ต้องมี typed CraftContext, GatherContext, CombatContext, BuildContext และ InventoryContext
context มี EventId, phase, Cancel(reason), mutable proposal พร้อม source/owner, validation errors และ correlation ID

กติกา:
- priority/after order deterministic
- Monitor แก้ context ไม่ได้
- field ชนกันต้อง claim หรือแจ้ง conflict
- security field ใช้ deny-by-default
- cancellation ระบุ modId/reason/event ID
- exception isolate และ recursion depth limit

Proposed API (ยังไม่ใช่ implemented contract):
IOverrideRegistry.Register<TContext>(operationName, phase, priority, handler)
IOverrideHandle.Dispose()
OverrideDecision.Allow/Deny/Modify

## 8. SDK v2

คง IModApi v1 เพื่อ compatibility แล้วเพิ่ม:
- Core: IModIdentity, IModLogger, IModRegistry, IModEvents, IModScheduler, IModStorage, IModPermissions, IModMetrics, IModNet
- Content: IItemRegistry, IRecipeRegistry, IBlueprintRegistry, IStatusEffectRegistry, ISkillRegistry, ILocalizationRegistry
- Client: IAssetRegistry, IContentResolver, IClientUiRegistry

ห้าม expose internal game types ใน public API และต้องมี API compatibility matrix

## 9. Lifecycle/dependency/reload

ขยาย 3 phase เดิมเป็น:
Discover -> Validate -> Resolve dependencies
-> PreLoad -> Register -> Resolve conflicts/lock registries
-> Enable jobs/network -> Ready

shutdown:
Disable callbacks -> stop jobs/network -> flush storage
-> dispose subscriptions/commands/override handles
-> unregister -> unload หรือ mark restart-required

dependency-first; cycle/missing/incompatible เป็น Blocked เฉพาะมอดนั้น; tie-break ด้วย modId แบบ ordinal

Server reload ใช้ shadow-copy + collectible AssemblyLoadContext, main-loop safe point, dispose/flush, validate, atomic swap และ rollback
Client Unity/Mono ใช้ restart-required สำหรับ code mod; soft reload เฉพาะ data/UI

## 10. Security/performance/persistence

capability: content.register, gameplay.override, player.inventory, player.teleport, command.register, admin.cheat, storage.namespaced, network.mod_channel, client.asset_load, filesystem.package_read

production ใช้ allowlist/version/hash/signature
command เป็น /mod modId verb; admin ตรวจ role/rate limit/audit
cheat เป็น test-only และใช้ --enable-cheat
path จำกัดใน package/storage namespace; ไม่ expose raw socket/process/reflection/game object โดยไม่มี capability

ห้ามงานหนักใน OnTick; ใช้ interval jobs, ไม่ block main thread ด้วย I/O, dispatch world mutation กลับ main thread, จำกัด queue/storage และเก็บ invocation/error/duration/queue metrics

custom item ต้องเก็บ modId, contentId, definitionVersion, amount และ payload
migration ต้อง idempotent/testable, save แบบ atomic และมี backup
unknown item ต้องมีรายงานและ policy

## 11. Client handshake

ก่อน gameplay UI:
Client -> Server: modId, clientVersion, capabilities, content hashes
Server -> Client: required mods, approved manifest, protocol version
Client -> Server: ready / missing asset / incompatible
Server: fallback, reject หรือ disable presentation

server ส่ง authoritative definitions; client ไม่มี companion ต้อง fallback หรือถูกปฏิเสธตาม policy
handshake มี timeout, size limit, hash และ compatibility reason

## 12. Testing/tooling

Contract: SDK reference, lifecycle/isolation, dependency order/cycle/version, duplicate ID, invalid namespace/path, permission
Content: item/recipe, ingredient/output/skill/workstation, admin-only visibility, craft success/failure, save/load/migration, companion/fallback
Override/runtime: cancel, priority, conflict ownership, after-commit, handler failure, handshake, multiplayer, restart, reload/rollback, duplicate handler/timer/buff/file lock, 120 tps, quota

เครื่องมือ: tools/ModHarness, tools/ModDoctor, tools/ModPack, tools/ModTemplate

## 13. Roadmap

### Phase 0: Freeze bootstrap
- [ ] ระบุจุดที่ Assembly-CSharp.dll จำเป็นต่อ bootstrap/bridge
- [ ] หยุด feature logic ใน DLL patcher
- [ ] lock ID/version/namespace/event contract
- [ ] baseline DLL/backup และทดสอบมอดตัวอย่าง

### Phase 1: Manifest/loader v2
- [ ] mod.json parser และ directory package
- [ ] validate path/assembly/API/hash/permission
- [ ] dependency graph resolver จริง
- [ ] states Discovered/Ready/Failed/Blocked/Disabled ใน admin
- [ ] deterministic order และ diagnostics
ไฟล์เป้าหมาย: PluginManager.cs, ModManifest.cs, ModDependencyResolver.cs, ModPackageReader.cs

### Phase 2: Registry/data-only content
- [ ] ItemDefinition, RecipeDefinition, IItemRegistry, IRecipeRegistry
- [ ] immutable definitions, registry lock, namespaced resolver
- [ ] registry-backed craft validation
- [ ] custom item persistence/migration
- [ ] ExampleContentMod: item + recipe + test command
- [ ] admin-only server visibility filter
ไฟล์เป้าหมาย: mod-sdk/Content, server Modding registries, CustomItemStore.cs

### Phase 3: Typed events/override
- [ ] typed Craft/Build/Combat/Inventory contexts
- [ ] operation pipeline กลาง
- [ ] OverrideDecision, ownership, conflict diagnostics
- [ ] compatibility adapter สำหรับ string events
- [ ] handler budget/exception isolation

### Phase 4: Paired client/content
- [ ] client resolver, asset/UI registry และ mod channel
- [ ] server manifest handshake
- [ ] base prototype/placeholder fallback
- [ ] asset hash verification
- [ ] example paired mod แสดง icon/model/localization

### Phase 5: Lifecycle/reload/tooling
- [ ] scheduler/handle disposal
- [ ] storage transaction/migration/atomic save
- [ ] server reload + rollback
- [ ] client code mod restart-required
- [ ] ModDoctor/ModPack/ModHarness

### Phase 6: ย้ายงานเดิมออกจาก DLL
- [ ] craft/admin recipe visibility ไป Dinoworld.Core server mod
- [ ] rest buff/status ไป StatusEffect registry
- [ ] MemoryBot ไป stable client API/mod channel
- [ ] UI/content presentation ไป client companion
- [ ] DllPatcher เหลือ bootstrap only
- [ ] เพิ่ม feature ใหม่โดยไม่เปลี่ยน game feature DLL

### Phase 7: Production hardening
- [ ] signed package/allowlist
- [ ] capability review
- [ ] metrics/slow-handler alert
- [ ] multiplayer/stress/security suite
- [ ] SDK v2 compatibility matrix/migration guide

## 14. แผนงานพรุ่งนี้ — 2026-08-29

เป้าหมาย: ทำฐาน loader v2 และ content contract ให้พร้อมเริ่มเขียน mod เพิ่ม item โดยไม่แตะ feature patch ใน game DLL

ช่วง 1: baseline/safety
- [ ] ตรวจ active DLL, backup และ tools/DllPatcher
- [ ] ยืนยัน patcher เหลือ bootstrap/integration
- [ ] ตรวจ ExampleMod, ExampleGameplayMod, MemoryBotMod
- [ ] เพิ่ม test command แสดง loaded mod/API/dependency

ช่วง 2: manifest/loader
- [ ] สร้าง ModManifest model/parser แบบ read-only
- [ ] validate id/version/path/assembly/API/capability
- [ ] dependency graph resolver พร้อม missing/cycle test
- [ ] เพิ่ม diagnostics ใน /admin/mods โดยไม่ reload ระหว่าง world run

ช่วง 3: content contract
- [ ] ร่าง ItemDefinition/RecipeDefinition ใน SDK รุ่นทดลอง
- [ ] กำหนด schema items.json/recipes.json
- [ ] กำหนด stable ID, BasePrototypeId และ admin/player visibility
- [ ] สร้าง skeleton ExampleContentMod โดยยังไม่ผูก UI

ช่วง 4: acceptance
- [ ] mod ใหม่โหลดจากโฟลเดอร์แยกได้
- [ ] dependency/collision ผิดพลาดแล้วเป็น Blocked ไม่ทำ server ล่ม
- [ ] registry อ่าน item/recipe และรายงาน ID ซ้ำ
- [ ] งานพรุ่งนี้ไม่แก้ Assembly-CSharp.dll
- [ ] สรุป blocker ลง handoff และ Discord

Deliverables: manifest/loader design, content schema รุ่นแรก, ExampleContentMod skeleton/test fixture, test matrix และรายการ API ที่ต้อง freeze
ไม่รวม true hot reload, custom 3D asset และ production signing ในวันเดียวกัน

## 15. Migration mapping

- ServerPlayer hooks -> typed operation pipeline + compatibility events
- ExampleGameplayMod -> typed override mod
- item/recipe -> IItemRegistry + IRecipeRegistry
- admin-only recipe -> definition flags + server filter + permission
- model เดิม -> BasePrototypeId
- icon/model ใหม่ -> client companion + asset pack + hash
- MemoryBot -> stable client API + approved channel
- rest buff -> IStatusEffectRegistry + player status API
- UI/hot reload workaround -> client UI registry; code reload restart-required
- DLL feature patch -> bootstrap/bridge only

## 16. Definition of Done

- mod แยกอ้างเฉพาะ SDK และมี manifest/dependency/permission/version
- เพิ่ม namespaced item/recipe โดยไม่แก้ Assembly-CSharp.dll
- server ตรวจวัตถุดิบ/skill/workstation/output และ persist item ได้
- mod อื่น override/cancel craft แบบ deterministic
- client manifest/fallback/companion icon-model ทำงาน
- admin-only content ไม่โผล่ player view และ server บังคับซ้ำ
- error isolate, metrics, clean shutdown/reload และ tests ผ่าน

Open decisions ก่อน SDK v2: .dmod หรือ directory, signature, policy ตอนถอดมอด, fallback/reject, reload strategy, typed context freeze และขอบเขต gameplay.override
