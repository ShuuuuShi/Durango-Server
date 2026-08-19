using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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
            Spawn = SpawnEntryConfig.Defaults(),
            Zones = ZoneConfig.Defaults(),
            World = WorldConfig.Defaults(),
            Tools = ToolConfig.Defaults(),
            Features = FeatureConfig.Defaults(),
            Survival = SurvivalConfig.Defaults(),
            Starter = StarterConfig.Defaults(),
            Food = FoodConfig.Defaults(),
            Abilities = AbilityConfig.Defaults(),
            Combat = CombatConfig.Defaults()
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
        if (Exp == null) { Exp = ExpConfig.Defaults(); filled = true; }
        if (Skills == null) { Skills = SkillConfig.Defaults(); filled = true; }
        if (Zones == null) { Zones = ZoneConfig.Defaults(); filled = true; }
        if (World == null) { World = WorldConfig.Defaults(); filled = true; }
        if (Tools == null) { Tools = ToolConfig.Defaults(); filled = true; }
        if (Features == null) { Features = FeatureConfig.Defaults(); filled = true; }
        if (Survival == null) { Survival = SurvivalConfig.Defaults(); filled = true; }
        if (Starter == null || Starter.Recipes == null) { Starter = StarterConfig.Defaults(); filled = true; }
        if (Food == null) { Food = FoodConfig.Defaults(); filled = true; }
        if (Abilities == null) { Abilities = AbilityConfig.Defaults(); filled = true; }
        if (Combat == null) { Combat = CombatConfig.Defaults(); filled = true; }
        if (Spawn == null || Spawn.Count == 0) { Spawn = SpawnEntryConfig.Defaults(); filled = true; }
        return filled;
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
        if (Tools.Enabled && (Tools.DurabilityBase < 0f || Tools.DurabilityPerTier < 0f
                              || Tools.DurabilityBase + Tools.DurabilityPerTier <= 0f))
        {
            return "ความทนทานเครื่องมือต้องมากกว่า 0 (Tools.DurabilityBase / DurabilityPerTier)";
        }
        if (Tools.WearPerUse < 0f)
        {
            return "Tools.WearPerUse ติดลบไม่ได้";
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

    /// <summary>ใช้ 1 ครั้ง (เก็บของ/แล่ซาก 1 ชิ้น) เสียความทนทานเท่าไร</summary>
    public float WearPerUse { get; set; }

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
            WearPerUse = 1f
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
    /// <summary>เควสจาก 4 กลุ่ม NPC</summary>
    public bool Quests { get; set; }
    /// <summary>PK บนเกาะ Lv.20+</summary>
    public bool Pvp { get; set; }
    /// <summary>สิทธิ์ในที่ดินส่วนตัว (เฉพาะเรา/เพื่อน/สาธารณะ)</summary>
    public bool LandPermission { get; set; }
    /// <summary>ปาร์ตี้/แคลน</summary>
    public bool PartyAndClan { get; set; }
    /// <summary>ท่าทาง/อีโมติคอนของผู้เล่น (ไม่ได้อยู่ในรายการ LBT1)</summary>
    public bool Emotes { get; set; }

    /// <summary>
    /// เพดานเลเวลของรอบนี้ — LBT1 ปล่อยคอนเทนต์แค่ Lv.1-20
    /// ตารางเลเวลจริงมีถึง 81 · ตั้ง 0 = ไม่จำกัด (ใช้เพดานเต็มของตาราง)
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
            Jobs = false,
            Cooking = true,             // เปิดแล้ว — สูตร cook 152 อัน ต้องยืนที่กองไฟ/เตาถึงจะทำได้
            Farming = false,
            Livestock = false,
            Taming = false,
            Market = false,
            Quests = false,
            Pvp = false,
            LandPermission = false,
            PartyAndClan = false,
            Emotes = false,

            MaxPlayerLevel = 20
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
    /// <summary>พักที่กองไฟแล้วความล้าลดวินาทีละเท่าไร (0 = พักไม่ได้)</summary>
    public float RestFatiguePerSec { get; set; }
    /// <summary>ต้องอยู่ห่างกองไฟไม่เกินกี่ tile ถึงจะพักได้</summary>
    public float RestRangeTiles { get; set; }

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
            StaminaCostCollect = 6f,
            StaminaCostCraft = 4f,
            StaminaCostBuild = 8f,

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

            RestFatiguePerSec = 4f,            // พัก 25 วินาทีที่กองไฟ = หายล้าทั้งหมด
            RestRangeTiles = 3f
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
    /// </summary>
    public int ChunkSendRange { get; set; }

    public static WorldConfig Defaults()
    {
        return new WorldConfig { ChunkSendRange = 2 };
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

    /// <summary>ซากอยู่ในโลกกี่วินาทีก่อนหาย (ต้องนานพอให้แล่จนครบ)</summary>
    public double CorpseSeconds { get; set; }
    /// <summary>ตายแล้วกี่วินาทีถึงเกิดตัวใหม่แทน</summary>
    public double RespawnSeconds { get; set; }

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

    public static AnimalConfig Defaults()
    {
        return new AnimalConfig
        {
            LifeBase = 30f,
            LifePerLevel = 8f,
            DamageBase = 2f,
            DamagePerLevel = 0.4f,
            CorpseSeconds = 150.0,
            RespawnSeconds = 60.0,
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
            WanderRadiusTiles = 12.5f
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
            CritMultiplier = 1.6f
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

    /// <summary>ตารางเกาะเริ่มต้น Beta 1.0 — ที่มาของตัวเลขอยู่ใน docs/BETA-1.0-PLAN.md</summary>
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
            E(2002, "โอวิแรปเตอร์",      4, 8,  2, "Aggressive", 12, 1.3),
            E(2001, "แร็ปเตอร์",         7, 10, 1, "Aggressive", 20, 1.7),
        };
    }
}
