using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทสเส้นทาง "สร้างตัวละครใหม่" ให้ครบวง — POST /players ➜ /sessions (ส่งแค่ id) ➜ เข้าเกม
///
/// สิ่งที่กันไว้: เดิม gateway ทิ้งชื่อ/เพศ/หน้าตาที่หน้าสร้างส่งมา แล้วคืน GUID เปล่า ๆ
/// ⇒ ตัวละครเข้าเกมมาไม่มีชื่อ หน้าตาเป็นค่า default (log `name=(ว่าง!) ... display=no`)
/// เทสนี้จะตกทันทีถ้าอาการนั้นกลับมา
/// </summary>
public static class CreateCharacterCheck
{
    private const string CharName = "ทดสอบสร้าง";
    private const string Hair = "Models/PC/Female/Hair/f_hair_long";
    private const string SkinColor = "C8A07A";
    private const float BodySize = 1.23f;
    private const int VoiceType = 4;

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        int failures = 0;
        void Check(string what, bool ok, string detail = null)
        {
            Console.WriteLine($"  [{(ok ? "ผ่าน" : "ตก ")}] {what}{(detail == null ? "" : " — " + detail)}");
            if (!ok) failures++;
        }

        // 1) สร้างตัวละครเหมือนหน้าสร้างในโปรล็อก (PrologueManager.RequestCreatePlayer)
        string modelInfo =
            "{\"hair\":\"" + Hair + "\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"" + SkinColor + "\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":" + VoiceType + "," +
            "\"body_size\":" + BodySize.ToString(CultureInfo.InvariantCulture) + "}";
        string entityId = CreatePlayer(host, gatewayPort, CharName, isMale: false, modelInfo);
        Check("POST /players คืน entity_id", !string.IsNullOrEmpty(entityId), entityId);
        if (string.IsNullOrEmpty(entityId))
        {
            Console.WriteLine($"[FAIL] create-check ตก {failures} ข้อ");
            return 1;
        }

        // 2) ขอ token แบบที่ client ทำหลังสร้างเสร็จ — ส่งมาแค่ entity id ไม่มีชื่อ/หน้าตา
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + entityId + "\"}}");
        Check("POST /sessions (ส่งแค่ id) ได้ token", !string.IsNullOrEmpty(token));
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"[FAIL] create-check ตก {failures} ข้อ");
            return 1;
        }

        // 3) เข้าเกมแล้วดูว่า server ประกาศตัวเราออกมาหน้าตาตรงกับที่สร้างไหม
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        AppearPlayer me = default;
        bool sawMe = false;
        string welcomeName = null;
        int aborts = 0;
        connection.Recv<Welcome>((m, h) => welcomeName = m.Name);
        connection.Recv<Abort>((m, h) => aborts++);
        connection.Recv<AppearPlayer>((m, h) => { if (m.EntityId == entityId) { me = m; sawMe = true; } });
        connection.Recv<Clock>((m, h) => { }); connection.Recv<OK>((m, h) => { });
        connection.Recv<Inventory>((m, h) => { }); connection.Recv<Skills>((m, h) => { });
        connection.Recv<Statistics>((m, h) => { }); connection.Recv<Equipments>((m, h) => { });
        connection.Recv<Survival>((m, h) => { }); connection.Recv<Points>((m, h) => { });
        connection.Recv<AppearAnimal>((m, h) => { }); connection.Recv<AppearArtifact>((m, h) => { });
        connection.Recv<Move>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { }); connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { }); connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "create-check" });
        Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 2200);
        connection.Close();

        Check("เข้าเกมได้ (ไม่มี Abort)", aborts == 0, "aborts=" + aborts);
        Check("Welcome ใช้ชื่อที่สร้าง", welcomeName == CharName, "ได้ '" + welcomeName + "'");
        Check("เห็น AppearPlayer ของตัวเอง", sawMe);
        if (!sawMe)
        {
            Console.WriteLine($"[FAIL] create-check ตก {failures} ข้อ");
            return 1;
        }
        Check("ชื่อตรงกับที่สร้าง", me.Name == CharName, "ได้ '" + me.Name + "'");
        Check("เพศหญิง (entity type 1001)", me.EntityType == 1001, "ได้ " + me.EntityType);
        Check("ทรงผมตรงกับที่ปั้น", me.Display.Hair == Hair, "ได้ '" + me.Display.Hair + "'");
        Check("สีผิวตรงกับที่ปั้น", me.Display.SkinColor == SkinColor, "ได้ '" + me.Display.SkinColor + "'");
        Check("ขนาดตัวตรงกับที่ปั้น", Math.Abs(me.Display.BodySize - BodySize) < 0.001f, "ได้ " + me.Display.BodySize);
        Check("เสียงตรงกับที่ปั้น", me.Display.VoiceType == VoiceType, "ได้ " + me.Display.VoiceType);
        Check("ร่างเปล่าเป็นของผู้หญิง",
            me.Display.DefaultBody != null && me.Display.DefaultBody.Contains("Female"),
            "ได้ '" + me.Display.DefaultBody + "'");

        Console.WriteLine(failures == 0
            ? "[PASS] create-check ผ่านทุกข้อ"
            : $"[FAIL] create-check ตก {failures} ข้อ");
        return failures == 0 ? 0 : 1;
    }

    /// <summary>ยิง POST /players เหมือนหน้าสร้างตัวละคร แล้วอ่าน entity_id ที่ server คืนมา</summary>
    internal static string CreatePlayer(string host, int gatewayPort, string name, bool isMale, string modelInfo)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            string form =
                "region_id=0" +
                "&gender=" + (isMale ? "male" : "female") +
                "&name=" + Uri.EscapeDataString(name) +
                "&region=" + Uri.EscapeDataString("personal_region_forest") +
                "&job=5&slot=0" +
                "&model_info=" + Uri.EscapeDataString(modelInfo);
            var body = new StringContent(form, Encoding.UTF8, "application/x-www-form-urlencoded");
            string reply = http.PostAsync($"http://{host}:{gatewayPort}/players", body)
                .GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
            int at = reply.IndexOf("\"entity_id\"", StringComparison.Ordinal);
            if (at < 0)
            {
                Console.WriteLine("[create] /players ตอบแปลก ๆ: " + reply);
                return null;
            }
            int open = reply.IndexOf('"', reply.IndexOf(':', at));
            int close = reply.IndexOf('"', open + 1);
            return close > open ? reply.Substring(open + 1, close - open - 1) : null;
        }
        catch (Exception e)
        {
            Console.WriteLine("[create] POST /players ไม่ได้: " + e.Message);
            return null;
        }
    }
}
