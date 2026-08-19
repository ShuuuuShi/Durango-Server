using MsgPack;

namespace Messages;

public struct Collect
{
	public const uint TypeCode = 2026u;

	public string EntityId;

	public Point2 Tile;

	public string GeneratorId;

	public int Level;

	public string ToolItemId;

	public static void Pack(Packer packer, Collect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2026u);
		}
		else
		{
			packer.PackArrayHeader(5);
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
		if (val.GeneratorId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.GeneratorId);
		}
		packer.Pack(val.Level);
		if (val.ToolItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ToolItemId);
		}
	}

	public static Collect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Collect result = default(Collect);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.GeneratorId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ToolItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Collect EntityId={EntityId} Tile={Tile} GeneratorId={GeneratorId} Level={Level} ToolItemId={ToolItemId}>";
	}
}
