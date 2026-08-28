using System;
using System.Collections.Generic;
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

    public static void Initialize(IClientModApi api) { _api = api; }

    public static string Execute(MemoryBotRequest request)
    {
        string name = request.Name ?? "";
        if (name == "bot.start")
        {
            _mode = string.IsNullOrEmpty(request.Kind) ? "gather" : request.Kind.ToLowerInvariant();
            if (_mode != "gather" && _mode != "survival") return Error("unsupported_mode");
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
            Log("autopilot started mode=" + _mode);
            return StatusJson();
        }
        if (name == "bot.stop")
        {
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
        return "{\"running\":" + (_running ? "true" : "false")
            + ",\"mode\":" + MemoryBotProtocol.Quote(_mode)
            + ",\"phase\":" + MemoryBotProtocol.Quote(_phase)
            + ",\"target_id\":" + MemoryBotProtocol.Quote(_targetId ?? "")
            + ",\"moves\":" + _moves
            + ",\"inspections\":" + _inspections
            + ",\"actions\":" + _actions
            + ",\"eats\":" + _eats
            + ",\"last_reason\":" + MemoryBotProtocol.Quote(_lastReason ?? "") + "}";
    }

    public static void Tick(float deltaTime)
    {
        if (!_running || Time.time < _nextThink) return;
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
        if (_phase == "moving")
        {
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
        if (TryLearnCombatSkill()) return;

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

    private static bool TryLearnSkill(SkillSystem skills, Durango.Logic.Skill.Skill skill)
    {
        if (skill == null || skill.Level >= skill.MaxLevel) return false;
        Durango.Logic.Skill.Node node = skill.Get(skill.Level + 1);
        if (node == null || node.State != Durango.Logic.Skill.State.Learnable) return false;
        _skillLearning = true;
        _nextSkillAttempt = Time.time + 3f;
        _lastReason = "learning_" + skill.Id + "_" + (skill.SubId ?? "base") + "_lv" + (skill.Level + 1);
        skills.LearnSkill(skill, delegate(bool success)
        {
            _skillLearning = false;
            _lastReason = success ? "learned_" + skill.Id : "learn_failed_" + skill.Id;
            _nextAction = Time.time + 0.8f;
        });
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
            if (go.GetComponentInParent<AnimalBehavior>() != null || go.GetComponentInParent<WildAnimalAI>() != null) continue;
            if (_mode == "gather" && !IsStoneTarget(go)) continue;
            InteractionObject target = new InteractionObject(go);
            string id = TargetKey(go);
            float until;
            if (id.Length > 0 && Cooldowns.TryGetValue(id, out until) && until > Time.time) continue;
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
        Vector3 destination = target.transform.position;
        float angle = (float)Rng.NextDouble() * Mathf.PI * 2f;
        float radius = 25f + (float)Rng.NextDouble() * 45f;
        destination.x += Mathf.Cos(angle) * radius;
        destination.z += Mathf.Sin(angle) * radius;
        Singleton<PlayerController>.Instance().MoveToPosition(destination);
        _moves++;
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
            foreach (InteractionMenuData menu in menus)
            {
                if (menu.Disabled || menu.AccessDenied || !IsHumanGatherAction(menu.Action.ToString())) continue;
                if (_mode == "gather" && !IsBareHandGatherMenu(menu)) continue;
                interaction.SelectTargetInteractionMenu(menu);
                _actions++;
                _phase = IsAnimalTarget() && IsAttackAction(menu.Action.ToString()) ? "combat" : "working";
                _phaseUntil = Time.time + Mathf.Max(1.2f, menu.Duration + 0.8f);
                _nextAction = Time.time + 0.4f + Jitter(0.8f);
                _lastReason = "executing_" + menu.Action;
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
        _target = null;
        _targetId = null;
        _phase = "idle";
        _lastReason = "work_complete";
        _nextAction = Time.time + 0.5f + Jitter(1.5f);
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
            if (go.GetComponentInParent<AnimalBehavior>() == null && go.GetComponentInParent<WildAnimalAI>() == null) continue;
            InteractionObject target = new InteractionObject(go);
            if (target.Distance > 1000f) continue;
            float score = target.Distance + (float)Rng.NextDouble() * 120f;
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
        return _target != null && (_target.GetComponentInParent<AnimalBehavior>() != null || _target.GetComponentInParent<WildAnimalAI>() != null);
    }

    private static bool IsAttackAction(string action)
    {
        return !string.IsNullOrEmpty(action) && action.IndexOf("attack", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void FightTarget()
    {
        AnimalBehavior animal = _target == null ? null : _target.GetComponentInParent<AnimalBehavior>();
        WildAnimalAI wildAnimal = _target == null ? null : _target.GetComponentInParent<WildAnimalAI>();
        if ((animal == null && wildAnimal == null) || (animal != null && !animal.IsAlive))
        {
            _target = null;
            _targetId = null;
            _phase = "idle";
            _lastReason = "target_dead_or_gone";
            _nextAction = Time.time + 1f + Jitter(1.5f);
            return;
        }
        InteractionObject target = new InteractionObject(_target);
        if (target.Distance > 180f)
        {
            MoveNear(_target);
            _phase = "combat";
            return;
        }
        CombatSystem combat = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
        if (combat == null || !combat.CombatMode)
        {
            _lastReason = "entering_combat";
            _nextAction = Time.time + 0.5f + Jitter(0.7f);
            return;
        }
        foreach (BattleAction action in combat.GetCurrentBattleActions())
        {
            if (action == null || action.Data == null || action.Data.Meta == null) continue;
            if (action.CooldownUntil > Time.time || action.ProhibitedUntil > Time.time) continue;
            combat.UseBattleAction(action.Data.Id);
            _actions++;
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
        if (!string.IsNullOrEmpty(_targetId)) Cooldowns[_targetId] = Time.time + 8f + Jitter(8f);
        _target = null;
        _targetId = null;
        _phase = "idle";
        _lastReason = reason;
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
