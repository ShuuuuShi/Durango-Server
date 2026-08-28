# Durango Map Editor Core

ระยะนี้เป็น **read-only importer/viewer foundation** ตาม `spec.md` ยังไม่ใช่ exporter หรือระบบ deploy

## ใช้งานจาก command line

จากโฟลเดอร์ `tools/MapEditor`:

```bash
npm test
node bin/import-report.js "../../game" "../../server/data/terrains/extracted/ri35te"
```

คำสั่ง report จะ:

- ตรวจ `DurangoV2_Data`, `resources.assets`, `level*`, `sharedassets*.assets` และ AssetBundle
- คำนวณ SHA-256 ของ Unity source files
- อ่าน `info.yml` และเก็บ unknown metadata
- อ่าน binary layers ที่ server/client ใช้จริง
- ตรวจขนาด array, record alignment, coordinate bounds และ chunk alignment
- รายงาน `resources.assets` เป็น `UNITY_ASSET_INTEGRITY_UNVERIFIED` เพราะต้องตรวจ corruption ด้วย Unity/UnityPy เพิ่มเติม

## Core API

`lib/map-editor-core.js` มี API หลัก:

- `scanGameFolder(gameFolder)`
- `readTerrainSource(terrainFolder)`
- `getTerrainChunk(terrain, chunkX, chunkY)`
- `validateTerrain(terrain)`
- `setBiomeType(rawByte, biomeType)` ซึ่งรักษา flag 2 บิตบน
- `encode/decodeGarden` และ `encode/decodeLandmarks`
- `ProjectStore.save/load`

รูปแบบที่รองรับตรงกับ `TerrainStore`:

- tile 16×16 ต่อ chunk
- biome chunk 18×18 = 324 bytes
- ocean chunk 17×17 = 289 bytes
- river chunk 17×17×3 = 867 bytes
- garden record 6 bytes
- landmark record 16 bytes, little-endian

`oceans.dm` ใช้ชื่อ `coastDistance` และยังไม่ถือเป็น water depth ส่วน `whole.elevations` และ `whole.waterdepths` ถูกเก็บเป็น opaque/unsupported layer จนกว่าจะพิสูจน์ schema ได้

## Electron viewer

`tools/Launcher-Electron/map-editor.html` เชื่อม importer ผ่าน IPC แบบ read-only:

- **นำเข้า terrain** เลือกโฟลเดอร์ terrain ที่สกัดแล้วและแสดง biome จริง, natural, landmark, entry point และ chunk boundary
- **ตรวจโฟลเดอร์เกม** สร้าง source scan report
- **รายงาน** บันทึก report ไปยังไฟล์ที่ผู้ใช้เลือก
- การกด **บันทึก** หลัง import จะถูกป้องกันไว้จนกว่า Map Project checkpoint จะเสร็จ

การ import ไม่เขียนทับไฟล์เกมหรือ server terrain ใด ๆ

## ข้อจำกัดของ checkpoint นี้

- ยังไม่มี biome/object editing ที่บันทึกลง Map Project ผ่าน UI
- ยังไม่มี staged exporter, backup, deploy หรือ rollback
- ยังไม่มี Unity asset write-back
- ความสมบูรณ์ของ `resources.assets` ยังต้องตรวจด้วย Unity/UnityPy
- ยังไม่มีการ decode heightmap/collision/navigation จริง
- map version/hash manifest ถูกเตรียมใน core model แต่ยังไม่ใช่ server protocol integration
