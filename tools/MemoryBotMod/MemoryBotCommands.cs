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
        if (request.Name == "combat.auto" || request.Name == "bot.start" || request.Name == "bot.stop"
            || request.Name == "bot.status" || request.Name == "bot.goal")
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
            // [แก้เอง 30 ส.ค. 2026] เดินด้วย pathfinding แทนการพุ่งเส้นตรง
            // เจ้าของสั่ง: "เวลาสั่งเดินไกล ๆ ให้ค้นหาเส้นทางด้วย pathfinding เดินหลบจุดที่เดินไม่ได้"
            // ของเดิมสั่ง MoveToPosition ตรง ๆ ⇒ เจอทะเลก็เดินลงน้ำแล้วจอด
            // ใส่ kind="direct" ถ้าอยากได้พฤติกรรมเดิม (เดินสั้น ๆ ในที่โล่ง เร็วกว่า)
            Vector3 world = new Vector3(worldX, 0f, worldZ);
            Vector3 client = TerrainUtil.WorldPositionToClientPosition(world);
            if (!string.Equals(request.Kind, "direct", StringComparison.OrdinalIgnoreCase))
            {
                Point2 goalTile = new Point2(Mathf.FloorToInt(worldX / 200f), Mathf.FloorToInt(worldZ / 200f));
                return MemoryBotRouteWalker.Ensure().Begin(goalTile);
            }
            Singleton<PlayerController>.Instance().MoveToPosition(client);
            return "{\"status\":\"accepted\",\"command\":" + MemoryBotProtocol.Quote(request.Name)
                + ",\"coordinate_space\":" + MemoryBotProtocol.Quote(tileCoordinates ? "tile" : "world")
                + ",\"x\":" + worldX.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                + ",\"z\":" + worldZ.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + "}";
        }        if (request.Name == "player.stop")
        {
            if (PlayerBehavior.LocalPlayer == null) return "{\"status\":\"rejected\",\"reason\":\"no_local_player\"}";
            if (MemoryBotRouteWalker.Instance != null) { MemoryBotRouteWalker.Instance.Cancel("stopped"); }
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
            if (!string.Equals(request.Kind, "direct", StringComparison.OrdinalIgnoreCase))
            {
                Point2 goalTile = new Point2(Mathf.FloorToInt(request.X / 200f), Mathf.FloorToInt(request.Y / 200f));
                return MemoryBotRouteWalker.Ensure().Begin(goalTile);
            }
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
        // [แก้เอง 1 ก.ย. 2026] สวมของ — inventory.use ใช้กับของกิน/ของใช้เท่านั้น
        // อาวุธ/เสื้อผ้าต้องผ่าน EquipSystem.EquipItem (เส้นเดียวกับปุ่ม "สวม" ในหน้ากระเป๋า)
        if (request.Name == "inventory.equip")
        {
            if (string.IsNullOrEmpty(request.ItemId)) throw new InvalidOperationException("equip_needs_item_id");
            InventorySystem inv2 = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            EquipSystem equip = GameSystem<EquipSystem>.HasInstance() ? GameSystem<EquipSystem>.Instance() : null;
            if (inv2 == null || equip == null) return "{\"status\":\"rejected\",\"reason\":\"equip_system_unavailable\"}";
            ItemData target = null;
            foreach (ItemData item in inv2.PlayerItemList)
            {
                if (item != null && (item.Id == request.ItemId || item.PrototypeId == request.ItemId)) { target = item; break; }
            }
            if (target == null) return "{\"status\":\"rejected\",\"reason\":\"item_not_found\"}";
            equip.EquipItem(target);
            return "{\"status\":\"accepted\",\"command\":\"inventory.equip\",\"item\":" + MemoryBotProtocol.Quote(target.Name ?? target.PrototypeId) + "}";
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
            // [แก้เอง 1 ก.ย. 2026] ของธรรมชาติ (ต้นไม้/พุ่ม/หิน) **ไม่มี EntityId** — เดิมจึงเลือกไม่ได้เลย
            // ต้องกรองด้วยชนิดคอมโพเนนต์แทน แล้วเลือกตัวใกล้สุดเหมือนผู้เล่นคลิกบนจอ
            bool naturalOnly = kind == "natural" || kind == "gather";
            GameObject nearest = null;
            float distance = float.MaxValue;
            foreach (GameObject obj in objects)
            {
                if (obj == null) continue;
                if (naturalOnly && obj.GetComponentInParent<NaturalObject>() == null) continue;
                InteractionObject candidate = new InteractionObject(obj);
                if (!naturalOnly && !string.IsNullOrEmpty(request.EntityId)
                    && !string.Equals(candidate.EntityId, request.EntityId, StringComparison.Ordinal)) continue;
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
                // [แก้เอง 1 ก.ย. 2026] แอ็กชันเดียวกันมีได้หลายเมนู (Collect มี "หิน" กับ "หินก้อนใหญ่")
                // เดิมจับตัวแรกที่เจอเสมอ ⇒ ไปกดอันที่เก็บไม่ได้ แล้วเงียบ ไม่มีของเข้ากระเป๋า
                if (!string.IsNullOrEmpty(request.MenuId)
                    && !string.Equals(menu.Id ?? "", request.MenuId, StringComparison.Ordinal)) continue;
                // ของที่เก็บไม่ได้ (เลเวลไม่ถึง/ไม่มีเครื่องมือ) ต้องไม่ถูกเลือกโดยบังเอิญ
                if (menu.GatheringData != null && !menu.GatheringData.IsAvailableForGathering()) continue;
                system.SelectTargetInteractionMenu(menu);
                return Accepted(request.Name);
            }
            return "{\"status\":\"rejected\",\"reason\":\"interaction_action_not_available\"}";
        }
        // [แก้เอง 1 ก.ย. 2026] คราฟต์ผ่านโค้ดชุดเดียวกับปุ่มในหน้าคราฟต์ของเกม
        // (CraftSlotContainer.Set → เลือกวัสดุลงช่อง → CraftSystem.Craft) ไม่ใช่ยิงแพ็กเก็ตเสกของ
        // เจ้าของสั่งไว้ว่าบอทต้องเล่นแบบผู้เล่นจริง ไม่งั้นไม่รู้ว่า UI ใช้งานได้จริงไหม
        if (request.Name == "craft.make")
        {
            if (string.IsNullOrEmpty(request.EntityId) && string.IsNullOrEmpty(request.MenuId))
                throw new InvalidOperationException("craft_make_needs_recipe_id");
            string recipeId = !string.IsNullOrEmpty(request.MenuId) ? request.MenuId : request.EntityId;
            string detail;
            string error = CraftRecipe(recipeId, false, out detail);
            if (error != null)
                return "{\"status\":\"rejected\",\"reason\":" + MemoryBotProtocol.Quote(error)
                     + ",\"detail\":" + MemoryBotProtocol.Quote(detail ?? "") + "}";
            return "{\"status\":\"accepted\",\"command\":\"craft.make\",\"recipe\":" + MemoryBotProtocol.Quote(detail) + "}";
        }
        if (request.Name == "ui.open")
        {
            string uri = request.Uri ?? "";
            if (uri == "Inventory") { UIManager.Open<Durango.UI.InventoryGroup>(); return Accepted(request.Name); }
            if (uri == "Skill") { UIManager.Open<Durango.UI.SkillGroup>(); return Accepted(request.Name); }
            if (uri == "Status") { UIManager.Open<global::CharacterStatusGroup>(); return Accepted(request.Name); }
            if (uri == "Craft" || uri == "Recipe")
            {
                UIManager.Open<Durango.UI.RecipeSelectorGroup>();
                return Accepted(request.Name);
            }
            return "{\"status\":\"rejected\",\"reason\":\"ui_uri_not_allowed\"}";
        }
        if (request.Name == "title.start")
        {
            if (!GameManager.IsTitleScene)
                return "{\"status\":\"rejected\",\"reason\":\"not_title_scene\"}";
            Durango.UI.TitleMenuUserControlBase uc = UnityEngine.Object.FindObjectOfType<Durango.UI.TitleMenuUserControlBase>();
            if (uc == null)
                return "{\"status\":\"rejected\",\"reason\":\"title_control_missing\"}";
            System.Reflection.MethodInfo confirm = typeof(Durango.UI.TitleMenuUserControlBase).GetMethod(
                "OnConfirm",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (confirm == null)
                return "{\"status\":\"rejected\",\"reason\":\"onconfirm_missing\"}";
            confirm.Invoke(uc, null);
            return Accepted(request.Name);
        }
        // [3 ก.ย. 2026] กดเลือก "Online Server" ที่หน้าไตเติ้ลเหมือนคนกด — ใช้คู่กับ title.start
        // (โหมดชีวิตทำให้เองอยู่แล้ว คำสั่งนี้ไว้ขับมือ)
        if (request.Name == "title.online")
        {
            if (!GameManager.IsTitleScene)
                return "{\"status\":\"rejected\",\"reason\":\"not_title_scene\"}";
            Durango.UI.TitleMenuUserControlBase uc = UnityEngine.Object.FindObjectOfType<Durango.UI.TitleMenuUserControlBase>();
            if (uc == null)
                return "{\"status\":\"rejected\",\"reason\":\"title_control_missing\"}";
            System.Reflection.MethodInfo choose = typeof(Durango.UI.TitleMenuUserControlBase).GetMethod(
                "OnClusterConfirmed",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (choose == null)
                return "{\"status\":\"rejected\",\"reason\":\"oncluster_missing\"}";
            choose.Invoke(uc, new object[] { string.IsNullOrEmpty(request.Kind) ? "online" : request.Kind });
            return Accepted(request.Name);
        }
        if (request.Name == "estate.declare")
        {
            if (!GameManager.IsReady || PlayerBehavior.LocalPlayer == null)
                return "{\"status\":\"rejected\",\"reason\":\"game_not_ready\"}";
            Vector3 world = TerrainUtil.ClientPositionToWorldPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
            Vector2 tile = TerrainUtil.WorldPositionToTilePosition(world);
            int tx = request.HasX ? (int)request.X : (int)tile.x;
            int ty = request.HasY ? (int)request.Y : (int)tile.y;
            Point2 cell = new Point2(tx / 4, ty / 4);
            GameSystem<EstateSystem>.Instance().DeclareEstate(Shared.Estate.OwnerType.Player, cell, null);
            return "{\"status\":\"accepted\",\"command\":\"estate.declare\",\"cell\":[" + tx + "," + ty + "]}";
        }
        if (request.Name == "estate.home")
        {
            if (!GameManager.IsReady)
                return "{\"status\":\"rejected\",\"reason\":\"game_not_ready\"}";
            EstateSystem.ReturnToEstate(Shared.Estate.OwnerType.Player);
            return Accepted(request.Name);
        }
        if (request.Name == "estate.refresh")
        {
            if (!GameManager.IsReady)
                return "{\"status\":\"rejected\",\"reason\":\"game_not_ready\"}";
            EstateSystem.GetEstateLicenses(delegate { });
            return Accepted(request.Name);
        }
        throw new InvalidOperationException("unknown_command");
    }

    /// <summary>
    /// คราฟต์ตามสูตรผ่านโค้ดชุดเดียวกับปุ่มในหน้าคราฟต์ของเกม
    /// (CraftSlotContainer.Set → เลือกวัสดุลงช่อง → CraftSystem.Craft) ไม่ใช่ยิงแพ็กเก็ตเสกของ
    /// เจ้าของสั่งไว้ว่าบอทต้องเล่นแบบผู้เล่นจริง ไม่งั้นไม่รู้ว่า UI ใช้งานได้จริงไหม
    ///
    /// <paramref name="dryRun"/> = ตรวจอย่างเดียวว่าวัสดุครบไหม ไม่กดคราฟต์จริง
    /// คืน null ถ้าผ่าน · ไม่ผ่านคืนรหัสเหตุผล แล้ว <paramref name="detail"/> บอกชื่อสูตร/ช่องที่ขาด
    /// </summary>
    /// <summary>tag แรกของช่องที่ขาดจากการ CraftRecipe ครั้งล่าสุด (ว่าง = ไม่ขาด/อ่านไม่ได้)</summary>
    public static string LastMissingTag = "";

    public static string CraftRecipe(string recipeId, bool dryRun, out string detail)
    {
        detail = null;
        LastMissingTag = "";
        RecipeSystem recipes = GameSystem<RecipeSystem>.HasInstance() ? GameSystem<RecipeSystem>.Instance() : null;
        CraftSystem crafts = GameSystem<CraftSystem>.HasInstance() ? GameSystem<CraftSystem>.Instance() : null;
        InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
        if (recipes == null || crafts == null || inv == null) return "craft_system_unavailable";
        Crafting.Recipe recipe = recipes.GetRecipe(recipeId);
        if (recipe == null) return "recipe_not_found";
        if (!recipe.Available) return "recipe_locked";
        detail = recipe.Name ?? recipeId;
        Artifact workbench = recipe.HasRequiredWorkbench ? recipes.FindNearestAvailableWorkbench(recipe) : null;
        if (recipe.HasRequiredWorkbench && workbench == null) return "workbench_not_in_range";
        crafts.SlotContainer.Set(recipe, workbench, inv.PlayerInventory, null);
        // เติมวัสดุลงช่องเหมือนที่ UI เลือกให้อัตโนมัติ — ใช้ IsSuitableItem ตัวเดียวกับหน้าจอ
        IList<ItemData> items = inv.PlayerInventory.Items;
        int slots = crafts.SlotContainer.SlotCount;
        // [3 ก.ย. 2026] 🐛 วัสดุที่ถูกล็อก (เช่น blade_stone ที่ LockTools ล็อกกันทิ้ง) พอเอาไปประกอบขวาน
        //    เซิร์ฟ abort ที่ HandleCraft:897 "วัสดุถูกล็อค" ⇒ บอทวนคราฟต์ไม่จบ (เห็นโทสต์รัวบนจอ)
        //    ⇒ เก็บ id วัสดุที่เลือกจริงลงช่อง แล้วปลดล็อกก่อนกดคราฟต์ (ปลดเฉพาะที่จะใช้ ไม่แตะที่เหลือ)
        List<string> selectedIds = new List<string>();
        for (int i = 0; i < slots; i++)
        {
            SlotInfo slot = crafts.SlotContainer.GetSlotInfo(i);
            if (slot == null) continue;
            for (int k = 0; k < items.Count && slot.CurrentCount < slot.TotalCount; k++)
            {
                ItemData item = items[k];
                if (item == null) continue;
                if (crafts.SlotContainer.GatherOtherSlotsSelectedItemIds(slot).Contains(item.Id)) continue;
                if (!slot.IsSuitableItem(item)) continue;
                slot.AddSelectedItem(item);
                if (item.Locked && !string.IsNullOrEmpty(item.Id)) selectedIds.Add(item.Id);
            }
            if (slot.CurrentCount < slot.TotalCount)
            {
                detail = slot.Name ?? ("#" + i);
                LastMissingTag = MemoryBotBuild.FirstTag(slot);
                return "missing_material";
            }
        }
        if (dryRun) return null;
        // [3 ก.ย. 2026] วัสดุที่จะใช้ยังถูกล็อกอยู่ ⇒ ปลดล็อกก่อน แล้วรอรอบถัดไปค่อยกดคราฟต์
        //    (ปลดล็อกเป็น packet ไป-กลับเซิร์ฟ กดคราฟต์ทันทีในติ๊กเดียวกัน client ยังเห็นล็อก = เด้งกล่องยืนยัน)
        //    ถ้ามีกล่อง "วัสดุถูกล็อก" ค้างอยู่ ปิดทิ้ง ไม่กดตกลง (กดตกลงทั้งที่ล็อก = เซิร์ฟ abort)
        if (selectedIds.Count > 0)
        {
            MemoryBotUi.DismissMessageBox();
            inv.LockItem(false, selectedIds.ToArray());
            return "material_locked";
        }
        if (MemoryBotUi.IsMessageBoxShown()) MemoryBotUi.DismissMessageBox();
        return MemoryBotUi.CraftThroughMenu(recipeId, out detail);
    }

    public static string ResolveRecipeId(string want)
    {
        if (string.IsNullOrEmpty(want)) return null;
        RecipeSystem recipes = GameSystem<RecipeSystem>.HasInstance() ? GameSystem<RecipeSystem>.Instance() : null;
        if (recipes == null || recipes.RecipeContainer == null) return null;
        if (string.Equals(want, "bow", StringComparison.OrdinalIgnoreCase)
            || want.IndexOf("ธนู", StringComparison.Ordinal) >= 0)
            want = "bow_wooden_assembled";
        Crafting.Recipe exact = recipes.GetRecipe(want);
        if (exact != null) return exact.Id;
        string needle = want.ToLowerInvariant();
        string bestAvailable = null;
        string bestAny = null;
        recipes.RecipeContainer.EnumerateRecipes(delegate(Crafting.Recipe r)
        {
            if (r == null || string.IsNullOrEmpty(r.Id)) return;
            string id = r.Id.ToLowerInvariant();
            string name = (r.Name ?? "").ToLowerInvariant();
            string proto = "";
            Crafting.RecipeCraft craft = r as Crafting.RecipeCraft;
            if (craft != null && !string.IsNullOrEmpty(craft.PrototypeId)) proto = craft.PrototypeId.ToLowerInvariant();
            bool hit = id == needle || proto == needle
                || id.IndexOf(needle, StringComparison.Ordinal) >= 0
                || proto.IndexOf(needle, StringComparison.Ordinal) >= 0
                || name.IndexOf(needle, StringComparison.Ordinal) >= 0;
            if (!hit) return;
            if (bestAny == null) bestAny = r.Id;
            if (r.Available && bestAvailable == null) bestAvailable = r.Id;
        });
        return bestAvailable ?? bestAny;
    }

    public static int RecipeMinLevel(string recipeId)
    {
        RecipeSystem recipes = GameSystem<RecipeSystem>.HasInstance() ? GameSystem<RecipeSystem>.Instance() : null;
        if (recipes == null) return 1;
        Crafting.Recipe recipe = recipes.GetRecipe(recipeId);
        return recipe == null || recipe.MinLevel < 1 ? 1 : recipe.MinLevel;
    }

    private static string Accepted(string name)
    {
        return "{\"status\":\"accepted\",\"command\":" + MemoryBotProtocol.Quote(name) + "}";
    }
}
}
