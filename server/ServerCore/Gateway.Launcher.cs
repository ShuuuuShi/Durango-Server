using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Durango.Offline;
using Newtonsoft.Json.Linq;
using IoPath = System.IO.Path;

namespace DurangoServer.Core;

// ============================================================================
// Gateway.Launcher — endpoint สำหรับ DinoWorld Launcher (tools/Launcher)
//
// ต่างจาก /admin/* ตรงที่นี่คือโซน "ผู้เล่น" ตอบเฉพาะข้อมูลที่ปลอดภัยพอให้ผู้เล่นเห็น:
//   GET /launcher/news    → ประกาศ/อีเวนต์/patch note อ่านจาก data/launcher_news.json
//                           (เจ้าของเซิร์ฟแก้ไฟล์นี้ได้ตลอด ไม่ต้อง restart — อ่านใหม่ทุกคำขอ)
//   GET /launcher/status  → ชื่อเซิฟ + จำนวนผู้เล่น/เพดาน + เวอร์ชันเกมล่าสุด
//                           (subset ของ /admin/status ที่ตัดของ sensitive ออกทั้งหมด)
//   GET /launcher/version → เวอร์ชัน+manifest แพตช์ (ถ้ามี) — launcher ใช้ตัดสินใจว่าต้องอัปเดตไหม
//
// ไม่มี auth (ไม่เหมือน admin token) เพราะไม่มี action ใดๆ ทั้งสิ้น — อ่านอย่างเดียว
// ============================================================================

public partial class Gateway
{
    /// <summary>path ไฟล์ข่าว launcher — ตั้งจาก Program.cs ตอนสร้าง Gateway</summary>
    public static string LauncherNewsPath = Path.Combine("data", "launcher_news.json");

    /// <summary>cache ข่าวสั้น ๆ กันอ่านไฟล์ถี่เกิน (ไฟล์เล็กมาก อ่านซ้ำก็ไม่หนัก แต่กัน disk churn)</summary>
    private string _launcherNewsCacheJson;
    private DateTime _launcherNewsCacheAtUtc = DateTime.MinValue;
    private static readonly TimeSpan LauncherNewsCacheTtl = TimeSpan.FromSeconds(5);

    private void RegisterLauncherRoutes()
    {
        // ── ประกาศ/ข่าว ─────────────────────────────────────────────
        _webServer.GetRoute["/launcher/news"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            try
            {
                return new WebServer.JsonResponse(ReadLauncherNews());
            }
            catch (Exception e)
            {
                Console.WriteLine("[launcher] อ่าน launcher_news.json ไม่สำเร็จ: " + e.Message);
                return new WebServer.JsonResponse(
                    new JObject { ["ok"] = false, ["error"] = "news_unavailable" }.ToString(),
                    HttpStatusCode.InternalServerError);
            }
        };

        // ── สถานะเซิฟฉบับผู้เล่น (ตัดของ internal/sensitive ออกหมด) ──
        _webServer.GetRoute["/launcher/status"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JObject o = new JObject
            {
                ["ok"] = true,
                ["name"] = _world.ServerName,
                ["online"] = true,
                ["players"] = ServerStats.OnlinePlayers,
                ["max_players"] = GameServer.MaxConnections,
                ["tps"] = Math.Round(ServerStats.Tps, 1),
                ["uptime_sec"] = Math.Round(ServerStats.UptimeSeconds, 1),
                ["latest_version"] = ReadLatestVersion()
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── manifest แพตช์ (เผื่อ launcher จะย้ายจาก GitHub Release มาอ่านที่เซิร์ฟ) ──
        _webServer.GetRoute["/launcher/version"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JObject o = new JObject
            {
                ["ok"] = true,
                ["version"] = ReadLatestVersion(),
            };
            // ถ้ามีไฟล์ dataDir/launcher_patch.json → แนบ zip_url+sha256 ให้ launcher โหลดจากเซิร์ฟเองได้
            JObject patch = TryReadPatchManifest();
            if (patch != null)
            {
                o["zip_url"] = (string?)patch["zip_url"];
                o["sha256"] = (string?)patch["sha256"];
                o["notes"] = (string?)patch["notes"];
            }
            return new WebServer.JsonResponse(o.ToString());
        };
    }

    /// <summary>
    /// อ่านข่าวจาก LauncherNewsPath (อ่านไฟล์ใหม่ทุกคำขอ แค่ cache 5 วิ) — ไฟล์ไม่มี/เสีย = คืน list ว่าง
    /// เพื่อให้ launcher ยังทำงานได้ปกติ (หลัก fail-safe เดียวกับ Updater: ของพังต้องไม่บล็อกการเข้าเกม)
    /// </summary>
    private string ReadLauncherNews()
    {
        if (_launcherNewsCacheJson != null && DateTime.UtcNow - _launcherNewsCacheAtUtc < LauncherNewsCacheTtl)
        {
            return _launcherNewsCacheJson;
        }

        string json;
        if (!File.Exists(LauncherNewsPath))
        {
            json = new JObject { ["items"] = new JArray() }.ToString();
        }
        else
        {
            // validate ผ่าน JObject.Parse ก่อน — ไฟล์ JSON เสียจะ throw แล้ว route ตอบ error ให้
            JObject parsed = JObject.Parse(File.ReadAllText(LauncherNewsPath));
            if (parsed["items"] == null)
            {
                parsed["items"] = new JArray();
            }
            json = parsed.ToString();
        }

        _launcherNewsCacheJson = json;
        _launcherNewsCacheAtUtc = DateTime.UtcNow;
        return json;
    }

    /// <summary>
    /// อ่าน manifest แพตช์จาก dataDir (โฟลเดอร์เดียวกับ launcher_news.json) — รูปแบบเดียวกับ
    /// manifest.json ของ Updater: version/zip_url/sha256/notes · ไม่มีไฟล์ = null
    /// </summary>
    private JObject TryReadPatchManifest()
    {
        string patchPath = IoPath.Combine(
            IoPath.GetDirectoryName(IoPath.GetFullPath(LauncherNewsPath)) ?? ".",
            "launcher_patch.json");
        if (!File.Exists(patchPath))
        {
            return null;
        }
        try
        {
            return JObject.Parse(File.ReadAllText(patchPath));
        }
        catch (Exception e)
        {
            Console.WriteLine("[launcher] launcher_patch.json parse error: " + e.Message);
            return null;
        }
    }

    /// <summary>เวอร์ชันล่าสุด — จาก data/launcher_patch.json (ถ้ามี) ไม่งั้นคืน null ให้ launcher ใช้เวอร์ชันตัวเอง</summary>
    private string ReadLatestVersion()
    {
        return (string?)TryReadPatchManifest()?["version"];
    }
}
