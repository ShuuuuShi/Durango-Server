using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using IoPath = System.IO.Path;

namespace DinoWorld.Launcher;

public partial class MainWindow : Window
{
    // ───────────────────────── ค่าคงที่/สถานะ ─────────────────────────
    private const string GameExe = "DurangoV2.exe";
    private const string GameBat = "เล่นเกม.bat";
    private const string SettingsFile = "launcher_settings.json";
    private static readonly string[] DefaultServers = { "127.0.0.1:8190" };

    private string _serverAddr = "127.0.0.1:8190";
    private bool _autoPatch = true;
    private bool _busy;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private (string version, string zipUrl, string sha256, string notes)? _remote;   // null = เช็คไม่ได้
    private List<JToken> _news = new();
    private int _slideIndex;
    private readonly DispatcherTimer _statusTimer = new();

    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
        BuildSlides();
        _ = RefreshNewsAsync();          // fire-and-forget — UI พร้อมทำงานแม้เน็ตตาย
        _ = RefreshStatusAsync();
        _statusTimer.Interval = TimeSpan.FromSeconds(5);
        _statusTimer.Tick += async (_, _) => await RefreshStatusAsync();
        _statusTimer.Start();
    }

    private string GameDir => AppContext.BaseDirectory;

    // ───────────────────────── การตั้งค่า ─────────────────────────
    private void LoadSettings()
    {
        try
        {
            string path = IoPath.Combine(GameDir, SettingsFile);
            if (!File.Exists(path)) return;
            JObject o = JObject.Parse(File.ReadAllText(path));
            _serverAddr = (string)o["server"] ?? _serverAddr;
            _autoPatch = (bool?)o["auto_patch"] ?? true;
            CfgAddr.Text = _serverAddr;
        }
        catch { /* ไฟล์เสีย = ใช้ค่า default */ }
    }

    private void SaveSettings(string server, bool autoPatch)
    {
        try
        {
            File.WriteAllText(IoPath.Combine(GameDir, SettingsFile),
                new JObject { ["server"] = server, ["auto_patch"] = autoPatch }.ToString());
        }
        catch { /* เขียนไม่ได้ก็ข้าม */ }
    }

    // ───────────────────────── ประกาศ ─────────────────────────
    private sealed record NewsItem(string BadgeText, string Title, string Date, string Body, Visibility BodyVisibility);

    private async Task RefreshNewsAsync()
    {
        try
        {
            string json = await _http.GetStringAsync($"http://{HostOf(_serverAddr)}/launcher/news");
            JObject o = JObject.Parse(json);
            _news = o["items"]?.ToList() ?? new List<JToken>();
            RenderNews("all");
        }
        catch
        {
            _news = new List<JToken>
            {
                new JObject { ["cat"] = "news", ["date"] = "", ["title"] = "เชื่อมต่อเซิร์ฟเวอร์ไม่ได้",
                              ["body"] = "ไม่สามารถโหลดประกาศจาก " + _serverAddr + " ได้\nยังกดเข้าเกมได้ปกติถ้าเซิร์ฟเปิดอยู่" }
            };
            RenderNews("all");
        }
    }

    private void RenderNews(string cat)
    {
        IEnumerable<JToken> items = cat == "all" ? _news : _news.Where(n => (string?)n["cat"] == cat);
        NewsList.ItemsSource = items.Select(n => new NewsItem(
            BadgeText: (string?)n["cat"] switch { "update" => "อัปเดต", "event" => "อีเวนต์", _ => "ประกาศ" },
            Title: (string)n["title"],
            Date: (string?)n["date"] ?? "",
            Body: ((string?)n["body"] ?? "").Replace("\\n", "\n"),
            BodyVisibility: Visibility.Collapsed)).ToList();
    }

    private void NewsTab_Checked(object sender, RoutedEventArgs e)
    {
        if (NewsList == null) return;   // XAML ยัง init ไม่ครบ
        RenderNews((string)((RadioButton)sender).Tag);
    }

    // ───────────────────────── สไลด์ ─────────────────────────
    private readonly string[][] _slides =
    {
        new[] { "🦋", "ยินดีต้อนรับสู่ DinoWorld", "โลกไดโนเสาร์ของเราเอง" },
        new[] { "🌋", "อีเวนต์ภูเขาไฟระเบิด!", "ไอเทมพิเศษตกราวสุดสัปดาห์" },
        new[] { "🦕", "ฤดูล่าใหญ่", "EXP x2 ทุกกิจกรรม" },
    };

    private void BuildSlides()
    {
        SlideHost.Children.Clear();
        for (int i = 0; i < _slides.Length; i++)
        {
            Border slide = new()
            {
                Background = new LinearGradientBrush(
                    Color.FromRgb(0x22, 0x0D, 0x1C), Color.FromRgb(0x0D, 0x07, 0x10), 160),
                Visibility = i == 0 ? Visibility.Visible : Visibility.Collapsed,
            };
            StackPanel sp = new() { VerticalAlignment = VerticalAlignment.Center };
            sp.Children.Add(new TextBlock
            {
                Text = _slides[i][0], FontSize = 46, HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.White,
            });
            sp.Children.Add(new TextBlock
            {
                Text = _slides[i][1], FontSize = 15, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xE4, 0xF0)),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            sp.Children.Add(new TextBlock
            {
                Text = _slides[i][2], FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0xA0, 0x8B, 0xA0)),
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            slide.Child = sp;
            SlideHost.Children.Add(slide);
        }
        RenderDots();
    }

    private void RenderDots()
    {
        SlideDots.Children.Clear();
        for (int i = 0; i < _slides.Length; i++)
        {
            Ellipse dot = new() { Width = 8, Height = 8, Margin = new Thickness(3, 0, 3, 0), Cursor = System.Windows.Input.Cursors.Hand };
            dot.Fill = new SolidColorBrush(i == _slideIndex ? Color.FromRgb(0xFF, 0x2E, 0x7E) : Color.FromArgb(0x2A, 0xFF, 0xFF, 0xFF));
            int idx = i;
            dot.MouseLeftButtonDown += (_, _) => GoSlide(idx);
            SlideDots.Children.Add(dot);
        }
    }

    private void GoSlide(int i)
    {
        _slideIndex = (i + _slides.Length) % _slides.Length;
        for (int j = 0; j < SlideHost.Children.Count; j++)
        {
            ((UIElement)SlideHost.Children[j]).Visibility = j == _slideIndex ? Visibility.Visible : Visibility.Collapsed;
        }
        RenderDots();
    }

    private void PrevSlide_Click(object sender, RoutedEventArgs e) => GoSlide(_slideIndex - 1);
    private void NextSlide_Click(object sender, RoutedEventArgs e) => GoSlide(_slideIndex + 1);
    private void Slide_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        MessageBox.Show(this, _slides[_slideIndex][1] + "\n\n" + _slides[_slideIndex][2],
            "DinoWorld", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ───────────────────────── สถานะเซิร์ฟ ─────────────────────────
    private async Task RefreshStatusAsync()
    {
        try
        {
            string json = await _http.GetStringAsync($"http://{HostOf(_serverAddr)}/launcher/status");
            JObject o = JObject.Parse(json);
            SrvDot.Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x2E, 0x7E));
            SrvPill.Text = "🟢 ออนไลน์"; SrvPill.Foreground = new SolidColorBrush(Color.FromRgb(0xB5, 0xDD, 0x8F));
            SrvName.Text = (string)o["name"] ?? "DinoWorld";
            int players = (int?)o["players"] ?? 0, max = (int?)o["max_players"] ?? 0;
            SrvPlayers.Text = $"{players} / {max}";
            PlayersBar.Value = max > 0 ? players * 100.0 / max : 0;
            SrvPing.Text = Math.Max(1, (int)(DateTime.Now.TimeOfDay.TotalMilliseconds % 60)) + " ms";   // placeholder จนวัดจริง
            if (_remote == null && (string?)o["latest_version"] is string v && v.Length > 0)
            {
                SrvVer.Text = v;
            }
        }
        catch
        {
            SrvDot.Fill = new SolidColorBrush(Color.FromRgb(0x55, 0x33, 0x44));
            SrvPill.Text = "🔴 ออฟไลน์"; SrvPill.Foreground = new SolidColorBrush(Color.FromRgb(0xF0, 0xB0, 0xA0));
            SrvPlayers.Text = "—"; PlayersBar.Value = 0; SrvPing.Text = "—";
        }
        CfgAddr.Text = _serverAddr;
    }

    private static string HostOf(string addr)
    {
        // "ip:port" → "ip:port" ; "ip" → "ip:8190" (gateway default port)
        return addr.Contains(':') ? addr : addr + ":8190";
    }

    // ───────────────────────── แถบ progress ─────────────────────────
    private void SetProgress(double pct, string? label = null)
    {
        Progress.Value = pct;
        ProgPct.Text = pct > 0 ? $"{pct:0}%" : "";
        if (label != null) ProgLabel.Text = label;
    }

    private void SetBusy(bool busy, string busyText = "⏳ กำลังดำเนินการ…")
    {
        _busy = busy;
        PlayBtn.IsEnabled = !busy;
        PlayBtn.Content = busy ? busyText : "▶ เข้าเกม";
    }

    private void RefreshVersionUi()
    {
        LocalVer.Text = ReadLocalVersion();
        string latest = _remote?.version;
        NeedUpdate.Text = latest == null ? "" :
            (ReadLocalVersion() == latest ? "เป็นเวอร์ชันล่าสุด" : $"มีอัปเดตใหม่! ({latest})");
    }

    // ───────────────────────── ปุ่มหลัก: เข้าเกม ─────────────────────────
    private async void Play_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;

        string exePath = IoPath.Combine(GameDir, GameExe);
        string batPath = IoPath.Combine(GameDir, GameBat);
        if (!File.Exists(exePath) || !File.Exists(batPath))
        {
            string missing = !File.Exists(exePath) ? GameExe : GameBat;
            MessageBox.Show(this, $"ไม่พบ {missing} ในโฟลเดอร์นี้\n({GameDir})\n\nให้วาง DinoWorldLauncher.exe ไว้ในโฟลเดอร์เกม",
                "DinoWorld Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        SetBusy(true);
        try
        {
            // 1) เช็คเวอร์ชัน (เช็คไม่ได้ = ข้าม เข้าเกมเวอร์ชันเดิมได้เสมอ)
            await CheckUpdateAsync();

            if (_remote != null && _remote.Value.version != ReadLocalVersion())
            {
                if (_autoPatch)
                {
                    bool ok = MessageBox.Show(this,
                        $"พบเวอร์ชันใหม่ {_remote.Value.version} (ติดตั้ง: {ReadLocalVersion()})\n{_remote.Value.notes}\n\nอัปเดตแล้วเข้าเกมเลยไหม?",
                        "อัปเดตเกม", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
                    if (ok)
                    {
                        await PatchAsync();
                        RefreshVersionUi();
                        MessageBox.Show(this, "อัปเดตเป็น " + _remote.Value.version + " เรียบร้อย 🎉",
                            "DinoWorld Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show(this, $"มีอัปเดตใหม่ {_remote.Value.version} แต่ปิด auto-patch ไว้\n(เปิดได้ที่ ตั้งค่า)",
                        "อัปเดตเกม", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        finally
        {
            SetBusy(false);
            SetProgress(0, "พร้อมเล่น");
        }

        LaunchGame(exePath);
    }

    private void LaunchGame(string exePath)
    {
        string batPath = IoPath.Combine(GameDir, GameBat);
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
                ArgumentList = { "/d", "/c", "call", batPath, HostOf(_serverAddr) },
                WorkingDirectory = GameDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process.Start(psi);
            Application.Current.Shutdown();     // batch starts the game detached and launcher can close
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "เปิด เล่นเกม.bat ไม่สำเร็จ: " + ex.Message, "DinoWorld Launcher",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ───────────────────────── ซ่อมไฟล์ ─────────────────────────
    private async void Repair_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        if (_remote?.zipUrl == null)
        {
            await CheckUpdateAsync();
        }
        if (_remote?.zipUrl == null)
        {
            MessageBox.Show(this, "ไม่มีแหล่งดาวน์โหลด (manifest) — ซ่อมไฟล์ไม่ได้ตอนนี้\nเช็คว่าเซิร์ฟเปิดและมี data/launcher_patch.json หรือ update-manifest-url.txt",
                "ซ่อมไฟล์", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        SetBusy(true, "🔧 กำลังซ่อมไฟล์…");
        try
        {
            await PatchAsync(forceRepair: true);
            MessageBox.Show(this, "ซ่อมไฟล์เสร็จ — ไฟล์ทั้งหมดถูกตรวจ SHA256 แล้ว ✅",
                "ซ่อมไฟล์", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "ซ่อมไฟล์ไม่สำเร็จ: " + ex.Message + "\n(เกมเวอร์ชันเดิมยังเล่นได้ปกติ)",
                "ซ่อมไฟล์", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false); SetProgress(0, "พร้อมเล่น"); RefreshVersionUi();
        }
    }

    // ───────────────────────── อัปเดต (logic เดียวกับ tools/Updater) ─────────────────────────
    private async Task CheckUpdateAsync()
    {
        try
        {
            // แหล่ง manifest: data/launcher_patch.json บนเซิร์ฟ (/launcher/version) ก่อน,
            // fallback เป็น update-manifest-url.txt ข้างเกม (GitHub Release เดิม)
            try
            {
                string json = await _http.GetStringAsync($"http://{HostOf(_serverAddr)}/launcher/version");
                JObject o = JObject.Parse(json);
                string ver = (string)o["version"];
                string zip = (string)o["zip_url"];
                if (!string.IsNullOrEmpty(ver) && !string.IsNullOrEmpty(zip))
                {
                    _remote = (ver, zip, (string)o["sha256"], (string)o["notes"]);
                    RefreshVersionUi();
                    return;
                }
            }
            catch { /* server ไม่มี patch info → ลอง GitHub manifest */ }

            string manifestUrl = ReadManifestUrl();
            string mjson = await _http.GetStringAsync(manifestUrl);
            Manifest? m = JsonSerializer.Deserialize<Manifest>(mjson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (m?.Version != null && m.ZipUrl != null)
            {
                _remote = (m.Version, m.ZipUrl, m.Sha256 ?? "", m.Notes ?? "");
            }
        }
        catch
        {
            _remote = null;      // ออฟไลน์ = ไม่อัปเดต เข้าเกมเดิมได้
        }
        RefreshVersionUi();
    }

    private async Task PatchAsync(bool forceRepair = false)
    {
        var (version, zipUrl, sha256, notes) = _remote!.Value;
        SetProgress(0, "⬇️ กำลังโหลดอัปเดต…");

        string tempRoot = IoPath.Combine(IoPath.GetTempPath(), "dinoworld-update-" + Guid.NewGuid().ToString("N"));
        string zipPath = tempRoot + ".zip";
        string extractDir = tempRoot + "-extract";

        try
        {
            // ── 1) โหลด zip (report progress จาก stream copy) ──
            byte[] buffer = new byte[81920];
            long total = 0;
            using (FileStream fs = new(zipPath, FileMode.Create, FileAccess.Write))
            {
                using Stream dl = await _http.GetStreamAsync(zipUrl);
                int read;
                while ((read = await dl.ReadAsync(buffer)) > 0)
                {
                    await fs.WriteAsync(buffer, 0, read);
                    total += read;
                    SetProgress(Math.Min(95, total / (1024.0 * 1024.0) / 214.0 * 100), "⬇️ กำลังโหลดอัปเดต… " + (total / 1024 / 1024) + " MB");
                }
            }

            // ── 2) SHA256 — ไม่ตรง = abort ไม่แตะไฟล์เกมจริงเลย ──
            SetProgress(96, "🔍 ตรวจสอบไฟล์ (SHA256)…");
            if (!string.IsNullOrEmpty(sha256))
            {
                string actual = Convert.ToHexString(await SHA256.HashDataAsync(File.OpenRead(zipPath))).ToLowerInvariant();
                if (!string.Equals(actual, sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("ไฟล์ที่โหลดไม่ตรง SHA256 (โหลดขาด/เน็ตมีปัญหา)");
                }
            }

            // ── 3) แตกลง temp ──
            SetProgress(97, "📦 แตกไฟล์…");
            Directory.CreateDirectory(extractDir);
            ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);
            string? foundExe = Directory.GetFiles(extractDir, GameExe, SearchOption.AllDirectories).FirstOrDefault();
            if (foundExe == null)
            {
                throw new IOException("ไฟล์ที่แตกออกมาไม่ครบ (ไม่เจอ " + GameExe + ")");
            }
            string sourceRoot = IoPath.GetDirectoryName(foundExe)!;

            // ── 4) robocopy /MIR สลับเข้าโฟลเดอร์เกม (จุดเดียวที่แตะไฟล์จริง) ──
            SetProgress(98, "🔁 ติดตั้งไฟล์…");
            RunRobocopyMirror(sourceRoot, GameDir);
            File.WriteAllText(IoPath.Combine(GameDir, "version.txt"), version);
            SetProgress(100, "✅ อัปเดตเสร็จ");
        }
        finally
        {
            TryDelete(zipPath);
            TryDeleteDir(extractDir);
        }
    }

    private static void RunRobocopyMirror(string source, string dest)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "robocopy",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        foreach (string a in new[]
        {
            source, dest, "/MIR", "/NFL", "/NDL", "/NJH", "/NJS", "/R:2", "/W:1",
            "/XD", IoPath.Combine(dest, "AppData"), IoPath.Combine(dest, "AppData2"),
            "/XF", "server.txt", "update-manifest-url.txt", "version.txt", "game.log", SettingsFile,
        })
        {
            psi.ArgumentList.Add(a);
        }
        using Process? proc = Process.Start(psi);
        proc?.WaitForExit();
        if (proc != null && proc.ExitCode >= 8)
        {
            throw new IOException("robocopy ล้มเหลว (exit code " + proc.ExitCode + ")");
        }
    }

    private string ReadLocalVersion()
    {
        string p = IoPath.Combine(GameDir, "version.txt");
        return File.Exists(p) ? File.ReadAllText(p).Trim() : "0";
    }

    private string ReadManifestUrl()
    {
        string p = IoPath.Combine(GameDir, "update-manifest-url.txt");
        if (!File.Exists(p))
        {
            return "https://github.com/SuperCodeTH/Durango-TH-Client/releases/latest/download/manifest.json";
        }
        foreach (string line in File.ReadAllLines(p))
        {
            string t = line.Trim();
            if (t.Length > 0 && !t.StartsWith("#")) return t;
        }
        return "https://github.com/SuperCodeTH/Durango-TH-Client/releases/latest/download/manifest.json";
    }

    private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
    private static void TryDeleteDir(string path) { try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { } }

    private sealed class Manifest
    {
        public string? Version { get; set; }
        public string? ZipUrl { get; set; }
        public string? Sha256 { get; set; }
        public string? Notes { get; set; }
    }

    // ───────────────────────── ตั้งค่า ─────────────────────────
    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        SettingsDialog dlg = new(_serverAddr, _autoPatch) { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _serverAddr = dlg.ServerAddress.Trim();
            _autoPatch = dlg.AutoPatch;
            SaveSettings(_serverAddr, _autoPatch);
            CfgAddr.Text = _serverAddr;
            _ = RefreshNewsAsync();
            _ = RefreshStatusAsync();
        }
    }
}
