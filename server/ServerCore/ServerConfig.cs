using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Shared.Economy;

namespace DurangoServer.Core;

/// <summary>
/// ค่าปรับสมดุลทั้งหมดที่แก้ได้โดย **ไม่ต้อง build ใหม่** — อยู่ที่ `data/config.json`
///
/// ทำไมต้องมี: เดิมเรทเกิดสัตว์/เลือด/ดาเมจ/exp เป็น `const` ในโค้ด แก้ทีต้อง build ใหม่ทุกที
/// คนที่ดูแลเซิร์ฟ (ที่ไม่ได้เขียนโค้ด) จึงปรับอะไรเองไม่ได้เลย
///
/// **hot-reload**: ตรวจเวลาแก้ไขไฟล์ทุก 5 วินาที เจอว่าเปลี่ยนแล้วโหลดใหม่ทันที
///   · ค่าตัวเลข (exp · เลือด/ดาเมจ · เวลาซาก/เกิดใหม่ · ความเร็ว) มีผล **ทันที**
///   · ตารางสัตว์ (ชนิด/โควตา) มีผลตอน **เปิดเซิร์ฟใหม่** เพราะสัตว์ถูกเกิดไปแล้วตั้งแต่ตอนเปิด
///
/// ไฟล์เสีย/อ่านไม่ได้ = ใช้ค่าเริ่มต้นที่ฝังในโค้ด แล้วเตือนใน log (เซิร์ฟไม่ล่ม)
/// </summary>
public static class ServerConfig
{
    private static readonly object _lock = new object();
    private static ConfigRoot _current = ConfigRoot.Defaults();
    private static string _path;
    private static DateTime _lastWrite;
    private static double _nextCheckAt;

    /// <summary>ค่าที่ใช้อยู่ตอนนี้ (อ่านได้จากทุก thread)</summary>
    public static ConfigRoot Current
    {
        get { lock (_lock) { return _current; } }
    }

    /// <summary>
    /// เลเวลของของธรรมชาติบนเกาะนี้ (ไม้ ผลไม้ หอย หิน) — แยกจากเลเวลสัตว์
    /// 0 ใน config = ใช้ MinLevel ของเกาะ ถ้าไม่มีเกาะใช้ 1
    /// </summary>
    public static int ResourceLevel
    {
        get
        {
            int v = Current?.World?.ResourceLevel ?? 0;
            if (v > 0)
            {
                return v;
            }
            if (IslandRegistry.Current != null)
            {
                return Math.Max(1, IslandRegistry.Current.MinLevel);
            }
            return 1;
        }
    }

    /// <summary>โหลดครั้งแรกตอนเปิดเซิร์ฟ — ไม่มีไฟล์ก็เขียนไฟล์ค่าเริ่มต้นให้เลย</summary>
    public static void Load(string path)
    {
        _path = path;
        if (!File.Exists(path))
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // โหมดหลายเกาะ: เกาะใหม่ได้ตารางสัตว์ที่เลื่อนช่วงเลเวลให้ตรงกับเกาะนั้นเลย
                ConfigRoot seed = IslandRegistry.Current == null
                    ? ConfigRoot.Defaults()
                    : ConfigRoot.DefaultsForIsland(IslandRegistry.Current);
                File.WriteAllText(path, JsonConvert.SerializeObject(seed, Formatting.Indented));
                Console.WriteLine("[config] ไม่มี {0} — สร้างไฟล์ค่าเริ่มต้นให้แล้ว", path);
            }
            catch (Exception e)
            {
                Console.WriteLine("[config] เขียนไฟล์ค่าเริ่มต้นไม่ได้: {0}", e.Message);
            }
        }
        Reload(quiet: false);
    }

    /// <summary>path ไฟล์ config ปัจจุบัน — admin web panel ใช้แสดง path ให้เจ้าของเซิร์ฟดู</summary>
    public static string ConfigPath => _path;

    /// <summary>ค่าปัจจุบันทั้งก้อนเป็น JSON สวย ๆ — admin panel เอาไปใส่กล่องข้อความให้แก้</summary>
    public static string CurrentJson
    {
        get { lock (_lock) { return JsonConvert.SerializeObject(_current, Formatting.Indented); } }
    }

    /// <summary>
    /// admin web panel ส่ง JSON ที่แก้แล้วมาที่นี่ — ใช้ตรรกะเดียวกับ Reload() (PopulateObject บนค่า
    /// เริ่มต้น + FillMissing + Validate) แต่ตรวจก่อนค่อยเขียนไฟล์จริง แล้วมีผล**ทันที**
    /// (ไม่ต้องรอ Tick ตรวจไฟล์ทุก 5 วิ) คืน true ถ้าสำเร็จ, false+เหตุผลถ้า JSON ผิดหรือค่าไม่ถูกต้อง
    /// </summary>
    public static bool TryApplyJson(string json, out string error)
    {
        error = null;
        if (_path == null)
        {
            error = "ยังไม่ได้ตั้งค่า config path (เซิร์ฟยังโหลด config ไม่เสร็จ)";
            return false;
        }
        ConfigRoot loaded;
        try
        {
            loaded = ConfigRoot.Defaults();
            JsonConvert.PopulateObject(json, loaded);
        }
        catch (Exception e)
        {
            error = "JSON ไม่ถูกต้อง: " + e.Message;
            return false;
        }
        loaded.FillMissing();
        string problem = loaded.Validate();
        if (problem != null)
        {
            error = problem;
            return false;
        }
        string normalized = JsonConvert.SerializeObject(loaded, Formatting.Indented);
        try
        {
            File.WriteAllText(_path, normalized);
        }
        catch (Exception e)
        {
            error = "เขียนไฟล์ไม่สำเร็จ: " + e.Message;
            return false;
        }
        lock (_lock)
        {
            _current = loaded;
        }
        try
        {
            _lastWrite = File.GetLastWriteTimeUtc(_path);
        }
        catch (Exception)
        {
        }
        Console.WriteLine("[config] แก้ผ่าน admin panel แล้ว มีผลทันที");
        return true;
    }

    /// <summary>เรียกทุก tick — ตรวจไฟล์ทุก 5 วินาที (ถูกกว่าการอ่านไฟล์ทุกเฟรมมาก)</summary>
    public static void Tick(double now)
    {
        if (_path == null || now < _nextCheckAt)
        {
            return;
        }
        _nextCheckAt = now + 5.0;
        try
        {
            if (File.Exists(_path) && File.GetLastWriteTimeUtc(_path) != _lastWrite)
            {
                Reload(quiet: false);
            }
        }
        catch (IOException)
        {
            // คนกำลังเซฟไฟล์อยู่พอดี — รอบหน้าค่อยลองใหม่
        }
    }

    private static void Reload(bool quiet)
    {
        try
        {
            string json = File.ReadAllText(_path);

            // 🐛 เดิมใช้ DeserializeObject ตรง ๆ ⇒ **ฟิลด์ใหม่ที่ไฟล์เก่ายังไม่มี จะกลายเป็น 0**
            // ไม่ใช่ค่าเริ่มต้น (FillMissing ช่วยได้แค่ระดับ "ทั้งหัวข้อหาย" ไม่ใช่ทีละฟิลด์)
            // เคสจริงที่เจอ: เพิ่ม `MinTilesInland` (ระยะห่างชายฝั่ง) แต่ config เก่าไม่มีฟิลด์นี้
            // ⇒ ได้ 0 = ให้สัตว์เกิดริมน้ำได้ ⇒ **ไดโนเสาร์เต็มชายหาด** ทั้งที่โค้ดกรองแล้ว
            //
            // แก้ด้วยการเริ่มจาก "ค่าเริ่มต้นทั้งก้อน" แล้วให้ไฟล์ทับเฉพาะฟิลด์ที่มีอยู่จริง
            // (ฟิลด์ที่ไม่มีในไฟล์จึงคงค่าเริ่มต้นไว้ — ใช้ได้กับทุกฟิลด์ที่จะเพิ่มในอนาคตด้วย)
            ConfigRoot loaded = ConfigRoot.Defaults();
            JsonConvert.PopulateObject(json, loaded);
            if (loaded == null)
            {
                throw new InvalidDataException("ไฟล์ว่างหรือไม่ใช่ JSON");
            }
            bool filled = loaded.FillMissing();
            // ไฟล์เก่าไม่มีฟิลด์ใหม่ = เขียนกลับให้ครบ คนที่แก้ผ่านเมนูจะได้เห็นและปรับได้
            string normalized = JsonConvert.SerializeObject(loaded, Formatting.Indented);
            filled = filled || normalized.Length != json.Trim().Length;
            string problem = loaded.Validate();
            if (problem != null)
            {
                Console.WriteLine("[config] ⚠️ ค่าไม่ถูกต้อง: {0} — ใช้ค่าเดิมต่อ", problem);
                return;
            }
            lock (_lock)
            {
                _current = loaded;
            }
            if (filled)
            {
                // เขียนกลับให้ไฟล์มีหัวข้อครบ (เช่นตอนอัปเดตเซิร์ฟแล้วมีหัวข้อใหม่เพิ่มมา)
                try
                {
                    File.WriteAllText(_path, normalized);
                    Console.WriteLine("[config] เติมค่าที่ยังไม่มีในไฟล์ให้ครบแล้ว");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[config] เขียนไฟล์กลับไม่ได้: {0}", e.Message);
                }
            }
            _lastWrite = File.GetLastWriteTimeUtc(_path);
            if (!quiet)
            {
                Console.WriteLine("[config] โหลด {0} แล้ว — สัตว์ {1} ชนิด (โควตารวม {2}) · exp ฆ่าสัตว์ {3}+{4}/เลเวล",
                    Path.GetFileName(_path), loaded.Spawn.Count, loaded.TotalQuota,
                    loaded.Exp.KillBase, loaded.Exp.KillPerLevel);
                // ขอบเขตของรอบนี้ — พิมพ์ทุกครั้งที่โหลด จะได้ไม่มีทางเปิดเซิร์ฟผิดขอบเขตโดยไม่รู้ตัว
                Console.WriteLine("[feature] {0}", loaded.Features.Describe());
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[config] ⚠️ อ่าน {0} ไม่ได้ ({1}) — ใช้ค่าเดิมต่อ", _path, e.Message);
        }
    }
}

// ── โครงของไฟล์ config ────────────────────────────────────────────────

public sealed class ConfigRoot
{
    public AnimalConfig Animals { get; set; }
    public ExpConfig Exp { get; set; }
    public SkillConfig Skills { get; set; }
    public StatusEffectConfig StatusEffects { get; set; }

    /// <summary>ระบบป่วย — ป่วยแล้วคราฟต์ช้า/เปลืองแรง/ล้าไว/เดินช้า</summary>
    public SicknessConfig Sickness { get; set; }

    /// <summary>การเซฟอัตโนมัติ + แบ็กอัพตามรอบเวลา</summary>
    public SaveConfig Save { get; set; }

    /// <summary>ประกาศค้าง — โชว์ให้ทุกคนตอนเข้าเกม จนกว่าจะลบข้อความออก</summary>
    public AnnouncementConfig Announcement { get; set; }

    /// <summary>
    /// ที่อยู่ฐานสำหรับโหลด asset bundle (ว่าง = เสิร์ฟจากตัวเซิร์ฟเองเหมือนเดิม)
    ///
    /// 🐛 [4 ก.ย. 2026] เซิร์ฟอ่าน bundle ด้วย File.ReadAllBytes บนเธรดเดียวกับลูปเกม
    ///    ⇒ มือถือโหลดชุด bundle (908 MB / 2,117 ไฟล์) ที tps ตกจาก 120 เหลือ 0-9 ทั้งเกาะ
    ///    (พิสูจน์แล้ว: เกาะที่ไม่มีคนโหลด bundle = 120 tps เต็มด้วยบิลด์เดียวกัน)
    ///    ตั้งค่านี้ชี้ไป nginx/CDN แล้ว client จะไปโหลดที่นั่นแทน — เกมไม่ต้องแบกงานไฟล์
    ///    เช่น "http://187.53.129.69:8790"  (ไม่ต้องมี / ปิดท้าย)
    /// </summary>
    public string AssetBundleUrlBase { get; set; } = "";
    public CraftMenuConfig CraftMenu { get; set; }

    /// <summary>[TodoList/02,04,06] กติกาคราฟต์ให้เหมือนต้นฉบับ (เลเวลผลลัพธ์ · เวลาทำงาน · สำเร็จมาก)</summary>
    public CraftingConfig Crafting { get; set; }

    /// <summary>[TodoList/07] ตายแล้วเสียอะไร (constants.death_penalty)</summary>
    public DeathConfig Death { get; set; }
    /// <summary>⚠️ ต้องเป็น Replace ไม่งั้น PopulateObject จะ**ต่อท้าย**ของเดิมจนสัตว์ซ้ำเป็นสองเท่า</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<SpawnEntryConfig> Spawn { get; set; }

    /// <summary>
    /// โซนที่อยู่อาศัยของสัตว์ — สัตว์แต่ละชนิดเกิดและเดินอยู่ในโซนของมัน
    /// (ว่าง = กระจายทั่วเกาะแบบเดิม)
    /// </summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<ZoneConfig> Zones { get; set; }

    public WorldConfig World { get; set; }

    public ToolConfig Tools { get; set; }

    /// <summary>สวิตช์เปิด/ปิดระบบทีละอย่าง — ขอบเขต beta 1.0.0 (ดู FeatureConfig)</summary>
    public FeatureConfig Features { get; set; }

    /// <summary>รอบสภาพอากาศที่ server ส่งให้ client ทุกคนในเกาะเดียวกัน</summary>
    public WeatherConfig Weather { get; set; }

    /// <summary>เลือด/สตามินา/ความล้า (ดู SurvivalConfig)</summary>
    public SurvivalConfig Survival { get; set; }

    /// <summary>ของที่ผู้เล่นใหม่คราฟได้ตั้งแต่แรก (ดู StarterConfig)</summary>
    public StarterConfig Starter { get; set; }

    /// <summary>อาหาร — กินแล้วได้อะไร (ดู FoodConfig)</summary>
    public FoodConfig Food { get; set; }

    /// <summary>ค่าสถานะพื้นฐาน 8 ตัวของตัวละคร (ดู AbilityConfig / AbilityData)</summary>
    public AbilityConfig Abilities { get; set; }

    /// <summary>ดาเมจ/ค่าป้องกัน (ดู CombatConfig)</summary>
    public CombatConfig Combat { get; set; }

    /// <summary>ปลูกผัก — ความเร็วโต/น้ำ/ปุ๋ย (ดู FarmingConfig)</summary>
    public FarmingConfig Farming { get; set; }

    /// <summary>รอยแยก/วาร์ปเรกเซเลอเรเตอร์ — เวลาแต่ละเฟส/จำนวนสัตว์/รางวัล (ดู WarpAcceleratorConfig)</summary>
    public WarpAcceleratorConfig WarpAccelerator { get; set; }

    /// <summary>
    /// [3 ก.ย. 2026] สิ่งที่เซิร์ฟทำ "แทน" แพตช์ client ให้เกมมือถือของแท้ (APK แพตช์แค่ URL ไม่มีโค้ดเรา)
    /// เพื่อให้มือถือได้ระบบใกล้เคียง client PC ชุดเรา — ดู AndroidConfig · hot-reload ได้
    /// </summary>
    public AndroidConfig Android { get; set; }

    /// <summary>ระบบสมัครไอดีของเราเอง (หน้า /id) — ดู <see cref="PlayerIdConfig"/></summary>
    public PlayerIdConfig PlayerIds { get; set; }

    /// <summary>
    /// [3 ก.ย. 2026] client ชุดเราตั้งแต่รุ่นไหนที่รู้จักบรอดแคสต์แบบกำหนดเวลา/ขนาด/สี ("##bc|…")
    /// client ที่เก่ากว่านี้ (และมือถือของแท้ที่ส่ง "5.2.1") จะได้รับเป็นข้อความธรรมดาแทน
    /// ไม่งั้นผู้เล่นเห็นรหัส "##bc|d=5|z=2|…" ดิบ ๆ บนจอ — GameManager.ShowAdminBroadcast เพิ่มใน 0.1.4
    /// </summary>
    public string StyledBroadcastMinClientVersion { get; set; }

    /// <summary>
    /// [แก้เอง] 24 ส.ค. 2026 — ข้ามฉากรถไฟ/หนังเปิดตอนสร้างตัวละครใหม่ไหม (true = ข้าม)
    /// ค่านี้ถูกส่งไปให้ client ผ่าน /entry (ดู Gateway.cs) แล้ว client ตั้ง
    /// PrologueManager.ToBeSkipped ตามนี้เอง ⇒ **สลับได้จาก data/config.json โดยไม่ต้อง build/แจก
    /// client ใหม่เลย** (แก้ไฟล์นี้ รอ hot-reload 5 วิ พอผู้เล่นคนต่อไปเข้า /entry ก็ได้ค่าใหม่ทันที)
    /// default = true เพราะฉากรถไฟเต็มรูปแบบมี MediaPlayerCtrl เล่นวิดีโอที่ไฟล์หายไปจาก asset bundle
    /// ที่แจกอยู่ ⇒ ทำให้เกม "ปิดตัวเองกะทันหัน" ตอนผู้เล่นใหม่สร้างตัวละครครั้งแรก (เจอจากเทสแจกจริง)
    /// </summary>
    public bool SkipPrologueVideo { get; set; }

    /// <summary>
    /// [4 ก.ย. 2026] template ของภูมิภาคที่ส่งให้ client (Region.TemplateId) — client เอาไปแสดง "เลเวลเกาะ"
    /// (RegionTemplateData[TemplateId].Level) และไอคอนง่าย/ยากของสัตว์ · ว่าง = ใช้ region_template ของ terrain
    /// เช่น terrain ri35te → ri35teSub01 = Lv35 · ตั้ง "ru10gr170511" = เกาะโชว์ Lv10, "ri15sa171228" = Lv15
    /// (ควรตั้ง Survival.RegionLevel ให้ตรงกับเลเวลนี้ด้วย เพื่อสูตรความล้า)
    /// </summary>
    public string RegionTemplateId { get; set; }

    /// <summary>
    /// [3 ก.ย. 2026] เพดานจำนวนผู้เล่นออนไลน์พร้อมกัน (hot-reload ได้ แก้ไฟล์รอ 5 วิ มีผลทันที ไม่ต้องรีสตาร์ต)
    /// เดิมตั้งได้แค่ตอนเปิดเซิร์ฟด้วย --max-connections ⇒ อยากเพิ่มต้องรีสตาร์ต (ผู้เล่นหลุด)
    /// ค่านี้ชนะ --max-connections · ตั้ง &lt;= 0 = ใช้ค่าจาก flag เดิม (หรือ default โค้ด)
    /// </summary>
    public int MaxOnlinePlayers { get; set; }

    /// <summary>เพดานผู้เล่นจาก IP เดียวกัน (hot-reload) · &lt;= 0 = ใช้ค่าจาก flag เดิม</summary>
    public int MaxPlayersPerIp { get; set; }

    public int TotalQuota
    {
        get
        {
            int n = 0;
            for (int i = 0; i < Spawn.Count; i++)
            {
                n += Spawn[i].Quota;
            }
            return n;
        }
    }

