using L10N;

namespace Durango.Logic.PlayGuide;

public static class ToDoFactory
{
	public static ToDoBase CreateToDo(string eventName, ToDoBase.ToDoJson json)
	{
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
			GetItemToDo getItemToDo = new GetItemToDo(json.tag, json.filters);
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
		case "craft":
			if (string.IsNullOrEmpty(json.tag))
			{
				return null;
			}
			toDoBase = new CraftToDo(json.tag, json.id);
			break;
		case "build":
			toDoBase = new BuildToDo(json.id);
			break;
		case "build_complete":
			toDoBase = new BuildCompleteToDo(json.id);
			break;
		case "completed_artifact":
			toDoBase = new CompletedArtifactToDo(json.id);
			break;
		case "set_home":
			toDoBase = new SetHomeToDo();
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
		case "do_interaction":
			toDoBase = new DoInteractionToDo(json.id);
			break;
		case "menu_button":
			toDoBase = new MenuButtonToDo(json.id);
			break;
		case "wait_time":
			toDoBase = new WaitTimeToDo(json.time_begin, json.time_end);
			break;
		case "learn_skill":
			toDoBase = new LearnSkillToDo(json.id, json.level);
			break;
		case "view_skill_list":
			toDoBase = new ViewSkillListPage(json.id);
			break;
		case "interact":
			toDoBase = new InteractToDo(json.id, json.count);
			break;
		case "return_to_home":
			toDoBase = new ReturnToHomeToDo();
			break;
		case "return_to_camp":
			toDoBase = new ReturnToCampToDo();
			break;
		case "rest":
			toDoBase = new RestToDo();
			break;
		case "gauge_fill":
			toDoBase = new GaugeToDo(json.id, json.ratio, high: true);
			break;
		case "gauge_empty":
			toDoBase = new GaugeToDo(json.id, json.ratio, high: false);
			break;
		case "status_effect":
			toDoBase = new StatusEffectToDo(json.id);
			break;
		case "sailing":
			toDoBase = new SailingToDo(json.id, json.level);
			break;
		case "level_up":
			toDoBase = new LevelUpToDo(json.level);
			break;
		case "category_level_up":
			toDoBase = new CategoryLevelToDo(json.id, json.level);
			break;
		case "join_clan":
			toDoBase = new JoinClanToDo();
			break;
		case "market_buy":
			toDoBase = new MarketBuyToDo();
			break;
		case "destruct_prop":
			toDoBase = new DestructPropToDo(json.id);
			break;
		case "warp":
			toDoBase = new WarpToDo();
			break;
		case "find_tile":
			toDoBase = new FindTileToDo(json.pos, json.radius);
			break;
		case "find_animal":
			toDoBase = new FindAnimalToDo(json.id);
			break;
		case "find_biome":
			toDoBase = new FindBiomeToDo(json.id, json.radius);
			break;
		case "find_warphole":
			toDoBase = new FindWarpholeToDo();
			break;
		case "find_crater":
			toDoBase = new FindCraterToDo();
			break;
		case "find_crack":
			toDoBase = new FindCrackToDo();
			break;
		case "find_immovable":
			toDoBase = new FindImmovable(json.id, json.radius);
			break;
		case "set_estate":
			toDoBase = new SetEstateToDo(json.id);
			break;
		case "mission_start":
			toDoBase = new MissionStartToDo(json.id);
			break;
		case "tutorial_boat":
			toDoBase = new TutorialIslandSystem.TutorialBoatToDo(json.id, json.count);
			break;
		case "airballoon_landing":
			toDoBase = new AirBalloonLandingTodo();
			break;
		case "play_emoticon":
			toDoBase = new PlayEmoticonToDo(json.id);
			break;
		case "faction_support_request":
			toDoBase = new FactionSupportRequestToDo();
			break;
		case "event_reward":
			toDoBase = new EventRewardToDo(json.id, json.index);
			break;
		case "quest_reward":
			toDoBase = new QuestRewardToDo(json.id);
			break;
		case "sailing_recommended_region":
			toDoBase = new SailingRecommendedRegionToDo(json.id);
			break;
		case "move_to_region":
			toDoBase = new MoveToRegionToDo(json.id);
			break;
		case "keyboard_shortcut":
			toDoBase = new KeyboardShortcutToDo(json.id);
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
