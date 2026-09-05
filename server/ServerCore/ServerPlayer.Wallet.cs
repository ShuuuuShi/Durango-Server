using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using Messages;
using Shared.Economy;

namespace DurangoServer.Core;

/// <summary>
/// Wallet system — กระเป๋าเงินโอน DurangoCoin ระหว่างผู้เล่น
///
/// โปรโตคอล:
/// - WalletUpdated → push เมื่อ balances เปลี่ยน (ส่งให้ทั้ง sender + recipient)
/// - TransferDurangoCoin → โอน coin จาก sender ไปหา recipient
///
/// ข้อจำกัด:
/// - Currency.DurangoCoin = PcCoin (ค่า 7 ใน enum) — ใช้ pcCoin เป็นตัวแทน DurangoCoin
/// - ต้องมี enough balance
/// - โอนได้เฉพาะ online players (offline mutation ผ่าน save)
/// - Feature gate: ServerConfig.Current.Features.Wallet
/// </summary>
public partial class ServerPlayer
{
    private Dictionary<Currency, long> _walletPaid;
    private Dictionary<Currency, long> _walletUnpaid;

    private static bool WalletEnabled => ServerConfig.Current.Features.Wallet;

    /// <summary>DurangoCoin = PcCoin ใน enum</summary>
    private const Currency DurangoCoinCurrency = Currency.PcCoin;

    private bool RejectWalletDisabled(PacketHeader header)
    {
        Send(new Info { Text = "ระบบกระเป๋าเงินยังไม่เปิดใช้งาน" }, header.Seq);
        Send(Aborts.Reason(), header.Seq);
        return false;
    }

    private void RegisterWalletHandlers()
    {
        _conn.Recv<TransferDurangoCoin>(HandleTransferDurangoCoin);
    }

    private void HandleTransferDurangoCoin(TransferDurangoCoin msg, PacketHeader header)
    {
        if (!WalletEnabled) { RejectWalletDisabled(header); return; }

        string recipientId = msg.RecipientEntityId;
        long amount = msg.Amount;
        if (string.IsNullOrEmpty(recipientId) || recipientId == EntityId)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (amount <= 0)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        long balance = GetWalletBalance(DurangoCoinCurrency);
        if (balance < amount)
        {
            Send(new Info { Text = $"DurangoCoin ไม่พอ (มี {balance}, ต้องการ {amount})" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        // Debit sender
        _walletPaid[DurangoCoinCurrency] = balance - amount;
        MarkDirty();
        SendWalletUpdated();

        // Credit recipient
        ServerPlayer recipient = _world.FindPlayer(recipientId);
        if (recipient != null)
        {
            recipient._walletPaid[DurangoCoinCurrency] =
                recipient.GetWalletBalance(DurangoCoinCurrency) + amount;
            recipient.MarkDirty();
            recipient.SendWalletUpdated();
        }
        else
        {
            MutateOfflinePlayer(recipientId, s =>
            {
                if (s.WalletPaid == null) s.WalletPaid = new Dictionary<string, long>();
                string key = DurangoCoinCurrency.ToString();
                long current = 0;
                if (s.WalletPaid.ContainsKey(key)) current = s.WalletPaid[key];
                s.WalletPaid[key] = current + amount;
            });
        }
        Send(default(OK), header.Seq);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    public long GetWalletBalance(Currency currency)
    {
        if (_walletPaid == null) return 0;
        return _walletPaid.TryGetValue(currency, out long val) ? val : 0;
    }

    public void AddWalletBalance(Currency currency, long amount)
    {
        if (_walletPaid == null) _walletPaid = new Dictionary<Currency, long>();
        if (_walletPaid.ContainsKey(currency))
            _walletPaid[currency] += amount;
        else
            _walletPaid[currency] = amount;
        MarkDirty();
    }

    public bool TryDebitWallet(Currency currency, long amount)
    {
        long bal = GetWalletBalance(currency);
        if (bal < amount) return false;
        _walletPaid[currency] = bal - amount;
        MarkDirty();
        return true;
    }

    private void SendWalletUpdated()
    {
        Send(new WalletUpdated
        {
            EntityId = EntityId,
            Wallet = BuildWalletSnapshot()
        });
    }

    private Wallet BuildWalletSnapshot()
    {
        return new Wallet
        {
            PaidBalances = _walletPaid ?? new Dictionary<Currency, long>(),
            UnpaidBalances = _walletUnpaid ?? new Dictionary<Currency, long>(),
            Vouchers = null
        };
    }

    /// <summary>init fields จาก PlayerSave</summary>
    private void ApplyWalletSave(PlayerSave save)
    {
        _walletPaid = new Dictionary<Currency, long>();
        _walletUnpaid = new Dictionary<Currency, long>();
        if (save.WalletPaid != null)
        {
            foreach (var kv in save.WalletPaid)
            {
                if (Enum.TryParse<Currency>(kv.Key, out Currency c))
                    _walletPaid[c] = kv.Value;
            }
        }
        if (save.WalletUnpaid != null)
        {
            foreach (var kv in save.WalletUnpaid)
            {
                if (Enum.TryParse<Currency>(kv.Key, out Currency c))
                    _walletUnpaid[c] = kv.Value;
            }
        }
    }

    /// <summary>เขียนลง PlayerSave</summary>
    private void FillWalletSave(PlayerSave save)
    {
        save.WalletPaid = new Dictionary<string, long>();
        save.WalletUnpaid = new Dictionary<string, long>();
        if (_walletPaid != null)
        {
            foreach (var kv in _walletPaid)
                save.WalletPaid[kv.Key.ToString()] = kv.Value;
        }
        if (_walletUnpaid != null)
        {
            foreach (var kv in _walletUnpaid)
                save.WalletUnpaid[kv.Key.ToString()] = kv.Value;
        }
    }
}
