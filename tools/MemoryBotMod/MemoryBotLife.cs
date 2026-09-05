using System;
using System.Collections.Generic;
using System.Reflection;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Quest;
using Durango.Modding;
using Durango.Network;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace DurangoMemoryBot
{
    /// <summary>
    /// โหมด "เล่นแบบคน" — ไม่ใช่บอทฟาร์ม ไม่ใช่ตัวไล่เช็คลิสต์
    ///
    /// [3 ก.ย. 2026] เจ้าของสั่ง: "เป้าหมายคือต้องเล่นได้เหมือนมนุษย์จริง ๆ"
    /// วงจรของผู้เล่นใหม่ในเกมนี้คือ เก็บของ → ทำเครื่องมือ → เครื่องมือเปิดทางให้เก็บของที่ดีกว่า
    /// → ล่า → แล่ → ก่อไฟ → ทำอาหาร → สร้าง → ต่อแพ (ตรงกับสายเนื้อเรื่อง "sunset" ที่เซิร์ฟมี)
    ///
    /// ลำดับความคิดทุกติ๊ก (บนสุดชนะ):
    ///   1. ยังไม่เข้าโลก → กดเข้าเซิร์ฟเองจากหน้าไตเติ้ล
    ///   2. ตาย → รอสักครู่แล้วฟื้น
    ///   3. ร่างกาย (MemoryBotBrain): โดนตี/หนี · หิว · ล้า · ถุงเต็ม
    ///   4. งานที่ค้างอยู่กลางคัน (กำลังสร้าง / กำลังเดินตามเส้นทาง)
    ///   5. พักมองรอบ ๆ เป็นครั้งคราว (คนไม่ได้ทำงานติดกันตลอด)
    ///   6. รับรางวัลเควส · เรียนสกิลที่มีแต้ม
    ///   7. เป้าสำรอง (ต้องมีมีดก่อนถึงจะล่าได้ · ต้องมีกิ่งก่อนถึงจะก่อไฟได้ …)
    ///   8. เป้าหลัก = เควสเนื้อเรื่องข้อถัดไป · หมดเควสแล้ว = เล่นอิสระ (ล่า/เก็บ/คราฟต์วนไป)
    ///
    /// ไม่มี cheat แม้แต่คำสั่งเดียว — ทุกอย่างผ่านระบบเกมและด่านตรวจของเซิร์ฟเหมือนผู้เล่นจริง
    /// </summary>
    internal static class MemoryBotLife
    {
        public const string StoryCategory = "sunset";

        public static bool Running;
        /// <summary>สมองร่างกายบอกว่าล้าแต่หาที่พักไม่เจอ — ให้ชีวิตไปก่อไฟ</summary>
        public static bool NeedFire;

        private static IClientModApi _api;
        private static readonly System.Random Rng = new System.Random();
        private static string _phase = "stopped";
        private static string _reason = "";
        private static string _story = "";
        private static float _nextAction;
        private static float _nextQuestRefresh;
        private static float _nextIdle;
        private static float _idleUntil;
        private static float _nextSkillTry;
        private static float _startedAt;
        private static int _deaths, _crafts, _builds, _claims;
        private static Vector3? _home;
        private static string _homeFireId;
        private static string _lastQuestId;
        private static float _questStuckSince;
        private static readonly HashSet<string> Skipped = new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<string> LockedBlueprints = new HashSet<string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, float> Cooldown = new Dictionary<string, float>(StringComparer.Ordinal);

        // หน้าไตเติ้ล
        private static bool _clusterChosen;
        private static int _titleTries;
        private static float _titleAt;

        // เล่นอิสระ
        private static int _freeStep;
        private static float _freeUntil;

        public static void Initialize(IClientModApi api) { _api = api; }

        public static string Start(MemoryBotRequest request)
        {
            Running = true;
            _phase = "starting";
            _reason = "เริ่มใช้ชีวิต";
            _nextAction = 0f;
            _nextQuestRefresh = 0f;
            _nextIdle = Time.time + 120f + J(120f);
            _idleUntil = 0f;
            _startedAt = Time.time;
            _clusterChosen = false;
            _titleTries = 0;
            _home = null;
            _homeFireId = null;
            _lastQuestId = null;
            _questStuckSince = 0f;
            NeedFire = false;
            Skipped.Clear();
            LockedBlueprints.Clear();
            Cooldown.Clear();
            MemoryBotGoals.Clear();
            MemoryBotAutopilot.Chatty = false;   // คนจริงไม่ประกาศในแชททุกครั้งที่จะเก็บหิน
            MemoryBotUi.UnlockAll();   // ล้างล็อกค้างจากรอบก่อน (blade_stone ที่เคยถูกล็อกกันทิ้ง)
            if (request != null && request.HasCount && request.Count >= 0 && request.Count <= 20)
                MemoryBotBrain.LevelMargin = request.Count;
            Log("เริ่มโหมดชีวิต · เลเวลสัตว์ที่กล้าสู้ = เลเวลเรา+" + MemoryBotBrain.LevelMargin);
            return StatusJson();
        }

        public static void Stop()
        {
            Running = false;
            _phase = "stopped";
            MemoryBotBuild.Cancel("stopped");
            MemoryBotGoals.Clear();
            MemoryBotMove.Stop();
            MemoryBotAutopilot.Chatty = true;
            Log("หยุดโหมดชีวิต");
        }

        public static string StatusJson()
        {
            PlayerBehavior p = PlayerBehavior.LocalPlayer;
            return "{\"owner\":\"life\",\"running\":" + (Running ? "true" : "false")
                + ",\"mode\":\"life\""
                + ",\"phase\":" + MemoryBotProtocol.Quote(_phase)
                + ",\"story\":" + MemoryBotProtocol.Quote(_story ?? "")
                + ",\"goals\":" + MemoryBotGoals.ToJson()
                + ",\"goal\":" + MemoryBotProtocol.Quote(MemoryBotGoals.Describe())
                + ",\"level\":" + (p != null ? p.Level : 0)
                + ",\"home\":" + (_home.HasValue ? "true" : "false")
                + ",\"deaths\":" + _deaths + ",\"crafts\":" + _crafts + ",\"builds\":" + _builds + ",\"claims\":" + _claims
                + ",\"uptime\":" + F(Running ? Time.time - _startedAt : 0f)
                + ",\"build\":" + MemoryBotBuild.StatusJson()
                + ",\"autopilot_reason\":" + MemoryBotProtocol.Quote(MemoryBotAutopilot.LastReason ?? "")
                + ",\"last_reason\":" + MemoryBotProtocol.Quote(_reason ?? "") + "}";
        }

        // ───────────────────────── วงจรหลัก ─────────────────────────

        public static void Tick()
        {
            if (!Running) return;
            try { TickInner(); }
            catch (Exception e)
            {
                _phase = "error";
                _reason = e.GetType().Name + ":" + e.Message;
                _nextAction = Time.time + 4f;
                Log("life error: " + _reason + "\n" + e.StackTrace);
            }
        }

        private static void TickInner()
        {
            float now = Time.time;
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (!GameManager.IsMainScene || !GameManager.IsReady || player == null)
            {
                TickTitle();
                return;
            }
            _clusterChosen = false;
            _titleTries = 0;

            if (!player.IsAlive)
            {
                if (now < _nextAction) return;
                Connections.Frontend.Send(new Revive { WarpholeTile = null });
                _deaths++;
                Set("dead", "ตาย… รอฟื้น (ครั้งที่ " + _deaths + ")", 4f + J(3f));
                MemoryBotBuild.Cancel("died");
                return;
            }
            if (now < _nextAction) return;

            if (MemoryBotBrain.Tick(player))
            {
                Set(MemoryBotBrain.Phase, MemoryBotBrain.Reason, MemoryBotBrain.Delay);
                return;
            }
            if (MemoryBotBuild.Active)
            {
                MemoryBotBuild.Tick();
                Set("build", MemoryBotBuild.Reason, 0.3f);
                if (!MemoryBotBuild.Active) OnBuildEnded();
                return;
            }
            if (MemoryBotMove.Routing)
            {
                Set("walking", "เดินตามเส้นทาง (" + MemoryBotMove.RouteStatus + ")", 0.5f);
                return;
            }
            if (now < _idleUntil)
            {
                Set("idle", "พักมองรอบ ๆ", 0.5f);
                return;
            }
            if (now > _nextIdle)
            {
                _idleUntil = now + 3f + J(6f);
                _nextIdle = now + 150f + J(200f);
                MemoryBotMove.Stop();
                Set("idle", "หยุดพักมองรอบ ๆ สักครู่", 0.5f);
                return;
            }

            RefreshQuests(now);
            if (ClaimReward()) return;
            // เหนื่อยแต่ไม่มีไฟ: ดันเป้า "ก่อไฟ" ขึ้นกอง แล้ว **ปล่อยให้ระบบเป้าสร้างจริง** (อย่า return
            // ทุกติ๊ก ไม่งั้นเป้าก่อไฟไม่เคยได้ทำ) · EnsureFire เจอไฟแล้วจะเคลียร์ NeedFire เอง
            if (NeedFire) EnsureFire();
            if (TryLearnSkill(now)) return;

            BotGoal top = MemoryBotGoals.Peek();
            if (top != null && top.Kind != BotGoalKind.Daily)
            {
                TickGoal(top, player);
                return;
            }
            TickStory(player);
        }

        private static void Set(string phase, string reason, float delay)
        {
            _phase = phase;
            _reason = reason;
            _nextAction = Time.time + delay;
        }

        // ───────────────────────── หน้าไตเติ้ล: กดเข้าเซิร์ฟเอง ─────────────────────────

        private static void TickTitle()
        {
            float now = Time.time;
            if (!GameManager.IsTitleScene)
            {
                Set("loading", "กำลังโหลด…", 1f);
                return;
            }
            if (now - _titleAt < 3f) return;
            _titleAt = now;
            if (_titleTries++ > 40)
            {
                Set("title_stuck", "เข้าโลกไม่ได้ (ลอง 40 ครั้ง) — ให้คนกดเอง", 10f);
                return;
            }
            Durango.UI.TitleMenuUserControlBase uc = UnityEngine.Object.FindObjectOfType<Durango.UI.TitleMenuUserControlBase>();
            if (uc == null)
            {
                Set("title", "รอหน้าไตเติ้ลพร้อม", 1f);
                return;
            }
            Type t = typeof(Durango.UI.TitleMenuUserControlBase);
            BindingFlags any = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            // รอให้หน้าไตเติ้ลถึงขั้น "เลือกเซิร์ฟ" ก่อน — กดตอน Initial = รายชื่อคลัสเตอร์ยังไม่มา (เคย throw)
            // และถ้าเป็นหน้า "ต้องอัปเดต" ห้ามกดยืนยัน (ปุ่มนั้นคือ "ปิดเกมแล้วอัปเดต" เกมจะปิดตัวเอง)
            string state = "";
            try
            {
                FieldInfo ls = t.GetField("LastState", any);
                if (ls != null) state = ls.GetValue(uc).ToString();
            }
            catch (Exception) { }
            if (state != "SelectCluster")
            {
                Set("title", "หน้าไตเติ้ลอยู่ขั้น " + state + " รอให้ถึงหน้าเลือกเซิร์ฟ", 1f);
                if (state == "NeedUpdate" || state == "Update" || state.IndexOf("Update", StringComparison.OrdinalIgnoreCase) >= 0)
                    Log("หน้าไตเติ้ลบอกให้อัปเดต (" + state + ") — เวอร์ชันเกมกับ launcher_patch.json ของเซิร์ฟไม่ตรงกัน บอทจะไม่กดปุ่มนั้น");
                return;
            }
            if (GameManager.Emigrated == GameManager.EmigratedType.None && !_clusterChosen)
            {
                // เลือกปุ่ม "Online Server" เหมือนคนกดเลือกเซิร์ฟเวอร์
                MethodInfo choose = t.GetMethod("OnClusterConfirmed", any);
                if (choose != null)
                {
                    try { choose.Invoke(uc, new object[] { "online" }); }
                    catch (Exception e) { Log("เลือกคลัสเตอร์ online ไม่ได้: " + e.Message); }
                }
                _clusterChosen = true;
                Set("title", "เลือกเซิร์ฟออนไลน์", 1f);
                return;
            }
            FieldInfo ready = t.GetField("IsAccountReady", any);
            bool accountReady = ready == null || (bool)ready.GetValue(uc);
            if (!accountReady)
            {
                Set("title", "รอรายชื่อตัวละครจากเซิร์ฟ", 1f);
                return;
            }
            MethodInfo confirm = t.GetMethod("OnConfirm", any);
            if (confirm == null)
            {
                Set("title", "ไม่เจอปุ่มยืนยัน", 1f);
                return;
            }
            try { confirm.Invoke(uc, null); }
            catch (Exception e) { Log("กดยืนยันไม่ได้: " + e.Message); }
            Set("title", GameManager.Emigrated == GameManager.EmigratedType.None ? "กดยืนยันเข้าเซิร์ฟ" : "กดเข้าโลก", 1f);
        }

        // ───────────────────────── เควส ─────────────────────────

        private static List<QuestToDo> _quests = new List<QuestToDo>();

        private static void RefreshQuests(float now)
        {
            QuestSystem quests = GameSystem<QuestSystem>.HasInstance() ? GameSystem<QuestSystem>.Instance() : null;
            if (quests == null) return;
            Category cat = quests.GetCategory(StoryCategory);
            List<QuestToDo> got = cat == null ? null : cat.GetCachedQuestList();
            if (got != null) _quests = got;
            if (now > _nextQuestRefresh)
            {
                _nextQuestRefresh = now + 15f;
                quests.GetQuests(StoryCategory);
            }
        }

        private static bool ClaimReward()
        {
            QuestSystem quests = GameSystem<QuestSystem>.HasInstance() ? GameSystem<QuestSystem>.Instance() : null;
            if (quests == null) return false;
            foreach (QuestToDo q in _quests)
            {
                if (q.Id == null || q.Finished || q.Progress < q.GoalCount) continue;
                if (OnCooldown("claim:" + q.Id)) continue;
                Cool("claim:" + q.Id, 8f);
                quests.RequestQuestReward(q.Id);
                _claims++;
                Set("claim", "รับรางวัลเควส " + q.Id, 2.5f + J(3f));
                Log("รับรางวัล " + q.Id);
                return true;
            }
            return false;
        }

        private static bool FindQuest(string id, out QuestToDo quest)
        {
            foreach (QuestToDo q in _quests)
            {
                if (q.Id == id) { quest = q; return true; }
            }
            quest = default(QuestToDo);
            return false;
        }

        private static bool NextStoryQuest(out QuestToDo quest)
        {
            foreach (QuestToDo q in _quests)
            {
                if (q.Id == null || q.Finished || q.Progress >= q.GoalCount) continue;
                if (Skipped.Contains(q.Id)) continue;
                quest = q;
                return true;
            }
            quest = default(QuestToDo);
            return false;
        }

        private static void TickStory(PlayerBehavior player)
        {
            QuestToDo q;
            if (!NextStoryQuest(out q))
            {
                _story = _quests.Count == 0 ? "รอเควส" : "เล่นอิสระ";
                FreePlay(player);
                return;
            }
            if (_lastQuestId != q.Id)
            {
                _lastQuestId = q.Id;
                _questStuckSince = Time.time;
                Log("เควสถัดไป: " + q.Id + " " + q.Progress + "/" + q.GoalCount);
            }
            _story = q.Id + " " + q.Progress + "/" + q.GoalCount;
            int remain = q.GoalCount - q.Progress;
            // เควสที่ต้องสร้างของที่ยังปลดล็อกไม่ได้ (ตัวละครข้ามด่านสอนเล่น) → ข้ามไปเควสถัดไป
            if ((q.Id == "daily_constructing_a_01" && LockedBlueprints.Contains("camp_square_fire"))
                || (q.Id == "story_enter_safehouse" && LockedBlueprints.Contains("tutorial_boat")))
            {
                Skipped.Add(q.Id);
                Set("skip", "เควส " + q.Id + " ต้องสร้างของที่ยังปลดล็อกไม่ได้ ข้ามไปก่อน", 2f);
                return;
            }
            switch (q.Id)
            {
                case "event_2018_fall_3_17_any_gathering_01":
                    Gather("any", 1, "เก็บของรอบตัว");
                    break;
                case "daily_weaponcrafting_b_01":
                    if (!EnsureKnife()) return;
                    if (!EnsureAxe()) return;
                    Craft("blade_stone", "ทำเครื่องมือ");
                    break;
                case "customize_estate_quest_05":
                    if (!EnsureAxe()) return;
                    Gather("wood_log", 1, "ตัดท่อนซุง");
                    break;
                case "anniversary_1st_03":
                case "story_enter_risky":
                    if (!EnsureKnife()) return;
                    Hunt("ล่าสัตว์");
                    break;
                case "permanent_butchery_meat_01":
                    if (!EnsureKnife()) return;
                    Hunt("ล่าแล้วแล่เนื้อ");
                    break;
                case "permanent_cooking_any_01":
                    Cook();
                    break;
                case "daily_constructing_a_01":
                    if (!EnsureKnife()) return;
                    Build("camp_square_fire", "ก่อไฟ");
                    break;
                case "story_enter_safehouse":
                    if (!EnsureAxe()) return;
                    if (CountProto("wood_log") < 4 && !HasAll("wood_log", 4)) { Gather("wood_log", 4, "ตัดซุงทำแพ"); return; }
                    if (CountProto("stem") < 6) { Gather("stem", 6, "เก็บลำต้นทำแพ"); return; }
                    Build("tutorial_boat", "ต่อแพ");
                    break;
                case "story_enter_personal":
                    Craft("blade_stone", "คราฟต์ 5 ชิ้น");
                    break;
                case "story_custom_event_meet_k":
                    Skipped.Add(q.Id);
                    Set("skip", "ยังไม่รู้ตำแหน่งหาดเหนือ ข้ามเควสนี้ไปก่อน", 2f);
                    break;
                case "story_level_16":
                    FreePlay(player);
                    break;
                default:
                    Skipped.Add(q.Id);
                    Set("skip", "ไม่รู้จักเควส " + q.Id, 2f);
                    break;
            }
        }

        // ───────────────────────── เล่นอิสระ (ไม่มีเควส) ─────────────────────────

        private static void FreePlay(PlayerBehavior player)
        {
            float now = Time.time;
            if (now > _freeUntil)
            {
                _freeStep = (_freeStep + 1) % 4;
                _freeUntil = now + 90f + J(120f);
            }
            if (!EnsureKnife()) return;
            switch (_freeStep)
            {
                case 0: Gather("any", 1, "เดินเก็บของ"); break;
                case 1: Hunt("ออกล่า"); break;
                case 2:
                    if (CountTag("eatable") > 0 && !HasFood(2)) { Cook(); }
                    else Craft("blade_stone", "ฝึกคราฟต์");
                    break;
                default:
                    if (_home.HasValue) { if (!GoHome()) Set("home", "อยู่ที่แคมป์", 3f + J(4f)); }
                    else EnsureFire();
                    break;
            }
        }

        // ───────────────────────── กิจกรรมพื้นฐาน ─────────────────────────

        private static void Gather(string filter, int need, string label)
        {
            if (MemoryBotUi.IsInventoryFull())
            {
                if (MemoryBotBrain.FreeBag()) { Set("bag", "กระเป๋าเต็ม จัดของ", 1.5f); return; }
                Set("bag_full", "กระเป๋าเต็ม เก็บต่อไม่ได้", 5f);
                return;
            }
            if (!EnsureToolFor(filter)) return;
            _phase = "gather";
            _reason = label + " (" + filter + ")";
            MemoryBotAutopilot.TickGather(filter);
            _nextAction = Time.time + 0.25f;
        }

        private static void Hunt(string label)
        {
            if (MemoryBotUi.IsInventoryFull())
            {
                if (MemoryBotBrain.FreeBag()) { Set("bag", "กระเป๋าเต็ม จัดของ", 1.5f); return; }
            }
            _phase = "hunt";
            _reason = label;
            MemoryBotAutopilot.TickHunt();
            _nextAction = Time.time + 0.25f;
        }

        private static void Craft(string recipeId, string label)
        {
            string detail;
            string error = MemoryBotCommands.CraftRecipe(recipeId, false, out detail);
            if (error == null)
            {
                _crafts++;
                Set("craft", label + ": " + (detail ?? recipeId), 3.5f + J(1.5f));
                return;
            }
            HandleCraftError(recipeId, error, detail);
        }

        private static void HandleCraftError(string recipeId, string error, string detail)
        {
            if (error == "material_locked")
            {
                // ปลดล็อกวัสดุแล้ว รอ packet ไป-กลับเซิร์ฟ ค่อยคราฟต์ใหม่รอบถัดไป
                Set("craft", "ปลดล็อกวัสดุก่อนคราฟต์ " + recipeId, 1.2f);
                return;
            }
            if (error == "missing_material")
            {
                string tag = MemoryBotCommands.LastMissingTag ?? "";
                PushNeedForTag(tag, detail, recipeId);
                return;
            }
            if (error == "workbench_not_in_range")
            {
                if (!EnsureFire()) Set("craft", "ต้องทำที่กองไฟ", 1f);
                return;
            }
            if (error == "recipe_locked")
            {
                if (TryLearnSkill(Time.time, true)) return;
                Set("craft_locked", "สูตร " + recipeId + " ยังล็อก (ต้องเลเวล/สกิล)", 6f);
                return;
            }
            Set("craft_error", recipeId + ": " + error, 3f);
        }

        /// <summary>ขาดของช่องไหน ก็ดันเป้าสำรองที่ตรงกับ tag ของช่องนั้น (คราฟต์ต่อหรือไปเก็บ)</summary>
        private static void PushNeedForTag(string tag, string slotName, string forWhat)
        {
            string t = tag.ToLowerInvariant();
            if (t.IndexOf("blade", StringComparison.Ordinal) >= 0)
            {
                Need(BotGoalKind.Craft, "blade_stone", 1, "ทำใบมีดหิน (สำหรับ " + forWhat + ")");
                return;
            }
            string filter = FilterForTag(t, slotName);
            Need(BotGoalKind.Gather, filter, 1, "หา " + filter + " (สำหรับ " + forWhat + ")");
        }

        private static string FilterForTag(string t, string slotName)
        {
            string s = (slotName ?? "").ToLowerInvariant();
            if (t.IndexOf("stick", StringComparison.Ordinal) >= 0 || s.IndexOf("กิ่ง", StringComparison.Ordinal) >= 0
                || t == "common" || s.IndexOf("handle", StringComparison.Ordinal) >= 0) return "wood_bough";
            if (t.IndexOf("pillar", StringComparison.Ordinal) >= 0 || t == "log" || s.IndexOf("ซุง", StringComparison.Ordinal) >= 0) return "wood_log";
            if (t.IndexOf("string", StringComparison.Ordinal) >= 0 || t.IndexOf("rope", StringComparison.Ordinal) >= 0
                || t == "connector" || s.IndexOf("เชือก", StringComparison.Ordinal) >= 0 || s.IndexOf("สาย", StringComparison.Ordinal) >= 0) return "flax";
            if (t == "stem" || s.IndexOf("ลำต้น", StringComparison.Ordinal) >= 0) return "stem";
            if (t.IndexOf("eatable", StringComparison.Ordinal) >= 0 || t.IndexOf("food", StringComparison.Ordinal) >= 0
                || t.IndexOf("meat", StringComparison.Ordinal) >= 0) return "food";
            if (t.IndexOf("stone", StringComparison.Ordinal) >= 0 || t.IndexOf("chunk", StringComparison.Ordinal) >= 0
                || t == "base" || s.IndexOf("หิน", StringComparison.Ordinal) >= 0) return "stone";
            if (t.IndexOf("wood", StringComparison.Ordinal) >= 0 || t.IndexOf("burnable", StringComparison.Ordinal) >= 0) return "wood_bough";
            return "stone";
        }

        private static void Build(string blueprintId, string label)
        {
            if (!MemoryBotUi.IsInventoryFull() && !EnsureToolFor("wood_bough")) return;
            string error = MemoryBotBuild.Start(blueprintId);
            if (error == null)
            {
                Set("build", label + ": " + MemoryBotBuild.Reason, 0.5f);
                return;
            }
            if (error == "missing_material")
            {
                PushNeedForTag(MemoryBotBuild.MissingTag ?? "", MemoryBotBuild.MissingSlot, blueprintId);
                return;
            }
            if (error == "no_place")
            {
                // ตรงนี้แน่นเกิน เดินออกไปหาที่โล่ง
                MemoryBotAutopilot.WanderPublic();
                Set("build", "หาที่วาง " + blueprintId + " ไม่ได้ เดินไปหาที่โล่ง", 3f);
                return;
            }
            // พิมพ์เขียวยังไม่ปลด (ตัวละครนี้ข้ามด่านสอนเล่นมา) — เลิกพยายามสร้าง ไม่วนซ้ำ
            if (error == "blueprint_locked" || error == "required_blueprint_missing")
            {
                LockedBlueprints.Add(blueprintId);
                DropBuildGoal(blueprintId);
                if (blueprintId == "camp_square_fire") { NeedFire = false; Cool("fire", 300f); }
                Set("build_locked", blueprintId + " ยังไม่ปลดล็อก (ตัวละครข้ามด่านสอนเล่น) — ข้ามไปทำอย่างอื่น", 8f);
                return;
            }
            Set("build_error", blueprintId + ": " + error, 5f);
        }

        private static void OnBuildEnded()
        {
            if (MemoryBotBuild.Phase == "done")
            {
                _builds++;
                if (MemoryBotBuild.LastBuiltBlueprint == "camp_square_fire")
                {
                    Artifact fire = FindFire();
                    if (fire != null) { _home = fire.Center; _homeFireId = fire.EntityId; }
                    NeedFire = false;
                }
                Set("built", "สร้าง " + MemoryBotBuild.LastBuiltBlueprint + " เสร็จ", 2f + J(3f));
                BotGoal top = MemoryBotGoals.Peek();
                if (top != null && top.Kind == BotGoalKind.Build && top.Target == MemoryBotBuild.LastBuiltBlueprint)
                    MemoryBotGoals.CompleteCurrent();
                return;
            }
            if (MemoryBotBuild.Phase == "need_material")
            {
                PushNeedForTag(MemoryBotBuild.MissingTag ?? "", MemoryBotBuild.MissingSlot, "งานสร้าง");
                return;
            }
            Set("build_failed", MemoryBotBuild.Reason, 4f);
        }

        private static void Cook()
        {
            if (CountTag("eatable") == 0)
            {
                if (!EnsureKnife()) return;
                Need(BotGoalKind.Gather, "food", 1, "หาของกินมาทำอาหาร");
                return;
            }
            if (!EnsureFire()) return;
            if (CountTag("stick_normal") == 0 && CountTag("stick_long") == 0 && CountTag("stick_short") == 0)
            {
                if (!EnsureKnife()) return;
                Need(BotGoalKind.Gather, "wood_bough", 1, "หากิ่งเสียบอาหาร");
                return;
            }
            if (!GoHome()) return;
            Craft("skewer", "ปิ้งอาหารที่กองไฟ");
        }

        // ───────────────────────── เงื่อนไขก่อนทำงาน (เป้าสำรอง) ─────────────────────────

        private static bool EnsureKnife()
        {
            if (CountTag("knife") > 0) return true;
            Need(BotGoalKind.Craft, "blade_stone", 1, "ทำมีดหิน");
            return false;
        }

        private static bool EnsureAxe()
        {
            if (CountTag("axe") > 0) return true;
            if (!EnsureKnife()) return false;
            Need(BotGoalKind.Craft, "assembled_axe_one_01", 1, "ประกอบขวาน");
            return false;
        }

        private static bool EnsureToolFor(string filter)
        {
            if (filter == "wood_log") return EnsureAxe();
            if (filter == "wood_bough" || filter == "wood_bush") return EnsureKnife();
            return true;
        }

        /// <summary>มีกองไฟใกล้ ๆ ไหม ไม่มีก็ก่อ — คืน true ถ้ารอบนี้ทำอะไรไปแล้ว (ผู้เรียกต้อง return)</summary>
        private static bool EnsureFire()
        {
            Artifact fire = FindFire();
            if (fire != null)
            {
                _home = fire.Center;
                _homeFireId = fire.EntityId;
                NeedFire = false;
                return false;
            }
            // เพิ่งลองก่อไฟแล้วปลดล็อกไม่ได้ — อย่าวนพยายามใหม่จนกว่าจะพ้น cooldown
            if (OnCooldown("fire")) { NeedFire = false; return false; }
            if (!EnsureKnife()) return true;
            Need(BotGoalKind.Build, "camp_square_fire", 1, "ก่อไฟ");
            return true;
        }

        /// <summary>ทิ้งเป้า Build ของพิมพ์เขียวนี้ออกจากกอง (ปลดล็อกไม่ได้ ไม่ต้องวน)</summary>
        private static void DropBuildGoal(string blueprintId)
        {
            BotGoal top = MemoryBotGoals.Peek();
            if (top != null && top.Kind == BotGoalKind.Build && top.Target == blueprintId)
                MemoryBotGoals.CompleteCurrent();
        }

        private static bool GoHome()
        {
            if (!_home.HasValue) return true;
            float d = MemoryBotMove.FlatDistance(_home.Value);
            if (d <= 350f) return true;
            MemoryBotMove.Near(_home.Value, 120f, 250f, Rng);
            Set("walk_home", "เดินกลับไปที่กองไฟ", 1f);
            return false;
        }

        private static Artifact FindFire()
        {
            if (!ArtifactManager.HasInstance()) return null;
            PlayerBehavior me = PlayerBehavior.LocalPlayer;
            Artifact best = null;
            float bestDist = 2500f;
            foreach (Artifact a in ArtifactManager.Instance().GetArtifacts())
            {
                if (a == null || a.gameObject == null || a.Blueprint == null) continue;
                if (!a.BuildCompleted && a.BuildState != Shared.Building.BuildingState.Built) continue;
                string id = a.BlueprintId ?? "";
                bool fire = id.IndexOf("fire", StringComparison.OrdinalIgnoreCase) >= 0
                    || (a.Blueprint.HasComponent("Shelter"));
                if (!fire) continue;
                float d = me == null ? 0f : Vector3.Distance(me.CurrentPosition, a.Center);
                if (d < bestDist) { bestDist = d; best = a; }
            }
            return best;
        }

        // ───────────────────────── เป้าสำรอง ─────────────────────────

        private static void Need(BotGoalKind kind, string target, int count, string label)
        {
            if (MemoryBotGoals.Need(kind, target, count, label, "sub"))
                Log("เป้าสำรอง: " + label);
            Set("need", label, 0.4f);
        }

        /// <summary>เป้านี้ทำสำเร็จแล้วหรือยัง (ใช้ตัดเป้ารองที่ทำเผื่อไว้แต่ไม่จำเป็นแล้ว)</summary>
        private static bool IsGoalSatisfied(BotGoal goal)
        {
            if (goal == null) return true;
            int need = Math.Max(1, goal.Count);
            switch (goal.Kind)
            {
                case BotGoalKind.Craft: return CountProto(goal.Target) + CountForRecipe(goal.Target) >= need;
                case BotGoalKind.Gather: return CountForFilter(goal.Target) >= need;
                case BotGoalKind.Hunt: return CountTag("meat") >= need;
                default: return false;   // Build/Daily เช็คไม่ได้ตรง ๆ ปล่อยให้ handler จัดการ
            }
        }

        private static void TickGoal(BotGoal goal, PlayerBehavior player)
        {
            // [3 ก.ย. 2026] เป้ารองที่ทำเพื่อเป้าหลัก — ถ้าเป้าหลักข้างล่างพอแล้ว ให้เด้งเป้ารองนี้ทิ้ง
            //    (กันอาการ: ประกอบขวานได้แล้ว 4 อัน แต่ยังวนหา flax ทำอันที่ 5 เพราะเป้ารองไม่ยอมจบ
            //    เพราะแมพไม่มี flax) — เช็คเป้าที่อยู่ใต้เป้ารองนี้ ถ้าเสร็จแล้วก็ไม่ต้องทำเป้ารองต่อ
            if (string.Equals(goal.Source, "sub", StringComparison.Ordinal))
            {
                BotGoal parent = MemoryBotGoals.PeekUnder();
                if (parent != null && IsGoalSatisfied(parent))
                {
                    MemoryBotGoals.CompleteCurrent();
                    Set("goal_skip", "เป้าหลักพอแล้ว ไม่ต้องทำ " + goal.Label + " ต่อ", 0.5f);
                    return;
                }
            }
            // เป้าที่ค้างนานเกินไป = ทำไม่ได้จริง ทิ้งแล้วไปต่อ (เป้ารองที่หาของบนแมพไม่เจอ ให้เลิกเร็ว)
            float limit = string.Equals(goal.Source, "sub", StringComparison.Ordinal) ? 90f : 600f;
            if (Time.time - goal.StartedAt > limit)
            {
                Log("เป้า " + goal.Label + " ค้างเกิน " + (int)limit + " วิ ทิ้ง");
                MemoryBotGoals.CompleteCurrent();
                Set("give_up", "ทำ " + goal.Label + " ไม่สำเร็จ ข้ามไปก่อน", 2f);
                return;
            }
            switch (goal.Kind)
            {
                case BotGoalKind.Craft:
                {
                    int have = CountProto(goal.Target) + CountForRecipe(goal.Target);
                    if (have >= Math.Max(1, goal.Count))
                    {
                        MemoryBotGoals.CompleteCurrent();
                        Set("goal_done", "ได้ " + goal.Target + " แล้ว", 0.8f + J(0.8f));
                        return;
                    }
                    Craft(goal.Target, goal.Label);
                    return;
                }
                case BotGoalKind.Gather:
                {
                    int have = CountForFilter(goal.Target);
                    if (have >= Math.Max(1, goal.Count))
                    {
                        MemoryBotGoals.CompleteCurrent();
                        Set("goal_done", "เก็บ " + goal.Target + " ครบ", 0.6f + J(0.8f));
                        return;
                    }
                    Gather(goal.Target, goal.Count, goal.Label);
                    return;
                }
                case BotGoalKind.Hunt:
                {
                    if (CountTag("meat") >= Math.Max(1, goal.Count))
                    {
                        MemoryBotGoals.CompleteCurrent();
                        return;
                    }
                    if (!EnsureKnife()) return;
                    Hunt(goal.Label);
                    return;
                }
                case BotGoalKind.Build:
                    Build(goal.Target, goal.Label);
                    return;
                default:
                    MemoryBotGoals.CompleteCurrent();
                    return;
            }
        }

        // ───────────────────────── สกิล ─────────────────────────

        private static readonly Shared.Skill.Category[] SkillPreference =
        {
            Shared.Skill.Category.Gathering, Shared.Skill.Category.MeleeCombat, Shared.Skill.Category.Weaponcrafting,
            Shared.Skill.Category.Butchery, Shared.Skill.Category.Cooking, Shared.Skill.Category.Survival,
            Shared.Skill.Category.Constructing
        };

        private static bool TryLearnSkill(float now, bool force = false)
        {
            if (!force && now < _nextSkillTry) return false;
            _nextSkillTry = now + 8f;
            SkillSystem skills = GameSystem<SkillSystem>.HasInstance() ? GameSystem<SkillSystem>.Instance() : null;
            if (skills == null || skills.Skills == null || skills.RemainSkillPoint <= 0) return false;
            for (int p = 0; p < SkillPreference.Length; p++)
            {
                foreach (Durango.Logic.Skill.Bundle bundle in skills.Skills)
                {
                    if (bundle == null || bundle.Category != SkillPreference[p]) continue;
                    if (TryLearn(bundle.Base)) return true;
                    if (bundle.Sub == null) continue;
                    foreach (Durango.Logic.Skill.Skill s in bundle.Sub) if (TryLearn(s)) return true;
                }
            }
            return false;
        }

        private static bool TryLearn(Durango.Logic.Skill.Skill skill)
        {
            if (skill == null || skill.Level >= skill.MaxLevel) return false;
            Durango.Logic.Skill.Node node = skill.Get(skill.Level + 1);
            if (node == null || node.State != Durango.Logic.Skill.State.Learnable) return false;
            if (!MemoryBotUi.OpenSkillMenu(skill)) return false;
            Set("skill", "เรียนสกิล " + skill.Id + " lv" + (skill.Level + 1), 2.5f + J(1.5f));
            Log("เรียนสกิล " + skill.Id);
            return true;
        }

        // ───────────────────────── นับของ ─────────────────────────

        private static IList<ItemData> Items()
        {
            InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            return inv == null ? new List<ItemData>() : inv.PlayerItemList;
        }

        public static int CountTag(string tag)
        {
            int n = 0;
            foreach (ItemData item in Items())
                if (item != null && !item.IsDestroyed() && item.GetTagData(tag) != null) n++;
            return n;
        }

        public static int CountProto(string proto)
        {
            int n = 0;
            foreach (ItemData item in Items())
                if (item != null && !item.IsDestroyed() && string.Equals(item.PrototypeId, proto, StringComparison.OrdinalIgnoreCase)) n++;
            return n;
        }

        private static bool HasAll(string proto, int n) { return CountProto(proto) >= n; }

        private static bool HasFood(int n) { return CountTag("eatable") >= n; }

        /// <summary>ของที่คราฟต์ได้จากสูตรนี้ (ชื่อไอเทมอาจไม่ตรง id สูตร เช่น assembled_axe_one_01 → axe_onehand_assembled_stone)</summary>
        private static int CountForRecipe(string recipeId)
        {
            if (recipeId.IndexOf("axe", StringComparison.OrdinalIgnoreCase) >= 0) return CountTag("axe");
            if (recipeId.IndexOf("bow", StringComparison.OrdinalIgnoreCase) >= 0) return MemoryBotAutopilot.CountItemsMatching("bow_wooden");
            return 0;
        }

        public static int CountForFilter(string filter)
        {
            switch (filter)
            {
                case "stone": return CountProto("stone");
                case "stem": return CountProto("stem");
                case "wood_log": return CountProto("wood_log");
                case "wood_bough":
                case "stick": return CountTag("stick_normal") + CountTag("stick_long");
                case "flax":
                case "string": return CountTag("string_normal") + CountTag("string_long");
                case "food":
                case "berry":
                case "fruit_berry": return CountTag("eatable");
                case "wood_bush": return CountProto("wood_bush");
                case "any": return 0;
                default: return MemoryBotAutopilot.CountItemsMatching(filter);
            }
        }

        // ───────────────────────── เครื่องมือเล็ก ๆ ─────────────────────────

        private static bool OnCooldown(string key)
        {
            float until;
            return Cooldown.TryGetValue(key, out until) && until > Time.time;
        }

        private static void Cool(string key, float seconds) { Cooldown[key] = Time.time + seconds; }

        private static float J(float max) { return (float)Rng.NextDouble() * max; }

        private static string F(float v) { return v.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture); }

        private static void Log(string text)
        {
            if (_api != null) _api.Log("[life] " + text);
        }
    }
}
