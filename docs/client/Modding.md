# ระบบ Mod ฝั่งเกม (client) — V1.1, เขียน/อัปเดต 27 ส.ค. 2026

คู่กับ `docs/server/Modding.md` (ฝั่งเซิร์ฟ) — โครงสถาปัตยกรรมเหมือนกันทั้งชุด (3 เฟสแบบ
Minecraft/Forge, error isolation ต่อ mod, interface assembly แยกจากตัวเกม) แต่ target
framework คนละตัว: client เป็น **net35** (Unity 2017.4 Mono runtime ของเกม) เซิร์ฟเป็น net9.0

## 1. ภาพรวมสถาปัตยกรรม

```
client-mod-sdk/DurangoClientModSdk.csproj   ← interface (IClientPlugin/IClientModApi/IClientPlayer)
                                              mod อ้างอิงแค่นี้ — อัปเดตเกมแล้ว mod เก่ายังใช้ได้
client/ClientModLoader.cs                   ← loader ใน Assembly-CSharp.dll (build จากซอร์สเรา)
game/mods/*.dll                             ← ใส่ mod ที่ build แล้ว (สแกนตอนเกมบูต ไม่ hot-reload)
tools/ExampleClientMod/                     ← mod ตัวอย่าง สาธิตทุก hook (ทดสอบผ่านแล้ว)
tools/MemoryBotMod/                         ← mod จริงตัวแรก (บอทจับภาพ/ควบคุม) ศึกษาต่อได้
```

- `GameManager.Start()` เรียก `ClientModLoader.LoadAll()` ครั้งเดียวตอนเกมบูต → สแกน
  `game\mods\*.dll` (ข้าง DurangoV2.exe) → `Assembly.LoadFrom` → หา type ที่ implement
  `IClientPlugin` → ไล่ 3 เฟส **PreLoad → Load → PostLoad** "ครบทุก mod ทีละเฟส"
- mod ที่ throw ตอนเฟสไหน = ปิดเฉพาะตัวนั้น (log `[clientmods]`) มองเห็นผ่าน game log
- ตัวขับ `__ClientModDriver` (MonoBehaviour, DontDestroyOnLoad) วิ่ง `Update()` ทุกเฟรมให้ mod:
  hotkey + OnGameReady + OnUpdate + scene/HUD hooks — ไม่พังตามตอนเกมรื้อ scene

## 2. วิธีเขียน client mod ใหม่

1. สร้าง class library **net35** (`<LangVersion>7.3</LangVersion>`, `GenerateAssemblyInfo=False`)
   อ้างอิง `DurangoClientModSdk.dll` (ProjectReference ตอน dev / Reference+HintPath ตอนแจก) —
   ก๊อปแบบ `tools/ExampleClientMod/ExampleClientMod.csproj` ไปได้เลย
2. เขียนคลาสเดียว implement `IClientPlugin`:

```csharp
public sealed class MyPlugin : IClientPlugin
{
    public string Name => "MyPlugin";
    public string Version => "1.0.0";
    public void OnLoad(IClientModApi api)
    {
        api.RegisterHotkey(KeyCode.F10, () =>
        {
            var p = api.LocalPlayer;                 // null ถ้ายังไม่เข้าเกาะ — เช็คเสมอ
            if (p != null) api.ShowMessage("อยู่ที่ " + p.Position);
        });
        api.OnGameReady(() => api.ShowMessage("ยินดีต้อนรับ!"));
    }
    public void OnPreLoad(IClientModApi api) { }
    public void OnPostLoad(IClientModApi api) { }
}
```

3. build + ติดตั้ง:
   ```
   dotnet build tools\ExampleClientMod -c Release          # ตัวอย่าง
   copy tools\ExampleClientMod\bin\Release\net35\<ชื่อmod>.dll game\mods\
   ```
4. เปิดเกมแล้วดู log: `[clientmods] โหลด 'MyPlugin' v1.0.0 ... สำเร็จ (ครบ 3 เฟส)`
   (log เกมอยู่ที่ไฟล์ที่สั่ง `-logFile` ใน `เทสเกม.bat` หรือ `game/client.log`)
5. ปิด/เปิด mod โดยไม่ลบ: ย้าย .dll ระหว่าง `game\mods\` ↔ `game\mods.disabled\`
   หรือให้ mod ปิดตัวเองด้วย env var (แบบ MemoryBotMod: `DURANGO_MEMORYBOT=0`)

⚠️ อย่าเอา `DurangoClientModSdk.dll` ไปวางใน `mods\` — เกมมีตัวของมันอยู่แล้วใน
`Managed\` assembly ชื่อซ้ำสองชุดทำให้ `typeof(IClientPlugin).IsAssignableFrom(...)` เช็ค
ไม่ผ่านแบบเงียบ ๆ (กับดักเดียวกับฝั่งเซิร์ฟ)

## 3. IClientModApi มีอะไรให้บ้าง (V1.1 + M4/M5)

| เมธอด | ทำอะไร |
|---|---|
| `Log(string)` | Debug.Log ขึ้นต้น `[clientmod:ชื่อmod]` |
| `ShowMessage(string)` | popup กลางจอ (`UIManager.SystemMsg`) — ใช้ได้เมื่อเข้าเกมแล้ว |
| `RegisterHotkey(KeyCode, Action)` | ปุ่มลัด raw (`Input.GetKeyDown`) ไม่ชนระบบ input ของเกม แต่ **ไม่รู้บริบท** (ไม่รู้ว่ากำลังพิมพ์แชท) — ใช้ F9-F12 ซึ่งเกมไม่ผูก |
| `OnGameReady(Action)` | ยิงครั้งเดียวเมื่อตัวละครเกิดในโลกจริง (เข้าเกาะ) — การันตี `LocalPlayer != null` |
| `OnUpdate(Action<float>)` *(V1.1)* | ทุกเฟรม dt=Time.deltaTime — ห้ามทำงานหนัก |

`IClientPlayer`: `Name`, `Position` (อ่านอย่างเดียว — null ถ้ายังไม่เข้าเกาะ)

Optional `IClientPresentationApi` (cast จาก API) มี `RegisterSceneHook(scene, handler)`,
`RegisterHud(id, draw)` และ `ValidateAsset(relativePath, sha256)` โดย path ถูกบังคับให้อยู่ใต้โฟลเดอร์
ของม็อดและ hash ไม่ตรงจะคืน `false` โดยไม่โหลดไฟล์ต่อ. `ClientModLoader` อ่าน asset manifest ได้ด้วย
รูปแบบ `relative/path|sha256` หนึ่งรายการต่อบรรทัด (บรรทัดว่าง/ขึ้นต้น `#` ข้าม) และมีเพดาน
256 รายการ. Loader ยังสร้าง manifest/hash
และส่ง `ModHello` หลัง Welcome ก่อน Ready เพื่อให้ server ตรวจ required/optional mods.

