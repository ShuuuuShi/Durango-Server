using Shared.Ability;

namespace StatisticsData;

public static class Statistics
{
	public static readonly Basic[] PhysicalAbility = new Basic[4]
	{
		Basic.Strength,
		Basic.Agility,
		Basic.Endurance,
		Basic.Charisma
	};

	public static readonly Basic[] MentalAbility = new Basic[4]
	{
		Basic.Intelligence,
		Basic.Dexterity,
		Basic.Will,
		Basic.Perception
	};
}
