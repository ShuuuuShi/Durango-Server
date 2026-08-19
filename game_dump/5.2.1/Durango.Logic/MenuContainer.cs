using System.Collections.Generic;
using System.Linq;
using Durango.System;
using JetBrains.Annotations;

namespace Durango.Logic;

public static class MenuContainer
{
	private static readonly MenuType[] FixedMenuList;

	private static readonly List<MenuType> FirstDepth;

	private static readonly List<MenuType> UIMenu;

	private static readonly Dictionary<MenuType, List<MenuType>> Children;

	public static IEnumerable<MenuType> Menus => UIMenu;

	public static IEnumerable<MenuType> FixedMenus => FixedMenuList;

	public static IEnumerable<MenuType> FirstDepthMenus => FirstDepth;

	static MenuContainer()
	{
		FixedMenuList = new MenuType[6]
		{
			MenuType.Mail,
			MenuType.Config,
			MenuType.Notice,
			MenuType.OfficialCommunity,
			MenuType.Offerwall,
			MenuType.MoveToTitle
		};
		FirstDepth = new List<MenuType>();
		UIMenu = new List<MenuType>();
		Children = new Dictionary<MenuType, List<MenuType>>();
		Add(MenuType.CharacterOnMenu);
		Add(MenuType.CategoryCharacter, MenuType.Character, MenuType.LearningGuide, MenuType.PlayerSelection, MenuType.Music);
		Add(MenuType.Skill);
		Add(MenuType.Craft);
		Add(MenuType.Inventory);
		Add(MenuType.Connect);
		Add(MenuType.Pet);
		Add(MenuType.Market);
		Add(MenuType.Estate);
		Add(MenuType.Shop);
		Add(MenuType.CategoryToDo, MenuType.Quest, MenuType.Faction, MenuType.Event, MenuType.PvpIsland, MenuType.Story);
		Add(MenuType.Clan);
		Add(MenuType.CategorySocial, MenuType.Social, MenuType.Party);
		Add(MenuType.Encyclopedia);
		Add(MenuType.MusicOnMenu);
		Add(MenuType.StoryOnMenu);
		Add(MenuType.WarpShop);
		if (Platform.Instance.UsePCUI)
		{
			Add(MenuType.Screenshot);
		}
	}

	public static bool HasChildren(MenuType menu)
	{
		return Children.ContainsKey(menu);
	}

	public static MenuType? GetParent(MenuType menu)
	{
		foreach (KeyValuePair<MenuType, List<MenuType>> child in Children)
		{
			if (child.Value.Contains(menu))
			{
				return child.Key;
			}
		}
		return null;
	}

	[NotNull]
	public static IEnumerable<MenuType> GetChildren(MenuType category)
	{
		if (Children.TryGetValue(category, out var value))
		{
			return value;
		}
		return Enumerable.Empty<MenuType>();
	}

	private static void Add(MenuType menu, params MenuType[] children)
	{
		if (children.Length != 0)
		{
			UIMenu.AddRange(children);
			Children[menu] = children.ToList();
		}
		else
		{
			UIMenu.Add(menu);
		}
		FirstDepth.Add(menu);
	}
}