ยังคงไม่มี API ให้แก้ stat/position หรือแกะ packet เพื่อข้าม anti-cheat

## 4. ทดสอบแล้ว — `tools/ExampleClientMod`

Build ผ่าน 27 ส.ค. 2026, DLL ติดตั้งที่ `game/mods/ExampleClientMod.dll` แล้ว และเปิด
เกมจริงต่อ server จริงแล้ว สาธิต:
ปุ่ม F9 (FPS เฉลี่ยจาก OnUpdate) · F10 (แจ้งตำแหน่งจาก LocalPlayer) · F11/F12 (ตั้งจุดจำ+
รายงานระยะ — **จงใจไม่ warp** เพื่อไม่สอนฝ่า anti-cheat) · OnGameReady popup ทักทาย

หลักฐานจากรอบ real client (27 ส.ค. 2026) ใน `game/clientmods.log`:

```
[clientmod:ExampleClientMod] PreLoad
[clientmod:ExampleClientMod] โหลดแล้ว — ปุ่มลัด: F10=แจ้งตำแหน่ง, F11=ตั้งจุด warp, F12=ย้อนกลับจุด warp, F9=FPS
[clientmod:ExampleClientMod] PostLoad — พร้อมใช้งานเต็มรูปแบบ
[clientmods] โหลด 'ExampleClientMod' v1.1.0 จาก ExampleClientMod.dll สำเร็จ (ครบ 3 เฟส)
[clientmods] โหลดสำเร็จ 1 mod จาก 1 ไฟล์ .dll ใน '.../game/DurangoV2_Data/../mods'
[clientmod:ExampleClientMod] เข้าเกาะแล้ว ผู้เล่น: multi-3
[clientmod:ExampleClientMod] position=(76.000, 0.000, 120.000)
[clientmod:ExampleClientMod] position=(-100.000, -44.142, 500.000)
```

บรรทัดตำแหน่งสองชุดเกิดก่อน/หลัง server สั่ง `control multi-3 tp 40 177` ตามลำดับ
จึงยืนยันทั้ง `OnGameReady`, hotkey dispatch และการอ่าน state ที่ server sync มาแล้ว

หมายเหตุ: Unity reference ของ net35 ทำให้ `Debug.Log` บาง call ถูกตัดตาม conditional
attribute ตอน compile จึงเพิ่ม trace file แบบ best-effort ใน `client/ClientModLoader.cs`;
ใช้ `game/clientmods.log` เป็นหลักฐานการโหลด/ทดสอบ ส่วน `output_log.txt` ใช้ตรวจ exception
ของ Unity ตามปกติ

ตัวจริงที่รันผ่าน MainScene: `tools/MemoryBotMod` → build/install เป็น `game/mods/DurangoMemoryBot.dll` แล้ว

รอบ bridge ล่าสุดยืนยัน `read player.local`, `player.move_to` และ `player.stop` ผ่าน TCP loopback โดยไม่ใช้เมาส์หรือคีย์บอร์ด (`docs/client/MemoryBot.md`)

## 5. กับดัก

- **net35 เท่านั้น** — compile net47/netstandard แล้ว Mono 2017.4 ของเกมโหลดไม่ขึ้น
- ห้าม reference `DurangoClientModSdk.dll` ซ้ำใน mods\ (ดูข้อ 2)
- `ShowMessage` ก่อนเข้าเกาะ = ไม่ขึ้นอะไร (UIManager ยังไม่พร้อม)
- Hotkey ยิงตอนพิมพ์แชทด้วย — อย่าผูกปุ่มตัวอักษร
- เปลี่ยน SDK interface: **เพิ่ม method ใหม่ได้ ลบ/เปลี่ยน signature เดิมไม่ได้**
  (mod เก่าที่ compile ไว้จะ fail ทันทีถ้า signature เปลี่ยน)
- แก้ SDK แล้วต้อง rebuild Assembly-CSharp ด้วย (`tools/build-client.ps1`) เพราะ
  `client-mod-sdk` เป็น ProjectReference ของตัวเกม — script จะ copy dll ทั้งคู่ให้เอง

## 6. Method override และ render assets

Client runtime รุ่นปัจจุบันรองรับ `IClientMethodOverridesApi` สำหรับ Prefix/Postfix/Replace และ
`IClientAssetOverrideApi` สำหรับ AssetBundle, full player model พร้อม bone remap, material/texture,
prefab/effect และ audio. รายละเอียดและตัวอย่างอยู่ที่ `docs/mod-system/MethodOverrides.md`,
`docs/client/RenderMods.md` และ `tools/ExampleRenderMod/`.
