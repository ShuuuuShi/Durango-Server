using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Durango.Offline;
using Newtonsoft.Json.Linq;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// Gateway.AdminTerrain — เครื่องมือแอดมินฝั่งเกาะ/แมพ
//
// ทำไมต้องมี: ข้อมูลที่ตัดสินว่า "เกาะนี้ปกติไหม" กระจายอยู่ในไฟล์ terrain หลายตัว
// (whole.biomes · oceans.dm · cliffs.dm · fertilities · pois.yml · herds.yml)
// เดิมต้องเปิดเกมเดินดูเองถึงจะรู้ว่าอะไรผิดที่ ซึ่งช้ามากและมองไม่เห็นภาพรวม
//
// เส้นทางที่เพิ่ม (ทั้งหมดถูกห่อด้วยด่านรหัสแอดมินเหมือน /admin/* อื่น ๆ):
//   GET  /admin/terrain/health    ตรวจสุขภาพเกาะที่เปิดอยู่ (สัตว์ในหิน/POI ผิดที่/สัดส่วนไบโอม)
//   GET  /admin/terrain/layer     ข้อมูลเลเยอร์แบบย่อสำหรับวาดบน canvas
//   GET  /admin/terrain/points    จุดต่าง ๆ ที่จะซ้อนบนแผนที่ (POI · จุดเกิดสัตว์ · สัตว์จริง)
//   GET  /admin/terrain/islands   ตรวจเกาะทุกใบในเครื่องด้วยกฎเดียวกับที่เซิร์ฟใช้โหลด
//   GET  /admin/macros            มาโครโกงที่ตัวเกมนิยามมาเอง (cheat_macro_definition.json)
//   POST /admin/macros/run        สั่งมาโครให้ผู้เล่นคนหนึ่ง
//   POST /admin/terrain/natural   เพิ่ม/ลบต้นไม้-หิน ณ tile หนึ่ง (มีผลทันที + ลงเซฟ)
// ============================================================================

public partial class Gateway
{
    /// <summary>สัดส่วนไบโอมของเกาะจริงทั้ง 13 ใบ (ต่ำสุด-สูงสุด) ใช้เทียบว่าเกาะที่โหลดอยู่ผิดปกติไหม</summary>
    private static readonly (string Name, double Lo, double Hi)[] BiomeShareRef =
    {
        ("ทะเล", 38.9, 92.6),
        ("พื้นดิน", 4.5, 40.7),
        ("หาด", 1.7, 15.9),
    };

    private void RegisterAdminTerrainRoutes()
    {
        _webServer.GetRoute["/admin/terrain/health"] = (HttpListenerRequest request, Dictionary<string, string> postData)
            => new WebServer.JsonResponse(BuildTerrainHealth().ToString());

        _webServer.GetRoute["/admin/terrain/layer"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string name = request.QueryString["name"] ?? "biome";
            int size = ParseInt(request.QueryString["size"], 256);
            return new WebServer.JsonResponse(BuildTerrainLayer(name, size).ToString());
        };

        _webServer.GetRoute["/admin/terrain/points"] = (HttpListenerRequest request, Dictionary<string, string> postData)
            => new WebServer.JsonResponse(BuildTerrainPoints().ToString());

        _webServer.GetRoute["/admin/terrain/islands"] = (HttpListenerRequest request, Dictionary<string, string> postData)
            => new WebServer.JsonResponse(BuildIslandReport().ToString());

        _webServer.GetRoute["/admin/macros"] = (HttpListenerRequest request, Dictionary<string, string> postData)
            => new WebServer.JsonResponse(BuildMacroList().ToString());

        _webServer.PostRoute["/admin/macros/run"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string name = Value(postData, "name");
            string target = Value(postData, "player");
            if (string.IsNullOrWhiteSpace(name))
            {
                return Fail("ต้องระบุชื่อมาโคร");
            }
            string[] commands = CheatMacros.Commands(name);
            if (commands == null)
            {
                return Fail("ไม่รู้จักมาโคร " + name);
            }
            ServerPlayer player = FindPlayerFor(target);
            if (player == null)
            {
                return Fail("ไม่เจอผู้เล่นที่จะสั่ง (ต้องมีคนออนไลน์)");
            }
            var results = new JArray();
            foreach (string line in commands)
            {
                string reply = "ส่งแล้ว";
                try
                {
                    // ผลลัพธ์เป็นข้อความจะถูกส่งกลับไปหาตัวเกมของผู้เล่นคนนั้น (Info packet)
                    player.RunAdminCheat(line);
                }
                catch (Exception e)
                {
                    reply = "ผิดพลาด: " + e.Message;
                }
                results.Add(new JObject { ["command"] = line, ["reply"] = reply });
            }
            return new WebServer.JsonResponse(new JObject
            {
                ["ok"] = true,
                ["macro"] = name,
                ["player"] = player.Name,
                ["count"] = commands.Length,
                ["results"] = results
            }.ToString());
        };

        _webServer.PostRoute["/admin/terrain/natural"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            int x = ParseInt(Value(postData, "x"), -1);
            int y = ParseInt(Value(postData, "y"), -1);
            string action = (Value(postData, "action") ?? "remove").ToLowerInvariant();
            if (x < 0 || y < 0 || x >= _world.Terrain.Width || y >= _world.Terrain.Height)
            {
                return Fail("พิกัดอยู่นอกเกาะ");
            }
            if (action == "remove")
            {
                bool removed = _world.Terrain.RemoveNatural(x, y);
                if (removed) { _world.MarkDirty(); }
                return new WebServer.JsonResponse(new JObject
                {
                    ["ok"] = removed,
                    ["message"] = removed ? $"เอาของธรรมชาติที่ ({x},{y}) ออกแล้ว" : $"ที่ ({x},{y}) ไม่มีของธรรมชาติอยู่"
                }.ToString());
            }
            return Fail("รองรับแค่ action=remove (การปลูกเพิ่มต้องแก้ไฟล์ terrain ตรง ๆ)");
        };
    }

