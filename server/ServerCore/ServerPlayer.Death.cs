using System;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

public partial class ServerPlayer
{
    private bool _hasDeathPoint;
    private Point2 _deathTile;
    private int _immediateReviveCount;

    /// <summary>
    /// จุดเกิดที่บันทึกไว้จากท่าเรือ (SetReturningPoint) — ผู้เล่นเหยียบ checkpoint trigger บน dock
    /// client ส่ง SetReturningPoint มา เซิร์ฟบันทึก tile ไว้ใช้ตอนฟื้น / warp back
    /// null = ยังไม่เคยเหยียบ ใช้ entry point แทน
    /// </summary>
    private Point2? _returningPoint;

    private Region CurrentRegion()
    {
        return new Region
        {
            Id = "1",
            TerrainId = "1",
            TemplateId = _world.Terrain.Info.region_template,
            Role = GameServer.RegionRole,
            Name = _world.ServerName,
            CreatedAt = 0
        };
    }

    private Points BuildPoints()
    {
        Point2 rp = _returningPoint ?? _world.Terrain.EntryPoint;
        Region region = CurrentRegion();
        return new Points
        {
            HomePoint = null,
            ReturningPoint = new RegionTile
            {
                Region = region,
                Tile = rp
            },
            DeathPoint = _hasDeathPoint ? new RegionTile { Region = region, Tile = _deathTile } : null,
            LastReturnPoint = null,
            CampPoint = null
        };
    }

    private void RememberDeathPoint()
    {
        WorldPosition position = CurrentPosition;
        _deathTile = new Point2(
            Math.Clamp((int)(position.x / 200f), 0, ushort.MaxValue),
            Math.Clamp((int)(position.y / 200f), 0, ushort.MaxValue));
        _hasDeathPoint = true;
        MarkDirty();
        Send(BuildPoints());
    }

    private void HandleRemoveDeathPoint(RemoveDeathPoint msg, PacketHeader header)
    {
        _hasDeathPoint = false;
        MarkDirty();
        Send(default(OK), header.Seq);
        Send(BuildPoints());
    }

    private void HandleGetReviveImmediatelyInfo(GetReviveImmediatelyInfo msg, PacketHeader header)
    {
        Send(new ReviveImmediatelyInfo { UsedCount = _immediateReviveCount, TotalCost = default }, header.Seq);
    }

    private void HandleReviveImmediately(ReviveImmediately msg, PacketHeader header)
    {
        if (!Dead)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Dead = false;
        _immediateReviveCount++;
        // [TodoList/07] เกจตอนฟื้นตาม death_penalty (60/40/20/10%) — ปิด Death.Enabled = ฟื้นเต็ม+ล้างความล้าเหมือนเดิม
        RestoreOnRevive();
        MarkDirty();
        Send(default(Revived), header.Seq);
        _world.BroadcastToViewers(EntityId, new EntityRevived { EntityId = EntityId, At = Durango.Utils.Times.UnixTimeNow() });
        SendSurvivalPublic();
        QuestProgress(QuestData.Goal.Revive);
    }

    private void ApplyDeathSave(PlayerSave save)
    {
        _hasDeathPoint = save.HasDeathPoint;
        _deathTile = new Point2(save.DeathTileX, save.DeathTileY);
        _immediateReviveCount = Math.Max(0, save.ImmediateReviveCount);
        Dead = save.Dead;
        ApplyDeathPenaltySave(save);
        if (save.HasReturningPoint && save.ReturningPointX > 0 && save.ReturningPointY > 0)
        {
            _returningPoint = new Point2(save.ReturningPointX, save.ReturningPointY);
        }
    }

    private void FillDeathSave(PlayerSave save)
    {
        save.HasDeathPoint = _hasDeathPoint;
        save.DeathTileX = _deathTile.x;
        save.DeathTileY = _deathTile.y;
        save.ImmediateReviveCount = _immediateReviveCount;
        save.Dead = Dead;
        FillDeathPenaltySave(save);
        save.HasReturningPoint = _returningPoint.HasValue;
        save.ReturningPointX = _returningPoint?.x ?? 0;
        save.ReturningPointY = _returningPoint?.y ?? 0;
    }

    /// <summary>
    /// SetReturningPoint — client ส่งมาเมื่อผู้เล่นเดินเข้า trigger collider บน dock artifact
    /// (ดู PlayerTriggerMakeCheckPoint.cs ฝั่ง client)
    /// บันทึก tile นั้นเป็นจุดเกิดสำหรับ warp back / revive
    /// </summary>
    private void HandleSetReturningPoint(SetReturningPoint msg, PacketHeader header)
    {
        Point2 tile = msg.Tile;
        if (tile.x < 0 || tile.y < 0
            || tile.x >= _world.Terrain.Width || tile.y >= _world.Terrain.Height)
        {
            return;
        }
        _returningPoint = tile;
        MarkDirty();
        Send(BuildPoints());
        Console.WriteLine("[checkpoint] {0} บันทึกจุดเกิดที่ tile {1},{2}", Name, tile.x, tile.y);
    }
}
