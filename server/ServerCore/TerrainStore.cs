using System;
using System.Collections.Generic;
using System.IO;
using Durango.Terrain;
using Newtonsoft.Json;

namespace DurangoServer.Core;

// โหลดและเก็บข้อมูล terrain (ไบโอม/น้ำ/แม่น้ำ/ของธรรมชาติ) จาก data\terrains\extracted\<id>
// ขนาด 1 tile = 16 px; 1 chunk = 16x16 tile (ChunkData = 324 biomes + 289 ocean + 867 river + landmarks)
public class TerrainStore
{
    public const int TileSize = 16;
    public const int BiomesPerChunk = 324;

    public string TerrainId { get; }
    public TerrainInfoJson Info { get; }
    public byte[] Biomes { get; }
    public byte[] Ocean { get; }
    public byte[] Rivers { get; }
    public byte[] Garden { get; private set; }
    public byte[] Elevations { get; }

    /// <summary>
    /// `oceans.dm` — **ระยะห่างจากชายฝั่งแบบมีเครื่องหมาย** 1 ไบต์ต่อ tile (อ่านเป็น signed −32..+32)
    ///   บวก = แผ่นดิน (ยิ่งมากยิ่งลึกเข้าไปในเกาะ) · ลบ = ทะเล · 0 = ริมน้ำพอดี
    ///
    /// พิสูจน์แล้วด้วยข้อมูลของเกมเอง: เอาพิกัดต้นไม้/ของธรรมชาติทั้งหมดใน `whole.garden` มาเทียบ
    /// **2,443 จุดอยู่บนค่าบวก · 219 จุดอยู่บนค่าลบ** (ที่ติดลบเป็น −1/−2 = พืชริมหาด)
    /// ⇒ ค่าบวกคือพื้นดินแน่นอน
    ///
    /// จุดเข้าเกม (40,177) = −3 คือ "จุดที่เรือมาจอด" อยู่ในน้ำตื้นหน้าหาด
    /// เพราะงั้นห้ามใช้จุดเข้าเกมเป็นศูนย์กลางการเกิดสัตว์ตรง ๆ
    /// </summary>
    public byte[] LandMap { get; private set; }

    /// <summary>
    /// tile นี้ลึกเข้าไปในแผ่นดินกี่ tile (ติดลบ = อยู่ในทะเล)
    /// ไม่มีข้อมูล = คืน 99 (ถือว่าเป็นบก จะได้ไม่พังกับ terrain ที่ไม่มีไฟล์นี้)
    /// </summary>
    public int LandDistance(int tileX, int tileY)
    {
        if (LandMap == null || LandMap.Length < Width * Height)
        {
            return 99;
        }
        if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
        {
            return -99;              // นอกแมพ = ไม่ใช่ที่ที่จะให้อะไรไปยืน
        }
        int v = LandMap[tileX + tileY * Width];
        return v > 127 ? v - 256 : v;
    }

