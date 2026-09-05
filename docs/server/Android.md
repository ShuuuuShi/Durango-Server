# Android — "ระบบเหมือน PC" ทำจากฝั่งเซิร์ฟ

อัปเดต: 4 ก.ย. 2026 · โค้ด: `ServerCore/ClientPlatform.cs`, `ServerConfig.AndroidConfig`, `ClientModPolicy.RequiredAndroidBuild`

## ทำไมต้องทำฝั่งเซิร์ฟ

- โค้ดเกม Android กับ PC เป็นชุดเดียวกัน (Assembly-CSharp 3,637 คลาส ต่างกันแค่ `STTController`) แต่ APK คอมไพล์เป็น
  **IL2CPP (native)** ⇒ วาง `Assembly-CSharp.dll` ที่แพตช์ของ PC ลงไปไม่ได้ · ระบบ mod/Harmony ของ PC ใช้ไม่ได้
- APK ชุดเรา (`tools/AndroidApk`) แพตช์เฉพาะ **string literal** ใน `global-metadata.dat` — ไม่มีโค้ดเราสักบรรทัด
- เจ้าของเลือก (3 ก.ย. 2026) แนวทาง **A: แก้ฝั่งเซิร์ฟ** ก่อน (ทางเลือกอื่น: แพตช์ไบนารี `libil2cpp.so` ทีละจุด ·
  native hook loader (NDK+Dobby) · build APK ใหม่ด้วย Unity 2017 Mono จาก `client/` — ทั้งหมดยังไม่ทำ)

⇒ ทุกอย่างในหน้านี้เป็นสิ่งที่ **เกม 5.2.1 ของแท้แสดงผลได้เองอยู่แล้ว**: popup `Info`, แชทช่อง `System`,
ค่า `cluster_mode` จาก `/entry`, `compatible`/`download_url` จาก `/knock`

## เซิร์ฟรู้ได้ยังไงว่าเป็นมือถือ (ไม่ต้องแพตช์ client)

| ข้อมูล | มาจาก | เก็บที่ |
|---|---|---|
| `platform` ("Android"/"WindowsPlayer") | `POST /sessions` ฟิลด์ `platform` (เกมส่งเองจาก `Platform.BuildSessionForm`) และ `GET /entry?platform=` | `PlayerData.Platform` → `ServerPlayer.Platform` |
| `os_version` | `POST /sessions` | `ServerPlayer.OsVersion` |
| `ClientVersion` / `DeviceModel` | packet `Auth` | `ServerPlayer.ClientVersion` (มือถือแท้ = "5.2.1" · PC ชุดเรา = "CustomClient 0.1.x") |
| `build` (APK ชุดเรา) | query `build=android-0.1.4` ใน `/knock` และ `/entry` — APK 0.1.4 แปะให้ (ดูล่าง) | `ServerPlayer.ClientBuild` |

log ตอนเข้าโลก: `[world] player joined: … client=Android/5.2.1/android-0.1.4`

## ฟีเจอร์ (ServerConfig `Android` — hot-reload)

```json
"Android": {
  "ClusterMode": "Online",
  "WelcomeInfo": true,
  "OnlineCountInChat": true
},
"StyledBroadcastMinClientVersion": "0.1.4"
```

### 0. ⚠️ เกมของแท้ **ไม่แสดง packet `Info` บนจอเลย** (4 ก.ย. 2026)
`GameManager.DefaultInfoHandler` ต้นฉบับดูแค่ `##goto` — popup จาก `Info` เป็นของแพตช์ PC ชุดเรา
⇒ ทุกอย่างที่อยากให้มือถือ "เห็น" ต้องไปทางช่องแชทระบบ: `ServerPlayer.SendNotice` = `SayInExclusiveChannel{System, Body=RadioNotice}`
(ต้นฉบับ `SocialSystem.OnSay` เรียก `UIManager.SystemMsg` ให้เอง = popup กลางจอ + บรรทัดในแท็บ "ระบบ") · `SendSystemChat` = `RadioTalk` บรรทัดแชทอย่างเดียว
เทสจริงบน MuMu: popup ขึ้นถูกต้อง (ภาษาไทยต้องส่ง form เป็น UTF-8 — curl ใน Git Bash ส่ง cp874 จะเพี้ยน หน้า admin ในเบราว์เซอร์ปกติ)

