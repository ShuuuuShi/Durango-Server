using MsgPack;

namespace Messages;

public struct MoveItemsInWarehouse
{
	public const uint TypeCode = 3685u;

	public string EntityId;

	public Point2 Tile;

	public string[] ItemIds;

	public string SourceSectionName;

	public string TargetSectionName;

	public static void Pack(Packer packer, MoveItemsInWarehouse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3685u);
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
		if (val.SourceSectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SourceSectionName);
		}
		if (val.TargetSectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetSectionName);
		}
	}

	public static MoveItemsInWarehouse Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MoveItemsInWarehouse result = default(MoveItemsInWarehouse);
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
		result.SourceSectionName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TargetSectionName = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<MoveItemsInWarehouse EntityId={EntityId} Tile={Tile} ItemIds={ItemIds} SourceSectionName={SourceSectionName} TargetSectionName={TargetSectionName}>";
	}
}
