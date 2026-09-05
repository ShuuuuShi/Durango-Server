using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Durango.Offline;
using Durango.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

// ============================================================================
// Gateway.Admin.Ops — route ฝั่ง "งานดูแลเซิร์ฟ" ที่เพิ่มเข้ามา 4 ก.ย. 2026
//
// แยกไฟล์จาก Gateway.Admin.cs ตั้งใจ — ไฟล์นั้นยาวมากแล้วและเป็นของเดิม
// ของใหม่รอบนี้อยู่ตรงนี้ทั้งก้อน จะได้อ่าน/ย้อนกลับได้ง่าย
//
//   GET  /admin/history         กราฟย้อนหลัง tps/คนออนไลน์ (ในหน่วยความจำ 12 ชม.)
//   GET  /admin/bans            รายชื่อที่ถูกระงับ
//   POST /admin/ban             ระงับการเข้าเล่น (เตะออกให้ด้วยถ้ายังออนไลน์)
//   POST /admin/unban           ปลดระงับ
//   POST /admin/restart         ประกาศนับถอยหลังแล้วปิด process ให้ systemd เปิดใหม่
//   GET  /admin/status-effects  รายการบัพ/ดีบัพทั้งหมดจากข้อมูลเกมจริง
//   POST /admin/islands         แก้ทะเบียนเกาะ (data/islands.json)
// ============================================================================

public partial class Gateway
{
    private static Thread _restartThread;

