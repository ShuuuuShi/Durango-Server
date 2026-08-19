using MsgPack;

namespace Messages;

public struct DumpItems
{
	public const uint TypeCode = 16u;

	public PropKey? SourceProp;

	public string SourcePetEntityId;

	public string SectionName;

	public string[] ItemIds;

	public Point2? Tile;

	public int? Floor;

	public static void Pack(Packer packer, DumpItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(16u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (!val.SourceProp.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PropKey.Pack(packer, val.SourceProp.Value);
		}
		if (val.SourcePetEntityId == null)
		{
			packer.PackNull();
		}
		else if (val.SourcePetEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SourcePetEntityId);
		}
		if (val.SectionName == null)
		{
			packer.PackNull();
		}
		else if (val.SectionName == null)
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
		if (!val.Tile.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack((ushort)val.Tile.Value.x);
			packer.Pack((ushort)val.Tile.Value.y);
		}
		if (!val.Floor.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Floor.Value);
		}
	}

	public static DumpItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DumpItems result = default(DumpItems);
		if (unpacker.LastReadData.IsNil)
		{
			result.SourceProp = null;
		}
		else
		{
			PropKey value = PropKey.Unpack(unpacker);
			result.SourceProp = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SourcePetEntityId = null;
		}
		else
		{
			string sourcePetEntityId = unpacker.LastReadData.AsString();
			result.SourcePetEntityId = sourcePetEntityId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SectionName = null;
		}
		else
		{
			string sectionName = unpacker.LastReadData.AsString();
			result.SectionName = sectionName;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Tile = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value2 = default(Point2);
			value2.x = result2;
			unpacker.ReadUInt16(out result2);
			value2.y = result2;
			result.Tile = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Floor = null;
		}
		else
		{
			int value3 = unpacker.LastReadData.AsInt32();
			result.Floor = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DumpItems SourceProp={SourceProp} SourcePetEntityId={SourcePetEntityId} SectionName={SectionName} ItemIds={ItemIds} Tile={Tile} Floor={Floor}>";
	}
}
