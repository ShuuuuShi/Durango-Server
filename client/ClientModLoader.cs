using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    private static readonly List<KeyValuePair<KeyCode, Action>> _hotkeys = new List<KeyValuePair<KeyCode, Action>>();
    private static readonly List<Action> _gameReadyHandlers = new List<Action>();
    private static bool _gameReadyFired;

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
            Debug.Log("[clientmods] LoadAll ล้ม: " + e);
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

    private static void LoadInternal(string modsDir)
    {
        if (!Directory.Exists(modsDir))
        {
            Debug.Log("[clientmods] ไม่มีโฟลเดอร์ '" + modsDir + "' — ข้าม (ปกติ ไม่ error)");
            return;
        }
        string[] dlls = Directory.GetFiles(modsDir, "*.dll", SearchOption.TopDirectoryOnly);
        if (dlls.Length == 0)
        {
            Debug.Log("[clientmods] โฟลเดอร์ '" + modsDir + "' ว่างเปล่า — ไม่มี mod ให้โหลด");
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
                Debug.Log("[clientmods] โหลดไฟล์ " + Path.GetFileName(dllPath) + " ไม่สำเร็จ: " + e.Message);
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
                Debug.Log("[clientmods] " + Path.GetFileName(dllPath) + ": บาง type โหลดไม่ได้ — โหลดเท่าที่ทำได้");
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
                    Debug.Log("[clientmods] สร้าง instance " + type.FullName + " ไม่สำเร็จ: " + e.Message);
                    continue;
                }
                PendingMod pm = new PendingMod();
                pm.Plugin = plugin;
                pm.Api = new ClientModApiImpl(plugin.Name);
                pm.SourceFile = Path.GetFileName(dllPath);
                pending.Add(pm);
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
                loaded++;
                Debug.Log("[clientmods] โหลด '" + pending[i].Plugin.Name + "' v" + pending[i].Plugin.Version + " จาก " + pending[i].SourceFile + " สำเร็จ (ครบ 3 เฟส)");
            }
        }
        Debug.Log("[clientmods] โหลดสำเร็จ " + loaded + " mod จาก " + dlls.Length + " ไฟล์ .dll ใน '" + modsDir + "'");
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
                Debug.Log("[clientmods] '" + pm.Plugin.Name + "' โยน exception ตอน " + phaseName + " — ปิดใช้งาน mod นี้: " + e);
            }
        }
    }

    // ── มุมมอง IClientModApi ของ mod แต่ละตัว ──────────────────────────────
    private sealed class ClientModApiImpl : IClientModApi
    {
        private readonly string _modName;

        public ClientModApiImpl(string modName)
        {
            _modName = modName;
        }

        public void Log(string message)
        {
            Debug.Log("[clientmod:" + _modName + "] " + message);
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
            Debug.Log("[clientmods] handler โยน exception: " + e);
        }
    }

    // ── เรียกจาก ClientModDriver.Update() เท่านั้น ─────────────────────────
    internal static void PumpFrame()
    {
        if (!_gameReadyFired && TerrainBase.IsPlayerInitialized)
        {
            _gameReadyFired = true;
            for (int i = 0; i < _gameReadyHandlers.Count; i++)
            {
                InvokeSafe(_gameReadyHandlers[i]);
            }
            _gameReadyHandlers.Clear();
        }
        for (int i = 0; i < _hotkeys.Count; i++)
        {
            if (Input.GetKeyDown(_hotkeys[i].Key))
            {
                InvokeSafe(_hotkeys[i].Value);
            }
        }
    }
}

/// <summary>MonoBehaviour ตัวเดียวไว้เรียก ClientModLoader.PumpFrame() ทุกเฟรม (ปุ่มลัด + OnGameReady)</summary>
public sealed class ClientModDriver : MonoBehaviour
{
    private void Update()
    {
        ClientModLoader.PumpFrame();
    }
}