    public static ConfigRoot Defaults()
    {
        return new ConfigRoot
        {
            Animals = AnimalConfig.Defaults(),
            Exp = ExpConfig.Defaults(),
            Skills = SkillConfig.Defaults(),
            StatusEffects = StatusEffectConfig.Defaults(),
            Sickness = SicknessConfig.Defaults(),
            Save = SaveConfig.Defaults(),
            Announcement = AnnouncementConfig.Defaults(),
            CraftMenu = CraftMenuConfig.Defaults(),
            Crafting = CraftingConfig.Defaults(),
            Death = DeathConfig.Defaults(),
            Spawn = SpawnEntryConfig.Defaults(),
            Zones = ZoneConfig.Defaults(),
            World = WorldConfig.Defaults(),
            Tools = ToolConfig.Defaults(),
            Features = FeatureConfig.Defaults(),
            Weather = WeatherConfig.Defaults(),
            Survival = SurvivalConfig.Defaults(),
            Starter = StarterConfig.Defaults(),
            Food = FoodConfig.Defaults(),
            Abilities = AbilityConfig.Defaults(),
            Combat = CombatConfig.Defaults(),
            Farming = FarmingConfig.Defaults(),
            WarpAccelerator = WarpAcceleratorConfig.Defaults(),
            Android = AndroidConfig.Defaults(),
            PlayerIds = PlayerIdConfig.Defaults(),
            StyledBroadcastMinClientVersion = "0.1.4",
            SkipPrologueVideo = true,
            RegionTemplateId = "",
            MaxOnlinePlayers = 50,
            MaxPlayersPerIp = 8
        };
    }

    /// <summary>
    /// ค่าเริ่มต้นของ "เกาะหนึ่ง" — เอาตารางสัตว์มาตรฐานมาเลื่อนช่วงเลเวลให้ตรงกับเกาะ
    ///
    /// ตัวอย่าง: ตารางมาตรฐานเป็น lv1-10 · เกาะ lv10-20 จะได้สัตว์ชุดเดิมแต่เลเวล 10-20
    /// (กิ้งก่าที่เกาะแรก lv1-3 → เกาะที่สอง lv10-12) เรียงจากอ่อนไปแรงเหมือนเดิม
    /// ปรับต่อเองได้ทั้งหมดในไฟล์ config ของเกาะนั้น
    /// </summary>
    public static ConfigRoot DefaultsForIsland(IslandInfo isle)
    {
        ConfigRoot cfg = Defaults();
        int srcLo = 1, srcHi = 10;              // ช่วงของตารางมาตรฐาน
        int dstLo = Math.Max(1, isle.MinLevel);
        int dstHi = Math.Max(dstLo, isle.MaxLevel);
        for (int i = 0; i < cfg.Spawn.Count; i++)
        {
            SpawnEntryConfig e = cfg.Spawn[i];
            e.MinLevel = Remap(e.MinLevel, srcLo, srcHi, dstLo, dstHi);
            e.MaxLevel = Math.Max(e.MinLevel, Remap(e.MaxLevel, srcLo, srcHi, dstLo, dstHi));
        }
        cfg.World.ResourceLevel = dstLo;
        return cfg;
    }

    private static int Remap(int value, int srcLo, int srcHi, int dstLo, int dstHi)
    {
        if (srcHi <= srcLo)
        {
            return dstLo;
        }
        float t = (value - srcLo) / (float)(srcHi - srcLo);
        return (int)Math.Round(dstLo + t * (dstHi - dstLo));
    }

    /// <summary>
    /// ฟิลด์ที่หายไปจากไฟล์ = ใช้ค่าเริ่มต้นแทน ไม่ใช่ null แล้วพัง
    /// คืน true ถ้าต้องเติมอะไรเข้าไป (ผู้เรียกจะได้เขียนไฟล์กลับให้ครบ —
    /// สำคัญเพราะตัวแก้ config ในเมนูอ่านจากไฟล์ตรง ๆ ถ้าไฟล์ไม่มีหัวข้อนั้นก็แก้ไม่ได้)
    /// </summary>
    public bool FillMissing()
    {
        bool filled = false;
        if (Animals == null) { Animals = AnimalConfig.Defaults(); filled = true; }
        if (Animals.Herds == null) { Animals.Herds = HerdConfig.Defaults(); filled = true; }
        if (Animals.Defense == null) { Animals.Defense = AnimalDefenseConfig.Defaults(); filled = true; }
        if (Exp == null) { Exp = ExpConfig.Defaults(); filled = true; }
        if (Skills == null) { Skills = SkillConfig.Defaults(); filled = true; }
        if (StatusEffects == null) { StatusEffects = StatusEffectConfig.Defaults(); filled = true; }
        if (Sickness == null) { Sickness = SicknessConfig.Defaults(); filled = true; }
        if (Save == null) { Save = SaveConfig.Defaults(); filled = true; }
        if (Announcement == null) { Announcement = AnnouncementConfig.Defaults(); filled = true; }
        if (CraftMenu == null || CraftMenu.HiddenCategories == null) { CraftMenu = CraftMenuConfig.Defaults(); filled = true; }
        if (Crafting == null) { Crafting = CraftingConfig.Defaults(); filled = true; }
        if (Death == null) { Death = DeathConfig.Defaults(); filled = true; }
        if (Zones == null) { Zones = ZoneConfig.Defaults(); filled = true; }
        if (World == null) { World = WorldConfig.Defaults(); filled = true; }
        if (Tools == null) { Tools = ToolConfig.Defaults(); filled = true; }
        if (Tools.Deltas == null) { Tools.Deltas = WearDeltaConfig.Defaults(); filled = true; }
        if (Features == null) { Features = FeatureConfig.Defaults(); filled = true; }
        if (Weather == null) { Weather = WeatherConfig.Defaults(); filled = true; }
        if (Survival == null) { Survival = SurvivalConfig.Defaults(); filled = true; }
        if (Starter == null || Starter.Recipes == null) { Starter = StarterConfig.Defaults(); filled = true; }
        if (Food == null) { Food = FoodConfig.Defaults(); filled = true; }
        if (Abilities == null) { Abilities = AbilityConfig.Defaults(); filled = true; }
        if (Combat == null) { Combat = CombatConfig.Defaults(); filled = true; }
        if (Farming == null) { Farming = FarmingConfig.Defaults(); filled = true; }
        if (WarpAccelerator == null) { WarpAccelerator = WarpAcceleratorConfig.Defaults(); filled = true; }
        if (Android == null) { Android = AndroidConfig.Defaults(); filled = true; }
        if (PlayerIds == null) { PlayerIds = PlayerIdConfig.Defaults(); filled = true; }
        if (string.IsNullOrWhiteSpace(StyledBroadcastMinClientVersion)) { StyledBroadcastMinClientVersion = "0.1.4"; filled = true; }
        if (Spawn == null || Spawn.Count == 0) { Spawn = SpawnEntryConfig.Defaults(); filled = true; }
        if (MaxOnlinePlayers <= 0) { MaxOnlinePlayers = 50; filled = true; }
        if (MaxPlayersPerIp <= 0) { MaxPlayersPerIp = 8; filled = true; }
        filled |= RepairMangledNames();
        return filled;
    }

    /// <summary>
    /// 🐛 ชื่อไทยในไฟล์ config พังเป็นตัวขยะ (เจอจริง: `[animal] เน€เธยเน€เธเธ...` ใน log)
    ///
    /// ต้นเหตุ: มีคน/เครื่องมืออ่านไฟล์ UTF-8 นี้เป็น ANSI (cp874) แล้วเขียนกลับ
    /// ⇒ ตัวอักษรไทยกลายเป็นลำดับ "เ + อักขระคุม C1" ซึ่ง**กู้กลับไม่ได้** (ข้อมูลหายจริง ๆ)
    ///
    /// ชื่อพวกนี้เป็นแค่ป้ายกำกับ ไม่มีผลต่อเกม — จึงเขียนทับด้วยชื่อตั้งต้นจากโค้ดไปเลย
    /// แล้ว Reload จะเซฟไฟล์กลับให้เอง (ไฟล์ซ่อมตัวเองรอบเดียวจบ ไม่ต้องแก้มือ)
    /// ⚠️ ชื่อที่ผู้ดูแลตั้งเองยังอยู่ครบ — เช็คเฉพาะตัวที่มีร่องรอยการแปลงรหัสผิดเท่านั้น
    /// </summary>
    private bool RepairMangledNames()
    {
        bool fixedAny = false;
        if (Spawn != null)
        {
            List<SpawnEntryConfig> defaults = SpawnEntryConfig.Defaults();
            for (int i = 0; i < Spawn.Count; i++)
            {
                SpawnEntryConfig e = Spawn[i];
                if (e == null || !LooksMangled(e.Name))
                {
                    continue;
                }
                SpawnEntryConfig d = defaults.Find(x => x.Type == e.Type);
                string repaired = d?.Name ?? ("สัตว์ " + e.Type);
                Console.WriteLine("[config] ชื่อสัตว์ type {0} ในไฟล์เป็นตัวขยะ (อ่าน UTF-8 เป็น ANSI มาก่อน) — ใช้ \"{1}\" แทน", e.Type, repaired);
                e.Name = repaired;
                fixedAny = true;
            }
        }
        if (Zones != null)
        {
            List<ZoneConfig> defaults = ZoneConfig.Defaults();
            for (int i = 0; i < Zones.Count; i++)
            {
                ZoneConfig z = Zones[i];
                if (z == null || !LooksMangled(z.Name))
                {
                    continue;
                }
                ZoneConfig d = defaults.Find(x => x.Id == z.Id);
                string repaired = d?.Name ?? (z.Id ?? "โซน");
                Console.WriteLine("[config] ชื่อโซน {0} ในไฟล์เป็นตัวขยะ — ใช้ \"{1}\" แทน", z.Id, repaired);
                z.Name = repaired;
                fixedAny = true;
            }
        }
        return fixedAny;
    }

    /// <summary>
    /// ข้อความนี้ผ่านการแปลงรหัสผิดมาหรือเปล่า — ดูจากร่องรอยที่ข้อความไทยจริง ๆ ไม่มีทางมี:
    /// อักขระแทนที่ (U+FFFD), อักขระคุมช่วง C1 (U+0080–U+009F) และเครื่องหมายยูโร
    /// (0x80 ใน cp874 = € — โผล่มาทุกครั้งที่ไบต์ UTF-8 ของสระ "เ" ถูกอ่านผิด)
    /// </summary>
    private static bool LooksMangled(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return true;
        }
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '�' || c == '€' || (c >= '\u0080' && c <= '\u009F'))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>คืนข้อความปัญหาถ้าค่าที่ใส่มาทำให้เกมพัง (คืน null = ผ่าน)</summary>
    public string Validate()
    {
        if (Animals.LifeBase <= 0 || Animals.LifePerLevel < 0)
        {
            return "เลือดสัตว์ต้องมากกว่า 0";
        }
        if (Animals.RespawnSeconds <= 0 || Animals.CorpseSeconds <= 0)
        {
            return "เวลาเกิดใหม่/เวลาซากต้องมากกว่า 0";
        }
        if (Animals.LifetimeSeconds <= 0)
        {
            return "อายุสัตว์ (LifetimeSeconds) ต้องมากกว่า 0";
        }
        if (Animals.Defense != null && Animals.Defense.Enabled && Animals.Defense.Scale < 0f)
        {
            return "Animals.Defense.Scale ติดลบไม่ได้";
        }
        if (Animals.Herds != null && Animals.Herds.Enabled)
        {
            HerdConfig h = Animals.Herds;
            if (h.CountScale <= 0f || h.SizeScale <= 0f || h.CountScale > 3f || h.SizeScale > 3f)
            {
                return "Animals.Herds.CountScale/SizeScale ต้องอยู่ระหว่าง 0-3 (1 = ตามเกม)";
            }
            if (h.RadiusTiles <= 0f || h.WanderTiles <= 0f || h.SeparationTiles < 0f || h.DriftTiles < 0f)
            {
                return "Animals.Herds.RadiusTiles/WanderTiles ต้องมากกว่า 0 · SeparationTiles/DriftTiles ติดลบไม่ได้";
            }
            if (h.DriftMinSeconds <= 0 || h.DriftMaxSeconds < h.DriftMinSeconds)
            {
                return "Animals.Herds.DriftMinSeconds ต้องมากกว่า 0 และไม่เกิน DriftMaxSeconds";
            }
            if (h.LevelMin < 0 || h.LevelMax < h.LevelMin)
            {
                return "Animals.Herds.LevelMin/LevelMax ไม่ถูกต้อง (0,0 = ใช้ level ของ template)";
            }
        }
        if (Animals.ChaseSpeed <= 0 || Animals.FleeSpeed <= 0 || Animals.WalkSpeed <= 0)
        {
            return "ความเร็วสัตว์ต้องมากกว่า 0";
        }
        if (Animals.MaxWalkLegSeconds <= 0 || Animals.RestMaxSeconds < Animals.RestMinSeconds)
        {
            return "เวลาเดิน/เวลาพักไม่ถูกต้อง";
        }
        for (int i = 0; i < Spawn.Count; i++)
        {
            SpawnEntryConfig e = Spawn[i];
            if (e.Type < 2000 || e.Type > 2999)
            {
                return $"ชนิดสัตว์ {e.Type} อยู่นอกช่วง 2000-2999";
            }
            if (e.MinLevel < 1 || e.MaxLevel < e.MinLevel)
            {
                return $"ช่วงเลเวลของ {e.Name} ไม่ถูกต้อง ({e.MinLevel}-{e.MaxLevel})";
            }
            if (e.Quota < 0 || e.Quota > 200)
            {
                return $"โควตาของ {e.Name} ต้องอยู่ระหว่าง 0-200";
            }
            if (e.AttackCooltime <= 0)
            {
                return $"คูลดาวน์กัดของ {e.Name} ต้องมากกว่า 0";
            }
        }
        if (Skills.FullAt < 1)
        {
            return "skills.FullAt ต้องมากกว่า 0";
        }
        for (int i = 0; i < Zones.Count; i++)
        {
            ZoneConfig z = Zones[i];
            if (z.RadiusTiles <= 0f)
            {
                return $"รัศมีของโซน {z.Name} ต้องมากกว่า 0";
            }
            if (z.Species == null || z.Species.Count == 0)
            {
                return $"โซน {z.Name} ไม่ได้ระบุว่ามีสัตว์ชนิดไหน";
            }
        }
        if (TotalQuota > 500)
        {
            return $"สัตว์รวมทั้งเกาะ {TotalQuota} ตัว เยอะเกินไป (เพดาน 500)";
        }
        if (World.ChunkSendRange < 1 || World.ChunkSendRange > 4)
        {
            return $"World.ChunkSendRange ต้องอยู่ระหว่าง 1-4 (ใส่มา {World.ChunkSendRange})";
        }
        if (World.ClientChunkRetainRange < 1 || World.ClientChunkRetainRange > 4)
        {
            return $"World.ClientChunkRetainRange ต้องอยู่ระหว่าง 1-4 (ใส่มา {World.ClientChunkRetainRange})";
        }
        if (World.ViewRangeTiles < 4f || World.ViewRangeTiles > 200f)
        {
            return $"World.ViewRangeTiles ต้องอยู่ระหว่าง 4-200 tile (ใส่มา {World.ViewRangeTiles})";
        }
        if (World.ViewMarginTiles < 1f || World.ViewMarginTiles > World.ViewRangeTiles)
        {
            // ต่ำกว่า 1 = คนที่ยืนขอบระยะจะโผล่-หายรัว ๆ · มากกว่าระยะเห็น = ช่องว่างกว้างจนไม่มีความหมาย
            return $"World.ViewMarginTiles ต้องอยู่ระหว่าง 1 ถึง ViewRangeTiles ({World.ViewRangeTiles})";
        }
        if (World.ViewCheckSeconds < 0.05 || World.ViewCheckSeconds > 5.0)
        {
            return $"World.ViewCheckSeconds ต้องอยู่ระหว่าง 0.05-5 วินาที (ใส่มา {World.ViewCheckSeconds})";
        }
        if (World.ResourceLevel < 0 || World.ResourceLevel > LevelData.MaxLevel)
        {
            return $"World.ResourceLevel ต้องอยู่ระหว่าง 0-{LevelData.MaxLevel} (0 = ตามเลเวลเกาะ, ใส่มา {World.ResourceLevel})";
        }
        if (Tools.Enabled && (Tools.DurabilityBase < 0f || Tools.DurabilityPerTier < 0f
                              || Tools.DurabilityBase + Tools.DurabilityPerTier <= 0f))
        {
            return "ความทนทานเครื่องมือต้องมากกว่า 0 (Tools.DurabilityBase / DurabilityPerTier)";
        }
        if (Tools.WearPerUse < 0f)
        {
            return "Tools.WearPerUse ติดลบไม่ได้";
        }
        if (Tools.Deltas != null && Tools.Deltas.Enabled)
        {
            WearDeltaConfig d = Tools.Deltas;
            if (d.Craft < 0f || d.Collect < 0f || d.Build < 0f || d.Attack < 0f || d.Defense < 0f || d.Taming < 0f)
            {
                return "Tools.Deltas.* ติดลบไม่ได้";
            }
            if (d.RepairDamageMin < 0f || d.RepairDamageMax > 0.9f || d.RepairDamageMax < d.RepairDamageMin)
            {
                return "Tools.Deltas.RepairDamageMin/Max ต้องอยู่ระหว่าง 0-0.9 และ Min ≤ Max";
            }
        }
        if (Farming.GrowthScale <= 0f || Farming.GrowthScale > 10f)
        {
            return $"Farming.GrowthScale ต้องอยู่ระหว่าง 0 ถึง 10 (ใส่มา {Farming.GrowthScale})";
        }
        if (Farming.MinGrowSeconds < 1f)
        {
            return "Farming.MinGrowSeconds ต้องอย่างน้อย 1 วินาที";
        }
        if (Farming.WaterPerItem <= 0f)
        {
            return "Farming.WaterPerItem ต้องมากกว่า 0";
        }
        if (Farming.SeedYield < 0 || Farming.SeedYield > 10)
        {
            return $"Farming.SeedYield ต้องอยู่ระหว่าง 0-10 (ใส่มา {Farming.SeedYield})";
        }
        if (Farming.WrongBiomeGrowthPenalty < 1f)
        {
            return "Farming.WrongBiomeGrowthPenalty ต้องอย่างน้อย 1 (1 = ไม่ลงโทษ)";
        }
        if (Farming.FertilizerYieldScale < 0f)
        {
            return "Farming.FertilizerYieldScale ติดลบไม่ได้";
        }
        if (Farming.MaxWaterCarryPerDraw <= 0f)
        {
            return "Farming.MaxWaterCarryPerDraw ต้องมากกว่า 0";
        }
        if (Features.MaxPlayerLevel < 0 || Features.MaxPlayerLevel > LevelData.MaxLevel)
        {
            return $"Features.MaxPlayerLevel ต้องอยู่ระหว่าง 0-{LevelData.MaxLevel} (0 = ไม่จำกัด)";
        }
        if (Survival.StaminaMax <= 0f || Survival.LifeMax <= 0f || Survival.FatigueMax <= 0f)
        {
            return "ค่าสูงสุดของเลือด/สตามินา/ความล้าต้องมากกว่า 0";
        }
        if (Survival.FatigueCaution >= Survival.FatigueDanger || Survival.FatigueDanger > Survival.FatigueMax)
        {
            return "ต้องเป็น FatigueCaution < FatigueDanger <= FatigueMax";
        }
        if (Survival.StaminaRegenPerSec <= 0f || Survival.StaminaRegenDelaySeconds < 0f)
        {
            return "อัตราฟื้นสตามินาต้องมากกว่า 0 และเวลาหน่วงติดลบไม่ได้";
        }
        if (Survival.LifeDrainWhenExhausted < 0f || Survival.RestFatiguePerSec < 0f)
        {
            return "เลือดที่ไหลตอนล้าเต็ม / อัตราพักผ่อน ติดลบไม่ได้";
        }
        if (Survival.LifePerLevel < 0f || Survival.LifePerEndurance < 0f
            || Survival.StaminaPerLevel < 0f || Survival.StaminaPerWill < 0f)
        {
            return "เลือด/สตามินาที่เพิ่มต่อเลเวลหรือค่าสถานะ ติดลบไม่ได้";
        }
        if (Abilities.Base < 0f || Abilities.PerLevel < 0f || Abilities.PerProficiency < 0f)
        {
            return "ค่าสถานะพื้นฐาน (abilities) ติดลบไม่ได้";
        }
        if (Abilities.Max < 1f)
        {
            return "Abilities.Max ต้องอย่างน้อย 1";
        }
        if (Combat.BareHandAttack <= 0f)
        {
            return "Combat.BareHandAttack ต้องมากกว่า 0";
        }
        if (Combat.WeaponAttackScale < 0f || Combat.ArmorDefenseScale < 0f || Combat.AttackPerStrength < 0f)
        {
            return "ตัวคูณพลังอาวุธ/เกราะ ติดลบไม่ได้";
        }
        if (Combat.ArmorDefenseK <= 0f)
        {
            return "Combat.ArmorDefenseK ต้องมากกว่า 0 (เป็นตัวหารในสูตรลดดาเมจ)";
        }
        if (Combat.ArmorMaxReduce < 0f || Combat.ArmorMaxReduce > 0.95f)
        {
            return "Combat.ArmorMaxReduce ต้องอยู่ระหว่าง 0-0.95";
        }
        if (Combat.CritChance < 0f || Combat.CritChance > 1f || Combat.CritMultiplier < 1f)
        {
            return "โอกาสคริต้อง 0-1 และตัวคูณคริต้องไม่น้อยกว่า 1";
        }
        return null;
    }
}

