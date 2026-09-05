using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DurangoServer.Core;

/// <summary>
/// ดัก exception ที่หลุดจากเธรดที่ไม่ใช่ main loop แล้วเขียนลงไฟล์ก่อนโปรเซสตาย
/// ไม่มีตัวนี้ = เซิร์ฟหายไปทั้งใบโดยไม่ทิ้งร่องรอย (ดู docs/Plan/STABILITY-REVIEW.md P0)
/// </summary>
public static class CrashLog
{
    private static readonly object _lock = new object();
    private static bool _installed;

    public static void Install()
    {
        if (_installed)
        {
            return;
        }
        _installed = true;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Write("UnhandledException", e.ExceptionObject as Exception ?? e.ExceptionObject);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Write("UnobservedTaskException", e.Exception);
            e.SetObserved();
        };
    }

    public static string Write(string kind, object error)
    {
        string text = Format(kind, error);
        string path = NextPath();
        lock (_lock)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, text, Encoding.UTF8);
            }
            catch (Exception io)
            {
                Console.WriteLine("[crash] เขียนไฟล์ crash ไม่สำเร็จ: " + io.Message);
            }
        }
        Console.WriteLine(text);
        LiveLog.Append(text.Replace("\r\n", "\n"));
        return path;
    }

    private static string NextPath()
    {
        string dir = Path.Combine(AppContext.BaseDirectory, "logs");
        string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
        return Path.Combine(dir, $"crash-{stamp}.txt");
    }

    private static string Format(string kind, object error)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== DurangoServer crash ===");
        sb.AppendLine("utc: " + DateTime.UtcNow.ToString("o"));
        sb.AppendLine("kind: " + kind);
        if (error is Exception ex)
        {
            sb.AppendLine(ex.ToString());
        }
        else
        {
            sb.AppendLine(error?.ToString() ?? "(null)");
        }
        return sb.ToString();
    }
}
