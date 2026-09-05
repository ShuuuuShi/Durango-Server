using MsgPack;

namespace Messages;

public struct Occupied
{
	public const uint TypeCode = 301u;

	public string EntityId;

	public int TileX;

	public int TileY;

	public int? Floor;

	public static void Pack(Packer packer, Occupied val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(301u);
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
		packer.Pack(val.TileX);
		packer.Pack(val.TileY);
		if (!val.Floor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Floor.Value);
		}
	}

	public static Occupied Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Occupied result = default(Occupied);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TileX = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.TileY = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Floor = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Floor = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Occupied EntityId={EntityId} TileX={TileX} TileY={TileY} Floor={Floor}>";
	}
}
