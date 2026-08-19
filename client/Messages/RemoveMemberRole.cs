using MsgPack;

namespace Messages;

public struct RemoveMemberRole
{
	public const uint TypeCode = 792253u;

	public int RoleId;

	public int MoveToRoleId;

	public static void Pack(Packer packer, RemoveMemberRole val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(792253u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.RoleId);
		packer.Pack(val.MoveToRoleId);
	}

	public static RemoveMemberRole Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemoveMemberRole result = default(RemoveMemberRole);
		result.RoleId = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MoveToRoleId = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RemoveMemberRole RoleId={RoleId} MoveToRoleId={MoveToRoleId}>";
	}
}
