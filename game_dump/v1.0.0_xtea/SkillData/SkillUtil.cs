using Shared.Skill;

namespace SkillData;

public static class SkillUtil
{
	public static string CategoryLocalizeName(Category category)
	{
		return LocalizeUtil.Get(category);
	}

	public static string CategoryLocalizeDescription(Category category)
	{
		return LocalizeSystem.Get($"{LocalizeUtil.GetKey(category)}_description");
	}

	public static string CategoryIcon(Category category)
	{
		return IconMap.Get(category, "icon_question");
	}
}
