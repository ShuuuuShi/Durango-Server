using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Party;
using Durango.Network;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;

namespace Durango.Logic;

public class PartySystem : GameSystem<PartySystem>
{
	private const string ShowPartyHudKey = "show_party_hud";

	[NotNull]
	private readonly List<Durango.Logic.Party.Member> _partyMembers = new List<Durango.Logic.Party.Member>();

	public string LeaderName { get; private set; }

	public string LeaderEntityId { get; private set; }

	public bool IsInvited { get; private set; }

	public bool IsLeader => PlayerBehavior.LocalPlayer.EntityId == LeaderEntityId;

	public bool NotInParty => MemberCount == 0;

	public bool IsAcceptedInParty { get; private set; }

	public bool ShowPartyHud
	{
		get
		{
			return Preferences.GetBool("show_party_hud", defaultValue: true);
		}
		set
		{
			Preferences.SetBool("show_party_hud", value);
			if (this.ShowPartyHudChanged != null)
			{
				this.ShowPartyHudChanged();
			}
		}
	}

	public int MemberCount => _partyMembers.Count;

	public event Action Invited;

	public event Action MembersUpdated;

	public event Action LeaderChanged;

	public event Action PartierStatusUpdated;

	public event Action ShowPartyHudChanged;

	public Durango.Logic.Party.Member GetMember(int index)
	{
		return _partyMembers[index];
	}

	public int GetMemberIndex([NotNull] Durango.Logic.Party.Member member)
	{
		return _partyMembers.IndexOf(member);
	}

	[NotNull]
	public IEnumerable<Durango.Logic.Party.Member> FindMembersInRegion([NotNull] string regionId)
	{
		return _partyMembers.Where((Durango.Logic.Party.Member member) => member.EntityId != PlayerBehavior.LocalPlayer.EntityId && member.RegionId == regionId);
	}

