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
    /// <summary>
    /// เวอร์ชันของเซิร์ฟ — **ต้องขยับพร้อมกับชุดแจก** (dist/manifest.json และ version.txt)
    /// ตัวเกมเอาไปโชว์ที่หน้าไตเติ้ล (TitleMenuGroup → UpdateVersionInfo) อย่างเดียว
    /// ไม่ได้เอาไปเทียบว่าเล่นด้วยกันได้ไหม (อันนั้นใช้ field "compatible" แยก)
    /// </summary>
    public const string ServerVersion = "0.1.6";

    public static string LauncherNewsPath = Path.Combine("data", "launcher_news.json");

    /// <summary>
    /// โฟลเดอร์ไฟล์ข้อมูลเกมที่เสิร์ฟให้ client ตอน Mode.Online (ดู route "/assets/" ใน Gateway.cs)
    /// ไฟล์ต้นฉบับดึงมาจากตัวเกมเอง (Resources/offline/assets) — แก้ไฟล์พวกนี้แล้ว restart เซิร์ฟ
    /// ผู้เล่นจะได้ค่าใหม่โดยไม่ต้องโหลดตัวเกมใหม่
    /// </summary>
    public static string AssetsDir = Path.Combine("data", "assets");

    /// <summary>
    /// [4 ก.ย. 2026] โฟลเดอร์ "ไฟล์เกมฉบับจริง" ที่ launcher เอาไว้ตรวจ/ซ่อมทีละไฟล์ (เจ้าของสั่ง: ทุกเครื่องต้องไฟล์เหมือนกัน)
    /// ข้างในต้องมี <c>filelist.json</c> (สร้างด้วย tools/make-filelist.py) + ตัวไฟล์เกมจริงตาม path ในนั้น
    /// launcher โหลดเฉพาะไฟล์ที่ขาด/hash ไม่ตรง ไม่ใช่ zip ทั้งก้อน
    /// </summary>
    public static string GameFilesDir = Path.Combine("data", "gamefiles");

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
                ["max_players"] = GameServer.EffectiveMaxConnections,
                ["tps"] = Math.Round(ServerStats.Tps, 1),
                ["uptime_sec"] = Math.Round(ServerStats.UptimeSeconds, 1),
                ["latest_version"] = ReadLatestVersion()
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── manifest แพตช์ (เผื่อ launcher จะย้ายจาก GitHub Release มาอ่านที่เซิร์ฟ) ──
        _webServer.GetRoute["/launcher/version"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JObject patch = TryReadPatchManifest() ?? new JObject();
            string version = (string?)patch["Version"] ?? (string?)patch["version"] ?? ReadLatestVersion();
            string zipUrl = (string?)patch["ZipUrl"] ?? (string?)patch["zip_url"];
            string sha = (string?)patch["Sha256"] ?? (string?)patch["sha256"];
            string notes = (string?)patch["Notes"] ?? (string?)patch["notes"];
            JToken files = patch["Files"] ?? patch["files"];
            string host = request.UserHostName;
            if (string.IsNullOrEmpty(host))
            {
                host = "127.0.0.1:8190";
            }
            if (files is JArray arr)
            {
                foreach (JToken item in arr)
                {
                    if (item is not JObject fo)
                    {
                        continue;
                    }
                    string path = (string?)fo["Path"] ?? (string?)fo["path"];
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }
                    string fileName = Path.GetFileName(path);
                    if (PatchFileExists(fileName))
                    {
                        fo["Url"] = $"http://{host}/launcher/file?name={Uri.EscapeDataString(fileName)}";
                        fo["url"] = fo["Url"];
                    }
                }
            }
            JObject o = new JObject
            {
                ["ok"] = true,
                ["version"] = version,
                ["Version"] = version,
                ["zip_url"] = zipUrl,
                ["ZipUrl"] = zipUrl,
                ["sha256"] = sha,
                ["Sha256"] = sha,
                ["notes"] = notes,
                ["Notes"] = notes
            };
            if (files != null)
            {
                o["files"] = files;
                o["Files"] = files;
            }
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── [4 ก.ย. 2026] รายชื่อไฟล์เกมทั้งชุด (launcher ใช้ตรวจว่าเครื่องผู้เล่นไฟล์ครบ/ตรงไหม) ──
        //    ตอบ path + sha256 + size + url รายไฟล์ · launcher โหลดเฉพาะตัวที่ขาด/ไม่ตรง
        _webServer.GetRoute["/launcher/files"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string listPath = Path.Combine(GameFilesDir, "filelist.json");
            if (!File.Exists(listPath))
            {
                return new WebServer.JsonResponse(
                    new JObject { ["ok"] = false, ["error"] = "no_filelist", ["hint"] = listPath }.ToString(),
                    HttpStatusCode.NotFound);
            }
            try
            {
                JObject src = JObject.Parse(File.ReadAllText(listPath));
                string host = request.UserHostName;
                if (string.IsNullOrEmpty(host))
                {
                    host = "127.0.0.1:8190";
                }
                if (src["files"] is JArray files)
                {
                    foreach (JToken item in files)
                    {
                        if (item is not JObject fo)
                        {
                            continue;
                        }
                        string rel = (string?)fo["path"];
                        if (string.IsNullOrEmpty(rel))
                        {
                            continue;
                        }
                        fo["url"] = $"http://{host}/launcher/gamefile?path={Uri.EscapeDataString(rel)}";
                    }
                }
                src["ok"] = true;
                return new WebServer.JsonResponse(src.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("[launcher] อ่าน filelist.json ไม่สำเร็จ: " + e.Message);
                return new WebServer.JsonResponse(
                    new JObject { ["ok"] = false, ["error"] = "filelist_unreadable" }.ToString(),
                    HttpStatusCode.InternalServerError);
            }
        };

        // ── เสิร์ฟไฟล์เกมรายตัวตาม relative path (คู่กับ /launcher/files) ──
        _webServer.GetRoute["/launcher/gamefile"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string rel = request.QueryString["path"] ?? "";
            if (!TryResolveGameFile(rel, out string full))
            {
                return new WebServer.TextResponse("text/plain", "ไม่พบไฟล์ " + rel, HttpStatusCode.NotFound);
            }
            return new WebServer.BinaryReponse
            {
                Content = File.ReadAllBytes(full),
                ContentType = "application/octet-stream"
            };
        };

        _webServer.GetRoute["/launcher/file"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string name = request.QueryString["name"] ?? "";
            name = Path.GetFileName(name);
            if (!PatchFileExists(name))
            {
                return new WebServer.TextResponse("text/plain", "ไม่พบไฟล์แพท " + name, HttpStatusCode.NotFound);
            }
            byte[] bytes = File.ReadAllBytes(PatchFilePath(name));
            return new WebServer.BinaryReponse { Content = bytes, ContentType = "application/octet-stream" };
        };
    }

    /// <summary>
    /// แปลง relative path จาก client เป็น path จริงใต้ <see cref="GameFilesDir"/> อย่างปลอดภัย
    /// กัน path traversal: ห้าม rooted / ".." และ path เต็มต้องอยู่ใต้ root จริง ๆ (เทียบหลัง GetFullPath)
    /// </summary>
    private static bool TryResolveGameFile(string rel, out string full)
    {
        full = string.Empty;
        if (string.IsNullOrWhiteSpace(rel))
        {
            return false;
        }
        rel = rel.Replace('\\', '/');
        if (rel.Contains("..", StringComparison.Ordinal) || Path.IsPathRooted(rel))
        {
            return false;
        }
        string root = Path.GetFullPath(GameFilesDir);
        string candidate = Path.GetFullPath(Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar)));
        string rootWithSep = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        if (!File.Exists(candidate))
        {
            return false;
        }
        full = candidate;
        return true;
    }

    private static string PatchDir()
    {
        string dataDir = Path.GetDirectoryName(Path.GetFullPath(LauncherNewsPath)) ?? "data";
        return Path.Combine(dataDir, "patches");
    }

    private static string PatchFilePath(string fileName)
    {
        return Path.Combine(PatchDir(), fileName);
    }

    private static bool PatchFileExists(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.IndexOf("..", StringComparison.Ordinal) >= 0)
        {
            return false;
        }
        if (!fileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        return File.Exists(PatchFilePath(fileName));
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
