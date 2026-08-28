using System;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Shared.Item;

namespace DurangoTestClient;

/// <summary>S1: ตรวจสิทธิ์การเปิดกล่องและการแย่งหยิบของจากกล่องเดียวกัน</summary>
public static class StorageWorkbenchCheck
{
    private sealed class Client
    {
        public string Id;
        public Connection Connection;
        public int Aborts;
        public int Oks;
        public Inventory? Inventory;
        public string BoxCapsuleId;
        public string LeafId;
        public string LastArtifactId;
        public int BoxItemUpdates;
        public readonly System.Collections.Generic.HashSet<string> ReceivedItemIds = new System.Collections.Generic.HashSet<string>();

        public void Pump(int ms)
        {
            for (int i = 0; i < ms / 10; i++) { Connection.Process(); Thread.Sleep(10); }
        }
    }

    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [PASS] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [FAIL] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static Client Connect(string host, int gamePort, int gatewayPort, string id)
    {
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) return null;
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var client = new Client { Id = id, Connection = new Connection(socket) };
        client.Connection.Recv<Welcome>((m, h) => { });
        client.Connection.Recv<Clock>((m, h) => { });
        client.Connection.Recv<Abort>((m, h) => client.Aborts++);
        client.Connection.Recv<OK>((m, h) => client.Oks++);
        client.Connection.Recv<Info>((m, h) => { });
        client.Connection.Recv<Inventory>((m, h) =>
        {
            client.Inventory = m;
            foreach (Item item in m.InventoryItems.Items ?? Array.Empty<Item>())
            {
                if (item.Prototype == "capsulated_fur_box_03_leaf") client.BoxCapsuleId = item.Id;
                if (item.Prototype == "leaf") client.LeafId = item.Id;
            }
        });
        client.Connection.Recv<InventoryUpdated>((m, h) =>
        {
            if (!string.IsNullOrEmpty(m.EntityId)) client.BoxItemUpdates++;
            if (m.EntityId == client.Id)
                foreach (Item item in m.Items ?? Array.Empty<Item>()) client.ReceivedItemIds.Add(item.Id);
        });
        client.Connection.Recv<AppearArtifact>((m, h) => client.LastArtifactId = m.EntityId);
        client.Connection.Recv<AppearPlayer>((m, h) => { }); client.Connection.Recv<AppearAnimal>((m, h) => { });
        client.Connection.Recv<Skills>((m, h) => { }); client.Connection.Recv<Statistics>((m, h) => { });
        client.Connection.Recv<Survival>((m, h) => { }); client.Connection.Recv<Move>((m, h) => { });
        client.Connection.Recv<DefoggedChunks>((m, h) => { }); client.Connection.Recv<Chunk>((m, h) => { });
        client.Connection.Recv<QuestCategories>((m, h) => { }); client.Connection.Recv<WalletUpdated>((m, h) => { });
        client.Connection.StartReceive();
        client.Connection.Send(new GetClock { Time = Times.UnixTimeNow() }); client.Pump(200);
        client.Connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "storage-check" }); client.Pump(500);
        client.Connection.Send(default(Ready)); client.Pump(1200);
        return client;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        var owner = Connect(host, gamePort, gatewayPort, "storage-owner-" + Guid.NewGuid().ToString("N")[..8]);
        var other = Connect(host, gamePort, gatewayPort, "storage-other-" + Guid.NewGuid().ToString("N")[..8]);
        if (owner == null || other == null) return 2;

        owner.Connection.Send(new Cheat { _Cheat = "add box" }); owner.Pump(500);
        Check("owner receives a storage capsule", !string.IsNullOrEmpty(owner.BoxCapsuleId));
        owner.LastArtifactId = null;
        owner.Connection.Send(new PlaceCapsulatedArtifact { ItemId = owner.BoxCapsuleId, Tile = new Point2(41, 175) }); owner.Pump(1000);
        string boxId = owner.LastArtifactId;
        Check("owner places a storage artifact", !string.IsNullOrEmpty(boxId));

        if (!string.IsNullOrEmpty(boxId))
        {
            other.Aborts = 0;
            other.Connection.Send(new GetInventory { Target = new PropKey { EntityId = boxId, Tile = new Point2(41, 175) } }); other.Pump(400);
            Check("other player cannot open or read owner's box", other.Aborts > 0, "abort=" + other.Aborts);

            owner.Connection.Send(new Cheat { _Cheat = "give leaf" }); owner.Pump(300);
            owner.Aborts = owner.Oks = 0;
            owner.Connection.Send(new PutInItem { EntityId = boxId, Tile = new Point2(41, 175), ItemIds = new[] { owner.LeafId } }); owner.Pump(500);
            Check("owner can deposit an item", owner.Oks == 0 && owner.Aborts == 0);

            other.Aborts = 0;
            other.Connection.Send(new TakeOutItem { EntityId = boxId, Tile = new Point2(41, 175), ItemIds = new[] { owner.LeafId } }); other.Pump(400);
            Check("other player cannot withdraw owner's item", other.Aborts > 0, "abort=" + other.Aborts);

            owner.Oks = owner.Aborts = 0;
            owner.Connection.Send(new TakeOutItem { EntityId = boxId, Tile = new Point2(41, 175), ItemIds = new[] { owner.LeafId } }); owner.Pump(500);
            Check("owner withdraws the stored item exactly once", owner.Oks > 0 && owner.Aborts == 0, "ok=" + owner.Oks + " abort=" + owner.Aborts);

            // Grant the second client shared authority, then race for the same item ID.
            owner.Connection.Send(new Cheat { _Cheat = "architect add " + boxId + " " + other.Id }); owner.Pump(500);
            other.Aborts = 0;
            other.Connection.Send(new GetInventory { Target = new PropKey { EntityId = boxId, Tile = new Point2(41, 175) } }); other.Pump(400);
            Check("granted architect can read the shared box", other.Aborts == 0);

            owner.Connection.Send(new Cheat { _Cheat = "give leaf" }); owner.Pump(300);
            owner.Connection.Send(default(GetInventory)); owner.Pump(300);
            string raceItemId = owner.LeafId;
            owner.Aborts = 0;
            owner.Connection.Send(new PutInItem { EntityId = boxId, Tile = new Point2(41, 175), ItemIds = new[] { raceItemId } }); owner.Pump(500);
            Check("owner deposits the race item", owner.Aborts == 0);

            owner.Aborts = owner.Oks = 0; other.Aborts = other.Oks = 0;
            owner.ReceivedItemIds.Clear(); other.ReceivedItemIds.Clear();
            owner.Connection.Send(new TakeOutItem { EntityId = boxId, Tile = new Point2(41, 175), ItemIds = new[] { raceItemId } });
            other.Connection.Send(new TakeOutItem { EntityId = boxId, Tile = new Point2(41, 175), ItemIds = new[] { raceItemId } });
            for (int i = 0; i < 80; i++) { owner.Connection.Process(); other.Connection.Process(); Thread.Sleep(10); }

            int recipients = (owner.ReceivedItemIds.Contains(raceItemId) ? 1 : 0) + (other.ReceivedItemIds.Contains(raceItemId) ? 1 : 0);
            Check("two architects receive the same item exactly once", recipients == 1, "recipients=" + recipients);
            Check("two-client race has one success and one abort", owner.Oks + other.Oks == 1 && owner.Aborts + other.Aborts == 1,
                "ok=" + owner.Oks + "+" + other.Oks + " abort=" + owner.Aborts + "+" + other.Aborts);
            Check("both clients remain connected after contention", owner.Connection.Connected() && other.Connection.Connected());
        }

        owner.Connection.Close(); other.Connection.Close();
        Console.WriteLine($"=== storage/workbench result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
