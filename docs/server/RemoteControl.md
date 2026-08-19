# รีโมทคุมตัวละครด้วย packet (แนว OpenKore)

**ไฟล์:** `ServerCore/ServerPlayer.RemoteControl.cs` · คำสั่งอยู่ใน `ServerCore/ServerPlayer.Cheat.cs`
**ตัวสั่ง:** `test-client` โหมด `--console` คำสั่ง `control`

เป้าหมาย: สั่งตัวละคร **ที่ล็อกอินอยู่ในตัวเกมจริง** ให้เดิน/เก็บของ/ตี ด้วย packet ล้วน
ไม่ต้องแตะเมาส์หรือคีย์บอร์ด — เอาไว้เทสว่า "ที่ server ส่งไป client วาดออกมาถูกไหม"

## วิธีใช้

```bash
cd test-client
dotnet run --no-build -- --console 127.0.0.1 8191 bot-op

> control ฟหกฟหก status              # ดูว่าอยู่ tile ไหน เลือดเท่าไร
> control ฟหกฟหก walk 45 182         # เดินไปทีละก้าว
> control ฟหกฟหก tp 40 177           # วาร์ปทันที
> control ฟหกฟหก gather              # แตะ+เก็บของธรรมชาติที่ใกล้ที่สุด
> control ฟหกฟหก attack              # ตีสัตว์ที่ใกล้ที่สุด
> control ฟหกฟหก say สวัสดี           # พูดแทน
> control ฟหกฟหก stop                # หยุดเดิน

# ── กล่องเครื่องมือตอนเล่นเอง (15 ส.ค.) ──
> control ฟหกฟหก spawn 2017          # เสกสัตว์มาเกิด "ข้างตัวคนนั้น" (ไม่ใส่เลข = สุ่มชนิด)
> control ฟหกฟหก kill                # ฆ่าสัตว์ตัวใกล้ที่สุดของคนนั้น → ได้ซากไว้เทสแล่เนื้อ
> control ฟหกฟหก heal                # เลือด/สตามินาเต็ม (ตายอยู่ = ฟื้นให้)
> control ฟหกฟหก give axe            # เสกของ: axe · clothes · bonfire · box · knife · stone
> control ฟหกฟหก give cook           # ชุดทำอาหารครบเซ็ต (เนื้อ/กิ่งไม้/น้ำ/หม้อ/เตาย่าง/กองไฟ)
> control ฟหกฟหก give meat           # หรือระบุชื่อ prototype ตรง ๆ ก็ได้
> cheat who                          # ใครออนไลน์บ้าง (ชื่อ · id · tile · เลเวล)

# ── สั่งให้ "เล่นเกม" จริง ๆ (18 ส.ค. — ServerPlayer.RemoteDrive.cs) ──
> control ฟหกฟหก place bonfire       # วางของจากแคปซูลลงตรงที่ยืน (ไม่ระบุ = แคปซูลชิ้นแรก)
> control ฟหกฟหก craft skewer        # คราฟต์ — เลือกวัตถุดิบ/เครื่องมือ/โต๊ะรอบตัวให้เอง
> control ฟหกฟหก eat meat            # กินของในกระเป๋า (ไม่ระบุ = อะไรก็ได้ที่กินได้)
> control ฟหกฟหก bag                 # ของในกระเป๋าแบบย่อ (รวมชิ้นซ้ำ + บอกว่าอันไหนแปรรูปแล้ว)
> control ฟหกฟหก prof                # ความชำนาญของทุกหมวดที่ขึ้นแล้ว + เลเวล + แต้มสกิล
```

## สคริปต์เล่นให้อัตโนมัติ — `tools/drive-game.ps1`

พิมพ์ทีละคำสั่งเทสได้ แต่ถ้าจะไล่ทั้งวงจรทุกครั้งมันช้า จึงมีสคริปต์ที่ร้อยคำสั่งเป็นชุด

```powershell
powershell -File tools\drive-game.ps1 -List                    # ดูชุดที่มี
powershell -File tools\drive-game.ps1 -Scenario cook           # เล่นทั้งวงจรทำอาหารให้ดู
powershell -File tools\drive-game.ps1 -Scenario craft-loop     # คราฟต์ซ้ำ ๆ ดูความชำนาญไต่
powershell -File tools\drive-game.ps1 -Scenario cook -Shots    # เก็บภาพหน้าจอเกมก่อน/หลัง
powershell -File tools\drive-game.ps1 -Cmd "walk 40 177","place bonfire","craft skewer"
```

ชุดที่มี: `status` · `cook` · `cook-tier` · `gather` · `craft-loop` · `hunt`

- **หาเป้าหมายให้เอง** — ถ้ามีผู้เล่นออนไลน์คนเดียวก็ยิงใส่คนนั้นเลย (ไม่ต้องพิมพ์ชื่อไทย/เกาหลี)
  มีหลายคนค่อยระบุ `-Target <ชื่อ|entityId>`
