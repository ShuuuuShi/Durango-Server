using MsgPack;

namespace Messages;

public struct SetSectionOrder
{
	public const uint TypeCode = 3688u;

	public string EntityId;

	public Point2 Tile;

	public string[] SectionOrder;

	public static void Pack(Packer packer, SetSectionOrder val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3688u);
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
		if (val.SectionOrder == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.SectionOrder.Length);
		for (int i = 0; i < val.SectionOrder.Length; i++)
		{
			if (val.SectionOrder[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.SectionOrder[i]);
			}
		}
	}

	public static SetSectionOrder Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SetSectionOrder result = default(SetSectionOrder);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.SectionOrder = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.SectionOrder[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetSectionOrder EntityId={EntityId} Tile={Tile} SectionOrder={SectionOrder}>";
	}
}
