using System;
using System.Collections.Generic;
using Durango.Utils;

namespace DurangoServer.Core;

// ============================================================================
// [TodoList/08 · 3 ก.ย. 2026] ฝูงสัตว์ตาม region template ของเกมต้นฉบับ
//
// ต้นฉบับกำหนด "ใบสั่งเกิด" ต่อเกาะไว้ใน region_templates.json (สกัดเป็น RegionTemplateData):
//   ri35te171228 → land 48 ฝูง (6 ชนิด × 8 ฝูง × 20 ตัว) + beach 6 ฝูง × 5 ตัว ≈ 990 ตัว
// และวางจุดตั้งฝูงไว้ใน herds.yml ของเกาะ (TerrainHerds) ฝูงละจุด
//
// ของเราเดิม (ตาราง Spawn ใน config): 34 ตัว 10 ชนิด เกิดทีละตัว **บังคับห่างกัน** 4 tile
// หายเองทุก 5 นาทีแล้วเกิดที่อื่น — ตรงข้ามกับฝูงเลย
//
// ที่ทำในไฟล์นี้:
//   • ฝูง = {ชนิด, บ้าน (จุดจาก herds.yml), สมาชิก N} — สมาชิกเกิดกระจุกรอบบ้าน
//   • สมาชิกใช้ Home ร่วมกัน = บ้านฝูง · บ้านขยับทีละนิด (drift) สมาชิกเดินสุ่มรอบ Home จึงตามไปเอง
//   • ถาวร: ไม่หมดอายุ · ตาย/ติดหิน → "เติมฝูง" กลับที่บ้านเดิม
//   • ชนิดที่ไม่มีใน config Spawn ใช้นิสัย/คูลดาวน์จากข้อมูลเกม (AnimalKindData)
//
// ปิดได้ที่ config Animals.Herds.Enabled=false หรือเกาะไม่มี template → โหมดเดิมทั้งหมด
// ============================================================================

public sealed partial class AnimalSpawner
{
    private sealed class Herd
    {
        public int Id;
        public ushort Type;
        public string Name = "";
        public string Group = "";
        /// <summary>จุดตั้งจาก herds.yml — บ้านขยับได้แต่ไม่ไกลจากตรงนี้</summary>
        public WorldPosition Anchor;
        /// <summary>บ้านปัจจุบัน = Home ของสมาชิกทุกตัว</summary>
        public WorldPosition Home;
        public int Size;
        public int MinInland;
        public readonly HashSet<string> Members = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>เวลาที่จะเติมสมาชิกกลับ (ตายหรือติดหิน) — 1 รายการต่อ 1 ตัว</summary>
        public readonly List<double> PendingAt = new List<double>();
        public double NextDriftAt;
        public int SpawnFailures;
    }

    private readonly List<Herd> _herds = new List<Herd>();
    /// <summary>animalId → ฝูง (แก้ใต้ _lock)</summary>
    private readonly Dictionary<string, Herd> _herdOf = new Dictionary<string, Herd>(StringComparer.Ordinal);
    private RegionTemplateData.Template _template;
    private bool _herdMode;
    private double _nextHerdCheckAt;

    /// <summary>true = เกาะนี้ใช้ฝูงตาม template (ตาราง Spawn/โควตาเดิมไม่ทำงาน)</summary>
    public bool HerdMode => _herdMode;

    private static HerdConfig HerdCfg => ServerConfig.Current.Animals.Herds;

