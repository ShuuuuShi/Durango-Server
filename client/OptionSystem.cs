using System;
using System.Collections.Generic;
using Durango.Logic;
using Durango.Network;
using Durango.Utils;
using Messages;

public class OptionSystem : GameSystem<OptionSystem>
{
	public const string ShopEnabledKey = "cashshop.ui_enabled";

	public const string CoinTransferEnabledKey = "cashshop.coin_transfer_enabled";

	public const string WebEventEnabeldKey = "quest.web_ui_enabled";

	public const string WarpRushShutdownKey = "shutdown.s02_warp_rush";

	public const string EngagementShutdownKey = "shutdown.engagement.disable";

	public const string S02WaitingQueueMin = "season2.waiting_queue_entree_min";

	public const string MarketUIEnableKey = "market.ui_enabled";

	public const string MarketSearchLimitKey = "market.search.limit";

	private readonly ObservableOptions<long> _serverLong = new ObservableOptions<long>();

	private readonly ObservableOptions<double> _serverDouble = new ObservableOptions<double>();

	private readonly ObservableOptions<bool> _serverBool = new ObservableOptions<bool>();

	private readonly Dictionary<string, MenuType> _menuBindings = new Dictionary<string, MenuType> { 
	{
		"party.ui_enabled",
		MenuType.Party
	} };

	private void Awake()
	{
		Singleton<GameManager>.Instance().WelcomeReceived += delegate(Welcome welcome)
		{
			Options options = welcome.Options;
			int i = 0;
			for (int size = KUtility.GetSize(options.Int); i < size; i++)
			{
				SetValue(options.Int[i]);
			}
			int j = 0;
			for (int size2 = KUtility.GetSize(options.Float); j < size2; j++)
			{
				SetValue(options.Float[j]);
			}
			int k = 0;
			for (int size3 = KUtility.GetSize(options.Bool); k < size3; k++)
			{
				SetValue(options.Bool[k]);
			}
			OnOptionLoaded();
		};
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			foreach (KeyValuePair<string, MenuType> menuBinding in _menuBindings)
			{
				GameSystem<MenuSystem>.Instance().EnableMenu(menuBinding.Value, GetBool(menuBinding.Key));
			}
		};
		Connections.Frontend.On(delegate(IntegerOption msg, PacketHeader header)
		{
			SetValue(msg);
		});
		Connections.Frontend.On(delegate(FloatOption msg, PacketHeader header)
		{
			SetValue(msg);
		});
		Connections.Frontend.On(delegate(BoolOption msg, PacketHeader header)
		{
			SetValue(msg);
		});
	}

	private void OnOptionLoaded()
	{
		foreach (KeyValuePair<string, MenuType> menuBinding in _menuBindings)
		{
			KeyValuePair<string, MenuType> pair = menuBinding;
			Action<bool> onChange = delegate(bool on)
			{
				GameSystem<MenuSystem>.Instance().EnableMenu(pair.Value, on);
			};
			AddOnChange(menuBinding.Key, onChange);
		}
	}

	private void SetValue(IntegerOption op)
	{
		_serverLong.Set(op.Key, op.Value);
	}

	private void SetValue(BoolOption op)
	{
		_serverBool.Set(op.Key, op.Value);
	}

	private void SetValue(FloatOption op)
	{
		_serverDouble.Set(op.Key, op.Value);
	}

	public static long GetLong(string key, long defaultValue = 0L)
	{
		return GameSystem<OptionSystem>.Instance()._serverLong.Get(key, defaultValue);
	}

	public static double GetDouble(string key, double defaultValue = 0.0)
	{
		return GameSystem<OptionSystem>.Instance()._serverDouble.Get(key, defaultValue);
	}

	public static bool GetBool(string key, bool defaultValue = false)
	{
		return GameSystem<OptionSystem>.Instance()._serverBool.Get(key, defaultValue);
	}

	public void AddOnChange(string key, Action<long> onChange)
	{
		_serverLong.AddOnChange(key, onChange);
	}

	public void AddOnChange(string key, Action<double> onChange)
	{
		_serverDouble.AddOnChange(key, onChange);
	}

	public void AddOnChange(string key, Action<bool> onChange)
	{
		_serverBool.AddOnChange(key, onChange);
	}

	public static bool IsTestCommoditiesOpened()
	{
		return GetBool("cashshop.open_test_commodities");
	}

	public static bool IsShopEnabled()
	{
		return GetBool("cashshop.ui_enabled");
	}

	public static bool IsWebEventEnabled()
	{
		return GetBool("quest.web_ui_enabled");
	}

	public static bool IsWarpRushShutdown()
	{
		return GetBool("shutdown.s02_warp_rush");
	}

	public static bool IsMarketEnabled()
	{
		return GetBool("market.ui_enabled");
	}

	public static bool IsShutdownTechSupport()
	{
		return GetBool("shutdown.tech_support.request_tech_support.disable");
	}

	public static bool IsShutdownTechSupportEstimate()
	{
		return GetBool("shutdown.tech_support.request_estimate.disable");
	}

	public static bool IsShutdownResetReformSlot()
	{
		return GetBool("shutdown.tech_support.reset_reform_slot.disable");
	}

	public static bool IsShutdownPersonalRegionsChannel()
	{
		return GetBool("shutdown.personal_regions.join_channel");
	}

	public static bool IsShutdownEngagement()
	{
		return GetBool("shutdown.engagement.disable");
	}

	public static bool IsWarpRushRankingEnabled()
	{
		return GetBool("ranking.feature.season2.enabled");
	}

	public static int GetS02WaitingQueueMin()
	{
		return (int)GetLong("season2.waiting_queue_entree_min", 0L);
	}

	public static double GetTimezoneOffset()
	{
		return GetDouble("time.tz_offset");
	}

	public static int GetMarketSearchLimit()
	{
		return (int)GetLong("market.search.limit", 50L);
	}

	public static long GetClanBattleCycleRepeat()
	{
		return GetLong("clan_battle.repeat_cycle", 0L);
	}

	public static bool GetBattlePvPEnabled()
	{
		return GetBool("battle.pvp_enabled");
	}

	public static int GetAllySuggestionCoolTime()
	{
		return (int)GetLong("ally.suggestion_cooltime", 0L);
	}

	public static int GetAllySuggestionExpireTime()
	{
		return (int)GetLong("ally.suggestion_expire_time", 0L);
	}

	public static int GetAllyLockedAfterBreak()
	{
		return (int)GetLong("ally.locked_after_break", 0L);
	}

	public static int GetInventoryAccessRefreshPeriod()
	{
		return (int)GetLong("inventory_access_refresh_period", 0L);
	}

	public static int GetWarpRushEntryCount()
	{
		return (int)GetLong("season2.entree_limit", 0L);
	}
}
