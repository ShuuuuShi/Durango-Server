using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Durango.Offline;
using Messages;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

// ============================================================================
// Gateway.Admin — หน้าควบคุม/มอนิเตอร์เซิร์ฟสำหรับ "เจ้าของเซิร์ฟ" เท่านั้น (ไม่ใช่ผู้เล่น)
//
// ทุก route อยู่ใต้ prefix /admin/* ตั้งใจแยกให้ชัดเจนว่าเป็นโซน admin-only
// ไม่มีระบบ auth ซับซ้อน เพราะ Gateway (ดู WebServer.cs) bind อยู่แค่ localhost/วงแลนของเจ้าของเซิร์ฟเอง
// (ไม่ได้ expose ออกอินเทอร์เน็ต) — งานนี้เน้นให้ endpoint คืนสถานะ "สด" จาก in-memory ของเซิร์ฟที่รันอยู่
// จริง ๆ (ServerStats/LiveLog/ServerWorld/ServerConfig) ไม่ใช่แค่อ่านไฟล์เซฟที่จะ stale ระหว่างเซิร์ฟรัน
//
// หน้า HTML อยู่ที่ server/admin/index.html — เสิร์ฟตรงจาก route GET /admin (กันปัญหา CORS เวลาเรียก fetch)
// ============================================================================

public partial class Gateway
{
    private static readonly System.Net.Http.HttpClient HandoffHttp = new System.Net.Http.HttpClient
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    /// <summary>
    /// ส่งไฟล์เซฟผู้เล่นทั้งหมดไปเซิร์ฟปลายทาง (ใช้โดย /admin/handoff)
    ///
    /// ส่ง**ทุกตัวละครที่มีไฟล์เซฟ** ไม่ใช่เฉพาะคนที่ออนไลน์ตอนนี้ — คนที่เพิ่งออกไปเมื่อกี้
    /// ก็ต้องเจอของตัวเองบนเซิร์ฟสำรองด้วย ไม่งั้นกลับเข้ามาแล้วตัวละครหาย
    /// คืนจำนวนที่ปลายทางรับสำเร็จ
    /// </summary>
    private static async System.Threading.Tasks.Task<int> PushSavesTo(string targetUrl, string targetToken)
    {
        string playersDir = Path.Combine(SaveStore.Root, "players");
        if (!Directory.Exists(playersDir))
        {
            return 0;
        }
        string accountsDir = Path.Combine(SaveStore.Root, "accounts");
        int ok = 0;
        foreach (string file in Directory.EnumerateFiles(playersDir, "*.json"))
        {
            string entityId = Path.GetFileNameWithoutExtension(file);
            try
            {
                var fields = new Dictionary<string, string>
                {
                    ["entity_id"] = entityId,
                    ["player_json"] = File.ReadAllText(file),
                    ["token"] = targetToken
                };
                string accPath = Path.Combine(accountsDir, entityId + ".json");
                if (File.Exists(accPath))
                {
                    fields["account_json"] = File.ReadAllText(accPath);
                }
                using var content = new System.Net.Http.FormUrlEncodedContent(fields);
                using System.Net.Http.HttpResponseMessage res =
                    await HandoffHttp.PostAsync(targetUrl.TrimEnd('/') + "/admin/save/import", content);
                if (res.IsSuccessStatusCode) { ok++; }
                else { Console.WriteLine($"[handoff] ปลายทางปฏิเสธ {entityId}: HTTP {(int)res.StatusCode}"); }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[handoff] ส่ง {entityId} ไม่สำเร็จ: {e.Message}");
            }
        }
        return ok;
    }

    /// <summary>POST ฟอร์มมาตรฐานของ WebServer.cs รับแค่ x-www-form-urlencoded — ตัวช่วยอ่านฟิลด์แบบไม่ล้ม</summary>
    private static string Field(Dictionary<string, string> postData, string key)
    {
        if (postData == null)
        {
            return null;
        }
        return postData.TryGetValue(key, out string v) ? v : null;
    }

    private static WebServer.JsonResponse AdminError(string message, HttpStatusCode status = HttpStatusCode.BadRequest)
    {
        return new WebServer.JsonResponse(new JObject { ["ok"] = false, ["error"] = message }.ToString(), status);
    }

    private static WebServer.JsonResponse AdminOk(JObject extra = null)
    {
        JObject o = extra ?? new JObject();
        o["ok"] = true;
        return new WebServer.JsonResponse(o.ToString());
    }

    private static string DataDirectory()
    {
        if (!string.IsNullOrEmpty(GatheringTools.FilePath))
        {
            return Path.GetDirectoryName(GatheringTools.FilePath);
        }
        if (!string.IsNullOrEmpty(ServerConfig.ConfigPath))
        {
            return Path.GetDirectoryName(ServerConfig.ConfigPath);
        }
        return null;
    }

