using MsgPack;
using Shared.System;

namespace Messages;

public struct RequestNearestPOI
{
	public const uint TypeCode = 911u;

	public Shared.System.PointOfInterest Type;

	public Point2 Tile;

	public static void Pack(Packer packer, RequestNearestPOI val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(911u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack((int)val.Type);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static RequestNearestPOI Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		RequestNearestPOI result = default(RequestNearestPOI);
		if (num < 0 || 4 < num)
		{
			result.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result.Type = (Shared.System.PointOfInterest)num;
		}
		unpacker.Read();
		ushort num2 = default(ushort);
		unpacker.ReadUInt16(ref num2);
		result.Tile.x = num2;
		unpacker.ReadUInt16(ref num2);
		result.Tile.y = num2;
		return result;
	}

	public override string ToString()
	{
		return $"<RequestNearestPOI Type={Type} Tile={Tile}>";
	}
}
