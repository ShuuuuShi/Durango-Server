namespace Durango.Logic.Clan;

public class Member
{
	public readonly string EntityId;

	public int RoleId;

	public bool IsApplier;

	public Member(Pair<string, int> pair)
	{
		EntityId = pair.Item1;
		RoleId = pair.Item2;
		IsApplier = false;
	}

	public Member(string applier)
	{
		EntityId = applier;
		RoleId = -1;
		IsApplier = true;
	}

	public override bool Equals(object obj)
	{
		if (obj is Member member)
		{
			return EntityId == member.EntityId;
		}
		if (obj is string text)
		{
			return EntityId == text;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return EntityId.GetHashCode();
	}
}
