# ฝั่ง client — แก้อะไรได้บ้าง (GUI) + ปรับอะไรให้ลื่นขึ้น

สำรวจ 19 ส.ค. 2026 · **ยังไม่ได้แก้โค้ดอะไรเลย** — เอกสารนี้เป็นรายการให้ตัดสินใจ

แหล่งข้อมูล: `client/` 3,760 ไฟล์ (ILSpy) · `game/game.log` (รันจริง 19 ส.ค.) ·
`game/game1.log` (13 ส.ค. · exception 58,878 บรรทัด) · `resources.strings.txt`

---

## สรุปหน้าเดียว

**เรื่องใหญ่ที่สุดที่เจอ: เกมมีคำแปลภาษาไทยฉบับทางการของ NEXON ติดมาอยู่แล้วครบทั้งเกม**
(ดู §2.1) — เป็นงาน GUI ที่ผลตอบแทนสูงสุดต่อแรงที่ลง และไม่ต้องแตะ prefab สักอัน

**เรื่องใหญ่ที่สุดของประสิทธิภาพ: เครื่องที่เทสใช้ GeForce 210 (ปี 2009, VRAM 972 MB)**
⇒ คอขวดอยู่ที่ GPU เกือบทั้งหมด การไปไล่แต่งโค้ด C# จะได้ผลน้อยกว่าปรับ 4 สวิตช์ใน §3.2 มาก

**ขอบเขตที่ต้องจำไว้ตลอด:** เราคอมไพล์ C# เองทั้งหมด ⇒ **ตรรกะ UI ทุกบรรทัดแก้ได้**
แต่ **prefab / atlas / ฟอนต์ / ซีน แก้ไม่ได้** เพราะอยู่ใน `resources.assets` ที่เสีย (ENV-01)

---

# ส่วนที่ 1 · ขอบเขต — อะไรแก้ได้ อะไรแก้ไม่ได้

## 1.1 ✅ แก้ได้เต็มที่ (อยู่ในโค้ดที่เรา build เอง)

| สิ่งที่แก้ได้ | ตัวอย่าง |
|---|---|
| ข้อความทุกจุด | ผ่าน `T._()` — เปลี่ยนภาษา/ข้อความได้หมด |
| ตำแหน่ง · ขนาด · สี · การจัดวางของ widget | NGUI ตั้งค่าตอน runtime ได้ (`SetPosition` · `width/height` · `Reposition()`) |
| ซ่อน/แสดงอะไรก็ได้ | `gameObject.SetActive` · `MenuSystem.NotImplementedYet` |
| **โคลนของที่มีอยู่แล้วเป็นปุ่ม/ช่องใหม่** | `NGUITools.AddChild(prefabเดิม)` แล้วเปลี่ยนข้อความ/ไอคอน |
| พฤติกรรมทุกอย่าง | ปุ่มกดแล้วทำอะไร · เงื่อนไขแสดงผล · ลำดับการเปิดหน้าต่าง |
| เพิ่ม component ใหม่ตอน runtime | `gameObject.AddComponent<T>()` |
| **hook กลางที่แตะ UI ได้ทุกอัน** | `UIManager.UIInitFunc(GameObject)` ถูกเรียกกับ prefab UI ทุกตัวตอนเริ่ม |
| **ตัวกรองว่าจะสร้าง UI ไหนบ้าง** | `UIManager.UIFilterFunc(GameObject)` — ตอนนี้กรองแค่ชื่อที่มี Development/Cheat |

## 1.2 ❌ แก้ไม่ได้ (ต้อง repack assets ซึ่งตอนนี้ทำไม่ได้)

- เพิ่ม **ไอคอน/รูปใหม่** ที่ไม่มีใน atlas เดิม
- เพิ่ม **ฟอนต์ใหม่** (สำคัญกับเรื่องภาษาไทย — ดู §2.1)
- สร้าง **prefab ใหม่ทั้งอัน** หรือแก้ผังใน prefab
- shader / วัสดุใหม่

## 1.3 ⚠️ กับดักที่ต้องรู้ (ENV-01)

`game.log` ยืนยันสด ๆ ว่า:

```
The file '…/DurangoV2_Data/resources.assets' is corrupted! [Position out of bounds!]   ← เจอ 2 ครั้ง
The file '…/DurangoV2_Data/level2' is corrupted! [Position out of bounds!]             ← ซีนก็เสียด้วย
```

⇒ **`[SerializeField]` ตัวไหนก็เป็น null ได้** โดยไม่มีรูปแบบตายตัว
เวลาจะแตะ UI ไหน ให้เช็ค null ทีละตัว อย่าผูก `.onClick` ยาวต่อกันรวดเดียว
(รอบก่อนเจอมาแล้ว 3 เคส: `FatigueGaugeScrollSprite` · `ExpectResultWidget` · `UITitleWidget_PC`)

## 1.4 🔑 กุญแจที่ยังไม่ได้ใช้ — `Json.ReadFromFile`

`client/Durango.Utils/Json.cs:88`

```csharp
public static T ReadFromFile<T>(string fileName)
{
    TextAsset textAsset = Resources.Load(fileName) as TextAsset;   // ← จาก resources.assets (ที่เสีย)
    ...
}
```

เมธอดเดียวนี้เป็นทางเข้าของ **ข้อมูล JSON แทบทุกอย่างของเกม** — เมนูตั้งค่า (`config_menu_pc`) ·
แผนที่ท่าทาง (`MotionInfos/*`) · play guide · cheat macro

> **ถ้าเติมให้มันลองอ่านจากโฟลเดอร์ข้างตัวเกมก่อน แล้วค่อย fallback ไป Resources**
> เราจะแก้เมนูตั้งค่า/ตารางข้อมูลได้ด้วยการแก้ไฟล์ .json เฉย ๆ ไม่ต้อง build client ใหม่ทุกครั้ง
> — เป็นการลงทุนครั้งเดียวที่ทำให้งาน GUI ทั้งหมดหลังจากนี้เร็วขึ้นมาก

---

# ส่วนที่ 2 · GUI — รายการที่ควรแก้ (เรียงตามผลที่ผู้เล่นเห็น)

## 🥇 2.1 เกมมีภาษาไทยของแท้อยู่แล้ว — แค่ยังไม่ได้เปิด

**หลักฐาน 3 ชั้น:**

1. `client/LocalizeSystem.cs:73-84` มีรายการภาษา 10 ภาษา และมีไทยอยู่ในนั้น
   ```csharp
   new LocaleItem("th", "th_TH", "ภาษาไทย", lengthy: false, usingSpace: false)
   ```
2. ในดัมป์ข้อมูลเกมมี **catalog ภาษาไทยฉบับสมบูรณ์**
   ```
   PO-Revision-Date: 2019-12-09 14:59+0900
   Last-Translator: May Cho <maycho@nexon.co.kr>
   Language: th_TH
   ```
   ตามด้วยข้อความไทยของจริง เช่น
   *"ไอเทมที่ถูกทิ้งบนเกาะจะค่อย ๆ สูญเสียความทนทานและหายไปในที่สุด"*
   นับบรรทัดที่มีอักษรไทยในดัมป์ได้ **758,846 บรรทัด**
3. ระบบแปลเป็น **GNU gettext** — `T._("캐릭터")` เอาข้อความเกาหลีเป็นกุญแจไปหาคำแปล
   ⇒ **ไม่ต้องแก้โค้ดที่เรียก `T._()` สักบรรทัดเดียว** แค่ติดตั้ง catalog ให้ถูก

**ทำไมตอนนี้ยังเป็นเกาหลี** — `LocalizeSystem.SetLocale()` ติดตั้ง catalog 2 ทาง และ **พังทั้งคู่**:

```csharp
T.InstallCatalog(Locale);                                  // → new Catalog("messages","locales",Culture)
                                                           //   หาโฟลเดอร์ locales/ ข้างตัวเกม — ซึ่งไม่มี
TextAsset ta = Resources.Load($"offline/i18n/{locale}");    // → resources.assets ที่เสีย
```
เมื่อหาคำแปลไม่เจอ gettext จะคืน **msgid ซึ่งคือภาษาเกาหลี** ⇒ ได้เกมเกาหลีอย่างที่เห็น