/// <summary>
/// ความทนทานของเครื่องมือ — ทำไมถึงต้องมี ดูหัวข้อบนสุดของ ToolDurability.cs
/// ปิดทั้งระบบได้ด้วย <c>"Enabled": false</c> (เครื่องมือกลับไปใช้ได้ตลอดชีพเหมือนเดิม)
/// </summary>
public sealed class ToolConfig
{
    public bool Enabled { get; set; }

    /// <summary>ความทนทานเต็ม = DurabilityBase + ระดับวัสดุ(1-3) × DurabilityPerTier</summary>
    public float DurabilityBase { get; set; }
    public float DurabilityPerTier { get; set; }

    /// <summary>ใช้ 1 ครั้ง (เก็บของ/แล่ซาก 1 ชิ้น) เสียความทนทานเท่าไร — ใช้เมื่อ Deltas ปิด</summary>
    public float WearPerUse { get; set; }

    /// <summary>[TodoList/03] หักตามชนิดงานตาม constants.durability.deltas ของเกม — ดู WearDeltaConfig</summary>
    public WearDeltaConfig Deltas { get; set; }

    public static ToolConfig Defaults()
    {
        // ค่าเริ่มต้นให้ หิน/ไม้ 40 ครั้ง · กระดูก 60 · โลหะ 80
        // 40 ครั้งคือ "ตัดไม้ได้ทั้งบ่าย แล้วต้องทำอันใหม่" — พอให้รู้สึกว่ามีต้นทุน
        // แต่ไม่ถึงกับต้องหยุดทำอย่างอื่นมานั่งคราฟต์ขวานทุก 10 นาที
        return new ToolConfig
        {
            Enabled = true,
            DurabilityBase = 20f,
            DurabilityPerTier = 20f,
            WearPerUse = 1f,
            Deltas = WearDeltaConfig.Defaults()
        };
    }
}

/// <summary>
/// **สวิตช์เปิด/ปิดระบบทีละอย่าง** — ขอบเขตของ beta 1.0.0
///
/// ค่าเริ่มต้นตั้งตาม `1.0.0 beta.txt` (สรุปรอบ LBT1 ของเกมต้นฉบับ ธ.ค. 2015)
/// อะไรที่ไม่ได้อยู่ในรายการนั้น = **ปิดไว้** เปิดทีละอันตอนที่ระบบพร้อมจริง
///
/// แก้ไฟล์ `data/config.json` แล้วเซิร์ฟโหลดใหม่ให้เองใน 5 วินาที ไม่ต้อง build ไม่ต้องปิดเซิร์ฟ
/// เปิดเซิร์ฟทีไรจะพิมพ์ตารางว่าอะไรเปิดอะไรปิดให้ดูทุกครั้ง
/// </summary>
public sealed class FeatureConfig
{
    // ───── เปิดอยู่: ทำเสร็จแล้วและอยู่ในรายการ LBT1 ─────

    /// <summary>เก็บของจากธรรมชาติ (ต้องมีเครื่องมือตามชนิด)</summary>
    public bool Gathering { get; set; }
    /// <summary>คราฟต์ของ/สิ่งปลูกสร้าง</summary>
    public bool Crafting { get; set; }
    /// <summary>ต่อสู้กับสัตว์</summary>
    public bool Combat { get; set; }
    /// <summary>แล่ซากสัตว์</summary>
    public bool Butchery { get; set; }
    /// <summary>สร้างสิ่งปลูกสร้าง/กล่องเก็บของ</summary>
    public bool Building { get; set; }
    /// <summary>เลเวล + exp + แต้มสกิล</summary>
    public bool Progression { get; set; }
    /// <summary>เรียน/ลืมสกิล</summary>
    public bool Skills { get; set; }
    /// <summary>สวมใส่อุปกรณ์</summary>
    public bool Equipment { get; set; }
    /// <summary>เลือด/สตามินา/ความล้า</summary>
    public bool Survival { get; set; }
    /// <summary>ความทนทานเครื่องมือ (ตรงกับ "ไอเทมทุกอย่างมี durability" ในรายการ)</summary>
    public bool ToolDurability { get; set; }
    /// <summary>แชทช่องรวม</summary>
    public bool Chat { get; set; }

    // ───── ปิดไว้: อยู่ในรายการ LBT1 แต่ยังไม่ได้ทำ — เปิดทีละแพทช์ ─────

    /// <summary>เดินทางข้ามเกาะในเกม (ท่าเรือ/วาร์ปโฮล) — โค้ดมีแล้วแต่ยังไม่ได้เทสในเกม</summary>
    public bool IslandTravel { get; set; }
    /// <summary>อาชีพเดิมจากโลกจริง (เลือกตอน Lv.10)</summary>
    public bool Jobs { get; set; }
    /// <summary>ทำอาหาร</summary>
    public bool Cooking { get; set; }
    /// <summary>เพาะปลูก/ทำนา</summary>
    public bool Farming { get; set; }
    /// <summary>เลี้ยงปศุสัตว์ (ให้นม)</summary>
    public bool Livestock { get; set; }
    /// <summary>จับ/ขี่ไดโนเสาร์</summary>
    public bool Taming { get; set; }
    /// <summary>ตลาดซื้อขายระหว่างผู้เล่น</summary>
    public bool Market { get; set; }
    /// <summary>
    /// รอยแยก/วาร์ปเรกเซเลอเรเตอร์ (blueprint "warp_accelerator") — กิจกรรม PvE ป้องกันคลื่นสัตว์
    /// เขียนใหม่ทั้งหมด 22 ส.ค. 2026 (ArtifactFactory + WarpAcceleratorManager + ServerPlayer.WarpAccelerator)
    /// ยังไม่เคยเทสในเกมจริงสักครั้ง (เซิร์ฟหลักกำลังรันสดคู่ขนาน ห้าม restart ทดสอบ) ⇒ ปิดไว้ก่อน
    /// เหมือน Livestock/Taming/Market — เปิดเองตอนพร้อมทดสอบ (แก้ data/config.json หรือ FeatureConfig.Defaults)
    /// </summary>
    public bool WarpAccelerator { get; set; }
    /// <summary>เควสจาก 4 กลุ่ม NPC</summary>
    public bool Quests { get; set; }

    /// <summary>
    /// เอา "เควสประจำวัน" (เดิม: รายการตรวจเซิร์ฟ) มาใส่เป็นเควส (ดู QuestData.Checklist)
    /// ปิดตัวนี้เมื่อเทสผ่านหมดแล้ว — เควสชุดตรวจจะหายไปทันทีโดยไม่กระทบสายสอนเล่น
    /// </summary>
    public bool QuestChecklist { get; set; }
    /// <summary>PK บนเกาะ Lv.20+</summary>
    public bool Pvp { get; set; }
    /// <summary>สิทธิ์ในที่ดินส่วนตัว (เฉพาะเรา/เพื่อน/สาธารณะ)</summary>
    public bool LandPermission { get; set; }
    /// <summary>ปาร์ตี้/แคลน</summary>
    public bool PartyAndClan { get; set; }
    /// <summary>ท่าทาง/อีโมติคอนของผู้เล่น (ไม่ได้อยู่ในรายการ LBT1)</summary>
    public bool Emotes { get; set; }

    // ───── ระบบสังคม/เศรษฐกิจ — ปิดเปิดจาก config.json ได้ ─────

    /// <summary>ระบบเพื่อน (ส่ง/ตอบรับ/ยกเลิกคำขอ จัดประเภทเพื่อน ติดตาม)</summary>
    public bool Friends { get; set; }
    /// <summary>ระบบจดหมาย (ส่งข้อความ/ของแนบ รับ/ลบ/ทำเครื่องหมายอ่าน)</summary>
    public bool Mail { get; set; }
    /// <summary>ระบบกระเป๋าเงิน (โอน DurangoCoin ระหว่างผู้เล่น)</summary>
    public bool Wallet { get; set; }
    /// <summary>กลุ่ม Faction (เข้าร่วม ทำภารกิจรายวัน สนับสนุน)</summary>
    public bool Factions { get; set; }
    /// <summary>ระบบเควส/ภารกิจ (เข้าร่วม/ยกเลิก/สลับ/ชาร์จ)</summary>
    public bool Missions { get; set; }
    /// <summary>ระบบเข้าร่วมประจำวัน (รับรางวัล/ของแถม)</summary>
    public bool Attendance { get; set; }
    /// <summary>ระบบขนส่ง/คลังสินค้า (เปิดปิดประตู ส่งของ ป้องกันรู)</summary>
    public bool Cargo { get; set; }
    /// <summary>ระบบหมู่เกาะ (ท้าทาย/ทำความสะอาด/ล้างพื้นที่/วาร์ป)</summary>
    public bool Archipelago { get; set; }
    /// <summary>ระบบเพลง/ดนตรี (เล่น/หยุด/แชร์/บันทึก)</summary>
    public bool Band { get; set; }
    /// <summary>ระบบ AddOns (วาง/ซื้อ DLC)</summary>
    public bool AddOns { get; set; }
    /// <summary>ระบบย้อม/ฟอกสีไอเทม</summary>
    public bool DyeAndBleach { get; set; }
    /// <summary>แชทส่วนตัว/สร้าง/เข้าร่วมห้องสนทนา (ต้อง auth ผ่าน Tune ก่อน)</summary>
    public bool PrivateConversation { get; set; }

    /// <summary>
    /// เพดานเลเวลผู้เล่น — 0 = ไม่จำกัด (ใช้เพดานเต็มของเกม = 60, <c>constants.max_levels.player</c>)
    ///
    /// beta 1.0.0 เคยล็อกไว้ที่ 20 ตาม LBT1 ของเกมต้นฉบับ ("ปล่อยคอนเทนต์แค่ Lv.1-20")
    /// ปลดเมื่อ 3 ก.ย. 2026 (TodoList/01) — ตารางสูตร/blueprint/สัตว์มีครบถึง 60 อยู่แล้ว
    /// เลเวลสัตว์ไม่ผูกกับค่านี้ (remap ตาม MinLevel/MaxLevel ของแต่ละเกาะ)
    /// ใส่ตัวเลข 1-60 ถ้าอยากล็อกรอบเทสเป็นพิเศษ
    /// </summary>
    public int MaxPlayerLevel { get; set; }

    public static FeatureConfig Defaults()
    {
        return new FeatureConfig
        {
            // ระบบพื้นฐานที่ทำเสร็จแล้ว
            Gathering = true,
            Crafting = true,
            Combat = true,
            Butchery = true,
            Building = true,
            Progression = true,
            Skills = true,
            Equipment = true,
            Survival = true,
            ToolDurability = true,
            Chat = true,

            // ยังไม่ได้ทำ / ยังไม่ได้เทส — เปิดทีละแพทช์
            IslandTravel = false,
            Jobs = true,
            Cooking = true,             // เปิดแล้ว — สูตร cook 152 อัน ต้องยืนที่กองไฟ/เตาถึงจะทำได้
            Farming = true,
            Livestock = false,
            Taming = false,
            Market = false,
            WarpAccelerator = false,   // ยังไม่เคยเทสในเกมจริง — เปิดเองตอนพร้อม
            Quests = true,
            QuestChecklist = true,             // เปิดแล้ว — สายสอนเล่น 8 ขั้นจบที่ต่อแพ (ดู QuestData)
            Pvp = false,
            LandPermission = true,
            PartyAndClan = false,
            Emotes = false,

            // ระบบสังคม/เศรษฐกิจ — ปิดเปิดจาก config.json ได้
            Friends = false,
            Mail = false,
            Wallet = false,
            Factions = false,
            Missions = false,
            Attendance = false,
            Cargo = false,
            Archipelago = false,
            Band = false,
            AddOns = false,
            DyeAndBleach = false,
            PrivateConversation = false,

            MaxPlayerLevel = 0          // ไม่จำกัด = 60 (เดิม 20 ตาม LBT1 — ดู TodoList/01-level-cap.md)
        };
    }

    /// <summary>ตารางสรุปไว้พิมพ์ตอนเปิดเซิร์ฟ</summary>
    public string Describe()
    {
        var on = new List<string>();
        var off = new List<string>();
        foreach (System.Reflection.PropertyInfo p in typeof(FeatureConfig).GetProperties())
        {
            if (p.PropertyType != typeof(bool))
            {
                continue;
            }
            ((bool)p.GetValue(this) ? on : off).Add(p.Name);
        }
        return $"เปิด ({on.Count}): {string.Join(" · ", on)}\n"
             + $"           ปิด ({off.Count}): {string.Join(" · ", off)}\n"
             + $"           เพดานเลเวล: {(MaxPlayerLevel > 0 ? MaxPlayerLevel.ToString() : "ไม่จำกัด")}";
    }
}

/// <summary>
/// เลือด / สตามินา / ความล้า — ตามรายการ beta 1.0.0:
/// *"หิว/กระหาย/สกปรก/เปียกน้ำ → เหนื่อยขึ้นเรื่อยๆ **จนตายได้** ฟื้นด้วยกองไฟ เต็นท์ หลับนอน"*
///
/// วงจรที่ตั้งใจให้เป็น:
///   ทำงาน → สตามินาลด (พักแป๊บเดียวก็คืน) → แต่**ความล้าสะสมไม่คืนเอง**
///   → ล้าถึงขีด = ทำอะไรก็เปลืองขึ้น → ล้าเต็ม = **เลือดไหลลงจนตาย**
///   → ทางแก้ทางเดียวคือกลับไปพักที่กองไฟ
/// ⇒ กองไฟไม่ใช่แค่โต๊ะคราฟต์ แต่เป็น "บ้าน" ที่ต้องกลับมา
/// </summary>
/// <summary>
/// อาหาร — ข้อมูลโภชนาการจริงของเกมอยู่ใน <see cref="FoodData"/> (352 ชนิด สกัดจาก TextAsset `performance`)
/// แต่ตัวเลขในนั้นเป็น **สเกลของเกมต้นฉบับ** ซึ่งหลอดพลังใหญ่กว่าของเรามาก
/// (เนื้อดิบให้ 63 · ความล้าลด 150 ทั้งที่หลอดเราเต็มแค่ 100)
/// หัวข้อนี้คือตัวคูณที่แปลงสเกลนั้นมาเป็นของเรา — ปรับได้โดยไม่ต้อง build
/// </summary>
public sealed class FoodConfig
{
    /// <summary>สตามินาที่ได้ = ค่าในข้อมูลเกม × ตัวนี้</summary>
    public float EnergyScale { get; set; }

    /// <summary>ความล้าที่ลด = ค่าในข้อมูลเกม × ตัวนี้ (ข้อมูลเกมเก็บเป็นเลขติดลบ เช่น -150)</summary>
    public float FatigueScale { get; set; }

    /// <summary>เลือดที่ฟื้น = ค่าในข้อมูลเกม × ตัวนี้</summary>
    public float HealthScale { get; set; }

    /// <summary>
    /// **ของดิบให้พลังแค่เท่านี้เท่าของสุก** — นี่คือเหตุผลที่ต้องทำอาหาร
    /// (ของที่ติด tag `raw_food` ในข้อมูลเกม: เนื้อ · ปลา · ไข่ดิบ)
    /// </summary>
    public float RawFoodEnergyScale { get; set; }

    /// <summary>กินของดิบแล้วความล้าเพิ่มเท่านี้ (ท้องไม่ดี) — 0 = ปิด</summary>
    public float RawFoodFatigue { get; set; }

    /// <summary>กินแล้วต้องรอกี่เท่าของ digestivetime ในข้อมูลเกมถึงกินชิ้นถัดไปได้</summary>
    public float DigestScale { get; set; }

    public static FoodConfig Defaults()
    {
        return new FoodConfig
        {
            // เนื้อดิบ 63 × 0.5 × 0.6 = ~19 · เนื้อย่างระดับเดียวกัน ~31 ⇒ สุกคุ้มกว่าชัดเจน
            EnergyScale = 0.5f,
            FatigueScale = 0.1f,        // -150 ในข้อมูลเกม = ลดความล้า 15 ของเรา
            HealthScale = 1f,
            RawFoodEnergyScale = 0.6f,
            RawFoodFatigue = 3f,
            DigestScale = 1f
        };
    }
}

public sealed class SurvivalConfig
{
    // ── สตามินา: ทรัพยากรระยะสั้น ────────────────────────────────
    public float StaminaMax { get; set; }
    public float StaminaRegenPerSec { get; set; }

    /// <summary>
    /// ใช้สตามินาแล้วต้องรอกี่วินาทีถึงเริ่มฟื้น
    ///
    /// **สำคัญกับความรู้สึกมาก** — ไม่มีตัวนี้ (ฟื้นทันที 4/วิ) การเก็บของ 1 ครั้งใช้เวลา 2-3 วิ
    /// ซึ่งฟื้นคืน 8-12 แต่หักแค่ 6 ⇒ เก็บรัวแค่ไหนสตามินาก็ไม่มีวันหมด = ระบบไม่มีความหมาย
    /// (วัดจริงด้วย --stamina-check แล้ว: เก็บรัว 25 ครั้งยังเหลือ 88)
    /// ตั้งให้นานกว่าเวลาเก็บของ 1 ครั้ง สตามินาจึงจะลดจริงตอนทำงานต่อเนื่อง
    /// </summary>
    public float StaminaRegenDelaySeconds { get; set; }

