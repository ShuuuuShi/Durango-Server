using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DurangoServer.Core;

// ============================================================================
// LiveLog — เก็บบรรทัด console ล่าสุดไว้ใน memory ให้ admin web panel poll อ่านได้
//
// ทำไมต้องมี: server ปกติพิมพ์ log ด้วย Console.WriteLine ทิ้งลงหน้าต่าง console เฉย ๆ
// (ดู Program.cs, ServerCore/*.cs) ไม่มีที่ไหนเก็บไว้ให้อ่านย้อนหลัง/จากที่อื่นได้เลย
// ไม่ว่าจะเปิดเซิร์ฟด้วยวิธีไหน (dotnet run ตรง ๆ, Start-Process หน้าต่างแยกจาก tools/menu.ps1
// ที่ไม่ redirect ไฟล์) — ตัวนี้ครอบ Console.Out ไว้ตั้งแต่ต้น Main() แล้วเก็บสำเนาบรรทัดล่าสุด
// (สูงสุด MaxLines บรรทัด) ไว้ในหน่วยความจำ, admin panel เรียก Gateway /admin/log?after=N
// มาอ่านได้แบบ real-time โดยไม่ต้องยุ่งกับไฟล์ log เลย
// ============================================================================

public static class LiveLog
{
    private const int MaxLines = 2000;

    private static readonly object _lock = new object();
    private static readonly List<string> _lines = new List<string>();

    /// <summary>จำนวนบรรทัดที่เคยเพิ่มเข้ามาทั้งหมดตั้งแต่เปิดเซิร์ฟ (ใช้เป็น cursor ให้ client poll ต่อจากเดิมได้)</summary>
    private static long _totalLines;

    public static void Append(string line)
    {
        if (line == null)
        {
            return;
        }
        lock (_lock)
        {
            _lines.Add(line);
            _totalLines++;
            if (_lines.Count > MaxLines)
            {
                _lines.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// คืนบรรทัดทั้งหมดตั้งแต่ตัวนับ <paramref name="after"/> (ค่าที่ได้จาก NextCursor รอบก่อน, 0 = ตั้งแต่ต้น
    /// เท่าที่ยังเหลืออยู่ใน buffer) พร้อม cursor ตัวใหม่ให้ใช้ในการ poll รอบถัดไป
    /// </summary>
    public static (string[] Lines, long NextCursor) GetSince(long after)
    {
        lock (_lock)
        {
            long firstKept = _totalLines - _lines.Count; // ตัวนับของ _lines[0] (0 ถ้ายังไม่เคยตัดทิ้ง)
            long skip = Math.Max(0, after - firstKept);
            if (skip >= _lines.Count)
            {
                return (Array.Empty<string>(), _totalLines);
            }
            int skipInt = (int)Math.Min(skip, int.MaxValue);
            string[] slice = _lines.GetRange(skipInt, _lines.Count - skipInt).ToArray();
            return (slice, _totalLines);
        }
    }
}

/// <summary>
/// ครอบ TextWriter เดิมของ Console — พิมพ์ลง console ตามปกติ **และ** เก็บสำเนาไว้ใน LiveLog
/// ดักเฉพาะ WriteLine(string) เพราะโค้ดทั้งโปรเจกต์ใช้ Console.WriteLine("...") / Console.WriteLine($"...")
/// เป็นหลัก (ครอบคลุมเกือบทุกบรรทัด log จริง) — Write อื่น ๆ ส่งต่อไปที่ writer เดิมตามปกติ ไม่เก็บ
/// </summary>
public sealed class LiveLogTextWriter : TextWriter
{
    private readonly TextWriter _inner;

    public LiveLogTextWriter(TextWriter inner)
    {
        _inner = inner;
    }

    public override Encoding Encoding => _inner.Encoding;

    public override void Write(char value) => _inner.Write(value);

    public override void Write(string value) => _inner.Write(value);

    public override void WriteLine(string value)
    {
        _inner.WriteLine(value);
        LiveLog.Append(value ?? string.Empty);
    }

    public override void WriteLine()
    {
        _inner.WriteLine();
    }

    public override void Flush() => _inner.Flush();
}
