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
using Messages;
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
            // เส้นทางที่กำลังเดินอยู่ (pathfinding) — ไว้ให้เครื่องมือภายนอกดูว่าไปถึงไหนแล้ว
            case "path.status": return MemoryBotRouteWalker.StatusJson();
            case "path.route": return RouteJson();
            case "survival": return Survival();
            case "progress": return Progress();
            case "skills": return Skills();
            case "quests":
            case "daily.quests": return Quests(limit);
            case "inventory":
            case "inv": return Inventory(limit);
            case "status": return Status();
            case "interaction": return Interaction();
            case "combat": return Combat();
            case "world.nearby": return Nearby(limit);
            case "recipes": return Recipes(limit);
            case "bot": return MemoryBotAutopilot.StatusJson();
            case "screen": return ScreenState();
            case "estate": return Estate();
            case "title": return Title();
            case "life": return MemoryBotLife.StatusJson();
            case "build": return MemoryBotBuild.StatusJson();
            default: throw new InvalidOperationException("unknown_read_path");
        }
    }

    /// <summary>สถานะหน้าไตเติ้ล — ไว้ดูว่าทำไมบอทยังเข้าโลกไม่ได้</summary>
    private static string Title()
    {
        Durango.UI.TitleMenuUserControlBase uc = UnityEngine.Object.FindObjectOfType<Durango.UI.TitleMenuUserControlBase>();
        string state = "";
        bool accountReady = false;
        string cluster = "";
        if (uc != null)
        {
            System.Reflection.BindingFlags any = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public;
            try
            {
                System.Reflection.FieldInfo ls = typeof(Durango.UI.TitleMenuUserControlBase).GetField("LastState", any);
                if (ls != null) state = ls.GetValue(uc).ToString();
                System.Reflection.FieldInfo ar = typeof(Durango.UI.TitleMenuUserControlBase).GetField("IsAccountReady", any);
                if (ar != null) accountReady = (bool)ar.GetValue(uc);
                cluster = uc.GetSelectedClusterKey() ?? "";
            }
            catch (Exception) { }
        }
        return "{\"title_scene\":" + (GameManager.IsTitleScene ? "true" : "false")
            + ",\"control\":" + (uc != null ? "true" : "false")
            + ",\"state\":" + Q(state)
            + ",\"account_ready\":" + (accountReady ? "true" : "false")
            + ",\"cluster\":" + Q(cluster)
            + ",\"emigrated\":" + Q(GameManager.Emigrated.ToString())
            + ",\"gateway\":" + Q(GameManager.GatewayUrl ?? "")
            + ",\"player_id\":" + Q(GameManager.PlayerId ?? "") + "}";
    }

    private static string Game()
    {
        return "{\"scene\":" + Q(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            + ",\"ready\":" + (GameManager.IsReady ? "true" : "false")
            + ",\"main_scene\":" + (GameManager.IsMainScene ? "true" : "false")
            + ",\"title_scene\":" + (GameManager.IsTitleScene ? "true" : "false")
            + ",\"prologue\":" + (GameManager.IsPrologueMode ? "true" : "false") + "}";
    }

    private static string Estate()
    {
        if (!GameSystem<EstateSystem>.HasInstance()) return "{\"ready\":false}";
        EstateSystem sys = GameSystem<EstateSystem>.Instance();
        Messages.EstateLicense? personal = sys.CurrentEstate != null ? (Messages.EstateLicense?)null : null;
        Messages.PioneerGradeInfo grade = sys.PioneerGradeInfo;
        Messages.EstateLicense lic = default(Messages.EstateLicense);
        bool has = false;
        if (sys.CurrentEstate != null)
        {
            has = true;
        }
        return "{\"ready\":true"
            + ",\"has_current\":" + (has ? "true" : "false")
            + ",\"pioneer_grade\":" + grade.Grade
            + ",\"max_estate_size\":" + grade.CurrentMaximumEstateSize
            + "}";
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
            // สวมอยู่ไหม — ต้องรู้เพื่อยืนยันว่าคำสั่ง inventory.equip ทำงานจริง ไม่ใช่แค่ "accepted"
            sb.Append(",\"equipped\":").Append(item.IsEquipments ? "true" : "false");
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
        // [แก้เอง 1 ก.ย. 2026] เจ้าของสั่ง: "ตอนเปิดเมนูแว่นขยายมันจะแสดงชื่อ เราดึงตรงนี้ไม่ได้เหรอ
        // จะเก็บอะไรมันต้องรู้ก่อนว่าที่คลิกมันจะได้ไหม"
        // ⇒ แนบข้อมูลชุดเดียวกับที่ UI แว่นขยายใช้: ชื่อเป้าหมาย + ชื่อของที่จะได้ + จำนวน +
        //    เลเวลที่ต้องใช้ + เครื่องมือที่ต้องมี/ที่มีอยู่จริง + เก็บได้จริงไหม
        StringBuilder sb = new StringBuilder("{");
        InteractionObject target = system.Target;
        if (target != null && target.Target != null)
        {
            ImmovableBase immovable = target.Target.GetComponentInParent<ImmovableBase>();
            sb.Append("\"target\":{\"name\":").Append(Q(immovable != null ? immovable.GetName() : CleanName(target.Target)))
              .Append(",\"entity_id\":").Append(Q(target.EntityId))
              .Append(",\"distance\":").Append(F(target.Distance))
              .Append(",\"in_range\":").Append(target.Distance <= target.CalcInteractionDistance() ? "true" : "false")
              .Append("},");
        }
        else
        {
            sb.Append("\"target\":null,");
        }
        sb.Append("\"menus\":[");
        bool first = true;
        foreach (InteractionMenuData menu in system.MenuList)
        {
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"action\":").Append(Q(menu.Action.ToString())).Append(",\"id\":").Append(Q(menu.Id));
            sb.Append(",\"name\":").Append(Q(menu.Name));
            sb.Append(",\"duration\":").Append(F(menu.Duration))
              .Append(",\"disabled\":").Append(menu.Disabled ? "true" : "false")
              .Append(",\"access_denied\":").Append(menu.AccessDenied ? "true" : "false");
            GatheringData g = menu.GatheringData;
            if (g != null)
            {
                sb.Append(",\"gather\":{\"item\":").Append(Q(g.Name))
                  .Append(",\"amount\":").Append(g.Amount)
                  .Append(",\"level\":").Append(g.Level)
                  .Append(",\"enabled\":").Append(g.Enabled ? "true" : "false")
                  .Append(",\"available\":").Append(g.IsAvailableForGathering() ? "true" : "false")
                  .Append(",\"tool\":").Append(Q(g.BestTool != null ? (g.BestTool.Name ?? g.BestTool.PrototypeId) : "bare_hands"))
                  .Append(",\"has_tool\":").Append(g.BestTool != null ? "true" : "false")
                  .Append('}');
            }
            sb.Append('}');
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

    /// <summary>ชื่อ GameObject แบบอ่านง่าย — ตัด "(Clone)" และเลขต่อท้ายที่ Unity เติมให้</summary>
    private static string CleanName(GameObject go)
    {
        if (go == null) return string.Empty;
        // ของธรรมชาติ/สิ่งปลูกสร้างมีชื่อจริงในเกมอยู่แล้ว (แปลตามภาษา) ใช้อันนั้นก่อน
        ImmovableBase immovable = go.GetComponentInParent<ImmovableBase>();
        if (immovable != null)
        {
            string real = immovable.GetName();
            if (!string.IsNullOrEmpty(real)) return real;
        }
        string name = go.name ?? string.Empty;
        int clone = name.IndexOf("(Clone)", StringComparison.Ordinal);
        if (clone >= 0) name = name.Substring(0, clone);
        return name.Trim();
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
            AnimalBehavior animal = go.GetComponentInParent<AnimalBehavior>();
            bool isAnimal = animal != null;
            // [แก้เอง 1 ก.ย. 2026] เจ้าของสั่ง: "มันต้องคลิกดูทีละอย่าง มันไม่รู้เหรอว่าคืออะไร"
            // เดิมคืนแค่เลข type ⇒ ต้อง select ทีละตัวเพื่อดูว่าเป็นอะไร (ช้าและเปลืองรอบ)
            // ตอนนี้แนบชื่อ GameObject + blueprint ของสิ่งปลูกสร้าง + สถานะสัตว์มาให้เลย
            Artifact artifact = go.GetComponentInParent<Artifact>();
            sb.Append("{\"kind\":").Append(Q(kind)).Append(",\"class\":").Append(Q(isAnimal ? "animal" : kind))
                .Append(",\"id\":").Append(Q(obj.EntityId))
                .Append(",\"type\":").Append(obj.EntityType)
                .Append(",\"name\":").Append(Q(CleanName(go)))
                .Append(",\"distance\":").Append(F(obj.Distance))
                .Append(",\"tile\":[").Append(F(tile.x)).Append(',').Append(F(tile.y)).Append(']');
            // สัตว์เคลื่อนที่ตลอด ค่า tile ของมันมักเป็น [-1,-1] ⇒ ต้องมีพิกัดโลกจริงไว้เดินเข้าหา
            Vector3 worldPos = TerrainUtil.ClientPositionToWorldPosition(obj.Position);
            sb.Append(",\"pos\":[").Append(F(worldPos.x)).Append(',').Append(F(worldPos.z)).Append(']');
            if (artifact != null) sb.Append(",\"blueprint\":").Append(Q(artifact.BlueprintId));
            if (isAnimal)
            {
                sb.Append(",\"lootable\":").Append(animal.IsLootable ? "true" : "false")
                  .Append(",\"alive\":").Append(animal.IsAlive ? "true" : "false");
            }
            sb.Append('}');
            count++;
        }
    }

    /// <summary>
    /// รายการสูตรคราฟต์ที่ตัวละครปลดแล้ว + บอกด้วยว่าวัสดุครบไหม / ต้องมีโต๊ะงานไหม
    /// ใช้ข้อมูลชุดเดียวกับหน้าคราฟต์ในเกม (RecipeContainer + RecipeSystem.HasMaterials)
    /// เพื่อให้บอทเลือกได้เหมือนผู้เล่นอ่านหน้าจอ ไม่ใช่เดาชื่อไอเทม
    /// </summary>
    private static string Recipes(int limit)
    {
        RecipeSystem system = GameSystem<RecipeSystem>.HasInstance() ? GameSystem<RecipeSystem>.Instance() : null;
        if (system == null || system.RecipeContainer == null) return "{\"ready\":false,\"recipes\":[]}";
        StringBuilder sb = new StringBuilder("{\"ready\":true,\"recipes\":[");
        List<Crafting.Recipe> all = new List<Crafting.Recipe>();
        system.RecipeContainer.EnumerateRecipes(delegate(Crafting.Recipe r) { if (r != null) all.Add(r); });
        int count = 0;
        bool first = true;
        for (int i = 0; i < all.Count && count < limit; i++)
        {
            Crafting.Recipe r = all[i];
            if (!r.Available) continue;          // ยังไม่ปลดสูตร = ผู้เล่นก็ไม่เห็นในหน้าคราฟต์
            bool hasTool;
            bool hasMats;
            try { hasMats = RecipeSystem.HasMaterials(r, out hasTool, null, 1); }
            catch { hasMats = false; hasTool = false; }
            if (!first) sb.Append(',');
            first = false;
            sb.Append("{\"id\":").Append(Q(r.Id)).Append(",\"name\":").Append(Q(r.Name))
              .Append(",\"category\":").Append(Q(r.Category))
              .Append(",\"min_level\":").Append(r.MinLevel)
              .Append(",\"has_materials\":").Append(hasMats ? "true" : "false")
              .Append(",\"has_tool\":").Append(hasTool ? "true" : "false")
              .Append(",\"needs_workbench\":").Append(r.HasRequiredWorkbench ? "true" : "false")
              .Append('}');
            count++;
        }
        return sb.Append("],\"total\":").Append(all.Count).Append('}').ToString();
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

    private static string Quests(int limit)
    {
        QuestSystem system = GameSystem<QuestSystem>.HasInstance() ? GameSystem<QuestSystem>.Instance() : null;
        if (system == null) return "{\"ready\":false,\"category\":\"daily\",\"quests\":[]}";
        // [แก้เอง 31 ส.ค. 2026] เดิมอ่านแค่หมวด "daily" ⇒ เทสเควสสายหลักไม่ได้เลย
        // (เซิร์ฟมีสายหลัก 9 ขั้นและนับความคืบหน้าอยู่จริง แต่บอทรายงานว่าง เข้าใจผิดว่าเควสพัง)
        // ⇒ ไล่ทุกหมวดที่มองเห็นได้ (VisibleCategories) แล้วบอกด้วยว่าเควสไหนอยู่หมวดอะไร
        List<QuestToDo> quests = new List<QuestToDo>();
        List<string> catNames = new List<string>();
        try
        {
            foreach (Durango.Logic.Quest.Category c in system.VisibleCategories)
            {
                if (c == null) { continue; }
                catNames.Add(c.Key);
                List<QuestToDo> got = c.GetCachedQuestList();
                if (got != null) { quests.AddRange(got); }
            }
        }
        catch { }
        StringBuilder sb = new StringBuilder("{\"ready\":true,\"categories\":"
            + Q(string.Join(",", catNames.ToArray())) + ",\"quests\":[");
        bool first = true;
        int count = 0;
        foreach (QuestToDo quest in quests)
        {
            if (count++ >= limit) break;
            if (!first) sb.Append(',');
            first = false;
            bool rewardReady = quest.Progress >= quest.GoalCount && !quest.Finished;
            sb.Append("{\"id\":").Append(Q(quest.Id))
                .Append(",\"progress\":").Append(quest.Progress)
                .Append(",\"goal\":").Append(quest.GoalCount)
                .Append(",\"finished\":").Append(quest.Finished ? "true" : "false")
                .Append(",\"reward_ready\":").Append(rewardReady ? "true" : "false")
                .Append(",\"reward\":").Append(quest.Reward.HasValue ? "true" : "false")
                .Append('}');
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

    /// <summary>เส้นทางเต็มเป็น tile — เอาไปวาดบนแผนที่ภายนอกได้</summary>
    private static string RouteJson()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("{\"index\":").Append(MemoryBotRouteWalker.CurrentIndex).Append(",\"tiles\":[");
        var route = MemoryBotRouteWalker.CurrentRoute;
        for (int i = 0; i < route.Count; i++)
        {
            if (i > 0) { sb.Append(','); }
            sb.Append('[').Append(route[i].x).Append(',').Append(route[i].y).Append(']');
        }
        sb.Append("]}");
        return sb.ToString();
    }
}
}
