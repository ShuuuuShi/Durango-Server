using System;
using System.Collections.Generic;
using Messages;
using Shared.Estate;
using EstateRights = Shared.Estate.AccessRights;

namespace DurangoServer.Core;

/// <summary>
/// ที่ดินส่วนตัว (사유지) บนเกาะเสถียร — ประกาศ 4×4 · ขยายทีละช่อง · สิทธิ์เพื่อน/คนนอก · วาร์ปบ้าน
/// เก็บใน world.json ไม่ใช่เซฟผู้เล่น เพราะช่องที่ดินทับกันข้ามตัวละครได้
/// </summary>
public sealed class EstateManager
{
    /// <summary>
    /// ด้านของแปลงตอนประกาศ (หน่วย = ช่อง; 1 ช่อง = 4×4 tile)
    /// [2 ก.ย. 2026] เจ้าของสั่งลดจาก 4×4 เหลือ 2×2 — แปลงเริ่มต้น 16 ช่องกินที่เยอะเกินไปตอนคนเยอะ
    /// </summary>
    public const int InitialSide = 2;

    /// <summary>ช่องเริ่มต้นตอนประกาศ (2×2 = 4 ช่อง) — หน่วยเดียวกับ EstateLicense.Size ที่ client เอาไปโชว์</summary>
    public const int InitialCells = InitialSide * InitialSide;

    /// <summary>
    /// เพดานจำนวนช่องต่อแปลง — เดิม TryExpand ไม่มีเพดานเลย คนเดียวขยายกินทั้งเกาะได้
    /// (64 ช่อง = เท่ากับ 8×8 ถ้าขยายเป็นสี่เหลี่ยม)
    /// </summary>
    public const int MaxCells = 64;

    public const int UpkeepDays = 7;
    public const int TileUnits = 200;
    public const int TilesPerUnit = 4;
    public const int MaxEstateUnit = 63;

    private readonly ServerWorld _world;
    private readonly object _lock = new object();
    private readonly List<EstateRecord> _estates = new List<EstateRecord>();

    public EstateManager(ServerWorld world)
    {
        _world = world;
    }

    public void Load(List<EstateSave>? saves)
    {
        lock (_lock)
        {
            _estates.Clear();
            if (saves == null)
            {
                return;
            }
            for (int i = 0; i < saves.Count; i++)
            {
                EstateRecord? rec = EstateRecord.FromSave(saves[i]);
                if (rec != null)
                {
                    rec.NormalizeToEstateUnits();
                    // เซฟเก่าเก็บเป็น OwnerType.Player (แท็บ "เกาะอารยธรรม" ที่ล็อก Lv.30)
                    // ย้ายมาเป็น PersonalPlayer ให้ตรงกับช่อง PersonalEstate ที่เราส่งให้ client
                    if (rec.Type == OwnerType.Player)
                    {
                        rec.Type = OwnerType.PersonalPlayer;
                    }
                    _estates.Add(rec);
                }
            }
        }
        Console.WriteLine($"[estate] โหลด {_estates.Count} แปลง");
    }

    public List<EstateSave> ToSave()
    {
        lock (_lock)
        {
            var list = new List<EstateSave>(_estates.Count);
            for (int i = 0; i < _estates.Count; i++)
            {
                list.Add(_estates[i].ToSave());
            }
            return list;
        }
    }

