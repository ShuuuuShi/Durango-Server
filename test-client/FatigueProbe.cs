using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// Probe สั้น ๆ ไว้ตอบคำถามเดียว: สั่ง `cheat exhaust` แล้วค่าความล้าที่ client ได้รับคือเท่าไร
///
/// ดัมป์ทุก packet Survival / SurvivalUpdated ที่ได้รับแบบดิบ ๆ พร้อมโครงสร้าง Gauge ข้างใน
/// (จำนวน node · เวลา · ค่า · Max) เพื่อแยกให้ออกว่าปัญหาอยู่ที่
///   ก) เซิร์ฟส่งค่าผิด · ข) เซิร์ฟไม่ส่งเลย · ค) ตัวเทสตีความ Gauge ผิด
///
/// รัน: dotnet run -- --fatigue-probe [host] [port เกม] [port gateway]
/// </summary>
public static class FatigueProbe
{
    private static string _id;

    private static void Dump(string tag, Dictionary<string, Gauge> gauges)
    {
        double now = Times.UnixTimeNow();
        if (gauges == null) { Console.WriteLine($"  [{tag}] (null)"); return; }
        foreach (KeyValuePair<string, Gauge> kv in gauges)
        {
            Gauge g = kv.Value;
            if (g == null) { Console.WriteLine($"  [{tag}] {kv.Key} = (null)"); continue; }
            GaugeNode[] nodes = g.Determination;
            string nodeText = "(ไม่มี node)";
            if (nodes != null && nodes.Length > 0)
            {
                var parts = new List<string>();
                for (int i = 0; i < nodes.Length; i++)
                {
                    parts.Add($"[t{(nodes[i].Time - now):+0.0;-0.0} v={nodes[i].Value:F2}]");
                }
                nodeText = string.Join(" ", parts);
            }
            Console.WriteLine($"  [{tag}] {kv.Key,-8} Get(now)={g.Get(now):F2}  Max={g.Max(now):F1}  nodes={nodeText}");
        }
    }

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== fatigue probe: {host}:{gamePort} ===");

        string modelInfo =
            "{\"hair\":\"hair_f_01\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"F0D9B7\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":1,\"body_size\":1.0}";
        _id = CreateCharacterCheck.CreatePlayer(host, gatewayPort,
            "probe-" + Guid.NewGuid().ToString("N").Substring(0, 6), isMale: false, modelInfo);
        if (string.IsNullOrEmpty(_id)) { Console.WriteLine("สร้างตัวละครไม่ได้"); return 2; }
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + _id + "\"}}");
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("ขอ token ไม่ได้"); return 2; }

        using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        var conn = new Connection(sock);

        int survivalCount = 0, updatedCount = 0;
        conn.Recv<Survival>((m, h) =>
        {
            if (m.EntityId != _id) return;
            survivalCount++;
            Console.WriteLine($"<< Survival #{survivalCount}");
            Dump("full", m.Gauges);
        });
        conn.Recv<SurvivalUpdated>((m, h) =>
        {
            if (m.EntityId != _id) return;
            updatedCount++;
            Console.WriteLine($"<< SurvivalUpdated #{updatedCount}");
            Dump("upd", m.Updated);
        });
        conn.Recv<Info>((m, h) => Console.WriteLine($"<< Info: {m.Text}"));
        conn.Recv<Welcome>((m, h) => { }); conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { }); conn.Recv<Abort>((m, h) => Console.WriteLine("<< Abort"));
        conn.Recv<Inventory>((m, h) => { }); conn.Recv<Skills>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { }); conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Points>((m, h) => { }); conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearAnimal>((m, h) => { }); conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<Move>((m, h) => { }); conn.Recv<Chunk>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { }); conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { }); conn.Recv<Recipes>((m, h) => { });
        conn.Recv<ArtifactBlueprints>((m, h) => { }); conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Messages.StatusEffects>((m, h) =>
            Console.WriteLine($"<< StatusEffects: {(m._StatusEffects == null ? 0 : m._StatusEffects.Length)} ตัว"));
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(conn, 300);
        conn.Send(new Auth { EntityId = _id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "probe" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2500);

        Console.WriteLine("\n--- ส่ง cheat exhaust (ควรตั้งความล้าเป็น 90) ---");
        int before = updatedCount;
        conn.Send(new Cheat { _Cheat = "exhaust" });
        Pump(conn, 2000);
        Console.WriteLine($"--- ได้ SurvivalUpdated เพิ่ม {updatedCount - before} แพ็กเก็ตหลังสั่ง exhaust ---");

        Console.WriteLine("\n--- ส่ง cheat survival (ให้เซิร์ฟรายงานค่าที่ตัวเองเก็บ) ---");
        conn.Send(new Cheat { _Cheat = "survival" });
        Pump(conn, 1500);

        Console.WriteLine("\n--- ส่ง cheat rest (ควรล้างความล้าเป็น 0) ---");
        before = updatedCount;
        conn.Send(new Cheat { _Cheat = "rest" });
        Pump(conn, 2000);
        Console.WriteLine($"--- ได้ SurvivalUpdated เพิ่ม {updatedCount - before} แพ็กเก็ตหลังสั่ง rest ---");

        Console.WriteLine($"\nสรุป: Survival {survivalCount} ครั้ง · SurvivalUpdated {updatedCount} ครั้ง");
        conn.Close();
        return 0;
    }
}
