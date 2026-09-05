using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// Gateway — ดูรายละเอียดที่ docs/server/Gateway.md

public partial class Gateway
{
    /// <summary>
    /// ตอบไฟล์ bundle แบบสตรีม + ETag — ชื่อไฟล์ของ Unity มี hash อยู่แล้ว ไฟล์เดิม = เนื้อเดิมเสมอ
    /// ⇒ ใช้ชื่อไฟล์เป็น ETag ได้ตรง ๆ · client ที่มีของอยู่แล้วจะได้ 304 ตัวเปล่า ไม่โหลดซ้ำ
    /// </summary>
    private static WebServer.Response BundleFile(string path)
    {
        return new WebServer.FileResponse(path, "\"" + Path.GetFileName(path) + "\"");
    }

    /// <summary>ที่อยู่ฐานของ asset bundle — ตั้งใน config ได้ (ว่าง = เสิร์ฟจากเซิร์ฟเองเหมือนเดิม)</summary>
    private static string AssetBundleBase(string host)
    {
        string b = (ServerConfig.Current.AssetBundleUrlBase ?? string.Empty).Trim().TrimEnd('/');
        return b.Length > 0 ? b : $"http://{host}";
    }

    public const int DefaultPort = 8190;

    public string BindPrefix => _webServer.Prefix;

    private readonly WebServer _webServer;
    /// <summary>IP|เวอร์ชัน → เวลาที่ log "ปฏิเสธ client" ล่าสุด (กัน log รัวจากจุดสถานะหน้าไตเติ้ล)</summary>
    private readonly Dictionary<string, System.DateTime> _knockRejectLogged = new Dictionary<string, System.DateTime>(StringComparer.Ordinal);
    private readonly GameServer _gameServer;
    private readonly ServerWorld _world;
    private readonly string _assetBundleDir;

    /// <summary>
    /// [Android · ROADMAP-ANDROID ด่าน 1] โฟลเดอร์ bundle ชุด Android (`--assetbundles-android`) — layout เดียวกับ Windows
    /// (`<name>.<hash>.bundle` + `Info.5.2.1.json`) · client มือถือส่ง `/knock?platform=Android` ⇒ ชี้ index/root ชุดนี้แทน
    /// ไม่ตั้ง = Android ได้ชุด Windows (โหลดไม่ขึ้น) เหมือนเดิม
    /// </summary>
    public static string AssetBundleAndroidDir;

