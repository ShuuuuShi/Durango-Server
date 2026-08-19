using Messages;
using Shared.Ability;
using Yaml;
using Yaml.Util;

public static class ArchipelagoRouteExtension
{
	public static bool IsAllConditionSatisfied(this ArchipelagoRoute archipelagoRoute)
	{
		bool flag = archipelagoRoute.IsPrerequisiteQuestFinished();
		if (archipelagoRoute.IsEpic)
		{
			return flag;
		}
		bool flag2 = archipelagoRoute.IsResistanceLevelSatisfied();
		bool flag3 = archipelagoRoute.IsClearedUnstableFactorSatisfied();
		bool flag4 = archipelagoRoute.IsPioneerGradeSatisfied();
		return flag3 && flag2 && flag4 && flag;
	}

	public static bool IsResistanceLevelSatisfied(this ArchipelagoRoute archipelagoRoute)
	{
		int num = SingletonDict<int, Recommends>.Instance.Get(archipelagoRoute.UnstableFactor)?.RequiredResistanceLevel ?? 0;
		Derived type = Singleton<Constants>.Instance.Resistance.TypeByBiome.Get(archipelagoRoute.Biome, Derived.MaxHealth);
		int resistanceLevel = GameSystem<StatisticsSystem>.Instance().GetResistanceLevel(type);
		return num <= resistanceLevel;
	}

	public static bool IsClearedUnstableFactorSatisfied(this ArchipelagoRoute archipelagoRoute)
	{
		int clearedUnstableFactor = GameSystem<ExploreSystem>.Instance().GetClearedUnstableFactor(archipelagoRoute.Level, archipelagoRoute.Biome);
		return clearedUnstableFactor >= archipelagoRoute.UnstableFactor - 1;
	}

	public static bool IsPioneerGradeSatisfied(this ArchipelagoRoute archipelagoRoute)
	{
		return GameSystem<EstateSystem>.Instance().PioneerGradeInfo.CurrentAccessLevel >= archipelagoRoute.UnstableFactor;
	}

	public static bool IsPrerequisiteQuestFinished(this ArchipelagoRoute archipelagoRoute)
	{
		if (!archipelagoRoute.PrerequisiteQuest.HasValue)
		{
			return true;
		}
		return archipelagoRoute.PrerequisiteQuest.Value.Item2;
	}
}
