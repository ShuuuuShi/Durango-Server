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
public sealed partial class AnimalSpawner
{
    // Beta 1.0: จำนวน/ชนิด/เลเวล มาจาก SpawnTable ไม่ใช่การสุ่มแล้ว (ดู docs/testing/BETA-1.0-PLAN.md)

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
        // [TodoList/08] เกาะที่มี region template ของเกม → เกิดเป็นฝูงตามใบสั่งจริง (AnimalSpawner.Herds.cs)
        if (SpawnInitialHerds(now))
        {
            ReportSpawnDepth();
            return;
        }
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

        // [แก้เอง] ตรวจซ้ำว่าไม่มีตัวไหนไปยืนอยู่ในก้อนหิน — ต้องเป็น 0 เสมอ
        // (ก่อนแก้: จุดที่ด่านเดิมยอมให้เกิด 22-28% เป็นเนื้อหิน)
        int inRock = 0;
        int onHerd = 0;
        TerrainHerds? herds = _world.Terrain.Herds;
        var herdTiles = new HashSet<(int, int)>();
        if (herds != null)
        {
            foreach (string g in TerrainHerds.LandGroups)
            {
                foreach (Point2 t in herds.Group(g)) { herdTiles.Add((t.x, t.y)); }
            }
        }
        foreach (ServerAnimal a in Snapshot())
        {
            int tx = (int)(a.Position.x / 200f);
            int ty = (int)(a.Position.y / 200f);
            if (_world.Terrain.IsCliff(tx, ty)) { inRock++; }
            if (herdTiles.Contains((tx, ty))) { onHerd++; }
        }
        Console.WriteLine("[animal] ตรวจจุดเกิด: อยู่ในก้อนหิน {0} ตัว (ต้องเป็น 0) · ตรงจุดที่เกมกำหนดไว้ {1} ตัว",
            inRock, onHerd);

