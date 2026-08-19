using MsgPack;

namespace Messages;

public struct DeclareWar
{
	public const uint TypeCode = 3664u;

	public Point2 Position;

	public ulong EstateId;

	public ulong ClanId;

	public static void Pack(Packer packer, DeclareWar val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3664u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Position.x);
		packer.Pack((ushort)val.Position.y);
		packer.Pack(val.EstateId);
		packer.Pack(val.ClanId);
	}

	public static DeclareWar Unpack(Unpacker unpacker)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		DeclareWar result = default(DeclareWar);
		result.Position.x = num;
		unpacker.ReadUInt16(ref num);
		result.Position.y = num;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.EstateId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ClanId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<DeclareWar Position={Position} EstateId={EstateId} ClanId={ClanId}>";
	}
}