    private static WebServer.Response Fail(string message)
    {
        // หน้าเว็บอ่านฟิลด์ `error` เวลาขึ้นข้อความผิดพลาด — ใส่ทั้งสองชื่อกันพลาด
        return new WebServer.JsonResponse(new JObject { ["ok"] = false, ["message"] = message, ["error"] = message }.ToString());
    }

    private static string Value(Dictionary<string, string> post, string key)
    {
        return post != null && post.TryGetValue(key, out string v) ? v : null;
    }

    private static int ParseInt(string raw, int fallback)
    {
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int v) ? v : fallback;
    }

    private ServerPlayer FindPlayerFor(string idOrName)
    {
        ServerPlayer[] players = _world.SnapshotPlayers();
        if (players.Length == 0)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(idOrName))
        {
            return players[0];
        }
        foreach (ServerPlayer p in players)
        {
            if (string.Equals(p.EntityId, idOrName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(p.Name, idOrName, StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }
        }
        return null;
    }

    // ───────────────────────────────────────────────────────── ตรวจสุขภาพเกาะ

    /// <summary>
    /// ตรวจว่าเกาะที่เปิดอยู่มีอะไรผิดที่บ้าง — ทุกข้อตรวจจากข้อมูลจริง ไม่ใช่เดา
    ///
    /// ข้อที่สำคัญที่สุดคือ "สัตว์อยู่ในก้อนหิน" ซึ่งเคยเป็น 22-28% ของจุดเกิดก่อนแก้
    /// </summary>
    private JObject BuildTerrainHealth()
    {
        TerrainStore t = _world.Terrain;
        int w = t.Width, h = t.Height;
        var issues = new JArray();

        // 1. สัตว์ที่ยืนอยู่ในหิน / ในน้ำ
        int inRock = 0, inWater = 0, onHerd = 0, total = 0;
        var herdTiles = new HashSet<(int, int)>();
        if (t.Herds != null)
        {
            foreach (string g in TerrainHerds.LandGroups)
            {
                foreach (Point2 p in t.Herds.Group(g)) { herdTiles.Add((p.x, p.y)); }
            }
        }
        foreach (ServerAnimal a in _world.Animals.Snapshot())
        {
            total++;
            int tx = (int)(a.Position.x / 200f);
            int ty = (int)(a.Position.y / 200f);
            if (t.IsCliff(tx, ty)) { inRock++; }
            if (t.LandDistance(tx, ty) < 0) { inWater++; }
            if (herdTiles.Contains((tx, ty))) { onHerd++; }
        }
        if (inRock > 0) { issues.Add(Issue("bad", $"สัตว์ {inRock} ตัวยืนอยู่ในก้อนหิน")); }
        if (inWater > 0) { issues.Add(Issue("bad", $"สัตว์บก {inWater} ตัวยืนอยู่ในน้ำ")); }

        // 2. POI ที่วางผิดที่
        int poiBad = 0;
        var poiRows = new JArray();
        foreach (Messages.AppearArtifact art in _world.SnapshotArtifacts())
        {
            if (!art.EntityId.StartsWith("poi", StringComparison.Ordinal)) { continue; }
            string problem = null;
            if (t.LandDistance(art.Tile.x, art.Tile.y) < 1) { problem = "อยู่ในน้ำ"; }
            else if (t.IsCliff(art.Tile.x, art.Tile.y)) { problem = "อยู่ในก้อนหิน"; }
            if (problem != null) { poiBad++; }
            poiRows.Add(new JObject
            {
                ["id"] = art.EntityId,
                ["x"] = art.Tile.x,
                ["y"] = art.Tile.y,
                ["problem"] = problem
            });
        }
        if (poiBad > 0) { issues.Add(Issue("bad", $"POI {poiBad} จุดวางผิดที่")); }

        // 3. ไฟล์ที่มากับเกาะ
        if (t.Pois == null) { issues.Add(Issue("warn", "เกาะนี้ไม่มี pois.yml — POI จะถูกสุ่มตำแหน่งเอา")); }
        if (t.Herds == null) { issues.Add(Issue("warn", "เกาะนี้ไม่มี herds.yml — จุดเกิดสัตว์จะถูกสุ่มเอา")); }
        if (t.CliffMap == null) { issues.Add(Issue("warn", "เกาะนี้ไม่มี cliffs.dm — ใช้ธง 0xC0 ใน whole.biomes แทน")); }

        // 4. สัดส่วนไบโอม
        var counts = new Dictionary<Shared.Region.Biome, int>();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Shared.Region.Biome b = t.BiomeAt(x, y);
                counts[b] = counts.TryGetValue(b, out int n) ? n + 1 : 1;
            }
        }
        double cells = Math.Max(1, w * h);
        var biomes = new JArray();
        double sea = 0, land = 0, beach = 0;
        foreach (KeyValuePair<Shared.Region.Biome, int> kv in counts.OrderByDescending(k => k.Value))
        {
            double pct = 100.0 * kv.Value / cells;
            biomes.Add(new JObject { ["biome"] = kv.Key.ToString(), ["tiles"] = kv.Value, ["pct"] = Math.Round(pct, 2) });
            switch (kv.Key)
            {
                case Shared.Region.Biome.WarmOcean:
                case Shared.Region.Biome.ColdOcean:
                    sea += pct; break;
                case Shared.Region.Biome.SandBeach:
                case Shared.Region.Biome.PebbleBeach:
                    beach += pct; break;
                case Shared.Region.Biome.River:
                case Shared.Region.Biome.Lake:
                case Shared.Region.Biome.Lava:
                    break;
                default:
                    land += pct; break;
            }
        }
        CheckShare(issues, "ทะเล", sea, 0);
        CheckShare(issues, "พื้นดิน", land, 1);
        CheckShare(issues, "หาด", beach, 2);

        // 5. จุดเข้าเกม — เกาะจริงอยู่ในน้ำตื้นหน้าหาดเสมอ (oceans.dm = -2..-5)
        Point2 entry = t.EntryPoint;
        int entryDist = t.LandDistance(entry.x, entry.y);
        if (entryDist >= 0 || entryDist < -8)
        {
            issues.Add(Issue("warn", $"จุดเข้าเกม ({entry.x},{entry.y}) มี oceans.dm = {entryDist} (เกาะจริงอยู่ที่ -2..-5)"));
        }

        int rockTiles = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (t.IsCliff(x, y)) { rockTiles++; }
            }
        }

        return new JObject
        {
            ["terrain_id"] = t.TerrainId,
            ["size"] = new JArray(w, h),
            ["animals"] = new JObject
            {
                ["total"] = total,
                ["in_rock"] = inRock,
                ["in_water"] = inWater,
                ["on_game_spawn"] = onHerd
            },
            ["rock_pct"] = Math.Round(100.0 * rockTiles / cells, 2),
            ["entry"] = new JObject { ["x"] = entry.x, ["y"] = entry.y, ["ocean_dm"] = entryDist },
            ["files"] = new JObject
            {
                ["pois_yml"] = t.Pois != null,
                ["herds_yml"] = t.Herds != null,
                ["cliffs_dm"] = t.CliffMap != null,
                ["fertilities"] = t.Fertilities.Length > 0,
                ["temperatures"] = t.Temperatures.Length > 0,
                ["humidities"] = t.Humidities.Length > 0
            },
            ["biomes"] = biomes,
            ["pois"] = poiRows,
            ["issues"] = issues,
            ["ok"] = issues.Count == 0
        };
    }

    private static void CheckShare(JArray issues, string label, double value, int index)
    {
        (string Name, double Lo, double Hi) r = BiomeShareRef[index];
        if (value < r.Lo || value > r.Hi)
        {
            issues.Add(Issue("warn",
                $"สัดส่วน{label} {value:F1}% อยู่นอกช่วงของเกาะจริง ({r.Lo:F1}-{r.Hi:F1}%)"));
        }
    }

    private static JObject Issue(string level, string text)
    {
        return new JObject { ["level"] = level, ["text"] = text };
    }

    // ─────────────────────────────────────────────────────────── เลเยอร์แผนที่

    /// <summary>
    /// ข้อมูลเลเยอร์แบบย่อ ส่งเป็นตัวเลขล้วนให้ฝั่งหน้าเว็บวาดบน canvas เอง
    ///
    /// ไม่ทำเป็น PNG ฝั่งเซิร์ฟเพราะ System.Drawing ใช้บน Linux ไม่ได้ (เซิร์ฟจริงรันบน Linux)
    /// ย่อขนาดด้วยการสุ่มตัวอย่างทุก step tile — 256x256 ส่งเต็มก็แค่ ~65 KB ในรูป JSON
    /// </summary>
    private JObject BuildTerrainLayer(string name, int size)
    {
        TerrainStore t = _world.Terrain;
        int w = t.Width, h = t.Height;
        size = Math.Max(32, Math.Min(size, Math.Max(w, h)));
        int step = Math.Max(1, (int)Math.Ceiling(Math.Max(w, h) / (double)size));
        int gw = w / step, gh = h / step;

        var data = new JArray();
        for (int gy = 0; gy < gh; gy++)
        {
            for (int gx = 0; gx < gw; gx++)
            {
                int x = gx * step, y = gy * step;
                int v = name switch
                {
                    "cliff" => t.IsCliff(x, y) ? 1 : 0,
                    "land" => t.LandDistance(x, y),
                    "elevation" => t.Elevations.Length >= w * h ? t.Elevations[x + y * w] : 0,
                    "fertility" => t.FertilityAt(x, y),
                    "temperature" => t.TemperatureAt(x, y),
                    "humidity" => t.HumidityAt(x, y),
                    "river" => t.RiverDistance(x, y),
                    "lake" => t.LakeDistance(x, y),
                    "scoop" => t.ScoopDistance(x, y),
                    _ => (int)t.BiomeAt(x, y),
                };
                data.Add(v);
            }
        }
        return new JObject
        {
            ["layer"] = name,
            ["w"] = gw,
            ["h"] = gh,
            ["step"] = step,
            ["tiles"] = w,
            ["data"] = data
        };
    }

    /// <summary>จุดที่จะซ้อนบนแผนที่ — POI · จุดเกิดที่เกมกำหนด · สัตว์ที่มีอยู่จริง · ผู้เล่น</summary>
    private JObject BuildTerrainPoints()
    {
        TerrainStore t = _world.Terrain;
        var pois = new JArray();
        foreach (Messages.AppearArtifact art in _world.SnapshotArtifacts())
        {
            if (!art.EntityId.StartsWith("poi", StringComparison.Ordinal)) { continue; }
            pois.Add(new JObject { ["id"] = art.EntityId, ["x"] = art.Tile.x, ["y"] = art.Tile.y });
        }

        var herds = new JArray();
        if (t.Herds != null)
        {
            foreach (KeyValuePair<string, List<Point2>> g in t.Herds.Groups)
            {
                foreach (Point2 p in g.Value)
                {
                    herds.Add(new JObject { ["g"] = g.Key, ["x"] = p.x, ["y"] = p.y });
                }
            }
        }

        var animals = new JArray();
        foreach (ServerAnimal a in _world.Animals.Snapshot())
        {
            int tx = (int)(a.Position.x / 200f);
            int ty = (int)(a.Position.y / 200f);
            animals.Add(new JObject
            {
                ["x"] = tx,
                ["y"] = ty,
                ["type"] = a.EntityType,
                ["rock"] = t.IsCliff(tx, ty)
            });
        }

        var players = new JArray();
        foreach (ServerPlayer p in _world.SnapshotPlayers())
        {
            players.Add(new JObject
            {
                ["name"] = p.Name,
                ["x"] = (int)(p.CurrentPosition.x / 200f),
                ["y"] = (int)(p.CurrentPosition.y / 200f)
            });
        }

        return new JObject
        {
            ["entry"] = new JObject { ["x"] = t.EntryPoint.x, ["y"] = t.EntryPoint.y },
            ["pois"] = pois,
            ["herds"] = herds,
            ["animals"] = animals,
            ["players"] = players
        };
    }

    // ─────────────────────────────────────────────────────────── ตรวจเกาะทุกใบ

    /// <summary>
    /// ไล่ตรวจเกาะทุกใบใน data/terrains/extracted ด้วยกฎเดียวกับที่ TerrainStore ใช้โหลด
    ///
    /// จับได้ทั้ง: ไฟล์ขนาดผิด · landmark หาร 16 ไม่ลง (เซิร์ฟจะทิ้งทั้งก้อน) ·
    /// garden หาร 6 ไม่ลง · biome id ที่เกมไม่รู้จัก · ไฟล์ที่หายไป
    /// </summary>
    private JObject BuildIslandReport()
    {
        string root = Path.Combine(DataDirectory() ?? "data", "terrains", "extracted");
        var rows = new JArray();
        if (!Directory.Exists(root))
        {
            return new JObject { ["root"] = root, ["islands"] = rows, ["error"] = "ไม่พบโฟลเดอร์ terrains/extracted" };
        }
        foreach (string dir in Directory.GetDirectories(root).OrderBy(d => d))
        {
            string id = Path.GetFileName(dir);
            var problems = new JArray();
            int w = 256, h = 256;
            try
            {
                string infoPath = Path.Combine(dir, "info.yml");
                if (File.Exists(infoPath))
                {
                    JObject info = JObject.Parse(File.ReadAllText(infoPath));
                    JArray tc = info["tile_count"] as JArray;
                    if (tc != null && tc.Count >= 2)
                    {
                        w = (int)tc[0];
                        h = (int)tc[1];
                    }
                }
                else
                {
                    problems.Add("ไม่มี info.yml");
                }
            }
            catch (Exception e)
            {
                problems.Add("info.yml อ่านไม่ได้: " + e.Message);
            }

            int n = w * h;
            CheckFile(dir, "whole.biomes", n, problems);
            CheckFile(dir, "oceans.dm", n, problems);
            CheckFile(dir, "whole.elevations", n, problems);
            CheckMultiple(dir, "whole.landmarks", 16, problems, "เซิร์ฟจะทิ้ง landmark ทั้งเกาะ");
            CheckMultiple(dir, "whole.garden", 6, problems, "เซิร์ฟจะไม่ส่งของธรรมชาติเลย");

            string cliffs = Path.Combine(dir, "cliffs.dm");
            bool hasCliff = File.Exists(cliffs);
            bool hasPois = File.Exists(Path.Combine(dir, "pois.yml"));
            bool hasHerds = File.Exists(Path.Combine(dir, "herds.yml"));

            rows.Add(new JObject
            {
                ["id"] = id,
                ["size"] = new JArray(w, h),
                ["cliffs_dm"] = hasCliff,
                ["pois_yml"] = hasPois,
                ["herds_yml"] = hasHerds,
                ["problems"] = problems,
                ["ok"] = problems.Count == 0
            });
        }
        return new JObject { ["root"] = root, ["islands"] = rows };
    }

    private static void CheckFile(string dir, string name, int want, JArray problems)
    {
        string path = Path.Combine(dir, name);
        if (!File.Exists(path))
        {
            problems.Add($"ไม่มี {name}");
            return;
        }
        long got = new FileInfo(path).Length;
        if (got < want)
        {
            problems.Add($"{name} {got:N0} ไบต์ (ต้องอย่างน้อย {want:N0})");
        }
    }

    private static void CheckMultiple(string dir, string name, int unit, JArray problems, string effect)
    {
        string path = Path.Combine(dir, name);
        if (!File.Exists(path))
        {
            return;                     // ไม่มีก็ได้ (บางเกาะไม่มีหน้าผาเลย)
        }
        long got = new FileInfo(path).Length;
        if (got % unit != 0)
        {
            problems.Add($"{name} {got:N0} ไบต์ หารด้วย {unit} ไม่ลงตัว — {effect}");
        }
    }

    private JObject BuildMacroList()
    {
        var arr = new JArray();
        foreach (KeyValuePair<string, string[]> kv in CheatMacros.All())
        {
            arr.Add(new JObject
            {
                ["name"] = kv.Key,
                ["count"] = kv.Value.Length,
                ["commands"] = new JArray(kv.Value)
            });
        }
        return new JObject { ["source"] = CheatMacros.SourcePath ?? "(ไม่พบไฟล์)", ["macros"] = arr };
    }
}
