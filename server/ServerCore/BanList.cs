using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Durango.Utils;
using Newtonsoft.Json;

namespace DurangoServer.Core;

// ============================================================================
// BanList — รายชื่อคนที่ห้ามเข้าเซิร์ฟ (ถาวรหรือมีวันหมดอายุ)
//
// [4 ก.ย. 2026] เดิมมีแค่ /admin/players/kick ซึ่ง "ตัดการเชื่อมต่อ" เฉย ๆ —
// คนก่อกวนกด Connect ใหม่ก็กลับเข้ามาได้ทันที ทางเดียวที่เคยได้ผลคือรีสตาร์ตทั้งเกาะ
// (ซึ่งเตะคนอื่นออกไปด้วยทั้งหมด)
//
// ไฟล์: data/bans.json — อยู่ในโฟลเดอร์ data ที่ทุกเกาะใช้ร่วมกัน ⇒ **แบนทีเดียวติดทุกเกาะ**
// (ตั้งใจ: คนที่โดนแบนไม่ควรหนีไปเล่นอีกเกาะได้เฉย ๆ)
//
// เก็บทั้ง entity id และชื่อ: id คือของจริงที่เชื่อถือได้ ส่วนชื่อไว้ให้คนอ่านรู้ว่าใคร
// เวลาตรวจใช้ id เป็นหลัก และเทียบชื่อด้วยเผื่อคนสร้างตัวใหม่ชื่อเดิมมาก่อกวนซ้ำ
// ============================================================================

public sealed class BanEntry
{
    public string EntityId { get; set; }
    public string Name { get; set; }
    public string Reason { get; set; }
    /// <summary>unix time ที่แบน</summary>
    public double At { get; set; }
    /// <summary>unix time ที่หมดอายุ — 0 = ถาวร</summary>
    public double Until { get; set; }
    public string By { get; set; }

    public bool Expired(double now) => Until > 0 && Until <= now;
}

public static class BanList
{
    private static readonly object _lock = new object();
    private static List<BanEntry> _bans = new List<BanEntry>();
    private static string _path;

    private sealed class BanFile
    {
        public List<BanEntry> Bans { get; set; }
    }

    public static void Load(string dataDirectory)
    {
        if (string.IsNullOrEmpty(dataDirectory)) { return; }
        _path = Path.Combine(dataDirectory, "bans.json");
        try
        {
            if (!File.Exists(_path))
            {
                lock (_lock) { _bans = new List<BanEntry>(); }
                return;
            }
            BanFile file = JsonConvert.DeserializeObject<BanFile>(File.ReadAllText(_path));
            lock (_lock) { _bans = file?.Bans ?? new List<BanEntry>(); }
            Console.WriteLine("[ban] โหลดรายชื่อแบน {0} คน จาก {1}", _bans.Count, _path);
        }
        catch (Exception e)
        {
            // อ่านไม่ได้ = ถือว่าไม่มีใครโดนแบน ดีกว่าเซิร์ฟไม่ขึ้น
            Console.WriteLine("[ban] อ่าน {0} ไม่ได้ ({1}) — ถือว่ายังไม่มีใครถูกแบน", _path, e.Message);
            lock (_lock) { _bans = new List<BanEntry>(); }
        }
    }

    private static void SaveLocked()
    {
        if (string.IsNullOrEmpty(_path)) { return; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            File.WriteAllText(_path, JsonConvert.SerializeObject(new BanFile { Bans = _bans }, Formatting.Indented));
        }
        catch (Exception e)
        {
            Console.WriteLine("[ban] เขียน {0} ไม่สำเร็จ: {1}", _path, e.Message);
        }
    }

    /// <summary>รายชื่อที่ยังมีผลอยู่ (ตัดอันหมดอายุออกให้แล้ว)</summary>
    public static BanEntry[] Active()
    {
        double now = Times.UnixTimeNow();
        lock (_lock)
        {
            int before = _bans.Count;
            _bans.RemoveAll(b => b.Expired(now));
            if (_bans.Count != before) { SaveLocked(); }
            return _bans.ToArray();
        }
    }

    /// <summary>โดนแบนอยู่ไหม — คืนเหตุผลถ้าโดน (null = เข้าได้)</summary>
    public static string CheckBanned(string entityId, string name)
    {
        double now = Times.UnixTimeNow();
        lock (_lock)
        {
            foreach (BanEntry b in _bans)
            {
                if (b.Expired(now)) { continue; }
                bool byId = !string.IsNullOrEmpty(entityId)
                    && string.Equals(b.EntityId, entityId, StringComparison.OrdinalIgnoreCase);
                bool byName = !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(b.Name)
                    && string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase);
                if (!byId && !byName) { continue; }
                string until = b.Until > 0
                    ? DateTimeOffset.FromUnixTimeSeconds((long)b.Until).ToLocalTime().ToString("d MMM HH:mm")
                    : "ถาวร";
                return string.IsNullOrWhiteSpace(b.Reason)
                    ? $"ถูกระงับการเข้าเล่น ({until})"
                    : $"ถูกระงับการเข้าเล่น: {b.Reason} ({until})";
            }
        }
        return null;
    }

    /// <summary>เพิ่ม/ต่ออายุการแบน — hours &lt;= 0 = ถาวร</summary>
    public static BanEntry Add(string entityId, string name, string reason, double hours, string by)
    {
        double now = Times.UnixTimeNow();
        var entry = new BanEntry
        {
            EntityId = entityId ?? "",
            Name = name ?? "",
            Reason = reason ?? "",
            At = now,
            Until = hours > 0 ? now + hours * 3600.0 : 0,
            By = by ?? "admin"
        };
        lock (_lock)
        {
            _bans.RemoveAll(b =>
                (!string.IsNullOrEmpty(entry.EntityId)
                    && string.Equals(b.EntityId, entry.EntityId, StringComparison.OrdinalIgnoreCase))
                || (!string.IsNullOrEmpty(entry.Name)
                    && string.Equals(b.Name, entry.Name, StringComparison.OrdinalIgnoreCase)));
            _bans.Add(entry);
            SaveLocked();
        }
        Console.WriteLine("[ban] แบน {0} ({1}) — {2} · หมดอายุ {3}",
            entry.Name, entry.EntityId, entry.Reason, entry.Until > 0 ? entry.Until.ToString("F0") : "ถาวร");
        return entry;
    }

    /// <summary>ปลดแบนตาม entity id หรือชื่อ — คืนจำนวนที่ถูกลบ</summary>
    public static int Remove(string idOrName)
    {
        if (string.IsNullOrWhiteSpace(idOrName)) { return 0; }
        lock (_lock)
        {
            int n = _bans.RemoveAll(b =>
                string.Equals(b.EntityId, idOrName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(b.Name, idOrName, StringComparison.OrdinalIgnoreCase));
            if (n > 0) { SaveLocked(); }
            Console.WriteLine("[ban] ปลดแบน '{0}' — {1} รายการ", idOrName, n);
            return n;
        }
    }
}
