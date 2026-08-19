using MsgPack;

namespace Messages;

public struct DisappearEntity
{
	public const uint TypeCode = 101u;

	public ulong EntityId;

	public ushort EntityType;

	public static void Pack(Packer packer, DisappearEntity val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(101u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.EntityType);
	}

	public static DisappearEntity Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		DisappearEntity result = default(DisappearEntity);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityType = ((MessagePackObject)(ref lastReadData2)).AsUInt16();
		return result;
	}

	public override string ToString()
	{
		return $"<DisappearEntity EntityId={EntityId} EntityType={EntityType}>";
	}
}
