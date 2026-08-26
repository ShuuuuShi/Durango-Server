using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace DurangoUpdater;

/// <summary>
/// ดู DurangoUpdater.csproj สำหรับหลักการทั้งหมด — สรุปสั้น ๆ: เช็คเวอร์ชัน → ถ้ามีใหม่กว่า
/// โหลด+ตรวจ hash+แตกไฟล์ลงโฟลเดอร์ชั่วคราวก่อนเสมอ → สลับเข้าโฟลเดอร์เกมจริงด้วย robocopy /MIR
/// เฉพาะตอนไฟล์ใหม่ครบสมบูรณ์แล้วเท่านั้น → เปิดเกม
///
/// ทุกขั้นตอนที่พลาดได้ (ไม่มีเน็ต, manifest เสีย, โหลดขาด, hash ไม่ตรง, แตกไฟล์ไม่ครบ) ต้อง
/// "ข้ามการอัปเดตแล้วเปิดเกมเวอร์ชันเดิมต่อ" ไม่ใช่ทำให้เปิดเกมไม่ได้เลย — ห้ามให้ exception หลุดจนเกม
/// ไม่เปิดเด็ดขาด
/// </summary>
internal static class Program
{
    private const string DefaultManifestUrl =
        "https://github.com/SuperCodeTH/Durango-TH-Client/releases/latest/download/manifest.json";

    private static async Task<int> Main(string[] args)
    {
        string gameDir = AppContext.BaseDirectory.TrimEnd('\\', '/');
        Console.WriteLine("Durango Updater");
        Console.WriteLine("โฟลเดอร์เกม: " + gameDir);

        try
        {
            await RunUpdateCheck(gameDir);
        }
        catch (Exception e)
        {
            // กันเหนียวชั้นนอกสุด — ไม่ว่าจะพังตรงไหนในขั้นตอนอัปเดต ต้องไปเปิดเกมต่อได้เสมอ
            Console.WriteLine("[อัปเดต] ข้ามไป (ผิดพลาด: " + e.Message + ")");
        }

        LaunchGame(gameDir);
        return 0;
    }

