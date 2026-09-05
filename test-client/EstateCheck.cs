using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Rights = Shared.Estate.AccessRights;

namespace DurangoTestClient;

/// <summary>
/// เทส "ระบบที่ดิน" (estate) ให้ครบก่อนเปิดให้ผู้เล่นช่วยหาบั๊ก
///
/// ครอบคลุม:
///  1. ประกาศที่ดิน 2×2 · ประกาศซ้ำไม่ได้
///  2. คนที่สองประกาศทับแปลงคนแรกไม่ได้
///  3. ขยายได้เฉพาะช่องที่ติดกัน · ขยายทับแปลงคนอื่นไม่ได้ · มีเพดานจำนวนช่อง
///  4. หดกลับได้ แต่เล็กกว่า 2×2 ไม่ได้
///  5. สิทธิ์ (SetEstateLicense) เก็บจริงและ **บังคับใช้จริง** — คนนอกสร้างของบนที่ดินเราไม่ได้
///     จนกว่าจะให้สิทธิ์ Occupy
///  6. ต่ออายุค่าดูแล (ExtendEstateActivation) เลื่อน DepositRunsOutAt ออกไป
///  7. วาร์ปกลับบ้าน (ReturnToEstate) / ไปเยี่ยม (VisitEstate)
///  8. EstateGrids ถูก broadcast ให้คนอื่นเห็นตอนประกาศ/ขยาย/สละ
///  9. สละที่ดิน (RemoveEstate) แล้วประกาศใหม่ได้ · คนอื่นมาจองที่เดิมได้
/// 10. หน่วยของ LargestPersonalEstateSize ตรงกับ Size (client โชว์ "size / largest")
///
/// รัน: dotnet run -- --estate-check [host] [port เกม] [port gateway]
/// ต้องเปิดเซิร์ฟด้วย Features.LandPermission = true (data/config.json)
/// </summary>
public static class EstateCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    private sealed class Client
    {
        public string Id;
        public Socket Sock;
        public Connection Conn;
        public bool Welcomed;
        public int Aborts;
        public int Oks;
        public readonly List<string> Infos = new List<string>();
        public readonly List<EstateLicense> Licenses = new List<EstateLicense>();
        public readonly List<EstateGrids> Grids = new List<EstateGrids>();
        public EstateLicenses? LastLicenses;
        public PioneerGradeInfo? LastGrade;
        public string EstateId;
        public string LastArtifactId;

        public void Reset()
        {
            Aborts = 0; Oks = 0;
            Infos.Clear(); Licenses.Clear(); Grids.Clear();
            LastLicenses = null; LastGrade = null;
        }

        public string InfoText() => string.Join(" | ", Infos);
    }

    private static void PumpAll(List<Client> cs, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            for (int c = 0; c < cs.Count; c++) cs[c].Conn.Process();
            Thread.Sleep(10);
        }
    }

    /// <summary>
    /// สร้างตัวละครจริงผ่าน POST /players ก่อน — ถ้าขอ token ด้วย id ที่ยังไม่มีไฟล์เซฟ
    /// gateway จะถือว่า "ยังไม่ได้เลือกตัวละคร" แล้วออก token ผูกกับ id ชั่วคราวแทน ⇒ Auth ตก
    /// </summary>
    private static Client Connect(string host, int gamePort, int gatewayPort, string name)
    {
        string modelInfo =
            "{\"hair\":\"hair_f_01\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"F0D9B7\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":1,\"body_size\":1.0}";
        string id = CreateCharacterCheck.CreatePlayer(host, gatewayPort, name, isMale: false, modelInfo);
        if (string.IsNullOrEmpty(id))
        {
            Console.WriteLine($"สร้างตัวละคร {name} ไม่ได้");
            return null;
        }
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + id + "\"}}");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"ขอ token ให้ {name} ({id}) ไม่ได้");
            return null;
        }
        var c = new Client { Id = id };
        c.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        c.Sock.Connect(host, gamePort);
        c.Conn = new Connection(c.Sock);

        c.Conn.Recv<Welcome>((m, h) => c.Welcomed = true);
        c.Conn.Recv<Clock>((m, h) => { });
        c.Conn.Recv<OK>((m, h) => c.Oks++);
        c.Conn.Recv<Abort>((m, h) => c.Aborts++);
        c.Conn.Recv<Info>((m, h) => { lock (c.Infos) c.Infos.Add(m.Text ?? ""); });
        c.Conn.Recv<EstateLicense>((m, h) => c.Licenses.Add(m));
        c.Conn.Recv<EstateLicenses>((m, h) => c.LastLicenses = m);
        c.Conn.Recv<EstateGrids>((m, h) => c.Grids.Add(m));
        c.Conn.Recv<PioneerGradeInfo>((m, h) => c.LastGrade = m);
        c.Conn.Recv<AppearArtifact>((m, h) => c.LastArtifactId = m.EntityId);
        c.Conn.Recv<Inventory>((m, h) => { });
        c.Conn.Recv<InventoryUpdated>((m, h) => { });
        c.Conn.Recv<Survival>((m, h) => { });
        c.Conn.Recv<SurvivalUpdated>((m, h) => { });
        c.Conn.Recv<Skills>((m, h) => { });
        c.Conn.Recv<Statistics>((m, h) => { });
        c.Conn.Recv<Equipments>((m, h) => { });
        c.Conn.Recv<Points>((m, h) => { });
        c.Conn.Recv<AppearPlayer>((m, h) => { });
        c.Conn.Recv<AppearAnimal>((m, h) => { });
        c.Conn.Recv<DisappearEntity>((m, h) => { });
        c.Conn.Recv<DisappearEntityOnTile>((m, h) => { });
        c.Conn.Recv<Move>((m, h) => { });
        c.Conn.Recv<Chunk>((m, h) => { });
        c.Conn.Recv<DefoggedChunks>((m, h) => { });
        c.Conn.Recv<QuestCategories>((m, h) => { });
        c.Conn.Recv<WalletUpdated>((m, h) => { });
        c.Conn.Recv<Recipes>((m, h) => { });
        c.Conn.Recv<ArtifactBlueprints>((m, h) => { });
        c.Conn.Recv<Messages.Timer>((m, h) => { });
        c.Conn.StartReceive();

        c.Conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        c.Conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "estate-check" });
        return c;
    }

    /// <summary>เลื่อนตัวละครไปยืนกลาง cell ที่ระบุ (หลายอย่างเช็ค IsWithinReach)</summary>
    private static void WalkToUnit(Client c, int unitX, int unitY)
    {
        float tileX = unitX * 4 + 2;
        float tileY = unitY * 4 + 2;
        c.Conn.Send(new Cheat { _Cheat = $"tp {(int)tileX} {(int)tileY}" });
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        _passed = _failed = 0;
        Console.WriteLine($"=== estate check: {host}:{gamePort} — ระบบที่ดิน ===");

        string suffix = Guid.NewGuid().ToString("N")[..6];
        Client a = Connect(host, gamePort, gatewayPort, "estate-a-" + suffix);
        if (a == null) return 2;
        Client b = Connect(host, gamePort, gatewayPort, "estate-b-" + suffix);
        if (b == null) return 2;
        var all = new List<Client> { a, b };

        PumpAll(all, 600);
        a.Conn.Send(default(Ready));
        b.Conn.Send(default(Ready));
        PumpAll(all, 2000);
        Check("ทั้งสองคนเข้าเกมได้", a.Welcomed && b.Welcomed, $"a={a.Welcomed} b={b.Welcomed}");

        // ── 0. ยังไม่มีที่ดิน ────────────────────────────────────────────
        a.Reset();
        a.Conn.Send(default(GetEstateLicenses));
        PumpAll(all, 500);
        Check("ยังไม่มีที่ดิน → PersonalEstate ว่าง",
            a.LastLicenses.HasValue && !a.LastLicenses.Value.PersonalEstate.HasValue,
            $"licenses={a.LastLicenses}");
        Check("LargestPersonalEstateSize เป็นหน่วยช่อง (4) ไม่ใช่ด้าน (2)",
            a.LastLicenses.HasValue && a.LastLicenses.Value.LargestPersonalEstateSize == 4,
            $"largest={a.LastLicenses?.LargestPersonalEstateSize}");

        a.Reset();
        a.Conn.Send(default(GetPioneerGradeInfo));
        PumpAll(all, 400);
        Check("CurrentMaximumEstateSize บอกเพดานจริง (64 ช่อง)",
            a.LastGrade.HasValue && a.LastGrade.Value.CurrentMaximumEstateSize == 64,
            $"max={a.LastGrade?.CurrentMaximumEstateSize}");

        a.Reset();
        a.Conn.Send(new ReturnToEstate { OwnerType = Shared.Estate.OwnerType.Player });
        PumpAll(all, 400);
        Check("ยังไม่มีที่ดิน → วาร์ปกลับบ้านไม่ได้ และบอกเหตุผล",
            a.InfoText().Contains("ยังไม่มีที่ดิน") && a.Oks == 0,
            $"info={a.InfoText()} ok={a.Oks}");

        // ── 1. ประกาศที่ดิน ─────────────────────────────────────────────
        // เลือกจุดที่ห่างจากแปลงเดิมในเซฟ — ใช้ hash ของ suffix กันชนกันเวลารันซ้ำ
        int baseX = 20 + (Math.Abs(suffix.GetHashCode()) % 8) * 3;
        int baseY = 20 + (Math.Abs(suffix.GetHashCode() / 7) % 8) * 3;

        a.Reset(); b.Reset();
        WalkToUnit(a, baseX, baseY);
        PumpAll(all, 400);
        a.Reset(); b.Reset();
        a.Conn.Send(new DeclareEstate { OwnerType = Shared.Estate.OwnerType.Player, Cell = new Point2(baseX, baseY) });
        PumpAll(all, 800);
        Check("ประกาศที่ดินได้ และได้ใบสิทธิ์กลับมา", a.Licenses.Count >= 1, $"licenses={a.Licenses.Count} info={a.InfoText()}");
        if (a.Licenses.Count == 0)
        {
            Console.WriteLine("!! ประกาศที่ดินไม่สำเร็จ — เทสต่อไม่ได้ (เปิด Features.LandPermission หรือยัง)");
            Console.WriteLine($"สรุป: ผ่าน {_passed} · ตก {_failed}");
            return _failed == 0 ? 0 : 1;
        }
        EstateLicense lic = a.Licenses[^1];
        a.EstateId = lic.EstateId;
        Check("แปลงแรกได้ 4 ช่อง (2×2)", lic.Size == 4, $"size={lic.Size}");
        Check("ใบสิทธิ์มี DepositRunsOutAt (ค่าดูแล 7 วัน)",
            lic.DepositRunsOutAt.HasValue && lic.DepositRunsOutAt.Value > Times.UnixTimeNow(),
            $"until={lic.DepositRunsOutAt}");
        Check("คนอื่นได้รับ EstateGrids ตอนมีคนประกาศที่ดิน", b.Grids.Count >= 1, $"grids={b.Grids.Count}");
        if (b.Grids.Count > 0)
        {
            EstateGrids g = b.Grids[^1];
            Check("EstateGrids มี 4 ช่องของแปลงใหม่",
                g.Cells != null && CountCellsOf(g, lic.EstateId) == 4,
                $"cells={(g.Cells == null ? -1 : CountCellsOf(g, lic.EstateId))}");
        }

        a.Reset();
        a.Conn.Send(default(GetEstateLicenses));
        PumpAll(all, 500);
        Check("GetEstateLicenses คืนแปลงของเราหลังประกาศ",
            a.LastLicenses.HasValue && a.LastLicenses.Value.PersonalEstate.HasValue
            && a.LastLicenses.Value.PersonalEstate.Value.EstateId == a.EstateId,
            $"licenses={a.LastLicenses}");
        Check("largest ตรงกับ size หลังประกาศ",
            a.LastLicenses.HasValue && a.LastLicenses.Value.LargestPersonalEstateSize == 4,
            $"largest={a.LastLicenses?.LargestPersonalEstateSize}");

        // ประกาศซ้ำ
        a.Reset();
        a.Conn.Send(new DeclareEstate { OwnerType = Shared.Estate.OwnerType.Player, Cell = new Point2(baseX + 10, baseY + 10) });
        PumpAll(all, 600);
        Check("ประกาศที่ดินแปลงที่สองไม่ได้", a.Licenses.Count == 0 && a.InfoText().Contains("มีที่ดินอยู่แล้ว"),
            $"licenses={a.Licenses.Count} info={a.InfoText()}");

        // ── 2. คนที่สองจองทับ ───────────────────────────────────────────
        b.Reset();
        // แปลงเริ่มต้นเป็น 2×2 (ครอบ dx/dy 0..1) — ต้องเล็งช่องที่อยู่ในแปลงจริง ๆ
        b.Conn.Send(new DeclareEstate { OwnerType = Shared.Estate.OwnerType.Player, Cell = new Point2(baseX + 1, baseY + 1) });
        PumpAll(all, 600);
        Check("คนอื่นจองทับแปลงเราไม่ได้", b.Licenses.Count == 0 && b.InfoText().Contains("ที่ดินคนอื่น"),
            $"licenses={b.Licenses.Count} info={b.InfoText()}");

        // ── 3. ขยาย ────────────────────────────────────────────────────
        a.Reset();
        a.Conn.Send(new ExpandEstate { EstateId = a.EstateId, Cell = new Point2(baseX + 9, baseY + 9) });
        PumpAll(all, 500);
        Check("ขยายไปช่องที่ไม่ติดกันไม่ได้", a.Licenses.Count == 0 && a.InfoText().Contains("ติดกับที่ดิน"),
            $"info={a.InfoText()}");

        a.Reset(); b.Reset();
        a.Conn.Send(new ExpandEstate { EstateId = a.EstateId, Cell = new Point2(baseX + 2, baseY) });
        PumpAll(all, 600);
        Check("ขยายไปช่องที่ติดกันได้ (4 → 5)",
            a.Licenses.Count >= 1 && a.Licenses[^1].Size == 5,
            $"size={(a.Licenses.Count > 0 ? a.Licenses[^1].Size : -1)} info={a.InfoText()}");
        Check("คนอื่นได้รับ EstateGrids ตอนขยาย", b.Grids.Count >= 1, $"grids={b.Grids.Count}");

        a.Reset();
        a.Conn.Send(new ExpandEstate { EstateId = a.EstateId, Cell = new Point2(baseX + 2, baseY) });
        PumpAll(all, 500);
        Check("ขยายช่องเดิมซ้ำไม่ได้", a.Licenses.Count == 0 && a.InfoText().Contains("อยู่ในที่ดินแล้ว"),
            $"info={a.InfoText()}");

        a.Reset();
        a.Conn.Send(new ExpandEstate { EstateId = "ไม่มีแปลงนี้", Cell = new Point2(baseX + 2, baseY + 1) });
        PumpAll(all, 500);
        Check("ขยายแปลงที่ไม่มีจริงไม่ได้", a.Licenses.Count == 0 && a.InfoText().Contains("ไม่พบที่ดิน"),
            $"info={a.InfoText()}");

        // ขยายจนชนเพดาน (64 ช่อง) — ตอนนี้มี 17 ช่อง ต่ออีก 47 ช่องให้เต็ม 8×8
        a.Reset();
        // เรียง row-major: ทุกช่องใหม่ติดกับช่องที่เพิ่งเพิ่ม (หรือช่องแถวบน) เสมอ
        for (int dy = 0; dy < 8; dy++)
        {
            for (int dx = 0; dx < 8; dx++)
            {
                a.Conn.Send(new ExpandEstate { EstateId = a.EstateId, Cell = new Point2(baseX + dx, baseY + dy) });
            }
        }
        PumpAll(all, 4000);
        int biggest = 0;
        for (int i = 0; i < a.Licenses.Count; i++) biggest = Math.Max(biggest, a.Licenses[i].Size);
        Check("ขยายได้ถึงเพดาน 64 ช่อง", biggest == 64, $"biggest={biggest} info={a.InfoText()}");

        // ยิงต่ออีกสองช่อง (ติดกันจริง แต่เต็มเพดานแล้ว) — ต้องถูกปฏิเสธ ไม่ใช่โตต่อ
        a.Reset();
        a.Conn.Send(new ExpandEstate { EstateId = a.EstateId, Cell = new Point2(baseX + 8, baseY) });
        a.Conn.Send(new ExpandEstate { EstateId = a.EstateId, Cell = new Point2(baseX + 8, baseY + 1) });
        PumpAll(all, 800);
        int over = 0;
        for (int i = 0; i < a.Licenses.Count; i++) over = Math.Max(over, a.Licenses[i].Size);
        Check("ขยายเกิน 64 ช่องไม่ได้ (กันคนเดียวกินทั้งเกาะ)",
            over <= 64 && a.InfoText().Contains("สูงสุด"),
            $"size={over} info={a.InfoText()}");
        biggest = Math.Max(biggest, over);

        // ── 4. หด ──────────────────────────────────────────────────────
        a.Reset();
        a.Conn.Send(new ShrinkEstate { EstateId = a.EstateId, Cell = new Point2(baseX + 4, baseY) });
        PumpAll(all, 500);
        Check("หดที่ดินได้", a.Licenses.Count >= 1 && a.Licenses[^1].Size < biggest,
            $"size={(a.Licenses.Count > 0 ? a.Licenses[^1].Size : -1)} info={a.InfoText()}");

        // หดลงจนเหลือ 16 แล้วหดอีกต้องไม่ได้
        a.Reset();
        for (int dx = 0; dx < 8; dx++)
            for (int dy = 0; dy < 8; dy++)
                a.Conn.Send(new ShrinkEstate { EstateId = a.EstateId, Cell = new Point2(baseX + dx, baseY + dy) });
        PumpAll(all, 2500);
        int smallest = int.MaxValue;
        for (int i = 0; i < a.Licenses.Count; i++) smallest = Math.Min(smallest, a.Licenses[i].Size);
        Check("หดต่ำกว่า 4 ช่อง (2×2) ไม่ได้",
            smallest >= 4 && a.InfoText().Contains("เล็กกว่า"),
            $"smallest={smallest} info={a.InfoText()}");

        // ── 5. สิทธิ์ — คนนอกสร้างของบนที่ดินเรา ──────────────────────
        // รีเซ็ตแปลงกลับเป็น 2×2 ที่จุดเดิมก่อน (ลูปหดข้างบนตัดช่องมุมซ้ายบนออกไปแล้ว)
        a.Reset();
        a.Conn.Send(new RemoveEstate { EstateId = a.EstateId });
        PumpAll(all, 600);
        a.Reset();
        a.Conn.Send(new DeclareEstate { OwnerType = Shared.Estate.OwnerType.Player, Cell = new Point2(baseX, baseY) });
        PumpAll(all, 800);
        Check("ประกาศแปลงใหม่ที่จุดเดิมได้หลังสละ", a.Licenses.Count >= 1, $"info={a.InfoText()}");
        if (a.Licenses.Count > 0) a.EstateId = a.Licenses[^1].EstateId;

        // ให้ B ไปยืนบนแปลงของ A แล้วลองจองที่สร้าง
        b.Reset();
        WalkToUnit(b, baseX + 1, baseY + 1);
        PumpAll(all, 500);
        b.Reset();
        b.Conn.Send(new OccupyArtifactSite
        {
            BlueprintId = "fur_box_03_leaf",
            Tile = new Point2((baseX + 1) * 4 + 1, (baseY + 1) * 4 + 1),
            Rotation = default,
            Floor = 0
        });
        PumpAll(all, 900);
        Check("คนนอกสร้างของบนที่ดินเราไม่ได้ (ยังไม่ให้สิทธิ์ Occupy)",
            b.Aborts >= 1 && b.InfoText().Contains("ที่ดินของ"),
            $"abort={b.Aborts} info={b.InfoText()}");

        // เจ้าของสร้างบนที่ดินตัวเองได้
        a.Reset();
        WalkToUnit(a, baseX + 1, baseY + 1);
        PumpAll(all, 400);
        a.Reset();
        a.Conn.Send(new OccupyArtifactSite
        {
            BlueprintId = "fur_box_03_leaf",
            Tile = new Point2((baseX + 1) * 4 + 2, (baseY + 1) * 4 + 2),
            Rotation = default,
            Floor = 0
        });
        PumpAll(all, 900);
        Check("เจ้าของสร้างบนที่ดินตัวเองได้",
            !a.InfoText().Contains("ที่ดินของ"),
            $"abort={a.Aborts} info={a.InfoText()}");

        // เปิดสิทธิ์ Occupy ให้คนนอก แล้วลองใหม่
        a.Reset();
        a.Conn.Send(new SetEstateLicense
        {
            EstateId = a.EstateId,
            AccessRights = new Messages.AccessRights
            {
                ForOthers = Rights.Enter | Rights.UseFacility | Rights.Occupy,
                ForFriends = new Dictionary<Shared.Player.FriendType, Rights>
                {
                    [Shared.Player.FriendType.JustFriend] = Rights.Enter | Rights.UseFacility | Rights.Give | Rights.Occupy
                },
                ForClanMembers = null
            }
        });
        PumpAll(all, 600);
        Check("ตั้งสิทธิ์ที่ดินได้ (ตอบ OK)", a.Oks >= 1, $"ok={a.Oks} info={a.InfoText()}");

        a.Reset();
        a.Conn.Send(new GetEstateLicenseById { EstateId = a.EstateId });
        PumpAll(all, 500);
        Check("อ่านสิทธิ์กลับมาได้ตามที่ตั้ง",
            a.Licenses.Count >= 1 && a.Licenses[^1].AccessRights.HasValue
            && (a.Licenses[^1].AccessRights.Value.ForOthers & Rights.Occupy) == Rights.Occupy,
            $"rights={(a.Licenses.Count > 0 ? a.Licenses[^1].AccessRights?.ForOthers.ToString() : "-")}");

        b.Reset();
        b.Conn.Send(new OccupyArtifactSite
        {
            BlueprintId = "fur_box_03_leaf",
            Tile = new Point2((baseX + 1) * 4 + 3, (baseY + 1) * 4 + 3),
            Rotation = default,
            Floor = 0
        });
        PumpAll(all, 900);
        Check("ให้สิทธิ์ Occupy แล้วคนนอกไม่ติดด่านที่ดินอีก",
            !b.InfoText().Contains("ที่ดินของ"),
            $"abort={b.Aborts} info={b.InfoText()}");

        // ── 6. ต่ออายุค่าดูแล ──────────────────────────────────────────
        a.Reset();
        a.Conn.Send(new GetEstateLicenseById { EstateId = a.EstateId });
        PumpAll(all, 400);
        double before = a.Licenses.Count > 0 ? (a.Licenses[^1].DepositRunsOutAt ?? 0) : 0;
        a.Reset();
        a.Conn.Send(new ExtendEstateActivation { EstateId = a.EstateId, Cost = 0 });
        PumpAll(all, 600);
        double after = a.Licenses.Count > 0 ? (a.Licenses[^1].DepositRunsOutAt ?? 0) : 0;
        Check("ต่ออายุค่าดูแลแล้ว DepositRunsOutAt เลื่อนออกไป 7 วัน",
            after > before && Math.Abs((after - before) - 7 * 86400.0) < 60.0,
            $"before={before} after={after} diff={after - before}");

        // ── 7. วาร์ป ───────────────────────────────────────────────────
        a.Reset();
        a.Conn.Send(new ReturnToEstate { OwnerType = Shared.Estate.OwnerType.Player });
        PumpAll(all, 700);
        Check("วาร์ปกลับที่ดินตัวเองได้", a.Oks >= 1, $"ok={a.Oks} info={a.InfoText()}");

        b.Reset();
        b.Conn.Send(new VisitEstate { OwnerId = a.Id, OwnerType = Shared.Estate.OwnerType.Player, RegionId = "" });
        PumpAll(all, 700);
        Check("ไปเยี่ยมที่ดินคนอื่นได้", b.Oks >= 1, $"ok={b.Oks} info={b.InfoText()}");

        b.Reset();
        b.Conn.Send(new VisitEstate { OwnerId = "ไม่มีคนนี้", OwnerType = Shared.Estate.OwnerType.Player, RegionId = "" });
        PumpAll(all, 500);
        Check("ไปเยี่ยมที่ดินที่ไม่มีจริงแล้วได้ข้อความ ไม่ใช่เงียบ",
            b.InfoText().Contains("ไม่พบที่ดิน") && b.Oks == 0,
            $"info={b.InfoText()} ok={b.Oks}");

        // ── 8. คนอื่นสละที่ดินเราไม่ได้ ────────────────────────────────
        b.Reset();
        b.Conn.Send(new RemoveEstate { EstateId = a.EstateId });
        PumpAll(all, 600);
        Check("คนอื่นสละที่ดินของเราไม่ได้", b.Oks == 0 && b.InfoText().Contains("ไม่พบที่ดิน"),
            $"ok={b.Oks} info={b.InfoText()}");

        b.Reset();
        b.Conn.Send(new SetEstateLicense
        {
            EstateId = a.EstateId,
            AccessRights = new Messages.AccessRights { ForOthers = Rights.Destruct, ForFriends = null, ForClanMembers = null }
        });
        PumpAll(all, 600);
        Check("คนอื่นตั้งสิทธิ์ที่ดินของเราไม่ได้", b.Oks == 0 && b.InfoText().Contains("ไม่พบที่ดิน"),
            $"ok={b.Oks} info={b.InfoText()}");

        // ── 9. สละแล้วประกาศใหม่ ───────────────────────────────────────
        a.Reset(); b.Reset();
        a.Conn.Send(new RemoveEstate { EstateId = a.EstateId });
        PumpAll(all, 700);
        Check("สละที่ดินได้", a.Oks >= 1, $"ok={a.Oks} info={a.InfoText()}");
        Check("คนอื่นได้รับ EstateGrids ตอนมีคนสละที่ดิน", b.Grids.Count >= 1, $"grids={b.Grids.Count}");

        b.Reset();
        b.Conn.Send(new DeclareEstate { OwnerType = Shared.Estate.OwnerType.Player, Cell = new Point2(baseX, baseY) });
        PumpAll(all, 800);
        Check("ที่ดินที่ถูกสละแล้ว คนอื่นมาจองต่อได้",
            b.Licenses.Count >= 1 && b.Licenses[^1].Size == 4,
            $"licenses={b.Licenses.Count} info={b.InfoText()}");
        // ── 10. เซฟลงดิสก์ เพื่อเทสว่ารีสตาร์ทแล้วที่ดินยังอยู่ ──────────
        if (b.Licenses.Count > 0)
        {
            b.EstateId = b.Licenses[^1].EstateId;
            b.Reset();
            b.Conn.Send(new Cheat { _Cheat = "save" });
            PumpAll(all, 900);
            Check("บังคับเซฟโลกได้ (ไว้เทียบหลังรีสตาร์ท)", b.InfoText().Contains("เซฟโลกแล้ว"), $"info={b.InfoText()}");
            Console.WriteLine($"[persist] แปลงที่ทิ้งไว้ให้เทสหลังรีสตาร์ท: owner={b.Id} estate={b.EstateId}");
            Console.WriteLine($"[persist] รีสตาร์ทเซิร์ฟแล้วรัน: --estate-reload-check <host> <port เกม> <port gateway> {b.Id}");
        }

        Console.WriteLine($"สรุป: ผ่าน {_passed} · ตก {_failed}");
        try { a.Conn.Close(); b.Conn.Close(); } catch (Exception) { }
        return _failed == 0 ? 0 : 1;
    }

    /// <summary>
    /// เฟสสอง: หลังรีสตาร์ทเซิร์ฟ — ที่ดินที่ประกาศไว้ต้องยังอยู่ครบ (เก็บใน world.json ไม่ใช่เซฟผู้เล่น)
    /// </summary>
    public static int RunReload(string host, int gamePort, int gatewayPort, string entityId)
    {
        _passed = _failed = 0;
        Console.WriteLine($"=== estate reload check: {entityId} ที่ {host}:{gamePort} ===");
        string token = SessionClient.FetchRaw(host, gatewayPort, "{\"appear_player\":{\"entity_id\":\"" + entityId + "\"}}");
        if (string.IsNullOrEmpty(token)) { Console.WriteLine("ขอ token ไม่ได้"); return 2; }

        var c = new Client { Id = entityId };
        c.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        c.Sock.Connect(host, gamePort);
        c.Conn = new Connection(c.Sock);
        c.Conn.Recv<Welcome>((m, h) => c.Welcomed = true);
        c.Conn.Recv<Clock>((m, h) => { });
        c.Conn.Recv<OK>((m, h) => c.Oks++);
        c.Conn.Recv<Abort>((m, h) => c.Aborts++);
        c.Conn.Recv<Info>((m, h) => c.Infos.Add(m.Text ?? ""));
        c.Conn.Recv<EstateLicense>((m, h) => c.Licenses.Add(m));
        c.Conn.Recv<EstateLicenses>((m, h) => c.LastLicenses = m);
        c.Conn.Recv<EstateGrids>((m, h) => c.Grids.Add(m));
        c.Conn.Recv<Inventory>((m, h) => { }); c.Conn.Recv<InventoryUpdated>((m, h) => { });
        c.Conn.Recv<Survival>((m, h) => { }); c.Conn.Recv<SurvivalUpdated>((m, h) => { });
        c.Conn.Recv<Skills>((m, h) => { }); c.Conn.Recv<Statistics>((m, h) => { });
        c.Conn.Recv<Equipments>((m, h) => { }); c.Conn.Recv<Points>((m, h) => { });
        c.Conn.Recv<AppearPlayer>((m, h) => { }); c.Conn.Recv<AppearAnimal>((m, h) => { });
        c.Conn.Recv<AppearArtifact>((m, h) => { }); c.Conn.Recv<Move>((m, h) => { });
        c.Conn.Recv<Chunk>((m, h) => { }); c.Conn.Recv<DefoggedChunks>((m, h) => { });
        c.Conn.Recv<QuestCategories>((m, h) => { }); c.Conn.Recv<WalletUpdated>((m, h) => { });
        c.Conn.Recv<Recipes>((m, h) => { }); c.Conn.Recv<ArtifactBlueprints>((m, h) => { });
        c.Conn.Recv<Messages.Timer>((m, h) => { }); c.Conn.Recv<DisappearEntity>((m, h) => { });
        c.Conn.StartReceive();
        var all = new List<Client> { c };

        c.Conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        c.Conn.Send(new Auth { EntityId = entityId, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "estate-reload" });
        PumpAll(all, 700);
        c.Conn.Send(default(Ready));
        PumpAll(all, 2000);
        Check("เข้าเกมได้หลังรีสตาร์ท", c.Welcomed);

        c.Reset();
        c.Conn.Send(default(GetEstateLicenses));
        PumpAll(all, 700);
        Check("ที่ดินยังอยู่หลังรีสตาร์ท",
            c.LastLicenses.HasValue && c.LastLicenses.Value.PersonalEstate.HasValue,
            $"licenses={c.LastLicenses}");
        Check("ขนาดที่ดินเท่าเดิม (4 ช่อง)",
            c.LastLicenses.HasValue && c.LastLicenses.Value.PersonalEstate.HasValue
            && c.LastLicenses.Value.PersonalEstate.Value.Size == 4,
            $"size={c.LastLicenses?.PersonalEstate?.Size}");
        Check("ค่าดูแลที่ค้างไว้ยังไม่หมดอายุ",
            c.LastLicenses.HasValue && c.LastLicenses.Value.PersonalEstate.HasValue
            && (c.LastLicenses.Value.PersonalEstate.Value.DepositRunsOutAt ?? 0) > Times.UnixTimeNow(),
            $"until={c.LastLicenses?.PersonalEstate?.DepositRunsOutAt}");

        c.Reset();
        c.Conn.Send(new ReturnToEstate { OwnerType = Shared.Estate.OwnerType.Player });
        PumpAll(all, 700);
        Check("วาร์ปกลับที่ดินเดิมได้หลังรีสตาร์ท", c.Oks >= 1, $"ok={c.Oks} info={c.InfoText()}");

        // เก็บกวาด: สละแปลงทิ้งไม่ให้ค้างในเซฟเทส
        c.Reset();
        if (c.LastLicenses?.PersonalEstate != null) { }
        c.Conn.Send(default(GetEstateLicenses));
        PumpAll(all, 500);
        string id = c.LastLicenses?.PersonalEstate?.EstateId;
        if (!string.IsNullOrEmpty(id))
        {
            c.Conn.Send(new RemoveEstate { EstateId = id });
            PumpAll(all, 500);
            c.Conn.Send(new Cheat { _Cheat = "save" });
            PumpAll(all, 700);
        }

        Console.WriteLine($"สรุป: ผ่าน {_passed} · ตก {_failed}");
        try { c.Conn.Close(); } catch (Exception) { }
        return _failed == 0 ? 0 : 1;
    }

    private static int CountCellsOf(EstateGrids grids, string estateId)
    {
        if (grids.Cells == null) return 0;
        int n = 0;
        foreach (KeyValuePair<Point2, string> kv in grids.Cells)
        {
            if (kv.Value == estateId) n++;
        }
        return n;
    }
}
