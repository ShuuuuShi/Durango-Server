using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Party system — ปาร์ตี้ผู้เล่นสูงสุด 4 คน
///
/// การทำงานตาม client protocol (client/Durango.Logic/PartySystem.cs):
/// - MakeParty: สร้าง party ตัวเองเป็น leader (accepted ทันที)
/// - InviteIntoParty: leader เชิญ → ผู้ถูกเชิญเข้าเป็นสมาชิกแบบ **pending** (isAccepted=false)
///   แล้ว broadcast Party ให้ทุกคน — client ของผู้ถูกเชิญเห็นตัวเอง pending
///   → เด้ง UI ยืนยัน (event Invited)
/// - JoinIntoParty: ผู้ถูกเชิญกดยอมรับ → isAccepted=true
/// - RejectPartyInvitation: ผู้ถูกเชิญปฏิเสธ (InviteeEntityId=null)
///   หรือ leader ยกเลิกคำเชิญ (InviteeEntityId=เป้าหมาย)
/// - LeaveParty / KickPartyMember / ElectPartyLeader: ออก/เตะ/เลือกหัวหน้าใหม่
///
/// bool ใน Pair&lt;PartierStatus, bool&gt; ของ PartyInfo คือ isAccepted ของสมาชิก
/// (client ใช้แยก "รอยืนยัน" กับ "อยู่ปาร์ตี้แล้ว")
///
/// ข้อจำกัด:
/// - Leader ออก/หลุด = ถ่ายโอนให้สมาชิกที่ accepted คนแรก (ไม่มีเลย = คนแรก)
/// - ผู้เล่น 1 คนอยู่ได้คนเดียว party เดียว
/// - ออกจากเกม = ออกจาก party (party เป็นของชั่วคราว ไม่ค้างข้าม session)
/// - Feature gate: ServerConfig.Current.Features.PartyAndClan
/// </summary>
public partial class ServerPlayer
{
    private string _partyId;
    private bool _partyLeader;

    /// <summary>ยอมรับคำเชิญปาร์ตี้แล้วหรือยัง — false = รอยืนยัน (pending)</summary>
    private bool _partyAccepted;

    private const int MaxPartySize = 4;

    private static bool PartyEnabled => ServerConfig.Current.Features.PartyAndClan;

    /// <summary>
    /// ปฏิเสธแบบสม่ำเสมอทุก entry point เมื่อ feature ปิด — ต้องมี Info พร้อมเหตุผลเสมอ
    /// ไม่ใช่ Abort เปล่า ๆ (client จะได้เห็นข้อความแทนที่จะค้างเฉย)
    /// </summary>
    private bool RejectPartyDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบปาร์ตี้ยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void RegisterPartyHandlers()
    {
        _conn.Recv<MakeParty>(HandleMakeParty);
        _conn.Recv<GetParty>(HandleGetParty);
        _conn.Recv<InviteIntoParty>(HandleInviteIntoParty);
        _conn.Recv<JoinIntoParty>(HandleJoinIntoParty);
        _conn.Recv<RejectPartyInvitation>(HandleRejectPartyInvitation);
        _conn.Recv<LeaveParty>(HandleLeaveParty);
        _conn.Recv<KickPartyMember>(HandleKickPartyMember);
        _conn.Recv<ElectPartyLeader>(HandleElectPartyLeader);
    }

    // ── Handlers ──────────────────────────────────────────────────────

