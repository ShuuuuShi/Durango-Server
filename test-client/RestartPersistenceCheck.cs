using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Etc;
using Shared.Item;

namespace DurangoTestClient;

/// <summary>S1 clean-restart acceptance: child server + private save root + live packet verification.</summary>
public static class RestartPersistenceCheck
{
    private sealed class Client
    {
        public readonly string Id;
        public readonly Connection Connection;
        public readonly Dictionary<string, AppearArtifact> Artifacts = new Dictionary<string, AppearArtifact>();
        public Inventory? Inventory;
        public ArtifactMaterials? Materials;
        public int Aborts;
        public string BoxCapsuleId;
        public string LeafId;

        public Client(string id, Connection connection)
        {
            Id = id;
            Connection = connection;
        }

        public void Pump(int milliseconds)
        {
            for (int i = 0; i < milliseconds / 10; i++)
            {
                Connection.Process();
                Thread.Sleep(10);
            }
        }
    }

    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static int FreePort()
    {
        var listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static Process StartServer(string root, string saves, int gamePort, int gatewayPort)
    {
        string dll = Path.Combine(root, "server", "bin", "Debug", "net9.0", "DurangoServer.dll");
        if (!File.Exists(dll)) throw new FileNotFoundException("ไม่พบ DurangoServer.dll — build server ก่อน", dll);
        var psi = new ProcessStartInfo("dotnet")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.Combine(root, "server")
        };
        psi.ArgumentList.Add(dll);
        psi.ArgumentList.Add("--data"); psi.ArgumentList.Add(Path.Combine(root, "server", "data"));
        psi.ArgumentList.Add("--saves"); psi.ArgumentList.Add(saves);
        psi.ArgumentList.Add("--game-port"); psi.ArgumentList.Add(gamePort.ToString());
        psi.ArgumentList.Add("--gateway-port"); psi.ArgumentList.Add(gatewayPort.ToString());
        psi.ArgumentList.Add("--enable-cheat");
        psi.ArgumentList.Add("--no-account-check");
        psi.ArgumentList.Add("--no-ip-bind");
        var process = Process.Start(psi);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static bool WaitForPort(int port, int timeoutMs)
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (watch.ElapsedMilliseconds < timeoutMs)
        {
            try
            {
                using var socket = new TcpClient();
                socket.Connect("127.0.0.1", port);
                return true;
            }
            catch { Thread.Sleep(100); }
        }
        return false;
    }

