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
/// เทส **ระบบทำอาหาร** (Features.Cooking)
///
/// สิ่งที่ต้องจริงทั้งหมดถึงจะเรียกว่าทำอาหารได้:
///   1. สูตรทำอาหาร **ต้องมีไฟ** — ยืนกลางทุ่งแล้วสั่งย่าง ต้องไม่ผ่าน
///   2. **ต้องมีเครื่องมือ** — มีกองไฟแต่ไม่มีไม้เสียบ/หม้อ ต้องไม่ผ่าน (และตอบ ToolNeeded)
///   3. ครบเงื่อนไข → ได้ของสุกจริง และ **วัตถุดิบหายไปจากกระเป๋า**
///   4. ของสุกให้พลังมากกว่าของดิบ (ไม่งั้นไม่มีเหตุผลให้ทำอาหาร)
///   5. เตาที่แรงไม่พอทำสูตรที่ยากไม่ได้ (น้ำซุปต้องใช้กองไฟใหญ่)
///   6. สูตรที่ได้ทีละหลายชิ้น ต้องได้ครบตามข้อมูลเกม
///   7. กินติด ๆ กันไม่ได้ (เวลาย่อย)
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat
///
/// รัน: dotnet run -- --cook-check [host] [port เกม] [port gateway]
/// </summary>
public static class CookCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private static int _aborts;
    private static string _info = "";
    private static readonly List<Item> _crafted = new List<Item>();
    private static Item[] _inventory = Array.Empty<Item>();
    private static string _toolNeeded;
    private static float _stamina;
    private static readonly List<string> _placedArtifacts = new List<string>();

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }

    private static void Reset()
    {
        _aborts = 0;
        _info = "";
        _crafted.Clear();
        _toolNeeded = null;
    }

    /// <summary>หาไอเทมชิ้นแรกในกระเป๋าที่เป็น prototype นี้</summary>
    private static Item? Find(string prototype)
    {
        for (int i = 0; i < _inventory.Length; i++)
        {
            if (_inventory[i].Prototype == prototype)
            {
                return _inventory[i];
            }
        }
        return null;
    }

    /// <summary>นับของที่ prototype นี้ซึ่ง **ยังดิบอยู่** (ยังมี tag raw_food)</summary>
    private static int CountRaw(string prototype)
    {
        int n = 0;
        for (int i = 0; i < _inventory.Length; i++)
        {
            if (_inventory[i].Prototype == prototype && HasTag(_inventory[i], "raw_food"))
            {
                n++;
            }
        }
        return n;
    }

    private static int CountOf(string prototype)
    {
        int n = 0;
        for (int i = 0; i < _inventory.Length; i++)
        {
            if (_inventory[i].Prototype == prototype)
            {
                n++;
            }
        }
        return n;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== cooking check: {host}:{gamePort} ===");

        string id = "cook-" + Guid.NewGuid().ToString("N").Substring(0, 8);
        string token = SessionClient.Fetch(host, gatewayPort, id, id);
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }

        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        Point2 myTile = default;

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _info += (m.Text ?? "") + "\n");
        conn.Recv<ToolNeeded>((m, h) => _toolNeeded = m.TagNames);
        conn.Recv<Crafted>((m, h) => { if (m.Items != null) _crafted.AddRange(m.Items); });
        conn.Recv<Inventory>((m, h) => { if (m.InventoryItems.Items != null) _inventory = m.InventoryItems.Items; });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<ItemUsed>((m, h) => { });
        conn.Recv<Survival>((m, h) => { });
        conn.Recv<SurvivalUpdated>((m, h) =>
        {
            if (m.Updated != null && m.Updated.TryGetValue("stamina", out Gauge g))
            {
                _stamina = g.Get();
            }
        });
        conn.Recv<AppearArtifact>((m, h) =>
        {
            if (!_placedArtifacts.Contains(m.EntityId ?? ""))
            {
                _placedArtifacts.Add(m.EntityId ?? "");
            }
        });
        conn.Recv<Occupied>((m, h) => { });
        conn.Recv<ArtifactBuilt>((m, h) => { });
        conn.Recv<ArtifactCompleted>((m, h) => { });
        conn.Recv<ArtifactMaterials>((m, h) => { });
        conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Chunk>((m, h) => { });
        conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearAnimal>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Recipes>((m, h) => { });
        conn.Recv<ArtifactBlueprints>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2000);

        conn.Send(new Cheat { _Cheat = "clearbag" });
        Pump(conn, 400);
        conn.Send(new Cheat { _Cheat = "rest" });
        Pump(conn, 400);
        conn.Send(new Cheat { _Cheat = "who" });
        Pump(conn, 500);
        // อ่าน tile ของตัวเองจากข้อความ who: "... | tile X,Y | ..."
        foreach (string line in _info.Split('\n'))
        {
            int t = line.IndexOf("tile ", StringComparison.Ordinal);
            if (t < 0)
            {
                continue;
            }
            string[] xy = line.Substring(t + 5).Split('|')[0].Trim().Split(',');
            if (xy.Length == 2 && int.TryParse(xy[0], out int tx) && int.TryParse(xy[1], out int ty))
            {
                myTile = new Point2(tx, ty);
                break;
            }
        }
        Console.WriteLine($"ยืนอยู่ tile {myTile.x},{myTile.y}");

        // ── รอบ 1: ทำอาหารกลางทุ่ง (ไม่มีไฟ) ────────────────────────────
        Console.WriteLine("รอบ 1 — ไม่มีไฟ ต้องทำอาหารไม่ได้");
        conn.Send(new Cheat { _Cheat = "give meat 4" });
        Pump(conn, 600);
        conn.Send(new Cheat { _Cheat = "give wood_bough 2" });
        Pump(conn, 400);
        Reset();
        Item? meat = Find("meat");
        if (meat == null)
        {
            Console.WriteLine("  [ตก ] เสกเนื้อไม่ได้ — cheat `give` ใช้ไม่ได้?");
            Console.WriteLine("        " + _info.Trim());
            conn.Close();
            return 1;
        }
        conn.Send(new Craft
        {
            RecipeId = "skewer",
            Materials = new Dictionary<string, string[]> { { "base", new[] { meat.Value.Id } } },
            ToolItemId = null,
            Workbench = null
        });
        Pump(conn, 1500);
        Check("ย่างกลางทุ่ง (ไม่มีกองไฟ) ไม่ผ่าน", _aborts > 0 && _crafted.Count == 0,
            $"abort={_aborts} ได้ของ={_crafted.Count}");
        Check("บอกเหตุผลว่าต้องใช้กองไฟ", _info.Contains("กองไฟ"), _info.Trim().Replace("\n", " / "));

        // ── รอบ 2: วางกองไฟแล้วลองใหม่ ─────────────────────────────────
        Console.WriteLine("รอบ 2 — วางกองไฟ");
        _placedArtifacts.Clear();
        conn.Send(new Cheat { _Cheat = "add bonfire" });
        Pump(conn, 600);
        Item? capsule = _inventory.FirstOrDefault(x => (x.Prototype ?? "").StartsWith("capsulated_bonfire", StringComparison.Ordinal)) is Item c && c.Id != null ? c : (Item?)null;
        if (capsule == null)
        {
            Console.WriteLine("  [ตก ] ไม่ได้แคปซูลกองไฟ");
            _failed++;
        }
        else
        {
            conn.Send(new PlaceCapsulatedArtifact
            {
                ItemId = capsule.Value.Id,
                Tile = myTile,
                Rotation = Shared.Etc.Rotation.None,
                Floor = null
            });
            Pump(conn, 2000);
        }
        string bonfireId = _placedArtifacts.Count > 0 ? _placedArtifacts[_placedArtifacts.Count - 1] : null;
        Check("วางกองไฟแล้วมีสิ่งปลูกสร้างโผล่", !string.IsNullOrEmpty(bonfireId), bonfireId);

        // ── รอบ 3: มีไฟแต่ไม่มีเครื่องมือ ────────────────────────────────
        Console.WriteLine("รอบ 3 — มีไฟแต่ไม่มีหม้อ");
        Reset();
        conn.Send(new Cheat { _Cheat = "give water 2" });
        Pump(conn, 500);
        Reset();
        Item? meat3 = Find("meat");
        Item? water3 = Find("water");
        conn.Send(new Craft
        {
            RecipeId = "boiled_meat",
            Materials = new Dictionary<string, string[]>
            {
                { "base", new[] { meat3?.Id ?? "" } },
                { "water", new[] { water3?.Id ?? "" } }
            },
            ToolItemId = null,
            Workbench = new PropKey { EntityId = bonfireId, Tile = myTile }
        });
        Pump(conn, 1500);
        Check("ต้มเนื้อโดยไม่มีหม้อ ไม่ผ่าน", _aborts > 0 && _crafted.Count == 0,
            $"abort={_aborts} toolNeeded={_toolNeeded ?? "(ไม่มี)"}");

        // ── รอบ 4: ครบเงื่อนไข → ย่างได้จริง ───────────────────────────
        Console.WriteLine("รอบ 4 — ครบเงื่อนไข: ย่างเนื้อที่กองไฟ");
        Reset();
        int rawBefore = CountRaw("meat");
        Item? stick = Find("wood_bough");
        Item? meat4 = Find("meat");
        conn.Send(new Craft
        {
            RecipeId = "skewer",
            Materials = new Dictionary<string, string[]> { { "base", new[] { meat4?.Id ?? "" } } },
            ToolItemId = stick?.Id,
            Workbench = new PropKey { EntityId = bonfireId, Tile = myTile }
        });
        Pump(conn, 3000);
        Check("ย่างที่กองไฟสำเร็จ", _crafted.Count > 0, $"ได้ {_crafted.Count} ชิ้น · abort={_aborts} · {_info.Trim()}");
        string cookedProto = _crafted.Count > 0 ? _crafted[0].Prototype : null;
        string cookedItemId = _crafted.Count > 0 ? _crafted[0].Id : null;
        Check("ได้ของที่ 'สุกแล้ว' ไม่ใช่เนื้อดิบ",
            cookedProto != null && (cookedProto == "skewer_meat" || !HasTag(_crafted[0], "raw_food")),
            $"prototype={cookedProto} tags={DescribeTags(_crafted.Count > 0 ? _crafted[0] : default)}");
        // แปรรูปในตัวเอง = prototype ไม่เปลี่ยน ต้องนับ "ก้อนที่ยังดิบ" ว่าลดลงจริงไหม
        Check("เนื้อดิบถูกใช้ไป 1 ก้อน", CountRaw("meat") == rawBefore - 1,
            $"ดิบก่อน {rawBefore} หลัง {CountRaw("meat")} · รวมเนื้อ {CountOf("meat")} ก้อน");

        // ── รอบ 5: ของสุกให้พลังมากกว่าของดิบ ──────────────────────────
        Console.WriteLine("รอบ 5 — กินดิบ vs กินสุก");
        conn.Send(new Cheat { _Cheat = "tired" });
        Pump(conn, 500);
        _stamina = 0f;
        Item? rawMeat = Find("meat");
        Reset();
        conn.Send(new UseItem { ItemId = rawMeat?.Id ?? "" });
        Pump(conn, 1500);
        float afterRaw = _stamina;
        Check("กินเนื้อดิบแล้วได้สตามินา", afterRaw > 0f, $"สตามินา {afterRaw:F1}");

        // เวลาย่อย: กินซ้ำทันทีต้องไม่ผ่าน
        Reset();
        Item? rawMeat2 = Find("meat");
        conn.Send(new UseItem { ItemId = rawMeat2?.Id ?? "" });
        Pump(conn, 800);
        Check("กินติดกันทันทีไม่ได้ (เวลาย่อย)", _aborts > 0, $"abort={_aborts} · {_info.Trim()}");

        Thread.Sleep(6000);         // รอให้ย่อยเสร็จ (digestivetime ของเนื้อ = 5 วิ)
        conn.Send(new Cheat { _Cheat = "tired" });
        Pump(conn, 600);
        _stamina = 0f;
        Reset();
        // ต้องกิน **ชิ้นที่เพิ่งย่าง** ไม่ใช่ชิ้นไหนก็ได้ที่ prototype เดียวกัน (ดิบกับสุกใช้ prototype เดียวกัน)
        conn.Send(new UseItem { ItemId = cookedItemId ?? "" });
        Pump(conn, 1500);
        float afterCooked = _stamina;
        Check("ของสุกให้พลังมากกว่าของดิบ", afterCooked > afterRaw,
            $"ดิบ {afterRaw:F1} · สุก {afterCooked:F1}");

        // ── รอบ 6: เตาแรงไม่พอ ทำสูตรยากไม่ได้ ─────────────────────────
        Console.WriteLine("รอบ 6 — น้ำซุปต้องใช้กองไฟใหญ่ (cook 40)");
        conn.Send(new Cheat { _Cheat = "give meat 3" });
        Pump(conn, 500);
        conn.Send(new Cheat { _Cheat = "give pot_02 1" });
        Pump(conn, 500);
        Reset();
        string[] threeMeat = _inventory.Where(x => x.Prototype == "meat").Take(3).Select(x => x.Id).ToArray();
        Item? pot = Find("pot_02");
        Item? water = _inventory.FirstOrDefault(x => (x.Prototype ?? "").Contains("water")) is Item w && w.Id != null ? w : (Item?)null;
        conn.Send(new Craft
        {
            RecipeId = "broth",
            Materials = new Dictionary<string, string[]>
            {
                { "main", threeMeat },
                { "water", new[] { water?.Id ?? "" } }
            },
            ToolItemId = pot?.Id,
            Workbench = new PropKey { EntityId = bonfireId, Tile = myTile }
        });
        Pump(conn, 2000);
        Check("ต้มน้ำซุปที่กองไฟธรรมดา ไม่ผ่าน", _crafted.Count == 0 && _aborts > 0,
            $"abort={_aborts} ได้ {_crafted.Count} ชิ้น · {_info.Trim()}");

        conn.Close();
        Console.WriteLine($"\n=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }

    private static bool HasTag(in Item item, string tag)
    {
        Messages.Tag[] tags = item.Tags;
        if (tags == null)
        {
            return false;
        }
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == tag)
            {
                return true;
            }
        }
        return false;
    }

    private static string DescribeTags(in Item item)
    {
        Messages.Tag[] tags = item.Tags;
        if (tags == null || tags.Length == 0)
        {
            return "(ไม่มี)";
        }
        return string.Join(",", tags.Select(t => t.Id));
    }
}
