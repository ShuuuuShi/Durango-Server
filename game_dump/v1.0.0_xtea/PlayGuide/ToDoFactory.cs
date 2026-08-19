using L10N;

namespace PlayGuide;

public static class ToDoFactory
{
	public static ToDoBase CreateToDo(string eventName, ToDoBase.ToDoJson json)
	{
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(json.type))
		{
			return null;
		}
		ToDoBase toDoBase;
		switch (json.type)
		{
		case "manual":
			toDoBase = new ManualToDo();
			break;
		case "move_player":
		{
			MovePlayerToDo movePlayerToDo = new MovePlayerToDo();
			movePlayerToDo.CheckTime = json.time;
			toDoBase = movePlayerToDo;
			break;
		}
		case "gather_item":
			toDoBase = new GatherItemToDo(json.count);
			break;
		case "use_item":
			if (string.IsNullOrEmpty(json.tag))
			{
				return null;
			}
			toDoBase = new UseItemToDo(json.tag);
			break;
		case "get":
		{
			if (string.IsNullOrEmpty(json.tag) && (json.filters == null || json.filters.Count == 0))
			{
				return null;
			}
			GetItemToDo getItemToDo = new GetItemToDo(json.tag, json.filters, json.ignore_already_owned);
			getItemToDo.TargetProgress = json.count;
			toDoBase = getItemToDo;
			break;
		}
		case "equip":
			if (string.IsNullOrEmpty(json.id))
			{
				return null;
			}
			toDoBase = new EquipToDo(json.id, json.tag);
			break;
		case "equip_slot_click":
			toDoBase = new EquipSlotClickToDo(json.id);
			break;
		case "craft":
			if (string.IsNullOrEmpty(json.tag))
			{
				return null;
			}
			toDoBase = new CraftToDo(json.tag, json.id);
			break;
		case "build":
			if (string.IsNullOrEmpty(json.id))
			{
				return null;
			}
			toDoBase = new BuildToDo(json.id);
			break;
		case "build_complete":
			if (string.IsNullOrEmpty(json.id))
			{
				return null;
			}
			toDoBase = new BuildCompleteToDo(json.id);
			break;
		case "set_home":
			toDoBase = new SetHomeToDo();
			break;
		case "set_base":
			toDoBase = new SetBaseToDo();
			break;
		case "run_away":
			toDoBase = new RunAwayToDo(json.time);
			break;
		case "hunt":
			toDoBase = new HuntToDo(json.id);
			break;
		case "click_button":
			toDoBase = new ClickButtonToDo(json.id);
			break;
		case "action_button":
			toDoBase = new ActionButtonToDo(json.id);
			break;
		case "menu_button":
			toDoBase = new MenuButtonToDo(json.id);
			break;
		case "wait_time":
			toDoBase = new WaitTimeToDo(json.time_begin, json.time_end);
			break;
		case "learn_skill":
		{
			string[] array = json.id.Split('/');
			toDoBase = new LearnSkillToDo(array[0], (array.Length <= 1) ? null : array[1], (array.Length > 1) ? json.level : 0);
			break;
		}
		case "interact":
			toDoBase = new InteractToDo(json.id, json.count);
			break;
		case "return_home":
			toDoBase = new ReturnHomeToDo();
			break;
		case "rest":
			toDoBase = new RestToDo();
			break;
		case "gauge_fill":
			toDoBase = new GaugeToDo(json.id, json.ratio, "high");
			break;
		case "gauge_empty":
			toDoBase = new GaugeToDo(json.id, json.ratio, "low");
			break;
		case "status_effect":
			toDoBase = new StatusEffectToDo(json.id);
			break;
		case "sailing":
			toDoBase = new SailingToDo();
			break;
		case "level_up":
			toDoBase = new LevelUpToDo(json.level);
			break;
		case "category_level_up":
			toDoBase = new CategoryLevelToDo(json.id, json.level);
			break;
		case "clan_join":
			toDoBase = new ClanToDo();
			break;
		case "market_buy":
			toDoBase = new MarketBuyToDo();
			break;
		case "destruct_prop":
			toDoBase = new DestructPropToDo(json.id);
			break;
		case "reach_pos":
			toDoBase = new ReachPosToDo(json.pos, json.radius);
			break;
		case "warp":
			toDoBase = new WarpToDo();
			break;
		case "auto_guide_set":
			toDoBase = new AutoGuideSetToDo();
			break;
		case "find_animal":
			toDoBase = new FindAnimalToDo(json.id);
			break;
		case "find_biome":
			toDoBase = new FindBiomeToDo(json.id);
			break;
		case "find_warphole":
			toDoBase = new FindWarpholeToDo();
			break;
		case "find_crater":
			toDoBase = new FindCraterToDo();
			break;
		case "find_immovable":
			toDoBase = new FindImmovable(json.id, json.radius);
			break;
		default:
			return null;
		}
		PostProcess(eventName, json, toDoBase);
		return toDoBase;
	}

	private static void PostProcess(string eventName, ToDoBase.ToDoJson json, ToDoBase item)
	{
		string text = json.name;
		if (string.IsNullOrEmpty(text))
		{
			text = json.type;
			if (!string.IsNullOrEmpty(json.id))
			{
				text = text + "_" + json.id;
			}
			if (!string.IsNullOrEmpty(json.tag))
			{
				text = text + "_" + json.tag;
			}
		}
		item.Key = eventName + "." + text;
		if (!string.IsNullOrEmpty(json.message))
		{
			item.LocalText = T._(json.message);
		}
		item.Tooltip = T._(json.tooltip);
	}
}
