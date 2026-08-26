using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;
using Shared.Accelerator;

namespace DurangoServer.Core;

// ServerPlayer.WarpAccelerator — handler ฝั่งเซิร์ฟของ blueprint "warp_accelerator"
// (ดูรายละเอียด state machine เต็ม ๆ ที่ WarpAcceleratorManager.cs)
//
// เมนู "เร่งวาร์ป/เข้าร่วม/รับรางวัล" มาจาก Touched.Interactions ล้วน ๆ เหมือนระบบอื่นในไฟล์นี้ทั้งหมด
// (ดูบันทึกที่ ServerPlayer.Gathering.cs HandleTouch) ⇒ ฟังก์ชัน AddWarpAcceleratorInteractions ด้านล่าง
// ถูกเรียกจาก HandleTouch โดยตรงตอนเจอ component "WarpAccelerator" ใน blueprint

public partial class ServerPlayer
{
    private static WarpAcceleratorConfig WarpCfg => ServerConfig.Current.WarpAccelerator;

    /// <summary>รหัสเมนู interaction ที่ client รู้จัก (client/InteractionData/Interaction.cs)</summary>
    private const int InteractionAccelerate = 670;
    private const int InteractionReceiveAccelerationRewards = 671;
    private const int InteractionParticipateAcceleration = 672;

    // Warp Matter สะสมของผู้เล่นคนนี้ — ดูหมายเหตุขอบเขต MVP ข้อ 2 ใน WarpAcceleratorManager.cs
    // (ไม่ใช่ Wallet/Currency เต็มระบบ เพราะเซิร์ฟยังไม่มีระบบกระเป๋าเงินจริงเลยสักจุด)
    private int _warpMatterBalance;
    private int _weeklyWarpMatterAcquired;
    private double _weeklyWarpMatterRefreshAt;

    private void RegisterWarpAcceleratorHandlers()
    {
        // GetWarpAcceleratorCost ไม่มีฟิลด์อะไรเลย (แค่ query ราคาปัจจุบัน) — ตอบได้เสมอไม่ต้องเช็คสิทธิ์
        // อะไรเป็นพิเศษ (ไม่ได้แก้ state อะไร) ต่อให้ฟีเจอร์ปิดอยู่ก็ตอบราคาไปได้ตามปกติ
        _conn.Recv<GetWarpAcceleratorCost>((msg, header) =>
        {
            Send(new Cost { Currency = WarpCfg.JoinCostCurrency, Amount = WarpCfg.JoinCostAmount }, header.Seq);
        });
        _conn.Recv<Messages.Accelerate>(HandleAccelerate);
        _conn.Recv<ParticipateAcceleration>(HandleParticipateAcceleration);
        _conn.Recv<ReceiveAcceleratorRewards>(HandleReceiveAcceleratorRewards);
    }

