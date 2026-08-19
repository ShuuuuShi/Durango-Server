using Shared.Skill;

namespace Durango.Logic.Skill;

public static class Util
{
	public static string CategoryLocalizeName(Shared.Skill.Category category)
	{
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
