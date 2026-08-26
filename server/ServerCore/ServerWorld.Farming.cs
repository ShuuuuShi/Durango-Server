using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ServerWorld.Farming — สถานะแปลงผักของทั้งโลก
//
// แปลงผัก = สิ่งปลูกสร้างที่ blueprint มี component "Growable" (farm_tile_01..04)
// สถานะการปลูกไม่ได้อยู่ใน AppearArtifact โดยตรง — เก็บเป็น FarmPlot แยก
// แล้ว "ฉาย" ลงไปที่ `States.Farming` / `Display.Crop` ของ artifact ทุกครั้งที่เปลี่ยน
// เพื่อให้คนที่เพิ่งเข้ามาเห็นต้นไม้ที่ปลูกไว้ด้วย (AppearArtifact พา States ไปด้วยอยู่แล้ว)

public partial class ServerWorld
{
    /// <summary>แปลงผัก 1 แปลง (1 artifact = 1 แปลง)</summary>
    public sealed class FarmPlot
    {
        public string ArtifactId;
        public int TileX;
        public int TileY;

        public string SeedId;
        public int SeedLevel = 1;

        public double PlantedAt;
        public double GrowsUntil;

        public float Water;
        public float Fertilizer;

        /// <summary>ความเหมาะสมของไบโอม — ตัดสินตอนปลูก ไม่เปลี่ยนอีก</summary>
        public Fitness Fitness = Fitness.Normal;

        /// <summary>คิดผลตอนโตครบแล้วหรือยัง (ตาย/ออกผล)</summary>
        public bool Resolved;
        public bool Dead;

        /// <summary>ชื่อสไปรต์ที่ client เอาไปวาด (ต้นอ่อน → ต้นโต → ต้นตาย)</summary>
        public string Look;

        /// <summary>ผลผลิตที่ยังไม่ได้เก็บ — เติมจาก generator จริงตอนเซฟ (ดู FarmSave.From)</summary>
        public int RemainProduct;
        public int RemainSeed;
    }

    private readonly Dictionary<string, FarmPlot> _farms = new Dictionary<string, FarmPlot>(StringComparer.Ordinal);
    private readonly object _farmLock = new object();
    private double _nextFarmTick;

    /// <summary>ตรวจแปลงผักทุกกี่วินาที — พืชโตช้ากว่านี้มาก ไม่ต้องถี่</summary>
    private const double FarmTickSeconds = 1.0;

    public FarmPlot[] SnapshotFarms()
    {
        lock (_farmLock)
        {
            var arr = new FarmPlot[_farms.Count];
            _farms.Values.CopyTo(arr, 0);
            return arr;
        }
    }

    public bool TryGetFarm(string artifactId, out FarmPlot plot)
    {
        lock (_farmLock)
        {
            return _farms.TryGetValue(artifactId ?? string.Empty, out plot);
        }
    }

    public int FarmCount
    {
        get
        {
            lock (_farmLock)
            {
                return _farms.Count;
            }
        }
    }

    /// <summary>สิ่งปลูกสร้างชิ้นนี้เป็นแปลงผักไหม (blueprint มี component Growable)</summary>
    public bool IsFarmArtifact(string artifactId)
    {
        string blueprintId;
        lock (_artifactLock)
        {
            if (!_artifactBlueprints.TryGetValue(artifactId ?? string.Empty, out blueprintId))
            {
                return false;
            }
        }
        return IsFarmBlueprint(blueprintId);
    }

    public static bool IsFarmBlueprint(string blueprintId)
    {
        return !string.IsNullOrEmpty(blueprintId)
               && RecipeData.BlueprintComponents.TryGetValue(blueprintId, out string[] comps)
               && Array.IndexOf(comps, "Growable") != -1;
    }

