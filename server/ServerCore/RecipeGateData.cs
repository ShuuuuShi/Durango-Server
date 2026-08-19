using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// เงื่อนไข "ต้องมีความสามารถเท่าไรถึงจะได้สูตรนี้" — ค่าจริงจากข้อมูลเกม
///
/// สกัดอัตโนมัติด้วย scripts/extract_recipe_gate.py · **อย่าแก้ด้วยมือ**
///
/// ใช้ที่ `GetRecipes` เพื่อส่งเฉพาะสูตรที่ผู้เล่นปลดล็อกแล้ว
/// เดิมส่งทั้ง 720 สูตรให้ทุกคนเสมอ ⇒ เลเวล 1 ก็เห็นสูตรครบทุกอัน
///
/// สูตรที่ไม่มีในตารางนี้ = ไม่มีเงื่อนไข ได้ตั้งแต่แรก
/// </summary>
public static class RecipeGateData
{
    /// <summary>รหัสสูตร → (ความสามารถที่ต้องใช้ (Shared.Ability.Derived), ค่าที่ต้องถึง)</summary>
    public static readonly Dictionary<string, (int Ability, float Value)> Required =
        new Dictionary<string, (int, float)>
    {
        { "reform_armorcrafting", (215, 0f) },
        { "reform_breathability", (215, 0f) },
        { "reform_breathability_t2", (215, 0f) },
        { "reform_clothover", (215, 0f) },
        { "reform_clothover_t2", (215, 0f) },
        { "reform_constructing", (215, 0f) },
        { "reform_cooking_farming", (215, 0f) },
        { "reform_gathering", (215, 0f) },
        { "reform_lightening", (215, 0f) },
        { "reform_lightening_t2", (215, 0f) },
        { "reform_nail", (210, 0f) },
        { "reform_pocket", (215, 0f) },
        { "reform_pocket_t2", (215, 0f) },
        { "reform_process", (215, 0f) },
        { "reform_protect_heat", (215, 0f) },
        { "reform_scales", (215, 0f) },
        { "reform_suncover", (215, 0f) },
        { "reform_suncover_t2", (215, 0f) },
        { "reform_temper", (210, 0f) },
        { "reform_weaponcrafting", (215, 0f) },
        { "reform_windbreak", (215, 0f) },
        { "reform_windbreak_t2", (215, 0f) },
    };

    public static bool TryGet(string recipeId, out int ability, out float value)
    {
        if (recipeId != null && Required.TryGetValue(recipeId, out (int Ability, float Value) req))
        {
            ability = req.Ability;
            value = req.Value;
            return true;
        }
        ability = 0;
        value = 0f;
        return false;
    }
}