### 1. บรอดแคสต์แอดมิน (`POST /admin/broadcast` · `announce-restart.sh`)
- ข้อความแบบกำหนดเวลา/ขนาด/สี ถูกเข้ารหัสเป็น `##bc|d=|z=|c=|ข้อความ` — client ที่รู้จักมีแค่ **CustomClient ≥ 0.1.4**
- `ServerWorld.BroadcastInfo`: มือถือ → `SendNotice(ข้อความล้วน)` · PC ≥ 0.1.4 → `Info` เต็ม `##bc|…` · PC เก่ากว่า → `Info` ข้อความล้วน
  (`ClientPlatform.PlainBroadcastText`) — ก่อนหน้านี้ทุกคนเห็นรหัสดิบบนจอ และมือถือไม่เห็นอะไรเลย

### 2. จำนวนคนออนไลน์
- PC ชุดเราโชว์บนแท็บแชทจาก `/knock online_players` (โค้ด client) — มือถือไม่มี
- เซิร์ฟทำแทน **เฉพาะคนที่เป็นมือถือ** (`ServerPlayer.WantsServerSideOnlineCount`):
  - เข้าโลก → popup `RadioNotice` "ยินดีต้อนรับสู่ <ชื่อเซิร์ฟ> · ออนไลน์ตอนนี้ N คน" (`WelcomeInfo`)
  - มีคนเข้า/ออก → บรรทัดแชทช่อง System "X เข้าเกม · ออนไลน์ N คน" (`OnlineCountInChat`) ผ่าน
    `ServerPlayer.SendSystemChat` = `SayInExclusiveChannel{ChannelType=System, Speaker="ระบบ"}` (เกมต้นฉบับรับได้ที่ `SocialSystem.OnSay`)