    /// <summary>
    /// นั่งพักที่กองไฟ = สตามินาฟื้นเร็วขึ้นเป็นวินาทีละเท่านี้ (แทนอัตราปกติ)
    /// ตั้งใจให้ "ยืนรอเฉย ๆ ฟื้นช้ามาก แต่นั่งพักที่กองไฟฟื้นไว" ⇒ มีเหตุผลให้กลับบ้าน
    /// </summary>
    public float StaminaRegenWhileResting { get; set; }

    public float StaminaCostCollect { get; set; }
    public float StaminaCostCraft { get; set; }
    public float StaminaCostBuild { get; set; }

    // ── ความล้า: ทรัพยากรระยะยาว ─────────────────────────────────
    public float FatigueMax { get; set; }
    /// <summary>ล้าขึ้นเองตามเวลา (ค่าเริ่มต้น = เต็มใน 1 ชั่วโมง)</summary>
    public float FatiguePerSec { get; set; }
    /// <summary>ทำงาน 1 ครั้ง (เก็บของ/คราฟต์/สร้าง) ล้าเพิ่มเท่าไร — อยู่เฉย ๆ กับทำงานต้องต่างกัน</summary>
    public float FatiguePerAction { get; set; }
    /// <summary>ถึงระดับนี้ = ทำอะไรก็เปลืองสตามินา 1.5 เท่า</summary>
    public float FatigueCaution { get; set; }
    /// <summary>ถึงระดับนี้ = เปลือง 2 เท่า และเลือดหยุดฟื้น</summary>
    public float FatigueDanger { get; set; }

    // ── เลือด ────────────────────────────────────────────────────
    /// <summary>เลือดสูงสุด **ที่เลเวล 1** — ของจริงบวกด้วยเลเวลและความอดทน (ดู LifePerLevel)</summary>
    public float LifeMax { get; set; }
    public float LifeRegenPerSec { get; set; }

    // ── หลอดโตตามตัวละคร (beta 1.0) ──────────────────────────────
    // 🐛 เดิมหลอดเป็นค่าคงที่จาก config ล้วน ⇒ **ขึ้นเลเวลแล้วตัวไม่แข็งขึ้นเลย**
    //    (คอมเมนต์ใน ServerPlayer.Progress ยังเขียนว่า "ผูกกับเลเวล" อยู่ทั้งที่ไม่ผูก)
    //    เลเวล 1 กับ 20 เลือดเท่ากันเป๊ะ — เลเวลให้แค่แต้มสกิลอย่างเดียว

    /// <summary>เลือดสูงสุดเพิ่มต่อ 1 เลเวลผู้เล่น</summary>
    public float LifePerLevel { get; set; }
    /// <summary>เลือดสูงสุดเพิ่มต่อ 1 หน่วยความอดทน (Endurance)</summary>
    public float LifePerEndurance { get; set; }
    /// <summary>สตามินาสูงสุดเพิ่มต่อ 1 เลเวลผู้เล่น</summary>
    public float StaminaPerLevel { get; set; }
    /// <summary>สตามินาสูงสุดเพิ่มต่อ 1 หน่วยความมุ่งมั่น (Will)</summary>
    public float StaminaPerWill { get; set; }
    /// <summary>
    /// ล้าเต็มหลอด = เลือดไหลลงวินาทีละเท่านี้ (0 = ปิด ล้าเต็มแล้วก็แค่เปลืองสตามินา)
    /// ค่าเริ่มต้น 0.6 ⇒ จากเลือดเต็มถึงตายใช้เวลา ~2 นาทีครึ่ง — พอให้วิ่งกลับไปกองไฟทัน
    /// </summary>
    public float LifeDrainWhenExhausted { get; set; }

    // ── พักผ่อน ──────────────────────────────────────────────────
    /// <summary>
    /// [เลิกใช้ 3 ก.ย. 2026] พักที่กองไฟแล้วความล้าลดวินาทีละเท่าไร — ตอนนี้ใช้สูตรของเกมจริงแทน
    /// (<see cref="RestFatigueBase"/> + <see cref="RestFatiguePerLevel"/>) เหลือไว้แค่เป็นสวิตช์ปิด/เปิด
    /// </summary>
    public float RestFatiguePerSec { get; set; }
    /// <summary>ต้องอยู่ห่างกองไฟไม่เกินกี่ tile ถึงจะพักได้</summary>
    public float RestRangeTiles { get; set; }

    // ── ค่าที่ถอดจากข้อมูลเกมจริง (3 ก.ย. 2026) ──────────────────────────────────
    // ที่มา: server/data/assets/constants.json และ server/data/assets/survival/status_effects.json
    // (ไฟล์ที่ AssetRipper ถอดจากตัวเกม ไม่ใช่ค่าที่เดาเอง)

    /// <summary>
    /// อัตราความล้าพื้นฐาน — สูตรจริงจาก constants.json → <c>fatigue_velocity</c>:
    /// <c>(0.04 + 0.001 × เลเวลตัวละคร) × max(0.5, 1 − 0.05 × (เลเวลตัวละคร − เลเวลเกาะ))</c>
    /// สองค่านี้คือ 0.04 กับ 0.001
    /// </summary>
    public float FatigueVelocityBase { get; set; }
    public float FatigueVelocityPerLevel { get; set; }

    /// <summary>เลเวลของเกาะ (rl ในสูตร) — เกาะเริ่มต้น = 1 · ยิ่งเลเวลผู้เล่นสูงกว่าเกาะ ยิ่งล้าช้าลง</summary>
    public int RegionLevel { get; set; }

    /// <summary>
    /// ความล้าต่อการกระทำ — สูตรจริงจาก constants.json → <c>fatigue_cost</c>
    /// (e = สตามินาที่การกระทำนั้นใช้): เก็บของ <c>0.4·√e</c> · คราฟต์ <c>2·√e</c> ·
    /// ก่อสร้าง <c>4·√e</c> · ต่อสู้ <c>4·e</c> · อย่างอื่น <c>e</c>
    /// </summary>
    public float FatigueCostCollect { get; set; }
    public float FatigueCostCraft { get; set; }
    public float FatigueCostBuild { get; set; }
    public float FatigueCostCombat { get; set; }

    /// <summary>
    /// พักผ่อน — สูตรจริงจาก status_effects.json → <c>rest</c>:
    /// ความล้า <c>−(0.15 + 0.0015 × level)</c> · เลือด <c>0.45 + 0.05 × level</c>
    /// (level = เลเวลของบัพ ซึ่งอิงคุณภาพที่นอน/กองไฟ — ตอนนี้เราใช้ 1 ทุกจุด)
    /// </summary>
    public float RestFatigueBase { get; set; }
    public float RestFatiguePerLevel { get; set; }
    public float RestLifeBase { get; set; }
    public float RestLifePerLevel { get; set; }

    /// <summary>
    /// กระหายน้ำ (<c>thirsty</c>) — จาก status_effects.json: อยู่ได้ 180 วินาที
    /// เพิ่มความล้า 0.2 ในหมวด <c>default</c> และ <c>arid</c>
    /// ⚠️ ต้นฉบับ **ไม่มีหลอดความหิวของผู้เล่น** — มีแต่ <c>satiety_high</c> (อิ่มเกินกินไม่ลง)
    /// ส่วน <c>Derived.HungryMax/HungryVelocity</c> เป็นของสัตว์เลี้ยง (ดู constants.json → pet/battle)
    /// </summary>
    public float ThirstSeconds { get; set; }
    public float ThirstFatigue { get; set; }

    /// <summary>ดื่มน้ำแล้วได้บัพ <c>drink_water</c> — 180 วินาที ลดความล้าหมวดร้อน 0.3</summary>
    public float DrinkWaterSeconds { get; set; }
    public float DrinkWaterFatigue { get; set; }

    /// <summary>
    /// สตามินาต่อการกระทำ — **สูตรจริงจาก constants.json** (ไม่ใช่ค่าที่เราตั้งเอง)
    ///   จองที่สร้าง <c>1 + (พื้นที่ช่อง × 2)</c> · ลงมือสร้าง <c>1</c> · ทุบ <c>10 + ความทนทาน/2</c>
    /// ค่าเก่าของเราคือ 8 เท่ากันหมดทุกอย่าง ซึ่งแพงกว่าต้นฉบับหลายเท่า
    /// </summary>
    public float BuildSiteEnergyBase { get; set; }
    public float BuildSiteEnergyPerArea { get; set; }
    public float BuildEnergy { get; set; }
    public float DestructEnergyBase { get; set; }
    public float DestructEnergyPerDurability { get; set; }

    /// <summary>
    /// อิ่มจนกินต่อไม่ได้ (<c>satiety_high</c>) — ความอิ่มเต็ม 100 · ลดลงเองวินาทีละเท่านี้
    /// ไม่ใช่หลอดโชว์ผู้เล่น ใช้กันกินรัวอย่างเดียวเหมือนต้นฉบับ
    /// </summary>
    public float SatietyMax { get; set; }
    public float SatietyDecayPerSec { get; set; }

    public static SurvivalConfig Defaults()
    {
        return new SurvivalConfig
        {
            StaminaMax = 100f,
            // ฟื้นเองช้ามาก: จาก 0 ถึงเต็มต้องยืนรอ ~83 วินาที
            // (เดิม 4/วิ = 25 วินาที ซึ่งเร็วเกินจนไม่ต้องพักเลย — เจอตอนเล่นจริง)
            StaminaRegenPerSec = 1.2f,
            StaminaRegenWhileResting = 10f,    // นั่งพักที่กองไฟ = 10 วินาทีเต็ม
            StaminaRegenDelaySeconds = 4f,     // นานกว่าเวลาเก็บของ 1 ครั้ง (~2-3 วิ)
            // ⚠️ ต้นฉบับไม่มีค่า "เก็บของ" ตรง ๆ ในข้อมูลที่ถอดมาได้
            //    แต่การกระทำเล็ก ๆ ทุกตัวใน constants.json อยู่ในช่วง 1-5
            //    (watering 1 · fertilizing 1 · put_water_in_container 2 · sprinkle_water 5)
            //    จึงลดจาก 6 มาที่ 2 ให้อยู่ในวงเดียวกัน
            StaminaCostCollect = 2f,
            // สองตัวล่างเป็นแค่ค่าสำรอง — ของจริงมาจากข้อมูลเกม
            // (คราฟต์ใช้ RecipeMeta.Energy ต่อสูตร · ก่อสร้างใช้สูตร BuildSiteEnergy/BuildEnergy)
            StaminaCostCraft = 5f,
            StaminaCostBuild = 3f,

            FatigueMax = 100f,
            FatiguePerSec = 100f / 3600f,      // เต็มใน 1 ชั่วโมงถ้าอยู่เฉย ๆ
            FatiguePerAction = 0.35f,          // ทำงานรัว ๆ ~285 ครั้งก็เต็ม (เร็วกว่านั่งเฉย ๆ)
            FatigueCaution = 60f,
            FatigueDanger = 85f,

            LifeMax = 100f,
            LifeRegenPerSec = 0.5f,
            LifeDrainWhenExhausted = 0.6f,

            // ตั้งให้ "ผู้เล่นใหม่เหมือนเดิมเป๊ะ" (lv1 ความอดทน ~10 ⇒ เลือด 108)
            // แล้วค่อย ๆ โตเป็น ~170 ที่เพดานเลเวล 20 พร้อมความชำนาญระดับกลาง
            LifePerLevel = 2f,
            LifePerEndurance = 0.8f,
            StaminaPerLevel = 1f,
            StaminaPerWill = 0.5f,

            RestFatiguePerSec = 4f,            // เหลือไว้เป็นสวิตช์เท่านั้น (>0 = พักได้)
            RestRangeTiles = 3f,

            // ── ค่าจากข้อมูลเกมจริง ──
            FatigueVelocityBase = 0.04f,
            FatigueVelocityPerLevel = 0.001f,
            RegionLevel = 1,

            FatigueCostCollect = 0.4f,
            FatigueCostCraft = 2f,
            // [4 ก.ย. 2026] บั๊ก #20 "สร้าง/ทำลายสิ่งปลูกสร้าง ค่าเหนื่อยเพิ่มเยอะเกินไป"
            // สูตร 4·√energy ⇒ ของ energy 100 กินเหนื่อย 40 จาก 100 (40% ของหลอด!) ลดเหลือ 1.2 ⇒ 12
            FatigueCostBuild = 1.2f,
            // 🐛 [แก้ 3 ก.ย. 2026] เดิม 4f ตามสูตรต้นฉบับตรง ๆ แต่สูตรนั้นอยู่บนสเกลความล้าของเกมเดิม
            //    (ใหญ่กว่าหลอด 100 ของเราราว 10 เท่า — ดู FoodConfig.FatigueScale = 0.1)
            //    ท่าโจมตีใช้สตามินา 6-50 ⇒ ล้า +24..200 ต่อครั้ง = **ตีไดโนทีเดียวหลอดแดง** (ผู้เล่นรายงาน)
            //    ลดลง 10 เท่าให้อยู่สเกลเดียวกับอาหาร: ตีธรรมดา (6) ล้า +2.4 · ท่าหนัก (35) +14
            FatigueCostCombat = 0.4f,

            // [3 ก.ย. 2026] เจ้าของสั่งให้พักฟื้นเร็วขึ้น 2 เท่า (เดิม 0.15 / 0.0015 ตามต้นฉบับ = หลอดเต็มใช้ ~11 นาที)
            RestFatigueBase = 0.3f,
            RestFatiguePerLevel = 0.003f,
            RestLifeBase = 0.45f,
            RestLifePerLevel = 0.05f,

            ThirstSeconds = 180f,
            ThirstFatigue = 0.2f,
            DrinkWaterSeconds = 180f,
            DrinkWaterFatigue = -0.3f,

            SatietyMax = 100f,
            SatietyDecayPerSec = 100f / 6000f,  // เต็มถึงหมดใน 100 นาที

            BuildSiteEnergyBase = 1f,
            BuildSiteEnergyPerArea = 2f,
            BuildEnergy = 1f,
            DestructEnergyBase = 10f,
            DestructEnergyPerDurability = 0.5f
        };
    }
}

/// <summary>
/// **ของที่ผู้เล่นใหม่คราฟได้ตั้งแต่วินาทีแรก** — อ้างอิงหัวข้อ 5 ของ `1.0.0 beta.txt`
///
/// ทำไมต้องเขียนเป็นรายการเอง: ข้อมูลเกมไม่มีธง "สูตรเริ่มต้น" ตรง ๆ
///   · กติกา "สูตรที่ไม่มีสกิลไหนปลดล็อก" ให้ผลเป็นขยะ 219 อัน (ของอีเวนต์/ซีซัน 2/โลหะขั้นสูง)
///     และ **ไม่มีของพื้นฐานเลย** — ไม่มีมีดหิน ไม่มีเชือก ไม่มีด้ามจับ
///   · กติกา "สกิลที่ไม่เสียแต้ม (skill_point 0)" ใกล้เคียงกว่ามาก แต่ยังมีของซีซัน 2 ปน
/// ⇒ beta 1.0.0 จึงกำหนดรายการเองตามเอกสาร แล้วให้ที่เหลือปลดล็อกด้วยสกิลตามปกติ
///
/// แก้รายการได้ใน `data/config.json` แล้วเซิร์ฟโหลดใหม่เองใน 5 วินาที
/// </summary>
public sealed class StarterConfig
{
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> Recipes { get; set; }

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> Blueprints { get; set; }

    public static StarterConfig Defaults()
    {
        return new StarterConfig
        {
            Recipes = new List<string>
            {
                // ── ชิ้นส่วนพื้นฐาน (doc: "มีด = ใบมีด + ด้าม + เชือก") ──
                "handle",                    // ด้าม
                "twist_rope",                // เชือก (กก/ราก)
                "twist_rope_02",             // เชือกเส้นใหญ่
                "extend_rope",               // ต่อเชือก
                "extend_stick",              // ต่อไม้
                "cut_pillar",                // ผ่าเสา
                "sheaf",                     // มัดของ

                // ── ใบมีด (doc: "ใบมีดหิน/กระดูก/เหล็ก") — เหล็กเอาไว้ก่อน ──
                "blade_stone",               // ใบมีดหิน
                "blade_big_stone",           // ใบมีดหินใหญ่
                "blade_bone",                // ใบมีดกระดูก
                "blade_big_bone",            // ใบมีดกระดูกใหญ่
                "blade_saw_stone",           // ใบเลื่อยหิน

                // ── อาวุธสายเริ่มต้น (doc: "ไม้กระบอง → ค้อนหิน → ขวานหิน") ──
                "club_onehand_wooden_01",    // ไม้กระบอง
                "assembled_hammer_one_01",   // ค้อนมือเดียว
                "assembled_hammer_two_01",   // ค้อนสองมือ
                "assembled_axe_one_01",      // ขวานมือเดียว
                "assembled_axe_two_01",      // ขวานสองมือ
                "assembled_sword_one_01",    // มีด/ดาบมือเดียว
                "assembled_sword_two_01",    // ดาบสองมือ

                // ── ธนู/ฉมวก/หอก (doc: "ธนู = กิ่ง + เชือก", "เบ็ด/ฉมวก") ──
                "bowstring_01",              // สายธนู
                "bow_wooden_assembled",      // ธนูไม้
                "harpoon_wooden_01",         // ฉมวกไม้
                "handle_lance_01",           // ด้ามหอก

                // ── เครื่องมือ (doc: "จอบ ขวาน มีดทำงาน เบ็ด/ฉมวก") ──
                "hoe_wooden_00",             // จอบหินอย่างง่าย
                "hoe_wooden_01",             // จอบหิน
                "pickaxe_wooden_01",         // อีเต้อหิน

                // ── เสื้อผ้าเริ่มต้น ──
                "clothes_leaf_01",           // เสื้อใบไม้
                "hat_leaf",                  // หมวกใบไม้
                "shoes_footwraps",           // ผ้าพันเท้า
                "shoes_sandal_straw",        // รองเท้าฟาง

                // ── อาหาร (doc: "ย่างไม้เสียบ ต้ม") ──
                "skewer",                    // ย่างไม้เสียบ
                "boil",                      // ต้ม
                "broth",                     // ต้มน้ำซุป
                "boiled_meat"                // เนื้อต้ม
            },
            Blueprints = new List<string>
            {
                // ── ที่อยู่อาศัย/เครื่องมือประจำที่ (doc หัวข้อ "ที่อยู่อาศัย") ──
                "bonfire",                   // กองไฟ — ใช้ทำอาหารพื้นฐาน (cook 15) และพักหายเหนื่อย
                "bonfire_01",                // กองไฟใหญ่ — ทำอาหารระดับถัดไปได้ (cook 40)
                "fur_table",                 // โต๊ะช่างอย่างง่าย (table 15) — ไม่มีตัวนี้ สูตรที่ต้องใช้โต๊ะทำไม่ได้เลย
                "tent",                      // เต็นท์
                "fishtrap",                  // ตาข่ายดักปลา
                "trap_basket",               // กับดักตะกร้า
                "dryingrack_01",             // รางตากแห้ง
                "kiln_01",                   // เตาเผา
                "furnace_01",                // เตาหลอม
                "fur_box_03_leaf",           // กล่องเก็บของ
                "fence1",                    // รั้ว
                "gate1"                      // ประตู
            }
        };
    }
}

