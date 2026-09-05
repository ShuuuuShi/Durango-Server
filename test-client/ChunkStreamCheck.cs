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
/// เทส "เดินไปแล้วโลกโหลดตามไหม" — จำลอง ChunkPool ของ client ต้นฉบับเป๊ะ ๆ
///
/// 🐛 บั๊กที่เทสนี้จับ: client ต้นฉบับ (retail) มี `_visibleRange = 1` ตายตัว
/// `ChunkPool.Load()` จะ **ทิ้ง chunk ที่อยู่นอกระยะ 1 ทันทีแบบเงียบ ๆ** (ไม่เข้า `_failedChunks`)
/// ถ้าเซิร์ฟส่งกว้างกว่านั้นแล้วจำว่า "ส่งไปแล้ว" พอเดินข้ามขอบ chunk มันจะข้ามการส่งซ้ำ
/// ⇒ ก้อนที่ client ทิ้งไปจะไม่มีวันกลับมา ⇒ เดินไปแล้วพื้นไม่โหลด
/// (แล้ว `IsEnoughChunkLoaded()` ที่ต้องครบ 9 ก้อนก็ไม่ผ่าน `IsLoadingChunks` ค้าง true ถาวร)
///
/// วิธีตรวจที่ไม่ต้องรู้ขนาดแมพ: เดินเป็นวงแล้ววนกลับจุดเดิม
/// ชุด chunk ที่ถืออยู่ตอนกลับมาต้องเท่ากับตอนเริ่มเป๊ะ ๆ
/// </summary>
public static class ChunkStreamCheck
{
    /// <summary>`_visibleRange` ของ client ต้นฉบับ (Durango.Terrain/TerrainBase.InitChunkPool)</summary>
    private const int ClientVisibleRange = 1;

    private static void Pump(Connection connection, int milliseconds)
    {
        for (int i = 0; i < milliseconds / 10; i++) { connection.Process(); Thread.Sleep(10); }
    }

    /// <summary>จำลอง ChunkPool ฝั่ง client: เก็บเฉพาะก้อนที่อยู่ในระยะ 1 รอบ center</summary>
    private sealed class FakeChunkPool
    {
        private readonly HashSet<(int x, int y)> _loaded = new HashSet<(int, int)>();
        public (int x, int y) Center { get; private set; }
        public int Dropped { get; private set; }

        /// <summary>ตรงกับ ChunkPool.IsVisibleChunk</summary>
        public bool IsVisible(int x, int y)
            => Math.Abs(x - Center.x) <= ClientVisibleRange && Math.Abs(y - Center.y) <= ClientVisibleRange;

        /// <summary>ตรงกับ ChunkPool.SetCenterChunkCoords → ResetFarChunks</summary>
        public void SetCenter((int x, int y) center)
        {
            Center = center;
            _loaded.RemoveWhere(c => !IsVisible(c.x, c.y));
        }

        /// <summary>ตรงกับ ChunkPool.Load — นอกระยะ = ทิ้งเงียบ ๆ</summary>
        public void Receive(int x, int y)
        {
            if (!IsVisible(x, y)) { Dropped++; return; }
            _loaded.Add((x, y));
        }

