using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

/// <summary>
/// บังคับให้ตัวเกมเริ่มผ่าน DurangoUpdater.exe เสมอ
/// ตัว updater จะส่ง -durango-updated กลับมาเพื่ออนุญาตให้เกมทำงานต่อ
/// </summary>
internal static class DurangoUpdateGate
{
    private const string UpdatedLaunchArgument = "-durango-updated";
    private static bool _handled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CheckLaunchSourceAtRuntime()
    {
        EnsureUpdaterLaunchAllowed();
    }

    public static bool EnsureUpdaterLaunchAllowed()
    {
        if (_handled || Application.isEditor)
        {
            return true;
        }

        _handled = true;
        if (HasUpdatedLaunchArgument())
        {
            UnityEngine.Debug.Log("[DurangoUpdater] verified launch from updater.");
            return true;
        }

        string gameDir = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(gameDir))
        {
            UnityEngine.Debug.LogError("[DurangoUpdater] cannot find game directory.");
            Application.Quit();
            return false;
        }

        string updaterPath = Path.Combine(gameDir, "DurangoUpdater.exe");
        if (!File.Exists(updaterPath))
        {
            UnityEngine.Debug.LogError("[DurangoUpdater] DurangoUpdater.exe is missing: " + updaterPath);
            Application.Quit();
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = updaterPath,
                WorkingDirectory = gameDir,
                UseShellExecute = true
            });
            UnityEngine.Debug.Log("[DurangoUpdater] direct launch blocked; updater started.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("[DurangoUpdater] cannot start updater: " + ex.Message);
        }

        Application.Quit();
        return false;
    }

    private static bool HasUpdatedLaunchArgument()
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], UpdatedLaunchArgument, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
