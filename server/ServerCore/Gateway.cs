using System;
using System.Collections.Generic;
using System.IO;
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

public class Gateway
{
    public const int DefaultPort = 8190;

    public string BindPrefix => _webServer.Prefix;

    private readonly WebServer _webServer;
    private readonly GameServer _gameServer;
    private readonly ServerWorld _world;
    private readonly string _assetBundleDir;
    private readonly int _radiotowerPort;
    private readonly string _publicHost;

    /// <param name="radiotowerPort">พอร์ตจริงของ RadiotowerServer (ไม่ใช่ค่าคงที่)</param>
    /// <param name="publicHost">
    /// ที่อยู่ที่ client ใช้ต่อ TCP (พอร์ตเกม 8191 / แชท 8192) — ไม่มี port นำหน้าตามหลัง
    /// เช่น "192.168.1.39" หรือ "127.0.0.1" (เมื่อเล่นผ่าน Cloudflare Tunnel ที่เครื่องผู้เล่นเอง)
    /// ถ้าไม่ระบุ จะใช้ host จากคำขอของ client เอง (เหมาะกับเล่นในวงแลน)
    /// </param>
    public Gateway(GameServer gameServer, ServerWorld world, int port = DefaultPort, string assetBundleDir = null, int radiotowerPort = RadiotowerServer.DefaultPort, string publicHost = null)
    {
        _gameServer = gameServer;
        _world = world;
        _assetBundleDir = assetBundleDir;
        _radiotowerPort = radiotowerPort;
        _publicHost = publicHost;
        _webServer = new WebServer(port);

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
                ["assetbundle_url_root"] = $"http://{host}/assetbundles/"
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
            string player = postData.Get("player");
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
            // H-1: entity id เป็นของสาธารณะ (มากับ AppearPlayer ที่ broadcast ให้ทุกคน)
            // ก่อนออก token ต้องเช็คก่อนว่า "คนขอเป็นเจ้าของ id นี้จริงไหม" (ดู AccountStore)
            string remoteIp = request?.RemoteEndPoint?.Address?.ToString() ?? "?";
            if (!AccountStore.TryClaim(entityId, data.Name, remoteIp, out string denyReason))
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
                ["radiotower_addresses"] = new JArray(ResolveTcpHost(request) + ":" + _radiotowerPort),
                ["cluster_mode"] = "SingleMode"
            }.ToString());
        _webServer.PostRoute["/players"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string entityId = Guid.NewGuid().ToString();
            string name = postData.Get("name");
            if (!string.IsNullOrEmpty(name))
            {
                _gameServer.RegisterName(entityId, name);
            }
            return new WebServer.JsonResponse(new JObject { ["entity_id"] = entityId }.ToString());
        };
        _webServer.GetRoute["/terrains/1"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string content = JsonConvert.SerializeObject(_world.Terrain.Info);
            return new WebServer.JsonResponse(content);
        };
        _webServer.GetRoute["/terrains/1/whole_biomes"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
            new WebServer.BinaryReponse { Content = _world.Terrain.Biomes };
        _webServer.UnhandledUrl += UnhandledUrl;
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
