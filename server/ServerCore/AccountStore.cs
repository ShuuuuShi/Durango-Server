using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace DurangoServer.Core;

/// <summary>
/// H-1: ผูก "entity id" เข้ากับเจ้าของ — กันคนอื่นสวมรอย
///
/// ปัญหาเดิม: `POST /sessions` ให้ client บอก entity id ของตัวเองมาดื้อ ๆ
/// แต่ entity id เป็นของสาธารณะ (มากับ AppearPlayer/Move/Damaged ที่ broadcast ให้ทุกคน)
/// ⇒ ใครก็ขอ token ของคนอื่นได้ แล้วเข้าเกมด้วยตัวละคร+ของของเขาทั้งดุ้น
/// (GP-12 แก้แค่ "token ต้องเป็นของที่ server ออก" ไม่ได้แก้ "ใครขอ id ไหนก็ได้")
///
/// ตัวเกมไม่ได้ส่งรหัสผ่านอะไรมาเลย (ดู player_info: มีแค่ ชื่อ/เลเวล/entity id)
/// และเราแก้ตัว client ไม่ได้ จึงกันด้วย 2 ชั้นที่ทำได้จริงฝั่ง server:
///
///   ชั้น 1 — รายชื่อที่อนุญาต (whitelist): มีไฟล์รายชื่อเมื่อไหร่ คนนอกรายชื่อเข้าไม่ได้เลย
///   ชั้น 2 — จองตอนเข้าครั้งแรก (first-claim): entity id ผูกกับ "ที่อยู่ IP ที่จองไว้ครั้งแรก"
///            คนละ IP มาอ้าง id เดิม = ปฏิเสธ
///
/// ทั้งสองชั้นปิดได้ด้วย --no-account-check (เช่นตอนเทสในเครื่องเดียว)
/// ดูรายละเอียดที่ docs/server/Accounts.md
/// </summary>
public static class AccountStore
{
    /// <summary>ปิดการตรวจทั้งหมด (ตั้งด้วย <c>--no-account-check</c>)</summary>
    public static bool Disabled { get; set; }

    /// <summary>ตรวจ IP ที่จองไว้ครั้งแรกไหม (ปิดด้วย <c>--no-ip-bind</c> เช่นเมื่อทุกคนอยู่หลัง NAT เดียวกัน)</summary>
    public static bool BindToFirstIp { get; set; } = true;

    /// <summary>ไฟล์รายชื่อที่อนุญาต — ไม่มีไฟล์ = ไม่ใช้ชั้นนี้ (ใครก็สมัครได้ แต่ยังโดนชั้น 2)</summary>
    public static string WhitelistPath { get; set; }

    private static HashSet<string> _whitelist;

    /// <summary>เวลาที่ไฟล์รายชื่อถูกแก้ล่าสุดตอนโหลด — ใช้โหลดซ้ำเองเมื่อไฟล์เปลี่ยน</summary>
    private static DateTime _whitelistStamp;

    public sealed class Account
    {
        public string EntityId { get; set; }
        public string Name { get; set; }
        public string OwnerKey { get; set; }
        /// <summary>IP ที่จอง id นี้ครั้งแรก</summary>
        public string ClaimedFromIp { get; set; }
        public double ClaimedAt { get; set; }
        public double LastSeenAt { get; set; }
        public int Logins { get; set; }
    }

    private sealed class OwnerRecord
    {
        public string OwnerKey { get; set; }
        public double FirstSeenAt { get; set; }
    }

    private static string PathFor(string entityId)
    {
        return Path.Combine(SaveStore.Root, "accounts", SafeName(entityId) + ".json");
    }

    private static string OwnerPath(string ownerKey)
    {
        return Path.Combine(SaveStore.Root, "owners", SafeName(ownerKey) + ".json");
    }

