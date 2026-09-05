using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json.Linq;

namespace DurangoTestClient;

/// <summary>
/// [3 ก.ย. 2026] เทส "ระบบเหมือน PC" ที่เซิร์ฟทำแทนให้เกมมือถือของแท้ (docs/server/Android.md)
/// จำลอง client 2 ตัว: A = มือถือ (platform=Android · Auth 5.2.1 · build=android-0.1.4) · B = PC ชุดเรา (CustomClient 0.1.4)
///   1. /knock และ /entry ตอบ cluster_mode ตาม platform (Android ได้ ServerConfig.Android.ClusterMode)
///   2. A เข้าโลก → ได้ popup Info "ยินดีต้อนรับ … ออนไลน์ N คน" · B ไม่ได้
///   3. B เข้าโลก → A ได้แชทช่อง System "… เข้าเกม · ออนไลน์ 2 คน" · B ไม่ได้
///   4. /admin/broadcast แบบมีสไตล์ → A ได้ข้อความธรรมดา (ไม่มี ##bc|) · B ได้ "##bc|…" เต็ม
///   5. B ออก → A ได้แชท "… ออกจากเกม · ออนไลน์ 1 คน"
/// ใช้: --android-check [host] [port เกม] [port gateway] [admin token]
/// </summary>
public static class AndroidParityCheck
{
    private sealed class Client
    {
        public string Id;
        public Connection Conn;
        public Socket Socket;
        public readonly List<string> Infos = new List<string>();
        public readonly List<string> SystemChats = new List<string>();
        public readonly HashSet<string> Seen = new HashSet<string>();
        public readonly List<string> Notices = new List<string>();
        public int Aborts;
    }

    private static void Pump(params Client[] clients)
    {
        for (int i = 0; i < 60; i++)
        {
            foreach (Client c in clients) c.Conn.Process();
            Thread.Sleep(10);
        }
    }

    public static int Run(string host, int gamePort, int gatewayPort, string adminToken)
    {
        int failures = 0;
        void Check(string what, bool ok, string detail = null)
        {
            Console.WriteLine($"  [{(ok ? "ผ่าน" : "ตก ")}] {what}{(detail == null ? "" : " — " + detail)}");
            if (!ok) failures++;
        }
        string gw = $"http://{host}:{gatewayPort}";
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };

        // 1) knock / entry ตาม platform
        JObject knockA = JObject.Parse(http.GetStringAsync(gw + "/knock?version=5.2.1&build=android-0.1.4&platform=Android").GetAwaiter().GetResult());
        JObject knockP = JObject.Parse(http.GetStringAsync(gw + "/knock?version=CustomClient%200.1.4&platform=WindowsPlayer").GetAwaiter().GetResult());
        string modeA = (string)knockA["cluster_mode"], modeP = (string)knockP["cluster_mode"];
        Check("/knock มือถือ compatible", (bool?)knockA["compatible"] == true);
        Check("/knock มือถือได้ cluster_mode Online จริง (เจ้าของสั่ง 4 ก.ย.: ไม่ใช้ SingleMode)", modeA == "Online", "ได้ " + modeA);
        Check("/knock PC ได้ cluster_mode Online", modeP == "Online", "ได้ " + modeP);
        Check("/knock มือถือชี้ bundle ชุด Android", ((string)knockA["assetbundle_url_root"] ?? "").Contains("/android/"), (string)knockA["assetbundle_url_root"]);

