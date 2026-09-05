using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Logic.Quest;
using Durango.Modding;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils;
using InteractionData;
using Messages;
using Shared.Battle;
using Shared.Building;
using Shared.Etc;
using UnityEngine;

namespace DurangoMemoryBot
{
    /// <summary>
    /// Test-only daily quest runner. It reads client state and sends the same
    /// messages the normal client sends; cheats are limited to fixture setup.
    /// </summary>
    internal static class MemoryBotDaily
    {
        private static readonly string[] TaskOrder =
        {
            "permanent_farming_seed_01",
            "event_1_farming_cherry_01",
            "event_1_farming_cherry_02",
            "event_1_gathering_cherry_bough_01",
            "event_newyear_2019_quest_04",
            "urban_weapon_event_04",
            "urban_weapon_event_10",
            "mainstory_chapter1_5",
            "mainstory_chapter4_6",
            "estate_build_lv55_02",
            "permanent_level_skill_gathering_10",
            "urban_cook_event_06",
            "daily_survival_rest",
            "daily_local_warp",
            "daily_island_travel"
        };

        private static IClientModApi _api;
        private static bool _running;
        private static bool _testProvisioning;
        private static bool _autoReward = true;
        private static string _phase = "stopped";
        private static string _taskId = "";
        private static string _lastReason = "";
        private static float _nextAction;
        private static float _lastQuestRequest;
        private static int _fixtureStep;
        private static bool _growthRushed;
        private static bool _reviveRequested;
        private static bool _boxPlacementRequested;
        private static bool _travelRequested;
        private static bool _skillLearning;
        private static string _interactionTask = "";
        private static string _interactionArtifactId = "";
        private static int _taskAttempts;

        public static bool IsRunning => _running;
        public static bool TestProvisioning => _testProvisioning;
        public static bool AutoReward => _autoReward;

        public static void Initialize(IClientModApi api)
        {
            _api = api;
            _testProvisioning = string.Equals(Environment.GetEnvironmentVariable("DURANGO_MEMORYBOT_TEST"), "1", StringComparison.Ordinal);
            MemoryBotBrain.LoadKnobs();
        }

        public static void Configure(bool testProvisioning, bool autoReward)
        {
            _testProvisioning = testProvisioning;
            _autoReward = autoReward;
        }

        public static string Execute(MemoryBotRequest request)
        {
            string name = request.Name ?? "";
            if (name == "bot.start" || name == "bot.daily.start")
            {
                _running = true;
                _phase = "waiting";
                _taskId = "";
                _lastReason = "started";
                _nextAction = 0f;
                _lastQuestRequest = 0f;
                _fixtureStep = 0;
                _growthRushed = false;
                _reviveRequested = false;
                _boxPlacementRequested = false;
                _travelRequested = false;
                _skillLearning = false;
                _interactionTask = "";
                _interactionArtifactId = "";
                _taskAttempts = 0;
                Log("daily runner started test_fixture=" + _testProvisioning + " auto_reward=" + _autoReward);
                return StatusJson();
            }
            if (name == "bot.stop" || name == "bot.daily.stop")
            {
                Stop();
                return StatusJson();
            }
            if (name == "bot.status") return StatusJson();
            if (name == "bot.goal")
            {
                string error;
                if (!StartSpecial(request, out error))
                    return "{\"status\":\"rejected\",\"reason\":" + MemoryBotProtocol.Quote(error) + "}";
                return StatusJson();
            }
            return "{\"status\":\"rejected\",\"reason\":\"unknown_daily_command\"}";
        }

        private static bool StartSpecial(MemoryBotRequest request, out string error)
        {
            error = null;
            BotGoalKind kind;
            if (!MemoryBotGoals.TryParseKind(request.Kind, out kind) || kind == BotGoalKind.Daily)
            {
                error = "special_goal_needs_kind";
                return false;
            }
            string target = request.EntityId;
            if (string.IsNullOrEmpty(target)) target = request.MenuId;
            if (string.IsNullOrEmpty(target)) target = request.ItemId;
            int count = request.HasCount ? request.Count : 0;
            if (kind == BotGoalKind.Craft)
            {
                if (string.IsNullOrEmpty(target)) { error = "craft_goal_needs_recipe"; return false; }
                string resolved = MemoryBotCommands.ResolveRecipeId(target);
                if (resolved != null) target = resolved;
                if (count <= 0) count = 1;
            }
            if ((kind == BotGoalKind.Gather || kind == BotGoalKind.Hunt) && count <= 0) count = 1;
            if (kind == BotGoalKind.Level)
            {
                int level;
                if (count <= 0 && int.TryParse(target, out level)) count = level;
                if (count <= 0) { error = "level_goal_needs_count"; return false; }
                target = count.ToString();
            }
            if (!_running)
            {
                _running = true;
                _phase = "special";
                _nextAction = 0f;
            }
            _testProvisioning = false;
            MemoryBotUi.OpenCraftAndSkillMenus();
            MemoryBotGoals.PushSpecial(kind, target ?? "", count, SpecialLabel(kind, target, count));
            MemoryBotAutopilot.ResetSkillSession();
            _lastReason = "special_" + MemoryBotGoals.KindName(kind);
            Log("เป้าพิเศษ: " + MemoryBotGoals.Describe());
            return true;
        }

        private static string SpecialLabel(BotGoalKind kind, string target, int count)
        {
            switch (kind)
            {
                case BotGoalKind.Craft: return "คราฟต์ " + target;
                case BotGoalKind.Gather: return "เก็บ " + target;
                case BotGoalKind.Hunt: return "ล่า";
                case BotGoalKind.Level: return "เลเวลถึง " + count;
                case BotGoalKind.Skill: return "อัปสกิล";
                default: return MemoryBotGoals.KindName(kind);
            }
        }

        public static void Stop()
        {
            _running = false;
            _phase = "stopped";
            _taskId = "";
            _skillLearning = false;
            _interactionTask = "";
            _interactionArtifactId = "";
            MemoryBotGoals.Clear();
            MemoryBotAutopilot.ResetSkillSession();
            if (PlayerBehavior.LocalPlayer != null && Singleton<PlayerController>.HasInstance())
                Singleton<PlayerController>.Instance().StopMove();
            Log("daily runner stopped");
        }

