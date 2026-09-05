using MsgPack;
using Shared.Faction;

namespace Messages;

public struct ShuffleMission
{
	public const uint TypeCode = 3627u;

	public string EntityId;

	public Point2 Tile;

	public FactionType FactionType;

	public static void Pack(Packer packer, ShuffleMission val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3627u);
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
		packer.Pack((int)val.FactionType);
	}

	public static ShuffleMission Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ShuffleMission result = default(ShuffleMission);
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
		return result;
	}

	public override string ToString()
	{
		return $"<ShuffleMission EntityId={EntityId} Tile={Tile} FactionType={FactionType}>";
	}
}
