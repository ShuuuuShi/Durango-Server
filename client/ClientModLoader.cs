using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Durango.Modding;
using Durango.Terrain;
using UnityEngine;

/// <summary>
/// [แก้เอง] 24 ส.ค. 2026 — ระบบ mod ฝั่งเกม คู่กับ PluginManager.cs ฝั่งเซิร์ฟ (เจ้าของสั่งเมื่อเห็น
/// ฝั่งเซิร์ฟทำงานจริงแล้ว) สแกน .dll จากโฟลเดอร์ mods\ ข้าง ๆ DurangoV2.exe ตอนเกมบูต โหลดด้วย
/// Assembly.LoadFrom แล้วหา type ที่ implement IClientPlugin (นิยามอยู่ที่ client-mod-sdk/ — คนละ
/// assembly กับ Assembly-CSharp.dll เหมือนฝั่งเซิร์ฟ อัปเดตเกมแล้ว mod เก่ายังใช้ได้ตราบใดที่ไม่แก้
/// interface) สร้าง instance แล้วเรียก OnLoad() ให้ mod ลงทะเบียนปุ่มลัด/event ผ่าน IClientModApi
///
/// เรียกจาก GameManager.Start() (ดูจุดเรียกใน GameManager.cs) — ครั้งเดียวตอนเกมบูต
///
/// ดูวิธีเขียน mod + ตัวอย่างที่ docs/client/Modding.md และ tools/ExampleClientMod/
/// </summary>
public static class ClientModLoader
{
    private static bool _loaded;
    private static bool _disabled;
    internal static readonly ClientMethodOverrideManager MethodOverrides = new ClientMethodOverrideManager();
    internal static readonly ClientAssetOverrideManager AssetOverrides = new ClientAssetOverrideManager();
    private static readonly List<KeyValuePair<KeyCode, Action>> _hotkeys = new List<KeyValuePair<KeyCode, Action>>();
    private static readonly List<Action> _gameReadyHandlers = new List<Action>();
    // [V1.1] 27 ส.ค. 2026 — OnUpdate: mod เลือกได้ว่าจะติดเฟรมพัส (dt = Time.deltaTime)
    private static readonly List<Action<float>> _updateHandlers = new List<Action<float>>();
    private sealed class SceneHook { public string Scene = ""; public Action Handler; }
    private sealed class HudHook { public string Id = ""; public Action Handler; }
    private static readonly List<SceneHook> _sceneHooks = new List<SceneHook>();
    private static readonly List<HudHook> _hudHooks = new List<HudHook>();
    private static string _lastScene = "";
    private static bool _gameReadyFired;
    private sealed class ClientModDescriptor { public string Id = ""; public string Version = ""; public string Sha256 = ""; public string Signature = ""; public string PublicKey = ""; }
    private static readonly List<ClientModDescriptor> _descriptors = new List<ClientModDescriptor>();

