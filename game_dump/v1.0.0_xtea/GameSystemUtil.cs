using UnityEngine;

public static class GameSystemUtil
{
	private static Transform _transform;

	public static Transform Transform
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected O, but got Unknown
			if ((Object)(object)_transform == (Object)null)
			{
				GameObject val = new GameObject("GameSystem");
				Object.DontDestroyOnLoad((Object)(object)val);
				_transform = val.transform;
			}
			return _transform;
		}
	}

	public static void InstantiateGameSystem(bool deleteOld)
	{
		if ((Object)(object)_transform != (Object)null && deleteOld)
		{
			((Component)_transform).gameObject.BroadcastMessage("Destroy");
			Object.Destroy((Object)(object)((Component)_transform).gameObject);
			_transform = null;
		}
		GameSystem<BuildSystem>.Instance();
		GameSystem<RecipeSystem>.Instance();
		GameSystem<ItemCraftingSystem>.Instance();
		GameSystem<CombatSystem>.Instance();
		GameSystem<EquipSystem>.Instance();
		GameSystem<InteractionSystem>.Instance();
		GameSystem<InventorySystem>.Instance();
		GameSystem<TimerSystem>.Instance();
		GameSystem<SocialSystem>.Instance();
		GameSystem<PlayerStatusEffectSystem>.Instance();
		GameSystem<TargetStatusEffectSystem>.Instance();
		GameSystem<OptionSystem>.Instance();
		GameSystem<ExploreSystem>.Instance();
		GameSystem<CPRSystem>.Instance();
		GameSystem<MailSystem>.Instance();
		GameSystem<ClanSystem>.Instance();
		GameSystem<EstateSystem>.Instance();
		GameSystem<StatisticsSystem>.Instance();
		GameSystem<SkillSystem>.Instance();
		GameSystem<MapSystem>.Instance();
		GameSystem<MarketSystem>.Instance();
		GameSystem<FatigueSystem>.Instance();
		GameSystem<TutorialIslandSystem>.Instance();
		GameSystem<AutoGuideSystem>.Instance();
		GameSystem<EncyclopediaSystem>.Instance();
		GameSystem<TimelineLogSystem>.Instance();
		GameSystem<SendReportSystem>.Instance();
		GameSystem<MenuSystem>.Instance();
		GameSystem<FactionSystem>.Instance();
		GameSystem<TicketSystem>.Instance();
		GameSystem<PackArtifactSystem>.Instance();
	}
}
