using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Notification;
using Durango.Utils;

namespace Durango.UI;

public static class MenuHelper
{
	private static readonly CategoryMenuNotificationContainer CategoryMenuNotificationContainer;

	static MenuHelper()
	{
		CategoryMenuNotificationContainer = new CategoryMenuNotificationContainer();
		GameManager.Reset += delegate
		{
			CategoryMenuNotificationContainer.Clear();
		};
	}

	public static INotificationable GetNotificationable(MenuType type)
	{
		if (type == MenuType.Notice)
		{
			return GameSystem<NoticeSystem>.Instance();
		}
		if (MenuContainer.HasChildren(type))
		{
			return CategoryMenuNotificationContainer.Get(type);
		}
		return GetScript(type) as INotificationable;
	}

	public static UIBase GetScript(MenuType type)
	{
		switch (type)
		{
		case MenuType.Character:
		case MenuType.CharacterOnMenu:
			return UIManager.FindScript<CharacterInfoGroup>();
		case MenuType.Skill:
			return UIManager.FindScript<SkillGroup>();
		case MenuType.Inventory:
			return UIManager.Inventory;
		case MenuType.Craft:
			return UIManager.FindScript<RecipeSelectorGroup>();
		case MenuType.Market:
		case MenuType.WarpShop:
			return UIManager.FindScript<MarketGroup>();
		case MenuType.Social:
			return UIManager.FindScript<SocialGroup>();
		case MenuType.Mail:
			return UIManager.FindScript<MailGroup>();
		case MenuType.Screenshot:
			return UIManager.FindScript<ScreenCaptureGroup>();
		case MenuType.Config:
			return UIManager.FindScript<ConfigGroup>();
		case MenuType.Encyclopedia:
			return UIManager.FindScript<EncyclopediaGroup>();
		case MenuType.Faction:
			return UIManager.FindScript<FactionGroup>();
		case MenuType.Clan:
			return UIManager.FindScript<ClanGroup>();
		case MenuType.Timeline:
			return UIManager.FindScript<TimelineLogGroup>();
		case MenuType.Pet:
			return UIManager.FindScript<PetGroup>();
		case MenuType.Estate:
			return UIManager.FindScript<EstateGroup>();
		case MenuType.Shop:
			return UIManager.FindScript<ShopGroup>();
		case MenuType.Event:
			return UIManager.FindScript<EventGroup>();
		case MenuType.Quest:
			return UIManager.FindScript<QuestGroup>();
		case MenuType.LearningGuide:
			return UIManager.FindScript<LearningGuideGroup>();
		case MenuType.Party:
			return UIManager.FindScript<PartyGroup>();
		case MenuType.PlayerSelection:
			return UIManager.FindScript<PlayerSelectionGroup>();
		case MenuType.PvpIsland:
			return UIManager.FindScript<PvpIslandGroup>();
		case MenuType.Story:
		case MenuType.StoryOnMenu:
			return UIManager.FindScript<StoryGroup>();
		case MenuType.WorldMap:
			return UIManager.FindScript<WorldMapGroup>();
		case MenuType.Music:
		case MenuType.MusicOnMenu:
			return UIManager.FindScript<MusicEditorGroup>();
		default:
			return null;
		}
	}

	public static bool IsEnabledMenu(UIBase group)
	{
		IEnumerable<MenuType> source = from type in Enums<MenuType>.All()
			where GetScript(type) == @group
			select type;
		if (source.Count() > 0)
		{
			return source.Any((MenuType type) => GameSystem<MenuSystem>.Instance().IsEnabled(type));
		}
		return true;
	}

	public static void Open(MenuType type, bool immediately = false)
	{
		UIBase script = GetScript(type);
		if (script != null)
		{
			if (immediately)
			{
				bool softOpen = script.SoftOpen;
				script.SoftOpen = false;
				script.Open();
				script.SoftOpen = softOpen;
			}
			else
			{
				script.Open();
			}
			MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
			if (menuListGroupBase != null)
			{
				menuListGroupBase.NotifyMenuOpened(type);
			}
		}
	}

	public static void SetLastOpendUI(MenuType type, UIBase script)
	{
		MenuListGroupBase menuListGroupBase = UIManager.FindScript<MenuListGroupBase>();
		if (menuListGroupBase != null)
		{
			menuListGroupBase.SetLastOpenUI(IconMap.Get(type), script);
		}
	}

	public static void Toggle(MenuType type, bool immediately = false)
	{
		UIBase script = GetScript(type);
		if (!(script == null))
		{
			if (script.IsOpened)
			{
				script.Close();
			}
			else
			{
				Open(type, immediately);
			}
		}
	}

	public static void RefreshCategoryMenuNotification()
	{
		CategoryMenuNotificationContainer.Refresh();
	}
}
