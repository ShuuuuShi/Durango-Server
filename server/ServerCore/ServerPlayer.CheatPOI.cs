using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Durango.Utils;
using Messages;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.CheatPOI — จัดการจุดสนใจ (POI) สด ๆ ตอนเซิร์ฟรันอยู่
//
// ทำไมต้องมี: เดิมแก้ตำแหน่งท่าเรือ/หลุมวาร์ปได้ทางเดียวคือ
//   หยุดเซิร์ฟ → แก้ saves/world.json ด้วยมือ → เปิดใหม่ → เข้าเกมไปดู
// ซึ่งรอบละหลายนาที ทั้งที่งานจริงคือ "ขยับทีละ 2-3 tile จนกว่าจะเข้าที่"
//
// ในเกม (แชท):
//   cheat poi list                    ดูทั้งหมด + สถานะว่าวางถูกที่ไหม
//   cheat poi check                   โชว์เฉพาะอันที่มีปัญหา
//   cheat poi tp <id>                 วาร์ปไปดูของจริง
//   cheat poi move <id> <x> <y>       ย้าย
//   cheat poi here <id>               ย้ายมาตรงที่ยืนอยู่
//   cheat poi remove <id>             ลบ
//   cheat poi add <blueprint> <x> <y> วางใหม่
//
// <id> พิมพ์แค่บางส่วนก็ได้ เช่น `near_dock` แทน `poi_near_dock_0`
//
// ⚙️ ลอจิกหลัก (list/check/move/remove/add) แยกเป็น static method รับ ServerWorld ตรง ๆ
// (ไม่ผูกกับ instance ผู้เล่นคนใดคนหนึ่ง) เพื่อให้ admin web panel (ดู Gateway.cs, /admin/poi/*)
// เรียกใช้ตัวเดียวกันได้โดยไม่ต้องมีผู้เล่นถืออยู่ — คำสั่งแชทข้างบนเป็นแค่ wrapper บาง ๆ ทับมันอีกที
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>blueprint ที่ `cheat poi add` วางได้ → entity type ที่ตัวเกมใช้เรนเดอร์</summary>
    internal static readonly Dictionary<string, (ushort Type, int SizeX, int SizeY)> POIBlueprints = new()
    {
        { "dock",             (7001, 3, 3) },
        { "camp_warphole",    (9101, 6, 6) },
        { "neutral_warphole", (9450, 6, 6) },
        { "warp_accelerator", (6282, 4, 4) },
    };

    /// <summary>แถวข้อมูล POI แบบโครงสร้าง — admin panel ใช้แปลงเป็น JSON, คำสั่งแชทใช้ต่อเป็นข้อความ</summary>
    internal struct PoiEntry
    {
        public string EntityId;
        public string ShortId;
        public string Blueprint;
        public int TileX, TileY;
        public int DistFromEntry;
        public string Problem; // null = ไม่มีปัญหา
    }

    private string CheatPOI(string args)
    {
        string[] a = (args ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        string verb = a.Length >= 1 ? a[0].ToLowerInvariant() : "list";

        switch (verb)
        {
            case "list":  return POIReportText(_world, onlyProblems: false);
            case "check": return POIReportText(_world, onlyProblems: true);
            case "tp":     return POITeleport(a);
            case "move":   return POIMove(a);
            case "here":   return POIMoveHere(a);
            case "remove": return POIRemove(a);
            case "add":    return POIAdd(a);
            default:
                return "ใช้: cheat poi list | check | tp <id> | move <id> <x> <y> | here <id> | remove <id> | add <blueprint> <x> <y>\n"
                     + "blueprint ที่วางได้: " + string.Join(" · ", POIBlueprints.Keys);
        }
    }

    // ───────────────────────── รายงาน (static — ไม่ผูกกับผู้เล่น) ─────────────────────────

    /// <summary>รายการ POI ทั้งหมด (หรือเฉพาะที่มีปัญหา) เป็นข้อมูลโครงสร้าง — ใช้ทั้งฝั่งแชทและ HTTP</summary>
    internal static List<PoiEntry> ListPOI(ServerWorld world, bool onlyProblems = false)
    {
        var list = new List<PoiEntry>();
        AppearArtifact[] all = world.SnapshotArtifacts();
        Point2 entry = world.Terrain.EntryPoint;
        foreach (AppearArtifact art in all)
        {
            if (!world.TryGetArtifactBlueprint(art.EntityId, out string bp) || bp == null)
            {
                continue;
            }
            if (!BlueprintPOIType.ContainsKey(bp))
            {
                continue;
            }
            string problem = DescribePOIProblem(world, art, bp);
            if (onlyProblems && problem == null)
            {
                continue;
            }
            int dx = art.Tile.x - entry.x, dy = art.Tile.y - entry.y;
            int dist = (int)Math.Sqrt(dx * dx + dy * dy);
            list.Add(new PoiEntry
            {
                EntityId = art.EntityId,
                ShortId = ShortPOIId(art.EntityId),
                Blueprint = bp,
                TileX = art.Tile.x,
                TileY = art.Tile.y,
                DistFromEntry = dist,
                Problem = problem
            });
        }
        return list;
    }

    /// <summary>ตรวจ POI ทุกชิ้นแล้วบอกว่าชิ้นไหนวางผิดที่ยังไง (ข้อความสำหรับแชท/console)</summary>
    private static string POIReportText(ServerWorld world, bool onlyProblems)
    {
        List<PoiEntry> entries = ListPOI(world, onlyProblems);
        if (entries.Count == 0)
        {
            return onlyProblems
                ? $"POI ทุกชิ้นวางถูกที่ (ตรวจแล้ว {ListPOI(world).Count} สิ่งปลูกสร้าง)"
                : "ยังไม่มี POI ในโลกนี้";
        }
        var sb = new StringBuilder();
        int bad = 0;
        foreach (PoiEntry e in entries)
        {
            if (e.Problem != null) bad++;
            sb.Append(e.ShortId)
              .Append("  ").Append(e.Blueprint)
              .Append("  tile ").Append(e.TileX).Append(',').Append(e.TileY)
              .Append("  ห่างจุดเกิด ").Append(e.DistFromEntry).Append(" tile");
            sb.Append(e.Problem == null ? "  [ok]" : "  [x] " + e.Problem);
            sb.Append('\n');
        }
        sb.Append("— รวม ").Append(entries.Count).Append(" ชิ้นที่แสดง · มีปัญหา ").Append(bad).Append(" ชิ้น");
        return sb.ToString();
    }

    /// <summary>คืนคำอธิบายปัญหา หรือ null ถ้าวางถูกที่</summary>
    private static string DescribePOIProblem(ServerWorld world, AppearArtifact art, string blueprint)
    {
        int sx = art.Size.x <= 0 ? 1 : art.Size.x;
        int sy = art.Size.y <= 0 ? 1 : art.Size.y;

        // 1. ทุก tile ใต้ตัวต้องเป็นบก — ⚠️ ใช้ LandDistance เท่านั้น (IsLand/WaterDepthAt พัง)
        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                if (world.Terrain.LandDistance(art.Tile.x + x, art.Tile.y + y) < 1)
                {
                    return $"มีส่วนอยู่ในน้ำ (tile {art.Tile.x + x},{art.Tile.y + y})";
                }
            }
        }

        // 2. ท่าเรือต้องติดแม่น้ำเท่านั้น (ไม่ใช่ทะเล/ทะเลสาบทั่วไป) — [แก้เอง] เจ้าของสั่ง
        //    เดิมเช็คด้วย TouchesWater (ติดน้ำอะไรก็ได้) เปลี่ยนมาเช็ค TouchesRiver ให้ตรงกับกฎวางใหม่
        if (blueprint == "dock" && !world.TouchesRiver(art.Tile.x, art.Tile.y, new Point2(sx, sy)))
        {
            return "ท่าเรือไม่ติดแม่น้ำ";
        }

        // 2.5 หลุมวาร์ป/รอยแยกต้องอยู่บนเกาะ ไม่ใช่ริมน้ำ — [แก้เอง] เจ้าของสั่ง (คนละอันกับข้อ 1
        //     ที่เช็คแค่ "ไม่จมน้ำ" — ข้อนี้เช็คว่าลึกเข้าเกาะพอไหม ตรงกับ minInland ที่ยกเป็น 6-10 ตอนวางใหม่)
        if (blueprint != "dock" && world.Terrain.LandDistance(art.Tile.x, art.Tile.y) < 6)
        {
            return "ใกล้น้ำเกินไป (ต้องอยู่บนเกาะ ไม่ใช่ริมฝั่ง)";
        }

        // 3. ของธรรมชาติทับตัว = หลุมโดนหิน/ต้นไม้บัง เดินเข้าไม่ถึง
        int under = 0;
        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                if (world.Terrain.TryGetNatural(art.Tile.x + x, art.Tile.y + y, out _))
                {
                    under++;
                }
            }
        }
        if (under > 0)
        {
            return $"มีต้นไม้/หินทับอยู่ {under} จุด";
        }
        return null;
    }

    // ───────────────────────── คำสั่งแก้ไข (static core + wrapper แชท) ─────────────────────────

    private string POITeleport(string[] a)
    {
        if (a.Length < 2) return "ใช้: cheat poi tp <id>";
        if (!TryFindPOI(_world, a[1], out AppearArtifact art, out string err)) return err;
        // ยืนข้าง ๆ ไม่ใช่บนตัวมัน จะได้เห็นทั้งชิ้น
        int sx = art.Size.x <= 0 ? 1 : art.Size.x;
        ControlTeleport(art.Tile.x + sx + 1, art.Tile.y);
        return $"วาร์ปไปข้าง {ShortPOIId(art.EntityId)} ที่ tile {art.Tile.x},{art.Tile.y} แล้ว";
    }

    private string POIMove(string[] a)
    {
        if (a.Length < 4 || !int.TryParse(a[2], out int tx) || !int.TryParse(a[3], out int ty))
        {
            return "ใช้: cheat poi move <id> <tileX> <tileY>";
        }
        if (!TryFindPOI(_world, a[1], out AppearArtifact art, out string err)) return err;
        return MovePOITo(_world, art, tx, ty);
    }

    private string POIMoveHere(string[] a)
    {
        if (a.Length < 2) return "ใช้: cheat poi here <id>  (ย้ายมาตรงที่ยืนอยู่)";
        if (!TryFindPOI(_world, a[1], out AppearArtifact art, out string err)) return err;
        WorldPosition at = CurrentPosition;
        return MovePOITo(_world, art, (int)(at.x / 200f), (int)(at.y / 200f));
    }

    /// <summary>ย้าย POI ไป tile ที่ระบุ — ใช้ร่วมกันทั้งคำสั่งแชทและ admin HTTP</summary>
    internal static string MovePOITo(ServerWorld world, AppearArtifact art, int tx, int ty)
    {
        int sx = art.Size.x <= 0 ? 1 : art.Size.x;
        int sy = art.Size.y <= 0 ? 1 : art.Size.y;
        if (tx < 0 || ty < 0 || tx + sx > world.Terrain.Width || ty + sy > world.Terrain.Height)
        {
            return $"tile {tx},{ty} อยู่นอกแผนที่ (แผนที่ {world.Terrain.Width}x{world.Terrain.Height})";
        }
        if (world.HasArtifactOverlapping(new Point2(tx, ty), new Point2(sx, sy), art.EntityId))
        {
            return $"tile {tx},{ty} มีของอื่นวางอยู่แล้ว";
        }
        if (!world.MoveArtifact(art.EntityId, new Point2(tx, ty)))
        {
            return "ย้ายไม่สำเร็จ (หา entity ไม่เจอ)";
        }
        // ตรวจซ้ำที่ตำแหน่งใหม่แล้วบอกเลย จะได้ไม่ต้องสั่ง check เอง
        world.TryGetArtifact(art.EntityId, out AppearArtifact now);
        world.TryGetArtifactBlueprint(art.EntityId, out string bp);
        string problem = DescribePOIProblem(world, now, bp);
        string head = $"ย้าย {ShortPOIId(art.EntityId)} ไป tile {tx},{ty} แล้ว";
        return problem == null ? head + " [ok]" : head + " ⚠️ " + problem;
    }

    private string POIRemove(string[] a)
    {
        if (a.Length < 2) return "ใช้: cheat poi remove <id>";
        return RemovePOI(_world, a[1]);
    }

    /// <summary>ลบ POI ตาม id (เต็มหรือบางส่วน) — ใช้ร่วมกันทั้งคำสั่งแชทและ admin HTTP</summary>
    internal static string RemovePOI(ServerWorld world, string idOrPartial)
    {
        if (!TryFindPOI(world, idOrPartial, out AppearArtifact art, out string err)) return err;
        string id = art.EntityId;
        if (!world.RemoveArtifact(id))
        {
            return "ลบไม่สำเร็จ";
        }
        world.AnnounceGone(id);
        // ⚠️ EnsureNaturalPOIs วางชุดที่ขาดกลับมาตอนเปิดเซิร์ฟใหม่ — ลบแล้วรีสตาร์ทมันจะโผล่ที่สุ่มใหม่
        return $"ลบ {ShortPOIId(id)} แล้ว (เปิดเซิร์ฟใหม่ระบบจะสุ่มวางชุดนี้กลับมา ถ้าไม่อยากให้กลับมาต้องแก้ EnsureNaturalPOIs)";
    }

    private string POIAdd(string[] a)
    {
        if (a.Length < 4 || !int.TryParse(a[2], out int tx) || !int.TryParse(a[3], out int ty))
        {
            return "ใช้: cheat poi add <blueprint> <tileX> <tileY>\nblueprint: " + string.Join(" · ", POIBlueprints.Keys);
        }
        return AddPOI(_world, a[1], tx, ty);
    }

    /// <summary>วาง POI ใหม่จาก blueprint — ใช้ร่วมกันทั้งคำสั่งแชทและ admin HTTP</summary>
    internal static string AddPOI(ServerWorld world, string blueprintRaw, int tx, int ty)
    {
        string bp = (blueprintRaw ?? string.Empty).ToLowerInvariant();
        if (!POIBlueprints.TryGetValue(bp, out var spec))
        {
            return $"ไม่รู้จัก blueprint '{bp}' — ใช้ได้: " + string.Join(" · ", POIBlueprints.Keys);
        }
        var size = new Point2(spec.SizeX, spec.SizeY);
        if (tx < 0 || ty < 0 || tx + size.x > world.Terrain.Width || ty + size.y > world.Terrain.Height)
        {
            return $"tile {tx},{ty} อยู่นอกแผนที่";
        }
        if (world.HasArtifactOverlapping(new Point2(tx, ty), size))
        {
            return $"tile {tx},{ty} มีของอื่นวางอยู่แล้ว";
        }
        // id ต้องไม่ชนของเดิม และต้องไม่ขึ้นต้นด้วย poi_<bp>_ ที่ EnsureNaturalPOIs ใช้เช็ค
        // ไม่งั้นวางเองแล้วชุดอัตโนมัติจะคิดว่ามีแล้วเลยไม่วางให้ (หรือกลับกัน)
        string id = "poi_manual_" + bp + "_" + DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                world.Terrain.RemoveNatural(tx + x, ty + y);
            }
        }
        AppearArtifact art = ArtifactFactory.Make(
            null, id, spec.Type, new Point2(tx, ty), size,
            Rotation.None, 0, 1, bp, BuildingState.Built);
        world.AddArtifact(art, bp);
        world.AnnounceArtifact(art);
        string problem = DescribePOIProblem(world, art, bp);
        string head = $"วาง {bp} ที่ tile {tx},{ty} แล้ว (id {ShortPOIId(id)})";
        return problem == null ? head + " [ok]" : head + " ⚠️ " + problem;
    }

    // ───────────────────────── ตัวช่วย ─────────────────────────

    /// <summary>หา POI จาก id เต็มหรือบางส่วน (ตรงตัวเดียวเท่านั้นถึงจะผ่าน)</summary>
    internal static bool TryFindPOI(ServerWorld world, string needle, out AppearArtifact art, out string error)
    {
        art = default;
        error = null;
        string want = (needle ?? string.Empty).ToLowerInvariant();
        if (want.Length == 0)
        {
            error = "ต้องบอก id ด้วย (ดูจาก `cheat poi list`)";
            return false;
        }
        var hits = new List<AppearArtifact>();
        var exact = new List<AppearArtifact>();
        foreach (AppearArtifact candidate in world.SnapshotArtifacts())
        {
            if (!world.TryGetArtifactBlueprint(candidate.EntityId, out string bp) || bp == null)
            {
                continue;
            }
            if (!BlueprintPOIType.ContainsKey(bp))
            {
                continue;
            }
            string full = candidate.EntityId.ToLowerInvariant();
            string shortId = ShortPOIId(candidate.EntityId).ToLowerInvariant();
            // ตรงตัวต้องชนะ: "warp_accelerator_0" ไม่งั้นไปโดน "near_warp_accelerator_0" ด้วย
            // แล้วสั่งอะไรก็ไม่ได้เพราะมันบอกว่ากำกวมตลอด
            if (full == want || shortId == want)
            {
                exact.Add(candidate);
            }
            else if (full.Contains(want))
            {
                hits.Add(candidate);
            }
        }
        if (exact.Count > 0)
        {
            hits = exact;
        }
        if (hits.Count == 0)
        {
            error = $"ไม่เจอ POI ที่ id มี '{needle}' (ดูรายชื่อด้วย `cheat poi list`)";
            return false;
        }
        if (hits.Count > 1)
        {
            var names = new List<string>();
            for (int i = 0; i < hits.Count; i++)
            {
                names.Add(ShortPOIId(hits[i].EntityId));
            }
            error = $"'{needle}' ตรงหลายอัน: " + string.Join(" · ", names) + " — พิมพ์ให้เจาะจงกว่านี้";
            return false;
        }
        art = hits[0];
        return true;
    }

    /// <summary>ตัด prefix "poi_" ออกให้อ่านง่ายในเกม (จอแชทแคบ)</summary>
    private static string ShortPOIId(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return "?";
        }
        return entityId.StartsWith("poi_", StringComparison.Ordinal) ? entityId.Substring(4) : entityId;
    }
}
