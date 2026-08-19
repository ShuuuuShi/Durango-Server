using MsgPack;

namespace Messages;

public struct SendCargo
{
	public const uint TypeCode = 3814u;

	public string EntityId;

	public Point2 Tile;

	public string[] ItemIds;

	public CargoDestination Destination;

	public static void Pack(Packer packer, SendCargo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3814u);
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
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ItemIds.Length);
			for (int i = 0; i < val.ItemIds.Length; i++)
			{
				if (val.ItemIds[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.ItemIds[i]);
				}
			}
		}
		CargoDestination.Pack(packer, val.Destination);
	}

	public static SendCargo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SendCargo result = default(SendCargo);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.Destination = CargoDestination.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SendCargo EntityId={EntityId} Tile={Tile} ItemIds={ItemIds} Destination={Destination}>";
	}
}
