using System;
using System.Collections.Generic;
using System.IO;

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
        /// <summary>IP ที่จอง id นี้ครั้งแรก</summary>
        public string ClaimedFromIp { get; set; }
        public double ClaimedAt { get; set; }
        public double LastSeenAt { get; set; }
        public int Logins { get; set; }
    }

    private static string PathFor(string entityId)
    {
        return Path.Combine(SaveStore.Root, "accounts", SafeName(entityId) + ".json");
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
    public static bool TryClaim(string entityId, string name, string remoteIp, out string reason)
    {
        reason = null;
        if (Disabled || string.IsNullOrEmpty(entityId))
        {
            return true;
        }

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
                ClaimedFromIp = remoteIp,
                ClaimedAt = now,
                LastSeenAt = now,
                Logins = 1
            };
            SaveStore.Save(path, acc);
            Console.WriteLine($"[account] จอง {entityId} ({name}) ให้ {remoteIp}");
            return true;
        }

        // ชั้น 2: ต้องมาจาก IP เดิมที่จองไว้
        if (BindToFirstIp
            && !string.IsNullOrEmpty(acc.ClaimedFromIp)
            && !string.Equals(acc.ClaimedFromIp, remoteIp, StringComparison.Ordinal))
        {
            reason = $"entity id นี้ถูกจองไว้จาก {acc.ClaimedFromIp} แล้ว";
            return false;
        }

        acc.Name = string.IsNullOrEmpty(name) ? acc.Name : name;
        acc.LastSeenAt = now;
        acc.Logins++;
        SaveStore.Save(path, acc);
        return true;
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
