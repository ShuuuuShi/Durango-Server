using MsgPack;

namespace Messages;

public struct Member
{
	public const uint TypeCode = 105u;

	public string EntityId;

	public string ClanId;

	public string ClanName;

	public int RoleId;

	public string ApplyingClanId;

	public static void Pack(Packer packer, Member val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(105u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.ClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanId);
		}
		if (val.ClanName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanName);
		}
		packer.Pack(val.RoleId);
		if (val.ApplyingClanId == null)
		{
			packer.PackNull();
		}
		else if (val.ApplyingClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ApplyingClanId);
		}
	}

	public static Member Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Member result = default(Member);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ClanId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ClanName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RoleId = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ApplyingClanId = null;
		}
		else
		{
			string applyingClanId = unpacker.LastReadData.AsString();
			result.ApplyingClanId = applyingClanId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Member EntityId={EntityId} ClanId={ClanId} ClanName={ClanName} RoleId={RoleId} ApplyingClanId={ApplyingClanId}>";
	}
}
