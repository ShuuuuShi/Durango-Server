using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Logic.Combat;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Utils;
using InteractionData;
using Shared.Battle;
using UnityEngine;
using Yaml;
using TerrainUtil = Durango.Terrain.Util;

/// <summary>
/// BotBridge — สะพานควบคุมตัวเกมจากโปรแกรมภายนอก (game-bot.exe)
///
/// ทำงานยังไง: เปิด TCP server พอร์ต 8192 (ตั้งได้ด้วย env DURANGO_BOT_PORT) ในตัวเกม
/// โปรแกรมภายนอกส่งคำสั่งมาทีละบรรทัด (รูปแบบ `คำสั่ง key=value`) แล้วได้คำตอบเป็น JSON 1 บรรทัด
///
/// ไม่แย่งเมาส์: ปุ่ม UI กดผ่าน UICamera.GetInputTouchCount/GetInputTouch (เดิน pipeline
/// เดิมทั้ง raycast → OnPress → OnClick) · เดินใช้ PlayerController.MoveToPosition ·
/// เก็บ/ตีผ่าน InteractionSystem ตัวเดียวกับที่การแตะของจริงเรียก
///
/// คำสั่ง:
///   ping                          → เช็คว่า bridge อยู่ไหม
///   state                         → สถานะเกมทั้งหมด (player/inv/animals/naturals/menus/battle)
///   move x=<worldX> y=<worldZ>    → เดินไปตำแหน่งโลก (tile*200)
///   stop                          → หยุดเดิน
///   tap x=<px> y=<px>             → แตะ UI ที่พิกัดจอ (px, ล่างซ้าย = 0,0 เหมือน Input.mousePosition)
///   gather [id=natural_x_y]       → แตะของธรรมชาติ (ไม่ระบุ = ตัวใกล้สุด) แล้วกด Collect ให้อัตโนมัติ
///   attack [id=animal_x]          → แตะสัตว์ (ใกล้สุดถ้าไม่ระบุ) แล้วกด Attack ให้อัตโนมัติ
///   butcher [id=animal_x]         → แล่ซาก (เฉพาะตัว IsLootable) → Collect อัตโนมัติ
///   action [id=...]               → ใช้สกิลต่อสู้ (UseBattleAction)
///   menu action=<Collect|Attack>  → กดปุ่มเมนูปฏิสัมพันธ์ของเป้าหมายปัจจุบัน
///   use [id=<itemId>]             → ใช้ของ (กิน) — ไม่ระบุ = ของที่กินได้ชิ้นแรก
///   log text=...                  → เขียน log เข้าเกม (Debug.Log)
///
/// ดู docs/server/GameBot.md
/// </summary>
public static class BotBridge
{
    private const int DefaultPort = 8192;
    private const int FakeTouchId = 9001;

    private static readonly object _lock = new object();
    private static readonly Queue<string> _commands = new Queue<string>();
    private static TcpListener _listener;
    private static Thread _thread;
    private static int _port;
    private static bool _started;

    // ---- fake touch (UI) ----
    private static bool _tapActive;
    private static Vector2 _tapPos;
    private static int _tapFrame;

    // ---- auto-offline ----
    private static string _offlineKey;
    private static bool _offlineStarted;
    private static float _offlineAt;

    // ---- interaction auto-select ----
    private static string _pendingMenuAction;
    private static bool _menuSelected;

    private sealed class BridgeBehaviour : MonoBehaviour
    {
        private void Update() { BotBridge.Pump(); }
    }

    // Unity 2017 มี RuntimeInitializeOnLoadMethod (5.3+) — เริ่ม bridge หลังโหลดซีนแรก
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (_started) return;
        _started = true;