        public static string StatusJson()
        {
            return "{\"running\":" + (_running ? "true" : "false")
                + ",\"mode\":\"daily\""
                + ",\"phase\":" + MemoryBotProtocol.Quote(_phase)
                + ",\"daily_task\":" + MemoryBotProtocol.Quote(_taskId)
                + ",\"goal\":" + MemoryBotProtocol.Quote(MemoryBotGoals.Describe())
                + ",\"goals\":" + MemoryBotGoals.ToJson()
                + ",\"test_provisioning\":" + (_testProvisioning ? "true" : "false")
                + ",\"auto_reward\":" + (_autoReward ? "true" : "false")
                + ",\"attempts\":" + _taskAttempts
                + ",\"body\":" + (MemoryBotBrain.BodyEnabled ? "true" : "false")
                + ",\"fight_back\":" + (MemoryBotBrain.FightBack ? "true" : "false")
                + ",\"drop_junk\":" + (MemoryBotBrain.DropJunk ? "true" : "false")
                + ",\"last_reason\":" + MemoryBotProtocol.Quote(_lastReason ?? "") + "}";
        }

        public static void Tick()
        {
            if (!_running || Time.time < _nextAction) return;
            try
            {
                TickMain();
            }
            catch (Exception e)
            {
                _phase = "error";
                _lastReason = e.GetType().Name + ":" + e.Message;
                _nextAction = Time.time + 5f;
                Log("daily runner error: " + _lastReason);
            }
        }

        private static void TickMain()
        {
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (!GameManager.IsMainScene || !GameManager.IsReady || player == null)
            {
                // TravelByRegion hands the client to the destination server and
                // briefly returns through Title. Do not leave the runner locked
                // on the old island if the quest notification arrives during
                // that handoff; after reconnect it can safely re-read the cache.
                if (GameManager.IsTitleScene && _taskId == "daily_island_travel" && _travelRequested)
                {
                    _taskId = "";
                    _travelRequested = false;
                }
                _phase = "waiting";
                _lastReason = "game_not_ready";
                return;
            }

            if (!player.IsAlive)
            {
                DoRevive(player);
                return;
            }
            if (MemoryBotBrain.Tick(player))
            {
                _phase = MemoryBotBrain.Phase;
                _lastReason = MemoryBotBrain.Reason;
                _nextAction = Time.time + MemoryBotBrain.Delay;
                return;
            }
            if (!MemoryBotGoals.HasAny) MemoryBotGoals.EnsureDailyMain();
            BotGoal current = MemoryBotGoals.Peek();
            if (current != null && current.Kind != BotGoalKind.Daily)
            {
                TickGoal(current, player);
                return;
            }

            QuestSystem quests = GameSystem<QuestSystem>.HasInstance() ? GameSystem<QuestSystem>.Instance() : null;
            Category category = quests == null ? null : quests.GetCategory("daily");
            if (category == null)
            {
                RequestQuests(quests);
                _phase = "waiting_quests";
                _lastReason = "daily_category_unavailable";
                return;
            }

            List<QuestToDo> todos = category.GetCachedQuestList();
            if (todos == null || todos.Count == 0)
            {
                RequestQuests(quests);
                _phase = "waiting_quests";
                _lastReason = "daily_quests_loading";
                return;
            }

            if (_testProvisioning && _fixtureStep < 14)
            {
                if (SendNextFixture(player)) return;
            }

            QuestToDo reward = FindRewardReady(todos);
            if (_autoReward && reward.Id != null)
            {
                quests.RequestQuestReward(reward.Id);
                _phase = "claiming_reward";
                _lastReason = "claiming_" + reward.Id;
                _nextAction = Time.time + 1.2f;
                return;
            }

            QuestToDo task;
            if (!FindNextTask(todos, out task))
            {
                _phase = "complete";
                _lastReason = "all_daily_quests_complete";
                _nextAction = Time.time + 5f;
                return;
            }
            if (_taskId != task.Id)
            {
                _taskId = task.Id;
                _taskAttempts = 0;
                _interactionTask = "";
                _interactionArtifactId = "";
                if (_taskId != "daily_island_travel") _travelRequested = false;
                if (_taskId != "mainstory_chapter1_5") _reviveRequested = false;
                _lastReason = "selected_" + _taskId;
            }
            if (Time.time < _nextAction) return;
            _taskAttempts++;

            if (!player.IsAlive)
            {
                DoRevive(player);
                return;
            }

            switch (_taskId)
            {
                case "permanent_farming_seed_01": DoPlant(); break;
                case "event_1_farming_cherry_01": DoWater(); break;
                case "event_1_farming_cherry_02": DoFertilize(); break;
                case "event_1_gathering_cherry_bough_01": DoHarvest(); break;
                case "event_newyear_2019_quest_04": DoDrawWater(); break;
                case "urban_weapon_event_04": DoEquip(); break;
                case "urban_weapon_event_10": DoRangedHunt(); break;
                case "mainstory_chapter4_6": DoRepair(); break;
                case "estate_build_lv55_02": DoStore(); break;
                case "permanent_level_skill_gathering_10": DoLearnSkill(); break;
                case "urban_cook_event_06": DoEat(); break;
                case "daily_survival_rest": DoRest(); break;
                case "daily_local_warp": DoLocalWarp(); break;
                case "mainstory_chapter1_5": DoRevive(player); break;
                case "daily_island_travel": DoIslandTravel(); break;
                default: Wait("unknown_task", 4f); break;
            }
        }

        private static void TickGoal(BotGoal current, PlayerBehavior player)
        {
            if (Time.time < _nextAction) return;
            switch (current.Kind)
            {
                case BotGoalKind.Craft: TickCraftGoal(current); break;
                case BotGoalKind.Gather: TickGatherGoal(current); break;
                case BotGoalKind.Hunt: TickHuntGoal(current); break;
                case BotGoalKind.Level: TickLevelGoal(current, player); break;
                case BotGoalKind.Skill: TickSkillGoal(current); break;
                default: Wait("unknown_goal", 2f); break;
            }
        }