        // 2) ตัวละครมือถือ A
        string model = "{\"hair\":\"Models/PC/Female/Hair/f_hair_long\",\"skin_color\":\"C8A07A\",\"voice_type\":1,\"body_size\":1.0}";
        string idA = CreateCharacterCheck.CreatePlayer(host, gatewayPort, "มือถือเทส", isMale: false, model);
        string idB = CreateCharacterCheck.CreatePlayer(host, gatewayPort, "พีซีเทส", isMale: true, model);
        Check("สร้างตัวละคร A/B ได้", !string.IsNullOrEmpty(idA) && !string.IsNullOrEmpty(idB));
        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB)) return Fail(failures);

        string tokA = Session(http, gw, idA, "Android", "Android OS 12 / API-31");
        string tokB = Session(http, gw, idB, "WindowsPlayer", "Windows 10");
        Check("POST /sessions (platform=Android / WindowsPlayer) ได้ token", !string.IsNullOrEmpty(tokA) && !string.IsNullOrEmpty(tokB));
        if (string.IsNullOrEmpty(tokA) || string.IsNullOrEmpty(tokB)) return Fail(failures);

        JObject entryA = JObject.Parse(http.GetStringAsync(gw + "/entry?entity_id=" + idA + "&build=android-0.1.4&platform=Android").GetAwaiter().GetResult());
        JObject entryB = JObject.Parse(http.GetStringAsync(gw + "/entry?entity_id=" + idB + "&platform=WindowsPlayer").GetAwaiter().GetResult());
        Check("/entry มือถือ cluster_mode = Online", (string)entryA["cluster_mode"] == "Online", (string)entryA["cluster_mode"]);
        Check("/entry PC cluster_mode ของเซิร์ฟ", (string)entryB["cluster_mode"] == modeP, (string)entryB["cluster_mode"]);

        Client a = Connect(host, gamePort, idA, tokA, "5.2.1", "samsung SM-G991B");
        Pump(a);
        Check("A (มือถือ) เข้าโลกได้", a.Aborts == 0, "aborts=" + a.Aborts);
        Check("A ได้ popup (RadioNotice) ยินดีต้อนรับ + จำนวนคนออนไลน์",
            a.Notices.Exists(t => t.Contains("ยินดีต้อนรับ") && t.Contains("ออนไลน์")), string.Join(" | ", a.Notices));

        // 3) PC B เข้า → A ได้แชทระบบ · B ไม่ได้อะไร
        Client b = Connect(host, gamePort, idB, tokB, "CustomClient 0.1.4", "PC");
        Pump(a, b); Pump(a, b);
        Check("B (PC) เข้าโลกได้", b.Aborts == 0, "aborts=" + b.Aborts);
        Check("A (มือถือ) เห็น AppearPlayer ของ B", a.Seen.Contains(idB), "เห็น " + a.Seen.Count + " ตัว");
        Check("B (PC) เห็น AppearPlayer ของ A", b.Seen.Contains(idA), "เห็น " + b.Seen.Count + " ตัว");
        Check("A ได้แชทระบบ 'พีซีเทส เข้าเกม · ออนไลน์ 2 คน'",
            a.SystemChats.Exists(t => t.Contains("พีซีเทส") && t.Contains("เข้าเกม") && t.Contains("ออนไลน์ 2 คน")), string.Join(" | ", a.SystemChats));
        // (ExampleMod ที่โหลดในเซิร์ฟ dev ส่ง "[ExampleMod] … ยินดีต้อนรับ!" ของมันเอง — ดูเฉพาะข้อความของเซิร์ฟที่มี "ออนไลน์ตอนนี้")
        Check("B (PC) ไม่ได้ popup ยินดีต้อนรับ+จำนวนคน", !b.Infos.Exists(t => t.Contains("ออนไลน์ตอนนี้")) && b.Notices.Count == 0, string.Join(" | ", b.Infos));
        Check("B (PC) ไม่ได้แชทระบบจำนวนคน", b.SystemChats.Count == 0, string.Join(" | ", b.SystemChats));

        // 4) บรอดแคสต์แบบมีสไตล์
        a.Infos.Clear(); b.Infos.Clear(); a.Notices.Clear();
        bool sent = Broadcast(http, gw, adminToken, "ประกาศทดสอบ", "5", "2", "FF3333");
        Check("POST /admin/broadcast สำเร็จ", sent);
        Pump(a, b);
        Check("A (มือถือ) ได้ RadioNotice ข้อความธรรมดา (ไม่มี ##bc|)", a.Notices.Contains("ประกาศทดสอบ") && a.Infos.Count == 0, string.Join(" | ", a.Notices) + " / Info: " + string.Join(" | ", a.Infos));
        Check("B (PC 0.1.4) ได้ ##bc| เต็ม", b.Infos.Exists(t => t.StartsWith("##bc|") && t.EndsWith("ประกาศทดสอบ")), string.Join(" | ", b.Infos));

        // 5) B ออก → A ได้แชทระบบ
        a.SystemChats.Clear();
        b.Conn.Close(); b.Socket.Close();
        Pump(a); Pump(a); Pump(a);
        Check("A ได้แชทระบบ 'ออกจากเกม · ออนไลน์ 1 คน'",
            a.SystemChats.Exists(t => t.Contains("ออกจากเกม") && t.Contains("ออนไลน์ 1 คน")), string.Join(" | ", a.SystemChats));
        a.Conn.Close(); a.Socket.Close();

        Console.WriteLine(failures == 0 ? "[PASS] android-check ผ่านทุกข้อ" : $"[FAIL] android-check ตก {failures} ข้อ");
        return failures == 0 ? 0 : 1;
    }

    private static int Fail(int failures)
    {
        Console.WriteLine($"[FAIL] android-check ตก {failures} ข้อ (หยุดก่อนถึงส่วนที่เหลือ)");
        return 1;
    }

    private static string Session(HttpClient http, string gw, string entityId, string platform, string os)
    {
        string form = "player=" + Uri.EscapeDataString("{\"appear_player\":{\"entity_id\":\"" + entityId + "\"}}") +
                      "&platform=" + Uri.EscapeDataString(platform) + "&os_version=" + Uri.EscapeDataString(os) +
                      "&account_provider=guest&account_id=" + entityId;
        var body = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded");
        string reply = http.PostAsync(gw + "/sessions", body).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
        try { return (string)JObject.Parse(reply)["session_token"]; } catch { Console.WriteLine("[sessions] ตอบแปลก: " + reply); return null; }
    }

    private static bool Broadcast(HttpClient http, string gw, string token, string text, string duration, string size, string color)
    {
        string form = "text=" + Uri.EscapeDataString(text) + "&duration=" + duration + "&size=" + size + "&color=" + color;
        var req = new HttpRequestMessage(HttpMethod.Post, gw + "/admin/broadcast")
        {
            Content = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded")
        };
        if (!string.IsNullOrEmpty(token)) req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = http.SendAsync(req).GetAwaiter().GetResult();
        string reply = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        if (!resp.IsSuccessStatusCode) Console.WriteLine("[broadcast] " + (int)resp.StatusCode + " " + reply);
        return resp.IsSuccessStatusCode;
    }

    private static Client Connect(string host, int gamePort, string entityId, string token, string clientVersion, string device)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var conn = new Connection(socket);
        var c = new Client { Id = entityId, Conn = conn, Socket = socket };
        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Abort>((m, h) => c.Aborts++);
        conn.Recv<Info>((m, h) => c.Infos.Add(m.Text ?? ""));
        conn.Recv<SayInExclusiveChannel>((m, h) =>
        {
            if (m.ChannelType != Shared.Chat.ChannelType.System) return;
            if (m.Message.Body is RadioNotice rn) { c.Notices.Add(rn.Text ?? ""); return; }   // popup ของเกมของแท้
            string text = m.Message.Body is RadioTalk rt ? rt.Text : m.Message.Body?.ToString();
            c.SystemChats.Add((m.Message.Speaker.HasValue ? m.Message.Speaker.Value.Name + ": " : "") + text);
        });
        conn.Recv<AppearPlayer>((m, h) => c.Seen.Add(m.EntityId)); conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Clock>((m, h) => { }); conn.Recv<OK>((m, h) => { });
        conn.Recv<Inventory>((m, h) => { }); conn.Recv<Skills>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { }); conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Survival>((m, h) => { }); conn.Recv<Points>((m, h) => { });
        conn.Recv<AppearAnimal>((m, h) => { }); conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<Move>((m, h) => { }); conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { }); conn.Recv<WalletUpdated>((m, h) => { });
        conn.Recv<Recipes>((m, h) => { }); conn.Recv<ArtifactBlueprints>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.StartReceive();
        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(c);
        conn.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = clientVersion, DeviceModel = device });
        Pump(c);
        conn.Send(default(Ready));
        Pump(c); Pump(c);
        return c;
    }
}
