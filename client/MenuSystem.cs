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

	private static readonly MenuType[] NotImplementedYet;

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
		_enableMenuUpdated = new DelayedFunction(() =>
		{
			if (EnableMenuUpdated != null)
			{
				EnableMenuUpdated();
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
				EnableMenu(type, false);
			}
		}
		EnableMenu(MenuType.Offerwall, Platform.Instance.IsAvailableOfferwall);
	}

	public static bool IsHiddenMenu(MenuType type)
	{
		for (int i = 0; i < NotImplementedYet.Length; i++)
		{
			if (NotImplementedYet[i] == type)
			{
				return true;
			}
		}
		// [แก้เอง] 31 ส.ค. 2026 — รายชื่อเมนูที่ซ่อน ให้เซิร์ฟสั่งได้ (`hidden_menus` ใน /knock)
		// เดิมฮาร์ดโค้ดไว้ที่ NotImplementedYet ⇒ เปิด/ปิดทีต้อง build client ใหม่
		// แล้วให้ผู้เล่นโหลด 828 MB ใหม่ทุกคน — ตอนนี้แก้ data/mods/config/DurangoClientCore.json
		// แล้ว restart เซิร์ฟพอ (ดู ClientModPolicy.HiddenMenus)
		string[] hiddenFromServer = Durango.Offline.Server.HiddenMenus;
		if (hiddenFromServer != null)
		{
			string typeName = type.ToString();
			for (int j = 0; j < hiddenFromServer.Length; j++)
			{
				if (string.Equals(hiddenFromServer[j], typeName, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
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
		Dictionary<string, byte[]> data = welcome.Storage.Data;
		byte[] array = ((data != null) ? data.Get("RecentlyUnlockedMenuList") : null);
		if (KUtility.GetSize(array) == 0)
		{
			InitRecentlyUnlocked(GameManager.ClusterMode == Mode.Online);
		}
		else if (!LoadRecentlyUnlocked(array))
		{
			InitRecentlyUnlocked(true);
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
			MenuType value;
			if (!item.Key.TryEnum<MenuType>(out value))
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
		SetStorageItem msg = new SetStorageItem
		{
			Key = "RecentlyUnlockedMenuList",
			Value = Json.WriteToBytes(dictionary)
		};
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
		// [แก้เอง] 31 ส.ค. 2026 — เจ้าของสั่ง "เปิดมาทุกเมนูเลย ไม่ต้องซ่อน"
		//
		// เดิมรายการนี้ปิด 23 เมนูของระบบที่ยังทำไม่เสร็จ (ตลาด/แคลน/เมล/สารานุกรม/สัตว์เลี้ยง ฯลฯ)
		// ตอนนี้ว่างเปล่า = ไม่ซ่อนอะไรจากฝั่งเรา ปล่อยให้ตัวกรองของเกมเอง
		// (HiddenInOnline / ShowInOffline ตาม ClusterMode) ทำงานตามปกติ
		//
		// ⚠️ เมนูที่เซิร์ฟยังไม่รองรับจะเปิดมาเป็นหน้าว่างหรือค้าง — เป็นพฤติกรรมที่ตั้งใจตอนนี้
		//    ถ้าจะปิดกลับ ใส่ MenuType ที่ต้องการซ่อนกลับเข้า array นี้
		NotImplementedYet = new MenuType[0];
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