### 3. เมนูครบในโหมด Online (กลับหน้าไตเติ้ล ฯลฯ) — **ปิดไว้: default = Online** (เจ้าของสั่ง 4 ก.ย. ไม่ใช้ SingleMode/Offline
เพราะโหมดพวกนั้นอ่านข้อมูลเกมจากไฟล์ในเครื่องแทน `/assets` ของเซิร์ฟ ⇒ สูตรคราฟต์ไม่ตรงกัน) · ตั้ง `"SingleMode"` เฉพาะถ้ายอมแลก
- เมนูของเกมต้นฉบับถูกกรองด้วย `ClusterMode` (`MenuSystem.IsHiddenMenu`):
  `Online` ⇒ ซ่อน `MoveToTitle`/`Connect`/`WarpShop`/`CharacterOnMenu`/… (`HiddenInOnline` — static array แก้จากเซิร์ฟไม่ได้)
  `SingleMode` ⇒ โชว์ตาม `ShowInSingleMode` ซึ่งมี Craft/Skill/Quest/Estate/Encyclopedia/WorldMap/**MoveToTitle** ครบ
- PC ชุดเราแก้รายการนี้ในโค้ด client แล้ว มือถือแก้ไม่ได้ ⇒ `/knock` และ `/entry` ตอบ `cluster_mode` ตาม
  `Android.ClusterMode` **เฉพาะ platform=Android** (PC ยังได้ค่าจาก `--cluster-mode` เหมือนเดิม)
- ผลข้างเคียงของ SingleMode บน client (ดู `extracted/.../Assembly-CSharp` ที่เช็ค `ClusterMode == Mode.Online`):
  แชทวิ่งบน connection เกมแทนพอร์ต radiotower (ช่องรวมใช้ได้ · แชทส่วนตัวไม่มี) · ปุ่ม tag ในช่องแชทซ่อน ·
  ไม่มีแท็บ "สร้างห้อง" · `CustomerServiceSystem` ไม่ยิง `/cs/answer` — ตั้ง `""` เพื่อกลับไปใช้ค่าเดียวกับเซิร์ฟ

### 4. เวอร์ชัน client มือถือเป็นของเรา (`ClientModPolicy` — `data/mods/config/DurangoClientCore.json`)
- เกมของแท้รายงาน `version=5.2.1` เสมอ (TextAsset `client_version` ใน APK) ⇒ แยก APK ชุดไหนไม่ได้
- APK **0.1.4** แพตช์ literal `"&platform="` → `"&build=android-0.1.4&platform="` (ใช้ร่วมกันใน `/knock` และ `/entry`) ⇒
  `GET /knock?version=5.2.1&build=android-0.1.4&platform=Android&bundle_id=…`
- `RequiredAndroidBuild` (ว่าง = ไม่บังคับ) เทียบ MAJOR.MINOR เหมือน PC · ไม่ผ่าน ⇒ `compatible=false` + `download_url` =
  `AndroidDownloadUrl` (client พาไปโหลดเองที่ `TitleMenuGroup.RedirectToDownloadUrl`)
- ⚠️ ตั้ง `RequiredAndroidBuild` แล้ว APK 0.1.3 (ไม่มี build=) เข้าไม่ได้ — แจก APK ใหม่ก่อนเสมอ

## สร้าง APK 0.1.4

```bash
cd "Durango Android/apk-work"
python "<repo>/tools/AndroidApk/patch_metadata_strings.py" global-metadata.orig.dat global-metadata.vps-0.1.4.dat \
  --set "http://127.0.0.1:=http://187.53.129.69:" \
  --set "http://assetbundles.k.nexon.com/{0}/{1}/Info.5.2.1.json=http://187.53.129.69:8190/{0}/{1}/Info.5.2.1.json" \
  --set "http://durango-assetbundles.akamaized.net/{0}/{1}/=http://187.53.129.69:8190/{0}/{1}/" \
  --set2 "&platform=" "&build=android-0.1.4&platform="
python "<repo>/tools/AndroidApk/build_apk.py" ../apk/durango-wild-lands-5.2.1-1912162014.apk global-metadata.vps-0.1.4.dat DurangoTH-Android-0.1.4.apk
```
(`--set2 old new` เพิ่มมาเพราะ literal มี `=` ในตัว · ตัวเทส local ใช้ host `192.168.1.34` → `DurangoTH-local-192.168.1.34-0.1.4.apk`)

## เซิร์ฟทดสอบแยกพอร์ต (VPS 8290 · `durango-test.service`)
- มือถือประกอบ URL จาก literal `http://127.0.0.1:` + **8190 เป็น int** ⇒ ใช้พอร์ตอื่นตรง ๆ ไม่ได้ · ทางออก: literal → `http://ip:8290/p`
  แล้วเกมต่อท้าย "8190" เอง = `/p8190/knock` … เซิร์ฟตัด prefix ด้วย `--url-prefix /p8190` (`WebServer.PathPrefix`) · PC ไม่มี prefix ก็เข้าได้
- APK: `DurangoTH-Android-test-0.1.4.apk` (VPS 8290) · `DurangoTH-local-test-8290.apk` (เครื่องนี้) · โลก/เซฟแยกจากเซิร์ฟหลัก (`/opt/durango/test/`)
- เทส 4 ก.ย.: มือถือจริง 9 คนเข้าเล่นบน 8290 ได้ (ท็อป, Miki0001, SunCeTH, Hamtaro, …)
- test-client: `DurangoTestClient --android-check <host> <port เกม> <port gateway> <admin token>` (AndroidParityCheck.cs) — ผ่านทุกข้อกับ local 8190

## เทส (MuMu Player 12 · instance 2 · adb 127.0.0.1:16448)
- ⚠️ MuMu instance ดับเองบ่อยตอนโลก 3D โหลด (VM หยุดทั้งตัว ไม่ใช่เกม/เซิร์ฟ) — เปิดใหม่ด้วย `MuMuManager control -v 2 launch`
- 4 ก.ย. 2026: มือถือ (ตัวละคร "55") + PC ("test") อยู่โลกเดียวกันบน local 8190 · Online · popup ประกาศ/ต้อนรับขึ้นจริง (`shots` ใน scratchpad l14.png)

- เซิร์ฟ local: `DurangoServer.exe --gateway-port 8190 --game-port 8191 --radiotower --cluster-mode Online --admin-token … --assetbundles-android "…/AssetBundles-android"`
- `/knock?platform=Android` → `cluster_mode: Online` (ตาม `Android.ClusterMode` · ว่าง = ตามเซิร์ฟ) ✅
- มือถือ knock ด้วย `build=android-0.1.4` ✅ · `/entry` มี build ✅ · เข้าโลก log `client=Android/5.2.1/android-0.1.4` ✅
- กับดัก: กดเข้าเกมซ้ำใน process เดิม ⇒ เซิร์ฟฝังในเกม bind พอร์ตซ้ำ (`SocketException: Address already in use` ที่
  `TitleMenuGroup.OnConfirm`) ⇒ ขึ้น "การเรียกข้อมูลล้มเหลว (CheckSoundManager)" — ต้อง `am force-stop` แล้วเปิดใหม่
  (พฤติกรรมเดิมของเกม ไม่เกี่ยวกับแพตช์)
- 🐛 CheckSoundManager อีกสาเหตุ (ตัวจริงที่เจอ 4 ก.ย.): เกมขอ `soundbanks$android$ko_kr$voice_event.bnk` ตามค่า "เสียงพากย์"
  ของเครื่อง แต่ชุด bundle มีแค่ `en_us` → 404 → เข้าเกมไม่ได้เลย ⇒ `Gateway.ResolveVoiceBankFallback` เสิร์ฟ bank en_us แทนทุกภาษา

## 🐛 บั๊กเรนเดอร์: พื้นเป็นสีชมพูบานเย็น / โซนทึบดำเป็นเส้นทแยง (4 ก.ย. 2026)

**อาการ** บนมือถือ พื้นบางโซนกลายเป็นสีชมพูบานเย็น (magenta) และบางโซนทึบดำ ขอบเขตเป็นเส้นตรงคมตามพิกัดโลก
(ไม่ใช่พิกัดจอ) หายไปเมื่อวาร์ปเข้าแผ่นดินใน ไม่เกี่ยวกับสภาพอากาศ ระยะส่ง chunk หรือเน็ตเวิร์ก และ logcat ฝั่ง client ไม่มี error

**ต้นเหตุ** ชุด bundle 2,152 ไฟล์แบ่งเป็นของแท้จากเครื่องผู้เล่นจริง 1,056 ไฟล์ (ไทม์สแตมป์ เม.ย. 2020) กับที่เรา build เอง
จากผลของ AssetRipper อีก 1,059 ไฟล์ — **AssetRipper กู้โค้ด shader จริงไม่ได้ ได้แค่ dummy** ตัว Unity จึงคอมไพล์
shader เปล่าฝังลงไป เทียบกันตรง ๆ:

| shader `LitSphere/Diffuse` | โค้ดที่คอมไพล์แล้ว (decompressed) | tags |
|---|---|---|
| bundle ของแท้ (preload) | 608,644 + 784,972 ไบต์ | QUEUE=Geometry+1, RenderType=Opaque |
| bundle ที่ build เอง | 76 + 1,452 ไบต์ | RenderType=Opaque (ไม่มี QUEUE) |

วัสดุที่ใช้ shader เปล่าจะเรนเดอร์เป็นสีชมพู (Unity ใช้สีนี้เมื่อ shader ใช้การไม่ได้) หรือทึบดำเมื่อ blend/queue ผิด
ขอบเป็นเส้นตรงเพราะมันคือขอบของ mesh พื้นแต่ละชิ้น (เช่น `models$landmark$scoop$…$scoop_ground_grass_01.mat`)

หมวดที่กระทบ: `models` (ของแท้ 808 / build เอง 1,051) และ `particle` เล็กน้อย —
ส่วน `sprite` (60), `water` (3), `ui` (65), `soundbanks` (38) เป็นของแท้ทั้งหมด จึงไม่พัง

**วิธีแก้** คัดลอกข้อมูล shader ตัวจริงจาก bundle ของแท้ทับลงตัวปลอมใน bundle ที่ build เอง โดย**คง PathID เดิม**
วัสดุจึงยังชี้ถูกตัว ไม่ต้องยุ่งกับ external reference ข้ามไฟล์ · shader ปลอมมี 33 ชื่อ หาตัวจริงมาแทนได้ครบทั้ง 33
(รวม 1,565 จุดใน 1,059 ไฟล์) ตัวที่ใช้เยอะสุดคือ `LitSphere/ThreeColor/Diffuse` (497), `LitSphere/Diffuse` (265),
`LitSphere/Skin` (210)

- สคริปต์ซ่อม: `scratchpad/fix_shaders.py --all` (สำรองไฟล์เดิมไว้ที่ `AssetBundles-android-backup-preshaderfix`)
- อัปเดต index: `scratchpad/reindex.py` — แก้ `Size` ตามไฟล์จริงและตั้ง `Hash` ใหม่เพื่อบังคับให้เครื่องที่แคช
  ไฟล์เสียไว้แล้วโหลดใหม่ (**ห้ามแตะ `Crc`** เพราะ client เอาไปประกอบเป็นชื่อไฟล์/URL)
- อัปขึ้น VPS: `tools/upload-fixed-bundles.sh` — bundle เป็นไฟล์สแตติก **ไม่ต้องรีสตาร์ตเซิร์ฟ ผู้เล่นไม่หลุด**

## 🐛 ตัวละครมือถือไม่ถูกเซฟ (ยังไม่แก้ · 4 ก.ย. 2026)

ฝั่งเซิร์ฟถูกต้องทุกอย่าง — `POST /accounts` จาก IP เดียวกันคืนตัวละคร 3 ตัวครบ และไฟล์เซฟมีอยู่จริง
แต่ **client มือถือไม่เคยถาม** เพราะ `Platform.ClusterListUrl` คืนค่าว่างบน Android (Platform_Android override แค่
`RequestPermission`) ⇒ เข้าโหมด `Clusters.Offline` ซึ่งโชว์เฉพาะ cluster ท้องถิ่น "free" (Creative Island)
และหน้าเลือกตัวละครของ cluster นั้นอ่านจากไฟล์ `.player` ในเครื่อง — ซึ่งโฟลเดอร์
`/sdcard/Android/data/com.nexon.durango.global/files/offline/free/` **ว่างเปล่า** ทุกครั้งจึงบังคับสร้างตัวใหม่

ทางแก้ที่เป็นไปได้: URL รายการ cluster อยู่ใน `GameManager._clusterListUrlFormat` ซึ่งเป็น `[SerializeField]`
⇒ ค่าจริงฝังอยู่ใน **ข้อมูล scene ของ APK ไม่ใช่ string literal ในโค้ด** จึงแพตช์ด้วย
`tools/AndroidApk/patch_metadata_strings.py` ไม่ได้ ต้องแก้ที่ไฟล์ scene แทน (ยังไม่ได้ทำ — รอเจ้าของตัดสินใจ)