        // [3 ก.ย. 2026] ให้เห็นว่าสูตรรายชนิดมีผลจริงไหม — อัตราส่วนเทียบสัตว์อ้างอิง ที่เลเวลต่ำสุดของแต่ละชนิด
        if (ServerConfig.Current.Animals.SpeciesStats)
        {
            var parts = new List<string>();
            foreach (SpawnTable.Entry e in SpawnTable.Entries)
            {
                float lr = SpawnTable.LifeRatio(e.EntityType, e.MinLevel);
                float dr = SpawnTable.DamageRatio(e.EntityType, e.MinLevel);
                parts.Add($"{e.Name} เลือด×{lr:F2} ดาเมจ×{dr:F2}");
            }
            Console.WriteLine("[animal] พลังรายชนิด (เทียบ {0}): {1}",
                ServerConfig.Current.Animals.SpeciesReference, string.Join(" · ", parts));
        }
        // [TodoList/05] เกราะสัตว์ — บอกว่าแต่ละชนิดลดดาเมจกี่ % ที่เลเวลต่ำสุดของมัน (สูตรเดียวกับเกราะผู้เล่น)
        AnimalDefenseConfig defCfg = ServerConfig.Current.Animals.Defense;
        if (defCfg != null && defCfg.Enabled)
        {
            var seen = new HashSet<ushort>();
            var defParts = new List<string>();
            foreach (ServerAnimal a in Snapshot())
            {
                if (!seen.Add(a.EntityType)) { continue; }
                float def = SpawnTable.DefenseFor(a.EntityType, a.Level);
                float reduce = 1f - ServerPlayer.ArmorScaleFor(def);
                defParts.Add($"{NameOf(a.EntityType)} lv{a.Level} def {def:F0} ลด {reduce:P0}");
            }
            Console.WriteLine("[animal] เกราะรายชนิด (K={0}, scale={1}, cap {2:P0}): {3}",
                ServerConfig.Current.Combat.ArmorDefenseK, defCfg.Scale, ServerConfig.Current.Combat.ArmorMaxReduce, string.Join(" · ", defParts));
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
    /// <summary>[TodoList/08] ความลึกที่ตัวนี้ต้องใช้ — สมาชิกฝูงหาดใช้ค่าของฝูง (1) ตัวอื่นคิดจาก size_level</summary>
    private static int InlandOf(ServerAnimal a)
    {
        return a.MinInland >= 0 ? a.MinInland : InlandFor(a.EntityType);
    }

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
    /// โซนไหนผูกกับ "รอยแยก" (warp_accelerator) จุดไหนของเกาะนี้จริง — คิดครั้งเดียวตอนสัตว์ตัวแรกขอ ZoneCenter
    /// key = zone.Id ?? zone.Name — โซนที่ไม่มีในนี้ = ไม่มี crack ให้จับคู่ ใช้ offset เดิมจาก config แทน
    /// </summary>
    private Dictionary<string, WorldPosition> _zoneCrackAssignment;
    private readonly object _zoneCrackLock = new object();

    /// <summary>
    /// จับคู่โซนกับ "รอยแยก" (POI blueprint warp_accelerator) จริงในโลกนี้ — ตามเกมต้นฉบับที่ไดโนเสาร์
    /// เกิดเฉพาะบริเวณใกล้รอยแยกเท่านั้น (ยืนยันจาก RecipeData.cs: warp_accelerator → model crack_02)
    ///
    /// ⚠️ ปัญหา: เกาะหนึ่งมี warp_accelerator แค่ ~3 จุด (ดู ServerWorld.EnsureNaturalPOIs) แต่มี 4 โซนใน
    /// config — ถ้าบังคับทุกโซนแชร์ crack เดียวกันจะกระจุกเป็นแพ ถ้าลดโซนเหลือ 3 พื้นที่ส่วนอื่นของเกาะ
    /// จะไม่มีสัตว์เลย ⇒ จับคู่เท่าที่มี crack พอ (โซนที่จุดยึดเดิมอยู่ใกล้จุดเกิดที่สุด จับกับ crack ที่ใกล้
    /// จุดเกิดที่สุด ไล่ไปเรื่อย ๆ ตามระยะ) โซนที่เหลือ (มักเป็นโซนไกลสุด/นอกสุด) **ใช้ offset เดิมจาก
    /// config ต่อไป** — นี่คือ fallback "กระจายทั่วเกาะแบบเดิม" แบบเดียวกับที่ ZoneOf ใช้เมื่อสัตว์ไม่มีโซน
    /// เลย แค่ทำระดับโซนแทน ผลคือ ~3 ใน 4 โซนขยับไปเกาะ crack จริง ส่วนที่เหลือยังกระจายเหมือนเดิม
    /// ไม่ทิ้งพื้นที่ไหนให้โล่งเปล่า และถ้าเกาะไหน crack วางไม่สำเร็จเลย (0 จุด) ทุกโซนจะ fallback หมด
    /// พฤติกรรมเดิมก่อนแก้ ระบบไม่พังในกรณีขอบ
    /// </summary>
    private Dictionary<string, WorldPosition> ZoneCrackAssignment(WorldPosition entry)
    {
        lock (_zoneCrackLock)
        {
            if (_zoneCrackAssignment != null)
            {
                return _zoneCrackAssignment;
            }
            var result = new Dictionary<string, WorldPosition>(StringComparer.Ordinal);
            WorldPosition[] cracks = _world.GetCrackPositions();
            List<ZoneConfig> zones = ServerConfig.Current.Zones;
            if (cracks.Length > 0 && zones != null && zones.Count > 0)
            {
                Array.Sort(cracks, (a, b) => DistSq(a, entry).CompareTo(DistSq(b, entry)));

                var order = new List<ZoneConfig>(zones);
                order.Sort((a, b) =>
                {
                    float da = DistSq(new WorldPosition(entry.x + a.OffsetTileX * 200f, entry.y + a.OffsetTileY * 200f), entry);
                    float db = DistSq(new WorldPosition(entry.x + b.OffsetTileX * 200f, entry.y + b.OffsetTileY * 200f), entry);
                    return da.CompareTo(db);
                });

                int n = Math.Min(order.Count, cracks.Length);
                for (int i = 0; i < n; i++)
                {
                    result[order[i].Id ?? order[i].Name ?? i.ToString()] = cracks[i];
                }
            }
            foreach (KeyValuePair<string, WorldPosition> kv in result)
            {
                Console.WriteLine("[animal] โซน {0} ผูกกับรอยแยกจริงที่ tile {1:F0},{2:F0}", kv.Key, kv.Value.x / 200f, kv.Value.y / 200f);
            }
            if (zones != null && result.Count < zones.Count)
            {
                Console.WriteLine("[animal] โซนที่เหลือ {0} จาก {1} ไม่มีรอยแยกให้จับคู่ — ใช้ offset เดิมจาก config",
                    zones.Count - result.Count, zones.Count);
            }
            _zoneCrackAssignment = result;
            return result;
        }
    }

    private static float DistSq(WorldPosition a, WorldPosition b)
    {
        float dx = a.x - b.x, dy = a.y - b.y;
        return dx * dx + dy * dy;
    }

    /// <summary>
    /// กึ่งกลางโซนในพิกัดโลก
    ///
    /// จุดยึดหลัก: ตำแหน่ง "รอยแยก" (warp_accelerator) จริงในโลก ถ้าโซนนี้จับคู่ไว้ได้ (ดู ZoneCrackAssignment)
    /// — สัตว์จะได้เกิดใกล้รอยแยกจริงตามเกมต้นฉบับ ไม่ใช่พิกัดคงที่ที่ตั้งไว้ล่วงหน้า
    /// จุดยึดสำรอง: offset จากไฟล์ config (เดิม) — ใช้เมื่อโซนนี้ไม่มี crack จับคู่ หรือเกาะนี้ไม่มี crack เลย
    ///
    /// ⚠️ จุดเข้าเกมของเกาะอยู่ริมน้ำ (จุดที่เรือมาจอด) ระยะที่ตั้งไว้ในไฟล์จึงมีสิทธิ์ไปตกกลางทะเล
    /// ถ้าเจอแบบนั้นให้ **หาจุดบนบกที่ใกล้ที่สุดแทน** (ค้นเป็นวงออกไปทีละ 2 tile)
    /// ไม่งั้นสัตว์ทั้งโซนจะหาที่เกิดไม่ได้ แล้วไปโผล่บนหาดตามจุดสุดท้ายที่สุ่มได้
    /// (จุดยึดที่มาจาก crack ปกติผ่านเงื่อนไขบนบกอยู่แล้วเพราะ POI วางบนบกเสมอ แต่ยังเช็คซ้ำ
    /// เผื่อ MinTilesInland ของสัตว์ตัวใหญ่ในโซนลึกกว่าที่ POI ต้องการตอนวาง)
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

        string zoneKey = zone.Id ?? zone.Name ?? "?";
        WorldPosition wanted = ZoneCrackAssignment(entry).TryGetValue(zoneKey, out WorldPosition crack)
            ? crack
            : new WorldPosition(entry.x + zone.OffsetTileX * 200f, entry.y + zone.OffsetTileY * 200f);
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

    private ServerAnimal? SpawnFromTable(SpawnTable.Entry e, WorldPosition center, double now)
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

        // [แก้เอง] ใช้จุดเกิดที่มากับเกาะ (herds.yml) ก่อนเสมอ — ทีมสร้างเกมวางไว้ให้แล้ว
        // และไม่มีจุดไหนอยู่ในหิน  ถ้าเกาะไม่มีไฟล์นี้ค่อยตกไปสุ่มเองแบบเดิมข้างล่าง
        WorldPosition herdPreferred = zone != null ? ZoneCenter(zone, center) : center;
        if (TryHerdSpawnSpot(herdPreferred, zone, center, minDist, minInland, out WorldPosition herdSpot))
        {
            home = herdSpot;
            ok = true;
        }

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
                 && (zone != null || minDist <= 0f || Distance(home, center) >= minDist)
                 && !TooCloseToOther(home)
                 && IsSpawnSpotClear(home);
        }
        if (!ok && TryFindValidSpawnPosition(home, zone, center, minDist, minInland, out WorldPosition fallback))
        {
            home = fallback;
            ok = true;
            Console.WriteLine($"[animal] หาจุดสุ่มให้ {e.Name} ไม่ได้ใน 80 ครั้ง — ใช้จุด land ที่ตรวจซ้ำแล้ว tile {home.x / 200f:F0},{home.y / 200f:F0}");
        }
        if (!ok)
        {
            Console.WriteLine($"[animal] ข้ามการเกิด {e.Name}: หา land ที่ผ่านเงื่อนไขไม่ได้");
            return null;
        }

        ServerAnimal animal = new ServerAnimal(
            "animal_" + Guid.NewGuid().ToString("N").Substring(0, 12),
            e.EntityType, level, scale, home, SpawnTable.LifeFor(e.EntityType, level), now);
        animal.NextMoveAt = now + NextInterval();
        animal.Height = GroundHeightAt(home);

