using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Clan;

namespace Durango.Logic.Clan;

public class Clan
{
	public const int MaxClanNoticeLength = 250;

	public const int MaxClanIntroLength = 55;

	public string Id;

	public string Name;

	public int Capacity;

	public int Level;

	public long Exp;

	public float Fund;

	public string Notice;

	public string Intro;

	public string Mainland;

	public double RenameableUntil;

	public double EmblemChangeableUntil;

	public bool IsDetail;

	public List<Member> Members;

	public List<Member> Appliers;

	private readonly Dictionary<int, MemberRole> _roleInfos = new Dictionary<int, MemberRole>();

	private int _memberCount;

	[NotNull]
	public Dictionary<int, MemberRole> RoleInfos => _roleInfos;

	public int MemberCount
	{
		get
		{
			if (Members == null)
			{
				return _memberCount;
			}
			return Members.Count;
		}
	}

	public static Permissions[] Permissions => Enums<Shared.Clan.Permissions>.Greater(Shared.Clan.Permissions.None);

	public void Set(ClanJson json, bool isDetail)
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
		RenameableUntil = json.renameable_until;
		EmblemChangeableUntil = json.emblem_changeable_until;
		IsDetail = isDetail;
		if (json.role_infos == null)
		{
			_roleInfos.Clear();
		}
		else
		{
			_roleInfos.Clear();
			List<RoleInfo> list = new List<RoleInfo>(json.role_infos.Values);
			list.Sort(delegate(RoleInfo r1, RoleInfo r2)
			{
				int num = r1.grade - r2.grade;
				if (num == 0)
				{
					num = r1.id - r2.id;
				}
				return num;
			});
			foreach (RoleInfo item in list)
			{
				_roleInfos[item.id] = new MemberRole
				{
					Id = item.id,
					Grade = item.grade,
					Permissions = item.permissions,
					Name = item.name,
					UserType = item.user_type
				};
			}
		}
		if (json.members == null)
		{
			Members = null;
			_memberCount = json.member_count;
		}
		else
		{
			List<Member> list2;
			if (Members == null)
			{
				list2 = new List<Member>();
			}
			else
			{
				list2 = Members;
				list2.Clear();
			}
			for (int i = 0; i < json.members.Length; i++)
			{
				list2.Add(new Member(json.members[i]));
			}
			Members = list2;
		}
		int size = KUtility.GetSize(json.appliers);
		if (size > 0)
		{
			List<Member> list3;
			if (Appliers == null)
			{
				list3 = new List<Member>();
			}
			else
			{
				list3 = Appliers;
				list3.Clear();
			}
			list3.Clear();
			for (int j = 0; j < size; j++)
			{
				list3.Add(new Member(json.appliers[j]));
			}
			Appliers = list3;
		}
		else
		{
			Appliers = null;
		}
	}

	public Member GetMember(string entityId)
	{
		for (int i = 0; i < KUtility.GetSize(Members); i++)
		{
			if (Members[i].EntityId == entityId)
			{
				return Members[i];
			}
		}
		return null;
	}

	public Member GetApplier(string entityId)
	{
		for (int i = 0; i < KUtility.GetSize(Appliers); i++)
		{
			if (Appliers[i].EntityId == entityId)
			{
				return Appliers[i];
			}
		}
		return null;
	}

	public bool TryGetRole(int id, out MemberRole role)
	{
		if (_roleInfos.TryGetValue(id, out role))
		{
			return true;
		}
		role = default(MemberRole);
		return false;
	}
}
