using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>
/// เทส **ผู้เล่นเข้าเซิร์ฟแล้วเห็นกันจริงไหม** (multiplayer presence / AppearPlayer สองทาง)
///
/// จำลอง 2 ผู้เล่น (ตั้ง DeviceModel=Android ให้เหมือนมือถือ) เข้าโลกเดียวกัน:
///   1. A เข้าโลก รอให้ settle (ได้ chunk รอบตัว)
///   2. B เข้าโลก รอ settle
///   3. pump ทั้งคู่ต่ออีกหลายวินาที ให้ AppearPlayer push ถึงกันสองทาง
///   4. เช็ค: A เห็น B (เจอ EntityId ของ B ใน AppearPlayer) · B เห็น A
///
/// ตอบคำถาม "Android เข้าไปเจอคนในเซิร์ฟไหม"
/// รัน: dotnet run -- --presence-check [host] [gamePort] [gatewayPort]
/// </summary>
public static class PresenceCheck
{
    private static int _passed, _failed;
    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private sealed class Session
    {
        public Connection Conn;
        public string Id;              // user_id ที่ gateway ออกให้ (= EntityId ของตัวเอง)
        public string Name;
        public readonly Dictionary<string, string> SeenPlayers = new Dictionary<string, string>(); // id -> name
        public int Appears;
    }

    private static void Pump(Session s, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { s.Conn.Process(); Thread.Sleep(10); }
    }

    private static void PumpBoth(Session a, Session b, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { a.Conn.Process(); b.Conn.Process(); Thread.Sleep(10); }
    }

    private static Session Connect(string host, int gamePort, int gatewayPort, string name)
    {
        string token = SessionClient.Fetch(host, gatewayPort, name, name);
        if (string.IsNullOrEmpty(token)) return null;
        // ต้องเก็บ id ทันทีหลัง Fetch — LastUserId เป็น static ตัวถัดไปจะทับ
        string id = !string.IsNullOrEmpty(SessionClient.LastUserId) ? SessionClient.LastUserId : name;

        var sess = new Session { Id = id, Name = name };
        var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        var conn = new Connection(sock);
        sess.Conn = conn;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => { });
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Actions>((m, h) => { });
        conn.Recv<Inventory>((m, h) => { });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Recipes>((m, h) => { });
        conn.Recv<ArtifactBlueprints>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.Recv<AppearPlayer>((m, h) =>
        {
            sess.Appears++;
            if (!string.IsNullOrEmpty(m.EntityId) && m.EntityId != sess.Id)
                sess.SeenPlayers[m.EntityId] = m.Name ?? "";
        });
        conn.Recv<AppearAnimal>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<AppearNatural>((m, h) => { });
        conn.Recv<AppearEntity>((m, h) => { });
        conn.Recv<AppearEntityOnTile>((m, h) => { });
        conn.Recv<AppearPet>((m, h) => { });
        conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(sess, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "Android" });
        Pump(sess, 600);
        conn.Send(default(Ready));
        Pump(sess, 2500);
        return sess;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== presence check (เจอกันไหม): {host}:{gamePort} ===");
        string tag = Guid.NewGuid().ToString("N").Substring(0, 4);
        string nameA = "มือถือA_" + tag;
        string nameB = "มือถือB_" + tag;

        Console.WriteLine($"[1] {nameA} เข้าโลก...");
        Session a = Connect(host, gamePort, gatewayPort, nameA);
        if (a == null) { Console.WriteLine("A ขอ token ไม่ได้"); return 1; }
        Console.WriteLine($"    A id={a.Id}");

        Console.WriteLine($"[2] {nameB} เข้าโลก...");
        Session b = Connect(host, gamePort, gatewayPort, nameB);
        if (b == null) { Console.WriteLine("B ขอ token ไม่ได้"); a.Conn.Close(); return 1; }
        Console.WriteLine($"    B id={b.Id}");

        Console.WriteLine("[3] รอให้ AppearPlayer propagate สองทาง (pump ทั้งคู่ 8 วิ)...");
        PumpBoth(a, b, 8000);

        Console.WriteLine("[4] ผล:");
        Console.WriteLine($"    A ({a.Name}) เห็นผู้เล่น {a.SeenPlayers.Count} คน: {string.Join(", ", a.SeenPlayers.Values.Take(6))}");
        Console.WriteLine($"    B ({b.Name}) เห็นผู้เล่น {b.SeenPlayers.Count} คน: {string.Join(", ", b.SeenPlayers.Values.Take(6))}");

        bool aSeesB = a.SeenPlayers.ContainsKey(b.Id);
        bool bSeesA = b.SeenPlayers.ContainsKey(a.Id);
        Check("A (มือถือ) เห็น B ในเซิร์ฟ", aSeesB, aSeesB ? "เห็น B แล้ว" : "ไม่เห็น B");
        Check("B (มือถือ) เห็น A ในเซิร์ฟ", bSeesA, bSeesA ? "เห็น A แล้ว" : "ไม่เห็น A");
        Check("เห็นกันครบสองทาง (เข้าไปเจอคนจริง)", aSeesB && bSeesA);

        a.Conn.Close();
        b.Conn.Close();
        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