    private void RegisterAdminOpsRoutes()
    {
        // ── กราฟย้อนหลัง ────────────────────────────────────────────────
        _webServer.GetRoute["/admin/history"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            ServerStats.Sample[] samples = ServerStats.History();
            JArray arr = new JArray();
            foreach (ServerStats.Sample s in samples)
            {
                arr.Add(new JObject
                {
                    ["t"] = Math.Round(s.At, 0),
                    ["tps"] = s.Tps,
                    ["players"] = s.Players,
                    ["ram_mb"] = s.RamMb
                });
            }
            return new WebServer.JsonResponse(new JObject
            {
                ["ok"] = true,
                // เก็บในหน่วยความจำอย่างเดียว — รีสตาร์ตแล้วเริ่มนับใหม่ (บอกหน้าเว็บไว้ด้วย)
                ["interval_sec"] = 60,
                ["since_restart"] = true,
                ["samples"] = arr
            }.ToString());
        };

        // ── ระงับการเข้าเล่น ────────────────────────────────────────────
        _webServer.GetRoute["/admin/bans"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            JArray arr = new JArray();
            foreach (BanEntry b in BanList.Active())
            {
                arr.Add(new JObject
                {
                    ["entity_id"] = b.EntityId,
                    ["name"] = b.Name,
                    ["reason"] = b.Reason,
                    ["at"] = b.At,
                    ["until"] = b.Until,
                    ["by"] = b.By
                });
            }
            return new WebServer.JsonResponse(new JObject { ["ok"] = true, ["bans"] = arr }.ToString());
        };

        _webServer.PostRoute["/admin/ban"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string who = Field(postData, "who");
            if (string.IsNullOrWhiteSpace(who))
            {
                return AdminError("ต้องระบุ who (ชื่อผู้เล่นหรือ entity id)");
            }
            string reason = Field(postData, "reason");
            double hours = 0;
            double.TryParse(Field(postData, "hours"), out hours);

            // ถ้ายังออนไลน์อยู่ ใช้ข้อมูลจริงจาก session (ได้ทั้ง id และชื่อที่ถูกต้อง) แล้วเตะออกเลย
            ServerPlayer p = _world.FindPlayerByNameOrId(who);
            string id = p != null ? p.EntityId : who;
            string name = p != null ? p.Name : who;
            BanEntry entry = BanList.Add(id, name, reason, hours, "admin panel");
            if (p != null)
            {
                p.Kick(string.IsNullOrWhiteSpace(reason) ? "ถูกระงับการเข้าเล่น" : "ถูกระงับการเข้าเล่น: " + reason);
            }
            Console.WriteLine("[admin] แบน {0} ({1}) · {2} · {3}",
                name, id, string.IsNullOrWhiteSpace(reason) ? "ไม่ระบุเหตุผล" : reason,
                hours > 0 ? hours + " ชม." : "ถาวร");
            return AdminOk(new JObject
            {
                ["message"] = (p != null ? "เตะออกและแบนแล้ว: " : "แบนแล้ว (ตอนนี้ไม่ได้ออนไลน์): ") + name,
                ["until"] = entry.Until
            });
        };

        _webServer.PostRoute["/admin/unban"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string who = Field(postData, "who");
            int n = BanList.Remove(who);
            if (n == 0)
            {
                return AdminError("ไม่พบ '" + who + "' ในรายชื่อที่ถูกระงับ");
            }
            return AdminOk(new JObject { ["message"] = "ปลดระงับ " + who + " แล้ว" });
        };

        // ── รีสตาร์ตแบบมีคำเตือน ────────────────────────────────────────
        // เดิมการรีสตาร์ตทำจาก ssh (`systemctl restart`) ซึ่งเตะทุกคนออกทันทีโดยไม่มีคำเตือน
        // (ExecStop มี announce ให้เฉพาะเกาะแรก และยิงไปพอร์ต 8190 ตายตัว = เกาะอื่นเงียบสนิท)
        // อันนี้ประกาศในเกาะ "ตัวเอง" ตามจังหวะที่กำหนด แล้วค่อยปิด process
        _webServer.PostRoute["/admin/restart"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (_restartThread != null && _restartThread.IsAlive)
            {
                return AdminError("มีการนับถอยหลังรีสตาร์ตค้างอยู่แล้ว");
            }
            int seconds = 300;
            int.TryParse(Field(postData, "seconds"), out seconds);
            seconds = Math.Clamp(seconds, 5, 3600);
            string note = Field(postData, "note");

            _restartThread = new Thread(() => RestartCountdown(seconds, note)) { IsBackground = true };
            _restartThread.Start();
            return AdminOk(new JObject { ["message"] = $"จะรีสตาร์ตในอีก {seconds} วินาที (ประกาศให้ผู้เล่นแล้ว)" });
        };

        // ── บัพ/ดีบัพทั้งหมดจากข้อมูลเกมจริง ────────────────────────────
        // ไฟล์ต้นทาง: data/assets/survival/status_effects.json (AssetRipper ถอดจากตัวเกม)
        // เอามาโชว์ให้เจ้าของเซิร์ฟรู้ว่ามีอะไรให้ใช้บ้าง + id ที่เอาไปใส่ `cheat effect <id>` ได้
        _webServer.GetRoute["/admin/status-effects"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string dataDir = DataDirectory();
            if (string.IsNullOrEmpty(dataDir))
            {
                return AdminError("ยังไม่รู้โฟลเดอร์ data ของเซิร์ฟ");
            }
            string path = Path.Combine(dataDir, "assets", "survival", "status_effects.json");
            if (!File.Exists(path))
            {
                return AdminError("ไม่พบ " + path);
            }
            try
            {
                JObject raw = JObject.Parse(File.ReadAllText(path));
                JArray outp = new JArray();
                foreach (KeyValuePair<string, JToken> kv in raw)
                {
                    JArray levels = kv.Value as JArray;
                    JObject first = levels != null && levels.Count > 0 ? levels[0] as JObject : null;
                    if (first == null) { continue; }
                    JArray effects = new JArray();
                    if (first["effects"] is JArray fx)
                    {
                        foreach (JToken f in fx)
                        {
                            effects.Add(new JObject
                            {
                                ["type"] = f["type"],
                                ["key"] = f["key"],
                                ["value"] = f["value"]
                            });
                        }
                    }
                    outp.Add(new JObject
                    {
                        ["id"] = kv.Key,
                        // ชื่อ/คำอธิบายในไฟล์เป็นเกาหลี เก็บเป็น key ของ object — เอาตัวแรกพอ
                        ["name"] = FirstKeyOf(first["name"]),
                        ["desc"] = FirstKeyOf(first["description"]),
                        ["color"] = (string)first["icon_color"] ?? "",
                        ["icon"] = (string)first["icon"] ?? "",
                        ["duration"] = (string)first["duration"] ?? "",
                        ["max_level"] = first["max_level"],
                        ["effects"] = effects
                    });
                }
                return new WebServer.JsonResponse(new JObject
                {
                    ["ok"] = true,
                    ["count"] = outp.Count,
                    ["effects"] = outp
                }.ToString());
            }
            catch (Exception e)
            {
                return AdminError("อ่าน status_effects.json ไม่สำเร็จ: " + e.Message);
            }
        };

        // ── ไฟล์ข้อมูลของเกาะ (terrains/extracted/<เกาะ>/*.yml) ─────────
        // [4 ก.ย. 2026] เจอของจริง: เกาะหิมะบน VPS **ไม่มี pois.yml เลย** ⇒ ระบบสุ่มตำแหน่ง
        // ท่าเรือให้เอง แล้วไปโผล่ห่างจุดเกิด 232 tile · เดิมต้อง scp ขึ้นเครื่องเองถึงจะแก้ได้
        //
        // /admin/data-file ที่มีอยู่แล้วรับเฉพาะ .json ใต้ data/ ⇒ ไฟล์ .yml ของเกาะเข้าไม่ได้
        // อันนี้เลยแยกออกมา จำกัดเฉพาะชื่อไฟล์ที่รู้จักและอยู่ใต้ terrains/extracted เท่านั้น
        _webServer.GetRoute["/admin/terrain/files"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string terrain = request.QueryString["terrain"];
            if (!TryTerrainDir(terrain, out string dir, out string err))
            {
                return AdminError(err);
            }
            JArray files = new JArray();
            foreach (string name in EditableTerrainFiles)
            {
                string full = Path.Combine(dir, name);
                bool exists = File.Exists(full);
                JObject o = new JObject
                {
                    ["name"] = name,
                    ["exists"] = exists,
                    ["bytes"] = exists ? new FileInfo(full).Length : 0
                };
                if (exists)
                {
                    try
                    {
                        var fi = new FileInfo(full);
                        o["content"] = fi.Length <= 256 * 1024 ? File.ReadAllText(full) : "(ไฟล์ใหญ่เกิน 256 KB — แก้บนเครื่องเซิร์ฟ)";
                    }
                    catch (Exception e) { o["content"] = "(อ่านไม่ได้: " + e.Message + ")"; }
                }
                files.Add(o);
            }
            return new WebServer.JsonResponse(new JObject
            {
                ["ok"] = true,
                ["terrain"] = terrain,
                ["dir"] = dir,
                ["files"] = files
            }.ToString());
        };

        _webServer.PostRoute["/admin/terrain/file"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string terrain = Field(postData, "terrain");
            string name = Field(postData, "name");
            string content = Field(postData, "content");
            if (!TryTerrainDir(terrain, out string dir, out string err))
            {
                return AdminError(err);
            }
            if (string.IsNullOrWhiteSpace(name) || Array.IndexOf(EditableTerrainFiles, name) < 0)
            {
                return AdminError("แก้ได้เฉพาะไฟล์: " + string.Join(", ", EditableTerrainFiles));
            }
            if (content == null)
            {
                return AdminError("ไม่มีเนื้อหาไฟล์ (ฟิลด์ 'content')");
            }
            if (content.Length > 1024 * 1024)
            {
                return AdminError("ไฟล์ใหญ่เกิน 1 MB");
            }
            string path = Path.Combine(dir, name);
            try
            {
                if (File.Exists(path))
                {
                    File.Copy(path, path + ".bak-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), overwrite: true);
                }
                // เขียนแบบ UTF-8 ไม่มี BOM · ปรับ CRLF เป็น LF · ปิดท้ายด้วยขึ้นบรรทัดใหม่เสมอ
                // (ตัวอ่าน yaml ของเราคาดแบบนี้ และ BOM ทำให้ key แรกอ่านไม่ออก)
                string body = content.Replace("\r\n", "\n").TrimEnd() + "\n";
                File.WriteAllText(path, body, new System.Text.UTF8Encoding(false));
            }
            catch (Exception e)
            {
                return AdminError("เขียนไฟล์ไม่สำเร็จ: " + e.Message);
            }
            Console.WriteLine("[terrain] เขียน {0} จากหน้า admin ({1} ตัวอักษร)", path, content.Length);
            return AdminOk(new JObject
            {
                ["message"] = $"บันทึก {name} ของเกาะ {terrain} แล้ว — **มีผลตอนเปิดเซิร์ฟใหม่** (สำรองของเดิมไว้ข้าง ๆ)",
                ["path"] = path
            });
        };

        // ── แก้ทะเบียนเกาะ ──────────────────────────────────────────────
        // มีผลจริงตอนเปิดเซิร์ฟใหม่ (แต่ละเกาะอ่านทะเบียนตอนบูต) — บอกไว้ในข้อความตอบกลับ
        _webServer.PostRoute["/admin/islands"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string json = Field(postData, "json");
            if (string.IsNullOrWhiteSpace(json))
            {
                return AdminError("ไม่มีเนื้อหา (ฟิลด์ 'json')");
            }
            string dataDir = DataDirectory();
            if (string.IsNullOrEmpty(dataDir))
            {
                return AdminError("ยังไม่รู้โฟลเดอร์ data ของเซิร์ฟ");
            }
            JObject parsed;
            try
            {
                parsed = JObject.Parse(json);
            }
            catch (Exception e)
            {
                return AdminError("JSON ไม่ถูกต้อง: " + e.Message);
            }
            if (!(parsed["Islands"] is JArray islands) || islands.Count == 0)
            {
                return AdminError("ต้องมี \"Islands\" เป็น array และมีอย่างน้อย 1 เกาะ");
            }
            foreach (JToken t in islands)
            {
                if (string.IsNullOrWhiteSpace((string)t["Id"]))
                {
                    return AdminError("ทุกเกาะต้องมี Id");
                }
                if ((int?)t["GatewayPort"] is not int gp || gp <= 0 || gp > 65535)
                {
                    return AdminError("เกาะ '" + (string)t["Id"] + "' ต้องมี GatewayPort ที่ถูกต้อง");
                }
            }
            string path = Path.Combine(dataDir, "islands.json");
            try
            {
                if (File.Exists(path))
                {
                    File.Copy(path, path + ".bak-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"), overwrite: true);
                }
                File.WriteAllText(path, parsed.ToString(Formatting.Indented));
            }
            catch (Exception e)
            {
                return AdminError("เขียน islands.json ไม่สำเร็จ: " + e.Message);
            }
            Console.WriteLine("[island] ทะเบียนเกาะถูกแก้จากหน้า admin — {0} เกาะ", islands.Count);
            return AdminOk(new JObject
            {
                ["message"] = $"บันทึกทะเบียน {islands.Count} เกาะแล้ว — **มีผลตอนเปิดเซิร์ฟใหม่** (สำรองของเดิมไว้ข้าง ๆ แล้ว)"
            });
        };
    }