    /// <summary>
    /// เช็คสิทธิ์ร่วมของทุกคำสั่งในระบบนี้ — คืน false = ปฏิเสธ (log แล้ว)
    ///
    /// ⚠️ ไม่มี Abort/Info มาตรฐานเหมือน CheckFarmAccess เพราะ client ฝั่ง Accelerate/
    /// ParticipateAcceleration/ReceiveAcceleratorRewards ยิง Send() แบบไม่รอ reply เลย (ไม่มี .On<T>
    /// ต่อท้ายสักตัว — ดู ArtifactInteractions.cs) ผู้เรียกจึงต้องส่ง Info เองถ้าอยากแจ้งผู้เล่นว่าทำไมถึงไม่ผ่าน
    /// </summary>
    private bool CheckWarpAcceleratorAccess(string entityId, out AppearArtifact artifact)
    {
        artifact = default;
        if (!ServerConfig.Current.Features.WarpAccelerator)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: วาร์ปเรกเซเลอเรเตอร์ปิดอยู่ในรอบนี้ (Features.WarpAccelerator)", Name);
            return false;
        }
        if (Dead)
        {
            return false;
        }
        if (!_world.TryGetArtifact(entityId, out artifact))
        {
            Console.WriteLine("[warp-accel] ปฏิเสธ {0}: ไม่มีสิ่งปลูกสร้าง {1}", Name, entityId);
            return false;
        }
        if (!_world.TryGetArtifactBlueprint(entityId, out string bp) || bp != "warp_accelerator")
        {
            Console.WriteLine("[warp-accel] ปฏิเสธ {0}: {1} ไม่ใช่ warp_accelerator", Name, entityId);
            return false;
        }
        if (!IsWithinReach(artifact.Tile))
        {
            Console.WriteLine("[warp-accel] ปฏิเสธ {0}: {1} อยู่ไกลเกินเอื้อม", Name, entityId);
            return false;
        }
        return true;
    }

    private void HandleAccelerate(Messages.Accelerate msg, PacketHeader header)
    {
        if (!CheckWarpAcceleratorAccess(msg.EntityId, out _))
        {
            return;
        }
        if (!_world.WarpAccelerators.TryAccelerate(msg.EntityId, EntityId, out string reason))
        {
            Send(new Info { Text = reason });
            return;
        }
        Send(new Info { Text = $"เริ่มกิจกรรมวาร์ปเรกเซเลอเรเตอร์แล้ว — รอ {WarpCfg.WaitSeconds:F0} วิก่อนคลื่นแรก" });
    }

    private void HandleParticipateAcceleration(ParticipateAcceleration msg, PacketHeader header)
    {
        if (!CheckWarpAcceleratorAccess(msg.EntityId, out _))
        {
            return;
        }
        if (!_world.WarpAccelerators.TryParticipate(msg.EntityId, EntityId, out string reason))
        {
            Send(new Info { Text = reason });
            return;
        }
        Send(new Info { Text = "เข้าร่วมกิจกรรมวาร์ปเรกเซเลอเรเตอร์แล้ว" });
    }

    private void HandleReceiveAcceleratorRewards(ReceiveAcceleratorRewards msg, PacketHeader header)
    {
        if (!CheckWarpAcceleratorAccess(msg.EntityId, out _))
        {
            return;
        }
        if (!_world.WarpAccelerators.TryReceiveRewards(msg.EntityId, EntityId, out int granted, out string reason))
        {
            Send(new Info { Text = reason });
            return;
        }
        GrantWarpMatter(granted);
    }

    /// <summary>
    /// ให้ Warp Matter ผู้เล่นคนนี้ พร้อมเพดานรายสัปดาห์ (WarpAcceleratorConfig.WeeklyWarpMatterCap)
    /// รีเซ็ตตัวนับรายสัปดาห์เองถ้าเลยเวลา RefreshAt ไปแล้ว (lazy refresh — ไม่ต้องมี timer แยก)
    /// </summary>
    private void GrantWarpMatter(int amount)
    {
        if (amount <= 0)
        {
            return;
        }
        double now = Durango.Utils.Times.UnixTimeNow();
        if (_weeklyWarpMatterRefreshAt <= 0 || now >= _weeklyWarpMatterRefreshAt)
        {
            _weeklyWarpMatterAcquired = 0;
            _weeklyWarpMatterRefreshAt = now + 7.0 * 24.0 * 3600.0;
        }
        int cap = Math.Max(0, WarpCfg.WeeklyWarpMatterCap);
        int room = Math.Max(0, cap - _weeklyWarpMatterAcquired);
        int actual = Math.Min(amount, room);

        _warpMatterBalance += actual;
        _weeklyWarpMatterAcquired += actual;
        _dirty = true;

        // ให้ client อัปเดต UI "ได้รับกี่หน่วยแล้วในสัปดาห์นี้ / เพดานเท่าไร" (WarpAcceleratorSystem.GetWarpMatterAcquisition)
        Send(new WarpAcceleratorAcquisition
        {
            WarpMatter = new LimitedAcquisition
            {
                Acquired = new Pair<int, int>(_weeklyWarpMatterAcquired, cap),
                RefreshAt = _weeklyWarpMatterRefreshAt
            }
        });

        string text = actual < amount
            ? $"ได้รับ Warp Matter {actual} หน่วย (เกินโควตารายสัปดาห์ไป {amount - actual} หน่วย) — ยอดสะสม {_warpMatterBalance}"
            : $"ได้รับ Warp Matter {actual} หน่วย — ยอดสะสม {_warpMatterBalance}";
        Send(new Info { Text = text });
        Console.WriteLine("[warp-accel] {0} ได้รับ Warp Matter {1} (ยอดรวม {2}, สัปดาห์นี้ {3}/{4})",
            Name, actual, _warpMatterBalance, _weeklyWarpMatterAcquired, cap);
    }

    /// <summary>
    /// เติมเมนูของ warp_accelerator ลงใน Touched — เรียกจาก ServerPlayer.Gathering.cs HandleTouch
    /// ตอนเจอ component "WarpAccelerator" ใน RecipeData.BlueprintComponents
    ///
    /// 🐛 บั๊กเดิม (ก่อนแก้ 22 ส.ค. 2026): switch ในนั้นไม่มี case "WarpAccelerator" เลย ⇒ ไม่มี
    /// interaction ไหนถูกเติมให้เลยนอกจาก 103 (เมนูฐาน) — client เลยตกไปทางเมนู fallback "ขุดหิน"
    /// ทั่วไปแทนที่จะเป็นเมนูของกิจกรรมนี้จริง ๆ
    /// </summary>
    private void AddWarpAcceleratorInteractions(string entityId, List<int> interactions)
    {
        if (!ServerConfig.Current.Features.WarpAccelerator)
        {
            // ฟีเจอร์ปิดอยู่ — ไม่เติมอะไรเลย (เมนูจะยังเป็นแบบเดิมก่อนแก้ จนกว่าจะเปิด Features.WarpAccelerator)
            return;
        }
        AcceleratorStatus status = _world.WarpAccelerators.GetStatus(entityId);
        switch (status)
        {
            case AcceleratorStatus.RiftInactivated:
                interactions.Add(InteractionAccelerate);
                break;
            case AcceleratorStatus.End:
                if (_world.WarpAccelerators.HasUnclaimedReward(entityId, EntityId))
                {
                    interactions.Add(InteractionReceiveAccelerationRewards);
                }
                break;
            default: // RiftActivated / Waiting / Processing / Intermission — กิจกรรมกำลังไปอยู่
                if (!_world.WarpAccelerators.IsParticipant(entityId, EntityId))
                {
                    interactions.Add(InteractionParticipateAcceleration);
                }
                break;
        }
    }
}
