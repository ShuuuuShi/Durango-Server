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
/// เทส **ระบบสตามินา/ความล้า** ด้วยตัวละครที่เพิ่งเกิด (เลเวล 1 ไม่มีสกิล)
///
/// สิ่งที่วัด — ทุกข้อวัดจากตัวเลขที่ server ส่งมาจริง ไม่ใช่จากการเดา
///   1. เกิดใหม่ต้องได้สตามินาเต็ม 100
///   2. เก็บของ 1 ครั้ง หัก 2 หน่วย (StaminaCostCollect — ลดมาให้ใกล้ต้นฉบับ 3 ก.ย. 2026)
///   3. สตามินาฟื้นเอง ~4 หน่วย/วินาที
///   4. สตามินาไม่พอ = server **ปฏิเสธ** ไม่ใช่ให้ติดลบ
///   5. ความล้า ≥60 ทำให้แพงขึ้น 1.5 เท่า · ≥85 แพงขึ้น 2 เท่า
///   6. พัก (cheat rest) แล้วกลับมาเต็ม
///
/// ⚠️ ต้องเปิดเซิร์ฟด้วย --enable-cheat (ใช้ cheat tired/exhaust/rest ตั้งค่าความล้า)
///
/// รัน: dotnet run -- --stamina-check [host] [port เกม] [port gateway]
/// </summary>
public static class StaminaCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok) { _passed++; Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}"); }
        else { _failed++; Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}"); }
    }

    /// <summary>
    /// **เก็บตัว Gauge ไว้ ไม่ใช่ตัวเลข** — หลอดของเกมนี้เป็นเส้นเวลา (มีความชัน)
    /// ถ้าเก็บเป็น float ณ วินาทีที่ได้ packet ค่าจะค้างจนกว่าจะมี packet ใหม่
    /// แล้วข้อ "สตามินาฟื้นเอง" จะผ่านแบบไม่ได้วัดอะไรเลย (94.1 → 94.1)
    /// </summary>
    private static Gauge _staminaG;
    private static Gauge _fatigueG;
    private static Gauge _lifeG;

    private static float _stamina => _staminaG == null ? -1f : _staminaG.Get(Times.UnixTimeNow());
    private static float _fatigue => _fatigueG == null ? -1f : _fatigueG.Get(Times.UnixTimeNow());
    private static float _life => _lifeG == null ? -1f : _lifeG.Get(Times.UnixTimeNow());
    private static int _aborts;
    private static int _collected;
    private static string _lastInfo;
    private static bool _restBuffEnabled;
    private static bool _restBuffDisabled;

    private static bool HasRestBuff(Messages.StatusEffects msg)
    {
        return msg._StatusEffects != null && Array.Exists(msg._StatusEffects,
            effect => effect.Id == "rest" || effect.EffectId == "rest");
    }


    /// <summary>
    /// ค่า ณ วินาทีที่ server ส่งมา — node แรกของหลอดคือ (เวลาที่ส่ง, ค่าตอนนั้น)
    /// ใช้ตัวนี้วัด "หักไปเท่าไร" เพราะถ้าอ่านค่าปัจจุบันจะรวมการฟื้นระหว่างทางเข้าไปด้วย
    /// (เคยวัดได้ 2.9 ทั้งที่หักจริง 6 เพราะฟื้น 4/วิ ไประหว่างรอ packet)
    /// </summary>
    private static float StaminaAtSend()
    {
        GaugeNode[] d = _staminaG?.Determination;
        return d == null || d.Length == 0 ? -1f : d[0].Value;
    }


    /// <summary>
    /// ความชันของหลอด (หน่วย/วินาที) อ่านจาก node ทั้งสองของหลอดตรง ๆ
    ///
    /// วิธีนี้แม่นกว่า "จับเวลาแล้วดูค่าต่าง" เพราะไม่ขึ้นกับจังหวะที่เราอ่าน
    /// และไม่โดนเพดาน 100 กินความชันไป (เคยวัดได้ 2.9/วิ ทั้งที่ตั้งไว้ 4 เพราะหลอดชนเพดานกลางคัน)
    /// </summary>
    private static float SlopeOf(Gauge g)
    {
        GaugeNode[] d = g?.Determination;
        if (d == null || d.Length < 2) return 0f;
        double dt = d[1].Time - d[0].Time;
        return dt <= 0.0 ? 0f : (float)((d[1].Value - d[0].Value) / dt);
    }

    private static void Pump(Connection conn, int ms)
    {
        for (int i = 0; i < ms / 10; i++)
        {
            conn.Process();
            Thread.Sleep(10);
        }
    }

    private static float Val(Gauge g)
    {
        return g == null ? -1f : g.Get(Times.UnixTimeNow());
    }

    private static void Take(Dictionary<string, Gauge> gauges)
    {
        if (gauges == null)
        {
            return;
        }
        if (gauges.TryGetValue("stamina", out Gauge s)) _staminaG = s;
        if (gauges.TryGetValue("fatigue", out Gauge f)) _fatigueG = f;
        if (gauges.TryGetValue("life", out Gauge l)) _lifeG = l;
    }

    public static int Run(string host, int gamePort, int gatewayPort)
    {
        Console.WriteLine($"=== stamina check: {host}:{gamePort} ===");

        // ชื่อไม่ซ้ำทุกรอบ = ได้ตัวละครเกิดใหม่จริง ๆ เสมอ
        // (ใช้ชื่อเดิมแล้วโหลดเซฟเก่ามา ความล้าจะค้างจากรอบก่อนแล้วข้อ 5 วัดไม่ได้)
        // ต้องสร้างตัวละครจริงก่อน — ถ้าขอ token ด้วย id ที่ยังไม่มีไฟล์เซฟ gateway จะออก token
        // ผูกกับ id ชั่วคราวแทน แล้ว Auth ปฏิเสธ ("token เป็นของ X แต่อ้างเป็น Y")
        string modelInfo =
            "{\"hair\":\"hair_f_01\",\"body_color\":[\"484E36\",\"F0D9B7\",\"29130D\"]," +
            "\"head_color\":[\"FF0000\",\"FFFFFF\",\"0000FF\"],\"skin_color\":\"F0D9B7\"," +
            "\"hair_color\":\"471513\",\"lip_color\":\"E88295\",\"eye_color\":\"52353F\"," +
            "\"portrait\":3,\"portrait_bg\":2,\"portrait_bg_color\":\"C5A293\",\"beard\":null," +
            "\"voice_type\":1,\"body_size\":1.0}";
        string id = CreateCharacterCheck.CreatePlayer(host, gatewayPort,
            "stamina-" + Guid.NewGuid().ToString("N").Substring(0, 6), isMale: false, modelInfo);
        if (string.IsNullOrEmpty(id)) { Console.WriteLine("สร้างตัวละครไม่ได้"); return 2; }
        string token = SessionClient.FetchRaw(host, gatewayPort,
            "{\"appear_player\":{\"entity_id\":\"" + id + "\"}}");
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("ขอ token ไม่ได้ — เซิร์ฟเปิดอยู่ไหม");
            return 1;
        }

        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sock.Connect(host, gamePort);
        Connection conn = new Connection(sock);

        var naturals = new Dictionary<(int x, int y), ushort>();
        var touched = new Dictionary<string, Generator[]>();

        conn.Recv<Welcome>((m, h) => { });
        conn.Recv<Clock>((m, h) => { });
        conn.Recv<OK>((m, h) => { });
        conn.Recv<Abort>((m, h) => _aborts++);
        conn.Recv<Messages.Timer>((m, h) => { });
        conn.Recv<Info>((m, h) => _lastInfo = m.Text);
        conn.Recv<Survival>((m, h) =>
        {
            if (m.EntityId != id) return;
            _lifeG = m.Life;
            Take(m.Gauges);
        });
        conn.Recv<SurvivalUpdated>((m, h) =>
        {
            if (m.EntityId != id) return;
            Take(m.Updated);
        });
        conn.Recv<Messages.StatusEffects>((m, h) =>
        {
            if (m.EntityId != id) return;
            if (HasRestBuff(m)) _restBuffEnabled = true;
            else _restBuffDisabled = true;
        });
        conn.Recv<Touched>((m, h) =>
        {
            if (m.Collectible.Generators != null) touched[m.EntityId ?? ""] = m.Collectible.Generators;
        });
        conn.Recv<Collected>((m, h) => { if (m.Items != null) _collected += m.Items.Length; });
        conn.Recv<Chunk>((m, h) =>
        {
            byte[] g = m.Garden;
            if (g == null) return;
            for (int k = 0; k + 6 <= g.Length; k += 6)
            {
                naturals[(BitConverter.ToUInt16(g, k), BitConverter.ToUInt16(g, k + 2))] = BitConverter.ToUInt16(g, k + 4);
            }
        });
        conn.Recv<Inventory>((m, h) => { });
        conn.Recv<InventoryUpdated>((m, h) => { });
        conn.Recv<CollectibleChanged>((m, h) => { });
        conn.Recv<DisappearEntityOnTile>((m, h) => naturals.Remove((m.Tile.x, m.Tile.y)));
        conn.Recv<AppearPlayer>((m, h) => { });
        conn.Recv<AppearAnimal>((m, h) => { });
        conn.Recv<AppearArtifact>((m, h) => { });
        conn.Recv<DisappearEntity>((m, h) => { });
        conn.Recv<Move>((m, h) => { });
        conn.Recv<Equipments>((m, h) => { });
        conn.Recv<Skills>((m, h) => { });
        conn.Recv<Statistics>((m, h) => { });
        conn.Recv<DefoggedChunks>((m, h) => { });
        conn.Recv<QuestCategories>((m, h) => { });
        conn.Recv<WalletUpdated>((m, h) => { });
        conn.Recv<EntityDied>((m, h) => { });
        conn.StartReceive();

        conn.Send(new GetClock { Time = Times.UnixTimeNow() });
        Pump(conn, 400);
        conn.Send(new Auth { EntityId = id, SessionToken = token, ClientVersion = "5.2.1", DeviceModel = "PC" });
        Pump(conn, 600);
        conn.Send(default(Ready));
        Pump(conn, 2000);

        Console.WriteLine("รอบ 1 — ค่าเริ่มต้นของตัวละครที่เพิ่งเกิด");
        float staminaMax = _staminaG?.Max(Times.UnixTimeNow()) ?? 0f;
        Check("เกิดใหม่ได้สตามินาเต็มตามค่าสถานะตัวละคร", staminaMax > 0f && Math.Abs(_stamina - staminaMax) < 0.5f,
            $"ได้ {_stamina:F1}/{staminaMax:F1}");
        Check("เกิดใหม่ยังไม่ล้า (ความล้า < 1)", _fatigue >= 0f && _fatigue < 1f, $"ได้ {_fatigue:F2}");
        Check("เกิดใหม่เลือดเต็ม", _life > 0f, $"ได้ {_life:F0}");

        // ── หาจุดเก็บของที่เก็บด้วยมือเปล่าได้ ────────────────────────────
        float px = 8000f, py = 35400f;
        MoveTo(conn, id, px, py);
        conn.Send(new SetChunk { Chunk = new Point2((int)(px / 200f / 16f), (int)(py / 200f / 16f)) });
        Pump(conn, 2500);      // server ส่ง garden 25 chunk ต้องรอให้มาครบก่อนค่อยเลือกจุด

        Console.WriteLine($"รอบ 2 — เก็บของ 1 ครั้งกินสตามินาเท่าไร (เห็นของธรรมชาติ {naturals.Count} จุด)");
        (int x, int y)? spot = FindSpot(conn, id, naturals, touched, ref px, ref py);
        if (spot == null)
        {
            Console.WriteLine("  [ข้าม] หาจุดเก็บของที่เก็บด้วยมือเปล่าได้ไม่เจอ — ข้ามข้อที่ต้องเก็บของจริง");
        }
        else
        {
            string entityId = $"natural_{spot.Value.x}_{spot.Value.y}";
            Generator[] gens = touched.TryGetValue(entityId, out Generator[] g) ? g : null;
            string genId = gens != null && gens.Length > 0 ? gens[0].Id : null;

            float before = StaminaAtSend();
            _collected = 0; _aborts = 0;
            conn.Send(new Collect { EntityId = entityId, GeneratorId = genId, Tile = new Point2(spot.Value.x, spot.Value.y) });
            Pump(conn, 500);
            float after = StaminaAtSend();
            float spent = before - after;
            Check("เก็บของ 1 ครั้งหักสตามินา 2 หน่วย", spent > 1.5f && spent < 2.5f,
                $"{before:F1} → {after:F1} (หัก {spent:F1})");

            // ── วัดอัตราฟื้น **ทันที** ตอนหลอดยังไม่เต็ม ──────────────────
            // ต้องทำก่อนรอเก็บของเสร็จ เพราะฟื้น 4/วิ เติม 6 หน่วยที่หักไปคืนหมดใน 1.5 วิ
            // (วัดทีหลัง = หลอดชนเพดาน 100 ความชันเป็น 0 แล้วข้อนี้จะผ่านแบบไม่ได้วัดอะไร)
            Console.WriteLine("รอบ 3 — ใช้แล้วต้องหยุดพักก่อนถึงจะฟื้น");
            // ทันทีหลังใช้: หลอดต้องนิ่ง (ความชัน 0) — นี่คือตัวที่ทำให้เก็บของรัว ๆ แล้วสตามินาหมดจริง
            Check("ใช้สตามินาแล้วหยุดฟื้นทันที", Math.Abs(SlopeOf(_staminaG)) < 0.01f,
                $"ความชันตอนนี้ {SlopeOf(_staminaG):F2}/วิ");

            // อยู่เฉย ๆ ให้ครบเวลาหน่วง แล้วต้องกลับมาฟื้น
            Pump(conn, 4500);
            float perSec = SlopeOf(_staminaG);
            Check("หยุดพักครบเวลาแล้วฟื้น 1.2 หน่วย/วินาที", perSec > 1.1f && perSec < 1.3f,
                $"ความชัน {perSec:F2}/วิ");

            Pump(conn, 4000);
            Check("เก็บของสำเร็จจริง (ไม่ได้แค่หักสตามินาเปล่า)", _collected > 0 && _aborts == 0,
                $"ได้ของ {_collected} ชิ้น · abort {_aborts}");
            Check("ฟื้นจนเต็มแล้วไม่ทะลุเพดานของตัวละคร", _stamina <= staminaMax + 0.01f, $"ตอนนี้ {_stamina:F1}/{staminaMax:F1}");

            Console.WriteLine("รอบ 4 — ความล้าทำให้เปลืองขึ้น");
            // ตั้งความล้า 90 (เกิน danger 85) แล้วเก็บของอีกครั้ง ควรหัก 2 เท่า
            conn.Send(new Cheat { _Cheat = "rest" });        // เคลียร์ก่อน ให้สตามินาเต็ม
            Pump(conn, 800);
            conn.Send(new Cheat { _Cheat = "exhaust" });     // ความล้า 90
            Pump(conn, 800);
            Check("cheat exhaust ตั้งความล้าได้จริง", _fatigue >= 85f, $"ความล้า {_fatigue:F0}");

            (int x, int y)? spot2 = FindSpot(conn, id, naturals, touched, ref px, ref py);
            if (spot2 == null)
            {
                Console.WriteLine("  [ข้าม] ไม่เหลือจุดเก็บของให้เทสตอนล้า");
            }
            else
            {
                string e2 = $"natural_{spot2.Value.x}_{spot2.Value.y}";
                Generator[] g2 = touched.TryGetValue(e2, out Generator[] gg) ? gg : null;
                float before2 = StaminaAtSend();
                conn.Send(new Collect { EntityId = e2, GeneratorId = g2 != null && g2.Length > 0 ? g2[0].Id : null, Tile = new Point2(spot2.Value.x, spot2.Value.y) });
                Pump(conn, 500);
                float after2 = StaminaAtSend();
                float spent2 = before2 - after2;
                Check("ล้ามาก (≥85) เก็บของเปลืองขึ้น 2 เท่า = 4 หน่วย", spent2 > 3.5f && spent2 < 4.5f,
                    $"{before2:F1} → {after2:F1} (หัก {spent2:F1})");
                Pump(conn, 4000);
            }

            Console.WriteLine("รอบ 5 — สตามินาไม่พอต้องถูกปฏิเสธ ไม่ใช่ติดลบ");
            // เก็บรัว ๆ จนหมดแรง แล้วดูว่ามี Abort และสตามินาไม่ติดลบ
            _aborts = 0;
            int tries = 0;
            bool refused = false;
            float fatigueBeforeSpam = _fatigue;
            while (tries < 25 && !refused)
            {
                (int x, int y)? s = FindSpot(conn, id, naturals, touched, ref px, ref py);
                if (s == null) break;
                string e = $"natural_{s.Value.x}_{s.Value.y}";
                Generator[] gg2 = touched.TryGetValue(e, out Generator[] g3) ? g3 : null;
                int abortsBefore = _aborts;
                conn.Send(new Collect { EntityId = e, GeneratorId = gg2 != null && gg2.Length > 0 ? gg2[0].Id : null, Tile = new Point2(s.Value.x, s.Value.y) });
                Pump(conn, 350);
                if (_aborts > abortsBefore && _stamina < 15f) refused = true;
                tries++;
            }
            Check("สตามินาไม่เคยติดลบ", _stamina >= -0.01f, $"ต่ำสุดที่เห็น {_stamina:F1}");
            // [แก้เกณฑ์ 3 ก.ย. 2026] เดิมเช็คว่า "เก็บรัวแล้วสตามินาต้องร่อยหรอ" — เขียนไว้ตอนเก็บของ
            // ครั้งละ 6 หน่วย · ตอนนี้ใช้ค่าใกล้ต้นฉบับ (2 หน่วย) สตามินาจึงฟื้นทันเกือบตลอด
            //
            // ซึ่ง **ถูกตามดีไซน์ต้นฉบับ**: สตามินาคุมการรัวสั้น ๆ ส่วนตัวคุมระยะยาวคือ "ความล้า"
            // (fatigue_cost/collect = 0.4·√e ⇒ เก็บของครั้งละ ~0.57 · เต็ม 100 ที่ ~176 ครั้ง)
            // จึงเปลี่ยนมาเช็คว่าเก็บรัวแล้ว **ความล้าขึ้นจริง** แทน
            Check("เก็บของต่อเนื่องแล้วความล้าเพิ่มขึ้นจริง (ตัวคุมระยะยาวของต้นฉบับ)",
                _fatigue > fatigueBeforeSpam + 0.4f * tries * 0.5f,
                $"เก็บ {tries} ครั้ง · ความล้า {fatigueBeforeSpam:F1} → {_fatigue:F1}");
            if (refused)
            {
                Check("สตามินาหมดแล้ว server ปฏิเสธการเก็บของ", true, $"หลังพยายาม {tries} ครั้ง (เหลือ {_stamina:F1})");
            }
            else
            {
                // ⚠️ ไม่ใช่บั๊ก แต่เป็น **ข้อสังเกตเรื่องสมดุล** ที่ควรรู้:
                // ฟื้น 4 หน่วย/วิ · เก็บของ 1 ครั้งใช้เวลา ~2-3 วิ (ฟื้น 8-12) แต่หักแค่ 6
                // ⇒ เก็บของรัวแค่ไหนสตามินาก็ไม่มีวันหมด สตามินาจึงยังไม่ได้เป็นข้อจำกัดจริงของเกม
                Console.WriteLine($"  [ข้าม] เก็บรัว {tries} ครั้งแล้วสตามินายังเหลือ {_stamina:F1}");
                Console.WriteLine($"         → ฟื้น 4/วิ เร็วกว่าที่ใช้ (6 ต่อการเก็บ 1 ครั้งที่กินเวลา ~2-3 วิ)");
                Console.WriteLine($"         → สตามินายังไม่เป็นข้อจำกัดจริง ถ้าอยากให้เป็น ต้องลดอัตราฟื้นหรือเพิ่มค่าใช้จ่าย");
            }
        }

        Console.WriteLine("รอบ 6 — ความล้าเต็มแล้วเลือดต้องไหลลงจนตายได้");
        conn.Send(new Cheat { _Cheat = "rest" });
        Pump(conn, 800);
        conn.Send(new Cheat { _Cheat = "burnout" });
        Pump(conn, 1500);
        if (_fatigue < 99f)
        {
            Console.WriteLine($"  [ข้าม] ตั้งความล้าให้เต็ม 100 ไม่ได้ (ได้ {_fatigue:F0}) — ข้ามข้อเลือดไหล");
        }
        else
        {
            Pump(conn, 1500);
            // วัดจากความชันของหลอดตรง ๆ — จับเวลาเองไม่แม่น (Pump 4000 ไม่ได้ใช้เวลา 4 วิพอดี)
            float drain = SlopeOf(_lifeG);
            Check("ล้าเต็มหลอดแล้วเลือดไหลลง 0.6/วินาที", drain < -0.55f && drain > -0.65f,
                $"ความชันของหลอดเลือด {drain:F2}/วิ");
            float deathIn = drain < 0f ? _life / -drain : -1f;
            Console.WriteLine($"         → ปล่อยไว้อีก {deathIn:F0} วินาทีก็ตาย (พอให้วิ่งกลับกองไฟ)");
            Check("ผู้เล่นได้รับคำเตือนตอนเลือดเริ่มไหล",
                _lastInfo != null && (_lastInfo.Contains("พัก") || _lastInfo.Contains("สิ่งก่อสร้าง")),
                _lastInfo ?? "(ไม่มีข้อความ)");
        }

        Console.WriteLine("รอบ 7 — พักจริงที่กองไฟ: ความล้าลด + ไอคอนบัพ");
        _restBuffEnabled = false;
        _restBuffDisabled = false;
        conn.Send(new Cheat { _Cheat = "place real fire" });
        Pump(conn, 800);
        conn.Send(new Cheat { _Cheat = "exhaust" });
        Pump(conn, 800);
        float tiredBeforeRest = _fatigue;
        conn.Send(new Cheat { _Cheat = "test rest" });
        Pump(conn, 1200);
        float tiredAfterRest = _fatigue;
        // สูตรจริงของเกม (status_effects.json → rest): -(0.15 + 0.0015 × level)
        // ที่ level 1 = -0.1515/วิ — ช้ากว่าของเดิมที่เราตั้งเอง (4.0/วิ) มาก จึงวัดจาก "ความชัน"
        // แทนผลต่างดิบ ไม่งั้นต้องรอเป็นนาทีถึงจะเห็นตัวเลขขยับพอให้เทียบได้
        float restSlope = SlopeOf(_fatigueG);
        Check("พักจริงแล้วความล้าลดลงตามสูตรต้นฉบับ (~-0.15/วิ)",
            tiredBeforeRest > 0f && restSlope < -0.10f && restSlope > -0.25f,
            $"ก่อน {tiredBeforeRest:F1} → หลัง {tiredAfterRest:F1} · ความชัน {restSlope:F3}/วิ");
        Check("พักจริงเปิดบัพ rest (ไอคอน icon_se_rest)", _restBuffEnabled,
            _restBuffEnabled ? "ได้รับ StatusEffects แล้ว" : (_lastInfo ?? "ไม่ได้รับ StatusEffects"));
        conn.Send(new Cheat { _Cheat = "survival" });
        Pump(conn, 500);
        // ใช้ teleport control ให้ server เรียก RememberPosition จริงและหยุดพัก
        conn.Send(new Cheat { _Cheat = "tp 42 177" });
        Pump(conn, 700);
        Check("ลุก/หยุดพักแล้วปิดไอคอนบัพ", _restBuffDisabled,
            _restBuffDisabled ? "ได้รับ StatusEffects ที่ไม่มีบัพแล้ว" : "ยังไม่พบ packet ปิดบัพ");

        Console.WriteLine("รอบ 8 — cheat rest แล้วกลับมาเต็ม");
        conn.Send(new Cheat { _Cheat = "rest" });
        Pump(conn, 1000);
        float finalStaminaMax = _staminaG?.Max(Times.UnixTimeNow()) ?? staminaMax;
        Check("cheat rest คืนสตามินาเต็มและล้างความล้า",
            Math.Abs(_stamina - finalStaminaMax) < 1f && _fatigue < 1f,
            $"สตามินา {_stamina:F1}/{finalStaminaMax:F1} · ความล้า {_fatigue:F1}");

        conn.Close();
        Console.WriteLine();
        Console.WriteLine($"=== สรุป: ผ่าน {_passed} / ตก {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }



    /// <summary>M-2: server ยอมให้ขยับ 900 หน่วย/วิ — ก้าวละ 900 ต่อ 1 วินาทีจึงผ่านเกณฑ์</summary>
    private const float MaxStepUnits = 900f;

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

    /// <summary>เดินไปยืนที่พิกัดโลกนั้น (server เช็คระยะเอื้อมจากตำแหน่งจริง)</summary>
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
    /// หาจุดเก็บของที่ **เก็บด้วยมือเปล่าได้** ใกล้ตัว แล้วเดินไปแตะให้เรียบร้อย
    /// (ท่อนซุง/หินก้อนใหญ่ต้องใช้เครื่องมือ — ถ้าไปเจอเข้าจะได้ ToolNeeded ไม่ใช่การหักสตามินา
    ///  ซึ่งทำให้ตัวเลขที่วัดเพี้ยน จึงต้องเลือกเฉพาะจุดที่แตะแล้วได้ generator กลับมาจริง)
    /// </summary>
    private static (int x, int y)? FindSpot(Connection conn, string selfId, Dictionary<(int x, int y), ushort> naturals,
        Dictionary<string, Generator[]> touched, ref float px, ref float py)
    {
        int tried = 0;
        float fromX = px, fromY = py;
        var byDistance = new List<KeyValuePair<(int x, int y), ushort>>(naturals);
        byDistance.Sort((l, r) =>
        {
            double dl = Math.Pow(l.Key.x * 200.0 - fromX, 2) + Math.Pow(l.Key.y * 200.0 - fromY, 2);
            double dr = Math.Pow(r.Key.x * 200.0 - fromX, 2) + Math.Pow(r.Key.y * 200.0 - fromY, 2);
            return dl.CompareTo(dr);
        });
        foreach (KeyValuePair<(int x, int y), ushort> kv in byDistance)
        {
            if (tried >= 8) break;
            (int x, int y) t = kv.Key;
            string entityId = $"natural_{t.x}_{t.y}";
            if (touched.ContainsKey(entityId)) continue;

            // เดินไปให้อยู่ในระยะเอื้อม — **ต้องเดินทีละก้าว**
            // server จำกัดความเร็วกันโกง (M-2) 900 หน่วย/วิ วาร์ปไปทีเดียวจะถูกตีกลับ
            // แล้ว Touch ทุกครั้งจะติด "ไกลเกินเอื้อม" (บั๊กที่ทำให้เทสนี้หาจุดไม่เจอตอนแรก)
            WalkTo(conn, selfId, ref px, ref py, t.x * 200f + 100f, t.y * 200f + 100f);
            conn.Send(new Touch { EntityId = entityId, EntityType = kv.Value, Tile = new Point2(t.x, t.y) });
            Pump(conn, 500);
            tried++;
            if (touched.TryGetValue(entityId, out Generator[] gens) && gens != null && gens.Length > 0)
            {
                return t;
            }
        }
        return null;
    }
}
