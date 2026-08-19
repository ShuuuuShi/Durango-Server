# แก้ตัวเกม: build จากซอร์สได้เลย ไม่ต้อง patch แล้ว

**ทำได้จริง เทสแล้ว** — เกมบูตขึ้น เข้าเซิร์ฟได้ ตัวละครเข้าโลกได้ ด้วย DLL ที่ build จาก `client/`

```bash
# วิธีที่สั้นที่สุด
เทสเกม.bat → ข้อ 18            # build จากซอร์ส + วางลงเกมให้เลย (สำรองของเดิมอัตโนมัติ)
เทสเกม.bat → ข้อ 19            # ย้อนกลับ DLL อันก่อนหน้าถ้าพัง

# หรือสั่งเอง
powershell -File tools\build-client.ps1
powershell -File tools\build-client.ps1 -Restore
powershell -File tools\build-client.ps1 -NoInstall    # build เฉย ๆ ไม่แตะเกม
```

---

## ทำไมถึงทำได้

`client/` คือซอร์ส 3,760 ไฟล์ที่ถอดจาก `Assembly-CSharp.dll` ด้วย ILSpy
มี `client/Assembly-CSharp.csproj` อ้าง DLL ของ Unity **ในโฟลเดอร์เกมโดยตรง** (`game/DurangoV2_Data/Managed/`)
build ด้วย `dotnet build -c Release` ใช้เวลาไม่กี่วินาที ได้ `Assembly-CSharp.dll` ออกมาแทนของเดิมได้เลย

| | patch ด้วย IL (`tools/DllPatcher`) | **แก้ซอร์สแล้ว build** |
|---|---|---|
| แก้อะไรได้ | เท่าที่เขียน IL ไหว | **ทั้งเกม** |
| อ่านง่ายไหม | ต้องเขียน `OpCodes.Ldarg_0` เอง | C# ธรรมดา |
| แก้หลายจุดพร้อมกัน | ยิ่งเยอะยิ่งเปราะ | เหมือนแก้โปรเจกต์ปกติ |
| เห็น error ตอนไหน | ตอนรันเกม | **ตอน build** |

### ผลการทดสอบ (16 ส.ค. 2026)

| เรื่อง | ผล |
|---|---|
| build | 0 error (~5 วินาที) · ได้ DLL 5.8 MB |
| เกมบูต | ✅ ถึงหน้าไตเติ้ล ไม่มี TypeLoad/MissingMethod |
| ต่อเซิร์ฟ | ✅ `[world] player joined: ฟหกฟหก level=60` |
| คำเตือน "referenced script is missing" | มี 1 อัน (`CombatModeButton`) — **มีอยู่แล้วตั้งแต่ DLL ที่ patch** ไม่ใช่ของใหม่ |

> รอบทดลองแรก (ก่อนพอร์ต patch เข้าซอร์ส) เจอ 4 อัน — พอพอร์ตครบเหลือ 1 เท่าเดิม

---

## patch เดิมย้ายเข้าซอร์สแล้ว

| เดิม patch ที่ไหน | ตอนนี้อยู่ในซอร์สที่ |
|---|---|
| `PatchAutoConnect` (env `DURANGO_AUTOCONNECT`) | `Durango.Offline/Server.cs → BeginServer` |
| `ConnectTo` ฮาร์ดโค้ดพอร์ต 8190 | `Durango.Offline/Server.cs → ConnectTo` (รับ `ip` หรือ `ip:port`) |
| `PatchHideUnimplementedMenus` (24 เมนู) | `MenuSystem.cs → NotImplementedYet` |
| `PatchServerAnimalSpawn` | `AnimalManager.cs` (handler ของ `AppearAnimal`) |
| `PatchSelfIpFilter` | `Durango.UI/MenuListGroupBase.cs → OnSelectItem` |
| `PatchAppDataBasePath` · `ForceLocalAssetBundles` · `GuardTitleWidget` · `GetIslandPort` | **อยู่ในซอร์สอยู่แล้ว** (ซอร์สถอดมาจาก DLL ที่ patch รอบแรกไปแล้ว) |

ทุกจุดที่แก้เองมีคอมเมนต์ `[แก้เอง]` กำกับไว้ — `grep -rn "\[แก้เอง\]" client/` เห็นครบทุกจุด

**`tools/DllPatcher` ยังเก็บไว้** เผื่อต้องแก้ DLL ที่ไม่มีซอร์ส แต่ปกติไม่ต้องใช้แล้ว

---

## ของใหม่ที่เพิ่มเข้าไปตอนย้าย: เดินทางข้ามเกาะ

ระบบเกาะแยกเลเวลติดตรงที่ client ฮาร์ดโค้ด gateway 8190 และไม่มี packet "ย้ายไปเซิร์ฟอื่น"
พอแก้ซอร์สได้ก็ทำให้จบได้เลย:

```csharp
// GameManager.cs — server ส่ง Info "##goto <ip:port>" แล้วตามด้วย Emigrated
public static string PendingIslandAddress { get; set; }

private static void DefaultInfoHandler(Info msg, PacketHeader header)
{
    if (!string.IsNullOrEmpty(msg.Text) && msg.Text.StartsWith("##goto "))
    {
        PendingIslandAddress = msg.Text.Substring("##goto ".Length).Trim();
    }
}

// Frontend_ConnectionClosed — ต่อไปเกาะปลายทางแทนที่จะกลับเกาะเดิม
if (!string.IsNullOrEmpty(PendingIslandAddress)) { ... Server.ConnectTo(target); return; }
```

ฝั่ง server อยู่ที่ `ServerPlayer.Travel.cs` (`cheat travel <รหัสเกาะ>`) — ดู `docs/server/Islands.md`
⚠️ **ยังไม่ได้เทสกับเกมจริง** เพิ่งเขียนเสร็จ

---

## ข้อควรระวัง

1. **ปิดเกมก่อน build** — DLL ถูกล็อกอยู่ (สคริปต์ปิดให้เอง)
2. **สำรองอัตโนมัติ** ทุกครั้งที่วางลงเกม เก็บ 10 อันล่าสุดใน `game-backup/` ย้อนกลับด้วย `-Restore`
3. **อย่าเปลี่ยนชื่อคลาส/ฟิลด์ที่ Unity ใช้** — scene/prefab ผูกกับ MonoBehaviour ด้วยชื่อคลาส
   และ deserialize ฟิลด์ด้วยชื่อฟิลด์ เปลี่ยนแล้วของในฉากจะหลุด (อาการ: "referenced script is missing")
4. ซอร์สที่ ILSpy ถอดมาอาจต่างจากของเดิมในรายละเอียดบางจุด (state machine ของ `yield`, switch บน string)
   — ถ้าเจออาการแปลกหลัง build ให้เทียบกับ DLL สำรองก่อนว่าเป็นของเดิมหรือของใหม่
5. `LangVersion` ตั้งไว้ที่ 11 แต่ target เป็น `net35` (Mono ของ Unity 2017) — เขียน C# ใหม่ ๆ ได้
   แต่อย่าใช้ฟีเจอร์ที่ต้องพึ่ง type ใน .NET รุ่นใหม่ (record, Span, init-only)
