using MsgPack;

namespace Messages;

public struct SetSectionItemOrder
{
	public const uint TypeCode = 3689u;

	public string EntityId;

	public Point2 Tile;

	public string SectionName;

	public string[] ItemOrder;

	public static void Pack(Packer packer, SetSectionItemOrder val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3689u);
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
		if (val.ItemOrder == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemOrder.Length);
		for (int i = 0; i < val.ItemOrder.Length; i++)
		{
			if (val.ItemOrder[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemOrder[i]);
			}
		}
	}

	public static SetSectionItemOrder Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetSectionItemOrder result = default(SetSectionItemOrder);
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
		result.ItemOrder = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemOrder[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetSectionItemOrder EntityId={EntityId} Tile={Tile} SectionName={SectionName} ItemOrder={ItemOrder}>";
	}
}