        private static void TickCraftGoal(BotGoal current)
        {
            string recipeId = current.Target;
            int need = current.Count > 0 ? current.Count : 1;
            if (CountOwnedForRecipe(recipeId) >= need)
            {
                Log("คราฟต์ครบแล้ว: " + recipeId);
                if (TryEquipCrafted(recipeId))
                    _lastReason = "ใส่ " + recipeId + " แล้ว ไปเควสรายวัน";
                MemoryBotGoals.CompleteCurrent();
                _nextAction = Time.time + 0.8f;
                return;
            }
            string detail;
            string error = MemoryBotCommands.CraftRecipe(recipeId, false, out detail);
            if (error == null)
            {
                _phase = "crafting";
                _lastReason = "crafting_" + (detail ?? recipeId);
                _nextAction = Time.time + 3.5f;
                Log(_lastReason);
                return;
            }
            if (error == "recipe_locked")
            {
                if (TryLearnNamedSkill(UnlockSkillForRecipe(recipeId)))
                {
                    _phase = "learning_skill";
                    _lastReason = "เรียนสกิลเพื่อปลด " + recipeId;
                    return;
                }
                _phase = "grind_unlock";
                _lastReason = "สูตรล็อก บดคราฟต์จนกว่าจะปลด " + recipeId;
                GrindWeaponcrafting();
                return;
            }
            if (error == "missing_material")
            {
                string filter = GatherFilterForSlot(detail);
                if (MemoryBotGoals.Need(BotGoalKind.Gather, filter, 1, "เก็บ " + filter, "sub"))
                    Log("เป้าสำรอง: เก็บ " + filter + " สำหรับ " + recipeId);
                _phase = "need_material";
                _lastReason = "ขาดวัสดุ " + (detail ?? filter);
                _nextAction = Time.time + 0.3f;
                return;
            }
            Wait(error, 2.5f);
        }

        private static void TickGatherGoal(BotGoal current)
        {
            int need = current.Count > 0 ? current.Count : 1;
            int have = MemoryBotAutopilot.CountItemsMatching(current.Target);
            if (have >= need)
            {
                Log("เก็บครบ: " + current.Target + " x" + have);
                MemoryBotGoals.CompleteCurrent();
                _nextAction = Time.time + 0.4f;
                return;
            }
            if (MemoryBotUi.IsInventoryFull())
            {
                Wait("inventory_full", 3f);
                Log("กระเป๋าเต็ม หยุดเก็บ");
                return;
            }
            _phase = "gather";
            _lastReason = "gathering_" + current.Target + "_" + have + "/" + need;
            MemoryBotAutopilot.TickGather(current.Target);
        }

        /// <summary>
        /// [แก้เอง 1 ก.ย. 2026] เป้าหมาย "ล่า/แล่เนื้อ"
        ///
        /// 🐛 เดิม Hunt เป็นชนิดเดียวที่ไม่มี handler ของตัวเอง — TickGoal เรียก TickHunt() เปล่า ๆ
        ///    จึงไม่มี 3 อย่างที่เป้าอื่นมี: (1) ไม่เช็คเงื่อนไขก่อนเริ่ม (2) ไม่นับความคืบหน้า
        ///    (3) ไม่เคยเรียก CompleteCurrent() ⇒ เป้าล่าค้างบนกองตลอดไป
        ///    อาการที่เจ้าของแจ้ง: "สั่งให้แล่เนื้อ แต่ไม่มีมีด ก็ตีไปเรื่อยไม่แล่เนื้อ"
        ///
        /// ตอนนี้ทำตามรูปแบบเดียวกับ TickCraftGoal: ขาดของที่จำเป็น → Need() เป้าสำรองขึ้นกอง
        /// (ไม่มีมีด → คราฟต์มีด → ซึ่งถ้าวัสดุไม่ครบก็จะ Need() เก็บหินต่อเป็นชั้นที่สาม)
        /// ได้ครบตามจำนวนแล้วจึง CompleteCurrent() เด้งกลับเป้าหลักเอง
        /// </summary>
        private static void TickHuntGoal(BotGoal current)
        {
            int need = current.Count > 0 ? current.Count : 1;
            int have = MemoryBotAutopilot.CountItemsMatching("meat");
            if (have >= need)
            {
                Log("ได้เนื้อครบแล้ว " + have + "/" + need);
                MemoryBotGoals.CompleteCurrent();
                _nextAction = Time.time + 0.6f;
                return;
            }
            if (!MemoryBotAutopilot.HasButcherTool())
            {
                if (MemoryBotGoals.Need(BotGoalKind.Craft, "blade_stone", 1, "ทำมีดแล่เนื้อ", "sub"))
                    Log("เป้าสำรอง: ทำมีดแล่เนื้อ (ล่าไว้ก่อนไม่ได้ แล่ไม่ได้อยู่ดี)");
                _phase = "need_tool";
                _lastReason = "ยังไม่มีมีดแล่เนื้อ";
                _nextAction = Time.time + 0.3f;
                return;
            }
            _phase = "hunt";
            _lastReason = "ล่า/แล่เนื้อ " + have + "/" + need;
            MemoryBotAutopilot.TickHunt();
        }

        private static void TickLevelGoal(BotGoal current, PlayerBehavior player)
        {
            int need = current.Count > 0 ? current.Count : 1;
            int level = player.Level;
            if (level >= need)
            {
                MemoryBotGoals.CompleteCurrent();
                _nextAction = Time.time + 0.4f;
                return;
            }
            _phase = "level";
            _lastReason = "เลเวล " + level + "/" + need + " บดคราฟต์ต่อ";
            GrindWeaponcrafting();
        }

        private static void TickSkillGoal(BotGoal current)
        {
            string skillId = current.Target;
            if (string.IsNullOrEmpty(skillId)) skillId = "bow_assembled";
            if (SkillLevel(skillId) >= 1)
            {
                MemoryBotGoals.CompleteCurrent();
                _nextAction = Time.time + 0.4f;
                return;
            }
            if (TryLearnNamedSkill(skillId))
            {
                _phase = "learning_skill";
                _lastReason = "learning_" + skillId;
                return;
            }
            _phase = "grind_skill";
            _lastReason = "สกิล " + skillId + " ยังไม่ปลด บดคราฟต์ต่อ";
            GrindWeaponcrafting();
        }

