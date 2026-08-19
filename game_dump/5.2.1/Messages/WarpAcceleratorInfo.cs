using MsgPack;

namespace Messages;

public struct WarpAcceleratorInfo
{
	public const uint TypeCode = 21112515u;

	public string EntityId;

	public Point2 Tile;

	public WarpAccelerator Warpaccelerator;

	public static void Pack(Packer packer, WarpAcceleratorInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(21112515u);
		}
		else
		{
			packer.PackArrayHeader(3);
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
		WarpAccelerator.Pack(packer, val.Warpaccelerator);
	}

	public static WarpAcceleratorInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		WarpAcceleratorInfo result = default(WarpAcceleratorInfo);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Warpaccelerator = WarpAccelerator.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<WarpAcceleratorInfo EntityId={EntityId} Tile={Tile} Warpaccelerator={Warpaccelerator}>";
	}
}
