using MsgPack;

namespace Messages;

public struct RenameCategory
{
	public const uint TypeCode = 3696u;

	public ulong EntityId;

	public Point2 Tile;

	public string Category;

	public string NewCategory;

	public static void Pack(Packer packer, RenameCategory val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3696u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		if (val.NewCategory == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.NewCategory);
		}
	}

	public static RenameCategory Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RenameCategory result = default(RenameCategory);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Category = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.NewCategory = ((MessagePackObject)(ref lastReadData3)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<RenameCategory EntityId={EntityId} Tile={Tile} Category={Category} NewCategory={NewCategory}>";
	}
}
