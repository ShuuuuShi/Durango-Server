using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Clusters;
using Durango.Network;
using Durango.System;
using Durango.Utils;
using Durango.Utils.Extensions;
using Messages;

public class MenuSystem : GameSystem<MenuSystem>
{
	private const string StorageKey = "RecentlyUnlockedMenuList";

	private static readonly MenuType[] HideInTutorial;

	private static readonly MenuType[] HideInSafeHouse;

	private static readonly MenuType[] HideInWarpRush;

	private static readonly MenuType[] HiddenInOnline;

	private static readonly MenuType[] ShowInOffline;

	private static readonly MenuType[] ShowInEditable;

	private static readonly MenuType[] ShowInPvpIsland;

	private bool[] _menuEnabled;

	private bool[] _recentlyUnlocked;

	private DelayedFunction _enableMenuUpdated;

	private static readonly MenuType[] ShowInSingleMode;

	public event Action EnableMenuUpdated;

	private void Awake()
	{
		MenuType[] array = Enums<MenuType>.All();
		_menuEnabled = new bool[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_menuEnabled[i] = true;
		}
		_recentlyUnlocked = new bool[array.Length];
		Singleton<GameManager>.Instance().MainSceneLoaded += GameManager_MainSceneLoaded;
		Singleton<GameManager>.Instance().WelcomeReceived += OnWelcome;
		_enableMenuUpdated = new DelayedFunction(delegate
		{
			if (this.EnableMenuUpdated != null)
			{
				this.EnableMenuUpdated();
			}
		});
	}

	private void GameManager_MainSceneLoaded()
	{
		MenuType[] array = Enums<MenuType>.All();
		foreach (MenuType type in array)
		{
			if (IsHiddenMenu(type))
			{
				EnableMenu(type, enable: false);
			}
		}
		EnableMenu(MenuType.Offerwall, Platform.Instance.IsAvailableOfferwall);
	}

	/// <summary>
	/// [แก้เอง] ซ่อนเมนูของระบบที่ยังไม่เปิดในรอบนี้ (beta 1.0.0)
	///
	/// ขอบเขตอ้างอิงจาก `1.0.0 beta.txt` — เปิดเฉพาะระบบที่ทำเสร็จจริง
	/// เปิดเพิ่มทีละแพทช์ = **ลบชื่อออกจากรายการนี้** แล้ว build client ใหม่
	/// (`tools/build-client.ps1`) พร้อมกับเปิดสวิตช์ฝั่ง server ที่ `Features` ใน config.json
	///
	/// ⚠️ ต้องแก้ให้ตรงกันทั้งสองฝั่ง — ซ่อนเมนูอย่างเดียวไม่พอ เพราะ packet ยิงตรงได้
	/// ส่วน server ปฏิเสธอย่างเดียวก็ไม่พอ เพราะผู้เล่นจะเห็นปุ่มที่กดแล้วไม่เกิดอะไรขึ้น
	///
	/// เดิมเป็น IL patch ใน tools/DllPatcher — ย้ายมาไว้ในซอร์สแล้ว
	/// ฝั่ง server สั่งซ่อนได้แค่ Party เมนูเดียว (มี binding แค่ party.ui_enabled) จึงต้องซ่อนที่ client
	/// </summary>
	private static readonly MenuType[] NotImplementedYet =
	{
		// ระบบที่ยังไม่ได้ทำ
		MenuType.Market, MenuType.Social, MenuType.Mail, MenuType.Encyclopedia,
		MenuType.Clan, MenuType.Faction, MenuType.Timeline, MenuType.Pet,
		MenuType.Estate, MenuType.Shop, MenuType.Event, MenuType.Quest,
		MenuType.LearningGuide, MenuType.Party, MenuType.Notice, MenuType.PlayerSelection,
		MenuType.OfficialCommunity, MenuType.Offerwall, MenuType.PvpIsland, MenuType.Story,
		MenuType.Music, MenuType.CharacterOnMenu, MenuType.MusicOnMenu, MenuType.StoryOnMenu,

		// [beta 1.0.0] ปิดเพิ่ม — หมวดที่เปิดไปก็เจอแต่ของว่าง เพราะระบบข้างในยังไม่มี
		MenuType.CategoryToDo,      // "할 일" = เควส/สิ่งที่ต้องทำ — ยังไม่มีระบบเควส
		MenuType.CategorySocial,    // "친구" = เพื่อน — Social/Mail/Clan ปิดหมดแล้ว หมวดนี้จึงว่างเปล่า
		MenuType.WarpShop           // "워프 유적" = วาร์ปโฮลข้ามเกาะ — ตรงกับ Features.IslandTravel ที่ยังปิดอยู่
	};