**ทางที่น่าจะได้ผล** (เรียงจากง่ายไปยาก)
1. สร้าง `locales/th_TH/LC_MESSAGES/messages.mo` จากข้อความในดัมป์ แล้ววางข้างตัวเกม —
   เส้นทางนี้อ่านจาก **ดิสก์จริง** ไม่ผ่าน assets ที่เสีย
2. ถ้าไม่ได้ ให้ patch `T.InstallCatalog` ให้ชี้ path เอง (เราคอมไพล์เองอยู่แล้ว)
3. ตั้ง `locale` เริ่มต้นเป็น `th_TH` (ตอนนี้ `NormalizeLocale` เดาจากภาษาเครื่อง)

**⚠️ ความเสี่ยงที่ยังไม่ได้พิสูจน์ — ฟอนต์**
NGUI ต้องมี glyph ไทยใน atlas ฟอนต์ ถ้า atlas มีแต่เกาหลี+ละติน จะออกมาเป็นสี่เหลี่ยม
เพิ่มฟอนต์ใหม่เข้า atlas **ทำไม่ได้** (§1.2) — **ต้องทดสอบข้อนี้ก่อนลงแรงที่เหลือทั้งหมด**
วิธีเทสเร็วสุด: เปลี่ยน `T._()` ให้คืนข้อความไทยแบบฮาร์ดโค้ดสัก 1 จุด แล้วดูว่าตัวอักษรขึ้นไหม
(NEXON เปิดเซิร์ฟไทยจริง ⇒ มีโอกาสสูงที่ฟอนต์ไทยจะติดมาด้วย แต่ไม่ควรเดา)

## 🥈 2.2 ปุ่มโหมดต่อสู้บน HUD สคริปต์หลุด

`game.log` เจอ **3 ครั้งต่อการเปิดเกม 1 รอบ**:

```
The referenced script on this Behaviour (Game Object 'CombatModeButton') is missing!
```

= prefab ของปุ่มนี้ชี้ไปที่สคริปต์ที่หาไม่เจอในบิลด์นี้ ⇒ ปุ่มมีอยู่แต่ไม่มีพฤติกรรม
ต้องเปิดเกมดูว่าปุ่มยังกดได้ไหม ถ้าไม่ได้ ให้ผูกพฤติกรรมกลับจากโค้ด (`UIInitFunc` เป็นจุดที่เหมาะ)

## 🥉 2.3 หน้าตัวละครโชว์ 0 สามช่องใหญ่

พลังรบ / พลังคราฟต์ / พลังเก็บของ — `Durango.UI/CharacterAbilityWidget.cs:84`
อ่านจาก `Statistics.RepresentPowers` ที่ server ส่ง null (รายละเอียดใน `UI-BUGS.md` §2.1)
**แก้ที่ฝั่ง server 3 บรรทัด ไม่ต้อง build client**

## 2.4 กดปุ่มลัดตอนกำลังโหลด → เมนูไม่เปิด

`InventoryGroup.cs:331` และ `ScreenCaptureGroup.cs:142` deref
`PlayerBehavior.LocalPlayer.Driver.…` โดยไม่เช็ค null (`LocalPlayer` เป็น static field ธรรมดา
= null จนกว่าตัวละครจะเกิด) — เจอใน log 3 และ 4 ครั้ง · รายละเอียดใน `UI-BUGS.md` §1.1-1.2

## 2.5 ขนาด UI ไม่เข้ากับหน้าต่างจริง

`config_menu_pc` ให้เลือก `ui_size` = **1280 / 1420 / 1600** (ค่าเริ่มต้น 1280)
แต่ HANDOFF จดไว้ว่าหน้าต่างเกมมักออกมา **1010×588** ⇒ UI ที่ออกแบบมาสำหรับ 1280 ถูกบีบ
**ควรทดลอง**: บังคับหน้าต่างเป็น 1280×720 ขึ้นไปตั้งแต่เปิด (`ScreenInfo` / `DeviceInfo` แก้ได้)
หรือเพิ่มตัวเลือก `ui_size` ที่เล็กกว่า 1280 ลงไป

## 2.6 แถบเมนูมีช่องว่างจากเมนูที่ซ่อน 24 อัน

