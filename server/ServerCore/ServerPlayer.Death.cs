using System;
using Durango.Network;
using Messages;

namespace DurangoServer.Core;

public partial class ServerPlayer
{
    private bool _hasDeathPoint;
    private Point2 _deathTile;
    private int _immediateReviveCount;

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
        WorldPosition spawn = _world.GetEntryPosition();
        Region region = CurrentRegion();
        return new Points
        {
            HomePoint = null,
            ReturningPoint = new RegionTile
            {
                Region = region,
                Tile = new Point2((int)(spawn.x / 200f), (int)(spawn.y / 200f))
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
            Send(default(Abort), header.Seq);
            return;
        }
        Dead = false;
        _immediateReviveCount++;
        // [แก้เอง] 25 ส.ค. 2026 — เหมือน ReviveAtSpawn (ServerPlayer.Combat.cs) — เจ้าของสั่งให้ความล้า
        // รีเซ็ทตอนฟื้นด้วย ไม่ใช่แค่เลือด/สตามินา
        RestoreSurvival(clearFatigue: true);
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
    }

    private void FillDeathSave(PlayerSave save)
    {
        save.HasDeathPoint = _hasDeathPoint;
        save.DeathTileX = _deathTile.x;
        save.DeathTileY = _deathTile.y;
        save.ImmediateReviveCount = _immediateReviveCount;
        save.Dead = Dead;
    }
}
