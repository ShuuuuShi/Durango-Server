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
    // [4 ก.ย. 2026] เลิกใช้ เล่นเกม.bat แล้ว (ลบไฟล์ทิ้ง) — launcher เปิด Durango.exe เอง
    private const string SettingsFile = "launcher_settings.json";
    private static readonly string[] DefaultServers = { "127.0.0.1:8190" };

    private string _serverAddr = "127.0.0.1:8190";
    private bool _autoPatch = true;
    private bool _busy;
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };
    private (string version, string zipUrl, string sha256, string notes)? _remote;   // null = เช็คไม่ได้
    // [4 ก.ย. 2026] รายการไฟล์แพตช์ (DLL) จาก manifest — มี = แพตช์เฉพาะไฟล์ที่ต่าง ไม่โหลด zip ทั้งเกม
    private List<PatchFile>? _remoteFiles;
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
        _ = CheckUpdateAsync();          // [4 ก.ย. 2026] เช็คเวอร์ชันตั้งแต่เปิด launcher (ไม่รอกดเข้าเกม)
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

        // [4 ก.ย. 2026] ไม่ต้องมี เล่นเกม.bat แล้ว (ลบทิ้ง) — launcher เปิด exe เอง
        // ไม่มี exe ก็ยังไปต่อได้: VerifyAndRepairAsync จะโหลด Durango.exe มาให้จาก /launcher/files
        string exePath = ResolveGameExe();

        SetBusy(true);
        try
        {
            // 1) เช็คเวอร์ชัน (เช็คไม่ได้ = ข้าม เข้าเกมเวอร์ชันเดิมได้เสมอ)
            await CheckUpdateAsync();

            // 1.5) ตรวจไฟล์ครบไหม — ขาด/ขนาดไม่ตรง โหลดเฉพาะไฟล์นั้นให้อัตโนมัติ (เจ้าของสั่ง)
            try
            {
                int repaired = await VerifyAndRepairAsync(deep: false);
                if (repaired > 0)
                {
                    MessageBox.Show(this, $"พบไฟล์เกมขาด/ไม่ครบ {repaired} ไฟล์ — โหลดมาให้เรียบร้อยแล้ว ✅",
                        "ตรวจไฟล์เกม", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "ซ่อมไฟล์อัตโนมัติไม่สำเร็จ: " + ex.Message + "\n(ลองกดปุ่ม \"ซ่อมไฟล์\")",
                    "ตรวจไฟล์เกม", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

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

        // ซ่อมไฟล์อาจเพิ่งโหลด Durango.exe มาให้ — resolve ใหม่หลังซ่อมเสร็จ
        exePath = ResolveGameExe();
        if (!File.Exists(exePath))
        {
            MessageBox.Show(this, $"ไม่พบ Durango.exe ในโฟลเดอร์นี้\n({GameDir})\n\nกดปุ่ม \"ซ่อมไฟล์\" เพื่อโหลดไฟล์เกมให้ครบ",
                "DinoWorld Launcher", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        LaunchGame(exePath);
    }

    /// <summary>
    /// [4 ก.ย. 2026] เปิดเกม "ตรง ๆ" ไม่ผ่าน เล่นเกม.bat แล้ว (ลบไฟล์ bat ทิ้งแล้ว — เจ้าของสั่ง)
    /// ตั้ง env ที่ bat เคยตั้งให้ครบ: DINOWORLD_LAUNCH (token กันเปิดเอง) · DURANGO_AUTOCONNECT · DURANGO_PERF_OVERLAY
    /// </summary>
    private void LaunchGame(string exePath)
    {
        try
        {
            // token ต่อครั้ง → launcher.session + env ⇒ เกม (LauncherGate) เทียบ ไม่ตรง = ปิดตัวเอง
            string launchToken = Guid.NewGuid().ToString("N");
            File.WriteAllText(IoPath.Combine(GameDir, "launcher.session"), launchToken);

            string autoConnect = HostOf(_serverAddr);
            if (!autoConnect.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                && !autoConnect.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                autoConnect = "http://" + autoConnect;   // client parse host:port เปล่า ๆ เป็น URI ไม่ได้
            }

            ProcessStartInfo psi = new()
            {
                FileName = exePath,
                WorkingDirectory = GameDir,
                UseShellExecute = false,
            };
            foreach (string a in new[]
            {
                "-force-d3d11", "-screen-fullscreen", "0",
                "-screen-width", "1600", "-screen-height", "900",
                "-logFile", IoPath.Combine(GameDir, "game.log"),
            })
            {
                psi.ArgumentList.Add(a);
            }
            psi.Environment["DINOWORLD_LAUNCH"] = launchToken;
            psi.Environment["DURANGO_AUTOCONNECT"] = autoConnect;
            psi.Environment["DURANGO_PERF_OVERLAY"] = "0";   // overlay F9 เป็นของ dev ผู้เล่นกดเปิดเองได้
            Process.Start(psi);
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "เปิดเกมไม่สำเร็จ: " + ex.Message, "DinoWorld Launcher",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ───────────────────────── ซ่อมไฟล์ ─────────────────────────
    private async void Repair_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        SetBusy(true, "🔧 กำลังซ่อมไฟล์…");
        try
        {
            // [4 ก.ย. 2026] ซ่อมแบบละเอียด: เทียบ SHA256 ทุกไฟล์กับเซิร์ฟ แล้วโหลดเฉพาะตัวที่ไม่ตรง
            // (ไม่โหลด zip ทั้งเกมแล้ว) — เซิร์ฟไม่มีรายการไฟล์ค่อย fallback ไป zip เต็มแบบเดิม
            int repaired = await VerifyAndRepairAsync(deep: true);
            if (repaired >= 0)
            {
                MessageBox.Show(this,
                    repaired == 0
                        ? "ตรวจครบทุกไฟล์แล้ว — ไฟล์ถูกต้องทั้งหมด ✅"
                        : $"ซ่อมเสร็จ — โหลดไฟล์ที่ขาด/เสีย {repaired} ไฟล์ ✅",
                    "ซ่อมไฟล์", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_remote?.zipUrl == null) await CheckUpdateAsync();
            if (_remote?.zipUrl == null)
            {
                MessageBox.Show(this, "เซิร์ฟไม่ได้ให้รายการไฟล์ และไม่มีแหล่งดาวน์โหลดสำรอง — ซ่อมไม่ได้ตอนนี้",
                    "ซ่อมไฟล์", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await PatchAsync(forceRepair: true);
            MessageBox.Show(this, "ซ่อมไฟล์เสร็จ (โหลดชุดเต็ม) ✅",
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
                    _remoteFiles = ParseFiles(o["files"] ?? o["Files"]);   // DLL-patch list (เซิร์ฟ rewrite Url เป็น /launcher/file ให้)
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
                _remoteFiles = m.Files;
            }
        }
        catch
        {
            _remote = null;      // ออฟไลน์ = ไม่อัปเดต เข้าเกมเดิมได้
            _remoteFiles = null;
        }
        RefreshVersionUi();
    }

    private async Task PatchAsync(bool forceRepair = false)
    {
        var (version, zipUrl, sha256, notes) = _remote!.Value;

        // [4 ก.ย. 2026] มีรายการไฟล์ (DLL) ใน manifest ⇒ แพตช์เฉพาะไฟล์ที่ต่าง ไม่โหลด zip ทั้งเกม
        // (logic เดียวกับ tools/Updater ApplyDllPatches) · ซ่อมไฟล์ (forceRepair) ค่อยใช้ zip เต็ม
        if (!forceRepair && _remoteFiles != null && _remoteFiles.Count > 0)
        {
            await ApplyDllPatchesAsync(version, _remoteFiles);
            return;
        }

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
            string? foundExe = Directory.GetFiles(extractDir, "Durango.exe", SearchOption.AllDirectories).FirstOrDefault()
                            ?? Directory.GetFiles(extractDir, GameExe, SearchOption.AllDirectories).FirstOrDefault();
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

    // ─────────── [4 ก.ย. 2026] ตรวจไฟล์ทั้งชุด + โหลดเฉพาะไฟล์ที่ขาด/ไม่ตรง (เจ้าของสั่ง) ───────────
    /// <summary>
    /// เทียบไฟล์ในเครื่องกับ <c>/launcher/files</c> ของเซิร์ฟ แล้วโหลด "เฉพาะไฟล์ที่ขาดหรือไม่ตรง"
    /// (ขาด dll ก้อน 80 MB ก็โหลดแค่ก้อนนั้น ไม่ใช่ทั้งเกม) ⇒ ทุกเครื่องได้ไฟล์ชุดเดียวกัน
    ///
    /// deep=false : เช็ค "มีไฟล์ไหม + ขนาดตรงไหม" (เร็ว ใช้ตอนกดเข้าเกมทุกครั้ง)
    /// deep=true  : เช็ค SHA256 ทุกไฟล์ (ช้ากว่า ใช้ตอนกดปุ่ม "ซ่อมไฟล์")
    /// คืนจำนวนไฟล์ที่โหลดมาซ่อม · -1 = ติดต่อเซิร์ฟไม่ได้ (ข้ามไป เข้าเกมได้ตามปกติ)
    /// </summary>
    private async Task<int> VerifyAndRepairAsync(bool deep)
    {
        SetProgress(0, deep ? "🔍 ตรวจไฟล์ทั้งหมด (SHA256)…" : "🔍 ตรวจไฟล์เกม…");
        JArray? files;
        try
        {
            string json = await _http.GetStringAsync($"http://{HostOf(_serverAddr)}/launcher/files");
            JObject o = JObject.Parse(json);
            files = o["files"] as JArray;
            if (files == null || files.Count == 0) return 0;
        }
        catch
        {
            return -1;      // เซิร์ฟไม่มีรายการไฟล์/ออฟไลน์ — ไม่บล็อกการเข้าเกม
        }

        List<(string path, string url, string sha, long size)> missing = new();
        int i = 0;
        foreach (JToken t in files)
        {
            i++;
            if (t is not JObject fo) continue;
            string? rel = (string?)fo["path"];
            string? url = (string?)fo["url"];
            string? sha = (string?)fo["sha256"];
            long size = (long?)fo["size"] ?? 0;
            if (!IsSafeRelPath(rel) || string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(sha)) continue;
            if ((i % 200) == 0) SetProgress(i * 40.0 / files.Count, $"🔍 ตรวจไฟล์ {i}/{files.Count}…");

            string dest = IoPath.Combine(GameDir, rel!.Replace('/', IoPath.DirectorySeparatorChar));
            bool bad;
            if (!File.Exists(dest))
            {
                bad = true;
            }
            else if (new FileInfo(dest).Length != size)
            {
                bad = true;
            }
            else if (deep)
            {
                bad = !string.Equals(await ComputeSha256Async(dest), sha, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                bad = false;
            }
            if (bad) missing.Add((rel!, url!, sha!, size));
        }

        if (missing.Count == 0)
        {
            SetProgress(100, "✅ ไฟล์ครบถูกต้อง");
            return 0;
        }

        long totalBytes = 0;
        foreach (var m in missing) totalBytes += m.size;
        SetProgress(40, $"⬇️ ต้องโหลด {missing.Count} ไฟล์ ({totalBytes / 1048576.0:0} MB)…");

        string stage = IoPath.Combine(IoPath.GetTempPath(), "dinoworld-repair-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
            // ⚠️ ห้ามใช้ _http (timeout 10 วิ) โหลดไฟล์ใหญ่ — ไฟล์ 80 MB บนเน็ตผู้เล่นจะ timeout ทุกครั้ง
            using HttpClient dlHttp = new() { Timeout = TimeSpan.FromMinutes(30) };
            byte[] buffer = new byte[81920];
            long done = 0;
            int n = 0;
            foreach (var m in missing)
            {
                n++;
                string staged = IoPath.Combine(stage, "f" + n + ".bin");
                using (FileStream fs = new(staged, FileMode.Create, FileAccess.Write))
                {
                    using Stream dl = await dlHttp.GetStreamAsync(m.url);
                    int read;
                    while ((read = await dl.ReadAsync(buffer)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, read);
                        done += read;
                        SetProgress(40 + Math.Min(55, totalBytes > 0 ? done * 55.0 / totalBytes : 0),
                            $"⬇️ {n}/{missing.Count}  {IoPath.GetFileName(m.path)}");
                    }
                }
                string actual = await ComputeSha256Async(staged);
                if (!string.Equals(actual, m.sha, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("ไฟล์ " + m.path + " ที่โหลดมาไม่ตรง SHA256 — ยกเลิก ไม่แตะไฟล์เกม");
                }
                string dest = IoPath.Combine(GameDir, m.path.Replace('/', IoPath.DirectorySeparatorChar));
                Directory.CreateDirectory(IoPath.GetDirectoryName(dest)!);
                File.Copy(staged, dest, overwrite: true);
                File.Delete(staged);
            }
            SetProgress(100, $"✅ ซ่อมไฟล์ครบ {missing.Count} ไฟล์");
            return missing.Count;
        }
        finally
        {
            TryDeleteDir(stage);
        }
    }

    // ───────────────────────── [4 ก.ย. 2026] DLL-patch (ไม่โหลด zip ทั้งเกม) ─────────────────────────
    /// <summary>
    /// แพตช์เฉพาะไฟล์ที่ SHA256 ต่างจาก manifest: โหลดลง temp → ตรวจ hash → ค่อยทับไฟล์จริง
    /// ไฟล์ไหนตรงอยู่แล้วข้าม · hash ไม่ตรง = ยกเลิกทั้งชุด ไม่แตะเกม · เสร็จค่อยเขียน version.txt
    /// </summary>
    private async Task ApplyDllPatchesAsync(string version, List<PatchFile> files)
    {
        SetProgress(0, "🔍 ตรวจไฟล์ที่ต้องอัปเดต…");
        List<PatchFile> needed = new();
        foreach (PatchFile f in files)
        {
            if (!IsSafeRelPath(f.Path) || string.IsNullOrWhiteSpace(f.Url) || string.IsNullOrWhiteSpace(f.Sha256)) continue;
            if (f.Path!.EndsWith("DurangoUpdater.exe", StringComparison.OrdinalIgnoreCase)) continue;
            if (f.Path.EndsWith("DinoWorldLauncher.exe", StringComparison.OrdinalIgnoreCase)) continue;   // ห้ามทับตัวเอง
            string dest = IoPath.Combine(GameDir, f.Path.Replace('/', IoPath.DirectorySeparatorChar));
            if (File.Exists(dest))
            {
                string localHash = await ComputeSha256Async(dest);
                if (string.Equals(localHash, f.Sha256, StringComparison.OrdinalIgnoreCase)) continue;
            }
            needed.Add(f);
        }

        if (needed.Count == 0)
        {
            // ไฟล์ครบตรง manifest แล้ว — ปรับเลขเวอร์ชันได้ (มีรายการไฟล์ให้เทียบจริง ไม่ใช่ list ว่าง)
            File.WriteAllText(IoPath.Combine(GameDir, "version.txt"), version);
            SetProgress(100, "✅ ไฟล์ครบแล้ว");
            return;
        }

        string stageDir = IoPath.Combine(IoPath.GetTempPath(), "dinoworld-patch-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stageDir);
        try
        {
            using HttpClient http = new() { Timeout = TimeSpan.FromMinutes(10) };
            long totalKnown = 0; foreach (PatchFile f in needed) totalKnown += f.Size > 0 ? f.Size : 0;
            long receivedAll = 0;
            byte[] buffer = new byte[81920];
            foreach (PatchFile f in needed)
            {
                string staged = IoPath.Combine(stageDir, IoPath.GetFileName(f.Path)!);
                SetProgress(totalKnown > 0 ? receivedAll * 90.0 / totalKnown : 0, "⬇️ โหลด " + IoPath.GetFileName(f.Path));
                using (FileStream fs = new(staged, FileMode.Create, FileAccess.Write))
                {
                    using Stream dl = await http.GetStreamAsync(f.Url);
                    int read;
                    while ((read = await dl.ReadAsync(buffer)) > 0)
                    {
                        await fs.WriteAsync(buffer, 0, read);
                        receivedAll += read;
                        if (totalKnown > 0) SetProgress(Math.Min(90, receivedAll * 90.0 / totalKnown));
                    }
                }
                string actual = await ComputeSha256Async(staged);
                if (!string.Equals(actual, f.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("ไฟล์ " + IoPath.GetFileName(f.Path) + " ที่โหลดไม่ตรง SHA256 — ยกเลิก ไม่แตะเกม");
                }
            }

            SetProgress(95, "🔁 ติดตั้งแพตช์…");
            foreach (PatchFile f in needed)
            {
                string staged = IoPath.Combine(stageDir, IoPath.GetFileName(f.Path)!);
                string dest = IoPath.Combine(GameDir, f.Path!.Replace('/', IoPath.DirectorySeparatorChar));
                Directory.CreateDirectory(IoPath.GetDirectoryName(dest)!);
                IOException? last = null;
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    try { File.Copy(staged, dest, overwrite: true); last = null; break; }
                    catch (IOException ex) { last = ex; await Task.Delay(500); }   // เกมอาจยังล็อกไฟล์อยู่แป๊บ
                }
                if (last != null) throw new IOException("เขียนทับ " + f.Path + " ไม่ได้ (ปิดเกมก่อนแล้วลองใหม่): " + last.Message);
            }
            File.WriteAllText(IoPath.Combine(GameDir, "version.txt"), version);
            SetProgress(100, "✅ อัปเดตเสร็จ (แพตช์ " + needed.Count + " ไฟล์)");
        }
        finally
        {
            TryDeleteDir(stageDir);
        }
    }

    private static List<PatchFile>? ParseFiles(JToken? arr)
    {
        if (arr is not JArray a) return null;
        List<PatchFile> list = new();
        foreach (JToken t in a)
        {
            if (t is not JObject o) continue;
            list.Add(new PatchFile
            {
                Path = (string?)o["Path"] ?? (string?)o["path"],
                Url = (string?)o["Url"] ?? (string?)o["url"],
                Sha256 = (string?)o["Sha256"] ?? (string?)o["sha256"],
                Size = (long?)o["Size"] ?? (long?)o["size"] ?? 0,
            });
        }
        return list;
    }

    private static async Task<string> ComputeSha256Async(string filePath)
    {
        await using FileStream fs = File.OpenRead(filePath);
        return Convert.ToHexString(await SHA256.HashDataAsync(fs)).ToLowerInvariant();
    }

    private static bool IsSafeRelPath(string? rel)
    {
        if (string.IsNullOrWhiteSpace(rel)) return false;
        if (IoPath.IsPathRooted(rel)) return false;
        if (rel.Contains("..", StringComparison.Ordinal)) return false;
        return true;
    }

    /// <summary>ชุดแจกจริงเป็น Durango.exe · ชุดเก่า DurangoV2.exe — รับทั้งสอง</summary>
    private string ResolveGameExe()
    {
        string a = IoPath.Combine(GameDir, "Durango.exe");
        return File.Exists(a) ? a : IoPath.Combine(GameDir, GameExe);
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
            return "https://github.com/ShuuuuShi/Durango-TH-Client/releases/latest/download/manifest.json";
        }
        foreach (string line in File.ReadAllLines(p))
        {
            string t = line.Trim();
            if (t.Length > 0 && !t.StartsWith("#")) return t;
        }
        return "https://github.com/ShuuuuShi/Durango-TH-Client/releases/latest/download/manifest.json";
    }

    private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
    private static void TryDeleteDir(string path) { try { if (Directory.Exists(path)) Directory.Delete(path, true); } catch { } }

    private sealed class Manifest
    {
        public string? Version { get; set; }
        public string? ZipUrl { get; set; }
        public string? Sha256 { get; set; }
        public string? Notes { get; set; }
        public List<PatchFile>? Files { get; set; }   // [4 ก.ย. 2026] รายการ DLL-patch (เหมือน tools/Updater)
    }

    private sealed class PatchFile
    {
        public string? Path { get; set; }
        public string? Url { get; set; }
        public string? Sha256 { get; set; }
        public long Size { get; set; }
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
