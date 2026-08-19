using MsgPack;

namespace Messages;

public struct DefoggedChunks
{
	public const uint TypeCode = 203u;

	public Point2[] Chunks;

	public static void Pack(Packer packer, DefoggedChunks val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(203u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Chunks == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Chunks.Length);
		for (int i = 0; i < val.Chunks.Length; i++)
		{
			packer.PackArrayHeader(2);
			packer.Pack((byte)val.Chunks[i].x);
			packer.Pack((byte)val.Chunks[i].y);
		}
	}

	public static DefoggedChunks Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		DefoggedChunks result = default(DefoggedChunks);
		result.Chunks = new Point2[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadByte(out var result2);
			result.Chunks[i].x = result2;
			unpacker.ReadByte(out result2);
			result.Chunks[i].y = result2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<DefoggedChunks Chunks={Chunks}>";
	}
}
