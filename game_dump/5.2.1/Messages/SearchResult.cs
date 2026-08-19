using MsgPack;
using Shared.System;

namespace Messages;

public struct SearchResult
{
	public Point2 Tile;

	public Shared.System.PointOfInterest Type;

	public static void Pack(Packer packer, SearchResult val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack((int)val.Type);
	}

	public static SearchResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		SearchResult result2 = default(SearchResult);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 6 < num)
		{
			result2.Type = Shared.System.PointOfInterest.Invalid;
		}
		else
		{
			result2.Type = (Shared.System.PointOfInterest)num;
		}
		return result2;
	}

	public override string ToString()
	{
		return $"<SearchResult Tile={Tile} Type={Type}>";
	}
}
