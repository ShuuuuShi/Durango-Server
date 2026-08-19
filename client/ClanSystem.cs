using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Network;
using Durango.UI;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ClanSystem : GameSystem<ClanSystem>
{
	public static readonly Clan InvalidClan = new Clan();

	private AsyncCachedDictionary<string, Clan> _cachedClanDict;

	private readonly HashSet<string> _simpleInfoRequestSet = new HashSet<string>();

	private EmblemAtlas _emblemAtlas;

	[CanBeNull]
	public Clan PlayerClan { get; private set; }

	public Clan WaitingClan { get; private set; }

	public AllySlot[] Allies { get; private set; }

	public static EmblemAtlas EmblemAtlas
	{
		get
		{
			if (GameSystem<ClanSystem>.HasInstance())
			{
				return GameSystem<ClanSystem>.Instance()._emblemAtlas;
			}
			return null;
		}
	}

	public event Action ClanChanged;

	public event Action ClanInfoUpdated;

	public event Action AlliesUpdated;

	private void Awake()
	{
		Connections.Radiotower.On<ClanRewardsUpdated>(OnDirtyClanReward);
		Connections.Radiotower.On<ClanStatusEffectsUpdated>(OnClanStatusEffectsUpdated);
		Connections.Radiotower.On<ClanInfoUpdated>(OnClanInfoUpdated);
		Connections.Frontend.On<AllySlots>(OnAllySlots);
		Connections.Frontend.On<ClanApplyDropped>(OnClanApplyDropped);
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.InviteToClan, delegate(InteractionObject target)
		{
			PlayerBehavior targetComponent = target.GetTargetComponent<PlayerBehavior>();
			if ((bool)targetComponent)
			{
				Invite(targetComponent);
			}
		});
		Durango.Utils.Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			Durango.Utils.Singleton<PlayerManager>.Instance().PlayerClanChanged += OnChangePlayerClan;
		};
		Durango.Utils.Singleton<GameManager>.Instance().AddOnReady(OnReady);
		_cachedClanDict = new AsyncCachedDictionary<string, Clan>(RequestClanInfo);
		AsyncCachedDictionary<string, Clan> cachedClanDict = _cachedClanDict;
		cachedClanDict.OnPostRequest = (AsyncCachedDictionary<string, Clan>.PostRequestDelegate)Delegate.Combine(cachedClanDict.OnPostRequest, (AsyncCachedDictionary<string, Clan>.PostRequestDelegate)delegate
		{
			_simpleInfoRequestSet.Remove(_cachedClanDict.CurrentKey);
		});
		_emblemAtlas = new EmblemAtlas();
	}

	private void OnReady()
	{
		GetAllySlots();
	}

	public void GetAllySlots()
	{
		if (PlayerBehavior.LocalPlayer.HasClan)
		{
			Connections.Frontend.Send(default(GetAllySlots));
		}
	}

	private void OnAllySlots(AllySlots slots, PacketHeader header)
	{
		Allies = slots.Slots;
		if (this.AlliesUpdated != null)
		{
			this.AlliesUpdated();
		}
	}

	private void OnClanApplyDropped(ClanApplyDropped message, PacketHeader header)
	{
		WaitingClan = null;
		if (message.Expired)
		{
		}
		UIManager.SystemMsg(T._("<em>{0}</em> 부족 가입 신청이 거절되었습니다.", message.ClanName));
		if (this.ClanInfoUpdated != null)
		{
			this.ClanInfoUpdated();
		}
	}

	private void RequestClanInfo(string clanId, Clan cachedInfo, Action<string, Clan> callback)
	{
		bool isDetail = !_simpleInfoRequestSet.Contains(clanId);
		string url = string.Format((!isDetail) ? "{0}/clans/{1}" : "{0}/clans/{1}?detail", GameManager.GatewayUrl, clanId);
		Http.RequestYml(url, delegate(ClanJson json)
		{
			if (string.IsNullOrEmpty(json.id))
			{
				callback(clanId, InvalidClan);
			}
			else
			{
				Clan clan = cachedInfo;
				if (clan == null)
				{
					clan = new Clan();
				}
				clan.Set(json, isDetail);
				if (callback != null)
				{
					callback(clanId, clan);
				}
			}
		});
	}

	private void RequestClanInfo(string clanId, [NotNull] Action<Clan> callback, bool refresh, bool detail)
	{
		if (!refresh && _cachedClanDict.TryGetCachedValue(clanId, out var value) && !value.IsDetail && detail)
		{
			refresh = true;
		}
		if (!detail)
		{
			_simpleInfoRequestSet.Add(clanId);
		}
		_cachedClanDict.Request(clanId, callback, refresh);
	}

	public void RequestClanInfo(string clanName, Action<IList<Clan>> callback)
	{
		if (callback == null)
		{
			return;
		}
		string url = $"{GameManager.GatewayUrl}/clans?keyword={WWW.EscapeURL(clanName)}";
		Http.RequestYml(url, delegate(ClanJsonContainer res)
		{
			int size = KUtility.GetSize(res.clans);
			Clan[] array = new Clan[size];
			for (int i = 0; i < size; i++)
			{
				if (_cachedClanDict.TryGetCachedValue(res.clans[i].id, out var value) && value.IsDetail)
				{
					array[i] = value;
				}
				else
				{
					if (value == null)
					{
						value = new Clan();
					}
					value.Set(res.clans[i], isDetail: false);
					array[i] = value;
					_cachedClanDict.SetValue(res.clans[i].id, value);
				}
			}
			callback(array);
		});
	}

	private void OnChangePlayerClan(PlayerBehavior player)
	{
		if (!player.IsLocalPlayer)
		{
			return;
		}
		string text = ((PlayerClan != null) ? PlayerClan.Id : string.Empty);
		RequestPlayerClan();
		if (!(text != player.ClanId))
		{
			return;
		}
		if (!string.IsNullOrEmpty(player.ClanId))
		{
			GetAllySlots();
		}
		if (this.ClanChanged == null)
		{
			return;
		}
		try
		{
			this.ClanChanged();
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void RequestPlayerClan()
	{
		if (GameManager.ClusterMode != 0)
		{
			OnReceivePlayerClan(null);
		}
		else if (PlayerBehavior.LocalPlayer.HasClan)
		{
			string clanId = PlayerBehavior.LocalPlayer.ClanId;
			_cachedClanDict.Request(clanId, OnReceivePlayerClan, refresh: true);
			EmblemAtlas.Get(clanId, null, refresh: true);
		}
		else if (!string.IsNullOrEmpty(PlayerBehavior.LocalPlayer.Clan.ApplyingClanId))
		{
			string applyingClanId = PlayerBehavior.LocalPlayer.Clan.ApplyingClanId;
			_cachedClanDict.Request(applyingClanId, OnReceiveWaitingClan, refresh: true);
			EmblemAtlas.Get(applyingClanId, null, refresh: false);
		}
		else
		{
			OnReceivePlayerClan(null);
		}
	}

	private void OnReceivePlayerClan(Clan clan)
	{
		PlayerClan = clan;
		WaitingClan = null;
		UpdateLocalplayerMember();
		if (this.ClanInfoUpdated != null)
		{
			this.ClanInfoUpdated();
		}
	}

	private void OnReceiveWaitingClan(Clan clan)
	{
		PlayerClan = null;
		WaitingClan = clan;
		UpdateLocalplayerMember();
		if (this.ClanInfoUpdated != null)
		{
			this.ClanInfoUpdated();
		}
	}

	private void UpdateLocalplayerMember()
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		Messages.Member clan = localPlayer.Clan;
		if (PlayerClan != null)
		{
			Durango.Logic.Clan.Member member = PlayerClan.GetMember(localPlayer.EntityId);
			if (member == null)
			{
				PlayerClan = null;
				clan = default(Messages.Member);
			}
			else
			{
				clan.RoleId = member.RoleId;
			}
		}
		else if (WaitingClan != null)
		{
			Durango.Logic.Clan.Member applier = WaitingClan.GetApplier(localPlayer.EntityId);
			if (applier == null)
			{
				WaitingClan = null;
			}
		}
		else
		{
			clan = default(Messages.Member);
		}
		localPlayer.Clan = clan;
	}

	private void OnDirtyClanReward(ClanRewardsUpdated msg, PacketHeader header)
	{
		RequestClanRewards();
	}

	private void RequestClanRewards()
	{
		Connections.Frontend.Send(default(RequestClanRewards));
	}

	private void OnClanStatusEffectsUpdated(ClanStatusEffectsUpdated msg, PacketHeader header)
	{
		RequestClanStatusEffects();
	}

	private void RequestClanStatusEffects()
	{
		Connections.Frontend.Send(default(RequestClanStatusEffects));
	}

	private void OnClanInfoUpdated(ClanInfoUpdated msg, PacketHeader header)
	{
		RequestPlayerClan();
	}

	public static bool IsMyClan([NotNull] PlayerBehavior other)
	{
		if (PlayerBehavior.LocalPlayer.EntityId == other.EntityId)
		{
			return false;
		}
		return other.HasClan && IsMyClan(other.ClanId);
	}

	public static bool IsMyClan(string clanId)
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		return localPlayer.HasClan && localPlayer.ClanId == clanId;
	}

	public static bool IsMyClanOrAlliance([NotNull] PlayerBehavior other)
	{
		if (PlayerBehavior.LocalPlayer.EntityId == other.EntityId)
		{
			return false;
		}
		return other.HasClan && IsMyClanOrAlliance(other.ClanId);
	}

	public static bool IsMyClanOrAlliance(string clanId)
	{
		return IsMyClan(clanId) || GameSystem<ClanSystem>.Instance().IsAlliedClan(clanId);
	}

	public bool IsAlliedClan(string clanId)
	{
		int i = 0;
		for (int size = KUtility.GetSize(Allies); i < size; i++)
		{
			AllySlot allySlot = Allies[i];
			if (allySlot.ClanId == clanId)
			{
				return allySlot.IsAlly;
			}
		}
		return false;
	}

	public static void GetExpRange(int level, out long min, out long max)
	{
		min = 0L;
		max = 0L;
		if (Yaml.Util.Singleton<ClanYaml>.Instance == null)
		{
			return;
		}
		long[] levelThresholds = Yaml.Util.Singleton<ClanYaml>.Instance.LevelThresholds;
		if (levelThresholds != null)
		{
			if (level - 2 >= 0 && level - 2 < levelThresholds.Length)
			{
				min = levelThresholds[level - 2];
			}
			if (level - 1 >= 0 && level - 1 < levelThresholds.Length)
			{
				max = levelThresholds[level - 1];
			}
		}
	}

	public static void GetEmblem(string clanid, [NotNull] Action<Point2> onResult, bool refresh = false)
	{
		EmblemAtlas?.Get(clanid, onResult, refresh);
	}

	public static void RefreshPlayerClan()
	{
		GameSystem<ClanSystem>.Instance().RequestPlayerClan();
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

	public static void SetClanNotice(string notice, Action<bool> onResult)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			onResult?.Invoke(obj: false);
		}
		else
		{
			SetClanComment(notice, playerClan.Intro, onResult);
		}
	}

	public static void SetClanIntro(string intro, Action<bool> onResult)
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			onResult?.Invoke(obj: false);
		}
		else
		{
			SetClanComment(playerClan.Notice, intro, onResult);
		}
	}

	public static void SetClanComment(string notice, string intro, Action<bool> onResult)
	{
		Connections.Frontend.Send(new SetClanInfo
		{
			Intro = intro,
			Notice = notice
		}).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (flag)
			{
				RefreshPlayerClan();
			}
			if (onResult != null)
			{
				onResult(flag);
			}
		});
	}

	public static void RenameClan(string clanName)
	{
		Connections.Frontend.Send(new RenameClan
		{
			ClanName = clanName
		}).All(delegate(Packet packet)
		{
			if (Packet.IsSuccess(packet))
			{
				RefreshPlayerClan();
			}
		});
	}

	public static void GetClanInfo(string clanId, [NotNull] Action<Clan> callback, bool refresh = false, bool detail = true)
	{
		if (GameManager.ClusterMode != 0)
		{
			callback(null);
		}
		else
		{
			GameSystem<ClanSystem>.Instance().RequestClanInfo(clanId, callback, refresh, detail);
		}
	}

	private static void HandleResult(Packet packet, Action<bool> onResult)
	{
		onResult?.Invoke(packet.Header.TypeCode == 1231);
	}

	public static void JoinClan(Clan clan, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new JoinClan
		{
			ClanId = clan.Id
		}).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("{0} 부족에 가입 신청했습니다.", clan.Name));
		}).All(delegate(Packet packet)
		{
			HandleResult(packet, onResult);
		});
	}

	public static void ApproveApplier(string entityId)
	{
		Connections.Frontend.Send(new ApproveClanApplier
		{
			EntityId = entityId
		}).All(delegate
		{
			RefreshPlayerClan();
		});
	}

	public static void DropApplier(string entityId)
	{
		Connections.Frontend.Send(new DropClanApplier
		{
			EntityId = entityId
		}).All(delegate
		{
			RefreshPlayerClan();
		});
	}

	public static void SetClanEmblem(byte[] emblem, Action onSuccess)
	{
		Connections.Frontend.Send(new SetClanEmblem
		{
			Emblem = emblem
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
			if (onSuccess != null)
			{
				onSuccess();
			}
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
			UIManager.Alarm.ShowNotify(T._("<em>{0}</em> 부족을 만들었습니다.", clanName), "icon_clan_createclan", major: true);
		}).All(delegate(Packet packet)
		{
			HandleResult(packet, onResult);
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
		}).All(delegate(Packet packet)
		{
			HandleResult(packet, onResult);
		});
	}

	public static void KickMember(string entityId, Action<bool> onResult = null)
	{
		Connections.Frontend.Send(new KickClanMember
		{
			EntityId = entityId
		}).On<OK>(delegate
		{
			RefreshPlayerClan();
		}).All(delegate(Packet packet)
		{
			HandleResult(packet, onResult);
		});
	}

	private static void Invite(PlayerBehavior player)
	{
		Connections.Frontend.Send(new InviteToClan
		{
			EntityId = player.EntityId
		}).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("{0}님을 부족에 초대했습니다.", player.PlayerName));
		});
	}

	public static void SetMemberRoleGrades(IList<int> order, Action<bool> onResult)
	{
		if (KUtility.GetSize(order) == 0)
		{
			return;
		}
		SetMemberRoleGrades setMemberRoleGrades = default(SetMemberRoleGrades);
		setMemberRoleGrades.RoleOrders = new RoleOrder[order.Count];
		SetMemberRoleGrades msg = setMemberRoleGrades;
		for (int i = 0; i < order.Count; i++)
		{
			ref RoleOrder reference = ref msg.RoleOrders[i];
			reference = new RoleOrder
			{
				RoleId = order[i],
				Grade = i
			};
		}
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (onResult != null)
			{
				onResult(flag);
			}
			if (flag)
			{
				RefreshPlayerClan();
			}
		});
	}

	public static void SetMemberRoleInfo(MemberRole role, Action<bool> onResult)
	{
		SetMemberRoleInfo setMemberRoleInfo = default(SetMemberRoleInfo);
		setMemberRoleInfo.RoleId = role.Id;
		setMemberRoleInfo.Info = role;
		SetMemberRoleInfo msg = setMemberRoleInfo;
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (onResult != null)
			{
				onResult(flag);
			}
			if (flag)
			{
				RefreshPlayerClan();
			}
		});
	}

	public static void RemoveMemberRole(int roleId, int moveToId, Action<bool> onResult)
	{
		RemoveMemberRole removeMemberRole = default(RemoveMemberRole);
		removeMemberRole.RoleId = roleId;
		removeMemberRole.MoveToRoleId = moveToId;
		RemoveMemberRole msg = removeMemberRole;
		Connections.Frontend.Send(msg).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (onResult != null)
			{
				onResult(flag);
			}
			if (flag)
			{
				RefreshPlayerClan();
			}
		});
	}

	public static void SetMemberRole(Durango.Logic.Clan.Member member, int roleId, Action onSuccess = null)
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

	public static void SuggestAlly(string clanId)
	{
		Connections.Frontend.Send(new SuggestAlly
		{
			ClanId = clanId
		});
	}

	public static void SuggestBreak(string clanId)
	{
		Connections.Frontend.Send(new SuggestBreak
		{
			ClanId = clanId
		});
	}

	public static void AcceptSuggestion(string clanId)
	{
		Connections.Frontend.Send(new AcceptSuggestion
		{
			ClanId = clanId
		});
	}

	public static void RefuseSuggestion(string clanId)
	{
		Connections.Frontend.Send(new RefuseSuggestion
		{
			ClanId = clanId
		});
	}

	public static void BreakAlly(string clanId)
	{
		Connections.Frontend.Send(new BreakAlly
		{
			ClanId = clanId
		});
	}

	public static void CancelWaitingClan(string clanId)
	{
		Connections.Frontend.Send(new CancelClanJoinRequest
		{
			ClanId = clanId
		}).On<OK>(delegate
		{
			UIManager.SystemMsg(T._("부족 가입 요청이 취소되었습니다."));
		});
	}
}
