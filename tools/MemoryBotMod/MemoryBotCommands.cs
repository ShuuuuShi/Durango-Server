using System;
using System.Collections.Generic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Utils;
using InteractionData;
using UnityEngine;
using Yaml;
using TerrainUtil = Durango.Terrain.Util;

namespace DurangoMemoryBot
{

internal static class MemoryBotCommands
{
    public static string Execute(MemoryBotRequest request)
    {
        if (request.Name == "bot.start" || request.Name == "bot.stop" || request.Name == "bot.status")
        {
            return MemoryBotAutopilot.Execute(request);
        }
        if (request.Name == "map.walk_to" || request.Name == "map.click")
        {
            if (!request.HasX || !request.HasY) throw new InvalidOperationException("map_walk_to_needs_x_y");
            if (PlayerBehavior.LocalPlayer == null || !Singleton<PlayerController>.HasInstance())
                return "{\"status\":\"rejected\",\"reason\":\"player_unavailable\"}";
            MemoryBotAutopilot.Execute(new MemoryBotRequest { Name = "bot.stop" });
            float worldX = request.X;
            float worldZ = request.Y;
            bool tileCoordinates = string.Equals(request.Kind, "tile", StringComparison.OrdinalIgnoreCase);
            if (tileCoordinates)
            {
                worldX = request.X * 200f + 100f;
                worldZ = request.Y * 200f + 100f;
            }
            int tileCount = Durango.Terrain.TerrainMeta.TileCount;
            float maxWorld = tileCount > 0 ? tileCount * 200f : float.MaxValue;
            if (worldX < 0f || worldZ < 0f || worldX >= maxWorld || worldZ >= maxWorld)
                return "{\"status\":\"rejected\",\"reason\":\"position_out_of_bounds\"}";
            Vector3 world = new Vector3(worldX, 0f, worldZ);
            Vector3 client = TerrainUtil.WorldPositionToClientPosition(world);
            Singleton<PlayerController>.Instance().MoveToPosition(client);
            return "{\"status\":\"accepted\",\"command\":" + MemoryBotProtocol.Quote(request.Name)
                + ",\"coordinate_space\":" + MemoryBotProtocol.Quote(tileCoordinates ? "tile" : "world")
                + ",\"x\":" + worldX.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                + ",\"z\":" + worldZ.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "}";
        }        if (request.Name == "player.stop")
        {
            if (PlayerBehavior.LocalPlayer == null) return "{\"status\":\"rejected\",\"reason\":\"no_local_player\"}";
            Singleton<PlayerController>.Instance().StopMove();
            return "{\"status\":\"accepted\",\"command\":\"player.stop\"}";
        }
        if (request.Name == "player.move_to")
        {
            if (!request.HasX || !request.HasY) throw new InvalidOperationException("move_to_needs_x_y");
            if (PlayerBehavior.LocalPlayer == null) return "{\"status\":\"rejected\",\"reason\":\"no_local_player\"}";
            // กันเดินออกนอกโลก: เทียบกับขอบเขต tile จริงของ terrain (TileCount x 200 หน่วย)
            int tileCount = Durango.Terrain.TerrainMeta.TileCount;
            float maxWorld = tileCount > 0 ? tileCount * 200f : float.MaxValue;
            if (request.X < 0f || request.Y < 0f || request.X >= maxWorld || request.Y >= maxWorld)
            {
                return "{\"status\":\"rejected\",\"reason\":\"position_out_of_bounds\"}";
            }
            Vector3 world = new Vector3(request.X, 0f, request.Y);
            Vector3 client = TerrainUtil.WorldPositionToClientPosition(world);
            Singleton<PlayerController>.Instance().MoveToPosition(client);
            return "{\"status\":\"accepted\",\"command\":\"player.move_to\"}";
        }
        if (request.Name == "inventory.use")
        {
            if (string.IsNullOrEmpty(request.ItemId)) throw new InvalidOperationException("use_needs_item_id");
            InventorySystem inventory = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inventory == null) return "{\"status\":\"rejected\",\"reason\":\"inventory_unavailable\"}";
            ItemData found = null;
            foreach (ItemData item in inventory.PlayerItemList)
            {
                if (item != null && (item.Id == request.ItemId || item.PrototypeId == request.ItemId)) { found = item; break; }
            }
            if (found == null) return "{\"status\":\"rejected\",\"reason\":\"item_not_found\"}";
            inventory.UseItem(found);
            return "{\"status\":\"accepted\",\"command\":\"inventory.use\"}";
        }
        if (request.Name == "combat.use_action")
        {
            if (string.IsNullOrEmpty(request.ActionId)) throw new InvalidOperationException("use_action_needs_action_id");
            CombatSystem combat = GameSystem<CombatSystem>.HasInstance() ? GameSystem<CombatSystem>.Instance() : null;
            if (combat == null) return "{\"status\":\"rejected\",\"reason\":\"combat_unavailable\"}";
            // ตรวจว่า action มีอยู่จริงในรายการที่ client เปิดอยู่ และยังใช้ได้ — ไม่งั้นจะตอบ accepted ทั้งที่ไม่มีผล
            foreach (BattleAction action in combat.GetCurrentBattleActions())
            {
                if (action == null || action.Data == null) continue;
                if (!string.Equals(action.Data.Id, request.ActionId, StringComparison.Ordinal)) continue;
                if (action.CooldownUntil > Time.time) return "{\"status\":\"rejected\",\"reason\":\"action_on_cooldown\"}";
                if (action.ProhibitedUntil > Time.time) return "{\"status\":\"rejected\",\"reason\":\"action_prohibited\"}";
                combat.UseBattleAction(request.ActionId);
                return Accepted(request.Name);
            }
            return "{\"status\":\"rejected\",\"reason\":\"action_not_available\"}";
        }
        if (request.Name == "interaction.select_nearest")
        {
            InteractionSystem system = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
            if (system == null || PlayerBehavior.LocalPlayer == null) return "{\"status\":\"rejected\",\"reason\":\"interaction_unavailable\"}";
            List<GameObject> objects = new List<GameObject>();
            string kind = (request.Kind ?? "prop").ToLowerInvariant();
            if (kind == "animal" || kind == "combat") InteractionSystem.SearchMovableObjects(objects);
            else InteractionSystem.SearchPropObjects(objects);
            GameObject nearest = null;
            float distance = float.MaxValue;
            foreach (GameObject obj in objects)
            {
                if (obj == null) continue;
                InteractionObject candidate = new InteractionObject(obj);
                if (!string.IsNullOrEmpty(request.EntityId) && !string.Equals(candidate.EntityId, request.EntityId, StringComparison.Ordinal)) continue;
                if (candidate.Distance < distance) { distance = candidate.Distance; nearest = obj; }
            }
            if (nearest == null) return "{\"status\":\"rejected\",\"reason\":\"interaction_target_not_found\"}";
            InteractionObject selected = new InteractionObject(nearest);
            system.SetInteractionTarget(selected);
            system.SendTouchMsg();
            return "{\"status\":\"accepted\",\"command\":" + MemoryBotProtocol.Quote(request.Name) + ",\"entity_id\":" + MemoryBotProtocol.Quote(selected.EntityId) + "}";
        }
        if (request.Name == "interaction.refresh")
        {
            InteractionSystem system = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
            if (system == null || system.Target == null) return "{\"status\":\"rejected\",\"reason\":\"interaction_target_missing\"}";
            system.SendTouchMsg();
            return Accepted(request.Name);
        }
        if (request.Name == "interaction.execute")
        {
            if (string.IsNullOrEmpty(request.ActionId)) throw new InvalidOperationException("interaction_execute_needs_action_id");
            InteractionSystem system = GameSystem<InteractionSystem>.HasInstance() ? GameSystem<InteractionSystem>.Instance() : null;
            if (system == null || system.Target == null) return "{\"status\":\"rejected\",\"reason\":\"interaction_target_missing\"}";
            foreach (InteractionMenuData menu in system.MenuList)
            {
                if (menu.Disabled || menu.AccessDenied) continue;
                if (!string.Equals(menu.Action.ToString(), request.ActionId, StringComparison.OrdinalIgnoreCase)) continue;
                system.SelectTargetInteractionMenu(menu);
                return Accepted(request.Name);
            }
            return "{\"status\":\"rejected\",\"reason\":\"interaction_action_not_available\"}";
        }
        if (request.Name == "ui.open")
        {
            string uri = request.Uri ?? "";
            if (uri == "Inventory") { UIManager.Open<Durango.UI.InventoryGroup>(); return Accepted(request.Name); }
            if (uri == "Skill") { UIManager.Open<Durango.UI.SkillGroup>(); return Accepted(request.Name); }
            if (uri == "Status") { UIManager.Open<global::CharacterStatusGroup>(); return Accepted(request.Name); }
            return "{\"status\":\"rejected\",\"reason\":\"ui_uri_not_allowed\"}";
        }
        throw new InvalidOperationException("unknown_command");
    }

    private static string Accepted(string name)
    {
        return "{\"status\":\"accepted\",\"command\":" + MemoryBotProtocol.Quote(name) + "}";
    }
}
}
