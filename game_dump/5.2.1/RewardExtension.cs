using Messages;

public static class RewardExtension
{
	public static bool IsEmpty(this RewardInfo info)
	{
		if (!info.Exp.HasValue && info.Currency == null && !info.SkillPoints.HasValue && !info.UsableSkillPoints.HasValue && info.Abilities == null && info.DerivedAbilities == null && info.UnlockedSkills == null && info.Titles == null && info.FriendshipPoint == null && info.Items == null && info.RandomItems == null && info.Vouchers == null && !info.QuestScore.HasValue && info.RecipeIds == null)
		{
			return info.Memos == null;
		}
		return false;
	}
}
