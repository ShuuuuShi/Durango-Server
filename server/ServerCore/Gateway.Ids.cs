using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Durango.Offline;
using Durango.Utils;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

/// <summary>
/// [4 ก.ย. 2026] หน้าสมัครไอดี + endpoint ของ DurangoID (ดู <see cref="PlayerIdStore"/>)
///
/// ผู้เล่นเปิด <c>/id</c> **จากเครื่องที่จะเล่น** เพราะการผูกใช้ IP ของ request เป็นตัวจับคู่
/// (เกมมือถือส่งตัวตนอะไรมาไม่ได้เลย — ดูเหตุผลเต็มใน PlayerIdStore)
///
/// ทุก endpoint ที่แตะข้อมูลของไอดี ต้องผ่าน <see cref="PlayerIdStore.Login"/> ก่อนเสมอ
/// (ไอดี+PIN) ไม่มีการเชื่อถือ IP อย่างเดียว — IP ใช้แค่บอกว่า "เครื่องไหนกำลังขอผูก"
/// </summary>
public partial class Gateway
{
    private string _idHtmlPath;

    private void RegisterPlayerIdRoutes()
    {
        _webServer.GetRoute["/id"] = ServeIdHtml;
        _webServer.GetRoute["/id/"] = ServeIdHtml;

        // สถานะของเครื่องที่เรียกมา — ใช้ให้หน้าเว็บโชว์ว่า "IP นี้ผูกไอดีอะไรไว้บ้าง"
        _webServer.GetRoute["/id/status"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            string ip = RemoteIpOf(request);
            var ids = new JArray();
            foreach (string id in PlayerIdStore.BoundIds(ip))
            {
                PlayerIdStore.Record rec = PlayerIdStore.Find(id);
                ids.Add(new JObject
                {
                    ["id"] = PlayerIdStore.Format(id),
                    ["name"] = rec?.DisplayName ?? "",
                    ["characters"] = rec?.EntityIds?.Count ?? 0
                });
            }
            return new WebServer.JsonResponse(new JObject
            {
                ["ip"] = ip,
                ["bound"] = ids,
                ["required"] = ServerConfig.Current.PlayerIds?.Required ?? false,
                ["binding_days"] = ServerConfig.Current.PlayerIds?.BindingDays ?? 30,
                // ตัวละครที่เคยเล่นจาก IP นี้แต่ยังไม่มีไอดีเป็นเจ้าของ — ให้หน้าเว็บเสนอปุ่ม "รับตัวละครเดิม"
                ["adoptable"] = AdoptableFrom(ip).Count
            }.ToString());
        };

        _webServer.PostRoute["/id/register"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!IdsEnabled(out WebServer.Response off))
            {
                return off;
            }
            string ip = RemoteIpOf(request);
            PlayerIdStore.Record rec = PlayerIdStore.Register(
                postData.Get("pin"), postData.Get("name"), ip, out string error);
            if (rec == null)
            {
                return IdError(error);
            }
            // สมัครแล้วผูกเครื่องที่สมัครให้เลย — ผู้เล่นจะได้ไม่ต้องกดสองรอบ
            PlayerIdStore.Bind(rec.Id, ip, ServerConfig.Current.PlayerIds?.BindingDays ?? 30);
            return new WebServer.JsonResponse(new JObject
            {
                ["ok"] = true,
                ["id"] = PlayerIdStore.Format(rec.Id),
                ["bound"] = true
            }.ToString());
        };

