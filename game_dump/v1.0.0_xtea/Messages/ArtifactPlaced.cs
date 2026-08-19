using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct ArtifactPlaced
{
	public const uint TypeCode = 3641u;

	public Point2 Tile;

	public KeyValuePair<int, int> Size;

	public static void Pack(Packer packer, ArtifactPlaced val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3641u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackArrayHeader(2);
		packer.Pack(val.Size.Key);
		packer.Pack(val.Size.Value);
	}

	public static ArtifactPlaced Unpack(Unpacker unpacker)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		ArtifactPlaced result = default(ArtifactPlaced);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int key = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int value = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Size = new KeyValuePair<int, int>(key, value);
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactPlaced Tile={Tile} Size={Size}>";
	}
}
