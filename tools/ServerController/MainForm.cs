using System.Diagnostics;
using System.Net.Http;
using Microsoft.Web.WebView2.WinForms;

namespace DurangoServerController;

/// <summary>
/// หน้าต่างหลัก — แถบเครื่องมือคุม process ของเซิร์ฟ (สิ่งที่หน้าเว็บทำเองไม่ได้) +
/// ฝัง WebView2 แสดง server/admin/index.html ตัวเดิม (ผู้เล่น/POI/config/log/mod/cheat)
/// </summary>
public sealed class MainForm : Form
{
    private const int GatewayPort = 8190;
    private readonly string AdminUrl = $"http://127.0.0.1:{GatewayPort}/admin";

    private readonly Button _btnStart = new() { Text = "▶ เปิดเซิร์ฟ", Width = 100 };
    private readonly Button _btnStop = new() { Text = "■ ปิดเซิร์ฟ", Width = 100, Enabled = false };
    private readonly Button _btnRestart = new() { Text = "↻ รีสตาร์ท", Width = 100, Enabled = false };
    private readonly CheckBox _chkCheat = new() { Text = "เปิดคำสั่งทดสอบ (--enable-cheat)", Checked = true, AutoSize = true };
    private readonly Label _statusLabel = new() { AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
    private readonly Label _pathLabel = new() { AutoSize = true, ForeColor = Color.Gray };
    private readonly TextBox _logBox = new()
    {
        Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical,
        Font = new Font("Consolas", 9F), BackColor = Color.Black, ForeColor = Color.Gainsboro
    };
    private readonly SplitContainer _split = new() { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
    private WebView2? _webView;

    private Process? _serverProcess;
    private string? _serverDir;

    public MainForm()
    {
        Text = "Durango Server Controller";
        Width = 1280;
        Height = 860;
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        Load += async (_, _) => await OnLoadedAsync();
        FormClosing += OnFormClosing;

        _btnStart.Click += async (_, _) => await StartServerAsync();
        _btnStop.Click += (_, _) => StopServer();
        _btnRestart.Click += async (_, _) => await RestartServerAsync();
    }

    private void BuildLayout()
    {
        TableLayoutPanel toolbar = new()
        {
            Dock = DockStyle.Top, Height = 44, ColumnCount = 6, Padding = new Padding(8, 6, 8, 6)
        };
        toolbar.Controls.Add(_btnStart, 0, 0);
        toolbar.Controls.Add(_btnStop, 1, 0);
        toolbar.Controls.Add(_btnRestart, 2, 0);
        toolbar.Controls.Add(_chkCheat, 3, 0);
        toolbar.Controls.Add(_statusLabel, 4, 0);
        Controls.Add(toolbar);

        _pathLabel.Dock = DockStyle.Top;
        _pathLabel.Padding = new Padding(8, 0, 8, 4);
        Controls.Add(_pathLabel);

        SetStatus(false, "ยังไม่ได้เริ่ม");

        // บนสุด = WebView2 (แผงควบคุมหลัก), ล่าง = log ดิบตอนเปิด/ปิดเซิร์ฟ (เผื่อ WebView2 ยังต่อไม่ติด)
        _split.Dock = DockStyle.Fill;
        Controls.Add(_split);
        _split.Panel2.Controls.Add(_logBox);
        _split.Panel2MinSize = 60;
        Controls.SetChildIndex(_split, 0); // ให้ toolbar/pathLabel อยู่บน สั่ง fill ทีหลังสุด

        // ต้องเซ็ต SplitterDistance หลัง handle สร้างแล้ว ไม่งั้น ArgumentOutOfRangeException ถ้าฟอร์มเล็กกว่าที่คิด
        HandleCreated += (_, _) => { try { _split.SplitterDistance = Math.Max(200, Height - 260); } catch { } };
    }

    private void SetStatus(bool running, string text)
    {
        _statusLabel.Text = (running ? "🟢 " : "🔴 ") + text;
        _statusLabel.ForeColor = running ? Color.ForestGreen : Color.Firebrick;
        _btnStart.Enabled = !running;
        _btnStop.Enabled = running;
        _btnRestart.Enabled = running;
    }

    private void Log(string line)
    {
        if (_logBox.IsDisposed) return;
        void Append()
        {
            _logBox.AppendText(line + Environment.NewLine);
        }
        if (_logBox.InvokeRequired) _logBox.BeginInvoke(Append);
        else Append();
    }

    // ── หา server/ จากตำแหน่งไฟล์ .exe นี้ (เดินขึ้นไปหาไฟล์ server/DurangoServer.csproj) ──
    private static string? FindServerDir()
    {
        DirectoryInfo? dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 10 && dir != null; i++)
        {
            string candidate = Path.Combine(dir.FullName, "server", "DurangoServer.csproj");
            if (File.Exists(candidate))
            {
                return Path.Combine(dir.FullName, "server");
            }
            dir = dir.Parent;
        }
        return null;
    }

    private async Task OnLoadedAsync()
    {
        _serverDir = FindServerDir();
        _pathLabel.Text = _serverDir != null
            ? $"โฟลเดอร์เซิร์ฟ: {_serverDir}"
            : "หาโฟลเดอร์ server\\ ไม่เจอ — ต้องวาง DurangoServerController.exe ไว้ในโปรเจกต์ (ใต้ tools\\) หรือแก้ FindServerDir() เอง";

        await InitWebViewAsync();

        // ถ้ามีเซิร์ฟรันอยู่แล้ว (จากที่เปิดเองก่อนหน้า/จาก session อื่น) แค่ต่อ WebView2 เข้าไปดู
        // ไม่ต้องเปิดซ้ำ (เปิดซ้ำสองตัว = ตัวที่สอง bind พอร์ตไม่ได้ ตายเงียบ ๆ)
        if (await IsServerRespondingAsync())
        {
            Log("[gui] เจอเซิร์ฟที่รันอยู่แล้วที่พอร์ต " + GatewayPort + " — ต่อเข้าไปดูโดยไม่เปิดซ้ำ");
            SetStatus(true, $"เชื่อมต่อกับเซิร์ฟที่รันอยู่แล้ว (พอร์ต {GatewayPort})");
            _webView?.CoreWebView2?.Navigate(AdminUrl);
        }
        else if (_serverDir != null)
        {
            await StartServerAsync();
        }
    }

    private async Task InitWebViewAsync()
    {
        _webView = new WebView2 { Dock = DockStyle.Fill };
        _split.Panel1.Controls.Add(_webView);
        try
        {
            await _webView.EnsureCoreWebView2Async(null);
        }
        catch (Exception e)
        {
            Log("[gui] เปิด WebView2 ไม่สำเร็จ (อาจไม่มี Microsoft Edge WebView2 Runtime ลงในเครื่อง): " + e.Message);
            Log("[gui] โหลด runtime ได้ที่ https://developer.microsoft.com/microsoft-edge/webview2/ — ระหว่างนี้ยังคุมเซิร์ฟผ่านปุ่มด้านบน + ดู log ล่างนี้ได้ตามปกติ");
        }
    }

    private static async Task<bool> IsServerRespondingAsync()
    {
        try
        {
            using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(1.5) };
            HttpResponseMessage res = await client.GetAsync($"http://127.0.0.1:{GatewayPort}/admin/status");
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task StartServerAsync()
    {
        if (_serverDir == null)
        {
            MessageBox.Show(this, "หาโฟลเดอร์ server\\ ไม่เจอ — ดูข้อความใต้แถบเครื่องมือ", "เปิดเซิร์ฟไม่ได้",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_serverProcess is { HasExited: false })
        {
            return; // กันกดซ้ำ
        }

        string args = "run -- " + (_chkCheat.Checked ? "--enable-cheat" : "");
        Log($"[gui] เปิดเซิร์ฟ: dotnet {args}  (ที่ {_serverDir})");
        SetStatus(false, "กำลังเปิด...");

        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            Arguments = args,
            WorkingDirectory = _serverDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        Process proc = new() { StartInfo = psi, EnableRaisingEvents = true };
        proc.OutputDataReceived += (_, e) => { if (e.Data != null) OnServerLine(e.Data); };
        proc.ErrorDataReceived += (_, e) => { if (e.Data != null) OnServerLine("[stderr] " + e.Data); };
        proc.Exited += (_, _) =>
        {
            Log("[gui] เซิร์ฟ process จบแล้ว (exit code " + SafeExitCode(proc) + ")");
            if (!IsDisposed) BeginInvoke(() => SetStatus(false, "หยุดอยู่"));
        };

        try
        {
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            _serverProcess = proc;
        }
        catch (Exception e)
        {
            Log("[gui] สั่งเปิดเซิร์ฟไม่สำเร็จ: " + e.Message);
            SetStatus(false, "เปิดไม่สำเร็จ");
            return;
        }

        // รอ gateway ขึ้นจริง (ไม่ใช่แค่ process เริ่ม — dotnet run ต้อง build ก่อนถ้ายังไม่ได้ build)
        for (int i = 0; i < 60; i++)
        {
            await Task.Delay(1000);
            if (_serverProcess is { HasExited: true })
            {
                Log("[gui] เซิร์ฟปิดตัวเองระหว่างเริ่ม — ดู log ด้านบนว่าทำไม (พอร์ตซ้ำ/build พัง ฯลฯ)");
                SetStatus(false, "เปิดไม่สำเร็จ");
                return;
            }
            if (await IsServerRespondingAsync())
            {
                Log("[gui] เซิร์ฟพร้อมแล้ว — เปิดแผงควบคุม");
                SetStatus(true, $"ทำงานอยู่ (PID {_serverProcess?.Id})");
                _webView?.CoreWebView2?.Navigate(AdminUrl);
                return;
            }
        }
        Log("[gui] รอเซิร์ฟตอบสนองนานเกิน 60 วิ — เช็ค log ด้านบน (อาจกำลัง build ครั้งแรกอยู่ รอเพิ่มได้)");
    }

    private static int SafeExitCode(Process p)
    {
        try { return p.ExitCode; } catch { return -1; }
    }

    private void OnServerLine(string line)
    {
        Log(line);
        if (line.Contains("[fatal]", StringComparison.Ordinal))
        {
            BeginInvoke(() => SetStatus(false, "พัง — ดู log (อาจมีเซิร์ฟรันอยู่แล้วที่พอร์ตนี้)"));
        }
    }

    private void StopServer()
    {
        if (_serverProcess is not { HasExited: false })
        {
            SetStatus(false, "หยุดอยู่");
            return;
        }
        Log("[gui] สั่งปิดเซิร์ฟ (kill process tree — เหมือนปิดด้วย taskkill /F, autosave ล่าสุดอาจหายได้ถึง 60 วิ)");
        try
        {
            _serverProcess.Kill(entireProcessTree: true);
        }
        catch (Exception e)
        {
            Log("[gui] สั่งปิดไม่สำเร็จ: " + e.Message);
        }
        SetStatus(false, "หยุดอยู่");
    }

    private async Task RestartServerAsync()
    {
        StopServer();
        await Task.Delay(1500); // ให้พอร์ตคืนก่อนเปิดใหม่ (TIME_WAIT สั้น ๆ บน localhost ปกติเร็ว แต่กันเผื่อ)
        await StartServerAsync();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        // ปิดโปรแกรมควบคุม ไม่ได้แปลว่าต้องปิดเซิร์ฟตาม (อาจอยากให้เซิร์ฟรันต่อ) — ถามก่อนเสมอถ้ายังรันอยู่
        if (_serverProcess is { HasExited: false })
        {
            DialogResult r = MessageBox.Show(this,
                "เซิร์ฟยังรันอยู่ — ปิดเซิร์ฟด้วยไหม?\n(เลือก \"No\" = ปิดแค่หน้าต่างนี้ เซิร์ฟรันต่อเบื้องหลัง)",
                "เซิร์ฟยังทำงานอยู่", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (r == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (r == DialogResult.Yes)
            {
                StopServer();
            }
        }
    }
}
