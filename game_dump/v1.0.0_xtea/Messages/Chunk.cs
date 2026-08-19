using MsgPack;

namespace Messages;

public struct Chunk
{
	public const uint TypeCode = 200u;

	public Point2 _Chunk;

	public byte[] Biomes;

	public byte[] Ocean;

	public byte[] Rivers;

	public byte[] Landmarks;

	public byte[] Garden;

	public static void Pack(Packer packer, Chunk val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(200u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.PackArrayHeader(2);
		packer.Pack((byte)val._Chunk.x);
		packer.Pack((byte)val._Chunk.y);
		if (val.Biomes == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Biomes);
		}
		if (val.Ocean == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Ocean);
		}
		if (val.Rivers == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Rivers);
		}
		if (val.Landmarks == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Landmarks);
		}
		if (val.Garden == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Garden);
		}
	}

	public static Chunk Unpack(Unpacker unpacker)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		byte b = default(byte);
		unpacker.ReadByte(ref b);
		Chunk result = default(Chunk);
		result._Chunk.x = b;
		unpacker.ReadByte(ref b);
		result._Chunk.y = b;
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Biomes = ((MessagePackObject)(ref lastReadData)).AsBinary();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Ocean = ((MessagePackObject)(ref lastReadData2)).AsBinary();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Rivers = ((MessagePackObject)(ref lastReadData3)).AsBinary();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Landmarks = ((MessagePackObject)(ref lastReadData4)).AsBinary();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Garden = ((MessagePackObject)(ref lastReadData5)).AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<Chunk _Chunk={_Chunk} Biomes={Biomes} Ocean={Ocean} Rivers={Rivers} Landmarks={Landmarks} Garden={Garden}>";
	}
}
