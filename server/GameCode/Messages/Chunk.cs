using MsgPack;

namespace Messages;

public struct Chunk
{
	public const uint TypeCode = 200u;

	public Point2 _Chunk;

	public byte[] Garden;

	public static void Pack(Packer packer, Chunk val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(200u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackArrayHeader(2);
		packer.Pack((byte)val._Chunk.x);
		packer.Pack((byte)val._Chunk.y);
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
		unpacker.Read();
		unpacker.ReadByte(out var result);
		Chunk result2 = default(Chunk);
		result2._Chunk.x = result;
		unpacker.ReadByte(out result);
		result2._Chunk.y = result;
		unpacker.Read();
		result2.Garden = unpacker.LastReadData.AsBinary();
		return result2;
	}

	public override string ToString()
	{
		return $"<Chunk _Chunk={_Chunk} Garden={Garden}>";
	}
}
