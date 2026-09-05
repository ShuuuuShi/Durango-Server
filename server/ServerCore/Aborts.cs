using System;
using System.IO;
using System.Runtime.CompilerServices;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// [สร้างใหม่ 30 ส.ค. 2026] ตัวช่วยสร้าง <see cref="Abort"/> — แก้ปัญหาใหญ่ 2 อย่างพร้อมกัน
///
/// 1) **client พังเพราะ Text เป็น null**
///    เดิมทั้งเซิร์ฟส่ง `default(Abort)` (356 จุด) ซึ่ง `Abort.Text` เป็น null
///    ฝั่ง client `GameManager.DefaultAbortHandler` เรียก `LimitText(null)` แล้ว
///    **NullReferenceException** ทันที (ยืนยันจาก log จริง) ⇒ ผู้เล่นถูกเตะออกแบบไม่รู้สาเหตุ
///    และสถานะเกมเพี้ยนต่อเนื่อง
///
/// 2) **ไล่บั๊กไม่ได้ว่าใครเป็นคนเตะ**
///    `default(Abort)` เหมือนกันหมดทุกจุด ดู log แล้วไม่รู้ว่ามาจาก handler ไหน
///    ตอนนี้พิมพ์ `[abort] ไฟล์.เมธอด: เหตุผล` ให้ทุกครั้ง
/// </summary>
public static class Aborts
{
    /// <summary>สร้าง Abort พร้อมข้อความและ log ว่ามาจากไหน (ใช้แทน <c>default(Abort)</c> ทุกที่)</summary>
    public static Abort Reason(
        string why = "",
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        string where = Path.GetFileNameWithoutExtension(file) + "." + caller + ":" + line;
        Console.WriteLine($"[abort] {where}" + (string.IsNullOrEmpty(why) ? "" : " — " + why));

        Abort a = default;
        // ต้องไม่เป็น null เด็ดขาด ไม่งั้น client NRE (ดูหัวข้อ 1 ด้านบน)
        a.Text = string.IsNullOrEmpty(why) ? where : why;
        return a;
    }
}
