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
    public const int DefaultPort = 8190;

    public string BindPrefix => _webServer.Prefix;

    private readonly WebServer _webServer;
    private readonly GameServer _gameServer;
    private readonly ServerWorld _world;
    private readonly string _assetBundleDir;
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
            JObject jObject = new JObject
            {
                ["server_version"] = "5.2.1",
                ["compatible"] = true,
                ["assetbundle_index_url"] = $"http://{host}/assetbundles/Info.5.2.1.json",
                ["assetbundle_url_root"] = $"http://{host}/assetbundles/",
                // [แก้เอง] 24 ส.ค. 2026 — ผู้เล่นใหม่ (ยังไม่มี PlayerId) เข้า State.FadeOutPrologue ตรงจาก
                // NPAGetUser เลย **ไม่ผ่าน** GetFrontend/entry ก่อน ⇒ ใส่ค่านี้ที่ /knock ด้วย (เร็วกว่า
                // /entry ในลำดับ state ทั้งหมด — ทุกคนเจอ /knock ก่อนเสมอไม่ว่าใหม่/เก่า) ให้ client อ่านทัน
                // ก่อนฉากรถไฟจะเริ่มตั้งค่า — ดู ServerConfig.SkipPrologueVideo / client ที่ case State.Knock
                ["skip_prologue_video"] = ServerConfig.Current.SkipPrologueVideo
            };
            return new WebServer.JsonResponse(jObject.ToString());
        };
        _webServer.GetRoute["/notice"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
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
			if (hasSelectedCharacter && SaveStore.Peek<PlayerSave>(SaveStore.PlayerPath(entityId)) == null)
			{
				return new WebServer.JsonResponse(
					new JObject { ["error"] = "character_not_found" }.ToString(),
					HttpStatusCode.NotFound);
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
        _webServer.GetRoute["/admission"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.JsonResponse(new JObject { ["admitted"] = true }.ToString());
        _webServer.GetRoute["/entry"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.JsonResponse(new JObject
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
                ["cluster_mode"] = _clusterMode,
                // [แก้เอง] 24 ส.ค. 2026 — ให้ "ข้ามฉากรถไฟไหม" เป็นค่าที่เซิร์ฟสั่งได้ (data/config.json
                // → ServerConfig.SkipPrologueVideo) ไม่ต้อง build/แจก client ใหม่ทุกครั้งที่จะสลับ
                ["skip_prologue_video"] = ServerConfig.Current.SkipPrologueVideo
            }.ToString());
        // สร้างตัวละครใหม่ (มาจากหน้าสร้างตัวละครในโปรล็อก — PrologueManager.RequestCreatePlayer)
        //
        // 🐛 เดิมที่นี่แค่สุ่ม GUID คืนไป **ทิ้งชื่อ/เพศ/หน้าตาทั้งหมด**
        // ⇒ ตัวละครที่เพิ่งสร้างเข้าเกมมาแบบไม่มีชื่อ (log: `name=(ว่าง!) level=0 display=no`)
        //   และหน้าตาเป็นค่า default ของ client ไม่ใช่ที่ผู้เล่นปั้นไว้
        // ⇒ ตอนนี้เขียนลง `saves/players/<id>.json` ตั้งแต่ตอนสร้าง
        //   ผู้เล่นจึงเข้าเกมมาพร้อมชื่อ+หน้าตาที่ถูกต้อง แม้ client จะส่ง JSON ขั้นต่ำมาก็ตาม
        //   (ServerPlayer.LoadPersistedState เป็นคนหยิบไปใช้ ดู ServerPlayer.Persistence.cs)
        _webServer.PostRoute["/players"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            _characterService.Create(postData);
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
                    return new WebServer.BinaryReponse { Content = File.ReadAllBytes(filePath) };
                }
                // client ประกอบ URL เป็น <ชื่อ>.<crc>.bundle (ดู AssetBundleItemInfo.GetCrcName)
                // ถ้า crc ที่มันคิดไม่ตรงกับ hash ในชื่อไฟล์บนดิสก์ จะ 404 แล้วของนั้นไม่ถูกวาด
                // (เจอตอนเทส: สัตว์ไม่โผล่ในจอทั้งที่ server ส่ง AppearAnimal ครบ)
                // เทียบแบบ "ตัดส่วน hash ทิ้ง" แล้วหาไฟล์ที่เหลือชื่อตรงกันแทน
                string resolved = ResolveBundleIgnoringHash(safeName);
                if (resolved != null)
                {
                    return new WebServer.BinaryReponse { Content = File.ReadAllBytes(resolved) };
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
                    return new WebServer.BinaryReponse
                    {
                        Content = _world.Terrain.GetChunkOcean(p.x, p.y)
                    };
                };
            }
            if (url.StartsWith("/terrains/1/rivers"))
            {
                return (HttpListenerRequest request, Dictionary<string, string> postData) =>
                {
                    Point2 p = Point2FromUrl(url);
                    return new WebServer.BinaryReponse
                    {
                        Content = _world.Terrain.GetChunkRiver(p.x, p.y)
                    };
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
                return new WebServer.BinaryReponse { Content = ms.ToArray() };
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
    private string ResolveBundleIgnoringHash(string requested)
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
            if (_bundleIndex == null)
            {
                _bundleIndex = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string path in Directory.GetFiles(_assetBundleDir, "*.bundle"))
                {
                    string name = Path.GetFileName(path);
                    string trimmed = name.Substring(0, name.Length - ".bundle".Length);
                    int d = trimmed.LastIndexOf('.');
                    if (d > 0)
                    {
                        _bundleIndex[trimmed.Substring(0, d)] = path;
                    }
                }
                Console.WriteLine("[assetbundle] ทำดัชนีไฟล์ {0} ก้อน (เทียบชื่อแบบไม่สน hash)", _bundleIndex.Count);
            }
            return _bundleIndex.TryGetValue(baseName, out string found) ? found : null;
        }
    }

    private Dictionary<string, string> _bundleIndex;
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