    /// <summary>
    /// ลงเมล็ด — คืน false ถ้าแปลงนี้มีต้นอยู่แล้ว
    /// (ผู้เรียกต้องตรวจสิทธิ์/ระยะ/ของในกระเป๋ามาก่อน)
    /// </summary>
    public bool PlantSeedOn(string artifactId, Point2 tile, CropData.CropInfo crop, int seedLevel,
                            double now, out FarmPlot plot)
    {
        plot = null;
        if (string.IsNullOrEmpty(artifactId))
        {
            return false;
        }
        Fitness fitness = FitnessAt(tile, crop);
        float scale = ServerConfig.Current.Farming.GrowthScale;
        float seconds = crop.GrowSecondsAt(seedLevel) * (scale <= 0f ? 1f : scale);
        if (fitness == Fitness.Bad)
        {
            seconds *= Math.Max(1f, ServerConfig.Current.Farming.WrongBiomeGrowthPenalty);
        }
        float min = ServerConfig.Current.Farming.MinGrowSeconds;
        if (seconds < min)
        {
            seconds = min;
        }

        var made = new FarmPlot
        {
            ArtifactId = artifactId,
            TileX = tile.x,
            TileY = tile.y,
            SeedId = crop.SeedId,
            SeedLevel = seedLevel < 1 ? 1 : seedLevel,
            PlantedAt = now,
            GrowsUntil = now + seconds,
            Water = 0f,
            Fertilizer = 0f,
            Fitness = fitness,
            Resolved = false,
            Dead = false,
            Look = crop.GrowingLook
        };
        lock (_farmLock)
        {
            if (_farms.ContainsKey(artifactId))
            {
                return false;
            }
            _farms[artifactId] = made;
        }
        plot = made;
        ApplyFarmToArtifact(made);
        MarkDirty();
        return true;
    }

    /// <summary>ถอนต้นทิ้ง (หรือเก็บเกี่ยวหมดแล้ว) — แปลงกลับเป็นแปลงเปล่า</summary>
    public bool ClearFarm(string artifactId)
    {
        bool removed;
        lock (_farmLock)
        {
            removed = _farms.Remove(artifactId ?? string.Empty);
        }
        if (!removed)
        {
            return false;
        }
        lock (_genLock)
        {
            _generators.Remove(artifactId ?? string.Empty);
        }
        ClearFarmOnArtifact(artifactId);
        MarkDirty();
        return true;
    }

    /// <summary>ไบโอมตรงนี้เหมาะกับพืชชนิดนี้แค่ไหน</summary>
    public Fitness FitnessAt(Point2 tile, CropData.CropInfo crop)
    {
        if (crop.Preference == Shared.Region.Biome.Invalid)
        {
            return Fitness.Normal;      // พืชที่ไม่เลือกที่ ปลูกตรงไหนก็เหมือนกัน
        }
        Shared.Region.Biome here = Terrain.BiomeAt(tile.x, tile.y);
        return here == crop.Preference ? Fitness.Excelent : Fitness.Bad;
    }

    // ---------------------------------------------------------------- เดินเวลา

    /// <summary>เรียกทุกเฟรมจาก ProcessPlayers — คิดผลของต้นที่โตครบแล้ว</summary>
    public void TickFarms(double now)
    {
        if (now < _nextFarmTick)
        {
            return;
        }
        _nextFarmTick = now + FarmTickSeconds;

        FarmPlot[] plots = SnapshotFarms();
        for (int i = 0; i < plots.Length; i++)
        {
            FarmPlot p = plots[i];
            if (p.Resolved || now < p.GrowsUntil)
            {
                continue;
            }
            ResolveGrowth(p, now);
        }
    }

