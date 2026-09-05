"""ดึง catalog ภาษา (ไฟล์ .mo ของ gettext) ออกจาก resources.assets ด้วย UnityPy

เกมมี **คำแปลไทยฉบับทางการของ NEXON** ติดมาอยู่แล้ว (33,066 ข้อความ · แปล 2019-12-09)
แต่ใช้ไม่ได้เพราะ LocalizeSystem โหลดผ่าน 2 ทางที่พังทั้งคู่:
  · Resources.Load("offline/i18n/th_TH")  → resources.assets ที่เสีย (ENV-01)
  · new Catalog("messages","locales",..)   → หาโฟลเดอร์ locales/ ข้างตัวเกม ซึ่งไม่มี

สคริปต์นี้แกะ .mo ออกมาเพื่อเอาไปวางที่ทางที่สอง (อ่านจากดิสก์จริง ไม่ผ่าน asset ที่เสีย):
    game/locales/th-TH/LC_MESSAGES/messages.mo

ใช้: python scripts/extract_i18n.py     แล้วก๊อปไฟล์ที่ได้ไปวางเอง
"""
import io
import os
import sys

import UnityPy

SRC = 'C:/Users/thana/Desktop/Durango Claude/game/DurangoV2_Data/resources.assets'
OUTDIR = 'C:/Users/thana/AppData/Local/Temp/claude/C--Users-thana/173e23bd-58f7-4bee-b299-f3b059c72276/scratchpad/i18n'
os.makedirs(OUTDIR, exist_ok=True)

env = UnityPy.load(SRC)
found = []
for obj in env.objects:
    if obj.type.name != 'TextAsset':
        continue
    try:
        data = obj.read()
    except Exception as e:                      # noqa: BLE001
        continue
    name = getattr(data, 'm_Name', None) or getattr(data, 'name', '')
    if not name:
        continue
    low = name.lower()
    if low in ('th_th', 'ko_kr', 'en_us') or 'th_th' in low:
        raw = getattr(data, 'm_Script', None)
        if raw is None:
            raw = getattr(data, 'script', None)
        if isinstance(raw, str):
            raw = raw.encode('utf-8', 'surrogateescape')
        path = os.path.join(OUTDIR, name + '.bin')
        io.open(path, 'wb').write(raw)
        found.append((name, len(raw)))

for n, sz in found:
    print('%-12s %d bytes' % (n, sz))
if not found:
    print('ไม่เจอ TextAsset ชื่อ th_TH / ko_KR / en_US')
