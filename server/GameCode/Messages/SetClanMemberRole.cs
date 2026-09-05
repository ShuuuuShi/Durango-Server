using MsgPack;

namespace Messages;

public struct SetClanMemberRole
{
	public const uint TypeCode = 3662u;

	public string TargetId;

	public int RoleId;

	public static void Pack(Packer packer, SetClanMemberRole val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3662u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.TargetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetId);
		}
		packer.Pack(val.RoleId);
	}

	public static SetClanMemberRole Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetClanMemberRole result = default(SetClanMemberRole);
		result.TargetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RoleId = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SetClanMemberRole TargetId={TargetId} RoleId={RoleId}>";
	}
}
