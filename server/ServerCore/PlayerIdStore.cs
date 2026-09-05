using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DurangoServer.Core;

/// <summary>
/// [4 ก.ย. 2026] DurangoID — ระบบสมัครไอดีของเราเอง (เจ้าของสั่งทำ "ใช้เลขสุ่มจากเราเป็นไอดี")
///
/// ## ทำไมต้องมี
/// ตัวตนของผู้เล่นเดิมผูกกับ **IP อย่างเดียว** (<see cref="AccountStore"/>) ซึ่งพังกับมือถือ:
/// เกมมือถือของแท้ส่ง `account_id` (NPSN) และ `adid` มาเป็น **ค่าว่าง** ทั้งคู่ เพราะ
/// `Platform_Android` ไม่ override สองตัวนี้ (ต่างจาก `Platform_PC` ที่สร้าง GUID เก็บใน PlayerPrefs)
/// ⇒ เน็ตมือถือสลับ IP เมื่อไหร่ ตัวละครหายทันที และเราแก้โค้ดใน APK ไม่ได้ (IL2CPP)
///
/// ## วิธีที่ใช้ (เจ้าของเลือก: "ผูกผ่านหน้าเว็บด้วย IP")
/// ผู้เล่นเปิดหน้าเว็บ <c>/id</c> **จากเครื่องที่จะเล่น** → สมัครได้เลข 8 หลัก + ตั้ง PIN →
/// กด "ผูกเครื่องนี้" → เซิร์ฟจด IP ปัจจุบันไว้ให้ไอดีนั้น (ค่าเริ่มต้น 30 วัน)
/// พอเข้าเกม เซิร์ฟเทียบ IP แล้วรู้ว่าเป็นไอดีไหน จึงคืนตัวละครได้ถูกคน
///
/// ## หลาย ๆ คนใช้ IP เดียวกัน (บ้านเดียวกัน/CGNAT)
/// เก็บ binding เป็น **รายการ** ต่อ IP ไม่ใช่ค่าเดียว — `/accounts` จะคืนตัวละครของทุกไอดีที่ผูก IP นั้นไว้
/// แล้วให้หน้าเลือกตัวละครของตัวเกมเป็นคนแยกเอง (ไม่ต้องแย่ง binding กัน)
/// ส่วนตอน **สร้าง** ตัวละครใหม่ ใช้ไอดีที่ผูกล่าสุด เพราะเป็นคนที่เพิ่งกดผูกจากหน้าเว็บ
/// </summary>
public static class PlayerIdStore
{
    /// <summary>ความยาวของเลขไอดี — 8 หลัก แสดงเป็น 4831-7266</summary>
    private const int IdDigits = 8;

    private const int PinIterations = 120_000;

    /// <summary>กันเดา PIN — จำนวนครั้งที่ผิดได้ต่อ IP ในหนึ่งช่วงเวลา</summary>
    private const int MaxFailuresPerIp = 10;

    private static readonly TimeSpan FailureWindow = TimeSpan.FromMinutes(15);

    private static readonly Dictionary<string, List<DateTime>> _failures = new();

    private static readonly object _lock = new();

    public sealed class Record
    {
        /// <summary>เลข 8 หลักไม่มีขีด เช่น "48317266"</summary>
        public string Id { get; set; }
        public string PinHash { get; set; }
        public string PinSalt { get; set; }
        /// <summary>ชื่อที่ผู้เล่นตั้งไว้ให้ไอดี (ไม่บังคับ ใช้โชว์ในหน้าแอดมิน)</summary>
        public string DisplayName { get; set; }
        public double CreatedAt { get; set; }
        public double LastLoginAt { get; set; }
        public string CreatedFromIp { get; set; }
        /// <summary>ตัวละครที่เป็นของไอดีนี้</summary>
        public List<string> EntityIds { get; set; } = new();
        public bool Banned { get; set; }
        public string BanReason { get; set; }
    }

    public sealed class Binding
    {
        public string Id { get; set; }
        public double BoundAt { get; set; }
        public double ExpiresAt { get; set; }
    }