        string envPort = Environment.GetEnvironmentVariable("DURANGO_BOT_PORT");
        _port = DefaultPort;
        if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out int p) && p > 0 && p < 65536)
        {
            _port = p;
        }

        // โหมดออฟไลน์: ตั้ง env DURANGO_OFFLINE=solo|free|multi → เกมเริ่มเซิร์ฟออฟไลน์ฝังในตัว
        // แล้วต่อเข้าเอง (ไม่ต้องคลิกเมนู) — ไว้เทสตัวเกมโดยไม่ต้องเปิด DurangoServer
        string offlineKey = Environment.GetEnvironmentVariable("DURANGO_OFFLINE");
        if (!string.IsNullOrEmpty(offlineKey))
        {
            _offlineKey = offlineKey;
            _offlineAt = Time.time + 3f;
            UnityEngine.Debug.Log("[BotBridge] DURANGO_OFFLINE=" + offlineKey + " — จะเริ่มเซิร์ฟออฟไลน์ให้หลัง Title โหลด");
        }

        var go = new GameObject("BotBridge");
        UnityEngine.Object.DontDestroyOnLoad(go);
        go.AddComponent<BridgeBehaviour>();

        // เอาทางเข้า touch ของ NGUI ไปไว้ที่ bridge — queue ที่ว่าง = พฤติกรรมเดิม (ใช้ Input จริง)
        UICamera.GetInputTouchCount = delegate { return _tapActive ? 1 : 0; };
        UICamera.GetInputTouch = delegate
        {
            return new UICamera.Touch
            {
                fingerId = FakeTouchId,
                phase = _tapFrame < 2 ? TouchPhase.Began : TouchPhase.Ended,
                position = _tapPos,
                tapCount = 1
            };
        };

        _thread = new Thread(ListenLoop);
        _thread.IsBackground = true;
        _thread.Start();

        UnityEngine.Debug.Log("[BotBridge] listening on port " + _port);
    }

    private static void ListenLoop()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Loopback, _port);
            _listener.Start();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[BotBridge] cannot listen: " + e.Message);
            return;
        }
        while (true)
        {
            TcpClient client;
            try
            {
                client = _listener.AcceptTcpClient();
            }
            catch
            {
                return;
            }
            try
            {
                using (client)
                {
                    var stream = client.GetStream();
                    var reader = new StreamReaderEx(stream);
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.Length == 0) continue;
                        string reply = Execute(line);
                        byte[] buf = Encoding.UTF8.GetBytes(reply + "\n");
                        stream.Write(buf, 0, buf.Length);
                        stream.Flush();
                    }
                }
            }
            catch
            {
                // client ตัดการเชื่อมต่อ = ปกติ
            }
        }
    }

    /// <summary>เรียกบน thread อ่าน TCP ได้ — งานจริงทั้งหมดถูกย้ายไป main thread ใน Pump()</summary>
    private static string Execute(string line)
    {
        lock (_lock)
        {
            _commands.Enqueue(line);
        }
        // รอจนกว่า Pump() (main thread) จะประมวลผลแล้วเก็บคำตอบ — เพื่อให้คำสั่งมีผลทันที
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            string reply;
            lock (_lock)
            {
                reply = _lastReply;
            }
            if (reply != null)
            {
                lock (_lock)
                {
                    _lastReply = null;
                }
                return reply;
            }
            Thread.Sleep(10);
        }
        return "{\"ok\":false,\"error\":\"timeout\"}";
    }

    private static string _lastReply;

    private static void Pump()
    {
        // 0) ออฟไลน์อัตโนมัติ: รอ Title โหลดแล้วเริ่มเซิร์ฟฝังในตัว + ต่อเอง
        if (_offlineKey != null && !_offlineStarted && Time.time >= _offlineAt)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Title")
            {
                _offlineStarted = true;
                StartOffline(_offlineKey);
            }
            else
            {
                _offlineAt = Time.time + 1f;
            }
        }

        // 1) งานที่ค้างอยู่ (คำสั่งยาว ๆ ไม่ได้ทำเสร็จในเฟรมเดียว)
        if (_pendingMenuAction != null)
        {
            PumpPendingInteraction();
        }

        // 2) fake touch จบรอบแล้ว
        if (_tapActive)
        {
            _tapFrame++;
            if (_tapFrame >= 3) _tapActive = false;
        }

        // 3) ประมวลผลคำสั่ง
        string cmd;
        lock (_lock)
        {
            cmd = _commands.Count > 0 ? _commands.Dequeue() : null;
        }
        if (cmd != null)
        {
            string reply = ExecuteNow(cmd);
            lock (_lock)
            {
                _lastReply = reply;
            }
        }
    }

    // =====================================================================
    // ตัวประมวลผลคำสั่ง (main thread เท่านั้น — เรียก Unity API ได้)
    // =====================================================================

    private static string ExecuteNow(string line)
    {
        try
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return Err("empty");
            string cmd = parts[0];
            var args = new Dictionary<string, string>();
            for (int i = 1; i < parts.Length; i++)
            {
                int eq = parts[i].IndexOf('=');
                if (eq > 0) args[parts[i].Substring(0, eq)] = parts[i].Substring(eq + 1);
            }
            switch (cmd)
            {
                case "ping": return "{\"ok\":true,\"t\":" + Time.time.ToString("F1", CultureInfo.InvariantCulture) + ",\"port\":" + _port + "}";
                case "state": return BuildState();
                case "offline": return CmdOffline(args);
                case "move": return CmdMove(args);
                case "stop": return CmdStop();
                case "tap": return CmdTap(args);
                case "gather": return CmdGather(args, false);
                case "butcher": return CmdGather(args, true);
                case "attack": return CmdAttack(args);
                case "action": return CmdAction(args);
                case "menu": return CmdMenu(args);
                case "use": return CmdUse(args);
                case "log": return CmdLog(args);
                default: return Err("unknown cmd: " + cmd);
            }
        }
        catch (Exception e)
        {
            return Err(e.GetType().Name + ": " + e.Message);
        }
    }

    // ---------------------------------------------------------------- offline

    /// <summary>เริ่มเกมออฟไลน์: เปิดเซิร์ฟฝังในตัว (AppData/offline/&lt;key&gt;) แล้วต่อเข้าเอง</summary>
    private static string CmdOffline(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("key", out string key) || key.Length == 0) key = "solo";
        if (_offlineStarted)
        {
            return Err("offline already started (key=" + (_offlineKey ?? "?") + ")");
        }
        _offlineKey = key;
        _offlineStarted = true;
        StartOffline(key);
        return Ok();
    }

    private static void StartOffline(string key)
    {
        try
        {
            var names = new Dictionary<string, string> { { "en_US", "Offline Bot" } };
            var server = new Durango.Offline.Server(key, names);
            Durango.Offline.Context ctx = server.Contexts.Count > 0 ? server.Contexts[0] : null;
            if (ctx == null)
            {
                // ยังไม่มีเซฟ — สร้างใหม่แบบเดียวกับ Cluster.OnConfirm
                var world = new Durango.Offline.WorldContext();
                world.Initialize(Durango.Offline.WorldContext.MakePath(0, key));
                world.PlayerSlot = 0;
                var player = new Durango.Offline.PlayerContext();
                player.Initialize(Durango.Offline.PlayerContext.MakePath(0, key));
                player.PlayerSlot = 0;
                ctx = new Durango.Offline.Context(world, player);
            }

            string gatewayUrl = "http://127.0.0.1:" + Durango.Offline.Server.GetIslandPort();

            // กัน title state machine ไม่เด้ง Error: ตั้ง PlayerId + GatewayUrl + คลัสเตอร์ที่เลือก
            Preferences.SetString("last_selected_cluster_key", key);
            GameManager.SetCluster(key, gatewayUrl, Mode.Offline);
            GameManager.PlayerId = ctx.EntityId;
            GameManager.IsPlayerIdSelected = true;

            // เริ่มเซิร์ฟออฟไลน์ฝังในตัว (gateway + game server) แล้วต่อเข้าเอง
            Durango.Offline.Server.BeginServer(ctx.World, ctx.Player);
            Durango.Offline.Server.ConnectTo("127.0.0.1:" + Durango.Offline.Server.GetIslandPort());
            UnityEngine.Debug.Log("[BotBridge] offline server started (key=" + key + ", player=" + ctx.EntityId + ", gateway=" + gatewayUrl + ")");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("[BotBridge] offline start failed: " + e);
        }
    }

    // ---------------------------------------------------------------- move

    private static string CmdMove(Dictionary<string, string> args)
    {
        if (!TryF(args, "x", out float wx) || !TryF(args, "y", out float wz)) return Err("move needs x= and y=");
        var player = PlayerBehavior.LocalPlayer;
        if (player == null) return Err("no local player");
        Vector3 world = new Vector3(wx, 0f, wz);
        Vector3 client = TerrainUtil.WorldPositionToClientPosition(world);
        Singleton<PlayerController>.Instance().MoveToPosition(client);
        return Ok();
    }

    private static string CmdStop()
    {
        if (PlayerBehavior.LocalPlayer != null)
        {
            Singleton<PlayerController>.Instance().StopMove();
        }
        return Ok();
    }

    // ---------------------------------------------------------------- tap (UI)

    private static string CmdTap(Dictionary<string, string> args)
    {
        if (!TryF(args, "x", out float x) || !TryF(args, "y", out float y)) return Err("tap needs x= and y=");
        _tapPos = new Vector2(x, y);
        _tapFrame = 0;
        _tapActive = true;
        return Ok();
    }

    // ---------------------------------------------------------------- gather / butcher

    private static string CmdGather(Dictionary<string, string> args, bool butcher)
    {
        var player = PlayerBehavior.LocalPlayer;
        if (player == null) return Err("no local player");

        var objs = new List<GameObject>();
        InteractionSystem.SearchPropObjects(objs);
        GameObject best = null;
        float bestDist = float.MaxValue;
        foreach (GameObject go in objs)
        {
            if (go == null) continue;
            if (butcher)
            {
                if (!ObjectIdentifier.IsDeadBody(go)) continue;
            }
            else
            {
                if (go.GetComponentInParent<NaturalObject>() == null) continue;
            }
            if (args.TryGetValue("id", out string wantId))
            {
                string got = ObjectIdentifier.GetEntityId(go);
                if (got != wantId) continue;
            }
            float d = InteractionObject.GetDistance(go);
            if (d < bestDist) { bestDist = d; best = go; }
        }
        if (best == null) return Err(butcher ? "no corpse near" : "no natural near");
        StartInteraction(best, "Collect");
        return Ok();
    }

    // ---------------------------------------------------------------- attack

    private static string CmdAttack(Dictionary<string, string> args)
    {
        var player = PlayerBehavior.LocalPlayer;
        if (player == null) return Err("no local player");

        var objs = new List<GameObject>();
        InteractionSystem.SearchCombatTargetObjects(objs);
        GameObject best = null;
        float bestDist = float.MaxValue;
        foreach (GameObject go in objs)
        {
            if (go == null) continue;
            if (args.TryGetValue("id", out string wantId))
            {
                string got = ObjectIdentifier.GetEntityId(go);
                if (got != wantId) continue;
            }
            float d = InteractionObject.GetDistance(go);
            if (d < bestDist) { bestDist = d; best = go; }
        }
        if (best == null) return Err("no target near");
        StartInteraction(best, "Attack");
        return Ok();
    }

    private static void StartInteraction(GameObject go, string menuAction)
    {
        if (!GameSystem<InteractionSystem>.HasInstance()) return;
        var inter = GameSystem<InteractionSystem>.Instance();
        inter.SetInteractionTarget(new InteractionObject(go));
        inter.SendTouchMsg();
        _pendingMenuAction = menuAction;
        _menuSelected = false;
    }

    private static void PumpPendingInteraction()
    {
        if (!GameSystem<InteractionSystem>.HasInstance()) return;
        var inter = GameSystem<InteractionSystem>.Instance();
        var menus = inter.MenuList;
        if (!_menuSelected && menus.Count > 0)
        {
            foreach (InteractionMenuData menu in menus)
            {
                if (menu.Disabled || menu.AccessDenied) continue;
                if (menu.Action.ToString() == _pendingMenuAction)
                {
                    inter.SelectTargetInteractionMenu(menu);
                    _menuSelected = true;
                    break;
                }
            }
        }
        // งานจบ: เมนูหาย (กด Attack แล้วเข้าโหมดต่อสู้/เปลี่ยนเป้าหมาย) หรือไม่มี timer เล่นอยู่
        // (เก็บของเสร็จแล้ว — GatheringSystem ลงทะเบียน timer ไว้ใน MenuList)
        if (_menuSelected && (menus.Count == 0 || !menus.HasPlayingTimer()))
        {
            _pendingMenuAction = null;
            _menuSelected = false;
        }
    }

    // ---------------------------------------------------------------- battle action

    private static string CmdAction(Dictionary<string, string> args)
    {
        if (!GameSystem<CombatSystem>.HasInstance()) return Err("no combat system");
        var combat = GameSystem<CombatSystem>.Instance();
        if (args.TryGetValue("id", out string id) && id.Length > 0)
        {
            combat.UseBattleAction(id);
            return Ok();
        }
        // ไม่ระบุ = เลือกท่าโจมตี (Melee/Range) ก่อน ถ้าไม่มีค่อยใช้ท่าอื่น
        foreach (BattleAction a in combat.GetCurrentBattleActions())
        {
            if (!IsUsable(a)) continue;
            var t = a.Data.Meta.BattleActionType;
            if (t == BattleActionType.Melee || t == BattleActionType.Range)
            {
                combat.UseBattleAction(a.Data.Id);
                return Ok();
            }
        }
        foreach (BattleAction a in combat.GetCurrentBattleActions())
        {
            if (IsUsable(a))
            {
                combat.UseBattleAction(a.Data.Id);
                return Ok();
            }
        }
        return Err("no usable battle action");
    }

    private static bool IsUsable(BattleAction a)
    {
        if (a == null || a.Data == null || a.Data.Meta == null) return false;
        if (a.CooldownUntil > Time.time || a.ProhibitedUntil > Time.time) return false;
        return a.Data.Meta.BattleActionType != BattleActionType.Invalid;
    }

    // ---------------------------------------------------------------- interaction menu

    private static string CmdMenu(Dictionary<string, string> args)
    {
        if (!GameSystem<InteractionSystem>.HasInstance()) return Err("no interaction system");
        if (!args.TryGetValue("action", out string action)) return Err("menu needs action=");
        var menus = GameSystem<InteractionSystem>.Instance().MenuList;
        foreach (InteractionMenuData menu in menus)
        {
            if (menu.Disabled || menu.AccessDenied) continue;
            if (menu.Action.ToString() == action)
            {
                GameSystem<InteractionSystem>.Instance().SelectTargetInteractionMenu(menu);
                return Ok();
            }
        }
        return Err("menu action not found: " + action);
    }

    // ---------------------------------------------------------------- use item

    private static string CmdUse(Dictionary<string, string> args)
    {
        if (!GameSystem<InventorySystem>.HasInstance()) return Err("no inventory system");
        var inv = GameSystem<InventorySystem>.Instance();
        ItemData pick = null;
        if (args.TryGetValue("id", out string id) && id.Length > 0)
        {
            foreach (ItemData it in inv.PlayerItemList)
            {
                if (it != null && it.Id == id) { pick = it; break; }
            }
            if (pick == null) return Err("no item with id " + id);
        }
        else
        {
            foreach (ItemData it in inv.PlayerItemList)
            {
                if (it == null || it.Tags == null) continue;
                foreach (TagData tag in it.Tags)
                {
                    if (tag != null && tag.Id != null && IsEdibleTag(tag.Id))
                    {
                        pick = it;
                        break;
                    }
                }
                if (pick != null) break;
            }
            if (pick == null) return Err("no edible item");
        }
        inv.UseItem(pick);
        return Ok();
    }

    // ---------------------------------------------------------------- log

    private static string CmdLog(Dictionary<string, string> args)
    {
        string text = args.TryGetValue("text", out string t) ? t : "";
        UnityEngine.Debug.Log("[BotBridge] " + text);
        return Ok();
    }

    // =====================================================================
    // state — สถานะทั้งหมดที่บอทต้องรู้
    // =====================================================================

    private static string BuildState()
    {
        var sb = new StringBuilder(1024);
        sb.Append("{\"ok\":true,\"t\":");
        sb.Append(Time.time.ToString("F1", CultureInfo.InvariantCulture));
        sb.Append(",\"scene\":");
        JStr(sb, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        sb.Append(",\"screen\":[");
        sb.Append(Screen.width).Append(',').Append(Screen.height);
        sb.Append(']');

        var player = PlayerBehavior.LocalPlayer;
        sb.Append(",\"player\":");
        if (player == null) sb.Append("null");
        else
        {
            Vector3 world = TerrainUtil.ClientPositionToWorldPosition(player.CurrentPosition);
            Vector2 tile = TerrainUtil.WorldPositionToTilePosition(world);
            sb.Append('{');
            sb.Append("\"pos\":[");
            sb.Append(world.x.ToString("F1", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(world.z.ToString("F1", CultureInfo.InvariantCulture)).Append(']');
            sb.Append(",\"tile\":[");
            sb.Append(tile.x.ToString("F0", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(tile.y.ToString("F0", CultureInfo.InvariantCulture)).Append(']');
            AppendGauge(sb, "life", player.Life);
            AppendGauge(sb, "stamina", player.Stamina);
            AppendGauge(sb, "fatigue", player.Fatigue);
            sb.Append(",\"alive\":").Append(player.IsAlive ? "true" : "false");
            sb.Append(",\"moving\":").Append(player.IsMoving ? "true" : "false");
            sb.Append('}');
        }

        AppendInventory(sb);
        AppendAnimals(sb);
        AppendNaturals(sb);
        AppendMenus(sb);
        AppendBattle(sb);

        var gathering = GameSystem<GatheringSystem>.HasInstance() ? GameSystem<GatheringSystem>.Instance() : null;
        sb.Append(",\"gathering\":").Append(gathering != null && gathering.IsGathering ? "true" : "false");
        sb.Append(",\"pending\":");
        JStr(sb, _pendingMenuAction ?? "");

        sb.Append('}');
        return sb.ToString();
    }

    private static void AppendGauge(StringBuilder sb, string key, Gauge g)
    {
        sb.Append(",\"").Append(key).Append("\":[");
        if (g == null) sb.Append("0,0");
        else
        {
            sb.Append(g.Get().ToString("F1", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(g.Max().ToString("F1", CultureInfo.InvariantCulture));
        }
        sb.Append(']');
    }

    private static void AppendInventory(StringBuilder sb)
    {
        sb.Append(",\"inv\":[");
        if (GameSystem<InventorySystem>.HasInstance())
        {
            var inv = GameSystem<InventorySystem>.Instance();
            if (inv != null)
            {
                var byProto = new Dictionary<string, ItemData>();
            var counts = new Dictionary<string, int>();
            foreach (ItemData it in inv.PlayerItemList)
            {
                if (it == null) continue;
                string key = it.PrototypeId ?? it.Id ?? "?";
                if (!byProto.ContainsKey(key)) byProto[key] = it;
                counts[key] = counts.ContainsKey(key) ? counts[key] + 1 : 1;
            }
            bool first = true;
            foreach (KeyValuePair<string, ItemData> kv in byProto)
            {
                ItemData it = kv.Value;
                if (!first) sb.Append(',');
                first = false;
                sb.Append("{\"id\":");
                JStr(sb, it.Id);
                sb.Append(",\"proto\":");
                JStr(sb, it.PrototypeId);
                sb.Append(",\"name\":");
                JStr(sb, it.Name ?? it.PrototypeName);
                sb.Append(",\"count\":").Append(counts[kv.Key]);
                sb.Append(",\"size\":").Append(it.Size);
                bool edible = false;
                if (it.Tags != null)
                {
                    bool firstTag = true;
                    sb.Append(",\"tags\":[");
                    foreach (TagData tag in it.Tags)
                    {
                        if (tag == null || tag.Id == null) continue;
                        if (!firstTag) sb.Append(',');
                        firstTag = false;
                        JStr(sb, tag.Id);
                        if (IsEdibleTag(tag.Id)) edible = true;
                    }
                    sb.Append(']');
                }
                else
                {
                    sb.Append(",\"tags\":[]");
                }
                if (!edible && (it.PrototypeId ?? "").IndexOf("food", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    edible = true;
                }
                sb.Append(",\"edible\":").Append(edible ? "true" : "false");
                sb.Append('}');
            }
            }
        }
        sb.Append(']');
    }

    private static void AppendAnimals(StringBuilder sb)
    {
        sb.Append(",\"animals\":[");
        bool first = true;
        if (AnimalManager.HasInstance())
        {
            var animals = AnimalManager.Instance()._animals;
            foreach (KeyValuePair<string, AnimalBehavior> kv in animals)
            {
                AnimalBehavior a = kv.Value;
                if (a == null) continue;
                if (!first) sb.Append(',');
                first = false;
                Vector3 world = TerrainUtil.ClientPositionToWorldPosition(a.CurrentPosition);
                sb.Append("{\"id\":");
                JStr(sb, a.EntityId ?? kv.Key);
                sb.Append(",\"type\":").Append(a.EntityTypeId);
                sb.Append(",\"pos\":[");
                sb.Append(world.x.ToString("F1", CultureInfo.InvariantCulture)).Append(',');
                sb.Append(world.z.ToString("F1", CultureInfo.InvariantCulture)).Append(']');
                sb.Append(",\"alive\":").Append(a.IsAlive ? "true" : "false");
                sb.Append(",\"lootable\":").Append(a.IsLootable ? "true" : "false");
                sb.Append('}');
            }
        }
        sb.Append(']');
    }

    private static void AppendNaturals(StringBuilder sb)
    {
        sb.Append(",\"naturals\":[");
        bool first = true;
        var objs = new List<GameObject>();
        InteractionSystem.SearchPropObjects(objs);
        foreach (GameObject go in objs)
        {
            if (go == null) continue;
            var io = new InteractionObject(go);
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"id\":");
            JStr(sb, io.EntityId);
            sb.Append(",\"type\":").Append(io.EntityType);
            sb.Append(",\"tile\":[");
            Vector2 tile = io.Tile;
            sb.Append(tile.x.ToString("F0", CultureInfo.InvariantCulture)).Append(',');
            sb.Append(tile.y.ToString("F0", CultureInfo.InvariantCulture)).Append(']');
            sb.Append(",\"dist\":");
            sb.Append(io.Distance.ToString("F1", CultureInfo.InvariantCulture));
            sb.Append(",\"corpse\":").Append(ObjectIdentifier.IsDeadBody(go) ? "true" : "false");
            sb.Append('}');
        }
        sb.Append(']');
    }

    private static void AppendMenus(StringBuilder sb)
    {
        sb.Append(",\"menus\":[");
        if (GameSystem<InteractionSystem>.HasInstance())
        {
            var menus = GameSystem<InteractionSystem>.Instance().MenuList;
            bool first = true;
            foreach (InteractionMenuData menu in menus)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append("{\"action\":");
                JStr(sb, menu.Action.ToString());
                sb.Append(",\"id\":");
                JStr(sb, menu.Id);
                sb.Append(",\"duration\":");
                sb.Append(menu.Duration.ToString("F1", CultureInfo.InvariantCulture));
                sb.Append(",\"disabled\":").Append(menu.Disabled ? "true" : "false");
                sb.Append('}');
            }
        }
        sb.Append(']');
    }

    private static void AppendBattle(StringBuilder sb)
    {
        sb.Append(",\"battle\":{");
        var combat = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
        sb.Append("\"mode\":").Append(combat != null && combat.CombatMode ? "true" : "false");
        sb.Append(",\"actions\":[");
        if (combat != null)
        {
            bool first = true;
            foreach (BattleAction a in combat.GetCurrentBattleActions())
            {
                if (a == null || a.Data == null || a.Data.Meta == null) continue;
                if (a.Data.Meta.BattleActionType == BattleActionType.Invalid) continue;
                if (!first) sb.Append(',');
                first = false;
                sb.Append("{\"id\":");
                JStr(sb, a.Data.Id);
                sb.Append(",\"cd\":");
                sb.Append(Math.Max(0.0, a.CooldownUntil - Time.time).ToString("F1", CultureInfo.InvariantCulture));
                sb.Append('}');
            }
        }
        sb.Append("]}");
    }

    // =====================================================================
    // ของเล็ก ๆ น้อย ๆ
    // =====================================================================

    private static string Ok()
    {
        return "{\"ok\":true}";
    }

    private static bool IsEdibleTag(string tagId)
    {
        return tagId.IndexOf("food", StringComparison.OrdinalIgnoreCase) >= 0 ||
               tagId.IndexOf("eat", StringComparison.OrdinalIgnoreCase) >= 0 ||
               tagId.IndexOf("drink", StringComparison.OrdinalIgnoreCase) >= 0 ||
               tagId.IndexOf("taste", StringComparison.OrdinalIgnoreCase) >= 0 ||
               tagId.IndexOf("meal", StringComparison.OrdinalIgnoreCase) >= 0 ||
               tagId.IndexOf("cook", StringComparison.OrdinalIgnoreCase) >= 0 ||
               tagId.IndexOf("fruit", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static string Err(string msg)
    {
        var sb = new StringBuilder(64);
        sb.Append("{\"ok\":false,\"error\":");
        JStr(sb, msg);
        sb.Append('}');
        return sb.ToString();
    }

    private static void JStr(StringBuilder sb, string s)
    {
        sb.Append('"');
        if (s != null)
        {
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
        }
        sb.Append('"');
    }

    private static bool TryF(Dictionary<string, string> args, string key, out float value)
    {
        value = 0f;
        if (!args.TryGetValue(key, out string s)) return false;
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>อ่านทีละบรรทัดจนจบบรรทัด (\n หรือ \r\n) — ใช้แทน StreamReader ใน net35 ที่อ่านช้า</summary>
    private sealed class StreamReaderEx
    {
        private readonly byte[] _buf = new byte[4096];
        private int _pos;
        private int _len;
        private readonly NetworkStream _stream;
        private readonly StringBuilder _sb = new StringBuilder(128);

        public StreamReaderEx(NetworkStream stream) { _stream = stream; }

        public string ReadLine()
        {
            while (true)
            {
                if (_pos >= _len)
                {
                    _len = _stream.Read(_buf, 0, _buf.Length);
                    _pos = 0;
                    if (_len == 0) return _sb.Length > 0 ? _sb.ToString() : null;
                }
                int start = _pos;
                while (_pos < _len && _buf[_pos] != (byte)'\n') _pos++;
                if (_pos < _len)
                {
                    string chunk = Encoding.UTF8.GetString(_buf, start, _pos - start);
                    _pos++;
                    if (chunk.EndsWith("\r", StringComparison.Ordinal)) chunk = chunk.Substring(0, chunk.Length - 1);
                    if (_sb.Length > 0)
                    {
                        chunk = _sb.ToString() + chunk;
                        _sb.Length = 0;
                    }
                    return chunk;
                }
                _sb.Append(Encoding.UTF8.GetString(_buf, start, _pos - start));
            }
        }
    }
}