    /// <summary>
    /// ไบโอมของ tile นี้ — **ถอดรหัสได้แล้ว** (`Shared.Region.Biome` ในซอร์ส client)
    ///
    /// 1 ไบต์ต่อ tile: **6 บิตล่าง = ชนิดไบโอม · 2 บิตบน = ธง** (เจอค่า 0x40 / 0xC0 ปนมาด้วย)
    /// ถ้าไม่มาสก์บิตบนออก จะเห็นเป็นเลขแปลก ๆ อย่าง 192/202/205 แล้วตีความไม่ออก
    ///
    /// ตัวอย่างที่นับได้จริง (ri35te 256×256): ทะเลอุ่น 57.2% · ป่าเขตอบอุ่น 23.8% ·
    /// หาดทราย 9.5% · แม่น้ำ 6.1% · ทะเลสาบ 3.4%
    /// </summary>
    public Shared.Region.Biome BiomeAt(int tileX, int tileY)
    {
        if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height || Biomes.Length < Width * Height)
        {
            return Shared.Region.Biome.Invalid;
        }
        return (Shared.Region.Biome)(Biomes[tileX + tileY * Width] & 0x3F);
    }

    /// <summary>
    /// ไบโอมที่สัตว์บกไม่ควรเกิด — น้ำทุกชนิดกับชายหาด
    /// (แม่น้ำ/ทะเลสาบก็นับ เพราะสัตว์ยืนกลางน้ำดูพอ ๆ กับยืนกลางทะเล)
    /// </summary>
    private static bool IsWaterOrBeach(Shared.Region.Biome b)
    {
        switch (b)
        {
            case Shared.Region.Biome.WarmOcean:
            case Shared.Region.Biome.ColdOcean:
            case Shared.Region.Biome.River:
            case Shared.Region.Biome.Lake:
            case Shared.Region.Biome.Lava:
            case Shared.Region.Biome.SandBeach:
            case Shared.Region.Biome.PebbleBeach:
                return true;
            default:
                return false;
        }
    }

    /// <summary>สรุปสัดส่วนไบโอมของเกาะนี้ (พิมพ์ตอนเปิดเซิร์ฟ ไว้ดูว่าเกาะหน้าตายังไง)</summary>
    private void ReportBiomes()
    {
        if (Biomes.Length < Width * Height)
        {
            return;
        }
        var count = new Dictionary<Shared.Region.Biome, int>();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Shared.Region.Biome b = BiomeAt(x, y);
                count[b] = count.TryGetValue(b, out int n) ? n + 1 : 1;
            }
        }
        var parts = new List<string>();
        foreach (var pair in count)
        {
            parts.Add($"{pair.Key} {100.0 * pair.Value / (Width * Height):F1}%");
        }
        parts.Sort();
        Console.WriteLine("[terrain] ไบโอมบนเกาะ: {0}", string.Join(" · ", parts));
    }

    /// <summary>
    /// ตรงนี้ให้สัตว์เกิด/เดินได้ไหม — ต้องผ่านทั้ง 2 ด่าน
    ///   1. ไม่ใช่ไบโอมทะเล/ชายหาด
    ///   2. ลึกเข้าไปในแผ่นดินอย่างน้อย minTilesInland tile
    /// </summary>
    public bool IsLand(float worldX, float worldY, int minTilesInland)
    {
        int tx = (int)(worldX / 200f);
        int ty = (int)(worldY / 200f);
        if (LandDistance(tx, ty) < minTilesInland)
        {
            return false;
        }
        return !IsWaterOrBeach(BiomeAt(tx, ty));
    }

    /// <summary>อ่านความสูงพื้นจาก whole.elevations (1 byte ต่อ tile)</summary>
    public bool TryGetGroundHeight(float worldX, float worldY, out float height)
    {
        height = 0f;
        if (Elevations == null || Elevations.Length < Width * Height)
        {
            return false;
        }
        int tileX = (int)Math.Floor(worldX / 200f);
        int tileY = (int)Math.Floor(worldY / 200f);
        if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
        {
            return false;
        }
        height = Elevations[tileX + tileY * Width];
        return true;
    }

    /// <summary>ระยะห่างชายฝั่งที่ลึกที่สุดบนเกาะนี้ (คำนวณครั้งเดียว)</summary>
    private int _deepestInland = -1;
    private float _deepestX;
    private float _deepestY;

    /// <summary>
    /// จุดที่ "กลางเกาะที่สุด" — tile บนบกที่ห่างชายฝั่งมากที่สุด
    ///
    /// ใช้เป็นทางออกสุดท้ายให้โซนของสัตว์ตัวใหญ่ ถ้าจุดที่ตั้งไว้ใน config
    /// หาที่ลึกพอไม่เจอ จะได้ไม่ตกไปอยู่ริมหาดเพราะ "หาไม่เจอเลยใช้จุดสุดท้ายที่สุ่มได้"
    /// เสมอกันหลายจุด → เลือกจุดที่ใกล้กึ่งกลางแมพที่สุด (เกาะส่วนใหญ่มีที่ราบสูงตรงกลาง)
    /// </summary>
    public bool TryDeepestLand(out float worldX, out float worldY, out int inlandTiles)
    {
        if (_deepestInland < 0)
        {
            int best = 0;
            float bx = 0f, by = 0f;
            double bestCenterDist = double.MaxValue;
            float cx = Width * 0.5f, cy = Height * 0.5f;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int d = LandDistance(x, y);
                    if (d < best || IsWaterOrBeach(BiomeAt(x, y)))
                    {
                        continue;
                    }
                    double cd = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                    if (d > best || cd < bestCenterDist)
                    {
                        best = d;
                        bestCenterDist = cd;
                        bx = (x + 0.5f) * 200f;
                        by = (y + 0.5f) * 200f;
                    }
                }
            }
            _deepestInland = best;
            _deepestX = bx;
            _deepestY = by;
        }
        worldX = _deepestX;
        worldY = _deepestY;
        inlandTiles = _deepestInland;
        return _deepestInland > 0;
    }

    public int Width { get; }
    public int Height { get; }
    public int NumChunksX { get; }
    public int NumChunksY { get; }

    private readonly Dictionary<(int, int), byte[]> _landmarkChunks = new Dictionary<(int, int), byte[]>();

    public Point2 EntryPoint =>
        Info?.entry_points != null && Info.entry_points.Length > 0 && Info.entry_points[0]?.Length >= 2
            ? new Point2(Info.entry_points[0][0], Info.entry_points[0][1])
            : new Point2(Width / 2, Height / 2);

    private TerrainStore(string terrainId, string dir)
    {
        TerrainId = terrainId;
        Biomes = LoadOrEmpty(dir, "whole.biomes");
        Ocean = LoadOrEmpty(dir, "whole.ocean");
        // ✅ ไฟล์ที่ใช้แยกบก/ทะเลได้จริงคือ `oceans.dm` ไม่ใช่ `whole.ocean`
        // (whole.ocean ตีความไม่สำเร็จ — ดู WaterDepthAt ข้างล่าง)
        LandMap = LoadOrEmpty(dir, "oceans.dm");
        Rivers = LoadOrEmpty(dir, "whole.rivers");
        Garden = LoadOrEmpty(dir, "whole.garden");
        Elevations = LoadOrEmpty(dir, "whole.elevations");

        Info = new TerrainInfoJson();
        string infoPath = Path.Combine(dir, "info.yml");
        if (File.Exists(infoPath))
        {
            try
            {
                Info = JsonConvert.DeserializeObject<TerrainInfoJson>(File.ReadAllText(infoPath)) ?? Info;
            }
            catch (Exception e)
            {
                Console.WriteLine("[terrain] info.yml parse failed: " + e.Message);
            }
        }
        if (Info.tile_count == null || Info.tile_count.Length < 2)
        {
            Info.tile_count = new[] { 256, 256 };
        }
        Width = Info.tile_count[0];
        Height = Info.tile_count[1];
        NumChunksX = Width / TileSize;
        NumChunksY = Height / TileSize;

        if (Biomes.Length < Width * Height)
        {
            Biomes = new byte[Width * Height];
        }
        int vCount = (Width + 1) * (Height + 1);
        if (Ocean.Length < vCount)
        {
            Ocean = new byte[vCount];
        }
        if (Rivers.Length < vCount * 3)
        {
            Rivers = new byte[vCount * 3];
        }
        if (Elevations.Length < Width * Height)
        {
            Console.WriteLine("[terrain] whole.elevations missing or too short; animal height lookup will use client hint");
        }
        else
        {
            Console.WriteLine("[terrain] loaded whole.elevations ({0} tiles)", Elevations.Length);
        }

        BuildLandmarkChunks(dir);
        ReportBiomes();                 // ต้องเรียกหลัง Width/Height/Biomes พร้อมแล้ว
    }

    private void BuildLandmarkChunks(string dir)
    {
        string lmPath = Path.Combine(dir, "whole.landmarks");
        if (!File.Exists(lmPath))
        {
            return;
        }
        byte[] lm = File.ReadAllBytes(lmPath);
        if (lm.Length % 16 != 0)
        {
            return;
        }
        var groups = new Dictionary<(int, int), List<byte[]>>();
        for (int i = 0; i < lm.Length; i += 16)
        {
            ushort x = BitConverter.ToUInt16(lm, i);
            ushort y = BitConverter.ToUInt16(lm, i + 2);
            var key = (x / TileSize, y / TileSize);
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<byte[]>();
                groups[key] = list;
            }
            byte[] rec = new byte[16];
            Array.Copy(lm, i, rec, 0, 16);
            list.Add(rec);
        }
        foreach (var kv in groups)
        {
            byte[] all = new byte[kv.Value.Count * 16];
            for (int i = 0; i < kv.Value.Count; i++)
            {
                Array.Copy(kv.Value[i], 0, all, i * 16, 16);
            }
            _landmarkChunks[kv.Key] = all;
        }
    }

    private static byte[] LoadOrEmpty(string dir, string file)
    {
        string path = Path.Combine(dir, file);
        return File.Exists(path) ? File.ReadAllBytes(path) : Array.Empty<byte>();
    }

    public static TerrainStore Load(string dataDir, string terrainId)
    {
        return new TerrainStore(terrainId, Path.Combine(dataDir, "terrains", "extracted", terrainId));
    }

    public byte[] GetChunkBiomes(int chunkX, int chunkY)
    {
        byte[] dst = new byte[BiomesPerChunk];
        CopyChunk(chunkX, chunkY, Biomes, dst, 1, 1, 1, 18);
        return dst;
    }

    public byte[] GetChunkOcean(int chunkX, int chunkY)
    {
        byte[] dst = new byte[289];
        CopyChunk(chunkX, chunkY, Ocean, dst, 1, 0, 1, 17);
        return dst;
    }

    public byte[] GetChunkRiver(int chunkX, int chunkY)
    {
        byte[] dst = new byte[867];
        CopyChunk(chunkX, chunkY, Rivers, dst, 3, 0, 1, 17);
        return dst;
    }

    public byte[] GetChunkLandmark(int chunkX, int chunkY)
    {
        return _landmarkChunks.TryGetValue((chunkX, chunkY), out var data) ? data : null;
    }

    /// <summary>
    /// M-6: แคช garden แยกตาม chunk
    ///
    /// เดิมทุกครั้งที่ขอ chunk จะสแกน Garden ทั้งก้อน (หลายพัน record) และ SetChunk 1 ครั้ง
    /// ขอถึง 9 chunk ⇒ ผู้เล่นคนเดียวกดเดินไปมาก็ทำ tps ตกได้ทั้งเซิร์ฟ
    /// สร้าง index ครั้งเดียวแล้วล้างเฉพาะตอน Garden เปลี่ยน (เก็บของหมด/ลบต้นไม้)
    /// </summary>
    private Dictionary<(int, int), byte[]> _gardenByChunk;

    public byte[] GetChunkGarden(int chunkX, int chunkY)
    {
        lock (_gardenLock)
        {
            if (Garden.Length % 6 != 0)
            {
                return Array.Empty<byte>();
            }
            if (_gardenByChunk == null)
            {
                _gardenByChunk = BuildGardenIndex();
            }
            return _gardenByChunk.TryGetValue((chunkX, chunkY), out byte[] cached)
                ? cached
                : Array.Empty<byte>();
        }
    }

    /// <summary>ต้องถือ _gardenLock ก่อนเรียก</summary>
    private Dictionary<(int, int), byte[]> BuildGardenIndex()
    {
        var groups = new Dictionary<(int, int), List<byte[]>>();
        for (int i = 0; i < Garden.Length; i += 6)
        {
            ushort x = BitConverter.ToUInt16(Garden, i);
            ushort y = BitConverter.ToUInt16(Garden, i + 2);
            var key = (x / TileSize, y / TileSize);
            if (!groups.TryGetValue(key, out List<byte[]> list))
            {
                list = new List<byte[]>();
                groups[key] = list;
            }
            byte[] rec = new byte[6];
            Array.Copy(Garden, i, rec, 0, 6);
            list.Add(rec);
        }
        var result = new Dictionary<(int, int), byte[]>(groups.Count);
        foreach (KeyValuePair<(int, int), List<byte[]>> kv in groups)
        {
            byte[] all = new byte[kv.Value.Count * 6];
            for (int i = 0; i < kv.Value.Count; i++)
            {
                Array.Copy(kv.Value[i], 0, all, i * 6, 6);
            }
            result[kv.Key] = all;
        }
        return result;
    }

    private readonly object _gardenLock = new object();

    // GP-07: จำพิกัดที่ถูกเก็บจนหมดไปแล้ว เพื่อเซฟลงดิสก์และเอากลับมาใช้ตอนเปิดเซิร์ฟใหม่
    // เก็บเป็น "รายการที่ถูกลบ" แทนการ dump Garden ทั้งก้อน เพราะ Garden derive มาจากไฟล์ terrain
    // ถ้าวันหลังเปลี่ยนแมพ ไฟล์เซฟเก่าก็ยังใช้ได้ (พิกัดไหนไม่มีของก็ข้ามไปเฉย ๆ)
    private readonly HashSet<(int, int)> _removedNaturals = new HashSet<(int, int)>();

    /// <summary>พิกัดที่ถูกเก็บไปแล้วทั้งหมด (สำเนา) — ใช้ตอนเซฟ</summary>
    public List<int[]> GetRemovedNaturals()
    {
        lock (_gardenLock)
        {
            var list = new List<int[]>(_removedNaturals.Count);
            foreach ((int x, int y) in _removedNaturals)
            {
                list.Add(new[] { x, y });
            }
            return list;
        }
    }

    /// <summary>เอารายการที่เคยถูกเก็บไปแล้วมาลบออกจาก Garden ตอนโหลดเซิร์ฟ (GP-07)</summary>
    public int ApplyRemovedNaturals(List<int[]> removed)
    {
        if (removed == null)
        {
            return 0;
        }
        int applied = 0;
        for (int i = 0; i < removed.Count; i++)
        {
            int[] p = removed[i];
            if (p == null || p.Length < 2)
            {
                continue;
            }
            if (RemoveNatural(p[0], p[1]))
            {
                applied++;
            }
            else
            {
                // ไม่มีของตรงนั้นให้ลบแล้ว — ยังจำไว้ว่าเคยถูกเก็บ เผื่อเซฟรอบหน้าจะได้ไม่หาย
                lock (_gardenLock)
                {
                    _removedNaturals.Add((p[0], p[1]));
                }
            }
        }
        return applied;
    }

    /// <summary>
    /// ⚠️ **ยังใช้ไม่ได้ — ตีความค่าใน whole.ocean ไม่สำเร็จ** (เก็บโค้ดไว้เป็นบันทึกการทดลอง)
    ///
    /// ตั้งใจจะใช้กรองไม่ให้สัตว์เกิดกลางทะเล แต่ค่าที่อ่านได้ขัดกับความจริงในเกม:
    ///   จุดที่ผู้เล่นสร้างกองไฟ (49,178) = 36 · จุดเกิด (40,177) = 86 · จุดเก็บหอย (42,182) = 63
    /// ถ้าตีความตาม client (`Durango.Render.Water/WaterData.ByteToDepth`: ค่า &lt;128 = ความลึกทะเล v/127)
    /// จุดพวกนี้จะเป็น "ใต้น้ำลึก 0.28-0.68" ซึ่งเป็นไปไม่ได้เพราะผู้เล่นยืน/สร้างบ้านตรงนั้น
    ///
    /// สมมติฐานที่เหลือ: ไฟล์ที่เราสกัดมาอาจเก็บ "ความสูงพื้น" ไม่ใช่ความลึกน้ำ
    /// หรือ index ไม่ใช่ x + y*257 — ต้องหาหลักฐานเพิ่มก่อนเอาไปใช้จริง
    /// </summary>
    public int WaterDepthAt(int tileX, int tileY)
    {
        if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
        {
            return 0;
        }
        int side = Width + 1;                 // ข้อมูลเป็นราย vertex จึงมีขอบเกินมา 1
        if (Ocean.Length < side * side)
        {
            return 0;
        }
        int worst = 0;
        for (int dy = 0; dy <= 1; dy++)
        {
            for (int dx = 0; dx <= 1; dx++)
            {
                int vx = Math.Clamp(tileX + dx, 0, side - 1);
                int vy = Math.Clamp(tileY + dy, 0, side - 1);
                int v = Ocean[vx + vy * side];
                if (v > worst)
                {
                    worst = v;
                }
            }
        }
        return worst;
    }

    /// <summary>tile นี้อยู่บนบกไหม (ใช้ตอนหาจุดเกิดของสัตว์ — ไม่งั้นได้ไดโนเสาร์ลอยกลางทะเล)</summary>
    public bool IsLand(int tileX, int tileY)
    {
        return WaterDepthAt(tileX, tileY) == 0;
    }

    /// <summary>
    /// GP-09: มีของธรรมชาติอยู่ที่ tile นี้จริงไหม (และเป็นชนิดอะไร)
    /// รูปแบบ record ของ garden = x:u16, y:u16, entityType:u16 (ตรงกับ Durango.Terrain.NaturalInfo ฝั่ง client)
    /// </summary>
    public bool TryGetNatural(int tileX, int tileY, out ushort entityType)
    {
        entityType = 0;
        if (tileX < 0 || tileY < 0 || tileX >= Width || tileY >= Height)
        {
            return false;
        }
        lock (_gardenLock)
        {
            if (Garden.Length % 6 != 0)
            {
                return false;
            }
            for (int i = 0; i < Garden.Length; i += 6)
            {
                if (BitConverter.ToUInt16(Garden, i) == tileX && BitConverter.ToUInt16(Garden, i + 2) == tileY)
                {
                    entityType = BitConverter.ToUInt16(Garden, i + 4);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>หาของธรรมชาติที่ใกล้จุดนี้ที่สุดภายในรัศมี (หน่วย tile) — ใช้กับรีโมทคุมตัวละคร</summary>
    public bool TryFindNaturalNear(WorldPosition pos, int radiusTiles, out Point2 tile, out ushort entityType)
    {
        tile = default;
        entityType = 0;
        float px = pos.x / 200f;
        float py = pos.y / 200f;
        double best = (double)radiusTiles * radiusTiles;
        bool found = false;
        lock (_gardenLock)
        {
            if (Garden.Length % 6 != 0)
            {
                return false;
            }
            for (int i = 0; i < Garden.Length; i += 6)
            {
                ushort x = BitConverter.ToUInt16(Garden, i);
                ushort y = BitConverter.ToUInt16(Garden, i + 2);
                double dx = x - px;
                double dy = y - py;
                double d = dx * dx + dy * dy;
                if (d <= best)
                {
                    best = d;
                    tile = new Point2(x, y);
                    entityType = BitConverter.ToUInt16(Garden, i + 4);
                    found = true;
                }
            }
        }
        return found;
    }

    // ลบธรรมชาติ 1 ตัวออกจาก garden (ใช้ตอนเก็บหมดหรือใช้ interaction RemoveNatural)
    // แล้ว chunk ที่ขอใหม่จะไม่มีธรรมชาตินั้นอีก
    public bool RemoveNatural(int tileX, int tileY)
    {
        lock (_gardenLock)
        {
            if (Garden.Length % 6 != 0)
            {
                return false;
            }
            var kept = new List<byte[]>();
            bool removed = false;
            for (int i = 0; i < Garden.Length; i += 6)
            {
                ushort x = BitConverter.ToUInt16(Garden, i);
                ushort y = BitConverter.ToUInt16(Garden, i + 2);
                if (!removed && x == tileX && y == tileY)
                {
                    removed = true;
                    continue;
                }
                byte[] rec = new byte[6];
                Array.Copy(Garden, i, rec, 0, 6);
                kept.Add(rec);
            }
            if (!removed)
            {
                return false;
            }
            byte[] all = new byte[kept.Count * 6];
            for (int i = 0; i < kept.Count; i++)
            {
                Array.Copy(kept[i], 0, all, i * 6, 6);
            }
            Garden = all;
            _gardenByChunk = null;                  // M-6: index เก่าใช้ไม่ได้แล้ว
            _removedNaturals.Add((tileX, tileY));   // GP-07
            return true;
        }
    }

    private static void CopyChunk(int chunkX, int chunkY, byte[] src, byte[] dst, int count, int prevOffset, int postOffset, int span)
    {
        int num = chunkX * TileSize;
        int num2 = chunkY * TileSize;
        int side = (int)Math.Sqrt(src.Length / (double)count);
        int step = span;
        for (int i = -prevOffset; i < TileSize + postOffset; i++)
        {
            for (int j = -prevOffset; j < TileSize + postOffset; j++)
            {
                int cx = Math.Clamp(num + i, 0, side - 1);
                int cy = Math.Clamp(num2 + j, 0, side - 1);
                int srcIdx = (cx + cy * side) * count;
                int dstIdx = ((i + prevOffset) + (j + prevOffset) * step) * count;
                for (int k = 0; k < count; k++)
                {
                    dst[dstIdx + k] = src[srcIdx + k];
                }
            }
        }
    }
}