    private sealed class BindingFile
    {
        public string Ip { get; set; }
        public List<Binding> Bindings { get; set; } = new();
    }

    // ---------- ที่อยู่ไฟล์ ----------

    private static string IdPath(string id) => Path.Combine(SaveStore.Root, "ids", Safe(id) + ".json");

    private static string BindingPath(string ip) => Path.Combine(SaveStore.Root, "idbindings", Safe(ip) + ".json");

    private static string Safe(string raw)
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

    // ---------- รูปแบบเลขไอดี ----------

    /// <summary>ตัดขีด/ช่องว่างออกจากเลขที่ผู้เล่นพิมพ์ ("4831-7266" → "48317266")</summary>
    public static string Normalize(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return "";
        }
        StringBuilder sb = new StringBuilder(IdDigits);
        foreach (char c in raw)
        {
            if (c >= '0' && c <= '9')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>แสดงให้ผู้เล่นอ่านง่าย ("48317266" → "4831-7266")</summary>
    public static string Format(string id)
    {
        string n = Normalize(id);
        return n.Length == IdDigits ? n.Substring(0, 4) + "-" + n.Substring(4) : n;
    }

    // ---------- PIN ----------

    private static string HashPin(string pin, string saltB64)
    {
        byte[] salt = Convert.FromBase64String(saltB64);
        using var kdf = new Rfc2898DeriveBytes(pin, salt, PinIterations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(kdf.GetBytes(32));
    }

    private static bool PinLooksValid(string pin) =>
        !string.IsNullOrEmpty(pin) && pin.Length >= 4 && pin.Length <= 12;

    /// <summary>ผิดบ่อยเกินไปจาก IP นี้หรือยัง (กันไล่เดา PIN)</summary>
    private static bool TooManyFailures(string ip)
    {
        lock (_lock)
        {
            if (!_failures.TryGetValue(ip ?? "?", out List<DateTime> list))
            {
                return false;
            }
            list.RemoveAll(t => DateTime.UtcNow - t > FailureWindow);
            return list.Count >= MaxFailuresPerIp;
        }
    }

    private static void NoteFailure(string ip)
    {
        lock (_lock)
        {
            if (!_failures.TryGetValue(ip ?? "?", out List<DateTime> list))
            {
                list = new List<DateTime>();
                _failures[ip ?? "?"] = list;
            }
            list.Add(DateTime.UtcNow);
        }
    }

    // ---------- สมัคร / เข้าสู่ระบบ ----------

    /// <summary>สุ่มเลขไอดีที่ยังไม่มีใครใช้ — ใช้ RandomNumberGenerator ไม่ใช่ Random ธรรมดา</summary>
    private static string NewUniqueId()
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            // หลักแรกไม่เป็น 0 เพื่อให้เลขมี 8 หลักเสมอเวลาพิมพ์/อ่าน
            var sb = new StringBuilder(IdDigits);
            sb.Append((char)('1' + RandomNumberGenerator.GetInt32(9)));
            for (int i = 1; i < IdDigits; i++)
            {
                sb.Append((char)('0' + RandomNumberGenerator.GetInt32(10)));
            }
            string id = sb.ToString();
            if (!File.Exists(IdPath(id)))
            {
                return id;
            }
        }
        return null;
    }

    /// <summary>สมัครไอดีใหม่ — คืนเลขที่ได้ หรือ null พร้อมเหตุผล</summary>
    public static Record Register(string pin, string displayName, string remoteIp, out string error)
    {
        error = null;
        if (!PinLooksValid(pin))
        {
            error = "PIN ต้องยาว 4-12 ตัว";
            return null;
        }

        string id = NewUniqueId();
        if (id == null)
        {
            error = "สุ่มเลขไอดีไม่สำเร็จ ลองใหม่อีกครั้ง";
            return null;
        }

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        string saltB64 = Convert.ToBase64String(salt);
        double now = Durango.Utils.Times.UnixTimeNow();
        var rec = new Record
        {
            Id = id,
            PinSalt = saltB64,
            PinHash = HashPin(pin, saltB64),
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim(),
            CreatedAt = now,
            LastLoginAt = now,
            CreatedFromIp = AccountStore.NormalizeIp(remoteIp)
        };
        SaveStore.Save(IdPath(id), rec);
        Console.WriteLine($"[id] สมัครไอดีใหม่ {Format(id)} จาก {rec.CreatedFromIp}");
        return rec;
    }

