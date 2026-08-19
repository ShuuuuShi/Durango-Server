using MsgPack;

namespace Messages;

public struct SearchResult
{
	public Point2 Tile;

	public static void Pack(Packer packer, SearchResult val, bool hint = false)
	{
		packer.PackArrayHeader(1);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static SearchResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		SearchResult result = default(SearchResult);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<SearchResult Tile={Tile}>";
	}
}
