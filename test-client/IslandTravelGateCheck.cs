using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>Regression: disabled IslandTravel replies without teleporting or starting a handoff.</summary>
public static class IslandTravelGateCheck
{
    private static int _passed;
    private static int _failed;
    private static readonly List<string> Infos = new List<string>();
    private static int _aborts;
    private static int _oks;
    private static int _teleports;
    private static int _moves;
    private static int _costs;
    private static int _tutorialReady;
    private static int _emigrated;

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static void Reset()
    {
        lock (Infos) { Infos.Clear(); }
        _aborts = _oks = _teleports = _moves = _costs = _tutorialReady = _emigrated = 0;
    }

    private static void AssertDisabled(string action)
    {
        string all;
        lock (Infos) { all = string.Join("\n", Infos); }
        Check(action + " returns disabled message and Abort", _aborts == 1 && all.Contains("การเดินทางข้ามเกาะยังไม่เปิด"),
            $"info={all.Replace('\n', '|')} abort={_aborts}");
        Check(action + " does not start a travel state transition",
            _oks == 0 && _teleports == 0 && _costs == 0 && _tutorialReady == 0 && _emigrated == 0,
            $"ok={_oks} teleported={_teleports} ambientMove={_moves} costs={_costs} tutorial={_tutorialReady} emigrated={_emigrated}");
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        string id = "travel-gate-" + Guid.NewGuid().ToString("N")[..8];
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("[FAIL] ขอ session ไม่ได้"); return 2; }

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        connection.Recv<Welcome>((m, h) => { }); connection.Recv<Clock>((m, h) => { });
        connection.Recv<Info>((m, h) => { lock (Infos) { Infos.Add(m.Text ?? ""); } });
        connection.Recv<Abort>((m, h) => _aborts++); connection.Recv<OK>((m, h) => _oks++);
        connection.Recv<Teleported>((m, h) => _teleports++); connection.Recv<Move>((m, h) => _moves++);
        connection.Recv<WarpCosts>((m, h) => _costs++); connection.Recv<DepartTutorialReady>((m, h) => _tutorialReady++);
        connection.Recv<Emigrated>((m, h) => _emigrated++);
        connection.Recv<Inventory>((m, h) => { }); connection.Recv<Skills>((m, h) => { });
        connection.Recv<Statistics>((m, h) => { }); connection.Recv<Equipments>((m, h) => { });
        connection.Recv<Survival>((m, h) => { }); connection.Recv<Points>((m, h) => { });
        connection.Recv<AppearPlayer>((m, h) => { }); connection.Recv<AppearAnimal>((m, h) => { });
        connection.Recv<AppearArtifact>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { }); connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { }); connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "travel-gate-check" }); Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 1600);

        Reset(); connection.Send(new Warp { Tile = new Point2(1, 1) }); Pump(connection, 350); AssertDisabled("Warp");
        Reset(); connection.Send(default(WarpBack)); Pump(connection, 350); AssertDisabled("WarpBack");
        Reset(); connection.Send(default(WarpToPort)); Pump(connection, 350); AssertDisabled("WarpToPort");
        Reset(); connection.Send(new IsWarpholeAvailable { EntityId = "missing-warphole" }); Pump(connection, 350); AssertDisabled("IsWarpholeAvailable");
        Reset(); connection.Send(default(GetWarpCosts)); Pump(connection, 350); AssertDisabled("GetWarpCosts");
        Reset(); connection.Send(default(GetWarpBackCost)); Pump(connection, 350); AssertDisabled("GetWarpBackCost");
        Reset(); connection.Send(new DepartTutorial { EntityId = id }); Pump(connection, 350); AssertDisabled("DepartTutorial");
        Reset(); connection.Send(new DepartTutorialFor { TargetRegionId = "mainland" }); Pump(connection, 350); AssertDisabled("DepartTutorialFor");

        Reset(); connection.Send(new Cheat { _Cheat = "travel island-does-not-matter" }); Pump(connection, 350);
        string cheatInfo; lock (Infos) { cheatInfo = string.Join("\n", Infos); }
        Check("Cheat travel is disabled without handoff", cheatInfo.Contains("การเดินทางข้ามเกาะยังปิดอยู่") && _emigrated == 0 && !cheatInfo.Contains("##goto"), cheatInfo);

        Console.WriteLine($"=== island travel gate result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
