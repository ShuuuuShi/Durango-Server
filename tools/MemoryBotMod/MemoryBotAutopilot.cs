using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Logic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Modding;
using Durango.Utils;
using InteractionData;
using UnityEngine;
using Yaml;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{
internal static class MemoryBotAutopilot
{
    private static readonly System.Random Rng = new System.Random();
    private static readonly Dictionary<string, float> Cooldowns = new Dictionary<string, float>(StringComparer.Ordinal);
    private static IClientModApi _api;
    private static bool _running;
    private static string _mode = "gather";
    private static string _phase = "stopped";
    private static GameObject _target;
    private static string _targetId;

    /// <summary>เวลา (Time.time) ที่เริ่มล็อกเป้าปัจจุบัน — ใช้บังคับเปลี่ยนเป้าเมื่อครบ TargetLockSeconds</summary>
    private static float _targetLockedAt;

    /// <summary>
    /// [แก้เอง 1 ก.ย. 2026] เจ้าของสั่ง: "ถ้าล็อกเป้าหมายเดิมนานกว่า 10 วิให้ยกเลิกและเปลี่ยนเป้าหมาย"
    /// กันบอทวิ่งไล่ตัวที่เข้าไม่ถึง (ติดกำแพง/หนีเร็วกว่า/ยืนในกองหิน) จนไม่ได้ทำอะไรเลย
    /// </summary>
    private const float TargetLockSeconds = 10f;

    /// <summary>
    /// [แก้เอง 2 ก.ย. 2026] เข้า AttackRange แล้วเลือดไม่ลด ~4 วิ = เป้าเข้าไม่ถึง (ติดหิน)
    /// → RememberTargetFailure("no_damage") ห้ามรีเซ็ตล็อกแค่เพราะยืนในระยะหรือยิงท่า
    /// </summary>
    private const float CombatNoDamageSeconds = 4f;

    /// <summary>เวลาเข้า AttackRange ของเป้าปัจจุบัน (0 = ยังไม่เข้า/ออกระยะแล้ว)</summary>
    private static float _combatEngageAt;

    /// <summary>เลือดล่าสุดของเป้าตอน engage / ตอนเลือดลดจริง</summary>
    private static float _lastKnownHp = -1f;

    /// <summary>
    /// [แก้เอง 1 ก.ย. 2026] เจ้าของสั่ง: "บอทไม่มีมีดเลยเก็บไม่ได้ ถ้าแบบนี้ให้บอทสร้างเครื่องมือจาก
    /// เมนูคราฟ ถ้าวัตถุดิบไม่ครบ ให้ไปหาเองกำหนดเป้าหมายด้วย เมื่อครบให้สร้างเครื่องมือ"
    ///
    /// ลำดับ: มีเครื่องมืออยู่แล้ว → ข้าม · วัสดุครบ → กดคราฟต์ · วัสดุไม่ครบ → ประกาศเป้าหมาย
    /// (ช่องที่ขาด) แล้วสลับไปโหมดเก็บของ พอเก็บได้เพิ่มก็วนกลับมาเช็คใหม่ทุกครั้ง
    /// </summary>
    private static readonly string[] ToolRecipeWishlist = { "blade_stone", "axe_onehand_stone_01" };

    /// <summary>ชื่อย่อของเครื่องมือที่ถือว่า "มีแล้ว" (เทียบกับ PrototypeId ในกระเป๋า)</summary>
    private static readonly string[] ToolPrototypeHints = { "blade", "knife", "axe" };

    private static float _nextToolCheck;
    private static int _gatherFailStreak;
    private static string _neededToolRecipe;

    private static string _toolGoal;
    private static string _gatherFilter = "";

    /// <summary>
    /// เป้าหมายหลัก/รอง — เจ้าของสั่ง: "เป้าหมายหลักแล่เนื้อ เป้าหมายรองทำมีด"
    /// เป้าหมายหลักมาจากโหมดที่สั่งตอน bot.start · เป้าหมายรองคือของที่ต้องมีก่อนถึงจะทำหลักได้
    /// </summary>
    /// <summary>
    /// [แก้เอง 1 ก.ย. 2026] ใครเป็นเจ้าของคิวคิดตอนนี้ — "autopilot" (เก็บของ/ล่า) หรือ "daily" (เควสรายวัน)
    ///
    /// 🐛 เดิมใช้ `_mode == "daily"` เป็นตัวตัดสินใน Tick() ซึ่งพังเพราะ TickHunt()/TickGather()
    ///    ที่ daily เรียกยืม TickMain ไปใช้ จะ "คืนค่า" ด้วยการเซ็ต _mode = "daily" ตายตัว
    ///    ⇒ ถ้า autopilot กำลังวิ่งโหมด survival อยู่แล้วมีใครเรียก TickHunt เพียงครั้งเดียว
    ///       _mode จะกลายเป็น "daily" ถาวร แล้ว Tick() เด้งไปรัน daily แทน (ซึ่ง _running=false)
    ///       ⇒ บอทหยุดเงียบ ๆ เอง · bot.status รายงาน "daily / stopped" ทั้งที่สั่ง survival
    ///    (เจอจริง 1 ก.ย.: สั่ง bot.start survival แล้ว 40 วินาทีถัดมาเด้งเป็น daily/stopped)
    ///
    /// ตอนนี้เจ้าของถูกตั้งตอนสั่งเริ่มเท่านั้น และ _mode เป็นแค่ "งานที่ทำอยู่" ของ autopilot
    /// </summary>
    private static string _owner = "none";

    /// <summary>ประกาศเป้าในแชทไหม — โหมดชีวิตปิด (คนจริงไม่พิมพ์บอกทุกครั้งที่จะเก็บหิน)</summary>
    public static bool Chatty = true;

    private static string _mainGoal;

    private static string _subGoal;

    /// <summary>ต้องการเครื่องมือตัด/แล่กี่ชิ้นถึงพอ (มีดสึก/หักระหว่างแล่ เลยเผื่อไว้ · ตั้งผ่าน bot.start x=N)</summary>
    private static int _toolTarget = 2;

    /// <summary>โหมดเดิมก่อนถูกสลับไปเก็บวัสดุทำเครื่องมือ — ได้เครื่องมือแล้วต้องกลับไปโหมดนี้</summary>
    private static string _modeBeforeToolHunt;

    /// <summary>
    /// ออบเจกต์ที่เพิ่งเก็บไป 10 รายการล่าสุด — เจ้าของสั่งให้จำไว้ "จะได้ไม่ต้องไปเก็บซ้ำ"
    /// คีย์เดียวกับ TargetKey (entity id หรือ natural_x_y)
    /// </summary>
    private static readonly List<string> RecentCollected = new List<string>();

    private const int RecentCollectedMax = 10;

    private static void RememberCollected(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        RecentCollected.Remove(key);
        RecentCollected.Add(key);
        while (RecentCollected.Count > RecentCollectedMax) RecentCollected.RemoveAt(0);
    }
    private static float _phaseUntil;
    private static float _nextThink;
    private static float _nextAction;
    private static int _moves;
    private static int _inspections;
    private static int _actions;
    private static int _eats;
    private static string _lastReason = "";
    private static Vector3 _lastPosition;
    private static float _lastProgressAt;
    private static float _moveCommandAt;
    private static float _nextSkillAttempt;
    private static bool _skillLearning;
    private static int _cleanupMoveAttempts;
    private static string _cleanupMoveTarget;

    public static void Initialize(IClientModApi api)
    {
        _api = api;
        MemoryBotDaily.Initialize(api);
        MemoryBotLife.Initialize(api);
    }

    public static void ConfigureDaily(bool testProvisioning, bool autoReward)
    {
        MemoryBotDaily.Configure(testProvisioning, autoReward);
    }

    public static string Execute(MemoryBotRequest request)
    {
        string name = request.Name ?? "";
        if (name == "combat.auto")
        {
            bool on = EnableAutoBattle();
            return "{\"status\":" + (on ? "\"accepted\"" : "\"rejected\"")
                + ",\"command\":\"combat.auto\",\"auto_battle\":" + (on ? "true" : "false")
                + (on ? "" : ",\"reason\":\"battle_ui_not_ready\"") + "}";
        }
        // [3 ก.ย. 2026] โหมดชีวิต — เล่นแบบคน (ดู MemoryBotLife)
        if (name == "bot.start" && string.Equals(request.Kind, "life", StringComparison.OrdinalIgnoreCase))
        {
            MemoryBotDaily.Execute(new MemoryBotRequest { Name = "bot.daily.stop" });
            _owner = "life";
            _mode = "life";
            _running = false;
            _target = null;
            _targetId = null;
            _phase = "idle";
            _gatherFailStreak = 0;
            _neededToolRecipe = null;
            RecentCollected.Clear();
            EnableAutoBattle();
            return MemoryBotLife.Start(request);
        }
        if (_owner == "life")
        {
            if (name == "bot.stop")
            {
                MemoryBotLife.Stop();
                _owner = "none";
                _phase = "stopped";
                _target = null;
                _targetId = null;
                return StatusJson();
            }
            if (name == "bot.status") return MemoryBotLife.StatusJson();
            if (name == "bot.goal")
            {
                BotGoalKind kind;
                if (!MemoryBotGoals.TryParseKind(request.Kind, out kind) || kind == BotGoalKind.Daily)
                    return Error("special_goal_needs_kind");
                string target = request.EntityId;
                if (string.IsNullOrEmpty(target)) target = request.MenuId;
                if (string.IsNullOrEmpty(target)) target = request.ItemId;
                if (kind == BotGoalKind.Craft && !string.IsNullOrEmpty(target))
                {
                    string resolved = MemoryBotCommands.ResolveRecipeId(target);
                    if (resolved != null) target = resolved;
                }
                int count = request.HasCount ? request.Count : 1;
                MemoryBotGoals.Need(kind, target ?? "", count, MemoryBotGoals.KindName(kind) + " " + (target ?? ""), "special");
                return MemoryBotLife.StatusJson();
            }
        }
        if (name == "bot.goal"
            || (name == "bot.start" && string.Equals(request.Kind, "daily", StringComparison.OrdinalIgnoreCase)))
        {
            _owner = "daily";
            _mode = "daily";
            _running = false;            // หยุด autopilot ไม่ให้แย่งคิวคิดกับ daily
            return MemoryBotDaily.Execute(request);
        }
        if ((name == "bot.stop" || name == "bot.status") && _owner == "daily")
        {
            return MemoryBotDaily.Execute(request);
        }
        if (name == "bot.daily.start")
        {
            _owner = "daily";
            _mode = "daily";
            _running = false;
            return MemoryBotDaily.Execute(request);
        }
        if (name == "bot.daily.stop")
        {
            string reply = MemoryBotDaily.Execute(request);
            _owner = "none";
            return reply;
        }
        if (name == "bot.start")
        {
            _mode = string.IsNullOrEmpty(request.Kind) ? "gather" : request.Kind.ToLowerInvariant();
            if (_mode != "gather" && _mode != "survival" && _mode != "cleanup_farms") return Error("unsupported_mode");
            // ยึดคิวคิดให้ autopilot และสั่ง daily หยุด ไม่งั้นสองตัวเขียนทับ _mode/_target กันเอง
            MemoryBotDaily.Execute(new MemoryBotRequest { Name = "bot.daily.stop" });
            _owner = "autopilot";
            _running = true;
            _phase = "waiting";
            _target = null;
            _targetId = null;
            _nextThink = 0f;
            _lastPosition = Vector3.zero;
            _lastProgressAt = Time.time;
            _moveCommandAt = 0f;
            _nextSkillAttempt = 0f;
            _skillLearning = false;
            _lastReason = "started";
            _mainGoal = _mode == "survival" ? "ล่าสัตว์แล้วแล่เนื้อ"
                      : _mode == "gather" ? "เก็บทรัพยากร"
                      : _mode;
            _subGoal = null;
            _modeBeforeToolHunt = null;
            RecentCollected.Clear();
            if (request.HasX && request.X >= 1f && request.X <= 10f) _toolTarget = (int)request.X;
            bool auto = EnableAutoBattle();
            Log("autopilot started mode=" + _mode + " auto_battle=" + (auto ? "on" : "ยังตั้งไม่ได้ (UI ต่อสู้ยังไม่ถูกสร้าง)"));
            return StatusJson();
        }
        if (name == "bot.stop")
        {
            _owner = "none";
            _running = false;
            _phase = "stopped";
            _target = null;
            _targetId = null;
            _lastPosition = Vector3.zero;
            if (PlayerBehavior.LocalPlayer != null && Singleton<PlayerController>.HasInstance())
                Singleton<PlayerController>.Instance().StopMove();
            Log("autopilot stopped");
            return StatusJson();
        }
        if (name == "bot.status") return StatusJson();
        return Error("unknown_bot_command");
    }

    public static string StatusJson()
    {
        if (_owner == "daily") return MemoryBotDaily.StatusJson();
        if (_owner == "life") return MemoryBotLife.StatusJson();
        return "{\"owner\":" + MemoryBotProtocol.Quote(_owner)
            + ",\"running\":" + (_running ? "true" : "false")
            + ",\"mode\":" + MemoryBotProtocol.Quote(_mode)
            + ",\"phase\":" + MemoryBotProtocol.Quote(_phase)
            + ",\"target_id\":" + MemoryBotProtocol.Quote(_targetId ?? "")
            + ",\"moves\":" + _moves
            + ",\"inspections\":" + _inspections
            + ",\"actions\":" + _actions
            + ",\"eats\":" + _eats
            + ",\"main_goal\":" + MemoryBotProtocol.Quote(_mainGoal ?? "")
            + ",\"sub_goal\":" + MemoryBotProtocol.Quote(_subGoal ?? "")
            + ",\"tool_target\":" + _toolTarget
            + ",\"recent_collected\":" + RecentCollected.Count
            + ",\"goals\":" + MemoryBotGoals.ToJson()
            + ",\"last_reason\":" + MemoryBotProtocol.Quote(_lastReason ?? "") + "}";
    }

    public static string LastReason { get { return _lastReason; } }
    public static string Phase { get { return _phase; } }

    public static void ResetSkillSession()
    {
        _target = null;
        _targetId = null;
        _targetLockedAt = 0f;
        _combatEngageAt = 0f;
        _lastKnownHp = -1f;
        _phase = "idle";
    }

    public static GameObject FindNearbyThreat(float range)
    {
        if (!AnimalManager.HasInstance()) return null;
        GameObject best = null;
        float bestDist = range;
        foreach (KeyValuePair<string, AnimalBehavior> pair in AnimalManager.Instance()._animals)
        {
            AnimalBehavior animal = pair.Value;
            if (animal == null || !animal.IsAlive || animal.gameObject == null) continue;
            float dist = new InteractionObject(animal.gameObject).Distance;
            if (dist < bestDist)
            {
                bestDist = dist;
                best = animal.gameObject;
            }
        }
        return best;
    }

    public static void FightBack(GameObject threat)
    {
        if (threat != null)
        {
            _target = threat;
            _targetId = TargetKey(threat);
            _targetLockedAt = Time.time;
            _combatEngageAt = 0f;
            _lastKnownHp = -1f;
        }
        string previousMode = _mode;
        _mode = "survival";
        _phase = "combat";
        FightTarget();
        _mode = previousMode;
    }

    public static void TickGather(string filter)
    {
        _gatherFilter = filter ?? "";
        string previousMode = _mode;
        _mode = "gather";
        TickMain();
        _mode = previousMode;
    }

    public static void TickHunt()
    {
        string previousMode = _mode;
        _mode = "survival";
        TickMain();
        _mode = previousMode;
    }

    public static int CountItemsMatching(string hint)
    {
        InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
        if (inventory == null || string.IsNullOrEmpty(hint)) return 0;
        string needle = hint.ToLowerInvariant();
        bool wantString = needle.IndexOf("string", StringComparison.Ordinal) >= 0
            || needle.IndexOf("สาย", StringComparison.Ordinal) >= 0
            || needle.IndexOf("เชือก", StringComparison.Ordinal) >= 0;
        int n = 0;
        foreach (ItemData item in inventory.PlayerItemList)
        {
            if (item == null) continue;
            string proto = item.PrototypeId == null ? "" : item.PrototypeId.ToLowerInvariant();
            string name = item.Name == null ? "" : item.Name.ToLowerInvariant();
            if (proto.IndexOf(needle, StringComparison.Ordinal) >= 0
                || name.IndexOf(needle, StringComparison.Ordinal) >= 0) { n++; continue; }
            if (wantString && (item.GetTagData("string_long") != null || item.GetTagData("string_normal") != null)) n++;
        }
        return n;
    }

    public static void Tick(float deltaTime)
    {
        if (_owner == "daily")
        {
            MemoryBotDaily.Tick();
            return;
        }
        if (_owner == "life")
        {
            MemoryBotLife.Tick();
            return;
        }
        if (_owner != "autopilot" || !_running || Time.time < _nextThink) return;
        _nextThink = Time.time + 0.25f;
        try { TickMain(); }
        catch (Exception e)
        {
            _phase = "error";
            _lastReason = e.GetType().Name + ":" + e.Message;
            Log("autopilot error: " + _lastReason);
            _nextAction = Time.time + 5f;
        }
    }

    /// <summary>มีเครื่องมือตัด/แล่อยู่ไหม — เป้าหมาย "ล่า/แล่เนื้อ" ต้องเช็คก่อนเริ่มงาน</summary>
    public static bool HasButcherTool()
    {
        return CountCuttingTools(GameSystem<InventorySystem>.HasInstance()
            ? GameSystem<InventorySystem>.Instance() : null) > 0;
    }

    /// <summary>มีเครื่องมือตัด/แล่อย่างน้อย 1 ชิ้นไหม (ตัวห่อไว้ให้โค้ดเดิมเรียกได้เหมือนเดิม)</summary>
    private static bool HasCuttingTool(InventorySystem inventory)
    {
        return CountCuttingTools(inventory) > 0;
    }

    /// <summary>มีเครื่องมือตัด/แล่อยู่ในกระเป๋ากี่ชิ้น</summary>
    private static int CountCuttingTools(InventorySystem inventory)
    {
        if (inventory == null) return 0;
        int count = 0;
        foreach (ItemData item in inventory.PlayerItemList)
        {
            if (item == null || string.IsNullOrEmpty(item.PrototypeId)) continue;
            string id = item.PrototypeId.ToLowerInvariant();
            for (int i = 0; i < ToolPrototypeHints.Length; i++)
            {
                if (id.IndexOf(ToolPrototypeHints[i], StringComparison.Ordinal) >= 0) { count++; break; }
            }
        }
        return count;
    }

    /// <summary>
    /// พยายามให้มีเครื่องมือก่อนออกทำงาน — คืน true ถ้ารอบนี้จัดการเรื่องเครื่องมือไปแล้ว
    /// (ผู้เรียกควร return ทันที) · false = มีเครื่องมือแล้ว หรือทำอะไรไม่ได้ตอนนี้
    /// </summary>
    private static bool EnsureTool(InventorySystem inventory)
    {
        // ตอน daily เป็นเจ้าของคิวคิด กองเป้าหมาย (MemoryBotGoals) เป็นคนจัดการเรื่องเครื่องมือเอง
        // ผ่าน TickHuntGoal → Need(Craft, blade_stone) ⇒ ตรงนี้ต้องไม่ไปสลับ _mode แข่งกัน
        if (_owner != "autopilot") return false;   // daily/life จัดการเครื่องมือผ่านกองเป้าหมายเอง
        if (Time.time < _nextToolCheck) return false;
        _nextToolCheck = Time.time + 3f;
        int tools = CountCuttingTools(inventory);
        if (tools >= _toolTarget)
        {
            _subGoal = null;
            if (_modeBeforeToolHunt != null)
            {
                _mode = _modeBeforeToolHunt;
                _modeBeforeToolHunt = null;
                _target = null;
                _targetId = null;
                _targetLockedAt = 0f;
                _combatEngageAt = 0f;
                _lastKnownHp = -1f;
                _phase = "idle";
                _lastReason = "มีเครื่องมือแล้ว กลับไปโหมด " + _mode;
                Log(_lastReason);
            }
            return false;
        }
        string firstMissing = null;
        for (int i = 0; i < ToolRecipeWishlist.Length; i++)
        {
            string recipeId = ToolRecipeWishlist[i];
            string detail;
            string check = MemoryBotCommands.CraftRecipe(recipeId, true, out detail);
            if (check == null)
            {
                string madeDetail;
                string error = MemoryBotCommands.CraftRecipe(recipeId, false, out madeDetail);
                _phase = "crafting";
                _lastReason = error == null ? ("คราฟต์ " + madeDetail) : ("คราฟต์ไม่สำเร็จ: " + error);
                _nextAction = Time.time + 4f;
                _nextToolCheck = Time.time + 6f;
                _subGoal = null;
                Log(_lastReason);
                return true;
            }
            if (check == "missing_material" && firstMissing == null)
            {
                firstMissing = detail;
            }
        }
        if (firstMissing != null)
        {
            // ประกาศเป้าหมายให้เห็นชัดใน bot.status แล้วไปเก็บของต่อในโหมด gather
            string goal = "ทำเครื่องมือ (มี " + tools + "/" + _toolTarget + ") ยังขาด: " + firstMissing;
            if (_subGoal != goal)
            {
                _subGoal = goal;
                Log("เป้าหมายรอง: " + goal + " · เป้าหมายหลัก: " + (_mainGoal ?? _mode));
            }
            if (_mode == "survival")
            {
                _modeBeforeToolHunt = _mode;
                _mode = "gather";
                _target = null;
                _targetId = null;
                _targetLockedAt = 0f;
                _combatEngageAt = 0f;
                _lastKnownHp = -1f;
                _phase = "idle";
            }
            _lastReason = goal;
        }
        return false;
    }

    private static void TickMain()
    {
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (!GameManager.IsMainScene || !GameManager.IsReady || player == null)
        {
            _phase = "waiting";
            _lastReason = "game_not_ready";
            return;
        }
        if (!player.IsAlive)
        {
            _phase = "dead";
            _lastReason = "player_dead";
            return;
        }
        if (MemoryBotBrain.Tick(player))
        {
            _phase = MemoryBotBrain.Phase;
            _lastReason = MemoryBotBrain.Reason;
            _nextAction = Time.time + MemoryBotBrain.Delay;
            return;
        }
        if (_mode == "cleanup_farms")
        {
            CleanupFarms();
            return;
        }
        if (_phase == "moving")
        {
            // เดินตามเส้นทาง (A*) อยู่ — RouteWalker จับอาการติดเอง ไม่ต้อง replan ซ้อน
            if (MemoryBotMove.Routing)
            {
                _lastReason = "moving_route_" + MemoryBotMove.RouteStatus;
                _lastProgressAt = Time.time;
                _lastPosition = player.CurrentPosition;
                _nextAction = Time.time + 0.4f;
                return;
            }
            Vector3 currentPosition = player.CurrentPosition;
            if (_lastPosition == Vector3.zero) _lastPosition = currentPosition;
            if (Vector3.Distance(currentPosition, _lastPosition) > 35f)
            {
                _lastPosition = currentPosition;
                _lastProgressAt = Time.time;
            }
            if (Time.time - _lastProgressAt > 4f && Time.time - _moveCommandAt > 2f)
            {
                ReplanTarget(player);
                return;
            }
        }
        if (Time.time < _nextAction) return;

        InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
        if (ShouldEat(player, inventory))
        {
            EatFirstFood(inventory);
            return;
        }
        if (_mode == "gather" && MemoryBotUi.IsInventoryFull())
        {
            _phase = "inventory_full";
            _lastReason = "กระเป๋าเต็ม หยุดเก็บ";
            _nextAction = Time.time + 3f;
            return;
        }
        if (TryLearnCombatSkill()) return;
        MemoryBotUi.LockTools();
        if (_gatherFailStreak >= 5 && !string.IsNullOrEmpty(_neededToolRecipe))
        {
            string toolDetail;
            string toolErr = MemoryBotUi.CraftThroughMenu(_neededToolRecipe, out toolDetail);
            _gatherFailStreak = 0;
            _phase = "crafting_tool";
            _lastReason = toolErr == null ? ("คราฟต์เครื่องมือ " + (_neededToolRecipe)) : ("craft_tool_" + toolErr);
            _nextAction = Time.time + 3.5f;
            return;
        }
        if (EnsureTool(inventory)) return;

        InteractionSystem interaction = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
        if (interaction == null)
        {
            _phase = "waiting";
            _lastReason = "interaction_unavailable";
            return;
        }
        if (_phase == "combat")
        {
            FightTarget();
            return;
        }
        if (_phase == "inspect" || _phase == "working")
        {
            PumpInteraction(interaction);
            return;
        }
        if (_target != null && !IsTargetUsable(_target)) RememberTargetFailure("target_unusable");
        if (_target != null)
        {
            InteractionObject target = new InteractionObject(_target);
            if (target.Distance > 135f) { MoveNear(_target); return; }
            InspectAndAct(interaction, _target);
            return;
        }

        List<GameObject> objects = new List<GameObject>();
        GameObject candidate = null;
        if (_mode == "survival")
        {
            candidate = ChooseManagedAnimal();
            if (candidate == null)
            {
                var combatObjects = new List<GameObject>();
                InteractionSystem.SearchCombatTargetObjects(combatObjects);
                candidate = ChooseAnimal(combatObjects);
            }
        }
        if (candidate == null && _mode != "survival")
        {
            InteractionSystem.SearchPropObjects(objects);
            candidate = ChooseTarget(objects);
        }
        if (candidate != null)
        {
            _target = candidate;
            _targetId = TargetKey(candidate);
            _targetLockedAt = Time.time;
            _combatEngageAt = 0f;
            _lastKnownHp = -1f;
            _phase = "targeted";
            _lastReason = "target_selected";
            AnnounceTarget(candidate);
            MoveNear(candidate);
            return;
        }
        Wander(player);
    }

    private static bool TryLearnCombatSkill()
    {
        if (_skillLearning || Time.time < _nextSkillAttempt) return false;
        SkillSystem skills = GameSystem<SkillSystem>.HasInstance() ? GameSystem<SkillSystem>.Instance() : null;
        if (skills == null || skills.Skills == null || skills.RemainSkillPoint <= 0)
        {
            _nextSkillAttempt = Time.time + 5f;
            return false;
        }
        foreach (Durango.Logic.Skill.Bundle bundle in skills.Skills)
        {
            if (bundle == null || bundle.Category != Shared.Skill.Category.MeleeCombat) continue;
            if (TryLearnSkill(skills, bundle.Base)) return true;
            if (bundle.Sub == null) continue;
            foreach (Durango.Logic.Skill.Skill skill in bundle.Sub)
            {
                if (TryLearnSkill(skills, skill)) return true;
            }
        }
        _nextSkillAttempt = Time.time + 10f;
        return false;
    }

    private static void CleanupFarms()
    {
        InteractionSystem interaction = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
        if (interaction == null || !ArtifactManager.HasInstance())
        {
            _phase = "waiting";
            _lastReason = "cleanup_system_unavailable";
            _nextAction = Time.time + 1f;
            return;
        }
        if (_phase == "cleanup_removing" && _target != null)
        {
            bool stillExists = false;
            foreach (Artifact artifact in ArtifactManager.Instance().GetArtifacts())
            {
                if (artifact != null && artifact.EntityId == _targetId) { stillExists = true; break; }
            }
            if (!stillExists)
            {
                interaction.SetInteractionTarget(null);
                _target = null;
                _targetId = null;
                _phase = "cleanup_next";
                _lastReason = "player_finished_removing_farm";
                _nextAction = Time.time + 0.5f;
                return;
            }
            if (interaction.MenuList.HasPlayingTimer() || Time.time < _phaseUntil)
            {
                _lastReason = "player_removing_farm_" + _targetId;
                _nextAction = Time.time + 0.5f;
                return;
            }
            interaction.SetInteractionTarget(new InteractionObject(_target));
            interaction.SendTouchMsg();
            _phase = "cleanup_inspect";
            _phaseUntil = Time.time + 2f;
            _lastReason = "retrying_farm_menu_" + _targetId;
            return;
        }
        if (_phase == "cleanup_inspect" && _target != null)
        {
            foreach (InteractionMenuData menu in interaction.MenuList)
            {
                if (menu.Disabled || menu.AccessDenied) continue;
                if (!string.Equals(menu.Action.ToString(), "DestructArtifact", StringComparison.Ordinal)) continue;
                Artifact farm = _target.GetComponentInParent<Artifact>();
                Type artifactInteractions = typeof(GameManager).Assembly.GetType("Durango.Logic.Interactions.ArtifactInteractions", false);
                MethodInfo destruct = artifactInteractions == null ? null : artifactInteractions.GetMethod("DestructArtifact", BindingFlags.Static | BindingFlags.NonPublic);
                if (farm == null || destruct == null)
                {
                    _lastReason = "destruct_handler_unavailable";
                    _nextAction = Time.time + 2f;
                    return;
                }
                destruct.Invoke(null, new object[] { farm });
                _actions++;
                _phase = "cleanup_removing";
                _lastReason = "player_selected_destruct_" + _targetId;
                _phaseUntil = Time.time + Mathf.Max(30f, menu.Duration + 5f);
                _nextAction = Time.time + 0.5f;
                return;
            }
            if (Time.time < _phaseUntil) return;
            interaction.SendTouchMsg();
            _phaseUntil = Time.time + 1.5f;
            _lastReason = "waiting_destruct_menu";
            return;
        }
        if (Time.time < _nextAction) return;
        Artifact nearest = null;
        float bestDistance = float.MaxValue;
        int farmCount = 0;
        foreach (Artifact artifact in ArtifactManager.Instance().GetArtifacts())
        {
            if (artifact == null || artifact.gameObject == null || artifact.BlueprintId != "farm_tile_01") continue;
            farmCount++;
            float cooldownUntil;
            if (Cooldowns.TryGetValue("cleanup:" + artifact.EntityId, out cooldownUntil) && cooldownUntil > Time.time) continue;
            float distance = new InteractionObject(artifact.gameObject).Distance;
            if (distance < bestDistance) { bestDistance = distance; nearest = artifact; }
        }
        if (nearest == null)
        {
            if (farmCount > 0)
            {
                _phase = "waiting";
                _lastReason = "waiting_for_reachable_farm";
                _nextAction = Time.time + 2f;
                return;
            }
            _running = false;
            _phase = "complete";
            _lastReason = "all_farms_removed_by_player";
            return;
        }
        _target = nearest.gameObject;
        _targetId = nearest.EntityId;
        _targetLockedAt = Time.time;
        if (bestDistance > 180f)
        {
            if (_cleanupMoveTarget != nearest.EntityId)
            {
                _cleanupMoveTarget = nearest.EntityId;
                _cleanupMoveAttempts = 0;
            }
            _cleanupMoveAttempts++;
            if (_cleanupMoveAttempts > 5)
            {
                Cooldowns["cleanup:" + nearest.EntityId] = Time.time + 20f;
                _target = null;
                _targetId = null;
                _cleanupMoveTarget = null;
                _cleanupMoveAttempts = 0;
                _phase = "cleanup_next";
                _lastReason = "skipping_blocked_farm";
                _nextAction = Time.time + 0.3f;
                return;
            }
            MoveCloseToFarm(nearest.gameObject);
            _lastReason = "player_walking_to_farm";
            return;
        }
        interaction.SetInteractionTarget(new InteractionObject(nearest.gameObject));
        interaction.SendTouchMsg();
        _inspections++;
        _phase = "cleanup_inspect";
        _phaseUntil = Time.time + 2f;
        _nextAction = Time.time + 0.35f;
        _lastReason = "player_opened_farm_menu_" + nearest.EntityId;
    }

    private static void MoveCloseToFarm(GameObject target)
    {
        if (target == null || !Singleton<PlayerController>.HasInstance()) return;
        Vector3 destination = target.transform.position;
        float angle = (float)Rng.NextDouble() * Mathf.PI * 2f;
        float radius = 10f + (float)Rng.NextDouble() * 10f;
        destination.x += Mathf.Cos(angle) * radius;
        destination.z += Mathf.Sin(angle) * radius;
        Singleton<PlayerController>.Instance().MoveToPosition(destination);
        _moves++;
        _phase = "moving";
        _moveCommandAt = Time.time;
        _nextAction = Time.time + 0.9f;
    }


    private static bool TryLearnSkill(SkillSystem skills, Durango.Logic.Skill.Skill skill)
    {
        if (skill == null || skill.Level >= skill.MaxLevel) return false;
        Durango.Logic.Skill.Node node = skill.Get(skill.Level + 1);
        if (node == null || node.State != Durango.Logic.Skill.State.Learnable) return false;
        _skillLearning = true;
        _nextSkillAttempt = Time.time + 3f;
        _lastReason = "learning_" + skill.Id + "_" + (skill.SubId ?? "base") + "_lv" + (skill.Level + 1);
        if (!MemoryBotUi.OpenSkillMenu(skill))
        {
            _skillLearning = false;
            return false;
        }
        _skillLearning = false;
        return true;
    }
    private static bool ShouldEat(PlayerBehavior player, InventorySystem inventory)
    {
        if (inventory == null || inventory.PlayerItemList == null) return false;
        return Ratio(player.Life) < 0.35f || Ratio(player.Stamina) < 0.25f;
    }

    private static void EatFirstFood(InventorySystem inventory)
    {
        foreach (ItemData item in inventory.PlayerItemList)
        {
            if (item == null || !IsEdible(item)) continue;
            inventory.UseItem(item);
            _eats++;
            _phase = "eating";
            _lastReason = "low_survival";
            _nextAction = Time.time + 2.5f + Jitter(1.5f);
            Log("ate " + (item.Name ?? item.PrototypeName ?? item.Id));
            return;
        }
        _phase = "resting";
        _lastReason = "low_survival_no_food";
        _nextAction = Time.time + 5f;
    }

    private static GameObject ChooseTarget(List<GameObject> objects)
    {
        GameObject best = null;
        float bestScore = float.MaxValue;
        foreach (GameObject go in objects)
        {
            if (!IsTargetUsable(go)) continue;
            if (go.GetComponentInParent<AnimalBehavior>() != null) continue;
            if (_mode == "gather" && !MatchesGatherFilter(go)) continue;
            InteractionObject target = new InteractionObject(go);
            string id = TargetKey(go);
            float until;
            if (id.Length > 0 && Cooldowns.TryGetValue(id, out until) && until > Time.time) continue;
            if (RecentCollected.Contains(id)) continue;   // เพิ่งเก็บไปแล้ว ไปหากองอื่น
            if (target.Distance > 1200f) continue;
            float score = target.Distance + (float)Rng.NextDouble() * 180f;
            if (go.GetComponentInParent<NaturalObject>() == null) score += 220f;
            if (score < bestScore) { bestScore = score; best = go; }
        }
        return best;
    }

    private static string TargetKey(GameObject go)
    {
        if (go == null) return "";
        InteractionObject target = new InteractionObject(go);
        if (!string.IsNullOrEmpty(target.EntityId)) return target.EntityId;
        Vector2 tile = target.Tile;
        return "natural_" + ((int)tile.x) + "_" + ((int)tile.y);
    }

    /// <summary>
    /// [3 ก.ย. 2026] ตารางว่าของธรรมชาติชนิดไหน (entity type) ให้อะไร — สกัดจาก server NaturalData.cs
    /// เดิมกรองด้วยชื่อ GameObject ซึ่งเป็นชื่อ prefab เกาหลี/ตัวเลข ⇒ ตัวกรอง "wood_log" ไม่เคยเจออะไร
    /// </summary>
    private static readonly Dictionary<string, int[]> GatherTypes = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase)
    {
        { "stone", new[] { 12000, 12003, 12124, 13000, 13044, 13045, 13046, 13047, 15001 } },
        { "stem", new[] { 11002, 11032 } },
        { "wood_log", new[] { 11026, 14004, 14005, 14014, 14017, 14029 } },
        { "wood_bough", new[] { 14004, 14005, 14014, 14017, 14029 } },
        { "stick", new[] { 14004, 14005, 14014, 14017, 14029 } },
        { "flax", new[] { 11021 } },
        { "string", new[] { 11021, 11002, 11032 } },
        { "fruit_berry", new[] { 11004, 11009, 11013, 11031, 11018 } },
        { "berry", new[] { 11004, 11009, 11013, 11031, 11018 } },
        { "food", new[] { 11004, 11009, 11013, 11031, 11018 } },
        { "wood_bush", new[] { 11004, 11009, 11013, 11018, 11031 } },
        { "clay", new[] { 13006 } },
    };

    /// <summary>ตัวกรอง → generator id ที่ต้องการในเมนูเก็บ (menu.Id)</summary>
    private static readonly Dictionary<string, string[]> GatherMenuIds = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        { "stone", new[] { "stone" } },
        { "stem", new[] { "stem" } },
        { "wood_log", new[] { "wood_log" } },
        { "wood_bough", new[] { "wood_bough" } },
        { "stick", new[] { "wood_bough" } },
        { "flax", new[] { "flax" } },
        { "string", new[] { "flax", "stem" } },
        { "fruit_berry", new[] { "fruit_berry", "wildberry" } },
        { "berry", new[] { "fruit_berry", "wildberry" } },
        { "food", new[] { "fruit_berry", "wildberry" } },
        { "wood_bush", new[] { "wood_bush" } },
        { "clay", new[] { "clay" } },
    };

    private static bool IsAnyFilter(string filter)
    {
        return string.IsNullOrEmpty(filter) || filter == "any" || filter == "*";
    }

    private static bool MatchesGatherFilter(GameObject go)
    {
        if (go == null) return false;
        if (IsAnyFilter(_gatherFilter)) return go.GetComponentInParent<NaturalObject>() != null;
        int[] types;
        if (GatherTypes.TryGetValue(_gatherFilter, out types))
        {
            if (go.GetComponentInParent<NaturalObject>() == null) return false;
            int type = new InteractionObject(go).EntityType;
            for (int i = 0; i < types.Length; i++) if (types[i] == type) return true;
            return false;
        }
        if (_gatherFilter.IndexOf("หิน", StringComparison.Ordinal) >= 0)
            return IsStoneTarget(go);
        string needle = _gatherFilter.ToLowerInvariant();
        string name = go.name == null ? "" : go.name.ToLowerInvariant();
        ImmovableBase immovable = go.GetComponent<ImmovableBase>();
        string resolved = immovable == null ? "" : (immovable.GetName() ?? "").ToLowerInvariant();
        if (name.IndexOf(needle, StringComparison.Ordinal) >= 0) return true;
        if (resolved.IndexOf(needle, StringComparison.Ordinal) >= 0) return true;
        return go.GetComponentInParent<NaturalObject>() != null;
    }

    private static bool IsStoneTarget(GameObject go)
    {
        if (go == null || go.GetComponentInParent<NaturalObject>() == null) return false;
        int type = new InteractionObject(go).EntityType;
        switch (type)
        {
            case 12000:
            case 12003:
            case 12124:
            case 13000:
            case 13044:
            case 13045:
            case 13046:
            case 13047:
            case 15001:
                return true;
            default:
                return false;
        }
    }

    private static bool IsTargetUsable(GameObject go)
    {
        if (go == null) return false;
        AnimalBehavior animal = go.GetComponentInParent<AnimalBehavior>();
        if (animal != null)
        {
            // ซากแล่ได้ยังใช้ได้ (เส้นทางเก็บซาก); ตาย/hp<=0 ที่แล่ไม่ได้ให้ตัดทิ้ง
            if (!animal.IsAlive && !animal.IsLootable) return false;
            if (animal.Life != null && animal.Life.Get() <= 0f && !animal.IsLootable) return false;
        }
        InteractionObject target = new InteractionObject(go);
        bool natural = go.GetComponentInParent<NaturalObject>() != null;
        return (natural || !string.IsNullOrEmpty(target.EntityId)) && target.Distance < 1500f;
    }

    private static void MoveNear(GameObject target)
    {
        if (!Singleton<PlayerController>.HasInstance())
        {
            _lastReason = "player_controller_unavailable";
            _nextAction = Time.time + 2f;
            return;
        }
        // [3 ก.ย. 2026] ไกล = หาเส้นทางก่อน (เดิมพุ่งเส้นตรงแล้วไปจอดริมน้ำ)
        string how = MemoryBotMove.Near(target.transform.position, 25f, 70f, Rng);
        _moves++;
        if (how == "player_unavailable")
        {
            _lastReason = "player_controller_unavailable";
            _nextAction = Time.time + 2f;
            return;
        }
        if (_phase != "moving")
        {
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            _lastPosition = player == null ? Vector3.zero : player.CurrentPosition;
            _lastProgressAt = Time.time;
        }
        _phase = "moving";
        _lastReason = "moving_to_target";
        _moveCommandAt = Time.time;
        _nextAction = Time.time + 0.8f + Jitter(0.7f);
    }

    private static void ReplanTarget(PlayerBehavior player)
    {
        if (_target == null || !Singleton<PlayerController>.HasInstance())
        {
            _lastReason = "replan_without_target";
            _nextAction = Time.time + 1f;
            return;
        }
        Vector3 from = player.CurrentPosition;
        Vector3 to = _target.transform.position;
        to.y = from.y;
        Vector3 delta = to - from;
        float distance = delta.magnitude;
        if (distance < 100f)
        {
            _lastProgressAt = Time.time;
            _nextAction = Time.time;
            return;
        }
        Vector3 direction = delta / distance;
        float step = Mathf.Min(500f, distance - 80f);
        Vector3 waypoint = from + direction * step;
        Vector3 side = new Vector3(-direction.z, 0f, direction.x);
        waypoint += side * ((float)Rng.NextDouble() * 160f - 80f);
        waypoint.y = from.y;
        Singleton<PlayerController>.Instance().MoveToPosition(waypoint);
        _moves++;
        _phase = "moving";
        _lastReason = "replanned_waypoint";
        _lastPosition = from;
        _lastProgressAt = Time.time;
        _moveCommandAt = Time.time;
        _nextAction = Time.time + 1f + Jitter(1f);
    }
    private static void InspectAndAct(InteractionSystem interaction, GameObject target)
    {
        interaction.SetInteractionTarget(new InteractionObject(target));
        interaction.SendTouchMsg();
        _inspections++;
        _phase = "inspect";
        _phaseUntil = Time.time + 2.2f;
        _nextAction = Time.time + 0.35f + Jitter(0.35f);
        _lastReason = "inspecting_target";
    }

    private static void PumpInteraction(InteractionSystem interaction)
    {
        InteractionMenuList menus = interaction.MenuList;
        if (_phase == "inspect")
        {
            // [3 ก.ย. 2026] ตัวกรองบอกว่าอยากได้อะไร — เลือกเมนูเก็บที่ให้ของนั้นก่อน (ต้นไม้มีทั้ง "กิ่ง" กับ "ท่อนซุง")
            string[] wantIds = null;
            bool filtered = _mode == "gather" && !IsAnyFilter(_gatherFilter) && GatherMenuIds.TryGetValue(_gatherFilter, out wantIds);
            if (filtered)
            {
                bool sawWanted = false;
                foreach (InteractionMenuData menu in menus)
                {
                    if (menu.GatheringData == null) continue;
                    if (!MenuIdWanted(menu, wantIds)) continue;
                    sawWanted = true;
                    if (menu.Disabled || menu.AccessDenied || !menu.GatheringData.IsAvailableForGathering()) continue;
                    interaction.SelectTargetInteractionMenu(menu);
                    _actions++;
                    _phase = "working";
                    _phaseUntil = Time.time + Mathf.Max(1.2f, menu.Duration + 0.8f);
                    _nextAction = Time.time + 0.4f + Jitter(0.8f);
                    _lastReason = "เก็บ " + (menu.GatheringData.Name ?? menu.Id) + " x" + menu.GatheringData.Amount;
                    return;
                }
                if (Time.time < _phaseUntil) return;
                // มีเมนูที่ต้องการแต่กดไม่ได้ (ไม่มีเครื่องมือ/เลเวลไม่ถึง) หรือก้อนนี้ไม่ให้ของที่ต้องการ
                RememberTargetFailure(sawWanted ? "wanted_menu_unavailable" : "no_wanted_menu");
                return;
            }
            foreach (InteractionMenuData menu in menus)
            {
                if (menu.Disabled || menu.AccessDenied) continue;
                // [แก้เอง 1 ก.ย. 2026] เจ้าของสั่ง: "ก่อนกดแล่ก็ตรวจปุ่มว่ามันใช่เนื้อไหมแค่นั้นเอง
                // มึงไม่เช็ค เดาอย่างเดียว" — ถูกต้อง โค้ดเดิมตัดสินจาก **ชื่อแอ็กชันเป็นสตริง**
                // (IsHumanGatherAction) เท่านั้น ไม่เคยดูว่ากดแล้วได้ของจริงไหม
                //
                // เมนู Collect หนึ่งเป้าหมายมีได้หลายรายการ และบางรายการกดไม่ได้ (เลเวลไม่ถึง /
                // ไม่มีเครื่องมือ) เช่นกองหินมี "หิน"=ได้ กับ "หินก้อนใหญ่"=ไม่ได้ · ซากสัตว์ก็เหมือนกัน
                // ⇒ เดิมคว้ารายการแรกที่ชื่อเข้าเค้า แล้วกดไปเฉย ๆ ไม่มีอะไรเกิดขึ้น รอจนหมดเวลา
                //
                // ตอนนี้ถ้าเป็นรายการเก็บ/แล่ (มี GatheringData) ให้ถามเกมตรง ๆ ว่าเก็บได้ไหม
                // ด้วย IsAvailableForGathering() ซึ่งเป็นตัวเดียวกับที่ UI ใช้ตัดสินว่าปุ่มกดได้
                GatheringData gathering = menu.GatheringData;
                if (gathering != null)
                {
                    if (!gathering.IsAvailableForGathering()) continue;
                }
                else if (!IsHumanGatherAction(menu.Action.ToString())) continue;
                if (_mode == "gather"
                    && (string.IsNullOrEmpty(_gatherFilter)
                        || _gatherFilter.IndexOf("stone", StringComparison.OrdinalIgnoreCase) >= 0
                        || _gatherFilter.IndexOf("หิน", StringComparison.Ordinal) >= 0)
                    && !IsBareHandGatherMenu(menu)) continue;
                interaction.SelectTargetInteractionMenu(menu);
                _actions++;
                _phase = IsAnimalTarget() && IsAttackAction(menu.Action.ToString()) ? "combat" : "working";
                _phaseUntil = Time.time + Mathf.Max(1.2f, menu.Duration + 0.8f);
                _nextAction = Time.time + 0.4f + Jitter(0.8f);
                _lastReason = gathering != null
                    ? ("เก็บ/แล่ " + (gathering.Name ?? menu.Action.ToString()) + " x" + gathering.Amount)
                    : ("executing_" + menu.Action);
                return;
            }
            if (Time.time < _phaseUntil) return;
            RememberTargetFailure("no_safe_menu");
            return;
        }

        if (menus.HasPlayingTimer() || (GameSystem<GatheringSystem>.HasInstance()
            && GameSystem<GatheringSystem>.Instance().IsGathering))
        {
            _lastReason = "working";
            _nextAction = Time.time + 0.8f;
            return;
        }
        if (Time.time < _phaseUntil) return;
        interaction.SetInteractionTarget(null);
        RememberCollected(_targetId);       // กันวนกลับไปเก็บกองเดิมซ้ำ
        _gatherFailStreak = 0;
        _target = null;
        _targetId = null;
        _phase = "idle";
        _lastReason = "work_complete";
        _nextAction = Time.time + 0.5f + Jitter(1.5f);
    }

    private static bool MenuIdWanted(InteractionMenuData menu, string[] wantIds)
    {
        string id = menu.Id ?? "";
        string gen = menu.GatheringData != null ? (menu.GatheringData.GeneratorId ?? "") : "";
        for (int i = 0; i < wantIds.Length; i++)
        {
            if (string.Equals(id, wantIds[i], StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(gen, wantIds[i], StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    /// <summary>เดินสำรวจแบบสุ่ม (ให้โหมดอื่นเรียกได้)</summary>
    public static void WanderPublic()
    {
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (player != null && Singleton<PlayerController>.HasInstance()) Wander(player);
    }

    private static bool IsBareHandGatherMenu(InteractionMenuData menu)
    {
        // NaturalData exposes both "stone" and "stone_big" for the same
        // entity type.  The latter is the large rock and requires a pickaxe.
        // Gathering mode is intentionally limited to the bare-hands generator.
        return string.Equals(menu.Id, "stone", StringComparison.OrdinalIgnoreCase);
    }

    private static GameObject ChooseManagedAnimal()
    {
        if (!AnimalManager.HasInstance()) return null;
        GameObject best = null;
        float bestScore = float.MaxValue;
        foreach (KeyValuePair<string, AnimalBehavior> pair in AnimalManager.Instance()._animals)
        {
            AnimalBehavior animal = pair.Value;
            if (animal == null || !animal.IsAlive || animal.gameObject == null) continue;
            if (!MemoryBotBrain.IsSafe(animal)) continue;   // คนไม่เดินเข้าไปหาตัวที่เลเวลสูงกว่ามาก
            string managedId = TargetKey(animal.gameObject);
            float managedCd;
            if (managedId.Length > 0 && Cooldowns.TryGetValue(managedId, out managedCd) && managedCd > Time.time) continue;
            InteractionObject target = new InteractionObject(animal.gameObject);
            if (target.Distance > 5000f) continue;
            float score = target.Distance + (float)Rng.NextDouble() * 220f;
            if (score < bestScore)
            {
                bestScore = score;
                best = animal.gameObject;
            }
        }
        return best;
    }

    private static GameObject ChooseAnimal(List<GameObject> objects)
    {
        GameObject best = null;
        float bestScore = float.MaxValue;
        foreach (GameObject go in objects)
        {
            if (!IsTargetUsable(go)) continue;
            AnimalBehavior candidate = go.GetComponentInParent<AnimalBehavior>();
            if (candidate == null) continue;
            // ตายแล้วแต่แล่ไม่ได้ = เป้าไร้ค่า; ตาย+แล่ได้ยังเก็บไว้ (คะแนนต่ำกว่า)
            if (!candidate.IsAlive && !candidate.IsLootable) continue;
            if (candidate.IsAlive && candidate.Life != null && candidate.Life.Get() <= 0f) continue;
            if (candidate.IsAlive && !MemoryBotBrain.IsSafe(candidate)) continue;
            string animalId = TargetKey(go);
            float animalCd;
            if (animalId.Length > 0 && Cooldowns.TryGetValue(animalId, out animalCd) && animalCd > Time.time) continue;
            InteractionObject target = new InteractionObject(go);
            if (target.Distance > 1000f) continue;
            float score = target.Distance + (float)Rng.NextDouble() * 120f;
            // ซากที่แล่ได้ต้องมาก่อนตัวเป็นเสมอ — ของอยู่กับที่ ได้เนื้อชัวร์ และซากมีเวลาจำกัดก่อนหาย
            if (!candidate.IsAlive && candidate.IsLootable) score -= 2000f;
            if (score < bestScore)
            {
                bestScore = score;
                best = go;
            }
        }
        return best;
    }

    private static bool IsAnimalTarget()
    {
        return _target != null && _target.GetComponentInParent<AnimalBehavior>() != null;
    }

    private static bool IsAttackAction(string action)
    {
        return !string.IsNullOrEmpty(action) && action.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>ระยะที่ยิงท่าโจมตีได้จริง — ไกลกว่านี้เกมจะไม่ส่ง UseBattleAction ให้</summary>
    private const float AttackRange = 150f;

    /// <summary>
    /// เปิด "สู้ออโต้" ให้เอง — เจ้าของสั่ง: "ให้บอทเปิดโหมดสู้ออโต้ได้เองด้วย"
    ///
    /// ปุ่มนี้อยู่ใน BattleActionButtons เป็นฟิลด์ private `_autoBattle` (Observable&lt;bool&gt;)
    /// และจำค่าไว้ที่ Preferences คีย์ "AutoBattle" ⇒ ตั้งทั้งสองที่:
    ///   1. Preferences เพื่อให้ค่าอยู่ข้ามรอบ (และตรงกับที่ปุ่มอ่านตอน Start)
    ///   2. ฟิลด์ในอินสแตนซ์ที่กำลังทำงานอยู่ ผ่าน reflection เพื่อให้มีผลทันทีโดยไม่ต้องรีโหลด UI
    /// ไม่ได้ข้ามระบบเกม — เป็นค่าเดียวกับที่ผู้เล่นกดปุ่มเอง
    /// </summary>
    private static bool EnableAutoBattle()
    {
        try
        {
            Preferences.SetBool("AutoBattle", true);
            Preferences.SetBool("AutoBattleClicked", true);
        }
        catch
        {
        }
        try
        {
            Durango.UI.BattleActionButtons buttons =
                UnityEngine.Object.FindObjectOfType<Durango.UI.BattleActionButtons>();
            if (buttons == null) return false;
            System.Reflection.FieldInfo field = typeof(Durango.UI.BattleActionButtons).GetField(
                "_autoBattle",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field == null) return false;
            object observable = field.GetValue(buttons);
            if (observable == null) return false;
            System.Reflection.PropertyInfo value = observable.GetType().GetProperty("Value");
            if (value == null || !value.CanWrite) return false;
            value.SetValue(observable, true, null);
            return true;
        }
        catch (Exception e)
        {
            Log("เปิดสู้ออโต้ไม่สำเร็จ: " + e.Message);
            return false;
        }
    }

    private static float ReadAnimalHp(AnimalBehavior animal)
    {
        if (animal == null || animal.Life == null) return -1f;
        return animal.Life.Get();
    }

    private static void FightTarget()
    {
        // Chase timeout only while NOT in AttackRange (in-range uses no_damage).
        if (_combatEngageAt <= 0f
            && _targetLockedAt > 0f
            && Time.time - _targetLockedAt > TargetLockSeconds)
        {
            RememberTargetFailure("target_lock_timeout");
            return;
        }
        AnimalBehavior animal = _target == null ? null : _target.GetComponentInParent<AnimalBehavior>();
        // corpse lootable: leave combat so TickMain can harvest
        if (animal != null && !animal.IsAlive && animal.IsLootable)
        {
            _combatEngageAt = 0f;
            _lastKnownHp = -1f;
            _phase = "idle";
            _lastReason = "ซากแล่ได้ ไปแล่เนื้อ";
            _nextAction = Time.time + 0.2f;
            return;
        }
        if (animal == null || !animal.IsAlive)
        {
            _target = null;
            _targetId = null;
            _targetLockedAt = 0f;
            _combatEngageAt = 0f;
            _lastKnownHp = -1f;
            _phase = "idle";
            _lastReason = "target_dead_or_gone";
            _nextAction = Time.time + 1f + Jitter(1.5f);
            return;
        }
        InteractionObject target = new InteractionObject(_target);
        CombatSystem combat = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
        if (combat == null)
        {
            _lastReason = "combat_system_unavailable";
            _nextAction = Time.time + 1f;
            return;
        }
        if (target.Distance > AttackRange)
        {
            // Left / never reached range — chase; TargetLockSeconds applies.
            _combatEngageAt = 0f;
            MoveNear(_target);
            _phase = "combat";
            return;
        }

        // [fix 2026-09-02] In AttackRange: progress = HP drop only.
        // Old bug reset _targetLockedAt every tick while in range and on UseBattleAction,
        // so rock-stuck targets never timed out.
        float hp = ReadAnimalHp(animal);
        if (_combatEngageAt <= 0f)
        {
            _combatEngageAt = Time.time;
            _lastKnownHp = hp;
        }
        else if (hp >= 0f && _lastKnownHp >= 0f && hp < _lastKnownHp - 0.01f)
        {
            _lastKnownHp = hp;
            _combatEngageAt = Time.time;
            _targetLockedAt = Time.time;
        }
        else if (Time.time - _combatEngageAt >= CombatNoDamageSeconds)
        {
            RememberTargetFailure("no_damage");
            return;
        }

        if (!combat.CombatMode)
        {
            DamageableEntity enemy = _target.GetComponentInParent<DamageableEntity>();
            if (enemy != null) combat.SelectTarget(enemy);
        }
        foreach (BattleAction action in combat.GetCurrentBattleActions())
        {
            if (action == null || action.Data == null || action.Data.Meta == null) continue;
            if (action.CooldownUntil > Time.time || action.ProhibitedUntil > Time.time) continue;
            combat.UseBattleAction(action.Data.Id);
            _actions++;
            // Firing an action alone is NOT progress — wait for HP drop next ticks.
            _lastReason = "battle_" + action.Data.Id;
            _nextAction = Time.time + 0.9f + Jitter(1.1f);
            return;
        }
        _lastReason = "waiting_for_battle_action";
        _nextAction = Time.time + 0.5f + Jitter(0.8f);
    }
private static bool IsHumanGatherAction(string action)
    {
        if (string.IsNullOrEmpty(action)) return false;
        string name = action.ToLowerInvariant();
        if (_mode == "survival" && IsAttackAction(name)) return true;
        return name.IndexOf("collect", StringComparison.Ordinal) >= 0
            || name.IndexOf("gather", StringComparison.Ordinal) >= 0
            || name.IndexOf("harvest", StringComparison.Ordinal) >= 0
            || name.IndexOf("pick", StringComparison.Ordinal) >= 0
            || name.IndexOf("mine", StringComparison.Ordinal) >= 0
            || name.IndexOf("chop", StringComparison.Ordinal) >= 0
            || name.IndexOf("cut", StringComparison.Ordinal) >= 0
            || name.IndexOf("dig", StringComparison.Ordinal) >= 0;
    }

    private static void RememberTargetFailure(string reason)
    {
        _gatherFailStreak++;
        if (_gatherFailStreak >= 5 && string.IsNullOrEmpty(_neededToolRecipe))
            _neededToolRecipe = HasCuttingTool(GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null)
                ? "assembled_axe_one_01" : "blade_stone";
        if (!string.IsNullOrEmpty(_targetId)) Cooldowns[_targetId] = Time.time + 8f + Jitter(8f);
        _target = null;
        _targetId = null;
        _targetLockedAt = 0f;
        _combatEngageAt = 0f;
        _lastKnownHp = -1f;
        _phase = "idle";
        _lastReason = reason + "_fails_" + _gatherFailStreak;
        _nextAction = Time.time + 0.5f + Jitter(1.2f);
    }
private static void Wander(PlayerBehavior player)
    {
        Vector3 world = TerrainUtil.ClientPositionToWorldPosition(player.CurrentPosition);
        float angle = (float)Rng.NextDouble() * Mathf.PI * 2f;
        float distance = 350f + (float)Rng.NextDouble() * 650f;
        world.x += Mathf.Cos(angle) * distance;
        world.z += Mathf.Sin(angle) * distance;
        int tileCount = Durango.Terrain.TerrainMeta.TileCount;
        if (tileCount > 0)
        {
            world.x = Mathf.Clamp(world.x, 150f, tileCount * 200f - 150f);
            world.z = Mathf.Clamp(world.z, 150f, tileCount * 200f - 150f);
        }
        Singleton<PlayerController>.Instance().MoveToPosition(TerrainUtil.WorldPositionToClientPosition(world));
        _moves++;
        _phase = "wandering";
        _lastReason = "no_interactable_in_range";
        _nextAction = Time.time + 1.5f + Jitter(1.5f);
    }

    private static bool IsEdible(ItemData item)
    {
        if (item.Tags == null) return false;
        foreach (TagData tag in item.Tags)
        {
            if (tag != null && tag.Id != null && (tag.Id.IndexOf("food", StringComparison.OrdinalIgnoreCase) >= 0
                || tag.Id.IndexOf("eat", StringComparison.OrdinalIgnoreCase) >= 0
                || tag.Id.IndexOf("drink", StringComparison.OrdinalIgnoreCase) >= 0)) return true;
        }
        return false;
    }

    private static float Ratio(Gauge gauge)
    {
        if (gauge == null || gauge.Max() <= 0f) return 1f;
        return gauge.Get() / gauge.Max();
    }

    private static float Jitter(float max) { return (float)Rng.NextDouble() * max; }

    private static void Log(string text)
    {
        if (_api != null) _api.Log("[autopilot] " + text);
    }

    private static void AnnounceTarget(GameObject target)
    {
        if (!Chatty) return;
        if (target == null || !GameSystem<SocialSystem>.HasInstance()) return;
        string name = target.name;
        if (IsStoneTarget(target)) name = "\u0e2b\u0e34\u0e19";
        ImmovableBase immovable = target.GetComponent<ImmovableBase>();
        if (immovable != null && !IsStoneTarget(target))
        {
            string resolved = immovable.GetName();
            if (!string.IsNullOrEmpty(resolved)) name = resolved;
        }
        if (string.IsNullOrEmpty(name)) name = "ไม่ทราบชื่อ";
        GameSystem<SocialSystem>.Instance().Say("ล็อคเป้าหมาย: " + name);
    }

    private static string Error(string reason)
    {
        return "{\"status\":\"rejected\",\"reason\":" + MemoryBotProtocol.Quote(reason) + "}";
    }
}
}
