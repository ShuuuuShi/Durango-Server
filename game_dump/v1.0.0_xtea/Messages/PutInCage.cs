using MsgPack;

namespace Messages;

public struct PutInCage
{
	public const uint TypeCode = 809u;

	public ulong PetId;

	public Point2 CageTile;

	public ulong CageEntityId;

	public static void Pack(Packer packer, PutInCage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(809u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.PetId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.CageTile.x);
		packer.Pack((ushort)val.CageTile.y);
		packer.Pack(val.CageEntityId);
	}

	public static PutInCage Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PutInCage result = default(PutInCage);
		result.PetId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.CageTile.x = num;
		unpacker.ReadUInt16(ref num);
		result.CageTile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.CageEntityId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<PutInCage PetId={PetId} CageTile={CageTile} CageEntityId={CageEntityId}>";
	}
}