    /// <summary>
    /// พยายามเข้าโหมดฝูง — คืน true ถ้าสร้างฝูงได้อย่างน้อย 1 ฝูง (ผู้เรียกจะไม่ใช้ตาราง Spawn ต่อ)
    /// </summary>
    private bool SpawnInitialHerds(double now)
    {
        HerdConfig cfg = HerdCfg;
        if (cfg == null || !cfg.Enabled)
        {
            return false;
        }
        string terrainId = _world.Terrain.TerrainId;
        RegionTemplateData.Template template;
        if (!string.IsNullOrEmpty(cfg.Template))
        {
            if (!RegionTemplateData.All.TryGetValue(cfg.Template, out template))
            {
                Console.WriteLine("[herd] ไม่มี template ชื่อ \"{0}\" ใน region_templates — ใช้ตาราง Spawn แบบเดิม", cfg.Template);
                return false;
            }
        }
        else
        {
            template = RegionTemplateData.Find(terrainId);
            if (template == null)
            {
                Console.WriteLine("[herd] เกาะ {0} ไม่มี region template (แมพที่สร้างเอง?) — ใช้ตาราง Spawn แบบเดิม", terrainId);
                return false;
            }
        }
        TerrainHerds herdTiles = _world.Terrain.Herds;
        if (herdTiles == null)
        {
            Console.WriteLine("[herd] เกาะ {0} ไม่มี herds.yml — ไม่มีจุดตั้งฝูง ใช้ตาราง Spawn แบบเดิม", terrainId);
            return false;
        }

        _template = template;
        WorldPosition entry = _world.GetEntryPosition();
        int skippedWater = 0, skippedWaterAnimals = 0, noSpot = 0, nextId = 1;
        var perType = new Dictionary<ushort, (int herds, int animals)>();

        // รอบแรก: คิดว่าแต่ละชนิดควรมีกี่ฝูง (spec.Count × CountScale, อย่างน้อย 1)
        // แล้ววางแบบ round-robin ข้ามชนิด — ถ้ามี MaxHerds ทุกชนิดจะยังได้อย่างน้อย 1 ฝูงก่อนที่ชนิดไหนจะได้ 2
        var plans = new List<(RegionTemplateData.HerdSpec spec, int herdCount, int herdSize, List<Point2> tiles, string name, int minInland, float minEntry)>();
        int skippedNoAndroidModel = 0;
        foreach (RegionTemplateData.HerdSpec spec in template.Herds)
        {
            // [Android] ถ้าเสิร์ฟชุด bundle มือถืออยู่และตั้ง AndroidSafeOnly — เกิดเฉพาะชนิดที่มีโมเดล Android
            // (ชนิดที่ไม่มี client มือถือจะได้ตัวล่องหน AnimalLoadFailed) · ดู TodoList/ROADMAP-ANDROID.md
            if (cfg.AndroidSafeOnly && !HasAndroidModel(spec.EntityType))
            {
                skippedNoAndroidModel++;
                continue;
            }
            if (Array.IndexOf(TerrainHerds.LandGroups, spec.Group) < 0)
            {
                // lake_*/ocean — ยังไม่มีระบบสัตว์น้ำ (ว่ายน้ำ/ดำน้ำ) ข้ามไปก่อน
                skippedWater += spec.Count;
                skippedWaterAnimals += spec.Count * spec.Size;
                continue;
            }
            List<Point2> tiles = herdTiles.Group(spec.Group);
            if (tiles == null || tiles.Count == 0)
            {
                Console.WriteLine("[herd] herds.yml ไม่มีจุดกลุ่ม {0} — ข้าม {1}", spec.Group, spec.EntityType);
                continue;
            }
            // ปัดครึ่งขึ้นเสมอ — Math.Round ปกติปัด 2.5 → 2 (banker's) ทำให้ฝูงหาด 5×0.5 เหลือ 2 ตัว
            int herdCount = Math.Max(1, (int)Math.Round(spec.Count * cfg.CountScale, MidpointRounding.AwayFromZero));
            int herdSize = Math.Max(1, (int)Math.Round(spec.Size * cfg.SizeScale, MidpointRounding.AwayFromZero));
            string name = AnimalData.TryGet(spec.EntityType, out AnimalData.AnimalInfo info) ? info.Name : spec.EntityType.ToString();
            // ตัวหาด: จุดใน herds.yml กลุ่ม beach อยู่ติดทะเลโดยนิยาม — ขืนใช้ InlandFor ของตัวใหญ่จะหาที่ไม่ได้เลย
            int minInland = spec.Group == "beach" ? 1 : InlandFor(spec.EntityType);
            // ตัวที่ "ไล่กัด" จริง (config Spawn override ชนะข้อมูลเกม) ต้องอยู่ห่างจุดเข้ามากกว่า
            bool hunts = BehaviorOf(spec.EntityType) == AnimalBehavior.Aggressive;
            float minEntry = (hunts ? cfg.CarnivoreMinTilesFromEntry : cfg.MinTilesFromEntry) * 200f;
            plans.Add((spec, herdCount, herdSize, tiles, name, minInland, minEntry));
        }

        int maxHerds = cfg.MaxHerds > 0 ? cfg.MaxHerds : int.MaxValue;
        int maxRounds = 0;
        foreach (var pl in plans) { maxRounds = Math.Max(maxRounds, pl.herdCount); }
        int planned = 0;
        for (int round = 0; round < maxRounds && _herds.Count < maxHerds; round++)
        {
            foreach (var pl in plans)
            {
                if (round >= pl.herdCount) { continue; }
                if (_herds.Count >= maxHerds) { break; }
                planned++;
                if (!TryPickHerdHome(pl.tiles, pl.minInland, pl.minEntry, entry, pl.spec.Group == "beach", out WorldPosition home))
                {
                    noSpot++;
                    continue;
                }
                var herd = new Herd
                {
                    Id = nextId++,
                    Type = pl.spec.EntityType,
                    Name = pl.name,
                    Group = pl.spec.Group,
                    Anchor = home,
                    Home = home,
                    Size = pl.herdSize,
                    MinInland = pl.minInland,
                    NextDriftAt = now + RandomDrift(cfg),
                };
                _herds.Add(herd);
                int born = 0;
                for (int m = 0; m < pl.herdSize; m++)
                {
                    if (TrySpawnHerdMember(herd, now, out _))
                    {
                        born++;
                    }
                    else
                    {
                        herd.PendingAt.Add(now + 15.0);      // ที่รอบบ้านแน่นตอนนี้ ค่อยลองใหม่
                    }
                }
                (int hs, int an) = perType.TryGetValue(pl.spec.EntityType, out var cur) ? cur : (0, 0);
                perType[pl.spec.EntityType] = (hs + 1, an + born);
            }
        }

        if (_herds.Count == 0)
        {
            Console.WriteLine("[herd] template {0} สร้างฝูงไม่ได้เลย — ใช้ตาราง Spawn แบบเดิม", template.Name);
            _template = null;
            return false;
        }
        _herdMode = true;

        int total = 0;
        foreach (Herd herd in _herds) { total += herd.Members.Count; }
        Console.WriteLine("[herd] template {0} (เกาะ Lv.{1} · ตามเกม {2} ฝูง ~{3} ตัว) → scale ฝูง×{4:0.##} ตัว×{5:0.##}{9} = {6} ฝูง {7} ตัว · เลเวล {8}",
            template.Name, template.Level, CountHerds(template), template.TotalAnimals,
            cfg.CountScale, cfg.SizeScale, _herds.Count, total, DescribeHerdLevel(cfg, template),
            cfg.MaxHerds > 0 ? $" เพดาน {cfg.MaxHerds} ฝูง" : "");
        foreach (KeyValuePair<ushort, (int herds, int animals)> kv in perType)
        {
            Console.WriteLine("[herd]   {0,-5} {1,-16} {2,3} ฝูง {3,4} ตัว · {4}", kv.Key, kv.Value.herds > 0 ? NameOf(kv.Key) : "?",
                kv.Value.herds, kv.Value.animals, DescribeBehavior(kv.Key));
        }
        foreach (Herd herd in _herds)
        {
            Console.WriteLine("[herd]   #{0,-2} {1,-5} {2,-6} tile {3,3:F0},{4,3:F0}  {5}/{6} ตัว", herd.Id, herd.Type, herd.Group,
                herd.Home.x / 200f, herd.Home.y / 200f, herd.Members.Count, herd.Size);
        }
        if (skippedNoAndroidModel > 0)
        {
            Console.WriteLine("[herd]   ข้าม {0} ชนิดที่ไม่มีโมเดลใน bundle ชุด Android (Herds.AndroidSafeOnly)", skippedNoAndroidModel);
        }
        if (skippedWater > 0)
        {
            Console.WriteLine("[herd]   ข้ามฝูงในน้ำ (lake/ocean) {0} ฝูง ~{1} ตัว — ยังไม่มีระบบสัตว์น้ำ", skippedWater, skippedWaterAnimals);
        }
        if (template.CraterCount > 0)
        {
            Console.WriteLine("[herd]   หลุมอุกกาบาต {0} หลุม ({1} ชนิด) — เกาะนี้มีหลุมใน pois.yml {2} จุด · ยังไม่ทำ",
                template.CraterCount, template.CraterSpecies.Length, _world.Terrain.Pois?.Craters.Count ?? 0);
        }
        if (noSpot > 0)
        {
            Console.WriteLine("[herd]   ⚠ หาที่ตั้งให้ฝูงไม่ได้ {0} ฝูง (จุดใน herds.yml ติดหิน/ตื้น/ใกล้จุดเข้า/ชิดฝูงอื่นหมด)", noSpot);
        }
        return true;
    }

