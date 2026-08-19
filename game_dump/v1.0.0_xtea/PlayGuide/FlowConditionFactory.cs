namespace PlayGuide;

public class FlowConditionFactory
{
	public static FlowCondition Create(string type, string param, string flowName)
	{
		if (string.IsNullOrEmpty(type))
		{
			return null;
		}
		FlowCondition flowCondition = null;
		switch (type)
		{
		case "manual":
			flowCondition = new ManualCondition();
			break;
		case "interaction":
			flowCondition = new InteractionCondition();
			break;
		case "collect_item":
			flowCondition = new CollectItemCondition();
			break;
		case "use_item":
			flowCondition = new UseItemCondition();
			break;
		case "craft_item":
			flowCondition = new CraftItemCondition();
			break;
		case "find_animal":
			flowCondition = new FindAnimalCondition();
			break;
		case "kill_animal":
			flowCondition = new KillAnimalCondition();
			break;
		case "kill_player":
			flowCondition = new KillPlayerCondition();
			break;
		case "player_dead":
			flowCondition = new PlayerDeadCondition();
			break;
		case "equip":
			flowCondition = new EquipCondition();
			break;
		case "status_effect":
			flowCondition = new StatusEffectCondition();
			break;
		case "main_status":
			flowCondition = new MainStatusCondition();
			break;
		case "level_up":
			flowCondition = new LevelUpCondition();
			break;
		case "category_level_up":
			flowCondition = new CategoryLevelUpCondition(param);
			break;
		case "gauge_low_20":
			flowCondition = new GaugeCondition(0.2f, -1, "low");
			break;
		case "gauge_low_50":
			flowCondition = new GaugeCondition(0.5f, -1, "low");
			break;
		case "gauge_high_90":
			flowCondition = new GaugeCondition(0.9f, -1, "high");
			break;
		case "collect_fail":
			flowCondition = new CollectFailCondition(param);
			break;
		case "return_from_unstable":
			flowCondition = new ReturnFromUnstableCondition();
			break;
		case "find_warphole":
			flowCondition = new FindWarpholeCondition();
			break;
		case "find_crater":
			flowCondition = new FindCraterCondition();
			break;
		}
		if (flowCondition != null)
		{
			flowCondition.Name = flowName;
			flowCondition.Param = param;
		}
		return flowCondition;
	}
}