        private static void GrindWeaponcrafting()
        {
            const string grind = "blade_stone";
            string detail;
            string error = MemoryBotCommands.CraftRecipe(grind, false, out detail);
            if (error == null)
            {
                _phase = "grind_craft";
                _lastReason = "คราฟต์ " + grind + " เพื่อบดความชำนาญอาวุธ";
                _nextAction = Time.time + 3.5f;
                Log(_lastReason);
                return;
            }
            if (error == "missing_material")
            {
                if (MemoryBotGoals.Need(BotGoalKind.Gather, "stone", 1, "เก็บหินทำมีด", "sub"))
                    Log("เป้าสำรอง: เก็บหิน");
                _nextAction = Time.time + 0.3f;
                return;
            }
            Wait("grind_" + error, 2f);
        }

        private static bool TryLearnNamedSkill(string skillId)
        {
            if (string.IsNullOrEmpty(skillId) || _skillLearning) return false;
            SkillSystem skills = GameSystem<SkillSystem>.HasInstance() ? GameSystem<SkillSystem>.Instance() : null;
            if (skills == null || skills.Skills == null || skills.RemainSkillPoint <= 0) return false;
            foreach (Durango.Logic.Skill.Bundle bundle in skills.Skills)
            {
                if (bundle == null) continue;
                if (TryLearnIfId(skills, bundle.Base, skillId)) return true;
                if (bundle.Sub == null) continue;
                foreach (Durango.Logic.Skill.Skill sub in bundle.Sub)
                    if (TryLearnIfId(skills, sub, skillId)) return true;
            }
            return false;
        }

        private static bool TryLearnIfId(SkillSystem skills, Durango.Logic.Skill.Skill skill, string skillId)
        {
            if (skill == null || !string.Equals(skill.Id, skillId, StringComparison.OrdinalIgnoreCase)) return false;
            return TryLearn(skills, skill);
        }

        private static int SkillLevel(string skillId)
        {
            SkillSystem skills = GameSystem<SkillSystem>.HasInstance() ? GameSystem<SkillSystem>.Instance() : null;
            if (skills == null || skills.Skills == null) return 0;
            foreach (Durango.Logic.Skill.Bundle bundle in skills.Skills)
            {
                if (bundle == null) continue;
                if (bundle.Base != null && string.Equals(bundle.Base.Id, skillId, StringComparison.OrdinalIgnoreCase))
                    return bundle.Base.Level;
                if (bundle.Sub == null) continue;
                foreach (Durango.Logic.Skill.Skill sub in bundle.Sub)
                    if (sub != null && string.Equals(sub.Id, skillId, StringComparison.OrdinalIgnoreCase))
                        return sub.Level;
            }
            return 0;
        }

        private static string UnlockSkillForRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return "";
            if (recipeId.IndexOf("bow", StringComparison.OrdinalIgnoreCase) >= 0) return "bow_assembled";
            return "";
        }

        private static string GatherFilterForSlot(string slotName)
        {
            string s = slotName == null ? "" : slotName.ToLowerInvariant();
            if (s.IndexOf("string", StringComparison.Ordinal) >= 0
                || s.IndexOf("connector", StringComparison.Ordinal) >= 0
                || s.IndexOf("เชือก", StringComparison.Ordinal) >= 0)
                return "string";
            if (s.IndexOf("stick", StringComparison.Ordinal) >= 0
                || s.IndexOf("main", StringComparison.Ordinal) >= 0
                || s.IndexOf("wood", StringComparison.Ordinal) >= 0
                || s.IndexOf("ไม้", StringComparison.Ordinal) >= 0)
                return "wood";
            if (s.IndexOf("stone", StringComparison.Ordinal) >= 0
                || s.IndexOf("หิน", StringComparison.Ordinal) >= 0
                || s.IndexOf("base", StringComparison.Ordinal) >= 0)
                return "stone";
            return string.IsNullOrEmpty(slotName) ? "stone" : slotName;
        }

