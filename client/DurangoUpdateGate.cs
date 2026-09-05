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
        // [4 ก.ย. 2026] มือถือไม่มี DurangoUpdater/โฟลเดอร์เกม (dataPath = ไฟล์ APK) — ข้ามด่านนี้ไปเลย
        if (_handled || Application.isEditor || Application.isMobilePlatform)
        {
            return true;
        }

        _handled = true;
        // [4 ก.ย. 2026] เลิกใช้ DurangoUpdater.exe แล้ว (เจ้าของสั่งใช้ DinoWorld Launcher แทน)
        // ด่านเดิม: ไม่มี -durango-updated ⇒ spawn DurangoUpdater แล้วปิดเกม — ตอนนี้ส่งต่อให้ LauncherGate
        // (เช็ค env DINOWORLD_LAUNCH + launcher.session) เท่านั้น · hook นี้รัน BeforeSceneLoad = บล็อกก่อนขึ้น splash
        return LauncherGate.Enforce();
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
