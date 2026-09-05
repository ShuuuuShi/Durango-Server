using System;
using System.Collections.Generic;
using System.IO;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// TerrainPois / TerrainHerds — ข้อมูลที่ "มากับเกาะ" แต่เดิมไม่มีใครอ่าน
//
// เกาะของเกมทุกใบมี pois.yml กับ herds.yml แถมมาด้วย ซึ่งเก็บพิกัดที่ทีมสร้างเกม
// วางไว้เอง (ผ่านการตรวจแล้วว่าไม่ตกน้ำ ไม่ทับหิน) แต่เซิร์ฟเราไม่เคยอ่านเลย
// เลยไปสุ่มตำแหน่งเองทั้งหมด — เป็นที่มาของอาการ "POI ลอยกลางน้ำ / โดนหินทับ"
//
// ตรวจกับเกาะจริงแล้ว (ri35te / ri40tr / ri35de):
//   warphole 5 · rift 2 · port_point 1 · camp_artifact 1  ต่อเกาะ
//   ไม่มีจุดไหนอยู่ในน้ำ (oceans.dm > 0 ทุกจุด) และไม่มีจุดไหนอยู่ในหิน (cliffs.dm >= 0)
//   port_points ของ ri35te = (40,177) ตรงกับ entry_points เป๊ะ
// ============================================================================

/// <summary>POI ที่เกมกำหนดมาให้พร้อมเกาะ</summary>
public sealed class TerrainPois
{
    public List<Point2> Warpholes { get; } = new();
    public List<Point2> Rifts { get; } = new();
    public List<Point2> PortPoints { get; } = new();
    public List<Point2> Craters { get; } = new();
    public List<Point2> ScoopSlots { get; } = new();

    /// <summary>สิ่งปลูกสร้างที่เกมวางไว้ให้ (entity_type + พิกัด) เช่น neutral_warphole 9450</summary>
    public List<(ushort EntityType, Point2 Tile)> CampArtifacts { get; } = new();

    public int Total => Warpholes.Count + Rifts.Count + PortPoints.Count
                        + Craters.Count + ScoopSlots.Count + CampArtifacts.Count;

    public static TerrainPois? Load(string dir)
    {
        YamlNode? root = TerrainYaml.Load(Path.Combine(dir, "pois.yml"));
        if (root == null) { return null; }

        var pois = new TerrainPois();
        ReadPoints(root.Get("warpholes"), pois.Warpholes);
        ReadPoints(root.Get("rifts"), pois.Rifts);
        ReadPoints(root.Get("port_points"), pois.PortPoints);
        ReadPoints(root.Get("craters"), pois.Craters);
        ReadPoints(root.Get("scoop_slots"), pois.ScoopSlots);

        YamlNode? camps = root.Get("camp_artifacts");
        if (camps != null)
        {
            foreach (YamlNode entry in camps.AsList())
            {
                YamlNode? tile = entry.Get("tile");
                YamlNode? type = entry.Get("entity_type");
                if (tile != null && tile.TryPoint(out int x, out int y)
                    && type != null && type.TryInt(out int t) && t > 0 && t <= ushort.MaxValue)
                {
                    pois.CampArtifacts.Add(((ushort)t, new Point2(x, y)));
                }
            }
        }
        return pois.Total > 0 ? pois : null;
    }

    private static void ReadPoints(YamlNode? node, List<Point2> into)
    {
        if (node == null) { return; }
        foreach (YamlNode entry in node.AsList())
        {
            if (entry.TryPoint(out int x, out int y))
            {
                into.Add(new Point2(x, y));
            }
        }
    }
}

/// <summary>
/// จุดเกิดสัตว์ที่เกมกำหนดมาให้ แบ่งตามถิ่นที่อยู่
///
/// ri35te มี 811 จุด: beach 200 · land 200 · ocean 200 · lake_shallow 200 · lake_deep 11
/// **ไม่มีจุดไหนอยู่ในหินเลย** — ต่างจากการสุ่มเองที่ ~22-28% ของจุดที่ผ่านด่านเป็นเนื้อหิน
/// </summary>
public sealed class TerrainHerds
{
    /// <summary>ชื่อกลุ่ม (beach/land/ocean/lake_shallow/lake_deep) -> จุดเกิด</summary>
    public Dictionary<string, List<Point2>> Groups { get; } = new(StringComparer.Ordinal);

    /// <summary>กลุ่มที่สัตว์บกใช้ได้ (ตัดกลุ่มน้ำออก)</summary>
    public static readonly string[] LandGroups = { "land", "beach" };

    public int Total
    {
        get
        {
            int n = 0;
            foreach (List<Point2> g in Groups.Values) { n += g.Count; }
            return n;
        }
    }

    public List<Point2> Group(string name)
    {
        return Groups.TryGetValue(name, out List<Point2>? g) ? g : new List<Point2>();
    }

    public static TerrainHerds? Load(string dir)
    {
        YamlNode? root = TerrainYaml.Load(Path.Combine(dir, "herds.yml"));
        YamlNode? herds = root?.Get("herds");
        if (herds?.Map == null) { return null; }

        var result = new TerrainHerds();
        foreach (KeyValuePair<string, YamlNode> group in herds.Map)
        {
            var spots = new List<Point2>();
            foreach (YamlNode entry in group.Value.AsList())
            {
                YamlNode? tile = entry.Get("tile");
                if (tile != null && tile.TryPoint(out int x, out int y))
                {
                    spots.Add(new Point2(x, y));
                }
            }
            if (spots.Count > 0)
            {
                result.Groups[group.Key] = spots;
            }
        }
        return result.Total > 0 ? result : null;
    }
}