	public static bool IsHiddenMenu(MenuType type)
	{
		for (int i = 0; i < NotImplementedYet.Length; i++)
		{
			if (NotImplementedYet[i] == type)
			{
				return true;
			}
		}
		if (Platform.Instance.UsePCUI && type == MenuType.Event)
		{
			return true;
		}
		Mode clusterMode = GameManager.ClusterMode;
		if (clusterMode == Mode.Online && HiddenInOnline.Contains(type))
		{
			return true;
		}
		switch (clusterMode)
		{
		case Mode.Offline:
			return !ShowInOffline.Contains(type);
		case Mode.Editable:
			return !ShowInEditable.Contains(type);
		case Mode.SingleMode:
			return !ShowInSingleMode.Contains(type);
		default:
			if (GameManager.Region.IsTutorial())
			{
				return HideInTutorial.Contains(type);
			}
			if (GameManager.Region.IsSafeHouse())
			{
				return HideInSafeHouse.Contains(type);
			}
			if (GameManager.Region.IsWarpRush())
			{
				return HideInWarpRush.Contains(type);
			}
			if (GameManager.Region.IsPvpIsland())
			{
				return !ShowInPvpIsland.Contains(type);
			}
			return false;
		}
	}

	private void OnWelcome(Welcome welcome)
	{
		byte[] array = welcome.Storage.Data?.Get("RecentlyUnlockedMenuList");
		if (KUtility.GetSize(array) == 0)
		{
			InitRecentlyUnlocked(GameManager.ClusterMode == Mode.Online);
		}
		else if (!LoadRecentlyUnlocked(array))
		{
			InitRecentlyUnlocked(value: true);
		}
	}

	private bool LoadRecentlyUnlocked(byte[] bytes)
	{
		Dictionary<string, bool> dictionary = Json.Read<Dictionary<string, bool>>(bytes);
		if (dictionary == null)
		{
			return false;
		}
		foreach (KeyValuePair<string, bool> item in dictionary)
		{
			if (!item.Key.TryEnum<MenuType>(out var value))
			{
				return false;
			}
			_recentlyUnlocked[(int)value] = item.Value;
		}
		return true;
	}

	private void InitRecentlyUnlocked(bool value)
	{
		MenuType[] hideInSafeHouse = HideInSafeHouse;
		foreach (MenuType menuType in hideInSafeHouse)
		{
			_recentlyUnlocked[(int)menuType] = value;
		}
	}

	private void SaveRecentlyUnlocked()
	{
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
		MenuType[] hideInSafeHouse = HideInSafeHouse;
		for (int i = 0; i < hideInSafeHouse.Length; i++)
		{
			MenuType menuType = hideInSafeHouse[i];
			dictionary.Add(menuType.ToString(), _recentlyUnlocked[(int)menuType]);
		}
		SetStorageItem msg = default(SetStorageItem);
		msg.Key = "RecentlyUnlockedMenuList";
		msg.Value = Json.WriteToBytes(dictionary);
		Connections.Frontend.Send(msg);
	}

	public void EnableMenu(MenuType type, bool enable, bool checkHidden = true)
	{
		if (checkHidden && IsHiddenMenu(type))
		{
			enable = false;
		}
		if (_menuEnabled[(int)type] != enable)
		{
			_menuEnabled[(int)type] = enable;
			_enableMenuUpdated.Call(this);
		}
	}

	public bool IsEnabled(MenuType type)
	{
		return _menuEnabled[(int)type];
	}

	public IEnumerable<MenuType> GetRecentlyUnlockedMenus()
	{
		return Enums<MenuType>.All().Where(IsRecentlyUnlocked);
	}

	public bool IsRecentlyUnlocked(MenuType type)
	{
		if (IsEnabled(type))
		{
			return _recentlyUnlocked[(int)type];
		}
		return false;
	}

