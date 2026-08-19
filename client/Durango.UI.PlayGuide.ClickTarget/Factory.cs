using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using JetBrains.Annotations;

namespace Durango.UI.PlayGuide.ClickTarget;

public static class Factory
{
	[NotNull]
	public static Locator Create(string type, Dictionary<string, Parameter> dict)
	{
		if (dict == null)
		{
			dict = new Dictionary<string, Parameter>();
		}
		Locator locator;
		switch (type)
		{
		case "menu":
			locator = new LocatorMenu();
			break;
		case "inventory":
			locator = new LocatorInventory();
			break;
		case "interaction":
			locator = new LocatorInteraction();
			break;
		case "interaction_movable":
			locator = new LocatorInteraction(null, movable: true);
			break;
		case "craft":
			locator = new LocatorCraft();
			break;
		case "interaction_and_craft":
			locator = new LocatorInteractionAndCraft();
			break;
		case "build":
			locator = new LocatorBuild();
			break;
		case "tutorial_boat":
			locator = new LocatorBuild(tutorial: true);
			break;
		case "estate":
			locator = new LocatorEstate();
			break;
		case "learning_guide":
			locator = new LocatorLearningGuide();
			break;
		case "sailing":
			locator = new LocatorSailing();
			break;
		case "skill":
			locator = new LocatorSkill();
			break;
		case "equip":
			locator = new LocatorEquip();
			break;
		case "context_action":
			locator = new LocatorContextAction();
			break;
		case "mission_start":
			locator = new LocatorMissionStart();
			break;
		case "delivery":
			locator = new LocatorMissionDelivery();
			break;
		case "worldmap":
			locator = new LocatorWorldMap();
			break;
		case "emoticon":
			locator = new LocatorEmoticon();
			break;
		case "faction_support_request":
			locator = new LocatorFactionSupportRequest();
			break;
		case "event":
			locator = new LocatorEvent();
			break;
		case "quest":
			locator = new LocatorQuest();
			break;
		case "recommended_region":
			locator = new LocatorRecommendedRegion();
			break;
		default:
			if (!string.IsNullOrEmpty(type))
			{
			}
			locator = new Locator();
			break;
		}
		locator.Initialize(dict);
		return locator;
	}
}
