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
/// [4 ก.ย. 2026] เทส "รับให้ได้ทุกแพ็กเก็ต" (ServerPlayer.Fallback.cs)
///
/// - message ที่มี handler เบา ๆ (Keepalive / GetLatestChatLog / SetRecipeLike / DrinkWater) ต้องตอบชนิดที่ถูก
///   พร้อม ReplyOf = Seq ของคำขอ
/// - message ที่ไม่มีใครรับเลย (HostConcert / MountVehicle) ต้องได้ Abort พร้อมข้อความไทย ไม่ใช่เงียบ
/// - message แจ้งเหตุ (PlayerDrawLine) ต้องเงียบ ไม่มี Abort โผล่
/// - ยิงซ้ำหลายรอบต้องได้คำตอบทุกรอบ และ connection ยังอยู่
///
/// รัน: dotnet run -- --fallback-check [host] [port เกม] [port gateway]
/// </summary>
public static class FallbackCheck
{
    private static int _passed;
    private static int _failed;
    private static readonly List<(string type, uint replyOf, string text)> Replies = new List<(string, uint, string)>();

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static void Reset() { lock (Replies) { Replies.Clear(); } }

    private static int Count(string type)
    {
        lock (Replies) { int n = 0; foreach (var r in Replies) { if (r.type == type) n++; } return n; }
    }

    private static bool AllHaveReplyOf(string type)
    {
        lock (Replies) { foreach (var r in Replies) { if (r.type == type && r.replyOf == 0) return false; } return Count(type) > 0; }
    }

    private static string Texts(string type)
    {
        lock (Replies) { var l = new List<string>(); foreach (var r in Replies) { if (r.type == type) l.Add(r.text ?? ""); } return string.Join("|", l); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        string id = "fallback-" + Guid.NewGuid().ToString("N")[..8];
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("[FAIL] ขอ session ไม่ได้"); return 2; }
        if (!string.IsNullOrEmpty(SessionClient.LastUserId)) { id = SessionClient.LastUserId; }   // token ผูกกับ id ที่ gateway ออกให้

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        void Note(string type, PacketHeader h, string text = null) { lock (Replies) { Replies.Add((type, h.ReplyOf, text)); } }
        connection.Recv<Welcome>((m, h) => { }); connection.Recv<Clock>((m, h) => { });
        connection.Recv<Info>((m, h) => Note("Info", h, m.Text));
        connection.Recv<Abort>((m, h) => Note("Abort", h, m.Text));
        connection.Recv<OK>((m, h) => Note("OK", h));
        connection.Recv<Keepalive>((m, h) => Note("Keepalive", h));
        connection.Recv<ChatLogs>((m, h) => Note("ChatLogs", h, (m.Logs?.Length ?? -1).ToString()));
        connection.Recv<StatusEffects>((m, h) => Note("StatusEffects", h));
        connection.Recv<Inventory>((m, h) => { }); connection.Recv<Skills>((m, h) => { });
        connection.Recv<Statistics>((m, h) => { }); connection.Recv<Equipments>((m, h) => { });
        connection.Recv<Survival>((m, h) => { }); connection.Recv<Points>((m, h) => { });
        connection.Recv<AppearPlayer>((m, h) => { }); connection.Recv<AppearAnimal>((m, h) => { });
        connection.Recv<AppearArtifact>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { }); connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { }); connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { }); connection.Recv<Move>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "CustomClient 0.1.4", DeviceModel = "fallback-check" }); Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 1600);

        Console.WriteLine("=== fallback check: ทุกแพ็กเก็ตต้องได้คำตอบ ===");

        Reset(); connection.Send(default(Keepalive)); Pump(connection, 400);
        Check("Keepalive answered with Keepalive (ReplyOf set)", AllHaveReplyOf("Keepalive"), $"keepalive={Count("Keepalive")} abort={Count("Abort")}");

        Reset(); connection.Send(default(GetLatestChatLog)); Pump(connection, 400);
        Check("GetLatestChatLog answered with empty ChatLogs", AllHaveReplyOf("ChatLogs") && Texts("ChatLogs") == "0", $"chatlogs={Count("ChatLogs")} logs={Texts("ChatLogs")} abort={Count("Abort")}");

        Reset(); connection.Send(default(SetRecipeLike)); Pump(connection, 400);
        Check("SetRecipeLike acked with OK", AllHaveReplyOf("OK") && Count("Abort") == 0, $"ok={Count("OK")} abort={Count("Abort")}");

        Reset(); connection.Send(default(DrinkWater)); Pump(connection, 500);
        Check("DrinkWater acked with OK (+drink_water buff)", AllHaveReplyOf("OK") && Count("Abort") == 0, $"ok={Count("OK")} statusEffects={Count("StatusEffects")} abort={Count("Abort")}");

        Reset(); connection.Send(default(HostConcert)); Pump(connection, 400);
        Check("HostConcert (no handler) answered with Abort + Thai text", AllHaveReplyOf("Abort") && Texts("Abort").Contains("ยังไม่เปิดในรุ่นนี้") && Texts("Abort").Contains("HostConcert"), $"abort={Count("Abort")} text={Texts("Abort")}");

        Reset(); connection.Send(default(MountVehicle)); Pump(connection, 400);
        Check("MountVehicle (no handler) answered with Abort", AllHaveReplyOf("Abort"), $"abort={Count("Abort")} text={Texts("Abort")}");

        Reset(); connection.Send(new PlayerDrawLine { PlayerId = id, DrawCommands = Array.Empty<DrawLineBase>() }); Pump(connection, 400);
        Check("PlayerDrawLine (notification) stays silent", Count("Abort") == 0 && Count("OK") == 0, $"abort={Count("Abort")} ok={Count("OK")}");

        Reset();
        for (int i = 0; i < 5; i++) connection.Send(default(HostConcert));
        Pump(connection, 800);
        Check("repeated unhandled request answered every time", Count("Abort") == 5, $"abort={Count("Abort")}");

        Check("still connected after all fallbacks", connection.Connected());

        connection.Close();
        Console.WriteLine();
        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
