using MsgPack;

namespace Messages;

public struct SetChunk
{
	public const uint TypeCode = 199u;

	public Point2 Chunk;

	public static void Pack(Packer packer, SetChunk val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(199u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.Chunk.x);
		packer.Pack((byte)val.Chunk.y);
	}

	public static SetChunk Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		byte b = default(byte);
		unpacker.ReadByte(ref b);
		SetChunk result = default(SetChunk);
		result.Chunk.x = b;
		unpacker.ReadByte(ref b);
		result.Chunk.y = b;
		return result;
	}

	public override string ToString()
	{
		return $"<SetChunk Chunk={Chunk}>";
	}
}
