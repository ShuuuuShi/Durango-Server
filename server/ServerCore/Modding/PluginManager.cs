using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Security.Cryptography;
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
    // [V1.1] 27 ส.ค. 2026 — hook ตาย (ยิงจาก ServerPlayer.Die() เท่านั้น)
    private readonly List<Action<IModPlayer>> _diedHandlers = new();
    private readonly List<Action<double>> _tickHandlers = new();
    private readonly List<EventSubscription> _eventSubscriptions = new();
    private readonly List<IModStorage> _storages = new();
    internal MethodOverrideManager MethodOverrides { get; } = new();
    private readonly List<LoadedPluginRuntime> _loadedPluginRuntimes = new();
    private bool _disabled;
    private long _registrationSequence;
    private static readonly string[] AvailableEvents =
    {
        "player.joined", "player.left", "player.died", "player.revived", "server.tick",
        "inventory.added", "inventory.removed", "craft.before", "craft.completed", "craft.failed",
        "gather.before", "gather.completed", "butchery.before", "butchery.completed",
        "farm.before_plant", "farm.planted", "farm.before_harvest", "farm.harvested",
        "building.before_place", "building.placed", "building.before_complete", "building.completed", "building.before_destroy", "building.destroyed",
        "combat.before_attack", "combat.attack", "combat.before_damage", "combat.damage",
        "quest.progressed", "quest.completed",
        "progress.level_up", "progress.skill_learned", "travel.entered", "travel.leaving",
        "chat.message"
    };

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
        public string State = "Discovered";
        public bool HasManifest;
        public string PackageDirectory = "";
        public string? Error;            // ข้อความ exception ถ้า Loaded == false
        public List<string> Commands { get; } = new();
        public bool HasPlayerJoinedHook;
        public bool HasPlayerLeftHook;
        public bool HasTickHook;
        public bool HasPlayerDiedHook;   // [V1.1]
        public bool HasEventBus;
        public string Id = "";
        public string ApiVersion = "1.0";
        public List<string> Dependencies { get; } = new();
        public List<string> Events { get; } = new();
        public long EventErrors;
        public double EventMilliseconds;
        public long EventCalls;
        public long CommandCalls;
        public long CommandErrors;
        public long RateLimitedCalls;
        public string AssemblySha256 = "";
        public string ContentSha256 = "";
        public bool Required;
        public bool HasMethodOverrides;
        public List<string> MethodOverrides { get; } = new();
        public long MethodOverrideErrors;
        public long MethodOverrideCalls;
        public double MethodOverrideMilliseconds;
    }

    private sealed class EventSubscription
    {
        public string Name = "";
        public string ModId = "";
        public Action<IModEventContext> Handler = null!;
        public EventPriority Priority;
        public long Sequence;
    }

    private sealed class ModEventContext : IGameplayEvent
    {
        public string EventName { get; init; } = "";
        public string EventId { get; init; } = Guid.NewGuid().ToString("N");
        public double OccurredAt { get; init; }
        public IModPlayer? Player { get; init; }
        public bool IsBefore { get; init; }
        public bool IsCommitted { get; init; }
        public bool IsCancelled { get; private set; }
        public string? CancelReason { get; private set; }
        public IReadOnlyDictionary<string, string> Data { get; init; } = new Dictionary<string, string>();
        public string ActionId => EventName;
        public IReadOnlyDictionary<string, string> Values => Data;
        public void Cancel(string reason)
        {
            if (!IsBefore || IsCommitted) return;
            IsCancelled = true;
            CancelReason = string.IsNullOrWhiteSpace(reason) ? "cancelled by mod" : reason;
        }
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

    private sealed class LoadedPluginRuntime
    {
        public IGamePlugin Plugin = null!;
        public ModApiView Api = null!;
        public LoadedModInfo Info = null!;
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
        List<string> dlls = Directory.GetFiles(modsDir, "*.dll", SearchOption.TopDirectoryOnly)
            .OrderBy(x => Path.GetFileName(x), StringComparer.OrdinalIgnoreCase).ToList();
        foreach (string dir in Directory.GetDirectories(modsDir, "*", SearchOption.TopDirectoryOnly)
                     .OrderBy(x => Path.GetFileName(x), StringComparer.OrdinalIgnoreCase))
        {
            string mp = Path.Combine(dir, "mod.json");
            if (!File.Exists(mp)) continue;
            if (!ModManifest.TryRead(mp, out ModManifest mf, out string me))
            {
                _mods.Add(new LoadedModInfo { SourceFile = Path.Combine(Path.GetFileName(dir), "mod.json"), State = "Failed", Error = me });
                continue;
            }
            string ap = mf.AssemblyPath(dir);
            if (!File.Exists(ap))
            {
                _mods.Add(new LoadedModInfo { Id = mf.Id, Name = mf.Name, Version = mf.Version, SourceFile = Path.Combine(Path.GetFileName(dir), mf.Assembly), State = "Failed", Error = "assembly not found" });
                continue;
            }
            dlls.Add(ap);
        }
        dlls = dlls.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (dlls.Count == 0)
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
                _mods.Add(new LoadedModInfo { SourceFile = Path.GetFileName(dllPath), State = "Failed", Error = "โหลดไฟล์ .dll ไม่สำเร็จ: " + e.Message });
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
                string packageManifestPath = Path.Combine(Path.GetDirectoryName(dllPath) ?? "", "mod.json");
                ModManifest? packageManifest = null;
                if (File.Exists(packageManifestPath))
                    ModManifest.TryRead(packageManifestPath, out packageManifest, out string _);
                info.HasManifest = packageManifest != null;
                info.PackageDirectory = packageManifest == null ? "" : Path.GetDirectoryName(dllPath) ?? "";
                _mods.Add(info);
                IGamePlugin plugin;
                try
                {
                    plugin = (IGamePlugin)Activator.CreateInstance(type)!;
                }
                catch (Exception e)
                {
                    info.Name = type.Name;
                    info.State = "Failed";
                    info.Error = "สร้าง instance ไม่สำเร็จ: " + e.Message;
                    Console.WriteLine($"[mods] สร้าง instance {type.FullName} ไม่สำเร็จ: {e.Message}");
                    continue;
                }
                info.Name = plugin.Name;
                info.Version = plugin.Version;
                IModIdentity? identity = plugin as IModIdentity;
                info.Id = identity?.Id ?? plugin.Name;
                info.ApiVersion = identity?.ApiVersion ?? "1.0";
                if (packageManifest != null)
                {
                    info.Id = packageManifest.Id; info.Name = packageManifest.Name; info.Version = packageManifest.Version;
                    info.ApiVersion = packageManifest.ApiVersion;
                    info.Dependencies.AddRange(packageManifest.Dependencies);
                    info.Required = packageManifest.Required;
                }
                if (identity != null && packageManifest == null)
                {
                    info.Dependencies.AddRange(identity.Dependencies ?? Array.Empty<string>());
                }
                try
                {
                    info.AssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dllPath))).ToLowerInvariant();
                    if (packageManifest != null && !string.IsNullOrWhiteSpace(packageManifest.Sha256) && !string.Equals(info.AssemblySha256, packageManifest.Sha256, StringComparison.OrdinalIgnoreCase))
                    { info.State = "Failed"; info.Error = "assembly sha256 mismatch"; continue; }
                    if (packageManifest != null)
                    {
                        ModContentPack content = ModContentPack.Load(Path.GetDirectoryName(dllPath) ?? "", info.Id);
                        info.ContentSha256 = content.ContentHash;
                        if (!string.IsNullOrWhiteSpace(packageManifest.ContentSha256) && !string.Equals(info.ContentSha256, packageManifest.ContentSha256, StringComparison.OrdinalIgnoreCase))
                        { info.State = "Failed"; info.Error = "content_sha256 mismatch"; continue; }
                        if (content.Errors.Count != 0)
                        { info.State = "Failed"; info.Error = "content validation: " + string.Join("; ", content.Errors.Take(3)); continue; }
                    }
                }
                catch (Exception e)
                { info.State = "Failed"; info.Error = "package validation failed: " + e.Message; continue; }
                pending.Add(new PendingMod { Plugin = plugin, Info = info, Api = new ModApiView(this, info) });
            }
            if (!foundAny)
            {
                Console.WriteLine($"[mods] {Path.GetFileName(dllPath)}: ไม่มี type ไหน implement IGamePlugin เลย (ไม่ใช่ mod หรือแค่ dependency ของ mod อื่น) — ข้าม");
            }
        }

        // ── ขั้นที่ 2: ไล่ 3 เฟสทีละเฟส "ครบทุก mod ก่อนขึ้นเฟสถัดไป" (แบบ Minecraft/Forge — ดู
        //     comment เต็มที่ IGamePlugin.cs ว่าทำไมต้องแยกแบบนี้ ไม่ใช่ mod ละ 3 เฟสรวด) ──
        List<PendingMod> loadOrder = OrderByDependencies(pending);
        RunPhase(loadOrder, "PreLoad", (p, api) => p.OnPreLoad(api));
        RunPhase(loadOrder, "Load", (p, api) => p.OnLoad(api));
        RunPhase(loadOrder, "PostLoad", (p, api) => p.OnPostLoad(api));

        int loaded = 0;
        foreach (PendingMod pm in loadOrder)
        {
            if (!pm.Failed)
            {
                pm.Info.Loaded = true;
                pm.Info.State = "Loaded";
                _loadedPluginRuntimes.Add(new LoadedPluginRuntime { Plugin = pm.Plugin, Api = pm.Api, Info = pm.Info });
                loaded++;
                Console.WriteLine($"[mods] โหลด '{pm.Plugin.Name}' v{pm.Plugin.Version} จาก {pm.Info.SourceFile} สำเร็จ (ครบ 3 เฟส)");
            }
        }
        Console.WriteLine($"[mods] โหลดสำเร็จ {loaded} mod จาก {dlls.Count} ไฟล์ .dll ใน '{modsDir}'");
    }

    private static List<PendingMod> OrderByDependencies(List<PendingMod> mods)
    {
        var map = new Dictionary<string, PendingMod>(StringComparer.OrdinalIgnoreCase);
        foreach (PendingMod pm in mods)
        {
            if (!string.IsNullOrWhiteSpace(pm.Info.Id)) map[pm.Info.Id] = pm;
        }
        var result = new List<PendingMod>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        bool Visit(PendingMod pm)
        {
            if (pm.Failed) return false;
            if (visited.Contains(pm.Info.Id)) return true;
            if (!visiting.Add(pm.Info.Id))
            {
                pm.Failed = true; pm.Info.State = "Blocked"; pm.Info.Error = "dependency cycle"; return false;
            }
            foreach (string depId in pm.Info.Dependencies)
            {
                if (!map.TryGetValue(depId, out PendingMod? dep))
                {
                    pm.Failed = true; pm.Info.State = "Blocked"; pm.Info.Error = $"missing dependency '{depId}'";
                    visiting.Remove(pm.Info.Id); return false;
                }
                if (!Visit(dep))
                {
                    pm.Failed = true; pm.Info.State = "Blocked"; pm.Info.Error ??= $"dependency '{depId}' is unavailable";
                    visiting.Remove(pm.Info.Id); return false;
                }
            }
            visiting.Remove(pm.Info.Id); visited.Add(pm.Info.Id); pm.Info.State = "Ready"; result.Add(pm); return true;
        }
        foreach (PendingMod pm in mods) Visit(pm);
        return result;
    }
    private void RunPhase(List<PendingMod> pending, string phaseName, Action<IGamePlugin, IModApi> call)
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
                pm.Info.State = "Failed";
                pm.Info.Error = $"{phaseName}: {e.Message}";
                MethodOverrides.RemoveForMod(pm.Info.Id);
                Console.WriteLine($"[mods] '{pm.Plugin.Name}' โยน exception ตอน {phaseName} — ปิดใช้งาน mod นี้: {e}");
            }
        }
    }

    // ── มุมมอง IModApi เฉพาะของ mod แต่ละตัว (ไม่ implement ตรง ๆ ที่ PluginManager
    //     เพราะ Log() ต้องรู้ว่าเป็น mod ไหนเรียก — ถ้าใช้ field ตัวเดียวใช้ร่วมกันจะผิดชื่อได้เวลา
    //     mod เรียก api.Log() จาก handler ที่ทำงานทีหลัง ไม่ใช่ตอน OnLoad) ──
    private sealed class ModApiView : IModApi, IModEventsApi, IModMethodOverridesApi
    {
        private readonly PluginManager _mgr;
        private readonly LoadedModInfo _info;
        private readonly IModStorage _storage;

        public ModApiView(PluginManager mgr, LoadedModInfo info)
        {
            _mgr = mgr;
            _info = info;
            _storage = new FileModStorage(mgr.ModStorageRoot(info.Id));
            mgr._storages.Add(_storage);
        }

        public IModStorage Storage => _storage;

        public bool Subscribe(string eventName, Action<IModEventContext> handler, EventPriority priority = EventPriority.Normal)
        {
            return _mgr.RegisterEventInternal(eventName, _info, handler, priority);
        }

        public IReadOnlyList<string> GetAvailableEvents() => AvailableEvents;

        public bool RegisterMethodOverride(
            string methodId,
            ModMethodOverrideKind kind,
            ModMethodOverrideHandler handler,
            int priority = 0)
        {
            bool ok = _mgr.RegisterMethodOverrideInternal(_info, methodId, kind, handler, priority, out string resolved);
            if (ok)
            {
                _info.HasMethodOverrides = true;
                _info.MethodOverrides.Add($"{kind}:{resolved}");
            }
            return ok;
        }

        public IReadOnlyList<string> GetRegisteredMethodOverrides() => _mgr.MethodOverrides.GetRegisteredForMod(_info.Id);

        public int UnregisterMethodOverrides()
        {
            int removed = _mgr.MethodOverrides.RemoveForMod(_info.Id);
            _info.MethodOverrides.Clear();
            _info.HasMethodOverrides = false;
            return removed;
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

        public void OnPlayerDied(Action<IModPlayer> handler)
        {
            _mgr._diedHandlers.Add(handler);
            _info.HasPlayerDiedHook = true;
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

    private string ModStorageRoot(string modId)
    {
        string safe = new string((modId ?? "unknown").Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-').ToArray());
        if (string.IsNullOrEmpty(safe)) safe = "unknown";
        return Path.Combine(SaveStore.Root, "mods", safe);
    }

    private bool RegisterMethodOverrideInternal(
        LoadedModInfo info,
        string methodId,
        ModMethodOverrideKind kind,
        ModMethodOverrideHandler handler,
        int priority,
        out string resolvedMethodId)
    {
        resolvedMethodId = "";
        if (string.IsNullOrWhiteSpace(methodId) || methodId.Length > 512 || handler == null ||
            !Enum.IsDefined(typeof(ModMethodOverrideKind), kind))
        {
            info.MethodOverrideErrors++;
            Console.WriteLine($"[mods] '{info.Name}' method override registration rejected: invalid request");
            return false;
        }
        bool ok = MethodOverrides.Register(info.Id, methodId, kind, handler, priority,
            out string error, out resolvedMethodId);
        if (!ok)
        {
            info.MethodOverrideErrors++;
            Console.WriteLine($"[mods] '{info.Name}' method override '{methodId}' rejected: {error}");
        }
        return ok;
    }

    private bool RegisterEventInternal(string eventName, LoadedModInfo info, Action<IModEventContext> handler, EventPriority priority)
    {
        if (string.IsNullOrWhiteSpace(eventName) || handler == null || !AvailableEvents.Contains(eventName, StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"[mods] '{info.Name}' ลงทะเบียน event '{eventName}' ไม่สำเร็จ — ไม่รู้จัก event นี้");
            return false;
        }
        _eventSubscriptions.Add(new EventSubscription
        {
            Name = eventName,
            ModId = info.Id,
            Handler = handler,
            Priority = priority,
            Sequence = ++_registrationSequence
        });
        info.Events.Add(eventName);
        info.HasEventBus = true;
        _eventSubscriptions.Sort((a, b) =>
        {
            int p = ((int)b.Priority).CompareTo((int)a.Priority);
            if (p != 0) return p;
            p = string.Compare(a.ModId, b.ModId, StringComparison.OrdinalIgnoreCase);
            return p != 0 ? p : a.Sequence.CompareTo(b.Sequence);
        });
        return true;
    }

    public IModEventContext FireEvent(string eventName, ServerPlayer? player, bool before, bool committed)
    {
        return FireEvent(eventName, player, before, committed, null);
    }

    public IModEventContext FireEvent(string eventName, ServerPlayer? player, bool before, bool committed,
        IReadOnlyDictionary<string, string>? data)
    {
        ModEventContext context = new ModEventContext
        {
            EventName = eventName,
            OccurredAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0,
            Player = player == null ? null : new ServerModPlayer(player),
            IsBefore = before,
            IsCommitted = committed,
            Data = data ?? new Dictionary<string, string>(StringComparer.Ordinal)
        };
        EventSubscription[] handlers = _eventSubscriptions.Where(x => string.Equals(x.Name, eventName, StringComparison.OrdinalIgnoreCase)).ToArray();
        foreach (EventSubscription sub in handlers)
        {
            LoadedModInfo? metricInfo = _mods.FirstOrDefault(x => string.Equals(x.Id, sub.ModId, StringComparison.OrdinalIgnoreCase));
            if (metricInfo != null) metricInfo.EventCalls++;
            Stopwatch sw = Stopwatch.StartNew();
            try { sub.Handler(context); }
            catch (Exception e)
            {
                LoadedModInfo? info = _mods.FirstOrDefault(x => x.Id == sub.ModId);
                if (info != null) info.EventErrors++;
                Console.WriteLine($"[mods] event {eventName} ของ '{sub.ModId}' พัง: {e.Message}");
            }
            finally
            {
                LoadedModInfo? info = _mods.FirstOrDefault(x => x.Id == sub.ModId);
                if (info != null) info.EventMilliseconds += sw.Elapsed.TotalMilliseconds;
            }
        }
        return context;
    }

    private bool RegisterCommandInternal(string verb, string modName, Func<IModPlayer, string[], string> handler)
    {
        if (string.IsNullOrWhiteSpace(verb) || verb.Length > 48 || handler == null)
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
            LoadedModInfo? metricInfo = _mods.FirstOrDefault(x => string.Equals(x.Name, entry.ModName, StringComparison.OrdinalIgnoreCase));
            if (metricInfo != null)
            {
                metricInfo.CommandCalls++;
                if (args.Length > 16 || args.Sum(x => x?.Length ?? 0) > 4096)
                { metricInfo.RateLimitedCalls++; reply = "mod command arguments exceed the safety limit"; return true; }
            }
            try
            {
                reply = entry.Handler(new ServerModPlayer(caller), args);
            }
            catch (Exception e)
            {
                if (metricInfo != null) metricInfo.CommandErrors++;
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
        if (_joinHandlers.Count != 0)
        {
            ServerModPlayer mp = new ServerModPlayer(player);
            foreach (Action<IModPlayer> h in _joinHandlers.ToArray()) SafeInvoke("OnPlayerJoined", () => h(mp));
        }
        FireEvent("player.joined", player, false, true);
    }

    public void FirePlayerLeft(ServerPlayer player)
    {
        if (_leaveHandlers.Count != 0)
        {
            ServerModPlayer mp = new ServerModPlayer(player);
            foreach (Action<IModPlayer> h in _leaveHandlers.ToArray()) SafeInvoke("OnPlayerLeft", () => h(mp));
        }
        FireEvent("player.left", player, false, true);
    }

    /// <summary>[V1.1] เรียกจาก ServerPlayer.Die() — Dead ถูก set เป็น true แล้วเสมอ</summary>
    public void FirePlayerDied(ServerPlayer player)
    {
        if (_diedHandlers.Count != 0)
        {
            ServerModPlayer mp = new ServerModPlayer(player);
            foreach (Action<IModPlayer> h in _diedHandlers.ToArray()) SafeInvoke("OnPlayerDied", () => h(mp));
        }
        FireEvent("player.died", player, false, true);
    }

    public void FireTick(double dtSeconds)
    {
        if (_tickHandlers.Count != 0)
        {
            foreach (Action<double> h in _tickHandlers.ToArray()) SafeInvoke("OnTick", () => h(dtSeconds));
        }
        FireEvent("server.tick", null, false, true);
    }

    internal void RecordMethodOverrideCall(string modId, double elapsedMilliseconds, bool failed)
    {
        LoadedModInfo? info = _mods.FirstOrDefault(x => string.Equals(x.Id, modId, StringComparison.OrdinalIgnoreCase));
        if (info == null) return;
        lock (info)
        {
            info.MethodOverrideCalls++;
            info.MethodOverrideMilliseconds += elapsedMilliseconds;
            if (failed) info.MethodOverrideErrors++;
        }
    }

    public void DisableAll()
    {
        if (_disabled) return;
        _disabled = true;
        for (int i = _loadedPluginRuntimes.Count - 1; i >= 0; i--)
        {
            LoadedPluginRuntime runtime = _loadedPluginRuntimes[i];
            try
            {
                if (runtime.Plugin is IModLifecycle lifecycle) lifecycle.OnDisable(runtime.Api);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[mods] '{runtime.Info.Id}' OnDisable failed: {e.Message}");
            }
            MethodOverrides.RemoveForMod(runtime.Info.Id);
            runtime.Info.MethodOverrides.Clear();
            runtime.Info.HasMethodOverrides = false;
            runtime.Info.Loaded = false;
            runtime.Info.State = "Disabled";
        }
        MethodOverrides.RemoveAll();
    }

    public bool FlushStorage()
    {
        bool ok = true;
        foreach (IModStorage storage in _storages)
        {
            try { ok = storage.Flush() && ok; }
            catch (Exception e) { ok = false; Console.WriteLine($"[mods] storage flush failed: {e.Message}"); }
        }
        return ok;
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
