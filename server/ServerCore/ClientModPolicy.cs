using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

/// <summary>Loads data/mods/config/DurangoClientCore.json independently from gameplay config.</summary>
public sealed class ClientModPolicy
{
    public bool Enabled { get; set; } = true;
    public string RequiredVersion { get; set; } = "1.0.0";
    public bool SkipRegionSelection { get; set; } = true;
    public int WorldChunkRange { get; set; } = 4;

    /// <summary>
    /// ระยะ chunk ที่เซิร์ฟ "ส่งจริง" (= World.ChunkSendRange) — ตั้งครั้งเดียวตอนเซิร์ฟเริ่ม
    ///
    /// 🐛 [31 ส.ค. 2026] ต้นตออาการ **ขอบแมพเป็นสีเทาตัดตรง ๆ** และ **ไดโนยืนแข็งจนกว่าจะตี**
    ///    ตั้งแต่ย้ายมาเป็นแพตช์ ตัวเกมเอา `world_chunk_range` ไปสร้าง ChunkPool ตรง ๆ
    ///    (client Durango.Terrain/TerrainBase.InitChunkPool) ⇒ ค่านี้ = ระยะที่ "วาด"
    ///    แต่ `World.ChunkSendRange` = ระยะที่ "ส่ง" ซึ่งเป็นคนละค่ากันและตั้งแยกกัน
    ///    ตอนนั้น วาด 4 (72 tile) แต่ส่ง 2 (40 tile) ⇒ วงนอกไม่มีข้อมูลเลย
    ///
    /// ⇒ บีบให้ค่าที่บอก client ไม่เกินที่ส่งจริง สองค่านี้จะขัดกันอีกไม่ได้
    /// </summary>
    public static int ServerChunkSendRange { get; set; } = 4;
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> ManagedMenus { get; set; } = new() { "Skill", "Craft", "Quest", "Estate" };
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> EnabledMenus { get; set; } = new() { "Skill", "Craft", "Quest", "Estate" };

    /// <summary>
    /// เมนูที่จะ "ซ่อน" จากแถบเมนูในเกม — ใส่ชื่อ MenuType เช่น "Market", "Clan", "Encyclopedia"
    ///
    /// [เพิ่มเอง] 31 ส.ค. 2026 — เจ้าของสั่ง "เปิด/ปิดเมนูได้จากฝั่งเซิร์ฟโดยไม่ต้องแก้ตัวเกม"
    /// เดิมรายการนี้ฮาร์ดโค้ดอยู่ใน client (`MenuSystem.NotImplementedYet`) ⇒ จะเปิด/ปิดทีต้อง
    /// build client ใหม่ + ให้ผู้เล่นทุกคนโหลดใหม่ 828 MB
    /// ตอนนี้แก้ไฟล์นี้แล้ว restart เซิร์ฟพอ ผู้เล่นเห็นผลทันทีตอนล็อกอินครั้งถัดไป
    ///
    /// ว่างเปล่า = ไม่ซ่อนอะไรเลย (ค่าเริ่มต้นปัจจุบัน)
    /// ⚠️ ตัวกรองของเกมเอง (HiddenInOnline/ShowInOffline ตาม ClusterMode) ยังทำงานทับอีกชั้น
    ///    เอาชื่อออกจากที่นี่ไม่ได้แปลว่าเมนูจะโผล่เสมอ
    /// </summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> HiddenMenus { get; set; } = new();

    /// <summary>
    /// เวอร์ชันชุดแจกที่ยอมให้เข้าเซิร์ฟ — ว่าง = **ไม่บังคับ ใครก็เข้าได้** (ค่าเริ่มต้น ปลอดภัยไว้ก่อน)
    ///
    /// [เพิ่มเอง] 31 ส.ค. 2026 — ตัวเกมมีระบบบังคับอัปเดตในตัวอยู่แล้ว
    /// (client TitleMenuGroup.cs:926 — ถ้า /knock ตอบ compatible=false จะพาไป download_url ทันที)
    /// เดิมเราตอบ compatible=true เสมอ ระบบนี้จึงไม่เคยถูกใช้เลย
    ///
    /// ⚠️ **ตั้งค่านี้ผิด = ผู้เล่นเข้าไม่ได้ทั้งเซิร์ฟ** ต้องตรงกับ
    ///    `CurrentBundleVersion.CustomVersion` ในตัวเกมที่แจกอยู่ **เป๊ะ ๆ**
    ///    ขั้นตอนที่ถูกต้อง: ปล่อย release ใหม่ก่อน → ค่อยตั้งค่านี้ ไม่ใช่สลับกัน
    /// </summary>
    public string RequiredVersionOfClient { get; set; } = "";

