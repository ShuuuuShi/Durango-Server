using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace GameBot;

/// <summary>
/// GameBot — บอทเล่นเกมจริง (ตัวเกมที่เปิดอยู่) ผ่าน BotBridge ที่ฝังในตัวเกม
///
/// ไม่แตะเมาส์/คีย์บอร์ดเลย: สั่งเดินผ่าน PlayerController.MoveToPosition, แตะ UI ผ่าน
/// UICamera pipeline เดิม, เก็บ/ตีผ่าน InteractionSystem — ทุกคำสั่งวิ่งผ่านโค้ด
/// ตัวเดียวกับที่ผู้เล่นกดจริง ๆ server จึงโดน packet ของจริงทุกอย่าง (กันโกงยังทำงาน)
///
/// วิธีรัน:
///   dotnet run -- --bot [host] [port] [นาที]          วงจรอัตโนมัติ (เก็บ→กิน→ล่า→แล่)
///   dotnet run -- --console [host] [port]              สั่งเอง (พิมพ์คำสั่งทีละบรรทัด)
///   dotnet run -- --cmd "state;move x=8200 y=35600"    สั่งรวดเดียวแล้วออก
///
/// ดู docs/server/GameBot.md
/// </summary>
public static class Program
{
    private const string DefaultHost = "127.0.0.1";
    private const int DefaultPort = 8192;

    private static TcpClient _client = null!;
    private static NetworkStream _stream = null!;
    private static StreamReader _reader = null!;

    // =====================================================================
    // transport
    // =====================================================================

    private static void Connect(string host, int port)
    {
        _client = new TcpClient();
        _client.Connect(host, port);
        _stream = _client.GetStream();
        _reader = new StreamReader(_stream, new UTF8Encoding(false));
    }

    /// <summary>ส่งคำสั่งไป bridge แล้วคืน JSON ที่ตอบมา (1 บรรทัด)</summary>
    private static string Send(string cmd)
    {
        byte[] buf = Encoding.UTF8.GetBytes(cmd + "\n");
        _stream.Write(buf, 0, buf.Length);
        _stream.Flush();
        string? line = _reader.ReadLine();
        if (line == null) throw new IOException("bridge closed");
        return line;
    }

    private static JsonDocument? SendJson(string cmd)
    {
        string line = Send(cmd);
        try { return JsonDocument.Parse(line); }
        catch { return null; }
    }

    private static bool Ok(JsonDocument? doc)
    {
        return doc != null && doc.RootElement.TryGetProperty("ok", out JsonElement ok) && ok.GetBoolean();
    }

    // =====================================================================
    // state model (ตรงกับที่ BotBridge ส่งมา)
    // =====================================================================

    private sealed class Animal
    {
        public required string Id;
        public int Type;
        public float X, Y;
        public bool Alive;
        public bool Lootable;
    }

    private sealed class Natural
    {
        public required string Id;
        public int Type;
        public int TileX, TileY;
        public float Dist;
        public bool Corpse;
    }

    private sealed class InvItem
    {
        public required string Id;
        public required string Proto;
        public required string Name;
        public int Count;
        public int Size;
        public bool Edible;
    }

    private sealed class GameState
    {
        public string Scene = "";
        public int ScreenW, ScreenH;
        public bool HasPlayer;
        public float Px, Py;
        public int PTileX, PTileY;
        public float Life = -1, LifeMax = 1, Stamina = -1, StaminaMax = 1;
        public bool Alive, Moving;
        public bool Gathering;
        public bool BattleMode;
        public string Pending = "";
        public readonly List<Animal> Animals = new();
        public readonly List<Natural> Naturals = new();
        public readonly List<InvItem> Inv = new();
        public readonly List<(string Action, string Id, bool Disabled)> Menus = new();
        public readonly List<(string Id, float Cd)> BattleActions = new();

        public int TotalItems => Inv.Sum(i => i.Count);
        public float StaminaRatio => StaminaMax > 0 ? Stamina / StaminaMax : 0f;
        public float LifeRatio => LifeMax > 0 ? Life / LifeMax : 0f;
    }

    private static float F(JsonElement e, string key, float def = 0f)
    {
        return e.TryGetProperty(key, out JsonElement v) && v.ValueKind == JsonValueKind.Number
            ? v.GetSingle()
            : def;
    }

