using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Player;
using Shared.Economy;

namespace DurangoServer.Core;

/// <summary>
/// Clan system — วงจรชีวิตพื้นฐาน (MakeClan, JoinClan, LeaveClan, InviteToClan, KickClanMember, RenameClan)
///
/// โปรโตคอล:
/// - MakeClan → สร้างแคลน (จ่าย 100 DurangoCoin)
/// - JoinClan → ส่งใบสมัคร
/// - LeaveClan → ออกจากแคลน
/// - InviteToClan → หัวหน้าเชิญ
/// - KickClanMember → หัวหน้าเตะ
/// - RenameClan → เปลี่ยนชื่อ
/// - ApproveClanApplier → หัวหน้ายอมรับ
/// - DropClanApplier → หัวหน้าปฏิเสธ
/// - GetClanFund → empty (gated behind wallet)
/// - DonateToClanFund → reject (gated behind wallet)
///
/// ข้อจำกัด:
/// - 1 คน = 1 แคลนเท่านั้น
/// - Clan registry อยู่ใน WorldSave.Clans
/// - PlayerSave.ClanId / ClanName / ClanRoleId = cache ของ member state
/// - Role: 0=member, 1=officer, 2=leader
/// - Feature gate: ServerConfig.Current.Features.PartyAndClan
/// </summary>
public partial class ServerPlayer
{
    private string _clanId;
    private string _clanName;
    private int _clanRoleId; // 0=member, 1=officer, 2=leader

