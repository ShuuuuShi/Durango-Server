using MsgPack;

namespace Messages;

public struct RadioPin
{
	public const uint TypeCode = 2605u;

	public ulong RegionId;

	public string RegionName;

	public Point2 Tile;

	public static void Pack(Packer packer, RadioPin val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2605u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.RegionId);
		if (val.RegionName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.RegionName);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static RadioPin Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RadioPin result = default(RadioPin);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.RegionName = null;
		}
		else
		{
			string regionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.RegionName = regionName;
		}
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<RadioPin RegionId={RegionId} RegionName={RegionName} Tile={Tile}>";
	}
}
