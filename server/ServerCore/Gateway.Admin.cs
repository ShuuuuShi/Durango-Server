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

    private void RegisterAdminRoutes()
    {
        // ── หน้า HTML ─────────────────────────────────────────────────
        _webServer.GetRoute["/admin"] = ServeAdminHtml;
        _webServer.GetRoute["/admin/"] = ServeAdminHtml;

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
                ["gateway_prefix"] = BindPrefix
            };
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
                    ["dead"] = p.Dead
                });
            }
            return new WebServer.JsonResponse(arr.ToString());
        };

        // ── บรอดแคสต์ข้อความไปทุกคนที่ออนไลน์อยู่ (โผล่เป็น popup ในเกม เหมือนข้อความจาก mod/cheat) ──
        _webServer.PostRoute["/admin/broadcast"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string text = Field(postData, "text");
            if (string.IsNullOrWhiteSpace(text))
            {
                return AdminError("ไม่มีข้อความมาด้วย (ฟิลด์ 'text')");
            }
            ServerPlayer[] players = _world.SnapshotPlayers();
            _world.Broadcast(new Info { Text = text });
            Console.WriteLine($"[admin] บรอดแคสต์ผ่าน admin panel: \"{text}\" (ถึง {players.Length} คน)");
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
            p.RunAdminCheat(command);
            return AdminOk(new JObject { ["message"] = $"ส่งคำสั่งให้ {p.Name} แล้ว — ดูผลในหน้าจอเกมของผู้เล่นคนนั้น" });
        };

        // ── กัน route ทั้งหมดข้างบนด้วย token (ยกเว้นหน้า HTML เอง) ──────
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