ตอนนี้ซ่อนด้วย `MenuSystem.NotImplementedYet` ซึ่งทำงานถูกต้อง
แต่ควรเปิดเกมดูว่าเมนูที่เหลือ (ตัวละคร · สกิล · กระเป๋า · คราฟต์ · ถ่ายภาพ · ตั้งค่า)
เรียงชิดกันสวยไหม หรือมีรูโหว่ — `TitleBarMenuGroup.InitMenuList` จัดตำแหน่งจากโค้ด แก้ได้

## 2.7 ของที่ "เพิ่มได้" ถ้าอยากทำ

| อยากได้ | ทำได้ไหม | ทำยังไง |
|---|---|---|
| เปลี่ยนสี/ธีม UI | ✅ | ตั้ง `UIWidget.color` ผ่าน `UIInitFunc` |
| ปุ่มใหม่บน HUD | ✅ | โคลนปุ่มเดิมด้วย `NGUITools.AddChild` แล้วเปลี่ยนไอคอน (ต้องใช้ไอคอนที่มีอยู่แล้ว) |
| ไอคอนใหม่ที่ไม่มีในเกม | ❌ | ต้อง repack atlas |
| ย้าย/ย่อ minimap | ✅ | `MapIndicators` / `UIRootAnchor` |
| ตัวนับ FPS บนจอ | ✅ | มี `Durango.Development/FrameChecker` อยู่แล้ว (ดู §3.1) |
| หน้าต่างเควส/สิ่งที่ต้องทำ | ✅ | เลิกซ่อน `MenuType.Quest` + `CategoryToDo` (ดูเอกสารเควส) |

---

# ส่วนที่ 3 · ประสิทธิภาพ

## 3.1 ⚠️ ต้องวัดก่อนแก้ — เครื่องมือมีอยู่แล้วในเกม

`client/Durango.Development/` มีชุดวัดครบชุดแต่ถูกปิดไว้:

| ไฟล์ | วัดอะไร |
|---|---|
| `FrameChecker.cs` | **FPS** — และมี `_showFrameRateOnConsole` ที่เขียนลง log ได้โดย**ไม่ต้องพึ่ง prefab** |
| `MemoryInfo.cs` | Mono heap / reserved memory (ผ่าน `UnityEngine.Profiling.Profiler`) |
| `LatencyDisplay.cs` | ping |
| `PacketWatcher.cs` / `PacketWatcherView.cs` | จำนวน/ขนาด packet |
| `ChunkBoundary.cs` · `ChunkIndicator.cs` · `TileLabel.cs` | ขอบเขต chunk ที่กำลังโหลด |

ปิดอยู่ 2 ชั้น: `Debug.isDebugBuild` และ `UIManager.UIFilterFunc` ที่ทิ้ง prefab ชื่อมี "Development"

> **ข้อเสนอข้อแรกของหมวดนี้: เปิด `FrameChecker` แบบเขียนลง log ก่อน**
> เพราะมันไม่ต้องใช้ widget จาก prefab เลย ⇒ ไม่โดน ENV-01
> จะได้รู้ว่าตอนนี้ได้กี่ FPS และตกตอนไหน ก่อนจะไปแก้อะไร

## 3.2 🔴 คอขวดใหญ่ที่สุดคือการ์ดจอ ไม่ใช่โค้ด

`game.log` บอกสเปกเครื่องที่เทส:

```
Renderer: NVIDIA GeForce 210  (ID=0xa65)      ← การ์ดจอปี 2009 ระดับล่างสุด
VRAM:     972 MB
Driver:   21.21.13.4201                       ← ไดรเวอร์ปี 2016
Direct3D 11.0 [level 10.1]                    ← feature level ต่ำกว่ามาตรฐาน
```

บนการ์ดระดับนี้ **ค่าที่ปรับใน `config_menu_pc` ให้ผลมากกว่าการแก้โค้ดหลายเท่า**
ค่าเริ่มต้นตอนนี้กับที่ควรเป็น:

