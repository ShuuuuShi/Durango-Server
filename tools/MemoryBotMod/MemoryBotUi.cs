using System;
using System.Collections.Generic;
using Crafting;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Skill;
using Durango.UI;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace DurangoMemoryBot
{
    /// <summary>
    /// ทำผ่านหน้าจอเกมจริง (เปิดเมนูคราฟต์/สกิล แล้วกดปุ่ม) ไม่ยิง Craft/Learn เป็นคำสั่งลัด
    /// </summary>
    internal static class MemoryBotUi
    {
        public static bool IsInventoryFull()
        {
            InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inv == null || inv.PlayerInventory == null) return false;
            Inventory bag = inv.PlayerInventory;
            if (bag.Capacity <= 0) return false;
            return bag.CurrentSize() >= bag.Capacity;
        }

        public static void LockTools()
        {
            // [3 ก.ย. 2026] โหมดชีวิตไม่ล็อกของ — เพราะ blade_stone เป็น "ชิ้นส่วน" ที่ต้องเอาไปประกอบขวาน
            //    ถ้าล็อกไว้ เซิร์ฟจะ abort ตอนคราฟต์ (HandleCraft:897 "วัสดุถูกล็อค") วนไม่จบ
            //    โหมดชีวิตกันของหายด้วย keep-list ตอนทิ้งขยะอยู่แล้ว ไม่ต้องล็อก
            if (MemoryBotLife.Running) return;
            InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inv == null || inv.PlayerInventory == null) return;
            List<string> ids = new List<string>();
            foreach (ItemData item in inv.PlayerInventory.Items)
            {
                if (item == null || item.Locked) continue;
                string proto = item.PrototypeId ?? "";
                // ล็อกเฉพาะ "อาวุธ/เครื่องมือที่ประกอบเสร็จแล้ว" ไม่ล็อก blade_stone (ชิ้นส่วนดิบที่ใช้ประกอบต่อ)
                if (proto.IndexOf("blade_stone", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                if (proto.IndexOf("blade", StringComparison.OrdinalIgnoreCase) < 0
                    && proto.IndexOf("knife", StringComparison.OrdinalIgnoreCase) < 0
                    && proto.IndexOf("axe", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                ids.Add(item.Id);
            }
            if (ids.Count == 0) return;
            inv.LockItem(true, ids.ToArray());
        }

        public static string CraftThroughMenu(string recipeId, out string detail)
        {
            detail = null;
            string dry = MemoryBotCommands.CraftRecipe(recipeId, true, out detail);
            if (dry != null) return dry;

            RecipeSystem recipes = GameSystem<RecipeSystem>.Instance();
            Recipe recipe = recipes.GetRecipe(recipeId);
            if (recipe == null) return "recipe_not_found";

            RecipeSelectorGroup list = UIManager.FindScript<RecipeSelectorGroup>();
            if (list == null) return "craft_ui_missing";
            if (!list.IsOpened) UIManager.Open<RecipeSelectorGroup>();
            list.Open(RecipeSystem.RecipeType.Crafting, recipeId);

            Artifact bench = recipe.HasRequiredWorkbench ? recipes.FindNearestAvailableWorkbench(recipe) : null;
            if (recipe.HasRequiredWorkbench && bench == null) return "workbench_not_in_range";

            CraftGroupBase craft = UIManager.FindScript<CraftGroupBase>();
            if (craft == null) return "craft_ui_missing";
            if (!craft.Open(recipe, bench, true)) return "craft_ui_open_failed";
            if (!Click(craft.GetButtonTransform())) return "craft_button_missing";
            detail = recipe.Name ?? recipeId;
            return null;
        }

        public static bool OpenSkillMenu(Durango.Logic.Skill.Skill skill)
        {
            if (skill == null) return false;
            SkillGroup group = UIManager.FindScript<SkillGroup>();
            if (group == null) return false;
            Node node = skill.Get(skill.Level + 1);
            if (node == null) return false;
            group.Open(node);
            SkillInfoWidget info = group.GetComponentInChildren<SkillInfoWidget>(true);
            if (info != null) info.LearnAndSelectNextSkill(node);
            else GameSystem<SkillSystem>.Instance().LearnSkill(skill, null);
            return true;
        }

        public static void OpenCraftAndSkillMenus()
        {
            UIManager.Open<RecipeSelectorGroup>();
            UIManager.Open<SkillGroup>();
        }

        /// <summary>ปลดล็อกของทุกชิ้นในกระเป๋า (คนจริงกดปลดล็อกเอง) — กันเซิร์ฟ abort ตอนคราฟต์/สร้าง</summary>
        public static void UnlockAll()
        {
            InventorySystem inv = GameSystem<InventorySystem>.HasInstance() ? GameSystem<InventorySystem>.Instance() : null;
            if (inv == null || inv.PlayerInventory == null) return;
            List<string> ids = new List<string>();
            foreach (ItemData item in inv.PlayerInventory.Items)
                if (item != null && item.Locked && !string.IsNullOrEmpty(item.Id)) ids.Add(item.Id);
            if (ids.Count > 0) inv.LockItem(false, ids.ToArray());
        }

        /// <summary>
        /// [3 ก.ย. 2026] ปิดกล่องยืนยัน "วัสดุถูกล็อก ประดิษฐ์ต่อหรือไม่" (CraftGroupBase → UIManager.MessageBox)
        /// โดยไม่กดคราฟต์ (กดคราฟต์ทั้งที่ล็อก = เซิร์ฟ abort วนไม่จบ) — คืน true ถ้ามีกล่องให้ปิด
        /// เราปลดล็อกวัสดุแล้วค่อยคราฟต์ใหม่รอบถัดไปแทน
        /// </summary>
        public static bool DismissMessageBox()
        {
            try
            {
                MessageBox mb = UIManager.MessageBox;
                if (mb == null || !mb.IsShow) return false;
                System.Reflection.MethodInfo show = typeof(MessageBox).GetMethod(
                    "Show",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, new[] { typeof(bool) }, null);
                if (show != null) show.Invoke(mb, new object[] { false });
                return true;
            }
            catch (Exception) { return false; }
        }

        public static bool IsMessageBoxShown()
        {
            try { MessageBox mb = UIManager.MessageBox; return mb != null && mb.IsShow; }
            catch (Exception) { return false; }
        }

        private static bool Click(Transform t)
        {
            if (t == null) return false;
            SelectableButton button = t.GetComponent<SelectableButton>();
            if (button == null) button = t.GetComponentInChildren<SelectableButton>(true);
            if (button == null || button.Clicked == null) return false;
            button.Clicked();
            return true;
        }
    }
}
