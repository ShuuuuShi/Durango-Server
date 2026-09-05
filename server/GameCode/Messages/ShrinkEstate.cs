using MsgPack;

namespace Messages;

public struct ShrinkEstate
{
	public const uint TypeCode = 2426u;

	public string EstateId;

	public Point2 Cell;

	public static void Pack(Packer packer, ShrinkEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2426u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.Cell.x);
		packer.Pack((byte)val.Cell.y);
	}

	public static ShrinkEstate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ShrinkEstate result = default(ShrinkEstate);
		result.EstateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadByte(out var result2);
		result.Cell.x = result2;
		unpacker.ReadByte(out result2);
		result.Cell.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<ShrinkEstate EstateId={EstateId} Cell={Cell}>";
	}
}