    /// <summary>
    /// ชี้ไปที่ไฟล์ .json ใต้โฟลเดอร์ data เท่านั้น — กัน path traversal
    /// รับได้ทั้งชื่อย่อ (gathering / client-core / config) และพาธเทียบ data/ เช่น assets/item/recipes.json
    /// </summary>
    private static bool TryResolveDataFile(string name, out string path, out string error)
    {
        path = null;
        error = null;
        string dataDir = DataDirectory();
        if (string.IsNullOrEmpty(dataDir))
        {
            error = "ยังไม่รู้โฟลเดอร์ data ของเซิร์ฟ";
            return false;
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            error = "ต้องระบุ name ของไฟล์ JSON";
            return false;
        }
        string relative = name.Trim().Replace('\\', '/').TrimStart('/');
        if (string.Equals(relative, "gathering", StringComparison.OrdinalIgnoreCase)
            || string.Equals(relative, "gathering_tools", StringComparison.OrdinalIgnoreCase))
        {
            relative = "gathering_tools.json";
        }
        else if (string.Equals(relative, "client-core", StringComparison.OrdinalIgnoreCase)
            || string.Equals(relative, "menus", StringComparison.OrdinalIgnoreCase))
        {
            relative = "mods/config/DurangoClientCore.json";
        }
        else if (string.Equals(relative, "config", StringComparison.OrdinalIgnoreCase))
        {
            relative = "config.json";
        }
        if (relative.Contains("..", StringComparison.Ordinal))
        {
            error = "path มี '..' ไม่ได้";
            return false;
        }
        if (!relative.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            error = "แก้ได้เฉพาะไฟล์ .json ใต้ data/";
            return false;
        }
        string fullData = Path.GetFullPath(dataDir);
        path = Path.GetFullPath(Path.Combine(fullData, relative.Replace('/', Path.DirectorySeparatorChar)));
        string prefix = fullData.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            error = "ไฟล์อยู่นอกโฟลเดอร์ data";
            return false;
        }
        return true;
    }

    private static bool IsGatheringToolsPath(string path)
    {
        return string.Equals(Path.GetFileName(path), "gathering_tools.json", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsServerConfigPath(string path)
    {
        if (string.IsNullOrEmpty(ServerConfig.ConfigPath) || string.IsNullOrEmpty(path))
        {
            return false;
        }
        return string.Equals(Path.GetFullPath(path), Path.GetFullPath(ServerConfig.ConfigPath), StringComparison.OrdinalIgnoreCase);
    }

    private void RegisterAdminRoutes()
    {
        // route กลุ่มงานดูแลเซิร์ฟที่เพิ่ม 4 ก.ย. 2026 อยู่คนละไฟล์ (Gateway.Admin.Ops.cs)
        RegisterAdminOpsRoutes();

        // ── หน้า HTML ─────────────────────────────────────────────────
        _webServer.GetRoute["/admin"] = ServeAdminHtml;
        _webServer.GetRoute["/admin/"] = ServeAdminHtml;

        // ── ทะเบียนเกาะ — ให้หน้า admin ทำแท็บสลับเกาะได้ ────────────
        // [4 ก.ย. 2026] เซิร์ฟเป็น island-mode แล้ว (1 เกาะ = 1 process = 1 พอร์ต)
        // หน้า admin จึงต้องรู้ว่ามีเกาะอะไรบ้างและอยู่พอร์ตไหน ไม่ใช่ให้คนจำเอง
        // ที่มา: data/islands.json (IslandRegistry) — ทุกเกาะอ่านไฟล์เดียวกันจึงรู้จักกันเอง
        _webServer.GetRoute["/admin/islands"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JArray list = new JArray();
            foreach (IslandInfo isle in IslandRegistry.All)
            {
                list.Add(new JObject
                {
                    ["id"] = isle.Id,
                    ["name"] = isle.Name,
                    ["terrain"] = isle.Terrain,
                    ["host"] = isle.Host,
                    ["gateway_port"] = isle.GatewayPort,
                    ["game_port"] = isle.GamePort,
                    ["min_level"] = isle.MinLevel,
                    ["max_level"] = isle.MaxLevel,
                    ["required_level"] = isle.RequiredLevel,
                    ["is_current"] = IslandRegistry.Current != null
                        && string.Equals(IslandRegistry.Current.Id, isle.Id, StringComparison.OrdinalIgnoreCase)
                });
            }
            JObject o = new JObject
            {
                // โหมดเกาะเดียวแบบเดิมก็ยังตอบ 200 ได้ แค่ current เป็น null และ list อาจว่าง
                ["mode"] = IslandRegistry.Current != null ? "island" : "single",
                ["current"] = IslandRegistry.Current?.Id,
                ["islands"] = list
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── สถานะเซิร์ฟภาพรวม ────────────────────────────────────────
        _webServer.GetRoute["/admin/status"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JObject o = new JObject
            {
                ["tps"] = Math.Round(ServerStats.Tps, 1),
                ["online_players"] = ServerStats.OnlinePlayers,
                ["alive_animals"] = ServerStats.AliveAnimals,
                ["corpse_animals"] = ServerStats.CorpseAnimals,
                ["ram_mb"] = ServerStats.RamMb,
                ["uptime_sec"] = Math.Round(ServerStats.UptimeSeconds, 1),
                ["started_at_utc"] = ServerStats.StartedAtUtc.ToString("o"),
                ["stats_age_sec"] = Math.Round((DateTime.UtcNow - ServerStats.LastUpdatedUtc).TotalSeconds, 1),
                ["cheats_enabled"] = GameServer.CheatsEnabled,
                ["admin_count"] = GameServer.AdminCount,
                ["server_name"] = _world.ServerName,
                ["game_port"] = _gameServer.Port,
                ["gateway_prefix"] = BindPrefix,
                // [4 ก.ย. 2026] เกาะที่ process นี้เป็นอยู่ — หน้า admin ใช้ไฮไลต์แท็บที่ถูกต้อง
                ["island"] = IslandRegistry.Current?.Id,
                ["island_name"] = IslandRegistry.Current?.Name
            };
            // [เพิ่มเอง] 31 ส.ค. 2026 — สรุป message ที่ตัวเกมส่งมาแต่เรายังไม่มี handler
            // ใช้ตรวจว่า "เซิร์ฟทำงานครบตามที่เกมต้องการหรือยัง" จากของจริง ไม่ใช่เดาจากโค้ด
            // เรียงจากที่เจอบ่อยสุด = ฟีเจอร์ที่ผู้เล่นพยายามใช้บ่อยสุดแล้วเงียบ
            JArray unhandled = new JArray();
            lock (Connection.UnhandledCounts)
            {
                foreach (KeyValuePair<uint, int> kv in Connection.UnhandledCounts.OrderByDescending(x => x.Value))
                {
                    unhandled.Add(new JObject { ["type"] = kv.Key, ["count"] = kv.Value });
                }
            }
            o["unhandled_messages"] = unhandled;

            // [4 ก.ย. 2026] สรุปว่าคนออนไลน์ใช้ client เวอร์ชันไหนบ้าง (ควรมีค่าเดียว = ทุกคนรุ่นเดียวกัน)
            // + digest ของชุดข้อมูลที่เซิร์ฟใช้ (client เทียบกับ /assets/manifest ได้)
            JObject versions = new JObject();
            foreach (ServerPlayer p in _world.SnapshotPlayers())
            {
                string key = (string.IsNullOrEmpty(p.ClientVersion) ? "?" : p.ClientVersion)
                             + " / " + (string.IsNullOrEmpty(p.Platform) ? "?" : p.Platform);
                versions[key] = (int)(versions[key] ?? 0) + 1;
            }
            o["client_versions"] = versions;
            o["data_manifest_digest"] = GameData.ManifestDigest ?? "";
            o["recipes_sha256"] = RecipeJsonLoader.LoadedSha256 ?? "";
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── mod ที่โหลดอยู่ ("รู้มอดโหลดจริงไหม") — ดู ServerCore/Modding/PluginManager.cs ──
        _webServer.GetRoute["/admin/mods"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            PluginManager mgr = PluginManager.Instance;
            JArray arr = new JArray();
            if (mgr != null)
            {
                foreach (PluginManager.LoadedModInfo m in mgr.Mods)
                {
                    JArray commands = new JArray();
                    foreach (string c in m.Commands) commands.Add(c);
                    arr.Add(new JObject
                    {
                        ["name"] = m.Name,
                        ["version"] = m.Version,
                        ["source_file"] = m.SourceFile,
                        ["loaded"] = m.Loaded,
                        ["state"] = m.State,
                        ["has_manifest"] = m.HasManifest,
                        ["package_directory"] = m.PackageDirectory,
                        ["error"] = m.Error,
                        ["commands"] = commands,
                        ["has_join_hook"] = m.HasPlayerJoinedHook,
                        ["has_leave_hook"] = m.HasPlayerLeftHook,
                        ["has_died_hook"] = m.HasPlayerDiedHook,
                        ["has_tick_hook"] = m.HasTickHook,
                        ["has_event_bus"] = m.HasEventBus,
                        ["id"] = m.Id,
                        ["api_version"] = m.ApiVersion,
                        ["dependencies"] = new JArray(m.Dependencies),
                        ["events"] = new JArray(m.Events),
                        ["event_errors"] = m.EventErrors,
                        ["event_milliseconds"] = m.EventMilliseconds,
                        ["event_calls"] = m.EventCalls,
                        ["command_calls"] = m.CommandCalls,
                        ["command_errors"] = m.CommandErrors,
                        ["rate_limited_calls"] = m.RateLimitedCalls,
                        ["assembly_sha256"] = m.AssemblySha256,
                        ["content_sha256"] = m.ContentSha256,
                        ["required"] = m.Required,
                        ["has_method_overrides"] = m.HasMethodOverrides,
                        ["method_overrides"] = new JArray(m.MethodOverrides),
                        ["method_override_errors"] = m.MethodOverrideErrors,
                        ["method_override_calls"] = m.MethodOverrideCalls,
                        ["method_override_milliseconds"] = m.MethodOverrideMilliseconds
                    });
                }
            }
            JObject o = new JObject
            {
                ["mods_dir"] = mgr?.ModsDir,
                ["mods_dir_exists"] = mgr?.ModsDirExists ?? false,
                ["items"] = arr
                , ["catalog_hash"] = _gameServer.ModCatalogHash
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── ผู้เล่นออนไลน์ ────────────────────────────────────────────
        _webServer.GetRoute["/admin/players"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            ServerPlayer[] players = _world.SnapshotPlayers();
            JArray arr = new JArray();
            foreach (ServerPlayer p in players)
            {
                WorldPosition pos = p.CurrentPosition;
                arr.Add(new JObject
                {
                    ["entity_id"] = p.EntityId,
                    ["name"] = p.Name,
                    ["level"] = p.Level,
                    ["tile_x"] = (int)(pos.x / 200f),
                    ["tile_y"] = (int)(pos.y / 200f),
                    ["hp"] = Math.Round(p.CurrentLife, 0),
                    ["hp_max"] = Math.Round(p.ComputedLifeMax, 0),
                    ["dead"] = p.Dead,
                    // [4 ก.ย. 2026] fleet view: ตรวจว่าทุกคนใช้ client เวอร์ชัน/แพลตฟอร์มเดียวกันไหม
                    ["client_version"] = string.IsNullOrEmpty(p.ClientVersion) ? "?" : p.ClientVersion,
                    ["platform"] = string.IsNullOrEmpty(p.Platform) ? "?" : p.Platform,
                    ["core_mod"] = p.HasClientCoreMod
                });
            }
            return new WebServer.JsonResponse(arr.ToString());
        };

        // ── บรอดแคสต์ข้อความไปทุกคนที่ออนไลน์อยู่ (โผล่เป็น popup ในเกม เหมือนข้อความจาก mod/cheat) ──
        _webServer.PostRoute["/admin/reload-gather"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string loaded = GatheringTools.ReloadNow();
            int cleared = _world.ForgetNaturalGeneratorCache();
            Console.WriteLine("[admin] reload gather: {0} · ล้าง cache {1} จุด", loaded, cleared);
            return AdminOk(new JObject { ["message"] = loaded, ["cleared"] = cleared });
        };

        _webServer.PostRoute["/admin/broadcast"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string text = Field(postData, "text");
            if (string.IsNullOrWhiteSpace(text))
            {
                return AdminError("ไม่มีข้อความมาด้วย (ฟิลด์ 'text')");
            }
            // [3 ก.ย. 2026] กำหนดเวลา/ขนาด/สีได้ (client อ่านผ่าน ##bc| — ดู GameManager.ShowAdminBroadcast)
            //   duration = วินาที (1-120) · size = ตัวคูณขนาด (0.5-4) · color = hex เช่น FF3333
            string durS = Field(postData, "duration");
            string sizeS = Field(postData, "size");
            string color = (Field(postData, "color") ?? "").Trim().TrimStart('#');
            string payload;
            bool hasStyle = !string.IsNullOrWhiteSpace(durS) || !string.IsNullOrWhiteSpace(sizeS) || !string.IsNullOrWhiteSpace(color);
            if (hasStyle)
            {
                var sb = new System.Text.StringBuilder("##bc|");
                if (float.TryParse(durS, out float d) && d > 0f) sb.Append("d=").Append(System.Math.Clamp(d, 1f, 120f)).Append('|');
                if (float.TryParse(sizeS, out float z) && z > 0f) sb.Append("z=").Append(System.Math.Clamp(z, 0.5f, 4f)).Append('|');
                if (color.Length is 6 or 8 && System.Text.RegularExpressions.Regex.IsMatch(color, "^[0-9a-fA-F]+$")) sb.Append("c=").Append(color).Append('|');
                sb.Append(text);
                payload = sb.ToString();
            }
            else
            {
                payload = text;
            }
            ServerPlayer[] players = _world.SnapshotPlayers();
            // [3 ก.ย. 2026] client ที่ไม่รู้จัก ##bc| (มือถือของแท้ / PC รุ่นเก่า) ได้รับเป็นข้อความธรรมดา
            _world.BroadcastInfo(payload);
            Console.WriteLine($"[admin] บรอดแคสต์ผ่าน admin panel: \"{text}\" (ถึง {players.Length} คน · style={hasStyle})");
            return AdminOk(new JObject { ["sent_to"] = players.Length });
        };

        _webServer.PostRoute["/admin/players/kick"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string entityId = Field(postData, "entity_id");
            ServerPlayer p = _world.FindPlayerByNameOrId(entityId);
            if (p == null)
            {
                return AdminError("ไม่พบผู้เล่นนี้ในเซิร์ฟ (ต้องออนไลน์อยู่)");
            }
            string reason = Field(postData, "reason");
            Console.WriteLine($"[admin] เตะ {p.Name} ({p.EntityId}) ผ่าน admin panel");
            p.Kick(string.IsNullOrWhiteSpace(reason) ? "ถูกเตะออกโดยผู้ดูแลระบบ" : reason);
            return AdminOk();
        };

        _webServer.PostRoute["/admin/players/teleport"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string entityId = Field(postData, "entity_id");
            ServerPlayer p = _world.FindPlayerByNameOrId(entityId);
            if (p == null)
            {
                return AdminError("ไม่พบผู้เล่นนี้ในเซิร์ฟ (ต้องออนไลน์อยู่)");
            }
            if (!int.TryParse(Field(postData, "x"), out int tx) || !int.TryParse(Field(postData, "y"), out int ty))
            {
                return AdminError("ต้องระบุ x, y เป็นพิกัด tile (จำนวนเต็ม)");
            }
            Console.WriteLine($"[admin] วาร์ป {p.Name} ({p.EntityId}) ไป tile {tx},{ty} ผ่าน admin panel");
            p.ControlTeleport(tx, ty);
            return AdminOk();
        };

        // ── POI (จุดสนใจ: ท่าเรือ/หลุมวาร์ป ฯลฯ) — ใช้ลอจิกเดียวกับ `cheat poi` ในเกม ──
        // (ดู ServerPlayer.CheatPOI.cs — ListPOI/MovePOITo/RemovePOI/AddPOI/TryFindPOI เป็น static
        //  รับ ServerWorld ตรง ๆ อยู่แล้ว เพื่อให้เรียกจากที่นี่ได้โดยไม่ต้องมีผู้เล่นถืออยู่)
        _webServer.GetRoute["/admin/poi"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            bool onlyProblems = request.QueryString["problems"] == "1";
            List<ServerPlayer.PoiEntry> entries = ServerPlayer.ListPOI(_world, onlyProblems);
            JArray arr = new JArray();
            foreach (ServerPlayer.PoiEntry e in entries)
            {
                arr.Add(new JObject
                {
                    ["id"] = e.EntityId,
                    ["short_id"] = e.ShortId,
                    ["blueprint"] = e.Blueprint,
                    ["tile_x"] = e.TileX,
                    ["tile_y"] = e.TileY,
                    ["dist_from_entry"] = e.DistFromEntry,
                    ["problem"] = e.Problem
                });
            }
            JObject o = new JObject
            {
                ["items"] = arr,
                ["blueprints"] = new JArray(ServerPlayer.POIBlueprints.Keys)
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        _webServer.PostRoute["/admin/poi/move"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string id = Field(postData, "id");
            if (!int.TryParse(Field(postData, "x"), out int tx) || !int.TryParse(Field(postData, "y"), out int ty))
            {
                return AdminError("ต้องระบุ x, y เป็นพิกัด tile (จำนวนเต็ม)");
            }
            if (!ServerPlayer.TryFindPOI(_world, id, out AppearArtifact art, out string err))
            {
                return AdminError(err);
            }
            string msg = ServerPlayer.MovePOITo(_world, art, tx, ty);
            Console.WriteLine($"[admin] {msg}");
            return AdminOk(new JObject { ["message"] = msg });
        };

        _webServer.PostRoute["/admin/poi/remove"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string id = Field(postData, "id");
            string msg = ServerPlayer.RemovePOI(_world, id);
            Console.WriteLine($"[admin] {msg}");
            return AdminOk(new JObject { ["message"] = msg });
        };

        _webServer.PostRoute["/admin/poi/add"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string blueprint = Field(postData, "blueprint");
            if (!int.TryParse(Field(postData, "x"), out int tx) || !int.TryParse(Field(postData, "y"), out int ty))
            {
                return AdminError("ต้องระบุ x, y เป็นพิกัด tile (จำนวนเต็ม)");
            }
            string msg = ServerPlayer.AddPOI(_world, blueprint, tx, ty);
            Console.WriteLine($"[admin] {msg}");
            return AdminOk(new JObject { ["message"] = msg });
        };

        // ── config.json สด — hot-reload อยู่แล้ว ตรวจ+เขียนที่นี่มีผลทันที ไม่ต้องรอ 5 วิ ──
        _webServer.GetRoute["/admin/config"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JObject o = new JObject
            {
                ["path"] = ServerConfig.ConfigPath,
                ["json"] = ServerConfig.CurrentJson
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        _webServer.PostRoute["/admin/config"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string json = Field(postData, "json");
            if (string.IsNullOrWhiteSpace(json))
            {
                return AdminError("ไม่มีเนื้อหา config มาด้วย (ฟิลด์ 'json')");
            }
            if (!ServerConfig.TryApplyJson(json, out string error))
            {
                return AdminError(error);
            }
            return AdminOk();
        };

        // รายการไฟล์ .json ใต้ data/ ให้ DevKit เลือกแก้จากบ้าน
        _webServer.GetRoute["/admin/data-files"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string dataDir = DataDirectory();
            if (string.IsNullOrEmpty(dataDir) || !Directory.Exists(dataDir))
            {
                return AdminError("ยังไม่รู้โฟลเดอร์ data ของเซิร์ฟ");
            }
            string fullData = Path.GetFullPath(dataDir);
            JArray arr = new JArray();
            foreach (string file in Directory.EnumerateFiles(fullData, "*.json", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(fullData, file).Replace('\\', '/');
                if (rel.StartsWith("reports/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var fi = new FileInfo(file);
                bool hot = string.Equals(rel, "gathering_tools.json", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(rel, "config.json", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(rel, "mods/config/DurangoClientCore.json", StringComparison.OrdinalIgnoreCase);
                arr.Add(new JObject
                {
                    ["name"] = rel,
                    ["bytes"] = fi.Length,
                    ["hot"] = hot
                });
            }
            return new WebServer.JsonResponse(new JObject { ["ok"] = true, ["files"] = arr }.ToString());
        };

        // JSON ใต้ data/ ที่ DevKit แก้จากบ้านได้ — จำกัดให้อยู่ในโฟลเดอร์ data และนามสกุล .json เท่านั้น
        _webServer.GetRoute["/admin/data-file"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string name = request.QueryString["name"] ?? Field(postData, "name");
            if (!TryResolveDataFile(name, out string path, out string err))
            {
                return AdminError(err);
            }
            if (!File.Exists(path))
            {
                return AdminError("ยังไม่มีไฟล์ " + path);
            }
            var fi = new FileInfo(path);
            if (fi.Length > 4 * 1024 * 1024)
            {
                return AdminError("ไฟล์ใหญ่เกิน 4 MB — แก้ผ่าน DevKit ไม่ได้ ต้องแก้บนเครื่องเซิร์ฟ");
            }
            return new WebServer.JsonResponse(new JObject
            {
                ["ok"] = true,
                ["name"] = Path.GetRelativePath(Path.GetFullPath(DataDirectory()), path).Replace('\\', '/'),
                ["path"] = path,
                ["bytes"] = fi.Length,
                ["hot"] = IsGatheringToolsPath(path) || IsServerConfigPath(path),
                ["json"] = File.ReadAllText(path)
            }.ToString());
        };

        _webServer.PostRoute["/admin/data-file"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string name = Field(postData, "name") ?? request.QueryString["name"];
            if (!TryResolveDataFile(name, out string path, out string err))
            {
                return AdminError(err);
            }
            string json = Field(postData, "json");
            if (string.IsNullOrWhiteSpace(json))
            {
                return AdminError("ไม่มีเนื้อหา json");
            }
            if (json.Length > 4 * 1024 * 1024)
            {
                return AdminError("เนื้อหาใหญ่เกิน 4 MB");
            }
            try
            {
                JToken.Parse(json);
            }
            catch (Exception e)
            {
                return AdminError("JSON ผิดรูปแบบ: " + e.Message);
            }
            if (IsServerConfigPath(path))
            {
                if (!ServerConfig.TryApplyJson(json, out string applyErr))
                {
                    return AdminError(applyErr);
                }
                Console.WriteLine("[admin] เขียน config.json แล้วใช้ทันที");
                return AdminOk(new JObject { ["path"] = path, ["message"] = "บันทึกแล้ว เซิร์ฟใช้ config ใหม่ทันที (ผู้เล่นไม่หลุด)" });
            }
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json);
            string extra = "";
            if (IsGatheringToolsPath(path))
            {
                extra = " · " + GatheringTools.ReloadNow();
                extra += " · ล้าง cache " + _world.ForgetNaturalGeneratorCache() + " จุด";
            }
            Console.WriteLine("[admin] เขียน {0}{1}", path, extra);
            string note = extra.Length > 0
                ? "บันทึกแล้ว" + extra
                : "บันทึกแล้ว — ไฟล์นี้เซิร์ฟอ่านตอนเปิด ถ้าไม่ใช่ gathering/config ต้องรีสตาร์ทถึงใช้ค่าใหม่";
            return AdminOk(new JObject { ["path"] = path, ["message"] = note });
        };

        // ── log สด (ดู LiveLog.cs) — poll ?after=<cursor ที่ได้จากรอบก่อน> ──
        _webServer.GetRoute["/admin/log"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            long after = 0;
            long.TryParse(request.QueryString["after"], out after);
            (string[] Lines, long NextCursor) result = LiveLog.GetSince(after);
            JObject o = new JObject
            {
                ["lines"] = new JArray(result.Lines),
                ["cursor"] = result.NextCursor
            };
            return new WebServer.JsonResponse(o.ToString());
        };

        // ── ย้ายผู้เล่นไปเซิร์ฟสำรองก่อนรีสตาร์ต (handoff A→B) ──────────────
        //
        // เจ้าของออกแบบไว้: "ก่อน deploy ให้มีข้อความแจ้งว่าเซิร์ฟจะรี แล้วนับถอยหลัง 10 วิ
        //  แล้ววาร์ปผู้เล่นไปห้อง B · ระหว่าง 10 วินาทีนี้ก็ซิงค์เซฟไปด้วย · พอเซิร์ฟกลับมาก็วาร์ปกลับ"
        //
        // กลไกที่ใช้จริง (ไม่ต้องเขียนระบบวาร์ปข้ามเซิร์ฟใหม่):
        //  1. บรอดแคสต์นับถอยหลังให้ผู้เล่นรู้ตัว (ผ่าน Info — ดู BroadcastDisplay ในมอด)
        //  2. `SaveAll(force)` เขียนเซฟลงดิสก์ให้ครบ **ตำแหน่ง x,y อยู่ในเซฟอยู่แล้ว** (PosX/PosY)
        //     ⇒ ย้ายเซฟ = ย้ายตำแหน่งไปด้วยโดยอัตโนมัติ ไม่ต้องส่งพิกัดแยก
        //  3. ยิงเซฟไปเข้าเซิร์ฟปลายทางผ่าน `/admin/save/import`
        //  4. พอ A ดับ client จะเห็นว่า A ออฟไลน์แล้วไปต่อ B ให้เอง (ดู PickServer ใน DurangoClientCore)
        //     — ผู้เล่นเจอตัวละคร/ของ/ตำแหน่งเดิมเพราะเซฟถูกซิงค์ไปแล้วในขั้นที่ 3
        //  ขากลับก็ทำแบบเดียวกันจาก B → A
        _webServer.PostRoute["/admin/handoff"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string target = Field(postData, "target");
            if (string.IsNullOrWhiteSpace(target))
            {
                return AdminError("ต้องระบุ target = ที่อยู่เซิร์ฟปลายทาง เช่น 127.0.0.1:8390");
            }
            if (!int.TryParse(Field(postData, "seconds"), out int seconds))
            {
                seconds = 10;
            }
            seconds = Math.Clamp(seconds, 0, 120);
            string targetUrl = target.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || target.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? target
                : "http://" + target;
            string targetToken = Field(postData, "target_token") ?? string.Empty;

            ServerPlayer[] online = _world.SnapshotPlayers();
            Console.WriteLine($"[handoff] เริ่มย้ายไป {targetUrl} ใน {seconds} วิ (ผู้เล่นออนไลน์ {online.Length} คน)");

            // ทำเป็น background — HTTP ต้องตอบกลับทันที ไม่งั้น admin panel ค้างรอทั้ง 10 วินาที
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    for (int left = seconds; left > 0; left--)
                    {
                        string text = left == seconds
                            ? $"⚠️ เซิร์ฟจะรีสตาร์ตใน {left} วินาที — ระบบจะย้ายทุกคนไปเซิร์ฟสำรองให้อัตโนมัติ ตำแหน่งและของยังอยู่ครบ"
                            : $"เซิร์ฟจะรีสตาร์ตใน {left} วินาที...";
                        _world.Broadcast(new Info { Text = text });
                        await System.Threading.Tasks.Task.Delay(1000);
                    }
                    _world.Broadcast(new Info { Text = "กำลังบันทึกและย้ายข้อมูลไปเซิร์ฟสำรอง..." });
                    int saved = _world.SaveAll(force: true);
                    int pushed = await PushSavesTo(targetUrl, targetToken);
                    Console.WriteLine($"[handoff] เซฟ {saved} ไฟล์ · ส่งไปปลายทางสำเร็จ {pushed} ตัวละคร");

                    // [แก้เอง 31 ส.ค. 2026] เจ้าของสั่ง: "ทำแบบตอนย้ายเกาะเลย ไม่ต้องรอเกมหลุด"
                    // ⇒ ใช้โปรโตคอลเดียวกับการเดินทางข้ามเกาะ (ดู ServerPlayer.Travel.cs):
                    //   `Info "##goto <address>"` บอกปลายทาง แล้วตามด้วย `Emigrated` ให้ client ออกจากโลก
                    //   อย่างเป็นระเบียบ — ไม่ใช่รอให้เซิร์ฟตายแล้ว client หลุดเอง (ซึ่งเห็น error กลางคัน)
                    // ฝั่งรับ `##goto` อยู่ใน mod (DurangoClientCore) เพราะ DLL ที่แจกจริงยังไม่มีโค้ดนี้
                    string gotoTarget = target;
                    ServerPlayer[] toMove = _world.SnapshotPlayers();
                    foreach (ServerPlayer p in toMove)
                    {
                        try
                        {
                            p.Send(new Info { Text = ServerPlayer.GotoPrefix + gotoTarget });
                            p.Send(new Emigrated { Type = Shared.Teleport.TeleportType.Unknown });
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"[handoff] ส่งคำสั่งย้ายให้ {p.Name} ไม่สำเร็จ: {e.Message}");
                        }
                    }
                    Console.WriteLine($"[handoff] สั่งย้าย {toMove.Length} คนไป {gotoTarget} แล้ว (แบบเดียวกับย้ายเกาะ)");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[handoff] ล้มเหลว: {e.Message}");
                    _world.Broadcast(new Info { Text = "ย้ายเซิร์ฟไม่สำเร็จ — ยกเลิกการรีสตาร์ต" });
                }
            });

            return AdminOk(new JObject
            {
                ["message"] = $"เริ่มนับถอยหลัง {seconds} วิ แล้วจะย้ายไป {targetUrl}",
                ["players_online"] = online.Length
            });
        };

        // รับเซฟที่เซิร์ฟอีกตัวส่งมา (ปลายทางของ /admin/handoff)
        _webServer.PostRoute["/admin/save/import"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string entityId = Field(postData, "entity_id");
            string playerJson = Field(postData, "player_json");
            if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(playerJson))
            {
                return AdminError("ต้องมี entity_id และ player_json");
            }
            // กันคนออนไลน์อยู่บนเซิร์ฟนี้แล้วโดนเขียนทับ — เซฟในหน่วยความจำของเขาใหม่กว่า
            if (_world.FindPlayer(entityId) != null)
            {
                Console.WriteLine($"[handoff] ข้าม {entityId}: กำลังออนไลน์อยู่บนเซิร์ฟนี้ (ไม่เขียนทับ)");
                return AdminOk(new JObject { ["skipped"] = "player_online_here" });
            }
            try
            {
                string path = SaveStore.PlayerPath(entityId);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, playerJson);
                string accountJson = Field(postData, "account_json");
                if (!string.IsNullOrWhiteSpace(accountJson))
                {
                    string accPath = Path.Combine(SaveStore.Root, "accounts", entityId + ".json");
                    Directory.CreateDirectory(Path.GetDirectoryName(accPath));
                    File.WriteAllText(accPath, accountJson);
                }
                Console.WriteLine($"[handoff] รับเซฟ {entityId} จากเซิร์ฟอีกตัวแล้ว");
                return AdminOk(new JObject { ["imported"] = entityId });
            }
            catch (Exception e)
            {
                return AdminError("เขียนเซฟไม่สำเร็จ: " + e.Message);
            }
        };

        // ── สั่ง cheat command แบบอิสระ "ในนามของ" ผู้เล่นออนไลน์ที่เลือก ──
        // (คำสั่งพวกนี้ผูกกับตัวละครเสมอ เช่น spawn/heal/tp มีผลที่ตำแหน่งของผู้เล่นคนนั้น
        //  ผลลัพธ์ข้อความจะไปโผล่ในเกมของผู้เล่นคนนั้น (Info packet) เหมือนพิมพ์เอง ไม่ได้ตอบกลับทาง HTTP ตรง ๆ)
        _webServer.PostRoute["/admin/cheat"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!GameServer.CheatsEnabled)
            {
                return AdminError("คำสั่งทดสอบถูกปิดอยู่ (เปิดเซิร์ฟด้วย --enable-cheat ถึงจะใช้ได้)");
            }
            string entityId = Field(postData, "entity_id");
            string command = Field(postData, "command");
            if (string.IsNullOrWhiteSpace(command))
            {
                return AdminError("ไม่มีคำสั่งมาด้วย (ฟิลด์ 'command')");
            }
            ServerPlayer p = _world.FindPlayerByNameOrId(entityId);
            if (p == null)
            {
                return AdminError("ไม่พบผู้เล่นนี้ในเซิร์ฟ (ต้องออนไลน์อยู่ — คำสั่งทดสอบผูกกับตัวละครเสมอ)");
            }
            Console.WriteLine($"[admin] สั่ง cheat '{command}' ในนามของ {p.Name} ({p.EntityId}) ผ่าน admin panel");
            // [แก้เอง 31 ส.ค. 2026] คืน "ผลจริง" ของคำสั่งกลับมาทาง HTTP
            // เดิมตอบแค่ "ดูผลในหน้าจอเกม" แต่ข้อความ Info ที่ cheat ตอบกลับ **ไม่มีใครแสดงในเกม**
            // ⇒ สั่งอะไรไปก็ไม่รู้ว่าสำเร็จหรือพลาดเพราะอะไร (ดู ServerPlayer.RunAdminCheatCapturing)
            string cheatReply = p.RunAdminCheatCapturing(command);
            Console.WriteLine($"[admin] ผล: {(string.IsNullOrEmpty(cheatReply) ? "(ไม่มีข้อความตอบกลับ)" : cheatReply)}");
            return AdminOk(new JObject
            {
                ["message"] = $"ส่งคำสั่งให้ {p.Name} แล้ว",
                ["result"] = string.IsNullOrEmpty(cheatReply) ? "(คำสั่งนี้ไม่มีข้อความตอบกลับ)" : cheatReply
            });
        };

        // ── กัน route ทั้งหมดข้างบนด้วย token (ยกเว้นหน้า HTML เอง) ──────
        // [เพิ่มเอง] เครื่องมือแอดมินฝั่งเกาะ/แมพ — ต้องลงทะเบียนก่อนด่านห่อรหัสข้างล่าง
        RegisterAdminTerrainRoutes();

        // ทำทีเดียวตรงนี้แทนที่จะห่อทุก route ข้างบนทีละอัน — วนดูทุก key ที่ขึ้นต้น "/admin/" ใน
        // GetRoute/PostRoute (ยกเว้น "/admin" กับ "/admin/" ที่เสิร์ฟ HTML เฉย ๆ ไม่มีข้อมูลอ่อนไหว)
        GuardAdminRoutes(_webServer.GetRoute);
        GuardAdminRoutes(_webServer.PostRoute);
        Console.WriteLine(string.IsNullOrEmpty(_adminToken)
            ? "[admin] /admin/* ถูกปิดจนกว่าจะตั้ง --admin-token"
            : "[admin] เปิด token กัน /admin/* แล้ว (--admin-token)");
    }

    private void GuardAdminRoutes(Dictionary<string, WebServer.RouteFunction> routes)
    {
        foreach (string key in new List<string>(routes.Keys))
        {
            if (key != "/admin/" && key.StartsWith("/admin/", StringComparison.Ordinal))
            {
                WebServer.RouteFunction inner = routes[key];
                routes[key] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
                {
                    string token = request.Headers["Authorization"];
                    if (token != null && token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        token = token.Substring(7).Trim();
                    }
                    if (string.IsNullOrEmpty(token))
                    {
                        token = request.QueryString["token"];
                    }
                    if (string.IsNullOrEmpty(token))
                    {
                        token = Field(postData, "token");
                    }
                    if (string.IsNullOrEmpty(_adminToken) || token != _adminToken)
                    {
                        return AdminError("unauthorized — ใช้ Authorization: Bearer <token>", HttpStatusCode.Unauthorized);
                    }
                    return inner(request, postData);
                };
            }
        }
    }

    /// <summary>เสิร์ฟ server/admin/index.html ตรง ๆ — อ่านจากดิสก์ทุกครั้ง (แก้ไฟล์แล้ว refresh เห็นผลทันที ไม่ต้อง restart)</summary>
    private WebServer.Response ServeAdminHtml(HttpListenerRequest request, Dictionary<string, string> postData)
    {
        string path = ResolveAdminHtmlPath();
        if (path == null || !File.Exists(path))
        {
            return new WebServer.TextResponse("text/plain", "admin/index.html ไม่พบ (คาดว่าอยู่ที่ server/admin/index.html)", HttpStatusCode.NotFound);
        }
        try
        {
            string html = File.ReadAllText(path);
            return new WebServer.TextResponse("text/html", html);
        }
        catch (Exception e)
        {
            return new WebServer.TextResponse("text/plain", "อ่าน admin/index.html ไม่สำเร็จ: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    private string _adminHtmlPath;

    private string ResolveAdminHtmlPath()
    {
        if (_adminHtmlPath != null)
        {
            return _adminHtmlPath;
        }
        // 1) รันด้วย `dotnet run` จากโฟลเดอร์ server/ — cwd คือ server/ ตรง ๆ
        string fromCwd = Path.Combine(Directory.GetCurrentDirectory(), "admin", "index.html");
        if (File.Exists(fromCwd))
        {
            _adminHtmlPath = fromCwd;
            return _adminHtmlPath;
        }
        // 2) รัน .exe ที่ build แล้วจาก server/bin/Debug/net9.0/... — ปีนกลับไปที่ server/
        string fromBase = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "admin", "index.html");
        if (File.Exists(fromBase))
        {
            _adminHtmlPath = Path.GetFullPath(fromBase);
            return _adminHtmlPath;
        }
        return null;
    }
}
