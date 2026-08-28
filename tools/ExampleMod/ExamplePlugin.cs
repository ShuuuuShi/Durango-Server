using DurangoServer.Modding;

namespace ExampleMod;

/// <summary>
/// Mod ตัวอย่าง — สาธิตทุกจุดของ IModApi ในไฟล์เดียว:
///   1. คำสั่งเอง: พิมพ์ "cheat hello" ในเกม
///   2. คำสั่งที่อ่าน/เขียน state ของ mod เอง: "cheat playtime" (นับวินาทีที่ผู้เล่นออนไลน์สะสม)
///   3. event ผู้เล่นเข้า/ออก: ทักทาย + บอกลาทุกคนที่ออนไลน์อยู่
///   4. OnTick: นับเวลาออนไลน์สะสมของทุกคนแบบเบา ๆ (ไม่ทำงานหนักทุก tick)
/// </summary>
public sealed class ExamplePlugin : IGamePlugin, IModIdentity
{
    public string Name => "ExampleMod";
    public string Version => "1.1.0";
    public string Id => "examplemod";
    public string ApiVersion => "1.1";
    public IReadOnlyList<string> Dependencies => Array.Empty<string>();

    // state ของ mod เอง — เก็บใน mod ได้อิสระ ไม่ต้องผ่านเซิร์ฟ (แต่หายเมื่อรีสตาร์ทเซิร์ฟ
    // ถ้าอยากเซฟถาวรต้องทำเองในไฟล์ของ mod เอง เซิร์ฟไม่มี hook เซฟให้ mod ใน V1 นี้)
    private readonly Dictionary<string, double> _playtimeSeconds = new(StringComparer.OrdinalIgnoreCase);
    private IModStorage? _storage;

    public void OnPreLoad(IModApi api)
    {
        // ตัวอย่างนี้ไม่ต้องเตรียมอะไรก่อนเฟส Load — mod อื่นที่อยากให้ mod นี้ "รู้จักชื่อ" ตั้งแต่ก่อน
        // ใครเริ่ม Load จริงค่อยทำอะไรที่นี่ (เช่นประกาศ id ของตัวเองไว้ใน registry กลาง ถ้ามีระบบแบบนั้น)
        api.Log("PreLoad");
    }

    public void OnLoad(IModApi api)
    {
        api.Log("โหลดแล้ว — พิมพ์ 'cheat hello' หรือ 'cheat playtime' ในเกมดูได้เลย (ต้องเปิดเซิร์ฟด้วย --enable-cheat)");

        // V2 optional capabilities: mod V1 เดิมยังใช้ IModApi ได้เหมือนเดิม
        if (api is IModEventsApi advanced)
        {
            _storage = advanced.Storage;
            string marker = _storage.LoadJson("install") ?? "{\"loads\":0}";
            _storage.SaveJson("install", marker);
            advanced.Subscribe("craft.completed", e => api.Log("event: craft.completed recipe=" + e.Data["recipe_id"]), EventPriority.Monitor);
            advanced.Subscribe("player.died", e => api.Log("event: player.died id=" + e.EventId), EventPriority.Monitor);
            advanced.Subscribe("player.revived", e => api.Log("event: player.revived"), EventPriority.Monitor);
        }

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

        // [V1.1] มุมมองกระเป๋า — "cheat inv" สรุปของที่ถืออยู่, "cheat give stone 5" สร้างของให้ตัวเอง
        api.RegisterCommand("inv", (player, _) =>
        {
            var summary = player.GetInventorySummary();
            if (summary.Count == 0)
            {
                return "กระเป๋าว่างเปล่า";
            }
            var lines = new List<string>();
            foreach (var kv in summary)
            {
                lines.Add($"{kv.Key} x{kv.Value}");
            }
            lines.Sort();
            return "ของติดตัว: " + string.Join(", ", lines);
        });

        api.RegisterCommand("count", (player, args) =>
        {
            if (args.Length < 1)
            {
                return "ใช้: cheat count <prototype-id> เช่น cheat count stone";
            }
            return $"{player.Name} ถือ {args[0]} อยู่ {player.CountItem(args[0])} ชิ้น";
        });

        // ชื่อ "modgive" (ไม่ใช่ "give") เพราะเซิร์ฟมีคำสั่งในตัว "give" อยู่แล้ว — คำสั่งในตัวเช็คก่อน
        // mod เสมอ ถ้าใช้ชื่อชนกัน mod จะไม่มีวันถูกเรียก (กับดักที่เจอจริงตอนทดสอบ V1.1 นี้เอง)
        api.RegisterCommand("modgive", (player, args) =>
        {
            if (args.Length < 1)
            {
                return "ใช้: cheat modgive <prototype-id> [จำนวน] เช่น cheat modgive stone 5";
            }
            int count = args.Length >= 2 && int.TryParse(args[1], out int c) && c > 0 ? c : 1;
            // GiveItem สร้างของเหมือน "เก็บได้เอง" (durability/tag ตามข้อมูลเกม) แล้ว sync client ทันที
            // name/icon โชว์เป็น prototype-id — mod จริงอยากโชว์ชื่อสวย ๆ ต้องเก็บตารางแปลงเอง
            return player.GiveItem(args[0], count)
                ? $"ให้ {args[0]} x{count} แล้ว (ในกระเป๋าตอนนี้รวม {player.CountItem(args[0])} ชิ้น)"
                : "ใส่ไม่สำเร็จ — ต้องระบุ prototype-id ที่ไม่ว่าง และจำนวน > 0";
        });

        api.OnPlayerJoined(player =>
        {
            _playtimeSeconds.TryAdd(player.EntityId, 0.0);
            api.BroadcastMessage($"[ExampleMod] {player.Name} เข้าเกมแล้ว — ยินดีต้อนรับ!");
        });

        // [V1.1] ตัวอย่าง hook ตาย — ประกาศให้ทั้งเซิร์ฟรู้ (สังเกตว่ายิงครั้งเดียวต่อการตายหนึ่งรอบ)
        api.OnPlayerDied(player =>
        {
            api.BroadcastMessage($"[ExampleMod] ☠ {player.Name} ล้มลงที่ tile {player.TileX},{player.TileY}!");
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
