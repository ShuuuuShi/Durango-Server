using MsgPack;
using Shared.System;

namespace Messages;

public struct NearestPOI
{
	public const uint TypeCode = 912u;

	public Shared.System.PointOfInterest Type;

	public Point2? Tile;

	public static void Pack(Packer packer, NearestPOI val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(912u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		if (!val.Tile.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.Value.x);
		packer.Pack((ushort)val.Tile.Value.y);
	}

	public static NearestPOI Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		NearestPOI result = default(NearestPOI);
		if (num < 0 || 4 < num)
		{
			result.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result.Type = (Shared.System.PointOfInterest)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData2)).IsNil)
		{
			result.Tile = null;
		}
		else
		{
			ushort num2 = default(ushort);
			unpacker.ReadUInt16(ref num2);
			Point2 value = default(Point2);
			value.x = num2;
			unpacker.ReadUInt16(ref num2);
			value.y = num2;
			result.Tile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<NearestPOI Type={Type} Tile={Tile}>";
	}
}