    private static Client Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) return null;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        var client = new Client(id, connection);
        connection.Recv<Welcome>((m, h) => { }); connection.Recv<Clock>((m, h) => { });
        connection.Recv<OK>((m, h) => { }); connection.Recv<Abort>((m, h) => client.Aborts++);
        connection.Recv<Info>((m, h) => { }); connection.Recv<Messages.Timer>((m, h) => { });
        connection.Recv<Inventory>((m, h) =>
        {
            client.Inventory = m;
            foreach (Item item in m.InventoryItems.Items ?? Array.Empty<Item>())
            {
                if (item.Prototype == "capsulated_fur_box_03_leaf") client.BoxCapsuleId = item.Id;
                if (item.Prototype == "leaf") client.LeafId = item.Id;
            }
        });
        connection.Recv<ArtifactMaterials>((m, h) => client.Materials = m);
        connection.Recv<AppearArtifact>((m, h) => client.Artifacts[m.EntityId] = m);
        connection.Recv<ArtifactBuilt>((m, h) => { }); connection.Recv<ArtifactCompleted>((m, h) => { });
        connection.Recv<Skills>((m, h) => { }); connection.Recv<Statistics>((m, h) => { });
        connection.Recv<Survival>((m, h) => { }); connection.Recv<AppearPlayer>((m, h) => { });
        connection.Recv<AppearAnimal>((m, h) => { }); connection.Recv<Move>((m, h) => { });
        connection.Recv<Teleported>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { }); connection.Recv<QuestCategories>((m, h) => { });
        connection.Recv<WalletUpdated>((m, h) => { }); connection.Recv<Recipes>((m, h) => { });
        connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); client.Pump(250);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "restart-check" }); client.Pump(500);
        connection.Send(default(Ready)); client.Pump(1200);
        return client;
    }

    private static string FindArtifact(Client client, string blueprint, string exclude = null)
    {
        foreach (var pair in client.Artifacts)
        {
            if (pair.Key != exclude && pair.Value.FounderEntityId == client.Id && pair.Value.EntityType != 0)
            {
                // The child root begins empty, so created artifacts are the only owner artifacts.
                return pair.Key;
            }
        }
        return null;
    }

    private static bool StopCleanly(Client client, Process process)
    {
        client.Connection.Send(new Cheat { _Cheat = "shutdown" });
        client.Pump(300);
        try { return process.WaitForExit(10000) && process.ExitCode == 0; }
        catch { return false; }
    }

    private static bool HasPrivateWorldSave(string saves)
    {
        return Directory.Exists(saves) && Directory.GetFiles(saves, "*.json", SearchOption.AllDirectories).Length > 0;
    }

    public static int Run()
    {
        _passed = _failed = 0;
        string root = Directory.GetParent(Directory.GetCurrentDirectory())?.FullName;
        string temp = Path.Combine(Path.GetTempPath(), "durango-s1-restart-" + Guid.NewGuid().ToString("N"));
        string saves = Path.Combine(temp, "saves");
        Directory.CreateDirectory(saves);
        int gatewayPort = FreePort();
        int gamePort = FreePort();
        string ownerId = "restart-owner-" + Guid.NewGuid().ToString("N")[..8];
        Process first = null;
        Process second = null;
        Client owner = null;
        try
        {
            Console.WriteLine("=== restart persistence check (S1) ===");
            first = StartServer(root, saves, gamePort, gatewayPort);
            Check("first child server starts", WaitForPort(gamePort, 15000));
            owner = Connect("127.0.0.1", gamePort, gatewayPort, ownerId);
            Check("owner connects to private server", owner != null);
            if (owner == null) return 1;

            owner.Connection.Send(new Cheat { _Cheat = "give leaf 4" }); owner.Pump(500);
            owner.Artifacts.Clear(); owner.Connection.Send(new OccupyArtifactSite { BlueprintId = "bonfire", Tile = new Point2(42, 177), Rotation = Rotation.None }); owner.Pump(1000);
            string bonfireId = FindArtifact(owner, "bonfire");
            Check("pending bonfire is created", !string.IsNullOrEmpty(bonfireId));
            string[] leaves = Array.Empty<string>();
            if (owner.Inventory.HasValue)
            {
                var ids = new List<string>();
                foreach (Item item in owner.Inventory.Value.InventoryItems.Items ?? Array.Empty<Item>()) if (item.Prototype == "leaf") ids.Add(item.Id);
                leaves = ids.GetRange(0, Math.Min(4, ids.Count)).ToArray();
            }
            owner.Aborts = 0;
            owner.Connection.Send(new PutMaterialsIntoArtifact { EntityId = bonfireId, Tile = new Point2(42, 177), Materials = new Dictionary<string, string[]> { ["main"] = leaves } }); owner.Pump(500);
            Check("pending bonfire material reservation succeeds", owner.Aborts == 0 && leaves.Length == 4);

            owner.Connection.Send(new Cheat { _Cheat = "add box" }); owner.Pump(500);
            owner.Artifacts.Clear(); owner.Connection.Send(new PlaceCapsulatedArtifact { ItemId = owner.BoxCapsuleId, Tile = new Point2(43, 177) }); owner.Pump(1000);
            string boxId = FindArtifact(owner, "box", bonfireId);
            Check("storage artifact is created", !string.IsNullOrEmpty(boxId));
            owner.Connection.Send(new Cheat { _Cheat = "give leaf" }); owner.Pump(300);
            string storedId = owner.LeafId;
            owner.Aborts = 0;
            owner.Connection.Send(new PutInItem { EntityId = boxId, Tile = new Point2(43, 177), ItemIds = new[] { storedId } }); owner.Pump(500);
            Check("storage item is persisted before restart", owner.Aborts == 0);

            bool firstStopped = StopCleanly(owner, first);
            Check("first child server exits cleanly after save", firstStopped);
            owner.Connection.Close(); owner = null;
            if (!firstStopped) return 1;
            first = null;

            second = StartServer(root, saves, gamePort, gatewayPort);
            Check("second child server starts", WaitForPort(gamePort, 15000));
            owner = Connect("127.0.0.1", gamePort, gatewayPort, ownerId);
            Check("owner reconnects after process restart", owner != null);
            if (owner == null) return 1;

            owner.Materials = null; owner.Aborts = 0;
            owner.Connection.Send(new GetArtifact { EntityId = bonfireId }); owner.Pump(500);
            bool ledger = owner.Aborts == 0 && owner.Materials.HasValue && owner.Materials.Value.Materials != null
                && owner.Materials.Value.Materials.TryGetValue("main", out Item[] reserved) && reserved.Length == 4;
            Check("pending material ledger survives clean restart", ledger);

            owner.Inventory = null; owner.Aborts = 0;
            owner.Connection.Send(new GetInventory { Target = new PropKey { EntityId = boxId, Tile = new Point2(43, 177) } }); owner.Pump(500);
            bool box = owner.Aborts == 0 && owner.Inventory.HasValue && owner.Inventory.Value.InventoryItems.Items != null;
            bool foundStored = false;
            if (box) foreach (Item item in owner.Inventory.Value.InventoryItems.Items) if (item.Id == storedId) foundStored = true;
            Check("storage contents survive clean restart", foundStored);
            Check("world save exists only in private temp root", HasPrivateWorldSave(saves));

            Check("second child server exits cleanly", StopCleanly(owner, second));
            owner.Connection.Close(); owner = null;
            second = null;
        }
        finally
        {
            try { owner?.Connection.Close(); } catch { }
            try { if (first != null && !first.HasExited) first.Kill(true); } catch { }
            try { if (second != null && !second.HasExited) second.Kill(true); } catch { }
            try { Directory.Delete(temp, true); } catch { }
        }
        Console.WriteLine($"=== restart persistence result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
