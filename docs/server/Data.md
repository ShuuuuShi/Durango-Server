# ตารางข้อมูล — `RecipeData` / `SkillData` / `NaturalData`

ไฟล์ที่ถอดข้อมูลออกมาจากตัวเกมแล้ว hardcode ไว้เป็น C# เพื่อให้ server อ้างอิงได้

## `ServerCore/RecipeData.cs` — 3,183 บรรทัด (ใหญ่สุดในโปรเจกต์)

| สมาชิก | ใช้ที่ไหน |
|---|---|
| `AllRecipeIds` | ตอบ `GetRecipes` — ส่งสูตร**ทั้งหมด**ให้ client |
| `AllBlueprintIds` | ตอบ `GetArtifactBlueprints` |
| `RecipeInfo[id]` → `(name, icon)` | `HandleCraft` ใช้ตั้งชื่อ/ไอคอนของที่คราฟต์ |
| `BlueprintType[id]` → `ushort` | แปลง blueprint เป็น entity type ตอนก่อสร้าง |
| `BlueprintSize[id]` → ขนาด tile | ใช้เมื่อ client ไม่ส่งขนาดมา |

> ถ้าเปิด Online mode ตามที่เขียนใน [ARCHITECTURE ข้อ 7](../ARCHITECTURE.md) ไฟล์นี้จะไม่จำเป็นอีกต่อไป
> เพราะข้อมูลจะมาจาก `/assets/item/recipes` และ `/assets/building/blueprints` ที่เราเสิร์ฟเองแทน

## `ServerCore/RecipeRequirements.cs` — 766 บรรทัด (สร้างอัตโนมัติ) ✅ GP-08

วัตถุดิบที่แต่ละสูตรต้องใช้ — **720 สูตร / 1,756 ช่อง**

```csharp
Recipes["axe_tool_bone_01"] = new[] {
    S("main",      1, 1, new[]{"blade_tool"}, new[]{"stone"}),
    S("connector", 1, 1, null,                new[]{"rope"}),
    S("handle",    1, 1, new[]{"handle"},     null),
};
```

| ฟิลด์ | มาจาก | ใช้ทำอะไร |
|---|---|---|
| `Id` | `slot_id` | คีย์ที่ client ส่งมาใน `Craft.Materials` |
| `Min` / `Max` | `count_min` / `count_max` | `ValidateMaterials` ตรวจจำนวนต่อช่อง |
| `Tags` / `Materials` | `required_tags` / `required_materials` | **ยังไม่ได้ใช้ตรวจ** — รอไอเทมมี `Tags` ก่อน |

สร้างด้วย:
```bash
python scripts/extract_recipes.py game/DurangoV2_Data/resources.strings.txt ServerCore/RecipeRequirements.cs
```
สคริปต์หา TextAsset ชื่อ `recipes` ใน dump แล้วเติมปีกกาชั้นนอกที่ dump ตัดทิ้งกลับเข้าไป
(ลองปิดทีละชั้นจนกว่า `json.loads` จะผ่าน) **อย่าแก้ไฟล์ผลลัพธ์ด้วยมือ**

## `ServerCore/SkillData.cs` — 284 บรรทัด
`SkillCategory[skillId]` → เลข category ใช้ใน `HandleLearnSkill` เพื่อจัดสกิลเข้าหมวด
ไม่เจอ = ใช้ `0` (ไม่ throw)

## `ServerCore/NaturalData.cs` — 64 บรรทัด
| สมาชิก | ใช้ที่ไหน |
|---|---|
| `Map[entityType]` → `GenEntry[]` | `MakeGenerators()` — ต้นไม้ชนิดนี้เก็บอะไรได้บ้าง |
| `MotionIds` / `EmoticonIds` | ตอบ `GetAvailableEmotions` — ท่าทางและอิโมติคอนที่ใช้ได้ |

`GenEntry` = `(Prototype, Name, Icon)` — คือของที่จะได้เมื่อเก็บ

---

## จะเพิ่มของใหม่ต้องแก้ตรงไหน

| อยากเพิ่ม | แก้ที่ |
|---|---|
| ต้นไม้/ก้อนหินชนิดใหม่ที่เก็บได้ | `NaturalData.Map` |
| ท่าทาง/อิโมติคอน | `NaturalData.MotionIds` / `EmoticonIds` |
| สูตรคราฟต์ | `RecipeData.AllRecipeIds` + `RecipeInfo` + `RecipeRequirements.Recipes` (ไม่มีในตัวหลัง = คราฟต์ไม่ได้) |
| สิ่งปลูกสร้าง | `RecipeData.AllBlueprintIds` + `BlueprintType` + `BlueprintSize` |

⚠️ ข้อจำกัด: เพิ่มได้เฉพาะของที่ **มี asset อยู่ในเกมแล้ว** — id ที่ client ไม่รู้จักจะวาดไม่ออก
ของใหม่จริง ๆ ต้องทำ asset bundle ด้วย Unity 2017.4.34f1
