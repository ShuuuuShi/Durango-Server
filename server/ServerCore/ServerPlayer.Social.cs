using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Offline;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Player;
using ProtocolFriendType = Messages.FriendType;

namespace DurangoServer.Core;

/// <summary>
/// Friends system — ส่ง/ตอบรับ/ยกเลิกคำขอ จัดประเภทเพื่อน ติดตาม
///
/// โปรโตคอลจาก client/Durango.Logic/SocialSystem.cs:
/// - GetSocial → ส่ง Social snapshot (friends, requests, blocked, following)
/// - RequestFriend → ส่งคำขอไปหา B (B ต้อง online)
///   → B ได้ FriendRequest (push) + FriendRequested (push)
/// - AcceptFriendRequest → B ยอมรับ → ทั้งคู่ได้ Social update
/// - RefuseFriendRequest → B ปฏิเสธ
/// - CancelFriendRequest → A ยกเลิกคำขอที่ส่งไป
/// - AcceptAllFriendRequests / RefuseAllFriendRequests → ทำทีเดียว
/// - RemoveFriend → ลบเพื่อน
/// - SetFriendType → เปลี่ยน JustFriend ↔ BestFriend
/// - GetMyFriendType → ถาม friend type ของตัวเอง
/// - Follow / Unfollow → ติดตาม/เลิกติดตาม (FavoriteRegionOwners)
///
/// ข้อจำกัด:
/// - 1 คน = สูงสุด 200 friends, 30 pending requests
/// - Blocked player ไม่สามารถส่งคำขอได้
/// - Offline mutation: โหลด/แก้/เซฟ PlayerSave ของผู้รับ
/// - Feature gate: ServerConfig.Current.Features.Friends
/// </summary>
public partial class ServerPlayer
{
    private List<string> _blockedEntityIds = new List<string>();
    private List<string> _friends;
    private List<string> _receivedFriendRequests;
    private List<string> _sentFriendRequests;
    private List<string> _followingEntityIds;

    private static bool FriendsEnabled => ServerConfig.Current.Features.Friends;

    private const int MaxFriends = 200;
    private const int MaxPendingRequests = 30;