/// <summary>ค่าที่เกี่ยวกับการส่งข้อมูลโลกให้ client</summary>
public sealed class WorldConfig
{
    /// <summary>
    /// ส่งข้อมูลต้นไม้/หิน (garden) รอบ chunk ที่ client ขอมา กี่ chunk
    /// 1 = 3×3 (ของเดิม) · 2 = 5×5 · 1 chunk = 16 tile
    ///
    /// **ต้องเท่ากับ `_visibleRange` ของ client** (Durango.Terrain/TerrainBase.InitChunkPool)
    /// น้อยกว่า = chunk วงนอกไม่มีของ แล้วต้องสร้างใหม่ตอนเดินเข้าไปใกล้ = เห็นแมพรีเฟรช
    /// มากกว่า = เปลืองแบนด์วิดท์เปล่า ๆ เพราะ client ทิ้ง chunk ที่อยู่นอกระยะทันที
    ///
    /// client ต้นฉบับ (retail) ใช้ `range = 1` ตายตัว ⇒ ค่าปกติของเราคือ 1
    /// </summary>
    public int ChunkSendRange { get; set; }

    /// <summary>
    /// [regrow] ต้นไม้/หิน/บ่อโคลนที่ถูกเก็บจนหมดหรือทำลาย งอกกลับหลังกี่วินาที (0 = ไม่งอก หายถาวรเหมือนเดิม)
    /// เกมต้นฉบับใช้ eco simulation รายวัน — ของเราตั้ง 20 นาที ให้เกาะเทสไม่โล่ง
    /// </summary>
    public double NaturalRegrowSeconds { get; set; }

    /// <summary>
    /// client **เก็บ** chunk ไว้กี่วงรอบตัว (= `_visibleRange` ของ client จริง ๆ)
    ///
    /// 🐛 ต้นตออาการ "เดินไปแล้วโลกไม่โหลด": ตัวนี้เคยถูกอนุมานจาก `ChunkSendRange`
    /// พอตั้ง ChunkSendRange กว้างกว่าที่ client รับไหว (retail = 1) `ChunkPool.Load()`
    /// จะ **ทิ้ง chunk วงนอกเงียบ ๆ** (ไม่เข้า `_failedChunks` ด้วยซ้ำ) แต่เซิร์ฟจำว่า
    /// "ส่งไปแล้ว" ⇒ พอเดินข้ามขอบ chunk เซิร์ฟข้ามการส่งซ้ำ client จึงไม่เคยได้ก้อนนั้น
    /// และ `IsEnoughChunkLoaded()` (ต้องครบ 9) ไม่ผ่าน ⇒ `IsLoadingChunks` ค้าง true ถาวร
    /// terrain หยุดอัปเดตทั้งระบบ
    ///
    /// จึงแยกออกมาเป็นค่าของตัวเอง — และตั้งแต่ 30 ส.ค. 2026 เซิร์ฟ **ตรวจเองรายคน** จาก ModHello:
    /// client ที่มี `DurangoClientCore` ใช้ `ClientModPolicy.WorldChunkRange` (มอดขยาย pool ให้)
    /// ค่านี้จึงเหลือหน้าที่เดียว = **ระยะของ client เปล่า ๆ ที่ไม่มีมอด** (retail = 1 อย่าแก้)
    /// </summary>
    public int ClientChunkRetainRange { get; set; }

    // ───── ระยะการมองเห็น (interest management) ─────
    //
    // 🐛 เดิมทุกอย่างที่เกิดขึ้นในโลกถูกส่งให้ **ทุกคนในเกาะ** โดยไม่ดูระยะเลย (47 จุดเรียก Broadcast)
    //    คนเดิน 1 ก้าว = ส่งออก N packet ตามจำนวนคนออนไลน์ ⇒ โตแบบ N²
    //    ที่ 100 คนเดินกันคนละ 2 ครั้ง/วินาที = ~20,000 packet/วินาที
    //    และ client ต้องวาด/อัปเดตคนที่อยู่คนละมุมเกาะซึ่งมองไม่เห็นอยู่ดี
    //
    // ตอนนี้ส่งเฉพาะสิ่งที่อยู่ในระยะรอบตัวผู้เล่นแต่ละคน

    /// <summary>
    /// ระยะที่ **เริ่มเห็น** กัน (tile · 1 tile = 200 หน่วยโลก)
    /// ควรกว้างกว่าที่จอแสดงจริงเล็กน้อย เพื่อให้ข้อมูลไปถึงก่อนที่เป้าจะโผล่เข้าจอ
    /// </summary>
    public float ViewRangeTiles { get; set; }

    /// <summary>
    /// เลยระยะเห็นออกไปอีกเท่านี้ถึงจะ "หายไป" (tile)
    ///
    /// ต้องมีช่องว่างตรงนี้ ไม่งั้นคนที่ยืนอยู่พอดีขอบระยะจะ **โผล่-หาย-โผล่-หายรัว ๆ**
    /// ทุกครั้งที่ขยับไปมาไม่กี่ก้าว (ปัญหาคลาสสิกของระบบกรองระยะ)
    /// </summary>
    public float ViewMarginTiles { get; set; }

    /// <summary>ตรวจว่าใครเข้า/ออกระยะทุกกี่วินาที (ถี่ไป = เปลือง CPU · ห่างไป = คนโผล่ช้า)</summary>
    public double ViewCheckSeconds { get; set; }

    /// <summary>ปิดการกรองระยะ = กลับไปส่งให้ทุกคนเหมือนเดิม (ไว้เทียบเวลาสงสัยว่าบั๊กมาจากตรงนี้ไหม)</summary>
    public bool ViewCulling { get; set; }

    /// <summary>
    /// เลเวลของไม้/ผลไม้/หอย/หิน ที่เก็บได้บนเกาะนี้
    /// 0 = ใช้ MinLevel ของเกาะ (หรือ 1 ถ้าเปิดแบบเกาะเดียว)
    /// </summary>
    public int ResourceLevel { get; set; }

    /// <summary>ระยะที่เริ่มเห็น (หน่วยโลก) — คิดจาก ViewRangeTiles ไม่ใช่ค่าที่ตั้งเองได้</summary>
    [JsonIgnore]
    public float ViewEnterUnits => ViewRangeTiles * 200f;

    /// <summary>
    /// ระยะที่หายไป (หน่วยโลก) — ใช้เป็นระยะส่ง packet ด้วย เพราะของที่ยังเห็นอยู่ต้องอัปเดตต่อ
    /// (JsonIgnore เพราะเป็นค่าคำนวณ ถ้าปล่อยให้ลงไฟล์ คนแก้จะนึกว่าแก้ได้แล้วงงว่าทำไมไม่มีผล)
    /// </summary>
    [JsonIgnore]
    public float ViewExitUnits => (ViewRangeTiles + ViewMarginTiles) * 200f;

    public static WorldConfig Defaults()
    {
        return new WorldConfig
        {
            ChunkSendRange = 1,          // client ต้นฉบับ (retail) ใช้ range 1 ตายตัว
            ClientChunkRetainRange = 1,  // TerrainBase.InitChunkPool ของ retail: range = 1, pool 9 ช่อง
            // 24 tile ≈ 1.5 chunk — กว้างกว่าที่จอเห็นพอสมควร แต่ยังอยู่ในพื้นที่ที่ client โหลด terrain ไว้แล้ว
            // (ChunkSendRange 2 = 5×5 chunk ⇒ มี terrain ถึง ~40 tile รอบตัว)
            ViewRangeTiles = 16f,        // ใกล้ระยะกล้องจริง — กว้างกว่านี้ส่งสัตว์เกิน terrain = เรดาร์
            ViewMarginTiles = 4f,
            ViewCheckSeconds = 0.4,
            ViewCulling = true,
            ResourceLevel = 0,
            NaturalRegrowSeconds = 1200.0
        };
    }
}

public sealed class AnimalConfig
{
    /// <summary>เลือดสัตว์ = LifeBase + เลเวล × LifePerLevel</summary>
    public float LifeBase { get; set; }
    public float LifePerLevel { get; set; }
    /// <summary>ดาเมจสัตว์ = DamageBase + เลเวล × DamagePerLevel</summary>
    public float DamageBase { get; set; }
    public float DamagePerLevel { get; set; }

    /// <summary>
    /// [3 ก.ย. 2026] ให้แต่ละชนิดแข็ง/แรงต่างกันตามข้อมูลเกมจริง (`AnimalStatData`)
    ///
    /// เดิมสูตรกลางข้างบนใช้กับทั้ง 213 ชนิด ⇒ ไทรเซอราท็อปส์เลือดเท่ากิ้งก่า ทั้งที่ข้อมูลเกม
    /// (`animal.json`) มีสูตรรายชนิด: เลือดต่างกัน 96 แบบ ดาเมจ 86 แบบ
    ///
    /// วิธีใช้: **ไม่เอาตัวเลขดิบของเกม** (เลือดแร็ปเตอร์ lv1 ตามเกม = 264 แต่ config ให้ 38 —
    /// ต่างกัน 7 เท่า เอามาตรง ๆ จะกลับไปเป็นอาการ "ตี 25 ครั้ง" ที่เคยแก้ไปแล้ว)
    /// แต่ใช้เป็น *อัตราส่วนเทียบกับสัตว์อ้างอิง* คูณสูตรกลาง:
    ///     เลือด = (LifeBase + lv×LifePerLevel) × life_ชนิดนี้(lv) / life_อ้างอิง(lv)
    /// สมดุลของตัวอ้างอิงเท่าเดิมเป๊ะ ตัวอื่นแข็ง/อ่อนกว่าตามสัดส่วนของเกม
    /// </summary>
    public bool SpeciesStats { get; set; }

    /// <summary>สัตว์อ้างอิงที่อัตราส่วน = 1.0 (ค่าเริ่มต้น 2001 แร็ปเตอร์ — อยู่ในตารางเกิดของเกาะเริ่มต้น)</summary>
    public ushort SpeciesReference { get; set; }

    /// <summary>ซากอยู่ในโลกกี่วินาทีก่อนหาย (ต้องนานพอให้แล่จนครบ)</summary>
    public double CorpseSeconds { get; set; }
    /// <summary>ตายแล้วกี่วินาทีถึงเกิดตัวใหม่แทน</summary>
    public double RespawnSeconds { get; set; }

    /// <summary>สัตว์แต่ละตัวอยู่ในโลกได้กี่วินาที แล้ว despawn + เกิดใหม่ที่จุดว่าง (ไม่ทับหิน)</summary>
    public double LifetimeSeconds { get; set; }

    public float ChaseSpeed { get; set; }
    public float FleeSpeed { get; set; }
    /// <summary>ตัวนิสัยดุเริ่มไล่เมื่อเห็นคนในระยะกี่ tile</summary>
    public float SightTiles { get; set; }
    /// <summary>ไล่ห่างเกินกี่ tile แล้วเลิกสนใจ</summary>
    public float GiveUpTiles { get; set; }
    /// <summary>โกรธนานกี่วินาที</summary>
    public double AggroSeconds { get; set; }
    /// <summary>โดนตีแล้วกี่วินาทีถึงสวนกลับครั้งแรก</summary>
    public double FirstAttackDelay { get; set; }
    /// <summary>ความเร็วเดินปกติ (ตอนไม่ได้ไล่/หนี)</summary>
    public float WalkSpeed { get; set; }
    /// <summary>ยืนพักหลังเดินถึงที่หมายกี่วินาที (สุ่มระหว่างสองค่านี้)</summary>
    public double RestMinSeconds { get; set; }
    public double RestMaxSeconds { get; set; }
    /// <summary>
    /// เดินทีละไม่เกินกี่วินาทีต่อ 1 คำสั่ง — เดินยาว ๆ ทีเดียวดูเป็นหุ่นยนต์
    /// และถ้ามีอะไรมาขัดกลางทาง (โดนตี) ผลกระทบก็ยิ่งใหญ่
    /// </summary>
    public double MaxWalkLegSeconds { get; set; }

    /// <summary>
    /// สัตว์ต้องเกิดลึกเข้าไปในแผ่นดินอย่างน้อยกี่ tile (0 = เกิดริมหาดได้)
    /// อ่านจาก `oceans.dm` ของ terrain — ไดโนเสาร์ยืนบนหาด/กลางทะเลดูพัง
    /// </summary>
    public int MinTilesInland { get; set; }

    /// <summary>
    /// ตัวใหญ่ต้องอยู่ลึกเข้าไปอีกกี่ tile ต่อ 1 ระดับขนาด
    /// ระยะที่ต้องการ = MinTilesInland + (size_level − 1) × ค่านี้
    /// (size_level 1 = กิ้งก่า · 2 = แร็ปเตอร์ · 4 = สเตโก/ทริเซรา · 7 = ไทแรนโนซอรัส)
    ///
    /// เหตุผล: ไดโนเสาร์ตัวเท่าบ้านยืนอยู่ริมหาดที่ผู้เล่นเพิ่งลงเรือมาดูพัง
    /// และทำให้ "ยิ่งเดินเข้าไปกลางเกาะยิ่งเจอตัวใหญ่" เป็นกติกาที่ผู้เล่นเรียนรู้ได้เอง
    /// </summary>
    public int InlandTilesPerSize { get; set; }

    /// <summary>รัศมีกระจายจุดเกิด (tile)</summary>
    public float SpawnRadiusTiles { get; set; }
    /// <summary>เดินออกจากบ้านตัวเองได้ไกลสุดกี่ tile</summary>
    public float WanderRadiusTiles { get; set; }

    /// <summary>
    /// สัตว์สองตัวต้องเกิดห่างกันอย่างน้อยกี่ tile — ป้องกันไดโนเสาร์จับกลุ่มกันเป็นแพ
    /// (เกิดจากทุกตัว RandomAround จุดเดียวกัน + ตัวเล็กๆ เดินวนใกล้บ้าน)
    /// </summary>
    public float MinSeparationTiles { get; set; }

    /// <summary>
    /// จุดหมายการเดินต้องห่างจากขอบสิ่งปลูกสร้างกี่ tile
    ///
    /// 🐛 [2 ก.ย. 2026] เดิม hardcode ไว้ 6 tile — สิ่งปลูกสร้าง 1 ชิ้นกินพื้นที่ห้ามเดิน ~13×13 tile
    /// รอบจุดเกิดมี 31 ชิ้น ⇒ **56% ของจุดหมายที่สุ่มได้ถูกปฏิเสธเพราะข้อนี้ข้อเดียว**
    /// รวมกับข้ออื่นแล้วสัตว์เดินได้จริงแค่ ~2-5 ครั้งต่อ 30 วินาทีทั้งเกาะ = ยืนแข็งกันหมด
    /// </summary>
    public float ArtifactAvoidTiles { get; set; }

    /// <summary>
    /// จุดหมายการเดินต้องไม่มีของธรรมชาติ (ต้นไม้/พุ่ม/หิน) ในรัศมีกี่ tile
    /// 0 = ห้ามแค่ช่องที่จะไปยืนเอง · 1 = 3×3 · 2 = 5×5
    ///
    /// 🐛 เดิม hardcode 2 (5×5 ต้องโล่งสนิท) — บนแมพป่าทึบแทบไม่มีจุดไหนผ่าน (33% ของที่ตก)
    /// </summary>
    public int NaturalAvoidRadius { get; set; }

    /// <summary>
    /// เว้นระยะรอบก้อนหิน/หน้าผาอีกกี่ tile (0 = ห้ามเฉพาะ tile ที่เป็นเนื้อหิน)
    ///
    /// 🐛 [2 ก.ย. 2026] อาการ "สัตว์เกิดในหิน" — ก้อนหินใหญ่ไม่ได้อยู่ใน `whole.garden`
    /// (นั่นคือต้นไม้/หินเล็กที่เก็บได้) แต่อยู่ใน `cliffs.dm` + ธง 0xC0 ของ `whole.biomes`
    /// ซึ่งเซิร์ฟ **ไม่เคยอ่านทั้งคู่** ⇒ ด่านกรองจุดเกิดมองไม่เห็นหินเลย
    /// เกาะจริงมีหิน 5.8% (ri35te) ถึง 7.8% (ri40tr) ของพื้นที่
    ///
    /// ตั้ง 0 พอสำหรับกันเกิดทับ · ตั้ง 1 ถ้าอยากให้เว้นขอบหินด้วย (ตัวใหญ่จะได้ไม่จมขอบ)
    /// </summary>
    public int CliffAvoidTiles { get; set; }

    /// <summary>
    /// สุ่มจุดหมายกี่จุดต่อ 1 tick ก่อนยอมแพ้แล้วรอ 1 วินาที
    /// เดิมสุ่มจุดเดียว — พลาดทีก็ยืนรอวินาทีเต็ม ๆ ทั้งที่จุดถัดไปอาจผ่าน
    /// </summary>
    public int WanderTriesPerTick { get; set; }

    /// <summary>
    /// [3 ก.ย. 2026 · TodoList/08] ระบบฝูงตาม region template ของเกม — ดู HerdConfig
    /// เป็น object แยกเพื่อให้ config เก่าที่ไม่มีหัวข้อนี้ถูก FillMissing เติมค่าเริ่มต้นให้เอง
    /// </summary>
    public HerdConfig Herds { get; set; }

    /// <summary>[TodoList/05] ใช้ค่า defense รายชนิดของสัตว์ (AnimalStatData) ลดดาเมจที่ผู้เล่นตี — ดู AnimalDefenseConfig</summary>
    public AnimalDefenseConfig Defense { get; set; }

    public static AnimalConfig Defaults()
    {
        return new AnimalConfig
        {
            Herds = HerdConfig.Defaults(),
            Defense = AnimalDefenseConfig.Defaults(),
            LifeBase = 30f,
            LifePerLevel = 8f,
            DamageBase = 2f,
            DamagePerLevel = 0.4f,
            CorpseSeconds = 150.0,
            RespawnSeconds = 60.0,
            LifetimeSeconds = 300.0,
            ChaseSpeed = 300f,
            FleeSpeed = 280f,
            SightTiles = 6f,
            GiveUpTiles = 20f,
            AggroSeconds = 20.0,
            FirstAttackDelay = 0.5,
            WalkSpeed = 120f,
            RestMinSeconds = 4.0,
            RestMaxSeconds = 11.0,
            MaxWalkLegSeconds = 5.0,
            MinTilesInland = 4,
            InlandTilesPerSize = 3,
            SpawnRadiusTiles = 30f,
            WanderRadiusTiles = 12.5f,
            MinSeparationTiles = 4f,
            ArtifactAvoidTiles = 1.5f,
            NaturalAvoidRadius = 0,
            CliffAvoidTiles = 0,
            SpeciesStats = true,
            SpeciesReference = 2001,
            WanderTriesPerTick = 8
        };
    }
}