        public HashSet<(int x, int y)> Snapshot() => new HashSet<(int, int)>(_loaded);
        public bool Has(int x, int y) => _loaded.Contains((x, y));
        public int Count => _loaded.Count;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        string modelInfo = "{\"hair\":\"Models/PC/Female/Hair/f_hair_long\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"],\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"C8A07A\",\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\",\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null,\"voice_type\":4,\"body_size\":1.2}";
        string entityId = CreateCharacterCheck.CreatePlayer(host, gatewayPort, "เทสโหลดแมพ", false, modelInfo);
        if (string.IsNullOrEmpty(entityId))
        {
            Console.WriteLine("[chunk-check] สร้างตัวละครไม่สำเร็จ — เช็ค gateway ก่อน");
            return 1;
        }
        string token = SessionClient.FetchRaw(host, gatewayPort, "{\"appear_player\":{\"entity_id\":\"" + entityId + "\"}}");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("[chunk-check] ขอ session token ไม่สำเร็จ");
            return 1;
        }

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(host, gamePort);
        var connection = new Connection(socket);

        var inbox = new List<(int x, int y)>();
        object inboxLock = new object();
        int aborts = 0;
        var selfPos = new WorldPosition(0f, 0f);
        bool sawSelf = false;

        connection.Recv<Abort>((m, h) => aborts++);
        connection.Recv<Chunk>((m, h) => { lock (inboxLock) { inbox.Add((m._Chunk.x, m._Chunk.y)); } });
        connection.Recv<AppearPlayer>((m, h) =>
        {
            if (sawSelf || m.EntityId != entityId) { return; }
            Location[] path = m.Move.Movements is { Length: > 0 } mv ? mv[0].Path : null;
            if (path is { Length: > 0 }) { selfPos = path[^1].Position; sawSelf = true; }
        });
        connection.Recv<Welcome>((m, h) => { }); connection.Recv<Clock>((m, h) => { });
        connection.Recv<OK>((m, h) => { }); connection.Recv<Inventory>((m, h) => { });
        connection.Recv<Skills>((m, h) => { }); connection.Recv<Statistics>((m, h) => { });
        connection.Recv<Equipments>((m, h) => { }); connection.Recv<Survival>((m, h) => { });
        connection.Recv<Points>((m, h) => { }); connection.Recv<AppearAnimal>((m, h) => { });
        connection.Recv<AppearArtifact>((m, h) => { }); connection.Recv<Move>((m, h) => { });
        connection.Recv<DefoggedChunks>((m, h) => { }); connection.Recv<QuestCategories>((m, h) => { });
        connection.Recv<WalletUpdated>((m, h) => { }); connection.Recv<ArtifactBlueprints>((m, h) => { });
        connection.Recv<Recipes>((m, h) => { });
        connection.StartReceive();

        connection.Send(new GetClock { Time = Times.UnixTimeNow() }); Pump(connection, 250);
        connection.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "chunk-check" });
        Pump(connection, 500);
        connection.Send(default(Ready)); Pump(connection, 2000);

        if (aborts > 0 || !sawSelf)
        {
            Console.WriteLine($"[chunk-check] เข้าโลกไม่สำเร็จ (aborts={aborts}, เห็นตัวเอง={sawSelf})");
            connection.Close();
            return 1;
        }

        // 1 chunk = 16 tile · 1 tile = 200 หน่วยโลก
        int startCx = (int)(selfPos.x / (16 * 200));
        int startCy = (int)(selfPos.y / (16 * 200));
        Console.WriteLine($"== เกิดที่ chunk {startCx},{startCy} (pos {selfPos.x:F0},{selfPos.y:F0}) ==");

        var pool = new FakeChunkPool();
        var everSent = new HashSet<(int x, int y)>();
        int passed = 0, failed = 0;

        void Check(string label, bool ok, string detail)
        {
            if (ok) { passed++; Console.WriteLine($"  ✅ {label}"); }
            else { failed++; Console.WriteLine($"  ❌ {label} — {detail}"); }
        }

        void Step(string label, int cx, int cy)
        {
            Console.WriteLine($"-- {label}: ไป chunk {cx},{cy} --");
            pool.SetCenter((cx, cy));
            connection.Send(new SetChunk { Chunk = new Point2(cx, cy) });
            Pump(connection, 900);
            List<(int x, int y)> arrived;
            lock (inboxLock) { arrived = new List<(int, int)>(inbox); inbox.Clear(); }
            foreach (var c in arrived) { everSent.Add(c); pool.Receive(c.x, c.y); }
            Console.WriteLine($"   เซิร์ฟส่งมา {arrived.Count} ก้อน · client เก็บไว้ {pool.Count} ก้อน · ทิ้ง {pool.Dropped} สะสม");

            // ก้อนที่เซิร์ฟเคยส่ง = อยู่ในแมพจริง ⇒ พออยู่ในระยะ 1 ต้องถืออยู่เสมอ
            var missing = everSent.Where(c => pool.IsVisible(c.x, c.y) && !pool.Has(c.x, c.y)).ToList();
            Check($"{label}: ก้อนในระยะที่เคยมีจริง ต้องโหลดครบ",
                missing.Count == 0,
                "ขาด " + string.Join(" ", missing.Select(c => $"{c.x},{c.y}")));

            // ตรงกับ ChunkPool.IsEnoughChunkLoaded() — ไม่ครบ 9 = IsLoadingChunks ค้าง true ตลอด
            Check($"{label}: โหลดครบ 9 ก้อน (IsEnoughChunkLoaded)",
                pool.Count >= 9,
                $"ได้แค่ {pool.Count} ก้อน ⇒ ฝั่งเกมจริง terrain จะค้างไม่อัปเดตอีกเลย");
        }

        Step("เข้าเกม", startCx, startCy);
        var atStart = pool.Snapshot();

        Step("เดินขวา", startCx + 1, startCy);
        Step("เดินลง", startCx + 1, startCy + 1);
        Step("เดินซ้าย", startCx, startCy + 1);
        Step("เดินกลับจุดเดิม", startCx, startCy);

        var atEnd = pool.Snapshot();
        Check("วนกลับจุดเดิมแล้วได้ chunk ชุดเดิมครบ",
            atStart.SetEquals(atEnd),
            $"เริ่ม {atStart.Count} ก้อน · กลับมาเหลือ {atEnd.Count} ก้อน · หาย " +
            string.Join(" ", atStart.Except(atEnd).Select(c => $"{c.x},{c.y}")));

        Check("ไม่มี Abort ระหว่างเทส", aborts == 0, $"aborts={aborts}");

        connection.Close();
        Console.WriteLine();
        Console.WriteLine($"== chunk-check: ผ่าน {passed} · ตก {failed} ==");
        if (pool.Dropped > 0)
        {
            Console.WriteLine($"⚠️ client ทิ้ง chunk ที่เซิร์ฟส่งมาเกินระยะไป {pool.Dropped} ก้อน " +
                              "— ตั้ง World.ChunkSendRange ให้เท่ากับ 1 เพื่อไม่ให้เปลืองแบนด์วิดท์");
        }
        return failed == 0 ? 0 : 1;
    }
}