        private static int CountOwnedForRecipe(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId)) return 0;
            if (recipeId.IndexOf("bow", StringComparison.OrdinalIgnoreCase) >= 0)
                return MemoryBotAutopilot.CountItemsMatching("bow_wooden");
            return MemoryBotAutopilot.CountItemsMatching(recipeId);
        }

        private static void RequestQuests(QuestSystem quests)
        {
            if (quests == null || Time.time - _lastQuestRequest < 2f) return;
            _lastQuestRequest = Time.time;
            quests.GetQuests("daily");
        }

        private static QuestToDo FindRewardReady(List<QuestToDo> todos)
        {
            for (int i = 0; i < todos.Count; i++)
            {
                if (todos[i].Progress >= todos[i].GoalCount && !todos[i].Finished) return todos[i];
            }
            return default(QuestToDo);
        }

        private static bool FindNextTask(List<QuestToDo> todos, out QuestToDo task)
        {
            for (int i = 0; i < TaskOrder.Length; i++)
            {
                for (int j = 0; j < todos.Count; j++)
                {
                    if (todos[j].Id == TaskOrder[i] && !todos[j].Finished && todos[j].Progress < todos[j].GoalCount)
                    {
                        task = todos[j];
                        return true;
                    }
                }
            }
            task = default(QuestToDo);
            return false;
        }

        private static bool SendNextFixture(PlayerBehavior player)
        {
            string command = null;
            switch (_fixtureStep)
            {
                case 0: command = "place real fire"; break;
                case 1:
                case 2:
                case 3:
                case 4: command = "farm"; break;
                case 5: command = "give s02_container_petbottle"; break;
                case 6: command = "give bow_wooden_01"; break;
                case 7: command = "give gunpowder_arrow 20"; break;
                case 8: command = "give tool_repair_kit_01 2"; break;
                case 9: command = "give meat 3"; break;
                case 10: command = "add axe"; break;
                case 11: command = "add clothes"; break;
                case 12: command = "add box"; break;
                case 13: command = "exp 1000"; break;
            }
            if (command == null) return false;
            Connections.Frontend.Send(new Cheat { _Cheat = command });
            _fixtureStep++;
            _phase = "provisioning";
            _lastReason = "fixture_" + command;
            _nextAction = Time.time + (_fixtureStep >= 1 && _fixtureStep <= 4 ? 1.0f : 0.8f);
            return true;
        }

        private static void DoPlant()
        {
            Artifact farm = FindFarm(true);
            ItemData seed = FindItem("corn_seed");
            if (farm == null || seed == null)
            {
                if (_testProvisioning && farm == null && _fixtureStep >= 13)
                {
                    Connections.Frontend.Send(new Cheat { _Cheat = "farm" });
                    _lastReason = "provisioning_extra_farm";
                    _nextAction = Time.time + 1f;
                }
                else Wait(farm == null ? "empty_farm_not_visible" : "seed_not_found", 2f);
                return;
            }
            Connections.Frontend.Send(new PlantSeed { EntityId = farm.EntityId, Tile = farm.WorldTile, SeedItemId = seed.Id });
            _phase = "planting";
            _lastReason = "planting_" + farm.EntityId;
            _nextAction = Time.time + 2.2f;
        }

        private static void DoWater()
        {
            Artifact farm = FindFarm(false);
            ItemData water = FindItem("water");
            if (farm == null || water == null)
            {
                Wait(farm == null ? "planted_farm_not_visible" : "water_not_found", 2f);
                return;
            }
            Connections.Frontend.Send(new WaterPlant { EntityId = farm.EntityId, Tile = farm.WorldTile, ItemIds = new[] { water.Id } });
            _phase = "watering";
            _lastReason = "watering_" + farm.EntityId;
            _nextAction = Time.time + 2.3f;
        }

        private static void DoFertilize()
        {
            Artifact farm = FindFarm(false);
            ItemData fertilizer = FindItem("fertilizer_01");
            if (farm == null || fertilizer == null)
            {
                Wait(farm == null ? "planted_farm_not_visible" : "fertilizer_not_found", 2f);
                return;
            }
            Connections.Frontend.Send(new FertilizePlant { EntityId = farm.EntityId, Tile = farm.WorldTile, ItemIds = new[] { fertilizer.Id } });
            _phase = "fertilizing";
            _lastReason = "fertilizing_" + farm.EntityId;
            _nextAction = Time.time + 2.3f;
        }

        private static void DoHarvest()
        {
            if (!_growthRushed && _testProvisioning)
            {
                Connections.Frontend.Send(new Cheat { _Cheat = "grow" });
                _growthRushed = true;
                _phase = "growing";
                _lastReason = "rushing_test_farms";
                _nextAction = Time.time + 2.5f;
                return;
            }
            Artifact farm = FindReadyFarm();
            if (farm == null)
            {
                Wait(_testProvisioning ? "grown_farm_not_visible" : "waiting_for_crop", 3f);
                return;
            }
            InteractionSystem interaction = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
            if (interaction == null) { Wait("interaction_unavailable", 2f); return; }
            InteractionObject target = new InteractionObject(farm.gameObject);
            if (target.Distance > 135f) { MoveNear(farm); return; }
            if (_interactionTask != _taskId || _interactionArtifactId != farm.EntityId)
            {
                interaction.SetInteractionTarget(target);
                interaction.SendTouchMsg();
                _interactionTask = _taskId;
                _interactionArtifactId = farm.EntityId;
                _phase = "inspect_harvest";
                _lastReason = "inspecting_ready_farm";
                _nextAction = Time.time + 0.6f;
                return;
            }
            foreach (InteractionMenuData menu in interaction.MenuList)
            {
                if (menu.Disabled || menu.AccessDenied) continue;
                string action = menu.Action.ToString();
                if (!IsHarvestAction(action)) continue;
                interaction.SelectTargetInteractionMenu(menu);
                _phase = "harvesting";
                _lastReason = "collecting_" + farm.EntityId;
                _nextAction = Time.time + Mathf.Max(2f, menu.Duration + 1.2f);
                return;
            }
            Wait("harvest_menu_not_ready", 1.5f);
        }

        private static bool IsHarvestAction(string action)
        {
            return !string.IsNullOrEmpty(action)
                && (action.IndexOf("collect", StringComparison.OrdinalIgnoreCase) >= 0
                    || action.IndexOf("harvest", StringComparison.OrdinalIgnoreCase) >= 0
                    || action == "506");
        }

        private static void DoDrawWater()
        {
            ItemData container = FindContainer();
            if (container == null) { Wait("water_container_not_found", 2f); return; }
            Point2 water;
            Point2 stand;
            if (!FindWater(out water, out stand))
            {
                if (_testProvisioning)
                {
                    Connections.Frontend.Send(new Cheat { _Cheat = "poi tp near_dock" });
                    _lastReason = "moving_to_test_dock";
                    _nextAction = Time.time + 2f;
                }
                else Wait("water_tile_not_loaded", 3f);
                return;
            }
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            Vector3 destination = Durango.Terrain.Util.TilePositionToClientPosition(stand, true);
            if (Vector3.Distance(player.CurrentPosition, destination) > 150f)
            {
                MoveTo(destination, "moving_to_water");
                return;
            }
            Connections.Frontend.Send(new DrawWater { ToolItemId = container.Id });
            _phase = "drawing_water";
            _lastReason = "drawing_water_at_" + water.x + "_" + water.y;
            _nextAction = Time.time + 2.5f;
        }

        private static bool FindWater(out Point2 water, out Point2 stand)
        {
            water = default(Point2);
            stand = default(Point2);
            PlayerBehavior player = PlayerBehavior.LocalPlayer;
            if (player == null || !Singleton<TerrainBase>.HasInstance()) return false;
            Vector3 world = Durango.Terrain.Util.ClientPositionToWorldPosition(player.CurrentPosition);
            Point2 center = new Point2((int)(world.x / 200f), (int)(world.z / 200f));
            TerrainBase terrain = Singleton<TerrainBase>.Instance();
            int max = Math.Max(8, Durango.Terrain.TerrainMeta.TileCount > 0 ? Durango.Terrain.TerrainMeta.TileCount : 64);
            int radius = Math.Min(64, max);
            float best = float.MaxValue;
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    Point2 candidate = new Point2(center.x + dx, center.y + dy);
                    if (candidate.x < 0 || candidate.y < 0 || (max > 0 && (candidate.x >= max || candidate.y >= max))) continue;
                    float depth = terrain.GetTileDepth(new Vector2(candidate.x + 0.5f, candidate.y + 0.5f));
                    if (depth <= 0.05f) continue;
                    float score = dx * dx + dy * dy;
                    if (score < best)
                    {
                        best = score;
                        water = candidate;
                        stand = FindStandTile(terrain, candidate);
                    }
                }
            }
            return best < float.MaxValue;
        }

        private static Point2 FindStandTile(TerrainBase terrain, Point2 water)
        {
            Point2[] offsets = { new Point2(1, 0), new Point2(-1, 0), new Point2(0, 1), new Point2(0, -1) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Point2 tile = water + offsets[i];
                if (terrain.GetTileDepth(new Vector2(tile.x + 0.5f, tile.y + 0.5f)) <= 0.05f) return tile;
            }
            return water;
        }

        private static void DoEquip()
        {
            EquipSystem equip = GameSystem<EquipSystem>.HasInstance() ? GameSystem<EquipSystem>.Instance() : null;
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (equip == null || inventory == null) { Wait("equipment_unavailable", 2f); return; }
            ItemData item = FindUnequipped("axe_onehand_stone_01");
            if (item == null) item = FindUnequipped("clothes_builder_01");
            if (item == null) { Wait("equipment_fixture_not_found", 2f); return; }
            equip.EquipItem(item);
            _phase = "equipping";
            _lastReason = "equipping_" + item.PrototypeId;
            _nextAction = Time.time + 2f;
        }

        private static void DoRangedHunt()
        {
            EnsureBowEquipped();
            GameObject target = ChooseAnimal();
            if (target == null)
            {
                if (_testProvisioning)
                {
                    Connections.Frontend.Send(new Cheat { _Cheat = "spawn" });
                    _lastReason = "spawning_test_animal";
                    _nextAction = Time.time + 2f;
                }
                else Wait("animal_not_visible", 3f);
                return;
            }
            InteractionObject targetObject = new InteractionObject(target);
            if (targetObject.Distance > 180f) { MoveNear(target, 90f); return; }
            CombatSystem combat = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
            if (combat == null || !combat.CombatMode)
            {
                if (combat != null) combat.SelectTarget(target.GetComponent<DamageableEntity>());
                _phase = "entering_combat";
                _lastReason = "entering_ranged_combat";
                _nextAction = Time.time + 1f;
                return;
            }
            foreach (BattleAction action in combat.GetCurrentBattleActions())
            {
                if (action == null || action.Data == null || action.Data.Meta == null
                    || action.Data.Meta.BattleActionType != BattleActionType.Range
                    || action.CooldownUntil > Time.time || action.ProhibitedUntil > Time.time) continue;
                combat.UseBattleAction(action.Data.Id);
                _phase = "ranged_attack";
                _lastReason = "battle_" + action.Data.Id;
                _nextAction = Time.time + 1.5f;
                return;
            }
            Wait("ranged_action_not_ready", 0.8f);
        }

        private static void EnsureBowEquipped()
        {
            EquipSystem equip = GameSystem<EquipSystem>.HasInstance() ? GameSystem<EquipSystem>.Instance() : null;
            ItemData bow = FindItem("bow_wooden_assembled");
            if (bow == null) bow = FindItem("bow_wooden_01");
            if (bow == null) bow = FindItem("bow_wooden");
            if (equip != null && bow != null && !equip.IsEquippedItem(bow)) equip.EquipItem(bow);
        }

        private static GameObject ChooseAnimal()
        {
            if (AnimalManager.HasInstance())
            {
                GameObject best = null;
                float bestDistance = float.MaxValue;
                foreach (KeyValuePair<string, AnimalBehavior> pair in AnimalManager.Instance()._animals)
                {
                    AnimalBehavior animal = pair.Value;
                    if (animal == null || !animal.IsAlive || animal.gameObject == null) continue;
                    float distance = Vector3.Distance(PlayerBehavior.LocalPlayer.CurrentPosition, animal.gameObject.transform.position);
                    if (distance < bestDistance) { bestDistance = distance; best = animal.gameObject; }
                }
                if (best != null) return best;
            }
            List<GameObject> objects = new List<GameObject>();
            InteractionSystem.SearchCombatTargetObjects(objects);
            GameObject result = null;
            float nearest = float.MaxValue;
            foreach (GameObject go in objects)
            {
                if (go == null) continue;
                AnimalBehavior ab = go.GetComponentInParent<AnimalBehavior>();
                if (ab == null || !ab.IsAlive) continue;
                float distance = Vector3.Distance(PlayerBehavior.LocalPlayer.CurrentPosition, go.transform.position);
                if (distance < nearest) { nearest = distance; result = go; }
            }
            return result;
        }

        private static void DoRepair()
        {
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inventory == null) { Wait("inventory_unavailable", 2f); return; }
            ItemData target = null;
            ItemData kit = null;
            foreach (ItemData item in inventory.PlayerItemList)
            {
                if (item == null) continue;
                if (target == null && item.IsRepairable && item.Durability != null && item.Durability.Get() < item.Durability.RealMax() - 0.01f)
                    target = item;
                if (kit == null && item.HasTag("tool_repair_kit")) kit = item;
            }
            if (target == null || kit == null)
            {
                Wait(target == null ? "damaged_equipment_not_found" : "repair_kit_not_found", 2f);
                return;
            }
            RepairSystem.RepairItem(target.Id, new[] { kit.Id }, delegate(bool success)
            {
                _lastReason = success ? "repair_succeeded" : "repair_rejected";
                _nextAction = Time.time + (success ? 1.2f : 2.5f);
            });
            _phase = "repairing";
            _lastReason = "repairing_" + target.PrototypeId;
            _nextAction = Time.time + 2f;
        }

        private static void DoStore()
        {
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inventory == null) { Wait("inventory_unavailable", 2f); return; }
            Artifact box = FindBox();
            if (box == null)
            {
                if (!_boxPlacementRequested && _testProvisioning)
                {
                    ItemData capsule = FindItem("capsulated_fur_box_03_leaf");
                    if (capsule != null && capsule.Capsule.HasValue)
                    {
                        Point2 tile = CurrentTile() + new Point2(8, 0);
                        ArtifactCapsule value = capsule.Capsule.Value;
                        Point2 size = value.OccupySize.GetValueOrDefault(Point2.one);
                        BuildSystem.PlaceCapsulatedArtifact(capsule.Id, "", tile, null, size, Rotation.None);
                        _boxPlacementRequested = true;
                        _phase = "placing_storage_box";
                        _lastReason = "placing_test_box";
                        _nextAction = Time.time + 3f;
                        return;
                    }
                }
                Wait("storage_box_not_visible", 2f);
                return;
            }
            List<string> ids = new List<string>();
            foreach (ItemData item in inventory.PlayerItemList)
            {
                if (item == null || item.Locked || item.IsEquipments || item.Id == null) continue;
                if (item.Capsule.HasValue) continue;
                ids.Add(item.Id);
                if (ids.Count == 3) break;
            }
            if (ids.Count < 3) { Wait("not_enough_storable_items", 2f); return; }
            InventorySystem.PutInItems(box.EntityId, box.WorldTile, ids.ToArray());
            _phase = "storing";
            _lastReason = "storing_3_items";
            _nextAction = Time.time + 2f;
        }

        private static void DoLearnSkill()
        {
            if (_skillLearning)
            {
                _phase = "learning_skill";
                _lastReason = "waiting_for_skill_result";
                _nextAction = Time.time + 1f;
                return;
            }
            if (!TryLearnCombatSkill()) Wait("skill_not_learnable_yet", 3f);
            else { _phase = "learning_skill"; _lastReason = "learning_melee_skill"; _nextAction = Time.time + 2f; }
        }

        private static bool TryLearnCombatSkill()
        {
            SkillSystem skills = GameSystem<SkillSystem>.HasInstance() ? GameSystem<SkillSystem>.Instance() : null;
            if (skills == null || skills.Skills == null || skills.RemainSkillPoint <= 0) return false;
            foreach (Durango.Logic.Skill.Bundle bundle in skills.Skills)
            {
                if (bundle == null || bundle.Category != Shared.Skill.Category.MeleeCombat) continue;
                Durango.Logic.Skill.Skill skill = bundle.Base;
                if (TryLearn(skills, skill)) return true;
                if (bundle.Sub == null) continue;
                foreach (Durango.Logic.Skill.Skill sub in bundle.Sub) if (TryLearn(skills, sub)) return true;
            }
            return false;
        }

        private static bool TryLearn(SkillSystem skills, Durango.Logic.Skill.Skill skill)
        {
            if (skill == null || skill.Level >= skill.MaxLevel) return false;
            Durango.Logic.Skill.Node node = skill.Get(skill.Level + 1);
            if (node == null || node.State != Durango.Logic.Skill.State.Learnable) return false;
            _skillLearning = true;
            if (!MemoryBotUi.OpenSkillMenu(skill))
            {
                _skillLearning = false;
                return false;
            }
            _lastReason = "opened_skill_menu_" + skill.Id;
            _nextAction = Time.time + 3f;
            _skillLearning = false;
            return true;
        }

        private static void DoEat()
        {
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inventory == null) { Wait("inventory_unavailable", 2f); return; }
            foreach (ItemData item in inventory.PlayerItemList)
            {
                if (item == null || !IsEdible(item) || item.IsEquipments) continue;
                inventory.UseItem(item);
                _phase = "eating";
                _lastReason = "eating_" + item.PrototypeId;
                _nextAction = Time.time + 2.5f;
                return;
            }
            Wait("food_not_found", 2f);
        }

        private static void DoRest()
        {
            Artifact rest = FindRestArtifact();
            if (rest == null) { Wait("rest_artifact_not_visible", 3f); return; }
            if (!Near(rest)) { MoveNear(rest); return; }
            Connections.Frontend.Send(new RestOn { EntityId = rest.EntityId, Tile = rest.WorldTile });
            _phase = "resting";
            _lastReason = "resting_at_" + rest.BlueprintId;
            _nextAction = Time.time + 2.5f;
        }

        private static void DoLocalWarp()
        {
            Artifact warp = FindWarp();
            if (warp == null) { Wait("warphole_not_visible", 3f); return; }
            if (!Near(warp)) { MoveNear(warp); return; }
            Connections.Frontend.Send(new Warp { Tile = warp.WorldTile });
            _phase = "warping_local";
            _lastReason = "warping_at_" + warp.WorldTile.x + "_" + warp.WorldTile.y;
            _nextAction = Time.time + 3f;
        }

        private static void DoRevive(PlayerBehavior player)
        {
            if (player.IsAlive)
            {
                if (_testProvisioning && !_reviveRequested)
                {
                    Connections.Frontend.Send(new Cheat { _Cheat = "die" });
                    _reviveRequested = true;
                    _phase = "creating_death_fixture";
                    _lastReason = "creating_real_death_for_revive_test";
                    _nextAction = Time.time + 1.5f;
                    return;
                }
                Wait("already_alive", 2f);
                return;
            }
            Connections.Frontend.Send(new Revive { WarpholeTile = null });
            _reviveRequested = true;
            _phase = "reviving";
            _lastReason = "sending_revive";
            _nextAction = Time.time + 2.5f;
        }

        private static void DoIslandTravel()
        {
            Artifact dock = FindDock();
            if (dock == null) { Wait("dock_not_visible", 3f); return; }
            if (!Near(dock)) { MoveNear(dock); return; }
            if (_travelRequested) { Wait("waiting_for_island_handoff", 4f); return; }
            _travelRequested = true;
            Point2 tile = dock.WorldTile;
            Connections.Frontend.Send(new GetIslandTravelOptions { EntityId = dock.EntityId, Tile = tile })
                .On(delegate(IslandTravelOptions options, PacketHeader _)
                {
                    int level = GameSystem<StatisticsSystem>.HasInstance() ? GameSystem<StatisticsSystem>.Instance().Level : 1;
                    string current = GameManager.Region == null ? "" : GameManager.Region.Id;
                    int count = Math.Min(options.Ids == null ? 0 : options.Ids.Length, options.RequiredLevels == null ? 0 : options.RequiredLevels.Length);
                    for (int i = 0; i < count; i++)
                    {
                        if (string.Equals(options.Ids[i], current, StringComparison.OrdinalIgnoreCase) || level < options.RequiredLevels[i]) continue;
                        Connections.Frontend.Send(new TravelByRegion
                        {
                            EntityId = dock.EntityId,
                            Tile = tile,
                            RegionId = options.Ids[i],
                            PartierId = null
                        });
                        _phase = "island_travel";
                        _lastReason = "traveling_to_" + options.Ids[i];
                        return;
                    }
                    _travelRequested = false;
                    _lastReason = "no_reachable_destination";
                    _nextAction = Time.time + 4f;
                });
            _phase = "requesting_island_options";
            _lastReason = "requesting_island_options";
            _nextAction = Time.time + 3f;
        }

        private static Artifact FindFarm(bool empty)
        {
            if (!ArtifactManager.HasInstance()) return null;
            foreach (Artifact artifact in ArtifactManager.Instance().GetArtifacts())
            {
                if (!IsUsableArtifact(artifact) || artifact.BlueprintId != "farm_tile_01") continue;
                bool planted = artifact.ArtifactState.Farming.HasValue;
                if (planted == empty) continue;
                return artifact;
            }
            return null;
        }

        private static Artifact FindReadyFarm()
        {
            if (!ArtifactManager.HasInstance()) return null;
            double now = Connections.Frontend.GetPredictedServerTime();
            foreach (Artifact artifact in ArtifactManager.Instance().GetArtifacts())
            {
                if (!IsUsableArtifact(artifact) || artifact.BlueprintId != "farm_tile_01" || !artifact.ArtifactState.Farming.HasValue) continue;
                if (artifact.ArtifactState.Farming.Value.GrowsUntil <= now + 1.0) return artifact;
            }
            return null;
        }

        private static Artifact FindRestArtifact()
        {
            return FindArtifact(delegate(Artifact a)
            {
                return a.BlueprintId == "camp_square_fire" || (a.Blueprint != null && a.Blueprint.HasComponent("Shelter"));
            });
        }

        private static Artifact FindWarp()
        {
            return FindArtifact(delegate(Artifact a)
            {
                return a.BlueprintId == "camp_warphole" || a.BlueprintId == "warphole_personal"
                    || (a.BlueprintId != "neutral_warphole" && a.Blueprint != null && a.Blueprint.HasComponent("Warphole"));
            });
        }

        private static Artifact FindDock()
        {
            return FindArtifact(delegate(Artifact a)
            {
                return a.BlueprintId == "dock" || (a.Blueprint != null && a.Blueprint.HasComponent("Port"));
            });
        }

        private static Artifact FindBox()
        {
            return FindArtifact(delegate(Artifact a)
            {
                return (a.Blueprint != null && a.Blueprint.HasComponent("Inventory"))
                    || (!string.IsNullOrEmpty(a.BlueprintId)
                        && a.BlueprintId.IndexOf("box", StringComparison.OrdinalIgnoreCase) >= 0);
            });
        }

        private static Artifact FindArtifact(Predicate<Artifact> predicate)
        {
            if (!ArtifactManager.HasInstance()) return null;
            foreach (Artifact artifact in ArtifactManager.Instance().GetArtifacts())
            {
                if (IsUsableArtifact(artifact) && predicate(artifact)) return artifact;
            }
            return null;
        }

        private static bool IsUsableArtifact(Artifact artifact)
        {
            if (artifact == null || artifact.Blueprint == null) return false;
            return artifact.BuildState == BuildingState.Built || artifact.BuildState == BuildingState.Completed;
        }

        private static bool TryEquipCrafted(string recipeId)
        {
            if (string.IsNullOrEmpty(recipeId) || recipeId.IndexOf("bow", StringComparison.OrdinalIgnoreCase) < 0)
                return false;
            EquipSystem equip = GameSystem<EquipSystem>.HasInstance() ? GameSystem<EquipSystem>.Instance() : null;
            ItemData bow = FindItem("bow_wooden_assembled");
            if (bow == null) bow = FindItem("bow_wooden_01");
            if (bow == null)
            {
                InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
                if (inventory != null)
                {
                    foreach (ItemData item in inventory.PlayerItemList)
                    {
                        if (item == null || item.IsDestroyed() || item.PrototypeId == null) continue;
                        if (item.PrototypeId.IndexOf("bow_wooden", StringComparison.OrdinalIgnoreCase) >= 0)
                        { bow = item; break; }
                    }
                }
            }
            if (equip == null || bow == null) return false;
            if (equip.IsEquippedItem(bow)) return true;
            equip.EquipItem(bow);
            return true;
        }

        private static ItemData FindItem(string prototype)
        {
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inventory == null) return null;
            foreach (ItemData item in inventory.PlayerItemList)
                if (item != null && item.PrototypeId == prototype && !item.IsDestroyed()) return item;
            return null;
        }

        private static ItemData FindUnequipped(string prototype)
        {
            ItemData item = FindItem(prototype);
            if (item == null || item.IsEquipments) return null;
            return item;
        }

        private static ItemData FindContainer()
        {
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inventory == null) return null;
            foreach (ItemData item in inventory.PlayerItemList)
            {
                if (item != null && !item.IsEquipments && (item.PrototypeId == "s02_container_petbottle" || item.HasTag("container"))) return item;
            }
            return null;
        }

        private static bool IsEdible(ItemData item)
        {
            if (item.Tags == null) return false;
            foreach (TagData tag in item.Tags)
                if (tag != null && tag.Id != null && (tag.Id.IndexOf("food", StringComparison.OrdinalIgnoreCase) >= 0 || tag.Id.IndexOf("eat", StringComparison.OrdinalIgnoreCase) >= 0)) return true;
            return false;
        }

        private static Point2 CurrentTile()
        {
            Vector3 world = Durango.Terrain.Util.ClientPositionToWorldPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
            return new Point2((int)(world.x / 200f), (int)(world.z / 200f));
        }

        private static bool Near(Artifact artifact)
        {
            return artifact != null && Vector3.Distance(PlayerBehavior.LocalPlayer.CurrentPosition, artifact.Center) <= 140f;
        }

        private static void MoveNear(Artifact artifact)
        {
            if (artifact == null) return;
            MoveTo(artifact.Center, "moving_to_" + artifact.BlueprintId);
        }

        private static void MoveNear(GameObject target, float radius)
        {
            if (target == null) return;
            Vector3 destination = target.transform.position;
            destination.x += radius;
            MoveTo(destination, "moving_to_animal");
        }

        private static void MoveTo(Vector3 destination, string reason)
        {
            if (!Singleton<PlayerController>.HasInstance()) { Wait("player_controller_unavailable", 2f); return; }
            Singleton<PlayerController>.Instance().MoveToPosition(destination);
            _phase = "moving";
            _lastReason = reason;
            _nextAction = Time.time + 1.2f;
        }

        private static void Wait(string reason, float seconds)
        {
            _phase = "waiting";
            _lastReason = reason;
            _nextAction = Time.time + seconds;
        }

        private static void Log(string text)
        {
            if (_api != null) _api.Log("[daily] " + text);
        }
    }
}
