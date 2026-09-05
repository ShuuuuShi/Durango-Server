using System;
using System.Collections.Generic;
using System.IO;

namespace DurangoServer.Core;

// ============================================================================
// SaveBackup — แบ็กอัพโฟลเดอร์เซฟตามรอบเวลา (4 ก.ย. 2026 · เจ้าของเซิร์ฟสั่งเพิ่ม)
//
// เดิมไม่มีแบ็กอัพอัตโนมัติเลย มีแต่แบ็กอัพที่ทำมือตอนจะ deploy
// ⇒ เซฟพัง/แก้ผิด = ย้อนไม่ได้ (เคยต้องกู้จาก /opt/durango/backups ที่ทำมือไว้)
//
// ก๊อปทั้งโฟลเดอร์ saves ไปไว้ที่ <BackupDir>/<เกาะ>-YYYYMMDD-HHmmss/
// แล้วลบชุดเก่าที่เกิน BackupKeep ทิ้ง
// ⚠️ ก๊อปหลัง SaveAll เสมอ ไม่งั้นได้ภาพของเก่ากว่าที่ควร
// ============================================================================

public static class SaveBackup
{
    /// <summary>เวลาที่จะแบ็กอัพรอบถัดไป (unix ms ของ Environment.TickCount64 ฝั่งผู้เรียก)</summary>
    private static double _nextBackupAt;

    /// <summary>ตั้งเวลาแบ็กอัพรอบแรก — เรียกครั้งเดียวตอนสตาร์ท</summary>
    public static void Schedule(double nowMs, bool immediately)
    {
        SaveConfig cfg = ServerConfig.Current.Save ?? SaveConfig.Defaults();
        double every = IntervalMs(cfg);
        _nextBackupAt = immediately ? nowMs : nowMs + every;
    }

    private static double IntervalMs(SaveConfig cfg)
    {
        float hours = cfg.BackupIntervalHours > 0f ? cfg.BackupIntervalHours : 4f;
        return hours * 3600.0 * 1000.0;
    }

    /// <summary>ถึงรอบแบ็กอัพหรือยัง — เช็คก่อนเซฟ ไม่งั้นบังคับเซฟทุก tick โดยเปล่าประโยชน์</summary>
    public static bool Due(double nowMs)
    {
        SaveConfig cfg = ServerConfig.Current.Save ?? SaveConfig.Defaults();
        return cfg.BackupEnabled && nowMs >= _nextBackupAt;
    }

    /// <summary>ถึงรอบแล้วก็ทำแบ็กอัพ (เรียกหลัง SaveAll)</summary>
    public static void Tick(double nowMs)
    {
        SaveConfig cfg = ServerConfig.Current.Save ?? SaveConfig.Defaults();
        if (!cfg.BackupEnabled) { return; }
        if (nowMs < _nextBackupAt) { return; }
        _nextBackupAt = nowMs + IntervalMs(cfg);
        RunOnce("ตามรอบเวลา");
    }

    /// <summary>แบ็กอัพเดี๋ยวนี้ — คืน path ที่เขียน (null = ไม่สำเร็จ/ไม่มีอะไรให้แบ็กอัพ)</summary>
    public static string RunOnce(string reason)
    {
        SaveConfig cfg = ServerConfig.Current.Save ?? SaveConfig.Defaults();
        string root = SaveStore.Root;
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
        {
            return null;
        }
        string baseDir = string.IsNullOrWhiteSpace(cfg.BackupDir)
            ? Path.Combine(root, "backups")
            : cfg.BackupDir;
        string island = IslandRegistry.Current?.Id ?? "single";
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string target = Path.Combine(baseDir, island + "-" + stamp);
        try
        {
            Directory.CreateDirectory(baseDir);
            int files = CopyTree(root, target, Path.GetFullPath(baseDir));
            if (files == 0)
            {
                // ไม่มีไฟล์เลย = ไม่ต้องทิ้งโฟลเดอร์ว่างไว้ให้รก
                try { Directory.Delete(target, true); } catch { }
                return null;
            }
            Console.WriteLine("[backup] {0}: {1} ไฟล์ → {2}", reason, files, target);
            Prune(baseDir, island, cfg.BackupKeep);
            return target;
        }
        catch (Exception e)
        {
            Console.WriteLine("[backup] ล้มเหลว ({0}) — เซิร์ฟทำงานต่อตามปกติ", e.Message);
            return null;
        }
    }

    /// <summary>ก๊อปทั้งต้นไม้ ยกเว้นโฟลเดอร์แบ็กอัพเอง (ไม่งั้นแบ็กอัพซ้อนแบ็กอัพจนดิสก์เต็ม)</summary>
    private static int CopyTree(string sourceDir, string destDir, string skipFullPath)
    {
        if (string.Equals(Path.GetFullPath(sourceDir), skipFullPath, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }
        Directory.CreateDirectory(destDir);
        int count = 0;
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            try
            {
                File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
                count++;
            }
            catch (IOException)
            {
                // ไฟล์กำลังถูกเขียนอยู่พอดี — ข้ามไฟล์นั้น ไม่ล้มทั้งรอบ
            }
        }
        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            count += CopyTree(dir, Path.Combine(destDir, Path.GetFileName(dir)), skipFullPath);
        }
        return count;
    }

    /// <summary>ลบแบ็กอัพเก่าของเกาะนี้ที่เกินจำนวนที่ให้เก็บ</summary>
    private static void Prune(string baseDir, string island, int keep)
    {
        if (keep <= 0) { return; }
        var mine = new List<string>();
        foreach (string dir in Directory.GetDirectories(baseDir))
        {
            if (Path.GetFileName(dir).StartsWith(island + "-", StringComparison.Ordinal))
            {
                mine.Add(dir);
            }
        }
        if (mine.Count <= keep) { return; }
        // ชื่อโฟลเดอร์ลงท้ายด้วย timestamp อยู่แล้ว เรียงตามชื่อ = เรียงตามเวลา
        mine.Sort(StringComparer.Ordinal);
        for (int i = 0; i < mine.Count - keep; i++)
        {
            try
            {
                Directory.Delete(mine[i], true);
                Console.WriteLine("[backup] ลบชุดเก่า {0}", Path.GetFileName(mine[i]));
            }
            catch (Exception e)
            {
                Console.WriteLine("[backup] ลบ {0} ไม่ได้: {1}", Path.GetFileName(mine[i]), e.Message);
            }
        }
    }
}