/// <summary>
/// [TodoList/08] ฝูงสัตว์ตาม "ใบสั่ง" ของเกมต้นฉบับ (`RegionTemplateData` สกัดจาก region_templates.json)
///
/// ต้นฉบับ: เกาะ ri35te = 54 ฝูง ~990 ตัว 7 ชนิด · ฝูงเกิดที่จุดจาก herds.yml เดินด้วยกัน อยู่ถาวร
/// ของเราเดิม: 34 ตัว 10 ชนิด เกิดทีละตัวห่างกัน หายเองทุก 5 นาที
///
/// ปิด (`Enabled=false`) หรือเกาะไม่มี template (แมพจาก map_generator) = ใช้ตาราง `Spawn` แบบเดิมทั้งหมด
/// </summary>
/// <summary>
/// [TodoList/05] เกราะของสัตว์ตามข้อมูลเกม (animal.json → defense รายชนิด สกัดไว้ใน AnimalStatData)
///   153 ชนิด = level×5 · 44 ชนิด = 150 + level×5 (กลุ่มเกราะหนา) · 12 = 75 + level×5 · 4 = level×2.5 · 1 = 187.5 + level×7
/// ใช้สูตรลดดาเมจเดียวกับเกราะผู้เล่น: 1 − def/(def + Combat.ArmorDefenseK) ตัดที่ ArmorMaxReduce
/// (K=120: แร็ปเตอร์ Lv.1 def 5 → ลด 4% · Lv.10 def 50 → 29% · กลุ่มหนา Lv.1 def 155 → 56%)
/// critical ของสัตว์ = 0.0 ทั้ง 214 ชนิดในข้อมูลเกม — ไม่มีอะไรให้ทำ
/// </summary>
public sealed class AnimalDefenseConfig
{
    public bool Enabled { get; set; }
    /// <summary>คูณค่า defense ของเกมก่อนเข้าสูตร (1 = ตามเกม)</summary>
    public float Scale { get; set; }

    public static AnimalDefenseConfig Defaults()
    {
        return new AnimalDefenseConfig { Enabled = true, Scale = 1f };
    }
}

public sealed class HerdConfig
{
    public bool Enabled { get; set; }

    /// <summary>ชื่อ template ใน region_templates.json · ว่าง = หาเองจากชื่อ terrain (เวอร์ชันวันที่ล่าสุด)</summary>
    public string Template { get; set; }

    /// <summary>คูณจำนวนฝูง (1.0 = ตามเกม) — ri35te ตามเกม 54 ฝูง</summary>
    public float CountScale { get; set; }
    /// <summary>คูณจำนวนตัวต่อฝูง (1.0 = ตามเกม 20 ตัว)</summary>
    public float SizeScale { get; set; }
    /// <summary>เพดานจำนวนฝูงทั้งเกาะ (0 = ไม่จำกัด) — วางแบบ round-robin ทุกชนิดได้อย่างน้อย 1 ฝูงก่อน</summary>
    public int MaxHerds { get; set; }
    /// <summary>[Android] เกิดเฉพาะชนิดที่มีโมเดลใน bundle ชุด Android (มีผลเมื่อรัน --assetbundles-android) — เปิดเมื่อมีผู้เล่นมือถือ</summary>
    public bool AndroidSafeOnly { get; set; }

    /// <summary>สมาชิกเกิดกระจุกรอบบ้านฝูงในรัศมีกี่ tile</summary>
    public float RadiusTiles { get; set; }
    /// <summary>บ้านฝูงต้องห่างกันอย่างน้อยกี่ tile</summary>
    public float SeparationTiles { get; set; }

    /// <summary>สมาชิกเดินสุ่มรอบบ้านฝูงในรัศมีกี่ tile (สั้นกว่า WanderRadiusTiles ของตัวเดี่ยว ฝูงจะได้ไม่กระจาย)</summary>
    public float WanderTiles { get; set; }

    /// <summary>บ้านฝูงขยับไปไม่เกินกี่ tile จากจุดตั้งเดิม (สมาชิกเดินตามเอง)</summary>
    public float DriftTiles { get; set; }
    public double DriftMinSeconds { get; set; }
    public double DriftMaxSeconds { get; set; }

    /// <summary>
    /// เลเวลสัตว์ในฝูง (สุ่มในช่วง แล้ว clamp ด้วย combat_level_ranges ของชนิดนั้น)
    /// ต้นฉบับ ri35te = เกาะ Lv.35 · เกาะเทสตั้ง 1-4 ไว้ก่อนจนกว่าจะมีของตามเลเวล (TodoList/02-05)
    /// 0 ทั้งคู่ = ใช้ level ของ template ตรง ๆ
    /// </summary>
    public int LevelMin { get; set; }
    public int LevelMax { get; set; }

    /// <summary>บ้านฝูงต้องห่างจุดเข้าเกาะอย่างน้อยกี่ tile · ตัวดุ (Carnivore) ใช้ค่าหลัง</summary>
    public float MinTilesFromEntry { get; set; }
    public float CarnivoreMinTilesFromEntry { get; set; }

    public static HerdConfig Defaults()
    {
        return new HerdConfig
        {
            Enabled = true,
            Template = "",
            CountScale = 0.25f,      // เจ้าของสั่ง 3 ก.ย.: ฝูงละไม่เกิน 8 · ทั้งเกาะ ~100 ตัว (ตามเกม 54 ฝูง ~990)
            SizeScale = 0.4f,
            MaxHerds = 8,            // เจ้าของสั่ง 3 ก.ย.: 8 ฝูงพอ
            RadiusTiles = 3f,
            SeparationTiles = 8f,
            WanderTiles = 5f,
            DriftTiles = 10f,
            DriftMinSeconds = 60.0,
            DriftMaxSeconds = 180.0,
            LevelMin = 1,
            LevelMax = 4,
            MinTilesFromEntry = 6f,
            CarnivoreMinTilesFromEntry = 15f
        };
    }
}

/// <summary>
/// [TodoList/03] ความทนทานหักตามชนิดงาน — ค่าจริงจาก data/assets/constants.json → durability.deltas
///   craft 3.2 · collect 1.6 · build 3.2 · attack 0.0768 · defense 0.064 · taming 10
/// สัดส่วนสำคัญ: ตีมอน 1 ที = 1/40 ของคราฟต์ 1 ครั้ง (เดิมของเราหักเท่ากันทุกงาน)
///
/// ข้อมูลเกมไม่บอก max พื้นฐานต่อ prototype (อนุมาน ~100 จาก tag durable_incr = +10/เลเวล)
/// ของเรา max = 40/60/80 ตาม tier ⇒ ScaleToMax=true คูณ delta ด้วย max/100 ให้สัดส่วนเท่าเกมโดยไม่เปลี่ยนสมดุลที่จูนไว้
/// </summary>
public sealed class WearDeltaConfig
{
    public bool Enabled { get; set; }
    public float Craft { get; set; }
    public float Collect { get; set; }
    public float Build { get; set; }
    public float Attack { get; set; }
    public float Defense { get; set; }
    public float Taming { get; set; }
    /// <summary>คูณ delta ด้วย (max ของชิ้นนั้น / ReferenceMax) — เกมคิดที่ max ~100 ของเราต่ำกว่า</summary>
    public bool ScaleToMax { get; set; }
    public float ReferenceMax { get; set; }
    /// <summary>ซ่อมแต่ละครั้ง max ลดลงสุ่มในช่วงนี้ (เกม repair_damage_range 0.05-0.13)</summary>
    public float RepairDamageMin { get; set; }
    public float RepairDamageMax { get; set; }

    public static WearDeltaConfig Defaults()
    {
        return new WearDeltaConfig
        {
            Enabled = true,
            Craft = 3.2f, Collect = 1.6f, Build = 3.2f,
            Attack = 0.0768f, Defense = 0.064f, Taming = 10f,
            ScaleToMax = true, ReferenceMax = 100f,
            RepairDamageMin = 0.05f, RepairDamageMax = 0.13f,
        };
    }
}

public sealed class ExpConfig
{
    public int KillBase { get; set; }
    public int KillPerLevel { get; set; }
    public int Gather { get; set; }
    public int Butchery { get; set; }
    public int Craft { get; set; }
    public int Build { get; set; }
    /// <summary>exp ตอนลงเมล็ด</summary>
    public int Plant { get; set; }
    /// <summary>exp ต่อผลผลิต 1 ชิ้นที่เก็บได้</summary>
    public int Harvest { get; set; }
    public int SkillPointsPerLevel { get; set; }

    public static ExpConfig Defaults()
    {
        return new ExpConfig
        {
            KillBase = 4,
            KillPerLevel = 3,
            Gather = 2,
            Butchery = 3,
            Craft = 5,
            Build = 8,
            Plant = 3,
            Harvest = 4,
            SkillPointsPerLevel = 3
        };
    }
}

public sealed class SkillConfig
{
    /// <summary>รวมเลเวลสกิลในหมวดหนึ่งถึงเท่านี้ = ได้โบนัสเต็มเพดาน</summary>
    public int FullAt { get; set; }

    /// <summary>เก็บของเร็วขึ้นสูงสุดกี่ % (0.4 = เร็วขึ้น 40% เมื่อสกิลเต็ม)</summary>
    public float GatherSpeed { get; set; }
    /// <summary>โอกาสได้ของเพิ่มอีก 1 ชิ้น สูงสุดกี่ %</summary>
    public float GatherBonus { get; set; }
    /// <summary>แล่ซากเร็วขึ้น / ได้ชิ้นส่วนเพิ่ม สูงสุดกี่ %</summary>
    public float ButcherySpeed { get; set; }
    public float ButcheryBonus { get; set; }
    /// <summary>ดาเมจที่ตีออกเพิ่มสูงสุดกี่ %</summary>
    public float MeleeDamage { get; set; }
    /// <summary>ดาเมจที่รับลดลงสูงสุดกี่ %</summary>
    public float DefenseReduce { get; set; }
    /// <summary>คราฟต์เร็วขึ้นสูงสุดกี่ %</summary>
    public float CraftSpeed { get; set; }
    /// <summary>ประหยัดสตามินาสูงสุดกี่ %</summary>
    public float StaminaSave { get; set; }

    /// <summary>เรียนสกิลเลเวล N ต้องมีเลเวลผู้เล่นอย่างน้อยเท่าไร (คูณกับเลเวลสกิล)</summary>
    public float RequiredPlayerLevelPerSkillLevel { get; set; }

    /// <summary>
    /// เปิดระบบ "ความชำนาญ" — เลเวลของหมวดสกิลที่ขึ้นเองจากการทำงานซ้ำ ๆ
    /// (คนละอย่างกับสกิลย่อยที่ต้องใช้แต้มไปกดเรียน) ดู ServerPlayer.Proficiency
    /// </summary>
    public bool ProficiencyEnabled { get; set; }

    /// <summary>
    /// ทำสำเร็จ 1 ครั้งได้ความชำนาญกี่หน่วย — 1.0 = ตามสเกลของเกมจริง
    /// (เก็บของ ~15 ครั้งขึ้นหมวดเก็บของ 5 เลเวลแรก · ยิ่งเลเวลสูงยิ่งใช้เยอะขึ้นเรื่อย ๆ)
    /// เพิ่มค่าถ้าอยากให้ไต่เร็วขึ้นในรอบเทส
    /// </summary>
    public float ProficiencyRate { get; set; }

    public static SkillConfig Defaults()
    {
        return new SkillConfig
        {
            FullAt = 60,
            GatherSpeed = 0.4f,
            GatherBonus = 0.3f,
            ButcherySpeed = 0.4f,
            ButcheryBonus = 0.3f,
            MeleeDamage = 0.5f,
            DefenseReduce = 0.3f,
            CraftSpeed = 0.4f,
            StaminaSave = 0.3f,
            RequiredPlayerLevelPerSkillLevel = 1.0f,
            ProficiencyEnabled = true,
            ProficiencyRate = 1.0f
        };
    }
}

/// <summary>
/// **บัฟ/ดีบัฟจากอาหาร-ยา (status effect) ให้มีผลจริง** — เดิมกินแล้วขึ้นแค่ไอคอน ไม่กระทบตัวเลข
///
/// ข้อมูลเกมจริงมี 18 บัฟ (สกัดจาก FoodData.cs) จับกลุ่มเป็น 4 กลไกที่ "มีของจริงให้กระทบ":
///   · บัฟสตามินา (energetic/stamina_up/drink_water ฯลฯ) → ทำอะไรก็เปลืองสตามินาน้อยลง
///   · ดีบัฟสตามินา (thirsty/eat_bizarre_food/drunk) → เปลืองสตามินามากขึ้น
///   · life_up → เลือดฟื้นเองต่อเนื่อง (heal over time)
///   · poisoning → เลือดไหลลงต่อเนื่อง (damage over time)
///
/// ค่าทั้งหมด "เกรดอ่อน ๆ" ตามที่เจ้าของสั่ง — เห็นผลจริงแต่ไม่โหด
/// (หมายเหตุ: เดิมวางแผนให้ drunk ลดความแม่นยำ แต่โค้ดต่อสู้ไม่มีระบบพลาด/เข้า — ตีเข้าเสมอ —
///  ความแม่นยำเป็นแค่ตัวเลขโชว์ ไม่มีผลจริง จึงเปลี่ยน drunk มาเป็นดีบัฟสตามินาแทน)
/// </summary>
public sealed class StatusEffectConfig
{
    /// <summary>บัฟสตามินาติดอยู่ = ทุกอย่างเปลืองสตามินาน้อยลงกี่ % (0.10 = ถูกลง 10%)</summary>
    public float BuffStaminaSave { get; set; }

    /// <summary>ดีบัฟสตามินาติดอยู่ = ทุกอย่างเปลืองสตามินามากขึ้นกี่ % (0.08 = แพงขึ้น 8%)</summary>
    public float DebuffStaminaPenalty { get; set; }

    /// <summary>life_up ติดอยู่ = เลือดฟื้นเองเพิ่มวินาทีละเท่าไร (บวกจากอัตราฟื้นปกติ)</summary>
    public float LifeUpRegenPerSec { get; set; }

    /// <summary>poisoning ติดอยู่ = เลือดไหลลงวินาทีละเท่าไร</summary>
    public float PoisonDamagePerSec { get; set; }

    /// <summary>กินของดิบ (tag raw_food) แล้วปวดท้องกี่วินาที — 0 = ปิด</summary>
    public float StomachacheSeconds { get; set; }

    /// <summary>ปวดท้องจากของดิบ = เลือดไหลวินาทีละเท่าไร (อ่อนกว่า poisoning)</summary>
    public float StomachacheDamagePerSec { get; set; }

    public static StatusEffectConfig Defaults()
    {
        return new StatusEffectConfig
        {
            BuffStaminaSave = 0.10f,
            DebuffStaminaPenalty = 0.08f,
            LifeUpRegenPerSec = 1.0f,
            PoisonDamagePerSec = 1.0f,
            // [4 ก.ย. 2026] เจ้าของแจ้ง "ปวดท้อง 4 นาที 55 วิ นานเกินไปมากกก" (บั๊ก #15)
            // 300 วิ = 5 นาที · ลดเหลือ 60 วิ (พอให้รู้สึกว่ากินของดิบมีโทษ แต่ไม่ทรมาน)
            StomachacheSeconds = 60f,
            StomachacheDamagePerSec = 0.7f
        };
    }
}

/// <summary>
/// ระบบป่วย (4 ก.ย. 2026 — เจ้าของเซิร์ฟสั่งเพิ่ม)
/// ป่วยแล้ว: คราฟต์ใช้เวลานานขึ้น · เปลืองสตามินามากขึ้น · ล้าไวขึ้น · เดินช้าลง
/// สถานะโชว์ผ่าน id `poison_heat` (열독) เพราะเป็น id ที่ client มีชื่อ+ไอคอนอยู่แล้ว
/// (ตาราง ItemNameData) — id ที่ไม่มีในตารางนั้นผู้เล่นจะไม่เห็นไอคอนอะไรเลย
/// </summary>
public sealed class SicknessConfig
{
    /// <summary>เปิดระบบป่วยไหม</summary>
    public bool Enabled { get; set; }

    /// <summary>ป่วยครั้งหนึ่งนานกี่วินาที</summary>
    public float DurationSeconds { get; set; }

    /// <summary>เวลาคราฟต์ × ค่านี้ (1.8 = ช้าลงเกือบเท่าตัว)</summary>
    public float CraftDurationScale { get; set; }

    /// <summary>สตามินาที่เสียต่อการกระทำ × ค่านี้</summary>
    public float StaminaCostScale { get; set; }

    /// <summary>ความล้าเพิ่มขึ้นต่อวินาที (บวกกับอัตราปกติ)</summary>
    public float FatiguePerSec { get; set; }

    /// <summary>ความเร็วเดิน × ค่านี้ (0.65 = เหลือ 65%)</summary>
    public float MoveSpeedScale { get; set; }

    /// <summary>ความเร็วเดินปกติของผู้เล่น (client/PlayerController.DefaultMoveSpeed)</summary>
    public float BaseMoveSpeed { get; set; }

    /// <summary>เปียกอยู่ในเขตหนาวติดต่อกันกี่วินาทีถึงจะป่วย (0 = ปิดสาเหตุนี้)</summary>
    public float ColdWetSeconds { get; set; }

    /// <summary>กินของดิบจนปวดท้องซ้ำกี่ครั้งถึงจะป่วย (0 = ปิดสาเหตุนี้)</summary>
    public int StomachacheStacksToSick { get; set; }

    public static SicknessConfig Defaults()
    {
        return new SicknessConfig
        {
            Enabled = true,
            DurationSeconds = 300f,
            CraftDurationScale = 1.8f,
            StaminaCostScale = 1.5f,
            FatiguePerSec = 0.15f,
            MoveSpeedScale = 0.65f,
            BaseMoveSpeed = 500f,
            ColdWetSeconds = 90f,
            StomachacheStacksToSick = 3
        };
    }
}

/// <summary>
/// การเซฟ (4 ก.ย. 2026 — เจ้าของเซิร์ฟสั่งให้ตั้งค่าได้ + มีแบ็กอัพตามรอบ)
/// เดิม autosave ทุก 60 วิ เป็นค่าคงที่ในโค้ด แก้ไม่ได้ และไม่มีแบ็กอัพอัตโนมัติเลย
/// </summary>
public sealed class SaveConfig
{
    /// <summary>เซฟอัตโนมัติทุกกี่วินาที (0 = ปิด เซฟเฉพาะตอนปิดเซิร์ฟ/ผู้เล่นออก)</summary>
    public int AutoSaveSeconds { get; set; }

    /// <summary>เปิดแบ็กอัพอัตโนมัติไหม</summary>
    public bool BackupEnabled { get; set; }

    /// <summary>แบ็กอัพทุกกี่ชั่วโมง</summary>
    public float BackupIntervalHours { get; set; }

    /// <summary>เก็บแบ็กอัพย้อนหลังกี่ชุด (เกินกว่านี้ลบตัวเก่าสุดทิ้ง · 0 = เก็บหมดไม่ลบ)</summary>
    public int BackupKeep { get; set; }

    /// <summary>โฟลเดอร์เก็บแบ็กอัพ (ว่าง = ใช้ &lt;saves&gt;/backups)</summary>
    public string BackupDir { get; set; }

