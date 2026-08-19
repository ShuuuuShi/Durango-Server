using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// ServerPlayer.Building — ดูรายละเอียดที่ docs/server/ServerPlayer.Building.md

public partial class ServerPlayer
{

    private static Item MakeCapsuleItem(string prototype, string name, string icon)
    {
        string blueprintId = prototype.StartsWith("capsulated_") ? prototype.Substring("capsulated_".Length) : prototype;
        return new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = name,
            Icon = icon,
            SubIcon = null,
            Prototype = prototype,
            Level = 1,
            OriginalLevel = 1,
            ModifiableCount = 0,
            ModifiedCount = 0,
            Size = 1,
            Durability = new Gauge(1f, 0f, new[] { new GaugeNode { Time = 0.0, Value = 1f } }),
            ColorR = "FFFFFF",
            ColorG = "FFFFFF",
            ColorB = "FFFFFF",
            Unstable = false,
            RepairRequirement = null,
            FounderId = null,
            FounderCategory = null,
            Tags = ItemTagData.For(prototype),
            TagModifications = null,
            Performance = null,
            Ext = new ArtifactCapsule
            {
                EntityId = null,
                BlueprintId = blueprintId,
                ArtifactLevel = 1,
                Tags = null,
                Performance = null,
                Display = default,
                State = default,
                LookNames = null,
                OccupySize = new Point2(1, 1)
            },
            CollectibleId = null,
            GeneratorId = null,
            EmotionalMotions = null,
            PioneerCost = 0f
        };
    }

    /// <summary>H-7: สร้างได้คนละกี่ชิ้น (กันบอทถมทั้งเกาะจนคนใหม่เข้าเกมไม่ได้)</summary>
    private const int MaxArtifactsPerPlayer = 40;

    private void HandleOccupyArtifactSite(OccupyArtifactSite msg, PacketHeader header)
    {
        if (Dead)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        // H-7: เดิมไม่ตรวจอะไรเลย — ปล่อยบอทค้างคืนได้บ้านหลายหมื่นหลัง
        // ทุกหลังถูกเซฟถาวรและถูกส่งให้ "ทุกคนที่เข้าเกม" ⇒ บัฟเฟอร์ส่งล้น = คนใหม่เข้าไม่ได้อีกเลย
        if (msg.Tile.x < 0 || msg.Tile.y < 0
            || msg.Tile.x >= _world.Terrain.Width || msg.Tile.y >= _world.Terrain.Height)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: tile {1},{2} อยู่นอกแมพ", Name, msg.Tile.x, msg.Tile.y);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!IsWithinReach(msg.Tile))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: tile {1},{2} ไกลเกินเอื้อม", Name, msg.Tile.x, msg.Tile.y);
            Send(default(Abort), header.Seq);
            return;
        }
        if (_world.HasArtifactAt(msg.Tile))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: tile {1},{2} มีสิ่งปลูกสร้างอยู่แล้ว", Name, msg.Tile.x, msg.Tile.y);
            Send(default(Abort), header.Seq);
            return;
        }
        int mine = _world.CountArtifactsOf(EntityId);
        if (mine >= MaxArtifactsPerPlayer)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: สร้างครบเพดานแล้ว ({1} ชิ้น)", Name, mine);
            Send(new Info { Text = $"สร้างได้สูงสุด {MaxArtifactsPerPlayer} ชิ้นต่อคน — ทุบของเก่าก่อน" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        // เฟส C
        if (!TrySpendStamina(StaminaCostBuild))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับก่อสร้าง", Name);
            Send(default(Abort), header.Seq);
            return;
        }
        string entityId = Guid.NewGuid().ToString();
        ushort entityType = 0;
        if (!RecipeData.BlueprintType.TryGetValue(msg.BlueprintId ?? "", out entityType))
        {
            Console.WriteLine("[build] occupy FAILED: unknown blueprint '{0}'", msg.BlueprintId);
            Send(default(Abort), header.Seq);
            return;
        }
        Point2 size = msg.Size;
        if ((size.x <= 0 || size.y <= 0) && RecipeData.BlueprintSize.TryGetValue(msg.BlueprintId ?? "", out var bpSize))
        {
            size = new Point2(bpSize.x, bpSize.y);
        }
        Console.WriteLine("[build] occupy {0} type={1} blueprint={2} tile={3},{4} size={5},{6}", entityId, entityType, msg.BlueprintId, msg.Tile.x, msg.Tile.y, size.x, size.y);
        AppearArtifact artifact = MakeArtifact(entityId, entityType, msg.Tile, size, msg.Rotation, msg.Floor, msg.Stories ?? 1, msg.BlueprintId);
        // GP-04: จำไว้ในโลกก่อน แล้วค่อย broadcast — คนที่เข้ามาทีหลังจะได้เห็นด้วย
        _world.AddArtifact(artifact, msg.BlueprintId);
        _world.Broadcast(artifact);
        Send(new Messages.Timer { Duration = 2f }, header.Seq);
        Send(new Occupied
        {
            EntityId = entityId,
            TileX = msg.Tile.x,
            TileY = msg.Tile.y,
            Floor = msg.Floor
        }, header.Seq);
    }

    // GP-07: ตัวสร้างจริงย้ายไป ArtifactFactory (static) เพราะตอนโหลดเซฟกลับมา
    // ServerWorld ต้องสร้าง artifact เองโดยไม่มี ServerPlayer ให้อ้างอิง
    private AppearArtifact MakeArtifact(string entityId, ushort entityType, Point2 tile, Point2 size, Rotation rotation, int? floor, int stories, string blueprintId = null)
    {
        return ArtifactFactory.Make(EntityId, entityId, entityType, tile, size, rotation, floor, stories, blueprintId);
    }

    private void HandlePlaceCapsulatedArtifact(PlaceCapsulatedArtifact msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Building || IsItemLocked(msg.ItemId))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        string proto = null;
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == msg.ItemId);
            if (idx >= 0)
            {
                proto = _inventory[idx].Prototype;
            }
        }
        if (proto == null)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        string blueprintId = proto.StartsWith("capsulated_") ? proto.Substring("capsulated_".Length) : proto;
        if (!RecipeData.BlueprintType.TryGetValue(blueprintId, out ushort entityType))
        {
            Console.WriteLine("[build] place capsule FAILED: unknown blueprint '{0}' from {1}", blueprintId, proto);
            Send(default(Abort), header.Seq);
            return;
        }
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == msg.ItemId && it.Prototype == proto);
            if (idx < 0)
            {
                Send(default(Abort), header.Seq);
                return;
            }
            _inventory.RemoveAt(idx);
            ForgetInventoryItem(msg.ItemId);
        }
        Point2 size = new Point2(1, 1);
        if (RecipeData.BlueprintSize.TryGetValue(blueprintId, out var bpSize))
        {
            size = new Point2(bpSize.x, bpSize.y);
        }
        string entityId = Guid.NewGuid().ToString();
        Console.WriteLine("[build] place capsule {0} (proto={1}) type={2} tile={3},{4}", entityId, proto, entityType, msg.Tile.x, msg.Tile.y);
        // ของที่อยู่ในแคปซูลคือ "ของสำเร็จรูป" — วางแล้วใช้ได้เลย ไม่ต้องเอาวัสดุมาสร้างซ้ำ
        // 🐛 เดิมวางออกมาเป็น Occupied (= แค่จองพื้นที่) ⇒ กองไฟที่วางจากแคปซูลใช้เป็นโต๊ะคราฟต์ไม่ได้
        AppearArtifact placed = ArtifactFactory.Make(EntityId, entityId, entityType, msg.Tile, size,
            msg.Rotation, msg.Floor, 1, blueprintId, BuildingState.Completed);
        // GP-04
        _world.AddArtifact(placed, blueprintId);
        _world.Broadcast(placed);
        MarkDirty();              // GP-07 — ของออกจากกระเป๋าไปแล้ว
        Send(new Messages.Timer { Duration = 2f }, header.Seq);
        SendInventory();
    }

    private void HandleBuildArtifact(BuildArtifact msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Building)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบก่อสร้างปิดอยู่ในรอบนี้ (Features.Building)", Name);
            Send(new Info { Text = "ระบบก่อสร้างยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        // H-6: เดิมไม่ตรวจอะไรเลย ยิงรัว ๆ ได้ไม่จำกัด → _deferred โตไม่หยุด
        // แล้วอีก 2.1 วิ ทุกงานยิง broadcast 2 packet คูณจำนวนผู้เล่น = main loop ค้าง
        // แถมส่ง id ของบ้านคนอื่นมาก็เปลี่ยนสถานะบ้านเขาเป็น Built ได้ฟรี ๆ
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact target))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: ไม่มีสิ่งปลูกสร้าง {1}", Name, msg.EntityId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!CanModifyArtifact(target))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: ไม่ใช่เจ้าของ {1}", Name, msg.EntityId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: มีงานค้างอยู่ {1} รายการแล้ว", Name, _deferred.Count);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!TrySpendStamina(StaminaCostBuild))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        Console.WriteLine("[build] build {0} tile={1},{2}", msg.EntityId, msg.Tile.x, msg.Tile.y);
        Send(new Messages.Timer { Duration = 2f }, header.Seq);
        _deferred.Add((Times.UnixTimeNow() + 2.1, () =>
        {
            // GP-04: อัปเดตสถานะที่เก็บไว้ด้วย ไม่งั้นคนที่เข้ามาทีหลังจะเห็นเป็น Occupied ตลอด
            _world.SetArtifactBuildingState(msg.EntityId, BuildingState.Built);
            _world.Broadcast(new ArtifactBuilt { EntityId = msg.EntityId, BuilderId = EntityId });
            _world.Broadcast(new ArtifactCompleted { EntityId = msg.EntityId });
            GainExpForBuild();
        }));
    }

    private void HandleGetArtifact(GetArtifact msg, PacketHeader header)
    {
        Send(new ArtifactMaterials
        {
            EntityId = msg.EntityId,
            Materials = new Dictionary<string, Item[]>()
        }, header.Seq);
    }

    private void HandleDestructArtifact(DestructArtifact msg, PacketHeader header)
    {
        Console.WriteLine("[build] destruct {0} tile={1},{2}", msg.EntityId, msg.Tile.x, msg.Tile.y);

        // GP-04: เดิม broadcast ทิ้งเลยโดยไม่ตรวจอะไร → ส่ง entityId อะไรมาก็ทุบได้ รวมถึงบ้านคนอื่น
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact artifact))
        {
            Console.WriteLine("[build] destruct ปฏิเสธ: ไม่รู้จัก entity '{0}'", msg.EntityId);
            Send(default(Abort), header.Seq);
            return;
        }
        if (!CanModifyArtifact(artifact))
        {
            Console.WriteLine("[build] destruct ปฏิเสธ: {0} ไม่ใช่เจ้าของ {1}", EntityId, msg.EntityId);
            Send(default(Abort), header.Seq);
            return;
        }

        _world.RemoveArtifact(msg.EntityId);
        _world.Broadcast(new DisappearEntity { EntityId = msg.EntityId });
    }

    /// <summary>ผู้เล่นคนนี้มีสิทธิ์แก้/ทุบสิ่งปลูกสร้างนี้ไหม (เป็นผู้สร้าง หรืออยู่ในรายชื่อสถาปนิก)</summary>
    private bool CanModifyArtifact(AppearArtifact artifact)
    {
        if (artifact.FounderEntityId == EntityId)
        {
            return true;
        }
        string[] architects = artifact.ArchitectEntityIds;
        if (architects != null)
        {
            for (int i = 0; i < architects.Length; i++)
            {
                if (architects[i] == EntityId)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