        lock (_lock)
        {
            _animals[animal.EntityId] = animal;
        }
        return animal;
    }

    private float GroundHeightAt(WorldPosition position)
    {
        return _world.Terrain.TryGetGroundHeight(position.x, position.y, out float height)
            ? height
            : _world.GroundHeightHint;
    }

    /// <summary>
    /// จุดนี้ว่างพอให้ไดโนเสาร์ยืนไหม — ต้องไม่ทับสิ่งปลูกสร้างและไม่ทับของธรรมชาติ (หิน/ต้นไม้)
    ///
    /// 🐛 [แก้เอง 1 ก.ย. 2026] เจ้าของแจ้ง: "ไดโนเสาร์มันเกิดในที่ที่เป็นสิ่งก่อสร้าง บอทเลยตีไม่ได้
    ///    มันเกิดในวาร์ปและในหิน" — เดิมเช็คแค่ 3 ข้อ (เป็นบก · ไกลจุดเข้าเกม · ไม่ชิดตัวอื่น)
    ///    ไม่เคยดูว่าช่องนั้นมีของตั้งอยู่หรือเปล่า ⇒ โผล่ทับจุดรับส่ง/กองหิน แล้วคลิกตีไม่โดน
    ///    เพราะ hitbox ของสิ่งปลูกสร้างบังอยู่ (เกมเลือกเป้าเป็นตัวสิ่งปลูกสร้างแทน)
    ///
    /// ช่องตัวเอง: ห้ามมีทั้ง artifact และ natural
    /// รอบตัว 3×3: ห้ามมี artifact (สิ่งปลูกสร้างกินพื้นที่หลายช่อง + ต้องมีที่ให้ขยับ)
    /// ไม่ห้าม natural รอบตัว เพราะเกาะมีต้นไม้/หินหนาแน่น จะหาที่เกิดไม่ได้เลย
    /// </summary>
    private bool IsSpawnSpotClear(WorldPosition pos)
    {
        int tx = (int)MathF.Floor(pos.x / 200f);
        int ty = (int)MathF.Floor(pos.y / 200f);
        // จุดศูนย์กลางห้ามทับของธรรมชาติ (หิน/ต้นไม้) — เกิดในหินไม่ได้
        // (การเดินยังกันรัศมี 3x3 ผ่าน IsSafeLand แยกต่างหาก)
        if (HasNaturalAt(tx, ty))
        {
            return false;
        }
        // [แก้เอง] 2 ก.ย. 2026 — ก้อนหินใหญ่/หน้าผา ต้นเหตุอาการ "สัตว์เกิดในหิน"
        //
        // ⚠️ ด่านนี้ต่างหากที่เป็นทางเกิดจริง (SpawnFromTable เรียกตัวนี้)
        //    ส่วน IsSafeLand ใช้ตอน *เดิน* — ใส่ไว้ที่นั่นอย่างเดียวไม่พอ แก้รอบแรกพลาดตรงนี้
        //
        // `whole.garden` (ที่ HasNaturalAt ดู) เก็บแค่ต้นไม้/หินเล็กที่เก็บได้
        // ก้อนหินใหญ่อยู่ใน `cliffs.dm` + ธง 0xC0 ของ `whole.biomes` ซึ่งเดิมไม่มีใครอ่าน
        if (_world.Terrain.IsCliff(tx, ty, CliffAvoidTiles))
        {
            return false;
        }
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (_world.HasArtifactAt(new Point2(tx + dx, ty + dy)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool TryFindValidSpawnPosition(WorldPosition preferred, ZoneConfig zone, WorldPosition entry, float minDist, int minInland, out WorldPosition result)
    {
        result = default;

        // [แก้เอง] 2 ก.ย. 2026 — ลองจุดเกิดที่ "มากับเกาะ" ก่อน (herds.yml)
        //
        // เกาะของเกมทุกใบมี herds.yml ที่ทีมสร้างเกมกำหนดจุดเกิดสัตว์ไว้ให้ แบ่งตามถิ่นที่อยู่
        // (ri35te: land 200 · beach 200 · ocean 200 · lake_shallow 200 · lake_deep 11 = 811 จุด)
        // **ไม่มีจุดไหนอยู่ในหินเลย** ต่างจากการสุ่มเองที่ 22-28% ของจุดที่ผ่านด่านเป็นเนื้อหิน
        // ถ้าไม่มีไฟล์ (เกาะที่ปั่นเอง) หรือจุดที่มีใช้ไม่ได้ ค่อยถอยไปกวาดหาแบบเดิม
        if (TryHerdSpawnSpot(preferred, zone, entry, minDist, minInland, out result))
        {
            return true;
        }

        int minX = 0, maxX = _world.Terrain.Width - 1, minY = 0, maxY = _world.Terrain.Height - 1;
        if (zone != null)
        {
            float radius = zone.RadiusTiles;
            minX = Math.Max(0, (int)Math.Floor(preferred.x / 200f - radius));
            maxX = Math.Min(_world.Terrain.Width - 1, (int)Math.Ceiling(preferred.x / 200f + radius));
            minY = Math.Max(0, (int)Math.Floor(preferred.y / 200f - radius));
            maxY = Math.Min(_world.Terrain.Height - 1, (int)Math.Ceiling(preferred.y / 200f + radius));
        }
        double best = double.MaxValue;
        bool found = false;
        WorldPosition zoneCenter = zone == null ? default : ZoneCenter(zone, entry);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                WorldPosition candidate = new WorldPosition(x * 200f + 100f, y * 200f + 100f);
                if (zone != null && Distance(candidate, zoneCenter) > zone.RadiusTiles * 200f) continue;
                if (minDist > 0f && Distance(candidate, entry) < minDist) continue;
                if (!_world.Terrain.IsLand(candidate.x, candidate.y, minInland) || TooCloseToOther(candidate)) continue;
                if (!IsSpawnSpotClear(candidate)) continue;
                double score = DistSq(candidate, preferred);
                if (score < best)
                {
                    best = score;
                    result = candidate;
                    found = true;
                }
            }
        }
        return found;
    }

    /// <summary>
    /// เลือกจุดเกิดจาก `herds.yml` ที่ใกล้จุดที่อยากได้ที่สุด และผ่านด่านเดียวกับการสุ่มเอง
    ///
    /// ใช้เฉพาะกลุ่มบก (land/beach) เพราะตัวนี้เรียกมาเพื่อหาที่ให้สัตว์บก
    /// กลุ่ม ocean/lake_* ยังไม่ได้ใช้ — เก็บไว้ให้ระบบสัตว์น้ำในอนาคต
    /// </summary>
    private bool TryHerdSpawnSpot(WorldPosition preferred, ZoneConfig zone, WorldPosition entry,
                                  float minDist, int minInland, out WorldPosition result)
    {
        result = default;
        TerrainHerds? herds = _world.Terrain.Herds;
        if (herds == null)
        {
            return false;
        }
        WorldPosition zoneCenter = zone == null ? default : ZoneCenter(zone, entry);
        double best = double.MaxValue;
        bool found = false;

        foreach (string group in TerrainHerds.LandGroups)
        {
            foreach (Point2 tile in herds.Group(group))
            {
                var candidate = new WorldPosition(tile.x * 200f + 100f, tile.y * 200f + 100f);
                if (zone != null && Distance(candidate, zoneCenter) > zone.RadiusTiles * 200f) { continue; }
                if (minDist > 0f && Distance(candidate, entry) < minDist) { continue; }
                if (!_world.Terrain.IsLand(candidate.x, candidate.y, minInland)) { continue; }
                if (TooCloseToOther(candidate) || !IsSpawnSpotClear(candidate)) { continue; }
                double score = DistSq(candidate, preferred);
                if (score < best)
                {
                    best = score;
                    result = candidate;
                    found = true;
                }
            }
        }
        return found;
    }

    private static float Distance(WorldPosition a, WorldPosition b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>จุดนี้อยู่ใกล้ไดโนเสาร์ตัวอื่นที่เกิดไปแล้วเกินระยะขั้นต่ำหรือเปล่า (กันจับกลุ่มเป็นแพ)</summary>
    private bool TooCloseToOther(WorldPosition pos)
    {
        float minSq = ServerConfig.Current.Animals.MinSeparationTiles * 200f;
        minSq *= minSq;
        lock (_lock)
        {
            foreach (ServerAnimal a in _animals.Values)
            {
                float dx = a.Position.x - pos.x;
                float dy = a.Position.y - pos.y;
                if (dx * dx + dy * dy < minSq)
                {
                    return true;
                }
            }
        }
        return false;
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
        float lifeMax = SpawnTable.LifeFor(type, level);

        ServerAnimal animal = new ServerAnimal(
            "animal_" + Guid.NewGuid().ToString("N").Substring(0, 12),
            type, level, scale, home, lifeMax, now);
        animal.NextMoveAt = now + NextInterval();
        animal.Height = GroundHeightAt(home);

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
        animal.Height = height != 0f ? height : GroundHeightAt(pos);
        _world.AnnounceAnimal(animal);
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
    private bool TryLandDestination(WorldPosition from, WorldPosition dest, int minInland, out WorldPosition result, bool allowBeach = false)
    {
        result = dest;
        if (IsSafeLand(dest.x, dest.y, minInland, allowBeach) && PathIsClear(from, dest, minInland, allowBeach))
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
                if (IsSafeLand(cand.x, cand.y, minInland, allowBeach) && PathIsClear(from, cand, minInland, allowBeach))
                {
                    result = cand;
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>ระยะกันชนรอบ footprint ของ POI/สิ่งปลูกสร้าง (หน่วย tile)</summary>
    private static float ArtifactAvoidTiles => ServerConfig.Current.Animals.ArtifactAvoidTiles;

    /// <summary>หิน/ต้นไม้กินพื้นที่กว้างกว่า 1 ช่อง — กันรอบปลายทางด้วยรัศมีนี้</summary>
    private static int NaturalAvoidRadius => ServerConfig.Current.Animals.NaturalAvoidRadius;

    /// <summary>เว้นระยะรอบก้อนหินเพิ่มอีกกี่ tile (0 = ห้ามเฉพาะ tile ที่เป็นเนื้อหิน)</summary>
    private static int CliffAvoidTiles => ServerConfig.Current.Animals.CliffAvoidTiles;

    /// <summary>
    /// เดินสุ่ม/ไล่/หนี ต้องเลี่ยงไม่ให้ปลายทางอยู่บนบก+ใกล้ POI/สิ่งปลูกสร้างเกินไป
    ///
    /// 🐛 ที่มา: `Process()` เดิมเช็คแค่ `Terrain.IsLand()` (บก vs ทะเล) — ไม่รู้จักก้อนหิน/พุ่มไม้
    /// ที่ client สุ่มวางตอนรัน (ข้อมูลฝั่ง client ล้วน ๆ เซิร์ฟไม่มีทางรู้ตำแหน่งจริง) ผลคือสัตว์เดินทะลุ
    /// เข้าไปในโขดหินได้ตรง ๆ (เจอจริงกับ `poi_near_camp_warphole_0` ที่วางทับก้อนหินใหญ่)
    ///
    /// ยังเช็ค collision รายก้อนหินไม่ได้ (เซิร์ฟไม่มีข้อมูล) — แต่ POI/สิ่งปลูกสร้างเป็นพิกัดที่เซิร์ฟ
    /// รู้แน่นอน (`SnapshotArtifacts`) และมักเป็นจุดที่ของตกแต่งหนาแน่นพอดี ⇒ กันชนรอบ footprint
    /// ไว้ก่อน ลดโอกาสเดินเข้าไปติดหินได้เยอะโดยไม่ต้องรู้ตำแหน่งหินจริง
    /// </summary>
    private bool IsSafeLand(float x, float y, int minInland)
    {
        return IsSafeLand(x, y, minInland, out _);
    }

    private bool IsSafeLand(float x, float y, int minInland, bool allowBeach)
    {
        return IsSafeLand(x, y, minInland, out _, allowBeach);
    }

    /// <summary>เวอร์ชันที่บอกด้วยว่าตกเพราะอะไร (ไว้ทำสถิติ "ทำไมสัตว์ไม่เดิน")</summary>
    /// <param name="reason">0 = ผ่าน · 1 = ไม่ใช่พื้นดิน/ริมน้ำเกิน · 2 = ใกล้สิ่งปลูกสร้าง · 3 = ใกล้ของธรรมชาติ · 4 = อยู่ใน/ติดหิน</param>
    private bool IsSafeLand(float x, float y, int minInland, out int reason, bool allowBeach = false)
    {
        reason = 0;
        if (!_world.Terrain.IsLand(x, y, minInland, allowBeach))
        {
            reason = 1;
            return false;
        }
        float pad = ArtifactAvoidTiles * 200f;
        foreach (Messages.AppearArtifact art in _world.SnapshotArtifacts())
        {
            float minX = art.Tile.x * 200f - pad;
            float maxX = (art.Tile.x + art.Size.x) * 200f + pad;
            float minY = art.Tile.y * 200f - pad;
            float maxY = (art.Tile.y + art.Size.y) * 200f + pad;
            if (x >= minX && x <= maxX && y >= minY && y <= maxY)
            {
                reason = 2;
                return false;
            }
        }
        // ปลายทางกันหินเป็นรัศมี 3×3 — ก้อนใหญ่กินหลายช่อง เช็คช่องเดียวไม่พอ
        int tx = (int)MathF.Floor(x / 200f);
        int ty = (int)MathF.Floor(y / 200f);
        if (HasNaturalNearby(tx, ty, NaturalAvoidRadius))
        {
            reason = 3;
            return false;
        }
        // [แก้เอง] หน้าผา/ก้อนหินใหญ่ — ต้นเหตุของอาการ "สัตว์เกิดในหิน"
        //
        // `whole.garden` เก็บแค่ต้นไม้/หินเล็กที่เก็บได้ ส่วนก้อนหินใหญ่ที่เดินทะลุไม่ได้
        // เป็นคนละชุดข้อมูล (`cliffs.dm` + ธง 0xC0 ใน `whole.biomes`) ซึ่งเดิมเซิร์ฟไม่เคยอ่าน
        // ⇒ ด่านนี้จึงมองไม่เห็นหินเลย สัตว์เกิดทับได้ตามปกติ
        // เกาะจริงมีหิน 5.8-7.8% ของพื้นที่ ไม่ใช่จำนวนน้อย ๆ
        if (_world.Terrain.IsCliff(tx, ty, CliffAvoidTiles))
        {
            reason = 4;
            return false;
        }
        return true;
    }

    private bool HasNaturalAt(int tx, int ty)
    {
        return _world.Terrain.TryGetNatural(tx, ty, out _);
    }

    private bool HasNaturalNearby(int tx, int ty, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (HasNaturalAt(tx + dx, ty + dy)) return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ช่องระหว่างทางเดินได้ไหม — เช็คช่องนั้นช่องเดียว (ไม่ใช้รัศมี 3×3)
    /// ไม่งั้นเส้นเดินบนเกาะที่มีหินหนาจะหาทางไม่เจอเลย
    /// </summary>
    private bool IsWalkableTile(float x, float y, int minInland, bool allowBeach = false)
    {
        if (!_world.Terrain.IsLand(x, y, minInland, allowBeach)) return false;
        int tx = (int)MathF.Floor(x / 200f);
        int ty = (int)MathF.Floor(y / 200f);
        if (HasNaturalAt(tx, ty)) return false;
        if (_world.Terrain.IsCliff(tx, ty)) return false;   // [แก้เอง] เดินทะลุก้อนหินไม่ได้
        float pad = ArtifactAvoidTiles * 200f;
        foreach (Messages.AppearArtifact art in _world.SnapshotArtifacts())
        {
            float minX = art.Tile.x * 200f - pad;
            float maxX = (art.Tile.x + art.Size.x) * 200f + pad;
            float minY = art.Tile.y * 200f - pad;
            float maxY = (art.Tile.y + art.Size.y) * 200f + pad;
            if (x >= minX && x <= maxX && y >= minY && y <= maxY) return false;
        }
        return true;
    }

    /// <summary>
    /// เส้นตรงจาก from→to ห้ามตัดช่องหิน/สิ่งปลูกสร้าง/ทะเล
    /// เดิมเช็คแค่ปลายทาง ⇒ ไล่/หนี/เดินสุ่มทะลุหินกลางทางได้
    /// </summary>
    private bool PathIsClear(WorldPosition from, WorldPosition to, int minInland, bool allowBeach = false)
    {
        float dx = to.x - from.x;
        float dy = to.y - from.y;
        float dist = MathF.Sqrt(dx * dx + dy * dy);
        if (dist < 1f) return true;
        int steps = Math.Max(1, (int)MathF.Ceiling(dist / 100f));
        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            float x = from.x + dx * t;
            float y = from.y + dy * t;
            if (i < steps)
            {
                if (!IsWalkableTile(x, y, minInland, allowBeach)) return false;
            }
            else if (!IsSafeLand(x, y, minInland, allowBeach))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// ถ้าสัตว์ยืนทับของธรรมชาติอยู่ ให้ดันออกไปช่องว่างรอบ ๆ ที่ใกล้ที่สุด (คืน true ถ้าสั่งย้าย)
    /// วนหาเป็นวงจากใกล้ไปไกล 1→3 ช่อง ถ้าไม่เจอเลยก็ปล่อยไว้ รอบหน้าค่อยลองใหม่
    /// </summary>
    private bool NudgeOutOfNatural(ServerAnimal a, double now)
    {
        WorldPosition here = a.PositionAt(now);
        int tx = (int)MathF.Floor(here.x / 200f);
        int ty = (int)MathF.Floor(here.y / 200f);
        if (!_world.Terrain.TryGetNatural(tx, ty, out _))
        {
            return false;
        }
        int minInland = InlandOf(a);
        for (int ring = 1; ring <= 3; ring++)
        {
            for (int dx = -ring; dx <= ring; dx++)
            {
                for (int dy = -ring; dy <= ring; dy++)
                {
                    if (Math.Abs(dx) != ring && Math.Abs(dy) != ring) continue;   // เฉพาะขอบวง
                    WorldPosition cand = new WorldPosition((tx + dx) * 200f + 100f, (ty + dy) * 200f + 100f);
                    if (!IsSafeLand(cand.x, cand.y, minInland, a.BeachOk) || !PathIsClear(here, cand, minInland, a.BeachOk)) continue;
                    a.Height = GroundHeightAt(cand);
                    Move move = a.MakeMove(cand, WalkSpeed, now, out double travel);
                    a.StandAt = now + travel;
                    a.NextMoveAt = now + travel + NextInterval();
                    _world.BroadcastToViewers(a.EntityId, move);
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
                    all[i].Height = GroundHeightAt(all[i].Position);
                    if (all[i].Height == 0f)
                    {
                        all[i].Height = player.CurrentHeight != 0f ? player.CurrentHeight : _world.GroundHeightHint;
                    }
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
        ProcessLifetimeExpiry(now);
        ProcessUnstuck(now);
        if (_herdMode) { MaintainHerds(now); } else { MaintainQuota(now); }

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
                _world.BroadcastToViewers(a.EntityId, a.MakeMotion(AnimalMotionData.Stand(a.EntityType), a.Yaw, now, 2.0, loop: true));
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
            // [แก้เอง 1 ก.ย. 2026] เจ้าของแจ้ง: "ยังมีไดโนเสาร์ติดในหิน" — ตัวเกมให้สัตว์ *เดินทะลุ*
            // ของธรรมชาติได้ (ของเดิมก็เป็นแบบนั้น) ปัญหาคือมันไป **หยุด** ค้างกลางกองหิน
            // เกิดได้แม้ปลายทางผ่าน IsSafeLand แล้ว เพราะขาเดินถูกตัดกลางคัน (โดนตี/เลิกไล่/หมดเวลาขา)
            // แล้ว PositionAt(now) ไปตกกลางหินพอดี ⇒ คลิกตีไม่โดน เพราะกองหินบัง hitbox
            // ตรงนี้กวาดทุก tick: ใครยืนทับ natural อยู่ ดันออกไปช่องว่างที่ใกล้ที่สุดทันที
            if (NudgeOutOfNatural(a, now))
            {
                continue;
            }
            if (now < a.NextMoveAt)
            {
                continue;
            }
            // เดินสุ่มอยู่ในโซนของตัวเอง — ไม่หลุดไปทั่วเกาะ
            // (รัศมีเดินไม่เกินทั้งค่ากลางและขอบโซน แล้วดึงกลับถ้าเผลอออกนอกโซน)
            // สุ่มจุดหมายหลายจุดต่อ tick — เดิมสุ่มจุดเดียว พลาดทีก็ยืนรอเต็มวินาที
            int tries = Math.Max(1, ServerConfig.Current.Animals.WanderTriesPerTick);
            int inland = InlandOf(a);
            WorldPosition here = a.PositionAt(now);
            bool found = false;
            WorldPosition dest = here;
            int why = 0;
            bool landOk = false, pathOk = false;
            for (int t = 0; t < tries && !found; t++)
            {
            // [TodoList/08] สมาชิกฝูงเดินรอบบ้านฝูงในรัศมีสั้น ๆ และไม่ถูกดึงเข้าโซนของโหมดเดิม
            bool inHerd = a.HerdId != 0;
            dest = RandomAround(a.Home, inHerd ? HerdCfg.WanderTiles * 200f : WanderRadius);
            ZoneConfig zone = inHerd ? null : ZoneOf(a.EntityType);
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
            float ldx = dest.x - here.x, ldy = dest.y - here.y;
            float legDist = MathF.Sqrt(ldx * ldx + ldy * ldy);
            if (legDist > maxLeg && legDist > 1f)
            {
                dest = new WorldPosition(here.x + ldx / legDist * maxLeg, here.y + ldy / legDist * maxLeg);
            }
            // ไม่เดินลงทะเล/หิน/สิ่งปลูกสร้าง ทั้งปลายทางและทุกช่องกลางทาง
            landOk = IsSafeLand(dest.x, dest.y, inland, out why, a.BeachOk);
            pathOk = landOk && PathIsClear(here, dest, inland, a.BeachOk);
            found = landOk && pathOk;
            }
            if (!found)
            {
                // สถิติไว้ตอบคำถาม "ทำไมไดโนไม่เดิน" — ถ้าตัวเลขนี้ชนเพดานตลอด
                // แปลว่าเงื่อนไขจุดหมาย (IsSafeLand/PathIsClear) แน่นเกินไปสำหรับแมพนี้
                if (!landOk)
                {
                    _wanderRejectLand++;
                    if (why == 1) _rejectWater++;
                    else if (why == 2) _rejectArtifact++;
                    else if (why == 3) _rejectNatural++;
                }
                else _wanderRejectPath++;
                a.NextMoveAt = now + 1.0;
                continue;
            }
            _wanderAccepted++;
            a.Height = GroundHeightAt(dest);
            Move move = a.MakeMove(dest, WalkSpeed, now, out double travelSeconds);
            a.StandAt = now + travelSeconds;
            // ต้องรอให้เดินถึงก่อนแล้วค่อยพัก ไม่งั้นสั่งเดินใหม่ทับของเดิม
            // server จะคิดว่ามันถึงที่หมายแล้วทั้งที่ client ยังเดินอยู่ → ตัวกระตุกไปข้างหน้า
            a.NextMoveAt = now + travelSeconds + NextInterval();
            _world.BroadcastToViewers(a.EntityId, move);
        }
        ReportWanderStats(now);
    }

    // ── สถิติการเดินสุ่ม ────────────────────────────────────────────────
    private int _wanderAccepted;
    private int _wanderRejectLand;
    private int _wanderRejectPath;
    private int _rejectWater;
    private int _rejectArtifact;
    private int _rejectNatural;
    private double _nextWanderReportAt;

    /// <summary>
    /// ทุก 30 วินาที บอกว่าการสุ่มจุดหมายผ่าน/ตกเท่าไร
    /// ตกเกือบ 100% = สัตว์ยืนแข็งทั้งเกาะ (จุดหมายถูกปฏิเสธก่อนถึง MakeMove เสมอ)
    /// </summary>
    private void ReportWanderStats(double now)
    {
        if (_nextWanderReportAt == 0.0)
        {
            _nextWanderReportAt = now + 30.0;
            return;
        }
        if (now < _nextWanderReportAt) return;
        _nextWanderReportAt = now + 30.0;
        int total = _wanderAccepted + _wanderRejectLand + _wanderRejectPath;
        if (total > 0)
        {
            Console.WriteLine("[animal] สุ่มจุดเดิน 30 วิที่ผ่านมา: เดินจริง {0} · ตกเพราะพื้นที่ {1} · ตกเพราะทางเดิน {2} ({3:P0} เดินไม่ได้)",
                _wanderAccepted, _wanderRejectLand, _wanderRejectPath,
                (double)(_wanderRejectLand + _wanderRejectPath) / total);
            Console.WriteLine("[animal]   แยกเหตุที่ตกเพราะพื้นที่: ริมน้ำ/ไม่ใช่พื้นดิน {0} · ใกล้สิ่งปลูกสร้าง {1} · ใกล้ของธรรมชาติ {2}",
                _rejectWater, _rejectArtifact, _rejectNatural);
        }
        _wanderAccepted = _wanderRejectLand = _wanderRejectPath = 0;
        _rejectWater = _rejectArtifact = _rejectNatural = 0;
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
        _world.BroadcastToViewers(animal.EntityId, new Survival { EntityId = animal.EntityId, Life = animal.LifeGauge() });
        // ไม่ส่ง `Damaged` ที่นี่ — `ServerPlayer.ResolveHit` ส่งแพ็กเก็ตนั้นอยู่แล้วก่อนเรียก Damage()
        // ถ้าส่งซ้ำ client จะวาดเลขดาเมจของเกมซ้อนกัน (ของเกมใช้ได้แล้ว ไม่ต้องมีชุดที่สอง)

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
            _world.BroadcastToViewers(animal.EntityId, animal.MakeMotion(dieClip, animal.Yaw, now, 2.0));
        }
        _world.BroadcastToViewers(animal.EntityId, new EntityDied { EntityId = animal.EntityId, At = now });

        // เปิดให้แล่เนื้อ: generator ของซากเป็นของกลางที่ world เหมือนจุดเก็บของธรรมชาติ
        // (ถ้าเก็บไว้ในตัวผู้เล่น สองคนจะแล่ซากเดียวกันได้ของครบทั้งคู่)
        _world.SetGenerators(animal.EntityId, ButcheryData.MakeGenerators(animal.EntityType, animal.Level));
        // client เอา DistributableEntities ไปเปิดไฟขอบเรืองแสงรอบซาก (AnimalBehavior.IsLootable)
        // ให้สิทธิ์เรืองแสงกับคนที่ฆ่า แต่ server ไม่ได้ห้ามคนอื่นแล่ — เล่นด้วยกันจะได้ช่วยกันเก็บได้
        if (!string.IsNullOrEmpty(attackerId))
        {
            _world.BroadcastToViewers(animal.EntityId, new CollectibleDisplay
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
            // [TodoList/08] สมาชิกฝูงเกิดใหม่ที่ฝูงเดิม (เติมฝูง) ไม่ใช่สุ่มที่ใหม่ตามโควตาชนิด
            if (_herdOf.TryGetValue(animal.EntityId, out Herd herdOfDead))
            {
                herdOfDead.PendingAt.Add(now + RespawnSeconds);
            }
            else
            {
                _respawnAt.Add((now + RespawnSeconds, animal.EntityType));
            }
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
        // [TodoList/08] ชนิดที่ไม่มีใน config Spawn → นิสัยจาก type ของเกม (Carnivore/Herbivore/Scavenger)
        return e?.Behavior ?? DefaultBehaviorOf(entityType);
    }

    private static bool IsTimid(ushort entityType)
    {
        return BehaviorOf(entityType) == AnimalBehavior.Flee;
    }

    /// <summary>คูลดาวน์การกัดของชนิดนี้ (ค่าจริงจากข้อมูลเกม — ดู SpawnTable)</summary>
    private static double AttackCooltimeOf(ushort entityType)
    {
        SpawnTable.Entry e = SpawnTable.Find(entityType);
        return e != null ? e.AttackCooltime : DefaultAttackCooltimeOf(entityType);
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
                if (!TryLandDestination(me, dest, InlandOf(a), out dest, a.BeachOk))
                {
                    a.NextMoveAt = now + 0.5;      // จนมุมริมหาด ยืนนิ่งรอบนี้
                    return true;
                }
                Move move = a.MakeMove(dest, FleeSpeed, now, out double travel, running: true);
                a.NextMoveAt = now + Math.Max(travel, 0.5);
                a.StandAt = now + travel;
                _world.BroadcastToViewers(a.EntityId, move);
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
                if (!TryLandDestination(me, dest, InlandOf(a), out dest, a.BeachOk))
                {
                    a.NextMoveAt = now + 0.5;
                    return true;
                }
                Move move = a.MakeMove(dest, ChaseSpeed, now, out double travel, running: true);
                a.NextMoveAt = now + Math.Max(travel, 0.4);
                a.StandAt = now + travel;
                _world.BroadcastToViewers(a.EntityId, move);
            }
            return true;
        }

        if (now >= aggro.NextAttackAt)
        {
            aggro.NextAttackAt = now + AttackCooltimeOf(a.EntityType);
            float damage = SpawnTable.DamageFor(a.EntityType, a.Level);

            // หันหน้าเข้าหาเหยื่อ + เล่นท่าโจมตี
            // ถ้าไม่ส่งอันนี้ ตัวจะค้างท่าเดินและหันไปทางที่เดินมาล่าสุด (ดูเหมือนกัดลม)
            string attackClip = AnimalMotionData.Attack(a.EntityType) ?? AnimalMotionData.Stand(a.EntityType);
            _world.BroadcastToViewers(a.EntityId, a.MakeMotion(attackClip, ServerAnimal.YawTo(a.PositionAt(now), p), now, AttackClipSeconds));
            // คลิปโจมตีขยับ root bone ของโมเดลไปข้างหน้า พอเล่นจบไม่มีอะไรดึงกลับ
            // ตัวเลยค้างอยู่หน้าตำแหน่งจริง แล้ว packet ถัดไปกระชากกลับ = เห็นเป็นวาร์ป
            // ปิดท้ายด้วยท่ายืนที่ตำแหน่งจริงเสมอ
            a.StandAt = now + AttackClipSeconds;

            // [4 ก.ย. 2026] บั๊ก #4 — ผู้เล่นกดท่าหลบอยู่ = ตีพลาด (ไม่เสียเลือด)
            // ต้องเช็คก่อนสร้าง Damaged ไม่งั้น client เห็น "โดน" ไปแล้วถึงจะหักดาเมจทีหลัง
            if (target.IsDodging)
            {
                _world.BroadcastToViewers(a.EntityId, new Damaged
                {
                    AttackerId = a.EntityId,
                    VictimId = target.EntityId,
                    Damage = new Damage
                    {
                        Result = Shared.Battle.DamageResult.Dodged,
                        Value = 0,
                        Part = Shared.Battle.BodyPart.Body,
                        Direction = Shared.Battle.DamageDirection.Front,
                        AttackType = Shared.Battle.AttackType.SmallBody,
                        Effects = Shared.Battle.DamageEffects.None
                    },
                    EventAt = now
                });
                Console.WriteLine("[animal] {0} ตี {1} แต่หลบพ้น (dodge)", a.EntityId, target.Name);
                return true;
            }

            _world.BroadcastToViewers(a.EntityId, new Damaged
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

    private double _nextUnstuckAt;
    private double _nextQuotaCheckAt;

    /// <summary>
    /// เติมโควตาที่ขาด (เช่น ตอนเกิดแรกหาจุดว่างไม่ได้) — ลองใหม่ทุก 15 วิ
    /// ไม่ให้ประชากรขาดเพราะข้ามจุดที่ทับหิน
    /// </summary>
    private void MaintainQuota(double now)
    {
        if (now < _nextQuotaCheckAt)
        {
            return;
        }
        _nextQuotaCheckAt = now + 15.0;
        // นับตัวเป็น + คิวเกิดใหม่ที่รออยู่ ต่อชนิด
        var alive = new Dictionary<ushort, int>();
        lock (_lock)
        {
            foreach (ServerAnimal a in _animals.Values)
            {
                if (!a.IsAlive) continue;
                alive[a.EntityType] = alive.TryGetValue(a.EntityType, out int n) ? n + 1 : 1;
            }
            for (int i = 0; i < _respawnAt.Count; i++)
            {
                ushort t = _respawnAt[i].type;
                alive[t] = alive.TryGetValue(t, out int n) ? n + 1 : 1;
            }
        }
        WorldPosition center = _world.GetEntryPosition();
        for (int i = 0; i < SpawnTable.Entries.Length; i++)
        {
            SpawnTable.Entry e = SpawnTable.Entries[i];
            int have = alive.TryGetValue(e.EntityType, out int h) ? h : 0;
            int need = e.Quota - have;
            for (int n = 0; n < need; n++)
            {
                ServerAnimal? born = SpawnFromTable(e, center, now);
                if (born == null)
                {
                    break; // รอบนี้หาจุดไม่ได้ — รอ 15 วิหน้า
                }
                _world.AnnounceAnimal(born);
                alive[e.EntityType] = (alive.TryGetValue(e.EntityType, out int cur) ? cur : 0) + 1;
                Console.WriteLine("[animal] เติมโควตา {0} ({1}) — มี {2}/{3}", e.Name, born.EntityId, alive[e.EntityType], e.Quota);
            }
        }
    }

    private static double LifetimeSeconds => ServerConfig.Current.Animals.LifetimeSeconds;

    /// <summary>
    /// สัตว์มีอายุ LifetimeSeconds (ค่าเริ่มต้น 300 วิ) แล้ว despawn + เกิดใหม่ที่จุดว่าง
    /// (ไม่ทับหิน/สิ่งปลูกสร้าง) — ไม่ทิ้งซาก เพราะไม่ใช่การตายจากการต่อสู้
    /// </summary>
    /// <remarks>ดู ProcessLifetimeExpiry ด้านล่าง</remarks>

    /// <summary>
    /// สัตว์ที่ยืนทับหิน/สิ่งปลูกสร้างพอดี (ช่องศูนย์กลาง) — despawn + เกิดใหม่ที่จุดว่าง
    /// ไม่ใช้รัศมีรอบตัว เพราะเกาะมีต้นไม้หนา จะ churn ตลอด
    /// </summary>
    private void ProcessUnstuck(double now)
    {
        // กันวน despawn ทุก tick ตอนเดินใกล้หินในรัศมี — เช็คเป็นช่วง ๆ พอ
        if (now < _nextUnstuckAt)
        {
            return;
        }
        _nextUnstuckAt = now + 5.0;

        List<(string id, ushort type)> stuck = null;
        ServerAnimal[] all = Snapshot();
        for (int i = 0; i < all.Length; i++)
        {
            ServerAnimal a = all[i];
            if (!a.IsAlive)
            {
                continue;
            }
            // เพิ่งเกิดใหม่ไม่กี่วิ — ยังไม่ unstuck ซ้ำ (กันวนกับจุดเกิด)
            if (now - a.SpawnedAt < 3.0)
            {
                continue;
            }
            WorldPosition here = a.PositionAt(now);
            int tx = (int)MathF.Floor(here.x / 200f);
            int ty = (int)MathF.Floor(here.y / 200f);
            // เฉพาะช่องที่ยืนอยู่ — ถ้าแค่ใกล้ต้นไม้/หิน ให้ NudgeOutOfNatural ดันออกแทน
            bool blocked = HasNaturalAt(tx, ty) || _world.HasArtifactAt(new Point2(tx, ty));
            if (!blocked)
            {
                continue;
            }
            (stuck ??= new List<(string, ushort)>()).Add((a.EntityId, a.EntityType));
        }
        if (stuck == null)
        {
            return;
        }
        WorldPosition center = _world.GetEntryPosition();
        for (int i = 0; i < stuck.Count; i++)
        {
            string id = stuck[i].id;
            ushort type = stuck[i].type;
            ClearTarget(id);
            // [TodoList/08] สมาชิกฝูง → กลับไปเกิดที่ฝูงเดิม (คิวก่อน Remove เพราะ Remove ลบ map ฝูง)
            if (QueueHerdRefill(id, now + 5.0))
            {
                Remove(id);
                Console.WriteLine("[animal] unstuck {0} (type {1}) — เป็นสมาชิกฝูง เกิดใหม่ที่ฝูงใน 5 วิ", id, type);
                continue;
            }
            Remove(id);
            SpawnTable.Entry e = SpawnTable.Find(type) ?? SpawnTable.Entries[0];
            ServerAnimal? born = SpawnFromTable(e, center, now);
            if (born == null)
            {
                lock (_lock)
                {
                    _respawnAt.Add((now + 5.0, type));
                }
                Console.WriteLine("[animal] unstuck {0} (type {1}) — หาจุดเกิดใหม่ไม่ได้ รอคิว 5 วิ", id, type);
                continue;
            }
            _world.AnnounceAnimal(born);
            Console.WriteLine("[animal] unstuck {0} → เกิดใหม่ {1} lv{2} ({3}) ที่ tile {4:F0},{5:F0}",
                id, e.Name, born.Level, born.EntityId, born.Home.x / 200f, born.Home.y / 200f);
        }
    }

    private void ProcessLifetimeExpiry(double now)
    {
        double life = LifetimeSeconds;
        if (life <= 0)
        {
            return;
        }
        List<(string id, ushort type)> expired = null;
        ServerAnimal[] all = Snapshot();
        for (int i = 0; i < all.Length; i++)
        {
            ServerAnimal a = all[i];
            if (!a.IsAlive)
            {
                continue;
            }
            if (now - a.SpawnedAt < life)
            {
                continue;
            }
            if (a.HerdId != 0)
            {
                continue;                 // [TodoList/08] ฝูงอยู่ถาวร ไม่หมดอายุ
            }
            (expired ??= new List<(string, ushort)>()).Add((a.EntityId, a.EntityType));
        }
        if (expired == null)
        {
            return;
        }
        WorldPosition center = _world.GetEntryPosition();
        for (int i = 0; i < expired.Count; i++)
        {
            string id = expired[i].id;
            ushort type = expired[i].type;
            ClearTarget(id);
            Remove(id);
            SpawnTable.Entry e = SpawnTable.Find(type) ?? SpawnTable.Entries[0];
            ServerAnimal? born = SpawnFromTable(e, center, now);
            if (born == null)
            {
                // หาจุดว่างไม่ได้ตอนนี้ — คิวเกิดใหม่เร็ว ๆ เพื่อไม่ให้โควตาขาด
                lock (_lock)
                {
                    _respawnAt.Add((now + 5.0, type));
                }
                Console.WriteLine("[animal] หมดอายุ {0} (type {1}) — หาจุดเกิดใหม่ไม่ได้ รอคิว 5 วิ", id, type);
                continue;
            }
            _world.AnnounceAnimal(born);
            Console.WriteLine("[animal] หมดอายุ {0} → เกิดใหม่ {1} lv{2} ({3}) ที่ tile {4:F0},{5:F0}",
                id, e.Name, born.Level, born.EntityId, born.Home.x / 200f, born.Home.y / 200f);
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
                        _world.BroadcastToViewers(dead.EntityId, dead.MakeMotion(clip, dead.Yaw, now, 30.0,
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
                ServerAnimal? born = SpawnFromTable(e, center, now);
                if (born == null)
                {
                    continue;
                }
                _world.AnnounceAnimal(born);
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
            ForgetHerdMember(entityId ?? string.Empty);
            // ซากที่ถูกแล่หมดก่อนเวลา ต้องไม่ค้างอยู่ในคิวจนไปลบสัตว์ตัวใหม่ที่ใช้ id ซ้ำ
            _corpses.Remove(entityId ?? string.Empty);
            _freezeAt.Remove(entityId ?? string.Empty);
        }
        if (removed)
        {
            // ของที่แล่ไม่หมดต้องทิ้งไปพร้อมซาก ไม่งั้น _generators โตขึ้นเรื่อย ๆ ทุกตัวที่ตาย
            _world.ForgetGenerators(entityId);
            _world.AnnounceGone(entityId);
        }
    }
}