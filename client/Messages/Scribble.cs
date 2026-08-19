using MsgPack;
using Shared.MessageBoard;

namespace Messages;

public struct Scribble
{
	public const uint TypeCode = 319u;

	public string EntityId;

	public Point2 Tile;

	public Drawing Type;

	public byte[] Data;

	public static void Pack(Packer packer, Scribble val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(319u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack((int)val.Type);
		if (val.Data == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Data);
		}
	}

	public static Scribble Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Scribble result = default(Scribble);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 1 < num)
		{
			result.Type = Drawing.Invalid;
		}
		else
		{
			result.Type = (Drawing)num;
		}
		unpacker.Read();
		result.Data = unpacker.LastReadData.AsBinary();
		return result;
	}

	public override string ToString()
	{
		return $"<Scribble EntityId={EntityId} Tile={Tile} Type={Type} Data={Data}>";
	}
}