- **`-Shots` เก็บภาพเฉพาะหน้าต่างเกม** ลง `shots\` — ไม่ยุ่งกับเคอร์เซอร์

## ทำไมไม่ใช้เมาส์

คุมด้วยเมาส์ = แย่งเดสก์ท็อป · พังทันทีที่หน้าต่างขยับ · เทสทิ้งไว้ไม่ได้
ทางนี้สั่งผ่าน **เซิร์ฟ** ไปที่ตัวละครที่ล็อกอินอยู่แล้ว ผู้เล่นแค่นั่งดูตัวเองทำงาน

และที่สำคัญ: **ทุกคำสั่งวิ่งผ่าน handler เดิมของเกมทั้งหมด** (`HandleCraft` / `HandleUseItem` /
`HandlePlaceCapsulatedArtifact`) ไม่ได้ลัดไปแก้ state ตรง ๆ — เงื่อนไขกันโกงทุกข้อยังบังคับเหมือนเดิม
ถ้าลัด ก็ไม่ได้เทสอะไรเลย

4 คำสั่งล่างเกิดจากปัญหาจริงตอนเทส: ยืนอยู่ในเกมแล้วต้อง **เดินหาสัตว์เป็นนาที** กว่าจะเทสได้สักรอบ
ตอนนี้กดจากเมนู `เทสเกม.bat` ข้อ 5 ให้สัตว์โผล่ตรงหน้าได้เลย

> `kill` ให้เครดิตการฆ่ากับ **คนที่ถูกสั่ง** ไม่ใช่ admin — ซากจะได้เรืองแสงให้คนที่ยืนอยู่ตรงนั้นจริง ๆ
> `heal` ตอนตายอยู่เรียก `ReviveAtSpawn()` ไม่ใช่ `HandleRevive()` เพราะ `Revived` เป็น "คำตอบ"
> ของ packet ที่ client ไม่เคยส่งมา (ReplyOf = 0 จะไปชนคีย์ reply ของ client — ดูกับดักข้อ 5 ใน HANDOFF)
ระบุด้วย **ชื่อ** (ขึ้นต้นตรงก็พอ) หรือ **entity id** ก็ได้ · คำสั่งวิ่งผ่าน packet `Cheat`
จึงไม่ต้องเพิ่ม packet ชนิดใหม่ให้ client รู้จัก

สั่งรวดเดียวแบบไม่ต้องพิมพ์:
```bash
dotnet run --no-build -- --console 127.0.0.1 8191 bot-op --cmd "control ฟหกฟหก walk 45 182; wait 10; control ฟหกฟหก attack"
```

## ทำไมต้องเดินด้วย "วาร์ปทีละก้าว"

ตัวเกม **ไม่ยอมให้ server สั่ง `Move` ตัวผู้เล่นเอง**:

```csharp
// client/PlayerManager.cs
public bool HandleMoveMsg(Move msg)
{
    bool flag = msg.EntityId == PlayerBehavior.LocalPlayer.EntityId;
    PlayerBehavior player = GetPlayer(msg.EntityId);   // _players ไม่มี local player อยู่ในนั้น
    if (player != null) player.HandleMoveMsg(msg);     // ⇒ ตัวเราเองไม่ถูกขยับ
    return flag || player != null;
}
```
`_players` เก็บเฉพาะผู้เล่น**คนอื่น** (ดู `PlayerManager` ตอนรับ `AppearPlayer`: ถ้าเป็นตัวเราจะไปตั้ง
`PlayerBehavior.LocalPlayer` แทนการใส่ dict) การเคลื่อนที่ของตัวเองจึงเป็นของ client ล้วน ๆ

แต่ `Teleported {Tile, Type}` เข้า `PlayerController.Teleport()` ตรง ๆ — ย้ายได้เสมอ
`ControlWalk` เลยยิง `Teleported` ทีละ 1 tile ทุก 0.35 วินาที (สูงสุด 120 ก้าวต่อคำสั่ง)
พร้อม broadcast `Move` ให้ **คนอื่น** เห็นเป็นการเดินลื่น ๆ ตามปกติ

| คำสั่ง | ตัวเราเห็น | คนอื่นเห็น |
|---|---|---|
| `tp` | วาร์ปทันที (`Teleported`) | `Move` 0.2 วิ |
| `walk` | วาร์ปทีละ tile ทุก 0.35 วิ | `Move` ต่อเนื่องทีละก้าว |

## เก็บของ/ต่อสู้ทำงานยังไง

`gather`/`attack` **ไม่ได้ปลอม packet** — มันเรียก handler ตัวเดียวกับที่ client เรียกจริง
(`HandleTouch` → `HandleCollect`, `HandleUseBattleAction`) ในบริบทของผู้เล่นคนนั้น
กฎทุกข้อจึงยังบังคับใช้ครบ: ระยะเอื้อม (GP-09) · สตามินา · คูลดาวน์ · ท่าต้องตรงกับอาวุธ · ตายแล้วทำไม่ได้

⇒ ถ้าสั่งแล้วโดนปฏิเสธ แปลว่า**กฎทำงานถูก** ไม่ใช่รีโมทพัง (ดู log `[control]` กับ `[combat]` ที่ server)

## ข้อควรรู้

- `Cheat` เปิดให้ client ไหนก็สั่งได้ (เหมือน cheat อื่น ๆ ในโปรเจกต์) — เซิร์ฟสาธารณะต้องปิดหรือใส่ระบบสิทธิ์ก่อน
- คุมได้เฉพาะคน**ที่ออนไลน์อยู่**
- ตัวละครที่ถูกคุมยังกดเองได้ตามปกติ ถ้าเจ้าตัวเดินสวนทาง คำสั่ง walk จะสู้กับ input ของเจ้าตัว
- `walk` ตายกลางทาง/มีคำสั่งใหม่ = ยกเลิกคิวเดิมอัตโนมัติ (`_walkToken`)

## เทียบกับบอทคอนโซลปกติ

| | บอทคอนโซล (`--console`) | รีโมทคุม (`control`) |
|---|---|---|
| ตัวละคร | สร้าง entity ใหม่ของตัวเอง | ตัวละครจริงของคนที่เล่นอยู่ |
| เห็นในจอเกมไหม | เห็น (เป็นผู้เล่นอีกคน) | เห็น เพราะเป็นตัวเราเอง |
| ใช้ทำอะไร | ทดสอบ server แบบไม่ต้องเปิดเกม | ทดสอบว่า client วาด/ขยับตามที่ server สั่งจริงไหม |
