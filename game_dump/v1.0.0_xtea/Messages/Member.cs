using MsgPack;

namespace Messages;

public struct Member
{
	public const uint TypeCode = 105u;

	public ulong EntityId;

	public ulong ClanId;

	public string ClanName;

	public int RoleId;

	public static void Pack(Packer packer, Member val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(105u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.ClanId);
		if (val.ClanName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanName);
		}
		packer.Pack(val.RoleId);
	}

	public static Member Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Member result = default(Member);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ClanId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ClanName = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.RoleId = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Member EntityId={EntityId} ClanId={ClanId} ClanName={ClanName} RoleId={RoleId}>";
	}
}