    /// <summary>M5 manifest sent to a compatible server before Ready.</summary>
    public static string BuildNegotiationManifest(out string catalogHash)
    {
        List<ClientModDescriptor> mods = new List<ClientModDescriptor>(_descriptors);
        mods.Sort(delegate(ClientModDescriptor a, ClientModDescriptor b) { return string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase); });
        StringBuilder hashInput = new StringBuilder();
        foreach (ClientModDescriptor mod in mods) hashInput.Append(mod.Id).Append('\n').Append(mod.Version).Append('\n').Append(mod.Sha256).Append('\n');
        catalogHash = ToHex(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(hashInput.ToString())));
        StringBuilder json = new StringBuilder("{\"protocol\":1,\"mods\":[");
        for (int i = 0; i < mods.Count; i++)
        {
            if (i != 0) json.Append(',');
            json.Append("{\"id\":\"").Append(mods[i].Id).Append("\",\"version\":\"").Append(mods[i].Version).Append("\",\"sha256\":\"").Append(mods[i].Sha256).Append("\",\"signature\":\"").Append(mods[i].Signature ?? "").Append("\",\"public_key\":\"").Append(mods[i].PublicKey ?? "").Append("\"}");
        }
        return json.Append("]}").ToString();
    }

    private static string ToHex(byte[] bytes)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++) result.Append(bytes[i].ToString("x2"));
        return result.ToString();
    }

    // Debug.Log is conditional in the Unity reference used by this net35 build,
    // so keep a tiny file trace as deterministic evidence when validating mods.
    internal static void Trace(string message)
    {
        try
        {
            string path = Path.Combine(Path.Combine(Application.dataPath, ".."), "clientmods.log");
            File.AppendAllText(path, DateTime.Now.ToString("O") + " " + message + Environment.NewLine);
        }
        catch
        {
        }
        Debug.Log(message);
    }

    public static void LoadAll()
    {
        if (_loaded)
        {
            return;
        }
        _loaded = true;

        string modsDir = Path.Combine(Path.Combine(Application.dataPath, ".."), "mods");
        try
        {
            LoadInternal(modsDir);
        }
        catch (Exception e)
        {
            Trace("[clientmods] LoadAll ล้ม: " + e);
        }

        // ตัวขับ Update() แยกต่างหาก ไม่ผูกกับ MonoBehaviour ไหนของเกม กันพังตามกันถ้าเกมรื้อ scene
        GameObject driver = new GameObject("__ClientModDriver");
        UnityEngine.Object.DontDestroyOnLoad(driver);
        driver.AddComponent<ClientModDriver>();
    }

    private sealed class PendingMod
    {
        public IClientPlugin Plugin;
        public ClientModApiImpl Api;
        public string SourceFile;
        public bool Failed;
    }
    private static readonly List<PendingMod> _loadedMods = new List<PendingMod>();

    private static void LoadInternal(string modsDir)
    {
        if (!Directory.Exists(modsDir))
        {
            Trace("[clientmods] ไม่มีโฟลเดอร์ '" + modsDir + "' — ข้าม (ปกติ ไม่ error)");
            return;
        }
        // Support both the legacy mods\MyMod.dll layout and isolated packages at
        // mods\my-mod\MyMod.dll. Only one package directory level is scanned so
        // DLLs kept under assets/dependencies are not treated as plugin entries.
        List<string> discoveredDlls = new List<string>();
        discoveredDlls.AddRange(Directory.GetFiles(modsDir, "*.dll", SearchOption.TopDirectoryOnly));
        string[] packageDirectories = Directory.GetDirectories(modsDir, "*", SearchOption.TopDirectoryOnly);
        for (int packageIndex = 0; packageIndex < packageDirectories.Length; packageIndex++)
            discoveredDlls.AddRange(Directory.GetFiles(packageDirectories[packageIndex], "*.dll", SearchOption.TopDirectoryOnly));
        discoveredDlls.Sort(StringComparer.OrdinalIgnoreCase);
        string[] dlls = discoveredDlls.ToArray();
        if (dlls.Length == 0)
        {
            Trace("[clientmods] โฟลเดอร์ '" + modsDir + "' ว่างเปล่า — ไม่มี mod ให้โหลด");
            return;
        }

        // ── ขั้นที่ 1: สแกน+สร้าง instance ของทุก mod ก่อน (ยังไม่เรียกเฟสไหนทั้งนั้น) ──
        List<PendingMod> pending = new List<PendingMod>();
        for (int d = 0; d < dlls.Length; d++)
        {
            string dllPath = dlls[d];
            Assembly asm;
            try
            {
                asm = Assembly.LoadFrom(dllPath);
            }
            catch (Exception e)
            {
                Trace("[clientmods] โหลดไฟล์ " + Path.GetFileName(dllPath) + " ไม่สำเร็จ: " + e.Message);
                continue;
            }
            Type[] types;
            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                List<Type> ok = new List<Type>();
                for (int t = 0; t < e.Types.Length; t++)
                {
                    if (e.Types[t] != null) ok.Add(e.Types[t]);
                }
                types = ok.ToArray();
                Trace("[clientmods] " + Path.GetFileName(dllPath) + ": บาง type โหลดไม่ได้ — โหลดเท่าที่ทำได้");
            }
            for (int t = 0; t < types.Length; t++)
            {
                Type type = types[t];
                if (type.IsInterface || type.IsAbstract || !typeof(IClientPlugin).IsAssignableFrom(type))
                {
                    continue;
                }
                IClientPlugin plugin;
                try
                {
                    plugin = (IClientPlugin)Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    Trace("[clientmods] สร้าง instance " + type.FullName + " ไม่สำเร็จ: " + e.Message);
                    continue;
                }
                IClientModIdentity identity = plugin as IClientModIdentity;
                string id = identity == null ? plugin.Name.ToLowerInvariant().Replace(' ', '-') : identity.Id;
                PendingMod pm = new PendingMod();
                pm.Plugin = plugin;
                pm.Api = new ClientModApiImpl(id, plugin.Name, Path.GetDirectoryName(dllPath));
                pm.SourceFile = Path.GetFileName(dllPath);
                pending.Add(pm);
                try
                {
                    _descriptors.Add(new ClientModDescriptor
                    {
                        Id = id,
                        Version = identity == null ? plugin.Version : identity.Version,
                        Sha256 = ToHex(SHA256.Create().ComputeHash(File.ReadAllBytes(dllPath))),
                        Signature = identity == null ? "" : identity.Signature,
                        PublicKey = identity == null ? "" : identity.PublicKey
                    });
                }
                catch (Exception e) { Trace("[clientmods] อ่าน hash ไม่สำเร็จ " + pm.SourceFile + ": " + e.Message); }
            }
        }

        // ── ขั้นที่ 2: ไล่ 3 เฟสทีละเฟส "ครบทุก mod ก่อนขึ้นเฟสถัดไป" (แบบ Minecraft/Forge) ──
        RunPhase(pending, "PreLoad", delegate(IClientPlugin p, IClientModApi api) { p.OnPreLoad(api); });
        RunPhase(pending, "Load", delegate(IClientPlugin p, IClientModApi api) { p.OnLoad(api); });
        RunPhase(pending, "PostLoad", delegate(IClientPlugin p, IClientModApi api) { p.OnPostLoad(api); });

        int loaded = 0;
        for (int i = 0; i < pending.Count; i++)
        {
            if (!pending[i].Failed)
            {
                _loadedMods.Add(pending[i]);
                loaded++;
                Trace("[clientmods] โหลด '" + pending[i].Plugin.Name + "' v" + pending[i].Plugin.Version + " จาก " + pending[i].SourceFile + " สำเร็จ (ครบ 3 เฟส)");
            }
        }
        Trace("[clientmods] โหลดสำเร็จ " + loaded + " mod จาก " + dlls.Length + " ไฟล์ .dll ใน '" + modsDir + "'");
    }

    private static void RunPhase(List<PendingMod> pending, string phaseName, Action<IClientPlugin, IClientModApi> call)
    {
        for (int i = 0; i < pending.Count; i++)
        {
            PendingMod pm = pending[i];
            if (pm.Failed)
            {
                continue;
            }
            try
            {
                call(pm.Plugin, pm.Api);
            }
            catch (Exception e)
            {
                pm.Failed = true;
                pm.Api.UnregisterMethodOverrides();
                AssetOverrides.RemoveForMod(pm.Api.ModId);
                Trace("[clientmods] '" + pm.Plugin.Name + "' โยน exception ตอน " + phaseName + " — ปิดใช้งาน mod นี้: " + e);
            }
        }
    }

    // ── มุมมอง IClientModApi ของ mod แต่ละตัว ──────────────────────────────
    private sealed class ClientModApiImpl : IClientModApi, IClientPresentationApi, IClientMethodOverridesApi, IClientAssetOverrideApi
    {
        private readonly string _modId;
        private readonly string _modName;
        private readonly string _modRoot;

        public ClientModApiImpl(string modId, string modName, string modRoot)
        {
            _modId = modId;
            _modName = modName;
            _modRoot = modRoot ?? "";
        }

        internal string ModId { get { return _modId; } }

        public void Log(string message)
        {
            Trace("[clientmod:" + _modName + "] " + message);
        }

        public void ShowMessage(string text)
        {
            UIManager.SystemMsg(text);
        }

        public void RegisterHotkey(KeyCode key, Action onPressed)
        {
            _hotkeys.Add(new KeyValuePair<KeyCode, Action>(key, onPressed));
        }

        public void OnGameReady(Action handler)
        {
            if (_gameReadyFired)
            {
                InvokeSafe(handler);
                return;
            }
            _gameReadyHandlers.Add(handler);
        }

        public void OnUpdate(Action<float> handler)
        {
            if (handler != null)
            {
                _updateHandlers.Add(handler);
            }
        }

        public bool RegisterSceneHook(string sceneName, Action onLoaded)
        {
            if (String.IsNullOrEmpty(sceneName) || onLoaded == null || _sceneHooks.Count >= 256) return false;
            _sceneHooks.Add(new SceneHook { Scene = sceneName, Handler = onLoaded });
            return true;
        }

        public bool RegisterHud(string id, Action draw)
        {
            if (String.IsNullOrEmpty(id) || draw == null || id.Length > 64 || _hudHooks.Count >= 128) return false;
            _hudHooks.Add(new HudHook { Id = _modName + ":" + id, Handler = draw });
            return true;
        }

        public bool ValidateAsset(string relativePath, string sha256)
        {
            if (String.IsNullOrEmpty(_modRoot) || String.IsNullOrEmpty(relativePath) || String.IsNullOrEmpty(sha256)) return false;
            string root = Path.GetFullPath(_modRoot);
            string candidate = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(candidate)) return false;
            try
            {
                string actual = ToHex(SHA256.Create().ComputeHash(File.ReadAllBytes(candidate)));
                return String.Equals(actual, sha256, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        public bool ValidateAssetManifest(string relativePath)
        {
            if (String.IsNullOrEmpty(_modRoot) || String.IsNullOrEmpty(relativePath)) return false;
            string root = Path.GetFullPath(_modRoot);
            string manifest = Path.GetFullPath(Path.Combine(root, relativePath));
            if (!manifest.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(manifest)) return false;
            try
            {
                string[] lines = File.ReadAllLines(manifest);
                if (lines.Length > 256) return false;
                int checkedAssets = 0;
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal)) continue;
                    int separator = line.IndexOf('|');
                    if (separator <= 0 || separator == line.Length - 1) return false;
                    string assetPath = line.Substring(0, separator).Trim();
                    string hash = line.Substring(separator + 1).Trim();
                    if (!ValidateAsset(assetPath, hash)) return false;
                    checkedAssets++;
                }
                return checkedAssets > 0;
            }
            catch { return false; }
        }

        public bool RegisterMethodOverride(string methodId, ClientMethodOverrideKind kind,
            ClientMethodOverrideHandler handler, int priority)
        {
            string error;
            string resolved;
            bool ok = MethodOverrides.Register(_modId, methodId, kind, handler, priority, out error, out resolved);
            if (ok) Trace("[clientmod:" + _modName + "] method override registered: " + kind + ":" + resolved);
            else Trace("[clientmod:" + _modName + "] method override rejected: " + methodId + " — " + error);
            return ok;
        }

        public string[] GetRegisteredMethodOverrides()
        {
            return MethodOverrides.GetRegisteredForMod(_modId);
        }

        public int UnregisterMethodOverrides()
        {
            return MethodOverrides.RemoveForMod(_modId);
        }

        public bool LoadAssetBundle(string bundleId, string relativePath, string sha256)
        {
            string error;
            bool ok = AssetOverrides.LoadBundle(_modId, _modRoot, bundleId, relativePath, sha256, out error);
            if (!ok) Trace("[clientmod:" + _modName + "] bundle load rejected: " + error);
            return ok;
        }

        public bool IsAssetBundleLoaded(string bundleId)
        {
            return AssetOverrides.IsBundleLoaded(_modId, bundleId);
        }

        public string[] GetLoadedAssetBundles()
        {
            return AssetOverrides.GetLoadedBundles(_modId);
        }

        public bool UnloadAssetBundle(string bundleId, bool unloadLoadedAssets)
        {
            string error;
            bool ok = AssetOverrides.UnloadBundle(_modId, bundleId, unloadLoadedAssets, out error);
            if (!ok && error.Length != 0) Trace("[clientmod:" + _modName + "] bundle unload rejected: " + error);
            return ok;
        }

        public bool SpawnPrefab(string instanceId, string bundleId, string prefabAssetName,
            ClientAssetAnchor anchor, Vector3 position, Vector3 eulerAngles, Vector3 scale)
        {
            string error;
            bool ok = AssetOverrides.SpawnPrefab(_modId, instanceId, bundleId, prefabAssetName,
                anchor, position, eulerAngles, scale, out error);
            if (!ok) Trace("[clientmod:" + _modName + "] prefab spawn rejected: " + error);
            return ok;
        }

        public bool DestroySpawnedAsset(string instanceId)
        {
            return AssetOverrides.DestroySpawn(_modId, instanceId);
        }

        public bool PlayAudioClip(string instanceId, string bundleId, string audioAssetName,
            ClientAssetAnchor anchor, Vector3 position, float volume, bool loop)
        {
            string error;
            bool ok = AssetOverrides.PlayAudio(_modId, instanceId, bundleId, audioAssetName,
                anchor, position, volume, loop, out error);
            if (!ok) Trace("[clientmod:" + _modName + "] audio playback rejected: " + error);
            return ok;
        }

        public bool ReplaceLocalPlayerModel(string overrideId, string bundleId,
            string prefabAssetName, bool remapSkeleton)
        {
            string error;
            bool ok = AssetOverrides.ReplaceModel(_modId, overrideId, bundleId, prefabAssetName,
                remapSkeleton, out error);
            if (!ok) Trace("[clientmod:" + _modName + "] model override rejected: " + error);
            return ok;
        }

        public bool ReplaceLocalPlayerMaterial(string overrideId, string rendererSelector,
            int materialIndex, string bundleId, string materialAssetName)
        {
            string error;
            bool ok = AssetOverrides.ReplaceMaterial(_modId, overrideId, rendererSelector,
                materialIndex, bundleId, materialAssetName, out error);
            if (!ok) Trace("[clientmod:" + _modName + "] material override rejected: " + error);
            return ok;
        }

        public bool ReplaceLocalPlayerTexture(string overrideId, string rendererSelector,
            int materialIndex, string textureProperty, string bundleId, string textureAssetName)
        {
            string error;
            bool ok = AssetOverrides.ReplaceTexture(_modId, overrideId, rendererSelector,
                materialIndex, textureProperty, bundleId, textureAssetName, out error);
            if (!ok) Trace("[clientmod:" + _modName + "] texture override rejected: " + error);
            return ok;
        }

        public bool RestoreLocalPlayerAppearance(string overrideId)
        {
            return AssetOverrides.RemoveAppearance(_modId, overrideId);
        }

        public int RestoreAllLocalPlayerAppearance()
        {
            return AssetOverrides.RemoveAllAppearance(_modId);
        }

        public IClientPlayer LocalPlayer
        {
            get
            {
                PlayerBehavior lp = PlayerBehavior.LocalPlayer;
                return lp == null ? null : new ClientPlayerImpl(lp);
            }
        }
    }

    private sealed class ClientPlayerImpl : IClientPlayer
    {
        private readonly PlayerBehavior _p;
        public ClientPlayerImpl(PlayerBehavior p) { _p = p; }
        public string Name => _p.PlayerName;
        public Vector3 Position => _p.transform.position;
    }

    private static void InvokeSafe(Action a)
    {
        try
        {
            a();
        }
        catch (Exception e)
        {
            Trace("[clientmods] handler โยน exception: " + e);
        }
    }

    internal static void DisableAll()
    {
        if (_disabled) return;
        _disabled = true;
        for (int i = _loadedMods.Count - 1; i >= 0; i--)
        {
            PendingMod mod = _loadedMods[i];
            try
            {
                IClientModLifecycle lifecycle = mod.Plugin as IClientModLifecycle;
                if (lifecycle != null) lifecycle.OnDisable(mod.Api);
            }
            catch (Exception e)
            {
                Trace("[clientmods] '" + mod.Plugin.Name + "' OnDisable failed: " + e.Message);
            }
            mod.Api.UnregisterMethodOverrides();
            AssetOverrides.RemoveForMod(mod.Api.ModId);
        }
        MethodOverrides.RemoveAll();
        AssetOverrides.RemoveAll();
        _loadedMods.Clear();
    }

    // ── เรียกจาก ClientModDriver.Update() เท่านั้น ─────────────────────────
    internal static void PumpFrame()
    {
        AssetOverrides.PumpFrame();
        // [V1.1] OnUpdate ก่อนอื่น — mod บางตัวใช้ dt ทำ cooldown/timer
        float dt = Time.deltaTime;
        for (int i = 0; i < _updateHandlers.Count; i++)
        {
            InvokeSafe(delegate { _updateHandlers[i](dt); });
        }
        if (!_gameReadyFired && TerrainBase.IsPlayerInitialized)
        {
            _gameReadyFired = true;
            for (int i = 0; i < _gameReadyHandlers.Count; i++)
            {
                InvokeSafe(_gameReadyHandlers[i]);
            }
            _gameReadyHandlers.Clear();
        }
        string scene = Application.loadedLevelName ?? "";
        if (!String.Equals(scene, _lastScene, StringComparison.Ordinal))
        {
            _lastScene = scene;
            for (int i = 0; i < _sceneHooks.Count; i++)
                if (String.Equals(_sceneHooks[i].Scene, scene, StringComparison.OrdinalIgnoreCase)) InvokeSafe(_sceneHooks[i].Handler);
        }
        for (int i = 0; i < _hotkeys.Count; i++)
        {
            if (Input.GetKeyDown(_hotkeys[i].Key))
            {
                InvokeSafe(_hotkeys[i].Value);
            }
        }
    }

    internal static void NotifyLocalPlayerAppearanceChanged()
    {
        AssetOverrides.MarkLocalPlayerAppearanceDirty();
    }

    internal static void PumpHud()
    {
        for (int i = 0; i < _hudHooks.Count; i++) InvokeSafe(_hudHooks[i].Handler);
    }
}

/// <summary>MonoBehaviour ตัวเดียวไว้เรียก ClientModLoader.PumpFrame() ทุกเฟรม (ปุ่มลัด + OnGameReady)</summary>
public sealed class ClientModDriver : MonoBehaviour
{
    private void Update()
    {
        ClientModLoader.PumpFrame();
    }

    private void OnGUI()
    {
        ClientModLoader.PumpHud();
    }

    private void OnApplicationQuit()
    {
        ClientModLoader.DisableAll();
    }
}
