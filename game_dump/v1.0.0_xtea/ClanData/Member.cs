using System.Collections.Generic;

namespace ClanData;

public class Member
{
	public readonly ulong EntityId;

	public int RoleId;

	public bool IsApplier;

	public Member(IList<ulong> array)
	{
		EntityId = array[0];
		RoleId = (int)array[1];
		IsApplier = false;
	}

	public Member(ulong applier)
	{
		EntityId = applier;
		RoleId = -1;
		IsApplier = true;
	}

	public override bool Equals(object obj)
	{
		if (obj is Member)
		{
			Member member = (Member)obj;
			return EntityId == member.EntityId;
		}
		if (obj is ulong)
		{
			return EntityId == (ulong)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return EntityId.GetHashCode();
	}
}
