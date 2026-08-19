using System;
using ExploreData;
using MenuData;
using Shared.Region;
using UnityEngine;

public class MenuSystem : GameSystem<MenuSystem>
{
	private bool[] _menuEnabled;

	public event Action EnableMenuUpdated;

	private void Awake()
	{
		Array values = Enum.GetValues(typeof(MenuType));
		_menuEnabled = new bool[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			_menuEnabled[i] = true;
		}
		KSingleton<GameManager>.Instance().MainSceneLoaded += GameManager_MainSceneLoaded;
	}

	private void GameManager_MainSceneLoaded()
	{
		Region region = KSingleton<GameManager>.Instance().Region;
		if (region != null)
		{
			Role role = region.Role();
			bool enable = role != Role.Tutorial && role != Role.Bootcamp;
			EnableMenu(MenuType.AutoGuide, enable);
			EnableMenu(MenuType.Mail, enable);
			EnableMenu(MenuType.Market, enable);
			EnableMenu(MenuType.Clan, enable);
			EnableMenu(MenuType.Faction, role != Role.Tutorial);
		}
	}

	public void EnableMenu(MenuType type, bool enable)
	{
		if (_menuEnabled[(int)type] == enable)
		{
			return;
		}
		_menuEnabled[(int)type] = enable;
		UIBase script = GetScript(type);
		if (!((Object)(object)script == (Object)null))
		{
			if (script is INewCheckerable newCheckerable)
			{
				newCheckerable.NewChecker.Enable = enable;
			}
			if (this.EnableMenuUpdated != null)
			{
				this.EnableMenuUpdated();
			}
		}
	}

	public bool IsEnabled(MenuType type)
	{
		return _menuEnabled[(int)type];
	}

	public static bool IsMenuAvailable(MenuType type)
	{
		if (type == MenuType.Music)
		{
			return Debug.isDebugBuild;
		}
		return true;
	}

	public static UIBase GetScript(MenuType type)
	{
		return type switch
		{
			MenuType.Character => UIManager.FindScript<CharacterInfoGroup>(), 
			MenuType.Equip => UIManager.FindScript<EquipGroup>(), 
			MenuType.Skill => UIManager.FindScript<SkillGroup>(), 
			MenuType.Inventory => UIManager.FindScript<InventoryGroup>(), 
			MenuType.Craft => UIManager.FindScript<RecipeSelectorGroup>(), 
			MenuType.Market => UIManager.FindScript<MarketGroup>(), 
			MenuType.Social => UIManager.FindScript<SocialGroup>(), 
			MenuType.Mail => UIManager.FindScript<MailGroup>(), 
			MenuType.Screenshot => UIManager.FindScript<ScreenCaptureGroup>(), 
			MenuType.Config => UIManager.FindScript<ConfigGroup>(), 
			MenuType.Encyclopedia => UIManager.FindScript<EncyclopediaGroup>(), 
			MenuType.Faction => UIManager.FindScript<FactionGroup>(), 
			MenuType.AutoGuide => UIManager.FindScript<AutoGuideGroup>(), 
			MenuType.Music => UIManager.FindScript<MusicEditorGroup>(), 
			MenuType.Clan => UIManager.FindScript<ClanGroup>(), 
			MenuType.Ticket => UIManager.FindScript<TicketGroup>(), 
			_ => null, 
		};
	}
}
