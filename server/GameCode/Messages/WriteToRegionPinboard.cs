using MsgPack;

namespace Messages;

public struct WriteToRegionPinboard
{
	public const uint TypeCode = 584793u;

	public string EntityId;

	public Point2 Tile;

	public string PlayerId;

	public string Content;

	public static void Pack(Packer packer, WriteToRegionPinboard val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(584793u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.PlayerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerId);
		}
		if (val.Content == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Content);
		}
	}

	public static WriteToRegionPinboard Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WriteToRegionPinboard result = default(WriteToRegionPinboard);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.PlayerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Content = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<WriteToRegionPinboard EntityId={EntityId} Tile={Tile} PlayerId={PlayerId} Content={Content}>";
	}
}
