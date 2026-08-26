using System;
using System.Collections.Generic;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Vision — "ใครเห็นอะไรอยู่บ้าง" (interest management)
//
// 🐛 ปัญหาเดิม: ทุกอย่างที่เกิดในโลกถูกส่งให้ **ทุกคนในเกาะ** (47 จุดที่เรียก Broadcast)
//    ⇒ คนเดิน 1 ก้าว = ส่งออกเท่าจำนวนคนออนไลน์ · โตแบบ N²
//    ที่ 100 คน เดินกันคนละ 2 ครั้ง/วินาที ≈ 20,000 packet ออกต่อวินาที
//    และ client ต้องรับ/วาดคนที่อยู่คนละมุมเกาะซึ่งมองไม่เห็นอยู่ดี
//
// วิธีแก้มี 2 ส่วน และ **ต้องมีทั้งคู่**:
//   1. ตอน broadcast → ส่งเฉพาะคนที่อยู่ในระยะ (ServerWorld.BroadcastNear)
//   2. ตัวนี้ → คอยดูว่าใครเพิ่ง "เข้ามาในระยะ" แล้วส่ง Appear ให้ · "หลุดออกไป" แล้วส่ง Disappear
//
// ถ้าทำแค่ข้อ 1 อย่างเดียว **คนที่เดินเข้ามาใหม่จะไม่มีวันเห็นใครเลย** เพราะเขาได้รับแต่
// packet ของสิ่งที่ตัวเองรู้จักอยู่แล้ว — ไม่มีอะไรบอกว่า "ตรงนั้นมีคนอยู่นะ"
//
// ⚠️ กติกาสำคัญ: **Appear ต้องออกทาง Observe* เท่านั้น** ห้ามยิง MakeAppear ตรง ๆ ผ่าน broadcast
//    ไม่งั้นคนรับจะได้ Appear ก่อน แล้วรอบตรวจถัดไปเห็นว่ายังไม่อยู่ในเซ็ต → **ส่งซ้ำอีกที**
//
// ระยะเข้า/ออกตั้งไม่เท่ากันโดยตั้งใจ (ViewRangeTiles / + ViewMarginTiles):
// ถ้าใช้ระยะเดียว คนที่ยืนพอดีขอบจะ **โผล่-หาย-โผล่-หาย** ทุกครั้งที่ขยับไม่กี่ก้าว
//
// ปรับได้สดที่ `data/config.json` → `World` (hot-reload 5 วิ ไม่ต้อง build)
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>entity id ของคนที่ "ตอนนี้เรามองเห็นอยู่" (client มี AppearPlayer ของเขาแล้ว)</summary>
    private readonly HashSet<string> _seenPlayers = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> _seenAnimals = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> _seenArtifacts = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>เอาไว้ใช้ซ้ำทุกรอบ ไม่ต้องจอง HashSet ใหม่ทุก 0.4 วินาทีคูณจำนวนคน</summary>
    private readonly HashSet<string> _visionScratch = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>คนนี้มองเห็น entity ตัวนี้อยู่ไหม</summary>
    public bool CanSee(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return false;
        }
        return entityId == EntityId
            || _seenPlayers.Contains(entityId)
            || _seenAnimals.Contains(entityId)
            || _seenArtifacts.Contains(entityId);
    }

    /// <summary>ระยะกำลังสองระหว่างเรากับจุดหนึ่ง (เทียบกำลังสองเพื่อไม่ต้องถอดราก)</summary>
    private float DistanceSqTo(WorldPosition at)
    {
        WorldPosition me = CurrentPosition;
        float dx = me.x - at.x;
        float dy = me.y - at.y;
        return dx * dx + dy * dy;
    }

    /// <summary>อยู่ในระยะที่จะ "เริ่มเห็น" ไหม</summary>
    private bool WithinViewEnter(WorldPosition at)
    {
        float r = ServerConfig.Current.World.ViewEnterUnits;
        return DistanceSqTo(at) <= r * r;
    }

    // ───────────────────────── ทางเข้าเดียวของการ "เห็น" ─────────────────────────

    /// <summary>เริ่มเห็นผู้เล่นคนนี้ — ส่ง Appear ให้ถ้ายังไม่เคยเห็น (กันส่งซ้ำ)</summary>
    public void ObservePlayer(ServerPlayer other)
    {
        if (other == null || other == this || string.IsNullOrEmpty(other.EntityId))
        {
            return;
        }
        if (!_seenPlayers.Add(other.EntityId))
        {
            return;
        }
        Send(other.MakeAppearPlayer());
        // หน้าตา/อุปกรณ์ที่ใส่อยู่ไม่ได้ติดมากับ AppearPlayer — ไม่ส่งตามจะเห็นเป็นตัวเปล่า
        Send(other.CurrentDisplay);
    }

    /// <summary>เริ่มเห็นสัตว์ตัวนี้</summary>
    public void ObserveAnimal(ServerAnimal a)
    {
        if (a == null || string.IsNullOrEmpty(a.EntityId))
        {
            return;
        }
        if (!_seenAnimals.Add(a.EntityId))
        {
            return;
        }
        // ความสูงพื้น: server ไม่มี heightmap เอง ใช้ค่าที่ผู้เล่นรายงานมา ไม่งั้นสัตว์จมใต้พื้น
        if (a.Height == 0f)
        {
            a.Height = CurrentHeight != 0f ? CurrentHeight : _world.GroundHeightHint;
        }
        Send(a.MakeAppear());
        if (!a.IsAlive)
        {
            // ซากที่ตายไปก่อนที่เราจะเดินมาถึง — ไม่บอกว่ามันตาย client จะวาดเป็นตัวยืนเฉย ๆ
            Send(new EntityDied { EntityId = a.EntityId, At = Durango.Utils.Times.UnixTimeNow() });
        }
    }

    /// <summary>เริ่มเห็นสิ่งปลูกสร้างนี้</summary>
    public void ObserveArtifact(AppearArtifact art)
    {
        if (string.IsNullOrEmpty(art.EntityId))
        {
            return;
        }
        if (!_seenArtifacts.Add(art.EntityId))
        {
            return;
        }
        Send(art);
    }

    /// <summary>
    /// ลืม entity นี้ไปเลย (ถูกลบออกจากโลกจริง ๆ — สัตว์หาย/บ้านถูกทุบ/คนออกเกม)
    /// ไม่ส่ง Disappear เพราะคนลบเป็นคนส่งเอง · ตรงนี้แค่ล้างเซ็ตไม่ให้ค้าง
    /// </summary>
    public void ForgetEntity(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return;
        }
        _seenPlayers.Remove(entityId);
        _seenAnimals.Remove(entityId);
        _seenArtifacts.Remove(entityId);
    }

    // ───────────────────────── รอบตรวจ ─────────────────────────

    /// <summary>
    /// ตรวจรอบเดียวว่าอะไรเข้า/ออกระยะบ้าง แล้วส่ง Appear/Disappear ให้ครบ
    ///
    /// เรียกจาก <c>ServerWorld.TickVisibility</c> ทุก <c>ViewCheckSeconds</c> วินาที
    /// (snapshot ส่งเข้ามาจากข้างนอกเพื่อไม่ให้ต้องล็อกโลกซ้ำต่อผู้เล่น 1 คน)
    /// </summary>
    public void TickVisibility(ServerPlayer[] players, ServerAnimal[] animals, AppearArtifact[] artifacts, double now)
    {
        WorldConfig cfg = ServerConfig.Current.World;
        float exitSq = cfg.ViewExitUnits * cfg.ViewExitUnits;

        // ── ผู้เล่น ──
        _visionScratch.Clear();
        for (int i = 0; i < players.Length; i++)
        {
            ServerPlayer other = players[i];
            if (other == this || string.IsNullOrEmpty(other.EntityId))
            {
                continue;
            }
            _visionScratch.Add(other.EntityId);
            WorldPosition at = other.CurrentPosition;
            if (_seenPlayers.Contains(other.EntityId))
            {
                if (DistanceSqTo(at) > exitSq)
                {
                    _seenPlayers.Remove(other.EntityId);
                    Send(new DisappearEntity { EntityId = other.EntityId });
                }
            }
            else if (WithinViewEnter(at))
            {
                ObservePlayer(other);
            }
        }
        PruneMissing(_seenPlayers, _visionScratch);

        // ── สัตว์ ──
        _visionScratch.Clear();
        for (int i = 0; i < animals.Length; i++)
        {
            ServerAnimal a = animals[i];
            if (a == null || string.IsNullOrEmpty(a.EntityId))
            {
                continue;
            }
            _visionScratch.Add(a.EntityId);
            WorldPosition at = a.PositionAt(now);
            if (_seenAnimals.Contains(a.EntityId))
            {
                if (DistanceSqTo(at) > exitSq)
                {
                    _seenAnimals.Remove(a.EntityId);
                    Send(new DisappearEntity { EntityId = a.EntityId });
                }
            }
            else if (WithinViewEnter(at))
            {
                ObserveAnimal(a);
            }
        }
        PruneMissing(_seenAnimals, _visionScratch);

        // ── สิ่งปลูกสร้าง ──
        _visionScratch.Clear();
        for (int i = 0; i < artifacts.Length; i++)
        {
            AppearArtifact art = artifacts[i];
            if (string.IsNullOrEmpty(art.EntityId))
            {
                continue;
            }
            _visionScratch.Add(art.EntityId);
            WorldPosition at = TileCenter(art.Tile);
            if (_seenArtifacts.Contains(art.EntityId))
            {
                if (DistanceSqTo(at) > exitSq)
                {
                    _seenArtifacts.Remove(art.EntityId);
                    Send(new DisappearEntity { EntityId = art.EntityId });
                }
            }
            else if (WithinViewEnter(at))
            {
                ObserveArtifact(art);
            }
        }
        PruneMissing(_seenArtifacts, _visionScratch);
    }

    /// <summary>กึ่งกลางของ tile เป็นพิกัดโลก (1 tile = 200 หน่วย)</summary>
    private static WorldPosition TileCenter(Point2 tile)
    {
        return new WorldPosition(tile.x * 200f + 100f, tile.y * 200f + 100f);
    }

    /// <summary>ตัด id ที่ไม่มีอยู่ในโลกแล้วออกจากเซ็ต (ไม่ต้องส่ง Disappear — ตัวที่หายส่งไปตอนถูกลบแล้ว)</summary>
    private static void PruneMissing(HashSet<string> seen, HashSet<string> alive)
    {
        if (seen.Count == 0)
        {
            return;
        }
        List<string> gone = null;
        foreach (string id in seen)
        {
            if (!alive.Contains(id))
            {
                (gone ??= new List<string>()).Add(id);
            }
        }
        if (gone != null)
        {
            for (int i = 0; i < gone.Count; i++)
            {
                seen.Remove(gone[i]);
            }
        }
    }

    /// <summary>
    /// ชุดข้อมูลรอบตัวตอนเพิ่งเข้าเกม — ส่งเฉพาะที่อยู่ในระยะ
    ///
    /// เดิม <c>ServerWorld.AddPlayer</c> ส่ง **สิ่งปลูกสร้างทั้งเกาะ + สัตว์ทั้งเกาะ + ผู้เล่นทุกคน**
    /// ให้คนที่เพิ่งเข้ามา · ที่ 100 คนคือ ~4,000 AppearArtifact ในชุดเดียว
    /// ตอนนี้ส่งแค่รอบตัว ที่เหลือ TickVisibility จะทยอยส่งตอนเดินไปถึงเอง
    /// </summary>
    public void SendInitialVision(ServerPlayer[] players, ServerAnimal[] animals, AppearArtifact[] artifacts)
    {
        bool culling = ServerConfig.Current.World.ViewCulling;
        for (int i = 0; i < artifacts.Length; i++)
        {
            if (!culling || WithinViewEnter(TileCenter(artifacts[i].Tile)))
            {
                ObserveArtifact(artifacts[i]);
            }
        }
        double now = Durango.Utils.Times.UnixTimeNow();
        for (int i = 0; i < animals.Length; i++)
        {
            ServerAnimal a = animals[i];
            if (a == null)
            {
                continue;
            }
            // ตอนเข้าเกมส่งเฉพาะตัวที่ยังมีชีวิต (พฤติกรรมเดิมของ Animals.SendAllTo)
            if (a.IsAlive && (!culling || WithinViewEnter(a.PositionAt(now))))
            {
                ObserveAnimal(a);
            }
        }
        for (int i = 0; i < players.Length; i++)
        {
            ServerPlayer other = players[i];
            if (other != this && (!culling || WithinViewEnter(other.CurrentPosition)))
            {
                ObservePlayer(other);
            }
        }
    }
}