    private void HandleMakeParty(MakeParty msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        if (_partyId != null)
        {
            Send(new Info { Text = "คุณอยู่ในปาร์ตี้อยู่แล้ว" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        string partyId = "party_" + Guid.NewGuid().ToString("N").Substring(0, 12);
        _partyId = partyId;
        _partyLeader = true;
        _partyAccepted = true;
        _world.CreateParty(partyId, this);
        MarkDirty();
        Console.WriteLine("[party] {0} สร้างปาร์ตี้ {1}", Name, partyId);
        SendPartyInfo(header.Seq);
    }

    private void HandleGetParty(GetParty msg, PacketHeader header)
    {
        // ปิดระบบ = ตอบว่าไม่มี party (client เคลียร์ state ไปเลย ไม่ค้างรอ)
        SendPartyInfo(header.Seq);
    }

    private void HandleInviteIntoParty(InviteIntoParty msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        if (string.IsNullOrEmpty(msg.InviteeEntityId))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (msg.InviteeEntityId == EntityId)
        {
            Send(new Info { Text = "เชิญตัวเองไม่ได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (_partyId != null && !_partyLeader)
        {
            Send(new Info { Text = "เฉพาะหัวหน้าปาร์ตี้เท่านั้นที่เชิญได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        ServerPlayer invitee = _world.FindPlayer(msg.InviteeEntityId);
        if (invitee == null)
        {
            Send(new Info { Text = "ไม่พบผู้เล่นนี้ออนไลน์อยู่" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (invitee._partyId != null)
        {
            Send(new Info { Text = invitee._partyId == _partyId
                ? "ผู้เล่นนี้อยู่ในปาร์ตี้นี้อยู่แล้ว"
                : "ผู้เล่นนี้อยู่ในปาร์ตี้อื่นอยู่แล้ว" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        // client อนุญาตให้เชิญตอนยังไม่มี party (CanInvite: IsLeader || NotInParty)
        // ⇒ ตรวจทุกอย่างผ่านแล้วค่อยสร้าง party ใหม่ — ไม่งั้น invite ล้ม
        //   แต่ inviter ติดค้างอยู่ใน party เปล่าที่สร้างไปก่อน
        if (_partyId == null)
        {
            string newPartyId = "party_" + Guid.NewGuid().ToString("N").Substring(0, 12);
            _partyId = newPartyId;
            _partyLeader = true;
            _partyAccepted = true;
            _world.CreateParty(newPartyId, this);
            MarkDirty();
            Console.WriteLine("[party] {0} สร้างปาร์ตี้ {1} (อัตโนมัติจากการเชิญ)", Name, newPartyId);
        }
        // เพิ่มเป็นสมาชิก pending — ต้องลงทะเบียนใน world list ด้วย
        // ไม่งั้น GetPartyMembers/BuildPartyInfo ไม่เห็นเขาและตอนออกก็ลบไม่ถูก
        if (!_world.TryAddToParty(_partyId, invitee, MaxPartySize))
        {
            Send(new Info { Text = "ปาร์ตี้เต็มแล้ว (สูงสุด 4 คน)" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        invitee._partyId = _partyId;
        invitee._partyLeader = false;
        invitee._partyAccepted = false;   // รอยืนยัน — client จะเด้ง UI ยอมรับ/ปฏิเสธ
        invitee.MarkDirty();
        Console.WriteLine("[party] {0} เชิญ {1} เข้าปาร์ตี้ {2}", Name, invitee.Name, _partyId);
        // broadcast ให้ทุกคนใน party — client ของ invitee เห็นตัวเองเป็น pending
        BroadcastPartyInfo();
    }

    private void HandleJoinIntoParty(JoinIntoParty msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        // กดยอมรับคำเชิญ — ต้องเป็นสมาชิก pending ของ party ใด party หนึ่ง
        if (_partyId == null || _partyAccepted)
        {
            SendPartyInfo(header.Seq);
            return;
        }
        _partyAccepted = true;
        MarkDirty();
        Console.WriteLine("[party] {0} ยอมรับคำเชิญเข้าปาร์ตี้ {1}", Name, _partyId);
        BroadcastPartyInfo();
    }

    private void HandleRejectPartyInvitation(RejectPartyInvitation msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        if (string.IsNullOrEmpty(msg.InviteeEntityId))
        {
            // ผู้ส่งปฏิเสธคำเชิญของตัวเอง — ต้องเป็นสมาชิก pending เท่านั้น
            if (_partyId == null || _partyAccepted || _partyLeader)
            {
                Send(default(Abort), header.Seq);
                return;
            }
            string partyId = _partyId;
            Console.WriteLine("[party] {0} ปฏิเสธคำเชิญปาร์ตี้ {1}", Name, partyId);
            RemoveFromParty(this);
            SendPartyInfo(header.Seq);
            BroadcastPartyInfoTo(partyId);
            return;
        }
        // leader ยกเลิกคำเชิญของ invitee คนนั้น
        if (_partyId == null || !_partyLeader)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        ServerPlayer invitee = _world.FindPlayer(msg.InviteeEntityId);
        if (invitee == null || invitee._partyId != _partyId || invitee._partyAccepted)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        string pid = _partyId;
        Console.WriteLine("[party] {0} ยกเลิกคำเชิญของ {1}", Name, invitee.Name);
        RemoveFromParty(invitee);
        invitee.SendPartyInfo();
        BroadcastPartyInfoTo(pid);
    }

    private void HandleLeaveParty(LeaveParty msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        if (_partyId == null)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        string oldPartyId = _partyId;
        bool wasLeader = _partyLeader;   // ต้องจำไว้ก่อน RemoveFromParty เคลียร์ค่า
        Console.WriteLine("[party] {0} ออกจากปาร์ตี้ {1}", Name, oldPartyId);
        RemoveFromParty(this);
        // leader ออก = ถ่ายโอนต่อให้คนที่เหลือ
        if (wasLeader)
        {
            PromoteNextLeader(oldPartyId);
        }
        SendPartyInfo(header.Seq);
        BroadcastPartyInfoTo(oldPartyId);
    }

    private void HandleKickPartyMember(KickPartyMember msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        if (_partyId == null || !_partyLeader)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.MemberEntityId))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        ServerPlayer member = _world.FindPlayer(msg.MemberEntityId);
        if (member == null || member._partyId != _partyId)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (member == this)
        {
            Send(new Info { Text = "หัวหน้าไม่สามารถเตะตัวเองได้" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        string partyId = _partyId;
        Console.WriteLine("[party] {0} เตะ {1} จากปาร์ตี้ {2}", Name, member.Name, partyId);
        // ใช้ helper เพื่อลบออกจาก world list ด้วย — ไม่ใช่แค่เคลียร์ field
        RemoveFromParty(member);
        member.SendPartyInfo();
        BroadcastPartyInfoTo(partyId);
    }

    private void HandleElectPartyLeader(ElectPartyLeader msg, PacketHeader header)
    {
        if (!PartyEnabled)
        {
            RejectPartyDisabled(header);
            return;
        }
        if (_partyId == null || !_partyLeader)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        if (string.IsNullOrEmpty(msg.MemberEntityId))
        {
            Send(default(Abort), header.Seq);
            return;
        }
        ServerPlayer newLeader = _world.FindPlayer(msg.MemberEntityId);
        if (newLeader == null || newLeader._partyId != _partyId || !newLeader._partyAccepted)
        {
            Send(new Info { Text = "เลือกได้เฉพาะสมาชิกที่ยอมรับเข้าปาร์ตี้แล้ว" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }
        if (newLeader == this)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        Console.WriteLine("[party] {0} ถ่ายโอนหัวหน้าให้ {1} ในปาร์ตี้ {2}", Name, newLeader.Name, _partyId);
        _partyLeader = false;
        newLeader._partyLeader = true;
        MarkDirty();
        newLeader.MarkDirty();
        BroadcastPartyInfo();
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private void SendPartyInfo(uint replyOf = 0)
    {
        // ปิดระบบ = ตอบว่าไม่มี party เสมอ (client เคลียร์ state ไปเลย ไม่ค้างรอ)
        if (_partyId == null || !PartyEnabled)
        {
            Send(new Party { Id = null, Info = null }, replyOf);
            return;
        }
        PartyInfo info = BuildPartyInfo();
        Send(new Party { Id = _partyId, Info = info }, replyOf);
    }

    private PartyInfo BuildPartyInfo()
    {
        var members = _world.GetPartyMembers(_partyId);
        ServerPlayer leader = _world.GetPartyLeader(_partyId) ?? (members.Count > 0 ? members[0] : this);

        var leaderStatus = MakePartierStatus(leader);
        var memberStatusList = new List<Pair<PartierStatus, bool>>();
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i] == leader)
            {
                continue;
            }
            // bool ตัวที่สอง = isAccepted — client ใช้แยก "รอยืนยัน" กับ "อยู่แล้ว"
            // และใช้เด้ง UI คำเชิญเมื่อเห็นตัวเองเป็น pending
            memberStatusList.Add(new Pair<PartierStatus, bool>(
                MakePartierStatus(members[i]),
                members[i]._partyAccepted));
        }

        return new PartyInfo
        {
            // client ใช้ LeaderRadioId.Name เป็น "ชื่อ" ของหัวหน้า (แสดงใน HUD)
            // จึงต้องใส่ชื่อตัวละคร ไม่ใช่ entity id
            LeaderRadioId = new RadioId { Name = leader.Name, Freq = 0 },
            LeaderStatus = leaderStatus,
            MemberStatus = memberStatusList.ToArray()
        };
    }

    private PartierStatus MakePartierStatus(ServerPlayer p)
    {
        double now = Times.UnixTimeNow();
        p.EnsureSurvival();
        return new PartierStatus
        {
            EntityId = p.EntityId,
            RegionId = IslandRegistry.Current?.Id ?? "",
            Tile = new Point2((int)(p.CurrentPosition.x / 200f), (int)(p.CurrentPosition.y / 200f)),
            Health = new UnityEngine.Vector2(p.ComputedLifeMax, p.CurrentLife),
            Energy = new UnityEngine.Vector2(p.ComputedStaminaMax, p._stamina.ValueAt(now)),
            Level = p.Level,
            IsOnline = true,
            ExpiresAt = 0
        };
    }

    private void BroadcastPartyInfo()
    {
        if (_partyId == null) return;
        BroadcastPartyInfoTo(_partyId);
    }

    private void BroadcastPartyInfoTo(string partyId)
    {
        if (partyId == null) return;
        var members = _world.GetPartyMembers(partyId);
        for (int i = 0; i < members.Count; i++)
        {
            members[i].SendPartyInfo();
        }
    }

    /// <summary>ถ่ายโอนตำแหน่งหัวหน้าให้สมาชิกที่ยอมรับแล้วคนแรก (ไม่มี = คนแรกสุด)</summary>
    private void PromoteNextLeader(string partyId)
    {
        var members = _world.GetPartyMembers(partyId);
        if (members.Count == 0) return;
        // เคลียร์ธง leader เดิมที่อาจค้างอยู่ก่อน
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i]._partyLeader)
            {
                members[i]._partyLeader = false;
                members[i].MarkDirty();
            }
        }
        ServerPlayer next = members.FirstOrDefault(m => m._partyAccepted) ?? members[0];
        next._partyLeader = true;
        next._partyAccepted = true;   // หัวหน้าต้องเป็นสมาชิกที่ยอมรับแล้วเสมอ
        next.MarkDirty();
        Console.WriteLine("[party] ถ่ายโอนหัวหน้าให้ {0}", next.Name);
    }

    /// <summary>
    /// ลบผู้เล่นออกจาก party ให้ครบทั้งสองที่:
    /// 1) world party list (ไม่งั้น GetPartyMembers/Broadcast ยังเห็นเขา)
    /// 2) field ของผู้เล่น (_partyId/_partyLeader/_partyAccepted)
    /// </summary>
    private void RemoveFromParty(ServerPlayer player)
    {
        if (player._partyId == null) return;
        _world.RemoveFromParty(player);
        player._partyId = null;
        player._partyLeader = false;
        player._partyAccepted = false;
        player.MarkDirty();
    }

    /// <summary>เรียกตอน disconnect — ออกจาก party ถ้ายังอยู่ (เรียกก่อน Save จึงไม่เซฟสถานะ party)</summary>
    internal void LeavePartyOnDisconnect()
    {
        if (_partyId == null) return;
        string partyId = _partyId;
        bool wasLeader = _partyLeader;   // จำก่อน RemoveFromParty เคลียร์ค่า
        RemoveFromParty(this);
        if (wasLeader)
        {
            PromoteNextLeader(partyId);
        }
        BroadcastPartyInfoTo(partyId);
    }

    // ── Persistence ───────────────────────────────────────────────────
    // party เป็นของชั่วคราว — LeavePartyOnDisconnect เคลียร์ก่อน Save เสมอ
    // PartyId ในเซฟจึงมีค่าก็ต่อเมื่อ server crash ก่อนได้เขียนเซฟ (crash recovery)

    private void FillPartySave(PlayerSave save)
    {
        save.PartyId = _partyId;
        save.PartyLeader = _partyLeader;
    }

    private void ApplyPartySave(PlayerSave save)
    {
        _partyId = save.PartyId;
        _partyLeader = save.PartyLeader;
        _partyAccepted = save.PartyId != null;
        if (_partyId != null)
        {
            // crash recovery — กลับเข้า party เดิมถ้ายังมีคนอยู่
            _world.TryAddToParty(_partyId, this, MaxPartySize);
            // กลับมาคนเดียวใน party ที่ไม่มีใครเป็นหัวหน้า ⇒ เป็นหัวหน้าเอง
            // ไม่งั้น party ค้างไร้หัวหน้า ไม่มีใครเชิญ/เตะ/ถ่ายโอนต่อได้
            if (!_partyLeader && _world.GetPartyLeader(_partyId) == null
                && _world.GetPartyMemberCount(_partyId) <= 1)
            {
                _partyLeader = true;
                _partyAccepted = true;
                MarkDirty();
            }
        }
    }
}