    private bool RejectClanDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบแคลนยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
        return false;
    }

    private void RegisterClanHandlers()
    {
        _conn.Recv<MakeClan>(HandleMakeClan);
        _conn.Recv<JoinClan>(HandleJoinClan);
        _conn.Recv<LeaveClan>(HandleLeaveClan);
        _conn.Recv<InviteToClan>(HandleInviteToClan);
        _conn.Recv<KickClanMember>(HandleKickClanMember);
        _conn.Recv<RenameClan>(HandleRenameClan);
        _conn.Recv<ApproveClanApplier>(HandleApproveClanApplier);
        _conn.Recv<DropClanApplier>(HandleDropClanApplier);
        _conn.Recv<GetClanFund>(HandleGetClanFund);
        _conn.Recv<DonateToClanFund>(HandleDonateToClanFund);
        _conn.Recv<BreakAlly>(HandleBreakAlly);
        _conn.Recv<SuggestAlly>(HandleSuggestAlly);
        _conn.Recv<GetAllySlots>(HandleGetAllySlots);
        _conn.Recv<GetClanResearch>(HandleGetClanResearch);
        _conn.Recv<StartClanResearch>(HandleStartClanResearch);
        _conn.Recv<GetAvailableClanResearch>(HandleGetAvailableClanResearch);
    }

    // ── Core handlers ─────────────────────────────────────────────────

    private void HandleMakeClan(MakeClan msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId != null)
        {
            Send(new Info { Text = "คุณอยู่ในแคลนอยู่แล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.ClanName) || msg.ClanName.Length > 20)
        {
            Send(new Info { Text = "ชื่อแคลนต้อง 1-20 ตัวอักษร" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // ตรวจ DurangoCoin (ถ้า wallet enabled)
        if (WalletEnabled && !TryDebitWallet(Currency.PcCoin, 100))
        {
            Send(new Info { Text = "ต้องมี DurangoCoin 100 เพื่อสร้างแคลน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        string clanId = Guid.NewGuid().ToString().Substring(0, 8);
        ClanSave clan = new ClanSave
        {
            Id = clanId,
            Name = msg.ClanName,
            LeaderEntityId = EntityId,
            MemberEntityIds = new List<string> { EntityId },
            ApplicantEntityIds = new List<string>()
        };

        _world.AddClan(clan);
        _clanId = clanId;
        _clanName = msg.ClanName;
        _clanRoleId = 2;
        MarkDirty();

        if (WalletEnabled) SendWalletUpdated();
        Send(default(OK), header.Seq);
    }

    private void HandleJoinClan(JoinClan msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId != null)
        {
            Send(new Info { Text = "คุณอยู่ในแคลนอยู่แล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        ClanSave clan = _world.GetClan(msg.ClanId);
        if (clan == null)
        {
            Send(new Info { Text = "ไม่พบแคลนนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (clan.ApplicantEntityIds.Contains(EntityId))
        {
            Send(new Info { Text = "ส่งใบสมัครไปแล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        clan.ApplicantEntityIds.Add(EntityId);
        _world.MarkDirty();
        Send(default(OK), header.Seq);
    }

    private void HandleLeaveClan(LeaveClan msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId == null)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        _world.RemoveFromClan(_clanId, EntityId);
        _clanId = null;
        _clanName = null;
        _clanRoleId = 0;
        MarkDirty();
        Send(default(OK), header.Seq);
    }

    private void HandleInviteToClan(InviteToClan msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId == null || _clanRoleId < 2)
        {
            Send(new Info { Text = "คุณไม่ใช่หัวหน้าแคลน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        ServerPlayer target = _world.FindPlayer(msg.EntityId);
        if (target == null)
        {
            Send(new Info { Text = "ผู้เล่นออฟไลน์" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (target._clanId != null)
        {
            Send(new Info { Text = "ผู้เล่นนี้อยู่ในแคลนอยู่แล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // auto-accept invite (simple model)
        target._clanId = _clanId;
        target._clanName = _clanName;
        target._clanRoleId = 0;
        target.MarkDirty();
        ClanSave clan = _world.GetClan(_clanId);
        if (clan != null && !clan.MemberEntityIds.Contains(msg.EntityId))
        {
            clan.MemberEntityIds.Add(msg.EntityId);
            _world.MarkDirty();
        }
        Send(default(OK), header.Seq);
    }

    private void HandleKickClanMember(KickClanMember msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId == null || _clanRoleId < 2)
        {
            Send(new Info { Text = "คุณไม่ใช่หัวหน้าแคลน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        string targetId = msg.EntityId;
        if (targetId == EntityId)
        {
            Send(new Info { Text = "เตะตัวเองไม่ได้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        _world.RemoveFromClan(_clanId, targetId);
        ServerPlayer target = _world.FindPlayer(targetId);
        if (target != null)
        {
            target._clanId = null;
            target._clanName = null;
            target._clanRoleId = 0;
            target.MarkDirty();
        }
        Send(default(OK), header.Seq);
    }

    private void HandleRenameClan(RenameClan msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId == null || _clanRoleId < 2)
        {
            Send(new Info { Text = "คุณไม่ใช่หัวหน้าแคลน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.ClanName) || msg.ClanName.Length > 20)
        {
            Send(new Info { Text = "ชื่อแคลนต้อง 1-20 ตัวอักษร" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        ClanSave clan = _world.GetClan(_clanId);
        if (clan != null) clan.Name = msg.ClanName;
        _clanName = msg.ClanName;
        _world.MarkDirty();
        MarkDirty();
        Send(default(OK), header.Seq);
    }

    private void HandleApproveClanApplier(ApproveClanApplier msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId == null || _clanRoleId < 2)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        ClanSave clan = _world.GetClan(_clanId);
        if (clan == null) { Send(Aborts.Reason(), header.Seq); return; }

        string targetId = msg.EntityId;
        if (!clan.ApplicantEntityIds.Contains(targetId))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        clan.ApplicantEntityIds.Remove(targetId);
        if (!clan.MemberEntityIds.Contains(targetId)) clan.MemberEntityIds.Add(targetId);
        _world.MarkDirty();

        ServerPlayer target = _world.FindPlayer(targetId);
        if (target != null)
        {
            target._clanId = _clanId;
            target._clanName = _clanName;
            target._clanRoleId = 0;
            target.MarkDirty();
        }
        else
        {
            MutateOfflinePlayer(targetId, s =>
            {
                s.ClanId = _clanId;
                s.ClanName = _clanName;
                s.ClanRoleId = 0;
            });
        }
        Send(default(OK), header.Seq);
    }

    private void HandleDropClanApplier(DropClanApplier msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        if (_clanId == null || _clanRoleId < 2) { Send(Aborts.Reason(), header.Seq); return; }
        ClanSave clan = _world.GetClan(_clanId);
        if (clan != null) clan.ApplicantEntityIds.Remove(msg.EntityId);
        _world.MarkDirty();
        Send(default(OK), header.Seq);
    }

    // ── Gated stubs (wallet/research/ally not implemented yet) ────────

    private void HandleGetClanFund(GetClanFund msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new Costs { _Costs = new Dictionary<Shared.Economy.Currency, long>() }, header.Seq);
    }

    private void HandleDonateToClanFund(DonateToClanFund msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new Info { Text = "ระบบบริจาคคลังแคลนยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
    }

    private void HandleBreakAlly(BreakAlly msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new Info { Text = "ระบบพันธมิตรแคลนยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
    }

    private void HandleSuggestAlly(SuggestAlly msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new Info { Text = "ระบบพันธมิตรแคลนยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
    }

    private void HandleGetAllySlots(GetAllySlots msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new AllySlots { Slots = new AllySlot[0] }, header.Seq);
    }

    private void HandleGetClanResearch(GetClanResearch msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new ClanResearchList { ResearchList = new ClanResearch[0] }, header.Seq);
    }

    private void HandleStartClanResearch(StartClanResearch msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new Info { Text = "ระบบวิจัยแคลนยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
    }

    private void HandleGetAvailableClanResearch(GetAvailableClanResearch msg, PacketHeader header)
    {
        if (!PartyEnabled) { RejectClanDisabled(header); return; }
        Send(new AvailableClanResearch { AvailableResearchIds = Array.Empty<string>() }, header.Seq);
    }

    // ── Persistence ───────────────────────────────────────────────────

    private void ApplyClanSave(PlayerSave save)
    {
        _clanId = save.ClanId;
        _clanName = save.ClanName;
        _clanRoleId = save.ClanRoleId;
    }

    private void FillClanSave(PlayerSave save)
    {
        save.ClanId = _clanId;
        save.ClanName = _clanName;
        save.ClanRoleId = _clanRoleId;
    }
}
