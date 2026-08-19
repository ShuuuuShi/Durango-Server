using MsgPack;

namespace Messages;

public struct SetClanMemberRole
{
	public const uint TypeCode = 3662u;

	public ulong TargetId;

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
		packer.Pack(val.TargetId);
		packer.Pack(val.RoleId);
	}

	public static SetClanMemberRole Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetClanMemberRole result = default(SetClanMemberRole);
		result.TargetId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.RoleId = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<SetClanMemberRole TargetId={TargetId} RoleId={RoleId}>";
	}
}