| ตัวเลือก | ค่าเริ่มต้นตอนนี้ | ควรเป็น | เหตุผล |
|---|---|---|---|
| `shadow` | **"high"** (= `ShadowOption.Normal`) | `"low"` (`Simple`) | เงาแบบ Normal แพงที่สุดในรายการนี้ · `PlaneShadowManager.ChangeOption` |
| `vignette` | **true** | false | post-processing เต็มจอ — GPU นี้ fill-rate ต่ำมาก |
| `visual_effect` | **true** | false | `Firefly` — particle หิ่งห้อยทั่วแมพ |
| `anti_aliasing` | "0" | คงไว้ | ✅ ปิดอยู่แล้ว ถูกต้อง |
| `v_sync` | false | คงไว้ | ✅ ถูกต้องสำหรับเครื่องที่วิ่งไม่ถึง 60 |
| `max_frame_rate` | 60 (ช่วง 30-144) | คงไว้ | ✅ PC ใช้ค่านี้ ไม่ได้โดนล็อก 30 |

> ℹ️ `ConfigInstance.ChangeFps` ที่ล็อก **30 fps** เมื่อค่าไม่ใช่ `"quality"` เป็น**เส้นทางของมือถือ**
> PC ใช้ `max_frame_rate` (slider) แทน — ตรวจแล้วไม่ได้โดนล็อก แต่ควรยืนยันในเกมจริงอีกที

**นอกจากนี้ยังมี shader ที่รันไม่ได้บนเครื่องนี้:**
```
WARNING: Shader Unsupported: 'Hidden/BlitToDepth' - Setting to default shader.
WARNING: Shader Unsupported: 'Hidden/BlitToDepth_MSAA' - Setting to default shader.
```
= สาย post-processing ถูก fallback ไป shader มาตรฐาน ⇒ อาจได้ภาพผิดหรือเปลืองกว่าเดิม
**ควรปิด post-processing ทั้งหมดบนเครื่องระดับนี้ไปเลย**

## 3.3 🟠 ของที่โหลดค้างในหน่วยความจำเยอะเกินจำเป็น

`game.log`:
```
Unloading 7141 unused Assets to reduce memory usage. Loaded Objects now:  98265.  Total: 184 ms
Unloading    7 unused Assets to reduce memory usage. Loaded Objects now: 117268.  Total: 232 ms
```

**ค้างอยู่แสนกว่า object** และการเก็บกวาดแต่ละครั้งกิน **180-250 ms** (`MarkObjects` กินไป 170-223 ms)
⇒ เห็นเป็นอาการ **ค้างแวบตอนโหลดเสร็จ**

**สาเหตุที่แก้ได้:** `UIManager.InitUIGroups()` → `LinkedPrefabs.Load()`
**สร้าง UI prefab ทุกอันตอนเปิดเกม** — ในโค้ดมีคลาสลูกของ `UIBase` **107 คลาส** และหน้าต่างหลัก 39 อัน
ทั้งที่ **เราซ่อนเมนูไป 24 อัน** (ตลาด · เพื่อน · เมล · สารานุกรม · แคลน · เพ็ท · สิทธิ์ที่ดิน · ร้านค้า …)

`UIFilterFunc` มีกลไกคัดกรองอยู่แล้วและใช้งานจริง:
```csharp
if (text.Contains("Development") || text.Contains("CommandButton") || …) return false;
```
> **ข้อเสนอ: ต่อยอด `UIFilterFunc` ให้ข้าม prefab ของเมนูที่อยู่ใน `MenuSystem.NotImplementedYet`**
> ได้ 3 อย่างพร้อมกัน — object น้อยลง · RAM ลด · **จำนวน `Update()`/`LateUpdate()` ที่ Unity ต้องเรียกทุกเฟรมลดลง**
> (ทั้งเกมมี `Update` 222 · `LateUpdate` 93 · `OnGUI` 8 เมธอด — ยิ่งสร้าง object น้อยยิ่งถูกเรียกน้อย)

`Resources.UnloadUnusedAssets()` ถูกเรียกที่ `Durango.UI/LoadingCurtainGroup.cs:131`
ทุกครั้งที่ม่านโหลดปิด — จังหวะนี้เหมาะแล้ว แต่ถ้าลด object ตั้งต้นได้ มันจะเร็วขึ้นเอง

