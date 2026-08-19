using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// เฟส C — เกิดสัตว์ในโลกและให้มันเดินสุ่ม
///
/// ตั้งใจให้สัตว์ **ไม่ถูกเซฟ** — เปิดเซิร์ฟใหม่ก็เกิดใหม่หมด
/// เพราะสัตว์เป็นของชั่วคราวในโลก ไม่ใช่ความคืบหน้าของผู้เล่น (ต่างจากบ้าน/ของในกล่อง)
///
/// ดูรายละเอียดที่ docs/server/Animals.md
/// </summary>
public sealed class AnimalSpawner
{
    // Beta 1.0: จำนวน/ชนิด/เลเวล มาจาก SpawnTable ไม่ใช่การสุ่มแล้ว (ดู docs/BETA-1.0-PLAN.md)

    /// <summary>รัศมีจากจุดเกิดที่ใช้กระจายสัตว์ (หน่วยโลก = tile * 200)</summary>
    private static float SpawnRadius => ServerConfig.Current.Animals.SpawnRadiusTiles * 200f;

    /// <summary>เดินออกจากบ้านตัวเองได้ไกลสุดเท่าไร</summary>
    private static float WanderRadius => ServerConfig.Current.Animals.WanderRadiusTiles * 200f;

    private static float WalkSpeed => ServerConfig.Current.Animals.WalkSpeed;
    /// <summary>เวลาพักหลังเดินถึงที่หมายแล้ว (ไม่ใช่ระยะห่างระหว่างคำสั่ง)</summary>
    private static double RestIntervalMin => ServerConfig.Current.Animals.RestMinSeconds;
    private static double RestIntervalMax => ServerConfig.Current.Animals.RestMaxSeconds;


    private readonly ServerWorld _world;
    private readonly Dictionary<string, ServerAnimal> _animals = new Dictionary<string, ServerAnimal>();
    private readonly object _lock = new object();
    private readonly Random _rng = new Random(12345);   // seed คงที่ ทำให้ทดสอบซ้ำได้

    public AnimalSpawner(ServerWorld world)
    {
        _world = world;
    }

