# Durango Map Editor Specification

## 1. เป้าหมาย

สร้างโปรแกรม Map Editor ที่สามารถอ่านแมพจริงจากโฟลเดอร์เกม Durango, แสดงผลแมพ, แก้ไขข้อมูลแมพ, บันทึกเป็นโปรเจกต์ และ Export ไปใช้กับเซิร์ฟเวอร์/เกมจริงได้

โฟลเดอร์เกมสำหรับทดสอบ:

```text
C:\Users\thana\Desktop\Durango Opencode\game
```

เกมใช้ Unity `2017.4.34f1` ไฟล์สำคัญอยู่ใน `game/DurangoV2_Data` เช่น `resources.assets`, `level*`, `sharedassets*` และ AssetBundles

## 2. หลักการสำคัญ

- ต้องอ่านข้อมูลจากโฟลเดอร์เกมจริง ไม่ใช่สร้างแมพจำลองขึ้นมาเอง
- ต้องรักษาพิกัด ขนาดแมพ chunk และชนิด object ให้ตรงกับเกม
- ห้ามแก้ไฟล์เกมต้นฉบับโดยตรงในช่วงแรก
- ทุกการ Export ต้องสร้าง backup และสามารถ Rollback ได้
- Client และ Server ต้องใช้ `map_id`, `map_version` และ hash ของแมพชุดเดียวกัน
- `resources.assets` ของชุดทดสอบมี Unity warning เรื่อง corrupted จึงต้องมีระบบตรวจสอบและรายงานข้อผิดพลาดก่อนใช้งาน

## 3. โครงสร้างโปรแกรมที่แนะนำ

แนะนำให้สร้างเป็น Unity Editor แยกจากตัวเกม โดยใช้ Unity `2017.4.34f1` เพื่อให้โหลด prefab, texture, material, terrain และ collision ได้ตรงกับเกม

```text
tools/MapEditor/
  spec.md
  README.md
  MapEditor.sln
  MapEditor/
  Importer/
  Renderer/
  EditorUI/
  Exporter/
  Models/
  Tests/
```

## 4. รูปแบบ Map Project

การแก้ไขต้องบันทึกเป็นโปรเจกต์แยกจากไฟล์เกม:

```text
map-project/
  project.json
  map.json
  objects.json
  spawn-points.json
  terrain/
    biome.bin
    ocean.bin
    river.bin
    landmark.bin
    garden.bin
    heightmap.bin
  backup/
  exports/
```

`project.json` ต้องเก็บอย่างน้อย:

- source game folder
- map id
- map version
- Unity/game version
- source file hashes
- export time
- editor version

## 5. ระยะที่ 1: Import และ Map Viewer

### TODO

- [ ] เลือกและตรวจสอบ Game Folder
- [ ] ตรวจสอบ `DurangoV2_Data`
- [ ] ค้นหา `resources.assets`, `level*`, `sharedassets*` และ AssetBundles
- [ ] อ่าน terrain metadata
- [ ] อ่าน Width/Height และขนาด chunk
- [ ] อ่าน biome, ocean, river, landmark และ garden
- [ ] แสดงแมพแบบ 2D top-down
- [ ] แสดง grid, tile coordinate และ chunk boundary
- [ ] แสดง entry point/spawn point
- [ ] แสดง object และ natural object บนแมพ
- [ ] มี Reload, Clear Cache และ Export Report
- [ ] แจ้งไฟล์ที่อ่านไม่ได้โดยไม่ทำให้โปรแกรมหยุดทำงาน

### เกณฑ์ผ่าน

- เลือกโฟลเดอร์เกมจริงได้
- เปิดดูแมพจริงจาก `DurangoV2_Data` ได้
- เห็นรูปร่างเกาะ น้ำ biome และ chunk ใกล้เคียงกับในเกม
- ยังไม่เขียนทับไฟล์ต้นฉบับ

## 6. ระยะที่ 2: แก้ไขแมพ

### TODO

- [ ] Brush แก้ biome
- [ ] Brush แก้พื้นดิน น้ำ และแม่น้ำ
- [ ] เพิ่ม/ลบต้นไม้ หิน และ natural object
- [ ] เพิ่ม/ลบ landmark
- [ ] Select, Move และ Delete object
- [ ] แก้ entry point และ spawn point
- [ ] แสดงชนิด object และพิกัดเมื่อคลิก
- [ ] Undo/Redo
- [ ] Autosave
- [ ] Validate ก่อนบันทึก
- [ ] บันทึกลง Map Project แทนการแก้ไฟล์เกมโดยตรง

### เกณฑ์ผ่าน

- แก้ข้อมูลแล้วปิดโปรแกรมได้โดยข้อมูลไม่หาย
- เปิดโปรเจกต์เดิมกลับมาแล้วเห็นการแก้ไข
- Undo/Redo ทำงาน
- ข้อมูลผิดรูปแบบถูกแจ้งเตือนและไม่ถูก Export

