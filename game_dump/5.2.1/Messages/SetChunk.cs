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
		unpacker.ReadByte(out var result);
		SetChunk result2 = default(SetChunk);
		result2.Chunk.x = result;
		unpacker.ReadByte(out result);
		result2.Chunk.y = result;
		return result2;
	}

	public override string ToString()
	{
		return $"<SetChunk Chunk={Chunk}>";
	}
}
