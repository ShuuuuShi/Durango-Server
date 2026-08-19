using System;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

public static class SmokeCheck
{
    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        string id = "smoke-" + Guid.NewGuid().ToString("N")[..8];
        string token = SessionClient.Fetch(host, gatewayPort, id, "SmokeCheck");
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("[FAIL] gateway session"); return 1; }
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        bool welcome = false, inventory = false, skills = false, stats = false, equipment = false, survival = false, points = false;
        int aborts = 0;
        connection.Recv<Welcome>((m, h) => welcome = true);
        connection.Recv<Clock>((m, h) => { }); connection.Recv<OK>((m, h) => { });
        connection.Recv<Abort>((m, h) => aborts++);
        connection.Recv<Inventory>((m, h) => inventory = true);
        connection.Recv<Skills>((m, h) => skills = true);
        connection.Recv<Statistics>((m, h) => stats = true);
        connection.Recv<Equipments>((m, h) => equipment = m.Presets?.Count >= 3);
        connection.Recv<Survival>((m, h) => survival = m.Gauges?.Count >= 3);
        connection.Recv<Points>((m, h) => points = true);
        connection.Recv<AppearPlayer>((m, h) => { }); connection.Recv<AppearAnimal>((m, h) => { });
        connection.Recv<AppearArtifact>((m, h) => { }); connection.Recv<Move>((m, h) => { });
        connection.Recv<DefoggedChunks>((m, h) => { }); connection.Recv<QuestCategories>((m, h) => { });
        connection.Recv<WalletUpdated>((m, h) => { }); connection.Recv<Recipes>((m, h) => { });
        connection.Recv<ArtifactBlueprints>((m, h) => { }); connection.Recv<Chunk>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "smoke-check" }); Pump(connection, 450);
        connection.Send(default(Ready)); Pump(connection, 2200);
        connection.Close();
        bool ok = welcome && inventory && skills && stats && equipment && survival && points && aborts == 0;
        Console.WriteLine($"[{(ok ? "PASS" : "FAIL")}] welcome={welcome} inventory={inventory} skills={skills} stats={stats} equipment3={equipment} survival={survival} points={points} aborts={aborts}");
        return ok ? 0 : 1;
    }
}
