using Shared.Skill;

namespace Durango.Logic.Skill;

public static class Util
{
	public static string CategoryLocalizeName(Shared.Skill.Category category)
	{
		// ข้อมูล localization (localize_text_enum) ยังใช้ชื่อ enum เก่า "WeaponCrafting"/"ArmorCrafting"
		// (ตัว C ใหญ่) แต่ enum ปัจจุบันคือ "Weaponcrafting"/"Armorcrafting" (ตัว c เล็ก)
		// ⇒ LocalizeSystem.Get หา key ไม่เจอแล้วคืน raw key กลับมา แก้โดยเทียบ key เก่าให้ตรงก่อน
		if (category == Shared.Skill.Category.Weaponcrafting || category == Shared.Skill.Category.Armorcrafting)
		{
			string legacy = category == Shared.Skill.Category.Weaponcrafting ? "WeaponCrafting" : "ArmorCrafting";
			string legacyKey = "#Shared.Skill.Category." + legacy;
			string name = LocalizeSystem.Get(legacyKey);
			if (name != legacyKey)
			{
				return name;
			}
		}
		return LocalizeUtil.Get(category);
	}

	public static string CategoryLocalizeDescription(Shared.Skill.Category category)
	{
		return LocalizeSystem.Get($"{LocalizeUtil.GetKey(category)}_description");
	}

	public static string CategoryIcon(Shared.Skill.Category category)
	{
		return IconMap.Get(category, "icon_question");
	}
}