## 7. ระยะที่ 3: Export และ Server Integration

Server ปัจจุบันอ่าน terrain ผ่าน `server/ServerCore/TerrainStore.cs` และข้อมูลใน `server/data/terrains` ดังนั้น Exporter ต้องสร้างรูปแบบที่ TerrainStore อ่านได้

### TODO

- [ ] Export biome/ocean/river/landmark/garden
- [ ] Export terrain metadata และ entry point
- [ ] Export spawn point
- [ ] สร้าง `map_id`, `map_version` และ file hash
- [ ] เพิ่ม validation ของขนาด array และ chunk boundary
- [ ] Deploy ไป `server/data/terrains`
- [ ] Backup terrain เดิมก่อน Deploy
- [ ] เพิ่ม Rollback ตาม version
- [ ] แสดงผล Deploy สำเร็จ/ล้มเหลว
- [ ] ตรวจสอบว่า Server โหลด terrain ใหม่ได้หลัง restart

### เกณฑ์ผ่าน

- Server โหลดแมพที่ Export ได้
- Client เข้าแมพได้
- biome, น้ำ, natural และจุดเกิดตรงกันระหว่าง Client/Server
- Restart แล้วข้อมูลยังอยู่
- Rollback กลับเวอร์ชันเดิมได้

## 8. ระยะที่ 4: Preview ในเกมจริง

### TODO

- [ ] สร้าง temporary export สำหรับทดสอบ
- [ ] เปิด Server ด้วย terrain ที่แก้ไข
- [ ] เปิด Client จากโฟลเดอร์เกม
- [ ] ทดสอบจุดเกิด
- [ ] ทดสอบการเดินข้าม chunk
- [ ] ทดสอบพื้นดิน/น้ำ/แม่น้ำ
- [ ] ทดสอบต้นไม้และหิน
- [ ] ทดสอบ collision และความสูงพื้น
- [ ] ทดสอบ restart server
- [ ] เก็บ screenshot และ log เป็นหลักฐาน

## 9. Heightmap และ Collision

ต้องตรวจสอบและรองรับ heightmap, collision และ navigation ให้ตรงกับ Client เพราะปัจจุบัน Server ยังไม่มี heightmap authority เต็มรูปแบบและบางส่วนอาศัยค่าที่ Client รายงานมา

หากยังถอด heightmap จาก asset จริงไม่ได้ ให้ระบุเป็นข้อจำกัดใน report และห้ามอ้างว่าแก้ภูมิประเทศ 3D ได้สมบูรณ์

## 10. ความปลอดภัยและข้อห้าม

- ห้ามลบ save ตัวละครหรือ world save
- ห้ามลบไฟล์ในโฟลเดอร์เกมอัตโนมัติ
- ห้ามเขียนทับ `resources.assets` หรือ `level*` โดยไม่มี backup
- ใช้ temporary directory ระหว่าง Import/Export
- ถ้า validation ไม่ผ่านต้องหยุด Deploy
- ต้องแสดงไฟล์ที่จะถูกแก้ก่อนยืนยัน
- เก็บประวัติทุก Export
- ไม่เปลี่ยน Unity version โดยไม่ตรวจ compatibility
- ต้องไม่ทำให้ Client เดิมเปิดเกมไม่ได้

## 11. การทดสอบที่ต้องมี

- [ ] Import แมพจริงจากโฟลเดอร์เกม
- [ ] Import ซ้ำโดยไม่สร้างข้อมูลซ้ำ
- [ ] เปิด project ที่บันทึกไว้
- [ ] แก้ biome แล้ว Reload ตรวจสอบ
- [ ] เพิ่ม/ลบ object แล้ว Reload ตรวจสอบ
- [ ] Undo/Redo
- [ ] Export และตรวจ hash
- [ ] Deploy และ Rollback
- [ ] Server restart persistence
- [ ] Client เข้าแมพจริง
- [ ] ตรวจสอบ map version ไม่ตรงแล้วแจ้งเตือน

## 12. Definition of Done

งานถือว่าเสร็จเมื่อผู้ใช้สามารถเลือกโฟลเดอร์เกมจริง, เปิดดูแมพจริง, แก้ biome/object/spawn point, บันทึกเป็น Map Project, Export ไป Server และเปิด Client เข้าแมพที่แก้ไขแล้วเห็นผลจริง โดยมี backup, validation และ rollback ครบ

## 13. ลำดับการทำงานที่ต้องยึด

1. ทำ Importer แบบอ่านอย่างเดียว
2. ทำ Map Viewer
3. ทำ Map Project และระบบ backup
4. ทำเครื่องมือแก้ไข biome/object
5. ทำ Exporter ให้เข้ากับ TerrainStore
6. เชื่อม Server
7. ทดสอบกับ Client จริง
8. ค่อยพิจารณาการเขียนกลับ AssetBundle หรือ Unity asset โดยตรง

