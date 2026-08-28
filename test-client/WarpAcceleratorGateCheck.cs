using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;

namespace DurangoTestClient;

/// <summary>Regression: disabled Warp Accelerator requests must explain and abort before manager state changes.</summary>
public static class WarpAcceleratorGateCheck
{
    private static int _passed;
    private static int _failed;
    private static readonly List<string> Infos = new List<string>();
    private static int _aborts, _oks, _artifacts, _acquisitions, _costs;

    private static void Pump(Connection c, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { c.Process(); Thread.Sleep(10); }
    }

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static void Reset()
    {
        lock (Infos) { Infos.Clear(); }
        _aborts = _oks = _artifacts = _acquisitions = _costs = 0;
    }

    private static void AssertDisabled(string action)
    {
        string text; lock (Infos) { text = string.Join("\n", Infos); }
        Check(action + " returns Info + Abort", _aborts == 1 && text.Contains("วาร์ปเรกเซเลอเรเตอร์ยังไม่เปิด"),
            $"info={text.Replace('\n', '|')} abort={_aborts}");
        Check(action + " does not start accelerator state", _oks == 0 && _artifacts == 0 && _acquisitions == 0 && _costs == 0,
            $"ok={_oks} artifact={_artifacts} acquisition={_acquisitions} cost={_costs}");
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        string id = "accel-gate-" + Guid.NewGuid().ToString("N")[..8];
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("[FAIL] ขอ session ไม่ได้"); return 2; }
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var c = new Connection(socket);
        c.Recv<Welcome>((m, h) => { }); c.Recv<Clock>((m, h) => { });
        c.Recv<Info>((m, h) => { lock (Infos) { Infos.Add(m.Text ?? ""); } });
        c.Recv<Abort>((m, h) => _aborts++); c.Recv<OK>((m, h) => _oks++);
        c.Recv<AppearArtifact>((m, h) => _artifacts++); c.Recv<WarpAcceleratorAcquisition>((m, h) => _acquisitions++);
        c.Recv<Cost>((m, h) => _costs++);
        c.Recv<Inventory>((m, h) => { }); c.Recv<Skills>((m, h) => { }); c.Recv<Statistics>((m, h) => { });
        c.Recv<Equipments>((m, h) => { }); c.Recv<Survival>((m, h) => { }); c.Recv<Points>((m, h) => { });
        c.Recv<AppearPlayer>((m, h) => { }); c.Recv<AppearAnimal>((m, h) => { }); c.Recv<Move>((m, h) => { });
        c.Recv<Teleported>((m, h) => { }); c.Recv<DefoggedChunks>((m, h) => { }); c.Recv<QuestCategories>((m, h) => { });
        c.Recv<WalletUpdated>((m, h) => { }); c.Recv<Recipes>((m, h) => { }); c.Recv<ArtifactBlueprints>((m, h) => { }); c.Recv<Chunk>((m, h) => { });
        c.StartReceive();
        c.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(c, 250);
        c.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "accel-gate-check" }); Pump(c, 500);
        c.Send(default(Ready)); Pump(c, 1400);

        const string fake = "disabled-accelerator";
        Reset(); c.Send(new Accelerate { EntityId = fake, Tile = new Point2(0, 0) }); Pump(c, 350); AssertDisabled("Accelerate");
        Reset(); c.Send(new ParticipateAcceleration { EntityId = fake, Tile = new Point2(0, 0) }); Pump(c, 350); AssertDisabled("ParticipateAcceleration");
        Reset(); c.Send(new ReceiveAcceleratorRewards { EntityId = fake, Tile = new Point2(0, 0) }); Pump(c, 350); AssertDisabled("ReceiveAcceleratorRewards");
        Reset(); c.Send(default(GetWarpAcceleratorCost)); Pump(c, 350);
        string queryInfo; lock (Infos) { queryInfo = string.Join("\n", Infos); }
        Check("cost query remains available while disabled", _costs == 1 && _aborts == 0 && _oks == 0 && _artifacts == 0 && _acquisitions == 0 && string.IsNullOrEmpty(queryInfo),
            $"cost={_costs} abort={_aborts} info={queryInfo}");

        Console.WriteLine($"=== warp accelerator gate result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
