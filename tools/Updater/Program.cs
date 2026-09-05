using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

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
        "https://github.com/ShuuuuShi/Durango-TH-Client/releases/latest/download/manifest.json";

    private static ProgressForm? _progress;
    /// <summary>ถูกเรียกจากในเกม — เกมยังล็อก DLL อยู่ ต้องรอให้ปิดก่อนค่อยแพตช์</summary>
    private static bool _fromGame;
    private static string? _manifestUrlOverride;

    [STAThread]
    private static void Main(string[] args)
    {
        ParseArgs(args);
        ApplicationConfiguration.Initialize();
        Application.Run(new UpdaterApplicationContext());
    }

    private static void ParseArgs(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--from-game", StringComparison.OrdinalIgnoreCase))
            {
                _fromGame = true;
            }
            else if (string.Equals(args[i], "--manifest-url", StringComparison.OrdinalIgnoreCase)
                     && i + 1 < args.Length)
            {
                _manifestUrlOverride = args[++i];
            }
        }
    }

    private sealed class UpdaterApplicationContext : ApplicationContext
    {
        private readonly string _gameDir;

        public UpdaterApplicationContext()
        {
            _gameDir = AppContext.BaseDirectory.TrimEnd('\\', '/');
            _progress = new ProgressForm();
            _progress.Show();
            _ = RunAsync();
        }

        private async Task RunAsync()
        {
            Console.WriteLine("Durango Updater");
            Console.WriteLine("โฟลเดอร์เกม: " + _gameDir);

            try
            {
                if (_fromGame || IsGameRunning())
                {
                    _progress?.SetBusy("รอเกมปิดก่อนติดตั้งแพท...");
                    await WaitForGameExit();
                }
                await RunUpdateCheck(_gameDir);
            }
            catch (Exception e)
            {
                // กันเหนียวชั้นนอกสุด — ไม่ว่าจะพังตรงไหนในขั้นตอนอัปเดต ต้องไปเปิดเกมต่อได้เสมอ
                Console.WriteLine("[อัปเดต] ข้ามไป (ผิดพลาด: " + e.Message + ")");
                _progress?.SetStatus("อัปเดตไม่สำเร็จ — กำลังเปิดเกมเวอร์ชันเดิม...");
                await Task.Delay(700);
            }

            _progress?.Close();
            LaunchGame(_gameDir);
            ExitThread();
        }
    }

    private static async Task WaitForGameExit()
    {
        for (int i = 0; i < 120; i++)
        {
            if (!IsGameRunning())
            {
                return;
            }
            await Task.Delay(500);
        }
        Console.WriteLine("[อัปเดต] เกมยังไม่ปิดภายใน 60 วินาที — ลองแพตช์ต่อ (ถ้า DLL ถูกล็อกจะข้าม)");
    }

    private static void SetProgressBusy(string message) => _progress?.SetBusy(message);

    private static async Task RunUpdateCheck(string gameDir)
    {
        SetProgressBusy("กำลังตรวจสอบแพท...");
        if (IsGameRunning())
        {
            SetProgressBusy("รอเกมปิดก่อนติดตั้งแพท...");
            await WaitForGameExit();
        }

        string localVersion = ReadLocalVersion(gameDir);
        Console.WriteLine($"[อัปเดต] เวอร์ชันปัจจุบัน: {localVersion}");

        Manifest? manifest = await FetchManifestFromKnownSources(gameDir);
        manifest?.Normalize();

        if (manifest == null || string.IsNullOrEmpty(manifest.Version))
        {
            Console.WriteLine("[อัปเดต] ไม่พบรายการแพท — เปิดเกมเวอร์ชันเดิมต่อ");
            _progress?.SetStatus("ไม่พบแพทใหม่ — กำลังเปิดเกม...");
            await Task.Delay(500);
            return;
        }

        bool hasPatches = manifest.Files != null && manifest.Files.Count > 0;
        if (!hasPatches && string.IsNullOrEmpty(manifest.ZipUrl))
        {
            Console.WriteLine("[อัปเดต] manifest ไม่มีรายการแพตช์และไม่มี zip — ข้าม");
            return;
        }

        if (!string.IsNullOrEmpty(manifest.Notes))
        {
            Console.WriteLine("[อัปเดต] " + manifest.Notes);
        }

        if (hasPatches)
        {
            await ApplyDllPatches(gameDir, manifest, localVersion);
            return;
        }

        if (manifest.Version == localVersion)
        {
            Console.WriteLine("[อัปเดต] เป็นเวอร์ชันล่าสุดอยู่แล้ว");
            return;
        }

        Console.WriteLine($"[อัปเดต] พบเวอร์ชันใหม่: {manifest.Version} — โหลดชุดเต็ม (ไม่มีรายการ DLL)");
        await ApplyFullZip(gameDir, manifest);
    }

    /// <summary>
    /// โหลดเฉพาะไฟล์ที่ hash ไม่ตรง (ปกติคือ Assembly-CSharp.dll ไม่กี่ MB)
    /// ดาวน์โหลดลง temp + ตรวจ SHA256 ครบก่อน แล้วค่อยทับของจริง
    /// </summary>
    private static async Task ApplyDllPatches(string gameDir, Manifest manifest, string localVersion)
    {
        List<PatchFile> needed = new List<PatchFile>();
        foreach (PatchFile file in manifest.Files!)
        {
            if (!IsSafeRelPath(file.Path) || string.IsNullOrWhiteSpace(file.Url) || string.IsNullOrWhiteSpace(file.Sha256))
            {
                Console.WriteLine("[อัปเดต] ข้ามรายการแพตช์ที่ไม่ถูกต้อง: " + (file.Path ?? "?"));
                continue;
            }
            if (file.Path!.EndsWith("DurangoUpdater.exe", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            string dest = Path.Combine(gameDir, file.Path.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(dest))
            {
                string localHash = await ComputeSha256(dest);
                if (string.Equals(localHash, file.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }
            needed.Add(file);
        }

        if (needed.Count == 0 && manifest.Version == localVersion)
        {
            Console.WriteLine("[อัปเดต] DLL ตรงกับเซิร์ฟอยู่แล้ว");
            return;
        }

        if (needed.Count == 0)
        {
            // 🐛 [แก้เอง] 1 ก.ย. 2026 — เดิมปรับเลขเวอร์ชันทันทีที่ "ไม่มีไฟล์ต้องโหลด"
            //    แต่ manifest บางแหล่ง (เช่น /launcher/version ของเซิร์ฟ) ส่งมาแค่ version+notes
            //    **ไม่มีรายการไฟล์เลย** ⇒ needed ว่างเพราะไม่มีอะไรให้เทียบ ไม่ใช่เพราะไฟล์ครบ
            //    ผลคือเขียน version.txt เป็นเลขใหม่ทั้งที่ไฟล์ยังเป็นของเก่า
            //    ⇒ พอแพตช์นั้นออกมาจริง ผู้เล่นจะไม่ได้รับ เพราะเลขตรงกันไปแล้ว
            //    (เจอจริงตอนเทส: เซิร์ฟบอก 0.1.2 → เขียน 0.1.2 โดยไม่โหลดอะไรเลย)
            if (manifest.Files!.Count == 0)
            {
                Console.WriteLine("[อัปเดต] manifest ไม่มีรายการไฟล์ — ไม่ขยับเลขเวอร์ชัน (กันเลขวิ่งไปก่อนไฟล์)");
                return;
            }
            WriteLocalVersion(gameDir, manifest.Version!);
            Console.WriteLine("[อัปเดต] ไฟล์ครบแล้ว ปรับเลขเวอร์ชันเป็น " + manifest.Version);
            return;
        }

        Console.WriteLine($"[อัปเดต] ต้องแพตช์ {needed.Count} ไฟล์ (ไม่โหลด zip ทั้งเกม)");
        string stageDir = Path.Combine(Path.GetTempPath(), "durango-patch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stageDir);
        try
        {
            using HttpClient http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("DurangoUpdater/1.1");
            long totalKnown = needed.Sum(f => f.Size > 0 ? f.Size : 0);
            long receivedAll = 0;
            foreach (PatchFile file in needed)
            {
                string staged = Path.Combine(stageDir, Path.GetFileName(file.Path)!);
                SetProgressBusy("กำลังดาวน์โหลด " + Path.GetFileName(file.Path));
                await DownloadToFile(http, file.Url!, staged, file.Size, receivedAll, totalKnown);
                receivedAll += new FileInfo(staged).Length;
                string actual = await ComputeSha256(staged);
                if (!string.Equals(actual, file.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[อัปเดต] hash ไม่ตรง: " + file.Path + " — ยกเลิก ไม่แตะเกมจริง");
                    return;
                }
            }

            SetProgressBusy("กำลังติดตั้งแพท...");
            foreach (PatchFile file in needed)
            {
                string staged = Path.Combine(stageDir, Path.GetFileName(file.Path)!);
                string dest = Path.Combine(gameDir, file.Path!.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                bool copied = false;
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    try
                    {
                        File.Copy(staged, dest, overwrite: true);
                        copied = true;
                        break;
                    }
                    catch (IOException)
                    {
                        await Task.Delay(400);
                    }
                }
                if (!copied)
                {
                    Console.WriteLine("[อัปเดต] วาง " + file.Path + " ไม่ได้ (ไฟล์ถูกล็อก) — ยกเลิก");
                    return;
                }
                Console.WriteLine("[อัปเดต] วาง " + file.Path);
            }
            WriteLocalVersion(gameDir, manifest.Version!);
            Console.WriteLine("[อัปเดต] แพตช์เป็นเวอร์ชัน " + manifest.Version + " แล้ว");
            _progress?.SetComplete("ติดตั้งเสร็จแล้ว — กำลังเปิดเกม...");
            await Task.Delay(500);
        }
        finally
        {
            TryDeleteDir(stageDir);
        }
    }

    private static async Task ApplyFullZip(string gameDir, Manifest manifest)
    {
        string tempRoot = Path.Combine(Path.GetTempPath(), "durango-update-" + Guid.NewGuid().ToString("N"));
        string zipPath = tempRoot + ".zip";
        string extractDir = tempRoot + "-extract";
        try
        {
            SetProgressBusy("กำลังดาวน์โหลดชุดเต็ม...");
            using HttpClient http = new HttpClient { Timeout = TimeSpan.FromHours(1) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("DurangoUpdater/1.1");
            using HttpResponseMessage downloadResponse = await http.GetAsync(
                manifest.ZipUrl, HttpCompletionOption.ResponseHeadersRead);
            downloadResponse.EnsureSuccessStatusCode();
            long totalBytes = downloadResponse.Content.Headers.ContentLength ?? -1L;
            long receivedBytes = 0L;
            byte[] buffer = new byte[1024 * 1024];
            await using (FileStream fs = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
            using (Stream dl = await downloadResponse.Content.ReadAsStreamAsync())
            {
                int read;
                while ((read = await dl.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
                {
                    await fs.WriteAsync(buffer.AsMemory(0, read));
                    receivedBytes += read;
                    _progress?.SetProgress(receivedBytes, totalBytes);
                }
            }
            _progress?.SetComplete("ดาวน์โหลดเสร็จแล้ว — กำลังตรวจสอบไฟล์...");
            if (!string.IsNullOrEmpty(manifest.Sha256))
            {
                string actual = await ComputeSha256(zipPath);
                if (!string.Equals(actual, manifest.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("[อัปเดต] ไฟล์ที่โหลดมาไม่ตรง SHA256 ที่คาดไว้ — ยกเลิก");
                    return;
                }
            }
            SetProgressBusy("กำลังติดตั้งแพท...");
            Directory.CreateDirectory(extractDir);
            ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);
            string? foundExe = GameExeNames
                .SelectMany(n => Directory.GetFiles(extractDir, n, SearchOption.AllDirectories))
                .FirstOrDefault();
            if (foundExe == null)
            {
                Console.WriteLine("[อัปเดต] ไฟล์ที่แตกออกมาไม่สมบูรณ์ — ยกเลิก");
                return;
            }
            RunRobocopyMirror(Path.GetDirectoryName(foundExe)!, gameDir);
            WriteLocalVersion(gameDir, manifest.Version!);
            _progress?.SetComplete("ติดตั้งเสร็จแล้ว — กำลังเปิดเกม...");
            await Task.Delay(500);
        }
        finally
        {
            TryDelete(zipPath);
            TryDeleteDir(extractDir);
        }
    }

    private static async Task DownloadToFile(HttpClient http, string url, string dest, long expectedSize, long receivedBefore, long totalKnown)
    {
        using HttpResponseMessage res = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        res.EnsureSuccessStatusCode();
        long thisTotal = res.Content.Headers.ContentLength ?? expectedSize;
        long thisReceived = 0;
        byte[] buffer = new byte[256 * 1024];
        await using FileStream fs = new FileStream(dest, FileMode.Create, FileAccess.Write);
        using Stream dl = await res.Content.ReadAsStreamAsync();
        int read;
        while ((read = await dl.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            await fs.WriteAsync(buffer.AsMemory(0, read));
            thisReceived += read;
            long overall = receivedBefore + thisReceived;
            long overallTotal = totalKnown > 0 ? totalKnown : thisTotal;
            _progress?.SetProgress(overall, overallTotal);
        }
    }

    private static bool IsSafeRelPath(string? rel)
    {
        if (string.IsNullOrWhiteSpace(rel)) return false;
        if (Path.IsPathRooted(rel)) return false;
        if (rel.Contains("..", StringComparison.Ordinal)) return false;
        return true;
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

    /// <summary>
    /// ชื่อ exe ของตัวเกมที่รองรับ — ชุดเก่าใช้ `DurangoV2.exe` ชุดใหม่ (DurangoTH-v2) ใช้ `Durango.exe`
    /// เรียงตามลำดับที่จะลองหา ต้องรับทั้งคู่ ไม่งั้นชุดแจกคนละรุ่นจะเปิดไม่ขึ้น/อัปเดตไม่ได้แบบเงียบ ๆ
    /// </summary>
    private static readonly string[] GameExeNames = { "Durango.exe", "DurangoV2.exe" };

    private static bool IsGameRunning()
    {
        return GameExeNames
            .Select(Path.GetFileNameWithoutExtension)
            .Any(n => Process.GetProcessesByName(n).Length > 0);
    }

    private static void LaunchGame(string gameDir)
    {
        string? exePath = GameExeNames
            .Select(n => Path.Combine(gameDir, n))
            .FirstOrDefault(File.Exists);
        if (exePath == null)
        {
            Console.WriteLine("ไม่พบ " + string.Join(" หรือ ", GameExeNames)
                + " — ตรวจว่าแตกไฟล์ zip ครบทั้งโฟลเดอร์แล้วหรือยัง");
            Console.WriteLine("(กด Enter เพื่อปิดหน้าต่างนี้)");
            Console.ReadLine();
            return;
        }

        string server = ReadServerTarget(gameDir);
        if (!server.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            server = "http://" + server;
        }
        Console.WriteLine();
        Console.WriteLine("  Durango TH");
        Console.WriteLine("  server : " + server);
        Console.WriteLine();

        // UseShellExecute ต้องเป็น false ถึงจะตั้ง EnvironmentVariables ได้ (.NET บังคับคู่กันแบบนี้ —
        // ลองแล้วจริงตอนเทส: true คู่กับ EnvironmentVariables โยน InvalidOperationException ทันที)
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = "-durango-updated -force-d3d11 -screen-fullscreen 0 -screen-width 1600 -screen-height 900 " +
                        "-logFile \"" + Path.Combine(gameDir, "game.log") + "\"",
            WorkingDirectory = gameDir,
            UseShellExecute = false
        };
        // 🐛 [แก้เอง] 29 ส.ค. 2026 — เดิมคำนวณ `server` จาก server.txt ถูกต้อง แต่ไม่เคยส่งต่อให้ตัวเกมเลย
        // (ขาดบรรทัดนี้) ⇒ DurangoV2.exe เปิดมาแล้วไม่มี DURANGO_AUTOCONNECT ⇒ PatchAutoConnect ใน DLL
        // (เช็ค env ตัวนี้ใน Server.BeginServer) ไม่ทำงาน ⇒ ปุ่ม "Dinoworld Server" fallback ไปที่
        // Preferences "last_connect_ip" (default 127.0.0.1) แทน — ผู้เล่นใหม่ที่ไม่เคยกรอก IP เอง
        // ผ่านเมนู "เยี่ยมชมเกาะเพื่อน" เลยต่อเข้าเครื่องตัวเองเปล่า ๆ ทุกครั้ง
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

    private static async Task<Manifest?> FetchManifestFromKnownSources(string gameDir)
    {
        List<string> urls = new List<string>();
        void AddUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            url = url.Trim();
            if (urls.Exists(u => string.Equals(u, url, StringComparison.OrdinalIgnoreCase))) return;
            urls.Add(url);
        }

        AddUrl(_manifestUrlOverride);
        string server = ReadServerTarget(gameDir);
        if (!string.IsNullOrEmpty(server))
        {
            if (!server.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !server.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                server = "http://" + server;
            }
            AddUrl(server.TrimEnd('/') + "/launcher/version");
        }
        AddUrl(ReadManifestUrlFile(gameDir));
        AddUrl(DefaultManifestUrl);

        foreach (string url in urls)
        {
            SetProgressBusy("กำลังตรวจสอบแพท...");
            Console.WriteLine("[อัปเดต] เช็ค manifest: " + url);
            Manifest? manifest = await FetchManifest(url);
            manifest?.Normalize();
            if (manifest == null || string.IsNullOrEmpty(manifest.Version))
            {
                continue;
            }
            bool hasPatches = manifest.Files != null && manifest.Files.Count > 0;
            if (!hasPatches && string.IsNullOrEmpty(manifest.ZipUrl))
            {
                continue;
            }
            return manifest;
        }
        return null;
    }

    private static async Task<Manifest?> FetchManifest(string manifestUrl)
    {
        try
        {
            using HttpClient probe = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true })
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            probe.DefaultRequestHeaders.UserAgent.ParseAdd("DurangoUpdater/1.2");
            string json = await probe.GetStringAsync(manifestUrl);
            return JsonSerializer.Deserialize<Manifest>(json, JsonOpts);
        }
        catch (Exception e)
        {
            Console.WriteLine("[อัปเดต] โหลด manifest ไม่ได้จาก " + manifestUrl + " (" + e.Message + ")");
            return null;
        }
    }

    private static string? ReadManifestUrlFile(string gameDir)
    {
        string path = Path.Combine(gameDir, "update-manifest-url.txt");
        if (!File.Exists(path)) return null;
        foreach (string line in File.ReadAllLines(path))
        {
            string t = line.Trim();
            if (t.Length == 0 || t.StartsWith("#")) continue;
            return t;
        }
        return null;
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

    /// <summary>
    /// รายการแพตช์ที่เซิร์ฟ/GitHub ส่งมา
    ///
    /// 🐛 [แก้เอง] 1 ก.ย. 2026 — **ต้นเหตุที่ระบบอัปเดตไม่เคยทำงานเลย**
    /// เดิมประกาศคู่กันทั้ง `Sha256`/`sha256` และ `Files`/`files` เพื่อรับ JSON ทั้งสองแบบ
    /// แต่ตัวอ่าน JSON ตั้ง `PropertyNameCaseInsensitive = true` ไว้ ⇒ สองชื่อนั้นชนกัน
    /// System.Text.Json โยน "The JSON property name for '...sha256' collides with another property"
    /// **ตั้งแต่ก่อนอ่านค่าแรก** ⇒ manifest พังทุกแหล่ง ⇒ เข้าทาง fallback "ไม่พบรายการแพตช์
    /// เปิดเกมเวอร์ชันเดิมต่อ" แบบเงียบ ๆ ไม่มีใครรู้ว่าอัปเดตไม่เคยติด
    ///
    /// ⇒ ไม่ต้องประกาศซ้ำ: PropertyNameCaseInsensitive จับ `sha256` → `Sha256` ให้เองอยู่แล้ว
    ///   เหลือแค่ `zip_url` ที่เป็นคนละชื่อจริง ๆ (มี _) ต้องใช้ JsonPropertyName กำกับ
    /// </summary>
    private sealed class Manifest
    {
        public string? Version { get; set; }
        public string? ZipUrl { get; set; }

        /// <summary>ชื่อแบบ snake_case ที่ /launcher/version ของเซิร์ฟใช้</summary>
        [JsonPropertyName("zip_url")]
        public string? ZipUrlSnake { get; set; }

        public string? Sha256 { get; set; }
        public string? Notes { get; set; }
        public List<PatchFile>? Files { get; set; }

        public void Normalize()
        {
            if (string.IsNullOrEmpty(ZipUrl))
            {
                ZipUrl = ZipUrlSnake;
            }
        }
    }

    private sealed class PatchFile
    {
        public string? Path { get; set; }
        public string? Url { get; set; }
        public string? Sha256 { get; set; }
        public long Size { get; set; }
    }
}
