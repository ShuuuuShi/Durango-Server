using System.Reflection;
using DurangoServer.Modding;
using Messages;

namespace DurangoServer.Core;

// ============================================================================
// PluginManager — ระบบโหลด mod (24 ส.ค. 2026, เจ้าของสั่งให้ทำระบบ mod เต็มรูปแบบ
// เหมือน Minecraft/Bukkit ฝั่งเซิร์ฟ)
//
// วิธีทำงาน: สแกนไฟล์ .dll ทุกไฟล์ในโฟลเดอร์ mods/ (ข้าง ๆ DurangoServer.exe) โหลดด้วย
// Assembly.LoadFrom แล้วหา type ที่ implement IGamePlugin (นิยามอยู่ที่โปรเจกต์แยก
// mod-sdk/DurangoModSdk.csproj — mod ภายนอกอ้างอิงแค่ .dll เล็ก ๆ ตัวนั้น ไม่ต้องพ่วง
// DurangoServer.dll ทั้งก้อน) สร้าง instance แล้วเรียก OnLoad() ให้ mod ลงทะเบียนคำสั่ง/
// event handler ผ่าน IModApi
//
// ดูวิธีเขียน mod + ตัวอย่างที่ docs/server/Modding.md และ tools/ExampleMod/
// ============================================================================

public sealed class PluginManager
{
    public static PluginManager? Instance { get; private set; }

    private readonly ServerWorld _world;

    // ชื่อคำสั่ง (ไม่สนตัวพิมพ์) → (ชื่อ mod เจ้าของ, handler)
    // ⚠️ เช็คชนกันเฉพาะระหว่าง mod ด้วยกันเอง — ชนกับคำสั่งในตัวเซิร์ฟ (give/heal/tp/...) ตรวจไม่ได้
    // จากตรงนี้ (ไม่ได้ hardcode รายชื่อคำสั่งในตัวไว้ ป้องกันหลุดตกยุคเวลาเพิ่มคำสั่งใหม่) — HandleCheat
    // ลองคำสั่งในตัวก่อนเสมอ ชื่อชนกับคำสั่งในตัว mod จะไม่มีวันถูกเรียกเลย ดู log ตอนโหลดถ้าสงสัย
    private readonly Dictionary<string, (string ModName, Func<IModPlayer, string[], string> Handler)> _commands
        = new(StringComparer.OrdinalIgnoreCase);

    private readonly List<Action<IModPlayer>> _joinHandlers = new();
    private readonly List<Action<IModPlayer>> _leaveHandlers = new();
    private readonly List<Action<double>> _tickHandlers = new();

    // [แก้เอง] 24 ส.ค. 2026 — เจ้าของสั่งให้มีหน้าดูสถานะ mod ใน admin panel ("รู้มอดโหลดจริงไหม")
    // เก็บสถานะการโหลดของ mod ทุกตัวไว้ (ทั้งที่โหลดสำเร็จและพัง) ให้ Gateway.Admin.cs อ่านออกไปได้
    private readonly List<LoadedModInfo> _mods = new();
    public string ModsDir { get; private set; } = "";
    public bool ModsDirExists { get; private set; }

    /// <summary>รายการ mod ทั้งหมดที่พยายามโหลด (สำเร็จ+ไม่สำเร็จ) — /admin/mods อ่านตัวนี้</summary>
    public IReadOnlyList<LoadedModInfo> Mods => _mods;

    private PluginManager(ServerWorld world)
    {
        _world = world;
    }

    /// <summary>สถานะของ mod หนึ่งตัวสำหรับแสดงผล (admin panel / debug) — คนละคลาสกับ IGamePlugin จริง
    /// เพราะต้องรอดแม้ mod โยน exception ตอนสร้าง/โหลด (Name/Version อาจว่างถ้าพังตั้งแต่ก่อนอ่านได้)</summary>
    public sealed class LoadedModInfo
    {
        public string Name = "";
        public string Version = "";
        public string SourceFile = "";
        public bool Loaded;              // true = OnLoad() จบโดยไม่ throw
        public string? Error;            // ข้อความ exception ถ้า Loaded == false
        public List<string> Commands { get; } = new();
        public bool HasPlayerJoinedHook;
        public bool HasPlayerLeftHook;
        public bool HasTickHook;
    }

    /// <summary>เรียกครั้งเดียวตอนเซิร์ฟบูต หลัง world.Load() — สแกน+โหลด mod ทั้งหมดใน modsDir</summary>
    public static void LoadAll(ServerWorld world, string modsDir)
    {
        PluginManager mgr = new PluginManager(world);
        Instance = mgr;
        mgr.LoadInternal(modsDir);
    }

    /// <summary>mod ตัวหนึ่งที่ผ่านขั้นตอน "สร้าง instance ได้แล้ว" กำลังรอไล่ 3 เฟส</summary>
    private sealed class PendingMod
    {
        public IGamePlugin Plugin = null!;
        public LoadedModInfo Info = null!;
        public ModApiView Api = null!;
        public bool Failed;
    }