    private static bool IsAndroidPlatform(string platform)
    {
        return !string.IsNullOrEmpty(platform) && platform.IndexOf("android", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool HasAndroidBundles => !string.IsNullOrEmpty(AssetBundleAndroidDir) && Directory.Exists(AssetBundleAndroidDir);

    /// <summary>
    /// "soundbanks$android$ko_kr$voice_event.bnk.bytes.&lt;hash&gt;.bundle" → ไฟล์ en_us ชื่อเดียวกัน (ไม่สน hash) · ไม่ใช่ voice bank แยกภาษา = null
    /// </summary>
    private string ResolveVoiceBankFallback(string aName)
    {
        const string prefix = "soundbanks$android$";
        if (string.IsNullOrEmpty(aName) || !aName.StartsWith(prefix, StringComparison.Ordinal)) return null;
        string rest = aName.Substring(prefix.Length);           // "<lang>$voice_event.bnk.bytes.<hash>.bundle"
        int dollar = rest.IndexOf('$');
        if (dollar <= 0) return null;
        string lang = rest.Substring(0, dollar);
        if (lang.Equals("en_us", StringComparison.OrdinalIgnoreCase)) return null;
        string enName = prefix + "en_us" + rest.Substring(dollar);
        string direct = Path.Combine(AssetBundleAndroidDir, enName);
        if (File.Exists(direct)) return direct;
        return ResolveBundleIgnoringHash(enName, AssetBundleAndroidDir, ref _bundleIndexAndroid);
    }

    /// <summary>
    /// [3 ก.ย. 2026] cluster_mode ที่ตอบ client — มือถือได้ค่าจาก ServerConfig.Android.ClusterMode (ถ้าตั้ง)
    /// เพราะเมนูของเกมต้นฉบับถูกกรองด้วย ClusterMode (Online ซ่อน "กลับหน้าไตเติ้ล") และ APK แก้โค้ดไม่ได้
    /// </summary>
    private string ClusterModeFor(bool android)
    {
        if (!android) return _clusterMode;
        string o = ServerConfig.Current?.Android?.ClusterMode;
        return string.IsNullOrWhiteSpace(o) ? _clusterMode : o.Trim();
    }
    private readonly int _radiotowerPort;
    private readonly string _publicHost;
    private readonly string _reportsDir;
    private readonly string _clusterMode;
    private readonly string _adminToken;
    private readonly CharacterService _characterService;

    /// <param name="radiotowerPort">พอร์ตจริงของ RadiotowerServer (ไม่ใช่ค่าคงที่)</param>
    /// <param name="publicHost">
    /// ที่อยู่ที่ client ใช้ต่อ TCP (พอร์ตเกม 8191 / แชท 8192) — ไม่มี port นำหน้าตามหลัง
    /// เช่น "192.168.1.39" หรือ "127.0.0.1" (เมื่อเล่นผ่าน Cloudflare Tunnel ที่เครื่องผู้เล่นเอง)
    /// ถ้าไม่ระบุ จะใช้ host จากคำขอของ client เอง (เหมาะกับเล่นในวงแลน)
    /// </param>
    /// <param name="reportsDir">โฟลเดอร์เก็บรายงานบัค/ข้อเสนอแนะที่ client ส่งมา (/reports)</param>
    /// <param name="clusterMode">
    /// ค่า cluster_mode ที่ /entry ตอบกลับไป client — เดิม hardcode "SingleMode" เสมอ
    /// ทำให้ client ปิดฟีเจอร์ที่เช็ค ClusterMode == Mode.Online ทั้งหมด (แชทส่วนตัว/ตลาด/สารานุกรม ฯลฯ)
    /// เปิดด้วย --cluster-mode Online (ดู Program.cs) — ค่า default ยังเป็น SingleMode เหมือนเดิม
    /// เพื่อไม่กระทบเซิร์ฟที่รันอยู่แล้ว (Online mode ยังไม่ได้เทสทุก UI ที่แยกสาขาตามโหมดนี้)
    /// </param>
    /// <param name="adminToken">
    /// [แก้เอง] 24 ส.ค. 2026 — /admin/* เดิมไม่มี auth เลย (คอมเมนต์เดิมสมมติว่า bind แค่ localhost/LAN)
    /// พอเอาเซิร์ฟไปตั้งบน VPS จริง (เปิดพอร์ต 8190 ออกอินเทอร์เน็ต) ใครก็เปิดเบราว์เซอร์เข้า /admin ได้
    /// โดยไม่ต้องมีรหัสอะไรเลย ทั้งที่มี endpoint สั่งเตะผู้เล่น/เทเลพอร์ต/สั่ง cheat/แก้ config ได้ —
    /// ใส่ token ไว้กันตรงนี้ ถ้าไม่ระบุ (ค่าว่าง) = พฤติกรรมเดิมทุกอย่าง (ไม่ auth เหมือนเดิม
    /// เหมาะกับรันในเครื่อง/LAN เท่านั้น) ระบุด้วย --admin-token ถ้าจะ expose ออกอินเทอร์เน็ต
    /// </param>
    public Gateway(GameServer gameServer, ServerWorld world, int port = DefaultPort, string assetBundleDir = null, int radiotowerPort = RadiotowerServer.DefaultPort, string publicHost = null, string reportsDir = null, string clusterMode = "SingleMode", string adminToken = null)
    {
        _gameServer = gameServer;
        _world = world;
        _assetBundleDir = assetBundleDir;
        _radiotowerPort = radiotowerPort;
        _publicHost = publicHost;
        _reportsDir = reportsDir;
        _clusterMode = string.IsNullOrEmpty(clusterMode) ? "SingleMode" : clusterMode;
        _adminToken = adminToken;
        _webServer = new WebServer(port);
        _characterService = new CharacterService(_gameServer);

        // /knock: client ใช้เช็กเวอร์ชัน + ที่อยู่ assetbundle (ตอบ URL ชี้มาที่ server ตัวเอง
        // เพราะ Nexon CDN ตายแล้ว; assetbundle serve จากโฟลเดอร์ของตัวเกมที่เครื่องนี้)
        _webServer.GetRoute["/knock"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string host = request.UserHostName;
            if (string.IsNullOrEmpty(host))
            {
                host = "127.0.0.1:" + port;
            }
            // เวอร์ชันที่ตัวเกมส่งมา (CurrentBundleVersion.GetClientVersion ฝั่ง client)
            string clientVersion = request.QueryString["version"] ?? "";
            string clientPlatform = request.QueryString["platform"] ?? "";
            bool android = IsAndroidPlatform(clientPlatform);
            // [3 ก.ย. 2026] APK ชุดเราแปะ build=android-0.1.x มาด้วย (มือถือของแท้ล้วนไม่มี) — ดู ClientModPolicy.RequiredAndroidBuild
            string clientBuild = request.QueryString["build"] ?? "";
            // [Android] เกมมือถือของแท้ส่งเวอร์ชันฐาน "5.2.1" (ไม่มี CustomVersion/ตัวอัปเดต) — ยอมเฉพาะเมื่อมีชุด bundle Android ให้
            bool clientVersionOk = ClientModPolicy.Current.IsClientVersionAllowed(clientVersion)
                                   || (android && HasAndroidBundles && ClientModPolicy.Current.AllowRawAndroidClient);
            // มือถือต้องผ่านนโยบายเวอร์ชัน APK ด้วย (ว่าง = ไม่บังคับ — ค่าเริ่มต้น)
            if (android && !ClientModPolicy.Current.IsAndroidBuildAllowed(clientBuild))
            {
                clientVersionOk = false;
            }
            if (!clientVersionOk)
            {
                // [3 ก.ย. 2026] จุดสถานะหน้าไตเติ้ล (client/Durango.Offline/Server.cs RefreshServerStatus)
                // ยิง /knock?version=5.2.1 ทุก 10 วิ ตลอดเวลาที่ผู้เล่นค้างหน้าไตเติ้ล — ไม่ใช่การล็อกอิน
                // ⇒ log บรรทัดนี้รัวจนเจ้าของนึกว่ามีบั๊ก · จำกัดให้ขึ้น 1 ครั้ง/IP+เวอร์ชัน/10 นาที
                //    (ของจริงที่ควรเห็นคือ "client เก่าพยายามเข้า" ซึ่งครั้งแรกยังขึ้นเหมือนเดิม)
                string who = (request.RemoteEndPoint?.Address?.ToString() ?? "?") + "|" + clientVersion;
                System.DateTime lastLogged;
                bool quiet = _knockRejectLogged.TryGetValue(who, out lastLogged) && (System.DateTime.UtcNow - lastLogged).TotalMinutes < 10;
                if (!quiet)
                {
                    _knockRejectLogged[who] = System.DateTime.UtcNow;
                    if (android)
                    {
                        Console.WriteLine("[knock] ปฏิเสธมือถือ build \"{0}\" (version {1}) — ต้องการ APK \"{2}\" ⇒ ส่งไปโหลด APK ใหม่ (จาก {3} · จะไม่ log ซ้ำใน 10 นาที)",
                            clientBuild, clientVersion, ClientModPolicy.Current.RequiredAndroidBuild, who.Split('|')[0]);
                    }
                    else
                    {
                        Console.WriteLine("[knock] ปฏิเสธ client เวอร์ชัน \"{0}\" — ต้องการ \"{1}\" ⇒ ส่งไปโหลดใหม่ (จาก {2} · ถ้าเป็น 5.2.1 = จุดสถานะหน้าไตเติ้ล ไม่ใช่ล็อกอิน · จะไม่ log ซ้ำใน 10 นาที)",
                            clientVersion, ClientModPolicy.Current.RequiredVersionOfClient, who.Split('|')[0]);
                    }
                }
            }
            else if (android && !string.IsNullOrEmpty(clientBuild))
            {
                // เห็น APK ชุดเราครั้งแรกจาก IP นี้ — log ไว้ให้รู้ว่ามือถือรุ่นไหนเข้ามาบ้าง (ไม่รัว: 1 ครั้ง/IP+build/10 นาที)
                string who = "android|" + (request.RemoteEndPoint?.Address?.ToString() ?? "?") + "|" + clientBuild;
                System.DateTime lastLogged;
                if (!_knockRejectLogged.TryGetValue(who, out lastLogged) || (System.DateTime.UtcNow - lastLogged).TotalMinutes >= 10)
                {
                    _knockRejectLogged[who] = System.DateTime.UtcNow;
                    Console.WriteLine("[knock] มือถือ APK build \"{0}\" (version {1}) จาก {2}", clientBuild, clientVersion, who.Split('|')[1]);
                }
            }
            JObject jObject = new JObject
            {
                // [แก้เอง] 30 ส.ค. 2026 — เวอร์ชันเซิร์ฟของเราเอง (เจ้าของสั่งให้เป็น 0.0.3beta)
                // ปลอดภัยที่จะใส่ค่าอะไรก็ได้: client เอาไป **แสดงผลอย่างเดียว**
                // (TitleMenuGroup.cs → UserControl.UpdateVersionInfo) ส่วนการเช็คว่าเล่นด้วยกันได้ไหม
                // ใช้ field "compatible" แยกต่างหาก ⇒ ไม่กระทบความเข้ากันได้
                //
                // [แก้เอง] 31 ส.ค. 2026 — เดินเลขให้ตรงกับเวอร์ชันชุดแจก (dist/manifest.json + version.txt)
                // 🐛 เดิมสองเลขนี้เดินคนละทาง: ปล่อยชุดเกม 0.1.0 แล้วผู้เล่นยังเห็น "0.0.3" บนหน้าจอ
                //    เพราะนี่คือเลขที่เกมเอาไปโชว์ ⇒ นึกว่าอัปเดตไม่ติด/เซิร์ฟรันซ้อนกัน
                //    ต่อจากนี้ **ขยับพร้อมกันทุกรุ่น** จะได้ตอบได้ทันทีว่าใครอยู่รุ่นไหน
                ["server_version"] = ServerVersion,
                // [แก้เอง] 31 ส.ค. 2026 — เดิมตอบ true เสมอ ⇒ ระบบบังคับอัปเดตที่ตัวเกมมีอยู่แล้ว
                // ไม่เคยถูกใช้เลย (client TitleMenuGroup.cs:926 → RedirectToDownloadUrl)
                // ตอนนี้เทียบกับ Client.RequiredVersionOfClient — ว่าง = ไม่บังคับ (ค่าเริ่มต้น)
                ["compatible"] = clientVersionOk,
                // มือถือพาไปโหลด APK (ไม่ใช่ zip ของ PC)
                ["download_url"] = (android ? ClientModPolicy.Current.AndroidDownloadUrl : ClientModPolicy.Current.DownloadUrl) ?? "",
                ["patch_notes"] = (string)(TryReadPatchManifest()?["notes"] ?? TryReadPatchManifest()?["Notes"]) ?? "",
                ["patch_version"] = (string)(TryReadPatchManifest()?["version"] ?? TryReadPatchManifest()?["Version"]) ?? "",
                // [Android] มือถือได้ชุด Android แยกโฟลเดอร์ (Unity bundle ผูก platform — ใช้ข้ามกันไม่ได้)
                // [4 ก.ย. 2026] ถ้าตั้ง AssetBundleUrlBase ไว้ ให้ client ไปโหลด bundle ที่นั่น (nginx)
                // แทนที่จะให้ process เกมอ่านไฟล์เอง — ดูเหตุผลที่ ServerConfig.AssetBundleUrlBase
                ["assetbundle_index_url"] = android && HasAndroidBundles
                    ? $"{AssetBundleBase(host)}/assetbundles/android/Info.5.2.1.json"
                    : $"{AssetBundleBase(host)}/assetbundles/Info.5.2.1.json",
                ["assetbundle_url_root"] = android && HasAndroidBundles
                    ? $"{AssetBundleBase(host)}/assetbundles/android/"
                    : $"{AssetBundleBase(host)}/assetbundles/",
                // [แก้เอง] 24 ส.ค. 2026 — ผู้เล่นใหม่ (ยังไม่มี PlayerId) เข้า State.FadeOutPrologue ตรงจาก
                // NPAGetUser เลย **ไม่ผ่าน** GetFrontend/entry ก่อน ⇒ ใส่ค่านี้ที่ /knock ด้วย (เร็วกว่า
                // /entry ในลำดับ state ทั้งหมด — ทุกคนเจอ /knock ก่อนเสมอไม่ว่าใหม่/เก่า) ให้ client อ่านทัน
                // ก่อนฉากรถไฟจะเริ่มตั้งค่า — ดู ServerConfig.SkipPrologueVideo / client ที่ case State.Knock
                ["skip_prologue_video"] = ServerConfig.Current.SkipPrologueVideo,

                // [แก้เอง] 30 ส.ค. 2026 — ข้อมูลไว้โชว์ในเมนู "Online Mode" ของหน้า Main
                // (เจ้าของสั่ง: "ดึงชื่อเซิฟจากไอพีเซิฟที่กรอกใน server.txt" + อนาคตจะมีหลายเซิร์ฟหลายรายการ)
                // client แค่ยิง GET /knock ไปที่แต่ละ IP ใน server.txt แล้วเอา server_name มาแสดงเป็นชื่อรายการ
                // เป็น field เพิ่มล้วน ๆ — client เก่าที่ไม่รู้จักก็ไม่พัง
                ["server_name"] = _world.ServerName,
                ["online_players"] = ServerStats.OnlinePlayers,
                ["region_name"] = _world.Terrain?.Info?.region_template ?? ""
                , ["client_mod"] = ClientModPolicy.Current.ToJson()
                , ["cluster_mode"] = ClusterModeFor(android)
            };
            return new WebServer.JsonResponse(jObject.ToString());
        };
        _webServer.GetRoute["/notice"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.JsonResponse("{}");
        // [เพิ่มเอง] 31 ส.ค. 2026 — ระบบ "คำตอบจากฝ่ายบริการลูกค้า"
        // client ยิงมาทุกครั้งที่เข้าฉากหลัก **เฉพาะตอน Mode.Online** (CustomerServiceSystem.cs:44)
        // เดิมเราไม่มี route นี้ ⇒ ตอบ 400 ทุกครั้ง (เห็นใน log ว่า `GET /cs/answer -> 400`)
        // ไม่ทำให้เล่นไม่ได้ แต่รกล็อกและทำให้ไล่ปัญหาจริงยาก
        // client อ่านแค่ฟิลด์ `updated_at` (Json.Read<JObject>().Get<double?>) — ไม่มีก็ถือว่าไม่มีคำตอบใหม่
        _webServer.GetRoute["/cs/answer"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.JsonResponse("{}");
        // /sessions: client ส่ง JSON ของ PlayerContext (เกาะตัวเอง) มาในฟิลด์ "player"
        // server ดึง entity id/ชื่อ/เลเวล/โมเดล/สกิล เก็บเป็น PlayerData แล้วคืน session_token
        _webServer.PostRoute["/sessions"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string entityId = Guid.NewGuid().ToString();
            string generatedEntityId = entityId;
            string player = postData.Get("player");
            // [debug] ดูว่า client ส่ง player field มาไหม — ใช้ไล่ปัญหา token ไม่ตรง id
            Console.WriteLine($"[gateway] /sessions player field: {(string.IsNullOrEmpty(player) ? "(ไม่มี)" : player.Length + " bytes: " + player.Substring(0, Math.Min(player.Length, 300)))}");
            GameServer.PlayerData data = new GameServer.PlayerData();
            if (!string.IsNullOrEmpty(player))
            {
                try
                {
                    JObject p = JObject.Parse(player);
                    string name = null;
                    JToken appear = p["appear_player"];
                    if (appear != null && appear.Type == JTokenType.Object)
                    {
                        entityId = appear.Value<string>("EntityId") ?? appear.Value<string>("entity_id") ?? entityId;
                        name = appear.Value<string>("Name") ?? appear.Value<string>("name") ?? name;
                        data.Level = appear.Value<int?>("Level") ?? 0;
                        data.EntityType = appear.Value<ushort?>("EntityType") ?? 0;
                        JToken display = appear["Display"];
                        if (display != null && display.Type == JTokenType.Object)
                        {
                            data.DisplayJson = display.ToString(Newtonsoft.Json.Formatting.None);
                        }
                    }
                    JToken info = p["player_info"];
                    if (info != null && info.Type == JTokenType.Object)
                    {
                        entityId = info.Value<string>("player_entity_id") ?? entityId;
                        // เจอตอนเทสกับเกมจริง: ชื่อ/เลเวลจริงอยู่ใน player_info ไม่ใช่ appear_player
                        // เดิมอ่านแค่ appear_player.Level ทำให้ตัวละคร Lv.60 เข้ามาเป็น Lv.1
                        // และชื่อว่างจนโชว์เป็น GUID
                        string infoName = info.Value<string>("player_name");
                        if (!string.IsNullOrEmpty(infoName))
                        {
                            name = infoName;
                        }
                        int infoLevel = info.Value<int?>("player_level") ?? 0;
                        if (infoLevel > data.Level)
                        {
                            data.Level = infoLevel;
                        }
                    }
                    JToken skills = p["skills"];
                    if (skills != null)
                    {
                        data.SkillsJson = skills.ToString(Newtonsoft.Json.Formatting.None);
                    }
                    data.SkillPoints = p.Value<int?>("skill_points") ?? 0;
                    JToken known = p["known_skills"];
                    if (known != null)
                    {
                        data.KnownSkillsJson = known.ToString(Newtonsoft.Json.Formatting.None);
                    }
                    entityId = p.Value<string>("EntityId") ?? p.Value<string>("entity_id") ?? entityId;
                    name = p.Value<string>("player_name") ?? name;
                    data.EntityId = entityId;
                    data.Name = name ?? "";
                    if (!string.IsNullOrEmpty(name))
                    {
                        _gameServer.RegisterName(entityId, name);
                    }
                    _gameServer.RegisterPlayerData(data);
                    Console.WriteLine($"[gateway] session player: {entityId} name={(string.IsNullOrEmpty(name) ? "(ว่าง!)" : name)} level={data.Level} type={data.EntityType} display={(string.IsNullOrEmpty(data.DisplayJson) ? "no" : "yes")}");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[gateway] /sessions player parse failed: " + e.Message);
                }
            }
			// No selected id is the pre-character-creation flow.  Keep its session
			// temporary; never fall back to the latest localhost character because
			// that auto-connected an unrelated old save and attached it to this owner.
			bool hasSelectedCharacter = !string.Equals(entityId, generatedEntityId, StringComparison.Ordinal);
            // client บางเส้นทาง (เช่นเพิ่งสร้างตัวละครเสร็จ) ส่ง player JSON มาแค่ entity id
            // ⇒ เติมชื่อ/เลเวล/หน้าตาจากไฟล์เซฟให้ครบ ไม่งั้น token ที่ออกไปพก PlayerData เปล่า
            //   (ตัว ServerPlayer โหลดเซฟเองอยู่แล้ว แต่ AccountStore.TryClaim ข้างล่างใช้ชื่อจากตรงนี้)
            //
            // 🐛 [แก้เอง] เดิม gate ทั้ง block (รวม Level) ไว้หลังเงื่อนไข "Name หรือ DisplayJson ว่าง"
            // เดียว — แต่เส้นทาง "เลือกตัวละครเดิม" (title screen, ไม่ใช่สร้างใหม่) client ส่ง Name/
            // DisplayJson มาครบอยู่แล้ว (จำได้จากตอนเลือก) ⇒ ทั้ง block ถูกข้ามไปเลย รวมถึง Level ด้วย
            // ⇒ data.Level ค้างที่ 0 (ค่า default ตอน client ไม่ได้ส่ง Level มาเลย) ⇒ AppearPlayer ส่ง
            // เลเวล 0 ⇒ RecipeSelectorGroup.IsValidCategoryItem (client) ซ่อนสูตรทุกอันที่ MinLevel > 0
            // (เช่นขวาน MinLevel=1) ทั้งที่เซฟจริงมีเลเวลถูกต้อง — ผู้เล่นรายงาน "ขวานไม่ขึ้นในเมนูคราฟต์"
            // แก้โดยแยก Level/EntityType ออกมาเช็ค+เติมเอง ไม่ผูกกับเงื่อนไข Name/DisplayJson อีกต่อไป
            if (!string.IsNullOrEmpty(data.EntityId))
            {
                PlayerSave known = SaveStore.Peek<PlayerSave>(SaveStore.PlayerPath(data.EntityId));
                if (known != null)
                {
                    if (string.IsNullOrEmpty(data.Name) && !string.IsNullOrEmpty(known.Name))
                    {
                        data.Name = known.Name;
                        _gameServer.RegisterName(data.EntityId, known.Name);
                    }
                    if (string.IsNullOrEmpty(data.DisplayJson) && !string.IsNullOrEmpty(known.DisplayJson))
                    {
                        data.DisplayJson = known.DisplayJson;
                    }
                    if (data.Level <= 0 && known.Level > 0)
                    {
                        data.Level = known.Level;
                    }
                    if (data.EntityType == 0 && known.EntityType != 0)
                    {
                        data.EntityType = known.EntityType;
                    }
                    _gameServer.RegisterPlayerData(data);
                    Console.WriteLine($"[gateway] เติมข้อมูลตัวละคร {data.EntityId} จากไฟล์เซฟ: " +
                        $"name={(string.IsNullOrEmpty(data.Name) ? "(ว่าง!)" : data.Name)} level={data.Level} " +
                        $"display={(string.IsNullOrEmpty(data.DisplayJson) ? "no" : "yes")}");
                }
            }
            // H-1: entity id เป็นของสาธารณะ (มากับ AppearPlayer ที่ broadcast ให้ทุกคน)
            // ก่อนออก token ต้องเช็คก่อนว่า "คนขอเป็นเจ้าของ id นี้จริงไหม" (ดู AccountStore)
			string remoteIp = request?.RemoteEndPoint?.Address?.ToString() ?? "?";
			// 🐛 [แก้เอง] 29 ส.ค. 2026 — เดิมตอบ 404 character_not_found ทันทีถ้า id ที่อ้างมาไม่มีไฟล์เซฟ
			// ⇒ ผู้เล่นกด "สร้างตัวละคร" แล้วเด้ง "[404] Not Found / Login failed" ทุกครั้ง
			//
			// สาเหตุ: เส้นทาง "กดปุ่มเลือกเซิร์ฟเองที่หน้าไตเติ้ล" ฝั่ง client เรียก Server.BeginServer()
			// ซึ่งสร้าง **ตัวละคร local dummy ของเกมรีเทล** (Level 60, ชื่อ = 8 ตัวแรกของ GUID,
			// title "영원한 개척자") แล้วแนบ id นั้นมากับ /sessions ทั้งที่ผู้เล่นยังไม่ได้เลือก/สร้างตัวละครจริง
			// ⇒ hasSelectedCharacter = true ทั้งที่ยังไม่มีตัวละคร ⇒ ชน 404 (id สุ่มใหม่ทุกครั้ง ไม่มีวันมีเซฟ)
			//
			// แก้ให้ตรงกับเจตนาเดิมของโค้ด (ดูคอมเมนต์ "No selected id is the pre-character-creation flow"):
			// id ที่ไม่มีเซฟบนเซิร์ฟนี้ = ถือว่า "ยังไม่ได้เลือกตัวละคร" → ออก session ชั่วคราวให้ไปสร้างตัวละครต่อ
			// ไม่ได้ลดความปลอดภัย เพราะด่านกันสวมสิทธิ์จริงคือ AccountStore.TryClaim ข้างล่าง ซึ่งทำงานเฉพาะ
			// ตัวละครที่ "มีอยู่จริง" อยู่แล้ว
			if (hasSelectedCharacter && SaveStore.Peek<PlayerSave>(SaveStore.PlayerPath(entityId)) == null)
			{
				Console.WriteLine($"[gateway] {remoteIp} อ้าง id {entityId} ที่ไม่มีเซฟ " +
					$"(ตัวละคร local ของ client) ⇒ ถือว่ายังไม่ได้เลือกตัวละคร ให้ไปสร้างใหม่");
				entityId = generatedEntityId;
				data = new GameServer.PlayerData { EntityId = entityId, Name = "" };
				_gameServer.RegisterPlayerData(data);
				hasSelectedCharacter = false;
			}
			if (hasSelectedCharacter && !AccountStore.TryClaim(entityId, data.Name, remoteIp, postData.Get("account_id"), out string denyReason))
            {
                Console.WriteLine($"[account] ปฏิเสธ {remoteIp} ที่อ้างเป็น {entityId} ({data.Name}): {denyReason}");
                return new WebServer.JsonResponse(
                    new JObject { ["error"] = denyReason }.ToString(),
                    System.Net.HttpStatusCode.Forbidden);
            }

            // GP-12: token ต้องเป็นความลับที่ server ออกเอง — เดิมคืน entity id ตรง ๆ
            // ใครรู้ entity id ของคนอื่น (เห็นได้จาก AppearPlayer ทุก packet) ก็สวมรอยได้เลย
            data.EntityId = entityId;
            // [3 ก.ย. 2026] จดว่า client นี้เป็นเครื่องแบบไหน (ฟอร์มจาก Platform.BuildSessionForm ของเกมต้นฉบับ:
            // platform=Android/WindowsPlayer, os_version=…) — ServerPlayer ใช้ตัดสินว่าต้องส่งอะไรแบบ "มือถือ"
            string sessionPlatform = postData.Get("platform");
            if (!string.IsNullOrEmpty(sessionPlatform)) data.Platform = sessionPlatform;
            string sessionOs = postData.Get("os_version");
            if (!string.IsNullOrEmpty(sessionOs)) data.OsVersion = sessionOs;
            string sessionToken = _gameServer.IssueSession(entityId, data);
            return new WebServer.JsonResponse(new JObject
            {
                ["user_id"] = entityId,
                ["session_token"] = sessionToken
            }.ToString());
        };
        // GP-15: หน้าเลือกตัวละคร (title screen) ถามอันนี้ว่า "IP นี้เคยสร้างตัวละครอะไรไว้บ้าง"
        // เดิมไม่มี route นี้เลย ⇒ client (ดู ForceSetCluster ใน TitleMenuUserControlBase.cs) ต้องใช้
        // ตัวแปรในหน่วยความจำแทน ซึ่งหายไปทุกครั้งที่ปิดเกม ⇒ ตัวละครเก่ายังอยู่ในเซฟจริง แต่บังคับสร้างใหม่
        // ตลอด — ใช้ IP เดียวกับที่ AccountStore ผูก entity id ไว้อยู่แล้ว (ดู AccountStore.FindByIp)
        _webServer.PostRoute["/accounts"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            _characterService.ListAccounts(request, postData.Get("account_id"));
        // [4 ก.ย. 2026] /assets/manifest — sha256 ของ JSON ทุกไฟล์ที่เซิร์ฟเสิร์ฟ + digest รวม
        // client/แอดมินเทียบได้ว่าโหลดข้อมูลชุดเดียวกับเซิร์ฟจริง ("ทุกเครื่องใช้ไฟล์เดียวกัน")
        _webServer.GetRoute["/assets/manifest"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JObject o = new JObject { ["digest"] = GameData.ManifestDigest ?? "" };
            JObject files = new JObject();
            if (GameData.Manifest != null)
            {
                foreach (KeyValuePair<string, string> kv in GameData.Manifest)
                {
                    files[kv.Key] = kv.Value;
                }
            }
            o["files"] = files;
            o["recipes_sha256"] = RecipeJsonLoader.LoadedSha256 ?? "";
            return new WebServer.JsonResponse(o.ToString());
        };
        _webServer.GetRoute["/admission"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.JsonResponse(new JObject { ["admitted"] = true }.ToString());
        _webServer.GetRoute["/entry"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            // [3 ก.ย. 2026] เกมต้นฉบับยิง /entry?entity_id=…&platform=… (TitleMenuGroup.RquestEntry) — จด platform
            // (+ build= จาก APK ชุดเรา) ไว้กับตัวละครนี้ เพราะ /sessions อาจออก token ให้ id ชั่วคราวไปก่อน
            string entryEntity = request.QueryString["entity_id"] ?? "";
            string entryPlatform = request.QueryString["platform"] ?? "";
            string entryBuild = request.QueryString["build"] ?? "";
            bool entryAndroid = IsAndroidPlatform(entryPlatform);
            _gameServer.RegisterClientInfo(entryEntity, entryPlatform, entryBuild);
            return new WebServer.JsonResponse(new JObject
            {
                // ต้องใช้พอร์ต "ที่เปิดฟังจริง" ไม่ใช่ค่าคงที่ — ไม่งั้นพอรันด้วย --game-port อื่น
                // client จะวิ่งไปพอร์ตที่ไม่มีใครฟัง (เจอตอนเทสกับเกมจริง)
                // host: ใช้ --public-host ถ้าระบุ (กรณี Cloudflare Tunnel ให้ใส่ 127.0.0.1
                // เพราะ client ต่อผ่าน cloudflared access tcp บนเครื่องตัวเอง)
                // ถ้าไม่ระบุ ใช้ host ที่ client เรียก gateway มา (กรณีเล่นในวงแลน)
                ["frontend_addresses"] = new JArray(ResolveTcpHost(request) + ":" + _gameServer.Port),
                ["radiotower_addresses"] = _radiotowerPort > 0
                    ? new JArray(ResolveTcpHost(request) + ":" + _radiotowerPort)
                    : new JArray(),
                // มือถือได้โหมดตาม ServerConfig.Android.ClusterMode (ให้เมนู "กลับหน้าไตเติ้ล" ฯลฯ โผล่)
                ["cluster_mode"] = ClusterModeFor(entryAndroid),
                // [แก้เอง] 24 ส.ค. 2026 — ให้ "ข้ามฉากรถไฟไหม" เป็นค่าที่เซิร์ฟสั่งได้ (data/config.json
                // → ServerConfig.SkipPrologueVideo) ไม่ต้อง build/แจก client ใหม่ทุกครั้งที่จะสลับ
                ["skip_prologue_video"] = ServerConfig.Current.SkipPrologueVideo
                , ["client_mod"] = ClientModPolicy.Current.ToJson()
            }.ToString());
        };
        // สร้างตัวละครใหม่ (มาจากหน้าสร้างตัวละครในโปรล็อก — PrologueManager.RequestCreatePlayer)
        //
        // 🐛 เดิมที่นี่แค่สุ่ม GUID คืนไป **ทิ้งชื่อ/เพศ/หน้าตาทั้งหมด**
        // ⇒ ตัวละครที่เพิ่งสร้างเข้าเกมมาแบบไม่มีชื่อ (log: `name=(ว่าง!) level=0 display=no`)
        //   และหน้าตาเป็นค่า default ของ client ไม่ใช่ที่ผู้เล่นปั้นไว้
        // ⇒ ตอนนี้เขียนลง `saves/players/<id>.json` ตั้งแต่ตอนสร้าง
        //   ผู้เล่นจึงเข้าเกมมาพร้อมชื่อ+หน้าตาที่ถูกต้อง แม้ client จะส่ง JSON ขั้นต่ำมาก็ตาม
        //   (ServerPlayer.LoadPersistedState เป็นคนหยิบไปใช้ ดู ServerPlayer.Persistence.cs)
        _webServer.PostRoute["/players"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            _characterService.Create(postData, request);
        _webServer.GetRoute["/terrains/1"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string content = JsonConvert.SerializeObject(_world.Terrain.Info);
            return new WebServer.JsonResponse(content);
        };
        _webServer.GetRoute["/terrains/1/whole_biomes"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.BinaryReponse { Content = _world.Terrain.Biomes };
        // /reports: รับรายงานบัค/ข้อเสนอแนะจากปุ่มรายงานในเกม (SendReportSystem)
        // เก็บเป็นไฟล์ใน data/reports/ ให้ผู้ดูแลเซิร์ฟอ่าน — client เช็คแค่ HTTP 200
        _webServer.PostRoute["/reports"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            try
            {
                string text = postData.Get("text") ?? "";
                if (text.Length > 4000)
                {
                    text = text.Substring(0, 4000);
                }
                if (string.IsNullOrWhiteSpace(text))
                {
                    return new WebServer.BadRequestResponse();
                }
                string reporter = postData.Get("reporter_id") ?? "?";
                string safeReporter = new string(reporter.Where(char.IsLetterOrDigit).Take(24).ToArray());
                if (string.IsNullOrEmpty(safeReporter))
                {
                    safeReporter = "unknown";
                }
                Directory.CreateDirectory(_reportsDir);
                string fileName = $"report_{System.DateTime.Now:yyyyMMdd_HHmmss}_{safeReporter}.txt";
                string content = string.Join(Environment.NewLine,
                    "[time]     " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    "[type]     " + (postData.Get("type") ?? ""),
                    "[category] " + (postData.Get("category") ?? ""),
                    "[reporter] " + reporter,
                    "[reportee] " + (postData.Get("reportee_id") ?? ""),
                    "[text]     " + text);
                File.WriteAllText(Path.Combine(_reportsDir, fileName), content);
                Console.WriteLine($"[report] เก็บรายงาน {fileName} จาก {reporter} ({postData.Get("type") ?? "?"})");
                return new WebServer.JsonResponse("{}");
            }
            catch (Exception e)
            {
                Console.WriteLine("[report] เขียนรายงานไม่สำเร็จ: " + e.Message);
                return new WebServer.JsonResponse("{}", System.Net.HttpStatusCode.InternalServerError);
            }
        };
        _webServer.UnhandledUrl += UnhandledUrl;

        // /admin/*: หน้าควบคุม/มอนิเตอร์เซิร์ฟสำหรับเจ้าของเซิร์ฟเท่านั้น (ไม่ใช่ผู้เล่น)
        // ดูรายละเอียดที่ Gateway.Admin.cs — ตั้งใจแยก prefix ให้ชัดว่าเป็นโซน admin
        // ไม่มี auth ซับซ้อนเพราะ Gateway บินด์แค่ localhost/วงแลนของเจ้าของเซิร์ฟเอง (ดู WebServer bind fallback)
        RegisterAdminRoutes();

        // /launcher/*: endpoint สำหรับ DinoWorld Launcher (tools/Launcher) — อ่านอย่างเดียว ไม่มี action
        // ดูรายละเอียดที่ Gateway.Launcher.cs
        RegisterLauncherRoutes();

        // /id, /id/*: หน้าสมัครไอดี DurangoID ของผู้เล่น — ดู Gateway.Ids.cs
        RegisterPlayerIdRoutes();
    }

    public void Close()
    {
        _webServer.Close();
    }

    /// <summary>
    /// ที่อยู่ host ที่ client ควรใช้ต่อ TCP (พอร์ตเกม/แชท) — อ่านจาก --public-host
    /// ถ้าระบุไว้ (กรณี Cloudflare Tunnel: 127.0.0.1 เพราะ client ต่อผ่าน
    /// cloudflared access tcp บนเครื่องตัวเอง) หรือไม่ก็ host ที่ client เรียก gateway มา
    /// (กรณีเล่นในวงแลนโดยตรง)
    /// </summary>
    private string ResolveTcpHost(HttpListenerRequest request)
    {
        if (!string.IsNullOrEmpty(_publicHost))
        {
            return _publicHost;
        }
        string host = request.UserHostName;
        if (string.IsNullOrEmpty(host))
        {
            return "127.0.0.1";
        }
        // ตัด ":port" ออก เหลือแค่ host — พอร์ตของ TCP ใช้ค่าคงที่ของพอร์ตจริง
        int colon = host.LastIndexOf(':');
        if (colon >= 0 && host.IndexOf(']') < 0)
        {
            host = host.Substring(0, colon);
        }
        return host;
    }

    private WebServer.RouteFunction UnhandledUrl(string url)
    {
        // [Android · ROADMAP-ANDROID ด่าน 2] APK ที่แพตช์ string CDN ให้ชี้มาเซิร์ฟเรา (tools/AndroidApk/) จะขอ
        // http://<เรา>/<live|release>/<android|windows|ios>/Info.5.2.1.json และ .../<bundle> ตามรูปแบบ CDN เดิม
        // ({0}=live/release ตาม Debug.isDebugBuild, {1}=platform — ดู client/Durango.Offline/Gateway.cs:47)
        // ⇒ เขียน URL ใหม่เป็น /assetbundles/android/... (มือถือ) หรือ /assetbundles/... (Windows) แล้วปล่อยไหลต่อ
        if (url.StartsWith("/live/", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/release/", StringComparison.OrdinalIgnoreCase))
        {
            string[] seg = url.Split(new[] { '/' }, 4, StringSplitOptions.RemoveEmptyEntries); // [env, platform, rest]
            if (seg.Length == 3)
            {
                url = (IsAndroidPlatform(seg[1]) ? "/assetbundles/android/" : "/assetbundles/") + seg[2];
            }
        }
        // Title-screen character selection requests /players/<entity-id> to build
        // the preview model. Keep this backed by the same PlayerSave.DisplayJson
        // that the in-game player uses, so the preview cannot drift from reality.
        if (url.StartsWith("/players/", StringComparison.OrdinalIgnoreCase))
        {
            string entityId = url.Substring("/players/".Length);
            bool cancelDeletion = entityId.EndsWith("/cancel_player_deletion", StringComparison.OrdinalIgnoreCase);
            if (cancelDeletion)
            {
                entityId = entityId.Substring(0, entityId.Length - "/cancel_player_deletion".Length);
            }
            int queryIndex = entityId.IndexOf('?');
            if (queryIndex >= 0)
            {
                entityId = entityId.Substring(0, queryIndex);
            }
            try
            {
                entityId = Uri.UnescapeDataString(entityId);
            }
            catch (Exception)
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
            }
            if (string.IsNullOrWhiteSpace(entityId) || entityId.Contains('/') || entityId.Contains('\\') || entityId.Contains(".."))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
            }

            return (HttpListenerRequest request, Dictionary<string, string> postData) =>
            {
                if (cancelDeletion && request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    return _characterService.CancelDeletion(entityId);
                }
                if (!cancelDeletion && request.HttpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    return _characterService.Delete(entityId, request);
                }
                if (!cancelDeletion && request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    return _characterService.GetInfo(entityId);
                }
                return new WebServer.NotFountResponse();
            };
        }
        // [เพิ่มเอง] 31 ส.ค. 2026 — เสิร์ฟไฟล์ข้อมูลเกม (สูตรคราฟต์ พิมพ์เขียว สกิล ฯลฯ)
        //
        // ตัวเกมมี 2 ทางในการโหลดข้อมูลพวกนี้ (ดู client/Yaml.Util/Loader.cs:164)
        //   Mode.Online  → ขอจากเซิร์ฟ  GET <gateway>/assets/<path>
        //   อื่น ๆ       → อ่านไฟล์ที่ฝังในตัวเกม  Resources.Load("offline/assets/<path>")
        //
        // เดิมเราต่อเป็น Mode.Offline เพราะเซิร์ฟไม่มี route นี้ ผลข้างเคียงคือ MenuSystem.ShowInOffline
        // อนุญาตแค่ 10 เมนู — ไม่มี Craft/Skill/Quest ⇒ ผู้เล่นไม่มีปุ่มคราฟต์เลย
        // ตอนนี้เสิร์ฟครบ 71 เส้นทางที่เกมขอแล้ว (ไฟล์ต้นฉบับดึงจากตัวเกมเอง) จึงเปิด Mode.Online ได้
        // ⇒ เมนูกลับมาครบเองโดยไม่ต้องไปแฮ็ก MenuSystem และแก้ balance ฝั่งเซิร์ฟได้โดยผู้เล่นไม่ต้องโหลดใหม่
        if (url.StartsWith("/assets/"))
        {
            string assetsDir = AssetsDir;
            if (string.IsNullOrEmpty(assetsDir) || !Directory.Exists(assetsDir))
            {
                return null;
            }
            string relative = url.Substring("/assets/".Length);
            int qIdx = relative.IndexOf('?');
            if (qIdx != -1)
            {
                relative = relative.Substring(0, qIdx);
            }
            // กัน path traversal — client ขอแค่ <โฟลเดอร์>/<ชื่อ> ธรรมดา ไม่มี .. และไม่มี path แบบเต็ม
            if (relative.Length == 0 || relative.Contains("..") || Path.IsPathRooted(relative))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
            }
            string assetPath = Path.GetFullPath(Path.Combine(assetsDir, relative.Replace('/', Path.DirectorySeparatorChar) + ".json"));
            string rootFull = Path.GetFullPath(assetsDir);
            if (!assetPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
            }
            return (HttpListenerRequest request, Dictionary<string, string> postData) =>
            {
                if (!File.Exists(assetPath))
                {
                    // ⚠️ ถ้าไฟล์ไหนขาด client จะรีทราย 5 รอบแล้วค้างหน้าโหลดนาน ๆ (Loader.CoLoadingYmls)
                    // จึงต้องดังพอให้เห็นใน log ทันที ไม่ปล่อยให้ 404 เงียบ
                    Console.WriteLine("[assets] 404 {0}", relative);
                    return new WebServer.NotFountResponse();
                }
                return new WebServer.BinaryReponse
                {
                    Content = File.ReadAllBytes(assetPath),
                    ContentType = "application/json"
                };
            };
        }
        if (url.StartsWith("/assetbundles/android/"))
        {
            // [Android] ชุด bundle มือถือ — โฟลเดอร์แยก ดัชนีแยก (ชื่อไฟล์ซ้ำกับ Windows แต่คนละ hash)
            if (!HasAndroidBundles)
            {
                return null;
            }
            string aName = Path.GetFileName(url.Substring("/assetbundles/android/".Length).Split('?')[0]);
            if (string.IsNullOrEmpty(aName) || aName.Contains(".."))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
            }
            string aPath = Path.Combine(AssetBundleAndroidDir, aName);
            return (HttpListenerRequest request, Dictionary<string, string> postData) =>
            {
                // [4 ก.ย. 2026] ตั้ง AssetBundleUrlBase = ส่งไปโหลดที่ nginx แทนการอ่านไฟล์เอง
                // APK มือถือถูกแพตช์ให้ขอ /live/android/... มาที่พอร์ตเกมตรง ๆ (ไม่ได้ใช้ url จาก /knock)
                // จึงต้องดักตรงนี้ด้วย ไม่งั้นลูปเกมยังโดนงานอ่านไฟล์เหมือนเดิม
                // [4 ก.ย. 2026] เคย redirect ไป nginx ตรงนี้ — **ใช้ไม่ได้** APK มือถือที่แพตช์ไว้
                //    ไม่ตาม 302 ⇒ โหลด bundle ไม่ได้ ค้างที่หน้า knock เข้าเกมไม่ได้เลย (เจอกับผู้เล่นจริง)
                //    เปลี่ยนมาแก้ที่ต้นเหตุแทน: สตรีมไฟล์ทีละก้อน + ETag ให้ client แคชได้
                if (File.Exists(aPath))
                {
                    return BundleFile(aPath);
                }
                string resolvedA = ResolveBundleIgnoringHash(aName, AssetBundleAndroidDir, ref _bundleIndexAndroid);
                if (resolvedA != null)
                {
                    return BundleFile(resolvedA);
                }
                // [3 ก.ย. 2026] 🐛 soundbank เสียงพากย์แยกภาษา (soundbanks$android$<lang>$voice_*.bnk) — ชุด Android มีแค่ en_us
                //   เกมขอตามค่า "เสียงพากย์" ที่ผู้เล่นตั้ง (ko_kr เป็นค่าเริ่มต้นของหลายเครื่อง) ⇒ 404 ⇒ เกมล้มที่
                //   state CheckSoundManager ("การเรียกข้อมูลล้มเหลว โปรดแตะหน้าจอ") เข้าเกมไม่ได้เลย
                //   ⇒ เสิร์ฟ bank ภาษา en_us แทน (Wwise event id ชุดเดียวกัน ต่างแค่ไฟล์เสียง) client ไม่เช็ค CRC
                string fallbackA = ResolveVoiceBankFallback(aName);
                if (fallbackA != null)
                {
                    Console.WriteLine("[assetbundle-android] {0} ไม่มี ⇒ เสิร์ฟ en_us แทน ({1})", aName, Path.GetFileName(fallbackA));
                    return BundleFile(fallbackA);
                }
                Console.WriteLine("[assetbundle-android] 404 {0} (อยู่ใน MISSING.json? ต้องหาจาก cache เครื่องผู้เล่นเก่า)", aName);
                return new WebServer.NotFountResponse();
            };
        }
        if (url.StartsWith("/assetbundles/"))
        {
            if (string.IsNullOrEmpty(_assetBundleDir) || !Directory.Exists(_assetBundleDir))
            {
                return null;
            }
            string fileName = url.Substring("/assetbundles/".Length);
            int queryIdx = fileName.IndexOf('?');
            if (queryIdx != -1)
            {
                fileName = fileName.Substring(0, queryIdx);
            }
            string safeName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(safeName) || safeName != fileName || fileName.Contains(".."))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
            }
            string filePath = Path.Combine(_assetBundleDir, safeName);
            return (HttpListenerRequest request, Dictionary<string, string> postData) =>
            {
                if (File.Exists(filePath))
                {
                    return BundleFile(filePath);
                }
                // client ประกอบ URL เป็น <ชื่อ>.<crc>.bundle (ดู AssetBundleItemInfo.GetCrcName)
                // ถ้า crc ที่มันคิดไม่ตรงกับ hash ในชื่อไฟล์บนดิสก์ จะ 404 แล้วของนั้นไม่ถูกวาด
                // (เจอตอนเทส: สัตว์ไม่โผล่ในจอทั้งที่ server ส่ง AppearAnimal ครบ)
                // เทียบแบบ "ตัดส่วน hash ทิ้ง" แล้วหาไฟล์ที่เหลือชื่อตรงกันแทน
                string resolved = ResolveBundleIgnoringHash(safeName, _assetBundleDir, ref _bundleIndex);
                if (resolved != null)
                {
                    return BundleFile(resolved);
                }
                Console.WriteLine("[assetbundle] 404 {0}", safeName);
                return new WebServer.NotFountResponse();
            };
        }
        if (url.StartsWith("/terrains/1/"))
        {
            if (url.StartsWith("/terrains/1/ocean"))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) =>
                {
                    Point2 p = Point2FromUrl(url);
                    byte[] content = _world.Terrain.GetChunkOcean(p.x, p.y);
                    return new WebServer.BinaryReponse { Content = content, ETag = TerrainETag(content) };
                };
            }
            if (url.StartsWith("/terrains/1/rivers"))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) =>
                {
                    Point2 p = Point2FromUrl(url);
                    byte[] content = _world.Terrain.GetChunkRiver(p.x, p.y);
                    return new WebServer.BinaryReponse { Content = content, ETag = TerrainETag(content) };
                };
            }
            return (HttpListenerRequest request, Dictionary<string, string> postData) =>
            {
                Point2 p = Point2FromUrl(url);
                byte[] biomes = _world.Terrain.GetChunkBiomes(p.x, p.y);
                byte[] ocean = _world.Terrain.GetChunkOcean(p.x, p.y);
                byte[] river = _world.Terrain.GetChunkRiver(p.x, p.y);
                byte[] landmark = _world.Terrain.GetChunkLandmark(p.x, p.y);
                MemoryStream ms = new MemoryStream();
                ms.Write(biomes, 0, biomes.Length);
                ms.Write(ocean, 0, ocean.Length);
                ms.Write(river, 0, river.Length);
                if (landmark != null)
                {
                    ms.Write(landmark, 0, landmark.Length);
                }
                byte[] content = ms.ToArray();
                return new WebServer.BinaryReponse { Content = content, ETag = TerrainETag(content) };
            };
        }
        return (HttpListenerRequest request, Dictionary<string, string> postData) => new WebServer.BadRequestResponse();
    }