	public void SetRecentlyUnlocked(MenuType type, bool on)
	{
		if (_recentlyUnlocked[(int)type] != on)
		{
			_recentlyUnlocked[(int)type] = on;
			SaveRecentlyUnlocked();
			_enableMenuUpdated.Call(this);
		}
	}

	static MenuSystem()
	{
		HideInTutorial = new MenuType[14]
		{
			MenuType.Mail,
			MenuType.Faction,
			MenuType.Pet,
			MenuType.Notice,
			MenuType.Market,
			MenuType.Clan,
			MenuType.Estate,
			MenuType.Shop,
			MenuType.Event,
			MenuType.LearningGuide,
			MenuType.Party,
			MenuType.PvpIsland,
			MenuType.Story,
			MenuType.Music
		};
		HideInSafeHouse = new MenuType[10]
		{
			MenuType.Market,
			MenuType.Clan,
			MenuType.Estate,
			MenuType.Shop,
			MenuType.Event,
			MenuType.LearningGuide,
			MenuType.Party,
			MenuType.PvpIsland,
			MenuType.Story,
			MenuType.Music
		};
		HideInWarpRush = new MenuType[17]
		{
			MenuType.Estate,
			MenuType.Quest,
			MenuType.Faction,
			MenuType.LearningGuide,
			MenuType.Clan,
			MenuType.Pet,
			MenuType.Event,
			MenuType.Shop,
			MenuType.Market,
			MenuType.Encyclopedia,
			MenuType.Party,
			MenuType.PvpIsland,
			MenuType.Mail,
			MenuType.Social,
			MenuType.Notice,
			MenuType.PlayerSelection,
			MenuType.Story
		};
		HiddenInOnline = new MenuType[6]
		{
			MenuType.Connect,
			MenuType.CharacterOnMenu,
			MenuType.MusicOnMenu,
			MenuType.StoryOnMenu,
			MenuType.MoveToTitle,
			MenuType.WarpShop
		};
		ShowInOffline = new MenuType[10]
		{
			MenuType.Character,
			MenuType.Inventory,
			MenuType.Connect,
			MenuType.Encyclopedia,
			MenuType.Music,
			MenuType.Story,
			MenuType.Screenshot,
			MenuType.Config,
			MenuType.MoveToTitle,
			MenuType.WorldMap
		};
		ShowInEditable = new MenuType[29]
		{
			MenuType.Character,
			MenuType.LearningGuide,
			MenuType.PlayerSelection,
			MenuType.Music,
			MenuType.Skill,
			MenuType.Inventory,
			MenuType.Connect,
			MenuType.Craft,
			MenuType.Market,
			MenuType.WarpShop,
			MenuType.Pet,
			MenuType.Event,
			MenuType.Shop,
			MenuType.Estate,
			MenuType.Quest,
			MenuType.Faction,
			MenuType.Story,
			MenuType.Clan,
			MenuType.Social,
			MenuType.Party,
			MenuType.PvpIsland,
			MenuType.Encyclopedia,
			MenuType.Timeline,
			MenuType.Screenshot,
			MenuType.WorldMap,
			MenuType.Mail,
			MenuType.Notice,
			MenuType.MoveToTitle,
			MenuType.Config
		};
		ShowInSingleMode = new MenuType[28]
		{
			MenuType.Character,
			MenuType.LearningGuide,
			MenuType.PlayerSelection,
			MenuType.Music,
			MenuType.Skill,
			MenuType.Inventory,
			MenuType.Connect,
			MenuType.Craft,
			MenuType.Market,
			MenuType.Pet,
			MenuType.Event,
			MenuType.Shop,
			MenuType.Estate,
			MenuType.Quest,
			MenuType.Faction,
			MenuType.Story,
			MenuType.Clan,
			MenuType.Social,
			MenuType.Party,
			MenuType.PvpIsland,
			MenuType.Encyclopedia,
			MenuType.Timeline,
			MenuType.Screenshot,
			MenuType.WorldMap,
			MenuType.Mail,
			MenuType.Notice,
			MenuType.MoveToTitle,
			MenuType.Config
		};
		ShowInPvpIsland = new MenuType[6]
		{
			MenuType.Character,
			MenuType.CategoryCharacter,
			MenuType.Inventory,
			MenuType.WorldMap,
			MenuType.Screenshot,
			MenuType.Config
		};
	}
}