    /// <summary>จำนวนสัตว์ทั้งหมดที่ยังอยู่ในโลก (นับซากที่ยังไม่หายด้วย)</summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _animals.Count;
            }
        }
    }

    /// <summary>
    /// นับเฉพาะตัวเป็น ๆ — ใช้กับบรรทัดสถิติ เพราะซากอยู่ได้นานกว่าเวลาเกิดใหม่
    /// ถ้ารายงานรวมซาก ตัวเลขจะเกินโควตาเป็นระยะจนคนดูแลเซิร์ฟเข้าใจผิด
    /// </summary>
    public int AliveCount
    {
        get
        {
            lock (_lock)
            {
                int n = 0;
                foreach (ServerAnimal a in _animals.Values)
                {
                    if (a.IsAlive)
                    {
                        n++;
                    }
                }
                return n;
            }
        }
    }

    public ServerAnimal[] Snapshot()
    {
        lock (_lock)
        {
            ServerAnimal[] all = new ServerAnimal[_animals.Count];
            _animals.Values.CopyTo(all, 0);
            return all;
        }
    }

    public bool TryGet(string entityId, out ServerAnimal animal)
    {
        lock (_lock)
        {
            return _animals.TryGetValue(entityId ?? string.Empty, out animal);
        }
    }

    /// <summary>เกิดสัตว์ให้ครบโควตาทุกชนิดตอนเปิดเซิร์ฟ</summary>
    public void SpawnInitial()
    {
        double now = Times.UnixTimeNow();
        WorldPosition center = _world.GetEntryPosition();
        for (int i = 0; i < SpawnTable.Entries.Length; i++)
        {
            SpawnTable.Entry e = SpawnTable.Entries[i];
            for (int n = 0; n < e.Quota; n++)
            {
                SpawnFromTable(e, center, now);
            }
        }
        Console.WriteLine($"[animal] เกิดสัตว์ {Count} ตัว จาก {SpawnTable.Entries.Length} ชนิด (โควตารวม {SpawnTable.TotalQuota})");
        ReportSpawnDepth();
    }

    /// <summary>
    /// สรุปว่าแต่ละชนิดเกิดลึกเข้าไปในเกาะเท่าไรจริง ๆ — ไว้ยืนยันด้วยตาว่า
    /// "ตัวใหญ่อยู่กลางเกาะ ตัวเล็กอยู่ชายป่า" ทำงานจริง ไม่ต้องเดินหาในเกม
    /// </summary>
    private void ReportSpawnDepth()
    {
        var minDepth = new Dictionary<ushort, int>();
        var count = new Dictionary<ushort, int>();
        foreach (ServerAnimal a in Snapshot())
        {
            WorldPosition p = a.Position;
            int d = _world.Terrain.LandDistance((int)(p.x / 200f), (int)(p.y / 200f));
            if (!minDepth.TryGetValue(a.EntityType, out int cur) || d < cur)
            {
                minDepth[a.EntityType] = d;
            }
            count[a.EntityType] = count.TryGetValue(a.EntityType, out int n) ? n + 1 : 1;
        }
        var lines = new List<string>();
        foreach (KeyValuePair<ushort, int> pair in minDepth)
        {
            SpawnTable.Entry e = SpawnTable.Find(pair.Key);
            int size = AnimalData.TryGet(pair.Key, out AnimalData.AnimalInfo info) ? info.SizeLevel : 1;
            lines.Add($"{e?.Name ?? pair.Key.ToString()} (ขนาด {size}) ×{count[pair.Key]} ใกล้ฝั่งสุด {pair.Value} tile / ต้องการ {InlandFor(pair.Key)}");
        }
        lines.Sort();
        for (int i = 0; i < lines.Count; i++)
        {
            Console.WriteLine("[animal]   {0}", lines[i]);
        }
    }

    /// <summary>เกิด 1 ตัวตามข้อมูลในตาราง (เลเวล/เลือด/เขตปลอดภัย ตามชนิด)</summary>
    /// <summary>
    /// โซนที่อยู่อาศัยของชนิดนี้ (null = ไม่ได้กำหนดโซน ใช้การกระจายทั่วเกาะแบบเดิม)
    /// </summary>
    private static ZoneConfig ZoneOf(ushort entityType)
    {
        List<ZoneConfig> zones = ServerConfig.Current.Zones;
        if (zones == null)
        {
            return null;
        }
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].Species != null && zones[i].Species.Contains(entityType))
            {
                return zones[i];
            }
        }
        return null;
    }

    /// <summary>
    /// ชนิดนี้ต้องเกิด/เดินลึกเข้าไปในเกาะอย่างน้อยกี่ tile — **ตัวใหญ่ = ลึกกว่า**
    ///
    /// ใช้ `size_level` จากข้อมูลเกมจริง (1–7) ไม่ใช่ `Scale` ซึ่งเป็นตัวคูณของแต่ละ prefab
    /// และเทียบข้ามชนิดไม่ได้ (แร็ปเตอร์ Scale 2.2 แต่ตัวเล็กกว่าบราคิโอที่ Scale 1.27)
    ///
    /// ผลที่ได้: กิ้งก่า/คอมป์โซ (size 1) เดินถึงชายป่าได้ · สเตโก/ทริเซรา (size 4) อยู่ลึกเข้าไป
    /// ⇒ "ยิ่งเข้ากลางเกาะยิ่งเจอตัวใหญ่" กลายเป็นกติกาที่ผู้เล่นเรียนรู้ได้เองจากการเดิน
    /// </summary>
    private static int InlandFor(ushort entityType)
    {
        AnimalConfig cfg = ServerConfig.Current.Animals;
        int size = AnimalData.TryGet(entityType, out AnimalData.AnimalInfo info) ? info.SizeLevel : 1;
        if (size < 1)
        {
            size = 1;
        }
        return cfg.MinTilesInland + (size - 1) * cfg.InlandTilesPerSize;
    }

    /// <summary>โซนหนึ่งต้องลึกเท่าตัวที่ใหญ่ที่สุดในโซนนั้น</summary>
    private static int InlandForZone(ZoneConfig zone)
    {
        int need = ServerConfig.Current.Animals.MinTilesInland;
        if (zone?.Species == null)
        {
            return need;
        }
        for (int i = 0; i < zone.Species.Count; i++)
        {
            int n = InlandFor(zone.Species[i]);
            if (n > need)
            {
                need = n;
            }
        }
        return need;
    }

    /// <summary>กึ่งกลางโซนที่ "ขยับให้ไปอยู่บนบกแล้ว" — คิดครั้งเดียวต่อโซน</summary>
    private readonly Dictionary<string, WorldPosition> _zoneCenters = new Dictionary<string, WorldPosition>();

    /// <summary>
    /// กึ่งกลางโซนในพิกัดโลก (config เก็บเป็นระยะห่างจากจุดเข้าเกม)
    ///
    /// ⚠️ จุดเข้าเกมของเกาะอยู่ริมน้ำ (จุดที่เรือมาจอด) ระยะที่ตั้งไว้ในไฟล์จึงมีสิทธิ์ไปตกกลางทะเล
    /// ถ้าเจอแบบนั้นให้ **หาจุดบนบกที่ใกล้ที่สุดแทน** (ค้นเป็นวงออกไปทีละ 2 tile)
    /// ไม่งั้นสัตว์ทั้งโซนจะหาที่เกิดไม่ได้ แล้วไปโผล่บนหาดตามจุดสุดท้ายที่สุ่มได้
    /// </summary>
    private WorldPosition ZoneCenter(ZoneConfig zone, WorldPosition entry)
    {
        // โซนต้องลึกพอสำหรับตัวที่ใหญ่ที่สุดในโซน + เผื่อรัศมีโซนไว้ครึ่งหนึ่ง
        // (ไม่เผื่อ = กึ่งกลางโซนลึกพอ แต่ขอบโซนยังยื่นลงหาด แล้วสัตว์ก็ไปกระจุกตรงขอบ)
        int minInland = InlandForZone(zone) + (int)(zone.RadiusTiles * 0.5f);
        // ใส่ค่าที่ใช้คิดลงใน key ด้วย — แก้ config ตอนเซิร์ฟรันอยู่แล้วโซนจะได้ขยับตาม
        string key = (zone.Id ?? zone.Name ?? "?") + "|" + minInland;
        lock (_zoneCenters)
        {
            if (_zoneCenters.TryGetValue(key, out WorldPosition cached))
            {
                return cached;
            }
        }

        var wanted = new WorldPosition(
            entry.x + zone.OffsetTileX * 200f,
            entry.y + zone.OffsetTileY * 200f);
        WorldPosition found = wanted;

        if (!_world.Terrain.IsLand(wanted.x, wanted.y, minInland))
        {
            // ค้นเป็นวงออกไป — ในวงแรกที่เจอที่ผ่านเกณฑ์ เลือก **จุดที่ลึกที่สุด**
            // ไม่ใช่จุดแรกที่เจอ ไม่งั้นโซนของตัวใหญ่จะไปเกาะขอบเงื่อนไขพอดี ๆ แล้วยังดูใกล้ฝั่ง
            bool ok = false;
            int bestDepth = int.MinValue;
            for (int ring = 2; ring <= 60 && !ok; ring += 2)
            {
                for (int step = 0; step < 16; step++)
                {
                    double ang = step * Math.PI / 8.0;
                    var cand = new WorldPosition(
                        wanted.x + (float)(Math.Cos(ang) * ring * 200.0),
                        wanted.y + (float)(Math.Sin(ang) * ring * 200.0));
                    if (!_world.Terrain.IsLand(cand.x, cand.y, minInland))
                    {
                        continue;
                    }
                    int depth = _world.Terrain.LandDistance((int)(cand.x / 200f), (int)(cand.y / 200f));
                    if (depth > bestDepth)
                    {
                        bestDepth = depth;
                        found = cand;
                        ok = true;
                    }
                }
            }
            if (!ok && _world.Terrain.TryDeepestLand(out float dx, out float dy, out int deep))
            {
                // ไม่มีที่ไหนรอบ ๆ ลึกพอเลย → ไปกลางเกาะไปเลย ดีกว่าปล่อยให้ตกอยู่ริมหาด
                found = new WorldPosition(dx, dy);
                Console.WriteLine("[animal] โซน {0}: หาที่ลึก {1} tile รอบจุดที่ตั้งไว้ไม่เจอ — ใช้กลางเกาะ (tile {2:F0},{3:F0} ลึก {4})",
                    zone.Name ?? key, minInland, dx / 200f, dy / 200f, deep);
            }
            else
            {
                Console.WriteLine("[animal] โซน {0}: จุดที่ตั้งไว้ (tile {1:F0},{2:F0}) ตื้นเกิน — ย้ายไป tile {3:F0},{4:F0} (ลึก {5} tile ต้องการ {6})",
                    zone.Name ?? key, wanted.x / 200f, wanted.y / 200f, found.x / 200f, found.y / 200f, bestDepth, minInland);
            }
        }

        lock (_zoneCenters)
        {
            _zoneCenters[key] = found;
        }
        return found;
    }

    private ServerAnimal SpawnFromTable(SpawnTable.Entry e, WorldPosition center, double now)
    {
        float scale = AnimalData.TryGet(e.EntityType, out AnimalData.AnimalInfo info) ? info.Scale : 1f;
        int level = e.MinLevel + _rng.Next(e.MaxLevel - e.MinLevel + 1);

        // หาจุดเกิดที่ผ่านเงื่อนไข 2 ข้อ: อยู่บนบก และไกลจุดเกิดของผู้เล่นตามที่ตารางกำหนด
        // (ไม่เช็คน้ำ = ได้ไดโนเสาร์ลอยกลางทะเล · ไม่เช็คระยะ = คนเพิ่งเข้าเกมโดนตัวดุรุมทันที)
        float minDist = e.MinTilesFromEntry * 200f;
        WorldPosition home = center;

        // มีโซนที่อยู่อาศัย = เกิดในโซนของตัวเอง (ทุ่งหญ้า/ชายป่า/ที่ราบสูง/หุบแร็ปเตอร์)
        // ไม่มีโซน = กระจายทั่วเกาะแบบเดิม
        ZoneConfig zone = ZoneOf(e.EntityType);
        int minInland = InlandFor(e.EntityType);      // ตัวใหญ่ต้องลึกกว่า
        bool ok = false;
        for (int tries = 0; tries < 80 && !ok; tries++)
        {
            if (zone != null)
            {
                home = RandomAround(ZoneCenter(zone, center), zone.RadiusTiles * 200f);
            }
            else
            {
                home = RandomAround(center, SpawnRadius);
            }
            // ✅ ต้องเป็นแผ่นดินที่ลึกเข้าไปพอ — ไม่งั้นไดโนเสาร์ไปยืนบนหาด/ในทะเล
            //    (ใช้ oceans.dm ของ terrain ดู TerrainStore.LandDistance)
            ok = _world.Terrain.IsLand(home.x, home.y, minInland)
                 && (zone != null || minDist <= 0f || Distance(home, center) >= minDist);
        }
        if (!ok)
        {
            Console.WriteLine($"[animal] หาจุดเกิดบนบกให้ {e.Name} ไม่ได้ใน 80 ครั้ง — ใช้จุดสุดท้ายที่สุ่มได้");
        }

        ServerAnimal animal = new ServerAnimal(
            "animal_" + Guid.NewGuid().ToString("N").Substring(0, 12),
            e.EntityType, level, scale, home, SpawnTable.LifeFor(level), now);
        animal.NextMoveAt = now + NextInterval();
        animal.Height = _world.GroundHeightHint;

        lock (_lock)
        {
            _animals[animal.EntityId] = animal;
        }
        return animal;
    }

    private static float Distance(WorldPosition a, WorldPosition b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <param name="radius">0 = เกิดตรงจุดนั้นพอดี</param>
    /// <param name="forceType">0 = สุ่มชนิด</param>
    private ServerAnimal SpawnOne(WorldPosition center, double now, float radius, ushort forceType)
    {
        ushort type = forceType > 0
            ? forceType
            : SpawnTable.Entries[_rng.Next(SpawnTable.Entries.Length)].EntityType;
        float scale = 1f;
        if (AnimalData.TryGet(type, out AnimalData.AnimalInfo info))
        {
            scale = info.Scale;
        }

        WorldPosition home = radius <= 0f ? center : RandomAround(center, radius);
        SpawnTable.Entry entry = SpawnTable.Find(type);
        int level = entry != null
            ? entry.MinLevel + _rng.Next(entry.MaxLevel - entry.MinLevel + 1)
            : 1 + _rng.Next(10);
        float lifeMax = SpawnTable.LifeFor(level);

        ServerAnimal animal = new ServerAnimal(
            "animal_" + Guid.NewGuid().ToString("N").Substring(0, 12),
            type, level, scale, home, lifeMax, now);
        animal.NextMoveAt = now + NextInterval();

        lock (_lock)
        {
            _animals[animal.EntityId] = animal;
        }
        return animal;
    }

    /// <summary>
    /// เกิดสัตว์ตรงจุดที่กำหนด แล้วบอกทุกคนทันที (ใช้กับ cheat ตอนเทส —
    /// สัตว์ปกติกระจายในรัศมี 30 tile ซึ่งมักอยู่นอกจอ ทำให้ตรวจสอบด้วยตาลำบาก)
    /// </summary>
    /// <param name="height">ความสูงพื้นตรงนั้น (0 = ใช้ค่าที่ client เคยรายงานไว้)</param>
    public ServerAnimal SpawnAt(WorldPosition pos, ushort type = 0, float height = 0f)
    {
        double now = Times.UnixTimeNow();
        ServerAnimal animal = SpawnOne(pos, now, 0f, type);
        animal.Height = height != 0f ? height : _world.GroundHeightHint;
        _world.Broadcast(animal.MakeAppear());
        Console.WriteLine($"[animal] เรียกเกิด {animal.EntityId} (type {animal.EntityType} lv{animal.Level}) ที่ tile {pos.x / 200f:F0},{pos.y / 200f:F0} สูง {animal.Height:F0}");
        return animal;
    }

    private WorldPosition RandomAround(WorldPosition center, float radius)
    {
        double ang = _rng.NextDouble() * Math.PI * 2.0;
        double r = Math.Sqrt(_rng.NextDouble()) * radius;   // sqrt ทำให้กระจายทั่วพื้นที่ ไม่กระจุกกลาง
        return new WorldPosition(
            center.x + (float)(Math.Cos(ang) * r),
            center.y + (float)(Math.Sin(ang) * r));
    }

    /// <summary>
    /// หาที่ลงเท้าที่ยังเป็นบก — ลองทิศตรงก่อน ถ้าเป็นทะเลก็เบนซ้าย/ขวาทีละ 45°
    /// (สัตว์ที่วิ่งหนีควรวิ่งเลียบชายฝั่ง ไม่ใช่วิ่งลงทะเล)
    /// คืน false ถ้าไม่มีทิศไหนเป็นบกเลย — ผู้เรียกควรอยู่เฉย ๆ รอบนั้น
    /// </summary>
    private bool TryLandDestination(WorldPosition from, WorldPosition dest, int minInland, out WorldPosition result)
    {
        result = dest;
        if (_world.Terrain.IsLand(dest.x, dest.y, minInland))
        {
            return true;
        }
        float dx = dest.x - from.x;
        float dy = dest.y - from.y;
        float len = MathF.Sqrt(dx * dx + dy * dy);
        if (len < 1f)
        {
            return false;
        }
        // เบนทีละ 45° สลับซ้าย-ขวา แล้วค่อยลองสั้นลงครึ่งหนึ่ง
        float[] angles = { 0.785f, -0.785f, 1.571f, -1.571f, 2.356f, -2.356f };
        for (int scale = 0; scale < 2; scale++)
        {
            float reach = scale == 0 ? len : len * 0.5f;
            for (int i = 0; i < angles.Length; i++)
            {
                float cos = MathF.Cos(angles[i]);
                float sin = MathF.Sin(angles[i]);
                float nx = (dx * cos - dy * sin) / len * reach;
                float ny = (dx * sin + dy * cos) / len * reach;
                var cand = new WorldPosition(from.x + nx, from.y + ny);
                if (_world.Terrain.IsLand(cand.x, cand.y, minInland))
                {
                    result = cand;
                    return true;
                }
            }
        }
        return false;
    }

    private double NextInterval()
    {
        return RestIntervalMin + _rng.NextDouble() * (RestIntervalMax - RestIntervalMin);
    }

    /// <summary>ส่งสัตว์ทั้งหมดให้ผู้เล่นที่เพิ่งเข้ามา</summary>
    public void SendAllTo(ServerPlayer player)
    {
        ServerAnimal[] all = Snapshot();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].IsAlive)
            {
                // ใช้ความสูงพื้นที่คนเข้าเกมรายงานมา ไม่งั้นสัตว์จมใต้พื้น (เห็นแต่เงา)
                if (all[i].Height == 0f)
                {
                    all[i].Height = player.CurrentHeight != 0f ? player.CurrentHeight : _world.GroundHeightHint;
                }
                player.Send(all[i].MakeAppear());
            }
        }
    }

    /// <summary>เรียกทุก tick จาก ServerWorld — เดินสุ่ม + AI ตอนโดนตี + จัดการซาก/เกิดใหม่</summary>
    public void Process()
    {
        double now = Times.UnixTimeNow();
        // ซาก/การเกิดใหม่ต้องเดินต่อแม้ไม่มีใครออนไลน์ ไม่งั้นสัตว์ที่ตายไปตอนคนออกเกม
        // จะค้างเป็นซากตลอดกาลและจำนวนสัตว์ในโลกลดลงเรื่อย ๆ
        ProcessCorpsesAndRespawn(now);

        // ส่วนการเดิน/AI ข้ามได้ถ้าไม่มีคนดู ประหยัดทั้ง CPU และแบนด์วิดท์
        if (_world.Count == 0)
        {
            return;
        }

        ServerAnimal[] all = Snapshot();
        for (int i = 0; i < all.Length; i++)
        {
            ServerAnimal a = all[i];
            if (!a.IsAlive)
            {
                continue;
            }
            // ถึงเวลากลับไปท่ายืน (เดินถึงที่หมาย / ตีจบ)
            // ถ้าไม่สั่ง client จะเล่นคลิปเดิมวนไปเรื่อย ๆ และตัวค้างผิดตำแหน่งจากคลิปที่มี root motion
            if (a.StandAt > 0 && now >= a.StandAt)
            {
                a.StandAt = 0;
                _world.Broadcast(a.MakeMotion(AnimalMotionData.Stand(a.EntityType), a.Yaw, now, 2.0, loop: true));
            }
            // ตัวนิสัยดุ: เห็นคนในระยะก็ไล่เลย ไม่ต้องรอโดนตีก่อน
            if (BehaviorOf(a.EntityType) == AnimalBehavior.Aggressive)
            {
                LookForPrey(a, now);
            }
            if (ProcessAi(a, now))
            {
                continue;                 // กำลังไล่/หนีอยู่ ไม่ต้องเดินสุ่ม
            }
            if (now < a.NextMoveAt)
            {
                continue;
            }
            // เดินสุ่มอยู่ในโซนของตัวเอง — ไม่หลุดไปทั่วเกาะ
            // (รัศมีเดินไม่เกินทั้งค่ากลางและขอบโซน แล้วดึงกลับถ้าเผลอออกนอกโซน)
            WorldPosition dest = RandomAround(a.Home, WanderRadius);
            ZoneConfig zone = ZoneOf(a.EntityType);
            if (zone != null)
            {
                WorldPosition zc = ZoneCenter(zone, _world.GetEntryPosition());
                float limit = zone.RadiusTiles * 200f;
                float zdx = dest.x - zc.x;
                float zdy = dest.y - zc.y;
                float zd = MathF.Sqrt(zdx * zdx + zdy * zdy);
                if (zd > limit && zd > 1f)
                {
                    dest = new WorldPosition(zc.x + zdx / zd * limit, zc.y + zdy / zd * limit);
                }
            }
            // เดินทีละขาสั้น ๆ — ดูเป็นธรรมชาติกว่าเดินยาว 20 วินาทีรวดเดียว
            // และถ้ามีอะไรมาขัดกลางทาง ตำแหน่งก็เพี้ยนได้น้อยกว่า
            float maxLeg = WalkSpeed * (float)ServerConfig.Current.Animals.MaxWalkLegSeconds;
            WorldPosition here = a.PositionAt(now);
            float ldx = dest.x - here.x, ldy = dest.y - here.y;
            float legDist = MathF.Sqrt(ldx * ldx + ldy * ldy);
            if (legDist > maxLeg && legDist > 1f)
            {
                dest = new WorldPosition(here.x + ldx / legDist * maxLeg, here.y + ldy / legDist * maxLeg);
            }
            // ไม่เดินลงทะเล — ถ้าปลายทางไม่ใช่บก ให้ข้ามรอบนี้ไปสุ่มใหม่รอบหน้า
            if (!_world.Terrain.IsLand(dest.x, dest.y, InlandFor(a.EntityType)))
            {
                a.NextMoveAt = now + 1.0;
                continue;
            }
            Move move = a.MakeMove(dest, WalkSpeed, now, out double travelSeconds);
            a.StandAt = now + travelSeconds;
            // ต้องรอให้เดินถึงก่อนแล้วค่อยพัก ไม่งั้นสั่งเดินใหม่ทับของเดิม
            // server จะคิดว่ามันถึงที่หมายแล้วทั้งที่ client ยังเดินอยู่ → ตัวกระตุกไปข้างหน้า
            a.NextMoveAt = now + travelSeconds + NextInterval();
            _world.Broadcast(move);
        }
    }

    // ───────── เฟส C รอบ 2: โดนตี / ตาย / เกิดใหม่ ─────────

    /// <summary>
    /// ซากอยู่ในโลกกี่วินาทีก่อนหาย
    /// ต้องนานพอให้เดินไปแล่จนครบทุกชิ้นส่วน (ตัวใหญ่มี 8-9 หน่วย × ~3 วิ = ~30 วิ)
    /// บวกเวลาเดินไปหาซากอีก — 30 วิเดิมสั้นเกินจนซากหายคาตา
    /// </summary>
    private static double CorpseSeconds => ServerConfig.Current.Animals.CorpseSeconds;

    /// <summary>ตายแล้วกี่วินาทีถึงเกิดตัวใหม่แทน</summary>
    private static double RespawnSeconds => ServerConfig.Current.Animals.RespawnSeconds;

    /// <summary>ซากที่รอหาย: entity id → เวลาที่จะเอาออก</summary>
    private readonly Dictionary<string, double> _corpses = new Dictionary<string, double>();

    /// <summary>ความยาวโดยประมาณของคลิปตาย — ครบเมื่อไรค่อยสั่งค้างเฟรม</summary>
    private const double DeathClipSeconds = 1.6;

    /// <summary>ซากที่รอ "หยุดเฟรม": entity id → เวลาที่ต้องส่งคำสั่งค้างท่า</summary>
    private readonly Dictionary<string, double> _freezeAt = new Dictionary<string, double>();

    /// <summary>คิวเกิดใหม่: (เวลาที่จะเกิด, ชนิดที่ตายไป) — ต้องเกิดชนิดเดิมเพื่อรักษาโควตาตามตาราง</summary>
    private readonly List<(double at, ushort type)> _respawnAt = new List<(double, ushort)>();

    /// <summary>
    /// เข้าดาเมจสัตว์ตัวหนึ่ง คืน true ถ้าตายจากหมัดนี้
    /// ตายแล้ว: broadcast EntityDied ทันที · ซากหายใน 20 วิ · เกิดตัวใหม่ใน 45 วิ
    /// </summary>
    public bool Damage(string entityId, float amount, string attackerId)
    {
        double now = Times.UnixTimeNow();
        ServerAnimal animal;
        lock (_lock)
        {
            if (!_animals.TryGetValue(entityId ?? string.Empty, out animal) || !animal.IsAlive)
            {
                return false;
            }
        }

        bool died = animal.ApplyDamage(amount, now);
        // หลอดเลือดของสัตว์ต้องอัปเดตให้ทุกคนเห็น ไม่งั้นตีจนตายแต่หลอดยังเต็ม
        _world.Broadcast(new Survival { EntityId = animal.EntityId, Life = animal.LifeGauge() });

        if (!died)
        {
            // โดนตีแล้วรู้ตัว: เข้าโหมดสู้/หนีตามนิสัย (ดู ProcessCombat)
            OnAttacked(animal, attackerId, now);
            return false;
        }

        Console.WriteLine("[animal] ☠ {0} (type {1} lv{2}) ตายด้วยมือ {3}", animal.EntityId, animal.EntityType, animal.Level, attackerId);
        // เล่นคลิปตายก่อน แล้วค่อยบอกว่าตาย (client ใช้ EntityDied ไปสั่ง SetAlive(false))
        string dieClip = AnimalMotionData.Die(animal.EntityType);
        if (dieClip != null)
        {
            _world.Broadcast(animal.MakeMotion(dieClip, animal.Yaw, now, 2.0));
        }
        _world.Broadcast(new EntityDied { EntityId = animal.EntityId, At = now });

        // เปิดให้แล่เนื้อ: generator ของซากเป็นของกลางที่ world เหมือนจุดเก็บของธรรมชาติ
        // (ถ้าเก็บไว้ในตัวผู้เล่น สองคนจะแล่ซากเดียวกันได้ของครบทั้งคู่)
        _world.SetGenerators(animal.EntityId, ButcheryData.MakeGenerators(animal.EntityType, animal.Level));
        // client เอา DistributableEntities ไปเปิดไฟขอบเรืองแสงรอบซาก (AnimalBehavior.IsLootable)
        // ให้สิทธิ์เรืองแสงกับคนที่ฆ่า แต่ server ไม่ได้ห้ามคนอื่นแล่ — เล่นด้วยกันจะได้ช่วยกันเก็บได้
        if (!string.IsNullOrEmpty(attackerId))
        {
            _world.Broadcast(new CollectibleDisplay
            {
                EntityId = animal.EntityId,
                DistributableEntities = new[] { attackerId }
            });
        }
        lock (_lock)
        {
            // คลิปตายของหลายชนิดตั้ง wrap mode เป็น Loop มาในตัวคลิปเอง (client ส่ง WrapMode.Default ให้)
            // ถ้าไม่สั่งหยุด ซากจะล้มแล้วลุกวนไปเรื่อย ๆ — ส่งซ้ำด้วย playbackRate 0 เพื่อค้างท่าสุดท้าย
            _freezeAt[animal.EntityId] = now + DeathClipSeconds;
            _corpses[animal.EntityId] = now + CorpseSeconds;
            _respawnAt.Add((now + RespawnSeconds, animal.EntityType));
            _targets.Remove(animal.EntityId);
        }
        return true;
    }

    // ───────── AI ตอนโดนตี: สู้กลับ / หนี ─────────

    /// <summary>ระยะที่สัตว์เข้าตีได้</summary>
    private const float AttackRange = 300f;

    /// <summary>ไล่ไกลกว่านี้แล้วเลิกสนใจ</summary>
    private static float GiveUpDistance => ServerConfig.Current.Animals.GiveUpTiles * 200f;

    private static float ChaseSpeed => ServerConfig.Current.Animals.ChaseSpeed;
    private static float FleeSpeed => ServerConfig.Current.Animals.FleeSpeed;

    /// <summary>คูลดาวน์ระหว่างการกัด — ใช้เมื่อชนิดนั้นไม่มีในตาราง</summary>
    private const double AttackInterval = 1.6;

    /// <summary>
    /// โดนตีแล้วอีกกี่วินาทีถึงสวนกลับครั้งแรก
    /// เดิมใช้คูลดาวน์เต็ม (2.5 วิ) เป็นครั้งแรกด้วย ทำให้ "ตีแล้วมันนิ่งไปพักใหญ่"
    /// </summary>
    private static double FirstAttackDelay => ServerConfig.Current.Animals.FirstAttackDelay;

    /// <summary>ความยาวโดยประมาณของคลิปโจมตี — ครบแล้วสั่งกลับไปท่ายืน</summary>
    private const double AttackClipSeconds = 1.0;
    private static double AggroSeconds => ServerConfig.Current.Animals.AggroSeconds;


    private sealed class Aggro
    {
        public string PlayerId;
        public bool Flee;
        public double UntilAt;
        public double NextAttackAt;
    }

    private readonly Dictionary<string, Aggro> _targets = new Dictionary<string, Aggro>();

    /// <summary>นิสัยของชนิดนี้ (ไม่มีในตาราง = สู้กลับ)</summary>
    private static AnimalBehavior BehaviorOf(ushort entityType)
    {
        SpawnTable.Entry e = SpawnTable.Find(entityType);
        return e?.Behavior ?? AnimalBehavior.FightBack;
    }

    private static bool IsTimid(ushort entityType)
    {
        return BehaviorOf(entityType) == AnimalBehavior.Flee;
    }

    /// <summary>คูลดาวน์การกัดของชนิดนี้ (ค่าจริงจากข้อมูลเกม — ดู SpawnTable)</summary>
    private static double AttackCooltimeOf(ushort entityType)
    {
        SpawnTable.Entry e = SpawnTable.Find(entityType);
        return e != null ? e.AttackCooltime : AttackInterval;
    }

    /// <summary>ระยะที่ตัวดุเริ่มสนใจผู้เล่น (หน่วยโลก — 1200 = 6 tile)</summary>
    private static float SightRange => ServerConfig.Current.Animals.SightTiles * 200f;

    /// <summary>ตัวดุมองหาเหยื่อเอง — เจอคนในระยะก็เริ่มไล่</summary>
    private void LookForPrey(ServerAnimal a, double now)
    {
        lock (_lock)
        {
            if (_targets.ContainsKey(a.EntityId))
            {
                return;                    // ไล่ใครอยู่แล้ว
            }
        }
        ServerPlayer prey = _world.FindNearestPlayer(a.PositionAt(now), SightRange);
        if (prey == null || prey.Dead)
        {
            return;
        }
        lock (_lock)
        {
            _targets[a.EntityId] = new Aggro
            {
                PlayerId = prey.EntityId,
                Flee = false,
                UntilAt = now + AggroSeconds,
                NextAttackAt = now + FirstAttackDelay
            };
        }
        a.NextMoveAt = now;      // เลิกพัก เริ่มไล่เดี๋ยวนี้
        Console.WriteLine("[animal] {0} (type {1}) เห็น {2} แล้วไล่กัด", a.EntityId, a.EntityType, prey.Name);
    }

    /// <summary>โดนตีแล้วรู้ตัว — ตัวขี้ตกใจวิ่งหนี ตัวอื่นไล่สู้กลับ</summary>
    private void OnAttacked(ServerAnimal animal, string attackerId, double now)
    {
        if (string.IsNullOrEmpty(attackerId))
        {
            return;
        }
        bool flee = IsTimid(animal.EntityType);
        lock (_lock)
        {
            // โดนตีซ้ำ = ต่ออายุความโกรธเท่านั้น
            // ห้ามสร้าง Aggro ใหม่ ไม่งั้น NextAttackAt ถูกเลื่อนออกไปทุกหมัด → สัตว์ไม่เคยได้ตีกลับเลย
            if (_targets.TryGetValue(animal.EntityId, out Aggro existing) && existing.PlayerId == attackerId)
            {
                existing.UntilAt = now + AggroSeconds;
                return;
            }
            _targets[animal.EntityId] = new Aggro
            {
                PlayerId = attackerId,
                Flee = flee,
                // ครั้งแรกสวนไว ๆ ครั้งต่อไปค่อยเว้นตามคูลดาวน์ของชนิดนั้น
                NextAttackAt = now + FirstAttackDelay,
                UntilAt = now + AggroSeconds
            };
        }
        // 🐛 สำคัญ: ตอนโดนตี สัตว์มักอยู่ในช่วง "พักหลังเดินถึงที่หมาย" ซึ่งยาวได้ถึง 14 วินาที
        // ทั้งการไล่และการหนีใน ProcessAi ติดเงื่อนไข now >= NextMoveAt เหมือนกัน
        // ถ้าไม่ล้างตรงนี้ ตีแล้วมันจะยืนเฉยรอหมดเวลาพักก่อนค่อยขยับ = "สวนกลับช้าเกินไป"
        animal.NextMoveAt = now;
        Console.WriteLine("[animal] {0} (type {1}) {2} {3}", animal.EntityId, animal.EntityType,
            flee ? "ตกใจวิ่งหนี" : "สู้กลับ", attackerId);
    }

    /// <summary>คืน true ถ้าตัวนี้มี AI คุมอยู่ (ไล่/หนี) — ผู้เรียกจะได้ไม่สั่งเดินสุ่มทับ</summary>
    private bool ProcessAi(ServerAnimal a, double now)
    {
        Aggro aggro;
        lock (_lock)
        {
            if (!_targets.TryGetValue(a.EntityId, out aggro))
            {
                return false;
            }
        }

        ServerPlayer target = _world.FindPlayer(aggro.PlayerId);
        if (target == null || target.Dead || now > aggro.UntilAt)
        {
            ClearTarget(a.EntityId);
            return false;
        }

        WorldPosition p = target.CurrentPosition;
        WorldPosition me = a.PositionAt(now);          // ตำแหน่งจริงระหว่างเดิน ไม่ใช่ปลายทาง
        float dx = p.x - me.x;
        float dy = p.y - me.y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);

        if (dist > GiveUpDistance)
        {
            ClearTarget(a.EntityId);
            return false;
        }

        if (aggro.Flee)
        {
            if (now >= a.NextMoveAt)
            {
                // วิ่งออกจากผู้เล่นครั้งละ ~1 วินาที แล้วประเมินใหม่
                float len = dist <= 1f ? 1f : dist;
                WorldPosition dest = new WorldPosition(
                    me.x - dx / len * FleeSpeed,
                    me.y - dy / len * FleeSpeed);
                // วิ่งหนีลงทะเลไม่ได้ — เบนไปวิ่งเลียบฝั่งแทน
                if (!TryLandDestination(me, dest, InlandFor(a.EntityType), out dest))
                {
                    a.NextMoveAt = now + 0.5;      // จนมุมริมหาด ยืนนิ่งรอบนี้
                    return true;
                }
                Move move = a.MakeMove(dest, FleeSpeed, now, out double travel, running: true);
                a.NextMoveAt = now + Math.Max(travel, 0.5);
                a.StandAt = now + travel;
                _world.Broadcast(move);
            }
            return true;
        }

        if (dist > AttackRange)
        {
            if (now >= a.NextMoveAt)
            {
                // เดินเข้าหาทีละก้าว (หยุดห่างเท่าระยะตี) — ก้าวละ ~1 วินาที กัน client กระตุก
                // ก้าวยาว = ChaseSpeed × 1 วิ ดังนั้นเพิ่ม ChaseSpeed = เข้าถึงตัวเร็วขึ้นด้วย
                float len = dist <= 1f ? 1f : dist;
                float step = Math.Min(ChaseSpeed, dist - AttackRange * 0.5f);
                WorldPosition dest = new WorldPosition(
                    me.x + dx / len * step,
                    me.y + dy / len * step);
                if (!TryLandDestination(me, dest, InlandFor(a.EntityType), out dest))
                {
                    a.NextMoveAt = now + 0.5;
                    return true;
                }
                Move move = a.MakeMove(dest, ChaseSpeed, now, out double travel, running: true);
                a.NextMoveAt = now + Math.Max(travel, 0.4);
                a.StandAt = now + travel;
                _world.Broadcast(move);
            }
            return true;
        }

        if (now >= aggro.NextAttackAt)
        {
            aggro.NextAttackAt = now + AttackCooltimeOf(a.EntityType);
            float damage = SpawnTable.DamageFor(a.Level);

            // หันหน้าเข้าหาเหยื่อ + เล่นท่าโจมตี
            // ถ้าไม่ส่งอันนี้ ตัวจะค้างท่าเดินและหันไปทางที่เดินมาล่าสุด (ดูเหมือนกัดลม)
            string attackClip = AnimalMotionData.Attack(a.EntityType) ?? AnimalMotionData.Stand(a.EntityType);
            _world.Broadcast(a.MakeMotion(attackClip, ServerAnimal.YawTo(a.PositionAt(now), p), now, AttackClipSeconds));
            // คลิปโจมตีขยับ root bone ของโมเดลไปข้างหน้า พอเล่นจบไม่มีอะไรดึงกลับ
            // ตัวเลยค้างอยู่หน้าตำแหน่งจริง แล้ว packet ถัดไปกระชากกลับ = เห็นเป็นวาร์ป
            // ปิดท้ายด้วยท่ายืนที่ตำแหน่งจริงเสมอ
            a.StandAt = now + AttackClipSeconds;
            _world.Broadcast(new Damaged
            {
                AttackerId = a.EntityId,
                VictimId = target.EntityId,
                Damage = new Damage
                {
                    Result = Shared.Battle.DamageResult.Hit,
                    Value = (int)MathF.Round(damage),
                    Part = Shared.Battle.BodyPart.Body,
                    Direction = Shared.Battle.DamageDirection.Front,
                    AttackType = Shared.Battle.AttackType.SmallBody,
                    Effects = Shared.Battle.DamageEffects.None
                },
                EventAt = now
            });
            Console.WriteLine("[animal] {0} กัด {1} {2:F0} หน่วย (ท่า {3})", a.EntityId, target.Name, damage, attackClip);
            if (target.ApplyDamage(damage))
            {
                target.Die();
                ClearTarget(a.EntityId);
            }
        }
        return true;
    }

    private void ClearTarget(string animalId)
    {
        lock (_lock)
        {
            _targets.Remove(animalId);
        }
    }

    /// <summary>ซากหายตามเวลา + เกิดตัวใหม่ให้ครบจำนวน</summary>
    private void ProcessCorpsesAndRespawn(double now)
    {
        List<string> gone = null;
        List<ushort> due = null;
        List<string> freeze = null;
        lock (_lock)
        {
            foreach (KeyValuePair<string, double> pair in _freezeAt)
            {
                if (now >= pair.Value)
                {
                    (freeze ??= new List<string>()).Add(pair.Key);
                }
            }
            if (freeze != null)
            {
                for (int i = 0; i < freeze.Count; i++)
                {
                    _freezeAt.Remove(freeze[i]);
                }
            }
            foreach (KeyValuePair<string, double> pair in _corpses)
            {
                if (now >= pair.Value)
                {
                    (gone ??= new List<string>()).Add(pair.Key);
                }
            }
            for (int i = _respawnAt.Count - 1; i >= 0; i--)
            {
                if (now >= _respawnAt[i].at)
                {
                    (due ??= new List<ushort>()).Add(_respawnAt[i].type);
                    _respawnAt.RemoveAt(i);
                }
            }
            if (gone != null)
            {
                for (int i = 0; i < gone.Count; i++)
                {
                    _corpses.Remove(gone[i]);
                }
            }
        }

        if (freeze != null)
        {
            for (int i = 0; i < freeze.Count; i++)
            {
                if (TryGet(freeze[i], out ServerAnimal dead))
                {
                    string clip = AnimalMotionData.Die(dead.EntityType);
                    if (clip != null)
                    {
                        // เล่นคลิปเดิมด้วยความเร็ว 0 โดยข้ามไปเกือบท้ายคลิป = ค้างท่านอนตาย
                        _world.Broadcast(dead.MakeMotion(clip, dead.Yaw, now, 30.0,
                            loop: false, playbackRate: 0f, clipOffset: DeathClipSeconds));
                    }
                }
            }
        }
        if (gone != null)
        {
            for (int i = 0; i < gone.Count; i++)
            {
                Remove(gone[i]);            // ลบออกจากโลก + broadcast DisappearEntity
            }
        }
        if (due != null)
        {
            WorldPosition center = _world.GetEntryPosition();
            for (int i = 0; i < due.Count; i++)
            {
                SpawnTable.Entry e = SpawnTable.Find(due[i]) ?? SpawnTable.Entries[0];
                ServerAnimal born = SpawnFromTable(e, center, now);
                _world.Broadcast(born.MakeAppear());
                Console.WriteLine("[animal] เกิดใหม่ {0} lv{1} ({2}) — ในโลกตอนนี้ {3} ตัว",
                    e.Name, born.Level, born.EntityId, Count);
            }
        }
    }

    /// <summary>เอาสัตว์ออกจากโลก (ตาย/หมดเวลา/แล่หมดตัว)</summary>
    public void Remove(string entityId)
    {
        bool removed;
        lock (_lock)
        {
            removed = _animals.Remove(entityId ?? string.Empty);
            // ซากที่ถูกแล่หมดก่อนเวลา ต้องไม่ค้างอยู่ในคิวจนไปลบสัตว์ตัวใหม่ที่ใช้ id ซ้ำ
            _corpses.Remove(entityId ?? string.Empty);
            _freezeAt.Remove(entityId ?? string.Empty);
        }
        if (removed)
        {
            // ของที่แล่ไม่หมดต้องทิ้งไปพร้อมซาก ไม่งั้น _generators โตขึ้นเรื่อย ๆ ทุกตัวที่ตาย
            _world.ForgetGenerators(entityId);
            _world.Broadcast(new DisappearEntity { EntityId = entityId });
        }
    }
}