        _webServer.PostRoute["/id/login"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!IdsEnabled(out WebServer.Response off))
            {
                return off;
            }
            string ip = RemoteIpOf(request);
            PlayerIdStore.Record rec = PlayerIdStore.Login(
                postData.Get("id"), postData.Get("pin"), ip, out string error);
            return rec == null ? IdError(error) : new WebServer.JsonResponse(IdInfo(rec, ip).ToString());
        };

        _webServer.PostRoute["/id/bind"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!IdsEnabled(out WebServer.Response off))
            {
                return off;
            }
            string ip = RemoteIpOf(request);
            PlayerIdStore.Record rec = PlayerIdStore.Login(
                postData.Get("id"), postData.Get("pin"), ip, out string error);
            if (rec == null)
            {
                return IdError(error);
            }
            PlayerIdStore.Bind(rec.Id, ip, ServerConfig.Current.PlayerIds?.BindingDays ?? 30);
            return new WebServer.JsonResponse(IdInfo(rec, ip).ToString());
        };

        _webServer.PostRoute["/id/unbind"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!IdsEnabled(out WebServer.Response off))
            {
                return off;
            }
            string ip = RemoteIpOf(request);
            PlayerIdStore.Record rec = PlayerIdStore.Login(
                postData.Get("id"), postData.Get("pin"), ip, out string error);
            if (rec == null)
            {
                return IdError(error);
            }
            PlayerIdStore.Unbind(rec.Id, ip);
            return new WebServer.JsonResponse(IdInfo(rec, ip).ToString());
        };

        // "รับตัวละครเดิม" — ผู้เล่นที่เล่นอยู่ก่อนมีระบบไอดี เอาตัวละครที่เคยจองจาก IP นี้เข้าไอดีตัวเอง
        // เงื่อนไข: ตัวละครนั้นต้องยังไม่มีไอดีอื่นเป็นเจ้าของ และต้องเคยจองจาก IP เดียวกับที่กำลังขอ
        _webServer.PostRoute["/id/adopt"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!IdsEnabled(out WebServer.Response off))
            {
                return off;
            }
            string ip = RemoteIpOf(request);
            PlayerIdStore.Record rec = PlayerIdStore.Login(
                postData.Get("id"), postData.Get("pin"), ip, out string error);
            if (rec == null)
            {
                return IdError(error);
            }
            int taken = 0;
            foreach (string entityId in AdoptableFrom(ip))
            {
                PlayerIdStore.AttachEntity(rec.Id, entityId);
                taken++;
            }
            // AttachEntity เขียนลงไฟล์ — ต้องอ่านกลับ ไม่งั้นรายชื่อที่ตอบกลับยังเป็นชุดก่อนรับ (ว่างเปล่า)
            rec = PlayerIdStore.Find(rec.Id) ?? rec;
            JObject info = IdInfo(rec, ip);
            info["adopted"] = taken;
            return new WebServer.JsonResponse(info.ToString());
        };

        _webServer.PostRoute["/id/change_pin"] = (HttpListenerRequest request, Dictionary<string, string> postData) =>
        {
            if (!IdsEnabled(out WebServer.Response off))
            {
                return off;
            }
            string ip = RemoteIpOf(request);
            bool ok = PlayerIdStore.ChangePin(
                postData.Get("id"), postData.Get("pin"), postData.Get("new_pin"), ip, out string error);
            return ok
                ? new WebServer.JsonResponse(new JObject { ["ok"] = true }.ToString())
                : IdError(error);
        };

        Console.WriteLine((ServerConfig.Current.PlayerIds?.Enabled ?? true)
            ? "[id] เปิดหน้าสมัครไอดีที่ /id" + ((ServerConfig.Current.PlayerIds?.Required ?? false)
                ? " (บังคับสมัครก่อนเล่น)" : " (สมัครหรือไม่ก็ได้)")
            : "[id] ระบบไอดีปิดอยู่ (PlayerIds.Enabled=false)");
    }

    // ---------- ตัวช่วย ----------

    private static string RemoteIpOf(HttpListenerRequest request) =>
        AccountStore.NormalizeIp(request?.RemoteEndPoint?.Address?.ToString() ?? "?");

    private static bool IdsEnabled(out WebServer.Response error)
    {
        if (ServerConfig.Current.PlayerIds?.Enabled ?? true)
        {
            error = null;
            return true;
        }
        error = new WebServer.JsonResponse(
            new JObject { ["ok"] = false, ["error"] = "ระบบไอดีปิดอยู่" }.ToString(),
            HttpStatusCode.ServiceUnavailable);
        return false;
    }

    private static WebServer.Response IdError(string message) =>
        new WebServer.JsonResponse(
            new JObject { ["ok"] = false, ["error"] = message ?? "ไม่สำเร็จ" }.ToString(),
            HttpStatusCode.BadRequest);

    /// <summary>ข้อมูลไอดี + รายชื่อตัวละคร ส่งกลับให้หน้าเว็บโชว์</summary>
    private static JObject IdInfo(PlayerIdStore.Record rec, string ip)
    {
        var chars = new JArray();
        if (rec.EntityIds != null)
        {
            foreach (string entityId in rec.EntityIds)
            {
                PlayerSave save = SaveStore.Load<PlayerSave>(SaveStore.PlayerPath(entityId));
                chars.Add(new JObject
                {
                    ["entity_id"] = entityId,
                    ["name"] = save?.Name ?? "(ไม่พบเซฟ)",
                    ["level"] = save?.Level ?? 0
                });
            }
        }
        bool boundHere = PlayerIdStore.BoundIds(ip).Contains(rec.Id);
        return new JObject
        {
            ["ok"] = true,
            ["id"] = PlayerIdStore.Format(rec.Id),
            ["name"] = rec.DisplayName ?? "",
            ["characters"] = chars,
            ["bound"] = boundHere,
            ["ip"] = ip,
            ["adoptable"] = AdoptableFrom(ip).Count
        };
    }

    /// <summary>
    /// ตัวละครที่เคยจองจาก IP นี้ (ระบบเดิม) และยังไม่มีไอดีไหนเป็นเจ้าของ — เอาไปให้ "รับตัวละครเดิม"
    /// </summary>
    private static List<string> AdoptableFrom(string ip)
    {
        var result = new List<string>();
        foreach (AccountStore.Account acc in AccountStore.FindByIp(ip))
        {
            if (string.IsNullOrEmpty(acc.EntityId) || PlayerIdStore.OwnerOf(acc.EntityId) != null)
            {
                continue;
            }
            // ต้องมีเซฟจริง ไม่งั้นเป็นขยะจากการเทส
            if (SaveStore.Load<PlayerSave>(SaveStore.PlayerPath(acc.EntityId)) != null)
            {
                result.Add(acc.EntityId);
            }
        }
        return result;
    }

    private WebServer.Response ServeIdHtml(HttpListenerRequest request, Dictionary<string, string> postData)
    {
        string path = ResolveIdHtmlPath();
        if (path == null || !File.Exists(path))
        {
            return new WebServer.TextResponse("text/plain",
                "web/id.html ไม่พบ (คาดว่าอยู่ที่ server/web/id.html)", HttpStatusCode.NotFound);
        }
        try
        {
            return new WebServer.TextResponse("text/html", File.ReadAllText(path));
        }
        catch (Exception e)
        {
            return new WebServer.TextResponse("text/plain",
                "อ่าน web/id.html ไม่สำเร็จ: " + e.Message, HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>หา server/web/id.html ทั้งตอนรันด้วย dotnet run และตอนรัน .exe ที่ build แล้ว (เหมือน admin/index.html)</summary>
    private string ResolveIdHtmlPath()
    {
        if (_idHtmlPath != null)
        {
            return _idHtmlPath;
        }
        string fromCwd = Path.Combine(Directory.GetCurrentDirectory(), "web", "id.html");
        if (File.Exists(fromCwd))
        {
            _idHtmlPath = fromCwd;
            return _idHtmlPath;
        }
        string fromBase = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "web", "id.html");
        if (File.Exists(fromBase))
        {
            _idHtmlPath = Path.GetFullPath(fromBase);
            return _idHtmlPath;
        }
        // ตอน publish (self-contained) ไฟล์ถูกก๊อปไปข้าง ๆ binary
        string beside = Path.Combine(AppContext.BaseDirectory, "web", "id.html");
        if (File.Exists(beside))
        {
            _idHtmlPath = beside;
            return _idHtmlPath;
        }
        return null;
    }
}
