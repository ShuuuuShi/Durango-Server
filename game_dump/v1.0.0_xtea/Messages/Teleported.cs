using MsgPack;
using Shared.Teleport;

namespace Messages;

public struct Teleported
{
	public const uint TypeCode = 2037u;

	public Point2 Tile;

	public TeleportType Type;

	public static void Pack(Packer packer, Teleported val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2037u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack((int)val.Type);
	}

	public static Teleported Unpack(Unpacker unpacker)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		Teleported result = default(Teleported);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData)).AsInt32();
		if (num2 < 0 || 5 < num2)
		{
			result.Type = TeleportType.Invalid;
		}
		else
		{
			result.Type = (TeleportType)num2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Teleported Tile={Tile} Type={Type}>";
	}
}