    /// <summary>
    /// ต้นโตครบแล้ว — ตัดสินว่ารอดหรือตาย แล้วตั้ง generator ให้เก็บเกี่ยว
    ///
    /// เกณฑ์น้ำ (ค่า Survivability ของพืชเป็นตัวกำหนดความใจดี):
    ///   น้ำครบ                          → รอดเต็ม ได้ผลผลิตตามปุ๋ย
    ///   น้ำ ≥ ที่ต้องการ × Survivability → รอดแบบไม่สมบูรณ์ ได้แค่ 1 ชิ้น
    ///   น้อยกว่านั้น                     → ตาย ต้องถอนทิ้ง
    /// </summary>
    private void ResolveGrowth(FarmPlot plot, double now)
    {
        if (!CropData.TryGet(plot.SeedId, out CropData.CropInfo crop))
        {
            plot.Resolved = true;
            plot.Dead = true;
            return;
        }
        FarmingConfig cfg = ServerConfig.Current.Farming;
        float needWater = crop.RequiredWater;
        bool healthy = plot.Water >= needWater;
        bool alive = healthy || plot.Water >= needWater * Math.Min(1f, Math.Max(0f, crop.Survivability));

        plot.Resolved = true;
        plot.Dead = !alive;
        plot.Look = alive ? crop.GrownLookFor(plot.TileX, plot.TileY) : crop.DeadLook;

        if (!alive)
        {
            Console.WriteLine("[farm] {0} ({1}) ตายเพราะน้ำไม่พอ — ได้ {2:F1}/{3}",
                plot.ArtifactId, crop.SeedId, plot.Water, needWater);
            ApplyFarmToArtifact(plot);
            MarkDirty();
            return;
        }

        int amount = 1;
        if (healthy && crop.AdditionalProduct > 0 && crop.RequiredFertilizer > 0)
        {
            float ratio = Math.Min(1f, plot.Fertilizer / crop.RequiredFertilizer);
            amount += (int)Math.Round(crop.AdditionalProduct * ratio * Math.Max(0f, cfg.FertilizerYieldScale));
        }
        if (plot.Fitness == Fitness.Bad)
        {
            amount = Math.Max(1, amount / 2);       // ผิดไบโอม = ได้ครึ่งเดียว
        }

        SetHarvestGenerators(plot, crop, amount, cfg.SeedYield);

        Console.WriteLine("[farm] {0} ({1}) โตแล้ว — {2} x{3}{4}{5}",
            plot.ArtifactId, crop.SeedId, crop.ProductId, amount,
            healthy ? "" : " (น้ำไม่ครบ ได้แค่ขั้นต่ำ)",
            plot.Fitness == Fitness.Bad ? " (ผิดไบโอม)" : "");
        ApplyFarmToArtifact(plot);
        MarkDirty();
    }

    /// <summary>ตั้ง generator ของที่เก็บเกี่ยวได้ — จำนวน 0 = ไม่ใส่ชิ้นนั้น</summary>
    private void SetHarvestGenerators(FarmPlot plot, CropData.CropInfo crop, int productAmount, int seedAmount)
    {
        float duration = Math.Max(0.5f, ServerConfig.Current.Farming.HarvestSeconds);
        var gens = new List<Generator>();
        if (productAmount > 0 && !string.IsNullOrEmpty(crop.ProductId))
        {
            gens.Add(new Generator
            {
                Id = crop.ProductId,
                Name = crop.ProductName ?? crop.Name ?? crop.ProductId,
                Icon = crop.ProductIcon ?? crop.Icon,
                Amount = productAmount,
                Effort = 1f,
                Duration = duration,
                ToolRequirements = new Dictionary<string, int>(),
                Enabled = true
            });
        }
        if (seedAmount > 0 && !string.IsNullOrEmpty(crop.SeedProductId))
        {
            gens.Add(new Generator
            {
                Id = crop.SeedProductId,
                Name = crop.SeedProductName ?? crop.SeedProductId,
                Icon = crop.SeedProductIcon ?? "icon_nat_farm_seed",
                Amount = seedAmount,
                Effort = 1f,
                Duration = duration,
                ToolRequirements = new Dictionary<string, int>(),
                Enabled = true
            });
        }
        SetGenerators(plot.ArtifactId, gens);
    }

    // ---------------------------------------------------------------- ฉายสถานะลง artifact

    /// <summary>สร้าง <see cref="Messages.Farming"/> จากแปลง — คือสิ่งที่ client เอาไปวาดหลอดเวลา/น้ำ/ปุ๋ย</summary>
    public Farming MakeFarmingState(FarmPlot plot)
    {
        CropData.TryGet(plot.SeedId, out CropData.CropInfo crop);
        float needFert = crop.RequiredFertilizer;
        return new Farming
        {
            PlantName = crop.Name ?? plot.SeedId,
            PlantedAt = plot.PlantedAt,
            GrowsUntil = plot.GrowsUntil,
            Water = new UnityEngine.Vector2(plot.Water, crop.RequiredWater),
            BiomeFitness = plot.Fitness,
            FertilizedRatio = needFert <= 0f ? 1f : Math.Min(1f, plot.Fertilizer / needFert),
            FertilizerAmount = plot.Fertilizer,
            RequiredFertilizer = crop.RequiredFertilizer,
            AppliedCropBooster = null,
            BoosterLevel = 0,
            RapidGrowthCost = null      // ยังไม่มีระบบเร่งโตด้วยเจม
        };
    }

