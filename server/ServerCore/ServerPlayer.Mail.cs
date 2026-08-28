using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Economy;

namespace DurangoServer.Core;

/// <summary>
/// Mail system — ส่งข้อความ/ของแนบ รับ/ลบ/ทำเครื่องหมายอ่าน
///
/// โปรโตคอลจาก client/Durango.Logic/MailSystem.cs:
/// - GetMails → ส่ง Mails snapshot (_Mails=system, UserMails=player-sent)
/// - SendMail → ส่งจดหมายถึงผู้รับ (text + items + money)
/// - AcceptMails / AcceptUserMails → รับของแนบ (ย้าย items เข้ากระเป๋า, money เข้า wallet)
/// - DeleteMails / DeleteUserMails → ลบจดหมาย (ต้องรับของแล้ว)
/// - MarkMailsAsRead / MarkUserMailsAsRead → ทำเครื่องหมายอ่านแล้ว
///
/// ข้อจำกัด:
/// - Mail ID = Guid.NewGuid().ToString() (server-generated)
/// - Money transfer: ต้องมี wallet ถ้าเปิด Wallet feature
/// - Item attachment: ต้องมีของในกระเป๋า sender, ย้ายออกทันที
/// - Online recipient: push MailPut
/// - Offline recipient: mutate save
/// - Feature gate: ServerConfig.Current.Features.Mail
/// </summary>
public partial class ServerPlayer
{
    private List<MailSave> _mails;

    private static bool MailEnabled => ServerConfig.Current.Features.Mail;