    private static void RememberOwner(string ownerKey)
    {
        if (string.IsNullOrEmpty(ownerKey))
        {
            return;
        }

        string path = OwnerPath(ownerKey);
        if (SaveStore.Peek<OwnerRecord>(path) != null)
        {
            return;
        }

        SaveStore.Save(path, new OwnerRecord
        {
            OwnerKey = ownerKey,
            FirstSeenAt = Durango.Utils.Times.UnixTimeNow()
        });
    }

    private static string SafeName(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return "_unknown";
        }
        char[] bad = Path.GetInvalidFileNameChars();
        char[] buf = raw.ToCharArray();
        for (int i = 0; i < buf.Length; i++)
        {
            if (Array.IndexOf(bad, buf[i]) != -1)
            {
                buf[i] = '_';
            }
        }
        string name = new string(buf);
        return name.Length > 120 ? name.Substring(0, 120) : name;
    }

    /// <summary>โหลดรายชื่อที่อนุญาต (1 บรรทัด = 1 entity id หรือชื่อตัวละคร, # = คอมเมนต์)</summary>
    public static int LoadWhitelist()
    {
        _whitelist = null;
        if (string.IsNullOrEmpty(WhitelistPath) || !File.Exists(WhitelistPath))
        {
            return 0;
        }
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string line in File.ReadAllLines(WhitelistPath))
        {
            string s = line.Trim();
            if (s.Length == 0 || s.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }
            set.Add(s);
        }
        _whitelist = set.Count > 0 ? set : null;
        _whitelistStamp = File.GetLastWriteTimeUtc(WhitelistPath);
        return set.Count;
    }

    /// <summary>
    /// โหลดรายชื่อใหม่ถ้าไฟล์ถูกแก้ — เจ้าของเซิร์ฟจะได้เพิ่มเพื่อนได้โดยไม่ต้องรีสตาร์ท
    /// (ผู้เล่นที่กำลังเล่นอยู่จะไม่หลุด)
    /// </summary>
    private static void ReloadWhitelistIfChanged()
    {
        if (string.IsNullOrEmpty(WhitelistPath) || !File.Exists(WhitelistPath))
        {
            return;
        }
        try
        {
            if (File.GetLastWriteTimeUtc(WhitelistPath) != _whitelistStamp)
            {
                int n = LoadWhitelist();
                Console.WriteLine($"[account] รายชื่อที่อนุญาตถูกแก้ — โหลดใหม่ {n} รายการ");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("[account] อ่านรายชื่อใหม่ไม่ได้: " + e.Message);
        }
    }

    public static bool WhitelistActive => _whitelist != null;

    /// <summary>
    /// ขอเข้าเกมด้วย entity id นี้จาก IP นี้ — คืน false พร้อมเหตุผลถ้าไม่ให้ผ่าน
    /// ผ่านแล้วจะบันทึก/อัปเดตไฟล์ account ให้เอง
    /// </summary>
    /// <summary>
    /// [แก้เอง] 25 ส.ค. 2026 — 🐛 ต้นเหตุ "บังคับสร้างตัวใหม่": client เชื่อมมาทาง loopback ที่ .NET
    /// มองเป็นคนละสตริงกับที่ account จองไว้ (`127.0.0.1`) — เจอจริง: `POST /accounts` ผ่าน localhost
    /// (=IPv6 ::1) คืนรายการว่าง แต่ผ่าน 127.0.0.1 คืนตัวละครครบ ⇒ หน้าเลือกตัวว่าง เลยเด้งไปสร้างใหม่
    /// ทำให้ทุก IP อยู่ในรูปมาตรฐานเดียวก่อนเทียบ: ::1 → 127.0.0.1, ::ffff:a.b.c.d → a.b.c.d
    /// </summary>
    public static string NormalizeIp(string ip)
    {
        if (string.IsNullOrEmpty(ip) || ip == "?")
        {
            return ip;
        }
        if (!IPAddress.TryParse(ip, out IPAddress addr))
        {
            return ip;
        }
        if (IPAddress.IsLoopback(addr))
        {
            return "127.0.0.1";                       // ::1 และ 127.0.0.1 = เครื่องเดียวกัน
        }
        if (addr.IsIPv4MappedToIPv6)
        {
            return addr.MapToIPv4().ToString();       // ::ffff:192.168.1.5 → 192.168.1.5
        }
        return addr.ToString();
    }

    public static bool TryClaim(string entityId, string name, string remoteIp, string ownerKey, out string reason)
    {
        reason = null;
        if (Disabled || string.IsNullOrEmpty(entityId))
        {
            return true;
        }
        remoteIp = NormalizeIp(remoteIp);

        ReloadWhitelistIfChanged();

        // ชั้น 1: รายชื่อที่อนุญาต
        if (_whitelist != null
            && !_whitelist.Contains(entityId)
            && (string.IsNullOrEmpty(name) || !_whitelist.Contains(name.Trim())))
        {
            reason = "ไม่ได้อยู่ในรายชื่อที่อนุญาต";
            return false;
        }

        double now = Durango.Utils.Times.UnixTimeNow();
        string path = PathFor(entityId);
        Account acc = SaveStore.Load<Account>(path);

        if (acc == null)
        {
            // ยังไม่มีใครจอง id นี้ = คนแรกที่มาถึงได้ไป
            acc = new Account
            {
                EntityId = entityId,
                Name = name,
                OwnerKey = ownerKey,
                ClaimedFromIp = remoteIp,
                ClaimedAt = now,
                LastSeenAt = now,
                Logins = 1
            };
            SaveStore.Save(path, acc);
            RememberOwner(ownerKey);
            Console.WriteLine($"[account] จอง {entityId} ({name}) ให้ {remoteIp}");
            return true;
        }

        // ชั้น 2: ต้องมาจาก IP เดิมที่จองไว้
        if (BindToFirstIp
            && !string.IsNullOrEmpty(acc.ClaimedFromIp)
            && !string.Equals(NormalizeIp(acc.ClaimedFromIp), remoteIp, StringComparison.Ordinal))
        {
            reason = $"entity id นี้ถูกจองไว้จาก {acc.ClaimedFromIp} แล้ว";
            return false;
        }

        if (!string.IsNullOrEmpty(acc.OwnerKey)
            && !string.IsNullOrEmpty(ownerKey)
            && !string.Equals(acc.OwnerKey, ownerKey, StringComparison.Ordinal))
        {
            reason = "ตัวละครนี้เป็นของบัญชีอื่น";
            return false;
        }

        acc.Name = string.IsNullOrEmpty(name) ? acc.Name : name;
        if (string.IsNullOrEmpty(acc.OwnerKey) && !string.IsNullOrEmpty(ownerKey))
        {
            acc.OwnerKey = ownerKey;
        }
        acc.LastSeenAt = now;
        acc.Logins++;
        SaveStore.Save(path, acc);
        RememberOwner(ownerKey);
        return true;
    }

    /// <summary>
    /// GP-15: หา entity id ทั้งหมดที่เคยจองไว้จาก IP นี้ — ใช้ตอบ `/accounts` (หน้าเลือกตัวละครฝั่ง client)
    ///
    /// เดิม client ใช้ตัวแปรในหน่วยความจำ (`Durango.Offline.Server._localPlayer`) แทน เพราะ endpoint นี้
    /// ยังไม่มี — ปัญหาคือตัวแปรนั้นหายไปทุกครั้งที่ปิดเกม ⇒ ตัวละครเก่ายังอยู่ในเซฟจริง แต่หน้าเลือก
    /// ตัวละครว่างเปล่าทุกครั้งที่เปิดเกมใหม่ บังคับสร้างใหม่ตลอด (ดู HANDOFF.md วันที่แก้)
    ///
    /// ไม่มี index แยกต่างหาก — ไฟล์ account มีไม่เยอะ (จำนวนคนเล่นจริง) สแกนทั้งโฟลเดอร์ทุกครั้งที่ถามพอ
    /// ไม่คุ้มทำ index แยกแล้วต้องคอยดูแลให้ตรงกันเวลาไฟล์ถูกลบ/แก้มือ
    /// </summary>
    public static List<Account> FindByIp(string remoteIp)
    {
        var result = new List<Account>();
        string dir = Path.Combine(SaveStore.Root, "accounts");
        if (string.IsNullOrEmpty(remoteIp) || !Directory.Exists(dir))
        {
            return result;
        }
        remoteIp = NormalizeIp(remoteIp);                 // ::1 / ::ffff:127.0.0.1 → 127.0.0.1 (ดู NormalizeIp)
        foreach (string file in Directory.EnumerateFiles(dir, "*.json"))
        {
            Account acc = SaveStore.Load<Account>(file);
            if (acc != null && string.Equals(NormalizeIp(acc.ClaimedFromIp), remoteIp, StringComparison.Ordinal))
            {
                result.Add(acc);
            }
        }
        // [แก้เอง] 25 ส.ค. 2026 — เดิมคืนตามลำดับที่ Directory.EnumerateFiles เจอ (สุ่ม/ตามชื่อไฟล์ ไม่ใช่
        // ตามความเก่าใหม่) — ตอนทดสอบสะสมไฟล์ account ไว้ 80+ อันจาก IP เดียว (127.0.0.1) เจอว่าตัวละคร
        // ที่เพิ่งเล่นจริงโผล่ปนกับของทดสอบเก่าแบบสุ่มลำดับ — เรียงเอาที่เล่นล่าสุดขึ้นก่อนเสมอ ให้ตัวที่
        // เจ้าของกำลังเล่นอยู่จริงมีโอกาสถูกเลือก/แสดงก่อนของเก่าที่ทิ้งไว้
        result.Sort((a, b) => b.LastSeenAt.CompareTo(a.LastSeenAt));
        return result;
    }

    /// <summary>
    /// Return only characters owned by one persistent client installation.
    /// Legacy saves did not have OwnerKey; on the first request, migrate only
    /// the most recently used legacy character so old localhost test accounts
    /// do not appear as somebody else's characters.
    /// </summary>
    public static List<Account> FindByOwner(string remoteIp, string ownerKey)
    {
        List<Account> byIp = FindByIp(remoteIp);
        if (string.IsNullOrEmpty(ownerKey))
        {
            return byIp;
        }

        List<Account> owned = byIp.FindAll(account =>
            !string.IsNullOrEmpty(account.OwnerKey)
            && string.Equals(account.OwnerKey, ownerKey, StringComparison.Ordinal));
        if (owned.Count > 0)
        {
            RememberOwner(ownerKey);
            return owned;
        }

        // Once an installation has been seen, an empty list really means it has
        // no characters. Do not migrate another old localhost test character
        // after the owner deletes their final character.
        if (SaveStore.Peek<OwnerRecord>(OwnerPath(ownerKey)) != null)
        {
            return owned;
        }

        Account legacy = byIp.Find(account => string.IsNullOrEmpty(account.OwnerKey));
        if (legacy != null)
        {
            legacy.OwnerKey = ownerKey;
            SaveStore.Save(PathFor(legacy.EntityId), legacy);
            Console.WriteLine($"[account] migrated legacy owner {legacy.EntityId} -> {ownerKey}");
            owned.Add(legacy);
        }

        // Remember even a brand-new owner with no legacy data so repeated empty
        // account requests stay empty until that owner creates a character.
        RememberOwner(ownerKey);
        return owned;
    }

    /// <summary>ล้างการจองของ entity id (ให้เจ้าของเครื่องแก้เคสย้าย IP/เปลี่ยนเน็ต)</summary>
    public static bool Release(string entityId)
    {
        string path = PathFor(entityId);
        if (!File.Exists(path))
        {
            return false;
        }
        File.Delete(path);
        Console.WriteLine($"[account] ปลดการจอง {entityId} แล้ว");
        return true;
    }
}