    /// <summary>เขียนสถานะแปลงลงใน artifact ที่เก็บไว้ แล้วบอกทุกคนที่มองเห็น</summary>
    public void ApplyFarmToArtifact(FarmPlot plot)
    {
        if (plot == null)
        {
            return;
        }
        Farming state = MakeFarmingState(plot);
        ArtifactDisplay display;
        lock (_artifactLock)
        {
            if (!_artifacts.TryGetValue(plot.ArtifactId, out AppearArtifact a))
            {
                return;
            }
            ArtifactState states = a.States;
            states.Farming = state;
            a.States = states;

            display = a.Display;
            display.Crop = plot.Look;
            a.Display = display;

            _artifacts[plot.ArtifactId] = a;
        }
        BroadcastToViewers(plot.ArtifactId, MakeArtifactStateMessage(plot.ArtifactId));
        BroadcastToViewers(plot.ArtifactId, display);
    }

    private void ClearFarmOnArtifact(string artifactId)
    {
        ArtifactDisplay display;
        lock (_artifactLock)
        {
            if (!_artifacts.TryGetValue(artifactId ?? string.Empty, out AppearArtifact a))
            {
                return;
            }
            ArtifactState states = a.States;
            states.Farming = null;
            a.States = states;

            display = a.Display;
            display.Crop = null;
            a.Display = display;

            _artifacts[artifactId] = a;
        }
        BroadcastToViewers(artifactId, MakeArtifactStateMessage(artifactId));
        BroadcastToViewers(artifactId, display);
    }

    /// <summary>สถานะปัจจุบันของ artifact ชิ้นนี้ในรูป packet (client เอาไปทับของเดิม)</summary>
    private ArtifactState MakeArtifactStateMessage(string artifactId)
    {
        lock (_artifactLock)
        {
            return _artifacts.TryGetValue(artifactId ?? string.Empty, out AppearArtifact a)
                ? a.States
                : new ArtifactState { EntityId = artifactId };
        }
    }

    // ---------------------------------------------------------------- เซฟ/โหลด

    public void LoadFarms(List<FarmSave> saves)
    {
        if (saves == null)
        {
            return;
        }
        lock (_farmLock)
        {
            _farms.Clear();
            for (int i = 0; i < saves.Count; i++)
            {
                FarmPlot p = saves[i].ToPlot();
                if (p == null || string.IsNullOrEmpty(p.ArtifactId))
                {
                    continue;
                }
                // เมล็ดที่ไม่มีในตารางแล้ว (ข้อมูลเกมเปลี่ยน) — ทิ้งไปเลย ไม่งั้นแปลงค้างถอนไม่ออก
                if (!CropData.IsSeed(p.SeedId))
                {
                    Console.WriteLine("[farm] ข้ามแปลง {0}: ไม่รู้จักเมล็ด {1}", p.ArtifactId, p.SeedId);
                    continue;
                }
                _farms[p.ArtifactId] = p;
            }
        }
        // ฉายลง artifact ที่โหลดมาแล้ว + ต้นที่โตครบระหว่างเซิร์ฟปิดอยู่ให้คิดผลทันที
        double now = Times.UnixTimeNow();
        FarmPlot[] plots = SnapshotFarms();
        for (int i = 0; i < plots.Length; i++)
        {
            FarmPlot p = plots[i];
            if (!p.Resolved && now >= p.GrowsUntil)
            {
                ResolveGrowth(p, now);       // โตครบตอนเซิร์ฟปิดอยู่
                continue;
            }
            if (p.Resolved && !p.Dead)
            {
                // ⚠️ ห้ามเรียก ResolveGrowth ซ้ำ — จะได้ผลผลิตเต็มจำนวนใหม่ทุกครั้งที่รีสตาร์ท
                // ต้องใช้จำนวนที่ "เหลือจริง" ที่เซฟไว้ตอนปิด
                if (p.RemainProduct <= 0 && p.RemainSeed <= 0)
                {
                    ClearFarm(p.ArtifactId);     // เก็บหมดแล้วก่อนปิดเซิร์ฟ = แปลงเปล่า
                    continue;
                }
                if (CropData.TryGet(p.SeedId, out CropData.CropInfo crop))
                {
                    SetHarvestGenerators(p, crop, p.RemainProduct, p.RemainSeed);
                }
            }
            ApplyFarmToArtifact(p);
        }
        int left = FarmCount;
        if (left > 0)
        {
            Console.WriteLine("[farm] โหลดแปลงผัก {0} แปลง", left);
        }
    }
}