    /// <summary>
    /// หาไฟล์ bundle ที่ "ชื่อตรงกันถ้าไม่นับส่วน hash"
    /// ชื่อบนดิสก์: <c>models$animals$brachio$brachioprefab.prefab.0331ce92....bundle</c>
    /// ที่ client ขอ:  <c>models$animals$brachio$brachioprefab.prefab.&lt;crc ที่ client คิดเอง&gt;.bundle</c>
    /// คืน path เต็มถ้าเจอ, null ถ้าไม่เจอ
    /// </summary>
    /// <summary>
    /// ETag ของข้อมูล terrain — คิดจากตัวเนื้อข้อมูลตรง ๆ
    ///
    /// biomes/ocean/river/landmark มาจากไฟล์ terrain ที่ไม่เคยเปลี่ยนระหว่างเซิร์ฟรัน
    /// (ของที่เปลี่ยนได้คือ Garden ซึ่งไปทาง packet TCP ไม่ใช่ endpoint นี้)
    /// ⇒ แคชได้ยาว ๆ ปลอดภัย และถ้าแมพเปลี่ยนจริง hash ก็เปลี่ยนตาม client จึงโหลดใหม่เอง
    /// </summary>
    private static string TerrainETag(byte[] content)
    {
        if (content == null) { content = Array.Empty<byte>(); }
        // FNV-1a 64-bit — เร็วพอสำหรับก้อนละไม่กี่ KB และไม่ต้องพึ่ง crypto
        ulong hash = 14695981039346656037UL;
        for (int i = 0; i < content.Length; i++)
        {
            hash ^= content[i];
            hash *= 1099511628211UL;
        }
        return "\"" + content.Length.ToString("x") + "-" + hash.ToString("x16") + "\"";
    }

