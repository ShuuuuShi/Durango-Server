using System;
using System.Collections.Generic;
using ClanData;
using JetBrains.Annotations;
using K1Network;
using L10N;
using MapData;
using Messages;
using Shared.Economy;
using Shared.System;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ClanSystem : GameSystem<ClanSystem>
{
	private EnemyClan[] _enemyClans;

	private List<ClanTerritory> _clanTerritories = new List<ClanTerritory>();

	private AsyncCachedDictionary<ulong, Clan> _cachedClanDict;

	public Clan PlayerClan { get; private set; }

	public ulong[] EnemyClanIds { get; private set; }

	public event Action<ulong, ulong> ClanChanged;

	public event Action ClanInfoUpdated;

	public event Action EnemyClansDirtied;

	private void Awake()
	{
		Connections.Radiotower.On<ClanRewardsUpdated>(OnDirtyClanReward);
		Connections.Radiotower.On<WarStateUpdated>(OnDirtyClanWarStates);
		Connections.Radiotower.On<TerritoryUpdated>(OnDirtyClanTerritories);
		Connections.Radiotower.On<ClanStatusEffectsUpdated>(OnClanStatusEffectsUpdated);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.InviteToClan, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent = target.GetTargetComponent<PlayerBehavior>();
			if (Object.op_Implicit((Object)(object)targetComponent))
			{
				Invite(targetComponent);
			}
		});
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			KSingleton<PlayerManager>.Instance().PlayerClanChanged += OnChangePlayerClan;
		};
		KSingleton<GameManager>.Instance().AddOnReady(RequestClanTerritories);
		KSingleton<GameManager>.Instance().AddOnReady(RequestClanWarState);
		_cachedClanDict = new AsyncCachedDictionary<ulong, Clan>(RequestClanInfo);
		AsyncCachedDictionary<ulong, Clan> cachedClanDict = _cachedClanDict;
		cachedClanDict.OnPostRequest = (AsyncCachedDictionary<ulong, Clan>.PostRequestDelegate)Delegate.Combine(cachedClanDict.OnPostRequest, new AsyncCachedDictionary<ulong, Clan>.PostRequestDelegate(OnPostClanRequest));
	}

	private void OnPostClanRequest(ref Clan clan)
	{
		if (clan == null)
		{
			return;
		}
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_enemyClans); i < size; i++)
		{
			if (_enemyClans[i].ClanId == clan.Id)
			{
				num = i;
				break;
			}
		}
		clan.DeclareWarTime = ((num != -1) ? _enemyClans[num].DeclareWarTime : 0.0);
	}

	private void RequestClanInfo(ulong clanId, Clan cachedInfo, Action<ulong, Clan> callback)
	{
		string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}clans/{clanId}";
		KUtility.RequestYml(url, delegate(ClanJson json)
		{
			Clan clan = cachedInfo;
			if (clan == null)
			{
				clan = new Clan(json);
			}
			else
			{
				clan.Set(json);
			}
			if (callback != null)
			{
				callback(clanId, clan);
			}
		});
	}

	private void OnChangePlayerClan(PlayerBehavior player)
	{
		if (!player.IsLocalPlayer)
		{
			return;
		}
		ulong num = ((PlayerClan != null) ? PlayerClan.Id : 0);
		RequestPlayerClan();
		if (num == player.ClanId || this.ClanChanged == null)
		{
			return;
		}
		try
		{
			this.ClanChanged(num, player.ClanId);
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	private void RequestPlayerClan()
	{
		ulong clanId = PlayerBehavior.LocalPlayer.ClanId;
		if (clanId == 0L)
		{
			OnReceivePlayerClan(null);
		}
		else
		{
			_cachedClanDict.Request(clanId, OnReceivePlayerClan, refresh: true);
		}
	}

	private void OnReceivePlayerClan(Clan clan)
	{
		PlayerClan = clan;
		if (this.ClanInfoUpdated != null)
		{
			this.ClanInfoUpdated();
		}
	}

	private void OnDirtyClanReward(ClanRewardsUpdated msg, PacketHeader header)
	{
		RequestClanRewards();
	}

	private void RequestClanRewards()
	{
		Connections.Frontend.Send(default(RequestClanRewards));
	}

	private void OnDirtyClanWarStates(WarStateUpdated msg, PacketHeader header)
	{
		RequestClanWarState();
	}

	private void RequestClanWarState()
	{
		Connections.Frontend.Send(default(GetClanWarState)).On<Messages.ClanWarState>(OnClanWarState);
	}

	private void OnClanWarState(Messages.ClanWarState msg, PacketHeader header)
	{
		_enemyClans = msg.EnemyClans;
		int size = KUtility.GetSize(_enemyClans);
		if (size > 0)
		{
			EnemyClanIds = new ulong[size];
			for (int i = 0; i < EnemyClanIds.Length; i++)
			{
				EnemyClanIds[i] = _enemyClans[i].ClanId;
			}
			_cachedClanDict.Request(EnemyClanIds, OnEnemyClans, refresh: true);
		}
		else
		{
			EnemyClanIds = null;
			if (this.EnemyClansDirtied != null)
			{
				this.EnemyClansDirtied();
			}
		}
	}

	private void OnEnemyClans(IList<Clan> enemies)
	{
		if (this.EnemyClansDirtied != null)
		{
			this.EnemyClansDirtied();
		}
	}

	private void OnDirtyClanTerritories(TerritoryUpdated msg, PacketHeader header)
	{
		RequestClanTerritories();
	}

	private void RequestClanTerritories()
	{
		string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}regions/{KSingleton<GameManager>.Instance().Region.Id}/clan_estates";
		KUtility.RequestYml<Dictionary<ulong, int[][]>>(url, OnReceiveClanTerritories, disableCache: true);
	}

	private void OnReceiveClanTerritories(Dictionary<ulong, int[][]> territories)
	{
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		_clanTerritories.Clear();
		MapIndicators.Remove(IndicatorType.ClanFlag);
		MapIndicators.Remove(IndicatorType.ClanEstate);
		if (territories == null)
		{
			return;
		}
		foreach (KeyValuePair<ulong, int[][]> territory in territories)
		{
			int[][] value = territory.Value;
			int i = 0;
			for (int size = KUtility.GetSize(value); i < size; i++)
			{
				Point2 point = new Point2(value[i][0], value[i][1]) / 2;
				bool flag = false;
				int j = 0;
				for (int size2 = KUtility.GetSize(_clanTerritories); j < size2; j++)
				{
					Point2 grid = _clanTerritories[j].Grid;
					if (grid == point)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					_clanTerritories.Add(new ClanTerritory
					{
						ClanId = territory.Key,
						Grid = point
					});
				}
			}
		}
		List<ClanTerritory> list = new List<ClanTerritory>(_clanTerritories);
		List<ClanTerritory> list2 = new List<ClanTerritory>();
		while (list.Count > 0)
		{
			bool flag2 = true;
			for (int k = 0; k < list.Count; k++)
			{
				if (list2.Count == 0)
				{
					list2.Add(list[k]);
					list.RemoveAt(k);
					k--;
					continue;
				}
				ClanTerritory clanTerritory = list[k];
				Point2 grid2 = clanTerritory.Grid;
				for (int l = 0; l < list2.Count; l++)
				{
					if (list2[l].ClanId == clanTerritory.ClanId)
					{
						Point2 grid3 = list2[l].Grid;
						int num = Mathf.Abs(grid2.x - grid3.x) + Mathf.Abs(grid2.y - grid3.y);
						if (num <= 1)
						{
							list2.Add(list[k]);
							list.RemoveAt(k);
							k--;
							flag2 = false;
							break;
						}
					}
				}
			}
			if (list2.Count != 0 && (flag2 || list.Count <= 0))
			{
				ClanTerritory clan = list2[0];
				Color color;
				Color color2;
				if (PlayerClan != null && PlayerClan.Id == clan.ClanId)
				{
					color = Color32.op_Implicit(new Color32((byte)171, (byte)232, (byte)56, byte.MaxValue));
					color2 = Color32.op_Implicit(new Color32((byte)53, (byte)151, (byte)18, byte.MaxValue));
				}
				else if (IsEnemyClan(clan.ClanId))
				{
					color = Color32.op_Implicit(new Color32((byte)241, (byte)51, (byte)51, byte.MaxValue));
					color2 = Color32.op_Implicit(new Color32((byte)172, (byte)48, (byte)48, byte.MaxValue));
				}
				else
				{
					color = Color32.op_Implicit(new Color32((byte)226, (byte)224, (byte)216, byte.MaxValue));
					color2 = Color32.op_Implicit(new Color32((byte)165, (byte)162, (byte)151, byte.MaxValue));
				}
				MapFlagIndicator mapFlagIndicator = MapIndicators.Add<MapFlagIndicator>(clan.Grid, IndicatorType.ClanFlag);
				mapFlagIndicator.SetOwnerClan(clan, color);
				for (int m = 0; m < list2.Count; m++)
				{
					MapEstateIndicator mapEstateIndicator = MapIndicators.Add<MapEstateIndicator>(list2[m].Grid, IndicatorType.ClanEstate);
					mapEstateIndicator.Set(list2[m].Grid, 8, color2);
				}
				list2.Clear();
			}
		}
	}

	private void OnClanStatusEffectsUpdated(ClanStatusEffectsUpdated msg, PacketHeader header)
	{
		RequestClanStatusEffects();
	}

	private void RequestClanStatusEffects()
	{
		Connections.Frontend.Send(default(RequestClanStatusEffects));
	}

	public bool IsEnemyClan(ulong clanId)
	{
		ClanData.ClanWarState clanWarState = GetClanWarState(clanId);
		return clanWarState == ClanData.ClanWarState.WarmUp || clanWarState == ClanData.ClanWarState.Match;
	}

	public void GetClanWarState(ulong clanId, out ClanData.ClanWarState state, out double remain)
	{
		Clan cachedValue = _cachedClanDict.GetCachedValue(clanId);
		if (cachedValue == null)
		{
			state = ClanData.ClanWarState.None;
			remain = 0.0;
		}
		else
		{
			cachedValue.GetClanWarState(out state, out remain);
		}
	}

	public ClanData.ClanWarState GetClanWarState(ulong clanId)
	{
		GetClanWarState(clanId, out var state, out var _);
		return state;
	}

	public void GetExpRange(int level, out long min, out long max)
	{
		min = 0L;
		max = 0L;
		if (Singleton<ClanYaml>.Instance == null)
		{
			return;
		}
		long[] level_thresholds = Singleton<ClanYaml>.Instance.level_thresholds;
		if (level_thresholds != null)
		{
			if (level - 2 >= 0 && level - 2 < level_thresholds.Length)
			{
				min = level_thresholds[level - 2];
			}
			if (level - 1 >= 0 && level - 1 < level_thresholds.Length)
			{
				max = level_thresholds[level - 1];
			}
		}
	}

	public static bool HasClanEstate()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			return false;
		}
		List<ClanTerritory> clanTerritories = GameSystem<ClanSystem>.Instance()._clanTerritories;
		int i = 0;
		for (int size = KUtility.GetSize(clanTerritories); i < size; i++)
		{
			if (clanTerritories[i].ClanId == playerClan.Id)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsClanExtensibleTile(Point2 tile)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			return false;
		}
		Point2 point = tile;
		point.x /= 8;
		point.y /= 8;
		List<ClanTerritory> clanTerritories = GameSystem<ClanSystem>.Instance()._clanTerritories;
		int i = 0;
		for (int size = KUtility.GetSize(clanTerritories); i < size; i++)
		{
			ClanTerritory clanTerritory = clanTerritories[i];
			if (clanTerritory.ClanId == playerClan.Id)
			{
				Point2 grid = clanTerritory.Grid;
				int num = Mathf.Abs(point.x - grid.x) + Mathf.Abs(point.y - grid.y);
				if (num == 1)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void RefreshPlayerClan()
	{
		GameSystem<ClanSystem>.Instance().RequestPlayerClan();
	}

	public static void GetClanTerritoryCosts([NotNull] Action<Costs> onCosts)
	{
		Connections.Frontend.Send(default(GetClanTerritoryCosts)).On(delegate(Costs costs, PacketHeader header)
		{
			onCosts(costs);
		});
	}

	public static void GetClanWarCosts([NotNull] Action<Costs> onCosts)
	{
		Connections.Frontend.Send(default(GetWarCosts)).On(delegate(Costs costs, PacketHeader header)
		{
			onCosts(costs);
		});
	}

	public static void GetClanFund([NotNull] Action<Costs> onResult)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			Connections.Frontend.Send(default(GetClanFund)).On(delegate(Costs costs, PacketHeader _)
			{
				onResult(costs);
			});
		}
	}

	public static void SetClanNotice(string notice)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			SetClanComment(notice, playerClan.Intro);
		}
	}

	public static void SetClanIntro(string intro)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan != null)
		{
			SetClanComment(playerClan.Notice, intro);
		}
	}

	public static void SetClanComment(string notice, string intro)
	{
		Connections.Frontend.Send(new SetClanInfo
		{
			Intro = intro,
			Notice = notice
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
		});
	}

	public static void GetClanInfo(Clan clan, [NotNull] Action<Clan> callback, bool refresh = false)
	{
		if (clan == null)
		{
			callback(null);
		}
		else
		{
			GetClanInfo(clan.Id, callback, refresh);
		}
	}

	public static void GetClanInfo(ulong clanId, [NotNull] Action<Clan> callback, bool refresh = false)
	{
		AsyncCachedDictionary<ulong, Clan> cachedClanDict = GameSystem<ClanSystem>.Instance()._cachedClanDict;
		cachedClanDict.Request(clanId, callback, refresh);
	}

	public static void JoinClan(Clan clan, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new JoinClan
		{
			ClanId = clan.Id
		}).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("<{0}> 부족에 가입 신청하였습니다.", clan.Name));
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void ApproveApplier(ulong entityId, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new ApproveClanApplier
		{
			EntityId = entityId
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void DropApplier(ulong entityId, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new DropClanApplier
		{
			EntityId = entityId
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void SetClanEmblem(byte[] emblem)
	{
		Connections.Frontend.Send(new SetClanEmblem
		{
			Emblem = emblem
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
		});
	}

	public static void MakeClan(Currency currency, string clanName, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new MakeClan
		{
			Currency = currency,
			ClanName = clanName
		}).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("<em>{0}</em> 부족을 생성했습니다", clanName));
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void GetClanMakeCost([NotNull] Action<Costs> onResult)
	{
		Connections.Frontend.Send(default(GetClanCreationCosts)).On(delegate(Costs cost, PacketHeader _)
		{
			onResult(cost);
		});
	}

	public static void LeaveClan(Action<bool> onResult = null)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			if (onResult != null)
			{
				onResult(obj: false);
			}
			return;
		}
		Connections.Frontend.Send(new LeaveClan
		{
			ClanId = playerClan.Id
		}).On<OK>(delegate
		{
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	public static void KickMember(ulong entityId, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new KickClanMember
		{
			EntityId = entityId
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
			if (onResult != null)
			{
				onResult(obj: true);
			}
		}).On(delegate(Error msg, PacketHeader header)
		{
			GameManager.DefaultErrorHandler(msg, header);
			if (onResult != null)
			{
				onResult(obj: false);
			}
		});
	}

	private static void Invite(PlayerBehavior player)
	{
		Connections.Frontend.Send(new InviteToClan
		{
			EntityId = player.EntityId
		}).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("{0}님을 부족에 초대하였습니다.", player.PlayerName));
		});
	}

	public static void SubmitRoleInfos(Dictionary<int, MemberRole> roles)
	{
		Connections.Frontend.Send(new SetMemberRoleInfos
		{
			Infos = roles
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
		});
	}

	public static void SetMemberRole(ClanData.Member member, int roleId, Action onSuccess = null)
	{
		Connections.Frontend.Send(new SetClanMemberRole
		{
			TargetId = member.EntityId,
			RoleId = roleId
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
			if (onSuccess != null)
			{
				onSuccess();
			}
		});
	}

	public static void RequestClanInfo(string clanName, Action<IList<Clan>> callback)
	{
		if (callback == null)
		{
			return;
		}
		string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}clans?keyword={clanName}";
		KUtility.RequestYml(url, delegate(ClanJsonConatiner res)
		{
			int size = KUtility.GetSize(res.clans);
			Clan[] array = new Clan[size];
			for (int i = 0; i < size; i++)
			{
				array[i] = new Clan(res.clans[i]);
			}
			callback(array);
		});
	}
}