    /// <summary>
    /// แปลงของผู้เล่นคนนี้ ไม่สนชนิด — คนละคนมีได้คนละ 1 แปลงอยู่แล้ว
    /// จำเป็นเพราะเซฟเก่าเก็บเป็น OwnerType.Player ส่วนของใหม่เป็น PersonalPlayer
    /// </summary>
    public EstateRecord? FindByOwner(string ownerId)
    {
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                if (_estates[i].OwnerId == ownerId)
                {
                    return _estates[i].Clone();
                }
            }
        }
        return null;
    }

    public EstateRecord? FindByOwner(string ownerId, OwnerType type)
    {
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                if (_estates[i].OwnerId == ownerId && _estates[i].Type == type)
                {
                    return _estates[i].Clone();
                }
            }
        }
        return null;
    }

    public EstateRecord? FindById(string estateId)
    {
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                if (_estates[i].Id == estateId)
                {
                    return _estates[i].Clone();
                }
            }
        }
        return null;
    }

    public bool TryDeclare(string ownerId, string ownerName, OwnerType type, Point2 cell, out EstateRecord rec, out string error)
    {
        rec = null!;
        error = "";
        if (type != OwnerType.Player && type != OwnerType.PersonalPlayer)
        {
            error = "ประกาศได้เฉพาะที่ดินส่วนตัว";
            return false;
        }
        // 🐛 [แก้เอง 2 ก.ย. 2026] เดิมบังคับเป็น OwnerType.Player เสมอ — ไม่ตรงกับช่องที่เราส่งข้อมูลไป
        //
        // ฝั่ง client: `EstateLicenses.PersonalEstate` ถูกวาดบนแท็บ "เกาะเทม" (OwnerType.PersonalPlayer)
        // แต่ `EstateGroup.TryOpen` เลือกแท็บจาก `License.Type` — Player = แท็บ "เกาะอารยธรรม" ที่ล็อก Lv.30
        // ⇒ เปิดเมนูมาเจอแท็บล็อกทั้งที่ที่ดินอยู่อีกแท็บ และเมนูที่ได้ก็เป็นคนละชุด
        // (EstatePage.Refresh: PersonalPlayer = ไม่มีปุ่มสละ/ต่ออายุ · Player = มี)
        //
        // ยึด PersonalPlayer เพราะเป็นแท็บที่ใช้ได้ทุกเลเวล (Player ต้อง Lv.30 + Role.Urban)
        type = OwnerType.PersonalPlayer;
        cell = ToEstateUnit(cell);
        lock (_lock)
        {
            if (FindLockedAnyType(ownerId) != null)
            {
                error = "มีที่ดินอยู่แล้ว — สละแปลงเก่าก่อนจึงจะประกาศใหม่ได้";
                return false;
            }
            var cells = new List<Point2>(InitialSide * InitialSide);
            for (int dx = 0; dx < InitialSide; dx++)
            {
                for (int dy = 0; dy < InitialSide; dy++)
                {
                    var p = new Point2(cell.x + dx, cell.y + dy);
                    if (OccupiedLocked(p, null))
                    {
                        error = "ช่องนี้มีที่ดินคนอื่นอยู่แล้ว";
                        return false;
                    }
                    cells.Add(p);
                }
            }
            double now = Durango.Utils.Times.UnixTimeNow();
            rec = new EstateRecord
            {
                Id = Guid.NewGuid().ToString("N"),
                OwnerId = ownerId,
                OwnerName = ownerName ?? "",
                Type = type,
                OriginX = cell.x,
                OriginY = cell.y,
                Size = cells.Count,
                LargestSize = cells.Count,
                ActivatedAt = now,
                UpkeepUntil = now + UpkeepDays * 86400.0,
                Others = EstateRights.Enter,
                Friends = EstateRights.Enter | EstateRights.UseFacility | EstateRights.Give,
                Cells = cells
            };
            _estates.Add(rec);
        }
        _world.MarkDirty();
        return true;
    }

    public bool TryExpand(string ownerId, string estateId, Point2 cell, out EstateRecord rec, out string error)
    {
        rec = null!;
        error = "";
        cell = ToEstateUnit(cell);
        lock (_lock)
        {
            EstateRecord? existing = FindLockedById(estateId);
            if (existing == null || existing.OwnerId != ownerId)
            {
                error = "ไม่พบที่ดินนี้";
                return false;
            }
            if (ContainsCell(existing, cell))
            {
                error = "ช่องนี้อยู่ในที่ดินแล้ว";
                return false;
            }
            if (!IsAdjacent(existing, cell))
            {
                error = "ขยายได้เฉพาะช่องที่ติดกับที่ดิน";
                return false;
            }
            if (OccupiedLocked(cell, existing.Id))
            {
                error = "ช่องนี้มีที่ดินคนอื่นอยู่แล้ว";
                return false;
            }
            if (existing.Cells.Count >= MaxCells)
            {
                error = $"ที่ดินขยายได้สูงสุด {MaxCells} ช่อง";
                return false;
            }
            existing.Cells.Add(cell);
            existing.Size++;
            if (existing.Size > existing.LargestSize)
            {
                existing.LargestSize = existing.Size;
            }
            rec = existing.Clone();
        }
        _world.MarkDirty();
        return true;
    }

    public bool TryShrink(string ownerId, string estateId, Point2 cell, out EstateRecord rec, out string error)
    {
        rec = null!;
        error = "";
        cell = ToEstateUnit(cell);
        lock (_lock)
        {
            EstateRecord? existing = FindLockedById(estateId);
            if (existing == null || existing.OwnerId != ownerId)
            {
                error = "ไม่พบที่ดินนี้";
                return false;
            }
            if (existing.Cells.Count <= InitialSide * InitialSide)
            {
                error = $"เล็กกว่า {InitialSide}×{InitialSide} ไม่ได้";
                return false;
            }
            int idx = IndexOfCell(existing, cell);
            if (idx < 0)
            {
                error = "ช่องนี้ไม่ได้อยู่ในที่ดิน";
                return false;
            }
            existing.Cells.RemoveAt(idx);
            existing.Size = existing.Cells.Count;
            rec = existing.Clone();
        }
        _world.MarkDirty();
        return true;
    }

    public bool TrySetRights(string ownerId, string estateId, Messages.AccessRights rights, out EstateRecord rec, out string error)
    {
        rec = null!;
        error = "";
        lock (_lock)
        {
            EstateRecord? existing = FindLockedById(estateId);
            if (existing == null || existing.OwnerId != ownerId)
            {
                error = "ไม่พบที่ดินนี้";
                return false;
            }
            existing.Others = rights.ForOthers;
            if (rights.ForFriends != null && rights.ForFriends.TryGetValue(Shared.Player.FriendType.JustFriend, out EstateRights friendRights))
            {
                existing.Friends = friendRights;
            }
            rec = existing.Clone();
        }
        _world.MarkDirty();
        return true;
    }

    public bool TryExtend(string ownerId, string estateId, out EstateRecord rec, out string error)
    {
        rec = null!;
        error = "";
        lock (_lock)
        {
            EstateRecord? existing = FindLockedById(estateId);
            if (existing == null || existing.OwnerId != ownerId)
            {
                error = "ไม่พบที่ดินนี้";
                return false;
            }
            double now = Durango.Utils.Times.UnixTimeNow();
            double baseAt = existing.UpkeepUntil > now ? existing.UpkeepUntil : now;
            existing.UpkeepUntil = baseAt + UpkeepDays * 86400.0;
            rec = existing.Clone();
        }
        _world.MarkDirty();
        return true;
    }

    public bool TryRemove(string ownerId, string estateId, out string error)
    {
        error = "";
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                if (_estates[i].Id == estateId && _estates[i].OwnerId == ownerId)
                {
                    _estates.RemoveAt(i);
                    _world.MarkDirty();
                    return true;
                }
            }
        }
        error = "ไม่พบที่ดินนี้";
        return false;
    }

    public static Point2 ToEstateUnit(Point2 cell)
    {
        if (cell.x > MaxEstateUnit || cell.y > MaxEstateUnit)
        {
            return new Point2(cell.x / TilesPerUnit, cell.y / TilesPerUnit);
        }
        return cell;
    }

    public EstateGrids BuildGrids(IList<Point2> chunks)
    {
        var cells = new Dictionary<Point2, string>();
        var licenses = new List<EstateLicense>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                EstateRecord rec = _estates[i];
                bool any = false;
                for (int c = 0; c < rec.Cells.Count; c++)
                {
                    Point2 unit = rec.Cells[c];
                    Point2 chunk = new Point2(unit.x / TilesPerUnit, unit.y / TilesPerUnit);
                    if (chunks != null && chunks.Count > 0 && !ContainsChunk(chunks, chunk))
                    {
                        continue;
                    }
                    cells[unit] = rec.Id;
                    any = true;
                }
                if (any && seen.Add(rec.Id))
                {
                    licenses.Add(rec.ToLicense());
                }
            }
        }
        Point2[] chunkArr;
        if (chunks == null || chunks.Count == 0)
        {
            chunkArr = Array.Empty<Point2>();
        }
        else
        {
            chunkArr = new Point2[chunks.Count];
            for (int i = 0; i < chunks.Count; i++)
            {
                chunkArr[i] = chunks[i];
            }
        }
        return new EstateGrids
        {
            Chunks = chunkArr,
            Cells = cells,
            EstateLicenses = licenses.ToArray()
        };
    }

    public EstateGrids BuildGridsFor(EstateRecord rec)
    {
        var chunks = new List<Point2>();
        var seen = new HashSet<long>();
        for (int i = 0; i < rec.Cells.Count; i++)
        {
            int cx = rec.Cells[i].x / TilesPerUnit;
            int cy = rec.Cells[i].y / TilesPerUnit;
            long key = ((long)cx << 32) ^ (uint)cy;
            if (seen.Add(key))
            {
                chunks.Add(new Point2(cx, cy));
            }
        }
        return BuildGrids(chunks);
    }

    private static bool ContainsChunk(IList<Point2> chunks, Point2 chunk)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            if (chunks[i].x == chunk.x && chunks[i].y == chunk.y)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>แปลงที่ครอบ tile นี้ (null = ที่สาธารณะ ใครก็ทำอะไรก็ได้)</summary>
    public EstateRecord? FindByTile(int tileX, int tileY)
    {
        Point2 unit = ToEstateUnit(new Point2(tileX, tileY));
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                if (ContainsCell(_estates[i], unit))
                {
                    return _estates[i].Clone();
                }
            }
        }
        return null;
    }

    public bool OwnsTile(string ownerId, int tileX, int tileY)
    {
        lock (_lock)
        {
            for (int i = 0; i < _estates.Count; i++)
            {
                if (_estates[i].OwnerId != ownerId)
                {
                    continue;
                }
                if (ContainsCell(_estates[i], ToEstateUnit(new Point2(tileX, tileY))))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private EstateRecord? FindLockedAnyType(string ownerId)
    {
        for (int i = 0; i < _estates.Count; i++)
        {
            if (_estates[i].OwnerId == ownerId)
            {
                return _estates[i];
            }
        }
        return null;
    }

    private EstateRecord? FindLocked(string ownerId, OwnerType type)
    {
        for (int i = 0; i < _estates.Count; i++)
        {
            if (_estates[i].OwnerId == ownerId && _estates[i].Type == type)
            {
                return _estates[i];
            }
        }
        return null;
    }

    private EstateRecord? FindLockedById(string estateId)
    {
        for (int i = 0; i < _estates.Count; i++)
        {
            if (_estates[i].Id == estateId)
            {
                return _estates[i];
            }
        }
        return null;
    }

    private bool OccupiedLocked(Point2 cell, string? exceptId)
    {
        for (int i = 0; i < _estates.Count; i++)
        {
            if (exceptId != null && _estates[i].Id == exceptId)
            {
                continue;
            }
            if (ContainsCell(_estates[i], cell))
            {
                return true;
            }
        }
        return false;
    }

    private static bool ContainsCell(EstateRecord rec, Point2 cell)
    {
        for (int i = 0; i < rec.Cells.Count; i++)
        {
            if (rec.Cells[i].x == cell.x && rec.Cells[i].y == cell.y)
            {
                return true;
            }
        }
        return false;
    }

    private static int IndexOfCell(EstateRecord rec, Point2 cell)
    {
        for (int i = 0; i < rec.Cells.Count; i++)
        {
            if (rec.Cells[i].x == cell.x && rec.Cells[i].y == cell.y)
            {
                return i;
            }
        }
        return -1;
    }

    private static bool IsAdjacent(EstateRecord rec, Point2 cell)
    {
        for (int i = 0; i < rec.Cells.Count; i++)
        {
            int dx = Math.Abs(rec.Cells[i].x - cell.x);
            int dy = Math.Abs(rec.Cells[i].y - cell.y);
            if (dx + dy == 1)
            {
                return true;
            }
        }
        return false;
    }
}

public sealed class EstateRecord
{
    public string Id = "";
    public string OwnerId = "";
    public string OwnerName = "";
    public OwnerType Type;
    public int OriginX;
    public int OriginY;
    public int Size;
    public int LargestSize;
    public double ActivatedAt;
    public double UpkeepUntil;
    public Shared.Estate.AccessRights Others;
    public Shared.Estate.AccessRights Friends;
    public List<Point2> Cells = new List<Point2>();

    public EstateRecord Clone()
    {
        return new EstateRecord
        {
            Id = Id,
            OwnerId = OwnerId,
            OwnerName = OwnerName,
            Type = Type,
            OriginX = OriginX,
            OriginY = OriginY,
            Size = Size,
            LargestSize = LargestSize,
            ActivatedAt = ActivatedAt,
            UpkeepUntil = UpkeepUntil,
            Others = Others,
            Friends = Friends,
            Cells = new List<Point2>(Cells)
        };
    }

    public EstateLicense ToLicense()
    {
        var friends = new Dictionary<Shared.Player.FriendType, EstateRights>
        {
            [Shared.Player.FriendType.JustFriend] = Friends
        };
        return new EstateLicense
        {
            EstateId = Id,
            Type = Type,
            OwnerId = OwnerId,
            ActivatedAt = ActivatedAt,
            DepositRunsOutAt = UpkeepUntil,
            ExpiresAt = UpkeepUntil + EstateManager.UpkeepDays * 86400.0,
            AccessRights = new Messages.AccessRights
            {
                ForOthers = Others,
                ForFriends = friends,
                ForClanMembers = null
            },
            Size = Size,
            RegionId = "",
            Tile = new Point2(OriginX * EstateManager.TilesPerUnit, OriginY * EstateManager.TilesPerUnit)
        };
    }

    public WorldPosition WarpPosition()
    {
        float tileX = OriginX * EstateManager.TilesPerUnit + EstateManager.TilesPerUnit * 0.5f;
        float tileY = OriginY * EstateManager.TilesPerUnit + EstateManager.TilesPerUnit * 0.5f;
        return new WorldPosition(tileX * EstateManager.TileUnits, tileY * EstateManager.TileUnits);
    }

    public void NormalizeToEstateUnits()
    {
        bool tiles = OriginX > EstateManager.MaxEstateUnit || OriginY > EstateManager.MaxEstateUnit;
        if (!tiles)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                if (Cells[i].x > EstateManager.MaxEstateUnit || Cells[i].y > EstateManager.MaxEstateUnit)
                {
                    tiles = true;
                    break;
                }
            }
        }
        if (!tiles)
        {
            Size = Cells.Count;
            return;
        }
        int ux = OriginX / EstateManager.TilesPerUnit;
        int uy = OriginY / EstateManager.TilesPerUnit;
        OriginX = ux;
        OriginY = uy;
        Cells.Clear();
        for (int dx = 0; dx < EstateManager.InitialSide; dx++)
        {
            for (int dy = 0; dy < EstateManager.InitialSide; dy++)
            {
                Cells.Add(new Point2(ux + dx, uy + dy));
            }
        }
        Size = Cells.Count;
        if (LargestSize < Size)
        {
            LargestSize = Size;
        }
    }

    public EstateSave ToSave()
    {
        var cells = new List<int[]>(Cells.Count);
        for (int i = 0; i < Cells.Count; i++)
        {
            cells.Add(new[] { Cells[i].x, Cells[i].y });
        }
        return new EstateSave
        {
            Id = Id,
            OwnerId = OwnerId,
            OwnerName = OwnerName,
            Type = (int)Type,
            OriginX = OriginX,
            OriginY = OriginY,
            Size = Size,
            LargestSize = LargestSize,
            ActivatedAt = ActivatedAt,
            UpkeepUntil = UpkeepUntil,
            Others = (int)Others,
            Friends = (int)Friends,
            Cells = cells
        };
    }

    public static EstateRecord? FromSave(EstateSave? save)
    {
        if (save == null || string.IsNullOrEmpty(save.Id) || string.IsNullOrEmpty(save.OwnerId))
        {
            return null;
        }
        var rec = new EstateRecord
        {
            Id = save.Id,
            OwnerId = save.OwnerId,
            OwnerName = save.OwnerName ?? "",
            Type = (OwnerType)save.Type,
            OriginX = save.OriginX,
            OriginY = save.OriginY,
            Size = save.Size,
            LargestSize = Math.Max(save.LargestSize, save.Size),
            ActivatedAt = save.ActivatedAt,
            UpkeepUntil = save.UpkeepUntil,
            Others = (EstateRights)save.Others,
            Friends = (EstateRights)save.Friends
        };
        if (save.Cells != null)
        {
            for (int i = 0; i < save.Cells.Count; i++)
            {
                int[] c = save.Cells[i];
                if (c != null && c.Length >= 2)
                {
                    rec.Cells.Add(new Point2(c[0], c[1]));
                }
            }
        }
        if (rec.Cells.Count == 0)
        {
            for (int dx = 0; dx < EstateManager.InitialSide; dx++)
            {
                for (int dy = 0; dy < EstateManager.InitialSide; dy++)
                {
                    rec.Cells.Add(new Point2(rec.OriginX + dx, rec.OriginY + dy));
                }
            }
            rec.Size = rec.Cells.Count;
        }
        return rec;
    }
}
