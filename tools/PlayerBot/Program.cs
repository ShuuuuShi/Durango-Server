using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;

// PlayerBot — บังคับตัวละครจริงในเกม DurangoV2
// โหมด:
//   PlayerBot.exe [นาที]              เดินสุ่ม WASD + E/F1 เก็บของ (default 3 นาที)
//   PlayerBot.exe moveto <x> <y> [entityId]   คลิกขวาให้ตัวละครเดินไปพิกัด x,y
//                                    (ใช้ระบบคลิกขวา=เดินของเกมเอง + อ่านตำแหน่งจาก /debug/players)
//   PlayerBot.exe --find              แสดงพิกัดหน้าต่างเกม
//
// ต้องมี server รันอยู่ที่ 127.0.0.1:8190 (moveto ใช้ /debug/players)

internal static class Program
{
    [DllImport("user32.dll")] private static extern bool EnumWindows(EnumWindowsProc cb, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);

    [DllImport("user32.dll")] private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")] private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")] private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")] private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")] private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")] private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

    [DllImport("user32.dll")] private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT { public int Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll")] private static extern uint SendInput(uint nInputs, INPUT[] inputs, int cbSize);

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    private const byte VK_W = 0x57, VK_A = 0x41, VK_S = 0x53, VK_D = 0x44, VK_E = 0x45, VK_F1 = 0x70;

    private const string ServerUrl = "http://127.0.0.1:8190";

    private static readonly Random _rng = new Random();
    private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };

    private static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "moveto")
        {
            return RunMoveTo(args.Skip(1).ToArray());
        }

        double minutes = 3.0;
        foreach (string a in args)
        {
            if (double.TryParse(a, out double m)) minutes = m;
            else if (a == "--find")
            {
                IntPtr h = FindGameWindow();
                if (h == IntPtr.Zero) { Console.WriteLine("ไม่เจอหน้าต่าง DurangoV2"); return 1; }
                GetWindowRect(h, out RECT r);
                Console.WriteLine($"hwnd=0x{h.ToInt64():X} rect=({r.Left},{r.Top})-({r.Right},{r.Bottom})");
                return 0;
            }
        }
        return RunRandomWalk(minutes);
    }

    // ---------------------------------------------------------------- moveto --
    // ใช้ hook ที่ patch ลงในเกม: เขียนไฟล์ moveto-cmd.bin (float x, float z 8 ไบต์)
    // เกมตรวจใน PlayerController.Update แล้วเรียก MoveToPosition ตรง ๆ — ไม่ต้องคลิก/ไม่ต้อง focus
    // ตัวละครเดินเองพร้อม pathfinding จริงของเกม (เหมือนคลิกขวา)

    private static string CmdPath()
    {
        // tools/PlayerBot/bin/.../PlayerBot.exe -> workspace/game/moveto-cmd.bin
        string root = AppContext.BaseDirectory;
        for (int i = 0; i < 8 && !string.IsNullOrEmpty(root); i++)
        {
            string gameDir = Path.Combine(root, "game");
            if (Directory.Exists(Path.Combine(gameDir, "DurangoV2_Data")))
            {
                return Path.Combine(gameDir, "moveto-cmd.bin");
            }
            root = Directory.GetParent(root)?.FullName;
        }
        return null;
    }

    private static int RunMoveTo(string[] args)
    {
        if (args.Length < 2 || !float.TryParse(args[0], out float targetX) || !float.TryParse(args[1], out float targetY))
        {
            Console.WriteLine("ใช้: PlayerBot moveto <x> <y> [entityId]");
            return 2;
        }
        string entityId = args.Length > 2 ? args[2] : null;

        string cmdPath = CmdPath();
        if (cmdPath == null)
        {
            Console.WriteLine("[moveto] หาโฟลเดอร์เกมไม่เจอ (หา DurangoV2_Data จากที่อยู่ของ PlayerBot)");
            return 1;
        }

        (string id, string name, float x, float y)? player = FindPlayer(entityId);
        if (player == null)
        {
            Console.WriteLine("[moveto] ไม่เจอผู้เล่นออนไลน์ (เช็คว่าเข้าเกมแล้วและ server รันอยู่)");
            return 1;
        }
        entityId = player.Value.id;
        Console.WriteLine($"[moveto] ผู้เล่น: {player.Value.name} อยู่ที่ ({player.Value.x:F0}, {player.Value.y:F0})");
        Console.WriteLine($"[moveto] เป้าหมาย: ({targetX:F0}, {targetY:F0}) ระยะ {Dist(player.Value.x, player.Value.y, targetX, targetY):F0}");

        double endAt = Environment.TickCount64 / 1000.0 + 150.0;
        double nextSend = 0;
        while (Environment.TickCount64 / 1000.0 < endAt)
        {
            player = FindPlayer(entityId);
            if (player == null)
            {
                Console.WriteLine("[moveto] ผู้เล่นหลุดจากเกม — หยุด");
                return 1;
            }
            double dist = Dist(player.Value.x, player.Value.y, targetX, targetY);
            if (dist <= 250.0)
            {
                Console.WriteLine($"[moveto] ถึงแล้ว ({player.Value.x:F0}, {player.Value.y:F0})");
                return 0;
            }

            double now = Environment.TickCount64 / 1000.0;
            if (now >= nextSend)
            {
                // เขียนคำสั่งเดินให้เกม (x, z ของ world) — เกมจะลบไฟล์เองเมื่อรับคำสั่ง
                try
                {
                    string tempPath = cmdPath + ".tmp";
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        byte[] buf = new byte[8];
                        BitConverter.GetBytes(targetX).CopyTo(buf, 0);
                        BitConverter.GetBytes(targetY).CopyTo(buf, 4);
                        fs.Write(buf, 0, 8);
                    }
                    File.Move(tempPath, cmdPath, true);
                    Console.WriteLine($"[moveto] สั่งเดินไป ({targetX:F0}, {targetY:F0}) เหลือ {dist:F0}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[moveto] เขียนไฟล์คำสั่งไม่ได้: " + e.Message);
                    return 1;
                }
                nextSend = now + 4000.0;   // ถ้ายังไม่ถึงใน 4 วิ สั่งซ้ำ
            }

            Thread.Sleep(400);
        }

        Console.WriteLine("[moveto] หมดเวลา ยังไม่ถึงเป้า");
        return 1;
    }

    private static (string, string, float, float)? FindPlayer(string entityId)
    {
        try
        {
            string json = _http.GetStringAsync(ServerUrl + "/debug/players").Result;
            using JsonDocument doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("players", out JsonElement arr)) return null;
            JsonElement fallback = default;
            bool hasFallback = false;
            foreach (JsonElement p in arr.EnumerateArray())
            {
                string id = p.GetProperty("entity_id").GetString();
                if (id == entityId)
                {
                    return (id, p.GetProperty("name").GetString(), p.GetProperty("x").GetSingle(), p.GetProperty("y").GetSingle());
                }
                if (!hasFallback)
                {
                    fallback = p;
                    hasFallback = true;
                }
            }
            if (!string.IsNullOrEmpty(entityId)) return null;
            if (hasFallback)
            {
                return (fallback.GetProperty("entity_id").GetString(), fallback.GetProperty("name").GetString(),
                        fallback.GetProperty("x").GetSingle(), fallback.GetProperty("y").GetSingle());
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    // ------------------------------------------------------------ random walk --

    private static int RunRandomWalk(double minutes)
    {
        IntPtr hwnd = FindGameWindow();
        if (hwnd == IntPtr.Zero)
        {
            Console.WriteLine("[playerbot] ไม่เจอหน้าต่าง DurangoV2 — เปิดเกมก่อน");
            return 1;
        }
        RECT rect = PrepareWindow(hwnd);
        Console.WriteLine($"[playerbot] เจอหน้าต่างเกม rect=({rect.Left},{rect.Top})-({rect.Right},{rect.Bottom})");
        Console.WriteLine($"[playerbot] วน: เดิน WASD -> หยุด -> E เล็งของใกล้สุด -> F1 เก็บ เป็นเวลา {minutes} นาที");
        Console.WriteLine("[playerbot] เริ่มใน 3 วินาที... อย่าแตะเมาส์/คีย์บอร์ด กด Ctrl+C เพื่อหยุด");
        Thread.Sleep(3000);
        FocusGame(hwnd);

        double endAt = Environment.TickCount64 / 1000.0 + minutes * 60.0;
        double nextDirChange = 0;
        double releaseAt = 0;
        double nextInteract = 0;
        double nextRefocus = 0;
        double interactStep = 0;   // 0=ว่าง 1=รอเมนูเปิดหลัง E 2=รอหลัง F1
        byte[] held = Array.Empty<byte>();

        while (Environment.TickCount64 / 1000.0 < endAt)
        {
            double now = Environment.TickCount64 / 1000.0;

            if (now >= nextRefocus)
            {
                // กัน focus หลุดไปหน้าต่างอื่น (เช่น terminal) — ไม่งั้นคีย์ส่งไม่ถึงเกม
                if (GetForegroundWindow() != hwnd)
                {
                    FocusGame(hwnd);
                    Thread.Sleep(120);
                }
                nextRefocus = now + 2.0;
            }

            if (now >= nextDirChange)
            {
                ReleaseKeys(held);
                held = Array.Empty<byte>();
                if (_rng.NextDouble() < 0.25)
                {
                    // หยุดยืนเฉย ๆ (ช่วงนี้จะกด E+F1 เก็บของ)
                    nextDirChange = now + 1.5 + _rng.NextDouble() * 1.5;
                }
                else
                {
                    held = PickDirection();
                    PressKeys(held);
                    releaseAt = now + 0.6 + _rng.NextDouble() * 1.4;
                    nextDirChange = releaseAt + (_rng.NextDouble() < 0.2 ? 0.3 + _rng.NextDouble() * 0.7 : 0);
                }
            }
            else if (held.Length > 0 && now >= releaseAt)
            {
                ReleaseKeys(held);
                held = Array.Empty<byte>();
            }

            // เก็บของเฉพาะตอนยืนนิ่ง (เมนูวงกลมจะปิดเองถ้าตัวละครกำลังเดิน)
            if (held.Length == 0 && now >= nextInteract)
            {
                switch (interactStep)
                {
                    case 0:
                        TapKey(VK_E);            // เล็งของใกล้สุด -> เปิดเมนูวงกลม (ส่ง Touch)
                        interactStep = 1;
                        nextInteract = now + 1.3;
                        break;
                    case 1:
                        TapKey(VK_F1);           // เมนูช่องแรก = เก็บ (Collect)
                        interactStep = 2;
                        nextInteract = now + 0.8;
                        break;
                    default:
                        TapKey(VK_E);            // กด E อีกทีปิดเมนูถ้ายังค้าง
                        interactStep = 0;
                        nextInteract = now + 1.0 + _rng.NextDouble() * 2.0;
                        break;
                }
            }

            Thread.Sleep(40);
        }

        ReleaseKeys(held);
        Console.WriteLine("[playerbot] เสร็จแล้ว");
        return 0;
    }

    // ---------------------------------------------------------------- helpers --

    private static double Dist(float x1, float y1, float x2, float y2)
    {
        double dx = x2 - x1, dy = y2 - y1;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // บังคับ focus เข้าหน้าต่างเกม — ใช้ AttachThreadInput trick เพราะ Windows
    // ปกติห้าม process หลังฉากขโมย focus (ไม่งั้น WASD ไปเข้าหน้าต่างอื่นหมด)
    private static void FocusGame(IntPtr hwnd)
    {
        ShowWindow(hwnd, 9);              // SW_RESTORE
        uint fgThread = 0;
        IntPtr fg = GetForegroundWindow();
        if (fg != IntPtr.Zero)
        {
            GetWindowThreadProcessId(fg, out fgThread);
        }
        GetWindowThreadProcessId(hwnd, out uint targetThread);
        AttachThreadInput(targetThread, fgThread, true);
        BringWindowToTop(hwnd);
        SetForegroundWindow(hwnd);
        AttachThreadInput(targetThread, fgThread, false);
    }

    // คืนหน้าต่างเกมคืนสภาพ: restore + focus + ขยายให้ได้ขนาดใช้งาน (กันโดนย่อจนคลิกไม่โดน)
    private static RECT PrepareWindow(IntPtr hwnd)
    {
        ShowWindow(hwnd, 9);
        FocusGame(hwnd);
        Thread.Sleep(400);
        GetWindowRect(hwnd, out RECT rect);
        int w = rect.Right - rect.Left;
        int h = rect.Bottom - rect.Top;
        if (w < 700 || h < 500)
        {
            MoveWindow(hwnd, 546, 268, 827, 544, true);
            Thread.Sleep(600);
            GetWindowRect(hwnd, out rect);
            Console.WriteLine($"[playerbot] ขยายหน้าต่างเกมเป็น {rect.Right - rect.Left}x{rect.Bottom - rect.Top} ที่ ({rect.Left},{rect.Top})");
        }
        return rect;
    }

    private static byte[] PickDirection()
    {
        var keys = new List<byte>();
        // 8 ทิศทางจาก WASD
        int dir = _rng.Next(8);
        if ((dir & 1) != 0) keys.Add(VK_W);
        else if (dir == 0) keys.Add(VK_W);
        if ((dir & 2) != 0) keys.Add(VK_A);
        if ((dir & 4) != 0) keys.Add(VK_S);
        if (dir == 5 || dir == 7) keys.Add(VK_D);
        if (keys.Count == 0) keys.Add(VK_W);
        // ลบซ้ำ: กัน W+S พร้อมกัน (ชนกันเอง)
        if (keys.Contains(VK_W) && keys.Contains(VK_S)) keys.Remove(VK_S);
        if (keys.Contains(VK_A) && keys.Contains(VK_D)) keys.Remove(VK_D);
        return keys.ToArray();
    }

    private static void PressKeys(byte[] keys)
    {
        if (keys.Length == 0) return;
        INPUT[] inputs = new INPUT[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            inputs[i].type = INPUT_KEYBOARD;
            inputs[i].U.ki.wVk = keys[i];
        }
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static void ReleaseKeys(byte[] keys)
    {
        if (keys.Length == 0) return;
        INPUT[] inputs = new INPUT[keys.Length];
        for (int i = 0; i < keys.Length; i++)
        {
            inputs[i].type = INPUT_KEYBOARD;
            inputs[i].U.ki.wVk = keys[i];
            inputs[i].U.ki.dwFlags = KEYEVENTF_KEYUP;
        }
        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static void TapKey(byte vk)
    {
        INPUT[] inputs = new INPUT[2];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].U.ki.wVk = vk;
        inputs[1].type = INPUT_KEYBOARD;
        inputs[1].U.ki.wVk = vk;
        inputs[1].U.ki.dwFlags = KEYEVENTF_KEYUP;
        SendInput(2, inputs, Marshal.SizeOf<INPUT>());
    }

    private static IntPtr FindGameWindow()
    {
        IntPtr found = IntPtr.Zero;
        uint[] pids = Array.Empty<uint>();
        foreach (Process p in Process.GetProcessesByName("DurangoV2"))
        {
            pids = pids.Length == 0 ? new[] { (uint)p.Id } : Append(pids, (uint)p.Id);
        }
        if (pids.Length == 0) return IntPtr.Zero;

        EnumWindows((h, l) =>
        {
            GetWindowThreadProcessId(h, out uint pid);
            if (Array.IndexOf(pids, pid) >= 0 && IsVisible(h))
            {
                found = h;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    private static bool IsVisible(IntPtr h)
    {
        return GetWindowText(h, new StringBuilder(256), 256) > 0;
    }

    private static uint[] Append(uint[] arr, uint v)
    {
        uint[] next = new uint[arr.Length + 1];
        Array.Copy(arr, next, arr.Length);
        next[arr.Length] = v;
        return next;
    }
}
