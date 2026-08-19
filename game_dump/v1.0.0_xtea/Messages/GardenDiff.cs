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
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		byte b = default(byte);
		unpacker.ReadByte(ref b);
		GardenDiff result = default(GardenDiff);
		result.Chunk.x = b;
		unpacker.ReadByte(ref b);
		result.Chunk.y = b;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result._GardenDiff = ((MessagePackObject)(ref lastReadData)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<GardenDiff Chunk={Chunk} _GardenDiff={_GardenDiff}>";
	}
}
