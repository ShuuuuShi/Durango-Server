namespace Durango.Logic.PlayGuide;

public class FlowConditionFactory
{
	public static FlowCondition Create(FlowJson flow, string flowName)
	{
		string type = flow.Type;
		string param = flow.Param;
		object obj = type switch
		{
			"interaction" => new InteractionCondition(), 
			"no_item" => new NoItemCondition(), 
			"collect_item" => new CollectItemCondition(), 
			"collect_unstable_item" => new CollectUnstableItemCondition(), 
			"use_item" => new UseItemCondition(), 
			"craft_item" => new CraftItemCondition(), 
			"find_animal" => new FindAnimalCondition(), 
			"kill_animal" => new KillAnimalCondition(), 
			"kill_player" => new KillPlayerCondition(), 
			"player_dead" => new PlayerDeadCondition(), 
			"equip" => new EquipCondition(), 
			"status_effect" => new StatusEffectCondition(param), 
			"level_up" => new LevelUpCondition(), 
			"category_level_up" => new CategoryLevelUpCondition(param), 
			"gauge_low_20" => new GaugeCondition(0.2f, -1, "low"), 
			"gauge_low_50" => new GaugeCondition(0.5f, -1, "low"), 
			"gauge_high_50" => new GaugeCondition(0.5f, -1, "high"), 
			"gauge_high_80" => new GaugeCondition(0.8f, -1, "high"), 
			"collect_skill_needed" => new CollectSkillNeededCondition(), 
			"return_from_unstable" => new ReturnFromUnstableCondition(), 
			"region" => new CurrentRegionCondition(param), 
			"find_warphole" => new FindWarpholeCondition(), 
			"find_crater" => new FindCraterCondition(), 
			"find_crack" => new FindCrackCondition(), 
			"find_immovable" => new FindImmovableCondition(param), 
			"private_estate_expire" => new PrivateEstateExpirationCondition(), 
			"taming_succeed" => new TamingSucceedCondition(), 
			"acquired_reins" => new AcquiredReins(), 
			"pet_dead" => new PetDeadCondition(), 
			"mission_complete" => new MissionCompleteCondition(param), 
			"menu_open" => new MenuOpenCondition(param), 
			"warp_rush_begin" => new WarpRushBegin(), 
			"no_personal_region" => new NoPersonalRegion(), 
			"quest_reward" => new QuestReward(param), 
			"level_up_and_find_rift" => new LevelUpAndFindRiftCondition(), 
			_ => new ManualCondition(), 
		};
		((FlowCondition)obj).Name = flowName;
		((FlowCondition)obj).Param = param;
		((FlowCondition)obj).SkipLoad = flow.SkipLoad;
		((FlowCondition)obj).CanRestart = flow.CanRestart;
		((FlowCondition)obj).Region = flow.Region;
		return (FlowCondition)obj;
	}
}
