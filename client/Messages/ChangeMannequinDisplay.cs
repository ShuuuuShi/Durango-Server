using MsgPack;

namespace Messages;

public struct ChangeMannequinDisplay
{
	public const uint TypeCode = 24311u;

	public string EntityId;

	public Point2 Tile;

	public string Slot;

	public string ItemId;

	public static void Pack(Packer packer, ChangeMannequinDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(24311u);
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
		if (val.Slot == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Slot);
		}
		if (val.ItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ItemId);
		}
	}

	public static ChangeMannequinDisplay Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ChangeMannequinDisplay result = default(ChangeMannequinDisplay);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Slot = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ChangeMannequinDisplay EntityId={EntityId} Tile={Tile} Slot={Slot} ItemId={ItemId}>";
	}
}