    /// <summary>
    /// [Android] ยอมให้เกมมือถือของแท้ (ส่ง version "5.2.1" platform=Android ไม่มี mod/ตัวอัปเดต) เข้าได้
    /// ทั้งที่ RequiredVersionOfClient ตั้งไว้เป็น CustomClient — มีผลเฉพาะเมื่อเซิร์ฟรันด้วย --assetbundles-android
    /// </summary>
    public bool AllowRawAndroidClient { get; set; } = true;

    /// <summary>
    /// [3 ก.ย. 2026] เวอร์ชัน APK ชุดเราที่ยอมให้เข้า — เทียบ MAJOR.MINOR เหมือน PC · ว่าง = ไม่บังคับ
    ///
    /// เกมมือถือของแท้รายงานเวอร์ชันเอนจิน "5.2.1" เสมอ (อ่านจาก TextAsset client_version ใน APK)
    /// เลยแยกไม่ออกว่าเป็น APK ชุดไหน ⇒ APK ชุดเราแปะ `build=android-0.1.x` เพิ่มใน query ของ
    /// /knock และ /entry (แพตช์ literal "&amp;platform=" ใน global-metadata.dat — tools/AndroidApk)
    /// ⚠️ ตั้งค่านี้แล้ว APK ที่ไม่มี build= (มือถือของแท้ล้วน / APK รุ่นก่อน 0.1.4) จะเข้าไม่ได้
    ///    ขั้นตอนที่ถูกต้อง: แจก APK ใหม่ก่อน → ค่อยตั้งค่านี้
    /// </summary>
    public string RequiredAndroidBuild { get; set; } = "";

    /// <summary>ลิงก์โหลด APK ให้มือถือเมื่อเวอร์ชันไม่ตรง (client พาไปเองผ่าน download_url ของ /knock)</summary>
    public string AndroidDownloadUrl { get; set; } = "https://github.com/ShuuuuShi/Durango-TH-Client/releases/tag/DurangoTH";

    /// <summary>APK build ที่มือถือส่งมา (query build=) ผ่านนโยบาย RequiredAndroidBuild ไหม</summary>
    public bool IsAndroidBuildAllowed(string build)
    {
        return ClientPlatform.IsBuildAllowed(RequiredAndroidBuild, build);
    }

    /// <summary>ที่อยู่ให้ผู้เล่นไปโหลดชุดใหม่ เมื่อเวอร์ชันไม่ตรง</summary>
    public string DownloadUrl { get; set; } = "https://github.com/ShuuuuShi/Durango-TH-Client/releases/tag/DurangoTH";

