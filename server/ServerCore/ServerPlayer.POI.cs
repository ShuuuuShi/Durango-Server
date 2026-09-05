using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.System;
using DurangoServer.Modding;
using Shared.Building;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.POI — ระบบจุดสนใจพิเศษ (Point of Interest)
//
// POI คือจุดพิเศษบนแผนที่ที่ผู้เล่นค้นหาด้วยปุ่ม "워프홀 탐지" (ค้นหาหลุมวาร์ป)
// มีหลายชนิด: Port (ท่าเรือ), Warphole (หลุมวาร์ป), Crater (หลุมอุกกาบาต),
// Rift (หลุมมอนด์/รอยแยกมิติ), CargoWarphole (หลุมวาร์ปส่งของ)
//
// flow ของเกมจริง:
//   client → GetLastSearchedTime   → server ตอบ LastSearchedTime (เวลาค้นหาล่าสุด)
//   client → GetPOICount           → server ตอบ POICount (จำนวนหลุมทั้งหมด)
//   client → SearchPOIs            → server ตอบ SearchedPOIs (รายการหลุมใกล้)
//   client → ExplorePOI            → server ตอบ OK (บันทึกว่าเจอหลุมแล้ว)
//   client → GetExploredPOIs       → server ตอบ ExploredPOIs (รายการหลุมที่เคยเจอ)
//   client → Warp{Tile}            → server วาร์ปผู้เล่นไปยัง tile นั้น
//   client → WarpBack              → server วาร์ปผู้เล่นกลับจุดเกิด
//   client → WarpToPort            → server วาร์ปผู้เล่นไปท่าเรือ
//
// POI มาจาก 2 แหล่ง:
// 1. Artifact ที่ผู้เล่นสร้าง (dock, camp_warphole, neutral_warphole, warp_accelerator)
// 2. POI ธรรมชาติที่ server สุ่มเกิดบนแผนที่ (เพื่อให้มีหลุมให้ค้นหา)
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>ระยะค้นหา POI (หน่วย tile) — ค้นหาได้ภายในระยะนี้จากตำแหน่งผู้เล่น</summary>
    private const int POISearchRadiusTiles = 50;

    /// <summary>เวลาค้นหาหลุมครั้งล่าสุด (Unix timestamp)</summary>
    private double _lastPOISearchedAt;

    /// <summary>POI ที่ผู้เล่นเคยค้นพบแล้ว (tile → type)</summary>
    private readonly Dictionary<Point2, Shared.System.PointOfInterest> _exploredPOIs = new();

    // ───────────────────────── ชื่อ blueprint → POI type ─────────────────────────

    /// <summary>แม็ป blueprint id → POI type สำหรับ artifact ที่ผู้เล่นสร้าง</summary>
    /// <remarks>internal: admin web panel (AdminPOI ใน Gateway) ใช้ตัวเดียวกันในการกรองว่า artifact ไหนนับเป็น POI</remarks>
    internal static readonly Dictionary<string, Shared.System.PointOfInterest> BlueprintPOIType = new()
    {
        { "dock", Shared.System.PointOfInterest.Port },
        { "camp_warphole", Shared.System.PointOfInterest.Warphole },
        { "neutral_warphole", Shared.System.PointOfInterest.CargoWarphole },
        { "cargo_warphole_in", Shared.System.PointOfInterest.CargoWarphole },
        { "warp_accelerator", Shared.System.PointOfInterest.Rift },
        { "warphole_personal", Shared.System.PointOfInterest.Warphole },
    };

    // ───────────────────────── Handler ─────────────────────────

    /// <summary>GetLastSearchedTime — client ขอเวลาค้นหาล่าสุด (ตอนเข้าเกม)</summary>
    private void HandleGetLastSearchedTime(GetLastSearchedTime msg, PacketHeader header)
    {
        Send(new LastSearchedTime { SearchedAt = _lastPOISearchedAt }, header.Seq);
    }

    /// <summary>GetPOICount — client ขอจำนวนหลุมทั้งหมดในภูมิภาค</summary>
    private void HandleGetPOICount(GetPOICount msg, PacketHeader header)
    {
        var counts = CountPOIs();
        Send(new POICount
        {
            PortCount = counts.port,
            WarpholeCount = counts.warphole,
            CraterCount = counts.crater,
            RiftCount = counts.rift
        }, header.Seq);
    }

    /// <summary>GetExploredPOIs — client ขอรายการหลุมที่เคยค้นพบ</summary>
    private void HandleGetExploredPOIs(GetExploredPOIs msg, PacketHeader header)
    {
        var pois = new List<Messages.PointOfInterest>();
        foreach (var kv in _exploredPOIs)
        {
            pois.Add(new Messages.PointOfInterest
            {
                Tile = kv.Key,
                Type = kv.Value,
                Icon = POIIcon(kv.Value),
                Title = null,
                EntityType = null,
                IsExplored = true
            });
        }
        Send(new ExploredPOIs
        {
            POIs = pois.ToArray(),
            FullCountRewarded = false,
            RewardCost = null,
            IsOpenedMap = true
        }, header.Seq);
    }

    /// <summary>SearchPOIs — ค้นหาหลุมใกล้ผู้เล่น ส่งผลกลับไป</summary>
    private void HandleSearchPOIs(SearchPOIs msg, PacketHeader header)
    {
        _lastPOISearchedAt = Times.UnixTimeNow();
        var results = SearchNearbyPOIs();
        Send(new SearchedPOIs
        {
            Results = results,
            SearchedAt = _lastPOISearchedAt
        }, header.Seq);
        Console.WriteLine("[poi] {0} ค้นหาหลุม เจอ {1} จุด", Name, results.Length);
    }

    /// <summary>ExplorePOI — client บอกว่าเจอ POI แล้ว (เดินใกล้พอ) → บันทึก</summary>
    private void HandleExplorePOI(ExplorePOI msg, PacketHeader header)
    {
        if (msg.Type == Shared.System.PointOfInterest.Invalid
            || msg.Tile.x < 0 || msg.Tile.y < 0
            || msg.Tile.x >= _world.Terrain.Width || msg.Tile.y >= _world.Terrain.Height
            || !IsWithinReach(msg.Tile))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        bool found = false;
        Point2 canonical = msg.Tile;
        foreach (var poi in AllPOIs())
        {
            if (poi.Type == msg.Type && POICoversTile(poi.Tile, poi.Size, msg.Tile))
            {
                found = true;
                canonical = poi.Tile;   // เก็บ tile มุมของ POI ไม่ใช่ tile ที่ผู้เล่นยืน
                break;
            }
        }
        if (!found)
        {
            Console.WriteLine("[poi] {0} สำรวจ {1} ที่ tile {2},{3} — ไม่เจอ POI ตรงนั้น",
                Name, msg.Type, msg.Tile.x, msg.Tile.y);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        _exploredPOIs[canonical] = msg.Type;
        Console.WriteLine("[poi] {0} ค้นพบ {1} ที่ tile {2},{3}", Name, msg.Type, canonical.x, canonical.y);
        MarkDirty();
        Send(default(OK), header.Seq);
    }

    // ───────────────────────── วาร์ป ─────────────────────────

    /// <summary>Warp — วาร์ปผู้เล่นไปยัง POI ที่ server ยืนยันแล้ว</summary>
    private void HandleWarp(Warp msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            RejectFeatureDisabled("IslandTravel", "Warp", "การเดินทางข้ามเกาะยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (msg.Tile.x < 0 || msg.Tile.y < 0
            || msg.Tile.x >= _world.Terrain.Width || msg.Tile.y >= _world.Terrain.Height)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        bool valid = false;
        foreach (var poi in AllPOIs())
        {
            if (poi.Type == Shared.System.PointOfInterest.Warphole
                && POICoversTile(poi.Tile, poi.Size, msg.Tile))
            {
                valid = true;
                break;
            }
        }
        if (!valid)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        WorldPosition dest = new WorldPosition(msg.Tile.x * 200f, msg.Tile.y * 200f);
        IModEventContext? travelBefore = PluginManager.Instance?.FireEvent("travel.leaving", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "warp", ["tile"] = msg.Tile.x + "," + msg.Tile.y });
        if (travelBefore?.IsCancelled == true) { Send(Aborts.Reason(), header.Seq); return; }
        Console.WriteLine("[warp] {0} วาร์ปไป tile {1},{2}", Name, msg.Tile.x, msg.Tile.y);
        RememberPosition(dest, 0f);
        SendTeleport(dest, Shared.Teleport.TeleportType.WarpBack);
        Send(default(OK), header.Seq);
        QuestProgress(QuestData.Goal.WarpLocal);
        PluginManager.Instance?.FireEvent("travel.entered", this, false, true,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "warp", ["tile"] = msg.Tile.x + "," + msg.Tile.y });
    }

    /// <summary>WarpBack — วาร์ปกลับจุดเกิด (ใช้ returning point ถ้ามี)</summary>
    private void HandleWarpBack(WarpBack msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            RejectFeatureDisabled("IslandTravel", "WarpBack", "การเดินทางข้ามเกาะยังไม่เปิดในรอบนี้", header);
            return;
        }
        WorldPosition dest = _returningPoint.HasValue
            ? new WorldPosition(_returningPoint.Value.x * 200f, _returningPoint.Value.y * 200f)
            : _world.GetEntryPosition();
        IModEventContext? travelBackBefore = PluginManager.Instance?.FireEvent("travel.leaving", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "warp_back" });
        if (travelBackBefore?.IsCancelled == true) { Send(Aborts.Reason(), header.Seq); return; }
        Console.WriteLine("[warp] {0} วาร์ปกลับจุดเกิด ({1},{2})", Name, (int)(dest.x / 200f), (int)(dest.y / 200f));
        RememberPosition(dest, 0f);
        SendTeleport(dest, Shared.Teleport.TeleportType.WarpBack);
        Send(default(OK), header.Seq);
        PluginManager.Instance?.FireEvent("travel.entered", this, false, true,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "warp_back" });
    }

    /// <summary>WarpToPort — วาร์ปไปท่าเรือ (หา dock ใกล้สุด ถ้าไม่มีไปจุดเกิด)</summary>
    private void HandleWarpToPort(WarpToPort msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            RejectFeatureDisabled("IslandTravel", "WarpToPort", "การเดินทางข้ามเกาะยังไม่เปิดในรอบนี้", header);
            return;
        }
        WorldPosition dest = FindNearestPOIPosition(Shared.System.PointOfInterest.Port);
        if (dest.x == 0f && dest.y == 0f)
        {
            dest = _world.GetEntryPosition();
        }
        IModEventContext? travelPortBefore = PluginManager.Instance?.FireEvent("travel.leaving", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "port" });
        if (travelPortBefore?.IsCancelled == true) { Send(Aborts.Reason(), header.Seq); return; }
        Console.WriteLine("[warp] {0} วาร์ปไปท่าเรือ ({1:F0},{2:F0})", Name, dest.x, dest.y);
        RememberPosition(dest, 0f);
        SendTeleport(dest, Shared.Teleport.TeleportType.WarpBack);
        Send(default(OK), header.Seq);
        PluginManager.Instance?.FireEvent("travel.entered", this, false, true,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["mode"] = "port" });
    }

    /// <summary>
    /// IsWarpholeAvailable — client ส่งทันทีหลังกดเมนู "วาป" (Interaction.Warp) ตอนแตะหลุมวาร์ป
    /// รอ reply เป็น OK ก่อนถึงจะเปิดแผนที่โหมดวาป (`WorldMapGroup.AddInteractionHandler` — ดู
    /// `.On&lt;OK&gt;(delegate { OpenForWarp(null); })`) [แก้เอง] 23 ส.ค. 2026 — เดิมไม่มี handler เลย
    /// client เลยรอ reply ไม่มาวันยังค่ำ แผนที่โหมดวาปเลยไม่เปิดสักที (ปุ่ม "วาป" กดแล้วเหมือนไม่มีอะไรเกิดขึ้น)
    /// ยังไม่มีระบบล็อกหลุมวาร์ป (เช่น ต้องมีเจ้าของ/ต้องเรียนสกิลก่อน) ⇒ ตอบ OK เสมอถ้า artifact
    /// ยังมีอยู่จริงและเป็น blueprint ตระกูลหลุมวาร์ป กันเผื่อ id เพี้ยน/ถูกทำลายไปแล้วระหว่างทาง
    /// </summary>
    private void HandleIsWarpholeAvailable(IsWarpholeAvailable msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            RejectFeatureDisabled("IslandTravel", "IsWarpholeAvailable", "การเดินทางข้ามเกาะยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact artifact)
            || artifact.Tile != msg.Tile
            || (artifact.States.BuildingState != BuildingState.Built
                && artifact.States.BuildingState != BuildingState.Completed)
            || !_world.TryGetArtifactBlueprint(msg.EntityId, out string bp)
            || !BlueprintPOIType.TryGetValue(bp ?? "", out var poiType)
            || poiType != Shared.System.PointOfInterest.Warphole)
        {
            Console.WriteLine("[warp] ปฏิเสธ {0}: {1} ไม่ใช่หลุมวาร์ปที่ใช้งานได้", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Send(default(OK), header.Seq);
    }

    /// <summary>
    /// GetWarpCosts — client เรียกก่อนเปิดโหมด "วาป" บนแผนที่โลกเสมอ (`WorldMapGroup.SetMapForWarp`)
    /// เพื่อเอาราคา/สถานะกดได้ของหลุมวาร์ปแต่ละจุดมาแปะป้ายบนไอคอน — [แก้เอง] 23 ส.ค. 2026: เดิมไม่มี
    /// handler เลย client เลยรอ reply ไม่มาวันยังค่ำ ⇒ ป้ายราคา/พื้นที่กดไม่โผล่บนไอคอนหลุมวาร์ปสักอัน
    /// (ดูเหมือน "ไม่มีเมนูกดวาป" ทั้งที่จริง ๆ คือขา request ตอบกลับหายไปเฉยๆ)
    ///
    /// ยังไม่มีระบบกระเป๋าเงิน/currency ในเซิร์ฟเลย (เหมือน WarpAccelerator — ดู WarpAcceleratorManager.cs
    /// ขอบเขต MVP ข้อ 1) ⇒ ตั้ง Cost = 0 เสมอ ไม่มีจุดไหน Prohibited (ยังไม่ทำระบบล็อกหลุมวาร์ป)
    /// รายการ POI ใช้ตัวเดียวกับ SearchNearbyPOIs/AllPOIs — กรองเอาเฉพาะ Warphole/CargoWarphole
    /// (client เป็นคนกรอง entry tile ปัจจุบันออกเอง ไม่ต้องกรองฝั่งนี้)
    /// </summary>
    private void HandleGetWarpCosts(GetWarpCosts msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            RejectFeatureDisabled("IslandTravel", "GetWarpCosts", "การเดินทางข้ามเกาะยังไม่เปิดในรอบนี้", header);
            return;
        }
        var costs = new List<WarpCost>();
        foreach (var poi in AllPOIs())
        {
            if (poi.Type != Shared.System.PointOfInterest.Warphole)
            {
                continue;
            }
            costs.Add(new WarpCost { Tile = poi.Tile, Cost = 0, Prohibited = false });
        }
        Send(new WarpCosts { Costs = costs.ToArray() }, header.Seq);
        Console.WriteLine("[warp] {0} ขอราคาหลุมวาร์ป — ตอบ {1} จุด", Name, costs.Count);
    }

    /// <summary>GetWarpBackCost — ราคาวาร์ปกลับจุดเกิด (`MapSystem.RequestWarpBackCost` อ่านแค่ Costs[0])
    /// [แก้เอง] เหมือน GetWarpCosts — ไม่เคยมี handler มาก่อน คู่กันเสมอ</summary>
    private void HandleGetWarpBackCost(GetWarpBackCost msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.IslandTravel)
        {
            RejectFeatureDisabled("IslandTravel", "GetWarpBackCost", "การเดินทางข้ามเกาะยังไม่เปิดในรอบนี้", header);
            return;
        }
        Point2 rp = _returningPoint ?? _world.Terrain.EntryPoint;
        Send(new WarpCosts
        {
            Costs = new[] { new WarpCost { Tile = rp, Cost = 0, Prohibited = false } }
        }, header.Seq);
    }

    // ───────────────────────── ตัวช่วย ─────────────────────────

    /// <summary>นับ POI ทั้งหมดในโลก แยกตามชนิด</summary>
    private (byte port, byte warphole, byte crater, byte rift) CountPOIs()
    {
        int port = 0, warphole = 0, crater = 0, rift = 0;
        foreach (var poi in AllPOIs())
        {
            switch (poi.Type)
            {
                case Shared.System.PointOfInterest.Port: port++; break;
                case Shared.System.PointOfInterest.Warphole: warphole++; break;
                case Shared.System.PointOfInterest.Crater: crater++; break;
                case Shared.System.PointOfInterest.Rift: rift++; break;
                case Shared.System.PointOfInterest.CargoWarphole: break;
            }
        }
        return ((byte)port, (byte)warphole, (byte)crater, (byte)rift);
    }

    private void ApplyPoiSave(PlayerSave save)
    {
        _lastPOISearchedAt = Math.Max(0d, save.LastPOISearchedAt);
        _exploredPOIs.Clear();
        if (save.ExploredPOIs == null)
        {
            return;
        }
        for (int i = 0; i < save.ExploredPOIs.Count; i++)
        {
            PoiDiscoverySave poi = save.ExploredPOIs[i];
            if (poi == null || poi.TileX < 0 || poi.TileY < 0
                || poi.TileX >= _world.Terrain.Width || poi.TileY >= _world.Terrain.Height)
            {
                continue;
            }
            var type = (Shared.System.PointOfInterest)poi.Type;
            if (type == Shared.System.PointOfInterest.Invalid
                || !Enum.IsDefined(typeof(Shared.System.PointOfInterest), type))
            {
                continue;
            }
            _exploredPOIs[new Point2(poi.TileX, poi.TileY)] = type;
        }
    }

    private void FillPoiSave(PlayerSave save)
    {
        save.LastPOISearchedAt = _lastPOISearchedAt;
        foreach (KeyValuePair<Point2, Shared.System.PointOfInterest> poi in _exploredPOIs)
        {
            save.ExploredPOIs.Add(new PoiDiscoverySave
            {
                TileX = poi.Key.x,
                TileY = poi.Key.y,
                Type = (int)poi.Value
            });
        }
    }

    /// <summary>ค้นหา POI ใกล้ผู้เล่น ส่งกลับเป็น SearchResult[]</summary>
    private SearchResult[] SearchNearbyPOIs()
    {
        var results = new List<SearchResult>();
        Point2 myTile = new Point2(
            (int)(CurrentPosition.x / 200f),
            (int)(CurrentPosition.y / 200f));

        foreach (var poi in AllPOIs())
        {
            int dx = poi.Tile.x - myTile.x;
            int dy = poi.Tile.y - myTile.y;
            int dist = dx * dx + dy * dy;
            if (dist <= POISearchRadiusTiles * POISearchRadiusTiles)
            {
                results.Add(new SearchResult
                {
                    Tile = poi.Tile,
                    Type = poi.Type
                });
            }
        }
        return results.ToArray();
    }

    /// <summary>
    /// POI นี้ครอบคลุม tile ที่ client ส่งมาไหม
    ///
    /// 🐛 [แก้เอง 30 ส.ค. 2026] **ต้นตอ "เดินชนวาร์ปแล้วไม่บันทึกเหมือนท่าเรือ"**
    /// POI ธรรมชาติถูกวางเป็น artifact ขนาด **6×6 tile** (`camp_warphole` ใน EnsureNaturalPOIs)
    /// แต่ `AllPOIs()` คืนแค่ tile มุมซ้ายล่าง แล้วโค้ดเทียบด้วย `poi.Tile == msg.Tile` เป๊ะ ๆ
    /// ⇒ client ส่ง tile ที่ผู้เล่นไปยืน/จุดกลางของหลุมมา ก็ไม่มีวันตรง ⇒ ตอบ Abort ทุกครั้ง
    /// (เห็นเป็นข้อความ "ServerPlayer.POI.HandleExplorePOI:140" กลางจอ)
    ///
    /// เผื่อขอบอีก 1 tile เพราะผู้เล่นยืน "ข้าง ๆ" หลุมตอนกดสำรวจ ไม่ได้ยืนทับพอดี
    /// </summary>
    private static bool POICoversTile(Point2 poiTile, Point2 poiSize, Point2 tile)
    {
        int sx = poiSize.x <= 0 ? 1 : poiSize.x;
        int sy = poiSize.y <= 0 ? 1 : poiSize.y;
        return tile.x >= poiTile.x - 1 && tile.x <= poiTile.x + sx
            && tile.y >= poiTile.y - 1 && tile.y <= poiTile.y + sy;
    }

    /// <summary>ดึง POI ทั้งหมดในโลก — จาก artifact ที่ผู้เล่นสร้าง + จากที่เคยค้นพบ</summary>
    private IEnumerable<(Point2 Tile, Point2 Size, Shared.System.PointOfInterest Type)> AllPOIs()
    {
        // 1. POI จาก artifact ที่ผู้เล่นสร้าง
        var artifacts = _world.SnapshotArtifacts();
        for (int i = 0; i < artifacts.Length; i++)
        {
            // อย่าจับคู่ Values/Keys จาก dictionary คนละรอบด้วย index — หลังมีการลบ/วาง
            // artifact ลำดับอาจไม่ตรงกัน ทำให้ `poi list` เห็น dock แต่ handler หาไม่เจอ
            // อ่าน blueprint จาก EntityId ของ artifact โดยตรงให้ทุกเส้นทางใช้ข้อมูลชุดเดียวกัน
            if (_world.TryGetArtifactBlueprint(artifacts[i].EntityId, out string bp)
                && bp != null && BlueprintPOIType.TryGetValue(bp, out var poiType))
            {
                yield return (artifacts[i].Tile, artifacts[i].Size, poiType);
            }
        }

        // 2. POI ที่เคยค้นพบ (จาก _exploredPOIs)
        foreach (var kv in _exploredPOIs)
        {
            yield return (kv.Key, new Point2(1, 1), kv.Value);
        }
    }

    /// <summary>หาตำแหน่ง POI ใกล้ผู้เล่นที่สุดตามชนิด</summary>
    private WorldPosition FindNearestPOIPosition(Shared.System.PointOfInterest type)
    {
        float bestDist = float.MaxValue;
        WorldPosition best = new WorldPosition(0, 0);
        foreach (var poi in AllPOIs())
        {
            if (poi.Type != type) continue;
            WorldPosition pos = new WorldPosition(poi.Tile.x * 200f, poi.Tile.y * 200f);
            float dx = pos.x - CurrentPosition.x;
            float dy = pos.y - CurrentPosition.y;
            float d = dx * dx + dy * dy;
            if (d < bestDist)
            {
                bestDist = d;
                best = pos;
            }
        }
        return best;
    }

    /// <summary>ชื่อไอคอนสำหรับ POI แต่ละชนิด</summary>
    private static string POIIcon(Shared.System.PointOfInterest type)
    {
        return type switch
        {
            Shared.System.PointOfInterest.Port => "icon_poi_port",
            Shared.System.PointOfInterest.Warphole => "icon_poi_warphole",
            Shared.System.PointOfInterest.Crater => "icon_poi_crater",
            Shared.System.PointOfInterest.Rift => "icon_poi_rift",
            Shared.System.PointOfInterest.CargoWarphole => "icon_poi_cargo_warphole",
            _ => null
        };
    }
}