## 3.4 🟡 โค้ดต่อเฟรม — สแกนแล้วสะอาดกว่าที่คิด

สแกน 2,521 ไฟล์ หาเมธอดต่อเฟรมที่มีรูปแบบเปลือง (LINQ / GetComponent / จองหน่วยความจำ /
สร้าง string / foreach dictionary / Instantiate / Camera.main) **เจอแค่ 4 เมธอดจาก 325**:

| ไฟล์ | เมธอด | สภาพจริงหลังอ่านโค้ด |
|---|---|---|
| `Durango.UI.Control/HyperGaugeViewer.cs:306` | `Update` | **ตัวที่ควรดูที่สุด** — early-return เมื่อหลอดนิ่ง แต่ตอนหลอดขยับจะไล่คำนวณสี/ตำแหน่งและเขียน widget ทุกเฟรม · เป็นหลอดเลือด/สตามินา ⇒ ทำงานเกือบตลอดเวลา |
| `MirzaBeig.Scripting.Effects/ParticleAffector.cs` | `LateUpdate` | มี `GetComponent` ในลูปต่อเฟรม (โค้ด asset จากภายนอก) |
| `Durango.UI/AlarmGroup.cs:124` | `Update` | มี LINQ แต่ early-return เกือบตลอด — **ไม่ใช่ปัญหาจริง** |
| `Durango.Development/PlayerInfoIndicator.cs` | `OnGUI` | ปิดอยู่ในบิลด์ปกติ — ถ้าจะเปิดเพื่อ debug ต้องรู้ว่า `OnGUI` แพงโดยธรรมชาติ |

**ตัวที่สแกนไม่เจอแต่ควรดู:** `Durango.UI/MapIndicators.cs:539 UpdateIndicators()`
วนทุก indicator **ทุก LateUpdate** และเขียน `transform.localPosition` ให้ทุกตัวที่ไม่ถูกซ่อน
— NGUI ถือว่า transform เปลี่ยน = ต้องสร้าง geometry ของ panel ใหม่
⇒ **ถ้ามี indicator เยอะ (ผู้เล่น + สิ่งปลูกสร้าง + สัตว์) minimap จะ rebuild ทุกเฟรม**
ยังไม่ได้วัดว่ามีกี่ตัวจริง ๆ — **ต้องนับในเกมก่อนตัดสิน**

## 3.5 🟡 การรับ packet ทำงานทั้งหมดใต้ lock เดียว

`client/Durango.Network/Connection.cs:800`

```csharp
lock (_packetQueue)
{
    while (_packetQueue.Count != 0)
        ProcessPacket(_packetQueue.Dequeue());   // ← แกะ packet + เรียก handler + อัปเดต UI ทั้งหมดอยู่ในนี้
}
```

handler ของเกม (สร้างสัตว์ · สร้างสิ่งปลูกสร้าง · อัปเดต UI) รันอยู่ **ข้างใน lock**
⇒ ตอน packet มาเป็นชุดใหญ่ (เข้าแมพใหม่ · เดินข้ามหลาย chunk) เธรดรับข้อมูลถูกบล็อกทั้งช่วง
ไม่ทำให้ FPS ตกโดยตรง แต่เป็นแหล่งของอาการกระตุกตอนโหลดพื้นที่ใหม่

**ข่าวดี:** ฝั่ง client **ไม่มี**บั๊ก "1 packet ต่อ 1 tick" แบบที่ฝั่ง server เคยมี (GP-01) — มันไล่จนหมดคิว

## 3.6 🟢 ที่แก้ไปแล้ว — ตัวใหญ่ที่สุดของเกมนี้

`game1.log` (13 ส.ค.) มี exception 58,878 บรรทัด และ **58,783 บรรทัด (99.8%)** มาจาก 3 ตัวนี้
ที่โยน NullReferenceException **ทุกเฟรม**:

| ตัวการ | ครั้ง |
|---|---:|
| `MapIndicators.UpdateIndicators` | 19,595 |
| `MoveTrail.Update` | 19,594 |
| `FatigueGaugeScrollSprite.Update` | 19,594 |

