using MsgPack;

namespace Messages;

public struct GardenDiff
{
	public const uint TypeCode = 202u;

	public Point2 Chunk;

	public byte[] _GardenDiff;

	public static void Pack(Packer packer, GardenDiff val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(202u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.Chunk.x);
		packer.Pack((byte)val.Chunk.y);
		if (val._GardenDiff == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val._GardenDiff);
		}
	}

	public static GardenDiff Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadByte(out var result);
		GardenDiff result2 = default(GardenDiff);
		result2.Chunk.x = result;
		unpacker.ReadByte(out result);
		result2.Chunk.y = result;
		unpacker.Read();
		result2._GardenDiff = unpacker.LastReadData.AsBinary();
		return result2;
	}

	public override string ToString()
	{
		return $"<GardenDiff Chunk={Chunk} _GardenDiff={_GardenDiff}>";
	}
}
