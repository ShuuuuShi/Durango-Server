# Durango Asset Browser

โปรแกรมสำหรับดูว่าภายใน assets ของเกม Durango (Unity) มีอะไรบ้าง —
แสดงผล **โมเดล (Mesh)**, **ไอคอน/กราฟฟิก (Texture2D, Sprite)**, **Material**,
และ metadata ของ GameObject / AnimationClip / Font / TextAsset ฯลฯ
พร้อมหน้าเว็บ gallery ที่ค้นหาและกรองได้

## วิธีใช้งาน

ต้องมี Python 3 และไลบรารี:

```bash
pip install UnityPy Pillow numpy
```

รันจากโฟลเดอร์ `tools/asset-browser`:

```bash
# 1) สแกน assets ทั้งหมด -> สร้าง output/catalog.json
python asset_browser.py scan

# 2) สร้างรูป thumbnail (texture/sprite -> png, mesh -> wireframe)
python asset_browser.py previews

# หรือทำทั้งสองอย่างพร้อมกัน
python asset_browser.py build

# 3) เปิด gallery ในเบราว์เซอร์
python asset_browser.py serve   # แล้วเปิด http://127.0.0.1:8787
```

`serve` จะค้างรอรับ request — กด `Ctrl+C` เพื่อหยุด

### คำสั่งเสริม

```bash
# export asset เดียวตาม id (จาก catalog.json)
python asset_browser.py export 1234     # Mesh -> .obj, Texture2D/Sprite -> .png

# สร้าง thumbnail เฉพาะบาง type
python asset_browser.py previews --types Texture2D Mesh

# จำกัดจำนวน thumbnail (สำหรับทดสอบ)
python asset_browser.py previews --limit 100
```

## โฟลเดอร์ output

| ไฟล์ | คำอธิบาย |
|------|----------|
| `output/catalog.json` | รายการ asset ทั้งหมด (id, type, name, source, path_id, extra) |
| `output/thumbs/` | thumbnail PNG (`NNNNNN.png` เรียงตาม id) |
| `output/export/` | ไฟล์ที่ export ออกมา (`export <id>`) |
| `output/index.html` | หน้า gallery ที่ `serve` เปิดให้ |

## แหล่งข้อมูลที่สแกน

- `game/DurangoV2_Data/*.assets` (resources.assets, sharedassets0/2/3, ...)
- `game/DurangoV2_Data/level*` (ฉาก/level)
- `game/DurangoV2_Data/StreamingAssets/AssetBundles/*.bundle` (AssetBundles 2,000+ ไฟล์)

ไฟล์ `.resS` (ข้อมูลดิบของ texture/mesh) จะถูกอ่านอัตโนมัติโดย UnityPy
ผ่านคู่ไฟล์ `.assets`/`.bundle`

## หมายเหตุ

- เกมนี้เป็น NGUI — "ไอคอน" ส่วนใหญ่เป็น region ภายใน atlas Texture2D
  (เช่น `Icon_Atlas`, `Shop_Atlas`) ไม่ใช่ Unity `Sprite` object จึงดูได้จาก
  Texture2D ทั้งผืน
- Mesh preview เป็น wireframe projection (ไม่ใช่ render แสงเงา) เพื่อให้ดูรูปทรงได้เร็ว
- สแกนเต็ม ~90,000 รายการ ใช้เวลาไม่กี่นาที thumbnail ของ mesh/texture
  ใช้เวลานานสุด
