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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RequestNearestPOI result = default(RequestNearestPOI);
		if (num < 0 || 6 < num)
		{
			result.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result.Type = (Shared.System.PointOfInterest)num;
		}
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<RequestNearestPOI Type={Type} Tile={Tile}>";
	}
}