    private static async Task RunUpdateCheck(string gameDir)
    {
        if (IsGameRunning())
        {
            Console.WriteLine("[อัปเดต] เกมกำลังเปิดอยู่แล้ว ข้ามการเช็คอัปเดตรอบนี้");
            return;
        }

        string localVersion = ReadLocalVersion(gameDir);
        string manifestUrl = ReadManifestUrl(gameDir);
        Console.WriteLine($"[อัปเดต] เวอร์ชันปัจจุบัน: {localVersion}");

        using HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("DurangoUpdater/1.0");

        Manifest? manifest;
        try
        {
            string json = await http.GetStringAsync(manifestUrl);
            manifest = JsonSerializer.Deserialize<Manifest>(json, JsonOpts);
        }
        catch (Exception e)
        {
            Console.WriteLine("[อัปเดต] เช็คอัปเดตไม่ได้ (" + e.Message + ") — เล่นเวอร์ชันปัจจุบันต่อ");
            return;
        }

        if (manifest == null || string.IsNullOrEmpty(manifest.Version) || string.IsNullOrEmpty(manifest.ZipUrl))
        {
            Console.WriteLine("[อัปเดต] manifest ไม่สมบูรณ์ — ข้าม");
            return;
        }

        if (manifest.Version == localVersion)
        {
            Console.WriteLine("[อัปเดต] เป็นเวอร์ชันล่าสุดอยู่แล้ว");
            return;
        }

        Console.WriteLine($"[อัปเดต] พบเวอร์ชันใหม่: {manifest.Version} — กำลังดาวน์โหลด...");
        if (!string.IsNullOrEmpty(manifest.Notes))
        {
            Console.WriteLine("[อัปเดต] " + manifest.Notes);
        }

        string tempRoot = Path.Combine(Path.GetTempPath(), "durango-update-" + Guid.NewGuid().ToString("N"));
        string zipPath = tempRoot + ".zip";
        string extractDir = tempRoot + "-extract";

        try
        {
            // ── 1) โหลดไฟล์ทั้งก้อนลงเครื่องชั่วคราวก่อน (ยังไม่แตะโฟลเดอร์เกมจริงเลย) ──
            await using (FileStream fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            using (Stream dl = await http.GetStreamAsync(manifest.ZipUrl))
            {
                await dl.CopyToAsync(fs);
            }

            // ── 2) ตรวจ SHA256 — ไม่ตรง = หยุดทันที ไม่แตะโฟลเดอร์เกมจริงเลย ──
            if (!string.IsNullOrEmpty(manifest.Sha256))
            {
                string actual = await ComputeSha256(zipPath);
                if (!string.Equals(actual, manifest.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[อัปเดต] ไฟล์ที่โหลดมาไม่ตรง SHA256 ที่คาดไว้ — ยกเลิก (อาจโหลดขาด/เน็ตมีปัญหา)");
                    return;
                }
                Console.WriteLine("[อัปเดต] ตรวจ SHA256 ผ่าน");
            }

            // ── 3) แตกไฟล์ลงโฟลเดอร์ชั่วคราว (ยังไม่แตะโฟลเดอร์เกมจริง) ──
            Directory.CreateDirectory(extractDir);
            ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);

            // 🐛 พบจริงตอนเทส: zip ที่แจกจริง (package-game.ps1) มีโฟลเดอร์ห่อชั้นนอกอีกชั้น
            // (เช่น "DurangoTH\DurangoV2.exe" ไม่ใช่ "DurangoV2.exe" ตรง root) — เช็คแบบเดิมที่ดูแค่
            // root ตรง ๆ จะไม่เจอไฟล์เลย "ทุกครั้ง" แล้วยกเลิกอัปเดตแบบเงียบ ๆ (fallback ไปเปิดเวอร์ชันเดิม
            // โดยไม่มี error ให้เห็น) ⇒ อัปเดตจริงไม่เคยทำงานเลยสักครั้งโดยไม่มีใครรู้ตัว
            // แก้โดยหา DurangoV2.exe แบบไล่ลึกลงไปในโฟลเดอร์ที่แตกออกมา แล้วใช้โฟลเดอร์ที่เจอไฟล์นั้นเป็น
            // root จริงสำหรับ mirror แทน — รองรับทั้ง zip ที่แบนราบและ zip ที่ห่อโฟลเดอร์ไว้ชั้นเดียว
            string? foundExe = Directory.GetFiles(extractDir, "DurangoV2.exe", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (foundExe == null)
            {
                Console.WriteLine("[อัปเดต] ไฟล์ที่แตกออกมาไม่สมบูรณ์ (ไม่เจอ DurangoV2.exe) — ยกเลิก");
                return;
            }
            string sourceRoot = Path.GetDirectoryName(foundExe)!;
            Console.WriteLine("[อัปเดต] แตกไฟล์เรียบร้อย กำลังติดตั้ง...");

            // ── 4) สลับเข้าโฟลเดอร์เกมจริง — จุดเดียวที่แตะไฟล์จริง ทำก็ต่อเมื่อของใหม่ครบแล้วเท่านั้น
            //    เว้น AppData/AppData2 (เซฟตัวละครในเครื่อง) + server.txt + update-manifest-url.txt
            //    (ค่าที่ผู้เล่น/ผู้ดูแลตั้งเองในเครื่องนี้ ไม่ใช่ของที่มากับชุดแจก) ไม่ให้โดนทับ/ลบ ──
            RunRobocopyMirror(sourceRoot, gameDir);

            WriteLocalVersion(gameDir, manifest.Version);
            Console.WriteLine("[อัปเดต] อัปเดตเป็นเวอร์ชัน " + manifest.Version + " เรียบร้อยแล้ว");
        }
        finally
        {
            TryDelete(zipPath);
            TryDeleteDir(extractDir);
        }
    }

    private static void RunRobocopyMirror(string source, string dest)
    {
        // /MIR ทำให้ dest มีของเหมือน source เป๊ะ แต่ /XD /XF กันโฟลเดอร์/ไฟล์ของผู้เล่นเองไม่ให้โดนลบ
        // แม้จะไม่มีอยู่ใน source (ชุดแจกไม่เคยมี AppData/server.txt อยู่แล้ว — ดู package-game.ps1)
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "robocopy",
            ArgumentList =
            {
                source, dest, "/MIR", "/NFL", "/NDL", "/NJH", "/NJS", "/R:2", "/W:1",
                "/XD", Path.Combine(dest, "AppData"), Path.Combine(dest, "AppData2"),
                "/XF", "server.txt", "update-manifest-url.txt", "version.txt", "game.log"
            },
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using Process? proc = Process.Start(psi);
        proc?.WaitForExit();
        // robocopy: exit code 0-7 = สำเร็จในระดับต่าง ๆ (ไม่ใช่ error), >=8 ถึงจะถือว่าพัง
        if (proc != null && proc.ExitCode >= 8)
        {
            throw new IOException("robocopy ล้มเหลว (exit code " + proc.ExitCode + ")");
        }
    }

    private static bool IsGameRunning()
    {
        return Process.GetProcessesByName("DurangoV2").Length > 0;
    }

    private static void LaunchGame(string gameDir)
    {
        string exePath = Path.Combine(gameDir, "DurangoV2.exe");
        if (!File.Exists(exePath))
        {
            Console.WriteLine("ไม่พบ DurangoV2.exe — ตรวจว่าแตกไฟล์ zip ครบทั้งโฟลเดอร์แล้วหรือยัง");
            Console.WriteLine("(กด Enter เพื่อปิดหน้าต่างนี้)");
            Console.ReadLine();
            return;
        }

        string server = ReadServerTarget(gameDir);
        Console.WriteLine();
        Console.WriteLine("  Durango TH");
        Console.WriteLine("  server : " + server);
        Console.WriteLine();

        // UseShellExecute ต้องเป็น false ถึงจะตั้ง EnvironmentVariables ได้ (.NET บังคับคู่กันแบบนี้ —
        // ลองแล้วจริงตอนเทส: true คู่กับ EnvironmentVariables โยน InvalidOperationException ทันที)
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "-force-d3d11 -screen-fullscreen 0 -screen-width 1600 -screen-height 900 " +
                        "-logFile \"" + Path.Combine(gameDir, "game.log") + "\"",
            WorkingDirectory = gameDir,
            UseShellExecute = false
        };
        psi.EnvironmentVariables["DURANGO_AUTOCONNECT"] = server;
        Process.Start(psi);
    }

    private static string ReadServerTarget(string gameDir)
    {
        string path = Path.Combine(gameDir, "server.txt");
        if (!File.Exists(path)) return "127.0.0.1";
        foreach (string line in File.ReadAllLines(path))
        {
            string t = line.Trim();
            if (t.Length == 0 || t.StartsWith("#")) continue;
            return t;
        }
        return "127.0.0.1";
    }

    private static string ReadLocalVersion(string gameDir)
    {
        string path = Path.Combine(gameDir, "version.txt");
        return File.Exists(path) ? File.ReadAllText(path).Trim() : "0";
    }

    private static void WriteLocalVersion(string gameDir, string version)
    {
        File.WriteAllText(Path.Combine(gameDir, "version.txt"), version);
    }

    private static string ReadManifestUrl(string gameDir)
    {
        string path = Path.Combine(gameDir, "update-manifest-url.txt");
        if (!File.Exists(path)) return DefaultManifestUrl;
        foreach (string line in File.ReadAllLines(path))
        {
            string t = line.Trim();
            if (t.Length == 0 || t.StartsWith("#")) continue;
            return t;
        }
        return DefaultManifestUrl;
    }

    private static async Task<string> ComputeSha256(string filePath)
    {
        await using FileStream fs = File.OpenRead(filePath);
        byte[] hash = await SHA256.HashDataAsync(fs);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { /* เก็บกวาดไม่ได้ก็ไม่เป็นไร */ }
    }

    private static void TryDeleteDir(string path)
    {
        try { if (Directory.Exists(path)) Directory.Delete(path, recursive: true); } catch { /* เก็บกวาดไม่ได้ก็ไม่เป็นไร */ }
    }

    private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class Manifest
    {
        public string? Version { get; set; }
        public string? ZipUrl { get; set; }
        public string? Sha256 { get; set; }
        public string? Notes { get; set; }
    }
}
