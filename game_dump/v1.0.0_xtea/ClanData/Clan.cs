using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Clan;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace ClanData;

public class Clan
{
	public ulong Id;

	public string Name;

	public int Capacity;

	public int Level;

	public long Exp;

	public float Fund;

	public string Notice;

	public string Intro;

	public string Mainland;

	public Dictionary<int, MemberRole> RoleInfos;

	public Member[] Members;

	public Member[] Appliers;

	public double ShieldStartsAt;

	public double DeclareWarTime;

	private static List<Member> _membersBuffer = new List<Member>();

	private bool _isLoadedEmblem;

	private Texture2D _emblem;

	private bool _isWaitEmblem;

	private Action<Texture2D> _onEmblem;

	private static Permissions[] _permissions;

	public int MemberCount => (Members != null) ? Members.Length : 0;

	public static Permissions[] Permissions
	{
		get
		{
			if (_permissions == null)
			{
				Array values = Enum.GetValues(typeof(Permissions));
				List<Permissions> list = new List<Permissions>();
				for (int i = 0; i < values.Length; i++)
				{
					Permissions permissions = (Permissions)(int)values.GetValue(i);
					if (permissions > Shared.Clan.Permissions.None)
					{
						list.Add(permissions);
					}
				}
				_permissions = list.ToArray();
			}
			return _permissions;
		}
	}

	public Clan(Messages.Member msg)
	{
		Id = msg.ClanId;
		Name = msg.ClanName;
	}

	public Clan(ClanJson json)
	{
		Set(json);
	}

	public void Set(ClanJson json)
	{
		Id = json.id;
		Name = json.name;
		Capacity = json.capacity;
		Fund = json.fund;
		Level = json.level;
		Notice = json.notice;
		Intro = json.intro;
		Exp = json.exp;
		Mainland = json.mainland;
		ShieldStartsAt = json.shield_starts_at;
		if (json.role_infos == null)
		{
			RoleInfos = null;
		}
		else
		{
			if (RoleInfos == null)
			{
				RoleInfos = new Dictionary<int, MemberRole>();
			}
			else
			{
				RoleInfos.Clear();
			}
			foreach (KeyValuePair<int, RoleInfo> role_info in json.role_infos)
			{
				RoleInfos[role_info.Key] = new MemberRole
				{
					Id = role_info.Value.id,
					Grade = role_info.Value.grade,
					Permissions = role_info.Value.permissions,
					Name = role_info.Value.name
				};
			}
		}
		List<Member> membersBuffer = _membersBuffer;
		membersBuffer.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(json.members); i < size; i++)
		{
			membersBuffer.Add(new Member(json.members[i]));
		}
		membersBuffer.Sort(MemberComparison);
		Members = membersBuffer.ToArray();
		int size2 = KUtility.GetSize(json.appliers);
		if (size2 > 0)
		{
			List<Member> membersBuffer2 = _membersBuffer;
			membersBuffer2.Clear();
			for (int j = 0; j < size2; j++)
			{
				membersBuffer2.Add(new Member(json.appliers[j]));
			}
			Appliers = membersBuffer2.ToArray();
		}
		else
		{
			Appliers = null;
		}
		_isLoadedEmblem = false;
		_emblem = null;
	}

	public Member GetMember(ulong entityId)
	{
		for (int i = 0; i < MemberCount; i++)
		{
			if (Members[i].EntityId == entityId)
			{
				return Members[i];
			}
		}
		return null;
	}

	public bool TryGetRole(int id, out MemberRole role)
	{
		if (RoleInfos != null && RoleInfos.TryGetValue(id, out role))
		{
			return true;
		}
		role = default(MemberRole);
		return false;
	}

	public void GetEmblem([NotNull] Action<Texture2D> onResult)
	{
		if (_isLoadedEmblem)
		{
			onResult(_emblem);
			return;
		}
		_onEmblem = (Action<Texture2D>)Delegate.Combine(_onEmblem, onResult);
		if (!_isWaitEmblem)
		{
			string url = $"{KSingleton<GameManager>.Instance().GatewayUrl}clans/{Id}/emblem.png";
			_isWaitEmblem = true;
			KUtility.RequestUrl(url, OnEmblem);
		}
	}

	public string GetIntro()
	{
		if (string.IsNullOrEmpty(Intro))
		{
			return T._("부족 소개글이 없습니다");
		}
		return Intro;
	}

	private void OnEmblem(byte[] bytes)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		if (KUtility.GetSize(bytes) == 0)
		{
			_emblem = null;
		}
		else
		{
			_emblem = new Texture2D(0, 0);
			((Texture)_emblem).filterMode = (FilterMode)0;
			((Texture)_emblem).wrapMode = (TextureWrapMode)1;
			_emblem.LoadImage(bytes);
		}
		if (_onEmblem != null)
		{
			_onEmblem(_emblem);
		}
		_isLoadedEmblem = true;
		_isWaitEmblem = false;
		_onEmblem = null;
	}

	private int MemberComparison(Member m1, Member m2)
	{
		MemberRole role;
		bool flag = TryGetRole(m1.RoleId, out role);
		MemberRole role2;
		bool flag2 = TryGetRole(m2.RoleId, out role2);
		int num = ((!flag) ? 100000 : role.Grade);
		int num2 = ((!flag2) ? 100000 : role2.Grade);
		return num - num2;
	}

	public void GetClanWarState(out ClanWarState state, out double remain)
	{
		state = ClanWarState.None;
		remain = 0.0;
		WarInfo war = Singleton<Constants>.Instance.war;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (ShieldStartsAt > 0.0 && predictedServerTime > ShieldStartsAt)
		{
			double num = predictedServerTime - ShieldStartsAt;
			if (num < (double)war.rematch_break)
			{
				state = ClanWarState.RematchBreak;
				remain = (double)war.rematch_break - num;
			}
			return;
		}
		double num2 = predictedServerTime - DeclareWarTime;
		if (num2 < (double)war.warm_up_time)
		{
			state = ClanWarState.WarmUp;
			remain = (double)war.warm_up_time - num2;
		}
		else if (num2 < (double)(war.warm_up_time + war.match_time))
		{
			state = ClanWarState.Match;
			remain = (double)(war.warm_up_time + war.match_time) - num2;
		}
	}
}
