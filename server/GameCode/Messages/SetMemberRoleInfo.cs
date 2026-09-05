using MsgPack;

namespace Messages;

public struct SetMemberRoleInfo
{
	public const uint TypeCode = 3681u;

	public int RoleId;

	public MemberRole Info;

	public static void Pack(Packer packer, SetMemberRoleInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3681u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.RoleId);
		MemberRole.Pack(packer, val.Info);
	}

	public static SetMemberRoleInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetMemberRoleInfo result = default(SetMemberRoleInfo);
		result.RoleId = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Info = MemberRole.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SetMemberRoleInfo RoleId={RoleId} Info={Info}>";
	}
}