    /// <summary>แบ็กอัพทันทีตอนเซิร์ฟเพิ่งเปิดด้วยไหม (กันเคสแก้พังแล้วไม่มีจุดย้อนกลับ)</summary>
    public bool BackupOnStartup { get; set; }

    public static SaveConfig Defaults()
    {
        return new SaveConfig
        {
            AutoSaveSeconds = 60,
            BackupEnabled = true,
            BackupIntervalHours = 4f,
            BackupKeep = 12,          // 4 ชม. × 12 = ย้อนได้ 2 วัน
            BackupDir = "",
            BackupOnStartup = true
        };
    }
}

/// <summary>
/// ประกาศค้าง (4 ก.ย. 2026 — เจ้าของเซิร์ฟสั่ง "ประกาศค้างไว้จนเซิร์ฟปิด คนจะได้ไม่งง")
///
/// /admin/broadcast เดิมเห็นเฉพาะคนที่ออนไลน์อยู่ตอนนั้น และอยู่ได้สูงสุด 120 วิ
/// อันนี้เก็บไว้ใน config ⇒ ใครเข้าเกมทีหลังก็เห็น จนกว่าจะลบข้อความออก (แก้ config มีผลทันที)
/// </summary>
public sealed class AnnouncementConfig
{
    /// <summary>ข้อความที่จะโชว์ตอนเข้าเกม (ว่าง = ไม่มีประกาศ)</summary>
    public string Text { get; set; }

    /// <summary>โชว์ซ้ำทุกกี่วินาทีระหว่างเล่น (0 = โชว์แค่ตอนเข้าเกมครั้งเดียว)</summary>
    public float RepeatSeconds { get; set; }

    public static AnnouncementConfig Defaults()
    {
        return new AnnouncementConfig { Text = "", RepeatSeconds = 0f };
    }
}

/// <summary>
/// **ซ่อน/โชว์แท็บในเมนู "สร้าง" ได้จาก config โดยไม่ต้อง build ใหม่**
///
/// เจ้าของสั่ง (จากรูปวงกลมแท็บ): ซ่อนบางแท็บจากผู้เล่นทั่วไป เหลือแค่ admin — ใส่ "หมวด" ที่อยากซ่อนลงใน
/// <see cref="HiddenCategories"/> (ชื่อหมวดจริงจากข้อมูลเกม ดู RecipeData.BlueprintsByCategory) เอาออก
/// จากลิสต์ = แท็บกลับมาโชว์ · แก้ไฟล์แล้ว hot-reload มีผลทันที (ไม่ต้อง build)
///
/// กลไก: server ไม่ส่ง blueprint "ฟรี" ในหมวดที่ซ่อนเข้า unlocked list ของ non-admin → แท็บฝั่ง client
/// หายเอง (client โชว์แท็บเฉพาะหมวดที่มีของ Available ≥1) · **ของที่ปลดล็อกด้วยสกิลไม่โดนซ่อน** (เป็น
/// progression จริง เช่น เตา/โต๊ะ/เตียง/กับดัก — ดู RecipeUnlockData.SkillGatedBlueprints) · admin ได้ครบ
/// </summary>
/// <summary>
/// [TodoList/02,04,06] กติกาคราฟต์/เก็บของ/สร้าง ให้ตรงต้นฉบับ — ค่าจริงจาก data/assets/constants.json
/// ทุกสวิตช์ปิดได้ = กลับพฤติกรรมเดิม (Lv.1 เสมอ / เวลาตายตัว / สำเร็จ 100%)
/// </summary>
public sealed class CraftingConfig
{
    // ── 02 เลเวลผลลัพธ์ ──────────────────────────────────────────────
    /// <summary>ผลลัพธ์คราฟต์ได้เลเวลจากค่าเฉลี่ยถ่วงน้ำหนักของวัสดุ (recipes.json slot.weight) clamp ด้วย min/max_level</summary>
    public bool MaterialLevel { get; set; }

    // ── 04 เวลาทำงาน (constants.effort_standard · duration_formula = "e") ──
    /// <summary>ใช้สูตร effort ตามเลเวลของ "สิ่งที่ทำ" แทนตัวเลขตายตัว</summary>
    public bool EffortFormula { get; set; }
    /// <summary>collect: 2.5 + (level-1)×0.25</summary>
    public float CollectBase { get; set; }
    public float CollectPerLevel { get; set; }
    /// <summary>craft: 5 + (level-1)×0.5 — ใช้เมื่อสูตรไม่ระบุ duration</summary>
    public float CraftBase { get; set; }
    public float CraftPerLevel { get; set; }
    /// <summary>build: 10 + (level-1)×1 — ใช้เมื่อ blueprint ไม่ระบุ effort</summary>
    public float BuildBase { get; set; }
    public float BuildPerLevel { get; set; }

    // ── 06 ผลคราฟต์ (constants.item.default_improvement / success_probability) ──
    /// <summary>เปิดโอกาส "สำเร็จมาก" (เลเวล/ความทนเพิ่ม)</summary>
    public bool GreatSuccess { get; set; }
    /// <summary>โอกาสสำเร็จมากพื้นฐาน (เกม 0.05) — คูณด้วย สกิล/ความยาก แล้ว clamp 0.01-0.3</summary>
    public float GreatBase { get; set; }
    /// <summary>สำเร็จมากได้เลเวลเพิ่มกี่ระดับ</summary>
    public int GreatLevelBonus { get; set; }
    /// <summary>สำเร็จมากได้ความทนสูงสุดเพิ่มกี่ % (0.2 = +20%)</summary>
    public float GreatDurabilityBonus { get; set; }
    /// <summary>เปิดโอกาสล้มเหลว (เกมเปิด: success = 1 − ((ความยาก − สกิล − correction)/100)²) — เซิร์ฟเล่นกันเองปิดไว้ก่อน</summary>
    public bool FailureEnabled { get; set; }
    public float SuccessCorrection { get; set; }
    /// <summary>ล้มเหลวแล้วคืนวัสดุกี่ส่วน (0.5 = ครึ่ง)</summary>
    public float FailureKeepRatio { get; set; }

    public static CraftingConfig Defaults()
    {
        return new CraftingConfig
        {
            MaterialLevel = true,
            EffortFormula = true,
            CollectBase = 2.5f, CollectPerLevel = 0.25f,
            CraftBase = 5f, CraftPerLevel = 0.5f,
            BuildBase = 10f, BuildPerLevel = 1f,
            GreatSuccess = true,
            GreatBase = 0.05f,
            GreatLevelBonus = 1,
            GreatDurabilityBonus = 0.2f,
            FailureEnabled = false,
            SuccessCorrection = 0f,
            FailureKeepRatio = 0.5f,
        };
    }

    public float CollectSeconds(int level) => CollectBase + Math.Max(0, level - 1) * CollectPerLevel;
    public float CraftSeconds(int level) => CraftBase + Math.Max(0, level - 1) * CraftPerLevel;
    public float BuildSeconds(int level) => BuildBase + Math.Max(0, level - 1) * BuildPerLevel;
}

/// <summary>
/// [TodoList/07] ตายแล้วเสียอะไร — ค่าจริงจาก data/assets/constants.json → death_penalty
/// Enabled=false = ฟื้นเต็ม ไม่หล่นของ (พฤติกรรมเดิม) · ItemDrop=false = ลงโทษแค่เกจ
/// </summary>
public sealed class DeathConfig
{
    public bool Enabled { get; set; }
    public bool ItemDrop { get; set; }
    /// <summary>default_item_drop_ratio 0.5</summary>
    public float DropRatio { get; set; }
    /// <summary>prevent_item_drop_ratio_by_level = 1 − PreventPerLevel×level (0.0125)</summary>
    public float PreventPerLevel { get; set; }
    /// <summary>death_point_remaining_duration 300 — กล่องของตก + หมุดอยู่กี่วิ</summary>
    public double DeathPointSeconds { get; set; }
    /// <summary>gauge_ratio_by_death_count — เลือด/สตามินาตอนฟื้น ตามจำนวนครั้งที่ตายติดกัน</summary>
    public float[] GaugeRatios { get; set; }
    /// <summary>fatigue_recovery_ratio_by_death_count — ความล้าที่หายตอนฟื้น</summary>
    public float[] FatigueRecoveryRatios { get; set; }
    /// <summary>ไม่ตายนานเท่านี้ (วิ) ตัวนับตายติดกันกลับเป็น 0 (เกมไม่ระบุ — เลือก 10 นาที)</summary>
    public double DeathCountDecaySeconds { get; set; }
    /// <summary>ของที่ใส่อยู่สึกกี่ % ของ max ตอนตาย (ของเราเอง เดิม 0.10 · ลดเหลือ 0.05 เพราะมีของหล่นแล้ว)</summary>
    public float EquipWearRatio { get; set; }
    /// <summary>blueprint กล่องที่ใช้เป็น "กล่องของตก" (ต้องมี component Inventory และมี look ให้ client วาด — fur_box_01 ไม่มี)</summary>
    public string BoxBlueprint { get; set; }

    public static DeathConfig Defaults()
    {
        return new DeathConfig
        {
            Enabled = true,
            ItemDrop = true,
            DropRatio = 0.5f,
            PreventPerLevel = 0.0125f,
            DeathPointSeconds = 300.0,
            GaugeRatios = new[] { 0.6f, 0.4f, 0.2f, 0.1f },
            FatigueRecoveryRatios = new[] { 0.3f, 0.2f, 0.1f, 0f },
            DeathCountDecaySeconds = 600.0,
            EquipWearRatio = 0.05f,
            BoxBlueprint = "fur_box_03_leaf",   // 7012 fur_box_01 ไม่มี look → client ไม่วาด · กล่องใบไม้ 6171 วาดแน่ (add_box ใช้อยู่)
        };
    }
}

public sealed class CraftMenuConfig
{
    /// <summary>ชื่อหมวด blueprint ที่ซ่อนจากผู้เล่นทั่วไป (ว่าง = ไม่ซ่อนอะไร)</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> HiddenCategories { get; set; }

    /// <summary>
    /// ซ่อนสูตร/แบบก่อสร้าง "ฟรี" (AlwaysRecipes/AlwaysBlueprints — ไม่ต้องเรียนสกิล) จาก non-admin
    /// เจ้าของสั่ง: ไอเทมที่ไม่ต้องใช้วัตถุดิบ ซ่อนให้หมด เป็นของแอดมิน — ฝั่ง blueprint ไม่หักวัตถุดิบเลย
    /// (ข้อจำกัดเบต้า) ส่วนฝั่งสูตรมีวัตถุดิบจริง แต่ที่ไม่ต้องเรียนสกิล = ดูเหมือนฟรีในสายตาผู้เล่น
    /// เปิด = non-admin เห็นเฉพาะของที่ปลดล็อกด้วยสกิลจริงเท่านั้น · admin ได้ครบเสมอ
    /// </summary>
    public bool HideFreeItems { get; set; } = true;

    /// <summary>อนุญาตการวาง/สร้างสิ่งก่อสร้างสำเร็จรูปโดยไม่ฝากวัตถุดิบ</summary>
    public bool AllowFreeBuild { get; set; } = false;

    public static CraftMenuConfig Defaults()
    {
        return new CraftMenuConfig
        {
            // 10 แท็บที่เจ้าของวงกลมไว้ให้ซ่อน (ชื่อไทยกำกับ) — เอาชื่อออกจากลิสต์ = แท็บนั้นกลับมาโชว์
            HiddenCategories = new List<string>
            {
                "deco_and_installation",   // Installation/Decoration
                "etc",                     // Other
                "clan",                    // Clan
                "residence",               // Rest/Shelter
                "traffic",                 // Transportation Facility
                "furniture_and_workbench", // Storage/Workbench
                "trap",                    // Snare/Trap
                "plant_collectible",       // Wood
                "region",                  // (#recipe_category_region)
                "building/furniture",      // (#recipe_category_building/furniture)
            }
        };
    }
}

/// <summary>
/// **ค่าสถานะพื้นฐาน 8 ตัว** — ที่มาของตัวเลขในหน้า "능력치" ของตัวละคร
///
/// เดิมส่งค่าคงที่ 20 เท่ากันหมดทุกคน (ดู AbilityData ว่าทำไมต้องออกแบบเอง)
/// สูตร: <c>Base + (เลเวลผู้เล่น - 1) × PerLevel + ผลรวม(เลเวลความชำนาญ - 1) × PerProficiency</c>
/// </summary>
public sealed class AbilityConfig
{
    /// <summary>ค่าเริ่มต้นของทุก ability ตอนเลเวล 1 ยังไม่ทำอะไรเลย</summary>
    public float Base { get; set; }
    /// <summary>เพิ่มต่อ 1 เลเวลผู้เล่นหลังเลเวลเริ่มต้น (ได้ทุก ability เท่ากัน)</summary>
    public float PerLevel { get; set; }
    /// <summary>เพิ่มต่อ 1 เลเวลความชำนาญหลังเลเวลเริ่มต้นของหมวดที่ป้อน ability ตัวนั้น</summary>
    public float PerProficiency { get; set; }
    /// <summary>เพดาน — กันเคสฟาร์มความชำนาญจนตัวเลขหลุดโลก</summary>
    public float Max { get; set; }

    public static AbilityConfig Defaults()
    {
        // lv1 ยังไม่ทำอะไร = 10 ทุกตัว · lv20 + ความชำนาญ 2 หมวดละ ~20 = 10+10+14 = 34
        return new AbilityConfig
        {
            Base = 10f,
            PerLevel = 0.5f,
            PerProficiency = 0.35f,
            Max = 100f
        };
    }
}

/// <summary>
/// **การต่อสู้** — ค่าที่เคยเป็น const ในโค้ด ย้ายมาปรับสดได้ที่นี่
///
/// 🐛 ที่แก้ในรอบนี้: อาวุธทุกชิ้นเคยบวกดาเมจ **+10 เท่ากันหมด** (ขวานหิน = ค้อนเหล็ก)
/// และเกราะ **ไม่มีค่าป้องกันเลย** ใส่แล้วได้แค่เปลี่ยนโมเดล
/// ตอนนี้อ่านค่าจริงรายชิ้นจากข้อมูลเกม (<see cref="EquipData"/>) แล้วคูณสเกลข้างล่างนี้
/// </summary>
public sealed class CombatConfig
{
    /// <summary>พลังโจมตีมือเปล่า</summary>
    public float BareHandAttack { get; set; }
    /// <summary>พลังโจมตีเพิ่มต่อ 1 เลเวลผู้เล่น</summary>
    public float AttackPerLevel { get; set; }
    /// <summary>พลังโจมตีเพิ่มต่อ 1 หน่วยพลัง (Strength)</summary>
    public float AttackPerStrength { get; set; }

    /// <summary>
    /// ตัวคูณค่า `attack` ดิบของอาวุธในข้อมูลเกม (10-250) ให้เข้าสเกลดาเมจของเซิร์ฟนี้
    /// 0.14 ⇒ ขวานหิน (73) ได้ ~10 เท่ากับโบนัสคงที่ของเดิมพอดี · ค้อนเหล็ก (177) ได้ ~25
    /// </summary>
    public float WeaponAttackScale { get; set; }

    /// <summary>ตัวคูณค่า `defense` ดิบของเกราะในข้อมูลเกม (0.3-105 ต่อชิ้น)</summary>
    public float ArmorDefenseScale { get; set; }

    /// <summary>
    /// ค่าคงที่ในสูตรลดดาเมจ: ลดลง = def / (def + K)
    /// K = 120 ⇒ ชุดหนัง+รองเท้า+ถุงมือของต้นเกม (รวม ~30) ลดดาเมจ ~20%
    /// </summary>
    public float ArmorDefenseK { get; set; }

    /// <summary>ลดดาเมจจากเกราะได้มากสุดกี่ % (กันชุดเทพจนตีไม่เข้า)</summary>
    public float ArmorMaxReduce { get; set; }

    public float CritChance { get; set; }
    public float CritMultiplier { get; set; }

    public static CombatConfig Defaults()
    {
        return new CombatConfig
        {
            BareHandAttack = 6f,
            AttackPerLevel = 0.3f,
            AttackPerStrength = 0.15f,
            WeaponAttackScale = 0.14f,
            ArmorDefenseScale = 1f,
            ArmorDefenseK = 120f,
            ArmorMaxReduce = 0.75f,
            CritChance = 0.12f,
            CritMultiplier = 1.9f     // [TodoList/05] เกม base_critical_damage_bonus 0.9 ⇒ ×1.9 (เดิม 1.6)
        };
    }
}

/// <summary>
/// โซนที่อยู่อาศัย — "ทุ่งหญ้ามีกิ้งก่ากับโดโด · ที่สูงมีสเตโก · หุบเขามีแร็ปเตอร์"
///
/// จุดกึ่งกลางเป็น **ระยะห่างจากจุดเข้าเกม** (ไม่ใช่พิกัดสัมบูรณ์) เพราะแต่ละเกาะจุดเข้าเกมคนละที่
/// เอา config เดียวไปใช้กับเกาะไหนก็ได้
///
/// สัตว์จะเกิดในโซนของตัวเองและ**เดินอยู่ในโซนนั้น** ไม่เดินหลุดไปทั่วเกาะ
/// ⇒ ผู้เล่นจำได้ว่า "ไปทางทิศไหนเจออะไร" แทนที่จะเดินสุ่มไปเรื่อย ๆ แล้วเจอมั่ว
/// </summary>
public sealed class ZoneConfig
{
    public string Id { get; set; }
    public string Name { get; set; }
    /// <summary>กึ่งกลางโซน — ห่างจากจุดเข้าเกมกี่ tile (บวก/ลบได้)</summary>
    public float OffsetTileX { get; set; }
    public float OffsetTileY { get; set; }
    /// <summary>รัศมีโซน (tile)</summary>
    public float RadiusTiles { get; set; }
    /// <summary>ชนิดสัตว์ที่อยู่ในโซนนี้ (entity type 2000-2999)</summary>
    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<ushort> Species { get; set; }

    public static List<ZoneConfig> Defaults()
    {
        return new List<ZoneConfig>
        {
            // ใกล้จุดเข้าเกม: ตัวเล็กไม่ดุ ให้ผู้เล่นใหม่มีอะไรล่าตั้งแต่นาทีแรก
            new ZoneConfig
            {
                Id = "meadow", Name = "ทุ่งหญ้าหน้าบ้าน",
                OffsetTileX = 0, OffsetTileY = 0, RadiusTiles = 14,
                Species = new List<ushort> { 2042, 2015, 2033 }
            },
            // ถัดออกไปทางตะวันออก: ตัวกลาง เริ่มสู้กลับ
            new ZoneConfig
            {
                Id = "forest", Name = "ชายป่า",
                OffsetTileX = 22, OffsetTileY = 6, RadiusTiles = 13,
                Species = new List<ushort> { 2006, 2017 }
            },
            // ทางเหนือ: ตัวใหญ่ ดูเป็นฉาก ล่าคุ้มแต่กินเวลา
            new ZoneConfig
            {
                Id = "highland", Name = "ที่ราบสูง",
                OffsetTileX = -8, OffsetTileY = 24, RadiusTiles = 13,
                Species = new List<ushort> { 2009, 2000, 2003 }
            },
            // ไกลสุด: ตัวที่ไล่กัดก่อน — ต้องตั้งใจเดินไปถึงจะเจอ
            new ZoneConfig
            {
                Id = "raptor_den", Name = "หุบแร็ปเตอร์",
                OffsetTileX = 26, OffsetTileY = -22, RadiusTiles = 10,
                Species = new List<ushort> { 2002, 2001 }
            },
        };
    }
}