    private bool RejectFriendsDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบเพื่อนยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
        return false;
    }

    private void RegisterSocialHandlers()
    {
        _conn.Recv<GetSocial>(HandleGetSocial);
        _conn.Recv<RequestFriend>(HandleRequestFriend);
        _conn.Recv<AcceptFriendRequest>(HandleAcceptFriendRequest);
        _conn.Recv<RefuseFriendRequest>(HandleRefuseFriendRequest);
        _conn.Recv<CancelFriendRequest>(HandleCancelFriendRequest);
        _conn.Recv<AcceptAllFriendRequests>(HandleAcceptAllFriendRequests);
        _conn.Recv<RefuseAllFriendRequests>(HandleRefuseAllFriendRequests);
        _conn.Recv<RemoveFriend>(HandleRemoveFriend);
        _conn.Recv<SetFriendType>(HandleSetFriendType);
        _conn.Recv<GetMyFriendType>(HandleGetMyFriendType);
        _conn.Recv<Follow>(HandleFollow);
        _conn.Recv<Unfollow>(HandleUnfollow);
    }

    // ── Handlers ──────────────────────────────────────────────────────

    private void HandleGetSocial(GetSocial msg, PacketHeader header)
    {
        Send(BuildSocialSnapshot(), header.Seq);
    }

    private void HandleRequestFriend(RequestFriend msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string targetId = msg.EntityId;
        if (string.IsNullOrEmpty(targetId) || targetId == EntityId)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (_friends.Contains(targetId))
        {
            Send(new Info { Text = "เป็นเพื่อนกันอยู่แล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (_sentFriendRequests.Contains(targetId))
        {
            Send(new Info { Text = "ส่งคำขอไปแล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (_sentFriendRequests.Count >= MaxPendingRequests)
        {
            Send(new Info { Text = "ส่งคำขอได้สูงสุด 30 รายการ" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // ตรวจ blocked
        ServerPlayer target = _world.FindPlayer(targetId);
        if (target != null && target._blockedEntityIds.Contains(EntityId))
        {
            Send(new Info { Text = "ผู้เล่นนี้บล็อกคุณอยู่" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // ตรวจ offline counterpart blocked
        if (target == null && IsBlockedOffline(targetId))
        {
            Send(new Info { Text = "ผู้เล่นนี้บล็อกคุณอยู่" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        _sentFriendRequests.Add(targetId);
        MarkDirty();

        if (target != null)
        {
            // online: push FriendRequested + FriendRequest
            target._receivedFriendRequests.Add(EntityId);
            target.MarkDirty();
            target.Send(new FriendRequested { EntityId = EntityId });
        }
        else
        {
            // offline: mutate save
            MutateOfflinePlayer(targetId, s =>
            {
                if (s.ReceivedFriendRequests == null) s.ReceivedFriendRequests = new List<string>();
                if (!s.ReceivedFriendRequests.Contains(EntityId))
                    s.ReceivedFriendRequests.Add(EntityId);
            });
        }
        Send(default(OK), header.Seq);
    }

    private void HandleAcceptFriendRequest(AcceptFriendRequest msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string fromId = msg.EntityId;
        if (string.IsNullOrEmpty(fromId) || !_receivedFriendRequests.Contains(fromId))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (_friends.Count >= MaxFriends)
        {
            Send(new Info { Text = "รายชื่อเพื่อนเต็มแล้ว (สูงสุด 200)" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        _receivedFriendRequests.Remove(fromId);
        _friends.Add(fromId);
        MarkDirty();

        ServerPlayer from = _world.FindPlayer(fromId);
        if (from != null)
        {
            from._sentFriendRequests.Remove(EntityId);
            if (from._friends.Count < MaxFriends)
            {
                from._friends.Add(EntityId);
            }
            from.MarkDirty();
            from.Send(new FriendRequestAccepted { EntityId = EntityId });
            from.Send(from.BuildSocialSnapshot());
        }
        else
        {
            MutateOfflinePlayer(fromId, s =>
            {
                if (s.SentFriendRequests != null) s.SentFriendRequests.Remove(EntityId);
                if (s.Friends == null) s.Friends = new List<string>();
                if (!s.Friends.Contains(EntityId)) s.Friends.Add(EntityId);
            });
        }
        Send(default(OK), header.Seq);
        Send(BuildSocialSnapshot());
    }

    private void HandleRefuseFriendRequest(RefuseFriendRequest msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string fromId = msg.EntityId;
        if (!string.IsNullOrEmpty(fromId))
        {
            _receivedFriendRequests.Remove(fromId);
            MarkDirty();
            ServerPlayer from = _world.FindPlayer(fromId);
            if (from != null)
            {
                from._sentFriendRequests.Remove(EntityId);
                from.MarkDirty();
            }
            else
            {
                MutateOfflinePlayer(fromId, s =>
                {
                    if (s.SentFriendRequests != null) s.SentFriendRequests.Remove(EntityId);
                });
            }
        }
        Send(default(OK), header.Seq);
    }

    private void HandleCancelFriendRequest(CancelFriendRequest msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string targetId = msg.EntityId;
        if (!string.IsNullOrEmpty(targetId))
        {
            _sentFriendRequests.Remove(targetId);
            MarkDirty();
            ServerPlayer target = _world.FindPlayer(targetId);
            if (target != null)
            {
                target._receivedFriendRequests.Remove(EntityId);
                target.MarkDirty();
            }
            else
            {
                MutateOfflinePlayer(targetId, s =>
                {
                    if (s.ReceivedFriendRequests != null) s.ReceivedFriendRequests.Remove(EntityId);
                });
            }
        }
        Send(default(OK), header.Seq);
    }

    private void HandleAcceptAllFriendRequests(AcceptAllFriendRequests msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        while (_receivedFriendRequests.Count > 0 && _friends.Count < MaxFriends)
        {
            string fromId = _receivedFriendRequests[0];
            _receivedFriendRequests.RemoveAt(0);
            _friends.Add(fromId);
            ServerPlayer from = _world.FindPlayer(fromId);
            if (from != null)
            {
                from._sentFriendRequests.Remove(EntityId);
                if (from._friends.Count < MaxFriends) from._friends.Add(EntityId);
                from.MarkDirty();
                from.Send(new FriendRequestAccepted { EntityId = EntityId });
                from.Send(from.BuildSocialSnapshot());
            }
            else
            {
                MutateOfflinePlayer(fromId, s =>
                {
                    if (s.SentFriendRequests != null) s.SentFriendRequests.Remove(EntityId);
                    if (s.Friends == null) s.Friends = new List<string>();
                    if (!s.Friends.Contains(EntityId)) s.Friends.Add(EntityId);
                });
            }
        }
        MarkDirty();
        Send(default(OK), header.Seq);
        Send(BuildSocialSnapshot());
    }

    private void HandleRefuseAllFriendRequests(RefuseAllFriendRequests msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        foreach (string fromId in _receivedFriendRequests)
        {
            ServerPlayer from = _world.FindPlayer(fromId);
            if (from != null)
            {
                from._sentFriendRequests.Remove(EntityId);
                from.MarkDirty();
            }
            else
            {
                MutateOfflinePlayer(fromId, s =>
                {
                    if (s.SentFriendRequests != null) s.SentFriendRequests.Remove(EntityId);
                });
            }
        }
        _receivedFriendRequests.Clear();
        MarkDirty();
        Send(default(OK), header.Seq);
        Send(BuildSocialSnapshot());
    }

    private void HandleRemoveFriend(RemoveFriend msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string targetId = msg.EntityId;
        if (string.IsNullOrEmpty(targetId))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        _friends.Remove(targetId);
        MarkDirty();
        ServerPlayer target = _world.FindPlayer(targetId);
        if (target != null)
        {
            target._friends.Remove(EntityId);
            target.MarkDirty();
            target.Send(target.BuildSocialSnapshot());
        }
        else
        {
            MutateOfflinePlayer(targetId, s =>
            {
                if (s.Friends != null) s.Friends.Remove(EntityId);
            });
        }
        Send(default(OK), header.Seq);
        Send(BuildSocialSnapshot());
    }

    private void HandleSetFriendType(SetFriendType msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        // SetFriendType ไม่ได้ส่ง type กลับ — client เรียก GetMyFriendType แทน
        Send(default(OK), header.Seq);
    }

    private void HandleGetMyFriendType(GetMyFriendType msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        Send(new ProtocolFriendType { _FriendType = Shared.Player.FriendType.JustFriend }, header.Seq);
    }

    private void HandleFollow(Follow msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string targetId = msg.EntityId;
        if (!string.IsNullOrEmpty(targetId) && !_followingEntityIds.Contains(targetId))
        {
            _followingEntityIds.Add(targetId);
            MarkDirty();
        }
        Send(default(OK), header.Seq);
    }

    private void HandleUnfollow(Unfollow msg, PacketHeader header)
    {
        if (!FriendsEnabled) { RejectFriendsDisabled(header); return; }
        string targetId = msg.EntityId;
        if (!string.IsNullOrEmpty(targetId))
        {
            _followingEntityIds.Remove(targetId);
            MarkDirty();
        }
        Send(default(OK), header.Seq);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private Social BuildSocialSnapshot()
    {
        return new Social
        {
            FriendEntities = new Dictionary<string, Shared.Player.FriendType>(
                _friends.Select(id =>
                {
                    // ทุกคนเป็น JustFriend — ไม่มี BestFriend mechanic ใน server
                    return new KeyValuePair<string, Shared.Player.FriendType>(id, Shared.Player.FriendType.JustFriend);
                }),
                StringComparer.OrdinalIgnoreCase),
            ReceivedFriendRequests = _receivedFriendRequests.ToArray(),
            SentFriendRequests = _sentFriendRequests.ToArray(),
            FollowingEntityIds = _followingEntityIds.ToArray(),
            BlockedEntityIds = _blockedEntityIds?.ToArray() ?? Array.Empty<string>(),
            FavoriteRegionOwners = Array.Empty<string>()
        };
    }

    /// <summary>ตรวจว่า targetId มี我们在在他的บล็อคลิสต์ไหม (ตอน offline — อ่าน save จากดิสก์)</summary>
    private bool IsBlockedOffline(string targetId)
    {
        try
        {
            string path = SaveStore.PlayerPath(targetId);
            if (!System.IO.File.Exists(path)) return false;
            PlayerSave save = SaveStore.Load<PlayerSave>(path);
            return save?.BlockedEntityIds?.Contains(EntityId) == true;
        }
        catch { return false; }
    }

    /// <summary>
    /// แก้ไข PlayerSave ของ offline player — ใช้สำหรับ mutation ที่ต้องผลกระทบข้าม session
    /// (เช่น friend request ที่ผู้รับ offline, mail attachment)
    /// </summary>
    private static void MutateOfflinePlayer(string entityId, Action<PlayerSave> mutate)
    {
        string path = SaveStore.PlayerPath(entityId);
        if (!System.IO.File.Exists(path)) return;
        try
        {
            PlayerSave save = SaveStore.Load<PlayerSave>(path);
            if (save == null) return;
            mutate(save);
            SaveStore.Save(path, save);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[social] offline mutate {entityId}: {e.Message}");
        }
    }

    /// <summary>init fields จาก PlayerSave (เรียกจาก LoadPersistedState)</summary>
    private void ApplySocialSave(PlayerSave save)
    {
        _friends = save.Friends ?? new List<string>();
        _receivedFriendRequests = save.ReceivedFriendRequests ?? new List<string>();
        _sentFriendRequests = save.SentFriendRequests ?? new List<string>();
        _followingEntityIds = save.FollowingEntityIds ?? new List<string>();
        _blockedEntityIds = save.BlockedEntityIds ?? new List<string>();
    }

    /// <summary>เขียนลง PlayerSave (เรียกจาก Save)</summary>
    private void FillSocialSave(PlayerSave save)
    {
        save.Friends = _friends;
        save.ReceivedFriendRequests = _receivedFriendRequests;
        save.SentFriendRequests = _sentFriendRequests;
        save.FollowingEntityIds = _followingEntityIds;
        save.BlockedEntityIds = _blockedEntityIds;
    }
}
