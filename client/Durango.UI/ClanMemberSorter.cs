using System;
using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.Player;
using Durango.Utils;

namespace Durango.UI;

public class ClanMemberSorter
{
	private readonly List<string> _ids = new List<string>();

	private readonly List<Member> _members = new List<Member>();

	private readonly List<Member> _appliers = new List<Member>();

	private Clan _clan;

	public List<Member> Members => _members;

	public List<Member> Appliers => _appliers;

	public void Request(Clan clan, Action response)
	{
		_clan = clan;
		GetMembers(clan.Members, clan.Appliers);
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(_ids, delegate
		{
			_members.Sort(CompareMember);
			_appliers.Sort(CompareMember);
			response();
		});
	}

	private void GetMembers(List<Member> members, List<Member> appliers)
	{
		_ids.Clear();
		_members.Clear();
		_appliers.Clear();
		if (members != null)
		{
			foreach (Member member in members)
			{
				_ids.Add(member.EntityId);
				_members.Add(member);
			}
		}
		if (appliers == null)
		{
			return;
		}
		foreach (Member applier in appliers)
		{
			_ids.Add(applier.EntityId);
			_appliers.Add(applier);
		}
	}

	private int CompareMember(Member x, Member y)
	{
		int num = 0;
		if (x != y)
		{
			num = CompareGrade(GetGrade(x), GetGrade(y));
			if (num == 0)
			{
				PlayerInfoManager playerInfoManager = Singleton<PlayerInfoManager>.Instance();
				num = ComparePlayerInfo(playerInfoManager.GetCachedPlayerInfoOrEmpty(x.EntityId), playerInfoManager.GetCachedPlayerInfoOrEmpty(y.EntityId));
			}
		}
		return num;
	}

	private int CompareGrade(int? x, int? y)
	{
		int num = 0;
		if (x.HasValue && y.HasValue)
		{
			return x.Value - y.Value;
		}
		return x.HasValue ? (-1) : (y.HasValue ? 1 : 0);
	}

	private int ComparePlayerInfo(PlayerInfo x, PlayerInfo y)
	{
		int num = 0;
		if (x.Valid && y.Valid)
		{
			num = y.Level - x.Level;
			if (num == 0)
			{
				num = string.CompareOrdinal(x.Name, y.Name);
			}
		}
		else
		{
			num = (x.Valid ? (-1) : (y.Valid ? 1 : 0));
		}
		return num;
	}

	private int? GetGrade(Member member)
	{
		if (_clan.TryGetRole(member.RoleId, out var role))
		{
			return role.Grade;
		}
		return null;
	}
}
