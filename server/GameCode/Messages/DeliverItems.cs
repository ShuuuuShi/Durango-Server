using MsgPack;
using Shared.Faction;

namespace Messages;

public struct DeliverItems
{
	public const uint TypeCode = 3614u;

	public string EntityId;

	public Point2 Tile;

	public FactionType FactionType;

	public string[] ItemIds;

	public static void Pack(Packer packer, DeliverItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3614u);
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
		packer.Pack((int)val.FactionType);
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

	public static DeliverItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DeliverItems result = default(DeliverItems);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 101 < num)
		{
			result.FactionType = FactionType.Invalid;
		}
		else
		{
			result.FactionType = (FactionType)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.ItemIds = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			result.ItemIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DeliverItems EntityId={EntityId} Tile={Tile} FactionType={FactionType} ItemIds={ItemIds}>";
	}
}
