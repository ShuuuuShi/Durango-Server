using DurangoServer.Modding;

namespace ExampleMod;

/// <summary>
/// Mod ตัวอย่าง — สาธิตทุกจุดของ IModApi ในไฟล์เดียว:
///   1. คำสั่งเอง: พิมพ์ "cheat hello" ในเกม
///   2. คำสั่งที่อ่าน/เขียน state ของ mod เอง: "cheat playtime" (นับวินาทีที่ผู้เล่นออนไลน์สะสม)
///   3. event ผู้เล่นเข้า/ออก: ทักทาย + บอกลาทุกคนที่ออนไลน์อยู่
///   4. OnTick: นับเวลาออนไลน์สะสมของทุกคนแบบเบา ๆ (ไม่ทำงานหนักทุก tick)
/// </summary>
public sealed class ExamplePlugin : IGamePlugin
{
    public string Name => "ExampleMod";
    public string Version => "1.0.0";

    // state ของ mod เอง — เก็บใน mod ได้อิสระ ไม่ต้องผ่านเซิร์ฟ (แต่หายเมื่อรีสตาร์ทเซิร์ฟ
    // ถ้าอยากเซฟถาวรต้องทำเองในไฟล์ของ mod เอง เซิร์ฟไม่มี hook เซฟให้ mod ใน V1 นี้)
    private readonly Dictionary<string, double> _playtimeSeconds = new(StringComparer.OrdinalIgnoreCase);

    public void OnPreLoad(IModApi api)
    {
        // ตัวอย่างนี้ไม่ต้องเตรียมอะไรก่อนเฟส Load — mod อื่นที่อยากให้ mod นี้ "รู้จักชื่อ" ตั้งแต่ก่อน
        // ใครเริ่ม Load จริงค่อยทำอะไรที่นี่ (เช่นประกาศ id ของตัวเองไว้ใน registry กลาง ถ้ามีระบบแบบนั้น)
        api.Log("PreLoad");
    }

    public void OnLoad(IModApi api)
    {
        api.Log("โหลดแล้ว — พิมพ์ 'cheat hello' หรือ 'cheat playtime' ในเกมดูได้เลย (ต้องเปิดเซิร์ฟด้วย --enable-cheat)");

        api.RegisterCommand("hello", (player, args) =>
        {
            return args.Length > 0
                ? $"สวัสดี {player.Name}! ทักมาว่า: {string.Join(' ', args)}"
                : $"สวัสดี {player.Name}! ตอนนี้อยู่ tile {player.TileX},{player.TileY} เลเวล {player.Level}";
        });

        api.RegisterCommand("playtime", (player, _) =>
        {
            double sec = _playtimeSeconds.TryGetValue(player.EntityId, out double s) ? s : 0.0;
            return $"{player.Name} ออนไลน์สะสม (นับจากเซิร์ฟรอบนี้) {sec:F0} วินาที";
        });

        api.OnPlayerJoined(player =>
        {
            _playtimeSeconds.TryAdd(player.EntityId, 0.0);
            api.BroadcastMessage($"[ExampleMod] {player.Name} เข้าเกมแล้ว — ยินดีต้อนรับ!");
        });

        api.OnPlayerLeft(player =>
        {
            api.Log($"{player.Name} ออกไปแล้ว (ออนไลน์สะสม {(_playtimeSeconds.TryGetValue(player.EntityId, out double s) ? s : 0):F0} วิ)");
        });

        api.OnTick(dtSeconds =>
        {
            foreach (IModPlayer p in api.GetOnlinePlayers())
            {
                _playtimeSeconds[p.EntityId] = (_playtimeSeconds.TryGetValue(p.EntityId, out double s) ? s : 0.0) + dtSeconds;
            }
        });
    }

    public void OnPostLoad(IModApi api)
    {
        // ตัวอย่าง: ถ้าอยาก "เช็คว่า mod อื่นลงทะเบียนคำสั่งอะไรไว้บ้าง" ต้องทำตรงนี้ ไม่ใช่ใน OnLoad —
        // ตอน OnLoad ของเรารัน mod อื่นอาจยังไม่ทันลงทะเบียนอะไรเลย (ลำดับ mod ในโฟลเดอร์ไม่รับประกัน)
        // แต่พอถึง OnPostLoad รับประกันว่าทุก mod (รวมของเราเอง) ผ่าน OnLoad หมดแล้ว
        api.Log("PostLoad — พร้อมใช้งานเต็มรูปแบบแล้ว");
    }
}