    private static GameState ParseState(JsonElement root)
    {
        var st = new GameState
        {
            Scene = S(root, "scene"),
        };
        if (root.TryGetProperty("screen", out JsonElement scr) && scr.ValueKind == JsonValueKind.Array)
        {
            if (scr.GetArrayLength() >= 2) { st.ScreenW = (int)scr[0].GetSingle(); st.ScreenH = (int)scr[1].GetSingle(); }
        }
        if (root.TryGetProperty("player", out JsonElement p) && p.ValueKind == JsonValueKind.Object)
        {
            st.HasPlayer = true;
            if (p.TryGetProperty("pos", out JsonElement pos) && pos.GetArrayLength() >= 2)
            {
                st.Px = pos[0].GetSingle();
                st.Py = pos[1].GetSingle();
            }
            if (p.TryGetProperty("tile", out JsonElement tile) && tile.GetArrayLength() >= 2)
            {
                st.PTileX = (int)tile[0].GetSingle();
                st.PTileY = (int)tile[1].GetSingle();
            }
            if (p.TryGetProperty("life", out JsonElement life) && life.GetArrayLength() >= 2)
            {
                st.Life = life[0].GetSingle();
                st.LifeMax = life[1].GetSingle();
            }
            if (p.TryGetProperty("stamina", out JsonElement stam) && stam.GetArrayLength() >= 2)
            {
                st.Stamina = stam[0].GetSingle();
                st.StaminaMax = stam[1].GetSingle();
            }
            st.Alive = B(p, "alive");
            st.Moving = B(p, "moving");
        }
        st.Gathering = B(root, "gathering");
        st.Pending = S(root, "pending");

        if (root.TryGetProperty("animals", out JsonElement animals))
        {
            foreach (JsonElement a in animals.EnumerateArray())
            {
                st.Animals.Add(new Animal
                {
                    Id = S(a, "id"),
                    Type = I(a, "type"),
                    Alive = B(a, "alive"),
                    Lootable = B(a, "lootable"),
                });
                if (a.TryGetProperty("pos", out JsonElement pos) && pos.GetArrayLength() >= 2)
                {
                    st.Animals[^1].X = pos[0].GetSingle();
                    st.Animals[^1].Y = pos[1].GetSingle();
                }
            }
        }
        if (root.TryGetProperty("naturals", out JsonElement naturals))
        {
            foreach (JsonElement n in naturals.EnumerateArray())
            {
                st.Naturals.Add(new Natural
                {
                    Id = S(n, "id"),
                    Type = I(n, "type"),
                    Dist = F(n, "dist"),
                    Corpse = B(n, "corpse"),
                });
                if (n.TryGetProperty("tile", out JsonElement tile) && tile.GetArrayLength() >= 2)
                {
                    st.Naturals[^1].TileX = (int)tile[0].GetSingle();
                    st.Naturals[^1].TileY = (int)tile[1].GetSingle();
                }
            }
        }
        if (root.TryGetProperty("inv", out JsonElement inv))
        {
            foreach (JsonElement it in inv.EnumerateArray())
            {
                st.Inv.Add(new InvItem
                {
                    Id = S(it, "id"),
                    Proto = S(it, "proto"),
                    Name = S(it, "name"),
                    Count = I(it, "count"),
                    Size = I(it, "size"),
                    Edible = B(it, "edible"),
                });
            }
        }
        if (root.TryGetProperty("menus", out JsonElement menus))
        {
            foreach (JsonElement m in menus.EnumerateArray())
            {
                st.Menus.Add((S(m, "action"), S(m, "id"), B(m, "disabled")));
            }
        }
        if (root.TryGetProperty("battle", out JsonElement battle))
        {
            st.BattleMode = B(battle, "mode");
            if (battle.TryGetProperty("actions", out JsonElement acts))
            {
                foreach (JsonElement a in acts.EnumerateArray())
                {
                    st.BattleActions.Add((S(a, "id"), F(a, "cd")));
                }
            }
        }
        return st;
    }

