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
/// ตัวทดสอบ "client โกง" — ยิง packet ที่ผู้เล่นปกติยิงไม่ได้ แล้วดูว่า server ปฏิเสธไหม
/// ครอบคลุมบั๊ก GP-08 (คราฟต์ไม่เช็ควัตถุดิบ), GP-09 (เชื่อ Tile จาก client), GP-12 (Auth ปลอมได้)
///
/// รัน: dotnet run -- --gp-check [host] [port เกม] [port gateway]
/// คืน exit code 1 ถ้ามีข้อไหนไม่ผ่าน (เอาไปใส่สคริปต์เทสได้)
/// </summary>
public static class GpCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok)
        {
            _passed++;
            Console.WriteLine($"  [ผ่าน] {name}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}");
        }
    }

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++) { conn.Process(); Thread.Sleep(10); }
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== GP check: {host}:{gamePort} (gateway {gatewayPort}) ===");

        // ── GP-12: Auth ที่ไม่มี token / token มั่ว ต้องไม่ผ่าน ──────────────
        Console.WriteLine("GP-12 — Auth ปลอม");
        Check("token มั่วเข้าไม่ได้", !CanEnter(host, gamePort, "gp-check-hacker", "token-ที่คิดขึ้นเอง"));
        Check("ไม่มี token เข้าไม่ได้", !CanEnter(host, gamePort, "gp-check-hacker", null));

        string token = SessionClient.Fetch(host, gatewayPort, "gp-check-1", "gp-check-1");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("ขอ token ไม่ได้ — server ปิดอยู่หรือพอร์ต gateway ผิด");
            return 1;
        }
        Check("token จริงเข้าได้", CanEnter(host, gamePort, "gp-check-1", token));

        // ── ต่อจริงเพื่อเทสข้อที่เหลือ ────────────────────────────────────
        using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        var conn = new Connection(sock);

        int aborts = 0;
        int touched = 0;
        int crafted = 0;
        int collected = 0;
        var naturals = new Dictionary<(int x, int y), ushort>();
        var inventory = new List<Item>();

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => aborts++);
        Touched lastTouched = default;
        conn.Recv<Touched>((m, h) => { touched++; lastTouched = m; });
        conn.Recv<Crafted>((m, h) => crafted++);
        conn.Recv<Collected>((m, h) => collected++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Inventory>((m, h) =>
        {
            inventory.Clear();
            if (m.InventoryItems.Items != null) inventory.AddRange(m.InventoryItems.Items);
        });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) => { });
        var appearLevels = new Dictionary<string, int>();
        conn.Recv<AppearPlayer>((m, h) => appearLevels[m.EntityId ?? ""] = m.Level);
        // จำตำแหน่งสัตว์ไว้ใช้เทสระยะโจมตี (หน่วยโลก)
        var appearAnimals = new Dictionary<string, (float x, float y)>();
        string lastAppearAnimal = null;      // ตัวที่เพิ่งเกิด — ใช้กับ cheat spawn ตอนเทสแล่เนื้อ
        conn.Recv<AppearAnimal>((m, h) =>
        {
            lastAppearAnimal = m.EntityId;
            Movement[] ms = m.Move.Movements;
            if (ms == null || ms.Length == 0) return;
            Location[] path = ms[ms.Length - 1].Path;
            if (path == null || path.Length == 0) return;
            appearAnimals[m.EntityId ?? ""] = (path[path.Length - 1].Position.x, path[path.Length - 1].Position.y);
        });
        // จำตัวที่ตาย/หายไปแล้ว — จะได้ไม่เผลอเลือกซากมาเทส "แตะสัตว์เป็น ๆ"
        var goneAnimals = new HashSet<string>();
        conn.Recv<EntityDied>((m, h) => goneAnimals.Add(m.EntityId ?? ""));
        conn.Recv<DisappearEntity>((m, h) => { goneAnimals.Add(m.EntityId ?? ""); appearAnimals.Remove(m.EntityId ?? ""); });
        conn.Recv<EntityRevived>((m, h) => { });
        // สัตว์เดินตลอดเวลา ถ้าใช้ตำแหน่งตอน spawn อย่างเดียว พอถึงตอนเทสมันย้ายไปแล้ว
        //
        // + วัด "ระยะกระโดด" ด้วย: ทุกครั้งที่มี Move ใหม่ของสัตว์ตัวเดิม เอาจุดเริ่มของคำสั่งใหม่
        //   ไปเทียบกับตำแหน่งที่ควรจะอยู่ตามคำสั่งเก่า ณ เวลาเดียวกัน — ห่างกันมาก = client เห็นตัววาร์ป
        var lastPath = new Dictionary<string, Location[]>();
        var maxJump = new Dictionary<string, float>();
        var moveCount = new Dictionary<string, int>();
        conn.Recv<Move>((m, h) =>
        {
            string id = m.EntityId ?? "";
            Movement[] ms = m.Movements;
            if (ms == null || ms.Length == 0) return;
            Location[] path = ms[ms.Length - 1].Path;
            if (path == null || path.Length == 0) return;

            moveCount[id] = moveCount.TryGetValue(id, out int mc) ? mc + 1 : 1;
            if (lastPath.TryGetValue(id, out Location[] prev) && prev.Length >= 2)
            {
                double t = path[0].Time;
                double span = prev[1].Time - prev[0].Time;
                float f = span <= 0 ? 1f : (float)Math.Max(0.0, Math.Min(1.0, (t - prev[0].Time) / span));
                float ex = prev[0].Position.x + (prev[1].Position.x - prev[0].Position.x) * f;
                float ey = prev[0].Position.y + (prev[1].Position.y - prev[0].Position.y) * f;
                float jx = path[0].Position.x - ex, jy = path[0].Position.y - ey;
                float jump = (float)Math.Sqrt(jx * jx + jy * jy);
                if (!maxJump.TryGetValue(id, out float old2) || jump > old2) maxJump[id] = jump;
            }
            lastPath[id] = path;

            if (!appearAnimals.ContainsKey(id)) return;
            appearAnimals[id] = (path[path.Length - 1].Position.x, path[path.Length - 1].Position.y);
        });
        string otherArtifact = null;
        conn.Recv<AppearArtifact>((m, h) =>
        {
            if (m.FounderEntityId != "gp-check-1") otherArtifact = m.EntityId;
        });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.Recv<Info>((m, h) => { });
        conn.Recv<CollectibleChanged>((m, h) => { });
        conn.Recv<DisappearEntityOnTile>((m, h) => naturals.Remove((m.Tile.x, m.Tile.y)));
        conn.Recv<Chunk>((m, h) =>
        {
            byte[] g = m.Garden;
            if (g == null) return;
            for (int i = 0; i + 6 <= g.Length; i += 6)
            {
                naturals[(BitConverter.ToUInt16(g, i), BitConverter.ToUInt16(g, i + 2))] = BitConverter.ToUInt16(g, i + 4);
            }
        });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 300);
        conn.Send(new Auth { EntityId = "gp-check-1", SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 500);
        conn.Send(default(Ready));
        Pump(conn, 1500);

        // บอทใช้ชื่อเดิมทุกรอบ (gp-check-1) และเซฟถูกเก็บไว้ ⇒ ของสะสมทุกครั้งที่รัน
        // พอกระเป๋าเต็ม ข้อที่ต้อง "เก็บของ/แล่ซากได้จริง" จะตกทั้งที่โค้ดถูก
        // เททิ้งก่อนเริ่มเสมอ เพื่อให้ผลการทดสอบไม่ขึ้นกับว่าเคยรันมากี่รอบ
        conn.Send(new Cheat { _Cheat = "clearbag" });
        Pump(conn, 500);

        // ยืนแถวจุดเกิด (tile 40,177) แล้วขอ chunk เพื่อให้รู้ว่าของธรรมชาติอยู่ตรงไหนจริง ๆ
        float px = 8000f, py = 35400f;
        MoveTo(conn, "gp-check-1", px, py);
        conn.Send(new SetChunk { Chunk = new Point2(2, 11) });
        Pump(conn, 800);

        // ── GP-09: Touch/Collect ต้องอิงพื้นที่จริง ────────────────────────
        Console.WriteLine("GP-09 — โกงพิกัด");

        // tile ที่ไม่มีของธรรมชาติเลย (หาช่องว่างข้าง ๆ จุดเกิด)
        (int x, int y) empty = (40, 177);
        for (int d = 0; d < 40 && naturals.ContainsKey(empty); d++) empty = (40 + d, 177);
        aborts = 0; touched = 0;
        conn.Send(new Touch { EntityId = $"natural_{empty.x}_{empty.y}", EntityType = 12119, Tile = new Point2(empty.x, empty.y) });
        Pump(conn, 800);
        Check("แตะ tile ที่ไม่มีของธรรมชาติ ไม่ผ่าน", aborts > 0 && touched == 0, $"abort={aborts} touched={touched}");

        // ของธรรมชาติจริงแต่อยู่คนละมุมแมพ
        naturals.Clear();
        conn.Send(new SetChunk { Chunk = new Point2(6, 6) });
        Pump(conn, 800);
        (int x, int y) far = default;
        bool haveFar = false;
        foreach (var kv in naturals)
        {
            double dx = kv.Key.x - px / 200.0, dy = kv.Key.y - py / 200.0;
            if (dx * dx + dy * dy > 30 * 30) { far = kv.Key; haveFar = true; break; }
        }
        if (haveFar)
        {
            aborts = 0; touched = 0;
            conn.Send(new Touch { EntityId = $"natural_{far.x}_{far.y}", EntityType = naturals[far], Tile = new Point2(far.x, far.y) });
            Pump(conn, 800);
            Check("แตะของธรรมชาติที่อยู่ไกลเกินเอื้อม ไม่ผ่าน", aborts > 0 && touched == 0, $"abort={aborts} touched={touched}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] ไม่เจอของธรรมชาติที่อยู่ไกลพอจะเทสระยะเอื้อม");
        }

        // สั่งลบของธรรมชาติที่ไม่เคยแตะ
        aborts = 0;
        conn.Send(new DisappearEntityOnTile { EntityId = "natural_ที่ไม่มีจริง", Tile = new Point2(41, 178) });
        Pump(conn, 600);
        naturals.Clear();
        conn.Send(new SetChunk { Chunk = new Point2(2, 11) });
        Pump(conn, 800);
        Check("สั่งลบของธรรมชาติที่ไม่เคยแตะ ไม่มีผล", naturals.Count > 0, "chunk รอบจุดเกิดว่างเปล่าหลังสั่งลบ");

        // H-5: เปลี่ยน EntityId ไปเรื่อย ๆ บน tile เดิม ต้องไม่ได้ generator ชุดใหม่
        // (ต้องกลับมายืนที่บ้านและขอ chunk ใหม่ก่อน เพราะเทสก่อนหน้าไปขอ chunk อื่นแล้วล้างรายการทิ้ง)
        MoveTo(conn, "gp-check-1", px, py);
        naturals.Clear();
        conn.Send(new SetChunk { Chunk = new Point2(2, 11) });
        Pump(conn, 1000);
        (int x, int y)? real = null;
        foreach (var kv in naturals)
        {
            double dx = kv.Key.x - px / 200.0, dy = kv.Key.y - py / 200.0;
            if (dx * dx + dy * dy <= 12 * 12) { real = kv.Key; break; }
        }
        if (real != null)
        {
            WalkTo(conn, "gp-check-1", ref px, ref py, real.Value.x * 200f, real.Value.y * 200f);
            string idFromServer = null;
            conn.Recv<Touched>((m, h) => { touched++; idFromServer = m.EntityId; });

            touched = 0;
            conn.Send(new Touch { EntityId = "id-ที่คิดขึ้นเอง-1", EntityType = naturals[real.Value], Tile = new Point2(real.Value.x, real.Value.y) });
            Pump(conn, 800);
            string first = idFromServer;

            touched = 0;
            conn.Send(new Touch { EntityId = "id-ที่คิดขึ้นเอง-2", EntityType = naturals[real.Value], Tile = new Point2(real.Value.x, real.Value.y) });
            Pump(conn, 800);

            Check("แตะ tile เดิมด้วย id ปลอมคนละตัว ได้ id เดียวกันจาก server",
                first != null && first == idFromServer && first.StartsWith("natural_"),
                $"ครั้งแรก={first} ครั้งที่สอง={idFromServer}");

            aborts = 0; collected = 0;
            conn.Send(new Collect { EntityId = "id-ที่คิดขึ้นเอง-1", GeneratorId = "leaf", Tile = new Point2(real.Value.x, real.Value.y) });
            Pump(conn, 1200);
            Check("เก็บของด้วย id ที่คิดขึ้นเอง ไม่ผ่าน", aborts > 0 && collected == 0, $"abort={aborts} collected={collected}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] ไม่มีของธรรมชาติใกล้พอจะเทส H-5");
        }

        // เก็บของโดยไม่เคยแตะ
        aborts = 0; collected = 0;
        conn.Send(new Collect { EntityId = "natural_999_999", GeneratorId = "leaf", Tile = new Point2(41, 178) });
        Pump(conn, 1000);
        Check("เก็บของโดยไม่เคยแตะ ไม่ผ่าน", aborts > 0 && collected == 0, $"abort={aborts} collected={collected}");

        // ── GP-08: คราฟต์ต้องมีวัตถุดิบ ───────────────────────────────────
        Console.WriteLine("GP-08 — คราฟต์ลม");
        conn.Send(default(GetInventory));
        Pump(conn, 600);

        aborts = 0; crafted = 0;
        conn.Send(new Craft { RecipeId = "axe_tool_bone_01", Materials = null });
        Pump(conn, 3000);
        Check("คราฟต์โดยไม่ใส่วัตถุดิบ ไม่ผ่าน", aborts > 0 && crafted == 0, $"abort={aborts} crafted={crafted}");

        aborts = 0; crafted = 0;
        conn.Send(new Craft
        {
            RecipeId = "axe_tool_bone_01",
            Materials = new Dictionary<string, string[]>
            {
                { "main", new[] { "ไอเทมที่ไม่มีจริง-1" } },
                { "connector", new[] { "ไอเทมที่ไม่มีจริง-2" } },
                { "handle", new[] { "ไอเทมที่ไม่มีจริง-3" } }
            }
        });
        Pump(conn, 3000);
        Check("คราฟต์ด้วย item id ที่ไม่มีในกระเป๋า ไม่ผ่าน", aborts > 0 && crafted == 0, $"abort={aborts} crafted={crafted}");

        aborts = 0; crafted = 0;
        conn.Send(new Craft { RecipeId = "สูตรที่ไม่มีในเกม", Materials = null });
        Pump(conn, 2000);
        Check("คราฟต์สูตรที่ไม่มีในเกม ไม่ผ่าน", aborts > 0 && crafted == 0, $"abort={aborts} crafted={crafted}");

        // ใส่ไอเทมชิ้นเดียวกันซ้ำหลายช่อง — ต้องมีของอย่างน้อย 1 ชิ้นถึงจะเทสได้
        // (เททิ้งกระเป๋าตอนเริ่มไปแล้ว จึงต้องเสกคืนมาเอง ไม่งั้นข้อนี้จะถูกข้ามตลอด)
        if (inventory.Count == 0)
        {
            conn.Send(new Cheat { _Cheat = "add stone" });
            Pump(conn, 800);
        }
        if (inventory.Count > 0)
        {
            string id = inventory[0].Id;
            aborts = 0; crafted = 0;
            conn.Send(new Craft
            {
                RecipeId = "axe_tool_bone_01",
                Materials = new Dictionary<string, string[]>
                {
                    { "main", new[] { id } },
                    { "connector", new[] { id } },
                    { "handle", new[] { id } }
                }
            });
            Pump(conn, 3000);
            Check("ใส่ไอเทมชิ้นเดียวซ้ำทุกช่อง ไม่ผ่าน", aborts > 0 && crafted == 0, $"abort={aborts} crafted={crafted}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] กระเป๋าว่าง เลยไม่ได้เทสการใส่ไอเทมซ้ำช่อง");
        }

        // ── GP-14: เลเวลต้องเป็นของ server ไม่ใช่ของ client ────────────────
        //
        // **beta 1.0.0 เปลี่ยนกติกา:** เดิมผู้เล่นใหม่ใช้เลเวลที่ client บอกได้ 1 ครั้ง
        // ตอนนี้ **ไม่รับเลยแม้แต่ครั้งแรก** — เลเวลคิดจาก exp ที่ server เก็บอย่างเดียว
        //
        // ที่เปลี่ยนเพราะ: ตัวเกมส่งเลเวลของตัวละครบนเกาะตัวเองมาทุก login
        // ⇒ ลบไฟล์เซฟเพื่อรีเซ็ตแล้วไม่เป็นผล (เจอจริง: รีเซ็ตเป็น lv1 แล้วในเกมยังขึ้น lv7)
        Console.WriteLine("GP-14 — โกงเลเวล");
        string lvId = "gp-check-lv-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        appearLevels.Remove(lvId);
        bool canTestLevels = EnterOnce(host, gamePort, gatewayPort, lvId, 5);
        if (!canTestLevels)
        {
            Console.WriteLine("  [ข้าม] server เปิด whitelist อยู่ — เทส GP-14 ต้องรันด้วย --no-account-check");
        }
        Pump(conn, 1200);
        int firstLevel = appearLevels.TryGetValue(lvId, out int l1) ? l1 : -1;
        if (canTestLevels)
        {
            Check("ผู้เล่นใหม่เริ่มเลเวล 1 เสมอ (ไม่รับเลเวลที่ client อ้าง)", firstLevel == 1,
                $"client อ้างเลเวล 5 · เห็นเลเวล {firstLevel}");
        }

        appearLevels.Remove(lvId);
        EnterOnce(host, gamePort, gatewayPort, lvId, 60);
        Pump(conn, 1200);
        int secondLevel = appearLevels.TryGetValue(lvId, out int l2) ? l2 : -1;
        if (canTestLevels)
        {
            Check("login รอบสองอ้างเลเวล 60 ไม่ได้", secondLevel == 1, $"เห็นเลเวล {secondLevel}");
        }

        // ผู้เล่นใหม่อีกคนที่อ้างเลเวลเกินเพดาน — ตอนนี้ไม่รับตั้งแต่แรกจึงได้เลเวล 1
        string capId = "gp-check-cap-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        appearLevels.Remove(capId);
        EnterOnce(host, gamePort, gatewayPort, capId, 9999);
        Pump(conn, 1200);
        int cappedLevel = appearLevels.TryGetValue(capId, out int l3) ? l3 : -1;
        if (canTestLevels)
        {
            Check("อ้างเลเวล 9999 แล้วยังได้เลเวล 1", cappedLevel == 1, $"เห็นเลเวล {cappedLevel}");
        }

        // ── เฟส C รอบ 2: ต่อสู้ต้องตรวจท่า/ระยะ/สถานะ ──────────────────────
        Console.WriteLine("ต่อสู้ — โกงท่า/ระยะ");
        int damaged = 0;
        conn.Recv<Damaged>((m, h) => damaged++);
        conn.Recv<BattleBegun>((m, h) => { });
        conn.Recv<Actions>((m, h) => { });
        conn.Send(default(GetActions));
        Pump(conn, 600);

        // หาสัตว์ใกล้ตัวไว้เป็นเป้า
        string animalNear = null, animalFar = null;
        double bestD = double.MaxValue;
        foreach (var kv in appearAnimals)
        {
            double dx = (kv.Value.x - px) / 200.0, dy = (kv.Value.y - py) / 200.0;
            double d = dx * dx + dy * dy;
            if (d < bestD) { bestD = d; animalNear = kv.Key; }
            if (d > 25 * 25) { animalFar = kv.Key; }
        }

        if (animalNear != null)
        {
            aborts = 0; damaged = 0;
            conn.Send(new UseBattleAction { ActionId = "ท่าที่ไม่มีในเกม", StartAt = Times.UnixTimeNow(), TargetEntityId = animalNear });
            Pump(conn, 1200);
            Check("ใช้ท่าที่ไม่มีในเกม ไม่ผ่าน", aborts > 0 && damaged == 0, $"abort={aborts} damaged={damaged}");

            aborts = 0; damaged = 0;
            // onehand_default_a เป็นท่าของดาบมือเดียว — มือเปล่าห้ามใช้
            conn.Send(new UseBattleAction { ActionId = "onehand_default_a", StartAt = Times.UnixTimeNow(), TargetEntityId = animalNear });
            Pump(conn, 1200);
            Check("ใช้ท่าของอาวุธที่ไม่ได้ถือ ไม่ผ่าน", aborts > 0 && damaged == 0, $"abort={aborts} damaged={damaged}");

            aborts = 0; damaged = 0;
            conn.Send(new UseBattleAction { ActionId = "barehand_default_a", StartAt = Times.UnixTimeNow(), TargetEntityId = "สัตว์ที่ไม่มีจริง" });
            Pump(conn, 1200);
            Check("ตีเป้าหมายที่ไม่มีอยู่จริง ไม่ผ่าน", aborts > 0 && damaged == 0, $"abort={aborts} damaged={damaged}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] ไม่เห็นสัตว์เลย เทสต่อสู้ไม่ได้");
        }

        if (animalFar != null)
        {
            aborts = 0; damaged = 0;
            conn.Send(new UseBattleAction { ActionId = "barehand_default_a", StartAt = Times.UnixTimeNow(), TargetEntityId = animalFar });
            Pump(conn, 1200);
            Check("ตีสัตว์ที่อยู่ไกลเกินระยะท่า ไม่ผ่าน", aborts > 0 && damaged == 0, $"abort={aborts} damaged={damaged}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] ไม่เจอสัตว์ที่ไกลพอจะเทสระยะ");
        }

        // ตีของจริงต้องเข้า (ทำก่อนเทสตาย เพราะฟื้นแล้วจะถูกวาร์ปกลับจุดเกิด)
        if (animalNear != null)
        {
            // เดินไปยืนบนตัวมันก่อน — สัตว์เดินหนีไปได้ระหว่างที่เทสข้ออื่นอยู่
            (float x, float y) at = appearAnimals[animalNear];
            WalkTo(conn, "gp-check-1", ref px, ref py, at.x, at.y);
            aborts = 0; damaged = 0;
            conn.Send(new UseBattleAction { ActionId = "barehand_default_a", StartAt = Times.UnixTimeNow(), TargetEntityId = animalNear });
            Pump(conn, 2000);
            Check("ตีสัตว์ใกล้ ๆ ด้วยท่าที่ถูกต้อง ได้ดาเมจจริง", damaged > 0, $"abort={aborts} damaged={damaged}");
        }

        // ── แตะสัตว์ต้องได้ปุ่มโจมตี · ซากต้องแล่ได้ ────────────────────────
        // (เจอตอนเล่นจริง: แตะสัตว์แล้วปุ่มโจมตีไม่ขึ้น เพราะ HandleTouch ไม่มีเคสของสัตว์เลย)
        Console.WriteLine("แตะสัตว์ / แล่เนื้อ");
        conn.Send(new Cheat { _Cheat = "spawn 2042" });     // กิ้งก่า — ชิ้นส่วนน้อย เทสเร็ว
        Pump(conn, 1200);
        // เลือก "ตัวที่อยู่ใกล้ตัวเราที่สุด" ไม่ใช่ตัวที่ appear มาล่าสุด — cheat spawn เกิดตรงตำแหน่งเรา
        // (ตัวที่ appear ล่าสุดอาจเป็นตัวที่เกิดใหม่แทนตัวที่ตายไป หรือตัวที่หายไปแล้วก็ได้)
        string victim = null;
        double victimD = double.MaxValue;
        foreach (var kv in appearAnimals)
        {
            if (goneAnimals.Contains(kv.Key)) continue;      // ซากใช้เทส "สัตว์เป็น ๆ" ไม่ได้
            double ax = kv.Value.x - px, ay = kv.Value.y - py;
            double d = ax * ax + ay * ay;
            if (d < victimD) { victimD = d; victim = kv.Key; }
        }
        if (victim == null)
        {
            Console.WriteLine("  [ข้าม] เรียกสัตว์มาเกิดไม่สำเร็จ เทสแล่เนื้อไม่ได้");
        }
        else
        {
            aborts = 0; touched = 0; lastTouched = default;
            conn.Send(new Touch { EntityId = victim, EntityType = 2042, Tile = new Point2(-1, -1) });
            Pump(conn, 800);
            bool hasAttack = touched > 0 && lastTouched.Interactions != null && Array.IndexOf(lastTouched.Interactions, 1) >= 0;
            Check("แตะสัตว์เป็น ๆ แล้วได้ปุ่มโจมตี", hasAttack,
                $"victim={victim} reply.id={lastTouched.EntityId} name={lastTouched.EntityName} lv={lastTouched.Level} touched={touched} interactions=[{(lastTouched.Interactions == null ? "null" : string.Join(",", lastTouched.Interactions))}]");

            // ยังไม่ตาย = แล่ไม่ได้
            aborts = 0; collected = 0;
            conn.Send(new Collect { EntityId = victim, GeneratorId = "meat", Tile = new Point2(-1, -1) });
            Pump(conn, 1000);
            Check("แล่สัตว์ที่ยังไม่ตาย ไม่ผ่าน", aborts > 0 && collected == 0, $"abort={aborts} collected={collected}");

            // Other aggressive animals may stand even closer than the freshly
            // spawned victim. Repeat until this exact entity is the dead one.
            for (int attempt = 0; attempt < 6 && !goneAnimals.Contains(victim); attempt++)
            {
                conn.Send(new Cheat { _Cheat = "kill animal" });
                Pump(conn, 900);
            }

            aborts = 0; touched = 0; lastTouched = default;
            conn.Send(new Touch { EntityId = victim, EntityType = 2042, Tile = new Point2(-1, -1) });
            Pump(conn, 800);
            Generator[] parts = lastTouched.Collectible.Generators;
            Check("แตะซากแล้วได้เมนูแล่เนื้อ", touched > 0 && parts != null && parts.Length > 0,
                $"touched={touched} parts={(parts?.Length ?? 0)}");

            if (parts != null && parts.Length > 0)
            {
                // 🐛 อาการ "ม่อนเดินวาร์ป": ตีสัตว์ที่กำลังเดินอยู่ แล้วคำสั่งใหม่เริ่มจาก "ปลายทางเก่า"
            // ⇒ client กระโดดไปข้างหน้าทันที · วัดด้วยการเทียบจุดเริ่มคำสั่งใหม่กับตำแหน่งที่ควรอยู่
            // ต้องจับจังหวะที่มัน "กำลังเดินทางไกล" อยู่ แล้วค่อยตี ถึงจะเห็นอาการวาร์ป
            // (โดนตีแล้ว AI ล้างเวลาพักทันที → สั่งเดินใหม่ทับคำสั่งเดิมที่ยังเดินไม่จบ)
            bool caught = false;
            for (int wait = 0; wait < 100 && !caught; wait++)
            {
                Pump(conn, 200);
                if (lastPath.TryGetValue(victim, out Location[] cur) && cur.Length >= 2)
                {
                    // เงื่อนไข 2 ข้อ: (1) ยังเดินไม่จบ เหลืออีก >1.5 วิ (2) อยู่ใกล้พอให้เราตีถึง
                    double nowT = Times.UnixTimeNow();
                    double span = cur[1].Time - cur[0].Time;
                    float f = span <= 0 ? 1f : (float)Math.Max(0.0, Math.Min(1.0, (nowT - cur[0].Time) / span));
                    float ax = cur[0].Position.x + (cur[1].Position.x - cur[0].Position.x) * f;
                    float ay = cur[0].Position.y + (cur[1].Position.y - cur[0].Position.y) * f;
                    float adx = ax - px, ady = ay - py;
                    caught = cur[1].Time - nowT > 1.5 && (adx * adx + ady * ady) < 500f * 500f;
                }
            }
            if (!caught)
            {
                Console.WriteLine("  [ข้าม] รอจังหวะที่สัตว์กำลังเดินทางไกลไม่ทัน");
            }
            else
            {
                maxJump.Remove(victim);
                moveCount.Remove(victim);
                conn.Send(new UseBattleAction { ActionId = "barehand_default_a", StartAt = Times.UnixTimeNow(), TargetEntityId = victim });
                Pump(conn, 2500);
                float jumped = maxJump.TryGetValue(victim, out float jv) ? jv : 0f;
                int moves = moveCount.TryGetValue(victim, out int mv) ? mv : 0;
                if (moves == 0)
                {
                    // ไม่มีคำสั่งใหม่เข้ามาเลย = ตีไม่โดน/มันไม่สนใจ → เทสข้อนี้ไม่ได้วัดอะไร
                    Console.WriteLine("  [ข้าม] ตีแล้วสัตว์ไม่เปลี่ยนคำสั่งเดิน วัดการวาร์ปไม่ได้");
                }
                else
                {
                    Check($"สัตว์ไม่วาร์ปตอนโดนตีกลางทาง (คำสั่งใหม่ {moves} ครั้ง)", jumped < 120f,
                        $"กระโดด {jumped:F0} หน่วย = {jumped / 200f:F1} tile");
                }
            }

            // เซฟของรอบก่อนอาจมีมีดค้างอยู่ — ทิ้งให้หมดก่อน ไม่งั้นเทส "ไม่มีมีด" ไม่จริง
                conn.Send(default(GetInventory));
                Pump(conn, 600);
                var knives = new List<string>();
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (inventory[i].Prototype == "blade_stone") knives.Add(inventory[i].Id);
                }
                if (knives.Count > 0)
                {
                    conn.Send(new DumpItems { ItemIds = knives.ToArray() });
                    Pump(conn, 1200);
                }

                // GP-08b: ไม่มีมีด = แล่ไม่ได้ (server ตอบ ToolNeeded ไม่ใช่ให้ของ)
                int toolNeeded = 0;
                conn.Recv<ToolNeeded>((m, h) => toolNeeded++);
                aborts = 0; collected = 0;
                conn.Send(new Collect { EntityId = victim, GeneratorId = parts[0].Id, Tile = new Point2(-1, -1) });
                Pump(conn, 1500);
                Check("แล่เนื้อโดยไม่มีมีด ไม่ผ่าน", collected == 0 && toolNeeded > 0,
                    $"collected={collected} toolNeeded={toolNeeded}");

                conn.Send(new Cheat { _Cheat = "add knife" });
                Pump(conn, 900);

                int before = inventory.Count;
                aborts = 0; collected = 0;
                conn.Send(new Collect { EntityId = victim, GeneratorId = parts[0].Id, Tile = new Point2(-1, -1) });
                Pump(conn, 5000);
                Check($"มีมีดแล้วแล่ซากได้ของจริง ({parts[0].Name})", collected > 0 && inventory.Count > before,
                    $"abort={aborts} collected={collected} ของในกระเป๋า {before}→{inventory.Count}");

                // ไกลเกินเอื้อมแล้วต้องแล่ไม่ได้ (2 ก้าว = 1800 หน่วย > 8 tile)
                float bx = px, by = py;
                WalkTo(conn, "gp-check-1", ref px, ref py, px + 1800f, py);
                aborts = 0; collected = 0;
                conn.Send(new Collect { EntityId = victim, GeneratorId = parts[0].Id, Tile = new Point2(-1, -1) });
                Pump(conn, 1200);
                Check("แล่ซากที่อยู่ไกลเกินเอื้อม ไม่ผ่าน", aborts > 0 && collected == 0, $"abort={aborts} collected={collected}");
                WalkTo(conn, "gp-check-1", ref px, ref py, bx, by);
            }
        }

        // ตายแล้วต้องทำอะไรไม่ได้
        aborts = 0; damaged = 0;
        conn.Send(new Cheat { _Cheat = "die" });
        Pump(conn, 1000);
        conn.Send(new UseBattleAction { ActionId = "barehand_default_a", StartAt = Times.UnixTimeNow(), TargetEntityId = animalNear ?? "x" });
        Pump(conn, 1000);
        conn.Send(new Collect { EntityId = "natural_41_178", GeneratorId = "leaf", Tile = new Point2(41, 178) });
        Pump(conn, 1000);
        Check("ตายแล้วตี/เก็บของไม่ได้", aborts >= 2 && damaged == 0, $"abort={aborts} damaged={damaged}");

        int revived = 0;
        conn.Recv<Revived>((m, h) => revived++);
        conn.Send(new Revive { WarpholeTile = null });
        Pump(conn, 1500);
        Check("สั่ง Revive แล้วฟื้นจริง", revived > 0, $"revived={revived}");

        // ── รอบ D: สร้างสิ่งปลูกสร้าง · กล่อง · ช่องอุปกรณ์ · สกิล ─────────
        Console.WriteLine("รอบ D — สร้าง/กล่อง/ช่อง/สกิล");

        // H-7: จองที่ไกลเกินเอื้อม และนอกแมพ
        aborts = 0;
        conn.Send(new OccupyArtifactSite { BlueprintId = "bonfire", Tile = new Point2(200, 200), Size = new Point2(1, 1) });
        Pump(conn, 900);
        Check("จองที่สร้างไกลเกินเอื้อม ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        aborts = 0;
        conn.Send(new OccupyArtifactSite { BlueprintId = "bonfire", Tile = new Point2(9999, 9999), Size = new Point2(1, 1) });
        Pump(conn, 900);
        Check("จองที่สร้างนอกแมพ ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        // H-6: สั่งสร้างบ้านที่ไม่มีจริง / ไม่ใช่ของตัวเอง
        aborts = 0;
        conn.Send(new BuildArtifact { EntityId = "สิ่งปลูกสร้างที่ไม่มีจริง", Tile = new Point2(40, 177) });
        Pump(conn, 900);
        Check("สั่งสร้างสิ่งปลูกสร้างที่ไม่มีจริง ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        if (otherArtifact != null)
        {
            aborts = 0;
            conn.Send(new BuildArtifact { EntityId = otherArtifact, Tile = new Point2(49, 178) });
            Pump(conn, 900);
            Check("สั่งสร้างของคนอื่น ไม่ผ่าน", aborts > 0, $"abort={aborts}");

            // M-4: เปิดกล่องของคนอื่น
            aborts = 0;
            conn.Send(new TakeOutItem { EntityId = otherArtifact, ItemIds = new[] { "อะไรก็ได้" } });
            Pump(conn, 900);
            Check("หยิบของจากสิ่งปลูกสร้างของคนอื่น ไม่ผ่าน", aborts > 0, $"abort={aborts}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] ไม่มีสิ่งปลูกสร้างของคนอื่นให้เทส H-6/M-4");
        }

        // M-7: ช่องอุปกรณ์ที่ไม่มีจริง
        aborts = 0;
        conn.Send(new Equip { ItemId = "ไอเทมอะไรก็ได้", SlotName = "ช่องที่คิดขึ้นเอง", Action = "equip" });
        Pump(conn, 900);
        Check("ใส่ของในช่องที่ไม่มีจริง ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        // M-7: สกิลที่ไม่มีจริง + เลเวลเกิน
        aborts = 0;
        conn.Send(new LearnSkill { SkillId = "สกิลที่ไม่มีในเกม", SubId = null, Level = 1 });
        Pump(conn, 900);
        Check("เรียนสกิลที่ไม่มีในเกม ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        aborts = 0;
        conn.Send(new LearnSkill { SkillId = "gathering", SubId = null, Level = 9999 });
        Pump(conn, 900);
        Check("เรียนสกิลด้วยเลเวล 9999 ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        // Beta 1.0: สกิลมีผลกับเกมจริงแล้ว → เลเวลสกิลต้องไม่เกินเลเวลผู้เล่น
        aborts = 0;
        conn.Send(new LearnSkill { SkillId = "gathering", SubId = null, Level = 60 });
        Pump(conn, 900);
        Check("เรียนสกิลเลเวล 60 ตอนตัวเองเลเวลต่ำ ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        // ── ของจริงต้องยังคราฟต์ได้ (กันแก้บั๊กแล้วห้ามทุกอย่างไปเลย) ─────
        Console.WriteLine("คราฟต์ปกติ");
        conn.Send(new Cheat { _Cheat = "add axe" });
        Pump(conn, 400);
        conn.Send(new Cheat { _Cheat = "add clothes" });
        Pump(conn, 400);
        conn.Send(new Cheat { _Cheat = "add box" });
        Pump(conn, 400);
        conn.Send(default(GetInventory));
        Pump(conn, 600);

        // GP-08b: สูตร blade_stone (มีดหิน) ขอ tag chunk_* + วัสดุ stone
        // เอาของที่ไม่ใช่หินมายัดต้องไม่ผ่าน แต่ใส่หินจริงต้องได้มีด
        conn.Send(new Cheat { _Cheat = "add stone" });
        Pump(conn, 500);
        conn.Send(default(GetInventory));
        Pump(conn, 600);
        string stoneId = null, notStoneId = null;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].Prototype == "stone" && stoneId == null) stoneId = inventory[i].Id;
            else if (inventory[i].Prototype == "axe_onehand_stone_01" && notStoneId == null) notStoneId = inventory[i].Id;
        }

        if (notStoneId != null)
        {
            aborts = 0; crafted = 0;
            conn.Send(new Craft
            {
                RecipeId = "blade_stone",
                Materials = new Dictionary<string, string[]> { { "base", new[] { notStoneId } } }
            });
            Pump(conn, 1500);
            Check("คราฟต์มีดหินด้วยของที่ไม่ใช่หิน ไม่ผ่าน", aborts > 0 && crafted == 0, $"abort={aborts} crafted={crafted}");
        }

        if (stoneId != null)
        {
            int before = inventory.Count;
            aborts = 0; crafted = 0;
            conn.Send(new Craft
            {
                RecipeId = "blade_stone",
                Materials = new Dictionary<string, string[]> { { "base", new[] { stoneId } } }
            });
            Pump(conn, 3500);
            conn.Send(default(GetInventory));
            Pump(conn, 600);
            Check("คราฟต์มีดหินด้วยหินจริง สำเร็จ", crafted > 0 && aborts == 0, $"abort={aborts} crafted={crafted}");
            bool stoneGone = inventory.FindIndex(it => it.Id == stoneId) < 0;
            Check("วัตถุดิบถูกหักออกจากกระเป๋าจริง", stoneGone && inventory.Count == before, $"ก่อน {before} หลัง {inventory.Count} หินหาย={stoneGone}");
        }
        else
        {
            Console.WriteLine("  [ข้าม] ไม่มีหินในกระเป๋า เลยไม่ได้เทสคราฟต์ปกติ");
        }

        // ── ทิ้งของ / กินของ (handler ใหม่ของ beta 1.0) ────────────────────
        Console.WriteLine("ทิ้งของ/กินของ");

        aborts = 0;
        conn.Send(new DumpItems { ItemIds = new[] { "ไอเทมที่ไม่มีจริง" } });
        Pump(conn, 800);
        Check("ทิ้งของที่ไม่มีในกระเป๋า ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        aborts = 0;
        var tooMany = new string[80];
        for (int i = 0; i < tooMany.Length; i++) tooMany[i] = "x" + i;
        conn.Send(new DumpItems { ItemIds = tooMany });
        Pump(conn, 800);
        Check("ทิ้งของทีเดียว 80 ชิ้น ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        if (otherArtifact != null)
        {
            aborts = 0;
            conn.Send(new DumpItems { SourceProp = new PropKey { EntityId = otherArtifact }, ItemIds = new[] { "อะไรก็ได้" } });
            Pump(conn, 800);
            Check("ทิ้งของในกล่องคนอื่น ไม่ผ่าน", aborts > 0, $"abort={aborts}");
        }

        aborts = 0;
        conn.Send(new UseItem { ItemId = "ไอเทมที่ไม่มีจริง" });
        Pump(conn, 800);
        Check("กินของที่ไม่มีในกระเป๋า ไม่ผ่าน", aborts > 0, $"abort={aborts}");

        conn.Send(default(GetInventory));
        Pump(conn, 600);
        if (inventory.Count > 0)
        {
            // ขวาน/เสื้อผ้าจาก cheat กินไม่ได้
            string toolId = null;
            for (int i = 0; i < inventory.Count; i++)
            {
                string proto = inventory[i].Prototype ?? "";
                if (proto.Contains("axe") || proto.Contains("cloth")) { toolId = inventory[i].Id; break; }
            }
            if (toolId != null)
            {
                aborts = 0;
                conn.Send(new UseItem { ItemId = toolId });
                Pump(conn, 800);
                Check("กินของที่กินไม่ได้ ไม่ผ่าน", aborts > 0, $"abort={aborts}");
            }

            int before = inventory.Count;
            aborts = 0;
            conn.Send(new DumpItems { ItemIds = new[] { inventory[0].Id } });
            Pump(conn, 900);
            conn.Send(default(GetInventory));
            Pump(conn, 600);
            Check("ทิ้งของจริงแล้วของหายจากกระเป๋า", inventory.Count == before - 1 && aborts == 0,
                $"ก่อน {before} หลัง {inventory.Count} abort={aborts}");
        }

        conn.Close();

        Console.WriteLine();
        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>
    /// เดินเป็นก้าว ๆ (ไม่เกิน 1200 หน่วยต่อ packet) — M-2 ที่ server กันการวาร์ป
    /// ทำให้ส่ง Move ทีเดียวข้ามหลาย tile ไม่ผ่านอีกต่อไป
    /// </summary>
    private static void WalkTo(Connection conn, string entityId, ref float px, ref float py, float x, float y)
    {
        for (int guard = 0; guard < 20; guard++)
        {
            float dx = x - px, dy = y - py;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            if (dist < 1f) break;
            float step = Math.Min(MaxStepUnits, dist);
            px += dx / dist * step;
            py += dy / dist * step;
            MoveTo(conn, entityId, px, py);
            Pump(conn, 1000);
            if (dist <= MaxStepUnits) break;
        }
    }

    /// <summary>M-2: server ยอมให้ขยับได้ 900 หน่วย/วิ + เผื่อ 300 — ก้าวละ 900 ต่อ 1 วินาทีจึงอยู่ในเกณฑ์</summary>
    private const float MaxStepUnits = 900f;

    private static void MoveTo(Connection conn, string entityId, float x, float y)
    {
        conn.Send(new Move
        {
            EntityId = entityId,
            Movements = new[]
            {
                new Movement
                {
                    MotionName = "Barehand_Walk",
                    MotionOption = 5,
                    PlaybackRate = 1f,
                    RotSpeed = 540f,
                    Path = new[]
                    {
                        new Location { Position = new WorldPosition(x, y), Yaw = 0f, Time = Times.UnixTimeNow(), Floor = 0, Height = 0f }
                    }
                }
            }
        });
    }

    /// <summary>
    /// เข้าเกมครบขั้นตอน (ขอ token → Auth → Ready) โดยอ้างเลเวลตามที่สั่ง แล้วออก
    /// ตอนตัดการเชื่อมต่อ server จะเซฟ state ของผู้เล่นคนนี้ลงดิสก์
    /// </summary>
    /// <summary>คืน false ถ้าขอ token ไม่ได้ (เช่น server เปิด whitelist อยู่ → ข้ามเทสไป ไม่ใช่สอบตก)</summary>
    private static bool EnterOnce(string host, int gamePort, int gatewayPort, string entityId, int level)
    {
        string token = SessionClient.Fetch(host, gatewayPort, entityId, entityId, level);
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }
        try
        {
            using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(host, gamePort);
            var conn = new Connection(sock);
            conn.Recv<Welcome>((m, h) => { });
            conn.Recv<Clock>((m, h) => { });
            conn.Recv<OK>((m, h) => { });
            conn.Recv<Abort>((m, h) => { });
            conn.StartReceive();
            conn.Send(new GetClock { Time = Times.UnixTimeNow() });
            Pump(conn, 300);
            conn.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
            Pump(conn, 1000);
            conn.Send(default(Ready));
            Pump(conn, 1500);
            conn.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("  (เข้าเกมด้วย " + entityId + " ไม่ได้: " + e.Message + ")");
        }
        return true;
    }

    /// <summary>ลอง Auth ด้วย token ที่ให้มา แล้วดูว่าได้ Welcome ไหม</summary>
    private static bool CanEnter(string host, int gamePort, string entityId, string token)
    {
        try
        {
            using var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect(host, gamePort);
            var conn = new Connection(sock);
            bool welcomed = false;
            conn.Recv<Welcome>((m, h) => welcomed = true);
            conn.Recv<Abort>((m, h) => { });
            conn.Recv<Clock>((m, h) => { });
            conn.StartReceive();
            conn.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
            Pump(conn, 1200);
            conn.Close();
            return welcomed;
        }
        catch (Exception e)
        {
            Console.WriteLine("  (ต่อ server ไม่ได้: " + e.Message + ")");
            return false;
        }
    }
}
