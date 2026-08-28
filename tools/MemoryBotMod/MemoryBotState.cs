using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Durango.Logic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Modding;
using Durango.Utils;
using InteractionData;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{

internal static class MemoryBotState
{
    public static string Read(string path, int limit)
    {
        switch (path)
        {
            case "game": return Game();
            case "player.local": return Player();
            case "survival": return Survival();
            case "progress": return Progress();
            case "skills": return Skills();
            case "inventory":
            case "inv": return Inventory(limit);
            case "status": return Status();
            case "interaction": return Interaction();
            case "combat": return Combat();
            case "world.nearby": return Nearby(limit);
            case "bot": return MemoryBotAutopilot.StatusJson();
            case "screen": return ScreenState();
            default: throw new InvalidOperationException("unknown_read_path");
        }
    }

    private static string Game()
    {
        return "{\"scene\":" + Q(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            + ",\"ready\":" + (GameManager.IsReady ? "true" : "false")
            + ",\"main_scene\":" + (GameManager.IsMainScene ? "true" : "false")
            + ",\"title_scene\":" + (GameManager.IsTitleScene ? "true" : "false")
            + ",\"prologue\":" + (GameManager.IsPrologueMode ? "true" : "false") + "}";
    }

    private static string Player()
    {
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (player == null) return "null";
        Vector3 world = TerrainUtil.ClientPositionToWorldPosition(player.CurrentPosition);
        Vector2 tile = TerrainUtil.WorldPositionToTilePosition(world);
        return "{\"id\":" + Q(GameManager.PlayerId) + ",\"name\":" + Q(player.PlayerName)
            + ",\"position\":[" + F(world.x) + "," + F(world.z) + "]"
            + ",\"tile\":[" + F(tile.x) + "," + F(tile.y) + "]"
            + ",\"alive\":" + (player.IsAlive ? "true" : "false")
            + ",\"moving\":" + (player.IsMoving ? "true" : "false")
            + ",\"riding\":" + (player.IsRiding ? "true" : "false") + "}";
    }

    private static string Survival()
    {
        PlayerBehavior player = PlayerBehavior.LocalPlayer;
        if (player == null) return "null";
        Gauge life = player.Life;
        Gauge stamina = player.Stamina;
        Gauge fatigue = player.Fatigue;
        FatigueSystem fs = GameSystem<FatigueSystem>.HasInstance() ? GameSystem<FatigueSystem>.Instance() : null;
        float velocity = fs != null && fs.Fatigue != null ? fs.Fatigue.Velocity : 0f;
        return "{\"life\":" + GaugeJson(life) + ",\"stamina\":" + GaugeJson(stamina)
            + ",\"fatigue\":" + GaugeJson(fatigue) + ",\"fatigue_velocity\":" + F(velocity)
            + ",\"fatigue_state\":" + Q(fs == null || fs.Fatigue == null ? "unknown" : fs.Fatigue.GetState().ToString()) + "}";
    }

    private static string Inventory(int limit)
    {
        InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
        if (inv == null) return "{\"items\":[],\"count\":0}";
        List<ItemData> items = inv.PlayerItemList;
        StringBuilder sb = new StringBuilder("{\"items\":[");
        int count = 0;
        bool first = true;
        for (int i = 0; i < items.Count && count < limit; i++)
        {
            ItemData item = items[i];
            if (item == null) continue;
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"id\":").Append(Q(item.Id)).Append(",\"prototype\":").Append(Q(item.PrototypeId));
            sb.Append(",\"name\":").Append(Q(item.Name ?? item.PrototypeName));
            sb.Append(",\"size\":").Append(item.Size).Append(",\"edible\":").Append(IsEdible(item) ? "true" : "false");
            sb.Append('}');
            count++;
        }
        sb.Append("],\"count\":").Append(items.Count).Append('}');
        return sb.ToString();
    }

    private static string Status()
    {
        if (!GameSystem<StatusEffectSystem>.HasInstance()) return "{\"effects\":[]}";
        Durango.Logic.StatusEffects effects = StatusEffectSystem.Instance().GetStatusEffects();
        if (effects == null || effects.List == null) return "{\"effects\":[]}";
        StringBuilder sb = new StringBuilder("{\"effects\":[");
        bool first = true;
        foreach (Durango.Logic.StatusEffect effect in effects.List)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"id\":").Append(Q(effect.Id)).Append(",\"level\":").Append(effect.Level).Append('}');
        }
        return sb.Append("]}").ToString();
    }

    private static string Interaction()
    {
        InteractionSystem system = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
        if (system == null) return "{\"menus\":[]}";
        StringBuilder sb = new StringBuilder("{\"menus\":[");
        bool first = true;
        foreach (InteractionMenuData menu in system.MenuList)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"action\":").Append(Q(menu.Action.ToString())).Append(",\"id\":").Append(Q(menu.Id));
            sb.Append(",\"duration\":").Append(F(menu.Duration)).Append(",\"disabled\":").Append(menu.Disabled ? "true" : "false").Append('}');
        }
        return sb.Append("]}").ToString();
    }

    private static string Combat()
    {
        CombatSystem system = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
        if (system == null) return "{\"mode\":false,\"actions\":[]}";
        StringBuilder sb = new StringBuilder("{\"mode\":");
        sb.Append(system.CombatMode ? "true" : "false").Append(",\"actions\":[");
        bool first = true;
        foreach (BattleAction action in system.GetCurrentBattleActions())
        {
            if (action == null || action.Data == null) continue;
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"id\":").Append(Q(action.Data.Id)).Append(",\"cooldown\":").Append(F((float)Math.Max(0.0, action.CooldownUntil - Time.time))).Append('}');
        }
        return sb.Append("]}").ToString();
    }

    private static string Nearby(int limit)
    {
        if (PlayerBehavior.LocalPlayer == null || !GameManager.IsMainScene || !GameSystem<InteractionSystem>.HasInstance())
            return "{\"ready\":false,\"objects\":[]}";
        var props = new List<GameObject>();
        var movables = new List<GameObject>();
        InteractionSystem.SearchPropObjects(props);
        InteractionSystem.SearchMovableObjects(movables);
        StringBuilder sb = new StringBuilder("{\"ready\":true,\"objects\":[");
        bool first = true;
        int count = 0;
        AppendNearbyList(sb, props, "prop", limit, ref first, ref count);
        AppendNearbyList(sb, movables, "movable", limit, ref first, ref count);
        return sb.Append("]}").ToString();
    }

    private static void AppendNearbyList(StringBuilder sb, List<GameObject> objects, string kind, int limit, ref bool first, ref int count)
    {
        foreach (GameObject go in objects)
        {
            if (go == null || count >= limit) break;
            InteractionObject obj = new InteractionObject(go);
            if (!first) sb.Append(',');
            first = false;
            Vector2 tile = obj.Tile;
            bool isAnimal = go.GetComponentInParent<AnimalBehavior>() != null || go.GetComponentInParent<WildAnimalAI>() != null;
            sb.Append("{\"kind\":").Append(Q(kind)).Append(",\"class\":").Append(Q(isAnimal ? "animal" : kind))
                .Append(",\"id\":").Append(Q(obj.EntityId))
                .Append(",\"type\":").Append(obj.EntityType)
                .Append(",\"distance\":").Append(F(obj.Distance))
                .Append(",\"tile\":[").Append(F(tile.x)).Append(',').Append(F(tile.y)).Append("]}");
            count++;
        }
    }

    private static string Progress()
    {
        StatisticsSystem stats = GameSystem<StatisticsSystem>.HasInstance() ? GameSystem<StatisticsSystem>.Instance() : null;
        if (stats == null || !stats.Statistics.HasValue) return "{\"ready\":false}";
        return "{\"ready\":true,\"level\":" + stats.Level + ",\"exp\":" + stats.Exp + "}";
    }

    private static string Skills()
    {
        SkillSystem system = GameSystem<SkillSystem>.HasInstance() ? GameSystem<SkillSystem>.Instance() : null;
        if (system == null) return "{\"ready\":false,\"remaining\":0,\"skills\":[]}";
        StringBuilder sb = new StringBuilder("{\"ready\":true,\"total\":" + system.SkillPoint
            + ",\"remaining\":" + system.RemainSkillPoint + ",\"skills\":[");
        bool first = true;
        foreach (Durango.Logic.Skill.Bundle bundle in system.Skills)
        {
            if (bundle == null || bundle.Category != Shared.Skill.Category.MeleeCombat) continue;
            AppendSkill(sb, bundle.Base, ref first);
            if (bundle.Sub == null) continue;
            foreach (Durango.Logic.Skill.Skill skill in bundle.Sub) AppendSkill(sb, skill, ref first);
        }
        return sb.Append("]}").ToString();
    }

    private static void AppendSkill(StringBuilder sb, Durango.Logic.Skill.Skill skill, ref bool first)
    {
        if (skill == null) return;
        Durango.Logic.Skill.Node next = skill.Level < skill.MaxLevel ? skill.Get(skill.Level + 1) : null;
        if (!first) sb.Append(',');
        first = false;
        sb.Append("{\"id\":").Append(Q(skill.Id)).Append(",\"sub_id\":").Append(Q(skill.SubId))
            .Append(",\"level\":").Append(skill.Level)
            .Append(",\"next_state\":").Append(Q(next == null ? "max" : next.State.ToString()))
            .Append(",\"next_cost\":").Append(next == null ? 0 : next.SkillPoints).Append('}');
    }
    private static string ScreenState()
    {
        return "{\"width\":" + UnityEngine.Screen.width + ",\"height\":" + UnityEngine.Screen.height + "}";
    }

    private static string GaugeJson(Gauge gauge)
    {
        return gauge == null ? "null" : "{\"value\":" + F(gauge.Get()) + ",\"max\":" + F(gauge.Max()) + "}";
    }

    private static bool IsEdible(ItemData item)
    {
        if (item.Tags == null) return false;
        foreach (TagData tag in item.Tags)
        {
            if (tag != null && tag.Id != null && (tag.Id.IndexOf("food", StringComparison.OrdinalIgnoreCase) >= 0 || tag.Id.IndexOf("eat", StringComparison.OrdinalIgnoreCase) >= 0)) return true;
        }
        return false;
    }

    private static string Q(string value) { return MemoryBotProtocol.Quote(value); }
    private static string F(float value) { return value.ToString("0.###", CultureInfo.InvariantCulture); }
}
}