    private static int CountHerds(RegionTemplateData.Template t)
    {
        int n = 0;
        foreach (RegionTemplateData.HerdSpec h in t.Herds) { n += h.Count; }
        return n;
    }

    private static string NameOf(ushort type)
    {
        return AnimalData.TryGet(type, out AnimalData.AnimalInfo info) ? info.Name : type.ToString();
    }

    private static string DescribeHerdLevel(HerdConfig cfg, RegionTemplateData.Template t)
    {
        return cfg.LevelMin <= 0 && cfg.LevelMax <= 0
            ? $"{t.Level} (ตาม template)"
            : $"{cfg.LevelMin}-{cfg.LevelMax} (config · template ให้ {t.Level})";
    }

    private static string DescribeBehavior(ushort type)
    {
        string src = SpawnTable.Find(type) != null ? "config" : "ข้อมูลเกม";
        string kind = AnimalKindData.TryGet(type, out AnimalKindData.Info k) ? k.Kind.ToString() : "?";
        return $"{kind} → {BehaviorOf(type)} · กัดทุก {AttackCooltimeOf(type):0.0} วิ ({src})";
    }

    /// <summary>
    /// [Android] มีไฟล์ bundle โมเดลของชนิดนี้ในโฟลเดอร์ --assetbundles-android ไหม
    /// ชื่อ bundle = models$animals$&lt;model_path ตัวเล็ก / → $&gt;.prefab.&lt;hash&gt;.bundle (ยืนยันจาก index Android: raptor/raptorprefab)
    /// ไม่ได้ตั้งโฟลเดอร์ = ถือว่ามีหมด (ไม่กรอง)
    /// </summary>
    private static bool HasAndroidModel(ushort type)
    {
        string dir = Gateway.AssetBundleAndroidDir;
        if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir))
        {
            return true;
        }
        if (!AnimalData.TryGet(type, out AnimalData.AnimalInfo info) || string.IsNullOrEmpty(info.ModelPath))
        {
            return false;
        }
        string prefix = "models$animals$" + info.ModelPath.ToLowerInvariant().Replace('/', '$') + ".prefab.";
        lock (_androidModelCache)
        {
            if (_androidModelCache.TryGetValue(prefix, out bool cached)) { return cached; }
            bool found = System.IO.Directory.GetFiles(dir, prefix + "*.bundle").Length > 0;
            _androidModelCache[prefix] = found;
            return found;
        }
    }

    private static readonly Dictionary<string, bool> _androidModelCache = new Dictionary<string, bool>(StringComparer.Ordinal);

    private static bool IsCarnivore(ushort type)
    {
        return AnimalKindData.TryGet(type, out AnimalKindData.Info k) && k.Kind == AnimalKindData.Kind.Carnivore;
    }

    /// <summary>
    /// นิสัยเริ่มต้นของชนิดที่ไม่มีใน config Spawn — จาก type ของเกม:
    /// Carnivore ไล่กัด · Scavenger สู้กลับ · Herbivore ตัวใหญ่ (size ≥ 4) สู้กลับ ตัวเล็กหนี
    /// </summary>
    private static AnimalBehavior DefaultBehaviorOf(ushort type)
    {
        if (!AnimalKindData.TryGet(type, out AnimalKindData.Info k))
        {
            return AnimalBehavior.FightBack;
        }
        switch (k.Kind)
        {
            case AnimalKindData.Kind.Carnivore: return AnimalBehavior.Aggressive;
            case AnimalKindData.Kind.Scavenger: return AnimalBehavior.FightBack;
            default:
                int size = AnimalData.TryGet(type, out AnimalData.AnimalInfo info) ? info.SizeLevel : 1;
                return size >= 4 ? AnimalBehavior.FightBack : AnimalBehavior.Flee;
        }
    }

    private static double DefaultAttackCooltimeOf(ushort type)
    {
        return AnimalKindData.TryGet(type, out AnimalKindData.Info k) && k.AttackCooltime > 0f ? k.AttackCooltime : AttackInterval;
    }

    private double RandomDrift(HerdConfig cfg)
    {
        return cfg.DriftMinSeconds + _rng.NextDouble() * (cfg.DriftMaxSeconds - cfg.DriftMinSeconds);
    }

    private int PickHerdLevel(ushort type)
    {
        HerdConfig cfg = HerdCfg;
        int lo = cfg.LevelMin, hi = cfg.LevelMax;
        if (lo <= 0 && hi <= 0)
        {
            lo = hi = Math.Max(1, _template?.Level ?? 1);
        }
        int level = lo + _rng.Next(Math.Max(0, hi - lo) + 1);
        // clamp ด้วย combat_level_ranges เฉพาะตอนใช้ level ของ template — ถ้า config ตั้งช่วงเองให้เคารพ config
        // (ไม่งั้นคอมป์ฯ ตัวตลก [10,60] โผล่ Lv.10 บนเกาะเทส 1-4)
        if (cfg.LevelMin <= 0 && cfg.LevelMax <= 0 && AnimalKindData.TryGet(type, out AnimalKindData.Info k))
        {
            level = Math.Clamp(level, Math.Max(1, k.LevelMin), Math.Max(1, k.LevelMax));
        }
        return Math.Max(1, level);
    }

    /// <summary>
    /// เลือกบ้านฝูงจากจุดใน herds.yml กลุ่มเดียวกัน: แผ่นดินลึกพอ · ไม่ใช่หิน · ห่างจุดเข้า · ห่างฝูงอื่น
    /// สุ่มลำดับเพื่อไม่ให้ทุกฝูงไปกองที่จุดแรก ๆ ของไฟล์
    /// </summary>
    private bool TryPickHerdHome(List<Point2> tiles, int minInland, float minEntry, WorldPosition entry, bool allowBeach, out WorldPosition home)
    {
        home = default;
        HerdConfig cfg = HerdCfg;
        float sep = cfg.SeparationTiles * 200f;
        int start = _rng.Next(tiles.Count);
        // รอบแรกใช้ระยะห่างเต็ม · ถ้าจุดในกลุ่มนี้ชิดฝูงอื่นหมด (เช่น หาดแคบ) ยอมลดระยะครึ่งหนึ่งแทนที่จะไม่มีฝูง
        for (int pass = 0; pass < 2; pass++)
        {
            float sepSq = (pass == 0 ? sep : sep * 0.5f);
            sepSq *= sepSq;
            for (int i = 0; i < tiles.Count; i++)
            {
                Point2 tile = tiles[(start + i) % tiles.Count];
                var cand = new WorldPosition(tile.x * 200f + 100f, tile.y * 200f + 100f);
                if (minEntry > 0f && DistSq(cand, entry) < minEntry * minEntry) { continue; }
                if (!_world.Terrain.IsLand(cand.x, cand.y, minInland, allowBeach)) { continue; }
                if (!IsSpawnSpotClear(cand)) { continue; }
                bool tooClose = false;
                for (int h = 0; h < _herds.Count; h++)
                {
                    if (DistSq(_herds[h].Anchor, cand) < sepSq) { tooClose = true; break; }
                }
                if (tooClose) { continue; }
                home = cand;
                return true;
            }
        }
        return false;
    }

    /// <summary>เกิดสมาชิก 1 ตัวรอบบ้านฝูง (ไม่บังคับห่างตัวอื่น — ฝูงต้องกระจุก)</summary>
    private bool TrySpawnHerdMember(Herd herd, double now, out ServerAnimal born)
    {
        born = null;
        float radius = HerdCfg.RadiusTiles * 200f;
        WorldPosition pos = default;
        bool ok = false;
        for (int t = 0; t < 24 && !ok; t++)
        {
            pos = t == 0 ? herd.Home : RandomAround(herd.Home, radius);
            ok = IsSafeLand(pos.x, pos.y, herd.MinInland, herd.Group == "beach") && IsSpawnSpotClear(pos);
        }
        if (!ok)
        {
            herd.SpawnFailures++;
            return false;
        }
        float scale = AnimalData.TryGet(herd.Type, out AnimalData.AnimalInfo info) ? info.Scale : 1f;
        int level = PickHerdLevel(herd.Type);
        var animal = new ServerAnimal(
            "animal_" + Guid.NewGuid().ToString("N").Substring(0, 12),
            herd.Type, level, scale, herd.Home, SpawnTable.LifeFor(herd.Type, level), now)
        {
            HerdId = herd.Id,
            BeachOk = herd.Group == "beach",
            MinInland = herd.MinInland,
        };
        animal.SetPosition(pos, (float)(_rng.NextDouble() * 360.0));
        animal.NextMoveAt = now + NextInterval();
        animal.Height = GroundHeightAt(pos);
        lock (_lock)
        {
            _animals[animal.EntityId] = animal;
            _herdOf[animal.EntityId] = herd;
            herd.Members.Add(animal.EntityId);
        }
        born = animal;
        return true;
    }

    private int AliveMembers(Herd herd)
    {
        int n = 0;
        lock (_lock)
        {
            foreach (string id in herd.Members)
            {
                if (_animals.TryGetValue(id, out ServerAnimal a) && a.IsAlive) { n++; }
            }
        }
        return n;
    }

    private bool TryGetHerd(string animalId, out Herd herd)
    {
        lock (_lock)
        {
            return _herdOf.TryGetValue(animalId ?? string.Empty, out herd);
        }
    }

    /// <summary>สัตว์ตัวนี้เป็นสมาชิกฝูง — ตาย/ติดหิน/หมดอายุ ให้เติมกลับที่ฝูง ไม่ใช่เกิดที่อื่น</summary>
    private bool QueueHerdRefill(string animalId, double at)
    {
        lock (_lock)
        {
            if (!_herdOf.TryGetValue(animalId ?? string.Empty, out Herd herd))
            {
                return false;
            }
            herd.PendingAt.Add(at);
            return true;
        }
    }

    /// <summary>ถอดสมาชิกออกจากฝูง (เรียกจาก Remove) — ต้องอยู่ใต้ _lock</summary>
    private void ForgetHerdMember(string animalId)
    {
        if (_herdOf.TryGetValue(animalId, out Herd herd))
        {
            herd.Members.Remove(animalId);
            _herdOf.Remove(animalId);
        }
    }

    /// <summary>
    /// ทุก 5 วิ: เติมสมาชิกที่ถึงเวลา + ขยับบ้านฝูง (drift) — แทน MaintainQuota ของโหมดเดิม
    /// </summary>
    private void MaintainHerds(double now)
    {
        if (now < _nextHerdCheckAt)
        {
            return;
        }
        _nextHerdCheckAt = now + 5.0;
        HerdConfig cfg = HerdCfg;

        for (int h = 0; h < _herds.Count; h++)
        {
            Herd herd = _herds[h];

            // เติมฝูง
            List<double> pending = herd.PendingAt;
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                if (now < pending[i]) { continue; }
                // นับเฉพาะตัวที่ยังมีชีวิต — ซากยังอยู่ใน Members จน CorpseSeconds (150 วิ) แต่คิวเติมมาถึงที่ 60 วิ
                // ถ้านับซากด้วย ฝูงจะ "เต็ม" ตลอดแล้วไม่เคยเติม (reviewer จับได้)
                if (AliveMembers(herd) >= herd.Size)
                {
                    pending.RemoveAt(i);          // ฝูงเต็มแล้ว (เช่น admin เสกเพิ่ม) ทิ้งคิว
                    continue;
                }
                if (TrySpawnHerdMember(herd, now, out ServerAnimal born))
                {
                    pending.RemoveAt(i);
                    _world.AnnounceAnimal(born);
                    Console.WriteLine("[herd] เติมฝูง #{0} {1} ({2}) — มี {3}/{4}", herd.Id, herd.Name, born.EntityId, herd.Members.Count, herd.Size);
                }
                else
                {
                    pending[i] = now + 15.0;      // รอบบ้านแน่น ลองใหม่
                }
            }

            // ขยับบ้าน — สมาชิกเดินสุ่มรอบ Home อยู่แล้ว เปลี่ยน Home ก็ค่อย ๆ ตามไปเอง
            if (cfg.DriftTiles > 0f && now >= herd.NextDriftAt)
            {
                herd.NextDriftAt = now + RandomDrift(cfg);
                WorldPosition next = default;
                bool ok = false;
                for (int t = 0; t < 12 && !ok; t++)
                {
                    next = RandomAround(herd.Anchor, cfg.DriftTiles * 200f);
                    bool beach = herd.Group == "beach";
                    ok = IsSafeLand(next.x, next.y, herd.MinInland, beach) && IsSpawnSpotClear(next)
                         && PathIsClear(herd.Home, next, herd.MinInland, beach);
                }
                if (ok)
                {
                    herd.Home = next;
                    lock (_lock)
                    {
                        foreach (string id in herd.Members)
                        {
                            if (_animals.TryGetValue(id, out ServerAnimal a)) { a.Home = next; }
                        }
                    }
                }
            }
        }
    }

    /// <summary>สรุปสถานะฝูงสำหรับ admin/log</summary>
    public string DescribeHerds()
    {
        if (!_herdMode) { return "โหมดตาราง Spawn (ไม่มีฝูง)"; }
        int total = 0, pending = 0;
        foreach (Herd h in _herds) { total += AliveMembers(h); pending += h.PendingAt.Count; }
        return $"template {_template?.Name} · {_herds.Count} ฝูง · {total} ตัว · รอเติม {pending}";
    }
}