    private void LoadInternal(string modsDir)
    {
        ModsDir = modsDir;
        ModsDirExists = Directory.Exists(modsDir);
        if (!ModsDirExists)
        {
            Console.WriteLine($"[mods] ไม่มีโฟลเดอร์ '{modsDir}' — ข้าม (ปกติ ไม่ error, สร้างโฟลเดอร์แล้วใส่ .dll ถ้าอยากใช้ mod)");
            return;
        }
        string[] dlls = Directory.GetFiles(modsDir, "*.dll", SearchOption.TopDirectoryOnly);
        if (dlls.Length == 0)
        {
            Console.WriteLine($"[mods] โฟลเดอร์ '{modsDir}' ว่างเปล่า — ไม่มี mod ให้โหลด");
            return;
        }

        // ── ขั้นที่ 1: สแกน+สร้าง instance ของทุก mod ก่อน (ยังไม่เรียกเฟสไหนทั้งนั้น) ──
        List<PendingMod> pending = new List<PendingMod>();
        foreach (string dllPath in dlls)
        {
            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(dllPath);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[mods] โหลดไฟล์ {Path.GetFileName(dllPath)} ไม่สำเร็จ: {e.Message}");
                _mods.Add(new LoadedModInfo { SourceFile = Path.GetFileName(dllPath), Error = "โหลดไฟล์ .dll ไม่สำเร็จ: " + e.Message });
                continue;
            }
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                // dependency ของ mod หายไปบางตัว — โหลด type ที่พอโหลดได้ต่อไป ไม่ทิ้งทั้งไฟล์
                types = Array.FindAll(e.Types, t => t != null)!;
                Console.WriteLine($"[mods] {Path.GetFileName(dllPath)}: บาง type โหลดไม่ได้ ({e.LoaderExceptions.Length} error) — โหลดเท่าที่ทำได้");
            }
            bool foundAny = false;
            foreach (Type type in types)
            {
                if (type.IsInterface || type.IsAbstract || !typeof(IGamePlugin).IsAssignableFrom(type))
                {
                    continue;
                }
                foundAny = true;
                LoadedModInfo info = new LoadedModInfo { SourceFile = Path.GetFileName(dllPath) };
                _mods.Add(info);
                IGamePlugin plugin;
                try
                {
                    plugin = (IGamePlugin)Activator.CreateInstance(type)!;
                }
                catch (Exception e)
                {
                    info.Name = type.Name;
                    info.Error = "สร้าง instance ไม่สำเร็จ: " + e.Message;
                    Console.WriteLine($"[mods] สร้าง instance {type.FullName} ไม่สำเร็จ: {e.Message}");
                    continue;
                }
                info.Name = plugin.Name;
                info.Version = plugin.Version;
                pending.Add(new PendingMod { Plugin = plugin, Info = info, Api = new ModApiView(this, info) });
            }
            if (!foundAny)
            {
                Console.WriteLine($"[mods] {Path.GetFileName(dllPath)}: ไม่มี type ไหน implement IGamePlugin เลย (ไม่ใช่ mod หรือแค่ dependency ของ mod อื่น) — ข้าม");
            }
        }

        // ── ขั้นที่ 2: ไล่ 3 เฟสทีละเฟส "ครบทุก mod ก่อนขึ้นเฟสถัดไป" (แบบ Minecraft/Forge — ดู
        //     comment เต็มที่ IGamePlugin.cs ว่าทำไมต้องแยกแบบนี้ ไม่ใช่ mod ละ 3 เฟสรวด) ──
        RunPhase(pending, "PreLoad", (p, api) => p.OnPreLoad(api));
        RunPhase(pending, "Load", (p, api) => p.OnLoad(api));
        RunPhase(pending, "PostLoad", (p, api) => p.OnPostLoad(api));

        int loaded = 0;
        foreach (PendingMod pm in pending)
        {
            if (!pm.Failed)
            {
                pm.Info.Loaded = true;
                loaded++;
                Console.WriteLine($"[mods] โหลด '{pm.Plugin.Name}' v{pm.Plugin.Version} จาก {pm.Info.SourceFile} สำเร็จ (ครบ 3 เฟส)");
            }
        }
        Console.WriteLine($"[mods] โหลดสำเร็จ {loaded} mod จาก {dlls.Length} ไฟล์ .dll ใน '{modsDir}'");
    }

    private static void RunPhase(List<PendingMod> pending, string phaseName, Action<IGamePlugin, IModApi> call)
    {
        foreach (PendingMod pm in pending)
        {
            if (pm.Failed)
            {
                continue; // เฟสก่อนหน้าพังไปแล้ว ข้ามเฟสที่เหลือของ mod นี้ (แต่ mod อื่นเดินหน้าต่อตามปกติ)
            }
            try
            {
                call(pm.Plugin, pm.Api);
            }
            catch (Exception e)
            {
                pm.Failed = true;
                pm.Info.Error = $"{phaseName}: {e.Message}";
                Console.WriteLine($"[mods] '{pm.Plugin.Name}' โยน exception ตอน {phaseName} — ปิดใช้งาน mod นี้: {e}");
            }
        }
    }

    // ── มุมมอง IModApi เฉพาะของ mod แต่ละตัว (ไม่ implement ตรง ๆ ที่ PluginManager
    //     เพราะ Log() ต้องรู้ว่าเป็น mod ไหนเรียก — ถ้าใช้ field ตัวเดียวใช้ร่วมกันจะผิดชื่อได้เวลา
    //     mod เรียก api.Log() จาก handler ที่ทำงานทีหลัง ไม่ใช่ตอน OnLoad) ──
    private sealed class ModApiView : IModApi
    {
        private readonly PluginManager _mgr;
        private readonly LoadedModInfo _info;

        public ModApiView(PluginManager mgr, LoadedModInfo info)
        {
            _mgr = mgr;
            _info = info;
        }

        public void Log(string message) => Console.WriteLine($"[mod:{_info.Name}] {message}");

        public bool RegisterCommand(string verb, Func<IModPlayer, string[], string> handler)
        {
            bool ok = _mgr.RegisterCommandInternal(verb, _info.Name, handler);
            if (ok) _info.Commands.Add(verb);
            return ok;
        }

        public void OnPlayerJoined(Action<IModPlayer> handler)
        {
            _mgr._joinHandlers.Add(handler);
            _info.HasPlayerJoinedHook = true;
        }

        public void OnPlayerLeft(Action<IModPlayer> handler)
        {
            _mgr._leaveHandlers.Add(handler);
            _info.HasPlayerLeftHook = true;
        }

        public void OnTick(Action<double> handler)
        {
            _mgr._tickHandlers.Add(handler);
            _info.HasTickHook = true;
        }

        public IReadOnlyList<IModPlayer> GetOnlinePlayers() => _mgr.GetOnlinePlayersInternal();
        public IModPlayer? FindPlayer(string nameOrEntityId) => _mgr.FindPlayerInternal(nameOrEntityId);
        public void BroadcastMessage(string text) => _mgr.BroadcastInternal(text);
    }

    private bool RegisterCommandInternal(string verb, string modName, Func<IModPlayer, string[], string> handler)
    {
        if (string.IsNullOrWhiteSpace(verb))
        {
            return false;
        }
        if (_commands.ContainsKey(verb))
        {
            Console.WriteLine($"[mods] '{modName}' ลงทะเบียนคำสั่ง '{verb}' ไม่สำเร็จ — ชื่อชนกับ mod อื่นที่ลงทะเบียนไปแล้ว");
            return false;
        }
        _commands[verb] = (modName, handler);
        return true;
    }

    private IReadOnlyList<IModPlayer> GetOnlinePlayersInternal()
    {
        ServerPlayer[] all = _world.SnapshotPlayers();
        List<IModPlayer> list = new List<IModPlayer>(all.Length);
        foreach (ServerPlayer p in all)
        {
            list.Add(new ServerModPlayer(p));
        }
        return list;
    }

    private IModPlayer? FindPlayerInternal(string nameOrEntityId)
    {
        ServerPlayer? p = _world.FindPlayerByNameOrId(nameOrEntityId);
        return p == null ? null : new ServerModPlayer(p);
    }

    private void BroadcastInternal(string text)
    {
        foreach (ServerPlayer p in _world.SnapshotPlayers())
        {
            p.Send(new Info { Text = text });
        }
    }

    // ── เรียกจากฝั่งเซิร์ฟ (ไม่ใช่ส่วนของ IModApi — mod เรียกไม่ได้) ──────────────

    /// <summary>HandleCheat เรียกอันนี้เมื่อ verb ไม่ตรงกับคำสั่งในตัวสักอัน — คืน true ถ้ามี mod รับคำสั่งนี้</summary>
    public bool TryRunCommand(string verb, ServerPlayer caller, string[] args, out string reply)
    {
        if (_commands.TryGetValue(verb, out (string ModName, Func<IModPlayer, string[], string> Handler) entry))
        {
            try
            {
                reply = entry.Handler(new ServerModPlayer(caller), args);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[mods] คำสั่ง '{verb}' ของ '{entry.ModName}' โยน exception: {e}");
                reply = $"mod '{entry.ModName}' error ตอนรันคำสั่ง (ดู server log)";
            }
            return true;
        }
        reply = "";
        return false;
    }

    public void FirePlayerJoined(ServerPlayer player)
    {
        if (_joinHandlers.Count == 0) return;
        ServerModPlayer mp = new ServerModPlayer(player);
        foreach (Action<IModPlayer> h in _joinHandlers) SafeInvoke("OnPlayerJoined", () => h(mp));
    }

    public void FirePlayerLeft(ServerPlayer player)
    {
        if (_leaveHandlers.Count == 0) return;
        ServerModPlayer mp = new ServerModPlayer(player);
        foreach (Action<IModPlayer> h in _leaveHandlers) SafeInvoke("OnPlayerLeft", () => h(mp));
    }

    public void FireTick(double dtSeconds)
    {
        if (_tickHandlers.Count == 0) return;
        foreach (Action<double> h in _tickHandlers) SafeInvoke("OnTick", () => h(dtSeconds));
    }

    private static void SafeInvoke(string hookName, Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[mods] {hookName} handler โยน exception: {e}");
        }
    }
}