	private void Start()
	{
		Connections.Frontend.On<Messages.Party>(OnParty);
		Connections.Radiotower.On<PartierStatus>(OnPartierStatus);
		Singleton<GameManager>.Instance().AddOnReady(OnReady);
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			Singleton<PlayerManager>.Instance().PlayerAppeared += PlayerManager_PlayerAppeared;
			Singleton<PlayerManager>.Instance().PlayerDisappeared += PlayerManager_PlayerDisappeared;
		};
	}

	private void PlayerManager_PlayerAppeared(PlayerBehavior player)
	{
		foreach (Durango.Logic.Party.Member partyMember in _partyMembers)
		{
			if (partyMember.EntityId == player.EntityId)
			{
				partyMember.SetPlayer(player);
				break;
			}
		}
	}

	private void PlayerManager_PlayerDisappeared(PlayerBehavior player)
	{
		foreach (Durango.Logic.Party.Member partyMember in _partyMembers)
		{
			if (partyMember.EntityId == player.EntityId)
			{
				partyMember.SetPlayer(null);
				break;
			}
		}
	}

	private void OnParty(Messages.Party msg, PacketHeader header)
	{
		Durango.Player.PlayerInfo leaderInfo = GetLeaderInfo();
		_partyMembers.Clear();
		PartyInfo? info = msg.Info;
		if (!info.HasValue || msg.Info.Value.MemberStatus == null)
		{
			LeaderEntityId = string.Empty;
			LeaderName = string.Empty;
			IsInvited = false;
			IsAcceptedInParty = false;
			if (this.MembersUpdated != null)
			{
				this.MembersUpdated();
			}
			if (this.LeaderChanged != null && leaderInfo != null)
			{
				this.LeaderChanged();
			}
			return;
		}
		PartyInfo value = msg.Info.Value;
		LeaderEntityId = value.LeaderStatus.EntityId;
		LeaderName = value.LeaderRadioId.Name;
		Durango.Logic.Party.Member member = CreateMember(value.LeaderStatus, isLeader: true, isAccepted: true);
		_partyMembers.Add(member);
		bool flag = false;
		Pair<PartierStatus, bool>[] memberStatus = value.MemberStatus;
		for (int i = 0; i < memberStatus.Length; i++)
		{
			Pair<PartierStatus, bool> pair = memberStatus[i];
			PartierStatus item = pair.Item1;
			Durango.Logic.Party.Member item2 = CreateMember(item, isLeader: false, pair.Item2);
			_partyMembers.Add(item2);
			flag |= !pair.Item2 && pair.Item1.EntityId == PlayerBehavior.LocalPlayer.EntityId;
		}
		bool isInvited = IsInvited;
		IsInvited = flag;
		IsAcceptedInParty = IsAcceptedMember(PlayerBehavior.LocalPlayer.EntityId);
		if (this.MembersUpdated != null)
		{
			this.MembersUpdated();
		}
		if (this.LeaderChanged != null && (leaderInfo == null || member.EntityId != leaderInfo.EntityId))
		{
			this.LeaderChanged();
		}
		if (isInvited != flag && flag && this.Invited != null)
		{
			this.Invited();
		}
	}

	[CanBeNull]
	public Durango.Player.PlayerInfo GetLeaderInfo()
	{
		return _partyMembers.FirstOrDefault((Durango.Logic.Party.Member member) => member.IsLeader)?.PlayerInfo;
	}

	private static Durango.Logic.Party.Member CreateMember(PartierStatus status, bool isLeader, bool isAccepted)
	{
		Durango.Logic.Party.Member member = new Durango.Logic.Party.Member(status.EntityId, isLeader, isAccepted);
		member.SetStatus(status);
		PlayerBehavior playerIncludeLocalPlayer = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(status.EntityId);
		member.SetPlayer(playerIncludeLocalPlayer);
		return member;
	}

	private void OnPartierStatus(PartierStatus msg, PacketHeader header)
	{
		foreach (Durango.Logic.Party.Member partyMember in _partyMembers)
		{
			if (partyMember.EntityId == msg.EntityId)
			{
				partyMember.SetStatus(msg);
				break;
			}
		}
		if (this.PartierStatusUpdated != null)
		{
			this.PartierStatusUpdated();
		}
	}

	private void OnReady()
	{
		GetParty();
	}

	public void GetParty()
	{
		Connections.Frontend.Send(default(GetParty));
	}

	public bool IsInParty(string entityId)
	{
		return IsAcceptedInParty && IsAcceptedMember(entityId);
	}

	private bool IsAcceptedMember(string entityId)
	{
		int i = 0;
		for (int count = _partyMembers.Count; i < count; i++)
		{
			Durango.Logic.Party.Member member = _partyMembers[i];
			if (member.EntityId == entityId && member.IsAccepted)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanInvite(string entityId)
	{
		if (IsLeader || NotInParty)
		{
			foreach (Durango.Logic.Party.Member partyMember in _partyMembers)
			{
				if (partyMember.EntityId == entityId)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void MakeParty()
	{
		Connections.Frontend.Send(default(MakeParty));
	}

	public void JoinIntoParty()
	{
		Connections.Frontend.Send(default(JoinIntoParty));
	}

	public void LeaveParty()
	{
		Connections.Frontend.Send(default(LeaveParty));
	}

	public void KickMember(string entityId)
	{
		Connections.Frontend.Send(new KickPartyMember
		{
			MemberEntityId = entityId
		});
	}

	public void ElectPartyLeader(string entityId)
	{
		Connections.Frontend.Send(new ElectPartyLeader
		{
			MemberEntityId = entityId
		});
	}

	public void InviteIntoParty(string entityId)
	{
		if (IsLeader || NotInParty)
		{
			Connections.Frontend.Send(new InviteIntoParty
			{
				InviteeEntityId = entityId
			});
		}
	}

	public void RejectPartyInvitation()
	{
		Connections.Frontend.Send(new RejectPartyInvitation
		{
			InviteeEntityId = null
		});
	}

	public void CancelPartyInvitation(string entityId)
	{
		Connections.Frontend.Send(new RejectPartyInvitation
		{
			InviteeEntityId = entityId
		});
	}

	[ExposedInEditor(null)]
	private void TestParty(bool invite, bool accept)
	{
		Messages.Party msg = default(Messages.Party);
		if (invite)
		{
			PartyInfo value = default(PartyInfo);
			value.LeaderStatus = default(PartierStatus);
			value.LeaderStatus.EntityId = "130129301923";
			value.LeaderRadioId.Name = "TestLeader11111111";
			value.MemberStatus = new Pair<PartierStatus, bool>[1];
			PartierStatus item = default(PartierStatus);
			item.EntityId = PlayerBehavior.LocalPlayer.EntityId;
			ref Pair<PartierStatus, bool> reference = ref value.MemberStatus[0];
			reference = new Pair<PartierStatus, bool>(item, accept);
			msg.Info = value;
		}
		OnParty(msg, default(PacketHeader));
	}
}
