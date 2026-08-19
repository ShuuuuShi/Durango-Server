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
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		NearestPOI result = default(NearestPOI);
		if (num < 0 || 6 < num)
		{
			result.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result.Type = (Shared.System.PointOfInterest)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Tile = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value = default(Point2);
			value.x = result2;
			unpacker.ReadUInt16(out result2);
			value.y = result2;
			result.Tile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<NearestPOI Type={Type} Tile={Tile}>";
	}
}