Unity เขียน stack trace ลงดิสก์ทุกครั้ง ⇒ log บวม 12 MB และเกมอืดจนโหลดแมพไม่จบ
**ทั้งสามตัว patch ไปแล้ว** — `game.log` วันที่ 19 ส.ค. มี exception **0**

⇒ ก่อนจะไปหาอะไรเพิ่ม ควรเล่นจริงสัก 15 นาทีแล้วดู `game.log` ว่า exception ยังเป็น 0 อยู่ไหม
**exception ต่อเฟรม 1 ตัวเดียวกินแรงมากกว่าทุกข้อใน §3.4 รวมกัน**

## 3.7 ของเล็ก ๆ ที่เจอระหว่างทาง

- `WwiseUnity: Bank title_preload failed to load (AK_Fail)` — bank เสียงหายไป 1 อัน (เกิดทุกรอบ)
- `Recursive Serialization is not supported…` — ตอนโหลดซีน มาจากไฟล์ที่เสีย
- `PlayerBehavior.cs:648` `Driver = GetComponent<Driver>()` **ไม่มี fallback**
  ต่างจากบรรทัดบนที่ `WorldLineRenderer` มี `AddComponent` สำรองให้ — เกี่ยวกับบั๊ก §2.4
- ไม่มี timeout ของ reply handler ใน `Connection` ⇒ packet ที่ server ไม่ตอบจะทิ้ง handler ค้างไว้ตลอด session
  (รายละเอียดใน `UI-BUGS.md` §0.3)

---

# ส่วนที่ 4 · ลำดับที่แนะนำ

## รอบที่ 1 — วัดและเก็บของถูก (ไม่ต้อง build client)

| # | ทำอะไร | ที่ | ได้อะไร |
|---|---|---|---|
| 1 | ตั้ง shadow=low · vignette=off · visual_effect=off ในหน้าตั้งค่าของเกม | ในเกม | น่าจะเห็นผลมากที่สุดต่อแรงที่ลง |
| 2 | เติม `RepresentPowers` 3 ค่า | server | หน้าตัวละครเลิกโชว์ 0 |
| 3 | เล่น 15 นาทีแล้วนับ exception ใน `game.log` | — | ยืนยันว่า §3.6 หายจริง |

## รอบที่ 2 — build client 1 ครั้ง เก็บหลายอย่างพร้อมกัน

| # | ทำอะไร | แรง |
|---|---|---|
| 4 | เปิด `FrameChecker` แบบเขียน FPS ลง log | ต่ำ |
| 5 | null guard `InventoryGroup` + `ScreenCaptureGroup` + `UIBase.Close` | ต่ำ |
| 6 | `UIFilterFunc` ข้าม prefab ของเมนูที่ซ่อนไว้ 24 อัน | ต่ำ-กลาง |
| 7 | `Json.ReadFromFile` อ่านจากดิสก์ก่อน (§1.4) | ต่ำ · **คุ้มระยะยาวที่สุด** |

## รอบที่ 3 — ภาษาไทย (งานใหญ่ ทำแยก)

| # | ทำอะไร | หมายเหตุ |
|---|---|---|
| 8 | **เทสฟอนต์ก่อน** — ฮาร์ดโค้ดข้อความไทย 1 จุดแล้วดูว่าขึ้นเป็นตัวอักษรหรือสี่เหลี่ยม | **ถ้าไม่ผ่าน ข้อ 9-10 ทำไม่ได้เลย** |
| 9 | ถอด catalog ไทยจากดัมป์ทำเป็น `messages.mo` | |
| 10 | วางที่ `locales/th_TH/LC_MESSAGES/` + ตั้ง locale เริ่มต้น | ได้ทั้งเกมเป็นไทย |

## ที่ยังไม่ควรแตะจนกว่าจะมีตัวเลข

- `HyperGaugeViewer.Update` · `MapIndicators.UpdateIndicators` (§3.4) — **ต้องเห็น profiler ก่อน**
  ทั้งคู่อาจไม่ใช่ปัญหาจริงบนเครื่องที่ GPU เป็นคอขวด
- `Connection` lock (§3.5) — แก้แล้วเสี่ยงเรื่อง thread safety มากกว่าที่ได้คืน
