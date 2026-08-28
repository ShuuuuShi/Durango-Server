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
/// เทสระบบจัดการ POI (`cheat poi …`) และ**ตรวจว่า POI ในโลกวางถูกที่จริง**
///
/// สองอย่างที่กันไว้:
///   1. ท่าเรือ/หลุมวาร์ปต้องไม่จมน้ำ ไม่โดนหินทับ ท่าเรือต้องติดน้ำ
///      (เคยพังเพราะ IsLand/WaterDepthAt คืนค่ามั่ว — ตอนนี้ใช้ LandDistance)
///   2. คำสั่ง move/here/remove/add ต้องทำงานสด ๆ ไม่ต้องหยุดเซิร์ฟแก้ world.json
///
/// ⚠️ ต้องรันเซิร์ฟด้วย --enable-cheat
/// </summary>
public static class PoiCheck
{
    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        string id = "poi-check-" + Guid.NewGuid().ToString("N")[..8];
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("[FAIL] ขอ session ไม่ได้"); return 1; }

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);
        var infos = new List<string>();
        int aborts = 0;
        int oks = 0;
        ExploredPOIs? explored = null;
        SearchedPOIs? searched = null;
        connection.Recv<Info>((m, h) => { lock (infos) { infos.Add(m.Text ?? ""); } });
        connection.Recv<Abort>((m, h) => aborts++);
        connection.Recv<Welcome>((m, h) => { }); connection.Recv<Clock>((m, h) => { });
        connection.Recv<OK>((m, h) => oks++);
        connection.Recv<ExploredPOIs>((m, h) => explored = m);
        connection.Recv<SearchedPOIs>((m, h) => searched = m); connection.Recv<Inventory>((m, h) => { });
        connection.Recv<Skills>((m, h) => { }); connection.Recv<Statistics>((m, h) => { });
        connection.Recv<Equipments>((m, h) => { }); connection.Recv<Survival>((m, h) => { });
        connection.Recv<Points>((m, h) => { }); connection.Recv<AppearPlayer>((m, h) => { });
        connection.Recv<AppearAnimal>((m, h) => { }); connection.Recv<AppearArtifact>((m, h) => { });
        connection.Recv<Move>((m, h) => { }); connection.Recv<DefoggedChunks>((m, h) => { });
        connection.Recv<QuestCategories>((m, h) => { }); connection.Recv<WalletUpdated>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { }); connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Chunk>((m, h) => { }); connection.Recv<Teleported>((m, h) => { });
        connection.Recv<DisappearEntity>((m, h) => { });
        connection.StartReceive();
        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "poi-check" });
        Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 1500);

        string Cheat(string command, int waitMs = 700)
        {
            lock (infos) { infos.Clear(); }
            connection.Send(new Cheat { _Cheat = command });
            Pump(connection, waitMs);
            lock (infos) { return infos.Count > 0 ? string.Join("\n", infos) : "(ไม่มีคำตอบ)"; }
        }

        int failures = 0;
        void Check(string what, bool ok, string detail = null)
        {
            Console.WriteLine($"  [{(ok ? "ผ่าน" : "ตก ")}] {what}{(detail == null ? "" : " — " + detail)}");
            if (!ok) failures++;
        }

        Check("เข้าเกมได้ (ไม่มี Abort)", aborts == 0, "aborts=" + aborts);

        // ── 1. packet discovery + persistence ─────────────────────────────
        connection.Send(new GetExploredPOIs { RegionId = "poi-check" });
        Pump(connection, 350);
        Check("ผู้เล่นใหม่ยังไม่มี POI ที่ค้นพบ", explored.HasValue && (explored.Value.POIs?.Length ?? 0) == 0,
            !explored.HasValue ? "ไม่มี response" : "count=" + (explored.Value.POIs?.Length ?? 0));
        Check("response รายการ POI มี contract เริ่มต้นถูกต้อง",
            explored.HasValue && explored.Value.IsOpenedMap && !explored.Value.FullCountRewarded && explored.Value.RewardCost == null);

        // ── 2. รายการ POI ────────────────────────────────────────────────
        string list = Cheat("poi list");
        Console.WriteLine("--- cheat poi list ---");
        Console.WriteLine(list);
        Console.WriteLine("----------------------");
        bool hasList = list.Contains("tile ") && !list.Contains("ยังไม่มี POI");
        bool hasCampWarphole = list.Contains("camp_warphole") || list.Contains("near_camp_warphole");
        if (!hasList || !hasCampWarphole)
        {
            // A production fresh world may intentionally contain only its dock. Create the
            // warphole as a controlled POI fixture before exercising discovery/persistence.
            string added = Cheat("poi add camp_warphole 100 100");
            Check("fresh world creates the camp warphole test fixture", added.Contains("วาง camp_warphole") && added.Contains("[ok]"), added.Trim());
            list = Cheat("poi list");
            hasList = list.Contains("tile ") && !list.Contains("ยังไม่มี POI");
            hasCampWarphole = list.Contains("camp_warphole") || list.Contains("near_camp_warphole");
        }
        Check("`poi list` คืนรายการ POI", hasList);

        // ── 3. ทุกชิ้นต้องวางถูกที่ ──────────────────────────────────────
        string check = Cheat("poi check");
        bool allOk = check.Contains("วางถูกที่");
        Check("POI ทุกชิ้นวางถูกที่ (ไม่จมน้ำ/ไม่โดนหินทับ/ท่าเรือติดน้ำ)", allOk,
              allOk ? null : "\n" + check);

        // ── 3. ท่าเรือใกล้จุดเกิดต้องมีจริง ──────────────────────────────
        Check("มี POI ที่ใช้ทดสอบในโลก", hasList);
        Check("world มีหลุมวาร์ปสำหรับ test", hasCampWarphole);

        // ── 4. ย้ายแล้วย้ายกลับ — ต้องมีผลจริงและรายงานตรง ────────────────
        (string id, int x, int y)? target = FirstPoi(list);
        if (target == null)
        {
            Check("หา POI สักชิ้นมาเทสย้าย", false);
        }
        else
        {
            var t = target.Value;
            string moved = Cheat($"poi move {t.id} {t.x + 1} {t.y}");
            Check("`poi move` ย้ายได้", moved.Contains($"tile {t.x + 1},{t.y}"), moved.Trim());

            string after = Cheat("poi list");
            Check("รายการสะท้อนตำแหน่งใหม่", after.Contains($"tile {t.x + 1},{t.y}"));

            string back = Cheat($"poi move {t.id} {t.x} {t.y}");
            Check("ย้ายกลับที่เดิมได้", back.Contains($"tile {t.x},{t.y}"), back.Trim());

            string tp = Cheat($"poi tp {t.id}");
            Check("`poi tp` วาร์ปไปดูได้", tp.Contains("วาร์ป"), tp.Trim());
        }

        // ── 5. คำสั่งที่ควรถูกปฏิเสธ ─────────────────────────────────────
        string bogus = Cheat("poi move ไม่มีจริง 10 10");
        Check("id ที่ไม่มีอยู่ ต้องถูกปฏิเสธ", bogus.Contains("ไม่เจอ"), bogus.Trim());

        string oob = target == null ? "" : Cheat($"poi move {target.Value.id} -5 -5");
        Check("ย้ายออกนอกแผนที่ ต้องถูกปฏิเสธ", target == null || oob.Contains("นอกแผนที่"), oob.Trim());

        // ── 6. packet discovery ต้อง persist ข้าม reconnect ──────────────
        if (target == null)
        {
            Check("มี POI สำหรับ packet discovery", false);
        }
        else
        {
            Cheat($"poi tp {target.Value.id}", 500);
            searched = null;
            connection.Send(default(SearchPOIs));
            Pump(connection, 500);
            SearchResult[] results = searched.HasValue ? (searched.Value.Results ?? Array.Empty<SearchResult>()) : Array.Empty<SearchResult>();
            SearchResult? found = null;
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].Tile.x == target.Value.x && results[i].Tile.y == target.Value.y
                    && results[i].Type != Shared.System.PointOfInterest.Invalid)
                {
                    found = results[i];
                    break;
                }
            }
            Check("SearchPOIs คืน POI ที่อยู่ใกล้ผู้เล่น", found.HasValue, "results=" + results.Length);
            if (found.HasValue)
            {
                int beforeOk = oks;
                int beforeAbort = aborts;
                connection.Send(new ExplorePOI { Tile = found.Value.Tile, Type = found.Value.Type });
                Pump(connection, 350);
                Check("ExplorePOI ที่ server ยืนยันแล้วสำเร็จ", oks == beforeOk + 1 && aborts == beforeAbort,
                    $"ok={oks - beforeOk} abort={aborts - beforeAbort}");

                explored = null;
                connection.Send(new GetExploredPOIs { RegionId = "poi-check" });
                Pump(connection, 350);
                Messages.PointOfInterest saved = FindExplored(explored, found.Value);
                Check("GetExploredPOIs คืน tile/type ที่เพิ่งค้นพบ",
                    saved.Tile == found.Value.Tile && saved.Type == found.Value.Type && saved.IsExplored,
                    saved.Tile.x + "," + saved.Tile.y + " type=" + saved.Type);
            }
        }

        // Disconnect saves the player. A fresh token and connection must restore the explored POI.
        connection.Close();
        Thread.Sleep(900);
        string reconnectToken = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(reconnectToken))
        {
            Check("ขอ session ใหม่สำหรับ reconnect", false);
        }
        else
        {
            using var reconnectSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            reconnectSocket.Connect(host, gamePort);
            var reconnect = new Connection(reconnectSocket);
            ExploredPOIs? restored = null;
            reconnect.Recv<Welcome>((m, h) => { }); reconnect.Recv<Clock>((m, h) => { });
            reconnect.Recv<ExploredPOIs>((m, h) => restored = m);
            reconnect.Recv<Info>((m, h) => { }); reconnect.Recv<Abort>((m, h) => { }); reconnect.Recv<OK>((m, h) => { });
            reconnect.Recv<Inventory>((m, h) => { }); reconnect.Recv<Skills>((m, h) => { }); reconnect.Recv<Statistics>((m, h) => { });
            reconnect.Recv<Equipments>((m, h) => { }); reconnect.Recv<Survival>((m, h) => { }); reconnect.Recv<Points>((m, h) => { });
            reconnect.Recv<AppearPlayer>((m, h) => { }); reconnect.Recv<AppearAnimal>((m, h) => { }); reconnect.Recv<AppearArtifact>((m, h) => { });
            reconnect.Recv<Move>((m, h) => { }); reconnect.Recv<Teleported>((m, h) => { }); reconnect.Recv<DefoggedChunks>((m, h) => { });
            reconnect.Recv<QuestCategories>((m, h) => { }); reconnect.Recv<WalletUpdated>((m, h) => { }); reconnect.Recv<Recipes>((m, h) => { });
            reconnect.Recv<ArtifactBlueprints>((m, h) => { }); reconnect.Recv<Chunk>((m, h) => { });
            reconnect.StartReceive();
            reconnect.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(reconnect, 250);
            reconnect.Send(new Auth { EntityId = id, SessionToken = reconnectToken, ClientVersion = "5.2.1", DeviceModel = "poi-check" }); Pump(reconnect, 500);
            reconnect.Send(default(Ready)); Pump(reconnect, 1300);
            reconnect.Send(new GetExploredPOIs { RegionId = "poi-check" }); Pump(reconnect, 350);
            bool restoredTarget = target.HasValue && restored.HasValue && restored.Value.POIs != null
                && Array.Exists(restored.Value.POIs, poi => poi.Tile.x == target.Value.x && poi.Tile.y == target.Value.y && poi.IsExplored);
            Check("POI ที่ค้นพบยังอยู่หลัง reconnect", restoredTarget,
                restored.HasValue ? "count=" + (restored.Value.POIs?.Length ?? 0) : "ไม่มี response");
            reconnect.Close();
        }
        Console.WriteLine();
        Console.WriteLine(failures == 0
            ? "[PASS] poi-check ผ่านทุกข้อ"
            : $"[FAIL] poi-check ตก {failures} ข้อ");
        return failures == 0 ? 0 : 1;
    }

    private static Messages.PointOfInterest FindExplored(ExploredPOIs? explored, SearchResult target)
    {
        if (explored.HasValue && explored.Value.POIs != null)
        {
            for (int i = 0; i < explored.Value.POIs.Length; i++)
            {
                Messages.PointOfInterest poi = explored.Value.POIs[i];
                if (poi.Tile == target.Tile && poi.Type == target.Type)
                {
                    return poi;
                }
            }
        }
        return new Messages.PointOfInterest { Tile = new Point2(-1, -1), Type = Shared.System.PointOfInterest.Invalid };
    }

    /// <summary>ดึง id + พิกัดของ POI ชิ้นแรกจากผลลัพธ์ `poi list`</summary>
    private static (string id, int x, int y)? FirstPoi(string list)
    {
        foreach (string line in list.Split('\n'))
        {
            int at = line.IndexOf("tile ", StringComparison.Ordinal);
            if (at < 0) continue;
            string[] head = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (head.Length == 0) continue;
            string rest = line.Substring(at + 5).Trim();
            int space = rest.IndexOf(' ');
            string coord = space < 0 ? rest : rest.Substring(0, space);
            string[] xy = coord.Split(',');
            if (xy.Length != 2) continue;
            if (int.TryParse(xy[0], out int x) && int.TryParse(xy[1], out int y))
            {
                return (head[0], x, y);
            }
        }
        return null;
    }
}