    private static string S(JsonElement e, string key) =>
        e.TryGetProperty(key, out JsonElement v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    private static int I(JsonElement e, string key) =>
        e.TryGetProperty(key, out JsonElement v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

    private static bool B(JsonElement e, string key) =>
        e.TryGetProperty(key, out JsonElement v) && v.ValueKind == JsonValueKind.True;

    // =====================================================================
    // main
    // =====================================================================

    public static void Main(string[] args)
    {
        string host = DefaultHost;
        int port = DefaultPort;
        var positional = new List<string>();
        var queued = new List<string>();
        bool interactive = true;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--cmd" && i + 1 < args.Length)
            {
                foreach (string part in args[++i].Split(';'))
                {
                    if (!string.IsNullOrWhiteSpace(part)) queued.Add(part.Trim());
                }
                interactive = false;
            }
            else if (args[i] == "--console" || args[i] == "--bot")
            {
                // รูปแบบ: --console host port  /  --bot host port นาที
            }
            else
            {
                positional.Add(args[i]);
            }
        }
        string mode = args.Length > 0 && args[0] is "--bot" or "--console" ? args[0] : "--console";
        if (positional.Count >= 1) host = positional[0];
        if (positional.Count >= 2) port = int.Parse(positional[1]);

        try
        {
            Connect(host, port);
        }
        catch (Exception e)
        {
            Console.WriteLine($"เชื่อม bridge ไม่ได้ ({host}:{port}) — เปิดเกมให้ BotBridge รออยู่ก่อน (ดู docs/server/GameBot.md)");
            Console.WriteLine(e.Message);
            Environment.Exit(2);
            return;
        }

        Console.WriteLine($"GameBot -> bridge {host}:{port}");
        var ping = SendJson("ping");
        if (!Ok(ping))
        {
            Console.WriteLine("bridge ไม่ตอบ ping — เกมยังไม่โหลด BotBridge?");
            Environment.Exit(2);
            return;
        }

        if (mode == "--bot")
        {
            double minutes = positional.Count >= 3 ? double.Parse(positional[2]) : 5.0;
            RunBot(minutes);
        }
        else if (queued.Count > 0)
        {
            RunCmdQueue(queued);
        }
        else
        {
            RunConsole();
        }
    }

    // =====================================================================
    // interactive / one-shot command mode
    // =====================================================================

    private static void RunCmdQueue(List<string> cmds)
    {
        foreach (string cmd in cmds)
        {
            Console.WriteLine("> " + cmd);
            Console.WriteLine(Send(cmd));
        }
    }

    private static void RunConsole()
    {
        Console.WriteLine("พิมพ์คำสั่ง (state / move x= y= / gather / attack / action / use / tap x= y= / stop / menu action= / quit)");
        while (true)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null) break;
            line = line.Trim();
            if (line.Length == 0) continue;
            if (line == "quit" || line == "exit") break;
            try
            {
                Console.WriteLine(Send(line));
            }
            catch (Exception e)
            {
                Console.WriteLine("err: " + e.Message);
                break;
            }
        }
    }

    // =====================================================================
    // AI — วงจรเล่นเกมอัตโนมัติ
    // =====================================================================

    private sealed class BotStats
    {
        public int Moves, Gathers, Collected, Butchers, Attacks, Actions, Eats, Aborts;
        public int GatherOk, HuntOk;
    }

    private static void RunBot(double minutes)
    {
        var stats = new BotStats();
        var badTiles = new HashSet<(int, int)>();
        var badAnimals = new HashSet<string>();

        string? targetId = null;          // สิ่งที่กำลังเดินหา/กำลังเก็บ
        int targetTileX = 0, targetTileY = 0;
        string phase = "idle";            // idle | moveto | waiting | hunting | butchering | eating | wandering
        DateTime phaseAt = DateTime.UtcNow;
        bool sawActivity = false;         // ระหว่าง waiting เห็นเมนู/gathering ขึ้นไหม (แยกงานจริงกับงานล้ม)
        int gatherCountSinceHunt = 0;
        double lastReport = 0;

        DateTime endAt = DateTime.UtcNow.AddMinutes(minutes);
        Console.WriteLine($"=== GameBot วงจรอัตโนมัติ เป็นเวลา {minutes} นาที ===");

        while (DateTime.UtcNow < endAt)
        {
            GameState? st;
            try
            {
                using var doc = SendJson("state");
                if (doc == null || !Ok(doc))
                {
                    Thread.Sleep(1000);
                    continue;
                }
                st = ParseState(doc.RootElement);
            }
            catch
            {
                Console.WriteLine("[bot] bridge หลุด — รอแล้วลองใหม่");
                Thread.Sleep(2000);
                continue;
            }

            double now = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;

            if (!st.HasPlayer)
            {
                Console.WriteLine("[bot] รอเข้าเกม... (scene=" + st.Scene + ")");
                Thread.Sleep(2000);
                continue;
            }

            if (!st.Alive)
            {
                Console.WriteLine("[bot] ตัวละครตาย — รอฟื้น/รีเซ็ต");
                Thread.Sleep(5000);
                continue;
            }

            // ---- กินเมื่อสตามินา/เลือดต่ำหรือของเต็ม ----
            if (st.Inv.Count > 0 && (st.StaminaRatio < 0.3f || st.LifeRatio < 0.5f))
            {
                var edible = st.Inv.FirstOrDefault(i => i.Edible);
                if (edible != null && phase != "eating")
                {
                    phase = "eating";
                    phaseAt = DateTime.UtcNow;
                    Console.WriteLine($"[bot] กิน {edible.Name} (stamina={st.Stamina:F0} life={st.Life:F0})");
                    Send($"use id={edible.Id}");
                    stats.Eats++;
                }
            }
            if (phase == "eating")
            {
                if (st.StaminaRatio >= 0.7f || (DateTime.UtcNow - phaseAt).TotalSeconds > 12)
                {
                    phase = "idle";
                    Console.WriteLine($"[bot] กินเสร็จ stamina={st.Stamina:F0}");
                }
                else
                {
                    Thread.Sleep(1500);
                    continue;
                }
            }

            // ---- ของเต็มจนเก็บไม่ได้ ----
            if (st.TotalItems >= 45)
            {
                var edible = st.Inv.FirstOrDefault(i => i.Edible);
                if (edible == null)
                {
                    Console.WriteLine($"[bot] กระเป๋าเต็ม ({st.TotalItems} ชิ้น) ไม่มีของกิน — หยุดพัก");
                    Thread.Sleep(10000);
                    continue;
                }
            }

            float distToTarget = float.MaxValue;
            if (targetId != null)
            {
                float tx = targetTileX * 200f + 100f;
                float ty = targetTileY * 200f + 100f;
                distToTarget = MathF.Sqrt((st.Px - tx) * (st.Px - tx) + (st.Py - ty) * (st.Py - ty));
            }

            // ==========================================================
            // state machine
            // ==========================================================
            switch (phase)
            {
                case "moveto":
                {
                    if (distToTarget < 80f)
                    {
                        phase = "interact";
                        phaseAt = DateTime.UtcNow;
                        break;
                    }
                    if (st.Moving || distToTarget < 200f)
                    {
                        // กำลังเดินหรือใกล้แล้ว — รอ
                        Thread.Sleep(600);
                        break;
                    }
                    if ((DateTime.UtcNow - phaseAt).TotalSeconds > 2.0)
                    {
                        // เดินแล้วไม่ได้ขยับ (ติดอะไร?) — ลองสั่งใหม่ครั้งเดียว
                        SendMove(st, targetTileX, targetTileY);
                        stats.Moves++;
                        phaseAt = DateTime.UtcNow;
                        Thread.Sleep(600);
                        break;
                    }
                    Thread.Sleep(600);
                    break;
                }

                case "interact":
                {
                    bool acted = false;
                    if (targetId != null && targetId.StartsWith("natural_", StringComparison.Ordinal))
                    {
                        acted = Ok(SendJson($"gather id={targetId}"));
                        stats.Gathers++;
                        if (!acted) acted = Ok(SendJson("gather"));
                    }
                    else if (targetId != null && targetId.StartsWith("animal_", StringComparison.Ordinal))
                    {
                        acted = Ok(SendJson($"attack id={targetId}"));
                        if (!acted) acted = Ok(SendJson("attack"));
                        stats.Attacks++;
                    }
                    else
                    {
                        acted = Ok(SendJson("gather"));
                        stats.Gathers++;
                    }
                    if (acted)
                    {
                        phase = "waiting";
                        phaseAt = DateTime.UtcNow;
                    }
                    else
                    {
                        phase = "idle";
                        phaseAt = DateTime.UtcNow;
                    }
                    Thread.Sleep(400);
                    break;
                }

                case "waiting":
                {
                    // รอให้ server ตอบ Touched → เมนูโผล่ → bridge กด Collect/Attack ให้อัตโนมัติ
                    if (st.Gathering || st.Pending.Length > 0 || st.Menus.Count > 0)
                    {
                        sawActivity = true;
                        Thread.Sleep(500);
                        break;
                    }
                    if ((DateTime.UtcNow - phaseAt).TotalSeconds > 1.5)
                    {
                        if (sawActivity)
                        {
                            if (targetId != null && targetId.StartsWith("animal_", StringComparison.Ordinal))
                            {
                                phase = "hunting";
                                phaseAt = DateTime.UtcNow;
                                Console.WriteLine("[bot] เข้าโหมดต่อสู้ — เริ่มตี");
                            }
                            else
                            {
                                stats.GatherOk++;
                                gatherCountSinceHunt++;
                                phase = "idle";
                                phaseAt = DateTime.UtcNow;
                            }
                        }
                        else
                        {
                            Console.WriteLine("[bot] งานไม่เริ่ม (ไม่มีเมนู) — จำจุดนี้ไว้");
                            stats.Aborts++;
                            if (targetId != null && targetId.StartsWith("natural_", StringComparison.Ordinal))
                            {
                                badTiles.Add((targetTileX, targetTileY));
                            }
                            else if (targetId != null && targetId.StartsWith("animal_", StringComparison.Ordinal))
                            {
                                badAnimals.Add(targetId);
                            }
                            phase = "idle";
                            phaseAt = DateTime.UtcNow;
                        }
                        sawActivity = false;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                    break;
                }

                case "hunting":
                {
                    var target = st.Animals.FirstOrDefault(a => a.Id == targetId);
                    if (target == null || !target.Alive)
                    {
                        // สัตว์ตาย/หาย — ไปแล่ซาก
                        var corpse = st.Naturals.FirstOrDefault(n => n.Corpse && n.Dist < 1500f);
                        if (corpse != null)
                        {
                            targetId = corpse.Id;
                            targetTileX = corpse.TileX;
                            targetTileY = corpse.TileY;
                            phase = "moveto";
                            phaseAt = DateTime.UtcNow;
                            SendMove(st, corpse.TileX, corpse.TileY);
                            stats.Moves++;
                            Console.WriteLine($"[bot] สัตว์ตาย — ไปแล่ซาก {corpse.Id}");
                        }
                        else
                        {
                            stats.HuntOk++;
                            gatherCountSinceHunt = 0;
                            
                            phase = "idle";
                            Console.WriteLine("[bot] ล่าเสร็จ (หาซากไม่เจอ — ข้ามไป)");
                        }
                        Thread.Sleep(500);
                        break;
                    }
                    float adx = target.X - st.Px, ady = target.Y - st.Py;
                    float adist = MathF.Sqrt(adx * adx + ady * ady);
                    if (adist > 500f)
                    {
                        targetTileX = (int)(target.X / 200f);
                        targetTileY = (int)(target.Y / 200f);
                        phase = "moveto";
                        phaseAt = DateTime.UtcNow;
                        SendMove(st, targetTileX, targetTileY);
                        stats.Moves++;
                        Thread.Sleep(400);
                        break;
                    }
                    if (!st.BattleMode)
                    {
                        // ยังไม่ได้เข้าโหมดต่อสู้ — ลองกด attack ใหม่
                        Send($"attack id={target.Id}");
                        stats.Attacks++;
                        Thread.Sleep(800);
                        break;
                    }
                    var usable = st.BattleActions.FirstOrDefault(a => a.Cd <= 0.5f);
                    if (usable.Id.Length > 0)
                    {
                        Send($"action id={usable.Id}");
                        stats.Actions++;
                    }
                    else
                    {
                        Send("action");
                        stats.Actions++;
                    }
                    Thread.Sleep(1500);
                    break;
                }

                case "butchering":
                {
                    if (!st.Gathering && st.Pending.Length == 0)
                    {
                        stats.GatherOk++;
                        gatherCountSinceHunt++;
                        
                        phase = "idle";
                        Console.WriteLine($"[bot] แล่ซากเสร็จ — ของในกระเป๋า {st.TotalItems} ชิ้น");
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                    break;
                }

                case "wandering":
                {
                    if (!st.Moving && distToTarget < 120f)
                    {
                        phase = "idle";
                        Thread.Sleep(300);
                    }
                    else
                    {
                        Thread.Sleep(600);
                    }
                    break;
                }

                default: // idle — หางานต่อไป
                {
                    if (gatherCountSinceHunt >= 3)
                    {
                        // ล่าสัตว์บ้างเป็นระยะ
                        var animal = st.Animals
                            .Where(a => a.Alive && !badAnimals.Contains(a.Id))
                            .OrderBy(a => (a.X - st.Px) * (a.X - st.Px) + (a.Y - st.Py) * (a.Y - st.Py))
                            .FirstOrDefault();
                        if (animal != null)
                        {
                            targetId = animal.Id;
                            targetTileX = (int)(animal.X / 200f);
                            targetTileY = (int)(animal.Y / 200f);
                            phase = "moveto";
                            phaseAt = DateTime.UtcNow;
                            SendMove(st, targetTileX, targetTileY);
                            stats.Moves++;
                            gatherCountSinceHunt = 0;
                            Console.WriteLine($"[bot] ไปล่า {animal.Id} (type={animal.Type})");
                            break;
                        }
                    }

                    var nat = st.Naturals
                        .Where(n => !n.Corpse && !badTiles.Contains((n.TileX, n.TileY)))
                        .OrderBy(n => n.Dist)
                        .FirstOrDefault();
                    if (nat != null)
                    {
                        targetId = nat.Id;
                        targetTileX = nat.TileX;
                        targetTileY = nat.TileY;
                        phase = "moveto";
                        phaseAt = DateTime.UtcNow;
                        SendMove(st, nat.TileX, nat.TileY);
                        stats.Moves++;
                        Console.WriteLine($"[bot] ไปเก็บ {nat.Id} (type={nat.Type} dist={nat.Dist:F0})");
                    }
                    else
                    {
                        // ไม่รู้จักของธรรมชาติแถวนี้ — เดินสุ่มหามุมใหม่
                        var rnd = new Random();
                        targetTileX = st.PTileX + rnd.Next(-6, 7);
                        targetTileY = st.PTileY + rnd.Next(-6, 7);
                        targetId = $"wander_{targetTileX}_{targetTileY}";
                        phase = "wandering";
                        phaseAt = DateTime.UtcNow;
                        SendMove(st, targetTileX, targetTileY);
                        stats.Moves++;
                        Console.WriteLine("[bot] ไม่เห็นของธรรมชาติ — เดินสำรวจ");
                    }
                    break;
                }
            }

            if (now >= lastReport + 15.0)
            {
                lastReport = now;
                Console.WriteLine(
                    $"[bot] เดิน={stats.Moves} เก็บ={stats.Gathers}(สำเร็จ {stats.GatherOk}) ล่า={stats.Attacks} " +
                    $"ตี={stats.Actions} กิน={stats.Eats} พลาด={stats.Aborts} | pos=({st.PTileX},{st.PTileY}) " +
                    $"stamina={st.Stamina:F0}/{st.StaminaMax:F0} life={st.Life:F0}/{st.LifeMax:F0} " +
                    $"ของ={st.TotalItems} สัตว์={st.Animals.Count(a => a.Alive)} ธรรมชาติ={st.Naturals.Count} " +
                    $"gathering={st.Gathering} battle={st.BattleMode}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== สรุป GameBot ===");
        Console.WriteLine($"  สั่งเดิน      : {stats.Moves}");
        Console.WriteLine($"  สั่งเก็บ/แล่  : {stats.Gathers} (สำเร็จ {stats.GatherOk})");
        Console.WriteLine($"  สั่งโจมตี     : {stats.Attacks} (ล่าสำเร็จ {stats.HuntOk})");
        Console.WriteLine($"  ใช้สกิลต่อสู้ : {stats.Actions}");
        Console.WriteLine($"  กิน          : {stats.Eats}");
        Console.WriteLine($"  โดนปฏิเสธ     : {stats.Aborts}");
        _client.Close();
    }

    private static void SendMove(GameState st, int tileX, int tileY)
    {
        float wx = tileX * 200f + 100f;
        float wy = tileY * 200f + 100f;
        Send($"move x={wx.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)} y={wy.ToString("F0", System.Globalization.CultureInfo.InvariantCulture)}");
    }
}