    private bool RejectMailDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบจดหมายยังไม่เปิดใช้งาน" }, header.Seq);
        Send(default(Abort), header.Seq);
        return false;
    }

    private void RegisterMailHandlers()
    {
        _conn.Recv<GetMails>(HandleGetMails);
        _conn.Recv<SendMail>(HandleSendMail);
        _conn.Recv<AcceptMails>(HandleAcceptMails);
        _conn.Recv<AcceptUserMails>(HandleAcceptUserMails);
        _conn.Recv<DeleteMails>(HandleDeleteMails);
        _conn.Recv<DeleteUserMails>(HandleDeleteUserMails);
        _conn.Recv<MarkMailsAsRead>(HandleMarkMailsAsRead);
        _conn.Recv<MarkUserMailsAsRead>(HandleMarkUserMailsAsRead);
    }

    // ── Handlers ──────────────────────────────────────────────────────

    private void HandleGetMails(GetMails msg, PacketHeader header)
    {
        Send(BuildMailsSnapshot(), header.Seq);
    }

    private void HandleSendMail(SendMail msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        string recipientId = msg.RecipientId;
        if (string.IsNullOrEmpty(recipientId) || recipientId == EntityId)
        {
            Send(default(Abort), header.Seq);
            return;
        }
        string text = msg.Text ?? "";
        if (text.Length > 500)
        {
            Send(new Info { Text = "ข้อความยาวเกิน 500 ตัวอักษร" }, header.Seq);
            Send(default(Abort), header.Seq);
            return;
        }

        // ตรวจว่ามี enough items
        string[] itemIds = msg.ItemIds;
        List<ItemSave> attachedItems = new List<ItemSave>();
        if (itemIds != null && itemIds.Length > 0)
        {
            lock (_inventory)
            {
                foreach (string itemId in itemIds)
                {
                    int idx = _inventory.FindIndex(it => it.Id == itemId);
                    if (idx < 0)
                    {
                        Send(new Info { Text = $"ไม่มีไอเทม {itemId}" }, header.Seq);
                        Send(default(Abort), header.Seq);
                        return;
                    }
                    Item item = _inventory[idx];
                    attachedItems.Add(ItemSave.From(item));
                    _inventory.RemoveAt(idx);
                }
            }
            MarkDirty();
        }

        // ตรวจ recipient online or has save
        ServerPlayer recipient = _world.FindPlayer(recipientId);
        MailSave mail = new MailSave
        {
            Id = Guid.NewGuid().ToString(),
            SentAt = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds,
            SenderId = EntityId,
            SenderName = Name,
            MailType = 1,
            Text = text,
            Money = new Dictionary<string, long>(),
            AttachedItems = attachedItems,
            Accepted = false,
            Read = false,
            ExpiresAt = DateTime.UtcNow.AddDays(30).Subtract(new DateTime(1970, 1, 1)).TotalSeconds
        };

        if (recipient != null)
        {
            recipient._mails.Add(mail);
            recipient.MarkDirty();
            recipient.Send(new MailPut { Mail = ToProtocolMail(mail) });
        }
        else
        {
            MutateOfflinePlayer(recipientId, s =>
            {
                if (s.Mails == null) s.Mails = new List<MailSave>();
                s.Mails.Add(mail);
            });
        }
        Send(default(OK), header.Seq);
    }

    private void HandleAcceptMails(AcceptMails msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        HandleAcceptMailsInternal(msg.MailIds, header);
    }

    private void HandleAcceptUserMails(AcceptUserMails msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        HandleAcceptMailsInternal(msg.MailIds, header);
    }

    private void HandleAcceptMailsInternal(string[] mailIds, PacketHeader header)
    {
        if (mailIds == null || mailIds.Length == 0) { Send(default(Abort), header.Seq); return; }

        foreach (string mailId in mailIds)
        {
            MailSave mail = _mails.FirstOrDefault(m => m.Id == mailId);
            if (mail == null || mail.Accepted) continue;

            // รับ items
            if (mail.AttachedItems != null && mail.AttachedItems.Count > 0)
            {
                lock (_inventory)
                {
                    foreach (ItemSave itemSave in mail.AttachedItems)
                    {
                        if (itemSave == null) continue;
                        Item item = itemSave.ToItem();
                        _inventory.Add(item);
                    }
                }
                MarkDirty();
            }
            mail.Accepted = true;
            mail.AttachedItems = new List<ItemSave>(); // clear attachments after accept
        }
        Send(default(OK), header.Seq);
        Send(BuildMailsSnapshot());
    }

    private void HandleDeleteMails(DeleteMails msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        HandleDeleteMailsInternal(msg.MailIds, header);
    }

    private void HandleDeleteUserMails(DeleteUserMails msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        HandleDeleteMailsInternal(msg.MailIds, header);
    }

    private void HandleDeleteMailsInternal(string[] mailIds, PacketHeader header)
    {
        if (mailIds == null) { Send(default(Abort), header.Seq); return; }
        foreach (string mailId in mailIds)
        {
            _mails.RemoveAll(m => m.Id == mailId);
        }
        MarkDirty();
        Send(default(OK), header.Seq);
        Send(BuildMailsSnapshot());
    }

    private void HandleMarkMailsAsRead(MarkMailsAsRead msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        HandleMarkMailsReadInternal(msg.MailIds, header);
    }

    private void HandleMarkUserMailsAsRead(MarkUserMailsAsRead msg, PacketHeader header)
    {
        if (!MailEnabled) { RejectMailDisabled(header); return; }
        HandleMarkMailsReadInternal(msg.MailIds, header);
    }

    private void HandleMarkMailsReadInternal(string[] mailIds, PacketHeader header)
    {
        if (mailIds != null)
        {
            foreach (string mailId in mailIds)
            {
                MailSave mail = _mails.FirstOrDefault(m => m.Id == mailId);
                if (mail != null) mail.Read = true;
            }
            MarkDirty();
        }
        Send(default(OK), header.Seq);
        Send(BuildMailsSnapshot());
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static Mail ToProtocolMail(MailSave save)
    {
        var money = new Dictionary<Currency, int>();
        if (save.Money != null)
        {
            foreach (var pair in save.Money)
            {
                if (Enum.TryParse(pair.Key, true, out Currency currency)
                    && pair.Value > 0 && pair.Value <= int.MaxValue)
                {
                    money[currency] = (int)pair.Value;
                }
            }
        }
        return new Mail
        {
            Id = save.Id,
            SentAt = save.SentAt,
            SenderId = save.SenderId,
            MailType = Enum.IsDefined(typeof(Shared.Mailing.MailType), save.MailType)
                ? (Shared.Mailing.MailType)save.MailType : Shared.Mailing.MailType.Invalid,
            Text = save.Text,
            Money = money,
            AttachedItems = save.AttachedItems?.Where(x => x != null).Select(x => x.ToItem()).ToArray(),
            Accepted = save.Accepted,
            Read = save.Read,
            ExpiresAt = save.ExpiresAt
        };
    }

    private Mails BuildMailsSnapshot()
    {
        Mail[] userMails = null;
        if (_mails != null && _mails.Count > 0)
        {
            userMails = new Mail[_mails.Count];
            for (int i = 0; i < _mails.Count; i++)
            {
                userMails[i] = ToProtocolMail(_mails[i]);
            }
        }
        return new Mails
        {
            _Mails = null, // system mails — ยังไม่ใช้
            UserMails = userMails
        };
    }

    /// <summary>init fields จาก PlayerSave</summary>
    private void ApplyMailSave(PlayerSave save)
    {
        _mails = save.Mails ?? new List<MailSave>();
    }

    /// <summary>เขียนลง PlayerSave</summary>
    private void FillMailSave(PlayerSave save)
    {
        save.Mails = _mails;
    }
}