public sealed class SpawnEntryConfig
{
    public ushort Type { get; set; }
    public string Name { get; set; }
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    /// <summary>ให้มีในโลกพร้อมกันกี่ตัว (0 = ไม่ต้องเกิดเลย)</summary>
    public int Quota { get; set; }
    /// <summary>Flee = หนีอย่างเดียว · FightBack = สู้กลับเมื่อโดนตี · Aggressive = ไล่กัดก่อน</summary>
    public string Behavior { get; set; }
    /// <summary>ต้องเกิดห่างจากจุดเกิดของผู้เล่นอย่างน้อยกี่ tile</summary>
    public int MinTilesFromEntry { get; set; }
    /// <summary>เว้นกี่วินาทีระหว่างการกัด (ค่าจริงจากข้อมูลเกม)</summary>
    public double AttackCooltime { get; set; }

    public AnimalBehavior BehaviorEnum
    {
        get
        {
            if (string.Equals(Behavior, "Flee", StringComparison.OrdinalIgnoreCase)) return AnimalBehavior.Flee;
            if (string.Equals(Behavior, "Aggressive", StringComparison.OrdinalIgnoreCase)) return AnimalBehavior.Aggressive;
            return AnimalBehavior.FightBack;
        }
    }

    private static SpawnEntryConfig E(ushort t, string n, int lo, int hi, int q, string b, int d, double cd)
    {
        return new SpawnEntryConfig
        {
            Type = t, Name = n, MinLevel = lo, MaxLevel = hi,
            Quota = q, Behavior = b, MinTilesFromEntry = d, AttackCooltime = cd
        };
    }

    /// <summary>ตารางเกาะเริ่มต้น Beta 1.0 — ที่มาของตัวเลขอยู่ใน docs/testing/BETA-1.0-PLAN.md</summary>
    public static List<SpawnEntryConfig> Defaults()
    {
        return new List<SpawnEntryConfig>
        {
            E(2042, "กิ้งก่า",           1, 3,  6, "Flee",       0,  1.4),
            E(2015, "คอมป์โซกนาทัส",     1, 4,  6, "Flee",       0,  3.0),
            E(2033, "โดโดฟิซิส",         2, 5,  4, "Flee",       0,  3.0),
            E(2006, "เฟนาโคดัส",         3, 6,  4, "Flee",       0,  1.6),
            E(2017, "โปรโตเซราท็อปส์",   3, 7,  4, "FightBack",  0,  1.3),
            E(2009, "พาราซอโรโลฟัส",     5, 9,  3, "FightBack",  4,  2.0),
            E(2000, "สเตโกซอรัส",        6, 10, 2, "Flee",       6,  2.1),
            E(2003, "ทริเซราท็อปส์",     6, 10, 2, "FightBack",  6,  1.4),
            E(2002, "โอวิแรปเตอร์",      4, 8,  2, "FightBack",  12, 1.3),   // [TodoList/08] ข้อมูลเกม type=Scavenger ไม่ใช่ตัวล่า
            E(2001, "แร็ปเตอร์",         7, 10, 1, "Aggressive", 20, 1.7),
        };
    }
}

/// <summary>
/// ปลูกผัก — ตัวเลขทั้งหมดของระบบเพาะปลูก
///
/// **ทำไมต้องมี GrowthScale:** เวลาโตในข้อมูลเกมเป็นของเกมออนไลน์จริง (ข้าวโพด 5 นาที
/// แต่ต้นไม้ผล 21 ชั่วโมง) ซึ่งยาวเกินไปสำหรับเซิร์ฟที่เปิดเล่นกันเอง
/// ค่าเริ่มต้น 0.05 = ย่อเหลือ 1/20 (ต้นไม้ผล 21 ชม. → ประมาณ 1 ชม.)
/// ใส่ 1.0 ถ้าอยากได้เวลาเท่าเกมจริง
/// </summary>
public sealed class FarmingConfig
{
    /// <summary>ตัวคูณเวลาโต (0.05 = เร็วกว่าเกมจริง 20 เท่า)</summary>
    public float GrowthScale { get; set; }

    /// <summary>เวลาโตอย่างน้อยที่สุด กันพืชที่โตเร็วอยู่แล้วกลายเป็นโตทันที</summary>
    public float MinGrowSeconds { get; set; }

    /// <summary>น้ำที่ได้ต่อไอเทมน้ำ 1 ชิ้น</summary>
    public float WaterPerItem { get; set; }

    /// <summary>ตัวคูณผลผลิตที่ได้จากปุ๋ย (1 = ตามข้อมูลเกม)</summary>
    public float FertilizerYieldScale { get; set; }

    /// <summary>เก็บเกี่ยวแล้วได้เมล็ดคืนกี่เม็ด (0 = ไม่คืนเมล็ด)</summary>
    public int SeedYield { get; set; }

    /// <summary>ปลูกผิดไบโอม → เวลาโตคูณเท่านี้ (และได้ผลผลิตครึ่งเดียว)</summary>
    public float WrongBiomeGrowthPenalty { get; set; }

    /// <summary>ตักน้ำ 1 ครั้งได้มากสุดกี่หน่วย (กันภาชนะเลเวลสูงตักทีเดียวจบ)</summary>
    public float MaxWaterCarryPerDraw { get; set; }

    public float StaminaCostPlant { get; set; }
    public float StaminaCostTend { get; set; }
    public float StaminaCostDraw { get; set; }

    public float PlantSeconds { get; set; }
    public float TendSeconds { get; set; }
    public float UprootSeconds { get; set; }
    public float HarvestSeconds { get; set; }
    public float DrawWaterSeconds { get; set; }

    /// <summary>ระยะที่ตักน้ำได้ นับจากตัวผู้เล่นถึง tile ที่เป็นน้ำ (tile)</summary>
    public int WaterSearchTiles { get; set; }

    public static FarmingConfig Defaults()
    {
        return new FarmingConfig
        {
            GrowthScale = 0.05f,
            MinGrowSeconds = 20f,
            WaterPerItem = 1f,
            FertilizerYieldScale = 1f,
            SeedYield = 1,
            WrongBiomeGrowthPenalty = 1.5f,
            MaxWaterCarryPerDraw = 5f,
            StaminaCostPlant = 6f,
            StaminaCostTend = 3f,
            StaminaCostDraw = 3f,
            PlantSeconds = 2f,
            TendSeconds = 1.5f,
            UprootSeconds = 2f,
            HarvestSeconds = 2f,
            DrawWaterSeconds = 2f,
            WaterSearchTiles = 6
        };
    }
}

/// <summary>
/// รอยแยก/วาร์ปเรกเซเลอเรเตอร์ (blueprint "warp_accelerator") — ดู WarpAcceleratorManager สำหรับ state machine
///
/// **ที่มาของตัวเลขเริ่มต้น**: เกมปิดตัวไปนานแล้ว ลองหาข้อมูลกลไกจริง (รีวิว/wiki เกาหลี) ไม่เจอ
/// ข้อมูลตัวเลขที่ยืนยันได้จากไฟล์เกม (client/Yaml/WarpAccelerator.cs) มีแค่ *ชื่อ* ฟิลด์
/// (breaktime/phase_time/reward_time/inactivate_time/max_phase) ไม่มีค่าจริงติดมาด้วย (อยู่ใน
/// asset bundle ที่เข้ารหัส อ่านไม่ได้จากซอร์สที่มี) ⇒ ค่าเริ่มต้นทั้งหมดด้านล่างนี้ **ออกแบบเองใหม่**
/// ให้เล่นได้จริงและสมเหตุสมผล ไม่ใช่ค่าจากเกมต้นฉบับ — ปรับได้อิสระผ่าน data/config.json (hot-reload)
/// </summary>
/// <summary>
/// Server-driven weather cycle. The client already contains the visual weather
/// presets; this section only controls which safe preset is broadcast and how
/// long each preset lasts.
/// </summary>
public sealed class WeatherConfig
{
    public bool Enabled { get; set; }
    public double CycleSeconds { get; set; }

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<string> Sequence { get; set; }

    public static WeatherConfig Defaults()
    {
        return new WeatherConfig
        {
            Enabled = true,
            CycleSeconds = 90.0,
            Sequence = new List<string>
            {
                "sunny",
                "cloudy",
                "rainy",
                "heavy_rainy",
                "sunny"
            }
        };
    }
}

/// <summary>
/// [3 ก.ย. 2026] "ระบบเหมือน PC" สำหรับเกมมือถือของแท้ — ทำจากฝั่งเซิร์ฟล้วน ๆ (เจ้าของเลือกแนวทางนี้
/// แทนการแพตช์ libil2cpp.so) client มือถือรู้จักแค่โปรโตคอลเกมต้นฉบับ ⇒ ทุกอย่างต้องเป็นสิ่งที่
/// เกม 5.2.1 แสดงผลได้เองอยู่แล้ว: ข้อความ Info (popup), แชทช่อง System, ค่า cluster_mode จาก /entry
/// มีผลเฉพาะ client ที่ /sessions หรือ /entry บอกว่า platform=Android (PC ไม่กระทบ)
/// </summary>
/// <summary>
/// [4 ก.ย. 2026] DurangoID — ระบบสมัครไอดีของเราเอง (เจ้าของสั่ง "ใช้เลขสุ่มจากเราเป็นไอดี + มีหน้าสมัคร")
///
/// ตัวตนเดิมผูกกับ IP ล้วน ๆ ซึ่งพังกับมือถือ (เกมมือถือส่ง account_id/adid มาเป็นค่าว่าง เพราะ
/// Platform_Android ไม่ override) ⇒ ให้ผู้เล่นสมัครเลข 8 หลักที่หน้า /id แล้ว "ผูกเครื่องนี้"
/// เซิร์ฟจด IP ไว้ให้ไอดีนั้น พอเข้าเกมจึงรู้ว่าเป็นใคร — ดู <see cref="DurangoServer.Core.PlayerIdStore"/>
/// </summary>
public sealed class PlayerIdConfig
{
    /// <summary>เปิดหน้าสมัคร /id และ endpoint ที่เกี่ยวข้อง</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// บังคับว่าต้องมีไอดีที่ผูก IP นี้ไว้ ถึงจะสร้าง/เห็นตัวละครได้ (เจ้าของเลือก "บังคับสมัครทุกคน")
    ///
    /// ⚠️ เปิดบนเซิร์ฟที่มีคนเล่นอยู่แล้ว = ทุกคนต้องไปสมัคร+กด "รับตัวละครเดิม" ที่หน้าเว็บก่อนถึงจะเล่นต่อได้
    /// </summary>
    public bool Required { get; set; }

    /// <summary>ผูก IP ไว้กี่วัน (0 = ไม่หมดอายุ) — เน็ตมือถือ IP เปลี่ยนบ่อย ผู้เล่นกดผูกใหม่ที่หน้าเว็บได้</summary>
    public int BindingDays { get; set; }

    /// <summary>ลิงก์หน้าสมัครที่เอาไปบอกผู้เล่นในเกม เช่น "http://187.53.129.69:8190/id" (ว่าง = เดาจาก request)</summary>
    public string PublicUrl { get; set; }

    public static PlayerIdConfig Defaults() => new PlayerIdConfig
    {
        Enabled = true,
        Required = true,
        BindingDays = 30,
        PublicUrl = ""
    };
}

public sealed class AndroidConfig
{
    /// <summary>
    /// cluster_mode ที่ตอบให้มือถือใน /knock และ /entry · ว่าง = ใช้ค่าเดียวกับเซิร์ฟ (--cluster-mode)
    ///
    /// ทำไมต้องแยก: เมนูในเกมถูกกรองด้วย ClusterMode ฝั่ง client (MenuSystem.IsHiddenMenu ของเกมต้นฉบับ)
    ///   · Online     ⇒ ซ่อน MoveToTitle (กลับหน้าไตเติ้ล) / Connect / WarpShop / CharacterOnMenu … (HiddenInOnline)
    ///   · SingleMode ⇒ โชว์ตามรายการ ShowInSingleMode ซึ่งมี Craft/Skill/Quest/Estate/Encyclopedia/**MoveToTitle** ครบ
    /// client PC ชุดเราแก้รายการนี้ในโค้ดแล้ว แต่มือถือแก้ไม่ได้ ⇒ สลับโหมดให้มันแทน
    /// แชทช่องรวมยังใช้ได้ทั้งสองโหมด (SingleMode ส่งผ่าน connection เกม · Online ส่งผ่านพอร์ต radiotower)
    /// </summary>
    public string ClusterMode { get; set; }

    /// <summary>เข้าโลกแล้วเด้ง popup "ยินดีต้อนรับ … ออนไลน์ N คน" ให้มือถือ (PC ชุดเรามีจำนวนคนที่แท็บแชทอยู่แล้ว)</summary>
    public bool WelcomeInfo { get; set; }

    /// <summary>มีคนเข้า/ออก ⇒ ส่งบรรทัดแชทช่อง System "X เข้าเกม · ออนไลน์ N คน" ให้มือถือ (แทนตัวเลขบนแท็บแชทของ PC)</summary>
    public bool OnlineCountInChat { get; set; }

    /// <summary>
    /// [4 ก.ย. 2026] ส่ง terrain chunk เต็มกรอบทุกครั้งที่มือถือข้ามขอบ chunk (ไม่ข้ามการส่งซ้ำ)
    ///
    /// 🐛 แก้อาการ "เดินต่อแล้วพื้นที่ใหม่ไม่เรนเดอร์ ค้างอยู่แค่โซนเดิม" — เกมต้นฉบับบนมือถือถือ chunk
    /// ไว้ไม่เท่าที่เซิร์ฟคิด ก้อนที่มันทิ้งไปจะไม่ถูกส่งซ้ำ กลายเป็นรูโหว่ถาวร (ดู ServerPlayer.HandleSetChunk)
    /// ปิด = กลับไปใช้พฤติกรรมเดิมแบบ PC (ประหยัดแบนด์วิดท์กว่า แต่มือถือจะมีรูโหว่)
    ///
    /// ⚠️ เป็น <c>bool?</c> ตั้งใจ: config.json ที่มีอยู่แล้ว (ไม่มีคีย์นี้) จะได้ null = เปิดไว้ตามค่าเริ่มต้น
    /// ถ้าใช้ bool ธรรมดา ไฟล์เก่าจะ deserialize เป็น false แล้วแก้บั๊กไม่ทำงานแบบเงียบ ๆ
    /// </summary>
    public bool? AlwaysResendChunks { get; set; }

    public static AndroidConfig Defaults() => new AndroidConfig
    {
        // [4 ก.ย. 2026] เจ้าของสั่ง: ทุก client ใช้ Online จริง (SingleMode/Offline อ่านข้อมูลเกมจากไฟล์ที่ฝังในตัวเกม
        // แทน /assets ของเซิร์ฟ ⇒ ผู้เล่นแต่ละคนเห็นสูตรคราฟต์ไม่เท่ากัน) — แลกกับเมนู "กลับหน้าไตเติ้ล" บนมือถือ
        ClusterMode = "Online",
        WelcomeInfo = true,
        OnlineCountInChat = true,
        AlwaysResendChunks = true
    };
}

public sealed class WarpAcceleratorConfig
{
    /// <summary>เวลารอก่อนคลื่นแรกเริ่ม (นับจากกด "เร่งวาร์ป") และเวลาพักระหว่างคลื่น (Waiting/Intermission)</summary>
    public float WaitSeconds { get; set; }

    /// <summary>เวลาที่มีให้ฆ่าสัตว์ในแต่ละคลื่นก่อนหมดเวลา — หมดเวลาแล้วยังมีสัตว์เหลือ = คลื่นนั้นล้มเหลว ไม่ได้รางวัล</summary>
    public float PhaseSeconds { get; set; }

    /// <summary>ผ่านครบทุกคลื่นแล้ว มีเวลากดรับรางวัล (ReceiveAcceleratorRewards) นานแค่ไหนก่อนรีเซ็ตอัตโนมัติ</summary>
    public float RewardWindowSeconds { get; set; }

    /// <summary>จบรอบแล้ว (สำเร็จหรือล้มเหลวก็ตาม) ต้องรอเท่าไรก่อนจะ "เร่งวาร์ป" รอบใหม่ที่จุดเดิมได้</summary>
    public float CooldownSeconds { get; set; }

    /// <summary>จำนวนคลื่นทั้งหมดต่อรอบ</summary>
    public int MaxPhase { get; set; }

    /// <summary>จำนวนสัตว์คลื่นแรก</summary>
    public int AnimalsBase { get; set; }

    /// <summary>จำนวนสัตว์ที่เพิ่มขึ้นต่อคลื่น (คลื่นที่ 2 = Base+Step, คลื่นที่ 3 = Base+Step*2, ...)</summary>
    public int AnimalsStep { get; set; }

    /// <summary>กระจายสัตว์รอบจุด warp_accelerator ในรัศมีกี่ tile</summary>
    public float SpawnRadiusTiles { get; set; }

    /// <summary>
    /// ค่าธรรมเนียมเข้าร่วม (Accelerate/ParticipateAcceleration) — ตั้ง 0 ไว้ก่อนโดยตั้งใจ:
    /// เซิร์ฟนี้ยังไม่มีระบบกระเป๋าเงิน/Currency ใช้งานจริงเลยสักจุด (grep "Currency\." ทั้ง
    /// ServerCore ไม่เจอที่ไหนใช้เลย) ผู้เล่นจึงไม่มีทางหา Gem/Coin มาจ่ายค่าธรรมเนียมได้ตั้งแต่ต้น
    /// ถ้าจะเก็บค่าธรรมเนียมจริงต้องสร้างระบบกระเป๋าเงินทั้งระบบก่อน (งานใหญ่แยกต่างหาก นอกขอบเขตนี้)
    /// </summary>
    public long JoinCostAmount { get; set; }

    /// <summary>สกุลเงินของค่าธรรมเนียมเข้าร่วม (ไม่มีผลตอนนี้เพราะ JoinCostAmount = 0)</summary>
    public Currency JoinCostCurrency { get; set; }

    /// <summary>Warp Matter ที่ได้ต่อคลื่นที่ผ่าน (สะสมไว้ก่อน กดรับจริงตอน Status = End เท่านั้น)</summary>
    public int WarpMatterPerPhase { get; set; }

    /// <summary>เพดาน Warp Matter ที่ผู้เล่นคนหนึ่งรับได้ต่อสัปดาห์ (ดู ServerPlayer.WarpAccelerator)</summary>
    public int WeeklyWarpMatterCap { get; set; }

    public static WarpAcceleratorConfig Defaults()
    {
        return new WarpAcceleratorConfig
        {
            WaitSeconds = 20f,
            PhaseSeconds = 90f,
            RewardWindowSeconds = 180f,
            CooldownSeconds = 30f,
            MaxPhase = 3,
            AnimalsBase = 4,
            AnimalsStep = 2,
            SpawnRadiusTiles = 4f,
            JoinCostAmount = 0,
            JoinCostCurrency = Currency.Coin,
            WarpMatterPerPhase = 5,
            WeeklyWarpMatterCap = 100
        };
    }
}
