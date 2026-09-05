using MsgPack;

namespace Messages;

public struct AddItemsToWarehouse
{
	public const uint TypeCode = 3690u;

	public string EntityId;

	public Point2 Tile;

	public string SectionName;

	public string[] ItemIds;

	public static void Pack(Packer packer, AddItemsToWarehouse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3690u);
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
		if (val.SectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SectionName);
		}
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
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

	public static AddItemsToWarehouse Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AddItemsToWarehouse result = default(AddItemsToWarehouse);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.SectionName = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AddItemsToWarehouse EntityId={EntityId} Tile={Tile} SectionName={SectionName} ItemIds={ItemIds}>";
	}
}
