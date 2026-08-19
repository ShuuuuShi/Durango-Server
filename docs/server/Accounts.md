# เจ้าของตัวละคร + สิทธิ์ admin (H-1 / H-2)

**ไฟล์:** `ServerCore/AccountStore.cs` · `ServerCore/Gateway.cs` (`/sessions`) · `ServerCore/ServerPlayer.Cheat.cs`

## H-1 — กันสวมรอยตัวละคร

**ปัญหา:** `POST /sessions` ให้ client บอก entity id ของตัวเองมาดื้อ ๆ แต่ entity id เป็นของสาธารณะ
(มากับ `AppearPlayer` / `Move` / `Damaged` ที่ broadcast ให้ทุกคน)
⇒ ใครเห็น id ของคนอื่นก็ขอ token ของเขาได้ แล้วเข้าเกมด้วยตัวละคร + ของทั้งหมดของเขา
พอ logout ยังเขียนทับไฟล์เซฟของเจ้าตัวอีก (GP-12 แก้แค่ "token ต้องเป็นของที่ server ออก" ไม่ได้แก้ "ใครขอ id ไหนก็ได้")

**ข้อจำกัด:** ตัวเกมไม่ได้ส่งรหัสผ่านอะไรมาเลย — `/sessions` มีแค่ `player_info` = ชื่อ, เลเวล, entity id
และเราแก้ตัว client ไม่ได้ จึงกันฝั่ง server ด้วย 2 ชั้น:

| ชั้น | ทำอะไร | ปิดด้วย |
|---|---|---|
| 1. รายชื่อที่อนุญาต | มีไฟล์รายชื่อเมื่อไหร่ คนนอกรายชื่อขอ token ไม่ได้เลย (ตอบ 403) | ไม่ตั้ง `--whitelist` |
| 2. จองตอนเข้าครั้งแรก | entity id ผูกกับ IP ที่จองครั้งแรก · IP อื่นมาอ้าง id เดิม = ปฏิเสธ | `--no-ip-bind` |

ทั้งสองชั้นปิดพร้อมกันด้วย `--no-account-check` (ใช้ตอนเทสในเครื่องเดียว)

### วิธีใช้

```bash
# เปิดเซิร์ฟให้เฉพาะคนในรายชื่อ
dotnet run -- --whitelist data/whitelist.txt

# ทุกคนอยู่หลังเราเตอร์เดียวกัน/เน็ตเปลี่ยน IP บ่อย → ปิดการผูก IP
dotnet run -- --whitelist data/whitelist.txt --no-ip-bind
```

`data/whitelist.txt` — บรรทัดละ 1 รายการ, ใส่ **entity id หรือชื่อตัวละคร** ก็ได้, `#` = คอมเมนต์:
```
# เพื่อนที่ให้เข้าได้
8ae11e65-ac1e-4dfc-9f4d-dc31b0e380a1
alice
```

### ไฟล์การจอง

`saves/accounts/<entityId>.json`
```json
{ "EntityId": "alice", "Name": "alice", "ClaimedFromIp": "127.0.0.1",
  "ClaimedAt": 1786739859.17, "LastSeenAt": 1786739859.17, "Logins": 1 }
```
**เพื่อนย้ายบ้าน/เปลี่ยนเน็ตแล้วเข้าไม่ได้** → ลบไฟล์ของ id นั้นทิ้ง (`AccountStore.Release`) แล้วให้เข้าใหม่เพื่อจองใหม่

### ที่ยังกันไม่ได้

- คนใน whitelist ด้วยกันเอง ถ้าอยู่ IP เดียวกัน (เช่นบ้านเดียวกัน) ยังอ้าง id ของกันและกันได้
- ถ้าไม่ตั้ง whitelist เลย = "ใครมาก่อนจองก่อน" — คนแปลกหน้ายังสร้างตัวละครใหม่ได้เรื่อย ๆ (ดู L-6: ไฟล์เซฟโตไม่จำกัด)
- กันแบบเด็ดขาดต้องมี login จริง ซึ่งต้องแก้ฝั่ง client (patch dll) — ยังไม่ได้ทำ

## H-2 — ปิดคำสั่งทดสอบ (packet `Cheat`)

เดิม client ไหนก็ส่ง `Cheat` ได้ทุกคำสั่ง: เสกของ · ฟื้นเลือด · เรียกสัตว์ไม่จำกัด ·
และ `control` ที่ **ลากตัวละครของคนอื่นไปไหนก็ได้ / พูดแทนเขา / บังคับให้ตีสัตว์จนตาย**

ตอนนี้:

| ค่า | พฤติกรรม |
|---|---|
| ไม่ใส่อะไร (ค่าเริ่มต้น) | ทุกคำสั่งถูกปฏิเสธ + ตอบ `Info` บอกเหตุผล |
| `--enable-cheat` | คำสั่งที่ทำกับ **ตัวเอง** ใช้ได้ (add/rest/hurt/die/tp spawn/spawn/stats) |
| `--enable-cheat --admin <ชื่อ\|id>` | คนในรายชื่อ admin ใช้ `control <คนอื่น> ...` ได้ด้วย |

ไม่ได้ตั้ง `--admin` = ไม่มีใครใช้ `control` ได้เลย แม้เปิด `--enable-cheat`

```bash
# เซิร์ฟจริง
dotnet run -- --whitelist data/whitelist.txt

# เทส (เปิด cheat ให้ตัวเอง + ให้เจ้าของเครื่องเป็น admin)
dotnet run -- --enable-cheat --admin "ฟหกฟหก"
```

ดูรายการคำสั่งทั้งหมดที่ [ServerPlayer.Cheat.md](ServerPlayer.Cheat.md) และการคุมตัวละครที่ [RemoteControl.md](RemoteControl.md)