    private string ResolveBundleIgnoringHash(string requested, string dir, ref Dictionary<string, string> index)
    {
        if (!requested.EndsWith(".bundle", StringComparison.Ordinal))
        {
            return null;
        }
        // ตัด ".bundle" แล้วตัดส่วน hash (segment สุดท้าย) ออก
        string withoutExt = requested.Substring(0, requested.Length - ".bundle".Length);
        int dot = withoutExt.LastIndexOf('.');
        if (dot <= 0)
        {
            return null;
        }
        string baseName = withoutExt.Substring(0, dot);

        lock (_bundleIndexLock)
        {
            if (index == null)
            {
                index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string path in Directory.GetFiles(dir, "*.bundle"))
                {
                    string name = Path.GetFileName(path);
                    string trimmed = name.Substring(0, name.Length - ".bundle".Length);
                    int d = trimmed.LastIndexOf('.');
                    if (d > 0)
                    {
                        index[trimmed.Substring(0, d)] = path;
                    }
                }
                Console.WriteLine("[assetbundle] ทำดัชนีไฟล์ {0} ก้อน ใน {1} (เทียบชื่อแบบไม่สน hash)", index.Count, dir);
            }
            return index.TryGetValue(baseName, out string found) ? found : null;
        }
    }

    private Dictionary<string, string> _bundleIndex;
    private Dictionary<string, string> _bundleIndexAndroid;
    private readonly object _bundleIndexLock = new object();

    private static Point2 Point2FromUrl(string url)
    {
        int num = url.LastIndexOf("/", StringComparison.Ordinal) + 1;
        string[] parts = url.Substring(num, url.Length - num).Split(',');
        return new Point2(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    public void Process()
    {
        _webServer.Process();
    }
}