    public static Record Find(string id)
    {
        string n = Normalize(id);
        return n.Length == IdDigits ? SaveStore.Load<Record>(IdPath(n)) : null;
    }

    /// <summary>ตรวจไอดี+PIN — คืน record ถ้าถูก</summary>
    public static Record Login(string id, string pin, string remoteIp, out string error)
    {
        error = null;
        string ip = AccountStore.NormalizeIp(remoteIp);
        if (TooManyFailures(ip))
        {
            error = "กรอกผิดหลายครั้งเกินไป รอสัก 15 นาทีแล้วลองใหม่";
            return null;
        }

        Record rec = Find(id);
        if (rec == null || string.IsNullOrEmpty(pin))
        {
            NoteFailure(ip);
            error = "ไอดีหรือ PIN ไม่ถูกต้อง";
            return null;
        }
        if (rec.Banned)
        {
            error = "ไอดีนี้ถูกระงับ" + (string.IsNullOrEmpty(rec.BanReason) ? "" : " — " + rec.BanReason);
            return null;
        }

        string given = HashPin(pin, rec.PinSalt);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(given), Encoding.UTF8.GetBytes(rec.PinHash ?? "")))
        {
            NoteFailure(ip);
            error = "ไอดีหรือ PIN ไม่ถูกต้อง";
            return null;
        }

        rec.LastLoginAt = Durango.Utils.Times.UnixTimeNow();
        SaveStore.Save(IdPath(rec.Id), rec);
        return rec;
    }

    /// <summary>เปลี่ยน PIN (ต้องรู้ PIN เดิม)</summary>
    public static bool ChangePin(string id, string oldPin, string newPin, string remoteIp, out string error)
    {
        Record rec = Login(id, oldPin, remoteIp, out error);
        if (rec == null)
        {
            return false;
        }
        if (!PinLooksValid(newPin))
        {
            error = "PIN ใหม่ต้องยาว 4-12 ตัว";
            return false;
        }
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        rec.PinSalt = Convert.ToBase64String(salt);
        rec.PinHash = HashPin(newPin, rec.PinSalt);
        SaveStore.Save(IdPath(rec.Id), rec);
        return true;
    }

    // ---------- ผูกเครื่อง (IP) ----------

    private static BindingFile LoadBindings(string ip)
    {
        BindingFile file = SaveStore.Load<BindingFile>(BindingPath(ip)) ?? new BindingFile { Ip = ip };
        file.Bindings ??= new List<Binding>();
        double now = Durango.Utils.Times.UnixTimeNow();
        file.Bindings.RemoveAll(b => b.ExpiresAt > 0 && b.ExpiresAt < now);
        return file;
    }

    /// <summary>ผูกไอดีนี้เข้ากับ IP ที่กำลังเรียกมา — เรียกหลัง <see cref="Login"/> ผ่านแล้วเท่านั้น</summary>
    public static void Bind(string id, string remoteIp, int days)
    {
        string ip = AccountStore.NormalizeIp(remoteIp);
        BindingFile file = LoadBindings(ip);
        double now = Durango.Utils.Times.UnixTimeNow();
        double expires = days > 0 ? now + days * 86400.0 : 0;

        Binding existing = file.Bindings.Find(b => b.Id == Normalize(id));
        if (existing != null)
        {
            existing.BoundAt = now;
            existing.ExpiresAt = expires;
        }
        else
        {
            file.Bindings.Add(new Binding { Id = Normalize(id), BoundAt = now, ExpiresAt = expires });
        }
        SaveStore.Save(BindingPath(ip), file);
        Console.WriteLine($"[id] ผูก {Format(id)} เข้ากับ {ip} ({(days > 0 ? days + " วัน" : "ไม่มีวันหมดอายุ")})");
    }

    /// <summary>เลิกผูกไอดีนี้จาก IP ที่กำลังเรียกมา</summary>
    public static bool Unbind(string id, string remoteIp)
    {
        string ip = AccountStore.NormalizeIp(remoteIp);
        BindingFile file = LoadBindings(ip);
        int removed = file.Bindings.RemoveAll(b => b.Id == Normalize(id));
        SaveStore.Save(BindingPath(ip), file);
        return removed > 0;
    }

    /// <summary>ไอดีทั้งหมดที่ผูกกับ IP นี้อยู่ (ใหม่สุดก่อน) — ตัวที่หมดอายุถูกตัดออกแล้ว</summary>
    public static List<string> BoundIds(string remoteIp)
    {
        BindingFile file = LoadBindings(AccountStore.NormalizeIp(remoteIp));
        file.Bindings.Sort((a, b) => b.BoundAt.CompareTo(a.BoundAt));
        var ids = new List<string>(file.Bindings.Count);
        foreach (Binding b in file.Bindings)
        {
            ids.Add(b.Id);
        }
        return ids;
    }

    /// <summary>ไอดีที่ผูกล่าสุดของ IP นี้ — ใช้ตอนสร้างตัวละครใหม่ (คนที่เพิ่งกดผูกจากหน้าเว็บ)</summary>
    public static string LatestBoundId(string remoteIp)
    {
        List<string> ids = BoundIds(remoteIp);
        return ids.Count > 0 ? ids[0] : null;
    }

    // ---------- ตัวละครของไอดี ----------

    /// <summary>ผูกตัวละครเข้ากับไอดี (เรียกตอนสร้างตัวละครใหม่ และตอนรับตัวละครเดิม)</summary>
    public static void AttachEntity(string id, string entityId)
    {
        Record rec = Find(id);
        if (rec == null || string.IsNullOrEmpty(entityId))
        {
            return;
        }
        rec.EntityIds ??= new List<string>();
        if (!rec.EntityIds.Contains(entityId))
        {
            rec.EntityIds.Add(entityId);
            SaveStore.Save(IdPath(rec.Id), rec);
            Console.WriteLine($"[id] ตัวละคร {entityId} เป็นของไอดี {Format(rec.Id)} แล้ว");
        }
    }

    public static void DetachEntity(string id, string entityId)
    {
        Record rec = Find(id);
        if (rec?.EntityIds == null)
        {
            return;
        }
        if (rec.EntityIds.Remove(entityId))
        {
            SaveStore.Save(IdPath(rec.Id), rec);
        }
    }

    /// <summary>ไอดีที่เป็นเจ้าของตัวละครนี้ — สแกนโฟลเดอร์ ids (ไฟล์มีไม่เยอะเท่าจำนวนคนเล่นจริง)</summary>
    public static string OwnerOf(string entityId)
    {
        string dir = Path.Combine(SaveStore.Root, "ids");
        if (string.IsNullOrEmpty(entityId) || !Directory.Exists(dir))
        {
            return null;
        }
        foreach (string file in Directory.EnumerateFiles(dir, "*.json"))
        {
            Record rec = SaveStore.Load<Record>(file);
            if (rec?.EntityIds != null && rec.EntityIds.Contains(entityId))
            {
                return rec.Id;
            }
        }
        return null;
    }

    /// <summary>ตัวละครทั้งหมดของทุกไอดีที่ผูกกับ IP นี้ (ใช้ตอบ /accounts)</summary>
    public static List<string> EntitiesForIp(string remoteIp)
    {
        var result = new List<string>();
        foreach (string id in BoundIds(remoteIp))
        {
            Record rec = Find(id);
            if (rec?.EntityIds == null)
            {
                continue;
            }
            foreach (string entityId in rec.EntityIds)
            {
                if (!result.Contains(entityId))
                {
                    result.Add(entityId);
                }
            }
        }
        return result;
    }

    /// <summary>จำนวนไอดีที่สมัครไว้ทั้งหมด (หน้าแอดมิน)</summary>
    public static int Count()
    {
        string dir = Path.Combine(SaveStore.Root, "ids");
        return Directory.Exists(dir) ? Directory.GetFiles(dir, "*.json").Length : 0;
    }
}