    /// <summary>
    /// เวอร์ชันที่ client ส่งมา ผ่านด่านไหม
    /// ว่าง = ไม่บังคับ · ตรงกัน = ผ่าน · ไม่ตรง = ให้ไปโหลดใหม่
    /// </summary>
    /// <summary>
    /// [3 ก.ย. 2026] นโยบายเวอร์ชัน (เจ้าของกำหนด): เทียบแค่ **MAJOR.MINOR (2 ตัวหน้า)** เท่านั้น
    ///   · MAJOR (ตัวหน้าสุด) = รุ่นเปิดจริง
    ///   · MINOR (ตัวกลาง)   = server version — ขยับเมื่อไร "บังคับอัปเดต" (MAJOR.MINOR ไม่ตรง = เตะ)
    ///   · PATCH (ตัวท้าย)   = hotfix เฉพาะ client — ไม่เช็คตอนต่อ (ออกเกมเข้าใหม่ค่อยได้ของใหม่)
    /// เช่น required "CustomClient 0.1.3" ⇒ client 0.1.x ผ่านหมด (x ต่างได้) · 0.2.x หรือ 1.x.x = เตะ
    /// </summary>
    public bool IsClientVersionAllowed(string clientVersion)
    {
        if (string.IsNullOrWhiteSpace(RequiredVersionOfClient))
        {
            return true;
        }
        string v = (clientVersion ?? "").Trim();
        // เทียบเฉพาะ 2 ตัวหน้าของเวอร์ชัน custom — ตัวท้าย (hotfix) ต่างกันได้
        if (TryMajorMinor(RequiredVersionOfClient, out int rMaj, out int rMin)
            && TryMajorMinor(v, out int cMaj, out int cMin)
            && rMaj == cMaj && rMin == cMin)
        {
            return true;
        }
        // 🐛 ช่วงเปลี่ยนผ่าน: client รุ่นเก่ายิง /knock ด้วยเวอร์ชันเอนจินล้วน "5.2.1" (hardcode เดิมใน
        //    Server.cs ไม่เคยส่ง CustomVersion) ⇒ เทียบเลขไม่ได้ (5.2 ไม่ใช่ 0.1) จะโดนเตะทั้งที่ client
        //    ถูกเวอร์ชัน · ยอมให้เวอร์ชันที่ไม่มีคำว่า "CustomClient" ผ่านไปก่อน (client รุ่นถัดไปแก้ให้ส่ง
        //    เวอร์ชันจริงแล้ว — เมื่อผู้เล่นอัปครบค่อยเอา escape นี้ออกเพื่อบังคับเวอร์ชันได้เต็มที่)
        if (v.IndexOf("CustomClient", StringComparison.OrdinalIgnoreCase) < 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>ดึง MAJOR.MINOR (เลข 2 ตัวแรก) จากสตริงเวอร์ชัน เช่น "CustomClient 0.1.3" → (0,1)</summary>
    private static bool TryMajorMinor(string s, out int major, out int minor)
    {
        major = -1;
        minor = -1;
        if (string.IsNullOrWhiteSpace(s)) return false;
        System.Text.RegularExpressions.Match m =
            System.Text.RegularExpressions.Regex.Match(s, @"(\d+)\.(\d+)");
        if (!m.Success) return false;
        return int.TryParse(m.Groups[1].Value, out major) && int.TryParse(m.Groups[2].Value, out minor);
    }

    private static readonly object Sync = new();
    private static ClientModPolicy _current = new();
    private static DateTime _lastWrite;

    public static ClientModPolicy Current
    {
        get
        {
            lock (Sync)
            {
                string dataDir = Path.GetDirectoryName(ServerConfig.ConfigPath ?? "data/config.json") ?? "data";
                string path = Path.Combine(dataDir, "mods", "config", "DurangoClientCore.json");
                try
                {
                    if (!File.Exists(path))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                        File.WriteAllText(path, JsonConvert.SerializeObject(_current, Formatting.Indented));
                    }
                    DateTime write = File.GetLastWriteTimeUtc(path);
                    if (write != _lastWrite)
                    {
                        ClientModPolicy loaded = JsonConvert.DeserializeObject<ClientModPolicy>(File.ReadAllText(path)) ?? new();
                        loaded.ManagedMenus ??= new();
                        loaded.EnabledMenus ??= new();
                        _current = loaded;
                        _lastWrite = write;
                        Console.WriteLine("[client-mod] loaded " + path);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("[client-mod] invalid DurangoClientCore.json; keeping last valid policy: " + e.Message);
                }
                return _current;
            }
        }
    }

    public JObject ToJson()
    {
        return new JObject
        {
            ["enabled"] = Enabled,
            ["required_version"] = RequiredVersion ?? "",
            ["managed_menus"] = new JArray(ManagedMenus ?? new()),
            ["enabled_menus"] = new JArray(EnabledMenus ?? new())
            , ["hidden_menus"] = new JArray(HiddenMenus ?? new())
            , ["skip_region_selection"] = SkipRegionSelection
            // ห้ามบอก client ให้วาดกว้างกว่าที่เซิร์ฟส่งจริง ไม่งั้นวงนอกจะเป็นที่ว่างสีเทา
            // (ดูคอมเมนต์ที่ ServerChunkSendRange)
            , ["world_chunk_range"] = Math.Clamp(Math.Min(WorldChunkRange, ServerChunkSendRange), 2, 4)
        };
    }
}