    /// <summary>ไฟล์ของเกาะที่ยอมให้แก้จากหน้าเว็บ — ไฟล์ .dm/.biomes เป็น binary ต้องอัปด้วยมือ</summary>
    private static readonly string[] EditableTerrainFiles = { "pois.yml", "herds.yml", "config.yml", "info.yml" };

    /// <summary>หาโฟลเดอร์ของเกาะใต้ data/terrains/extracted — กัน path ทะลุออกนอกโฟลเดอร์</summary>
    private static bool TryTerrainDir(string terrain, out string dir, out string error)
    {
        dir = null;
        error = null;
        if (string.IsNullOrWhiteSpace(terrain))
        {
            error = "ต้องระบุ terrain (ชื่อโฟลเดอร์เกาะ เช่น sn20snow)";
            return false;
        }
        if (terrain.Contains("..", StringComparison.Ordinal) || terrain.Contains('/') || terrain.Contains('\\'))
        {
            error = "ชื่อเกาะมี path แปลก ๆ ไม่ได้";
            return false;
        }
        string root = Path.Combine(DataDirectory() ?? "data", "terrains", "extracted");
        string full = Path.GetFullPath(Path.Combine(root, terrain));
        string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            error = "โฟลเดอร์อยู่นอก terrains/extracted";
            return false;
        }
        if (!Directory.Exists(full))
        {
            error = "ไม่พบโฟลเดอร์เกาะ " + terrain + " (" + full + ")";
            return false;
        }
        dir = full;
        return true;
    }

    /// <summary>ชื่อ/คำอธิบายในไฟล์ของเกมเก็บเป็น { "ข้อความ": null } — เอา key แรกมาใช้</summary>
    private static string FirstKeyOf(JToken token)
    {
        if (token is JObject o)
        {
            foreach (KeyValuePair<string, JToken> kv in o) { return kv.Key; }
        }
        return (string)token ?? "";
    }

    /// <summary>ประกาศเตือนเป็นระยะแล้วปิด process — systemd (Restart=always) จะเปิดให้ใหม่เอง</summary>
    private void RestartCountdown(int seconds, string note)
    {
        // เตือนที่ 300/180/60/30/10 วินาที เฉพาะจุดที่ยังมาไม่ถึงเวลาปิด
        int[] marks = { 300, 180, 60, 30, 10 };
        string suffix = string.IsNullOrWhiteSpace(note) ? "" : " — " + note;
        int remaining = seconds;
        try
        {
            foreach (int m in marks)
            {
                if (m > remaining) { continue; }
                int sleep = remaining - m;
                if (sleep > 0) { Thread.Sleep(sleep * 1000); }
                remaining = m;
                string when = m >= 60 ? (m / 60) + " นาที" : m + " วินาที";
                _world.BroadcastInfo($"⚠ เซิร์ฟจะรีสตาร์ตในอีก {when}{suffix} — ของในตัวไม่หาย ออกจากเกมก่อนได้เลย");
                Console.WriteLine("[restart] เหลืออีก {0} วินาที", m);
            }
            if (remaining > 0) { Thread.Sleep(remaining * 1000); }
            _world.BroadcastInfo("⚠ กำลังรีสตาร์ตเดี๋ยวนี้ — เข้าใหม่ได้ในอีกสักครู่");
            Thread.Sleep(1500);
        }
        catch (Exception e)
        {
            Console.WriteLine("[restart] ประกาศไม่สำเร็จ ({0}) — ปิดเซิร์ฟต่อ", e.Message);
        }
        Console.WriteLine("[restart] ปิด process ตามคำสั่งจากหน้า admin");
        Environment.Exit(0);
    }
}
