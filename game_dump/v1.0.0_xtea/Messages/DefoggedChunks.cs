using MsgPack;

namespace Messages;

public struct DefoggedChunks
{
	public const uint TypeCode = 203u;

	public Point2[] Chunks;

	public byte[] Biomes;

	public static void Pack(Packer packer, DefoggedChunks val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(203u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Chunks == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Chunks.Length);
			for (int i = 0; i < val.Chunks.Length; i++)
			{
				packer.PackArrayHeader(2);
				packer.Pack((byte)val.Chunks[i].x);
				packer.Pack((byte)val.Chunks[i].y);
			}
		}
		if (val.Biomes == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Biomes);
		}
	}

	public static DefoggedChunks Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		DefoggedChunks result = default(DefoggedChunks);
		result.Chunks = new Point2[num];
		byte b = default(byte);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadByte(ref b);
			result.Chunks[i].x = b;
			unpacker.ReadByte(ref b);
			result.Chunks[i].y = b;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Biomes = ((MessagePackObject)(ref lastReadData2)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<DefoggedChunks Chunks={Chunks} Biomes={Biomes}>";
	}
}
